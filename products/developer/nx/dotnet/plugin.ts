import { hash } from 'node:crypto';
import { basename, dirname, extname, format, join, parse, posix, relative } from 'node:path';
import { env, platform } from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResult, DependencyType, hashArray, normalizePath, type ProjectGraphExternalNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, emitPluginWorkerLog, globWithWorkspaceContext, hashFile, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, fingerprinted, inferring, manifested, spawned } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Config, Duration, Effect, Equal, Equivalence, FileSystem, Graph, Option, Record, Ref, Result, Schema, Stream, String, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import which from 'which';
import { ANALYZER, Edges, ENVIRONMENT, line, Node, PLUGIN, publishing, run, targets } from '#/targets';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const SOLUTIONS = ['.slnx', '.sln'];
const FILES = combineGlobPatterns([`*{${[...PROJECTS, ...SOLUTIONS].join(',')},.props,.targets,.globalconfig}`, 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'].map((file) => `**/${file}`));
const PLATFORMS: Readonly<Record<string, NodeJS.Platform>> = { windows: 'win32', macos: 'darwin', maccatalyst: 'darwin', ios: 'darwin', tvos: 'darwin' };
const SOURCE = format({ dir: dirname(dirname(ANALYZER)), name: parse(ANALYZER).name, ext: '.csproj' });
const SUMS = format({ dir: dirname(ANALYZER), name: parse(ANALYZER).name, ext: '.sha256' });

// --- [MODELS] --------------------------------------------------------------------------

class Report extends Schema.Opaque<Report, unknown>()(Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String), probed: Schema.Array(Schema.String), variables: Schema.Array(Schema.String) })) {
    static readonly empty: Report = { nodes: [], failures: [], evaluated: [], probed: [], variables: [] };
}
const Reported = Schema.fromJsonString(Report);
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));
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

const stamped = Effect.fnUntraced(function* (workspaceRoot: string, host: Option.Option<string>, { evaluated, nodes, probed, variables }: Report) {
    const fs = yield* FileSystem.FileSystem;
    const read = Array.match([...evaluated, ...nodes.flatMap(({ locks }) => locks)], { onEmpty: () => Effect.succeed(hashArray([])), onNonEmpty: (globs) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, globs)) });
    const installed = Effect.fromOption(host).pipe(
        Effect.flatMap(fs.realPath),
        Effect.flatMap((real) => Effect.map(fs.readDirectory(join(dirname(real), 'sdk')), (versions) => [real, ...versions.toSorted()])),
    );
    const [tracked, sdks, present] = yield* Effect.all([read, Effect.option(installed), Effect.forEach(probed, fs.exists, { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
    const files = Array.dedupe([ANALYZER, ...nodes.flatMap(({ restored }) => restored.map((at) => join(workspaceRoot, at))), ...nodes.flatMap(({ external: held }) => held)]);
    const set = Record.fromEntries(Object.entries(env).map(([name, value]) => [name.toLowerCase(), value ?? ''] as const));
    return hashArray([
        tracked,
        ...Option.getOrElse(sdks, Array.empty<string>),
        ...files.flatMap((file) => [relative(workspaceRoot, file), ...Array.fromNullishOr(hashFile(file))]),
        ...Array.zipWith(probed, present, (at, exists) => `${at} ${exists}`),
        ...variables.map((name) => `${name}=${set[name.toLowerCase()] ?? ''}`),
    ]);
});

const published = (workspaceRoot: string) => `${line(['dotnet', 'publish', relative(workspaceRoot, SOURCE)])} builds it`;

const ready = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const [host, present, sourced, sums] = yield* Effect.all([Effect.promise(() => which('dotnet', { nothrow: true })), fs.exists(ANALYZER), fs.exists(SOURCE), Effect.option(fs.readFileString(SUMS))], { concurrency: 'unbounded' });
    const resolved = Result.fromNullishOr(host, () => 'no folder of PATH holds dotnet').pipe(
        Result.filterOrFail(
            () => present,
            () => `${relative(workspaceRoot, ANALYZER)} is absent, ${published(workspaceRoot)}`,
        ),
    );
    yield* Option.match(
        Option.filter(Result.getFailure(resolved), () => matched.some((file) => PROJECTS.includes(extname(file)))),
        {
            onNone: () => Effect.void,
            onSome: (reason) => Effect.sync(() => emitPluginWorkerLog('warn', `No MSBuild project of the workspace enters the graph, as ${reason}`)),
        },
    );
    const stale = yield* Result.isSuccess(resolved) && sourced
        ? Option.match(sums, {
              onNone: () => Effect.succeed([SUMS]),
              onSome: (text) =>
                  Effect.filter(
                      text
                          .split('\n')
                          .filter(String.isNonEmpty)
                          .map((row) => [join(dirname(SOURCE), row.slice(row.indexOf(' ')).trim()), row.slice(0, row.indexOf(' ')).toLowerCase()] as const),
                      ([file, digest]) => Effect.match(fs.readFile(file), { onFailure: () => true, onSuccess: (bytes) => hash('sha256', bytes) !== digest }),
                      { concurrency: 'unbounded' },
                  ).pipe(Effect.map(Array.map(([file]) => file))),
          })
        : Effect.succeed(Array.empty<string>());
    return yield* Array.match(stale, {
        onEmpty: () => Effect.succeed(Result.getSuccess(resolved)),
        onNonEmpty: (files) => Effect.fail(new Cause.UnknownError(files, `${relative(workspaceRoot, ANALYZER)} predates ${files.map((file) => relative(workspaceRoot, file)).join(', ')}, ${published(workspaceRoot)}`)),
    });
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const argv = ['dotnet', ANALYZER, workspaceRoot] as const;
    const seconds = yield* Timeout;
    const stdout = yield* spawned(argv, { cwd: workspaceRoot, env: ENVIRONMENT, stdin: Stream.encodeText(Stream.make(matched.join('\n'))) }).pipe(
        Effect.timeoutOrElse({ duration: Duration.seconds(seconds), orElse: () => Effect.fail(new Cause.UnknownError(seconds, `${CommandLine.display(argv)} ran past ${seconds} seconds, set NX_DOTNET_PROJECT_GRAPH_TIMEOUT to a higher count of seconds`)) }),
    );
    const report = yield* Effect.mapError(Schema.decodeEffect(Reported)(stdout), (error) => new Cause.UnknownError(error, `${CommandLine.display(argv)} printed a report this plugin cannot read, as ${relative(workspaceRoot, ANALYZER)} predates the plugin, ${published(workspaceRoot)}: ${error.message}`));
    const known = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(report.nodes.flatMap(({ inputs }) => inputs)))));
    return { ...report, nodes: report.nodes.map((node) => ({ ...node, inputs: node.inputs.filter((input) => known.has(input)) })) };
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

const projected = (nodes: readonly Node[], { coveragePath }: Options, manifests: readonly Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>[], listed: (node: Node) => Option.Option<string>) => {
    const placed = Record.fromEntries(nodes.map((node, index) => [dirname(node.file), { index, released: releasable(node) }] as const));
    const referenced = Graph.fromSnapshot({
        type: 'directed',
        nodes: nodes.map((data, index) => ({ index, data })),
        edges: nodes.flatMap((node, source) => Array.getSomes(node.references.map(({ root }) => Record.get(placed, root))).map(({ index: target, released }) => ({ source, target, data: released }))).map((edge, index) => ({ ...edge, index })),
    });
    const unreleased = Graph.mutate(referenced, (mutable) => {
        Graph.filterEdges(mutable, (released) => !released);
    });
    const reached =
        (graph: typeof referenced) =>
        (node: Node): readonly Node[] =>
            Array.fromIterable(Graph.values(Graph.dfs(graph, { start: Option.toArray(Option.map(Record.get(placed, dirname(node.file)), Struct.get('index'))) })));
    const closure = reached(referenced);
    const solution = (node: Node) => (configuration: string) => {
        const assigned = closure(node).map((each) => [each.file, [each.evaluated, ...each.declared].find((mode) => mode.configuration.toLowerCase() === configuration.toLowerCase())?.configuration ?? each.evaluated.configuration, each.platform] as const);
        return assigned.every(([, held]) => held === configuration) ? Option.none() : Option.some(Record.fromIterableWith(assigned, ([file, held, pair]) => [file, `${held}|${pair}`] as const));
    };
    const carried = (node: Node) => reached(unreleased)(node).flatMap(({ releases }) => Option.toArray(Option.fromNullOr(releases)));
    const evaluated = (node: Node): readonly string[] => closure(node).flatMap(({ inputs }) => inputs);
    return Array.zip(nodes, manifests).map(([node, manifest]): [string, CreateNodesResult] => {
        const mapped = Array.getSomes(node.platforms.map((identifier) => Record.get(PLATFORMS, identifier.toLowerCase())));
        const supported = Array.dedupe(mapped);
        const limited = node.test && mapped.length > 0 && mapped.length === node.platforms.length;
        const tags = [...(releasable(node) ? ['release'] : []), ...(limited ? supported.map((held) => `os:${held}`) : []), ...node.tags];
        const project = {
            root: dirname(node.file),
            name: node.name,
            metadata: { technologies: ['dotnet', node.language] },
            targets: {
                ...aggregated(targets({ ...node, inputs: Array.dedupe(evaluated(node)), test: limited ? supported.some((held) => held === platform) : node.test }, Option.fromUndefinedOr(coveragePath), solution(node), listed(node)), manifest, 'dotnet'),
                ...publishing(node),
            },
            ...(releasable(node) && { release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: basename(node.file), releases: Array.dedupeWith(carried(node), Equivalence.mapInput(Equivalence.String, Struct.get('unshipped'))) } } } }),
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
    const key = hashArray([hashObject(settings), hashed, yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...matched]))]);
    const host = yield* ready(workspaceRoot, matched);
    const entries = Option.isSome(host) ? (new PluginCache<readonly { readonly report: Report; readonly stamp: string }[]>(slot).get(key) ?? []) : [];
    const cached = yield* Effect.findFirst(entries, (entry) => Effect.map(stamped(workspaceRoot, host, entry.report), Equal.equals(entry.stamp)));
    const fresh = Option.isNone(cached) && Option.isSome(host);
    const read = yield* Effect.result(Option.match(cached, { onSome: (stored) => Effect.succeed(stored.report), onNone: () => (fresh ? analyzed(workspaceRoot, matched) : Effect.succeed(Report.empty)) }));
    const report = Result.getOrElse(read, () => Report.empty);
    yield* fresh && Result.isSuccess(read) && report.failures.length === 0 ? Effect.flatMap(stamped(workspaceRoot, host, report), (stamp) => Effect.sync(() => new PluginCache(slot, { [key]: [{ report, stamp }, ...Array.take(entries, 1)] }).writeToDisk())) : Effect.void;
    const publisher = Array.findFirst(report.nodes, ({ evaluated }) => [...evaluated.outputs, ...evaluated.published].includes(normalizePath(relative(workspaceRoot, dirname(ANALYZER)))));
    yield* Ref.set(edges, [...related(report), ...Option.toArray(Option.map(publisher, (node): RawProjectGraphDependency => ({ source: normalizePath(relative(workspaceRoot, dirname(dirname(SOURCE)))), target: dirname(node.file), type: DependencyType.implicit })))]);
    yield* Effect.forEach(
        report.nodes.filter((node) => node.external.length > 0),
        (node) => Effect.flatMap(Effect.option(fs.readFileString(listing(node))), (held) => (Option.contains(held, node.external.join('\n')) ? Effect.void : Effect.andThen(fs.makeDirectory(dirname(listing(node)), { recursive: true }), fs.writeFileString(listing(node), node.external.join('\n'))))),
        { discard: true },
    );
    const declared = Array.dedupe(report.nodes.flatMap((node) => [node.file, ...node.inputs, ...node.locks])).map((at) => posix.join('{workspaceRoot}', at));
    const manifests = yield* Effect.forEach(report.nodes, (node) => manifested(workspaceRoot, dirname(node.file)));
    const building = Record.getSomes({ dependsOn: Option.map(publisher, (node) => [{ projects: [node.name], target: 'build--dotnet' }]) });
    const solutions = yield* Effect.forEach(
        matched.filter((file) => SOLUTIONS.includes(extname(file))),
        (file) =>
            Effect.map(Effect.all([Effect.option(Effect.flatMap(fs.readFileString(join(workspaceRoot, dirname(file), 'package.json')), Schema.decodeEffect(Manifest))), manifested(workspaceRoot, dirname(file))]), ([named, manifest]): [string, CreateNodesResult] => {
                const moved = ['dotnet', relative(join(workspaceRoot, dirname(file)), ANALYZER), 'upgrade', basename(file)] as const;
                const cleared = ['dotnet', 'nuget', 'locals', 'global-packages', '--clear'] as const;
                const locked = ['dotnet', 'restore', basename(file), '--force-evaluate'] as const;
                const upgrade: TargetConfiguration = {
                    executor: `${distribution.name}:upgrade`,
                    ...building,
                    options: { cwd: dirname(file), commands: [{ apply: moved, preview: [...moved, '--dry-run'], env: ENVIRONMENT }, { apply: cleared }, { apply: locked }] },
                    inputs: [posix.join('{workspaceRoot}', file), ...declared, { externalDependencies: [] }],
                    metadata: {
                        description: `Moves each central package version ${basename(file)} projects read to the newest release its version range admits and its consumers' frameworks support with ${line(moved)}, then empties the global packages folder with ${line(cleared)} so the restore fetches only what the new lock files name, and rewrites lock files with ${line(locked)}`,
                        help: { command: 'dotnet restore --help', example: { options: { dryRun: true } } },
                    },
                };
                const purged = ['dotnet', 'nuget', 'locals', 'all', '--clear'] as const;
                const clean: TargetConfiguration = {
                    executor: `${distribution.name}:clean`,
                    options: { cwd: dirname(file) },
                    configurations: { cache: { commands: [purged] } },
                    inputs: [{ externalDependencies: [] }],
                    metadata: { description: `Under configuration cache, empties every NuGet local folder ${basename(file)} reads, the global packages, HTTP cache, temp, and plugin cache, with ${line(purged)}`, help: { command: 'dotnet nuget locals --help', example: { args: [] } } },
                };
                return [file, { projects: { [dirname(file)]: { root: dirname(file), ...(Option.isSome(named) ? {} : { name: basename(file, extname(file)) }), metadata: { technologies: ['dotnet'] }, targets: aggregated({ upgrade: { dotnet: upgrade }, clean: { nuget: clean } }, manifest, 'dotnet') } } }];
            }),
    );
    const results = [
        ...projected(report.nodes, settings, manifests, (node) =>
            Option.map(
                Option.liftPredicate(node, (held) => held.external.length > 0),
                (held) => relative(workspaceRoot, listing(held)),
            ),
        ),
        ...solutions,
    ];
    const failures = [...Option.toArray(Option.map(Result.getFailure(read), (error): [null, Error] => [null, error])), ...report.failures.map(({ file, message }): [string, Error] => [file, new Cause.UnknownError(file, message)])];
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
        Effect.map(Effect.flatMap(Edges, Ref.get), (edges) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const named = (at: string) => Option.orElse(Option.fromUndefinedOr(names.get(at)), () => Option.liftPredicate(at, (id) => id in externalNodes));
            return Array.getSomes(edges.map((edge) => Option.map(Option.all({ source: named(edge.source), target: named(edge.target) }), (ends): RawProjectGraphDependency => ({ ...edge, ...ends }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
