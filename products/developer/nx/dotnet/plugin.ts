import { hash } from 'node:crypto';
import { basename, dirname, extname, format, join, parse, posix, relative } from 'node:path';
import { env, platform } from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResult, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, emitPluginWorkerLog, globWithWorkspaceContext, hashFile, hashObject, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { type Anchor, aggregated, anchored, EXECUTORS, fingerprinted, inferring, manifested, Named, type Stated, spawned, stamped } from '@rasm/nx';
import { Array, Cause, Config, Duration, Effect, Equal, Equivalence, FileSystem, Function, Graph, Option, Record, Ref, Result, Schema, Stream, String, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import which from 'which';
import plugin from '#/package.json' with { type: 'json' };
import { ANALYZER, Edges, ENVIRONMENT, line, Node, publishing, run, targets } from '#/targets';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const SOLUTIONS = ['.slnx', '.sln'];
const FILES = `**/*{${[...PROJECTS, ...SOLUTIONS].join(',')}}`;
const SIDES = combineGlobPatterns(['*{.props,.targets,.globalconfig}', 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'].map((file) => `**/${file}`));
const PLATFORMS: Readonly<Record<string, NodeJS.Platform>> = { windows: 'win32', macos: 'darwin', maccatalyst: 'darwin', ios: 'darwin', tvos: 'darwin' };
const SOURCE = format({ dir: dirname(dirname(ANALYZER)), name: parse(ANALYZER).name, ext: '.csproj' });
const SUMS = format({ dir: dirname(ANALYZER), name: parse(ANALYZER).name, ext: '.sha256' });

// --- [MODELS] --------------------------------------------------------------------------

class Report extends Schema.Opaque<Report, unknown>()(Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String), probed: Schema.Array(Schema.String), variables: Schema.Array(Schema.String) })) {
    static readonly empty: Report = { nodes: [], failures: [], evaluated: [], probed: [], variables: [] };
}
const Reported = Schema.fromJsonString(Report);
class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({
        coveragePath: Schema.optionalKey(Schema.String),
    }).annotate({
        description: 'coveragePath: workspace folder the coverage configuration of test--dotnet passes to dotnet test as --results-directory, where coverlet.MTP writes its lcov report coverage.<timestamp>.info, {projectName} naming the project, absent leaves the report in the test results folder of the project',
    }),
) {}
const Timeout = Config.schema(Schema.Finite.check(Schema.isGreaterThan(0)), 'NX_DOTNET_PROJECT_GRAPH_TIMEOUT').pipe(Config.withDefault(isCI() === false ? 120 : 600));

// --- [OPERATIONS] ----------------------------------------------------------------------

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const own = fingerprinted(import.meta.dirname);

const stamp = Effect.fnUntraced(function* (workspaceRoot: string, host: Option.Option<string>, { evaluated, nodes, probed, variables }: Report) {
    const fs = yield* FileSystem.FileSystem;
    const installed = Effect.fromOption(host).pipe(
        Effect.flatMap(fs.realPath),
        Effect.flatMap((real) => Effect.map(fs.readDirectory(join(dirname(real), 'sdk')), (versions) => [real, ...versions.toSorted()])),
        Effect.orElseSucceed(Array.empty<string>),
    );
    const [tracked, sdks, present] = yield* Effect.all([stamped(workspaceRoot)([...evaluated, ...nodes.flatMap(({ locks }) => locks)]), installed, Effect.forEach(probed, fs.exists, { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
    const files = Array.dedupe([ANALYZER, ...nodes.flatMap(({ restored }) => restored.map((at) => join(workspaceRoot, at))), ...nodes.flatMap(({ external: held }) => held)]);
    const set = Record.fromEntries(Object.entries(env).map(([name, value]) => [name.toLowerCase(), value] as const));
    return hashArray([tracked, ...sdks, ...files.flatMap((file) => [relative(workspaceRoot, file), ...Array.fromNullishOr(hashFile(file))]), ...Array.zipWith(probed, present, (at, exists) => `${at} ${exists}`), ...variables.map((name) => `${name}=${set[name.toLowerCase()] ?? ''}`)]);
});

const published = (workspaceRoot: string) => `${line(['dotnet', 'publish', relative(workspaceRoot, SOURCE), '--output', relative(workspaceRoot, dirname(ANALYZER))])} builds it`;

const ready = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const [host, present, sourced, sums] = yield* Effect.all([Effect.promise(() => which('dotnet', { nothrow: true })), fs.exists(ANALYZER), fs.exists(SOURCE), Effect.option(fs.readFileString(SUMS))], { concurrency: 'unbounded' });
    const resolved = Result.fromNullishOr(host, () => 'no folder of PATH holds dotnet').pipe(Result.filterOrFail(Function.constant(present), () => `${relative(workspaceRoot, ANALYZER)} is absent, ${published(workspaceRoot)}`));
    yield* Result.getFailure(resolved).pipe(
        Option.filter(() => matched.some((file) => PROJECTS.includes(extname(file)))),
        Option.map((reason) => Effect.sync(() => emitPluginWorkerLog('warn', `No MSBuild project of the workspace enters the graph, as ${reason}`))),
        Effect.transposeOption,
    );
    yield* Effect.fromOption(sums).pipe(
        Effect.map((text) => Array.map(text.split('\n').filter(String.isNonEmpty), (row) => [join(dirname(SOURCE), row.slice(row.indexOf(' ')).trim()), row.slice(0, row.indexOf(' ')).toLowerCase()] as const)),
        Effect.flatMap((rows) => Effect.filter(rows, ([file, digest]) => Effect.match(fs.readFile(file), { onFailure: () => true, onSuccess: (bytes) => hash('sha256', bytes) !== digest }), { concurrency: 'unbounded' })),
        Effect.map(Array.map(([file]) => file)),
        Effect.orElseSucceed(() => [SUMS]),
        Effect.filterOrFail(Array.isArrayEmpty, (files) => new Cause.UnknownError(files, `${relative(workspaceRoot, ANALYZER)} predates ${files.map((file) => relative(workspaceRoot, file)).join(', ')}, ${published(workspaceRoot)}`)),
        Effect.when(Effect.succeed(Result.isSuccess(resolved) && sourced)),
    );
    return Result.getSuccess(resolved);
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const argv = ['dotnet', ANALYZER, workspaceRoot] as const;
    const seconds = yield* Timeout;
    const report = yield* spawned(argv, { cwd: workspaceRoot, env: ENVIRONMENT, stdin: Stream.encodeText(Stream.make(matched.join('\n'))) }).pipe(
        Effect.timeoutOrElse({ duration: Duration.seconds(seconds), orElse: () => new Cause.UnknownError(seconds, `${CommandLine.display(argv)} ran past ${seconds} seconds, set NX_DOTNET_PROJECT_GRAPH_TIMEOUT to a higher count of seconds`) }),
        Effect.flatMap(Schema.decodeEffect(Reported)),
        Effect.catchTag('SchemaError', (error) => new Cause.UnknownError(error, `${CommandLine.display(argv)} printed a report this plugin cannot read, as ${relative(workspaceRoot, ANALYZER)} predates the plugin, ${published(workspaceRoot)}: ${error.message}`)),
    );
    const known = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(report.nodes.flatMap(({ inputs, loaded }) => [...inputs, ...loaded])))));
    return { ...report, nodes: report.nodes.map((node) => ({ ...node, inputs: node.inputs.filter((input) => known.has(input)), loaded: node.loaded.filter((input) => known.has(input)) })) };
});

const related = ({ nodes }: Report): readonly RawProjectGraphDependency[] =>
    nodes.flatMap(({ file, references, packages }) => {
        const pinned = Record.fromIterableWith(packages, (reference) => [reference.id, reference] as const);
        return [
            ...references.map((reference): RawProjectGraphDependency => ({ source: dirname(file), target: reference.root, ...(reference.declared === file ? { type: DependencyType.static, sourceFile: file } : { type: DependencyType.implicit }) })),
            ...packages.filter(({ direct }) => direct).map((reference): RawProjectGraphDependency => ({ source: dirname(file), target: external(reference), type: DependencyType.implicit })),
            ...packages.flatMap((reference) => Array.getSomes(reference.dependencies.map((dependency) => Record.get(pinned, dependency))).map((target): RawProjectGraphDependency => ({ source: external(reference), target: external(target), type: DependencyType.static }))),
        ];
    });

const releasable = (node: Node) => node.versioned && (node.packable || node.publishable);

const listing = (node: Node) => join(workspaceDataDirectory, 'dotnet-external', `${hashArray([node.file])}.txt`);

const solution = (closure: readonly Node[]) => (configuration: string) => {
    const assigned = closure.map((each) => [each.file, [each.evaluated, ...each.declared].find((mode) => mode.configuration.toLowerCase() === configuration.toLowerCase())?.configuration ?? each.evaluated.configuration, each.platform] as const);
    return assigned.every(([, held]) => held === configuration) ? Option.none() : Option.some(Record.fromIterableWith(assigned, ([file, held, pair]) => [file, `${held}|${pair}`] as const));
};

const projected = (nodes: readonly Node[], { coveragePath }: Options, manifests: readonly Stated[], anchor: Anchor) => {
    const placed = Record.fromEntries(nodes.map((node, index) => [dirname(node.file), { index, released: releasable(node) }] as const));
    const referenced = Graph.fromSnapshot({
        type: 'directed',
        nodes: nodes.map((data, index) => ({ index, data })),
        edges: nodes.flatMap((node, source) => Array.getSomes(node.references.map(({ root }) => Record.get(placed, root))).map(({ index: target, released }) => ({ source, target, data: released }))).map((edge, index) => ({ ...edge, index })),
    });
    const unreleased = Graph.mutate(referenced, (mutable) => {
        Graph.filterEdges(mutable, (released) => !released);
    });
    const reached = (graph: typeof referenced, node: Node): readonly Node[] => Array.fromIterable(Graph.values(Graph.dfs(graph, { start: Option.toArray(Option.map(Record.get(placed, dirname(node.file)), Struct.get('index'))) })));
    const carried = (node: Node) => reached(unreleased, node).flatMap(({ releases }) => Array.fromNullishOr(releases));
    return Array.zip(nodes, manifests).map(([node, manifest]): [string, CreateNodesResult] => {
        const closure = reached(referenced, node);
        const mapped = Array.getSomes(node.platforms.map((identifier) => Record.get(PLATFORMS, identifier.toLowerCase())));
        const supported = Array.dedupe(mapped);
        const limited = node.test && mapped.length > 0 && mapped.length === node.platforms.length;
        const tags = [...(releasable(node) ? ['release'] : []), ...(limited ? supported.map((held) => `os:${held}`) : []), ...node.tags];
        const listed = Option.liftPredicate(relative(anchor.workspaceRoot, listing(node)), () => node.external.length > 0);
        const project = {
            root: dirname(node.file),
            name: node.name,
            metadata: { technologies: ['dotnet', node.language] },
            targets: aggregated(targets({ ...node, inputs: Array.dedupe(closure.flatMap(({ inputs }) => inputs)), test: limited ? supported.some((held) => held === platform) : node.test }, coveragePath, solution(closure), listed), manifest, 'dotnet', anchor, publishing(node)),
            ...(releasable(node) && { release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { manifest: basename(node.file), releases: Array.dedupeWith(carried(node), Equivalence.mapInput(Equivalence.String, Struct.get('unshipped'))) } } } }),
            ...(tags.length > 0 && { tags }),
            ...(node.implicit.length > 0 && { implicitDependencies: [...node.implicit] }),
        };
        const externalNodes = Record.fromIterableWith(node.packages, (reference): readonly [string, ProjectGraphExternalNode] => [external(reference), { type: 'nuget', name: external(reference), data: { version: reference.version, packageName: reference.id, hash: reference.hash } }]);
        return [node.file, { projects: { [project.root]: project }, externalNodes }];
    });
};

const inferred = Effect.fnUntraced(function* (matched: Array.NonEmptyReadonlyArray<string>, settings: Options, { workspaceRoot }: CreateNodesContext) {
    const [fs, edges, hashed] = yield* Effect.all([FileSystem.FileSystem, Edges, own]);
    const slot = join(workspaceDataDirectory, 'dotnet.hash');
    const key = hashArray([hashObject(settings), hashed, yield* stamped(workspaceRoot)([...matched, SIDES])]);
    const host = yield* ready(workspaceRoot, matched);
    const entries = Option.isSome(host) ? (new PluginCache<readonly { readonly report: Report; readonly stamp: string }[]>(slot).get(key) ?? []) : [];
    const cached = yield* Effect.findFirst(entries, (entry) => Effect.map(stamp(workspaceRoot, host, entry.report), Equal.equals(entry.stamp)));
    const fresh = Option.isNone(cached) && Option.isSome(host);
    const read = yield* Effect.result(Option.match(cached, { onSome: (stored) => Effect.succeed(stored.report), onNone: () => (fresh ? analyzed(workspaceRoot, matched) : Effect.succeed(Report.empty)) }));
    const report = Result.getOrElse(read, () => Report.empty);
    yield* fresh && Result.isSuccess(read) && report.failures.length === 0 ? Effect.flatMap(stamp(workspaceRoot, host, report), (held) => Effect.sync(() => new PluginCache(slot, { [key]: [{ report, stamp: held }, ...Array.take(entries, 1)] }).writeToDisk())) : Effect.void;
    yield* Ref.set(edges, related(report));
    yield* Effect.forEach(
        report.nodes.filter((node) => node.external.length > 0),
        (node) => Effect.flatMap(Effect.option(fs.readFileString(listing(node))), (held) => (Option.contains(held, node.external.join('\n')) ? Effect.void : Effect.andThen(fs.makeDirectory(dirname(listing(node)), { recursive: true }), fs.writeFileString(listing(node), node.external.join('\n'))))),
        { discard: true },
    );
    const declared = Array.dedupe(report.nodes.flatMap((node) => [node.file, ...node.inputs, ...node.catalogs, ...node.locks])).map((at) => posix.join('{workspaceRoot}', at));
    const manifests = yield* Effect.forEach(report.nodes, (node) => manifested(workspaceRoot, dirname(node.file)));
    const anchor = anchored(workspaceRoot);
    const solutions = yield* Effect.forEach(
        matched.filter((file) => SOLUTIONS.includes(extname(file))),
        (file) =>
            Effect.map(Effect.all([Effect.option(Effect.flatMap(fs.readFileString(join(workspaceRoot, dirname(file), 'package.json')), Schema.decodeEffect(Named))), manifested(workspaceRoot, dirname(file))]), ([named, manifest]): [string, CreateNodesResult] => {
                const moved = ['dotnet', relative(join(workspaceRoot, dirname(file)), ANALYZER), 'upgrade', basename(file)] as const;
                const cleared = ['dotnet', 'nuget', 'locals', 'global-packages', '--clear'] as const;
                const locked = ['dotnet', 'restore', basename(file), '--force-evaluate'] as const;
                const upgrade: TargetConfiguration = {
                    executor: EXECUTORS.upgrade,
                    options: { cwd: dirname(file), commands: [{ apply: moved, preview: [...moved, '--dry-run'], env: ENVIRONMENT }, { apply: cleared }, { apply: locked }] },
                    inputs: [posix.join('{workspaceRoot}', file), ...declared, { externalDependencies: [] }],
                    metadata: {
                        description: `Moves each central package version ${basename(file)} projects read to the newest release its version range admits and its consumers' frameworks support with ${line(moved)}, then empties the global packages folder with ${line(cleared)} so the restore fetches only what the new lock files name, and rewrites lock files with ${line(locked)}`,
                        help: { command: 'dotnet restore --help', example: { options: { dryRun: true } } },
                    },
                };
                const restored = ['dotnet', 'restore', basename(file), '--locked-mode'] as const;
                const restore: TargetConfiguration = {
                    executor: 'nx:run-commands',
                    options: { cwd: dirname(file), command: line(restored) },
                    cache: true,
                    inputs: [posix.join('{workspaceRoot}', file), ...declared, { runtime: 'dotnet --version' }, { externalDependencies: [] }],
                    metadata: {
                        description: `Fails where a lock file of ${basename(file)} disagrees with the central package versions, project files, or NuGet settings its projects read, with ${line(restored)}, as task hashes reach a version change through lock files alone`,
                        help: { command: 'dotnet restore --help', example: { args: ['--verbosity', 'detailed'] } },
                    },
                };
                const purged = ['dotnet', 'nuget', 'locals', 'all', '--clear'] as const;
                const clean: TargetConfiguration = {
                    executor: EXECUTORS.clean,
                    options: { cwd: dirname(file) },
                    configurations: { cache: { commands: [purged] } },
                    inputs: [{ externalDependencies: [] }],
                    metadata: { description: `Under configuration cache, empties every NuGet local folder ${basename(file)} reads, the global packages, HTTP cache, temp, and plugin cache, with ${line(purged)}`, help: { command: 'dotnet nuget locals --help', example: { args: [] } } },
                };
                return [
                    file,
                    {
                        projects: {
                            [dirname(file)]: {
                                root: dirname(file),
                                ...(Option.isSome(named) ? {} : { name: basename(file, extname(file)) }),
                                metadata: { technologies: ['dotnet'] },
                                targets: aggregated({ upgrade: { dotnet: upgrade }, check: { 'dotnet-restore': restore }, clean: { nuget: clean } }, manifest, 'dotnet', anchor),
                            },
                        },
                    },
                ];
            }),
    );
    const results = [...projected(report.nodes, settings, manifests, anchor), ...solutions];
    const failures = [...Option.toArray(Result.getFailure(read)).map((error): [null, Error] => [null, error]), ...report.failures.map(({ file, message }): [string, Error] => [file, new Cause.UnknownError(file, message)])];
    return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (matched, settings, context) {
        return yield* Array.match(matched, {
            onEmpty: () => Edges.use((edges) => Ref.set(edges, [])).pipe(Effect.as([])),
            onNonEmpty: (files) => inferred(files, settings, context),
        });
    }),
];

const createDependencies: CreateDependencies = (_, { projects, externalNodes = {} }) =>
    run(
        Effect.map(Edges.use(Ref.get), (edges) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const named = (at: string) => Option.orElse(Option.fromUndefinedOr(names.get(at)), () => Option.liftPredicate(at, (id) => id in externalNodes));
            return Array.getSomes(edges.map((edge) => Option.map(Option.all({ source: named(edge.source), target: named(edge.target) }), (ends): RawProjectGraphDependency => ({ ...edge, ...ends }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
