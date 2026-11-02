import type { InsertionPoint, Text } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Kerns } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Match, Option } from 'effect';
import { HostSettings } from '#/documents/settings';
import { Model } from '#/dom/model';
import { reference } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { Change } from '#/requests/change';

// --- [OPERATIONS] ----------------------------------------------------------------------

const manualKern = Model.use(({ app }) =>
    Effect.try({
        try: () => {
            const manual = app.translateKeyString('$ID/Manual');
            return (point: InsertionPoint) => (point.kerningMethod === manual ? point.kerningValue : 0);
        },
        catch: HostError.refused('$ID/Manual'),
    }),
);
const clearKerns = Effect.fnUntraced(function* (text: Text) {
    yield* Change.write(text, { kerningMethod: '$ID/Metrics' });
    return yield* Effect.try({ try: () => text.length, catch: HostError.refused(`length of ${text.toSpecifier()}`) });
});
const writeKerns = Effect.fnUntraced(function* (text: Text, rows: Exclude<Kerns['kerns'], 'clear'>) {
    const held = yield* manualKern;
    const points = text.insertionPoints;
    const refused = HostError.refused(`insertion points of ${text.toSpecifier()}`);
    const length = yield* Effect.try({ try: () => points.length, catch: refused });
    const outside = rows.filter(({ index }) => index >= length);
    yield* Array.isReadonlyArrayEmpty(outside) ? Effect.void : Unmatched.make({ subject: `insertion point among indexes 0 to ${length - 1}`, query: Option.some(outside.map(({ index }) => index).join(', ')), candidates: [] });
    const open = yield* Effect.try({ try: () => rows.filter(({ index }) => held(points.item(index)) === 0), catch: refused });
    yield* Effect.forEach(open, ({ index, em }) => Change.write(points.item(index), { kerningValue: HostSettings.thousandths(em) }), { discard: true });
    return open.length;
});
const kerns = Effect.fn('kerns')(function* (payload: Kerns) {
    const text = yield* Effect.flatMap(HostSettings.target(payload.target), HostSettings.range);
    return yield* commit(
        payload.label,
        Effect.map(
            Match.value(payload.kerns).pipe(
                Match.when('clear', () => clearKerns(text)),
                Match.orElse((rows) => writeKerns(text, rows)),
            ),
            (written) => ({ ...reference(text), written }),
        ),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { clearKerns, kerns, manualKern, writeKerns };
