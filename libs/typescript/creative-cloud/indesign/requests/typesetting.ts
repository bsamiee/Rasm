import type { InsertionPoint, Story, Text, TextDefault } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Adjust, Kerns, Typeset } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Record, Schema, Struct } from 'effect';
import { HostSettings } from '#/documents/settings';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { Change } from '#/requests/change';

// --- [TYPES] ---------------------------------------------------------------------------

type Target = Text | TextDefault;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{M}+$/v;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TARGETS]

const target = Effect.fnUntraced(function* (path: string) {
    const [{ app, owning, text }, specifier] = yield* Effect.all([Model, Schema.decodeUnknownEffect(Specifier.Path)(path)]);
    if (specifier.root) {
        return app.textDefaults;
    }
    const found = yield* Specifier.object(specifier);
    if (text(found)) {
        return found;
    }
    return yield* owning<Story>('texts')(found) ? Effect.succeed(found.texts.item(0)) : Unmatched.make({ subject: 'text', query: Option.some(path), candidates: [] });
});
const range = (held: Target) => Model.use(({ text }) => Effect.filterOrFail(Effect.succeed(held), text, () => Unmatched.make({ subject: 'text range', query: Option.some(held.toSpecifier()), candidates: [] })));

// --- [WRITES]

const manualKern = Model.use(({ app }) =>
    Effect.try({
        try: () => {
            const manual = app.translateKeyString('$ID/Manual');
            return (point: InsertionPoint, method = point.kerningMethod) => (method === manual ? point.kerningValue : 0);
        },
        catch: HostError.refused('$ID/Manual'),
    }),
);
const pointAfterBase = (text: Text) =>
    Effect.try({
        try: () => {
            const last = text.insertionPoints.lastItem();
            const container = last.parent;
            const marks = Array.unfold(last.index - 1, (at) => (at >= 0 && MARK.test(String(container.characters.item(at).contents)) ? Option.some([at, at - 1] as const) : Option.none()));
            return last.index > marks.length ? Option.some(container.insertionPoints.item(last.index - marks.length)) : Option.none();
        },
        catch: HostError.refused(`base letter before ${text.toSpecifier()}`),
    }).pipe(Effect.flatMap(Effect.fromOption(() => Unmatched.make({ subject: 'base letter', query: Option.some(text.toSpecifier()), candidates: [] }))));
const kernBy = Effect.fnUntraced(function* (text: Text, em: number) {
    const [held, point] = yield* Effect.all([manualKern, pointAfterBase(text)]);
    const current = yield* Effect.try({ try: () => held(point), catch: HostError.refused(`kerning of ${point.toSpecifier()}`) });
    return yield* Change.write(point, { kerningValue: current + HostSettings.thousandths(em) });
});
const adjustText = Effect.fnUntraced(function* (held: Target, { kerning = 0, tracking = 0, baselineShift = 0, marks = { x: 0, y: 0 } }: Adjust['deltas']) {
    const [{ text }, offsets] = yield* Effect.all([Model, Effect.orDie(Schema.encodeEffect(HostSettings.fields.marks)(marks))]);
    const amounts = Record.toEntries({ tracking: HostSettings.thousandths(tracking), baselineShift, ...offsets }).filter(([, amount]) => amount !== 0);
    const sums = yield* Effect.try({
        try: () => Array.map(text(held) ? held.textStyleRanges.everyItem().getElements() : [held], (run) => [run, Record.fromIterableWith(amounts, ([member, amount]) => [member, run[member] + amount])] as const),
        catch: HostError.refused(`style runs of ${held.toSpecifier()}`),
    });
    const written = yield* Effect.forEach(sums, ([run, values]) => Change.write(run, values));
    const kerned = kerning === 0 ? [] : yield* Effect.flatMap(range(held), (letters) => kernBy(letters, kerning));
    return Array.dedupe(Array.flatten([...written, kerned]));
});
const writeKerns = Effect.fnUntraced(function* (text: Text, rows: Kerns['kerns']) {
    const held = yield* manualKern;
    const refused = HostError.refused(`insertion points of ${text.toSpecifier()}`);
    const methods = yield* Effect.try({ try: () => text.insertionPoints.everyItem().kerningMethod, catch: refused });
    const outside = rows.filter(({ index }) => index >= methods.length);
    yield* Array.isReadonlyArrayEmpty(outside) ? Effect.void : Unmatched.make({ subject: `insertion point among indexes 0 to ${methods.length - 1}`, query: Option.some(outside.map(({ index }) => index).join(', ')), candidates: [] });
    const open = yield* Effect.try({ try: () => rows.filter(({ index }) => held(text.insertionPoints.item(index), methods[index]) === 0), catch: refused });
    yield* Effect.forEach(open, ({ index, em }) => Change.write(text.insertionPoints.item(index), { kerningValue: HostSettings.thousandths(em) }), { discard: true });
    return open.length;
});

// --- [HANDLERS]

const adjust = Effect.fn('adjust')(function* (payload: Adjust) {
    const held = yield* target(payload.target);
    return yield* commit(
        payload.label,
        Effect.map(adjustText(held, payload.deltas), (written) => ({ ...reference(held), written })),
    );
});
const kerns = Effect.fn('kerns')(function* (payload: Kerns) {
    const text = yield* Effect.flatMap(target(payload.target), range);
    return yield* commit(
        payload.label,
        Effect.map(writeKerns(text, payload.kerns), (written) => ({ ...reference(text), written })),
    );
});
const typeset = Effect.fn('typeset')(function* (payload: Typeset) {
    const held = yield* target(payload.target);
    yield* Effect.transposeOption(Option.map(payload.settings, (given) => commit(`Set ${Struct.keys(given).join(', ')} of ${payload.target}`, HostSettings.write(held, given))));
    return yield* HostSettings.read(held);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { adjust, adjustText, kerns, typeset, writeKerns };
