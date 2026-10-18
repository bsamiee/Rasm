import { focus } from '@rasm/ui/focus.ts';
import { Array, Match, Option, Record, Schema } from 'effect';
import { FileText, Film } from 'lucide-react';
import { useId } from 'react';
import { GridList, GridListItem } from 'react-aria-components';
import { byteSize, entryTitle, quantity } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import { type Asset, type AssetCollection, PdfAsset, VideoAsset } from '../model/asset.ts';
import { type Portfolio, placements } from '../model/document.ts';
import { placementsFor } from '../model/placement.ts';
import { ConfirmDialog, MenuButton } from './controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Library = ({
    assets,
    draft,
    published,
    sourceId,
    onSelect,
    onUse,
    onHero,
    onDelete,
}: {
    sourceId: Option.Option<string>;
    onSelect: (id: Option.Option<string>) => void;
    onUse: (entryId: string, assetId: string) => void;
    assets: typeof AssetCollection.Type;
    draft: typeof Portfolio.Type;
    published: typeof Portfolio.Type;
    onHero: (asset: typeof Asset.Type) => void;
    onDelete: (asset: typeof Asset.Type) => void;
}) => {
    const ids = useId();
    const selected = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const used = (portfolio: typeof Portfolio.Type) => new Set(placements(portfolio).flatMap((placement) => [placement.asset.id, ...(placement.kind === 'video' && placement.poster !== undefined ? [placement.poster.id] : [])]));
    const summary = (asset: typeof Asset.Type) => [byteSize.format(asset.size), ...(Schema.is(PdfAsset)(asset) ? [quantity(asset.pages.length, { one: 'sheet', other: 'sheets' })] : [])];
    return (
        <section className="min-w-0 border-line border-t pt-5">
            <div className="mb-4 flex items-center justify-between gap-3">
                <h3 className="text-xl tracking-tight">Library</h3>
                <span className="text-muted text-xs">{quantity(Object.keys(assets).length, { one: 'file', other: 'files' })}</span>
            </div>
            <div className="grid min-w-0 editor:grid-cols-2 gap-6">
                <GridList aria-label="Source files" className="grid grid-cols-2 content-start gap-3 max-editor:data-chosen:hidden" data-chosen={Option.isSome(selected) || undefined} dependencies={[sourceId]} id={`${ids}library`} items={Object.values(assets)} selectionMode="none">
                    {(asset) => (
                        <GridListItem
                            className="min-w-0 cursor-pointer border border-line p-3 outline-none focus-visible:outline-2 focus-visible:outline-accent-text data-current:border-accent-text"
                            data-current={Option.contains(sourceId, asset.id) || undefined}
                            id={asset.id}
                            onAction={() => {
                                onSelect(Option.some(asset.id));
                                focus(`${ids}source`);
                            }}
                            textValue={asset.name}
                        >
                            <div className="media-h-30 pointer-events-none grid aspect-4/3 place-items-center bg-surface">
                                {Match.value(asset).pipe(
                                    Match.when({ mime: Match.is(...PdfAsset.fields.mime.literals) }, () => <FileText className="size-6 text-muted" />),
                                    Match.when({ mime: Match.is(...VideoAsset.fields.mime.literals) }, () => <Film className="size-6 text-muted" />),
                                    Match.orElse((image) => <MediaFigure active={false} placement={Array.headNonEmpty(placementsFor(image))} presentation="thumbnail" priority={false} renderMedia={true} />),
                                )}
                            </div>
                            <span className="wrap-anywhere mt-3 block text-sm">{asset.name}</span>
                            <span className="meta-line">{summary(asset).join(' · ')}</span>
                        </GridListItem>
                    )}
                </GridList>
                {Option.match(selected, {
                    onNone: () => <p className="hint">Select a source to preview or place it.</p>,
                    onSome: (asset) => {
                        const uses = [...(used(draft).has(asset.id) ? ['your draft'] : []), ...(used(published).has(asset.id) ? ['published work'] : [])];
                        return (
                            <div className="flex min-w-0 flex-col gap-4 editor:border-line editor:border-l editor:pl-6">
                                <button
                                    className="button button-ghost self-start"
                                    onClick={() => {
                                        onSelect(Option.none());
                                        focus(`${ids}library`);
                                    }}
                                    type="button"
                                >
                                    Back to files
                                </button>
                                <h4 className="wrap-anywhere text-lg tracking-tight outline-none" id={`${ids}source`} tabIndex={-1}>
                                    {asset.name}
                                </h4>
                                <div className="media-h-70 grid place-items-center bg-surface p-3">
                                    <MediaFigure active={true} placement={Array.headNonEmpty(placementsFor(asset))} presentation="expandable" priority={false} renderMedia={true} />
                                </div>
                                <div className="actions">
                                    {draft.entries.length > 0 && (
                                        <MenuButton label="Add to entry" onAction={({ id }) => onUse(id, asset.id)} options={draft.entries.map((entry) => ({ id: entry.id, label: entryTitle(entry) }))}>
                                            Add to entry
                                        </MenuButton>
                                    )}
                                    <button className="button button-ghost" onClick={() => onHero(asset)} type="button">
                                        Use as hero
                                    </button>
                                </div>
                                <p className="hint">{[...summary(asset), ...(uses.length > 0 ? [`Used in ${new Intl.ListFormat('en', { type: 'conjunction' }).format(uses)}.`] : [])].join(' · ')}</p>
                                <details className="hint">
                                    <summary className="min-h-11 cursor-pointer content-center">Source removal</summary>
                                    {uses.length > 0 && <p>Remove this file from saved and published work before deleting.</p>}
                                    <ConfirmDialog description="Permanently delete this source file. This cannot be undone." disabled={uses.length > 0} label="Delete file" onConfirm={() => onDelete(asset)} />
                                </details>
                            </div>
                        );
                    },
                })}
            </div>
        </section>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Library };
