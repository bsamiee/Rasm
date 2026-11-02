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
interface Letter {
    readonly group: string;
    readonly position: string;
}
interface Join {
    readonly cluster: number;
    readonly before: Letter;
    readonly after: Letter;
    readonly following: string;
}
interface Layout {
    readonly run: Run;
    readonly shaped: Shaped;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const ALTERNATES = 'jalt';
const { spacing: NEUTRAL } = Settings.make({});

// --- [MODELS] --------------------------------------------------------------------------

class Place extends Data.Class<{
    readonly cluster: number;
    readonly priority: number;
    readonly group: string;
}> {
    static readonly joins = [
        { side: 'after', groups: ['Tatweel'], position: ['medial', 'final'], following: [], priority: 0 },
        { side: 'before', groups: ['Seen'], position: ['initial', 'medial'], following: [], priority: 1 },
        { side: 'after', groups: ['Yeh', 'FarsiYeh'], position: ['final'], following: [], priority: 'none' },
        { side: 'before', groups: ['Sad'], position: ['initial', 'medial'], following: [], priority: 1 },
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
            Array.findFirst(Place.joins, ({ side, groups, position, following }) => Array.contains<string>(position, join[side].position) && listed(groups, join[side].group) && listed(following, join.following)),
            ({ side, priority }) => (Predicate.isNumber(priority) ? Option.some(new Place({ cluster: join.cluster, priority, group: join[side].group })) : Option.none()),
        );
    };
    static readonly within = Effect.fn('Justification.Place.within')(function* (text: string, allowed: (cluster: number) => boolean) {
        const { JoiningGroup, JoiningType } = yield* Unicode;
        const words = yield* Word.of(text);
        const joins = ({ start, end }: { readonly start: number; readonly end: number }): readonly Join[] => {
            const [, read] = Array.mapAccum(Array.fromIterable(text.slice(start, end)), start, (offset, character) => [offset + character.length, { offset, point: character.codePointAt(0) ?? 0 }] as const);
            const letters = read.filter(({ point }) => JoiningType.forChar(point) !== JoiningType.Transparent).map(({ offset, point }) => ({ offset, group: point === TATWEEL ? 'Tatweel' : JoiningGroup.forChar(point).value }));
            return Array.zip(letters, Array.drop(letters, 1)).map(([before, after], at) => {
                const following = Array.get(letters, at + 2);
                return {
                    cluster: after.offset,
                    before: { group: before.group, position: at === 0 ? 'initial' : 'medial' },
                    after: { group: after.group, position: Option.isNone(following) ? 'final' : 'medial' },
                    following: Option.match(following, { onNone: () => '', onSome: Struct.get('group') }),
                };
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
class Justification extends Data.Class<{
    readonly run: Run;
    readonly shaped: Shaped;
    readonly slack: number;
    readonly mechanisms: readonly { readonly kind: 'word' | 'kashida' | 'letter' | 'glyph'; readonly amount: number }[];
}> {
    static readonly Place = Place;
    static readonly places = (text: string, shaped: Shaped) => {
        const flagged = new Set(shaped.glyphs.filter(({ flags }) => flags.includes('SAFE_TO_INSERT_TATWEEL')).map(Struct.get('cluster')));
        return Place.within(text, (cluster) => flagged.has(cluster));
    };
    static readonly width = (face: Face) =>
        Option.map(
            Option.orElse(Array.head(face.extenders), () => Option.fromUndefinedOr(face.font.nominalGlyph(TATWEEL))),
            (glyph) => face.font.glyphHAdvance(glyph) / face.unitsPerEm,
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
        const space = Option.getOrElse(
            Option.map(Option.fromUndefinedOr(face.font.nominalGlyph(' '.codePointAt(0) ?? 0)), (each) => face.font.glyphHAdvance(each) / face.unitsPerEm),
            () => 0,
        );
        const run = { ...source, flags: Array.union(source.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) };
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
            return width - layout.shaped.advance - Number.sumAll(Record.collect(levels, (kind, level) => basis[kind] * (level - NEUTRAL[kind].desired)));
        };
        const spread = (kind: keyof Levels, layout: Layout, levels: Levels) => {
            const { min, desired, max } = spacing[kind];
            const basis = bases(layout)[kind];
            const amount = Number.clamp(remainder(layout, levels), { minimum: basis * (min - desired), maximum: basis * (max - desired) });
            return { amount, levels: { ...levels, [kind]: desired + Option.getOrElse(Number.divide(amount, basis), () => 0) } };
        };
        const natural = { run, shaped: Shaped.of(face, run) };
        const word = spread('word', natural, Record.map(spacing, Struct.get('desired')));
        const allowed = remainder(natural, word.levels);
        const fitted = (layout: Layout, features: Run['features']) => {
            const next = { ...layout.run, features };
            const shaped = Shaped.of(face, next);
            return shaped.advance !== layout.shaped.advance && shaped.advance - natural.shaped.advance <= allowed ? { run: next, shaped } : layout;
        };
        const alternated = kashida.alternates ? fitted(natural, [...run.features, new Feature(ALTERNATES)]) : natural;
        const grade = Justification.grade(kashida.length);
        const count = Option.match(Justification.width(face), {
            onNone: () => 0,
            onSome: (each) => Justification.fit({ slack: allowed - (alternated.shaped.advance - natural.shaped.advance), width: each, grade, fractional: kashida.fractional }).count,
        });
        const ranked = yield* kashida.insertion === 'stretched' ? Place.within(alternated.run.text, Function.constTrue) : Justification.places(alternated.run.text, alternated.shaped);
        const inserted = (): Layout => {
            const rows = Array.sort(
                ranked
                    .map(({ cluster }, at) => {
                        const length = grade * (kashida.fractional ? count / ranked.length : Math.floor(count / ranked.length) + (at < count % ranked.length ? 1 : 0));
                        return { cluster, length, tatweels: Math.ceil(length) };
                    })
                    .filter(({ tatweels }) => tatweels > 0),
                Order.mapInput(Order.Number, ({ cluster }: { readonly cluster: number }) => cluster),
            );
            const shifted = (offset: number) => offset + Number.sumAll(rows.filter(({ cluster }) => cluster < offset).map(Struct.get('tatweels')));
            const next = {
                ...alternated.run,
                text: rows.reduceRight((text, { cluster, tatweels }) => `${text.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${text.slice(cluster)}`, alternated.run.text),
                features: alternated.run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start), Math.min(shifted(end), Feature.GLOBAL_END))),
            };
            const ratios = new Map(rows.flatMap(({ cluster, length, tatweels }) => Array.makeBy(tatweels, (index) => [shifted(cluster) + index, length / tatweels] as const)));
            const { glyphs, missing } = Shaped.of(face, next);
            return { run: next, shaped: Shaped.make({ glyphs: glyphs.map((each) => ({ ...each, xAdvance: each.xAdvance * (ratios.get(each.cluster) ?? 1) })), missing }) };
        };
        const elongated = Match.value(kashida.insertion).pipe(
            Match.when('none', () => alternated),
            Match.when('stretched', () => (grade === 0 ? [] : ranked).reduce<Layout>((layout, { cluster }) => fitted(layout, [...layout.run.features, new Feature(ALTERNATES, 1, cluster, cluster + 1)]), alternated)),
            Match.when('tatweel', inserted),
            Match.exhaustive,
        );
        const letter = spread('letter', elongated, word.levels);
        const glyph = spread('glyph', elongated, letter.levels);
        const last = Option.getOrElse(
            Array.findLastIndex(elongated.shaped.glyphs, (each) => lettered(elongated, each)),
            () => -1,
        );
        const [, placed] = Array.mapAccum(elongated.shaped.glyphs, { cluster: 0, delta: 0 }, (carried, each, at) => {
            const between = at === last ? 0 : glyph.levels.letter * space;
            const delta = (spaced(elongated, each.cluster) ? each.xAdvance * (glyph.levels.word - NEUTRAL.word.desired) : 0) + (lettered(elongated, each) ? each.xAdvance * (glyph.levels.glyph - NEUTRAL.glyph.desired) + between : 0);
            return each.class === 'MARK'
                ? ([carried, { ...each, xOffset: each.xOffset - (each.cluster >= carried.cluster ? carried.delta : 0) }] as const)
                : ([
                      { cluster: each.cluster, delta },
                      { ...each, xAdvance: each.xAdvance + delta },
                  ] as const);
        });
        const shaped = Shaped.make({ glyphs: placed, missing: elongated.shaped.missing });
        return new Justification({
            run: elongated.run,
            shaped,
            slack: width - shaped.advance,
            mechanisms: [
                { kind: 'word', amount: word.amount },
                { kind: 'kashida', amount: elongated.shaped.advance - natural.shaped.advance },
                { kind: 'letter', amount: letter.amount },
                { kind: 'glyph', amount: glyph.amount },
            ],
        });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Justification };
