import { Handle, Position, NodeToolbar, useReactFlow } from '@xyflow/react';
import { useIsRunner } from '../../context/SchemaContext';
import { portColor, NODE_ICONS, NODE_ROUTING_PORTS, DEFAULT_ROUTING_PORTS } from '../../nodeConfig';

/**
 * Standard rectangular node used for most node types.
 *
 * Layout:
 *   ┌─ colored left accent ──────────────────────────────┐
 *   │ [icon]  Node Name (label)           [nodeId badge] │
 *   │         description text (2-line clamp)            │
 *   └────────────────────────────────────────────────────┘
 */
export default function GenericNode({ id, data, selected }) {
  const isRunner = useIsRunner();
  const { setNodes, setEdges } = useReactFlow();

  const deleteNode = () => {
    setNodes((ns) => ns.filter((n) => n.id !== id));
    setEdges((es) => es.filter((e) => e.source !== id && e.target !== id));
  };

  const routing = NODE_ROUTING_PORTS[data.type] ?? DEFAULT_ROUTING_PORTS;
  const inputPorts  = routing.inputs;
  const outputPorts = routing.outputs;

  const color = data.color ?? '#3498db';
  const icon  = data.icon  ?? NODE_ICONS[data.type] ?? 'bi-box';
  const desc  = data.parameters?.description ?? '';

  const portTop = (index, total) => {
    if (total <= 1) return '50%';
    const step = 100 / (total + 1);
    return `${step * (index + 1)}%`;
  };

  const runnerClass = data.runnerState ? `rf-runner-${data.runnerState}` : '';

  return (
    <>
      {selected && !isRunner && data.type !== 'StartNode' && (
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
              boxShadow: '0 2px 6px rgba(239,68,68,0.35)',
            }}
          >
            <i className="bi bi-x" />
          </button>
        </NodeToolbar>
      )}
      <div
        className={runnerClass}
        title={data.type}
        style={{
          borderRadius: 10,
          backgroundColor: selected ? '#f8faff' : '#ffffff',
          border: `1.5px solid ${selected ? color : '#e2e8f0'}`,
          borderLeft: `4px solid ${color}`,
          minWidth: 184,
          minHeight: 62,
          boxShadow: selected
            ? `0 0 0 3px ${color}22, 0 4px 16px rgba(15,23,42,0.12)`
            : '0 1px 3px rgba(15,23,42,0.08), 0 4px 12px rgba(15,23,42,0.05)',
          position: 'relative',
          fontFamily: 'inherit',
          cursor: 'default',
          userSelect: 'none',
          transition: 'border-color 0.15s, box-shadow 0.15s, background 0.15s',
        }}
      >
        {inputPorts.map((port, i) => (
          <Handle
            key={port.id}
            type="target"
            position={Position.Left}
            id={port.id}
            style={{ top: portTop(i, inputPorts.length), background: portColor(port.id, 'target') }}
            title={port.label}
          />
        ))}

        {/* Header row: icon + name + nodeId */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: 7,
          padding: '9px 11px 4px 11px',
        }}>
          {/* Icon badge */}
          <div style={{
            width: 28, height: 28, borderRadius: 7,
            background: `linear-gradient(135deg, ${color}22 0%, ${color}12 100%)`,
            border: `1px solid ${color}28`,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            flexShrink: 0,
          }}>
            <i className={`bi ${icon}`} style={{ color, fontSize: 13 }} />
          </div>

          {/* Node name */}
          <div style={{
            fontWeight: 600,
            fontSize: 12.5,
            color: '#0f172a',
            lineHeight: 1.2,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            flex: 1,
            letterSpacing: '-0.01em',
          }}>
            {data.label}
          </div>

          {/* NodeId badge */}
          {data.nodeId && (
            <div
              title={`Reference this node's outputs as {{${data.nodeId}.key}}`}
              style={{
                fontSize: 9.5,
                fontFamily: 'monospace',
                color: '#6366f1',
                background: '#eef2ff',
                border: '1px solid #c7d2fe',
                borderRadius: 4,
                padding: '2px 5px',
                flexShrink: 0,
                userSelect: 'text',
                letterSpacing: '0.02em',
              }}
            >
              {data.nodeId}
            </div>
          )}
        </div>

        {/* Description — strictly 2 lines */}
        <div style={{
          padding: '0 11px 9px 46px',
          fontSize: 11,
          color: desc ? '#64748b' : '#94a3b8',
          fontStyle: desc ? 'normal' : 'italic',
          lineHeight: 1.45,
          height: '2.9em',
          overflow: 'hidden',
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
        }}>
          {desc || 'No description'}
        </div>

        {outputPorts.map((port, i) => (
          <Handle
            key={port.id}
            type="source"
            position={Position.Right}
            id={port.id}
            style={{ top: portTop(i, outputPorts.length), background: portColor(port.id, 'source') }}
            title={port.label}
          />
        ))}
      </div>
    </>
  );
}
