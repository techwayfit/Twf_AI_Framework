/**
 * runner.js  —  Workflow Runner View client logic
 *
 * Responsibilities:
 *  - Render the workflow as a read-only canvas (reuses the designer's render() pipeline)
 *  - Manage the input modal (collect initial data key-value pairs)
 *  - Stream SSE events from POST /Workflow/RunStream/{id}
 *  - Highlight nodes as they execute (running / done / error)
 *  - Populate the Execution Log step panel with per-node input/output snapshots
 */

'use strict';

// ─── State ────────────────────────────────────────────────────────────────────

let _workflowId  = null;
let _workflowDef = null;

/** AbortController for the current fetch stream (allows Stop). */
let _abortController = null;

/** Maps nodeId → current runner CSS state class suffix */
let _nodeStates = {};

// ─── Initialisation ───────────────────────────────────────────────────────────

/**
 * Entry point called from Runner.cshtml after DOMContentLoaded.
 * @param {string} workflowId
 * @param {object} workflowDef  — server-serialised WorkflowDefinition
 */
function initRunner(workflowId, workflowDef) {
    _workflowId  = workflowId;
    _workflowDef = workflowDef;

    // Load schemas first, then wire state so the first render has correct ports.
    _initCanvas();
}

async function _initCanvas() {
    try {
        const resp = await fetch('/Workflow/GetAllNodeSchemas');
        if (resp.ok) {
            window.nodeSchemas = await resp.json();
            // nodeSchemas is a `let` in state.js — patch it the same way
            if (typeof nodeSchemas !== 'undefined') {
                // eslint-disable-next-line no-global-assign
                nodeSchemas = window.nodeSchemas;
            }
        }
    } catch (e) {
        console.warn('Runner: could not load node schemas, falling back to legacy renderer.', e);
    }

    // Use the same state functions initialization.js uses so the `let workflow`
    // variable in state.js is set correctly (window.workflow alone is not enough).
    setRootWorkflowData(_workflowDef);
    setActiveWorkflowContext('main', null, { syncUrl: false });
    // setActiveWorkflowContext already calls render() at the end — no extra call needed.

    // Mark all nodes as pending.
    document.querySelectorAll('.workflow-node').forEach(el => {
        el.classList.add('node-runner-pending');
    });
}

// ─── Input Modal ──────────────────────────────────────────────────────────────

function showInputModal() {
    document.getElementById('input-modal').style.display = 'flex';
}

function closeInputModal() {
    document.getElementById('input-modal').style.display = 'none';
}

function onModalOverlayClick(event) {
    // Close when clicking the dark backdrop (not the box itself).
    if (event.target === document.getElementById('input-modal')) {
        closeInputModal();
    }
}

function addInputField() {
    const row = document.createElement('div');
    row.className = 'input-row';
    row.innerHTML =
        '<input type="text" placeholder="key"   class="form-control form-control-sm input-key" />' +
        '<input type="text" placeholder="value" class="form-control form-control-sm input-value" />' +
        '<button class="btn btn-sm btn-outline-danger px-2" onclick="this.closest(\'.input-row\').remove()" title="Remove">' +
        '<i class="bi bi-x"></i></button>';
    document.getElementById('input-fields').appendChild(row);
}

function _collectInputs() {
    const inputs = {};
    document.querySelectorAll('#input-fields .input-row').forEach(row => {
        const key = row.querySelector('.input-key').value.trim();
        const val = row.querySelector('.input-value').value;
        if (key) inputs[key] = val;
    });
    return inputs;
}

// ─── Run execution ────────────────────────────────────────────────────────────

function startRun() {
    closeInputModal();
    _resetRunnerState();

    const initialData = _collectInputs();

    _abortController = new AbortController();

    document.getElementById('btn-run').disabled  = true;
    document.getElementById('btn-stop').disabled = false;
    _updateStatusBadge('running');

    // Mark all nodes as pending so the canvas dims everything.
    document.getElementById('canvas-area').classList.add('runner-active');
    document.querySelectorAll('.workflow-node').forEach(el => {
        el.classList.remove('node-runner-done', 'node-runner-error', 'node-runner-running');
        el.classList.add('node-runner-pending');
    });

    fetch(`/Workflow/RunStream/${_workflowId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ initialData }),
        signal: _abortController.signal
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(t => { throw new Error(`HTTP ${response.status}: ${t}`); });
        }
        return _readSSEStream(response.body.getReader());
    })
    .catch(err => {
        if (err.name === 'AbortError') {
            _updateStatusBadge('stopped');
        } else {
            console.error('Runner stream error:', err);
            _updateStatusBadge('error');
            _appendErrorCard('Stream error: ' + err.message);
        }
        _onRunFinished();
    });
}

function stopRun() {
    if (_abortController) {
        _abortController.abort();
        _abortController = null;
    }
    _updateStatusBadge('stopped');
    _onRunFinished();
}

function _onRunFinished() {
    document.getElementById('btn-run').disabled  = false;
    document.getElementById('btn-stop').disabled = true;
    document.getElementById('canvas-area').classList.remove('runner-active');
}

// ─── SSE stream reader ────────────────────────────────────────────────────────

async function _readSSEStream(reader) {
    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });

        // SSE events are delimited by double newlines.
        const events = buffer.split('\n\n');
        buffer = events.pop(); // keep incomplete trailing chunk

        for (const chunk of events) {
            _parseAndDispatch(chunk);
        }
    }

    // Process any remaining buffered data.
    if (buffer.trim()) _parseAndDispatch(buffer);
}

function _parseAndDispatch(chunk) {
    let eventType = '';
    let data = '';

    for (const line of chunk.split('\n')) {
        if (line.startsWith('event: ')) eventType = line.slice(7).trim();
        else if (line.startsWith('data: ')) data = line.slice(6).trim();
    }

    if (!eventType || !data) return;

    let payload;
    try {
        payload = JSON.parse(data);
    } catch (e) {
        console.warn('Runner: failed to parse SSE payload', data, e);
        return;
    }

    _handleEvent(eventType, payload);
}

function _handleEvent(eventType, payload) {
    switch (eventType) {
        case 'node_start':
            _setNodeState(payload.nodeId, 'running');
            _addStepCard(payload, 'running');
            break;

        case 'node_done':
            _setNodeState(payload.nodeId, 'done');
            _updateStepCard(payload.nodeId, 'done', payload);
            break;

        case 'node_error':
            _setNodeState(payload.nodeId, 'error');
            _updateStepCard(payload.nodeId, 'error', payload);
            break;

        case 'workflow_done':
            _updateStatusBadge('done');
            _onRunFinished();
            break;

        case 'workflow_error':
            _updateStatusBadge('error');
            if (payload.errorMessage) {
                _appendErrorCard('Workflow failed: ' + payload.errorMessage);
            }
            _onRunFinished();
            break;

        default:
            console.log('Runner: unhandled event type', eventType, payload);
    }
}

// ─── Node highlighting ────────────────────────────────────────────────────────

function _setNodeState(nodeId, state) {
    _nodeStates[nodeId] = state;

    const el = document.querySelector(`[data-node-id="${nodeId}"]`);
    if (!el) return;

    el.classList.remove(
        'node-runner-pending',
        'node-runner-running',
        'node-runner-done',
        'node-runner-error'
    );
    el.classList.add(`node-runner-${state}`);

    if (state === 'running') {
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}

// ─── Step panel cards ─────────────────────────────────────────────────────────

function _addStepCard(step, state) {
    const cardId = `step-card-${step.nodeId}`;

    // Remove duplicate if re-running a previously-seen node (e.g. loop iteration).
    const existing = document.getElementById(cardId);
    if (existing) existing.remove();

    const card = document.createElement('div');
    card.className = `step-card step-${state}`;
    card.id = cardId;

    const inputJson = _safeStringify(step.inputData);

    card.innerHTML =
        `<div class="step-header">` +
            `<span class="step-icon">${_stateIcon(state)}</span>` +
            `<span class="step-name">${_escapeHtml(step.nodeName)}</span>` +
            `<span class="step-type">${_escapeHtml(step.nodeType)}</span>` +
            `<span class="step-time">${_formatTime(step.timestamp)}</span>` +
            `<span class="step-chevron"><i class="bi bi-chevron-down"></i></span>` +
        `</div>` +
        `<div class="step-body">` +
            `<div class="step-section">` +
                `<span class="step-section-label">Input</span>` +
                `<pre>${_escapeHtml(inputJson)}</pre>` +
            `</div>` +
            `<div class="step-section" id="step-output-section-${step.nodeId}">` +
                `<span class="step-section-label">Output</span>` +
                `<pre id="step-output-${step.nodeId}">(waiting…)</pre>` +
            `</div>` +
        `</div>`;

    // Toggle body on header click.
    card.querySelector('.step-header').addEventListener('click', () => {
        const body = card.querySelector('.step-body');
        body.classList.toggle('expanded');
        const chevron = card.querySelector('.step-chevron i');
        chevron.className = body.classList.contains('expanded')
            ? 'bi bi-chevron-up'
            : 'bi bi-chevron-down';
    });

    document.getElementById('step-cards').appendChild(card);
    card.scrollIntoView({ behavior: 'smooth', block: 'end' });
}

function _updateStepCard(nodeId, state, step) {
    const card = document.getElementById(`step-card-${nodeId}`);
    if (!card) return;

    card.className = `step-card step-${state}`;
    card.querySelector('.step-icon').textContent = _stateIcon(state);

    const outputEl = document.getElementById(`step-output-${nodeId}`);
    if (outputEl) {
        if (step.errorMessage) {
            outputEl.innerHTML =
                `<span class="step-error-msg">&#10060; ${_escapeHtml(step.errorMessage)}</span>`;
        } else {
            outputEl.textContent = _safeStringify(step.outputData);
        }
    }

    // Auto-expand card on error so the message is immediately visible.
    if (state === 'error') {
        const body = card.querySelector('.step-body');
        if (body && !body.classList.contains('expanded')) {
            body.classList.add('expanded');
            const chevron = card.querySelector('.step-chevron i');
            if (chevron) chevron.className = 'bi bi-chevron-up';
        }
    }
}

function _appendErrorCard(message) {
    const card = document.createElement('div');
    card.className = 'step-card step-error';
    card.innerHTML =
        `<div class="step-header">` +
            `<span class="step-icon">❌</span>` +
            `<span class="step-name step-error-msg">${_escapeHtml(message)}</span>` +
        `</div>`;
    document.getElementById('step-cards').appendChild(card);
    card.scrollIntoView({ behavior: 'smooth', block: 'end' });
}

// ─── UI helpers ───────────────────────────────────────────────────────────────

function _updateStatusBadge(state) {
    const badge = document.getElementById('run-status-badge');
    const map = {
        running: ['bg-warning text-dark', 'Running…'],
        done:    ['bg-success',           'Completed'],
        error:   ['bg-danger',            'Failed'],
        stopped: ['bg-dark',              'Stopped'],
    };
    const [cls, label] = map[state] || ['bg-secondary', 'Ready'];
    badge.className = `badge ${cls}`;
    badge.textContent = label;
}

function _stateIcon(state) {
    return { running: '⏳', done: '✅', error: '❌', pending: '○' }[state] ?? '○';
}

function _formatTime(isoString) {
    try {
        return new Date(isoString).toLocaleTimeString();
    } catch {
        return '';
    }
}

function _safeStringify(obj) {
    try {
        return JSON.stringify(obj, null, 2);
    } catch {
        return String(obj);
    }
}

function _escapeHtml(str) {
    if (str == null) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

// ─── Log management ───────────────────────────────────────────────────────────

function clearLog() {
    document.getElementById('step-cards').innerHTML = '';
    _nodeStates = {};
    // Reset node visual states.
    document.querySelectorAll('.workflow-node').forEach(el => {
        el.classList.remove(
            'node-runner-pending',
            'node-runner-running',
            'node-runner-done',
            'node-runner-error'
        );
    });
    _updateStatusBadge('ready');
}

function _resetRunnerState() {
    clearLog();
}
