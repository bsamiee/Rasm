import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-distribution';
import { Clean, execute } from '@rasm/nx/executors/clean';
import { it } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('node:process'), async (actual) => {
    const platform: NodeJS.Platform = 'darwin';
    return { ...(await actual()), platform };
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('clean executor', () => {
    it('removes what the inferred build wrote and keeps the manifest it read', async ({ workspace }) => {
        const graph = await workspace.write({ '.gitignore': '*.yak\n', 'rhino/manifest.yml': 'name: marker\nversion: 1.2.3\nauthors: [Suite]\ndescription: Marks points\n', 'rhino/marker-1.2.3-rh8-any.yak': 'archive' }).graph({ createNodes });

        const stated: unknown = { commands: [], cwd: '.', folders: [], ...graph.nodes['marker']?.data.targets?.['clean--yak']?.options };
        assert(Schema.is(Clean)(stated), 'clean--yak states clean options');

        await execute(stated, workspace.context(graph, 'marker', 'clean--yak'));

        expect(['rhino/marker-1.2.3-rh8-any.yak', 'rhino/manifest.yml'].filter((file) => existsSync(join(workspace.root, file)))).toStrictEqual(['rhino/manifest.yml']);
    });
});
