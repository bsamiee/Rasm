import { ArrowUpRight, X } from 'lucide-react';
import { motion, useInView } from 'motion/react';
import { type ComponentProps, type CSSProperties, lazy, type ReactElement, Suspense, useId, useRef } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Modal, ModalOverlay, Text } from 'react-aria-components';
import type { Placement } from '../model/placement.ts';
import { mediaRatio, placementTitle, sheetLabel } from './display.ts';
import { Media } from './element.tsx';
import { MediaLoading } from './status.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomViewer = lazy(() => import('./viewer.tsx').then((module) => ({ default: module.ZoomViewer })));
function MediaFigure({ placement, active, priority, presentation }: { placement: typeof Placement.Type; active: boolean; priority: boolean; presentation: Exclude<ComponentProps<typeof Media>['presentation'], 'expanded'> }): ReactElement {
    const layoutId = useId();
    const figure = useRef<HTMLElement>(null);
    const nearby = useInView(figure, { margin: '200px' });
    const visible = useInView(figure);
    const style: CSSProperties & Record<'--media-ratio', number> = { '--media-ratio': mediaRatio(placement) };
    const expandable = presentation === 'expandable' && placement.kind !== 'video';
    const description = placement.description && (
        <details className="hint landscape-short:col-span-full landscape-short:row-start-3">
            <summary className="list-item min-h-11 cursor-pointer content-center">{placement.kind === 'video' ? 'Description / transcript' : 'Drawing description'}</summary>
            <p className="max-h-[25svh] overflow-auto whitespace-pre-line">{placement.description}</p>
        </details>
    );
    return (
        // biome-ignore lint/nursery/noInlineStyles: Aspect ratio comes from the uploaded dimensions
        <figure className="w-[min(100%,calc(var(--media-height)*var(--media-ratio)))] min-w-0" ref={figure} style={style}>
            <motion.div {...(expandable && { layoutId })} className="relative flex aspect-(--media-ratio) w-full items-center justify-center overflow-hidden bg-surface">
                {(priority || nearby) && <Media active={active && visible} key={placement.asset.id} placement={placement} presentation={presentation} priority={priority} />}
            </motion.div>
            {presentation !== 'thumbnail' && (
                <>
                    <figcaption className="wrap-anywhere flex flex-wrap items-baseline gap-x-4 gap-y-1 whitespace-pre-line pt-2 text-muted text-sm leading-[1.55] [&>span]:basis-full">
                        {placement.caption && <span>{placement.caption}</span>}
                        {placement.kind === 'pdf' && <span className="text-xs">{sheetLabel(placement)}</span>}
                        {expandable && (
                            <DialogTrigger>
                                <Button aria-label={`Expand ${placementTitle(placement)}`} className="ml-auto gap-1 whitespace-nowrap text-[13px] text-foreground text-link hover:text-accent-text">
                                    Expand
                                    <ArrowUpRight className="size-6" strokeLinecap="butt" strokeLinejoin="miter" />
                                </Button>
                                <ModalOverlay className="fixed inset-0 z-50 bg-foreground/70 motion-safe:entering:animate-fade motion-safe:exiting:animate-[fade_200ms_reverse]" isDismissable={true}>
                                    <Modal>
                                        <Dialog className="outline-none">
                                            <motion.div
                                                className="fixed inset-4 z-[51] flex flex-col gap-1.5 bg-background px-6 py-5 max-sm:inset-0 max-sm:px-3.5 max-sm:pt-[max(10px,env(safe-area-inset-top))] max-sm:pb-[max(12px,env(safe-area-inset-bottom))] landscape-short:inset-0 landscape-short:grid landscape-short:grid-cols-[minmax(0,1fr)_auto_44px] landscape-short:grid-rows-[auto_minmax(0,1fr)_auto] landscape-short:gap-x-4 landscape-short:gap-y-0 landscape-short:px-4 landscape-short:py-2"
                                                layoutRoot={true}
                                            >
                                                <div className="flex items-center justify-between gap-4 landscape-short:contents">
                                                    <Heading className="wrap-anywhere min-w-0 text-lg leading-[1.25] max-sm:text-base landscape-short:col-start-1 landscape-short:row-start-1 landscape-short:text-[15px]" slot="title">
                                                        {placementTitle(placement)}
                                                    </Heading>
                                                    <Button aria-label="Close viewer" className="grid min-h-11 min-w-11 shrink-0 place-items-center landscape-short:col-start-3 landscape-short:row-start-1" slot="close">
                                                        <X className="size-6" strokeLinecap="butt" />
                                                    </Button>
                                                </div>
                                                <Text className="wrap-anywhere text-[13px] text-muted landscape-short:hidden" elementType="p" slot="description">
                                                    {placement.kind === 'pdf' ? sheetLabel(placement) : 'Scroll or pinch to zoom. Drag to move.'}
                                                </Text>
                                                <Suspense fallback={<MediaLoading />}>
                                                    <ZoomViewer layoutId={layoutId} placement={placement} />
                                                </Suspense>
                                                {description}
                                            </motion.div>
                                        </Dialog>
                                    </Modal>
                                </ModalOverlay>
                            </DialogTrigger>
                        )}
                    </figcaption>
                    {!expandable && description}
                </>
            )}
        </figure>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFigure };
