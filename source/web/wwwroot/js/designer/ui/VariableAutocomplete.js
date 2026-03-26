/**
 * VariableAutocomplete - Autocomplete dropdown for variable names in condition expressions
 * 
 * Shows workflow variables as suggestions while typing in expression inputs.
 * Supports keyboard navigation and mouse selection.
 */
class VariableAutocomplete {
    /**
     * Attach autocomplete to an expression input element
     * @param {HTMLInputElement} input - The input element to attach to
     * @param {Array<string>} variables - Available variable names
     * @param {Function} onSelect - Callback when variable is selected: (variableName) => void
     */
    static attach(input, variables, onSelect) {
   if (!input || !variables) return;

 // Store state
        input._autocompleteState = {
         variables,
       onSelect,
            dropdown: null,
  selectedIndex: 0
 };

 // Listen for input events
        input.addEventListener('input', (e) => {
         this._handleInput(e.target);
        });

        // Listen for keyboard events
        input.addEventListener('keydown', (e) => {
    this._handleKeydown(e);
        });

        // Hide on blur (with delay for click)
     input.addEventListener('blur', () => {
            setTimeout(() => this.hide(input), 200);
        });
  }

  /**
     * Handle input event - show/update suggestions
     * @private
     */
    static _handleInput(input) {
   const state = input._autocompleteState;
        if (!state) return;

        const value = input.value;
        const cursorPos = input.selectionStart;
 
        // Get word being typed at cursor
        const wordInfo = this._getWordAtCursor(value, cursorPos);
        
if (wordInfo && wordInfo.word.length > 0) {
            // Filter variables that match the current word
          const suggestions = state.variables.filter(v => 
v.toLowerCase().startsWith(wordInfo.word.toLowerCase())
    );

         if (suggestions.length > 0) {
       this.show(input, suggestions, wordInfo);
       return;
   }
        }

        // No suggestions - hide dropdown
        this.hide(input);
    }

    /**
     * Get the word being typed at cursor position
     * @private
     */
    static _getWordAtCursor(text, cursorPos) {
      // Find word boundaries (letters, numbers, underscores)
let start = cursorPos;
        let end = cursorPos;

        // Move start back to beginning of word
        while (start > 0 && /[a-zA-Z0-9_]/.test(text[start - 1])) {
            start--;
     }

        // Move end forward to end of word
        while (end < text.length && /[a-zA-Z0-9_]/.test(text[end])) {
            end++;
        }

        const word = text.substring(start, end);

        // Only show autocomplete if we're typing a variable name
        // (not inside a string literal, not after a number)
        const before = text.substring(0, start);
        const isInString = (before.match(/'/g) || []).length % 2 === 1;
   
   if (isInString) return null;
     if (word.length === 0) return null;
        if (/^\d/.test(word)) return null; // Starts with digit

        return { word, start, end };
    }

    /**
     * Handle keydown event - navigation and selection
     * @private
     */
    static _handleKeydown(event) {
        const input = event.target;
        const state = input._autocompleteState;
        
        if (!state || !state.dropdown) return;

        const items = state.dropdown.querySelectorAll('.autocomplete-item');
        if (items.length === 0) return;

     switch (event.key) {
            case 'ArrowDown':
        event.preventDefault();
      this._navigate(input, 1, items);
     break;

     case 'ArrowUp':
                event.preventDefault();
                this._navigate(input, -1, items);
     break;

            case 'Enter':
       case 'Tab':
   if (state.dropdown.style.display !== 'none') {
      event.preventDefault();
         this._selectCurrent(input, items);
 }
break;

  case 'Escape':
  event.preventDefault();
                this.hide(input);
           break;
        }
    }

    /**
  * Navigate through suggestions
* @private
  */
    static _navigate(input, direction, items) {
        const state = input._autocompleteState;
        if (!state) return;

        // Remove current selection
   items[state.selectedIndex]?.classList.remove('selected');

      // Update index
        state.selectedIndex += direction;
        if (state.selectedIndex < 0) state.selectedIndex = items.length - 1;
     if (state.selectedIndex >= items.length) state.selectedIndex = 0;

        // Add new selection
      items[state.selectedIndex]?.classList.add('selected');
        items[state.selectedIndex]?.scrollIntoView({ block: 'nearest' });
 }

    /**
  * Select current item
     * @private
     */
    static _selectCurrent(input, items) {
        const state = input._autocompleteState;
        if (!state) return;

        const selected = items[state.selectedIndex];
   if (selected) {
      const variableName = selected.dataset.variable;
     this._insertVariable(input, variableName);
        }
    }

    /**
     * Insert variable at cursor position
     * @private
     */
    static _insertVariable(input, variableName) {
        const state = input._autocompleteState;
        if (!state) return;

        const value = input.value;
        const cursorPos = input.selectionStart;
        const wordInfo = this._getWordAtCursor(value, cursorPos);

        if (!wordInfo) return;

     // Replace the current word with the variable name
     const newValue = 
      value.substring(0, wordInfo.start) +
      variableName +
            value.substring(wordInfo.end);

        input.value = newValue;

        // Set cursor after inserted variable
     const newCursorPos = wordInfo.start + variableName.length;
        input.setSelectionRange(newCursorPos, newCursorPos);

  // Trigger change event
        input.dispatchEvent(new Event('change', { bubbles: true }));

   // Call onSelect callback
        if (state.onSelect) {
    state.onSelect(variableName);
        }

        // Hide dropdown
      this.hide(input);

    // Focus back on input
        input.focus();
    }

    /**
     * Show autocomplete dropdown
     */
    static show(input, suggestions, wordInfo) {
const state = input._autocompleteState;
        if (!state) return;

        // Hide existing dropdown
    this.hide(input);

        // Create dropdown
  const dropdown = document.createElement('div');
        dropdown.className = 'variable-autocomplete';
     dropdown.id = `autocomplete-${Date.now()}`;

        // Add suggestions
        suggestions.forEach((variable, index) => {
     const item = document.createElement('div');
          item.className = 'autocomplete-item' + (index === 0 ? ' selected' : '');
   item.dataset.variable = variable;

            // Variable name
   const varSpan = document.createElement('span');
          varSpan.className = 'autocomplete-item-variable';
          varSpan.textContent = variable;

   // Description (if available from workflow)
    const descSpan = document.createElement('div');
   descSpan.className = 'autocomplete-item-description';
            descSpan.textContent = 'Workflow variable';

            item.appendChild(varSpan);
      item.appendChild(descSpan);

         // Click handler
       item.addEventListener('mousedown', (e) => {
      e.preventDefault();
    this._insertVariable(input, variable);
            });

       // Hover handler
    item.addEventListener('mouseenter', () => {
       dropdown.querySelectorAll('.autocomplete-item').forEach(i => 
     i.classList.remove('selected')
       );
    item.classList.add('selected');
 state.selectedIndex = index;
            });

       dropdown.appendChild(item);
    });

        // Position dropdown
        const rect = input.getBoundingClientRect();
        dropdown.style.position = 'fixed';
        dropdown.style.left = rect.left + 'px';
    dropdown.style.top = (rect.bottom + 4) + 'px';
        dropdown.style.width = Math.max(rect.width, 200) + 'px';
    dropdown.style.maxWidth = '400px';

    // Add to document
        document.body.appendChild(dropdown);

      // Store reference
        state.dropdown = dropdown;
   state.selectedIndex = 0;
    }

  /**
     * Hide autocomplete dropdown
     */
  static hide(input) {
    const state = input._autocompleteState;
        if (!state) return;

        if (state.dropdown) {
 state.dropdown.remove();
        state.dropdown = null;
        }
        state.selectedIndex = 0;
  }

    /**
     * Detach autocomplete from input
     */
    static detach(input) {
        if (!input) return;

        this.hide(input);
 delete input._autocompleteState;
    }

/**
     * Update available variables
  */
    static updateVariables(input, variables) {
        const state = input._autocompleteState;
        if (!state) return;

        state.variables = variables || [];
     
      // Re-trigger input event to update dropdown
    this._handleInput(input);
    }
}

// Make available globally
window.VariableAutocomplete = VariableAutocomplete;
