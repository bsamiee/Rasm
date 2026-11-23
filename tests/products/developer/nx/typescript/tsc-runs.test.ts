import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIB = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '', 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/index.ts': 'export const value = 1;' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('running the tsc targets', { timeout: 30_000 }, () => {
    it.for([
        { case: 'an outDir holds declarations and source maps', target: 'build--tsc', files: { 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"outDir":"dist","declaration":true,"declarationMap":true,"sourceMap":true}}' }, emitted: 'libs/lib/dist/index.js' },
        { case: 'no outDir places output beside its source', target: 'build--tsc', files: { 'libs/lib/tsconfig.json': '{"files":["src/index.ts"],"compilerOptions":{"declaration":true}}', 'libs/lib/src/index.ts': 'export {};' }, emitted: 'libs/lib/src/index.js' },
        { case: 'a composite program roots at src beside its outDir', target: 'build--tsc', files: { 'libs/lib/tsconfig.json': '{"include":["src"],"compilerOptions":{"composite":true,"rootDir":"src","outDir":"dist"}}', 'libs/lib/src/index.ts': 'export {};' }, emitted: 'libs/lib/dist/index.js' },
        { case: 'an incremental tsBuildInfoFile sits outside the outDir', target: 'build--tsc', files: { 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"incremental":true,"outDir":"dist","tsBuildInfoFile":"cache/lib.tsbuildinfo"}}' }, emitted: 'libs/lib/cache/lib.tsbuildinfo' },
        { case: 'a declarationDir sits apart from the outDir', target: 'build--tsc', files: { 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"outDir":"dist","declaration":true,"declarationDir":"types"}}' }, emitted: 'libs/lib/types/index.d.ts' },
        {
            case: 'a nodenext program imports json its module implies',
            target: 'build--tsc',
            files: {
                'libs/lib/package.json': '{"name":"lib","type":"module"}',
                'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"outDir":"dist","module":"nodenext"}}',
                'libs/lib/index.ts': "import data from './data.json' with { type: 'json' };\nexport const value = data.value;",
                'libs/lib/data.json': '{"value":1}',
            },
            emitted: 'libs/lib/dist/data.json',
        },
        {
            case: 'a solution tsconfig references a composite program',
            target: 'build--tsc',
            files: { 'libs/lib/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.lib.json"}]}', 'libs/lib/tsconfig.lib.json': '{"include":["src"],"compilerOptions":{"composite":true,"outDir":"dist"}}', 'libs/lib/src/index.ts': 'export {};' },
            emitted: 'libs/lib/dist/src/index.js',
        },
        {
            case: 'a solution tsconfig references a program in a subfolder holding no manifest',
            target: 'build--tsc',
            files: { 'libs/lib/tsconfig.json': '{"files":[],"references":[{"path":"./src"}]}', 'libs/lib/src/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"composite":true,"outDir":"dist"}}', 'libs/lib/src/index.ts': 'export {};' },
            emitted: 'libs/lib/src/dist/index.js',
        },
        {
            case: 'a program references another program it builds first',
            target: 'build--tsc',
            files: {
                'libs/lib/tsconfig.json': '{"files":["index.ts"],"references":[{"path":"./tsconfig.core.json"}],"compilerOptions":{"composite":true,"outDir":"dist"}}',
                'libs/lib/tsconfig.core.json': '{"files":["core.ts"],"compilerOptions":{"composite":true,"outDir":"dist/core"}}',
                'libs/lib/core.ts': 'export {};',
            },
            emitted: 'libs/lib/dist/core/core.js',
        },
        { case: 'a noEmit program checks incrementally', target: 'check--tsc', files: { 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true,"incremental":true}}' }, emitted: 'libs/lib/tsconfig.tsbuildinfo' },
    ])('declares every file $target writes when $case', async ({ files, target, emitted }, { workspace }) => {
        workspace.install(typescript).write({ ...LIB, ...files });
        const graph = await workspace.graph({ createNodes });
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'lib', target);

        const written = workspace.files('node_modules', 'tmp').difference(before);
        const cached = workspace.cached(graph, 'lib', target);
        expect(success).toBe(true);
        expect([...written]).toContain(emitted);
        expect([...written.difference(cached)]).toStrictEqual([]);
        expect([...cached.intersection(before)]).toStrictEqual([]);
    });

    it('checks a program its tsconfig sets noEmit for without writing a file or inferring a build', async ({ workspace }) => {
        workspace.install(typescript).write({ ...LIB, 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"outDir":"dist","declaration":true,"noEmit":true}}' });
        const graph = await workspace.graph({ createNodes });
        expect(graph.nodes['lib']?.data.targets?.['build--tsc']).toBeUndefined();
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'lib', 'check--tsc');

        expect(success).toBe(true);
        expect([...workspace.files('node_modules', 'tmp').difference(before)]).toStrictEqual([]);
    });

    it('checks the program a workspace root solution references beside it, and no program of another project', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...LIB,
            'tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.node.json"},{"path":"./libs/lib"}]}',
            'tsconfig.node.json': '{"files":["tool.ts"],"compilerOptions":{"composite":true,"noEmit":true}}',
            'tool.ts': 'export {};',
            'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"composite":true,"outDir":"dist"}}',
            'libs/lib/index.ts': "export const value: number = 'text';",
        });
        const graph = await workspace.graph({ createNodes });
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'root', 'check--tsc');

        expect(success).toBe(true);
        expect([...workspace.files('node_modules', 'tmp').difference(before)]).toStrictEqual(['tsconfig.node.tsbuildinfo']);
    });

    it('runs a referenced tsconfig whose name holds shell syntax as that file', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...LIB,
            'libs/lib/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig $(touch pwned).json"}]}',
            'libs/lib/tsconfig $(touch pwned).json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}',
        });
        const graph = await workspace.graph({ createNodes });

        const { success } = await workspace.run(graph, 'lib', 'check--tsc');

        expect(success).toBe(true);
        expect(existsSync(join(workspace.root, 'libs/lib/pwned'))).toBe(false);
    });
});
