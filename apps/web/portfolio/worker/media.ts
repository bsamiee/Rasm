import { unavailable } from '@rasm/hosting/http.ts';
import { putObject, releaseObject, type Storage, serveObject } from '@rasm/hosting/storage.ts';
import { Array, Crypto, Effect, type Layer, Match, Option, Schema, Stream } from 'effect';
import { HttpRouter, type HttpServerRequest, type Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError, type HttpApiGroup } from 'effect/http-api';
import type { SqlClient } from 'effect/sql';
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

const releaseObjects = Effect.fnUntraced(function* (target: Parameters<typeof releaseUploads>[0]) {
    const objects = yield* releaseUploads(target);
    const [, failures] = yield* Effect.partition(objects, ({ key, uploadId }) => Effect.andThen(releaseObject(key, uploadId), finishRelease(key)), { concurrency: 'unbounded' });
    if (Array.isReadonlyArrayNonEmpty(failures)) {
        yield* Effect.logError(...failures);
        return yield* HttpApiError.ServiceUnavailable.make();
    }
});
const advance = Match.type<{ readonly current: Upload; readonly part: Multipart.Part }>().pipe(
    Match.when({ current: { stage: 'asset' }, part: { _tag: 'Field', key: 'asset' } }, ({ part: field }) =>
        Effect.gen(function* () {
            const asset = yield* Schema.decodeEffect(Schema.fromJsonString(Asset))(field.value).pipe(Effect.mapError(() => HttpApiError.BadRequest.make()));
            const crypto = yield* Crypto.Crypto;
            const attemptId = yield* crypto.randomUUIDv7();
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
                () => releaseObjects({ attemptId }).pipe(Effect.catchTag(['ServiceUnavailable', 'Conflict'], Effect.logError)),
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
            yield* state.stage === 'stored' ? Stream.runDrain(file.content) : putObject(object, file.content, (uploadId) => attachUpload({ key: object.key, uploadId }));
            return { ...state, files: state.files.slice(1) } satisfies Upload;
        }),
    ),
    Match.orElse(() => Effect.fail(HttpApiError.BadRequest.make())),
);
const readMedia = Effect.fn('readMedia')(
    function* (email: string, request: HttpServerRequest.HttpServerRequest) {
        const asset = yield* HttpRouter.schemaPathParams(Schema.Struct({ id: Id, width: Schema.OptionFromOptionalKey(Schema.FiniteFromString) })).pipe(Effect.flatMap(readAsset));
        if (!asset.published) {
            yield* authorize(email);
        }
        return yield* serveObject(request, { key: asset.objectKey, mime: asset.mime });
    },
    Effect.catchTag('SchemaError', () => Effect.fail(HttpApiError.NotFound.make())),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const media: Layer.Layer<HttpApiGroup.ToService<typeof Api.identifier, typeof Api.groups.media>, never, Owner | HttpRouter.Request.From<'Requires', SqlClient.SqlClient | Crypto.Crypto | Storage>> = HttpApiBuilder.group(Api, 'media', (handlers) =>
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
                    yield* completeUpload(uploaded.attemptId);
                }
                return { id: uploaded.asset.id };
            },
            Effect.scoped,
            Effect.catchTag('MultipartError', () => Effect.fail(HttpApiError.BadRequest.make())),
            Effect.catchTag(['UnknownError', 'PlatformError'], unavailable),
        ),
        pending: () => readPendingUploads(),
        discard: ({ params }) => releaseObjects({ assetId: params.id, remove: false }),
        delete: ({ params }) => releaseObjects({ assetId: params.id, remove: true }),
    }),
);
const download = (email: string): Layer.Layer<never, never, HttpRouter.HttpRouter | HttpRouter.Request.From<'Requires', SqlClient.SqlClient | Storage> | HttpRouter.Request.From<'Error', Effect.Error<ReturnType<typeof readMedia>>>> =>
    HttpRouter.addAll((['/api/media/:id', '/api/media/:id/:width'] as const).map((path) => HttpRouter.route('GET', path, (request) => readMedia(email, request))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
