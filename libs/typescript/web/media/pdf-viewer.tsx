import 'pdfjs-dist';
import { usePinch } from '@use-gesture/react';
import { Cause, Data, Duration, Effect, Filter, flow, HashSet, identity, Option, Predicate, Queue, Result, Schema, Stream, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import type { DEFAULT_SCALE, DEFAULT_SCALE_VALUE, MAX_SCALE, MIN_SCALE, TextLayerMode } from 'pdfjs-dist/types/web/ui_utils';
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
        Effect.fn('PdfViewer.mount')(function* (container: HTMLDivElement, document: NonNullable<Parameters<PDFViewer['setDocument']>[0]>, { link, find, view: { currentScaleValue = 'auto' satisfies typeof DEFAULT_SCALE_VALUE, ...view } = {}, ...options }: Options) {
            const labels = yield* Effect.tryPromise(() => document.getPageLabels());
            const eventBus = new EventBus();
            const linkService = new PDFLinkService({ ...link, eventBus });
            const signal = yield* Effect.abortSignal;
            const viewer: ConstructorParameters<typeof PDFViewer>[0] & { readonly abortSignal: AbortSignal } = { ...options, container, eventBus, linkService, findController: new PDFFindController({ ...find, eventBus, linkService }), abortSignal: signal };
            const pdfViewer = yield* Effect.acquireRelease(
                Effect.sync(() => new PDFViewer(viewer)),
                (mounted) => Effect.sync(mounted.setDocument.bind(mounted, null)),
            );
            const observer = yield* Effect.acquireRelease(
                Effect.sync(() => new ResizeObserver(() => Object.assign(pdfViewer, { currentScaleValue: pdfViewer.currentScaleValue }).update())),
                (created) => Effect.sync(created.disconnect.bind(created)),
            );
            const initialized = () => {
                pdfViewer.setPageLabels(labels);
                Object.assign(pdfViewer, { ...view, currentScaleValue });
                observer.observe(container);
            };
            const updates = yield* Effect.acquireRelease(Queue.make<(status: Status) => Status, Cause.UnknownError | Cause.Done>(), Queue.shutdown);
            const viewed = () => Queue.offerUnsafe(updates, identity);
            const found = (event: unknown) => Option.map(Schema.decodeUnknownOption(Found)(event), (update) => Queue.offerUnsafe(updates, Struct.assign(update)));
            const rendered = (event: unknown) => Option.map(Schema.decodeUnknownOption(Rendered)(event), ({ pageNumber, error }) => Queue.offerUnsafe(updates, Struct.evolve({ failedPages: Predicate.isNullish(error) ? HashSet.remove(pageNumber) : HashSet.add(pageNumber) })));
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
                Stream.filterMap(
                    flow(
                        Filter.fromPredicateOption((status: Status) => Option.map(Schema.decodeUnknownOption(View)(pdfViewer), Struct.assign({ ...status, pdfViewer }))),
                        Result.map((state) => new PdfViewer(state)),
                    ),
                ),
            );
        }),
        Stream.unwrap,
    );
    static readonly useViewer = (document: NonNullable<Parameters<PDFViewer['setDocument']>[0]>, { pinch, ...options }: Options & { readonly pinch?: { readonly drawingDelay: Duration.Input } }) => {
        const [container, setContainer] = useState<HTMLDivElement | null>(null);
        const [state, setState] = useState<AsyncResult.AsyncResult<PdfViewer, Cause.UnknownError>>(AsyncResult.initial());
        const initial = useEffectEvent(() => options);
        const pinching = Option.all([AsyncResult.value(state), Option.fromNullishOr(pinch)]);
        useEffect(() => {
            if (container === null) {
                return;
            }
            const publish = (result: AsyncResult.AsyncResult<PdfViewer, Cause.UnknownError>) => Effect.sync(() => setState(result));
            const controller = new AbortController();
            Effect.runFork(
                PdfViewer.mount(container, document, initial()).pipe(
                    Stream.runForEach(flow(AsyncResult.success, publish)),
                    Effect.catchCause(flow(AsyncResult.failure<PdfViewer, Cause.UnknownError>, publish)),
                    Effect.onInterrupt(() => publish(AsyncResult.initial())),
                ),
                { signal: controller.signal },
            );
            return () => controller.abort();
        }, [container, document]);
        usePinch(({ offset: [target], origin }) => Option.map(pinching, ([{ pdfViewer }, { drawingDelay }]) => pdfViewer.updateScale({ scaleFactor: target / pdfViewer.currentScale, origin: [...origin], drawingDelay: Duration.toMillis(drawingDelay) })), {
            target: { current: container },
            enabled: Option.isSome(pinching),
            from: () => [Option.match(pinching, { onNone: () => 1 satisfies typeof DEFAULT_SCALE, onSome: ([{ pdfViewer }]) => pdfViewer.currentScale }), 0],
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
    assign(view: Partial<typeof View.Type>) {
        Object.assign(this.pdfViewer, view);
    }
    rotate(quarterTurns: number) {
        Object.assign(this.pdfViewer, { pagesRotation: this.pagesRotation + quarterTurns * rotations.CLOCKWISE });
    }
    find(query: string | readonly string[], type: '' | 'again' | 'highlightallchange' | 'casesensitivitychange' | 'entirewordchange' | 'diacriticmatchingchange', flags: Partial<Record<'caseSensitive' | 'entireWord' | 'findPrevious' | 'highlightAll' | 'matchDiacritics', boolean>> = {}) {
        this.pdfViewer.eventBus.dispatch('find', { source: this.pdfViewer, query, type, highlightAll: true, ...flags });
    }
    closeFind() {
        this.pdfViewer.eventBus.dispatch('findbarclose', { source: this.pdfViewer });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfViewer };
