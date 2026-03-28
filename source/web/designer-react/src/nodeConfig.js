export const NODE_COLORS = {
  AI: '#4A90E2',
  Control: '#F5A623',
  Data: '#7ED321',
  IO: '#BD10E0',
};

// Default parameters per node type — used when a node is dropped onto the canvas
export const NODE_DEFAULT_PARAMS = {
  LlmNode: {
    provider: 'openai',
    model: 'gpt-4o',
    apiKey: '',
    apiUrl: '',
    systemPrompt: '',
    temperature: 0.7,
    maxTokens: 1000,
    maintainHistory: false,
  },
  PromptBuilderNode: { promptTemplate: '', systemTemplate: '' },
  EmbeddingNode: {
    provider: 'openai',
    model: 'text-embedding-ada-002',
    apiKey: '',
    textKey: '',
    outputKey: 'embedding',
  },
  OutputParserNode: { outputKey: 'parsed', schema: '' },
  StartNode: { description: '' },
  EndNode: {},
  ErrorNode: {},
  ErrorRouteNode: {},
  ConditionNode: { condition: '' },
  SubWorkflowNode: { subWorkflowId: '' },
  DelayNode: { delayMs: 1000 },
  MergeNode: { targetKey: 'merged', sourceKeys: '' },
  LogNode: { message: '', logLevel: 'Info' },
  LoopNode: { itemsKey: '', outputKey: '', loopItemKey: '__loop_item__', maxIterations: 0 },
  ParallelNode: { branchCount: 3, mergeStrategy: 'overwrite' },
  BranchNode: { valueKey: '', case1Value: '', case2Value: '', case3Value: '', caseSensitive: false },
  TransformNode: { expression: '', outputKey: '' },
  DataMapperNode: { mappings: '{}' },
  FilterNode: { condition: '', outputKey: '' },
  ChunkTextNode: { textKey: '', chunkSize: 500, chunkOverlap: 50, outputKey: 'chunks' },
  MemoryNode: { action: 'read', key: '', scope: 'global' },
  HttpRequestNode: { url: '', method: 'GET', headers: '{}', body: '', outputKey: 'response' },
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
  if (portId === 'input')                           return '#3b82f6'; // blue
  if (portId === 'output')                          return '#6c757d'; // grey
  if (portId === 'success')                         return '#22c55e'; // green
  if (portId === 'failure' || portId === 'error')   return '#ef4444'; // red
  if (portId.startsWith('case') || portId.startsWith('branch')) return '#f59e0b'; // amber
  if (portId === 'afterAll')                        return '#22c55e'; // green
  if (portId === 'default')                         return '#8b5cf6'; // purple
  return handleType === 'target' ? '#3b82f6' : '#6c757d'; // fallback by direction
}
