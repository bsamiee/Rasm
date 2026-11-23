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
import '#/media/pdf-sheet.css';
import { MediaFailure, MediaLoading } from '#/media/status.tsx';
import { assetUrl } from '#/model/api';
import type { PdfAsset, Sheet } from '#/model/asset';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sheet = (currentPageNumber: number) => ({ removePageBorders: true, annotationEditorMode: AnnotationEditorType.DISABLE, view: { currentPageNumber, currentScaleValue: 'page-fit', scrollMode: ScrollMode.PAGE, spreadMode: SpreadMode.NONE } }) satisfies Parameters<typeof PdfViewer.useViewer>[1];
const pdfFailure = (retry: () => void) =>
    Match.type<MediaError['reason']>().pipe(
        Match.tagsExhaustive({
            Protected: () => ({ message: 'PDF is password protected' }),
            Missing: () => ({ message: 'PDF not found' }),
            Unreadable: () => ({ message: 'Browser cannot read PDF' }),
            Unsupported: () => ({ message: 'Browser cannot read PDF' }),
            Unavailable: () => ({ message: 'PDF failed to load', onRetry: retry }),
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const RenderFailure = ({ viewer, page, asset }: { readonly viewer: Option.Option<PdfViewer>; readonly page: number; readonly asset: PdfAsset }) => Option.exists(viewer, ({ failedPages }) => HashSet.has(failedPages, page)) && <MediaFailure asset={asset} message="Sheet failed to render" />;
const SheetViewer = ({ document, asset, page: placed, layoutId, url }: { readonly document: PDFDocumentProxy; readonly asset: PdfAsset; readonly page: Sheet; readonly layoutId: string; readonly url: string }) => {
    const pages = asset.sheets;
    const viewer = PdfViewer.useViewer(document, { ...sheet(placed.number), link: { externalLinkTarget: LinkTarget.BLANK }, pinch: { drawingDelay: '400 millis' } });
    const live = AsyncResult.value(viewer.state);
    const page = Option.flatMap(live, ({ currentPageNumber }) => Array.get(pages, currentPageNumber - 1)).pipe(Option.getOrElse(() => placed));
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
                .onFailure(() => <MediaFailure asset={asset} message="PDF viewer failed to load" />)
                .orNull()}
            <RenderFailure asset={asset} page={page.number} viewer={live} />
            <motion.section aria-label={sheetLabel(page, pages.length)} className="viewer-stage" layoutId={layoutId}>
                {viewer.container({ className: 'sheet-scroller', tabIndex: 0 })}
            </motion.section>
        </>
    );
};
const SheetPreview = ({ document, page, asset }: { readonly document: PDFDocumentProxy; readonly page: number; readonly asset: PdfAsset }) => {
    const viewer = PdfViewer.useViewer(document, { ...sheet(page), annotationMode: AnnotationMode.ENABLE, textLayerMode: PdfViewer.TextLayerMode.DISABLE });
    return (
        <>
            {viewer.container({ className: 'sheet-preview', inert: true })}
            {AsyncResult.builder(viewer.state)
                .onFailure(() => <MediaFailure asset={asset} message="PDF viewer failed to load" />)
                .orNull()}
            <RenderFailure asset={asset} page={page} viewer={AsyncResult.value(viewer.state)} />
        </>
    );
};
const PdfSheet = ({ asset, page, layoutId }: { readonly asset: PdfAsset; readonly page: Sheet; readonly layoutId: string | undefined }) => {
    const url = assetUrl(asset.id);
    const source = PdfDocument({ url, disableStream: true, disableAutoFetch: true });
    const retry = useAtomRefresh(source);
    return AsyncResult.builder(useAtomValue(source))
        .onInitialOrWaiting(() => <MediaLoading />)
        .onSuccess((document) => (layoutId === undefined ? <SheetPreview asset={asset} document={document} page={page.number} /> : <SheetViewer asset={asset} document={document} layoutId={layoutId} page={page} url={url} />))
        .onErrorTag('MediaError', ({ reason }) => <MediaFailure asset={asset} {...pdfFailure(retry)(reason)} />)
        .onFailure(() => <MediaFailure asset={asset} message="PDF failed to load" onRetry={retry} />)
        .exhaustive();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
