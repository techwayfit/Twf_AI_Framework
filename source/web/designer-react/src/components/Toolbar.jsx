/**
 * Toolbar – top bar with workflow name, zoom controls, and action buttons.
 * When editing a sub-workflow, shows the sub-workflow name and a "Back to Main" button.
 */
import { useState, useRef, useEffect } from 'react';

/** Small icon-only or icon+label button for the toolbar */
function ToolBtn({ icon, label, onClick, title, href, variant = 'ghost', disabled }) {
  const base = {
    display: 'inline-flex', alignItems: 'center', gap: 5,
    padding: label ? '5px 11px' : '5px 8px',
    border: 'none', borderRadius: 6, cursor: disabled ? 'not-allowed' : 'pointer',
    fontSize: 12, fontWeight: 500, lineHeight: 1, whiteSpace: 'nowrap',
    textDecoration: 'none', transition: 'background 0.12s, opacity 0.12s',
    opacity: disabled ? 0.5 : 1,
    fontFamily: 'inherit',
  };

  const variants = {
    ghost:   { background: 'rgba(255,255,255,0.06)', color: '#cbd5e1' },
    primary: { background: '#6366f1', color: '#fff' },
    success: { background: '#22c55e', color: '#fff' },
    warning: { background: '#f59e0b', color: '#1e293b' },
  };

  const hoverBg = {
    ghost:   'rgba(255,255,255,0.12)',
    primary: '#4f46e5',
    success: '#16a34a',
    warning: '#d97706',
  };

  const style = { ...base, ...variants[variant] };

  const [hovered, setHovered] = useState(false);
  const hStyle = hovered && !disabled ? { ...style, background: hoverBg[variant] } : style;

  const content = (
    <>
      <i className={`bi ${icon}`} style={{ fontSize: 13 }} />
      {label && <span>{label}</span>}
    </>
  );

  if (href) {
    return (
      <a href={href} title={title} style={hStyle}
        onMouseEnter={() => setHovered(true)}
        onMouseLeave={() => setHovered(false)}>
        {content}
      </a>
    );
  }

  return (
    <button onClick={onClick} title={title} disabled={disabled} style={hStyle}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}>
      {content}
    </button>
  );
}

/** Thin vertical separator between button groups */
function Sep() {
  return (
    <div style={{
      width: 1, height: 20, background: 'rgba(255,255,255,0.12)',
      flexShrink: 0, alignSelf: 'center',
    }} />
  );
}

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
  activeSubWorkflow,
  onBackToMain,
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
      setDropPos({ top: rect.bottom + 6, right: window.innerWidth - rect.right });
    }
    setExportOpen((o) => !o);
  };

  useEffect(() => {
    if (!exportOpen) return;
    const handler = (e) => {
      if (exportRef.current && !exportRef.current.contains(e.target)) setExportOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [exportOpen]);

  const toolbarBg = activeSubWorkflow
    ? 'linear-gradient(90deg, #2d1b69 0%, #4a1d96 100%)'
    : 'linear-gradient(90deg, #0f172a 0%, #1e293b 100%)';

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 12px',
      height: 46,
      background: toolbarBg,
      flexShrink: 0,
      gap: 8,
      borderBottom: '1px solid rgba(255,255,255,0.07)',
      transition: 'background 0.3s',
    }}>
      {/* ── Left: branding + workflow name ── */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
        <div style={{
          width: 28, height: 28, borderRadius: 7,
          background: 'linear-gradient(135deg, #6366f1, #818cf8)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          flexShrink: 0,
          boxShadow: '0 2px 6px rgba(99,102,241,0.35)',
        }}>
          <i className="bi bi-diagram-3-fill" style={{ color: '#fff', fontSize: 14 }} />
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, minWidth: 0 }}>
          {activeSubWorkflow && (
            <span style={{ color: '#64748b', fontSize: 13 }}>
              {workflowName}
              <i className="bi bi-chevron-right" style={{ fontSize: 10, margin: '0 4px', verticalAlign: 'middle' }} />
            </span>
          )}
          <span style={{
            fontWeight: 600, fontSize: 14, color: '#f1f5f9',
            letterSpacing: '-0.01em', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
          }}>
            {activeSubWorkflow ? activeSubWorkflow.name : workflowName}
          </span>
          {isRunner && (
            <span style={{
              fontSize: 10, fontWeight: 600, padding: '2px 7px', borderRadius: 20,
              background: 'rgba(34,197,94,0.15)', color: '#4ade80',
              border: '1px solid rgba(34,197,94,0.3)',
              letterSpacing: '0.04em',
            }}>
              RUNNER
            </span>
          )}
        </div>
      </div>

      {/* ── Right: action buttons ── */}
      <div style={{ display: 'flex', gap: 4, alignItems: 'center', flexShrink: 0 }}>
        {/* Back to main sub-workflow */}
        {!isRunner && activeSubWorkflow && (
          <>
            <ToolBtn icon="bi-arrow-left-short" label="Main Flow" onClick={onBackToMain} variant="warning" />
            <Sep />
          </>
        )}

        {/* Zoom group */}
        <ToolBtn icon="bi-zoom-in"          onClick={onZoomIn}  title="Zoom In"     />
        <ToolBtn icon="bi-zoom-out"         onClick={onZoomOut} title="Zoom Out"    />
        <ToolBtn icon="bi-arrows-fullscreen" onClick={onFitView} title="Fit to view" />

        {!isRunner && (
          <>
            <Sep />
            {/* Save */}
            <ToolBtn icon="bi-save-fill" label={saving ? 'Saving…' : 'Save'} onClick={onSave} disabled={saving} variant="success" />

            {/* Export dropdown */}
            <div style={{ position: 'relative' }} ref={exportRef}>
              <ToolBtn icon="bi-download" label="Export" onClick={toggleExport} />
              {exportOpen && (
                <ul style={{
                  position: 'fixed',
                  top: dropPos.top,
                  right: dropPos.right,
                  zIndex: 99999,
                  background: '#fff',
                  border: '1px solid #e2e8f0',
                  borderRadius: 8,
                  boxShadow: '0 8px 24px rgba(15,23,42,0.15)',
                  listStyle: 'none',
                  padding: '4px 0',
                  margin: 0,
                  minWidth: 175,
                }}>
                  {[
                    { fmt: 'png', icon: 'bi-file-image',    label: 'Export as PNG' },
                    { fmt: 'svg', icon: 'bi-filetype-svg',  label: 'Export as SVG' },
                  ].map(({ fmt, icon, label }) => (
                    <li key={fmt}>
                      <button
                        onClick={() => { onExport(fmt); setExportOpen(false); }}
                        style={{
                          display: 'flex', alignItems: 'center', gap: 8,
                          width: '100%', textAlign: 'left', padding: '8px 14px',
                          background: 'none', border: 'none', cursor: 'pointer',
                          fontSize: 13, color: '#1e293b', fontFamily: 'inherit',
                          borderRadius: 0,
                        }}
                        onMouseEnter={(e) => e.currentTarget.style.background = '#f8fafc'}
                        onMouseLeave={(e) => e.currentTarget.style.background = 'none'}
                      >
                        <i className={`bi ${icon}`} style={{ color: '#6366f1', fontSize: 14 }} />
                        {label}
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </>
        )}

        {/* Runner-only: edit link */}
        {isRunner && (
          <ToolBtn icon="bi-pencil-square" label="Edit" href={`/Workflow/Designer/${workflowId}`} />
        )}

        <Sep />

        {/* Run link (designer only, main flow only) */}
        {!isRunner && !activeSubWorkflow && (
          <ToolBtn icon="bi-play-circle-fill" label="Run" href={`/Workflow/Runner/${workflowId}`} variant="primary" />
        )}

        <ToolBtn icon="bi-arrow-left-circle-fill" label="Back" href="/Workflow" />
      </div>
    </div>
  );
}
