import { ModalDialog } from '@rasm/ui';
import { Array, Option, Predicate } from 'effect';
import { Grid2x2, X } from 'lucide-react';
import { Button, DialogTrigger, GridLayout, GridList, GridListItem, Heading, Size, Virtualizer } from 'react-aria-components';
import { CompositionGrid } from '#/media/composition-grid.tsx';
import { compositionLabel, format } from '#/media/labels.ts';
import type { Composition } from '#/model/placement.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Overview = ({ title, compositions, selected, onSelect }: { title: string; compositions: readonly (typeof Composition.Type)[]; selected: number; onSelect: (slide: number) => void }) => {
    const overviewItem = { width: 120, height: 180 };
    return (
        <DialogTrigger>
            <Button className="col-start-1 row-start-1 inline-flex min-h-11 items-center gap-2 justify-self-start text-xs hover:text-accent-text">
                <Grid2x2 aria-hidden="true" className="size-4" />
                Overview
            </Button>
            <ModalDialog className="fixed inset-0 z-40 flex items-center justify-center bg-foreground/35 p-4" dialog={{ className: 'flex min-h-0 flex-col outline-none' }} isDismissable modal={{ className: 'flex max-h-viewport-inset w-full max-w-5xl flex-col border border-line bg-background p-5 shadow-xl max-sm:p-3' }}>
                {({ close }) => (
                    <>
                        <div className="mb-5 flex shrink-0 items-start justify-between gap-4">
                            <div className="min-w-0">
                                <Heading className="eyebrow text-muted" slot="title">
                                    Contact sheet
                                </Heading>
                                <p className="mt-2 text-xl leading-tight wrap-anywhere">{title}</p>
                            </div>
                            <Button aria-label="Close overview" className="grid size-11 shrink-0 place-items-center hover:text-accent-text" slot="close">
                                <X aria-hidden="true" className="size-5" />
                            </Button>
                        </div>
                        <Virtualizer layout={GridLayout} layoutOptions={{ minItemSize: new Size(overviewItem.width, overviewItem.height), maxColumns: 4 }}>
                            <GridList
                                aria-label={`Compositions in ${title}`}
                                autoFocus
                                className="max-h-overview min-h-0 overflow-auto overscroll-contain outline-none"
                                escapeKeyBehavior="none"
                                layout="grid"
                                onSelectionChange={(keys) => {
                                    onSelect(Option.getOrElse(Array.findFirst(keys, Predicate.isNumber), () => selected));
                                    close();
                                }}
                                selectedKeys={[selected]}
                                selectionMode="single"
                            >
                                {compositions.map((composition, slide) => (
                                    <GridListItem
                                        className="group cursor-pointer border border-transparent p-2 outline-none hover:bg-background-muted focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-accent-text selected:border-accent-text"
                                        id={slide}
                                        key={composition.id}
                                        textValue={compositionLabel(composition)}
                                    >
                                        <CompositionGrid active={false} composition={composition} presentation="thumbnail" renderMedia />
                                        <div className="mt-3 flex flex-col gap-1 text-sm leading-snug underline-offset-4 group-selected:underline">
                                            <span className="eyebrow text-muted">Composition {format.numeral(slide + 1, compositions.length)}</span>
                                            <span className="line-clamp-3 min-w-0 wrap-anywhere">{compositionLabel(composition)}</span>
                                        </div>
                                    </GridListItem>
                                ))}
                            </GridList>
                        </Virtualizer>
                    </>
                )}
            </ModalDialog>
        </DialogTrigger>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Overview };
