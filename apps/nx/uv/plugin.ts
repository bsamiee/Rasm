import { findPackageJSON } from 'node:module';
import { basename, dirname, join, relative } from 'node:path';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, hashFile, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, Option, Predicate, Record, Result, Schema, UndefinedOr } from 'effect';
import { CONFIGS, documents, Linked, manifested, printed, run, SHARED, targets, UvError } from '#/targets';
import { folders, LOCK, located, Metadata, report } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Read = PluginCache<{ readonly digest: string; readonly reply: string }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = combineGlobPatterns([LOCK, 'uv.toml', '.python-version', ...CONFIGS].map((file) => `**/${file}`));
const ROOT = '{workspaceRoot}';

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'outputPath: workspace folder holding distributions as <outputPath>/uv/<member> and coverage reports as <outputPath>/coverage/<member>, absent writes distributions under dist in each uv workspace and no coverage report',
});

const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (cache: Read, { workspaceRoot }: CreateNodesContext, outputPath: Option.Option<string>, digest: string, lock: string) {
    const placed = (yield* Schema.encodeEffect(Quoted)(workspaceRoot)).slice(1, -1);
    const reply = yield* Option.match(
        Option.filter(Option.fromNullishOr(cache.get(lock)), (held) => held.digest === digest),
        {
            onSome: (held) => Effect.succeed(held.reply.replaceAll(ROOT, placed)),
            onNone: () =>
                Effect.tap(
                    Effect.map(printed('uv', ['workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: join(workspaceRoot, dirname(lock)) }), ({ stdout }) => stdout),
                    (stdout) => Effect.sync(() => cache.set(lock, { digest, reply: stdout.replaceAll(placed, ROOT) })),
                ),
        },
    );
    const metadata = yield* Schema.decodeEffect(Metadata)(reply);
    const { documented, unparsed } = yield* documents(workspaceRoot, folders(workspaceRoot, metadata));
    const { members, edges, failures } = yield* located(workspaceRoot, report(workspaceRoot, lock, metadata, documented));
    yield* Array.match([...unparsed, ...failures], { onEmpty: () => Effect.void, onNonEmpty: (errors) => Effect.fail(UvError.of({ _tag: 'Refused', rule: errors.map(({ message }) => message).join('; ') }, lock)(errors)) });
    const links = yield* Linked;
    links.set(workspaceRoot, [...(links.get(workspaceRoot) ?? []), ...edges]);
    const manifests = yield* Effect.forEach(members, (member) => manifested(workspaceRoot, member.root));
    const projects = Record.fromIterableWith(Array.zip(members, manifests), ([member, manifest]): [string, ProjectConfiguration] => [
        member.root,
        {
            root: member.root,
            name: member.name,
            metadata: { technologies: ['python'] },
            ...(member.packaged && { tags: ['release'], release: { version: { versionActions: '@rasm/nx-uv/release/version-actions', versionActionsOptions: { lock } } } }),
            ...(member.implicit !== undefined && { implicitDependencies: [...member.implicit] }),
            targets: targets(member, outputPath, manifest),
        },
    ]);
    return { projects };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (matched, options, context) => {
        const cache: Read = new PluginCache(join(workspaceDataDirectory, 'uv.hash'));
        const outputPath = Option.flatMap(Schema.decodeOption(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) }), (placed) => Option.fromUndefinedOr(placed.outputPath));
        const locks = matched.filter((file) => basename(file) === LOCK);
        return run(
            Effect.gen(function* () {
                const digest = hashArray([...Array.fromNullishOr(UndefinedOr.map(findPackageJSON(import.meta.url), hashFile)), yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [FILES, join(relative(context.workspaceRoot, import.meta.dirname), '**')]))]);
                (yield* Linked).delete(context.workspaceRoot);
                const read = Array.zip(locks, yield* Effect.forEach(locks, (lock) => Effect.result(inferred(cache, context, outputPath, digest, lock)), { concurrency: 'unbounded' }));
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
