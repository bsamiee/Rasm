import { Array, Cause, Deferred, Effect, FiberSet, Match, Option, pipe, Record, String, Struct } from 'effect';
import { Atom } from 'effect/reactivity';
import { GlobalWorkerOptions, getDocument, InvalidPDFException, PasswordException, PDFDataRangeTransport, PDFWorker } from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import { InspectError } from '#/inspection';
import { assets } from '#/pdf/assets';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pdfWorker = Effect.acquireRelease(
    Effect.sync(() => {
        GlobalWorkerOptions.workerSrc = workerUrl;
        return new PDFWorker();
    }),
    (worker) => Effect.sync(() => worker.destroy()),
);
const openPdf = Effect.fn('openPdf')(function* (source: DocumentInitParameters & { readonly worker: PDFWorker }) {
    const task = yield* Effect.acquireRelease(
        Effect.try(() => getDocument({ ...Record.map(assets.directories, (directory) => `${import.meta.env.BASE_URL}${assets.path}/${directory}/`), ...source })),
        (loading) => Effect.promise(() => loading.destroy()),
    );
    return yield* Effect.tryPromise({
        try: () => task.promise,
        catch: Match.type<unknown>().pipe(
            Match.when(Match.instanceOf(PasswordException), (cause) => InspectError.make({ reason: { _tag: 'Protected' }, cause })),
            Match.when(Match.instanceOf(InvalidPDFException), (cause) => InspectError.make({ reason: { _tag: 'Unreadable' }, cause })),
            Match.orElse((cause) => new Cause.UnknownError(cause)),
        ),
    });
});
const pdfPages = Effect.fn('pdfPages')(function* (file: Blob) {
    const unread = yield* Deferred.make<never, Cause.UnknownError>();
    const read = yield* FiberSet.makeRuntime();
    const range: PDFDataRangeTransport = Object.assign(new PDFDataRangeTransport(file.size, null), {
        requestDataRange: (begin: number, end: number) =>
            read(
                Effect.tryPromise(() => file.slice(begin, end).bytes()).pipe(
                    Effect.map((chunk) => range.onDataRange(begin, chunk)),
                    Effect.catch((error) => Deferred.fail(unread, error)),
                ),
            ),
    });
    const unreadable = Deferred.await(unread);
    const document = yield* Effect.raceFirst(openPdf({ range, disableAutoFetch: true, disableStream: true, worker: yield* pdfWorker }), unreadable);
    const [labels, pages] = yield* Effect.raceFirst(Effect.all([Effect.tryPromise(() => document.getPageLabels()), Effect.forEach(Array.range(1, document.numPages), (number) => Effect.tryPromise(() => document.getPage(number)), { concurrency: 'unbounded' })], { concurrency: 'unbounded' }), unreadable);
    return {
        pages: Array.map(pages, (page, index) => ({
            ...Struct.pick(page.getViewport({ scale: 1 }), ['width', 'height']),
            ...pipe(Option.fromNullOr(labels), Option.flatMap(Array.get(index)), Option.filter(String.isNonEmpty), Option.match({ onNone: () => ({}), onSome: (label) => ({ label }) })),
        })),
    };
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const sharedWorker = Atom.make(pdfWorker).pipe(Atom.keepAlive);
const pdfDocument = Atom.family((source: DocumentInitParameters) => Atom.make((get) => Effect.flatMap(get.result(sharedWorker), (worker) => openPdf({ ...source, worker }))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfDocument, pdfPages };
