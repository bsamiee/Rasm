import { basename, dirname, extname, join, matchesGlob, relative, resolve } from 'node:path';
import process from 'node:process';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, DependencyType, getOutputsForTargetAndConfiguration, hashArray, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, emitPluginWorkerLog, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, fingerprinted, inferring, manifested, runner, spawned, type Verbs } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Context, Effect, FileSystem, flow, Layer, Option, Record, Result, Schema, Stream } from 'effect';
import { Described, ENTITLEMENTS, INFO, MANIFEST, type Package, packageEdges, packageParts, RESOLVED, sources } from '#/swift-package';
import { analyzer, Options, type Part, PLUGIN, tokened } from '#/targets';
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
const TAGS = { release: 'release', platform: 'os:' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ platform: Schema.String }) }));
const Listed = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [SERVICES] ------------------------------------------------------------------------

class Graphs extends Context.Service<Graphs, Map<string, Graph>>()(`${PLUGIN}/plugin/Graphs`) {
    static readonly layer = Layer.sync(Graphs, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const own = fingerprinted(import.meta.dirname);

const joined = (left: Verbs, right: Verbs): Verbs => Record.union(left, right, (base, stated) => Record.union(base, stated, (held, over) => mergeTargetConfigurations(over, held)));

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, files: readonly string[], { info, xcodebuild, compiled }: Readonly<Record<'info' | 'xcodebuild' | 'compiled', Option.Option<string>>>) {
    const fs = yield* FileSystem.FileSystem;
    const paths = new Set(files);
    const manifests = files.filter((file) => basename(file) === MANIFEST);
    const containers = files.filter((file) => basename(file) === 'project.xcproj' || (basename(file) === 'contents.xcworkspacedata' && extname(dirname(dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => basename(file) === 'project.pbxproj' && !paths.has(join(dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => dirname(file) === container) ?? container;
    const host = yield* Effect.transposeOption(Option.map(info, (text) => Effect.map(Schema.decodeEffect(Host)(text), ({ target: { platform } }) => platform)));
    const reader = Option.filter(
        Option.map(Option.all({ xcodebuild, compiled }), ({ compiled: at }) => at),
        () => containers.length > 0,
    );
    const readable = Option.isSome(info) ? manifests : [];
    const absent = [...(Option.isNone(info) ? ['swift'] : []), ...(containers.length > 0 && Option.isNone(xcodebuild) ? ['xcodebuild'] : []), ...(containers.length > 0 && Option.isNone(compiled) ? ['the @rasm/nx-apple-darwin-arm64 core'] : [])];
    const describedResults = yield* Effect.forEach(readable, (file) => Effect.result(Effect.flatMap(spawned(['swift', 'package', '--package-path', dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }), Schema.decodeEffect(Described))), {
        concurrency: 'unbounded',
    });
    const packages = yield* Effect.forEach(
        Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(result, (manifest) => ({ file, root: dirname(file), manifest }))),
        Effect.fnUntraced(function* (swiftPackage: Omit<Package, 'plists' | 'entitled' | 'bins' | 'whole'>) {
            const held = (named: (module: string) => string) =>
                Effect.map(
                    Effect.filter(
                        swiftPackage.manifest.targets.map((module) => [module.name, join(swiftPackage.root, module.path, named(module.name))] as const),
                        ([, at]) => fs.exists(join(workspaceRoot, at)),
                    ),
                    (found) => new Map(found),
                );
            const [plists, entitled] = yield* Effect.all([held(() => INFO), held((module) => `${module}${ENTITLEMENTS}`)]);
            const { products, targets } = swiftPackage.manifest;
            const shown = (...argv: readonly string[]) => Effect.map(spawned(['swift', ...argv, '--package-path', swiftPackage.root, '--experimental-skip-acquiring-lock'], { cwd: workspaceRoot }), (at) => relative(workspaceRoot, at.trim()));
            const whole = Array.isReadonlyArrayNonEmpty(products) && products.every(({ type }) => 'executable' in type) && targets.every(({ resources }) => resources.length === 0);
            const bins = yield* Effect.all({ debug: shown('build', '--show-bin-path'), release: shown('build', '--configuration', 'release', '--show-bin-path'), codecov: shown('test', '--show-codecov-path') }, { concurrency: 'unbounded' });
            return { ...swiftPackage, bins, whole, plists, entitled } satisfies Package;
        }),
    );
    const described = new Map(packages.map((swiftPackage) => [resolve(workspaceRoot, swiftPackage.root), swiftPackage] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(resolve(workspaceRoot, at)));
    const analysis = yield* Option.match(reader, {
        onNone: () => Effect.succeed(Result.succeed(Analyzed.make({ containers: [], failures: {} }))),
        onSome: (at) => Effect.flatMap(Schema.encodeEffect(Listed)(containers), (input) => spawned([at], { cwd: workspaceRoot, stdin: Stream.encodeText(Stream.make(input)) })).pipe(Effect.flatMap(Schema.decodeEffect(Analyzed)), Effect.result),
    });
    const analyzed = Result.getOrElse(analysis, () => Analyzed.make({ containers: [], failures: {} }));
    const workspaces = analyzed.containers.filter((container) => basename(marker(container.path)) === 'contents.xcworkspacedata');
    const built = (at: string) => {
        const owners = workspaces.filter((workspace) => workspace.projects.includes(at)).map((workspace) => workspace.path);
        return Option.getOrElse(Array.head(owners.length === 1 ? owners : []), () => at);
    };
    const unread = [...(Option.isNone(info) ? manifests.map((file) => ({ file, reason: 'swift is absent on this host' })) : []), ...(Option.isNone(reader) ? containers.map((file) => ({ file, reason: `${absent.join(', ')} absent on this host` })) : [])];
    const broken: readonly [string, Error][] = [
        ...Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(Result.flip(result), (error): [string, Error] => [file, error])),
        ...(Result.isFailure(analysis) ? containers.map((file): [string, Error] => [file, analysis.failure]) : []),
        ...Record.collect(analyzed.failures, (at, reason): [string, Error] => [marker(at), new Cause.UnknownError(at, reason)]),
        ...unsupported.map((file): [string, Error] => [file, new Cause.UnknownError(file, `${file} has no reader, and no project.xcproj sits beside it`)]),
    ];
    yield* Effect.forEach(unread, ({ file, reason }) => Effect.sync(() => emitPluginWorkerLog('warn', `${file} presents no project: ${reason}`)));
    const packaged = flow(
        local,
        Option.map((swiftPackage) => swiftPackage.manifest.name),
    );
    const platforms = (at: string) => Option.match(local(at), { onNone: () => [], onSome: (swiftPackage) => swiftPackage.manifest.platforms.map((platform) => platform.name) });
    const destinations = (folders: readonly string[]) => Array.dedupe(folders.flatMap((folder) => (platforms(folder).length === 0 ? Option.toArray(host) : platforms(folder))));
    const nested = packages.flatMap((owner) => owner.manifest.targets.map((module) => ({ owner: owner.file, folder: `${join(owner.root, module.path)}/` }))).flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && `${dirname(other)}/`.startsWith(folder)).map((inner) => ({ owner, inner })));
    yield* Effect.forEach(nested, ({ owner, inner }) => Effect.sync(() => emitPluginWorkerLog('warn', `${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`)));
    const linkedSources = (container: Analyzed['containers'][number]) => Array.getSomes(container.projects.map(local)).flatMap((swiftPackage) => sources(swiftPackage, described));
    const parts: readonly Part[] = [
        ...packages.flatMap((swiftPackage) => packageParts(options, swiftPackage, described)),
        ...analyzed.containers.map((container) =>
            xcodeParts(options, marker(container.path), container, Array.dedupe([...container.inputs.flatMap(tokened), ...linkedSources(container).filter((input) => !input.endsWith(RESOLVED))]), built(container.path), destinations, packaged, relative(workspaceRoot, workspaceDataDirectory)),
        ),
    ];
    const grouped = Array.groupBy(parts, (part) => part.root);
    const conflicts = Record.filter(grouped, (group) => new Set(group.map((part) => part.name)).size > 1);
    const merged = Record.collect(
        Record.filter(grouped, (_, root) => !(root in conflicts)),
        (root, group) => {
            const { name, file, technologies } = Array.headNonEmpty(group);
            const release = Array.findFirst(group, (part) => Option.fromUndefinedOr(part.release));
            const project: ProjectConfiguration = {
                root,
                name,
                metadata: { technologies: Array.dedupe(group.flatMap((part) => part.technologies)) },
                tags: [...Option.toArray(Option.as(release, TAGS.release)), `${TAGS.platform}${HOST}`],
                ...Option.getOrUndefined(Option.map(release, (held) => ({ release: held }))),
                targets: Record.getSomes({ 'nx-release-publish': Array.findFirst(group, (part) => Option.fromUndefinedOr(part.published)) }),
            };
            return { file, technology: Array.headNonEmpty(technologies), project, tools: Array.map(group, (part) => part.tools).reduce(joined) };
        },
    );
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
        const [hashed, developer, swift, compiled] = yield* Effect.all([own, Effect.option(spawned(['xcode-select', '--print-path'], { cwd: workspaceRoot })), Effect.option(spawned(['which', 'swift'], { cwd: workspaceRoot })), Effect.option(Effect.tap(analyzer, (at) => fs.stat(at)))], { concurrency: 'unbounded' });
        const cache = new PluginCache<Graph>(join(workspaceDataDirectory, 'apple.hash'));
        const stamps = [
            ...Option.toArray(developer),
            ...Option.toArray(Option.flatMap(developer, (path) => Option.fromNullishOr(hashFile(join(path.trim(), '..', 'version.plist'))))),
            ...[...Option.toArray(swift), ...Option.toArray(compiled)].flatMap((file) => [file, ...Array.fromNullishOr(hashFile(file.trim()))]),
            ...Option.toArray(yield* Config.option(Config.String('TOOLCHAINS'))),
        ];
        const digest = hashArray([hashObject(settings), hashed, yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, READ)), ...stamps, ...files]);
        const stored = Option.fromNullishOr(cache.get(digest));
        const read = yield* Option.match(stored, {
            onSome: (graph) => Effect.succeed({ graph, failures: [], partial: false }),
            onNone: () =>
                Effect.flatMap(
                    Effect.all(
                        {
                            info: Effect.option(spawned(['swift', '-print-target-info'], { cwd: workspaceRoot })),
                            xcodebuild: Effect.option(
                                Effect.tap(
                                    Effect.map(Effect.fromOption(developer), (path) => join(path.trim(), 'usr', 'bin', 'xcodebuild')),
                                    (at) => fs.stat(at),
                                ),
                            ),
                        },
                        { concurrency: 'unbounded' },
                    ),
                    (probed) => analyze(workspaceRoot, settings, files, { ...probed, compiled }),
                ),
        });
        yield* Option.isNone(stored) && read.failures.length === 0 && !read.partial
            ? Effect.sync(() => {
                  cache.set(digest, read.graph);
                  cache.writeToDisk();
              })
            : Effect.void;
        (yield* Graphs).set(workspaceRoot, read.graph);
        const [placed, unread] = yield* Effect.partition(read.graph.projects, ({ file, technology, project, tools }) =>
            Effect.mapBoth(manifested(workspaceRoot, project.root), {
                onFailure: (error): [string, Error] => [file, error],
                onSuccess: (stated) => ({ file, project: { ...project, targets: { ...aggregated(tools, stated, technology), ...project.targets } } }),
            }),
        );
        const results = Record.collect(
            Array.groupBy(placed, ({ file }) => file),
            (file, group) => [file, { projects: Record.fromIterableWith(group, ({ project }) => [project.root, project] as const) }] as const,
        );
        const failures = [...read.failures, ...unread];
        return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
    }),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Graphs, (graphs) => {
            const { edges = [], buildFiles = {} } = graphs.get(workspaceRoot) ?? {};
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            const built = Record.collect(buildFiles, (root, files) => files.map((at) => ({ root, at }))).flat();
            const declared = Record.collect(projects, (name, data) =>
                Record.collect(
                    Record.filter(data.targets ?? {}, ({ outputs }) => outputs !== undefined),
                    (target, { configurations = {} }) => ({ name, data, target, configurations }),
                ),
            ).flat();
            const produced = declared.flatMap(({ name, data, target, configurations }) =>
                [{}, ...Record.keys(configurations).map((configuration) => ({ configuration }))]
                    .flatMap((selected) => getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data }))
                    .filter((output) => !output.startsWith('!'))
                    .map((output) => ({ root: data.root, output })),
            );
            const implied = Array.cartesian(built, produced)
                .filter(([{ at }, { output }]) => matchesGlob(at, output) || matchesGlob(at, join(output, '**')) || !relative(at, output).startsWith('..'))
                .map(([{ root }, { root: producer }]): RawProjectGraphDependency => ({ source: root, target: producer, type: DependencyType.implicit }));
            return Array.getSomes([...edges, ...implied].map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(edge.source)), target: Option.fromUndefinedOr(names.get(edge.target)) }), (named) => ({ ...edge, ...named }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
