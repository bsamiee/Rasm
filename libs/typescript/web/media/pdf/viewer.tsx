import 'pdfjs-dist';
import { usePinch } from '@use-gesture/react';
import { Cause, Data, Duration, Effect, Filter, flow, HashSet, identity, Option, Predicate, Queue, Result, Schema, Stream } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import type { DEFAULT_SCALE_VALUE, MAX_SCALE, MIN_SCALE, TextLayerMode } from 'pdfjs-dist/types/web/ui_utils.d.ts';
import { EventBus, FindState, PDFFindController, PDFLinkService, PDFViewer, ScrollMode, SpreadMode } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { type ComponentProps, useEffect, useEffectEvent, useState } from 'react';
import 'pdfjs-dist/web/pdf_viewer.css';

// --- [TYPES] ---------------------------------------------------------------------------

type Options = Omit<ConstructorParameters<typeof PDFViewer>[0], 'container' | 'eventBus' | 'linkService' | 'findController'> & {
    readonly link?: Omit<NonNullable<ConstructorParameters<typeof PDFLinkService>[0]>, 'eventBus'>;
    readonly find?: Omit<ConstructorParameters<typeof PDFFindController>[0], 'eventBus' | 'linkService'>;
    readonly view?: Partial<typeof View.Type>;
};
type Status = typeof Found.Type & { readonly failedPages: HashSet.HashSet<number> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const rotations = { UPRIGHT: 0, CLOCKWISE: 90, INVERTED: 180, COUNTERCLOCKWISE: 270 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const View = Schema.Struct({
    pagesRotation: Schema.Enum(rotations),
    scrollMode: Schema.Enum(ScrollMode),
    spreadMode: Schema.Enum(SpreadMode),
    currentPageNumber: Schema.Int,
    currentScaleValue: Schema.Union([Schema.Literals(['auto', 'page-actual', 'page-fit', 'page-height', 'page-width']), Schema.TemplateLiteral([Schema.Finite])]),
});
const Found = Schema.Struct({ matchesCount: Schema.Struct({ current: Schema.Int, total: Schema.Int }), findState: Schema.optionalKey(Schema.Enum(FindState)) }).pipe(Schema.encodeKeys({ findState: 'state' }));
const Rendered = Schema.Struct({ pageNumber: Schema.Int, error: Schema.Unknown });
class PdfViewer extends Data.Class<typeof View.Type & Status & { readonly pdfViewer: PDFViewer }> {
    static readonly TextLayerMode = { DISABLE: 0, ENABLE: 1, ENABLE_PERMISSIONS: 2 } as const satisfies typeof TextLayerMode;
    static readonly mount = flow(
        Effect.fn('PdfViewer.mount')(function* (container: HTMLDivElement, document: PDFDocumentProxy, { link, find, view: { currentScaleValue = 'auto' satisfies typeof DEFAULT_SCALE_VALUE, ...view } = {}, ...options }: Options) {
            const labels = yield* Effect.tryPromise(() => document.getPageLabels());
            const eventBus = new EventBus();
            const linkService = new PDFLinkService({ ...link, eventBus });
            const signal = yield* Effect.abortSignal;
            const viewer: ConstructorParameters<typeof PDFViewer>[0] & { readonly abortSignal: AbortSignal } = { ...options, container, eventBus, linkService, findController: new PDFFindController({ ...find, eventBus, linkService }), abortSignal: signal };
            const pdfViewer = yield* Effect.acquireRelease(
                Effect.sync(() => new PDFViewer(viewer)),
                (mounted) => Effect.sync(() => mounted.setDocument(null)),
            );
            const observer = yield* Effect.acquireRelease(
                Effect.sync(() => new ResizeObserver(() => Object.assign(pdfViewer, { currentScaleValue: pdfViewer.currentScaleValue }).update())),
                (created) => Effect.sync(() => created.disconnect()),
            );
            const updates = yield* Queue.unbounded<(status: Status) => Status, Cause.UnknownError>();
            const offered = (update: (status: Status) => Status) => Queue.offerUnsafe(updates, update);
            const viewed = () => offered(identity);
            const initialized = () => {
                pdfViewer.setPageLabels(labels);
                Object.assign(pdfViewer, { ...view, currentScaleValue });
                observer.observe(container);
            };
            const found = (event: unknown) => Option.map(Schema.decodeUnknownOption(Found)(event), (update) => offered((status) => ({ ...status, ...update })));
            const rendered = (event: unknown) => Option.map(Schema.decodeUnknownOption(Rendered)(event), ({ pageNumber, error }) => offered((status) => ({ ...status, failedPages: Predicate.isNullish(error) ? HashSet.remove(status.failedPages, pageNumber) : HashSet.add(status.failedPages, pageNumber) })));
            eventBus.on('pagesinit', () => Result.match(Result.try(initialized), { onSuccess: viewed, onFailure: (error) => Queue.failCauseUnsafe(updates, Cause.fail(new Cause.UnknownError(error))) }), { signal });
            ['pagechanging', 'scalechanging', 'rotationchanging', 'scrollmodechanged', 'spreadmodechanged'].forEach((name) => eventBus.on(name, viewed, { signal }));
            ['updatefindmatchescount', 'updatefindcontrolstate'].forEach((name) => eventBus.on(name, found, { signal }));
            eventBus.on('pagerendered', rendered, { signal });
            linkService.setViewer(pdfViewer);
            linkService.setDocument(document);
            pdfViewer.setDocument(document);
            return Stream.fromQueue(updates).pipe(
                Stream.scan(
                    (): Status => ({ matchesCount: { current: 0, total: 0 }, failedPages: HashSet.empty() }),
                    (status, update) => update(status),
                ),
                Stream.filterMap(Filter.fromPredicateOption((status) => Option.map(Schema.decodeUnknownOption(View)(pdfViewer), (state) => new PdfViewer({ ...state, ...status, pdfViewer })))),
            );
        }),
        Stream.unwrap,
    );
    static readonly assign = (viewer: PdfViewer, view: Partial<typeof View.Type>) => {
        Object.assign(viewer.pdfViewer, view);
    };
    static readonly rotate = (viewer: PdfViewer, quarterTurns: number) => {
        Object.assign(viewer.pdfViewer, { pagesRotation: viewer.pagesRotation + quarterTurns * rotations.CLOCKWISE });
    };
    static readonly find = (
        viewer: PdfViewer,
        query: string | readonly string[],
        type: '' | 'again' | 'highlightallchange' | 'casesensitivitychange' | 'entirewordchange' | 'diacriticmatchingchange',
        flags: Partial<Record<'caseSensitive' | 'entireWord' | 'findPrevious' | 'highlightAll' | 'matchDiacritics', boolean>> = {},
    ) => viewer.pdfViewer.eventBus.dispatch('find', { source: viewer.pdfViewer, query, type, highlightAll: true, ...flags });
    static readonly closeFind = (viewer: PdfViewer) => viewer.pdfViewer.eventBus.dispatch('findbarclose', { source: viewer.pdfViewer });
    static readonly useViewer = (document: PDFDocumentProxy, { pinch, ...options }: Options & { readonly pinch?: { readonly drawingDelay: Duration.Input } }) => {
        const [container, setContainer] = useState<HTMLDivElement | null>(null);
        const [state, setState] = useState<AsyncResult.AsyncResult<PdfViewer, Cause.UnknownError>>(AsyncResult.initial());
        const initial = useEffectEvent(() => options);
        const viewer = AsyncResult.value(state);
        useEffect(() => {
            if (container === null) {
                return;
            }
            const controller = new AbortController();
            Effect.runFork(
                PdfViewer.mount(container, document, initial()).pipe(
                    Stream.runForEach((mounted) => Effect.sync(() => setState(AsyncResult.success(mounted)))),
                    Effect.catchCause((cause) => Effect.sync(() => setState(AsyncResult.failure(cause)))),
                    Effect.onInterrupt(() => Effect.sync(() => setState(AsyncResult.initial()))),
                ),
                { signal: controller.signal },
            );
            return () => controller.abort();
        }, [container, document]);
        usePinch(({ offset: [target], origin }) => Option.map(Option.all([viewer, Option.fromNullishOr(pinch)]), ([{ pdfViewer }, { drawingDelay }]) => pdfViewer.updateScale({ scaleFactor: target / pdfViewer.currentScale, origin: [...origin], drawingDelay: Duration.toMillis(drawingDelay) })), {
            target: { current: container },
            enabled: pinch !== undefined && Option.isSome(viewer),
            from: () => [Option.getOrUndefined(viewer)?.pdfViewer.currentScale ?? 1, 0],
            scaleBounds: { min: 0.1, max: 25 } satisfies { readonly min: typeof MIN_SCALE; readonly max: typeof MAX_SCALE },
            pointer: { touch: true },
            preventDefault: true,
            eventOptions: { passive: false },
        });
        return {
            state,
            container: (props: Omit<ComponentProps<'div'>, 'children' | 'ref'> & { readonly [attribute: `data-${string}`]: string | number | boolean | undefined }) => (
                <div {...props} ref={setContainer}>
                    <div className="pdfViewer" />
                </div>
            ),
        };
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfViewer };
