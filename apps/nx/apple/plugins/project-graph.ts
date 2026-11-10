import { join, matchesGlob, relative } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, getOutputsForTargetAndConfiguration, hashArray, logger, type ProjectConfiguration, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext } from '@nx/devkit/internal';
import { aggregated, cached, inferred, reported } from '@rasm/nx-devkit/inference';
import { present, printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Combiner, Effect, FileSystem, flow, Option, Path, Record, Result, Schema } from 'effect';
import { Described, INFO, MANIFEST, type Package, packageEdges, packageParts, RESOLVED, sources } from '#/plugins/swift-package';
import { analyzer, core, type Part, type Plugin, target, tokened } from '#/plugins/targets';
import { Analyzed, evaluated, invocation, released, xcodeParts } from '#/plugins/xcode-project';

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
const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ platform: Schema.String }) }));
const Request = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const combined = (left: TargetConfiguration, right: TargetConfiguration): TargetConfiguration => ({
    ...left,
    ...right,
    options: { ...left.options, ...right.options, runs: [...(left.options?.runs ?? []), ...(right.options?.runs ?? [])] },
    ...(left.inputs === undefined && right.inputs === undefined ? {} : { inputs: Array.dedupe([...(left.inputs ?? []), ...(right.inputs ?? [])]) }),
});

const joined = Record.makeReducerUnion<string, Readonly<Record<string, TargetConfiguration>>>(Record.makeReducerUnion(Combiner.make(combined)));

const analyze = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, plugin: Plugin, files: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const paths = new Set(files);
    const manifests = files.filter((file) => path.basename(file) === MANIFEST);
    const containers = files.filter((file) => path.basename(file) === 'project.xcproj' || (path.basename(file) === 'contents.xcworkspacedata' && path.extname(path.dirname(path.dirname(file))) !== '.xcodeproj'));
    const unsupported = files.filter((file) => path.basename(file) === 'project.pbxproj' && !paths.has(path.join(path.dirname(file), 'project.xcproj')));
    const marker = (container: string) => containers.find((file) => path.dirname(file) === container) ?? container;
    const [info, xcodebuild, compiled] = yield* Effect.all([present('swift', ['-print-target-info'], { cwd: workspaceRoot }), containers.length === 0 ? Effect.succeedNone : present('xcodebuild', ['-version'], { cwd: workspaceRoot }), containers.length === 0 ? Effect.succeedNone : Effect.option(analyzer)]);
    const host = yield* Effect.transposeOption(Option.map(info, (text) => Effect.map(Schema.decodeEffect(Host)(text), ({ target: { platform } }) => platform)));
    const reader = Option.map(Option.all({ xcodebuild, compiled }), ({ compiled: at }) => at);
    const readable = Option.isSome(info) ? manifests : [];
    const absent = [...(Option.isNone(info) ? ['swift'] : []), ...(containers.length > 0 && Option.isNone(xcodebuild) ? ['xcodebuild'] : []), ...(containers.length > 0 && Option.isNone(compiled) ? ['the #analyzer core'] : [])];
    const describedResults = yield* Effect.forEach(readable, (file) => Effect.result(Effect.flatMap(printed('swift', ['package', '--package-path', path.dirname(file), '--experimental-skip-acquiring-lock', 'describe', '--type', 'json'], { cwd: workspaceRoot }), Schema.decodeEffect(Described))), {
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
    const analysis = yield* Option.match(reader, {
        onNone: () => Effect.succeed(Result.succeed<typeof Analyzed.Type>({ containers: [], failures: {} })),
        onSome: (at) =>
            Effect.result(
                Effect.flatMap(
                    Effect.flatMap(Schema.encodeEffect(Request)(containers), (input) => printed(at, [], { cwd: workspaceRoot, input })),
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
    const xcode = Array.filterMap(Array.zip(analyzed.containers, evaluatedResults), ([container, result]) => Result.map(result, (success) => ({ container, ...success })));
    const unread = [...(Option.isNone(info) ? manifests.map((file) => ({ file, reason: 'swift is absent on this host' })) : []), ...(Option.isNone(reader) ? containers.map((file) => ({ file, reason: `${absent.join(', ')} absent on this host` })) : [])];
    const broken: readonly (readonly [string, Error])[] = [
        ...Array.filterMap(Array.zip(readable, describedResults), ([file, result]) => Result.map(Result.flip(result), (error) => [file, error] as const)),
        ...(Result.isFailure(analysis) ? containers.map((file) => [file, analysis.failure] as const) : []),
        ...Record.collect(analyzed.failures, (at, reason) => [marker(at), ToolError.make({ subject: at, detail: reason })] as const),
        ...Array.filterMap(Array.zip(analyzed.containers, evaluatedResults), ([container, result]) => Result.map(Result.flip(result), (error) => [marker(container.path), error] as const)),
        ...unsupported.map((file) => [file, ToolError.make({ subject: file, detail: 'has no reader, and no project.xcproj sits beside it' })] as const),
    ];
    yield* Effect.forEach(unread, ({ file, reason }) => Effect.sync(() => logger.warn(`${file} takes its project and targets from the file alone, and its runs report the tool's own failure: ${reason}`)));
    const filed = unread.map(({ file, reason }) =>
        path.basename(file) === MANIFEST
            ? { file, reason, root: path.dirname(file), name: path.basename(path.resolve(workspaceRoot, path.dirname(file))), technology: 'swift', tool: 'swift-build', clean: 'swift-package', run: ['swift', 'build', '--package-path', path.dirname(file)] as const }
            : { file, reason, root: path.dirname(path.dirname(file)), name: path.basename(path.dirname(file), path.extname(path.dirname(file))), technology: 'xcode', tool: 'xcodebuild', clean: 'xcodebuild', run: ['xcodebuild', 'build', ...invocation(path, options, path.dirname(file)).flags] as const },
    );
    const packaged = flow(local, Option.isSome);
    const platforms = (at: string) => Option.match(local(at), { onNone: () => [], onSome: (swiftPackage) => swiftPackage.manifest.platforms.map((platform) => platform.name) });
    const destinations = (folders: readonly string[]) => Array.dedupe(folders.flatMap((folder) => (platforms(folder).length === 0 ? Option.toArray(host) : platforms(folder))));
    const nested = packages
        .flatMap((owner) => owner.manifest.targets.map((module) => ({ owner: owner.file, folder: `${path.join(owner.root, module.path)}/` })))
        .flatMap(({ owner, folder }) => manifests.filter((other) => other !== owner && `${path.dirname(other)}/`.startsWith(folder)).map((inner) => ({ owner, inner })));
    yield* Effect.forEach(nested, ({ owner, inner }) => Effect.sync(() => logger.warn(`${inner} sits inside a target folder of ${owner}, so SwiftPM compiles it into that target`)));
    const linked = (container: (typeof xcode)[number]['container']) => Array.getSomes(container.projects.map(local)).flatMap((swiftPackage) => sources(path, swiftPackage, described));
    const parts: readonly Part[] = [
        ...packages.flatMap((swiftPackage) => packageParts(path, options, plugin, swiftPackage, described)),
        ...xcode.map(({ container, inputs, archives }) => xcodeParts(path, options, plugin, marker(container.path), container, Array.dedupe([...inputs.flatMap((at) => tokened(path, at)), ...linked(container).filter((input) => !input.endsWith(RESOLVED))]), archives, built(container.path), destinations, packaged)),
        ...filed.map(
            ({ file, reason, root, name, technology, tool, clean, run }): Part => ({
                root,
                name,
                file,
                technologies: [technology],
                clean,
                ...released(path, plugin, file),
                members: {
                    build: { [tool]: target(plugin, [run], { technologies: [technology], description: `Builds ${name} with ${run[0]}, inferred from ${file} alone: ${reason}` }, { inputs: [path.join('{projectRoot}', '**/*'), ...plugin.inputs] }) },
                    check: { [tool]: { executor: 'nx:noop', dependsOn: [`build--${tool}`], metadata: { technologies: [technology], description: `Compiles ${name} as its type check` } } },
                },
            }),
        ),
    ];
    const grouped = Array.groupBy(parts, (part) => part.root);
    const conflicts = Record.filter(grouped, (group) => new Set(group.map((part) => part.name)).size > 1);
    const merged = Record.collect(
        Record.filter(grouped, (_, root) => !(root in conflicts)),
        (_, group): Part => {
            const [release] = Array.getSomes(group.map((part) => Option.fromUndefinedOr(part.release)));
            const publishes = Array.getSomes(group.map((part) => Option.fromUndefinedOr(part.published)));
            return {
                ...Array.headNonEmpty(group),
                ...(release === undefined ? {} : { release }),
                ...(Array.isReadonlyArrayNonEmpty(publishes) ? { published: publishes.reduce(combined) } : {}),
                technologies: Array.dedupe(group.flatMap((part) => part.technologies)),
                members: joined.combineAll(group.map((part) => part.members)),
            };
        },
    );
    const roots = new Map(merged.map((part) => [part.name, part.root] as const));
    const edges = packages
        .flatMap((swiftPackage) => packageEdges(swiftPackage, described))
        .map((edge) => ({ edge, from: roots.get(edge.source), to: roots.get(edge.target) }))
        .filter(({ from, to }) => from !== undefined && to !== undefined && from !== to)
        .map(({ edge, from, to }) => ({ ...edge, source: from ?? edge.source, target: to ?? edge.target }));
    const failures = [...broken, ...Record.collect(conflicts, (root, group) => [Array.headNonEmpty(group).file, ToolError.make({ subject: root, detail: `holds projects named ${group.map((part) => part.name).join(' and ')}` })] as const)];
    return {
        complete: unread.length === 0 && failures.length === 0,
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
                                    targets: { ...aggregated(part.members, part.technologies, { executor: plugin.cleaner, tool: part.clean }), ...(part.published === undefined ? {} : { 'nx-release-publish': part.published }) },
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
        failures,
    };
});

const reports = new Map<string, Graph>();

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, matched: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const files = matched.toSorted();
    const plugin = yield* core(workspaceRoot);
    const compiled = yield* Effect.option(Effect.flatMap(analyzer, fs.stat));
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [`**/{${READ.join(',')}}`])),
        plugin.digest,
        String(
            Option.getOrElse(
                Option.flatMap(compiled, (stat) => Option.map(stat.mtime, (at) => at.valueOf())),
                () => 0,
            ),
        ),
        ...files,
    ]);
    const cache = yield* cached<Graph>(plugin, options);
    const result = yield* Option.match(yield* cache.get(FILES, () => Effect.succeed(hash)), {
        onSome: (held) => Effect.succeed({ graph: held, failures: Array.empty<readonly [string, Error]>() }),
        onNone: () => Effect.tap(analyze(workspaceRoot, options, plugin, files), (fresh) => (fresh.complete ? cache.set(FILES, fresh.graph, hash) : Effect.void)),
    });
    reports.set(hashArray([workspaceRoot, hashObject(options)]), result.graph);
    return yield* reported(result.graph.nodes, result.failures);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [FILES, (files, options, { workspaceRoot }) => inferred(Effect.flatMap(Schema.decodeEffect(Options)(options ?? {}), (decoded) => graph(workspaceRoot, decoded, files)))];

const createDependencies: CreateDependencies<typeof Options.Encoded> = (options, { workspaceRoot, projects }) => {
    const owners = createProjectRootMappingsFromProjectConfigurations(projects);
    const report = reports.get(hashArray([workspaceRoot, hashObject(options ?? {})]));
    const declared = Record.collect(projects, (producer, project) =>
        Record.collect(
            Record.filter(project.targets ?? {}, (configured) => configured.outputs !== undefined),
            (name, configured) => [{}, ...Record.keys(configured.configurations ?? {}).map((configuration) => ({ configuration }))].map((selected) => ({ producer, project, name, selected })),
        ).flat(),
    ).flat();
    const outputs = declared.flatMap(({ producer, project, name, selected }) =>
        getOutputsForTargetAndConfiguration({ project: producer, target: name, ...selected }, {}, { name: producer, type: 'lib', data: project })
            .filter((output) => !output.startsWith('!'))
            .map((output) => ({ root: project.root, output })),
    );
    const buildFiles = Record.collect(report?.buildFiles ?? {}, (root, files) => files.map((at) => ({ root, at }))).flat();
    const produced = Array.cartesian(buildFiles, outputs)
        .filter(([{ root, at }, { root: producer, output }]) => root !== producer && (matchesGlob(at, output) || matchesGlob(at, join(output, '**')) || !relative(at, output).startsWith('..')))
        .map(([{ root }, { root: producer }]): RawProjectGraphDependency => ({ source: root, target: producer, type: DependencyType.implicit }));
    return Array.dedupeWith([...(report?.edges ?? []), ...produced], (left, right) => left.source === right.source && left.target === right.target)
        .map((edge) => ({ edge, from: owners.get(edge.source), to: owners.get(edge.target) }))
        .filter(({ from, to }) => from !== undefined && to !== undefined)
        .map(({ edge, from, to }) => ({ ...edge, source: from ?? edge.source, target: to ?? edge.target }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, Options };
