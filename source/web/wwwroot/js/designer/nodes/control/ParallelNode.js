/**
 * Parallel Node
 * Executes multiple branches simultaneously and merges results
 * Similar to Promise.all() or C# Workflow.Parallel()
 */
class ParallelNode extends BaseNode {
    constructor(id, name = 'Parallel') {
        super(id, name, 'ParallelNode', NODE_CATEGORIES.CONTROL);
  
   this.parameters = {
            branchCount: 3,
 mergeStrategy: 'overwrite'
        };
    
        // Sub-workflows for each branch
        this.subWorkflows = {
            branch1: { nodes: [], connections: [], variables: {} },
      branch2: { nodes: [], connections: [], variables: {} },
            branch3: { nodes: [], connections: [], variables: {} }
        };
        
        this.isExpanded = false;
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
     description: 'Input data for all parallel branches' 
      }
        ];
    }

    /**
     * Get output port definitions
     * @returns {Array}
     */
    getOutputPorts() {
        const branchCount = this.parameters.branchCount || 3;
        const ports = [];
 
        // Add branch ports
        for (let i = 1; i <= branchCount; i++) {
            ports.push({
            id: `branch${i}`,
 label: `Branch ${i}`,
         type: 'control',
       description: `Parallel execution branch ${i} (sub-workflow)`
  });
        }
        
  // Add completed port
        ports.push({
  id: 'completed',
      label: 'After All',
            type: 'data',
    description: 'Executes after all branches complete (merged results)'
        });
        
        return ports;
    }

    /**
     * Get node color
     * @returns {string}
     */
    getDefaultColor() {
        return '#9b59b6';  // Purple for parallel execution
    }

    /**
     * Validate node configuration
     * @returns {object}
     */
validate() {
        const errors = [];
        const warnings = [];

        // Check if branches have content
        const branchCount = this.parameters.branchCount || 3;
        for (let i = 1; i <= branchCount; i++) {
        const branchKey = `branch${i}`;
    if (!this.subWorkflows[branchKey] || 
    !this.subWorkflows[branchKey].nodes || 
this.subWorkflows[branchKey].nodes.length === 0) {
        warnings.push(`Branch ${i} is empty`);
            }
        }

        // Check if completed port is connected
        if (!this.hasOutgoingConnections('completed')) {
            warnings.push('Completed port should be connected to process merged results');
        }

      return {
 isValid: errors.length === 0,
 errors,
            warnings
     };
    }

    /**
 * Open sub-workflow editor for a specific branch
     * @param {string} branchId - e.g., 'branch1', 'branch2', 'branch3'
     */
    openSubWorkflowEditor(branchId) {
        if (!this.subWorkflows[branchId]) {
 this.subWorkflows[branchId] = { nodes: [], connections: [], variables: {} };
        }
        
        if (window.SubWorkflowEditor) {
       window.SubWorkflowEditor.open(this, branchId);
   } else {
            console.error('SubWorkflowEditor not available');
    }
    }

    /**
     * Render properties panel content
     * @returns {string} HTML string
     */
    renderProperties() {
        const baseProperties = super.renderProperties();
 
        // Add branch editors
        const branchCount = this.parameters.branchCount || 3;
        let branchEditorsHtml = `
<hr class="mt-4" />
    <h6 class="small fw-bold mb-3">
    <i class="bi bi-diagram-3"></i> Parallel Branches
            </h6>
 <p class="small text-muted">Define the sub-workflow for each parallel branch.</p>
        `;
        
 for (let i = 1; i <= branchCount; i++) {
       const branchKey = `branch${i}`;
         const branchWorkflow = this.subWorkflows[branchKey] || { nodes: [], connections: [] };
     const nodeCount = branchWorkflow.nodes?.length || 0;
         const connCount = branchWorkflow.connections?.length || 0;
            
    branchEditorsHtml += `
                <div class="mb-3">
     <button class="btn btn-outline-primary btn-sm w-100" 
               onclick="window.designerInstance.editSubWorkflow('${this.id}', '${branchKey}')">
      <i class="bi bi-pencil-square"></i> Edit Branch ${i}
          ${nodeCount > 0 ? `<span class="badge bg-secondary ms-2">${nodeCount} nodes, ${connCount} connections</span>` : ''}
             </button>
      </div>
    `;
        }
        
        // Insert before delete button
        return baseProperties.replace(
            '<hr class="mt-4" />',
        branchEditorsHtml + '<hr class="mt-4" />'
        );
    }

    /**
     * Serialize to JSON
     * @returns {object}
     */
    toJSON() {
        return {
            ...super.toJSON(),
       subWorkflows: this.subWorkflows,
 isExpanded: this.isExpanded
        };
    }

    /**
     * Deserialize from JSON
     * @param {object} json
     * @returns {ParallelNode}
     */
    static fromJSON(json) {
   const node = new ParallelNode(json.id, json.name);
  node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        node.subWorkflows = json.subWorkflows || node.subWorkflows;
        node.isExpanded = json.isExpanded || false;
        return node;
    }
}

nodeRegistry.register('ParallelNode', ParallelNode);
