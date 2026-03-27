// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Variables + Sub-Workflow Management
// ???????????????????????????????????????????????????????????????????????????

function renderVariablesList() {
    const variablesList = document.getElementById('variables-list');
    if (!variablesList || !workflow) {
        return;
    }

    ensureWorkflowCollections(workflow);

    const variableEntries = Object.entries(workflow.variables || {});
    if (variableEntries.length === 0) {
        variablesList.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-inbox"></i>
                <div>No variables defined</div>
                <div style="font-size: 0.75rem; margin-top: 8px;">
                    Click "Add" to create your first variable
                </div>
            </div>
        `;

        if (selectedVariable && selectedVariable !== '_new_') {
            selectedVariable = null;
            const panel = document.getElementById('properties-content');
            if (panel) {
                panel.innerHTML = '<p class="text-muted small">Select a variable or node to edit.</p>';
            }
        }
    } else {
        let html = '';
        variableEntries.forEach(([name, value]) => {
            const isSelected = selectedVariable === name;
            html += `
                <div class="variable-list-item ${isSelected ? 'selected' : ''}"
                    onclick="selectVariable('${name}')">
                    <div class="var-name">{{${name}}}</div>
                    <div class="var-value-preview">${value || '(empty)'}</div>
                </div>
            `;
        });

        variablesList.innerHTML = html;
        variablesList.scrollTop = variablesList.scrollHeight;
    }

    renderSubWorkflowsList();
    updateWorkflowTitle();
}

function renderSubWorkflowsList() {
    const listEl = document.getElementById('subworkflows-list');
    const backButton = document.getElementById('btn-back-main-workflow');
    const root = getRootWorkflow();

    if (!listEl || !root) {
        return;
    }

    ensureRootWorkflowCollections(root);
    const context = getActiveWorkflowContext();

    if (backButton) {
        backButton.classList.toggle('hidden', context.type !== 'sub');
    }

    if (!root.subWorkflows || root.subWorkflows.length === 0) {
        listEl.innerHTML = `
            <div class="empty-state" style="padding: 12px;">
                <i class="bi bi-diagram-2" style="font-size: 1rem;"></i>
                <div style="font-size: 0.8rem; margin-top: 6px;">No sub workflows</div>
                <div style="font-size: 0.72rem; margin-top: 4px;">Use Add to create one</div>
            </div>
        `;
        return;
    }

    let html = '';
    root.subWorkflows.forEach(sw => {
        const active = context.type === 'sub' && context.subWorkflowId === sw.id;
        const nodeCount = Array.isArray(sw.nodes) ? sw.nodes.length : 0;
        const connCount = Array.isArray(sw.connections) ? sw.connections.length : 0;
        const hasErrorHandler = !!sw.errorNodeId || (sw.nodes || []).some(n => n.type === 'ErrorNode');
        const safeName = escapeHtml(sw.name || 'Sub Workflow');

        html += `
            <div class="subworkflow-list-item ${active ? 'active' : ''}" onclick="openSubWorkflowDesigner('${sw.id}')">
                <div class="subworkflow-row">
                    <div class="subworkflow-name">${safeName}</div>
                    <div class="subworkflow-item-actions">
                        <button class="subworkflow-action-btn" title="Open in new tab" onclick="openSubWorkflowInNewTab('${sw.id}', event)">
                            <i class="bi bi-box-arrow-up-right"></i>
                        </button>
                        <button class="subworkflow-action-btn" title="Rename" onclick="renameSubWorkflow('${sw.id}', event)">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="subworkflow-action-btn subworkflow-action-btn-danger" title="Delete" onclick="deleteSubWorkflow('${sw.id}', event)">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
                <div class="subworkflow-meta">${nodeCount} node(s), ${connCount} connection(s)</div>
                <div class="subworkflow-meta">Error Handler: ${hasErrorHandler ? 'Yes' : 'No'}</div>
            </div>
        `;
    });

    listEl.innerHTML = html;
}

function promptCreateSubWorkflow(openAfterCreate = true) {
    const name = prompt('Sub workflow name:', 'Sub Workflow');
    if (name === null) {
        return null;
    }

    return createSubWorkflowByName(name, openAfterCreate);
}

function createSubWorkflowByName(name, openAfterCreate = false) {
    const cleanName = (name || '').trim();
    if (!cleanName) {
        alert('Sub workflow name is required.');
        return null;
    }

    const root = getRootWorkflow();
    if (!root) {
        return null;
    }

    ensureRootWorkflowCollections(root);

    const normalized = cleanName.toLowerCase();
    const duplicate = root.subWorkflows.find(sw => (sw.name || '').trim().toLowerCase() === normalized);
    if (duplicate) {
        alert(`Sub workflow "${cleanName}" already exists.`);
        return duplicate;
    }

    const child = {
        id: generateGuid(),
        name: cleanName,
        description: '',
        nodes: [],
        connections: [],
        variables: {},
        errorNodeId: null
    };

    root.subWorkflows.push(child);
    renderSubWorkflowsList();

    // Persist newly created sub-workflow immediately so refresh keeps it.
    if (typeof saveWorkflow === 'function') {
        saveWorkflow().catch(error => {
            console.error('Auto-save failed after sub workflow creation:', error);
        });
    }

    if (openAfterCreate) {
        openSubWorkflowDesigner(child.id);
    }

    return child;
}

function escapeHtml(value) {
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function getSubWorkflowById(subWorkflowId) {
    const root = getRootWorkflow();
    if (!root || !Array.isArray(root.subWorkflows)) {
        return null;
    }

    return root.subWorkflows.find(sw => sw.id === subWorkflowId) || null;
}

function getAllWorkflowScopes() {
    const root = getRootWorkflow();
    if (!root) {
        return [];
    }

    const scopes = [{ workflowId: root.id, workflowName: root.name || 'Main', nodes: root.nodes || [] }];
    (root.subWorkflows || []).forEach(sw => {
        scopes.push({ workflowId: sw.id, workflowName: sw.name || 'Sub Workflow', nodes: sw.nodes || [] });
    });
    return scopes;
}

function getSubWorkflowReferences(subWorkflowId) {
    const references = [];
    const scopes = getAllWorkflowScopes();

    scopes.forEach(scope => {
        (scope.nodes || []).forEach(node => {
            if (node?.type !== 'SubWorkflowNode') {
                return;
            }

            const mappedId = node?.parameters?.subWorkflowId || '';
            if (mappedId === subWorkflowId) {
                references.push({
                    workflowId: scope.workflowId,
                    workflowName: scope.workflowName,
                    nodeId: node.id,
                    nodeName: node.name || 'Sub Workflow'
                });
            }
        });
    });

    return references;
}

function openSubWorkflowInNewTab(subWorkflowId, event) {
    event?.stopPropagation();
    event?.preventDefault();

    if (!getSubWorkflowById(subWorkflowId)) {
        return;
    }

    const url = typeof buildSubWorkflowUrl === 'function'
        ? buildSubWorkflowUrl(subWorkflowId)
        : window.location.href;
    window.open(url, '_blank', 'noopener');
}

function renameSubWorkflow(subWorkflowId, event) {
    event?.stopPropagation();
    event?.preventDefault();

    const subWorkflow = getSubWorkflowById(subWorkflowId);
    if (!subWorkflow) {
        return;
    }

    const proposed = prompt('Rename sub workflow:', subWorkflow.name || 'Sub Workflow');
    if (proposed === null) {
        return;
    }

    const cleanName = (proposed || '').trim();
    if (!cleanName) {
        alert('Sub workflow name is required.');
        return;
    }

    const root = getRootWorkflow();
    const normalized = cleanName.toLowerCase();
    const duplicate = (root?.subWorkflows || []).find(sw =>
        sw.id !== subWorkflowId &&
        (sw.name || '').trim().toLowerCase() === normalized);
    if (duplicate) {
        alert(`Sub workflow "${cleanName}" already exists.`);
        return;
    }

    subWorkflow.name = cleanName;
    renderSubWorkflowsList();
    updateWorkflowTitle();

    if (typeof saveWorkflow === 'function') {
        saveWorkflow().catch(error => {
            console.error('Auto-save failed after sub workflow rename:', error);
        });
    }
}

function deleteSubWorkflow(subWorkflowId, event) {
    event?.stopPropagation();
    event?.preventDefault();

    const subWorkflow = getSubWorkflowById(subWorkflowId);
    if (!subWorkflow) {
        return;
    }

    const references = getSubWorkflowReferences(subWorkflowId);
    if (references.length > 0) {
        const first = references[0];
        alert(
            `Cannot delete "${subWorkflow.name}". It is used by ${references.length} SubWorkflowNode(s).\n\n` +
            `First reference: workflow "${first.workflowName}", node "${first.nodeName}".`
        );
        return;
    }

    const confirmed = confirm(`Delete sub workflow "${subWorkflow.name}"?\n\nThis action cannot be undone.`);
    if (!confirmed) {
        return;
    }

    const root = getRootWorkflow();
    root.subWorkflows = (root.subWorkflows || []).filter(sw => sw.id !== subWorkflowId);

    const context = getActiveWorkflowContext();
    if (context.type === 'sub' && context.subWorkflowId === subWorkflowId) {
        openMainWorkflowDesigner();
    } else {
        renderSubWorkflowsList();
    }

    if (typeof saveWorkflow === 'function') {
        saveWorkflow().catch(error => {
            console.error('Auto-save failed after sub workflow delete:', error);
        });
    }
}

function selectVariable(name) {
    selectedVariable = name;
    selectedNode = null;
    selectedNodes.clear();

    renderVariablesList();
    renderVariableProperties();
}

function showAddVariableForm() {
    selectedVariable = '_new_';
    selectedNode = null;
    selectedNodes.clear();

    renderVariablesList();
    renderAddVariableForm();
}

function renderAddVariableForm() {
    const panel = document.getElementById('properties-content');
    panel.innerHTML = `
        <h6 class="border-bottom pb-2 mb-3">
            <i class="bi bi-plus-circle"></i> Add Variable
        </h6>

        <div class="mb-3">
            <label class="form-label small fw-bold">Variable Name <span class="text-danger">*</span></label>
            <input type="text" id="new-var-name" class="form-control form-control-sm"
                placeholder="e.g., api_key"
                pattern="[a-zA-Z_][a-zA-Z0-9_]*" />
            <small class="form-text text-muted">
                Letters, numbers, underscores only. Must start with letter or underscore.
            </small>
        </div>

        <div class="mb-3">
            <label class="form-label small fw-bold">Default Value</label>
            <textarea id="new-var-value" class="form-control form-control-sm" rows="3"
                placeholder="Enter default value..."></textarea>
        </div>

        <div class="d-grid gap-2">
            <button class="btn btn-sm btn-primary" onclick="createVariable()">
                <i class="bi bi-check-circle"></i> Create Variable
            </button>
            <button class="btn btn-sm btn-secondary" onclick="cancelAddVariable()">
                <i class="bi bi-x-circle"></i> Cancel
            </button>
        </div>
    `;

    setTimeout(() => document.getElementById('new-var-name')?.focus(), 100);
}

function createVariable() {
    const nameInput = document.getElementById('new-var-name');
    const valueInput = document.getElementById('new-var-value');

    const name = nameInput.value.trim();
    const value = valueInput.value;

    if (!name) {
        alert('Variable name is required.');
        nameInput.focus();
        return;
    }

    if (!/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(name)) {
        alert('Invalid variable name. Must start with letter or underscore and contain only letters, numbers, and underscores.');
        nameInput.focus();
        return;
    }

    if (Object.prototype.hasOwnProperty.call(workflow.variables, name)) {
        alert(`Variable "{{${name}}}" already exists!`);
        nameInput.focus();
        return;
    }

    workflow.variables[name] = value;
    selectedVariable = name;

    renderVariablesList();
    renderVariableProperties();

    console.log(`Variable created: {{${name}}} = ${value}`);
}

function cancelAddVariable() {
    selectedVariable = null;
    renderVariablesList();
    document.getElementById('properties-content').innerHTML =
        '<p class="text-muted small">Select a variable or node to edit.</p>';
}

function renderVariableProperties() {
    if (!selectedVariable || selectedVariable === '_new_') return;

    const value = workflow.variables[selectedVariable];
    const panel = document.getElementById('properties-content');

    panel.innerHTML = `
        <h6 class="border-bottom pb-2 mb-3">
            <i class="bi bi-braces"></i> Variable
        </h6>

        <div class="mb-3">
            <label class="form-label small fw-bold">Name</label>
            <div class="input-group input-group-sm">
                <span class="input-group-text">{{</span>
                <input type="text" class="form-control form-control-sm"
                    value="${selectedVariable}" disabled />
                <span class="input-group-text">}}</span>
            </div>
            <small class="form-text text-muted">Variable name cannot be changed after creation.</small>
        </div>

        <div class="mb-3">
            <label class="form-label small fw-bold">Value</label>
            <textarea id="var-value-input" class="form-control form-control-sm" rows="4"
                onchange="updateVariableValue('${selectedVariable}', this.value)">${value || ''}</textarea>
            <small class="form-text text-muted">
                This value will be used when {{${selectedVariable}}} is referenced.
            </small>
        </div>

        <div class="alert alert-info small">
            <i class="bi bi-info-circle"></i>
            <strong>Usage:</strong> Type <code>{{${selectedVariable}}}</code> in any text parameter.
        </div>

        <div class="d-grid gap-2 mt-3">
            <button class="btn btn-sm btn-danger" onclick="deleteVariable('${selectedVariable}')">
                <i class="bi bi-trash"></i> Delete Variable
            </button>
        </div>
    `;
}

function updateVariableValue(name, value) {
    workflow.variables[name] = value;
    renderVariablesList();
    console.log(`Variable updated: {{${name}}} = ${value}`);
}

function deleteVariable(name) {
    if (!confirm(`Delete variable {{${name}}}?\n\nThis will not update nodes that reference this variable.`)) {
        return;
    }

    delete workflow.variables[name];
    selectedVariable = null;

    renderVariablesList();
    document.getElementById('properties-content').innerHTML =
        '<p class="text-muted small">Select a variable or node to edit.</p>';

    console.log(`Variable deleted: {{${name}}}`);
}
