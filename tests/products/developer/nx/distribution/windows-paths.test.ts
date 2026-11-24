import { createNodes } from '@rasm/nx-distribution';
import { it, taskGraph, unportable } from '@rasm/testing/nx';
import { Option, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENGINE = { 'products/engines/Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n' };
const files = {
    'nx.json': '{ "release": { "projects": ["*"], "projectsRelationship": "independent" } }',
    'package.json': '{ "name": "repo" }',
    '.github/workflows/ci.yml': 'on: push\n',
    '.config/mise.toml': '[tools]\n',
    'products/apps/rhino/Arches/manifest.yml': 'name: arches\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Arches\n',
    'products/apps/adobe/Bridge/manifest.json': '{ "manifestVersion": 5, "id": "com.example.bridge", "name": "Bridge", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'products/web/site/project.json': '{ "name": "site" }',
    'products/games/Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../../engines/Engine", "Plugins": [{ "Name": "Marker", "Enabled": true }, { "Name": "Tools", "Enabled": true }, { "Name": "Retired", "Enabled": false }] }\n',
    'products/games/Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
    'products/games/Game/Plugins/Retired/Retired.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "0.9.0" }\n',
    'products/plugins/Tools/Tools.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "2.0.0" }\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ descriptor: Schema.String, dependencies: Schema.Array(Schema.String) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('@rasm/nx-distribution under Windows path separators', () => {
    it('states every root, input, and output as a workspace-relative posix path', async ({ workspace }) => {
        const graph = await workspace.write({ ...files, ...ENGINE }).graph({ createNodes });

        expect(unportable(graph)).toStrictEqual([]);
        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['Game', 'Marker', 'Retired', 'Tools', 'arches', 'com.example.bridge', 'repo', 'site']);
    });

    it('builds a nested project under its parent-relative engine, naming each plug-in manifest it loads by workspace path, after installing the one outside its folders, linking only the plug-ins it enables', async ({ workspace }) => {
        const graph = await workspace.write({ ...files, ...ENGINE }).graph({ createNodes });
        const stated = Option.map(Schema.decodeUnknownOption(Built)(graph.nodes['Game']?.data.targets?.['build--unreal']?.options), ({ descriptor, dependencies }) => ({ descriptor, dependencies: dependencies.toSorted() }));

        const { dependencies } = taskGraph(graph, ['Game'], ['build--unreal']);

        expect([graph.dependencies['Game']?.map(({ target }) => target).toSorted(), stated, dependencies['Game:build--unreal']?.map(({ id }) => id)]).toStrictEqual([
            ['Marker', 'Tools'],
            Option.some({ descriptor: 'products/games/Game/Game.uproject', dependencies: ['products/games/Game/Plugins/Marker/Marker.uplugin', 'products/plugins/Tools/Tools.uplugin'] }),
            ['Tools:install'],
        ]);
    });

    it('names no folder of this machine in the targets of any project, the engine an Unreal project associates included', async ({ workspace }) => {
        const graph = await workspace.write({ ...files, ...ENGINE }).graph({ createNodes });

        const named = Record.keys(
            Record.filter(
                Record.map(graph.nodes, ({ data }) => JSON.stringify(data.targets)),
                (stated) => stated.includes(workspace.root),
            ),
        );

        expect(named).toStrictEqual([]);
    });
});
