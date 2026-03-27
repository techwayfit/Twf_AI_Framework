/**
 * Try Catch Node
 * Container node with separate try/catch sub-workflows.
 */
class TryCatchNode extends BaseNode {
    constructor(id, name = 'Try Catch') {
        super(id, name, 'TryCatchNode', NODE_CATEGORIES.CONTROL);

        this.parameters = {
            rethrowOnCatchFailure: true
        };

        this.tryWorkflow = {
            nodes: [],
            connections: [],
            variables: {}
        };

        this.catchWorkflow = {
            nodes: [],
            connections: [],
            variables: {}
        };
    }

    getOutputPorts() {
        return [
            {
                id: 'success',
                label: 'Success',
                type: 'conditional',
                description: 'Try workflow succeeded'
            },
            {
                id: 'error',
                label: 'Error',
                type: 'conditional',
                description: 'Try workflow failed and catch handled'
            }
        ];
    }

    renderProperties() {
        const schema = this.getSchema();
        let html = `
            <h6 class="border-bottom pb-2 mb-3">
                <i class="bi bi-shield-exclamation"></i> ${this.name}
            </h6>

            <div class="mb-3">
                <label class="form-label small fw-bold">Node Name</label>
                <input type="text" class="form-control form-control-sm"
                    value="${this.name}"
                    onchange="window.designerInstance.updateNodeProperty('${this.id}', 'name', this.value)" />
            </div>

            <div class="mb-3">
                <label class="form-label small text-muted">Type</label>
                <input type="text" class="form-control form-control-sm"
                    value="${this.type}" disabled />
            </div>

            <hr />
            <h6 class="small fw-bold mb-3">
                <i class="bi bi-lightning-charge"></i> Try/Catch Configuration
            </h6>
        `;

        if (schema.parameters) {
            schema.parameters.forEach(param => {
                html += this.renderParameter(param);
            });
        }

        const tryNodeCount = this.tryWorkflow?.nodes?.length || 0;
        const tryConnCount = this.tryWorkflow?.connections?.length || 0;
        const catchNodeCount = this.catchWorkflow?.nodes?.length || 0;
        const catchConnCount = this.catchWorkflow?.connections?.length || 0;

        html += `
            <hr />
            <h6 class="small fw-bold mb-3">
                <i class="bi bi-diagram-3"></i> Nested Workflows
            </h6>

            <div class="alert alert-info small mb-3">
                <i class="bi bi-info-circle"></i>
                Define steps for the <strong>Try</strong> path and fallback <strong>Catch</strong> path.
            </div>

            <div class="mb-2">
                <button class="btn btn-outline-primary btn-sm w-100"
                    onclick="window.openTryCatchWorkflowEditor('${this.id}', 'tryWorkflow')">
                    <i class="bi bi-play-circle"></i> Edit Try Workflow
                    ${tryNodeCount > 0 ? `<span class="badge bg-secondary ms-2">${tryNodeCount} nodes, ${tryConnCount} connections</span>` : ''}
                </button>
            </div>

            <div class="mb-3">
                <button class="btn btn-outline-danger btn-sm w-100"
                    onclick="window.openTryCatchWorkflowEditor('${this.id}', 'catchWorkflow')">
                    <i class="bi bi-life-preserver"></i> Edit Catch Workflow
                    ${catchNodeCount > 0 ? `<span class="badge bg-secondary ms-2">${catchNodeCount} nodes, ${catchConnCount} connections</span>` : ''}
                </button>
            </div>
        `;

        if (schema.executionOptions && schema.executionOptions.length > 0) {
            html += '<hr />';
            html += ExecutionOptionsEditor.render(this, schema.executionOptions);
        }

        html += `
            <hr class="mt-4" />
            <div class="d-grid gap-2">
                <button class="btn btn-danger btn-sm" onclick="window.designerInstance.deleteNode('${this.id}')">
                    <i class="bi bi-trash"></i> Delete Node
                </button>
            </div>
        `;

        return html;
    }

    static editWorkflow(nodeId, workflowKey) {
        const workflowRef = window.workflow || (typeof workflow !== 'undefined' ? workflow : null);
        const node =
            window.designerInstance?.getNode(nodeId) ||
            (typeof selectedNode !== 'undefined' && selectedNode?.id === nodeId ? selectedNode : null) ||
            workflowRef?.nodes?.find(n => `${n.id}` === `${nodeId}`) ||
            null;

        if (!node) {
            console.error(`TryCatchNode ${nodeId} not found`);
            alert('TryCatchNode not found in current workflow state. Please refresh and try again.');
            return;
        }

        if (!window.SubWorkflowEditor || typeof window.SubWorkflowEditor.openEditor !== 'function') {
            console.error('SubWorkflowEditor is not available on window');
            alert('Sub-workflow editor is not available.');
            return;
        }

        const isTry = workflowKey === 'tryWorkflow';
        const title = isTry
            ? `Edit Try Workflow: ${node.name}`
            : `Edit Catch Workflow: ${node.name}`;
        const hint = isTry
            ? 'Define steps to run inside the try block'
            : 'Define fallback steps to run when try fails';

        window.SubWorkflowEditor.openEditor(nodeId, workflowKey, { title, hint });
    }

    toJSON() {
        return {
            ...super.toJSON(),
            tryWorkflow: this.tryWorkflow,
            catchWorkflow: this.catchWorkflow
        };
    }

    static fromJSON(json) {
        const node = new TryCatchNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        node.tryWorkflow = json.tryWorkflow || { nodes: [], connections: [], variables: {} };
        node.catchWorkflow = json.catchWorkflow || { nodes: [], connections: [], variables: {} };
        return node;
    }
}

nodeRegistry.register('TryCatchNode', TryCatchNode);
window.TryCatchNode = TryCatchNode;
window.openTryCatchWorkflowEditor = function(nodeId, workflowKey) {
    TryCatchNode.editWorkflow(nodeId, workflowKey);
};
