/**
 * Responsible for rendering nodes on the canvas
 */
class NodeRenderer {
    /**
     * Render a node to the DOM
     * @param {BaseNode} node
     * @param {boolean} isSelected
     * @returns {HTMLElement}
     */
    static renderNode(node, isSelected = false) {
     const nodeEl = document.createElement('div');
        nodeEl.className = 'workflow-node' + (isSelected ? ' selected multi-selected' : '');
        nodeEl.style.left = node.position.x + 'px';
        nodeEl.style.top = node.position.y + 'px';
        nodeEl.style.borderColor = node.color;
        nodeEl.dataset.nodeId = node.id;
        
  // Render node header and type
  let html = `
     <div class="node-header">${node.name}</div>
        <div class="node-type">${node.type}</div>
      `;
        
  // Render ports (currently single input/output)
        html += this.renderPorts(node);
  
   nodeEl.innerHTML = html;
        
        return nodeEl;
    }

  /**
     * Render input and output ports
     * @param {BaseNode} node
   * @returns {string}
     */
    static renderPorts(node) {
        // For now, render simple single ports
        // Phase 2 will implement multi-port rendering based on schema
        return `
    <div class="port input" data-port="input"></div>
            <div class="port output" data-port="output"></div>
        `;
    }

    /**
     * Calculate port position percentage for multi-port support
     * @param {number} index
  * @param {number} total
  * @returns {number}
     */
    static calculatePortPosition(index, total) {
        if (total === 1) return 50;
    const spacing = 40 / (total - 1);
        return 30 + (index * spacing);
    }
}
