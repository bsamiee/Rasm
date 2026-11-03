import { useAtomRefresh, useAtomValue } from '@effect/atom-react';
import { pdfDocument } from '@rasm/media/pdf/document';
import { PdfViewer } from '@rasm/media/pdf/viewer';
import { Array, HashSet, Match, Option } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { ChevronLeft, ChevronRight, RotateCw } from 'lucide-react';
import { motion } from 'motion/react';
import { AnnotationEditorType, AnnotationMode, type PDFDocumentProxy } from 'pdfjs-dist';
import { LinkTarget, ScrollMode, SpreadMode } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { useState } from 'react';
import { Button, Input, SearchField, ToggleButton, Toolbar } from 'react-aria-components';
import { details, format, sheetLabel } from '#/media/labels.ts';
import { MediaFailure, MediaLoading } from '#/media/status.tsx';
import { ZoomControls } from '#/media/zoom-controls.tsx';
import { assetUrl } from '#/model/api.ts';
import type { PdfAsset } from '#/model/asset.ts';
import type { Placement } from '#/model/placement.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sheet = (currentPageNumber: number) => ({ removePageBorders: true, annotationEditorMode: AnnotationEditorType.DISABLE, view: { currentPageNumber, currentScaleValue: 'page-fit', scrollMode: ScrollMode.PAGE, spreadMode: SpreadMode.NONE } }) satisfies Parameters<typeof PdfViewer.useViewer>[1];

// --- [COMPOSITION] ---------------------------------------------------------------------

const RenderFailure = ({ viewer, page, asset }: { viewer: Option.Option<PdfViewer>; page: number; asset: PdfAsset }) => Option.exists(viewer, ({ failedPages }) => HashSet.has(failedPages, page)) && <MediaFailure asset={asset} message="Sheet failed to render" />;
const SheetViewer = ({ document, placement, layoutId, url }: { document: PDFDocumentProxy; placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string; url: string }) => {
    const [query, setQuery] = useState('');
    const [pan, setPan] = useState(false);
    const matches = { one: (count: string) => `${count} match`, other: (count: string) => `${count} matches` };
    const pages = placement.asset.sheets;
    const viewer = PdfViewer.useViewer(document, { ...sheet(placement.page.number), link: { externalLinkTarget: LinkTarget.BLANK }, pinch: { drawingDelay: '400 millis' } });
    const live = AsyncResult.value(viewer.state);
    const page = Option.flatMap(live, ({ currentPageNumber }) => Array.get(pages, currentPageNumber - 1)).pipe(Option.getOrElse(() => placement.page));
    return (
        <>
            {AsyncResult.builder(viewer.state)
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
                        zoomIn={() => state.pdfViewer.increaseScale()}
                        zoomOut={() => state.pdfViewer.decreaseScale()}
                    >
                        <div className="flex w-fit max-w-full min-w-0 items-center">
                            <span className="sr-only" role="status">
                                {sheetLabel(page, pages.length)}
                            </span>
                            <Button aria-label="Previous sheet" className="button button-ghost min-w-11 px-2" isDisabled={page.number === 1} onPress={() => state.pdfViewer.previousPage()}>
                                <ChevronLeft className="size-4" />
                            </Button>
                            <select aria-label="Sheet" className="min-h-11 max-w-48 min-w-0 bg-transparent px-1 font-mono text-xs tabular-nums" onChange={(event) => PdfViewer.assign(state, { currentPageNumber: Number(event.currentTarget.value) })} value={page.number}>
                                {pages.map(({ label, number }) => (
                                    <option key={number} value={number}>
                                        {details([`${format.numeral(number, pages.length)} / ${format.numeral(pages.length)}`, label])}
                                    </option>
                                ))}
                            </select>
                            <Button aria-label="Next sheet" className="button button-ghost min-w-11 px-2" isDisabled={page.number === pages.length} onPress={() => state.pdfViewer.nextPage()}>
                                <ChevronRight className="size-4" />
                            </Button>
                        </div>
                        <details className="ml-auto min-w-0 open:basis-full">
                            <summary className="min-h-11 cursor-pointer content-center text-sm">Tools</summary>
                            <Toolbar aria-label="View options" className="viewer-toolbar *:px-3">
                                <ToggleButton className="button button-ghost" isSelected={pan} onChange={setPan}>
                                    Pan
                                </ToggleButton>
                                <Button className="button button-ghost" onPress={() => PdfViewer.rotate(state, 1)}>
                                    <RotateCw className="size-4" />
                                    Rotate view
                                </Button>
                            </Toolbar>
                            <p className="hint py-2">Select text to copy. Drag with Pan on or press arrow keys on focused sheet to move</p>
                        </details>
                        <details className="min-w-0 open:basis-full" onToggle={(event) => (event.currentTarget.open ? PdfViewer.find(state, query, 'highlightallchange') : PdfViewer.closeFind(state))}>
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
                .onFailure(() => <MediaFailure asset={placement.asset} message="PDF viewer failed to load" />)
                .orNull()}
            <RenderFailure asset={placement.asset} page={page.number} viewer={live} />
            <motion.div
                aria-label={sheetLabel(page, pages.length)}
                className="viewer-stage relative"
                layoutId={layoutId}
                onDoubleClick={() => pan && Option.map(live, ({ pdfViewer }) => pdfViewer.increaseScale())}
                onPan={(_event, info) => pan && Option.map(live, ({ pdfViewer }) => pdfViewer.panBy(info.delta.x, info.delta.y))}
                role="region"
            >
                {viewer.container({ className: 'sheet-scroller', 'data-pan': pan || undefined, tabIndex: 0 })}
            </motion.div>
        </>
    );
};
const SheetPreview = ({ document, page, asset }: { document: PDFDocumentProxy; page: number; asset: PdfAsset }) => {
    const viewer = PdfViewer.useViewer(document, { ...sheet(page), annotationMode: AnnotationMode.ENABLE, textLayerMode: PdfViewer.TextLayerMode.DISABLE });
    return (
        <>
            {viewer.container({ className: 'pointer-events-none absolute inset-0 overflow-hidden' })}
            {AsyncResult.builder(viewer.state)
                .onFailure(() => <MediaFailure asset={asset} message="PDF viewer failed to load" />)
                .orNull()}
            <RenderFailure asset={asset} page={page} viewer={AsyncResult.value(viewer.state)} />
        </>
    );
};
const PdfSheet = ({ placement, layoutId }: { placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string | undefined }) => {
    const url = assetUrl(placement.asset);
    const source = pdfDocument({ url, disableStream: true, disableAutoFetch: true });
    const retry = useAtomRefresh(source);
    return AsyncResult.builder(useAtomValue(source))
        .onInitialOrWaiting(() => <MediaLoading />)
        .onSuccess((document) => (layoutId === undefined ? <SheetPreview asset={placement.asset} document={document} page={placement.page.number} /> : <SheetViewer document={document} layoutId={layoutId} placement={placement} url={url} />))
        .onErrorTag('MediaError', ({ reason }) => (
            <MediaFailure
                asset={placement.asset}
                message={Match.value(reason).pipe(
                    Match.tag('Protected', () => 'PDF is password protected'),
                    Match.tag('Missing', () => 'PDF not found'),
                    Match.orElse(() => 'Browser cannot read PDF'),
                )}
            />
        ))
        .onFailure(() => <MediaFailure asset={placement.asset} message="PDF failed to load" onRetry={retry} />)
        .exhaustive();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
