import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { claude } from '@rasm/vite/claude';
import { defineConfig, mergeConfig, type UserConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const parser = resolve(import.meta.dirname, 'node_modules/sh-syntax');

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig((env) =>
    claude(import.meta.dirname)(env).then((config) =>
        mergeConfig(config, {
            resolve: {
                alias: [
                    { find: /^sh-syntax$/u, replacement: resolve(parser, 'lib/common.js') },
                    { find: /^sh-syntax\/sync$/u, replacement: resolve(parser, 'lib/sync/processor.js') },
                    { find: /^sh-syntax\/vendors\/wasm_exec$/u, replacement: resolve(parser, 'vendors/wasm_exec.cjs') },
                ],
            },
            plugins: [
                {
                    name: 'parser',
                    generateBundle() {
                        this.emitFile({ type: 'asset', fileName: 'hooks/main.wasm', source: readFileSync(resolve(parser, 'main.wasm')) });
                    },
                },
            ],
        } satisfies UserConfig),
    ),
);
