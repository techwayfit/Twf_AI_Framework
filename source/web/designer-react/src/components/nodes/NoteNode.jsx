import { Handle, Position, NodeToolbar, useReactFlow } from '@xyflow/react';

const FOLD = 16; // px — folded corner size

const COLOR_MAP = {
  yellow: { bg: '#fff9c4', border: '#f9a825', text: '#5d4037', fold: '#f9a825' },
  blue:   { bg: '#e3f2fd', border: '#1565c0', text: '#0d3c61', fold: '#1565c0' },
  green:  { bg: '#e8f5e9', border: '#2e7d32', text: '#1b5e20', fold: '#2e7d32' },
  red:    { bg: '#fce4ec', border: '#c62828', text: '#4a0000', fold: '#c62828' },
  purple: { bg: '#f3e5f5', border: '#6a1b9a', text: '#3e0063', fold: '#6a1b9a' },
};

export default function NoteNode({ id, data, selected }) {
  const { setNodes, setEdges } = useReactFlow();
  const theme = COLOR_MAP[data.parameters?.color ?? 'yellow'] ?? COLOR_MAP.yellow;
  const text  = data.parameters?.text ?? '';

  const deleteNode = () => {
    setNodes((ns) => ns.filter((n) => n.id !== id));
    setEdges((es) => es.filter((e) => e.source !== id && e.target !== id));
  };

  return (
    <>
      {selected && (
        <NodeToolbar position={Position.Top} align="end" offset={4}>
          <button
            onClick={deleteNode}
            title="Delete node"
            className="nodrag"
            style={{
              width: 22, height: 22, borderRadius: '50%',
              background: '#ef4444', border: '2px solid #fff',
              color: '#fff', fontSize: 13, lineHeight: 1,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              cursor: 'pointer', padding: 0,
              boxShadow: '0 1px 4px rgba(0,0,0,0.25)',
            }}
          >
            <i className="bi bi-x" />
          </button>
        </NodeToolbar>
      )}
    <div
      style={{
        position: 'relative',
        width: 200,
        minHeight: 80,
        backgroundColor: theme.bg,
        border: `1.5px ${selected ? 'solid' : 'dashed'} ${theme.border}`,
        borderRadius: 4,
        // Clip the top-right corner for the fold effect
        clipPath: `polygon(0 0, calc(100% - ${FOLD}px) 0, 100% ${FOLD}px, 100% 100%, 0 100%)`,
        padding: `10px ${FOLD + 6}px 10px 12px`,
        fontFamily: "'Segoe UI', sans-serif",
        cursor: 'default',
        userSelect: 'none',
        opacity: selected ? 1 : 0.92,
      }}
    >
      {/* Folded corner triangle */}
      <div
        style={{
          position: 'absolute',
          top: 0,
          right: 0,
          width: 0,
          height: 0,
          borderStyle: 'solid',
          borderWidth: `0 ${FOLD}px ${FOLD}px 0`,
          borderColor: `transparent ${theme.fold} transparent transparent`,
          opacity: 0.5,
          pointerEvents: 'none',
        }}
      />

      {/* "Note" badge */}
      <div
        style={{
          fontSize: 9,
          fontWeight: 700,
          letterSpacing: 1,
          color: theme.border,
          textTransform: 'uppercase',
          marginBottom: 5,
          opacity: 0.7,
        }}
      >
        ✏ Note
      </div>

      {/* Note text */}
      <div
        style={{
          fontSize: 12,
          color: theme.text,
          lineHeight: 1.5,
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-word',
          minHeight: 24,
        }}
      >
        {text || <span style={{ opacity: 0.4, fontStyle: 'italic' }}>No text</span>}
      </div>

      {/* Single output handle — connects to the node this note refers to */}
      <Handle
        type="source"
        position={Position.Right}
        id="ref"
        style={{
          background: theme.border,
          border: `2px solid ${theme.bg}`,
          width: 8,
          height: 8,
          opacity: 0.7,
        }}
        title="Link to node"
      />
    </div>
    </>
  );
}
