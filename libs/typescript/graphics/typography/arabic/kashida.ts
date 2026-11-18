import { Array, Function, flow, Match, Number, Option, Order, Predicate, pipe, Record, Struct } from 'effect';
import { Feature } from 'harfbuzzjs';
import type { JoiningGroup } from 'icu';
import { Elongation } from '#/arabic/elongation';
import { joiningGroups, joins } from '#/arabic/joining';
import type { Face } from '#/shaping/face';
import { Profile } from '#/shaping/internal/profile';
import type { Measured } from '#/shaping/justification';
import type { ShapedText } from '#/shaping/shaped-text';
import { Span, words } from '#/shaping/word';

// --- [TYPES] ---------------------------------------------------------------------------

type Place = Elongating['places'][number];
type Elongating = Exclude<typeof Elongation.Type, { readonly _tag: 'None' }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const TATWEEL = 0x6_40;
const BEH = 0x6_28;
const JUSTIFICATION_ALTERNATES = 'jalt';
const BY_RANK = Order.Struct({ rank: Order.Number, cluster: Order.flip(Order.Number) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FACE]

const tatweelAdvance = (face: Face) => Array.findFirst([...Option.getOrElse(Record.get(face.extenders, 'arab'), Array.empty<number>).map((glyph) => face.em(face.font.glyphHAdvance(glyph))), ...Option.toArray(face.advance(TATWEEL))], Number.isGreaterThan(0));
const nuqtaWidth = (face: Face) => {
    const widths = Option.map(face.glyph(BEH), (glyph) =>
        Profile.outline(face.font.glyphToJson(glyph))
            .map((contour) => contour.flat().map(Struct.get('x')))
            .map((xs) => Math.max(...xs) - Math.min(...xs))
            .filter(Number.isGreaterThan(0)),
    );
    return Option.map(Option.flatMap(widths, Option.liftPredicate(Array.isArrayNonEmpty)), flow(Array.min(Order.Number), face.em));
};
const drawsKashida = (face: Face) => Option.isSome(tatweelAdvance(face)) || face.layout.substitutions.has(JUSTIFICATION_ALTERNATES);

// --- [PLACES]

const candidates = (text: string, join: Span, rows: readonly Place[]) => {
    const within = (groups: Place['before']) => (group: JoiningGroup) => Option.match(groups, { onNone: Function.constTrue, onSome: ({ groups: listed, except }) => listed.includes(group.value) !== except });
    const follows = (next: Place['next'], letter: Option.Option<JoiningGroup>) => Option.match(next, { onNone: Function.constTrue, onSome: Match.valueTags({ End: () => Option.isNone(letter), Letter: ({ groups }) => Option.exists(letter, within(groups)) }) });
    const letters = joiningGroups(join.slice(text));
    return Array.getSomes(
        Array.window(letters, 2).map(([before, after], at) => {
            const next = Option.map(Array.get(letters, at + 2), Struct.get('group'));
            const row = Array.findFirst(rows, (each) => within(each.before)(before.group) && within(each.after)(after.group) && follows(each.next, next));
            return Option.map(row, ({ rank }) => ({ cluster: join.start + after.offset, rank }));
        }),
    );
};
const kashidaPlaces = (text: string, rows: readonly Place[], allowed: (cluster: number) => boolean = Function.constTrue) =>
    Array.sort(
        Array.getSomes(
            words(text).map((word) =>
                pipe(
                    joins(text, word)
                        .flatMap((join) => candidates(text, join, rows))
                        .filter(({ cluster }) => allowed(cluster)),
                    Array.match({ onEmpty: Option.none, onNonEmpty: flow(Array.min(BY_RANK), Option.some) }),
                ),
            ),
        ),
        BY_RANK,
    );

// --- [ELONGATION]

const elongationOf = (settings: { readonly kashida: typeof Elongation.Type }) => Option.liftPredicate(settings.kashida, Elongation.isAnyOf(['Tatweel', 'Stretched']));
const eligible = (measured: Measured, elongation: Elongating, stretched: boolean, stage: Place['stage']) => {
    const flagged = (glyph: ShapedText['glyphs'][number]) => glyph.flags.includes('SAFE_TO_INSERT_TATWEEL') || (stretched && measured.face.layout.covers(JUSTIFICATION_ALTERNATES, glyph.glyph));
    const clusters = new Set(measured.shaped.glyphs.filter(flagged).map(Struct.get('cluster')));
    return kashidaPlaces(
        measured.shaped.text,
        elongation.places.filter((row) => row.stage === stage),
        (cluster) => clusters.has(cluster),
    );
};
const solved = ({ lower, upper, limit }: Elongating, count: number, room: Option.Option<number>) => {
    const most = Math.min(
        count,
        Option.getOrElse(limit, () => count),
    );
    const fitted = Option.match(room, {
        onNone: () => ({ count: most, length: lower }),
        onSome: (available) => {
            const fitting = Math.min(most, Math.floor(available / lower), Math.ceil(available / upper));
            return { count: fitting, length: Number.clamp(available / fitting, { minimum: lower, maximum: upper }) };
        },
    });
    return Option.liftPredicate(fitted, (solution) => solution.count > 0);
};
const inserted = (measured: Measured, chosen: readonly number[], tatweels: number, scale: number): Measured => {
    const shifted = (offset: number, precedes: (cluster: number, offset: number) => boolean) => offset + tatweels * chosen.filter((cluster) => precedes(cluster, offset)).length;
    const { run } = measured;
    const text = chosen.reduceRight((inserting, cluster) => `${inserting.slice(0, cluster)}${String.fromCodePoint(TATWEEL).repeat(tatweels)}${inserting.slice(cluster)}`, measured.shaped.text);
    const scales = [...measured.scales.entries().map(([cluster, ratio]) => [shifted(cluster, Number.isLessThanOrEqualTo), ratio] as const), ...chosen.flatMap((cluster) => Array.makeBy(tatweels, (index) => [shifted(cluster, Number.isLessThan) + index, scale] as const))];
    const features = run.features.map(({ tag, value, start, end }) => new Feature(tag, value, shifted(start, Number.isLessThanOrEqualTo), Math.min(shifted(end, Number.isLessThan), Feature.GLOBAL_END)));
    const elongated = measured.reshaped({ ...run, text, item: Option.map(run.item, ({ start, end }) => Span.make({ start: shifted(start, Number.isLessThanOrEqualTo), end: shifted(end, Number.isLessThan) })), features }, new Map(scales));
    return elongated.gained('kashida', elongated.shaped.advance - measured.shaped.advance);
};
const tatweeled = (measured: Measured, elongation: Elongating, clusters: readonly number[], room: Option.Option<number>) =>
    Option.getOrElse(
        Option.gen(function* () {
            const [dot, extender] = yield* Option.all([nuqtaWidth(measured.face), tatweelAdvance(measured.face)]);
            const { count, length } = yield* solved(
                elongation,
                clusters.length,
                Option.map(room, (em) => Math.max(0, em) / dot),
            );
            const span = length * dot;
            const tatweels = yield* Option.liftPredicate(elongation.fractional ? Math.ceil(span / extender) : Math.floor(span / extender), Number.isGreaterThan(0));
            return inserted(measured, Array.sort(Array.take(clusters, count), Order.Number), tatweels, elongation.fractional ? span / (tatweels * extender) : 1);
        }),
        () => measured,
    );
const probed = (current: Measured, cluster: number, left: number) => {
    const values = Option.match(
        Array.findFirst(current.shaped.glyphs, (each) => each.cluster === cluster),
        { onNone: Array.empty<number>, onSome: ({ glyph }) => current.face.layout.variants(JUSTIFICATION_ALTERNATES, glyph) },
    );
    const probes = Array.getSomes(values.map((_, index) => Option.map(current.featured(new Feature(JUSTIFICATION_ALTERNATES, index + 1, cluster, cluster + 1), left), ({ next, gain }) => next.gained('kashida', gain))));
    return Option.getOrElse(Option.map(Option.liftPredicate(probes, Array.isArrayNonEmpty), Array.max(Order.mapInput(Order.Number, ({ kashida }: Measured) => kashida))), () => current);
};
const reached = (measured: Measured, elongation: Elongating, stretched: boolean, rank: number) =>
    eligible(measured, elongation, stretched, 'line')
        .filter((place) => place.rank === rank)
        .map(Struct.get('cluster'));
const elongated = (current: Measured, elongation: Elongating, rank: number, amount: number) => {
    const stretched = Predicate.isTagged(elongation, 'Stretched');
    const jalted = stretched ? reached(current, elongation, true, rank).reduce((probing, cluster) => probed(probing, cluster, amount - (probing.kashida - current.kashida)), current) : current;
    return tatweeled(jalted, elongation, reached(jalted, elongation, false, rank), Option.some(amount - (jalted.kashida - current.kashida)));
};

// --- [JUSTIFICATION]

const kashidaFlagged = (run: typeof ShapedText.Run.Type, settings: { readonly kashida: typeof Elongation.Type }): typeof ShapedText.Run.Type => Option.match(elongationOf(settings), { onNone: () => run, onSome: () => ({ ...run, flags: Array.union(run.flags, ['PRODUCE_SAFE_TO_INSERT_TATWEEL' as const]) }) });
const kashidaPrepared = (measured: Measured) =>
    Option.match(elongationOf(measured.settings), {
        onNone: () => measured,
        onSome: (elongation) => tatweeled(measured, elongation, eligible(measured, elongation, false, 'text').map(Struct.get('cluster')), Option.none()),
    });
const kashidaRanks = (settings: { readonly kashida: typeof Elongation.Type }) => Option.match(elongationOf(settings), { onNone: Array.empty<number>, onSome: ({ places }) => places.map(Struct.get('rank')) });
const kashidaMechanisms = (measured: Measured, rank: number) =>
    Option.match(Option.all([elongationOf(measured.settings), nuqtaWidth(measured.face)]), {
        onNone: () => [],
        onSome: ([elongation, nuqta]) => [
            {
                capacity: Option.match(solved(elongation, reached(measured, elongation, Predicate.isTagged(elongation, 'Stretched'), rank).length, Option.some(globalThis.Number.POSITIVE_INFINITY)), {
                    onNone: () => 0,
                    onSome: ({ count, length }) => count * length * nuqta,
                }),
                take: (current: Measured, amount: number) => elongated(current, elongation, rank, amount),
            },
        ],
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { drawsKashida, kashidaFlagged, kashidaMechanisms, kashidaPlaces, kashidaPrepared, kashidaRanks, nuqtaWidth, TATWEEL, tatweelAdvance };
