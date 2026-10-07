import type { Entry } from '../model/document.ts';
import { type Composition, type Placement, placementDimensions } from '../model/placement.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const mediaRatio = (placement: typeof Placement.Type): number => {
    const { width, height } = placement.kind === 'image' && placement.framing.mode === 'cover' ? { width: 4, height: 3 } : placementDimensions(placement);
    return width / height;
};
const mediaUrl = (placement: typeof Placement.Type): string => `/api/media/${placement.asset.id}`;
const entryTitle = (entry: typeof Entry.Type): string => entry.title || 'Untitled entry';
const placementTitle = (placement: typeof Placement.Type): string => placement.caption || placement.asset.name;
const placementAlt = (placement: typeof Placement.Type): string => placement.alt || placementTitle(placement);
const placementLabel = (placement: typeof Placement.Type): string => `${placement.asset.name}${placement.kind === 'pdf' ? ` · Sheet ${placement.page.number}` : ''}`;
const compositionLabel = (composition: typeof Composition.Type): string => composition.items.map((placement) => placement.caption || placementLabel(placement)).join(' + ');
const sheetLabel = (sheet: Extract<typeof Placement.Type, { kind: 'pdf' }>): string => `${placementLabel(sheet)} of ${sheet.asset.pages.length}${sheet.page.label ? ` · ${sheet.page.label}` : ''}`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { compositionLabel, entryTitle, mediaRatio, mediaUrl, placementAlt, placementLabel, placementTitle, sheetLabel };
