import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const NESTED = {
    'shared/compose.base.yaml': 'services:\n    store:\n        image: valkey/valkey:8\n',
    'deploy/infra/project.json': '{"name":"infra"}',
    'deploy/infra/compose.yaml': [
        'include:',
        '    - ../../shared/compose.base.yaml',
        'services:',
        '    api:',
        '        build:',
        '            context: ../../apps/api',
        '        env_file: env/api.env',
        '    site:',
        '        build:',
        '            context: ../../libs/site/dist',
        '            dockerfile: ../../../deploy/infra/images/site.Dockerfile',
        '',
    ].join('\n'),
    'deploy/infra/env/api.env': 'PORT=8080\n',
    'deploy/infra/images/site.Dockerfile': 'FROM scratch\n',
    'apps/api/project.json': '{"name":"api"}',
    'apps/api/Dockerfile': 'FROM scratch\n',
    'libs/site/project.json': '{"name":"site","targets":{"build--vite":{"command":"true","outputs":["{projectRoot}/dist"]}}}',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Windows path separators', () => {
    it('hands Nx workspace-relative posix roots, inputs, and working folders', async ({ workspace }) => {
        const graph = await workspace.write(NESTED).graph({ createNodes, createDependencies });

        expect(unportable(graph)).toStrictEqual([]);
        expect(graph.nodes['infra']?.data.targets?.['check--compose']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/apps/api/Dockerfile', '{workspaceRoot}/deploy/infra/env/api.env', '{workspaceRoot}/deploy/infra/images/site.Dockerfile', '{workspaceRoot}/shared/compose.base.yaml']));
    });

    it('matches a build context to the output of the project producing it', async ({ workspace }) => {
        const graph = await workspace.write(NESTED).graph({ createNodes, createDependencies });

        expect(graph.dependencies['infra']).toContainEqual({ source: 'infra', target: 'site', type: 'static' });
    });
});
