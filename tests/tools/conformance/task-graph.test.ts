import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache, type ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/task-graph';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (name: string, targets: Readonly<Record<string, TargetConfiguration>>, files: readonly string[] = []): ProjectNodesWithFiles => ({
    projectNode: { name, type: 'lib', data: { root: `libs/${name}`, targets: { clean: { executor: 'nx:noop' }, ...targets } } },
    projectFiles: files.map((file) => ({ projectRootRelativeFile: file, depsItCreates: [] })),
});

const run = (target: Omit<TargetConfiguration, 'executor' | 'options'> = {}): TargetConfiguration => ({ executor: 'nx:run-commands', options: { command: 'true' }, ...target });

const checked = async (...projects: readonly ProjectNodesWithFiles[]) => {
    const { tree, cleanup } = await createReadOnlyTree();
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(tree, [...projects]);
    const { details } = await rule.implementation({ tree, projectGraph, fileMapCache, ruleOptions: { folders: ['.artifacts/*/**', '**/dist{,/**}'], clean: 'clean' } });
    return details.violations.map(({ message }) => message);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task-graph', () => {
    it('accepts resolved dependencies, signalled continuous tasks, and apart cached outputs', async () => {
        const served = project('web', {
            'build--vite': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/vite/{projectName}'], dependsOn: ['^build--*'] }),
            'serve--vite': run({ continuous: true, readyWhen: { url: 'http://localhost:4200' } }),
            'test--playwright': run({ dependsOn: [{ target: 'serve--vite', waitFor: 'ready' }] }),
        });
        await expect(checked(served, project('lib', { 'build--tsc': run({ cache: true, outputs: ['{projectRoot}/dist'] }) }))).resolves.toStrictEqual([]);
    });

    it('reports a dependsOn entry naming no target', async () => {
        await expect(checked(project('lib', { test: run({ dependsOn: ['lint', '^compile', 'build--*'] }), 'build--tsc': run() }))).resolves.toStrictEqual([
            'lib:test dependsOn "lint" names no target, name an existing target or drop the entry',
            'lib:test dependsOn "^compile" names no target, name an existing target or drop the entry',
        ]);
    });

    it('reports a task cycle and a continuous dependency without a ready signal', async () => {
        const messages = await checked(project('lib', { first: run({ dependsOn: ['second'] }), second: run({ dependsOn: ['first'] }), serve: run({ continuous: true }), e2e: run({ dependsOn: ['serve'] }) }));
        expect(messages).toStrictEqual(['Task cycle lib:first -> lib:second -> lib:first, drop one dependsOn entry of it', 'lib:e2e waits for lib:serve to be started, give lib:serve a readyWhen signal and the edge waitFor ready']);
    });

    it('reports cached outputs that escape the folders, overwrite workspace files, or nest', async () => {
        const messages = await checked(
            project('lib', { 'build--tsc': run({ cache: true, outputs: ['{workspaceRoot}/out/{projectName}', '{projectRoot}/src'] }), 'check--tsc': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/tsc/lib'] }), 'pack--tsc': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/tsc/lib/packed'] }) }, [
                'src/index.ts',
            ]),
        );
        expect(messages).toStrictEqual([
            'lib:build--tsc writes out/lib outside .artifacts/*/**, **/dist{,/**}',
            'lib:build--tsc writes libs/lib/src outside .artifacts/*/**, **/dist{,/**}',
            'lib:build--tsc writes libs/lib/src over workspace file libs/lib/src, which a replay overwrites',
            'lib:check--tsc writes .artifacts/tsc/lib, which holds .artifacts/tsc/lib/packed of lib:pack--tsc',
        ]);
    });

    it('fails with the refusal of a graph Nx cannot plan', async () => {
        await expect(checked(project('lib', { 'build--tsc': run({ cache: true, outputs: ['dist'] }) }))).rejects.toThrow(/\{workspaceRoot\}.*\{projectRoot\}/su);
    });

    it('reports outputs without a clean target and a cached task depending on clean', async () => {
        const messages = await checked(project('lib', { 'build--tsc': run({ cache: true, outputs: ['{projectRoot}/dist'], dependsOn: ['clean'] }) }), {
            projectNode: { name: 'bare', type: 'lib', data: { root: 'libs/bare', targets: { 'build--tsc': run({ outputs: ['{projectRoot}/dist'] }) } } },
        });
        expect(messages).toStrictEqual(['bare:build--tsc declares outputs in a project without a clean target, give the project clean', 'lib:build--tsc is cached and depends on a clean task, which deletes outputs before every replay, drop that dependsOn entry']);
    });
});
