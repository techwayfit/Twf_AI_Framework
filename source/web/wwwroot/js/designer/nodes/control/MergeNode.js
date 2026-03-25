/**
 * Merge Node
 * Merges multiple data values into a single output
 */
class MergeNode extends BaseNode {
    constructor(id, name = 'Merge') {
   super(id, name, 'MergeNode', NODE_CATEGORIES.CONTROL);
        
 this.parameters = {
     sourceKeys: '',
      outputKey: '',
      separator: '\n'
  };
    }

    static fromJSON(json) {
const node = new MergeNode(json.id, json.name);
  node.parameters = { ...node.parameters, ...json.parameters };
      node.position = json.position || { x: 0, y: 0 };
node.color = json.color || node.getDefaultColor();
  node.executionOptions = json.executionOptions;
 return node;
 }
}

nodeRegistry.register('MergeNode', MergeNode);
