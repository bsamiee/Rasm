import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { env, stderr } from 'node:process';
import type { NxPlugin } from '@nx/devkit';
import { createNodes } from '@rasm/nx-docker';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { workspaceRoot } from 'nx/src/utils/workspace-root';
import { describe, expect, onTestFinished, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const { targetDefaults } = readNxJson(workspaceRoot);
const HOSTED = {
    'project.json': JSON.stringify({ name: 'workspace', targets: { 'check--lint': { command: 'true', cache: true } } }),
    'nx.json': JSON.stringify({ targetDefaults }),
    '.config/mise.toml': '[tools]\n',
    '.gitignore': '.cache/\ntmp/\n',
};
const CLIENT = { 'node_modules/@testcontainers/postgresql/package.json': '{"name":"@testcontainers/postgresql","dependencies":{"testcontainers":"12.2.2"}}' };
const SUITE = (folder: string, required: object, targets: readonly string[] = ['test--vitest']) => ({
    [`${folder}/project.json`]: JSON.stringify({ name: folder, targets: Object.fromEntries(targets.map((target) => [target, { command: 'true' }])) }),
    [`${folder}/package.json`]: JSON.stringify({ name: folder, private: true, ...required }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = async (workspace: Workspace, plugin: Pick<NxPlugin, 'createNodes'>, project: string, target: string) => {
    const graph = await workspace.graph(plugin);
    const tasks = taskGraph(graph, [project], [target]);
    const { value } = await createTaskHasher(graph, readNxJson(workspace.root)).hashTask(tasks.tasks[`${project}:${target}`] ?? expect.unreachable(`${project}:${target} is no task`), tasks, env);
    return value;
};

const homed = (home: string | undefined, cache?: string) => {
    vi.stubEnv('COLIMA_HOME', home);
    vi.stubEnv('CACHE_DIR', cache);
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('root project', () => {
    it.for([
        { home: 'inside the workspace', at: (root: string) => join(root, '.cache/colima') },
        { home: 'unset', at: () => undefined },
    ])('hashes a root task as a workspace without the plugin does, with COLIMA_HOME $home', async ({ at }, { workspace }) => {
        homed(at(workspace.root), join(workspace.root, '.cache'));
        workspace.write(HOSTED);

        const bare = await hashed(workspace, {}, 'workspace', 'check--lint');

        await expect(hashed(workspace, { createNodes }, 'workspace', 'check--lint')).resolves.toBe(bare);
    });
});

describe('engine project', () => {
    it('holds Colima at a COLIMA_HOME inside the workspace, reclaiming the VM disk only once the VM runs', async ({ workspace }) => {
        homed(join(workspace.root, '.cache/colima'));
        const graph = await workspace.write(HOSTED).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['docker'], ['install--docker', 'clean--docker']);

        expect([graph.nodes['docker']?.data.root, dependencies]).toStrictEqual(['.cache/colima', { 'docker:install--docker': [], 'docker:clean--docker': [{ id: 'docker:install--docker' }] }]);
    });

    it.for([
        { home: 'unset', at: () => undefined },
        { home: 'empty', at: () => '' },
        { home: 'outside the workspace', at: () => join(tmpdir(), 'colima') },
    ])('confirms the service manager engine answers under CACHE_DIR, holding no clean target, with COLIMA_HOME $home', async ({ at }, { workspace }) => {
        homed(at(), join(workspace.root, '.cache'));
        const graph = await workspace.write(HOSTED).graph({ createNodes });

        expect([graph.nodes['docker']?.data.root, graph.nodes['docker']?.data.targets?.['install--docker']?.options, Object.keys(graph.nodes['docker']?.data.targets ?? {})]).toStrictEqual(['.cache/docker', { command: 'docker version' }, ['install', 'install--docker']]);
    });

    it('infers no engine project with neither COLIMA_HOME nor CACHE_DIR inside the workspace', async ({ workspace }) => {
        homed(undefined, join(tmpdir(), 'cache'));
        const graph = await workspace.write(HOSTED).graph({ createNodes });

        expect(Object.values(graph.nodes).map((node) => [node.name, Object.keys(node.data.targets ?? {})])).toStrictEqual([['workspace', ['check--lint']]]);
    });
});

describe('engine clients', () => {
    it.for([
        { client: 'testcontainers named directly', required: { devDependencies: { testcontainers: '12.2.2' } }, tagged: true },
        { client: 'testcontainers through a module requiring it', required: { devDependencies: { '@testcontainers/postgresql': '12.2.2' } }, tagged: true },
        { client: '@pulumi/docker', required: { dependencies: { '@pulumi/docker': '5.4.0' } }, tagged: true },
        { client: 'no engine client', required: { devDependencies: { vitest: '4.1.0' } }, tagged: false },
    ])('tags a package requiring $client: $tagged', async ({ required, tagged }, { workspace }) => {
        const graph = await workspace.write({ ...HOSTED, ...CLIENT, ...SUITE('suite', required) }).graph({ createNodes });

        expect(graph.nodes['suite']?.data.tags?.includes('host:docker')).toBe(tagged);
    });
});

describe('engine edges', () => {
    it.for([
        { engine: 'Colima', at: (root: string) => join(root, '.cache/colima') },
        { engine: 'the service manager', at: () => undefined },
    ])('starts the engine $engine holds before the test and install targets of each tagged package with no warning, adding no target', async ({ at }, { workspace }) => {
        homed(at(workspace.root), join(workspace.root, '.cache'));
        workspace.write({
            ...HOSTED,
            ...CLIENT,
            ...SUITE('containers', { devDependencies: { '@testcontainers/postgresql': '12.2.2' } }),
            ...SUITE('stack', { dependencies: { '@pulumi/docker': '5.4.0' } }, ['install--pulumi', 'test--vitest']),
            ...SUITE('unrelated', { devDependencies: { vitest: '4.1.0' } }),
            ...SUITE('bare', { devDependencies: { testcontainers: '12.2.2' } }, ['check--lint']),
        });
        const graph = await workspace.graph({ createNodes });
        const written = vi.spyOn(stderr, 'write');
        onTestFinished(() => {
            written.mockRestore();
        });

        const { dependencies } = taskGraph(graph, ['containers', 'stack', 'unrelated'], ['test--vitest', 'install--pulumi']);

        expect([dependencies, Object.keys(graph.nodes['bare']?.data.targets ?? {}), written.mock.calls]).toStrictEqual([
            {
                'containers:test--vitest': [{ id: 'docker:install--docker' }],
                'stack:install--pulumi': [{ id: 'docker:install--docker' }],
                'stack:test--vitest': [{ id: 'docker:install--docker' }],
                'unrelated:test--vitest': [],
                'docker:install--docker': [],
            },
            ['check--lint'],
            [],
        ]);
    });

    it('hashes a tagged suite alike whether Colima or the service manager holds the engine', async ({ workspace }) => {
        workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } }) });
        homed(undefined, join(workspace.root, '.cache'));
        const served = await hashed(workspace, { createNodes }, 'suite', 'test--vitest');
        homed(join(workspace.root, '.cache/colima'), join(workspace.root, '.cache'));

        await expect(hashed(workspace, { createNodes }, 'suite', 'test--vitest')).resolves.toBe(served);
    });
});

describe('inference cache', () => {
    it('tags a package that gains an engine client after the last graph', async ({ workspace }) => {
        await workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { vitest: '4.1.0' } }) }).graph({ createNodes });

        const graph = await workspace.write(SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } })).graph({ createNodes });

        expect(graph.nodes['suite']?.data.tags).toContain('host:docker');
    });

    it('tags a package that joins the workspace after the last graph', async ({ workspace }) => {
        await workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { vitest: '4.1.0' } }) }).graph({ createNodes });

        const graph = await workspace.write(SUITE('joined', { devDependencies: { testcontainers: '12.2.2' } })).graph({ createNodes });

        expect(graph.nodes['joined']?.data.tags).toContain('host:docker');
    });
});
