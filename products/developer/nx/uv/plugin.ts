import { join, posix } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, createNodesFromFiles, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, createProjectRootMappingsFromProjectConfigurations, globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { above, fingerprinted, inferring, manifested, spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, MutableRef, Option, Record, Schema } from 'effect';
import { CONFIGS, Linked, Options, PLUGIN, run, targets, UV } from '#/targets';
import { LOCK, located, MANIFEST, Metadata, SCHEMA } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ: readonly string[] = [LOCK, MANIFEST, UV.settings, ...UV.pins];
const FILES = combineGlobPatterns([LOCK, MANIFEST].map((file) => `**/${file}`));
const FOUND = Array.difference(Array.dedupe([...READ, ...CONFIGS]), [LOCK, MANIFEST]).map((file) => `**/${file}`);
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (pass: { readonly cache: PluginCache<{ readonly digest: string; readonly reply: string }>; readonly stored: MutableRef.MutableRef<boolean>; readonly files: ReadonlySet<string> }, { workspaceRoot }: CreateNodesContext, settings: Options, lock: string) {
    const token = '{workspaceRoot}';
    const folder = posix.dirname(lock);
    const parents = new Set(above(posix.dirname(folder)));
    const placed = (yield* Schema.encodeEffect(Quoted)(workspaceRoot)).slice(1, -1);
    const read = [...pass.files].filter((file) => READ.includes(posix.basename(file)) && (folder === '.' || file.startsWith(`${folder}/`) || parents.has(posix.dirname(file))));
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, read)), yield* OWN]);
    const metadata = yield* Option.match(
        Option.filter(Option.fromNullishOr(pass.cache.get(lock)), (held) => held.digest === digest),
        {
            onSome: (held) => Schema.decodeEffect(Metadata)(held.reply.replaceAll(token, placed)),
            onNone: Effect.fnUntraced(function* () {
                const decoded = yield* Effect.flatMap(spawned(['uv', 'workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', SCHEMA, '--quiet'], { cwd: join(workspaceRoot, folder) }), Schema.decodeEffect(Metadata));
                const reply = yield* Schema.encodeEffect(Metadata)(decoded);
                pass.cache.set(lock, { digest, reply: reply.replaceAll(placed, token) });
                MutableRef.set(pass.stored, true);
                return decoded;
            }),
        },
    );
    const { members, failures } = yield* located(workspaceRoot, pass.files, lock, metadata);
    yield* Array.match(failures, { onEmpty: () => Effect.void, onNonEmpty: (errors) => Effect.fail(new Cause.UnknownError(errors, `${lock}: ${errors.map(({ message }) => message).join('; ')}`)) });
    const links = yield* Linked;
    links.set(workspaceRoot, [...(links.get(workspaceRoot) ?? []), ...members.flatMap(({ edges }) => edges)]);
    const projects = yield* Effect.forEach(members, (member) =>
        Effect.map(manifested(workspaceRoot, member.root), (stated): [string, ProjectConfiguration] => [
            member.root,
            {
                root: member.root,
                name: member.name,
                metadata: { technologies: ['python'] },
                ...(member.packaged && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { lock } } } }),
                ...(member.implicit !== undefined && { implicitDependencies: [...member.implicit] }),
                targets: targets(member, settings, stated, workspaceRoot),
            },
        ]),
    );
    return { projects: Record.fromEntries(projects) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (matched, settings, context) {
        const found = yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, FOUND));
        const pass = { cache: new PluginCache<{ readonly digest: string; readonly reply: string }>(join(workspaceDataDirectory, 'uv.hash')), stored: MutableRef.make(false), files: new Set([...matched, ...found]) };
        const locks = matched.filter((file) => posix.basename(file) === LOCK);
        (yield* Linked).delete(context.workspaceRoot);
        return yield* Effect.ensuring(
            Effect.promise(() => createNodesFromFiles((lock) => run(inferred(pass, context, settings, lock)), locks, settings, context)),
            Effect.when(
                Effect.sync(() => pass.cache.writeToDisk()),
                Effect.sync(() => MutableRef.get(pass.stored)),
            ),
        );
    }),
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
