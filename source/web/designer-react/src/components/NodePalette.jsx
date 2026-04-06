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
  AI:      '#4A90E2',
  Control: '#F5A623',
  Data:    '#7ED321',
  IO:      '#BD10E0',
  Logic:   '#20c997',
  Visual:  '#6366f1',
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
    <div style={{ padding: '8px 6px', overflowY: 'auto', height: '100%' }}>
      {Object.entries(grouped).map(([category, nodes]) => {
        const catColor = CATEGORY_COLORS[category] ?? '#6c757d';
        return (
          <div key={category} style={{ marginBottom: 12 }}>
            {/* Category header */}
            <div
              style={{
                fontSize: 10,
                fontWeight: 700,
                color: catColor,
                textTransform: 'uppercase',
                letterSpacing: '0.7px',
                padding: '4px 6px 3px',
                display: 'flex',
                alignItems: 'center',
                gap: 5,
              }}
            >
              <i className={`bi ${CATEGORY_ICONS[category] ?? 'bi-box'}`} />
              {category}
            </div>

            {nodes.map((node) => {
              const disabled = disabledTypes.has(node.type);
              const icon = node.icon ?? NODE_ICONS[node.type] ?? 'bi-box';
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
                    padding: '5px 8px',
                    marginBottom: 2,
                    borderRadius: 6,
                    background: disabled ? '#f8f8f8' : '#fff',
                    border: `1px solid ${disabled ? '#e2e8f0' : '#f1f5f9'}`,
                    borderLeft: `3px solid ${disabled ? '#cbd5e1' : node.color}`,
                    cursor: disabled ? 'not-allowed' : 'grab',
                    userSelect: 'none',
                    opacity: disabled ? 0.5 : 1,
                    transition: 'background 0.1s, border-color 0.1s',
                  }}
                  onMouseEnter={(e) => { if (!disabled) e.currentTarget.style.background = '#f8fafc'; }}
                  onMouseLeave={(e) => { if (!disabled) e.currentTarget.style.background = '#fff'; }}
                >
                  {/* Icon badge */}
                  <div
                    style={{
                      width: 24,
                      height: 24,
                      borderRadius: 5,
                      backgroundColor: disabled ? '#e2e8f0' : `${node.color}18`,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      flexShrink: 0,
                    }}
                  >
                    <i
                      className={`bi ${icon}`}
                      style={{ color: disabled ? '#94a3b8' : node.color, fontSize: 12 }}
                    />
                  </div>

                  {/* Name */}
                  <span style={{ fontSize: 12, fontWeight: 500, color: disabled ? '#94a3b8' : '#334155', flex: 1 }}>
                    {node.name}
                  </span>

                  {disabled && (
                    <i className="bi bi-lock-fill" style={{ fontSize: 10, color: '#94a3b8' }} />
                  )}
                </div>
              );
            })}
          </div>
        );
      })}
    </div>
  );
}
