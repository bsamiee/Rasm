import 'pdfjs-dist';
import { usePinch } from '@use-gesture/react';
import { Cause, Data, Duration, Effect, Filter, flow, Option, Queue, Result, Schema, Stream, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import type { MAX_SCALE, MIN_SCALE } from 'pdfjs-dist/types/web/ui_utils.d.ts';
import { EventBus, PDFFindController, PDFLinkService, PDFViewer } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useEffect, useEffectEvent, useState } from 'react';
import 'pdfjs-dist/web/pdf_viewer.css';

// --- [MODELS] --------------------------------------------------------------------------

const View = Schema.Struct({
    pagesRotation: Schema.Int,
    scrollMode: Schema.Int,
    spreadMode: Schema.Int,
    currentPageNumber: Schema.Int,
    currentScaleValue: Schema.Union([Schema.Literals(['auto', 'page-actual', 'page-fit', 'page-height', 'page-width']), Schema.TemplateLiteral([Schema.Finite])]),
});
const Found = Schema.Struct({ matchesCount: Schema.Struct({ current: Schema.Int, total: Schema.Int }) });
const Rendered = Schema.Struct({ error: Schema.Unknown });
class PdfViewer extends Data.Class<typeof View.Type & typeof Found.Type & { readonly pdf: PDFViewer }> {
    static readonly make = flow(
        Effect.fn('PdfViewer.make')(function* (
            container: HTMLDivElement,
            document: PDFDocumentProxy,
            {
                link,
                find,
                view: { currentScaleValue = 'auto', ...view } = {},
                ...options
            }: Omit<ConstructorParameters<typeof PDFViewer>[0], 'container' | 'eventBus' | 'linkService' | 'findController'> & {
                readonly link?: Omit<NonNullable<ConstructorParameters<typeof PDFLinkService>[0]>, 'eventBus'>;
                readonly find?: Omit<ConstructorParameters<typeof PDFFindController>[0], 'eventBus' | 'linkService'>;
                readonly view?: Partial<typeof View.Type>;
            },
        ) {
            const labels = yield* Effect.tryPromise(() => document.getPageLabels());
            const eventBus = new EventBus();
            const linkService = new PDFLinkService({ ...link, eventBus });
            const signal = yield* Effect.abortSignal;
            const viewer: ConstructorParameters<typeof PDFViewer>[0] & { readonly abortSignal: AbortSignal } = { ...options, container, eventBus, linkService, findController: new PDFFindController({ ...find, eventBus, linkService }), abortSignal: signal };
            const pdf = yield* Effect.acquireRelease(
                Effect.try(() => new PDFViewer(viewer)),
                (live) => Effect.sync(() => live.setDocument(null)),
            );
            return Stream.callback<Partial<typeof Found.Type>, Cause.UnknownError>((queue) => PdfViewer.#listen(queue, { pdf, document, labels, view: { ...view, currentScaleValue }, signal })).pipe(
                Stream.scan(
                    (): typeof Found.Type => ({ matchesCount: { current: 0, total: 0 } }),
                    (found, update) => ({ ...found, ...update }),
                ),
                Stream.filterMap(Filter.fromPredicateOption((found) => Option.map(Schema.decodeUnknownOption(View)(pdf), (state) => new PdfViewer({ ...state, ...found, pdf })))),
            );
        }),
        Stream.unwrap,
    );
    static readonly #listen = (
        queue: Queue.Queue<Partial<typeof Found.Type>, Cause.UnknownError | Cause.Done>,
        { pdf, document, labels, view, signal }: { readonly pdf: PDFViewer; readonly document: PDFDocumentProxy; readonly labels: Parameters<PDFViewer['setPageLabels']>[0]; readonly view: Partial<typeof View.Type>; readonly signal: AbortSignal },
    ) =>
        Effect.acquireRelease(
            Effect.try(() => {
                const offered = (update: Partial<typeof Found.Type>) => Queue.offerUnsafe(queue, update);
                const viewed = () => offered({});
                const failed = (error: unknown) => Queue.failCauseUnsafe(queue, Cause.fail(new Cause.UnknownError(error)));
                const observer = new ResizeObserver(() => {
                    Object.assign(pdf, { currentScaleValue: pdf.currentScaleValue }).update();
                });
                const initialized = () => {
                    pdf.setPageLabels(labels);
                    Object.assign(pdf, view);
                    observer.observe(pdf.container);
                };
                const found = (event: unknown) => Option.map(Schema.decodeUnknownOption(Found)(event), offered);
                const rendered = (event: unknown) => Option.map(Option.flatMapNullishOr(Schema.decodeUnknownOption(Rendered)(event), Struct.get('error')), failed);
                pdf.eventBus.on('pagesinit', () => Result.match(Result.try(initialized), { onSuccess: viewed, onFailure: failed }), { signal });
                ['pagechanging', 'scalechanging', 'rotationchanging', 'scrollmodechanged', 'spreadmodechanged'].forEach((name) => pdf.eventBus.on(name, viewed, { signal }));
                ['updatefindmatchescount', 'updatefindcontrolstate'].forEach((name) => pdf.eventBus.on(name, found, { signal }));
                pdf.eventBus.on('pagerendered', rendered, { signal });
                pdf.linkService.setViewer(pdf);
                pdf.linkService.setDocument(document);
                pdf.setDocument(document);
                return observer;
            }),
            (observer) => Effect.sync(() => observer.disconnect()),
        );
    static readonly assign = (viewer: PdfViewer, view: Partial<typeof View.Type>) => {
        Object.assign(viewer.pdf, view);
    };
    static readonly find = (
        viewer: PdfViewer,
        query: string | readonly string[],
        type: '' | 'again' | 'highlightallchange' | 'casesensitivitychange' | 'entirewordchange' | 'diacriticmatchingchange',
        flags: Partial<Record<'caseSensitive' | 'entireWord' | 'findPrevious' | 'highlightAll' | 'matchDiacritics', boolean>> = {},
    ) => viewer.pdf.eventBus.dispatch('find', { source: viewer.pdf, query, type, highlightAll: true, ...flags });
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const usePdfViewer = (document: PDFDocumentProxy, options: Parameters<typeof PdfViewer.make>[2]) => {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const [viewer, setViewer] = useState<AsyncResult.AsyncResult<PdfViewer, Cause.UnknownError>>(AsyncResult.initial());
    const initial = useEffectEvent(() => options);
    const live = AsyncResult.value(viewer);
    useEffect(() => {
        if (container === null) {
            return;
        }
        const controller = new AbortController();
        Effect.runFork(
            PdfViewer.make(container, document, initial()).pipe(
                Stream.runForEach((state) => Effect.sync(() => setViewer(AsyncResult.success(state)))),
                Effect.catchCause((cause) => Effect.sync(() => setViewer(AsyncResult.failure(cause)))),
                Effect.onInterrupt(() => Effect.sync(() => setViewer(AsyncResult.initial()))),
            ),
            { signal: controller.signal },
        );
        return () => controller.abort();
    }, [container, document]);
    usePinch(({ offset: [target], origin }) => Option.map(live, ({ pdf }) => pdf.updateScale({ scaleFactor: target / pdf.currentScale, origin: [...origin], drawingDelay: Duration.toMillis('400 millis') })), {
        target: { current: container },
        enabled: Option.isSome(live),
        from: () => [Option.match(live, { onNone: () => 1, onSome: ({ pdf }) => pdf.currentScale }), 0],
        scaleBounds: { min: 0.1, max: 25 } satisfies { readonly min: typeof MIN_SCALE; readonly max: typeof MAX_SCALE },
        pointer: { touch: true },
        preventDefault: true,
        eventOptions: { passive: false },
    });
    return { ref: setContainer, viewer };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfViewer, usePdfViewer };
