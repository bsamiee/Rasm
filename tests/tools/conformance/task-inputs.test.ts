import type { ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/task-inputs';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, violations } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = (name: string, root: string, inputs: NonNullable<TargetConfiguration['inputs']>, files: readonly string[], depends: readonly string[] = []) => project(name, { root, targets: { build: { executor: 'nx:run-commands', options: { command: 'true' }, inputs } } }, files, depends);

const checked = (...projects: readonly ProjectNodesWithFiles[]) => Effect.runPromise(violations(rule, { files: ['products/developer/nx/**', '.config/mise.lock'], env: ['NODE_AUTH_TOKEN'] }, { projects, nonProjectFiles: ['.config/mise.lock'] }));

const plugin = built('plugin', 'products/developer/nx/cargo', ['{projectRoot}/**/*'], ['plugin.ts']);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task-inputs', () => {
    it('accepts a plugin hashing its own files, a dependent reaching them through ^default, and an env input outside the list', async () => {
        await expect(checked(plugin, built('suite', 'libs/suite', ['default', '^default', { env: 'CI' }], ['index.ts'], ['plugin']))).resolves.toStrictEqual([]);
    });

    it('accepts a plugin file hashed by a project depending on that plugin', async () => {
        await expect(checked(plugin, built('suite', 'libs/suite', ['default', '{workspaceRoot}/products/developer/nx/cargo/**/*'], ['index.ts'], ['plugin']))).resolves.toStrictEqual([]);
    });

    it('accepts a manifest hashed by a project its owner depends on', async () => {
        await expect(
            checked(built('package', 'products/developer/nx/apple/analyzer', ['default'], ['Package.swift'], ['module']), built('module', 'products/developer/nx/apple/analyzer/Sources/module', ['default', '{workspaceRoot}/products/developer/nx/apple/analyzer/Package.swift'], ['main.swift'])),
        ).resolves.toStrictEqual([]);
    });

    it('accepts a workspace pattern whose negation drops the files no tool of the task reads', async () => {
        await expect(checked(plugin, built('lib', 'libs/lib', ['default', '{workspaceRoot}/products/developer/nx/**/*', '!{workspaceRoot}/products/developer/nx/cargo/**/*'], ['index.ts']))).resolves.toStrictEqual([]);
    });

    it('reports plugin source, the mise lock, and a credential variable in the inputs of an unrelated task', async () => {
        await expect(checked(plugin, built('lib', 'libs/lib', ['default', '{workspaceRoot}/products/developer/nx/**/*', '{workspaceRoot}/.config/mise.lock', { env: 'NODE_AUTH_TOKEN' }], ['index.ts']))).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib:build hashes products/developer/nx/cargo/plugin.ts, which its tool never reads, drop it from the inputs' },
            { sourceProject: 'lib', message: 'lib:build hashes .config/mise.lock, which its tool never reads, drop it from the inputs' },
            { sourceProject: 'lib', message: 'lib:build hashes credential variable NODE_AUTH_TOKEN, drop it from the inputs' },
        ]);
    });
});
