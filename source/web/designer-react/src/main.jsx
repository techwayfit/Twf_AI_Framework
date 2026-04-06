import { createRoot } from 'react-dom/client';
import App from './App';

const config = window.__DESIGNER_CONFIG__ ?? {};

const root = createRoot(document.getElementById('designer-root'));
root.render(<App workflowId={config.workflowId} mode={config.mode ?? 'designer'} />);
