import { join } from 'node:path';
import { platform } from 'node:process';
import { pathToFileURL } from 'node:url';
import { createNodes } from '@rasm/nx-docker';
import { it, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = { 'project.json': '{"name":"workspace"}', '.config/mise.toml': '[tools]\n' };
const CLIENT = { 'node_modules/@testcontainers/postgresql/package.json': '{"name":"@testcontainers/postgresql","dependencies":{"testcontainers":"12.2.2"}}' };
const SUITE = (folder: string, required: object, targets: readonly string[] = ['test--vitest', 'install--pulumi']) => ({
    [`${folder}/project.json`]: JSON.stringify({ name: folder, targets: Object.fromEntries(targets.map((target) => [target, { command: 'true' }])) }),
    [`${folder}/package.json`]: JSON.stringify({ name: folder, ...required }),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.runIf(platform === 'darwin')('engine on macOS', () => {
    it('reclaims the VM disk only once the VM runs', async ({ workspace }) => {
        const graph = await workspace.write(HOSTED).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['workspace'], ['clean--colima']);

        expect(dependencies['workspace:clean--colima']).toStrictEqual([{ id: 'workspace:install--colima' }]);
    });

    it.for([
        { client: 'testcontainers named directly', target: 'test--vitest', required: { devDependencies: { testcontainers: '12.2.2' } }, started: true },
        { client: 'testcontainers through a module requiring it', target: 'test--vitest', required: { devDependencies: { '@testcontainers/postgresql': '12.2.2' } }, started: true },
        { client: '@pulumi/docker', target: 'install--pulumi', required: { dependencies: { '@pulumi/docker': '5.4.0' } }, started: true },
        { client: '@pulumi/docker', target: 'test--vitest', required: { dependencies: { '@pulumi/docker': '5.4.0' } }, started: false },
        { client: 'no engine client', target: 'test--vitest', required: { devDependencies: { vitest: '4.1.0' } }, started: false },
    ])('starts the engine before $target of a package requiring $client: $started', async ({ target, required, started }, { workspace }) => {
        const graph = await workspace.write({ ...HOSTED, ...CLIENT, ...SUITE('suite', required) }).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['suite'], [target]);

        expect(dependencies[`suite:${target}`]?.some(({ id }) => id === 'workspace:install--colima')).toBe(started);
    });

    it('gives the engine edge to the target of each client a package reaches, through a workspace package included, and adds no target', async ({ workspace }) => {
        workspace.write({ ...HOSTED, ...SUITE('harness', { dependencies: { testcontainers: '12.2.2' } }, ['test--vitest']), ...SUITE('library', { devDependencies: { harness: 'workspace:*' } }, ['test--vitest']), ...SUITE('unrelated', { dependencies: { effect: '4.1.5' } }, ['test--vitest']) });
        const graph = await workspace.install(['harness', pathToFileURL(join(workspace.root, 'harness/package.json')).href]).graph({ createNodes });

        const { dependencies } = taskGraph(graph, Object.keys(graph.nodes), ['test--vitest', 'install--pulumi', 'install--colima', 'clean--colima']);

        expect(dependencies).toStrictEqual({
            'workspace:install--colima': [],
            'workspace:clean--colima': [{ id: 'workspace:install--colima' }],
            'harness:test--vitest': [{ id: 'workspace:install--colima' }],
            'library:test--vitest': [{ id: 'workspace:install--colima' }],
            'unrelated:test--vitest': [],
        });
    });

    it('infers no engine in a workspace mise does not configure', async ({ workspace }) => {
        const graph = await workspace.write({ 'project.json': '{"name":"workspace"}', ...SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } }) }).graph({ createNodes });

        expect([Object.keys(graph.nodes['workspace']?.data.targets ?? {}).filter((target) => target.endsWith('--colima')), taskGraph(graph, ['suite'], ['test--vitest']).dependencies['suite:test--vitest']]).toStrictEqual([[], []]);
    });
});

describe.runIf(platform === 'darwin')('inference cache', () => {
    it('starts the engine for a package that gains an engine client after the last graph', async ({ workspace }) => {
        await workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { vitest: '4.1.0' } }) }).graph({ createNodes });

        const graph = await workspace.write(SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } })).graph({ createNodes });

        expect(taskGraph(graph, ['suite'], ['test--vitest']).dependencies['suite:test--vitest']).toStrictEqual([{ id: 'workspace:install--colima' }]);
    });

    it('starts the engine for a package that joins the workspace after the last graph', async ({ workspace }) => {
        await workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { vitest: '4.1.0' } }) }).graph({ createNodes });

        const graph = await workspace.write(SUITE('joined', { devDependencies: { testcontainers: '12.2.2' } })).graph({ createNodes });

        expect(taskGraph(graph, ['joined'], ['test--vitest']).dependencies['joined:test--vitest']).toStrictEqual([{ id: 'workspace:install--colima' }]);
    });
});

describe.runIf(platform !== 'darwin')('engine elsewhere', () => {
    it('infers no engine target or edge where Colima does not run', async ({ workspace }) => {
        const graph = await workspace.write({ ...HOSTED, ...SUITE('suite', { devDependencies: { testcontainers: '12.2.2' } }) }).graph({ createNodes });

        expect([Object.keys(graph.nodes['workspace']?.data.targets ?? {}), taskGraph(graph, ['suite'], ['test--vitest']).dependencies['suite:test--vitest']]).toStrictEqual([[], []]);
    });
});
