/**
 * Abstract base class for all workflow nodes
 * Mirrors TwfAiFramework.Nodes.BaseNode
 */
class BaseNode {
    /**
     * @param {string} id - Unique node identifier
     * @param {string} name - Display name
   * @param {string} type - Node type (e.g., 'LlmNode')
     * @param {string} category - Node category
     */
    constructor(id, name, type, category) {
      if (new.target === BaseNode) {
   throw new Error('BaseNode is abstract and cannot be instantiated');
        }
    
   this.id = id;
        this.name = name;
        this.type = type;
        this.category = category;
        this.parameters = {};
        this.position = { x: 0, y: 0 };
        this.color = this.getDefaultColor();
        this.executionOptions = null;
    }

    /**
     * Get node schema from global nodeSchemas
     * @returns {object}
     */
    getSchema() {
    return nodeSchemas[this.type] || {};
 }

    /**
  * Get input port definitions
  * @returns {Array}
     */
    getInputPorts() {
        const schema = this.getSchema();
     return schema.inputPorts || [
        { id: 'input', label: 'Input', type: 'data', required: true }
        ];
    }

    /**
  * Get output port definitions
 * @returns {Array}
     */
    getOutputPorts() {
  const schema = this.getSchema();
        return schema.outputPorts || [
      { id: 'output', label: 'Output', type: 'data' }
        ];
    }

    /**
     * Validate node configuration
     * @returns {{isValid: boolean, errors: Array<string>}}
     */
    validate() {
const errors = [];
   const schema = this.getSchema();
    
        // Validate required parameters
      if (schema.parameters) {
  schema.parameters.forEach(param => {
                if (param.required && !this.parameters[param.name]) {
        errors.push(`${param.label} is required`);
    }
      });
      }
        
      return {
     isValid: errors.length === 0,
            errors
        };
    }

    /**
     * Get default color for this node type
     * @returns {string}
     */
    getDefaultColor() {
    return NODE_COLORS[this.category] || '#95a5a6';
    }

    /**
     * Render properties panel content
   * @returns {string} HTML string
 */
    renderProperties() {
        const schema = this.getSchema();
      
      let html = `
         <h6 class="border-bottom pb-2 mb-3">
     <i class="bi bi-cpu"></i> ${this.name}
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
         <h6 class="small fw-bold mb-3">Parameters</h6>
        `;
        
        // Render each parameter
        if (schema.parameters) {
    schema.parameters.forEach(param => {
                html += this.renderParameter(param);
    });
    }

        // Render execution options (Phase 4)
        if (schema.executionOptions && schema.executionOptions.length > 0) {
       html += '<hr />';
            html += ExecutionOptionsEditor.render(this, schema.executionOptions);
        }

        // Add delete button at the bottom
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

    /**
     * Render a single parameter field
     * @param {object} param - Parameter definition
  * @returns {string} HTML string
     */
    renderParameter(param) {
        const value = this.parameters[param.name] !== undefined 
      ? this.parameters[param.name] 
      : param.defaultValue;
      
        const required = param.required ? 'required' : '';
      const requiredLabel = param.required ? '<span class="text-danger">*</span>' : '';
        const inputId = `param-${param.name}-${this.id}`;
     
        // Check if value contains variables
        const hasVariables = typeof value === 'string' && value.includes('{{');
     const inputClass = hasVariables ? 'has-variables' : '';
        
 let fieldHtml = `
<div class="mb-3">
     <label class="form-label small fw-bold">
${param.label} ${requiredLabel}
        </label>
        `;
        
        switch (param.type) {
            case 'Text':
   fieldHtml += `
       <input type="text" 
            id="${inputId}"
 class="form-control form-control-sm ${inputClass}" 
   value="${value || ''}"
  placeholder="${param.placeholder || ''}"
          ${required}
        onchange="window.designerInstance.updateNodeParameter('${this.id}', '${param.name}', this.value)" />
             `;
         break;
            
            case 'TextArea':
    fieldHtml += `
          <textarea id="${inputId}"
     class="form-control form-control-sm ${inputClass}" 
               rows="3"
            placeholder="${param.placeholder || ''}"
         ${required}
     onchange="window.designerInstance.updateNodeParameter('${this.id}', '${param.name}', this.value)">${value || ''}</textarea>
 `;
  break;
  
      case 'Number':
   fieldHtml += `
               <input type="number" 
            id="${inputId}"
   class="form-control form-control-sm" 
   value="${value !== undefined ? value : ''}"
      min="${param.minValue !== undefined ? param.minValue : ''}"
    max="${param.maxValue !== undefined ? param.maxValue : ''}"
   step="${param.type === 'Number' ? '0.1' : '1'}"
      ${required}
         onchange="window.designerInstance.updateNodeParameter('${this.id}', '${param.name}', parseFloat(this.value))" />
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
      onchange="window.designerInstance.updateNodeParameter('${this.id}', '${param.name}', this.checked)" />
              <label class="form-check-label small" for="${inputId}">
        ${param.description || 'Enable this option'}
        </label>
     </div>
      `;
      break;
            
 case 'Select':
    fieldHtml += `<select id="${inputId}" class="form-select form-select-sm" 
 ${required}
              onchange="window.designerInstance.updateNodeParameter('${this.id}', '${param.name}', this.value)">`;
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
         <textarea id="${inputId}" 
              class="form-control form-control-sm font-monospace ${inputClass}" 
               rows="4"
 placeholder="${param.placeholder || '{}'}"
              ${required}
     onchange="window.designerInstance.updateNodeParameterJson('${this.id}', '${param.name}', this.value)">${value ? JSON.stringify(value, null, 2) : ''}</textarea>
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
     onchange="window.designerInstance.updateNodeParameter('${this.id}', '${param.name}', this.value)" />
    `;
      }
        
        if (param.description && param.type !== 'Boolean') {
        fieldHtml += `<small class="form-text text-muted">${param.description}</small>`;
        }
        
        // Show available variables hint
      if ((param.type === 'Text' || param.type === 'TextArea' || param.type === 'Json') && 
        workflow && workflow.variables && Object.keys(workflow.variables).length > 0) {
 const count = Object.keys(workflow.variables).length;
        fieldHtml += `<small class="form-text text-warning">
    <i class="bi bi-lightbulb"></i> Type <code>{{</code> to see ${count} available variable${count > 1 ? 's' : ''}
     </small>`;
        }
  
        fieldHtml += '</div>';
        
        // Setup autocomplete after rendering (will be called after DOM update)
    setTimeout(() => {
            const input = document.getElementById(inputId);
       if (input && (param.type === 'Text' || param.type === 'TextArea' || param.type === 'Json')) {
                setupVariableAutocomplete(input, inputId);
     }
        }, 0);
        
        return fieldHtml;
}

    /**
     * Update parameter value
     * @param {string} paramName
     * @param {any} value
     */
    updateParameter(paramName, value) {
        this.parameters[paramName] = value;
    }

  /**
* Serialize to JSON
     * @returns {object}
     */
    toJSON() {
        return {
 id: this.id,
       name: this.name,
         type: this.type,
            category: this.category,
            parameters: this.parameters,
        position: this.position,
            color: this.color,
  executionOptions: this.executionOptions
 };
    }

    /**
     * Deserialize from JSON (must be implemented by subclass)
     * @param {object} json
     * @returns {BaseNode}
     */
    static fromJSON(json) {
      throw new Error('fromJSON() must be implemented by subclass');
    }

    /**
     * Clone this node
     * @returns {BaseNode}
     */
    clone() {
        const json = this.toJSON();
        json.id = generateGuid();
        return this.constructor.fromJSON(json);
    }
}
