import { createNodes } from '@rasm/nx-playwright';
import { it as nx, taskGraph, unaggregated } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const rich = {
    'package.json': '{"name":"repo","workspaces":["apps/*"]}',
    'package-lock.json': '{}',
    'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.spec.ts"]}}',
    'tsconfig.base.json': '{}',
    'tsconfig.shared.json': '{"extends":"./tsconfig.base.json"}',
    'apps/e2e/package.json': '{"name":"e2e"}',
    'apps/e2e/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
    'apps/e2e/playwright.config.js': "module.exports = { reporter: [['html', { outputFolder: '../../reports/e2e' }]], webServer: { command: 'nx run web:serve', reuseExistingServer: true } };\n",
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
});

describe('projects test--playwright joins', { timeout: 60_000 }, () => {
    it('infers test--playwright from every config file name playwright test loads', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'package.json': '{"name":"repo"}',
                'apps/ts/project.json': '{"name":"ts"}',
                'apps/ts/playwright.config.ts': 'export default {};\n',
                'apps/js/project.json': '{"name":"js"}',
                'apps/js/playwright.config.js': 'module.exports = {};\n',
                'apps/mts/project.json': '{"name":"mts"}',
                'apps/mts/playwright.config.mts': 'export default {};\n',
                'apps/mjs/project.json': '{"name":"mjs"}',
                'apps/mjs/playwright.config.mjs': 'export default {};\n',
                'apps/cts/project.json': '{"name":"cts"}',
                'apps/cts/playwright.config.cts': 'module.exports = {};\n',
                'apps/cjs/project.json': '{"name":"cjs"}',
                'apps/cjs/playwright.config.cjs': 'module.exports = {};\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ name, data: { targets = {} } }) => [name, 'test--playwright' in targets]))).toStrictEqual({ cjs: true, cts: true, js: true, mjs: true, mts: true, ts: true });
    });

    it('infers no project where neither a project.json nor a package.json the workspaces list sits beside the config', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'package.json': '{"name":"repo","workspaces":["apps/*"]}',
                'apps/listed/package.json': '{"name":"listed"}',
                'apps/listed/playwright.config.js': 'module.exports = {};\n',
                'tools/marked/project.json': '{"name":"marked"}',
                'tools/marked/playwright.config.js': 'module.exports = {};\n',
                'tools/unlisted/package.json': '{"name":"unlisted"}',
                'tools/unlisted/playwright.config.js': 'module.exports = {};\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ data: { root, targets = {} } }) => [root, 'test--playwright' in targets]))).toStrictEqual({ 'apps/listed': true, 'tools/marked': true });
    });

    it('runs test--playwright and clean--playwright through the test and clean verbs, hashing every input of each', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes }, { outputPath: '.artifacts/playwright/{projectName}' });

        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it('names every playwright target that declares outputs in the targets clean--playwright removes', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes });
        const targets = graph.nodes['e2e']?.data.targets ?? {};

        expect(targets['clean--playwright']).toHaveProperty(
            ['options', 'targets'],
            Object.keys(targets).filter((name) => name.endsWith('--playwright') && (targets[name]?.outputs ?? []).length > 0),
        );
    });

    it('runs test--playwright after the build targets of each project the suite depends on, and of none it does not', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'package.json': '{"name":"repo","workspaces":["apps/*"]}',
                'apps/e2e/package.json': '{"name":"e2e","nx":{"implicitDependencies":["extension"]}}',
                'apps/e2e/playwright.config.js': 'module.exports = {};\n',
                'apps/extension/project.json': '{"name":"extension","targets":{"build--vite":{"executor":"nx:noop"}}}',
                'apps/site/project.json': '{"name":"site","targets":{"build--vite":{"executor":"nx:noop"}}}',
            })
            .graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['e2e'], ['test--playwright']);

        expect(dependencies['e2e:test--playwright']?.map(({ id }) => id)).toStrictEqual(['extension:build--vite']);
    });

    it('tags the project of a Playwright config host:browser, the tag CI routes to its browser job', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph({ createNodes });

        expect(graph.nodes['e2e']?.data.tags).toContain('host:browser');
    });
});
