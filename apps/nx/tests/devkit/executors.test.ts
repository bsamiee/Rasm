import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-devkit/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Ajv } from 'ajv';
import { Ajv2020 } from 'ajv/dist/2020.js';
import { Duration, Effect, Layer, Predicate, Schema } from 'effect';
import nx from 'nx/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: Duration.toMillis('5 minutes'), test: Duration.toMillis('3 minutes') };
const settings = { sync: { globalGenerators: [`${plugin.name}:executor-schemas`] }, release: { projects: ['lib'] } };
const installed = `node_modules/${plugin.name}`;
const recorded = (name: string) => `import { writeFileSync } from 'node:fs';\n\nwriteFileSync(new URL('./${name}.json', import.meta.url), JSON.stringify(process.argv.slice(2)));\n`;
const files = {
    'package.json': JSON.stringify({ name: 'consumer', private: true, type: 'module', workspaces: ['lib'], devDependencies: { nx: nx.version, [plugin.name]: '*' } }),
    'tools/pack.js': "import { mkdirSync, writeFileSync } from 'node:fs';\n\nmkdirSync('packed', { recursive: true });\nwriteFileSync('packed/lib.tgz', 'archive');\n",
    'tools/publish.js': recorded('published'),
    'tools/preview.js': recorded('previewed'),
    'tools/tool-clean.js': recorded('cleaned'),
    'lib/index.js': 'export const lib = 1;\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Arguments = Schema.fromJsonString(Schema.Array(Schema.String));
const Collection = Schema.fromJsonString(
    Schema.Struct({
        executors: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String, description: Schema.String }))),
        generators: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ factory: Schema.String, schema: Schema.String, description: Schema.String }))),
    }),
).annotate({ description: 'executors.json or generators.json of an Nx plugin: each entry with its implementation or factory module, option schema file, and description' });
const Options = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown));

// --- [OPERATIONS] ----------------------------------------------------------------------

const prepared = Effect.gen(function* () {
    const { called, committed, root, wrote } = yield* Consumer;
    const targets = {
        pack: { command: `node ${root}/tools/pack.js`, options: { cwd: 'lib' }, outputs: ['{projectRoot}/packed/lib.tgz'] },
        clean: { executor: `${plugin.name}:clean`, options: { targets: ['pack'], commands: [['node', `${root}/tools/tool-clean.js`, 'swept']], cwd: 'lib' } },
        unstated: { executor: `${plugin.name}:clean`, options: { commands: [], cwd: 'lib' } },
        'nx-release-publish': { executor: `${plugin.name}:publish`, dependsOn: ['pack'], options: { publish: ['node', `${root}/tools/publish.js`, 'push'], preview: ['node', `${root}/tools/preview.js`, 'push'] } },
        attach: { executor: `${plugin.name}:attach`, dependsOn: ['pack'] },
    };
    yield* wrote({
        'lib/package.json': yield* Effect.orDie(Schema.encodeEffect(Json)({ name: 'lib', version: '1.2.3', type: 'module', nx: { targets } })),
        'node_modules/.bin/gh': `#!/bin/sh\nprintf '%s\\n' "$@" > "${root}/tools/uploaded.txt"\n`,
    });
    yield* called('chmod', ['+x', 'node_modules/.bin/gh']);
    yield* committed('targets');
});

const collection = (file: string) =>
    Effect.flatMap(
        Consumer.use(({ text }) => text(`${installed}/${file}`)),
        Schema.decodeEffect(Collection),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = Layer.provideMerge(Layer.effectDiscard(prepared), Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings, ignored: ['tools/*.json', 'tools/uploaded.txt', 'packed'], files }), Layer.provideMerge(Packed.layer(plugin.name), layer)));

it.layer(workspace, { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-devkit installed in a workspace', (methods) => {
    const packaged = 'README.md TASKS "Attach and publish targets depend on the targets that package their files"';

    methods.effect(
        'every executor and generator of the installed package states a schema file and a module that exist',
        Effect.fnUntraced(function* () {
            const checks = 'nrwl/nx packages/eslint-plugin/src/rules/nx-plugin-checks.ts';
            const [{ absent }, { executors = {} }, { generators = {} }] = yield* Effect.all([Consumer, collection('executors.json'), collection('generators.json')]);
            const stated = [...Object.values(executors).map(({ implementation, schema }) => [implementation, schema] as const), ...Object.values(generators).map(({ factory, schema }) => [factory, schema] as const)];
            assert.isNotEmpty(stated, `${checks} noExecutorsOrBuildersFound, noGeneratorsOrSchematicsFound`);
            assert.deepStrictEqual(yield* absent(stated.flatMap(([module, schema]) => [`${installed}/${module.split('#')[0]}.js`, `${installed}/${schema}`])), [], `${checks} invalidSchemaPath "Schema path should point to a valid file", invalidImplementationPath "Implementation path should point to a valid file"`);
        }),
        bounds.test,
    );

    methods.effect(
        'every option schema is a valid JSON Schema object',
        Effect.fnUntraced(function* () {
            const [{ text }, { executors = {} }, { generators = {} }] = yield* Effect.all([Consumer, collection('executors.json'), collection('generators.json')]);
            const schemas = yield* Effect.forEach([...Object.values(executors), ...Object.values(generators)], ({ schema }) => Effect.flatMap(text(`${installed}/${schema}`), Schema.decodeEffect(Options)));
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
        'the sync generator finds a workspace holding no executor in sync and writes nothing',
        Effect.fnUntraced(function* () {
            const sync = 'nrwl/nx packages/nx/src/utils/sync-generators.ts SyncGeneratorResult';
            const { nx: run, written } = yield* Consumer;
            const [checked, created] = yield* written(run('sync:check'));
            const [synced, changed] = yield* written(run('sync'));
            assert.strictEqual(checked.code, 0, `${sync}: a generator with no change to make reports in sync: ${checked.stdout}${checked.stderr}`);
            assert.strictEqual(synced.code, 0, `nx sync exits 0: ${synced.stdout}${synced.stderr}`);
            assert.deepStrictEqual([created, changed], [[], []], `${sync}: an in-sync generator writes no file`);
        }),
        bounds.test,
    );

    methods.effect(
        'Nx refuses a clean target missing a required option',
        Effect.fnUntraced(function* () {
            const params = 'nrwl/nx packages/nx/src/utils/params.spec.ts';
            const { nx: run } = yield* Consumer;
            const { code, stdout, stderr } = yield* run('run', 'lib:unstated', '--output-style=static');
            assert.notStrictEqual(code, 0, `${params}: options missing a required property fail validation`);
            assert.include(`${stdout}${stderr}`, "Required property 'targets' is missing", `${params} "Required property 'a' is missing"`);
        }),
        bounds.test,
    );

    methods.effect(
        'publish under a dry run runs the preview command and publishes nothing',
        Effect.fnUntraced(function* () {
            const { absent, nx: run, text } = yield* Consumer;
            const { code, stdout, stderr } = yield* run('release', 'publish', '--dry-run');
            assert.strictEqual(code, 0, `nx release publish --dry-run exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual(yield* absent(['tools/published.json']), ['tools/published.json'], 'nrwl/nx packages/nx/src/command-line/release/publish.ts: a dry run publishes nothing');
            assert.strictEqual((yield* Effect.flatMap(text('tools/previewed.json'), Schema.decodeEffect(Arguments)))[0], 'push', "the tool's own dry-run form runs in place of publish");
        }),
        bounds.test,
    );

    methods.effect(
        'publish runs the tool command with each file its packaging target declares',
        Effect.fnUntraced(function* () {
            const publish = 'README.md DELIVERY "Publishing delivers those files by each destination\'s own command"';
            const { nx: run, text } = yield* Consumer;
            const { code, stdout, stderr } = yield* run('release', 'publish');
            const passed = yield* Effect.flatMap(text('tools/published.json'), Schema.decodeEffect(Arguments));
            assert.strictEqual(code, 0, `nx release publish exits 0: ${stdout}${stderr}`);
            assert.strictEqual(passed[0], 'push', `${publish}: the stated publish arguments come first`);
            assert.isTrue(
                passed.some((file) => file.endsWith('lib/packed/lib.tgz')),
                `${publish}; ${packaged}: the command receives lib/packed/lib.tgz, the declared output of pack`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'attach uploads the packaged files to the release of the version tag through gh release upload --clobber',
        Effect.fnUntraced(function* () {
            const attach = 'README.md DELIVERY "Releases give every project one version and tag through nx release, and the GitHub release holds the packaged files each attach target uploads"';
            const { nx: run, text } = yield* Consumer;
            const { code, stdout, stderr } = yield* run('run', 'lib:attach', '--output-style=static');
            const passed = (yield* text('tools/uploaded.txt')).trim().split('\n');
            const [command, subcommand, tag] = passed;
            assert.strictEqual(code, 0, `nx run lib:attach exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual([command, subcommand, tag], ['release', 'upload', 'v1.2.3'], `${attach}; nrwl/nx packages/nx/src/command-line/release/config/config.ts defaultFixedReleaseTagPattern "v{version}"; cli.github.com/manual/gh_release_upload "gh release upload <tag> <files>..."`);
            assert.include(passed, '--clobber', 'cli.github.com/manual/gh_release_upload --clobber "Overwrite existing assets of the same name"');
            assert.isTrue(
                passed.some((file) => file.endsWith('lib/packed/lib.tgz')),
                `${attach}; ${packaged}: the upload names lib/packed/lib.tgz, the declared output of pack`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'clean removes each declared output, keeps source files, and runs the tool clean command',
        Effect.fnUntraced(function* () {
            const clean = 'README.md TASKS "clean removes the outputs a project\'s targets declare and runs its tool\'s clean"';
            const { absent, nx: run, text } = yield* Consumer;
            yield* run('run', 'lib:pack', '--output-style=static');
            const { code, stdout, stderr } = yield* run('run', 'lib:clean', '--output-style=static');
            assert.strictEqual(code, 0, `nx run lib:clean exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual(yield* absent(['lib/packed/lib.tgz', 'lib/index.js']), ['lib/packed/lib.tgz'], `${clean}: the declared output goes and a source file stays`);
            assert.deepStrictEqual(yield* Effect.flatMap(text('tools/cleaned.json'), Schema.decodeEffect(Arguments)), ['swept'], `${clean}: the tool's own clean command runs`);
        }),
        bounds.test,
    );
});
