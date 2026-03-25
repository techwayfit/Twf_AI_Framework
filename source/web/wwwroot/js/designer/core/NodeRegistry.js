/**
 * Central registry for all node types
 * Provides factory methods for creating nodes
 */
class NodeRegistry {
    constructor() {
        /** @type {Map<string, typeof BaseNode>} */
    this.nodeTypes = new Map();
        
        /** @type {Map<string, object>} */
  this.schemas = new Map();
    }

    /**
     * Register a node type
     * @param {string} type - Node type name
     * @param {typeof BaseNode} nodeClass - Node class constructor
     */
    register(type, nodeClass) {
        if (this.nodeTypes.has(type)) {
  console.warn(`Node type '${type}' is already registered. Overwriting.`);
    }
        this.nodeTypes.set(type, nodeClass);
    }

    /**
     * Create a node instance
     * @param {string} type - Node type
 * @param {string} name - Node name
     * @param {number} x - X position
     * @param {number} y - Y position
     * @returns {BaseNode}
     */
    createNode(type, name, x = 0, y = 0) {
        const NodeClass = this.nodeTypes.get(type);
        if (!NodeClass) {
            throw new Error(`Unknown node type: ${type}`);
        }
 
        const id = generateGuid();
    const node = new NodeClass(id, name);
    node.position = { x, y };
        return node;
    }

    /**
     * Get all registered node types grouped by category
     * @returns {Map<string, Array<{type: string, name: string, description: string, color: string}>>}
     */
    getNodesByCategory() {
    const grouped = new Map();
        
for (const [type, NodeClass] of this.nodeTypes) {
const instance = new NodeClass(generateGuid(), 'temp');
        const category = instance.category;
    
            if (!grouped.has(category)) {
       grouped.set(category, []);
            }

            const schema = instance.getSchema();
            grouped.get(category).push({
        type: type,
    name: instance.name,
        description: schema.description || '',
      color: instance.color
   });
        }
        
        return grouped;
    }

/**
     * Load schemas from server (stores them globally in nodeSchemas)
     * @returns {Promise<void>}
  */
    async loadSchemas() {
        try {
            const response = await fetch('/Workflow/GetAllNodeSchemas');
    const schemas = await response.json();
      
// Store in global nodeSchemas variable for backward compatibility
            window.nodeSchemas = schemas;
      
         for (const [type, schema] of Object.entries(schemas)) {
         this.schemas.set(type, schema);
            }
        } catch (error) {
    console.error('Error loading node schemas:', error);
      }
    }

    /**
     * Get schema for a node type
     * @param {string} type
 * @returns {object}
     */
    getSchema(type) {
   return this.schemas.get(type) || nodeSchemas[type] || {};
    }
}

// Global singleton instance
const nodeRegistry = new NodeRegistry();
