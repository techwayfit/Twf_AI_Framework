/**
 * ConditionEditor - Visual editor for ConditionNode conditions
 * 
 * Renders a list of named conditions with add/edit/delete functionality.
 * Each condition has a name (output port ID) and an expression.
 */
class ConditionEditor {
    /**
     * Render condition editor UI
     * @param {Object} conditions - Dictionary of { conditionName: expression }
  * @param {Array<string>} availableVariables - Variable names for autocomplete
     * @param {Function} onChange - Callback when conditions change: (newConditions) => void
     * @param {string} containerId - DOM element ID to render into
     * @returns {string} HTML string
     */
    static render(conditions = {}, availableVariables = [], onChange, containerId) {
    const conditionEntries = Object.entries(conditions);

        const html = `
            <div class="condition-editor" id="${containerId}">
        <div class="condition-list">
            ${conditionEntries.length === 0 
             ? '<p class="no-conditions-message">No conditions defined. Click "Add Condition" to create one.</p>'
   : conditionEntries.map(([name, expr], index) => 
 this._renderConditionItem(name, expr, index, availableVariables, containerId)
      ).join('')
        }
          </div>
    
        <button class="add-condition-btn" type="button" 
        onclick="ConditionEditor.addCondition('${containerId}')">
 <span>?</span> Add Condition
</button>
       
     ${availableVariables.length > 0 ? `
  <div class="available-variables">
       <small><strong>Available Variables:</strong> ${availableVariables.join(', ')}</small>
        </div>
        ` : ''}
    
     <div class="condition-help">
           <details>
  <summary>Expression Syntax Help</summary>
               <div class="help-content">
        <p><strong>Operators:</strong> ==, !=, &gt;, &lt;, &gt;=, &lt;=, &&, ||</p>
           <p><strong>Examples:</strong></p>
       <ul>
    ${ExpressionValidator.getExamples()
            .map(ex => `<li><code>${ex.expression}</code> - ${ex.description}</li>`)
  .join('')
          }
        </ul>
  </div>
  </details>
         </div>
            </div>
  `;

        // Store callback for later use
        if (!window._conditionEditorCallbacks) {
         window._conditionEditorCallbacks = {};
        }
     window._conditionEditorCallbacks[containerId] = { onChange, conditions, availableVariables };

        return html;
    }

    /**
     * Render a single condition item
     * @private
     */
    static _renderConditionItem(name, expression, index, availableVariables, containerId) {
        // Validate expression
        const validation = ExpressionValidator.validate(expression, availableVariables);
        const isValid = validation.valid;
        const errorClass = isValid ? '' : 'invalid';

        return `
            <div class="condition-item ${errorClass}" data-condition-name="${name}">
     <input type="text" 
         class="condition-name-input"
           value="${this._escapeHtml(name)}"
        placeholder="condition_name"
        data-original-name="${name}"
     onchange="ConditionEditor.updateConditionName('${containerId}', '${name}', this.value)"
           pattern="[a-zA-Z_][a-zA-Z0-9_]*"
     title="Must start with letter or underscore, followed by letters, numbers, or underscores"
   maxlength="50" />
    
        <div class="condition-expression-wrapper">
      <input type="text" 
        class="condition-expression-input ${errorClass}"
       id="expr-${containerId}-${name}"
           value="${this._escapeHtml(expression)}"
  placeholder="e.g., priority > 7"
       data-condition-name="${name}"
          onblur="ConditionEditor.updateConditionExpression('${containerId}', '${name}', this.value)"
               onkeydown="ConditionEditor.handleExpressionKeydown(event, '${containerId}', '${name}')"
      autocomplete="off" />
     
     ${!isValid ? `<div class="condition-error">${validation.error}</div>` : ''}
          </div>
        
    <button class="condition-delete-btn" 
                 type="button"
    onclick="ConditionEditor.removeCondition('${containerId}', '${name}')"
       title="Delete condition">
    ???
        </button>
            </div>
        `;
    }

    /**
     * Add a new condition
     */
    static addCondition(containerId) {
  const state = window._conditionEditorCallbacks[containerId];
   if (!state) return;

   const { conditions, onChange, availableVariables } = state;

  // Generate unique name
        let counter = 1;
        let newName = 'condition_1';
        while (conditions[newName]) {
            counter++;
    newName = `condition_${counter}`;
    }

     // Add new condition
     const newConditions = { ...conditions, [newName]: '' };
        
     // Update state
    state.conditions = newConditions;

        // Trigger onChange
        if (onChange) {
            onChange(newConditions);
        }

        // Re-render
      this._updateEditor(containerId);

        // Focus on new expression input
        setTimeout(() => {
            const editor = document.getElementById(containerId);
            const newInput = editor?.querySelector(`input.condition-expression-input[data-condition-name="${newName}"]`);
       if (newInput) {
          newInput.focus();
     }
        }, 50);
    }

    /**
     * Remove a condition
     */
    static removeCondition(containerId, conditionName) {
        const state = window._conditionEditorCallbacks[containerId];
        if (!state) return;

        const { conditions, onChange } = state;

  // Check if there are connections using this condition's port
 let affectedConnections = [];
        if (window.ConnectionValidator && window.workflow) {
  affectedConnections = ConnectionValidator.getAffectedConnectionsByConditionChange(
      window.selectedNode?.id,
 [conditionName],
      window.workflow
            );
        }

      // Build confirmation message
        let confirmMessage = `Delete condition "${conditionName}"?`;
  if (affectedConnections.length > 0) {
            confirmMessage += `\n\nThis will also remove ${affectedConnections.length} connection(s) using this port.`;
        }

  // Confirm deletion
   if (!confirm(confirmMessage)) {
    return;
        }

        // Remove condition
        const newConditions = { ...conditions };
   delete newConditions[conditionName];

        // Update state
        state.conditions = newConditions;

  // Trigger onChange
   if (onChange) {
            onChange(newConditions);
    }

        // Remove affected connections if any
        if (affectedConnections.length > 0 && window.workflow) {
            affectedConnections.forEach(conn => {
   const index = window.workflow.connections.findIndex(c => c.id === conn.id);
       if (index !== -1) {
       window.workflow.connections.splice(index, 1);
          console.log(`Removed connection ${conn.id} due to deleted condition "${conditionName}"`);
}
   });
        }

        // Re-render
     this._updateEditor(containerId);
    }

  /**
     * Update condition name (rename)
     */
    static updateConditionName(containerId, oldName, newName) {
  const state = window._conditionEditorCallbacks[containerId];
        if (!state) return;

        const { conditions, onChange } = state;

    // Validate name
        newName = newName.trim();
        if (!newName) {
        alert('Condition name cannot be empty');
       return;
     }

     if (!/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(newName)) {
    alert('Condition name must start with a letter or underscore, followed by letters, numbers, or underscores');
      return;
     }

if (newName !== oldName && conditions[newName]) {
            alert(`Condition "${newName}" already exists`);
            return;
    }

        if (newName === oldName) return; // No change

        // Rename condition (preserve order)
      const newConditions = {};
  for (const [key, value] of Object.entries(conditions)) {
     if (key === oldName) {
    newConditions[newName] = value;
        } else {
         newConditions[key] = value;
  }
        }

        // Update state
        state.conditions = newConditions;

        // Trigger onChange
        if (onChange) {
       onChange(newConditions);
        }

        // Re-render
        this._updateEditor(containerId);
    }

    /**
     * Update condition expression
     */
    static updateConditionExpression(containerId, conditionName, newExpression) {
      const state = window._conditionEditorCallbacks[containerId];
        if (!state) return;

        const { conditions, onChange } = state;

  // Update expression
   const newConditions = { ...conditions, [conditionName]: newExpression.trim() };

      // Update state
        state.conditions = newConditions;

     // Trigger onChange
        if (onChange) {
      onChange(newConditions);
        }

        // Re-render to show validation
        this._updateEditor(containerId);
    }

    /**
     * Handle keydown in expression input
     */
    static handleExpressionKeydown(event, containerId, conditionName) {
        if (event.key === 'Enter') {
         event.preventDefault();
 this.updateConditionExpression(containerId, conditionName, event.target.value);
            
         // Add new condition on Enter
  this.addCondition(containerId);
        }
    }

    /**
     * Re-render the editor (internal)
     * @private
     */
    static _updateEditor(containerId) {
        const state = window._conditionEditorCallbacks[containerId];
        if (!state) return;

        const { conditions, availableVariables } = state;

  const editor = document.getElementById(containerId);
if (!editor) return;

        // Re-render (keep callback intact)
        const newHtml = this.render(conditions, availableVariables, state.onChange, containerId);
        
        // Create temporary container
        const temp = document.createElement('div');
   temp.innerHTML = newHtml;
        
    // Replace content
        editor.parentNode.replaceChild(temp.firstElementChild, editor);

        // Re-attach autocomplete to all expression inputs
        this._attachAutocompleteToInputs(containerId, availableVariables);
  }

    /**
     * Attach autocomplete to all expression inputs
     * @private
     */
    static _attachAutocompleteToInputs(containerId, availableVariables) {
        if (!window.VariableAutocomplete) return;

        const editor = document.getElementById(containerId);
        if (!editor) return;

      const inputs = editor.querySelectorAll('.condition-expression-input');
        inputs.forEach(input => {
          VariableAutocomplete.attach(
       input,
              availableVariables,
    (variableName) => {
    console.log(`Autocomplete selected: ${variableName}`);
         }
            );
        });
    }

    /**
* Escape HTML to prevent XSS
     * @private
     */
    static _escapeHtml(text) {
        const div = document.createElement('div');
    div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Clean up when editor is removed
     */
    static dispose(containerId) {
      if (window._conditionEditorCallbacks) {
     delete window._conditionEditorCallbacks[containerId];
     }
    }
}

// Make available globally
window.ConditionEditor = ConditionEditor;
