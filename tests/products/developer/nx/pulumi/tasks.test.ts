import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-pulumi';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUILT = { targets: { build: { dependsOn: [{ target: 'build--*', params: 'forward' }] }, 'build--tsc': { command: 'true' } } };
const MANIFESTS: Readonly<Record<string, { readonly name: string; readonly bin?: string; readonly exports?: object; readonly dependencies?: Readonly<Record<string, string>>; readonly nx: object }>> = {
    shared: { name: 'shared', nx: BUILT },
    source: { name: 'source', exports: { '.': './index.ts', './*': './*.ts' }, nx: BUILT },
    sourced: { name: 'sourced', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*', source: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['source'] } },
    layered: { name: 'layered', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*', wrapper: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['wrapper'] } },
    wrapper: { name: 'wrapper', exports: { '.': './index.ts' }, dependencies: { shared: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['shared'] } },
    infra: { name: 'infra', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*', '@pulumi/cloudflare': '*', shared: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['shared'] } },
    site: { name: 'site', dependencies: { infra: 'workspace:*' }, nx: { ...BUILT, implicitDependencies: ['infra'] } },
    compiled: { name: 'compiled', bin: 'dist/stacks.js', dependencies: { '@pulumi/pulumi': '*' }, nx: BUILT },
    app: { name: 'app', dependencies: { compiled: 'workspace:*', react: '*' }, nx: { ...BUILT, implicitDependencies: ['compiled'] } },
};
const STACKS = {
    'package.json': '{"name":"root","workspaces":["packages/*"]}',
    'packages/infra/stacks.mjs': 'export {};\n',
    'packages/sourced/stacks.mjs': 'export {};\n',
    'packages/layered/stacks.mjs': 'export {};\n',
    'packages/source/index.ts': 'export {};\n',
    'packages/source/util.ts': 'export {};\n',
    'packages/wrapper/index.ts': 'export {};\n',
    ...Object.fromEntries(Object.entries(MANIFESTS).map(([name, manifest]) => [`packages/${name}/package.json`, JSON.stringify(manifest)])),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph, task: string, versions: Readonly<Record<string, string>>) => {
    const [project = '', target = ''] = task.split(':');
    const planned = taskGraph(graph, [project], [target]);
    const installed: ProjectGraph = {
        nodes: graph.nodes,
        externalNodes: Object.fromEntries(Object.entries(versions).map(([name, version]) => [`npm:${name}`, { type: 'npm', name: `npm:${name}`, data: { packageName: name, version, hash: `${name}@${version}` } }])),
        dependencies: {
            ...graph.dependencies,
            ...Object.fromEntries(
                Object.values(MANIFESTS).map(({ name, dependencies = {} }) => [
                    name,
                    [
                        ...(graph.dependencies[name] ?? []),
                        ...Object.keys(dependencies)
                            .filter((at) => at in versions)
                            .map((at) => ({ source: name, target: `npm:${at}`, type: 'static' })),
                    ],
                ]),
            ),
        },
    };
    const found = planned.tasks[task];
    return found === undefined
        ? Promise.resolve(undefined)
        : createTaskHasher(installed, readNxJson(workspace.root))
              .hashTask(found, planned, {})
              .then(({ value }) => value);
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
        { task: 'sourced:deploy--pulumi', before: 'source:build--tsc', runs: false },
        { task: 'layered:deploy--pulumi', before: 'wrapper:build--tsc', runs: true },
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
});

describe('what the program targets hash', () => {
    it.for([
        { case: 'a package the program it runs declares', task: 'site:dry-run--pulumi', moved: '@pulumi/cloudflare', changes: true },
        { case: 'a package the program declares', task: 'infra:dry-run--pulumi', moved: '@pulumi/cloudflare', changes: true },
        { case: 'a package another project declares', task: 'infra:dry-run--pulumi', moved: 'react', changes: false },
    ])('hashes $task again after a new release of $case: $changes', async ({ task, moved, changes }, { workspace }) => {
        const graph = await workspace.write(STACKS).graph({ createNodes });
        const installed = { '@pulumi/pulumi': '3.0.0', '@pulumi/cloudflare': '6.0.0', react: '19.0.0' };

        const [before, after] = await Promise.all([hashed(workspace, graph, task, installed), hashed(workspace, graph, task, { ...installed, [moved]: '7.0.0' })]);

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });
});
