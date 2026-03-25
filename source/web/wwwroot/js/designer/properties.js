// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Properties Panel
// ???????????????????????????????????????????????????????????????????????????

function renderProperties() {
    if (!selectedNode) return;
    
    const panel = document.getElementById('properties-content');
 
    // Check if node has its own renderProperties method (new architecture)
    if (typeof selectedNode.renderProperties === 'function') {
  try {
  panel.innerHTML = selectedNode.renderProperties();
     return;
 } catch (error) {
   console.error('Error rendering properties from node class:', error);
   // Fall through to legacy rendering
     }
  }
    
  // Legacy rendering for plain objects
    const schema = nodeSchemas[selectedNode.type];
    
    if (!schema) {
panel.innerHTML = `
       <div class="alert alert-warning small">
   <i class="bi bi-exclamation-triangle"></i> No schema available for ${selectedNode.type}
    </div>
        `;
return;
    }
    
  let html = `
        <h6 class="border-bottom pb-2 mb-3">
      <i class="bi bi-diagram-3"></i> ${selectedNode.name}
        </h6>
  
        <div class="mb-3">
      <label class="form-label small fw-bold">Node Name</label>
     <input type="text" class="form-control form-control-sm" 
     value="${selectedNode.name}" 
         onchange="updateNodeProperty('name', this.value)" />
        </div>
        
   <div class="mb-3">
 <label class="form-label small text-muted">Node Type</label>
   <input type="text" class="form-control form-control-sm" 
     value="${selectedNode.type}" disabled />
   </div>
        
    <hr />
<h6 class="small fw-bold mb-3">Parameters</h6>
    `;
 
    // Render each parameter based on its type
  schema.parameters.forEach(param => {
    html += renderParameterField(param, selectedNode.parameters[param.name]);
    });
    
    // Add delete button at the bottom
    html += `
     <hr class="mt-4" />
  <div class="d-grid gap-2">
      <button class="btn btn-danger" onclick="deleteSelectedNode()">
                <i class="bi bi-trash"></i> Delete Node
            </button>
        </div>
    `;
    
    panel.innerHTML = html;
}

function renderParameterField(param, currentValue) {
 const value = currentValue !== undefined ? currentValue : param.defaultValue;
    const required = param.required ? 'required' : '';
    const requiredLabel = param.required ? '<span class="text-danger">*</span>' : '';
    
    // Check if value contains variables
    const hasVariables = typeof value === 'string' && value.includes('{{');
 const inputClass = hasVariables ? 'has-variables' : '';
    
    let fieldHtml = `
        <div class="mb-3">
  <label class="form-label small fw-bold">
          ${param.label} ${requiredLabel}
       </label>
    `;
    
    const inputId = `param-${param.name}-${selectedNode.id}`;
    
    switch (param.type) {
   case 'Text':
        fieldHtml += `
        <input type="text" 
      id="${inputId}"
    class="form-control form-control-sm ${inputClass}" 
            value="${value || ''}"
          placeholder="${param.placeholder || ''}"
${required}
    onchange="updateNodeParameter('${param.name}', this.value)" />
    `;
   break;
     
        case 'TextArea':
            fieldHtml += `
       <textarea id="${inputId}"
         class="form-control form-control-sm ${inputClass}" 
   rows="3"
     placeholder="${param.placeholder || ''}"
           ${required}
        onchange="updateNodeParameter('${param.name}', this.value)">${value || ''}</textarea>
            `;
   break;
        
 case 'Number':
     fieldHtml += `
  <input type="number" 
   id="${inputId}"
    class="form-control form-control-sm" 
   value="${value || ''}"
      min="${param.minValue || ''}"
       max="${param.maxValue || ''}"
      ${required}
           onchange="updateNodeParameter('${param.name}', parseFloat(this.value))" />
      `;
   break;
  
        case 'Boolean':
         const checked = value ? 'checked' : '';
  fieldHtml += `
     <div class="form-check">
    <input type="checkbox" 
        class="form-check-input" 
    id="${inputId}"
    ${checked}
      onchange="updateNodeParameter('${param.name}', this.checked)" />
       <label class="form-check-label small" for="${inputId}">
      ${param.description || 'Enable this option'}
  </label>
    </div>
  `;
            break;
   
   case 'Select':
 fieldHtml += `<select id="${inputId}" class="form-select form-select-sm" 
       ${required}
       onchange="updateNodeParameter('${param.name}', this.value)">`;
            if (!param.required) {
   fieldHtml += '<option value="">-- Select --</option>';
            }
   param.options?.forEach(opt => {
const selected = value === opt.value ? 'selected' : '';
fieldHtml += `<option value="${opt.value}" ${selected}>${opt.label}</option>`;
         });
            fieldHtml += '</select>';
   break;
        
        case 'Json':
      fieldHtml += `
  <textarea id="${inputId}" class="form-control form-control-sm font-monospace ${inputClass}" 
        rows="4"
       placeholder="${param.placeholder || '{}'}"
      ${required}
           onchange="updateNodeParameterJson('${param.name}', this.value)">${value ? JSON.stringify(value, null, 2) : ''}</textarea>
     <small class="form-text text-muted">Enter valid JSON or use {{variable}} syntax</small>
            `;
        break;
 
   default:
    fieldHtml += `
    <input type="text" 
     id="${inputId}"
 class="form-control form-control-sm ${inputClass}" 
  value="${value || ''}"
         ${required}
          onchange="updateNodeParameter('${param.name}', this.value)" />
     `;
    }
    
    if (param.description && param.type !== 'Boolean') {
 fieldHtml += `<small class="form-text text-muted">${param.description}</small>`;
    }
    
    // Show available variables hint
    if ((param.type === 'Text' || param.type === 'TextArea' || param.type === 'Json') && 
workflow.variables && Object.keys(workflow.variables).length > 0) {
        const count = Object.keys(workflow.variables).length;
   fieldHtml += `<small class="form-text text-warning">
      <i class="bi bi-lightbulb"></i> Type <code>{{</code> to see ${count} available variable${count > 1 ? 's' : ''}
        </small>`;
    }
    
    fieldHtml += '</div>';
    
 // Setup autocomplete after rendering
    if (param.type === 'Text' || param.type === 'TextArea' || param.type === 'Json') {
 setTimeout(() => {
       const input = document.getElementById(inputId);
            if (input) {
                setupVariableAutocomplete(input, inputId);
  }
        }, 0);
    }
    
    return fieldHtml;
}

function updateNodeParameter(paramName, value) {
    // Delegate to designerInstance if available (new architecture)
    if (window.designerInstance && selectedNode) {
window.designerInstance.updateNodeParameter(selectedNode.id, paramName, value);
    return;
    }
    
    // Fallback to direct update
    if (selectedNode) {
        if (!selectedNode.parameters) selectedNode.parameters = {};
        selectedNode.parameters[paramName] = value;
      console.log(`Updated ${selectedNode.name}.${paramName} = ${value}`);
    }
}

function updateNodeParameterJson(paramName, jsonString) {
    // Delegate to designerInstance if available (new architecture)
  if (window.designerInstance && selectedNode) {
   window.designerInstance.updateNodeParameterJson(selectedNode.id, paramName, jsonString);
     return;
    }
    
    // Fallback to direct update
    if (!selectedNode) return;
    
    try {
        const value = jsonString ? JSON.parse(jsonString) : null;
    if (!selectedNode.parameters) selectedNode.parameters = {};
  selectedNode.parameters[paramName] = value;
 console.log(`Updated ${selectedNode.name}.${paramName} =`, value);
    } catch (error) {
        console.error('Invalid JSON:', error);
        alert('Invalid JSON format. Please check your input.');
    }
}

function updateNodeProperty(property, value) {
    // Delegate to designerInstance if available (new architecture)
  if (window.designerInstance && selectedNode) {
window.designerInstance.updateNodeProperty(selectedNode.id, property, value);
        return;
    }
    
    // Fallback to direct update
  if (selectedNode) {
    selectedNode[property] = value;
        render();
    }
}

// Helper function to delete the selected node
function deleteSelectedNode() {
    if (!selectedNode) return;
    
    if (confirm(`Delete node "${selectedNode.name}"?`)) {
deleteNode(selectedNode.id);
    }
}
