import type { ProjectGraph, ProjectGraphProjectNode, Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import CmakeVersionActions from '@rasm/nx-cmake/release/version-actions';
import type { VersionActions } from 'nx/release';
import type { ReleaseGroupWithName } from 'nx/src/command-line/release/config/filter-release-groups';
import type { FinalConfigForProject } from 'nx/src/command-line/release/utils/release-graph';
import { describe, expect, it } from 'vitest';

const node: ProjectGraphProjectNode = { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib' } };

const createProjectGraph = (): ProjectGraph => ({
    nodes: {
        'my-lib': node,
        dependency: {
            name: 'dependency',
            type: 'lib',
            data: { root: 'packages/dependency' },
        },
    },
    dependencies: {},
});

const createVersionActions = async (tree: Tree): Promise<VersionActions> => {
    const releaseGroup: ReleaseGroupWithName = { name: 'release-group', projects: ['my-lib'], projectsRelationship: 'independent', resolvedVersionPlans: false };
    const finalConfigForProject: FinalConfigForProject = {
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
    const versionActions = new CmakeVersionActions(releaseGroup, node, finalConfigForProject);
    await versionActions.init(tree);
    return versionActions;
};

describe('CmakeVersionActions', () => {
    it('preserves CMakeLists.txt formatting when updating versions and dependencies', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(
            'packages/my-lib/CMakeLists.txt',
            `cmake_minimum_required(VERSION 4.5)
project(my_lib
    VERSION 1.0.0
    LANGUAGES CXX) # the library
find_package(dependency 1.0.0 REQUIRED)
`,
        );
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');
        await versionActions.updateProjectDependencies(tree, createProjectGraph(), { dependency: '2.0.0' });

        expect(tree.read('packages/my-lib/CMakeLists.txt', 'utf-8')).toBe(`cmake_minimum_required(VERSION 4.5)
project(my_lib
    VERSION 1.1.0
    LANGUAGES CXX) # the library
find_package(dependency 1.0.0 REQUIRED)
`);
    });

    it.each([
        {
            name: 'spaces and LF endings',
            input: 'project(my_lib\n    VERSION 1.0.0\n    LANGUAGES CXX)\n',
            expected: 'project(my_lib\n    VERSION 1.1.0\n    LANGUAGES CXX)\n',
        },
        {
            name: 'tabs and CRLF endings',
            input: 'project(my_lib\r\n\tVERSION 1.0.0\r\n\tLANGUAGES CXX)\r\n',
            expected: 'project(my_lib\r\n\tVERSION 1.1.0\r\n\tLANGUAGES CXX)\r\n',
        },
    ])('formats an updated version using $name', async ({ input, expected }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/my-lib/CMakeLists.txt', input);
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read('packages/my-lib/CMakeLists.txt', 'utf-8')).toBe(expected);
    });

    it('rejects a malformed manifest without changing it', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const malformedManifest = `project(my_lib LANGUAGES CXX)
`;
        tree.write('packages/my-lib/CMakeLists.txt', malformedManifest);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow('packages/my-lib/CMakeLists.txt states no VERSION in a project() call, which nx release reads and writes');
        expect(tree.read('packages/my-lib/CMakeLists.txt', 'utf-8')).toBe(malformedManifest);
    });

    it('rejects a prerelease version, which project(VERSION) cannot state, without changing the manifest', async () => {
        const tree = createTreeWithEmptyWorkspace();
        const manifest = `project(my_lib VERSION 1.0.0 LANGUAGES CXX)
`;
        tree.write('packages/my-lib/CMakeLists.txt', manifest);
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0-rc.1')).rejects.toThrow('packages/my-lib/CMakeLists.txt takes a major.minor.patch version alone in project(VERSION), which 1.1.0-rc.1 is not');
        expect(tree.read('packages/my-lib/CMakeLists.txt', 'utf-8')).toBe(manifest);
    });

    it('preserves comments and quoted arguments in a valid manifest', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(
            'packages/my-lib/CMakeLists.txt',
            `project(my_lib # VERSION 9.9.9 was the old line
    #[[ VERSION 5.5.5 ]]
    VERSION "1.0.0"
    LANGUAGES CXX)
`,
        );
        const versionActions = await createVersionActions(tree);

        await versionActions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read('packages/my-lib/CMakeLists.txt', 'utf-8')).toBe(`project(my_lib # VERSION 9.9.9 was the old line
    #[[ VERSION 5.5.5 ]]
    VERSION "1.1.0"
    LANGUAGES CXX)
`);
    });

    it('reads the current version from the VERSION of the project() call', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/my-lib/CMakeLists.txt', 'cmake_minimum_required(VERSION 4.5)\nproject(my_lib VERSION 1.0.0 LANGUAGES CXX)\n');
        const versionActions = await createVersionActions(tree);

        await expect(versionActions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: '1.0.0', manifestPath: 'packages/my-lib/CMakeLists.txt' });
    });
});
