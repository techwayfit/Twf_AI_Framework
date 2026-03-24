# Workflow Variables - Implementation Guide

## Overview

The workflow designer now supports **workflow-level variables** that can be defined once and referenced throughout the workflow using `{{variable_name}}` syntax. This eliminates duplication and makes workflows more maintainable.

## Features Implemented

### ? 1. Variables Panel
A dedicated panel below the toolbar displays all workflow variables with inline editing.

### ? 2. Variable Management
- **Add** variables with names and default values
- **Edit** variable values inline
- **Delete** variables with confirmation
- **Validation** - variable names must be valid identifiers

### ? 3. Variable Syntax
Use `{{variable_name}}` in any text or textarea parameter field.

### ? 4. Visual Indicators
- Parameters with variables get a **yellow highlight**
- **Insert Variable** button for easy insertion
- **Available variables** shown below each text field

### ? 5. Persistence
Variables are saved with the workflow definition and restored on load.

---

## User Interface

### Variables Panel

Located between the toolbar and main canvas:

```
??????????????????????????????????????????????????????????????
? ?? Toolbar (Save, Delete, Zoom, etc.)      ?
??????????????????????????????????????????????????????????????
? ?? Variables: {{api_key}}=sk-... {{model}}=gpt-4o [+ Add] ? ? NEW!
??????????????????????????????????????????????????????????????
? [Palette] ? [Canvas]    ? [Properties] ?
??????????????????????????????????????????????????????????????
```

### Variables Panel Components

```
???????????????????????????????????????????????????????????????
? { } Variables           ?
???????????????????????????????????????????????????????????????
? ???????????????? ???????????????? ????????????????         ?
? ? {{api_key}}  ? ? {{model}}  ? ? {{temp}}   ?  [+Add] ?
? ? = [sk-123  ] ? ? = [gpt-4o  ] ? ? = [0.7     ] ?Variable?
? ? [×]  ? ? [×]       ? ? [×]      ?         ?
? ???????????????? ???????????????? ????????????????         ?
???????????????????????????????????????????????????????????????
```

### Parameter Field with Variables

```
???????????????????????????????????????????????
? API Key *    ?
???????????????????????????????????????????????
? [{ } Insert Variable]            ?
? ??????????????????????????????????????????? ?
? ? {{api_key}}         ? ? ? Yellow highlight
? ??????????????????????????????????????????? ?
? ?? Available: {{api_key}}, {{model}}      ?
???????????????????????????????????????????????
```

---

## How to Use

### Step 1: Add Variables

**Method 1: Manual Entry**
1. Click **"+ Add Variable"** button
2. Enter variable name (e.g., `api_key`)
3. Enter default value (e.g., `sk-proj-...`)
4. Variable appears in variables panel

**Method 2: Quick Add**
```
Click [+ Add Variable]
  ?
  Name: api_key
  ?
  Value: sk-proj-abc123
  ?
  Variable created: {{api_key}}
```

### Step 2: Use Variables in Parameters

**Method 1: Type Manually**
```
In any text field:
Type: {{api_key}}
  ?
  Field turns yellow (has-variables)
  ?
  Variable reference saved
```

**Method 2: Insert Button**
```
Click [{ } Insert Variable]
  ?
  Choose: 1. {{api_key}} = sk-...
          2. {{model}} = gpt-4o
  ?
  Enter: 1
?
  {{api_key}} inserted at cursor
```

### Step 3: Edit Variable Values

**Inline Editing:**
```
Variables Panel:
{{api_key}} = [sk-123    ]
           ? Edit here
           ?
  Updated globally!
  All nodes using {{api_key}} reflect change
```

### Step 4: Delete Variables

```
Click [×] on variable
  ?
  Confirm deletion
  ?
  Variable removed
  (Node parameters not updated automatically)
```

---

## Example Workflows

### Example 1: LLM Configuration

**Without Variables (Before):**
```
Node 1 (LLM):
- API Key: sk-proj-abc123xyz...
- Model: gpt-4o
- Temperature: 0.7

Node 2 (LLM):
- API Key: sk-proj-abc123xyz... ? Duplicate!
- Model: gpt-4o    ? Duplicate!
- Temperature: 0.7            ? Duplicate!

Node 3 (LLM):
- API Key: sk-proj-abc123xyz... ? Duplicate!
- Model: gpt-4o       ? Duplicate!
- Temperature: 0.7     ? Duplicate!
```

**With Variables (After):**
```
Variables:
{{api_key}} = sk-proj-abc123xyz...
{{model}} = gpt-4o
{{temperature}} = 0.7

Node 1 (LLM):
- API Key: {{api_key}}
- Model: {{model}}
- Temperature: {{temperature}}

Node 2 (LLM):
- API Key: {{api_key}}
- Model: {{model}}
- Temperature: {{temperature}}

Node 3 (LLM):
- API Key: {{api_key}}
- Model: {{model}}
- Temperature: {{temperature}}

? Single source of truth!
? Change once, updates everywhere!
```

### Example 2: HTTP Requests

**Variables:**
```
{{base_url}} = https://api.example.com
{{api_token}} = Bearer abc123
```

**Node Parameters:**
```
HTTP Request Node:
- URL: {{base_url}}/users/{{user_id}}
- Headers: {"Authorization": "{{api_token}}"}
```

### Example 3: Prompt Templates

**Variables:**
```
{{language}} = English
{{tone}} = professional
{{max_length}} = 500 words
```

**Prompt Builder Node:**
```
Prompt Template:
"Summarize the following in {{language}} using a {{tone}} tone. 
Maximum length: {{max_length}}.

Content: {{content}}"
```

---

## JSON Structure

### Workflow Definition with Variables

```json
{
  "id": "workflow-123",
  "name": "Customer Support Bot",
  "variables": {
    "api_key": "sk-proj-abc123",
    "model": "gpt-4o",
    "temperature": "0.7",
    "system_prompt": "You are a helpful assistant."
  },
  "nodes": [
    {
"id": "node-1",
      "name": "LLM Call",
      "type": "LlmNode",
      "parameters": {
        "apiKey": "{{api_key}}",
        "model": "{{model}}",
        "temperature": "{{temperature}}",
        "systemPrompt": "{{system_prompt}}"
      }
    }
  ]
}
```

---

## Variable Syntax Rules

### ? Valid Variable Names

```
{{api_key}}        ? Lowercase with underscore
{{apiKey}}  ? camelCase
{{API_KEY}}        ? UPPERCASE
{{model_name}}     ? Multiple underscores
{{temperature1}}   ? Numbers allowed
{{_private}}       ? Leading underscore
```

### ? Invalid Variable Names

```
{{api-key}}        ? Hyphens not allowed
{{api key}}        ? Spaces not allowed
{{1model}}         ? Cannot start with number
{{api.key}}        ? Dots not allowed
{{api$key}}        ? Special chars not allowed
```

### Variable Usage Examples

**Single Variable:**
```
{{api_key}}
```

**Multiple Variables:**
```
{{base_url}}/users/{{user_id}}
```

**In JSON:**
```json
{
  "Authorization": "Bearer {{token}}",
  "Content-Type": "application/json"
}
```

**In Prompts:**
```
Summarize this text in {{language}} using a {{tone}} tone: {{content}}
```

---

## Visual Indicators

### Yellow Highlight

Parameters containing `{{variables}}` get a yellow background:

```css
.has-variables {
    background-color: #fff3cd !important;  /* Light yellow */
    border-left: 3px solid #ffc107 !important;  /* Orange border */
}
```

**Before:**
```
???????????????????
? API Key         ?
? [      ]  ? ? Normal white background
???????????????????
```

**After (with variable):**
```
???????????????????
? API Key    ?
? [{{api_key}}  ] ? ? Yellow background + orange border
???????????????????
```

---

## Implementation Details

### Files Modified

#### 1. `source\web\Models\WorkflowDefinition.cs`
**Added:**
```csharp
public Dictionary<string, object> Variables { get; set; } = new();
```

#### 2. `source\web\Views\Workflow\Designer.cshtml`
**Added:**
- Variables panel HTML structure
- CSS styling for variables (.variable-item, .has-variables, etc.)

**CSS Classes:**
```css
#variables-panel      - Yellow panel container
.variable-item          - Individual variable display
.variable-name          - Variable name ({{var}})
.variable-value    - Editable value input
.variable-delete        - Delete button
.btn-add-variable       - Add variable button
.has-variables          - Yellow highlight for inputs
```

#### 3. `source\web\wwwroot\js\workflow-designer.js`
**Added Functions:**
```javascript
renderVariables()       // Render variables panel
addVariable()                  // Add new variable
updateVariable(name, value)    // Update variable value
deleteVariable(name)         // Delete variable
getAvailableVariables()  // Get list of variables
checkForVariables(input)       // Check if input has {{}}
insertVariableAtCursor(id)     // Insert variable at cursor
```

**Modified Functions:**
```javascript
loadWorkflow()   // Initialize variables
renderParameterField()         // Add variable picker
```

---

## Validation

### Variable Name Validation

```javascript
// Regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/
// Must start with letter or underscore
// Can contain letters, numbers, underscores

if (!/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(name)) {
    alert('Invalid variable name!');
}
```

### Examples:
```
api_key      ? Valid
ApiKey       ? Valid
_private     ? Valid
api-key      ? Invalid (hyphen)
123key       ? Invalid (starts with number)
api key      ? Invalid (space)
```

---

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Add variable | Click "+ Add Variable" |
| Edit variable | Click value field |
| Delete variable | Click × icon |
| Insert variable | Click "Insert Variable" button |

---

## Current Limitations

### ?? 1. No Runtime Substitution (Yet)
- Variables are stored in the workflow
- Currently **NOT substituted** when executing workflow
- Future: Execution engine will replace `{{var}}` with actual values

### ?? 2. No Automatic Update on Delete
- Deleting a variable doesn't update node parameters
- User must manually remove `{{variable}}` references

### ?? 3. No Variable Type Validation
- All variables stored as strings
- Type conversion happens at execution time (future)

### ?? 4. No Nested Variables
```
{{outer_{{inner}}}}  ? Not supported
```

### ?? 5. No Expression Evaluation
```
{{api_key + "_suffix"}}  ? Not supported
{{count * 2}}  ? Not supported
```

---

## Future Enhancements

### Phase 1: Advanced UI (Planned)
- [ ] **Bulk edit** - Edit multiple variables at once
- [ ] **Import/Export** - Import variables from JSON/CSV
- [ ] **Variable types** - String, Number, Boolean, Secret
- [ ] **Variable groups** - Organize variables by category
- [ ] **Search/filter** - Find variables quickly

### Phase 2: Smart Features (Planned)
- [ ] **Autocomplete** - Suggest variables while typing
- [ ] **Validation** - Warn about undefined variables
- [ ] **Usage tracking** - Show which nodes use each variable
- [ ] **Rename refactor** - Update all references when renaming
- [ ] **Secret management** - Mask sensitive values

### Phase 3: Execution Support (Required)
- [ ] **Runtime substitution** - Replace {{var}} during execution
- [ ] **Environment overrides** - Different values per environment
- [ ] **Dynamic variables** - Compute values at runtime
- [ ] **Variable scoping** - Global vs node-local variables

---

## Best Practices

### ? Do:
1. **Use descriptive names**: `api_key` not `key1`
2. **Group related variables**: `db_host`, `db_port`, `db_name`
3. **Use UPPERCASE** for constants: `{{MAX_RETRIES}}`
4. **Document defaults**: Add comments in variable values
5. **Test before saving**: Verify variables work as expected

### ? Don't:
1. **Hardcode secrets**: Use variables instead
2. **Duplicate values**: Use variables to eliminate duplication
3. **Use invalid names**: Follow naming rules
4. **Delete without checking**: Verify no nodes use the variable
5. **Overuse variables**: Not everything needs to be a variable

---

## Example Use Cases

### 1. Multi-Environment Workflows
```
Development:
{{api_url}} = https://dev-api.example.com
{{log_level}} = DEBUG

Production:
{{api_url}} = https://api.example.com
{{log_level}} = ERROR
```

### 2. A/B Testing
```
{{model_a}} = gpt-4o
{{model_b}} = gpt-4o-mini
{{test_percentage}} = 20
```

### 3. Localization
```
{{language}} = English
{{currency}} = USD
{{date_format}} = MM/DD/YYYY
```

### 4. API Configuration
```
{{base_url}} = https://api.example.com
{{api_version}} = v2
{{timeout}} = 30
{{max_retries}} = 3
```

---

## Troubleshooting

### Variable not showing in list
- **Cause:** Variable not saved
- **Fix:** Click in variable value field and check if it saves

### Yellow highlight not appearing
- **Cause:** Typo in variable syntax
- **Fix:** Ensure format is exactly `{{variable_name}}`

### Variable not inserting at cursor
- **Cause:** Input field not focused
- **Fix:** Click in field before clicking "Insert Variable"

### Can't delete variable
- **Cause:** Confirmation dialog blocked
- **Fix:** Allow popups for this site

---

## Summary

### ? What's New

**Variables Panel:**
- Add, edit, delete variables
- Yellow panel with inline editing
- Visual feedback for variable usage

**Parameter Fields:**
- "Insert Variable" button
- Yellow highlight for `{{variables}}`
- Available variables hint

**Workflow Model:**
- Variables stored in `workflow.variables`
- Persisted with workflow definition
- Loaded and saved automatically

### ?? Benefits

1. **No duplication** - Define once, use everywhere
2. **Easy updates** - Change in one place
3. **Better organization** - Group related values
4. **Flexibility** - Mix variables and literal values
5. **Visibility** - See all variables at a glance

### ?? Next Steps

**Refresh your browser and try:**
1. Click **"+ Add Variable"**
2. Create `{{api_key}}` = `your-key`
3. Add an LLM node
4. Click **"Insert Variable"** in API Key field
5. Select `{{api_key}}`
6. See yellow highlight!
7. Save and reload - variables persist!

**Your workflows just got 10x more maintainable!** ??
