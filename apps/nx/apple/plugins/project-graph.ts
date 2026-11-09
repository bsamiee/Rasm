import { layer } from '@effect/platform-node/NodeServices';
import {
    AggregateCreateNodesError,
    type CreateDependencies,
    type CreateNodes,
    type CreateNodesResultArray,
    DependencyType,
    hashArray,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    type TargetConfiguration,
} from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';
import manifest from '../package.json' with { type: 'json' };
import { Described, MANIFEST, packageEdges, packageParts, sources } from './swift-package.ts';
import { AppleError, type Options, type Part, spawned, tokened } from './targets.ts';
import { Analyzed, evaluated, Host, Request, xcodeParts } from './xcode-project.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [MANIFEST, 'Package@swift-*.swift', '*.xcodeproj/project.xcproj', '*.xcworkspace/contents.xcworkspacedata', '*.xcscheme', '*.xcconfig'];
const FILES = `**/{${[...READ, '*.xcodeproj/project.pbxproj', '*.xctestplan', '*.entitlements', '*.plist'].join(',')}}`;
const ANALYZER = '../.build/release/nx-apple-analyzer';
const { name } = manifest;

// --- [OPERATIONS] ----------------------------------------------------------------------

const combined = (left: TargetConfiguration, right: TargetConfiguration): TargetConfiguration => ({
    ...left,
    ...right,
    options: { ...left.options, ...right.options, runs: [...(left.options?.runs ?? []), ...(right.options?.runs ?? [])] },
    ...(left.inputs === undefined && right.inputs === undefined ? {} : { inputs: Array.dedupe([...(left.inputs ?? []), ...(right.inputs ?? [])]) }),
});

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, files: readonly string[]) {
    const path = yield* Path.Path;
    const manifests = files.filter((file) => path.basename(file) === MANIFEST);
    const containers = files.filter((file) => path.basename(file) === 'project.xcproj' || (path.basename(file) === 'contents.xcworkspacedata' && path.extname(path.dirname(path.dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => path.basename(file) === 'project.pbxproj' && !files.includes(path.join(path.dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => path.dirname(file) === container) ?? container;
    const describedResults = yield* Effect.forEach(
        manifests,
        (file) =>
            Effect.result(
                Effect.flatMap(
                    spawned('swift', ['package', '--package-path', path.dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }),
                    Schema.decodeEffect(Described),
                ),
            ),
        { concurrency: 'unbounded' },
    );
    const packages = Array.zip(manifests, describedResults).flatMap(([file, held]) => (Result.isSuccess(held) ? [{ file, held: held.success }] : []));
    const described = new Map(packages.map(({ file, held }) => [path.resolve(workspaceRoot, path.dirname(file)), held] as const));
    const local = (at: string) => Option.fromUndefinedOr(described.get(path.resolve(workspaceRoot, at)));
    const analyzed =
        containers.length === 0
            ? { containers: [], failures: {} }
            : yield* Effect.flatMap(spawned(path.join(import.meta.dirname, ANALYZER), [], { cwd: workspaceRoot, input: yield* Schema.encodeEffect(Request)(containers) }), Schema.decodeEffect(Analyzed));
    const evaluatedResults = yield* Effect.forEach(
        analyzed.containers,
        (container) => Effect.result(evaluated(workspaceRoot, container)),
        { concurrency: 'unbounded' },
    );
    const xcode = Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, inputs]) => (Result.isSuccess(inputs) ? [{ container, inputs: inputs.success }] : []));
    const workspaces = xcode.filter(({ container }) => path.basename(marker(container.path)) === 'contents.xcworkspacedata').map(({ container }) => container);
    const packaged = (at: string) => Option.isSome(local(at));
    const platforms = (at: string) => Option.match(local(at), { onNone: () => [], onSome: (held) => held.platforms.map(({ name }) => name) });
    const unplatformed = xcode.some(({ container }) => container.schemes.some(({ containers: owners }) => owners.length > 0 && owners.every((owner) => packaged(owner) && platforms(owner).length === 0)));
    const host = unplatformed ? Option.some((yield* Effect.flatMap(spawned('swift', ['-print-target-info'], { cwd: workspaceRoot }), Schema.decodeEffect(Host))).target.platform) : Option.none<string>();
    const destinations = (roots: readonly string[]) => Array.dedupe(roots.flatMap((root) => (platforms(root).length === 0 ? Option.toArray(host) : platforms(root))));
    yield* Effect.forEach(packages, ({ file, held }) =>
        Effect.forEach(
            manifests.filter((other) => other !== file && held.targets.some((module) => `${path.dirname(other)}/`.startsWith(`${path.join(path.dirname(file), module.path)}/`))),
            (nested) => Effect.logWarning(`${nested} sits inside a target folder of ${file}, so SwiftPM compiles it into that target`),
        ),
    );
    const parts: readonly Part[] = [
        ...packages.flatMap(({ file, held }) => packageParts(path, workspaceRoot, file, held, described)),
        ...xcode.map(({ container, inputs }) => {
            const owners = workspaces.filter((workspace) => workspace.projects.includes(container.path)).map((workspace) => workspace.path);
            const linked = container.projects.flatMap((at) => Option.toArray(local(at)).flatMap((held) => sources(path, workspaceRoot, path.normalize(at), held, described)));
            const read = Array.dedupe([...inputs.flatMap((at) => tokened(path, at)), ...linked.filter((input) => !input.endsWith('Package.resolved'))]);
            return xcodeParts(path, options, marker(container.path), container, read, owners.length === 1 ? Array.head(owners) : Option.none(), destinations, packaged);
        }),
    ];
    const grouped = Array.groupBy(parts, (part) => part.root);
    const conflicts = Record.filter(grouped, (group) => new Set(group.map(({ name }) => name)).size > 1);
    const merged = Record.collect(
        Record.filter(grouped, (_, root) => !(root in conflicts)),
        (_, group) => group.reduce((left, right) => ({ ...left, technologies: Array.dedupe([...left.technologies, ...right.technologies]), targets: Record.union(left.targets, right.targets, combined) })),
    );
    const roots = new Map(merged.map((part) => [part.name, part.root] as const));
    const byRoot = new Map(merged.map((part) => [part.root, part.name] as const));
    const edges = [
        ...packages.flatMap(({ held }) => packageEdges(held, described)),
        ...xcode.flatMap(({ container }) =>
            [
                ...Record.collect(container.members, (product, roots) => roots.flatMap((root) => Option.toArray(local(root))).flatMap((held) => held.products.filter(({ name }) => name === product).flatMap(({ targets }) => targets))).flat(),
                ...container.projects.flatMap((at) => Option.toArray(Option.fromUndefinedOr(byRoot.get(packaged(at) ? path.normalize(at) : path.dirname(at))))),
            ].map((module): RawProjectGraphDependency => ({ source: path.basename(container.path, path.extname(container.path)), target: module, type: DependencyType.implicit })),
        ),
    ].flatMap((edge) =>
        Option.toArray(Option.all([Option.fromUndefinedOr(roots.get(edge.source)), Option.fromUndefinedOr(roots.get(edge.target))])).flatMap(([source, sink]) => (source === sink ? [] : [{ ...edge, source, target: sink }])),
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
                                    targets: {
                                        ...part.targets,
                                        check: {
                                            executor: 'nx:noop',
                                            dependsOn: ['check--*'],
                                            inputs: ['...', ...Array.dedupe(Record.collect(Record.filter(part.targets, (_, target) => target.startsWith('check--')), (_, held) => held.inputs ?? []).flat())],
                                            metadata: { technologies: [...part.technologies], description: 'Runs every check--* target' },
                                        },
                                    },
                                } satisfies Omit<ProjectConfiguration, 'root'>,
                            },
                        },
                    ] as const,
            ),
            edges: Array.dedupeWith(edges, (left, right) => left.source === right.source && left.target === right.target),
        } satisfies Graph,
        failures: [
            ...Array.zip(manifests, describedResults).flatMap(([file, held]) => (Result.isFailure(held) ? [[file, held.failure] as const] : [])),
            ...Record.collect(analyzed.failures, (at, reason) => [marker(at), AppleError.make({ subject: at, detail: reason })] as const),
            ...Array.zip(analyzed.containers, evaluatedResults).flatMap(([container, held]) => (Result.isFailure(held) ? [[marker(container.path), held.failure] as const] : [])),
            ...unsupported.map((file) => [file, AppleError.make({ subject: file, detail: 'has no project.xcproj beside it, and project.pbxproj has no reader' })] as const),
            ...Record.collect(conflicts, (root, group) => [group[0]?.file ?? root, AppleError.make({ subject: root, detail: `holds projects named ${group.map(({ name }) => name).join(' and ')}` })] as const),
        ],
    };
});

const reports = new Map<string, Graph>();

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, matched: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const files = matched.toSorted();
    const modules = (yield* fs.readDirectory(import.meta.dirname)).toSorted();
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [`**/{${READ.join(',')}}`])),
        ...(yield* Effect.forEach(modules, (module) => fs.readFileString(path.join(import.meta.dirname, module)))),
        String(Option.getOrElse(Option.flatMap(yield* Effect.option(fs.stat(path.join(import.meta.dirname, ANALYZER))), ({ mtime }) => Option.map(mtime, (at) => at.valueOf())), () => 0)),
        hashObject(options),
        String(Boolean(isCI())),
        ...files,
    ]);
    const cache = new PluginCache<Graph>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options)}.hash`));
    const built = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: (held) => Effect.succeed({ graph: held, failures: [] }),
        onNone: () => Effect.tap(analyze(workspaceRoot, options, files), ({ graph: report, failures }) => (failures.length === 0 ? Effect.sync(() => (cache.set(hash, report), cache.writeToDisk())) : Effect.void)),
    });
    reports.set(hashArray([workspaceRoot, hashObject(options)]), built.graph);
    return built;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<Options> = [
    FILES,
    (files, options = {}, { workspaceRoot }) =>
        Effect.runPromise(
            Effect.flatMap(graph(workspaceRoot, options, files), ({ graph: built, failures }) =>
                failures.length === 0 ? Effect.succeed(built.nodes) : Effect.fail(new AggregateCreateNodesError(failures.map(([file, error]) => [file, new Error(error.message)]), [...built.nodes])),
            ).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies<Options> = (options = {}, { workspaceRoot, projects }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    return (reports.get(hashArray([workspaceRoot, hashObject(options)]))?.edges ?? []).flatMap((edge) =>
        Option.toArray(Option.all([Option.fromUndefinedOr(names.get(edge.source)), Option.fromUndefinedOr(names.get(edge.target))])).map(([source, sink]) => ({ ...edge, source, target: sink })),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, name, type Options };
