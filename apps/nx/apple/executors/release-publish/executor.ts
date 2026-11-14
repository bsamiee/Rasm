import { fileURLToPath } from 'node:url';
import { createProjectFileMapUsingProjectGraph, type ExecutorContext } from '@nx/devkit';
import { Completed } from '@rasm/process';
import cf from 'cf/package.json' with { type: 'json' };
import { Array, Config, Effect, FileSystem, Path, Record, Redacted, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { ReleaseVersion } from 'nx/src/command-line/release/utils/shared';
import { run } from '#/executors/run/executor';
import { refused } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    exports: Schema.Array(
        Schema.Struct({
            archive: Schema.String.annotate({ description: 'The .xcarchive whose app states its Sparkle feed in SUFeedURL' }),
            folder: Schema.String.annotate({ description: 'Folder holding the disk image of that app' }),
        }),
    ).annotate({ description: 'Archived apps whose disk image gets a Sparkle feed' }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
}).annotate({
    title: 'Release publish',
    description:
        "Signs a Sparkle feed for each disk image an export folder holds with generate_appcast under SPARKLE_ED_PRIVATE_KEY, pointing at the image's asset on the GitHub release nx release tags the archived app's version with, and uploads it to the R2 bucket SPARKLE_FEED_BUCKET names at the path of the app's SUFeedURL through cf r2 objects put; runs through nx release publish",
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String, CFBundleShortVersionString: Schema.String }));
const Assets = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.URLFromString })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const fed = Effect.fnUntraced(function* (context: ExecutorContext, { archive, folder }: (typeof Publish.Type)['exports'][number], dry: boolean) {
    const [fs, path, project] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.fromNullishOr(context.projectName)]);
    const images = (yield* fs.readDirectory(path.join(context.root, folder))).filter((name) => path.extname(name) === '.dmg');
    if (!Array.isReadonlyArrayNonEmpty(images)) {
        return;
    }
    const printed = (command: string, args: readonly string[]) => Completed.output(ChildProcess.make(command, args, { cwd: context.root, stdin: 'ignore' }));
    const { ApplicationPath, CFBundleShortVersionString: version } = yield* Effect.flatMap(printed('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')]), Schema.decodeEffect(Application));
    const owned = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(context.projectGraph));
    const { error, nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration?.release));
    const { groups = {} } = yield* Effect.mapError(Effect.fromNullishOr(nxReleaseConfig), () => refused(project, 'has no git tag: nx.json release does not resolve')(error?.code));
    const [releaseGroupName, group] = yield* Effect.mapError(Effect.fromOption(Record.findFirst(groups, ({ projects }) => projects.includes(project))), refused(project, 'has no git tag: no release group of nx.json release holds it'));
    const tag = new ReleaseVersion({ version, releaseTagPattern: group.releaseTag.pattern, projectName: project, releaseGroupName }).gitTag;
    if (dry) {
        return yield* Effect.logInfo(`Would publish the Sparkle feed of ${images.join(', ')} for ${tag} through generate_appcast and cf r2 objects put`);
    }
    const [key, bucket] = yield* Effect.all([Config.Redacted('SPARKLE_ED_PRIVATE_KEY'), Config.String('SPARKLE_FEED_BUCKET')]);
    const feed = yield* Effect.flatMap(printed('plutil', ['-extract', 'SUFeedURL', 'raw', '-o', '-', path.join(archive, 'Products', ApplicationPath, 'Contents', 'Info.plist')]), (stated) => Schema.decodeEffect(Schema.URLFromString)(stated.trim()));
    const { assets } = yield* Effect.flatMap(printed('gh', ['release', 'view', tag, '--json', 'assets']), Schema.decodeEffect(Assets));
    const asset = yield* Effect.mapError(Effect.fromOption(Array.findFirst(assets, ({ name }) => name === Array.headNonEmpty(images))), refused(tag, `holds no ${Array.headNonEmpty(images)}`));
    const staging = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(images, (image) => fs.copyFile(path.join(context.root, folder, image), path.join(staging, image)), { discard: true });
    yield* Completed.output(ChildProcess.make('generate_appcast', ['--ed-key-file', '-', '--download-url-prefix', new URL('.', asset.url).href, '-o', path.join(staging, 'appcast.xml'), staging], { stdin: Stream.encodeText(Stream.succeed(Redacted.value(key))) }));
    yield* printed(fileURLToPath(new URL(cf.bin.cf, import.meta.resolve('cf/package.json'))), ['r2', 'objects', 'put', feed.pathname.slice(1), '--bucket-name', bucket, '--file', path.join(staging, 'appcast.xml'), '--content-type', 'application/xml']);
    return yield* Effect.logInfo(`Published the Sparkle feed of ${images.join(', ')} at ${feed.href}`);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ exports, dryRun = false }: typeof Publish.Type, context: ExecutorContext) {
    const dry = yield* Config.withDefault(Config.Boolean('NX_DRY_RUN'), false);
    yield* Effect.forEach(exports, (exported) => fed(context, exported, dryRun || dry), { discard: true });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
