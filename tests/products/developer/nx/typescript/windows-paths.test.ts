import { createNodes } from '@rasm/nx-typescript';
import { it, typescript, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('inference under Windows path separators', () => {
    it('hands Nx posix roots, inputs, outputs, and working folders', async ({ workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'), new URL('../package.json', import.meta.resolve('knip')).href)
            .write({
                'package.json': '{"name":"root","workspaces":["libs/*"]}',
                'package-lock.json': '',
                'eslint.config.mjs': "import { files } from './tools/lint/files.mjs';\n\nexport default [{ files }];\n",
                'tools/lint/files.mjs': "export const files = ['**/*.ts'];\n",
                'tsconfig.base.json': '{"compilerOptions":{"strict":true}}',
                'libs/shared/units.ts': 'export {};',
                'libs/other/tsconfig.json': '{"files":[],"compilerOptions":{"composite":true}}',
                'libs/lib/package.json': '{"name":"lib","version":"1.0.0"}',
                'libs/lib/tsconfig.json': '{"extends":"../../tsconfig.base.json","files":[],"references":[{"path":"./tsconfig.lib.json"},{"path":"../other"}]}',
                'libs/lib/tsconfig.lib.json': '{"include":["src/**/*.ts","../shared/*.ts"],"compilerOptions":{"composite":true,"rootDir":"..","outDir":"../../dist/lib","declarationDir":"types"}}',
                'libs/lib/src/nested/index.ts': 'export {};',
            })
            .graph({ createNodes }, { outputPath: '.artifacts/npm/{projectName}' });

        expect([graph.nodes['lib']?.data.targets?.['build--tsc']?.inputs?.includes('{workspaceRoot}/libs/shared/units.ts'), 'pack--npm' in (graph.nodes['lib']?.data.targets ?? {}), 'check--eslint' in (graph.nodes['lib']?.data.targets ?? {}), unportable(graph)]).toStrictEqual([true, true, true, []]);
    });
});
