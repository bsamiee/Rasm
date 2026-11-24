import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-distribution';
import { execute, Upgrade } from '@rasm/nx/executors/upgrade';
import { it, shimmed, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const repository = {
    'package.json': '{ "name": "repo" }',
    '.github/workflows/ci.yml': 'on: push\n',
    '.github/actions/setup/action.yml': 'runs:\n    using: composite\n    steps: []\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('upgrade--jactionlint', () => {
    it.for([
        { case: 'a run', options: {}, mode: '--apply' },
        { case: 'a dry run', options: { dryRun: true }, mode: '--diff' },
    ])('runs jactionlint update $mode at the workspace root with no file, as jactionlint reads every workflow and action file of the repository itself, under $case', async ({ options, mode }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), { jactionlint: `printf '%s\\n' "$PWD" "$@" > "${join(workspace.root, 'ran.txt')}"` });
        const graph = await workspace.write(repository).graph({ createNodes });
        await execute({ ...(await targetOptions(Upgrade, graph, 'repo', 'upgrade--jactionlint')), ...options }, workspace.context(graph, 'repo', 'upgrade--jactionlint'));

        expect(readFileSync(join(workspace.root, 'ran.txt'), 'utf-8').trim().split('\n')).toStrictEqual([workspace.root, 'update', mode]);
    });
});
