import { useAtomSet } from '@effect/atom-react';
import { Field } from '@rasm/ui/field';
import { MenuButton } from '@rasm/ui/menu-button';
import { Array, Function, Option, Record, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { useState } from 'react';
import { CheckboxGroup } from 'react-aria-components';
import { format, joinDetails, sheetLabel } from '#/client/labels';
import { draft, updateEntry } from '#/client/state';
import { MediaFigure } from '#/media/media-figure';
import type { AssetId, Assets } from '#/model/asset';
import { Composition, CompositionId, Placement } from '#/model/placement';
import type { EntryId } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PlaceFile = ({
    assets,
    entryId,
    assetId,
    onSelect,
    onAdd,
}: {
    readonly assets: typeof Assets.Service;
    readonly entryId: typeof EntryId.Type;
    readonly assetId: Option.Option<typeof AssetId.Type>;
    readonly onSelect: (id: Option.Option<typeof AssetId.Type>) => void;
    readonly onAdd: (compositionId: typeof CompositionId.Type) => void;
}) => {
    const setDraft = useAtomSet(draft);
    const [order, setOrder] = useState<string[]>([]);
    const file = Option.flatMap(assetId, (id) => Record.get(assets, id));
    const placements = Option.toArray(file).flatMap(Placement.fromAsset);
    const pages = placements.filter((placement) => placement.kind === 'pdf');
    const chosen = pages.length > 0 ? Array.getSomes(order.map((number) => Array.findFirst(pages, ({ page }) => String(page.number) === number))) : placements;
    const add = () => {
        const compositions = chosen.map((placement) => Composition.make({ id: CompositionId.make(SecureRandom.uuidV7Unsafe()), items: [placement] }));
        setDraft(updateEntry(entryId, Struct.evolve({ compositions: Array.appendAll(compositions) })));
        onSelect(Option.none());
        Option.match(Array.head(compositions), { onNone: Function.constVoid, onSome: ({ id }) => onAdd(id) });
    };
    return (
        <div className="flex min-w-0 flex-col gap-4">
            {pages.length > 0 && <p className="hint">Select sheets in presentation order. Publishing any sheet makes whole PDF public</p>}
            <MenuButton aria-label="Source file" onAction={({ asset }) => onSelect(Option.some(asset.id))} options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}>
                {Option.match(file, { onNone: () => 'Select file', onSome: Struct.get('name') })}
            </MenuButton>
            <div className="flex flex-wrap items-center justify-between gap-3">
                <p className="hint">{joinDetails([format.quantity(chosen.length, { other: (shown) => `${shown} selected` }), pages.length > 0 ? format.quantity(pages.length, { one: (shown) => `${shown} sheet`, other: (shown) => `${shown} sheets` }) : undefined])}</p>
                <button className="button button-outline" disabled={chosen.length === 0} onClick={add} type="button">
                    {format.quantity(chosen.length, { one: (shown) => `Add ${shown} composition`, other: (shown) => `Add ${shown} compositions` })}
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
                                <Field.Checkbox value={String(placement.page.number)}>
                                    <span className="min-w-0 text-sm wrap-anywhere">
                                        {sheetLabel(placement.page, pages.length)}
                                        {position >= 0 && <span className="text-muted block text-xs">Order {position + 1}</span>}
                                    </span>
                                </Field.Checkbox>
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

export { PlaceFile };
