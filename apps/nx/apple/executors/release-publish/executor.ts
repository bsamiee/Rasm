import { fileURLToPath } from 'node:url';
import { createProjectFileMapUsingProjectGraph, type ExecutorContext } from '@nx/devkit';
import cf from 'cf/package.json' with { type: 'json' };
import { Array, Cause, Config, Effect, FileSystem, Function, flow, Option, Path, Record, Result, Schema, Stream, String } from 'effect';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { ReleaseVersion } from 'nx/src/command-line/release/utils/shared';
import { run, spawned } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    exports: Schema.Array(
        Schema.Struct({
            archive: Schema.String.annotate({ description: 'The .xcarchive whose app states its Sparkle feed in SUFeedURL' }),
            folder: Schema.String.annotate({ description: 'Folder holding the disk image of that app' }),
        }),
    ).annotate({ description: 'Archived apps whose disk image gets a Sparkle feed' }),
    appcast: Schema.Struct({
        key: Schema.String.annotate({ description: 'Variable holding the Sparkle EdDSA private key generate_appcast signs with' }),
        bucket: Schema.String.annotate({ description: 'Variable holding the R2 bucket the feed is uploaded to' }),
    }).annotate({ description: 'Variables naming the Sparkle key and the R2 bucket of the feed' }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({
    title: 'Release publish',
    description:
        "Signs a Sparkle feed for each disk image an export folder holds with generate_appcast under the key appcast.key names, pointing at the image's asset on the GitHub release nx release tags the archived app's version with, and uploads it to the R2 bucket appcast.bucket names at the path of the app's SUFeedURL with cf r2 objects put, and where a folder holds no disk image or either variable is unset names what is missing and publishes nothing; runs through nx release publish",
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, CFBundleShortVersionString: Schema.String }));
const Assets = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.URLFromString })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const fed = Effect.fnUntraced(function* (context: ExecutorContext, { archive, folder }: (typeof Publish.Type)['exports'][number], appcast: (typeof Publish.Type)['appcast'], dry: boolean, words: readonly string[]) {
    const [fs, path, project] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.fromNullishOr(context.projectName)]);
    const images = (yield* fs.readDirectory(path.join(context.root, folder))).filter((name) => path.extname(name) === '.dmg');
    if (!Array.isReadonlyArrayNonEmpty(images)) {
        return yield* Effect.logWarning(`Skipped the Sparkle feed of ${folder}: it holds no disk image, which pack--ditto writes where the variables its notary option names are set`);
    }
    const { ApplicationPath, CFBundleShortVersionString: version } = yield* Effect.flatMap(spawned(['plutil', '-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application));
    const owned = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(context.projectGraph));
    const { error, nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration?.release));
    const { groups = {} } = yield* Effect.mapError(Effect.fromNullishOr(nxReleaseConfig), () => new Cause.UnknownError(error, [`${project} has no git tag: nx.json release does not resolve`, ...Array.fromNullishOr(error?.code)].join(': ')));
    const [releaseGroupName, group] = yield* Effect.mapError(Effect.fromOption(Record.findFirst(groups, ({ projects }) => projects.includes(project))), (cause) => new Cause.UnknownError(cause, `${project} has no git tag: no release group of nx.json release holds it`));
    const tag = new ReleaseVersion({ version, releaseTagPattern: group.releaseTag.pattern, projectName: project, releaseGroupName }).gitTag;
    if (dry) {
        return yield* Effect.logInfo(`Would publish the Sparkle feed of ${images.join(', ')} for ${tag} through generate_appcast and cf r2 objects put`);
    }
    const configured = (variable: string) => Effect.map(Config.option(Config.String(variable)), flow(Option.filter(String.isNonEmpty), Result.fromOption(Function.constant(variable))));
    const held = yield* Effect.all({ key: configured(appcast.key), bucket: configured(appcast.bucket) });
    const signed = Effect.fnUntraced(function* ({ key, bucket }: { readonly key: string; readonly bucket: string }) {
        const feed = yield* Effect.flatMap(spawned(['plutil', '-extract', 'SUFeedURL', 'raw', '-o', '-', path.join(archive, 'Products', ApplicationPath, 'Contents', 'Info.plist')], { cwd: context.root }), (stated) => Schema.decodeEffect(Schema.URLFromString)(stated.trim()));
        const { assets } = yield* Effect.flatMap(spawned(['gh', 'release', 'view', tag, '--json', 'assets'], { cwd: context.root }), Schema.decodeEffect(Assets));
        const asset = yield* Effect.mapError(Effect.fromOption(Array.findFirst(assets, ({ name }) => name === Array.headNonEmpty(images))), (cause) => new Cause.UnknownError(cause, `${tag} holds no ${Array.headNonEmpty(images)}`));
        const staging = yield* fs.makeTempDirectoryScoped();
        yield* Effect.forEach(images, (image) => fs.copyFile(path.join(context.root, folder, image), path.join(staging, image)), { discard: true });
        yield* spawned(['generate_appcast', '--ed-key-file', '-', '--download-url-prefix', new URL('.', asset.url).href, '-o', path.join(staging, 'appcast.xml'), ...words, staging], { cwd: context.root, stdin: Stream.encodeText(Stream.make(key)) });
        yield* spawned([fileURLToPath(new URL(cf.bin.cf, import.meta.resolve('cf/package.json'))), 'r2', 'objects', 'put', feed.pathname.slice(1), '--bucket-name', bucket, '--file', path.join(staging, 'appcast.xml'), '--content-type', 'application/xml'], { cwd: context.root });
        return yield* Effect.logInfo(`Published the Sparkle feed of ${images.join(', ')} at ${feed.href}`);
    });
    return yield* Result.match(Result.all(held), { onFailure: () => Effect.logWarning(`Skipped the Sparkle feed of ${images.join(', ')}: ${Array.getFailures(Object.values(held)).join(' and ')} not set`), onSuccess: signed });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ exports, appcast, dryRun = false, __unparsed__: words = [] }: typeof Publish.Type, context: ExecutorContext) {
    const dry = yield* Config.withDefault(Config.Boolean('NX_DRY_RUN'), false);
    yield* Effect.forEach(exports, (exported) => fed(context, exported, appcast, dryRun || dry, words), { discard: true });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
