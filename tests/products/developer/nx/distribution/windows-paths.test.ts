import { createNodes } from '@rasm/nx-distribution';
import { it, unportable } from '@rasm/testing/nx';
import { Option, Schema } from 'effect';
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

const files = {
    'nx.json': '{ "release": { "projects": ["*"], "projectsRelationship": "independent" } }',
    'products/apps/rhino/Arches/manifest.yml': 'name: arches\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Arches\n',
    'products/apps/adobe/Bridge/manifest.json': '{ "manifestVersion": 5, "id": "com.example.bridge", "name": "Bridge", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'products/web/site/project.json': '{ "name": "site" }',
    'products/web/site/.github/workflows/ci.yml': 'on: push\n',
    'products/engines/Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n',
    'products/games/Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../../engines/Engine", "Plugins": [{ "Name": "Marker", "Enabled": true }, { "Name": "Tools", "Enabled": true }] }\n',
    'products/games/Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
    'products/plugins/Tools/Tools.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "2.0.0" }\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ descriptor: Schema.String, dependencies: Schema.Array(Schema.String) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('@rasm/nx-distribution under Windows path separators', () => {
    it('states every root, input, and output as a workspace-relative posix path', async ({ workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });

        expect(unportable(graph)).toStrictEqual([]);
        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['Game', 'Marker', 'Tools', 'arches', 'com.example.bridge', 'site']);
    });

    it('links a nested project to the plug-ins it enables inside and outside its folders', async ({ workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });

        expect(graph.dependencies['Game']?.map(({ target }) => target).toSorted()).toStrictEqual(['Marker', 'Tools']);
    });

    it('builds a nested project under its parent-relative engine and names each plug-in manifest it loads by workspace path', async ({ workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });
        const stated = Option.map(Schema.decodeUnknownOption(Built)(graph.nodes['Game']?.data.targets?.['build--unreal']?.options), ({ descriptor, dependencies }) => ({ descriptor, dependencies: dependencies.toSorted() }));

        expect(stated).toStrictEqual(Option.some({ descriptor: 'products/games/Game/Game.uproject', dependencies: ['products/games/Game/Plugins/Marker/Marker.uplugin', 'products/plugins/Tools/Tools.uplugin'] }));
    });
});
