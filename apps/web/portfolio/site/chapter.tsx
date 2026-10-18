import { useAtom } from '@effect/atom-react';
import { dialogOpen } from '@rasm/ui/dialogs.ts';
import { Match, String } from 'effect';
import useEmblaCarousel from 'embla-carousel-react';
import { ArrowLeft, ArrowRight, Grid2x2, X } from 'lucide-react';
import { useInView, useReducedMotion } from 'motion/react';
import { type KeyboardEvent, useEffect, useEffectEvent, useId, useRef, useSyncExternalStore } from 'react';
import { Button, Dialog, DialogTrigger, GridLayout, GridList, GridListItem, Heading, Modal, ModalOverlay, Size, Virtualizer } from 'react-aria-components';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle, numeral } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { Entry } from '../model/document.ts';
import type { useNavigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Chapter = ({ entry, index, navigation }: { entry: typeof Entry.Type; index: number; navigation: ReturnType<typeof useNavigation> }) => {
    const slideDuration = 28;
    const overviewItem = { width: 120, height: 180 };
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
    const [isOpen, setOpen] = useAtom(dialogOpen(headingId));
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
                <div className="eyebrow col-start-1 row-start-1 flex min-w-0 flex-col gap-2 text-muted">
                    <span className="inline-flex items-baseline gap-3">
                        <span className="text-accent-text">[{numeral(index + 1)}]</span>
                        {String.capitalize(entry.kind)}
                    </span>
                    <span className="wrap-anywhere empty:hidden">{[entry.year, entry.location].filter(Boolean).join(' / ')}</span>
                </div>
                <h3 className="chapter-title" id={headingId} tabIndex={-1}>
                    {title}
                </h3>
                {entry.cover !== undefined && (
                    <div className="media-h-45 col-start-2 row-span-2 row-start-1 @max-3xl:hidden w-45 self-end">
                        <MediaFigure active={false} placement={entry.cover} presentation="thumbnail" priority={false} renderMedia={true} />
                    </div>
                )}
                <p className="chapter-lead empty:hidden">{entry.description}</p>
            </div>
            {entry.compositions.length > 0 ? (
                <section aria-label={`${title} compositions`} aria-roledescription="carousel" className="@container touch-pan-y touch-pinch-zoom">
                    <div className={carousel ? 'grid grid-cols-trailing-pair items-center gap-x-3 gap-y-2' : ''}>
                        {carousel && (
                            <>
                                <DialogTrigger isOpen={isOpen} onOpenChange={setOpen}>
                                    <Button className="col-start-1 row-start-1 inline-flex min-h-11 items-center gap-2 justify-self-start text-xs hover:text-accent-text">
                                        <Grid2x2 aria-hidden="true" className="size-4" />
                                        Overview
                                    </Button>
                                    <ModalOverlay className="fixed inset-0 z-40 flex items-center justify-center bg-foreground/35 p-4" isDismissable={true}>
                                        <Modal className="flex max-h-viewport-inset w-full max-w-5xl flex-col border border-line bg-background p-5 shadow-xl max-sm:p-3">
                                            <Dialog className="flex min-h-0 flex-col outline-none">
                                                <div className="mb-5 flex shrink-0 items-start justify-between gap-4">
                                                    <div className="min-w-0">
                                                        <Heading className="eyebrow text-muted" slot="title">
                                                            Contact sheet
                                                        </Heading>
                                                        <p className="wrap-anywhere mt-2 text-xl leading-tight">{title}</p>
                                                    </div>
                                                    <Button aria-label="Close overview" className="grid size-11 shrink-0 place-items-center hover:text-accent-text" slot="close">
                                                        <X aria-hidden="true" className="size-5" />
                                                    </Button>
                                                </div>
                                                <Virtualizer layout={GridLayout} layoutOptions={{ minItemSize: new Size(overviewItem.width, overviewItem.height), maxColumns: 4 }}>
                                                    <GridList aria-label={`Views in ${title}`} className="max-h-overview min-h-0 overflow-auto overscroll-contain outline-none" layout="grid">
                                                        {entry.compositions.map((composition, slide) => (
                                                            <GridListItem
                                                                className="cursor-pointer border border-transparent p-2 outline-none hover:bg-surface focus-visible:outline-2 focus-visible:outline-accent-text focus-visible:-outline-offset-2 has-aria-current:border-accent-text"
                                                                id={slide}
                                                                key={composition.id}
                                                                onAction={() => {
                                                                    api.goTo(slide);
                                                                    setOpen(false);
                                                                }}
                                                                textValue={compositionLabel(composition)}
                                                            >
                                                                <CompositionGrid active={false} composition={composition} presentation="thumbnail" renderMedia={true} />
                                                                <div aria-current={slide === selectedSnap} className="mt-3 flex flex-col gap-1 text-sm leading-snug underline-offset-4 aria-current:underline">
                                                                    <span className="eyebrow text-muted">View {numeral(slide + 1)}</span>
                                                                    <span className="wrap-anywhere line-clamp-3 min-w-0">{compositionLabel(composition)}</span>
                                                                </div>
                                                            </GridListItem>
                                                        ))}
                                                    </GridList>
                                                </Virtualizer>
                                            </Dialog>
                                        </Modal>
                                    </ModalOverlay>
                                </DialogTrigger>
                                <button aria-disabled={!api.canGoToPrev()} aria-label="Previous view" className="button button-ghost col-start-2 row-start-1 size-11 p-0 not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line" onClick={() => api.goToPrev()} onKeyDown={navigate} type="button">
                                    <ArrowLeft aria-hidden="true" className="size-5" />
                                </button>
                                <button aria-disabled={!api.canGoToNext()} aria-label="Next view" className="button button-ghost col-start-3 row-start-1 size-11 p-0 not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line" onClick={() => api.goToNext()} onKeyDown={navigate} type="button">
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
                <dl className="mt-10 flex gap-12 text-sm leading-prose max-sm:flex-wrap max-sm:gap-6">
                    {details.map(({ label, value }) => (
                        <div className="wrap-anywhere min-w-0" key={label}>
                            <dt className="text-muted text-xs uppercase tracking-label">{label}</dt>
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
