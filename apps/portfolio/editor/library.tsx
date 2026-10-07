import type { ReactNode } from 'react';
import type { Asset, AssetCollection } from '../model/asset.ts';
import { type Portfolio, placements } from '../model/document.ts';
import { ConfirmDialog } from './controls.tsx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const megabyte = 1_000_000;
const usage = new Intl.ListFormat('en', { type: 'conjunction' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const assetIds = (portfolio: typeof Portfolio.Type): ReadonlySet<string> => new Set(placements(portfolio).map((placement) => placement.asset.id));

// --- [COMPOSITION] ---------------------------------------------------------------------

function Library({ assets, draft, published, onHero, onDelete }: { assets: typeof AssetCollection.Type; draft: typeof Portfolio.Type; published: typeof Portfolio.Type; onHero: (asset: typeof Asset.Type) => void; onDelete: (asset: typeof Asset.Type) => void }): ReactNode {
    const inDraft = assetIds(draft);
    const inPublished = assetIds(published);
    return (
        <div>
            {Object.values(assets).map((asset) => {
                const uses = [...(inDraft.has(asset.id) ? ['your draft'] : []), ...(inPublished.has(asset.id) ? ['published work'] : [])];
                return (
                    <div className="flex items-center justify-between gap-5 border-line border-t py-5 max-sm:flex-col max-sm:items-start max-sm:gap-3" key={asset.id}>
                        <div className="min-w-0">
                            <strong className="wrap-anywhere font-normal text-sm">{asset.name}</strong>
                            <span className="meta-line">
                                {(asset.size / megabyte).toFixed(1)} MB{asset.mime === 'application/pdf' ? ` · ${asset.pages.length} sheets` : ''}
                            </span>
                            {uses.length > 0 && <span className="meta-line">Used in {usage.format(uses)}. Remove its placements, save, and publish before deleting.</span>}
                        </div>
                        <div className="actions">
                            <button className="button button-outline" onClick={(): void => onHero(asset)} type="button">
                                Use as hero
                            </button>
                            <ConfirmDialog description="Permanently delete this source file. This cannot be undone." disabled={uses.length > 0} label="Delete file" onConfirm={(): void => onDelete(asset)} />
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Library };
