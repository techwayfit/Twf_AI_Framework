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
