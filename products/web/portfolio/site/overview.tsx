import { ModalDialog } from '@rasm/ui';
import { Array, Option, Predicate } from 'effect';
import { Grid2x2, X } from 'lucide-react';
import { Button, DialogTrigger, GridLayout, GridList, GridListItem, Heading, Size, Virtualizer } from 'react-aria-components';
import { CompositionGrid } from '#/media/composition-grid.tsx';
import { compositionCaption, format } from '#/media/labels';
import type { Composition } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Overview = ({ title, compositions, selected, onSelect }: { readonly title: string; readonly compositions: readonly (typeof Composition.Type)[]; readonly selected: number; readonly onSelect: (slide: number) => void }) => {
    const overviewItem = { width: 120, height: 180 };
    return (
        <DialogTrigger>
            <Button className="overview-trigger">
                <Grid2x2 aria-hidden="true" size={16} />
                Overview
            </Button>
            <ModalDialog isDismissable modal={{ className: 'overview' }}>
                {({ close }) => (
                    <>
                        <header>
                            <div>
                                <Heading className="eyebrow" slot="title">
                                    Contact sheet
                                </Heading>
                                <p>{title}</p>
                            </div>
                            <Button aria-label="Close overview" slot="close">
                                <X aria-hidden="true" size={20} />
                            </Button>
                        </header>
                        <Virtualizer layout={GridLayout} layoutOptions={{ minItemSize: new Size(overviewItem.width, overviewItem.height), maxColumns: 4 }}>
                            <GridList
                                aria-label={`Compositions in ${title}`}
                                autoFocus
                                escapeKeyBehavior="none"
                                layout="grid"
                                onSelectionChange={(keys) => {
                                    onSelect(Option.getOrElse(Array.findFirst(keys, Predicate.isNumber), () => selected));
                                    close();
                                }}
                                selectedKeys={[selected]}
                                selectionMode="single"
                            >
                                {compositions.map((composition, slide) => {
                                    const ordinal = `Composition ${format.numeral(slide + 1, compositions.length)}`;
                                    const caption = compositionCaption(composition);
                                    return (
                                        <GridListItem id={slide} key={composition.id} textValue={Option.getOrElse(caption, () => ordinal)}>
                                            <CompositionGrid active={false} composition={composition} presentation="thumbnail" renderMedia />
                                            <div className="overview-label">
                                                <span className="eyebrow">{ordinal}</span>
                                                {Option.getOrNull(Option.map(caption, (text) => <span>{text}</span>))}
                                            </div>
                                        </GridListItem>
                                    );
                                })}
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
