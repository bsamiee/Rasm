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
    aggregate: 'README.md PRINCIPLES: Verbs aggregate `<verb>--<tool>` targets and read their inputs; nrwl/nx packages/nx/src/native/affected/tasks/selection.rs: a change reaches a requested task through its own inputs or the outputs it reads, never along plain dependsOn',
    own: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects a task whose own files changed, and its dependents',
    none: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects nothing when the change reaches no input; a target stating `{workspaceRoot}/**/*` reads every file',
    projects: 'nrwl/nx e2e/nx/src/affected-graph.test.ts show projects --affected: should print information about affected projects; packages/nx/src/native/affected/projects/implicit_dependencies.rs implicitly_touched_projects: a `{workspaceRoot}/…` fileset of a target input marks its project for a matching file',
} as const;
const READ = [
    { file: 'Cargo.lock', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'Cargo.toml', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'rust-toolchain.toml', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'Directory.Packages.props', plugins: ['@rasm/nx-dotnet'], source: SOURCES.versions },
    { file: 'tsconfig.base.json', plugins: ['@rasm/nx-typescript'], source: SOURCES.options },
] as const;
const BOUND = [
    { file: 'Cargo.lock', plugins: ['@rasm/nx-cargo'] },
    { file: 'Cargo.toml', plugins: ['@rasm/nx-cargo'] },
    { file: 'rust-toolchain.toml', plugins: ['@rasm/nx-cargo'] },
    { file: 'uv.lock', plugins: ['@rasm/nx-uv'] },
    { file: 'pnpm-lock.yaml', plugins: ['@rasm/nx-typescript', '@rasm/nx-vite', '@rasm/nx-vitest', '@rasm/nx-storybook'] },
    { file: 'Directory.Packages.props', plugins: ['@rasm/nx-dotnet'] },
] as const;
const BUILT = [
    { plugin: '@rasm/nx-cargo', extension: '.rs' },
    { plugin: '@rasm/nx-uv', extension: '.py' },
    { plugin: '@rasm/nx-dotnet', extension: '.cs' },
    { plugin: '@rasm/nx-apple', extension: '.swift' },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Explained = Schema.fromJsonString(Schema.Struct({ affected: Schema.Record(Schema.String, Schema.Unknown), upstream: Schema.Record(Schema.String, Schema.Unknown) })).annotate({
    description: 'JSON nx affected --explain=stdout prints: under affected the selected tasks and under upstream the reached tasks outside the selection, each keyed by task id, reached through its own inputs or the outputs it reads',
});
const Listed = Schema.fromJsonString(Schema.Array(Schema.String)).annotate({ description: 'JSON nx show projects --json prints: project names' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const reached = Effect.fnUntraced(function* (targets: readonly string[], file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    assert.isTrue(yield* fs.exists(path.join(workspaceRoot, file)), `${file} exists`);
    const { checked } = yield* nx(['affected', `--targets=${targets.join(',')}`, `--files=${file}`, '--explain=stdout']);
    const { affected, upstream } = yield* Effect.flatMap(checked, Schema.decodeEffect(Explained));
    return new Set([...Record.keys(affected), ...Record.keys(upstream)]);
});

const named = (workspace: Workspace) => Array.dedupe(declared(workspace).map(({ name }) => name));

const inferred = (workspace: Workspace, plugins: readonly string[]) => declared(workspace).filter(({ plugin }) => Option.exists(plugin, (owner) => plugins.includes(owner)));

const whole = (workspace: Workspace) => declared(workspace).filter(({ target }) => (target.inputs ?? []).includes('{workspaceRoot}/**/*'));

const bounded = (workspace: Workspace) => {
    const every = new Set(whole(workspace).map(({ task }) => task));
    return declared(workspace).filter(({ task, target }) => target.cache === true && !every.has(task));
};

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
    return { locks: Record.keys(locked), missed: Array.zip(Record.toEntries(locked), found).flatMap(([[lock, tasks], held]) => tasks.filter(({ task }) => !held.has(task)).map(({ task }) => `${lock}: ${task}`)) };
});

const sampled = Effect.fnUntraced(function* (workspace: Workspace, plugin: string, extension: string) {
    const building = new Set(
        inferred(workspace, [plugin])
            .filter(({ name }) => name.startsWith('build--'))
            .map(({ project }) => project),
    );
    const roots = createProjectRootMappings(workspace.nodes);
    const owned = (yield* tracked(`*${extension}`)).flatMap((file) => Array.fromNullishOr(findProjectForPath(file, roots)).map((project) => ({ project, file })));
    return yield* Effect.fromOption(Array.findFirst(owned, ({ project }) => building.has(project)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect.each(READ)(
        'reaches every task of $plugins with a change to $file',
        Effect.fnUntraced(function* ({ file, plugins, source }) {
            const workspace = yield* read('fresh');
            const found = yield* reached(named(workspace), file);
            const tasks = new Set(inferred(workspace, plugins).map(({ task }) => task));
            assert.isNotEmpty([...tasks], source);
            assert.deepStrictEqual([...tasks.difference(found)], [], source);
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

    suite.effect.each(BOUND)(
        'reaches with a change to $file no cached task outside the projects of $plugins and the projects that depend on them',
        Effect.fnUntraced(function* ({ file, plugins }) {
            const workspace = yield* read('fresh');
            const found = yield* reached(named(workspace), file);
            const allowed = dependents(workspace, new Set(inferred(workspace, plugins).map(({ project }) => project)));
            const beyond = bounded(workspace).filter(({ project, task }) => !allowed.has(project) && found.has(task));
            assert.deepStrictEqual(
                beyond.map(({ task }) => task),
                [],
                SOURCES.none,
            );
        }),
    );

    suite.effect.each([
        { file: 'README.md', source: SOURCES.none },
        { file: '.config/mise.lock', source: SOURCES.read },
    ])(
        'reaches no cached task with a change to $file',
        Effect.fnUntraced(function* ({ file, source }) {
            const workspace = yield* read('fresh');
            const found = yield* reached(named(workspace), file);
            const cached = bounded(workspace).filter(({ task }) => found.has(task));
            assert.deepStrictEqual(
                cached.map(({ task }) => task),
                [],
                source,
            );
        }),
    );

    suite.effect.each(BUILT.filter(({ plugin }) => hosted(plugin)))(
        'selects for build the project of $plugin that owns the first tracked $extension file with a change to that file',
        Effect.fnUntraced(function* ({ plugin, extension }) {
            const { project, file } = yield* sampled(yield* read('fresh'), plugin, extension);
            assert.isTrue((yield* reached(['build'], file)).has(`${project}:build`), `${SOURCES.aggregate}: ${project}:build for ${file}`);
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
            const marked = dependents(workspace, new Set([changed.name, ...whole(workspace).map(({ project }) => project)]));
            assert.deepStrictEqual(Array.sort(yield* Effect.flatMap(checked, Schema.decodeEffect(Listed)), Order.String), Array.sort([...marked], Order.String), `${SOURCES.own}; ${SOURCES.projects}`);
        }),
    );
});
