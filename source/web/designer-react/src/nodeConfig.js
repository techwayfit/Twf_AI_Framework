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
  HttpResponseNode: { statusCode: 200, showStatusCode: false, contentType: 'application/json', bodyKey: '', headers: '{}' },
  FileReadNode: { filePath: '', format: 'Text', encoding: 'utf-8', outputKey: 'file_content' },
  FileWriteNode: { filePath: '', contentKey: '', writeMode: 'Overwrite', createDirectories: true, encoding: 'utf-8' },
  EmailSendNode: { provider: 'SMTP', smtpHost: '', smtpPort: 587, apiKey: '', from: '', to: '', cc: '', subject: '', body: '', isHtml: false, useSSL: true },
  WebhookNode: { path: '', method: 'POST', authType: 'None', secretKey: '', outputKey: 'webhook_payload' },
  QueueNode: { provider: 'RabbitMQ', connectionString: '', queueName: '', operation: 'Publish', messageKey: '', outputKey: 'queue_message' },
  CacheNode: { provider: 'InMemory', connectionString: '', operation: 'Get', cacheKey: '', valueKey: '', outputKey: 'cached_value', ttlSeconds: 300 },
  NotificationNode: { provider: 'Slack', webhookUrl: '', message: '', channel: '', title: '' },
  StorageNode: { provider: 'S3', connectionString: '', operation: 'Read', bucket: '', objectKey: '', contentKey: '', outputKey: 'storage_result' },
  FunctionNode: { functionName: '', parameters: '{}', outputKey: 'function_result', async: false },
  DbQueryNode: { connectionString: '', queryType: 'SELECT', query: '', parameters: '{}', outputKey: 'db_result', singleRow: false },
  ProcessNode: { processType: 'Command', command: '', arguments: '', workingDirectory: '', environmentVariables: '{}', outputKey: 'process_output', captureStderr: false },
  StepNode: { stepType: 'Action', description: '', actionKey: '', inputKeys: '', outputKey: 'step_result', metadata: '{}' },
  ScriptNode: { language: 'JavaScript', script: '', outputKey: '' },
  RateLimiterNode: { strategy: 'FixedWindow', maxRequests: 10, windowMs: 60000, key: '', blockAction: 'Route' },
  WaitNode: { waitType: 'Duration', durationMs: 5000, eventName: '', webhookPath: '', timeoutMs: 0, resumeDataKey: '' },
  RetryNode: { maxRetries: 3, retryDelayMs: 1000, backoffMultiplier: 2.0, retryOn: 'Error', retryCondition: '' },
  TimeoutNode: { timeoutMs: 30000, outputKey: 'timeout_message' },
  EventTriggerNode: { mode: 'Emit', eventName: '', payloadKey: '' },
  SwitchNode: { case1Expression: '', case1Label: 'Case 1', case2Expression: '', case2Label: 'Case 2', case3Expression: '', case3Label: 'Case 3', case4Expression: '', case4Label: 'Case 4', stopOnFirstMatch: true },
  SetVariableNode: { assignments: '{}', mergeMode: 'Merge' },
  ParseJsonNode: { sourceKey: '', outputKey: '', strict: true },
  AggregateNode: { itemsKey: '', field: '', operation: 'Count', outputKey: 'aggregate_result' },
  SortNode: { itemsKey: '', sortBy: '', direction: 'Asc', outputKey: '' },
  JoinNode: { leftKey: '', rightKey: '', joinField: '', joinType: 'Inner', outputKey: 'joined_result' },
  SchemaValidateNode: { schema: '{}', dataKey: '', errorsKey: 'validation_errors' },
  TemplateNode: { engine: 'Handlebars', template: '', outputKey: 'rendered_template' },
  CsvParseNode: { sourceKey: '', delimiter: ',', hasHeaders: true, outputKey: 'csv_rows' },
  XmlParseNode: { sourceKey: '', outputKey: 'xml_data', preserveAttributes: true },
  Base64Node: { operation: 'Encode', sourceKey: '', outputKey: 'base64_result' },
  HashNode: { algorithm: 'SHA256', sourceKey: '', secretKey: '', outputKey: 'hash_result', encoding: 'Hex' },
  DateTimeNode: { operation: 'Now', sourceKey: '', inputFormat: '', outputFormat: 'yyyy-MM-ddTHH:mm:ssZ', amount: 1, unit: 'Days', outputKey: 'datetime_result' },
  RandomNode: { type: 'UUID', min: 0, max: 100, listKey: '', outputKey: 'random_result' },
  VectorSearchNode: { provider: 'Qdrant', connectionString: '', indexName: '', queryKey: 'embedding', topK: 5, minScore: 0.7, filter: '{}', outputKey: 'search_results' },
  AgentNode: { provider: 'openai', model: 'gpt-4o', apiKey: '', systemPrompt: '', goalKey: '', tools: '[]', maxIterations: 10, outputKey: 'agent_result' },
  TextSplitterNode: { strategy: 'Recursive', textKey: '', chunkSize: 1000, chunkOverlap: 200, codeLanguage: '', outputKey: 'text_chunks' },
  SummariseNode: { provider: 'openai', model: 'gpt-4o', apiKey: '', textKey: '', strategy: 'Stuff', systemPrompt: '', maxLength: 300, outputKey: 'summary' },
  NoteNode: { text: '', color: 'yellow' },
  AnchorNode: { anchorName: '' },
  ContainerNode: { backgroundColor: '#6366f1', opacity: 0.12, width: 300, height: 200 },
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
  if (portId === 'timeout')                         return '#f97316'; // orange
  if (portId === 'blocked' || portId === 'miss')    return '#f97316'; // orange
  if (portId === 'hit')                             return '#22c55e'; // green
  if (portId === 'attempt' || portId === 'execute') return '#6c757d'; // grey
  if (portId.startsWith('case') || portId.startsWith('branch')) return '#f59e0b'; // amber
  if (portId === 'afterAll' || portId === 'completed' || portId === 'resumed') return '#22c55e'; // green
  if (portId === 'default')                         return '#8b5cf6'; // purple
  return handleType === 'target' ? '#3b82f6' : '#6c757d'; // fallback by direction
}
