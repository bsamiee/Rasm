import type { ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/task-graph';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, violations } from '#/workspace.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = (target: Omit<TargetConfiguration, 'executor' | 'options'> = {}): TargetConfiguration => ({ executor: 'nx:run-commands', options: { command: 'true' }, ...target });

const cleaned = (name: string, targets: Readonly<Record<string, TargetConfiguration>>, files: readonly string[] = []) => project(name, { targets: { clean: { executor: 'nx:noop' }, ...targets } }, files);

const checked = (...projects: readonly ProjectNodesWithFiles[]) => Effect.runPromise(violations(rule, { folders: ['.artifacts/*/**', '**/dist{,/**}'], clean: 'clean' }, { projects }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task-graph', () => {
    it('accepts resolved dependencies, signalled continuous tasks, and apart cached outputs', async () => {
        const served = cleaned('web', {
            'build--vite': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/vite/{projectName}'], dependsOn: ['^build--*'] }),
            'serve--vite': run({ continuous: true, readyWhen: { url: 'http://localhost:4200' } }),
            'test--playwright': run({ dependsOn: [{ target: 'serve--vite', waitFor: 'ready' }] }),
        });
        await expect(checked(served, cleaned('lib', { 'build--tsc': run({ cache: true, outputs: ['{projectRoot}/dist'] }) }))).resolves.toStrictEqual([]);
    });

    it('reports a dependsOn entry naming no target', async () => {
        await expect(checked(cleaned('lib', { test: run({ dependsOn: ['lint', '^compile', 'build--*'] }), 'build--tsc': run() }))).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib:test dependsOn "lint" names no target, name an existing target or drop the entry' },
            { sourceProject: 'lib', message: 'lib:test dependsOn "^compile" names no target, name an existing target or drop the entry' },
        ]);
    });

    it('reports a task cycle and a continuous dependency without a ready signal', async () => {
        await expect(checked(cleaned('lib', { first: run({ dependsOn: ['second'] }), second: run({ dependsOn: ['first'] }), serve: run({ continuous: true }), e2e: run({ dependsOn: ['serve'] }) }))).resolves.toStrictEqual([
            { workspaceViolation: true, message: 'Task cycle lib:first -> lib:second -> lib:first, drop one dependsOn entry of it' },
            { sourceProject: 'lib', message: 'lib:e2e waits for lib:serve to be started, give lib:serve a readyWhen signal and the edge waitFor ready' },
        ]);
    });

    it('reports cached outputs that escape the workspace or the folders, overwrite workspace files, or nest', async () => {
        await expect(
            checked(
                cleaned(
                    'lib',
                    {
                        'build--tsc': run({ cache: true, outputs: ['{workspaceRoot}/out/{projectName}', '{projectRoot}/src', '{workspaceRoot}/../../escaped'] }),
                        'check--tsc': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/tsc/lib'] }),
                        'pack--tsc': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/tsc/lib/packed'] }),
                    },
                    ['src/index.ts'],
                ),
            ),
        ).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib:build--tsc writes out/lib outside .artifacts/*/**, **/dist{,/**}' },
            { sourceProject: 'lib', message: 'lib:build--tsc writes libs/lib/src outside .artifacts/*/**, **/dist{,/**}' },
            { sourceProject: 'lib', message: 'lib:build--tsc writes libs/lib/src over workspace file libs/lib/src, which a replay overwrites' },
            { sourceProject: 'lib', message: 'lib:build--tsc resolves output ../escaped outside the workspace or with a token left' },
            { sourceProject: 'lib', message: 'lib:build--tsc writes ../escaped outside .artifacts/*/**, **/dist{,/**}' },
            { sourceProject: 'lib', message: 'lib:check--tsc writes .artifacts/tsc/lib, which holds .artifacts/tsc/lib/packed of lib:pack--tsc' },
        ]);
    });

    it('reports outputs without a clean target and a cached task depending on clean', async () => {
        await expect(checked(cleaned('lib', { 'build--tsc': run({ cache: true, outputs: ['{projectRoot}/dist'], dependsOn: ['clean'] }) }), project('bare', { targets: { 'build--tsc': run({ outputs: ['{projectRoot}/dist'] }) } }))).resolves.toStrictEqual([
            { sourceProject: 'bare', message: 'bare:build--tsc declares outputs in a project without a clean target, give the project clean' },
            { sourceProject: 'lib', message: 'lib:build--tsc is cached and depends on a clean task, which deletes outputs before every replay, drop that dependsOn entry' },
        ]);
    });
});
