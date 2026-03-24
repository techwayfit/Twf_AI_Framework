// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Variable Autocomplete
// ???????????????????????????????????????????????????????????????????????????

function setupVariableAutocomplete(inputElement, inputId) {
    inputElement.addEventListener('input', (e) => {
        const value = e.target.value;
        const cursorPos = e.target.selectionStart;
  
        // Check if we just typed {{
        const textBeforeCursor = value.substring(0, cursorPos);
      if (textBeforeCursor.endsWith('{{')) {
 showVariableAutocomplete(inputElement, inputId);
        } else {
   hideVariableAutocomplete();
        }
        
        // Visual feedback
    checkForVariables(inputElement);
    });
    
    inputElement.addEventListener('keydown', (e) => {
        if (activeAutocomplete) {
     if (e.key === 'ArrowDown') {
      e.preventDefault();
    navigateAutocomplete(1);
         } else if (e.key === 'ArrowUp') {
                e.preventDefault();
        navigateAutocomplete(-1);
     } else if (e.key === 'Enter' || e.key === 'Tab') {
    e.preventDefault();
       selectAutocompleteItem();
         } else if (e.key === 'Escape') {
    hideVariableAutocomplete();
     }
        }
    });
    
    inputElement.addEventListener('blur', () => {
   // Delay to allow click on autocomplete
  setTimeout(() => hideVariableAutocomplete(), 200);
    });
}

function showVariableAutocomplete(inputElement, inputId) {
    if (!workflow.variables || Object.keys(workflow.variables).length === 0) {
        return;
    }
    
 hideVariableAutocomplete();
    
    const dropdown = document.createElement('div');
    dropdown.className = 'variable-autocomplete';
    dropdown.id = 'var-autocomplete';
    
    let html = '';
  let index = 0;
    Object.entries(workflow.variables).forEach(([name, value]) => {
  const displayValue = value.length > 30 ? value.substring(0, 30) + '...' : value;
        html += `
     <div class="var-suggestion ${index === 0 ? 'selected' : ''}" 
                 data-var-name="${name}" 
        data-index="${index}"
          onclick="insertVariableFromAutocomplete('${inputId}', '${name}')">
        <span class="var-suggestion-name">{{${name}}}</span>
    <span class="var-suggestion-value">${displayValue}</span>
            </div>
        `;
        index++;
    });
    
    dropdown.innerHTML = html;
    
    // Position dropdown
    const rect = inputElement.getBoundingClientRect();
    dropdown.style.position = 'fixed';
    dropdown.style.left = rect.left + 'px';
  dropdown.style.top = (rect.bottom + 4) + 'px';
    dropdown.style.width = rect.width + 'px';
    
    document.body.appendChild(dropdown);
    activeAutocomplete = { inputElement, inputId, selectedIndex: 0, dropdown };
}

function navigateAutocomplete(direction) {
    if (!activeAutocomplete) return;
    
    const suggestions = activeAutocomplete.dropdown.querySelectorAll('.var-suggestion');
    if (suggestions.length === 0) return;
    
 suggestions[activeAutocomplete.selectedIndex].classList.remove('selected');
    
    activeAutocomplete.selectedIndex += direction;
    if (activeAutocomplete.selectedIndex < 0) {
        activeAutocomplete.selectedIndex = suggestions.length - 1;
    } else if (activeAutocomplete.selectedIndex >= suggestions.length) {
        activeAutocomplete.selectedIndex = 0;
    }
    
    suggestions[activeAutocomplete.selectedIndex].classList.add('selected');
    suggestions[activeAutocomplete.selectedIndex].scrollIntoView({ block: 'nearest' });
}

function selectAutocompleteItem() {
    if (!activeAutocomplete) return;
    
    const selected = activeAutocomplete.dropdown.querySelector('.var-suggestion.selected');
    if (selected) {
      const varName = selected.dataset.varName;
        insertVariableFromAutocomplete(activeAutocomplete.inputId, varName);
    }
}

function insertVariableFromAutocomplete(inputId, varName) {
    const input = document.getElementById(inputId);
    if (!input) return;

    const cursorPos = input.selectionStart;
    const value = input.value;
  
    // Find the {{ before cursor
    const textBefore = value.substring(0, cursorPos);
    const lastBracePos = textBefore.lastIndexOf('{{');
    
    if (lastBracePos === -1) return;
    
    // Replace {{ with {{varName}}
    const newValue = value.substring(0, lastBracePos) + 
                  `{{${varName}}}` + 
     value.substring(cursorPos);
    
    input.value = newValue;
    input.setSelectionRange(lastBracePos + varName.length + 4, lastBracePos + varName.length + 4);
    
    // Trigger change event
    input.dispatchEvent(new Event('change'));
    checkForVariables(input);
    
    hideVariableAutocomplete();
    input.focus();
}

function hideVariableAutocomplete() {
 if (activeAutocomplete) {
    activeAutocomplete.dropdown.remove();
        activeAutocomplete = null;
    }
}

function checkForVariables(inputElement) {
    const value = inputElement.value;
    const hasVariables = value && value.includes('{{');
    
    if (hasVariables) {
        inputElement.classList.add('has-variables');
    } else {
        inputElement.classList.remove('has-variables');
    }
}

function getAvailableVariables() {
 if (!workflow.variables) return [];
    return Object.keys(workflow.variables).map(name => `{{${name}}}`);
}
