import { Handle, Position } from '@xyflow/react';

const SIZE = 110;

/**
 * Diamond-shaped node for ConditionNode.
 * Two output handles: "success" (right) and "failure" (bottom).
 */
export default function DiamondNode({ data, selected }) {
  const color = data.color ?? '#F5A623';

  return (
    <div style={{ position: 'relative', width: SIZE, height: SIZE }}>
      {/* Rotated square for the diamond shape */}
      <div
        style={{
          position: 'absolute',
          top: 0,
          left: 0,
          width: SIZE,
          height: SIZE,
          backgroundColor: selected ? '#fff8ee' : '#fff',
          border: `2px solid ${color}`,
          transform: 'rotate(45deg)',
          boxShadow: selected
            ? `0 0 0 2px ${color}, 0 2px 8px rgba(0,0,0,0.18)`
            : '0 1px 4px rgba(0,0,0,0.12)',
        }}
      />

      {/* Label sits on top of the rotated square, NOT rotated */}
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

      {/* Input – left edge of the diamond */}
      <Handle
        type="target"
        position={Position.Left}
        id="input"
        style={{ top: '50%', zIndex: 2 }}
        title="Input"
      />

      {/* Success output – right edge */}
      <Handle
        type="source"
        position={Position.Right}
        id="success"
        style={{ top: '50%', zIndex: 2 }}
        title="Success"
      />

      {/* Failure output – bottom edge */}
      <Handle
        type="source"
        position={Position.Bottom}
        id="failure"
        style={{ left: '50%', zIndex: 2 }}
        title="Failure"
      />
    </div>
  );
}
