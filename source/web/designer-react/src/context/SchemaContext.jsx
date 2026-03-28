import { createContext, useContext } from 'react';

/**
 * Holds all node schemas keyed by node type string.
 * e.g. { LlmNode: { parameters: [...], inputPorts: [...], ... }, ... }
 */
export const SchemaContext = createContext({});

export function useSchemas() {
  return useContext(SchemaContext);
}
