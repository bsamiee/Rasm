import { type Cause, Effect, Queue, Stream } from 'effect';
import { AnnotationMode, type PDFDocumentProxy, PixelsPerInch } from 'pdfjs-dist';
import { EventBus, PDFPageView } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useState } from 'react';
import 'pdfjs-dist/web/pdf_viewer.css';

// --- [OPERATIONS] ----------------------------------------------------------------------

const resized = (container: HTMLDivElement) =>
    Stream.callback<void>(
        (queue) =>
            Effect.acquireRelease(
                Effect.sync(() => {
                    const observer = new ResizeObserver(() => Queue.offerUnsafe(queue, undefined));
                    observer.observe(container);
                    return observer;
                }),
                (observer) => Effect.sync(() => observer.disconnect()),
            ),
        { bufferSize: 1, strategy: 'sliding' },
    );
const pdfPage = Effect.fn('pdfPage')(function* (container: HTMLDivElement, document: PDFDocumentProxy, page: number) {
    const proxy = yield* Effect.tryPromise(() => document.getPage(page));
    const viewport = proxy.getViewport({ scale: 1 });
    const view = yield* Effect.acquireRelease(
        Effect.sync(() => new PDFPageView({ container, id: page, defaultViewport: viewport, eventBus: new EventBus(), annotationMode: AnnotationMode.ENABLE })),
        (live) =>
            Effect.sync(() => {
                live.destroy();
                live.div.remove();
            }),
    );
    view.setPdfPage(proxy);
    yield* resized(container).pipe(
        Stream.runForEach(() =>
            Effect.tryPromise(() => {
                view.update({ scale: Math.min(container.clientWidth / viewport.width, container.clientHeight / viewport.height) / PixelsPerInch.PDF_TO_CSS_UNITS });
                return view.draw();
            }),
        ),
    );
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const usePdfPage = (document: PDFDocumentProxy, page: number) => {
    const [failure, setFailure] = useState<Cause.UnknownError>();
    if (failure !== undefined) {
        throw failure;
    }
    return (container: HTMLDivElement) => {
        const controller = new AbortController();
        Effect.runFork(pdfPage(container, document, page).pipe(Effect.catch((error) => Effect.sync(() => setFailure(error)))), { signal: controller.signal });
        return () => controller.abort();
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfPage, usePdfPage };
