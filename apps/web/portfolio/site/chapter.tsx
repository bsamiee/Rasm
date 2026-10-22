import { Match, String } from 'effect';
import useEmblaCarousel from 'embla-carousel-react';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { useInView, useReducedMotion } from 'motion/react';
import { type KeyboardEvent, useEffect, useEffectEvent, useId, useRef, useSyncExternalStore } from 'react';
import { CompositionGrid } from '#/media/composition-grid';
import { entryTitle } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure';
import type { Entry } from '#/model/portfolio';
import type { useNavigation } from '#/site/navigation';
import { Overview } from '#/site/overview';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Chapter = ({ entry, navigation }: { entry: typeof Entry.Type; navigation: ReturnType<typeof useNavigation> }) => {
    const slideDuration = 28;
    const reduced = useReducedMotion();
    const [viewport, clientApi, serverApi] = useEmblaCarousel({ align: 'start', duration: reduced === true ? 0 : slideDuration });
    const api = clientApi ?? serverApi;
    const selectedSnap = useSyncExternalStore(
        (onStoreChange) => {
            api.on('select', onStoreChange).on('reinit', onStoreChange);
            return () => {
                api.off('select', onStoreChange).off('reinit', onStoreChange);
            };
        },
        api.selectedSnap,
        serverApi.selectedSnap,
    );
    const headingId = useId();
    const chapter = useRef<HTMLElement>(null);
    const near = useInView(chapter, { margin: '700px' });
    const visible = useInView(chapter);
    const selected = navigation.selected(entry);
    const title = entryTitle(entry);
    const carousel = entry.compositions.length > 1;
    const details = [
        { label: 'Role', value: entry.role },
        { label: 'Credits', value: entry.credits },
    ].filter(({ value }) => value !== '');
    const select = useEffectEvent(() => {
        const composition = entry.compositions[api.selectedSnap()]?.id;
        if (composition !== navigation.selected(entry)) {
            navigation.select(entry.id, composition);
        }
    });
    const navigate = (event: KeyboardEvent<HTMLButtonElement>) =>
        Match.value(event).pipe(
            Match.when({ altKey: false, ctrlKey: false, metaKey: false, key: Match.is('ArrowLeft', 'ArrowRight') }, ({ key }) => {
                event.preventDefault();
                ({ ArrowLeft: api.goToPrev, ArrowRight: api.goToNext })[key]();
            }),
            Match.option,
        );
    useEffect(() => {
        const allowDrag = (_api: unknown, { detail }: { readonly detail: Event }) => !(detail.target instanceof Element && detail.target.closest('a,button,video') !== null);
        const release = () => reduced === true && api.goTo(api.selectedSnap(), true);
        api.on('select', select).on('pointerdown', allowDrag).on('pointerup', release);
        return () => {
            api.off('select', select).off('pointerdown', allowDrag).off('pointerup', release);
        };
    }, [api, reduced]);
    useEffect(() => {
        const target = entry.compositions.findIndex((item) => item.id === selected);
        if (target >= 0 && target !== api.selectedSnap()) {
            api.goTo(target, true);
        }
    }, [api, selected, entry.compositions]);
    return (
        <section aria-labelledby={headingId} className="chapter" id={entry.id} ref={chapter}>
            <div className="chapter-heading">
                <div className="eyebrow text-muted col-start-1 row-start-1 flex min-w-0 flex-col gap-2">
                    <span className="before:text-accent-text inline-flex items-baseline gap-3 ordinal">{String.capitalize(entry.kind)}</span>
                    <span className="wrap-anywhere empty:hidden">{[entry.year, entry.location].filter(Boolean).join(' / ')}</span>
                </div>
                <h3 className="chapter-title" id={headingId} tabIndex={-1}>
                    {title}
                </h3>
                {entry.cover !== undefined && (
                    <div className="media-h-45 col-start-2 row-span-2 row-start-1 w-45 self-end @max-3xl:hidden">
                        <MediaFigure active={false} placement={entry.cover} presentation="thumbnail" priority={false} renderMedia={true} />
                    </div>
                )}
                <p className="chapter-lead empty:hidden">{entry.description}</p>
            </div>
            {entry.compositions.length > 0 ? (
                <section aria-label={`${title} compositions`} aria-roledescription="carousel" className="@container touch-pan-y touch-pinch-zoom">
                    <div className={carousel ? 'grid-cols-trailing-pair grid items-center gap-x-3 gap-y-2' : ''}>
                        {carousel && (
                            <>
                                <Overview compositions={entry.compositions} onSelect={api.goTo} selected={selectedSnap} title={title} />
                                <button aria-disabled={!api.canGoToPrev()} aria-label="Previous view" className="button button-ghost not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line col-start-2 row-start-1 size-11 p-0" onClick={() => api.goToPrev()} onKeyDown={navigate} type="button">
                                    <ArrowLeft aria-hidden="true" className="size-5" />
                                </button>
                                <button aria-disabled={!api.canGoToNext()} aria-label="Next view" className="button button-ghost not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line col-start-3 row-start-1 size-11 p-0" onClick={() => api.goToNext()} onKeyDown={navigate} type="button">
                                    <ArrowRight aria-hidden="true" className="size-5" />
                                </button>
                                <span aria-atomic="true" aria-live="polite" className="sr-only">
                                    View {selectedSnap + 1} of {entry.compositions.length}
                                </span>
                            </>
                        )}
                        <div className={carousel ? 'col-span-full row-start-2 min-w-0 overflow-hidden' : 'overflow-hidden'} ref={viewport}>
                            <div className="flex items-start">
                                {entry.compositions.map((composition, slide) => (
                                    <fieldset aria-label={`${slide + 1} of ${entry.compositions.length}`} aria-roledescription="slide" className="min-w-0 shrink-0 basis-full inert:contain-size" inert={slide !== selectedSnap} key={composition.id}>
                                        <CompositionGrid active={visible && slide === selectedSnap} composition={composition} presentation="expandable" renderMedia={near && Math.abs(slide - selectedSnap) <= 1} />
                                    </fieldset>
                                ))}
                            </div>
                        </div>
                    </div>
                </section>
            ) : (
                <div className="flex flex-col items-center gap-2 px-5 py-10 text-center">
                    <h4 className="text-xl">No compositions yet</h4>
                    <p className="hint">Drawings and images will appear here as they are published.</p>
                </div>
            )}
            {details.length > 0 && (
                <dl className="leading-prose mt-10 flex gap-12 text-sm max-sm:flex-wrap max-sm:gap-6">
                    {details.map(({ label, value }) => (
                        <div className="min-w-0 wrap-anywhere" key={label}>
                            <dt className="text-muted tracking-label text-xs uppercase">{label}</dt>
                            <dd className="whitespace-pre-line">{value}</dd>
                        </div>
                    ))}
                </dl>
            )}
        </section>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Chapter };
