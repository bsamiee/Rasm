import { useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { pdfDocument } from '@rasm/media/pdf/document.ts';
import { usePdfPage } from '@rasm/media/pdf/page.ts';
import { usePdfViewer } from '@rasm/media/pdf/viewer.ts';
import { AsyncResult } from 'effect/reactivity';
import { ChevronLeft, ChevronRight, RotateCw } from 'lucide-react';
import { motion } from 'motion/react';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { LinkTarget, ScrollMode, SpreadMode } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useState } from 'react';
import { Button, Input, SearchField, ToggleButton, Toolbar } from 'react-aria-components';
import { mediaUrl, sheets } from '../model/asset.ts';
import type { Placement } from '../model/placement.ts';
import { numeral, sheetLabel } from './labels.ts';
import { MediaFailure, MediaLoading } from './status.tsx';
import { ZoomControls } from './zoom-controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const SheetViewer = ({ document, placement, layoutId }: { document: PDFDocumentProxy; placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string }) => {
    const [query, setQuery] = useState('');
    const [pan, setPan] = useState(false);
    const quarterTurn = 90;
    const pages = sheets(placement.asset);
    const { ref, viewer } = usePdfViewer(document, { page: placement.page.number, scale: 'page-fit', linkTarget: LinkTarget.BLANK, scrollMode: ScrollMode.PAGE, spreadMode: SpreadMode.NONE, removePageBorders: true });
    const current = pages[(viewer?.page ?? placement.page.number) - 1] ?? placement.page;
    return (
        <>
            {viewer && (
                <ZoomControls
                    fit={
                        <>
                            <ToggleButton className="button button-ghost" isSelected={viewer.scale === 'page-fit'} onPress={() => viewer.fit('page-fit')}>
                                Fit page
                            </ToggleButton>
                            <ToggleButton className="button button-ghost" isSelected={viewer.scale === 'page-width'} onPress={() => viewer.fit('page-width')}>
                                Fit width
                            </ToggleButton>
                        </>
                    }
                    href={`${mediaUrl(placement.asset)}#page=${current.number}`}
                    zoomIn={() => viewer.pdf.increaseScale()}
                    zoomOut={() => viewer.pdf.decreaseScale()}
                >
                    <div className="flex w-fit max-w-full min-w-0 items-center">
                        <span className="sr-only" role="status">
                            {sheetLabel(current, document.numPages)}
                        </span>
                        <Button aria-label="Previous sheet" className="button button-ghost min-w-11 px-2" isDisabled={current.number === 1} onPress={() => viewer.pdf.previousPage()}>
                            <ChevronLeft className="size-4" />
                        </Button>
                        <select aria-label="Sheet" className="min-h-11 max-w-48 min-w-0 bg-transparent px-1 font-mono text-xs tabular-nums" onChange={(event) => viewer.pdf.linkService.goToPage(event.currentTarget.selectedIndex + 1)} value={current.number}>
                            {pages.map(({ label, number }) => (
                                <option key={number} value={number}>
                                    {[`${numeral(number)} / ${numeral(document.numPages)}`, label].filter(Boolean).join(' · ')}
                                </option>
                            ))}
                        </select>
                        <Button aria-label="Next sheet" className="button button-ghost min-w-11 px-2" isDisabled={current.number === document.numPages} onPress={() => viewer.pdf.nextPage()}>
                            <ChevronRight className="size-4" />
                        </Button>
                    </div>
                    <details className="ml-auto min-w-0 open:basis-full">
                        <summary className="min-h-11 cursor-pointer content-center text-sm">Tools</summary>
                        <Toolbar aria-label="View options" className="viewer-toolbar *:px-3">
                            <ToggleButton className="button button-ghost" isSelected={pan} onChange={setPan}>
                                Pan
                            </ToggleButton>
                            <Button className="button button-ghost" onPress={() => viewer.rotate(quarterTurn)}>
                                <RotateCw className="size-4" />
                                Rotate view
                            </Button>
                        </Toolbar>
                        <p className="hint py-2">Select text to copy. Use Pan or focus the drawing and press arrow keys to move.</p>
                    </details>
                    <details className="min-w-0 open:basis-full" onToggle={(event) => event.currentTarget.open || viewer.pdf.eventBus.dispatch('findbarclose', {})}>
                        <summary className="min-h-11 cursor-pointer content-center text-sm">Find</summary>
                        <div className="flex flex-wrap items-center gap-2 pb-1">
                            <SearchField
                                aria-label="Find in document"
                                className="border-control-line flex min-w-0 flex-1 basis-40 items-center border"
                                onChange={(text) => {
                                    setQuery(text);
                                    viewer.find(text, 'first');
                                }}
                                onSubmit={() => viewer.find(query, 'next')}
                                value={query}
                            >
                                <Input className="min-h-11 w-full min-w-0 bg-transparent px-3 text-base" />
                                <Button className="button button-ghost px-3" isDisabled={query === ''}>
                                    Clear
                                </Button>
                            </SearchField>
                            <Button aria-label="Previous match" className="button button-ghost min-w-11 px-3" isDisabled={viewer.matches.total === 0} onPress={() => viewer.find(query, 'previous')}>
                                <ChevronLeft className="size-4" />
                            </Button>
                            <Button aria-label="Next match" className="button button-ghost min-w-11 px-3" isDisabled={viewer.matches.total === 0} onPress={() => viewer.find(query, 'next')}>
                                <ChevronRight className="size-4" />
                            </Button>
                            <span className="text-muted basis-full text-xs" role="status">
                                {query === '' ? 'Search drawing numbers or text.' : `${viewer.matches.current} of ${viewer.matches.total} matches`}
                            </span>
                        </div>
                    </details>
                </ZoomControls>
            )}
            <motion.div aria-label={sheetLabel(current, document.numPages)} className="viewer-stage relative" layoutId={layoutId} onDoubleClick={() => pan && viewer?.pdf.increaseScale()} onPan={(_event, info) => pan && viewer?.pdf.panBy(info.delta.x, info.delta.y)} role="region">
                {/* biome-ignore lint/a11y/noNoninteractiveTabindex: Safari leaves scroll containers out of keyboard focus, arrow-key panning needs a focusable PDF.js container */}
                <div className="sheet-scroller" data-pan={pan || undefined} ref={ref} tabIndex={0}>
                    <div className="pdfViewer" />
                </div>
            </motion.div>
        </>
    );
};
const SheetPreview = ({ document, page }: { document: PDFDocumentProxy; page: number }) => {
    const ref = usePdfPage(document, page);
    return <div className="pdfViewer singlePageView pointer-events-none size-full overflow-hidden" ref={ref} />;
};
const PdfSheet = ({ placement, layoutId }: { placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string | undefined }) => {
    const url = mediaUrl(placement.asset);
    const retry = useAtomRefresh(pdfDocument(url));
    return AsyncResult.builder(useAtomValue(pdfDocument(url)))
        .onInitial(() => <MediaLoading />)
        .onError(() => <MediaFailure href={url} linkLabel="Open original PDF" message="This document could not be displayed." onRetry={retry} />)
        .onSuccess((document) => (layoutId === undefined ? <SheetPreview document={document} page={placement.page.number} /> : <SheetViewer document={document} layoutId={layoutId} placement={placement} />))
        .render();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
