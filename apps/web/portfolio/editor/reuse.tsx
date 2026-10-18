import { Array, Effect, Option, Record, Struct } from 'effect';
import { Check } from 'lucide-react';
import { CheckboxButton, CheckboxField } from 'react-aria-components';
import { quantity, sheetLabel } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { AssetCollection } from '../model/asset.ts';
import { type Composition, createComposition, placementsFor } from '../model/placement.ts';
import { MenuButton } from './controls.tsx';
import { identifier } from './services.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ReuseFile = ({
    assets,
    sourceId,
    selected,
    onSource,
    onSelect,
    onAdd,
}: {
    assets: typeof AssetCollection.Type;
    sourceId: Option.Option<string>;
    selected: readonly number[];
    onSource: (id: Option.Option<string>) => void;
    onSelect: (indices: readonly number[]) => void;
    onAdd: (compositions: readonly (typeof Composition.Type)[]) => void;
}) => {
    const source = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const placements = Option.toArray(source).flatMap(placementsFor);
    const pages = placements.filter((placement) => placement.kind === 'pdf');
    const chosen = pages.length > 0 ? Array.getSomes(selected.map((index) => Array.get(pages, index))) : placements;
    return (
        <div className="flex min-w-0 flex-col gap-4">
            {pages.length > 0 && <p className="hint">Select sheets in presentation order. Publishing a sheet makes the entire original PDF public, including unselected pages.</p>}
            <MenuButton
                label="Source file"
                onAction={({ asset }) => {
                    onSource(Option.some(asset.id));
                    onSelect([]);
                }}
                options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}
            >
                {Option.match(source, { onNone: () => 'Select a file', onSome: Struct.get('name') })}
            </MenuButton>
            <div className="flex flex-wrap items-center justify-between gap-3">
                <p className="hint">{[`${chosen.length} selected`, ...(pages.length > 0 ? [quantity(pages.length, { one: 'sheet', other: 'sheets' })] : [])].join(' · ')}</p>
                <button
                    className="button button-outline"
                    disabled={chosen.length === 0}
                    onClick={() => {
                        onAdd(chosen.map((placement) => createComposition(Effect.runSync(identifier), placement)));
                        onSource(Option.none());
                        onSelect([]);
                    }}
                    type="button"
                >
                    Add {quantity(chosen.length, { one: 'composition', other: 'compositions' })}
                </button>
            </div>
            {pages.length > 0 ? (
                <div className="media-h-40 grid grid-cols-sheets gap-3">
                    <div className="actions col-span-full">
                        <button className="button button-ghost" onClick={() => onSelect(pages.map((_, index) => index))} type="button">
                            All sheets
                        </button>
                        <button className="button button-ghost" onClick={() => onSelect([])} type="button">
                            Clear
                        </button>
                    </div>
                    {pages.map((placement, index) => (
                        <div className="min-w-0 bg-background p-3" key={placement.page.number}>
                            <MediaFigure active={true} placement={placement} presentation="thumbnail" priority={false} renderMedia={true} />
                            <CheckboxField isSelected={selected.includes(index)} onChange={(checked) => onSelect(checked ? [...selected, index] : selected.filter((value) => value !== index))}>
                                <CheckboxButton className="group flex min-h-11 w-full items-center gap-2 text-left">
                                    <span className="editor-checkbox">
                                        <Check className="editor-check" />
                                    </span>
                                    <span className="wrap-anywhere min-w-0 text-sm">
                                        {sheetLabel(placement.page, pages.length)}
                                        {selected.includes(index) && <span className="block text-muted text-xs">Order {selected.indexOf(index) + 1}</span>}
                                    </span>
                                </CheckboxButton>
                            </CheckboxField>
                        </div>
                    ))}
                </div>
            ) : (
                chosen.map((placement) => (
                    <div className="media-h-60" key={placement.asset.id}>
                        <MediaFigure active={true} placement={placement} presentation="expandable" priority={false} renderMedia={true} />
                    </div>
                ))
            )}
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReuseFile };
