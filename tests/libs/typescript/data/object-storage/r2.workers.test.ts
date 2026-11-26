import { assert, layer } from '@effect/vitest';
import { r2Storage } from '@rasm/object-storage/r2';
import { ObjectStorage, ObjectStorageError } from '@rasm/object-storage/storage';
import { env } from 'cloudflare:workers';
import { Array, ByteSize, ConfigProvider, Effect, Layer, Option, Record } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const unsigned = { AWS_ENDPOINT_URL_S3: 'http://127.0.0.1', AWS_REGION: 'auto', AWS_ACCESS_KEY_ID: 'unsigned', AWS_SECRET_ACCESS_KEY: 'unsigned' };
const media = { mime: 'video/mp4', options: { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) } } as const;
const code = { ok: 200, partial: 206, unchanged: 304, missing: 404, failed: 412, unsatisfiable: 416 };
const content = Uint8Array.from({ length: 61 }, (_, index) => (index * 37) % 251);

// --- [OPERATIONS] ----------------------------------------------------------------------

const stored = (key: string, bytes: Uint8Array) => Effect.tryPromise({ try: () => env.BUCKET.put(key, bytes, { httpMetadata: { contentType: media.mime } }), catch: ObjectStorageError.of('Unavailable', key) });
const read = Effect.fn('read')(function* (key: string, init: RequestInit = {}) {
    const response = HttpServerResponse.toWeb(yield* ObjectStorage.serve({ key, mime: media.mime }, media.options).pipe(Effect.provideService(HttpServerRequest.HttpServerRequest, HttpServerRequest.fromWeb(new Request(`https://storage.test/${key}`, init)))));
    return { status: response.status, headers: response.headers, body: yield* Effect.tryPromise({ try: () => response.bytes(), catch: ObjectStorageError.of('Unavailable', key) }) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(r2Storage({ bucket: env.BUCKET, name: 'objects' }).pipe(Layer.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(unsigned)))), { excludeTestServices: true })('object storage over an R2 binding', (it) => {
    it.effect(
        'serves ranges, answers preconditions and sets the caller headers',
        Effect.fnUntraced(function* () {
            const [key, size] = ['media/served', content.length];
            const { httpEtag: etag } = yield* Effect.flatMap(stored(key, content), Effect.fromNullishOr);
            const requests = {
                whole: [{}, code.ok, [0, size], null],
                head: [{ method: 'HEAD' }, code.ok, [0, 0], null],
                bounded: [{ headers: { range: 'bytes=1-8' } }, code.partial, [1, 9], `bytes 1-8/${size}`],
                open: [{ headers: { range: 'bytes=50-' } }, code.partial, [50, size], `bytes 50-60/${size}`],
                suffix: [{ headers: { range: 'bytes=-8' } }, code.partial, [53, size], `bytes 53-60/${size}`],
                unsatisfiable: [{ headers: { range: `bytes=${size}-` } }, code.unsatisfiable, [0, 0], `bytes */${size}`],
                unchanged: [{ headers: { 'if-none-match': etag } }, code.unchanged, [0, 0], null],
                mismatched: [{ headers: { 'if-match': '"other"' } }, code.failed, [0, 0], null],
                resumed: [{ headers: { range: 'bytes=0-8', 'if-range': etag } }, code.partial, [0, 9], `bytes 0-8/${size}`],
                restarted: [{ headers: { range: 'bytes=0-8', 'if-range': '"other"' } }, code.ok, [0, size], null],
            } as const;
            const served = yield* Effect.all(
                Record.map(requests, ([init]) => read(key, init)),
                { concurrency: 'unbounded' },
            );
            assert.deepStrictEqual(
                Record.map(requests, (_, name) => [served[name].status, served[name].headers.get('content-range'), Array.fromIterable(served[name].body)]),
                Record.map(requests, ([, expected, [start, end], range]) => [expected, range, Array.fromIterable(content.subarray(start, end))]),
            );
            assert.deepStrictEqual([Array.map(['content-type', 'content-disposition', 'x-content-type-options', 'accept-ranges', 'etag'], (name) => served.whole.headers.get(name)), (yield* read(`${key}/absent`)).status], [[media.mime, media.options.disposition, 'nosniff', 'bytes', etag], code.missing]);
        }),
    );
    it.effect(
        'states the size and type of a stored object and none once it is removed',
        Effect.fnUntraced(function* () {
            const key = 'media/stated';
            yield* stored(key, content.subarray(0, 13));
            const stat = ObjectStorage.use((storage) => storage.stat(key)).pipe(Effect.map(Option.map(({ size, mime }) => ({ size: ByteSize.toNumberUnsafe(size), mime }))));
            const before = yield* stat;
            yield* ObjectStorage.remove(key, Option.none());
            assert.deepStrictEqual([before, yield* stat], [Option.some({ size: 13, mime: media.mime }), Option.none()]);
        }),
    );
});
