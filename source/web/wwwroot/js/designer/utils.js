// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - UI Utilities
// ???????????????????????????????????????????????????????????????????????????

function switchSidebarTab(tabName) {
  activeSidebarTab = tabName;
    
    // Update tab buttons
    document.querySelectorAll('.sidebar-tab').forEach(tab => {
    tab.classList.remove('active');
    });
    event.target.closest('.sidebar-tab').classList.add('active');
    
    // Update panels
    document.querySelectorAll('.sidebar-panel').forEach(panel => {
        panel.classList.remove('active');
    });
    
 if (tabName === 'nodes') {
        document.getElementById('nodes-panel').classList.add('active');
    } else if (tabName === 'variables') {
     document.getElementById('variables-panel-sidebar').classList.add('active');
    }
}

function zoomIn() {
    zoomLevel = Math.min(zoomLevel + 0.1, 2);
    applyZoom();
}

function zoomOut() {
    zoomLevel = Math.max(zoomLevel - 0.1, 0.5);
    applyZoom();
}

function resetZoom() {
    zoomLevel = 1;
    applyZoom();
}

function applyZoom() {
    const nodesLayer = document.getElementById('nodes-layer');
    const svg = document.getElementById('workflow-canvas');
    
    nodesLayer.style.transform = `scale(${zoomLevel})`;
    nodesLayer.style.transformOrigin = '0 0';
    svg.style.transform = `scale(${zoomLevel})`;
  svg.style.transformOrigin = '0 0';
    
    render();
}

function generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
