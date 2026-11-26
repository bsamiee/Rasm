import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import process from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, DependencyType, getOutputsForTargetAndConfiguration, hashArray, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, emitPluginWorkerLog, globWithWorkspaceContext, hashFile, hashObject, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, anchored, fingerprinted, inferring, inside, manifested, runner, spawned, stamped, type Verbs, within } from '@rasm/nx/inference';
import { Array, Cause, Config, Context, Effect, FileSystem, Glob, Layer, Option, Predicate, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import which from 'which';
import { Described, ENTITLEMENTS, Exclusions, INFO, MANIFEST, PACKAGED, type Package, packageEdges, packageParts, RESOLVED, Shipped, sources } from '#/swift-package';
import { analyzer, BUILDER, CORE, Options, type Part, PLUGIN, tokened } from '#/targets';
import { Analyzed, xcodeParts } from '#/xcode-project';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly projects: readonly { readonly file: string; readonly technology: string; readonly project: ProjectConfiguration; readonly tools: Verbs }[];
    readonly edges: readonly RawProjectGraphDependency[];
    readonly buildFiles: Readonly<Record<string, readonly string[]>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns(['*.xcodeproj/project.xcproj', '*.xcodeproj/project.pbxproj', '*.xcworkspace/contents.xcworkspacedata'].map((file) => `**/${file}`));
const MANIFESTS = combineGlobPatterns([MANIFEST, 'Package@swift-*.swift'].map((file) => `**/${file}`));
const READ = [FILES, MANIFESTS, ...['*.xcscheme', '*.xcconfig', '*.xctestplan', '*.entitlements', '*.plist'].map((file) => `**/${file}`)];
const HOST: NodeJS.Platform = 'darwin';

// --- [MODELS] --------------------------------------------------------------------------

const Stamp = Schema.fromJsonString(Schema.Struct({ sources: Schema.Array(Schema.String), files: Schema.Record(Schema.String, Schema.String) })).annotate({
    description: 'The stamp the mise row nx-apple writes beside the core it builds: the workspace globs of the sources the core compiles, and the Nx hash of each file they matched',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Graphs extends Context.Service<Graphs, Map<string, Graph>>()(`${PLUGIN}/plugin/Graphs`) {
    static readonly layer = Layer.sync(Graphs, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = fingerprinted(import.meta.dirname);

const current = Effect.fnUntraced(function* (workspaceRoot: string, binary: string) {
    const stamp = `${binary}.hash`;
    return yield* FileSystem.FileSystem.use((fs) => fs.readFileString(stamp)).pipe(
        Effect.flatMap(Schema.decodeEffect(Stamp)),
        Effect.matchEffect({
            onFailure: () => Effect.succeed([relative(workspaceRoot, stamp)]),
            onSuccess: (stated) => Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [...stated.sources])).pipe(Effect.map((found) => Array.union(found, Record.keys(stated.files)).filter((file) => stated.files[file] !== hashFile(join(workspaceRoot, file))))),
        }),
        Effect.filterOrFail(Array.isArrayEmpty, (named) => new Cause.UnknownError(named, `the ${CORE} core predates ${named.join(', ')}, ${BUILDER} rebuilds it`)),
    );
});

const joined = (left: Verbs, right: Verbs): Verbs => Record.union(left, right, (base, stated) => Record.union(base, stated, (held, over) => mergeTargetConfigurations(over, held)));

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, files: readonly string[], { developer, compiled, xcconfig }: { readonly developer: Option.Option<string>; readonly compiled: Result.Result<string, string>; readonly xcconfig: Option.Option<string> }) {
    const fs = yield* FileSystem.FileSystem;
    const [info, xcodebuild] = yield* Effect.all([Effect.option(spawned(['swift', '-print-target-info'], { cwd: workspaceRoot })), Effect.option(Effect.flatMap(Effect.fromOption(developer), (path) => fs.stat(join(path, 'usr', 'bin', 'xcodebuild'))))], { concurrency: 'unbounded' });
    const paths = new Set(files);
    const manifests = files.filter((file) => basename(file) === MANIFEST);
    const containers = files.filter((file) => basename(file) === 'project.xcproj' || (basename(file) === 'contents.xcworkspacedata' && extname(dirname(dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => basename(file) === 'project.pbxproj' && !paths.has(join(dirname(file), 'project.xcproj')));
    const markers = new Map(containers.map((file) => [dirname(file), file] as const));
    const marker = (container: string) => markers.get(container) ?? container;
    const [listed, unreadable] = yield* Effect.partition(
        Option.isSome(info) ? manifests : [],
        (file) =>
            spawned(['swift', 'package', '--package-path', dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }).pipe(
                Effect.flatMap(Schema.decodeEffect(Described)),
                Effect.mapBoth({ onFailure: (error): [string, Error] => [file, error], onSuccess: (manifest) => ({ file, root: dirname(file), manifest }) }),
            ),
        { concurrency: 'unbounded' },
    );
    const packages = yield* Effect.forEach(
        listed,
        Effect.fnUntraced(function* (swiftPackage: Pick<Package, 'file' | 'root' | 'manifest'>) {
            const held = (named: (module: string) => string) => {
                const expected = swiftPackage.manifest.targets.map((module) => [module.name, join(swiftPackage.root, module.path, named(module.name))] as const);
                return Effect.filter(expected, ([, at]) => fs.exists(join(workspaceRoot, at))).pipe(Effect.map(Record.fromEntries));
            };
            const [plists, entitled] = yield* Effect.all([held(() => INFO), held((module) => `${module}${ENTITLEMENTS}`)]);
            const { files: shipped } = yield* fs.readFileString(join(workspaceRoot, swiftPackage.root, PACKAGED)).pipe(
                Effect.flatMap(Schema.decodeEffect(Shipped)),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed({ files: [] })),
            );
            const swift = (...argv: readonly string[]) => spawned(['swift', ...argv, '--package-path', swiftPackage.root, '--experimental-skip-acquiring-lock'], { cwd: workspaceRoot });
            const shown = (...argv: readonly string[]) => Effect.map(swift(...argv), (at) => relative(workspaceRoot, at.trim()));
            const { exclusions, ...bins } = yield* Effect.all(
                {
                    debug: shown('build', '--show-bin-path'),
                    release: shown('build', '--configuration', 'release', '--show-bin-path'),
                    exclusions: swift('package', 'dump-package').pipe(Effect.flatMap(Schema.decodeEffect(Exclusions)), Effect.map(Struct.get('targets'))),
                },
                { concurrency: 'unbounded' },
            );
            return { ...swiftPackage, bins, plists, entitled, exclusions, shipped } satisfies Package;
        }),
    );
    const described = new Map(packages.map((swiftPackage) => [resolve(workspaceRoot, swiftPackage.root), swiftPackage] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(resolve(workspaceRoot, at)));
    const analysis = yield* Effect.fromOption(xcodebuild, () => 'xcodebuild is absent on this host').pipe(
        Effect.andThen(Effect.fromResult(compiled)),
        Effect.flatMap((at) => spawned([at, 'analyze', ...containers], { cwd: workspaceRoot })),
        Effect.flatMap(Schema.decodeEffect(Analyzed)),
        Effect.catchTag('SchemaError', ({ message }) => Effect.fail(`the ${CORE} core reports in a form this plugin does not read, ${BUILDER} rebuilds it: ${message}`)),
        Effect.result,
        Effect.when(Effect.succeed(Array.isReadonlyArrayNonEmpty(containers))),
    );
    const [reasons, errors] = Array.partition(Option.toArray(Option.flatMap(analysis, Result.getFailure)), (failure) => (Predicate.isString(failure) ? Result.succeed(failure) : Result.fail(failure)));
    const analyzed = Option.getOrElse(Option.flatMap(analysis, Result.getSuccess), () => Analyzed.make({ containers: [], failures: {} }));
    const workspaces = analyzed.containers.filter((container) => basename(marker(container.path)) === 'contents.xcworkspacedata');
    const built = (at: string) => {
        const owners = workspaces.filter((workspace) => workspace.projects.includes(at));
        return Predicate.isTupleOf(owners, 1) ? owners[0].path : at;
    };
    const unread = [...(Option.isNone(info) && Array.isReadonlyArrayNonEmpty(manifests) ? [`No project is inferred from ${manifests.join(', ')}: swift is absent on this host`] : []), ...reasons.map((reason) => `No Xcode target is inferred from ${containers.join(', ')}: ${reason}`)];
    const broken: readonly [string, Error][] = [
        ...unreadable,
        ...errors.flatMap((error) => containers.map((file): [string, Error] => [file, error])),
        ...Record.collect(analyzed.failures, (at, reason): [string, Error] => [marker(at), new Cause.UnknownError(at, reason)]),
        ...unsupported.map((file): [string, Error] => [file, new Cause.UnknownError(file, `${file} has no reader, and no project.xcproj sits beside it`)]),
    ];
    const nested = packages
        .flatMap((owner) => owner.manifest.targets.map((module) => ({ owner: owner.file, folder: join(owner.root, module.path) })))
        .flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && inside(folder)(dirname(other))).map((inner) => `${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`));
    yield* Effect.forEach([...unread, ...nested], (warning) => Effect.sync(() => emitPluginWorkerLog('warn', warning)), { discard: true });
    const configured = Option.toArray(xcconfig).flatMap((at) => tokened(relative(workspaceRoot, at)));
    const linkedSources = (container: Analyzed['containers'][number]) => container.projects.flatMap((at) => Option.match(local(at), { onNone: () => [], onSome: (swiftPackage) => sources(swiftPackage, described, Record.get(container.products, at)) })).filter((input) => !input.endsWith(RESOLVED));
    const titled = (container: Analyzed['containers'][number]) => {
        const packaging = extname(container.path) === '.xcworkspace' ? Array.dedupe(container.schemes.filter((scheme) => scheme.containers.some((at) => Option.isSome(local(at)))).map(Struct.get('name'))) : [];
        return Predicate.isTupleOf(packaging, 1) ? packaging[0] : basename(container.path, extname(container.path));
    };
    const owning = new Map(analyzed.containers.flatMap((container) => Array.getSomes(container.schemes.flatMap(Struct.get('containers')).map(local)).map((swiftPackage) => [swiftPackage, container] as const)));
    const packaged = (at: string) => Option.map(local(at), (swiftPackage) => UndefinedOr.map(owning.get(swiftPackage), titled) ?? swiftPackage.manifest.name);
    const xcode = new Map(
        analyzed.containers.map(
            (container) => [container, xcodeParts(options, marker(container.path), container, titled(container), Array.dedupe([...container.inputs.flatMap(tokened), ...configured, ...linkedSources(container)]), built(container.path), packaged, relative(workspaceRoot, workspaceDataDirectory))] as const,
        ),
    );
    const covered = new Map(owning.entries().flatMap(([swiftPackage, container]) => Array.fromNullishOr(xcode.get(container)).map((part) => [swiftPackage, part] as const)));
    const parts: readonly Part[] = [...packages.flatMap((swiftPackage) => packageParts(options, swiftPackage, described, covered)), ...xcode.values()];
    const [settled, conflicts] = Record.partition(Array.groupBy(parts, Struct.get('root')), (group) => (new Set(group.map(Struct.get('name'))).size > 1 ? Result.fail(group) : Result.succeed(group)));
    const merged = Record.collect(settled, (root, group) => {
        const { name, file, technologies } = Array.headNonEmpty(group);
        const release = Array.findFirst(group, (part) => Option.fromUndefinedOr(part.release));
        const project: ProjectConfiguration = {
            root,
            name,
            metadata: { technologies: Array.dedupe(group.flatMap((part) => part.technologies)) },
            tags: [...Option.toArray(Option.as(release, 'release')), `os:${HOST}`],
            ...Option.getOrUndefined(Option.bindTo(release, 'release')),
            targets: Record.getSomes({ 'nx-release-publish': Array.findFirst(group, (part) => Option.fromUndefinedOr(part.published)) }),
        };
        return { file, technology: Array.headNonEmpty(technologies), project, tools: Array.map(group, (part) => part.tools).reduce(joined) };
    });
    const roots = new Map(merged.map(({ project }) => [project.name, project.root] as const));
    return {
        graph: {
            projects: merged,
            edges: Array.getSomes(packages.flatMap((swiftPackage) => packageEdges(swiftPackage, described, covered)).map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(roots.get(edge.source)), target: Option.fromUndefinedOr(roots.get(edge.target)) }), (at) => ({ ...edge, ...at })))),
            buildFiles: Record.fromIterableWith(xcode, ([container, part]) => [part.root, container.buildFiles]),
        } satisfies Graph,
        failures: [...broken, ...Record.collect(conflicts, (root, group): [string, Error] => [Array.headNonEmpty(group).file, new Cause.UnknownError(group, `${root} holds projects named ${group.map((part) => part.name).join(' and ')}`)])],
        partial: unread.length > 0,
    };
});

const produced = (projects: Readonly<Record<string, ProjectConfiguration>>, built: Array.NonEmptyReadonlyArray<{ readonly source: string; readonly at: string }>) => {
    const declared = Record.collect(projects, (name, data) =>
        Record.collect(
            Record.filter(data.targets ?? {}, ({ outputs }) => outputs !== undefined),
            (target, { configurations = {} }) => ({ name, data, target, configurations }),
        ),
    ).flat();
    const outputs = declared.flatMap(({ name, data, target, configurations }) =>
        [{}, ...Record.keys(configurations).map((configuration) => ({ configuration }))]
            .flatMap((selected) => getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data }))
            .filter((output) => !output.startsWith('!'))
            .map((output) => ({ name, output, covering: Glob.make(`${output}{,/**}`, { dot: true }) })),
    );
    return built.flatMap(({ source, at }) => Array.dedupe(outputs.filter(({ output, covering }) => Glob.match(covering, at) || inside(at)(output)).map(Struct.get('name'))).map((target): RawProjectGraphDependency => ({ source, target, type: DependencyType.implicit })));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Graphs.layer);

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (matched, settings, { workspaceRoot }) {
        const files = process.platform === HOST ? [...matched, ...(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [MANIFESTS])))].toSorted() : [];
        const [hashed, developer, swift, compiled] = yield* Effect.all(
            [
                own,
                process.platform === HOST
                    ? Effect.option(Effect.firstSuccessOf([Config.String('DEVELOPER_DIR'), FileSystem.FileSystem.use((fs) => fs.readLink('/var/db/xcode_select_link')), Effect.map(spawned(['xcode-select', '--print-path'], { cwd: workspaceRoot }), (at) => at.trim())]))
                    : Effect.succeed(Option.none<string>()),
                Effect.promise(() => which('swift', { nothrow: true })).pipe(Effect.map(Option.fromNullOr)),
                Effect.result(Effect.mapError(analyzer, ({ message }) => message)),
            ],
            { concurrency: 'unbounded' },
        );
        yield* Effect.forEach(Option.toArray(Option.filter(Result.getSuccess(compiled), (binary) => process.platform === HOST && within(relative(workspaceRoot, binary)))), (binary) => current(workspaceRoot, binary), { discard: true });
        const slot = join(workspaceDataDirectory, 'apple.hash');
        const xcconfig = yield* Config.option(Config.String('XCODE_XCCONFIG_FILE'));
        const stamps = [
            ...Option.toArray(xcconfig),
            ...Option.toArray(developer),
            ...Option.toArray(Option.flatMapNullishOr(developer, (path) => hashFile(join(path, '..', 'version.plist')))),
            ...[...Option.toArray(swift), ...Option.toArray(Result.getSuccess(compiled))].flatMap((file) => [file, ...Array.fromNullishOr(hashFile(file))]),
            ...Option.toArray(yield* Config.option(Config.String('TOOLCHAINS'))),
            ...files.filter((file) => basename(file) === MANIFEST).flatMap((file) => Array.fromNullishOr(hashFile(join(workspaceRoot, dirname(file), PACKAGED)))),
        ];
        const digest = hashArray([hashObject(settings), hashed, yield* stamped(workspaceRoot)(READ), ...stamps, ...files]);
        const stored = Option.filter(Option.fromNullishOr(new PluginCache<{ readonly digest: string; readonly graph: Graph }>(slot).get(FILES)), (held) => held.digest === digest);
        const read = yield* Option.match(stored, {
            onSome: ({ graph }) => Effect.succeed({ graph, failures: [], partial: false }),
            onNone: () => Effect.tap(analyze(workspaceRoot, settings, files, { developer, compiled, xcconfig }), ({ graph, failures, partial }) => (failures.length === 0 && !partial ? Effect.sync(() => new PluginCache(slot, { [FILES]: { digest, graph } }).writeToDisk()) : Effect.void)),
        });
        (yield* Graphs).set(workspaceRoot, read.graph);
        const anchor = anchored(workspaceRoot);
        const [placed, unread] = yield* Effect.partition(read.graph.projects, ({ file, technology, project, tools }) =>
            Effect.mapBoth(manifested(workspaceRoot, project.root), {
                onFailure: (error): [string, Error] => [file, error],
                onSuccess: (stated) => ({ file, project: { ...project, targets: aggregated(tools, stated, technology, anchor, project.targets) } }),
            }),
        );
        const results = Record.collect(Array.groupBy(placed, Struct.get('file')), (file, group) => [file, { projects: Record.fromIterableWith(group, ({ project }) => [project.root, project] as const) }] as const);
        const failures = [...read.failures, ...unread];
        return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
    }),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Graphs, (graphs) => {
            const { edges = [], buildFiles = {} } = graphs.get(workspaceRoot) ?? {};
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const built = Record.collect(buildFiles, (root, files) => Array.cartesianWith(Array.fromNullishOr(names.get(root)), files, (source, at) => ({ source, at }))).flat();
            return [...Array.getSomes(edges.map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(edge.source)), target: Option.fromUndefinedOr(names.get(edge.target)) }), (named) => ({ ...edge, ...named })))), ...(Array.isReadonlyArrayNonEmpty(built) ? produced(projects, built) : [])];
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
