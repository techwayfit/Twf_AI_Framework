/**
 * RunnerPanel — left sidebar shown when the app is in runner mode.
 *
 * Responsibilities:
 *  - Collect initial input key-value pairs
 *  - Stream SSE events from POST /Workflow/RunStream/{id}
 *  - Fire onNodeStateChange(nodeId, state) so the canvas can highlight nodes
 *  - Render a collapsible step-card execution log
 */
import { useState, useRef, useEffect, useCallback } from 'react';

export default function RunnerPanel({ workflowId, onNodeStateChange, onResetNodeStates }) {
  const [inputs, setInputs] = useState([{ key: '', value: '' }]);
  const [status, setStatus] = useState('idle'); // idle | running | done | error | stopped
  const [steps, setSteps] = useState([]);
  const abortRef = useRef(null);

  // Keep a stable ref to the callback so the async SSE loop never captures a stale version
  const onNodeStateRef = useRef(onNodeStateChange);
  useEffect(() => { onNodeStateRef.current = onNodeStateChange; }, [onNodeStateChange]);

  // ── Input helpers ──────────────────────────────────────────────────────────
  const addInput    = () => setInputs(s => [...s, { key: '', value: '' }]);
  const removeInput = i  => setInputs(s => s.filter((_, idx) => idx !== i));
  const updateInput = (i, field, val) =>
    setInputs(s => s.map((item, idx) => idx === i ? { ...item, [field]: val } : item));

  // ── SSE streaming ──────────────────────────────────────────────────────────
  const handleStart = useCallback(async () => {
    onResetNodeStates();
    setSteps([]);
    setStatus('running');

    const initialData = {};
    inputs.forEach(({ key, value }) => { if (key.trim()) initialData[key.trim()] = value; });

    abortRef.current = new AbortController();

    const dispatch = (chunk) => {
      let eventType = '', data = '';
      for (const line of chunk.split('\n')) {
        if (line.startsWith('event: ')) eventType = line.slice(7).trim();
        else if (line.startsWith('data: ')) data = line.slice(6).trim();
      }
      if (!eventType || !data) return;
      let payload;
      try { payload = JSON.parse(data); } catch { return; }

      if (eventType === 'node_start') {
        onNodeStateRef.current(payload.nodeId, 'running');
        setSteps(s => [...s, { id: payload.nodeId, ...payload, stepType: 'node_start' }]);
      } else if (eventType === 'node_done') {
        onNodeStateRef.current(payload.nodeId, 'done');
        setSteps(s => s.map(st => st.id === payload.nodeId
          ? { ...st, ...payload, stepType: 'node_done' } : st));
      } else if (eventType === 'node_error') {
        onNodeStateRef.current(payload.nodeId, 'error');
        setSteps(s => s.map(st => st.id === payload.nodeId
          ? { ...st, ...payload, stepType: 'node_error' } : st));
      } else if (eventType === 'workflow_done') {
        setStatus('done');
      } else if (eventType === 'workflow_error') {
        setStatus('error');
        if (payload.errorMessage) {
          setSteps(s => [...s, {
            id: '_workflow_error',
            stepType: 'node_error',
            nodeName: 'Workflow',
            nodeType: '',
            errorMessage: payload.errorMessage,
          }]);
        }
      }
    };

    try {
      const response = await fetch(`/Workflow/RunStream/${workflowId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ initialData }),
        signal: abortRef.current.signal,
      });

      if (!response.ok) {
        const text = await response.text();
        throw new Error(`HTTP ${response.status}: ${text}`);
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });
        const parts = buffer.split('\n\n');
        buffer = parts.pop();
        for (const chunk of parts) dispatch(chunk);
      }
      if (buffer.trim()) dispatch(buffer);
    } catch (err) {
      if (err.name === 'AbortError') {
        setStatus('stopped');
      } else {
        setStatus('error');
        setSteps(s => [...s, {
          id: '_stream_error',
          stepType: 'node_error',
          nodeName: 'Connection Error',
          nodeType: '',
          errorMessage: err.message,
        }]);
      }
    }
  }, [workflowId, inputs]);

  const handleStop = () => {
    abortRef.current?.abort();
    abortRef.current = null;
    setStatus('stopped');
  };

  const handleClear = () => {
    setSteps([]);
    setStatus('idle');
    onResetNodeStates();
  };

  const isRunning = status === 'running';

  const STATUS_CONFIG = {
    idle:    { label: 'Ready',     cls: 'bg-secondary' },
    running: { label: 'Running…',  cls: 'bg-warning text-dark' },
    done:    { label: 'Completed', cls: 'bg-success' },
    error:   { label: 'Failed',    cls: 'bg-danger' },
    stopped: { label: 'Stopped',   cls: 'bg-dark' },
  };
  const { label: statusLabel, cls: statusCls } = STATUS_CONFIG[status] ?? STATUS_CONFIG.idle;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden', fontSize: 13 }}>

      {/* ── Header: status + run/stop/clear ── */}
      <div style={{ padding: '10px 12px', borderBottom: '1px solid #dee2e6', background: '#f8f9fa', flexShrink: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
          <span style={{ fontWeight: 700, fontSize: 13 }}>
            <i className="bi bi-play-circle" style={{ marginRight: 5 }} />
            Run Workflow
          </span>
          <span className={`badge ${statusCls}`} style={{ fontSize: 11 }}>{statusLabel}</span>
        </div>
        <div style={{ display: 'flex', gap: 5 }}>
          <button className="btn btn-success btn-sm" style={{ flex: 1 }}
            onClick={handleStart} disabled={isRunning}>
            <i className="bi bi-play-fill" /> Run
          </button>
          <button className="btn btn-danger btn-sm" style={{ flex: 1 }}
            onClick={handleStop} disabled={!isRunning}>
            <i className="bi bi-stop-fill" /> Stop
          </button>
          <button className="btn btn-outline-secondary btn-sm px-2"
            onClick={handleClear} disabled={isRunning} title="Clear log">
            <i className="bi bi-trash" />
          </button>
        </div>
      </div>

      {/* ── Initial input data ── */}
      <div style={{ padding: '10px 12px', borderBottom: '1px solid #dee2e6', flexShrink: 0 }}>
        <div style={{ fontSize: 11, fontWeight: 600, color: '#495057', marginBottom: 6, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
          Initial Data
        </div>
        {inputs.map((item, i) => (
          <div key={i} style={{ display: 'flex', gap: 4, marginBottom: 4 }}>
            <input
              type="text"
              className="form-control form-control-sm"
              placeholder="key"
              value={item.key}
              onChange={e => updateInput(i, 'key', e.target.value)}
              style={{ flex: '0 0 40%', minWidth: 0 }}
            />
            <input
              type="text"
              className="form-control form-control-sm"
              placeholder="value"
              value={item.value}
              onChange={e => updateInput(i, 'value', e.target.value)}
              style={{ flex: 1, minWidth: 0 }}
            />
            <button
              className="btn btn-outline-danger btn-sm px-2"
              onClick={() => removeInput(i)}
              disabled={inputs.length === 1}
              title="Remove"
            >
              <i className="bi bi-x" />
            </button>
          </div>
        ))}
        <button className="btn btn-outline-secondary btn-sm w-100 mt-1" onClick={addInput}>
          <i className="bi bi-plus" /> Add field
        </button>
      </div>

      {/* ── Execution log ── */}
      <div style={{ flex: 1, overflowY: 'auto', padding: '8px 10px' }}>
        <div style={{ fontSize: 11, fontWeight: 600, color: '#495057', marginBottom: 6, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
          Execution Log
        </div>
        {steps.length === 0 ? (
          <div style={{ color: '#adb5bd', fontSize: 12, textAlign: 'center', marginTop: 20 }}>
            No steps yet — press Run to start
          </div>
        ) : (
          steps.map(step => <StepCard key={step.id} step={step} />)
        )}
      </div>
    </div>
  );
}

// ─── Step card ────────────────────────────────────────────────────────────────

const NO_PORTS_MSG = 'No Data in/out is configured';

function StepCard({ step }) {
  const isError = step.stepType === 'node_error';
  const isDone  = step.stepType === 'node_done';
  const isTerminal = isDone || isError;

  // A terminal step always has something to show (data or the "not configured" message)
  const [expanded, setExpanded] = useState(false);
  useEffect(() => {
    if (isTerminal) setExpanded(true);
  }, [isTerminal]);

  const borderColor = isError ? '#ef4444' : isDone ? '#22c55e' : '#f59e0b';
  const icon        = isError ? '❌' : isDone ? '✅' : '⏳';

  const hasDetails = isTerminal || !!step.errorMessage;

  return (
    <div style={{
      borderLeft: `3px solid ${borderColor}`,
      background: '#fff',
      borderRadius: 4,
      marginBottom: 5,
      overflow: 'hidden',
      boxShadow: '0 1px 3px rgba(0,0,0,0.07)',
    }}>
      <div
        style={{
          display: 'flex', alignItems: 'center', gap: 6, padding: '6px 10px',
          cursor: hasDetails ? 'pointer' : 'default',
          userSelect: 'none',
        }}
        onClick={() => hasDetails && setExpanded(e => !e)}
      >
        <span style={{ fontSize: 12, flexShrink: 0 }}>{icon}</span>
        <span style={{
          flex: 1, fontWeight: 600, fontSize: 12, color: '#212529',
          overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
        }}>
          {step.nodeName}
        </span>
        <span style={{ fontSize: 10, color: '#6c757d', flexShrink: 0 }}>{step.nodeType}</span>
        {hasDetails && (
          <i className={`bi bi-chevron-${expanded ? 'up' : 'down'}`} style={{ fontSize: 10, color: '#adb5bd', flexShrink: 0 }} />
        )}
      </div>

      {expanded && hasDetails && (
        <div style={{ padding: '0 10px 8px', borderTop: '1px solid #f0f0f0' }}>
          {step.errorMessage && (
            <div style={{ color: '#ef4444', fontSize: 11, marginBottom: 4, fontWeight: 500 }}>
              {step.errorMessage}
            </div>
          )}
          {isTerminal && (
            <>
              {step.dataInConfigured
                ? <DataBlock label="Data In"  data={step.inputData  ?? {}} />
                : <NoPortsMessage label="Data In" />}
              {step.dataOutConfigured
                ? <DataBlock label="Data Out" data={step.outputData ?? {}} />
                : <NoPortsMessage label="Data Out" />}
            </>
          )}
        </div>
      )}
    </div>
  );
}

function DataBlock({ label, data }) {
  return (
    <div style={{ marginTop: 4 }}>
      <div style={{ fontSize: 10, fontWeight: 600, color: '#6c757d', marginBottom: 2 }}>{label}</div>
      <pre style={{
        background: '#f8f9fa', border: '1px solid #e9ecef', borderRadius: 3,
        padding: '4px 6px', fontSize: 10, margin: 0,
        overflowX: 'auto', maxHeight: 140, overflowY: 'auto',
      }}>
        {JSON.stringify(data, null, 2)}
      </pre>
    </div>
  );
}

function NoPortsMessage({ label }) {
  return (
    <div style={{ marginTop: 4 }}>
      <div style={{ fontSize: 10, fontWeight: 600, color: '#6c757d', marginBottom: 2 }}>{label}</div>
      <div style={{
        background: '#f8f9fa', border: '1px solid #e9ecef', borderRadius: 3,
        padding: '4px 6px', fontSize: 10, color: '#adb5bd', fontStyle: 'italic',
      }}>
        {NO_PORTS_MSG}
      </div>
    </div>
  );
}
