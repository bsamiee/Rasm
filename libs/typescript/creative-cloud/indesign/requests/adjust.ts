import type { Characters, InDesignDOMObject, InsertionPoint, Text, TextDefault } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Adjust } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, flow, Match, Option, Record, Schema } from 'effect';
import { HostSettings } from '#/documents/settings';
import { reference } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { Change } from '#/requests/change';
import { manualKern } from '#/requests/kerns';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{M}+$/v;

// --- [OPERATIONS] ----------------------------------------------------------------------

const added = <K extends string>(target: InDesignDOMObject<unknown> & Record<NoInfer<K>, number>, amounts: Record.ReadonlyRecord<K, number>) =>
    Effect.flatMap(Effect.try({ try: () => Record.map(amounts, (amount, member) => target[member] + amount), catch: HostError.refused(`${Record.keys(amounts).join(', ')} of ${target.toSpecifier()}`) }), (values) => Change.write(target, values));
const baseLetter = (characters: Characters, at: number): Option.Option<number> => {
    if (at < 0) {
        return Option.none();
    }
    return MARK.test(String(characters.item(at).contents)) ? baseLetter(characters, at - 1) : Option.some(at);
};
const pointAfterBase = Effect.fnUntraced(function* (text: Text) {
    const specifier = text.toSpecifier();
    const point = yield* Effect.try({
        try: (): Option.Option<InsertionPoint> => {
            const last = text.insertionPoints.lastItem();
            const container = last.parent;
            return Option.map(baseLetter(container.characters, last.index - 1), (base) => container.insertionPoints.item(base + 1));
        },
        catch: HostError.refused(`base letter before ${specifier}`),
    });
    return yield* Effect.fromOption(point, () => Unmatched.make({ subject: 'base letter', query: Option.some(specifier), candidates: [] }));
});
const kernBy = Effect.fnUntraced(function* (text: Text, em: number) {
    const [point, held] = yield* Effect.all([pointAfterBase(text), manualKern]);
    const current = yield* Effect.try({ try: () => held(point), catch: HostError.refused(`kerning of ${point.toSpecifier()}`) });
    return yield* Change.write(point, { kerningValue: current + HostSettings.thousandths(em) });
});
const adjustText = (target: Text | TextDefault, deltas: Adjust['deltas']) =>
    Effect.map(
        Effect.forEach(deltas, (delta) =>
            Match.valueTags(delta, {
                Kerning: ({ em }) => Effect.flatMap(HostSettings.range(target), (text) => kernBy(text, em)),
                Tracking: ({ em }) => added(target, { tracking: HostSettings.thousandths(em) }),
                BaselineShift: ({ points }) => added(target, { baselineShift: points }),
                Marks: ({ x, y }) => Effect.flatMap(Effect.orDie(Schema.encodeEffect(HostSettings.fields.marks)({ x, y })), (amounts) => added(target, amounts)),
            }),
        ),
        flow(Array.flatten, Array.dedupe),
    );
const adjust = Effect.fn('adjust')(function* (payload: Adjust) {
    const target = yield* HostSettings.target(payload.target);
    return yield* commit(
        payload.label,
        Effect.map(adjustText(target, payload.deltas), (applied) => ({ ...reference(target), applied })),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { adjust, adjustText };
