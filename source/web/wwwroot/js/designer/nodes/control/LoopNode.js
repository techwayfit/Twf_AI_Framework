/**
 * LoopNode - ForEach Container Node
 * Iterates over a collection and processes each item through a sub-workflow
 * Mirrors TwfAiFramework.Core.Workflow.ForEach()
 * 
 * Phase 5: Special Node Types
 */
class LoopNode extends BaseNode {
    /**
     * @param {string} id - Unique node identifier
     * @param {string} name - Display name
     * @param {number} x - X position
   * @param {number} y - Y Position
     */
  constructor(id, name, x = 0, y = 0) {
        super(id, name, 'LoopNode', 'Control');
        this.position = { x, y };
        this.parameters = {
            itemsKey: '',
            outputKey: '',
         loopItemKey: '__loop_item__',
       maxIterations: 0
        };
        this.subWorkflow = {
     nodes: [],
 connections: []
     };
    this.isExpanded = false; // Collapsed by default
        this.color = NODE_COLORS['Control'] || '#f39c12';
}

    /**
     * Get input port definitions
     * @returns {Array}
     */
    getInputPorts() {
        return [
   { id: 'input', label: 'Input', type: 'data', required: true, description: 'Collection to iterate over' }
        ];
    }

    /**
  * Get output port definitions
     * @returns {Array}
     */
    getOutputPorts() {
        return [
          { 
          id: 'iteration', 
    label: 'For Each Item', 
        type: 'control', 
   description: 'Executes for each item (sub-workflow)' 
            },
    { 
           id: 'completed', 
                label: 'After Loop', 
        type: 'data', 
         description: 'Executes after all iterations complete' 
   }
        ];
    }

    /**
     * Render properties panel content
     * @returns {string} HTML string
     */
    renderProperties() {
        const schema = this.getSchema();
        
   let html = `
         <h6 class="border-bottom pb-2 mb-3">
                <i class="bi bi-arrow-repeat"></i> ${this.name}
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
            <h6 class="small fw-bold mb-3">Loop Configuration</h6>
        `;
        
        // Render parameters
     if (schema.parameters) {
    schema.parameters.forEach(param => {
   html += this.renderParameter(param);
       });
   }
        
        // Sub-workflow management
        const nodeCount = this.subWorkflow.nodes?.length || 0;
    const connCount = this.subWorkflow.connections?.length || 0;
     const itemKey = this.parameters.loopItemKey || '__loop_item__';
    
        html += `
        <hr />
   <h6 class="small fw-bold mb-3">
                <i class="bi bi-diagram-3"></i> Sub-Workflow
   </h6>
            
        <div class="alert alert-info small mb-3">
      <i class="bi bi-info-circle"></i>
         The sub-workflow executes once for each item. 
     Current item is available as <code>{{${itemKey}}}</code>
            </div>
            
      <div class="alert alert-warning small mb-3">
       <i class="bi bi-diagram-2-fill"></i>
      <strong>Output Ports:</strong><br/>
     <span class="badge bg-purple me-2">For Each Item</span> Connects to sub-workflow (iteration logic)<br/>
  <span class="badge bg-success">After Loop</span> Connects to next nodes (processed results)
   </div>
   
   <div class="mb-3">
        <div class="workflow-summary p-3" style="background: #f8f9fa; border: 1px solid #dee2e6; border-radius: 4px;">
          <div class="d-flex justify-content-between align-items-center mb-2">
        <span class="small text-muted">
      <i class="bi bi-diagram-2"></i> ${nodeCount} node(s), ${connCount} connection(s)
            </span>
     <span class="badge bg-${nodeCount > 0 ? 'success' : 'secondary'}">
    ${nodeCount > 0 ? 'Configured' : 'Empty'}
           </span>
     </div>
                    <button class="btn btn-sm btn-primary w-100" onclick="LoopNode.editSubWorkflow('${this.id}')">
       <i class="bi bi-pencil-square"></i> Edit Sub-Workflow
</button>
     </div>
            </div>
        `;
   
     // Render execution options
  if (schema.executionOptions && schema.executionOptions.length > 0) {
    html += '<hr />';
            html += ExecutionOptionsEditor.render(this, schema.executionOptions);
  }
    
        // Delete button
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
     * Open sub-workflow editor modal
     * @param {string} nodeId - Loop node ID
     */
    static editSubWorkflow(nodeId) {
const node = window.designerInstance?.getNode(nodeId);
        if (!node) {
         console.error(`LoopNode ${nodeId} not found`);
   return;
   }
    
        // Open the sub-workflow editor modal
        if (typeof SubWorkflowEditor !== 'undefined') {
      SubWorkflowEditor.openEditor(nodeId);
        } else {
            console.error('SubWorkflowEditor not loaded');
         alert('Sub-workflow editor is not available. Please ensure SubWorkflowEditor.js is loaded.');
        }
    }

    /**
     * Toggle expand/collapse state
     * @param {string} nodeId - Loop node ID
     */
    static toggleExpand(nodeId) {
 const node = window.designerInstance?.getNode(nodeId);
      if (!node) return;
        
        node.isExpanded = !node.isExpanded;
        
      // Re-render canvas
        if (typeof render === 'function') {
    render();
  }
    }

    /**
     * Serialize to JSON
     * @returns {object}
     */
    toJSON() {
        return {
...super.toJSON(),
         subWorkflow: this.subWorkflow,
            isExpanded: this.isExpanded
  };
    }

    /**
 * Deserialize from JSON
     * @param {object} json - JSON data
     * @returns {LoopNode}
  */
    static fromJSON(json) {
        const node = new LoopNode(json.id, json.name, json.position.x, json.position.y);
        node.parameters = json.parameters || {
 itemsKey: '',
     outputKey: '',
            loopItemKey: '__loop_item__',
        maxIterations: 0
        };
      node.color = json.color || node.color;
        node.executionOptions = json.executionOptions || null;
        node.subWorkflow = json.subWorkflow || { nodes: [], connections: [] };
        node.isExpanded = json.isExpanded || false;
     return node;
    }

    /**
     * Validate node configuration
     * @returns {{isValid: boolean, errors: Array<string>}}
     */
  validate() {
        const baseValidation = super.validate();
        const errors = [...baseValidation.errors];
     
        // Validate loop-specific configuration
        if (!this.parameters.itemsKey) {
            errors.push('Items Key is required');
        }
        
        if (!this.parameters.outputKey) {
     errors.push('Output Key is required');
        }
        
     if (this.parameters.itemsKey === this.parameters.outputKey) {
 errors.push('Items Key and Output Key must be different');
 }
        
        // Warn if sub-workflow is empty
        if (!this.subWorkflow.nodes || this.subWorkflow.nodes.length === 0) {
  errors.push('Sub-workflow is empty - add nodes to process loop items');
      }
        
        return {
            isValid: errors.length === 0,
            errors
        };
    }
}

// Register with NodeRegistry
if (typeof nodeRegistry !== 'undefined') {
    nodeRegistry.register('LoopNode', LoopNode);
}
