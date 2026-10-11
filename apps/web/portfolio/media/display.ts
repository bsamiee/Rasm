import type { Asset, ImageAsset } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { type Composition, type Placement, placementDimensions } from '../model/placement.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const byteSize = new Intl.NumberFormat('en', { notation: 'compact', style: 'unit', unit: 'byte', unitDisplay: 'narrow' });
const numeral = new Intl.NumberFormat('en', { minimumIntegerDigits: 2, useGrouping: false }).format;

// --- [OPERATIONS] ----------------------------------------------------------------------

const mediaRatio = (placement: typeof Placement.Type): number => {
    const { width, height } = placement.kind === 'image' && placement.framing.mode === 'cover' ? { width: 4, height: 3 } : placementDimensions(placement);
    return width / height;
};
const mediaUrl = (asset: typeof Asset.Type, width?: number): string => `/api/media/${asset.id}${width === undefined ? '' : `/${width}`}`;
const imageSourceSet = (asset: ImageAsset): string | undefined => asset.renditions && [...asset.renditions.map(({ width }) => `${mediaUrl(asset, width)} ${width}w`), `${mediaUrl(asset)} ${asset.width}w`].join(', ');
const entryTitle = (entry: typeof Entry.Type): string => entry.title || 'Untitled entry';
const placementTitle = (placement: typeof Placement.Type): string => placement.caption || placement.asset.name;
const placementAlt = (placement: typeof Placement.Type): string => placement.alt || placementTitle(placement);
const placementLabel = (placement: typeof Placement.Type): string => `${placement.asset.name}${placement.kind === 'pdf' ? ` · Sheet ${placement.page.number}` : ''}`;
const compositionLabel = (composition: typeof Composition.Type): string => composition.items.map((placement) => placement.caption || placementLabel(placement)).join(' + ');

// --- [EXPORTS] -------------------------------------------------------------------------

export { byteSize, compositionLabel, entryTitle, imageSourceSet, mediaRatio, mediaUrl, numeral, placementAlt, placementLabel, placementTitle };
