import { Handle, Position, NodeToolbar, useReactFlow } from '@xyflow/react';
import { NODE_ICONS } from '../../nodeConfig';

const CIRCLE_COLORS = {
  StartNode:      '#2ecc71',
  EndNode:        '#e74c3c',
  ErrorNode:      '#e74c3c',
  ErrorRouteNode: '#c0392b',
};

export default function CircularNode({ id, data, selected }) {
  const { setNodes, setEdges } = useReactFlow();
  const color   = CIRCLE_COLORS[data.type] ?? '#95a5a6';
  const icon    = data.icon ?? NODE_ICONS[data.type] ?? 'bi-circle';
  const isStart = data.type === 'StartNode';
  const isEnd   = data.type === 'EndNode';
  const isError = data.type === 'ErrorNode';
  const hasInput = !isStart && !isError;

  const runnerClass = data.runnerState ? `rf-runner-${data.runnerState}` : '';

  const deleteNode = () => {
    setNodes((ns) => ns.filter((n) => n.id !== id));
    setEdges((es) => es.filter((e) => e.source !== id && e.target !== id));
  };

  return (
    <>
      {selected && !isStart && (
        <NodeToolbar position={Position.Top} align="end" offset={4}>
          <button
            onClick={deleteNode}
            title="Delete node"
            className="nodrag"
            style={{
              width: 22, height: 22, borderRadius: '50%',
              background: '#ef4444', border: '2px solid #fff',
              color: '#fff', fontSize: 13, lineHeight: 1,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              cursor: 'pointer', padding: 0,
              boxShadow: '0 1px 4px rgba(0,0,0,0.25)',
            }}
          >
            <i className="bi bi-x" />
          </button>
        </NodeToolbar>
      )}
      <div
        className={runnerClass}
        style={{
          width: 80, height: 80, borderRadius: '50%',
          backgroundColor: color,
          border: selected ? '3px solid #1e293b' : `3px solid ${color}`,
          display: 'flex', flexDirection: 'column',
          alignItems: 'center', justifyContent: 'center', gap: 2,
          color: '#fff',
          boxShadow: selected
            ? '0 0 0 3px #1e293b, 0 2px 8px rgba(0,0,0,0.25)'
            : '0 2px 6px rgba(0,0,0,0.2)',
          cursor: 'default', userSelect: 'none', position: 'relative',
        }}
      >
        {hasInput && (
          <Handle type="target" position={Position.Left} id="input"
            style={{ background: '#3b82f6', border: '2px solid #fff' }} title="Input" />
        )}
        <i className={`bi ${icon}`} style={{ fontSize: 18, lineHeight: 1 }} />
        <span style={{ fontSize: 11, fontWeight: 700, lineHeight: 1, textAlign: 'center', padding: '0 4px' }}>
          {data.label}
        </span>
        {!isEnd && (
          <Handle type="source" position={Position.Right} id="output"
            style={{ background: '#6c757d', border: '2px solid #fff' }} title="Output" />
        )}
      </div>
    </>
  );
}
