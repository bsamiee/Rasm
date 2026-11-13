import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import cargo from '@rasm/nx-cargo/package.json' with { type: 'json' };
import uv from '@rasm/nx-uv/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { NX, README, VERBS } from '@rasm/testing/nx/exemplars';
import { affected, foreign, hashed, omitted, pairs, placed, publishing, reprinted, seen, together, tools, unread } from '@rasm/testing/nx/projects';
import { Duration, Effect, Layer, String } from 'effect';
import { CARGO, files, members, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('3 minutes') } as const;
const SHARED = ['Cargo.toml', 'Cargo.lock'];

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cargo project graph over a Cargo workspace', (methods) => {
    methods.effect(
        'lists every package cargo reports as a workspace member, each under the crate name Cargo derives from its package, library and binary-only crates alike',
        Effect.fnUntraced(function* () {
            const [stated, { names }] = yield* Effect.all([members, placed]);
            const crates = stated.map(({ crate }) => crate);
            assert.sameMembers(names, crates, `[${NX.detection}; ${CARGO.crate}]`);
            assert.deepStrictEqual(
                crates,
                stated.map(({ name }) => String.kebabToSnake(name)),
                `[${CARGO.crate}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'roots every project at the folder of its Cargo.toml, a path dependency outside the member globs included',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                folders,
                stated.map(({ root }) => root),
                `[${NX.roots}; ${CARGO.metadata} packages[].manifest_path]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'gives every crate build, test, check, fix, and clean, and install to crates with a binary target alone',
        Effect.fnUntraced(function* () {
            const [stated, { held, lacking }] = yield* Effect.all([members, placed]);
            assert.deepStrictEqual(lacking(['build', 'test', 'check', 'fix', 'clean']), [], `[${README.tasks}; ${NX.detection}]`);
            assert.sameMembers(
                held('install'),
                stated.filter(({ binary }) => binary).map(({ root }) => root),
                `[${README.tasks}; ${NX.detection}; ${CARGO.metadata} packages[].targets[].kind]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'names every target a verb of the task list, <verb>--<tool>, or nx-release-publish',
        Effect.fnUntraced(function* () {
            const { targets } = yield* placed;
            assert.deepStrictEqual(
                targets.filter(({ target, verb }) => target !== 'nx-release-publish' && !VERBS.includes(verb)).map(({ target }) => target),
                [],
                `[${README.tasks}; ${README.aggregates}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'runs each <verb>--<tool> target when its verb runs',
        Effect.fnUntraced(function* () {
            const planned = yield* tools(VERBS);
            assert.deepStrictEqual(
                planned.filter(({ waited }) => !waited).map(({ id }) => id),
                [],
                `[${README.aggregates}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'caches the build, test, and check targets of every crate',
        Effect.fnUntraced(function* () {
            const planned = yield* tools(['build', 'test', 'check']);
            assert.isNotEmpty(planned, `[${README.aggregates}]`);
            assert.deepStrictEqual(
                planned.filter(({ cache }) => !cache).map(({ id }) => id),
                [],
                `[${NX.cached}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'never caches a target that edits sources, removes outputs, installs, or publishes',
        Effect.fnUntraced(function* () {
            const [planned, { cached }] = yield* Effect.all([tools(['fix', 'clean', 'install', 'upgrade']), publishing]);
            assert.deepStrictEqual([...planned.filter(({ cache }) => cache).map(({ id }) => id), ...cached], [], `[${NX.effects}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes the workspace manifest, the lock, the crate manifest, and every target source cargo reports for the build of each crate',
        Effect.fnUntraced(function* () {
            const [stated, builds] = yield* Effect.all([members, hashed]);
            const read = stated.map(({ root, manifest, sources, dependencies }) => ({ root, dependencies, sources: [manifest, ...sources] }));
            assert.deepStrictEqual(omitted(builds, read, SHARED), [], `[${NX.inputs}; ${README.aggregates}; ${CARGO.metadata} packages[].targets[].src_path]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes for a build no source of a crate outside the dependencies cargo reports for it',
        Effect.fnUntraced(function* () {
            const [stated, builds] = yield* Effect.all([members, hashed]);
            assert.deepStrictEqual(foreign(builds, stated), [], `[${NX.narrow}; ${CARGO.metadata} packages[].dependencies]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes no tool version command and no .config/mise.lock for a cached task',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(yield* unread, [], `[${README.read}]`);
        }),
        bounds.test,
    );

    methods.effect.each(SHARED)(
        'selects every crate as affected by a change to %s',
        Effect.fnUntraced(function* (file) {
            const [stated, selected] = yield* Effect.all([members, affected(file)]);
            assert.sameMembers(
                selected,
                stated.map(({ root }) => root),
                `[${NX.affected}; ${README.aggregates}; ${CARGO.metadata}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'draws one static edge per path dependency cargo reports, renamed and dev dependencies included, and none for a transitive one',
        Effect.fnUntraced(function* () {
            const [stated, drawn] = yield* Effect.all([members, placed]);
            assert.sameMembers(drawn.pairs, pairs(stated), `[${NX.edges}; ${CARGO.metadata} packages[].dependencies[].path]`);
            assert.deepStrictEqual(drawn.loose, [], `[${NX.owned}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'prints the same graph on a second cold read, with no absolute path in it',
        Effect.fnUntraced(function* () {
            const { root, first, second } = yield* reprinted;
            assert.strictEqual(first.code, 0, first.stderr);
            assert.strictEqual(second.graph, first.graph, `[${NX.stable}]`);
            assert.notInclude(first.graph, root, `[${NX.stable}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives a second checkout at another path the same graph and the same build inputs',
        Effect.fnUntraced(function* () {
            const [here, there] = yield* Effect.all([seen, Effect.provide(seen, workspace())]);
            assert.deepStrictEqual(there, here, `[${NX.stable}; ${README.read}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'writes no file into the workspace while reading the graph, a missing lock included',
        Effect.fnUntraced(function* () {
            const { nx, called, written } = yield* Consumer;
            yield* called('git', ['rm', '--quiet', '--cached', 'Cargo.lock']);
            yield* called('rm', ['Cargo.lock']);
            yield* nx('reset');
            const [shown, created] = yield* written(nx('show', 'projects'));
            yield* called('git', ['reset', '--quiet', '--hard']);
            assert.strictEqual(Number(shown.code), 0, shown.stderr);
            assert.deepStrictEqual(created, [], `[${NX.stable}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'answers concurrent cold graph reads alike',
        Effect.fnUntraced(function* () {
            const { failed, answers } = yield* together;
            assert.deepStrictEqual(failed, [], `[${NX.stable}]`);
            assert.lengthOf(answers, 1, `[${NX.stable}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'keeps every node as it was when a file no crate reads appears',
        Effect.fnUntraced(function* () {
            const { nx, wrote } = yield* Consumer;
            const first = yield* nx('graph', '--print');
            yield* wrote({ 'docs/notes.md': 'No crate reads this file\n' });
            const second = yield* nx('graph', '--print');
            assert.strictEqual(second.stdout, first.stdout, `[${NX.stable}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives a warm graph the binary target a new source file adds to a library crate',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* placed;
            yield* wrote({ 'crates/middle/src/bin/report.rs': 'fn main() {\n    println!("{}", rasm_nx_fixture_middle::two());\n}\n' });
            const { held } = yield* placed;
            const stated = yield* members;
            assert.sameMembers(
                held('install'),
                stated.filter(({ binary }) => binary).map(({ root }) => root),
                `[${NX.reread}; ${CARGO.metadata} packages[].targets[].kind]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'gives a warm graph the member and the edge a manifest edit adds',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* placed;
            yield* wrote({
                'crates/extra/Cargo.toml': '[package]\nname = "rasm-nx-fixture-extra"\nversion = "0.1.0"\nedition.workspace = true\n',
                'crates/extra/src/lib.rs': 'pub fn zero() -> u32 {\n    0\n}\n',
                'tools/generator/Cargo.toml': `${files['tools/generator/Cargo.toml']}rasm-nx-fixture-extra = { path = "../../crates/extra" }\n`,
            });
            const drawn = yield* placed;
            const stated = yield* members;
            assert.sameMembers(
                drawn.folders,
                stated.map(({ root }) => root),
                `[${NX.reread}; ${CARGO.metadata}]`,
            );
            assert.sameMembers(drawn.pairs, pairs(stated), `[${NX.reread}; ${CARGO.metadata}]`);
        }),
        bounds.test,
    );
});

it.layer(workspace({ 'broken/Cargo.toml': '[package\nname = ' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cargo project graph beside a manifest cargo refuses', (methods) => {
    methods.effect(
        'fails the graph with an aggregate error naming the manifest',
        Effect.fnUntraced(function* () {
            const [{ nx }, { refused }] = yield* Effect.all([Consumer, placed]);
            const shown = yield* nx('show', 'projects');
            assert.notStrictEqual(Number(shown.code), 0, `[${NX.partial}]`);
            assert.include(`${shown.stdout}${shown.stderr}`, 'broken/Cargo.toml', `[${NX.partial}]`);
            assert.deepStrictEqual(refused, ['broken/Cargo.toml'], `[${NX.partial}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'keeps every member of the sound workspace in the partial graph',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                folders,
                stated.map(({ root }) => root),
                `[${NX.partial}; ${CARGO.metadata}]`,
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ 'crates/middle/Cargo.toml': '[package\nname = ' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cargo project graph with a member manifest cargo refuses', (methods) => {
    methods.effect(
        'names the member manifest cargo names in its refusal',
        Effect.fnUntraced(function* () {
            const { refusals } = yield* placed;
            assert.include(refusals, 'crates/middle/Cargo.toml', `[${NX.partial}; cargo metadata stderr]`);
        }),
        bounds.test,
    );
});

it.layer(
    Layer.provideMerge(
        Consumer.layer({
            resolved: import.meta.url,
            settings: { plugins: [cargo.name, uv.name] },
            ignored: ['target', '.venv'],
            files: {
                'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
                'crates/shared/Cargo.toml': '[package]\nname = "rasm-nx-fixture-shared"\nversion = "0.1.0"\nedition = "2024"\n',
                'crates/shared/src/lib.rs': '',
                'pyproject.toml': '[tool.uv.workspace]\nmembers = ["packages/*"]\n',
                'packages/shared/pyproject.toml': '[project]\nname = "rasm-nx-fixture-shared"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n',
            },
        }),
        Layer.provideMerge(Packed.layer(cargo.name, uv.name), layer),
    ),
    { timeout: bounds.built, excludeTestServices: true },
)('@rasm/nx-cargo project graph beside a uv member of the same package name', (methods) => {
    methods.effect(
        'builds a graph holding the crate and the uv member as two projects',
        Effect.fnUntraced(function* () {
            const [{ called }, { folders, refused }] = yield* Effect.all([Consumer, placed]);
            const [crates, python] = yield* Effect.all([called('cargo', ['metadata', '--format-version', '1', '--no-deps']), called('uv', ['workspace', 'list'])]);
            assert.include(crates.stdout, '"name":"rasm-nx-fixture-shared"', `[${CARGO.metadata}]`);
            assert.include(python.stdout, 'rasm-nx-fixture-shared', '[uv workspace list]');
            assert.deepStrictEqual(refused, [], `[${NX.duplicate}]`);
            assert.sameMembers(folders, ['crates/shared', 'packages/shared'], `[${NX.roots}; ${CARGO.metadata}; uv workspace list]`);
        }),
        bounds.test,
    );
});
