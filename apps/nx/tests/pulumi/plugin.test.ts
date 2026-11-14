import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import pulumi from '@pulumi/pulumi/package.json' with { type: 'json' };
import plugin from '@rasm/nx-pulumi/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Formatter, Layer, Path, Predicate, Record, Schema } from 'effect';
import effect from 'effect/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNREAD = /(?:^|[\s"'])-{0,2}version\b|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const REFUSAL = /missing.*dev.*ops.*edge/su;
const bounds = { built: Duration.toMillis('5 minutes'), test: Duration.toMillis('5 minutes') };
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
    '.state/order.log': '',
    'infra/package.json': '{ "name": "infra", "type": "module", "exports": { ".": "./stacks.ts" }, "devDependencies": { "@pulumi/pulumi": "*", "@rasm/nx-pulumi": "*", "effect": "*" } }\n',
    'infra/stacks.ts': `import { appendFileSync, readFileSync } from 'node:fs';
import { setTimeout as waited } from 'node:timers/promises';
import { Effect } from 'effect';
import secrets from './secrets.json' with { type: 'json' };

const envVars = { PULUMI_BACKEND_URL: new URL('../.state', import.meta.url).href, ...secrets };
const noted = (line) => appendFileSync(new URL('../.state/order.log', import.meta.url), \`\${line}\\n\`);
const stated = (stackName, torn = false) => ({
    stackName,
    projectName: 'fixture',
    envVars,
    program: async () => {
        noted(\`start \${stackName}\`);
        await waited(1000);
        if (torn) {
            throw new Error('torn');
        }
        noted(\`end \${stackName}\`);
        return { greeting: \`\${readFileSync(new URL('./greeting.txt', import.meta.url), 'utf8').trim()} \${stackName}\` };
    },
});
const groups = {
    Install: () => [],
    Deploy: ({ production }) => (production ? [[stated('live')], [stated('torn', true)], [stated('late')]] : [[stated('dev'), stated('ops')], [stated('edge')]]),
};

export const stacks = (request) => Effect.succeed(groups[request._tag](request));
`,
    'infra/secrets.json': `{ "PULUMI_CONFIG_PASSPHRASE": "${passphrase}" }\n`,
    'infra/greeting.txt': 'hello\n',
    'libs/plain/package.json': '{ "name": "plain", "type": "module" }\n',
    'libs/plain/index.ts': 'export const plain = 1;\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Held = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Unknown))).annotate({ description: 'Outputs of each stack the Pulumi Automation API lists in one backend, by stack name' });
const Executors = Schema.fromJsonString(Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String, description: Schema.String })) })).annotate({
    description: 'executors.json of an Nx plugin: each executor with its implementation module, option schema file, and description',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const held = Effect.gen(function* () {
    const [{ evaluated, root }, path] = yield* Effect.all([Consumer, Path.Path]);
    const backend = yield* path.toFileUrl(path.join(root, '.state'));
    return yield* Effect.flatMap(evaluated(ASK, [backend.href], 'infra'), Schema.decodeEffect(Held));
});

const applied = Effect.fnUntraced(function* (...args: readonly string[]) {
    const { nx, text, wrote } = yield* Consumer;
    yield* wrote({ '.state/order.log': '' });
    const { code, stdout, stderr } = yield* nx('run', ...args, '--output-style=static');
    return { code, printed: `${stdout}${stderr}`, order: (yield* text('.state/order.log')).trim().split('\n'), held: yield* held };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>>) => Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, packages: [pulumi.name, effect.name], ignored: ['.state'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const DEPLOY = 'README.md TASKS "deploy applies a project\'s infrastructure stacks, -c release those of a released version, and nx run <project>:deploy--pulumi --stack=<name> one of them"';
    const GROUPS = 'pulumi.com/docs/iac/concepts/stacks "Stack references": a stack reading the outputs of another applies after it; @rasm/nx-pulumi/stacks Stacks: stacks(request) returns ordered groups';
    const API = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';
    methods.effect(
        'reading the graph writes no file and applies no stack',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${API}: createNodes reads configuration and runs no Pulumi program`);
            assert.deepStrictEqual(yield* held, {}, 'the backend holds no stack before a deploy');
        }),
        bounds.test,
    );

    methods.effect(
        'a package exporting a Pulumi program gets deploy--pulumi under the deploy verb, uncached, with a release configuration',
        Effect.fnUntraced(function* () {
            const { nodes, failures } = yield* read;
            const { infra, plain } = nodes;
            const { deploy: verb, 'deploy--pulumi': deployed } = infra?.data.targets ?? {};
            assert.deepStrictEqual(failures, [], 'a valid workspace reads without failure');
            assert.isDefined(deployed, DEPLOY);
            assert.isTrue(
                (verb?.dependsOn ?? []).some((row) => row === 'deploy--pulumi' || row === 'deploy--*'),
                'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"',
            );
            assert.notStrictEqual(deployed?.cache, true, 'nx/schemas/project-schema.json cache: a deploy changes live resources, and a replayed result applies nothing');
            assert.property(deployed?.configurations ?? {}, 'release', DEPLOY);
            assert.notProperty(plain?.data.targets ?? {}, 'deploy--pulumi', `${API}: a package exporting no program gets no target from the plugin`);
        }),
        bounds.test,
    );

    methods.effect(
        'deploy--pulumi waits for the deploys of its dependencies and for no check, and runs beside no other task',
        Effect.fnUntraced(function* () {
            const { infra } = (yield* read).nodes;
            const { 'deploy--pulumi': deployed } = infra?.data.targets ?? {};
            const rows = (deployed?.dependsOn ?? []).map((row) => (Predicate.isString(row) ? row : row.target));
            assert.include(rows, '^deploy', `${GROUPS}: a project deploys after the projects it depends on`);
            assert.deepStrictEqual(
                rows.filter((row) => row.includes('check')),
                [],
                'owner ruling 2026-11-13 "No target may wait on a dependency it does not read": an apply reads no checker result',
            );
            assert.isFalse(deployed?.parallelism, 'nx/schemas/project-schema.json parallelism; pulumi.com/docs/iac/concepts/state-and-backends "Concurrency control": one update holds the lock of a stack');
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
        'no input of an inferred target is a version command (--version, -version, <tool> version), plugin source, mise.lock, or a credential variable',
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
            assert.deepStrictEqual([second.code, second.stdout], [first.code, first.stdout], `${API}: one set of files gives one set of nodes`);
            assert.deepStrictEqual([shared.code, shared.stdout], [first.code, first.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );

    methods.effect(
        'deploy --stack=<name> applies that stack alone with the outputs its program returns',
        Effect.fnUntraced(function* () {
            const one = yield* applied('infra:deploy--pulumi', '--stack=ops');
            assert.strictEqual(one.code, 0, `nx run infra:deploy--pulumi --stack=ops exits 0: ${one.printed}`);
            assert.deepStrictEqual(one.held, { ops: { greeting: 'hello ops' } }, DEPLOY);
        }),
        bounds.test,
    );

    methods.effect(
        'deploy applies every stack, a later group starting after each stack of the earlier group ends',
        Effect.fnUntraced(function* () {
            const every = yield* applied('infra:deploy');
            assert.strictEqual(every.code, 0, `nx run infra:deploy exits 0: ${every.printed}`);
            assert.deepStrictEqual(
                Record.map(every.held, ({ greeting }) => greeting),
                { dev: 'hello dev', ops: 'hello ops', edge: 'hello edge' },
                DEPLOY,
            );
            assert.deepStrictEqual(Array.take(every.order, 2).toSorted(), ['start dev', 'start ops'], `${GROUPS}: the stacks of one group apply at once`);
            assert.deepStrictEqual(Array.takeRight(every.order, 2), ['start edge', 'end edge'], `${GROUPS}: the second group starts after both stacks of the first end`);
        }),
        bounds.test,
    );

    methods.effect(
        'an unknown --stack fails naming every stack the program states, and applies none',
        Effect.fnUntraced(function* () {
            const missing = yield* applied('infra:deploy--pulumi', '--stack=missing');
            assert.notStrictEqual(missing.code, 0, `${DEPLOY}: a stack the program does not state is no stack to apply`);
            assert.match(missing.printed, REFUSAL, `root CLAUDE.md "Messages ... exact subject, values over prose": the refusal names the stack asked for and each stack stated: ${missing.printed}`);
            assert.deepStrictEqual(missing.order, [''], 'no program runs');
        }),
        bounds.test,
    );

    methods.effect(
        'a run whose program states no stack fails, naming the project and no absent value',
        Effect.fnUntraced(function* () {
            const empty = yield* applied('infra:install');
            assert.notStrictEqual(empty.code, 0, 'README.md TASKS "install puts a local build into its host or starts local services": a run that applies nothing reports no success');
            assert.include(empty.printed, 'infra', empty.printed);
            assert.notInclude(empty.printed, 'undefined', 'root CLAUDE.md "Messages ... exact subject, values over prose"');
        }),
        bounds.test,
    );

    methods.effect(
        'operation preview changes no stack, and operation destroy removes the stack',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* wrote({ 'infra/greeting.txt': 'goodbye\n' });
            const previewed = yield* applied('infra:deploy--pulumi', '--stack=edge', '--operation=preview');
            const destroyed = yield* applied('infra:deploy--pulumi', '--stack=edge', '--operation=destroy');
            const { edge } = previewed.held;
            assert.strictEqual(previewed.code, 0, `nx run infra:deploy--pulumi --stack=edge --operation=preview exits 0: ${previewed.printed}`);
            assert.deepStrictEqual(edge, { greeting: 'hello edge' }, 'pulumi.com/docs/iac/cli/commands/pulumi_preview "Show a preview of updates to a stack\'s resources": a preview applies nothing');
            assert.strictEqual(destroyed.code, 0, `nx run infra:deploy--pulumi --stack=edge --operation=destroy exits 0: ${destroyed.printed}`);
            assert.notProperty(destroyed.held, 'edge', 'pulumi.com/docs/reference/pkg/nodejs/pulumi/pulumi/classes/automation.Stack.html destroy "remove": the stack and its configuration go after its resources');
            assert.property(destroyed.held, 'dev', DEPLOY);
        }),
        bounds.test,
    );

    methods.effect(
        'deploy -c release applies the stacks of a released version, and a group that fails leaves each later group unapplied',
        Effect.fnUntraced(function* () {
            const released = yield* applied('infra:deploy', '--configuration=release');
            const { live } = released.held;
            assert.notStrictEqual(released.code, 0, `${GROUPS}: the run of a program whose second group fails reports failure`);
            assert.deepStrictEqual(live, { greeting: 'goodbye live' }, DEPLOY);
            assert.deepStrictEqual(released.order, ['start live', 'end live', 'start torn'], `${GROUPS}: group late starts no program after group torn fails`);
            assert.notProperty(released.held, 'late', GROUPS);
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'libs/torn/package.json': '{ "name": ' }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'a package manifest that fails to parse is named in the failure, and the program package keeps its targets',
        Effect.fnUntraced(function* () {
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            const { nodes, failures } = yield* read;
            const { infra } = nodes;
            assert.include(
                failures.flatMap((failure) => failure.files).map(({ file }) => file),
                'libs/torn/package.json',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.property(infra?.data.targets ?? {}, 'deploy--pulumi', `${errors}: "It allows Nx to recieve partial results and continue processing"`);
        }),
        bounds.test,
    );
});
