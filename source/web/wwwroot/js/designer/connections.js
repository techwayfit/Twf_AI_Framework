// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Connection Management
// ???????????????????????????????????????????????????????????????????????????

function addConnection(sourceNodeId, sourcePort, targetNodeId, targetPort) {
    const connection = {
    id: generateGuid(),
  sourceNodeId: sourceNodeId,
  sourcePort: sourcePort,
        targetNodeId: targetNodeId,
   targetPort: targetPort
    };
    
    workflow.connections.push(connection);
    render();
}

function deleteConnection(connectionId) {
    workflow.connections = workflow.connections.filter(c => c.id !== connectionId);
    selectedConnection = null;
    render();
}

function showConnectionProperties(connection) {
    const sourceNode = workflow.nodes.find(n => n.id === connection.sourceNodeId);
  const targetNode = workflow.nodes.find(n => n.id === connection.targetNodeId);
  
    const panel = document.getElementById('properties-content');
    panel.innerHTML = `
  <h6 class="border-bottom pb-2 mb-3">
    <i class="bi bi-bezier2"></i> Connection
      </h6>
        
 <div class="alert alert-info small">
     <i class="bi bi-info-circle"></i>
   <strong>Drag endpoints to reconnect</strong><br/>
      <small>Drag the green circle (source) or red circle (target) to a different node port.</small>
        </div>
        
      <div class="mb-3">
  <label class="form-label small fw-bold">Source Node</label>
  <div class="input-group input-group-sm">
    <span class="input-group-text bg-success text-white">
   <i class="bi bi-arrow-right-circle"></i>
       </span>
        <input type="text" class="form-control form-control-sm" 
      value="${sourceNode?.name || 'Unknown'} (${connection.sourcePort})" 
          disabled />
   </div>
        <small class="form-text text-muted">Drag the green circle on the canvas to reconnect the source</small>
      </div>
        
        <div class="mb-3">
   <label class="form-label small fw-bold">Target Node</label>
<div class="input-group input-group-sm">
      <span class="input-group-text bg-danger text-white">
    <i class="bi bi-arrow-left-circle"></i>
         </span>
        <input type="text" class="form-control form-control-sm" 
      value="${targetNode?.name || 'Unknown'} (${connection.targetPort})" 
  disabled />
 </div>
            <small class="form-text text-muted">Drag the red circle on the canvas to reconnect the target</small>
  </div>
        
        <hr class="mt-4" />
        <div class="d-grid gap-2">
 <button class="btn btn-danger" onclick="deleteSelectedConnection()">
  <i class="bi bi-trash"></i> Delete Connection
    </button>
        </div>
    `;
}

// Helper function to delete the selected connection
function deleteSelectedConnection() {
    if (!selectedConnection) return;
    
    if (confirm('Delete this connection?')) {
        deleteConnection(selectedConnection.id);
    }
}

// ??? Connection Endpoint Dragging ??????????????????????????????????????????

function startConnectionDrag(connectionId, end, event) {
    event.preventDefault();
    event.stopPropagation();
    
    // Immediately set the dragging state to prevent other handlers
    isDraggingConnection = true;
    draggedConnectionEnd = { connectionId, end };

    const connection = workflow.connections.find(c => c.id === connectionId);
    if (!connection) {
        isDraggingConnection = false;
        draggedConnectionEnd = null;
        return;
    }
    
  // Highlight the connection being dragged
    const pathEl = document.querySelector(`path[data-connection-id="${connectionId}"]`);
    if (pathEl) {
        pathEl.classList.add(end === 'source' ? 'dragging-source' : 'dragging-target');
 }
    
    // Highlight valid drop targets
    highlightValidPorts(connection, end);
    
  console.log(`Started dragging ${end} endpoint of connection ${connectionId}`);
}

function highlightValidPorts(connection, draggedEnd) {
    // Get all ports
    const allPorts = document.querySelectorAll('.workflow-node .port');
 
    allPorts.forEach(port => {
     const portType = port.dataset.port; // 'input' or 'output'
 const nodeId = port.closest('.workflow-node').dataset.nodeId;
        
        // Determine if this port is a valid drop target
        let isValid = false;
        
      if (draggedEnd === 'source') {
            // Dragging source endpoint - can only connect to input ports
       // Can't connect to the current target node
 isValid = portType === 'input' && nodeId !== connection.targetNodeId;
        } else {
      // Dragging target endpoint - can only connect to output ports
     // Can't connect to the current source node
      isValid = portType === 'output' && nodeId !== connection.sourceNodeId;
        }
        
        // Can't connect node to itself
  if (draggedEnd === 'source' && nodeId === connection.sourceNodeId) isValid = false;
      if (draggedEnd === 'target' && nodeId === connection.targetNodeId) isValid = false;

        if (isValid) {
            port.classList.add('drop-target');
        } else {
   port.classList.add('drop-invalid');
        }
  });
}

function clearPortHighlights() {
    document.querySelectorAll('.workflow-node .port').forEach(port => {
     port.classList.remove('drop-target', 'drop-invalid');
    });
}

function finishConnectionDrag(event) {
    if (!isDraggingConnection || !draggedConnectionEnd) return;
    
    const { connectionId, end } = draggedConnectionEnd;
    const connection = workflow.connections.find(c => c.id === connectionId);
    
    if (!connection) {
        cancelConnectionDrag();
        return;
    }
    
    // Check if dropped on a valid port
    const target = event.target;
    if (target.classList.contains('port') && target.classList.contains('drop-target')) {
        const newNodeId = target.closest('.workflow-node').dataset.nodeId;
        const newPort = target.dataset.port;
   
   // Update the connection
  if (end === 'source') {
connection.sourceNodeId = newNodeId;
        connection.sourcePort = newPort;
console.log(`Reconnected source to node ${newNodeId}, port ${newPort}`);
        } else {
   connection.targetNodeId = newNodeId;
            connection.targetPort = newPort;
      console.log(`Reconnected target to node ${newNodeId}, port ${newPort}`);
        }
        
        render();
    } else {
        // Dropped on invalid target - cancel
        console.log('Dropped on invalid target, reverting');
  }
    
 cancelConnectionDrag();
}

function cancelConnectionDrag() {
    if (!isDraggingConnection) return;
    
    // Remove highlight from dragged connection
    if (draggedConnectionEnd) {
        const pathEl = document.querySelector(`path[data-connection-id="${draggedConnectionEnd.connectionId}"]`);
        if (pathEl) {
            pathEl.classList.remove('dragging-source', 'dragging-target');
        }
    }
    
    // Clear port highlights
    clearPortHighlights();
    
    isDraggingConnection = false;
    draggedConnectionEnd = null;
    
    render();
}

function updateConnectionDragFeedback(e) {
    // Highlight port under mouse
    const target = e.target;
    
    // Remove previous hover highlights
    document.querySelectorAll('.port.drop-target').forEach(port => {
        port.style.transform = '';
    });
    
    if (target.classList.contains('port') && target.classList.contains('drop-target')) {
        // Visual feedback on valid drop target
        target.style.transform = 'translateY(-50%) scale(1.8)';
    }
}
