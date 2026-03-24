// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Event Handlers
// ???????????????????????????????????????????????????????????????????????????

function setupEventListeners() {
    const canvas = document.getElementById('canvas-area');
    
 // Canvas drop event for creating new nodes
    canvas.addEventListener('dragover', (e) => e.preventDefault());
    canvas.addEventListener('drop', onCanvasDrop);
    
    // Canvas mousedown for drag-to-select
    canvas.addEventListener('mousedown', onCanvasMouseDown);
    
    // Global mouse handlers
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
    
    // Keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Delete') {
            deleteSelected();
        }
        if (e.key === 'Escape') {
            deselectAll();
   }
        if (e.ctrlKey && e.key === 's') {
 e.preventDefault();
     saveWorkflow();
        }
        if (e.ctrlKey && e.key === 'a') {
     e.preventDefault();
     selectAll();
}
    });
}

// ??? Canvas Events ??????????????????????????????????????????????????????????

function onCanvasMouseDown(e) {
    // Only start selection if clicking on canvas directly, not on nodes
    if (e.target.id === 'canvas-area' || e.target.id === 'workflow-canvas' || 
        e.target.id === 'nodes-layer' || e.target.closest('g')) {
        
     isSelecting = true;
        const canvas = document.getElementById('canvas-area');
        const rect = canvas.getBoundingClientRect();
      
      selectionStart.x = (e.clientX - rect.left + canvas.scrollLeft) / zoomLevel;
        selectionStart.y = (e.clientY - rect.top + canvas.scrollTop) / zoomLevel;
        
        // Clear selection if not holding Ctrl
        if (!e.ctrlKey) {
    deselectAll();
        }
        
        // Create selection rectangle
        createSelectionRect();
    }
}

function onCanvasDrop(event) {
    event.preventDefault();
    
  const nodeType = event.dataTransfer.getData('nodeType');
    const nodeCategory = event.dataTransfer.getData('nodeCategory');
    const nodeName = event.dataTransfer.getData('nodeName');
    const nodeColor = event.dataTransfer.getData('nodeColor');

    if (!nodeType) return;
    
    const canvas = document.getElementById('canvas-area');
    const rect = canvas.getBoundingClientRect();
    
    // Calculate position relative to canvas scroll - center the node at drop point
 const x = (event.clientX - rect.left + canvas.scrollLeft) / zoomLevel - 80;
    const y = (event.clientY - rect.top + canvas.scrollTop) / zoomLevel - 40;

    addNode(nodeType, nodeCategory, nodeName, nodeColor, Math.max(0, x), Math.max(0, y));
}

// ??? Node Events ????????????????????????????????????????????????????????????

function onNodeMouseDown(e, node) {
    if (e.target.classList.contains('port')) return;
    
    // Check if clicking on already selected node
    const clickedNodeSelected = selectedNodes.has(node.id);
    
    if (e.ctrlKey) {
        // Ctrl+click toggles selection
        if (clickedNodeSelected) {
       selectedNodes.delete(node.id);
        if (selectedNode?.id === node.id) selectedNode = null;
        } else {
      selectedNodes.add(node.id);
          selectedNode = node;
 }
      render();
      if (selectedNodes.size === 1) {
            renderProperties();
        } else {
  showMultiSelectionInfo();
        }
    } else {
        // Regular click
      if (!clickedNodeSelected) {
            // Click on unselected node - select only this one
 selectNode(node.id, false);
     }
        
     // Start dragging
    isDraggingNode = true;
        isDraggingSelection = selectedNodes.size > 1;
   
        const canvas = document.getElementById('canvas-area');
        const canvasRect = canvas.getBoundingClientRect();
        
        // Calculate offset from mouse to THIS node's position
     const mouseX = e.clientX - canvasRect.left + canvas.scrollLeft;
        const mouseY = e.clientY - canvasRect.top + canvas.scrollTop;
        
        dragOffset.x = (mouseX / zoomLevel) - node.position.x;
        dragOffset.y = (mouseY / zoomLevel) - node.position.y;
        
   // Store initial positions of all selected nodes for group dragging
      if (isDraggingSelection) {
   groupDragStart = {};
  selectedNodes.forEach(nId => {
         const n = workflow.nodes.find(nd => nd.id === nId);
if (n) {
                groupDragStart[nId] = { x: n.position.x, y: n.position.y };
  }
            });
 }
        
        // Set the clicked node as the primary node for dragging
        selectedNode = node;
    }
    
    e.preventDefault();
}

function onPortMouseDown(e, node) {
 e.stopPropagation();
    isConnecting = true;
    connectingFrom = {
        nodeId: node.id,
        port: e.target.dataset.port,
        element: e.target
    };
}

// ??? Mouse Move/Up Handlers ?????????????????????????????????????????????????

function onMouseMove(e) {
    if (isSelecting && selectionRect) {
        const canvas = document.getElementById('canvas-area');
        const rect = canvas.getBoundingClientRect();
        
   const currentX = (e.clientX - rect.left + canvas.scrollLeft) / zoomLevel;
    const currentY = (e.clientY - rect.top + canvas.scrollTop) / zoomLevel;
  
        updateSelectionRect(currentX, currentY);
    } else if (isDraggingConnection) {
        updateConnectionDragFeedback(e);
    } else if (isDraggingNode) {
        const canvas = document.getElementById('canvas-area');
      const rect = canvas.getBoundingClientRect();
        
        const mouseX = e.clientX - rect.left + canvas.scrollLeft;
 const mouseY = e.clientY - rect.top + canvas.scrollTop;
        
 const newX = (mouseX / zoomLevel) - dragOffset.x;
        const newY = (mouseY / zoomLevel) - dragOffset.y;

        if (isDraggingSelection && selectedNodes.size > 1) {
   const initialPos = groupDragStart[selectedNode.id];
      if (initialPos) {
  const deltaX = newX - initialPos.x;
       const deltaY = newY - initialPos.y;

  selectedNodes.forEach(nodeId => {
    const node = workflow.nodes.find(n => n.id === nodeId);
       const startPos = groupDragStart[nodeId];
            if (node && startPos) {
              node.position.x = Math.max(0, Math.round(startPos.x + deltaX));
         node.position.y = Math.max(0, Math.round(startPos.y + deltaY));
      }
 });
}
   } else if (selectedNode) {
            selectedNode.position.x = Math.max(0, Math.round(newX));
            selectedNode.position.y = Math.max(0, Math.round(newY));
        }
 
        render();
    } else if (isConnecting && connectingFrom) {
        renderTempConnection(e);
    }
}

function onMouseUp(e) {
    if (isSelecting) {
        isSelecting = false;
    if (selectionRect) {
      selectionRect.remove();
     selectionRect = null;
        }
        
  if (selectedNodes.size === 1) {
 selectedNode = workflow.nodes.find(n => n.id === [...selectedNodes][0]);
        renderProperties();
} else if (selectedNodes.size > 1) {
       showMultiSelectionInfo();
        }
    } else if (isDraggingConnection) {
        finishConnectionDrag(e);
    } else if (isDraggingNode) {
     isDraggingNode = false;
     isDraggingSelection = false;
        groupDragStart = {};
    } else if (isConnecting) {
        const target = e.target;
        if (target.classList.contains('port')) {
      const targetNodeId = target.closest('.workflow-node').dataset.nodeId;
            const targetPort = target.dataset.port;
   
      // Don't connect to the same node or same port type
            if (targetNodeId !== connectingFrom.nodeId && 
      targetPort !== connectingFrom.port) {
             addConnection(
           connectingFrom.nodeId,
       connectingFrom.port,
  targetNodeId,
     targetPort
           );
            }
        }
   
   isConnecting = false;
        connectingFrom = null;
document.getElementById('temp-connection-layer').innerHTML = '';
    }
}

// ??? Selection Rectangle ????????????????????????????????????????????????????

function createSelectionRect() {
    const existing = document.getElementById('selection-rect');
    if (existing) existing.remove();
    
    selectionRect = document.createElement('div');
    selectionRect.id = 'selection-rect';
    
    document.getElementById('nodes-layer').appendChild(selectionRect);
}

function updateSelectionRect(currentX, currentY) {
    if (!selectionRect) return;
    
  const left = Math.min(selectionStart.x, currentX);
    const top = Math.min(selectionStart.y, currentY);
    const width = Math.abs(currentX - selectionStart.x);
    const height = Math.abs(currentY - selectionStart.y);
    
    selectionRect.style.left = left + 'px';
    selectionRect.style.top = top + 'px';
    selectionRect.style.width = width + 'px';
    selectionRect.style.height = height + 'px';
  
    // Select nodes within rectangle
    selectNodesInRect(left, top, width, height);
}

function selectNodesInRect(rectLeft, rectTop, rectWidth, rectHeight) {
  const rectRight = rectLeft + rectWidth;
  const rectBottom = rectTop + rectHeight;
    
    workflow.nodes.forEach(node => {
  const nodeLeft = node.position.x;
        const nodeTop = node.position.y;
     const nodeRight = nodeLeft + 180;
        const nodeBottom = nodeTop + 80;
        
        const intersects = !(nodeRight < rectLeft || 
 nodeLeft > rectRight || 
        nodeBottom < rectTop || 
            nodeTop > rectBottom);
        
        if (intersects) {
            selectedNodes.add(node.id);
  }
    });
    
    render();
}
