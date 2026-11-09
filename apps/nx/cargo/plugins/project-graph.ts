import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, hashArray, logger, type RawProjectGraphDependency, validateDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';
import { aggregated, crateTargets, released, TECHNOLOGIES, unresolved, workspaceTargets } from '#/plugins/targets';
import { DENY, MANIFEST, Manifest, toml, workspaces } from '#/plugins/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = `**/{${[MANIFEST, 'rust-toolchain.toml', 'rust-toolchain', '.cargo/config.toml', '.cargo/config', 'clippy.toml', '.clippy.toml', '.config/nextest.toml', ...Object.values(DENY).flat()].join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Package = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyze = Effect.fnUntraced(function* (context: CreateNodesContext, plugin: Parameters<typeof released>[0], matched: readonly string[]) {
    const path = yield* Path.Path;
    const files = new Set(matched);
    const manifests = matched.filter((file) => path.basename(file) === MANIFEST).toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right));
    const results = yield* workspaces(context.workspaceRoot, manifests, files);
    const [resolved, failures] = Array.partition(results, ([file, result]) => Result.mapError(result, (error) => [file, error] as const));
    const parts = resolved.map((workspace) => {
        const crates = Record.fromIterableWith(workspace.members, ({ crate, root }) => {
            const named = getNamedInputs(root, context);
            const { targets, ...configured } = released(plugin, path, crate, workspace.root);
            return [
                root,
                {
                    name: crate.name,
                    metadata: { technologies: TECHNOLOGIES },
                    ...configured,
                    targets: { ...crateTargets(plugin, path, workspace, files, 'production' in named ? ['production', '^production'] : ['default', '^default'], crate, root), ...targets },
                },
            ] as const;
        });
        const own = crates[workspace.root];
        const projects = Record.map({ ...crates, [workspace.root]: { metadata: { technologies: TECHNOLOGIES }, ...own, targets: { ...own?.targets, ...workspaceTargets(plugin, path, workspace) } } }, (project) => ({
            ...project,
            targets: aggregated(project.targets),
        }));
        return { node: [path.join(workspace.root, MANIFEST), { projects }] as const, edges: workspace.edges };
    });
    const fallbacks = yield* Effect.forEach(failures, ([file]) =>
        toml(Manifest, path.join(context.workspaceRoot, file)).pipe(
            Effect.map(({ package: declared }) => (declared === undefined ? {} : { name: declared.name })),
            Effect.orElseSucceed(() => ({})),
            Effect.map((named) => [file, { projects: { [path.dirname(file)]: { ...named, metadata: { technologies: TECHNOLOGIES }, targets: aggregated(unresolved(plugin, path.dirname(file))) } } }] as const),
        ),
    );
    yield* Option.match(Array.head(failures), {
        onNone: () => Effect.void,
        onSome: ([, error]) => Effect.sync(() => logger.warn(`${error.message}; ${failures.length} Cargo manifests keep check--cargo alone until cargo resolves them, and it reports cargo's error`)),
    });
    return {
        graph: { nodes: [...parts.map(({ node }) => node), ...fallbacks], edges: parts.flatMap(({ edges }) => edges) } satisfies Graph,
        resolved: failures.length === 0,
    };
});

const reports = new Map<string, Graph>();

const keyed = (workspaceRoot: string, options: unknown) => hashArray([workspaceRoot, hashObject(options ?? {})]);

const graph = Effect.fnUntraced(function* (context: CreateNodesContext, matched: readonly string[], options: unknown) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const manifest = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Package));
    const tracked = yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [path.relative(context.workspaceRoot, located)]));
    const plugin = { name: manifest.name, inputs: tracked.length === 0 ? [{ externalDependencies: [manifest.name] }] : [{ input: 'default', projects: [manifest.name] }, { externalDependencies: [] }] };
    const hash = hashArray([
        yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [FILES, path.join(path.relative(context.workspaceRoot, path.dirname(import.meta.dirname)), '**/*.{ts,json}')])),
        manifest.version,
        hashObject(context.nxJsonConfiguration.namedInputs ?? {}),
        String(isCI() !== false),
    ]);
    const cache = new PluginCache<Graph>(path.join(workspaceDataDirectory, `${path.basename(manifest.name)}-${hashObject(options ?? {})}.hash`));
    const built = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.map(
                Effect.tap(analyze(context, plugin, matched.toSorted()), ({ graph: report, resolved }) =>
                    resolved
                        ? Effect.sync(() => {
                              cache.set(hash, report);
                              cache.writeToDisk();
                          })
                        : Effect.void,
                ),
                ({ graph: report }) => report,
            ),
    });
    reports.set(keyed(context.workspaceRoot, options), built);
    return built.nodes;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, options, context) =>
        graph(context, files, options).pipe(
            Effect.mapError((error) => new AggregateCreateNodesError([[null, error]], [])),
            Effect.provide(layer),
            Effect.runPromise,
        ),
];

const createDependencies: CreateDependencies = (options, context) => {
    const names = createProjectRootMappingsFromProjectConfigurations(context.projects);
    return Array.getSomes(
        (reports.get(keyed(context.workspaceRoot, options))?.edges ?? []).map((edge) =>
            Option.map(Option.all([Option.fromUndefinedOr(names.get(edge.source)), Option.fromUndefinedOr(names.get(edge.target))]), ([source, target]) => {
                const dependency = { ...edge, source, target };
                validateDependency(dependency, context);
                return dependency;
            }),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
