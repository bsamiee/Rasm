import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const SITE = { name: 'site', targets: { 'build--vite': { command: 'true', outputs: ['{projectRoot}/dist'] } } };
const CONTEXT = (context: string) => ({
    'ops/project.json': '{"name":"ops"}',
    'ops/compose.yaml': `services:\n    site:\n        build:\n            context: ${context}\n            dockerfile_inline: FROM scratch\n`,
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('build context dependencies', () => {
    it.for([
        { context: 'the output folder', at: '../libs/site/dist', site: SITE, edge: true },
        { context: 'a folder inside the output', at: '../libs/site/dist/public', site: SITE, edge: true },
        { context: 'the output folder of a configuration', at: '../libs/site/dist', site: { name: 'site', targets: { 'build--vite': { command: 'true', outputs: ['{options.outputPath}'], options: { outputPath: 'libs/site/out' }, configurations: { production: { outputPath: 'libs/site/dist' } } } } }, edge: true },
        { context: 'a source folder no output covers', at: '../libs/site/public', site: SITE, edge: false },
    ])('makes the Compose project depend on the project building $context: $edge', async ({ at, site, edge }, { workspace }) => {
        const graph = await workspace.write({ ...CONTEXT(at), 'libs/site/project.json': JSON.stringify(site), 'libs/site/public/index.html': '<p>site</p>\n' }).graph({ createNodes, createDependencies });

        expect(graph.dependencies['ops']?.some(({ target, type }) => target === 'site' && type === 'static')).toBe(edge);
    });

    it('builds the project producing a build context before the image build reading it', async ({ workspace }) => {
        const graph = await workspace.write({ ...CONTEXT('../libs/site/dist'), 'libs/site/project.json': JSON.stringify(SITE) }).graph({ createNodes, createDependencies });

        const { dependencies } = taskGraph(graph, ['ops'], ['build--compose']);

        expect(dependencies['ops:build--compose']).toContainEqual({ id: 'site:build--vite' });
    });
});
