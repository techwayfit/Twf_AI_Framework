# TWF AI Framework - Workflow Designer Enhancement Plan

**Version:** 1.1  
**Date:** January 2025  
**Status:** Planning Phase  
**Owner:** Development Team

---

## ?? Executive Summary

The current workflow designer implements basic sequential workflows but lacks full support for the framework's advanced capabilities. This document outlines a comprehensive plan to enhance the designer, **starting with a solid JavaScript architecture** that mirrors the C# framework structure.

### Key Strategy Change (v1.1)
**Phase 0 has been added as the foundation:** Before implementing complex UI features, we will refactor the JavaScript codebase into a clean, object-oriented architecture with separate classes for each node type. This mirrors the C# structure and provides:
- ? Better maintainability (each node is a separate file)
- ? Clear separation of concerns
- ? Easier testing and debugging
- ? Scalability for future features
- ? Type safety through JSDoc comments

---

## ?? Goals & Objectives

### Primary Goals
1. **Solid Architecture** - Create maintainable JavaScript structure mirroring C# design
2. **Full Feature Parity** - Support all workflow features available in the C# fluent API
3. **Visual Programming** - Enable non-developers to build complex workflows
4. **Developer-Friendly** - Export to clean, maintainable C# code
5. **Production-Ready** - Add execution, debugging, and monitoring capabilities

### Success Metrics
- ? All node types from `NodeSchemaProvider` are fully configurable
- ? Branching and conditional routing work visually
- ? Loop/ForEach nodes can be created and configured
- ? Workflows can be executed directly from the designer
- ? Generated C# code matches manual implementations

---

## ?? Current State Analysis

### ? What Works Today
- Basic sequential workflow design (A ? B ? C)
- Node drag-and-drop from palette
- Single input/output port per node
- Parameter editing via properties panel
- Variable system with autocomplete
- JSON export/import
- Connection creation and deletion
- Multi-node selection and deletion

### ? Current Limitations

#### 1. **Node Configuration**
- [ ] Single input/output port only (no multi-port support)
- [ ] No conditional output ports (e.g., ConditionNode true/false branches)
- [ ] No execution options (retry, timeout, error handling)
- [ ] Limited parameter types (no expression builder, port mapper)

#### 2. **Control Flow**
- [ ] No visual branching (must use C# `Workflow.Branch()` method)
- [ ] No loop/ForEach containers
- [ ] No parallel execution nodes
- [ ] Conditional routing not supported

#### 3. **Execution & Debugging**
- [ ] Cannot execute workflows from designer
- [ ] No real-time execution visualization
- [ ] No debugging or breakpoints
- [ ] No variable inspection during execution

#### 4. **Developer Experience**
- [ ] No C# code generation
- [ ] No validation before execution
- [ ] Limited error messages
- [ ] No workflow templates or examples

---

## ??? Development Roadmap

### Overview

| Phase | Timeline | Status | Completion |
|-------|----------|--------|-----------|
| **Phase 0: JavaScript Architecture** | Week 1 | ?? Not Started | 0% |
| **Phase 1: Node Schema Enhancement** | Week 2 | ?? Not Started | 0% |
| **Phase 2: Visual Node Enhancements** | Week 3 | ?? Not Started | 0% |
| **Phase 3: Condition Node Enhancement** | Week 4 | ?? Not Started | 0% |
| **Phase 4: Node Execution Options** | Week 5 | ?? Not Started | 0% |
| **Phase 5: Special Node Types** | Week 6-7 | ?? Not Started | 0% |
| **Phase 6: Variable System Enhancement** | Week 7-8 | ?? Not Started | 0% |
| **Phase 7: Execution & Debugging** | Week 8-9 | ?? Not Started | 0% |
| **Phase 8: Export/Import & Code Gen** | Week 9-10 | ?? Not Started | 0% |

**Legend:**
- ?? Not Started
- ?? In Progress
- ?? Completed
- ?? Blocked

---

## ??? Phase 0: JavaScript Architecture Refactoring (NEW)

**Timeline:** Week 1  
**Priority:** Critical (Foundation)  
**Dependencies:** None

### Objectives
- Create object-oriented JavaScript architecture mirroring C# design
- Separate each node type into its own file/class
- Establish clear base classes and inheritance hierarchy
- Implement consistent patterns for node rendering, validation, and serialization
- Add JSDoc type annotations for better IDE support

### Why This Matters
Before we add complex features like multi-port support, conditional routing, and execution options, we need a solid foundation. The current monolithic JavaScript files make it hard to:
- Understand node-specific behavior
- Test individual node types
- Add new node types without breaking existing ones
- Maintain consistent behavior across nodes

### Architecture Overview

```
wwwroot/js/designer/
+-- core/
¦   +-- BaseNode.js           // Abstract base class for all nodes
¦   +-- NodeRegistry.js    // Central registry for node types
¦   +-- WorkflowData.js     // WorkflowData model
¦   +-- Constants.js          // Shared constants
+-- nodes/
¦   +-- ai/
¦   ¦   +-- LlmNode.js
¦   ¦   +-- PromptBuilderNode.js
¦   ¦   +-- EmbeddingNode.js
¦   ¦   +-- OutputParserNode.js
¦   +-- control/
¦   ¦   +-- ConditionNode.js
¦   ¦   +-- DelayNode.js
¦   ¦   +-- MergeNode.js
¦   ¦   +-- LogNode.js
¦   +-- data/
¦   ¦+-- TransformNode.js
¦   ¦   +-- FilterNode.js
¦   ¦   +-- ChunkTextNode.js
¦   ¦   +-- MemoryNode.js
¦   +-- io/
¦       +-- HttpRequestNode.js
+-- ui/
¦   +-- NodeRenderer.js       // Responsible for visual rendering
¦   +-- ConnectionManager.js  // Handles connections
¦   +-- PropertiesPanel.js    // Properties panel logic
¦   +-- Canvas.js  // Canvas management
+-- utils/
¦   +-- validation.js
¦   +-- serialization.js
¦   +-- helpers.js
+-- designer.js   // Main orchestrator
```

### Tasks

#### 0.1 Create Base Classes

**File:** `source/web/wwwroot/js/designer/core/BaseNode.js`

```javascript
/**
 * Abstract base class for all workflow nodes
 * Mirrors TwfAiFramework.Nodes.BaseNode
 */
class BaseNode {
    /**
     * @param {string} id - Unique node identifier
     * @param {string} name - Display name
* @param {string} type - Node type (e.g., 'LlmNode')
     * @param {string} category - Node category
     */
    constructor(id, name, type, category) {
        if (new.target === BaseNode) {
            throw new Error('BaseNode is abstract and cannot be instantiated');
    }
        
  this.id = id;
        this.name = name;
     this.type = type;
        this.category = category;
        this.parameters = {};
     this.position = { x: 0, y: 0 };
        this.color = this.getDefaultColor();
        this.executionOptions = null;
    }

  /**
     * Get node schema from server
     * @returns {NodeSchema}
     */
    getSchema() {
        throw new Error('getSchema() must be implemented by subclass');
    }

    /**
     * Get input port definitions
     * @returns {PortDefinition[]}
 */
    getInputPorts() {
        return this.getSchema().inputPorts || [
    { id: 'input', label: 'Input', type: 'data', required: true }
        ];
    }

    /**
     * Get output port definitions
     * @returns {PortDefinition[]}
     */
    getOutputPorts() {
        return this.getSchema().outputPorts || [
   { id: 'output', label: 'Output', type: 'data' }
        ];
    }

    /**
     * Validate node configuration
     * @returns {{isValid: boolean, errors: string[]}}
     */
    validate() {
        const errors = [];
        const schema = this.getSchema();
        
        // Validate required parameters
        schema.parameters.forEach(param => {
            if (param.required && !this.parameters[param.name]) {
 errors.push(`${param.label} is required`);
 }
        });
        
        return {
isValid: errors.length === 0,
        errors
        };
    }

 /**
     * Get default color for this node type
     * @returns {string}
     */
    getDefaultColor() {
        const colorMap = {
    'AI': '#4A90E2',
      'Control': '#F5A623',
 'Data': '#7ED321',
  'IO': '#BD10E0'
        };
        return colorMap[this.category] || '#95a5a6';
    }

    /**
     * Render properties panel content
     * @returns {string} HTML string
     */
renderProperties() {
        throw new Error('renderProperties() must be implemented by subclass');
    }

    /**
   * Serialize to JSON
     * @returns {object}
     */
    toJSON() {
        return {
            id: this.id,
    name: this.name,
     type: this.type,
            category: this.category,
      parameters: this.parameters,
 position: this.position,
         color: this.color,
        executionOptions: this.executionOptions
        };
    }

    /**
     * Deserialize from JSON
     * @param {object} json
     * @returns {BaseNode}
     */
    static fromJSON(json) {
        throw new Error('fromJSON() must be implemented by subclass');
    }

    /**
     * Clone this node
     * @returns {BaseNode}
 */
    clone() {
    const json = this.toJSON();
        json.id = generateGuid();
        return this.constructor.fromJSON(json);
    }
}
```

**Tasks:**
- [ ] Create `BaseNode.js` with JSDoc annotations
- [ ] Add validation logic
- [ ] Add serialization/deserialization
- [ ] Add clone functionality

#### 0.2 Create Node Registry

**File:** `source/web/wwwroot/js/designer/core/NodeRegistry.js`

```javascript
/**
 * Central registry for all node types
 * Provides factory methods for creating nodes
 */
class NodeRegistry {
 constructor() {
        /** @type {Map<string, typeof BaseNode>} */
   this.nodeTypes = new Map();
    
        /** @type {Map<string, NodeSchema>} */
 this.schemas = new Map();
    }

    /**
  * Register a node type
     * @param {string} type - Node type name
     * @param {typeof BaseNode} nodeClass - Node class constructor
     */
    register(type, nodeClass) {
    if (this.nodeTypes.has(type)) {
        console.warn(`Node type '${type}' is already registered. Overwriting.`);
        }
 this.nodeTypes.set(type, nodeClass);
    }

    /**
     * Create a node instance
     * @param {string} type - Node type
     * @param {string} name - Node name
     * @param {number} x - X position
     * @param {number} y - Y position
     * @returns {BaseNode}
     */
    createNode(type, name, x = 0, y = 0) {
    const NodeClass = this.nodeTypes.get(type);
        if (!NodeClass) {
            throw new Error(`Unknown node type: ${type}`);
        }
      
        const id = generateGuid();
  const node = new NodeClass(id, name);
        node.position = { x, y };
        return node;
    }

    /**
     * Get all registered node types grouped by category
     * @returns {Map<string, Array<{type: string, name: string, description: string}>>}
     */
    getNodesByCategory() {
        const grouped = new Map();
        
      for (const [type, NodeClass] of this.nodeTypes) {
     const instance = new NodeClass(generateGuid(), 'temp');
  const category = instance.category;
            
   if (!grouped.has(category)) {
      grouped.set(category, []);
    }

            grouped.get(category).push({
                type: type,
     name: instance.name,
              description: instance.getSchema().description || '',
   color: instance.color
 });
        }
    
        return grouped;
    }

    /**
     * Load schemas from server
     * @returns {Promise<void>}
     */
    async loadSchemas() {
        try {
            const response = await fetch('/Workflow/GetAllNodeSchemas');
         const schemas = await response.json();
            
 for (const [type, schema] of Object.entries(schemas)) {
     this.schemas.set(type, schema);
            }
 } catch (error) {
    console.error('Error loading node schemas:', error);
        }
    }

    /**
     * Get schema for a node type
     * @param {string} type
     * @returns {NodeSchema}
     */
    getSchema(type) {
  return this.schemas.get(type);
    }
}

// Global singleton instance
const nodeRegistry = new NodeRegistry();
```

**Tasks:**
- [ ] Create `NodeRegistry.js`
- [ ] Implement registration system
- [ ] Add factory methods
- [ ] Add schema loading

#### 0.3 Implement Concrete Node Classes

**Example: LlmNode**

**File:** `source/web/wwwroot/js/designer/nodes/ai/LlmNode.js`

```javascript
/**
 * Large Language Model API call node
 * Mirrors TwfAiFramework.Nodes.AI.LlmNode
 */
class LlmNode extends BaseNode {
    constructor(id, name = 'LLM') {
        super(id, name, 'LlmNode', 'AI');
        
        // Set default parameters
        this.parameters = {
       provider: 'openai',
   model: 'gpt-4o',
 apiKey: '',
            apiUrl: '',
    systemPrompt: '',
            temperature: 0.7,
maxTokens: 1000,
 maintainHistory: false
        };
    }

    getSchema() {
   return nodeRegistry.getSchema('LlmNode');
    }

    /**
     * Render properties panel
     * @returns {string}
     */
    renderProperties() {
        const schema = this.getSchema();
        
        let html = `
            <h6 class="border-bottom pb-2 mb-3">
   <i class="bi bi-cpu"></i> ${this.name}
            </h6>
         
   <div class="mb-3">
  <label class="form-label small fw-bold">Node Name</label>
           <input type="text" class="form-control form-control-sm" 
        value="${this.name}" 
   onchange="updateNodeProperty('${this.id}', 'name', this.value)" />
         </div>
        `;
    
        // Render each parameter
        schema.parameters.forEach(param => {
          html += this.renderParameter(param);
        });

        return html;
    }

    /**
   * Render a single parameter field
     * @param {ParameterDefinition} param
     * @returns {string}
     */
    renderParameter(param) {
     const value = this.parameters[param.name] !== undefined 
            ? this.parameters[param.name] 
         : param.defaultValue;
     
        const required = param.required ? '<span class="text-danger">*</span>' : '';
 
        let fieldHtml = `
        <div class="mb-3">
         <label class="form-label small fw-bold">
${param.label} ${required}
                </label>
        ";
        
        switch (param.type) {
            case 'Text':
                fieldHtml += `
             <input type="text" 
     class="form-control form-control-sm" 
value="${value || ''}"
  placeholder="${param.placeholder || ''}"
       onchange="updateNodeParameter('${this.id}', '${param.name}', this.value)" />
   `;
        break;
     
            case 'Number':
  fieldHtml += `
           <input type="number" 
  class="form-control form-control-sm" 
     value="${value || ''}"
       min="${param.minValue || ''}"
      max="${param.maxValue || ''}"
   step="0.1"
               onchange="updateNodeParameter('${this.id}', '${param.name}', parseFloat(this.value))" />
     `;
        break;
      
      case 'Select':
                fieldHtml += `<select class="form-select form-select-sm" 
onchange="updateNodeParameter('${this.id}', '${param.name}', this.value)">`;
            param.options?.forEach(opt => {
           const selected = value === opt.value ? 'selected' : '';
               fieldHtml += `<option value="${opt.value}" ${selected}>${opt.label}</option>`;
  });
           fieldHtml += '</select>';
        break;
        
            // ... other parameter types
        }
        
    if (param.description) {
            fieldHtml += `<small class="form-text text-muted">${param.description}</small>`;
        }
  
   fieldHtml += '</div>';
     return fieldHtml;
    }

    /**
     * Update parameter value
     * @param {string} paramName
     * @param {any} value
     */
 updateParameter(paramName, value) {
    this.parameters[paramName] = value;
    }

    /**
     * Deserialize from JSON
     * @param {object} json
     * @returns {LlmNode}
     */
    static fromJSON(json) {
      const node = new LlmNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position;
  node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
   return node;
    }
}

// Register with registry
nodeRegistry.register('LlmNode', LlmNode);
```

**Tasks for Each Node Type:**
- [ ] Create separate file for each node
- [ ] Implement constructor with defaults
- [ ] Implement `renderProperties()`
- [ ] Implement parameter rendering
- [ ] Implement `fromJSON()` static method
- [ ] Register with NodeRegistry

**Node Types to Implement:**

**AI Nodes (Priority 1):**
- [ ] `LlmNode.js`
- [ ] `PromptBuilderNode.js`
- [ ] `EmbeddingNode.js`
- [ ] `OutputParserNode.js`

**Control Nodes (Priority 2):**
- [ ] `ConditionNode.js`
- [ ] `DelayNode.js`
- [ ] `MergeNode.js`
- [ ] `LogNode.js`

**Data Nodes (Priority 3):**
- [ ] `TransformNode.js`
- [ ] `FilterNode.js`
- [ ] `ChunkTextNode.js`
- [ ] `MemoryNode.js`

**IO Nodes (Priority 4):**
- [ ] `HttpRequestNode.js`

#### 0.4 Create UI Components

**File:** `source/web/wwwroot/js/designer/ui/NodeRenderer.js`

```javascript
/**
 * Responsible for rendering nodes on the canvas
 */
class NodeRenderer {
    /**
     * Render a node to the DOM
     * @param {BaseNode} node
     * @returns {HTMLElement}
     */
    static renderNode(node) {
        const nodeEl = document.createElement('div');
        const isSelected = selectedNodes.has(node.id) || selectedNode?.id === node.id;
 
        nodeEl.className = 'workflow-node' + (isSelected ? ' selected multi-selected' : '');
        nodeEl.style.left = node.position.x + 'px';
    nodeEl.style.top = node.position.y + 'px';
        nodeEl.style.borderColor = node.color;
    nodeEl.dataset.nodeId = node.id;
    
        // Render node header
        let html = `
         <div class="node-header">${node.name}</div>
<div class="node-type">${node.type}</div>
        `;
 
        // Render ports
  html += this.renderPorts(node);
      
        nodeEl.innerHTML = html;
        
        // Attach event handlers
      this.attachEventHandlers(nodeEl, node);
        
   return nodeEl;
    }

    /**
     * Render input and output ports
     * @param {BaseNode} node
* @returns {string}
  */
    static renderPorts(node) {
        const inputPorts = node.getInputPorts();
        const outputPorts = node.getOutputPorts();

        let html = '<div class="ports-container">';
    
        // Render input ports
      inputPorts.forEach((port, index) => {
          const topPercent = this.calculatePortPosition(index, inputPorts.length);
         html += `
            <div class="port input" 
        data-port-id="${port.id}"
     data-port-type="input"
          style="top: ${topPercent}%"
      title="${port.label}">
  </div>
            `;
   });
        
        // Render output ports
        outputPorts.forEach((port, index) => {
 const topPercent = this.calculatePortPosition(index, outputPorts.length);
            html += `
    <div class="port output" 
           data-port-id="${port.id}"
        data-port-type="output"
        style="top: ${topPercent}%"
  title="${port.label}">
             </div>
       `;
   });
        
        html += '</div>';
      return html;
  }

    /**
     * Calculate port position percentage
     * @param {number} index
     * @param {number} total
   * @returns {number}
*/
    static calculatePortPosition(index, total) {
   if (total === 1) return 50;
        const spacing = 40 / (total - 1);
        return 30 + (index * spacing);
    }

    /**
  * Attach event handlers to node element
   * @param {HTMLElement} nodeEl
     * @param {BaseNode} node
     */
    static attachEventHandlers(nodeEl, node) {
      // Node drag handlers
        nodeEl.addEventListener('mousedown', (e) => {
    if (e.target.classList.contains('port')) return;
            handleNodeMouseDown(node, e);
        });
  
        // Port connection handlers
        const ports = nodeEl.querySelectorAll('.port');
        ports.forEach(port => {
            port.addEventListener('mousedown', (e) => {
 e.stopPropagation();
  handlePortMouseDown(node, port, e);
         });
        });
    }
}
```

**Tasks:**
- [ ] Create `NodeRenderer.js`
- [ ] Implement node rendering
- [ ] Implement port rendering
- [ ] Add event handler attachment

#### 0.5 Create Main Orchestrator

**File:** `source/web/wwwroot/js/designer/designer.js`

```javascript
/**
 * Main designer orchestrator
 * Coordinates all components
 */
class WorkflowDesigner {
  constructor(workflowId) {
        this.workflowId = workflowId;
        this.workflow = null;
  this.selectedNode = null;
        this.selectedNodes = new Set();
    }

    /**
 * Initialize the designer
     */
    async initialize() {
// Load schemas
 await nodeRegistry.loadSchemas();
  
        // Load workflow
        await this.loadWorkflow();
        
        // Setup UI
        this.setupPalette();
        this.setupEventListeners();
        
        // Initial render
      this.render();
    }

    /**
     * Load workflow from server
     */
    async loadWorkflow() {
        const response = await fetch(`/Workflow/GetWorkflow/${this.workflowId}`);
        const json = await response.json();
        
        this.workflow = {
   id: json.id,
   name: json.name,
     description: json.description,
            nodes: [],
            connections: json.connections || [],
            variables: json.variables || {}
        };
        
        // Deserialize nodes using registry
        json.nodes?.forEach(nodeJson => {
       const NodeClass = nodeRegistry.nodeTypes.get(nodeJson.type);
            if (NodeClass) {
         const node = NodeClass.fromJSON(nodeJson);
         this.workflow.nodes.push(node);
            } else {
 console.warn(`Unknown node type: ${nodeJson.type}`);
       }
     });
    }

    /**
     * Setup node palette
     */
    setupPalette() {
        const palette = document.getElementById('palette-nodes');
        const nodesByCategory = nodeRegistry.getNodesByCategory();
        
     let html = '';
        for (const [category, nodes] of nodesByCategory) {
            html += `<div class="node-category">${category}</div>`;
         
         nodes.forEach(node => {
                html += `
          <div class="node-item" 
  draggable="true" 
      data-type="${node.type}"
        data-name="${node.name}"
       data-color="${node.color}"
             style="border-left-color: ${node.color}">
          <div class="node-item-name" style="color: ${node.color}">
      ${node.name}
</div>
             <div class="node-item-desc">${node.description}</div>
        </div>
`;
            });
        }
        
        palette.innerHTML = html;
  }

    /**
     * Render entire workflow
  */
    render() {
        this.renderNodes();
        this.renderConnections();
    }

 /**
     * Render all nodes
     */
    renderNodes() {
        const nodesLayer = document.getElementById('nodes-layer');
        nodesLayer.innerHTML = '';
        
        this.workflow.nodes.forEach(node => {
      const nodeEl = NodeRenderer.renderNode(node);
          nodesLayer.appendChild(nodeEl);
        });
    }

    /**
     * Add a new node
     * @param {string} type
     * @param {string} name
     * @param {number} x
     * @param {number} y
     */
    addNode(type, name, x, y) {
        const node = nodeRegistry.createNode(type, name, x, y);
        this.workflow.nodes.push(node);
        this.render();
      this.selectNode(node.id);
    }

    /**
  * Select a node
   * @param {string} nodeId
   */
    selectNode(nodeId) {
        this.selectedNodes.clear();
        this.selectedNode = this.workflow.nodes.find(n => n.id === nodeId);
        this.selectedNodes.add(nodeId);
        this.render();
        this.renderProperties();
    }

    /**
     * Render properties panel
     */
    renderProperties() {
        if (!this.selectedNode) return;
   
    const panel = document.getElementById('properties-content');
 panel.innerHTML = this.selectedNode.renderProperties();
    }

    /**
     * Save workflow
     */
    async save() {
        const json = {
      id: this.workflow.id,
         name: this.workflow.name,
            description: this.workflow.description,
  nodes: this.workflow.nodes.map(n => n.toJSON()),
       connections: this.workflow.connections,
            variables: this.workflow.variables
        };
  
        const response = await fetch('/Workflow/SaveWorkflow', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(json)
        });
        
        const result = await response.json();
        if (result.success) {
      showSuccessMessage('Workflow saved successfully');
    } else {
            showErrorMessage('Error saving workflow: ' + result.error);
        }
    }
}

// Initialize when page loads
let designer;
async function initializeDesigner(workflowId) {
    designer = new WorkflowDesigner(workflowId);
    await designer.initialize();
}
```

**Tasks:**
- [ ] Create `designer.js`
- [ ] Implement initialization
- [ ] Implement node management
- [ ] Implement save/load

#### 0.6 Update HTML to Load New Scripts

**File:** `source/web/Views/Workflow/Designer.cshtml`

```html
<!-- Load in order -->
<script src="~/js/designer/core/Constants.js"></script>
<script src="~/js/designer/core/WorkflowData.js"></script>
<script src="~/js/designer/core/BaseNode.js"></script>
<script src="~/js/designer/core/NodeRegistry.js"></script>

<!-- Load AI nodes -->
<script src="~/js/designer/nodes/ai/LlmNode.js"></script>
<script src="~/js/designer/nodes/ai/PromptBuilderNode.js"></script>
<script src="~/js/designer/nodes/ai/EmbeddingNode.js"></script>
<script src="~/js/designer/nodes/ai/OutputParserNode.js"></script>

<!-- Load Control nodes -->
<script src="~/js/designer/nodes/control/ConditionNode.js"></script>
<script src="~/js/designer/nodes/control/DelayNode.js"></script>
<script src="~/js/designer/nodes/control/MergeNode.js"></script>
<script src="~/js/designer/nodes/control/LogNode.js"></script>

<!-- Load Data nodes -->
<script src="~/js/designer/nodes/data/TransformNode.js"></script>
<script src="~/js/designer/nodes/data/FilterNode.js"></script>
<script src="~/js/designer/nodes/data/ChunkTextNode.js"></script>
<script src="~/js/designer/nodes/data/MemoryNode.js"></script>

<!-- Load IO nodes -->
<script src="~/js/designer/nodes/io/HttpRequestNode.js"></script>

<!-- Load UI components -->
<script src="~/js/designer/ui/NodeRenderer.js"></script>
<script src="~/js/designer/ui/ConnectionManager.js"></script>
<script src="~/js/designer/ui/PropertiesPanel.js"></script>
<script src="~/js/designer/ui/Canvas.js"></script>

<!-- Load utilities -->
<script src="~/js/designer/utils/validation.js"></script>
<script src="~/js/designer/utils/serialization.js"></script>
<script src="~/js/designer/utils/helpers.js"></script>

<!-- Main orchestrator -->
<script src="~/js/designer/designer.js"></script>

<script>
    const workflowId = '@Model.Id';
    initializeDesigner(workflowId);
</script>
```

**Tasks:**
- [ ] Update Designer.cshtml script references
- [ ] Ensure correct loading order
- [ ] Test that all scripts load

### Acceptance Criteria
- ? All node types have dedicated JS classes
- ? Each node can render its own properties panel
- ? Node palette populates from registry
- ? Can create, edit, and delete nodes
- ? Serialization/deserialization works correctly
- ? Backward compatible with existing workflows
- ? No console errors
- ? JSDoc annotations provide IntelliSense

### Testing Checklist
- [ ] Test creating each node type
- [ ] Test node validation
- [ ] Test parameter updates
- [ ] Test serialization round-trip (save/load)
- [ ] Test with existing workflows
- [ ] Test node cloning
- [ ] Test node deletion
- [ ] Visual regression testing

### Benefits of This Architecture

1. **Maintainability**
   - Each node type is self-contained
   - Easy to find and fix bugs
   - Clear separation of concerns

2. **Scalability**
   - Adding new node types is straightforward
   - No risk of breaking existing nodes
   - Can add node-specific features easily

3. **Testability**
   - Can unit test individual nodes
   - Mock dependencies easily
   - Test validation in isolation

4. **Developer Experience**
   - IntelliSense from JSDoc
   - Clear code organization
   - Easy to onboard new developers

5. **Future-Proofing**
   - Foundation for TypeScript migration
   - Supports complex features (multi-port, conditions)
   - Clean base for advanced UI features

---

## ?? Phase 1: Node Schema Enhancement

**Timeline:** Week 2  
**Priority:** High  
**Dependencies:** Phase 0

### Objectives
- Extend node schema system to support multiple input/output ports
- Add port type definitions (data, control, conditional)
- Define node capabilities (retry, timeout, multi-port)
- Add execution options to node schema

### Tasks

#### 1.1 Backend Schema Model Updates

**File:** `source/web/Models/NodeParameterSchema.cs`

- [ ] Add `PortDefinition` class
  ```csharp
  public class PortDefinition
  {
      public string Id { get; set; } = string.Empty;
      public string Label { get; set; } = string.Empty;
      public string Type { get; set; } = "data"; // data, control, conditional
      public bool Required { get; set; }
      public string? Condition { get; set; } // For conditional outputs
  }
  ```

- [ ] Add `NodeCapabilities` class
  ```csharp
  public class NodeCapabilities
  {
      public bool SupportsMultipleInputs { get; set; }
      public bool SupportsMultipleOutputs { get; set; }
      public bool SupportsConditionalRouting { get; set; }
      public bool SupportsRetry { get; set; }
      public bool SupportsTimeout { get; set; }
      public bool SupportsSubWorkflow { get; set; } // For loop containers
  }
  ```

- [ ] Add `ExecutionOption` class
  ```csharp
  public class ExecutionOption
  {
      public string Name { get; set; } = string.Empty;
      public string Label { get; set; } = string.Empty;
      public ParameterType Type { get; set; }
      public object? DefaultValue { get; set; }
      public string Description { get; set; } = string.Empty;
  }
  ```

- [ ] Extend `NodeParameterSchema` with:
  - `List<PortDefinition> InputPorts`
  - `List<PortDefinition> OutputPorts`
  - `NodeCapabilities Capabilities`
  - `List<ExecutionOption> ExecutionOptions`

#### 1.2 Update Node Schemas

**File:** `source/web/Services/NodeSchemaProvider.cs`

- [ ] Update **ConditionNode** schema with conditional ports
- [ ] Update **LlmNode** schema with retry/timeout options
- [ ] Update **HttpRequestNode** schema with timeout options
- [ ] Add port definitions to all existing node types

#### 1.3 New Parameter Types

- [ ] Add `KeyValueList` parameter type
  - For ConditionNode conditions
  - UI: Dynamic key-value pair editor

- [ ] Add `ExpressionBuilder` parameter type
  - Visual expression editor
  - UI: Dropdown + input builder

- [ ] Add `PortMapper` parameter type
  - Map input/output port connections
  - UI: Port selection dropdown

### Acceptance Criteria
- ? All node schemas include port definitions
- ? ConditionNode has true/false output ports defined
- ? Execution options are defined in schema
- ? API endpoint returns enhanced schemas
- ? No breaking changes to existing workflows

### Testing Checklist
- [ ] Unit tests for new schema classes
- [ ] Integration tests for API endpoints
- [ ] Verify existing workflows still load correctly
- [ ] Test schema validation

---

## ?? Phase 2: Visual Node Enhancements

**Timeline:** Week 3  
**Priority:** High  
**Dependencies:** Phase 1

### Objectives
- Implement multi-port visual rendering
- Add port labels and tooltips
- Implement conditional port styling (true/false branches)
- Add connection validation based on port types

### Tasks

#### 2.1 CSS Updates

**File:** `source/web/wwwroot/css/designer.css`

- [ ] Add `.ports-container` styling
- [ ] Add multi-port layout (vertical stacking)
- [ ] Add `.port-label` styling for input/output labels
- [ ] Add `.conditional-true` and `.conditional-false` port colors
- [ ] Add port hover effects and tooltips

**Example CSS:**
```css
/* Input ports - left side, stacked vertically */
.workflow-node .port.input {
    left: -6px;
}

.workflow-node .port.input:nth-child(1) { top: 30%; }
.workflow-node .port.input:nth-child(2) { top: 50%; }
.workflow-node .port.input:nth-child(3) { top: 70%; }

/* Conditional ports - distinct colors */
.workflow-node .port.conditional-true {
    background: #27ae60;
}

.workflow-node .port.conditional-false {
 background: #e74c3c;
}

/* Port labels */
.port-label {
    position: absolute;
    font-size: 0.7rem;
    white-space: nowrap;
color: #6c757d;
}
```

#### 2.2 JavaScript Rendering Updates

**File:** `source/web/wwwroot/js/designer/nodes.js`

- [ ] Update `renderNode()` to use schema port definitions
- [ ] Implement dynamic port rendering based on node type
- [ ] Add port labels from schema
- [ ] Apply conditional port styling

**Example Implementation:**
```javascript
function renderNodePorts(node, schema) {
  let portsHtml = '<div class="ports-container">';
    
    // Render input ports
    schema.inputPorts?.forEach((port, index) => {
      const topPercent = 30 + (index * 20);
        portsHtml += `
     <div class="port input" 
        data-port-id="${port.id}"
         data-port-type="input"
       style="top: ${topPercent}%"
       title="${port.label}">
     </div>
<div class="port-label input-label" style="top: ${topPercent}%">
     ${port.label}
      </div>
   `;
    });
    
    // Render output ports
    schema.outputPorts?.forEach((port, index) => {
      const topPercent = 30 + (index * 20);
      const conditionalClass = port.type === 'conditional' ? 
    `conditional-${port.condition?.startsWith('!') ? 'false' : 'true'}` : '';
        
     portsHtml += `
  <div class="port output ${conditionalClass}" 
    data-port-id="${port.id}"
     data-port-type="output"
           style="top: ${topPercent}%"
                 title="${port.label}">
   </div>
 <div class="port-label output-label" style="top: ${topPercent}%">
  ${port.label}
     </div>
      `;
    });
    
    portsHtml += '</div>';
    return portsHtml;
}
```

#### 2.3 Connection Validation

**File:** `source/web/wwwroot/js/designer/connections.js`

- [ ] Implement `canConnect()` validation function
- [ ] Check port type compatibility
- [ ] Prevent duplicate connections
- [ ] Validate connection direction (output ? input)
- [ ] Show visual feedback for valid/invalid connections

```javascript
function canConnect(sourceNode, sourcePortId, targetNode, targetPortId) {
    // Can't connect node to itself
    if (sourceNode.id === targetNode.id) return false;
    
    // Check if connection already exists
    const exists = workflow.connections.some(c => 
     c.sourceNodeId === sourceNode.id && 
  c.sourcePortId === sourcePortId &&
        c.targetNodeId === targetNode.id &&
        c.targetPortId === targetPortId
    );
    if (exists) return false;
    
    // Get port schemas
    const sourceSchema = nodeSchemas[sourceNode.type];
    const targetSchema = nodeSchemas[targetNode.type];
    
    const sourcePort = sourceSchema.outputPorts.find(p => p.id === sourcePortId);
    const targetPort = targetSchema.inputPorts.find(p => p.id === targetPortId);
  
    if (!sourcePort || !targetPort) return false;
    
    // Add type compatibility check here (future enhancement)
    return true;
}
```

#### 2.4 Connection Model Updates

**File:** `source/web/Models/WorkflowDefinition.cs`

- [ ] Update `ConnectionDefinition` to include port IDs
  ```csharp
  public class ConnectionDefinition
  {
public Guid Id { get; set; }
      public Guid SourceNodeId { get; set; }
      public string SourcePortId { get; set; } = "output"; // NEW
   public Guid TargetNodeId { get; set; }
      public string TargetPortId { get; set; } = "input"; // NEW
  }
  ```

### Acceptance Criteria
- ? Nodes render with correct number of ports based on schema
- ? Port labels are visible and readable
- ? Conditional ports (true/false) have distinct colors
- ? Connection validation prevents invalid connections
- ? Existing workflows with legacy single-port connections still work

### Testing Checklist
- [ ] Test ConditionNode with true/false ports
- [ ] Test connection validation logic
- [ ] Test port hover effects and tooltips
- [ ] Test backward compatibility with existing workflows
- [ ] Visual regression tests for node rendering

---

## ?? Phase 3: Condition Node Enhancement

**Timeline:** Week 4  
**Priority:** High  
**Dependencies:** Phase 2

### Objectives
- Create visual condition builder UI
- Support multiple condition expressions
- Dynamic output port creation based on conditions
- Expression parsing and validation

### Tasks

#### 3.1 Condition Builder UI Component

**File:** `source/web/wwwroot/js/designer/conditionBuilder.js` (NEW)

- [ ] Create `renderConditionBuilder()` function
- [ ] Implement add/remove condition UI
- [ ] Create condition expression builder
  - Variable selector
  - Operator dropdown (==, !=, >, <, >=, <=, contains)
  - Value input
- [ ] Parse existing conditions from JSON
- [ ] Build condition expressions from UI inputs

**UI Structure:**
```javascript
function renderConditionBuilder(node) {
    return `
        <div id="condition-builder">
            <h6 class="small fw-bold mb-2">Conditions</h6>
    <div id="conditions-list">
                <!-- Condition items -->
   <div class="condition-item">
  <div class="row g-2 mb-2">
 <div class="col-3">
              <label class="small">Output Key</label>
               <input type="text" class="form-control form-control-sm" 
   placeholder="is_positive" />
                </div>
    <div class="col-3">
  <label class="small">Variable</label>
        <select class="form-select form-select-sm">
 <option value="sentiment">{{sentiment}}</option>
          <option value="score">{{score}}</option>
   </select>
         </div>
       <div class="col-2">
         <label class="small">Operator</label>
           <select class="form-select form-select-sm">
   <option value="==">equals</option>
             <option value="!=">not equals</option>
      <option value=">">greater than</option>
        </select>
       </div>
        <div class="col-3">
           <label class="small">Value</label>
                  <input type="text" class="form-control form-control-sm" />
               </div>
    <div class="col-1 d-flex align-items-end">
      <button class="btn btn-sm btn-danger">
            <i class="bi bi-trash"></i>
            </button>
          </div>
     </div>
 </div>
  </div>
            <button class="btn btn-sm btn-primary mt-2">
     <i class="bi bi-plus-circle"></i> Add Condition
         </button>
      </div>
    `;
}
```

#### 3.2 Expression Parser

- [ ] Implement `parseConditionExpression(expr)` function
  - Parse: `"{{sentiment}} == 'positive'"` ? `{ variable, operator, value }`
- [ ] Implement `buildConditionExpression(variable, operator, value)` function
  - Build: `{ variable, operator, value }` ? `"{{sentiment}} == 'positive'"`
- [ ] Add expression validation
- [ ] Support complex expressions (AND/OR - future)

#### 3.3 Dynamic Port Management

- [ ] Update `renderNodePorts()` to check for dynamic ports
- [ ] For ConditionNode, create output ports based on conditions
- [ ] Update connection validation for conditional ports
- [ ] Add visual indicators for conditional branches

**Example:**
```javascript
function updateConditionNodePorts(nodeId) {
    const node = workflow.nodes.find(n => n.id === nodeId);
    if (!node || node.type !== 'ConditionNode') return;
    
    const conditions = node.parameters.conditions || {};
    
    // Create output ports for each condition
    node._runtime = node._runtime || {};
    node._runtime.outputPorts = Object.keys(conditions).map(key => ({
      id: key,
        label: key,
        type: 'conditional',
  condition: key
    }));
    
    render();
}
```

#### 3.4 Properties Panel Integration

**File:** `source/web/wwwroot/js/designer/properties.js`

- [ ] Update `renderProperties()` to detect ConditionNode
- [ ] Render condition builder instead of raw JSON editor
- [ ] Auto-update ports when conditions change
- [ ] Validate conditions before saving

### Acceptance Criteria
- ? Condition builder UI renders for ConditionNode
- ? Can add/remove conditions visually
- ? Conditions are stored in correct JSON format
- ? Output ports are created dynamically
- ? Connections work with conditional ports
- ? Expression validation prevents invalid syntax

### Testing Checklist
- [ ] Test adding multiple conditions
- [ ] Test removing conditions
- [ ] Test expression parsing/building
- [ ] Test port updates after condition changes
- [ ] Test connections to conditional ports
- [ ] Test with existing workflows

---

## ?? Phase 4: Node Execution Options UI

**Timeline:** Week 5  
**Priority:** Medium  
**Dependencies:** Phase 1

### Objectives
- Add "Advanced Options" panel to properties
- Support retry configuration (maxRetries, retryDelay)
- Support timeout configuration
- Support error handling options (continueOnError)
- Support conditional execution (runCondition)

### Tasks

#### 4.1 Execution Options Data Model

**File:** `source/web/Models/NodeDefinition.cs` (UPDATE)

- [ ] Add `ExecutionOptions` property to `NodeDefinition`
  ```csharp
  public class NodeDefinition
  {
      // ...existing properties...

      public NodeExecutionOptions? ExecutionOptions { get; set; }
  }
  
  public class NodeExecutionOptions
  {
   public int MaxRetries { get; set; } = 0;
  public int RetryDelayMs { get; set; } = 1000;
   public int? TimeoutMs { get; set; }
      public bool ContinueOnError { get; set; } = false;
      public string? RunCondition { get; set; } // e.g., "{{should_run}} == true"
      public WorkflowData? FallbackData { get; set; }
  }
  ```

#### 4.2 UI Component - Execution Options Panel

**File:** `source/web/wwwroot/js/designer/properties.js`

- [ ] Create `renderExecutionOptions()` function
- [ ] Implement collapsible accordion panel
- [ ] Add input fields for all options
- [ ] Add validation and hints

**UI Structure:**
```javascript
function renderExecutionOptions(node) {
    const options = node.executionOptions || {
        maxRetries: 0,
        retryDelayMs: 1000,
        timeoutMs: null,
  continueOnError: false,
        runCondition: null
    };
    
    return `
        <div class="accordion mt-3" id="execution-options">
       <div class="accordion-item">
    <h2 class="accordion-header">
    <button class="accordion-button collapsed" type="button" 
       data-bs-toggle="collapse" data-bs-target="#advanced-options">
       ?? Advanced Execution Options
        </button>
        </h2>
         <div id="advanced-options" class="accordion-collapse collapse">
       <div class="accordion-body">
            <!-- Retry Settings -->
                  <div class="mb-3">
      <label class="form-label small fw-bold">Max Retries</label>
        <input type="number" class="form-control form-control-sm" 
                    value="${options.maxRetries}" min="0" max="10"
             onchange="updateExecutionOption('${node.id}', 'maxRetries', parseInt(this.value))" />
       <small class="text-muted">Number of retry attempts on failure</small>
     </div>
      
   <!-- Additional options... -->
     </div>
             </div>
            </div>
        </div>
`;
}
```

#### 4.3 JavaScript Event Handlers

- [ ] Implement `updateExecutionOption(nodeId, optionName, value)`
- [ ] Validate numeric inputs
- [ ] Validate run condition expressions
- [ ] Auto-save on change

#### 4.4 Visual Indicators

- [ ] Add badge to node if execution options are set
- [ ] Show retry icon if maxRetries > 0
- [ ] Show timeout icon if timeout is set
- [ ] Show error-handling icon if continueOnError = true

### Acceptance Criteria
- ? Execution options panel is accessible for all nodes
- ? All options save correctly to node model
- ? Visual indicators appear on nodes with options
- ? Run conditions support variable interpolation
- ? Validation prevents invalid values

### Testing Checklist
- [ ] Test setting all execution options
- [ ] Test validation (negative numbers, invalid expressions)
- [ ] Test visual indicators
- [ ] Test saving and reloading workflows
- [ ] Test with different node types

---

## ?? Phase 5: Special Node Types (Loop & Parallel)

**Timeline:** Week 6-7  
**Priority:** High  
**Dependencies:** Phases 1-4

### Objectives
- Create ForEach/Loop container node
- Create Parallel execution node
- Implement sub-workflow rendering
- Support nested workflows

### Tasks

#### 5.1 ForEach Loop Container Node

**Schema:**
```csharp
["ForEachNode"] = new()
{
    NodeType = "ForEachNode",
    Category = "Control",
  Capabilities = new()
    {
        SupportsSubWorkflow = true,
        SupportsMultipleInputs = false
    },
    Parameters = new()
    {
        new() { Name = "itemsKey", Label = "Items Variable", 
 Type = ParameterType.Text, Required = true, 
     Placeholder = "{{items}}", Description = "Variable containing array to iterate" },
        new() { Name = "outputKey", Label = "Output Variable", 
            Type = ParameterType.Text, Required = true, 
     DefaultValue = "processed_items" },
     new() { Name = "itemVariableName", Label = "Loop Item Variable", 
 Type = ParameterType.Text, DefaultValue = "__loop_item__",
           Description = "Variable name for current item in loop body" }
  }
}
```

**UI Tasks:**
- [ ] Create loop container visual style
- [ ] Implement sub-workflow canvas area
- [ ] Support drag-drop nodes into loop body
- [ ] Show loop configuration (items, output)
- [ ] Visual indicator for loop item variable

**CSS:**
```css
.workflow-node.loop-container {
    min-width: 350px;
    min-height: 250px;
    background: linear-gradient(135deg, #fff 0%, #f8f9fa 100%);
    border: 2px dashed #F5A623;
}

.loop-body-canvas {
    background: #fafafa;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    min-height: 150px;
    padding: 10px;
  margin: 10px;
}

.empty-loop-hint {
    color: #adb5bd;
    text-align: center;
    padding: 30px;
    font-size: 0.85rem;
}
```

#### 5.2 Parallel Execution Node

**Schema:**
```csharp
["ParallelNode"] = new()
{
    NodeType = "ParallelNode",
    Category = "Control",
 Capabilities = new()
    {
    SupportsMultipleOutputs = true,
  SupportsDynamicPorts = true
    },
    Parameters = new()
    {
        new() { Name = "branchCount", Label = "Number of Parallel Branches", 
         Type = ParameterType.Number, DefaultValue = 2, 
          MinValue = 2, MaxValue = 10 }
    }
}
```

**UI Tasks:**
- [ ] Render node with multiple output ports (one per branch)
- [ ] Dynamically update ports based on branchCount
- [ ] Show branch labels (Branch 1, Branch 2, etc.)
- [ ] Support connecting each branch to different node chains

#### 5.3 Sub-Workflow Management

- [ ] Implement nested workflow data structure
  ```javascript
  {
      id: "loop-node-1",
      type: "ForEachNode",
      parameters: { ... },
      subWorkflow: {
          nodes: [ ... ],
          connections: [ ... ]
      }
  }
```

- [ ] Implement sub-workflow rendering
- [ ] Handle drag-drop into sub-workflow areas
- [ ] Prevent infinite nesting
- [ ] Validate sub-workflow connections

### Acceptance Criteria
- ? ForEach node renders as expandable container
- ? Can drag nodes into loop body
- ? Loop configuration is editable
- ? Parallel node shows correct number of output ports
- ? Sub-workflows save/load correctly

### Testing Checklist
- [ ] Test loop node creation and configuration
- [ ] Test adding nodes to loop body
- [ ] Test parallel node with 2-10 branches
- [ ] Test nested workflow serialization
- [ ] Test complex workflows with loops and branches

---

## ?? Phase 6: Variable System Enhancement

**Timeline:** Week 7-8  
**Priority:** Medium  
**Dependencies:** None (parallel to other phases)

### Objectives
- Add variable types (string, number, boolean, JSON, array)
- Implement variable scoping (global vs node-scoped)
- Add variable validation
- Support computed variables

### Tasks

#### 6.1 Typed Variables

**File:** `source/web/Models/WorkflowDefinition.cs`

- [ ] Update `Variables` property to support typed values
  ```csharp
  public class WorkflowVariable
  {
      public string Name { get; set; } = string.Empty;
  public VariableType Type { get; set; } = VariableType.String;
      public object? DefaultValue { get; set; }
      public VariableScope Scope { get; set; } = VariableScope.Global;
      public bool IsComputed { get; set; } = false;
      public string? ComputedExpression { get; set; }
  }
  
  public enum VariableType
  {
    String,
      Number,
      Boolean,
      Json,
      Array
  }
  
  public enum VariableScope
  {
      Global,     // Available to all nodes
      NodeScoped  // Created by specific node (e.g., loop item)
  }
  ```

#### 6.2 Variable UI Enhancements

- [ ] Add variable type selector in add variable form
- [ ] Add type-specific input validation
- [ ] Show variable type in variables list
- [ ] Add variable scope indicator
- [ ] Support JSON editor for JSON type variables

#### 6.3 Variable Validation

- [ ] Validate variable types when used in parameters
- [ ] Show warnings for type mismatches
- [ ] Validate variable names (no duplicates, valid identifiers)
- [ ] Validate JSON syntax for JSON variables

#### 6.4 Computed Variables

- [ ] Add "Computed" checkbox to variable form
- [ ] Add expression editor for computed variables
- [ ] Show computed variables distinctly in list
- [ ] Validate computed expressions

### Acceptance Criteria
- ? Can create variables with specific types
- ? Type validation works for all parameter fields
- ? Variable scoping is enforced
- ? Computed variables can reference other variables
- ? UI clearly shows variable types and scopes

### Testing Checklist
- [ ] Test creating all variable types
- [ ] Test type validation in parameters
- [ ] Test computed variables
- [ ] Test variable name validation
- [ ] Test JSON variable editor

---

## ?? Phase 7: Workflow Execution & Debugging

**Timeline:** Week 8-9  
**Priority:** High  
**Dependencies:** All previous phases

### Objectives
- Execute workflows directly from designer
- Real-time execution visualization
- Step-by-step debugging
- Variable inspection
- Execution history

### Tasks

#### 7.1 Backend Execution API

**File:** `source/web/Controllers/WorkflowController.cs`

- [ ] Create `ExecuteWorkflow` API endpoint
  ```csharp
  [HttpPost]
  public async Task<IActionResult> ExecuteWorkflow([FromBody] ExecuteWorkflowRequest request)
  {
      var workflowDef = request.WorkflowDefinition;
   var inputData = request.InputData;
      
      // Build workflow from definition
    var workflow = _workflowBuilder.BuildFromDefinition(workflowDef);
   
      // Execute
      var result = await workflow.RunAsync(WorkflowData.From(inputData));
      
      return Json(new
  {
  success = result.IsSuccess,
          output = result.Data.ToJson(),
      nodeResults = result.NodeResults.Select(nr => new
  {
      nodeId = GetNodeIdFromName(nr.NodeName), // Map back to designer node
       nodeName = nr.NodeName,
     status = nr.Status.ToString(),
      duration = nr.Duration.TotalMilliseconds,
       logs = nr.Logs,
        metadata = nr.Metadata
          })
      });
  }
  ```

- [ ] Create `WorkflowBuilder` service to convert JSON definition to C# workflow

#### 7.2 Execution UI

**File:** `source/web/wwwroot/js/designer/execution.js` (NEW)

- [ ] Create execution panel UI
- [ ] Add input data form
- [ ] Add "Run Workflow" button to toolbar
- [ ] Show execution progress
- [ ] Display results panel

**UI Components:**
```javascript
async function runWorkflow() {
 // Show input dialog
    const inputData = await promptForInputData();
    if (!inputData) return;
    
    // Show loading state
    showExecutionProgress();
    
    try {
        const response = await fetch('/Workflow/ExecuteWorkflow', {
      method: 'POST',
     headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
   workflowDefinition: workflow,
      inputData: inputData
         })
        });
        
        const result = await response.json();
        showExecutionResults(result);
    } catch (error) {
        showExecutionError(error);
    }
}

function showExecutionResults(result) {
    // Highlight nodes based on execution status
    result.nodeResults.forEach(nr => {
        const nodeEl = document.querySelector(`[data-node-id="${nr.nodeId}"]`);
 if (nodeEl) {
          nodeEl.classList.remove('node-pending', 'node-running', 'node-success', 'node-failed');
   nodeEl.classList.add(`node-${nr.status.toLowerCase()}`);
        }
    });
    
    // Open results panel
    openExecutionResultsPanel(result);
}
```

#### 7.3 Real-Time Execution Visualization

- [ ] Add execution state classes to CSS
  ```css
  .workflow-node.node-pending { opacity: 0.5; }
  .workflow-node.node-running { 
      border-color: #F5A623;
      animation: pulse 1s infinite;
  }
  .workflow-node.node-success { 
      border-color: #27ae60;
      background: #f0fff4;
  }
  .workflow-node.node-failed { 
      border-color: #e74c3c;
    background: #fff5f5;
  }
  ```

- [ ] Animate connections during execution
- [ ] Show execution time on each node
- [ ] Display node output data on hover

#### 7.4 Debugging Features

- [ ] Add breakpoint support (click node to add/remove)
- [ ] Implement "Step Over" functionality
- [ ] Add variable inspection panel
- [ ] Show execution trace/timeline
- [ ] Support execution history

**Breakpoints:**
```javascript
function toggleBreakpoint(nodeId) {
 const node = workflow.nodes.find(n => n.id === nodeId);
    if (!node) return;
    
    node.breakpoint = !node.breakpoint;
    render();
}

// Visual indicator
.workflow-node.has-breakpoint::before {
    content: '?';
    position: absolute;
    top: -10px;
    right: -10px;
    color: #e74c3c;
    font-size: 1.5rem;
}
```

#### 7.5 Execution Results Panel

- [ ] Create sliding panel from right
- [ ] Show execution summary (success/failed, duration)
- [ ] List all node results with status
- [ ] Display node logs
- [ ] Show final output data
- [ ] Add "Copy Output" button
- [ ] Add "Export Results" button (JSON)

### Acceptance Criteria
- ? Can execute workflow from designer
- ? Real-time visual feedback during execution
- ? Execution results are displayed clearly
- ? Can inspect node outputs and logs
- ? Breakpoints pause execution
- ? Step-by-step debugging works

### Testing Checklist
- [ ] Test simple sequential workflow execution
- [ ] Test workflow with branches
- [ ] Test workflow with loops
- [ ] Test error handling and failed nodes
- [ ] Test breakpoints and step-over
- [ ] Test variable inspection
- [ ] Test execution with invalid data

---

## ?? Phase 8: Export/Import & Code Generation

**Timeline:** Week 9-10  
**Priority:** Medium  
**Dependencies:** All previous phases

### Objectives
- Generate clean C# code from visual design
- Export workflows to various formats (JSON, YAML, C#)
- Import existing C# workflows (future)
- Generate workflow templates

### Tasks

#### 8.1 C# Code Generation

**File:** `source/web/wwwroot/js/designer/codeGen.js` (NEW)

- [ ] Implement `generateCSharpCode()` function
  ```javascript
function generateCSharpCode() {
      let code = `// Generated by TWF AI Framework Designer\n`;
      code += `// Date: ${new Date().toISOString()}\n\n`;
      code += `using TwfAiFramework.Core;\n`;
      code += `using TwfAiFramework.Nodes.*;\n\n`;
      code += `var workflow = Workflow.Create("${workflow.name}")\n`;
      
      // Generate nodes
    workflow.nodes.forEach(node => {
    code += generateNodeCode(node);
      });
 
      // Generate execution options if any
      if (hasExecutionOptions()) {
 code += generateExecutionOptionsCode();
      }
      
      code += `.RunAsync(initialData);\n`;
      
      return code;
  }
  
  function generateNodeCode(node) {
      const schema = nodeSchemas[node.type];
      const params = buildNodeParameters(node);
   
      let code = `    .AddNode(new ${node.type}("${node.name}"`;
      if (params.length > 0) {
      code += `, ${params}`;
  }
      code += `)`;
      
   // Add execution options if present
      if (node.executionOptions && hasNonDefaultOptions(node.executionOptions)) {
 code += `, ${generateNodeOptions(node.executionOptions)}`;
      }
      
      code += `)\n`;
   return code;
  }
  ```

- [ ] Implement parameter value formatting
- [ ] Handle variable interpolation in code
- [ ] Generate branching code (if/else)
- [ ] Generate loop code (ForEach)
- [ ] Add code comments and formatting

#### 8.2 Export Functionality

**Formats:**
- [ ] **JSON Export** (native format)
  - Already implemented
  - Enhance with metadata and version

- [ ] **C# Code Export**
  - Add "Export to C#" button
  - Show code in modal with syntax highlighting
  - Add "Copy to Clipboard" button
  - Add "Download .cs file" button

- [ ] **YAML Export** (optional)
  - Human-readable format
  - Useful for documentation

**UI:**
```javascript
function showExportDialog() {
    const modal = `
   <div class="modal fade" id="export-modal">
            <div class="modal-dialog modal-lg">
    <div class="modal-content">
         <div class="modal-header">
          <h5>Export Workflow</h5>
</div>
         <div class="modal-body">
  <ul class="nav nav-tabs mb-3">
       <li class="nav-item">
             <a class="nav-link active" data-tab="json">JSON</a>
        </li>
         <li class="nav-item">
     <a class="nav-link" data-tab="csharp">C# Code</a>
                </li>
     </ul>
         <div class="tab-content">
  <pre id="export-content"></pre>
        </div>
            </div>
        <div class="modal-footer">
           <button class="btn btn-primary" onclick="copyExport()">
      Copy to Clipboard
   </button>
    <button class="btn btn-success" onclick="downloadExport()">
  Download File
         </button>
      </div>
             </div>
            </div>
        </div>
    `;
    // Show modal...
}
```

#### 8.3 Import Functionality

- [ ] Import from JSON (already working)
- [ ] Validate imported workflows
- [ ] Migrate old workflow versions
- [ ] Import from C# code (future enhancement)

#### 8.4 Workflow Templates

- [ ] Create template system
- [ ] Pre-built workflow templates:
  - Customer Support Chatbot
  - Document Q&A (RAG)
  - Content Generation Pipeline
  - Data Processing Workflow
- [ ] Template browser UI
- [ ] "Create from Template" functionality

### Acceptance Criteria
- ? Generated C# code is syntactically correct
- ? Generated code matches manual implementations
- ? Export dialog supports multiple formats
- ? Can copy and download generated code
- ? Import validates and handles errors gracefully
- ? Templates are accessible and work correctly

### Testing Checklist
- [ ] Test code generation for all node types
- [ ] Test code generation with branches
- [ ] Test code generation with loops
- [ ] Test code generation with execution options
- [ ] Compile generated code and verify it works
- [ ] Test export/import round-trip
- [ ] Test all workflow templates

---

## ?? Quick Start Guide (For Developers)

### Setting Up Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/techwayfit/Twf_AI_Framework.git
   cd Twf_AI_Framework
   ```

2. **Open solution in Visual Studio 2024 or VS Code**
   ```bash
   code .
   # or
   start TwfAiFramework.sln
   ```

3. **Run the web project**
   ```bash
   cd source/web
   dotnet run
   ```

4. **Navigate to Designer**
   - Open browser: `https://localhost:5001/Workflow`
   - Create new workflow
   - Open Designer

### Working on Phases

#### Phase 0 Checklist
- [ ] Refactor JavaScript files into new architecture
- [ ] Implement base classes and registry
- [ ] Migrate existing node types to new structure
- [ ] Update HTML to load new scripts

#### Phase 1 Checklist
- [ ] Update `NodeParameterSchema.cs`
- [ ] Update `NodeSchemaProvider.cs`
- [ ] Test API endpoint `/Workflow/GetAllNodeSchemas`
- [ ] Verify schema structure in browser console

#### Phase 2 Checklist
- [ ] Update `designer.css`
- [ ] Update `nodes.js`
- [ ] Update `connections.js`
- [ ] Test multi-port rendering
- [ ] Test connection validation

### Debugging Tips

**JavaScript Console:**
```javascript
// Inspect current workflow
console.log(workflow);

// Inspect node schemas
console.log(nodeSchemas);

// Test connection validation
console.log(canConnect(sourceNode, 'output', targetNode, 'input'));

// Generate code
console.log(generateCSharpCode());
```

**Backend Debugging:**
- Set breakpoints in `WorkflowController.cs`
- Inspect `NodeSchemaProvider.GetAllSchemas()`
- Verify model serialization

---

## ?? Reference Documentation

### Key Files Reference

| File | Purpose | Phase |
|------|---------|-------|
| `Models/NodeParameterSchema.cs` | Node schema definitions | 1 |
| `Services/NodeSchemaProvider.cs` | Schema provider service | 1 |
| `wwwroot/css/designer.css` | Designer styles | 2 |
| `wwwroot/js/designer/nodes.js` | Node rendering | 2 |
| `wwwroot/js/designer/connections.js` | Connection logic | 2 |
| `wwwroot/js/designer/conditionBuilder.js` | Condition builder UI | 3 |
| `wwwroot/js/designer/properties.js` | Properties panel | 4 |
| `wwwroot/js/designer/execution.js` | Execution & debugging | 7 |
| `wwwroot/js/designer/codeGen.js` | Code generation | 8 |

### API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/Workflow/GetAvailableNodes` | GET | Get node palette items |
| `/Workflow/GetAllNodeSchemas` | GET | Get all node schemas |
| `/Workflow/GetWorkflow/{id}` | GET | Load workflow definition |
| `/Workflow/SaveWorkflow` | POST | Save workflow |
| `/Workflow/ExecuteWorkflow` | POST | Execute workflow |

### Key Concepts

**Node Schema:**
- Defines node capabilities and parameters
- Includes port definitions
- Specifies execution options

**Port Types:**
- `data` - Standard data flow
- `control` - Control flow
- `conditional` - Conditional branches (true/false)

**Connection Validation:**
- Output ports can only connect to input ports
- Type compatibility checking
- Prevent self-connections
- Prevent duplicate connections

**Execution Options:**
- Retry logic
- Timeout handling
- Error handling (continue vs stop)
- Conditional execution

---

## ?? Success Metrics & KPIs

### Phase Completion Criteria

Each phase is considered complete when:
1. ? All tasks in the phase are completed
2. ? All acceptance criteria are met
3. ? All tests pass
4. ? Code review approved
5. ? Documentation updated
6. ? Demo to stakeholders completed

### Quality Metrics

- **Code Coverage:** Minimum 80% for new code
- **Performance:** Designer must remain responsive (<100ms UI updates)
- **Browser Compatibility:** Chrome, Edge, Firefox, Safari
- **Accessibility:** WCAG 2.1 Level AA compliance
- **User Testing:** At least 5 users test each major phase

---

## ?? Known Issues & Limitations

### Current Known Issues
1. **Connection routing** - Bezier curves don't avoid nodes
2. **Zoom** - Connection endpoints misalign at non-100% zoom
3. **Large workflows** - Performance degrades with >50 nodes
4. **Undo/Redo** - Not implemented

### Future Enhancements (Beyond Phase 8)
- [ ] Workflow versioning and history
- [ ] Collaborative editing (multi-user)
- [ ] Workflow testing framework
- [ ] Performance profiling
- [ ] Workflow marketplace/sharing
- [ ] AI-assisted workflow creation
- [ ] Mobile/tablet support
- [ ] Dark mode
- [ ] Accessibility improvements
- [ ] Internationalization (i18n)

---

## ?? Change Log

### Version 1.0 - January 2025
- Initial planning document created
- 8-phase roadmap defined
- All task breakdowns completed

### Version 1.1 - January 2025
- Added Phase 0 for JavaScript architecture refactoring
- Expanded Executive Summary
- Updated Goals & Objectives
- Revised Development Roadmap

---

## ?? Contributors

- **Architecture:** AI Framework Team
- **Frontend Development:** UI Team
- **Backend Development:** Core Team
- **Testing:** QA Team
- **Documentation:** All teams

---

## ?? Support & Questions

For questions or issues related to this plan:
- **GitHub Issues:** https://github.com/techwayfit/Twf_AI_Framework/issues
- **Discussions:** https://github.com/techwayfit/Twf_AI_Framework/discussions
- **Email:** support@techwayfit.com

---

**Last Updated:** January 2025
**Document Version:** 1.1  
**Next Review Date:** End of Phase 4
