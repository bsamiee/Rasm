import { Array, Cause, Effect, FiberSet, Match, Option, pipe, Record, String, Struct } from 'effect';
import { Atom } from 'effect/reactivity';
import { getDocument, InvalidPDFException, PasswordException, PDFDataRangeTransport, PDFWorker, ResponseException } from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import { MediaError } from '#/error';
import { assets } from '#/pdf/assets';

// --- [SERVICES] ------------------------------------------------------------------------

const worker = Atom.make(
    Effect.acquireRelease(
        Effect.sync(() => PDFWorker.create({ port: new Worker(workerUrl, { type: 'module' }) })),
        (created) =>
            Effect.sync(() => {
                created.port.terminate();
                created.destroy();
            }),
    ),
).pipe(Atom.keepAlive);

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = (subject: string) =>
    Match.type<unknown>().pipe(
        Match.when(Match.instanceOf(PasswordException), (cause) => MediaError.make({ reason: { _tag: 'Protected', code: cause.code }, subject, cause })),
        Match.when(Match.instanceOf(InvalidPDFException), (cause) => MediaError.make({ reason: { _tag: 'Unreadable' }, subject, cause })),
        Match.when((cause: unknown) => cause instanceof ResponseException && cause.missing === true, (cause) => MediaError.make({ reason: { _tag: 'Missing' }, subject, cause })),
        Match.orElse((cause) => new Cause.UnknownError(cause)),
    );
const open = Effect.fn('PdfDocument.open')(function* (source: DocumentInitParameters, subject: string) {
    const shared = yield* Atom.getResult(worker);
    const task = yield* Effect.acquireRelease(
        Effect.sync(() => getDocument({ ...Record.map(assets.directories, (directory) => `${import.meta.env.BASE_URL}${assets.path}/${directory}/`), ...source, worker: shared })),
        (loading) => Effect.promise(() => loading.destroy()),
    );
    return yield* Effect.tryPromise({ try: () => task.promise, catch: failure(subject) });
});
const pages = Effect.fn('PdfDocument.pages')(function* (file: File, { concurrency }: { readonly concurrency: number }) {
    const fetches = yield* FiberSet.make<void, Cause.UnknownError>();
    const fetchRange = yield* FiberSet.runtime(fetches)();
    const range: PDFDataRangeTransport = Object.assign(new PDFDataRangeTransport(file.size, null), {
        requestDataRange: (begin: number, end: number) => fetchRange(Effect.tryPromise(() => file.slice(begin, end).bytes()).pipe(Effect.map((chunk) => range.onDataRange(begin, chunk)))),
    });
    const [labels, proxies] = yield* Effect.gen(function* () {
        const document = yield* open({ range, disableAutoFetch: true, disableStream: true }, file.name);
        return yield* Effect.all([Effect.tryPromise(() => document.getPageLabels()), Effect.forEach(Array.range(1, document.numPages), (number) => Effect.tryPromise(() => document.getPage(number)), { concurrency })], { concurrency: 'unbounded' });
    }).pipe(Effect.raceFirst(Effect.andThen(FiberSet.join(fetches), Effect.never)));
    return Array.map(proxies, (page, index) => ({ ...Struct.pick(page.getViewport({ scale: 1 }), ['width', 'height']), ...pipe(Array.get(labels ?? [], index), Option.filter(String.isNonEmpty), Option.map((label) => ({ label })), Option.getOrUndefined) }));
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const family = Atom.family((source: DocumentInitParameters & { readonly url: string | URL }) => Atom.make(open(source, `${source.url}`)));
const PdfDocument = { family, pages };

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfDocument };
