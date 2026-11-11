import { createProjectGraphAsync, type ExecutorContext, joinPathFragments, type Tree } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, type ManagedRuntime, Match, Option, Path, Record, Schema, type Scope, Struct } from 'effect';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { sanitizeProjectNameForGitTag } from 'nx/src/command-line/release/utils/git';
import { interpolateReleaseConfig } from 'nx/src/command-line/release/utils/interpolate-release-config';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { printed, ran, runtime, ToolError } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope;

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Released = Dry.mapFields(
    Struct.assign({
        nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))).annotate({
            description: 'Versions nx release set in this run, a project it left unversioned is skipped; absent under nx release publish alone, where a version holding no release tag is skipped',
        }),
        registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry nx release publish passes as --registry, in the form the tool names one' }),
        tag: Schema.optionalKey(Schema.String).annotate({ description: 'Distribution tag nx release publish passes as --tag' }),
        access: Schema.optionalKey(Schema.String).annotate({ description: 'Access level nx release publish passes as --access' }),
        otp: Schema.optionalKey(Schema.Finite).annotate({ description: 'One-time password nx release publish passes as --otp' }),
        firstRelease: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Set by nx release publish --first-release, when the registry holds no earlier version' }),
    }),
);
const Viewed = Schema.fromJsonString(Schema.Struct({ isImmutable: Schema.Boolean, assets: Schema.Array(Schema.Struct({ name: Schema.String })) }));
const Locked = Schema.Struct({ lock: Schema.String });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Stated = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const released = Effect.fnUntraced(function* (options: typeof Released.Type, context: ExecutorContext, project: string) {
    const [path, dry] = yield* Effect.all([Path.Path, dried(options)]);
    const version = options.nxReleaseVersionData?.[project]?.newVersion;
    const named = Effect.fnUntraced(function* (set: string) {
        const owned = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(context.projectGraph));
        const { nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration.release));
        const [group, { releaseTag }] = yield* Effect.mapError(Effect.fromOption(Record.findFirst(nxReleaseConfig?.groups ?? {}, ({ projects }) => projects.includes(project))), ToolError.of(project, 'is in no release group of nx.json release'));
        return interpolateReleaseConfig(releaseTag.pattern, { version: set, projectName: sanitizeProjectNameForGitTag(project), releaseGroupName: group });
    });
    const tagged = Effect.fnUntraced(function* (set: string) {
        return options.nxReleaseVersionData !== undefined || (yield* ran('git', ['rev-parse', '--verify', '--quiet', `refs/tags/${yield* named(set)}`], { cwd: context.root })).code === 0;
    });
    const attached = Effect.fnUntraced(function* (set: string, files: readonly string[]) {
        const tag = yield* named(set);
        const view = Effect.flatMap(printed('gh', ['release', 'view', tag, '--json', 'isImmutable,assets'], { cwd: context.root }), Schema.decodeEffect(Viewed));
        const { isImmutable, assets } = yield* dry ? Effect.orElseSucceed(view, () => ({ isImmutable: false, assets: [] })) : view;
        const held = new Set(assets.map(({ name }) => name));
        const missing = files.filter((file) => !held.has(path.basename(file)));
        yield* Match.value({ attached: missing.length === 0, immutable: isImmutable, dry }).pipe(
            Match.when({ attached: true }, () => Effect.logInfo(`Skipped ${files.join(', ')}, GitHub release ${tag} already holds each`)),
            Match.when({ immutable: true }, () => ToolError.make({ subject: tag, detail: `is an immutable GitHub release, which takes no asset after nx release publishes it, so ${missing.join(', ')} stay unattached` })),
            Match.when({ dry: true }, () => Effect.logInfo(`Would run gh release upload ${tag} ${missing.join(' ')}`)),
            Match.orElse(() => Effect.andThen(printed('gh', ['release', 'upload', tag, ...missing], { cwd: context.root }), Effect.logInfo(`Attached ${missing.join(', ')} to GitHub release ${tag}`))),
        );
        return tag;
    });
    return { dry, version: Option.fromNullishOr(version), unversioned: version === null, tagged, attached };
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
