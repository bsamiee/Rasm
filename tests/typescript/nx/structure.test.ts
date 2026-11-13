import { isDeepStrictEqual } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { readProjectsConfigurationFromProjectGraph, type TargetConfiguration, workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Path, Predicate, Record } from 'effect';
import { Minimatch, minimatch } from 'minimatch';
import { getExecutorInformation, parseExecutor } from 'nx/src/command-line/run/executor-utils';
import { readNxJson } from 'nx/src/config/nx-json';
import { findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { normalizeTargetDependencyWithStringProjects } from 'nx/src/tasks-runner/utils';
import { combineOptionsForExecutor } from 'nx/src/utils/params';
import { declared, read, refusals, verbs, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    built: 'nrwl/nx e2e/nx/src/misc.test.ts: should successfully build a project graph',
    inferred: 'README.md PRINCIPLES: Nx plugins infer every project, target, and dependency from manifests and tool output, with no `project.json` or hand-written `namedInputs`',
    aggregate: 'README.md PRINCIPLES: Verbs aggregate `<verb>--<tool>` targets and read their inputs',
    answered: 'README.md PRINCIPLES: Verbs name one operation in every ecosystem, and a project answers a verb only where one of its tools performs that operation',
    platform: 'README.md PRINCIPLES: Plugins present a target that runs a host or needs a platform only on a machine holding it, and tag a project a platform limits `os:<platform>`',
    released: "README.md PRINCIPLES: Plugins tag a project they give release configuration `release`, which `nx.json` `release.projects` selects beside Nx's `npm:public`",
    ordered: 'README.md PRINCIPLES: Plugin rows merge in order and a later row names a shared root, so `@rasm/nx-distribution` comes first and a host manifest names only a folder no ecosystem manifest names',
    roots: 'nrwl/nx packages/nx/src/project-graph/error-types.ts MultipleProjectsWithSameNameError and ProjectWithNoNameError: one project per root, one root per name',
    cycle: 'nrwl/nx packages/nx/src/tasks-runner/task-graph-utils.spec.ts findCycle: should return null when no cycle',
    edges: 'nrwl/nx packages/nx/src/project-graph/project-graph-builder.spec.ts: should throw an error when the source or the target of a dependency does not exist',
    dependsOn: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts expandDependencyConfigSyntaxSugar and expandWildcardTargetConfiguration: a dependsOn entry names targets of its own project, of named projects, or of dependencies',
    executors: 'nrwl/nx e2e/plugin/src/nx-plugin.test.ts: should be able to use local generators and executors',
} as const;
const UPSTREAM = /^\^/u;
const TAG = /^(?:release|npm:public|npm:private|os:(?:aix|android|cygwin|darwin|freebsd|haiku|linux|netbsd|openbsd|sunos|win32))$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const rows = () => (readNxJson(workspaceRoot).plugins ?? []).map((row) => (Predicate.isString(row) ? row : row.plugin));

const misnamed = ({ nodes }: Workspace) => Record.keys(Record.filter(nodes, (node, key) => node.name !== key || node.data.name !== key));

const shared = ({ nodes }: Workspace) =>
    Record.keys(
        Record.filter(
            Array.groupBy(Object.values(nodes), ({ data }) => data.root),
            (held) => held.length > 1,
        ),
    );

const missing = Effect.fnUntraced(function* ({ nodes }: Workspace) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const absent = yield* Effect.filter(Object.values(nodes), ({ data }) => Effect.map(fs.exists(path.join(workspaceRoot, data.root)), (found) => !found));
    return absent.map(({ name }) => name);
});

const described = Effect.fnUntraced(function* ({ nodes, sourceMaps }: Workspace) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const sourced = Object.values(sourceMaps).flatMap((entries) => Object.values(entries).flatMap(([file]) => Array.fromNullishOr(file)));
    const candidates = Array.dedupe([...Object.values(nodes).map(({ data }) => path.join(data.root, 'project.json')), ...sourced.filter((file) => path.basename(file) === 'project.json')]);
    return yield* Effect.filter(candidates, (file) => fs.exists(path.join(workspaceRoot, file)));
});

const among = ({ nodes, dependencies }: Workspace) =>
    Record.map(nodes, (_, name) =>
        Array.intersection(
            (dependencies[name] ?? []).map(({ target }) => target),
            Record.keys(nodes),
        ),
    );

const unresolved = ({ nodes, dependencies, externalNodes }: Workspace) =>
    Record.collect(dependencies, (key, edges) => edges.filter(({ source, target }) => source !== key || !(source in nodes || source in externalNodes) || !(target in nodes || target in externalNodes)))
        .flat()
        .map(({ source, target }) => `${source} -> ${target}`);

const depended = (workspace: Workspace, project: string, entry: NonNullable<TargetConfiguration['dependsOn']>[number]) => {
    const names = (owner: string) => Record.keys(workspace.nodes[owner]?.data.targets ?? {});
    const { target: pattern, projects, dependencies = false } = Predicate.isString(entry) ? { target: entry.replace(UPSTREAM, ''), dependencies: UPSTREAM.test(entry) } : normalizeTargetDependencyWithStringProjects(entry);
    const anywhere = dependencies === true || (projects === undefined && new Minimatch(pattern).hasMagic());
    return (anywhere ? [Record.keys(workspace.nodes).flatMap(names)] : (projects ?? [project]).map(names)).every((targets) => targets.some((candidate) => minimatch(candidate, pattern)));
};

const dangling = (workspace: Workspace) =>
    declared(workspace)
        .flatMap(({ project, task, target }) => (target.dependsOn ?? []).map((entry) => ({ project, task, entry })))
        .filter(({ project, entry }) => !depended(workspace, project, entry))
        .map(({ task, entry }) => [task, Predicate.isString(entry) ? entry : [entry.target, entry.projects]]);

const executing = (workspace: Workspace) => declared(workspace).flatMap(({ task, target }) => Array.fromNullishOr(target.executor).map((executor) => ({ task, target, executor })));

const unanswered = ({ nodes }: Workspace, stated: readonly string[]) => Object.values(nodes).flatMap(({ name, data }) => stated.filter((verb) => verb in (data.targets ?? {}) !== Record.keys(data.targets ?? {}).some((target) => target.startsWith(`${verb}--`))).map((verb) => `${name}:${verb}`));

const unread = (workspace: Workspace, stated: readonly string[]) =>
    declared(workspace)
        .flatMap(({ project, name, task, target }) => Array.fromNullishOr(name.includes('--') ? name.split('--')[0] : undefined).map((verb) => ({ verb, task, target, held: workspace.nodes[project]?.data.targets?.[verb]?.inputs ?? [] })))
        .filter(({ verb, target, held }) => stated.includes(verb) && (target.inputs ?? []).some((input) => !held.some((other) => isDeepStrictEqual(other, input))))
        .map(({ task }) => task);

const contributing = ({ sourceMaps }: Workspace) => new Set(Object.values(sourceMaps).flatMap((entries) => Object.values(entries).map(([, plugin]) => plugin)));

const renamed = ({ sourceMaps }: Workspace, [first, ...later]: readonly string[]) =>
    Record.keys(
        Record.filter(sourceMaps, ({ name }, root) => {
            const plugins = new Set(Object.values(sourceMaps[root] ?? {}).map(([, plugin]) => plugin));
            return name?.[1] === first && !plugins.isDisjointFrom(new Set(later));
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('graph structure', (suite) => {
    suite.effect(
        'builds the project graph without a plugin failure',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual((yield* read('fresh')).failures, [], SOURCES.built);
        }),
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
        'holds no project.json',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(yield* described(yield* read('fresh')), [], SOURCES.inferred);
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
        'names every target a README verb, a tool of one, or the publish target of Nx release',
        Effect.fnUntraced(function* () {
            const [workspace, stated] = yield* Effect.all([read('fresh'), verbs]);
            const named = new Set(declared(workspace).map(({ name }) => name.split('--')[0] ?? name));
            assert.isNotEmpty(stated, 'README.md TASKS lists verbs');
            assert.deepStrictEqual([...named.difference(new Set([...stated, 'nx-release-publish']))], [], SOURCES.answered);
        }),
    );

    suite.effect(
        'gives a project a verb target exactly where it holds a tool target of that verb',
        Effect.fnUntraced(function* () {
            const [workspace, stated] = yield* Effect.all([read('fresh'), verbs]);
            assert.deepStrictEqual(unanswered(workspace, stated), [], SOURCES.answered);
        }),
    );

    suite.effect(
        'makes every verb target a noop that depends on its own tool targets alone',
        Effect.fnUntraced(function* () {
            const [workspace, stated] = yield* Effect.all([read('fresh'), verbs]);
            const wrong = declared(workspace).filter(({ name, target }) => stated.includes(name) && (target.executor !== 'nx:noop' || !isDeepStrictEqual(target.dependsOn, [`${name}--*`])));
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
            const [workspace, stated] = yield* Effect.all([read('fresh'), verbs]);
            assert.deepStrictEqual(unread(workspace, stated), [], SOURCES.aggregate);
        }),
    );

    suite.effect(
        'tags projects with os:<platform>, release, and the npm tags of Nx alone',
        Effect.fnUntraced(function* () {
            const { nodes } = yield* read('fresh');
            const tags = Array.dedupe(Object.values(nodes).flatMap(({ data }) => data.tags ?? []));
            assert.deepStrictEqual(
                tags.filter((tag) => !TAG.test(tag)),
                [],
                `${SOURCES.platform}; ${SOURCES.released}`,
            );
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

    suite.effect(
        'lets a later plugin row name each root it shares with @rasm/nx-distribution',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.isTrue(contributing(workspace).has('@rasm/nx-distribution'), SOURCES.ordered);
            assert.strictEqual(rows()[0], '@rasm/nx-distribution', SOURCES.ordered);
            assert.deepStrictEqual(renamed(workspace, rows()), [], SOURCES.ordered);
        }),
    );
});
