import { HostError, Reference, Specifier } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import type { InsertionPoint, Text, TextDefault, TextStyleRange } from 'adobe:indesign';
import { Array, Effect, flow, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import type { Adjust, Kerns, Runs, Typeset } from '#/typesetting';
import { Dom } from '#/uxp/dom';
import { InDesignSettings, thousandths } from '#/uxp/settings';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = /^\p{M}+$/v;

// --- [MODELS] --------------------------------------------------------------------------

const Deltas = Schema.Struct({ kerning: thousandths, tracking: thousandths, baselineShift: Schema.Finite, marks: InDesignSettings.fields.marks });
const Kerned = Schema.Array(Schema.Struct({ index: Schema.Int, length: Schema.Int, kerning: thousandths, baselineShift: Schema.Finite }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const textTarget = Effect.fnUntraced(function* (path: string) {
    const [{ app, textContainer, text, textDefault }, specifier] = yield* Effect.all([Dom, Schema.decodeEffect(Specifier.FromString)(path)]);
    const found = specifier.root ? app.textDefaults : yield* Dom.locate(specifier);
    const target = text(found) || textDefault(found) || !textContainer(found) ? found : found.texts.item(0);
    return yield* text(target) || textDefault(target) ? Effect.succeed(target) : Unmatched.of({ subject: 'text', query: Option.some(path) });
});
const textRange = (target: Text | TextDefault) => Dom.use(({ text }) => (text(target) ? Effect.succeed(target) : Unmatched.of({ subject: 'text range', query: Option.some(target.toSpecifier()) })));
const kerningMethod = (method: 'Manual' | 'Metrics') => Dom.use(({ app }) => Effect.try({ try: () => app.translateKeyString(`$ID/${method}`), catch: HostError.refused(`$ID/${method}`) }));
const pointAfterBase = (text: Text) =>
    Effect.try({
        try: () => {
            const last = text.insertionPoints.lastItem();
            const container = last.parent;
            const marks = Array.unfold(last.index - 1, (index) => (index >= 0 && MARK.test(container.characters.item(index).contents.toString()) ? Option.some([index, index - 1] as const) : Option.none()));
            return last.index > marks.length ? Option.some(container.insertionPoints.item(last.index - marks.length)) : Option.none();
        },
        catch: HostError.refused(`base letter before ${text.toSpecifier()}`),
    }).pipe(Effect.flatMap(Effect.fromOption(() => Unmatched.of({ subject: 'base letter', query: Option.some(text.toSpecifier()) }))));
const adjust = Effect.fn('adjust')(function* ({ label, target: path, deltas }: Adjust) {
    const { kerning = 0, tracking = 0, baselineShift = 0, marks = {} } = deltas;
    const [{ text }, target, manual, encoded] = yield* Effect.all([Dom, textTarget(path), kerningMethod('Manual'), Effect.orDie(Schema.encodeEffect(Deltas)({ kerning, tracking, baselineShift, marks: { x: 0, y: 0, ...marks } }))]);
    const points: readonly InsertionPoint[] = kerning === 0 ? [] : [yield* Effect.flatMap(textRange(target), pointAfterBase)];
    const sumDeltas = (run: TextStyleRange | TextDefault) => Record.filterMap({ tracking: encoded.tracking, baselineShift: encoded.baselineShift * run.pointSize, ...encoded.marks }, (amount, member) => (amount === 0 ? Result.failVoid : Result.succeed(run[member] + amount)));
    const sums = yield* Effect.try({ try: () => Array.map(text(target) ? target.textStyleRanges.everyItem().getElements() : [target], (run) => [run, sumDeltas(run)] as const), catch: HostError.refused(`style runs of ${target.toSpecifier()}`) });
    const kerned = yield* Effect.try({ try: () => Array.map(points, (point) => [point, { kerningValue: (point.kerningMethod === manual ? point.kerningValue : 0) + encoded.kerning }] as const), catch: HostError.refused(`kerning of ${target.toSpecifier()}`) });
    return yield* Dom.commitWrite(label, target, Effect.forEach([...sums, ...kerned], ([run, values]) => Effect.flatten(Dom.write(run, values))).pipe(Effect.map(flow(Array.flatten, Array.dedupe))));
});
const kerns = Effect.fn('kerns')(function* (payload: Kerns) {
    const [text, manual, metrics, rows] = yield* Effect.all([Effect.flatMap(textTarget(payload.target), textRange), kerningMethod('Manual'), kerningMethod('Metrics'), Effect.orDie(Schema.encodeEffect(Kerned)(payload.kerns))]);
    const refused = HostError.refused(`insertion points of ${text.toSpecifier()}`);
    const count = yield* Effect.try({ try: () => text.insertionPoints.length, catch: refused });
    const outOfRange = [...Option.toArray(payload.restore), ...rows].filter(({ index, length }) => index + length >= count);
    yield* Array.isReadonlyArrayEmpty(outOfRange) ? Effect.void : Unmatched.of({ subject: `run in characters 0 to ${count - 2}`, query: Option.some(outOfRange.map(({ index, length }) => `${index} to ${index + length - 1}`).join(', ')) });
    const characters = ({ index, length }: Pick<Kerns['kerns'][number], 'index' | 'length'>) => text.characters.itemByRange(index, index + length - 1);
    const restored = Effect.transposeOption(Option.map(payload.restore, (run) => Effect.flatMap(Effect.try({ try: () => characters(run).getElements(), catch: refused }), Effect.forEach(flow(Dom.write({ kerningMethod: metrics }), Effect.flatten), { discard: true }))));
    const kerned = Effect.try({
        try: () => {
            const methods = text.insertionPoints.everyItem().kerningMethod;
            const kept = rows.filter(({ index }) => methods[index] !== manual || text.insertionPoints.item(index).kerningValue === 0);
            const points = kept.filter(({ kerning }) => kerning !== 0).map(({ index, kerning }) => [text.insertionPoints.item(index), { kerningValue: kerning }] as const);
            const shifts = kept.filter(({ baselineShift }) => baselineShift !== 0).flatMap((row) => Array.map(characters(row).getElements(), (character) => [character, { baselineShift: row.baselineShift * character.pointSize }] as const));
            return { kept: kept.length, members: [...points, ...shifts] };
        },
        catch: refused,
    }).pipe(
        Effect.tap(({ members }) => Effect.forEach(members, ([target, values]) => Effect.flatten(Dom.write(target, values)), { discard: true })),
        Effect.map(Struct.get('kept')),
    );
    return yield* Dom.commitWrite(payload.label, text, Effect.andThen(restored, kerned));
});
const runs = Effect.fn('runs')(function* ({ target: path }: Runs) {
    const [{ text, textContainer }, target] = yield* Effect.all([Dom, textTarget(path)]);
    if (!text(target)) {
        return yield* Effect.try({ try: () => ({ ...Struct.pick(Dom.referenceUnsafe(target), Struct.keys(Reference.fields)), start: 0, length: 0, runs: [{ text: '', font: target.appliedFont.name, axes: target.designAxes }] }), catch: HostError.refused(`applied font of ${target.toSpecifier()}`) });
    }
    const container = target.parent;
    const whole = yield* textContainer(container) ? Effect.succeed(container.texts.item(0)) : Unmatched.of({ subject: 'text container', query: Option.some(path) });
    const ranges = yield* Effect.try({
        try: () => Array.map(whole.textStyleRanges.everyItem().getElements(), (run) => ({ text: Predicate.isString(run.contents) ? run.contents : '\n'.repeat(run.length), font: run.appliedFont.name, axes: run.designAxes })),
        catch: HostError.refused(`style ranges of ${whole.toSpecifier()}`),
    });
    return { ...Struct.pick(yield* Dom.reference(container), Struct.keys(Reference.fields)), start: target.index, length: target.length, runs: ranges };
});
const typeset = Effect.fn('typeset')(function* ({ label, target: path, settings }: Typeset) {
    const target = yield* textTarget(path);
    yield* Effect.transposeOption(Option.map(settings, (change) => Dom.commit(label, InDesignSettings.write(target, change))));
    return yield* InDesignSettings.read(target);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { adjust, kerns, runs, typeset };
