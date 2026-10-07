import { env } from 'cloudflare:workers';
import { type Cause, Effect, Function, Match, Predicate, Result, Schema, Sink, Stream } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse, HttpStatus, type Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { evaluateConditionalRequest } from 'partial-content';
import { Api } from '../model/api.ts';
import { Asset, Id } from '../model/asset.ts';
import { deleteAsset, isPublished, storeAsset, unavailable } from './database.ts';
import { authorize } from './session.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Upload = { readonly stage: 'asset' } | { readonly stage: 'file' | 'complete'; readonly asset: typeof Asset.Type };

// --- [OPERATIONS] ----------------------------------------------------------------------

const putObject = Effect.fnUntraced(function* (asset: typeof Asset.Type, content: Stream.Stream<Uint8Array, Multipart.MultipartError>) {
    const existing = yield* Effect.tryPromise(() => env.BUCKET.head(asset.id));
    return yield* Match.value(existing).pipe(
        Match.when(Match.null, () => {
            const body = new FixedLengthStream(asset.size);
            return Effect.all(
                [
                    Stream.run(content, Sink.fromWritableStream({ evaluate: () => body.writable, onError: () => new HttpApiError.BadRequest() })),
                    Effect.tryPromise(() => env.BUCKET.put(asset.id, body.readable, { onlyIf: new Headers({ 'If-None-Match': '*' }), httpMetadata: { contentType: asset.mime } })).pipe(Effect.filterOrFail(Predicate.isNotNull, () => new HttpApiError.Conflict())),
                ],
                { concurrency: 'unbounded', mode: 'result' },
            ).pipe(Effect.flatMap(([written, stored]) => Effect.fromResult(Result.isFailure(stored) && stored.failure._tag === 'Conflict' ? stored : Result.andThen(written, stored))));
        }),
        Match.when({ size: asset.size, httpMetadata: { contentType: asset.mime } }, () => Stream.runDrain(content)),
        Match.orElse(() => Effect.fail(new HttpApiError.Conflict())),
    );
});
const advance = (current: Upload, incoming: Multipart.Part): Effect.Effect<Upload, HttpApiError.BadRequest | HttpApiError.Conflict | Cause.UnknownError | Multipart.MultipartError> =>
    Match.value({ state: current, part: incoming }).pipe(
        Match.when({ state: { stage: 'asset' }, part: { _tag: 'Field', key: 'asset' } }, ({ part }) =>
            Schema.decodeUnknownEffect(Schema.fromJsonString(Asset))(part.value).pipe(
                Effect.mapError(() => new HttpApiError.BadRequest()),
                Effect.map((asset): Upload => ({ stage: 'file', asset })),
            ),
        ),
        Match.when({ state: { stage: 'file' }, part: { _tag: 'File' } }, ({ state, part }) => Effect.as(putObject(state.asset, part.content), { stage: 'complete', asset: state.asset } satisfies Upload)),
        Match.orElse(() => Effect.fail(new HttpApiError.BadRequest())),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const media = HttpApiBuilder.group(Api, 'media', (handlers) =>
    handlers.handleAll({
        upload: Effect.fn('upload')(
            function* ({ payload }) {
                const uploaded = yield* Stream.runFoldEffect(payload, (): Upload => ({ stage: 'asset' }), advance);
                return yield* Match.value(uploaded).pipe(
                    Match.when({ stage: 'complete' }, ({ asset }) => Effect.as(storeAsset(asset), { id: asset.id })),
                    Match.orElse(() => Effect.fail(new HttpApiError.BadRequest())),
                );
            },
            Effect.catchTag('MultipartError', () => Effect.fail(new HttpApiError.BadRequest())),
            Effect.catchTag('UnknownError', unavailable),
        ),
        delete: Effect.fn('delete')(
            function* ({ params }) {
                yield* deleteAsset(params.id);
                yield* Effect.tryPromise(() => env.BUCKET.delete(params.id));
            },
            Effect.catchTag('UnknownError', unavailable),
        ),
    }),
);
const download = HttpRouter.add(
    'GET',
    '/api/media/:id',
    Effect.fn('download')(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const { id } = yield* HttpRouter.schemaPathParams(Schema.Struct({ id: Id }));
            yield* Effect.filterOrElse(isPublished(id), Function.identity, () => authorize);
            const requested = new Headers(request.headers);
            const conditional = ['Range', 'If-Match', 'If-None-Match', 'If-Modified-Since', 'If-Unmodified-Since'].some((header) => requested.has(header));
            const metadata = yield* Effect.tryPromise((): Promise<R2Object | R2ObjectBody | null> => (conditional ? env.BUCKET.head(id) : env.BUCKET.get(id))).pipe(Effect.filterOrFail(Predicate.isNotNull, () => new HttpApiError.NotFound()));
            const response = evaluateConditionalRequest(requested, { totalSize: metadata.size, etag: metadata.httpEtag, lastModified: metadata.uploaded.toUTCString() });
            const headers = new Headers(response.headers);
            metadata.writeHttpMetadata(headers);
            headers.set('Cache-Control', 'private, no-cache');
            headers.set('X-Content-Type-Options', 'nosniff');
            if (response.status !== HttpStatus.fromLiteral('Ok') && response.status !== HttpStatus.fromLiteral('PartialContent')) {
                return HttpServerResponse.empty({ status: response.status, headers });
            }
            const object =
                'body' in metadata ? metadata : yield* Effect.tryPromise(() => env.BUCKET.get(id, response.range === null ? {} : { range: { offset: response.range.start, length: response.range.end - response.range.start + 1 } })).pipe(Effect.filterOrFail(Predicate.isNotNull, () => new HttpApiError.NotFound()));
            return HttpServerResponse.raw(object.body, { status: response.status, headers });
        },
        Effect.catchTag('SchemaError', () => Effect.fail(new HttpApiError.NotFound())),
        Effect.catchTag('UnknownError', unavailable),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
