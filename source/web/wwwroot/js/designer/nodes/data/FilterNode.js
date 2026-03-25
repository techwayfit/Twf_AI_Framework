/**
 * Filter Node
 * Validates and filters workflow data
 */
class FilterNode extends BaseNode {
    constructor(id, name = 'Filter') {
  super(id, name, 'FilterNode', NODE_CATEGORIES.DATA);
    
        this.parameters = {
      throwOnFail: true,
    requiredKeys: '',
  maxLengths: {}
     };
  }

    static fromJSON(json) {
   const node = new FilterNode(json.id, json.name);
   node.parameters = { ...node.parameters, ...json.parameters };
    node.position = json.position || { x: 0, y: 0 };
      node.color = json.color || node.getDefaultColor();
     node.executionOptions = json.executionOptions;
      return node;
    }
}

nodeRegistry.register('FilterNode', FilterNode);
