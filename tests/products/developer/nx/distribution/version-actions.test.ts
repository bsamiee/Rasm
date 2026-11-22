import { createNodes } from '@rasm/nx-distribution';
import HostVersionActions from '@rasm/nx-distribution/release/version-actions';
import { it as workspaced } from '@rasm/testing/nx';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { readNxJson } from 'nx/src/config/nx-json';
import { FsTree } from 'nx/src/generators/tree';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const manifests = [
    {
        host: 'Blender',
        file: 'tools/blender_manifest.toml',
        name: 'marker_tools',
        input: '# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.0.0"  # bumped by nx release\nname = "Marker Tools"\n',
        expected: '# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.1.0"  # bumped by nx release\nname = "Marker Tools"\n',
    },
    {
        host: 'Yak',
        file: 'rhino/manifest.yml',
        name: 'marker',
        input: '# Marks points\nname: marker\nversion: 1.0.0 # bumped by nx release\nauthors:\n    - Suite\ndescription: Marks points\n',
        expected: '# Marks points\nname: marker\nversion: 1.1.0 # bumped by nx release\nauthors:\n    - Suite\ndescription: Marks points\n',
    },
    {
        host: 'UXP',
        file: 'panel/manifest.json',
        name: 'com.example.panel',
        input: '{\n\t"manifestVersion": 5,\n\t"id": "com.example.panel",\n\t"name": "Panel",\n\t"version": "1.0.0",\n\t"host": { "app": "PS", "minVersion": "24.0.0" }\n}\n',
        expected: '{\n\t"manifestVersion": 5,\n\t"id": "com.example.panel",\n\t"name": "Panel",\n\t"version": "1.1.0",\n\t"host": { "app": "PS", "minVersion": "24.0.0" }\n}\n',
    },
    {
        host: 'Chrome',
        file: 'clipper/manifest.json',
        name: 'clipper',
        input: '{\n    "manifest_version": 3,\n    "name": "Clipper",\n    "version": "1.0.0",\n    "key": "dGVzdA=="\n}\n',
        expected: '{\n    "manifest_version": 3,\n    "name": "Clipper",\n    "version": "1.1.0",\n    "key": "dGVzdA=="\n}\n',
    },
    {
        host: 'Unreal',
        file: 'Plugins/Marker/Marker.uplugin',
        name: 'Marker',
        input: '{\n    "FileVersion": 3,\n    "Version": 1,\n    "VersionName": "1.0.0",\n    "FriendlyName": "Marker"\n}\n',
        expected: '{\n    "FileVersion": 3,\n    "Version": 2,\n    "VersionName": "1.1.0",\n    "FriendlyName": "Marker"\n}\n',
    },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('versioned', ({ workspace }) => async (file: string, text: string, name: string) => {
    const graph = await workspace.write({ 'nx.json': '{ "release": { "projects": ["*"], "projectsRelationship": "independent" } }', [file]: text }).graph({ createNodes });
    const { nxReleaseConfig } = await createNxReleaseConfig(graph, await createProjectFileMapUsingProjectGraph(graph), readNxJson(workspace.root).release);
    const [node, [group, configured] = []] = [graph.nodes[name], Object.entries(nxReleaseConfig?.groups ?? {})[0]];
    assert(node !== undefined && group !== undefined && configured !== undefined, `${name} is a project of one release group`);
    const { docker: _docker, ...released } = configured;
    const actions = new HostVersionActions({ ...released, name: group, resolvedVersionPlans: false }, node, { ...released.version, versionActionsOptions: node.data.release?.version?.versionActionsOptions ?? {}, manifestRootsToUpdate: [], dockerOptions: {} });
    const tree = new FsTree(workspace.root, false);
    await actions.init(tree);
    return { node, actions, tree };
});

describe('version actions the inferred release names', () => {
    it('names version actions resolving to the host version actions', async ({ versioned }) => {
        const { node } = await versioned(manifests[1].file, manifests[1].input, manifests[1].name);

        const loaded: unknown = await import(String(node.data.release?.version?.versionActions));

        expect(loaded).toHaveProperty('default', HostVersionActions);
    });

    it.for(manifests)('reads the current version of a $host manifest', async ({ file, input, name }, { versioned }) => {
        const { actions, tree } = await versioned(file, input, name);

        await expect(actions.readCurrentVersionFromSourceManifest(tree)).resolves.toStrictEqual({ currentVersion: '1.0.0', manifestPath: file });
    });

    it.for(manifests)('writes the new version into a $host manifest keeping its comments and layout', async ({ file, input, expected, name }, { versioned }) => {
        const { actions, tree } = await versioned(file, input, name);

        await actions.updateProjectVersion(tree, '1.1.0');

        expect(tree.read(file, 'utf-8')).toBe(expected);
    });

    it.for(manifests.filter(({ host }) => host === 'UXP' || host === 'Chrome'))('refuses a prerelease version a $host manifest cannot hold, writing nothing', async ({ file, input, name }, { versioned }) => {
        const { actions, tree } = await versioned(file, input, name);

        await expect(actions.updateProjectVersion(tree, '1.1.0-beta.0')).rejects.toThrow(`${file} takes a major.minor.patch version alone, which 1.1.0-beta.0 is not`);
        expect(tree.read(file, 'utf-8')).toBe(input);
    });

    it.for([
        { host: 'UXP', file: 'panel/manifest.json', name: 'com.example.panel', input: '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.0.0", "version": "5.5.5", "host": { "app": "PS", "minVersion": "24.0.0" } }' },
        { host: 'Chrome', file: 'clipper/manifest.json', name: 'clipper', input: '{ "manifest_version": 3, "name": "Clipper", "version": "1.0.0", "version": "5.5.5" }' },
        { host: 'Unreal', file: 'Plugins/Marker/Marker.uplugin', name: 'Marker', input: '{ "FileVersion": 3, "VersionName": "1.0.0", "VersionName": "5.5.5" }' },
    ])('refuses to version a $host manifest whose version a duplicate key shadows, writing nothing', async ({ file, input, name }, { versioned }) => {
        const { actions, tree } = await versioned(file, input, name);

        await expect(actions.updateProjectVersion(tree, '1.1.0')).rejects.toThrow(`Cannot update ${file}: its version resolves to 5.5.5 instead of 1.1.0`);
        expect(tree.read(file, 'utf-8')).toBe(input);
    });
});
