import { createNodes } from '@rasm/nx-pulumi';
import { it, unaggregated, taskGraph } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUILT = { targets: { build: { dependsOn: [{ target: 'build--*', params: 'forward' }] }, 'build--tsc': { command: 'true' } } };
const STACKS = {
    'package.json': '{"name":"root","workspaces":["packages/*"]}',
    'packages/shared/package.json': JSON.stringify({ name: 'shared', nx: BUILT }),
    'packages/infra/package.json': JSON.stringify({ name: 'infra', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*', shared: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['shared'] } }),
    'packages/infra/stacks.mjs': 'export {};\n',
    'packages/site/package.json': JSON.stringify({ name: 'site', dependencies: { infra: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['infra'] } }),
    'packages/compiled/package.json': JSON.stringify({ name: 'compiled', bin: 'dist/stacks.js', dependencies: { '@pulumi/pulumi': '*' }, nx: BUILT }),
    'packages/app/package.json': JSON.stringify({ name: 'app', dependencies: { compiled: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['compiled'] } }),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task order', () => {
    it.for([
        { task: 'infra:deploy--pulumi', before: 'shared:build--tsc', runs: true },
        { task: 'infra:deploy--pulumi', before: 'infra:build--tsc', runs: false },
        { task: 'site:deploy--pulumi', before: 'infra:deploy--pulumi', runs: true },
        { task: 'site:install--pulumi', before: 'infra:install--pulumi', runs: true },
        { task: 'site:deploy--pulumi', before: 'site:build--tsc', runs: true },
        { task: 'site:deploy--pulumi', before: 'infra:build--tsc', runs: false },
        { task: 'compiled:deploy--pulumi', before: 'compiled:build--tsc', runs: true },
        { task: 'app:deploy--pulumi', before: 'compiled:build--tsc', runs: true },
    ])('$task runs $before first: $runs', async ({ task, before, runs }, { workspace }) => {
        const graph = await workspace.write(STACKS).graph({ createNodes });
        const [project = '', target = ''] = task.split(':');

        const { dependencies } = taskGraph(graph, [project], [target]);

        expect(dependencies[task]?.some(({ id }) => id === before)).toBe(runs);
    });

    it('previews with no task run first, so a preview applies no stack it depends on', async ({ workspace }) => {
        const graph = await workspace.write(STACKS).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['site', 'app'], ['dry-run--pulumi']);

        expect([dependencies['site:dry-run--pulumi'], dependencies['app:dry-run--pulumi']]).toStrictEqual([[], []]);
    });

    it('runs each program alone, never beside another task', async ({ workspace }) => {
        const graph = await workspace.write(STACKS).graph({ createNodes });
        const { targets } = graph.nodes['site']?.data ?? {};

        expect(['install--pulumi', 'deploy--pulumi', 'dry-run--pulumi'].map((target) => targets?.[target]?.parallelism)).toStrictEqual([false, false, false]);
    });

    it('runs every tool target of a project through its verb', async ({ workspace }) => {
        const graph = await workspace.write(STACKS).graph({ createNodes });

        expect(unaggregated(graph)).toStrictEqual([]);
    });
});

describe('inputs', () => {
    it.for([
        { namedInputs: { default: ['{projectRoot}/**/*'] }, named: 'default' },
        { namedInputs: { default: ['{projectRoot}/**/*'], production: ['default', '!{projectRoot}/**/*.test.mjs'] }, named: 'production' },
    ])('hashes the $named files of the project and its dependencies', async ({ namedInputs, named }, { workspace }) => {
        const graph = await workspace.write({ ...STACKS, 'nx.json': JSON.stringify({ namedInputs }) }).graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['deploy--pulumi']?.inputs).toStrictEqual(expect.arrayContaining([named, `^${named}`]));
    });
});
