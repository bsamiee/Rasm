import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it, taskGraph, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };
const CHECKED = '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('projects nx check type-checks', () => {
    it.for([
        { case: 'beside a package.json the root workspaces cover', files: { 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': CHECKED, 'libs/lib/index.ts': 'export {};' }, checked: ['lib:check--tsc'] },
        { case: 'beside a project.json', files: { 'apps/app/project.json': '{"name":"app"}', 'apps/app/tsconfig.json': CHECKED, 'apps/app/index.ts': 'export {};' }, checked: ['app:check--tsc'] },
        { case: 'beside a package.json outside the root workspaces', files: { 'tools/tool/package.json': '{"name":"tool"}', 'tools/tool/tsconfig.json': CHECKED, 'tools/tool/index.ts': 'export {};' }, checked: [] },
        {
            case: 'at a workspace root whose solution references programs of other projects alone',
            files: { 'tsconfig.json': '{"files":[],"references":[{"path":"./libs/lib"}]}', 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': CHECKED, 'libs/lib/index.ts': 'export {};' },
            checked: ['lib:check--tsc'],
        },
    ])('schedules $checked for nx check of a tsconfig.json $case', async ({ files, checked }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...ROOT, ...files })
            .graph({ createNodes });

        const { tasks } = taskGraph(
            graph,
            Object.keys(graph.nodes).filter((project) => 'check' in (graph.nodes[project]?.data.targets ?? {})),
            ['check'],
        );

        expect(Object.keys(tasks).filter((task) => task.endsWith(':check--tsc'))).toStrictEqual(checked);
    });

    it.for([
        { case: 'an emitting program', tsconfig: '{"files":["index.ts"],"compilerOptions":{"outDir":"dist"}}', tasks: ['lib:build--tsc', 'lib:check', 'lib:check--tsc'] },
        { case: 'a noEmit program', tsconfig: CHECKED, tasks: ['lib:check', 'lib:check--tsc'] },
    ])('runs the tasks $tasks for nx check of $case', async ({ tsconfig, tasks }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': tsconfig, 'libs/lib/index.ts': 'export {};' })
            .graph({ createNodes });

        const { tasks: scheduled } = taskGraph(graph, ['lib'], ['check']);

        expect(Object.keys(scheduled).toSorted()).toStrictEqual(tasks);
    });

    it.for([
        { case: 'build--tsc of an emitting dependent', tsconfig: '{"files":["index.ts"],"compilerOptions":{"outDir":"dist"}}', task: 'build--tsc' },
        { case: 'check--tsc of a noEmit dependent', tsconfig: CHECKED, task: 'check--tsc' },
    ])('runs $case after build--tsc of the project it depends on', async ({ tsconfig, task }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({
                ...ROOT,
                'libs/lib/package.json': '{"name":"lib"}',
                'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"outDir":"dist"}}',
                'libs/lib/index.ts': 'export {};',
                'apps/app/project.json': '{"name":"app","implicitDependencies":["lib"]}',
                'apps/app/tsconfig.json': tsconfig,
                'apps/app/index.ts': 'export {};',
            })
            .graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['app'], [task]);

        expect(dependencies[`app:${task}`]).toContainEqual({ id: 'lib:build--tsc' });
    });

    it('refuses the graph when the first compilerPackages package does not resolve, naming it', async ({ workspace }) => {
        workspace.install(typescript).write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"]}', 'libs/lib/index.ts': 'export {};' });

        await expect(workspace.graph({ createNodes }, { compilerPackages: ['@absent/compiler'] })).rejects.toSatisfy(refused(/resolves no @absent\/compiler\/sync$/u));
    });
});
