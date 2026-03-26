/**
 * Execution Options Editor Component
 * Renders and manages node execution options (retry, timeout, error handling)
 * Phase 4: Node Execution Options
 */
class ExecutionOptionsEditor {
    /**
     * Render execution options section
     * @param {BaseNode} node - The node to render options for
     * @param {Array} optionsSchema - Execution option definitions from node schema
     * @returns {string} HTML string
  */
    static render(node, optionsSchema) {
   if (!optionsSchema || optionsSchema.length === 0) {
            return ''; // No execution options to display
     }

        // Initialize executionOptions if not present
  if (!node.executionOptions) {
 node.executionOptions = {};
 }

     let html = `
            <div class="execution-options-section">
         <div class="section-header" onclick="ExecutionOptionsEditor.toggleSection('${node.id}')">
           <h6 class="small fw-bold mb-0">
  <i class="bi bi-gear-fill"></i> Execution Options
   <i class="bi bi-chevron-down toggle-icon" id="exec-toggle-${node.id}"></i>
     </h6>
       </div>
                <div class="section-content" id="exec-options-${node.id}" style="display: none;">
        <div class="alert alert-info alert-sm mb-3">
            <i class="bi bi-info-circle"></i> Configure retry, timeout, and error handling behavior
           </div>
        `;

        // Render each execution option
        optionsSchema.forEach(option => {
  html += this.renderOption(node, option);
        });

        html += `
    </div>
          </div>
        `;

        return html;
    }

    /**
     * Render a single execution option field
     * @param {BaseNode} node - The node
     * @param {object} option - Execution option definition
     * @returns {string} HTML string
     */
    static renderOption(node, option) {
        const value = node.executionOptions[option.name] !== undefined 
     ? node.executionOptions[option.name] 
         : option.defaultValue;

  const inputId = `exec-opt-${option.name}-${node.id}`;
    
   let fieldHtml = `
    <div class="mb-3">
        <label class="form-label small fw-bold">
      ${option.label}
      </label>
        `;

        switch (option.type) {
            case 'Number':
           fieldHtml += `
    <input type="number" 
         id="${inputId}"
    class="form-control form-control-sm" 
             value="${value !== undefined ? value : ''}"
    min="${option.minValue !== undefined ? option.minValue : ''}"
   max="${option.maxValue !== undefined ? option.maxValue : ''}"
     step="1"
      onchange="ExecutionOptionsEditor.updateOption('${node.id}', '${option.name}', parseInt(this.value, 10))" />
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
     onchange="ExecutionOptionsEditor.updateOption('${node.id}', '${option.name}', this.checked)" />
          <label class="form-check-label small" for="${inputId}">
     ${option.description || 'Enable this option'}
  </label>
   </div>
          `;
      break;

            default:
       fieldHtml += `
    <input type="text" 
     id="${inputId}"
   class="form-control form-control-sm" 
  value="${value || ''}"
           onchange="ExecutionOptionsEditor.updateOption('${node.id}', '${option.name}', this.value)" />
        `;
        }

        if (option.description && option.type !== 'Boolean') {
            fieldHtml += `<small class="form-text text-muted">${option.description}</small>`;
      }

   fieldHtml += '</div>';

        return fieldHtml;
    }

    /**
     * Update execution option value
     * @param {string} nodeId - Node ID
     * @param {string} optionName - Option name
     * @param {any} value - New value
     */
    static updateOption(nodeId, optionName, value) {
        if (!window.designerInstance) {
            console.error('Designer instance not found');
   return;
        }

        const node = window.designerInstance.getNode(nodeId);
  if (!node) {
            console.error(`Node ${nodeId} not found`);
         return;
        }

      // Initialize executionOptions if not present
   if (!node.executionOptions) {
  node.executionOptions = {};
        }

 // Update the value
        node.executionOptions[optionName] = value;

        // Mark workflow as modified
     if (window.designerInstance.markModified) {
       window.designerInstance.markModified();
   }

        console.log(`Updated execution option: ${node.name}.${optionName} = ${value}`);
    }

    /**
     * Toggle section visibility
     * @param {string} nodeId - Node ID
     */
    static toggleSection(nodeId) {
        const content = document.getElementById(`exec-options-${nodeId}`);
        const icon = document.getElementById(`exec-toggle-${nodeId}`);
        
 if (!content || !icon) return;

        if (content.style.display === 'none') {
            content.style.display = 'block';
            icon.classList.remove('bi-chevron-down');
 icon.classList.add('bi-chevron-up');
        } else {
   content.style.display = 'none';
       icon.classList.remove('bi-chevron-up');
     icon.classList.add('bi-chevron-down');
        }
    }

    /**
     * Get default execution options from schema
     * @param {Array} optionsSchema - Execution option definitions
     * @returns {object} Default values
     */
    static getDefaults(optionsSchema) {
const defaults = {};
        
        if (optionsSchema) {
        optionsSchema.forEach(option => {
          defaults[option.name] = option.defaultValue;
 });
        }

        return defaults;
    }

    /**
     * Validate execution options
     * @param {object} executionOptions - Current execution options
     * @param {Array} optionsSchema - Execution option definitions
     * @returns {{isValid: boolean, errors: Array<string>}}
     */
    static validate(executionOptions, optionsSchema) {
    const errors = [];

        if (!executionOptions || !optionsSchema) {
        return { isValid: true, errors };
     }

     optionsSchema.forEach(option => {
            const value = executionOptions[option.name];

            // Check number ranges
    if (option.type === 'Number' && value !== undefined) {
     if (option.minValue !== undefined && value < option.minValue) {
            errors.push(`${option.label} must be at least ${option.minValue}`);
              }
    if (option.maxValue !== undefined && value > option.maxValue) {
       errors.push(`${option.label} must be at most ${option.maxValue}`);
                }
     }
        });

        return {
            isValid: errors.length === 0,
      errors
        };
    }

    /**
     * Get summary of configured execution options
     * @param {object} executionOptions - Current execution options
     * @param {Array} optionsSchema - Execution option definitions
     * @returns {string} Summary text
     */
    static getSummary(executionOptions, optionsSchema) {
        if (!executionOptions || !optionsSchema) {
            return 'Default settings';
        }

   const summaryParts = [];

        // Check for retry configuration
        const maxRetries = executionOptions.maxRetries;
    if (maxRetries > 0) {
            summaryParts.push(`${maxRetries} retries`);
     }

        // Check for timeout configuration
        const timeout = executionOptions.timeoutMs;
        if (timeout) {
          summaryParts.push(`${timeout}ms timeout`);
        }

        // Check for error handling
        const continueOnError = executionOptions.continueOnError;
        if (continueOnError) {
            summaryParts.push('Continue on error');
        }

        return summaryParts.length > 0 ? summaryParts.join(', ') : 'Default settings';
    }
}
