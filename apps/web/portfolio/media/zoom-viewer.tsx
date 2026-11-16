import { Effect } from 'effect';
import { motion, useReducedMotion } from 'motion/react';
import { Button } from 'react-aria-components';
import { TransformComponent, TransformWrapper } from 'react-zoom-pan-pinch';
import { MediaElement } from '#/media/media-element.tsx';
import { ZoomControls } from '#/media/zoom-controls.tsx';
import { assetUrl } from '#/model/api';
import type { Placement } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomViewer = ({ placement, layoutId }: { placement: Exclude<typeof Placement.Type, { kind: 'video' }>; layoutId: string }) => {
    const reduced = useReducedMotion() ?? false;
    const zoomDuration = 200;
    const animationTime = reduced ? 0 : zoomDuration;
    return placement.kind === 'pdf' ? (
        <MediaElement active layoutId={layoutId} placement={placement} presentation="expanded" priority={false} />
    ) : (
        <TransformWrapper autoAlignment={{ disabled: reduced }} doubleClick={{ animationTime }} fitOnInit="contain" keyboard={{ disabled: false, animationTime }} minScale={0} smooth={!reduced} velocityAnimation={{ disabled: reduced }} zoomAnimation={{ disabled: reduced }}>
            {({ zoomIn, zoomOut, fitToView }) => (
                <>
                    <ZoomControls
                        fit={
                            <Button
                                className="button button-ghost"
                                onPress={() => {
                                    Effect.runFork(Effect.promise(() => fitToView({ animationTime })));
                                }}
                            >
                                Fit
                            </Button>
                        }
                        href={assetUrl(placement.asset)}
                        zoomIn={() => {
                            Effect.runFork(Effect.promise(() => zoomIn(undefined, animationTime)));
                        }}
                        zoomOut={() => {
                            Effect.runFork(Effect.promise(() => zoomOut(undefined, animationTime)));
                        }}
                    />
                    <div className="viewer-stage">
                        <TransformComponent contentClass="items-center justify-center" wrapperClass="size-full!">
                            <motion.div className="shrink-0" layoutId={layoutId}>
                                <MediaElement active placement={placement} presentation="expanded" priority={false} />
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
