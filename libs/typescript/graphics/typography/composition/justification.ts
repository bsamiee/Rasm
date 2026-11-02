import { Array, Data, Effect, Function, Match, Number, Option, Order, Predicate, pipe, Record, Struct } from 'effect';
import { Feature } from 'harfbuzzjs';
import { Settings } from '#/composition/settings';
import type { Face } from '#/face';
import { Shaped } from '#/shaped';
import { Unicode } from '#/text/unicode';
import { Word } from '#/text/words';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = typeof Shaped.Run.Type;
type Levels = { readonly [K in keyof typeof Settings.Type.spacing]: number };
type Fit = { readonly width: number; readonly grade: number } & ({ readonly slack: number; readonly fractional: boolean } | { readonly count: number });
interface Layout {
    readonly run: Run;
    readonly shaped: Shaped;
}
interface Join {
    readonly cluster: number;
    readonly before: string;
    readonly after: string;
    readonly position: 'medial' | 'final';
    readonly following: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const ALTERNATES = 'jalt';
const NATURAL: Levels = Record.map(Settings.make({}).spacing, Struct.get('desired'));

// --- [MODELS] --------------------------------------------------------------------------

class Place extends Data.Class<{
    readonly cluster: number;
    readonly priority: number;
    readonly group: string;
}> {
    static readonly joins = [
        { side: 'after', groups: ['NoJoiningGroup'], position: [], following: [], priority: 0 },
        { side: 'before', groups: ['Seen'], position: [], following: [], priority: 1 },
        { side: 'after', groups: ['Yeh', 'FarsiYeh'], position: ['final'], following: [], priority: 'none' },
        { side: 'before', groups: ['Sad'], position: [], following: [], priority: 1 },
        { side: 'after', groups: ['TehMarbuta', 'Heh', 'Dal'], position: ['final'], following: [], priority: 2 },
        { side: 'after', groups: ['Alef', 'Tah', 'Lam', 'Kaf', 'Gaf'], position: ['final'], following: [], priority: 3 },
        { side: 'after', groups: ['Beh'], position: ['medial'], following: ['Reh', 'Yeh', 'FarsiYeh'], priority: 4 },
        { side: 'after', groups: ['Waw', 'Ain', 'Qaf', 'Feh'], position: ['final'], following: [], priority: 5 },
        { side: 'after', groups: [], position: ['final'], following: [], priority: 6 },
    ] as const;
    static readonly ranked = Order.combine(
        Order.mapInput(Order.Number, (place: Place) => place.priority),
        Order.flip(Order.mapInput(Order.Number, (place: Place) => place.cluster)),
    );
    static readonly of = (join: Join) => {
        const listed = (names: readonly string[], name: string) => Array.isReadonlyArrayEmpty(names) || Array.contains(names, name);
        return Option.flatMap(
            Array.findFirst(Place.joins, ({ side, groups, position, following }) => listed(groups, join[side]) && listed(position, join.position) && listed(following, join.following)),
            ({ side, priority }) => (Predicate.isNumber(priority) ? Option.some(new Place({ cluster: join.cluster, priority, group: join[side] })) : Option.none()),
        );
    };
    static readonly within = Effect.fn('Justification.Place.within')(function* (text: string, allowed: (cluster: number) => boolean) {
        const { GeneralCategory, GeneralCategoryGroup, JoiningGroup } = yield* Unicode;
        const letter = GeneralCategoryGroup.letter();
        const words = yield* Word.of(text);
        const joins = ({ start, end }: Word['runs'][number]): readonly Join[] => {
            const [, read] = Array.mapAccum(Array.fromIterable(text.slice(start, end)), start, (offset, character) => [offset + character.length, { offset, point: character.codePointAt(0) ?? 0 }] as const);
            const letters = read.filter(({ point }) => letter.contains(GeneralCategory.forChar(point))).map(({ offset, point }) => ({ offset, group: JoiningGroup.forChar(point).value }));
            return Array.window(letters, 2).map(([before, after], at) => {
                const following = Array.get(letters, at + 2);
                return { cluster: after.offset, before: before.group, after: after.group, position: Option.isNone(following) ? 'final' : 'medial', following: Option.match(following, { onNone: () => '', onSome: Struct.get('group') }) };
            });
        };
        const best = ({ runs }: Word) =>
            pipe(
                runs
                    .flatMap(joins)
                    .filter(({ cluster }) => allowed(cluster))
                    .map(Place.of),
                Array.getSomes,
                Array.sort(Place.ranked),
                Array.head,
                Option.toArray,
            );
        return Array.sort(words.flatMap(best), Place.ranked);
    });
}
class Justification extends Data.Class<
    Layout & {
        readonly slack: number;
        readonly scale: number;
        readonly mechanisms: readonly { readonly kind: 'word' | 'kashida' | 'letter' | 'glyph'; readonly amount: number }[];
    }
> {
    static readonly Place = Place;
    static readonly places = (text: string, shaped: Shaped) => {
        const flagged = new Set(shaped.glyphs.filter(({ flags }) => flags.includes('SAFE_TO_INSERT_TATWEEL')).map(Struct.get('cluster')));
        return Place.within(text, (cluster) => flagged.has(cluster));
    };
    static readonly width = (face: Face) =>
        Option.map(
            Option.orElse(Option.fromUndefinedOr(face.font.nominalGlyph(TATWEEL)), () => Array.head(face.extenders)),
            advance(face),
        );
    static readonly grade = (length: typeof Settings.Type.kashida.length) => Settings.fields.kashida.schema.fields.length.schema.literals.indexOf(length);
    static readonly fit = ({ width, grade, ...given }: Fit) => {
        const count = Match.value(given).pipe(
            Match.when({ count: Match.number }, (solved) => solved.count),
            Match.orElse(({ slack, fractional }) => {
                const solved = Option.getOrElse(Number.divide(slack, grade * width), () => 0);
                return Math.max(0, fractional ? solved : Math.floor(solved));
            }),
        );
        return { count, slack: count * grade * width };
    };
    static readonly justify = Effect.fn('Justification.justify')(function* (face: Face, source: Run, width: number, { kashida, spacing }: Settings) {
        const { GeneralCategory } = yield* Unicode;
        const spacePoint = 0x20;
        const space = Option.getOrElse(Option.map(Option.fromUndefinedOr(face.font.nominalGlyph(spacePoint)), advance(face)), () => 0);
        const laid = (run: Run): Layout => ({ run, shaped: Shaped.of(face, run) });
        const spaced = ({ run: { text } }: Layout, cluster: number) => GeneralCategory.forChar(text.codePointAt(cluster) ?? 0) === GeneralCategory.SpaceSeparator;
        const lettered = (layout: Layout, { class: kind, xAdvance, cluster }: Shaped['glyphs'][number]) => kind !== 'MARK' && xAdvance > 0 && !spaced(layout, cluster);
        const bases = (layout: Layout): Levels => {
            const letters = layout.shaped.glyphs.filter((each) => lettered(layout, each));
            return {
                word: Number.sumAll(layout.shaped.glyphs.filter(({ cluster }) => spaced(layout, cluster)).map(Struct.get('xAdvance'))),
                letter: Math.max(0, letters.length - 1) * space,
                glyph: Number.sumAll(letters.map(Struct.get('xAdvance'))),
            };
        };
        const remainder = (layout: Layout, levels: Levels) => {
            const basis = bases(layout);
            return width - layout.shaped.advance - Number.sumAll(Record.collect(levels, (kind, level) => basis[kind] * (level - NATURAL[kind])));
        };
        const spread = (kind: keyof Levels, layout: Layout, levels: Levels): Levels => ({
            ...levels,
            [kind]: Number.clamp(levels[kind] + Option.getOrElse(Number.divide(remainder(layout, levels), bases(layout)[kind]), () => 0), { minimum: spacing[kind].min, maximum: spacing[kind].max }),
        });
        const natural = laid({ ...source, flags: Array.union(source.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) });
        const word = spread('word', natural, Record.map(spacing, Struct.get('desired')));
        const fitted = (layout: Layout, features: Run['features']) => {
            const next = laid({ ...layout.run, features });
            const [before, after] = [remainder(layout, word), remainder(next, word)];
            return Math.abs(after) < Math.abs(before) && before * after >= 0 ? next : layout;
        };
        const alternated = kashida.alternates ? fitted(natural, [...natural.run.features, new Feature(ALTERNATES)]) : natural;
        const inserted = (ranked: readonly Place[]): Layout => {
            const grade = Justification.grade(kashida.length);
            const count = Option.match(
                Option.filter(Justification.width(face), () => face.has(TATWEEL)),
                { onNone: () => 0, onSome: (each) => Justification.fit({ slack: remainder(alternated, word), width: each, grade, fractional: kashida.fractional }).count },
            );
            const rows = Array.sortWith(
                ranked
                    .map(({ cluster }, at) => {
                        const length = grade * (kashida.fractional ? count / ranked.length : Math.floor(count / ranked.length) + (at < count % ranked.length ? 1 : 0));
                        return { cluster, length, tatweels: Math.ceil(length) };
                    })
                    .filter(({ tatweels }) => tatweels > 0),
                Struct.get('cluster'),
                Order.Number,
            );
            const shifted = (offset: number) => offset + Number.sumAll(rows.filter(({ cluster }) => cluster < offset).map(Struct.get('tatweels')));
            const run = {
                ...alternated.run,
                text: rows.reduceRight((text, { cluster, tatweels }) => `${text.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${text.slice(cluster)}`, alternated.run.text),
                features: alternated.run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start), Math.min(shifted(end), Feature.GLOBAL_END))),
            };
            const ratios = new Map(rows.flatMap(({ cluster, length, tatweels }) => Array.makeBy(tatweels, (index) => [shifted(cluster) + index, length / tatweels] as const)));
            const { glyphs, missing } = Shaped.of(face, run);
            return { run, shaped: Shaped.make({ glyphs: glyphs.map((each) => ({ ...each, xAdvance: each.xAdvance * (ratios.get(each.cluster) ?? 1) })), missing }) };
        };
        const stretched = (ranked: readonly Place[]) => ranked.reduce<Layout>((layout, { cluster }) => fitted(layout, [...layout.run.features, new Feature(ALTERNATES, 1, cluster, cluster + 1)]), alternated);
        const elongated = yield* Match.value(kashida).pipe(
            Match.whenOr({ insertion: 'none' }, { length: 'none' }, () => Effect.succeed(alternated)),
            Match.when({ insertion: 'tatweel' }, () => Effect.map(Justification.places(source.text, alternated.shaped), inserted)),
            Match.orElse(() => Effect.map(Place.within(source.text, Function.constTrue), stretched)),
        );
        const reached = spread('glyph', elongated, spread('letter', elongated, word));
        const last = Array.findLastIndex(elongated.shaped.glyphs, (each) => lettered(elongated, each));
        const [, placed] = Array.mapAccum(elongated.shaped.glyphs, { cluster: 0, drift: 0 }, (carried, each, at) => {
            const letter = lettered(elongated, each);
            const unlettered = spaced(elongated, each.cluster) ? reached.word : 1;
            const xAdvance = each.xAdvance * (letter ? reached.glyph : unlettered) + (letter && !Option.contains(last, at) ? reached.letter * space : 0);
            const xOffset = each.xOffset * reached.glyph;
            return each.class === 'MARK'
                ? ([carried, { ...each, xOffset: xOffset - (each.cluster >= carried.cluster ? carried.drift : 0) }] as const)
                : ([
                      { cluster: each.cluster, drift: xAdvance - each.xAdvance * reached.glyph },
                      { ...each, xAdvance, xOffset },
                  ] as const);
        });
        const shaped = Shaped.make({ glyphs: placed, missing: elongated.shaped.missing });
        const added = (kind: keyof Levels) => bases(elongated)[kind] * (reached[kind] - NATURAL[kind]);
        return new Justification({
            run: elongated.run,
            shaped,
            slack: width - shaped.advance,
            scale: reached.glyph,
            mechanisms: [
                { kind: 'word', amount: added('word') },
                { kind: 'kashida', amount: elongated.shaped.advance - natural.shaped.advance },
                { kind: 'letter', amount: added('letter') },
                { kind: 'glyph', amount: added('glyph') },
            ],
        });
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const advance = (face: Face) => (glyph: number) => face.font.glyphHAdvance(glyph) / face.unitsPerEm;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Justification };
