import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, pathed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = {
    'infra/project.json': '{"name":"infra"}',
    'infra/compose.yaml': [
        'include:',
        '    - ../shared/store.yaml',
        '    - path:',
        '          - ../shared/queue.yaml',
        'services:',
        '    api:',
        '        build:',
        '            context: ../apps/api',
        '        env_file: [api.env, /etc/hosts]',
        '    proxy:',
        '        extends:',
        '            file: ../shared/proxy.yaml',
        '            service: proxy',
        '    site:',
        '        build:',
        '            context: .',
        '            dockerfile: images/site.Dockerfile',
        '',
    ].join('\n'),
    'infra/compose.override.yaml': 'services:\n    api:\n        environment:\n            MODE: local\n',
    'infra/api.env': 'PORT=8080\n',
    'infra/images/site.Dockerfile': 'FROM scratch\n',
    'shared/store.yaml': 'include:\n    - cache.yaml\nservices:\n    store:\n        image: valkey/valkey:8\n',
    'shared/cache.yaml': 'services:\n    cache:\n        image: memcached:1\n',
    'shared/queue.yaml': 'services:\n    queue:\n        image: nats:2\n',
    'shared/proxy.yaml': 'services:\n    proxy:\n        image: traefik:v3\n',
    'apps/api/Dockerfile': 'FROM scratch\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('inputs', () => {
    it('hashes every workspace file the Compose model reads into the checks, and no file outside the workspace', async ({ workspace }) => {
        const graph = await workspace.write(READ).graph({ createNodes, createDependencies });

        expect(new Set(graph.nodes['infra']?.data.targets?.['check--compose']?.inputs)).toStrictEqual(
            new Set([
                '{workspaceRoot}/apps/api/Dockerfile',
                '{workspaceRoot}/infra/.env',
                '{workspaceRoot}/infra/api.env',
                '{workspaceRoot}/infra/compose.override.yaml',
                '{workspaceRoot}/infra/compose.yaml',
                '{workspaceRoot}/infra/images/site.Dockerfile',
                '{workspaceRoot}/shared/cache.yaml',
                '{workspaceRoot}/shared/proxy.yaml',
                '{workspaceRoot}/shared/queue.yaml',
                '{workspaceRoot}/shared/store.yaml',
                { externalDependencies: [] },
            ]),
        );
    });

    it('hashes the project files of the build context into the image build and the start', async ({ workspace }) => {
        const graph = await workspace.write(READ).graph({ createNodes, createDependencies });
        const { 'build--compose': build, 'install--compose': install } = graph.nodes['infra']?.data.targets ?? {};

        expect([build?.inputs, install?.inputs]).toStrictEqual([expect.arrayContaining(['default', '^default', '{workspaceRoot}/shared/store.yaml']), expect.arrayContaining(['default', '^default', '{workspaceRoot}/shared/store.yaml'])]);
    });
});

describe('inference cache', () => {
    it('answers an unchanged Compose model from the cache with no docker run', async ({ workspace }) => {
        await workspace.write(READ).graph({ createNodes, createDependencies });
        pathed(join(workspace.root, 'bin'), {});

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(graph.nodes['infra']?.data.targets).toHaveProperty('check--compose');
    });

    it('reads the Compose model again after an included file changes', async ({ workspace }) => {
        await workspace.write(READ).graph({ createNodes, createDependencies });

        const graph = await workspace.write({ 'shared/cache.yaml': 'services:\n    cache:\n        image: memcached:1\n        profiles: [cached]\n' }).graph({ createNodes, createDependencies });

        expect(graph.nodes['infra']?.data.targets?.['install--compose']?.configurations).toStrictEqual({ cached: { env: { COMPOSE_PROFILES: 'cached' } } });
    });

    it('reads the Compose model again after a Compose file joins the folder', async ({ workspace }) => {
        await workspace.write(READ).remove('infra/compose.override.yaml').graph({ createNodes, createDependencies });

        const graph = await workspace.write({ 'infra/compose.override.yaml': 'services:\n    debugger:\n        image: node:24\n        profiles: [debug]\n' }).graph({ createNodes, createDependencies });

        expect(graph.nodes['infra']?.data.targets?.['install--compose']?.configurations).toStrictEqual({ debug: { env: { COMPOSE_PROFILES: 'debug' } } });
    });
});
