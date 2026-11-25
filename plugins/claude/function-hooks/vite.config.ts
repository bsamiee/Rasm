import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { defineConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const parser = resolve(import.meta.dirname, 'node_modules/sh-syntax');
const manifests = ['hooks/hooks.json', '.claude-plugin/plugin.json'];

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    root: import.meta.dirname,
    resolve: {
        alias: [
            { find: /^sh-syntax$/u, replacement: resolve(parser, 'lib/common.js') },
            { find: /^sh-syntax\/sync$/u, replacement: resolve(parser, 'lib/sync/processor.js') },
            { find: /^sh-syntax\/vendors\/wasm_exec$/u, replacement: resolve(parser, 'vendors/wasm_exec.cjs') },
        ],
    },
    plugins: [
        {
            name: 'assets',
            generateBundle() {
                this.emitFile({ type: 'asset', fileName: 'hooks/main.wasm', source: readFileSync(resolve(parser, 'main.wasm')) });
                manifests.forEach((fileName) => {
                    this.emitFile({ type: 'asset', fileName, source: readFileSync(resolve(import.meta.dirname, fileName)) });
                });
            },
        },
    ],
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
