import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Config, Effect, FileSystem, Match, Option, Path, Schema } from 'effect';
import { AppleError, spawned } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    files: Schema.Array(Schema.String),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Release = Schema.fromJsonString(Schema.Struct({ isImmutable: Schema.Boolean, assets: Schema.Array(Schema.Struct({ name: Schema.String })) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const executed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [{ files, dryRun, nxReleaseVersionData }, environment, fs, path] = yield* Effect.all([Schema.decodeUnknownEffect(Publish)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), FileSystem.FileSystem, Path.Path]);
    const project = yield* Effect.fromNullishOr(context.projectName);
    const resolved = nxReleaseVersionData?.[project]?.newVersion;
    if (resolved === null) {
        return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), { success: true });
    }
    const pattern = yield* Effect.mapError(Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern), () => AppleError.make({ subject: 'nx.json release.releaseTag.pattern', detail: 'is unset, and names the GitHub release each zip attaches to' }));
    const named = pattern.replaceAll('{projectName}', project);
    const tag = resolved === undefined ? (yield* spawned('git', ['describe', '--tags', '--abbrev=0', '--match', named.replaceAll('{version}', '*')], { cwd: context.root })).trim() : named.replaceAll('{version}', resolved);
    const missing = yield* Effect.filter(files, (file) => Effect.map(fs.exists(path.join(context.root, file)), (present) => !present));
    yield* Array.match(missing, { onEmpty: () => Effect.void, onNonEmpty: (absent) => AppleError.make({ subject: project, detail: `${absent.join(', ')} is missing; run nx build ${project} -c release first` }) });
    const dry = dryRun ?? environment;
    const release = dry ? Option.none() : Option.some(yield* Effect.flatMap(spawned('gh', ['release', 'view', tag, '--json', 'isImmutable,assets'], { cwd: context.root }), Schema.decodeEffect(Release)));
    const held = new Set(Option.toArray(release).flatMap(({ assets }) => assets.map(({ name }) => name)));
    const immutable = Option.exists(release, ({ isImmutable }) => isImmutable);
    yield* Effect.forEach(
        files,
        (file) =>
            Match.value({ dry, held: held.has(path.basename(file)), immutable }).pipe(
                Match.when({ dry: true }, () => Effect.logInfo(`Would run gh release upload ${tag} ${file}`)),
                Match.when({ held: true }, () => Effect.logInfo(`Skipped ${file}, GitHub release ${tag} already holds it`)),
                Match.when({ immutable: true }, () => AppleError.make({ subject: tag, detail: `is an immutable GitHub release, which takes no asset once published, and nx release publishes it before nx-release-publish attaches ${file}` })),
                Match.orElse(() => Effect.andThen(spawned('gh', ['release', 'upload', tag, file], { cwd: context.root }), Effect.logInfo(`Attached ${file} to GitHub release ${tag}`))),
            ),
        { discard: true },
    );
    return { success: true };
}, Effect.provide(layer));

const execute = (options: unknown, context: ExecutorContext) => executed(options, context).pipe(Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
