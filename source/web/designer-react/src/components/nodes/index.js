import GenericNode from './GenericNode';
import CircularNode from './CircularNode';
import DiamondNode from './DiamondNode';
import ParallelNode from './ParallelNode';
import ContainerNode from './ContainerNode';

/**
 * Registry mapping backend node type strings → React Flow node components.
 * Pass this object to the ReactFlow `nodeTypes` prop.
 */
export const nodeTypes = {
  // AI
  LlmNode: GenericNode,
  PromptBuilderNode: GenericNode,
  EmbeddingNode: GenericNode,
  OutputParserNode: GenericNode,

  // Control – circular
  StartNode: CircularNode,
  EndNode: CircularNode,
  ErrorNode: CircularNode,
  ErrorRouteNode: CircularNode,

  // Control – diamond
  ConditionNode: DiamondNode,

  // Control – rectangular
  SubWorkflowNode: GenericNode,
  DelayNode: GenericNode,
  MergeNode: GenericNode,
  LogNode: GenericNode,
  LoopNode: GenericNode,
  ParallelNode: ParallelNode,
  BranchNode: GenericNode,

  // Data
  TransformNode: GenericNode,
  DataMapperNode: GenericNode,
  FilterNode: GenericNode,
  ChunkTextNode: GenericNode,
  MemoryNode: GenericNode,

  // IO
  HttpRequestNode: GenericNode,
  HttpResponseNode: GenericNode,
  DbQueryNode: GenericNode,
  FileReadNode: GenericNode,
  FileWriteNode: GenericNode,
  EmailSendNode: GenericNode,
  WebhookNode: GenericNode,
  QueueNode: GenericNode,
  CacheNode: GenericNode,
  NotificationNode: GenericNode,
  StorageNode: GenericNode,

  // Logic
  FunctionNode: GenericNode,
  ProcessNode: GenericNode,
  StepNode: GenericNode,
  ScriptNode: GenericNode,
  RateLimiterNode: GenericNode,

  // Control (new)
  WaitNode: GenericNode,
  RetryNode: GenericNode,
  TimeoutNode: GenericNode,
  EventTriggerNode: GenericNode,
  SwitchNode: GenericNode,

  // Data (new)
  SetVariableNode: GenericNode,
  ParseJsonNode: GenericNode,
  AggregateNode: GenericNode,
  SortNode: GenericNode,
  JoinNode: GenericNode,
  SchemaValidateNode: GenericNode,
  TemplateNode: GenericNode,
  CsvParseNode: GenericNode,
  XmlParseNode: GenericNode,
  Base64Node: GenericNode,
  HashNode: GenericNode,
  DateTimeNode: GenericNode,
  RandomNode: GenericNode,

  // AI (new)
  VectorSearchNode: GenericNode,
  AgentNode: GenericNode,
  TextSplitterNode: GenericNode,
  SummariseNode: GenericNode,

  // Visual (new)
  NoteNode: GenericNode,
  AnchorNode: GenericNode,
  ContainerNode: ContainerNode,
};
