import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*","apps/*"]}', 'package-lock.json': '' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('tsc inference across graph runs', () => {
    it('infers the same targets from its cache as from a fresh read', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...ROOT,
            'tsconfig.base.json': '{"compilerOptions":{"rootDir":"."}}',
            'libs/lib/package.json': '{"name":"lib"}',
            'libs/lib/tsconfig.json': '{"extends":"../../tsconfig.base.json","files":[],"references":[{"path":"./tsconfig.lib.json"}]}',
            'libs/lib/tsconfig.lib.json': '{"extends":"./tsconfig.json","include":["src/**/*.ts"],"compilerOptions":{"outDir":"out-tsc/lib","composite":true}}',
            'libs/lib/src/index.ts': 'export {};',
        });
        const fresh = await workspace.graph({ createNodes });

        const cached = await workspace.graph({ createNodes });

        expect(cached.nodes['lib']?.data.targets).toStrictEqual(fresh.nodes['lib']?.data.targets);
    });

    it('infers the targets it read before once the compiler stops resolving, while every config and source is unchanged', async ({ workspace }) => {
        workspace.install(typescript).write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"include":["src"],"compilerOptions":{"noEmit":true}}', 'libs/lib/src/index.ts': 'export {};' });
        const fresh = await workspace.graph({ createNodes });

        const cached = await workspace.remove('node_modules/typescript').graph({ createNodes });

        expect(cached.nodes['lib']?.data.targets).toStrictEqual(fresh.nodes['lib']?.data.targets);
    });

    it('reads the references again after a tsconfig appears beside the tsconfig another folder holds', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...ROOT,
            'apps/app/package.json': '{"name":"app"}',
            'apps/app/tsconfig.json': '{"files":["main.ts"],"references":[{"path":"../../libs/shared"}],"compilerOptions":{"noEmit":true}}',
            'apps/app/main.ts': 'export {};',
            'libs/shared/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.one.json"}]}',
            'libs/shared/index.ts': 'export {};',
        });
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ 'libs/shared/tsconfig.one.json': '{"files":["index.ts"]}' }).graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--tsc']?.inputs?.filter((input) => typeof input === 'string' && input.startsWith('{workspaceRoot}/libs/shared/'))).toStrictEqual(['{workspaceRoot}/libs/shared/tsconfig.json', '{workspaceRoot}/libs/shared/tsconfig.one.json']);
    });

    it('reads the references again after an edit to the tsconfig another folder holds', async ({ workspace }) => {
        workspace.install(typescript).write({
            ...ROOT,
            'apps/app/package.json': '{"name":"app"}',
            'apps/app/tsconfig.json': '{"files":["main.ts"],"references":[{"path":"../../libs/shared"}],"compilerOptions":{"noEmit":true}}',
            'apps/app/main.ts': 'export {};',
            'libs/shared/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.one.json"}]}',
            'libs/shared/tsconfig.one.json': '{"files":["index.ts"]}',
            'libs/shared/tsconfig.two.json': '{"files":["index.ts"]}',
            'libs/shared/index.ts': 'export {};',
        });
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ 'libs/shared/tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.two.json"}]}' }).graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--tsc']?.inputs?.filter((input) => typeof input === 'string' && input.startsWith('{workspaceRoot}/libs/shared/'))).toStrictEqual(['{workspaceRoot}/libs/shared/tsconfig.json', '{workspaceRoot}/libs/shared/tsconfig.two.json']);
    });

    it('hashes a file added in a new folder the include covers', async ({ workspace }) => {
        workspace.install(typescript).write({ ...ROOT, 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"include":["src"],"compilerOptions":{"noEmit":true}}', 'libs/lib/src/index.ts': 'export {};' });
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ 'libs/lib/src/nested/added.ts': 'export {};' }).graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['check--tsc']?.inputs).toContain('{projectRoot}/src/nested/added.ts');
    });
});
