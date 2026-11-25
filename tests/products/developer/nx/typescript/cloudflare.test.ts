import { join, relative } from 'node:path';
import { env } from 'node:process';
import { loadConfig } from '@cloudflare/config';
import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const EDGE = {
    'package.json': '{"name":"root","type":"module","workspaces":["apps/*","libs/*"]}',
    'package-lock.json': '',
    'shared/dates.ts': "export const compatibilityDate = '2026-10-01';\n",
    'libs/up/package.json': '{"name":"up","type":"module"}',
    'libs/up/index.ts': 'export const up = 1;\n',
    'apps/edge/package.json': '{"name":"edge","type":"module","nx":{"implicitDependencies":["up"]}}',
    'apps/edge/cloudflare.config.ts': "import { defineConfig, defineWorker } from 'cf/config';\nimport { compatibilityDate } from '../../shared/dates.ts';\nimport { name } from './name.ts';\n\nexport default defineConfig({ worker: defineWorker({ name, entrypoint: './worker.ts', compatibilityDate }) });\n",
    'apps/edge/name.ts': "export const name = 'edge';\n",
    'apps/edge/notes.md': '# edge\n',
    'apps/edge/worker.ts': "import { up } from '../../libs/up/index.ts';\nexport default { fetch: () => new Response(String(up)) };\n",
};
const CF = { 'npm:cf': { type: 'npm', name: 'npm:cf', data: { packageName: 'cf', version: '1.0.0' } } } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const fingerprinted = async (workspace: Workspace) => {
    const graph = { ...(await workspace.graph({ createNodes })), externalNodes: CF };
    const tasks = taskGraph(graph, ['edge'], ['codegen--cf']);
    const task = tasks.tasks['edge:codegen--cf'];
    assert(task !== undefined, 'edge:codegen--cf is a task of the graph');
    const { value } = await createTaskHasher(graph, readNxJson(workspace.root)).hashTask(task, tasks, env);
    return value;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Worker binding type generation', () => {
    it('hashes the workspace files cf reports cloudflare.config.ts loads, and neither the worker it names, a file beside it, nor an upstream source', async ({ workspace }) => {
        const graph = await workspace
            .install(import.meta.resolve('cf/package.json'))
            .write(EDGE)
            .graph({ createNodes });
        const inspector = new HashPlanInspector({ ...graph, externalNodes: CF }, workspace.root);
        await inspector.init();

        const { dependencies } = await loadConfig(join(workspace.root, 'apps/edge/cloudflare.config.ts'));
        const reported = [...dependencies].map((at) => relative(workspace.root, at)).filter((at) => !(at.startsWith('..') || at.includes('node_modules')));
        const hashed = new Set(inspector.inspectTaskInputs({ project: 'edge', target: 'codegen--cf' })['edge:codegen--cf']?.files);

        expect(reported).toContain('shared/dates.ts');
        expect(reported.filter((at) => !hashed.has(at))).toStrictEqual([]);
        expect(['apps/edge/worker.ts', 'apps/edge/notes.md', 'libs/up/index.ts'].filter((at) => hashed.has(at))).toStrictEqual([]);
    });

    it.for([
        { case: 'a module it imports from inside the folder of a project nested in its own', edit: { 'apps/edge/sub/entry.ts': "export const entrypoint = './worker.ts';\nexport const unused = 1;\n" } },
        { case: 'a module it imports from a folder of its own outside the nested project', edit: { 'apps/edge/config/name.ts': "export const name = 'edge-renamed';\n" } },
        { case: 'a module it imports directly from its own folder', edit: { 'apps/edge/dates.ts': "export const compatibilityDate = '2026-10-02';\n" } },
    ])('hashes codegen--cf of a project holding another project again after an edit to $case', async ({ edit }, { workspace }) => {
        workspace.install(import.meta.resolve('cf/package.json')).write({
            ...EDGE,
            'apps/edge/cloudflare.config.ts':
                "import { defineConfig, defineWorker } from 'cf/config';\nimport { name } from './config/name.ts';\nimport { compatibilityDate } from './dates.ts';\nimport { entrypoint } from './sub/entry.ts';\n\nexport default defineConfig({ worker: defineWorker({ name, entrypoint, compatibilityDate }) });\n",
            'apps/edge/config/name.ts': "export const name = 'edge';\n",
            'apps/edge/dates.ts': "export const compatibilityDate = '2026-10-01';\n",
            'apps/edge/sub/project.json': '{"name":"sub"}',
            'apps/edge/sub/entry.ts': "export const entrypoint = './worker.ts';\n",
        });
        const before = await fingerprinted(workspace);

        const after = await fingerprinted(workspace.write(edit));

        expect(after).not.toBe(before);
    });

    it('refuses the graph naming a cloudflare.config.ts that fails to load', async ({ workspace }) => {
        workspace.install(import.meta.resolve('cf/package.json')).write({ ...EDGE, 'apps/broken/package.json': '{"name":"broken","type":"module"}', 'apps/broken/cloudflare.config.ts': "throw new Error('broken on load');\n" });

        await expect(workspace.graph({ createNodes })).rejects.toSatisfy(refused(/^apps\/broken\/cloudflare\.config\.ts does not load as a Cloudflare config$/u));
    });
});
