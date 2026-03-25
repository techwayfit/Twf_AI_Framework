/**
 * Memory Node
 * Reads from and writes to workflow memory storage
 */
class MemoryNode extends BaseNode {
    constructor(id, name = 'Memory') {
    super(id, name, 'MemoryNode', NODE_CATEGORIES.DATA);
  
        this.parameters = {
 mode: 'Read',
 keys: ''
};
    }

    static fromJSON(json) {
        const node = new MemoryNode(json.id, json.name);
      node.parameters = { ...node.parameters, ...json.parameters };
   node.position = json.position || { x: 0, y: 0 };
   node.color = json.color || node.getDefaultColor();
    node.executionOptions = json.executionOptions;
   return node;
}
}

nodeRegistry.register('MemoryNode', MemoryNode);
