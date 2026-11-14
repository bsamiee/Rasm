import { assert, it } from '@effect/vitest';
import { Consumer } from '@rasm/testing/nx';
import { NX, README, VERBS } from '@rasm/testing/nx/exemplars';
import { affected, foreign, hashed, omitted, pairs, placed, publishing, reprinted, seen, spied, together, tools, unread } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect } from 'effect';
import { APPLE, files, members, misplaced, modules, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-apple project graph over a Swift package and an Xcode project', (methods) => {
    methods.effect(
        'lists every module SwiftPM describes and every project xcodebuild lists under the name the tool gives it, and no project the tools do not name',
        Effect.fnUntraced(function* () {
            const [stated, { names }] = yield* Effect.all([members, placed]);
            assert.includeMembers(
                names,
                stated.filter(({ tests }) => !tests).map(({ name }) => name),
                `[${NX.detection}; ${README.modules}; ${APPLE.described} targets[].name; ${APPLE.listed} project.name]`,
            );
            assert.deepStrictEqual(
                Array.difference(
                    names,
                    stated.map(({ name }) => name),
                ),
                [],
                `[${NX.detection}; ${APPLE.described} targets[].name; ${APPLE.listed} project.name]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'roots every module at the folder SwiftPM describes for its target, and the Xcode project at the folder holding its .xcodeproj',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.deepStrictEqual(misplaced(stated, folders), [], `[${NX.roots}; ${README.modules}; ${APPLE.described} targets[].path]`);
        }),
        bounds.test,
    );

    methods.effect(
        'gives every project build and clean, test only to a test target or the module it tests, and install to the modules SwiftPM types executable',
        Effect.fnUntraced(function* () {
            const [swift, { held, lacking }] = yield* Effect.all([modules, placed]);
            const suites = swift.filter(({ tests }) => tests);
            assert.deepStrictEqual(lacking(['build', 'clean']), [], `[${README.tasks}; ${NX.detection}]`);
            assert.isNotEmpty(held('test'), `[${README.tasks}; ${APPLE.described} targets[].type]`);
            assert.deepStrictEqual(
                Array.difference(
                    held('test'),
                    suites.flatMap(({ root, dependencies }) => [root, ...dependencies]),
                ),
                [],
                `[${README.tasks}; ${NX.detection}; ${APPLE.described} targets[].type, targets[].target_dependencies]`,
            );
            assert.sameMembers(
                Array.intersection(
                    held('install'),
                    swift.map(({ root }) => root),
                ),
                swift.filter(({ executable }) => executable).map(({ root }) => root),
                `[${README.tasks}; ${NX.detection}; ${APPLE.described} targets[].type]`,
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
        'never caches a target that edits sources, removes outputs, installs, upgrades, or publishes',
        Effect.fnUntraced(function* () {
            const [planned, { cached }] = yield* Effect.all([tools(['fix', 'clean', 'install', 'upgrade', 'attach']), publishing]);
            assert.deepStrictEqual([...planned.filter(({ cache }) => cache).map(({ id }) => id), ...cached], [], `[${NX.effects}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes the manifest and every source the tool lists for the build of each project',
        Effect.fnUntraced(function* () {
            const [stated, builds, { folders }] = yield* Effect.all([members, hashed, placed]);
            const read = stated.filter(({ root }) => folders.includes(root)).map(({ root, manifest, sources, dependencies }) => ({ root, dependencies, sources: [manifest, ...sources] }));
            assert.isNotEmpty(read, `[${APPLE.described}]`);
            assert.deepStrictEqual(omitted(builds, read, []), [], `[${NX.inputs}; ${README.aggregates}; ${APPLE.described} targets[].sources]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hashes for a build no source of a module outside the target dependencies SwiftPM describes for it',
        Effect.fnUntraced(function* () {
            const [stated, builds] = yield* Effect.all([members, hashed]);
            assert.deepStrictEqual(foreign(builds, stated), [], `[${NX.narrow}; ${APPLE.described} targets[].target_dependencies]`);
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

    methods.effect(
        'selects every module as affected by a change to Package.swift',
        Effect.fnUntraced(function* () {
            const [swift, selected] = yield* Effect.all([modules, affected('Package.swift')]);
            assert.includeMembers(
                selected,
                swift.filter(({ tests }) => !tests).map(({ root }) => root),
                `[${NX.affected}; ${README.aggregates}; ${APPLE.described}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'draws one static edge per target dependency SwiftPM describes between projects, and none for a transitive one',
        Effect.fnUntraced(function* () {
            const [stated, drawn] = yield* Effect.all([members, placed]);
            assert.isNotEmpty(drawn.pairs, `[${NX.edges}]`);
            assert.sameMembers(drawn.pairs, pairs(stated.filter(({ root }) => drawn.folders.includes(root))), `[${NX.edges}; ${APPLE.described} targets[].target_dependencies]`);
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
        'runs swift and xcodebuild for a cold graph read and neither for a warm read of unchanged files',
        Effect.fnUntraced(function* () {
            const { cold, warm } = yield* spied('swift', 'xcodebuild');
            assert.strictEqual(cold.code, 0, `[${NX.reused}]`);
            assert.isAbove(cold.runs, 0, `[${NX.reused}]`);
            assert.strictEqual(warm.runs, 0, `[${NX.reused}]`);
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
        'gives a warm graph the module and the edge a Package.swift edit adds',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* placed;
            yield* wrote({
                'Package.swift': files['Package.swift'].replace('        .target(name: "Core", path: "libs/Core"),', '        .target(name: "Core", dependencies: ["Extra"], path: "libs/Core"),\n        .target(name: "Extra", path: "libs/Extra"),'),
                'libs/Extra/Extra.swift': 'public func zero() -> Int { 0 }\n',
            });
            const drawn = yield* placed;
            const stated = yield* members;
            assert.deepStrictEqual(misplaced(stated, drawn.folders), [], `[${NX.reread}; ${APPLE.described}]`);
            assert.include(drawn.pairs, 'libs/Core -> libs/Extra', `[${NX.reread}; ${APPLE.described} targets[].target_dependencies]`);
            assert.sameMembers(drawn.pairs, pairs(stated.filter(({ root }) => drawn.folders.includes(root))), `[${NX.reread}; ${APPLE.described} targets[].target_dependencies]`);
        }),
        bounds.test,
    );
});

it.layer(
    workspace({
        'Package.swift': files['Package.swift']
            .replace('    targets: [', '    dependencies: [.package(url: "https://github.com/apple/swift-argument-parser", from: "1.5.0")],\n    targets: [')
            .replace('dependencies: ["Middle"], path: "apps/Tool"', 'dependencies: ["Middle", .product(name: "ArgumentParser", package: "swift-argument-parser")], path: "apps/Tool"'),
    }),
    { timeout: bounds.built, excludeTestServices: true },
)('@rasm/nx-apple project graph with a remote package dependency and no resolved file', (methods) => {
    methods.effect(
        'reads the graph without resolving or fetching a package, and writes no file outside the scratch folder SwiftPM keeps',
        Effect.fnUntraced(function* () {
            const { nx, written } = yield* Consumer;
            const [shown, created] = yield* written(nx('show', 'projects'));
            assert.strictEqual(Number(shown.code), 0, `[${NX.stable}]: ${shown.stderr}`);
            assert.deepStrictEqual(
                created.filter((file) => !file.startsWith('.build/') || file.startsWith('.build/checkouts/') || file.startsWith('.build/repositories/') || file === 'Package.resolved'),
                [],
                `[${NX.stable}; ${APPLE.described} writes .build/CACHEDIR.TAG and a build system marker alone]`,
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ 'broken/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: \n' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-apple project graph beside a manifest SwiftPM refuses', (methods) => {
    methods.effect(
        'fails the graph with an aggregate error naming the manifest',
        Effect.fnUntraced(function* () {
            const [{ nx }, { refused }] = yield* Effect.all([Consumer, placed]);
            const shown = yield* nx('show', 'projects');
            assert.notStrictEqual(Number(shown.code), 0, `[${NX.partial}]`);
            assert.include(`${shown.stdout}${shown.stderr}`, 'broken/Package.swift', `[${NX.partial}]`);
            assert.deepStrictEqual(refused, ['broken/Package.swift'], `[${NX.partial}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'keeps every module of the sound package and the Xcode project in the partial graph',
        Effect.fnUntraced(function* () {
            const [stated, { folders }] = yield* Effect.all([members, placed]);
            assert.deepStrictEqual(misplaced(stated, folders), [], `[${NX.partial}; ${APPLE.described}; ${APPLE.listed}]`);
        }),
        bounds.test,
    );
});
