import { join, matchesGlob, relative } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, getOutputsForTargetAndConfiguration, hashArray, type ProjectConfiguration, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Result, Schema } from 'effect';
import { Described, MANIFEST, type Package, packageEdges, packageParts, scratched, sources } from '#/plugins/swift-package';
import { AppleError, core, type Part, spawned, tokened } from '#/plugins/targets';
import { Analyzed, evaluated, Host, Request, xcodeParts } from '#/plugins/xcode-project';

// --- [TYPES] ---------------------------------------------------------------------------

type Options = typeof Options.Type;
interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
    readonly references: Readonly<Record<string, readonly string[]>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [MANIFEST, 'Package@swift-*.swift', '*.xcodeproj/project.xcproj', '*.xcworkspace/contents.xcworkspacedata', '*.xcscheme', '*.xcconfig'];
const FILES = `**/{${[...READ, '*.xcodeproj/project.pbxproj', '*.xctestplan', '*.entitlements', '*.plist'].join(',')}}`;
const VERBS = ['build', 'check', 'fix', 'upgrade'];

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ trustPackages: Schema.optionalKey(Schema.Boolean), outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'Plugin options from the nx.json row' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const combined = (left: TargetConfiguration, right: TargetConfiguration): TargetConfiguration => ({
    ...left,
    ...right,
    options: { ...left.options, ...right.options, runs: [...(left.options?.runs ?? []), ...(right.options?.runs ?? [])] },
    ...(left.inputs === undefined && right.inputs === undefined ? {} : { inputs: Array.dedupe([...(left.inputs ?? []), ...(right.inputs ?? [])]) }),
});

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, plugin: Effect.Success<typeof core>, files: readonly string[]) {
    const path = yield* Path.Path;
    const manifests = files.filter((file) => path.basename(file) === MANIFEST);
    const containers = files.filter((file) => path.basename(file) === 'project.xcproj' || (path.basename(file) === 'contents.xcworkspacedata' && path.extname(path.dirname(path.dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => path.basename(file) === 'project.pbxproj' && !files.includes(path.join(path.dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => path.dirname(file) === container) ?? container;
    const describedResults = yield* Effect.forEach(
        manifests,
        (file) => Effect.result(Effect.flatMap(spawned('swift', ['package', '--package-path', path.dirname(file), ...scratched(path, options, path.dirname(file)), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }), Schema.decodeEffect(Described))),
        { concurrency: 'unbounded' },
    );
    const packages = Array.zip(manifests, describedResults).flatMap(([file, held]): readonly Package[] => (Result.isSuccess(held) ? [{ file, root: path.dirname(file), held: held.success }] : []));
    const described = new Map(packages.map((found) => [path.resolve(workspaceRoot, found.root), found] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(path.resolve(workspaceRoot, at)));
    const analyzed = containers.length === 0 ? { containers: [], failures: {} } : yield* Effect.flatMap(spawned(yield* plugin.analyzer, [], { cwd: workspaceRoot, input: yield* Schema.encodeEffect(Request)(containers) }), Schema.decodeEffect(Analyzed));
    const workspaces = analyzed.containers.filter((container) => path.basename(marker(container.path)) === 'contents.xcworkspacedata');
    const built = (at: string) => {
        const owners = workspaces.filter((workspace) => workspace.projects.includes(at)).map((workspace) => workspace.path);
        return Option.getOrElse(Array.head(owners.length === 1 ? owners : []), () => at);
    };
    const evaluatedResults = yield* Effect.forEach(analyzed.containers, (container) => Effect.result(evaluated(workspaceRoot, options, container, built(container.path))));
    const xcode = Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, inputs]) => (Result.isSuccess(inputs) ? [{ container, inputs: inputs.success }] : []));
    const packaged = (at: string) => Option.isSome(local(at));
    const platforms = (at: string) => Option.match(local(at), { onNone: () => [], onSome: (found) => found.held.platforms.map((platform) => platform.name) });
    const unplatformed = (at: string) => Option.exists(local(at), (found) => found.held.platforms.length === 0);
    const host = xcode.flatMap(({ container }) => container.schemes).some((scheme) => scheme.containers.length > 0 && scheme.containers.every(unplatformed))
        ? Option.some((yield* Effect.flatMap(spawned('swift', ['-print-target-info'], { cwd: workspaceRoot }), Schema.decodeEffect(Host))).target.platform)
        : Option.none<string>();
    const destinations = (folders: readonly string[]) => Array.dedupe(folders.flatMap((folder) => (platforms(folder).length === 0 ? Option.toArray(host) : platforms(folder))));
    const nested = packages.flatMap((owner) => owner.held.targets.map((module) => ({ owner: owner.file, folder: `${path.join(owner.root, module.path)}/` }))).flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && `${path.dirname(other)}/`.startsWith(folder)).map((inner) => ({ owner, inner })));
    yield* Effect.forEach(nested, ({ owner, inner }) => Effect.logWarning(`${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`));
    const linked = (container: (typeof xcode)[number]['container']) => container.projects.flatMap((at) => Option.toArray(local(at))).flatMap((found) => sources(path, found, described));
    const parts: readonly Part[] = [
        ...packages.flatMap((found) => packageParts(path, options, plugin.name, found, described)),
        ...xcode.map(({ container, inputs }) => xcodeParts(path, options, plugin.name, marker(container.path), container, Array.dedupe([...inputs.flatMap((at) => tokened(path, at)), ...linked(container).filter((input) => !input.endsWith('Package.resolved'))]), built(container.path), destinations, packaged)),
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
    const byRoot = new Map(merged.map((part) => [part.root, part.name] as const));
    const modules = (product: string, folders: readonly string[]) => folders.flatMap((folder) => Option.toArray(local(folder))).flatMap((found) => found.held.products.filter((owned) => owned.name === product).flatMap((owned) => owned.targets));
    const edges = [
        ...packages.flatMap((found) => packageEdges(found.held, described)),
        ...xcode.flatMap(({ container }) =>
            [...Record.collect(container.members, modules).flat(), ...container.projects.flatMap((at) => Option.toArray(Option.fromUndefinedOr(byRoot.get(packaged(at) ? path.normalize(at) : path.dirname(at)))))].map(
                (module): RawProjectGraphDependency => ({ source: path.basename(container.path, path.extname(container.path)), target: module, type: DependencyType.implicit }),
            ),
        ),
    ].flatMap((edge) => Option.toArray(Option.all([Option.fromUndefinedOr(roots.get(edge.source)), Option.fromUndefinedOr(roots.get(edge.target))])).flatMap(([source, sink]) => (source === sink ? [] : [{ ...edge, source, target: sink }])));
    const aggregates = (part: Part) =>
        Record.fromIterableWith(
            VERBS.filter((verb) => Record.keys(part.targets).some((owned) => owned.startsWith(`${verb}--`))),
            (verb): readonly [string, TargetConfiguration] => [
                verb,
                {
                    executor: 'nx:noop',
                    dependsOn: [{ target: `${verb}--*`, params: 'forward' }],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(
                                Record.filter(part.targets, (_, owned) => owned.startsWith(`${verb}--`)),
                                (_, held) => held.inputs ?? [],
                            ).flat(),
                        ),
                    ],
                    metadata: { technologies: [...part.technologies], description: `Runs every ${verb}--* target` },
                },
            ],
        );
    return {
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
                                    ...(part.release === undefined ? {} : { release: part.release }),
                                } satisfies Omit<ProjectConfiguration, 'root'>,
                            },
                        },
                    ] as const,
            ),
            edges: Array.dedupeWith(edges, (left, right) => left.source === right.source && left.target === right.target),
            references: Record.fromIterableWith(xcode, ({ container }) => [path.dirname(container.path), container.references]),
        } satisfies Graph,
        failures: [
            ...Array.zip(manifests, describedResults).flatMap(([file, held]) => (Result.isFailure(held) ? [[file, held.failure] as const] : [])),
            ...Record.collect(analyzed.failures, (at, reason) => [marker(at), AppleError.make({ subject: at, detail: reason })] as const),
            ...Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, held]) => (Result.isFailure(held) ? [[marker(container.path), held.failure] as const] : [])),
            ...unsupported.map((file) => [file, AppleError.make({ subject: file, detail: 'has no project.xcproj beside it, and project.pbxproj has no reader' })] as const),
            ...Record.collect(conflicts, (root, group) => [Array.headNonEmpty(group).file, AppleError.make({ subject: root, detail: `holds projects named ${group.map((part) => part.name).join(' and ')}` })] as const),
        ],
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
        onSome: (held) => Effect.succeed({ graph: held, failures: [] }),
        onNone: () =>
            Effect.tap(analyze(workspaceRoot, options, plugin, files), (fresh) =>
                fresh.failures.length === 0
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
                    (error) => new AggregateCreateNodesError([[null, new Error(error.message)]], []),
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
            Record.filter(project.targets ?? {}, (held) => held.outputs !== undefined),
            (target, held) => [{}, ...Record.keys(held.configurations ?? {}).map((configuration) => ({ configuration }))].map((selected) => ({ producer, project, target, selected })),
        ).flat(),
    ).flat();
    const outputs = declared.flatMap(({ producer, project, target, selected }) => getOutputsForTargetAndConfiguration({ project: producer, target, ...selected }, {}, { name: producer, type: 'lib', data: project }).flatMap((output) => (output.startsWith('!') ? [] : [{ root: project.root, output }])));
    const references = Record.collect(report?.references ?? {}, (root, held) => held.map((at) => ({ root, at }))).flat();
    const produced = Array.cartesian(references, outputs)
        .filter(([{ root, at }, { root: producer, output }]) => root !== producer && (matchesGlob(at, output) || matchesGlob(at, join(output, '**')) || !relative(at, output).startsWith('..')))
        .map(([{ root }, { root: producer }]): RawProjectGraphDependency => ({ source: root, target: producer, type: DependencyType.implicit }));
    return Array.dedupeWith([...(report?.edges ?? []), ...produced], (left, right) => left.source === right.source && left.target === right.target).flatMap((edge) =>
        Option.toArray(Option.all([Option.fromUndefinedOr(owners.get(edge.source)), Option.fromUndefinedOr(owners.get(edge.target))])).map(([source, sink]) => ({ ...edge, source, target: sink })),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, type Options };
