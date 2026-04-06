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
  StartNode: 'bi-play-circle-fill',
  EndNode: 'bi-stop-circle-fill',
  ErrorNode: 'bi-exclamation-triangle-fill',
  ErrorRouteNode: 'bi-arrow-right-circle-fill',
  ConditionNode: 'bi-question-diamond',
  SwitchNode: 'bi-diagram-3',
  SubWorkflowNode: 'bi-box-arrow-in-right',
  DelayNode: 'bi-clock',
  MergeNode: 'bi-intersect',
  LogNode: 'bi-journal-text',
  LoopNode: 'bi-arrow-repeat',
  ParallelNode: 'bi-lightning-charge',
  BranchNode: 'bi-signpost-split',
  WaitNode: 'bi-hourglass-split',
  RetryNode: 'bi-arrow-counterclockwise',
  TimeoutNode: 'bi-alarm',
  EventTriggerNode: 'bi-broadcast',
  // Data
  TransformNode: 'bi-arrow-left-right',
  DataMapperNode: 'bi-map',
  FilterNode: 'bi-funnel',
  ChunkTextNode: 'bi-file-break',
  MemoryNode: 'bi-memory',
  SetVariableNode: 'bi-pencil',
  ParseJsonNode: 'bi-code-slash',
  AggregateNode: 'bi-calculator',
  SortNode: 'bi-sort-down',
  JoinNode: 'bi-link-45deg',
  SchemaValidateNode: 'bi-check2-square',
  TemplateNode: 'bi-file-earmark-code',
  CsvParseNode: 'bi-filetype-csv',
  XmlParseNode: 'bi-filetype-xml',
  Base64Node: 'bi-file-binary',
  HashNode: 'bi-hash',
  DateTimeNode: 'bi-calendar-event',
  RandomNode: 'bi-shuffle',
  // IO
  HttpRequestNode: 'bi-globe',
  HttpResponseNode: 'bi-reply-fill',
  DbQueryNode: 'bi-database',
  FileReadNode: 'bi-file-earmark-text',
  FileWriteNode: 'bi-file-earmark-arrow-down',
  EmailSendNode: 'bi-envelope',
  WebhookNode: 'bi-plug',
  QueueNode: 'bi-collection',
  CacheNode: 'bi-lightning',
  NotificationNode: 'bi-bell',
  StorageNode: 'bi-cloud-arrow-up',
  // Logic
  FunctionNode: 'bi-braces',
  ProcessNode: 'bi-terminal',
  StepNode: 'bi-play-btn',
  ScriptNode: 'bi-file-code',
  RateLimiterNode: 'bi-speedometer2',
  // AI
  LlmNode: 'bi-chat-left-dots',
  PromptBuilderNode: 'bi-pencil-square',
  EmbeddingNode: 'bi-diagram-2',
  OutputParserNode: 'bi-list-check',
  VectorSearchNode: 'bi-search',
  AgentNode: 'bi-robot',
  TextSplitterNode: 'bi-scissors',
  SummariseNode: 'bi-file-text',
  // Visual
  ContainerNode: 'bi-bounding-box',
  NoteNode: 'bi-sticky',
  AnchorNode: 'bi-geo-alt',
};

// Default parameters per node type — used when a node is dropped onto the canvas
// Every entry includes `description: ''` so it appears in the properties panel.
export const NODE_DEFAULT_PARAMS = {
  LlmNode: { description: '', provider: 'openai', model: 'gpt-4o', apiKey: '', apiUrl: '', systemPrompt: '', temperature: 0.7, maxTokens: 1000, maintainHistory: false },
  PromptBuilderNode: { description: '', promptTemplate: '', systemTemplate: '' },
  EmbeddingNode: { description: '', provider: 'openai', model: 'text-embedding-ada-002', apiKey: '', textKey: '', outputKey: 'embedding' },
  OutputParserNode: { description: '', outputKey: 'parsed', schema: '' },
  StartNode: { description: '' },
  EndNode: { description: '' },
  ErrorNode: { description: '' },
  ErrorRouteNode: { description: '' },
  ConditionNode: { description: '', condition: '' },
  SubWorkflowNode: { description: '', subWorkflowId: '' },
  DelayNode: { description: '', delayMs: 1000 },
  MergeNode: { description: '', targetKey: 'merged', sourceKeys: '' },
  LogNode: { description: '', message: '', logLevel: 'Info' },
  LoopNode: { description: '', itemsKey: '', outputKey: '', loopItemKey: '__loop_item__', maxIterations: 0 },
  ParallelNode: { description: '', branchCount: 3, mergeStrategy: 'overwrite' },
  BranchNode: { description: '', valueKey: '', case1Value: '', case2Value: '', case3Value: '', caseSensitive: false },
  TransformNode: { description: '', expression: '', outputKey: '' },
  DataMapperNode: { description: '', mappings: '{}' },
  FilterNode: { description: '', condition: '', outputKey: '' },
  ChunkTextNode: { description: '', textKey: '', chunkSize: 500, chunkOverlap: 50, outputKey: 'chunks' },
  MemoryNode: { description: '', action: 'read', key: '', scope: 'global' },
  HttpRequestNode: { description: '', url: '', method: 'GET', headers: '{}', body: '', outputKey: 'response' },
  HttpResponseNode: { description: '', statusCode: 200, showStatusCode: false, contentType: 'application/json', bodyKey: '', headers: '{}' },
  FileReadNode: { description: '', filePath: '', format: 'Text', encoding: 'utf-8', outputKey: 'file_content' },
  FileWriteNode: { description: '', filePath: '', contentKey: '', writeMode: 'Overwrite', createDirectories: true, encoding: 'utf-8' },
  EmailSendNode: { description: '', provider: 'SMTP', smtpHost: '', smtpPort: 587, apiKey: '', from: '', to: '', cc: '', subject: '', body: '', isHtml: false, useSSL: true },
  WebhookNode: { description: '', path: '', method: 'POST', authType: 'None', secretKey: '', outputKey: 'webhook_payload' },
  QueueNode: { description: '', provider: 'RabbitMQ', connectionString: '', queueName: '', operation: 'Publish', messageKey: '', outputKey: 'queue_message' },
  CacheNode: { description: '', provider: 'InMemory', connectionString: '', operation: 'Get', cacheKey: '', valueKey: '', outputKey: 'cached_value', ttlSeconds: 300 },
  NotificationNode: { description: '', provider: 'Slack', webhookUrl: '', message: '', channel: '', title: '' },
  StorageNode: { description: '', provider: 'S3', connectionString: '', operation: 'Read', bucket: '', objectKey: '', contentKey: '', outputKey: 'storage_result' },
  FunctionNode: { description: '', functionName: '', parameters: '{}', outputKey: 'function_result', async: false },
  DbQueryNode: { description: '', connectionString: '', queryType: 'SELECT', query: '', parameters: '{}', outputKey: 'db_result', singleRow: false },
  ProcessNode: { description: '', processType: 'Command', command: '', arguments: '', workingDirectory: '', environmentVariables: '{}', outputKey: 'process_output', captureStderr: false },
  StepNode: { description: '', stepType: 'Action', actionKey: '', inputKeys: '', outputKey: 'step_result', metadata: '{}' },
  ScriptNode: { description: '', language: 'JavaScript', script: '', outputKey: '' },
  RateLimiterNode: { description: '', strategy: 'FixedWindow', maxRequests: 10, windowMs: 60000, key: '', blockAction: 'Route' },
  WaitNode: { description: '', waitType: 'Duration', durationMs: 5000, eventName: '', webhookPath: '', timeoutMs: 0, resumeDataKey: '' },
  RetryNode: { description: '', maxRetries: 3, retryDelayMs: 1000, backoffMultiplier: 2.0, retryOn: 'Error', retryCondition: '' },
  TimeoutNode: { description: '', timeoutMs: 30000, outputKey: 'timeout_message' },
  EventTriggerNode: { description: '', mode: 'Emit', eventName: '', payloadKey: '' },
  SwitchNode: { description: '', case1Expression: '', case1Label: 'Case 1', case2Expression: '', case2Label: 'Case 2', case3Expression: '', case3Label: 'Case 3', case4Expression: '', case4Label: 'Case 4', stopOnFirstMatch: true },
  SetVariableNode: { description: '', assignments: '{}', mergeMode: 'Merge' },
  ParseJsonNode: { description: '', sourceKey: '', outputKey: '', strict: true },
  AggregateNode: { description: '', itemsKey: '', field: '', operation: 'Count', outputKey: 'aggregate_result' },
  SortNode: { description: '', itemsKey: '', sortBy: '', direction: 'Asc', outputKey: '' },
  JoinNode: { description: '', leftKey: '', rightKey: '', joinField: '', joinType: 'Inner', outputKey: 'joined_result' },
  SchemaValidateNode: { description: '', schema: '{}', dataKey: '', errorsKey: 'validation_errors' },
  TemplateNode: { description: '', engine: 'Handlebars', template: '', outputKey: 'rendered_template' },
  CsvParseNode: { description: '', sourceKey: '', delimiter: ',', hasHeaders: true, outputKey: 'csv_rows' },
  XmlParseNode: { description: '', sourceKey: '', outputKey: 'xml_data', preserveAttributes: true },
  Base64Node: { description: '', operation: 'Encode', sourceKey: '', outputKey: 'base64_result' },
  HashNode: { description: '', algorithm: 'SHA256', sourceKey: '', secretKey: '', outputKey: 'hash_result', encoding: 'Hex' },
  DateTimeNode: { description: '', operation: 'Now', sourceKey: '', inputFormat: '', outputFormat: 'yyyy-MM-ddTHH:mm:ssZ', amount: 1, unit: 'Days', outputKey: 'datetime_result' },
  RandomNode: { description: '', type: 'UUID', min: 0, max: 100, listKey: '', outputKey: 'random_result' },
  VectorSearchNode: { description: '', provider: 'Qdrant', connectionString: '', indexName: '', queryKey: 'embedding', topK: 5, minScore: 0.7, filter: '{}', outputKey: 'search_results' },
  AgentNode: { description: '', provider: 'openai', model: 'gpt-4o', apiKey: '', systemPrompt: '', goalKey: '', tools: '[]', maxIterations: 10, outputKey: 'agent_result' },
  TextSplitterNode: { description: '', strategy: 'Recursive', textKey: '', chunkSize: 1000, chunkOverlap: 200, codeLanguage: '', outputKey: 'text_chunks' },
  SummariseNode: { description: '', provider: 'openai', model: 'gpt-4o', apiKey: '', textKey: '', strategy: 'Stuff', systemPrompt: '', maxLength: 300, outputKey: 'summary' },
  NoteNode: { text: '', color: 'yellow' },
  AnchorNode: { description: '', anchorName: '' },
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
