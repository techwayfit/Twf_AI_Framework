import { NodeResizer, useReactFlow } from '@xyflow/react';

/**
 * ContainerNode — a purely visual grouping node with:
 *   • No input / output handles (ports)
 *   • Resizable via NodeResizer
 *   • **Customizable background color via color picker**
 *   • Semi-transparent configurable background colour
 *   • Rendered behind all other nodes (inserted at index 0)
 */
export default function ContainerNode({ id, data, selected }) {
  const { updateNodeData } = useReactFlow();

  // Support for custom background color with fallback to default indigo
  const bgColor  = data.parameters?.backgroundColor ?? '#6366f1';
  const opacity  = Math.max(0, Math.min(1, Number(data.parameters?.opacity ?? 0.12)));

  // Convert hex to rgba for the fill
  const hex = bgColor.replace('#', '');
  const r   = parseInt(hex.slice(0, 2), 16) || 0;
  const g   = parseInt(hex.slice(2, 4), 16) || 0;
  const b   = parseInt(hex.slice(4, 6), 16) || 0;
  const fill = `rgba(${r},${g},${b},${opacity})`;

  const handleResizeEnd = (_, params) => {
    updateNodeData(id, {
      parameters: { ...data.parameters, width: params.width, height: params.height },
    });
  };

  return (
    <>
      <NodeResizer
        minWidth={120}
        minHeight={80}
        isVisible={selected}
        onResizeEnd={handleResizeEnd}
        lineStyle={{ border: `1.5px dashed ${bgColor}` }}
        handleStyle={{ width: 8, height: 8, borderRadius: 2, borderColor: bgColor, background: '#fff', border: `1.5px solid ${bgColor}` }}
      />
      <div
        style={{
          width: '100%',
          height: '100%',
          background: fill,
          border: selected ? `2px solid ${bgColor}` : `2px dashed ${bgColor}`,
          borderRadius: 8,
          boxSizing: 'border-box',
          padding: '6px 12px',
          userSelect: 'none',
        }}
      >
        <div
          style={{
            fontSize: 11,
            fontWeight: 700,
            color: bgColor,
            textTransform: 'uppercase',
            letterSpacing: '0.6px',
          }}
        >
          {data.label ?? 'Group'}
        </div>
      </div>
    </>
  );
}
