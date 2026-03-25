/**
 * Chunk Text Node
 * Splits text into smaller chunks for processing
 */
class ChunkTextNode extends BaseNode {
    constructor(id, name = 'Chunk Text') {
 super(id, name, 'ChunkTextNode', NODE_CATEGORIES.DATA);
        
        this.parameters = {
chunkSize: 500,
 overlap: 50,
     strategy: 'Character'
 };
  }

    static fromJSON(json) {
   const node = new ChunkTextNode(json.id, json.name);
  node.parameters = { ...node.parameters, ...json.parameters };
    node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
  node.executionOptions = json.executionOptions;
  return node;
    }
}

nodeRegistry.register('ChunkTextNode', ChunkTextNode);
