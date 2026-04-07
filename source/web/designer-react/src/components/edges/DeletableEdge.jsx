import { BaseEdge, EdgeLabelRenderer, getSmoothStepPath, useReactFlow } from '@xyflow/react';

export default function DeletableEdge({
  id, selected,
  sourceX, sourceY, targetX, targetY,
  sourcePosition, targetPosition,
  style, markerEnd,
  label, labelStyle, labelBgStyle, labelBgPadding, labelBgBorderRadius,
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

  // Offset the delete button so it doesn't overlap the label text
  const deleteOffsetY = label ? -14 : 0;

  return (
    <>
      <BaseEdge path={edgePath} style={style} markerEnd={markerEnd} />

      <EdgeLabelRenderer>
        {/* Edge label */}
        {label && (
          <div
            style={{
              position: 'absolute',
              transform: `translate(-50%, -50%) translate(${labelX}px, ${labelY}px)`,
              pointerEvents: 'none',
              padding: '2px 6px',
              borderRadius: 3,
              fontSize: 11,
              fontWeight: 600,
              color: '#495057',
              background: 'rgba(255,255,255,0.92)',
              border: '1px solid #dee2e6',
              whiteSpace: 'nowrap',
              ...(labelStyle ?? {}),
            }}
          >
            {label}
          </div>
        )}

        {/* Delete button — shown when selected */}
        {selected && (
          <button
            onClick={deleteEdge}
            title="Delete connection"
            style={{
              position: 'absolute',
              transform: `translate(-50%, -50%) translate(${labelX}px, calc(${labelY}px + ${deleteOffsetY}px))`,
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
        )}
      </EdgeLabelRenderer>
    </>
  );
}
