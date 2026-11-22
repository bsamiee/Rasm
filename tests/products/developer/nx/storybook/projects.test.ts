import { createNodes } from '@rasm/nx-storybook';
import { it as nx, unaggregated } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const rich = {
    'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.spec.ts"]}}',
    'tsconfig.base.json': '{}',
    'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
    'tools/storybook/shared.ts': 'export const stories = ["../src/**/*.stories.tsx"];\n',
    'apps/site/project.json': '{"name":"site"}',
    'apps/site/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
    'apps/site/node_modules/@storybook/test-runner/package.json': '{"name":"@storybook/test-runner","version":"0.24.0"}',
    'apps/site/.storybook/main.ts': "import { stories } from '../../../tools/storybook/shared.ts';\n\nexport default { stories, framework: '@storybook/react-vite' };\n",
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}' });
});

describe('projects the Storybook targets join', { timeout: 60_000 }, () => {
    it('infers build--storybook for the folder holding .storybook, from every main config name Storybook loads', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/js/project.json': '{"name":"js"}',
                'apps/js/.storybook/main.js': 'export default { stories: [] };\n',
                'apps/mjs/project.json': '{"name":"mjs"}',
                'apps/mjs/.storybook/main.mjs': 'export default { stories: [] };\n',
                'apps/cjs/project.json': '{"name":"cjs"}',
                'apps/cjs/.storybook/main.cjs': 'module.exports = { stories: [] };\n',
                'apps/jsx/project.json': '{"name":"jsx"}',
                'apps/jsx/.storybook/main.jsx': 'export default { stories: [] };\n',
                'apps/ts/project.json': '{"name":"ts"}',
                'apps/ts/.storybook/main.ts': 'export default { stories: [] };\n',
                'apps/mts/project.json': '{"name":"mts"}',
                'apps/mts/.storybook/main.mts': 'export default { stories: [] };\n',
                'apps/cts/project.json': '{"name":"cts"}',
                'apps/cts/.storybook/main.cts': 'module.exports = { stories: [] };\n',
                'apps/tsx/project.json': '{"name":"tsx"}',
                'apps/tsx/.storybook/main.tsx': 'export default { stories: [] };\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ name, data: { targets = {} } }) => [name, 'build--storybook' in targets]))).toStrictEqual({ cjs: true, cts: true, js: true, jsx: true, mjs: true, mts: true, ts: true, tsx: true });
    });

    it('infers no project where neither a project.json nor a package.json the workspaces list sits beside .storybook', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/listed/package.json': '{"name":"listed"}',
                'apps/listed/.storybook/main.ts': 'export default { stories: [] };\n',
                'tools/marked/project.json': '{"name":"marked"}',
                'tools/marked/.storybook/main.ts': 'export default { stories: [] };\n',
                'tools/unlisted/package.json': '{"name":"unlisted"}',
                'tools/unlisted/.storybook/main.ts': 'export default { stories: [] };\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ data: { root, targets = {} } }) => [root, 'build--storybook' in targets]))).toStrictEqual({ 'apps/listed': true, 'tools/marked': true });
    });

    it('runs every Storybook target through its verb, hashing every input of each', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes }, { outputPath: '.artifacts/storybook/{projectName}' });

        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it('names every storybook target that declares outputs in the targets clean--storybook removes', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes });
        const targets = graph.nodes['site']?.data.targets ?? {};

        expect(targets['clean--storybook']).toHaveProperty(
            ['options', 'targets'],
            Object.keys(targets).filter((name) => name.endsWith('--storybook') && (targets[name]?.outputs ?? []).length > 0),
        );
    });
});
