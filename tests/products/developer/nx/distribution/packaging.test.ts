import { globSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import { ZipFile } from 'node:zlib';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-distribution';
import { execute, Package } from '@rasm/nx-distribution/executors/package';
import { it, type Workspace, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const settings = { outputPath: '.artifacts/distribution/{projectName}' };
const panel = {
    '.gitignore': '.artifacts\n*.log\n',
    'panel/manifest.json': '{ "manifestVersion": 5, "id": "com.example.panel", "name": "Panel", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }',
    'panel/index.html': '<!doctype html><title>Panel</title>\n',
    'panel/scripts/main.js': 'export const main = 1;\n',
    'panel/debug.log': 'ignored\n',
};
const clipper = {
    '.gitignore': '.artifacts\n',
    'clipper/manifest.json': '{ "manifest_version": 3, "name": "Clipper", "version": "1.2.3", "background": { "service_worker": "worker.ts" } }',
    'clipper/worker.ts': 'chrome.runtime.id;\n',
    'clipper/project.json': '{ "name": "Clipper", "targets": { "build--vite": { "executor": "nx:noop", "outputs": ["{workspaceRoot}/.artifacts/vite/clipper"] } } }',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const packed = async (workspace: Workspace, graph: ProjectGraph, project: string, target: string) => execute(await targetOptions(Package, graph, project, target), workspace.context(graph, project, target));

const listed = (workspace: Workspace, archive: string) => {
    const opened = ZipFile.openSync(join(workspace.root, archive));
    const entries = [...opened.keys()].toSorted();
    opened.closeSync();
    return entries;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('build--uxp and build--chrome', () => {
    it('packs the tracked source of a UXP plug-in into a .ccx holding its manifest at the root', async ({ workspace }) => {
        const graph = await workspace.write(panel).graph({ createNodes }, settings);

        await packed(workspace, graph, 'com.example.panel', 'build--uxp');

        expect(globSync('.artifacts/distribution/com.example.panel/*.ccx', { cwd: workspace.root }).flatMap((archive) => listed(workspace, archive))).toStrictEqual(['index.html', 'manifest.json', 'scripts/main.js']);
    });

    it('writes exactly the files the outputs of build--uxp match', async ({ workspace }) => {
        const graph = await workspace.write(panel).graph({ createNodes }, settings);
        const before = workspace.files('tmp');

        await packed(workspace, graph, 'com.example.panel', 'build--uxp');

        const written = workspace.files('tmp').difference(before);
        expect([...written]).toStrictEqual([...workspace.cached(graph, 'com.example.panel', 'build--uxp')]);
        expect(written.size).toBe(1);
    });

    it('replaces every earlier archive of its kind in the output folder and keeps other files', async ({ workspace }) => {
        const graph = await workspace.write({ ...panel, '.artifacts/distribution/com.example.panel/com.example.panel-1.2.2.ccx': 'stale', '.artifacts/distribution/com.example.panel/notes.txt': 'kept' }).graph({ createNodes }, settings);

        await packed(workspace, graph, 'com.example.panel', 'build--uxp');

        const left = readdirSync(join(workspace.root, '.artifacts/distribution/com.example.panel'));
        expect(left.filter((name) => name.endsWith('.ccx')).flatMap((archive) => listed(workspace, join('.artifacts/distribution/com.example.panel', archive)))).toStrictEqual(['index.html', 'manifest.json', 'scripts/main.js']);
        expect(left).toContain('notes.txt');
    });

    it('packs the extension folder an earlier build of the project wrote in place of its source', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...clipper,
                '.artifacts/vite/clipper/manifest.json': '{ "manifest_version": 3, "name": "Clipper", "version": "1.2.3", "background": { "service_worker": "worker-loader.js" } }',
                '.artifacts/vite/clipper/worker-loader.js': "import './assets/worker.js';\n",
                '.artifacts/vite/clipper/assets/worker.js': 'chrome.runtime.id;\n',
            })
            .graph({ createNodes }, settings);

        await packed(workspace, graph, 'Clipper', 'build--chrome');

        expect(globSync('.artifacts/distribution/Clipper/*.zip', { cwd: workspace.root }).flatMap((archive) => listed(workspace, archive))).toStrictEqual(['assets/worker.js', 'manifest.json', 'worker-loader.js']);
    });

    it('refuses an earlier build that wrote no manifest, naming the manifests it looks for', async ({ workspace }) => {
        const graph = await workspace.write({ ...clipper, '.artifacts/vite/clipper/worker-loader.js': "import './assets/worker.js';\n" }).graph({ createNodes }, settings);

        await expect(packed(workspace, graph, 'Clipper', 'build--chrome')).rejects.toThrow('Clipper finds none of manifest.yml, manifest.json, .claude-plugin/plugin.json in what its builds wrote');
    });
});
