// ???????????????????????????????????????????????????????????????????????????
// Workflow Designer - Initialization & Data Loading
// ???????????????????????????????????????????????????????????????????????????

async function initializeDesigner(workflowId, initialSubWorkflowId = null) {
    // Load available node types
    await loadAvailableNodes();
    
    // Load node schemas
    await loadNodeSchemas();
    
  // Load workflow
    await loadWorkflow(workflowId, initialSubWorkflowId);
    
    // Setup event listeners
    setupEventListeners();
    
    // Render everything
    render();
}

async function loadAvailableNodes() {
  try {
        const response = await fetch('/Workflow/GetAvailableNodes');
     availableNodes = await response.json();
        renderNodePalette();
    } catch (error) {
  console.error('Error loading available nodes:', error);
    }
}

async function loadNodeSchemas() {
    try {
    const response = await fetch('/Workflow/GetAllNodeSchemas');
        nodeSchemas = await response.json();
    } catch (error) {
        console.error('Error loading node schemas:', error);
    }
}

async function loadWorkflow(workflowId, initialSubWorkflowId = null) {
    try {
        const response = await fetch(`/Workflow/GetWorkflow/${workflowId}`);
        const loadedWorkflow = await response.json();
        setRootWorkflowData(loadedWorkflow);
        setActiveWorkflowContext('main', null, { syncUrl: false });

        if (initialSubWorkflowId) {
            setActiveWorkflowContext('sub', initialSubWorkflowId, { syncUrl: false });
        }
  
        renderVariablesList();
    } catch (error) {
   console.error('Error loading workflow:', error);
    }
}

async function saveWorkflow() {
    try {
        const workflowToSave = window.rootWorkflow || workflow;
        const response = await fetch('/Workflow/SaveWorkflow', {
          method: 'POST',
     headers: {
        'Content-Type': 'application/json',
          },
      body: JSON.stringify(workflowToSave)
        });
     
      const result = await response.json();
     
        if (result.success) {
 // Show success message
            const toolbar = document.getElementById('toolbar-buttons');
       const msg = document.createElement('span');
            msg.style.color = '#27ae60';
            msg.style.fontWeight = 'bold';
  msg.innerHTML = '<i class="bi bi-check-circle-fill"></i> Saved!';
     toolbar.appendChild(msg);
       setTimeout(() => msg.remove(), 2000);
    } else {
   alert('Error saving workflow: ' + result.error);
        }
    } catch (error) {
        console.error('Error saving workflow:', error);
        alert('Error saving workflow: ' + error.message);
    }
}
