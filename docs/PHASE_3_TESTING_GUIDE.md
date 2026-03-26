# Phase 3 - Testing Guide

**Version:** 1.0  
**Date:** January 25, 2025  
**Status:** Ready for Testing  

---

## ?? What to Test

This guide covers testing the **Condition Node Enhancement** features implemented in Phase 3.

---

## ? Prerequisites

1. **Start the web project:**
   ```bash
   cd source/web
   dotnet run
   ```

2. **Open browser:**
   - Navigate to `http://localhost:5000` (or your configured port)
   - Login if required
   - Go to Workflows section

3. **Create or open a workflow:**
   - Click "Create New Workflow" or edit existing
   - Open the Workflow Designer

---

## ?? Test Scenarios

### Test 1: Basic Condition Editor

**Steps:**
1. Drag **ConditionNode** from palette to canvas
2. Click the node to select it
3. **Expected:** Properties panel shows visual condition editor (NOT raw JSON input)
4. Click **"+ Add Condition"** button
5. **Expected:** New condition row appears with:
   - Name input field (filled with "condition_1")
   - Expression input field (empty)
   - Delete button (???)

**Pass Criteria:**
- ? Condition editor renders correctly
- ? Add button works
- ? Auto-generated name appears
- ? All fields visible and styled

---

### Test 2: Add Multiple Conditions

**Steps:**
1. Click **"+ Add Condition"** 3 times
2. **Expected:** See 3 condition rows:
   - `condition_1`
   - `condition_2`
   - `condition_3`
3. Fill in conditions:
   - Name: `is_urgent`, Expression: `priority > 7`
   - Name: `is_high`, Expression: `priority >= 5`
   - Name: `is_normal`, Expression: `priority < 5`

**Pass Criteria:**
- ? Can add unlimited conditions
- ? Each has unique auto-generated name
- ? Can edit both name and expression
- ? Layout remains clean

---

### Test 3: Expression Validation - Valid Expressions

**Steps:**
1. Add a condition
2. Enter name: `test_valid`
3. Enter each expression below and click outside input:

| Expression | Expected Result |
|------------|----------------|
| `priority > 7` | ? Green/normal border |
| `status == 'urgent'` | ? Green/normal border |
| `score >= 5 && active == true` | ? Green/normal border |
| `(count > 10 \|\| flag == true)` | ? Green/normal border |

**Pass Criteria:**
- ? All valid expressions accepted
- ? No red border
- ? No error message

---

### Test 4: Expression Validation - Invalid Expressions

**Steps:**
1. Add a condition
2. Enter name: `test_invalid`
3. Enter each expression below and observe validation:

| Expression | Expected Error |
|------------|----------------|
| `priority >` | "Expression must contain a comparison operator" |
| `unknown_var > 5` | "Undefined variable(s): unknown_var" |
| `status == urgent` | "String values must be in single quotes" |
| `priority > (7` | "Unbalanced parentheses" |
| `alert('xss')` | "Expression contains invalid characters" |

**Pass Criteria:**
- ? Red border on invalid expressions
- ? Error message shows below input
- ? Error message is helpful
- ? Dangerous code rejected

---

### Test 5: Variable Autocomplete

**Prerequisites:**
- Create workflow variables first:
  1. Switch to Variables tab
  2. Click "Add"
  3. Create variables: `priority`, `status`, `category`

**Steps:**
1. Add a ConditionNode
2. Add a condition
3. Click in expression input
4. Start typing: `pr`
5. **Expected:** Autocomplete dropdown appears showing `priority`
6. Use **Arrow Down** key
7. **Expected:** Selection moves down
8. Press **Enter** or **Tab**
9. **Expected:** Variable `priority` inserted at cursor
10. Continue typing: ` > 7`
11. **Expected:** Full expression: `priority > 7`

**Pass Criteria:**
- ? Autocomplete appears while typing
- ? Filters variables by prefix
- ? Arrow keys navigate
- ? Enter/Tab selects
- ? Escape dismisses
- ? Variable inserted correctly
- ? Cursor positioned after variable

---

### Test 6: Available Variables Hint

**Prerequisites:**
- Have at least 1 workflow variable defined

**Steps:**
1. Add ConditionNode
2. Add condition
3. **Expected:** See hint below inputs:
   - "Available Variables: priority, status, category"
   - (in yellow/info box)

**Pass Criteria:**
- ? Hint displays correctly
- ? Shows all variable names
- ? Updates when variables change

---

### Test 7: Delete Condition

**Steps:**
1. Add 3 conditions
2. Click delete button (???) on middle condition
3. **Expected:** Confirmation dialog appears
4. Click **OK**
5. **Expected:** Condition removed from list
6. **Expected:** Remaining 2 conditions still visible

**Pass Criteria:**
- ? Confirmation dialog shows
- ? Can cancel deletion
- ? Condition removed on confirm
- ? Other conditions unaffected

---

### Test 8: Rename Condition

**Steps:**
1. Add condition with name `condition_1`
2. Change name to `is_urgent`
3. Click outside name input
4. **Expected:** Name updates, no error

**Try invalid names:**
5. Change to `123invalid` (starts with number)
6. **Expected:** Error message, name reverts
7. Change to `has-dash` (contains hyphen)
8. **Expected:** Error message, name reverts

**Pass Criteria:**
- ? Can rename to valid names
- ? Invalid names rejected
- ? Helpful error messages
- ? Cannot duplicate names

---

### Test 9: Syntax Help Section

**Steps:**
1. Add ConditionNode
2. Look for **"Expression Syntax Help"** section
3. Click to expand
4. **Expected:** See:
   - Supported operators
   - Example expressions
   - Usage descriptions

**Pass Criteria:**
- ? Help section visible
- ? Collapsible/expandable
- ? Contains useful information
- ? Examples are correct

---

### Test 10: Save and Reload Workflow

**Steps:**
1. Add ConditionNode with 3 conditions:
   - `is_urgent: priority > 7`
   - `is_high: priority >= 5`
 - `is_normal: priority < 5`
2. Click **Save** button in toolbar
3. **Expected:** "Saved!" message appears
4. Refresh browser page (F5)
5. **Expected:** Workflow reloads
6. Click the ConditionNode
7. **Expected:** All 3 conditions still present with correct values

**Pass Criteria:**
- ? Conditions save correctly
- ? Conditions load correctly
- ? No data loss
- ? Expressions intact

---

### Test 11: Empty State

**Steps:**
1. Add ConditionNode
2. Select node
3. **Expected:** See message:
   - "No conditions defined. Click 'Add Condition' to create one."
4. Add a condition
5. Delete it
6. **Expected:** Empty state message returns

**Pass Criteria:**
- ? Empty state shows when no conditions
- ? Message is clear
- ? Styled consistently

---

### Test 12: Keyboard Shortcuts

**Steps:**
1. Add condition
2. Focus on expression input
3. Type expression and press **Enter**
4. **Expected:** 
   - Expression saved
   - New condition added automatically
   - Focus on new condition's expression input

**Pass Criteria:**
- ? Enter saves and adds new
- ? Focus management works
- ? Smooth user experience

---

### Test 13: Multi-Node Workflow

**Steps:**
1. Create workflow:
   ```
   LLM Node ? Condition Node ? 3 downstream nodes
   ```
2. Configure ConditionNode with 2 conditions
3. Try to connect output ports
4. **Expected (Phase 2 already implemented):**
   - See multiple output ports on ConditionNode
   - Each condition has its own port
 - Plus "default" port

**Pass Criteria:**
- ? Multiple ports visible
- ? Ports labeled correctly
- ? Can connect each port
- ? Connections persist

---

### Test 14: Browser Compatibility

**Test in multiple browsers:**
- Chrome/Edge
- Firefox
- Safari (if available)

**For each browser:**
1. Open designer
2. Add ConditionNode
3. Add conditions
4. Test autocomplete
5. Test validation

**Pass Criteria:**
- ? Works in Chrome/Edge
- ? Works in Firefox
- ? Works in Safari
- ? Consistent appearance
- ? No console errors

---

### Test 15: Responsive Design

**Steps:**
1. Open designer
2. Resize browser window to narrow width
3. **Expected:** Properties panel adapts
4. Add ConditionNode
5. **Expected:** Condition editor still usable

**Pass Criteria:**
- ? Layout doesn't break
- ? All buttons accessible
- ? Text readable
- ? No horizontal scroll

---

## ?? Bug Report Template

If you find a bug, please report using this template:

```markdown
## Bug Report

**Test Scenario:** [Test number and name]

**Steps to Reproduce:**
1. 
2. 
3. 

**Expected Behavior:**
[What should happen]

**Actual Behavior:**
[What actually happened]

**Screenshots:**
[If applicable]

**Browser:**
[Chrome 120, Firefox 121, etc.]

**Console Errors:**
[Copy any errors from browser console]
```

---

## ? Success Checklist

After testing all scenarios, verify:

- [ ] All 15 test scenarios pass
- [ ] No console errors in browser
- [ ] UI is responsive and looks good
- [ ] Performance is acceptable
- [ ] Save/load works correctly
- [ ] No data corruption
- [ ] Validation is helpful
- [ ] Autocomplete is intuitive
- [ ] Help text is clear
- [ ] No accessibility issues

---

## ?? Test Results Template

Use this to track your testing:

| Test | Status | Notes |
|------|--------|-------|
| 1. Basic Condition Editor | ? | |
| 2. Add Multiple Conditions | ? | |
| 3. Valid Expressions | ? | |
| 4. Invalid Expressions | ? | |
| 5. Variable Autocomplete | ? | |
| 6. Available Variables Hint | ? | |
| 7. Delete Condition | ? | |
| 8. Rename Condition | ? | |
| 9. Syntax Help | ? | |
| 10. Save and Reload | ? | |
| 11. Empty State | ? | |
| 12. Keyboard Shortcuts | ? | |
| 13. Multi-Node Workflow | ? | |
| 14. Browser Compatibility | ? | |
| 15. Responsive Design | ? | |

**Legend:** ? Not Tested | ? Pass | ? Fail | ?? Issues Found

---

## ?? Quick Test (5 minutes)

If you have limited time, run this quick test:

1. Add ConditionNode
2. Add 2 conditions with expressions
3. Test autocomplete (if variables exist)
4. Save and reload workflow
5. Verify conditions persisted

**If all work:** ? Core functionality is working!

---

## ?? Next Steps After Testing

1. **Document bugs** found during testing
2. **Fix critical bugs** before proceeding
3. **Move to Phase 3 remaining tasks:**
   - Task 3.5: Optimize port updates
   - Task 3.6: Connection validation
   - Task 3.7: Sample templates
   - Task 3.8: Final serialization testing

---

**Happy Testing! ??**
