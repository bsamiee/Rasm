import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Order, Path, Record, Schema } from 'effect';
import { createProjectRootMappings, findProjectForPath } from 'nx/src/project-graph/utils/find-project-for-path';
import { declared, dependents, hosted, nx, read, tracked, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    crate: 'README.md PRINCIPLES: `@rasm/nx-cargo` infers the `default` named input of a crate (its files, the workspace lock, manifest, toolchain, and Cargo variables), so every target of the crate reads them',
    locked: 'README.md PRINCIPLES: Cargo, uv, SwiftPM, and xcodebuild targets read the committed lock (`--locked`, `--force-resolved-versions`, `-onlyUsePackageVersionsFromResolvedFile`); astral-sh/uv docs/concepts/projects/workspaces.md: the workspace shares a single lockfile; docs/concepts/projects/layout.md: uv creates a `uv.lock` file next to the `pyproject.toml`; crates/uv-tool-commands/src/common.rs: `uv tool install` writes its own lock into the tool folder and reads no project `uv.lock`, so `install` targets are outside the rule',
    read: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source, `.config/mise.lock`, tool `--version` output, and credentials are no input",
    versions: 'README.md OWNERS: Package version — `pnpm-workspace.yaml` catalog, `pyproject.toml` `[project]` or group, `Directory.Packages.props` row',
    options: 'README.md LAYOUT: tsconfig.base.json — TypeScript options every package compiles and emits with; nrwl/nx packages/nx/src/native/affected/tasks/changed_contents.rs compare_ts_configs: a root tsconfig change outside `paths` touches every task, so the file bounds no reach',
    own: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects a task whose own files changed, and its dependents; packages/nx/src/native/affected/tasks/selection.rs: a change reaches a requested task through its own inputs or the outputs it reads, never along plain dependsOn',
    none: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects nothing when the change reaches no input',
    apart: 'nrwl/nx packages/nx/src/native/affected/tasks/touched.rs: a task is touched by a file its inputs match, an external node its plan names, or a file that configures its project, and selection.rs carries a touch to the tasks reading its outputs; a root manifest configures the root project, so every task of that project is touched by rule and none through an input; README.md PRINCIPLES: Suites reading the project graph take tag `os:all`',
    always: 'nrwl/nx packages/nx/src/native/tasks/hash_planner.rs ALWAYS_ON_FILES: every task hashes nx.json, .gitignore, and .nxignore',
    moved: "nrwl/nx packages/nx/src/native/affected/tasks/touched.rs: a lockfile change reaches a plan as the external node's name",
    projects: 'nrwl/nx e2e/nx/src/affected-graph.test.ts show projects --affected: should print information about affected projects',
    origins: 'nrwl/nx packages/nx/src/project-graph/utils/project-configuration/source-maps.ts: Nx records for each project property the file and plugin it came from; packages/nx/src/command-line/show/target.ts: show target inputs lists the workspace files Nx hashes for a task',
} as const;
const CHECKED = ['check', 'test', 'build'] as const;
const NODE = ['@rasm/nx-typescript', '@rasm/nx-vite', '@rasm/nx-vitest', '@rasm/nx-storybook'] as const;
const READ = [
    { file: 'Cargo.lock', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'Cargo.toml', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'rust-toolchain.toml', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'Directory.Packages.props', plugins: ['@rasm/nx-dotnet'], source: SOURCES.versions },
    { file: 'tsconfig.base.json', plugins: ['@rasm/nx-typescript'], source: SOURCES.options },
] as const;
const APART = [
    { file: 'Cargo.lock', plugins: ['@rasm/nx-cargo'] },
    { file: 'Cargo.toml', plugins: ['@rasm/nx-cargo'] },
    { file: 'rust-toolchain.toml', plugins: ['@rasm/nx-cargo'] },
    { file: 'uv.lock', plugins: ['@rasm/nx-uv'] },
    { file: 'pnpm-lock.yaml', plugins: NODE },
    { file: 'Directory.Packages.props', plugins: ['@rasm/nx-dotnet'] },
    { file: 'Package.resolved', plugins: ['@rasm/nx-apple'] },
    { file: 'vcpkg.json', plugins: ['@rasm/nx-cmake'] },
] as const;
const SOURCED = [
    { plugin: '@rasm/nx-cargo', files: ['*.rs'] },
    { plugin: '@rasm/nx-uv', files: ['*.py'] },
    { plugin: '@rasm/nx-dotnet', files: ['*.cs'] },
    { plugin: '@rasm/nx-apple', files: ['*.swift', ':(exclude)*Package*.swift'] },
    { plugin: '@rasm/nx-cmake', files: ['*.cpp'] },
    { plugin: '@rasm/nx-typescript', files: ['*/index.ts'] },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Reasons = Schema.Record(Schema.String, Schema.Array(Schema.Struct({ kind: Schema.String })));
const Explained = Schema.fromJsonString(Schema.Struct({ affected: Reasons, upstream: Reasons })).annotate({
    description: 'JSON nx affected --explain=stdout prints: under affected the selected tasks and under upstream the reached tasks outside the selection, each keyed by task id with the kind of every reason it was reached (input-file, dependent-output, project-configuration, external-dependencies, moved-ecosystem)',
});
const Listed = Schema.fromJsonString(Schema.Array(Schema.String)).annotate({ description: 'JSON nx show projects --json prints: project names' });
const Hashed = Schema.fromJsonString(Schema.Struct({ files: Schema.Array(Schema.String) })).annotate({ description: 'JSON nx show target inputs prints: the workspace files Nx hashes for one task' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const reached = Effect.fnUntraced(function* (targets: readonly string[], file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    assert.isTrue(yield* fs.exists(path.join(workspaceRoot, file)), `${file} exists`);
    const { checked } = yield* nx(['affected', `--targets=${targets.join(',')}`, `--files=${file}`, '--explain=stdout']);
    const { affected, upstream } = yield* Effect.flatMap(checked, Schema.decodeEffect(Explained));
    return Record.map({ ...upstream, ...affected }, (reasons) => reasons.map(({ kind }) => kind));
});

const named = (workspace: Workspace) => Array.dedupe(declared(workspace).map(({ name }) => name));

const inferred = (workspace: Workspace, plugins: readonly string[]) => declared(workspace).filter(({ plugin }) => Option.exists(plugin, (owner) => plugins.includes(owner)));

const reading = ({ nodes }: Workspace) => new Set(Record.keys(Record.filter(nodes, ({ data }) => (data.tags ?? []).includes('os:all'))));

const unreached = (tasks: readonly { readonly task: string }[], found: Record.ReadonlyRecord<string, unknown>) => tasks.filter(({ task }) => !(task in found)).map(({ task }) => task);

const leaked = (workspace: Workspace, plugins: readonly string[], found: Record.ReadonlyRecord<string, readonly string[]>) => {
    const own = new Set(inferred(workspace, plugins).map(({ project }) => project));
    const allowed = dependents(workspace, own).difference(own).union(reading(workspace));
    return declared(workspace)
        .filter(({ name, plugin }) => name.includes('--') && !Option.exists(plugin, (owner) => plugins.includes(owner)))
        .filter(({ project, task }) => !allowed.has(project) && Array.intersection(found[task] ?? [], ['input-file', 'external-dependencies']).length > 0)
        .map(({ task }) => `${task}: ${(found[task] ?? []).join(', ')}`);
};

const outside = (workspace: Workspace, allowed: ReadonlySet<string>, found: Record.ReadonlyRecord<string, unknown>) => {
    const owners = Record.fromIterableWith(declared(workspace), ({ task, project }) => [task, project]);
    return Record.keys(found).filter((task) => !Option.exists(Record.get(owners, task), (project) => allowed.has(project)));
};

const packaged = (workspace: Workspace) => declared(workspace).filter(({ task }) => Record.keys(workspace.hashes[task]?.inputs ?? {}).some((input) => input.startsWith('npm:')));

const nearest = (path: Path.Path, held: ReadonlySet<string>, folder: string, name: string): string => {
    const file = path.join(folder, name);
    return held.has(file) || folder === '.' ? file : nearest(path, held, path.dirname(folder), name);
};

const unlocked = Effect.fnUntraced(function* (workspace: Workspace, plugin: string, name: string) {
    const [path, files] = yield* Effect.all([Path.Path, tracked(`*${name}`)]);
    const locked = Array.groupBy(
        inferred(workspace, [plugin]).filter(({ name: target }) => target.split('--')[0] !== 'install'),
        ({ root }) => nearest(path, new Set(files), root, name),
    );
    const found = yield* Effect.forEach(Record.keys(locked), (lock) => reached(named(workspace), lock));
    return { locks: Record.keys(locked), missed: Array.zip(Record.toEntries(locked), found).flatMap(([[lock, tasks], held]) => unreached(tasks, held).map((task) => `${lock}: ${task}`)) };
});

const sampled = Effect.fnUntraced(function* (workspace: Workspace, plugin: string, files: readonly string[]) {
    const building = new Set(
        inferred(workspace, [plugin])
            .filter(({ name }) => name.startsWith('build--'))
            .map(({ project }) => project),
    );
    const roots = createProjectRootMappings(workspace.nodes);
    const owned = (yield* tracked(...files)).flatMap((file) => Array.fromNullishOr(findProjectForPath(file, roots)).map((project) => ({ project, file })));
    return yield* Effect.fromOption(Array.findFirst(owned, ({ project }) => building.has(project)));
});

const origins = ({ sourceMaps }: Workspace) => Array.dedupe(Object.values(sourceMaps).flatMap((entries) => Object.values(entries).flatMap(([file]) => Array.fromNullishOr(file))));

const hashed = Effect.fnUntraced(function* (task: string) {
    const { checked } = yield* nx(['show', 'target', 'inputs', task, '--json']);
    const { files } = yield* Effect.flatMap(checked, Schema.decodeEffect(Hashed));
    return new Set(files);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect.each(READ)(
        'reaches every task of $plugins with a change to $file',
        Effect.fnUntraced(function* ({ file, plugins, source }) {
            const workspace = yield* read('fresh');
            const tasks = inferred(workspace, plugins);
            assert.isNotEmpty(tasks, source);
            assert.deepStrictEqual(unreached(tasks, yield* reached(named(workspace), file)), [], source);
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

    suite.effect.each(SOURCED.filter(({ plugin }) => hosted(plugin)))(
        'selects with a change to one source file of a $plugin project each of check, test, and build that project holds, and no task outside it and the projects depending on it',
        Effect.fnUntraced(function* ({ plugin, files }) {
            const workspace = yield* read('fresh');
            const { project, file } = yield* sampled(workspace, plugin, files);
            const found = yield* reached(CHECKED, file);
            const held = CHECKED.filter((verb) => verb in (workspace.nodes[project]?.data.targets ?? {})).map((verb) => ({ task: `${project}:${verb}` }));
            assert.isNotEmpty(held, `${SOURCES.own}: ${project} for ${file}`);
            assert.deepStrictEqual(unreached(held, found), [], `${SOURCES.own}: ${file}`);
            assert.deepStrictEqual(outside(workspace, dependents(workspace, new Set([project])), found), [], `${SOURCES.own}: ${file}`);
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

    suite.effect(
        'hashes for its own test target the files its tests open by name and every file Nx records as the origin of a project property',
        Effect.fnUntraced(function* () {
            const [path, workspace] = yield* Effect.all([Path.Path, read('fresh')]);
            const own = yield* Effect.fromNullishOr(findProjectForPath(path.relative(workspaceRoot, import.meta.dirname), createProjectRootMappings(workspace.nodes)));
            const files = yield* hashed(`${own}:test--vitest`);
            assert.isNotEmpty(origins(workspace), SOURCES.origins);
            assert.deepStrictEqual(
                ['nx.json', 'README.md', '.github/workflows/ci.yml', ...origins(workspace)].filter((file) => !files.has(file)),
                [],
                `${SOURCES.origins}; ${SOURCES.always}`,
            );
        }),
    );
});
