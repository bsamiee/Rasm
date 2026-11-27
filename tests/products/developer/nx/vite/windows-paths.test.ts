import { createNodes } from '@rasm/nx-vite';
import { it as nx, unportable } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [MODELS] --------------------------------------------------------------------------

const Planned = Schema.Struct({ command: Schema.String }).annotate({ description: 'The command of an inferred nx:run-commands target' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('vite/package.json'), import.meta.resolve('@nx/vite/package.json'));
    workspace.write({
        'package.json': '{"name":"repo","type":"module","workspaces":["apps/*"]}',
        'package-lock.json': '{}',
        'tsconfig.base.json': '{}',
        'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
        'tools/vite/base.ts': "export const outDir = '../../dist/web';\n",
        'apps/web/package.json': '{"name":"web","type":"module"}',
        'apps/web/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
        'apps/web/index.html': '<!doctype html>\n',
        'apps/web/vite.config.ts': "import { outDir } from '../../tools/vite/base.ts';\n\nexport default { build: { outDir } };\n",
    });
});

describe('@rasm/nx-vite under Windows path separators', () => {
    it.for([
        { case: 'the outDir the config states', options: {} },
        { case: 'outputPath', options: { outputPath: '.artifacts/vite/{projectName}' } },
    ])('hands Nx and vite posix paths for a build into $case', async ({ options }, { workspace }) => {
        const graph = await workspace.graph({ createNodes }, options);
        const { command: built } = await Schema.decodeUnknownPromise(Planned)(graph.nodes['web']?.data.targets?.['build--vite']?.options);
        const { command: previewed } = await Schema.decodeUnknownPromise(Planned)(graph.nodes['web']?.data.targets?.['preview--vite']?.options);

        expect(unportable(graph)).toStrictEqual([]);
        expect([built, previewed].filter((command) => command.includes('\\'))).toStrictEqual([]);
    });
});
