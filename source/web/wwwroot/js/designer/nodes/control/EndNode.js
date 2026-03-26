/**
 * End Node
 * Exit point for workflow execution
 * Marks successful completion of the workflow
 */
class EndNode extends BaseNode {
    constructor(id, name = 'End') {
        super(id, name, 'EndNode', NODE_CATEGORIES.CONTROL);
        
        this.parameters = {
        status: 'success',
            outputKey: ''
        };
    }

    /**
     * Get input port definitions
     * @returns {Array}
  */
    getInputPorts() {
      return [
   { 
       id: 'input', 
    label: 'End', 
       type: 'control', 
    description: 'Workflow ends here' 
          }
        ];
    }

    /**
     * Get output port definitions
     * End node has no outputs
     * @returns {Array}
     */
    getOutputPorts() {
        return [];  // No output ports
    }

    /**
     * Get node color
     * @returns {string}
     */
    getDefaultColor() {
      return '#e74c3c';  // Red - stop/end
    }

    /**
     * Validate node configuration
     * @returns {object}
     */
    validate() {
    const errors = [];
   const warnings = [];

     // End node should have incoming connection
 if (!this.hasIncomingConnections()) {
         warnings.push('End node should be connected to terminate the workflow');
        }

      return {
   isValid: errors.length === 0,
errors,
          warnings
 };
    }

    static fromJSON(json) {
   const node = new EndNode(json.id, json.name);
    node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
  node.color = json.color || node.getDefaultColor();
   node.executionOptions = json.executionOptions;
        return node;
    }
}

nodeRegistry.register('EndNode', EndNode);
