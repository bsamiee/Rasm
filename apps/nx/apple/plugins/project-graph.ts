import { join, matchesGlob, relative } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, getOutputsForTargetAndConfiguration, hashArray, logger, type ProjectConfiguration, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Result, Schema } from 'effect';
import { Described, INFO, MANIFEST, type Package, packageEdges, packageParts, sources } from '#/plugins/swift-package';
import { AppleError, core, type Part, present, spawned, target, tokened } from '#/plugins/targets';
import { Analyzed, evaluated, Host, invocation, Request, released, xcodeParts } from '#/plugins/xcode-project';

// --- [TYPES] ---------------------------------------------------------------------------

type Options = typeof Options.Type;
interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
    readonly buildFiles: Readonly<Record<string, readonly string[]>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [MANIFEST, INFO, 'Package@swift-*.swift', '*.xcodeproj/project.xcproj', '*.xcworkspace/contents.xcworkspacedata', '*.xcscheme', '*.xcconfig'];
const FILES = `**/{${[...READ, '*.xcodeproj/project.pbxproj', '*.xctestplan', '*.entitlements', '*.plist'].join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ trustPackages: Schema.optionalKey(Schema.Boolean), outputPath: Schema.optionalKey(Schema.String), notaryProfile: Schema.optionalKey(Schema.String) }).annotate({ description: 'Plugin options from the nx.json row' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const combined = (left: TargetConfiguration, right: TargetConfiguration): TargetConfiguration => ({
    ...left,
    ...right,
    options: { ...left.options, ...right.options, runs: [...(left.options?.runs ?? []), ...(right.options?.runs ?? [])] },
    ...(left.inputs === undefined && right.inputs === undefined ? {} : { inputs: Array.dedupe([...(left.inputs ?? []), ...(right.inputs ?? [])]) }),
});

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, located: Effect.Success<typeof core>, files: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const plugin = yield* located.plugin(workspaceRoot);
    const manifests = files.filter((file) => path.basename(file) === MANIFEST);
    const containers = files.filter((file) => path.basename(file) === 'project.xcproj' || (path.basename(file) === 'contents.xcworkspacedata' && path.extname(path.dirname(path.dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => path.basename(file) === 'project.pbxproj' && !files.includes(path.join(path.dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => path.dirname(file) === container) ?? container;
    const [printed, xcodebuild, compiled] = yield* Effect.all([present('swift', ['-print-target-info'], workspaceRoot), containers.length === 0 ? Effect.succeedNone : present('xcodebuild', ['-version'], workspaceRoot), containers.length === 0 ? Effect.succeedNone : Effect.option(located.analyzer)]);
    const host = yield* Effect.transposeOption(Option.map(printed, (text) => Effect.map(Schema.decodeEffect(Host)(text), ({ target: { platform } }) => platform)));
    const analyzer = Option.map(Option.all({ xcodebuild, compiled }), ({ compiled: at }) => at);
    const readable = Option.isSome(printed) ? manifests : [];
    const absent = [...(Option.isNone(printed) ? ['swift'] : []), ...(containers.length > 0 && Option.isNone(xcodebuild) ? ['xcodebuild'] : []), ...(containers.length > 0 && Option.isNone(compiled) ? ['the #analyzer core'] : [])];
    const describedResults = yield* Effect.forEach(readable, (file) => Effect.result(Effect.flatMap(spawned('swift', ['package', '--package-path', path.dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }), Schema.decodeEffect(Described))), {
        concurrency: 'unbounded',
    });
    const packages = yield* Effect.forEach(
        Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(result, (manifest) => ({ file, root: path.dirname(file), manifest }))),
        Effect.fnUntraced(function* (swiftPackage: Omit<Package, 'plists'>) {
            const listed = yield* Effect.filter(swiftPackage.manifest.targets, (module) => fs.exists(path.join(workspaceRoot, swiftPackage.root, module.path, INFO)));
            return { ...swiftPackage, plists: new Set(listed.map(({ name }) => name)) } satisfies Package;
        }),
    );
    const described = new Map(packages.map((swiftPackage) => [path.resolve(workspaceRoot, swiftPackage.root), swiftPackage] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(path.resolve(workspaceRoot, at)));
    const analysis = yield* Option.match(analyzer, {
        onNone: () => Effect.succeed(Result.succeed<typeof Analyzed.Type>({ containers: [], failures: {} })),
        onSome: (at) =>
            Effect.result(
                Effect.flatMap(
                    Effect.flatMap(Schema.encodeEffect(Request)(containers), (input) => spawned(at, [], { cwd: workspaceRoot, input })),
                    Schema.decodeEffect(Analyzed),
                ),
            ),
    });
    const analyzed = Result.getOrElse(analysis, (): typeof Analyzed.Type => ({ containers: [], failures: {} }));
    const workspaces = analyzed.containers.filter((container) => path.basename(marker(container.path)) === 'contents.xcworkspacedata');
    const built = (at: string) => {
        const owners = workspaces.filter((workspace) => workspace.projects.includes(at)).map((workspace) => workspace.path);
        return Option.getOrElse(Array.head(owners.length === 1 ? owners : []), () => at);
    };
    const evaluatedResults = yield* Effect.forEach(analyzed.containers, (container) => Effect.result(evaluated(workspaceRoot, options, container, built(container.path))));
    const xcode = Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, result]) => (Result.isSuccess(result) ? [{ container, ...result.success }] : []));
    const unread = [
        ...(Option.isNone(printed) ? manifests.map((file) => ({ file, reason: 'swift is absent on this host' })) : []),
        ...Array.zip(readable, describedResults).flatMap(([file, result]) => (Result.isFailure(result) ? [{ file, reason: result.failure.message }] : [])),
        ...(Option.isNone(analyzer) ? containers.map((file) => ({ file, reason: `${absent.join(', ')} absent on this host` })) : []),
        ...(Result.isFailure(analysis) ? containers.map((file) => ({ file, reason: analysis.failure.message })) : []),
        ...Record.collect(analyzed.failures, (at, reason) => ({ file: marker(at), reason })),
        ...Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, result]) => (Result.isFailure(result) ? [{ file: marker(container.path), reason: result.failure.message }] : [])),
        ...unsupported.map((file) => ({ file, reason: 'project.pbxproj has no reader, and no project.xcproj sits beside it' })),
    ];
    yield* Effect.forEach(unread, ({ file, reason }) => Effect.sync(() => logger.warn(`${file} takes its project and targets from the file alone, and its runs report the tool's own failure: ${reason}`)));
    const filed = unread.map(({ file, reason }) =>
        path.basename(file) === MANIFEST
            ? { file, reason, root: path.dirname(file), name: path.basename(path.resolve(workspaceRoot, path.dirname(file))), technology: 'swift', tool: 'swift-build', run: ['swift', 'build', '--package-path', path.dirname(file), ...(Predicate.isTruthy(isCI()) ? ['--disable-automatic-resolution'] : [])] as const }
            : { file, reason, root: path.dirname(path.dirname(file)), name: path.basename(path.dirname(file), path.extname(path.dirname(file))), technology: 'xcode', tool: 'xcodebuild', run: ['xcodebuild', 'build', ...invocation(path, options, path.dirname(file)).flags] as const },
    );
    const packaged = (at: string) => Option.isSome(local(at));
    const platforms = (at: string) => Option.match(local(at), { onNone: () => [], onSome: (swiftPackage) => swiftPackage.manifest.platforms.map((platform) => platform.name) });
    const destinations = (folders: readonly string[]) => Array.dedupe(folders.flatMap((folder) => (platforms(folder).length === 0 ? Option.toArray(host) : platforms(folder))));
    const nested = packages
        .flatMap((owner) => owner.manifest.targets.map((module) => ({ owner: owner.file, folder: `${path.join(owner.root, module.path)}/` })))
        .flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && `${path.dirname(other)}/`.startsWith(folder)).map((inner) => ({ owner, inner })));
    yield* Effect.forEach(nested, ({ owner, inner }) => Effect.sync(() => logger.warn(`${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`)));
    const linked = (container: (typeof xcode)[number]['container']) => container.projects.flatMap((at) => Option.toArray(local(at))).flatMap((swiftPackage) => sources(path, swiftPackage, described));
    const parts: readonly Part[] = [
        ...packages.flatMap((swiftPackage) => packageParts(path, options, plugin, swiftPackage, described)),
        ...xcode.map(({ container, inputs, archives }) =>
            xcodeParts(path, options, plugin, marker(container.path), container, Array.dedupe([...inputs.flatMap((at) => tokened(path, at)), ...linked(container).filter((input) => !input.endsWith('Package.resolved'))]), archives, built(container.path), destinations, packaged),
        ),
        ...filed.map(
            ({ file, reason, root, name, technology, tool, run }): Part => ({
                root,
                name,
                file,
                technologies: [technology],
                ...released(path, plugin, file),
                targets: {
                    [`build--${tool}`]: target(plugin, [run], { technologies: [technology], description: `Builds ${name} with ${run[0]}, inferred from ${file} alone: ${reason.split('\n')[0] ?? reason}` }, { inputs: [path.join('{projectRoot}', '**/*'), ...plugin.inputs] }),
                    [`check--${tool}`]: { executor: 'nx:noop', dependsOn: [`build--${tool}`], metadata: { technologies: [technology], description: `Compiles ${name} as its type check` } },
                },
            }),
        ),
    ];
    const grouped = Array.groupBy(parts, (part) => part.root);
    const conflicts = Record.filter(grouped, (group) => new Set(group.map((part) => part.name)).size > 1);
    const merged = Record.collect(
        Record.filter(grouped, (_, root) => !(root in conflicts)),
        (_, group): Part => ({
            ...Array.headNonEmpty(group),
            ...Object.fromEntries(group.flatMap((part) => (part.release === undefined ? [] : [['release', part.release]]))),
            technologies: Array.dedupe(group.flatMap((part) => part.technologies)),
            targets: group.map((part) => part.targets).reduce((left, right) => Record.union(left, right, combined)),
        }),
    );
    const roots = new Map(merged.map((part) => [part.name, part.root] as const));
    const edges = packages
        .flatMap((swiftPackage) => packageEdges(swiftPackage.manifest, described))
        .flatMap((edge) => Option.toArray(Option.all([Option.fromUndefinedOr(roots.get(edge.source)), Option.fromUndefinedOr(roots.get(edge.target))])).flatMap(([source, sink]) => (source === sink ? [] : [{ ...edge, source, target: sink }])));
    const aggregates = (part: Part) =>
        Record.map(
            Array.groupBy(
                Record.toEntries(part.targets).filter(([owned]) => owned.includes('--')),
                ([owned]) => owned.slice(0, owned.indexOf('--')),
            ),
            (group, verb): TargetConfiguration => ({
                executor: 'nx:noop',
                dependsOn: [`${verb}--*`],
                inputs: ['...', ...Array.dedupe(group.flatMap(([, member]) => member.inputs ?? []))],
                metadata: { technologies: [...part.technologies], description: `Runs every ${verb}--* target` },
            }),
        );
    return {
        complete: unread.length === 0,
        graph: {
            nodes: merged.map(
                (part) =>
                    [
                        part.file,
                        {
                            projects: {
                                [part.root]: {
                                    name: part.name,
                                    metadata: { technologies: [...part.technologies] },
                                    targets: { ...part.targets, ...aggregates(part) },
                                    tags: ['os:darwin', ...(part.release === undefined ? [] : ['release'])],
                                    ...(part.release === undefined ? {} : { release: part.release }),
                                } satisfies Omit<ProjectConfiguration, 'root'>,
                            },
                        },
                    ] as const,
            ),
            edges: Array.dedupeWith(edges, (left, right) => left.source === right.source && left.target === right.target),
            buildFiles: Record.fromIterableWith(xcode, ({ container }) => [path.dirname(container.path), container.buildFiles]),
        } satisfies Graph,
        failures: [...Record.collect(conflicts, (root, group) => [Array.headNonEmpty(group).file, AppleError.make({ subject: root, detail: `holds projects named ${group.map((part) => part.name).join(' and ')}` })] as const)],
    };
});

const reports = new Map<string, Graph>();

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, matched: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const files = matched.toSorted();
    const plugin = yield* core;
    const compiled = yield* Effect.option(Effect.flatMap(plugin.analyzer, fs.stat));
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [`**/{${READ.join(',')}}`, path.join(path.relative(workspaceRoot, plugin.folder), '**/*.{ts,json,swift}')])),
        plugin.version,
        String(
            Option.getOrElse(
                Option.flatMap(compiled, (stat) => Option.map(stat.mtime, (at) => at.valueOf())),
                () => 0,
            ),
        ),
        hashObject(options),
        String(Predicate.isTruthy(isCI())),
        ...files,
    ]);
    const cache = new PluginCache<Graph>(path.join(workspaceDataDirectory, `${path.basename(plugin.name)}-${hashObject(options)}.hash`));
    const result = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: (cached) => Effect.succeed({ graph: cached, failures: [] }),
        onNone: () =>
            Effect.tap(analyze(workspaceRoot, options, plugin, files), (fresh) =>
                fresh.complete && fresh.failures.length === 0
                    ? Effect.sync(() => {
                          cache.set(hash, fresh.graph);
                          cache.writeToDisk();
                      })
                    : Effect.void,
            ),
    });
    reports.set(hashArray([workspaceRoot, hashObject(options)]), result.graph);
    return result;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (files, options, { workspaceRoot }) =>
        Effect.runPromise(
            Effect.flatMap(
                Effect.mapError(
                    Effect.flatMap(Schema.decodeEffect(Options)(options ?? {}), (decoded) => graph(workspaceRoot, decoded, files)),
                    (error) => new AggregateCreateNodesError([[null, error]], []),
                ),
                (result) =>
                    result.failures.length === 0
                        ? Effect.succeed(result.graph.nodes)
                        : Effect.fail(
                              new AggregateCreateNodesError(
                                  result.failures.map(([file, error]) => [file, new Error(error.message)]),
                                  [...result.graph.nodes],
                              ),
                          ),
            ).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies<typeof Options.Encoded> = (options, { workspaceRoot, projects }) => {
    const owners = createProjectRootMappingsFromProjectConfigurations(projects);
    const report = reports.get(hashArray([workspaceRoot, hashObject(options ?? {})]));
    const declared = Record.collect(projects, (producer, project) =>
        Record.collect(
            Record.filter(project.targets ?? {}, (configured) => configured.outputs !== undefined),
            (name, configured) => [{}, ...Record.keys(configured.configurations ?? {}).map((configuration) => ({ configuration }))].map((selected) => ({ producer, project, name, selected })),
        ).flat(),
    ).flat();
    const outputs = declared.flatMap(({ producer, project, name, selected }) => getOutputsForTargetAndConfiguration({ project: producer, target: name, ...selected }, {}, { name: producer, type: 'lib', data: project }).flatMap((output) => (output.startsWith('!') ? [] : [{ root: project.root, output }])));
    const buildFiles = Record.collect(report?.buildFiles ?? {}, (root, files) => files.map((at) => ({ root, at }))).flat();
    const produced = Array.cartesian(buildFiles, outputs)
        .filter(([{ root, at }, { root: producer, output }]) => root !== producer && (matchesGlob(at, output) || matchesGlob(at, join(output, '**')) || !relative(at, output).startsWith('..')))
        .map(([{ root }, { root: producer }]): RawProjectGraphDependency => ({ source: root, target: producer, type: DependencyType.implicit }));
    return Array.dedupeWith([...(report?.edges ?? []), ...produced], (left, right) => left.source === right.source && left.target === right.target).flatMap((edge) =>
        Option.toArray(Option.all([Option.fromUndefinedOr(owners.get(edge.source)), Option.fromUndefinedOr(owners.get(edge.target))])).map(([source, sink]) => ({ ...edge, source, target: sink })),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, type Options };
