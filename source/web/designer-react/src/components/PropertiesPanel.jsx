import { useState, useEffect, useRef } from 'react';
import { useSchemas } from '../context/SchemaContext';

/**
 * Right-side properties panel.
 * Uses local form state for smooth editing; commits to the ReactFlow node
 * on every field change for instant canvas updates.
 */
export default function PropertiesPanel({ selectedNode, onChange, onDelete }) {
  const schemas = useSchemas();

  // Local form state — reset only when a *different* node is selected
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
      <div style={{ padding: 16 }}>
        <h6 style={{ color: '#6c757d', fontWeight: 600, marginBottom: 8 }}>
          <i className="bi bi-sliders" /> Properties
        </h6>
        <p className="text-muted small">Select a node or connector to edit its properties.</p>
      </div>
    );
  }

  const schema = schemas[selectedNode.type] ?? {};

  // ── helpers ──────────────────────────────────────────────────────────────

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

  const renderField = (param) => {
    const value = params[param.name] ?? param.defaultValue ?? '';
    const id = `prop-${param.name}`;

    switch (param.type) {
      case 'TextArea':
      case 'Json':
        return (
          <div key={param.name} className="mb-2">
            <label htmlFor={id} className="form-label small fw-bold mb-1">
              {param.label}
              {param.required && <span className="text-danger"> *</span>}
            </label>
            <textarea
              id={id}
              className="form-control form-control-sm"
              rows={param.type === 'Json' ? 4 : 3}
              value={String(value)}
              placeholder={param.placeholder ?? param.description ?? ''}
              onChange={(e) => commitParam(param.name, e.target.value)}
              style={{ fontFamily: param.type === 'Json' ? 'monospace' : 'inherit', fontSize: 12 }}
            />
          </div>
        );

      case 'Number':
        return (
          <div key={param.name} className="mb-2">
            <label htmlFor={id} className="form-label small fw-bold mb-1">
              {param.label}
              {param.required && <span className="text-danger"> *</span>}
            </label>
            <input
              id={id}
              type="number"
              className="form-control form-control-sm"
              value={value}
              min={param.minValue}
              max={param.maxValue}
              step={param.name === 'temperature' ? 0.1 : 1}
              onChange={(e) => commitParam(param.name, parseFloat(e.target.value))}
            />
          </div>
        );

      case 'Boolean':
        return (
          <div key={param.name} className="mb-2 form-check">
            <input
              id={id}
              type="checkbox"
              className="form-check-input"
              checked={Boolean(value)}
              onChange={(e) => commitParam(param.name, e.target.checked)}
            />
            <label htmlFor={id} className="form-check-label small">
              {param.label}
            </label>
          </div>
        );

      case 'Select':
        return (
          <div key={param.name} className="mb-2">
            <label htmlFor={id} className="form-label small fw-bold mb-1">
              {param.label}
              {param.required && <span className="text-danger"> *</span>}
            </label>
            <select
              id={id}
              className="form-select form-select-sm"
              value={String(value)}
              onChange={(e) => commitParam(param.name, e.target.value)}
            >
              {(param.options ?? []).map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
        );

      default: // Text
        return (
          <div key={param.name} className="mb-2">
            <label htmlFor={id} className="form-label small fw-bold mb-1">
              {param.label}
              {param.required && <span className="text-danger"> *</span>}
            </label>
            <input
              id={id}
              type="text"
              className="form-control form-control-sm"
              value={String(value)}
              placeholder={param.placeholder ?? param.description ?? ''}
              onChange={(e) => commitParam(param.name, e.target.value)}
            />
          </div>
        );
    }
  };

  // ── render ───────────────────────────────────────────────────────────────

  return (
    <div style={{ padding: 14, overflowY: 'auto', height: '100%' }}>
      <h6
        style={{
          fontWeight: 600,
          color: '#495057',
          marginBottom: 12,
          paddingBottom: 8,
          borderBottom: '1px solid #dee2e6',
        }}
      >
        <i className="bi bi-sliders" /> {label || selectedNode.data.label}
      </h6>

      {/* Node Name */}
      <div className="mb-2">
        <label className="form-label small fw-bold mb-1">Node Name</label>
        <input
          type="text"
          className="form-control form-control-sm"
          value={label}
          onChange={(e) => commitLabel(e.target.value)}
        />
      </div>

      {/* Type (read-only) */}
      <div className="mb-3">
        <label className="form-label small text-muted mb-1">Type</label>
        <input
          type="text"
          className="form-control form-control-sm"
          value={selectedNode.type}
          disabled
        />
      </div>

      {/* Schema-driven parameter fields */}
      {schema.parameters && schema.parameters.length > 0 && (
        <>
          <hr style={{ margin: '8px 0' }} />
          <h6 className="small fw-bold mb-2">Parameters</h6>
          {schema.parameters.map(renderField)}
        </>
      )}

      {/* Delete — StartNode is permanent and cannot be removed */}
      <hr style={{ margin: '14px 0 10px' }} />
      {selectedNode.type === 'StartNode' ? (
        <div
          className="text-muted small text-center"
          style={{ padding: '6px 0', background: '#f8f9fa', borderRadius: 4 }}
        >
          <i className="bi bi-lock-fill" /> Start node cannot be deleted
        </div>
      ) : (
        <button
          className="btn btn-danger btn-sm w-100"
          onClick={() => onDelete(selectedNode.id)}
        >
          <i className="bi bi-trash" /> Delete Node
        </button>
      )}
    </div>
  );
}
