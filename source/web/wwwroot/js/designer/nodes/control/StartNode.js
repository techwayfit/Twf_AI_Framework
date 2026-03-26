/**
 * Start Node
 * Entry point for workflow execution
 * Every workflow must have exactly one Start node
 */
class StartNode extends BaseNode {
    constructor(id, name = 'Start') {
        super(id, name, 'StartNode', NODE_CATEGORIES.CONTROL);
        
   this.parameters = {
        description: ''
};
    }

    /**
     * Get input port definitions
     * Start node has no inputs
     * @returns {Array}
     */
    getInputPorts() {
        return [];  // No input ports
    }

    /**
     * Get output port definitions
     * @returns {Array}
   */
    getOutputPorts() {
        return [
    { 
     id: 'output', 
 label: 'Start', 
      type: 'control', 
    description: 'Workflow begins here' 
        }
      ];
 }

    /**
     * Get node color
     * @returns {string}
     */
    getDefaultColor() {
        return '#2ecc71';  // Green - go/start
    }

    /**
     * Validate node configuration
 * @returns {object}
     */
    validate() {
     const errors = [];
   const warnings = [];

        // Start node should be connected
        if (!this.hasOutgoingConnections()) {
            warnings.push('Start node should be connected to begin the workflow');
        }

        return {
     isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    static fromJSON(json) {
        const node = new StartNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
    node.executionOptions = json.executionOptions;
   return node;
    }
}

nodeRegistry.register('StartNode', StartNode);
