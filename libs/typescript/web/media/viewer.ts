import { usePinch } from '@use-gesture/react';
import { Cause, Effect, flow, Queue, Stream } from 'effect';
import { AnnotationMode, type PDFDocumentProxy, PixelsPerInch } from 'pdfjs-dist';
import { EventBus, PDFFindController, PDFLinkService, PDFPageView, PDFSinglePageViewer } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useEffect, useEffectEvent, useState } from 'react';
import 'pdfjs-dist/web/pdf_viewer.css';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pdfPage = Effect.fn('pdfPage')(function* (container: HTMLDivElement, document: PDFDocumentProxy, page: number) {
    const proxy = yield* Effect.tryPromise(() => document.getPage(page));
    const viewport = proxy.getViewport({ scale: 1 });
    const view = yield* Effect.acquireRelease(
        Effect.sync(() => new PDFPageView({ container, id: page, defaultViewport: viewport, eventBus: new EventBus(), annotationMode: AnnotationMode.ENABLE })),
        (value) =>
            Effect.sync(() => {
                value.destroy();
                value.div.remove();
            }),
    );
    view.setPdfPage(proxy);
    yield* Stream.callback(
        (queue) => {
            const observer = new ResizeObserver(() => Queue.offerUnsafe(queue, undefined));
            observer.observe(container);
            return Effect.ensuring(
                Effect.never,
                Effect.sync(() => observer.disconnect()),
            );
        },
        { bufferSize: 1, strategy: 'sliding' },
    ).pipe(
        Stream.runForEach(() =>
            Effect.andThen(
                Effect.sync(() => view.update({ scale: Math.min(container.clientWidth / viewport.width, container.clientHeight / viewport.height) / PixelsPerInch.PDF_TO_CSS_UNITS })),
                Effect.tryPromise(() => view.draw()),
            ),
        ),
    );
}, Effect.scoped);
const pdfViewer = flow(
    Effect.fn('pdfViewer')(function* (container: HTMLDivElement, document: PDFDocumentProxy, { page, scale, linkTarget }: { readonly page: number; readonly scale: string; readonly linkTarget?: number }) {
        const labels = yield* Effect.tryPromise(() => document.getPageLabels());
        const eventBus = new EventBus();
        const linkService = new PDFLinkService({ eventBus, externalLinkTarget: linkTarget });
        const options: ConstructorParameters<typeof PDFSinglePageViewer>[0] & { readonly abortSignal: AbortSignal } = { container, eventBus, linkService, findController: new PDFFindController({ eventBus, linkService }), removePageBorders: true, abortSignal: yield* Effect.abortSignal };
        const viewer = yield* Effect.acquireRelease(
            Effect.sync(() => new PDFSinglePageViewer(options)),
            (live) => Effect.sync(() => live.setDocument(null)),
        );
        const controls = {
            fit: (value: string) => {
                viewer.currentScaleValue = value;
            },
            scale: () => viewer.currentScale,
            zoomIn: () => viewer.increaseScale(),
            zoomOut: () => viewer.decreaseScale(),
            zoom: (target: number, origin: readonly number[]) => viewer.updateScale({ scaleFactor: target / viewer.currentScale, origin: [...origin] }),
            page: (number: number) => {
                viewer.currentPageNumber = number;
            },
            previous: () => viewer.previousPage(),
            next: () => viewer.nextPage(),
            rotate: (degrees: number) => {
                viewer.pagesRotation += degrees;
            },
            pan: (x: number, y: number) => viewer.panBy(x, y),
            find: (query: string, step: 'first' | 'next' | 'previous') => eventBus.dispatch('find', { query, type: step === 'first' ? '' : 'again', findPrevious: step === 'previous', highlightAll: true }),
            closeFind: () => eventBus.dispatch('findbarclose', {}),
        };
        const observer = yield* Effect.acquireRelease(
            Effect.sync(() => new ResizeObserver(() => controls.fit(viewer.currentScaleValue))),
            (live) => Effect.sync(() => live.disconnect()),
        );
        return Stream.callback<{ readonly page: number; readonly fit: string } | { readonly matches: { readonly current: number; readonly total: number } }, Cause.UnknownError>((queue) => {
            const viewed = () => Queue.offerUnsafe(queue, { page: viewer.currentPageNumber, fit: viewer.currentScaleValue });
            const found = ({ matchesCount }: { readonly matchesCount: { readonly current: number; readonly total: number } }) => Queue.offerUnsafe(queue, { matches: matchesCount });
            eventBus.on('pagesinit', () => {
                viewer.setPageLabels(labels);
                controls.page(page);
                controls.fit(scale);
                observer.observe(container);
                viewed();
            });
            eventBus.on('pagechanging', viewed);
            eventBus.on('scalechanging', viewed);
            eventBus.on('updatefindmatchescount', found);
            eventBus.on('updatefindcontrolstate', found);
            eventBus.on('pagerendered', ({ error }: { readonly error: unknown }) => error === null || Queue.failCauseUnsafe(queue, Cause.fail(new Cause.UnknownError(error))));
            linkService.setViewer(viewer);
            linkService.setDocument(document);
            viewer.setDocument(document);
            return Effect.void;
        }).pipe(
            Stream.scan(
                () => ({ page, fit: scale, matches: { current: 0, total: 0 } }),
                (state, update) => ({ ...state, ...update }),
            ),
            Stream.drop(1),
            Stream.map((state) => ({ ...state, controls })),
        );
    }),
    Stream.unwrap,
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const usePdfPage = (document: PDFDocumentProxy, page: number, onError: (error: unknown) => void) => {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const failed = useEffectEvent(onError);
    useEffect(() => {
        if (container === null) {
            return;
        }
        const controller = new AbortController();
        Effect.runFork(pdfPage(container, document, page).pipe(Effect.catch((error) => Effect.sync(() => failed(error)))), { signal: controller.signal });
        return () => controller.abort();
    }, [container, document, page]);
    return { ref: setContainer };
};
const usePdfViewer = (document: PDFDocumentProxy, options: Parameters<typeof pdfViewer>[2] & { readonly onError: (error: unknown) => void }) => {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const [viewer, setViewer] = useState<Stream.Success<ReturnType<typeof pdfViewer>> | null>(null);
    const latest = useEffectEvent(() => options);
    useEffect(() => {
        if (container === null) {
            return;
        }
        const controller = new AbortController();
        Effect.runFork(
            pdfViewer(container, document, latest()).pipe(
                Stream.runForEach((state) => Effect.sync(() => setViewer(state))),
                Effect.catch((error) => Effect.sync(() => latest().onError(error))),
                Effect.ensuring(Effect.sync(() => setViewer(null))),
            ),
            { signal: controller.signal },
        );
        return () => controller.abort();
    }, [container, document]);
    usePinch(({ offset: [scale], origin }) => viewer?.controls.zoom(scale, origin), { target: { current: container }, enabled: viewer !== null, ...(viewer && { from: () => [viewer.controls.scale(), 0] }), pointer: { touch: true }, preventDefault: true, eventOptions: { passive: false } });
    return { ref: setContainer, viewer };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfPage, pdfViewer, usePdfPage, usePdfViewer };
