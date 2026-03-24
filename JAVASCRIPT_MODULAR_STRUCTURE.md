# Workflow Designer JavaScript - Modular Structure

## ?? File Organization

The workflow designer JavaScript has been split into **11 modular files** for better maintainability and organization.

### Directory Structure

```
source/web/wwwroot/js/designer/
??? state.js         # Global state variables
??? initialization.js  # Startup & data loading
??? variables.js       # Variable management
??? autocomplete.js    # Variable autocomplete
??? nodes.js # Node operations
??? connections.js     # Connection operations
??? rendering.js    # Canvas rendering
??? properties.js      # Properties panel
??? events.js         # Event handlers
??? utils.js          # Utility functions
```

---

## ?? File Descriptions

### 1. **state.js** - Global State
**Purpose:** Central location for all global variables

**Contains:**
- `workflow` - Current workflow data
- Selection state (`selectedNode`, `selectedNodes`, `selectedVariable`, `selectedConnection`)
- Drag state (`isDraggingNode`, `isDraggingConnection`, etc.)
- UI state (`activeAutocomplete`, `activeSidebarTab`, `zoomLevel`)

**Why separate?**
- Easy to see all shared state in one place
- Prevents variable redeclaration issues
- Clear dependency management

---

### 2. **initialization.js** - Startup & Data Loading
**Purpose:** Initialize the designer and load data

**Key Functions:**
- `initializeDesigner(workflowId)` - Main entry point
- `loadAvailableNodes()` - Fetch node types
- `loadNodeSchemas()` - Fetch node parameters
- `loadWorkflow(workflowId)` - Load workflow data
- `saveWorkflow()` - Save changes

**Dependencies:**
- Requires: `state.js`
- Used by: Designer.cshtml (startup)

---

### 3. **variables.js** - Variable Management  
**Purpose:** CRUD operations for workflow variables

**Key Functions:**
- `renderVariablesList()` - Display variables in sidebar
- `selectVariable(name)` - Select variable for editing
- `showAddVariableForm()` - Show add form
- `createVariable()` - Create new variable
- `updateVariableValue()` - Update existing variable
- `deleteVariable()` - Remove variable
- `renderVariableProperties()` - Show in properties panel

**Dependencies:**
- Requires: `state.js`
- Used by: `initialization.js`, `autocomplete.js`

---

### 4. **autocomplete.js** - Variable Autocomplete
**Purpose:** Smart autocomplete when typing `{{`

**Key Functions:**
- `setupVariableAutocomplete(input, inputId)` - Attach to input
- `showVariableAutocomplete()` - Display dropdown
- `navigateAutocomplete(direction)` - Keyboard navigation
- `selectAutocompleteItem()` - Insert variable
- `insertVariableFromAutocomplete()` - Replace `{{` with `{{varName}}`
- `hideVariableAutocomplete()` - Close dropdown
- `checkForVariables(input)` - Yellow highlight

**Dependencies:**
- Requires: `state.js`, `variables.js`
- Used by: `properties.js`

**Events Handled:**
- `input` - Detect `{{` typing
- `keydown` - Arrow keys, Enter, Tab, Esc
- `blur` - Close dropdown

---

### 5. **nodes.js** - Node Operations
**Purpose:** Node CRUD and selection

**Key Functions:**
- `renderNodePalette()` - Display available nodes
- `onPaletteDragStart()` - Start dragging from palette
- `addNode()` - Create new node on canvas
- `deleteNode()` - Remove node
- `selectNode()` - Select single/multiple nodes
- `selectAll()` - Select all nodes
- `deselectAll()` - Clear selection
- `deleteSelected()` - Delete selected nodes/connection
- `showMultiSelectionInfo()` - Show selection count

**Dependencies:**
- Requires: `state.js`
- Used by: `events.js`, `rendering.js`

---

### 6. **connections.js** - Connection Operations
**Purpose:** Connection CRUD and endpoint dragging

**Key Functions:**
- `addConnection()` - Create new connection
- `deleteConnection()` - Remove connection
- `showConnectionProperties()` - Display in properties panel

**Dragging Functions:**
- `startConnectionDrag()` - Begin dragging endpoint
- `highlightValidPorts()` - Show valid drop targets
- `clearPortHighlights()` - Remove highlights
- `finishConnectionDrag()` - Complete reconnection
- `cancelConnectionDrag()` - Abort drag
- `updateConnectionDragFeedback()` - Visual feedback

**Dependencies:**
- Requires: `state.js`
- Used by: `rendering.js`, `events.js`

**Features:**
- Green circle = source endpoint (draggable)
- Red circle = target endpoint (draggable)
- Valid ports highlighted in green
- Invalid ports grayed out

---

### 7. **rendering.js** - Canvas Rendering
**Purpose:** Draw nodes and connections on SVG canvas

**Key Functions:**
- `render()` - Main render function
- `renderNodes()` - Draw all nodes as DOM elements
- `renderConnections()` - Draw connections as SVG paths
- `renderTempConnection()` - Draw connection being created
- `createBezierPath()` - Generate curved path

**Dependencies:**
- Requires: `state.js`, `nodes.js`, `connections.js`
- Used by: All modules (calls `render()` after changes)

**Performance:**
- Clears and redraws everything
- Preserves selection rectangle during redraw
- Attaches event listeners to rendered elements

---

### 8. **properties.js** - Properties Panel
**Purpose:** Display and edit node/variable properties

**Key Functions:**
- `renderProperties()` - Show node properties
- `renderParameterField()` - Render single parameter input
- `updateNodeParameter()` - Update parameter value
- `updateNodeParameterJson()` - Parse and update JSON
- `updateNodeProperty()` - Update node metadata (name, etc.)

**Dependencies:**
- Requires: `state.js`, `autocomplete.js`
- Used by: `nodes.js`, `variables.js`

**Features:**
- Auto-detects parameter type (Text, Number, Boolean, Select, JSON)
- Yellow highlight for fields with `{{variables}}`
- Attaches autocomplete to Text/TextArea/JSON fields

---

### 9. **events.js** - Event Handlers
**Purpose:** Handle all user interactions

**Key Functions:**
- `setupEventListeners()` - Attach all listeners
- `onCanvasMouseDown()` - Start selection rectangle
- `onCanvasDrop()` - Drop node from palette
- `onNodeMouseDown()` - Start node drag / toggle selection
- `onPortMouseDown()` - Start creating connection
- `onMouseMove()` - Handle all drag types
- `onMouseUp()` - Finish drag operation

**Selection Rectangle:**
- `createSelectionRect()` - Create DOM element
- `updateSelectionRect()` - Resize during drag
- `selectNodesInRect()` - Detect intersecting nodes

**Dependencies:**
- Requires: `state.js`, `nodes.js`, `connections.js`, `rendering.js`
- Used by: `initialization.js` (calls `setupEventListeners()`)

**Keyboard Shortcuts:**
- `Delete` - Delete selected
- `Esc` - Deselect all
- `Ctrl+S` - Save workflow
- `Ctrl+A` - Select all nodes

---

### 10. **utils.js** - Utility Functions
**Purpose:** Helper functions used across modules

**Key Functions:**
- `switchSidebarTab()` - Toggle Nodes/Variables tab
- `zoomIn()` - Increase canvas zoom
- `zoomOut()` - Decrease canvas zoom
- `resetZoom()` - Reset to 100%
- `applyZoom()` - Apply zoom transform
- `generateGuid()` - Generate unique IDs

**Dependencies:**
- Requires: `state.js`
- Used by: All modules

---

## ?? Data Flow Diagram

```
Designer.cshtml
     ?
initialization.js
??? loadAvailableNodes() ? renderNodePalette() (nodes.js)
??? loadNodeSchemas() ? (stores in state.js)
??? loadWorkflow() ? renderVariablesList() (variables.js)
     ?
setupEventListeners() (events.js)
     ?
User Interaction
     ?
Event Handlers (events.js)
??? Node drag ? render() (rendering.js)
??? Port click ? addConnection() (connections.js) ? render()
??? Canvas drop ? addNode() (nodes.js) ? render()
??? Selection ? renderProperties() (properties.js)
     ?
Properties Panel Update
??? Text input ? setupVariableAutocomplete() (autocomplete.js)
??? Parameter change ? updateNodeParameter() ? state update
```

---

## ?? Module Dependencies Graph

```
state.js (no dependencies)
  ?
??? utils.js
??? initialization.js
?   ??? variables.js
?   ??? nodes.js
??? autocomplete.js
?   ??? variables.js
??? connections.js
??? properties.js
?   ??? autocomplete.js
??? rendering.js
?   ??? nodes.js
?   ??? connections.js
??? events.js
    ??? nodes.js
    ??? connections.js
    ??? rendering.js
```

---

## ?? Load Order (Critical!)

The scripts **MUST** be loaded in this order in `Designer.cshtml`:

```html
<script src="~/js/designer/state.js"></script>        <!-- 1. State first -->
<script src="~/js/designer/utils.js"></script>          <!-- 2. Utils (no deps) -->
<script src="~/js/designer/initialization.js"></script> <!-- 3. Initialization -->
<script src="~/js/designer/variables.js"></script>      <!-- 4. Variables -->
<script src="~/js/designer/autocomplete.js"></script>   <!-- 5. Autocomplete -->
<script src="~/js/designer/nodes.js"></script>          <!-- 6. Nodes -->
<script src="~/js/designer/connections.js"></script>    <!-- 7. Connections -->
<script src="~/js/designer/rendering.js"></script>      <!-- 8. Rendering -->
<script src="~/js/designer/properties.js"></script>     <!-- 9. Properties -->
<script src="~/js/designer/events.js"></script>         <!-- 10. Events last -->

<script>
    initializeDesigner('@Model.Id'); <!-- Entry point -->
</script>
```

**Why this order?**
- `state.js` defines global variables used by all modules
- `utils.js` has no dependencies
- `initialization.js` calls functions from `variables.js` and `nodes.js`
- `autocomplete.js` needs `variables.js`
- `properties.js` needs `autocomplete.js`
- `rendering.js` needs `nodes.js` and `connections.js`
- `events.js` needs everything else

---

## ?? How to Modify

### Adding a New Feature

**Example: Add "Duplicate Node" feature**

1. **Choose appropriate file:**
   - Node operation? ? `nodes.js`
 - Canvas rendering? ? `rendering.js`
   - Event handling? ? `events.js`

2. **Add function to file:**
   ```javascript
   // In nodes.js
   function duplicateNode(nodeId) {
   const originalNode = workflow.nodes.find(n => n.id === nodeId);
       if (!originalNode) return;
       
       const newNode = {
         ...originalNode,
  id: generateGuid(),
  position: {
   x: originalNode.position.x + 20,
       y: originalNode.position.y + 20
           }
  };
       
       workflow.nodes.push(newNode);
       render();
       selectNode(newNode.id);
   }
 ```

3. **Add UI trigger** (if needed):
   - Keyboard shortcut in `events.js`:
     ```javascript
   if (e.ctrlKey && e.key === 'd') {
       e.preventDefault();
       if (selectedNode) duplicateNode(selectedNode.id);
     }
     ```
   - Or context menu in `rendering.js`
   - Or button in `properties.js`

---

### Debugging Tips

**Issue: Function not defined**
- Check load order in `Designer.cshtml`
- Ensure function is in correct file
- Check browser console for script load errors

**Issue: State not updating**
- Verify you're modifying `state.js` variables
- Don't declare new variables with `let` in other files
- Call `render()` after state changes

**Issue: Events not firing**
- Check `setupEventListeners()` is called in `initializeDesigner()`
- Verify event targets match rendered DOM
- Use `e.stopPropagation()` to prevent event bubbling

---

## ?? Benefits of Modular Structure

### Before (Single 1500-line file)
- ? Hard to find functions
- ? Merge conflicts
- ? Difficult to test
- ? Unclear dependencies
- ? Slow to load in editor

### After (11 focused files)
- ? Logical organization by feature
- ? Each file < 300 lines
- ? Clear responsibilities
- ? Easy to locate code
- ? Better for team collaboration
- ? Easier to unit test
- ? Faster editor performance

---

## ?? Testing Strategy

### Unit Testing (Future)
Each module can be tested independently:

```javascript
// Test nodes.js
describe('addNode', () => {
    it('should create node with correct properties', () => {
        const count = workflow.nodes.length;
        addNode('LlmNode', 'AI', 'LLM', '#blue', 100, 100);
        expect(workflow.nodes.length).toBe(count + 1);
        expect(workflow.nodes[count].type).toBe('LlmNode');
    });
});

// Test autocomplete.js
describe('insertVariableFromAutocomplete', () => {
    it('should replace {{ with {{varName}}', () => {
    // Mock input element
     const input = document.createElement('input');
        input.value = 'API Key: {{';
 input.selectionStart = 11;
        
      insertVariableFromAutocomplete(input.id, 'api_key');
        
  expect(input.value).toBe('API Key: {{api_key}}');
    });
});
```

---

## ?? Best Practices

### 1. **Keep State in state.js**
```javascript
// ? Bad - declaring new state in other files
let myNewState = null; // in nodes.js

// ? Good - add to state.js
let myNewState = null; // in state.js
```

### 2. **Call render() After Changes**
```javascript
// ? Bad
function addNode(...) {
    workflow.nodes.push(node);
    // Forgot to render!
}

// ? Good
function addNode(...) {
  workflow.nodes.push(node);
    render(); // Update display
}
```

### 3. **Use Descriptive Names**
```javascript
// ? Bad
function handleEvent(e) { ... }

// ? Good
function onNodeMouseDown(e, node) { ... }
```

### 4. **Document Complex Functions**
```javascript
/**
 * Highlights ports that are valid drop targets for connection dragging.
 * Source endpoints can only connect to input ports.
 * Target endpoints can only connect to output ports.
 * A node cannot connect to itself.
 * 
 * @param {object} connection - The connection being dragged
 * @param {string} draggedEnd - 'source' or 'target'
 */
function highlightValidPorts(connection, draggedEnd) {
    // ...
}
```

---

## ?? Quick Reference

| Task | File | Function |
|------|------|----------|
| Add node to canvas | `nodes.js` | `addNode()` |
| Create connection | `connections.js` | `addConnection()` |
| Show properties | `properties.js` | `renderProperties()` |
| Add variable | `variables.js` | `createVariable()` |
| Setup autocomplete | `autocomplete.js` | `setupVariableAutocomplete()` |
| Redraw canvas | `rendering.js` | `render()` |
| Handle mouse drag | `events.js` | `onMouseMove()` |
| Save workflow | `initialization.js` | `saveWorkflow()` |
| Zoom canvas | `utils.js` | `zoomIn()`, `zoomOut()` |
| Toggle sidebar tab | `utils.js` | `switchSidebarTab()` |

---

## ?? Future Improvements

1. **Add module bundling** (Webpack/Vite) for production
2. **Add TypeScript** for type safety
3. **Extract CSS** into separate SCSS modules
4. **Add unit tests** for each module
5. **Add JSDoc comments** throughout
6. **Create module loader** for dynamic imports
7. **Add hot reload** during development

---

## ? Migration Checklist

When updating from old single-file structure:

- [?] Create `js/designer/` directory
- [?] Split code into 11 modules
- [?] Update `Designer.cshtml` script tags
- [?] Test all features still work
- [?] Delete old `workflow-designer.js`
- [ ] Add unit tests
- [ ] Update documentation
- [ ] Train team on new structure

---

**Current Status:** ? Complete and functional!

**Old file:** `source/web/wwwroot/js/workflow-designer.js` (can be deleted)

**New structure:** `source/web/wwwroot/js/designer/*.js` (11 modular files)

**Benefits:** Better organization, easier maintenance, clearer code structure!
