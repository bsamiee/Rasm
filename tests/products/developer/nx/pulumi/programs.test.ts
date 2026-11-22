import { createNodes } from '@rasm/nx-pulumi';
import { refused } from '@rasm/testing/errors';
import { it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACES = { 'package.json': '{"name":"root","workspaces":["packages/*"]}' };
const PROGRAM = { name: 'infra', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*' } };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Pulumi programs', () => {
    it.for([
        { case: 'a string bin and @pulumi/pulumi in dependencies', manifest: PROGRAM, program: true },
        { case: 'one named bin and @pulumi/pulumi in devDependencies', manifest: { name: 'infra', bin: { infra: './stacks.mjs' }, devDependencies: { '@pulumi/pulumi': '*' } }, program: true },
        { case: 'two named bins', manifest: { name: 'infra', bin: { up: './up.mjs', down: './down.mjs' }, dependencies: { '@pulumi/pulumi': '*' } }, program: false },
        { case: 'no bin', manifest: { name: 'infra', dependencies: { '@pulumi/pulumi': '*' } }, program: false },
        { case: 'a bin and no @pulumi/pulumi', manifest: { name: 'infra', bin: './cli.mjs' }, program: false },
    ])('runs a package with $case as a program: $program', async ({ manifest, program }, { workspace }) => {
        const graph = await workspace.write({ ...WORKSPACES, 'packages/infra/package.json': JSON.stringify(manifest) }).graph({ createNodes });

        expect(graph.nodes['infra']?.data.targets?.['deploy--pulumi'] !== undefined).toBe(program);
    });

    it.for([
        { case: 'depends on it', site: { name: 'site', dependencies: { infra: 'workspace:*' } }, hosted: true },
        { case: 'names it in nx.implicitDependencies', site: { name: 'site', nx: { implicitDependencies: ['infra'] } }, hosted: true },
        { case: 'neither depends on it nor names it', site: { name: 'site' }, hosted: false },
    ])('runs the program for a package that $case: $hosted', async ({ site, hosted }, { workspace }) => {
        const graph = await workspace.write({ ...WORKSPACES, 'packages/infra/package.json': JSON.stringify(PROGRAM), 'packages/site/package.json': JSON.stringify(site) }).graph({ createNodes });

        expect(graph.nodes['site']?.data.targets?.['deploy--pulumi'] !== undefined).toBe(hosted);
    });

    it.for([
        { anchor: 'a project.json beside it', files: { 'tools/infra/project.json': '{"name":"infra"}' }, program: true },
        { anchor: 'no project.json and no workspaces entry', files: {}, program: false },
    ])('runs a program outside the workspaces with $anchor: $program', async ({ files, program }, { workspace }) => {
        const graph = await workspace.write({ ...WORKSPACES, ...files, 'tools/infra/package.json': JSON.stringify(PROGRAM) }).graph({ createNodes });

        expect(Object.values(graph.nodes).some(({ data }) => data.root === 'tools/infra' && data.targets?.['deploy--pulumi'] !== undefined)).toBe(program);
    });

    it('refuses a workspace manifest that is no JSON, naming the decode failure', async ({ workspace }) => {
        const read = workspace.write({ ...WORKSPACES, 'packages/infra/package.json': JSON.stringify(PROGRAM), 'packages/torn/package.json': '{ "name": ' }).graph({ createNodes });

        await expect(read).rejects.toSatisfy(refused(/^Expected a valid JSON string/u));
    });
});
