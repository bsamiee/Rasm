import { assert, type Vitest } from '@effect/vitest';
import { ObjectStorage } from '@rasm/hosting';
import { Array, ByteSize, Bytes, Context, Deferred, Duration, Effect, Fiber, Function, Layer, Option, Record, type Scope, Stream, Struct } from 'effect';
import { HttpServerRequest, HttpServerResponse, Multipart } from 'effect/http';
import { isObjectNotFoundError } from 'partial-content';
import { checkObjectStore, checkResumableWriteStore, formatConformanceReport, tallyConformance } from 'partial-content/conformance';
import { isUploadNotFoundError } from 'partial-content/upload';

// --- [OPERATIONS] ----------------------------------------------------------------------

const cases = <E, R>(it: Vitest.MethodsNonLive<R>, storage: Layer.Layer<ObjectStorage, E, R>) => {
    const part = ByteSize.toNumberUnsafe(ByteSize.fromStringUnsafe('5 MiB'));
    const chunk = ByteSize.toNumberUnsafe(ByteSize.fromStringUnsafe('1 MiB')) + 1;
    const fill = (size: number, seed: number) => Uint8Array.from({ length: size }, (_, index) => (index + seed) * Math.SQRT2);
    const content = fill(2 * part + chunk, 0);
    const media = { mime: 'video/mp4', options: { disposition: 'inline', securityHeaders: () => ({ 'X-Content-Type-Options': 'nosniff' }) } } as const;
    const code = { ok: 200, partial: 206, unchanged: 304, missing: 404, failed: 412, unsatisfiable: 416 };
    const timeout = { timeout: Duration.toMillis('1 minute') };
    const chunked = (bytes: Uint8Array) => Stream.fromIterable(Array.makeBy(Math.ceil(bytes.length / chunk), (index) => bytes.subarray(index * chunk, (index + 1) * chunk)));
    const put = (key: string, size: number, body: Stream.Stream<Uint8Array, unknown>, started: (uploadToken: string) => Effect.Effect<unknown> = Function.constant(Effect.void)) =>
        ObjectStorage.upload({ key, size: ByteSize.bytes(size) }).pipe(
            Effect.tap(({ uploadToken }) => started(uploadToken)),
            Effect.flatMap(({ write }) => write(body)),
            Effect.scoped,
        );
    const read = Effect.fn('read')(function* (key: string, init: RequestInit = {}) {
        const response = HttpServerResponse.toWeb(yield* ObjectStorage.serve({ key, mime: media.mime }, media.options).pipe(Effect.provideService(HttpServerRequest.HttpServerRequest, HttpServerRequest.fromWeb(new Request(`https://storage.test/${key}`, init)))));
        return { status: response.status, headers: response.headers, body: yield* Effect.promise(() => response.bytes()) };
    });
    const status = (key: string) => Effect.map(read(key), Struct.get('status'));
    const held = Effect.fn('held')(
        function* (key: string) {
            const { objects } = yield* ObjectStorage;
            const { body } = yield* Effect.tryPromise(() => objects.getObject(key));
            return yield* Effect.promise(() => new Response(body).bytes());
        },
        Effect.catchIf(
            ({ cause }) => isObjectNotFoundError(cause),
            () => Effect.undefined,
        ),
    );
    const reopened = Effect.map(Layer.build(Layer.fresh(storage)), (built) => Context.get(built, ObjectStorage).uploads);
    const discarded = (uploadToken: string) => ObjectStorage.use(({ uploads }) => Effect.tryPromise(() => uploads.getUploadState(uploadToken)).pipe(Effect.match({ onFailure: ({ cause }) => isUploadNotFoundError(cause), onSuccess: Function.constFalse })));
    it.effect(
        'serves ranges across multipart boundaries, answers preconditions and sets the caller headers',
        Effect.fnUntraced(function* () {
            const [key, size] = ['media/served', content.length];
            yield* put(key, size, chunked(content));
            const etag = yield* Effect.fromNullishOr((yield* read(key, { method: 'HEAD' })).headers.get('etag'));
            const span = 8;
            const ranged = (first: number, last: number, headers: Record.ReadonlyRecord<string, string> = {}) => [{ headers: { ...headers, range: `bytes=${first}-${last}` } }, code.partial, [first, last + 1], `bytes ${first}-${last}/${size}`] as const;
            const requests = {
                whole: [{}, code.ok, [0, size], null],
                head: [{ method: 'HEAD' }, code.ok, [0, 0], null],
                bounded: ranged(1, span),
                straddling: ranged(part - span, part + span - 1),
                suffix: [{ headers: { range: `bytes=-${span}` } }, code.partial, [size - span, size], `bytes ${size - span}-${size - 1}/${size}`],
                tail: [{ headers: { range: `bytes=${2 * part}-` } }, code.partial, [2 * part, size], `bytes ${2 * part}-${size - 1}/${size}`],
                unsatisfiable: [{ headers: { range: `bytes=${size}-` } }, code.unsatisfiable, [0, 0], `bytes */${size}`],
                unchanged: [{ headers: { 'if-none-match': etag } }, code.unchanged, [0, 0], null],
                changed: [{ headers: { 'if-none-match': '"other"' } }, code.ok, [0, size], null],
                mismatched: [{ headers: { 'if-match': '"other"' } }, code.failed, [0, 0], null],
                matched: ranged(0, span, { 'if-match': etag }),
                resumed: ranged(0, span, { 'if-range': etag }),
                restarted: [{ headers: { 'if-range': '"other"', range: `bytes=0-${span}` } }, code.ok, [0, size], null],
            } as const;
            const [served, multiple, missing] = yield* Effect.all(
                [
                    Effect.all(
                        Record.map(requests, ([init]) => read(key, init)),
                        { concurrency: 'unbounded' },
                    ),
                    read(key, { headers: { range: `bytes=0-1,${part}-${part + 1}` } }),
                    read(`${key}/absent`),
                ],
                { concurrency: 'unbounded' },
            );
            assert.deepStrictEqual(
                Record.map(requests, ([, , [start, end]], name) => [served[name].status, served[name].headers.get('content-range'), Bytes.Equivalence(served[name].body, content.subarray(start, end))]),
                Record.map(requests, ([, expected, , range]) => [expected, range, true]),
            );
            assert.deepStrictEqual(
                [
                    Array.map(['content-type', 'content-disposition', 'x-content-type-options', 'accept-ranges', 'etag'], (name) => served.whole.headers.get(name)),
                    served.head.headers.get('content-length'),
                    served.unchanged.headers.get('x-content-type-options'),
                    multiple.status,
                    multiple.headers.get('content-type')?.split(';')[0],
                    missing.status,
                ],
                [[media.mime, media.options.disposition, 'nosniff', 'bytes', etag], `${size}`, 'nosniff', code.partial, 'multipart/byteranges', code.missing],
            );
        }, Effect.provide(storage)),
        timeout,
    );
    it.effect(
        'aborts failed and interrupted uploads past a part and leaves no object or upload resource',
        Effect.fnUntraced(function* () {
            const [failedToken, interruptedToken, stalled] = yield* Effect.all([Deferred.make<string>(), Deferred.make<string>(), Deferred.make<void>()]);
            const prefix = chunked(content.subarray(0, part + 1));
            const failed = yield* Effect.flip(put('media/failed', content.length, Stream.concat(prefix, Stream.fail(Multipart.MultipartError.fromReason('Parse'))), (uploadToken) => Deferred.succeed(failedToken, uploadToken)));
            const interrupted = yield* Effect.forkChild(put('media/interrupted', content.length, Stream.concat(Stream.onEnd(prefix, Deferred.succeed(stalled, undefined)), Stream.never), (uploadToken) => Deferred.succeed(interruptedToken, uploadToken)));
            yield* Effect.andThen(Deferred.await(stalled), Fiber.interrupt(interrupted));
            const tokens = yield* Effect.all([Deferred.await(failedToken), Deferred.await(interruptedToken)]);
            assert.deepStrictEqual([failed._tag, yield* Effect.forEach(tokens, discarded), yield* Effect.forEach(['media/failed', 'media/interrupted'], status)], ['BadRequest', [true, true], [code.missing, code.missing]]);
        }, Effect.provide(storage)),
        timeout,
    );
    it.effect(
        'refuses bodies longer or shorter than the declared size and publishes nothing',
        Effect.fnUntraced(function* () {
            const bodies = { overrun: Stream.concat(chunked(content), Stream.make(Uint8Array.of(0))), underrun: chunked(content.subarray(1)) };
            const refused = yield* Effect.all(
                Record.map(bodies, (body, name) => Effect.flip(put(`media/${name}`, content.length, body))),
                { concurrency: 'unbounded' },
            );
            const stored = yield* Effect.all(Record.map(bodies, (_, name) => status(`media/${name}`)));
            assert.deepStrictEqual([Record.map(refused, ({ _tag }) => _tag), stored], [Record.map(bodies, Function.constant('BadRequest')), Record.map(bodies, Function.constant(code.missing))]);
        }, Effect.provide(storage)),
        timeout,
    );
    it.effect(
        'refuses a torn body after receiving all declared bytes',
        Effect.fnUntraced(function* () {
            const [key, token] = ['media/torn', yield* Deferred.make<string>()];
            const body = Stream.concat(chunked(content), Stream.fail(Multipart.MultipartError.fromReason('Parse')));
            const error = yield* Effect.flip(put(key, content.length, body, (issued) => Deferred.succeed(token, issued)));
            const uploadToken = yield* Deferred.await(token);
            assert.deepStrictEqual([error._tag, yield* status(key), yield* discarded(uploadToken)], ['BadRequest', code.missing, true]);
        }, Effect.provide(storage)),
        timeout,
    );
    it.effect(
        'keeps a whole successful body at a contended key and reports rejected writes as conflicts',
        Effect.fnUntraced(function* () {
            const contenders = 4;
            const bodies = Array.makeBy(contenders, (seed) => fill(part + chunk, seed + 1));
            const [published, refused] = yield* Effect.partition(bodies, (body) => Effect.as(put('media/contended', body.length, chunked(body)), body), { concurrency: 'unbounded' });
            const stored = yield* read('media/contended');
            assert.strictEqual(stored.status, code.ok);
            assert.isTrue(published.some((body) => Bytes.Equivalence(body, stored.body)));
            assert.deepStrictEqual(
                Array.map(refused, ({ _tag }) => _tag),
                Array.map(refused, Function.constant('Conflict')),
            );
        }, Effect.provide(storage)),
        timeout,
    );
    it.effect(
        'removes a stored object and its upload resource on every repeat and for an absent key',
        Effect.fnUntraced(function* () {
            const [key, token] = ['media/removed', yield* Deferred.make<string>()];
            yield* put(key, content.length, chunked(content), (issued) => Deferred.succeed(token, issued));
            const uploadToken = yield* Deferred.await(token);
            yield* Effect.all([ObjectStorage.remove(key, Option.some(uploadToken)), ObjectStorage.remove(key, Option.some(uploadToken)), ObjectStorage.remove(`${key}/absent`, Option.none())]);
            assert.deepStrictEqual([(yield* read(key)).status, yield* discarded(uploadToken)], [code.missing, true]);
        }, Effect.provide(storage)),
        timeout,
    );
    it.effect(
        'meets partial-content read and resumable write contracts across store instances and against published objects',
        Effect.fnUntraced(function* () {
            const [{ objects }, context] = yield* Effect.all([ObjectStorage, Effect.context<ObjectStorage | R | Scope.Scope>()]);
            const key = 'media/conformance';
            yield* put(key, content.length, chunked(content));
            const reopen = () => Effect.runPromiseWith(context)(reopened);
            const options = { key: (n: number) => `conformance/${n}`, readObject: (written: string) => Effect.runPromiseWith(context)(held(written)) };
            const reports = yield* Effect.all([Effect.promise(() => checkObjectStore(objects, { key, content })), Effect.promise(() => checkResumableWriteStore(reopen, options))], { concurrency: 'unbounded' });
            const report = tallyConformance(Array.flatMap(reports, ({ checks }) => checks));
            assert.isTrue(report.ok, formatConformanceReport(report));
        }, Effect.provide(storage)),
        timeout,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases };
