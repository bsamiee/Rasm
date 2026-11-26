import { assert, describe, expect, it } from '@effect/vitest';
import type { ProjectGraphProjectNode } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import CmakeVersionActions from '@rasm/nx-cmake/release/version-actions';
import { Effect, Schema } from 'effect';
import type { ReleaseGroupWithName } from 'nx/src/command-line/release/config/filter-release-groups';
import type { FinalConfigForProject } from 'nx/src/command-line/release/utils/release-graph';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'packages/my-lib/CMakeLists.txt';
const node: ProjectGraphProjectNode = { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib' } };
const group: ReleaseGroupWithName = { name: 'release-group', projects: ['my-lib'], projectsRelationship: 'independent', resolvedVersionPlans: false };
const settled: FinalConfigForProject = {
    specifierSource: 'prompt',
    currentVersionResolver: 'disk',
    currentVersionResolverMetadata: {},
    fallbackCurrentVersionResolver: 'disk',
    versionPrefix: 'auto',
    preserveLocalDependencyProtocols: false,
    preserveMatchingDependencyRanges: false,
    adjustSemverBumpsForZeroMajorVersion: false,
    applyPreidToDependents: false,
    versionActionsOptions: {},
    manifestRootsToUpdate: [],
    dockerOptions: {},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const manifested = async (text: string) => {
    const tree = createTreeWithEmptyWorkspace();
    tree.write(MANIFEST, text);
    const actions = new CmakeVersionActions(group, node, settled);
    await actions.init(tree);
    return { tree, actions };
};

describe('project(VERSION) in CMakeLists.txt', () => {
    it.for([
        {
            case: 'a multi-line call after another command taking VERSION',
            text: 'cmake_minimum_required(VERSION 4.5)\nproject(my_lib\n    VERSION 1.0.0\n    LANGUAGES CXX) # the library\n',
            written: 'cmake_minimum_required(VERSION 4.5)\nproject(my_lib\n    VERSION 1.1.0\n    LANGUAGES CXX) # the library\n',
        },
        { case: 'tabs and CRLF line endings', text: 'project(my_lib\r\n\tVERSION 1.0.0\r\n\tLANGUAGES CXX)\r\n', written: 'project(my_lib\r\n\tVERSION 1.1.0\r\n\tLANGUAGES CXX)\r\n' },
        {
            case: 'a quoted argument after comments naming VERSION',
            text: 'project(my_lib # VERSION 9.9.9 was the old line\n    #[[ VERSION 5.5.5 ]]\n    VERSION "1.0.0"\n    LANGUAGES CXX)\n',
            written: 'project(my_lib # VERSION 9.9.9 was the old line\n    #[[ VERSION 5.5.5 ]]\n    VERSION "1.1.0"\n    LANGUAGES CXX)\n',
        },
    ])('rewrites the VERSION argument alone in $case', async ({ text, written }) => {
        const { tree, actions } = await manifested(text);

        await actions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read(MANIFEST, 'utf-8')).toBe(written);
    });

    it.for([
        { case: 'a project() call stating no VERSION', text: 'project(my_lib LANGUAGES CXX)\n', version: '1.1.0', refusal: `${MANIFEST} states no VERSION in a project() call, which nx release reads and writes` },
        { case: 'a prerelease version project(VERSION) cannot state', text: 'project(my_lib VERSION 1.0.0 LANGUAGES CXX)\n', version: '1.1.0-rc.1', refusal: `${MANIFEST} takes a major.minor.patch version alone in project(VERSION), which 1.1.0-rc.1 is not` },
    ])('refuses $case, leaving the manifest as it was', async ({ text, version, refusal }) => {
        const { tree, actions } = await manifested(text);

        await expect(actions.updateProjectVersion(tree, version)).rejects.toThrow(refusal);
        expect(tree.read(MANIFEST, 'utf-8')).toBe(text);
    });

    it.effect.prop(
        'writes any major.minor.patch version into project(VERSION) and reads that version back',
        [Schema.Natural, Schema.Natural, Schema.Natural],
        Effect.fnUntraced(function* (parts) {
            const version = parts.join('.');
            const { tree, actions } = yield* Effect.promise(() => manifested('project(my_lib VERSION 0.0.1 LANGUAGES CXX)\n'));

            yield* Effect.promise(() => actions.updateProjectVersion(tree, version));

            assert.deepStrictEqual([tree.read(MANIFEST, 'utf-8'), yield* Effect.promise(() => actions.readCurrentVersionFromSourceManifest(tree))], [`project(my_lib VERSION ${version} LANGUAGES CXX)\n`, { currentVersion: version, manifestPath: MANIFEST }]);
        }),
    );

    it('reads the current version from the VERSION of the project() call, not of an earlier command', async () => {
        const { tree, actions } = await manifested('cmake_minimum_required(VERSION 4.5)\nproject(my_lib VERSION 2.3.4 LANGUAGES CXX)\n');

        await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: '2.3.4', manifestPath: MANIFEST });
    });
});
