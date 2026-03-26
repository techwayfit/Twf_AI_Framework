/**
 * ConnectionValidator - Validates workflow connections
 * Phase 3 Task 3.6: Connection validation for conditional ports
 * 
 * Ensures connections remain valid when:
 * - Conditions are added/removed from ConditionNode
 * - Ports are dynamically created/destroyed
 * - Node types change
 */
class ConnectionValidator {
    /**
     * Validate all connections in workflow
     * @param {Object} workflow - Workflow definition
* @param {Object} nodeSchemas - Node type schemas
     * @returns {{ valid: boolean, errors: Array<{connectionId, message}>, warnings: Array }}
     */
    static validateWorkflow(workflow, nodeSchemas) {
        const errors = [];
        const warnings = [];

        if (!workflow.connections || workflow.connections.length === 0) {
 return { valid: true, errors: [], warnings: [] };
        }

        workflow.connections.forEach(conn => {
 const result = this.validateConnection(conn, workflow, nodeSchemas);
            
         if (!result.valid) {
       errors.push({
        connectionId: conn.id,
           message: result.error
    });
        }

            if (result.warnings) {
             result.warnings.forEach(warning => {
      warnings.push({
          connectionId: conn.id,
    message: warning
       });
             });
   }
        });

        return {
     valid: errors.length === 0,
       errors,
     warnings
        };
    }

  /**
     * Validate a single connection
     * @param {Object} connection - Connection to validate
     * @param {Object} workflow - Workflow definition
     * @param {Object} nodeSchemas - Node type schemas
     * @returns {{ valid: boolean, error?: string, warnings?: Array<string> }}
     */
  static validateConnection(connection, workflow, nodeSchemas) {
        const warnings = [];

        // Check if source node exists
        const sourceNode = workflow.nodes.find(n => n.id === connection.sourceNodeId);
        if (!sourceNode) {
            return { valid: false, error: 'Source node not found' };
   }

        // Check if target node exists
        const targetNode = workflow.nodes.find(n => n.id === connection.targetNodeId);
        if (!targetNode) {
 return { valid: false, error: 'Target node not found' };
   }

     // Get node schemas
        const sourceSchema = nodeSchemas[sourceNode.type];
  const targetSchema = nodeSchemas[targetNode.type];

        if (!sourceSchema) {
            warnings.push(`No schema found for source node type: ${sourceNode.type}`);
        }

        if (!targetSchema) {
warnings.push(`No schema found for target node type: ${targetNode.type}`);
        }

        // Validate source port exists
        const sourcePortValid = this.validatePort(
          sourceNode,
            sourceSchema,
   connection.sourcePort,
            'output'
        );

        if (!sourcePortValid.valid) {
            return { valid: false, error: `Source port invalid: ${sourcePortValid.error}` };
      }

        // Validate target port exists
        const targetPortValid = this.validatePort(
     targetNode,
          targetSchema,
            connection.targetPort,
          'input'
        );

   if (!targetPortValid.valid) {
            return { valid: false, error: `Target port invalid: ${targetPortValid.error}` };
      }

        // Check for self-connection
        if (connection.sourceNodeId === connection.targetNodeId) {
        return { valid: false, error: 'Node cannot connect to itself' };
        }

        return { valid: true, warnings: warnings.length > 0 ? warnings : undefined };
    }

    /**
* Validate that a port exists on a node
     * @param {Object} node - Node instance
     * @param {Object} schema - Node schema
     * @param {string} portId - Port identifier
     * @param {string} direction - 'input' or 'output'
     * @returns {{ valid: boolean, error?: string }}
     */
    static validatePort(node, schema, portId, direction) {
        if (!schema) {
 // Without schema, assume legacy single ports (input/output)
            const validLegacyPorts = direction === 'input' ? ['input'] : ['output'];
       if (!validLegacyPorts.includes(portId)) {
      return { valid: false, error: `Port '${portId}' not found (legacy mode)` };
    }
         return { valid: true };
    }

 // Get port definitions from schema
        const ports = direction === 'input' ? schema.inputPorts : schema.outputPorts;

        if (!ports || ports.length === 0) {
            return { valid: false, error: `No ${direction} ports defined in schema` };
        }

        // Check for dynamic ports (e.g., ConditionNode)
      if (schema.capabilities?.supportsDynamicPorts) {
    return this.validateDynamicPort(node, portId, direction);
        }

        // Check if port exists in schema
     const portExists = ports.some(p => p.id === portId);

 if (!portExists) {
            return { valid: false, error: `Port '${portId}' not found in ${direction} ports` };
   }

        return { valid: true };
    }

    /**
     * Validate dynamic port (e.g., conditional output from ConditionNode)
     * @param {Object} node - Node instance
  * @param {string} portId - Port identifier
  * @param {string} direction - 'input' or 'output'
  * @returns {{ valid: boolean, error?: string }}
     */
    static validateDynamicPort(node, portId, direction) {
        // For ConditionNode, check if port matches a condition or is 'default'
        if (node.type === 'ConditionNode' && direction === 'output') {
            const conditions = node.parameters?.conditions || {};
  const validPorts = [...Object.keys(conditions), 'default'];

            if (!validPorts.includes(portId)) {
    return {
    valid: false,
      error: `Conditional port '${portId}' not found. Available: ${validPorts.join(', ')}`
         };
            }

            return { valid: true };
}

  // Default validation for other dynamic port types
        return { valid: true };
    }

    /**
     * Find invalid connections that should be removed
* @param {Object} workflow - Workflow definition
     * @param {Object} nodeSchemas - Node type schemas
     * @returns {Array<string>} Array of connection IDs to remove
     */
    static findInvalidConnections(workflow, nodeSchemas) {
        const validation = this.validateWorkflow(workflow, nodeSchemas);
      return validation.errors.map(e => e.connectionId);
    }

    /**
     * Clean up invalid connections from workflow
     * @param {Object} workflow - Workflow definition
     * @param {Object} nodeSchemas - Node type schemas
     * @returns {{ removed: number, connectionIds: Array<string> }}
     */
    static cleanupInvalidConnections(workflow, nodeSchemas) {
        const invalidIds = this.findInvalidConnections(workflow, nodeSchemas);

    if (invalidIds.length === 0) {
            return { removed: 0, connectionIds: [] };
 }

        workflow.connections = workflow.connections.filter(
         conn => !invalidIds.includes(conn.id)
        );

     return {
            removed: invalidIds.length,
connectionIds: invalidIds
  };
    }

    /**
     * Get connections affected by condition changes
     * @param {string} nodeId - ConditionNode ID
     * @param {Array<string>} removedConditions - Condition names that were removed
     * @param {Object} workflow - Workflow definition
     * @returns {Array<Object>} Affected connections
  */
    static getAffectedConnectionsByConditionChange(nodeId, removedConditions, workflow) {
        if (!removedConditions || removedConditions.length === 0) {
        return [];
        }

    return workflow.connections.filter(conn => {
      // Check if connection is from this node and uses a removed condition port
         return conn.sourceNodeId === nodeId && removedConditions.includes(conn.sourcePort);
        });
    }

    /**
   * Show user-friendly error messages for invalid connections
     * @param {Array} errors - Validation errors
     * @returns {string} Formatted error message
     */
    static formatErrors(errors) {
     if (!errors || errors.length === 0) {
         return '';
        }

        const messages = errors.map((err, index) => {
     return `${index + 1}. Connection issue: ${err.message}`;
        });

        return messages.join('\n');
 }
}

// Make available globally
window.ConnectionValidator = ConnectionValidator;
