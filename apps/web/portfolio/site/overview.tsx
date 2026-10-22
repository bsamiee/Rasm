import { useAtom } from '@effect/atom-react';
import { dialogOpen } from '@rasm/ui/dialogs.ts';
import { Grid2x2, X } from 'lucide-react';
import { useId } from 'react';
import { Button, Dialog, DialogTrigger, GridLayout, GridList, GridListItem, Heading, Modal, ModalOverlay, Size, Virtualizer } from 'react-aria-components';
import { CompositionGrid } from '../media/composition-grid.tsx';
import { compositionLabel, numeral } from '../media/labels.ts';
import type { Composition } from '../model/placement.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Overview = ({ title, compositions, selected, onSelect }: { title: string; compositions: readonly (typeof Composition.Type)[]; selected: number; onSelect: (slide: number) => void }) => {
    const overviewItem = { width: 120, height: 180 };
    const [isOpen, setOpen] = useAtom(dialogOpen(useId()));
    return (
        <DialogTrigger isOpen={isOpen} onOpenChange={setOpen}>
            <Button className="hover:text-accent-text col-start-1 row-start-1 inline-flex min-h-11 items-center gap-2 justify-self-start text-xs">
                <Grid2x2 aria-hidden="true" className="size-4" />
                Overview
            </Button>
            <ModalOverlay className="bg-foreground/35 fixed inset-0 z-40 flex items-center justify-center p-4" isDismissable={true}>
                <Modal className="max-h-viewport-inset border-line bg-background flex w-full max-w-5xl flex-col border p-5 shadow-xl max-sm:p-3">
                    <Dialog className="flex min-h-0 flex-col outline-none">
                        <div className="mb-5 flex shrink-0 items-start justify-between gap-4">
                            <div className="min-w-0">
                                <Heading className="eyebrow text-muted" slot="title">
                                    Contact sheet
                                </Heading>
                                <p className="mt-2 text-xl leading-tight wrap-anywhere">{title}</p>
                            </div>
                            <Button aria-label="Close overview" className="hover:text-accent-text grid size-11 shrink-0 place-items-center" slot="close">
                                <X aria-hidden="true" className="size-5" />
                            </Button>
                        </div>
                        <Virtualizer layout={GridLayout} layoutOptions={{ minItemSize: new Size(overviewItem.width, overviewItem.height), maxColumns: 4 }}>
                            <GridList aria-label={`Views in ${title}`} className="max-h-overview min-h-0 overflow-auto overscroll-contain outline-none" layout="grid">
                                {compositions.map((composition, slide) => (
                                    <GridListItem
                                        className="hover:bg-surface focus-visible:outline-accent-text has-aria-current:border-accent-text cursor-pointer border border-transparent p-2 outline-none focus-visible:outline-2 focus-visible:-outline-offset-2"
                                        id={slide}
                                        key={composition.id}
                                        onAction={() => {
                                            onSelect(slide);
                                            setOpen(false);
                                        }}
                                        textValue={compositionLabel(composition)}
                                    >
                                        <CompositionGrid active={false} composition={composition} presentation="thumbnail" renderMedia={true} />
                                        <div aria-current={slide === selected} className="mt-3 flex flex-col gap-1 text-sm leading-snug underline-offset-4 aria-current:underline">
                                            <span className="eyebrow text-muted">View {numeral(slide + 1)}</span>
                                            <span className="line-clamp-3 min-w-0 wrap-anywhere">{compositionLabel(composition)}</span>
                                        </div>
                                    </GridListItem>
                                ))}
                            </GridList>
                        </Virtualizer>
                    </Dialog>
                </Modal>
            </ModalOverlay>
        </DialogTrigger>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Overview };
