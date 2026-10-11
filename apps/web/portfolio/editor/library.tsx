import { Array, Match, Option, Record } from 'effect';
import { FileText, Film } from 'lucide-react';
import { type ReactNode, useId } from 'react';
import { GridList, GridListItem } from 'react-aria-components';
import { byteSize, entryTitle } from '../media/display.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { Asset, AssetCollection } from '../model/asset.ts';
import { type Portfolio, placements } from '../model/document.ts';
import { placementsFor } from '../model/placement.ts';
import { ConfirmDialog, MenuButton } from './controls.tsx';
import { focus } from './focus.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const usage = new Intl.ListFormat('en', { type: 'conjunction' });

// --- [COMPOSITION] ---------------------------------------------------------------------

function Library({
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
}): ReactNode {
    const assetIds = (portfolio: typeof Portfolio.Type): ReadonlySet<string> => new Set(placements(portfolio).flatMap((placement) => (placement.kind === 'video' && placement.poster ? [placement.asset.id, placement.poster.id] : [placement.asset.id])));
    const inDraft = assetIds(draft);
    const inPublished = assetIds(published);
    const ids = useId();
    const selected = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    return (
        <section className="min-w-0 border-line border-t pt-5">
            <div className="mb-4 flex items-center justify-between gap-3">
                <h3 className="text-xl tracking-tight">Library</h3>
                <span className="text-muted text-xs">{Object.keys(assets).length} files</span>
            </div>
            <div className="grid min-w-0 gap-6 min-[800px]:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
                <GridList aria-label="Source files" className={`grid grid-cols-2 content-start gap-3 ${Option.isSome(selected) ? 'max-[799px]:hidden' : ''}`} dependencies={[sourceId]} id={`${ids}library`} items={Object.values(assets)} selectionMode="none">
                    {(asset): ReactNode => (
                        <GridListItem
                            className="min-w-0 cursor-pointer border border-line p-3 outline-none focus-visible:outline-2 focus-visible:outline-accent-text data-[current=true]:border-accent-text"
                            data-current={Option.contains(sourceId, asset.id)}
                            id={asset.id}
                            onAction={(): void => {
                                onSelect(Option.some(asset.id));
                                focus(`${ids}source`);
                            }}
                            textValue={asset.name}
                        >
                            <div className="pointer-events-none grid aspect-[4/3] place-items-center bg-surface [--media-height:120px]">
                                {Match.value(asset.mime).pipe(
                                    Match.when('application/pdf', () => <FileText className="size-6 text-muted" />),
                                    Match.when(Match.is('video/mp4', 'video/webm'), () => <Film className="size-6 text-muted" />),
                                    Match.orElse(() => <MediaFigure active={false} placement={Array.headNonEmpty(placementsFor(asset, false))} presentation="thumbnail" priority={false} renderMedia={true} />),
                                )}
                            </div>
                            <span className="wrap-anywhere mt-3 block text-sm">{asset.name}</span>
                            <span className="meta-line">
                                {byteSize.format(asset.size)}
                                {asset.mime === 'application/pdf' ? ` · ${asset.pages.length} sheets` : ''}
                            </span>
                        </GridListItem>
                    )}
                </GridList>
                {Option.match(selected, {
                    onNone: (): ReactNode => <p className="hint">Select a source to preview or place it.</p>,
                    onSome: (asset): ReactNode => {
                        const uses = [...(inDraft.has(asset.id) ? ['your draft'] : []), ...(inPublished.has(asset.id) ? ['published work'] : [])];
                        return (
                            <div className="flex min-w-0 flex-col gap-4 min-[800px]:border-line min-[800px]:border-l min-[800px]:pl-6">
                                <button
                                    className="button button-ghost self-start"
                                    id={`${ids}back`}
                                    onClick={(): void => {
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
                                <div className="grid place-items-center bg-surface p-3 [--media-height:280px]">
                                    <MediaFigure active={true} placement={Array.headNonEmpty(placementsFor(asset, false))} presentation="expandable" priority={false} renderMedia={true} />
                                </div>
                                <div className="actions">
                                    {draft.entries.length > 0 && (
                                        <MenuButton label="Add to entry" onAction={({ id }): void => onUse(id, asset.id)} options={draft.entries.map((entry) => ({ id: entry.id, label: entryTitle(entry) }))}>
                                            Add to entry
                                        </MenuButton>
                                    )}
                                    <button className="button button-ghost" onClick={(): void => onHero(asset)} type="button">
                                        Use as hero
                                    </button>
                                </div>
                                <p className="hint">
                                    {byteSize.format(asset.size)}
                                    {asset.mime === 'application/pdf' ? ` · ${asset.pages.length} sheets` : ''}
                                    {uses.length > 0 ? ` · Used in ${usage.format(uses)}.` : ''}
                                </p>
                                <details className="hint">
                                    <summary className="min-h-11 cursor-pointer content-center">Source removal</summary>
                                    {uses.length > 0 && <p>Remove this file from saved and published work before deleting.</p>}
                                    <ConfirmDialog description="Permanently delete this source file. This cannot be undone." disabled={uses.length > 0} label="Delete file" onConfirm={(): void => onDelete(asset)} />
                                </details>
                            </div>
                        );
                    },
                })}
            </div>
        </section>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Library };
