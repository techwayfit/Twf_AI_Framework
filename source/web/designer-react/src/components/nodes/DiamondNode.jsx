import { Handle, Position } from '@xyflow/react';

// INNER: side of the rotated square.
// OUTER: full diamond span = INNER * √2.  The outer container is OUTER×OUTER so
// that each edge midpoint of the container coincides exactly with a diamond tip.
// PAD: offset so the inner square sits centred inside the outer container.
const INNER = 110;
const OUTER = Math.round(INNER * Math.SQRT2); // ≈ 156
const PAD   = (OUTER - INNER) / 2;            // ≈ 23

/**
 * Diamond-shaped node for ConditionNode.
 * Handles are placed at the real corner tips of the diamond:
 *   input   → left tip
 *   success → right tip
 *   failure → bottom tip
 */
export default function DiamondNode({ data, selected }) {
  const color = data.color ?? '#F5A623';

  const runnerClass = data.runnerState ? `rf-runner-${data.runnerState}` : '';

  return (
    <div className={runnerClass} style={{ position: 'relative', width: OUTER, height: OUTER }}>
      {/* Rotated square centred inside the OUTER container */}
      <div
        style={{
          position: 'absolute',
          top: PAD,
          left: PAD,
          width: INNER,
          height: INNER,
          backgroundColor: selected ? '#fff8ee' : '#fdf6ec',
          border: `3px solid ${color}`,
          transform: 'rotate(45deg)',
          boxShadow: selected
            ? `0 0 0 2px ${color}, 0 2px 8px rgba(0,0,0,0.18)`
            : '0 1px 4px rgba(0,0,0,0.12)',
        }}
      />

      {/* Label — centred over the full OUTER area, not rotated */}
      <div
        style={{
          position: 'absolute',
          inset: 0,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1,
          pointerEvents: 'none',
          userSelect: 'none',
        }}
      >
        <div style={{ fontWeight: 700, fontSize: 11, color: '#212529', textAlign: 'center', lineHeight: 1.3 }}>
          {data.label}
        </div>
        <div style={{ fontSize: 10, color, fontWeight: 500, opacity: 0.85 }}>
          Condition
        </div>
      </div>

      {/* Input — LEFT tip */}
      <Handle type="target" position={Position.Left} id="input" style={{ top: '50%', zIndex: 2, background: '#3b82f6' }} title="Input" />

      {/* Success — RIGHT tip */}
      <Handle type="source" position={Position.Right} id="success" style={{ top: '50%', zIndex: 2, background: '#22c55e' }} title="Success" />

      {/* Failure — BOTTOM tip */}
      <Handle type="source" position={Position.Bottom} id="failure" style={{ left: '50%', zIndex: 2, background: '#ef4444' }} title="Failure" />
    </div>
  );
}
