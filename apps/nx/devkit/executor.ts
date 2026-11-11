import { type ExecutorContext, readCachedProjectGraph } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, type ManagedRuntime, Option, Path, Schema, type Scope, Struct } from 'effect';
import type { AfterAllProjectsVersioned } from 'nx/release';
import { printed, runtime } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope;

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Released = Dry.mapFields(
    Struct.assign({
        nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))).annotate({ description: 'Versions nx release set in this run, a project it left unversioned is skipped' }),
    }),
);
const Locked = Schema.Struct({ lock: Schema.String });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const released = Effect.fnUntraced(function* (options: typeof Released.Type, project: string) {
    const version = options.nxReleaseVersionData?.[project]?.newVersion;
    return { dry: yield* dried(options), version: Option.fromNullishOr(version), unversioned: version === null };
});

const relocked =
    (lock: string, command: Array.NonEmptyReadonlyArray<string>): AfterAllProjectsVersioned =>
    (cwd, { dryRun, rootVersionActionsOptions }) =>
        runtime.runPromise(
            Effect.gen(function* () {
                const [path, fs, { skipLockFileUpdate }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeEffect(Unlocked)(rootVersionActionsOptions ?? {})]);
                const locks =
                    dryRun === true || skipLockFileUpdate === true
                        ? []
                        : Array.dedupe(
                              Object.values(readCachedProjectGraph().nodes).flatMap((node) =>
                                  Schema.decodeUnknownOption(Locked)(node.data.release?.version?.versionActionsOptions).pipe(
                                      Option.map((held) => held.lock),
                                      Option.filter((held) => path.basename(held) === lock),
                                      Option.toArray,
                                  ),
                              ),
                          );
                const changed = yield* Effect.filter(locks, (held) =>
                    Effect.zipWith(fs.readFileString(path.join(cwd, held)), Effect.andThen(printed(Array.headNonEmpty(command), Array.tailNonEmpty(command), { cwd: path.join(cwd, path.dirname(held)) }), fs.readFileString(path.join(cwd, held))), (before, after) => before !== after),
                );
                return { changedFiles: changed, deletedFiles: [] };
            }),
        );

// --- [COMPOSITION] ---------------------------------------------------------------------

const executor = <A, E>(options: Schema.Decoder<A>, run: (options: A, context: ExecutorContext, project: string) => Effect.Effect<boolean, E, Services>) =>
    Object.assign(
        (raw: unknown, context: ExecutorContext) =>
            runtime.runPromise(
                Effect.scoped(
                    Effect.gen(function* () {
                        const [decoded, project] = yield* Effect.all([Schema.decodeUnknownEffect(options)(raw), Effect.fromNullishOr(context.projectName)]);
                        return { success: yield* run(decoded, context, project) };
                    }),
                ),
            ),
        { options },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dry, dried, executor, Released, released, relocked };
