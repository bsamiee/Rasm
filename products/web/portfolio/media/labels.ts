import { Format } from '@rasm/ui/format';
import { Array, Function, Option, Predicate, Result, String } from 'effect';
import type { Sheet } from '#/model/asset';
import type { Composition } from '#/model/placement';
import type { Entry } from '#/model/portfolio';

// --- [CONSTANTS] -----------------------------------------------------------------------

const format = new Format({ locales: 'en' });
const choiceLayout = { orientation: 'horizontal', radios: 3 } as const;
const confirmation = { cancel: 'Keep', description: 'Source files stay in library. Published work changes on next publish', trigger: { className: 'button button-ghost' } } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const literalOptions = <const T extends string>(literals: readonly T[]) => literals.map((id) => ({ id, label: String.capitalize(id) }));
const joinDetails = (parts: readonly (string | undefined)[]) => parts.filter(Predicate.isNotUndefined).join(' · ');
const sheetLabel = ({ number, label }: Pick<Sheet, 'number' | 'label'>, total: number) => joinDetails([`Sheet ${number} of ${total}`, label]);
const entryTitle = ({ title }: Entry) => title ?? 'Untitled entry';
const placementTitle = (placement: Composition['items'][number]) => joinDetails([placement.caption ?? placement.asset.name, placement.kind === 'pdf' ? sheetLabel(placement.page, placement.asset.pages.length) : undefined]);
const placementAlt = (placement: Composition['items'][number]) => placement.alt ?? placementTitle(placement);
const descriptionLabel = ({ kind }: Composition['items'][number]) => (kind === 'video' ? 'Description or transcript' : 'Detailed description');
const compositionLabel = ({ items }: Composition) => format.list(items.map(placementTitle));
const compositionCaption = ({ items }: Composition) => Option.liftPredicate(format.list(Array.filterMap(items, ({ caption }) => Result.fromNullishOr(caption, Function.constVoid))), String.isNonEmpty);

// --- [EXPORTS] -------------------------------------------------------------------------

export { choiceLayout, compositionCaption, compositionLabel, confirmation, descriptionLabel, entryTitle, format, joinDetails, literalOptions, placementAlt, placementTitle, sheetLabel };
