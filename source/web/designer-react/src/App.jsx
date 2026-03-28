import { useState, useEffect, useCallback, useRef } from 'react';
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  addEdge,
  useReactFlow,
  ReactFlowProvider,
  MarkerType,
  Panel,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';

import { SchemaContext } from './context/SchemaContext';
import { nodeTypes } from './components/nodes';
import Toolbar from './components/Toolbar';
import NodePalette from './components/NodePalette';
import PropertiesPanel from './components/PropertiesPanel';
import EdgePropertiesPanel from './components/EdgePropertiesPanel';
import VariablesPanel from './components/VariablesPanel';
import { loadWorkflow, saveWorkflow, loadAvailableNodes, loadAllSchemas } from './api';
import { toReactFlow, getContextData, applyContextData } from './adapter';
import { NODE_DEFAULT_PARAMS } from './nodeConfig';
import './App.css';

let _idSeq = 1;
const genId = () => `n_${Date.now()}_${_idSeq++}`;
const genSubId = () => `sw_${Date.now()}_${Math.random().toString(36).slice(2, 8)}`;

// ─── Inner component (needs ReactFlowProvider ancestor) ───────────────────────
function DesignerInner({ workflowId }) {
  const [workflowDef, setWorkflowDef] = useState(null);
  const [nodes, setNodes, onNodesChangeRaw] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [schemas, setSchemas] = useState({});
  const [availableNodes, setAvailableNodes] = useState([]);
  const [sidebarTab, setSidebarTab] = useState('nodes');
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(null);
  // context: { type: 'main' } | { type: 'sub', id: '<guid>' }
  const [context, setContext] = useState({ type: 'main' });

  const { screenToFlowPosition, zoomIn, zoomOut, fitView } = useReactFlow();

  // Returns the node list, adding a StartNode at (80,200) if none exists
  function ensureStartNode(rfNodes) {
    if (rfNodes.some((n) => n.type === 'StartNode')) return rfNodes;
    return [
      {
        id: genId(),
        type: 'StartNode',
        position: { x: 80, y: 200 },
        data: { label: 'Start', type: 'StartNode', category: 'Control', color: '#F5A623', parameters: { description: '' }, executionOptions: null },
      },
      ...rfNodes,
    ];
  }
  const wrapperRef = useRef(null);
  // Keep a ref to the latest nodes/edges for use inside context-switch (avoids stale closures)
  const nodesRef = useRef(nodes);
  const edgesRef = useRef(edges);

  // Intercept ReactFlow's built-in node changes to prevent StartNode being deleted
  // (e.g. via the Delete key or programmatic removal)
  const onNodesChange = useCallback(
    (changes) => {
      const safe = changes.filter((c) => {
        if (c.type === 'remove') {
          const node = nodesRef.current.find((n) => n.id === c.id);
          return node?.type !== 'StartNode';
        }
        return true;
      });
      onNodesChangeRaw(safe);
    },
    [onNodesChangeRaw],
  );
  useEffect(() => { nodesRef.current = nodes; }, [nodes]);
  useEffect(() => { edgesRef.current = edges; }, [edges]);

  // Derive selected node/edge from ReactFlow's built-in selection state
  const selectedNode = nodes.find((n) => n.selected) ?? null;
  const selectedEdge = edges.find((e) => e.selected) ?? null;

  // ── Load on mount ──────────────────────────────────────────────────────────
  useEffect(() => {
    async function init() {
      try {
        const [wf, schemaDefs, nodeList] = await Promise.all([
          loadWorkflow(workflowId),
          loadAllSchemas(),
          loadAvailableNodes(),
        ]);
        setWorkflowDef(wf);
        setSchemas(schemaDefs);
        setAvailableNodes(nodeList);
        const { nodes: rfNodes, edges: rfEdges } = toReactFlow(wf);
        // Guarantee the main workflow always has exactly one StartNode
        const rfResult = ensureStartNode(rfNodes);
        setNodes(rfResult);
        setEdges(rfEdges);
      } catch (err) {
        setLoadError(err.message);
      } finally {
        setLoading(false);
      }
    }
    init();
  }, [workflowId]);

  // ── Context switching ──────────────────────────────────────────────────────
  /**
   * Switch the canvas to a different context (main workflow or a sub-workflow).
   * Saves current nodes/edges back into workflowDef before switching.
   */
  const switchContext = useCallback((newContext) => {
    setWorkflowDef((prev) => {
      if (!prev) return prev;
      // Persist current context's state into workflowDef
      const updated = applyContextData(prev, context, nodesRef.current, edgesRef.current);
      // Load new context
      const { nodes: ctxNodes, connections: ctxConns } = getContextData(updated, newContext);
      const { nodes: rfNodes, edges: rfEdges } = toReactFlow({
        nodes: ctxNodes,
        connections: ctxConns,
      });
      setNodes(rfNodes);
      setEdges(rfEdges);
      setContext(newContext);
      return updated;
    });
  }, [context]);

  // ── Connections ────────────────────────────────────────────────────────────
  const onConnect = useCallback(
    (connection) =>
      setEdges((eds) =>
        addEdge(
          {
            ...connection,
            type: 'smoothstep',
            markerEnd: { type: MarkerType.ArrowClosed },
          },
          eds,
        ),
      ),
    [setEdges],
  );

  // ── Drag-and-drop from palette ─────────────────────────────────────────────
  const onDragOver = useCallback((e) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'copy';
  }, []);

  const onDrop = useCallback(
    (e) => {
      e.preventDefault();
      const raw = e.dataTransfer.getData('application/nodeType');
      if (!raw) return;
      const nodeInfo = JSON.parse(raw);
      // Enforce single-instance nodes
      if (nodeInfo.type === 'StartNode' && nodesRef.current.some((n) => n.type === 'StartNode')) {
        alert('A workflow can only have one Start node.');
        return;
      }
      if (nodeInfo.type === 'ErrorNode' && nodesRef.current.some((n) => n.type === 'ErrorNode')) {
        alert('A workflow can only have one Error Handler node.');
        return;
      }
      const position = screenToFlowPosition({ x: e.clientX, y: e.clientY });
      setNodes((ns) => [
        ...ns,
        {
          id: genId(),
          type: nodeInfo.type,
          position,
          data: {
            label: nodeInfo.name,
            type: nodeInfo.type,
            category: nodeInfo.category,
            color: nodeInfo.color,
            parameters: { ...(NODE_DEFAULT_PARAMS[nodeInfo.type] ?? {}) },
            executionOptions: null,
          },
        },
      ]);
    },
    [screenToFlowPosition, setNodes],
  );

  // ── Edge callbacks ─────────────────────────────────────────────────────────
  const handleEdgeLabelChange = useCallback((edgeId, label) => {
    const hasLabel = label.trim().length > 0;
    setEdges((es) =>
      es.map((e) =>
        e.id === edgeId
          ? {
              ...e,
              label: hasLabel ? label : undefined,
              labelStyle: hasLabel ? { fontSize: 11, fill: '#495057', fontWeight: 600 } : undefined,
              labelBgStyle: hasLabel ? { fill: '#fff', fillOpacity: 0.9, stroke: '#dee2e6', strokeWidth: 1 } : undefined,
              labelBgPadding: hasLabel ? [4, 2] : undefined,
              labelBgBorderRadius: hasLabel ? 3 : undefined,
            }
          : e,
      ),
    );
  }, [setEdges]);

  const handleDeleteEdge = useCallback((edgeId) => {
    setEdges((es) => es.filter((e) => e.id !== edgeId));
  }, [setEdges]);

  // ── Properties panel callbacks ─────────────────────────────────────────────
  const handleNodeDataChange = useCallback(
    (nodeId, changes) => {
      setNodes((ns) =>
        ns.map((n) =>
          n.id === nodeId ? { ...n, data: { ...n.data, ...changes } } : n,
        ),
      );
    },
    [setNodes],
  );

  const handleDeleteNode = useCallback(
    (nodeId) => {
      const node = nodesRef.current.find((n) => n.id === nodeId);
      if (node?.type === 'StartNode') return; // StartNode is permanent
      setNodes((ns) => ns.filter((n) => n.id !== nodeId));
      setEdges((es) => es.filter((e) => e.source !== nodeId && e.target !== nodeId));
    },
    [setNodes, setEdges],
  );

  // ── Variables ──────────────────────────────────────────────────────────────
  const handleVariablesChange = useCallback((vars) => {
    setWorkflowDef((prev) => ({ ...prev, variables: vars }));
  }, []);

  // ── Sub-workflow CRUD ──────────────────────────────────────────────────────
  const handleSubWorkflowOpen = useCallback((id) => {
    switchContext({ type: 'sub', id });
  }, [switchContext]);

  const handleSubWorkflowCreate = useCallback((name) => {
    const startId = genId();
    const endId = genId();
    const newSub = {
      id: genSubId(),
      name,
      description: null,
      nodes: [
        { id: startId, name: 'Start', type: 'StartNode', category: 'Control', color: '#F5A623', position: { x: 80, y: 200 }, parameters: { description: '' }, executionOptions: null },
        { id: endId,   name: 'End',   type: 'EndNode',   category: 'Control', color: '#F5A623', position: { x: 500, y: 200 }, parameters: {}, executionOptions: null },
      ],
      connections: [],
      variables: {},
      errorNodeId: null,
    };
    setWorkflowDef((prev) => ({
      ...prev,
      subWorkflows: [...(prev.subWorkflows ?? []), newSub],
    }));
  }, []);

  const handleSubWorkflowRename = useCallback((id, newName) => {
    setWorkflowDef((prev) => ({
      ...prev,
      subWorkflows: (prev.subWorkflows ?? []).map((s) =>
        s.id === id ? { ...s, name: newName } : s,
      ),
    }));
  }, []);

  const handleSubWorkflowDelete = useCallback((id) => {
    // If currently editing this sub-workflow, switch back to main first
    if (context.type === 'sub' && context.id === id) {
      switchContext({ type: 'main' });
    }
    setWorkflowDef((prev) => ({
      ...prev,
      subWorkflows: (prev.subWorkflows ?? []).filter((s) => s.id !== id),
    }));
  }, [context, switchContext]);

  // ── Save ───────────────────────────────────────────────────────────────────
  const handleSave = useCallback(async () => {
    if (!workflowDef) return;
    setSaving(true);
    try {
      // Persist current context's canvas state into the def before saving
      const latestDef = applyContextData(workflowDef, context, nodes, edges);
      await saveWorkflow(latestDef);
      // Keep in-memory def up to date
      setWorkflowDef(latestDef);
    } catch (err) {
      alert(`Save failed: ${err.message}`);
    } finally {
      setSaving(false);
    }
  }, [workflowDef, context, nodes, edges]);

  // Ctrl+S shortcut
  useEffect(() => {
    const handler = (e) => {
      if (e.ctrlKey && e.key === 's') {
        e.preventDefault();
        handleSave();
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [handleSave]);

  // ── Loading / error states ─────────────────────────────────────────────────
  if (loading) {
    return (
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100vh',
          gap: 12,
          color: '#6c757d',
        }}
      >
        <div className="spinner-border spinner-border-sm text-primary" />
        Loading designer…
      </div>
    );
  }

  if (loadError) {
    return (
      <div style={{ padding: 40 }}>
        <div className="alert alert-danger">
          <strong>Error loading designer:</strong> {loadError}
        </div>
      </div>
    );
  }

  // Resolve active sub-workflow info for Toolbar
  const subWorkflows = workflowDef?.subWorkflows ?? [];
  // Types that can only appear once — disable them in the palette when already present
  const disabledTypes = new Set();
  if (nodes.some((n) => n.type === 'StartNode')) disabledTypes.add('StartNode');
  if (nodes.some((n) => n.type === 'ErrorNode')) disabledTypes.add('ErrorNode');
  const activeSubWorkflow =
    context.type === 'sub'
      ? (subWorkflows.find((s) => s.id === context.id) ?? { id: context.id, name: 'Sub-Workflow' })
      : null;

  // ── Render ─────────────────────────────────────────────────────────────────
  return (
    <SchemaContext.Provider value={schemas}>
      <div className="designer-shell">
        {/* Top toolbar */}
        <Toolbar
          workflowName={workflowDef?.name ?? ''}
          workflowId={workflowId}
          onSave={handleSave}
          onZoomIn={() => zoomIn()}
          onZoomOut={() => zoomOut()}
          onFitView={() => fitView({ padding: 0.2 })}
          saving={saving}
          activeSubWorkflow={activeSubWorkflow}
          onBackToMain={() => switchContext({ type: 'main' })}
        />

        <div className="designer-body">
          {/* Left sidebar */}
          <div className="designer-sidebar">
            <div className="sidebar-tabs">
              {['nodes', 'variables'].map((tab) => (
                <button
                  key={tab}
                  className={`sidebar-tab-btn${sidebarTab === tab ? ' active' : ''}`}
                  onClick={() => setSidebarTab(tab)}
                >
                  {tab === 'nodes' ? (
                    <><i className="bi bi-box-seam-fill" /> Nodes</>
                  ) : (
                    <><i className="bi bi-braces" /> Variables</>
                  )}
                </button>
              ))}
            </div>

            <div className="sidebar-content">
              {sidebarTab === 'nodes' ? (
                <NodePalette availableNodes={availableNodes} disabledTypes={disabledTypes} />
              ) : (
                <VariablesPanel
                  variables={workflowDef?.variables ?? {}}
                  onChange={handleVariablesChange}
                  subWorkflows={subWorkflows}
                  activeSubId={context.type === 'sub' ? context.id : null}
                  onSubWorkflowOpen={handleSubWorkflowOpen}
                  onSubWorkflowCreate={handleSubWorkflowCreate}
                  onSubWorkflowRename={handleSubWorkflowRename}
                  onSubWorkflowDelete={handleSubWorkflowDelete}
                />
              )}
            </div>
          </div>

          {/* Canvas */}
          <div ref={wrapperRef} className="designer-canvas">
            <ReactFlow
              nodes={nodes}
              edges={edges}
              nodeTypes={nodeTypes}
              onNodesChange={onNodesChange}
              onEdgesChange={onEdgesChange}
              onConnect={onConnect}
              onDrop={onDrop}
              onDragOver={onDragOver}
              fitView
              snapToGrid
              snapGrid={[16, 16]}
              deleteKeyCode="Delete"
              multiSelectionKeyCode="Control"
            >
              <Background gap={16} color="#e9ecef" />
              <Controls />
              <MiniMap
                nodeColor={(n) => n.data?.color ?? '#3498db'}
                pannable
                zoomable
              />
              {saving && (
                <Panel position="bottom-center">
                  <div className="badge bg-secondary">Saving…</div>
                </Panel>
              )}
            </ReactFlow>
          </div>

          {/* Right properties panel */}
          <div className="designer-properties">
            {selectedEdge && !selectedNode ? (
              <EdgePropertiesPanel
                selectedEdge={selectedEdge}
                sourceNodeName={nodes.find((n) => n.id === selectedEdge.source)?.data.label}
                targetNodeName={nodes.find((n) => n.id === selectedEdge.target)?.data.label}
                onChange={handleEdgeLabelChange}
                onDelete={handleDeleteEdge}
              />
            ) : (
              <PropertiesPanel
                selectedNode={selectedNode}
                onChange={handleNodeDataChange}
                onDelete={handleDeleteNode}
              />
            )}
          </div>
        </div>
      </div>
    </SchemaContext.Provider>
  );
}

// ─── Root export (wraps in ReactFlowProvider) ─────────────────────────────────
export default function App({ workflowId }) {
  return (
    <ReactFlowProvider>
      <DesignerInner workflowId={workflowId} />
    </ReactFlowProvider>
  );
}
