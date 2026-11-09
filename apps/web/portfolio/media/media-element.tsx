import { Match } from 'effect';
import { useInView } from 'motion/react';
import { lazy, type RefObject, Suspense, useEffect, useRef, useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { placementAlt } from '#/media/labels';
import { MediaFailure, MediaLoading } from '#/media/status.tsx';
import { assetUrl } from '#/model/api';
import type { ImageAsset } from '#/model/asset';
import type { Placement } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PdfSheet = lazy(() => import('#/media/pdf-sheet.tsx').then((module) => ({ default: module.PdfSheet })));
const MediaElement = ({ placement, active, priority, presentation, layoutId, viewport }: { placement: typeof Placement.Type; active: boolean; priority: boolean; presentation: 'expandable' | 'expanded' | 'full' | 'preview' | 'thumbnail'; layoutId?: string; viewport?: RefObject<HTMLElement | null> }) => {
    const video = useRef<HTMLVideoElement>(null);
    const nearby = useInView(viewport ?? video, { margin: '200px' });
    const visible = useInView(viewport ?? video);
    const [attempt, setAttempt] = useState({ count: 0, failed: false });
    const url = assetUrl(placement.asset);
    const label = placementAlt(placement);
    const thumbnail = presentation === 'preview' || presentation === 'thumbnail';
    const expanded = presentation === 'expanded';
    const eager = priority || presentation === 'expandable';
    const fail = () => setAttempt(({ count }) => ({ count, failed: true }));
    const imageSourceSet = (asset: ImageAsset) => asset.renditions && [...asset.renditions.map(({ width: rendition }) => `${assetUrl(asset, rendition)} ${rendition}w`), `${assetUrl(asset)} ${asset.width}w`].join(', ');
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
            () => <MediaFailure asset={placement.asset} message={placement.kind === 'video' ? 'Video failed to play' : 'Image failed to load'} onRetry={() => setAttempt(({ count }) => ({ count: count + 1, failed: false }))} />,
        ),
        Match.when({ kind: 'video', poster: (poster: ImageAsset | undefined): poster is ImageAsset => thumbnail && poster !== undefined }, ({ poster, asset: { width, height } }) => (
            <img alt={label} className="block size-full object-contain" decoding="async" height={height} loading="lazy" sizes="auto, 100vw" src={assetUrl(poster)} srcSet={imageSourceSet(poster)} width={width} />
        )),
        Match.discriminatorsExhaustive('kind')({
            pdf: (sheet) => (
                <ErrorBoundary fallback={<MediaFailure asset={placement.asset} message="PDF viewer failed to load" />}>
                    <Suspense fallback={<MediaLoading />}>
                        <PdfSheet layoutId={layoutId} placement={sheet} />
                    </Suspense>
                </ErrorBoundary>
            ),
            video: ({ poster, asset: { width, height } }) => (
                // biome-ignore lint/a11y/useMediaCaption: Uploads hold no timed text, owners burn in captions and write transcripts as descriptions
                <video aria-label={label} className="block size-full object-contain" controls={!thumbnail} height={height} key={attempt.count} onError={fail} playsInline={true} poster={poster && assetUrl(poster)} preload={active || thumbnail || eager ? 'metadata' : 'none'} ref={video} src={url} width={width} />
            ),
            image: (image) => (
                // biome-ignore lint/a11y/noNoninteractiveElementInteractions: Error event is the only load failure signal of img
                <img
                    alt={label}
                    className={expanded ? 'block max-w-none' : { contain: 'block size-full object-contain', cover: 'block size-full object-cover object-[var(--media-focus-x)_var(--media-focus-y)]' }[image.role === 'drawing' ? 'contain' : image.framing.mode]}
                    decoding="async"
                    fetchPriority={Match.value({ priority, active }).pipe(
                        Match.when({ priority: true }, () => 'high' as const),
                        Match.when({ active: true }, () => 'auto' as const),
                        Match.orElse(() => 'low' as const),
                    )}
                    height={image.asset.height}
                    key={attempt.count}
                    loading={expanded || eager ? 'eager' : 'lazy'}
                    onError={fail}
                    sizes={eager ? '100vw' : 'auto, 100vw'}
                    src={url}
                    srcSet={expanded ? undefined : imageSourceSet(image.asset)}
                    width={image.asset.width}
                />
            ),
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaElement };
