import GenericNode from './GenericNode';
import CircularNode from './CircularNode';
import DiamondNode from './DiamondNode';

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
  ParallelNode: GenericNode,
  BranchNode: GenericNode,

  // Data
  TransformNode: GenericNode,
  DataMapperNode: GenericNode,
  FilterNode: GenericNode,
  ChunkTextNode: GenericNode,
  MemoryNode: GenericNode,

  // IO
  HttpRequestNode: GenericNode,
};
