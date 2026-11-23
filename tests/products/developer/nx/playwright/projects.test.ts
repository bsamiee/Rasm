import { createNodes } from '@rasm/nx-playwright';
import { it as nx, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}' });
});

describe('projects test--playwright joins', { timeout: 60_000 }, () => {
    it('infers a browser suite from each config file name playwright test loads, only where a project.json or a package.json the workspaces list sits beside it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/ts/package.json': '{"name":"ts"}',
                'apps/ts/playwright.config.ts': 'export default {};\n',
                'apps/js/package.json': '{"name":"js"}',
                'apps/js/playwright.config.js': 'module.exports = {};\n',
                'apps/mts/package.json': '{"name":"mts"}',
                'apps/mts/playwright.config.mts': 'export default {};\n',
                'apps/mjs/package.json': '{"name":"mjs"}',
                'apps/mjs/playwright.config.mjs': 'export default {};\n',
                'apps/cts/package.json': '{"name":"cts"}',
                'apps/cts/playwright.config.cts': 'module.exports = {};\n',
                'tools/cjs/project.json': '{"name":"cjs"}',
                'tools/cjs/playwright.config.cjs': 'module.exports = {};\n',
                'tools/unlisted/package.json': '{"name":"unlisted"}',
                'tools/unlisted/playwright.config.js': 'module.exports = {};\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ data: { root, targets = {}, tags = [] } }) => [root, { suite: 'test--playwright' in targets, browser: tags.includes('host:browser') }]))).toStrictEqual(
            Object.fromEntries(['apps/ts', 'apps/js', 'apps/mts', 'apps/mjs', 'apps/cts', 'tools/cjs'].map((root) => [root, { suite: true, browser: true }])),
        );
    });

    it('runs test--playwright after the Vite build of each project the suite depends on, and after no other build', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/e2e/package.json': '{"name":"e2e","nx":{"implicitDependencies":["extension","lib"]}}',
                'apps/e2e/playwright.config.js': 'module.exports = {};\n',
                'apps/extension/project.json': '{"name":"extension","targets":{"build--vite":{"executor":"nx:noop"},"build--storybook":{"executor":"nx:noop"}}}',
                'apps/lib/project.json': '{"name":"lib","targets":{"build--tsc":{"executor":"nx:noop"}}}',
                'apps/site/project.json': '{"name":"site","targets":{"build--vite":{"executor":"nx:noop"}}}',
            })
            .graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['e2e'], ['test--playwright']);

        expect(dependencies['e2e:test--playwright']?.map(({ id }) => id)).toStrictEqual(['extension:build--vite']);
    });
});
