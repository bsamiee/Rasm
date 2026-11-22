import { basename, dirname, extname, join, posix, relative } from 'node:path';
import { platform } from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResult, DependencyType, hashArray, normalizePath, type ProjectGraphExternalNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, globWithWorkspaceContext, hashFile, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, fingerprinted, manifested, spawned } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Config, Duration, Effect, Equal, Equivalence, FileSystem, Option, Record, Ref, Result, Schema, Stream, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { ANALYZER, Edges, environment, launched, line, Node, PLUGIN, publishing, run, SDK, targets } from '#/targets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = typeof Report.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['.csproj', '.fsproj', '.vbproj'];
const SOLUTIONS = ['.slnx', '.sln'];
const FILES = combineGlobPatterns([`*{${[...PROJECTS, ...SOLUTIONS].join(',')},.props,.targets,.globalconfig}`, 'global.json', 'Directory.Build.rsp', 'nuget.config', 'NuGet.config', 'NuGet.Config', '.editorconfig'].map((file) => `**/${file}`));
const PLATFORMS: Readonly<Record<string, NodeJS.Platform>> = { windows: 'win32', macos: 'darwin', maccatalyst: 'darwin', ios: 'darwin', tvos: 'darwin' };
const TAGS = { release: 'release', platform: 'os:' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({ nodes: Schema.Array(Node), failures: Schema.Array(Schema.Struct({ file: Schema.String, message: Schema.String })), evaluated: Schema.Array(Schema.String) });
const Reported = Schema.fromJsonString(Report);
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));
const Options = Schema.Struct({
    coveragePath: Schema.optionalKey(Schema.String),
}).annotate({
    description: 'coveragePath: workspace folder the coverage configuration of test--dotnet passes to dotnet test as --results-directory, where coverlet.MTP writes its lcov report coverage.<timestamp>.info, {projectName} naming the project, absent leaves the report in the test results folder of the project',
});
const Timeout = Config.schema(Schema.Finite.check(Schema.isGreaterThan(0)), 'NX_DOTNET_PROJECT_GRAPH_TIMEOUT').pipe(Config.withDefault(isCI() === false ? 120 : 600));

// --- [OPERATIONS] ----------------------------------------------------------------------

const external = ({ id, version }: Node['packages'][number]) => `nuget:${id}@${version}`;

const own = fingerprinted(import.meta.dirname);

const stamped = Effect.fnUntraced(function* (workspaceRoot: string, { evaluated, nodes }: Report) {
    const read = Array.match([...evaluated, ...nodes.flatMap(({ locks }) => locks)], { onEmpty: () => Effect.succeed(hashArray([])), onNonEmpty: (globs) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, globs)) });
    const [tracked, sdk] = yield* Effect.all([read, spawned(SDK, { cwd: workspaceRoot })], { concurrency: 'unbounded' });
    const files = Array.dedupe([ANALYZER, ...nodes.flatMap(({ restored }) => restored.map((at) => join(workspaceRoot, at))), ...nodes.flatMap(({ external: held }) => held)]);
    return hashArray([tracked, sdk, ...files.flatMap((file) => [relative(workspaceRoot, file), ...Array.fromNullishOr(hashFile(file))])]);
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, matched: readonly string[]) {
    const argv = [...launched(ANALYZER), workspaceRoot] as const;
    const seconds = yield* Timeout;
    const stdout = yield* spawned(argv, { cwd: workspaceRoot, env: environment(workspaceRoot), stdin: Stream.encodeText(Stream.make(matched.join('\n'))) }).pipe(
        Effect.timeoutOrElse({ duration: Duration.seconds(seconds), orElse: () => Effect.fail(new Cause.UnknownError(seconds, `${CommandLine.display(argv)} ran past ${seconds} seconds, set NX_DOTNET_PROJECT_GRAPH_TIMEOUT to a higher count of seconds`)) }),
    );
    const report = yield* Effect.mapError(
        Schema.decodeEffect(Reported)(stdout),
        (error) => new Cause.UnknownError(error, `${CommandLine.display(argv)} printed a report this plugin cannot read, as ${relative(workspaceRoot, ANALYZER)} predates the plugin, run mise deps msbuild-analyzer to rebuild it: ${error.message}`),
    );
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

const releasable = (node: Node) => node.versioned && (node.packable || node.publishable);

const listing = (node: Node) => join(workspaceDataDirectory, 'dotnet-external', `${hashArray([node.file])}.txt`);

const projected = (nodes: readonly Node[], settings: typeof Options.Type, manifests: readonly Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>[], listed: (node: Node) => Option.Option<string>) => {
    const folders = Record.fromIterableWith(nodes, (node) => [dirname(node.file), node] as const);
    const closure = (node: Node): readonly Node[] => [node, ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root))).flatMap(closure)];
    const solution = (node: Node) => (configuration: string) => {
        const assigned = closure(node).map((each) => [each.file, [each.evaluated, ...each.declared].find((mode) => mode.configuration.toLowerCase() === configuration.toLowerCase())?.configuration ?? each.evaluated.configuration, each.platform] as const);
        return assigned.every(([, held]) => held === configuration) ? Option.none() : Option.some(Record.fromIterableWith(assigned, ([file, held, pair]) => [file, `${held}|${pair}`] as const));
    };
    const carried = (node: Node): readonly NonNullable<Node['releases']>[] => [
        ...Option.toArray(Option.fromNullOr(node.releases)),
        ...Array.getSomes(node.references.map(({ root }) => Record.get(folders, root)))
            .filter((reference) => !releasable(reference))
            .flatMap(carried),
    ];
    const evaluated = (node: Node): readonly string[] => closure(node).flatMap(({ inputs }) => inputs);
    return Array.zip(nodes, manifests).map(([node, manifest]): [string, CreateNodesResult] => {
        const mapped = Array.getSomes(node.platforms.map((identifier) => Record.get(PLATFORMS, identifier.toLowerCase())));
        const supported = Array.dedupe(mapped);
        const limited = node.test && mapped.length > 0 && mapped.length === node.platforms.length;
        const tags = [...(releasable(node) ? [TAGS.release] : []), ...(limited ? supported.map((held) => `${TAGS.platform}${held}`) : []), ...node.tags];
        const project = {
            root: dirname(node.file),
            name: node.name,
            metadata: { technologies: ['dotnet', node.language] },
            targets: {
                ...aggregated(targets({ ...node, inputs: Array.dedupe(evaluated(node)), test: limited ? supported.some((held) => held === platform) : node.test }, Option.fromUndefinedOr(settings.coveragePath), solution(node), listed(node)), manifest, 'dotnet'),
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

const inferred = Effect.fnUntraced(function* (matched: Array.NonEmptyReadonlyArray<string>, options: typeof Options.Encoded | undefined, { workspaceRoot }: CreateNodesContext) {
    const [fs, edges, settings, hashed] = yield* Effect.all([FileSystem.FileSystem, Edges, Schema.decodeEffect(Options)(options ?? {}), own]);
    const slot = join(workspaceDataDirectory, 'dotnet.hash');
    const key = hashArray([hashObject(settings), hashed, yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...matched]))]);
    const cached = yield* Effect.findFirst(Array.fromNullishOr(new PluginCache<{ readonly report: Report; readonly stamp: string }>(slot).get(key)), (stored) => Effect.map(stamped(workspaceRoot, stored.report), Equal.equals(stored.stamp)));
    const read = yield* Effect.result(Option.match(cached, { onSome: (stored) => Effect.succeed(stored.report), onNone: () => analyzed(workspaceRoot, matched) }));
    const report = Result.getOrElse(read, (): Report => ({ nodes: [], failures: [], evaluated: [] }));
    yield* Option.isNone(cached) && Result.isSuccess(read) && report.failures.length === 0 ? Effect.flatMap(stamped(workspaceRoot, report), (stamp) => Effect.sync(() => new PluginCache(slot, { [key]: { report, stamp } }).writeToDisk())) : Effect.void;
    yield* Ref.set(edges, related(report));
    yield* Effect.forEach(
        report.nodes.filter((node) => node.external.length > 0),
        (node) => Effect.andThen(fs.makeDirectory(dirname(listing(node)), { recursive: true }), fs.writeFileString(listing(node), node.external.join('\n'))),
        { discard: true },
    );
    const declared = Array.dedupe(report.nodes.flatMap((node) => [node.file, ...node.inputs, ...node.locks])).map((at) => posix.join('{workspaceRoot}', at));
    const manifests = yield* Effect.forEach(report.nodes, (node) => manifested(workspaceRoot, dirname(node.file)));
    const building = Record.getSomes({
        dependsOn: Option.map(
            Array.findFirst(report.nodes, ({ evaluated: { outputs, published } }) => [...outputs, ...published].includes(normalizePath(relative(workspaceRoot, dirname(ANALYZER))))),
            (node) => [{ projects: [node.name], target: 'build--dotnet' }],
        ),
    });
    const solutions = yield* Effect.forEach(
        matched.filter((file) => SOLUTIONS.includes(extname(file))),
        (file) =>
            Effect.map(Effect.all([Effect.option(Effect.flatMap(fs.readFileString(join(workspaceRoot, dirname(file), 'package.json')), Schema.decodeEffect(Manifest))), manifested(workspaceRoot, dirname(file))]), ([named, manifest]): [string, CreateNodesResult] => {
                const moved = [...launched(relative(join(workspaceRoot, dirname(file)), ANALYZER)), 'upgrade', basename(file)] as const;
                const cleared = ['dotnet', 'nuget', 'locals', 'global-packages', '--clear'] as const;
                const locked = ['dotnet', 'restore', basename(file), '--force-evaluate'] as const;
                const upgrade: TargetConfiguration = {
                    executor: `${distribution.name}:upgrade`,
                    ...building,
                    options: { cwd: dirname(file), commands: [{ apply: moved, preview: [...moved, '--dry-run'], env: environment('{root}') }, { apply: cleared }, { apply: locked }] },
                    inputs: [posix.join('{workspaceRoot}', file), ...declared, { externalDependencies: [] }],
                    metadata: {
                        description: `Moves each central package version ${basename(file)} projects read to the newest release its version range admits and its consumers' frameworks support with ${line(moved)}, then empties the global packages folder with ${line(cleared)} so the restore fetches only what the new lock files name, and rewrites lock files with ${line(locked)}`,
                        help: { command: 'dotnet restore --help', example: { options: { dryRun: true } } },
                    },
                };
                return [file, { projects: { [dirname(file)]: { root: dirname(file), ...(Option.isSome(named) ? {} : { name: basename(file, extname(file)) }), metadata: { technologies: ['dotnet'] }, targets: aggregated({ upgrade: { dotnet: upgrade } }, manifest, 'dotnet') } } }];
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

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (matched, options, context) =>
        run(
            Array.match(matched, {
                onEmpty: () =>
                    Effect.as(
                        Effect.flatMap(Edges, (edges) => Ref.set(edges, [])),
                        [],
                    ),
                onNonEmpty: (files) => inferred(files, options, context),
            }),
        ),
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
