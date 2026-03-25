/**
 * Output Parser Node
 * Parses and extracts structured data from LLM outputs
 */
class OutputParserNode extends BaseNode {
    constructor(id, name = 'Output Parser') {
    super(id, name, 'OutputParserNode', NODE_CATEGORIES.AI);
        
 this.parameters = {
   fieldMapping: {},
  strictMode: false
     };
    }

    static fromJSON(json) {
        const node = new OutputParserNode(json.id, json.name);
     node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
     node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
      return node;
    }
}

nodeRegistry.register('OutputParserNode', OutputParserNode);
