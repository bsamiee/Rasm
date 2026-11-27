import { useAtom, useAtomSet, useAtomValue } from '@effect/atom-react';
import { Option, Predicate, Record, String } from 'effect';
import useEmblaCarousel from 'embla-carousel-react';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { useInView, useReducedMotion } from 'motion/react';
import { useEffect, useEffectEvent, useId, useSyncExternalStore } from 'react';
import { Button, Toolbar } from 'react-aria-components';
import { CompositionGrid } from '#/media/composition-grid.tsx';
import { entryTitle } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure.tsx';
import type { Entry } from '#/model/portfolio';
import { chapterSection, selectComposition, selectedComposition } from '#/site/navigation';
import { Overview } from '#/site/overview.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Chapter = ({ entry }: { readonly entry: Entry }) => {
    const reduced = useReducedMotion();
    const [viewport, clientApi, serverApi] = useEmblaCarousel({ align: 'start', duration: reduced === true ? 0 : 28 });
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
    const [section, setSection] = useAtom(chapterSection(entry.id));
    const chapter = { current: Option.getOrNull(section) };
    const near = useInView(chapter, { margin: '700px' });
    const visible = useInView(chapter);
    const selected = useAtomValue(selectedComposition(entry.id)).pipe(Option.getOrUndefined);
    const navigate = useAtomSet(selectComposition);
    const title = entryTitle(entry);
    const details = Record.getSomes({ Role: Option.fromUndefinedOr(entry.role), Credits: Option.fromUndefinedOr(entry.credits) });
    const select = useEffectEvent(() => {
        const composition = entry.compositions[api.selectedSnap()]?.id;
        if (composition !== selected) {
            navigate({ project: entry.id, composition });
        }
    });
    useEffect(() => {
        const allowDrag = (_api: unknown, { detail }: { readonly detail: Event }) => !detail.composedPath().some((node) => node instanceof HTMLAnchorElement || node instanceof HTMLButtonElement || node instanceof HTMLVideoElement);
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
        <section aria-labelledby={headingId} className="chapter" ref={setSection}>
            <div className="chapter-heading">
                <div className="chapter-meta eyebrow">
                    <span className="ordinal">{String.capitalize(entry.kind)}</span>
                    <span>{[entry.year, entry.location].filter(Predicate.isNotUndefined).join(' / ')}</span>
                </div>
                <h3 className="chapter-title" id={headingId} tabIndex={-1}>
                    {title}
                </h3>
                {entry.cover !== undefined && (
                    <div className="chapter-cover">
                        <MediaFigure active={false} placement={entry.cover} presentation="thumbnail" priority={false} renderMedia />
                    </div>
                )}
                <p className="chapter-lead lead">{entry.description}</p>
            </div>
            {entry.compositions.length > 0 ? (
                <section aria-label={`${title} compositions`} aria-roledescription="carousel" className="carousel">
                    {entry.compositions.length > 1 && (
                        <>
                            <Toolbar aria-label={`${title} composition controls`}>
                                <Overview compositions={entry.compositions} onSelect={(slide) => api.goTo(slide)} selected={selectedSnap} title={title} />
                                <Button aria-disabled={!api.canGoToPrev()} aria-label="Previous composition" className="button button-ghost" onPress={() => api.goToPrev()}>
                                    <ArrowLeft aria-hidden="true" size={20} />
                                </Button>
                                <Button aria-disabled={!api.canGoToNext()} aria-label="Next composition" className="button button-ghost" onPress={() => api.goToNext()}>
                                    <ArrowRight aria-hidden="true" size={20} />
                                </Button>
                            </Toolbar>
                            <span aria-atomic="true" aria-live="polite" className="sr-only">
                                Composition {selectedSnap + 1} of {entry.compositions.length}
                            </span>
                        </>
                    )}
                    <div ref={viewport}>
                        <div>
                            {entry.compositions.map((composition, slide) => (
                                <fieldset aria-label={`${slide + 1} of ${entry.compositions.length}`} aria-roledescription="slide" inert={slide !== selectedSnap} key={composition.id}>
                                    <CompositionGrid active={visible && slide === selectedSnap} composition={composition} presentation="expandable" renderMedia={near && Math.abs(slide - selectedSnap) <= 1} />
                                </fieldset>
                            ))}
                        </div>
                    </div>
                </section>
            ) : (
                <h4 className="chapter-empty">No compositions yet</h4>
            )}
            {!Record.isEmptyReadonlyRecord(details) && (
                <dl className="chapter-details">
                    {Record.collect(details, (label, value) => (
                        <div key={label}>
                            <dt>{label}</dt>
                            <dd>{value}</dd>
                        </div>
                    ))}
                </dl>
            )}
        </section>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Chapter };
