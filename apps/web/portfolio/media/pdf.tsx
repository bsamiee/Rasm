import { useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { pdfDocument } from '@rasm/media/pdf.ts';
import { usePdfPage, usePdfViewer } from '@rasm/media/viewer.ts';
import { AsyncResult } from 'effect/reactivity';
import { ChevronLeft, ChevronRight, RotateCw } from 'lucide-react';
import { motion } from 'motion/react';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { LinkTarget } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { type ReactElement, useState } from 'react';
import { Button, Input, SearchField, ToggleButton, Toolbar } from 'react-aria-components';
import { useErrorBoundary } from 'react-error-boundary';
import { sheets } from '../model/asset.ts';
import type { Placement } from '../model/placement.ts';
import { ZoomControls } from './controls.tsx';
import { mediaUrl, numeral } from './display.ts';
import { MediaFailure, MediaLoading } from './status.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function SheetPreview({ document, page }: { document: PDFDocumentProxy; page: number }): ReactElement {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const { showBoundary } = useErrorBoundary();
    usePdfPage(container, document, page, showBoundary);
    return <div className="pdfViewer singlePageView pointer-events-none size-full overflow-hidden" ref={setContainer} />;
}
function SheetViewer({ document, placement, layoutId }: { document: PDFDocumentProxy; placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string }): ReactElement {
    const [container, setContainer] = useState<HTMLDivElement | null>(null);
    const [query, setQuery] = useState('');
    const quarterTurn = 90;
    const [pan, setPan] = useState(false);
    const { showBoundary } = useErrorBoundary();
    const { view, matches, controls } = usePdfViewer(container, document, { page: placement.page.number, labels: sheets(placement.asset).map(({ label: name, number }) => name ?? String(number)), scale: 'page-fit', linkTarget: LinkTarget.BLANK, onError: showBoundary });
    const currentPage = view?.page ?? placement.page.number;
    const label = placement.asset.pages[currentPage - 1]?.label;
    return (
        <>
            {view && (
                <ZoomControls
                    fit={
                        <>
                            <ToggleButton className="button button-ghost" isSelected={view.fit === 'page-fit'} onPress={(): void => controls.scale('page-fit')}>
                                Fit page
                            </ToggleButton>
                            <ToggleButton className="button button-ghost" isSelected={view.fit === 'page-width'} onPress={(): void => controls.scale('page-width')}>
                                Fit width
                            </ToggleButton>
                        </>
                    }
                    href={`${mediaUrl(placement.asset)}#page=${currentPage}`}
                    linkLabel="Original"
                    zoomIn={controls.zoomIn}
                    zoomOut={controls.zoomOut}
                >
                    <div className="flex w-fit min-w-0 max-w-full items-center [&>button]:min-w-11 [&>button]:px-2">
                        <span className="sr-only" role="status">
                            Sheet {currentPage} of {document.numPages}
                            {label !== undefined && label !== '' ? ` · ${label}` : ''}
                        </span>
                        <Button aria-label="Previous sheet" className="button button-ghost" isDisabled={currentPage === 1} onPress={controls.previous}>
                            <ChevronLeft className="size-4" />
                        </Button>
                        <select aria-label="Sheet" className="min-h-11 min-w-0 max-w-48 bg-transparent px-1 font-mono text-xs tabular-nums" onChange={(event): void => controls.page(event.currentTarget.selectedIndex + 1)} value={currentPage}>
                            {sheets(placement.asset).map(({ label: name, number }) => (
                                <option key={number} value={number}>
                                    {numeral(number)} / {numeral(document.numPages)}
                                    {name !== undefined && name !== '' ? ` · ${name}` : ''}
                                </option>
                            ))}
                        </select>
                        <Button aria-label="Next sheet" className="button button-ghost" isDisabled={currentPage === document.numPages} onPress={controls.next}>
                            <ChevronRight className="size-4" />
                        </Button>
                    </div>
                    <details className="ml-auto min-w-0 open:basis-full">
                        <summary className="min-h-11 cursor-pointer content-center text-sm">Tools</summary>
                        <Toolbar aria-label="View options" className="flex w-fit items-center [&>button]:min-w-11 [&>button]:selected:bg-transparent [&>button]:px-3 [&>button]:selected:text-foreground [&>button]:selected:decoration-accent-text">
                            <ToggleButton className="button button-ghost" isSelected={pan} onChange={setPan}>
                                Pan
                            </ToggleButton>
                            <Button className="button button-ghost" onPress={(): void => controls.rotate(quarterTurn)}>
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
                                controls.closeFind();
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
                                    controls.find(text, 'first');
                                }}
                                onSubmit={(): void => controls.find(query, 'next')}
                                value={query}
                            >
                                <Input className="min-h-11 w-full min-w-0 bg-transparent px-3 text-base" />
                                <Button className="button button-ghost px-3" isDisabled={query === ''}>
                                    Clear
                                </Button>
                            </SearchField>
                            <Button aria-label="Previous match" className="button button-ghost min-w-11 px-3" isDisabled={matches.total === 0} onPress={(): void => controls.find(query, 'previous')}>
                                <ChevronLeft className="size-4" />
                            </Button>
                            <Button aria-label="Next match" className="button button-ghost min-w-11 px-3" isDisabled={matches.total === 0} onPress={(): void => controls.find(query, 'next')}>
                                <ChevronRight className="size-4" />
                            </Button>
                            <span className="basis-full text-muted text-xs" role="status">
                                {query === '' ? 'Search drawing numbers or text.' : `${matches.current} of ${matches.total} matches`}
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
                        controls.zoomIn();
                    }
                }}
                onPan={(_event, info): void => {
                    if (pan) {
                        controls.pan(info.delta.x, info.delta.y);
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
            <MediaFailure href={mediaUrl(placement.asset)} linkLabel="Open original PDF" message={error.reason._tag === 'Protected' ? 'This PDF is protected. Upload an unlocked export.' : 'This document could not be displayed.'} {...(error.reason._tag === 'Protected' ? {} : { onRetry: retry })} />
        ),
        onDefect: (cause): ReactElement => {
            throw cause;
        },
        onSuccess: ({ value: document }): ReactElement => (layoutId === undefined ? <SheetPreview document={document} page={placement.page.number} /> : <SheetViewer document={document} layoutId={layoutId} placement={placement} />),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
