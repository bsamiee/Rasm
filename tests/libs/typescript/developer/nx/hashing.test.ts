import { join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { CreateNodes, ProjectGraph } from '@nx/devkit';
import { aggregated, anchored, EXECUTORS } from '@rasm/nx';
import { it, type Workspace } from '@rasm/testing/nx';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { assert, describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const provider = {
    'package.json': '{ "name": "repo", "private": true, "workspaces": ["app", "tools/*"] }',
    'nx.json': '{ "namedInputs": { "default": ["{projectRoot}/**/*"] } }',
    'app/package.json': '{ "name": "app", "private": true }',
    'tools/runner/package.json': '{ "name": "@fixture/runner", "dependencies": { "@fixture/shared": "workspace:*" } }',
    'tools/runner/executors/run/executor.ts': 'export default async () => ({ success: true });\n',
    'tools/shared/package.json': '{ "name": "@fixture/shared", "dependencies": { "@fixture/deep": "workspace:*" } }',
    'tools/shared/index.ts': 'export const shared = 1;\n',
    'tools/deep/package.json': '{ "name": "@fixture/deep" }',
    'tools/deep/index.ts': 'export const deep = 1;\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const planned = async (workspace: Workspace, graph: ProjectGraph, task: string) => {
    const [project = '', target = ''] = task.split(':');
    const inspector = new HashPlanInspector(graph, workspace.root);
    await inspector.init();
    const inputs = inspector.inspectTaskInputs({ project, target }, {}, undefined, true)[task];
    assert(inputs !== undefined, `${task} is a task of the graph`);
    return inputs.files;
};

const executing = (executor: string): { readonly createNodes: CreateNodes } => ({
    createNodes: [
        'app/package.json',
        (matched, _, context) =>
            matched.map((file) => [
                file,
                {
                    projects: {
                        app: {
                            root: 'app',
                            targets: aggregated({ check: { runner: { executor, inputs: ['default'] }, shell: { executor: 'nx:run-commands', options: { command: 'true' }, inputs: ['default'] } } }, {}, 'fixture', anchored(context.workspaceRoot), { 'nx-release-publish': { executor, inputs: ['default'] } }),
                        },
                    },
                },
            ]),
    ],
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('executor package inputs', () => {
    it.for([
        { task: 'app:check--runner', file: 'tools/runner/executors/run/executor.ts', hashes: true },
        { task: 'app:check--runner', file: 'tools/shared/index.ts', hashes: true },
        { task: 'app:check--runner', file: 'tools/deep/index.ts', hashes: false },
        { task: 'app:check', file: 'tools/runner/executors/run/executor.ts', hashes: true },
        { task: 'app:nx-release-publish', file: 'tools/runner/executors/run/executor.ts', hashes: true },
        { task: 'app:check--shell', file: 'tools/runner/executors/run/executor.ts', hashes: false },
    ])('$task hashes $file ($hashes) where the executor package is a project of the workspace', async ({ task, file, hashes }, { workspace }) => {
        const graph = await workspace
            .write(provider)
            .install(...['runner', 'shared', 'deep'].map((tool) => pathToFileURL(join(workspace.root, 'tools', tool, 'package.json')).href))
            .graph(executing('@fixture/runner:run'));

        const covered = await planned(workspace, graph, task);

        expect(covered.includes(file)).toBe(hashes);
    });

    it.for([
        { case: 'installed from outside the workspace', base: {}, executor: EXECUTORS.clean, installed: [fileURLToPath(import.meta.resolve('@rasm/nx/package.json'))] },
        { case: 'installed in node_modules with a project.json', base: { 'node_modules/@fixture/runner/package.json': '{ "name": "@fixture/runner" }', 'node_modules/@fixture/runner/project.json': '{}' }, executor: '@fixture/runner:run', installed: [] },
        { case: 'held by no workspace member', base: { 'package.json': '{ "name": "repo", "private": true, "workspaces": ["app"] }' }, executor: '@fixture/runner:run', installed: ['tools/runner/package.json'] },
        { case: 'resolving nowhere', base: {}, executor: '@fixture/absent:run', installed: [] },
    ])('targets keep their stated inputs where the executor package is $case', async ({ base, executor, installed }, { workspace }) => {
        const graph = await workspace
            .write({ ...provider, ...base })
            .install(...installed.map((at) => pathToFileURL(resolve(workspace.root, at)).href))
            .graph(executing(executor));

        expect(['check', 'check--runner', 'nx-release-publish'].map((target) => graph.nodes['app']?.data.targets?.[target]?.inputs)).toStrictEqual([['default'], ['default'], ['default']]);
    });
});
