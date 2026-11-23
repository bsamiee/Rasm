import 'pdfjs-dist';
import { useDrag, usePinch } from '@use-gesture/react';
import { Array, Cause, Data, Duration, Effect, Filter, flow, Function, HashSet, identity, Match, Option, Predicate, Queue, Record, Result, Schema, Stream, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { ChevronLeft, ChevronRight, RotateCw } from 'lucide-react';
import type { DEFAULT_SCALE, DEFAULT_SCALE_VALUE, MAX_SCALE, MIN_SCALE, TextLayerMode } from 'pdfjs-dist/types/web/ui_utils.js';
import { EventBus, FindState, PDFFindController, PDFLinkService, PDFViewer, ScrollMode, SpreadMode } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { type ComponentProps, useEffect, useEffectEvent, useState } from 'react';
import { Button, Group, Input, SearchField, ToggleButton, Toolbar, VisuallyHidden } from 'react-aria-components';
import { MediaError } from '#/error';
import { ZoomControls } from '#/zoom-controls.tsx';

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
const useViewer = (document: NonNullable<Parameters<PDFViewer['setDocument']>[0]>, { pinch, ...options }: Options & { readonly pinch?: { readonly drawingDelay: Duration.Input } }) => {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const [state, setState] = useState<AsyncResult.AsyncResult<PdfViewer, MediaError>>(AsyncResult.initial());
    const [pan, setPan] = useState(false);
    const [query, setQuery] = useState('');
    const initial = useEffectEvent(() => options);
    const live = AsyncResult.value(state);
    const pinching = Option.all([live, Option.fromNullishOr(pinch)]);
    const panning = Option.filter(live, () => pan);
    useEffect(() => {
        const controller = new AbortController();
        const publish = (result: AsyncResult.AsyncResult<PdfViewer, MediaError>) => Effect.sync(() => setState(result));
        if (container !== null) {
            Effect.runFork(
                PdfViewer.mount(container, document, initial()).pipe(
                    Stream.runForEach(flow(AsyncResult.success, publish)),
                    Effect.catchCause(flow(AsyncResult.failure<PdfViewer, MediaError>, publish)),
                    Effect.onInterrupt(() => publish(AsyncResult.initial())),
                ),
                { signal: controller.signal },
            );
        }
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
    useDrag(({ delta: [x, y] }) => Option.map(panning, ({ pdfViewer }) => pdfViewer.panBy(x, y)), { target: { current: container }, enabled: Option.isSome(panning), pointer: { touch: true } });
    return {
        state,
        container: (props: Omit<ComponentProps<'div'>, 'children' | 'ref' | 'onDoubleClick'> & Readonly<Record<`data-${string}`, string | number | boolean | undefined>>) => (
            <div {...props} data-pan={pan || undefined} onDoubleClick={() => Option.match(panning, { onNone: Function.constVoid, onSome: ({ pdfViewer }) => pdfViewer.increaseScale() })} ref={setContainer}>
                <div className="pdfViewer" />
            </div>
        ),
        controls: ({ className, href, page }: { readonly className?: string; readonly href: string; readonly page: (number: number) => string }) =>
            AsyncResult.builder(state)
                .onSuccess((viewer) => (
                    <ZoomControls
                        className={className}
                        fit={Record.collect({ 'page-fit': 'Fit page', 'page-width': 'Fit width' } as const, (scale, label) => (
                            <ToggleButton isSelected={viewer.currentScaleValue === scale} key={scale} onPress={() => viewer.assign({ currentScaleValue: scale })}>
                                {label}
                            </ToggleButton>
                        ))}
                        href={`${href}#page=${viewer.currentPageNumber}`}
                        zoomIn={() => viewer.pdfViewer.increaseScale()}
                        zoomOut={() => viewer.pdfViewer.decreaseScale()}
                    >
                        <Group aria-label="Pages">
                            <VisuallyHidden elementType="output">{page(viewer.currentPageNumber)}</VisuallyHidden>
                            <Button
                                aria-label="Previous page"
                                isDisabled={viewer.currentPageNumber === 1}
                                onPress={() => {
                                    viewer.pdfViewer.previousPage();
                                }}
                            >
                                <ChevronLeft />
                            </Button>
                            <select aria-label="Page" onChange={(event) => viewer.assign({ currentPageNumber: Number(event.currentTarget.value) })} value={viewer.currentPageNumber}>
                                {Array.makeBy(viewer.pdfViewer.pagesCount, (index) => (
                                    <option key={index} value={index + 1}>
                                        {page(index + 1)}
                                    </option>
                                ))}
                            </select>
                            <Button
                                aria-label="Next page"
                                isDisabled={viewer.currentPageNumber === viewer.pdfViewer.pagesCount}
                                onPress={() => {
                                    viewer.pdfViewer.nextPage();
                                }}
                            >
                                <ChevronRight />
                            </Button>
                        </Group>
                        <details>
                            <summary>Tools</summary>
                            <Toolbar aria-label="View options">
                                <ToggleButton isSelected={pan} onChange={setPan}>
                                    Pan
                                </ToggleButton>
                                <Button onPress={() => viewer.rotate(1)}>
                                    <RotateCw />
                                    Rotate view
                                </Button>
                            </Toolbar>
                            <p>Select text to copy. Drag with Pan on or press arrow keys on focused page to move</p>
                        </details>
                        <details onToggle={(event) => (event.currentTarget.open ? viewer.find(query, 'highlightallchange') : viewer.closeFind())}>
                            <summary>Find</summary>
                            <div>
                                <SearchField
                                    aria-label="Find in document"
                                    onChange={(text) => {
                                        setQuery(text);
                                        viewer.find(text, '');
                                    }}
                                    onSubmit={() => viewer.find(query, 'again')}
                                    value={query}
                                >
                                    <Input />
                                    <Button isDisabled={query === ''}>Clear</Button>
                                </SearchField>
                                <Button aria-label="Previous match" isDisabled={viewer.matchesCount.total === 0} onPress={() => viewer.find(query, 'again', { findPrevious: true })}>
                                    <ChevronLeft />
                                </Button>
                                <Button aria-label="Next match" isDisabled={viewer.matchesCount.total === 0} onPress={() => viewer.find(query, 'again')}>
                                    <ChevronRight />
                                </Button>
                                <output>
                                    {Match.value({ query, ...viewer.matchesCount }).pipe(
                                        Match.when({ query: '' }, () => 'Search text'),
                                        Match.when({ total: 0 }, () => 'No matches'),
                                        Match.orElse(({ current, total }) => `${current} of ${total}`),
                                    )}
                                </output>
                            </div>
                        </details>
                    </ZoomControls>
                ))
                .orNull(),
    };
};
class PdfViewer extends Data.Class<typeof View.Type & Status & { readonly pdfViewer: PDFViewer }> {
    static readonly TextLayerMode = { DISABLE: 0, ENABLE: 1, ENABLE_PERMISSIONS: 2 } as const satisfies typeof TextLayerMode;
    static readonly useViewer = useViewer;
    static readonly mount = flow(
        Effect.fn('PdfViewer.mount')(function* (container: HTMLDivElement, document: NonNullable<Parameters<PDFViewer['setDocument']>[0]>, { link, find, view: { currentScaleValue = 'auto' satisfies typeof DEFAULT_SCALE_VALUE, ...view } = {}, ...options }: Options) {
            const labels = yield* Effect.tryPromise({ try: () => document.getPageLabels(), catch: MediaError.of('Unreadable', 'PDF page labels') });
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
            const updates = yield* Effect.acquireRelease(Queue.make<(status: Status) => Status, MediaError | Cause.Done>(), Queue.shutdown);
            const viewed = () => Queue.offerUnsafe(updates, identity);
            const found = (event: unknown) => Option.map(Schema.decodeUnknownOption(Found)(event), (update) => Queue.offerUnsafe(updates, Struct.assign(update)));
            const rendered = (event: unknown) => Option.map(Schema.decodeUnknownOption(Rendered)(event), ({ pageNumber, error }) => Queue.offerUnsafe(updates, Struct.evolve({ failedPages: Predicate.isNullish(error) ? HashSet.remove(pageNumber) : HashSet.add(pageNumber) })));
            eventBus.on('pagesinit', () => Result.match(Result.try({ try: initialized, catch: MediaError.of('Unsupported', 'PDF viewer view') }), { onSuccess: viewed, onFailure: (error) => Queue.failCauseUnsafe(updates, Cause.fail(error)) }), { signal });
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
