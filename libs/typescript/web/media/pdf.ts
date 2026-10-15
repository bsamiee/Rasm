import { Effect, Formatter, Match, Record, Schema, type Scope } from 'effect';
import { Atom } from 'effect/reactivity';
import { GlobalWorkerOptions, getDocument, InvalidPDFException, PasswordException, type PDFDocumentProxy, type PDFPageProxy } from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import { base, directories } from './pdfjs.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const assets = Record.map(directories, (directory) => `${import.meta.env.BASE_URL}${base}/${directory}/`);

// --- [ERRORS] --------------------------------------------------------------------------

const PdfReason = Schema.TaggedUnion({
    Unread: { cause: Schema.Defect() },
    Protected: {},
    Invalid: { cause: Schema.Defect() },
    Unopened: { cause: Schema.Defect() },
    Unlabeled: { cause: Schema.Defect() },
    Unloaded: { page: Schema.Finite, cause: Schema.Defect() },
});
class PdfError extends Schema.TaggedError<PdfError, { readonly brand: unique symbol }>()('PdfError', { reason: PdfReason }) {
    override get message(): string {
        return PdfReason.match(this.reason, {
            Unread: ({ cause }) => `the PDF file could not be read: ${Formatter.format(cause)}`,
            Protected: () => 'the PDF is password protected',
            Invalid: ({ cause }) => `the file is not a valid PDF: ${Formatter.format(cause)}`,
            Unopened: ({ cause }) => `PDF.js could not open the document: ${Formatter.format(cause)}`,
            Unlabeled: ({ cause }) => `PDF.js could not read the page labels: ${Formatter.format(cause)}`,
            Unloaded: ({ page, cause }) => `PDF.js could not load page ${page}: ${Formatter.format(cause)}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const loadPage = (document: PDFDocumentProxy, page: number): Effect.Effect<PDFPageProxy, PdfError> => Effect.tryPromise({ try: () => document.getPage(page), catch: (cause) => PdfError.make({ reason: { _tag: 'Unloaded', page, cause } }) });
const openPdf = Effect.fnUntraced(function* (source: DocumentInitParameters): Effect.fn.Return<PDFDocumentProxy, PdfError, Scope.Scope> {
    const task = yield* Effect.acquireRelease(
        Effect.sync(() => getDocument({ ...source, ...assets })),
        (loading) => Effect.promise(() => loading.destroy()),
    );
    return yield* Effect.tryPromise({
        try: () => task.promise,
        catch: (cause) =>
            PdfError.make({
                reason: Match.value(cause).pipe(
                    Match.withReturnType<typeof PdfReason.Type>(),
                    Match.when(Match.instanceOf(PasswordException), () => ({ _tag: 'Protected' })),
                    Match.when(Match.instanceOf(InvalidPDFException), () => ({ _tag: 'Invalid', cause })),
                    Match.orElse(() => ({ _tag: 'Unopened', cause })),
                ),
            }),
    });
});
const inspectPdf = Effect.fnUntraced(function* (file: Blob) {
    const data = yield* Effect.tryPromise({ try: () => file.arrayBuffer(), catch: (cause) => PdfError.make({ reason: { _tag: 'Unread', cause } }) });
    const document = yield* openPdf({ data });
    const labels = yield* Effect.tryPromise({ try: () => document.getPageLabels(), catch: (cause) => PdfError.make({ reason: { _tag: 'Unlabeled', cause } }) });
    const pages = yield* Effect.forEach(
        Array.from({ length: document.numPages }, (_, index) => index),
        (index) =>
            loadPage(document, index + 1).pipe(
                Effect.map((page) => {
                    const { width, height } = page.getViewport({ scale: 1 });
                    const label = labels?.[index];
                    return { width, height, ...(label === undefined ? {} : { label }) };
                }),
            ),
    );
    return { pages };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

GlobalWorkerOptions.workerSrc = workerUrl;

const pdfDocument = Atom.family((url: string) => Atom.make(openPdf({ url, disableStream: true, disableAutoFetch: true })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectPdf, loadPage, PdfError, pdfDocument };
