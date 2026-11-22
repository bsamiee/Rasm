import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/aggregate-targets';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, violations } from '#/workspace.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = (targets: Readonly<Record<string, TargetConfiguration>>) =>
    Effect.runPromise(violations(rule, { separator: '--' }, { projects: [project('lib', { targets }), project('app', { targets: { test: { executor: 'nx:noop', dependsOn: [{ target: 'test--*', params: 'forward' }] }, 'test--vitest': { executor: 'nx:run-commands' } } })] }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('aggregate-targets', () => {
    it('accepts an nx:noop aggregate over its tool targets, naming its own project or none, stating their inputs', async () => {
        const inputs = ['default', { externalDependencies: ['typescript'] }];
        await expect(
            checked({
                build: { executor: 'nx:noop', dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: [...inputs, { dependentTasksOutputFiles: '**/*.d.ts', transitive: true }] },
                'build--tsc': { executor: 'nx:run-commands', inputs: [...inputs, { dependentTasksOutputFiles: '**/*.d.ts' }] },
                check: { executor: 'nx:noop', dependsOn: [{ target: 'check--*', params: 'forward', projects: ['lib'] }] },
                'check--tsc': { executor: 'nx:run-commands' },
            }),
        ).resolves.toStrictEqual([]);
    });

    it('reports a tool target without its aggregate and an aggregate without tool targets', async () => {
        await expect(checked({ 'build--tsc': { executor: 'nx:run-commands' }, test: { executor: 'nx:noop', dependsOn: [{ target: 'test--*', params: 'forward' }] } })).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib holds build--tsc and no build target, give it build over them' },
            { sourceProject: 'lib', message: 'lib test runs no test--<tool> target, drop test' },
        ]);
    });

    it('reports an aggregate that runs a command, depends on other targets, or lacks an input of its tool targets', async () => {
        await expect(checked({ build: { executor: 'nx:run-commands', dependsOn: ['^build'], inputs: ['default'] }, 'build--tsc': { executor: 'nx:run-commands', inputs: ['default', '^production'] } })).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib build runs nx:run-commands after ["^build"], make it nx:noop depending on build--* with its parameters forwarded' },
            { sourceProject: 'lib', message: 'lib build lacks input "^production" of build--tsc, add it' },
            { sourceProject: 'lib', message: 'lib build depends on build, which forwards the parameters of the run to its tool targets, depend on build--* in its place' },
        ]);
    });

    it('reports a target depending on an aggregate of its own project, a dependency, or a named project', async () => {
        await expect(
            checked({
                check: { executor: 'nx:noop', dependsOn: [{ target: 'check--*', params: 'forward' }] },
                'check--tsc': { executor: 'nx:run-commands', dependsOn: ['^test', { target: 'check', params: 'ignore' }, 'app:test', '^test--*'] },
            }),
        ).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib check--tsc depends on test, which forwards the parameters of the run to its tool targets, depend on test--* in its place' },
            { sourceProject: 'lib', message: 'lib check--tsc depends on check, which forwards the parameters of the run to its tool targets, depend on check--* in its place' },
            { sourceProject: 'lib', message: 'lib check--tsc depends on test, which forwards the parameters of the run to its tool targets, depend on test--* in its place' },
        ]);
    });
});
