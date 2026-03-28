import { Handle, Position } from '@xyflow/react';

const CIRCLE_COLORS = {
  StartNode: '#2ecc71',
  EndNode: '#e74c3c',
  ErrorNode: '#e74c3c',
  ErrorRouteNode: '#c0392b',
};

/**
 * Circular node used for Start, End, and Error nodes.
 * StartNode  — no input handle (flow starts here).
 * EndNode    — no output handle (flow terminates here).
 * ErrorNode  — no input handle (receives connections via the error route,
 *              not a regular in-port; errors are routed to it implicitly).
 */
export default function CircularNode({ data, selected }) {
  const color = CIRCLE_COLORS[data.type] ?? '#95a5a6';
  const isStart = data.type === 'StartNode';
  const isEnd = data.type === 'EndNode';
  const isError = data.type === 'ErrorNode';
  // No input handle for Start or Error nodes
  const hasInput = !isStart && !isError;

  return (
    <div
      style={{
        width: 80,
        height: 80,
        borderRadius: '50%',
        backgroundColor: color,
        border: selected ? '3px solid #212529' : `3px solid ${color}`,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: '#fff',
        fontWeight: 700,
        fontSize: 13,
        textAlign: 'center',
        boxShadow: selected
          ? '0 0 0 3px #212529, 0 2px 8px rgba(0,0,0,0.25)'
          : '0 2px 6px rgba(0,0,0,0.2)',
        cursor: 'default',
        userSelect: 'none',
        position: 'relative',
      }}
    >
      {hasInput && (
        <Handle
          type="target"
          position={Position.Left}
          id="input"
          style={{ background: '#3b82f6', border: '2px solid #fff' }}
          title="Input"
        />
      )}

      <span style={{ lineHeight: 1.2, padding: 4 }}>{data.label}</span>

      {!isEnd && (
        <Handle
          type="source"
          position={Position.Right}
          id="output"
          style={{ background: '#6c757d', border: '2px solid #fff' }}
          title="Output"
        />
      )}
    </div>
  );
}
