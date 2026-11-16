import type { WritableConformanceTree } from '@nx/conformance';
import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache } from '@nx/conformance/testing';
import { type ProjectConfiguration, writeJson } from '@nx/devkit';
import rule from '@rasm/conformance/nx-schemas';
import { Effect } from 'effect';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = async (data: Omit<ProjectConfiguration, 'root'>, settings: Readonly<Record<string, unknown>> = {}) => {
    const { tree, cleanup } = await createReadOnlyTree((writable: WritableConformanceTree) => Effect.runPromise(Effect.sync(() => writeJson(writable, 'nx.json', settings))));
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(tree, [{ projectNode: { name: 'lib', type: 'lib', data: { root: 'libs/lib', ...data } } }]);
    const { details } = await rule.implementation({ tree, projectGraph, fileMapCache, ruleOptions: undefined });
    return details.violations.map(({ message }) => message);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('nx-schemas', () => {
    it('accepts settings, a project, and target options under the schemas Nx states', async () => {
        await expect(checked({ tags: ['os:all'], targets: { build: { executor: 'nx:run-commands', options: { command: 'tsc' }, configurations: { watch: { command: 'tsc --watch' } } } } }, { parallel: 4 })).resolves.toStrictEqual([]);
    });

    it('reports nx.json settings outside the settings schema', async () => {
        await expect(checked({}, { parallel: 'many' })).resolves.toStrictEqual(['nx.json data/parallel must be number']);
    });

    it('reports a project outside the project schema', async () => {
        await expect(checked({ targets: { build: { executor: 'nx:noop', dependsOn: [{ target: 'build', projects: ['app'], dependencies: true }] } } })).resolves.toStrictEqual([
            'lib data/targets/build/dependsOn/0 must be string, data/targets/build/dependsOn/0 must match exactly one schema in oneOf, data/targets/build/dependsOn/0 must match exactly one schema in oneOf',
        ]);
    });

    it('reports target options and configurations outside the executor schema', async () => {
        await expect(checked({ targets: { build: { executor: 'nx:run-commands', options: { command: 'tsc' }, configurations: { broken: { parallel: 'yes' } } }, missing: { executor: '@rasm/absent:run' } } })).resolves.toStrictEqual([
            "lib:build:broken Property 'parallel' does not match the schema. 'yes' should be a 'boolean'.",
            "lib:missing Unable to resolve @rasm/absent:run.\nCannot find module '@rasm/absent/package.json'\nRequire stack:\n- /__nx_detached_resolver__/x.js",
        ]);
    });
});
