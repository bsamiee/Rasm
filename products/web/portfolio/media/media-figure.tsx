import { ModalDialog } from '@rasm/ui/modal-dialog';
import { Match } from 'effect';
import { Expand, X } from 'lucide-react';
import { motion, useReducedMotion } from 'motion/react';
import { type ComponentProps, lazy, Suspense, useId, useRef } from 'react';
import { Button, DialogTrigger, Heading, Text } from 'react-aria-components';
import { ErrorBoundary } from 'react-error-boundary';
import { descriptionLabel, format, placementTitle } from '#/media/labels';
import { MediaElement } from '#/media/media-element.tsx';
import { MediaFailure, MediaLoading } from '#/media/status.tsx';
import { assetUrl } from '#/model/api';
import { Placement } from '#/model/placement';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'react' {
    interface CSSProperties {
        readonly '--focus-x'?: number;
        readonly '--focus-y'?: number;
        readonly '--media-ratio'?: number;
        readonly '--ratio-max'?: number;
        readonly '--ratio-sum'?: number;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomViewer = lazy(() => import('@rasm/media/zoom-viewer').then((module) => ({ default: module.ZoomViewer })));
const MediaFigure = ({ placement, active, presentation, priority, renderMedia }: Pick<ComponentProps<typeof MediaElement>, 'placement' | 'active' | 'priority'> & { readonly presentation: Exclude<ComponentProps<typeof MediaElement>['presentation'], 'expanded'>; readonly renderMedia: boolean }) => {
    const layoutId = useId();
    const figure = useRef<HTMLElement>(null);
    const reducedMotion = useReducedMotion() ?? false;
    const viewer = presentation === 'expandable' && placement.kind !== 'video' ? placement : undefined;
    const ratio = Placement.ratio(placement);
    const framed = Match.value(placement).pipe(
        Match.when({ kind: 'image', role: 'photograph', framing: { mode: 'cover' } }, ({ framing }) => ({ 'data-framing': framing.mode, style: { '--media-ratio': ratio, '--focus-x': framing.x, '--focus-y': framing.y } })),
        Match.orElse(() => ({ style: { '--media-ratio': ratio } })),
    );
    const description = Boolean(placement.description) && (
        <details className="media-description hint">
            <summary>{descriptionLabel(placement)}</summary>
            <p>{placement.description}</p>
        </details>
    );
    return (
        <figure className="media-figure" ref={figure} {...framed}>
            <motion.div {...(viewer && { layoutId })} className="media-frame">
                {renderMedia && <MediaElement active={active} key={placement.asset.id} placement={placement} presentation={presentation} priority={priority} viewport={figure} />}
            </motion.div>
            {presentation !== 'thumbnail' && (
                <>
                    <figcaption>
                        {placement.caption !== undefined && <span>{placement.caption}</span>}
                        {placement.kind === 'pdf' && (
                            <span className="sheet-number">
                                <span>{placement.asset.name}</span>
                                <span>
                                    <span className="sr-only">Sheet </span>
                                    <span>{format.numeral(placement.page.number, placement.asset.pages.length)}</span>
                                    <span aria-hidden="true">/</span>
                                    <span className="sr-only"> of </span>
                                    <span>{format.numeral(placement.asset.pages.length)}</span>
                                </span>
                                <span>{placement.page.label}</span>
                            </span>
                        )}
                        {viewer && (
                            <DialogTrigger>
                                <Button aria-label={`Expand ${placementTitle(viewer)}`}>
                                    <Expand size={18} />
                                </Button>
                                <ModalDialog isDismissable>
                                    <motion.div className="viewer-dialog" layoutRoot>
                                        <div className="viewer-header">
                                            <Heading className="viewer-heading" slot="title">
                                                {viewer.kind === 'pdf' ? viewer.asset.name : placementTitle(viewer)}
                                            </Heading>
                                            <Button aria-label="Close viewer" slot="close">
                                                <X size={24} />
                                            </Button>
                                        </div>
                                        {viewer.kind === 'image' && (
                                            <Text className="viewer-hint" elementType="p" slot="description">
                                                Scroll, pinch, or press + and − to zoom. Drag or press arrow keys to move
                                            </Text>
                                        )}
                                        <ErrorBoundary fallback={<MediaFailure asset={viewer.asset} message="Viewer failed to load" />}>
                                            <Suspense fallback={<MediaLoading />}>
                                                {viewer.kind === 'pdf' ? (
                                                    <MediaElement active layoutId={layoutId} placement={viewer} presentation="expanded" priority={false} />
                                                ) : (
                                                    <ZoomViewer classNames={{ controls: 'viewer-controls', stage: 'viewer-stage' }} href={assetUrl(viewer.asset.id)} reducedMotion={reducedMotion}>
                                                        <motion.div layoutId={layoutId}>
                                                            <MediaElement active placement={viewer} presentation="expanded" priority={false} />
                                                        </motion.div>
                                                    </ZoomViewer>
                                                )}
                                            </Suspense>
                                        </ErrorBoundary>
                                        {viewer.kind === 'image' && description}
                                    </motion.div>
                                </ModalDialog>
                            </DialogTrigger>
                        )}
                    </figcaption>
                    {viewer?.kind !== 'image' && description}
                </>
            )}
        </figure>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFigure };
