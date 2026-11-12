import { fileURLToPath } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import { Dry, dried, executor, packaged, tagged } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import cf from 'cf/package.json' with { type: 'json' };
import { Array, Config, Effect, FileSystem, Path, Redacted, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Export = Schema.Struct({
    archive: Schema.String.annotate({ description: 'The .xcarchive whose app states its Sparkle feed in SUFeedURL' }),
    folder: Schema.String.annotate({ description: 'Folder holding the disk image of that app' }),
});
const Publish = Dry.mapFields(Struct.assign({ exports: Schema.Array(Export).annotate({ description: 'Archived apps whose disk image gets a Sparkle feed' }) })).annotate({
    title: 'Release publish',
    description:
        "Signs a Sparkle feed for each disk image the target's dependency targets wrote with generate_appcast under SPARKLE_ED_PRIVATE_KEY, pointing at the image's asset on the GitHub release of the project's version, and uploads it to the R2 bucket SPARKLE_FEED_BUCKET names at the path of the app's SUFeedURL through cf r2 objects put; runs through nx release publish",
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));
const Assets = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.URLFromString })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const fed = Effect.fnUntraced(function* (context: ExecutorContext, { archive }: typeof Export.Type, images: Array.NonEmptyReadonlyArray<string>, tag: string) {
    const [fs, path, key, bucket] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.Redacted('SPARKLE_ED_PRIVATE_KEY'), Config.String('SPARKLE_FEED_BUCKET')]);
    const { ApplicationPath } = yield* Effect.flatMap(printed('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application));
    const feed = yield* Effect.flatMap(printed('plutil', ['-extract', 'SUFeedURL', 'raw', '-o', '-', path.join(archive, 'Products', ApplicationPath, 'Contents', 'Info.plist')], { cwd: context.root }), (stated) => Schema.decodeEffect(Schema.URLFromString)(stated.trim()));
    const { assets } = yield* Effect.flatMap(printed('gh', ['release', 'view', tag, '--json', 'assets'], { cwd: context.root }), Schema.decodeEffect(Assets));
    const held = yield* Effect.mapError(Effect.fromOption(Array.findFirst(assets, ({ name }) => name === path.basename(Array.headNonEmpty(images)))), ToolError.of(tag, `holds no ${Array.headNonEmpty(images)}`));
    const staging = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(images, (image) => fs.copyFile(path.join(context.root, image), path.join(staging, path.basename(image))), { discard: true });
    yield* printed('generate_appcast', ['--ed-key-file', '-', '--download-url-prefix', new URL('.', held.url).href, '-o', path.join(staging, 'appcast.xml'), staging], { input: Redacted.value(key) });
    yield* printed(fileURLToPath(new URL(cf.bin.cf, import.meta.resolve('cf/package.json'))), ['r2', 'objects', 'put', feed.pathname.slice(1), '--bucket-name', bucket, '--file', path.join(staging, 'appcast.xml'), '--content-type', 'application/xml']);
    yield* Effect.logInfo(`Published the Sparkle feed of ${images.join(', ')} at ${feed.href}`);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ exports, ...stated }, context, project) {
        const [path, dry, files, tag] = yield* Effect.all([Path.Path, dried(stated), packaged(context), tagged(context, project)]);
        const feeds = exports.flatMap((exported) => Array.match(files.filter((file) => path.extname(file) === '.dmg' && path.dirname(file) === exported.folder), { onEmpty: () => [], onNonEmpty: (images) => [{ exported, images }] }));
        yield* Effect.forEach(feeds, ({ exported, images }) => (dry ? Effect.logInfo(`Would publish the Sparkle feed of ${images.join(', ')} for ${tag} through generate_appcast and cf r2 objects put`) : fed(context, exported, images, tag)), { discard: true });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
