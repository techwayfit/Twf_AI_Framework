import { Handle, Position } from '@xyflow/react';
import { portColor } from '../../nodeConfig';

const MAX_BRANCHES = 7;

/**
 * ParallelNode — custom renderer so branch ports react live to branchCount changes.
 *   input     → LEFT edge
 *   branch1…N → BOTTOM edge, evenly spaced (max 7)
 *   afterAll  → RIGHT edge (fires after all branches complete)
 */
export default function ParallelNode({ data, selected }) {
  const color = data.color ?? '#8e44ad';

  const branchCount = Math.min(
    Math.max(parseInt(data.parameters?.branchCount ?? 3, 10), 1),
    MAX_BRANCHES,
  );

  const portLeft = (index, total) => {
    if (total <= 1) return '50%';
    const step = 100 / (total + 1);
    return `${step * (index + 1)}%`;
  };

  const branchPorts = Array.from({ length: branchCount }, (_, i) => ({
    id: `branch${i + 1}`,
    label: `Branch ${i + 1}`,
  }));

  return (
    <div
      style={{
        border: `2px solid ${color}`,
        borderRadius: 6,
        backgroundColor: selected ? '#f8f0ff' : '#fff',
        minWidth: 160,
        padding: '10px 18px 22px',
        boxShadow: selected
          ? `0 0 0 2px ${color}, 0 2px 8px rgba(0,0,0,0.18)`
          : '0 1px 4px rgba(0,0,0,0.12)',
        position: 'relative',
        fontFamily: 'inherit',
        cursor: 'default',
        userSelect: 'none',
      }}
    >
      {/* Input — LEFT */}
      <Handle
        type="target"
        position={Position.Left}
        id="input"
        style={{ top: '40%', background: portColor('input', 'target') }}
        title="Input"
      />

      <div style={{ fontWeight: 600, fontSize: 13, color: '#212529', marginBottom: 2 }}>
        {data.label}
      </div>
      <div style={{ fontSize: 11, color, fontWeight: 500, opacity: 0.85 }}>
        ParallelNode
      </div>
      <div style={{ fontSize: 10, color: '#6c757d', marginTop: 3 }}>
        {branchCount} branch{branchCount !== 1 ? 'es' : ''}
      </div>

      {/* Branch ports — BOTTOM */}
      {branchPorts.map((port, i) => (
        <Handle
          key={port.id}
          type="source"
          position={Position.Bottom}
          id={port.id}
          style={{ left: portLeft(i, branchCount), background: portColor(port.id, 'source') }}
          title={port.label}
        />
      ))}

      {/* After All — RIGHT */}
      <Handle
        type="source"
        position={Position.Right}
        id="afterAll"
        style={{ top: '40%', background: '#22c55e' }}
        title="After All"
      />
    </div>
  );
}
