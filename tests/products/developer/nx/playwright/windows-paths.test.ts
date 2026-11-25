import { createNodes } from '@rasm/nx-playwright';
import { it as nx, unportable } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [MODELS] --------------------------------------------------------------------------

const Planned = Schema.Struct({ command: Schema.String, env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) }).annotate({ description: 'The command and environment of the inferred nx:run-commands target' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({
        'package.json': '{"name":"repo","workspaces":["apps/*"]}',
        'package-lock.json': '{}',
        'tsconfig.base.json': '{}',
        'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
        'tools/pw/base.js': 'module.exports = {};\n',
        'apps/e2e/package.json': '{"name":"e2e"}',
        'apps/e2e/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
        'apps/e2e/playwright.config.js': "require('../../tools/pw/base.js');\nmodule.exports = { reporter: [['html', { outputFolder: '../../reports/e2e' }], ['junit']] };\n",
    });
});

describe('@rasm/nx-playwright under Windows path separators', () => {
    it.for([
        { case: 'in the folders the config states', options: {} },
        { case: 'under outputPath', options: { outputPath: '.artifacts/playwright/{projectName}' } },
    ])('hands Nx and Playwright posix paths for outputs $case', async ({ options }, { workspace }) => {
        const graph = await workspace.graph({ createNodes }, options);
        const { command, env = {} } = await Schema.decodeUnknownPromise(Planned)(graph.nodes['e2e']?.data.targets?.['test--playwright']?.options);

        expect(unportable(graph)).toStrictEqual([]);
        expect([command, ...Object.values(env)].filter((stated) => stated.includes('\\'))).toStrictEqual([]);
    });
});
