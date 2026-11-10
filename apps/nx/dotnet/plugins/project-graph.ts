import { posix } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, hashFile, hashWithWorkspaceContext } from '@nx/devkit/internal';
import { aggregated, cached, identity, inferred, reported } from '@rasm/nx-devkit/inference';
import { printed } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, FileSystem, Option, Path, Record, Schema, Struct } from 'effect';
import { analyzer, Node, targets, upgraded } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const FILES = `**/{*.{${PROJECTS.map((extension) => extension.slice(1)).join(',')},props,targets,globalconfig},global.json,Directory.Build.rsp,nuget.config,NuGet.config,NuGet.Config,.editorconfig}`;
const PLATFORMS = { macOS: 'darwin', Windows: 'win32', Linux: 'linux' } as const satisfies Readonly<Record<string, NodeJS.Platform>>;

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);

// --- [OPERATIONS] ----------------------------------------------------------------------

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

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

const reports = new Map<string, { readonly report: Report; readonly plugin: { readonly root: string; readonly core: string } }>();

const evaluate = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[], plugin: Effect.Success<ReturnType<typeof identity>>, options: unknown) {
    const [path, fs, core] = yield* Effect.all([Path.Path, FileSystem.FileSystem, analyzer]);
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...files])), plugin.digest, yield* hashed(fs, path, [path.dirname(core)]), ...files]);
    const digest = ({ evaluated, nodes }: Report) =>
        Effect.map(
            Effect.all([
                Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...evaluated])),
                hashed(
                    fs,
                    path,
                    nodes.flatMap(({ restored }) => restored.map((folder) => path.join(workspaceRoot, folder))),
                ),
            ]),
            (stamps) => hashArray([hash, ...stamps]),
        );
    const cache = yield* cached<Report>(plugin, options);
    const stored = (fresh: Report) => (fresh.failures.length > 0 ? Effect.void : Effect.flatMap(digest(fresh), (now) => cache.set(FILES, fresh, now)));
    const analyzed = Effect.flatMap(printed('dotnet', [core, workspaceRoot], { cwd: workspaceRoot, input: files.join('\n') }), Schema.decodeEffect(Reported)).pipe(Effect.tap(stored));
    return yield* Effect.fromOption(yield* cache.get(FILES, digest)).pipe(Effect.catch(() => analyzed));
});

const graph = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[], options: unknown) {
    const [path, core, plugin] = yield* Effect.all([Path.Path, analyzer, identity(import.meta.url, workspaceRoot)]);
    const files = matched.toSorted();
    const report = files.some((file) => PROJECTS.includes(path.extname(file))) ? yield* evaluate(workspaceRoot, files, plugin, options) : { nodes: [], failures: [], evaluated: [] };
    reports.set(workspaceRoot, { report, plugin: { root: path.relative(workspaceRoot, plugin.folder), core: path.relative(workspaceRoot, core) } });
    return { report, plugin };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) =>
        inferred(
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const { report, plugin } = yield* graph(context.workspaceRoot, matched, options);
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
                                    targets: targets(plugin, path, node, 'production' in getNamedInputs(root, context) ? 'production' : 'default'),
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
                return yield* reported(
                    [...nodes, ...centrals],
                    report.failures.map(({ file, message }) => [file, new Error(message)] as const),
                );
            }),
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
        return { source: yield* owned(plugin.root), target: yield* owned(posix.dirname(built.file)), type: DependencyType.static, sourceFile: posix.join(plugin.root, 'package.json') } satisfies RawProjectGraphDependency;
    });
    return [...nodes.flatMap((node) => [...referenced(node), ...Option.toArray(owned(posix.dirname(node.file))).flatMap((source) => declared(node, source)), ...locked(node)]), ...Option.toArray(core)];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
