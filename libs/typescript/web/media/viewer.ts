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
        (live) =>
            Effect.sync(() => {
                live.destroy();
                live.div.remove();
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
            Effect.tryPromise(() => {
                view.update({ scale: Math.min(container.clientWidth / viewport.width, container.clientHeight / viewport.height) / PixelsPerInch.PDF_TO_CSS_UNITS });
                return view.draw();
            }),
        ),
    );
}, Effect.scoped);
const pdfViewer = flow(
    Effect.fn('pdfViewer')(function* (container: HTMLDivElement, document: PDFDocumentProxy, { page, scale, linkTarget }: { readonly page: number; readonly scale: string; readonly linkTarget?: number }) {
        const labels = yield* Effect.tryPromise(() => document.getPageLabels());
        const eventBus = new EventBus();
        const linkService = new PDFLinkService({ eventBus, externalLinkTarget: linkTarget });
        const options: ConstructorParameters<typeof PDFSinglePageViewer>[0] & { readonly abortSignal: AbortSignal } = { container, eventBus, linkService, findController: new PDFFindController({ eventBus, linkService }), removePageBorders: true, abortSignal: yield* Effect.abortSignal };
        const pdf = yield* Effect.acquireRelease(
            Effect.sync(() => new PDFSinglePageViewer(options)),
            (live) => Effect.sync(() => live.setDocument(null)),
        );
        const actions = {
            fit: (value: string) => {
                pdf.currentScaleValue = value;
            },
            rotate: (degrees: number) => {
                pdf.pagesRotation += degrees;
            },
            find: (query: string, step: 'first' | 'next' | 'previous') => eventBus.dispatch('find', { query, type: step === 'first' ? '' : 'again', findPrevious: step === 'previous', highlightAll: true }),
        };
        const observer = yield* Effect.acquireRelease(
            Effect.sync(() => new ResizeObserver(() => actions.fit(pdf.currentScaleValue))),
            (live) => Effect.sync(() => live.disconnect()),
        );
        const initial = { page, scale, matches: { current: 0, total: 0 } };
        return Stream.callback<Partial<typeof initial>, Cause.UnknownError>((queue) => {
            const viewed = () => Queue.offerUnsafe(queue, { page: pdf.currentPageNumber, scale: pdf.currentScaleValue });
            const found = ({ matchesCount }: { readonly matchesCount: typeof initial.matches }) => Queue.offerUnsafe(queue, { matches: matchesCount });
            eventBus.on('pagesinit', () => {
                pdf.setPageLabels(labels);
                pdf.currentPageNumber = page;
                actions.fit(scale);
                observer.observe(container);
                viewed();
            });
            eventBus.on('pagechanging', viewed);
            eventBus.on('scalechanging', viewed);
            eventBus.on('updatefindmatchescount', found);
            eventBus.on('updatefindcontrolstate', found);
            eventBus.on('pagerendered', ({ error }: { readonly error: unknown }) => error === null || Queue.failCauseUnsafe(queue, Cause.fail(new Cause.UnknownError(error))));
            linkService.setViewer(pdf);
            linkService.setDocument(document);
            pdf.setDocument(document);
            return Effect.void;
        }).pipe(
            Stream.scan(
                () => initial,
                (state, update) => ({ ...state, ...update }),
            ),
            Stream.drop(1),
            Stream.map((state) => ({ ...state, ...actions, pdf })),
        );
    }),
    Stream.unwrap,
);

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
const usePdfViewer = (document: PDFDocumentProxy, options: Parameters<typeof pdfViewer>[2]) => {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const [viewer, setViewer] = useState<Stream.Success<ReturnType<typeof pdfViewer>> | null>(null);
    const [failure, setFailure] = useState<Cause.UnknownError>();
    const initial = useEffectEvent(() => options);
    useEffect(() => {
        if (container === null) {
            return;
        }
        const controller = new AbortController();
        Effect.runFork(
            pdfViewer(container, document, initial()).pipe(
                Stream.runForEach((state) => Effect.sync(() => setViewer(state))),
                Effect.catch((error) => Effect.sync(() => setFailure(error))),
                Effect.ensuring(Effect.sync(() => setViewer(null))),
            ),
            { signal: controller.signal },
        );
        return () => controller.abort();
    }, [container, document]);
    usePinch(({ offset: [target], origin }) => viewer?.pdf.updateScale({ scaleFactor: target / viewer.pdf.currentScale, origin: [...origin] }), {
        target: { current: container },
        enabled: viewer !== null,
        ...(viewer && { from: () => [viewer.pdf.currentScale, 0] }),
        pointer: { touch: true },
        preventDefault: true,
        eventOptions: { passive: false },
    });
    if (failure !== undefined) {
        throw failure;
    }
    return { ref: setContainer, viewer };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfPage, pdfViewer, usePdfPage, usePdfViewer };
