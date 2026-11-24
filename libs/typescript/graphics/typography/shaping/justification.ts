import { Array, Data, flow, Number, Option, Order, pipe, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { Feature, type Font } from 'harfbuzzjs';
import { Script } from 'icu';
import { joins } from '#/arabic/joining';
import { kashidaFlagged, kashidaMechanisms, kashidaPrepared, kashidaRanks } from '#/arabic/kashida';
import { Direction, Settings } from '#/settings';
import type { Face } from '#/shaping/face';
import { ShapedText } from '#/shaping/shaped-text';
import { baseDirection, letters, words } from '#/shaping/word';

// --- [TYPES] ---------------------------------------------------------------------------

interface Styled {
    readonly face: Face;
    readonly run: typeof ShapedText.Run.Type;
    readonly settings: Settings;
}
interface Mechanism {
    readonly index: number;
    readonly capacity: number;
    readonly continuous: boolean;
    readonly take: (measured: Measured, amount: number) => Measured;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const JUSTIFICATION_ALTERNATES = 'jalt';
const WORD_SPACE = /^[\u{20}\u{A0}]$/u;
const SPACE_SEPARATOR = /^\p{Zs}$/u;
const SPACINGS = Struct.keys(Settings.fields.spacing.schema.fields);
const MECHANISMS = [...SPACINGS, 'alternates', 'kashida'] as const;

// --- [MODELS] --------------------------------------------------------------------------

class Measured extends Data.Class<
    Styled & {
        readonly shaped: ShapedText;
        readonly base: ReturnType<typeof bases>;
        readonly scales: ReadonlyMap<number, number>;
        readonly kashida: number;
        readonly alternates: number;
        readonly amounts: { readonly words: Record.ReadonlyRecord<string, number>; readonly letter: number; readonly glyph: number };
    }
> {
    static readonly of = (styled: Styled, run: Styled['run']) => {
        const shaped = ShapedText.of(styled.face, run, styled.settings);
        return new Measured({ ...styled, run, shaped, base: bases(styled.face, shaped), scales: new Map(), kashida: 0, alternates: 0, amounts: { words: {}, letter: 0, glyph: 0 } });
    };

    reshaped(run: Styled['run'], scales: ReadonlyMap<number, number> = this.scales, font: Font = this.face.font): Measured {
        const { text, glyphs, missing } = ShapedText.of({ font, em: this.face.em }, run, this.settings);
        const shaped = ShapedText.make({ text, glyphs: glyphs.map((glyph) => UndefinedOr.match(scales.get(glyph.cluster), { onUndefined: () => glyph, onDefined: (scale) => ({ ...rescaled(glyph, scale), xAdvance: glyph.xAdvance * scale }) })), missing });
        return new Measured({ ...this, run, scales, shaped, base: bases(this.face, shaped) });
    }
    featured(feature: Feature, limit: number) {
        const next = this.reshaped({ ...this.run, features: [...this.run.features, feature] });
        return Option.liftPredicate({ next, gain: next.shaped.advance - this.shaped.advance }, ({ gain }) => gain > 0 && gain <= limit);
    }
    gained(mechanism: 'alternates' | 'kashida', width: number) {
        return new Measured({ ...this, [mechanism]: this[mechanism] + width });
    }
}
class Justification extends Schema.Class<Justification, { readonly brand: unique symbol }>('Justification')({
    runs: Schema.NonEmptyArray(
        Schema.Struct({
            shaped: ShapedText,
            ratios: Schema.Struct(Struct.Record(SPACINGS, Schema.Finite)).annotate({ description: 'Applied spacing ratios' }),
        }),
    ).annotate({ description: 'Style runs in logical order, set in line direction from offset' }),
    direction: Direction.annotate({ description: 'Line direction, RTL setting runs from the right' }),
    offset: Schema.Finite.annotate({ description: 'Em from the line left edge to its first glyph, slack placed by alignment and direction' }),
    slack: Schema.Finite.annotate({ description: 'Width left in em, negative when overfull' }),
    addedWidths: Schema.Struct(Struct.Record(MECHANISMS, Schema.Finite)).annotate({ description: 'Width each mechanism added in em' }),
}) {
    static readonly natural = (runs: Array.NonEmptyReadonlyArray<Styled>) => lineWidth(Array.map(runs, prepared));
    static readonly of = (runs: Array.NonEmptyReadonlyArray<Styled>, width: number, last = false): Justification => {
        const [first] = runs;
        const text = runs.map(({ run }) => Option.match(run.item, { onNone: () => run.text, onSome: (span) => span.slice(run.text) })).join('');
        const alignment = first.settings.lineAlignment({ last, single: words(text).length <= 1 });
        const aligned = { start: 0, center: 0.5, end: 1, justified: 0 }[alignment];
        const direction = Option.getOrElse(first.settings.direction, () => baseDirection(text));
        const unjustified: Array.NonEmptyReadonlyArray<Measured> = Array.map(runs, prepared);
        const justifying = alignment === 'justified' || width < lineWidth(unjustified);
        const ranks = Array.dedupe(runs.flatMap(({ settings }) => [...Object.values(Struct.omit(settings.justification, ['scripts'])), ...Object.values(settings.justification.scripts), ...kashidaRanks(settings)]).toSorted(Order.Number));
        const distributed = justifying ? ranks.reduce((line, rank) => distribute(line, rank, width), unjustified) : unjustified;
        const finished = Array.map(distributed, finish);
        const advance = Math.sumPrecise(finished.map(({ shaped }) => shaped.advance));
        const slack = width - advance;
        return Justification.make({
            runs: Array.map(finished, ({ shaped, ratios }) => ({ shaped, ratios })),
            direction,
            offset: slack * (direction === 'RTL' ? 1 - aligned : aligned),
            slack,
            addedWidths: Record.map(Struct.Record(MECHANISMS, 0), (_, mechanism) => Math.sumPrecise(finished.map(({ added }) => added[mechanism]))),
        });
    };

    get advance() {
        return Math.sumPrecise(this.runs.map(({ shaped }) => shaped.advance));
    }
    get extents() {
        const [, boxes] = Array.mapAccum(this.direction === 'RTL' ? Array.reverse(this.runs) : this.runs, this.offset, (left, { shaped }) => [left + shaped.advance, Option.map(shaped.extents, (box) => ({ ...box, minX: box.minX + left, maxX: box.maxX + left }))] as const);
        return ShapedText.enclosing(Array.getSomes(boxes));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [MEASURES]

const summed = flow(Object.values<number>, Math.sumPrecise);
const alternate = (settings: Settings) => Option.filter(Record.get(settings.features, JUSTIFICATION_ALTERNATES), Number.isGreaterThan(0));
const rescaled = (glyph: ShapedText['glyphs'][number], ratio: number): ShapedText['glyphs'][number] => ({ ...glyph, xOffset: glyph.xOffset * ratio, scale: glyph.scale * ratio, extents: Option.map(glyph.extents, (box) => ({ ...box, xBearing: box.xBearing * ratio, width: box.width * ratio })) });
const isWordSpace = (text: string, cluster: number) => WORD_SPACE.test(text.charAt(cluster));
const isLetter = (text: string, { class: kind, xAdvance, cluster }: ShapedText['glyphs'][number]) => kind !== 'MARK' && xAdvance !== 0 && !SPACE_SEPARATOR.test(text.charAt(cluster));
const scriptAt = (placed: ReturnType<typeof letters>, cluster: number) =>
    pipe(
        Array.findLast(placed, ({ offset }) => offset < cluster),
        Option.orElse(() => Array.findFirst(placed, ({ offset }) => offset > cluster)),
        Option.map(({ script }) => script.value),
        Option.getOrElse(() => Script.Common.value),
    );
const bases = (face: Face, shaped: ShapedText) => {
    const wordSpace = 0x20;
    const { text } = shaped;
    const lettered = letters(text);
    const joined = words(text).flatMap((word) => joins(text, word));
    const joinAt = (cluster: number) => Array.findFirstIndex(joined, (join) => join.contains(cluster));
    const placed = Array.filterMap(shaped.glyphs, (each, at) => (isLetter(text, each) ? Result.succeed({ at, cluster: each.cluster, xAdvance: each.xAdvance }) : Result.failVoid));
    const spaces = shaped.glyphs.filter(({ cluster }) => isWordSpace(text, cluster));
    const spaced = Array.groupBy(spaces, ({ cluster }) => scriptAt(lettered, cluster));
    return {
        words: Record.map(spaced, (group) => Math.sumPrecise(group.map(Struct.get('xAdvance')))),
        slots: new Set(Array.filterMap(Array.window(placed, 2), ([before, after]) => (Option.exists(joinAt(before.cluster), (join) => Option.contains(joinAt(after.cluster), join)) ? Result.failVoid : Result.succeed(before.at)))),
        space: Option.getOrElse(face.advance(wordSpace), () => 0),
        glyph: Math.sumPrecise(placed.map(Struct.get('xAdvance'))),
    };
};
const widths = ({ base, settings: { spacing }, amounts }: Measured) => ({
    words: Record.map(base.words, (total, script) => total * spacing.word.desired + Option.getOrElse(Record.get(amounts.words, script), () => 0)),
    letter: base.slots.size * base.space * spacing.letter.desired + amounts.letter,
    glyph: base.glyph * spacing.glyph.desired + amounts.glyph,
});
const lineWidth = (line: Array.NonEmptyReadonlyArray<Measured>) =>
    Math.sumPrecise(
        line.map((measured) => {
            const { words: spaces, letter, glyph } = widths(measured);
            return measured.shaped.advance + summed(spaces) - summed(measured.base.words) + letter + glyph - measured.base.glyph;
        }),
    );
const prepared = (styled: Styled): Measured => {
    const { run, settings } = styled;
    const plain = ShapedText.converted(kashidaFlagged({ ...run, features: Option.isSome(alternate(settings)) ? [new Feature(JUSTIFICATION_ALTERNATES, 0), ...run.features] : run.features }, settings), settings);
    return kashidaPrepared(Measured.of(styled, plain));
};

// --- [DISTRIBUTION]

const alternates = (measured: Measured, rank: number): readonly Omit<Mechanism, 'index'>[] =>
    alternate(measured.settings).pipe(
        Option.filter(() => measured.settings.justification.alternates === rank),
        Option.map((value) => new Feature(JUSTIFICATION_ALTERNATES, value)),
        Option.map((feature) => ({
            capacity: Option.match(measured.featured(feature, globalThis.Number.POSITIVE_INFINITY), { onNone: () => 0, onSome: Struct.get('gain') }),
            continuous: false,
            take: (current: Measured, amount: number) => Option.match(current.featured(feature, amount), { onNone: () => current, onSome: ({ next, gain }) => next.gained('alternates', gain) }),
        })),
        Option.toArray,
    );
const spacings = ({ base, settings }: Measured, rank: number, stretching: boolean): readonly Omit<Mechanism, 'index'>[] => {
    const { spacing, justification: ranked } = settings;
    const sign = stretching ? 1 : -1;
    return [
        ...Record.collect(base.words, (script, width) => ({
            at: Option.getOrElse(Record.get(ranked.scripts, script), () => ranked.word),
            range: spacing.word,
            width,
            added: (amounts: Measured['amounts'], amount: number) => ({ ...amounts, words: { ...amounts.words, [script]: (amounts.words[script] ?? 0) + amount } }),
        })),
        ...Record.collect({ letter: base.slots.size * base.space, glyph: base.glyph }, (key, width) => ({ at: ranked[key], range: spacing[key], width, added: (amounts: Measured['amounts'], amount: number) => ({ ...amounts, [key]: amounts[key] + amount }) })),
    ]
        .filter(({ at }) => at === rank)
        .map(({ range, width, added }) => ({ capacity: width * (stretching ? range.max - range.desired : range.desired - range.min), continuous: true, take: (current: Measured, amount: number) => new Measured({ ...current, amounts: added(current.amounts, sign * amount) }) }));
};
const mechanisms = (line: Array.NonEmptyReadonlyArray<Measured>, rank: number, stretching: boolean): readonly Mechanism[] =>
    line.flatMap((measured, index) => [...spacings(measured, rank, stretching), ...(stretching ? [...alternates(measured, rank), ...kashidaMechanisms(measured, rank)] : [])].map((mechanism) => ({ ...mechanism, index })));
const applied = (line: Array.NonEmptyReadonlyArray<Measured>, { index, take }: Mechanism, amount: number) => Array.map(line, (measured, at) => (at === index ? take(measured, amount) : measured));
const distribute = (line: Array.NonEmptyReadonlyArray<Measured>, rank: number, width: number) => {
    const slack = width - lineWidth(line);
    const group = mechanisms(line, rank, slack >= 0).filter((mechanism) => mechanism.capacity > 0);
    const total = Math.sumPrecise(group.map(Struct.get('capacity')));
    const target = Math.min(Math.abs(slack), total);
    const [continuous, discrete] = Array.partition(group, (mechanism) => (mechanism.continuous ? Result.succeed(mechanism) : Result.fail(mechanism)));
    const placed = discrete.reduce((current, mechanism) => applied(current, mechanism, (target * mechanism.capacity) / total), line);
    const rest = Math.max(0, target - Math.abs(lineWidth(placed) + slack - width));
    const shared = Math.sumPrecise(continuous.map(Struct.get('capacity')));
    return continuous.reduce((current, mechanism) => applied(current, mechanism, Math.min(mechanism.capacity, (rest * mechanism.capacity) / shared)), placed);
};

// --- [FINISHING]

const finish = (measured: Measured) => {
    const [original, spacing] = [measured.base, measured.settings.spacing];
    const { words: spaces, letter: letterWidth, glyph: glyphWidth } = widths(measured);
    const widening = Option.getOrElse(Number.divide(glyphWidth, original.glyph), () => spacing.glyph.desired);
    const wdth = Array.findFirst(measured.face.axes, ({ tag }) => widening !== 1 && tag === 'wdth');
    const set = Option.match(wdth, { onNone: () => measured, onSome: ({ value }) => measured.reshaped(measured.run, measured.scales, measured.face.instance({ wdth: value * widening })) });
    const { base } = set;
    const scale = Option.getOrElse(Number.divide(glyphWidth, base.glyph), () => 1);
    const wordRatio = (script: string) => Option.getOrElse(Option.flatMap(Record.get(spaces, script), Number.divide(Option.getOrElse(Record.get(base.words, script), () => 0))), () => spacing.word.desired);
    const slot = Option.getOrElse(Number.divide(letterWidth, base.slots.size), () => 0);
    const { text } = set.shaped;
    const lettered = letters(text);
    const spaceRatio = (cluster: number) => (isWordSpace(text, cluster) ? wordRatio(scriptAt(lettered, cluster)) : 1);
    const [, glyphs] = Array.mapAccum(set.shaped.glyphs, { cluster: 0, addedAdvance: 0 }, (previous, each, at) => {
        const ratio = isLetter(text, each) ? scale : spaceRatio(each.cluster);
        const xAdvance = each.xAdvance * ratio + (base.slots.has(at) ? slot : 0);
        const scaled = rescaled(each, scale);
        return each.xAdvance === 0
            ? ([previous, { ...scaled, xOffset: scaled.xOffset - (each.cluster >= previous.cluster ? previous.addedAdvance : 0) }] as const)
            : ([
                  { cluster: each.cluster, addedAdvance: xAdvance - each.xAdvance * scale },
                  { ...scaled, xAdvance },
              ] as const);
    });
    const spacesWidth = summed(spaces);
    return {
        shaped: ShapedText.make({ text, glyphs, missing: set.shaped.missing }),
        ratios: {
            word: Option.getOrElse(Number.divide(spacesWidth, summed(base.words)), () => spacing.word.desired),
            letter: Option.getOrElse(Number.divide(slot, base.space), () => spacing.letter.desired),
            glyph: widening,
        },
        added: { word: spacesWidth - summed(original.words), letter: letterWidth, glyph: glyphWidth - original.glyph, alternates: measured.alternates, kashida: measured.kashida },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Justification, type Measured };
