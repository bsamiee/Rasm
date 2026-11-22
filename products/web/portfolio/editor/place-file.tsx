import { useAtomSet } from '@effect/atom-react';
import { Field, MenuButton } from '@rasm/ui';
import { Array, Function, Option, Record, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { useState } from 'react';
import { CheckboxGroup } from 'react-aria-components';
import { draft, updateEntry } from '#/editor/state.ts';
import { format, joinDetails, sheetLabel } from '#/media/labels.ts';
import { MediaFigure } from '#/media/media-figure.tsx';
import type { AssetId, Assets } from '#/model/asset.ts';
import { Composition, CompositionId, Placement } from '#/model/placement.ts';
import type { EntryId } from '#/model/portfolio.ts';

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
                <p className="hint">{joinDetails([format.quantity(chosen.length, { other: (count) => `${count} selected` }), pages.length > 0 ? format.quantity(pages.length, { one: (count) => `${count} sheet`, other: (count) => `${count} sheets` }) : undefined])}</p>
                <button className="button button-outline" disabled={chosen.length === 0} onClick={add} type="button">
                    {format.quantity(chosen.length, { one: (count) => `Add ${count} composition`, other: (count) => `Add ${count} compositions` })}
                </button>
            </div>
            {pages.length > 0 ? (
                <CheckboxGroup aria-label="Sheets in presentation order" className="grid grid-cols-sheets gap-3 media-h-40" onChange={setOrder} value={order}>
                    <div className="col-span-full actions">
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
                            <div className="min-w-0 bg-background p-3" key={placement.page.number}>
                                <MediaFigure active placement={placement} presentation="thumbnail" priority={false} renderMedia />
                                <Field.Checkbox value={String(placement.page.number)}>
                                    <span className="min-w-0 text-sm wrap-anywhere">
                                        {sheetLabel(placement.page, pages.length)}
                                        {position !== -1 && <span className="block text-xs text-muted">Order {position + 1}</span>}
                                    </span>
                                </Field.Checkbox>
                            </div>
                        );
                    })}
                </CheckboxGroup>
            ) : (
                chosen.map((placement) => (
                    <div className="media-h-60" key={placement.asset.id}>
                        <MediaFigure active placement={placement} presentation="expandable" priority={false} renderMedia />
                    </div>
                ))
            )}
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlaceFile };
