/**
 * SubWorkflowNode
 * Executes a child workflow and exposes success/error branches.
 */
class SubWorkflowNode extends BaseNode {
    constructor(id, name = 'Sub Workflow') {
        super(id, name, 'SubWorkflowNode', NODE_CATEGORIES.CONTROL);

        this.parameters = {
            subWorkflowId: ''
        };
    }

    getInputPorts() {
        return [
            {
                id: 'input',
                label: 'Input',
                type: 'data',
                required: true,
                description: 'Input data for selected child workflow'
            }
        ];
    }

    getOutputPorts() {
        return [
            {
                id: 'success',
                label: 'Success',
                type: 'conditional',
                condition: 'success',
                description: 'Selected sub workflow succeeds'
            },
            {
                id: 'error',
                label: 'Error',
                type: 'conditional',
                condition: 'error',
                description: 'Selected sub workflow fails'
            }
        ];
    }

    getDefaultColor() {
        return '#8e44ad';
    }

    renderProperties() {
        const root = (typeof getRootWorkflow === 'function')
            ? getRootWorkflow()
            : (window.rootWorkflow || workflow);

        const subWorkflows = root?.subWorkflows || [];
        const selectedId = this.parameters.subWorkflowId || '';
        const selectedWorkflow = subWorkflows.find(sw => sw.id === selectedId) || null;

        const optionsHtml = [
            '<option value="">-- Select Sub Workflow --</option>',
            ...subWorkflows.map(sw => {
                const selected = sw.id === selectedId ? 'selected' : '';
                const nodeCount = Array.isArray(sw.nodes) ? sw.nodes.length : 0;
                return `<option value="${sw.id}" ${selected}>${sw.name} (${nodeCount} nodes)</option>`;
            })
        ].join('');

        return `
            <h6 class="border-bottom pb-2 mb-3">
                <i class="bi bi-box-arrow-in-down-right"></i> ${this.name}
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
            <h6 class="small fw-bold mb-3">Sub Workflow</h6>

            <div class="mb-3">
                <label class="form-label small fw-bold">Selected Workflow <span class="text-danger">*</span></label>
                <select class="form-select form-select-sm"
                    onchange="window.designerInstance.updateNodeParameter('${this.id}', 'subWorkflowId', this.value); renderProperties();">
                    ${optionsHtml}
                </select>
                <small class="form-text text-muted">
                    Calls a reusable child workflow and routes to Success/Error outputs.
                </small>
            </div>

            <div class="d-grid gap-2 mb-3">
                <button class="btn btn-sm btn-primary"
                    onclick="SubWorkflowNode.openAssignedWorkflow('${this.id}')"
                    ${selectedWorkflow ? '' : 'disabled'}>
                    <i class="bi bi-pencil-square"></i> Open Selected Workflow
                </button>
                <button class="btn btn-sm btn-outline-primary"
                    onclick="SubWorkflowNode.createAndAssign('${this.id}')">
                    <i class="bi bi-plus-circle"></i> Create New Sub Workflow
                </button>
            </div>

            <div class="alert alert-info small">
                <i class="bi bi-info-circle"></i>
                ${selectedWorkflow
                    ? `Selected: <strong>${selectedWorkflow.name}</strong>`
                    : 'No sub workflow selected yet.'}
            </div>

            <hr class="mt-4" />
            <div class="d-grid gap-2">
                <button class="btn btn-danger btn-sm" onclick="window.designerInstance.deleteNode('${this.id}')">
                    <i class="bi bi-trash"></i> Delete Node
                </button>
            </div>
        `;
    }

    static openAssignedWorkflow(nodeId) {
        const node = window.designerInstance?.getNode(nodeId);
        const subWorkflowId = node?.parameters?.subWorkflowId;

        if (!subWorkflowId) {
            alert('Select a sub workflow first.');
            return;
        }

        if (typeof openSubWorkflowDesigner === 'function') {
            openSubWorkflowDesigner(subWorkflowId);
        }
    }

    static createAndAssign(nodeId) {
        const node = window.designerInstance?.getNode(nodeId);
        if (!node) {
            return;
        }

        if (typeof promptCreateSubWorkflow !== 'function') {
            alert('Sub workflow creation is not available.');
            return;
        }

        const created = promptCreateSubWorkflow(false);
        if (!created?.id) {
            return;
        }

        window.designerInstance.updateNodeParameter(nodeId, 'subWorkflowId', created.id);
        if (typeof renderProperties === 'function') {
            renderProperties();
        }
    }

    static fromJSON(json) {
        const node = new SubWorkflowNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        return node;
    }
}

nodeRegistry.register('SubWorkflowNode', SubWorkflowNode);
