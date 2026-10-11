import { type Cause, Effect, type Scope } from 'effect';
import { Atom } from 'effect/reactivity';
import { GlobalWorkerOptions, getDocument, type PDFDocumentProxy } from 'pdfjs-dist';
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';
import type { DocumentInitParameters } from 'pdfjs-dist/types/src/display/api.d.ts';
import type { Page } from '../model/asset.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const openPdf = Effect.fnUntraced(function* (source: DocumentInitParameters): Effect.fn.Return<PDFDocumentProxy, Cause.UnknownError, Scope.Scope> {
    const task = yield* Effect.acquireRelease(
        Effect.sync(() => getDocument({ ...source, cMapUrl: '/pdfjs/cmaps/', standardFontDataUrl: '/pdfjs/standard_fonts/', wasmUrl: '/pdfjs/wasm/' })),
        (loading) => Effect.promise(() => loading.destroy()),
    );
    return yield* Effect.tryPromise(() => task.promise);
});
const inspectPdf = Effect.fnUntraced(function* (file: Blob): Effect.fn.Return<{ readonly pages: readonly (typeof Page.Type)[] }, Cause.UnknownError, Scope.Scope> {
    const data = yield* Effect.tryPromise(() => file.arrayBuffer());
    const document = yield* openPdf({ data });
    const labels = yield* Effect.tryPromise(() => document.getPageLabels());
    const pages = yield* Effect.forEach(
        Array.from({ length: document.numPages }, (_, index) => index),
        (index) =>
            Effect.tryPromise(() => document.getPage(index + 1)).pipe(
                Effect.map((page) => {
                    const { width, height } = page.getViewport({ scale: 1 });
                    const label = labels?.[index];
                    return { width, height, ...(label === undefined ? {} : { label }) };
                }),
            ),
        { concurrency: 'unbounded' },
    );
    return { pages };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

GlobalWorkerOptions.workerSrc = workerUrl;

const pdfDocument = Atom.family((url: string) => Atom.make(openPdf({ url, disableStream: true, disableAutoFetch: true })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectPdf, pdfDocument };
