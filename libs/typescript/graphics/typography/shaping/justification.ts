import { Array, Function, flow, Match, Number, Option, Order, pipe, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { Feature, type Font } from 'harfbuzzjs';
import { type JoiningGroup, Script } from 'icu';
import { Settings } from '#/settings.ts';
import { Face } from '#/shaping/face.ts';
import { ShapedText } from '#/shaping/shaped-text.ts';
import { Word } from '#/shaping/word.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = typeof ShapedText.Run.Type;
type Glyph = ShapedText['glyphs'][number];
type Place = Settings['justification']['places'][number];
type Elongation = Option.Option.Value<ReturnType<typeof elongationOf>>;
type Line = Array.NonEmptyReadonlyArray<Measured>;
interface Styled {
    readonly face: Face;
    readonly run: Run;
    readonly settings: Settings;
}
interface Amounts {
    readonly words: Record.ReadonlyRecord<string, number>;
    readonly letter: number;
    readonly glyph: number;
}
interface Measured extends Styled {
    readonly shaped: ShapedText;
    readonly scales: ReadonlyMap<number, number>;
    readonly kashida: number;
    readonly alternates: number;
    readonly amounts: Amounts;
}
interface Mechanism {
    readonly capacity: number;
    readonly continuous: boolean;
    readonly take: (line: Line, amount: number) => Line;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const JUSTIFICATION_ALTERNATES = 'jalt';
const WORD_SPACE = /^[\u{20}\u{A0}]$/u;
const SPACE_SEPARATOR = /^\p{Zs}$/u;
const SPACINGS = Struct.keys(Settings.Spacing.fields);
const MECHANISMS = [...SPACINGS, 'alternates', 'kashida'] as const;
const BY_RANK = Order.Struct({ rank: Order.Number, cluster: Order.flip(Order.Number) });

// --- [MODELS] --------------------------------------------------------------------------

class Justification extends Schema.Class<Justification, { readonly brand: unique symbol }>('Justification')({
    runs: Schema.NonEmptyArray(
        Schema.Struct({
            shaped: ShapedText,
            ratios: Schema.Struct(Struct.Record(SPACINGS, Schema.Finite)).annotate({ description: 'Applied spacing ratios' }),
        }),
    ).annotate({ description: 'Style runs in logical order, set in line direction from offset' }),
    direction: Settings.Direction.annotate({ description: 'Line direction, RTL setting runs from the right' }),
    offset: Schema.Finite.annotate({ description: 'Em from the line left edge to its first glyph, slack placed by alignment and direction' }),
    slack: Schema.Finite.annotate({ description: 'Width left in em, negative when overfull' }),
    addedWidths: Schema.Struct(Struct.Record(MECHANISMS, Schema.Finite)).annotate({ description: 'Width each mechanism added in em' }),
}) {
    static readonly places = (text: string, rows: readonly Place[], allowed: (cluster: number) => boolean = Function.constTrue) =>
        Array.sort(
            Array.getSomes(
                Word.of(text).map(({ joins }) =>
                    pipe(
                        joins.flatMap((join) => candidates(text, join, rows)).filter(({ cluster }) => allowed(cluster)),
                        Array.match({ onEmpty: Option.none, onNonEmpty: flow(Array.min(BY_RANK), Option.some) }),
                    ),
                ),
            ),
            BY_RANK,
        );
    static readonly natural = (runs: Array.NonEmptyReadonlyArray<Styled>) => lineWidth(Array.map(runs, prepared));
    static readonly of = (runs: Array.NonEmptyReadonlyArray<Styled>, width: number, last = false): Justification => {
        const [first] = runs;
        const text = runs.map(({ run }) => Option.match(run.item, { onNone: () => run.text, onSome: (span) => span.slice(run.text) })).join('');
        const alignment = first.settings.lineAlignment({ last, single: Word.of(text).length <= 1 });
        const aligned = { start: 0, center: 0.5, end: 1, justified: 0 }[alignment];
        const direction = Option.getOrElse(
            Option.orElse(first.settings.direction, () => Word.direction(text)),
            () => 'LTR' as const,
        );
        const unjustified: Line = Array.map(runs, prepared);
        const justifying = alignment === 'justified' || width < lineWidth(unjustified);
        const ranks = Array.dedupe(
            Array.sort(
                runs.flatMap(({ settings: { justification: ranked } }) => [ranked.word, ranked.alternates, ranked.letter, ranked.glyph, ...Object.values(ranked.scripts), ...ranked.places.map(Struct.get('rank'))]),
                Order.Number,
            ),
        );
        const distributed = justifying ? ranks.reduce((line, rank) => distribute(line, rank, width), unjustified) : unjustified;
        const finished = Array.map(distributed, finish);
        const advance = Number.sumAll(finished.map(({ shaped }) => shaped.advance));
        const slack = width - advance;
        return Justification.make({
            runs: Array.map(finished, ({ shaped, ratios }) => ({ shaped, ratios })),
            direction,
            offset: slack * (direction === 'RTL' ? 1 - aligned : aligned),
            slack,
            addedWidths: Record.map(Struct.Record(MECHANISMS, 0), (_, mechanism) => Number.sumAll(finished.map(({ added }) => added[mechanism]))),
        });
    };

    get advance() {
        return Number.sumAll(this.runs.map(({ shaped }) => shaped.advance));
    }
    get extents() {
        const [, boxes] = Array.mapAccum(this.direction === 'RTL' ? Array.reverse(this.runs) : this.runs, this.offset, (left, { shaped }) => [left + shaped.advance, Option.map(shaped.extents, (box) => ({ ...box, minX: box.minX + left, maxX: box.maxX + left }))] as const);
        return ShapedText.enclosing(Array.getSomes(boxes));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [MEASURES]

const summed = flow(Object.values<number>, Number.sumAll);
const alternate = (settings: Settings) => Option.filter(Record.get(settings.features, JUSTIFICATION_ALTERNATES), Number.isGreaterThan(0));
const rescaled = (glyph: Glyph, ratio: number): Glyph => ({ ...glyph, xOffset: glyph.xOffset * ratio, scale: glyph.scale * ratio, extents: Option.map(glyph.extents, (box) => ({ ...box, xBearing: box.xBearing * ratio, width: box.width * ratio })) });
const isWordSpace = (text: string, cluster: number) => WORD_SPACE.test(text.charAt(cluster));
const isLetter = (text: string, { class: kind, xAdvance, cluster }: Glyph) => kind !== 'MARK' && xAdvance !== 0 && !SPACE_SEPARATOR.test(text.charAt(cluster));
const scriptAt = (letters: ReturnType<typeof Word.letters>, cluster: number) =>
    pipe(
        Array.findLast(letters, ({ offset }) => offset < cluster),
        Option.orElse(() => Array.findFirst(letters, ({ offset }) => offset > cluster)),
        Option.map(({ script }) => script.value),
        Option.getOrElse(() => Script.Common.value),
    );
const bases = ({ face, shaped }: Measured) => {
    const wordSpace = 0x20;
    const { text } = shaped;
    const letters = Word.letters(text);
    const joins = Word.of(text).flatMap(Struct.get('joins'));
    const joinAt = (cluster: number) => Array.findFirstIndex(joins, (join) => join.contains(cluster));
    const placed = Array.filterMap(shaped.glyphs, (each, at) => (isLetter(text, each) ? Result.succeed({ at, cluster: each.cluster }) : Result.failVoid));
    return {
        words: Record.map(
            Array.groupBy(
                shaped.glyphs.filter(({ cluster }) => isWordSpace(text, cluster)),
                ({ cluster }) => scriptAt(letters, cluster),
            ),
            (spaces) => Number.sumAll(spaces.map(Struct.get('xAdvance'))),
        ),
        slots: new Set(Array.filterMap(Array.window(placed, 2), ([before, after]) => (Option.exists(joinAt(before.cluster), (join) => Option.contains(joinAt(after.cluster), join)) ? Result.failVoid : Result.succeed(before.at)))),
        space: Option.getOrElse(face.advance(wordSpace), () => 0),
        glyph: Number.sumAll(shaped.glyphs.filter((glyph) => isLetter(text, glyph)).map(Struct.get('xAdvance'))),
    };
};
const widths = (measured: Measured) => {
    const base = bases(measured);
    const {
        settings: { spacing },
        amounts,
    } = measured;
    return {
        base,
        words: Record.map(base.words, (total, script) => total * spacing.word.desired + Option.getOrElse(Record.get(amounts.words, script), () => 0)),
        letter: base.slots.size * base.space * spacing.letter.desired + amounts.letter,
        glyph: base.glyph * spacing.glyph.desired + amounts.glyph,
    };
};
const lineWidth = (line: Line) =>
    Number.sumAll(
        line.map((measured) => {
            const { base, words, letter, glyph } = widths(measured);
            return measured.shaped.advance + summed(words) - summed(base.words) + letter + glyph - base.glyph;
        }),
    );
const remeasured = (measured: Measured, run: Run, scales = measured.scales, font: Font = measured.face.font): Measured => {
    const { text, glyphs, missing } = ShapedText.of({ font, em: measured.face.em }, run, measured.settings);
    return { ...measured, run, scales, shaped: ShapedText.make({ text, glyphs: glyphs.map((glyph) => UndefinedOr.match(scales.get(glyph.cluster), { onUndefined: () => glyph, onDefined: (scale) => ({ ...rescaled(glyph, scale), xAdvance: glyph.xAdvance * scale }) })), missing }) };
};
const prepared = (styled: Styled): Measured => {
    const { face, run, settings } = styled;
    const plain = ShapedText.converted({ ...run, features: Option.isSome(alternate(settings)) ? [new Feature(JUSTIFICATION_ALTERNATES, 0), ...run.features] : run.features, flags: Array.union(run.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) }, settings);
    const measured: Measured = { ...styled, run: plain, shaped: ShapedText.of(face, plain, settings), scales: new Map(), kashida: 0, alternates: 0, amounts: { words: {}, letter: 0, glyph: 0 } };
    return Option.match(elongationOf(settings), {
        onNone: () => measured,
        onSome: (elongation) => tatweeled(measured, elongation, eligible(measured, false, 'text').map(Struct.get('cluster')), Option.none()),
    });
};

// --- [KASHIDA]

const elongationOf = (settings: Settings) => Option.liftPredicate(settings.kashida, Settings.Kashida.isAnyOf(['Tatweel', 'Stretched']));
const candidates = (text: string, join: Word['joins'][number], rows: readonly Place[]) => {
    const within = (groups: Place['before']) => (group: JoiningGroup) => Option.match(groups, { onNone: Function.constTrue, onSome: ({ groups: listed, except }) => listed.includes(group.value) !== except });
    const follows = (next: Place['next'], letter: Option.Option<JoiningGroup>) => Option.match(next, { onNone: Function.constTrue, onSome: Match.valueTags({ End: () => Option.isNone(letter), Letter: ({ groups }) => Option.exists(letter, within(groups)) }) });
    const letters = Word.letters(join.slice(text));
    return Array.getSomes(
        Array.window(letters, 2).map(([before, after], at) => {
            const next = Option.map(Array.get(letters, at + 2), Struct.get('group'));
            const row = Array.findFirst(rows, (each) => within(each.before)(before.group) && within(each.after)(after.group) && follows(each.next, next));
            return Option.map(row, ({ rank }) => ({ cluster: join.start + after.offset, rank }));
        }),
    );
};
const eligible = (measured: Measured, stretched: boolean, stage: Place['stage']) => {
    const flagged = (glyph: Glyph) => glyph.flags.includes('SAFE_TO_INSERT_TATWEEL') || (stretched && measured.face.layout.covers(JUSTIFICATION_ALTERNATES, glyph.glyph));
    const clusters = new Set(measured.shaped.glyphs.filter(flagged).map(Struct.get('cluster')));
    return Justification.places(
        measured.shaped.text,
        measured.settings.justification.places.filter((row) => row.stage === stage),
        (cluster) => clusters.has(cluster),
    );
};
const solved = ({ lower, upper }: Elongation, places: number, limit: Option.Option<number>, room: Option.Option<number>) => {
    const most = Math.min(
        places,
        Option.getOrElse(limit, () => places),
    );
    const fitted = Option.match(room, {
        onNone: () => ({ count: most, length: lower }),
        onSome: (available) => {
            const count = Math.min(most, Math.floor(available / lower), Math.ceil(available / upper));
            return { count, length: Number.clamp(available / count, { minimum: lower, maximum: upper }) };
        },
    });
    return Option.liftPredicate(fitted, ({ count }) => count > 0);
};
const tatweeled = (measured: Measured, elongation: Elongation, clusters: readonly number[], room: Option.Option<number>) =>
    Option.getOrElse(
        Option.gen(function* () {
            const [nuqta, tatweel] = yield* Option.all([measured.face.nuqta, measured.face.tatweel]);
            const { count, length } = yield* solved(
                elongation,
                clusters.length,
                measured.settings.justification.kashidas,
                Option.map(room, (em) => Math.max(0, em) / nuqta),
            );
            const span = length * nuqta;
            const tatweels = yield* Option.liftPredicate(elongation.fractional ? Math.ceil(span / tatweel) : Math.floor(span / tatweel), Number.isGreaterThan(0));
            return inserted(measured, Array.sort(Array.take(clusters, count), Order.Number), tatweels, elongation.fractional ? span / (tatweels * tatweel) : 1);
        }),
        () => measured,
    );
const inserted = (measured: Measured, chosen: readonly number[], tatweels: number, scale: number): Measured => {
    const shifted = (offset: number, bound: 'start' | 'end') => offset + tatweels * chosen.filter((cluster) => cluster < offset || (bound === 'start' && cluster === offset)).length;
    const { run } = measured;
    const text = chosen.reduceRight((inserting, cluster) => `${inserting.slice(0, cluster)}${String.fromCodePoint(Face.TATWEEL).repeat(tatweels)}${inserting.slice(cluster)}`, measured.shaped.text);
    const scales = [...Array.fromIterable(measured.scales).map(([cluster, ratio]) => [shifted(cluster, 'start'), ratio] as const), ...chosen.flatMap((cluster) => Array.makeBy(tatweels, (index) => [shifted(cluster, 'end') + index, scale] as const))];
    const features = run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start, 'start'), Math.min(shifted(end, 'end'), Feature.GLOBAL_END)));
    const elongated = remeasured(measured, { ...run, text, item: Option.map(run.item, ({ start, end }) => Word.Span.make({ start: shifted(start, 'start'), end: shifted(end, 'end') })), features }, new Map(scales));
    return { ...elongated, kashida: measured.kashida + elongated.shaped.advance - measured.shaped.advance };
};
const featured = (measured: Measured, feature: Feature, limit: number) => {
    const next = remeasured(measured, { ...measured.run, features: [...measured.run.features, feature] });
    return Option.liftPredicate({ next, gain: next.shaped.advance - measured.shaped.advance }, ({ gain }) => gain > 0 && gain <= limit);
};
const probed = (current: Measured, cluster: number, left: number) => {
    const values = Option.match(
        Array.findFirst(current.shaped.glyphs, (each) => each.cluster === cluster),
        { onNone: Array.empty<number>, onSome: ({ glyph }) => current.face.layout.variants(JUSTIFICATION_ALTERNATES, glyph) },
    );
    const probes = Array.getSomes(values.map((_, index) => Option.map(featured(current, new Feature(JUSTIFICATION_ALTERNATES, index + 1, cluster, cluster + 1), left), ({ next, gain }) => ({ ...next, kashida: current.kashida + gain }))));
    return Option.getOrElse(Option.map(Option.liftPredicate(probes, Array.isArrayNonEmpty), Array.max(Order.mapInput(Order.Number, ({ kashida }: Measured) => kashida))), () => current);
};
const alternated = (measured: Measured, clusters: readonly number[], room: number) => clusters.reduce((current, cluster) => probed(current, cluster, room - (current.kashida - measured.kashida)), measured);
const reached = (measured: Measured, stretched: boolean, rank: number) =>
    eligible(measured, stretched, 'line')
        .filter((place) => place.rank === rank)
        .map(Struct.get('cluster'));
const elongated = (current: Measured, elongation: Elongation, rank: number, amount: number) => {
    const jalted = elongation._tag === 'Stretched' ? alternated(current, reached(current, true, rank), amount) : current;
    return tatweeled(jalted, elongation, reached(jalted, false, rank), Option.some(amount - (jalted.kashida - current.kashida)));
};
const kashidas = (measured: Measured, index: number, rank: number): readonly Mechanism[] =>
    Option.match(Option.all([elongationOf(measured.settings), measured.face.nuqta]), {
        onNone: () => [],
        onSome: ([elongation, nuqta]) => [
            {
                capacity: Option.match(solved(elongation, reached(measured, elongation._tag === 'Stretched', rank).length, measured.settings.justification.kashidas, Option.some(globalThis.Number.POSITIVE_INFINITY)), {
                    onNone: () => 0,
                    onSome: ({ count, length }) => count * length * nuqta,
                }),
                continuous: false,
                take: (line, amount) => replaced(line, index, (current) => elongated(current, elongation, rank, amount)),
            },
        ],
    });

// --- [DISTRIBUTION]

const replaced = (line: Line, index: number, update: (measured: Measured) => Measured): Line => Array.map(line, (measured, at) => (at === index ? update(measured) : measured));
const alternates = (measured: Measured, index: number, rank: number): readonly Mechanism[] =>
    Option.match(
        Option.filter(alternate(measured.settings), () => measured.settings.justification.alternates === rank),
        {
            onNone: () => [],
            onSome: (value) => [
                {
                    capacity: Option.match(featured(measured, new Feature(JUSTIFICATION_ALTERNATES, value), globalThis.Number.POSITIVE_INFINITY), { onNone: () => 0, onSome: Struct.get('gain') }),
                    continuous: false,
                    take: (line, amount) =>
                        replaced(line, index, (current) =>
                            Option.match(featured(current, new Feature(JUSTIFICATION_ALTERNATES, value), amount), {
                                onNone: () => current,
                                onSome: ({ next, gain }) => ({ ...next, alternates: current.alternates + gain }),
                            }),
                        ),
                },
            ],
        },
    );
const spacings = (measured: Measured, index: number, rank: number, stretching: boolean): readonly Mechanism[] => {
    const base = bases(measured);
    const { spacing, justification: ranked } = measured.settings;
    const sign = stretching ? 1 : -1;
    const continuous = (at: number, { min, desired, max }: Settings['spacing']['word'], width: number, update: (amounts: Amounts, amount: number) => Amounts): readonly Mechanism[] =>
        at === rank ? [{ capacity: width * (stretching ? max - desired : desired - min), continuous: true, take: (line, amount) => replaced(line, index, (current) => ({ ...current, amounts: update(current.amounts, sign * amount) })) }] : [];
    return [
        ...Record.collect(base.words, (script, total) =>
            continuous(
                Option.getOrElse(Record.get(ranked.scripts, script), () => ranked.word),
                spacing.word,
                total,
                (amounts, amount) => ({
                    ...amounts,
                    words: { ...amounts.words, [script]: (amounts.words[script] ?? 0) + amount },
                }),
            ),
        ),
        ...Record.collect({ letter: base.slots.size * base.space, glyph: base.glyph }, (key, width) => continuous(ranked[key], spacing[key], width, (amounts, amount) => ({ ...amounts, [key]: amounts[key] + amount }))),
    ].flat();
};
const mechanisms = (line: Line, rank: number, stretching: boolean): readonly Mechanism[] => line.flatMap((measured, index) => [...spacings(measured, index, rank, stretching), ...(stretching ? [...alternates(measured, index, rank), ...kashidas(measured, index, rank)] : [])]);
const distribute = (line: Line, rank: number, width: number): Line => {
    const slack = width - lineWidth(line);
    const group = mechanisms(line, rank, slack >= 0).filter((mechanism) => mechanism.capacity > 0);
    const total = Number.sumAll(group.map(Struct.get('capacity')));
    const target = Math.min(Math.abs(slack), total);
    const [continuous, discrete] = Array.partition(
        group,
        Result.liftPredicate((mechanism: Mechanism) => mechanism.continuous, Function.identity),
    );
    const placed = discrete.reduce((current, mechanism) => mechanism.take(current, (target * mechanism.capacity) / total), line);
    const rest = Math.max(0, target - Math.abs(lineWidth(placed) - lineWidth(line)));
    const shared = Number.sumAll(continuous.map(Struct.get('capacity')));
    return continuous.reduce((current, mechanism) => mechanism.take(current, Math.min(mechanism.capacity, (rest * mechanism.capacity) / shared)), placed);
};

// --- [FINISHING]

const finish = (measured: Measured) => {
    const { spacing } = measured.settings;
    const { base: original, words: spaces, letter: letterWidth, glyph: glyphWidth } = widths(measured);
    const widening = Option.getOrElse(Number.divide(glyphWidth, original.glyph), () => spacing.glyph.desired);
    const set = Option.match(
        Option.filter(
            Array.findFirst(measured.face.axes, ({ tag }) => tag === 'wdth'),
            () => widening !== 1,
        ),
        { onNone: () => measured, onSome: ({ value }) => remeasured(measured, measured.run, measured.scales, measured.face.instance({ wdth: value * widening })) },
    );
    const base = bases(set);
    const scale = Option.getOrElse(Number.divide(glyphWidth, base.glyph), () => 1);
    const wordRatio = (script: string) => Option.getOrElse(Option.flatMap(Record.get(spaces, script), Number.divide(Option.getOrElse(Record.get(base.words, script), () => 0))), () => spacing.word.desired);
    const slot = Option.getOrElse(Number.divide(letterWidth, base.slots.size), () => 0);
    const { text } = set.shaped;
    const letters = Word.letters(text);
    const spaceRatio = (cluster: number) => (isWordSpace(text, cluster) ? wordRatio(scriptAt(letters, cluster)) : 1);
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

export { Justification };
