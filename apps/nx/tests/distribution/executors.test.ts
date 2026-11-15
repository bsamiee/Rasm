import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import js from '@nx/js/package.json' with { type: 'json' };
import plugin from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Layer, Schema } from 'effect';
import nx from 'nx/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ATTACH = 'README.md DELIVERY "Each release tag gets its own GitHub release, holding every file its project packages"';
const TAGGED = /git tag --annotate (?<tag>\S+)/u;
const bounds = { built: Duration.toMillis('5 minutes'), test: Duration.toMillis('3 minutes') };
const installed = `node_modules/${plugin.name}`;
const recorded = (name: string) => `import { writeFileSync } from 'node:fs';\n\nwriteFileSync(new URL('./${name}.json', import.meta.url), JSON.stringify(process.argv.slice(2)));\n`;
const files = {
    'package.json': JSON.stringify({ name: 'consumer', private: true, type: 'module', workspaces: ['lib'], devDependencies: { nx: nx.version, [js.name]: js.version, [plugin.name]: '*' } }),
    'tools/pack.js': "import { mkdirSync, writeFileSync } from 'node:fs';\n\nmkdirSync('packed', { recursive: true });\nwriteFileSync('packed/lib.tgz', 'archive');\n",
    'tools/stage.js': "import { writeFileSync } from 'node:fs';\n\nwriteFileSync('staged/built.txt', 'built');\n",
    'tools/publish.js': "import { writeFileSync } from 'node:fs';\n\nconst argv = process.argv.slice(2);\nwriteFileSync(new URL(argv.includes('--dry-run') ? './previewed.json' : './published.json', import.meta.url), JSON.stringify(argv));\n",
    'tools/keyed.js': recorded('keyed'),
    'tools/unkeyed.js': recorded('unkeyed'),
    'tools/alone.js': recorded('alone'),
    'tools/built.js': recorded('built'),
    'tools/tool-clean.js': recorded('cleaned'),
    'lib/index.js': 'export const lib = 1;\n',
    'lib/staged/tracked.txt': 'source\n',
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const prepared = Effect.gen(function* () {
    const { called, committed, root, wrote } = yield* Consumer;
    const targets = {
        pack: { command: `node ${root}/tools/pack.js`, options: { cwd: 'lib' }, outputs: ['{projectRoot}/packed/lib.tgz'] },
        stage: { command: `node ${root}/tools/stage.js`, options: { cwd: 'lib' }, outputs: ['{projectRoot}/staged'] },
        hollow: { command: 'true', outputs: ['{projectRoot}/absent/*.tgz'] },
        clean: { executor: `${plugin.name}:clean`, options: { targets: ['pack', 'stage'], commands: [['node', `${root}/tools/tool-clean.js`, 'swept']], cwd: 'lib' } },
        unstated: { executor: `${plugin.name}:clean` },
        'nx-release-publish': { executor: `${plugin.name}:publish`, dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }], options: { publish: ['node', `${root}/tools/publish.js`, 'push'], preview: ['--dry-run'], credential: 'HOME' } },
        keyed: { executor: `${plugin.name}:publish`, dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }], options: { publish: ['node', `${root}/tools/keyed.js`, 'push'], credential: 'HOME', flag: '--api-key' } },
        unkeyed: { executor: `${plugin.name}:publish`, dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }], options: { publish: ['node', `${root}/tools/unkeyed.js`, 'push'], credential: 'RASM_FIXTURE_ABSENT_TOKEN' } },
        alone: { executor: `${plugin.name}:publish`, options: { publish: ['node', `${root}/tools/alone.js`, 'push'], credential: 'HOME' } },
        built: { executor: `${plugin.name}:publish`, dependsOn: ['stage'], options: { publish: ['node', `${root}/tools/built.js`, 'push'], credential: 'HOME' } },
        attach: { executor: `${plugin.name}:attach`, dependsOn: ['pack'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }] },
        bare: { executor: `${plugin.name}:attach`, dependsOn: ['hollow'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }] },
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

const uploaded = Effect.gen(function* () {
    const { absent, nx: run, text } = yield* Consumer;
    const { code, stdout, stderr } = yield* run('run', 'lib:attach', '--output-style=static');
    const passed = Array.isReadonlyArrayEmpty(yield* absent(['tools/uploaded.txt'])) ? (yield* text('tools/uploaded.txt')).trim().split('\n') : [];
    return { code, printed: `${stdout}${stderr}`, passed };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (release: object) =>
    Layer.provideMerge(
        Layer.effectDiscard(prepared),
        Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { sync: { globalGenerators: [`${plugin.name}:executor-schemas`] }, release }, ignored: ['tools/*.json', 'tools/uploaded.txt', 'packed', 'built.txt'], files }), Layer.provideMerge(Packed.layer(plugin.name), layer)),
    );

it.layer(workspace({ projects: ['lib'], projectsRelationship: 'independent' }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const PACKAGED = 'README.md TASKS "Attach and publish targets depend on the targets that package their files"';
    const RELEASED = 'README.md DELIVERY "Attach, publish targets, and the Sparkle feed act on projects tagged at the released commit, and a project without a tag there uploads nothing"';

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
        'a clean target stating no option runs under the defaults its schema states and removes nothing',
        Effect.fnUntraced(function* () {
            const params = 'nrwl/nx packages/nx/src/utils/params.ts combineOptionsForExecutor: setDefaults fills each property stating a default before validateOptsAgainstSchema reads required';
            const { nx: run, written } = yield* Consumer;
            const [{ code, stdout, stderr }, created] = yield* written(run('run', 'lib:unstated', '--output-style=static'));
            assert.strictEqual(code, 0, `${params}: ${stdout}${stderr}`);
            assert.deepStrictEqual(created, [], `${params}: default targets [] names no output to remove and default commands [] runs none`);
        }),
        bounds.test,
    );

    methods.effect(
        'publish skips a commit holding no tag of its project, and uploads where its credential variable is set and names the variable and succeeds where it is not',
        Effect.fnUntraced(function* () {
            const rule = 'README.md PRINCIPLES "Publish targets upload where the credential variable their tool reads is set, and name the variable and succeed where it is not"';
            const { absent, called, nx: run, text } = yield* Consumer;
            const untagged = yield* run('run', 'lib:keyed', '--output-style=static');
            assert.strictEqual(untagged.code, 0, `${RELEASED}: a commit holding no tag of lib exits 0: ${untagged.stdout}${untagged.stderr}`);
            assert.include(`${untagged.stdout}${untagged.stderr}`, 'HEAD holds no tag of lib', 'root CLAUDE.md "Messages ... exact subject, values over prose": the skip names the project HEAD holds no tag of');
            assert.deepStrictEqual(yield* absent(['tools/keyed.json']), ['tools/keyed.json'], `${RELEASED}: the publish command does not run`);
            yield* Effect.flatMap(called('git', ['tag', '--annotate', 'lib@1.2.3', '--message', 'lib@1.2.3']), ({ checked }) => checked);
            const skipped = yield* run('run', 'lib:unkeyed', '--output-style=static');
            assert.strictEqual(skipped.code, 0, `${rule}: a missing credential exits 0: ${skipped.stdout}${skipped.stderr}`);
            assert.include(`${skipped.stdout}${skipped.stderr}`, 'RASM_FIXTURE_ABSENT_TOKEN is not set', `${rule}: the run names the missing variable`);
            assert.deepStrictEqual(yield* absent(['tools/unkeyed.json']), ['tools/unkeyed.json'], `${rule}: the publish command does not run`);
            const keyed = yield* run('run', 'lib:keyed', '--output-style=static');
            const passed = yield* Effect.flatMap(text('tools/keyed.json'), Schema.decodeEffect(Arguments));
            assert.strictEqual(keyed.code, 0, `${rule}: a set credential publishes: ${keyed.stdout}${keyed.stderr}`);
            assert.strictEqual(passed.at(-2), '--api-key', 'dotnet nuget push --help "-k|--api-key <API_KEY>": a tool reading no variable takes the credential under its own argument, after the files');
            assert.deepStrictEqual(passed.slice(1, -2), ['lib/packed/lib.tgz'], `${PACKAGED}: the command receives the packed file alone`);
            const alone = yield* run('run', 'lib:alone', '--output-style=static');
            assert.strictEqual(alone.code, 0, `a publish target with no packaging dependency runs its command alone: ${alone.stdout}${alone.stderr}`);
            assert.deepStrictEqual(yield* Effect.flatMap(text('tools/alone.json'), Schema.decodeEffect(Arguments)), ['push'], 'a publish target naming no dependency passes its command no file');
            const built = yield* run('run', 'lib:built', '--output-style=static');
            const order = 'README.md TASKS "`pnpm publish --recursive` states no such input, takes no file, and depends on the build targets of its package"; pnpm.io/cli/publish --recursive "will publish all the packages that have versions not yet published to the registry"';
            assert.strictEqual(built.code, 0, `${order}: ${built.stdout}${built.stderr}`);
            assert.deepStrictEqual(yield* absent(['lib/staged/built.txt']), [], `${order}: stage, the target built depends on, ran before the publish command`);
            assert.deepStrictEqual(yield* Effect.flatMap(text('tools/built.json'), Schema.decodeEffect(Arguments)), ['push'], `${order}: built states no dependentTasksOutputFiles input, and its command receives no file stage wrote`);
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
            assert.deepStrictEqual(yield* Effect.flatMap(text('tools/previewed.json'), Schema.decodeEffect(Arguments)), ['push', 'lib/packed/lib.tgz', '--dry-run'], "docs.astral.sh/uv/reference/cli/#uv-publish--dry-run: the tool's own dry run runs in place of publish, its arguments after the files");
        }),
        bounds.test,
    );

    methods.effect(
        'publish runs the tool command with each file its packaging target declares',
        Effect.fnUntraced(function* () {
            const publish = 'README.md DELIVERY "Publishing delivers through each destination\'s own command"';
            const { nx: run, text } = yield* Consumer;
            const { code, stdout, stderr } = yield* run('release', 'publish');
            const passed = yield* Effect.flatMap(text('tools/published.json'), Schema.decodeEffect(Arguments));
            assert.strictEqual(code, 0, `nx release publish exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual(passed, ['push', 'lib/packed/lib.tgz'], `${publish}; ${PACKAGED}: the stated publish arguments come first, then lib/packed/lib.tgz, the declared output of pack`);
        }),
        bounds.test,
    );

    methods.effect(
        'attach skips a commit holding no tag of its project, and uploads the packaged files to the release of its own tag among the tags at the commit',
        Effect.fnUntraced(function* () {
            const independent = 'nrwl/nx packages/nx/src/command-line/release/config/config.ts defaultIndependentReleaseTagPattern: `{projectName}@{version}`';
            const { absent, called, committed, nx: run } = yield* Consumer;
            const tagged = (name: string) => Effect.flatMap(called('git', ['tag', '--annotate', name, '--message', name]), ({ checked }) => checked);
            yield* committed('untagged');
            yield* tagged('other@2.0.0');
            const untagged = yield* uploaded;
            assert.strictEqual(untagged.code, 0, `${RELEASED}: a commit holding no tag of lib exits 0: ${untagged.printed}`);
            assert.include(untagged.printed, 'HEAD holds no tag of lib', 'root CLAUDE.md "Messages ... exact subject, values over prose": the skip names the project HEAD holds no tag of');
            assert.deepStrictEqual(untagged.passed, [], `${RELEASED}: gh runs for no upload`);
            const previewed = yield* run('run', 'lib:attach', '--dryRun', '--output-style=static');
            assert.include(`${previewed.stdout}${previewed.stderr}`, 'Would run gh release upload', 'workflow release.yml dry-run input "Previews each project\'s version, tag, and GitHub release": a dry run previews the upload of a project nx release has not tagged yet');
            assert.deepStrictEqual(yield* absent(['tools/uploaded.txt']), ['tools/uploaded.txt'], 'a dry run runs gh for no upload');
            yield* tagged('lib@1.2.4');
            const { code, printed, passed } = yield* uploaded;
            const [command, subcommand, tag, ...sent] = passed;
            assert.strictEqual(code, 0, `nx run lib:attach exits 0: ${printed}`);
            assert.deepStrictEqual([command, subcommand, tag], ['release', 'upload', 'lib@1.2.4'], `${ATTACH}; ${independent}; cli.github.com/manual/gh_release_upload "gh release upload <tag> <files>...": other@2.0.0 at the same commit belongs to another project`);
            assert.deepStrictEqual(sent, ['lib/packed/lib.tgz', '--clobber'], `${ATTACH}; ${PACKAGED}; cli.github.com/manual/gh_release_upload --clobber "Overwrite existing assets of the same name": the upload names lib/packed/lib.tgz, the declared output of pack, alone`);
        }),
        bounds.test,
    );

    methods.effect(
        'attach fails where its packaging target wrote no file, and uploads nothing',
        Effect.fnUntraced(function* () {
            const { nx: run, text, wrote } = yield* Consumer;
            yield* wrote({ 'tools/uploaded.txt': '' });
            const { code, stdout, stderr } = yield* run('run', 'lib:bare', '--output-style=static');
            assert.notStrictEqual(code, 0, `${ATTACH}; cli.github.com/manual/gh_release_upload "gh release upload <tag> <files>...": an upload names at least one file`);
            assert.include(`${stdout}${stderr}`, 'lib/absent', 'root CLAUDE.md "Messages ... exact subject, values over prose": the failure names the declared output holding no file');
            assert.strictEqual(yield* text('tools/uploaded.txt'), '', 'gh runs for no upload');
        }),
        bounds.test,
    );

    methods.effect(
        'clean removes each declared output, keeps each file the workspace tracks, and runs the tool clean command',
        Effect.fnUntraced(function* () {
            const clean = 'README.md TASKS "clean removes the outputs a project\'s targets declare and runs its tool\'s clean"';
            const { absent, nx: run, text } = yield* Consumer;
            yield* run('run-many', '--targets=pack,stage', '--output-style=static');
            const { code, stdout, stderr } = yield* run('run', 'lib:clean', '--output-style=static');
            assert.strictEqual(code, 0, `nx run lib:clean exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual(
                yield* absent(['lib/packed/lib.tgz', 'lib/staged/built.txt', 'lib/staged/tracked.txt', 'lib/index.js']),
                ['lib/packed/lib.tgz', 'lib/staged/built.txt'],
                `${clean}: each file a target wrote under its declared outputs goes, and lib/staged/tracked.txt, a tracked source file under a declared output, stays`,
            );
            assert.deepStrictEqual(yield* Effect.flatMap(text('tools/cleaned.json'), Schema.decodeEffect(Arguments)), ['swept'], `${clean}: the tool's own clean command runs`);
        }),
        bounds.test,
    );
});

it.layer(workspace({ projects: ['lib'], releaseTag: { pattern: '{projectName}@{version}' } }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'attach uploads to the tag nx release made at the commit it released, for a fixed group whose tag pattern names the project',
        Effect.fnUntraced(function* () {
            const { nx: run } = yield* Consumer;
            const released = yield* run('release', '1.2.4', '--skip-publish', '--first-release', '--verbose');
            const { tag } = TAGGED.exec(`${released.stdout}${released.stderr}`)?.groups ?? {};
            const { code, printed, passed } = yield* uploaded;
            assert.strictEqual(released.code, 0, `nx release 1.2.4 --skip-publish --first-release exits 0: ${released.stdout}${released.stderr}`);
            assert.include(tag, '1.2.4', `nx release 1.2.4 --verbose prints its git tag command: ${released.stdout}${released.stderr}`);
            assert.strictEqual(code, 0, `nx run lib:attach exits 0: ${printed}`);
            assert.strictEqual(passed[2], tag, `${ATTACH}; nrwl/nx packages/nx/src/command-line/release/utils/shared.ts createGitTagValues: a fixed group gets one tag, interpolated from version and group name alone, at the release commit`);
        }),
        bounds.test,
    );
});
