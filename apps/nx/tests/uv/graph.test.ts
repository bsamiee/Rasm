import { assert, it } from '@effect/vitest';
import { Consumer } from '@rasm/testing/nx/consumer';
import { NX, README, VERBS } from '@rasm/testing/nx/exemplars';
import { affected, foreign, hashed, omitted, pairs, placed, publishing, reprinted, seen, together, tools, unread } from '@rasm/testing/nx/projects';
import { Duration, Effect } from 'effect';
import { built, files, members, UV, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('3 minutes') } as const;
const SHARED = ['pyproject.toml', 'uv.lock'];

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-uv project graph over a uv workspace', (methods) => {
    methods.effect(
        'lists every member uv reports for the workspace, under the name uv gives it',
        Effect.fnUntraced(function* () {
            const [stated, { names }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                names,
                stated.map(({ name }) => name),
                `[${NX.detection}; ${UV.metadata} members[].name]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'roots every project at the folder uv reports for the member',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                folders,
                stated.map(({ root }) => root),
                `[${NX.roots}; ${UV.metadata} members[].path]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'gives build to the members uv builds a distribution for, and test to the members whose resolution holds pytest',
        Effect.fnUntraced(function* () {
            const [stated, packaged, { held }] = yield* Effect.all([members, built, placed]);
            assert.sameMembers(
                held('build'),
                packaged.map(({ root }) => root),
                `[${README.tasks}; ${NX.detection}; ${UV.build}]`,
            );
            assert.sameMembers(
                held('test'),
                stated.filter(({ tested }) => tested).map(({ root }) => root),
                `[${README.tasks}; ${NX.detection}; ${UV.metadata} resolution]`,
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
        'caches the build, test, and check targets of every member',
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
        'never caches a target that edits sources, removes outputs, installs, upgrades, or publishes',
        Effect.fnUntraced(function* () {
            const [planned, { cached }] = yield* Effect.all([tools(['fix', 'clean', 'install', 'upgrade']), publishing]);
            assert.deepStrictEqual([...planned.filter(({ cache }) => cache).map(({ id }) => id), ...cached], [], `[${NX.effects}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes the workspace manifest, the lock, and every file uv packs into the source distribution for the build of each member',
        Effect.fnUntraced(function* () {
            const [packaged, builds] = yield* Effect.all([built, hashed]);
            assert.deepStrictEqual(omitted(builds, packaged, SHARED), [], `[${NX.inputs}; ${README.aggregates}; ${UV.sdist}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes for a build no source of a member outside the dependencies uv resolves for it',
        Effect.fnUntraced(function* () {
            const [packaged, builds] = yield* Effect.all([built, hashed]);
            assert.deepStrictEqual(foreign(builds, packaged), [], `[${NX.narrow}; ${UV.metadata} resolution[].dependencies]`);
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
        'selects every member with a build as affected by a change to %s',
        Effect.fnUntraced(function* (file) {
            const [packaged, selected] = yield* Effect.all([built, affected(file)]);
            assert.sameMembers(
                selected,
                packaged.map(({ root }) => root),
                `[${NX.affected}; ${README.aggregates}; ${UV.metadata}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'draws one static edge per member dependency uv resolves, a dependency group included, and none for a transitive one',
        Effect.fnUntraced(function* () {
            const [stated, drawn] = yield* Effect.all([members, placed]);
            assert.sameMembers(drawn.pairs, pairs(stated.map(({ root, dependencies }) => ({ root, dependencies, sources: [] }))), `[${NX.edges}; ${UV.metadata} resolution[].dependencies]`);
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
        'writes no file into the workspace while reading the graph, a missing lock and a missing environment included',
        Effect.fnUntraced(function* () {
            const { nx, called, written } = yield* Consumer;
            yield* called('git', ['rm', '--quiet', '--cached', 'uv.lock']);
            yield* called('rm', ['-rf', 'uv.lock', '.venv']);
            yield* nx('reset');
            const [shown, created] = yield* written(nx('show', 'projects'));
            yield* called('git', ['reset', '--quiet', '--hard']);
            assert.deepStrictEqual(created, [], `[${NX.stable}]: exit ${shown.code} ${shown.stderr}`);
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
        'keeps every node as it was when a file no member reads appears',
        Effect.fnUntraced(function* () {
            const { nx, wrote } = yield* Consumer;
            const first = yield* nx('graph', '--print');
            yield* wrote({ 'docs/notes.md': 'No member reads this file\n' });
            const second = yield* nx('graph', '--print');
            assert.strictEqual(second.stdout, first.stdout, `[${NX.stable}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives a warm graph the member and the edge a manifest edit adds',
        Effect.fnUntraced(function* () {
            const { wrote, called } = yield* Consumer;
            yield* placed;
            yield* wrote({
                'packages/extra/pyproject.toml': files['packages/core/pyproject.toml'].replace('rasm-nx-fixture-core', 'rasm-nx-fixture-extra'),
                'packages/extra/src/rasm_nx_fixture_extra/__init__.py': 'ZERO = 0\n',
                'tools/report/pyproject.toml': files['tools/report/pyproject.toml'].replace('dev = ["rasm-nx-fixture-core"]', 'dev = ["rasm-nx-fixture-core", "rasm-nx-fixture-extra"]').concat('rasm-nx-fixture-extra = { workspace = true }\n'),
            });
            yield* called('uv', ['lock']);
            const drawn = yield* placed;
            const stated = yield* members;
            assert.sameMembers(
                drawn.folders,
                stated.map(({ root }) => root),
                `[${NX.reread}; ${UV.metadata}]`,
            );
            assert.sameMembers(drawn.pairs, pairs(stated.map(({ root, dependencies }) => ({ root, dependencies, sources: [] }))), `[${NX.reread}; ${UV.metadata}]`);
        }),
        bounds.test,
    );
});

it.layer(workspace({ 'broken/pyproject.toml': '[project\nname = ' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-uv project graph beside a manifest uv refuses', (methods) => {
    methods.effect(
        'fails the graph with an aggregate error naming the manifest',
        Effect.fnUntraced(function* () {
            const [{ nx }, { refused }] = yield* Effect.all([Consumer, placed]);
            const shown = yield* nx('show', 'projects');
            assert.notStrictEqual(Number(shown.code), 0, `[${NX.partial}]`);
            assert.include(`${shown.stdout}${shown.stderr}`, 'broken/pyproject.toml', `[${NX.partial}]`);
            assert.deepStrictEqual(refused, ['broken/pyproject.toml'], `[${NX.partial}]`);
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
                `[${NX.partial}; ${UV.metadata}]`,
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ 'packages/middle/pyproject.toml': '[project\nname = ' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-uv project graph with a member manifest uv refuses', (methods) => {
    methods.effect(
        'names the member manifest in its refusal',
        Effect.fnUntraced(function* () {
            const { refusals } = yield* placed;
            assert.include(refusals, 'packages/middle/pyproject.toml', `[${NX.partial}; uv stderr]`);
        }),
        bounds.test,
    );
});
