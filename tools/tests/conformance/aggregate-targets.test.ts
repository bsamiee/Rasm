import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/aggregate-targets';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = async (targets: Readonly<Record<string, TargetConfiguration>>) => {
    const { tree, cleanup } = await createReadOnlyTree();
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(tree, [
        { projectNode: { name: 'lib', type: 'lib', data: { root: 'libs/lib', targets } } },
        { projectNode: { name: 'app', type: 'app', data: { root: 'apps/app', targets: { test: { executor: 'nx:noop', dependsOn: ['test--*'] }, 'test--vitest': { executor: 'nx:run-commands' } } } } },
    ]);
    const { details } = await rule.implementation({ tree, projectGraph, fileMapCache, ruleOptions: { separator: '--' } });
    return details.violations.map(({ message }) => message);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('aggregate-targets', () => {
    it('accepts an nx:noop target over its tool targets stating their inputs', async () => {
        const inputs = ['default', { externalDependencies: ['typescript'] }];
        await expect(
            checked({
                build: { executor: 'nx:noop', dependsOn: ['build--*'], inputs: [...inputs, { dependentTasksOutputFiles: '**/*.d.ts', transitive: true }] },
                'build--tsc': { executor: 'nx:run-commands', inputs: [...inputs, { dependentTasksOutputFiles: '**/*.d.ts' }] },
            }),
        ).resolves.toStrictEqual([]);
    });

    it('reports a tool target without its aggregate and an aggregate without tool targets', async () => {
        await expect(checked({ 'build--tsc': { executor: 'nx:run-commands' }, test: { executor: 'nx:noop', dependsOn: ['test--*'] } })).resolves.toStrictEqual(['lib holds build--tsc and no build target, give it build over them', 'lib test runs no test--<tool> target, drop test']);
    });

    it('reports an aggregate that runs a command, depends on other targets, or lacks an input of its tool targets', async () => {
        await expect(checked({ build: { executor: 'nx:run-commands', dependsOn: ['^build'], inputs: ['default'] }, 'build--tsc': { executor: 'nx:run-commands', inputs: ['default', '^production'] } })).resolves.toStrictEqual([
            'lib build runs nx:run-commands after ["^build"], make it nx:noop depending on build--*',
            'lib build lacks input "^production" of build--tsc, add it',
        ]);
    });
});
