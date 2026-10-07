import { motion, useReducedMotion } from 'motion/react';
import type { ReactElement } from 'react';
import { TransformComponent, TransformWrapper } from 'react-zoom-pan-pinch';
import type { Placement } from '../model/placement.ts';
import { ZoomControls } from './controls.tsx';
import { mediaUrl } from './display.ts';
import { Media } from './element.tsx';
import { PdfSheet } from './pdf.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ZoomViewer({ placement, layoutId }: { placement: Exclude<typeof Placement.Type, { kind: 'video' }>; layoutId: string }): ReactElement {
    const reduced = useReducedMotion() ?? false;
    const zoomDuration = 200;
    const animationTime = reduced ? 0 : zoomDuration;
    return placement.kind === 'pdf' ? (
        <PdfSheet layoutId={layoutId} placement={placement} />
    ) : (
        <TransformWrapper autoAlignment={{ disabled: reduced }} doubleClick={{ animationTime }} fitOnInit="contain" keyboard={{ disabled: false, animationTime }} minScale={0} smooth={!reduced} velocityAnimation={{ disabled: reduced }} zoomAnimation={{ disabled: reduced }}>
            {({ zoomIn, zoomOut, fitToView }): ReactElement => (
                <>
                    <ZoomControls fit={(): Promise<void> => fitToView({ animationTime })} href={mediaUrl(placement)} linkLabel="Original" zoomIn={(): Promise<void> => zoomIn(undefined, animationTime)} zoomOut={(): Promise<void> => zoomOut(undefined, animationTime)} />
                    <div className="min-h-0 flex-1 overflow-hidden bg-surface">
                        <TransformComponent contentClass="flex items-center justify-center" wrapperClass="size-full">
                            <motion.div className="shrink-0" layoutId={layoutId} style={{ width: placement.asset.width, height: placement.asset.height }}>
                                <Media active={true} placement={placement} presentation="expanded" priority={false} />
                            </motion.div>
                        </TransformComponent>
                    </div>
                </>
            )}
        </TransformWrapper>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomViewer };
