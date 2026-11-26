import { useAtom, useAtomSet, useAtomValue } from '@effect/atom-react';
import { MenuButton } from '@rasm/ui/menu-button';
import { ModalDialog } from '@rasm/ui/modal-dialog';
import { Array, ByteSize, Optic, Option, Record } from 'effect';
import { FileText, Film } from 'lucide-react';
import { useRef } from 'react';
import { GridList, GridListItem } from 'react-aria-components';
import { flushSync } from 'react-dom';
import { chosen, deleteRequest, draft, hero } from '#/editor/state';
import { confirmation, entryTitle, format, joinDetails } from '#/media/labels';
import { MediaFigure } from '#/media/media-figure.tsx';
import { Asset, type Assets } from '#/model/asset';
import { Placement } from '#/model/placement';
import { type Entry, Portfolio } from '#/model/portfolio';
import { published } from '#/site/state';

// --- [OPERATIONS] ----------------------------------------------------------------------

const summary = Asset.matchOrElse({ pdf: ({ size, pages }) => [format.bytes(ByteSize.bytes(size)), format.quantity(pages.length, { one: (count) => `${count} sheet`, other: (count) => `${count} sheets` })] }, ({ size }) => [format.bytes(ByteSize.bytes(size))]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Library = ({ assets, portfolio, onPlace }: { readonly assets: Assets['Service']; readonly portfolio: Portfolio; readonly onPlace: (entryId: Entry['id'], assetId: keyof Assets['Service']) => void }) => {
    const files = useRef<HTMLDivElement>(null);
    const detail = useRef<HTMLHeadingElement>(null);
    const [assetId, setAssetId] = useAtom(chosen);
    const live = useAtomValue(published).portfolio;
    const setDraft = useAtomSet(draft);
    const choose = (next: Option.Option<keyof Assets['Service']>, target: typeof files | typeof detail) => {
        flushSync(() => setAssetId(next));
        target.current?.focus();
    };
    const remove = useAtomSet(deleteRequest);
    const selected = Option.flatMap(assetId, (id) => Record.get(assets, id));
    return (
        <section className="library">
            <header>
                <h3>Library</h3>
                <span>{format.quantity(Object.keys(assets).length, { one: (count) => `${count} file`, other: (count) => `${count} files` })}</span>
            </header>
            <div className="library-layout">
                <GridList
                    aria-label="Source files"
                    className="library-files"
                    data-chosen={Option.isSome(selected) || undefined}
                    items={Object.values(assets)}
                    onSelectionChange={(keys) =>
                        choose(
                            Array.findFirst(Record.keys(assets), (id) => keys !== 'all' && keys.has(id)),
                            detail,
                        )
                    }
                    ref={files}
                    selectedKeys={Option.toArray(assetId)}
                    selectionMode="single"
                >
                    {(asset) => (
                        <GridListItem id={asset.id} textValue={asset.name}>
                            <div>
                                {Asset.match(asset, {
                                    pdf: () => <FileText size={24} />,
                                    video: () => <Film size={24} />,
                                    image: (image) => <MediaFigure active={false} placement={Array.headNonEmpty(Placement.fromAsset(image))} presentation="thumbnail" priority={false} renderMedia />,
                                })}
                            </div>
                            <span className="asset-name">{asset.name}</span>
                            <span className="meta-line">{joinDetails(summary(asset))}</span>
                        </GridListItem>
                    )}
                </GridList>
                {Option.match(selected, {
                    onNone: () => <p className="hint">Select file to preview or place</p>,
                    onSome: (asset) => {
                        const uses = Record.keys(Record.filter({ draft: portfolio, 'published work': live }, (document) => Portfolio.assetIds(document).has(asset.id)));
                        const placement = Array.headNonEmpty(Placement.fromAsset(asset));
                        return (
                            <div className="library-detail">
                                <button className="button button-ghost" onClick={() => choose(Option.none(), files)} type="button">
                                    Back to files
                                </button>
                                <h4 ref={detail} tabIndex={-1}>
                                    {asset.name}
                                </h4>
                                <div className="library-preview">
                                    <MediaFigure active placement={placement} presentation="expandable" priority={false} renderMedia />
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
                                    <summary>Deletion</summary>
                                    {uses.length > 0 && <p>Remove its placements and publish before deleting</p>}
                                    <ModalDialog.Confirm {...confirmation} description="Deletion is permanent" isDisabled={uses.length > 0} label="Delete file" onConfirm={() => remove(asset.id)} />
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
