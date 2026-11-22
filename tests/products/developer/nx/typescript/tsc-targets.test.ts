import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it, taskGraph, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('tsc target scheduling', () => {
    it.for([
        { case: 'an emitting program', tsconfig: '{"files":["index.ts"],"compilerOptions":{"outDir":"dist"}}', tasks: ['lib:build--tsc', 'lib:check', 'lib:check--tsc'] },
        { case: 'a noEmit program', tsconfig: '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', tasks: ['lib:check', 'lib:check--tsc'] },
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
        { case: 'check--tsc of a noEmit dependent', tsconfig: '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', task: 'check--tsc' },
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
});

describe('tsc target hashing', () => {
    it.for([
        { case: 'nx.json names a production input', files: { 'nx.json': '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.spec.ts"]}}' }, upstream: '^production' },
        { case: 'nx.json names no production input', files: {}, upstream: '^default' },
    ])('hashes the $upstream inputs of dependencies when $case', async ({ files, upstream }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...ROOT, ...files, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'libs/lib/index.ts': 'export {};' })
            .graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['check--tsc']?.inputs?.filter((input) => typeof input === 'string' && input.startsWith('^'))).toStrictEqual([upstream]);
    });

    it.for([
        { case: 'the default compiler', options: undefined, packages: ['typescript'] },
        { case: 'compilerPackages', options: { compilerPackages: ['typescript', '@effect/tsgo'] as const }, packages: ['typescript', '@effect/tsgo'] },
    ])('hashes the installed version of $case as an external dependency', async ({ options, packages }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'libs/lib/index.ts': 'export {};' })
            .graph({ createNodes }, options);

        expect(graph.nodes['lib']?.data.targets?.['check--tsc']?.inputs).toContainEqual({ externalDependencies: packages });
    });

    it('refuses the graph when the first compilerPackages package does not resolve, naming it', async ({ workspace }) => {
        workspace.install(typescript).write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"]}', 'libs/lib/index.ts': 'export {};' });

        await expect(workspace.graph({ createNodes }, { compilerPackages: ['@absent/compiler'] })).rejects.toSatisfy(refused(/resolves no @absent\/compiler\/sync$/u));
    });

    it.for([
        { case: 'where @nx/js resolves', packages: [typescript, import.meta.resolve('@nx/js/package.json')] as const, generators: ['@nx/js:typescript-sync'] },
        { case: 'where @nx/js does not resolve', packages: [typescript] as const, generators: undefined },
    ])('keeps project references in sync through the @nx/js generator $case', async ({ packages, generators }, { workspace }) => {
        const graph = await workspace
            .install(...packages)
            .write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'libs/lib/index.ts': 'export {};' })
            .graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['check--tsc']?.syncGenerators).toStrictEqual(generators);
    });
});
