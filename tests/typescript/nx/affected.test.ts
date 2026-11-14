import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import type { Graph } from '@rasm/testing/nx';
import { Array, Effect, FileSystem, Option, Order, Path, Record, Schema } from 'effect';
import { createProjectRootMappings, findProjectForPath } from 'nx/src/project-graph/utils/find-project-for-path';
import { declared, dependents, ECOSYSTEMS, nx, read, tracked } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    shared: "README.md PRINCIPLES: `@rasm/nx-cargo` infers the `default` named input of a crate (its files, the workspace lock, manifest, toolchain, and Cargo variables), so every target of the crate reads them; OWNERS: Package version — `Directory.Packages.props` row; LAYOUT: tsconfig.base.json — TypeScript options every package compiles and emits with; nrwl/nx packages/nx/src/native/affected/tasks/changed_contents.rs compare_ts_configs: a root tsconfig change outside `paths` touches every task",
    locked: 'README.md PRINCIPLES: Cargo, uv, SwiftPM, and xcodebuild targets read the committed lock (`--locked`, `--force-resolved-versions`, `-onlyUsePackageVersionsFromResolvedFile`); astral-sh/uv docs/concepts/projects/workspaces.md: the workspace shares a single lockfile; docs/concepts/projects/layout.md: uv creates a `uv.lock` file next to the `pyproject.toml`; crates/uv-tool-commands/src/common.rs: `uv tool install` writes its own lock into the tool folder and reads no project `uv.lock`, so `install` targets are outside the rule',
    read: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source, `.config/mise.lock`, tool `--version` output, and credentials are no input",
    own: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects a task whose own files changed, and its dependents; packages/nx/src/native/affected/tasks/selection.rs: a change reaches a requested task through its own inputs or the outputs it reads, never along plain dependsOn',
    none: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects nothing when the change reaches no input',
    apart: 'nrwl/nx packages/nx/src/native/affected/tasks/touched.rs: a task is touched by a file its inputs match, an external node its plan names, or a file that configures its project, and selection.rs carries a touch to the tasks reading its outputs; a root manifest configures the root project, so every task of that project is touched by rule and none through an input; README.md PRINCIPLES: Suites reading the project graph take tag `os:all`',
    always: 'nrwl/nx packages/nx/src/native/tasks/hash_planner.rs ALWAYS_ON_FILES: every task hashes nx.json, .gitignore, and .nxignore',
    moved: "nrwl/nx packages/nx/src/native/affected/tasks/touched.rs: a lockfile change reaches a plan as the external node's name",
    projects: 'nrwl/nx e2e/nx/src/affected-graph.test.ts show projects --affected: should print information about affected projects',
} as const;
const CHECKED = ['check', 'test', 'build'] as const;
const SHARED = Record.collect(ECOSYSTEMS, (plugin, { shared }) => shared.map((file) => ({ plugin, file }))).flat();
const APART = Record.collect(
    Array.groupBy(
        Record.collect(ECOSYSTEMS, (plugin, { locks }) => locks.map((file) => ({ plugin, file }))).flat(),
        ({ file }) => file,
    ),
    (file, held) => ({ file, plugins: held.map(({ plugin }) => plugin) }),
);
const SOURCED = Record.collect(
    Record.filter(ECOSYSTEMS, ({ sources }) => Array.isReadonlyArrayNonEmpty(sources)),
    (plugin, { sources }) => ({ plugin, files: sources }),
);

// --- [MODELS] --------------------------------------------------------------------------

const Reasons = Schema.Record(Schema.String, Schema.Array(Schema.Struct({ kind: Schema.String })));
const Explained = Schema.fromJsonString(Schema.Struct({ affected: Reasons, upstream: Reasons })).annotate({
    description: 'JSON nx affected --explain=stdout prints: under affected the selected tasks and under upstream the reached tasks outside the selection, each keyed by task id with the kind of every reason it was reached (input-file, dependent-output, project-configuration, external-dependencies, moved-ecosystem)',
});
const Listed = Schema.fromJsonString(Schema.Array(Schema.String)).annotate({ description: 'JSON nx show projects --json prints: project names' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const reached = Effect.fnUntraced(function* (targets: readonly string[], file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    assert.isTrue(yield* fs.exists(path.join(workspaceRoot, file)), `${file} exists`);
    const { checked } = yield* nx(['affected', `--targets=${targets.join(',')}`, `--files=${file}`, '--explain=stdout']);
    const { affected, upstream } = yield* Effect.flatMap(checked, Schema.decodeEffect(Explained));
    return Record.map({ ...upstream, ...affected }, (reasons) => reasons.map(({ kind }) => kind));
});

const named = (workspace: Graph) => Array.dedupe(declared(workspace).map(({ name }) => name));

const inferred = (workspace: Graph, plugins: readonly string[]) => declared(workspace).filter(({ plugin }) => Option.exists(plugin, (owner) => plugins.includes(owner)));

const reading = ({ nodes }: Graph) => new Set(Record.keys(Record.filter(nodes, ({ data }) => (data.tags ?? []).includes('os:all'))));

const unreached = (tasks: readonly { readonly task: string }[], found: Record.ReadonlyRecord<string, unknown>) => tasks.filter(({ task }) => !(task in found)).map(({ task }) => task);

const leaked = (workspace: Graph, plugins: readonly string[], found: Record.ReadonlyRecord<string, readonly string[]>) => {
    const own = new Set(inferred(workspace, plugins).map(({ project }) => project));
    const allowed = dependents(workspace, own).difference(own).union(reading(workspace));
    return declared(workspace)
        .filter(({ name, plugin }) => name.includes('--') && !Option.exists(plugin, (owner) => plugins.includes(owner)))
        .filter(({ project, task }) => !allowed.has(project) && Array.intersection(found[task] ?? [], ['input-file', 'external-dependencies']).length > 0)
        .map(({ task }) => `${task}: ${(found[task] ?? []).join(', ')}`);
};

const outside = (workspace: Graph, allowed: ReadonlySet<string>, found: Record.ReadonlyRecord<string, unknown>) => {
    const owners = Record.fromIterableWith(declared(workspace), ({ task, project }) => [task, project]);
    return Record.keys(found).filter((task) => !Option.exists(Record.get(owners, task), (project) => allowed.has(project)));
};

const packaged = (workspace: Graph) => declared(workspace).filter(({ task }) => Record.keys(workspace.hashes[task]?.inputs ?? {}).some((input) => input.startsWith('npm:')));

const nearest = (path: Path.Path, held: ReadonlySet<string>, folder: string, name: string): string => {
    const file = path.join(folder, name);
    return held.has(file) || folder === '.' ? file : nearest(path, held, path.dirname(folder), name);
};

const unlocked = Effect.fnUntraced(function* (workspace: Graph, plugin: string, name: string) {
    const [path, files] = yield* Effect.all([Path.Path, tracked(`*${name}`)]);
    const locked = Array.groupBy(
        inferred(workspace, [plugin]).filter(({ name: target }) => target.split('--')[0] !== 'install'),
        ({ root }) => nearest(path, new Set(files), root, name),
    );
    const found = yield* Effect.forEach(Record.keys(locked), (lock) => reached(named(workspace), lock));
    return { locks: Record.keys(locked), missed: Array.zip(Record.toEntries(locked), found).flatMap(([[lock, tasks], held]) => unreached(tasks, held).map((task) => `${lock}: ${task}`)) };
});

const sampled = Effect.fnUntraced(function* (workspace: Graph, plugin: string, files: readonly string[]) {
    const building = new Set(
        inferred(workspace, [plugin])
            .filter(({ name }) => name.startsWith('build--'))
            .map(({ project }) => project),
    );
    const roots = createProjectRootMappings(workspace.nodes);
    const owned = (yield* tracked(...files)).flatMap((file) => Array.fromNullishOr(findProjectForPath(file, roots)).map((project) => ({ project, file })));
    return Array.findFirst(owned, ({ project }) => building.has(project));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect.each(SHARED)(
        'reaches every task of $plugin with a change to $file',
        Effect.fnUntraced(function* ({ file, plugin }) {
            const workspace = yield* read('fresh');
            const tasks = inferred(workspace, [plugin]);
            assert.isNotEmpty(tasks, SOURCES.shared);
            assert.deepStrictEqual(unreached(tasks, yield* reached(named(workspace), file)), [], SOURCES.shared);
        }),
    );

    suite.effect(
        'reaches every task of a uv project but its install into the tool folder with a change to the uv.lock nearest its folder',
        Effect.fnUntraced(function* () {
            const { locks, missed } = yield* unlocked(yield* read('fresh'), '@rasm/nx-uv', 'uv.lock');
            assert.isNotEmpty(locks, SOURCES.locked);
            assert.deepStrictEqual(missed, [], SOURCES.locked);
        }),
    );

    suite.effect(
        'reaches every task with a change to nx.json',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.deepStrictEqual(unreached(declared(workspace), yield* reached(named(workspace), 'nx.json')), [], SOURCES.always);
        }),
    );

    suite.effect(
        'reaches every task hashing an npm package with a change to pnpm-lock.yaml',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.isNotEmpty(packaged(workspace), SOURCES.moved);
            assert.deepStrictEqual(unreached(packaged(workspace), yield* reached(named(workspace), 'pnpm-lock.yaml')), [], SOURCES.moved);
        }),
    );

    suite.effect.each(APART)(
        'selects for check, test, and build with a change to $file through an input or an external package no tool task of another plugin than $plugins, but in a project depending on theirs or in a suite reading the graph',
        Effect.fnUntraced(function* ({ file, plugins }) {
            const workspace = yield* read('fresh');
            assert.deepStrictEqual(leaked(workspace, plugins, yield* reached(CHECKED, file)), [], SOURCES.apart);
        }),
    );

    suite.effect(
        'selects no check, test, or build task with a change to a file under docs or to .config/mise.lock, and with a change to README.md only tasks of the suites reading the graph',
        Effect.fnUntraced(function* () {
            const [workspace, noted] = yield* Effect.all([read('fresh'), tracked('docs/*.md')]);
            const [documented, locked, stated] = yield* Effect.forEach([...Array.take(noted, 1), '.config/mise.lock', 'README.md'], (file) => reached(CHECKED, file));
            assert.isNotEmpty(noted, SOURCES.none);
            assert.deepStrictEqual(Record.keys(documented ?? {}), [], SOURCES.none);
            assert.deepStrictEqual(Record.keys(locked ?? {}), [], SOURCES.read);
            assert.deepStrictEqual(outside(workspace, reading(workspace), stated ?? {}), [], SOURCES.none);
        }),
    );

    suite.effect.each(SOURCED)(
        'selects with a change to one source file of a $plugin project each of check, test, and build that project holds, and no task outside it and the projects depending on it',
        Effect.fnUntraced(function* ({ plugin, files }) {
            const workspace = yield* read('fresh');
            const sample = yield* sampled(workspace, plugin, files);
            yield* Effect.forEach(
                Option.toArray(sample),
                Effect.fnUntraced(function* ({ project, file }) {
                    const found = yield* reached(CHECKED, file);
                    const held = CHECKED.filter((verb) => verb in (workspace.nodes[project]?.data.targets ?? {})).map((verb) => ({ task: `${project}:${verb}` }));
                    assert.isNotEmpty(held, `${SOURCES.own}: ${project} for ${file}`);
                    assert.deepStrictEqual(unreached(held, found), [], `${SOURCES.own}: ${file}`);
                    assert.deepStrictEqual(outside(workspace, dependents(workspace, new Set([project])), found), [], `${SOURCES.own}: ${file}`);
                }),
            );
        }),
    );

    suite.effect(
        'marks a project and the projects that depend on it, and no other, for a change to one of its files',
        Effect.fnUntraced(function* () {
            const [fs, path, workspace] = yield* Effect.all([FileSystem.FileSystem, Path.Path, read('fresh')]);
            const depended = Object.values(workspace.nodes).filter(({ name, data }) => data.root !== '.' && dependents(workspace, new Set([name])).size > 1);
            const found = yield* Effect.findFirst(
                Array.sort(
                    depended,
                    Order.mapInput(Order.String, ({ name }: { readonly name: string }) => name),
                ),
                ({ data }) => fs.exists(path.join(workspaceRoot, data.root, 'index.ts')),
            );
            const changed = yield* Effect.fromOption(found);
            const { checked } = yield* nx(['show', 'projects', '--affected', `--files=${path.join(changed.data.root, 'index.ts')}`, '--json']);
            assert.deepStrictEqual(Array.sort(yield* Effect.flatMap(checked, Schema.decodeEffect(Listed)), Order.String), Array.sort([...dependents(workspace, new Set([changed.name]))], Order.String), `${SOURCES.own}; ${SOURCES.projects}`);
        }),
    );
});
