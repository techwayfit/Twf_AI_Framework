/**
 * Log Node
 * Logs workflow data at a specific checkpoint
 */
class LogNode extends BaseNode {
    constructor(id, name = 'Log') {
 super(id, name, 'LogNode', NODE_CATEGORIES.CONTROL);
 
this.parameters = {
      label: '',
      keysToLog: '',
      logLevel: 'Information'
   };
    }

    static fromJSON(json) {
   const node = new LogNode(json.id, json.name);
  node.parameters = { ...node.parameters, ...json.parameters };
    node.position = json.position || { x: 0, y: 0 };
 node.color = json.color || node.getDefaultColor();
   node.executionOptions = json.executionOptions;
 return node;
    }
}

nodeRegistry.register('LogNode', LogNode);
