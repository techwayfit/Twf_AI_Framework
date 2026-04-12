import GenericNode from './GenericNode';
import CircularNode from './CircularNode';
import DiamondNode from './DiamondNode';
import ParallelNode from './ParallelNode';
import ContainerNode from './ContainerNode';
import NoteNode from './NoteNode';

/**
 * Registry mapping backend node type strings → React Flow node components.
 *
 * Only non-generic shapes need explicit entries here. All other node types
 * (including new nodes added to the backend) automatically use GenericNode
 * via the Proxy fallback — no manual registration required.
 */
const specialNodeTypes = {
  // Special shapes
  StartNode:      CircularNode,
  EndNode:        CircularNode,
  ErrorNode:      CircularNode,
  ErrorRouteNode: CircularNode,
  ConditionNode:  DiamondNode,
  ParallelNode:   ParallelNode,

  // Visual-only nodes
  ContainerNode:  ContainerNode,
  NoteNode:       NoteNode,
};

/**
 * Proxy that returns GenericNode for any type not explicitly registered above.
 * React Flow calls nodeTypes[node.type] to resolve the component — unknown
 * types previously fell back to React Flow's built-in default (top/bottom handles).
 * With this proxy they use GenericNode (left/right handles, colored border, icon).
 */
export const nodeTypes = new Proxy(specialNodeTypes, {
  get(target, prop) {
    return target[prop] ?? GenericNode;
  },
});
