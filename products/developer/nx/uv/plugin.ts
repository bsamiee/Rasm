import { join, posix } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, createNodesFromFiles, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { fingerprinted, manifested, spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, Option, Record, Schema } from 'effect';
import { ancestors, CONFIGS, documents, Linked, Options, PLUGIN, run, targets } from '#/targets.ts';
import { folders, LOCK, located, MANIFEST, Metadata, report, SCHEMA } from '#/workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Read = PluginCache<{ readonly digest: string; readonly reply: string }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [LOCK, MANIFEST, 'uv.toml', '.python-version'];
const FILES = combineGlobPatterns(Array.dedupe([...READ, ...CONFIGS]).map((file) => `**/${file}`));
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (cache: Read, { workspaceRoot }: CreateNodesContext, settings: typeof Options.Type, lock: string) {
    const token = '{workspaceRoot}';
    const placed = (yield* Schema.encodeEffect(Quoted)(workspaceRoot)).slice(1, -1);
    const read = Array.cartesianWith([posix.join(posix.dirname(lock), '**'), ...ancestors(posix.dirname(posix.dirname(lock)))], READ, posix.join);
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, read)), yield* OWN]);
    const metadata = yield* Option.match(
        Option.filter(Option.fromNullishOr(cache.get(lock)), (held) => held.digest === digest),
        {
            onSome: (held) => Schema.decodeEffect(Metadata)(held.reply.replaceAll(token, placed)),
            onNone: Effect.fnUntraced(function* () {
                const decoded = yield* Effect.flatMap(spawned(['uv', 'workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', SCHEMA, '--quiet'], { cwd: join(workspaceRoot, posix.dirname(lock)) }), Schema.decodeEffect(Metadata));
                const reply = yield* Schema.encodeEffect(Metadata)(decoded);
                cache.set(lock, { digest, reply: reply.replaceAll(placed, token) });
                cache.writeToDisk();
                return decoded;
            }),
        },
    );
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
            ...(member.packaged && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { lock } } } }),
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
        const cache: Read = new PluginCache(join(workspaceDataDirectory, 'uv.hash'));
        const locks = matched.filter((file) => posix.basename(file) === LOCK);
        return run(
            Effect.gen(function* () {
                const settings = yield* Schema.decodeEffect(Options)(options ?? {});
                (yield* Linked).delete(context.workspaceRoot);
                return yield* Effect.promise(() => createNodesFromFiles((lock) => run(inferred(cache, context, settings, lock)), locks, options, context));
            }),
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
