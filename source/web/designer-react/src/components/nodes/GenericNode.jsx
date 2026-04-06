import { useContext } from 'react';
import { Handle, Position, NodeToolbar, useReactFlow } from '@xyflow/react';
import { SchemaContext } from '../../context/SchemaContext';
import { portColor, NODE_ICONS } from '../../nodeConfig';

/**
 * Standard rectangular node used for most node types.
 *
 * Layout:
 *   ┌─ colored left border ──────────────────────────┐
 *   │ [icon]  Node Name (label)          [type badge] │
 *   │         description text (2-line clamp)         │
 *   └─────────────────────────────────────────────────┘
 */
export default function GenericNode({ id, data, selected }) {
  const schemas = useContext(SchemaContext);
  const { setNodes, setEdges } = useReactFlow();

  const deleteNode = () => {
    setNodes((ns) => ns.filter((n) => n.id !== id));
    setEdges((es) => es.filter((e) => e.source !== id && e.target !== id));
  };
  const schema = schemas[data.type] ?? {};

  const inputPorts  = schema.inputPorts  ?? [{ id: 'input',  label: 'Input',  type: 'Data' }];
  const outputPorts = schema.outputPorts ?? [{ id: 'output', label: 'Output', type: 'Data' }];

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
      {selected && data.type !== 'StartNode' && (
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
      title={data.type}
      style={{
        borderRadius: 8,
        backgroundColor: selected ? '#f0f4ff' : '#fff',
        border: `1.5px solid ${selected ? color : '#e2e8f0'}`,
        borderLeft: `4px solid ${color}`,
        minWidth: 180,
        minHeight: 60,
        boxShadow: selected
          ? `0 0 0 2px ${color}33, 0 4px 12px rgba(0,0,0,0.12)`
          : '0 1px 4px rgba(0,0,0,0.08)',
        position: 'relative',
        fontFamily: 'inherit',
        cursor: 'default',
        userSelect: 'none',
        transition: 'border-color 0.15s, box-shadow 0.15s',
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

      {/* Header row: icon + name */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 7,
          padding: '9px 12px 4px 12px',
        }}
      >
        {/* Icon badge */}
        <div
          style={{
            width: 26,
            height: 26,
            borderRadius: 6,
            backgroundColor: `${color}18`,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0,
          }}
        >
          <i className={`bi ${icon}`} style={{ color, fontSize: 13 }} />
        </div>

        {/* Node name */}
        <div
          style={{
            fontWeight: 600,
            fontSize: 13,
            color: '#1e293b',
            lineHeight: 1.2,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            flex: 1,
          }}
        >
          {data.label}
        </div>
      </div>

      {/* Description — strictly 2 lines, never expands node */}
      <div
        style={{
          padding: '0 12px 9px 45px',
          fontSize: 11,
          color: desc ? '#64748b' : '#94a3b8',
          fontStyle: desc ? 'normal' : 'italic',
          lineHeight: 1.4,
          height: '2.8em',   /* exactly 2 lines — prevents node from growing */
          overflow: 'hidden',
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
        }}
      >
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
