# Phase 3 Quick Start Guide

**Phase:** Condition Node Enhancement  
**Status:** ?? Ready to Start  
**Priority:** High  
**Estimated Time:** 1 week  

---

## ?? What We're Building

A complete UI for configuring conditional routing in workflows. Users will be able to:

1. Add named conditions to ConditionNode (e.g., "is_urgent", "is_high_priority")
2. Write expressions for each condition (e.g., `priority > 7`)
3. See output ports appear automatically for each condition
4. Connect each condition's port to different downstream nodes
5. Get real-time validation and autocomplete for expressions

---

## ?? Implementation Checklist

### ? Prerequisites (COMPLETE)
- [x] Phase 1: Backend schema supports conditional ports
- [x] Phase 2: Multi-port visual rendering working
- [x] NodeRenderer.getDynamicConditionPorts() implemented
- [x] Connection model tracks port IDs

### ?? Phase 3 Tasks (IN PROGRESS)

#### Week 4 - Day 1-2: Core Components
- [ ] **Task 3.3** - Expression Validation
  - Create `ExpressionValidator.js`
  - Implement expression parser
  - Define validation rules
  - Test with sample expressions

- [ ] **Task 3.1** - Condition Editor UI
  - Create `ConditionEditor.js`
  - Implement render() method
  - Add condition add/remove/edit logic
  - Style condition items

#### Week 4 - Day 3: Enhancement Features
- [ ] **Task 3.4** - Variable Autocomplete
  - Create `VariableAutocomplete.js`
  - Implement dropdown logic
  - Add keyboard navigation
  - Style autocomplete

- [ ] **Task 3.2** - Properties Panel Integration
  - Modify `PropertiesPanel.js`
  - Detect ConditionNode
  - Render ConditionEditor
  - Wire up onChange handlers

#### Week 4 - Day 4: Real-time Updates
- [ ] **Task 3.5** - Real-time Port Updates
  - Create `rerenderNode()` function
  - Preserve positions and connections
  - Update ports dynamically
  - Test no flicker

- [ ] **Task 3.6** - Connection Validation
  - Enhance `validateConnection()`
  - Handle deleted ports
  - Validate conditional connections
  - Show error messages

#### Week 4 - Day 5: Polish & Ship
- [ ] **Task 3.8** - Serialization
  - Update serialize/deserialize
  - Test save/load
  - Backward compatibility

- [ ] **Task 3.9** - UI/UX Polish
  - Add CSS for condition editor
  - Style autocomplete dropdown
  - Test accessibility

- [ ] **Task 3.7** - Sample Templates
  - Create sentiment routing template
  - Create priority escalation template
  - Add template loader

- [ ] **Task 3.10** - Testing & Docs
  - Test all workflows
  - Write user guide
  - Update code comments
  - Final QA

---

## ??? Files to Create

### New Files (7)
1. `source/web/wwwroot/js/designer/ui/ConditionEditor.js`
2. `source/web/wwwroot/js/designer/utils/ExpressionValidator.js`
3. `source/web/wwwroot/js/designer/ui/VariableAutocomplete.js`
4. `source/web/wwwroot/js/designer/templates/ConditionWorkflowTemplate.js`
5. `docs/USER_GUIDE_CONDITIONAL_ROUTING.md`

### Files to Modify (4)
1. `source/web/wwwroot/js/designer/ui/PropertiesPanel.js`
2. `source/web/wwwroot/js/designer/rendering.js`
3. `source/web/wwwroot/js/designer/connections.js`
4. `source/web/wwwroot/css/designer/designer-sidebar.css`

---

## ?? Visual Goal

### Before Phase 3
```
Properties Panel:
???????????????????????
? ConditionNode  ?
? parameters: {...}   ?  ? Raw JSON
???????????????????????
```

### After Phase 3
```
Properties Panel:
???????????????????????????????
? ConditionNode ?
?   ?
? Conditions:  ?
? ??????????????????????????? ?
? ? is_urgent: priority > 7 ? ?
? ??????????????????????????? ?
? ??????????????????????????? ?
? ? is_high: priority >= 5  ? ?
? ??????????????????????????? ?
? [ + Add Condition ]   ?
? ?
? Variables: priority, status ?
???????????????????????????????

Canvas:
???????????????
? Condition   ?
?  ?       ?? is_urgent  ??? [Alert]
?        ?? is_high    ??? [Queue]
??? default    ??? [Log]
???????????????
```

---

## ?? Key Concepts

### 1. Condition Data Model
```javascript
// Stored in node.parameters.conditions
{
  "is_urgent": "priority > 7",
  "is_high": "priority >= 5",
  "is_positive": "sentiment == 'positive'"
}
```

### 2. Port Generation
```javascript
// getDynamicConditionPorts() creates ports from conditions
const conditions = node.parameters?.conditions || {};
const ports = [
  ...Object.keys(conditions).map(name => ({
    id: name,
    label: name.replace(/_/g, ' '),
    type: 'Conditional'
  })),
  { id: 'default', label: 'Default', type: 'Conditional' }
];
```

### 3. Connection Format
```javascript
{
  sourceNodeId: "cond-1",
  sourcePortId: "is_urgent",  // ? Condition name
  targetNodeId: "alert-1",
  targetPortId: "input"
}
```

### 4. Expression Validation
```javascript
const valid = ExpressionValidator.validate(
  "priority > 7",
  ["priority", "status", "category"]
);
// Returns: { valid: true }

const invalid = ExpressionValidator.validate(
  "unknown_var > 5",
  ["priority"]
);
// Returns: { valid: false, error: "Variable 'unknown_var' not found" }
```

---

## ?? Testing Strategy

### Manual Tests
1. **Add condition** - Click "Add Condition", see new port appear
2. **Edit condition** - Change expression, port updates
3. **Delete condition** - Remove condition, port disappears, connections removed
4. **Variable autocomplete** - Type variable name, see suggestions
5. **Invalid expression** - Enter invalid syntax, see error
6. **Connect ports** - Connect each conditional port to different nodes
7. **Save/Load** - Save workflow, reload, verify conditions preserved
8. **Multi-way routing** - Create 5+ conditions, test all branches

### Automated Tests (Future)
- Unit tests for ExpressionValidator
- Integration tests for ConditionEditor
- E2E tests for complete workflows

---

## ?? Implementation Tips

### 1. Start Small
Begin with basic add/remove conditions, then add validation, then autocomplete.

### 2. Use Existing Patterns
Follow patterns from Phase 0 and Phase 2:
- BaseNode for structure
- NodeRenderer for rendering
- Event-driven updates

### 3. Real-time Updates
Every change should trigger:
```javascript
function onConditionChange(newConditions) {
  node.parameters.conditions = newConditions;
  rerenderNode(node.id);
  workflowModified = true;
}
```

### 4. Validation is UX, Not Security
Client-side validation is for UX only. Server will validate on execution.

### 5. Keep It Simple
Don't try to build a full expression language. Support basic comparisons:
- `==`, `!=`, `>`, `<`, `>=`, `<=`
- `&&`, `||`, `!`
- String literals in quotes
- Variable names

---

## ?? Quick Start Commands

### 1. Create New Files
```bash
# Create new JavaScript files
New-Item -Path "source/web/wwwroot/js/designer/ui/ConditionEditor.js" -ItemType File
New-Item -Path "source/web/wwwroot/js/designer/utils/ExpressionValidator.js" -ItemType File
New-Item -Path "source/web/wwwroot/js/designer/ui/VariableAutocomplete.js" -ItemType File
New-Item -Path "source/web/wwwroot/js/designer/templates/ConditionWorkflowTemplate.js" -ItemType File
```

### 2. Test in Browser
1. Run the web project: `dotnet run --project source/web`
2. Open designer: `http://localhost:5000/workflow/designer`
3. Add ConditionNode from palette
4. Open properties panel
5. Test condition editor

### 3. Debug Tips
- Use browser DevTools Console for JavaScript errors
- Use Network tab to verify scripts load
- Use Elements tab to inspect DOM structure
- Add `console.log()` liberally during development

---

## ?? Reference Documentation

- **Phase 3 Implementation Plan**: `docs/PHASE_3_IMPLEMENTATION.md`
- **Phase 1 Summary**: `docs/PHASE_1_IMPLEMENTATION.md` (schema details)
- **Phase 2 Summary**: `docs/PHASE_2_IMPLEMENTATION.md` (port rendering)
- **CSS Architecture**: `docs/CSS_ARCHITECTURE.md`
- **Designer Enhancement Plan**: `docs/DESIGNER_ENHANCEMENT_PLAN.md`

---

## ? Success Indicators

You'll know Phase 3 is complete when:

1. ? User can create a workflow entirely in the UI:
   ```
   LLM ? ConditionNode (3 conditions) ? 4 target nodes
   ```

2. ? Autocomplete works when typing variable names

3. ? Invalid expressions show helpful errors

4. ? Ports update in real-time as conditions change

5. ? Save/load preserves all conditions and connections

6. ? No console errors

7. ? User guide documentation complete

---

## ?? Next Steps After Phase 3

Once Phase 3 is complete, you'll be ready for:

**Phase 4: Node Execution Options**
- Retry configuration UI
- Timeout settings
- Error handling options
- Uses same editor pattern as conditions

**Phase 5: Special Node Types**
- Loop/ForEach containers
- Parallel execution nodes
- Subworkflow nodes

---

**Ready to start?** Begin with Task 3.3 (Expression Validation) - it's the foundation for everything else!

**Questions?** Refer to `docs/PHASE_3_IMPLEMENTATION.md` for detailed specs.

**Stuck?** Check existing code in Phase 0/1/2 for patterns.

---

**Good luck! ??**
