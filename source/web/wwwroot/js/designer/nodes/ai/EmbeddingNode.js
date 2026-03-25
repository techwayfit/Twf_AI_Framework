/**
 * Embedding Node
 * Generates embeddings for text using various embedding models
 */
class EmbeddingNode extends BaseNode {
    constructor(id, name = 'Embedding') {
        super(id, name, 'EmbeddingNode', NODE_CATEGORIES.AI);
        
        this.parameters = {
model: 'text-embedding-3-small',
            apiKey: '',
      apiUrl: 'https://api.openai.com/v1/embeddings'
     };
    }

    static fromJSON(json) {
const node = new EmbeddingNode(json.id, json.name);
   node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
     node.color = json.color || node.getDefaultColor();
node.executionOptions = json.executionOptions;
 return node;
    }
}

nodeRegistry.register('EmbeddingNode', EmbeddingNode);
