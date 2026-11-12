import type { ExecutorContext } from '@nx/devkit';
import { attached, executor, Released, released, written } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Config, Effect, FileSystem, Path, Redacted, Schema, Struct } from 'effect';
import { versioned } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Export = Schema.Struct({
    archive: Schema.String.annotate({ description: 'The .xcarchive whose app states its Sparkle feed in SUFeedURL' }),
    folder: Schema.String.annotate({ description: 'Folder holding the disk image of that app' }),
});
const Publish = Released.mapFields(
    Struct.assign({
        manifest: Schema.String.annotate({ description: 'Info.plist or Xcode project file stating the released version, relative to the workspace root' }),
        exports: Schema.optionalKey(Schema.Array(Export)).annotate({ description: 'Archived apps whose disk image gets a Sparkle feed' }),
    }),
).annotate({
    title: 'Release publish',
    description:
        "Attaches each disk image or zip the target's dependency targets wrote to the GitHub release of the version the manifest states through gh release upload, then for each disk image it uploaded signs a Sparkle feed with generate_appcast under SPARKLE_ED_PRIVATE_KEY and uploads it to the R2 bucket SPARKLE_FEED_BUCKET names at the path of the app's SUFeedURL through cf r2 objects put; skips a file that release holds and a project nx release left unversioned; runs through nx release publish",
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));
const Assets = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.URLFromString })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const fed = Effect.fnUntraced(function* (context: ExecutorContext, { archive }: typeof Export.Type, images: Array.NonEmptyReadonlyArray<string>, tag: string, dry: boolean) {
    if (dry) {
        return yield* Effect.logInfo(`Would publish the Sparkle feed of ${images.join(', ')} through generate_appcast and cf r2 objects put`);
    }
    const [fs, path, key, bucket] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.Redacted('SPARKLE_ED_PRIVATE_KEY'), Config.String('SPARKLE_FEED_BUCKET')]);
    const { ApplicationPath } = yield* Effect.flatMap(printed('plutil', ['-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application));
    const feed = yield* Effect.flatMap(printed('plutil', ['-extract', 'SUFeedURL', 'raw', '-o', '-', path.join(archive, 'Products', ApplicationPath, 'Contents', 'Info.plist')], { cwd: context.root }), (stated) => Schema.decodeEffect(Schema.URLFromString)(stated.trim()));
    const { assets } = yield* Effect.flatMap(printed('gh', ['release', 'view', tag, '--json', 'assets'], { cwd: context.root }), Schema.decodeEffect(Assets));
    const held = yield* Effect.mapError(Effect.fromOption(Array.findFirst(assets, ({ name }) => name === path.basename(Array.headNonEmpty(images)))), ToolError.of(tag, `holds no ${Array.headNonEmpty(images)}`));
    const staging = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(images, (image) => fs.copyFile(path.join(context.root, image), path.join(staging, path.basename(image))), { discard: true });
    yield* printed('generate_appcast', ['--ed-key-file', '-', '--download-url-prefix', new URL('.', held.url).href, '-o', path.join(staging, 'appcast.xml'), staging], { input: Redacted.value(key) });
    yield* printed('cf', ['r2', 'objects', 'put', feed.pathname.slice(1), '--bucket-name', bucket, '--file', path.join(staging, 'appcast.xml'), '--content-type', 'application/xml']);
    yield* Effect.logInfo(`Published the Sparkle feed of ${images.join(', ')} at ${feed.href}`);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ manifest, exports = [], ...stated }, context, project) {
        const [fs, path, { dry, unreleased }, target] = yield* Effect.all([FileSystem.FileSystem, Path.Path, released(stated, project), Effect.fromNullishOr(context.targetName)]);
        if (unreleased) {
            return true;
        }
        const { files, outputs } = yield* written(context, target);
        const packaged = yield* Array.match(files, {
            onEmpty: () => ToolError.make({ subject: project, detail: `no file sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first` }),
            onNonEmpty: (found) => Effect.succeed(Array.map(found, (file) => path.relative(context.root, file))),
        });
        const { version } = yield* Effect.flatMap(fs.readFileString(path.join(context.root, manifest)), (text) => versioned(context.root, manifest, text));
        const { tag, uploaded } = yield* attached(context, project, yield* Effect.mapError(Effect.fromNullishOr(version), ToolError.of(manifest, 'states no version')), packaged, dry);
        yield* Effect.forEach(
            exports,
            (exported) =>
                Array.match(
                    uploaded.filter((file) => path.extname(file) === '.dmg' && path.dirname(file) === exported.folder),
                    { onEmpty: () => Effect.void, onNonEmpty: (images) => fed(context, exported, images, tag, dry) },
                ),
            { discard: true },
        );
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
