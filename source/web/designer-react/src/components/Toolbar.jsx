/**
 * Toolbar – top bar with workflow name, zoom controls, and action buttons.
 * When editing a sub-workflow, shows the sub-workflow name and a "Back to Main" button.
 */
export default function Toolbar({
  workflowName,
  workflowId,
  onSave,
  onZoomIn,
  onZoomOut,
  onFitView,
  saving,
  // Sub-workflow context
  activeSubWorkflow,  // null | { id, name }
  onBackToMain,       // () => void
}) {
  const contextLabel = activeSubWorkflow
    ? `${workflowName}  ›  ${activeSubWorkflow.name}`
    : workflowName;

  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '6px 16px',
        background: activeSubWorkflow ? '#4a235a' : '#343a40',
        color: '#fff',
        flexShrink: 0,
        gap: 8,
        transition: 'background 0.2s',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontWeight: 600, fontSize: 15 }}>
        <i className={activeSubWorkflow ? 'bi bi-diagram-3' : 'bi bi-diagram-3-fill'} />
        <span>{contextLabel}</span>
      </div>

      <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', alignItems: 'center' }}>
        {activeSubWorkflow && (
          <button
            className="btn btn-sm btn-warning"
            onClick={onBackToMain}
            title="Return to main workflow"
            style={{ fontWeight: 600 }}
          >
            <i className="bi bi-arrow-left-short" /> Main Flow
          </button>
        )}
        <button className="btn btn-sm btn-outline-light" onClick={onZoomIn} title="Zoom In">
          <i className="bi bi-zoom-in" /> Zoom In
        </button>
        <button className="btn btn-sm btn-outline-light" onClick={onZoomOut} title="Zoom Out">
          <i className="bi bi-zoom-out" /> Zoom Out
        </button>
        <button className="btn btn-sm btn-outline-light" onClick={onFitView} title="Fit to view">
          <i className="bi bi-arrows-fullscreen" /> Reset
        </button>
        <button className="btn btn-sm btn-success" onClick={onSave} disabled={saving}>
          <i className="bi bi-save-fill" /> {saving ? 'Saving…' : 'Save'}
        </button>
        {!activeSubWorkflow && (
          <>
            <a
              href={`/Workflow/Runner/${workflowId}`}
              className="btn btn-sm btn-primary"
              target="_blank"
              rel="noreferrer"
              title="Open runner in new tab"
            >
              <i className="bi bi-play-circle-fill" /> Run
            </a>
            <a href="/Workflow" className="btn btn-sm btn-outline-light" title="Back to list">
              <i className="bi bi-arrow-left-circle-fill" /> Back
            </a>
          </>
        )}
      </div>
    </div>
  );
}
