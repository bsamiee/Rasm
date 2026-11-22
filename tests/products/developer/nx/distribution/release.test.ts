import { createNodes } from '@rasm/nx-distribution';
import { it } from '@rasm/testing/nx';
import { Option, Record, Schema } from 'effect';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { readNxJson } from 'nx/src/config/nx-json';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const hosts = {
    'nx.json': '{ "release": { "projects": ["tag:release"], "projectsRelationship": "independent" } }',
    'tools/blender_manifest.toml': 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n',
    'rhino/manifest.yml': 'name: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\n',
    'panel/manifest.json': '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'clipper/manifest.json': '{ "manifest_version": 3, "name": "Clipper", "version": "1.2.3" }',
    'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n',
    'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n',
    'Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
    'Game/Plugins/Draft/Draft.uplugin': '{ "FileVersion": 3, "Version": 1 }\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Published = Schema.Struct({ credential: Schema.String });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('nx release', () => {
    it('releases each manifest stating a version through the release tag nx.json selects', async ({ workspace }) => {
        const graph = await workspace.write(hosts).graph({ createNodes });

        const { nxReleaseConfig } = await createNxReleaseConfig(graph, await createProjectFileMapUsingProjectGraph(graph), readNxJson(workspace.root).release);

        expect(
            Object.values(nxReleaseConfig?.groups ?? {})
                .flatMap(({ projects }) => projects)
                .toSorted(),
        ).toStrictEqual(['Marker', 'clipper', 'com.example.panel', 'marker', 'marker_tools']);
    });

    it('publishes to each store taking uploads under the variable release.yml sets, and to none for Adobe', async ({ workspace }) => {
        const graph = await workspace.write(hosts).graph({ createNodes });

        const credentials = Record.getSomes(Record.map(graph.nodes, ({ data: { targets = {} } }) => Schema.decodeUnknownOption(Published)(targets['nx-release-publish']?.options)));

        expect(credentials).toStrictEqual({ clipper: { credential: 'CHROME_WEB_STORE_TOKEN' }, marker: { credential: 'YAK_TOKEN' }, marker_tools: { credential: 'BLENDER_EXTENSIONS_TOKEN' } });
    });

    it('reads the credential of a store from the variable storeCredentials names for its technology', async ({ workspace }) => {
        const graph = await workspace.write(hosts).graph({ createNodes }, { storeCredentials: { yak: 'RHINO_PACKAGE_TOKEN' } });

        expect(['marker', 'marker_tools'].map((name) => Schema.decodeUnknownOption(Published)(graph.nodes[name]?.data.targets?.['nx-release-publish']?.options))).toStrictEqual([Option.some({ credential: 'RHINO_PACKAGE_TOKEN' }), Option.some({ credential: 'BLENDER_EXTENSIONS_TOKEN' })]);
    });
});
