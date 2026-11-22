import { join } from 'node:path';
import { createNodes } from '@rasm/nx-distribution';
import { it, pathed, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => ({
    ...(await actual()),
    get platform() {
        return host.platform;
    },
}));
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const host = vi.hoisted((): { platform: NodeJS.Platform } => ({ platform: 'darwin' }));
const files = {
    'rhino/manifest.yml': 'name: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\n',
    'panel/manifest.json': '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n',
    'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n',
    'Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('platforms a host runs on', () => {
    it.for([
        { platform: 'darwin', installs: ['com.example.panel:install--uxp', 'marker:install--yak'] },
        { platform: 'win32', installs: ['com.example.panel:install--uxp', 'marker:install--yak'] },
        { platform: 'linux', installs: [] },
    ] as const)('infers the Rhino and Adobe installs on $platform as $installs', async ({ platform, installs }, { workspace }) => {
        host.platform = platform;
        const graph = await workspace.write(files).graph({ createNodes });

        const inferred = ['com.example.panel:install--uxp', 'marker:install--yak'].filter((id) => id.slice(id.indexOf(':') + 1) in (graph.nodes[id.slice(0, id.indexOf(':'))]?.data.targets ?? {}));

        expect(inferred).toStrictEqual(installs);
    });

    it.for(['darwin', 'linux', 'win32'] as const)('infers unreal builds on %s only where the os tags of the project name it, each project tagged host:unreal', async (platform, { workspace }) => {
        host.platform = platform;
        const graph = await workspace.write(files).graph({ createNodes });

        const built = ['Game', 'Marker'].map((name) => ({ name, tags: graph.nodes[name]?.data.tags?.toSorted(), building: 'build--unreal' in (graph.nodes[name]?.data.targets ?? {}) }));

        expect(built).toStrictEqual(['Game', 'Marker'].map((name) => ({ name, tags: ['host:unreal', 'os:darwin', 'os:linux'], building: platform !== 'win32' })));
    });

    it.for([
        { case: 'on PATH', scripts: { blender: 'exit 0' }, installs: true },
        { case: 'absent', scripts: {}, installs: false },
    ])('infers install--blender where blender is $case', async ({ scripts, installs }, { workspace }) => {
        host.platform = 'darwin';
        pathed(join(workspace.root, '.bin'), scripts);
        const graph = await workspace.write({ 'tools/blender_manifest.toml': 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n' }).graph({ createNodes });

        expect('install--blender' in (graph.nodes['marker_tools']?.data.targets ?? {})).toBe(installs);
    });

    it('installs an Adobe plug-in after its other builds and without packing it', async ({ workspace }) => {
        host.platform = 'darwin';
        const graph = await workspace.write({ ...files, 'panel/project.json': '{ "name": "com.example.panel", "targets": { "build--vite": { "executor": "nx:noop", "outputs": ["{workspaceRoot}/.artifacts/vite/panel"] } } }' }).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['com.example.panel'], ['install--uxp']);

        expect(dependencies['com.example.panel:install--uxp']?.map(({ id }) => id)).toStrictEqual(['com.example.panel:build--vite']);
    });
});
