import { assert, effect } from '@effect/vitest';
import { createNodes } from '@rasm/nx-pulumi';
import { refused } from '@rasm/testing/errors';
import { it, Workspace } from '@rasm/testing/nx';
import { Effect, Predicate, Record, Schema } from 'effect';
import { describe, expect } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKSPACES = { 'package.json': '{"name":"root","workspaces":["packages/*"]}' };
const PROGRAM = { name: 'infra', bin: './stacks.mjs', dependencies: { '@pulumi/pulumi': '*' } };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Pulumi programs and the packages running them', () => {
    effect.prop(
        'runs a package as a program exactly when it declares @pulumi/pulumi and one bin command',
        {
            bin: Schema.Union([Schema.Undefined, Schema.String.check(Schema.isPattern(/^\.\/.+\.mjs$/u)), Schema.Record(Schema.NonEmptyString, Schema.Literal('./stacks.mjs')).check(Schema.isMaxProperties(3))]),
            declared: Schema.UndefinedOr(Schema.Literals(['dependencies', 'devDependencies'])),
        },
        Effect.fnUntraced(function* ({ bin, declared }) {
            const [workspace, manifest] = yield* Effect.all([Workspace.scoped, Schema.encodeEffect(Json)({ name: 'program', ...(bin === undefined ? {} : { bin }), ...(declared === undefined ? {} : { [declared]: { '@pulumi/pulumi': '*' } }) })]);

            const graph = yield* Effect.promise(() => workspace.write({ ...WORKSPACES, 'packages/program/package.json': manifest }).graph({ createNodes }));

            assert.strictEqual('deploy--pulumi' in (graph.nodes['program']?.data.targets ?? {}), declared !== undefined && (Predicate.isString(bin) ? 1 : Record.size(bin ?? {})) === 1);
        }),
        { arbitrary: { runs: 60 } },
    );

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
