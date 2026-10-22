import { useAtomSet } from '@effect/atom-react';
import { focus } from '@rasm/ui/focus.ts';
import { MenuButton } from '@rasm/ui/menu-button.tsx';
import { Array, Match, Optic, Option, Record, Schema } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { FileText, Film } from 'lucide-react';
import { useId } from 'react';
import { GridList, GridListItem } from 'react-aria-components';
import { byteSize, conjunction, entryTitle, quantity } from '../media/labels.ts';
import { MediaFigure } from '../media/media-figure.tsx';
import { type Asset, type AssetCollection, PdfAsset, VideoAsset } from '../model/asset.ts';
import { placementsFor } from '../model/placement.ts';
import { assetIds, type Portfolio } from '../model/portfolio.ts';
import { ConfirmDialog } from './confirm-dialog.tsx';
import { deleteRequest, draft, hero } from './state.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Library = ({
    assets,
    portfolio,
    published,
    sourceId,
    onSelect,
    onUse,
}: {
    assets: typeof AssetCollection.Type;
    portfolio: typeof Portfolio.Type;
    published: typeof Portfolio.Type;
    sourceId: Option.Option<string>;
    onSelect: (id: Option.Option<string>) => void;
    onUse: (entryId: string, assetId: string) => void;
}) => {
    const ids = useId();
    const setDraft = useAtomSet(draft);
    const remove = useAtomSet(deleteRequest);
    const selected = Option.flatMap(sourceId, (id) => Record.get(assets, id));
    const summary = (asset: typeof Asset.Type) => [byteSize.format(asset.size), ...(Schema.is(PdfAsset)(asset) ? [quantity(asset.pages.length, { one: 'sheet', other: 'sheets' })] : [])];
    return (
        <section className="border-line min-w-0 border-t pt-5">
            <div className="mb-4 flex items-center justify-between gap-3">
                <h3 className="text-xl tracking-tight">Library</h3>
                <span className="text-muted text-xs">{quantity(Object.keys(assets).length, { one: 'file', other: 'files' })}</span>
            </div>
            <div className="editor:grid-cols-2 grid min-w-0 gap-6">
                <GridList aria-label="Source files" className="max-editor:data-chosen:hidden grid grid-cols-2 content-start gap-3" data-chosen={Option.isSome(selected) || undefined} dependencies={[sourceId]} id={`${ids}library`} items={Object.values(assets)} selectionMode="none">
                    {(asset) => (
                        <GridListItem
                            className="border-line focus-visible:outline-accent-text data-current:border-accent-text min-w-0 cursor-pointer border p-3 outline-none focus-visible:outline-2"
                            data-current={Option.contains(sourceId, asset.id) || undefined}
                            id={asset.id}
                            onAction={() => {
                                onSelect(Option.some(asset.id));
                                focus(`${ids}source`);
                            }}
                            textValue={asset.name}
                        >
                            <div className="media-h-30 bg-surface pointer-events-none grid aspect-4/3 place-items-center">
                                {Match.value(asset).pipe(
                                    Match.when({ mime: Match.is(...PdfAsset.fields.mime.literals) }, () => <FileText className="text-muted size-6" />),
                                    Match.when({ mime: Match.is(...VideoAsset.fields.mime.literals) }, () => <Film className="text-muted size-6" />),
                                    Match.orElse((image) => <MediaFigure active={false} placement={Array.headNonEmpty(placementsFor(image))} presentation="thumbnail" priority={false} renderMedia={true} />),
                                )}
                            </div>
                            <span className="mt-3 block text-sm wrap-anywhere">{asset.name}</span>
                            <span className="meta-line">{summary(asset).join(' · ')}</span>
                        </GridListItem>
                    )}
                </GridList>
                {Option.match(selected, {
                    onNone: () => <p className="hint">Select a source to preview or place it.</p>,
                    onSome: (asset) => {
                        const uses = [...(assetIds(portfolio).has(asset.id) ? ['your draft'] : []), ...(assetIds(published).has(asset.id) ? ['published work'] : [])];
                        return (
                            <div className="editor:border-line editor:border-l editor:pl-6 flex min-w-0 flex-col gap-4">
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
                                <h4 className="text-lg tracking-tight wrap-anywhere outline-none" id={`${ids}source`} tabIndex={-1}>
                                    {asset.name}
                                </h4>
                                <div className="media-h-70 bg-surface grid place-items-center p-3">
                                    <MediaFigure active={true} placement={Array.headNonEmpty(placementsFor(asset))} presentation="expandable" priority={false} renderMedia={true} />
                                </div>
                                <div className="actions">
                                    {portfolio.entries.length > 0 && (
                                        <MenuButton label="Add to entry" onAction={({ id }) => onUse(id, asset.id)} options={portfolio.entries.map((entry) => ({ id: entry.id, label: entryTitle(entry) }))}>
                                            Add to entry
                                        </MenuButton>
                                    )}
                                    <button className="button button-ghost" onClick={() => setDraft(AsyncResult.map(Optic.replace(hero, Array.headNonEmpty(placementsFor(asset)))))} type="button">
                                        Use as hero
                                    </button>
                                </div>
                                <p className="hint">{[...summary(asset), ...(uses.length > 0 ? [`Used in ${conjunction.format(uses)}.`] : [])].join(' · ')}</p>
                                <details className="hint">
                                    <summary className="min-h-11 cursor-pointer content-center">Source removal</summary>
                                    {uses.length > 0 && <p>Remove this file from saved and published work before deleting.</p>}
                                    <ConfirmDialog description="Permanently delete this source file. This cannot be undone." disabled={uses.length > 0} label="Delete file" onConfirm={() => remove(asset.id)} />
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
