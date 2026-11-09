import { findPackageJSON } from 'node:module';
import { posix } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashFile, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, Equal, Equivalence, FileSystem, Formatter, flow, Option, Path, Record, Schema, Stream, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { aggregated, targets, upgraded } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;
type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const FILES = `**/{*.{${PROJECTS.map((extension) => extension.slice(1)).join(',')},props,targets,globalconfig},global.json,Directory.Build.rsp,nuget.config,NuGet.config,NuGet.Config,.editorconfig}`;
const PLATFORMS = { macOS: 'darwin', Windows: 'win32', Linux: 'linux' } as const satisfies Readonly<Record<string, NodeJS.Platform>>;

// --- [MODELS] --------------------------------------------------------------------------

const Node = Schema.Struct({
    file: Schema.String,
    name: Schema.String,
    language: Schema.String,
    test: Schema.Boolean,
    packable: Schema.Boolean,
    publishable: Schema.Boolean,
    configuration: Schema.String,
    modes: Schema.Array(Schema.Struct({ configuration: Schema.String, runtime: Schema.NullOr(Schema.String), outputs: Schema.Array(Schema.String), published: Schema.Array(Schema.String) })),
    references: Schema.Array(Schema.Struct({ root: Schema.String, declared: Schema.String })),
    inputs: Schema.Array(Schema.String),
    bases: Schema.Array(Schema.String),
    packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, hash: Schema.String, direct: Schema.Boolean, dependencies: Schema.Array(Schema.String) })),
    locks: Schema.Array(Schema.String),
    restored: Schema.Array(Schema.String),
    results: Schema.Array(Schema.String),
    platforms: Schema.Array(Schema.String),
    central: Schema.NullOr(Schema.String),
    releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
});
const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));

// --- [ERRORS] --------------------------------------------------------------------------

class DotnetError extends Schema.TaggedError<DotnetError, { readonly brand: unique symbol }>()('DotnetError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const dotnet = Effect.fnUntraced(function* (args: readonly string[], cwd: string, input?: string) {
    const handle = yield* ChildProcess.make('dotnet', args, { cwd, extendEnv: true, stdin: input === undefined ? 'ignore' : Stream.encodeText(Stream.succeed(input)) });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return { code, stdout, stderr };
}, Effect.scoped);

const spawned = Effect.fnUntraced(function* (args: readonly string[], cwd: string, input?: string) {
    const run = yield* dotnet(args, cwd, input);
    return run.code === 0 ? run.stdout : yield* DotnetError.make({ subject: ['dotnet', ...args].join(' '), detail: `exited ${run.code}: ${run.stderr.trim()}` });
});

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const analyzer = Effect.flatMap(Path.Path, (path) => path.fromFileUrl(new URL(import.meta.resolve('#analyzer'))));

const hashed = (fs: FileSystem.FileSystem, path: Path.Path, folders: readonly string[]) =>
    Effect.map(
        Effect.forEach(folders, (folder) =>
            Effect.map(
                Effect.orElseSucceed(fs.readDirectory(folder), () => []),
                (entries) => entries.toSorted().map((entry) => path.join(folder, entry)),
            ),
        ),
        (files) => hashArray(files.flat().flatMap((file) => [file, hashFile(file)])),
    );

const reports = new Map<string, { readonly report: Report; readonly plugin: { readonly root: string; readonly manifest: string; readonly core: string } | undefined }>();

const evaluate = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[], located: string, name: string) {
    const [path, fs, core] = yield* Effect.all([Path.Path, FileSystem.FileSystem, analyzer]);
    const modules = [located, ...(yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module))];
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...files])), ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))), yield* hashed(fs, path, [path.dirname(core)]), ...files]);
    const revalidated = ({ evaluated, nodes }: Report) =>
        Effect.all([
            Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...evaluated])),
            hashed(
                fs,
                path,
                nodes.flatMap(({ restored }) => restored.map((folder) => path.join(workspaceRoot, folder))),
            ),
        ]);
    const cache = new PluginCache<{ readonly report: Report; readonly revalidated: readonly [string, string] }>(path.join(workspaceDataDirectory, `${path.basename(name)}.hash`));
    const cached = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onNone: () => Effect.succeedNone,
        onSome: (entry) => Effect.map(revalidated(entry.report), (stamps) => (Equal.equals(stamps, entry.revalidated) ? Option.some(entry.report) : Option.none())),
    });
    const stored = (fresh: Report): Effect.Effect<void> =>
        fresh.failures.length > 0
            ? Effect.void
            : Effect.flatMap(revalidated(fresh), (stamps) =>
                  Effect.sync(() => {
                      cache.set(hash, { report: fresh, revalidated: stamps });
                      cache.writeToDisk();
                  }),
              );
    const analyzed = Effect.flatMap(spawned([core, workspaceRoot], workspaceRoot, files.join('\n')), Schema.decodeEffect(Reported)).pipe(Effect.tap(stored));
    return yield* Effect.fromOption(cached).pipe(Effect.catch(() => analyzed));
});

const graph = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const [path, fs, core] = yield* Effect.all([Path.Path, FileSystem.FileSystem, analyzer]);
    const files = matched.toSorted();
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const report = files.some((file) => PROJECTS.includes(path.extname(file))) ? yield* evaluate(workspaceRoot, files, located, name) : { nodes: [], failures: [], evaluated: [] };
    const manifest = path.relative(workspaceRoot, located);
    const tracked = (yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [manifest]))).length > 0;
    reports.set(workspaceRoot, { report, plugin: tracked ? { root: path.dirname(manifest), manifest, core: path.relative(workspaceRoot, core) } : undefined });
    return { report, plugin: { name, inputs: tracked ? [{ input: 'default', projects: [name] }, { externalDependencies: [] }] : [{ externalDependencies: [name] }] } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    flow(
        Effect.fnUntraced(function* (files: readonly string[], _: unknown, context: CreateNodesContext) {
            const path = yield* Path.Path;
            const { report, plugin } = yield* Effect.mapError(graph(context.workspaceRoot, files), (error) => new AggregateCreateNodesError([[null, error]], []));
            const releasable = (node: Node) => node.packable || node.publishable;
            const folders = Record.fromIterableWith(report.nodes, (node) => [path.dirname(node.file), node] as const);
            const carried = (node: Node): readonly NonNullable<Node['releases']>[] => [
                ...Option.toArray(Option.fromNullOr(node.releases)),
                ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root)))
                    .filter((reference) => !releasable(reference))
                    .flatMap(carried),
            ];
            const nodes: CreateNodesResultArray = report.nodes.map((node) => {
                const root = path.dirname(node.file);
                const supported = Array.intersection(Record.keys(PLATFORMS), node.platforms);
                const hosts = supported.length === Record.keys(PLATFORMS).length ? [] : supported.map((platform) => `os:${PLATFORMS[platform]}`);
                return [
                    node.file,
                    {
                        projects: {
                            [root]: {
                                name: node.name,
                                metadata: { technologies: ['dotnet', node.language] },
                                targets: aggregated(targets(plugin, path, node, 'production' in getNamedInputs(root, context) ? 'production' : 'default'), ['dotnet', node.language]),
                                tags: [...hosts, ...(releasable(node) ? ['release'] : [])],
                                ...(releasable(node)
                                    ? { release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { projectFile: path.basename(node.file), releases: Array.dedupeWith(carried(node), Equivalence.mapInput(Equivalence.String, Struct.get('unshipped'))) } } } }
                                    : {}),
                            },
                        },
                        externalNodes: Record.fromIterableWith(node.packages, (reference): readonly [string, ProjectGraphExternalNode] => [external(reference), { type: 'nuget', name: external(reference), data: { version: reference.version, packageName: reference.id, hash: reference.hash } }]),
                    },
                ] as const;
            });
            const centrals = Record.collect(
                Array.groupBy(Array.getSomes(report.nodes.map((node) => Option.map(Option.fromNullOr(node.central), (central) => ({ central, node })))), Struct.get('central')),
                (central, consumers) => [central, { projects: { [path.dirname(central)]: { targets: aggregated(upgraded(plugin, path, central, consumers.map(Struct.get('node'))), ['dotnet']) } } }] as const,
            );
            return report.failures.length === 0
                ? [...nodes, ...centrals]
                : yield* Effect.fail(
                      new AggregateCreateNodesError(
                          report.failures.map(({ file, message }) => [file, new Error(message)]),
                          [...nodes, ...centrals],
                      ),
                  );
        }, Effect.provide(layer)),
        Effect.runPromise,
    ),
];

const createDependencies: CreateDependencies = (_, { workspaceRoot, projects }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const owned = (root: string) => Option.fromUndefinedOr(names.get(root));
    const entry = reports.get(workspaceRoot);
    const nodes = entry?.report.nodes ?? [];
    const referenced = (node: Node) =>
        Array.getSomes(
            node.references.map((reference) =>
                Option.map(Option.all([owned(posix.dirname(node.file)), owned(reference.root)]), ([source, target]): RawProjectGraphDependency => (reference.declared === node.file ? { source, target, type: DependencyType.static, sourceFile: node.file } : { source, target, type: DependencyType.implicit })),
            ),
        );
    const declared = (node: Node, source: string) => node.packages.filter(({ direct }) => direct).map((reference): RawProjectGraphDependency => ({ source, target: external(reference), type: DependencyType.implicit }));
    const locked = (node: Node) => {
        const pinned = Record.fromIterableWith(node.packages, (reference) => [reference.id, reference] as const);
        const resolved = (reference: Node['packages'][number]) => Array.getSomes(reference.dependencies.map((dependency) => Record.get(pinned, dependency)));
        return node.packages.flatMap((reference) => resolved(reference).map((target) => ({ source: external(reference), target: external(target), type: DependencyType.static }) satisfies RawProjectGraphDependency));
    };
    const core = Option.gen(function* () {
        const plugin = yield* Option.fromUndefinedOr(entry?.plugin);
        const built = yield* Array.findFirst(nodes, ({ modes }) => modes.flatMap(Struct.get('outputs')).some((output) => plugin.core.startsWith(`${output}/`)));
        return { source: yield* owned(plugin.root), target: yield* owned(posix.dirname(built.file)), type: DependencyType.static, sourceFile: plugin.manifest } satisfies RawProjectGraphDependency;
    });
    return [...nodes.flatMap((node) => [...referenced(node), ...Option.toArray(owned(posix.dirname(node.file))).flatMap((source) => declared(node, source)), ...locked(node)]), ...Option.toArray(core)];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { analyzer, createDependencies, createNodes, DotnetError, dotnet, type Node, spawned };
