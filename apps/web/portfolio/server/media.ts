import { createUpload, deleteObject, serveObject, unavailable } from '@rasm/hosting';
import { Array, Cause, Effect, Match, Option, Schema, Stream } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter, type Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { Api } from '#/model/api';
import { type Asset, AssetJson, Id, mediaPath } from '#/model/asset';
import { authorize } from '#/server/session';
import { attachUpload, beginUpload, completeUpload, finishRelease, Release, readAsset, readPendingUploads, releaseUploads } from '#/server/store/assets';

// --- [TYPES] ---------------------------------------------------------------------------

type Upload =
    | { readonly stage: 'asset' }
    | {
          readonly stage: 'files' | 'stored';
          readonly asset: typeof Asset.Type;
          readonly attemptId: string;
          readonly files: readonly { readonly key: string; readonly field: 'file' | 'renditions'; readonly name?: string; readonly size: number }[];
      };

// --- [MODELS] --------------------------------------------------------------------------

const MediaParams = Schema.Struct({ id: Id, width: Schema.OptionFromOptionalKey(Schema.FiniteFromString) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const releaseObjects = Effect.fnUntraced(function* (release: Release) {
    const objects = yield* releaseUploads(release);
    const [released, failures] = yield* Effect.partition(objects, ({ key, uploadId }) => Effect.as(deleteObject(key, uploadId), key), { concurrency: 'unbounded' });
    yield* Array.match(released, { onEmpty: () => Effect.void, onNonEmpty: (keys) => finishRelease(release.assetId, keys) });
    if (Array.isReadonlyArrayNonEmpty(failures)) {
        yield* Effect.logError('object delete failed', ...failures.map(Cause.fail));
        return yield* HttpApiError.ServiceUnavailable.make();
    }
});
const advance = Match.type<{ readonly current: Upload; readonly part: Multipart.Part }>().pipe(
    Match.when({ current: { stage: 'asset' }, part: { _tag: 'Field', key: 'asset' } }, ({ part: field }) =>
        Effect.gen(function* () {
            const asset = yield* Schema.decodeEffect(AssetJson)(field.value).pipe(Effect.mapError(() => HttpApiError.BadRequest.make()));
            const attemptId = yield* SecureRandom.SecureRandom.use((random) => random.uuidV7());
            const files: Extract<Upload, { stage: 'files' | 'stored' }>['files'] = [...('renditions' in asset ? (asset.renditions ?? []).map(({ width, size }) => ({ key: `${attemptId}/${width}`, field: 'renditions' as const, name: String(width), size })) : []), { key: attemptId, field: 'file', size: asset.size }];
            const existing = yield* Effect.acquireRelease(
                beginUpload(
                    asset,
                    attemptId,
                    files.map(({ key }) => key),
                ),
                () => releaseObjects(Release.Abandon({ assetId: asset.id, attemptId })).pipe(Effect.ignore({ log: 'Error', message: 'abandoned upload release failed' })),
            );
            return { stage: Option.isSome(existing) ? 'stored' : 'files', asset, attemptId, files } satisfies Upload;
        }),
    ),
    Match.when({ current: { stage: Match.is('files', 'stored') }, part: { _tag: 'File' } }, ({ current: state, part: file }) =>
        Effect.gen(function* () {
            const object = yield* Effect.fromOption(Array.head(state.files), () => HttpApiError.BadRequest.make());
            if (file.key !== object.field || (object.name !== undefined && file.name !== object.name)) {
                return yield* HttpApiError.BadRequest.make();
            }
            yield* state.stage === 'stored'
                ? Stream.runDrain(file.content)
                : createUpload(object).pipe(
                      Effect.tap(({ uploadToken }) => attachUpload(state.asset.id, object.key, uploadToken)),
                      Effect.flatMap(({ write }) => write(file.content)),
                  );
            return { ...state, files: state.files.slice(1) } satisfies Upload;
        }),
    ),
    Match.orElse(() => Effect.fail(HttpApiError.BadRequest.make())),
);
const readMedia = Effect.fn('readMedia')(
    function* (email: string) {
        const asset = yield* HttpRouter.schemaPathParams(MediaParams).pipe(Effect.flatMap(readAsset));
        if (!asset.published) {
            yield* authorize(email);
        }
        return yield* serveObject({ key: asset.objectKey, mime: asset.mime });
    },
    Effect.catchTag('SchemaError', () => Effect.fail(HttpApiError.NotFound.make())),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const media = HttpApiBuilder.group(Api, 'media', (handlers) =>
    handlers.handleAll({
        upload: Effect.fn('upload')(
            function* ({ payload }) {
                const uploaded = yield* Stream.runFoldEffect(
                    payload,
                    (): Upload => ({ stage: 'asset' }),
                    (current, part) => advance({ current, part }),
                );
                if (uploaded.stage === 'asset' || uploaded.files.length > 0) {
                    return yield* HttpApiError.BadRequest.make();
                }
                if (uploaded.stage === 'files') {
                    yield* completeUpload(uploaded.asset.id, uploaded.attemptId);
                }
                return { id: uploaded.asset.id };
            },
            Effect.scoped,
            Effect.catchTags({ MultipartError: () => Effect.fail(HttpApiError.BadRequest.make()), UnknownError: unavailable }),
        ),
        pending: () => readPendingUploads,
        discard: ({ params }) => releaseObjects(Release.Discard({ assetId: params.id })),
        delete: ({ params }) => releaseObjects(Release.Delete({ assetId: params.id })),
    }),
);
const download = (email: string) => HttpRouter.add('GET', mediaPath, readMedia(email));

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
