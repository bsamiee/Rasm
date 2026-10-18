import type { Asset, ImageAsset, Sheet } from '../model/asset.ts';
import type { Entry } from '../model/document.ts';
import { type Composition, type Placement, placementDimensions } from '../model/placement.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const byteSize = new Intl.NumberFormat('en', { notation: 'compact', style: 'unit', unit: 'byte', unitDisplay: 'narrow' });
const numeral = new Intl.NumberFormat('en', { minimumIntegerDigits: 2, useGrouping: false }).format;
const plural = new Intl.PluralRules('en');
const entryKind = { project: 'Project', study: 'Study' } as const satisfies Record<typeof Entry.Type.kind, string>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const quantity = (count: number, forms: Record<'one' | 'other', string>) => `${count} ${plural.select(count) === 'one' ? forms.one : forms.other}`;
const mediaRatio = (placement: typeof Placement.Type) => {
    const { width, height } = placement.kind === 'image' && placement.framing.mode === 'cover' ? { width: 4, height: 3 } : placementDimensions(placement);
    return width / height;
};
const mediaUrl = (asset: typeof Asset.Type, width?: number) => `/api/media/${asset.id}${width === undefined ? '' : `/${width}`}`;
const imageSourceSet = (asset: ImageAsset) => asset.renditions && [...asset.renditions.map(({ width }) => `${mediaUrl(asset, width)} ${width}w`), `${mediaUrl(asset)} ${asset.width}w`].join(', ');
const sheetLabel = ({ number, label }: typeof Sheet.Type, total: number) => [`Sheet ${number} of ${total}`, label].filter(Boolean).join(' · ');
const entryTitle = (entry: typeof Entry.Type) => entry.title || 'Untitled entry';
const placementTitle = (placement: typeof Placement.Type) => [placement.caption || placement.asset.name, placement.kind === 'pdf' && sheetLabel(placement.page, placement.asset.pages.length)].filter(Boolean).join(' · ');
const placementAlt = (placement: typeof Placement.Type) => placement.alt || placementTitle(placement);
const descriptionLabel = (placement: typeof Placement.Type) => (placement.kind === 'video' ? 'Description or transcript' : 'Detailed description');
const compositionLabel = (composition: typeof Composition.Type) => composition.items.map(placementTitle).join(' + ');

// --- [EXPORTS] -------------------------------------------------------------------------

export { byteSize, compositionLabel, descriptionLabel, entryKind, entryTitle, imageSourceSet, mediaRatio, mediaUrl, numeral, placementAlt, placementTitle, quantity, sheetLabel };
