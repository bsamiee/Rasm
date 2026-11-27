import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { env, execPath } from 'node:process';
import { execute } from '@rasm/nx/executors/upgrade';
import { it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const repository = { 'package.json': '{ "name": "repo" }' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('upgrade commands', () => {
    it('runs a command under its env row as written, and the next command without it', async ({ workspace }) => {
        const graph = await workspace.write({ ...repository, 'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync(process.argv[2], process.env.UPGRADE_DUMPS ?? 'unset');\n" }).graph({});
        const recorded = (file: string) => [execPath, join(workspace.root, 'tools/record.js'), file] as const;

        await execute({ cwd: '.', commands: [{ apply: recorded('moved.txt'), env: { UPGRADE_DUMPS: '{root}/dumps/{projectName}-%p.dmp' } }, { apply: recorded('locked.txt') }] }, workspace.context(graph, 'repo', 'upgrade'));

        expect([readFileSync(join(workspace.root, 'moved.txt'), 'utf-8'), readFileSync(join(workspace.root, 'locked.txt'), 'utf-8'), env['UPGRADE_DUMPS']]).toStrictEqual(['{root}/dumps/{projectName}-%p.dmp', 'unset', undefined]);
    });

    it('runs nothing under a dry run of a command stating no preview', async ({ workspace }) => {
        const graph = await workspace.write({ ...repository, 'tools/record.js': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync(process.argv[2], 'ran');\n" }).graph({});

        await execute({ cwd: '.', dryRun: true, commands: [{ apply: [execPath, join(workspace.root, 'tools/record.js'), 'moved.txt'] }] }, workspace.context(graph, 'repo', 'upgrade'));

        expect(existsSync(join(workspace.root, 'moved.txt'))).toBe(false);
    });
});
