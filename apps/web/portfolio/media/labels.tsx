import type { Sheet } from '#/model/asset';
import type { Composition, Placement } from '#/model/placement';
import type { Entry } from '#/model/portfolio';

// --- [CONSTANTS] -----------------------------------------------------------------------

const locale = 'en';
const byteSize = new Intl.NumberFormat(locale, { notation: 'compact', style: 'unit', unit: 'byte', unitDisplay: 'narrow' });
const numeral = new Intl.NumberFormat(locale, { minimumIntegerDigits: 2, useGrouping: false }).format;
const plural = new Intl.PluralRules(locale);
const conjunction = new Intl.ListFormat(locale, { type: 'conjunction' });
const disjunction = new Intl.ListFormat(locale, { type: 'disjunction' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const quantity = (count: number, forms: Record<'one' | 'other', string>) => `${count} ${plural.select(count) === 'one' ? forms.one : forms.other}`;
const sheetLabel = ({ number, label }: typeof Sheet.Type, total: number) => [`Sheet ${number} of ${total}`, label].filter(Boolean).join(' · ');
const entryTitle = (entry: typeof Entry.Type) => entry.title || 'Untitled entry';
const placementTitle = (placement: typeof Placement.Type) => [placement.caption || placement.asset.name, placement.kind === 'pdf' && sheetLabel(placement.page, placement.asset.pages.length)].filter(Boolean).join(' · ');
const placementAlt = (placement: typeof Placement.Type) => placement.alt || placementTitle(placement);
const descriptionLabel = (placement: typeof Placement.Type) => (placement.kind === 'video' ? 'Description or transcript' : 'Detailed description');
const compositionLabel = (composition: typeof Composition.Type) => composition.items.map(placementTitle).join(' + ');

// --- [EXPORTS] -------------------------------------------------------------------------

export { byteSize, compositionLabel, conjunction, descriptionLabel, disjunction, entryTitle, numeral, placementAlt, placementTitle, quantity, sheetLabel };
