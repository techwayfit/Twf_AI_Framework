# Node Parameter Editing - Implementation Guide

## Overview

The workflow designer now supports **dynamic parameter editing** for all node types. Each node has a schema that defines what parameters it accepts, and the UI automatically generates the appropriate form fields.

## Architecture

### 1. **Parameter Schema System**

Each node type has a schema defining:
- Parameter name, label, and description
- Data type (Text, Number, Boolean, Select, JSON, etc.)
- Validation rules (required, min/max, patterns)
- Default values and options

### 2. **Three-Layer Approach**

```
???????????????????????????????????????????
?  UI Layer (Designer.cshtml + JS)       ?
?  - Renders dynamic forms based on schema?
?  - Validates user input     ?
?  - Stores parameters in NodeDefinition  ?
???????????????????????????????????????????
                ?
???????????????????????????????????????????
?  Model Layer (NodeDefinition)   ?
?  - Stores parameters as Dictionary      ?
?  - Serializes to JSON for persistence   ?
???????????????????????????????????????????
         ?
???????????????????????????????????????????
?  Backend Layer (Node construction)     ?
?  - Reads parameters from definition     ?
?  - Constructs actual INode instances    ?
?  - Executes workflow                    ?
???????????????????????????????????????????
```

## Files Created/Modified

### New Files

1. **`Models/NodeParameterSchema.cs`**
 - Defines parameter schema model
   - Supports 7 parameter types
   - Includes validation rules

2. **`Services/NodeSchemaProvider.cs`**
   - Provides schemas for all 13 node types
   - Centralized parameter definitions
   - Maps node types to their schemas

### Modified Files

3. **`Controllers/WorkflowController.cs`**
   - Added `GetNodeSchema(nodeType)` endpoint
   - Added `GetAllNodeSchemas()` endpoint
 - Returns JSON schemas to UI

4. **`wwwroot/js/workflow-designer.js`**
   - Added `loadNodeSchemas()` function
   - Added `renderParameterField()` for dynamic forms
   - Added `updateNodeParameter()` for live updates
   - Added `updateNodeParameterJson()` for JSON parameters

## Parameter Types Supported

| Type | UI Control | Example |
|------|-----------|---------|
| **Text** | Single-line input | API Key, Model name |
| **TextArea** | Multi-line textarea | Prompts, templates |
| **Number** | Number input | Temperature, Max tokens |
| **Boolean** | Checkbox | Enable/disable features |
| **Select** | Dropdown | Provider selection, HTTP method |
| **Json** | JSON textarea | Field mappings, headers |
| **KeyValueList** | Dynamic key-value editor | (Future) |
| **StringList** | Dynamic list editor | (Future) |

## Example: LLM Node Parameters

### Schema Definition (C#)
```csharp
["LlmNode"] = new()
{
    NodeType = "LlmNode",
    Parameters = new()
    {
        new() { 
            Name = "provider", 
      Label = "Provider", 
            Type = ParameterType.Select, 
            Required = true, 
         DefaultValue = "openai",
            Options = new() {
      new() { Value = "openai", Label = "OpenAI" },
          new() { Value = "azure", Label = "Azure OpenAI" },
    new() { Value = "anthropic", Label = "Anthropic" }
     }
        },
        new() { 
            Name = "model", 
      Label = "Model", 
            Type = ParameterType.Text, 
          Required = true, 
    DefaultValue = "gpt-4o" 
        },
        new() { 
      Name = "temperature", 
       Label = "Temperature", 
       Type = ParameterType.Number, 
            DefaultValue = 0.7, 
          MinValue = 0, 
          MaxValue = 2 
        },
        // ... more parameters
    }
}
```

### Rendered UI

When you select an LLM node in the designer, you'll see:

```
???????????????????????????????????????
? LLM                    ?
???????????????????????????????????????
? Node Name:  [LLM Call             ] ?
?            ?
? Type:       LlmNode      ?
?    ?
? ?????????????????????????????????? ?
? Parameters            ?
?      ?
? Provider: *          ?
? [? OpenAI         ] ?
?   ?
? Model: *     ?
? [gpt-4o            ] ?
?               ?
? API Key:  ?
? [     ] ?
? Leave empty to use env variable      ?
?       ?
? System Prompt:    ?
? ???????????????????????????????????? ?
? ?You are a helpful assistant...    ? ?
? ???????????????????????????????????? ?
?        ?
? Temperature:                  ?
? [0.7       ] (0-2)     ?
?       ?
? Max Tokens:    ?
? [1000  ] (1-128000)         ?
?    ?
? ? Maintain Chat History       ?
?   Enable multi-turn conversation     ?
???????????????????????????????????????
```

### Saved JSON

When saved, the node definition looks like:

```json
{
  "id": "node-123",
  "name": "LLM Call",
  "type": "LlmNode",
  "category": "AI",
  "parameters": {
    "provider": "openai",
    "model": "gpt-4o",
    "apiKey": "",
    "systemPrompt": "You are a helpful assistant...",
    "temperature": 0.7,
    "maxTokens": 1000,
    "maintainHistory": false
  },
  "position": { "x": 100, "y": 200 },
  "color": "#4A90E2"
}
```

## How It Works: Step-by-Step

### 1. User Drags Node onto Canvas

```javascript
// workflow-designer.js
function addNode(type, category, name, color, x, y) {
    const node = {
        id: generateGuid(),
        name: name,
        type: type,
        category: category,
        parameters: {},  // Empty initially
        position: { x, y },
        color: color
    };
workflow.nodes.push(node);
    render();
    selectNode(node.id);  // Shows properties panel
}
```

### 2. Properties Panel Loads Schema

```javascript
async function loadNodeSchemas() {
    const response = await fetch('/Workflow/GetAllNodeSchemas');
    nodeSchemas = await response.json();
}

function renderProperties() {
    const schema = nodeSchemas[selectedNode.type];
    // Render form fields based on schema...
}
```

### 3. User Edits Parameters

```javascript
function updateNodeParameter(paramName, value) {
    selectedNode.parameters[paramName] = value;
    // Value is immediately stored in the node object
}
```

### 4. Workflow is Saved

```javascript
async function saveWorkflow() {
    const response = await fetch('/Workflow/SaveWorkflow', {
  method: 'POST',
        body: JSON.stringify(workflow)  // Includes all parameters
    });
}
```

### 5. Backend Persists JSON

```csharp
// WorkflowController.cs
[HttpPost]
public async Task<IActionResult> SaveWorkflow([FromBody] WorkflowDefinition workflow)
{
    // workflow.Nodes[i].Parameters contains all the values
    await _repository.UpdateAsync(workflow);
    return Json(new { success = true });
}
```

## Validation

### Client-Side Validation

The UI enforces:
- **Required fields** - Can't be empty
- **Number ranges** - Min/max values
- **JSON syntax** - Valid JSON for JSON parameters
- **Select options** - Must choose from dropdown

### Example Validation

```javascript
function renderParameterField(param, currentValue) {
    const required = param.required ? 'required' : '';
  
    if (param.type === 'Number') {
        return `
            <input type="number" 
        min="${param.minValue || ''}"
  max="${param.maxValue || ''}"
   ${required}
   onchange="updateNodeParameter('${param.name}', parseFloat(this.value))" />
    `;
    }
}
```

## Adding a New Node Type

To add a new node with parameters:

### 1. Define the Schema

```csharp
// Services/NodeSchemaProvider.cs
["MyCustomNode"] = new()
{
    NodeType = "MyCustomNode",
    Parameters = new()
    {
        new() { 
     Name = "inputKey", 
 Label = "Input Key", 
   Type = ParameterType.Text, 
      Required = true,
            Placeholder = "data_key"
        },
        new() { 
            Name = "mode", 
  Label = "Processing Mode", 
            Type = ParameterType.Select,
      Options = new() {
       new() { Value = "fast", Label = "Fast Processing" },
        new() { Value = "accurate", Label = "Accurate Processing" }
       }
   }
    }
}
```

### 2. Add to Available Nodes

```csharp
// Controllers/WorkflowController.cs
public IActionResult GetAvailableNodes()
{
    var nodes = new[]
    {
        // ...existing nodes...
        new { 
         type = "MyCustomNode", 
        category = "Custom", 
            name = "My Custom Node", 
    description = "Does custom processing", 
        color = "#9C27B0" 
   }
    };
}
```

### 3. Implement the Backend Node (Later)

When executing workflows, you'll construct the actual node:

```csharp
// Future: Workflow execution engine
var parameters = nodeDefinition.Parameters;
var node = new MyCustomNode(
    inputKey: parameters["inputKey"]?.ToString(),
    mode: parameters["mode"]?.ToString()
);
```

## Parameter Schema Examples

### HTTP Request Node

```csharp
["HttpRequestNode"] = new()
{
 Parameters = new()
    {
    new() { 
          Name = "method", 
      Type = ParameterType.Select,
    Options = new() {
    new() { Value = "GET", Label = "GET" },
         new() { Value = "POST", Label = "POST" },
         new() { Value = "PUT", Label = "PUT" },
        new() { Value = "DELETE", Label = "DELETE" }
            }
    },
  new() { 
            Name = "urlTemplate", 
            Type = ParameterType.Text,
 Required = true,
     Placeholder = "https://api.example.com/{{id}}"
        },
     new() { 
   Name = "headers", 
            Type = ParameterType.Json,
  Placeholder = "{\"Authorization\": \"Bearer token\"}"
        },
     new() { 
     Name = "timeout", 
            Type = ParameterType.Number,
 DefaultValue = 30,
 MinValue = 1,
 MaxValue = 300
        }
    }
}
```

### Prompt Builder Node

```csharp
["PromptBuilderNode"] = new()
{
    Parameters = new()
    {
        new() { 
            Name = "promptTemplate", 
         Type = ParameterType.TextArea,
            Required = true,
 Placeholder = "Summarize this text: {{content}}\n\nOutput format: {{format}}"
     },
    new() { 
Name = "systemTemplate", 
            Type = ParameterType.TextArea,
            Placeholder = "You are a helpful assistant."
        }
    }
}
```

## Current Limitations

1. **No workflow execution** - Parameters are saved but not yet used to construct actual nodes for execution
2. **No parameter validation in backend** - Backend doesn't validate parameter schemas yet
3. **No conditional parameters** - Can't show/hide parameters based on other values
4. **No array/object editors** - KeyValueList and StringList types not yet implemented

## Future Enhancements

### Phase 1: Advanced UI Controls
- [ ] Rich JSON editor with syntax highlighting
- [ ] Key-value pair list editor
- [ ] String array editor
- [ ] File upload for API keys/certificates
- [ ] Color picker for visual customization

### Phase 2: Smart Validation
- [ ] Real-time validation feedback
- [ ] Conditional parameters (show/hide based on other values)
- [ ] Cross-parameter validation
- [ ] Preview mode to test parameters

### Phase 3: Workflow Execution
- [ ] Build actual INode instances from parameters
- [ ] Execute workflows from designer
- [ ] Real-time execution monitoring
- [ ] Parameter testing/debugging

## API Endpoints

### Get Schema for Single Node Type
```
GET /Workflow/GetNodeSchema/{nodeType}

Response:
{
  "nodeType": "LlmNode",
  "parameters": [
 {
      "name": "provider",
      "label": "Provider",
      "type": "Select",
      "required": true,
 "defaultValue": "openai",
      "options": [...]
 }
  ]
}
```

### Get All Node Schemas
```
GET /Workflow/GetAllNodeSchemas

Response:
{
  "LlmNode": { ... },
  "PromptBuilderNode": { ... },
  ...
}
```

## Testing

To test the parameter editing:

1. **Start the application** and open the designer
2. **Drag an LLM node** onto the canvas
3. **Click the node** to select it
4. **Properties panel** should show all LLM parameters
5. **Edit values** (provider, model, temperature, etc.)
6. **Click Save** - parameters should be persisted
7. **Reload the workflow** - parameters should be restored

## Summary

? **Dynamic parameter forms** based on schemas  
? **13 node types** with full parameter support  
? **7 parameter types** (Text, Number, Boolean, Select, JSON, etc.)  
? **Client-side validation** (required, min/max, JSON syntax)  
? **Automatic persistence** via JSON serialization  
? **Type-safe storage** in NodeDefinition model  

The UI now provides a complete parameter editing experience! Users can configure all node properties directly in the designer without writing code.
