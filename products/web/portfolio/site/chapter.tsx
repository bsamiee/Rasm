import { String } from 'effect';
import useEmblaCarousel from 'embla-carousel-react';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { useInView, useReducedMotion } from 'motion/react';
import { type KeyboardEvent, useEffect, useEffectEvent, useId, useRef, useSyncExternalStore } from 'react';
import { CompositionGrid } from '#/media/composition-grid.tsx';
import { entryTitle } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure.tsx';
import type { Entry } from '#/model/portfolio';
import type { useNavigation } from '#/site/navigation';
import { Overview } from '#/site/overview.tsx';

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
    const navigate = (event: KeyboardEvent<HTMLButtonElement>) => {
        if (!(event.altKey || event.ctrlKey || event.metaKey) && (event.key === 'ArrowLeft' || event.key === 'ArrowRight')) {
            event.preventDefault();
            ({ ArrowLeft: api.goToPrev, ArrowRight: api.goToNext })[event.key]();
        }
    };
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
        if (target !== -1 && target !== api.selectedSnap()) {
            api.goTo(target, true);
        }
    }, [api, selected, entry.compositions]);
    return (
        <section aria-labelledby={headingId} className="chapter" id={entry.id} ref={chapter}>
            <div className="chapter-heading">
                <div className="col-start-1 row-start-1 flex min-w-0 flex-col gap-2 eyebrow text-muted">
                    <span className="inline-flex items-baseline gap-3 ordinal before:text-accent-text">{String.capitalize(entry.kind)}</span>
                    <span className="wrap-anywhere empty:hidden">{[entry.year, entry.location].filter(Boolean).join(' / ')}</span>
                </div>
                <h3 className="chapter-title" id={headingId} tabIndex={-1}>
                    {title}
                </h3>
                {entry.cover !== undefined && (
                    <div className="col-start-2 row-span-2 row-start-1 w-45 self-end media-h-45 @max-3xl:hidden">
                        <MediaFigure active={false} placement={entry.cover} presentation="thumbnail" priority={false} renderMedia />
                    </div>
                )}
                <p className="chapter-lead empty:hidden">{entry.description}</p>
            </div>
            {entry.compositions.length > 0 ? (
                <section aria-label={`${title} compositions`} aria-roledescription="carousel" className="@container touch-pan-y touch-pinch-zoom">
                    <div className={carousel ? 'grid grid-cols-trailing-pair items-center gap-x-3 gap-y-2' : undefined}>
                        {carousel && (
                            <>
                                <Overview compositions={entry.compositions} onSelect={(slide) => api.goTo(slide)} selected={selectedSnap} title={title} />
                                <button aria-disabled={!api.canGoToPrev()} aria-label="Previous composition" className="col-start-2 row-start-1 button size-11 button-ghost p-0 not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line" onClick={() => api.goToPrev()} onKeyDown={navigate} type="button">
                                    <ArrowLeft aria-hidden="true" className="size-5" />
                                </button>
                                <button aria-disabled={!api.canGoToNext()} aria-label="Next composition" className="col-start-3 row-start-1 button size-11 button-ghost p-0 not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line" onClick={() => api.goToNext()} onKeyDown={navigate} type="button">
                                    <ArrowRight aria-hidden="true" className="size-5" />
                                </button>
                                <span aria-atomic="true" aria-live="polite" className="sr-only">
                                    Composition {selectedSnap + 1} of {entry.compositions.length}
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
                </div>
            )}
            {details.length > 0 && (
                <dl className="mt-10 flex gap-12 text-sm leading-prose max-sm:flex-wrap max-sm:gap-6">
                    {details.map(({ label, value }) => (
                        <div className="min-w-0 wrap-anywhere" key={label}>
                            <dt className="text-xs tracking-label text-muted uppercase">{label}</dt>
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
