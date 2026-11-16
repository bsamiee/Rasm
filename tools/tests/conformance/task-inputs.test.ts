import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache, type ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/task-inputs';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (name: string, root: string, inputs: NonNullable<TargetConfiguration['inputs']>, files: readonly string[] = [], depends: readonly string[] = []): ProjectNodesWithFiles => ({
    projectNode: { name, type: 'lib', data: { root, targets: { build: { executor: 'nx:run-commands', options: { command: 'true' }, inputs } } } },
    projectFiles: files.map((file, at) => ({ projectRootRelativeFile: file, depsItCreates: at === 0 ? [...depends] : [] })),
});

const checked = async (...projects: readonly ProjectNodesWithFiles[]) => {
    const { tree, cleanup } = await createReadOnlyTree();
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(tree, [...projects]);
    const { details } = await rule.implementation({
        tree,
        projectGraph,
        fileMapCache: { ...fileMapCache, fileMap: { ...fileMapCache.fileMap, nonProjectFiles: [{ file: '.config/mise.lock', hash: 'lock' }] } },
        ruleOptions: { files: ['apps/nx/!(tests)/**', '.config/mise.lock'], env: ['NODE_AUTH_TOKEN'] },
    });
    return details.violations.map(({ message }) => message);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task-inputs', () => {
    it('accepts a plugin hashing its own files, a dependent reaching them through ^default, and an env input outside the list', async () => {
        await expect(checked(project('plugin', 'apps/nx/cargo', ['{projectRoot}/**/*'], ['plugin.ts']), project('suite', 'libs/suite', ['default', '^default', { env: 'CI' }], ['index.ts'], ['plugin']))).resolves.toStrictEqual([]);
    });

    it('accepts a plugin file hashed by a project depending on that plugin, and a manifest hashed by a project its owner depends on', async () => {
        await expect(checked(project('plugin', 'apps/nx/cargo', ['{projectRoot}/**/*'], ['plugin.ts']), project('suite', 'libs/suite', ['default', '{workspaceRoot}/apps/nx/cargo/**/*'], ['index.ts'], ['plugin']))).resolves.toStrictEqual([]);
        await expect(checked(project('package', 'apps/nx/apple/analyzer', ['default'], ['Package.swift'], ['module']), project('module', 'apps/nx/apple/analyzer/Sources/module', ['default', '{workspaceRoot}/apps/nx/apple/analyzer/Package.swift'], ['main.swift']))).resolves.toStrictEqual([]);
    });

    it('reports plugin source, the mise lock, and a credential variable in the inputs of an unrelated task', async () => {
        await expect(checked(project('plugin', 'apps/nx/cargo', ['{projectRoot}/**/*'], ['plugin.ts']), project('lib', 'libs/lib', ['default', '{workspaceRoot}/apps/nx/**/*', '{workspaceRoot}/.config/mise.lock', { env: 'NODE_AUTH_TOKEN' }], ['index.ts']))).resolves.toStrictEqual([
            'lib:build hashes apps/nx/cargo/plugin.ts, which its tool never reads, drop it from the inputs',
            'lib:build hashes .config/mise.lock, which its tool never reads, drop it from the inputs',
            'lib:build hashes credential variable NODE_AUTH_TOKEN, drop it from the inputs',
        ]);
    });
});
