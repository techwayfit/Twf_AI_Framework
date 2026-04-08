import { MarkerType } from '@xyflow/react';

/**
 * Convert a backend WorkflowDefinition → ReactFlow nodes + edges.
 * Backend NodeDefinition fields:  id, name, type, category, color, position, parameters, executionOptions
 * ReactFlow node fields:          id, type, position, data:{label, type, category, color, parameters, executionOptions}
 *
 * Backend ConnectionDefinition:   id, sourceNodeId, sourcePort, targetNodeId, targetPort
 * ReactFlow edge:                 id, source, sourceHandle, target, targetHandle
 */
export function toReactFlow(workflowDef) {
  // ContainerNodes must come first so they render behind all other nodes
  const sorted = [...(workflowDef.nodes ?? [])].sort((a, b) =>
    a.type === 'ContainerNode' ? -1 : b.type === 'ContainerNode' ? 1 : 0,
  );

  const nodes = sorted.map((n) => {
    const isContainer = n.type === 'ContainerNode';
    const w = n.parameters?.width ?? 300;
    const h = n.parameters?.height ?? 200;
    return {
      id: n.id,
      type: n.type,
      position: { x: n.position?.x ?? 0, y: n.position?.y ?? 0 },
      ...(isContainer ? { style: { width: w, height: h } } : {}),
      data: {
        label: n.name,
        type: n.type,
        category: n.category ?? '',
        color: n.color ?? null,
        nodeId: n.nodeId ?? '',
        parameters: { ...(n.parameters ?? {}) },
        executionOptions: n.executionOptions ?? null,
      },
    };
  });

  const edges = (workflowDef.connections ?? []).map((c) => {
    const hasLabel = !!c.label;
    return {
      id: c.id,
      source: c.sourceNodeId,
      sourceHandle: c.sourcePort ?? 'output',
      target: c.targetNodeId,
      targetHandle: c.targetPort ?? 'input',
      type: 'smoothstep',
      markerEnd: { type: MarkerType.ArrowClosed },
      ...(hasLabel ? {
        label: c.label,
        labelStyle: { fontSize: 11, fill: '#495057', fontWeight: 600 },
        labelBgStyle: { fill: '#fff', fillOpacity: 0.9, stroke: '#dee2e6', strokeWidth: 1 },
        labelBgPadding: [4, 2],
        labelBgBorderRadius: 3,
      } : {}),
    };
  });

  return { nodes, edges };
}

// ─── Context-aware helpers ───────────────────────────────────────────────────

/**
 * Return the node/connection arrays for the currently active context.
 * context: { type: 'main' } | { type: 'sub', id: '<guid>' }
 */
export function getContextData(workflowDef, context) {
  if (!context || context.type === 'main') {
    return { nodes: workflowDef.nodes ?? [], connections: workflowDef.connections ?? [] };
  }
  const sub = (workflowDef.subWorkflows ?? []).find((s) => s.id === context.id);
  return { nodes: sub?.nodes ?? [], connections: sub?.connections ?? [] };
}

/**
 * Write ReactFlow nodes + edges back into the right place in workflowDef.
 * Returns a new workflowDef object (does not mutate).
 */
export function applyContextData(workflowDef, context, nodes, edges) {
  const backendNodes = nodes.map((n) => ({
    id: n.id,
    nodeId: n.data.nodeId ?? '',
    name: n.data.label,
    type: n.type,
    category: n.data.category ?? '',
    color: n.data.color ?? null,
    position: { x: Math.round(n.position.x), y: Math.round(n.position.y) },
    parameters: n.data.parameters ?? {},
    executionOptions: n.data.executionOptions ?? null,
  }));
  const backendConns = edges.map((e) => ({
    id: e.id,
    sourceNodeId: e.source,
    sourcePort: e.sourceHandle ?? 'output',
    targetNodeId: e.target,
    targetPort: e.targetHandle ?? 'input',
    ...(e.label ? { label: e.label } : {}),
  }));

  if (!context || context.type === 'main') {
    return { ...workflowDef, nodes: backendNodes, connections: backendConns };
  }
  return {
    ...workflowDef,
    subWorkflows: (workflowDef.subWorkflows ?? []).map((s) =>
      s.id === context.id
        ? { ...s, nodes: backendNodes, connections: backendConns }
        : s,
    ),
  };
}

/**
 * Convert ReactFlow nodes + edges → backend WorkflowDefinition format.
 * Merges into the existing workflowDef so metadata (id, name, variables, etc.) is preserved.
 */
export function fromReactFlow(workflowDef, nodes, edges) {
  return {
    ...workflowDef,
    nodes: nodes.map((n) => ({
      id: n.id,
      nodeId: n.data.nodeId ?? '',
      name: n.data.label,
      type: n.type,
      category: n.data.category ?? '',
      color: n.data.color ?? null,
      position: { x: Math.round(n.position.x), y: Math.round(n.position.y) },
      parameters: n.data.parameters ?? {},
      executionOptions: n.data.executionOptions ?? null,
    })),
    connections: edges.map((e) => ({
      id: e.id,
      sourceNodeId: e.source,
      sourcePort: e.sourceHandle ?? 'output',
      targetNodeId: e.target,
      targetPort: e.targetHandle ?? 'input',
    })),
  };
}
