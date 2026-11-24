import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import process from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, DependencyType, hashArray, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, emitPluginWorkerLog, hashFile, hashObject, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, fingerprinted, inferring, manifested, produced, runner, spawned, stamped, type Verbs, within } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Context, Effect, FileSystem, identity, Layer, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import which from 'which';
import { Described, ENTITLEMENTS, INFO, MANIFEST, PACKAGED, type Package, packageEdges, packageParts, RESOLVED, Shipped, sources } from '#/swift-package';
import { analyzer, CORE, core, Options, type Part, PLUGIN, tokened } from '#/targets';
import { Analyzed, xcodeParts } from '#/xcode-project';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly projects: readonly { readonly file: string; readonly technology: string; readonly project: ProjectConfiguration; readonly tools: Verbs }[];
    readonly edges: readonly RawProjectGraphDependency[];
    readonly buildFiles: Readonly<Record<string, readonly string[]>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [MANIFEST, INFO, 'Package@swift-*.swift', '*.xcodeproj/project.xcproj', '*.xcworkspace/contents.xcworkspacedata', '*.xcscheme', '*.xcconfig'].map((file) => `**/${file}`);
const FILES = combineGlobPatterns([...READ, ...['*.xcodeproj/project.pbxproj', '*.xctestplan', '*.entitlements', '*.plist'].map((file) => `**/${file}`)]);
const HOST: NodeJS.Platform = 'darwin';

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ platform: Schema.String }) }));

// --- [SERVICES] ------------------------------------------------------------------------

class Graphs extends Context.Service<Graphs, Map<string, Graph>>()(`${PLUGIN}/plugin/Graphs`) {
    static readonly layer = Layer.sync(Graphs, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = fingerprinted(import.meta.dirname);

const joined = (left: Verbs, right: Verbs): Verbs => Record.union(left, right, (base, stated) => Record.union(base, stated, (held, over) => mergeTargetConfigurations(over, held)));

const analyze = Effect.fnUntraced(function* (
    workspaceRoot: string,
    options: Options,
    files: readonly string[],
    { developer, compiled, stamp }: { readonly developer: Option.Option<string>; readonly compiled: Result.Result<string, string>; readonly stamp: Option.Option<Readonly<Record<'root' | 'name' | 'hash', string>>> },
) {
    const fs = yield* FileSystem.FileSystem;
    const [info, xcodebuild] = yield* Effect.all([Effect.option(spawned(['swift', '-print-target-info'], { cwd: workspaceRoot })), Effect.option(Effect.flatMap(Effect.fromOption(developer), (path) => fs.stat(join(path, 'usr', 'bin', 'xcodebuild'))))], { concurrency: 'unbounded' });
    const paths = new Set(files);
    const manifests = files.filter((file) => basename(file) === MANIFEST);
    const containers = files.filter((file) => basename(file) === 'project.xcproj' || (basename(file) === 'contents.xcworkspacedata' && extname(dirname(dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => basename(file) === 'project.pbxproj' && !paths.has(join(dirname(file), 'project.xcproj')));
    const markers = new Map(containers.map((file) => [dirname(file), file] as const));
    const marker = (container: string) => markers.get(container) ?? container;
    const host = yield* Effect.transposeOption(Option.map(info, (text) => Effect.map(Schema.decodeEffect(Host)(text), ({ target: { platform } }) => platform)));
    const readable = Option.isSome(info) ? manifests : [];
    const describedResults = yield* Effect.forEach(readable, (file) => Effect.result(Effect.flatMap(spawned(['swift', 'package', '--package-path', dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }), Schema.decodeEffect(Described))), {
        concurrency: 'unbounded',
    });
    const packages = yield* Effect.forEach(
        Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(result, (manifest) => ({ file, root: dirname(file), manifest }))),
        Effect.fnUntraced(function* (swiftPackage: Pick<Package, 'file' | 'root' | 'manifest'>) {
            const held = (named: (module: string) => string) =>
                Effect.map(
                    Effect.filter(
                        swiftPackage.manifest.targets.map((module) => [module.name, join(swiftPackage.root, module.path, named(module.name))] as const),
                        ([, at]) => fs.exists(join(workspaceRoot, at)),
                    ),
                    (found) => new Map(found),
                );
            const [plists, entitled] = yield* Effect.all([held(() => INFO), held((module) => `${module}${ENTITLEMENTS}`)]);
            const { files: shipped } = yield* fs.readFileString(join(workspaceRoot, swiftPackage.root, PACKAGED)).pipe(
                Effect.flatMap(Schema.decodeEffect(Shipped)),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed({ files: [] })),
            );
            const { products, targets } = swiftPackage.manifest;
            const shown = (...argv: readonly string[]) => Effect.map(spawned(['swift', ...argv, '--package-path', swiftPackage.root, '--experimental-skip-acquiring-lock'], { cwd: workspaceRoot }), (at) => relative(workspaceRoot, at.trim()));
            const whole = Array.isReadonlyArrayNonEmpty(products) && products.every(({ type }) => 'executable' in type) && targets.every(({ resources }) => resources.length === 0);
            const bins = yield* Effect.all({ debug: shown('build', '--show-bin-path'), release: shown('build', '--configuration', 'release', '--show-bin-path'), codecov: shown('test', '--show-codecov-path') }, { concurrency: 'unbounded' });
            return { ...swiftPackage, bins, whole, plists, entitled, shipped } satisfies Package;
        }),
    );
    const described = new Map(packages.map((swiftPackage) => [resolve(workspaceRoot, swiftPackage.root), swiftPackage] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(resolve(workspaceRoot, at)));
    const analysis = yield* Effect.transposeOption(
        Option.map(Option.liftPredicate(containers, Array.isReadonlyArrayNonEmpty), (listed) =>
            Effect.fromOption(xcodebuild, () => 'xcodebuild is absent on this host').pipe(
                Effect.andThen(Effect.fromResult(compiled)),
                Effect.flatMap((at) => spawned([at, 'analyze', ...listed], { cwd: workspaceRoot })),
                Effect.flatMap(Schema.decodeEffect(Analyzed)),
                Effect.catchTag('SchemaError', ({ message }) => Effect.fail(`the ${CORE} core reports in a form this plugin does not read, nx build ${CORE} rebuilds it: ${message}`)),
                Effect.result,
            ),
        ),
    );
    const [reasons, errors] = Array.partition(Option.toArray(Option.flatMap(analysis, Result.getFailure)), (failure) => (Predicate.isString(failure) ? Result.succeed(failure) : Result.fail(failure)));
    const analyzed = Option.getOrElse(Option.flatMap(analysis, Result.getSuccess), () => Analyzed.make({ containers: [], failures: {} }));
    const workspaces = analyzed.containers.filter((container) => basename(marker(container.path)) === 'contents.xcworkspacedata');
    const built = (at: string) => {
        const owners = workspaces.filter((workspace) => workspace.projects.includes(at)).map((workspace) => workspace.path);
        return Option.getOrElse(Array.head(owners.length === 1 ? owners : []), () => at);
    };
    const unread = [...(Option.isNone(info) && Array.isReadonlyArrayNonEmpty(manifests) ? [`No project is inferred from ${manifests.join(', ')}: swift is absent on this host`] : []), ...reasons.map((reason) => `No Xcode target is inferred from ${containers.join(', ')}: ${reason}`)];
    const broken: readonly [string, Error][] = [
        ...Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(Result.flip(result), (error): [string, Error] => [file, error])),
        ...errors.flatMap((error) => containers.map((file): [string, Error] => [file, error])),
        ...Record.collect(analyzed.failures, (at, reason): [string, Error] => [marker(at), new Cause.UnknownError(at, reason)]),
        ...unsupported.map((file): [string, Error] => [file, new Cause.UnknownError(file, `${file} has no reader, and no project.xcproj sits beside it`)]),
    ];
    yield* Effect.forEach(unread, (warning) => Effect.sync(() => emitPluginWorkerLog('warn', warning)));
    const packaged = (at: string) => Option.map(local(at), ({ manifest }) => manifest.name);
    const destinations = (folders: readonly string[]) => Array.dedupe(folders.flatMap((folder) => Array.match(Option.match(local(folder), { onNone: () => [], onSome: ({ manifest }) => manifest.platforms.map(Struct.get('name')) }), { onEmpty: () => Option.toArray(host), onNonEmpty: identity })));
    const nested = packages.flatMap((owner) => owner.manifest.targets.map((module) => ({ owner: owner.file, folder: `${join(owner.root, module.path)}/` }))).flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && `${dirname(other)}/`.startsWith(folder)).map((inner) => ({ owner, inner })));
    yield* Effect.forEach(nested, ({ owner, inner }) => Effect.sync(() => emitPluginWorkerLog('warn', `${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`)));
    const linkedSources = (container: Analyzed['containers'][number]) => Array.getSomes(container.projects.map((at) => Option.map(local(at), (swiftPackage) => sources(swiftPackage, described, Record.get(container.products, at))))).flat();
    const parts: readonly Part[] = [
        ...packages.flatMap((swiftPackage) => packageParts(options, swiftPackage, described, stamp)),
        ...analyzed.containers.map((container) =>
            xcodeParts(options, marker(container.path), container, Array.dedupe([...container.inputs.flatMap(tokened), ...linkedSources(container).filter((input) => !input.endsWith(RESOLVED))]), built(container.path), destinations, packaged, relative(workspaceRoot, workspaceDataDirectory)),
        ),
    ];
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
            edges: Array.getSomes(packages.flatMap((swiftPackage) => packageEdges(swiftPackage, described)).map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(roots.get(edge.source)), target: Option.fromUndefinedOr(roots.get(edge.target)) }), (at) => ({ ...edge, ...at })))),
            buildFiles: Record.fromIterableWith(analyzed.containers, (container) => [dirname(container.path), container.buildFiles]),
        } satisfies Graph,
        failures: [...broken, ...Record.collect(conflicts, (root, group): [string, Error] => [Array.headNonEmpty(group).file, new Cause.UnknownError(group, `${root} holds projects named ${group.map((part) => part.name).join(' and ')}`)])],
        partial: unread.length > 0,
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Graphs.layer);

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (matched, settings, { workspaceRoot }) {
        const [files, fs] = [process.platform === HOST ? matched.toSorted() : [], yield* FileSystem.FileSystem];
        const sourced = Effect.gen(function* () {
            const { manifest, binary } = yield* core;
            const root = yield* Effect.fromOption(Option.liftPredicate(relative(workspaceRoot, dirname(manifest)), within));
            return { root, name: `${basename(binary)}.hash`, hash: yield* stamped(workspaceRoot)([join(root, '**/*')]) };
        });
        const [hashed, developer, swift, stamp] = yield* Effect.all(
            [
                own,
                process.platform === HOST ? Effect.option(Effect.firstSuccessOf([Config.String('DEVELOPER_DIR'), fs.readLink('/var/db/xcode_select_link'), Effect.map(spawned(['xcode-select', '--print-path'], { cwd: workspaceRoot }), (at) => at.trim())])) : Effect.succeed(Option.none<string>()),
                Effect.promise(() => which('swift', { nothrow: true })).pipe(Effect.map(Option.fromNullOr)),
                Effect.option(sourced),
            ],
            { concurrency: 'unbounded' },
        );
        const compiled = yield* Effect.result(
            Effect.tap(
                Effect.mapError(analyzer, ({ message }) => message),
                (at) =>
                    Effect.filterOrFail(
                        Effect.option(fs.readFileString(`${at}.hash`)),
                        (held) => Option.match(stamp, { onNone: () => true, onSome: ({ hash }) => Option.contains(held, hash) }),
                        () => `the ${CORE} core is older than its sources, nx build ${CORE} rebuilds it`,
                    ),
            ),
        );
        const slot = join(workspaceDataDirectory, 'apple.hash');
        const stamps = [
            ...Option.toArray(developer),
            ...Option.toArray(Option.flatMap(developer, (path) => Option.fromNullishOr(hashFile(join(path, '..', 'version.plist'))))),
            ...[...Option.toArray(swift), ...Option.toArray(Result.getSuccess(compiled))].flatMap((file) => [file, ...Array.fromNullishOr(hashFile(file))]),
            ...Option.toArray(Option.map(stamp, ({ hash }) => hash)),
            ...Option.toArray(yield* Config.option(Config.String('TOOLCHAINS'))),
            ...files.filter((file) => basename(file) === MANIFEST).flatMap((file) => Array.fromNullishOr(hashFile(join(workspaceRoot, dirname(file), PACKAGED)))),
        ];
        const digest = hashArray([hashObject(settings), hashed, yield* stamped(workspaceRoot)(READ), ...stamps, ...files]);
        const stored = Option.filter(Option.fromNullishOr(new PluginCache<{ readonly digest: string; readonly graph: Graph }>(slot).get(FILES)), (held) => held.digest === digest);
        const read = yield* Option.match(stored, {
            onSome: ({ graph }) => Effect.succeed({ graph, failures: [], partial: false }),
            onNone: () => analyze(workspaceRoot, settings, files, { developer, compiled, stamp }),
        });
        yield* Option.isNone(stored) && read.failures.length === 0 && !read.partial ? Effect.sync(() => new PluginCache(slot, { [FILES]: { digest, graph: read.graph } }).writeToDisk()) : Effect.void;
        (yield* Graphs).set(workspaceRoot, read.graph);
        const [placed, unread] = yield* Effect.partition(read.graph.projects, ({ file, technology, project, tools }) =>
            Effect.mapBoth(manifested(workspaceRoot, project.root), {
                onFailure: (error): [string, Error] => [file, error],
                onSuccess: (stated) => ({ file, project: { ...project, targets: { ...aggregated(tools, stated, technology), ...project.targets } } }),
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
            const built = Array.getSomes(Record.collect(buildFiles, (root, files) => Option.map(Option.fromUndefinedOr(names.get(root)), (source) => ({ source, files })))).flatMap(({ source, files }) => files.map((at) => ({ source, at })));
            const made: Readonly<Record<string, readonly string[]>> = Array.match(Array.map(built, Struct.get('at')), { onEmpty: () => ({}), onNonEmpty: (files) => produced(projects, files) });
            const implied = built.flatMap(({ source, at }) => (made[at] ?? []).map((target): RawProjectGraphDependency => ({ source, target, type: DependencyType.implicit })));
            return [...Array.getSomes(edges.map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(edge.source)), target: Option.fromUndefinedOr(names.get(edge.target)) }), (named) => ({ ...edge, ...named })))), ...implied];
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
