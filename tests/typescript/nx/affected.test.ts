import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Order, Path, Schema } from 'effect';
import { declared, dependents, nx, read, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    crate: 'README.md PRINCIPLES: `@rasm/nx-cargo` infers the `default` named input of a crate (its files, the workspace lock, manifest, toolchain, and Cargo variables), so every target of the crate reads them',
    lock: 'README.md PRINCIPLES: Cargo, uv, SwiftPM, and xcodebuild targets read the committed lock (`--locked`, `--force-resolved-versions`, `-onlyUsePackageVersionsFromResolvedFile`)',
    externals: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects every task hashing all externals when the lockfile changes',
    read: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source, `.config/mise.lock`, tool `--version` output, and credentials are no input",
    versions: 'README.md OWNERS: Package version — `pnpm-workspace.yaml` catalog, `pyproject.toml` `[project]` or group, `Directory.Packages.props` row',
    options: 'README.md LAYOUT: tsconfig.base.json — TypeScript options every package compiles and emits with',
    own: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects a task whose own files changed, and its dependents',
    none: 'nrwl/nx packages/nx/src/project-graph/affected/affected-tasks.spec.ts computeAffectedTasks: selects nothing when the change reaches no input',
    projects: 'nrwl/nx e2e/nx/src/affected-graph.test.ts show projects --affected: should print information about affected projects',
} as const;
const REACH = [
    { file: 'Cargo.lock', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'Cargo.toml', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'rust-toolchain.toml', plugins: ['@rasm/nx-cargo'], source: SOURCES.crate },
    { file: 'uv.lock', plugins: ['@rasm/nx-uv'], source: SOURCES.lock },
    { file: 'pnpm-lock.yaml', plugins: ['@rasm/nx-typescript', '@rasm/nx-vite', '@rasm/nx-vitest', '@rasm/nx-storybook'], source: SOURCES.externals },
    { file: 'Directory.Packages.props', plugins: ['@rasm/nx-dotnet'], source: SOURCES.versions },
    { file: 'tsconfig.base.json', plugins: ['@rasm/nx-typescript'], source: SOURCES.options },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Explained = Schema.fromJsonString(Schema.Struct({ touched: Schema.Array(Schema.String) })).annotate({ description: 'JSON nx affected --explain=stdout prints: the ids of the tasks a change reaches through their own inputs or the outputs they read' });
const Listed = Schema.fromJsonString(Schema.Array(Schema.String)).annotate({ description: 'JSON nx show projects --json prints: project names' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const touched = Effect.fnUntraced(function* (workspace: Workspace, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    assert.isTrue(yield* fs.exists(path.join(workspaceRoot, file)), `${file} exists`);
    const { checked } = yield* nx(['affected', `--targets=${Array.dedupe(declared(workspace).map(({ name }) => name)).join(',')}`, `--files=${file}`, '--explain=stdout']);
    return new Set((yield* Effect.flatMap(checked, Schema.decodeEffect(Explained))).touched);
});

const inferred = (workspace: Workspace, plugins: readonly string[]) => declared(workspace).filter(({ plugin }) => Option.exists(plugin, (owner) => plugins.includes(owner)));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('affected reach', (suite) => {
    suite.effect.each(REACH)(
        'reaches every task of $plugins with a change to $file',
        Effect.fnUntraced(function* ({ file, plugins, source }) {
            const workspace = yield* read('fresh');
            const reached = yield* touched(workspace, file);
            const tasks = new Set(inferred(workspace, plugins).map(({ task }) => task));
            assert.isNotEmpty([...tasks], source);
            assert.deepStrictEqual([...tasks.difference(reached)], [], source);
        }),
    );

    suite.effect.each(REACH)(
        'reaches with a change to $file no cached task outside the projects of $plugins and the projects that depend on them',
        Effect.fnUntraced(function* ({ file, plugins }) {
            const workspace = yield* read('fresh');
            const reached = yield* touched(workspace, file);
            const allowed = dependents(workspace, new Set(inferred(workspace, plugins).map(({ project }) => project)));
            const beyond = declared(workspace).filter(({ project, task, target }) => target.cache === true && !allowed.has(project) && reached.has(task));
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
            const reached = yield* touched(workspace, file);
            const cached = declared(workspace).filter(({ task, target }) => target.cache === true && reached.has(task));
            assert.deepStrictEqual(
                cached.map(({ task }) => task),
                [],
                source,
            );
        }),
    );

    suite.effect(
        'marks a project and the projects that depend on it, and no other, for a change to one of its files',
        Effect.fnUntraced(function* () {
            const [fs, path, workspace] = yield* Effect.all([FileSystem.FileSystem, Path.Path, read('fresh')]);
            const depended = Object.values(workspace.nodes).filter(({ name, data }) => data.root !== '.' && dependents(workspace, new Set([name])).size > 1);
            const sampled = yield* Effect.findFirst(
                Array.sort(
                    depended,
                    Order.mapInput(Order.String, ({ name }: { readonly name: string }) => name),
                ),
                ({ data }) => fs.exists(path.join(workspaceRoot, data.root, 'index.ts')),
            );
            const changed = yield* Effect.fromOption(sampled);
            const { checked } = yield* nx(['show', 'projects', '--affected', `--files=${path.join(changed.data.root, 'index.ts')}`, '--json']);
            assert.deepStrictEqual(Array.sort(yield* Effect.flatMap(checked, Schema.decodeEffect(Listed)), Order.String), Array.sort([...dependents(workspace, new Set([changed.name]))], Order.String), `${SOURCES.own}; ${SOURCES.projects}`);
        }),
    );
});
