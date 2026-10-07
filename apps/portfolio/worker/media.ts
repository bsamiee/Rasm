import { env } from 'cloudflare:workers';
import { type Cause, Effect, Function, Match, Predicate, Result, Schema, Sink, Stream } from 'effect';
import { HttpRouter, type HttpServerRequest, HttpServerResponse, HttpStatus, type Multipart } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
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
            const conditions = new Headers(request.headers);
            const object = yield* Effect.tryPromise(() => env.BUCKET.get(id, { range: conditions, onlyIf: conditions })).pipe(Effect.filterOrFail(Predicate.isNotNull, () => new HttpApiError.NotFound()));
            const headers = new Headers([
                ['Accept-Ranges', 'bytes'],
                ['Cache-Control', 'private, no-cache'],
                ['ETag', object.httpEtag],
                ['X-Content-Type-Options', 'nosniff'],
            ]);
            object.writeHttpMetadata(headers);
            return 'body' in object
                ? Match.value(object.range).pipe(
                      Match.when({ offset: Match.number, length: (length: number) => length < object.size }, ({ offset, length }) =>
                          HttpServerResponse.raw(object.body, { status: HttpStatus.fromLiteral('PartialContent'), headers, contentLength: length }).pipe(HttpServerResponse.setHeader('Content-Range', `bytes ${offset}-${offset + length - 1}/${object.size}`)),
                      ),
                      Match.orElse(() => HttpServerResponse.raw(object.body, { headers, contentLength: object.size })),
                  )
                : HttpServerResponse.empty({ status: HttpStatus.fromLiteral('NotModified'), headers });
        },
        Effect.catchTag('SchemaError', () => Effect.fail(new HttpApiError.NotFound())),
        Effect.catchTag('UnknownError', unavailable),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { download, media };
