import { useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { pdfDocument } from '@rasm/media/pdf/document';
import { PdfViewer, usePdfViewer } from '@rasm/media/pdf/viewer';
import { Array, Match, Option } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { ChevronLeft, ChevronRight, RotateCw } from 'lucide-react';
import { motion } from 'motion/react';
import { AnnotationEditorType, AnnotationMode, type PDFDocumentProxy } from 'pdfjs-dist';
import { LinkTarget, ScrollMode, SpreadMode } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useState } from 'react';
import { Button, Input, SearchField, ToggleButton, Toolbar } from 'react-aria-components';
import { format, sheetLabel } from '#/media/labels';
import { MediaFailure, MediaLoading } from '#/media/status';
import { ZoomControls } from '#/media/zoom-controls';
import { mediaUrl, sheets } from '#/model/asset';
import type { Placement } from '#/model/placement';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sheet = (currentPageNumber: number) => ({ removePageBorders: true, annotationEditorMode: AnnotationEditorType.DISABLE, view: { currentPageNumber, currentScaleValue: 'page-fit', scrollMode: ScrollMode.PAGE, spreadMode: SpreadMode.NONE } }) satisfies Parameters<typeof usePdfViewer>[1];

// --- [COMPOSITION] ---------------------------------------------------------------------

const SheetViewer = ({ document, placement, layoutId, url }: { document: PDFDocumentProxy; placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string; url: string }) => {
    const [query, setQuery] = useState('');
    const [pan, setPan] = useState(false);
    const quarterTurn = 90;
    const matches = { one: 'match', other: 'matches' } as const;
    const pages = sheets(placement.asset);
    const { ref, viewer } = usePdfViewer(document, { ...sheet(placement.page.number), link: { externalLinkTarget: LinkTarget.BLANK } });
    const live = AsyncResult.value(viewer);
    const page = Option.flatMap(live, ({ currentPageNumber }) => Array.get(pages, currentPageNumber - 1)).pipe(Option.getOrElse(() => placement.page));
    return (
        <>
            {AsyncResult.builder(viewer)
                .onSuccess((state) => (
                    <ZoomControls
                        fit={
                            <>
                                <ToggleButton className="button button-ghost" isSelected={state.currentScaleValue === 'page-fit'} onPress={() => PdfViewer.assign(state, { currentScaleValue: 'page-fit' })}>
                                    Fit page
                                </ToggleButton>
                                <ToggleButton className="button button-ghost" isSelected={state.currentScaleValue === 'page-width'} onPress={() => PdfViewer.assign(state, { currentScaleValue: 'page-width' })}>
                                    Fit width
                                </ToggleButton>
                            </>
                        }
                        href={`${url}#page=${page.number}`}
                        zoomIn={() => state.pdf.increaseScale()}
                        zoomOut={() => state.pdf.decreaseScale()}
                    >
                        <div className="flex w-fit max-w-full min-w-0 items-center">
                            <span className="sr-only" role="status">
                                {sheetLabel(page, pages.length)}
                            </span>
                            <Button aria-label="Previous sheet" className="button button-ghost min-w-11 px-2" isDisabled={page.number === 1} onPress={() => state.pdf.previousPage()}>
                                <ChevronLeft className="size-4" />
                            </Button>
                            <select aria-label="Sheet" className="min-h-11 max-w-48 min-w-0 bg-transparent px-1 font-mono text-xs tabular-nums" onChange={(event) => PdfViewer.assign(state, { currentPageNumber: Number(event.currentTarget.value) })} value={page.number}>
                                {pages.map(({ label, number }) => (
                                    <option key={number} value={number}>
                                        {[`${format.numeral(number)} / ${format.numeral(pages.length)}`, label].filter(Boolean).join(' · ')}
                                    </option>
                                ))}
                            </select>
                            <Button aria-label="Next sheet" className="button button-ghost min-w-11 px-2" isDisabled={page.number === pages.length} onPress={() => state.pdf.nextPage()}>
                                <ChevronRight className="size-4" />
                            </Button>
                        </div>
                        <details className="ml-auto min-w-0 open:basis-full">
                            <summary className="min-h-11 cursor-pointer content-center text-sm">Tools</summary>
                            <Toolbar aria-label="View options" className="viewer-toolbar *:px-3">
                                <ToggleButton className="button button-ghost" isSelected={pan} onChange={setPan}>
                                    Pan
                                </ToggleButton>
                                <Button className="button button-ghost" onPress={() => PdfViewer.assign(state, { pagesRotation: state.pagesRotation + quarterTurn })}>
                                    <RotateCw className="size-4" />
                                    Rotate view
                                </Button>
                            </Toolbar>
                            <p className="hint py-2">Select text to copy. Drag with Pan on or press arrow keys on focused sheet to move</p>
                        </details>
                        <details className="min-w-0 open:basis-full" onToggle={(event) => (event.currentTarget.open ? PdfViewer.find(state, query, 'highlightallchange') : state.pdf.eventBus.dispatch('findbarclose', { source: state.pdf }))}>
                            <summary className="min-h-11 cursor-pointer content-center text-sm">Find</summary>
                            <div className="flex flex-wrap items-center gap-2 pb-1">
                                <SearchField
                                    aria-label="Find in document"
                                    className="border-control-line flex min-w-0 flex-1 basis-40 items-center border"
                                    onChange={(text) => {
                                        setQuery(text);
                                        PdfViewer.find(state, text, '');
                                    }}
                                    onSubmit={() => PdfViewer.find(state, query, 'again')}
                                    value={query}
                                >
                                    <Input className="min-h-11 w-full min-w-0 bg-transparent px-3 text-base" />
                                    <Button className="button button-ghost px-3" isDisabled={query === ''}>
                                        Clear
                                    </Button>
                                </SearchField>
                                <Button aria-label="Previous match" className="button button-ghost min-w-11 px-3" isDisabled={state.matchesCount.total === 0} onPress={() => PdfViewer.find(state, query, 'again', { findPrevious: true })}>
                                    <ChevronLeft className="size-4" />
                                </Button>
                                <Button aria-label="Next match" className="button button-ghost min-w-11 px-3" isDisabled={state.matchesCount.total === 0} onPress={() => PdfViewer.find(state, query, 'again')}>
                                    <ChevronRight className="size-4" />
                                </Button>
                                <span className="text-muted basis-full text-xs" role="status">
                                    {Match.value({ query, ...state.matchesCount }).pipe(
                                        Match.when({ query: '' }, () => 'Search drawing numbers or text'),
                                        Match.when({ total: 0 }, ({ total }) => format.quantity(total, matches)),
                                        Match.orElse(({ current, total }) => `${current} of ${format.quantity(total, matches)}`),
                                    )}
                                </span>
                            </div>
                        </details>
                    </ZoomControls>
                ))
                .onFailure(() => <MediaFailure href={url} linkLabel="Open original PDF" message="PDF viewer failed to load" />)
                .orNull()}
            <motion.div aria-label={sheetLabel(page, pages.length)} className="viewer-stage relative" layoutId={layoutId} onDoubleClick={() => pan && Option.map(live, ({ pdf }) => pdf.increaseScale())} onPan={(_event, info) => pan && Option.map(live, ({ pdf }) => pdf.panBy(info.delta.x, info.delta.y))} role="region">
                {/* biome-ignore lint/a11y/noNoninteractiveTabindex: Safari skips scroll containers in focus order, arrow-key panning needs focusable PDF.js container */}
                <div className="sheet-scroller" data-pan={pan || undefined} ref={ref} tabIndex={0}>
                    <div className="pdfViewer" />
                </div>
            </motion.div>
        </>
    );
};
const SheetPreview = ({ document, page, url }: { document: PDFDocumentProxy; page: number; url: string }) => {
    const { ref, viewer } = usePdfViewer(document, { ...sheet(page), annotationMode: AnnotationMode.ENABLE, textLayerMode: 0 });
    return (
        <>
            <div className="pointer-events-none absolute inset-0 overflow-hidden" ref={ref}>
                <div className="pdfViewer" />
            </div>
            {AsyncResult.builder(viewer)
                .onFailure(() => <MediaFailure href={url} linkLabel="Open original PDF" message="PDF viewer failed to load" />)
                .orNull()}
        </>
    );
};
const PdfSheet = ({ placement, layoutId }: { placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string | undefined }) => {
    const url = mediaUrl(placement.asset);
    const source = pdfDocument({ url, disableStream: true, disableAutoFetch: true });
    const retry = useAtomRefresh(source);
    return AsyncResult.builder(useAtomValue(source))
        .onInitialOrWaiting(() => <MediaLoading />)
        .onSuccess((document) => (layoutId === undefined ? <SheetPreview document={document} page={placement.page.number} url={url} /> : <SheetViewer document={document} layoutId={layoutId} placement={placement} url={url} />))
        .onErrorTag('InspectError', (error) => <MediaFailure href={url} linkLabel="Open original PDF" message={error.message} />)
        .onFailure(() => <MediaFailure href={url} linkLabel="Open original PDF" message="PDF failed to load" onRetry={retry} />)
        .exhaustive();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
