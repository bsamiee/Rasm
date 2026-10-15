import type { D1Client } from '@effect/sql-d1';
import { unavailable } from '@rasm/hosting/http.ts';
import { putObject, releaseObject, serveObject } from '@rasm/hosting/storage.ts';
import { Array, Crypto, Effect, type Layer, Match, Option, Schema, Stream } from 'effect';
import { HttpRouter, type HttpServerRequest, type Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError, type HttpApiGroup } from 'effect/http-api';
import { Api, type Owner } from '../model/api.ts';
import { Asset, Id } from '../model/asset.ts';
import { readAsset } from './database.ts';
import { authorize } from './session.ts';
import { attachUpload, beginUpload, completeUpload, finishRelease, readPendingUploads, releaseUploads } from './storage.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Upload =
    | { readonly stage: 'asset' }
    | {
          readonly stage: 'files' | 'stored';
          readonly asset: typeof Asset.Type;
          readonly attemptId: string;
          readonly files: readonly { readonly key: string; readonly field: 'file' | 'renditions'; readonly name?: string; readonly mime: string; readonly size: number }[];
      };

// --- [OPERATIONS] ----------------------------------------------------------------------

const releaseObjects = Effect.fnUntraced(function* (bucket: R2Bucket, target: Parameters<typeof releaseUploads>[0]) {
    const objects = yield* releaseUploads(target);
    const [, failures] = yield* Effect.partition(objects, ({ key, uploadId }) => Effect.andThen(releaseObject(bucket, key, uploadId), finishRelease(key)), { concurrency: 'unbounded' });
    if (Array.isReadonlyArrayNonEmpty(failures)) {
        yield* Effect.logError(...failures);
        return yield* HttpApiError.ServiceUnavailable.make();
    }
});
const advance = Effect.fnUntraced(function* (bucket: R2Bucket, current: Upload, part: Multipart.Part) {
    return yield* Match.value({ current, part }).pipe(
        Match.when({ current: { stage: 'asset' }, part: { _tag: 'Field', key: 'asset' } }, ({ part: field }) =>
            Effect.gen(function* () {
                const asset = yield* Schema.decodeEffect(Schema.fromJsonString(Asset))(field.value).pipe(Effect.mapError(() => HttpApiError.BadRequest.make()));
                const crypto = yield* Crypto.Crypto;
                const attemptId = yield* crypto.randomUUIDv4();
                const files: Extract<Upload, { stage: 'files' | 'stored' }>['files'] = [
                    ...('renditions' in asset ? (asset.renditions ?? []).map((rendition) => ({ ...rendition, key: `${attemptId}/${rendition.width}`, field: 'renditions' as const, name: String(rendition.width) })) : []),
                    { key: attemptId, field: 'file', mime: asset.mime, size: asset.size },
                ];
                const existing = yield* Effect.acquireRelease(
                    beginUpload(
                        asset,
                        attemptId,
                        files.map(({ key }) => key),
                    ),
                    () => releaseObjects(bucket, { attemptId }).pipe(Effect.catchTag(['ServiceUnavailable', 'Conflict'], Effect.logError)),
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
                yield* state.stage === 'stored' ? Stream.runDrain(file.content) : putObject(bucket, object, file.content, { started: (uploadId) => attachUpload({ key: object.key, uploadId }), onError: () => HttpApiError.BadRequest.make() });
                return { ...state, files: state.files.slice(1) } satisfies Upload;
            }),
        ),
        Match.orElse(() => Effect.fail(HttpApiError.BadRequest.make())),
    );
});
const readMedia = Effect.fn('readMedia')(
    function* (bucket: R2Bucket, email: string, request: HttpServerRequest.HttpServerRequest) {
        const asset = yield* HttpRouter.schemaPathParams(Schema.Struct({ id: Id, width: Schema.OptionFromOptionalKey(Schema.FiniteFromString) })).pipe(Effect.flatMap(readAsset));
        if (!asset.published) {
            yield* authorize(email);
        }
        return yield* serveObject(bucket, request, { key: asset.objectKey, mime: asset.mime });
    },
    Effect.catchTag('SchemaError', () => Effect.fail(HttpApiError.NotFound.make())),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const media = (bucket: R2Bucket): Layer.Layer<HttpApiGroup.ToService<typeof Api.identifier, typeof Api.groups.media>, never, Owner | HttpRouter.Request.From<'Requires', D1Client.D1Client | Crypto.Crypto>> =>
    HttpApiBuilder.group(Api, 'media', (handlers) =>
        handlers.handleAll({
            upload: Effect.fn('upload')(
                function* ({ payload }) {
                    const uploaded = yield* Stream.runFoldEffect(
                        payload,
                        (): Upload => ({ stage: 'asset' }),
                        (current, part) => advance(bucket, current, part),
                    );
                    if (uploaded.stage === 'asset' || uploaded.files.length > 0) {
                        return yield* HttpApiError.BadRequest.make();
                    }
                    if (uploaded.stage === 'files') {
                        yield* completeUpload(uploaded.attemptId);
                    }
                    return { id: uploaded.asset.id };
                },
                Effect.scoped,
                Effect.catchTag('MultipartError', () => Effect.fail(HttpApiError.BadRequest.make())),
                Effect.catchTag(['StorageError', 'PlatformError'], unavailable),
            ),
            pending: () => readPendingUploads(),
            discard: ({ params }) => releaseObjects(bucket, { assetId: params.id, remove: false }),
            delete: ({ params }) => releaseObjects(bucket, { assetId: params.id, remove: true }),
        }),
    );
const download = (bucket: R2Bucket, email: string): Layer.Layer<never, never, HttpRouter.HttpRouter | HttpRouter.Request.From<'Requires', D1Client.D1Client> | HttpRouter.Request.From<'Error', Effect.Error<ReturnType<typeof readMedia>>>> =>
    HttpRouter.addAll((['/api/media/:id', '/api/media/:id/:width'] as const).map((path) => HttpRouter.route('GET', path, (request) => readMedia(bucket, email, request))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
