const CATEGORY_ICONS = {
  AI: 'bi-robot',
  Control: 'bi-signpost-split-fill',
  Data: 'bi-table',
  IO: 'bi-plug-fill',
  Visual: 'bi-bounding-box',
};

/**
 * Left sidebar node palette.
 * Nodes are draggable onto the canvas via HTML5 drag-and-drop.
 * `disabledTypes` is a Set of node type strings that cannot be added again
 * (single-instance nodes: StartNode, ErrorNode).
 */
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
      {Object.entries(grouped).map(([category, nodes]) => (
        <div key={category} style={{ marginBottom: 10 }}>
          <div
            style={{
              fontSize: 10,
              fontWeight: 700,
              color: '#6c757d',
              textTransform: 'uppercase',
              letterSpacing: '0.6px',
              padding: '4px 6px',
            }}
          >
            <i className={`bi ${CATEGORY_ICONS[category] ?? 'bi-box'}`} /> {category}
          </div>

          {nodes.map((node) => {
            const disabled = disabledTypes.has(node.type);
            const tooltipText = disabled
              ? `Only one ${node.name} is allowed per workflow`
              : node.description;
            return (
              <div
                key={node.type}
                draggable={!disabled}
                onDragStart={(e) => handleDragStart(e, node)}
                title={tooltipText}
                style={{
                  padding: '5px 8px',
                  marginBottom: 3,
                  borderRadius: 4,
                  background: disabled ? '#f0f0f0' : '#fff',
                  border: `1px solid ${disabled ? '#ccc' : node.color + '22'}`,
                  borderLeft: `4px solid ${disabled ? '#bbb' : node.color}`,
                  cursor: disabled ? 'not-allowed' : 'grab',
                  fontSize: 12,
                  fontWeight: 500,
                  color: disabled ? '#aaa' : '#343a40',
                  userSelect: 'none',
                  opacity: disabled ? 0.55 : 1,
                  transition: 'background 0.1s',
                }}
                onMouseEnter={(e) => { if (!disabled) e.currentTarget.style.background = '#f8f9fa'; }}
                onMouseLeave={(e) => { if (!disabled) e.currentTarget.style.background = '#fff'; }}
              >
                {node.name}
                {disabled && (
                  <i className="bi bi-lock-fill" style={{ float: 'right', fontSize: 10, marginTop: 2 }} />
                )}
              </div>
            );
          })}
        </div>
      ))}
    </div>
  );
}
