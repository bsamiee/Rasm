import { Array, Cause, Effect, FiberSet, Match, Option, Record, String, Struct } from 'effect';
import { Atom } from 'effect/reactivity';
import { getDocument, InvalidPDFException, PasswordException, PDFDataRangeTransport, PDFWorker, ResponseException } from 'pdfjs-dist';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import { assets } from '#/internal/assets';
import { MediaError } from '#/media-error';

// --- [TYPES] ---------------------------------------------------------------------------

type Source = DocumentInitParameters & { readonly base: string };

// --- [SERVICES] ------------------------------------------------------------------------

const worker = Atom.make(
    Effect.acquireRelease(
        Effect.sync(() => PDFWorker.create({ port: new Worker(new URL('pdfjs-dist/build/pdf.worker.min.mjs', import.meta.url), { type: 'module' }) })),
        (created) =>
            Effect.sync(() => {
                created.port.terminate();
                created.destroy();
            }),
    ),
).pipe(Atom.keepAlive);

// --- [OPERATIONS] ----------------------------------------------------------------------

const open = Effect.fn('open')(function* ({ base, ...source }: Source, subject: string) {
    const shared = yield* Atom.getResult(worker);
    const directories = Record.map(assets.directories, (directory) => `${base}${assets.path}/${directory}/`);
    const task = yield* Effect.acquireRelease(
        Effect.sync(() => getDocument({ ...directories, ...source, worker: shared })),
        (loading) => Effect.promise(loading.destroy.bind(loading)),
    );
    return yield* Effect.tryPromise({
        try: () => task.promise,
        catch: Match.type<unknown>().pipe(
            Match.when(Match.instanceOf(PasswordException), MediaError.of('Protected', subject)),
            Match.when(Match.instanceOf(InvalidPDFException), MediaError.of('Unreadable', subject)),
            Match.when((cause: unknown) => cause instanceof ResponseException && cause.missing, MediaError.of('Missing', subject)),
            Match.orElse((cause) => new Cause.UnknownError(cause)),
        ),
    });
});
const pages = Effect.fn('pages')(function* (file: File, { concurrency, base }: { readonly concurrency: number; readonly base: string }) {
    const fetches = yield* FiberSet.make<void, Cause.UnknownError>();
    const fetchRange = yield* FiberSet.runtime(fetches)();
    const range: PDFDataRangeTransport = Object.assign(new PDFDataRangeTransport(file.size, null), {
        requestDataRange: (begin: number, end: number) => fetchRange(Effect.tryPromise(() => file.slice(begin, end).bytes()).pipe(Effect.map((chunk) => range.onDataRange(begin, chunk)))),
    });
    const failed = Effect.andThen(FiberSet.join(fetches), Effect.interrupt);
    const document = yield* Effect.raceFirst(open({ base, range, disableAutoFetch: true, disableStream: true }, file.name), failed);
    const [labels, proxies] = yield* Effect.raceFirst(Effect.all([Effect.tryPromise(() => document.getPageLabels()), Effect.forEach(Array.range(1, document.numPages), (number) => Effect.tryPromise(document.getPage.bind(document, number)), { concurrency })], { concurrency: 'unbounded' }), failed);
    return Array.map(proxies, (page, index) => ({
        ...Struct.pick(page.getViewport({ scale: 1 }), ['width', 'height']),
        ...Record.getSomes({ label: Option.filter(Array.get(labels ?? [], index), String.isNonEmpty) }),
    }));
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const PdfDocument = Object.assign(
    Atom.family((source: Source & { readonly url: string | URL }) => Atom.make(open(source, `${source.url}`))),
    { pages },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfDocument };
