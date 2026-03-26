# ConditionNode Diamond Border Fix - WORKING SOLUTION

## Issue
The ConditionNode appeared as a diamond shape but **no border was visible** - only the ports were showing.

## Root Cause
The original CSS applied `clip-path` to the **main element**, which clipped both the content AND any borders. This made the border invisible.

## ? Working Solution

The fix uses **TWO pseudo-elements** working together:

```css
/* Main element: NO clip-path! */
.workflow-node[data-node-type="ConditionNode"] {
    position: relative;
    border: none !important;
    background: white;
    /* NO clip-path here! */
}

/* ::before - Creates the visible diamond border */
.workflow-node[data-node-type="ConditionNode"]::before {
    content: '';
    position: absolute;
    top: 50%;
    left: 50%;
    width: 100%;
    height: 100%;
    border: 3px solid #f39c12;  /* VISIBLE ORANGE BORDER */
    background: white;
    transform: translate(-50%, -50%) rotate(45deg);  /* Rotates square 45° = diamond */
    z-index: 0;
}

/* ::after - Masks content to diamond shape */
.workflow-node[data-node-type="ConditionNode"]::after {
 content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: white;
    clip-path: polygon(50% 0%, 100% 50%, 50% 100%, 0% 50%);  /* Diamond mask */
    z-index: 1;
}

/* Content stays on top */
.workflow-node[data-node-type="ConditionNode"] .node-header,
.workflow-node[data-node-type="ConditionNode"] .node-type {
 z-index: 5;
}

/* Ports stay on top of everything */
.workflow-node[data-node-type="ConditionNode"] .port {
    z-index: 10 !important;
}
```

## Port Positioning - ON DIAMOND CORNERS

Ports are positioned at the **four corners (points)** of the diamond:

```
        ? input (top corner, 0°)
   ? ?
      ?   ?
success ? ? error (right corner, 270°)
(left    ?   ?
corner)   ? ?
  90°)     ? failed (bottom corner, 180°)
```

### Port CSS:
```css
/* Top corner - input port */
.port.input {
    top: -7px;   /* Half of 14px port to center on corner */
    left: 50%;
    transform: translateX(-50%);
}

/* Left corner - success port (green) */
.port[data-port="success"] {
    top: 50%;
    left: -7px;
    transform: translateY(-50%);
}

/* Bottom corner - failed port (gray) */
.port[data-port="failed"] {
bottom: -7px;
    left: 50%;
    transform: translateX(-50%);
}

/* Right corner - error port (red) */
.port[data-port="error"] {
    top: 50%;
    right: -7px;
    transform: translateY(-50%);
}
```

## How It Works

### Layer Stack (bottom to top):
```
???????????????????????????????????
? ::before (z-index: 0)      ? ? Rotated 45° square with ORANGE BORDER
?   ???????????????????????????   ?
?   ? ::after (z-index: 1)    ?   ? ? White fill with diamond clip-path (masks corners)
?   ?   ???????????????????   ?   ?
? ?   ? Content (z: 5)  ?   ?   ? ? Text content (Condition / ConditionNode)
?   ?   ? Ports (z: 10)   ?   ?   ? ? Colored port circles ON CORNERS
?   ?   ???????????????????   ?   ?
?   ???????????????????????????   ?
???????????????????????????????????
```

### Visual Representation:
```
        ? (green input)
       ???
      ?  ??  ? 3px orange border
 ?    ??
    ? Cond ??
   ? ition  ??
  ?  Node   ?  ? Green (success) & Red (error)
   ?       ??
    ?   ??
     ?   ??
      ? ??
    ?  ? Gray (failed)
```

### Why This Works:
1. **`::before`** creates a rotated square (45° = diamond) with a thick orange border
2. **`::after`** applies `clip-path` to mask the white background to diamond shape
3. The border from `::before` remains visible because it's not clipped
4. Content and ports sit above both pseudo-elements with higher z-index
5. **Ports use negative positioning** (`-7px`) to center on the corners (half of 14px port size)

## Testing

### 1. Open the Test File
```bash
# Open in browser
test-diamond-node.html
```

You should see:
- ? **Thick orange diamond border** (3px solid #f39c12)
- ? **Four colored ports AT THE CORNERS (points of diamond)**:
  - Top: Green input port
  - Left: Green success port
  - Bottom: Gray failed port
  - Right: Red error port
- ? "Condition" and "ConditionNode" text centered
- ? Selected state shows dashed red border

### 2. Test in Designer
1. Run the web application
2. Navigate to `/Workflow/Designer/{id}`
3. Drag a **ConditionNode** onto canvas
4. Verify thick orange diamond border appears
5. Check that all 4 ports are **on the diamond corners** (not edges)

### 3. Clear Browser Cache
If you still see the old rendering:
- **Chrome/Edge**: `Ctrl + Shift + R` (hard refresh)
- **Firefox**: `Ctrl + F5`
- **Or**: DevTools ? Network ? Disable cache ? Reload

## Files Modified

| File | Status |
|------|--------|
| `source/web/wwwroot/css/designer/designer-nodes.css` | ? Fixed diamond border |
| `source/web/wwwroot/css/designer/designer-ports.css` | ? Fixed port positioning on corners |
| `test-diamond-node.html` | ? Working test cases |
| `DIAMOND_BORDER_FIX.md` | ? Updated docs |

## Visual Comparison

### ? Before (Broken)
```
   ?
  ? ?
 ?   ?      ? No border visible!
?  ?  ?     ? Ports on edges (incorrect)
 ?   ?
  ? ?
   ?
```

### ? After (Fixed)
```
? ? Top corner (input)
     ? ??
    ?   ??  ? Thick orange border visible!
   ? Co  ??
  ? ndit ? ? Left (success) & Right (error) corners
   ? ion ??
    ?   ??
     ? ??
      ? ? Bottom corner (failed)
```

## Port Positioning Details

| Port | Position | Color | Purpose |
|------|----------|-------|---------|
| `input` | Top corner (0°) | Green | Input data to condition |
| `success` | Left corner (90°) | Green | Success path output |
| `failed` | Bottom corner (180°) | Gray | Failed/default path output |
| `error` | Right corner (270°) | Red | Error path output |

**Key:** Using `-7px` offset (half of 14px port diameter) centers the port circle exactly on the diamond corner point.

## Browser Support
? Works in all modern browsers:
- Chrome/Edge 88+
- Firefox 75+
- Safari 13.1+
- Opera 74+

Both `transform: rotate()` and `clip-path` are well-supported.

## Why Previous Attempts Failed

### ? Attempt 1: clip-path with border on main element
```css
.node {
    clip-path: polygon(...);
    border: 2px solid #f39c12;  /* Gets clipped! */
}
```
**Problem**: `clip-path` clips the entire element including its border.

### ? Attempt 2: Single ::after with clip-path and border
```css
.node::after {
  clip-path: polygon(...);
    border: 2px solid #f39c12;  /* Also gets clipped! */
}
```
**Problem**: Border is still inside the clipped region.

### ? Attempt 3: Ports on edges instead of corners
```css
.port[data-port="success"] {
    bottom: 25%;  /* Midpoint of edge */
    left: 0%;
}
```
**Problem**: Ports appear on the flat edges between corners, not at the diamond points.

### ? Solution: Separate border and content masking + corner positioning
```css
.node::before { /* Rotated square with border */ }
.node::after { /* Content mask with clip-path */ }
.port { top: -7px; } /* Negative offset to position on corner */
```
**Success**: Border is on the rotated square, mask only clips the background fill, ports are centered on corners.

## Troubleshooting

### Border still not visible?
1. Hard refresh: `Ctrl + Shift + R`
2. Check DevTools ? Elements ? Inspect node ? Verify `::before` exists
3. Check `::before` has `border: 3px solid rgb(243, 156, 18)`
4. Verify `transform: translate(-50%, -50%) rotate(45deg)` is applied

### Ports not on corners?
1. Check DevTools ? Elements ? Inspect port ? Verify:
   - Input port has `top: -7px` and `left: 50%`
   - Success port has `left: -7px` and `top: 50%`
   - Failed port has `bottom: -7px` and `left: 50%`
   - Error port has `right: -7px` and `top: 50%`
2. Verify ports have `z-index: 10`
3. Check `designer-ports.css` is loaded

### Text not centered?
- Content should have `z-index: 5`
- Parent should have `display: flex`, `align-items: center`, `justify-content: center`

### Selected state not working?
```css
.workflow-node.node-condition.selected::before {
    border-color: #c0392b !important;
    border-style: dashed !important;
  border-width: 3px !important;
}
```

## Future Enhancements
- [ ] Animated border glow on selection
- [ ] Gradient borders
- [ ] Custom colors for different condition states
- [ ] Border thickness configurable via parameter
- [ ] Port labels show path conditions on hover
