import { spawnSync } from 'node:child_process';
import { readdirSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { execute, Pack } from '@rasm/nx-typescript/executors/pack';
import { it, targetOptions, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('package tarballs', () => {
    it.for([
        { case: 'states a version', manifest: '{"name":"lib","version":"1.0.0"}', packed: true },
        { case: 'states no version', manifest: '{"name":"lib"}', packed: false },
    ])('offers a package for release through nx release and nx pack only where its manifest $case', async ({ manifest, packed }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/lib/package.json': manifest }).graph({ createNodes });

        const scheduled = 'pack' in (graph.nodes['lib']?.data.targets ?? {}) ? Object.keys(taskGraph(graph, ['lib'], ['pack']).tasks) : [];

        expect([scheduled.includes('lib:pack--npm'), graph.nodes['lib']?.data.release !== undefined]).toStrictEqual([packed, packed]);
    });

    it.for([
        { manager: 'pnpm', files: { 'package.json': '{"name":"root"}', 'pnpm-lock.yaml': '', 'pnpm-workspace.yaml': 'packages:\n    - libs/*\n' } },
        { manager: 'npm', files: { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '{"name":"root","lockfileVersion":3,"packages":{}}' } },
    ])('packs with $manager one tarball holding the files the manifest lists into the missing outputPath folder, where Nx caches it', async ({ manager, files }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'libs/lib/package.json': '{"name":"lib","version":"1.0.0","files":["index.js"]}', 'libs/lib/index.js': 'export {};', 'libs/lib/notes.txt': 'unlisted' }).graph({ createNodes }, { outputPath: `.artifacts/${manager}/{projectName}` });

        const { success } = await execute(await targetOptions(Pack, graph, 'lib', `pack--${manager}`), workspace.context(graph, 'lib', `pack--${manager}`));

        const written = readdirSync(join(workspace.root, `.artifacts/${manager}/lib`)).map((name) => `.artifacts/${manager}/lib/${name}`);
        const listed = written.flatMap((tarball) => spawnSync(...workspace.spawned('tar', ['-tzf', join(workspace.root, tarball)], { encoding: 'utf-8' })).stdout.split('\n')).filter((entry) => entry.length > 0);
        expect([success, written.length, listed.toSorted(), [...workspace.cached(graph, 'lib', `pack--${manager}`)]]).toStrictEqual([true, 1, ['package/index.js', 'package/package.json'], written]);
    });
});
