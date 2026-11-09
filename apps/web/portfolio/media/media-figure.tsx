import { Backdrop, CustomProperties } from '@rasm/ui';
import { Expand, X } from 'lucide-react';
import { motion } from 'motion/react';
import { type ComponentProps, lazy, Suspense, useId, useRef } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, Text } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { descriptionLabel, format, placementTitle } from '#/media/labels';
import { MediaElement } from '#/media/media-element.tsx';
import { MediaFailure, MediaLoading } from '#/media/status.tsx';
import { Placement } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const AnimatedDialog = motion.create(Dialog);
const ZoomViewer = lazy(() => import('#/media/zoom-viewer.tsx').then((module) => ({ default: module.ZoomViewer })));
const MediaFigure = ({ placement, active, presentation, priority, renderMedia }: { placement: typeof Placement.Type; active: boolean; presentation: Exclude<ComponentProps<typeof MediaElement>['presentation'], 'expanded'>; priority: boolean; renderMedia: boolean }) => {
    const layoutId = useId();
    const figure = useRef<HTMLElement>(null);
    const viewer = presentation === 'expandable' && placement.kind !== 'video' ? placement : undefined;
    const description = Boolean(placement.description) && (
        <details className="hint landscape-short:col-span-full landscape-short:row-start-4">
            <summary className="list-item min-h-11 cursor-pointer content-center">{descriptionLabel(placement)}</summary>
            <p className="max-h-quarter-screen overflow-auto whitespace-pre-line">{placement.description}</p>
        </details>
    );
    return (
        <CustomProperties properties={{ '--media-ratio': Placement.ratio(placement), ...(placement.kind === 'image' && placement.role === 'photograph' && placement.framing.mode === 'cover' && { '--media-focus-x': `${placement.framing.x}%`, '--media-focus-y': `${placement.framing.y}%` }) }}>
            {(scope) => (
                <figure aria-hidden={presentation === 'thumbnail'} className="media-figure" ref={figure} {...scope}>
                    <motion.div {...(viewer && { layoutId })} className="bg-background-muted relative flex aspect-(--media-ratio) w-full items-center justify-center overflow-hidden">
                        {renderMedia && <MediaElement active={active} key={placement.asset.id} placement={placement} presentation={presentation} priority={priority} viewport={figure} />}
                    </motion.div>
                    {presentation !== 'thumbnail' && (
                        <>
                            <figcaption className="grid-cols-trailing text-muted leading-body grid items-start gap-x-3 pt-2 text-sm wrap-anywhere empty:hidden">
                                {Boolean(placement.caption) && <span className="col-start-1 block min-w-0 whitespace-pre-line">{placement.caption}</span>}
                                {placement.kind === 'pdf' && (
                                    <span className="col-start-1 flex min-w-0 flex-wrap items-baseline gap-x-4 gap-y-1">
                                        <span>{placement.asset.name}</span>
                                        <span className="inline-flex shrink-0 gap-1.5 font-mono text-xs tabular-nums">
                                            <span className="sr-only">Sheet </span>
                                            <span className="text-accent-text">{format.numeral(placement.page.number, placement.asset.pages.length)}</span>
                                            <span aria-hidden="true">/</span>
                                            <span className="sr-only"> of </span>
                                            <span>{format.numeral(placement.asset.pages.length)}</span>
                                        </span>
                                        <span className="empty:hidden">{placement.page.label}</span>
                                    </span>
                                )}
                                {viewer && (
                                    <DialogTrigger>
                                        <Button aria-label={`Expand ${placementTitle(viewer)}`} className="text-foreground hover:text-accent-text col-start-2 row-start-1 grid size-11 place-items-center">
                                            <Expand className="size-4.5" />
                                        </Button>
                                        <Backdrop className="bg-foreground/70 motion-safe:entering:animate-fade motion-safe:exiting:animate-fade-out fixed inset-0 z-50" isDismissable={true}>
                                            <Modal>
                                                <AnimatedDialog className="viewer-dialog" layoutRoot={true}>
                                                    <div className="landscape-short:contents flex shrink-0 items-start justify-between gap-4">
                                                        <Heading className="viewer-heading" slot="title">
                                                            {viewer.kind === 'pdf' ? viewer.asset.name : placementTitle(viewer)}
                                                        </Heading>
                                                        <Button aria-label="Close viewer" className="landscape-short:col-start-2 landscape-short:row-start-1 grid min-h-11 min-w-11 shrink-0 place-items-center" slot="close">
                                                            <X className="size-6" />
                                                        </Button>
                                                    </div>
                                                    {viewer.kind === 'image' && (
                                                        <Text className="viewer-hint" elementType="p" slot="description">
                                                            Scroll, pinch, or press + and − to zoom. Drag or press arrow keys to move
                                                        </Text>
                                                    )}
                                                    <ErrorBoundary fallback={<MediaFailure asset={viewer.asset} message="Viewer failed to load" />}>
                                                        <Suspense fallback={<MediaLoading />}>
                                                            <ZoomViewer layoutId={layoutId} placement={viewer} />
                                                        </Suspense>
                                                    </ErrorBoundary>
                                                    {viewer.kind === 'image' && description}
                                                </AnimatedDialog>
                                            </Modal>
                                        </Backdrop>
                                    </DialogTrigger>
                                )}
                            </figcaption>
                            {viewer?.kind !== 'image' && description}
                        </>
                    )}
                </figure>
            )}
        </CustomProperties>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFigure };
