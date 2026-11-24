import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { env, stderr } from 'node:process';
import { pathToFileURL } from 'node:url';
import type { NxPlugin } from '@nx/devkit';
import { createNodes } from '@rasm/nx-docker';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { Option } from 'effect';
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
const SUITE = (folder: string, required: object, targets: readonly string[] = ['test--vitest'], stated: object = {}) => ({
    [`${folder}/project.json`]: JSON.stringify({ name: folder, ...stated, targets: Object.fromEntries(targets.map((target) => [target, { command: 'true' }])) }),
    [`${folder}/package.json`]: JSON.stringify({ name: folder, private: true, ...required }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = async (workspace: Workspace, plugin: Pick<NxPlugin, 'createNodes'>, id: string) => {
    const graph = await workspace.graph(plugin);
    const [project = '', target = ''] = id.split(':');
    const tasks = taskGraph(graph, [project], [target]);
    const task = Option.getOrThrow(Option.fromUndefinedOr(tasks.tasks[id]));
    const { value } = await createTaskHasher(graph, readNxJson(workspace.root)).hashTask(task, tasks, env);
    return value;
};

const homed = (home: string | undefined) => {
    vi.stubEnv('COLIMA_HOME', home);
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
        homed(at(workspace.root));
        workspace.write(HOSTED);

        const bare = await hashed(workspace, {}, 'workspace:check--lint');

        await expect(hashed(workspace, { createNodes }, 'workspace:check--lint')).resolves.toBe(bare);
    });
});

describe('engine project', () => {
    it('holds Colima at a COLIMA_HOME inside the workspace, reclaiming the VM disk only once the VM runs', async ({ workspace }) => {
        homed(join(workspace.root, '.cache/colima'));
        const graph = await workspace.write(HOSTED).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['colima'], ['install--colima', 'clean--colima']);

        expect([graph.nodes['colima']?.data.root, dependencies]).toStrictEqual(['.cache/colima', { 'colima:install--colima': [], 'colima:clean--colima': [{ id: 'colima:install--colima' }] }]);
    });

    it.for([
        { home: 'unset', at: () => undefined },
        { home: 'empty', at: () => '' },
        { home: 'outside the workspace', at: () => join(tmpdir(), 'colima') },
    ])('infers no engine target with COLIMA_HOME $home', async ({ at }, { workspace }) => {
        homed(at());
        const graph = await workspace.write(HOSTED).graph({ createNodes });

        expect(Object.values(graph.nodes).map(({ name, data: { targets = {} } }) => [name, Object.keys(targets)])).toStrictEqual([['workspace', ['check--lint']]]);
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

    it('keeps the tags a project states beside its own', async ({ workspace }) => {
        const graph = await workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } }, ['test--vitest'], { tags: ['scope:suite'] }) }).graph({ createNodes });

        expect(graph.nodes['suite']?.data.tags).toStrictEqual(expect.arrayContaining(['scope:suite', 'host:docker']));
    });

    it('tags a package reaching a client through a workspace package', async ({ workspace }) => {
        workspace.write({ ...HOSTED, ...SUITE('harness', { dependencies: { testcontainers: '12.2.2' } }), ...SUITE('library', { devDependencies: { harness: 'workspace:*' } }), ...SUITE('unrelated', { dependencies: { effect: '4.1.5' } }) });
        const graph = await workspace.install(['harness', pathToFileURL(join(workspace.root, 'harness/package.json')).href]).graph({ createNodes });

        expect(['harness', 'library', 'unrelated'].map((name) => graph.nodes[name]?.data.tags?.includes('host:docker'))).toStrictEqual([true, true, false]);
    });
});

describe('engine edges', () => {
    it('starts the engine before the test and install targets of each tagged package, adding no target', async ({ workspace }) => {
        homed(join(workspace.root, '.cache/colima'));
        workspace.write({
            ...HOSTED,
            ...CLIENT,
            ...SUITE('containers', { devDependencies: { '@testcontainers/postgresql': '12.2.2' } }),
            ...SUITE('stack', { dependencies: { '@pulumi/docker': '5.4.0' } }, ['install--pulumi', 'test--vitest']),
            ...SUITE('unrelated', { devDependencies: { vitest: '4.1.0' } }),
            ...SUITE('bare', { devDependencies: { testcontainers: '12.2.2' } }, ['check--lint']),
        });
        const graph = await workspace.graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['containers', 'stack', 'unrelated'], ['test--vitest', 'install--pulumi']);

        expect([dependencies, Object.keys(graph.nodes['bare']?.data.targets ?? {})]).toStrictEqual([
            {
                'containers:test--vitest': [{ id: 'colima:install--colima' }],
                'stack:install--pulumi': [{ id: 'colima:install--colima' }],
                'stack:test--vitest': [{ id: 'colima:install--colima' }],
                'unrelated:test--vitest': [],
                'colima:install--colima': [],
            },
            ['check--lint'],
        ]);
    });

    it('tags a package and plans its tasks with no dependency and no warning where no engine project exists', async ({ workspace }) => {
        homed(undefined);
        const graph = await workspace.write({ ...HOSTED, ...SUITE('suite', { dependencies: { '@pulumi/docker': '5.4.0' } }, ['test--vitest', 'install--pulumi']) }).graph({ createNodes });
        const written = vi.spyOn(stderr, 'write');
        onTestFinished(() => {
            written.mockRestore();
        });

        const { dependencies } = taskGraph(graph, ['suite'], ['test--vitest', 'install--pulumi']);

        expect([graph.nodes['suite']?.data.tags?.includes('host:docker'), dependencies, written.mock.calls]).toStrictEqual([true, { 'suite:test--vitest': [], 'suite:install--pulumi': [] }, []]);
    });

    it('hashes a tagged suite alike with and without the engine project', async ({ workspace }) => {
        workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } }) });
        homed(undefined);
        const apart = await hashed(workspace, { createNodes }, 'suite:test--vitest');
        homed(join(workspace.root, '.cache/colima'));

        await expect(hashed(workspace, { createNodes }, 'suite:test--vitest')).resolves.toBe(apart);
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
