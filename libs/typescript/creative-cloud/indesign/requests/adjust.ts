import type { InDesignDOMObject, InsertionPoint, Text, TextDefault } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Adjust } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Match, Option } from 'effect';
import { accumulated, HostSettings, THOUSANDTHS } from '#/documents/settings';
import { reference } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { Change } from '#/requests/change';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{Mn}+$/v;

// --- [OPERATIONS] ----------------------------------------------------------------------

const added = <K extends string>(target: InDesignDOMObject<unknown> & Record<K, number>, member: K, amount: number) => Effect.flatMap(Effect.try({ try: () => target[member], catch: HostError.refused(`${target.toSpecifier()} ${member}`) }), (current) => Change.write(target, { [member]: current + amount }));
const baseLetter = (characters: { readonly item: (index: number) => { readonly contents: unknown } }, at: number): Option.Option<number> => {
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
const adjustText = (target: Text | TextDefault, deltas: Adjust['deltas']) =>
    Effect.map(
        accumulated(deltas, (delta) =>
            Match.valueTags(delta, {
                Kerning: ({ em }) =>
                    HostSettings.range(target).pipe(
                        Effect.flatMap(pointAfterBase),
                        Effect.flatMap((point) => Change.write(point, { kerningValue: Option.getOrElse(Option.liftThrowable(() => point.kerningValue)(), () => 0) + em * THOUSANDTHS })),
                    ),
                Tracking: ({ em }) => added(target, 'tracking', em * THOUSANDTHS),
                BaselineShift: ({ points }) => added(target, 'baselineShift', points),
                Marks: ({ x, y }) => Effect.flatMap(HostSettings.read(target), ({ marks }) => HostSettings.write(target, { marks: { x: marks.x + x, y: marks.y + y } })),
            }),
        ),
        Array.dedupe,
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
