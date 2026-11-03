import { Format } from '@rasm/ui/format';
import { Array, Option, String } from 'effect';
import type { Sheet } from '#/model/asset.ts';
import type { Composition, Placement } from '#/model/placement.ts';
import type { Entry } from '#/model/portfolio.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const format = new Format({ locales: 'en' });
const choice = { orientation: 'horizontal', radios: 3 } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const options = <const T extends string>(literals: readonly T[]) => literals.map((id) => ({ id, label: String.capitalize(id) }));
const text = (value: string | undefined) => Option.fromUndefinedOr(value).pipe(Option.map(String.trim), Option.filter(String.isNonEmpty));
const details = (parts: readonly (string | undefined)[]) => Array.getSomes(parts.map(text)).join(' · ');
const sheetLabel = ({ number, label }: typeof Sheet.Type, total: number) => details([`Sheet ${number} of ${total}`, label]);
const entryTitle = (entry: typeof Entry.Type) => Option.getOrElse(text(entry.title), () => 'Untitled entry');
const placementTitle = (placement: typeof Placement.Type) => details([Option.getOrElse(text(placement.caption), () => placement.asset.name), placement.kind === 'pdf' ? sheetLabel(placement.page, placement.asset.pages.length) : undefined]);
const placementAlt = (placement: typeof Placement.Type) => Option.getOrElse(text(placement.alt), () => placementTitle(placement));
const descriptionLabel = (placement: typeof Placement.Type) => (placement.kind === 'video' ? 'Description or transcript' : 'Detailed description');
const compositionLabel = (composition: typeof Composition.Type) => format.list(composition.items.map(placementTitle));

// --- [EXPORTS] -------------------------------------------------------------------------

export { choice, compositionLabel, descriptionLabel, details, entryTitle, format, options, placementAlt, placementTitle, sheetLabel };
