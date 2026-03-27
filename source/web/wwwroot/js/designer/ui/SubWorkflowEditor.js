/**
 * Sub-Workflow Editor Modal
 * Opens a modal dialog with a mini designer canvas for editing container node contents
 * Phase 5: Special Node Types - Task 5.6
 */
class SubWorkflowEditor {
    static currentContainerNode = null;
    static currentWorkflowKey = 'subWorkflow';
    static currentEditorOptions = {};
    static miniDesignerState = null;
    static isDraggingConnection = false;
    static connectionStart = null;
    static tempLine = null;

    /**
     * Open the sub-workflow editor modal
     * @param {string} containerNodeId - ID of the container node (LoopNode, ParallelNode, etc.)
     * @param {string} workflowKey - Property name on node that stores workflow data
     * @param {object} options - UI options ({ title, hint })
     */
    static openEditor(containerNodeId, workflowKey = 'subWorkflow', options = {}) {
        const node = window.designerInstance?.getNode(containerNodeId);
        if (!node) {
     console.error(`Container node ${containerNodeId} not found`);
            return;
        }

        this.currentContainerNode = node;
        this.currentWorkflowKey = workflowKey;
        this.currentEditorOptions = options || {};

        if (!node[workflowKey]) {
            node[workflowKey] = { nodes: [], connections: [], variables: {} };
        }
        
        // Initialize mini designer state with sub-workflow data
        const workflowData = node[workflowKey] || {};
   this.miniDesignerState = {
     nodes: [...(workflowData.nodes || [])],
       connections: [...(workflowData.connections || [])],
variables: { ...(workflowData.variables || {}) }
 };

        // Create modal
        this.createModal(node);
      
      // Initialize mini designer after modal is rendered
      setTimeout(() => this.initializeMiniDesigner(node), 100);
    }

    /**
     * Create and display the modal
     * @param {BaseNode} node - Container node
     */
    static createModal(node) {
        const modalTitle = this.currentEditorOptions.title || `Edit Sub-Workflow: ${node.name}`;
        const hint = this.currentEditorOptions.hint ||
            (node.type === 'LoopNode'
                ? `Loop variable: <code>{{${node.parameters?.loopItemKey || '__loop_item__'}}}</code>`
                : 'Edit nested workflow logic');

        const modalHtml = `
            <div class="modal fade show" id="sub-workflow-modal" style="display: block;" tabindex="-1">
      <div class="modal-dialog modal-xl modal-fullscreen-lg-down">
 <div class="modal-content">
       <div class="modal-header bg-light">
              <h5 class="modal-title">
        <i class="bi bi-diagram-3"></i> 
             ${modalTitle}
         </h5>
          <button type="button" class="btn-close" onclick="SubWorkflowEditor.closeEditor()"></button>
          </div>
   <div class="modal-body p-0" style="height: 70vh;">
            <div class="sub-workflow-designer-container">
 <!-- Toolbar -->
      <div class="sub-workflow-toolbar">
      <div class="toolbar-section">
       <span class="small text-muted">
             <i class="bi bi-info-circle"></i> 
        ${hint}
        </span>
  </div>
   <div class="toolbar-section">
     <span class="badge bg-secondary" id="sub-node-count">0 nodes</span>
        <span class="badge bg-secondary" id="sub-connection-count">0 connections</span>
         </div>
    </div>
  
   <!-- Mini Designer Canvas -->
          <div class="sub-workflow-canvas-wrapper">
        <!-- Left Sidebar: Simple Node Palette -->
     <div class="sub-workflow-palette">
         <h6 class="palette-title">Add Nodes</h6>
      <div id="sub-workflow-palette-nodes">
   <!-- Simplified node palette will be rendered here -->
       </div>
   </div>
     
      <!-- Canvas Area -->
           <div class="sub-workflow-canvas-area" id="sub-canvas-area">
   <svg id="sub-workflow-canvas" xmlns="http://www.w3.org/2000/svg">
        <defs>
      <marker id="sub-arrowhead" markerWidth="10" markerHeight="6" refX="5" refY="3" orient="auto">
    <polygon points="0 0, 10 3, 0 6" fill="#3498db" />
    </marker>
                  </defs>
       <g id="sub-connections-layer"></g>
   <g id="sub-temp-connection-layer"></g>
       </svg>
             <div id="sub-nodes-layer"></div>
          </div>
              
                 <!-- Right Sidebar: Properties -->
    <div class="sub-workflow-properties">
<h6 class="properties-title">
    <i class="bi bi-sliders"></i> Properties
           </h6>
        <div id="sub-properties-content" class="properties-content">
              <p class="text-muted small">Select a node to edit</p>
   </div>
        </div>
         </div>
        </div>
   </div>
   <div class="modal-footer">
         <button class="btn btn-secondary" onclick="SubWorkflowEditor.closeEditor()">
               <i class="bi bi-x-circle"></i> Cancel
              </button>
               <button class="btn btn-primary" onclick="SubWorkflowEditor.saveSubWorkflow()">
    <i class="bi bi-save"></i> Save Sub-Workflow
        </button>
          </div>
           </div>
        </div>
     </div>
      `;

      // Add modal to body
        const modalContainer = document.createElement('div');
      modalContainer.innerHTML = modalHtml;
        document.body.appendChild(modalContainer);

        // Add backdrop
        const backdrop = document.createElement('div');
        backdrop.className = 'modal-backdrop fade show';
        backdrop.id = 'sub-workflow-backdrop';
        document.body.appendChild(backdrop);
    }

    /**
     * Initialize the mini designer canvas
 * @param {BaseNode} containerNode - Container node
     */
    static initializeMiniDesigner(containerNode) {
    // Render simplified node palette (no container nodes allowed)
     this.renderMiniPalette();
        
        // Render existing nodes if any
      this.renderMiniCanvas();
     
        // Setup event listeners
        this.setupMiniDesignerEvents();
 
        // Update counts
        this.updateNodeCounts();
    }

    /**
     * Render simplified node palette for sub-workflow
     */
    static renderMiniPalette() {
        const palette = document.getElementById('sub-workflow-palette-nodes');
      if (!palette) return;

        // Only allow basic nodes in sub-workflow (no loops, parallels, branches)
        const allowedNodeTypes = [
       { type: 'PromptBuilderNode', category: 'AI', name: 'Prompt Builder', color: '#4A90E2' },
      { type: 'LlmNode', category: 'AI', name: 'LLM', color: '#4A90E2' },
         { type: 'TransformNode', category: 'Data', name: 'Transform', color: '#7ED321' },
            { type: 'HttpRequestNode', category: 'IO', name: 'HTTP Request', color: '#BD10E0' },
            { type: 'LogNode', category: 'Control', name: 'Log', color: '#F5A623' }
        ];

        let html = '';
        allowedNodeTypes.forEach(node => {
        html += `
     <div class="sub-palette-node-item" 
         draggable="true"
         data-type="${node.type}"
   data-category="${node.category}"
     data-name="${node.name}"
       data-color="${node.color}"
      style="border-left-color: ${node.color}"
     ondragstart="SubWorkflowEditor.onPaletteDragStart(event)">
 <div class="node-item-name" style="color: ${node.color}">${node.name}</div>
     </div>
      `;
        });

  palette.innerHTML = html;
    }

    /**
     * Render mini canvas with existing nodes
     */
    static renderMiniCanvas() {
        const nodesLayer = document.getElementById('sub-nodes-layer');
   if (!nodesLayer) return;

        // Clear existing nodes
        nodesLayer.innerHTML = '';

    // Render nodes from miniDesignerState
  if (this.miniDesignerState.nodes) {
  this.miniDesignerState.nodes.forEach(nodeData => {
 this.renderMiniNode(nodeData);
        });
        }

      // Render connections
        this.renderMiniConnections();
    }

    /**
   * Render a single node in mini canvas
  * @param {object} nodeData - Node data
     */
    static renderMiniNode(nodeData) {
    const nodesLayer = document.getElementById('sub-nodes-layer');
  if (!nodesLayer) return;

        const nodeEl = document.createElement('div');
      nodeEl.className = 'workflow-node sub-workflow-node';
    nodeEl.style.left = nodeData.position.x + 'px';
        nodeEl.style.top = nodeData.position.y + 'px';
        nodeEl.style.borderColor = nodeData.color || '#3498db';
    nodeEl.dataset.nodeId = nodeData.id;
        nodeEl.dataset.nodeType = nodeData.type;

        nodeEl.innerHTML = `
            <div class="node-header">${nodeData.name}</div>
 <div class="node-type">${nodeData.type}</div>
      <div class="port-container input" style="left: -7px; top: 50%; transform: translateY(-50%);">
       <div class="port port-data" data-port="input" data-node-id="${nodeData.id}"></div>
            </div>
       <div class="port-container output" style="right: -7px; top: 50%; transform: translateY(-50%);">
        <div class="port port-data" data-port="output" data-node-id="${nodeData.id}"></div>
   </div>
    `;

        // Add click handler for node selection
        nodeEl.addEventListener('click', (e) => {
            if (!e.target.classList.contains('port')) {
         this.selectMiniNode(nodeData.id);
   }
        });

    // Add port event listeners for connection creation
  const ports = nodeEl.querySelectorAll('.port');
        ports.forEach(port => {
    port.addEventListener('mousedown', (e) => this.onPortMouseDown(e, nodeData.id, port.dataset.port));
        });

      nodesLayer.appendChild(nodeEl);
    }

    /**
     * Render connections in mini canvas
     */
    static renderMiniConnections() {
        const connectionsLayer = document.getElementById('sub-connections-layer');
  if (!connectionsLayer) return;

        // Clear existing connections
        connectionsLayer.innerHTML = '';

        // Render each connection
        if (this.miniDesignerState.connections) {
    this.miniDesignerState.connections.forEach(conn => {
  this.renderConnection(conn);
 });
        }
    }

 /**
     * Render a single connection
     * @param {object} connection - Connection data
  */
    static renderConnection(connection) {
        const connectionsLayer = document.getElementById('sub-connections-layer');
   if (!connectionsLayer) return;

        const sourceNode = document.querySelector(`[data-node-id="${connection.sourceNodeId}"]`);
   const targetNode = document.querySelector(`[data-node-id="${connection.targetNodeId}"]`);

      if (!sourceNode || !targetNode) return;

   const sourcePort = sourceNode.querySelector(`.port[data-port="${connection.sourcePortId || 'output'}"]`);
        const targetPort = targetNode.querySelector(`.port[data-port="${connection.targetPortId || 'input'}"]`);

        if (!sourcePort || !targetPort) return;

        const canvasArea = document.getElementById('sub-canvas-area');
        const canvasRect = canvasArea.getBoundingClientRect();
   const sourceRect = sourcePort.getBoundingClientRect();
        const targetRect = targetPort.getBoundingClientRect();

        const x1 = sourceRect.left + sourceRect.width / 2 - canvasRect.left + canvasArea.scrollLeft;
   const y1 = sourceRect.top + sourceRect.height / 2 - canvasRect.top + canvasArea.scrollTop;
 const x2 = targetRect.left + targetRect.width / 2 - canvasRect.left + canvasArea.scrollLeft;
     const y2 = targetRect.top + targetRect.height / 2 - canvasRect.top + canvasArea.scrollTop;

        // Create curved path
        const dx = x2 - x1;
        const dy = y2 - y1;
      const curve = Math.abs(dx) * 0.5;

        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute('d', `M ${x1} ${y1} C ${x1 + curve} ${y1}, ${x2 - curve} ${y2}, ${x2} ${y2}`);
        path.setAttribute('class', 'connection-line');
        path.setAttribute('stroke', '#3498db');
      path.setAttribute('stroke-width', '2.5');
   path.setAttribute('fill', 'none');
        path.setAttribute('marker-end', 'url(#sub-arrowhead)');
        path.setAttribute('vector-effect', 'non-scaling-stroke');
        path.dataset.connectionId = connection.id;

   // Add click handler for deletion
 path.addEventListener('click', () => this.deleteConnection(connection.id));
        path.style.cursor = 'pointer';

        connectionsLayer.appendChild(path);
    }

    /**
     * Handle port mousedown for connection creation
     */
    static onPortMouseDown(event, nodeId, portId) {
        event.preventDefault();
        event.stopPropagation();

        this.isDraggingConnection = true;
        this.connectionStart = { nodeId, portId };

        const canvasArea = document.getElementById('sub-canvas-area');
        const svg = document.getElementById('sub-workflow-canvas');
        const tempLayer = document.getElementById('sub-temp-connection-layer');

        // Create temporary line
     const canvasRect = canvasArea.getBoundingClientRect();
    const portRect = event.target.getBoundingClientRect();
        const startX = portRect.left + portRect.width / 2 - canvasRect.left + canvasArea.scrollLeft;
        const startY = portRect.top + portRect.height / 2 - canvasRect.top + canvasArea.scrollTop;

   this.tempLine = document.createElementNS('http://www.w3.org/2000/svg', 'line');
        this.tempLine.setAttribute('x1', startX);
  this.tempLine.setAttribute('y1', startY);
        this.tempLine.setAttribute('x2', startX);
        this.tempLine.setAttribute('y2', startY);
        this.tempLine.setAttribute('stroke', '#3498db');
        this.tempLine.setAttribute('stroke-width', '2.5');
  this.tempLine.setAttribute('stroke-dasharray', '5,5');
        this.tempLine.setAttribute('vector-effect', 'non-scaling-stroke');
      tempLayer.appendChild(this.tempLine);

    // Add mousemove and mouseup handlers
        const onMouseMove = (e) => this.onConnectionDrag(e);
        const onMouseUp = (e) => this.onConnectionDrop(e, onMouseMove, onMouseUp);

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
    }

    /**
     * Handle connection drag
     */
    static onConnectionDrag(event) {
  if (!this.isDraggingConnection || !this.tempLine) return;

        const canvasArea = document.getElementById('sub-canvas-area');
        const canvasRect = canvasArea.getBoundingClientRect();
        
 const x = event.clientX - canvasRect.left + canvasArea.scrollLeft;
        const y = event.clientY - canvasRect.top + canvasArea.scrollTop;

        this.tempLine.setAttribute('x2', x);
        this.tempLine.setAttribute('y2', y);
    }

    /**
     * Handle connection drop
   */
    static onConnectionDrop(event, mouseMoveHandler, mouseUpHandler) {
   document.removeEventListener('mousemove', mouseMoveHandler);
        document.removeEventListener('mouseup', mouseUpHandler);

        if (!this.isDraggingConnection) return;

    // Remove temporary line
        if (this.tempLine) {
            this.tempLine.remove();
 this.tempLine = null;
        }

  // Check if dropped on a port
  const targetPort = event.target.closest('.port');
        if (targetPort) {
      const targetNodeId = targetPort.dataset.nodeId;
 const targetPortId = targetPort.dataset.port;

   // Create connection if valid
   if (this.validateConnection(this.connectionStart.nodeId, this.connectionStart.portId, targetNodeId, targetPortId)) {
this.addConnection(this.connectionStart.nodeId, this.connectionStart.portId, targetNodeId, targetPortId);
      }
        }

        this.isDraggingConnection = false;
        this.connectionStart = null;
 }

    /**
     * Validate connection
     */
    static validateConnection(sourceNodeId, sourcePortId, targetNodeId, targetPortId) {
        // Can't connect to self
        if (sourceNodeId === targetNodeId) {
            return false;
        }

     // Output port must connect to input port
   if (sourcePortId === 'output' && targetPortId !== 'input') {
 return false;
        }

      if (sourcePortId === 'input' && targetPortId !== 'output') {
            return false;
        }

        // Check if connection already exists
      const exists = this.miniDesignerState.connections.some(conn => 
    conn.sourceNodeId === sourceNodeId && 
            conn.targetNodeId === targetNodeId &&
     conn.sourcePortId === sourcePortId &&
 conn.targetPortId === targetPortId
        );

        return !exists;
    }

    /**
     * Add connection
     */
    static addConnection(sourceNodeId, sourcePortId, targetNodeId, targetPortId) {
        const connection = {
            id: this.generateGuid(),
         sourceNodeId,
      sourcePortId: sourcePortId || 'output',
            targetNodeId,
     targetPortId: targetPortId || 'input'
        };

  this.miniDesignerState.connections.push(connection);
        this.renderConnection(connection);
        this.updateNodeCounts();
    }

    /**
     * Delete connection
     */
    static deleteConnection(connectionId) {
        if (!confirm('Delete this connection?')) return;

        this.miniDesignerState.connections = this.miniDesignerState.connections.filter(
            c => c.id !== connectionId
        );
        this.renderMiniConnections();
   this.updateNodeCounts();
    }

    /**
     * Setup event listeners for mini designer
     */
    static setupMiniDesignerEvents() {
        const canvasArea = document.getElementById('sub-canvas-area');
        if (!canvasArea) return;

        // Drop handler for adding nodes
        canvasArea.addEventListener('drop', (e) => this.onCanvasDrop(e));
        canvasArea.addEventListener('dragover', (e) => e.preventDefault());
    }

    /**
     * Handle palette drag start
     * @param {DragEvent} event 
     */
    static onPaletteDragStart(event) {
     event.dataTransfer.setData('nodeType', event.currentTarget.dataset.type);
event.dataTransfer.setData('nodeCategory', event.currentTarget.dataset.category);
        event.dataTransfer.setData('nodeName', event.currentTarget.dataset.name);
        event.dataTransfer.setData('nodeColor', event.currentTarget.dataset.color);
 }

    /**
     * Handle canvas drop
     * @param {DragEvent} event 
     */
    static onCanvasDrop(event) {
event.preventDefault();

        const type = event.dataTransfer.getData('nodeType');
        const category = event.dataTransfer.getData('nodeCategory');
        const name = event.dataTransfer.getData('nodeName');
        const color = event.dataTransfer.getData('nodeColor');

        if (!type) return;

        const canvasArea = document.getElementById('sub-canvas-area');
        const rect = canvasArea.getBoundingClientRect();
        const x = event.clientX - rect.left + canvasArea.scrollLeft;
        const y = event.clientY - rect.top + canvasArea.scrollTop;

        this.addMiniNode(type, category, name, color, x, y);
 }

    /**
     * Add a node to mini canvas
     */
    static addMiniNode(type, category, name, color, x, y) {
        const nodeData = {
            id: this.generateGuid(),
            name: name,
     type: type,
            category: category,
            parameters: {},
            position: { x: Math.round(x), y: Math.round(y) },
      color: color
        };

    this.miniDesignerState.nodes.push(nodeData);
        this.renderMiniNode(nodeData);
   this.updateNodeCounts();
    }

    /**
     * Select a node in mini canvas
     * @param {string} nodeId 
     */
    static selectMiniNode(nodeId) {
        const node = this.miniDesignerState.nodes.find(n => n.id === nodeId);
        if (!node) return;

        // Update selection visual
        document.querySelectorAll('#sub-nodes-layer .workflow-node').forEach(el => {
      el.classList.remove('selected');
        });
        document.querySelector(`[data-node-id="${nodeId}"]`)?.classList.add('selected');

        // Show properties
this.showMiniNodeProperties(node);
    }

 /**
     * Show node properties in mini panel
     * @param {object} node 
     */
  static showMiniNodeProperties(node) {
        const panel = document.getElementById('sub-properties-content');
      if (!panel) return;

        let html = `
       <h6 class="border-bottom pb-2 mb-3">${node.name}</h6>
       <div class="mb-3">
          <label class="form-label small fw-bold">Node Name</label>
  <input type="text" class="form-control form-control-sm" 
     value="${node.name}" 
            onchange="SubWorkflowEditor.updateMiniNodeProperty('${node.id}', 'name', this.value)" />
     </div>
        <div class="mb-3">
 <label class="form-label small text-muted">Type</label>
          <input type="text" class="form-control form-control-sm" 
         value="${node.type}" disabled />
            </div>
            <hr />
      <div class="d-grid gap-2">
     <button class="btn btn-danger btn-sm" onclick="SubWorkflowEditor.deleteMiniNode('${node.id}')">
            <i class="bi bi-trash"></i> Delete Node
            </button>
   </div>
        `;

        panel.innerHTML = html;
    }

    /**
     * Update mini node property
     */
    static updateMiniNodeProperty(nodeId, property, value) {
  const node = this.miniDesignerState.nodes.find(n => n.id === nodeId);
 if (!node) return;

        node[property] = value;
     this.renderMiniCanvas();
    }

    /**
     * Delete node from mini canvas
     */
    static deleteMiniNode(nodeId) {
        if (!confirm('Delete this node and its connections?')) return;

    this.miniDesignerState.nodes = this.miniDesignerState.nodes.filter(n => n.id !== nodeId);
        this.miniDesignerState.connections = this.miniDesignerState.connections.filter(
            c => c.sourceNodeId !== nodeId && c.targetNodeId !== nodeId
        );
        this.renderMiniCanvas();
 this.updateNodeCounts();
        
        document.getElementById('sub-properties-content').innerHTML = 
            '<p class="text-muted small">Select a node to edit</p>';
    }

  /**
     * Update node and connection counts
     */
    static updateNodeCounts() {
      const nodeCount = this.miniDesignerState.nodes.length;
        const connCount = this.miniDesignerState.connections.length;

   const nodeCountEl = document.getElementById('sub-node-count');
        const connCountEl = document.getElementById('sub-connection-count');

        if (nodeCountEl) nodeCountEl.textContent = `${nodeCount} node${nodeCount !== 1 ? 's' : ''}`;
        if (connCountEl) connCountEl.textContent = `${connCount} connection${connCount !== 1 ? 's' : ''}`;
    }

    /**
     * Save sub-workflow back to container node
     */
    static saveSubWorkflow() {
  if (!this.currentContainerNode) return;

        // Update container node's sub-workflow
        this.currentContainerNode[this.currentWorkflowKey] = {
  nodes: this.miniDesignerState.nodes,
            connections: this.miniDesignerState.connections,
         variables: this.miniDesignerState.variables
        };

        console.log(`Saved nested workflow '${this.currentWorkflowKey}':`, this.currentContainerNode[this.currentWorkflowKey]);

   // Close modal
        this.closeEditor();

        // Refresh properties panel if this node is selected
    if (window.selectedNode?.id === this.currentContainerNode.id) {
      if (typeof renderProperties === 'function') {
    renderProperties();
     }
     }

        // Mark workflow as modified
        if (window.designerInstance?.markModified) {
     window.designerInstance.markModified();
    }
    }

    /**
     * Close the editor modal
     */
    static closeEditor() {
        const modal = document.getElementById('sub-workflow-modal');
        const backdrop = document.getElementById('sub-workflow-backdrop');

        if (modal) modal.closest('div').remove();
        if (backdrop) backdrop.remove();

        this.currentContainerNode = null;
        this.currentWorkflowKey = 'subWorkflow';
        this.currentEditorOptions = {};
        this.miniDesignerState = null;
        this.isDraggingConnection = false;
        this.connectionStart = null;
        this.tempLine = null;
    }

    /**
     * Generate a GUID for new nodes
     * @returns {string}
     */
    static generateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0;
     const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
 });
    }
}

// Expose globally for inline event handlers in modal HTML.
window.SubWorkflowEditor = SubWorkflowEditor;
