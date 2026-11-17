import { assert, type Vitest } from '@effect/vitest';
import { ObjectStorage } from '@rasm/object-storage';
import { Array, ByteSize, Bytes, Duration, Effect, Record, Struct } from 'effect';
import { type HttpClient, HttpServerRequest, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const part = ByteSize.toNumberUnsafe(ByteSize.fromStringUnsafe('5 MiB'));
const media = { mime: 'video/mp4', options: { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) } } as const;
const code = { ok: 200, partial: 206, unchanged: 304, missing: 404, failed: 412, unsatisfiable: 416 };
const timeout = { timeout: Duration.toMillis('2 minutes') };

// --- [OPERATIONS] ----------------------------------------------------------------------

const fill = (size: number, seed: number) => Uint8Array.from({ length: size }, (_, index) => (index + seed) * Math.SQRT2);
const content = fill(2 * part + 7, 0);
const read = Effect.fn('read')(function* (key: string, init: RequestInit = {}) {
    const response = HttpServerResponse.toWeb(yield* ObjectStorage.serve({ key, mime: media.mime }, media.options).pipe(Effect.provideService(HttpServerRequest.HttpServerRequest, HttpServerRequest.fromWeb(new Request(`https://storage.test/${key}`, init)))));
    return { status: response.status, headers: response.headers, body: yield* Effect.tryPromise(() => response.bytes()) };
});
const status = (key: string) => Effect.map(read(key), Struct.get('status'));
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { reads };
