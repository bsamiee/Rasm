import { Array, Equal, Function, Match, Number, Option, Order, Predicate, Record, Result, Schema, Struct } from 'effect';
import { Feature } from 'harfbuzzjs';
import { JoiningGroup } from 'icu';
import type { Face } from '#/face';
import { Settings } from '#/settings';
import { ShapedText } from '#/shaped-text';
import { Word } from '#/word';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = typeof ShapedText.Run.Type;
type Ratios = Justification['ratios'];
interface Measurement {
    readonly run: Run;
    readonly shaped: ShapedText;
    readonly widths: Ratios;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const JUSTIFICATION_ALTERNATES = 'jalt';
const WORD_SPACE = /^[\u{20}\u{A0}]$/u;
const SPACE_SEPARATOR = /^\p{Zs}$/u;
const SPACINGS = Struct.keys(Settings.fields.spacing.schema.fields);
const IDENTITY: Ratios = { word: 1, letter: 0, glyph: 1 };
const BY_RANK = Order.Struct({ rank: Order.Number, cluster: Order.flip(Order.Number) });

// --- [MODELS] --------------------------------------------------------------------------

class Justification extends Schema.Class<Justification, { readonly brand: unique symbol }>('Justification')({
    text: Schema.String.annotate({ description: 'Run text with inserted tatweels, glyph clusters index it' }),
    shaped: ShapedText,
    slack: Schema.Finite.annotate({ description: 'Width left in em, negative when overfull' }),
    ratios: Schema.Struct(Struct.Record(SPACINGS, Schema.Finite)).annotate({ description: 'Applied spacing ratios' }),
    addedWidths: Schema.Struct(Struct.Record([...SPACINGS, 'kashida' as const], Schema.Finite)).annotate({ description: 'Width each mechanism added in em' }),
}) {
    static readonly kashidaClusters = (text: string, allowed: (cluster: number) => boolean = Function.constTrue) =>
        Array.sort(Array.getSomes(Word.of(text).map(({ joins }) => Array.head(Array.sort(BY_RANK)(joins.flatMap((join) => kashidaCandidates(text, join)).filter(({ cluster }) => allowed(cluster)))))), BY_RANK).map(Struct.get('cluster'));
    static readonly of = (face: Face, source: Run, width: number, settings = Settings.make({})): Justification => {
        const { kashida, spacing } = settings;
        const space = 0x20;
        const advance = (point: number) => Option.map(Option.fromUndefinedOr(face.font.nominalGlyph(point)), (glyph) => face.font.glyphHAdvance(glyph) / face.unitsPerEm);
        const spaceAdvance = Option.getOrElse(advance(space), () => 0);
        const alternate = Option.filter(Record.get(settings.features, JUSTIFICATION_ALTERNATES), Number.isGreaterThan(0));
        const isWordSpace = (text: string, cluster: number) => WORD_SPACE.test(text.charAt(cluster));
        const isLetter = (text: string, { class: kind, xAdvance, cluster }: ShapedText['glyphs'][number]) => kind !== 'MARK' && xAdvance !== 0 && !SPACE_SEPARATOR.test(text.charAt(cluster));
        const spaced = (text: string, glyphs: ShapedText['glyphs']) => {
            const joined = inOneJoin(text);
            const letters = Array.filterMap(glyphs, (each, at) => (isLetter(text, each) ? Result.succeed({ at, cluster: each.cluster }) : Result.failVoid));
            return new Set(Array.filterMap(Array.window(letters, 2), ([before, after]) => (joined(before.cluster, after.cluster) ? Result.failVoid : Result.succeed(before.at))));
        };
        const measured = (run: Run, shaped = ShapedText.of(face, run, settings)): Measurement => ({
            run,
            shaped,
            widths: { word: Number.sumAll(shaped.glyphs.filter(({ cluster }) => isWordSpace(run.text, cluster)).map(Struct.get('xAdvance'))), letter: spaced(run.text, shaped.glyphs).size * spaceAdvance, glyph: Number.sumAll(shaped.glyphs.filter((each) => isLetter(run.text, each)).map(Struct.get('xAdvance'))) },
        });
        const addedWidths = ({ widths }: Measurement, ratios: Ratios): Ratios => Record.map(widths, (base, kind) => base * (ratios[kind] - IDENTITY[kind]));
        const remainder = (layout: Measurement, ratios: Ratios) => width - layout.shaped.advance - Number.sumAll(Object.values(addedWidths(layout, ratios)));
        const fitRatio = (kind: keyof Ratios, layout: Measurement, ratios: Ratios): Ratios => ({ ...ratios, [kind]: Number.clamp(ratios[kind] + Option.getOrElse(Number.divide(remainder(layout, ratios), layout.widths[kind]), () => 0), { minimum: spacing[kind].min, maximum: spacing[kind].max }) });
        const natural = measured({ ...source, text: settings.digits(source.text), features: Option.isSome(alternate) ? [new Feature(JUSTIFICATION_ALTERNATES, 0), ...source.features] : source.features, flags: Array.union(source.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) });
        const wordRatios = fitRatio('word', natural, Record.map(spacing, Struct.get('desired')));
        const withFeature = (layout: Measurement, feature: Feature) => measured({ ...layout.run, features: [...layout.run.features, feature] });
        const closer = (layout: Measurement, next: Measurement) => {
            const [before, after] = [remainder(layout, wordRatios), remainder(next, wordRatios)];
            return Math.abs(after) < Math.abs(before) && before * after >= 0 ? next : layout;
        };
        const withAlternates = Option.match(alternate, { onNone: () => natural, onSome: (value) => closer(natural, withFeature(natural, new Feature(JUSTIFICATION_ALTERNATES, value))) });
        const withStretchedAlternates = (layout: Measurement) => {
            const shapedClusters = new Set(layout.shaped.glyphs.map(Struct.get('cluster')));
            return Justification.kashidaClusters(layout.run.text, (cluster) => shapedClusters.has(cluster)).reduce((closest, cluster) => {
                const glyphAt = ({ shaped }: Measurement) => Array.findFirst(shaped.glyphs, (glyph) => glyph.cluster === cluster).pipe(Option.map(Struct.get('glyph')));
                const probe = (value: number) => withFeature(closest, new Feature(JUSTIFICATION_ALTERNATES, value, cluster, cluster + 1));
                const base = glyphAt(probe(0));
                return Array.unfold(1, (value) => Option.liftPredicate([probe(value), value + 1] as const, ([candidate]) => !Equal.equals(glyphAt(candidate), base))).reduce(closer, closest);
            }, layout);
        };
        const withTatweels = (layout: Measurement, { lower, upper, fractional }: Extract<Settings['kashida'], { readonly lower: number }>): Measurement => {
            const safeClusters = new Set(layout.shaped.glyphs.filter(({ flags }) => flags.includes('SAFE_TO_INSERT_TATWEEL')).map(Struct.get('cluster')));
            const clusters = Justification.kashidaClusters(layout.run.text, (cluster) => safeClusters.has(cluster));
            const inserted = Option.gen(function* () {
                const [nuqta, tatweel] = yield* Option.all([face.nuqta, Option.filter(advance(TATWEEL), Number.isGreaterThan(0))]);
                const room = Math.max(0, remainder(layout, wordRatios)) / nuqta;
                const count = yield* Option.liftPredicate(room < lower ? 0 : Math.min(clusters.length, Math.floor(room / lower), Math.ceil(room / upper)), Number.isGreaterThan(0));
                const length = Number.clamp(room / count, { minimum: lower, maximum: upper }) * nuqta;
                const tatweels = yield* Option.liftPredicate(fractional ? Math.ceil(length / tatweel) : Math.floor(length / tatweel), Number.isGreaterThan(0));
                const chosen = Array.sort(Array.take(clusters, count), Order.Number);
                const scale = fractional ? length / (tatweels * tatweel) : 1;
                const shifted = (offset: number) => offset + tatweels * chosen.filter((cluster) => cluster < offset).length;
                const run = {
                    ...layout.run,
                    text: chosen.reduceRight((text, cluster) => `${text.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${text.slice(cluster)}`, layout.run.text),
                    item: Option.map(layout.run.item, ({ start, end }) => ({ start: shifted(start), end: shifted(end) })),
                    features: layout.run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start), Math.min(shifted(end), Feature.GLOBAL_END))),
                };
                const stretched = new Set(chosen.flatMap((cluster) => Array.makeBy(tatweels, (index) => shifted(cluster) + index)));
                const { glyphs, missing } = ShapedText.of(face, run, settings);
                return measured(run, ShapedText.make({ glyphs: glyphs.map((each) => (stretched.has(each.cluster) ? { ...each, xAdvance: each.xAdvance * scale } : each)), missing }));
            });
            return Option.getOrElse(inserted, () => layout);
        };
        const elongated = Match.valueTags(kashida, {
            None: () => withAlternates,
            Tatweel: (elongation) => withTatweels(withAlternates, elongation),
            Stretched: (elongation) => withTatweels(withStretchedAlternates(withAlternates), elongation),
        });
        const finalRatios = fitRatio('glyph', elongated, fitRatio('letter', elongated, wordRatios));
        const slots = spaced(elongated.run.text, elongated.shaped.glyphs);
        const [, positioned] = Array.mapAccum(elongated.shaped.glyphs, { cluster: 0, addedAdvance: 0 }, (previous, each, at) => {
            const letterGlyph = isLetter(elongated.run.text, each);
            const otherRatio = isWordSpace(elongated.run.text, each.cluster) ? finalRatios.word : 1;
            const xAdvance = each.xAdvance * (letterGlyph ? finalRatios.glyph : otherRatio) + (slots.has(at) ? finalRatios.letter * spaceAdvance : 0);
            const scaled = { ...each, xOffset: each.xOffset * finalRatios.glyph, extents: Option.map(each.extents, (box) => ({ ...box, xBearing: box.xBearing * finalRatios.glyph, width: box.width * finalRatios.glyph })) };
            return each.xAdvance === 0
                ? ([previous, { ...scaled, xOffset: scaled.xOffset - (each.cluster >= previous.cluster ? previous.addedAdvance : 0) }] as const)
                : ([
                      { cluster: each.cluster, addedAdvance: xAdvance - each.xAdvance * finalRatios.glyph },
                      { ...scaled, xAdvance },
                  ] as const);
        });
        const justified = ShapedText.make({ glyphs: positioned, missing: elongated.shaped.missing });
        return Justification.make({ text: elongated.run.text, shaped: justified, slack: width - justified.advance, ratios: finalRatios, addedWidths: { ...addedWidths(elongated, finalRatios), kashida: elongated.shaped.advance - natural.shaped.advance } });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inOneJoin = (text: string) => {
    const joins = Word.of(text).flatMap(Struct.get('joins'));
    const joinAt = (cluster: number) => Array.findFirstIndex(joins, ({ start, end }) => start <= cluster && cluster < end);
    return (before: number, after: number) => Option.exists(joinAt(before), (join) => Option.contains(joinAt(after), join));
};
const kashidaCandidates = (text: string, { start, end }: Word['joins'][number]) => {
    const { NoJoiningGroup, Seen, Sad, Yeh, FarsiYeh, TehMarbuta, Heh, Dal, Alef, Tah, Lam, Kaf, Gaf, Beh, Reh, Waw, Ain, Qaf, Feh } = JoiningGroup;
    const among =
        (...groups: readonly JoiningGroup[]) =>
        (group: JoiningGroup) =>
            groups.includes(group);
    const any = Function.constTrue;
    const notYeh = Predicate.not(among(Yeh, FarsiYeh));
    const rows: readonly { readonly rank: number; readonly before: Predicate.Predicate<JoiningGroup>; readonly after: Predicate.Predicate<JoiningGroup>; readonly next: Predicate.Predicate<Option.Option<JoiningGroup>> }[] = [
        { rank: 0, before: any, after: among(NoJoiningGroup), next: any },
        { rank: 1, before: among(Seen), after: any, next: any },
        { rank: 1, before: among(Sad), after: notYeh, next: any },
        { rank: 1, before: among(Sad), after: any, next: Option.isSome },
        { rank: 2, before: any, after: among(TehMarbuta, Heh, Dal), next: Option.isNone },
        { rank: 3, before: any, after: among(Alef, Tah, Lam, Kaf, Gaf), next: Option.isNone },
        { rank: 4, before: any, after: among(Beh), next: Option.exists(among(Reh, Yeh, FarsiYeh)) },
        { rank: 5, before: any, after: among(Waw, Ain, Qaf, Feh), next: Option.isNone },
        { rank: 6, before: any, after: notYeh, next: Option.isNone },
    ];
    const letters = Word.letters(text.slice(start, end));
    return Array.window(letters, 2).flatMap(([before, after], at) => {
        const next = Option.map(Array.get(letters, at + 2), Struct.get('group'));
        const row = Array.findFirst(rows, (each) => each.before(before.group) && each.after(after.group) && each.next(next));
        return Option.toArray(row).map(({ rank }) => ({ cluster: start + after.offset, rank }));
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Justification };
