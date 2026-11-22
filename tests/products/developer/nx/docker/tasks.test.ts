import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, unaggregated, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVED = {
    'infra/project.json': '{"name":"infra","targets":{"build--tsc":{"command":"true"}}}',
    'infra/compose.yaml': 'services:\n    proxy:\n        build: .\n    api:\n        build:\n            context: ../apps/api\n',
    'infra/Dockerfile': 'FROM scratch\n',
    'apps/api/project.json': '{"name":"api","targets":{"build--tsc":{"command":"true"}}}',
    'apps/api/Dockerfile': 'FROM scratch\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task order', () => {
    it.for([
        { task: 'infra:build--compose', first: 'infra:build--tsc' },
        { task: 'infra:install--compose', first: 'infra:build--compose' },
        { task: 'api:serve--compose', first: 'api:build--buildx' },
        { task: 'api:build--buildx', first: 'api:build--tsc' },
    ])('runs $first before $task', async ({ task, first }, { workspace }) => {
        const graph = await workspace.write(SERVED).graph({ createNodes, createDependencies });
        const [project = '', target = ''] = task.split(':');

        const { dependencies } = taskGraph(graph, [project], [target]);

        expect(dependencies[task]).toContainEqual({ id: first });
    });

    it('keeps the service start running for the tasks that need it, and caches only the checks', async ({ workspace }) => {
        const graph = await workspace.write(SERVED).graph({ createNodes, createDependencies });
        const stated = { ...graph.nodes['infra']?.data.targets, ...graph.nodes['api']?.data.targets };

        expect(Object.fromEntries(['check--compose', 'check--buildx', 'build--compose', 'install--compose', 'build--buildx', 'serve--compose'].map((target) => [target, [stated[target]?.cache, stated[target]?.continuous]]))).toStrictEqual({
            'check--compose': [true, undefined],
            'check--buildx': [true, undefined],
            'build--compose': [undefined, undefined],
            'install--compose': [undefined, undefined],
            'build--buildx': [undefined, undefined],
            'serve--compose': [undefined, true],
        });
    });

    it('install--compose starts the services from built images and returns once each is running and healthy', async ({ workspace }) => {
        const graph = await workspace.write(SERVED).graph({ createNodes, createDependencies });

        expect(graph.nodes['infra']?.data.targets?.['install--compose']?.options).toHaveProperty('command', expect.stringMatching(/^(?=.* --detach\b)(?=.* --wait\b)(?=.* --no-build\b)docker compose up /u));
    });

    it('runs every tool target of a project through its verb', async ({ workspace }) => {
        const graph = await workspace.write(SERVED).graph({ createNodes, createDependencies });

        expect(unaggregated(graph)).toStrictEqual([]);
    });
});

describe('profiles', () => {
    it('selects each Compose profile through a configuration of the build and the start', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'proj/project.json': '{"name":"proj"}',
                'proj/Dockerfile': 'FROM scratch\n',
                'proj/compose.yaml': 'services:\n    app:\n        build: .\n    traces:\n        image: jaegertracing/jaeger:2\n        profiles: [observed]\n    debugger:\n        image: node:24\n        profiles: [debug, observed]\n',
            })
            .graph({ createNodes, createDependencies });
        const configurations = { debug: { env: { COMPOSE_PROFILES: 'debug' } }, observed: { env: { COMPOSE_PROFILES: 'observed' } } };

        expect(graph.nodes['proj']?.data.targets).toMatchObject({ 'build--compose': { configurations }, 'install--compose': { configurations } });
    });

    it('gives no configuration where no service names a profile', async ({ workspace }) => {
        const graph = await workspace.write({ 'proj/project.json': '{"name":"proj"}', 'proj/Dockerfile': 'FROM scratch\n', 'proj/compose.yaml': 'services:\n    app:\n        build: .\n' }).graph({ createNodes, createDependencies });

        expect([graph.nodes['proj']?.data.targets?.['build--compose']?.configurations, graph.nodes['proj']?.data.targets?.['install--compose']?.configurations]).toStrictEqual([{}, {}]);
    });
});
