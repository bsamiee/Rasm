import { fileURLToPath } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import { spawned } from '@rasm/nx-distribution/inference';
import cf from 'cf/package.json' with { type: 'json' };
import { Array, Cause, Config, Effect, FileSystem, Function, Option, Path, Record, Result, Schema, SemVer, Stream } from 'effect';
import { APPCAST, NOTARY, run } from '#/targets.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Schema.Struct({
    exports: Schema.Array(
        Schema.Struct({
            archive: Schema.String.annotate({ description: 'The .xcarchive whose app states its Sparkle feed in SUFeedURL' }),
            folder: Schema.String.annotate({ description: 'Folder holding the disk image of that app' }),
        }),
    ).annotate({ description: 'Archived apps whose disk image gets a Sparkle feed' }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
}).annotate({
    title: 'Release publish',
    description: `Signs a Sparkle feed for each disk image an export folder holds with generate_appcast under the key ${APPCAST.key} holds, pointing at the image's asset on the GitHub release of the project's tag at the checked-out commit, the tag nx release made from release.releaseTag.pattern, and uploads it to the R2 bucket ${APPCAST.bucket} names at the path of the app's SUFeedURL with cf r2 objects put, and where a folder holds no disk image, either variable is unset, or nx release released no version of the project at that commit names what is missing and publishes nothing; runs through nx release publish, a dry run previewing every project`,
});
const Application = Schema.fromJsonString(Schema.Struct({ ApplicationPath: Schema.String }));
const Assets = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.URLFromString })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const released = Effect.fnUntraced(function* ({ root, projectName, nxJsonConfiguration: { release } }: ExecutorContext) {
    const project = yield* Effect.fromNullishOr(projectName);
    const independent = release?.projectsRelationship === 'independent';
    const pattern = release?.releaseTag?.pattern ?? (independent ? '{projectName}@{version}' : 'v{version}');
    const named = project
        .replaceAll(':', '/')
        .replaceAll(/[\p{Cc} ~^?*[\\]/gu, '-')
        .replaceAll(/\/{2,}/gu, '/')
        .replaceAll(/\.{2,}/gu, '.')
        .replaceAll(/^\/+|\/+$/gu, '');
    const matcher = new RegExp(`^${(independent ? pattern.replaceAll('{projectName}', named) : pattern).split('{version}').map(RegExp.escape).join('(.+)')}$`, 'u');
    const tags = (yield* spawned(['git', 'tag', '--points-at', 'HEAD'], { cwd: root })).split('\n').filter((tag) => Option.exists(Option.fromNullishOr(matcher.exec(tag)?.[1]), Function.flow(SemVer.parse, Result.isSuccess)));
    return tags.length > 1 ? yield* new Cause.UnknownError(tags, `HEAD holds [${tags.join(', ')}], each matching ${pattern} for ${project}, in place of the one tag nx release makes per project`) : Array.head(tags);
});

const fed = Effect.fnUntraced(function* (context: ExecutorContext, { archive, folder }: (typeof Publish.Type)['exports'][number], tag: string, dry: boolean, words: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const images = (yield* fs.readDirectory(path.join(context.root, folder))).filter((name) => path.extname(name) === '.dmg');
    if (!Array.isReadonlyArrayNonEmpty(images)) {
        return yield* Effect.logWarning(`Skipped the Sparkle feed of ${folder}: it holds no disk image, which pack--ditto writes where ${NOTARY.key}, ${NOTARY.keyId}, and ${NOTARY.issuer} are set`);
    }
    const held = yield* Effect.all(Record.map(APPCAST, (variable) => Effect.map(Config.option(Config.String(variable)), Result.fromOption(Function.constant(variable)))));
    const signed = Effect.fnUntraced(function* ({ key, bucket }: { readonly key: string; readonly bucket: string }) {
        if (dry) {
            return yield* Effect.logInfo(`Would publish the Sparkle feed of ${images.join(', ')} for ${tag} through generate_appcast and cf r2 objects put`);
        }
        const { ApplicationPath } = yield* Effect.flatMap(spawned(['plutil', '-extract', 'ApplicationProperties', 'json', '-o', '-', path.join(archive, 'Info.plist')], { cwd: context.root }), Schema.decodeEffect(Application));
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

const execute = Effect.fnUntraced(function* ({ exports, dryRun = false, __unparsed__: words = [] }: typeof Publish.Type, context: ExecutorContext) {
    const [stated, tag] = yield* Effect.all([Config.withDefault(Config.Boolean('NX_DRY_RUN'), false), released(context)]);
    const dry = dryRun || stated;
    yield* Option.match(dry ? Option.some(Option.getOrElse(tag, () => `<tag nx release makes for ${context.projectName}>`)) : tag, {
        onNone: () => Effect.logInfo(`Skipped the Sparkle feeds of ${context.projectName}: HEAD holds no tag of ${context.projectName}, so nx release released no version of it at this commit`),
        onSome: (at) => Effect.forEach(exports, (exported) => fed(context, exported, at, dry, words), { discard: true }),
    });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
