import { Handle, Position, NodeToolbar, useReactFlow } from '@xyflow/react';
import { NODE_ICONS } from '../../nodeConfig';

const INNER = 110;
const OUTER = Math.round(INNER * Math.SQRT2);
const PAD   = (OUTER - INNER) / 2;

export default function DiamondNode({ id, data, selected }) {
  const { setNodes, setEdges } = useReactFlow();
  const color = data.color ?? '#F5A623';
  const icon  = data.icon  ?? NODE_ICONS[data.type] ?? 'bi-question-diamond';

  const runnerClass = data.runnerState ? `rf-runner-${data.runnerState}` : '';

  const deleteNode = () => {
    setNodes((ns) => ns.filter((n) => n.id !== id));
    setEdges((es) => es.filter((e) => e.source !== id && e.target !== id));
  };

  return (
    <>
      {selected && (
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
      <div className={runnerClass} style={{ position: 'relative', width: OUTER, height: OUTER }}>
        <div style={{
          position: 'absolute', top: PAD, left: PAD, width: INNER, height: INNER,
          backgroundColor: selected ? '#fff8ee' : '#fdf6ec',
          border: `3px solid ${color}`,
          transform: 'rotate(45deg)',
          boxShadow: selected
            ? `0 0 0 2px ${color}, 0 2px 8px rgba(0,0,0,0.18)`
            : '0 1px 4px rgba(0,0,0,0.12)',
        }} />
        <div style={{
          position: 'absolute', inset: 0,
          display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
          zIndex: 1, pointerEvents: 'none', userSelect: 'none', gap: 3,
        }}>
          <i className={`bi ${icon}`} style={{ color, fontSize: 18 }} />
          <div style={{ fontWeight: 700, fontSize: 11, color: '#1e293b', textAlign: 'center', lineHeight: 1.3 }}>
            {data.label}
          </div>
        </div>
        <Handle type="target"  position={Position.Left}   id="input"   style={{ top: '50%',  zIndex: 2, background: '#3b82f6' }} title="Input"   />
        <Handle type="source"  position={Position.Right}  id="success" style={{ top: '50%',  zIndex: 2, background: '#22c55e' }} title="Success" />
        <Handle type="source"  position={Position.Bottom} id="failure" style={{ left: '50%', zIndex: 2, background: '#ef4444' }} title="Failure" />
      </div>
    </>
  );
}
