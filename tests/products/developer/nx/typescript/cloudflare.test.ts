import { join, relative } from 'node:path';
import { loadConfig } from '@cloudflare/config';
import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it } from '@rasm/testing/nx';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { describe, expect, vi } from 'vitest';

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

    it('refuses the graph naming a cloudflare.config.ts that fails to load', async ({ workspace }) => {
        workspace.install(import.meta.resolve('cf/package.json')).write({ ...EDGE, 'apps/broken/package.json': '{"name":"broken","type":"module"}', 'apps/broken/cloudflare.config.ts': "throw new Error('broken on load');\n" });

        await expect(workspace.graph({ createNodes })).rejects.toSatisfy(refused(/^apps\/broken\/cloudflare\.config\.ts does not load as a Cloudflare config$/u));
    });
});
