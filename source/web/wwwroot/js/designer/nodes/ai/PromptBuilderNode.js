/**
 * Prompt Builder Node
 * Builds prompts from templates with variable interpolation
 */
class PromptBuilderNode extends BaseNode {
    constructor(id, name = 'Prompt Builder') {
  super(id, name, 'PromptBuilderNode', NODE_CATEGORIES.AI);
   
        this.parameters = {
 promptTemplate: '',
          systemTemplate: ''
        };
    }

    static fromJSON(json) {
   const node = new PromptBuilderNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
   node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        return node;
    }
}

nodeRegistry.register('PromptBuilderNode', PromptBuilderNode);
