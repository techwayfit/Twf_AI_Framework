// ??????????????????????????????????????????????????????????????????????????
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
        // Get node schema for port definitions
      const schema = nodeSchemas?.[node.type];
        const isSelected = selectedNodes.has(node.id) || selectedNode?.id === node.id;
 
        // Use NodeRenderer if available (Phase 2 enhancement)
   let nodeEl;
    
    // Debug logging
        console.log(`Rendering node: ${node.name} (${node.type})`);
 console.log('NodeRenderer available?', !!window.NodeRenderer);
        console.log('Schema available?', !!schema);
        
        if (window.NodeRenderer && schema) {
  console.log('Using NodeRenderer for', node.type);
            try {
       nodeEl = NodeRenderer.renderNode(node, schema, isSelected);
      } catch (error) {
     console.error('NodeRenderer failed, falling back to legacy:', error);
      nodeEl = renderNodeLegacy(node, isSelected);
    }
  } else {
       console.log('Using legacy renderer for', node.type, '- NodeRenderer:', !!window.NodeRenderer, 'Schema:', !!schema);
            // Fallback to legacy rendering
            nodeEl = renderNodeLegacy(node, isSelected);
        }
   
  // Ensure node dataset is set (NodeRenderer already sets this, but keep for backward compatibility)
        nodeEl.dataset.nodeId = node.id;
        nodeEl.dataset.nodeType = node.type;
        
        // Add node-condition class for ConditionNode (CSS fallback)
     if (node.type === 'ConditionNode' && !nodeEl.classList.contains('node-condition')) {
   nodeEl.classList.add('node-condition');
      }
   
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

/**
 * Legacy node rendering (fallback)
 * @param {object} node
 * @param {boolean} isSelected
 * @returns {HTMLElement}
 */
function renderNodeLegacy(node, isSelected) {
  const nodeEl = document.createElement('div');
    
    let className = 'workflow-node';
    if (isSelected) className += ' selected multi-selected';
    if (node.type === 'ConditionNode') className += ' node-condition';
    
    nodeEl.className = className;
  nodeEl.style.left = node.position.x + 'px';
    nodeEl.style.top = node.position.y + 'px';
    nodeEl.style.borderColor = node.color || '#3498db';
    
    // Special handling for ConditionNode - render 3 output ports
    if (node.type === 'ConditionNode') {
        nodeEl.innerHTML = `
     <div class="node-header">${node.name}</div>
     <div class="node-type">${node.type}</div>
          <div class="port input" data-port="input" data-port-type="Data"></div>
    <div class="port output" data-port="success" data-port-type="Conditional"></div>
        <div class="port output" data-port="failed" data-port-type="Conditional"></div>
      <div class="port output" data-port="error" data-port-type="Conditional"></div>
      `;
} else {
        // Standard 1 input + 1 output
    nodeEl.innerHTML = `
            <div class="node-header">${node.name}</div>
       <div class="node-type">${node.type}</div>
          <div class="port input" data-port="input"></div>
     <div class="port output" data-port="output"></div>
        `;
    }
    
    return nodeEl;
}

function renderConnections() {
    const connectionsLayer = document.getElementById('connections-layer');
    connectionsLayer.innerHTML = '';
    
  workflow.connections.forEach(conn => {
        const sourceNode = workflow.nodes.find(n => n.id === conn.sourceNodeId);
    const targetNode = workflow.nodes.find(n => n.id === conn.targetNodeId);
        
 if (!sourceNode || !targetNode) return;
   
        // Use NodeRenderer helper if available, fallback to direct query
        let sourceEl, targetEl;
     if (window.NodeRenderer) {
            sourceEl = NodeRenderer.getPortElement(conn.sourceNodeId, conn.sourcePort, 'output');
  targetEl = NodeRenderer.getPortElement(conn.targetNodeId, conn.targetPort, 'input');
        } else {
 sourceEl = document.querySelector(`[data-node-id="${conn.sourceNodeId}"] .port.${conn.sourcePort}`);
    targetEl = document.querySelector(`[data-node-id="${conn.targetNodeId}"] .port.${conn.targetPort}`);
      }
  
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
    
        // Check if this connection is selected
        const isSelected = selectedConnection && selectedConnection.id === conn.id;
    
        // Determine arrowhead marker based on state
  let arrowheadMarker = 'url(#arrowhead)';
        if (isSelected) {
   arrowheadMarker = 'url(#arrowhead-selected)';
        }
    
  // Determine connection type for styling
        const portType = sourceEl.dataset.portType || 'Data';
   const connectionClass = `connection-line connection-${portType.toLowerCase()}` + (isSelected ? ' selected' : '');
    
        // Create connection group
        const group = document.createElementNS('http://www.w3.org/2000/svg', 'g');
     group.dataset.connectionId = conn.id;
        
        // Create path element
        const pathEl = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        pathEl.setAttribute('d', path);
     pathEl.setAttribute('class', connectionClass);
     pathEl.setAttribute('marker-end', arrowheadMarker);
        pathEl.dataset.connectionId = conn.id;
  
        pathEl.addEventListener('click', (e) => {
        e.stopPropagation();
   e.preventDefault();
            selectedConnection = conn;
  selectedNode = null;
            selectedNodes.clear();
            render();
       showConnectionProperties(conn);
 });
    
        // Add hover listener to change arrowhead on hover
        pathEl.addEventListener('mouseenter', () => {
     if (!isSelected) {
          pathEl.setAttribute('marker-end', 'url(#arrowhead-hover)');
            }
        });
   
pathEl.addEventListener('mouseleave', () => {
            if (!isSelected) {
       pathEl.setAttribute('marker-end', 'url(#arrowhead)');
      }
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
        e.preventDefault();
   startConnectionDrag(conn.id, 'source', e);
        });
 
     // Prevent the endpoint from triggering canvas mousedown
        sourceEndpoint.addEventListener('click', (e) => {
 e.stopPropagation();
     e.preventDefault();
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
     e.preventDefault();
       startConnectionDrag(conn.id, 'target', e);
        });
  
      // Prevent the endpoint from triggering canvas mousedown
     targetEndpoint.addEventListener('click', (e) => {
          e.stopPropagation();
e.preventDefault();
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
      <path d="${path}" class="temp-connection" marker-end="url(#arrowhead-temp)" />
    `;
}

function renderDraggingConnection(e) {
    if (!isDraggingConnection || !draggedConnectionEnd) return;
    
    const { connectionId, end } = draggedConnectionEnd;
    const connection = workflow.connections.find(c => c.id === connectionId);
    if (!connection) return;
    
    const canvas = document.getElementById('canvas-area');
    const canvasRect = canvas.getBoundingClientRect();
    
    // Get the fixed endpoint
    const fixedNodeId = end === 'source' ? connection.targetNodeId : connection.sourceNodeId;
    const fixedPort = end === 'source' ? connection.targetPort : connection.sourcePort;
    
  const fixedEl = document.querySelector(`[data-node-id="${fixedNodeId}"] .port.${fixedPort}`);
    if (!fixedEl) return;
    
    const fixedRect = fixedEl.getBoundingClientRect();
    const fixedX = (fixedRect.left + fixedRect.width / 2 - canvasRect.left + canvas.scrollLeft) / zoomLevel;
    const fixedY = (fixedRect.top + fixedRect.height / 2 - canvasRect.top + canvas.scrollTop) / zoomLevel;
    
 // Mouse position
    const mouseX = (e.clientX - canvasRect.left + canvas.scrollLeft) / zoomLevel;
    const mouseY = (e.clientY - canvasRect.top + canvas.scrollTop) / zoomLevel;
  
    // Create path from fixed to mouse
    let path;
    let markerType;
    if (end === 'source') {
    // Dragging source, so mouse is source
   path = createBezierPath(mouseX, mouseY, fixedX, fixedY);
        markerType = 'url(#arrowhead-dragging-source)';
    } else {
        // Dragging target, so mouse is target
        path = createBezierPath(fixedX, fixedY, mouseX, mouseY);
        markerType = 'url(#arrowhead-dragging-target)';
    }
    
    document.getElementById('temp-connection-layer').innerHTML = `
        <path d="${path}" class="temp-connection" marker-end="${markerType}" />
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
