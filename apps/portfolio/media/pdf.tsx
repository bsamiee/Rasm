import type { OpenInput } from '@embedpdf/engine';
import { interactionPlugin, useTool } from '@embedpdf/react/interaction';
import { LinkLayer, linkPlugin } from '@embedpdf/react/link';
import { RenderLayer, renderPlugin } from '@embedpdf/react/render';
import { useDocumentStatus, Viewer } from '@embedpdf/react/runtime';
import { SelectionClipboard, SelectionHandles, SelectionLayer, selectionPlugin } from '@embedpdf/react/selection';
import { Stage, stagePlugin, usePageList, usePages, useStage, useZoom } from '@embedpdf/react/stage';
import { Effect, Match, Struct } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { motion } from 'motion/react';
import { type ReactElement, type ReactNode, useId, useLayoutEffect, useState } from 'react';
import { Button, ToggleButton } from 'react-aria-components';
import type { Placement } from '../model/placement.ts';
import { ZoomControls } from './controls.tsx';
import { mediaUrl, placementAlt } from './display.ts';
import { pdfEngine } from './inspect.ts';
import { MediaFailure, MediaLoading } from './status.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const pdfPlugins = [stagePlugin({ flow: 'paged', zoom: { mode: 'fit-page' }, padding: 0, responsive: [] }), renderPlugin(), interactionPlugin(), selectionPlugin(), linkPlugin()];
function SheetStage({ page, interactive }: { page: number; interactive: boolean }): ReactElement {
    const stage = useStage();
    useLayoutEffect(() => stage.goToPage(page - 1, { behavior: 'instant' }), [stage, page]);
    return (
        <Stage className={interactive ? 'size-full' : 'pointer-events-none size-full'} interaction={interactive} overlay={interactive && <SelectionHandles />} zoomGestures={interactive}>
            {(): ReactElement => (
                <>
                    <RenderLayer />
                    {interactive && (
                        <>
                            <SelectionLayer />
                            <LinkLayer />
                        </>
                    )}
                </>
            )}
        </Stage>
    );
}
function SheetViewer({ page, href, layoutId }: { page: number; href: string; layoutId: string }): ReactElement {
    const { zoomIn, zoomOut, fitPage } = useZoom();
    const { activeToolId, activate } = useTool();
    const { currentPage, pageCount, prev, next } = usePages();
    const { pages } = usePageList();
    const label = pages[currentPage]?.label;
    return (
        <>
            <ZoomControls fit={fitPage} href={`${href}#page=${currentPage + 1}`} linkLabel="Original PDF" zoomIn={zoomIn} zoomOut={zoomOut}>
                <ToggleButton className="button button-outline" isSelected={activeToolId === 'pan'} onChange={(pan): void => activate(pan ? 'pan' : 'pointer')}>
                    Pan
                </ToggleButton>
                <div className="flex items-center gap-2 [&>button]:min-w-11">
                    <Button aria-label="Previous sheet" className="button button-outline" isDisabled={currentPage === 0} onPress={(): void => prev({ behavior: 'instant' })}>
                        <ChevronLeft className="size-4" strokeLinecap="butt" />
                    </Button>
                    <span aria-live="polite" className="text-muted text-xs">
                        Sheet {currentPage + 1} of {pageCount}
                        {label ? ` · ${label}` : ''}
                    </span>
                    <Button aria-label="Next sheet" className="button button-outline" isDisabled={currentPage + 1 === pageCount} onPress={(): void => next({ behavior: 'instant' })}>
                        <ChevronRight className="size-4" strokeLinecap="butt" />
                    </Button>
                </div>
            </ZoomControls>
            <motion.div className="relative min-h-0 flex-1 overflow-hidden bg-surface landscape-short:col-span-full landscape-short:row-start-2" layoutId={layoutId}>
                <SheetStage interactive={true} page={page} />
                <SelectionClipboard />
            </motion.div>
        </>
    );
}
function SheetDocument({ page, href, layoutId, failure }: { page: number; href: string; layoutId: string | undefined; failure: ReactElement }): ReactNode {
    return Match.value(useDocumentStatus()).pipe(
        Match.when('ready', (): ReactElement => (layoutId === undefined ? <SheetStage interactive={false} page={page} /> : <SheetViewer href={href} layoutId={layoutId} page={page} />)),
        Match.when(Match.is('error', 'locked'), (): ReactElement => failure),
        Match.orElse((): ReactElement => <MediaLoading />),
    );
}
function PdfSheet({ placement, layoutId }: { placement: Extract<typeof Placement.Type, { kind: 'pdf' }>; layoutId?: string }): ReactElement {
    const [attempt, setAttempt] = useState(0);
    const url = mediaUrl(placement);
    const id = `${useId()}${url}#${attempt}`;
    const href = `${url}#page=${placement.page.number}`;
    const failure = <MediaFailure className="absolute inset-0 justify-center bg-surface" href={href} linkLabel="Open original PDF" message="This document could not be displayed." onRetry={(): void => setAttempt((value) => value + 1)} />;
    return (
        // biome-ignore lint/a11y/useSemanticElements: Fieldset groups form controls, and the sheet canvas with its viewer controls is no form
        <div aria-label={placementAlt(placement)} className={layoutId === undefined ? 'relative size-full' : 'contents'} role="group">
            <Viewer
                engine={pdfEngine}
                initialDocuments={[
                    {
                        scope: ['*'],
                        source: (signal): Promise<OpenInput> =>
                            Effect.runPromise(
                                HttpClient.get(url).pipe(
                                    Effect.flatMap(HttpClientResponse.filterStatusOk),
                                    Effect.flatMap(Struct.get('arrayBuffer')),
                                    Effect.map((baseBytes): OpenInput => ({ kind: 'layerBytes', id, baseKey: url, baseBytes })),
                                    Effect.provide(FetchHttpClient.layer),
                                ),
                                { signal },
                            ),
                    },
                ]}
                key={id}
                plugins={pdfPlugins}
                renderError={(): ReactElement => failure}
            >
                <SheetDocument failure={failure} href={url} layoutId={layoutId} page={placement.page.number} />
            </Viewer>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PdfSheet };
