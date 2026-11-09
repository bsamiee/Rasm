import { useAtomSet } from '@effect/atom-react';
import { focusElement, MenuButton } from '@rasm/ui';
import { Array, ByteSize, Function, Optic, Option, Record } from 'effect';
import { FileText, Film } from 'lucide-react';
import { useId } from 'react';
import { GridList, GridListItem } from 'react-aria-components';
import { ConfirmDialog } from '#/editor/confirm-dialog.tsx';
import { deleteRequest, draft, hero } from '#/editor/state';
import { entryTitle, format, joinDetails } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure.tsx';
import { Asset, type AssetId, type Assets } from '#/model/asset';
import { Placement } from '#/model/placement';
import { type EntryId, Portfolio } from '#/model/portfolio';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Library = ({
    assets,
    portfolio,
    published,
    assetId,
    onSelect,
    onPlace,
}: {
    assets: typeof Assets.Service;
    portfolio: typeof Portfolio.Type;
    published: typeof Portfolio.Type;
    assetId: Option.Option<typeof AssetId.Type>;
    onSelect: (id: Option.Option<typeof AssetId.Type>) => void;
    onPlace: (entryId: typeof EntryId.Type, assetId: typeof AssetId.Type) => void;
}) => {
    const idPrefix = useId();
    const setDraft = useAtomSet(draft);
    const remove = useAtomSet(deleteRequest);
    const selected = Option.flatMap(assetId, (id) => Record.get(assets, id));
    const summary = (asset: typeof Asset.Type) => [format.bytes(ByteSize.bytes(asset.size)), asset.kind === 'pdf' ? format.quantity(asset.pages.length, { one: (count) => `${count} sheet`, other: (count) => `${count} sheets` }) : undefined];
    return (
        <section className="border-line min-w-0 border-t pt-5">
            <div className="mb-4 flex items-center justify-between gap-3">
                <h3 className="text-xl tracking-tight">Library</h3>
                <span className="text-muted text-xs">{format.quantity(Object.keys(assets).length, { one: (count) => `${count} file`, other: (count) => `${count} files` })}</span>
            </div>
            <div className="editor:grid-cols-2 grid min-w-0 gap-6">
                <GridList
                    aria-label="Source files"
                    className="max-editor:data-chosen:hidden grid grid-cols-2 content-start gap-3"
                    data-chosen={Option.isSome(selected) || undefined}
                    id={`${idPrefix}library`}
                    items={Object.values(assets)}
                    onSelectionChange={(keys) => {
                        const chosen = Array.findFirst(Record.keys(assets), (id) => keys !== 'all' && keys.has(id));
                        onSelect(chosen);
                        Option.match(chosen, { onNone: Function.constVoid, onSome: () => focusElement(`${idPrefix}place`) });
                    }}
                    selectedKeys={Option.toArray(assetId)}
                    selectionMode="single"
                >
                    {(asset) => (
                        <GridListItem className="border-line focus-visible:outline-accent-text selected:border-accent-text min-w-0 cursor-pointer border p-3 outline-none focus-visible:outline-2" id={asset.id} textValue={asset.name}>
                            <div className="media-h-30 bg-background-muted pointer-events-none grid aspect-4/3 place-items-center">
                                {Asset.match(asset, {
                                    pdf: () => <FileText className="text-muted size-6" />,
                                    video: () => <Film className="text-muted size-6" />,
                                    image: (image) => <MediaFigure active={false} placement={Array.headNonEmpty(Placement.fromAsset(image))} presentation="thumbnail" priority={false} renderMedia={true} />,
                                })}
                            </div>
                            <span className="mt-3 block text-sm wrap-anywhere">{asset.name}</span>
                            <span className="meta-line">{joinDetails(summary(asset))}</span>
                        </GridListItem>
                    )}
                </GridList>
                {Option.match(selected, {
                    onNone: () => <p className="hint">Select file to preview or place</p>,
                    onSome: (asset) => {
                        const uses = Record.keys(Record.filter({ draft: portfolio, 'published work': published }, (document) => Portfolio.assetIds(document).has(asset.id)));
                        const placement = Array.headNonEmpty(Placement.fromAsset(asset));
                        return (
                            <div className="editor:border-line editor:border-l editor:pl-6 flex min-w-0 flex-col gap-4">
                                <button
                                    className="button button-ghost self-start"
                                    onClick={() => {
                                        onSelect(Option.none());
                                        focusElement(`${idPrefix}library`);
                                    }}
                                    type="button"
                                >
                                    Back to files
                                </button>
                                <h4 className="text-lg tracking-tight wrap-anywhere outline-none" id={`${idPrefix}place`} tabIndex={-1}>
                                    {asset.name}
                                </h4>
                                <div className="media-h-70 bg-background-muted grid place-items-center p-3">
                                    <MediaFigure active={true} placement={placement} presentation="expandable" priority={false} renderMedia={true} />
                                </div>
                                <div className="actions">
                                    {portfolio.entries.length > 0 && (
                                        <MenuButton onAction={({ id }) => onPlace(id, asset.id)} options={portfolio.entries.map((entry) => ({ id: entry.id, label: entryTitle(entry) }))}>
                                            Add to entry
                                        </MenuButton>
                                    )}
                                    <button className="button button-ghost" onClick={() => setDraft(Optic.replace(hero, placement))} type="button">
                                        Use as hero
                                    </button>
                                </div>
                                <p className="hint">{joinDetails([...summary(asset), uses.length > 0 ? `Used in ${format.list(uses)}` : undefined])}</p>
                                <details className="hint">
                                    <summary className="min-h-11 cursor-pointer content-center">Deletion</summary>
                                    {uses.length > 0 && <p>Remove its placements and publish before deleting</p>}
                                    <ConfirmDialog description="Deletion is permanent" disabled={uses.length > 0} label="Delete file" onConfirm={() => remove(asset.id)} />
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
