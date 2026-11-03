import { Array, Cause, Deferred, Effect, FiberSet, Match, Option, pipe, Record, String, Struct } from 'effect';
import { Atom } from 'effect/reactivity';
import { getDocument, InvalidPDFException, PasswordException, PDFDataRangeTransport, PDFWorker, ResponseException } from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import { MediaError } from '#/error';
import { assets } from '#/pdf/assets';

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = (subject: string) =>
    Match.type<unknown>().pipe(
        Match.when(Match.instanceOf(PasswordException), (cause) => MediaError.make({ reason: { _tag: 'Protected', code: cause.code }, subject, cause })),
        Match.when(Match.instanceOf(InvalidPDFException), (cause) => MediaError.make({ reason: { _tag: 'Unreadable' }, subject, cause })),
        Match.when(Match.instanceOf(ResponseException), (cause) => (cause.missing === true ? MediaError.make({ reason: { _tag: 'Missing' }, subject, cause }) : new Cause.UnknownError(cause))),
        Match.orElse((cause) => new Cause.UnknownError(cause)),
    );
const open = Effect.fn('open')(function* (source: DocumentInitParameters & { readonly worker: PDFWorker }, subject: string) {
    const task = yield* Effect.acquireRelease(
        Effect.sync(() => getDocument({ ...Record.map(assets.directories, (directory) => `${import.meta.env.BASE_URL}${assets.path}/${directory}/`), ...source })),
        (loading) => Effect.promise(() => loading.destroy()),
    );
    return yield* Effect.tryPromise({ try: () => task.promise, catch: failure(subject) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = Atom.make(
    Effect.gen(function* () {
        const port = yield* Effect.acquireRelease(
            Effect.sync(() => new Worker(workerUrl, { type: 'module' })),
            (spawned) => Effect.sync(() => spawned.terminate()),
        );
        return yield* Effect.acquireRelease(
            Effect.sync(() => PDFWorker.create({ port })),
            (created) => Effect.sync(() => created.destroy()),
        );
    }),
).pipe(Atom.keepAlive);
const pdfDocument = Atom.family((source: DocumentInitParameters & { readonly url: string | URL }) => Atom.make((get) => Effect.flatMap(get.result(worker), (shared) => open({ ...source, worker: shared }, `${source.url}`))));
const pdfPages = Effect.fn('pdfPages')(function* (file: File, { concurrency }: { readonly concurrency: number }) {
    const rangeFailure = yield* Deferred.make<never, Cause.UnknownError>();
    const fetchRange = yield* FiberSet.makeRuntime();
    const range: PDFDataRangeTransport = Object.assign(new PDFDataRangeTransport(file.size, null), {
        requestDataRange: (begin: number, end: number) =>
            fetchRange(
                Effect.tryPromise(() => file.slice(begin, end).bytes()).pipe(
                    Effect.map((chunk) => range.onDataRange(begin, chunk)),
                    Effect.catch((error) => Deferred.fail(rangeFailure, error)),
                ),
            ),
    });
    const rangeFailed = Deferred.await(rangeFailure);
    const document = yield* open({ range, disableAutoFetch: true, disableStream: true, worker: yield* Atom.getResult(worker) }, file.name).pipe(Effect.raceFirst(rangeFailed));
    const [labels, pages] = yield* Effect.all([Effect.tryPromise(() => document.getPageLabels()), Effect.forEach(Array.range(1, document.numPages), (number) => Effect.tryPromise(() => document.getPage(number)), { concurrency })], { concurrency: 'unbounded' }).pipe(Effect.raceFirst(rangeFailed));
    return Array.map(pages, (page, index) => ({
        ...Struct.pick(page.getViewport({ scale: 1 }), ['width', 'height']),
        ...pipe(Option.fromNullOr(labels), Option.flatMap(Array.get(index)), Option.filter(String.isNonEmpty), Option.match({ onNone: () => ({}), onSome: (label) => ({ label }) })),
    }));
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfDocument, pdfPages };
