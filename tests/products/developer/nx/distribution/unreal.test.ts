import { createNodes } from '@rasm/nx-distribution';
import { it, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const engine = { 'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n' };
const plugins = {
    'Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3" }\n',
    'Game/Plugins/Retired/Retired.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "0.9.0" }\n',
    'Shared/Tools/Tools.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "2.0.0" }\n',
};
const game = { 'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Plugins": [{ "Name": "Marker", "Enabled": true }, { "Name": "Retired", "Enabled": false }, { "Name": "Tools", "Enabled": true }] }\n' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('unreal projects and plug-ins', () => {
    it('makes a project depend on each plug-in it enables, and on none it disables', async ({ workspace }) => {
        const graph = await workspace.write({ ...engine, ...plugins, ...game }).graph({ createNodes });

        expect(graph.dependencies['Game']?.map(({ target }) => target).toSorted()).toStrictEqual(['Marker', 'Tools']);
    });

    it('builds a project after installing each plug-in it enables from outside its folders', async ({ workspace }) => {
        const graph = await workspace.write({ ...engine, ...plugins, ...game }).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['Game'], ['build--unreal']);

        expect(dependencies['Game:build--unreal']?.map(({ id }) => id)).toStrictEqual(['Tools:install']);
    });

    it('tests a plug-in after building the project hosting it', async ({ workspace }) => {
        const graph = await workspace.write({ ...engine, ...plugins, ...game }).graph({ createNodes });

        const { tasks } = taskGraph(graph, ['Marker'], ['test']);

        expect(Object.keys(tasks)).toContain('Game:build--unreal');
    });

    it('attaches the per-engine archives a plug-in packs to its release', async ({ workspace }) => {
        const graph = await workspace.write({ ...engine, ...plugins, ...game }).graph({ createNodes });

        const { dependencies } = taskGraph(graph, ['Marker'], ['attach--gh']);

        expect(dependencies['Marker:attach--gh']?.map(({ id }) => id)).toStrictEqual(['Marker:pack--unreal']);
    });

    it.for([
        { case: 'its engine tree is absent', files: { ...plugins, ...game }, building: false },
        { case: 'its engine tree holds Build.version', files: { ...engine, ...plugins, ...game }, building: true },
    ])('infers the build of a project where $case: $building', async ({ files, building }, { workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });

        expect(['build--unreal', 'check--unreal'].map((target) => target in (graph.nodes['Game']?.data.targets ?? {}))).toStrictEqual([building, building]);
    });
});
