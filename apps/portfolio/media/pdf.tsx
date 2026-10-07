import { useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { usePinch } from '@use-gesture/react';
import { type Cause, Effect, Fiber, Layer, type Scope } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { ChevronLeft, ChevronRight, RotateCw } from 'lucide-react';
import { motion } from 'motion/react';
import { AnnotationMode, PasswordException, type PDFDocumentProxy, PixelsPerInch, RenderingCancelledException } from 'pdfjs-dist';
import { EventBus, LinkTarget, PDFFindController, PDFLinkService, PDFPageView, PDFSinglePageViewer } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { type ReactElement, useEffect, useReducer, useState } from 'react';
import { Button, Input, SearchField, ToggleButton, Toolbar } from 'react-aria-components';
import { useErrorBoundary } from 'react-error-boundary';
import type { Placement } from '../model/placement.ts';
import { ZoomControls } from './controls.tsx';
import { mediaUrl, numeral } from './display.ts';
import { pdfDocument } from './engine.ts';
import { MediaFailure, MediaLoading } from './status.tsx';
import 'pdfjs-dist/web/pdf_viewer.css';

// --- [OPERATIONS] ----------------------------------------------------------------------

const renderPreview = Effect.fnUntraced(function* (container: HTMLDivElement, document: PDFDocumentProxy, page: number, showBoundary: (error: unknown) => void): Effect.fn.Return<void, Cause.UnknownError, Scope.Scope> {
    const pdfPage = yield* Effect.tryPromise(() => document.getPage(page));
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
                    view.draw().catch((error: unknown) => (error instanceof RenderingCancelledException ? undefined : showBoundary(error)));
                }),
        ),
        (value) => Effect.sync(() => value.disconnect()),
    );
    observer.observe(container);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function SheetPreview({ document, page }: { document: PDFDocumentProxy; page: number }): ReactElement {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const { showBoundary } = useErrorBoundary();
    useEffect(() => {
        if (!container) {
            return;
        }
        const fiber = renderPreview(container, document, page, showBoundary).pipe(
            Effect.catch((error) => Effect.sync(() => showBoundary(error.cause))),
            Layer.effectDiscard,
            Layer.launch,
            Effect.runFork,
        );
        return (): void => {
            Effect.runFork(Fiber.interrupt(fiber));
        };
    }, [container, document, page, showBoundary]);
    return <div className="pdfViewer singlePageView pointer-events-none size-full overflow-hidden" ref={setContainer} />;
}
function SheetViewer({ document, placement, layoutId }: { document: PDFDocumentProxy; placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string }): ReactElement {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const [viewer, setViewer] = useState<PDFSinglePageViewer | null>(null);
    const [query, setQuery] = useState('');
    const [matches, setMatches] = useState({ current: 0, total: 0 });
    const quarterTurn = 90;
    const [pan, setPan] = useState(false);
    const [, redraw] = useReducer((value: number): number => value + 1, 0);
    const { showBoundary } = useErrorBoundary();
    useEffect(() => {
        if (!container) {
            return;
        }
        const controller = new AbortController();
        const eventBus = new EventBus();
        const linkService = new PDFLinkService({ eventBus, externalLinkTarget: LinkTarget.BLANK });
        const findController = new PDFFindController({ eventBus, linkService });
        const options = { container, eventBus, linkService, findController, removePageBorders: true, abortSignal: controller.signal };
        const nativeViewer = new PDFSinglePageViewer(options);
        linkService.setViewer(nativeViewer);
        linkService.setDocument(document);
        const resize = new ResizeObserver(() => {
            const scale = nativeViewer.currentScaleValue;
            nativeViewer.currentScaleValue = scale;
        });
        eventBus.on('pagesinit', () => {
            nativeViewer.setPageLabels(placement.asset.pages.map((page, index) => page.label ?? String(index + 1)));
            nativeViewer.currentPageNumber = placement.page.number;
            nativeViewer.currentScaleValue = 'page-fit';
            resize.observe(container);
            redraw();
        });
        eventBus.on('pagechanging', redraw);
        eventBus.on('scalechanging', redraw);
        eventBus.on('updatefindmatchescount', ({ matchesCount }: { matchesCount: typeof matches }) => setMatches(matchesCount));
        eventBus.on('updatefindcontrolstate', ({ matchesCount }: { matchesCount: typeof matches }) => setMatches(matchesCount));
        eventBus.on('pagerendered', ({ error }: { error: unknown }) => {
            if (error) {
                showBoundary(error);
            }
        });
        nativeViewer.setDocument(document);
        setViewer(nativeViewer);
        return (): void => {
            resize.disconnect();
            nativeViewer.setDocument(null);
            controller.abort();
        };
    }, [container, document, placement.asset.pages, placement.page.number, showBoundary]);
    usePinch(
        ({ offset: [scale], origin }): void => {
            viewer?.updateScale({ scaleFactor: scale / viewer.currentScale, origin });
        },
        { target: { current: container }, enabled: viewer !== null, from: () => [viewer?.currentScale ?? 1, 0], pointer: { touch: true }, preventDefault: true, eventOptions: { passive: false } },
    );
    const currentPage = viewer?.currentPageNumber ?? placement.page.number;
    return (
        <>
            {viewer && (
                <ZoomControls
                    fit={
                        <>
                            <ToggleButton
                                className="button button-ghost"
                                isSelected={viewer.currentScaleValue === 'page-fit'}
                                onPress={(): void => {
                                    viewer.currentScaleValue = 'page-fit';
                                }}
                            >
                                Fit page
                            </ToggleButton>
                            <ToggleButton
                                className="button button-ghost"
                                isSelected={viewer.currentScaleValue === 'page-width'}
                                onPress={(): void => {
                                    viewer.currentScaleValue = 'page-width';
                                }}
                            >
                                Fit width
                            </ToggleButton>
                        </>
                    }
                    href={`${mediaUrl(placement.asset)}#page=${currentPage}`}
                    linkLabel="Original"
                    zoomIn={(): void => viewer.increaseScale()}
                    zoomOut={(): void => viewer.decreaseScale()}
                >
                    <div className="flex w-fit min-w-0 max-w-full items-center [&>button]:min-w-11 [&>button]:px-2">
                        <span className="sr-only" role="status">
                            Sheet {currentPage} of {document.numPages}
                            {placement.asset.pages[currentPage - 1]?.label ? ` · ${placement.asset.pages[currentPage - 1]?.label}` : ''}
                        </span>
                        <Button
                            aria-label="Previous sheet"
                            className="button button-ghost"
                            isDisabled={currentPage === 1}
                            onPress={(): void => {
                                viewer.previousPage();
                            }}
                        >
                            <ChevronLeft className="size-4" />
                        </Button>
                        <select
                            aria-label="Sheet"
                            className="min-h-11 min-w-0 max-w-48 bg-transparent px-1 font-mono text-xs tabular-nums"
                            onChange={(event): void => {
                                viewer.currentPageNumber = event.currentTarget.selectedIndex + 1;
                            }}
                            value={currentPage}
                        >
                            {placement.asset.pages.map(({ label }, index) => (
                                // biome-ignore lint/suspicious/noArrayIndexKey: A PDF source page ordinal is its immutable identity.
                                <option key={index} value={index + 1}>
                                    {numeral(index + 1)} / {numeral(document.numPages)}
                                    {label ? ` · ${label}` : ''}
                                </option>
                            ))}
                        </select>
                        <Button
                            aria-label="Next sheet"
                            className="button button-ghost"
                            isDisabled={currentPage === document.numPages}
                            onPress={(): void => {
                                viewer.nextPage();
                            }}
                        >
                            <ChevronRight className="size-4" />
                        </Button>
                    </div>
                    <details className="ml-auto min-w-0 open:basis-full">
                        <summary className="min-h-11 cursor-pointer content-center text-sm">Tools</summary>
                        <Toolbar aria-label="View options" className="flex w-fit items-center [&>button]:min-w-11 [&>button]:selected:bg-transparent [&>button]:px-3 [&>button]:selected:text-foreground [&>button]:selected:decoration-accent-text">
                            <ToggleButton className="button button-ghost" isSelected={pan} onChange={setPan}>
                                Pan
                            </ToggleButton>
                            <Button
                                className="button button-ghost"
                                onPress={(): void => {
                                    viewer.pagesRotation += quarterTurn;
                                }}
                            >
                                <RotateCw className="size-4" />
                                Rotate view
                            </Button>
                        </Toolbar>
                        <p className="hint py-2">Select text to copy. Use Pan or focus the drawing and press arrow keys to move.</p>
                    </details>
                    <details
                        className="min-w-0 open:basis-full"
                        onToggle={(event): void => {
                            if (!event.currentTarget.open) {
                                viewer.eventBus.dispatch('findbarclose', {});
                            }
                        }}
                    >
                        <summary className="min-h-11 cursor-pointer content-center text-sm">Find</summary>
                        <div className="flex flex-wrap items-center gap-2 pb-1">
                            <SearchField
                                aria-label="Find in document"
                                className="flex min-w-0 flex-1 basis-40 items-center border border-control-line"
                                onChange={(text): void => {
                                    setQuery(text);
                                    viewer.eventBus.dispatch('find', { query: text, type: '', highlightAll: true });
                                }}
                                onSubmit={(): void => viewer.eventBus.dispatch('find', { query, type: 'again', highlightAll: true })}
                                value={query}
                            >
                                <Input className="min-h-11 w-full min-w-0 bg-transparent px-3 text-base" />
                                <Button className="button button-ghost px-3" isDisabled={!query}>
                                    Clear
                                </Button>
                            </SearchField>
                            <Button aria-label="Previous match" className="button button-ghost min-w-11 px-3" isDisabled={matches.total === 0} onPress={(): void => viewer.eventBus.dispatch('find', { query, type: 'again', findPrevious: true, highlightAll: true })}>
                                <ChevronLeft className="size-4" />
                            </Button>
                            <Button aria-label="Next match" className="button button-ghost min-w-11 px-3" isDisabled={matches.total === 0} onPress={(): void => viewer.eventBus.dispatch('find', { query, type: 'again', highlightAll: true })}>
                                <ChevronRight className="size-4" />
                            </Button>
                            <span className="basis-full text-muted text-xs" role="status">
                                {query ? `${matches.current} of ${matches.total} matches` : 'Search drawing numbers or text.'}
                            </span>
                        </div>
                    </details>
                </ZoomControls>
            )}
            <motion.div
                aria-label={`Sheet ${currentPage} of ${document.numPages}`}
                className="relative min-h-0 flex-1 overflow-hidden bg-surface landscape-short:col-span-full landscape-short:row-start-3"
                layoutId={layoutId}
                onDoubleClick={(): void => {
                    if (pan) {
                        viewer?.increaseScale();
                    }
                }}
                onPan={(_event, info): void => {
                    if (pan) {
                        viewer?.panBy(info.delta.x, info.delta.y);
                    }
                }}
            >
                {/* biome-ignore lint/a11y/noNoninteractiveTabindex: The native scroll viewport is keyboard-pannable. */}
                <div className={`absolute inset-0 overflow-auto overscroll-contain ${pan ? 'cursor-grab touch-none select-none [&_.textLayer]:pointer-events-none' : ''}`} ref={setContainer} tabIndex={0}>
                    <div className="pdfViewer" />
                </div>
            </motion.div>
        </>
    );
}
function PdfSheet({ placement, layoutId }: { placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId?: string }): ReactElement {
    const resource = pdfDocument(mediaUrl(placement.asset));
    const result = useAtomValue(resource);
    const retry = useAtomRefresh(resource);
    return AsyncResult.matchWithError(result, {
        onInitial: (): ReactElement => <MediaLoading />,
        onError: (error): ReactElement => (
            <MediaFailure href={mediaUrl(placement.asset)} linkLabel="Open original PDF" message={error.cause instanceof PasswordException ? 'This PDF is protected. Upload an unlocked export.' : 'This document could not be displayed.'} {...(error.cause instanceof PasswordException ? {} : { onRetry: retry })} />
        ),
        onDefect: (cause): ReactElement => {
            throw cause;
        },
        onSuccess: ({ value: document }): ReactElement => (layoutId === undefined ? <SheetPreview document={document} page={placement.page.number} /> : <SheetViewer document={document} layoutId={layoutId} placement={placement} />),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
