/**
 * Main designer orchestrator
 * Coordinates all components using the new architecture
 */
class WorkflowDesigner {
    constructor(workflowId) {
        this.workflowId = workflowId;
  this.workflow = null;
        this.selectedNode = null;
        this.selectedNodes = new Set();
        this.selectedVariable = null;
        this.selectedConnection = null;
        
        // Expose globally for backward compatibility
        window.designerInstance = this;
    }

    /**
     * Initialize the designer (called after existing initialization)
     */
    async initialize() {
        console.log('Initializing new node architecture...');
        
        // Load schemas into registry
    await nodeRegistry.loadSchemas();

        // Convert existing workflow nodes to class instances if needed
        this.convertWorkflowNodesToClasses();
        
   console.log('New architecture initialized');
    }

    /**
     * Get workflow reference while supporting both legacy global lexical state
     * and window-bound state used by the new architecture.
     * @returns {object|null}
     */
    _getWorkflow() {
        if (window.workflow && window.workflow.nodes) {
            return window.workflow;
        }

        if (typeof workflow !== 'undefined' && workflow && workflow.nodes) {
            window.workflow = workflow;
            return workflow;
        }

        return null;
    }

  /**
     * Convert plain workflow node objects to class instances
     */
    convertWorkflowNodesToClasses() {
        const wf = this._getWorkflow();
        if (!wf || !wf.nodes) return;
        
        const convertedNodes = [];
        
        wf.nodes.forEach(nodeData => {
    // Check if it's already a class instance
  if (nodeData.constructor && nodeData.constructor !== Object) {
     convertedNodes.push(nodeData);
        return;
      }
            
        // Convert plain object to class instance
            const NodeClass = nodeRegistry.nodeTypes.get(nodeData.type);
  if (NodeClass) {
     const node = NodeClass.fromJSON(nodeData);
      convertedNodes.push(node);
            } else {
   console.warn(`Unknown node type: ${nodeData.type}, keeping as plain object`);
      convertedNodes.push(nodeData);
          }
        });
     
        wf.nodes = convertedNodes;
        window.workflow = wf;
      console.log(`Converted ${convertedNodes.length} nodes to class instances`);
  }

    /**
     * Add a new node using the registry
     * @param {string} type
     * @param {string} name
     * @param {number} x
     * @param {number} y
     * @param {string} color
     * @returns {BaseNode}
     */
    addNodeFromRegistry(type, name, x, y, color) {
        const wf = this._getWorkflow();
        if (!wf) {
            throw new Error('Workflow is not loaded');
        }

        const node = nodeRegistry.createNode(type, name, x, y);
        if (color) {
  node.color = color;
        }
        
        wf.nodes.push(node);
        window.workflow = wf;
      return node;
 }

    /**
   * Update node property (called from property panel)
     * @param {string} nodeId
     * @param {string} property
     * @param {any} value
     */
    updateNodeProperty(nodeId, property, value) {
        const wf = this._getWorkflow();
        if (!wf) return;

        const node = wf.nodes.find(n => n.id === nodeId);
        if (node) {
            node[property] = value;
      if (typeof render === 'function') {
            render();
          }
        }
    }

  /**
     * Update node parameter (called from property panel)
     * @param {string} nodeId
 * @param {string} paramName
     * @param {any} value
     */
    updateNodeParameter(nodeId, paramName, value) {
        const wf = this._getWorkflow();
        if (!wf) return;

        const node = wf.nodes.find(n => n.id === nodeId);
        if (node) {
            if (typeof node.updateParameter === 'function') {
      node.updateParameter(paramName, value);
    } else {
           // Fallback for plain objects
        if (!node.parameters) node.parameters = {};
                node.parameters[paramName] = value;
            }
 console.log(`Updated ${node.name}.${paramName} =`, value);
      }
    }

    /**
     * Update node parameter from JSON (called from property panel)
     * @param {string} nodeId
     * @param {string} paramName
     * @param {string} jsonString
     */
    updateNodeParameterJson(nodeId, paramName, jsonString) {
        const wf = this._getWorkflow();
        if (!wf) return;

        const node = wf.nodes.find(n => n.id === nodeId);
        if (!node) return;

        try {
       const value = jsonString ? JSON.parse(jsonString) : null;
        if (typeof node.updateParameter === 'function') {
  node.updateParameter(paramName, value);
        } else {
    if (!node.parameters) node.parameters = {};
   node.parameters[paramName] = value;
      }
    console.log(`Updated ${node.name}.${paramName} =`, value);
     } catch (error) {
            console.error('Invalid JSON:', error);
      alert('Invalid JSON format. Please check your input.');
 }
    }

    /**
     * Get a node by ID
     * @param {string} nodeId
     * @returns {BaseNode|null}
     */
    getNode(nodeId) {
        const wf = this._getWorkflow();
        return wf?.nodes.find(n => n.id === nodeId) || null;
    }

    /**
     * Delete a node by ID
     * @param {string} nodeId
     */
    deleteNode(nodeId) {
        const wf = this._getWorkflow();
        if (!wf) return;

        const deletedNode = wf.nodes.find(n => n.id === nodeId);

        // Remove node
        wf.nodes = wf.nodes.filter(n => n.id !== nodeId);
        
        // Remove connections
        wf.connections = wf.connections.filter(c => c.sourceNodeId !== nodeId && c.targetNodeId !== nodeId);
        if (deletedNode?.type === 'ErrorNode' || wf.errorNodeId === nodeId) {
            wf.errorNodeId = null;
        }
        window.workflow = wf;
        
// Clear selection
        this.selectedNode = null;
        this.selectedNodes.delete(nodeId);
        if (typeof selectedNode !== 'undefined') selectedNode = null;
        if (typeof selectedNodes !== 'undefined') selectedNodes.delete(nodeId);
        
        // Re-render
        if (typeof render === 'function') {
   render();
        }
        
   // Clear properties panel
   const propertiesPanel = document.getElementById('properties-content');
     if (propertiesPanel) {
     propertiesPanel.innerHTML = '<p class="text-muted small">Select a node to edit its properties.</p>';
        }
      
        console.log(`Node ${nodeId} deleted`);
    }

    /**
   * Mark workflow as modified
     */
    markModified() {
 // Could implement unsaved changes tracking here
        console.log('Workflow modified');
    }
}

// Initialize designer with new architecture (called after existing initialization)
let designerInstance = null;

async function initializeNewArchitecture() {
    designerInstance = new WorkflowDesigner(window.workflowId);
    await designerInstance.initialize();
    window.designer = designerInstance;
}

// Hook into existing initialization
const originalInitializeDesigner = window.initializeDesigner;
window.initializeDesigner = async function(workflowId, initialSubWorkflowId = null) {
    window.workflowId = workflowId;
    window.initialSubWorkflowId = initialSubWorkflowId;
    
    // Call original initialization
 if (originalInitializeDesigner && originalInitializeDesigner !== window.initializeDesigner) {
   await originalInitializeDesigner(workflowId, initialSubWorkflowId);
    }
    
// Then initialize new architecture
    await initializeNewArchitecture();
};

// Global wrapper functions for toolbar buttons and backward compatibility
function updateNodeProperty(nodeId, property, value) {
    if (window.designerInstance) {
        window.designerInstance.updateNodeProperty(nodeId, property, value);
    }
}

function updateNodeParameter(nodeId, paramName, value) {
    if (window.designerInstance) {
        window.designerInstance.updateNodeParameter(nodeId, paramName, value);
    }
}

function updateNodeParameterJson(nodeId, paramName, jsonString) {
    if (window.designerInstance) {
        window.designerInstance.updateNodeParameterJson(nodeId, paramName, jsonString);
    }
}
