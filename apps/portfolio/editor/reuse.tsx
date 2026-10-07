import { BrowserCrypto } from '@effect/platform-browser';
import { Array, Effect, Option, Record, Struct } from 'effect';
import { type ReactNode, useState } from 'react';
import { CheckboxButton, CheckboxField } from 'react-aria-components';
import { placementLabel } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { AssetCollection } from '../model/asset.ts';
import { type Composition, createComposition, placementsFor } from '../model/placement.ts';
import { MenuButton } from './controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ReuseFile({ assets, onAdd }: { assets: typeof AssetCollection.Type; onAdd: (compositions: readonly (typeof Composition.Type)[]) => void }): ReactNode {
    const [sourceId, setSourceId] = useState(Option.none<string>());
    const [selected, setSelected] = useState<readonly number[]>([]);
    const source = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const sheets = Option.match(source, { onNone: () => [], onSome: placementsFor });
    const chosen = Array.getSomes(selected.map((index) => Array.get(sheets, index)));
    return (
        <div className="flex flex-col gap-4 bg-surface p-6 max-sm:p-4">
            <h4 className="text-[17px] tracking-[-0.02em]">Reuse a source file</h4>
            <p className="note">Select PDF sheets in the order you want them to appear. Each sheet becomes a composition. Publishing any sheet makes the whole original document public, including unselected pages.</p>
            <MenuButton
                label="Source file"
                onAction={({ asset }): void => {
                    setSourceId(Option.some(asset.id));
                    setSelected(asset.mime === 'application/pdf' ? [] : [0]);
                }}
                options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}
            >
                {Option.match(source, { onNone: () => 'Select a file', onSome: Struct.get('name') })}
            </MenuButton>
            {Option.exists(source, (asset) => asset.mime === 'application/pdf') && (
                <div className="flex max-h-[360px] flex-wrap gap-3 overflow-auto [--media-height:130px]">
                    <div className="actions w-full">
                        <button className="button button-ghost" onClick={(): void => setSelected(sheets.map((_, index) => index))} type="button">
                            All sheets
                        </button>
                        <button className="button button-ghost" onClick={(): void => setSelected([])} type="button">
                            Clear
                        </button>
                    </div>
                    {sheets.map((placement, index) => {
                        const order = selected.indexOf(index);
                        return (
                            <div className="w-[calc(50%-6px)] bg-background p-3" key={placementLabel(placement)}>
                                <MediaFigure active={true} placement={placement} presentation="thumbnail" priority={false} />
                                <CheckboxField className="group flex min-h-11 items-center gap-2" isSelected={order >= 0} onChange={(checked): void => setSelected(checked ? [...selected, index] : Array.remove(selected, order))}>
                                    <CheckboxButton className="size-5 border border-control-line group-selected:border-accent-text group-selected:bg-accent-text" />
                                    {placement.kind === 'pdf' ? placement.page.label || `Sheet ${index + 1}` : placementLabel(placement)}
                                    {order >= 0 ? ` · Order ${order + 1}` : ''}
                                </CheckboxField>
                            </div>
                        );
                    })}
                </div>
            )}
            <button
                className="button button-outline"
                disabled={chosen.length === 0}
                onClick={(): void => {
                    const crypto = Effect.runSync(BrowserCrypto.WebCrypto);
                    onAdd(chosen.map((placement) => createComposition(crypto.randomUUID(), placement)));
                    setSourceId(Option.none());
                    setSelected([]);
                }}
                type="button"
            >
                Add compositions
            </button>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReuseFile };
