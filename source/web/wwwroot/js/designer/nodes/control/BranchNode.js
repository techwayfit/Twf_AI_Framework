/**
 * Branch Node (Switch/Case)
 * Routes workflow to different paths based on value matching
 * Like a switch/case statement or pattern matching
 */
class BranchNode extends BaseNode {
    constructor(id, name = 'Branch') {
 super(id, name, 'BranchNode', NODE_CATEGORIES.CONTROL);
        
        this.parameters = {
        valueKey: '',
            case1Value: '',
          case2Value: '',
    case3Value: '',
         caseSensitive: false
        };
    }

    /**
     * Get input port definitions
* @returns {Array}
     */
    getInputPorts() {
        return [
      { 
       id: 'input', 
    label: 'Input', 
     type: 'data', 
     required: true,
                description: 'Input value to evaluate' 
            }
    ];
    }

    /**
     * Get output port definitions
     * @returns {Array}
     */
    getOutputPorts() {
 return [
            {
      id: 'case1',
 label: this.getCaseLabel(1),
                type: 'conditional',
      condition: 'case1',
    description: 'Matches first case value'
        },
            {
          id: 'case2',
     label: this.getCaseLabel(2),
    type: 'conditional',
  condition: 'case2',
                description: 'Matches second case value'
          },
            {
      id: 'case3',
     label: this.getCaseLabel(3),
     type: 'conditional',
        condition: 'case3',
         description: 'Matches third case value'
       },
            {
                id: 'default',
   label: 'Default',
     type: 'conditional',
         condition: 'default',
          description: 'No case matches (fallback)'
  }
        ];
    }

    /**
     * Get label for a case based on its value
   * @param {number} caseNum - Case number (1, 2, or 3)
     * @returns {string}
     */
    getCaseLabel(caseNum) {
        const value = this.parameters[`case${caseNum}Value`];
        if (value && value.trim() !== '') {
            return `Case: "${value}"`;
}
        return `Case ${caseNum}`;
    }

    /**
     * Get node color
     * @returns {string}
     */
    getDefaultColor() {
        return '#e67e22';  // Orange (between ConditionNode and other control)
    }

    /**
     * Validate node configuration
     * @returns {object}
     */
    validate() {
        const errors = [];
        const warnings = [];

        // Check if valueKey is specified
     if (!this.parameters.valueKey || this.parameters.valueKey.trim() === '') {
 errors.push('Value Key is required');
        }

        // Check if at least one case is defined
        const hasAnyCase = this.parameters.case1Value || 
    this.parameters.case2Value || 
     this.parameters.case3Value;
     if (!hasAnyCase) {
   warnings.push('No case values defined - all will go to default');
      }

  // Check for duplicate case values
        const cases = [
       this.parameters.case1Value,
         this.parameters.case2Value,
    this.parameters.case3Value
      ].filter(v => v && v.trim() !== '');

        const uniqueCases = new Set(
     this.parameters.caseSensitive 
    ? cases 
                : cases.map(c => c.toLowerCase())
        );

        if (uniqueCases.size < cases.length) {
          warnings.push('Duplicate case values detected');
        }

 // Check if default is connected
     if (!this.hasOutgoingConnections('default')) {
          warnings.push('Default case should be connected as fallback');
        }

   return {
            isValid: errors.length === 0,
    errors,
            warnings
      };
    }

    /**
     * Check if a port has outgoing connections
     * @param {string} portId
     * @returns {boolean}
     */
    hasOutgoingConnections(portId) {
        if (!workflow || !workflow.connections) return false;
        return workflow.connections.some(conn => 
            conn.sourceNodeId === this.id && conn.sourcePort === portId
 );
    }

    /**
     * Render properties panel content
     * @returns {string} HTML string
   */
    renderProperties() {
        let html = `
            <h6 class="border-bottom pb-2 mb-3">
         <i class="bi bi-diagram-3"></i> ${this.name}
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
        <h6 class="small fw-bold mb-3">
       <i class="bi bi-shuffle"></i> Switch/Case Configuration
     </h6>
  <p class="small text-muted">Route based on value matching (like switch/case)</p>
        `;

        // Render each parameter
        const schema = this.getSchema();
      if (schema.parameters) {
   schema.parameters.forEach(param => {
          html += this.renderParameter(param);
            });
        }

        // Add case summary
  html += this.renderCaseSummary();

        // Render execution options
  if (schema.executionOptions && schema.executionOptions.length > 0) {
  html += '<hr />';
         html += ExecutionOptionsEditor.render(this, schema.executionOptions);
      }

        // Add delete button
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
     * Render a visual summary of the cases
     * @returns {string} HTML string
     */
    renderCaseSummary() {
        const valueKey = this.parameters.valueKey || '(not set)';
   const case1 = this.parameters.case1Value || '(empty)';
        const case2 = this.parameters.case2Value || '(empty)';
   const case3 = this.parameters.case3Value || '(empty)';
        const caseSensitive = this.parameters.caseSensitive ? 'Yes' : 'No';

        return `
            <div class="card bg-light mt-3">
         <div class="card-body p-3">
        <h6 class="small fw-bold mb-2">
    <i class="bi bi-info-circle"></i> Routing Logic
       </h6>
   <p class="small mb-2">
     <strong>Value from:</strong> <code>${valueKey}</code><br>
        <strong>Case Sensitive:</strong> ${caseSensitive}
      </p>
             <table class="table table-sm small mb-0">
   <tbody>
           <tr>
      <td><span class="badge bg-primary">Case 1</span></td>
            <td><code>"${case1}"</code></td>
          <td>? case1 port</td>
           </tr>
        <tr>
         <td><span class="badge bg-primary">Case 2</span></td>
        <td><code>"${case2}"</code></td>
    <td>? case2 port</td>
          </tr>
  <tr>
              <td><span class="badge bg-primary">Case 3</span></td>
           <td><code>"${case3}"</code></td>
        <td>? case3 port</td>
  </tr>
            <tr>
                    <td><span class="badge bg-secondary">Default</span></td>
             <td><em>no match</em></td>
               <td>? default port</td>
            </tr>
                 </tbody>
        </table>
   </div>
  </div>
        `;
    }

    /**
     * Serialize to JSON
     * @returns {object}
     */
    toJSON() {
        return {
      ...super.toJSON()
        };
  }

    /**
     * Deserialize from JSON
     * @param {object} json
     * @returns {BranchNode}
     */
    static fromJSON(json) {
 const node = new BranchNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
    node.executionOptions = json.executionOptions;
  return node;
    }
}

nodeRegistry.register('BranchNode', BranchNode);
