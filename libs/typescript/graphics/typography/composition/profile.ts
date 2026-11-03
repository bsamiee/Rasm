import { Array, Data, Option, Order, Struct } from 'effect';
import type { SvgPathCommand } from 'harfbuzzjs';
import type { Face } from '#/face';
import type { Shaped } from '#/shaped';

// --- [TYPES] ---------------------------------------------------------------------------

type Edge = ReadonlyMap<number, number>;
interface Point {
    readonly x: number;
    readonly y: number;
}

// --- [MODELS] --------------------------------------------------------------------------

class Profile extends Data.Class<{
    readonly left: Edge;
    readonly right: Edge;
    readonly advance: number;
}> {
    static readonly of = (face: Face, shaped: Shaped) => {
        const { unitsPerEm } = face;
        const [, placed] = Array.mapAccum(shaped.glyphs, { x: 0, y: 0 }, ({ x, y }, { glyph, xAdvance, yAdvance, xOffset, yOffset }) => [{ x: x + xAdvance, y: y + yAdvance }, segments(face.font.glyphToJson(glyph), { x: (x + xOffset) * unitsPerEm, y: (y + yOffset) * unitsPerEm })]);
        const bands = [...Map.groupBy(placed.flat().flatMap(crossings), Struct.get('band'))];
        return new Profile({
            left: new Map(bands.map(([band, rows]) => [band, Math.min(...rows.map(Struct.get('left')))])),
            right: new Map(bands.map(([band, rows]) => [band, Math.max(...rows.map(Struct.get('right')))])),
            advance: shaped.advance * unitsPerEm,
        });
    };
    static readonly wall = (profile: Profile, { x, top, foot }: { readonly x: number; readonly top: number; readonly foot: number }) => {
        const line = Array.range(Math.floor(foot), Math.ceil(top))
            .filter((height) => foot <= height && height < top)
            .map((height) => [height, (x * (height - foot)) / (top - foot)] as const);
        return Option.match(contact(line, profile.left), {
            onNone: () => profile,
            onSome: (slid) => new Profile({ ...profile, left: new Map([...profile.left, ...line.map(([height, lean]) => [height, slid + lean] as const)]) }),
        });
    };
    static readonly gap = (before: Profile, after: Profile) => Option.map(contact(before.right, after.left), (clear) => before.advance + clear);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const contact = (right: Iterable<readonly [number, number]>, left: Edge) =>
    Array.head(
        Array.sort(
            [...right].flatMap(([height, x]) => Option.toArray(Option.map(Option.fromUndefinedOr(left.get(height)), (edge) => edge - x))),
            Order.Number,
        ),
    );
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
    const crossing = (band: number, entry: number, exit: number) => ({ band, left: Math.min(entry, exit), right: Math.max(entry, exit) });
    return low === high ? [crossing(Math.floor(low), from.x, to.x)] : Array.range(Math.floor(low), Math.ceil(high) - 1).map((band) => crossing(band, at(Math.max(low, band)), at(Math.min(high, band + 1))));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
