import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Layer, Record } from 'effect';
import { configured, files, members, placed, plugin, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CODEMODEL = 'CMake file API codemodel-v2 reply';
const READ = 'CMake file API cmakeFiles-v1 reply inputs[]';
const LISTED = 'ctest --show-only=json-v1';
const CONFIGURES = 'README.md PRINCIPLES "`@rasm/nx-cmake` configures the build folder each configure preset states"';
const TASKS = 'README.md TASKS';
const AGGREGATES = 'README.md PRINCIPLES "Verbs aggregate `<verb>--<tool>` targets and read their inputs"';
const DETECTION = 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:31-74';
const ROOTS = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts:49-52';
const EDGES = 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:201-216';
const INPUTS = 'nrwl/nx e2e/dotnet/src/dotnet-advanced-msbuild.test.ts:426-462';
const NARROW = 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts:186-195';
const CACHED = 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc:60-71, e2e/dotnet/src/dotnet-cache.test.ts:62-76';
const EFFECTS = 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc:73-77';
const STABLE = 'nrwl/nx astro-docs/src/content/docs/kb/performant-project-graph-plugins.mdoc:189-191';
const REREAD = 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts:254-270';
const PARTIAL = 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts:132-185';
const AFFECTED = 'nrwl/nx astro-docs/src/content/docs/features/CI Features/affected.mdoc';
const VERBS = ['codegen', 'build', 'pack', 'attach', 'check', 'fix', 'test', 'serve', 'preview', 'install', 'deploy', 'upgrade', 'clean'];
const bounds = { built: Duration.minutes(5), test: Duration.toMillis(Duration.minutes(3)) };

// --- [OPERATIONS] ----------------------------------------------------------------------

const planned = Effect.fnUntraced(function* (kinds: readonly string[]) {
    const [consumer, { targets }] = yield* Effect.all([Consumer, placed]);
    const tools = targets.filter(({ target, verb }) => kinds.includes(verb) && target !== verb);
    const { tasks, dependencies } = yield* consumer.planned('run-many', `--targets=${Array.dedupe(tools.flatMap(({ target, verb }) => [target, verb])).join(',')}`);
    return { tools: tools.map((tool) => ({ ...tool, id: `${tool.project}:${tool.target}`, cache: tasks[`${tool.project}:${tool.target}`]?.cache ?? false })), dependencies };
});

const hashed = Effect.gen(function* () {
    const [{ inputs }, { projects, targets }] = yield* Effect.all([Consumer, placed]);
    const builds = projects.filter(({ name }) => targets.some(({ project, target }) => project === name && target === 'build'));
    return yield* Effect.forEach(builds, ({ name, data }) => Effect.map(inputs(`${name}:build`), ({ files: read }) => ({ root: data.root, read })));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.provideMerge(Packed.layer(plugin.name), layer), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cmake project graph', (suite) => {
    suite.layer(workspace(), { timeout: bounds.built })('over a CMake tree of nested projects', (methods) => {
        methods.effect(
            'lists every project the file API reports below the root, under the name its project() call gives',
            Effect.fnUntraced(function* () {
                const [stated, { projects }] = yield* Effect.all([members, placed]);
                assert.sameMembers(
                    projects.map(({ name }) => name),
                    stated.map(({ name }) => name),
                    `[${DETECTION}; ${CODEMODEL} projects[].name]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'roots every project at the source folder the file API reports for it',
            Effect.fnUntraced(function* () {
                const [stated, { roots }] = yield* Effect.all([members, placed]);
                assert.sameMembers(
                    Object.values(roots),
                    stated.map(({ root }) => root),
                    `[${ROOTS}; ${CODEMODEL} directories[].source]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'gives every project build and clean, and test to the projects CTest lists a test for',
            Effect.fnUntraced(function* () {
                const [stated, { targets }] = yield* Effect.all([members, placed]);
                const held = (root: string, verb: string) => targets.some((target) => target.root === root && target.target === verb);
                const wrong = stated.flatMap(({ root, tested }) => [...['build', 'clean'].filter((verb) => !held(root, verb)).map((verb) => `${root} lacks ${verb}`), ...(held(root, 'test') === tested ? [] : [`${root} test is ${String(!tested)} where CTest lists a test is ${String(tested)}`])]);
                assert.deepStrictEqual(wrong, [], `[${TASKS}; ${DETECTION}; ${LISTED}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'names every target a verb of the task list, <verb>--<tool>, or nx-release-publish',
            Effect.fnUntraced(function* () {
                const { targets } = yield* placed;
                assert.deepStrictEqual(Array.dedupe(targets.filter(({ target, verb }) => target !== 'nx-release-publish' && !VERBS.includes(verb)).map(({ target }) => target)), [], `[${TASKS}; ${AGGREGATES}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'runs each <verb>--<tool> target when its verb runs',
            Effect.fnUntraced(function* () {
                const { tools, dependencies } = yield* planned(VERBS);
                assert.deepStrictEqual(
                    tools.filter(({ project, verb, id }) => !(dependencies[`${project}:${verb}`] ?? []).some((waited) => waited.id === id)).map(({ id }) => id),
                    [],
                    `[${AGGREGATES}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'caches the build, test, and check targets of every project',
            Effect.fnUntraced(function* () {
                const { tools } = yield* planned(['build', 'test', 'check']);
                assert.isNotEmpty(tools, `[${AGGREGATES}]`);
                assert.deepStrictEqual(
                    tools.filter(({ cache }) => !cache).map(({ id }) => id),
                    [],
                    `[${CACHED}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'never caches a target that edits sources, removes outputs, installs, or publishes',
            Effect.fnUntraced(function* () {
                const { tools } = yield* planned(['fix', 'clean', 'install', 'attach']);
                assert.deepStrictEqual(
                    tools.filter(({ cache }) => cache).map(({ id }) => id),
                    [],
                    `[${EFFECTS}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'hashes the presets, every file CMake read to configure the tree, and every source the file API lists for the build of each project',
            Effect.fnUntraced(function* () {
                const [{ members: stated, read: configuredFrom }, builds] = yield* Effect.all([configured, hashed]);
                const lost = stated.flatMap(({ root, sources }) => {
                    const read = builds.find((build) => build.root === root)?.read ?? [];
                    return ['CMakePresets.json', ...configuredFrom.filter((file) => file === 'CMakeLists.txt' || file.startsWith('cmake/') || file.startsWith(`${root}/`)), ...sources].filter((file) => !read.includes(file)).map((file) => `${root} build omits ${file}`);
                });
                assert.deepStrictEqual(lost, [], `[${INPUTS}; ${AGGREGATES}; ${READ}; ${CODEMODEL} targets[].sources]`);
            }),
            bounds.test,
        );

        methods.effect(
            'hashes for a build no source of a project outside the target dependencies the file API reports for it',
            Effect.fnUntraced(function* () {
                const [stated, builds] = yield* Effect.all([members, hashed]);
                const near = (root: string, held: ReadonlySet<string> = new Set()): ReadonlySet<string> => (held.has(root) ? held : (stated.find((member) => member.root === root)?.dependencies ?? []).reduce<ReadonlySet<string>>((seen, dependency) => near(dependency, seen), held.union(new Set([root]))));
                const foreign = builds.flatMap(({ root, read }) => stated.filter((member) => !near(root).has(member.root)).flatMap(({ sources }) => sources.filter((file) => read.includes(file)).map((file) => `${root} build reads ${file}`)));
                assert.deepStrictEqual(foreign, [], `[${NARROW}; ${CODEMODEL} targets[].dependencies]`);
            }),
            bounds.test,
        );

        methods.effect.each(['CMakePresets.json', 'cmake/toolchain.cmake', 'CMakeLists.txt'])(
            'selects every project as affected by a change to %s',
            Effect.fnUntraced(function* (file) {
                const [{ nx }, stated, { roots }] = yield* Effect.all([Consumer, members, placed]);
                const { stdout } = yield* nx('show', 'projects', '--affected', `--files=${file}`, '--withTarget=build', '--json');
                assert.sameMembers(
                    Record.keys(roots)
                        .filter((name) => stdout.includes(`"${name}"`))
                        .map((name) => roots[name]),
                    stated.map(({ root }) => root),
                    `[${AFFECTED}; ${AGGREGATES}; ${READ}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'draws one edge per pair of projects with a target dependency the file API reports',
            Effect.fnUntraced(function* () {
                const [stated, { roots, edges }] = yield* Effect.all([members, placed]);
                assert.sameMembers(
                    Array.dedupe(edges.map(({ source, target }) => `${roots[source]} -> ${roots[target]}`)),
                    stated.flatMap(({ root, dependencies }) => dependencies.map((dependency) => `${root} -> ${dependency}`)),
                    `[${EDGES}; ${CODEMODEL} targets[].dependencies]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'prints the same graph on a second cold read, with no absolute path in it',
            Effect.fnUntraced(function* () {
                const { root, nx } = yield* Consumer;
                const first = yield* nx('graph', '--print');
                yield* nx('reset');
                const second = yield* nx('graph', '--print');
                assert.strictEqual(Number(first.code), 0, first.stderr);
                assert.strictEqual(second.stdout, first.stdout, `[${STABLE}]`);
                assert.notInclude(first.stdout, root, `[${STABLE}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'gives a second checkout at another path the same graph and the same build inputs',
            Effect.fnUntraced(function* () {
                const seen = Effect.gen(function* () {
                    const { nx } = yield* Consumer;
                    const printed = yield* nx('graph', '--print');
                    return { graph: printed.stdout, inputs: yield* hashed };
                });
                const [here, there] = yield* Effect.all([seen, Effect.provide(seen, workspace())]);
                assert.strictEqual(there.graph, here.graph, `[${STABLE}]`);
                assert.deepStrictEqual(there.inputs, here.inputs, `[${STABLE}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'writes no file outside the build folder its configure preset states while reading the graph',
            Effect.fnUntraced(function* () {
                const { nx, cleaned, written } = yield* Consumer;
                yield* cleaned;
                yield* nx('reset');
                const [shown, created] = yield* written(nx('show', 'projects'));
                assert.strictEqual(Number(shown.code), 0, shown.stderr);
                assert.deepStrictEqual(
                    created.filter((file) => !file.startsWith('build/default/')),
                    [],
                    `[${CONFIGURES}; ${STABLE}]`,
                );
            }),
            bounds.test,
        );

        methods.effect(
            'answers concurrent cold graph reads alike',
            Effect.fnUntraced(function* () {
                const { nx, cleaned } = yield* Consumer;
                yield* cleaned;
                yield* nx('reset');
                const shown = yield* Effect.all(
                    Array.makeBy(6, () => nx('show', 'projects', '--json')),
                    { concurrency: 'unbounded' },
                );
                assert.deepStrictEqual(
                    shown.map(({ code }) => Number(code)),
                    Array.makeBy(6, () => 0),
                    `[${STABLE}]: ${shown.map(({ stderr }) => stderr).join('\n')}`,
                );
                assert.lengthOf(Array.dedupe(shown.map(({ stdout }) => stdout.split(',').toSorted().join(','))), 1, `[${STABLE}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'keeps every node as it was when a file CMake never reads appears',
            Effect.fnUntraced(function* () {
                const { nx, wrote } = yield* Consumer;
                const first = yield* nx('graph', '--print');
                yield* wrote({ 'docs/notes.md': 'CMake never reads this file\n' });
                const second = yield* nx('graph', '--print');
                assert.strictEqual(second.stdout, first.stdout, `[${STABLE}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'gives a warm graph the project and the edge a CMakeLists edit adds',
            Effect.fnUntraced(function* () {
                const { wrote } = yield* Consumer;
                yield* placed;
                yield* wrote({
                    'CMakeLists.txt': files['CMakeLists.txt'].replace('add_subdirectory(libs/core)', 'add_subdirectory(libs/extra)\nadd_subdirectory(libs/core)'),
                    'libs/extra/CMakeLists.txt': 'project(fixture_extra VERSION 0.1.0 LANGUAGES CXX)\nadd_library(fixture_extra STATIC extra.cpp)\n',
                    'libs/extra/extra.cpp': 'int zero() { return 0; }\n',
                    'libs/core/CMakeLists.txt': `${files['libs/core/CMakeLists.txt']}target_link_libraries(fixture_core PRIVATE fixture_extra)\n`,
                });
                const { roots, edges } = yield* placed;
                const stated = yield* members;
                assert.sameMembers(
                    Object.values(roots),
                    stated.map(({ root }) => root),
                    `[${REREAD}; ${CODEMODEL}]`,
                );
                assert.sameMembers(
                    Array.dedupe(edges.map(({ source, target }) => `${roots[source]} -> ${roots[target]}`)),
                    stated.flatMap(({ root, dependencies }) => dependencies.map((dependency) => `${root} -> ${dependency}`)),
                    `[${REREAD}; ${CODEMODEL} targets[].dependencies]`,
                );
            }),
            bounds.test,
        );
    });

    suite.layer(workspace({ 'broken/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.0)\nproject(Broken LANGUAGES CXX\n', 'broken/CMakePresets.json': files['CMakePresets.json'].replace('"toolchainFile": "${sourceDir}/cmake/toolchain.cmake", ', '') }), { timeout: bounds.built })(
        'beside a tree CMake refuses to configure',
        (methods) => {
            methods.effect(
                'fails the graph with an aggregate error naming the refused tree',
                Effect.fnUntraced(function* () {
                    const [{ nx }, { failures }] = yield* Effect.all([Consumer, placed]);
                    const shown = yield* nx('show', 'projects');
                    assert.notStrictEqual(Number(shown.code), 0, `[${PARTIAL}]`);
                    assert.include(`${shown.stdout}${shown.stderr}`, 'broken/CMake', `[${PARTIAL}]`);
                    assert.isNotEmpty(
                        failures.flatMap(({ files: refused }) => refused.filter(({ file }) => file?.startsWith('broken/') ?? false)),
                        `[${PARTIAL}]`,
                    );
                }),
                bounds.test,
            );

            methods.effect(
                'keeps every project of the sound tree in the partial graph',
                Effect.fnUntraced(function* () {
                    const [stated, { roots }] = yield* Effect.all([members, placed]);
                    assert.sameMembers(
                        Object.values(roots),
                        stated.map(({ root }) => root),
                        `[${PARTIAL}; ${CODEMODEL}]`,
                    );
                }),
                bounds.test,
            );
        },
    );
});
