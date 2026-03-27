/**
 * Error Route Node
 * Routes workflow to success/error based on error indicators.
 */
class ErrorRouteNode extends BaseNode {
    constructor(id, name = 'Error Route') {
        super(id, name, 'ErrorRouteNode', NODE_CATEGORIES.CONTROL);

        this.parameters = {
            errorMessageKey: 'error_message',
            statusCodeKey: 'http_status_code',
            errorStatusThreshold: 400
        };
    }

    getOutputPorts() {
        return [
            {
                id: 'success',
                label: 'Success',
                type: 'conditional',
                description: 'No error detected'
            },
            {
                id: 'error',
                label: 'Error',
                type: 'conditional',
                description: 'Error detected'
            }
        ];
    }

    static fromJSON(json) {
        const node = new ErrorRouteNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        return node;
    }
}

nodeRegistry.register('ErrorRouteNode', ErrorRouteNode);
