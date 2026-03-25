/**
 * Transform Node
 * Transforms data using various transformation strategies
 */
class TransformNode extends BaseNode {
    constructor(id, name = 'Transform') {
 super(id, name, 'TransformNode', NODE_CATEGORIES.DATA);
        
 this.parameters = {
     transformType: 'custom',
      fromKey: '',
            toKey: '',
            keys: '',
       separator: ' '
  };
    }

    static fromJSON(json) {
 const node = new TransformNode(json.id, json.name);
      node.parameters = { ...node.parameters, ...json.parameters };
 node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
 node.executionOptions = json.executionOptions;
     return node;
}
}

nodeRegistry.register('TransformNode', TransformNode);
