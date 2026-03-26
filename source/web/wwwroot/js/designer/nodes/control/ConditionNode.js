/**
 * Condition Node
 * Routes workflow based on conditional expressions
 * Evaluates a single condition and routes to success or failure port
 */
class ConditionNode extends BaseNode {
    constructor(id, name = 'Condition') {
        super(id, name, 'ConditionNode', NODE_CATEGORIES.CONTROL);
        
        this.parameters = {
            condition: ''// Single condition expression
        };
 }

    /**
     * Get output port definitions
     * @returns {Array}
     */
    getOutputPorts() {
    return [
 { 
  id: 'success', 
      label: 'Success', 
       type: 'conditional', 
        description: 'Condition evaluates to true' 
     },
            { 
id: 'failure', 
                label: 'Failure', 
           type: 'conditional', 
      description: 'Condition evaluates to false or error' 
            }
        ];
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
