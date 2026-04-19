import { NODE_ICONS } from '../nodeConfig';

const CATEGORY_ICONS = {
  AI:      'bi-robot',
  Control: 'bi-signpost-split-fill',
  Data:    'bi-table',
  IO:      'bi-plug-fill',
  Logic:   'bi-cpu-fill',
  Visual:  'bi-bounding-box',
};

const CATEGORY_COLORS = {
  AI:      '#6366f1',
  Control: '#f59e0b',
  Data:    '#22c55e',
  IO:      '#ec4899',
  Logic:   '#14b8a6',
  Visual:  '#8b5cf6',
};

export default function NodePalette({ availableNodes, disabledTypes = new Set() }) {
  const grouped = availableNodes.reduce((acc, node) => {
    (acc[node.category] ??= []).push(node);
    return acc;
  }, {});

  const handleDragStart = (e, node) => {
    if (disabledTypes.has(node.type)) { e.preventDefault(); return; }
    e.dataTransfer.setData('application/nodeType', JSON.stringify(node));
    e.dataTransfer.effectAllowed = 'copy';
  };

  return (
    <div style={{ padding: '10px 8px', overflowY: 'auto', height: '100%' }}>
      {Object.entries(grouped).map(([category, nodes]) => {
        const catColor = CATEGORY_COLORS[category] ?? '#6c757d';
        return (
          <div key={category} style={{ marginBottom: 14 }}>
            {/* Category header */}
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: 6,
              padding: '3px 6px 5px',
              marginBottom: 3,
            }}>
              <div style={{
                width: 18, height: 18, borderRadius: 4,
                background: `${catColor}18`,
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                flexShrink: 0,
              }}>
                <i className={`bi ${CATEGORY_ICONS[category] ?? 'bi-box'}`}
                   style={{ color: catColor, fontSize: 10 }} />
              </div>
              <span style={{
                fontSize: 10.5,
                fontWeight: 700,
                color: catColor,
                textTransform: 'uppercase',
                letterSpacing: '0.06em',
              }}>
                {category}
              </span>
            </div>

            {nodes.map((node) => {
              const disabled = disabledTypes.has(node.type);
              const icon = node.icon ?? NODE_ICONS[node.type] ?? 'bi-box';
              const nodeColor = disabled ? '#94a3b8' : node.color;
              return (
                <div
                  key={node.type}
                  draggable={!disabled}
                  onDragStart={(e) => handleDragStart(e, node)}
                  title={disabled ? `Only one ${node.name} is allowed per workflow` : node.description}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                    padding: '6px 8px',
                    marginBottom: 2,
                    borderRadius: 7,
                    background: disabled ? '#f8fafc' : '#ffffff',
                    border: `1px solid ${disabled ? '#e2e8f0' : '#eef2f7'}`,
                    borderLeft: `3px solid ${disabled ? '#d1d5db' : node.color}`,
                    cursor: disabled ? 'not-allowed' : 'grab',
                    userSelect: 'none',
                    opacity: disabled ? 0.5 : 1,
                    transition: 'background 0.12s, box-shadow 0.12s, border-color 0.12s',
                    boxShadow: disabled ? 'none' : '0 1px 2px rgba(15,23,42,0.04)',
                  }}
                  onMouseEnter={(e) => {
                    if (!disabled) {
                      e.currentTarget.style.background = '#f8fafc';
                      e.currentTarget.style.boxShadow = '0 2px 6px rgba(15,23,42,0.08)';
                      e.currentTarget.style.borderColor = node.color + '55';
                    }
                  }}
                  onMouseLeave={(e) => {
                    if (!disabled) {
                      e.currentTarget.style.background = '#ffffff';
                      e.currentTarget.style.boxShadow = '0 1px 2px rgba(15,23,42,0.04)';
                      e.currentTarget.style.borderColor = '#eef2f7';
                    }
                  }}
                >
                  {/* Icon badge */}
                  <div style={{
                    width: 26, height: 26, borderRadius: 6,
                    background: disabled ? '#f1f5f9' : `linear-gradient(135deg, ${node.color}20, ${node.color}10)`,
                    border: `1px solid ${disabled ? '#e2e8f0' : node.color + '28'}`,
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    flexShrink: 0,
                  }}>
                    <i className={`bi ${icon}`} style={{ color: nodeColor, fontSize: 12 }} />
                  </div>

                  {/* Name */}
                  <span style={{
                    fontSize: 12, fontWeight: 500,
                    color: disabled ? '#94a3b8' : '#1e293b',
                    flex: 1, lineHeight: 1.3,
                    letterSpacing: '-0.005em',
                  }}>
                    {node.name}
                  </span>

                  {disabled
                    ? <i className="bi bi-lock-fill" style={{ fontSize: 9, color: '#94a3b8' }} />
                    : <i className="bi bi-grip-vertical" style={{ fontSize: 10, color: '#d1d5db' }} />
                  }
                </div>
              );
            })}
          </div>
        );
      })}
    </div>
  );
}
