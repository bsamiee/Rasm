import { cp } from 'node:fs/promises';
import { resolve } from 'node:path';
import { defineConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const syntax = resolve(import.meta.dirname, 'node_modules/sh-syntax');
const staged = { 'hooks/main.wasm': resolve(syntax, 'main.wasm'), 'hooks/hooks.json': resolve(import.meta.dirname, 'hooks/hooks.json'), '.claude-plugin/plugin.json': resolve(import.meta.dirname, '.claude-plugin/plugin.json') };

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    root: import.meta.dirname,
    resolve: {
        alias: [
            { find: /^sh-syntax$/u, replacement: resolve(syntax, 'lib/common.js') },
            { find: /^sh-syntax\/sync$/u, replacement: resolve(syntax, 'lib/sync/processor.js') },
            { find: /^sh-syntax\/vendors\/wasm_exec$/u, replacement: resolve(syntax, 'vendors/wasm_exec.cjs') },
        ],
    },
    plugins: [{ name: 'staged', writeBundle: ({ dir = '' }) => Promise.all(Object.entries(staged).map(([to, from]) => cp(from, resolve(dir, to)))).then(() => undefined) }],
    build: {
        lib: { entry: { register: resolve(import.meta.dirname, 'hooks/register.ts') }, formats: ['es'] },
        target: 'esnext',
        emptyOutDir: true,
        copyPublicDir: false,
        rolldownOptions: {
            output: {
                entryFileNames: 'hooks/[name].js',
                chunkFileNames: 'hooks/[name]-[hash].js',
                minify: { compress: true, mangle: false, codegen: { removeWhitespace: true } },
                codeSplitting: {
                    groups: [
                        { name: 'effect', test: /\/effect@/u, maxSize: 900_000 },
                        { name: 'usage', test: /\/usage\/[^/]+\.json$/u, maxSize: 900_000 },
                        { name: 'libs', test: /\/(?:libs\/typescript\/runtime|node_modules)\//u, maxSize: 900_000 },
                    ],
                },
                strictExecutionOrder: true,
            },
        },
    },
});
