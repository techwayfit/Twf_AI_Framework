// API wrappers for the workflow designer

export async function loadWorkflow(id) {
  const res = await fetch(`/Workflow/GetWorkflow/${id}`);
  if (!res.ok) throw new Error(`Failed to load workflow: ${res.statusText}`);
  return res.json();
}

export async function saveWorkflow(workflowDefinition) {
  const res = await fetch('/Workflow/SaveWorkflow', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(workflowDefinition),
  });
  if (!res.ok) throw new Error(`Save failed: ${res.statusText}`);
  return res.json();
}

export async function loadAvailableNodes() {
  const res = await fetch('/Workflow/GetAvailableNodes');
  if (!res.ok) throw new Error('Failed to load available nodes');
  return res.json();
}

export async function loadAllSchemas() {
  const res = await fetch('/Workflow/GetAllNodeSchemas');
  if (!res.ok) throw new Error('Failed to load node schemas');
  return res.json();
}
