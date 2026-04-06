/**
 * Toolbar – top bar with workflow name, zoom controls, and action buttons.
 * When editing a sub-workflow, shows the sub-workflow name and a "Back to Main" button.
 */
import { useState, useRef, useEffect } from 'react';

export default function Toolbar({
  workflowName,
  workflowId,
  mode,
  onSave,
  onZoomIn,
  onZoomOut,
  onFitView,
  saving,
  onExport,
  // Sub-workflow context
  activeSubWorkflow,  // null | { id, name }
  onBackToMain,       // () => void
}) {
  const isRunner = mode === 'runner';
  const contextLabel = activeSubWorkflow
    ? `${workflowName}  ›  ${activeSubWorkflow.name}`
    : workflowName;

  const [exportOpen, setExportOpen] = useState(false);
  const exportRef = useRef(null);
  const [dropPos, setDropPos] = useState({ top: 0, right: 0 });

  const toggleExport = () => {
    if (!exportOpen && exportRef.current) {
      const rect = exportRef.current.getBoundingClientRect();
      setDropPos({ top: rect.bottom + 4, right: window.innerWidth - rect.right });
    }
    setExportOpen((o) => !o);
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    if (!exportOpen) return;
    const handler = (e) => {
      if (exportRef.current && !exportRef.current.contains(e.target)) {
        setExportOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [exportOpen]);

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
        {/* Sub-workflow back button (designer mode only) */}
        {!isRunner && activeSubWorkflow && (
          <button
            className="btn btn-sm btn-warning"
            onClick={onBackToMain}
            title="Return to main workflow"
            style={{ fontWeight: 600 }}
          >
            <i className="bi bi-arrow-left-short" /> Main Flow
          </button>
        )}

        {/* Zoom / fit — always visible */}
        <button className="btn btn-sm btn-outline-light" onClick={onZoomIn} title="Zoom In">
          <i className="bi bi-zoom-in" /> Zoom In
        </button>
        <button className="btn btn-sm btn-outline-light" onClick={onZoomOut} title="Zoom Out">
          <i className="bi bi-zoom-out" /> Zoom Out
        </button>
        <button className="btn btn-sm btn-outline-light" onClick={onFitView} title="Fit to view">
          <i className="bi bi-arrows-fullscreen" /> Reset
        </button>

        {/* Designer-only: Save + Export */}
        {!isRunner && (
          <>
            <button className="btn btn-sm btn-success" onClick={onSave} disabled={saving}>
              <i className="bi bi-save-fill" /> {saving ? 'Saving…' : 'Save'}
            </button>
            <div style={{ flexShrink: 0, position: 'relative' }} ref={exportRef}>
              <button
                className="btn btn-sm btn-outline-light"
                onClick={toggleExport}
                title="Export canvas"
              >
                <i className="bi bi-download" /> Export
              </button>
              {exportOpen && (
                <ul
                  style={{
                    position: 'fixed',
                    top: dropPos.top,
                    right: dropPos.right,
                    zIndex: 99999,
                    background: '#fff',
                    border: '1px solid #dee2e6',
                    borderRadius: 6,
                    boxShadow: '0 4px 16px rgba(0,0,0,0.18)',
                    listStyle: 'none',
                    padding: '4px 0',
                    margin: 0,
                    minWidth: 170,
                  }}
                >
                  <li>
                    <button
                      style={{ display: 'block', width: '100%', textAlign: 'left', padding: '7px 16px', background: 'none', border: 'none', cursor: 'pointer', fontSize: 13, color: '#212529' }}
                      onMouseEnter={(e) => e.currentTarget.style.background = '#f8f9fa'}
                      onMouseLeave={(e) => e.currentTarget.style.background = 'none'}
                      onClick={() => { onExport('png'); setExportOpen(false); }}
                    >
                      <i className="bi bi-file-image" style={{ marginRight: 8 }} />Export as PNG
                    </button>
                  </li>
                  <li>
                    <button
                      style={{ display: 'block', width: '100%', textAlign: 'left', padding: '7px 16px', background: 'none', border: 'none', cursor: 'pointer', fontSize: 13, color: '#212529' }}
                      onMouseEnter={(e) => e.currentTarget.style.background = '#f8f9fa'}
                      onMouseLeave={(e) => e.currentTarget.style.background = 'none'}
                      onClick={() => { onExport('svg'); setExportOpen(false); }}
                    >
                      <i className="bi bi-filetype-svg" style={{ marginRight: 8 }} />Export as SVG
                    </button>
                  </li>
                </ul>
              )}
            </div>
          </>
        )}

        {/* Runner-only: Edit in Designer link */}
        {isRunner && (
          <a
            href={`/Workflow/Designer/${workflowId}`}
            className="btn btn-sm btn-outline-light"
            title="Open in designer"
          >
            <i className="bi bi-pencil-square" /> Edit
          </a>
        )}

        {/* Designer-only: Run link + Back */}
        {!isRunner && !activeSubWorkflow && (
          <a
            href={`/Workflow/Runner/${workflowId}`}
            className="btn btn-sm btn-primary"
            title="Open runner"
          >
            <i className="bi bi-play-circle-fill" /> Run
          </a>
        )}
        <a href="/Workflow" className="btn btn-sm btn-outline-light" title="Back to list">
          <i className="bi bi-arrow-left-circle-fill" /> Back
        </a>
      </div>
    </div>
  );
}
