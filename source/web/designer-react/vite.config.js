import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  // Replace Node.js globals that React relies on (process.env.NODE_ENV)
  define: {
    'process.env.NODE_ENV': JSON.stringify('production'),
  },
  build: {
    // lib mode: no index.html needed, produces a self-contained bundle
    lib: {
      entry: 'src/main.jsx',
      formats: ['iife'],   // IIFE = works with a plain <script> tag, no module system needed
      name: 'DesignerApp', // global name (not really used since main.jsx doesn't export)
      fileName: () => 'designer.js',
    },
    outDir: '../wwwroot/js/designer-app',
    emptyOutDir: true,
    rollupOptions: {
      output: {
        // Force CSS output name to "designer.css"
        assetFileNames: 'designer[extname]',
      },
    },
  },
});
