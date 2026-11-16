import type { WritableConformanceTree } from '@nx/conformance';
import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache } from '@nx/conformance/testing';
import { type ProjectConfiguration, writeJson } from '@nx/devkit';
import rule from '@rasm/conformance/release-projects';
import { Effect } from 'effect';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = async (release: Readonly<Record<string, unknown>>, ...projects: readonly (Pick<ProjectConfiguration, 'tags' | 'release'> & { readonly name: string })[]) => {
    const { tree, cleanup } = await createReadOnlyTree((writable: WritableConformanceTree) => Effect.runPromise(Effect.sync(() => writeJson(writable, 'nx.json', { release }))));
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(
        tree,
        projects.map(({ name, ...data }) => ({ projectNode: { name, type: 'lib', data: { root: `libs/${name}`, ...data } } })),
    );
    const { details } = await rule.implementation({ tree, projectGraph, fileMapCache, ruleOptions: undefined });
    return details.violations.map(({ message }) => message);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('release-projects', () => {
    it('accepts release configuration on exactly the projects release projects and groups select', async () => {
        await expect(checked({ groups: { libraries: { projects: ['tag:release'] }, tools: { projects: 'cli' } } }, { name: 'lib', tags: ['release'], release: {} }, { name: 'cli', release: {} }, { name: 'app', tags: ['os:all'] })).resolves.toStrictEqual([]);
    });

    it('reports a selected project without release configuration and release configuration outside the selection', async () => {
        await expect(checked({ projects: ['tag:release'] }, { name: 'lib', tags: ['release'] }, { name: 'app', release: {} })).resolves.toStrictEqual([
            'lib is a release project without release configuration, give it both or neither',
            'app holds release configuration outside nx.json release projects, give it both or neither',
        ]);
    });

    it('selects every project when release names neither projects nor groups, as nx release does', async () => {
        await expect(checked({}, { name: 'lib', release: {} }, { name: 'app' })).resolves.toStrictEqual(['app is a release project without release configuration, give it both or neither']);
    });
});
