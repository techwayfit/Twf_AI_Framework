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
  getNodesBounds,
  getViewportForBounds,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';

import { SchemaContext } from './context/SchemaContext';
import { nodeTypes } from './components/nodes';
import DeletableEdge from './components/edges/DeletableEdge';

const edgeTypes = { deletable: DeletableEdge };
import Toolbar from './components/Toolbar';
import NodePalette from './components/NodePalette';
import PropertiesPanel from './components/PropertiesPanel';
import EdgePropertiesPanel from './components/EdgePropertiesPanel';
import VariablesPanel from './components/VariablesPanel';
import RunnerPanel from './components/RunnerPanel';
import { toPng } from 'html-to-image';
import { loadWorkflow, saveWorkflow, loadAvailableNodes, loadAllSchemas } from './api';
import { toReactFlow, getContextData, applyContextData } from './adapter';
import { generateNodeId } from './nodeConfig';
import './App.css';

const genId = () => crypto.randomUUID();
const genSubId = () => crypto.randomUUID();

// ─── Bootstrap Icons font cache for export ────────────────────────────────────
// html-to-image cannot embed cross-origin web fonts. We fetch the woff2 once,
// convert to base64, and inject a self-contained @font-face at export time.
let _biFontFaceCSS = null;
async function getBootstrapIconsFontFace() {
  if (_biFontFaceCSS) return _biFontFaceCSS;
  try {
    const woff2Url = 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/fonts/bootstrap-icons.woff2';
    const res = await fetch(woff2Url);
    const buf = await res.arrayBuffer();
    const b64 = btoa(String.fromCharCode(...new Uint8Array(buf)));
    _biFontFaceCSS = `@font-face {
      font-family: "bootstrap-icons";
      src: url("data:font/woff2;base64,${b64}") format("woff2");
      font-weight: normal; font-style: normal;
    }`;
  } catch {
    _biFontFaceCSS = ''; // font unavailable — icons will still show as squares but won't crash
  }
  return _biFontFaceCSS;
}

// ─── Export helpers ───────────────────────────────────────────────────────────

/** Promise wrapper for loading an image. */
function loadImage(src) {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = () => reject(new Error('Image failed to load'));
    img.src = src;
  });
}

/**
 * Walk live/clone SVG trees simultaneously and copy computed presentation
 * attributes (stroke, fill, opacity, …) onto the clone so they survive
 * serialisation without a live stylesheet.
 */
function inlineSvgPresentationStyles(liveRoot, cloneRoot) {
  const props = [
    'stroke', 'stroke-width', 'stroke-dasharray', 'stroke-linecap',
    'stroke-linejoin', 'stroke-opacity', 'fill', 'fill-opacity',
    'opacity', 'visibility', 'display', 'marker-end', 'marker-start',
  ];
  const liveEls  = [liveRoot,  ...liveRoot.querySelectorAll('*')];
  const cloneEls = [cloneRoot, ...cloneRoot.querySelectorAll('*')];

  liveEls.forEach((live, i) => {
    const clone = cloneEls[i];
    if (!clone || live.nodeType !== Node.ELEMENT_NODE) return;
    try {
      const cs = window.getComputedStyle(live);
      props.forEach(prop => {
        const val = cs.getPropertyValue(prop);
        if (val !== undefined && val !== '') clone.setAttribute(prop, val);
      });
    } catch (_) { /* cross-origin / detached — skip */ }
  });
}

/**
 * Serialize the ReactFlow edges SVG into a data URL with the export viewport
 * transform baked in.  This sidesteps the Chromium bug where <svg> elements
 * inside <foreignObject> (what html-to-image uses) are silently dropped.
 *
 * @param {SVGElement} edgesEl   – .react-flow__edges
 * @param {SVGElement|null} markerEl – .react-flow__marker (optional arrowhead defs)
 * @param {number} w   Export canvas width  (px)
 * @param {number} h   Export canvas height (px)
 * @param {number} tx  Viewport translate X (px)
 * @param {number} ty  Viewport translate Y (px)
 * @param {number} zoom Viewport scale
 */
function buildEdgesDataUrl(edgesEl, markerEl, w, h, tx, ty, zoom) {
  // Build a fresh SVG element so we control width/height exactly
  const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
  svg.setAttribute('xmlns', 'http://www.w3.org/2000/svg');
  svg.setAttribute('width',  String(w));
  svg.setAttribute('height', String(h));

  // Inline arrowhead marker defs so url(#…) references resolve after serialisation
  const defsSource = edgesEl.querySelector('defs') ?? markerEl?.querySelector('defs');
  if (defsSource) svg.appendChild(defsSource.cloneNode(true));

  // Wrap all non-defs children in a transform group matching the export viewport
  const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
  g.setAttribute('transform', `translate(${tx} ${ty}) scale(${zoom})`);

  Array.from(edgesEl.children).forEach(child => {
    const tag = child.tagName.toLowerCase();
    if (tag === 'defs') return;

    if (tag === 'svg') {
      // Individual per-edge SVG wrappers — unwrap their contents into the <g>
      // (nested <svg> inside <g> is invalid in SVG 1.1 and clips in many browsers).
      // Edge paths use flow coordinates, so they transform correctly via the parent <g>.
      Array.from(child.children).forEach(inner => {
        if (inner.tagName.toLowerCase() === 'defs') return;
        const cloned = inner.cloneNode(true);
        inlineSvgPresentationStyles(inner, cloned);
        g.appendChild(cloned);
      });
    } else {
      const cloned = child.cloneNode(true);
      inlineSvgPresentationStyles(child, cloned);
      g.appendChild(cloned);
    }
  });

  svg.appendChild(g);

  const str = new XMLSerializer().serializeToString(svg);
  return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(str);
}

// ─── Inner component (needs ReactFlowProvider ancestor) ───────────────────────
function DesignerInner({ workflowId, mode }) {
  const isRunner = mode === 'runner';
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

  const { screenToFlowPosition, zoomIn, zoomOut, fitView, } = useReactFlow();

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

  // ── Runner node state ─────────────────────────────────────────────────────
  const handleNodeStateChange = useCallback((nodeId, state) => {
    setNodes((ns) =>
      ns.map((n) => n.id === nodeId ? { ...n, data: { ...n.data, runnerState: state } } : n),
    );
  }, [setNodes]);

  const handleResetNodeStates = useCallback(() => {
    setNodes((ns) =>
      ns.map((n) => ({ ...n, data: { ...n.data, runnerState: 'pending' } })),
    );
  }, [setNodes]);

  // ── Connections ────────────────────────────────────────────────────────────
  const onConnect = useCallback(
    (connection) => {
      const sourceNode = nodesRef.current.find((n) => n.id === connection.source);
      const isNoteLink = sourceNode?.type === 'NoteNode';
      setEdges((eds) =>
        addEdge(
          isNoteLink
            ? {
                ...connection,
                id: crypto.randomUUID(),
                type: 'deletable',
                style: { stroke: '#aaa', strokeWidth: 1.5, strokeDasharray: '5,4' },
                markerEnd: undefined,
              }
            : {
                ...connection,
                id: crypto.randomUUID(),
                type: 'deletable',
                markerEnd: { type: MarkerType.ArrowClosed },
              },
          eds,
        ),
      );
    },
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
      const isContainer = nodeInfo.type === 'ContainerNode';
      // Default parameters come from the backend schema (via API), not a hardcoded map.
      const defaultParams = { ...(nodeInfo.defaultParams ?? {}) };
      const nodeId = generateNodeId(nodeInfo.idPrefix, nodesRef.current);
      const newNode = {
        id: genId(),
        type: nodeInfo.type,
        position,
        ...(isContainer ? { style: { width: defaultParams.width ?? 300, height: defaultParams.height ?? 200 } } : {}),
        data: {
          label: nodeInfo.name,
          type: nodeInfo.type,
          category: nodeInfo.category,
          color: nodeInfo.color,
          icon: nodeInfo.icon,
          nodeId,
          parameters: defaultParams,
          executionOptions: null,
        },
      };
      // ContainerNodes go at the front so they render behind other nodes
      setNodes((ns) => isContainer ? [newNode, ...ns] : [...ns, newNode]);
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

  // ── Export ────────────────────────────────────────────────────────────────
  const handleExport = useCallback(async (format) => {
    if (nodes.length === 0) return;

    const name = (workflowDef?.name ?? 'workflow').replace(/[^a-z0-9_-]/gi, '_');

    const exportW = 1800;
    const exportH = 1100;

    const bounds = getNodesBounds(nodes);
    const { x, y, zoom } = getViewportForBounds(bounds, exportW, exportH, 0.3, 2, 0.08);

    const viewportEl = wrapperRef.current?.querySelector('.react-flow__viewport');
    if (!viewportEl) return;

    // ── Suppress shadows during capture so edges aren't hidden underneath ─
    const biFontFace = await getBootstrapIconsFontFace();

    const noShadowStyle = document.createElement('style');
    noShadowStyle.setAttribute('data-export-override', '1');
    noShadowStyle.textContent = `
      ${biFontFace}
      .react-flow__node * { box-shadow: none !important; filter: none !important; }
      .react-flow__node { box-shadow: none !important; filter: none !important; }
      .react-flow__handle { opacity: 0 !important; }
    `;
    document.head.appendChild(noShadowStyle);

    // ── Step 1: capture HTML nodes layer (transparent background) ─────────
    // html-to-image serialises HTML nodes correctly but silently drops SVG
    // children inside <foreignObject> (Chromium bug) — so edges are NOT
    // captured here.  We handle edges separately below.
    const nodesPng = await toPng(viewportEl, {
      backgroundColor: 'transparent',
      pixelRatio: 1,
      width: exportW,
      height: exportH,
      fontEmbedCSS: biFontFace,   // embed the bi font directly into html-to-image's clone
      style: {
        transform: `translate(${x}px, ${y}px) scale(${zoom})`,
        transformOrigin: '0 0',
        width: `${exportW}px`,
        height: `${exportH}px`,
      },
      // Skip SVG children — they would be blank/clipped anyway
      filter: el => el.tagName?.toLowerCase() !== 'svg' || el.closest('.react-flow__nodes') != null,
    });

    // ── Step 2: serialize edge SVG directly (bypass html-to-image for SVG) ─
    const edgesEl  = viewportEl.querySelector('.react-flow__edges');
    const markerEl = viewportEl.querySelector('.react-flow__marker');
    const edgesDataUrl = edgesEl
      ? buildEdgesDataUrl(edgesEl, markerEl, exportW, exportH, x, y, zoom)
      : null;

    // ── Step 3: composite background + edges + nodes on a Canvas ──────────
    try {
      const canvas = document.createElement('canvas');
      canvas.width  = exportW;
      canvas.height = exportH;
      const ctx = canvas.getContext('2d');

      // Background fill
      ctx.fillStyle = '#f8f9fa';
      ctx.fillRect(0, 0, exportW, exportH);

      // Draw nodes first (edges render on top so connectors are never hidden
      // behind opaque node backgrounds — e.g. the Start circle)
      const nodesImg = await loadImage(nodesPng);
      ctx.drawImage(nodesImg, 0, 0);

      // Draw edges on top
      if (edgesDataUrl) {
        const edgesImg = await loadImage(edgesDataUrl);
        ctx.drawImage(edgesImg, 0, 0);
      }

      if (format === 'png') {
        const pngDataUrl = canvas.toDataURL('image/png');
        const a = document.createElement('a');
        a.download = `${name}.png`;
        a.href = pngDataUrl;
        a.click();
      } else {
        // Embed the composite PNG inside an SVG container
        const pngDataUrl = canvas.toDataURL('image/png');
        const svgContent = [
          '<?xml version="1.0" encoding="UTF-8"?>',
          `<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"`,
          `     width="${exportW}" height="${exportH}" viewBox="0 0 ${exportW} ${exportH}">`,
          `  <image href="${pngDataUrl}" x="0" y="0" width="${exportW}" height="${exportH}"/>`,
          '</svg>',
        ].join('\n');
        const blob = new Blob([svgContent], { type: 'image/svg+xml' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.download = `${name}.svg`;
        a.href = url;
        a.click();
        setTimeout(() => URL.revokeObjectURL(url), 2000);
      }
    } catch (err) {
      alert(`Export failed: ${err.message}`);
    } finally {
      document.head.querySelector('style[data-export-override]')?.remove();
    }
  }, [workflowDef, nodes]);

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
    <SchemaContext.Provider value={{ schemas, isRunner }}>
      <div className="designer-shell">
        {/* Top toolbar */}
        <Toolbar
          workflowName={workflowDef?.name ?? ''}
          workflowId={workflowId}
          mode={mode}
          onSave={handleSave}
          onZoomIn={() => zoomIn()}
          onZoomOut={() => zoomOut()}
          onFitView={() => fitView({ padding: 0.2 })}
          saving={saving}
          activeSubWorkflow={activeSubWorkflow}
          onBackToMain={() => switchContext({ type: 'main' })}
          onExport={handleExport}
        />

        <div className="designer-body">
          {/* Left sidebar — palette in designer mode, runner panel in runner mode */}
          {isRunner ? (
            <div className="runner-sidebar">
              <RunnerPanel
                workflowId={workflowId}
                onNodeStateChange={handleNodeStateChange}
                onResetNodeStates={handleResetNodeStates}
              />
            </div>
          ) : (
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
          )}

          {/* Canvas */}
          <div ref={wrapperRef} className="designer-canvas">
            <ReactFlow
              nodes={nodes}
              edges={edges}
              nodeTypes={nodeTypes}
              edgeTypes={edgeTypes}
              onNodesChange={onNodesChange}
              onEdgesChange={isRunner ? undefined : onEdgesChange}
              onConnect={isRunner ? undefined : onConnect}
              onDrop={isRunner ? undefined : onDrop}
              onDragOver={isRunner ? undefined : onDragOver}
              nodesDraggable={!isRunner}
              nodesConnectable={!isRunner}
              deleteKeyCode={isRunner ? null : 'Delete'}
              fitView
              snapToGrid={!isRunner}
              snapGrid={[4, 4]}
              nodeDragThreshold={1}
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

          {/* Right properties panel — designer mode only */}
          {!isRunner && (
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
          )}
        </div>
      </div>
    </SchemaContext.Provider>
  );
}

// ─── Root export (wraps in ReactFlowProvider) ─────────────────────────────────
export default function App({ workflowId, mode }) {
  return (
    <ReactFlowProvider>
      <DesignerInner workflowId={workflowId} mode={mode} />
    </ReactFlowProvider>
  );
}
