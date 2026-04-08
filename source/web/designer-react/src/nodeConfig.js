/**
 * Short prefix used to generate human-readable NodeIds when a node is dropped onto the canvas.
 * Must match the IdPrefix defined on each INode implementation in the core library.
 * Format: prefix + zero-padded counter → "llm001", "http002", etc.
 */
export const NODE_ID_PREFIXES = {
  // Control
  StartNode:         'start',
  EndNode:           'end',
  ErrorNode:         'error',
  ErrorRouteNode:    'errroute',
  ConditionNode:     'cond',
  BranchNode:        'branch',
  SubWorkflowNode:   'sub',
  LoopNode:          'loop',
  DelayNode:         'delay',
  MergeNode:         'merge',
  LogNode:           'log',
  // Data
  SetVariableNode:   'setvar',
  TransformNode:     'transform',
  DataMapperNode:    'mapper',
  FilterNode:        'filter',
  ChunkTextNode:     'chunk',
  MemoryNode:        'memory',
  // IO
  HttpRequestNode:   'http',
  FileReadNode:      'fread',
  FileWriteNode:     'fwrite',
  // AI
  LlmNode:           'llm',
  PromptBuilderNode: 'prompt',
  EmbeddingNode:     'embed',
  OutputParserNode:  'parser',
  // Visual
  ContainerNode:     'container',
  NoteNode:          'note',
};

/**
 * Generates the next NodeId for a given node type by scanning existing nodes.
 * Gaps from deleted nodes are fine — stability matters more than consecutive numbering.
 *
 * @param {string} nodeType  e.g. "LlmNode"
 * @param {Array}  existingNodes  ReactFlow nodes array
 * @returns {string}  e.g. "llm003"
 */
export function generateNodeId(nodeType, existingNodes) {
  const prefix = NODE_ID_PREFIXES[nodeType] ?? 'node';
  const max = existingNodes.reduce((acc, n) => {
    const nid = n.data?.nodeId ?? '';
    if (!nid.startsWith(prefix)) return acc;
    const num = parseInt(nid.slice(prefix.length), 10);
    return isNaN(num) ? acc : Math.max(acc, num);
  }, 0);
  return `${prefix}${String(max + 1).padStart(3, '0')}`;
}

export const NODE_COLORS = {
  AI: '#4A90E2',
  Control: '#F5A623',
  Data: '#7ED321',
  IO: '#BD10E0',
};

/**
 * Bootstrap Icons class name for each node type.
 * Used as a fallback for nodes saved before the icon field was added.
 */
export const NODE_ICONS = {
  // Control
  StartNode:         'bi-play-circle-fill',
  EndNode:           'bi-stop-circle-fill',
  ErrorNode:         'bi-exclamation-triangle-fill',
  ErrorRouteNode:    'bi-arrow-right-circle-fill',
  ConditionNode:     'bi-question-diamond',
  BranchNode:        'bi-signpost-split',
  SubWorkflowNode:   'bi-box-arrow-in-right',
  LoopNode:          'bi-arrow-repeat',
  DelayNode:         'bi-clock',
  MergeNode:         'bi-intersect',
  LogNode:           'bi-journal-text',
  // Data
  SetVariableNode:   'bi-pencil',
  TransformNode:     'bi-arrow-left-right',
  DataMapperNode:    'bi-map',
  FilterNode:        'bi-funnel',
  ChunkTextNode:     'bi-file-break',
  MemoryNode:        'bi-memory',
  // IO
  HttpRequestNode:   'bi-globe',
  FileReadNode:      'bi-file-earmark-text',
  FileWriteNode:     'bi-file-earmark-arrow-down',
  // AI
  LlmNode:           'bi-chat-left-dots',
  PromptBuilderNode: 'bi-pencil-square',
  EmbeddingNode:     'bi-diagram-2',
  OutputParserNode:  'bi-list-check',
  // Visual
  ContainerNode:     'bi-bounding-box',
  NoteNode:          'bi-sticky',
};

// Default parameters per node type — used when a node is dropped onto the canvas.
// Every entry includes `description: ''` so it appears in the properties panel.
export const NODE_DEFAULT_PARAMS = {
  // ── Control ───────────────────────────────────────────────────────────────
  StartNode:         { description: '' },
  EndNode:           { description: '' },
  ErrorNode:         { description: '' },
  ErrorRouteNode:    { description: '' },
  ConditionNode:     { description: '', condition: '' },
  BranchNode:        { description: '', valueKey: '', case1Value: '', case2Value: '', case3Value: '', caseSensitive: false },
  SubWorkflowNode:   { description: '', subWorkflowId: '' },
  LoopNode:          { description: '', itemsKey: 'items', outputKey: 'results', loopItemKey: '__item__', maxIterations: 0 },
  DelayNode:         { description: '', delayMs: 1000 },
  MergeNode:         { description: '', targetKey: 'merged', sourceKeys: '' },
  LogNode:           { description: '', message: '', logLevel: 'Info' },
  // ── Data ──────────────────────────────────────────────────────────────────
  SetVariableNode:   { description: '', assignments: '{}' },
  TransformNode:     { description: '', expression: '', outputKey: '' },
  DataMapperNode:    { description: '', mappings: '{}' },
  FilterNode:        { description: '', condition: '', outputKey: '' },
  ChunkTextNode:     { description: '', textKey: '', chunkSize: 500, chunkOverlap: 50, outputKey: 'chunks' },
  MemoryNode:        { description: '', action: 'read', key: '', scope: 'global' },
  // ── IO ────────────────────────────────────────────────────────────────────
  HttpRequestNode:   { description: '', url: '', method: 'GET', headers: '{}', body: '', outputKey: 'response' },
  FileReadNode:      { description: '', filePath: '', format: 'Text', encoding: 'utf-8', outputKey: 'file_content' },
  FileWriteNode:     { description: '', filePath: '', contentKey: '', writeMode: 'Overwrite', createDirectories: true, encoding: 'utf-8' },
  // ── AI ────────────────────────────────────────────────────────────────────
  LlmNode:           { description: '', provider: 'openai', model: 'gpt-4o', apiKey: '', apiUrl: '', systemPrompt: '', temperature: 0.7, maxTokens: 1000, maintainHistory: false },
  PromptBuilderNode: { description: '', promptTemplate: '', systemTemplate: '' },
  EmbeddingNode:     { description: '', provider: 'openai', model: 'text-embedding-ada-002', apiKey: '', textKey: '', outputKey: 'embedding' },
  OutputParserNode:  { description: '', outputKey: 'parsed', schema: '' },
  // ── Visual ────────────────────────────────────────────────────────────────
  ContainerNode:     { backgroundColor: '#6366f1', opacity: 0.12, width: 300, height: 200 },
  NoteNode:          { text: '', color: 'yellow' },
};

/**
 * Routing handles per node type — used by GenericNode to render React Flow connection points.
 * Only nodes that differ from the default single input→output need an entry here.
 * Format: { inputs: [{id, label}], outputs: [{id, label}] }
 * Omitting a node uses the default: input on left, output on right.
 */
export const NODE_ROUTING_PORTS = {
  StartNode:       { inputs: [],                         outputs: [{ id: 'output',   label: 'Start' }] },
  EndNode:         { inputs: [{ id: 'input', label: 'End' }], outputs: [] },
  ErrorNode:       { inputs: [],                         outputs: [{ id: 'output',   label: 'On Error' }] },
  ErrorRouteNode:  { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'success', label: 'Success' }, { id: 'error', label: 'Error' }] },
  ConditionNode:   { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'success', label: 'True' }, { id: 'failure', label: 'False' }] },
  BranchNode:      { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'case1', label: 'Case 1' }, { id: 'case2', label: 'Case 2' },
                               { id: 'case3', label: 'Case 3' }, { id: 'default', label: 'Default' }] },
  SubWorkflowNode: { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'success', label: 'Success' }, { id: 'error', label: 'Error' }] },
  LoopNode:        { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'output', label: 'After Loop' }] },
  HttpRequestNode: { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'output', label: 'Output' }, { id: 'error', label: 'Error' }] },
  FileReadNode:    { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'output', label: 'Output' }, { id: 'error', label: 'Error' }] },
  FileWriteNode:   { inputs: [{ id: 'input', label: 'Input' }],
                     outputs: [{ id: 'output', label: 'Output' }, { id: 'error', label: 'Error' }] },
  ContainerNode:   { inputs: [], outputs: [] },
  NoteNode:        { inputs: [], outputs: [] },
};

// Default routing ports for any node not in NODE_ROUTING_PORTS
export const DEFAULT_ROUTING_PORTS = {
  inputs:  [{ id: 'input',  label: 'Input'  }],
  outputs: [{ id: 'output', label: 'Output' }],
};

// Nodes rendered as circles (no rectangular box)
export const CIRCULAR_NODE_TYPES = new Set(['StartNode', 'EndNode', 'ErrorNode', 'ErrorRouteNode']);

// Nodes rendered as a diamond
export const DIAMOND_NODE_TYPES = new Set(['ConditionNode']);

/**
 * Returns the fill colour for a port handle based on its ID and direction.
 *   input      → blue   (target/inbound)
 *   output     → grey   (source/outbound, generic)
 *   success    → green
 *   failure/error → red
 *   case*      → amber  (BranchNode cases)
 *   default    → muted purple
 */
export function portColor(portId, handleType) {
  if (portId === 'input')                         return '#3b82f6'; // blue
  if (portId === 'output')                        return '#6c757d'; // grey
  if (portId === 'success')                       return '#22c55e'; // green
  if (portId === 'failure' || portId === 'error') return '#ef4444'; // red
  if (portId === 'default')                       return '#8b5cf6'; // purple
  if (portId.startsWith('case') || portId.startsWith('branch')) return '#f59e0b'; // amber
  return handleType === 'target' ? '#3b82f6' : '#6c757d'; // fallback by direction
}
