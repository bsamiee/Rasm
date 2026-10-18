import { Cause, Effect, Match, Record, Struct } from 'effect';
import { Atom } from 'effect/reactivity';
import { GlobalWorkerOptions, getDocument, InvalidPDFException, PasswordException } from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import { InspectError } from './inspect.ts';
import { base, directories } from './pdfjs.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const openPdf = Effect.fn('openPdf')(function* (source: DocumentInitParameters) {
    const task = yield* Effect.acquireRelease(
        Effect.sync(() => getDocument({ ...source, ...Record.map(directories, (directory) => `${import.meta.env.BASE_URL}${base}/${directory}/`) })),
        (loading) => Effect.promise(() => loading.destroy()),
    );
    return yield* Effect.tryPromise({
        try: () => task.promise,
        catch: Match.type<unknown>().pipe(
            Match.when(Match.instanceOf(PasswordException), () => InspectError.make({ reason: { _tag: 'Protected' } })),
            Match.when(Match.instanceOf(InvalidPDFException), (cause) => InspectError.make({ reason: { _tag: 'Invalid', cause } })),
            Match.orElse((cause) => new Cause.UnknownError(cause)),
        ),
    });
});
const inspectPdf = Effect.fn('inspectPdf')(function* (file: Blob) {
    const document = yield* openPdf({ data: yield* Effect.tryPromise(() => file.arrayBuffer()) });
    const [labels, pages] = yield* Effect.all(
        [
            Effect.tryPromise(() => document.getPageLabels()),
            Effect.forEach(
                Array.from({ length: document.numPages }, (_, index) => index + 1),
                (number) => Effect.tryPromise(() => document.getPage(number)),
                { concurrency: 'unbounded' },
            ),
        ],
        { concurrency: 'unbounded' },
    );
    return {
        pages: pages.map((page, index) => {
            const label = labels?.[index];
            return { ...Struct.pick(page.getViewport({ scale: 1 }), ['width', 'height']), ...(label === undefined ? {} : { label }) };
        }),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

GlobalWorkerOptions.workerSrc = workerUrl;

const pdfDocument = Atom.family((url: string) => Atom.make(openPdf({ url, disableStream: true, disableAutoFetch: true })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectPdf, pdfDocument };
