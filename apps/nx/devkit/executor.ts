import { createProjectGraphAsync, type ExecutorContext, joinPathFragments, type Tree } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, type ManagedRuntime, Option, Path, Schema, type Scope, Struct } from 'effect';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { printed, runtime, ToolError } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope;

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Released = Dry.mapFields(
    Struct.assign({
        nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))).annotate({
            description: 'Versions nx release set in this run, a project it left unversioned is skipped',
        }),
        registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry nx release publish passes as --registry, in the form the tool names one' }),
        tag: Schema.optionalKey(Schema.String).annotate({ description: 'Distribution tag nx release publish passes as --tag' }),
        access: Schema.optionalKey(Schema.String).annotate({ description: 'Access level nx release publish passes as --access' }),
        otp: Schema.optionalKey(Schema.Finite).annotate({ description: 'One-time password nx release publish passes as --otp' }),
        firstRelease: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Set by nx release publish --first-release, when the registry holds no earlier version' }),
    }),
);
const Locked = Schema.Struct({ lock: Schema.String });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Stated = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const released = (options: typeof Released.Type, project: string) => Effect.map(dried(options), (dry) => ({ dry, unversioned: options.nxReleaseVersionData?.[project]?.newVersion === null }));

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
                              Object.values((yield* Effect.promise(() => createProjectGraphAsync())).nodes).flatMap((node) =>
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

const edited = Effect.fnUntraced(function* ({ projectGraphNode: { name, data }, validManifestFilenames }: Manifested, tree: Tree, edit: (text: string, at: string) => Effect.Effect<string, Error, Services>) {
    const file = yield* Effect.mapError(Effect.fromOption(Array.head(validManifestFilenames)), ToolError.of(name, 'names no manifest in its versionActionsOptions'));
    const at = joinPathFragments(data.root, file);
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), ToolError.of(at, 'is missing'));
    return { at, result: yield* edit(text, at) };
});

// --- [SERVICES] ------------------------------------------------------------------------

abstract class Manifested extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    abstract stated(text: string, at: string): Effect.Effect<string, Error, Services>;

    abstract restated(text: string, version: string, at: string): Effect.Effect<string, Error, Services>;

    override get validManifestFilenames(): string[] {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Stated)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return runtime.runPromise(
            Effect.scoped(
                Effect.map(
                    edited(this, tree, (text, at) => this.stated(text, at)),
                    ({ at, result }) => ({ currentVersion: result, manifestPath: at }),
                ),
            ),
        );
    }

    override readCurrentVersionFromRegistry() {
        return runtime.runPromise(Effect.succeed(null));
    }

    override readCurrentVersionOfDependency() {
        return runtime.runPromise(Effect.succeed({ currentVersion: null, dependencyCollection: null }));
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return runtime.runPromise(
            Effect.scoped(
                Effect.map(
                    edited(this, tree, (text, at) => this.restated(text, newVersion, at)),
                    ({ at, result }) => {
                        tree.write(at, result);
                        return [`New version ${newVersion} written to manifest: ${at}`];
                    },
                ),
            ),
        );
    }

    override updateProjectDependencies() {
        return runtime.runPromise(Effect.succeed(Array.empty<string>()));
    }
}

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

export { Dry, dried, executor, Manifested, Released, released, relocked };
