/**
 * WorkflowTemplates - Pre-built workflow templates
 * Phase 3 Task 3.7: Sample workflows demonstrating ConditionNode usage
 * 
 * Provides ready-to-use workflow templates for common scenarios:
 * - Sentiment-based routing
 * - Priority escalation
 * - Content categorization
 */
class WorkflowTemplates {
    /**
     * Get all available templates
     * @returns {Array<{id, name, description, category, nodeCount, thumbnail}>}
     */
    static getAllTemplates() {
        return [
         {
       id: 'sentiment_routing',
            name: 'Sentiment-Based Routing',
       description: 'Route messages based on sentiment analysis (positive/negative/neutral)',
           category: 'AI & Analytics',
     nodeCount: 5,
    thumbnail: '??'
      },
   {
    id: 'priority_escalation',
       name: 'Priority Escalation',
    description: 'Escalate high-priority items to different channels',
        category: 'Business Logic',
      nodeCount: 4,
            thumbnail: '?'
            },
      {
   id: 'content_categorization',
    name: 'Content Categorization',
                description: 'Categorize and route content based on topic detection',
     category: 'AI & Analytics',
        nodeCount: 6,
     thumbnail: '??'
  }
        ];
    }

    /**
     * Get template by ID
     * @param {string} templateId
     * @returns {Object} Workflow definition
  */
    static getTemplate(templateId) {
        const templates = {
            sentiment_routing: this._createSentimentRoutingTemplate(),
  priority_escalation: this._createPriorityEscalationTemplate(),
            content_categorization: this._createContentCategorizationTemplate()
        };

        return templates[templateId] || null;
    }

    /**
     * Apply template to current workflow
 * @param {string} templateId
 * @param {Object} currentWorkflow - Current workflow to merge into
     * @param {Object} options - { offsetX, offsetY, clearExisting }
     * @returns {Object} Updated workflow
     */
    static applyTemplate(templateId, currentWorkflow, options = {}) {
        const template = this.getTemplate(templateId);
  if (!template) {
            throw new Error(`Template not found: ${templateId}`);
        }

        const { offsetX = 50, offsetY = 50, clearExisting = false } = options;

        // Clear existing workflow if requested
  if (clearExisting) {
 currentWorkflow.nodes = [];
   currentWorkflow.connections = [];
            currentWorkflow.variables = {};
        }

        // Calculate offset for new nodes
        const existingMaxX = currentWorkflow.nodes.reduce((max, n) => 
    Math.max(max, n.position.x), 0);
        const existingMaxY = currentWorkflow.nodes.reduce((max, n) => 
            Math.max(max, n.position.y), 0);

  const finalOffsetX = clearExisting ? offsetX : existingMaxX + offsetX;
        const finalOffsetY = clearExisting ? offsetY : existingMaxY + offsetY;

        // Generate new GUIDs for nodes and connections
        const nodeIdMap = new Map();
     
        template.nodes.forEach(node => {
            const newId = generateGuid();
   nodeIdMap.set(node.id, newId);

   currentWorkflow.nodes.push({
     ...node,
    id: newId,
      position: {
   x: node.position.x + finalOffsetX,
          y: node.position.y + finalOffsetY
                }
    });
        });

        // Add connections with new IDs
        template.connections.forEach(conn => {
            currentWorkflow.connections.push({
         ...conn,
  id: generateGuid(),
     sourceNodeId: nodeIdMap.get(conn.sourceNodeId),
    targetNodeId: nodeIdMap.get(conn.targetNodeId)
     });
        });

        // Merge variables
  if (template.variables) {
            currentWorkflow.variables = {
             ...currentWorkflow.variables,
   ...template.variables
   };
        }

  return currentWorkflow;
    }

    /**
     * Create sentiment routing template
     * @private
   */
    static _createSentimentRoutingTemplate() {
        return {
            name: 'Sentiment-Based Routing',
            description: 'Analyzes sentiment and routes to appropriate handler',
  variables: {
   'user_input': 'Sample customer feedback text'
    },
          nodes: [
           {
    id: 'llm-sentiment',
      name: 'Sentiment Analysis',
       type: 'LlmNode',
          category: 'AI',
            color: '#9b59b6',
         position: { x: 100, y: 100 },
   parameters: {
   provider: 'openai',
 model: 'gpt-4o',
          systemPrompt: 'Analyze the sentiment of the following text and respond with only one word: positive, negative, or neutral.',
          temperature: 0.3,
          maxTokens: 10
        }
},
      {
      id: 'condition-sentiment',
name: 'Route by Sentiment',
   type: 'ConditionNode',
  category: 'Control',
       color: '#f39c12',
           position: { x: 100, y: 220 },
            parameters: {
     conditions: {
      'is_positive': "sentiment == 'positive'",
              'is_negative': "sentiment == 'negative'",
      'is_neutral': "sentiment == 'neutral'"
             }
             }
       },
      {
id: 'log-positive',
      name: 'Handle Positive',
 type: 'LogNode',
      category: 'Control',
  color: '#27ae60',
          position: { x: 400, y: 100 },
         parameters: {
           label: 'Positive Sentiment',
      logLevel: 'Information'
           }
       },
                {
   id: 'log-negative',
             name: 'Handle Negative',
           type: 'LogNode',
   category: 'Control',
    color: '#e74c3c',
     position: { x: 400, y: 220 },
           parameters: {
          label: 'Negative Sentiment - Alert Support',
         logLevel: 'Warning'
    }
     },
     {
          id: 'log-neutral',
     name: 'Handle Neutral',
   type: 'LogNode',
          category: 'Control',
        color: '#95a5a6',
position: { x: 400, y: 340 },
    parameters: {
     label: 'Neutral Sentiment',
       logLevel: 'Information'
       }
      }
        ],
            connections: [
    {
         id: 'conn-1',
              sourceNodeId: 'llm-sentiment',
 sourcePort: 'output',
          targetNodeId: 'condition-sentiment',
              targetPort: 'input'
      },
       {
          id: 'conn-2',
    sourceNodeId: 'condition-sentiment',
          sourcePort: 'is_positive',
    targetNodeId: 'log-positive',
        targetPort: 'input'
        },
                {
         id: 'conn-3',
           sourceNodeId: 'condition-sentiment',
   sourcePort: 'is_negative',
    targetNodeId: 'log-negative',
  targetPort: 'input'
  },
   {
              id: 'conn-4',
  sourceNodeId: 'condition-sentiment',
    sourcePort: 'is_neutral',
         targetNodeId: 'log-neutral',
        targetPort: 'input'
    }
 ]
  };
    }

    /**
 * Create priority escalation template
     * @private
     */
    static _createPriorityEscalationTemplate() {
        return {
          name: 'Priority Escalation',
        description: 'Routes items based on priority score',
        variables: {
     'priority_score': '8'
  },
   nodes: [
        {
       id: 'transform-priority',
                    name: 'Calculate Priority',
            type: 'TransformNode',
  category: 'Data',
    color: '#3498db',
        position: { x: 100, y: 150 },
      parameters: {
    transformType: 'custom',
     fromKey: 'priority_score',
        toKey: 'priority'
     }
           },
              {
      id: 'condition-priority',
           name: 'Priority Router',
         type: 'ConditionNode',
   category: 'Control',
          color: '#f39c12',
   position: { x: 100, y: 270 },
      parameters: {
          conditions: {
       'is_critical': 'priority >= 9',
         'is_high': 'priority >= 7 && priority < 9',
   'is_medium': 'priority >= 4 && priority < 7'
   }
         }
        },
                {
           id: 'log-critical',
name: 'Critical Handler',
      type: 'LogNode',
     category: 'Control',
     color: '#e74c3c',
       position: { x: 400, y: 100 },
       parameters: {
             label: 'CRITICAL - Immediate Action Required',
    logLevel: 'Error'
        }
    },
            {
             id: 'log-high',
         name: 'High Priority Handler',
    type: 'LogNode',
    category: 'Control',
    color: '#f39c12',
          position: { x: 400, y: 220 },
      parameters: {
         label: 'High Priority',
   logLevel: 'Warning'
      }
        },
   {
         id: 'log-default',
          name: 'Standard Handler',
          type: 'LogNode',
            category: 'Control',
          color: '#27ae60',
         position: { x: 400, y: 340 },
   parameters: {
      label: 'Standard Processing',
            logLevel: 'Information'
        }
     }
            ],
            connections: [
      {
            id: 'conn-1',
         sourceNodeId: 'transform-priority',
           sourcePort: 'output',
      targetNodeId: 'condition-priority',
       targetPort: 'input'
   },
     {
     id: 'conn-2',
     sourceNodeId: 'condition-priority',
       sourcePort: 'is_critical',
        targetNodeId: 'log-critical',
        targetPort: 'input'
          },
   {
         id: 'conn-3',
           sourceNodeId: 'condition-priority',
     sourcePort: 'is_high',
  targetNodeId: 'log-high',
targetPort: 'input'
            },
                {
         id: 'conn-4',
        sourceNodeId: 'condition-priority',
          sourcePort: 'default',
    targetNodeId: 'log-default',
 targetPort: 'input'
            }
      ]
        };
    }

    /**
     * Create content categorization template
     * @private
     */
    static _createContentCategorizationTemplate() {
        return {
     name: 'Content Categorization',
     description: 'Categorizes content and routes to appropriate processor',
      variables: {
      'content': 'Sample article text about technology and innovation'
            },
nodes: [
       {
      id: 'llm-categorize',
        name: 'Categorize Content',
type: 'LlmNode',
        category: 'AI',
         color: '#9b59b6',
           position: { x: 100, y: 100 },
          parameters: {
         provider: 'openai',
 model: 'gpt-4o',
               systemPrompt: 'Categorize the following content into one category: technology, business, health, entertainment, or other.',
           temperature: 0.2,
         maxTokens: 10
           }
     },
             {
       id: 'condition-category',
 name: 'Route by Category',
           type: 'ConditionNode',
          category: 'Control',
   color: '#f39c12',
 position: { x: 100, y: 220 },
        parameters: {
      conditions: {
    'is_tech': "category == 'technology'",
           'is_business': "category == 'business'",
            'is_health': "category == 'health'"
         }
        }
        },
    {
          id: 'log-tech',
        name: 'Tech Processor',
                    type: 'LogNode',
          category: 'Control',
        color: '#3498db',
   position: { x: 400, y: 80 },
       parameters: {
              label: 'Technology Content',
    logLevel: 'Information'
  }
       },
                {
     id: 'log-business',
                 name: 'Business Processor',
     type: 'LogNode',
         category: 'Control',
            color: '#27ae60',
position: { x: 400, y: 180 },
                  parameters: {
            label: 'Business Content',
               logLevel: 'Information'
                    }
  },
       {
         id: 'log-health',
             name: 'Health Processor',
    type: 'LogNode',
  category: 'Control',
            color: '#e74c3c',
         position: { x: 400, y: 280 },
   parameters: {
          label: 'Health Content',
     logLevel: 'Information'
 }
             },
                {
     id: 'log-other',
 name: 'General Processor',
               type: 'LogNode',
  category: 'Control',
  color: '#95a5a6',
       position: { x: 400, y: 380 },
        parameters: {
      label: 'Other Content',
        logLevel: 'Information'
   }
         }
     ],
     connections: [
                {
     id: 'conn-1',
 sourceNodeId: 'llm-categorize',
        sourcePort: 'output',
      targetNodeId: 'condition-category',
         targetPort: 'input'
         },
      {
     id: 'conn-2',
       sourceNodeId: 'condition-category',
         sourcePort: 'is_tech',
       targetNodeId: 'log-tech',
       targetPort: 'input'
    },
   {
       id: 'conn-3',
      sourceNodeId: 'condition-category',
 sourcePort: 'is_business',
   targetNodeId: 'log-business',
      targetPort: 'input'
        },
       {
    id: 'conn-4',
          sourceNodeId: 'condition-category',
           sourcePort: 'is_health',
         targetNodeId: 'log-health',
           targetPort: 'input'
     },
       {
      id: 'conn-5',
             sourceNodeId: 'condition-category',
              sourcePort: 'default',
           targetNodeId: 'log-other',
      targetPort: 'input'
      }
     ]
      };
    }
}

// Make available globally
window.WorkflowTemplates = WorkflowTemplates;
