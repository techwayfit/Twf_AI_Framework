/**
 * ExpressionValidator - Validates condition expressions for ConditionNode
 * 
 * Supports simple comparison expressions with variables, strings, and numbers.
 * Does NOT use eval() for security reasons - uses pattern matching instead.
 */
class ExpressionValidator {
    /**
     * Validate a condition expression
     * @param {string} expression - e.g., "priority > 7" or "sentiment == 'positive'"
     * @param {Array<string>} availableVariables - Variable names that exist in workflow
     * @returns {{ valid: boolean, error?: string, usedVariables?: string[] }}
     */
    static validate(expression, availableVariables = []) {
        if (!expression || typeof expression !== 'string') {
     return { valid: false, error: 'Expression cannot be empty' };
        }

        const trimmed = expression.trim();
        if (trimmed.length === 0) {
      return { valid: false, error: 'Expression cannot be empty' };
        }

        // Check for dangerous characters (prevent script injection)
    if (this._hasDangerousCharacters(trimmed)) {
      return { valid: false, error: 'Expression contains invalid characters (;, <script>, etc.)' };
        }

        // Check for balanced parentheses
    if (!this._hasBalancedParentheses(trimmed)) {
       return { valid: false, error: 'Unbalanced parentheses' };
        }

        // Extract variable references
        const usedVariables = this.extractVariables(trimmed);

  // Check if variables exist in workflow
        const undefinedVars = usedVariables.filter(v => !availableVariables.includes(v));
        if (undefinedVars.length > 0) {
            return {
valid: false,
  error: `Undefined variable(s): ${undefinedVars.join(', ')}`
    };
        }

// Check for at least one comparison operator
        const hasComparison = /(?:==|!=|>=|<=|>|<)/.test(trimmed);
      if (!hasComparison) {
            return {
     valid: false,
                error: 'Expression must contain a comparison operator (==, !=, >, <, >=, <=)'
     };
   }

        // Check for valid string literals (must be in quotes)
        if (!this._hasValidStringLiterals(trimmed)) {
            return {
                valid: false,
         error: 'String values must be in single quotes (e.g., \'value\')'
          };
        }

        return {
          valid: true,
     usedVariables
        };
    }

    /**
 * Extract variable references from expression
  * @param {string} expression
     * @returns {string[]} Unique variable names used
 */
    static extractVariables(expression) {
// Remove string literals first to avoid matching variables inside strings
        const withoutStrings = expression.replace(/'[^']*'/g, '');

    // Match variable names (alphanumeric + underscore, not starting with number)
const variablePattern = /\b([a-zA-Z_][a-zA-Z0-9_]*)\b/g;
        const matches = withoutStrings.matchAll(variablePattern);

        const variables = new Set();
        for (const match of matches) {
 const word = match[1];
  // Exclude JavaScript keywords and common constants
          if (!this._isReservedWord(word)) {
     variables.add(word);
    }
        }

        return Array.from(variables);
    }

    /**
     * Get autocomplete suggestions based on partial expression
     * @param {string} partialExpression - Current expression text
     * @param {Array<string>} availableVariables - Variables to suggest
     * @returns {Array<{text: string, description: string, type: string}>}
     */
    static getSuggestions(partialExpression, availableVariables = []) {
        const suggestions = [];

        // Find the current word being typed (last word in expression)
    const words = partialExpression.split(/[\s()]+/);
        const currentWord = words[words.length - 1] || '';

   // Suggest variables
        if (currentWord.length > 0) {
            availableVariables
           .filter(v => v.toLowerCase().startsWith(currentWord.toLowerCase()))
   .forEach(variable => {
  suggestions.push({
         text: variable,
      description: 'Workflow variable',
         type: 'variable'
        });
       });
}

     // Suggest operators if at operator position
        const needsOperator = /[a-zA-Z0-9_]\s*$/.test(partialExpression);
    if (needsOperator) {
          const operators = [
    { text: '==', description: 'Equal to' },
     { text: '!=', description: 'Not equal to' },
          { text: '>', description: 'Greater than' },
      { text: '<', description: 'Less than' },
          { text: '>=', description: 'Greater than or equal' },
      { text: '<=', description: 'Less than or equal' },
         { text: '&&', description: 'Logical AND' },
      { text: '||', description: 'Logical OR' }
            ];

            operators.forEach(op => {
     suggestions.push({
            text: op.text,
          description: op.description,
        type: 'operator'
      });
            });
        }

        return suggestions;
    }

    /**
  * Check for dangerous characters that could be script injection
     * @private
     */
    static _hasDangerousCharacters(expression) {
        const dangerous = [
  ';',     // Statement separator
  '<script',     // Script tags
       'eval(',       // Eval function
            'function(',   // Function declaration
            '=>',    // Arrow function
   'import',      // Module import
       'require('     // Module require
 ];

        const lower = expression.toLowerCase();
return dangerous.some(d => lower.includes(d));
    }

    /**
     * Check for balanced parentheses
 * @private
     */
    static _hasBalancedParentheses(expression) {
        let depth = 0;
for (const char of expression) {
            if (char === '(') depth++;
            if (char === ')') depth--;
            if (depth < 0) return false; // Closing before opening
   }
        return depth === 0; // All opened parens are closed
    }

    /**
     * Check if string literals are properly quoted
* @private
     */
    static _hasValidStringLiterals(expression) {
        // Remove valid string literals (single-quoted)
    const withoutValidStrings = expression.replace(/'[^']*'/g, '');

        // Check for unquoted strings that look like they should be quoted
      // (sequences of letters after comparison operators)
        const unquotedStringPattern = /(==|!=)\s*([a-zA-Z][a-zA-Z0-9_]*)\b/;
        const match = withoutValidStrings.match(unquotedStringPattern);

    if (match) {
 const word = match[2];
          // If it's not a number and not a known keyword, it should be quoted
      if (isNaN(word) && !this._isReservedWord(word)) {
    return false;
   }
        }

        return true;
    }

    /**
     * Check if word is a reserved word (should not be treated as variable)
     * @private
     */
    static _isReservedWord(word) {
        const reserved = [
            'true', 'false', 'null', 'undefined',
         'and', 'or', 'not',
 'length', 'value' // Common properties
   ];
 return reserved.includes(word.toLowerCase());
    }

    /**
     * Format expression for display (add spacing around operators)
     * @param {string} expression
     * @returns {string}
     */
    static format(expression) {
        if (!expression) return '';

     return expression
 // Add spaces around operators
.replace(/([=!><]+)/g, ' $1 ')
         .replace(/&&/g, ' && ')
         .replace(/\|\|/g, ' || ')
     // Clean up multiple spaces
   .replace(/\s+/g, ' ')
    .trim();
    }

    /**
     * Get example expressions for help text
     * @returns {Array<{expression: string, description: string}>}
     */
    static getExamples() {
        return [
            { expression: "priority > 7", description: "Number comparison" },
            { expression: "status == 'urgent'", description: "String equality" },
         { expression: "score >= 5 && active == true", description: "Multiple conditions" },
    { expression: "category != 'spam'", description: "String inequality" },
    { expression: "(priority > 5 || urgent == true) && status == 'open'", description: "Complex logic" }
        ];
    }
}

// Make available globally
window.ExpressionValidator = ExpressionValidator;
