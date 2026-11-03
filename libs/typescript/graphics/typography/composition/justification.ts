import { Array, Data, Function, Match, Number, Option, Order, Record, Struct, UndefinedOr } from 'effect';
import { Feature } from 'harfbuzzjs';
import { GeneralCategory, JoiningGroup } from 'icu';
import { Settings } from '#/composition/settings';
import type { Face } from '#/face';
import { ShapedText } from '#/shaped-text';
import { Word } from '#/text/words';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = typeof ShapedText.Run.Type;
type SpacingValues = { readonly [K in keyof typeof Settings.Type.spacing]: number };
interface ShapedRun {
    readonly run: Run;
    readonly shaped: ShapedText;
    readonly widths: SpacingValues;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const JUSTIFICATION_ALTERNATES = 'jalt';
const DEFAULT_RATIOS: SpacingValues = Record.map(Settings.make({}).spacing, Struct.get('desired'));
const BY_RANK = Order.Struct({ rank: Order.Number, cluster: Order.flip(Order.Number) });

// --- [MODELS] --------------------------------------------------------------------------

class Justification extends Data.Class<
    Omit<ShapedRun, 'widths'> & {
        readonly slack: number;
        readonly ratios: SpacingValues;
        readonly amounts: SpacingValues & { readonly kashida: number };
    }
> {
    static readonly kashidaClusters = (text: string, allowed: (cluster: number) => boolean = Function.constTrue) => {
        const best = ({ runs }: Word) => {
            const found = runs.flatMap((run) => kashidaCandidates(text, run)).filter(({ cluster }) => allowed(cluster));
            return Array.take(Array.sort(found, BY_RANK), 1);
        };
        return Array.sort(Word.of(text).flatMap(best), BY_RANK).map(Struct.get('cluster'));
    };
    static readonly justify = (face: Face, source: Run, width: number, { kashida, spacing }: Settings): Justification => {
        const spacePoint = 0x20;
        const advance = (point: number) => UndefinedOr.match(face.font.nominalGlyph(point), { onUndefined: () => 0, onDefined: (glyph) => face.font.glyphHAdvance(glyph) / face.unitsPerEm });
        const spaceAdvance = advance(spacePoint);
        const lengthIndex = Settings.fields.kashida.schema.fields.length.schema.literals.indexOf(kashida.length);
        const isSpace = (text: string, cluster: number) => GeneralCategory.forChar(text.codePointAt(cluster) ?? 0) === GeneralCategory.SpaceSeparator;
        const isLetter = (text: string, { class: kind, xAdvance, cluster }: ShapedText['glyphs'][number]) => kind !== 'MARK' && xAdvance !== 0 && !isSpace(text, cluster);
        const measured = (run: Run, shaped = ShapedText.of(face, run)): ShapedRun => {
            const letters = shaped.glyphs.filter((each) => isLetter(run.text, each));
            return { run, shaped, widths: { word: Number.sumAll(shaped.glyphs.filter(({ cluster }) => isSpace(run.text, cluster)).map(Struct.get('xAdvance'))), letter: Math.max(0, letters.length - 1) * spaceAdvance, glyph: Number.sumAll(letters.map(Struct.get('xAdvance'))) } };
        };
        const addedWidths = ({ widths }: ShapedRun, ratios: SpacingValues): SpacingValues => Record.map(widths, (base, kind) => base * (ratios[kind] - DEFAULT_RATIOS[kind]));
        const remainder = (layout: ShapedRun, ratios: SpacingValues) => width - layout.shaped.advance - Number.sumAll(Object.values(addedWidths(layout, ratios)));
        const fitRatio = (kind: keyof SpacingValues, layout: ShapedRun, ratios: SpacingValues): SpacingValues => ({
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
        const withTatweels = (layout: ShapedRun): ShapedRun => {
            const safeClusters = new Set(layout.shaped.glyphs.filter(({ flags }) => flags.includes('SAFE_TO_INSERT_TATWEEL')).map(Struct.get('cluster')));
            const clusters = Justification.kashidaClusters(layout.run.text, (cluster) => safeClusters.has(cluster));
            const room = Math.max(
                0,
                Option.getOrElse(Number.divide(remainder(layout, wordRatios), advance(TATWEEL)), () => 0),
            );
            const steps = Math.floor(room / lengthIndex);
            const insertions = Array.sortWith(
                clusters
                    .map((cluster, at) => {
                        const length = kashida.fractional ? room / clusters.length : lengthIndex * Math.ceil((steps - at) / clusters.length);
                        return { cluster, length, tatweels: Math.ceil(length) };
                    })
                    .filter(({ tatweels }) => tatweels > 0),
                Struct.get('cluster'),
                Order.Number,
            );
            const shifted = (offset: number) => offset + Number.sumAll(insertions.filter(({ cluster }) => cluster < offset).map(Struct.get('tatweels')));
            const run = {
                ...layout.run,
                text: insertions.reduceRight((text, { cluster, tatweels }) => `${text.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${text.slice(cluster)}`, layout.run.text),
                features: layout.run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start), Math.min(shifted(end), Feature.GLOBAL_END))),
            };
            const tatweelScales = new Map(insertions.flatMap(({ cluster, length, tatweels }) => Array.makeBy(tatweels, (index) => [shifted(cluster) + index, length / tatweels] as const)));
            const { glyphs, missing } = ShapedText.of(face, run);
            return measured(run, ShapedText.make({ glyphs: glyphs.map((each) => ({ ...each, xAdvance: each.xAdvance * (tatweelScales.get(each.cluster) ?? 1) })), missing }));
        };
        const withStretchedAlternates = (layout: ShapedRun) => Array.cartesian(Justification.kashidaClusters(layout.run.text), Array.range(1, lengthIndex)).reduce((closest, [cluster, value]) => closerFit(closest, [...closest.run.features, new Feature(JUSTIFICATION_ALTERNATES, value, cluster, cluster + 1)]), layout);
        const elongated = Match.value(kashida).pipe(
            Match.whenOr({ insertion: 'none' }, { length: 'none' }, () => withAlternates),
            Match.when({ insertion: 'tatweel' }, () => withTatweels(withAlternates)),
            Match.orElse(() => withTatweels(withStretchedAlternates(withAlternates))),
        );
        const finalRatios = fitRatio('glyph', elongated, fitRatio('letter', elongated, wordRatios));
        const lastLetter = Array.findLastIndex(elongated.shaped.glyphs, (each) => isLetter(elongated.run.text, each));
        const [, positioned] = Array.mapAccum(elongated.shaped.glyphs, { cluster: 0, addedAdvance: 0 }, (previous, each, at) => {
            const letterGlyph = isLetter(elongated.run.text, each);
            const otherRatio = isSpace(elongated.run.text, each.cluster) ? finalRatios.word : 1;
            const xAdvance = each.xAdvance * (letterGlyph ? finalRatios.glyph : otherRatio) + (letterGlyph && !Option.contains(lastLetter, at) ? finalRatios.letter * spaceAdvance : 0);
            const scaled = { ...each, xOffset: each.xOffset * finalRatios.glyph, extents: Option.map(each.extents, (box) => ({ ...box, xBearing: box.xBearing * finalRatios.glyph, width: box.width * finalRatios.glyph })) };
            return each.xAdvance === 0
                ? ([previous, { ...scaled, xOffset: scaled.xOffset - (each.cluster >= previous.cluster ? previous.addedAdvance : 0) }] as const)
                : ([
                      { cluster: each.cluster, addedAdvance: xAdvance - each.xAdvance * finalRatios.glyph },
                      { ...scaled, xAdvance },
                  ] as const);
        });
        const justified = ShapedText.make({ glyphs: positioned, missing: elongated.shaped.missing });
        return new Justification({ run: elongated.run, shaped: justified, slack: width - justified.advance, ratios: finalRatios, amounts: { ...addedWidths(elongated, finalRatios), kashida: elongated.shaped.advance - natural.shaped.advance } });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

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
