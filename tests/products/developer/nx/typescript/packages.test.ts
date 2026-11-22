import { readdirSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { execute, Pack } from '@rasm/nx-typescript/executors/pack';
import { it, targetOptions, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };
const PUBLINT = { 'node_modules/publint/package.json': '{"name":"publint"}' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('package release targets', () => {
    it.for([
        { case: 'states a version', manifest: '{"name":"lib","version":"1.0.0"}', released: ['attach--gh', 'clean--npm', 'pack--npm'], release: true },
        { case: 'states no version', manifest: '{"name":"lib"}', released: [], release: false },
    ])('packs and attaches a package that $case', async ({ manifest, released, release }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/lib/package.json': manifest }).graph({ createNodes });

        expect([
            Object.keys(graph.nodes['lib']?.data.targets ?? {})
                .filter((target) => /^(?:pack|attach|clean)--/u.test(target))
                .toSorted(),
            graph.nodes['lib']?.data.release !== undefined,
        ]).toStrictEqual([released, release]);
    });

    it.for([
        { case: 'a released package', manifest: '{"name":"lib","version":"1.0.0","os":["darwin","!win32"]}', tags: ['npm', 'os:darwin'] },
        { case: 'an unreleased package', manifest: '{"name":"lib","os":["darwin","!win32"]}', tags: ['os:darwin'] },
    ])('tags $case with its release group selector and each platform its os field allows', async ({ manifest, tags }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'nx.json': '{"release":{"groups":{"npm":{"projects":["tag:npm"]}}}}', 'libs/lib/package.json': manifest }).graph({ createNodes });

        expect(graph.nodes['lib']?.data.tags?.filter((tag) => !tag.startsWith('npm:')).toSorted()).toStrictEqual(tags);
    });

    it.for([
        { manager: 'pnpm', files: { 'package.json': '{"name":"root"}', 'pnpm-lock.yaml': '', 'pnpm-workspace.yaml': 'packages:\n    - libs/*\n' }, tarball: 'lib.tgz' },
        { manager: 'npm', files: { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '{"name":"root","lockfileVersion":3,"packages":{}}' }, tarball: 'lib-1.0.0.tgz' },
    ])('packs one $manager tarball into the missing outputPath folder of the project, where Nx caches it', { timeout: 30_000 }, async ({ manager, files, tarball }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'libs/lib/package.json': '{"name":"lib","version":"1.0.0"}', 'libs/lib/index.js': 'export {};' }).graph({ createNodes }, { outputPath: `.artifacts/${manager}/{projectName}` });

        const { success } = await execute(await targetOptions(Pack, graph, 'lib', `pack--${manager}`), workspace.context(graph, 'lib', `pack--${manager}`));

        const cached = [...workspace.cached(graph, 'lib', `pack--${manager}`)];
        expect([success, readdirSync(join(workspace.root, `.artifacts/${manager}/lib`)), cached]).toStrictEqual([true, [tarball], [`.artifacts/${manager}/lib/${tarball}`]]);
    });
});

describe('package lint target', () => {
    it.for([
        { case: 'a versioned package where publint resolves', manifest: '{"name":"lib","version":"1.0.0"}', files: PUBLINT, linted: true },
        { case: 'an unversioned package where publint resolves', manifest: '{"name":"lib"}', files: PUBLINT, linted: false },
        { case: 'a versioned package where publint does not resolve', manifest: '{"name":"lib","version":"1.0.0"}', files: {}, linted: false },
    ])('lints the packed package with publint only for $case', async ({ manifest, files, linted }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...ROOT, ...files, 'libs/lib/package.json': manifest, 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"outDir":"dist"}}', 'libs/lib/index.ts': 'export {};' })
            .graph({ createNodes });

        expect('check--publint' in (graph.nodes['lib']?.data.targets ?? {})).toBe(linted);
    });
});
