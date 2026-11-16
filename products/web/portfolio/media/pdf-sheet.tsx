import { useAtomRefresh, useAtomValue } from '@effect/atom-react';
import type { MediaError } from '@rasm/media';
import { PdfDocument } from '@rasm/media/pdf-document';
import { PdfViewer } from '@rasm/media/pdf-viewer';
import { Array, HashSet, Match, Option } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { motion } from 'motion/react';
import { AnnotationEditorType, AnnotationMode, type PDFDocumentProxy } from 'pdfjs-dist';
import { LinkTarget, ScrollMode, SpreadMode } from 'pdfjs-dist/web/pdf_viewer.mjs';
import { sheetLabel } from '#/media/labels';
import { MediaFailure, MediaLoading } from '#/media/status.tsx';
import { assetUrl } from '#/model/api';
import type { PdfAsset } from '#/model/asset';
import type { Placement } from '#/model/placement';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sheet = (currentPageNumber: number) => ({ removePageBorders: true, annotationEditorMode: AnnotationEditorType.DISABLE, view: { currentPageNumber, currentScaleValue: 'page-fit', scrollMode: ScrollMode.PAGE, spreadMode: SpreadMode.NONE } }) satisfies Parameters<typeof PdfViewer.useViewer>[1];
const pdfFailure = Match.type<MediaError['reason']>().pipe(Match.tagsExhaustive({ Protected: () => 'PDF is password protected', Missing: () => 'PDF not found', Unreadable: () => 'Browser cannot read PDF', Unsupported: () => 'Browser cannot read PDF' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const RenderFailure = ({ viewer, page, asset }: { viewer: Option.Option<PdfViewer>; page: number; asset: PdfAsset }) => Option.exists(viewer, ({ failedPages }) => HashSet.has(failedPages, page)) && <MediaFailure asset={asset} message="Sheet failed to render" />;
const SheetViewer = ({ document, placement, layoutId, url }: { document: PDFDocumentProxy; placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId: string; url: string }) => {
    const pages = placement.asset.sheets;
    const viewer = PdfViewer.useViewer(document, { ...sheet(placement.page.number), link: { externalLinkTarget: LinkTarget.BLANK }, pinch: { drawingDelay: '400 millis' } });
    const live = AsyncResult.value(viewer.state);
    const page = Option.flatMap(live, ({ currentPageNumber }) => Array.get(pages, currentPageNumber - 1)).pipe(Option.getOrElse(() => placement.page));
    return (
        <>
            {viewer.controls({
                className: 'viewer-controls',
                href: url,
                page: (number) =>
                    sheetLabel(
                        Option.getOrElse(Array.get(pages, number - 1), () => ({ number })),
                        pages.length,
                    ),
            })}
            {AsyncResult.builder(viewer.state)
                .onFailure(() => <MediaFailure asset={placement.asset} message="PDF viewer failed to load" />)
                .orNull()}
            <RenderFailure asset={placement.asset} page={page.number} viewer={live} />
            <motion.section aria-label={sheetLabel(page, pages.length)} className="relative viewer-stage" layoutId={layoutId}>
                {viewer.container({ className: 'sheet-scroller', tabIndex: 0 })}
            </motion.section>
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
    const source = PdfDocument({ url, disableStream: true, disableAutoFetch: true });
    const retry = useAtomRefresh(source);
    return AsyncResult.builder(useAtomValue(source))
        .onInitialOrWaiting(() => <MediaLoading />)
        .onSuccess((document) => (layoutId === undefined ? <SheetPreview asset={placement.asset} document={document} page={placement.page.number} /> : <SheetViewer document={document} layoutId={layoutId} placement={placement} url={url} />))
        .onErrorTag('MediaError', ({ reason }) => <MediaFailure asset={placement.asset} message={pdfFailure(reason)} />)
        .onFailure(() => <MediaFailure asset={placement.asset} message="PDF failed to load" onRetry={retry} />)
        .exhaustive();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
