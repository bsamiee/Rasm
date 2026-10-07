import { env } from 'cloudflare:workers';
import { BrowserCrypto } from '@effect/platform-browser';
import { Array, Crypto, Effect, Exit, flow, Match, Option, Schema, Sink, Stream } from 'effect';
import { HttpRouter, HttpServerRequest, HttpServerResponse, type Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { r2Store } from 'partial-content/r2';
import { serveObjectRaw } from 'partial-content/web';
import { Api } from '../model/api.ts';
import { Asset, Id } from '../model/asset.ts';
import { readAsset, unavailable } from './database.ts';
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
    const [, failures] = yield* Effect.partition(
        objects,
        Effect.fnUntraced(function* ({ key, uploadId }) {
            if (uploadId !== null) {
                yield* Effect.tryPromise(() => env.BUCKET.resumeMultipartUpload(key, uploadId).abort());
            }
            yield* Effect.tryPromise(() => env.BUCKET.delete(key));
            yield* finishRelease(key);
        }),
        { concurrency: 'unbounded' },
    );
    if (Array.isReadonlyArrayNonEmpty(failures)) {
        yield* Effect.logError(...failures);
        return yield* new HttpApiError.ServiceUnavailable();
    }
});
const putObject = Effect.fnUntraced(function* (object: { readonly key: string; readonly mime: string; readonly size: number }, content: Stream.Stream<Uint8Array, Multipart.MultipartError>) {
    const upload = yield* Effect.acquireRelease(
        Effect.tryPromise(() => env.BUCKET.createMultipartUpload(object.key, { httpMetadata: { contentType: object.mime } })),
        (handle, exit) => (Exit.isSuccess(exit) ? Effect.void : Effect.tryPromise(() => handle.abort()).pipe(Effect.catchTag('UnknownError', Effect.logError))),
    );
    yield* attachUpload(object.key, upload.uploadId);
    const body = new FixedLengthStream(object.size);
    const [, part] = yield* Effect.all([Stream.run(content, Sink.fromWritableStream({ evaluate: () => body.writable, onError: () => new HttpApiError.BadRequest() })), Effect.tryPromise(() => upload.uploadPart(1, body.readable))], { concurrency: 'unbounded' });
    yield* Effect.tryPromise(() => upload.complete([part]));
});
const advance = Effect.fnUntraced(function* (current: Upload, part: Multipart.Part) {
    return yield* Match.value({ current, part }).pipe(
        Match.when({ current: { stage: 'asset' }, part: { _tag: 'Field', key: 'asset' } }, ({ part: field }) =>
            Effect.gen(function* () {
                const asset = yield* Schema.decodeUnknownEffect(Schema.fromJsonString(Asset))(field.value).pipe(Effect.mapError(() => new HttpApiError.BadRequest()));
                const crypto = yield* Crypto.Crypto;
                const attemptId = yield* crypto.randomUUIDv4;
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
                const object = yield* Option.match(Array.head(state.files), { onNone: () => Effect.fail(new HttpApiError.BadRequest()), onSome: Effect.succeed });
                if (file.key !== object.field || (object.name !== undefined && file.name !== object.name)) {
                    return yield* new HttpApiError.BadRequest();
                }
                yield* state.stage === 'stored' ? Stream.runDrain(file.content) : putObject(object, file.content);
                return { ...state, files: state.files.slice(1) } satisfies Upload;
            }),
        ),
        Match.orElse(() => Effect.fail(new HttpApiError.BadRequest())),
    );
});
const readMedia = Effect.fn('readMedia')(
    function* (request: HttpServerRequest.HttpServerRequest) {
        const { id, width } = yield* HttpRouter.schemaPathParams(Schema.Struct({ id: Id, width: Schema.OptionFromOptionalKey(Schema.NumberFromString) }));
        const asset = yield* readAsset(id, width);
        if (!asset.published) {
            yield* authorize;
        }
        const requested = yield* HttpServerRequest.toWeb(request);
        const run = Effect.runSyncWith(yield* Effect.context());
        const response = yield* Effect.promise(() => serveObjectRaw(r2Store({ bucket: env.BUCKET, reservedPrefix: '' }), { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }), onError: flow(Effect.logError, run) })(requested, { key: asset.objectKey, mime: asset.mime }));
        return HttpServerResponse.raw(response.body instanceof ReadableStream ? response.body.pipeThrough(new FixedLengthStream(Number(response.headers['Content-Length']))) : response.body, response);
    },
    Effect.catchTag('SchemaError', () => Effect.fail(new HttpApiError.NotFound())),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const media = HttpApiBuilder.group(Api, 'media', (handlers) =>
    handlers.handleAll({
        upload: Effect.fn('upload')(
            function* ({ payload }) {
                const uploaded = yield* Stream.runFoldEffect(payload, (): Upload => ({ stage: 'asset' }), advance);
                if (uploaded.stage === 'asset' || uploaded.files.length > 0) {
                    return yield* new HttpApiError.BadRequest();
                }
                if (uploaded.stage === 'files') {
                    yield* completeUpload(uploaded.attemptId);
                }
                return { id: uploaded.asset.id };
            },
            Effect.provide(BrowserCrypto.layer),
            Effect.scoped,
            Effect.catchTag('MultipartError', () => Effect.fail(new HttpApiError.BadRequest())),
            Effect.catchTag(['UnknownError', 'PlatformError'], unavailable),
        ),
        pending: () => readPendingUploads(),
        discard: ({ params }) => releaseObjects({ assetId: params.id, remove: false }),
        delete: ({ params }) => releaseObjects({ assetId: params.id, remove: true }),
    }),
);
const download = HttpRouter.addAll((['/api/media/:id', '/api/media/:id/:width'] as const).map((path) => HttpRouter.route('GET', path, readMedia)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
