import { Array, Data, Function, Match, Number, Option, Order, Record, Struct, UndefinedOr } from 'effect';
import { Feature } from 'harfbuzzjs';
import { GeneralCategory, JoiningGroup } from 'icu';
import { Settings } from '#/composition/settings';
import type { Face } from '#/face';
import { Shaped } from '#/shaped';
import { Word } from '#/text/words';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = typeof Shaped.Run.Type;
type Levels = { readonly [K in keyof typeof Settings.Type.spacing]: number };
interface Layout {
    readonly run: Run;
    readonly shaped: Shaped;
    readonly basis: Levels;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const ALTERNATES = 'jalt';
const NATURAL: Levels = Record.map(Settings.make({}).spacing, Struct.get('desired'));
const RANKED = Order.Struct({ rank: Order.Number, cluster: Order.flip(Order.Number) });

// --- [MODELS] --------------------------------------------------------------------------

class Justification extends Data.Class<
    Omit<Layout, 'basis'> & {
        readonly slack: number;
        readonly levels: Levels;
        readonly amounts: Levels & { readonly kashida: number };
    }
> {
    static readonly places = (text: string, allowed: (cluster: number) => boolean = Function.constTrue) => {
        const best = ({ runs }: Word) => {
            const found = runs.flatMap((run) => joins(text, run)).filter(({ cluster }) => allowed(cluster));
            return Array.take(Array.sort(found, RANKED), 1);
        };
        return Array.sort(Word.of(text).flatMap(best), RANKED).map(Struct.get('cluster'));
    };
    static readonly justify = (face: Face, source: Run, width: number, { kashida, spacing }: Settings): Justification => {
        const spacePoint = 0x20;
        const advance = (point: number) => UndefinedOr.match(face.font.nominalGlyph(point), { onUndefined: () => 0, onDefined: (glyph) => face.font.glyphHAdvance(glyph) / face.unitsPerEm });
        const space = advance(spacePoint);
        const grade = Settings.fields.kashida.schema.fields.length.schema.literals.indexOf(kashida.length);
        const spaced = (text: string, cluster: number) => GeneralCategory.forChar(text.codePointAt(cluster) ?? 0) === GeneralCategory.SpaceSeparator;
        const lettered = (text: string, { class: kind, xAdvance, cluster }: Shaped['glyphs'][number]) => kind !== 'MARK' && xAdvance !== 0 && !spaced(text, cluster);
        const laid = (run: Run, shaped = Shaped.of(face, run)): Layout => {
            const letters = shaped.glyphs.filter((each) => lettered(run.text, each));
            return { run, shaped, basis: { word: Number.sumAll(shaped.glyphs.filter(({ cluster }) => spaced(run.text, cluster)).map(Struct.get('xAdvance'))), letter: Math.max(0, letters.length - 1) * space, glyph: Number.sumAll(letters.map(Struct.get('xAdvance'))) } };
        };
        const added = ({ basis }: Layout, levels: Levels): Levels => Record.map(basis, (base, kind) => base * (levels[kind] - NATURAL[kind]));
        const remainder = (layout: Layout, levels: Levels) => width - layout.shaped.advance - Number.sumAll(Object.values(added(layout, levels)));
        const spread = (kind: keyof Levels, layout: Layout, levels: Levels): Levels => ({
            ...levels,
            [kind]: Number.clamp(levels[kind] + Option.getOrElse(Number.divide(remainder(layout, levels), layout.basis[kind]), () => 0), { minimum: spacing[kind].min, maximum: spacing[kind].max }),
        });
        const natural = laid({ ...source, flags: Array.union(source.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) });
        const word = spread('word', natural, Record.map(spacing, Struct.get('desired')));
        const fitted = (layout: Layout, features: Run['features']) => {
            const next = laid({ ...layout.run, features });
            const [before, after] = [remainder(layout, word), remainder(next, word)];
            return Math.abs(after) < Math.abs(before) && before * after >= 0 ? next : layout;
        };
        const alternated = kashida.alternates ? fitted(natural, [...natural.run.features, new Feature(ALTERNATES)]) : natural;
        const inserted = (layout: Layout): Layout => {
            const safe = new Set(layout.shaped.glyphs.filter(({ flags }) => flags.includes('SAFE_TO_INSERT_TATWEEL')).map(Struct.get('cluster')));
            const clusters = Justification.places(layout.run.text, (cluster) => safe.has(cluster));
            const room = Math.max(
                0,
                Option.getOrElse(Number.divide(remainder(layout, word), advance(TATWEEL)), () => 0),
            );
            const units = Math.floor(room / grade);
            const rows = Array.sortWith(
                clusters
                    .map((cluster, at) => {
                        const length = kashida.fractional ? room / clusters.length : grade * Math.ceil((units - at) / clusters.length);
                        return { cluster, length, tatweels: Math.ceil(length) };
                    })
                    .filter(({ tatweels }) => tatweels > 0),
                Struct.get('cluster'),
                Order.Number,
            );
            const shifted = (offset: number) => offset + Number.sumAll(rows.filter(({ cluster }) => cluster < offset).map(Struct.get('tatweels')));
            const run = {
                ...layout.run,
                text: rows.reduceRight((text, { cluster, tatweels }) => `${text.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${text.slice(cluster)}`, layout.run.text),
                features: layout.run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start), Math.min(shifted(end), Feature.GLOBAL_END))),
            };
            const ratios = new Map(rows.flatMap(({ cluster, length, tatweels }) => Array.makeBy(tatweels, (index) => [shifted(cluster) + index, length / tatweels] as const)));
            const { glyphs, missing } = Shaped.of(face, run);
            return laid(run, Shaped.make({ glyphs: glyphs.map((each) => ({ ...each, xAdvance: each.xAdvance * (ratios.get(each.cluster) ?? 1) })), missing }));
        };
        const stretched = (layout: Layout) => Array.cartesian(Justification.places(layout.run.text), Array.range(1, grade)).reduce((held, [cluster, value]) => fitted(held, [...held.run.features, new Feature(ALTERNATES, value, cluster, cluster + 1)]), layout);
        const elongated = Match.value(kashida).pipe(
            Match.whenOr({ insertion: 'none' }, { length: 'none' }, () => alternated),
            Match.when({ insertion: 'tatweel' }, () => inserted(alternated)),
            Match.orElse(() => inserted(stretched(alternated))),
        );
        const reached = spread('glyph', elongated, spread('letter', elongated, word));
        const last = Array.findLastIndex(elongated.shaped.glyphs, (each) => lettered(elongated.run.text, each));
        const [, placed] = Array.mapAccum(elongated.shaped.glyphs, { cluster: 0, drift: 0 }, (carried, each, at) => {
            const letter = lettered(elongated.run.text, each);
            const unlettered = spaced(elongated.run.text, each.cluster) ? reached.word : 1;
            const xAdvance = each.xAdvance * (letter ? reached.glyph : unlettered) + (letter && !Option.contains(last, at) ? reached.letter * space : 0);
            const scaled = { ...each, xOffset: each.xOffset * reached.glyph, extents: Option.map(each.extents, (box) => ({ ...box, xBearing: box.xBearing * reached.glyph, width: box.width * reached.glyph })) };
            return each.xAdvance === 0
                ? ([carried, { ...scaled, xOffset: scaled.xOffset - (each.cluster >= carried.cluster ? carried.drift : 0) }] as const)
                : ([
                      { cluster: each.cluster, drift: xAdvance - each.xAdvance * reached.glyph },
                      { ...scaled, xAdvance },
                  ] as const);
        });
        const composed = Shaped.make({ glyphs: placed, missing: elongated.shaped.missing });
        return new Justification({ run: elongated.run, shaped: composed, slack: width - composed.advance, levels: reached, amounts: { ...added(elongated, reached), kashida: elongated.shaped.advance - natural.shaped.advance } });
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const listed = (group: JoiningGroup, groups: readonly JoiningGroup[] = [group]) => groups.includes(group);
const joins = (text: string, { start, end }: Word['runs'][number]) => {
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
        const row = Array.findFirst(rows, (each) => listed(before.group, each.before) && listed(after.group, each.after) && Option.match(Array.get(letters, at + 2), { onNone: () => each.following === undefined, onSome: ({ group }) => each.final === undefined && listed(group, each.following) }));
        return Option.toArray(Option.map(Option.flatMapNullishOr(row, Struct.get('rank')), (rank) => ({ cluster: start + after.offset, rank })));
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Justification };
