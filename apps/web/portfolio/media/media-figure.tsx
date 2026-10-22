import { useAtom } from '@effect/atom-react';
import { CustomProperties } from '@rasm/ui/custom-properties.tsx';
import { dialogOpen } from '@rasm/ui/dialogs.ts';
import { Expand, X } from 'lucide-react';
import { motion } from 'motion/react';
import { type ComponentProps, lazy, type ReactNode, Suspense, useId, useRef } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, ModalOverlay, Text } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { mediaUrl } from '../model/asset.ts';
import { type Placement, placementRatio } from '../model/placement.ts';
import { descriptionLabel, numeral, placementTitle } from './labels.ts';
import { MediaElement } from './media-element.tsx';
import { MediaFailure, MediaLoading } from './status.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const AnimatedDialog = motion.create(Dialog);
const ZoomViewer = lazy(() => import('./zoom-viewer.tsx').then((module) => ({ default: module.ZoomViewer })));
const ExpandedViewer = ({ placement, layoutId, description }: { placement: Exclude<typeof Placement.Type, { kind: 'video' }>; layoutId: string; description: ReactNode }) => (
    <ModalOverlay className="bg-foreground/70 motion-safe:entering:animate-fade motion-safe:exiting:animate-fade-out fixed inset-0 z-50" isDismissable={true}>
        <Modal>
            <AnimatedDialog className="viewer-dialog" layoutRoot={true}>
                <div className="landscape-short:contents flex shrink-0 items-start justify-between gap-4">
                    <Heading className="viewer-heading" slot="title">
                        {placement.kind === 'pdf' ? placement.asset.name : placementTitle(placement)}
                    </Heading>
                    <Button aria-label="Close viewer" className="landscape-short:col-start-2 landscape-short:row-start-1 grid min-h-11 min-w-11 shrink-0 place-items-center" slot="close">
                        <X className="size-6" />
                    </Button>
                </div>
                {placement.kind === 'image' && (
                    <Text className="viewer-hint" elementType="p" slot="description">
                        Scroll or pinch to zoom. Drag or use arrow keys to move. Use + and − to zoom.
                    </Text>
                )}
                <ErrorBoundary fallback={<MediaFailure href={mediaUrl(placement.asset)} linkLabel="Open original" message="The expanded viewer could not be loaded." />}>
                    <Suspense fallback={<MediaLoading />}>
                        <ZoomViewer layoutId={layoutId} placement={placement} />
                    </Suspense>
                </ErrorBoundary>
                {description}
            </AnimatedDialog>
        </Modal>
    </ModalOverlay>
);
const MediaFigure = ({ placement, active, priority, presentation, renderMedia }: { placement: typeof Placement.Type; active: boolean; priority: boolean; presentation: Exclude<ComponentProps<typeof MediaElement>['presentation'], 'expanded'>; renderMedia: boolean }) => {
    const layoutId = useId();
    const [isOpen, setOpen] = useAtom(dialogOpen(layoutId));
    const figure = useRef<HTMLElement>(null);
    const viewer = presentation === 'expandable' && placement.kind !== 'video' ? placement : undefined;
    const description = Boolean(placement.description) && (
        <details className="hint landscape-short:col-span-full landscape-short:row-start-4">
            <summary className="list-item min-h-11 cursor-pointer content-center">{descriptionLabel(placement)}</summary>
            <p className="max-h-quarter-screen overflow-auto whitespace-pre-line">{placement.description}</p>
        </details>
    );
    return (
        <CustomProperties properties={{ '--media-ratio': placementRatio(placement), ...(placement.kind === 'image' && placement.framing.mode === 'cover' && { '--media-focus': `${placement.framing.x}% ${placement.framing.y}%` }) }}>
            {(scope) => (
                <figure aria-hidden={presentation === 'thumbnail'} className="media-figure" ref={figure} {...scope}>
                    <motion.div {...(viewer && { layoutId })} className="bg-surface relative flex aspect-(--media-ratio) w-full items-center justify-center overflow-hidden">
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
                                            <span className="text-accent-text">{numeral(placement.page.number)}</span>
                                            <span aria-hidden="true">/</span>
                                            <span className="sr-only"> of </span>
                                            <span>{numeral(placement.asset.pages.length)}</span>
                                        </span>
                                        <span className="empty:hidden">{placement.page.label}</span>
                                    </span>
                                )}
                                {viewer && (
                                    <DialogTrigger isOpen={isOpen} onOpenChange={setOpen}>
                                        <Button aria-label={`Expand ${placementTitle(viewer)}`} className="text-foreground hover:text-accent-text col-start-2 row-start-1 grid size-11 place-items-center">
                                            <Expand className="size-4.5" />
                                        </Button>
                                        <ExpandedViewer description={viewer.kind === 'image' && description} layoutId={layoutId} placement={viewer} />
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
