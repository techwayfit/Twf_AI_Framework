// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Global State
// ???????????????????????????????????????????????????????????????????????????

// Workflow data
let workflow = null;
let rootWorkflow = null;
let workflowContext = { type: 'main', subWorkflowId: null };

// Selection state
let selectedNode = null;
let selectedNodes = new Set();
let selectedVariable = null;
let selectedConnection = null;

// Drag state
let isDraggingNode = false;
let isDraggingSelection = false;
let isDraggingConnection = false;
let draggedConnectionEnd = null; // { connectionId, end: 'source' | 'target' }
let dragOffset = { x: 0, y: 0 };
let groupDragStart = {};

// Selection rectangle state
let isSelecting = false;
let selectionStart = { x: 0, y: 0 };
let selectionRect = null;

// Connection state
let connectingFrom = null;
let isConnecting = false;

// Node palette data
let availableNodes = [];
let nodeSchemas = {};

// Canvas state
let zoomLevel = 1;
let canvasOffset = { x: 0, y: 0 };

// UI state
let activeAutocomplete = null;
let activeSidebarTab = 'nodes';
let hasRegisteredPopStateHandler = false;

function ensureWorkflowCollections(targetWorkflow) {
    if (!targetWorkflow) {
        return;
    }

    if (!Array.isArray(targetWorkflow.nodes)) {
        targetWorkflow.nodes = [];
    }

    if (!Array.isArray(targetWorkflow.connections)) {
        targetWorkflow.connections = [];
    }

    if (!targetWorkflow.variables || typeof targetWorkflow.variables !== 'object') {
        targetWorkflow.variables = {};
    }
}

function ensureRootWorkflowCollections(targetRootWorkflow) {
    ensureWorkflowCollections(targetRootWorkflow);

    if (!Array.isArray(targetRootWorkflow.subWorkflows)) {
        targetRootWorkflow.subWorkflows = [];
    }

    targetRootWorkflow.subWorkflows.forEach(sw => {
        ensureWorkflowCollections(sw);
        if (sw.errorNodeId === undefined) {
            sw.errorNodeId = null;
        }
    });

    if (targetRootWorkflow.errorNodeId === undefined) {
        targetRootWorkflow.errorNodeId = null;
    }
}

function setRootWorkflowData(data) {
    rootWorkflow = data;
    window.rootWorkflow = data;
    ensureRootWorkflowCollections(rootWorkflow);
    if (rootWorkflow?.id) {
        window.workflowId = rootWorkflow.id;
    }

    registerDesignerPopStateHandler();
}

function getRootWorkflow() {
    if (window.rootWorkflow) {
        return window.rootWorkflow;
    }

    if (rootWorkflow) {
        return rootWorkflow;
    }

    return workflow;
}

function getActiveWorkflowContext() {
    return workflowContext || { type: 'main', subWorkflowId: null };
}

function getActiveWorkflowDisplayName() {
    const root = getRootWorkflow();
    if (!root) {
        return '';
    }

    if (workflowContext?.type === 'sub' && workflowContext?.subWorkflowId) {
        const sub = root.subWorkflows?.find(sw => sw.id === workflowContext.subWorkflowId);
        if (sub) {
            return `${root.name} / ${sub.name}`;
        }
    }

    return root.name || 'Workflow';
}

function updateWorkflowTitle() {
    const titleEl = document.getElementById('workflow-name');
    if (titleEl) {
        titleEl.textContent = getActiveWorkflowDisplayName();
    }
}

function getMainWorkflowId() {
    const root = getRootWorkflow();
    return root?.id || window.workflowId || null;
}

function buildMainWorkflowUrl() {
    const mainWorkflowId = getMainWorkflowId();
    return mainWorkflowId ? `/Workflow/Designer/${mainWorkflowId}` : window.location.pathname;
}

function buildSubWorkflowUrl(subWorkflowId) {
    const mainWorkflowId = getMainWorkflowId();
    if (!mainWorkflowId || !subWorkflowId) {
        return buildMainWorkflowUrl();
    }

    return `/${mainWorkflowId}/${subWorkflowId}`;
}

function updateHeaderNavigation() {
    const backMainHeaderButton = document.getElementById('header-back-main-workflow');
    if (!backMainHeaderButton) {
        return;
    }

    if (workflowContext?.type === 'sub' && workflowContext?.subWorkflowId) {
        backMainHeaderButton.style.display = '';
    } else {
        backMainHeaderButton.style.display = 'none';
    }
}

function syncDesignerUrl(replaceHistory = false) {
    if (!window.history || typeof window.history.pushState !== 'function') {
        return;
    }

    const isSubWorkflowContext = workflowContext?.type === 'sub' && workflowContext?.subWorkflowId;
    const nextUrl = isSubWorkflowContext
        ? buildSubWorkflowUrl(workflowContext.subWorkflowId)
        : buildMainWorkflowUrl();

    const currentPathAndQuery = `${window.location.pathname}${window.location.search}`;
    if (nextUrl === currentPathAndQuery) {
        return;
    }

    const state = {
        designerContext: true,
        type: workflowContext?.type || 'main',
        subWorkflowId: workflowContext?.subWorkflowId || null
    };

    if (replaceHistory) {
        window.history.replaceState(state, '', nextUrl);
    } else {
        window.history.pushState(state, '', nextUrl);
    }
}

function isGuid(value) {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value || '');
}

function parseDesignerContextFromUrl() {
    const segments = window.location.pathname.split('/').filter(Boolean);

    if (segments.length === 2 && isGuid(segments[0]) && isGuid(segments[1])) {
        return { type: 'sub', subWorkflowId: segments[1] };
    }

    if (segments.length >= 3 &&
        segments[0].toLowerCase() === 'workflow' &&
        segments[1].toLowerCase() === 'designer' &&
        isGuid(segments[2])) {
        return { type: 'main', subWorkflowId: null };
    }

    return null;
}

function registerDesignerPopStateHandler() {
    if (hasRegisteredPopStateHandler) {
        return;
    }

    window.addEventListener('popstate', () => {
        const parsed = parseDesignerContextFromUrl();
        if (!parsed) {
            return;
        }

        if (parsed.type === 'sub' && parsed.subWorkflowId) {
            setActiveWorkflowContext('sub', parsed.subWorkflowId, { syncUrl: false });
            return;
        }

        setActiveWorkflowContext('main', null, { syncUrl: false });
    });

    hasRegisteredPopStateHandler = true;
}

function setActiveWorkflowContext(type = 'main', subWorkflowId = null, options = {}) {
    const { syncUrl = true, replaceHistory = false } = options;
    const root = getRootWorkflow();
    if (!root) {
        return;
    }

    let activeWorkflow = root;
    let resolvedSubWorkflowId = null;

    if (type === 'sub') {
        const sub = root.subWorkflows?.find(sw => sw.id === subWorkflowId);
        if (!sub) {
            console.warn(`Sub workflow ${subWorkflowId} not found. Falling back to main workflow.`);
        } else {
            activeWorkflow = sub;
            resolvedSubWorkflowId = sub.id;
        }
    }

    ensureWorkflowCollections(activeWorkflow);
    workflow = activeWorkflow;
    window.workflow = activeWorkflow;
    workflowContext = { type: resolvedSubWorkflowId ? 'sub' : 'main', subWorkflowId: resolvedSubWorkflowId };
    window.workflowContext = workflowContext;

    document.title = `Workflow Designer - ${getActiveWorkflowDisplayName()}`;

    if (typeof window.designerInstance?.convertWorkflowNodesToClasses === 'function') {
        window.designerInstance.convertWorkflowNodesToClasses();
    }

    selectedVariable = null;
    selectedConnection = null;
    selectedNode = null;
    selectedNodes.clear();

    if (typeof render === 'function') {
        render();
    }

    if (typeof renderVariablesList === 'function') {
        renderVariablesList();
    }

    const panel = document.getElementById('properties-content');
    if (panel) {
        panel.innerHTML = '<p class="text-muted small">Select a node or variable to edit.</p>';
    }

    if (syncUrl) {
        syncDesignerUrl(replaceHistory);
    }

    updateWorkflowTitle();
    updateHeaderNavigation();
}

function openMainWorkflowDesigner() {
    setActiveWorkflowContext('main', null, { syncUrl: true });
}

function openSubWorkflowDesigner(subWorkflowId) {
    setActiveWorkflowContext('sub', subWorkflowId, { syncUrl: true });
}
