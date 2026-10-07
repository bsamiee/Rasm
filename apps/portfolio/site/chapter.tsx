import useEmblaCarousel from 'embla-carousel-react';
import { useInView, useReducedMotion } from 'motion/react';
import { type KeyboardEvent, type ReactNode, useEffect, useEffectEvent, useId, useRef, useSyncExternalStore } from 'react';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { Entry } from '../model/document.ts';
import { type Navigation, numeral } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

function Chapter({ entry, index, navigation }: { entry: typeof Entry.Type; index: number; navigation: Navigation }): ReactNode {
    const slideDuration = 28;
    const reduced = useReducedMotion();
    const [viewport, clientApi, serverApi] = useEmblaCarousel({ align: 'start', duration: reduced ? 0 : slideDuration });
    const api = clientApi ?? serverApi;
    const selectedSnap = useSyncExternalStore(
        (onStoreChange) => {
            api.on('select', onStoreChange).on('reinit', onStoreChange);
            return (): void => {
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
    const selected = navigation.selections[entry.id];
    const title = entryTitle(entry);
    const details = (
        [
            ['Role', entry.role],
            ['Credits', entry.credits],
        ] as const
    ).filter(([, value]) => value);
    const select = useEffectEvent(() => navigation.select(entry.id, entry.compositions[api.selectedSnap()]?.id));
    const navigate = (event: KeyboardEvent<HTMLButtonElement>): void => {
        if (event.altKey || event.ctrlKey || event.metaKey) {
            return;
        }
        switch (event.key) {
            case 'ArrowLeft':
                event.preventDefault();
                api.goToPrev();
                break;
            case 'ArrowRight':
                event.preventDefault();
                api.goToNext();
                break;
        }
    };
    useEffect(() => {
        const allowDrag = (_api: unknown, { detail }: { readonly detail: Event }): boolean => !(detail.target instanceof Element && detail.target.closest('a,button,video'));
        const release = (): void => {
            if (reduced) {
                api.goTo(api.selectedSnap(), true);
            }
        };
        api.on('select', select).on('pointerdown', allowDrag).on('pointerup', release);
        return (): void => {
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
        <section aria-labelledby={headingId} className="border-line border-b pt-[76px] pb-[88px] max-md:pt-[55px] max-md:pb-[66px]" id={entry.id} ref={chapter}>
            <div className="eyebrow flex justify-between gap-[30px] text-muted max-sm:flex-wrap max-sm:gap-2">
                <span className="text-accent-text">
                    {numeral(index + 1)} / {entry.kind === 'study' ? 'Independent study' : 'Project'}
                </span>
                <span>{[entry.year, entry.location].filter(Boolean).join(' / ')}</span>
            </div>
            <div className="mt-[25px] mb-8 flex items-center justify-between gap-9 max-sm:gap-4">
                <h2 className="wrap-anywhere min-w-0 text-[clamp(38px,5.7vw,82px)] leading-[1.04] tracking-[-0.045em] max-md:text-[clamp(36px,8vw,68px)]" id={headingId} tabIndex={-1}>
                    {title}
                </h2>
                {entry.cover !== undefined && (
                    <div className="w-[90px] shrink-0 [--media-height:80px] max-sm:w-[60px]">
                        <MediaFigure active={false} placement={entry.cover} presentation="thumbnail" priority={false} />
                    </div>
                )}
            </div>
            {Boolean(entry.description) && <p className="lead mb-10">{entry.description}</p>}
            {entry.compositions.length > 0 ? (
                <section aria-label={`${title} compositions`} aria-roledescription="carousel" className="touch-pan-y touch-pinch-zoom">
                    <div className="overflow-hidden" ref={viewport}>
                        <div className="flex">
                            {entry.compositions.map((composition, slide) => (
                                // biome-ignore lint/a11y/useSemanticElements: Slides are ARIA groups inside the carousel region
                                <div aria-label={`${slide + 1} of ${entry.compositions.length}`} aria-roledescription="slide" className="min-w-0 flex-[0_0_100%]" inert={slide !== selectedSnap} key={composition.id} role="group">
                                    <CompositionGrid active={visible && slide === selectedSnap} composition={composition} preview={false} renderMedia={near && Math.abs(slide - selectedSnap) <= 1} />
                                </div>
                            ))}
                        </div>
                    </div>
                    <fieldset aria-label={`${title} composition navigation`} className="mt-6 flex min-w-0 items-center gap-4 border-line border-t pt-3 max-sm:flex-wrap max-sm:gap-2">
                        <span aria-atomic="true" aria-live="polite" className="eyebrow shrink-0 text-accent-text">
                            {numeral(selectedSnap + 1)} / {numeral(entry.compositions.length)}
                        </span>
                        <select aria-label={`Choose a composition in ${title}`} className="control min-w-0 max-w-[60%] flex-1 truncate text-[13px] max-sm:max-w-none" onChange={(event): void => api.goTo(event.target.selectedIndex)} value={selectedSnap}>
                            {entry.compositions.map((composition, slide) => (
                                <option key={composition.id} value={slide}>
                                    {slide + 1} — {compositionLabel(composition)}
                                </option>
                            ))}
                        </select>
                        <div className="ml-auto flex max-sm:w-full max-sm:justify-between">
                            <button aria-disabled={!api.canGoToPrev()} className="button button-ghost" onClick={(): void => api.goToPrev()} onKeyDown={navigate} type="button">
                                Previous
                            </button>
                            <button aria-disabled={!api.canGoToNext()} className="button button-ghost" onClick={(): void => api.goToNext()} onKeyDown={navigate} type="button">
                                Next
                            </button>
                        </div>
                    </fieldset>
                </section>
            ) : (
                <div className="flex flex-col items-center gap-2 px-5 py-10 text-center">
                    <h3 className="text-xl">No compositions yet</h3>
                    <p className="hint">Drawings and images will appear here as they are published.</p>
                </div>
            )}
            {details.length > 0 && (
                <dl className="mt-10 flex gap-12 text-sm leading-[1.6] max-sm:flex-wrap max-sm:gap-6">
                    {details.map(([label, value]) => (
                        <div key={label}>
                            <dt className="text-muted text-xs uppercase tracking-[0.04em]">{label}</dt>
                            <dd className="whitespace-pre-line">{value}</dd>
                        </div>
                    ))}
                </dl>
            )}
        </section>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Chapter };
