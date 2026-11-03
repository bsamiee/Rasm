import type { InsertionPoint, Text, TextDefault } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { Adjust, Kerns, Runs, Typeset } from '@rasm/automation/typesetting';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Predicate, Record, Schema } from 'effect';
import { HostSettings } from '#/documents/settings';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { commit, written } from '#/dom/transactions';
import { write } from '#/requests/change';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{M}+$/v;

// --- [MODELS] --------------------------------------------------------------------------

const Deltas = Schema.Struct({ kerning: HostSettings.Thousandths, tracking: HostSettings.Thousandths, baselineShift: Schema.Finite, marks: HostSettings.fields.marks });
const Kerned = Schema.Array(Schema.Struct({ index: Schema.Int, kerning: HostSettings.Thousandths }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TARGETS]

const resolveTarget = Effect.fnUntraced(function* (path: string) {
    const [{ app, textContainer, text, textDefault }, specifier] = yield* Effect.all([Model, Schema.decodeUnknownEffect(Specifier.Path)(path)]);
    const found = specifier.root ? app.textDefaults : yield* Model.located(specifier);
    const target = text(found) || textDefault(found) || !textContainer(found) ? found : found.texts.item(0);
    return yield* text(target) || textDefault(target) ? Effect.succeed(target) : Unmatched.make({ subject: 'text', query: Option.some(path) });
});
const textRange = (target: Text | TextDefault) => Model.use(({ text }) => Effect.filterOrFail(Effect.succeed(target), text, () => Unmatched.make({ subject: 'text range', query: Option.some(target.toSpecifier()) })));

// --- [READS]

const manualKerning = Model.use(({ app }) => Effect.try({ try: () => app.translateKeyString('$ID/Manual'), catch: HostError.refused('$ID/Manual') }));
const pointAfterBase = (text: Text) =>
    Effect.try({
        try: () => {
            const last = text.insertionPoints.lastItem();
            const container = last.parent;
            const marks = Array.unfold(last.index - 1, (index) => (index >= 0 && MARK.test(String(container.characters.item(index).contents)) ? Option.some([index, index - 1] as const) : Option.none()));
            return last.index > marks.length ? Option.some(container.insertionPoints.item(last.index - marks.length)) : Option.none();
        },
        catch: HostError.refused(`base letter before ${text.toSpecifier()}`),
    }).pipe(Effect.flatMap(Effect.fromOption(() => Unmatched.make({ subject: 'base letter', query: Option.some(text.toSpecifier()) }))));

// --- [HANDLERS]

const adjust = Effect.fn('adjust')(function* (payload: Adjust) {
    const { kerning = 0, tracking = 0, baselineShift = 0, marks = { x: 0, y: 0 } } = payload.deltas;
    const [{ text }, target, manual, encoded] = yield* Effect.all([Model, resolveTarget(payload.target), manualKerning, Effect.orDie(Schema.encodeEffect(Deltas)({ kerning, tracking, baselineShift, marks }))]);
    const amounts = Record.toEntries({ tracking: encoded.tracking, baselineShift: encoded.baselineShift, ...encoded.marks }).filter(([, amount]) => amount !== 0);
    const points: readonly InsertionPoint[] = kerning === 0 ? [] : [yield* Effect.flatMap(textRange(target), pointAfterBase)];
    const sums = yield* Effect.try({
        try: () => Array.map(text(target) ? target.textStyleRanges.everyItem().getElements() : [target], (run) => [run, Record.fromIterableWith(amounts, ([member, amount]) => [member, run[member] + amount])] as const),
        catch: HostError.refused(`style runs of ${target.toSpecifier()}`),
    });
    const kerned = yield* Effect.try({ try: () => Array.map(points, (point) => [point, { kerningValue: (point.kerningMethod === manual ? point.kerningValue : 0) + encoded.kerning }] as const), catch: HostError.refused(`kerning of ${target.toSpecifier()}`) });
    return yield* written(payload.label, target, Effect.forEach([...sums, ...kerned], ([run, values]) => write(run, values)).pipe(Effect.map((names) => Array.dedupe(Array.flatten(names)))));
});
const kerns = Effect.fn('kerns')(function* (payload: Kerns) {
    const [text, manual, rows] = yield* Effect.all([Effect.flatMap(resolveTarget(payload.target), textRange), manualKerning, Effect.orDie(Schema.encodeEffect(Kerned)(payload.kerns))]);
    const refused = HostError.refused(`insertion points of ${text.toSpecifier()}`);
    const methods = yield* Effect.try({ try: () => text.insertionPoints.everyItem().kerningMethod, catch: refused });
    const outOfRange = rows.filter(({ index }) => index >= methods.length);
    yield* Array.isReadonlyArrayEmpty(outOfRange) ? Effect.void : Unmatched.make({ subject: `insertion point in 0 to ${methods.length - 1}`, query: Option.some(outOfRange.map(({ index }) => index).join(', ')) });
    const unkerned = yield* Effect.try({ try: () => rows.filter(({ index }) => methods[index] !== manual || text.insertionPoints.item(index).kerningValue === 0), catch: refused });
    return yield* written(payload.label, text, Effect.forEach(unkerned, ({ index, kerning }) => write(text.insertionPoints.item(index), { kerningValue: kerning }), { discard: true }).pipe(Effect.as(unkerned.length)));
});
const runs = Effect.fn('runs')(function* (payload: Runs) {
    const [{ textContainer }, text] = yield* Effect.all([Model, Effect.flatMap(resolveTarget(payload.target), textRange)]);
    const container = text.parent;
    const whole = yield* textContainer(container) ? Effect.succeed(container.texts.item(0)) : Unmatched.make({ subject: 'text container', query: Option.some(payload.target) });
    const ranges = yield* Effect.try({
        try: () => Array.map(whole.textStyleRanges.everyItem().getElements(), (run) => ({ text: Predicate.isString(run.contents) ? run.contents : '\n'.repeat(run.length), font: run.appliedFont.name, axes: run.designAxes })),
        catch: HostError.refused(`style ranges of ${whole.toSpecifier()}`),
    });
    return { ...(yield* Model.reference(container)), start: text.index, length: text.length, runs: ranges };
});
const typeset = Effect.fn('typeset')(function* (payload: Typeset) {
    const target = yield* resolveTarget(payload.target);
    yield* Effect.transposeOption(Option.map(payload.settings, (settings) => commit(payload.label, HostSettings.write(target, settings))));
    return yield* HostSettings.read(target);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { adjust, kerns, runs, typeset };
