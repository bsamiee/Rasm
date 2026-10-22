import { Match } from 'effect';
import { useInView } from 'motion/react';
import { lazy, type RefObject, Suspense, useEffect, useRef, useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { placementAlt } from '#/media/labels';
import { MediaFailure, MediaLoading } from '#/media/status';
import { type ImageAsset, mediaUrl } from '#/model/asset';
import { type Placement, placementDimensions } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PdfSheet = lazy(() => import('#/media/pdf-sheet').then((module) => ({ default: module.PdfSheet })));
const MediaElement = ({ placement, active, priority, presentation, layoutId, viewport }: { placement: typeof Placement.Type; active: boolean; priority: boolean; presentation: 'expandable' | 'expanded' | 'full' | 'preview' | 'thumbnail'; layoutId?: string; viewport?: RefObject<HTMLElement | null> }) => {
    const video = useRef<HTMLVideoElement>(null);
    const nearby = useInView(viewport ?? video, { margin: '200px' });
    const visible = useInView(viewport ?? video);
    const [attempt, setAttempt] = useState({ count: 0, failed: false });
    const { width, height } = placementDimensions(placement);
    const url = mediaUrl(placement.asset);
    const label = placementAlt(placement);
    const thumbnail = presentation === 'preview' || presentation === 'thumbnail';
    const expanded = presentation === 'expanded';
    const eager = priority || presentation === 'expandable';
    const fail = () => setAttempt(({ count }) => ({ count, failed: true }));
    const imageSourceSet = (asset: ImageAsset) => asset.renditions && [...asset.renditions.map(({ width: rendition }) => `${mediaUrl(asset, rendition)} ${rendition}w`), `${mediaUrl(asset)} ${asset.width}w`].join(', ');
    useEffect(() => {
        if (!(active && visible)) {
            video.current?.pause();
        }
    }, [active, visible]);
    if (viewport !== undefined && !eager && !nearby) {
        return null;
    }
    return Match.value(placement).pipe(
        Match.when(
            () => attempt.failed,
            () => <MediaFailure href={url} linkLabel="Open original" message={placement.kind === 'video' ? 'This video could not be played.' : 'This image could not be displayed.'} onRetry={() => setAttempt(({ count }) => ({ count: count + 1, failed: false }))} />,
        ),
        Match.discriminatorsExhaustive('kind')({
            pdf: (sheet) => (
                <ErrorBoundary fallback={<MediaFailure href={url} linkLabel="Open original PDF" message="The PDF viewer could not be loaded." />}>
                    <Suspense fallback={<MediaLoading />}>
                        <PdfSheet layoutId={layoutId} placement={sheet} />
                    </Suspense>
                </ErrorBoundary>
            ),
            video: ({ poster }) =>
                thumbnail && poster !== undefined ? (
                    <img alt={label} className="block size-full object-contain" decoding="async" height={height} loading="lazy" sizes="auto, 100vw" src={mediaUrl(poster)} srcSet={imageSourceSet(poster)} width={width} />
                ) : (
                    // biome-ignore lint/a11y/useMediaCaption: Uploads hold no timed text, owners burn captions into the video and write transcripts as descriptions
                    <video aria-label={label} className="block size-full object-contain" controls={!thumbnail} height={height} key={attempt.count} onError={fail} playsInline={true} poster={poster && mediaUrl(poster)} preload={active || thumbnail || eager ? 'metadata' : 'none'} ref={video} src={url} width={width} />
                ),
            image: ({ framing, asset }) => (
                // biome-ignore lint/a11y/noNoninteractiveElementInteractions: The error event is the only load failure signal an img element reports
                <img
                    alt={label}
                    className={expanded ? 'block max-w-none' : { contain: 'block size-full object-contain', cover: 'block size-full object-cover object-(--media-focus)' }[framing.mode]}
                    decoding="async"
                    fetchPriority={Match.value({ priority, active }).pipe(
                        Match.when({ priority: true }, () => 'high' as const),
                        Match.when({ active: true }, () => 'auto' as const),
                        Match.orElse(() => 'low' as const),
                    )}
                    height={height}
                    key={attempt.count}
                    loading={expanded || eager ? 'eager' : 'lazy'}
                    onError={fail}
                    sizes={eager ? '100vw' : 'auto, 100vw'}
                    src={url}
                    srcSet={expanded ? undefined : imageSourceSet(asset)}
                    width={width}
                />
            ),
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaElement };
