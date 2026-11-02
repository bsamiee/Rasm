import type { Text } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Kerns } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Effect, Match, Option, Predicate } from 'effect';
import { accumulated, HostSettings, THOUSANDTHS } from '#/documents/settings';
import { Model } from '#/dom/model';
import { reference } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { Change } from '#/requests/change';

// --- [OPERATIONS] ----------------------------------------------------------------------

const clearKerns = Effect.fnUntraced(function* (text: Text) {
    yield* Change.write(text, { kerningMethod: '$ID/Metrics' });
    return yield* Effect.try({ try: () => text.insertionPoints.length, catch: HostError.refused(`insertion points of ${text.toSpecifier()}`) });
});
const writeKerns = Effect.fnUntraced(function* (text: Text, rows: Exclude<Kerns['kerns'], 'clear'>) {
    const { app } = yield* Model;
    const manual = app.translateKeyString('$ID/Manual');
    const points = text.insertionPoints;
    const written = yield* accumulated(
        rows,
        Effect.fnUntraced(function* ({ index, em }) {
            const refused = HostError.refused(`insertion point ${index} of ${text.toSpecifier()}`);
            const point = points.item(index);
            if (!point.isValid) {
                return yield* Unmatched.make({ subject: `insertion point among indexes 0 to ${points.length - 1}`, query: Option.some(`${index}`), candidates: [] });
            }
            const kept = yield* Effect.try({ try: () => point.kerningMethod === manual && point.kerningValue !== 0, catch: refused });
            return kept ? [] : yield* Change.write(point, { kerningValue: em * THOUSANDTHS });
        }),
    );
    return written.length;
});
const kerns = Effect.fn('kerns')(function* (payload: Kerns) {
    const text = yield* Effect.flatMap(HostSettings.target(payload.target), HostSettings.range);
    return yield* commit(
        payload.label,
        Effect.map(
            Match.value(payload.kerns).pipe(
                Match.when(Predicate.isString, () => clearKerns(text)),
                Match.orElse((rows) => writeKerns(text, rows)),
            ),
            (written) => ({ ...reference(text), written }),
        ),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { clearKerns, kerns, writeKerns };
