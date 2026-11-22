import { createNodes } from '@rasm/nx-storybook';
import { it as nx, outputs, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('storybook/package.json'), import.meta.resolve('@storybook/react-vite/package.json'), import.meta.resolve('react/package.json'), import.meta.resolve('react-dom/package.json'), import.meta.resolve('vite/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/site/package.json': '{"name":"site"}', 'apps/site/.storybook/main.ts': "export default { stories: ['../src/**/*.stories.tsx'], framework: '@storybook/react-vite' };\n" });
});

describe('targets of a Storybook project', { timeout: 60_000 }, () => {
    it.for([
        { case: 'in the project', options: {} },
        { case: 'under outputPath', options: { outputPath: '.artifacts/storybook/{projectName}' } },
    ])('serves with preview--storybook the folder build--storybook writes $case, after building it', async ({ options }, { workspace }) => {
        const graph = await workspace.graph({ createNodes }, options);
        const { dependencies } = taskGraph(graph, ['site'], ['preview--storybook']);
        const [written] = outputs(graph, 'site', 'build--storybook');

        expect(dependencies['site:preview--storybook']).toStrictEqual([{ id: 'site:build--storybook' }]);
        expect(graph.nodes['site']?.data.targets?.['preview--storybook']).toHaveProperty(['options', 'staticFilePath'], written);
    });

    it('declares the folder each spelling of the storybook build output flag names', async ({ workspace }) => {
        const graph = await workspace.graph({ createNodes });

        expect([outputs(graph, 'site', 'build--storybook', undefined, { 'output-dir': 'kebab' }), outputs(graph, 'site', 'build--storybook', undefined, { outputDir: 'camel' }), outputs(graph, 'site', 'build--storybook', undefined, { o: 'short' })]).toStrictEqual([
            expect.arrayContaining(['apps/site/kebab']),
            expect.arrayContaining(['apps/site/camel']),
            expect.arrayContaining(['apps/site/short']),
        ]);
    });

    it.for([
        { case: 'resolves', files: { 'apps/site/node_modules/@storybook/test-runner/package.json': '{"name":"@storybook/test-runner","version":"0.24.0"}' }, waits: [{ id: 'site:serve--storybook', waitFor: 'ready' }] },
        { case: 'does not resolve', files: {}, waits: undefined },
    ])('runs test-storybook against the running serve--storybook where @storybook/test-runner $case from the main config', async ({ files, waits }, { workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });
        const tested = 'test--storybook' in (graph.nodes['site']?.data.targets ?? {});

        expect(tested ? taskGraph(graph, ['site'], ['test--storybook']).continuousDependencies['site:test--storybook'] : undefined).toStrictEqual(waits);
    });
});
