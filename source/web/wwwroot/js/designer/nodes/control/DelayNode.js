/**
 * Delay Node
 * Introduces a delay in workflow execution
 */
class DelayNode extends BaseNode {
    constructor(id, name = 'Delay') {
        super(id, name, 'DelayNode', NODE_CATEGORIES.CONTROL);
    
 this.parameters = {
      delayMs: 1000,
    reason: ''
};
    }

static fromJSON(json) {
    const node = new DelayNode(json.id, json.name);
      node.parameters = { ...node.parameters, ...json.parameters };
  node.position = json.position || { x: 0, y: 0 };
 node.color = json.color || node.getDefaultColor();
  node.executionOptions = json.executionOptions;
      return node;
    }
}

nodeRegistry.register('DelayNode', DelayNode);
