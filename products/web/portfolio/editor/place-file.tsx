import { useAtomSet } from '@effect/atom-react';
import { Field, MenuButton } from '@rasm/ui';
import { Array, Function, Option, Record, Result, Struct } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { useState } from 'react';
import { CheckboxGroup } from 'react-aria-components';
import { draft, updateEntry } from '#/editor/state';
import { format, joinDetails, sheetLabel } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure.tsx';
import type { Assets } from '#/model/asset';
import { Composition, Placement } from '#/model/placement';
import type { Entry } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PlaceFile = ({
    assets,
    entryId,
    assetId,
    onSelect,
    onAdd,
}: {
    readonly assets: Assets['Service'];
    readonly entryId: Entry['id'];
    readonly assetId: Option.Option<keyof Assets['Service']>;
    readonly onSelect: (id: Option.Option<keyof Assets['Service']>) => void;
    readonly onAdd: (compositionId: Composition['id']) => void;
}) => {
    const setDraft = useAtomSet(draft);
    const [order, setOrder] = useState<string[]>([]);
    const file = Option.flatMap(assetId, (id) => Record.get(assets, id));
    const placements = Option.toArray(file).flatMap(Placement.fromAsset);
    const pages = placements.filter((placement) => placement.kind === 'pdf');
    const chosen = pages.length > 0 ? Array.filterMap(order, (number) => Result.fromOption(Array.get(pages, Number(number) - 1), Function.constVoid)) : placements;
    const add = () => {
        const compositions = chosen.map((placement) => Composition.make({ id: Composition.fields.id.make(SecureRandom.uuidV7Unsafe()), items: [placement] }));
        setDraft(updateEntry(entryId, Struct.evolve({ compositions: Array.appendAll(compositions) })));
        onSelect(Option.none());
        Option.match(Array.head(compositions), { onNone: Function.constVoid, onSome: ({ id }) => onAdd(id) });
    };
    return (
        <div className="place-file">
            {pages.length > 0 && <p className="hint">Select sheets in presentation order. Publishing any sheet makes whole PDF public</p>}
            <MenuButton aria-label="Source file" onAction={({ asset }) => onSelect(Option.some(asset.id))} options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}>
                {Option.match(file, { onNone: () => 'Select file', onSome: Struct.get('name') })}
            </MenuButton>
            <div className="place-summary">
                <p className="hint">{joinDetails([format.quantity(chosen.length, { other: (count) => `${count} selected` }), pages.length > 0 ? format.quantity(pages.length, { one: (count) => `${count} sheet`, other: (count) => `${count} sheets` }) : undefined])}</p>
                <button className="button button-outline" disabled={chosen.length === 0} onClick={add} type="button">
                    {format.quantity(chosen.length, { one: (count) => `Add ${count} composition`, other: (count) => `Add ${count} compositions` })}
                </button>
            </div>
            {pages.length > 0 ? (
                <CheckboxGroup aria-label="Sheets in presentation order" className="sheet-grid" onChange={setOrder} value={order}>
                    <div className="actions">
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
                            <div key={placement.page.number}>
                                <MediaFigure active placement={placement} presentation="thumbnail" priority={false} renderMedia />
                                <Field.Checkbox value={String(placement.page.number)}>
                                    <span>
                                        {sheetLabel(placement.page, pages.length)}
                                        {position !== -1 && <span>Order {position + 1}</span>}
                                    </span>
                                </Field.Checkbox>
                            </div>
                        );
                    })}
                </CheckboxGroup>
            ) : (
                chosen.map((placement) => (
                    <div className="place-preview" key={placement.asset.id}>
                        <MediaFigure active placement={placement} presentation="expandable" priority={false} renderMedia />
                    </div>
                ))
            )}
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlaceFile };
