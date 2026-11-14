import { basename, dirname, join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Completed } from '@rasm/process';
import { Array, Cause, Context, Effect, Layer, ManagedRuntime, Option, Predicate, Record, Result, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { CONFIGS, documents, refused, SHARED, targets } from '#/targets';
import { folders, LOCK, located, Metadata, report } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Read = PluginCache<{ readonly digest: string; readonly metadata: typeof Metadata.Type }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([LOCK, 'uv.toml', '.python-version', ...CONFIGS].map((file) => `**/${file}`));

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'outputPath: workspace folder holding distributions as <outputPath>/uv/<member> and coverage reports as <outputPath>/coverage/<member>, absent writes distributions under dist in each uv workspace and no coverage report',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Links extends Context.Service<Links, Map<string, readonly RawProjectGraphDependency[]>>()('@rasm/nx-uv/plugin/Links') {
    static readonly layer = Layer.sync(Links, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (cache: Read, { workspaceRoot }: CreateNodesContext, outputPath: Option.Option<string>, digest: string, lock: string) {
    const metadata = yield* Option.match(
        Option.filter(Option.fromNullishOr(cache.get(lock)), (held) => held.digest === digest),
        {
            onSome: (held) => Effect.succeed(held.metadata),
            onNone: () =>
                Completed.output(ChildProcess.make('uv', ['workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: join(workspaceRoot, dirname(lock)), stdin: 'ignore' })).pipe(
                    Effect.flatMap(Schema.decodeEffect(Metadata)),
                    Effect.tap((read) => Effect.sync(() => cache.set(lock, { digest, metadata: read }))),
                ),
        },
    );
    const { documented, unparsed } = yield* documents(workspaceRoot, folders(workspaceRoot, metadata));
    const { members, edges, failures } = yield* located(workspaceRoot, report(workspaceRoot, lock, metadata, documented));
    yield* Array.match([...unparsed, ...failures], { onEmpty: () => Effect.void, onNonEmpty: (errors) => Effect.fail(new Cause.UnknownError(errors, errors.map(({ message }) => message).join('; '))) });
    const links = yield* Links;
    links.set(workspaceRoot, [...(links.get(workspaceRoot) ?? []), ...edges]);
    const projects = Record.fromIterableWith(members, (member): [string, ProjectConfiguration] => [
        member.root,
        {
            root: member.root,
            name: member.name,
            metadata: { technologies: ['python'] },
            ...(member.published && { tags: ['release'], release: { version: { versionActions: '@rasm/nx-uv/release/version-actions', versionActionsOptions: { lock } } } }),
            ...(member.implicit !== undefined && { implicitDependencies: [...member.implicit] }),
            targets: targets(member, outputPath),
        },
    ]);
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Links.layer, layer));

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) => {
        const cache: Read = new PluginCache(join(workspaceDataDirectory, 'uv.hash'));
        const outputPath = Option.flatMap(Schema.decodeOption(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) }), (placed) => Option.fromUndefinedOr(placed.outputPath));
        const locks = matched.filter((file) => basename(file) === LOCK);
        return runtime.runPromise(
            Effect.gen(function* () {
                const digest = yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [FILES]));
                (yield* Links).delete(context.workspaceRoot);
                const read = Array.zip(locks, yield* Effect.forEach(locks, (lock) => Effect.result(inferred(cache, context, outputPath, digest, lock)), { concurrency: 'unbounded' }));
                const failures = Array.filterMap(read, ([lock, result]) => Result.map(Result.flip(result), (error): [string, Error] => [lock, refused(lock, error.message)(error)]));
                const results = Array.filterMap(read, ([lock, result]) => Result.map(result, (held) => [lock, held] as const));
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
            }).pipe(Effect.ensuring(Effect.sync(() => cache.writeToDisk()))),
        );
    },
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    runtime.runPromise(
        Effect.map(Links, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            return Array.getSomes((links.get(workspaceRoot) ?? []).map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(edge.source)), target: Option.fromUndefinedOr(names.get(edge.target)) }), (named) => ({ ...edge, ...named }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
