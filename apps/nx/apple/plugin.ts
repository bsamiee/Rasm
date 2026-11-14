import { tmpdir } from 'node:os';
import { basename, dirname, extname, join, matchesGlob, relative, resolve } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, DependencyType, getOutputsForTargetAndConfiguration, hashArray, type PostTasksExecution, type PreTasksExecution, type ProjectConfiguration, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, emitPluginWorkerLog, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Completed } from '@rasm/process';
import { Array, Config, Context, Effect, FileSystem, flow, Layer, ManagedRuntime, Option, Predicate, Record, Redacted, Result, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { Described, INFO, MANIFEST, type Package, packageEdges, packageParts, RESOLVED, sources } from '#/swift-package';
import { analyzer, Options, type Part, refused, SHARED, tokened } from '#/targets';
import { Analyzed, evaluated, xcodeParts } from '#/xcode-project';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly projects: Readonly<Record<string, Readonly<Record<string, ProjectConfiguration>>>>;
    readonly edges: readonly RawProjectGraphDependency[];
    readonly buildFiles: Readonly<Record<string, readonly string[]>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [MANIFEST, INFO, 'Package@swift-*.swift', '*.xcodeproj/project.xcproj', '*.xcworkspace/contents.xcworkspacedata', '*.xcscheme', '*.xcconfig'].map((file) => `**/${file}`);
const FILES = combineGlobPatterns([...READ, ...['*.xcodeproj/project.pbxproj', '*.xctestplan', '*.entitlements', '*.plist'].map((file) => `**/${file}`)]);
const HOST: NodeJS.Platform = 'darwin';
const IDENTITIES = ['APPLE_DEVELOPMENT_P12', 'DEVELOPER_ID_P12'] as const;
const KEYCHAINS = join(tmpdir(), 'nx-apple');

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ platform: Schema.String }) }));
const Listed = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [SERVICES] ------------------------------------------------------------------------

class Graphs extends Context.Service<Graphs, Map<string, Graph>>()('@rasm/nx-apple/plugin/Graphs') {
    static readonly layer = Layer.sync(Graphs, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const joined = (left: Part['targets'], right: Part['targets']) => Record.union(left, right, (base, stated) => mergeTargetConfigurations(stated, base));

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: typeof Options.Type, files: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const paths = new Set(files);
    const manifests = files.filter((file) => basename(file) === MANIFEST);
    const containers = files.filter((file) => basename(file) === 'project.xcproj' || (basename(file) === 'contents.xcworkspacedata' && extname(dirname(dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => basename(file) === 'project.pbxproj' && !paths.has(join(dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => dirname(file) === container) ?? container;
    const printed = (command: string, args: readonly string[]) => Completed.output(ChildProcess.make(command, args, { cwd: workspaceRoot, stdin: 'ignore' }));
    const [info, xcodebuild, compiled] = yield* Effect.all([
        Effect.option(printed('swift', ['-print-target-info'])),
        containers.length === 0 ? Effect.succeedNone : Effect.option(printed('xcodebuild', ['-version'])),
        containers.length === 0 ? Effect.succeedNone : Effect.option(Effect.tap(analyzer, (at) => fs.stat(at))),
    ]);
    const host = yield* Effect.transposeOption(Option.map(info, (text) => Effect.map(Schema.decodeEffect(Host)(text), ({ target: { platform } }) => platform)));
    const reader = Option.map(Option.all({ xcodebuild, compiled }), ({ compiled: at }) => at);
    const readable = Option.isSome(info) ? manifests : [];
    const absent = [...(Option.isNone(info) ? ['swift'] : []), ...(containers.length > 0 && Option.isNone(xcodebuild) ? ['xcodebuild'] : []), ...(containers.length > 0 && Option.isNone(compiled) ? ['the #analyzer core'] : [])];
    const describedResults = yield* Effect.forEach(readable, (file) => Effect.result(Effect.flatMap(printed('swift', ['package', '--package-path', dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json']), Schema.decodeEffect(Described))), {
        concurrency: 'unbounded',
    });
    const packages = yield* Effect.forEach(
        Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(result, (manifest) => ({ file, root: dirname(file), manifest }))),
        Effect.fnUntraced(function* (swiftPackage: Omit<Package, 'plists'>) {
            const listed = yield* Effect.filter(swiftPackage.manifest.targets, (module) => fs.exists(join(workspaceRoot, swiftPackage.root, module.path, INFO)));
            const { products, targets } = swiftPackage.manifest;
            const shown = (configuration: string) => Effect.map(printed('swift', ['build', '--package-path', swiftPackage.root, '--configuration', configuration, '--show-bin-path']), (at) => relative(workspaceRoot, at.trim()));
            const whole = Array.isReadonlyArrayNonEmpty(products) && products.every(({ type }) => 'executable' in type) && targets.every(({ resources }) => resources.length === 0);
            const bins = whole ? { bins: yield* Effect.all({ debug: shown('debug'), release: shown('release') }, { concurrency: 'unbounded' }) } : {};
            return { ...swiftPackage, ...bins, plists: new Set(listed.map(({ name }) => name)) } satisfies Package;
        }),
    );
    const described = new Map(packages.map((swiftPackage) => [resolve(workspaceRoot, swiftPackage.root), swiftPackage] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(resolve(workspaceRoot, at)));
    const analysis = yield* Option.match(reader, {
        onNone: () => Effect.succeed(Result.succeed<typeof Analyzed.Type>({ containers: [], failures: {} })),
        onSome: (at) =>
            Effect.result(
                Effect.flatMap(
                    Effect.flatMap(Schema.encodeEffect(Listed)(containers), (input) => Completed.output(ChildProcess.make(at, [], { cwd: workspaceRoot, stdin: Stream.encodeText(Stream.succeed(input)) }))),
                    Schema.decodeEffect(Analyzed),
                ),
            ),
    });
    const analyzed = Result.getOrElse(analysis, (): typeof Analyzed.Type => ({ containers: [], failures: {} }));
    const workspaces = analyzed.containers.filter((container) => basename(marker(container.path)) === 'contents.xcworkspacedata');
    const built = (at: string) => {
        const owners = workspaces.filter((workspace) => workspace.projects.includes(at)).map((workspace) => workspace.path);
        return Option.getOrElse(Array.head(owners.length === 1 ? owners : []), () => at);
    };
    const evaluatedResults = yield* Effect.forEach(analyzed.containers, (container) => Effect.result(evaluated(workspaceRoot, options, container, built(container.path))));
    const xcode = Array.filterMap(Array.zip(analyzed.containers, evaluatedResults), ([container, result]) => Result.map(result, (success) => ({ container, ...success })));
    const unread = [...(Option.isNone(info) ? manifests.map((file) => ({ file, reason: 'swift is absent on this host' })) : []), ...(Option.isNone(reader) ? containers.map((file) => ({ file, reason: `${absent.join(', ')} absent on this host` })) : [])];
    const broken: readonly [string, Error][] = [
        ...Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(Result.flip(result), (error): [string, Error] => [file, refused(file, error.message)(error)])),
        ...(Result.isFailure(analysis) ? containers.map((file): [string, Error] => [file, refused(file, analysis.failure.message)(analysis.failure)]) : []),
        ...Record.collect(analyzed.failures, (at, reason): [string, Error] => [marker(at), refused(at, reason)()]),
        ...Array.filterMap(Array.zip(analyzed.containers, evaluatedResults), ([container, result]) => Result.map(Result.flip(result), (error): [string, Error] => [marker(container.path), refused(container.path, error.message)(error)])),
        ...unsupported.map((file): [string, Error] => [file, refused(file, 'has no reader, and no project.xcproj sits beside it')()]),
    ];
    yield* Effect.forEach(unread, ({ file, reason }) => Effect.sync(() => emitPluginWorkerLog('warn', `${file} presents no project: ${reason}`)));
    const packaged = flow(local, Option.isSome);
    const platforms = (at: string) => Option.match(local(at), { onNone: () => [], onSome: (swiftPackage) => swiftPackage.manifest.platforms.map((platform) => platform.name) });
    const destinations = (folders: readonly string[]) => Array.dedupe(folders.flatMap((folder) => (platforms(folder).length === 0 ? Option.toArray(host) : platforms(folder))));
    const nested = packages.flatMap((owner) => owner.manifest.targets.map((module) => ({ owner: owner.file, folder: `${join(owner.root, module.path)}/` }))).flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && `${dirname(other)}/`.startsWith(folder)).map((inner) => ({ owner, inner })));
    yield* Effect.forEach(nested, ({ owner, inner }) => Effect.sync(() => emitPluginWorkerLog('warn', `${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`)));
    const linkedSources = (container: (typeof xcode)[number]['container']) => Array.getSomes(container.projects.map(local)).flatMap((swiftPackage) => sources(swiftPackage, described));
    const parts: readonly Part[] = [
        ...packages.flatMap((swiftPackage) => packageParts(options, swiftPackage, described)),
        ...xcode.map(({ container, inputs, archives }) => xcodeParts(options, marker(container.path), container, Array.dedupe([...inputs.flatMap(tokened), ...linkedSources(container).filter((input) => !input.endsWith(RESOLVED))]), archives, built(container.path), destinations, packaged)),
    ];
    const grouped = Array.groupBy(parts, (part) => part.root);
    const conflicts = Record.filter(grouped, (group) => new Set(group.map((part) => part.name)).size > 1);
    const merged = Record.collect(
        Record.filter(grouped, (_, root) => !(root in conflicts)),
        (root, group) => {
            const { name, file } = Array.headNonEmpty(group);
            const tools = Array.map(group, (part) => part.targets).reduce(joined);
            const hashed = Record.map(
                Array.groupBy(
                    Object.entries(tools).filter(([label]) => label.includes('--')),
                    ([label]) => label.slice(0, label.indexOf('--')),
                ),
                Array.flatMap(([, { inputs = [] }]) => inputs),
            );
            const release = Array.findFirst(group, (part) => Option.fromUndefinedOr(part.release));
            const project: ProjectConfiguration = {
                root,
                name,
                metadata: { technologies: Array.dedupe(group.flatMap((part) => part.technologies)) },
                tags: [...Option.toArray(Option.as(release, 'release')), `os:${HOST}`],
                ...Option.getOrUndefined(Option.map(release, (held) => ({ release: held }))),
                targets: { ...tools, ...Record.map(hashed, (inputs, verb): TargetConfiguration => ({ dependsOn: [`${verb}--*`], ...(inputs.length > 0 && { inputs: ['...', ...Array.dedupe(inputs)] }) })) },
            };
            return { file, project };
        },
    );
    const roots = new Map(merged.map(({ project }) => [project.name, project.root] as const));
    return {
        graph: {
            projects: Record.map(
                Array.groupBy(merged, ({ file }) => file),
                Record.fromIterableWith(({ project }: (typeof merged)[number]): [string, ProjectConfiguration] => [project.root, project]),
            ),
            edges: Array.getSomes(packages.flatMap((swiftPackage) => packageEdges(swiftPackage, described)).map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(roots.get(edge.source)), target: Option.fromUndefinedOr(roots.get(edge.target)) }), (at) => ({ ...edge, ...at })))),
            buildFiles: Record.fromIterableWith(xcode, ({ container }) => [dirname(container.path), container.buildFiles]),
        } satisfies Graph,
        failures: [...broken, ...Record.collect(conflicts, (root, group): [string, Error] => [Array.headNonEmpty(group).file, refused(root, `holds projects named ${group.map((part) => part.name).join(' and ')}`)()])],
        partial: unread.length > 0,
    };
});

const security = (...args: readonly string[]) => Completed.output(ChildProcess.make('security', args, { stdin: 'ignore' }));

const keychain = (id: string) => join(KEYCHAINS, `${process.pid}-${id}.keychain-db`);

const removed = (id: string) => Effect.flatMap(FileSystem.FileSystem, (fs) => Effect.asVoid(Effect.when(security('delete-keychain', keychain(id)), fs.exists(keychain(id)))));

const identified = Effect.fnUntraced(function* (id: string) {
    const fs = yield* FileSystem.FileSystem;
    const abandoned = (yield* Effect.orElseSucceed(fs.readDirectory(KEYCHAINS), Array.empty<string>)).filter((name) => Option.isNone(Option.liftThrowable(process.kill)(Number.parseInt(name, 10), 0)));
    yield* Effect.forEach(abandoned, (name) => Effect.ignore(security('delete-keychain', join(KEYCHAINS, name))), { discard: true });
    const identities = Array.getSomes(yield* Effect.forEach(IDENTITIES, (name) => Config.option(Config.all({ name: Config.succeed(name), p12: Config.schema(Schema.Redacted(Schema.Uint8ArrayFromBase64), name), passphrase: Config.Redacted(`${name}_PASSWORD`) }))));
    if (identities.length === 0) {
        return;
    }
    const [at, folder] = [keychain(id), yield* fs.makeTempDirectoryScoped()];
    const listed = yield* Effect.filter(
        [...(yield* security('list-keychains', '-d', 'user')).matchAll(/(?<=")[^"\n]+(?=")/gu)].map(([held]) => held),
        fs.exists,
    );
    yield* fs.makeDirectory(KEYCHAINS, { recursive: true });
    yield* security('create-keychain', '-p', id, at);
    yield* security('set-keychain-settings', at);
    yield* security('unlock-keychain', '-p', id, at);
    yield* Effect.forEach(identities, ({ name, p12, passphrase }) =>
        Effect.andThen(fs.writeFile(join(folder, name), Redacted.value(p12), { mode: 0o600 }), Effect.mapError(security('import', join(folder, name), '-k', at, '-P', Redacted.value(passphrase), '-A', '-t', 'cert', '-f', 'pkcs12'), refused(name, `does not import as a PKCS#12 identity under ${name}_PASSWORD`))),
    );
    yield* security('set-key-partition-list', '-S', 'apple-tool:,apple:', '-k', id, at);
    yield* security('list-keychains', '-d', 'user', '-s', at, ...listed);
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Graphs.layer, layer));

const preTasksExecution: PreTasksExecution = (_, { id }) => runtime.runPromise(process.platform === HOST ? Effect.onError(identified(id), () => Effect.ignore(removed(id))) : Effect.void);

const postTasksExecution: PostTasksExecution = (_, { id }) => runtime.runPromise(process.platform === HOST ? removed(id) : Effect.void);

const createNodes: CreateNodes = [
    FILES,
    (matched, options, { workspaceRoot, nxJsonConfiguration: { pluginsConfig } }) =>
        runtime.runPromise(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const files = process.platform === HOST ? matched.toSorted() : [];
                const settings = yield* Schema.decodeEffect(Options)({ ...pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) });
                const cache = new PluginCache<Graph>(join(workspaceDataDirectory, `apple-${hashObject(settings)}.hash`));
                const built = yield* Effect.option(Effect.flatMap(analyzer, fs.stat));
                const stamp = Option.getOrElse(
                    Option.flatMap(built, (stat) => Option.map(stat.mtime, (at) => at.valueOf())),
                    () => 0,
                );
                const key = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, READ)), String(stamp), ...files]);
                const read = yield* Option.match(Option.fromNullishOr(cache.get(key)), { onSome: (graph) => Effect.succeed({ graph, failures: [], partial: false }), onNone: () => analyze(workspaceRoot, settings, files) });
                yield* Effect.sync(() => (read.failures.length === 0 && !read.partial ? cache.set(key, read.graph) : undefined));
                yield* Effect.sync(() => cache.writeToDisk());
                (yield* Graphs).set(workspaceRoot, read.graph);
                const results = Record.collect(read.graph.projects, (file, projects) => [file, { projects }] as const);
                return yield* read.failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError([...read.failures], results));
            }),
        ),
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    runtime.runPromise(
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

export { createDependencies, createNodes, postTasksExecution, preTasksExecution };
