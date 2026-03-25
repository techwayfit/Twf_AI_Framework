/**
 * Large Language Model API call node
 * Mirrors TwfAiFramework.Nodes.AI.LlmNode
 */
class LlmNode extends BaseNode {
    constructor(id, name = 'LLM') {
        super(id, name, 'LlmNode', NODE_CATEGORIES.AI);
   
    // Set default parameters
   this.parameters = {
      provider: 'openai',
      model: 'gpt-4o',
    apiKey: '',
      apiUrl: '',
   systemPrompt: '',
      temperature: 0.7,
   maxTokens: 1000,
   maintainHistory: false
        };
    }

 /**
     * Deserialize from JSON
     * @param {object} json
* @returns {LlmNode}
     */
    static fromJSON(json) {
   const node = new LlmNode(json.id, json.name);
node.parameters = { ...node.parameters, ...json.parameters };
     node.position = json.position || { x: 0, y: 0 };
     node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
    return node;
}
}

// Register with registry
nodeRegistry.register('LlmNode', LlmNode);
