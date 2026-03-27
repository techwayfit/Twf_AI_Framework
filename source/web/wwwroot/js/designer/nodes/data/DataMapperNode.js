/**
 * Data Mapper Node
 * Explicitly maps source fields/paths to target input keys.
 */
class DataMapperNode extends BaseNode {
    constructor(id, name = 'Data Mapper') {
        super(id, name, 'DataMapperNode', NODE_CATEGORIES.DATA);

        this.parameters = {
            mappings: {},
            defaultValues: {},
            throwOnMissing: false,
            removeUnmapped: false
        };
    }

    static fromJSON(json) {
        const node = new DataMapperNode(json.id, json.name);
        node.parameters = { ...node.parameters, ...json.parameters };
        node.position = json.position || { x: 0, y: 0 };
        node.color = json.color || node.getDefaultColor();
        node.executionOptions = json.executionOptions;
        return node;
    }
}

nodeRegistry.register('DataMapperNode', DataMapperNode);
