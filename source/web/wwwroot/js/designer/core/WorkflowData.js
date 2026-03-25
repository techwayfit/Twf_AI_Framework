/**
 * WorkflowData model representing the entire workflow
 */
class WorkflowData {
    /**
     * @param {string} id - Workflow ID
     * @param {string} name - Workflow name
     * @param {string} description - Workflow description
     */
    constructor(id, name, description = '') {
        this.id = id;
      this.name = name;
        this.description = description;
        this.nodes = [];
        this.connections = [];
        this.variables = {};
    }

  /**
     * Find a node by ID
     * @param {string} nodeId
     * @returns {BaseNode|null}
     */
    findNode(nodeId) {
        return this.nodes.find(n => n.id === nodeId) || null;
    }

    /**
   * Add a node to the workflow
  * @param {BaseNode} node
     */
 addNode(node) {
      this.nodes.push(node);
    }

    /**
     * Remove a node from the workflow
     * @param {string} nodeId
     */
    removeNode(nodeId) {
        this.nodes = this.nodes.filter(n => n.id !== nodeId);
        this.connections = this.connections.filter(
      c => c.sourceNodeId !== nodeId && c.targetNodeId !== nodeId
     );
    }

    /**
     * Add a connection
     * @param {object} connection
     */
    addConnection(connection) {
        this.connections.push(connection);
    }

    /**
     * Remove a connection
     * @param {string} connectionId
     */
    removeConnection(connectionId) {
     this.connections = this.connections.filter(c => c.id !== connectionId);
 }

    /**
     * Serialize to JSON
     * @returns {object}
     */
    toJSON() {
        return {
            id: this.id,
            name: this.name,
      description: this.description,
            nodes: this.nodes.map(n => n.toJSON()),
            connections: this.connections,
variables: this.variables
        };
    }

    /**
     * Deserialize from JSON
     * @param {object} json
     * @param {NodeRegistry} registry
     * @returns {WorkflowData}
     */
    static fromJSON(json, registry) {
        const workflow = new WorkflowData(json.id, json.name, json.description);
        workflow.variables = json.variables || {};
        workflow.connections = json.connections || [];

        // Deserialize nodes using registry
        if (json.nodes) {
   json.nodes.forEach(nodeJson => {
            const NodeClass = registry.nodeTypes.get(nodeJson.type);
         if (NodeClass) {
      const node = NodeClass.fromJSON(nodeJson);
          workflow.nodes.push(node);
        } else {
              console.warn(`Unknown node type: ${nodeJson.type}`);
             }
     });
        }

        return workflow;
    }
}
