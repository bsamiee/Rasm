import { join } from 'node:path';
import { createNodes } from '@rasm/nx-distribution';
import { execute as install } from '@rasm/nx-distribution/executors/install';
import { execute as unreal } from '@rasm/nx-distribution/executors/unreal-build';
import { it, pathed, targetOptions, taskGraph } from '@rasm/testing/nx';
import { Record, Schema } from 'effect';
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
const ENGINE = { 'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n' };
const files = {
    'rhino/manifest.yml': 'name: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\n',
    'panel/manifest.json': '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Plugins": [{ "Name": "Lights", "Enabled": true }] }\n',
    'Game/Plugins/Lights/Lights.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('platforms a host runs on', () => {
    it('infers the same targets on every platform, with or without the engine an Unreal project associates', async ({ workspace }) => {
        const targets = async (platform: NodeJS.Platform) => {
            host.platform = platform;
            const { nodes } = await workspace.graph({ createNodes });
            return Record.map(nodes, ({ data }) => data.targets ?? {});
        };
        workspace.write({ ...files, ...ENGINE });
        const engined = await targets('darwin');
        workspace.remove(...Object.keys(ENGINE));

        const [darwin, linux, windows] = [await targets('darwin'), await targets('linux'), await targets('win32')];

        expect(['Game:build--unreal', 'Lights:pack--unreal', 'com.example.panel:install--uxp', 'marker:install--yak'].filter((id) => id.slice(id.indexOf(':') + 1) in (engined[id.slice(0, id.indexOf(':'))] ?? {}))).toHaveLength(4);
        expect([darwin, linux, windows]).toStrictEqual([engined, engined, engined]);
    });

    it('refuses an Adobe install on linux, naming the platforms its host runs on', async ({ workspace }) => {
        host.platform = 'linux';
        const graph = await workspace.write(files).graph({ createNodes });

        await expect(install({}, workspace.context(graph, 'com.example.panel', 'install--uxp'))).rejects.toThrow(/: uxp installs on darwin, win32, and this machine runs linux$/u);
    });

    it('refuses to build an Unreal project on a machine without the engine it associates, naming where that engine should be', async ({ workspace }) => {
        host.platform = 'darwin';
        const graph = await workspace.write(files).graph({ createNodes });

        await expect(unreal({ users: [], operation: 'build', ...(await targetOptions(Schema.Record(Schema.String, Schema.Unknown), graph, 'Game', 'build--unreal')) }, workspace.context(graph, 'Game', 'build--unreal'))).rejects.toThrow(
            new RegExp(`^${RegExp.escape(join(workspace.root, 'Game/Game.uproject'))} associates the engine at ${RegExp.escape(join(workspace.root, 'Engine'))}, which holds no Engine/Build/Build\\.version$`, 'u'),
        );
    });

    it.for([
        { case: 'on PATH', scripts: { blender: 'exit 0' } },
        { case: 'absent', scripts: {} },
    ])('infers install--blender alike where blender is $case', async ({ scripts }, { workspace }) => {
        host.platform = 'darwin';
        pathed(join(workspace.root, '.bin'), scripts);
        const graph = await workspace.write({ 'tools/blender_manifest.toml': 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n' }).graph({ createNodes });

        expect('install--blender' in (graph.nodes['marker_tools']?.data.targets ?? {})).toBe(true);
    });

    it('installs an Adobe plug-in after its other builds and without packing it', async ({ workspace }) => {
        host.platform = 'darwin';
        const graph = await workspace.write({ ...files, 'panel/project.json': '{ "name": "com.example.panel", "targets": { "build--vite": { "executor": "nx:noop", "outputs": ["{workspaceRoot}/.artifacts/vite/panel"] } } }' }).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['com.example.panel'], ['install--uxp']);

        expect(dependencies['com.example.panel:install--uxp']?.map(({ id }) => id)).toStrictEqual(['com.example.panel:build--vite']);
    });
});
