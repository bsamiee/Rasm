import { Array, Data, Option, Order, Record, Struct } from 'effect';
import type { Face } from '#/face';
import type { Shaped } from '#/shaped';

// --- [TYPES] ---------------------------------------------------------------------------

interface Point {
    readonly x: number;
    readonly y: number;
}
interface Wall {
    readonly x: number;
    readonly top: number;
    readonly foot: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WALL_INSET = 1;

// --- [MODELS] --------------------------------------------------------------------------

class Profile extends Data.Class<{
    readonly unitsPerEm: number;
    readonly descender: number;
    readonly left: readonly Option.Option<number>[];
    readonly right: readonly Option.Option<number>[];
    readonly advance: number;
}> {
    static readonly of = (face: Face, { glyphs }: Shaped) => {
        const { unitsPerEm, metrics } = face;
        const descender = Math.round(metrics.extents.hhea.descender * unitsPerEm);
        const [pen, placed] = Array.mapAccum(glyphs, { x: 0, y: 0 }, ({ x, y }, { glyph, xAdvance, yAdvance, xOffset, yOffset }) => [{ x: x + xAdvance, y: y + yAdvance }, segments(face.outline(glyph), { x: (x + xOffset) * unitsPerEm, y: (y + yOffset) * unitsPerEm })]);
        const rows = Array.groupBy(Array.flatMap(placed.flat(), crossings), ({ band }) => `${band}`);
        const bands = Array.makeBy(Math.round(metrics.extents.hhea.ascender * unitsPerEm) - descender, (band) =>
            Option.map(Record.get(rows, `${band + descender}`), (crossed) => ({ left: Array.min(Array.map(crossed, Struct.get('low')), Order.Number), right: Array.max(Array.map(crossed, Struct.get('high')), Order.Number) })),
        );
        return new Profile({ unitsPerEm, descender, left: bands.map(Option.map(Struct.get('left'))), right: bands.map(Option.map(Struct.get('right'))), advance: pen.x * unitsPerEm });
    };
    static readonly wall = (profile: Profile, { x, top, foot }: Wall) => {
        const height = (band: number) => profile.descender + band;
        const lean = (band: number) => (x * (height(band) - foot)) / (top - foot);
        const spanned = (band: number) => foot <= height(band) && height(band) < top;
        const contact = least(
            Array.getSomes(
                profile.left.map((left, band) =>
                    Option.filter(
                        Option.map(left, (bound) => bound - lean(band)),
                        () => spanned(band),
                    ),
                ),
            ),
        );
        return Option.match(contact, {
            onNone: () => profile,
            onSome: (slid) =>
                new Profile({
                    ...profile,
                    left: profile.left.map((left, band) =>
                        spanned(band)
                            ? Option.some(
                                  Math.min(
                                      Option.getOrElse(left, () => Number.POSITIVE_INFINITY),
                                      slid - WALL_INSET + lean(band),
                                  ),
                              )
                            : left,
                    ),
                }),
        });
    };
    static readonly gap = (before: Profile, after: Profile) => least(Array.getSomes(Array.zipWith(before.right, after.left, (right, left) => Option.zipWith(right, left, (bound, start) => before.advance + start - bound))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const least = Array.match({ onEmpty: () => Option.none<number>(), onNonEmpty: (present: Array.NonEmptyReadonlyArray<number>) => Option.some(Array.min(present, Order.Number)) });
const midpoints = (points: readonly Point[]) => Array.zipWith(points, Array.drop(points, 1), (a, b) => ({ x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 }));
const deviation = (curve: readonly Point[]) => {
    const wangDivisor = 8;
    const degree = curve.length - 1;
    const bends = Array.zipWith(Array.zip(curve, Array.drop(curve, 1)), Array.drop(curve, 2), ([a, b], c) => Math.hypot(a.x - 2 * b.x + c.x, a.y - 2 * b.y + c.y));
    return (bends.reduce((largest, bend) => Math.max(largest, bend), 0) * degree * (degree - 1)) / wangDivisor;
};
const flattened = (curve: readonly Point[]): readonly Point[] => {
    const tolerance = 0.5;
    const levels = Array.unfold(curve, (level) => Option.liftPredicate([level, midpoints(level)] as const, () => Array.isReadonlyArrayNonEmpty(level)));
    return deviation(curve) <= tolerance ? Array.takeRight(curve, 1) : [...flattened(Array.getSomes(levels.map(Array.head))), ...flattened(Array.reverse(Array.getSomes(levels.map(Array.last))))];
};
const segments = (commands: ReturnType<Face['outline']>, origin: Point) =>
    Array.mapAccum(commands, { cursor: origin, start: origin }, ({ cursor, start }, { type, values }) => {
        const points = Array.chunksOf(values, 2).map(([x = 0, y = 0]) => ({ x: origin.x + x, y: origin.y + y }));
        const end = Option.getOrElse(Array.last(points), () => start);
        const line = [cursor, ...flattened([cursor, ...Array.dropRight(points, 1), end])];
        return [{ cursor: end, start: type === 'M' ? end : start }, type === 'M' ? [] : Array.zip(line, Array.drop(line, 1))];
    })[1].flat();
const crossings = ([from, to]: readonly [Point, Point]) => {
    const [low, high] = [Math.min(from.y, to.y), Math.max(from.y, to.y)];
    const at = (y: number) => from.x + ((to.x - from.x) * (y - from.y)) / (to.y - from.y);
    return low === high
        ? [{ band: Math.floor(low), low: Math.min(from.x, to.x), high: Math.max(from.x, to.x) }]
        : Array.range(Math.floor(low), Math.max(Math.floor(low), Math.ceil(high) - 1)).map((band) => {
              const [entry, exit] = [at(Math.max(low, band)), at(Math.min(high, band + 1))];
              return { band, low: Math.min(entry, exit), high: Math.max(entry, exit) };
          });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
