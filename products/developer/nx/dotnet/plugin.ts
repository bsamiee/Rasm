import { basename, dirname, extname, join, relative } from 'node:path';
import { platform } from 'node:process';
import { fileURLToPath } from 'node:url';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResult, DependencyType, hashArray, type ProjectGraphExternalNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, globWithWorkspaceContext, hashFile, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, Equivalence, FileSystem, Option, Predicate, Record, Ref, Result, Schema, Stream, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { Edges, Node, PLUGIN, publishing, run, spawned, targets, type Verbs } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const SOLUTIONS = ['.slnx', '.sln'];
const FILES = combineGlobPatterns([`*{${[...PROJECTS, ...SOLUTIONS].join(',')},.props,.targets,.globalconfig}`, 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'].map((file) => `**/${file}`));
const PLATFORMS: Readonly<Record<string, NodeJS.Platform>> = { windows: 'win32', macos: 'darwin', maccatalyst: 'darwin', ios: 'darwin', tvos: 'darwin' };
const ANALYZER = fileURLToPath(import.meta.resolve('#analyzer'));
const TAGS = { release: 'release', platform: 'os:' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);
const Options = Schema.Struct({
    coveragePath: Schema.optionalKey(Schema.String),
}).annotate({
    description: 'coveragePath: workspace folder the coverage configuration of test--dotnet passes to dotnet test as --results-directory, where coverlet.MTP writes its lcov report coverage.<timestamp>.info, {projectName} naming the project, absent leaves the report in the test results folder of the project',
});
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const own = Effect.runSync(
    Effect.cached(
        FileSystem.FileSystem.pipe(
            Effect.flatMap((fs) => fs.readDirectory(import.meta.dirname)),
            Effect.map((names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]))),
        ),
    ),
);

const stamped = Effect.fnUntraced(function* (workspaceRoot: string, { evaluated, nodes }: Report) {
    const fs = yield* FileSystem.FileSystem;
    const tracked = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...evaluated, ...nodes.flatMap(({ locks }) => locks)]));
    const folders = Array.dedupe([dirname(ANALYZER), ...nodes.flatMap(({ restored }) => restored.map((folder) => join(workspaceRoot, folder)))]);
    const held = yield* Effect.forEach(folders, (folder) => Effect.map(Effect.orElseSucceed(fs.readDirectory(folder), Array.empty<string>), (names) => ({ folder, names })), { concurrency: 'unbounded' });
    const files = held.flatMap(({ folder, names }) => names.toSorted().map((name) => join(folder, name)));
    return hashArray([tracked, ...files.flatMap((file) => [relative(workspaceRoot, file), ...Array.fromNullishOr(hashFile(file))])]);
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const argv = ['dotnet', ANALYZER, workspaceRoot] as const;
    const report = yield* Effect.flatMap(spawned(argv, { cwd: workspaceRoot, stdin: Stream.encodeText(Stream.make(matched.join('\n'))) }), (stdout) => Effect.mapError(Schema.decodeEffect(Reported)(stdout), (error) => new Cause.UnknownError(error, `${CommandLine.display(argv)} printed no report: ${stdout}`)));
    const known = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(report.nodes.flatMap(({ inputs }) => inputs)))));
    return { ...report, nodes: report.nodes.map((node) => ({ ...node, inputs: node.inputs.filter((input) => known.has(input)) })) };
});

const related = ({ nodes }: Report): readonly RawProjectGraphDependency[] => {
    const pinned = new Map(nodes.map((node) => [node.file, Record.fromIterableWith(node.packages, (reference) => [reference.id, reference] as const)] as const));
    return [
        ...nodes.flatMap((node) => node.references.map((reference): RawProjectGraphDependency => ({ source: dirname(node.file), target: reference.root, ...(reference.declared === node.file ? { type: DependencyType.static, sourceFile: node.file } : { type: DependencyType.implicit }) }))),
        ...nodes.flatMap((node) => node.packages.filter(({ direct }) => direct).map((reference): RawProjectGraphDependency => ({ source: dirname(node.file), target: external(reference), type: DependencyType.implicit }))),
        ...nodes.flatMap((node) =>
            node.packages.flatMap((reference) => Array.getSomes(reference.dependencies.map((dependency) => Record.get(pinned.get(node.file) ?? {}, dependency))).map((target) => ({ source: external(reference), target: external(target), type: DependencyType.static }) satisfies RawProjectGraphDependency)),
        ),
    ];
};

const releasable = (node: Node) => node.packable || node.publishable;

const projected = (nodes: readonly Node[], settings: typeof Options.Type, manifests: readonly Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>[]) => {
    const folders = Record.fromIterableWith(nodes, (node) => [dirname(node.file), node] as const);
    const carried = (node: Node): readonly NonNullable<Node['releases']>[] => [
        ...Option.toArray(Option.fromNullOr(node.releases)),
        ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root)))
            .filter((reference) => !releasable(reference))
            .flatMap(carried),
    ];
    const evaluated = (node: Node): readonly string[] => [...node.inputs, ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root))).flatMap(evaluated)];
    return Array.zip(nodes, manifests).map(([node, manifest]): [string, CreateNodesResult] => {
        const mapped = Array.getSomes(node.platforms.map((identifier) => Record.get(PLATFORMS, identifier.toLowerCase())));
        const supported = Array.dedupe(mapped);
        const limited = node.test && mapped.length > 0 && mapped.length === node.platforms.length;
        const tags = [...(releasable(node) ? [TAGS.release] : []), ...(limited ? supported.map((held) => `${TAGS.platform}${held}`) : [])];
        const project = {
            root: dirname(node.file),
            name: node.name,
            metadata: { technologies: ['dotnet', node.language] },
            targets: {
                ...aggregated(targets({ ...node, inputs: Array.dedupe(evaluated(node)), test: limited ? supported.some((held) => held === platform) : node.test }, Option.fromUndefinedOr(settings.coveragePath)), manifest, 'dotnet'),
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

const inferred = Effect.fnUntraced(function* (matched: readonly string[], options: typeof Options.Encoded | undefined, { workspaceRoot }: CreateNodesContext) {
    const [fs, edges, settings, hashed] = yield* Effect.all([FileSystem.FileSystem, Edges, Schema.decodeEffect(Options)(options ?? {}), own]);
    const cache = new PluginCache<{ readonly report: Report; readonly stamp: string }>(join(workspaceDataDirectory, `dotnet-${hashed}.hash`));
    const key = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...matched]));
    const held = yield* Effect.findFirst(Array.fromNullishOr(cache.get(key)), (stored) => Effect.map(stamped(workspaceRoot, stored.report), (now) => now === stored.stamp));
    const read = yield* Effect.result(Option.match(held, { onSome: (stored) => Effect.succeed(stored.report), onNone: () => analyzed(workspaceRoot, matched) }));
    const report = Result.getOrElse(read, (): Report => ({ nodes: [], failures: [], evaluated: [] }));
    yield* Option.isNone(held) && Result.isSuccess(read) && report.failures.length === 0 ? Effect.flatMap(stamped(workspaceRoot, report), (stamp) => Effect.sync(() => cache.set(key, { report, stamp }))).pipe(Effect.andThen(Effect.sync(() => cache.writeToDisk()))) : Effect.void;
    yield* Ref.set(edges, related(report));
    const declared = Array.dedupe(report.nodes.flatMap((node) => [node.file, ...node.inputs, ...node.locks])).map((at) => join('{workspaceRoot}', at));
    const manifests = yield* Effect.forEach(report.nodes, (node) => manifested(workspaceRoot, dirname(node.file)));
    const solutions = yield* Effect.forEach(
        matched.filter((file) => SOLUTIONS.includes(extname(file))),
        (file) =>
            Effect.map(Effect.all([fs.exists(join(workspaceRoot, dirname(file), 'package.json')), manifested(workspaceRoot, dirname(file))]), ([named, manifest]): [string, CreateNodesResult] => {
                const command = ['dotnet', 'package', 'update', '--project', basename(file)].map(quoteShellArg).join(' ');
                const upgrade: TargetConfiguration = {
                    command,
                    options: { cwd: dirname(file) },
                    inputs: [join('{workspaceRoot}', file), ...declared, { externalDependencies: [] }],
                    metadata: { description: `Moves every package reference of ${basename(file)} to its newest release with ${command}`, help: { command: 'dotnet package update --help', example: { args: ['--vulnerable'] } } },
                };
                return [file, { projects: { [dirname(file)]: { root: dirname(file), ...(named ? {} : { name: basename(file, extname(file)) }), metadata: { technologies: ['dotnet'] }, targets: aggregated({ upgrade: { dotnet: upgrade } }, manifest, 'dotnet') } } }];
            }),
    );
    const results = [...projected(report.nodes, settings, manifests), ...solutions];
    const failures = [...Option.toArray(Option.map(Result.getFailure(read), (error): [null, Error] => [null, error])), ...report.failures.map(({ file, message }): [string, Error] => [file, new Cause.UnknownError(file, message)])];
    return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [FILES, (matched, options, context) => run(inferred(matched, options, context))];

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
