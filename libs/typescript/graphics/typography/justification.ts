import { Array, Function, Match, Number, Option, Order, Record, Schema, Struct } from 'effect';
import { Feature } from 'harfbuzzjs';
import { GeneralCategory, JoiningGroup } from 'icu';
import { Settings } from '#/composition/settings';
import type { Face } from '#/face';
import { ShapedText } from '#/shaped-text';
import { Word } from '#/text/words';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = typeof ShapedText.Run.Type;
type Spacings = typeof Spacings.Type;
interface ShapedRun {
    readonly run: Run;
    readonly shaped: ShapedText;
    readonly widths: Spacings;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const JUSTIFICATION_ALTERNATES = 'jalt';
const DEFAULT_RATIOS: Spacings = Record.map(Settings.make({}).spacing, Struct.get('desired'));
const BY_RANK = Order.Struct({ rank: Order.Number, cluster: Order.flip(Order.Number) });

// --- [MODELS] --------------------------------------------------------------------------

const Spacings = Schema.Struct(Struct.Record(Struct.keys(Settings.fields.spacing.schema.fields), Schema.Finite));
class Justification extends Schema.Class<Justification, { readonly brand: unique symbol }>('Justification')({
    run: ShapedText.Run.annotate({ description: 'Run with inserted tatweels, glyph clusters index its text' }),
    shaped: ShapedText,
    slack: Schema.Finite.annotate({ description: 'Width left in em, negative when overfull' }),
    ratios: Spacings.annotate({ description: 'Applied spacing ratios' }),
    amounts: Spacings.mapFields(Struct.assign({ kashida: Schema.Finite })).annotate({ description: 'Width each mechanism added in em' }),
}) {
    static readonly kashidaClusters = (text: string, allowed: (cluster: number) => boolean = Function.constTrue) => {
        const best = ({ runs }: Word) => {
            const found = runs.flatMap((run) => kashidaCandidates(text, run)).filter(({ cluster }) => allowed(cluster));
            return Option.toArray(Option.map(Option.liftPredicate(found, Array.isArrayNonEmpty), (some) => Array.min(some, BY_RANK)));
        };
        return Array.sort(Word.of(text).flatMap(best), BY_RANK).map(Struct.get('cluster'));
    };
    static readonly justify = (face: Face, source: Run, width: number, settings: Settings): Justification => {
        const { kashida, spacing } = settings;
        const spacePoint = 0x20;
        const advance = (point: number) => Option.map(Option.fromUndefinedOr(face.font.nominalGlyph(point)), (glyph) => face.font.glyphHAdvance(glyph) / face.unitsPerEm);
        const spaceAdvance = Option.getOrElse(advance(spacePoint), () => 0);
        const isSpace = (text: string, cluster: number) => GeneralCategory.forChar(text.codePointAt(cluster) ?? 0) === GeneralCategory.SpaceSeparator;
        const isLetter = (text: string, { class: kind, xAdvance, cluster }: ShapedText['glyphs'][number]) => kind !== 'MARK' && xAdvance !== 0 && !isSpace(text, cluster);
        const spaced = (text: string, glyphs: ShapedText['glyphs']) => {
            const joined = joinedClusters(text);
            const letters = Array.getSomes(
                glyphs.map((each, at) =>
                    Option.map(
                        Option.liftPredicate(each, (glyph) => isLetter(text, glyph)),
                        ({ cluster }) => ({ at, cluster }),
                    ),
                ),
            );
            return new Set(
                Array.window(letters, 2)
                    .filter(([before, after]) => !joined(before.cluster, after.cluster))
                    .map(([before]) => before.at),
            );
        };
        const measured = (run: Run, shaped = ShapedText.of(face, run, settings)): ShapedRun => {
            const letters = shaped.glyphs.filter((each) => isLetter(run.text, each));
            return { run, shaped, widths: { word: Number.sumAll(shaped.glyphs.filter(({ cluster }) => isSpace(run.text, cluster)).map(Struct.get('xAdvance'))), letter: spaced(run.text, shaped.glyphs).size * spaceAdvance, glyph: Number.sumAll(letters.map(Struct.get('xAdvance'))) } };
        };
        const addedWidths = ({ widths }: ShapedRun, ratios: Spacings): Spacings => Record.map(widths, (base, kind) => base * (ratios[kind] - DEFAULT_RATIOS[kind]));
        const remainder = (layout: ShapedRun, ratios: Spacings) => width - layout.shaped.advance - Number.sumAll(Object.values(addedWidths(layout, ratios)));
        const fitRatio = (kind: keyof Spacings, layout: ShapedRun, ratios: Spacings): Spacings => ({
            ...ratios,
            [kind]: Number.clamp(ratios[kind] + Option.getOrElse(Number.divide(remainder(layout, ratios), layout.widths[kind]), () => 0), { minimum: spacing[kind].min, maximum: spacing[kind].max }),
        });
        const natural = measured({ ...source, flags: Array.union(source.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) });
        const wordRatios = fitRatio('word', natural, Record.map(spacing, Struct.get('desired')));
        const closerFit = (layout: ShapedRun, features: Run['features']) => {
            const next = measured({ ...layout.run, features });
            const [before, after] = [remainder(layout, wordRatios), remainder(next, wordRatios)];
            return Math.abs(after) < Math.abs(before) && before * after >= 0 ? next : layout;
        };
        const withAlternates = kashida.alternates ? closerFit(natural, [...natural.run.features, new Feature(JUSTIFICATION_ALTERNATES)]) : natural;
        const withTatweels = (layout: ShapedRun, { nuqta, tatweel }: { readonly nuqta: number; readonly tatweel: number }): ShapedRun => {
            const safeClusters = new Set(layout.shaped.glyphs.filter(({ flags }) => flags.includes('SAFE_TO_INSERT_TATWEEL')).map(Struct.get('cluster')));
            const clusters = Justification.kashidaClusters(layout.run.text, (cluster) => safeClusters.has(cluster));
            const room = Math.max(0, remainder(layout, wordRatios)) / nuqta;
            const places = room < kashida.degree ? 0 : Math.min(clusters.length, Math.floor(room / kashida.degree), Math.ceil(room / kashida.upper));
            const elongation = Option.map(Option.liftPredicate(places, Number.isGreaterThan(0)), (count) => {
                const length = Number.clamp(room / count, { minimum: kashida.degree, maximum: kashida.upper }) * nuqta;
                const tatweels = Math.ceil(length / tatweel);
                return { clusters: Array.sort(Array.take(clusters, count), Order.Number), tatweels, scale: kashida.fractional ? length / (tatweels * tatweel) : 1 };
            });
            const inserted = ({ clusters: chosen, tatweels, scale }: Option.Option.Value<typeof elongation>) => {
                const shifted = (offset: number) => offset + tatweels * chosen.filter((cluster) => cluster < offset).length;
                const run = {
                    ...layout.run,
                    text: chosen.reduceRight((text, cluster) => `${text.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${text.slice(cluster)}`, layout.run.text),
                    features: layout.run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start), Math.min(shifted(end), Feature.GLOBAL_END))),
                };
                const stretched = new Set(chosen.flatMap((cluster) => Array.makeBy(tatweels, (index) => shifted(cluster) + index)));
                const { glyphs, missing } = ShapedText.of(face, run, settings);
                return measured(run, ShapedText.make({ glyphs: glyphs.map((each) => (stretched.has(each.cluster) ? { ...each, xAdvance: each.xAdvance * scale } : each)), missing }));
            };
            return Option.match(elongation, { onNone: () => layout, onSome: inserted });
        };
        const withStretchedAlternates = (layout: ShapedRun) => Array.cartesian(Justification.kashidaClusters(layout.run.text), Array.range(1, kashida.degree)).reduce((closest, [cluster, value]) => closerFit(closest, [...closest.run.features, new Feature(JUSTIFICATION_ALTERNATES, value, cluster, cluster + 1)]), layout);
        const elongated = Option.match(Option.all({ nuqta: face.nuqta, tatweel: Option.filter(advance(TATWEEL), Number.isGreaterThan(0)) }), {
            onNone: () => withAlternates,
            onSome: (units) =>
                Match.value(kashida).pipe(
                    Match.whenOr({ insertion: 'none' }, { degree: 0 }, () => withAlternates),
                    Match.when({ insertion: 'tatweel' }, () => withTatweels(withAlternates, units)),
                    Match.orElse(() => withTatweels(withStretchedAlternates(withAlternates), units)),
                ),
        });
        const finalRatios = fitRatio('glyph', elongated, fitRatio('letter', elongated, wordRatios));
        const slots = spaced(elongated.run.text, elongated.shaped.glyphs);
        const [, positioned] = Array.mapAccum(elongated.shaped.glyphs, { cluster: 0, addedAdvance: 0 }, (previous, each, at) => {
            const letterGlyph = isLetter(elongated.run.text, each);
            const otherRatio = isSpace(elongated.run.text, each.cluster) ? finalRatios.word : 1;
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
        return Justification.make({ run: elongated.run, shaped: justified, slack: width - justified.advance, ratios: finalRatios, amounts: { ...addedWidths(elongated, finalRatios), kashida: elongated.shaped.advance - natural.shaped.advance } });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const joinedClusters = (text: string) => {
    const runs = Word.of(text).flatMap(Struct.get('runs'));
    const runAt = (cluster: number) => Array.findFirstIndex(runs, ({ start, end }) => start <= cluster && cluster < end);
    return (before: number, after: number) =>
        Option.getOrElse(
            Option.zipWith(runAt(before), runAt(after), (left, right) => left === right),
            () => false,
        );
};
const inGroups = (group: JoiningGroup, groups: readonly JoiningGroup[] = [group]) => groups.includes(group);
const kashidaCandidates = (text: string, { start, end }: Word['runs'][number]) => {
    const { NoJoiningGroup, Seen, Sad, Yeh, FarsiYeh, TehMarbuta, Heh, Dal, Alef, Tah, Lam, Kaf, Gaf, Beh, Reh, Waw, Ain, Qaf, Feh } = JoiningGroup;
    const rows: readonly { readonly rank?: number; readonly before?: readonly JoiningGroup[]; readonly after?: readonly JoiningGroup[]; readonly following?: readonly JoiningGroup[]; readonly final?: true }[] = [
        { rank: 0, after: [NoJoiningGroup] },
        { rank: 1, before: [Seen] },
        { after: [Yeh, FarsiYeh], final: true },
        { rank: 1, before: [Sad] },
        { rank: 2, after: [TehMarbuta, Heh, Dal], final: true },
        { rank: 3, after: [Alef, Tah, Lam, Kaf, Gaf], final: true },
        { rank: 4, after: [Beh], following: [Reh, Yeh, FarsiYeh] },
        { rank: 5, after: [Waw, Ain, Qaf, Feh], final: true },
        { rank: 6, final: true },
    ];
    const letters = Word.letters(text.slice(start, end));
    return Array.window(letters, 2).flatMap(([before, after], at) => {
        const row = Array.findFirst(rows, (each) => inGroups(before.group, each.before) && inGroups(after.group, each.after) && Option.match(Array.get(letters, at + 2), { onNone: () => each.following === undefined, onSome: ({ group }) => each.final === undefined && inGroups(group, each.following) }));
        return Option.toArray(Option.map(Option.flatMapNullishOr(row, Struct.get('rank')), (rank) => ({ cluster: start + after.offset, rank })));
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Justification };
