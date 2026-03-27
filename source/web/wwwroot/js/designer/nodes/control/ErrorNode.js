/**
 * ErrorNode
 * Workflow-level error handling entry point (max one per workflow in UI)
 */
class ErrorNode extends BaseNode {
    constructor(id, name = 'Error Handler') {
        super(id, name, 'ErrorNode', NODE_CATEGORIES.CONTROL);

        this.parameters = {
            description: ''
        };
    }

    /**
     * Error node has no input ports.
     * @returns {Array}
     */
    getInputPorts() {
        return [];
    }

    /**
     * Error handler flow starts from this node.
     * @returns {Array}
     */
    getOutputPorts() {
        return [
            {
                id: 'output',
                label: 'On Error',
                type: 'control',
                description: 'Runs when workflow enters error handling'
            }
        ];
    }

    getDefaultColor() {
        return '#e74c3c';
    }

    static fromJSON(json) {
        const node = new ErrorNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        return node;
    }
}

nodeRegistry.register('ErrorNode', ErrorNode);
