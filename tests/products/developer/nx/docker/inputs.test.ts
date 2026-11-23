import { join } from 'node:path';
import { env } from 'node:process';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, pathed, taskGraph } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { assert, describe, expect, vi } from 'vitest';

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

describe('task hashes', () => {
    it.for([
        { task: 'infra:check--compose', edited: { 'shared/cache.yaml': 'services:\n    cache:\n        image: memcached:2\n' }, changes: true },
        { task: 'infra:check--compose', edited: { 'infra/api.env': 'PORT=9090\n' }, changes: true },
        { task: 'infra:check--buildx', edited: { 'infra/images/site.Dockerfile': 'FROM busybox\n' }, changes: true },
        { task: 'infra:check--compose', edited: { 'docs/notes.md': 'More notes\n' }, changes: false },
        { task: 'infra:build--compose', edited: { 'apps/api/Dockerfile': 'FROM busybox\n' }, changes: true },
    ])('$task hash changes ($changes) after editing $edited', async ({ task, edited, changes }, { workspace }) => {
        const [project = '', target = ''] = task.split(':');
        const hashed = async (files: Readonly<Record<string, string>>) => {
            const graph = await workspace.write(files).graph({ createNodes, createDependencies });
            const tasks = taskGraph(graph, [project], [target]);
            const found = tasks.tasks[task];
            assert(found !== undefined, `${task} is a task of the graph`);
            return createTaskHasher(graph, readNxJson(workspace.root)).hashTask(found, tasks, env);
        };
        const before = await hashed({ ...READ, 'apps/api/project.json': '{"name":"api"}', 'docs/notes.md': 'Notes\n' });

        const after = await hashed(edited);

        expect(before.value === after.value).toBe(!changes);
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
