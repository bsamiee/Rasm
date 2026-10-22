import { assert, type Vitest } from '@effect/vitest';
import { createUpload, deleteObject, ObjectStorage, serveObject } from '@rasm/hosting/storage.ts';
import { Cause, Deferred, Effect, Function, type Layer, Option, Stream } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import { checkObjectStore, checkResumableWriteStore } from 'partial-content/conformance';
import { isUploadNotFoundError } from 'partial-content/upload';

// --- [OPERATIONS] ----------------------------------------------------------------------

const cases = <E, R>(it: Vitest.MethodsNonLive<R>, storage: Layer.Layer<ObjectStorage, E, R>) => {
    const encoder = new TextEncoder();
    const [content, other] = [encoder.encode('partial content bytes'), encoder.encode('PARTIAL CONTENT BYTES')];
    const object = { key: 'media/object', size: content.length, mime: 'text/plain' };
    const code = { ok: 200, partial: 206, unchanged: 304, missing: 404, failed: 412 };
    const ignore = Function.constant(Effect.void);
    const put = (key: string, body: Stream.Stream<Uint8Array, unknown>, started: (uploadToken: string) => Effect.Effect<unknown>) => Effect.scoped(Effect.flatMap(createUpload({ key, size: object.size }), ({ uploadToken, write }) => Effect.andThen(started(uploadToken), write(body))));
    const read = Effect.fn('read')(function* (key: string, headers: Record<string, string>) {
        const response = HttpServerResponse.toWeb(yield* Effect.provideService(serveObject({ key, mime: object.mime }), HttpServerRequest.HttpServerRequest, HttpServerRequest.fromWeb(new Request(`https://storage.test/${key}`, { headers }))));
        return { status: response.status, headers: response.headers, text: yield* Effect.promise(() => response.text()) };
    });
    it.effect('serves byte ranges and answers If-None-Match and If-Match against the stored object', () =>
        Effect.gen(function* () {
            yield* put(object.key, Stream.make(content), ignore);
            const full = yield* read(object.key, {});
            const etag = yield* Effect.fromNullishOr(full.headers.get('etag'));
            const [ranged, suffix, unchanged, changed, mismatched, matched] = yield* Effect.all([
                read(object.key, { range: 'bytes=2-5' }),
                read(object.key, { range: 'bytes=-5' }),
                read(object.key, { 'if-none-match': etag }),
                read(object.key, { 'if-none-match': '"other"' }),
                read(object.key, { 'if-match': '"other"' }),
                read(object.key, { 'if-match': etag, range: 'bytes=0-6' }),
            ]);
            assert.deepStrictEqual(
                [full, ranged, suffix, unchanged, changed, mismatched, matched].map(({ status, text }) => [status, text]),
                [
                    [code.ok, 'partial content bytes'],
                    [code.partial, 'rtia'],
                    [code.partial, 'bytes'],
                    [code.unchanged, ''],
                    [code.ok, 'partial content bytes'],
                    [code.failed, ''],
                    [code.partial, 'partial'],
                ],
            );
            assert.deepStrictEqual([ranged.headers.get('content-range'), full.headers.get('accept-ranges')], [`bytes 2-5/${object.size}`, 'bytes']);
        }).pipe(Effect.provide(storage)),
    );
    it.effect('aborts an interrupted upload, serves no object, and leaves no upload resource', () =>
        Effect.gen(function* () {
            const [{ uploads }, token] = yield* Effect.all([ObjectStorage, Deferred.make<string>()]);
            const failed = yield* Effect.flip(put('media/interrupted', Stream.concat(Stream.make(content.subarray(1)), Stream.fail(new Cause.UnknownError(undefined))), (uploadToken) => Deferred.succeed(token, uploadToken)));
            const issued = yield* Deferred.await(token);
            const missing = yield* Effect.flip(Effect.tryPromise(() => uploads.getUploadState(issued)));
            assert.deepStrictEqual([failed._tag, isUploadNotFoundError(missing.cause), (yield* read('media/interrupted', {})).status], ['BadRequest', true, code.missing]);
        }).pipe(Effect.provide(storage)),
    );
    it.effect('refuses a body longer than its declared size and publishes nothing', () =>
        Effect.gen(function* () {
            const refused = yield* Effect.flip(put('media/overrun', Stream.make(content, content), ignore));
            assert.deepStrictEqual([refused._tag, (yield* read('media/overrun', {})).status], ['BadRequest', code.missing]);
        }).pipe(Effect.provide(storage)),
    );
    it.effect('publishes one whole object from overlapping uploads to one key', () =>
        Effect.gen(function* () {
            yield* Effect.all([put(object.key, Stream.make(content), ignore), put(object.key, Stream.make(other), ignore)], { concurrency: 'unbounded', discard: true });
            assert.oneOf((yield* read(object.key, {})).text, ['partial content bytes', 'PARTIAL CONTENT BYTES']);
        }).pipe(Effect.provide(storage)),
    );
    it.effect('deletes a stored object and its upload resource', () =>
        Effect.gen(function* () {
            const token = yield* Deferred.make<string>();
            yield* put(object.key, Stream.make(content), (uploadToken) => Deferred.succeed(token, uploadToken));
            yield* deleteObject(object.key, Option.some(yield* Deferred.await(token)));
            assert.strictEqual((yield* read(object.key, {})).status, code.missing);
        }).pipe(Effect.provide(storage)),
    );
    it.effect('meets the partial-content read and resumable write contracts', () =>
        Effect.gen(function* () {
            const { objects, uploads } = yield* ObjectStorage;
            yield* put(object.key, Stream.make(content), ignore);
            const conformance = { key: (n: number) => `conformance/${n}` };
            const reports = yield* Effect.all([Effect.tryPromise(() => checkObjectStore(objects, { key: object.key, content })), Effect.tryPromise(() => checkResumableWriteStore(Function.constant(uploads), conformance))]);
            assert.deepStrictEqual(
                reports.flatMap(({ checks }) => checks).filter(({ ok }) => !ok),
                [],
            );
        }).pipe(Effect.provide(storage)),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases };
