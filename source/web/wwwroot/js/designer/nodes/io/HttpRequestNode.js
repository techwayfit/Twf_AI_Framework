/**
 * HTTP Request Node
 * Makes HTTP requests to external APIs
 */
class HttpRequestNode extends BaseNode {
    constructor(id, name = 'HTTP Request') {
        super(id, name, 'HttpRequestNode', NODE_CATEGORIES.IO);
  
     this.parameters = {
   method: 'GET',
      urlTemplate: '',
      headers: {},
            body: '',
  timeout: 30,
            throwOnError: true
  };
    }

    static fromJSON(json) {
  const node = new HttpRequestNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
node.color = json.color || node.getDefaultColor();
   node.executionOptions = json.executionOptions;
   return node;
 }
}

nodeRegistry.register('HttpRequestNode', HttpRequestNode);
