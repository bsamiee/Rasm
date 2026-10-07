import { useAtom } from '@effect/atom-react';
import { Expand, X } from 'lucide-react';
import { motion } from 'motion/react';
import { type ComponentProps, type CSSProperties, lazy, type ReactElement, Suspense, useId, useRef } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, ModalOverlay, Text } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import type { Placement } from '../model/placement.ts';
import { dialogOpen } from '../site/navigation.ts';
import { mediaRatio, mediaUrl, numeral, placementTitle } from './display.ts';
import { Media } from './element.tsx';
import { MediaFailure, MediaLoading } from './status.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const AnimatedDialog = motion.create(Dialog);
const ZoomViewer = lazy(() => import('./viewer.tsx').then((module) => ({ default: module.ZoomViewer })));
function MediaFigure({ placement, active, priority, presentation, preload, renderMedia }: { placement: typeof Placement.Type; active: boolean; priority: boolean; presentation: Exclude<ComponentProps<typeof Media>['presentation'], 'expanded'>; preload?: boolean; renderMedia: boolean }): ReactElement {
    const layoutId = useId();
    const [isOpen, setOpen] = useAtom(dialogOpen(layoutId));
    const figure = useRef<HTMLElement>(null);
    const style: CSSProperties & Record<'--media-ratio', number> = { '--media-ratio': mediaRatio(placement) };
    const expandable = presentation === 'expandable' && placement.kind !== 'video';
    const description = placement.description && (
        <details className="hint landscape-short:col-span-full landscape-short:row-start-4">
            <summary className="list-item min-h-11 cursor-pointer content-center">{placement.kind === 'video' ? 'Description / transcript' : 'Detailed description'}</summary>
            <p className="max-h-[25svh] overflow-auto whitespace-pre-line">{placement.description}</p>
        </details>
    );
    return (
        // biome-ignore lint/nursery/noInlineStyles: Aspect ratio comes from the uploaded dimensions
        <figure aria-hidden={presentation === 'thumbnail'} className="w-[min(100%,calc(var(--media-height)*var(--media-ratio)))] min-w-0" ref={figure} style={style}>
            <motion.div {...(expandable && { layoutId })} className="relative flex aspect-(--media-ratio) w-full items-center justify-center overflow-hidden bg-surface">
                {renderMedia && <Media active={active} key={placement.asset.id} placement={placement} preload={preload} presentation={presentation} priority={priority} viewport={figure} />}
            </motion.div>
            {presentation !== 'thumbnail' && (placement.caption || placement.kind === 'pdf' || expandable) && (
                <figcaption className="wrap-anywhere grid grid-cols-[minmax(0,1fr)_auto] items-start gap-x-3 pt-2 text-muted text-sm leading-[1.55]">
                    {(placement.caption || placement.kind === 'pdf') && (
                        <div className="min-w-0">
                            {placement.caption && <span className="block whitespace-pre-line">{placement.caption}</span>}
                            {placement.kind === 'pdf' && (
                                <span className="flex flex-wrap items-baseline gap-x-4 gap-y-1">
                                    <span>{placement.asset.name}</span>
                                    <span className="inline-flex shrink-0 gap-1.5 font-mono text-xs tabular-nums">
                                        <span className="sr-only">Sheet </span>
                                        <span className="text-accent-text">{numeral(placement.page.number)}</span>
                                        <span aria-hidden="true">/</span>
                                        <span className="sr-only"> of </span>
                                        <span>{numeral(placement.asset.pages.length)}</span>
                                    </span>
                                    {placement.page.label && <span>{placement.page.label}</span>}
                                </span>
                            )}
                        </div>
                    )}
                    {expandable && (
                        <DialogTrigger isOpen={isOpen} onOpenChange={setOpen}>
                            <Button aria-label={`Expand ${placementTitle(placement)}${placement.kind === 'pdf' ? ` · Sheet ${placement.page.number} of ${placement.asset.pages.length}` : ''}`} className="col-start-2 row-start-1 grid size-11 place-items-center text-foreground hover:text-accent-text">
                                <Expand className="size-4.5" />
                            </Button>
                            <ModalOverlay className="fixed inset-0 z-50 bg-foreground/70 motion-safe:entering:animate-fade motion-safe:exiting:animate-[fade_200ms_reverse]" isDismissable={true}>
                                <Modal>
                                    <AnimatedDialog
                                        className="fixed inset-x-4 top-4 z-[51] flex h-[calc(var(--visual-viewport-height)-2rem)] flex-col gap-1.5 bg-background px-6 py-5 outline-none max-sm:inset-x-0 max-sm:top-0 max-sm:h-(--visual-viewport-height) max-sm:px-3.5 max-sm:pt-[max(10px,env(safe-area-inset-top))] max-sm:pb-[max(12px,env(safe-area-inset-bottom))] landscape-short:inset-x-0 landscape-short:top-0 landscape-short:grid landscape-short:h-(--visual-viewport-height) landscape-short:grid-cols-[minmax(0,1fr)_44px] landscape-short:grid-rows-[auto_auto_minmax(0,1fr)_auto] landscape-short:gap-x-4 landscape-short:gap-y-0 landscape-short:px-4 landscape-short:py-2"
                                        layoutRoot={true}
                                    >
                                        <div className="flex shrink-0 items-start justify-between gap-4 landscape-short:contents">
                                            <Heading className="wrap-anywhere max-h-[calc(var(--visual-viewport-height)*0.25)] min-w-0 overflow-y-auto text-lg leading-[1.25] max-sm:text-base landscape-short:col-start-1 landscape-short:row-start-1 landscape-short:text-[15px]" slot="title">
                                                {placement.kind === 'pdf' ? placement.asset.name : placementTitle(placement)}
                                            </Heading>
                                            <Button aria-label="Close viewer" className="grid min-h-11 min-w-11 shrink-0 place-items-center landscape-short:col-start-2 landscape-short:row-start-1" slot="close">
                                                <X className="size-6" />
                                            </Button>
                                        </div>
                                        {placement.kind === 'image' && (
                                            <Text className="wrap-anywhere max-h-[calc(var(--visual-viewport-height)*0.15)] shrink-0 overflow-y-auto text-[13px] text-muted landscape-short:hidden" elementType="p" slot="description">
                                                Scroll or pinch to zoom. Drag or use arrow keys to move. Use + and − to zoom.
                                            </Text>
                                        )}
                                        <ErrorBoundary fallback={<MediaFailure href={mediaUrl(placement.asset)} linkLabel="Open original" message="The expanded viewer could not be loaded." />}>
                                            <Suspense fallback={<MediaLoading />}>
                                                <ZoomViewer layoutId={layoutId} placement={placement} />
                                            </Suspense>
                                        </ErrorBoundary>
                                        {placement.kind !== 'pdf' && description}
                                    </AnimatedDialog>
                                </Modal>
                            </ModalOverlay>
                        </DialogTrigger>
                    )}
                </figcaption>
            )}
            {presentation !== 'thumbnail' && (!expandable || placement.kind === 'pdf') && description}
        </figure>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFigure };
