import { assert, type Vitest } from '@effect/vitest';
import { ObjectStorage, type Presignable } from '@rasm/object-storage';
import { Array, ByteSize, Bytes, Duration, Effect, Function, Option, Record, Schema, Struct } from 'effect';
import { HttpClient, HttpClientRequest, HttpClientResponse, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { XMLParser } from 'fast-xml-parser';
import { checkObjectStore, formatConformanceReport } from 'partial-content/conformance';

// --- [CONSTANTS] -----------------------------------------------------------------------

const part = ByteSize.toNumberUnsafe(ByteSize.fromStringUnsafe('5 MiB'));
const media = { mime: 'video/mp4', options: { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) } } as const;
const code = { ok: 200, partial: 206, unchanged: 304, missing: 404, failed: 412, unsatisfiable: 416 };
const timeout = { timeout: Duration.toMillis('2 minutes') };

// --- [MODELS] --------------------------------------------------------------------------

const Created = Schema.Struct({ InitiateMultipartUploadResult: Schema.Struct({ UploadId: Schema.String }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const fill = (size: number, seed: number) => Uint8Array.from({ length: size }, (_, index) => (index + seed) * Math.SQRT2);
const content = fill(2 * part + 7, 0);
const xml = new XMLParser({ parseTagValue: false });
const read = Effect.fn('read')(function* (key: string, init: RequestInit = {}) {
    const response = HttpServerResponse.toWeb(yield* ObjectStorage.serve({ key, mime: media.mime }, media.options).pipe(Effect.provideService(HttpServerRequest.HttpServerRequest, HttpServerRequest.fromWeb(new Request(`https://storage.test/${key}`, init)))));
    return { status: response.status, headers: response.headers, body: yield* Effect.tryPromise(() => response.bytes()) };
});
const status = (key: string) => Effect.map(read(key), Struct.get('status'));
const sent = Effect.fn('sent')(function* (request: typeof Presignable.Type, stated: { readonly size: number; readonly mime: string }, body: (prepared: HttpClientRequest.HttpClientRequest) => HttpClientRequest.HttpClientRequest = Function.identity) {
    const { url, headers } = yield* ObjectStorage.use((storage) => storage.sign(request, { size: ByteSize.bytes(stated.size), mime: stated.mime }));
    const client = yield* HttpClient.HttpClient;
    return yield* client.execute(body(HttpClientRequest.make(request.method)(url, { headers })));
});
const multipart = Effect.fn('multipart')(function* (key: string, bytes: Uint8Array, stated: { readonly size: number; readonly mime: string }) {
    const created = yield* sent({ method: 'POST', key }, stated);
    const { UploadId: uploadId } = (yield* Schema.decodeUnknownEffect(Created)(xml.parse(yield* created.text))).InitiateMultipartUploadResult;
    const chunks = Array.chunksOf(Array.fromIterable(bytes), part).map((chunk) => Uint8Array.from(chunk));
    const etags = yield* Effect.forEach(
        chunks,
        Effect.fnUntraced(function* (chunk, index) {
            const accepted = yield* HttpClientResponse.filterStatusOk(yield* sent({ method: 'PUT', key, uploadId, partNumber: index + 1 }, stated, HttpClientRequest.bodyUint8Array(chunk)));
            return yield* Effect.fromNullishOr(accepted.headers['etag']);
        }),
    );
    const listed = etags.map((etag, index) => `<Part><PartNumber>${index + 1}</PartNumber><ETag>${etag}</ETag></Part>`).join('');
    const completion = yield* Effect.result(sent({ method: 'POST', key, uploadId }, stated, HttpClientRequest.bodyText(`<CompleteMultipartUpload>${listed}</CompleteMultipartUpload>`, 'application/xml')));
    return { uploadId, completion };
});
const completed = Effect.fn('completed')(function* (key: string, bytes: Uint8Array) {
    const { completion } = yield* multipart(key, bytes, { size: bytes.length, mime: media.mime });
    return (yield* Effect.fromResult(completion)).status;
});

const reads = <E>(it: Vitest.MethodsNonLive<ObjectStorage | HttpClient.HttpClient>, stored: (key: string, bytes: Uint8Array) => Effect.Effect<unknown, E, ObjectStorage | HttpClient.HttpClient>) => {
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
            assert.deepStrictEqual([Array.map(['content-type', 'content-disposition', 'x-content-type-options', 'accept-ranges', 'etag'], (name) => served.whole.headers.get(name)), yield* status(`${key}/absent`)], [[media.mime, media.options.disposition, 'nosniff', 'bytes', etag], code.missing]);
        }),
        timeout,
    );
};
const conforms = <E>(it: Vitest.MethodsNonLive<ObjectStorage | HttpClient.HttpClient>, stored: (key: string, bytes: Uint8Array) => Effect.Effect<unknown, E, ObjectStorage | HttpClient.HttpClient>) => {
    it.effect(
        'meets the partial-content read contract',
        Effect.fnUntraced(function* () {
            const key = 'media/conformance';
            yield* stored(key, content);
            const report = yield* ObjectStorage.use(({ objects }) => Effect.tryPromise(() => checkObjectStore(objects, { key, content })));
            assert.isTrue(report.ok, formatConformanceReport(report));
        }),
        timeout,
    );
};
const writes = (it: Vitest.MethodsNonLive<ObjectStorage | HttpClient.HttpClient>) => {
    it.effect(
        'publishes a presigned multipart upload at completion with its stated type',
        Effect.fnUntraced(function* () {
            const key = 'media/multipart';
            assert.strictEqual(yield* completed(key, content), code.ok);
            const stated = yield* ObjectStorage.use((storage) => storage.stat(key));
            const served = yield* read(key);
            assert.deepStrictEqual([Option.map(stated, ({ size, mime }) => [ByteSize.toNumberUnsafe(size), mime]), served.status, Bytes.Equivalence(served.body, content)], [Option.some([content.length, media.mime]), code.ok, true]);
        }),
        timeout,
    );
    it.effect(
        'keeps the first completed body at a contended key and refuses every later completion with 412',
        Effect.fnUntraced(function* () {
            const bodies = Array.makeBy(3, (seed) => fill(part + 3, seed + 1));
            const [first, ...later] = yield* Effect.forEach(bodies, (body) => completed('media/contended', body));
            const served = yield* read('media/contended');
            assert.deepStrictEqual([first, later, Bytes.Equivalence(served.body, Array.headNonEmpty(bodies))], [code.ok, [code.failed, code.failed], true]);
        }),
        timeout,
    );
    it.effect(
        'refuses a single presigned put whose body differs from the stated size or lands on a stored key',
        Effect.fnUntraced(function* () {
            const small = fill(64, 3);
            const put = (key: string, size: number) => Effect.map(sent({ method: 'PUT', key }, { size, mime: media.mime }, HttpClientRequest.bodyUint8Array(small)), Struct.get('status'));
            const answered = yield* Effect.all([put('media/sized', small.length + 1), put('media/single', small.length), put('media/single', small.length)]);
            assert.deepStrictEqual([answered[0] >= 400, answered[1], answered[2], yield* status('media/sized')], [true, code.ok, code.failed, code.missing]);
        }),
        timeout,
    );
    it.effect(
        'removes an upload and its object on every repeat and for an absent key',
        Effect.fnUntraced(function* () {
            const key = 'media/removed';
            const { uploadId } = yield* multipart(key, content, { size: 1, mime: media.mime });
            yield* completed(key, content);
            yield* Effect.all([ObjectStorage.remove(key, Option.some(uploadId)), ObjectStorage.remove(key, Option.some(uploadId)), ObjectStorage.remove(`${key}/absent`, Option.none())]);
            const listed = yield* sent({ method: 'GET', key, uploadId }, { size: 1, mime: media.mime });
            assert.deepStrictEqual([yield* status(key), listed.status, yield* ObjectStorage.use((storage) => storage.stat(key))], [code.missing, code.missing, Option.none()]);
        }),
        timeout,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { completed, conforms, reads, writes };
