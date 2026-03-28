import { useState, useEffect, useRef } from 'react';

/**
 * Properties panel shown when a connector (edge) is selected.
 * Displays source → target names, an editable label, and a delete button.
 */
export default function EdgePropertiesPanel({ selectedEdge, sourceNodeName, targetNodeName, onChange, onDelete }) {
  const [label, setLabel] = useState('');
  const edgeIdRef = useRef(null);

  // Reset local state when a different edge is selected
  useEffect(() => {
    if (selectedEdge && selectedEdge.id !== edgeIdRef.current) {
      edgeIdRef.current = selectedEdge.id;
      setLabel(selectedEdge.label ?? '');
    }
  }, [selectedEdge?.id]);

  if (!selectedEdge) return null;

  const commitLabel = (val) => {
    setLabel(val);
    onChange(selectedEdge.id, val);
  };

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
        <i className="bi bi-arrow-left-right" /> Connector
      </h6>

      {/* Source → Target */}
      <div className="mb-2">
        <label className="form-label small text-muted mb-1">From</label>
        <input
          type="text"
          className="form-control form-control-sm"
          value={sourceNodeName ?? selectedEdge.source}
          disabled
          style={{ fontFamily: 'monospace', fontSize: 11 }}
        />
      </div>
      <div className="mb-3" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
        <i className="bi bi-arrow-down text-muted" style={{ flexShrink: 0 }} />
      </div>
      <div className="mb-3">
        <label className="form-label small text-muted mb-1">To</label>
        <input
          type="text"
          className="form-control form-control-sm"
          value={targetNodeName ?? selectedEdge.target}
          disabled
          style={{ fontFamily: 'monospace', fontSize: 11 }}
        />
      </div>

      {selectedEdge.sourceHandle && selectedEdge.sourceHandle !== 'output' && (
        <div className="mb-3">
          <label className="form-label small text-muted mb-1">Port</label>
          <input
            type="text"
            className="form-control form-control-sm"
            value={selectedEdge.sourceHandle}
            disabled
            style={{ fontSize: 11 }}
          />
        </div>
      )}

      <hr style={{ margin: '8px 0 12px' }} />

      {/* Label */}
      <div className="mb-1">
        <label className="form-label small fw-bold mb-1">Label</label>
        <input
          type="text"
          className="form-control form-control-sm"
          placeholder="Optional label…"
          value={label}
          onChange={(e) => commitLabel(e.target.value)}
        />
        <div className="form-text" style={{ fontSize: 10 }}>
          Displayed inline on the connector line.
        </div>
      </div>

      {/* Delete */}
      <hr style={{ margin: '14px 0 10px' }} />
      <button
        className="btn btn-danger btn-sm w-100"
        onClick={() => onDelete(selectedEdge.id)}
      >
        <i className="bi bi-trash" /> Delete Connector
      </button>
    </div>
  );
}
