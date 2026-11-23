import { createNodes } from '@rasm/nx-typescript';
import { it, typescript, unaggregated } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('projects the plugin infers', () => {
    it.for([
        { case: 'beside a package.json the root workspaces cover', files: { 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'libs/lib/index.ts': 'export {};' }, project: 'lib', tsc: ['check--tsc'] },
        { case: 'beside a project.json', files: { 'apps/app/project.json': '{"name":"app"}', 'apps/app/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'apps/app/index.ts': 'export {};' }, project: 'app', tsc: ['check--tsc'] },
        { case: 'beside a package.json outside the root workspaces', files: { 'tools/tool/package.json': '{"name":"tool"}', 'tools/tool/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'tools/tool/index.ts': 'export {};' }, project: 'tool', tsc: [] },
        { case: 'at a workspace root, holding files', files: { 'tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}', 'index.ts': 'export {};' }, project: 'root', tsc: ['check--tsc'] },
        {
            case: 'at a workspace root, referencing a program beside it',
            files: { 'tsconfig.json': '{"files":[],"references":[{"path":"./tsconfig.node.json"}]}', 'tsconfig.node.json': '{"files":["index.ts"],"compilerOptions":{"composite":true,"noEmit":true}}', 'index.ts': 'export {};' },
            project: 'root',
            tsc: ['check--tsc', 'clean--tsc'],
        },
        {
            case: 'at a workspace root, referencing programs of other projects alone',
            files: { 'tsconfig.json': '{"files":[],"references":[{"path":"./libs/lib"}]}', 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"composite":true,"outDir":"dist"}}', 'libs/lib/index.ts': 'export {};' },
            project: 'root',
            tsc: [],
        },
    ])('infers the tsc targets $tsc for a tsconfig.json $case', async ({ files, project, tsc }, { workspace }) => {
        const graph = await workspace
            .install(typescript)
            .write({ ...ROOT, ...files })
            .graph({ createNodes });

        expect(Object.keys(graph.nodes[project]?.data.targets ?? {}).filter((target) => target.endsWith('--tsc'))).toStrictEqual(tsc);
    });

    it('runs every tool target under its verb and hashes its inputs into the verb, for each kind of project', async ({ workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'), import.meta.resolve('cf/package.json'))
            .write({
                ...ROOT,
                '.jscpd.json': '{"pattern":"**/*.ts"}',
                'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                'tsconfig.base.json': '{"compilerOptions":{"strict":true}}',
                'libs/lib/package.json': '{"name":"lib","version":"1.0.0"}',
                'libs/lib/tsconfig.json': '{"extends":"../../tsconfig.base.json","files":["index.ts"],"compilerOptions":{"outDir":"dist"}}',
                'libs/lib/index.ts': 'export {};',
                'libs/edge/package.json': '{"name":"edge","type":"module"}',
                'libs/edge/cloudflare.config.ts': "import { defineConfig, defineWorker } from 'cf/config';\n\nexport default defineConfig({ worker: defineWorker({ name: 'edge', entrypoint: './worker.ts', compatibilityDate: '2026-10-01' }) });\n",
                'libs/edge/worker.ts': "export default { fetch: () => new Response('ok') };\n",
                'apps/app/project.json': '{"name":"app"}',
                'apps/app/tsconfig.json': '{"files":["index.ts"],"compilerOptions":{"noEmit":true}}',
                'apps/app/index.ts': 'export {};',
            })
            .graph({ createNodes });

        expect(
            Object.values(graph.nodes)
                .flatMap(({ data: { targets = {} } }) => Object.keys(targets))
                .filter((target) => target.includes('--'))
                .toSorted(),
        ).toStrictEqual(['attach--gh', 'build--tsc', 'check--eslint', 'check--eslint', 'check--eslint', 'check--jscpd', 'check--tsc', 'check--tsc', 'clean--cf', 'clean--npm', 'clean--npm', 'clean--tsc', 'codegen--cf', 'pack--npm', 'upgrade--npm'].toSorted());
        expect(unaggregated(graph)).toStrictEqual([]);
    });
});
