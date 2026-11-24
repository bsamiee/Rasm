import { fileURLToPath } from 'node:url';
import { packaged, released } from '@rasm/nx-distribution/executor';
import { dried, executor, spawned, Words } from '@rasm/nx-distribution/inference';
import cf from 'cf/package.json' with { type: 'json' };
import { Array, Cause, Config, Effect, FileSystem, Function, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { APPCAST, NOTARY } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    exports: Schema.Array(
        Schema.Struct({
            archive: Schema.String.annotate({ description: 'The .xcarchive whose app states its Sparkle feed in SUFeedURL, beside the disk image pack--ditto writes of that app' }),
        }),
    ).annotate({ description: 'Archived apps whose disk image gets a Sparkle feed' }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    __unparsed__: Words,
}).annotate({
    title: 'Release publish',
    description: `Signs a Sparkle feed for the disk images beside each archive among the files the target's dependentTasksOutputFiles input matches in the outputs of the tasks it depends on with generate_appcast under the key ${APPCAST.key} holds, pointing at the image's asset on the GitHub release of the project's tag at the checked-out commit, the tag nx release made from the release tag pattern of the project's release group, and uploads it to the R2 bucket ${APPCAST.bucket} names at the path of the app's SUFeedURL with cf r2 objects put, fails naming those outputs where they hold no matching file, and where an archive has no disk image beside it, either variable is unset, or nx release released no version of the project at that commit names what is missing and publishes nothing; runs through nx release publish, a dry run previewing every project`,
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));
const Assets = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.URLFromString })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const fed = Effect.fnUntraced(function* (root: string, archive: string, files: readonly string[], tag: string, dry: boolean, words: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const images = files.filter((file) => path.dirname(file) === path.dirname(archive));
    if (!Array.isReadonlyArrayNonEmpty(images)) {
        return yield* Effect.logWarning(`Skipped the Sparkle feed of ${archive}: no disk image sits beside it, which pack--ditto writes where ${NOTARY.key}, ${NOTARY.keyId}, and ${NOTARY.issuer} are set`);
    }
    const names = Array.map(images, (image) => path.basename(image));
    const held = yield* Effect.all(Record.map(APPCAST, (variable) => Effect.map(Config.option(Config.String(variable)), Result.fromOption(Function.constant(variable)))));
    const signed = Effect.fnUntraced(function* ({ key, bucket }: { readonly key: string; readonly bucket: string }) {
        if (dry) {
            return yield* Effect.logInfo(`Would publish the Sparkle feed of ${names.join(', ')} for ${tag} through generate_appcast and cf r2 objects put`);
        }
        const { ApplicationPath } = yield* Effect.flatMap(spawned(['plutil', '-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: root }), Schema.decodeEffect(Application));
        const feed = yield* Effect.flatMap(spawned(['plutil', '-extract', 'SUFeedURL', 'raw', '-o', '-', path.join(archive, 'Products', ApplicationPath, 'Contents', 'Info.plist')], { cwd: root }), (stated) => Schema.decodeEffect(Schema.URLFromString)(stated.trim()));
        const { assets } = yield* Effect.flatMap(spawned(['gh', 'release', 'view', tag, '--json', 'assets'], { cwd: root }), Schema.decodeEffect(Assets));
        const asset = yield* Effect.mapError(Effect.fromOption(Array.findFirst(assets, ({ name }) => name === Array.headNonEmpty(names))), (cause) => new Cause.UnknownError(cause, `${tag} holds no ${Array.headNonEmpty(names)}`));
        const staging = yield* fs.makeTempDirectoryScoped();
        yield* Effect.forEach(images, (image) => fs.copyFile(path.join(root, image), path.join(staging, path.basename(image))), { discard: true });
        yield* spawned(['generate_appcast', '--ed-key-file', '-', '--download-url-prefix', new URL('.', asset.url).href, '-o', path.join(staging, 'appcast.xml'), ...words, staging], { cwd: root, stdin: Stream.encodeText(Stream.make(key)) });
        yield* spawned([fileURLToPath(new URL(cf.bin.cf, import.meta.resolve('cf/package.json'))), 'r2', 'objects', 'put', feed.pathname.slice(1), '--bucket-name', bucket, '--file', path.join(staging, 'appcast.xml'), '--content-type', 'application/xml'], { cwd: root });
        return yield* Effect.logInfo(`Published the Sparkle feed of ${names.join(', ')} at ${feed.href}`);
    });
    return yield* Result.match(Result.all(held), { onFailure: () => Effect.logWarning(`Skipped the Sparkle feed of ${names.join(', ')}: ${Array.getFailures(Object.values(held)).join(' and ')} not set`), onSuccess: signed });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Publish, function* ({ exports, dryRun, __unparsed__: words = [] }, context) {
    const [dry, tag, { files }] = yield* Effect.all([dried(dryRun), released(context), packaged(context)]);
    yield* Option.match(dry ? Option.some(Option.getOrElse(tag, () => `<tag nx release makes for ${context.projectName}>`)) : tag, {
        onNone: () => Effect.logInfo(`Skipped the Sparkle feeds of ${context.projectName}: HEAD holds no tag of ${context.projectName}, so nx release released no version of it at this commit`),
        onSome: (at) => Effect.forEach(exports, ({ archive }) => fed(context.root, archive, files, at, dry, words), { discard: true }),
    });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
