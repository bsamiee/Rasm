import type { ProjectGraphProjectNode, Tree } from '@nx/devkit';
import { createTreeWithEmptyWorkspace } from '@nx/devkit/testing';
import HostVersionActions from '@rasm/nx-distribution/release/version-actions';
import type { VersionActions } from 'nx/release';
import { createNxReleaseConfig, IMPLICIT_DEFAULT_RELEASE_GROUP } from 'nx/src/command-line/release/config/config';
import { assert, describe, expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const manifests = [
    {
        manifest: 'blender_manifest.toml',
        input: '# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.0.0"  # bumped by nx release\nname = "Marker Tools"\n',
        expected: '# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.1.0"  # bumped by nx release\nname = "Marker Tools"\n',
        malformed: 'id = "marker_tools\nversion = "1.0.0"\n',
        shadowed: 'id = "marker_tools"\nversion = "1.0.0"\nversion = "5.5.5"\n',
    },
    {
        manifest: 'manifest.yml',
        input: '# Marks points\nname: marker\nversion: 1.0.0 # bumped by nx release\nauthors:\n    - Suite\ndescription: Marks points\n',
        expected: '# Marks points\nname: marker\nversion: 1.1.0 # bumped by nx release\nauthors:\n    - Suite\ndescription: Marks points\n',
        malformed: 'name: marker\nversion: [1.0.0\n',
        shadowed: 'name: marker\nversion: 1.0.0\nversion: 5.5.5\nauthors: [Suite]\ndescription: Marks points\n',
    },
    {
        manifest: 'manifest.json',
        input: '{\n\t"manifestVersion": 5,\n\t"id": "com.example.panel",\n\t"name": "Panel",\n\t"version": "1.0.0",\n\t"host": { "app": "PS", "minVersion": "24.0.0" }\n}\n',
        expected: '{\n\t"manifestVersion": 5,\n\t"id": "com.example.panel",\n\t"name": "Panel",\n\t"version": "1.1.0",\n\t"host": { "app": "PS", "minVersion": "24.0.0" }\n}\n',
        malformed: '{\n  "manifestVersion": 5,\n  "id": "com.example.panel",\n  "version": "1.0.0"\n',
        shadowed: '{\n  "manifestVersion": 5,\n  "id": "com.example.panel",\n  "name": "Panel",\n  "version": "1.0.0",\n  "version": "5.5.5",\n  "host": { "app": "PS", "minVersion": "24.0.0" }\n}\n',
    },
    {
        manifest: 'Marker.uplugin',
        input: '{\n    "FileVersion": 3,\n    "Version": 1,\n    "VersionName": "1.0.0",\n    "FriendlyName": "Marker"\n}\n',
        expected: '{\n    "FileVersion": 3,\n    "Version": 2,\n    "VersionName": "1.1.0",\n    "FriendlyName": "Marker"\n}\n',
        malformed: '{\n    "FileVersion": 3,\n    "VersionName": "1.0.0"\n',
        shadowed: '{\n    "FileVersion": 3,\n    "Version": 1,\n    "VersionName": "1.0.0",\n    "VersionName": "5.5.5"\n}\n',
    },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const createVersionActions = async (tree: Tree, manifest: string): Promise<VersionActions> => {
    const node: ProjectGraphProjectNode = { name: 'my-lib', type: 'lib', data: { root: 'packages/my-lib' } };
    const { nxReleaseConfig } = await createNxReleaseConfig({ nodes: { [node.name]: node }, dependencies: {} }, {}, { projects: [node.name], projectsRelationship: 'independent' });
    const group = nxReleaseConfig?.groups?.[IMPLICIT_DEFAULT_RELEASE_GROUP];
    assert(group, `nx release config holds the ${IMPLICIT_DEFAULT_RELEASE_GROUP} group`);
    const { docker: _docker, ...configured } = group;
    const versionActions = new HostVersionActions({ ...configured, name: IMPLICIT_DEFAULT_RELEASE_GROUP, resolvedVersionPlans: false }, node, { ...configured.version, versionActionsOptions: { manifest }, manifestRootsToUpdate: [], dockerOptions: {} });
    await versionActions.init(tree);
    return versionActions;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('readCurrentVersionFromRegistry', () => {
    it('resolves no registry version, so nx release falls back to the manifest on disk', async () => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write('packages/my-lib/manifest.yml', manifests[1].input);
        const versionActions = await createVersionActions(tree, 'manifest.yml');

        await expect(versionActions.readCurrentVersionFromRegistry(tree, {})).resolves.toBeNull();
    });
});

describe('HostVersionActions', () => {
    it.each(manifests)('reads the current version from $manifest', async ({ manifest, input }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(`packages/my-lib/${manifest}`, input);
        const versionActions = await createVersionActions(tree, manifest);

        await expect(versionActions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: '1.0.0', manifestPath: `packages/my-lib/${manifest}` });
    });

    it.each(manifests)('preserves $manifest formatting and comments when updating versions', async ({ manifest, input, expected }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(`packages/my-lib/${manifest}`, input);
        const versionActions = await createVersionActions(tree, manifest);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).resolves.toStrictEqual([`New version 1.1.0 written to manifest: packages/my-lib/${manifest}`]);

        expect(tree.read(`packages/my-lib/${manifest}`, 'utf-8')).toBe(expected);
    });

    it.each(manifests)('rejects a malformed $manifest without changing it', async ({ manifest, malformed }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(`packages/my-lib/${manifest}`, malformed);
        const versionActions = await createVersionActions(tree, manifest);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow(`packages/my-lib/${manifest}`);
        expect(tree.read(`packages/my-lib/${manifest}`, 'utf-8')).toBe(malformed);
    });

    it.each(manifests)('rejects a version update of $manifest shadowed by a duplicate key', async ({ manifest, shadowed }) => {
        const tree = createTreeWithEmptyWorkspace();
        tree.write(`packages/my-lib/${manifest}`, shadowed);
        const versionActions = await createVersionActions(tree, manifest);

        await expect(versionActions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow(`packages/my-lib/${manifest}`);
        expect(tree.read(`packages/my-lib/${manifest}`, 'utf-8')).toBe(shadowed);
    });
});
