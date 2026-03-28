import { useState } from 'react';
import SubWorkflowsPanel from './SubWorkflowsPanel.jsx';

/**
 * Variables sidebar panel.
 * Displays key/value pairs stored on the workflow definition,
 * and below that the list of reusable sub-workflows.
 */
export default function VariablesPanel({
  variables,
  onChange,
  // Sub-workflow props
  subWorkflows,
  activeSubId,
  onSubWorkflowOpen,
  onSubWorkflowCreate,
  onSubWorkflowRename,
  onSubWorkflowDelete,
}) {
  const [newKey, setNewKey] = useState('');
  const [newValue, setNewValue] = useState('');

  const handleAdd = () => {
    const key = newKey.trim();
    if (!key) return;
    onChange({ ...variables, [key]: newValue });
    setNewKey('');
    setNewValue('');
  };

  const handleDelete = (key) => {
    const updated = { ...variables };
    delete updated[key];
    onChange(updated);
  };

  const handleValueChange = (key, value) => {
    onChange({ ...variables, [key]: value });
  };

  return (
    <div style={{ padding: 10 }}>
      <div style={{ fontWeight: 600, fontSize: 13, marginBottom: 10, color: '#495057' }}>
        <i className="bi bi-braces" /> Workflow Variables
      </div>

      {Object.entries(variables).length === 0 && (
        <p className="text-muted small">No variables defined.</p>
      )}

      {Object.entries(variables).map(([key, value]) => (
        <div key={key} style={{ display: 'flex', alignItems: 'center', gap: 4, marginBottom: 5 }}>
          <code
            style={{
              flex: '0 0 auto',
              maxWidth: 90,
              fontSize: 11,
              padding: '2px 5px',
              background: '#e9ecef',
              borderRadius: 3,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              title: key,
            }}
            title={`{{${key}}}`}
          >
            {`{{${key}}}`}
          </code>
          <input
            type="text"
            className="form-control form-control-sm"
            style={{ flex: 1, fontSize: 11 }}
            value={String(value ?? '')}
            onChange={(e) => handleValueChange(key, e.target.value)}
          />
          <button
            className="btn btn-sm btn-outline-danger"
            style={{ padding: '1px 6px', flexShrink: 0 }}
            onClick={() => handleDelete(key)}
            title={`Delete ${key}`}
          >
            <i className="bi bi-x" />
          </button>
        </div>
      ))}

      {/* Add new variable */}
      <div style={{ display: 'flex', gap: 4, marginTop: 10 }}>
        <input
          type="text"
          className="form-control form-control-sm"
          placeholder="key"
          value={newKey}
          onChange={(e) => setNewKey(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
          style={{ fontFamily: 'monospace', fontSize: 11 }}
        />
        <input
          type="text"
          className="form-control form-control-sm"
          placeholder="value"
          value={newValue}
          onChange={(e) => setNewValue(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
          style={{ fontSize: 11 }}
        />
        <button
          className="btn btn-sm btn-outline-primary"
          onClick={handleAdd}
          style={{ flexShrink: 0 }}
          title="Add variable"
        >
          <i className="bi bi-plus" />
        </button>
      </div>

      {/* Divider */}
      <hr style={{ margin: '14px 0' }} />

      {/* Sub-workflows */}
      <SubWorkflowsPanel
        subWorkflows={subWorkflows ?? []}
        activeSubId={activeSubId}
        onOpen={onSubWorkflowOpen}
        onCreate={onSubWorkflowCreate}
        onRename={onSubWorkflowRename}
        onDelete={onSubWorkflowDelete}
      />
    </div>
  );
}
