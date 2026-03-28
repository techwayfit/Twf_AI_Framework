/**
 * Sub-Workflows panel — lists reusable child workflows (ChildWorkflowDefinition)
 * that are stored under workflowDef.subWorkflows[].
 * A SubWorkflowNode references them by id.
 */
export default function SubWorkflowsPanel({
  subWorkflows,
  activeSubId,   // null = main workflow is active
  onOpen,        // (id) => void
  onCreate,      // (name) => void
  onRename,      // (id, newName) => void
  onDelete,      // (id) => void
}) {
  const handleCreate = () => {
    const name = prompt('Sub-workflow name:', 'Sub Workflow');
    if (name?.trim()) onCreate(name.trim());
  };

  const handleRename = (e, sw) => {
    e.stopPropagation();
    const name = prompt('Rename sub-workflow:', sw.name);
    if (name?.trim()) onRename(sw.id, name.trim());
  };

  const handleDelete = (e, sw) => {
    e.stopPropagation();
    if (confirm(`Delete "${sw.name}"? This cannot be undone.`)) onDelete(sw.id);
  };

  return (
    <div style={{ padding: '8px 6px' }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 8,
        }}
      >
        <span style={{ fontWeight: 600, fontSize: 13, color: '#495057' }}>
          <i className="bi bi-diagram-3" /> Sub-Workflows
        </span>
        <button
          className="btn btn-sm btn-outline-primary"
          style={{ padding: '1px 8px', fontSize: 12 }}
          onClick={handleCreate}
          title="Add sub-workflow"
        >
          <i className="bi bi-plus-circle" /> Add
        </button>
      </div>

      {subWorkflows.length === 0 && (
        <p className="text-muted small" style={{ padding: '0 4px' }}>
          No sub-workflows. Click Add to create one.
        </p>
      )}

      {subWorkflows.map((sw) => {
        const isActive = sw.id === activeSubId;
        const nodeCount = sw.nodes?.length ?? 0;
        const connCount = sw.connections?.length ?? 0;

        return (
          <div
            key={sw.id}
            onClick={() => onOpen(sw.id)}
            style={{
              padding: '6px 8px',
              marginBottom: 4,
              borderRadius: 4,
              background: isActive ? '#e8f4fd' : '#fff',
              border: isActive ? '1px solid #0d6efd' : '1px solid #dee2e6',
              borderLeft: `4px solid ${isActive ? '#0d6efd' : '#8e44ad'}`,
              cursor: 'pointer',
              transition: 'background 0.1s',
            }}
            onMouseEnter={(e) => {
              if (!isActive) e.currentTarget.style.background = '#f8f9fa';
            }}
            onMouseLeave={(e) => {
              if (!isActive) e.currentTarget.style.background = '#fff';
            }}
          >
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <span
                style={{
                  fontSize: 12,
                  fontWeight: isActive ? 700 : 500,
                  color: '#212529',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                  flex: 1,
                }}
              >
                {sw.name}
              </span>
              <span style={{ display: 'flex', gap: 2, flexShrink: 0 }}>
                <button
                  className="btn btn-sm"
                  style={{ padding: '0 4px', fontSize: 11, color: '#6c757d' }}
                  title="Rename"
                  onClick={(e) => handleRename(e, sw)}
                >
                  <i className="bi bi-pencil" />
                </button>
                <button
                  className="btn btn-sm"
                  style={{ padding: '0 4px', fontSize: 11, color: '#dc3545' }}
                  title="Delete"
                  onClick={(e) => handleDelete(e, sw)}
                >
                  <i className="bi bi-trash" />
                </button>
              </span>
            </div>
            <div style={{ fontSize: 10, color: '#6c757d', marginTop: 2 }}>
              {nodeCount} node{nodeCount !== 1 ? 's' : ''}, {connCount} connection{connCount !== 1 ? 's' : ''}
            </div>
          </div>
        );
      })}
    </div>
  );
}
