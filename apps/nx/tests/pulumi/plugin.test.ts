import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import pulumi from '@pulumi/pulumi/package.json' with { type: 'json' };
import plugin from '@rasm/nx-pulumi/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Ajv } from 'ajv';
import { Ajv2020 } from 'ajv/dist/2020.js';
import { Duration, Effect, Formatter, Layer, Path, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNREAD = /--version|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: Duration.toMillis('5 minutes'), test: Duration.toMillis('5 minutes') };
const settings = { plugins: [plugin.name] };
const installed = `node_modules/${plugin.name}`;
const passphrase = 'suite';
const ASK = `import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
const workspace = await LocalWorkspace.create({ projectSettings: { name: 'fixture', runtime: 'nodejs' }, envVars: { PULUMI_BACKEND_URL: process.argv[1], PULUMI_CONFIG_PASSPHRASE: '${passphrase}' } });
const held = {};
for (const { name } of await workspace.listStacks()) {
    held[name] = Object.fromEntries(Object.entries(await workspace.stackOutputs(name)).map(([key, { value }]) => [key, value]));
}
console.log(JSON.stringify(held));`;
const files = {
    '.state/held': '',
    'infra/package.json': '{ "name": "infra", "type": "module", "exports": { ".": "./stacks.ts" }, "devDependencies": { "@pulumi/pulumi": "*", "@rasm/nx-pulumi": "*" } }\n',
    'infra/stacks.ts': `const envVars = { PULUMI_BACKEND_URL: new URL('../.state', import.meta.url).href, PULUMI_CONFIG_PASSPHRASE: '${passphrase}' };\n\nexport const stacks = async () => ['dev', 'ops'].map((stackName) => ({ stackName, projectName: 'fixture', envVars, program: async () => ({ greeting: \`hello \${stackName}\` }) }));\n`,
    'libs/plain/package.json': '{ "name": "plain", "type": "module" }\n',
    'libs/plain/index.ts': 'export const plain = 1;\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Held = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Unknown))).annotate({ description: 'Outputs of each stack the Pulumi Automation API lists in one backend, by stack name' });
const Executors = Schema.fromJsonString(Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String, description: Schema.String })) })).annotate({
    description: 'executors.json of an Nx plugin: each executor with its implementation module, option schema file, and description',
});
const Options = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const held = Effect.gen(function* () {
    const [{ evaluated, root }, path] = yield* Effect.all([Consumer, Path.Path]);
    const backend = yield* path.toFileUrl(path.join(root, '.state'));
    return yield* Effect.flatMap(evaluated(ASK, [backend.href], 'infra'), Schema.decodeEffect(Held));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings, packages: [pulumi.name], ignored: ['.state'], files }), Layer.provideMerge(Packed.layer(plugin.name), layer));

it.layer(workspace, { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-pulumi over one program package', (methods) => {
    const deploy = 'README.md TASKS "deploy applies a project\'s infrastructure stacks, -c release those of a released version, and nx run <project>:deploy--pulumi --stack=<name> one of them"';
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'reading the graph writes no file and applies no stack',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${api}: createNodes reads configuration and runs no Pulumi program`);
            assert.deepStrictEqual(yield* held, {}, 'the backend holds no stack before a deploy');
        }),
        bounds.test,
    );

    methods.effect(
        'a package exporting a Pulumi program gets deploy--pulumi under the deploy verb, uncached, with a release configuration',
        Effect.fnUntraced(function* () {
            const { nodes, failures } = yield* read;
            const { infra, plain } = nodes;
            const { deploy: verb, 'deploy--pulumi': applied } = infra?.data.targets ?? {};
            assert.deepStrictEqual(failures, [], 'a valid workspace reads without failure');
            assert.isDefined(applied, deploy);
            assert.isTrue(
                (verb?.dependsOn ?? []).some((row) => row === 'deploy--pulumi' || row === 'deploy--*'),
                'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"',
            );
            assert.notStrictEqual(applied?.cache, true, 'nx/schemas/project-schema.json cache: a deploy changes live resources, and a replayed result applies nothing');
            assert.property(applied?.configurations ?? {}, 'release', deploy);
            assert.notProperty(plain?.data.targets ?? {}, 'deploy--pulumi', `${api}: a package exporting no program gets no target from the plugin`);
        }),
        bounds.test,
    );

    methods.effect(
        'every executor of the installed plugin states a schema file and an implementation module that exist',
        Effect.fnUntraced(function* () {
            const checks = 'nrwl/nx packages/eslint-plugin/src/rules/nx-plugin-checks.ts';
            const { absent, text } = yield* Consumer;
            const { executors } = yield* Effect.flatMap(text(`${installed}/executors.json`), Schema.decodeEffect(Executors));
            const stated = Object.values(executors).flatMap(({ implementation, schema }) => [`${installed}/${schema}`, `${installed}/${implementation.split('#')[0]}.js`]);
            assert.isNotEmpty(stated, `${checks} noExecutorsOrBuildersFound`);
            assert.deepStrictEqual(yield* absent(stated), [], `${checks} invalidSchemaPath "Schema path should point to a valid file", invalidImplementationPath "Implementation path should point to a valid file"`);
        }),
        bounds.test,
    );

    methods.effect(
        'every executor option schema is a valid JSON Schema of the draft it names',
        Effect.fnUntraced(function* () {
            const { text } = yield* Consumer;
            const { executors } = yield* Effect.flatMap(text(`${installed}/executors.json`), Schema.decodeEffect(Executors));
            const schemas = yield* Effect.forEach(Object.values(executors), ({ schema }) => Effect.flatMap(text(`${installed}/${schema}`), Schema.decodeEffect(Options)));
            const drafts = new Map([
                ['http://json-schema.org/draft-07/schema#', new Ajv({ strict: false })],
                ['https://json-schema.org/draft/2020-12/schema', new Ajv2020({ strict: false })],
            ]);
            assert.deepStrictEqual(
                schemas.map((schema) => {
                    const { $schema: draft, type } = schema;
                    return [Predicate.isString(draft) && drafts.get(draft)?.validateSchema(schema), type];
                }),
                schemas.map(() => [true, 'object']),
                'json-schema.org meta-schema of the draft each $schema names; nrwl/nx packages/plugin/src/generators/executor/files/executor/schema.json.template "type": "object"',
            );
        }),
        bounds.test,
    );

    methods.effect(
        'no input of an inferred target is a --version command, plugin source, mise.lock, or a credential variable',
        Effect.fnUntraced(function* () {
            const rows = Object.values((yield* read).nodes)
                .flatMap(({ data }) => Object.values(data.targets ?? {}))
                .flatMap(({ inputs = [] }) => inputs)
                .map((row) => Formatter.format(row));
            assert.deepStrictEqual(
                rows.filter((row) => UNREAD.test(row)),
                [],
                'README.md PRINCIPLES "Task inputs hold the files a task\'s tool reads: plugin and executor source, .config/mise.lock, tool --version output, and credentials are no input"',
            );
        }),
        bounds.test,
    );
    methods.effect(
        'two reads of one workspace give equal projects, with plugin isolation on and off',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const printed = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'infra', '--json']);
            const [first, second, shared] = [yield* printed(true), yield* printed(true), yield* printed(false)];
            assert.deepStrictEqual([second.code, second.stdout], [first.code, first.stdout], `${api}: one set of files gives one set of nodes`);
            assert.deepStrictEqual([shared.code, shared.stdout], [first.code, first.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );

    methods.effect(
        'deploy --stack=<name> applies that stack alone, and deploy applies every stack with the outputs its program returns',
        Effect.fnUntraced(function* () {
            const { nx } = yield* Consumer;
            const one = yield* nx('run', 'infra:deploy--pulumi', '--stack=ops', '--output-style=static');
            const first = yield* held;
            const every = yield* nx('run', 'infra:deploy', '--output-style=static');
            assert.strictEqual(one.code, 0, `nx run infra:deploy--pulumi --stack=ops exits 0: ${one.stdout}${one.stderr}`);
            assert.deepStrictEqual(first, { ops: { greeting: 'hello ops' } }, deploy);
            assert.strictEqual(every.code, 0, `nx run infra:deploy exits 0: ${every.stdout}${every.stderr}`);
            assert.deepStrictEqual(
                Record.map(yield* held, ({ greeting }) => greeting),
                { dev: 'hello dev', ops: 'hello ops' },
                deploy,
            );
        }),
        bounds.test,
    );
});
