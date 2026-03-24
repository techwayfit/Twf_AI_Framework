// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Variables Management
// ???????????????????????????????????????????????????????????????????????????

function renderVariablesList() {
    const variablesList = document.getElementById('variables-list');
    
    if (!workflow.variables || Object.keys(workflow.variables).length === 0) {
      variablesList.innerHTML = `
            <div class="empty-state">
     <i class="bi bi-inbox"></i>
 <div>No variables defined</div>
 <div style="font-size: 0.75rem; margin-top: 8px;">
          Click "Add" to create your first variable
    </div>
      </div>
   `;
        
// Clear properties panel if showing variable
     if (selectedVariable) {
            selectedVariable = null;
 document.getElementById('properties-content').innerHTML = 
        '<p class="text-muted small">Select a variable or node to edit.</p>';
        }
        return;
    }
    
    let html = '';
    Object.entries(workflow.variables).forEach(([name, value]) => {
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
  
    // Focus on name input
    setTimeout(() => document.getElementById('new-var-name')?.focus(), 100);
}

function createVariable() {
    const nameInput = document.getElementById('new-var-name');
    const valueInput = document.getElementById('new-var-value');
    
    const name = nameInput.value.trim();
  const value = valueInput.value;
    
    // Validate name
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
    
    // Check if already exists
    if (workflow.variables[name]) {
     alert(`Variable "{{${name}}}" already exists!`);
    nameInput.focus();
  return;
    }
    
    // Create variable
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
