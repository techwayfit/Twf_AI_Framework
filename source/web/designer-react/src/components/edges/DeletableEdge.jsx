import { BaseEdge, EdgeLabelRenderer, getSmoothStepPath, useReactFlow } from '@xyflow/react';

/**
 * Smoothstep edge that shows a delete button at its midpoint when selected.
 * Works for both regular flow edges and note-link edges (straight style override
 * is handled via the `style` prop passed through from onConnect).
 */
export default function DeletableEdge({
  id, selected,
  sourceX, sourceY, targetX, targetY,
  sourcePosition, targetPosition,
  style, markerEnd, label, labelStyle, labelBgStyle, labelBgPadding, labelBgBorderRadius,
}) {
  const { setEdges } = useReactFlow();

  const [edgePath, labelX, labelY] = getSmoothStepPath({
    sourceX, sourceY, sourcePosition,
    targetX, targetY, targetPosition,
  });

  const deleteEdge = (e) => {
    e.stopPropagation();
    setEdges((es) => es.filter((e) => e.id !== id));
  };

  return (
    <>
      <BaseEdge
        path={edgePath}
        style={style}
        markerEnd={markerEnd}
        label={label}
        labelStyle={labelStyle}
        labelBgStyle={labelBgStyle}
        labelBgPadding={labelBgPadding}
        labelBgBorderRadius={labelBgBorderRadius}
      />

      {selected && (
        <EdgeLabelRenderer>
          <button
            onClick={deleteEdge}
            title="Delete connection"
            style={{
              position: 'absolute',
              transform: `translate(-50%, -50%) translate(${labelX}px, ${labelY}px)`,
              pointerEvents: 'all',
              width: 20,
              height: 20,
              borderRadius: '50%',
              background: '#ef4444',
              border: '2px solid #fff',
              color: '#fff',
              fontSize: 11,
              lineHeight: 1,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              boxShadow: '0 1px 4px rgba(0,0,0,0.25)',
              zIndex: 10,
              padding: 0,
            }}
            className="nodrag nopan"
          >
            <i className="bi bi-x" style={{ fontSize: 12, lineHeight: 1 }} />
          </button>
        </EdgeLabelRenderer>
      )}
    </>
  );
}
