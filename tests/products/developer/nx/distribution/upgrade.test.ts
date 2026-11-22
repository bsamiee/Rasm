import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { env, execPath } from 'node:process';
import { createNodes } from '@rasm/nx-distribution';
import { execute, Upgrade } from '@rasm/nx-distribution/executors/upgrade';
import { it, shimmed, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const site = {
    'sites/web/project.json': '{ "name": "web" }',
    'sites/web/.github/workflows/ci.yml': 'on: push\n',
    'sites/web/.github/actions/setup/action.yml': 'runs:\n    using: composite\n    steps: []\n',
    'sites/web/tools/lint/action.yml': 'runs:\n    using: composite\n    steps: []\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('upgrade--jactionlint', () => {
    it.for([
        { case: 'a run', options: {}, mode: '--apply' },
        { case: 'a dry run', options: { dryRun: true }, mode: '--diff' },
    ])('runs jactionlint update $mode over the workflows and composite actions under .github, in the folder holding it, under $case', async ({ options, mode }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), { jactionlint: `printf '%s\\n' "$PWD" "$@" > "${join(workspace.root, 'ran.txt')}"` });
        const graph = await workspace.write(site).graph({ createNodes });
        await execute({ ...(await targetOptions(Upgrade, graph, 'web', 'upgrade--jactionlint')), ...options }, workspace.context(graph, 'web', 'upgrade--jactionlint'));

        const [folder, ...argv] = readFileSync(join(workspace.root, 'ran.txt'), 'utf-8').trim().split('\n');
        expect([folder, argv.slice(0, 2), argv.slice(2).toSorted()]).toStrictEqual([join(workspace.root, 'sites/web'), ['update', mode], ['.github/actions/setup/action.yml', '.github/workflows/ci.yml']]);
    });
});

describe('upgrade commands', () => {
    it('runs a command under its env row with Nx tokens resolved, and the next command without it', async ({ workspace }) => {
        const graph = await workspace.write({ ...site, 'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync(process.argv[2], process.env.UPGRADE_DUMPS ?? 'unset');\n" }).graph({ createNodes });
        const recorded = (file: string) => [execPath, join(workspace.root, 'tools/record.js'), file] as const;

        await execute({ cwd: '.', commands: [{ apply: recorded('moved.txt'), env: { UPGRADE_DUMPS: '{root}/dumps/{projectName}-%p.dmp' } }, { apply: recorded('locked.txt') }] }, workspace.context(graph, 'web', 'upgrade--jactionlint'));

        expect([readFileSync(join(workspace.root, 'moved.txt'), 'utf-8'), readFileSync(join(workspace.root, 'locked.txt'), 'utf-8'), env['UPGRADE_DUMPS']]).toStrictEqual([`${workspace.root}/dumps/web-%p.dmp`, 'unset', undefined]);
    });

    it('prints a command with no preview and runs nothing under a dry run', async ({ workspace }) => {
        const graph = await workspace.write({ ...site, 'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync(process.argv[2], 'ran');\n" }).graph({ createNodes });

        await execute({ cwd: '.', dryRun: true, commands: [{ apply: [execPath, join(workspace.root, 'tools/record.js'), 'moved.txt'] }] }, workspace.context(graph, 'web', 'upgrade--jactionlint'));

        expect(existsSync(join(workspace.root, 'moved.txt'))).toBe(false);
    });
});
