import { unavailable } from '@rasm/hosting/response';
import { ObjectStorage } from '@rasm/hosting/storage';
import { Array, Cause, Effect, Option, Schema, Sink, Stream, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter, Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { Api } from '#/model/api';
import { type Asset, AssetJson, Id, ImageAsset, mediaPath } from '#/model/asset';
import { authorize } from '#/server/session';
import { attachUpload, beginUpload, completeUpload, finishRelease, Release, readAsset, readPendingUploads, releaseUploads } from '#/server/store/assets';

// --- [MODELS] --------------------------------------------------------------------------

const MediaParams = Schema.Struct({ id: Id, width: Schema.OptionFromOptionalKey(Schema.FiniteFromString.check(Schema.isInt())) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const objects = (asset: typeof Asset.Type, attempt: string) => [
    ...(Schema.is(ImageAsset)(asset) ? (asset.renditions ?? []) : []).map(({ width, size, mime }) => ({ key: `${attempt}/${width}`, size, mime, field: 'renditions', name: Option.some(String(width)) })),
    { key: attempt, size: asset.size, mime: asset.mime, field: 'file', name: Option.none<string>() },
];
const receive = Effect.fnUntraced(function* (assetId: string, stored: boolean, expected: Option.Option<ReturnType<typeof objects>[number]>, part: Multipart.Part) {
    const [object, file] = yield* Effect.fromOption(
        Option.filter(Option.product(expected, Option.liftPredicate(part, Multipart.isFile)), ([{ field, mime, name }, sent]) => sent.key === field && sent.contentType === mime && Option.match(name, { onNone: () => true, onSome: (width) => width === sent.name })),
        () => HttpApiError.BadRequest.make(),
    );
    yield* stored
        ? Stream.runDrain(file.content)
        : ObjectStorage.upload(object).pipe(
              Effect.tap(({ uploadToken }) => attachUpload(assetId, object.key, uploadToken)),
              Effect.flatMap(({ write }) => write(file.content)),
          );
});
const releaseObjects = Effect.fnUntraced(function* (release: Release) {
    const [released, failures] = yield* Effect.partition(yield* releaseUploads(release), ({ key, uploadId }) => Effect.as(ObjectStorage.remove(key, uploadId), key), { concurrency: 'unbounded' });
    yield* Array.match(released, { onEmpty: () => Effect.void, onNonEmpty: (keys) => finishRelease(release.assetId, keys) });
    if (Array.isReadonlyArrayNonEmpty(failures)) {
        yield* Effect.logError('object delete failed', ...failures.map(Cause.fail));
        return yield* HttpApiError.ServiceUnavailable.make();
    }
});
const readMedia = Effect.fn('readMedia')(
    function* (email: string) {
        const { objectKey, mime, name, published } = yield* HttpRouter.schemaPathParams(MediaParams).pipe(Effect.flatMap(readAsset));
        if (!published) {
            yield* authorize(email);
        }
        return yield* ObjectStorage.serve({ key: objectKey, mime, ...Option.match(name, { onNone: () => ({}), onSome: (filename) => ({ filename }) }) }, { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) });
    },
    Effect.catchTag('SchemaError', () => Effect.fail(HttpApiError.NotFound.make())),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const media = HttpApiBuilder.group(Api, 'media', (handlers) =>
    handlers.handleAll({
        upload: Effect.fn('upload')(
            function* ({ payload }) {
                const [head, parts] = yield* Stream.peel(payload, Sink.head());
                const asset = yield* head.pipe(
                    Option.filter(Multipart.isField),
                    Option.filter(({ key }) => key === 'asset'),
                    Effect.fromOption,
                    Effect.flatMap(({ value }) => Schema.decodeEffect(AssetJson)(value)),
                );
                const attempt = yield* SecureRandom.SecureRandom.use((random) => random.uuidV7());
                const expected = objects(asset, attempt);
                const stored = yield* Effect.acquireRelease(beginUpload(asset, attempt, Array.map(expected, Struct.get('key'))), () => releaseObjects(Release.Abandon({ assetId: asset.id, attemptId: attempt })).pipe(Effect.ignore({ log: 'Error', message: 'abandoned upload release failed' })));
                yield* parts.pipe(
                    Stream.mapEffect((part, index) => receive(asset.id, Option.isSome(stored), Array.get(expected, index), part)),
                    Stream.runCount,
                    Effect.filterOrFail(
                        (received) => received === expected.length,
                        () => HttpApiError.BadRequest.make(),
                    ),
                );
                yield* Option.match(stored, { onNone: () => completeUpload(asset.id, attempt), onSome: () => Effect.void });
                return { id: asset.id };
            },
            Effect.scoped,
            Effect.catchTag(['MultipartError', 'NoSuchElementError', 'SchemaError'], () => Effect.fail(HttpApiError.BadRequest.make())),
            Effect.catchTag('UnknownError', unavailable),
        ),
        pending: () => readPendingUploads,
        discard: ({ params }) => releaseObjects(Release.Discard({ assetId: params.id })),
        delete: ({ params }) => releaseObjects(Release.Delete({ assetId: params.id })),
    }),
);
const download = (email: string) => HttpRouter.add('GET', mediaPath, readMedia(email));

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
