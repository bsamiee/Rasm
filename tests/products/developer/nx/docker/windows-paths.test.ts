import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, taskGraph, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const NESTED = (site: string, recipe = '../../../deploy/infra/images/site.Dockerfile') => ({
    'shared/compose.base.yaml': 'services:\n    store:\n        image: valkey/valkey:8\n',
    'deploy/infra/project.json': '{"name":"infra"}',
    'deploy/infra/compose.yaml': ['include:', '    - ../../shared/compose.base.yaml', 'services:', '    api:', '        build:', '            context: ../../apps/api', '        env_file: env/api.env', '    site:', '        build:', `            context: ${site}`, `            dockerfile: ${recipe}`, ''].join('\n'),
    'deploy/infra/env/api.env': 'PORT=8080\n',
    'deploy/infra/images/site.Dockerfile': 'FROM scratch\n',
    'apps/api/project.json': '{"name":"api"}',
    'apps/api/Dockerfile': 'FROM scratch\n',
    'libs/site/project.json': '{"name":"site","targets":{"build--vite":{"command":"true","outputs":["{projectRoot}/dist"]}}}',
    'libs/site/public/index.html': '<p>site</p>\n',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Windows path separators', () => {
    it('hands Nx workspace-relative posix roots, inputs, and working folders', async ({ workspace }) => {
        const graph = await workspace.write(NESTED('../../libs/site/dist')).graph({ createNodes, createDependencies });

        expect(unportable(graph)).toStrictEqual([]);
        expect(graph.nodes['infra']?.data.targets?.['check--compose']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/apps/api/Dockerfile', '{workspaceRoot}/deploy/infra/env/api.env', '{workspaceRoot}/deploy/infra/images/site.Dockerfile', '{workspaceRoot}/shared/compose.base.yaml']));
    });

    it.for([
        { context: 'a folder holding the output', at: '../../libs/site', recipe: '../../deploy/infra/images/site.Dockerfile', edge: true },
        { context: 'a source folder no output covers', at: '../../libs/site/public', recipe: '../../../deploy/infra/images/site.Dockerfile', edge: false },
    ])('makes the Compose project depend on the project whose output covers a build context of $context: $edge', async ({ at, recipe, edge }, { workspace }) => {
        const graph = await workspace.write(NESTED(at, recipe)).graph({ createNodes, createDependencies });

        expect(graph.dependencies['infra']?.some(({ target, type }) => target === 'site' && type === 'static')).toBe(edge);
    });

    it('depends on the project whose output folder is a build context, building it before the image build reading it', async ({ workspace }) => {
        const graph = await workspace.write(NESTED('../../libs/site/dist')).graph({ createNodes, createDependencies });

        const { dependencies } = taskGraph(graph, ['infra'], ['build--compose']);

        expect([graph.dependencies['infra']?.some(({ target, type }) => target === 'site' && type === 'static'), dependencies['infra:build--compose']]).toStrictEqual([true, expect.arrayContaining([{ id: 'site:build--vite' }])]);
    });
});
