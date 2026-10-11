import { useAtom } from '@effect/atom-react';
import { Option, Record } from 'effect';
import useEmblaCarousel from 'embla-carousel-react';
import { ArrowLeft, ArrowRight, Grid2x2, X } from 'lucide-react';
import { useInView, useReducedMotion } from 'motion/react';
import { type KeyboardEvent, type ReactNode, useEffect, useEffectEvent, useId, useRef, useSyncExternalStore } from 'react';
import { Button, Dialog, DialogTrigger, GridLayout, GridList, GridListItem, Heading, Modal, ModalOverlay, Size, Virtualizer } from 'react-aria-components';
import { CompositionGrid } from '../media/composition.tsx';
import { compositionLabel, entryTitle, numeral } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { Entry } from '../model/document.ts';
import { dialogOpen, type Navigation } from './navigation.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

function Chapter({ entry, index, navigation }: { entry: typeof Entry.Type; index: number; navigation: Navigation }): ReactNode {
    const slideDuration = 28;
    const overviewItem = { width: 120, height: 180 };
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
    const [isOpen, setOpen] = useAtom(dialogOpen(headingId));
    const chapter = useRef<HTMLElement>(null);
    const near = useInView(chapter, { margin: '700px' });
    const visible = useInView(chapter);
    const selected = navigation.selected(entry);
    const title = entryTitle(entry);
    const details = (
        [
            ['Role', entry.role],
            ['Credits', entry.credits],
        ] as const
    ).filter(([, value]) => value);
    const select = useEffectEvent(() => {
        const composition = entry.compositions[api.selectedSnap()]?.id;
        if (composition !== navigation.selected(entry)) {
            navigation.select(entry.id, composition);
        }
    });
    const navigate = (event: KeyboardEvent<HTMLButtonElement>): void => {
        const step = Record.get<string, () => void>({ ArrowLeft: api.goToPrev, ArrowRight: api.goToNext }, event.key);
        if (Option.isSome(step) && !(event.altKey || event.ctrlKey || event.metaKey)) {
            event.preventDefault();
            step.value();
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
        <section aria-labelledby={headingId} className="border-line border-b pt-14 pb-16 last:border-b-0 max-md:pt-10 max-md:pb-12" id={entry.id} ref={chapter}>
            <div className="mb-8 grid grid-cols-1 items-start gap-x-8 gap-y-6 @min-[48rem]:has-[>div>figure]:grid-cols-[minmax(0,1fr)_auto]">
                <div className="eyebrow col-start-1 row-start-1 flex min-w-0 flex-col gap-2 text-muted">
                    <span className="inline-flex items-baseline gap-3">
                        <span className="text-accent-text">[{numeral(index + 1)}]</span>
                        {entry.kind === 'study' ? 'Study' : 'Project'}
                    </span>
                    {(entry.year || entry.location) && <span className="wrap-anywhere">{[entry.year, entry.location].filter(Boolean).join(' / ')}</span>}
                </div>
                <h3 className="wrap-anywhere col-start-1 row-start-2 min-w-0 text-balance text-[clamp(2.25rem,calc(1.25rem+3.6cqw),4.5rem)] leading-[1.05] tracking-[-0.035em] outline-none" id={headingId} tabIndex={-1}>
                    {title}
                </h3>
                {entry.cover !== undefined && (
                    <div className="col-start-2 row-span-2 row-start-1 @max-[48rem]:hidden w-45 self-end [--media-height:11.25rem]">
                        <MediaFigure active={false} placement={entry.cover} presentation="thumbnail" priority={false} renderMedia={true} />
                    </div>
                )}
                {Boolean(entry.description) && <p className="lead col-span-full max-w-[64ch] hyphens-auto text-justify leading-[1.55] [hyphenate-limit-chars:7_3_3] [text-align-last:start]">{entry.description}</p>}
            </div>
            {entry.compositions.length > 0 ? (
                <section aria-label={`${title} compositions`} aria-roledescription="carousel" className="@container touch-pan-y touch-pinch-zoom">
                    <div className={entry.compositions.length > 1 ? 'grid grid-cols-[minmax(0,1fr)_auto_auto] items-center gap-x-3 gap-y-2' : ''}>
                        {entry.compositions.length > 1 && (
                            <>
                                <DialogTrigger isOpen={isOpen} onOpenChange={setOpen}>
                                    <Button className="col-start-1 row-start-1 inline-flex min-h-11 items-center gap-2 justify-self-start text-xs hover:text-accent-text">
                                        <Grid2x2 aria-hidden="true" className="size-4" />
                                        Overview
                                    </Button>
                                    <ModalOverlay className="fixed inset-0 z-40 flex items-center justify-center bg-foreground/35 p-4" isDismissable={true}>
                                        <Modal className="flex max-h-[calc(var(--visual-viewport-height)-2rem)] w-full max-w-5xl flex-col border border-line bg-background p-5 shadow-xl max-sm:p-3">
                                            <Dialog className="flex min-h-0 flex-col outline-none">
                                                {({ close }): ReactNode => (
                                                    <>
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
                                                            <GridList aria-label={`Views in ${title}`} className="max-h-[min(36rem,65dvh)] min-h-0 overflow-auto overscroll-contain outline-none" layout="grid">
                                                                {entry.compositions.map((composition, slide) => (
                                                                    <GridListItem
                                                                        className="cursor-pointer border border-transparent p-2 outline-none hover:bg-surface focus-visible:outline-2 focus-visible:outline-accent-text focus-visible:-outline-offset-2 has-aria-[current=true]:border-accent-text"
                                                                        id={slide}
                                                                        key={composition.id}
                                                                        onAction={(): void => {
                                                                            api.goTo(slide);
                                                                            close();
                                                                        }}
                                                                        textValue={compositionLabel(composition)}
                                                                    >
                                                                        <CompositionGrid active={false} composition={composition} presentation="thumbnail" renderMedia={true} />
                                                                        <div aria-current={slide === selectedSnap ? true : undefined} className="mt-3 flex flex-col gap-1 text-sm leading-snug underline-offset-4 aria-[current=true]:underline">
                                                                            <span className="eyebrow text-muted">View {numeral(slide + 1)}</span>
                                                                            <span className="wrap-anywhere line-clamp-3 min-w-0">{compositionLabel(composition)}</span>
                                                                        </div>
                                                                    </GridListItem>
                                                                ))}
                                                            </GridList>
                                                        </Virtualizer>
                                                    </>
                                                )}
                                            </Dialog>
                                        </Modal>
                                    </ModalOverlay>
                                </DialogTrigger>
                                <button aria-disabled={!api.canGoToPrev()} aria-label="Previous view" className="button button-ghost col-start-2 row-start-1 size-11 p-0 not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line" onClick={(): void => api.goToPrev()} onKeyDown={navigate} type="button">
                                    <ArrowLeft aria-hidden="true" className="size-5" />
                                </button>
                                <button aria-disabled={!api.canGoToNext()} aria-label="Next view" className="button button-ghost col-start-3 row-start-1 size-11 p-0 not-aria-disabled:hover:text-accent-text aria-disabled:text-control-line" onClick={(): void => api.goToNext()} onKeyDown={navigate} type="button">
                                    <ArrowRight aria-hidden="true" className="size-5" />
                                </button>
                                <span aria-atomic="true" aria-live="polite" className="sr-only">
                                    View {selectedSnap + 1} of {entry.compositions.length}
                                </span>
                            </>
                        )}
                        <div className={entry.compositions.length > 1 ? 'col-span-full row-start-2 min-w-0 overflow-hidden' : 'overflow-hidden'} ref={viewport}>
                            <div className="flex items-start">
                                {entry.compositions.map((composition, slide) => (
                                    // biome-ignore lint/a11y/useSemanticElements: Slides are ARIA groups inside the carousel region
                                    <div aria-label={`${slide + 1} of ${entry.compositions.length}`} aria-roledescription="slide" className="min-w-0 flex-[0_0_100%] [&[inert]]:[contain:size]" inert={slide !== selectedSnap} key={composition.id} role="group">
                                        <CompositionGrid active={visible && slide === selectedSnap} composition={composition} presentation="expandable" renderMedia={near && Math.abs(slide - selectedSnap) <= 1} />
                                    </div>
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
                <dl className="mt-10 flex gap-12 text-sm leading-[1.6] max-sm:flex-wrap max-sm:gap-6">
                    {details.map(([label, value]) => (
                        <div className="wrap-anywhere min-w-0" key={label}>
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
