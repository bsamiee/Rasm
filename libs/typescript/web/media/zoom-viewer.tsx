import { Effect } from 'effect';
import type { ReactNode } from 'react';
import { Button } from 'react-aria-components';
import { TransformComponent, TransformWrapper } from 'react-zoom-pan-pinch';
import { ZoomControls } from '#/zoom-controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomViewer = ({ href, reducedMotion, classNames, children }: { readonly href: string; readonly reducedMotion: boolean; readonly classNames: { readonly controls: string; readonly stage: string }; readonly children: ReactNode }) => {
    const animationTime = reducedMotion ? 0 : 200;
    return (
        <TransformWrapper autoAlignment={{ disabled: reducedMotion }} doubleClick={{ animationTime }} fitOnInit="contain" keyboard={{ disabled: false, animationTime }} minScale={0} smooth={!reducedMotion} velocityAnimation={{ disabled: reducedMotion }} zoomAnimation={{ disabled: reducedMotion }}>
            {({ zoomIn, zoomOut, fitToView }) => (
                <>
                    <ZoomControls
                        className={classNames.controls}
                        fit={
                            <Button
                                onPress={() => {
                                    Effect.runFork(Effect.promise(() => fitToView({ animationTime })));
                                }}
                            >
                                Fit
                            </Button>
                        }
                        href={href}
                        zoomIn={() => {
                            Effect.runFork(Effect.promise(() => zoomIn(undefined, animationTime)));
                        }}
                        zoomOut={() => {
                            Effect.runFork(Effect.promise(() => zoomOut(undefined, animationTime)));
                        }}
                    />
                    <TransformComponent wrapperClass={classNames.stage}>{children}</TransformComponent>
                </>
            )}
        </TransformWrapper>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomViewer };
