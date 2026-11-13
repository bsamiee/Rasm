import { assert, it } from '@effect/vitest';
import { Consumer } from '@rasm/testing/nx/consumer';
import { NX, README, VERBS } from '@rasm/testing/nx/exemplars';
import { affected, foreign, hashed, omitted, pairs, placed, publishing, reprinted, seen, together, tools, unread } from '@rasm/testing/nx/projects';
import { Duration, Effect } from 'effect';
import { files, MSBUILD, members, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('3 minutes') } as const;
const SHARED = ['Directory.Build.props', 'Directory.Packages.props'];
const PROPS = 'Directory.Build.props';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-dotnet project graph over MSBuild projects', (methods) => {
    methods.effect(
        'lists every project file MSBuild evaluates, under the project name MSBuild gives it',
        Effect.fnUntraced(function* () {
            const [stated, { names }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                names,
                stated.map(({ name }) => name),
                `[${NX.detection}; ${MSBUILD.name}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'roots every project at the folder of its project file',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                folders,
                stated.map(({ root }) => root),
                `[${NX.roots}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'gives every project build and clean, test to the projects MSBuild marks as test applications, and pack to the projects MSBuild marks packable',
        Effect.fnUntraced(function* () {
            const [stated, { held, lacking }] = yield* Effect.all([members, placed]);
            assert.deepStrictEqual(lacking(['build', 'clean']), [], `[${README.tasks}; ${NX.detection}]`);
            assert.sameMembers(
                held('test'),
                stated.filter(({ tested }) => tested).map(({ root }) => root),
                `[${README.tasks}; ${NX.detection}; ${MSBUILD.kinds}]`,
            );
            assert.sameMembers(
                held('pack'),
                stated.filter(({ packable }) => packable).map(({ root }) => root),
                `[${README.tasks}; ${NX.detection}; ${MSBUILD.kinds}]`,
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
        'caches the build, test, and check targets of every project',
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
            const [planned, { cached }] = yield* Effect.all([tools(['fix', 'clean', 'install']), publishing]);
            assert.deepStrictEqual([...planned.filter(({ cache }) => cache).map(({ id }) => id), ...cached], [], `[${NX.effects}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes the Directory.Build.props, the Directory.Packages.props, the project file, and every Compile item MSBuild evaluates for the build of each project',
        Effect.fnUntraced(function* () {
            const [stated, builds] = yield* Effect.all([members, hashed]);
            assert.deepStrictEqual(omitted(builds, stated, SHARED), [], `[${NX.inputs}; ${README.aggregates}; ${MSBUILD.compiled}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes for a build no source of a project outside the references MSBuild evaluates for it, and no file under bin or obj',
        Effect.fnUntraced(function* () {
            const [stated, builds] = yield* Effect.all([members, hashed]);
            const built = builds.flatMap(({ read }) => read).filter((file) => file.includes('/bin/') || file.includes('/obj/'));
            assert.deepStrictEqual([...foreign(builds, stated), ...built], [], `[${NX.narrow}; ${MSBUILD.references}]`);
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
        'selects every project as affected by a change to %s',
        Effect.fnUntraced(function* (file) {
            const [stated, selected] = yield* Effect.all([members, affected(file)]);
            assert.sameMembers(
                selected,
                stated.map(({ root }) => root),
                `[${NX.affected}; ${NX.inputs}; ${README.aggregates}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'draws one static edge per ProjectReference MSBuild evaluates, and none for a transitive one',
        Effect.fnUntraced(function* () {
            const [stated, drawn] = yield* Effect.all([members, placed]);
            assert.sameMembers(drawn.pairs, pairs(stated), `[${NX.edges}; ${MSBUILD.references}]`);
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
        'writes no file into the workspace while reading the graph, restore outputs absent',
        Effect.fnUntraced(function* () {
            const { nx, called, cleaned, written } = yield* Consumer;
            yield* cleaned;
            yield* nx('reset');
            const [shown, created] = yield* written(nx('show', 'projects'));
            yield* called('dotnet', ['restore', 'tests/Core.Tests/Fixture.Core.Tests.csproj']);
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
        'keeps every node as it was when a file no project reads appears',
        Effect.fnUntraced(function* () {
            const { nx, wrote } = yield* Consumer;
            const first = yield* nx('graph', '--print');
            yield* wrote({ 'docs/notes.md': 'No project reads this file\n' });
            const second = yield* nx('graph', '--print');
            assert.strictEqual(second.stdout, first.stdout, `[${NX.stable}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives a warm graph the project and the edge a project file edit adds',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* placed;
            yield* wrote({
                'src/Extra/Fixture.Extra.csproj': files['src/Core/Fixture.Core.csproj'],
                'src/Extra/Zero.cs': 'namespace Fixture.Extra;\n\npublic static class Zero\n{\n    public static int Value() => 0;\n}\n',
                'src/App/Fixture.App.csproj': files['src/App/Fixture.App.csproj'].replace('    </ItemGroup>', '        <ProjectReference Include="../Extra/Fixture.Extra.csproj" />\n    </ItemGroup>'),
            });
            const drawn = yield* placed;
            const stated = yield* members;
            assert.sameMembers(
                drawn.folders,
                stated.map(({ root }) => root),
                `[${NX.reread}; ${MSBUILD.evaluated}]`,
            );
            assert.sameMembers(drawn.pairs, pairs(stated), `[${NX.reread}; ${MSBUILD.references}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives a warm graph the targets a shared props edit changes for every project',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* placed;
            yield* wrote({ [PROPS]: files[PROPS].replace('<Nullable>enable</Nullable>', '<Nullable>enable</Nullable>\n        <IsPackable>false</IsPackable>') });
            const { held } = yield* placed;
            const stated = yield* members;
            yield* wrote({ [PROPS]: files[PROPS] });
            assert.sameMembers(
                held('pack'),
                stated.filter(({ packable }) => packable).map(({ root }) => root),
                `[${NX.reread}; ${MSBUILD.kinds}]`,
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ 'src/Broken/Fixture.Broken.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-dotnet project graph beside a project file MSBuild refuses', (methods) => {
    methods.effect(
        'fails the graph with an aggregate error naming the project file',
        Effect.fnUntraced(function* () {
            const [{ nx }, { refused }] = yield* Effect.all([Consumer, placed]);
            const shown = yield* nx('show', 'projects');
            assert.notStrictEqual(Number(shown.code), 0, `[${NX.partial}]`);
            assert.include(`${shown.stdout}${shown.stderr}`, 'src/Broken/Fixture.Broken.csproj', `[${NX.partial}]`);
            assert.deepStrictEqual(refused, ['src/Broken/Fixture.Broken.csproj'], `[${NX.partial}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'keeps every project MSBuild still evaluates in the partial graph',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.sameMembers(
                folders,
                stated.map(({ root }) => root),
                `[${NX.partial}; ${MSBUILD.evaluated}]`,
            );
        }),
        bounds.test,
    );
});
