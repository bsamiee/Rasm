import { useAtomSet } from '@effect/atom-react';
import { Checkbox } from '@rasm/ui/checkbox.tsx';
import { MenuButton } from '@rasm/ui/menu-button.tsx';
import { Array, Effect, Option, Record, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { useState } from 'react';
import { quantity, sheetLabel } from '../media/labels.ts';
import { MediaFigure } from '../media/media-figure.tsx';
import type { AssetCollection } from '../model/asset.ts';
import { Composition, placementsFor } from '../model/placement.ts';
import { draft, identifier, updateEntry } from './state.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ReuseFile = ({ assets, entryId, sourceId, onSource, onAdd }: { assets: typeof AssetCollection.Type; entryId: string; sourceId: Option.Option<string>; onSource: (id: Option.Option<string>) => void; onAdd: (compositionId: string | undefined) => void }) => {
    const setDraft = useAtomSet(draft);
    const [selected, setSelected] = useState<readonly number[]>([]);
    const source = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const placements = Option.toArray(source).flatMap(placementsFor);
    const pages = placements.filter((placement) => placement.kind === 'pdf');
    const chosen = pages.length > 0 ? Array.getSomes(selected.map((index) => Array.get(pages, index))) : placements;
    return (
        <div className="flex min-w-0 flex-col gap-4">
            {pages.length > 0 && <p className="hint">Select sheets in presentation order. Publishing a sheet makes the entire original PDF public, including unselected pages.</p>}
            <MenuButton label="Source file" onAction={({ asset }) => onSource(Option.some(asset.id))} options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}>
                {Option.match(source, { onNone: () => 'Select a file', onSome: Struct.get('name') })}
            </MenuButton>
            <div className="flex flex-wrap items-center justify-between gap-3">
                <p className="hint">{[`${chosen.length} selected`, ...(pages.length > 0 ? [quantity(pages.length, { one: 'sheet', other: 'sheets' })] : [])].join(' · ')}</p>
                <button
                    className="button button-outline"
                    disabled={chosen.length === 0}
                    onClick={() => {
                        const compositions = chosen.map((placement) => Composition.make({ id: Effect.runSync(identifier), items: [placement] }));
                        setDraft(AsyncResult.map(updateEntry(entryId, Struct.evolve({ compositions: Array.appendAll(compositions) }))));
                        onSource(Option.none());
                        onAdd(compositions[0]?.id);
                    }}
                    type="button"
                >
                    Add {quantity(chosen.length, { one: 'composition', other: 'compositions' })}
                </button>
            </div>
            {pages.length > 0 ? (
                <div className="media-h-40 grid-cols-sheets grid gap-3">
                    <div className="actions col-span-full">
                        <button className="button button-ghost" onClick={() => setSelected(pages.map((_, index) => index))} type="button">
                            All sheets
                        </button>
                        <button className="button button-ghost" onClick={() => setSelected([])} type="button">
                            Clear
                        </button>
                    </div>
                    {pages.map((placement, index) => (
                        <div className="bg-background min-w-0 p-3" key={placement.page.number}>
                            <MediaFigure active={true} placement={placement} presentation="thumbnail" priority={false} renderMedia={true} />
                            <Checkbox isSelected={selected.includes(index)} onChange={(checked) => setSelected(checked ? [...selected, index] : selected.filter((value) => value !== index))}>
                                <span className="min-w-0 text-sm wrap-anywhere">
                                    {sheetLabel(placement.page, pages.length)}
                                    {selected.includes(index) && <span className="text-muted block text-xs">Order {selected.indexOf(index) + 1}</span>}
                                </span>
                            </Checkbox>
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
