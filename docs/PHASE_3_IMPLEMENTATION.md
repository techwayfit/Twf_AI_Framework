# Phase 3 Implementation Plan - Condition Node Enhancement

**Status:** ?? In Progress  
**Start Date:** January 25, 2025  
**Target Completion:** Week 4  
**Dependencies:** Phase 1 ?, Phase 2 ?  

---

## ?? Overview

Phase 3 builds on the multi-port foundation from Phase 2 to create a fully functional condition node configuration UI. Users will be able to visually configure complex conditional routing with multiple output branches.

### Goals

1. **Dynamic Condition Editor** - Add/remove/edit conditions in the properties panel
2. **Visual Port Generation** - Output ports automatically update based on conditions
3. **Expression Builder** - Simple UI for building condition expressions
4. **Variable Autocomplete** - Suggest available variables in expressions
5. **Validation** - Real-time validation of condition expressions
6. **Connection Routing** - Each condition routes to different downstream nodes

---

## ?? Success Criteria

- ? Can add multiple named conditions to a ConditionNode
- ? Each condition generates a unique output port
- ? Output ports update in real-time as conditions change
- ? Can connect each conditional port to different nodes
- ? Conditions support variable references with autocomplete
- ? Expression validation provides helpful error messages
- ? Default port always present as fallback
- ? Workflow JSON export includes all conditions

---

## ??? Architecture

### Component Structure

```
ConditionNode UI Components:
??? Properties Panel
?   ??? Condition List (dynamic)
?   ?   ??? Condition Item
?   ?   ?   ??? Name Input
?   ?   ?   ??? Expression Input (with autocomplete)
?   ?   ?   ??? Delete Button
?   ?   ??? Add Condition Button
?   ??? Validation Messages
??? Node Renderer
?   ??? getDynamicConditionPorts() [EXISTING]
?   ??? renderPorts() [EXISTING]
??? Data Model
    ??? NodeDefinition.parameters.conditions (Dictionary<string, string>)
```

### Data Flow

```
1. User adds condition in properties panel
 ?
2. Update node.parameters.conditions
   ?
3. Trigger node re-render
   ?
4. getDynamicConditionPorts() generates new port list
   ?
5. renderPorts() creates visual port elements
   ?
6. User connects conditional port to target node
   ?
7. Connection stores sourcePortId (e.g., "is_positive")
```

---

## ? Implementation Tasks

### Task 3.1: Condition Editor UI Component

**File:** `source/web/wwwroot/js/designer/ui/ConditionEditor.js` (NEW)

**Create a reusable condition editor component:**

```javascript
/**
 * ConditionEditor - UI component for editing condition lists
 */
class ConditionEditor {
    /**
     * Render condition list editor
   * @param {Object} conditions - Dictionary of { conditionName: expression }
     * @param {Array<string>} availableVariables - Variable names for autocomplete
     * @param {Function} onChange - Callback when conditions change
     * @returns {string} HTML string
     */
static render(conditions, availableVariables, onChange) {
        // Returns HTML for condition editor
    }
    
    /**
  * Add a new condition
     */
    static addCondition(container, conditions, onChange) {
        // Add empty condition
    }
    
    /**
     * Remove a condition
     */
    static removeCondition(container, conditionName, conditions, onChange) {
      // Remove and trigger onChange
    }
    
    /**
     * Update condition name or expression
     */
    static updateCondition(container, oldName, newName, expression, conditions, onChange) {
      // Update and trigger onChange
    }
    
    /**
     * Validate condition expression
     */
    static validateExpression(expression, availableVariables) {
        // Returns { valid: boolean, error?: string }
    }
}
```

**Key Features:**
- Dynamic add/remove conditions
- Real-time validation
- Variable autocomplete dropdown
- Clear error messages
- Keyboard shortcuts (Enter to add, Delete to remove)

**Acceptance Criteria:**
- [ ] Can add unlimited conditions
- [ ] Can remove any condition
- [ ] Can rename conditions (triggers port update)
- [ ] Expression validates on blur
- [ ] Variables show in autocomplete dropdown
- [ ] Invalid expressions show red border + error message

---

### Task 3.2: Integrate Condition Editor into Properties Panel

**File:** `source/web/wwwroot/js/designer/ui/PropertiesPanel.js` (MODIFY)

**Extend PropertiesPanel to handle ConditionNode specially:**

```javascript
// In renderNodeProperties()
if (node.type === 'ConditionNode') {
    // Render condition editor instead of generic parameter form
    const conditions = node.parameters?.conditions || {};
    const variableNames = getAvailableVariables(); // From designer state
    
    propertiesHtml += ConditionEditor.render(
        conditions,
        variableNames,
(newConditions) => {
   // Update node
     node.parameters.conditions = newConditions;
      
         // Trigger re-render to update ports
            rerenderNode(node.id);
     
            // Mark workflow as modified
     workflowModified = true;
  }
    );
}
```

**Acceptance Criteria:**
- [ ] ConditionNode shows condition editor (not generic param form)
- [ ] Other nodes still show generic parameter form
- [ ] Changing conditions immediately updates visual ports
- [ ] Variable list includes workflow variables
- [ ] Save/load preserves all conditions

---

### Task 3.3: Expression Validation

**File:** `source/web/wwwroot/js/designer/utils/ExpressionValidator.js` (NEW)

**Create expression parser/validator:**

```javascript
/**
 * ExpressionValidator - Validates condition expressions
 */
class ExpressionValidator {
    /**
     * Validate a condition expression
     * @param {string} expression - e.g., "sentiment == 'positive'"
     * @param {Array<string>} availableVariables - Variable names
     * @returns {{ valid: boolean, error?: string, usedVariables?: string[] }}
     */
    static validate(expression, availableVariables) {
  // Parse expression
  // Check for syntax errors
      // Check variable references exist
        // Return validation result
    }
    
    /**
     * Extract variable references from expression
   * @param {string} expression
     * @returns {string[]} Variable names used
  */
    static extractVariables(expression) {
        // Return array of variable names
    }
    
    /**
     * Get suggestions for autocomplete
     * @param {string} partialExpression
     * @param {Array<string>} availableVariables
     * @returns {Array<{text: string, description: string}>}
     */
    static getSuggestions(partialExpression, availableVariables) {
      // Return autocomplete suggestions
    }
}
```

**Validation Rules:**
1. Must contain at least one comparison operator (`==`, `!=`, `>`, `<`, `>=`, `<=`)
2. Variable names must exist in `availableVariables`
3. String literals must be in quotes
4. Balanced parentheses
5. No dangerous characters (semicolons, script tags, etc.)

**Example Valid Expressions:**
- `sentiment == 'positive'`
- `score > 7`
- `priority >= 5 && category == 'urgent'`
- `text.length > 100`
- `status != 'completed'`

**Acceptance Criteria:**
- [ ] Detects syntax errors
- [ ] Detects undefined variable references
- [ ] Provides helpful error messages
- [ ] Extracts variable dependencies
- [ ] Suggests completions while typing

---

### Task 3.4: Variable Autocomplete

**File:** `source/web/wwwroot/js/designer/ui/VariableAutocomplete.js` (NEW)

**Create autocomplete dropdown:**

```javascript
/**
 * VariableAutocomplete - Autocomplete for variable names in expressions
 */
class VariableAutocomplete {
    /**
     * Attach autocomplete to an input element
     * @param {HTMLInputElement} input
   * @param {Array<string>} variables
     * @param {Function} onSelect
     */
    static attach(input, variables, onSelect) {
   // Listen for input events
    // Show dropdown on variable trigger (e.g., typing letter)
        // Filter suggestions
    // Handle keyboard navigation
        // Call onSelect when item chosen
    }
    
    /**
     * Show autocomplete dropdown
     */
    static show(input, suggestions, position) {
        // Create/show dropdown
    }
    
 /**
     * Hide autocomplete dropdown
     */
    static hide() {
        // Remove dropdown
    }
}
```

**Features:**
- Appears while typing variable names
- Arrow key navigation
- Enter to select
- Escape to dismiss
- Mouse click to select
- Shows variable type/description if available

**Acceptance Criteria:**
- [ ] Shows suggestions as user types
- [ ] Filters based on current input
- [ ] Keyboard navigation works
- [ ] Selecting inserts variable at cursor
- [ ] Dismisses on blur/escape
- [ ] Positions dropdown correctly

---

### Task 3.5: Real-time Port Updates

**File:** `source/web/wwwroot/js/designer/rendering.js` (MODIFY)

**Ensure ports update when conditions change:**

```javascript
/**
 * Re-render a specific node (preserves position and connections)
 */
function rerenderNode(nodeId) {
    const node = workflow.nodes.find(n => n.id === nodeId);
    if (!node) return;
    
    const nodeElement = document.querySelector(`[data-node-id="${nodeId}"]`);
    if (!nodeElement) return;
    
    // Save current position
    const x = parseInt(nodeElement.style.left);
    const y = parseInt(nodeElement.style.top);
    
    // Save current connections
    const nodeConnections = workflow.connections.filter(
        c => c.sourceNodeId === nodeId || c.targetNodeId === nodeId
    );
    
    // Remove old element
  nodeElement.remove();
    
    // Re-render with updated ports
    const schema = nodeSchemas[node.type];
    const newElement = NodeRenderer.renderNode(node, schema, false);
    newElement.style.left = `${x}px`;
    newElement.style.top = `${y}px`;
 canvas.appendChild(newElement);
    
    // Re-attach connections
    renderConnections();
    
    // Re-attach event listeners
    makeNodeDraggable(newElement);
 attachNodeEventListeners(newElement);
}
```

**Acceptance Criteria:**
- [ ] Node re-renders without position change
- [ ] Existing valid connections preserved
- [ ] Invalid connections removed (port no longer exists)
- [ ] New ports immediately connectable
- [ ] No visual flicker during update

---

### Task 3.6: Connection Validation for Conditional Ports

**File:** `source/web/wwwroot/js/designer/connections.js` (MODIFY)

**Enhance connection validation:**

```javascript
/**
 * Validate a connection before creating it
 */
function validateConnection(sourceNode, sourcePortId, targetNode, targetPortId) {
    // Existing validation...
    
// NEW: Validate conditional port rules
if (sourceNode.type === 'ConditionNode') {
        const conditions = sourceNode.parameters?.conditions || {};
        const validPortIds = [
            ...Object.keys(conditions),
            'default'
    ];
     
 if (!validPortIds.includes(sourcePortId)) {
       return {
           valid: false,
     error: `Port '${sourcePortId}' no longer exists on this node`
       };
        }
    }
    
    return { valid: true };
}
```

**Connection Rules:**
1. Each conditional port can have **multiple** outgoing connections (allows parallel routing)
2. Target nodes can have **one** incoming connection per input port
3. Deleting a condition removes all connections from that port
4. Cannot connect to non-existent ports

**Acceptance Criteria:**
- [ ] Can connect conditional port to multiple targets
- [ ] Invalid connections show error message
- [ ] Deleting condition removes its connections
- [ ] Cannot create connection to deleted port

---

### Task 3.7: Sample Workflow Templates

**File:** `source/web/wwwroot/js/designer/templates/ConditionWorkflowTemplate.js` (NEW)

**Create example workflows:**

```javascript
/**
 * Sample workflow templates demonstrating conditional routing
 */
const ConditionWorkflowTemplates = {
    /**
     * Sentiment Analysis with 3-way routing
     */
    sentimentRouting: {
        name: "Sentiment-based Routing",
   description: "Route messages based on sentiment analysis",
        nodes: [
       {
           id: "llm-1",
         type: "LlmNode",
           name: "Analyze Sentiment",
x: 100, y: 100,
        parameters: {
                 provider: "openai",
  model: "gpt-4o",
 prompt: "Analyze sentiment: {{input_text}}"
   }
  },
      {
          id: "condition-1",
     type: "ConditionNode",
   name: "Route by Sentiment",
         x: 400, y: 100,
      parameters: {
          conditions: {
               "is_positive": "sentiment == 'positive'",
                   "is_negative": "sentiment == 'negative'"
  }
     }
   },
         {
          id: "log-positive",
         type: "LogNode",
            name: "Log Positive",
   x: 700, y: 50
    },
            {
  id: "log-negative",
   type: "LogNode",
           name: "Log Negative",
x: 700, y: 150
            },
    {
            id: "log-neutral",
       type: "LogNode",
            name: "Log Neutral",
     x: 700, y: 250
       }
  ],
        connections: [
            {
      sourceNodeId: "llm-1",
         sourcePortId: "output",
       targetNodeId: "condition-1",
     targetPortId: "input"
   },
     {
      sourceNodeId: "condition-1",
       sourcePortId: "is_positive",
      targetNodeId: "log-positive",
     targetPortId: "input"
        },
         {
         sourceNodeId: "condition-1",
                sourcePortId: "is_negative",
      targetNodeId: "log-negative",
     targetPortId: "input"
         },
            {
    sourceNodeId: "condition-1",
    sourcePortId: "default",
    targetNodeId: "log-neutral",
     targetPortId: "input"
            }
]
    },
    
    /**
     * Priority-based escalation
     */
    priorityEscalation: {
        // Similar structure...
    }
};
```

**Acceptance Criteria:**
- [ ] Templates load correctly
- [ ] All connections valid
- [ ] Conditions configured properly
- [ ] Visually demonstrates conditional routing
- [ ] Can be used as starting point

---

### Task 3.8: Update Workflow Serialization

**File:** `source/web/wwwroot/js/designer/utils/serialization.js` (MODIFY)

**Ensure conditions serialize/deserialize correctly:**

```javascript
/**
 * Serialize workflow to JSON
 */
function serializeWorkflow() {
    return {
        // ...existing fields...
        nodes: workflow.nodes.map(node => ({
         id: node.id,
            type: node.type,
 name: node.name,
  position: { x: node.x, y: node.y },
    parameters: node.parameters // Includes conditions dictionary
        })),
connections: workflow.connections.map(conn => ({
            id: conn.id,
   sourceNodeId: conn.sourceNodeId,
            sourcePortId: conn.sourcePortId, // Must preserve
    targetNodeId: conn.targetNodeId,
            targetPortId: conn.targetPortId
        }))
 };
}

/**
 * Deserialize workflow from JSON
 */
function deserializeWorkflow(json) {
    // Load nodes
    workflow.nodes = json.nodes.map(n => ({
        ...n,
        parameters: n.parameters || {} // Ensure parameters exists
    }));
    
    // Load connections (with port IDs)
    workflow.connections = json.connections.map(c => ({
        id: c.id || generateId(),
        sourceNodeId: c.sourceNodeId,
        sourcePortId: c.sourcePortId || 'output', // Backward compat
      targetNodeId: c.targetNodeId,
 targetPortId: c.targetPortId || 'input'
    }));
    
    // Validate all connections (remove invalid ones)
    workflow.connections = workflow.connections.filter(conn => {
        const validation = validateConnection(
  workflow.nodes.find(n => n.id === conn.sourceNodeId),
    conn.sourcePortId,
            workflow.nodes.find(n => n.id === conn.targetNodeId),
conn.targetPortId
    );
        return validation.valid;
    });
}
```

**Acceptance Criteria:**
- [ ] Conditions save in node.parameters.conditions
- [ ] Port IDs save in connections
- [ ] Loading workflow recreates all conditions
- [ ] Invalid connections removed on load
- [ ] Backward compatible with old workflows

---

### Task 3.9: UI/UX Polish

**File:** `source/web/wwwroot/css/designer/designer-sidebar.css` (MODIFY)

**Add styling for condition editor:**

```css
/* Condition editor container */
.condition-editor {
    margin-top: 16px;
}

.condition-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-bottom: 12px;
}

.condition-item {
    display: grid;
    grid-template-columns: 120px 1fr 32px;
    gap: 8px;
    align-items: start;
    padding: 12px;
    background: #f8f9fa;
    border: 1px solid #dee2e6;
    border-radius: 6px;
}

.condition-item.invalid {
    border-color: #dc3545;
    background: #fff5f5;
}

.condition-name-input {
    font-size: 13px;
    font-weight: 500;
 padding: 6px 8px;
    border: 1px solid #ced4da;
    border-radius: 4px;
}

.condition-expression-input {
    font-family: 'Consolas', 'Monaco', monospace;
    font-size: 12px;
    padding: 6px 8px;
    border: 1px solid #ced4da;
    border-radius: 4px;
}

.condition-expression-input.invalid {
 border-color: #dc3545;
}

.condition-error {
    grid-column: 2 / 3;
    color: #dc3545;
    font-size: 11px;
    margin-top: 4px;
}

.condition-delete-btn {
    padding: 6px;
    background: transparent;
    border: 1px solid #dc3545;
    border-radius: 4px;
    color: #dc3545;
    cursor: pointer;
    transition: all 0.2s;
}

.condition-delete-btn:hover {
    background: #dc3545;
    color: white;
}

.add-condition-btn {
    width: 100%;
    padding: 8px;
    background: #e7f3ff;
    border: 1px dashed #4a90e2;
    border-radius: 4px;
    color: #4a90e2;
    cursor: pointer;
    font-weight: 500;
 transition: all 0.2s;
}

.add-condition-btn:hover {
    background: #4a90e2;
    color: white;
    border-style: solid;
}

/* Autocomplete dropdown */
.variable-autocomplete {
    position: absolute;
    background: white;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    max-height: 200px;
    overflow-y: auto;
    z-index: 1000;
}

.autocomplete-item {
    padding: 8px 12px;
    cursor: pointer;
    font-size: 13px;
}

.autocomplete-item:hover,
.autocomplete-item.selected {
    background: #e7f3ff;
}

.autocomplete-item-variable {
  font-family: 'Consolas', 'Monaco', monospace;
    color: #4a90e2;
    font-weight: 500;
}

.autocomplete-item-description {
    font-size: 11px;
    color: #6c757d;
    margin-top: 2px;
}
```

**Acceptance Criteria:**
- [ ] Condition items have clear visual hierarchy
- [ ] Invalid conditions highlighted in red
- [ ] Buttons have hover states
- [ ] Autocomplete dropdown styled consistently
- [ ] Responsive to container width
- [ ] Accessible (keyboard navigation, focus states)

---

### Task 3.10: Testing & Documentation

**Create test workflows:**

1. **Simple 2-way branch**
   - LLM ? ConditionNode (1 condition) ? 2 targets
   
2. **Multi-way routing**
   - ConditionNode with 5 conditions ? 6 targets (5 + default)
   
3. **Nested conditions**
   - ConditionNode ? ConditionNode ? multiple endpoints
   
4. **Variable references**
   - Conditions using workflow variables
   
5. **Complex expressions**
 - `(score > 7 && category == 'urgent') || priority >= 9`

**Documentation to create:**

```markdown
## docs/USER_GUIDE_CONDITIONAL_ROUTING.md

# Conditional Routing Guide

## Overview
Conditional routing allows workflows to branch based on data values...

## Adding Conditions
1. Add a ConditionNode to canvas
2. Select the node
3. Click "Add Condition" in properties panel
4. Enter condition name (e.g., "is_urgent")
5. Enter expression (e.g., "priority > 7")
6. Connect conditional port to target node

## Expression Syntax
- Comparison: ==, !=, >, <, >=, <=
- Logical: &&, ||, !
- Parentheses: ( )
- Variables: Any workflow variable name
- Strings: 'single quotes'
- Numbers: 123, 45.67

## Examples
...
```

**Acceptance Criteria:**
- [ ] All test workflows work correctly
- [ ] User guide complete
- [ ] API documentation updated
- [ ] Code comments added
- [ ] No console errors

---

## ?? Progress Tracking

| Task | Status | Assignee | Completion |
|------|--------|----------|------------|
| 3.1 - Condition Editor UI | ?? Not Started | - | 0% |
| 3.2 - Properties Panel Integration | ?? Not Started | - | 0% |
| 3.3 - Expression Validation | ?? Not Started | - | 0% |
| 3.4 - Variable Autocomplete | ?? Not Started | - | 0% |
| 3.5 - Real-time Port Updates | ?? Not Started | - | 0% |
| 3.6 - Connection Validation | ?? Not Started | - | 0% |
| 3.7 - Sample Templates | ?? Not Started | - | 0% |
| 3.8 - Serialization Updates | ?? Not Started | - | 0% |
| 3.9 - UI/UX Polish | ?? Not Started | - | 0% |
| 3.10 - Testing & Docs | ?? Not Started | - | 0% |

**Overall Phase 3 Progress:** 0%

---

## ?? Visual Mockup

### Before Phase 3 (Properties Panel)
```
?? Properties: Condition Node ??????
? Name: Check Priority           ?
?          ?
? Parameters:         ?
? ???????????????????????????????  ?
? ? conditions: {...} ?  ?
? ? (raw JSON editor)     ?  ?
? ???????????????????????????????  ?
?????????????????????????????????????
```

### After Phase 3 (Properties Panel)
```
?? Properties: Condition Node ??????
? Name: Check Priority ?
?           ?
? Conditions: ?
? ???????????????????????????????  ?
? ? Name: is_urgent             ?  ?
? ? Expr: priority > 7      ? X?
? ???????????????????????????????  ?
? ???????????????????????????????  ?
? ? Name: is_high     ?  ?
? ? Expr: priority >= 5         ? X?
? ???????????????????????????????  ?
? [ + Add Condition ]    ?
?   ?
? Available Variables:            ?
? • priority • category • status    ?
?????????????????????????????????????
```

### Canvas View (Multi-branch)
```
    ?????????????????
 ?  LLM Node     ?
    ?  Analyze      ?
    ?      ?        ? output
    ?????????????????
   ?
     ?
    ?????????????????
    ?  Condition    ?
    ?  Route by     ?
? ?            ?? is_urgent   ?????? [Alert Handler]
 ?          ?? is_high   ?????? [Queue Handler]
    ?          ?? default     ?????? [Log Handler]
 ?????????????????
```

---

## ?? Implementation Order

**Week 4 Schedule:**

**Day 1-2:**
- Task 3.3 - Expression Validation (foundation)
- Task 3.1 - Condition Editor UI (core component)

**Day 3:**
- Task 3.4 - Variable Autocomplete
- Task 3.2 - Properties Panel Integration

**Day 4:**
- Task 3.5 - Real-time Port Updates
- Task 3.6 - Connection Validation

**Day 5:**
- Task 3.8 - Serialization
- Task 3.9 - UI/UX Polish
- Task 3.7 - Sample Templates
- Task 3.10 - Testing & Documentation

---

## ?? Dependencies

**Requires from Previous Phases:**
- ? Phase 1: Port definitions in schema
- ? Phase 1: NodeCapabilities.SupportsConditionalRouting
- ? Phase 2: Multi-port rendering
- ? Phase 2: getDynamicConditionPorts()
- ? Phase 2: Connection port ID tracking

**Enables for Future Phases:**
- Phase 4: Execution options (uses similar editor pattern)
- Phase 5: Loop containers (condition-based iteration)
- Phase 7: Workflow execution with branching

---

## ?? Notes

### Design Decisions

1. **Condition Storage Format:**
   - Store as `{ conditionName: expression }` dictionary
   - Simple, serializable, human-readable
   - Matches C# ConditionNode constructor pattern

2. **Port ID Convention:**
   - Condition port IDs = condition names
   - Always include "default" port
   - Port order: defined conditions first, then default

3. **Expression Language:**
   - Keep it simple: JavaScript-like syntax
   - No full JavaScript eval (security)
   - Limited to safe comparisons and operators
   - Server-side C# will re-evaluate expressions

4. **Validation Strategy:**
   - Client-side validation is UX only
   - Server-side validation is authoritative
   - Show warnings, don't block saving

### Known Limitations

1. **No Complex Logic:** Expressions limited to comparisons (no functions, loops, etc.)
2. **No Type Checking:** Variables assumed to exist at runtime
3. **No Debugging:** Can't test expressions in designer (Phase 7)
4. **No Expression Library:** No saved/reusable expressions

### Future Enhancements (Post-Phase 3)

- Visual expression builder (drag-and-drop conditions)
- Expression testing with sample data
- Saved expression templates
- AI-assisted expression generation
- Multi-language expression support (C#, JavaScript, Python)

---

**Phase 3 Complete when:** Users can create complex conditional workflows entirely through the UI without editing JSON.

**Next Phase:** Phase 4 - Node Execution Options (retry, timeout, error handling UI)
