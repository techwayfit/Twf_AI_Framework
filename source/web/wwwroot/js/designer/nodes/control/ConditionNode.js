/**
 * Condition Node
 * Routes workflow based on conditional expressions
 */
class ConditionNode extends BaseNode {
    constructor(id, name = 'Condition') {
   super(id, name, 'ConditionNode', NODE_CATEGORIES.CONTROL);
    
this.parameters = {
conditions: {}
   };
    }

    static fromJSON(json) {
   const node = new ConditionNode(json.id, json.name);
   node.parameters = { ...node.parameters, ...json.parameters };
    node.position = json.position || { x: 0, y: 0 };
    node.color = json.color || node.getDefaultColor();
 node.executionOptions = json.executionOptions;
   return node;
    }
}

nodeRegistry.register('ConditionNode', ConditionNode);
