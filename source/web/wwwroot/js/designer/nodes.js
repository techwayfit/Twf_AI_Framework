// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Node Management
// ???????????????????????????????????????????????????????????????????????????

function renderNodePalette() {
    const palette = document.getElementById('palette-nodes');
    const categories = [...new Set(availableNodes.map(n => n.category))];
    
    let html = '';
    categories.forEach(category => {
     html += `<div class="node-category">${category}</div>`;
  
        const nodes = availableNodes.filter(n => n.category === category);
      nodes.forEach(node => {
            html += `
           <div class="node-item" 
          draggable="true" 
       data-type="${node.type}"
           data-category="${node.category}"
         data-name="${node.name}"
     data-color="${node.color}"
      style="border-left-color: ${node.color}"
           ondragstart="onPaletteDragStart(event)">
          <div class="node-item-name" style="color: ${node.color}">${node.name}</div>
       <div class="node-item-desc">${node.description}</div>
  </div>
            `;
        });
    });
    
palette.innerHTML = html;
}

function onPaletteDragStart(event) {
    event.dataTransfer.setData('nodeType', event.currentTarget.dataset.type);
    event.dataTransfer.setData('nodeCategory', event.currentTarget.dataset.category);
    event.dataTransfer.setData('nodeName', event.currentTarget.dataset.name);
    event.dataTransfer.setData('nodeColor', event.currentTarget.dataset.color);
}

function addNode(type, category, name, color, x, y) {
    const node = {
        id: generateGuid(),
        name: name,
        type: type,
        category: category,
        parameters: {},
        position: { x: Math.round(x), y: Math.round(y) },
        color: color
    };
    
    workflow.nodes.push(node);
 render();
    selectNode(node.id);
}

function deleteNode(nodeId) {
    workflow.nodes = workflow.nodes.filter(n => n.id !== nodeId);
    workflow.connections = workflow.connections.filter(
        c => c.sourceNodeId !== nodeId && c.targetNodeId !== nodeId
    );
    deselectAll();
    render();
}

function selectNode(nodeId, addToSelection = false) {
    if (!addToSelection) {
  deselectAll();
    }
    
    selectedNode = workflow.nodes.find(n => n.id === nodeId);
    selectedNodes.add(nodeId);
    render();
    
    // Only show properties if single selection
    if (selectedNodes.size === 1) {
        renderProperties();
    } else {
 showMultiSelectionInfo();
    }
}

function selectAll() {
    selectedNodes.clear();
    workflow.nodes.forEach(node => selectedNodes.add(node.id));
    selectedNode = null;
    render();
  showMultiSelectionInfo();
}

function deselectAll() {
    selectedNode = null;
    selectedNodes.clear();
    selectedConnection = null;
    selectedVariable = null;
    render();
    document.getElementById('properties-content').innerHTML = 
   '<p class="text-muted small">Select a node or variable to edit.</p>';
}

function deleteSelected() {
    if (selectedNodes.size > 0) {
     if (confirm(`Delete ${selectedNodes.size} selected node(s)?`)) {
            selectedNodes.forEach(nodeId => {
     workflow.nodes = workflow.nodes.filter(n => n.id !== nodeId);
     workflow.connections = workflow.connections.filter(
  c => c.sourceNodeId !== nodeId && c.targetNodeId !== nodeId
   );
     });
  deselectAll();
    render();
 }
    } else if (selectedNode) {
if (confirm(`Delete node "${selectedNode.name}"?`)) {
      deleteNode(selectedNode.id);
        }
    } else if (selectedConnection) {
  if (confirm('Delete this connection?')) {
 deleteConnection(selectedConnection.id);
        }
    }
}

function showMultiSelectionInfo() {
    const panel = document.getElementById('properties-content');
    panel.innerHTML = `
        <div class="alert alert-info small">
         <i class="bi bi-check2-square"></i> 
            <strong>${selectedNodes.size} nodes selected</strong>
        </div>
      <div class="d-grid gap-2">
  <button class="btn btn-sm btn-danger" onclick="deleteSelected()">
          <i class="bi bi-trash"></i> Delete Selected
            </button>
            <button class="btn btn-sm btn-secondary" onclick="deselectAll()">
      <i class="bi bi-x-circle"></i> Deselect All
    </button>
</div>
    `;
}
