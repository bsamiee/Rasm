import { Match } from 'effect';
import { lazy, type ReactElement, Suspense, useEffect, useRef, useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { type Placement, placementDimensions } from '../model/placement.ts';
import { mediaUrl, placementAlt } from './display.ts';
import { MediaFailure, MediaLoading } from './status.tsx';

// --- [TYPES] ---------------------------------------------------------------------------

type Presentation = 'expandable' | 'expanded' | 'full' | 'preview' | 'thumbnail';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PdfSheet = lazy(() => import('./pdf.tsx').then((module) => ({ default: module.PdfSheet })));
function Media({ placement, active, priority, presentation, preload, layoutId }: { placement: typeof Placement.Type; active: boolean; priority: boolean; presentation: Presentation; preload?: boolean | undefined; layoutId?: string }): ReactElement {
    const video = useRef<HTMLVideoElement>(null);
    const [failed, setFailed] = useState(false);
    const [attempt, setAttempt] = useState(0);
    const { width, height } = placementDimensions(placement);
    const url = mediaUrl(placement);
    const label = placementAlt(placement);
    const thumbnail = presentation === 'preview' || presentation === 'thumbnail';
    const expanded = presentation === 'expanded';
    useEffect(() => {
        if (!active) {
            video.current?.pause();
        }
    }, [active]);
    return failed ? (
        <MediaFailure
            href={url}
            linkLabel="Open original"
            message={placement.kind === 'video' ? 'This video could not be played.' : 'This image could not be displayed.'}
            onRetry={(): void => {
                setFailed(false);
                setAttempt((value) => value + 1);
            }}
        />
    ) : (
        Match.value(placement).pipe(
            Match.discriminatorsExhaustive('kind')({
                pdf: (sheet): ReactElement => (
                    <ErrorBoundary fallback={<MediaFailure href={url} linkLabel="Open original PDF" message="The PDF viewer could not be loaded." />}>
                        <Suspense fallback={<MediaLoading />}>
                            <PdfSheet {...(layoutId && { layoutId })} placement={sheet} />
                        </Suspense>
                    </ErrorBoundary>
                ),
                video: (): ReactElement => (
                    // biome-ignore lint/a11y/useMediaCaption: Description text accompanies each film as its transcript
                    <video aria-label={label} className="block size-full object-contain" controls={!thumbnail} height={height} key={attempt} onError={(): void => setFailed(true)} playsInline={true} preload={active || thumbnail || preload ? 'metadata' : 'none'} ref={video} src={url} width={width} />
                ),
                image: ({ framing }): ReactElement => (
                    // biome-ignore lint/a11y/noNoninteractiveElementInteractions: Image load failures report through onError
                    <img
                        alt={label}
                        className="block size-full"
                        decoding="async"
                        fetchPriority={priority ? 'high' : 'auto'}
                        height={height}
                        key={attempt}
                        loading={expanded || priority || preload ? 'eager' : 'lazy'}
                        onError={(): void => setFailed(true)}
                        src={url}
                        // biome-ignore lint/nursery/noInlineStyles: Focal point and framing come from the placement
                        style={{ objectFit: expanded ? 'contain' : framing.mode, objectPosition: framing.mode === 'cover' ? `${framing.x}% ${framing.y}%` : undefined }}
                        width={width}
                    />
                ),
            }),
        )
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Media };
