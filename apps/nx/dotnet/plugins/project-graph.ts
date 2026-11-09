import { findPackageJSON } from 'node:module';
import { posix } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { aggregated, configured, type Options, targets } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = typeof Node.Type;
type Report = typeof Report.Type;
type Tracking = NonNullable<Node['releases']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const FILES = `**/{*.{${PROJECTS.map((extension) => extension.slice(1)).join(',')},props,targets,globalconfig},global.json,Directory.Build.rsp,nuget.config,NuGet.config,NuGet.Config,.editorconfig}`;

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
    restored: Schema.String,
    results: Schema.Array(Schema.String),
    releases: Schema.NullOr(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })),
});
const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));

// --- [ERRORS] --------------------------------------------------------------------------

class DotnetError extends Schema.TaggedError<DotnetError, { readonly brand: unique symbol }>()('DotnetError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (args: readonly string[], cwd: string, input?: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make('dotnet', args, { cwd, extendEnv: true, stdin: input === undefined ? 'ignore' : Stream.encodeText(Stream.succeed(input)) });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* DotnetError.make({ subject: ['dotnet', ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
        }),
    );

const analyzer = Effect.flatMap(Path.Path, (path) => path.fromFileUrl(new URL(import.meta.resolve('#analyzer'))));

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const core = yield* analyzer;
    return yield* Effect.flatMap(spawned([core, workspaceRoot], workspaceRoot, files.join('\n')), (stdout) => Schema.decodeEffect(Schema.fromJsonString(Report))(stdout).pipe(Effect.mapError((error) => DotnetError.make({ subject: core, detail: error.message }))));
});

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const stamped = (fs: FileSystem.FileSystem, files: readonly string[]) =>
    Effect.map(
        Effect.forEach(files, (file) =>
            Effect.map(Effect.option(fs.stat(file)), (held) =>
                Option.match(held, {
                    onNone: () => `${file}:`,
                    onSome: ({ mtime, size }) =>
                        `${file}:${Option.getOrElse(
                            Option.map(mtime, (at) => at.valueOf()),
                            () => 0,
                        )}:${size}`,
                }),
            ),
        ),
        (stamps) => stamps.join(','),
    );

const reports = new Map<string, Report>();

const evaluate = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, files: readonly string[], located: string, name: string) {
    const [path, fs, core] = yield* Effect.all([Path.Path, FileSystem.FileSystem, analyzer]);
    const folder = path.dirname(core);
    const modules = [located, ...(yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module))];
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...files])),
        ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))),
        yield* stamped(
            fs,
            (yield* fs.readDirectory(folder)).toSorted().map((entry) => path.join(folder, entry)),
        ),
        hashObject(options),
        ...files,
    ]);
    const restoredStamp = ({ nodes }: Report) =>
        stamped(
            fs,
            nodes.map(({ restored: at }) => path.join(workspaceRoot, at)),
        );
    const evaluatedHash = ({ evaluated }: Report) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...evaluated]));
    const cache = new PluginCache<{ readonly report: Report; readonly evaluated: string; readonly restored: string }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options)}.hash`));
    const held = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onNone: () => Effect.succeedNone,
        onSome: (entry) => Effect.map(Effect.all([evaluatedHash(entry.report), restoredStamp(entry.report)]), ([inputs, stamps]) => (inputs === entry.evaluated && stamps === entry.restored ? Option.some(entry.report) : Option.none())),
    });
    const report = Option.isSome(held) ? held.value : yield* analyzed(workspaceRoot, files);
    yield* Option.isSome(held) || report.failures.length > 0
        ? Effect.void
        : Effect.flatMap(Effect.all([evaluatedHash(report), restoredStamp(report)]), ([inputs, stamps]) =>
              Effect.sync(() => {
                  cache.set(hash, { report, evaluated: inputs, restored: stamps });
                  cache.writeToDisk();
              }),
          );
    return report;
});

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, matched: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const files = matched.toSorted();
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const report = files.some((file) => PROJECTS.includes(path.extname(file))) ? yield* evaluate(workspaceRoot, options, files, located, name) : { nodes: [], failures: [], evaluated: [] };
    const tracked = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.relative(workspaceRoot, located)]));
    reports.set(hashArray([workspaceRoot, hashObject(options)]), report);
    return { report, plugin: { name, inputs: tracked.length === 0 ? [{ externalDependencies: [name] }] : [{ input: 'default', projects: [name] }, { externalDependencies: [] }] } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<Options> = [
    FILES,
    (files, given, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const options = given ?? {};
                const { report, plugin } = yield* Effect.mapError(graph(context.workspaceRoot, options, files), (error) => new AggregateCreateNodesError([[null, new Error(error.message)]], []));
                const releasable = (node: Node) => node.packable || node.publishable;
                const folders = Record.fromIterableWith(report.nodes, (node) => [path.dirname(node.file), node] as const);
                const carried = (node: Node): readonly Tracking[] => [
                    ...Option.toArray(Option.fromNullOr(node.releases)),
                    ...node.references
                        .flatMap(({ root }) => Option.toArray(Record.get(folders, root)))
                        .filter((held) => !releasable(held))
                        .flatMap(carried),
                ];
                const nodes: CreateNodesResultArray = report.nodes.map((node) => {
                    const root = path.dirname(node.file);
                    return [
                        node.file,
                        {
                            projects: {
                                [root]: {
                                    name: node.name,
                                    metadata: { technologies: ['dotnet', node.language] },
                                    targets: aggregated(configured(targets(plugin, path, node, 'production' in getNamedInputs(root, context) ? 'production' : 'default'), options), ['dotnet', node.language]),
                                    ...(releasable(node)
                                        ? { tags: ['release'], release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { projectFile: path.basename(node.file), releases: Array.dedupeWith(carried(node), (left, right) => left.unshipped === right.unshipped) } } } }
                                        : {}),
                                },
                            },
                            externalNodes: Record.fromIterableWith(node.packages, (held): readonly [string, ProjectGraphExternalNode] => [external(held), { type: 'nuget', name: external(held), data: { version: held.version, packageName: held.id, hash: held.hash } }]),
                        },
                    ] as const;
                });
                return report.failures.length === 0
                    ? nodes
                    : yield* Effect.fail(
                          new AggregateCreateNodesError(
                              report.failures.map(({ file, message }) => [file, new Error(message)]),
                              nodes,
                          ),
                      );
            }).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies<Options> = (options, { workspaceRoot, projects }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const owned = (root: string) => Option.fromUndefinedOr(names.get(root));
    const referenced = (node: Node) =>
        node.references.flatMap((reference): readonly RawProjectGraphDependency[] =>
            Option.toArray(Option.all([owned(posix.dirname(node.file)), owned(reference.root)])).map(([source, target]) => (reference.declared === node.file ? { source, target, type: DependencyType.static, sourceFile: node.file } : { source, target, type: DependencyType.implicit })),
        );
    const declared = (node: Node, source: string) => node.packages.filter(({ direct }) => direct).map((held): RawProjectGraphDependency => ({ source, target: external(held), type: DependencyType.implicit }));
    const locked = (node: Node) => {
        const pinned = Record.fromIterableWith(node.packages, (held) => [held.id, held] as const);
        const resolved = (held: Node['packages'][number]) => held.dependencies.flatMap((dependency) => Option.toArray(Record.get(pinned, dependency)));
        return node.packages.flatMap((held) => resolved(held).map((target): RawProjectGraphDependency => ({ source: external(held), target: external(target), type: DependencyType.static })));
    };
    return (reports.get(hashArray([workspaceRoot, hashObject(options ?? {})]))?.nodes ?? []).flatMap((node) => [...referenced(node), ...Option.toArray(owned(posix.dirname(node.file))).flatMap((source) => declared(node, source)), ...locked(node)]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { analyzer, createDependencies, createNodes, DotnetError, type Node, spawned };
