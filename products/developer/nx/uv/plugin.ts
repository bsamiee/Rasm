import { join, posix } from 'node:path';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, hashArray, type ProjectConfiguration } from '@nx/devkit';
import { combineGlobPatterns, globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { above, fingerprinted, inferring, manifested, run, spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, MutableRef, Option, type Record, Schema } from 'effect';
import { CONFIGS, Options, PLUGIN, targets, UV } from '#/targets';
import { LOCK, located, MANIFEST, Metadata, SCHEMA } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ: readonly string[] = [LOCK, MANIFEST, UV.settings, ...UV.pins];
const FILES = combineGlobPatterns([LOCK, MANIFEST].map((file) => `**/${file}`));
const FOUND = Array.difference(Array.dedupe([...READ, ...CONFIGS]), [LOCK, MANIFEST]).map((file) => `**/${file}`);
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const inferred = Effect.fnUntraced(function* (pass: { readonly cache: PluginCache<{ readonly digest: string; readonly reply: string }>; readonly stored: MutableRef.MutableRef<boolean>; readonly files: ReadonlySet<string> }, { workspaceRoot }: CreateNodesContext, lock: string) {
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
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(members), onNonEmpty: (errors) => Effect.fail(new Cause.UnknownError(errors, `${lock}: ${errors.map(({ message }) => message).join('; ')}`)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (matched, settings, context) {
        const found = yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, FOUND));
        const pass = { cache: new PluginCache<{ readonly digest: string; readonly reply: string }>(join(workspaceDataDirectory, 'uv.hash')), stored: MutableRef.make(false), files: new Set([...matched, ...found]) };
        const [read, refused] = yield* Effect.ensuring(
            Effect.partition(
                matched.filter((file) => posix.basename(file) === LOCK),
                (lock) => Effect.mapError(inferred(pass, context, lock), (error): [string, Error] => [lock, error]),
                { concurrency: 'unbounded' },
            ),
            Effect.when(
                Effect.sync(() => pass.cache.writeToDisk()),
                Effect.sync(() => MutableRef.get(pass.stored)),
            ),
        );
        const members = read.flat();
        const roots = new Set(members.map(({ root }) => root));
        const stated = yield* Effect.forEach(members, ({ root }) => manifested(context.workspaceRoot, root), { concurrency: 'unbounded' });
        const results = Array.zipWith(members, stated, (member, manifest): [string, { readonly projects: Record<string, ProjectConfiguration> }] => [
            member.lock,
            {
                projects: {
                    [member.root]: {
                        root: member.root,
                        name: member.name,
                        metadata: { technologies: ['python'] },
                        ...(member.packaged && { tags: ['release'], release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } }),
                        implicitDependencies: member.linked.filter((root) => roots.has(root)).map((root) => `directory:${root}`),
                        targets: targets(member, settings, manifest, context.workspaceRoot),
                    },
                },
            },
        ]);
        return yield* Array.match(refused, { onEmpty: () => Effect.succeed(results), onNonEmpty: (errors) => Effect.fail(new AggregateCreateNodesError(errors, results)) });
    }),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
