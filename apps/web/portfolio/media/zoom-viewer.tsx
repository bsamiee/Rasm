import { motion, useReducedMotion } from 'motion/react';
import { Button } from 'react-aria-components';
import { TransformComponent, TransformWrapper } from 'react-zoom-pan-pinch';
import { MediaElement } from '#/media/media-element';
import { ZoomControls } from '#/media/zoom-controls';
import { mediaUrl } from '#/model/asset';
import type { Placement } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomViewer = ({ placement, layoutId }: { placement: Exclude<typeof Placement.Type, { kind: 'video' }>; layoutId: string }) => {
    const reduced = useReducedMotion() ?? false;
    const zoomDuration = 200;
    const animationTime = reduced ? 0 : zoomDuration;
    return placement.kind === 'pdf' ? (
        <MediaElement active={true} layoutId={layoutId} placement={placement} presentation="expanded" priority={false} />
    ) : (
        <TransformWrapper autoAlignment={{ disabled: reduced }} doubleClick={{ animationTime }} fitOnInit="contain" keyboard={{ disabled: false, animationTime }} minScale={0} smooth={!reduced} velocityAnimation={{ disabled: reduced }} zoomAnimation={{ disabled: reduced }}>
            {({ zoomIn, zoomOut, fitToView }) => (
                <>
                    <ZoomControls
                        fit={
                            <Button className="button button-ghost" onPress={() => fitToView({ animationTime })}>
                                Fit
                            </Button>
                        }
                        href={mediaUrl(placement.asset)}
                        zoomIn={() => zoomIn(undefined, animationTime)}
                        zoomOut={() => zoomOut(undefined, animationTime)}
                    />
                    <div className="viewer-stage">
                        <TransformComponent contentClass="items-center justify-center" wrapperClass="size-full!">
                            <motion.div className="shrink-0" layoutId={layoutId}>
                                <MediaElement active={true} placement={placement} presentation="expanded" priority={false} />
                            </motion.div>
                        </TransformComponent>
                    </div>
                </>
            )}
        </TransformWrapper>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomViewer };
