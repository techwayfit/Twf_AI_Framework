/**
 * Phase 2: Enhanced Node Rendering
 * Handles multi-port nodes, port labels, and dynamic port generation
 */
class NodeRenderer {
    /**
     * Render a single node with multi-port support
     * @param {object} node - Node instance or plain object
     * @param {object} schema - Node schema with port definitions
     * @param {boolean} isSelected - Whether node is selected
     * @returns {HTMLElement} Rendered node element
     */
    static renderNode(node, schema, isSelected = false) {
        const nodeEl = document.createElement('div');
        
        // Build class list
        let classList = ['workflow-node'];
        if (isSelected) {
            classList.push('selected', 'multi-selected');
        }
        
        // Add node-type-specific class (fallback for CSS styling)
        if (node.type === 'ConditionNode') {
            classList.push('node-condition');
        }
      
        nodeEl.className = classList.join(' ');
        nodeEl.style.left = node.position.x + 'px';
        nodeEl.style.top = node.position.y + 'px';
        nodeEl.style.borderColor = node.color || '#3498db';
        nodeEl.dataset.nodeId = node.id;

     // Set node type for CSS styling (e.g., diamond shape for ConditionNode)
     nodeEl.dataset.nodeType = node.type;

     // Get port definitions from schema
        const inputPorts = schema?.inputPorts || [
      { id: 'input', label: 'Input', type: 'Data', required: true }
  ];
        const outputPorts = NodeRenderer.getOutputPorts(node, schema);

        // Calculate required height based on port count (skip for diamond-shaped nodes)
        const maxPorts = Math.max(inputPorts.length, outputPorts.length);
      if (maxPorts > 1 && node.type !== 'ConditionNode') {
     const requiredHeight = 35 + (maxPorts * 20) + 10; // header + ports + padding
  nodeEl.style.minHeight = `${requiredHeight}px`;
      }

    // Build node HTML
        let html = `
         <div class="node-header">${node.name}</div>
  <div class="node-type">${node.type}</div>
        `;

// Render input ports
  html += NodeRenderer.renderPorts(inputPorts, 'input', node);

        // Render output ports
        html += NodeRenderer.renderPorts(outputPorts, 'output', node);

        nodeEl.innerHTML = html;
        return nodeEl;
    }

    /**
     * Get output ports for a node (handles dynamic ports for ConditionNode)
     * @param {object} node
     * @param {object} schema
     * @returns {Array} Output port definitions
     */
  static getOutputPorts(node, schema) {
  if (!schema?.outputPorts) {
            return [{ id: 'output', label: 'Output', type: 'Data' }];
        }

        // ConditionNode always uses the fixed 3-port schema (Success, Failed, Error)
        // No dynamic port generation needed
        return schema.outputPorts;
    }

    /**
     * Generate dynamic output ports for ConditionNode based on conditions parameter
     * @param {object} node
     * @param {object} schema
     * @returns {Array} Dynamic port definitions
     * @deprecated - ConditionNode now uses fixed ports
     */
  static getDynamicConditionPorts(node, schema) {
        // This method is kept for backward compatibility but is no longer used
   // ConditionNode now has fixed Success/Failed/Error ports
        return schema.outputPorts;
    }

    /**
   * Render ports (input or output)
     * @param {Array} ports - Port definitions
   * @param {string} direction - 'input' or 'output'
 * @param {object} node - Node instance
     * @returns {string} HTML string
     */
    static renderPorts(ports, direction, node) {
 if (!ports || ports.length === 0) return '';

        const isInput = direction === 'input';
        const className = isInput ? 'node-ports-left' : 'node-ports-right';
        
        let html = `<div class="${className}">`;

        ports.forEach((port, index) => {
       const portClass = NodeRenderer.getPortClass(port, direction);
     const portStyle = NodeRenderer.getPortStyle(port, index, ports.length, isInput);
            const tooltip = port.description || port.label || port.id;

            // For INPUT ports: port circle FIRST, then label (label appears on RIGHT when visible)
    // For OUTPUT ports: label FIRST, then port circle (label appears on LEFT when visible)
 const portCircle = `
      <div class="port ${portClass}" 
                  data-port="${port.id}" 
    data-port-type="${port.type || 'Data'}"
         data-required="${port.required || false}">
      </div>
    `;
            
      const portLabel = isInput 
        ? `<span class="port-label port-label-left">${port.label}</span>`
           : `<span class="port-label port-label-right">${port.label}</span>`;

        html += `
 <div class="port-container ${isInput ? 'port-container-left' : 'port-container-right'}" 
         style="${portStyle}"
            title="${tooltip}">
 ${isInput ? portCircle + portLabel : portLabel + portCircle}
            </div>
            `;
    });

        html += '</div>';
        return html;
    }

    /**
     * Get CSS class for port based on type
     * @param {object} port
     * @param {string} direction
     * @returns {string}
     */
  static getPortClass(port, direction) {
        const classes = [direction];
        
        // Add type-specific class
        if (port.type) {
        // Handle both string and enum types
       const portType = typeof port.type === 'string' 
        ? port.type 
   : (port.type.toString ? port.type.toString() : 'Data');
     classes.push(`port-${portType.toLowerCase()}`);
        }

        // Add required class
  if (port.required) {
            classes.push('port-required');
        }

        // Add condition class for conditional ports
        if (port.condition) {
     classes.push('port-conditional');
  }

        return classes.join(' ');
    }

 /**
     * Calculate port vertical position for multi-port layout
     * @param {object} port
     * @param {number} index
     * @param {number} total
     * @param {boolean} isInput
     * @returns {string} CSS style string
     */
 static getPortStyle(port, index, total, isInput) {
  // Port circle is 14px, so we need to offset by 7px to center it on the edge
  const portRadius = 7; // Half of 14px port size
        
      // For single port, center vertically and horizontally on edge
    if (total === 1) {
  if (isInput) {
  // Left edge: move container left by port radius so port circle is centered on edge
    return `top: 50%; transform: translateX(-${portRadius}px) translateY(-50%);`;
            } else {
         // Right edge: move container right by port radius so port circle is centered on edge
      return `top: 50%; transform: translateX(${portRadius}px) translateY(-50%);`;
            }
        }

        // For multiple ports, distribute vertically
        const headerHeight = 35;
        const estimatedNodeHeight = Math.max(60, headerHeight + (total * 20));
      const availableHeight = estimatedNodeHeight - headerHeight;
 const spacing = availableHeight / (total + 1);
        const topPosition = headerHeight + (spacing * (index + 1));

        // Apply horizontal offset to center port on edge
  if (isInput) {
            return `top: ${topPosition}px; transform: translateX(-${portRadius}px) translateY(-50%);`;
        } else {
     return `top: ${topPosition}px; transform: translateX(${portRadius}px) translateY(-50%);`;
        }
    }

    /**
     * Format port ID to readable label
* @param {string} portId
     * @returns {string}
     */
    static formatPortLabel(portId) {
    // Convert snake_case or camelCase to Title Case
        return portId
            .replace(/_/g, ' ')
       .replace(/([A-Z])/g, ' $1')
         .split(' ')
  .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ')
 .trim();
    }

    /**
     * Get port element for connection rendering
     * @param {string} nodeId
     * @param {string} portId
     * @param {string} portClass - 'input' or 'output'
     * @returns {HTMLElement|null}
     */
    static getPortElement(nodeId, portId, portClass) {
   // Try exact match first
     let portEl = document.querySelector(
      `[data-node-id="${nodeId}"] .port.${portClass}[data-port="${portId}"]`
        );

        // Fallback to first port of that type if exact match not found
        if (!portEl) {
      portEl = document.querySelector(
     `[data-node-id="${nodeId}"] .port.${portClass}`
 );
        }

    return portEl;
    }

    /**
     * Validate connection between two ports
  * @param {object} sourceNode
     * @param {string} sourcePortId
     * @param {object} targetNode
     * @param {string} targetPortId
     * @returns {{isValid: boolean, reason?: string}}
*/
    static validateConnection(sourceNode, sourcePortId, targetNode, targetPortId) {
  // Can't connect node to itself
        if (sourceNode.id === targetNode.id) {
        return { isValid: false, reason: 'Cannot connect node to itself' };
        }

        // Get schemas
  const sourceSchema = nodeSchemas[sourceNode.type];
        const targetSchema = nodeSchemas[targetNode.type];

        if (!sourceSchema || !targetSchema) {
   return { isValid: true }; // Allow if schemas not available
        }

        // Get port definitions
  const sourceOutputPorts = NodeRenderer.getOutputPorts(sourceNode, sourceSchema);
        const targetInputPorts = targetSchema.inputPorts || [];

        const sourcePort = sourceOutputPorts.find(p => p.id === sourcePortId);
        const targetPort = targetInputPorts.find(p => p.id === targetPortId);

if (!sourcePort || !targetPort) {
            return { isValid: true }; // Allow if ports not defined in schema
        }

        // Type compatibility check (basic - can be extended)
        // For now, allow all connections
        // TODO: Add port type compatibility rules

   return { isValid: true };
    }
}

// Export for use in other modules
window.NodeRenderer = NodeRenderer;
