import { assert, layer } from '@effect/vitest';
import { ObjectStorage, ObjectStorageError } from '@rasm/object-storage';
import { r2Storage } from '@rasm/object-storage/r2';
import { env } from 'cloudflare:workers';
import { Array, ByteSize, Bytes, ConfigProvider, Duration, Effect, Layer, Option, Record } from 'effect';
import { FetchHttpClient, HttpServerRequest, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const unsigned = { AWS_ENDPOINT_URL_S3: 'http://127.0.0.1', AWS_REGION: 'auto', AWS_ACCESS_KEY_ID: 'unsigned', AWS_SECRET_ACCESS_KEY: 'unsigned' };
const part = ByteSize.toNumberUnsafe(ByteSize.fromStringUnsafe('5 MiB'));
const media = { mime: 'video/mp4', options: { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) } } as const;
const code = { ok: 200, partial: 206, unchanged: 304, missing: 404, failed: 412, unsatisfiable: 416 };

// --- [OPERATIONS] ----------------------------------------------------------------------

const content = Uint8Array.from({ length: 2 * part + 7 }, (_, index) => index * Math.SQRT2);
const stored = (key: string, bytes: Uint8Array) => Effect.tryPromise({ try: () => env.BUCKET.put(key, bytes, { httpMetadata: { contentType: media.mime } }), catch: ObjectStorageError.of('Unavailable', key) });
const read = Effect.fn('read')(function* (key: string, init: RequestInit = {}) {
    const response = HttpServerResponse.toWeb(yield* ObjectStorage.serve({ key, mime: media.mime }, media.options).pipe(Effect.provideService(HttpServerRequest.HttpServerRequest, HttpServerRequest.fromWeb(new Request(`https://storage.test/${key}`, init)))));
    return { status: response.status, headers: response.headers, body: yield* Effect.tryPromise({ try: () => response.bytes(), catch: ObjectStorageError.of('Unavailable', key) }) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(r2Storage({ bucket: env.BUCKET, name: 'objects' }).pipe(Layer.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(unsigned))), Layer.merge(FetchHttpClient.layer)), { excludeTestServices: true })('object storage over an R2 binding', (it) => {
    it.effect(
        'serves ranges, answers preconditions and sets the caller headers',
        Effect.fnUntraced(function* () {
            const [key, size, span] = ['media/served', content.length, 8];
            yield* stored(key, content);
            const etag = yield* Effect.fromNullishOr((yield* read(key, { method: 'HEAD' })).headers.get('etag'));
            const ranged = (first: number, last: number, headers: Record.ReadonlyRecord<string, string> = {}) => [{ headers: { ...headers, range: `bytes=${first}-${last}` } }, code.partial, [first, last + 1], `bytes ${first}-${last}/${size}`] as const;
            const requests = {
                whole: [{}, code.ok, [0, size], null],
                head: [{ method: 'HEAD' }, code.ok, [0, 0], null],
                bounded: ranged(1, span),
                straddling: ranged(part - span, part + span - 1),
                suffix: [{ headers: { range: `bytes=-${span}` } }, code.partial, [size - span, size], `bytes ${size - span}-${size - 1}/${size}`],
                unsatisfiable: [{ headers: { range: `bytes=${size}-` } }, code.unsatisfiable, [0, 0], `bytes */${size}`],
                unchanged: [{ headers: { 'if-none-match': etag } }, code.unchanged, [0, 0], null],
                mismatched: [{ headers: { 'if-match': '"other"' } }, code.failed, [0, 0], null],
                resumed: ranged(0, span, { 'if-range': etag }),
            } as const;
            const served = yield* Effect.all(
                Record.map(requests, ([init]) => read(key, init)),
                { concurrency: 'unbounded' },
            );
            assert.deepStrictEqual(
                Record.map(requests, ({ 2: [start, end] }, name) => [served[name].status, served[name].headers.get('content-range'), Bytes.Equivalence(served[name].body, content.subarray(start, end))]),
                Record.map(requests, ([, expected, , range]) => [expected, range, true]),
            );
            assert.deepStrictEqual([Array.map(['content-type', 'content-disposition', 'x-content-type-options', 'accept-ranges', 'etag'], (name) => served.whole.headers.get(name)), (yield* read(`${key}/absent`)).status], [[media.mime, media.options.disposition, 'nosniff', 'bytes', etag], code.missing]);
        }),
        { timeout: Duration.toMillis('2 minutes') },
    );
    it.effect(
        'states the size and type of a stored object and none once it is removed',
        Effect.fnUntraced(function* () {
            const key = 'media/stated';
            yield* stored(key, content.subarray(0, 13));
            const stat = Effect.map(
                ObjectStorage.use((storage) => storage.stat(key)),
                Option.map(({ size, mime }) => ({ size: ByteSize.toNumberUnsafe(size), mime })),
            );
            const before = yield* stat;
            yield* ObjectStorage.remove(key, Option.none());
            assert.deepStrictEqual([before, yield* stat], [Option.some({ size: 13, mime: media.mime }), Option.none()]);
        }),
    );
    it.effect.each([
        { operation: 'PutObject', request: { method: 'PUT', key: 'media/a.mp4' }, query: [], headers: { 'Content-Type': media.mime, 'If-None-Match': '*' }, signed: 'content-length;content-type;host;if-none-match' },
        {
            operation: 'UploadPart',
            request: { method: 'PUT', key: 'media/a.mp4', uploadId: 'u1', partNumber: 3 },
            query: [
                ['partNumber', '3'],
                ['uploadId', 'u1'],
            ],
            headers: {},
            signed: 'host',
        },
        { operation: 'CreateMultipartUpload', request: { method: 'POST', key: 'media/a.mp4' }, query: [['uploads', '']], headers: { 'Content-Type': media.mime }, signed: 'content-type;host' },
        { operation: 'CompleteMultipartUpload', request: { method: 'POST', key: 'media/a.mp4', uploadId: 'u1' }, query: [['uploadId', 'u1']], headers: { 'If-None-Match': '*' }, signed: 'host;if-none-match' },
        { operation: 'ListParts', request: { method: 'GET', key: 'media/a.mp4', uploadId: 'u1' }, query: [['uploadId', 'u1']], headers: {}, signed: 'host' },
        { operation: 'AbortMultipartUpload', request: { method: 'DELETE', key: 'media/a.mp4', uploadId: 'u1' }, query: [['uploadId', 'u1']], headers: {}, signed: 'host' },
    ] as const)(
        'presigns $operation as $request.method with its S3 query, signed headers, and caller headers',
        Effect.fnUntraced(function* ({ request, query, headers, signed }) {
            const presigned = yield* ObjectStorage.use(({ sign }) => sign(request, { size: ByteSize.bytes(21), mime: media.mime }));
            const url = new URL(presigned.url);
            assert.deepStrictEqual([url.origin, url.pathname, [...url.searchParams].filter(([name]) => !name.startsWith('X-Amz-')), url.searchParams.get('X-Amz-SignedHeaders'), presigned.headers], ['http://127.0.0.1', '/objects/media/a.mp4', query, signed, headers]);
        }),
    );
});
