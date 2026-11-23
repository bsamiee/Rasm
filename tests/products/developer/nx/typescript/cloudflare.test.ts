import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const EDGE = {
    'package.json': '{"name":"root","type":"module","workspaces":["apps/*"]}',
    'package-lock.json': '',
    'shared/dates.ts': "export const compatibilityDate = '2026-10-01';\n",
    'apps/edge/package.json': '{"name":"edge","type":"module"}',
    'apps/edge/cloudflare.config.ts': "import { defineConfig, defineWorker } from 'cf/config';\nimport { compatibilityDate } from '../../shared/dates.ts';\nimport { name } from './name.ts';\n\nexport default defineConfig({ worker: defineWorker({ name, entrypoint: './worker.ts', compatibilityDate }) });\n",
    'apps/edge/name.ts': "export const name = 'edge';\n",
    'apps/edge/worker.ts': "export default { fetch: () => new Response('ok') };\n",
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Worker binding type generation', () => {
    it('hashes the workspace files cloudflare.config.ts imports from outside the project', async ({ workspace }) => {
        const graph = await workspace
            .install(import.meta.resolve('cf/package.json'))
            .write(EDGE)
            .graph({ createNodes });

        expect(graph.nodes['edge']?.data.targets?.['codegen--cf']?.inputs?.filter((input) => typeof input === 'string' && input.startsWith('{'))).toStrictEqual(['{workspaceRoot}/shared/dates.ts']);
    });

    it('refuses the graph naming a cloudflare.config.ts that fails to load', async ({ workspace }) => {
        workspace.install(import.meta.resolve('cf/package.json')).write({ ...EDGE, 'apps/broken/package.json': '{"name":"broken","type":"module"}', 'apps/broken/cloudflare.config.ts': "throw new Error('broken on load');\n" });

        await expect(workspace.graph({ createNodes })).rejects.toSatisfy(refused(/^apps\/broken\/cloudflare\.config\.ts does not load as a Cloudflare config$/u));
    });
});
