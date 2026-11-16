import { readdirSync } from 'node:fs';
import { basename, dirname, join } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, createNodesFromFiles, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, hashFile, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, Option, Record, Schema } from 'effect';
import { ancestors, CONFIGS, documents, Linked, manifested, Options, PLUGIN, run, spawned, targets } from '#/targets';
import { folders, LOCK, located, MANIFEST, Metadata, report, SCHEMA } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Read = PluginCache<{ readonly digest: string; readonly reply: string }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = [LOCK, MANIFEST, 'uv.toml', '.python-version'];
const FILES = combineGlobPatterns(Array.dedupe([...READ, ...CONFIGS]).map((file) => `**/${file}`));
const OWN = hashArray(
    readdirSync(import.meta.dirname)
        .toSorted()
        .flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]),
);

// --- [MODELS] --------------------------------------------------------------------------

const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (cache: Read, { workspaceRoot }: CreateNodesContext, settings: typeof Options.Type, lock: string) {
    const token = '{workspaceRoot}';
    const placed = (yield* Schema.encodeEffect(Quoted)(workspaceRoot)).slice(1, -1);
    const read = Array.cartesianWith([join(dirname(lock), '**'), ...ancestors(dirname(dirname(lock)))], READ, join);
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, read)), OWN]);
    const metadata = yield* Option.match(
        Option.filter(Option.fromNullishOr(cache.get(lock)), (held) => held.digest === digest),
        {
            onSome: (held) => Schema.decodeEffect(Metadata)(held.reply.replaceAll(token, placed)),
            onNone: Effect.fnUntraced(function* () {
                const decoded = yield* Effect.flatMap(spawned(['uv', 'workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', SCHEMA, '--quiet'], { cwd: join(workspaceRoot, dirname(lock)) }), Schema.decodeEffect(Metadata));
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
        const locks = matched.filter((file) => basename(file) === LOCK);
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
