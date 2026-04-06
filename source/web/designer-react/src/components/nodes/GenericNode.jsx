import { useContext } from 'react';
import { Handle, Position } from '@xyflow/react';
import { SchemaContext } from '../../context/SchemaContext';
import { portColor } from '../../nodeConfig';

/**
 * Standard rectangular node used for most node types.
 * Reads schema from context to render the correct number of port handles.
 * Port labels appear as CSS tooltips on hover (no overlap with node text).
 */
export default function GenericNode({ data, selected }) {
  const schemas = useContext(SchemaContext);
  const schema = schemas[data.type] ?? {};

  const inputPorts = schema.inputPorts ?? [
    { id: 'input', label: 'Input', type: 'Data' },
  ];
  const outputPorts = schema.outputPorts ?? [
    { id: 'output', label: 'Output', type: 'Data' },
  ];

  const color = data.color ?? '#3498db';

  const portTop = (index, total) => {
    if (total <= 1) return '50%';
    const step = 100 / (total + 1);
    return `${step * (index + 1)}%`;
  };

  const runnerClass = data.runnerState ? `rf-runner-${data.runnerState}` : '';

  return (
    <div
      className={runnerClass}
      style={{
        border: `2px solid ${color}`,
        borderRadius: 6,
        backgroundColor: selected ? '#f0f4ff' : '#fff',
        minWidth: 160,
        minHeight: 64,
        padding: '10px 18px',
        boxShadow: selected
          ? `0 0 0 2px ${color}, 0 2px 8px rgba(0,0,0,0.18)`
          : '0 1px 4px rgba(0,0,0,0.12)',
        position: 'relative',
        fontFamily: 'inherit',
        cursor: 'default',
        userSelect: 'none',
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

      <div style={{ fontWeight: 600, fontSize: 13, color: '#212529', marginBottom: 2 }}>
        {data.label}
      </div>
      <div style={{ fontSize: 11, color, fontWeight: 500, opacity: 0.85 }}>
        {data.type}
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
  );
}
