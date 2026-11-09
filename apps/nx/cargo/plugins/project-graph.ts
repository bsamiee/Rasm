import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, hashArray, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, isCI, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Effect, FileSystem, Option, Path, Record, Result, Schema } from 'effect';
import { aggregated, crateTargets, released, TECHNOLOGIES, workspaceTargets } from '#/plugins/targets';
import { MANIFEST, workspaces } from '#/plugins/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Graph {
    readonly nodes: CreateNodesResultArray;
    readonly edges: readonly RawProjectGraphDependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = `**/{${[MANIFEST, 'Cargo.lock', 'rust-toolchain.toml', 'rust-toolchain', '.cargo/config.toml', '.cargo/config', 'clippy.toml', '.clippy.toml', '.config/nextest.toml', 'deny.toml', '.deny.toml', '.cargo/deny.toml'].join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyze = Effect.fnUntraced(function* (context: CreateNodesContext, plugin: Parameters<typeof released>[0], matched: readonly string[]) {
    const path = yield* Path.Path;
    const files = new Set(matched);
    const manifests = matched.filter((file) => path.basename(file) === MANIFEST).toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right));
    const results = yield* workspaces(context.workspaceRoot, manifests, files);
    const found = results.flatMap(([, held]) => (Result.isSuccess(held) ? [held.success] : []));
    const parts = found.map((workspace) => {
        const crates = Record.fromIterableWith(workspace.members, ({ crate, root }) => {
            const named = getNamedInputs(root, context);
            const { targets, ...configured } = released(plugin, crate, workspace.root);
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
        return {
            node: [path.join(workspace.root, MANIFEST), { projects, ...Option.match(workspace.externalNodes, { onNone: () => ({}), onSome: (externalNodes) => ({ externalNodes }) }) }] as const,
            edges: workspace.edges,
        };
    });
    return {
        graph: { nodes: parts.map(({ node }) => node), edges: parts.flatMap(({ edges }) => edges) } satisfies Graph,
        failures: results.flatMap(([file, held]) => (Result.isFailure(held) ? [[file, held.failure] as const] : [])),
    };
});

const reports = new Map<string, Graph>();

const keyed = (workspaceRoot: string, options: unknown) => hashArray([workspaceRoot, hashObject(options ?? {})]);

const graph = Effect.fnUntraced(function* (context: CreateNodesContext, matched: readonly string[], options: unknown) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const manifest = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
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
        onSome: (held) => Effect.succeed({ graph: held, failures: [] }),
        onNone: () =>
            Effect.tap(analyze(context, plugin, matched.toSorted()), ({ graph: report, failures }) =>
                failures.length === 0
                    ? Effect.sync(() => {
                          cache.set(hash, report);
                          cache.writeToDisk();
                      })
                    : Effect.void,
            ),
    });
    reports.set(keyed(context.workspaceRoot, options), built.graph);
    return built;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, options, context) =>
        Effect.runPromise(
            Effect.flatMap(
                Effect.mapError(graph(context, files, options), (error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])),
                ({ graph: built, failures }) =>
                    failures.length === 0
                        ? Effect.succeed(built.nodes)
                        : Effect.fail(
                              new AggregateCreateNodesError(
                                  failures.map(([file, error]) => [file, new Error(error.message)]),
                                  [...built.nodes],
                              ),
                          ),
            ).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies = (options, { workspaceRoot, projects, externalNodes }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const resolved = (at: string) => Option.orElse(Option.fromUndefinedOr(names.get(at)), () => Option.liftPredicate(at, (node) => node in (externalNodes ?? {})));
    return Record.values(
        Record.fromIterableWith(
            (reports.get(keyed(workspaceRoot, options))?.edges ?? []).flatMap((edge) => Option.toArray(Option.all([resolved(edge.source), resolved(edge.target)])).flatMap(([source, target]) => (source === target ? [] : [{ ...edge, source, target }]))),
            (edge) => [`${edge.source} ${edge.target}`, edge] as const,
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
