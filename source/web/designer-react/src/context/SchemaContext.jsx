import { createContext, useContext } from 'react';

/**
 * Shared designer context.
 * - schemas: all node schemas keyed by node type string.
 * - isRunner: true when the canvas is in read-only runner/playback mode.
 */
export const SchemaContext = createContext({ schemas: {}, isRunner: false });

export function useSchemas() {
  return useContext(SchemaContext).schemas;
}

export function useIsRunner() {
  return useContext(SchemaContext).isRunner;
}
