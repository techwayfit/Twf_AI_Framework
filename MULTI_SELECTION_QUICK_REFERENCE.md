# Multi-Selection Quick Reference

## ??? Mouse Actions

| Action | Result |
|--------|--------|
| **Click node** | Select node (clear others) |
| **Ctrl+Click node** | Toggle node in/out of selection |
| **Drag on empty canvas** | Draw selection rectangle |
| **Drag selected node** | Move all selected nodes together |
| **Click empty canvas** | Deselect all |

## ?? Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Ctrl+A** | Select all nodes |
| **Ctrl+Click** | Toggle node selection |
| **Delete** | Delete selected nodes |
| **Escape** | Deselect all |
| **Ctrl+S** | Save workflow |

## ?? Visual Indicators

### Single Selection
```
???????????????
? ? Node Name ? ? Red border
?  LlmNode    ?
???????????????

Properties Panel:
?? Node Name: [____]
?? Parameters
?? [Edit fields...]
```

### Multi-Selection
```
???????????????  ???????????????
? ? Node 1    ?  ? ? Node 2    ? ? Red borders
???????????????  ???????????????

Properties Panel:
?? 2 nodes selected
[??? Delete Selected]
[? Deselect All]
```

### Selection Rectangle
```
Canvas:
????????????????????????
?  ????????????????    ?
?  ? ?????????? ? ? Dashed blue border   ?
?  ? ? [Node] ? ? ? Semi-transparent fill  ?
?  ? ?????????? ?       ?
?  ????????????????    ?
????????????????????????
```

## ?? Common Workflows

### Selecting Multiple Nodes

**Method 1: Drag-to-Select**
```
1. Click empty canvas area
2. Hold and drag to create rectangle
3. Release ? All nodes in rectangle selected
```

**Method 2: Ctrl+Click**
```
1. Click first node ? Selected
2. Ctrl+Click second node ? Both selected
3. Ctrl+Click third node ? All three selected
```

**Method 3: Select All**
```
1. Press Ctrl+A ? All nodes selected
```

### Moving Multiple Nodes

```
1. Select multiple nodes (any method above)
2. Click and drag any selected node
3. All selected nodes move together
4. Release to drop
```

### Deleting Multiple Nodes

```
1. Select nodes to delete
2. Press Delete key
3. Confirm deletion dialog
4. All selected nodes removed
```

### Adding to Existing Selection

```
1. Nodes A and B are selected
2. Hold Ctrl
3. Drag rectangle over Node C
4. A, B, and C now selected
```

### Removing from Selection

```
1. Nodes A, B, and C are selected
2. Hold Ctrl
3. Click Node B
4. A and C remain selected
```

## ?? Selection Behavior

### Click Without Ctrl

| Starting State | Action | Result |
|----------------|--------|--------|
| Nothing selected | Click Node A | A selected |
| A selected | Click Node B | B selected (A deselected) |
| A, B selected | Click Node C | C selected (A, B deselected) |
| A, B selected | Click A again | A, B remain selected (start drag) |

### Click With Ctrl

| Starting State | Action | Result |
|----------------|--------|--------|
| Nothing selected | Ctrl+Click A | A selected |
| A selected | Ctrl+Click B | A, B selected |
| A, B selected | Ctrl+Click A | B selected (A removed) |
| A, B selected | Ctrl+Click C | A, B, C selected |

### Rectangle Selection Without Ctrl

| Starting State | Action | Result |
|----------------|--------|--------|
| Nothing selected | Drag rect over A, B | A, B selected |
| C selected | Drag rect over A, B | A, B selected (C deselected) |
| A, C selected | Drag rect over B | B selected (A, C deselected) |

### Rectangle Selection With Ctrl

| Starting State | Action | Result |
|----------------|--------|--------|
| A selected | Ctrl+Drag rect over B, C | A, B, C selected |
| A, B selected | Ctrl+Drag rect over C, D | A, B, C, D selected |

## ?? Visual States

### Unselected Node
```
???????????????
?  Node Name  ? ? Blue border
?  NodeType   ?
???????????????
```

### Selected Node (Hover)
```
???????????????
? ? Node Name ? ? Red border + glow
?  NodeType   ? ? Cursor: move
???????????????
     ?
```

### During Group Drag
```
???????????????  ???????????????
? ? Node 1    ?  ? ? Node 2    ?
?  Moving...?  ?  Moving...  ?
???????????????  ???????????????
   ?      ?
      Moving together
```

## ?? Practical Examples

### Example 1: Reorganize Workflow

**Goal:** Move all AI nodes to the left side

```
1. Ctrl+Click all LLM, Prompt Builder nodes
2. Drag group to left side
3. All nodes move together maintaining spacing
```

### Example 2: Delete Test Nodes

**Goal:** Remove all temporary debug nodes

```
1. Drag rectangle over debug section
2. Press Delete
3. Confirm ? All debug nodes removed
```

### Example 3: Select All Transform Nodes

**Goal:** Visually identify all data transformation nodes

```
Current: Manual selection with Ctrl+Click
Future: "Select by type" feature (planned)
```

### Example 4: Create Sub-Workflow

**Goal:** Group related nodes together

```
Current: Select nodes and drag to organize
Future: "Create Group" feature (planned)
```

## ? Performance Tips

### For Large Workflows (100+ nodes)

- Use **Ctrl+A** sparingly (selects all)
- Use **targeted rectangle selection** instead
- **Zoom out** before selecting large groups
- **Deselect** (Escape) when done to improve rendering

### For Precise Selection

- **Zoom in** before drawing selection rectangle
- Use **Ctrl+Click** for individual nodes
- **Click empty canvas** to deselect and start over

## ?? Known Limitations

| Limitation | Workaround |
|------------|------------|
| Can't select nodes across zoom levels | Reset zoom to 100% first |
| Selection rect doesn't show count | Check properties panel |
| No undo for bulk delete | Save before deleting |
| No copy/paste yet | Planned feature |
| Can't save selection as group | Planned feature |

## ?? Pro Tips

1. **Quick multi-select**: Hold Ctrl and click rapidly
2. **Partial intersect selects**: Even if rectangle only touches node, it selects
3. **Preserve selection**: Click selected node without Ctrl to drag group
4. **Clear and start over**: Click empty canvas or press Escape
5. **Check count**: Properties panel shows "X nodes selected"
6. **Keyboard only**: Tab through nodes, Space to select (future feature)

## ?? Coming Soon

- **Alignment tools**: Align left/right/top/bottom
- **Distribution**: Space nodes evenly
- **Copy/Paste**: Duplicate selections (Ctrl+C, Ctrl+V)
- **Undo/Redo**: Revert multi-node operations
- **Lasso selection**: Free-form selection
- **Select by category**: All AI nodes, all Data nodes, etc.
- **Bulk parameter edit**: Edit common parameters together

---

**Quick Start:**
1. **Drag on canvas** to select multiple nodes
2. **Drag any selected node** to move group
3. **Press Delete** to remove selection
4. **Press Escape** to clear selection

**Enjoy your new multi-selection powers!** ??
