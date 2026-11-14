import { findPackageJSON } from 'node:module';
import { basename, dirname, extname, join, relative } from 'node:path';
import { platform } from 'node:process';
import { fileURLToPath } from 'node:url';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResult, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, globWithWorkspaceContext, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, Equivalence, FileSystem, Option, Record, Ref, Result, Schema, Stream, Struct, UndefinedOr } from 'effect';
import { DotnetError, Edges, manifested, Node, PLUGIN, printed, publishing, run, targets, type Verbs } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const SOLUTIONS = ['.slnx', '.sln'];
const FILES = combineGlobPatterns([`*{${[...PROJECTS, ...SOLUTIONS].join(',')},.props,.targets,.globalconfig}`, 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'].map((file) => `**/${file}`));
const PLATFORMS = { macOS: 'darwin', Windows: 'win32', Linux: 'linux' } as const satisfies Readonly<Record<string, NodeJS.Platform>>;
const PROJECT = Option.liftThrowable(() => fileURLToPath(import.meta.resolve('#analyzer/project')))();
const ANALYZER = Option.getOrElse(PROJECT, () => fileURLToPath(import.meta.resolve('#analyzer')));

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);
const Options = Schema.Record(Schema.String, Schema.Literal(false)).annotate({
    description: 'Each key naming a target the plugin infers (build--dotnet, pack--dotnet, check--dotnet-build, check--dotnet-format, fix--dotnet-format, test--dotnet, upgrade--dotnet): false removes the target from every project; nx.json targetDefaults state every other change',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const stamped = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[], { evaluated, nodes }: Report) {
    const fs = yield* FileSystem.FileSystem;
    const tracked = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...matched, ...evaluated, ...nodes.flatMap(({ locks }) => locks), join(relative(workspaceRoot, import.meta.dirname), '**')]));
    const folders = Array.dedupe([dirname(ANALYZER), ...nodes.flatMap(({ restored }) => restored.map((folder) => join(workspaceRoot, folder)))]);
    const held = yield* Effect.forEach(folders, (folder) => Effect.map(Effect.orElseSucceed(fs.readDirectory(folder), Array.empty<string>), (names) => ({ folder, names })), { concurrency: 'unbounded' });
    const files = held.flatMap(({ folder, names }) => names.toSorted().map((name) => join(folder, name)));
    return hashArray([...Array.fromNullishOr(UndefinedOr.map(findPackageJSON(import.meta.url), hashFile)), tracked, ...files.flatMap((file) => [relative(workspaceRoot, file), ...Array.fromNullishOr(hashFile(file))])]);
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const [command, ...args] = [...Option.match(PROJECT, { onNone: () => [ANALYZER], onSome: (project) => ['dotnet', 'run', '--project', project, '--'] }), workspaceRoot];
    const report = yield* Effect.flatMap(printed(command ?? ANALYZER, args, { cwd: workspaceRoot, stdin: Stream.encodeText(Stream.make(matched.join('\n'))) }), Schema.decodeEffect(Reported));
    const known = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(report.nodes.flatMap(({ inputs }) => inputs)))));
    return { ...report, nodes: report.nodes.map((node) => ({ ...node, inputs: node.inputs.filter((input) => known.has(input)) })) };
});

const related = (workspaceRoot: string, { nodes }: Report): readonly RawProjectGraphDependency[] => {
    const pinned = new Map(nodes.map((node) => [node.file, Record.fromIterableWith(node.packages, (reference) => [reference.id, reference] as const)] as const));
    const plugin = relative(workspaceRoot, dirname(dirname(ANALYZER)));
    return [
        ...nodes.flatMap((node) => node.references.map((reference): RawProjectGraphDependency => ({ source: dirname(node.file), target: reference.root, ...(reference.declared === node.file ? { type: DependencyType.static, sourceFile: node.file } : { type: DependencyType.implicit }) }))),
        ...nodes.flatMap((node) => node.packages.filter(({ direct }) => direct).map((reference): RawProjectGraphDependency => ({ source: dirname(node.file), target: external(reference), type: DependencyType.implicit }))),
        ...nodes.flatMap((node) =>
            node.packages.flatMap((reference) => Array.getSomes(reference.dependencies.map((dependency) => Record.get(pinned.get(node.file) ?? {}, dependency))).map((target) => ({ source: external(reference), target: external(target), type: DependencyType.static }) satisfies RawProjectGraphDependency)),
        ),
        ...nodes.filter((node) => Option.contains(PROJECT, join(workspaceRoot, node.file))).map((node): RawProjectGraphDependency => ({ source: plugin, target: dirname(node.file), type: DependencyType.static, sourceFile: join(plugin, 'package.json') })),
    ];
};

const delivered = (removed: typeof Options.Type, verbs: Verbs, manifest: Readonly<Record<string, TargetConfiguration>>, technologies: readonly string[]): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(
            Record.filter(
                Record.map(verbs, (tools, verb) => Record.filter(tools, (_, tool) => removed[`${verb}--${tool}`] === undefined)),
                (tools) => !Record.isEmptyRecord(tools),
            ),
            (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
                ...Record.collect(tools, (tool, row): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...row, metadata: { technologies: [...technologies], ...row.metadata } }]),
                [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, row) => mergeTargetConfigurations(manifest[`${verb}--${tool}`] ?? {}, row, undefined, undefined, undefined, false).inputs ?? ['default', '^default']).flat())] }],
            ],
        ).flat(),
    );

const projected = (nodes: readonly Node[], removed: typeof Options.Type, manifests: readonly Readonly<Record<string, TargetConfiguration>>[]) => {
    const releasable = (node: Node) => node.packable || node.publishable;
    const folders = Record.fromIterableWith(nodes, (node) => [dirname(node.file), node] as const);
    const carried = (node: Node): readonly NonNullable<Node['releases']>[] => [
        ...Option.toArray(Option.fromNullOr(node.releases)),
        ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root)))
            .filter((reference) => !releasable(reference))
            .flatMap(carried),
    ];
    const evaluated = (node: Node): readonly string[] => [...node.inputs, ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root))).flatMap(evaluated)];
    return Array.zip(nodes, manifests).map(([node, manifest]): [string, CreateNodesResult] => {
        const supported = Array.intersection(Record.keys(PLATFORMS), node.platforms).map((name) => PLATFORMS[name]);
        const limited = node.test && supported.length > 0 && supported.length < Record.keys(PLATFORMS).length;
        const tags = [...(releasable(node) ? ['release'] : []), ...(limited ? supported.map((held) => `os:${held}`) : [])];
        const project = {
            root: dirname(node.file),
            name: node.name,
            metadata: { technologies: ['dotnet', node.language] },
            targets: { ...delivered(removed, targets({ ...node, inputs: Array.dedupe(evaluated(node)), test: limited ? supported.some((held) => held === platform) : node.test }), manifest, ['dotnet', node.language]), ...publishing(node) },
            ...(releasable(node) && { release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: basename(node.file), releases: Array.dedupeWith(carried(node), Equivalence.mapInput(Equivalence.String, Struct.get('unshipped'))) } } } }),
            ...(tags.length > 0 && { tags }),
            ...(node.implicit.length > 0 && { implicitDependencies: [...node.implicit] }),
        };
        const externalNodes = Record.fromIterableWith(node.packages, (reference): readonly [string, ProjectGraphExternalNode] => [external(reference), { type: 'nuget', name: external(reference), data: { version: reference.version, packageName: reference.id, hash: reference.hash } }]);
        return [node.file, { projects: { [project.root]: project }, externalNodes }];
    });
};

const inferred = Effect.fnUntraced(function* (matched: readonly string[], options: unknown, { workspaceRoot }: CreateNodesContext) {
    const [fs, edges, removed] = yield* Effect.all([FileSystem.FileSystem, Edges, Schema.decodeUnknownEffect(Options)(options ?? {})]);
    const cache = new PluginCache<{ readonly report: Report; readonly digest: string }>(join(workspaceDataDirectory, `dotnet-${hashObject(options ?? {})}.hash`));
    const held = yield* Effect.findFirst(Array.fromNullishOr(cache.get(PLUGIN)), (stored) => Effect.map(stamped(workspaceRoot, matched, stored.report), (now) => now === stored.digest));
    const read = yield* Effect.result(Option.match(held, { onSome: (stored) => Effect.succeed(stored.report), onNone: () => analyzed(workspaceRoot, matched) }));
    const report = Result.getOrElse(read, (): Report => ({ nodes: [], failures: [], evaluated: [] }));
    yield* Option.isNone(held) && Result.isSuccess(read) && report.failures.length === 0 ? Effect.map(stamped(workspaceRoot, matched, report), (digest) => cache.set(PLUGIN, { report, digest })) : Effect.void;
    cache.writeToDisk();
    yield* Ref.set(edges, related(workspaceRoot, report));
    const declared = Array.dedupe(report.nodes.flatMap((node) => [node.file, ...node.inputs, ...node.locks])).map((at) => join('{workspaceRoot}', at));
    const manifests = yield* Effect.forEach(report.nodes, (node) => manifested(workspaceRoot, dirname(node.file)));
    const solutions = yield* Effect.forEach(
        matched.filter((file) => SOLUTIONS.includes(extname(file))),
        (file) =>
            Effect.flatMap(Effect.all([fs.exists(join(workspaceRoot, dirname(file), 'package.json')), manifested(workspaceRoot, dirname(file))]), ([named, manifest]) => {
                const upgrade: TargetConfiguration = {
                    command: ['dotnet', 'package', 'update', '--project', basename(file)].map(quoteShellArg).join(' '),
                    options: { cwd: dirname(file) },
                    inputs: [join('{workspaceRoot}', file), ...declared],
                    metadata: { description: `Moves every package reference of ${basename(file)} to its newest release with dotnet package update`, help: { command: 'dotnet package update --help', example: { args: ['--vulnerable'] } } },
                };
                return Effect.succeed<[string, CreateNodesResult]>([file, { projects: { [dirname(file)]: { root: dirname(file), ...(named ? {} : { name: basename(file, extname(file)) }), metadata: { technologies: ['dotnet'] }, targets: delivered(removed, { upgrade: { dotnet: upgrade } }, manifest, ['dotnet']) } } }]);
            }),
    );
    const results = [...projected(report.nodes, removed, manifests), ...solutions];
    const failures = [...Option.toArray(Option.map(Result.getFailure(read), (error): [null, Error] => [null, error])), ...report.failures.map(({ file, message }): [string, Error] => [file, DotnetError.of({ _tag: 'Refused', rule: message }, file)()])];
    return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [FILES, (matched, options, context) => run(inferred(matched, options, context))];

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
