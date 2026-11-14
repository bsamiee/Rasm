import { isDeepStrictEqual } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { readProjectsConfigurationFromProjectGraph, type TargetConfiguration, workspaceRoot } from '@nx/devkit';
import type { Graph } from '@rasm/testing/nx';
import { Reading } from '@rasm/testing/reading';
import { Array, Duration, Effect, FileSystem, Path, Predicate, Record, Schema } from 'effect';
import { Minimatch, minimatch } from 'minimatch';
import { getExecutorInformation, parseExecutor } from 'nx/src/command-line/run/executor-utils';
import { findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { normalizeTargetDependencyWithStringProjects } from 'nx/src/tasks-runner/utils';
import { combineOptionsForExecutor } from 'nx/src/utils/params';
import { declared, read, refusals, taken, verbs } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    built: 'nrwl/nx e2e/nx/src/misc.test.ts: should successfully build a project graph',
    continuous: 'nrwl/nx e2e/nx/src/misc.test.ts: should successfully build a project graph; microsoft/playwright docs/src/test-api/class-testconfig.md forbidOnly: boolean, set from `!!process.env.CI`; a tool config that reads CI loads under the value a runner sets',
    aggregate: "README.md PRINCIPLES: Verbs aggregate `<verb>--<tool>` targets and read their inputs, and one run's arguments go to the tool-named target",
    selected: 'nrwl/nx packages/nx/src/native/affected/tasks/selection.rs: affectedness is carried from a producer to the tasks that read its outputs, never along plain dependsOn, so a verb target states each input of its tool targets for nx affected -t <verb> to select it',
    matched: 'nrwl/nx packages/nx/src/command-line/report/report.ts: nx report names each @nx package whose version differs from the installed nx; @nx/devkit/internal reads members of the nx release it was built with',
    cleaned: "README.md TASKS: `clean` removes the outputs a project's targets declare and runs its tool's clean",
    answered: 'README.md PRINCIPLES: Verbs name one operation in every ecosystem, and a project answers a verb only where one of its tools performs that operation',
    released: 'README.md PRINCIPLES: Plugins tag a project they give release configuration `release`, the one selector of `nx.json` `release.projects`',
    roots: 'nrwl/nx packages/nx/src/project-graph/error-types.ts MultipleProjectsWithSameNameError and ProjectWithNoNameError: one project per root, one root per name',
    cycle: 'nrwl/nx packages/nx/src/tasks-runner/task-graph-utils.spec.ts findCycle: should return null when no cycle',
    edges: 'nrwl/nx packages/nx/src/project-graph/project-graph-builder.spec.ts: should throw an error when the source or the target of a dependency does not exist',
    dependsOn: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts expandDependencyConfigSyntaxSugar and expandWildcardTargetConfiguration: a dependsOn entry names targets of its own project, of named projects, or of dependencies',
    executors: 'nrwl/nx e2e/plugin/src/nx-plugin.test.ts: should be able to use local generators and executors',
} as const;
const UPSTREAM = /^\^/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const misnamed = ({ nodes }: Graph) => Record.keys(Record.filter(nodes, (node, key) => node.name !== key || node.data.name !== key));

const shared = ({ nodes }: Graph) =>
    Record.keys(
        Record.filter(
            Array.groupBy(Object.values(nodes), ({ data }) => data.root),
            (held) => held.length > 1,
        ),
    );

const missing = Effect.fnUntraced(function* ({ nodes }: Graph) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const absent = yield* Effect.filter(Object.values(nodes), ({ data }) => Effect.map(fs.exists(path.join(workspaceRoot, data.root)), (found) => !found));
    return absent.map(({ name }) => name);
});

const among = ({ nodes, dependencies }: Graph) =>
    Record.map(nodes, (_, name) =>
        Array.intersection(
            (dependencies[name] ?? []).map(({ target }) => target),
            Record.keys(nodes),
        ),
    );

const unresolved = ({ nodes, dependencies, externalNodes }: Graph) =>
    Record.collect(dependencies, (key, edges) => edges.filter(({ source, target }) => source !== key || !(source in nodes || source in externalNodes) || !(target in nodes || target in externalNodes)))
        .flat()
        .map(({ source, target }) => `${source} -> ${target}`);

const depended = (workspace: Graph, project: string, entry: NonNullable<TargetConfiguration['dependsOn']>[number]) => {
    const names = (owner: string) => Record.keys(workspace.nodes[owner]?.data.targets ?? {});
    const { target: pattern, projects, dependencies = false } = Predicate.isString(entry) ? { target: entry.replace(UPSTREAM, ''), dependencies: UPSTREAM.test(entry) } : normalizeTargetDependencyWithStringProjects(entry);
    const anywhere = dependencies === true || (projects === undefined && new Minimatch(pattern).hasMagic());
    return (anywhere ? [Record.keys(workspace.nodes).flatMap(names)] : (projects ?? [project]).map(names)).every((targets) => targets.some((candidate) => minimatch(candidate, pattern)));
};

const dangling = (workspace: Graph) =>
    declared(workspace)
        .flatMap(({ project, task, target }) => (target.dependsOn ?? []).map((entry) => ({ project, task, entry })))
        .filter(({ project, entry }) => !depended(workspace, project, entry))
        .map(({ task, entry }) => [task, Predicate.isString(entry) ? entry : [entry.target, entry.projects]]);

const executing = (workspace: Graph) => declared(workspace).flatMap(({ task, target }) => Array.fromNullishOr(target.executor).map((executor) => ({ task, target, executor })));

const unanswered = (workspace: Graph) => Object.values(workspace.nodes).flatMap(({ name, data }) => verbs(workspace).filter((verb) => verb in (data.targets ?? {}) !== Record.keys(data.targets ?? {}).some((target) => target.startsWith(`${verb}--`))).map((verb) => `${name}:${verb}`));

const unread = (workspace: Graph) =>
    declared(workspace)
        .flatMap(({ project, name, task, target }) => Array.fromNullishOr(name.includes('--') ? name.split('--')[0] : undefined).map((verb) => ({ task, target, held: workspace.nodes[project]?.data.targets?.[verb]?.inputs ?? [] })))
        .filter(({ target, held }) => (target.inputs ?? []).some((input) => !held.some((other) => isDeepStrictEqual(other, input))))
        .map(({ task }) => task);

const unclean = ({ nodes }: Graph) =>
    Object.values(nodes)
        .filter(({ data }) => !('clean' in (data.targets ?? {})) && Object.values(data.targets ?? {}).some(({ outputs = [] }) => outputs.length > 0))
        .map(({ name }) => name);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect(
        'builds the project graph without a plugin failure',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual((yield* read('fresh')).failures, [], SOURCES.built);
        }),
    );

    suite.effect(
        'builds the project graph without a plugin failure under CI=true',
        Effect.fnUntraced(function* () {
            const fs = yield* FileSystem.FileSystem;
            const { checked } = yield* taken(yield* fs.makeTempDirectoryScoped({ prefix: 'nx-reading-' }), { CI: 'true' });
            const { failures } = yield* Effect.flatMap(checked, Schema.decodeEffect(Reading));
            assert.deepStrictEqual(failures, [], SOURCES.continuous);
        }, Effect.scoped),
        Duration.toMillis(Duration.minutes(10)),
    );

    suite.effect(
        'gives every project the name of its graph key',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(misnamed(yield* read('fresh')), [], SOURCES.roots);
        }),
    );

    suite.effect(
        'gives every project a root folder that exists and no other project holds',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.deepStrictEqual(yield* missing(workspace), [], SOURCES.roots);
            assert.deepStrictEqual(shared(workspace), [], SOURCES.roots);
        }),
    );

    suite.effect(
        'has no cycle among project dependencies',
        Effect.fnUntraced(function* () {
            assert.isNull(findCycle({ dependencies: among(yield* read('fresh')) }), SOURCES.cycle);
        }),
    );

    suite.effect(
        'resolves the source and target of every edge to a project or an external node',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(unresolved(yield* read('fresh')), [], SOURCES.edges);
        }),
    );

    suite.effect(
        'resolves every dependsOn entry to a target that exists',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(dangling(yield* read('fresh')), [], SOURCES.dependsOn);
        }),
    );

    suite.effect(
        'resolves the executor of every target and accepts its options under the executor schema',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const { projects } = readProjectsConfigurationFromProjectGraph(workspace);
            const refused = yield* refusals(
                executing(workspace),
                ({ task, executor }) => `${task} (${executor})`,
                ({ target, executor }) => combineOptionsForExecutor({}, '', target, getExecutorInformation(...parseExecutor(executor), workspaceRoot, projects).schema, null, null),
            );
            assert.strictEqual(executing(workspace).length, declared(workspace).length, SOURCES.executors);
            assert.deepStrictEqual(refused, [], SOURCES.executors);
        }),
    );

    suite.effect(
        'gives a project a verb target exactly where it holds a tool target of that verb',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(unanswered(yield* read('fresh')), [], SOURCES.answered);
        }),
    );

    suite.effect(
        'makes every verb target a noop that depends on its own tool targets alone',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const wrong = declared(workspace).filter(({ name, target }) => verbs(workspace).includes(name) && (target.executor !== 'nx:noop' || !isDeepStrictEqual(target.dependsOn, [`${name}--*`])));
            assert.deepStrictEqual(
                wrong.map(({ task, target }) => [task, target.executor, target.dependsOn]),
                [],
                SOURCES.aggregate,
            );
        }),
    );

    suite.effect(
        'gives every verb target each input of its tool targets',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(unread(yield* read('fresh')), [], SOURCES.selected);
        }),
    );

    suite.effect(
        'installs nx and every @nx package at one version',
        Effect.fnUntraced(function* () {
            const { externalNodes } = yield* read('fresh');
            const versions = Object.values(externalNodes)
                .filter(({ data }) => data.packageName === 'nx' || data.packageName.startsWith('@nx/'))
                .map(({ data }) => data.version);
            assert.lengthOf(Array.dedupe(versions), 1, SOURCES.matched);
        }),
    );

    suite.effect(
        'gives a clean target to every project holding a target that declares outputs',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(unclean(yield* read('fresh')), [], SOURCES.cleaned);
        }),
    );

    suite.effect(
        'tags a project release exactly where a plugin gives it release configuration',
        Effect.fnUntraced(function* () {
            const { nodes } = yield* read('fresh');
            const uneven = Object.values(nodes).filter(({ data }) => (data.tags ?? []).includes('release') !== (data.release !== undefined));
            assert.deepStrictEqual(
                uneven.map(({ name }) => name),
                [],
                SOURCES.released,
            );
        }),
    );
});
