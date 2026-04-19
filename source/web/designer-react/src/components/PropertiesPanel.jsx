import { useState, useEffect, useRef } from 'react';
import { useSchemas } from '../context/SchemaContext';
import { NODE_ICONS } from '../nodeConfig';

/** Section heading used inside the panel */
function SectionHead({ icon, label, color }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 6,
      padding: '8px 14px 6px',
      borderTop: '1px solid #f1f5f9',
      marginTop: 4,
    }}>
      <i className={`bi ${icon}`} style={{ color: color ?? '#6366f1', fontSize: 12 }} />
      <span style={{ fontSize: 11, fontWeight: 700, color: color ?? '#475569', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
        {label}
      </span>
    </div>
  );
}

/**
 * Right-side properties panel.
 * Uses local form state for smooth editing; commits to the ReactFlow node
 * on every field change for instant canvas updates.
 */
export default function PropertiesPanel({ selectedNode, onChange, onDelete }) {
  const schemas = useSchemas();

  const [label, setLabel] = useState('');
  const [params, setParams] = useState({});
  const nodeIdRef = useRef(null);

  useEffect(() => {
    if (selectedNode && selectedNode.id !== nodeIdRef.current) {
      nodeIdRef.current = selectedNode.id;
      setLabel(selectedNode.data.label ?? '');
      setParams({ ...(selectedNode.data.parameters ?? {}) });
    }
  }, [selectedNode?.id]);

  if (!selectedNode) {
    return (
      <div style={{
        display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
        height: '100%', padding: '24px 16px', gap: 10,
      }}>
        <div style={{
          width: 44, height: 44, borderRadius: 12,
          background: '#f1f5f9',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }}>
          <i className="bi bi-sliders" style={{ fontSize: 20, color: '#94a3b8' }} />
        </div>
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: 13, fontWeight: 600, color: '#475569', marginBottom: 4 }}>Properties</div>
          <div style={{ fontSize: 12, color: '#94a3b8', lineHeight: 1.5 }}>Select a node or connector to edit its properties.</div>
        </div>
      </div>
    );
  }

  const schema = schemas[selectedNode.type] ?? {};
  const nodeColor = selectedNode.data.color ?? '#3498db';

  const commitLabel = (val) => {
    setLabel(val);
    onChange(selectedNode.id, { label: val });
  };

  const commitParam = (name, value) => {
    const next = { ...params, [name]: value };
    setParams(next);
    onChange(selectedNode.id, { parameters: next });
  };

  // ── field renderers ──────────────────────────────────────────────────────

  const inputStyle = {
    width: '100%', padding: '6px 8px', fontSize: 12,
    border: '1px solid #e2e8f0', borderRadius: 6,
    background: '#fff', color: '#1e293b', fontFamily: 'inherit',
    outline: 'none', boxSizing: 'border-box',
    transition: 'border-color 0.12s',
  };

  const labelStyle = {
    display: 'block', fontSize: 11, fontWeight: 600,
    color: '#475569', marginBottom: 4,
    letterSpacing: '0.02em',
  };

  const renderField = (param) => {
    const value = params[param.name] ?? param.defaultValue ?? '';
    const fid = `prop-${param.name}`;

    const lbl = (
      <label htmlFor={fid} style={labelStyle}>
        {param.label}
        {param.required && <span style={{ color: '#ef4444', marginLeft: 2 }}>*</span>}
      </label>
    );

    switch (param.type) {
      case 'TextArea':
      case 'Json':
        return (
          <div key={param.name} style={{ marginBottom: 10 }}>
            {lbl}
            <textarea
              id={fid}
              rows={param.type === 'Json' ? 4 : 3}
              value={String(value)}
              placeholder={param.placeholder ?? param.description ?? ''}
              onChange={(e) => commitParam(param.name, e.target.value)}
              style={{ ...inputStyle, fontFamily: param.type === 'Json' ? 'monospace' : 'inherit', resize: 'vertical' }}
            />
          </div>
        );

      case 'Number':
        return (
          <div key={param.name} style={{ marginBottom: 10 }}>
            {lbl}
            <input id={fid} type="number" value={value}
              min={param.minValue} max={param.maxValue}
              step={param.name === 'temperature' ? 0.1 : 1}
              onChange={(e) => commitParam(param.name, parseFloat(e.target.value))}
              style={inputStyle}
            />
          </div>
        );

      case 'Boolean':
        return (
          <div key={param.name} style={{ marginBottom: 10, display: 'flex', alignItems: 'center', gap: 8 }}>
            <input id={fid} type="checkbox" checked={Boolean(value)}
              onChange={(e) => commitParam(param.name, e.target.checked)}
              style={{ width: 14, height: 14, cursor: 'pointer', accentColor: '#6366f1' }}
            />
            <label htmlFor={fid} style={{ ...labelStyle, marginBottom: 0, cursor: 'pointer' }}>
              {param.label}
            </label>
          </div>
        );

      case 'Select':
        return (
          <div key={param.name} style={{ marginBottom: 10 }}>
            {lbl}
            <select id={fid} value={String(value)}
              onChange={(e) => commitParam(param.name, e.target.value)}
              style={{ ...inputStyle, cursor: 'pointer' }}
            >
              {(param.options ?? []).map((opt) => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
              ))}
            </select>
          </div>
        );

      case 'Color':
        return (
          <div key={param.name} style={{ marginBottom: 10 }}>
            {lbl}
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input id={fid} type="color" value={String(value)}
                onChange={(e) => commitParam(param.name, e.target.value)}
                style={{ width: 36, height: 28, padding: 2, border: '1px solid #e2e8f0', borderRadius: 5, cursor: 'pointer' }}
              />
              <span style={{ fontSize: 11, color: '#94a3b8', fontFamily: 'monospace' }}>{String(value)}</span>
            </div>
          </div>
        );

      default: // Text
        return (
          <div key={param.name} style={{ marginBottom: 10 }}>
            {lbl}
            <input id={fid} type="text" value={String(value)}
              placeholder={param.placeholder ?? param.description ?? ''}
              onChange={(e) => commitParam(param.name, e.target.value)}
              style={inputStyle}
            />
          </div>
        );
    }
  };

  // ── render ───────────────────────────────────────────────────────────────

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      {/* ── Header ── */}
      <div style={{
        padding: '12px 14px 10px',
        borderBottom: '1px solid #f1f5f9',
        background: `linear-gradient(135deg, ${nodeColor}0a 0%, transparent 100%)`,
        flexShrink: 0,
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div style={{
            width: 36, height: 36, borderRadius: 9,
            background: `linear-gradient(135deg, ${nodeColor}22, ${nodeColor}12)`,
            border: `1.5px solid ${nodeColor}30`,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            flexShrink: 0,
          }}>
            <i
              className={`bi ${selectedNode.data.icon ?? NODE_ICONS[selectedNode.type] ?? 'bi-box'}`}
              style={{ color: nodeColor, fontSize: 16 }}
            />
          </div>
          <div style={{ minWidth: 0 }}>
            <div style={{
              fontWeight: 700, color: '#0f172a', fontSize: 13.5,
              lineHeight: 1.2, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
              letterSpacing: '-0.01em',
            }}>
              {label || selectedNode.data.label}
            </div>
            <div style={{
              fontSize: 10.5, color: '#94a3b8', marginTop: 2,
              fontFamily: 'monospace', letterSpacing: '0.02em',
            }}>
              {selectedNode.type}
            </div>
          </div>
        </div>
      </div>

      {/* ── Scrollable body ── */}
      <div style={{ flex: 1, overflowY: 'auto', padding: '12px 14px' }}>

        {/* Node Name */}
        <div style={{ marginBottom: 10 }}>
          <label style={labelStyle}>Node Name</label>
          <input type="text" value={label} onChange={(e) => commitLabel(e.target.value)} style={inputStyle} />
        </div>

        {/* Description */}
        {selectedNode.type !== 'NoteNode' && selectedNode.type !== 'ContainerNode' && (
          <div style={{ marginBottom: 12 }}>
            <label style={labelStyle}>
              Description
              <span style={{ color: '#94a3b8', fontWeight: 400, marginLeft: 4, fontSize: 10 }}>(shown on canvas)</span>
            </label>
            <textarea
              rows={2}
              value={params.description ?? ''}
              placeholder="Briefly describe what this node does…"
              onChange={(e) => commitParam('description', e.target.value)}
              style={{ ...inputStyle, resize: 'vertical' }}
            />
          </div>
        )}

        {/* Schema parameters */}
        {schema.parameters && schema.parameters.filter(p => p.name !== 'description').length > 0 && (
          <>
            <SectionHead icon="bi-sliders" label="Parameters" />
            <div style={{ padding: '6px 2px 0' }}>
              {schema.parameters.filter(p => p.name !== 'description').map(renderField)}
            </div>
          </>
        )}

        {/* Data Inputs */}
        {schema.dataInputs && schema.dataInputs.length > 0 && (
          <>
            <SectionHead icon="bi-arrow-down-circle-fill" label="Reads from WorkflowData" color="#3b82f6" />
            <div style={{ display: 'flex', flexDirection: 'column', gap: 4, padding: '6px 2px 4px' }}>
              {schema.dataInputs.map((p) => (
                <div key={p.key} style={{ display: 'flex', alignItems: 'baseline', gap: 6, fontSize: 11 }}>
                  <code style={{
                    background: p.isDynamic ? '#fef3c7' : '#eff6ff',
                    color: p.isDynamic ? '#92400e' : '#1d4ed8',
                    padding: '2px 6px', borderRadius: 4, fontFamily: 'monospace', fontSize: 11,
                    border: `1px solid ${p.isDynamic ? '#fde68a' : '#bfdbfe'}`,
                    whiteSpace: 'nowrap',
                  }}>{p.key}</code>
                  {p.required && <span style={{ color: '#ef4444', fontSize: 10, fontWeight: 600 }}>required</span>}
                  {p.isDynamic && <span style={{ color: '#92400e', fontSize: 10 }}>dynamic</span>}
                  {p.description && <span style={{ color: '#94a3b8' }}>{p.description}</span>}
                </div>
              ))}
            </div>
          </>
        )}

        {/* Data Outputs */}
        {schema.dataOutputs && schema.dataOutputs.length > 0 && (
          <>
            <SectionHead icon="bi-arrow-up-circle-fill" label="Writes to WorkflowData" color="#22c55e" />
            {selectedNode.data.nodeId && (
              <div style={{ fontSize: 10, color: '#64748b', padding: '4px 2px 2px', fontStyle: 'italic' }}>
                Reference as{' '}
                <code style={{ fontFamily: 'monospace', fontSize: 10, color: '#6366f1' }}>
                  {'{{' + selectedNode.data.nodeId + '.key}}'}
                </code>
              </div>
            )}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 4, padding: '6px 2px 4px' }}>
              {schema.dataOutputs.map((p) => (
                <div key={p.key} style={{ display: 'flex', alignItems: 'baseline', gap: 6, fontSize: 11 }}>
                  <code style={{
                    background: p.isDynamic ? '#fef3c7' : '#f0fdf4',
                    color: p.isDynamic ? '#92400e' : '#15803d',
                    padding: '2px 6px', borderRadius: 4, fontFamily: 'monospace', fontSize: 11,
                    border: `1px solid ${p.isDynamic ? '#fde68a' : '#bbf7d0'}`,
                    whiteSpace: 'nowrap',
                    cursor: selectedNode.data.nodeId ? 'pointer' : 'default',
                    userSelect: 'text',
                  }}
                    title={selectedNode.data.nodeId ? `Copy: {{${selectedNode.data.nodeId}.${p.key}}}` : p.description}
                  >{p.key}</code>
                  {p.isDynamic && <span style={{ color: '#92400e', fontSize: 10 }}>dynamic</span>}
                  {p.description && <span style={{ color: '#94a3b8' }}>{p.description}</span>}
                </div>
              ))}
            </div>
          </>
        )}

        {/* Delete / lock */}
        <div style={{ marginTop: 16, paddingTop: 12, borderTop: '1px solid #f1f5f9' }}>
          {selectedNode.type === 'StartNode' ? (
            <div style={{
              display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6,
              padding: '8px 12px', background: '#f8fafc', borderRadius: 7,
              fontSize: 12, color: '#94a3b8',
              border: '1px solid #f1f5f9',
            }}>
              <i className="bi bi-lock-fill" style={{ fontSize: 11 }} />
              Start node cannot be deleted
            </div>
          ) : (
            <button
              onClick={() => onDelete(selectedNode.id)}
              style={{
                width: '100%', padding: '8px 12px',
                background: 'transparent', border: '1.5px solid #fecaca',
                borderRadius: 7, cursor: 'pointer',
                color: '#ef4444', fontSize: 12, fontWeight: 600,
                display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6,
                fontFamily: 'inherit', transition: 'background 0.12s, border-color 0.12s',
              }}
              onMouseEnter={(e) => { e.currentTarget.style.background = '#fef2f2'; e.currentTarget.style.borderColor = '#fca5a5'; }}
              onMouseLeave={(e) => { e.currentTarget.style.background = 'transparent'; e.currentTarget.style.borderColor = '#fecaca'; }}
            >
              <i className="bi bi-trash" />
              Delete Node
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
