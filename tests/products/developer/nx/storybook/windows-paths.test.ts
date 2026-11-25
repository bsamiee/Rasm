import { createNodes } from '@rasm/nx-storybook';
import { it as nx, unportable } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [MODELS] --------------------------------------------------------------------------

const Planned = Schema.Struct({ command: Schema.String }).annotate({ description: 'The command of the inferred storybook build target' });
const Served = Schema.Struct({ staticFilePath: Schema.String }).annotate({ description: 'The folder @nx/web:file-server serves' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
    workspace.write({
        'package.json': '{"name":"repo","workspaces":["apps/*"]}',
        'package-lock.json': '{}',
        'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default"]}}',
        'tsconfig.base.json': '{}',
        'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
        'tools/storybook/shared.ts': "export const stories = ['../src/**/*.stories.tsx', '../../../libs/ui/**/*.stories.tsx'];\n",
        'apps/site/package.json': '{"name":"site"}',
        'apps/site/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
        'apps/site/.storybook/main.ts': "import { stories } from '../../../tools/storybook/shared.ts';\n\nexport default { stories, framework: '@storybook/react-vite' };\n",
    });
});

describe('@rasm/nx-storybook under Windows path separators', () => {
    it.for([
        { case: 'storybook-static', options: {} },
        { case: 'outputPath', options: { outputPath: '.artifacts/storybook/{projectName}' } },
    ])('hands Nx and storybook posix paths for a build into $case', async ({ options }, { workspace }) => {
        const graph = await workspace.graph({ createNodes }, options);
        const targets = graph.nodes['site']?.data.targets ?? {};
        const { command } = await Schema.decodeUnknownPromise(Planned)(targets['build--storybook']?.options);
        const { staticFilePath } = await Schema.decodeUnknownPromise(Served)(targets['preview--storybook']?.options);

        expect(unportable(graph)).toStrictEqual([]);
        expect([command, staticFilePath].filter((path) => path.includes('\\'))).toStrictEqual([]);
    });
});
