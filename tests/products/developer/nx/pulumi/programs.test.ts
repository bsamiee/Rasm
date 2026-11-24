import { createNodes } from '@rasm/nx-pulumi';
import { refused } from '@rasm/testing/errors';
import { it } from '@rasm/testing/nx';
import { asyncProperty, assert, constant, constantFrom, dictionary, oneof, record, string } from 'fast-check';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACES = { 'package.json': '{"name":"root","workspaces":["packages/*"]}' };
const PROGRAM = { name: 'infra', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*' } };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Pulumi programs and the packages running them', () => {
    it('runs a package as a program exactly when it declares @pulumi/pulumi and one bin command', async ({ workspace }) => {
        await assert(
            asyncProperty(
                record({
                    stated: oneof(
                        constant({ bin: {}, commands: 0 }),
                        string({ minLength: 1 }).map((path) => ({ bin: { bin: `./${path}.mjs` }, commands: 1 })),
                        dictionary(string({ minLength: 1 }), constant('./stacks.mjs'), { maxKeys: 3 }).map((bin) => ({ bin: { bin }, commands: Object.keys(bin).length })),
                    ),
                    declared: constantFrom('none', 'dependencies', 'devDependencies'),
                }),
                async ({ stated: { bin, commands }, declared }) => {
                    const manifest = { name: 'program', ...bin, ...(declared === 'none' ? {} : { [declared]: { '@pulumi/pulumi': '*' } }) };

                    const graph = await workspace.write({ ...WORKSPACES, 'packages/program/package.json': JSON.stringify(manifest) }).graph({ createNodes });

                    expect({ manifest, program: 'deploy--pulumi' in (graph.nodes['program']?.data.targets ?? {}) }).toStrictEqual({ manifest, program: declared !== 'none' && commands === 1 });
                },
            ),
            { numRuns: 60 },
        );
    });

    it('runs the program for each package holding at its root the file the program manifest names, where a project.json or the workspaces anchor it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...WORKSPACES,
                'packages/infra/package.json': JSON.stringify({ ...PROGRAM, nx: { metadata: { pulumi: { hosts: 'hosting.config.ts' } } } }),
                'packages/site/package.json': '{"name":"site"}',
                'packages/site/hosting.config.ts': 'export default {};\n',
                'packages/nested/package.json': '{"name":"nested"}',
                'packages/nested/server/hosting.config.ts': 'export default {};\n',
                'packages/dependent/package.json': JSON.stringify({ name: 'dependent', dependencies: { infra: 'workspace:*' } }),
                'packages/silent/package.json': JSON.stringify({ ...PROGRAM, name: 'silent' }),
                'tools/marked/project.json': '{"name":"marked"}',
                'tools/marked/package.json': JSON.stringify({ ...PROGRAM, name: 'marked' }),
                'tools/unlisted/package.json': JSON.stringify({ ...PROGRAM, name: 'unlisted' }),
                'tools/unlisted/hosting.config.ts': 'export default {};\n',
            })
            .graph({ createNodes });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ data: { root, targets = {} } }) => [root, 'deploy--pulumi' in targets]))).toStrictEqual({
            'packages/infra': true,
            'packages/site': true,
            'packages/nested': false,
            'packages/dependent': false,
            'packages/silent': true,
            'tools/marked': true,
        });
    });

    it('refuses a workspace manifest that is no JSON, naming the decode failure', async ({ workspace }) => {
        const read = workspace.write({ ...WORKSPACES, 'packages/torn/package.json': '{ "name": ' }).graph({ createNodes });

        await expect(read).rejects.toSatisfy(refused(/^Expected a valid JSON string/u));
    });
});
