# Phase 0 Developer Quick Reference

## Adding a New Node Type

### Step 1: Create Node Class

Create `source/web/wwwroot/js/designer/nodes/{category}/{NodeName}.js`:

```javascript
/**
 * Node description
 */
class MyNodeName extends BaseNode {
    constructor(id, name = 'Display Name') {
    super(id, name, 'MyNodeName', NODE_CATEGORIES.{CATEGORY});
        
        // Define default parameters
  this.parameters = {
   param1: 'default',
       param2: 0
        };
    }

  static fromJSON(json) {
     const node = new MyNodeName(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        return node;
  }
}

nodeRegistry.register('MyNodeName', MyNodeName);
```

### Step 2: Update Designer.cshtml

Add script reference in correct category section:

```html
<!-- Phase 0: Node Classes - {Category} -->
<script src="~/js/designer/nodes/{category}/{NodeName}.js" asp-append-version="true"></script>
```

### Step 3: Add Backend Schema

In `NodeSchemaProvider.cs`:

```csharp
["MyNodeName"] = new()
{
    NodeType = "MyNodeName",
    Parameters = new()
    {
        new() { Name = "param1", Label = "Param 1", Type = ParameterType.Text, Required = true },
  new() { Name = "param2", Label = "Param 2", Type = ParameterType.Number, DefaultValue = 0 }
    }
}
```

That's it! Your node will automatically:
- ? Appear in the palette
- ? Render on canvas
- ? Show in properties panel
- ? Serialize/deserialize
- ? Support variables

## Available Node Categories

```javascript
NODE_CATEGORIES.AI      // Blue (#4A90E2)
NODE_CATEGORIES.CONTROL // Orange (#F5A623)
NODE_CATEGORIES.DATA    // Green (#7ED321)
NODE_CATEGORIES.IO      // Purple (#BD10E0)
```

## Available Parameter Types

```javascript
PARAMETER_TYPES.TEXT       // Single-line text input
PARAMETER_TYPES.TEXT_AREA  // Multi-line textarea
PARAMETER_TYPES.NUMBER     // Number input
PARAMETER_TYPES.BOOLEAN    // Checkbox
PARAMETER_TYPES.SELECT     // Dropdown
PARAMETER_TYPES.JSON       // JSON editor
```

## Common BaseNode Methods

```javascript
// Get node schema from server
node.getSchema()

// Get port definitions (Phase 2)
node.getInputPorts()
node.getOutputPorts()

// Validate node
const { isValid, errors } = node.validate()

// Render properties panel
const html = node.renderProperties()

// Update parameter
node.updateParameter('paramName', value)

// Serialize
const json = node.toJSON()

// Deserialize (static method)
const node = MyNodeName.fromJSON(json)

// Clone
const clone = node.clone()
```

## WorkflowDesigner Methods

```javascript
// Access designer instance
window.designer
window.designerInstance // Same as above

// Node operations
designer.addNode(type, name, x, y)
designer.selectNode(nodeId, addToSelection)
designer.deselectAll()
designer.deleteSelected()

// Workflow operations
await designer.saveWorkflow()
await designer.loadWorkflow()

// Rendering
designer.render()
designer.renderNodes()
designer.renderProperties()

// Node manipulation
designer.updateNodeProperty(nodeId, property, value)
designer.updateNodeParameter(nodeId, paramName, value)
designer.updateNodeParameterJson(nodeId, paramName, jsonString)
```

## NodeRegistry Methods

```javascript
// Register a node type
nodeRegistry.register('NodeType', NodeClass)

// Create a node instance
const node = nodeRegistry.createNode(type, name, x, y)

// Get nodes by category
const categorized = nodeRegistry.getNodesByCategory()

// Get schema
const schema = nodeRegistry.getSchema(type)

// Load all schemas from server
await nodeRegistry.loadSchemas()
```

## WorkflowData Methods

```javascript
// Create new workflow
const workflow = new WorkflowData(id, name, description)

// Add/remove nodes
workflow.addNode(node)
workflow.removeNode(nodeId)

// Add/remove connections
workflow.addConnection(connection)
workflow.removeConnection(connectionId)

// Find node
const node = workflow.findNode(nodeId)

// Serialize
const json = workflow.toJSON()

// Deserialize
const workflow = WorkflowData.fromJSON(json, nodeRegistry)
```

## Debugging Tips

### Console Inspection

```javascript
// Inspect current workflow
console.log(workflow)

// Inspect node registry
console.log(nodeRegistry)

// Inspect all node schemas
console.log(nodeSchemas)

// Inspect selected node
console.log(designer.selectedNode)

// List all registered node types
console.log([...nodeRegistry.nodeTypes.keys()])
```

### Common Issues

**Node doesn't appear in palette**
- Check script is loaded in Designer.cshtml
- Check node is registered: `nodeRegistry.register()`
- Check schema exists in NodeSchemaProvider.cs

**Properties panel doesn't render**
- Check `renderProperties()` calls `super.renderProperties()` or implements correctly
- Check schema has parameters defined
- Check console for JavaScript errors

**Node doesn't save/load**
- Check `fromJSON()` static method exists
- Check `toJSON()` returns correct structure
- Check parameters match schema

**Variable autocomplete doesn't work**
- Check input ID is unique
- Check `setTimeout()` is used to setup after DOM update
- Check workflow.variables exists

## File Locations

```
Core Classes:
  source/web/wwwroot/js/designer/core/Constants.js
  source/web/wwwroot/js/designer/core/WorkflowData.js
  source/web/wwwroot/js/designer/core/BaseNode.js
  source/web/wwwroot/js/designer/core/NodeRegistry.js
  source/web/wwwroot/js/designer/core/Designer.js

Node Classes:
  source/web/wwwroot/js/designer/nodes/{category}/{NodeName}.js

UI Components:
  source/web/wwwroot/js/designer/ui/NodeRenderer.js

Backend:
  source/web/Services/NodeSchemaProvider.cs
  source/web/Models/NodeParameterSchema.cs
  source/web/Controllers/WorkflowController.cs

Views:
  source/web/Views/Workflow/Designer.cshtml
```

## Testing Your Changes

```bash
# Build
dotnet build

# Run
cd source/web
dotnet run

# Navigate to
https://localhost:5001/Workflow

# Check browser console for errors
# Test node creation, editing, saving, loading
```

## Next Steps

After Phase 0, we can:
- **Phase 1**: Add multi-port support
- **Phase 2**: Visual multi-port rendering
- **Phase 3**: Conditional branching
- **Phase 4**: Execution options UI
- **Phase 7**: Workflow execution
- **Phase 8**: Code generation

See `docs/DESIGNER_ENHANCEMENT_PLAN.md` for full roadmap.
