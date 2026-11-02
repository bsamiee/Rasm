import { Array, Data, Option, Order, Record, Struct } from 'effect';
import type { SvgPathCommand } from 'harfbuzzjs';
import type { Face } from '#/face';
import type { Shaped } from '#/shaped';

// --- [TYPES] ---------------------------------------------------------------------------

interface Point {
    readonly x: number;
    readonly y: number;
}
interface Bound {
    readonly left: number;
    readonly right: number;
}
interface Wall {
    readonly x: number;
    readonly top: number;
    readonly foot: number;
}

// --- [MODELS] --------------------------------------------------------------------------

class Profile extends Data.Class<{
    readonly unitsPerEm: number;
    readonly bands: Record.ReadonlyRecord<string, Bound>;
    readonly advance: number;
}> {
    static readonly of = (face: Face, { glyphs }: Shaped) => {
        const { unitsPerEm } = face;
        const [pen, placed] = Array.mapAccum(glyphs, { x: 0, y: 0 }, ({ x, y }, { glyph, xAdvance, yAdvance, xOffset, yOffset }) => [{ x: x + xAdvance, y: y + yAdvance }, segments(face.font.glyphToJson(glyph), { x: (x + xOffset) * unitsPerEm, y: (y + yOffset) * unitsPerEm })]);
        const crossed = Array.groupBy(placed.flat().flatMap(crossings), ({ band }) => `${band}`);
        return new Profile({
            unitsPerEm,
            bands: Record.map(crossed, (rows) => ({ left: Array.min(Array.map(rows, Struct.get('left')), Order.Number), right: Array.max(Array.map(rows, Struct.get('right')), Order.Number) })),
            advance: pen.x * unitsPerEm,
        });
    };
    static readonly wall = (profile: Profile, { x, top, foot }: Wall) => {
        const heights = Array.range(Math.floor(foot), Math.ceil(top)).filter((height) => foot <= height && height < top);
        const lean = (height: number) => (x * (height - foot)) / (top - foot);
        const contact = least(heights.flatMap((height) => Option.toArray(Option.map(Record.get(profile.bands, `${height}`), ({ left }) => left - lean(height)))));
        return Option.match(contact, {
            onNone: () => profile,
            onSome: (slid) =>
                new Profile({
                    ...profile,
                    bands: Record.union(
                        profile.bands,
                        Record.fromIterableWith(heights, (height) => [`${height}`, { left: slid + lean(height), right: slid + lean(height) }]),
                        ({ right }, { left }) => ({ left, right }),
                    ),
                }),
        });
    };
    static readonly gap = (before: Profile, after: Profile) => least(Object.values(Record.intersection(before.bands, after.bands, ({ right }, { left }) => before.advance + left - right)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const least = Array.match({ onEmpty: () => Option.none<number>(), onNonEmpty: (present: Array.NonEmptyReadonlyArray<number>) => Option.some(Array.min(present, Order.Number)) });
const deviation = (curve: readonly Point[]) => {
    const wangDivisor = 8;
    const bendPoints = 3;
    const degree = curve.length - 1;
    return (Math.max(0, ...Array.window(curve, bendPoints).map(([a, b, c]) => Math.hypot(a.x - 2 * b.x + c.x, a.y - 2 * b.y + c.y))) * degree * (degree - 1)) / wangDivisor;
};
const halves = (curve: readonly Point[]) => {
    const levels = Array.unfold(curve, (level) => Option.liftPredicate([level, Array.window(level, 2).map(([a, b]) => ({ x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 }))] as const, () => Array.isReadonlyArrayNonEmpty(level)));
    return [Array.getSomes(levels.map(Array.head)), Array.reverse(Array.getSomes(levels.map(Array.last)))];
};
const flattened = (curve: readonly Point[]): readonly Point[] => {
    const tolerance = 0.5;
    return deviation(curve) <= tolerance ? Array.takeRight(curve, 1) : halves(curve).flatMap((half) => flattened(half));
};
const segments = (commands: readonly SvgPathCommand[], origin: Point) =>
    Array.mapAccum(commands, { cursor: origin, start: origin }, ({ cursor, start }, { type, values }) => {
        const points = Array.chunksOf(values, 2).map(([x = 0, y = 0]) => ({ x: origin.x + x, y: origin.y + y }));
        const end = Option.getOrElse(Array.last(points), () => start);
        return [{ cursor: end, start: type === 'M' ? end : start }, type === 'M' ? [] : Array.window([cursor, ...flattened([cursor, ...Array.dropRight(points, 1), end])], 2)];
    })[1].flat();
const crossings = ([from, to]: readonly [Point, Point]) => {
    const [low, high] = [Math.min(from.y, to.y), Math.max(from.y, to.y)];
    const at = (y: number) => from.x + ((to.x - from.x) * (y - from.y)) / (to.y - from.y);
    return low === high
        ? [{ band: Math.floor(low), left: Math.min(from.x, to.x), right: Math.max(from.x, to.x) }]
        : Array.range(Math.floor(low), Math.max(Math.floor(low), Math.ceil(high) - 1)).map((band) => {
              const [entry, exit] = [at(Math.max(low, band)), at(Math.min(high, band + 1))];
              return { band, left: Math.min(entry, exit), right: Math.max(entry, exit) };
          });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
