// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Canvas Rendering
// ???????????????????????????????????????????????????????????????????????????

function render() {
    renderNodes();
    renderConnections();
}

function renderNodes() {
  const nodesLayer = document.getElementById('nodes-layer');
    
    // Remove all nodes but keep selection rectangle
    const existingRect = document.getElementById('selection-rect');
    nodesLayer.innerHTML = '';
    if (existingRect) {
        nodesLayer.appendChild(existingRect);
    }
    
    workflow.nodes.forEach(node => {
  const nodeEl = document.createElement('div');
        const isSelected = selectedNodes.has(node.id) || selectedNode?.id === node.id;
        nodeEl.className = 'workflow-node' + (isSelected ? ' selected multi-selected' : '');
  nodeEl.style.left = node.position.x + 'px';
        nodeEl.style.top = node.position.y + 'px';
 nodeEl.style.borderColor = node.color || '#3498db';
        nodeEl.dataset.nodeId = node.id;
 
        nodeEl.innerHTML = `
     <div class="node-header">${node.name}</div>
   <div class="node-type">${node.type}</div>
      <div class="port input" data-port="input"></div>
     <div class="port output" data-port="output"></div>
     `;
   
        // Node drag handlers
        nodeEl.addEventListener('mousedown', (e) => onNodeMouseDown(e, node));
   
   // Port connection handlers
     const ports = nodeEl.querySelectorAll('.port');
        ports.forEach(port => {
          port.addEventListener('mousedown', (e) => onPortMouseDown(e, node));
        });
        
  nodesLayer.appendChild(nodeEl);
    });
}

function renderConnections() {
    const connectionsLayer = document.getElementById('connections-layer');
    connectionsLayer.innerHTML = '';
    
    workflow.connections.forEach(conn => {
   const sourceNode = workflow.nodes.find(n => n.id === conn.sourceNodeId);
        const targetNode = workflow.nodes.find(n => n.id === conn.targetNodeId);
   
        if (!sourceNode || !targetNode) return;
        
 const sourceEl = document.querySelector(`[data-node-id="${conn.sourceNodeId}"] .port.${conn.sourcePort}`);
const targetEl = document.querySelector(`[data-node-id="${conn.targetNodeId}"] .port.${conn.targetPort}`);
        
 if (!sourceEl || !targetEl) return;
        
        const canvas = document.getElementById('canvas-area');
        const canvasRect = canvas.getBoundingClientRect();
        
        const sourceRect = sourceEl.getBoundingClientRect();
      const targetRect = targetEl.getBoundingClientRect();
        
  const x1 = (sourceRect.left + sourceRect.width / 2 - canvasRect.left + canvas.scrollLeft) / zoomLevel;
   const y1 = (sourceRect.top + sourceRect.height / 2 - canvasRect.top + canvas.scrollTop) / zoomLevel;
        const x2 = (targetRect.left + targetRect.width / 2 - canvasRect.left + canvas.scrollLeft) / zoomLevel;
        const y2 = (targetRect.top + targetRect.height / 2 - canvasRect.top + canvas.scrollTop) / zoomLevel;
        
        const path = createBezierPath(x1, y1, x2, y2);
     
     // Create connection group
  const group = document.createElementNS('http://www.w3.org/2000/svg', 'g');
        group.dataset.connectionId = conn.id;
        
        // Create path element
  const pathEl = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        pathEl.setAttribute('d', path);
     pathEl.setAttribute('class', 'connection-line');
pathEl.setAttribute('marker-end', 'url(#arrowhead)');
        pathEl.dataset.connectionId = conn.id;
 
        pathEl.addEventListener('click', (e) => {
 e.stopPropagation();
      selectedConnection = conn;
   selectedNode = null;
    selectedNodes.clear();
        render();
            showConnectionProperties(conn);
        });
   
 // Create draggable source endpoint
        const sourceEndpoint = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        sourceEndpoint.setAttribute('cx', x1);
        sourceEndpoint.setAttribute('cy', y1);
        sourceEndpoint.setAttribute('class', 'connection-endpoint source');
        sourceEndpoint.dataset.connectionId = conn.id;
      sourceEndpoint.dataset.end = 'source';
        
   sourceEndpoint.addEventListener('mousedown', (e) => {
            e.stopPropagation();
  startConnectionDrag(conn.id, 'source', e);
        });
     
        // Create draggable target endpoint
        const targetEndpoint = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        targetEndpoint.setAttribute('cx', x2);
        targetEndpoint.setAttribute('cy', y2);
        targetEndpoint.setAttribute('class', 'connection-endpoint target');
        targetEndpoint.dataset.connectionId = conn.id;
        targetEndpoint.dataset.end = 'target';
        
  targetEndpoint.addEventListener('mousedown', (e) => {
     e.stopPropagation();
            startConnectionDrag(conn.id, 'target', e);
        });
        
        group.appendChild(pathEl);
        group.appendChild(sourceEndpoint);
        group.appendChild(targetEndpoint);
      connectionsLayer.appendChild(group);
    });
}

function renderTempConnection(e) {
    if (!connectingFrom) return;

    const canvas = document.getElementById('canvas-area');
  const canvasRect = canvas.getBoundingClientRect();
    const port = connectingFrom.element;
    const portRect = port.getBoundingClientRect();
    
    const x1 = (portRect.left + portRect.width / 2 - canvasRect.left + canvas.scrollLeft) / zoomLevel;
    const y1 = (portRect.top + portRect.height / 2 - canvasRect.top + canvas.scrollTop) / zoomLevel;
    const x2 = (e.clientX - canvasRect.left + canvas.scrollLeft) / zoomLevel;
    const y2 = (e.clientY - canvasRect.top + canvas.scrollTop) / zoomLevel;
    
    const path = createBezierPath(x1, y1, x2, y2);
    
    document.getElementById('temp-connection-layer').innerHTML = `
    <path d="${path}" class="temp-connection" />
    `;
}

function createBezierPath(x1, y1, x2, y2) {
    const dx = x2 - x1;
    const controlDistance = Math.abs(dx) * 0.5;
    
    const cx1 = x1 + controlDistance;
    const cy1 = y1;
    const cx2 = x2 - controlDistance;
    const cy2 = y2;
    
    return `M ${x1} ${y1} C ${cx1} ${cy1}, ${cx2} ${cy2}, ${x2} ${y2}`;
}
