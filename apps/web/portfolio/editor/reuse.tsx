import { BrowserCrypto } from '@effect/platform-browser';
import { Array, Effect, Option, Record, Struct } from 'effect';
import { Check } from 'lucide-react';
import type { ReactNode } from 'react';
import { CheckboxButton, CheckboxField } from 'react-aria-components';
import { placementLabel } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { AssetCollection } from '../model/asset.ts';
import { type Composition, createComposition, placementsFor } from '../model/placement.ts';
import { MenuButton } from './controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ReuseFile({
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
}): ReactNode {
    const source = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const sheets = Option.match(source, { onNone: () => [], onSome: (asset) => placementsFor(asset, true) });
    const order = new Map(selected.map((index, position) => [index, position]));
    const pdf = Option.exists(source, (asset) => asset.mime === 'application/pdf');
    const chosen = pdf ? Array.getSomes(selected.map((index) => Array.get(sheets, index))) : sheets;
    return (
        <div className="flex min-w-0 flex-col gap-4">
            {pdf && <p className="hint">Select sheets in presentation order. Publishing a sheet makes the entire original PDF public, including unselected pages.</p>}
            <MenuButton
                label="Source file"
                onAction={({ asset }): void => {
                    onSource(Option.some(asset.id));
                    onSelect([]);
                }}
                options={Object.values(assets).map((asset) => ({ id: asset.id, label: asset.name, asset }))}
            >
                {Option.match(source, { onNone: () => 'Select a file', onSome: Struct.get('name') })}
            </MenuButton>
            <div className="flex flex-wrap items-center justify-between gap-3">
                <p className="hint">
                    {chosen.length} selected{pdf ? ` · ${sheets.length} sheets` : ''}
                </p>
                <button
                    className="button button-outline"
                    disabled={chosen.length === 0}
                    onClick={(): void => {
                        const crypto = Effect.runSync(BrowserCrypto.WebCrypto);
                        onAdd(chosen.map((placement) => createComposition(crypto.randomUUID(), placement)));
                        onSource(Option.none());
                        onSelect([]);
                    }}
                    type="button"
                >
                    Add {chosen.length} {chosen.length === 1 ? 'composition' : 'compositions'}
                </button>
            </div>
            {pdf && (
                <div className="grid grid-cols-[repeat(auto-fit,minmax(min(100%,140px),1fr))] gap-3 [--media-height:160px]">
                    <div className="actions col-span-full">
                        <button className="button button-ghost" onClick={(): void => onSelect(sheets.map((_, index) => index))} type="button">
                            All sheets
                        </button>
                        <button className="button button-ghost" onClick={(): void => onSelect([])} type="button">
                            Clear
                        </button>
                    </div>
                    {sheets.map((placement, index) => {
                        const position = order.get(index);
                        return (
                            <div className="min-w-0 bg-background p-3" key={placementLabel(placement)}>
                                <MediaFigure active={true} placement={placement} presentation="thumbnail" priority={false} renderMedia={true} />
                                <CheckboxField isSelected={position !== undefined} onChange={(checked): void => onSelect(checked ? [...selected, index] : selected.filter((value) => value !== index))}>
                                    <CheckboxButton className="group flex min-h-11 w-full items-center gap-2 text-left">
                                        <span className="grid size-5 shrink-0 place-items-center border border-control-line group-selected:border-accent-text group-selected:bg-accent-text group-selected:text-background">
                                            <Check className="size-3.5 opacity-0 group-selected:opacity-100" />
                                        </span>
                                        <span className="wrap-anywhere min-w-0 text-sm">
                                            {placement.kind === 'pdf' ? placement.page.label || `Sheet ${index + 1}` : placementLabel(placement)}
                                            {position !== undefined && <span className="block text-muted text-xs">Order {position + 1}</span>}
                                        </span>
                                    </CheckboxButton>
                                </CheckboxField>
                            </div>
                        );
                    })}
                </div>
            )}
            {!pdf &&
                chosen.map((placement) => (
                    <div className="[--media-height:240px]" key={placement.asset.id}>
                        <MediaFigure active={true} placement={placement} presentation="expandable" priority={false} renderMedia={true} />
                    </div>
                ))}
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ReuseFile };
