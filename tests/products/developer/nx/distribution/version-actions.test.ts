import { createNodes } from '@rasm/nx-distribution';
import HostVersionActions from '@rasm/nx-distribution/release/version-actions';
import { it as workspaced } from '@rasm/testing/nx';
import { asyncProperty, assert as holds, nat, tuple } from 'fast-check';
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
    { host: 'Blender', file: 'tools/blender_manifest.toml', name: 'marker_tools', text: (version: string) => `# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "${version}"  # bumped by nx release\nname = "Marker Tools"\n` },
    { host: 'Yak', file: 'rhino/manifest.yml', name: 'marker', text: (version: string) => `# Marks points\nname: marker\nversion: ${version} # bumped by nx release\nauthors:\n    - Suite\ndescription: Marks points\n` },
    { host: 'UXP', file: 'panel/manifest.json', name: 'com.example.panel', text: (version: string) => `{\n\t"manifestVersion": 5,\n\t"id": "com.example.panel",\n\t"name": "Panel",\n\t"version": "${version}",\n\t"host": { "app": "PS", "minVersion": "24.0.0" }\n}\n` },
    { host: 'Chrome', file: 'clipper/manifest.json', name: 'clipper', text: (version: string) => `{\n    "manifest_version": 3,\n    "name": "Clipper",\n    "version": "${version}",\n    "key": "dGVzdA=="\n}\n` },
    { host: 'Unreal', file: 'Plugins/Marker/Marker.uplugin', name: 'Marker', text: (version: string, build = 1) => `{\n    "FileVersion": 3,\n    "Version": ${build},\n    "VersionName": "${version}",\n    "FriendlyName": "Marker"\n}\n` },
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
    return { actions, tree };
});

describe('version actions the inferred release names', () => {
    it.for(manifests)('rewrites the version of a $host manifest and nothing else, reading back the version it wrote', async ({ file, name, text }, { versioned }) => {
        const { actions, tree } = await versioned(file, text('1.0.0'), name);

        await holds(
            asyncProperty(tuple(nat(40), nat(40), nat(400)), async ([major, minor, patch]) => {
                const version = `${major}.${minor}.${patch}`;
                tree.write(file, text('1.0.0'));
                await actions.updateProjectVersion(tree, version);
                expect([tree.read(file, 'utf-8'), await actions.readCurrentVersionFromSourceManifest(tree)]).toStrictEqual([text(version, 2), { currentVersion: version, manifestPath: file }]);
            }),
            { numRuns: 12 },
        );
    });

    it.for(manifests.filter(({ host }) => host === 'UXP' || host === 'Chrome'))('refuses a prerelease version a $host manifest cannot hold, writing nothing', async ({ file, text, name }, { versioned }) => {
        const { actions, tree } = await versioned(file, text('1.0.0'), name);

        await expect(actions.updateProjectVersion(tree, '1.1.0-beta.0')).rejects.toThrow(`${file} takes a major.minor.patch version alone, which 1.1.0-beta.0 is not`);
        expect(tree.read(file, 'utf-8')).toBe(text('1.0.0'));
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
