import { useAtomSet } from '@effect/atom-react';
import { MenuButton } from '@rasm/ui/collection/menu-button';
import { Checkbox } from '@rasm/ui/form/field';
import { Array, Option, Record, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { AsyncResult } from 'effect/reactivity';
import { useState } from 'react';
import { CheckboxGroup } from 'react-aria-components';
import { draft, updateEntry } from '#/editor/state';
import { format, sheetLabel } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure';
import type { AssetCollection } from '#/model/asset';
import { Composition, placementsFor } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ReuseFile = ({ assets, entryId, sourceId, onSource, onAdd }: { readonly assets: typeof AssetCollection.Type; readonly entryId: string; readonly sourceId: Option.Option<string>; readonly onSource: (id: Option.Option<string>) => void; readonly onAdd: (compositionId: string) => void }) => {
    const setDraft = useAtomSet(draft);
    const [order, setOrder] = useState<string[]>([]);
    const source = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const placements = Option.toArray(source).flatMap(placementsFor);
    const pages = placements.filter((placement) => placement.kind === 'pdf');
    const chosen = pages.length > 0 ? Array.getSomes(order.map((number) => Array.findFirst(pages, ({ page }) => String(page.number) === number))) : placements;
    const add = () => {
        const compositions = chosen.map((placement) => Composition.make({ id: SecureRandom.uuidV7Unsafe(), items: [placement] }));
        setDraft(AsyncResult.map(updateEntry(entryId, Struct.evolve({ compositions: Array.appendAll(compositions) }))));
        onSource(Option.none());
        Option.map(Array.head(compositions), ({ id }) => onAdd(id));
    };
    return (
        <div className="flex min-w-0 flex-col gap-4">
            {pages.length > 0 && <p className="hint">Select sheets in presentation order. Publishing any sheet makes whole PDF public</p>}
            <MenuButton aria-label="Source file" onAction={({ asset }) => onSource(Option.some(asset.id))} options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}>
                {Option.match(source, { onNone: () => 'Select file', onSome: Struct.get('name') })}
            </MenuButton>
            <div className="flex flex-wrap items-center justify-between gap-3">
                <p className="hint">{[format.quantity(chosen.length, { other: 'selected' }), ...(pages.length > 0 ? [format.quantity(pages.length, { one: 'sheet', other: 'sheets' })] : [])].join(' · ')}</p>
                <button className="button button-outline" disabled={chosen.length === 0} onClick={add} type="button">
                    Add {format.quantity(chosen.length, { one: 'composition', other: 'compositions' })}
                </button>
            </div>
            {pages.length > 0 ? (
                <CheckboxGroup aria-label="Sheets in presentation order" className="media-h-40 grid-cols-sheets grid gap-3" onChange={setOrder} value={order}>
                    <div className="actions col-span-full">
                        <button className="button button-ghost" onClick={() => setOrder(pages.map(({ page }) => String(page.number)))} type="button">
                            All sheets
                        </button>
                        <button className="button button-ghost" onClick={() => setOrder([])} type="button">
                            Clear
                        </button>
                    </div>
                    {pages.map((placement) => {
                        const position = order.indexOf(String(placement.page.number));
                        return (
                            <div className="bg-background min-w-0 p-3" key={placement.page.number}>
                                <MediaFigure active={true} placement={placement} presentation="thumbnail" priority={false} renderMedia={true} />
                                <Checkbox value={String(placement.page.number)}>
                                    <span className="min-w-0 text-sm wrap-anywhere">
                                        {sheetLabel(placement.page, pages.length)}
                                        {position >= 0 && <span className="text-muted block text-xs">Order {position + 1}</span>}
                                    </span>
                                </Checkbox>
                            </div>
                        );
                    })}
                </CheckboxGroup>
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
