import { usePinch } from '@use-gesture/react';
import { Effect, Fiber, Layer, type Scope } from 'effect';
import { AnnotationMode, type PDFDocumentProxy, PixelsPerInch, RenderingCancelledException } from 'pdfjs-dist';
import { EventBus, PDFFindController, PDFLinkService, PDFPageView, PDFSinglePageViewer } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useEffect, useRef, useState } from 'react';
import 'pdfjs-dist/web/pdf_viewer.css';
import { loadPage, type PdfError } from './pdf.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface View {
    readonly page: number;
    readonly fit: string;
}
interface Matches {
    readonly current: number;
    readonly total: number;
}
interface Controls {
    readonly scale: (value: string) => void;
    readonly zoomIn: () => void;
    readonly zoomOut: () => void;
    readonly page: (number: number) => void;
    readonly previous: () => void;
    readonly next: () => void;
    readonly rotate: (degrees: number) => void;
    readonly pan: (x: number, y: number) => void;
    readonly find: (query: string, step: 'first' | 'next' | 'previous') => void;
    readonly closeFind: () => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const renderPage = Effect.fnUntraced(function* (container: HTMLDivElement, document: PDFDocumentProxy, page: number, onError: (error: unknown) => void): Effect.fn.Return<void, PdfError, Scope.Scope> {
    const pdfPage = yield* loadPage(document, page);
    const viewport = pdfPage.getViewport({ scale: 1 });
    const view = yield* Effect.acquireRelease(
        Effect.sync(() => new PDFPageView({ container, id: page, defaultViewport: viewport, eventBus: new EventBus(), annotationMode: AnnotationMode.ENABLE })),
        (value) =>
            Effect.sync(() => {
                value.destroy();
                value.div.remove();
            }),
    );
    view.setPdfPage(pdfPage);
    const observer = yield* Effect.acquireRelease(
        Effect.sync(
            () =>
                new ResizeObserver(() => {
                    const { clientWidth: width, clientHeight: height } = container;
                    view.update({ scale: Math.min(width / viewport.width, height / viewport.height) / PixelsPerInch.PDF_TO_CSS_UNITS });
                    view.draw().catch((error: unknown) => (error instanceof RenderingCancelledException ? undefined : onError(error)));
                }),
        ),
        (value) => Effect.sync(() => value.disconnect()),
    );
    observer.observe(container);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const usePdfPage = (container: HTMLDivElement | null, document: PDFDocumentProxy, page: number, onError: (error: unknown) => void): void => {
    useEffect(() => {
        if (container === null) {
            return;
        }
        const fiber = renderPage(container, document, page, onError).pipe(
            Effect.catch((error) => Effect.sync(() => onError(error))),
            Layer.effectDiscard,
            Layer.launch,
            Effect.runFork,
        );
        return (): void => {
            Effect.runFork(Fiber.interrupt(fiber));
        };
    }, [container, document, page, onError]);
};
const usePdfViewer = (
    container: HTMLDivElement | null,
    document: PDFDocumentProxy,
    { page, labels, scale, linkTarget, onError }: { readonly page: number; readonly labels: readonly string[]; readonly scale: string; readonly linkTarget?: number; readonly onError: (error: unknown) => void },
): { readonly view: View | null; readonly matches: Matches; readonly controls: Controls } => {
    const viewer = useRef<PDFSinglePageViewer | null>(null);
    const [view, setView] = useState<View | null>(null);
    const [matches, setMatches] = useState<Matches>({ current: 0, total: 0 });
    useEffect(() => {
        if (container === null) {
            return;
        }
        const controller = new AbortController();
        const eventBus = new EventBus();
        const linkService = new PDFLinkService({ eventBus, externalLinkTarget: linkTarget });
        const findController = new PDFFindController({ eventBus, linkService });
        const options = { container, eventBus, linkService, findController, removePageBorders: true, abortSignal: controller.signal };
        const nativeViewer = new PDFSinglePageViewer(options);
        linkService.setViewer(nativeViewer);
        linkService.setDocument(document);
        const resize = new ResizeObserver(() => {
            const current = nativeViewer.currentScaleValue;
            nativeViewer.currentScaleValue = current;
        });
        eventBus.on('pagesinit', () => {
            nativeViewer.setPageLabels([...labels]);
            nativeViewer.currentPageNumber = page;
            nativeViewer.currentScaleValue = scale;
            resize.observe(container);
        });
        const read = (): void => setView({ page: nativeViewer.currentPageNumber, fit: nativeViewer.currentScaleValue });
        eventBus.on('pagechanging', read);
        eventBus.on('scalechanging', read);
        eventBus.on('updatefindmatchescount', ({ matchesCount }: { matchesCount: Matches }) => setMatches(matchesCount));
        eventBus.on('updatefindcontrolstate', ({ matchesCount }: { matchesCount: Matches }) => setMatches(matchesCount));
        eventBus.on('pagerendered', ({ error }: { error: unknown }) => {
            if (error !== null) {
                onError(error);
            }
        });
        nativeViewer.setDocument(document);
        viewer.current = nativeViewer;
        return (): void => {
            resize.disconnect();
            nativeViewer.setDocument(null);
            controller.abort();
        };
    }, [container, document, page, labels, scale, linkTarget, onError]);
    usePinch(
        ({ offset: [offset], origin }): void => {
            viewer.current?.updateScale({ scaleFactor: offset / viewer.current.currentScale, origin });
        },
        { target: { current: container }, enabled: view !== null, from: () => [viewer.current?.currentScale ?? 1, 0], pointer: { touch: true }, preventDefault: true, eventOptions: { passive: false } },
    );
    const controls: Controls = {
        scale: (value) => {
            if (viewer.current !== null) {
                viewer.current.currentScaleValue = value;
            }
        },
        zoomIn: () => viewer.current?.increaseScale(),
        zoomOut: () => viewer.current?.decreaseScale(),
        page: (number) => {
            if (viewer.current !== null) {
                viewer.current.currentPageNumber = number;
            }
        },
        previous: () => viewer.current?.previousPage(),
        next: () => viewer.current?.nextPage(),
        rotate: (degrees) => {
            if (viewer.current !== null) {
                viewer.current.pagesRotation += degrees;
            }
        },
        pan: (x, y) => viewer.current?.panBy(x, y),
        find: (query, step) => viewer.current?.eventBus.dispatch('find', { query, type: step === 'first' ? '' : 'again', findPrevious: step === 'previous', highlightAll: true }),
        closeFind: () => viewer.current?.eventBus.dispatch('findbarclose', {}),
    };
    return { view, matches, controls };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { usePdfPage, usePdfViewer };
