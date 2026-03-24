# Multi-Node Selection and Group Operations

## Overview

The workflow designer now supports **multi-node selection** with drag-to-select functionality, allowing you to select and manipulate multiple nodes simultaneously.

## Features Added

### ? 1. Drag-to-Select Rectangle

Click and drag on the canvas (not on nodes) to create a selection rectangle. All nodes within or intersecting the rectangle will be selected.

```
????????????????????????????????????????????
? Canvas ?
?           ?
?   ??????????????????      ?
?   ? ?????????????? ? ? Selection Rectangle  ?
?   ? ? [Node 1] ?   ?   ?
?   ? ?          ?   ?    ?
?   ? ? [Node 2] ?   ?      ?
?   ? ?????????????? ?  ?
?   ??????????????????    ?
?    ?
?  [Node 3]    [Node 4]  ?
?        ?
????????????????????????????????????????????

Result: Node 1 and Node 2 are selected
```

### ? 2. Ctrl+Click to Toggle Selection

Hold **Ctrl** and click nodes to add/remove them from the selection.

```
Click Node 1 ? Node 1 selected
Ctrl+Click Node 2   ? Node 1, Node 2 selected
Ctrl+Click Node 1   ? Node 2 selected (Node 1 deselected)
Ctrl+Click Node 3   ? Node 2, Node 3 selected
```

### ? 3. Group Dragging

When multiple nodes are selected, dragging any selected node moves **all selected nodes** together, maintaining their relative positions.

```
Before:
[A]????
 ???>[D]
[B]????

After dragging A, B, and C together:
      [A]????
             ???>[D]
     [B]????

(D stays in place if not selected)
```

### ? 4. Multi-Node Delete

Delete all selected nodes at once with the **Delete** key or delete button.

```
Select multiple nodes ? Press Delete ? Confirm ? All deleted
```

### ? 5. Select All (Ctrl+A)

Press **Ctrl+A** to select all nodes in the workflow.

### ? 6. Visual Feedback

- **Selection Rectangle**: Dashed blue border with semi-transparent fill
- **Selected Nodes**: Red border with glow effect
- **Multi-Selection Panel**: Shows count and action buttons

## User Interface Changes

### Properties Panel (Multi-Selection)

When multiple nodes are selected, the properties panel shows:

```
???????????????????????????????????
? ?? 3 nodes selected  ?
???????????????????????????????????
??
? [??? Delete Selected]   ?
?       ?
? [? Deselect All]          ?
?           ?
???????????????????????????????????
```

### Properties Panel (Single Selection)

When one node is selected, shows the normal parameter editing interface.

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Click** | Select single node (clears previous selection) |
| **Ctrl+Click** | Toggle node in/out of selection |
| **Drag on canvas** | Draw selection rectangle |
| **Ctrl+A** | Select all nodes |
| **Delete** | Delete selected node(s) |
| **Escape** | Deselect all |
| **Ctrl+S** | Save workflow |

## Mouse Interactions

### Single Node Selection
```
Click Node ? Node selected (previous selection cleared)
Drag Node  ? Node moves
```

### Multi-Node Selection
```
Drag on canvas       ? Selection rectangle appears
Release mouse? Nodes in rectangle are selected
Ctrl+Click Node      ? Add/remove from selection
Drag selected node   ? All selected nodes move together
```

### Connection Creation
```
Drag from port ? Drag to another port ? Connection created
(Works even with multi-selection active)
```

## Implementation Details

### State Management

```javascript
let selectedNodes = new Set();  // Set of node IDs
let selectedNode = null;        // Current "primary" node for dragging
let isSelecting = false;      // Drawing selection rectangle?
let isDraggingSelection = false; // Dragging multiple nodes?
let selectionRect = null;       // DOM element for selection rectangle
```

### Selection Rectangle Algorithm

```javascript
function selectNodesInRect(rectLeft, rectTop, rectWidth, rectHeight) {
    const rectRight = rectLeft + rectWidth;
    const rectBottom = rectTop + rectHeight;
    
 workflow.nodes.forEach(node => {
      const nodeLeft = node.position.x;
const nodeTop = node.position.y;
        const nodeRight = nodeLeft + 180; // Node width
        const nodeBottom = nodeTop + 80;  // Node height
      
        // Check intersection
    const intersects = !(nodeRight < rectLeft || 
                 nodeLeft > rectRight || 
          nodeBottom < rectTop || 
   nodeTop > rectBottom);
        
        if (intersects) {
            selectedNodes.add(node.id);
        }
    });
}
```

### Group Dragging Algorithm

```javascript
function onMouseMove(e) {
    if (isDraggingSelection && selectedNodes.size > 1) {
    // Calculate delta from primary node's new position
const deltaX = newX - selectedNode.position.x;
        const deltaY = newY - selectedNode.position.y;
        
  // Apply delta to all selected nodes
  selectedNodes.forEach(nodeId => {
            const node = workflow.nodes.find(n => n.id === nodeId);
    if (node) {
          node.position.x += deltaX;
 node.position.y += deltaY;
   }
 });
    }
}
```

## Visual Examples

### Example 1: Selecting Multiple Nodes

**Step 1:** Click and drag on empty canvas
```
??????????????????????????????????
?        ?
?  Mouse down here     ?
?    ?  ?
?    ???????????????????   ?
?    ? ????????????? ?    ?
?    ? ? [LLM]  ?    ?      ?
? ? ?        ?     ?    ?
?    ? ? [Filter] ?  ?      ?
?    ? ????????????? ?  ? Drag to here     ?
?    ???????????????????         ?
??????????????????????????????????
```

**Step 2:** Release mouse - nodes are selected
```
??????????????????????????????????
?     ?
?    ???????????????????  ?
?    ? ? LLM       ?  ? Red border (selected)  ?
? ?      ?   ?
?    ? ? Filter    ?  ? Red border (selected)  ?
?  ???????????????????         ?
?       ?
?  [Transform]  ? Not selected   ?
??????????????????????????????????
```

### Example 2: Adding to Selection with Ctrl

**Initial state:**
```
[? LLM]  [Filter]  [Transform]
  ? selected
```

**Ctrl+Click Filter:**
```
[? LLM]  [? Filter]  [Transform]
  ? selected  ? selected (added)
```

**Ctrl+Click LLM:**
```
[LLM]  [? Filter]  [Transform]
     ? selected (LLM deselected)
```

### Example 3: Group Dragging

**Before:**
```
[? A] ???
    ???> [D]
[? B] ???
     ?
[? C]

(A, B, C selected; D not selected)
```

**Drag A to the right:**
```
         [? A] ???
      ???> [D]
      [? B] ???
        ?
         [? C]

(A, B, C moved together; D stayed in place)
(Connections automatically follow nodes)
```

## Edge Cases Handled

### 1. **Clicking Selected Node Without Ctrl**
- Preserves selection
- Starts dragging all selected nodes
- Does NOT deselect to just that node

### 2. **Clicking Unselected Node Without Ctrl**
- Clears previous selection
- Selects only the clicked node

### 3. **Drawing Selection Rectangle With Ctrl**
- Preserves existing selection
- Adds newly selected nodes

### 4. **Drawing Selection Rectangle Without Ctrl**
- Clears previous selection
- Selects only nodes in rectangle

### 5. **Port Interaction With Multi-Selection**
- Connections still work normally
- Port clicks don't affect selection

### 6. **Empty Selection Rectangle**
- If rectangle doesn't intersect any nodes, previous selection is cleared (without Ctrl)

## CSS Changes Required

The selection rectangle styling is dynamically created in JavaScript:

```javascript
selectionRect.style.position = 'absolute';
selectionRect.style.border = '2px dashed #3498db';
selectionRect.style.backgroundColor = 'rgba(52, 152, 219, 0.1)';
selectionRect.style.pointerEvents = 'none';
selectionRect.style.zIndex = '1000';
```

The selected node styling uses the existing `.selected` class:

```css
.workflow-node.selected {
    border-color: #e74c3c;
    box-shadow: 0 6px 20px rgba(231, 76, 60, 0.3);
}
```

## Performance Considerations

### Efficient Selection Check

Uses Set for O(1) lookup:
```javascript
const isSelected = selectedNodes.has(node.id);
```

### Batch Updates

All selected nodes update in a single render cycle:
```javascript
selectedNodes.forEach(nodeId => {
    // Update positions...
});
render(); // Single render after all updates
```

### Intersection Algorithm

Simple bounding box intersection - O(n) where n = number of nodes:
```javascript
const intersects = !(nodeRight < rectLeft || 
    nodeLeft > rectRight || 
         nodeBottom < rectTop || 
        nodeTop > rectBottom);
```

## Future Enhancements

### Planned Features
- [ ] **Alignment tools** - Align selected nodes (left, right, top, bottom, center)
- [ ] **Distribution tools** - Evenly space selected nodes
- [ ] **Group creation** - Save selection as a reusable group
- [ ] **Copy/paste** - Duplicate selected nodes (Ctrl+C, Ctrl+V)
- [ ] **Undo/redo** - Revert group operations
- [ ] **Lasso selection** - Free-form selection path
- [ ] **Select by type** - Select all nodes of a certain category
- [ ] **Inverse selection** - Select all except current selection

### Enhancement Ideas
- [ ] **Bounding box visualization** - Show rectangle around selection
- [ ] **Selection count badge** - Show count on canvas
- [ ] **Smart guides** - Snap to alignment when dragging group
- [ ] **Bulk parameter editing** - Edit common parameters for multiple nodes

## Troubleshooting

### Selection rectangle doesn't appear
- **Cause:** Clicking on a node instead of canvas
- **Fix:** Click on empty space between nodes

### Can't deselect nodes
- **Cause:** Holding Ctrl unintentionally
- **Fix:** Press Escape to clear all selections

### Group dragging not working
- **Cause:** Only one node selected
- **Fix:** Select multiple nodes first

### Nodes jumping when dragging
- **Cause:** Zoom level calculation issue
- **Fix:** Already handled in code with proper offset calculations

## Testing Checklist

- [x] Drag-to-select rectangle appears on canvas
- [x] Nodes inside rectangle get selected
- [x] Selection clears when clicking empty canvas (without Ctrl)
- [x] Ctrl+Click toggles individual node selection
- [x] Dragging one selected node moves all selected nodes
- [x] Relative positions maintained during group drag
- [x] Connections follow nodes during group drag
- [x] Delete key deletes all selected nodes
- [x] Ctrl+A selects all nodes
- [x] Escape clears selection
- [x] Properties panel shows multi-selection info
- [x] Can still create connections with multi-selection active
- [x] Selection rectangle disappears after release
- [x] Selected nodes have visual indicator (red border)

## Summary

**Multi-selection features:**
? **Drag-to-select** with visual rectangle  
? **Ctrl+Click** to toggle selection  
? **Group dragging** - move multiple nodes together  
? **Ctrl+A** to select all  
? **Multi-delete** with confirmation  
? **Visual feedback** with red borders and info panel  
? **Keyboard shortcuts** for efficiency  
? **Smart interaction** - preserves selection when appropriate  

The designer now supports professional multi-node operations just like advanced visual editors!

**Refresh your browser to try it out!** ??
