import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-pulumi';
import { it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACES = { 'package.json': '{"name":"root","workspaces":["packages/*"]}' };
const RECORDING = "import { writeFileSync } from 'node:fs';\nwriteFileSync(new URL('ran.json', import.meta.url), JSON.stringify({ cwd: process.cwd(), argv: process.argv.slice(2) }));\n";
const PROGRAM = { 'packages/infra/package.json': JSON.stringify({ name: 'infra', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*' }, nx: { metadata: { pulumi: { hosts: 'hosting.config.ts' } } } }), 'packages/infra/stacks.mjs': RECORDING };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('program runs', () => {
    it.for([
        { case: 'install--pulumi of a program', files: PROGRAM, project: 'infra', target: 'install--pulumi', program: 'infra', cwd: 'infra', argv: ['install'] },
        { case: 'deploy--pulumi of a program', files: PROGRAM, project: 'infra', target: 'deploy--pulumi', program: 'infra', cwd: 'infra', argv: ['deploy'] },
        { case: 'dry-run--pulumi of a program', files: PROGRAM, project: 'infra', target: 'dry-run--pulumi', program: 'infra', cwd: 'infra', argv: ['deploy', '--operation=preview'] },
        { case: 'deploy--pulumi of a package a program hosts', files: { ...PROGRAM, 'packages/site/package.json': '{"name":"site"}', 'packages/site/hosting.config.ts': 'export default {};\n' }, project: 'site', target: 'deploy--pulumi', program: 'infra', cwd: 'site', argv: ['deploy'] },
        {
            case: 'deploy--pulumi of a program whose path the shell would split',
            files: { 'packages/infra/package.json': JSON.stringify({ name: 'infra', bin: './my stacks.mjs', dependencies: { '@pulumi/pulumi': '*' } }), 'packages/infra/my stacks.mjs': RECORDING },
            project: 'infra',
            target: 'deploy--pulumi',
            program: 'infra',
            cwd: 'infra',
            argv: ['deploy'],
        },
    ])('$case runs the program of packages/$program in packages/$cwd with $argv', async ({ files, project, target, program, cwd, argv }, { workspace }) => {
        const graph = await workspace.write({ ...WORKSPACES, ...files }).graph({ createNodes });

        const { success } = await workspace.run(graph, project, target);

        expect(success).toBe(true);
        expect(JSON.parse(readFileSync(join(workspace.root, 'packages', program, 'ran.json'), 'utf-8'))).toStrictEqual({ cwd: join(workspace.root, 'packages', cwd), argv });
    });
});
