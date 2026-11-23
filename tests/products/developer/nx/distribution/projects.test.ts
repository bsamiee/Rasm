import { createNodes } from '@rasm/nx-distribution';
import { refused } from '@rasm/testing/errors';
import { it, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const hosts = [
    { technology: 'blender', file: 'tools/blender_manifest.toml', text: 'schema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\n', name: 'marker_tools', root: 'tools' },
    { technology: 'yak', file: 'rhino/manifest.yml', text: 'name: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\n', name: 'marker', root: 'rhino' },
    { technology: 'uxp', file: 'panel/manifest.json', text: '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }', name: 'com.example.panel', root: 'panel' },
    { technology: 'chrome', file: 'clipper/manifest.json', text: '{ "manifest_version": 3, "name": "__MSG_name__", "default_locale": "en", "version": "1.2.3" }', name: 'clipper', root: 'clipper' },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('host manifests', () => {
    it.for(hosts)('infers $name at $root from its $technology manifest, its build running build--$technology', async ({ technology, file, text, name, root }, { workspace }) => {
        const graph = await workspace.write({ [file]: text }).graph({ createNodes });

        const { tasks } = taskGraph(graph, [name], ['build']);

        expect(graph.nodes[name]?.data).toMatchObject({ root, metadata: { technologies: [technology] } });
        expect(Object.keys(tasks).toSorted()).toStrictEqual([`${name}:build`, `${name}:build--${technology}`]);
    });

    it.for([
        { case: 'a web app manifest', file: 'site/manifest.json', text: '{ "name": "Site", "short_name": "Site", "start_url": "/", "display": "standalone" }' },
        { case: 'a Manifest V2 extension', file: 'legacy/manifest.json', text: '{ "manifest_version": 2, "name": "Legacy", "version": "1.0.0" }' },
        { case: 'a plug-in Unreal installs with the engine', file: 'Engine/Plugins/Bundled/Bundled.uplugin', text: '{ "FileVersion": 3, "VersionName": "1.0.0", "Installed": true }' },
    ])('infers no project from $case', async ({ file, text }, { workspace }) => {
        const graph = await workspace.write({ [file]: text }).graph({ createNodes });

        expect(graph.nodes).toStrictEqual({});
    });

    it('roots a manifest at the workspace root in the root project', async ({ workspace }) => {
        const graph = await workspace.write({ 'manifest.yml': 'name: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\n' }).graph({ createNodes });

        expect(graph.nodes['marker']?.data.root).toBe('.');
    });

    it('refuses a manifest its host format cannot read, naming the file', async ({ workspace }) => {
        const pending = workspace.write({ 'torn/blender_manifest.toml': 'id = "torn\nversion = \n' }).graph({ createNodes });

        await expect(pending).rejects.toSatisfy(refused(/^torn\/blender_manifest\.toml does not decode/u));
    });
});

describe('workflow folders', () => {
    it.for([
        { case: 'the workspace root', files: { 'package.json': '{ "name": "repo" }', '.github/workflows/ci.yml': 'on: push\n' }, name: 'repo' },
        { case: 'a nested project', files: { 'sites/web/project.json': '{ "name": "web" }', 'sites/web/.github/workflows/release.yaml': 'on: push\n' }, name: 'web' },
    ])('gives $case holding .github/workflows an upgrade running upgrade--jactionlint', async ({ files, name }, { workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });

        const { tasks } = taskGraph(graph, [name], ['upgrade']);

        expect(Object.keys(tasks).toSorted()).toStrictEqual([`${name}:upgrade`, `${name}:upgrade--jactionlint`]);
    });
});
