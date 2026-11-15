import { basename, dirname, join } from 'node:path';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, Option, Record, Result, Schema } from 'effect';
import { CONFIGS, documents, Linked, manifested, Options, PLUGIN, run, spawned, targets } from '#/targets';
import { folders, LOCK, located, Metadata, report } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Read = PluginCache<{ readonly digest: string; readonly reply: string }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([LOCK, 'uv.toml', '.python-version', ...CONFIGS].map((file) => `**/${file}`));
const ROOT = '{workspaceRoot}';

// --- [MODELS] --------------------------------------------------------------------------

const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const inferred = Effect.fnUntraced(function* (cache: Read, { workspaceRoot }: CreateNodesContext, settings: typeof Options.Type, digest: string, lock: string) {
    const placed = (yield* Schema.encodeEffect(Quoted)(workspaceRoot)).slice(1, -1);
    const reply = yield* Option.match(
        Option.filter(Option.fromNullishOr(cache.get(lock)), (held) => held.digest === digest),
        {
            onSome: (held) => Effect.succeed(held.reply.replaceAll(ROOT, placed)),
            onNone: () => Effect.tap(spawned(['uv', 'workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: join(workspaceRoot, dirname(lock)) }), (stdout) => Effect.sync(() => cache.set(lock, { digest, reply: stdout.replaceAll(placed, ROOT) }))),
        },
    );
    const metadata = yield* Schema.decodeEffect(Metadata)(reply);
    const { documented, unparsed } = yield* documents(workspaceRoot, folders(workspaceRoot, metadata));
    const { members, edges, failures } = yield* located(workspaceRoot, report(workspaceRoot, lock, metadata, documented));
    yield* Array.match([...unparsed, ...failures], { onEmpty: () => Effect.void, onNonEmpty: (errors) => Effect.fail(new Cause.UnknownError(errors, `${lock}: ${errors.map(({ message }) => message).join('; ')}`)) });
    const links = yield* Linked;
    links.set(workspaceRoot, [...(links.get(workspaceRoot) ?? []), ...edges]);
    const manifests = yield* Effect.forEach(members, (member) => manifested(workspaceRoot, member.root));
    const projects = Record.fromIterableWith(Array.zip(members, manifests), ([member, stated]): [string, ProjectConfiguration] => [
        member.root,
        {
            root: member.root,
            name: member.name,
            metadata: { technologies: ['python'] },
            ...(member.packaged && { ...(settings.releaseTag !== undefined && { tags: [settings.releaseTag] }), release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { lock } } } }),
            ...(member.implicit !== undefined && { implicitDependencies: [...member.implicit] }),
            targets: targets(member, settings, stated),
        },
    ]);
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (matched, options, context) => {
        const cache: Read = new PluginCache(join(workspaceDataDirectory, `uv-${hashObject(options ?? {})}.hash`));
        const locks = matched.filter((file) => basename(file) === LOCK);
        return run(
            Effect.gen(function* () {
                const settings = yield* Schema.decodeEffect(Options)(options ?? {});
                const digest = yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [FILES]));
                (yield* Linked).delete(context.workspaceRoot);
                const read = Array.zip(locks, yield* Effect.forEach(locks, (lock) => Effect.result(Effect.catchCause(inferred(cache, context, settings, digest, lock), rendered)), { concurrency: 'unbounded' }));
                const failures = Array.filterMap(read, ([lock, result]) => Result.map(Result.flip(result), (error): [string, Error] => [lock, error]));
                const results = Array.filterMap(read, ([lock, result]) => Result.map(result, (held) => [lock, held] as const));
                return yield* failures.length === 0 ? Effect.succeed(results) : Effect.fail(new AggregateCreateNodesError(failures, results));
            }).pipe(Effect.ensuring(Effect.sync(() => cache.writeToDisk()))),
        );
    },
];

const createDependencies: CreateDependencies = (_, { projects, workspaceRoot }) =>
    run(
        Effect.map(Linked, (links) => {
            const names = createProjectRootMappingsFromProjectConfigurations(projects);
            return Array.getSomes((links.get(workspaceRoot) ?? []).map((edge) => Option.map(Option.all({ source: Option.fromUndefinedOr(names.get(edge.source)), target: Option.fromUndefinedOr(names.get(edge.target)) }), (named) => ({ ...edge, ...named }))));
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
