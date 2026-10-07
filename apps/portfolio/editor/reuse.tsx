import { Array, Option } from 'effect';
import { type ReactNode, useState } from 'react';
import { CheckboxButton, CheckboxField } from 'react-aria-components';
import { placementLabel } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { Asset, AssetCollection } from '../model/asset.ts';
import { type Composition, createComposition, type Placement, placementsFor } from '../model/placement.ts';
import { MenuButton } from './controls.tsx';
import { randomId } from './services.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Source {
    readonly asset: typeof Asset.Type;
    readonly sheets: readonly (typeof Placement.Type)[];
}

// --- [COMPOSITION] ---------------------------------------------------------------------

function ReuseFile({ assets, onAdd }: { assets: typeof AssetCollection.Type; onAdd: (compositions: readonly (typeof Composition.Type)[]) => void }): ReactNode {
    const [source, setSource] = useState(Option.none<Source>());
    const [selected, setSelected] = useState<readonly (typeof Placement.Type)[]>([]);
    return (
        <div className="flex flex-col gap-4 bg-surface p-6 max-sm:p-4">
            <h4 className="text-[17px] tracking-[-0.02em]">Reuse a source file</h4>
            <p className="note">Select PDF sheets in the order you want them to appear. Each sheet becomes a composition. Publishing any sheet makes the whole original document public, including unselected pages.</p>
            <MenuButton
                label="Source file"
                onAction={({ asset }): void => {
                    const sheets = placementsFor(asset);
                    setSource(Option.some({ asset, sheets }));
                    setSelected(asset.mime === 'application/pdf' ? [] : sheets);
                }}
                options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}
            >
                {Option.match(source, { onNone: () => 'Select a file', onSome: ({ asset }) => asset.name })}
            </MenuButton>
            {Option.match(
                Option.filter(source, ({ asset }) => asset.mime === 'application/pdf'),
                {
                    onNone: (): ReactNode => null,
                    onSome: ({ sheets }) => (
                        <div className="flex max-h-[360px] flex-wrap gap-3 overflow-auto [--media-height:130px]">
                            <div className="actions w-full">
                                <button className="button button-ghost" onClick={(): void => setSelected(sheets)} type="button">
                                    All sheets
                                </button>
                                <button className="button button-ghost" onClick={(): void => setSelected([])} type="button">
                                    Clear
                                </button>
                            </div>
                            {sheets.map((placement, index) => {
                                const order = selected.indexOf(placement);
                                return (
                                    <div className="w-[calc(50%-6px)] bg-background p-3" key={placementLabel(placement)}>
                                        <MediaFigure active={true} placement={placement} presentation="thumbnail" priority={false} />
                                        <CheckboxField className="group flex min-h-11 items-center gap-2" isSelected={order >= 0} onChange={(checked): void => setSelected(checked ? [...selected, placement] : Array.remove(selected, order))}>
                                            <CheckboxButton className="size-5 border border-control-line group-selected:border-accent-text group-selected:bg-accent-text" />
                                            Sheet {index + 1}
                                            {order >= 0 ? ` · Order ${order + 1}` : ''}
                                        </CheckboxField>
                                    </div>
                                );
                            })}
                        </div>
                    ),
                },
            )}
            <button
                className="button button-outline"
                disabled={selected.length === 0}
                onClick={(): void => {
                    onAdd(selected.map((placement) => createComposition(randomId(), placement)));
                    setSource(Option.none());
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
