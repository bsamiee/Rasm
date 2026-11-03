import { Array, Data, Option, Order, pipe, Struct } from 'effect';
import type { SvgPathCommand } from 'harfbuzzjs';
import type { ShapedText } from '#/shaped-text';

// --- [TYPES] ---------------------------------------------------------------------------

type Edge = ReadonlyMap<number, number>;
type Segment = readonly [Point, Point];
interface Point {
    readonly x: number;
    readonly y: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ORIGIN: Point = { x: 0, y: 0 };

// --- [MODELS] --------------------------------------------------------------------------

class Profile extends Data.Class<{
    readonly left: Edge;
    readonly right: Edge;
    readonly advance: number;
}> {
    static readonly outline = (commands: readonly SvgPathCommand[]): readonly Segment[] =>
        Array.mapAccum(commands, { cursor: ORIGIN, start: ORIGIN }, ({ cursor, start }, { type, values }) => {
            const points = Array.chunksOf(values, 2).map(([x = 0, y = 0]) => ({ x, y }));
            const end = Option.getOrElse(Array.last(points), () => start);
            return [{ cursor: end, start: type === 'M' ? end : start }, type === 'M' ? [] : Array.window([cursor, ...flattened([cursor, ...Array.dropRight(points, 1), end])], 2)];
        })[1].flat();
    static readonly of = (shaped: ShapedText, unitsPerEm: number, outline: (glyph: number) => readonly Segment[]) => {
        const [, placed] = Array.mapAccum(shaped.glyphs, { x: 0, y: 0 }, ({ x, y }, { glyph, xAdvance, yAdvance, xOffset, yOffset }) => {
            const origin = { x: (x + xOffset) * unitsPerEm, y: (y + yOffset) * unitsPerEm };
            return [{ x: x + xAdvance, y: y + yAdvance }, outline(glyph).map(([from, to]) => [moved(from, origin), moved(to, origin)] as const)];
        });
        const bands = [...Map.groupBy(placed.flat().flatMap(crossings), Struct.get('band'))];
        return new Profile({
            left: new Map(bands.map(([band, rows]) => [band, Math.min(...rows.map(Struct.get('left')))])),
            right: new Map(bands.map(([band, rows]) => [band, Math.max(...rows.map(Struct.get('right')))])),
            advance: shaped.advance * unitsPerEm,
        });
    };

    bounded({ slant, top, foot }: { readonly slant: number; readonly top: number; readonly foot: number }) {
        const line = Array.range(Math.floor(foot), Math.ceil(top))
            .filter((height) => foot <= height && height < top)
            .map((height) => [height, (slant * (height - foot)) / (top - foot)] as const);
        return Option.match(minClearance(line, this.left), {
            onNone: () => this,
            onSome: (offset) => new Profile({ ...this, left: new Map([...this.left, ...line.map(([height, shift]) => [height, offset + shift] as const)]) }),
        });
    }
    gap(after: Profile) {
        return Option.map(minClearance(this.right, after.left), (clearance) => this.advance + clearance);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const moved = ({ x, y }: Point, by: Point): Point => ({ x: x + by.x, y: y + by.y });
const minClearance = (right: Iterable<readonly [number, number]>, left: Edge) => pipe(Array.getSomes(Array.map([...right], ([height, x]) => Option.map(Option.fromUndefinedOr(left.get(height)), (edge) => edge - x))), Option.liftPredicate(Array.isArrayNonEmpty), Option.map(Array.min(Order.Number)));
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
const crossings = ([from, to]: Segment) => {
    const [low, high] = [Math.min(from.y, to.y), Math.max(from.y, to.y)];
    const at = (y: number) => from.x + ((to.x - from.x) * (y - from.y)) / (to.y - from.y);
    const crossing = (band: number, entry: number, exit: number) => ({ band, left: Math.min(entry, exit), right: Math.max(entry, exit) });
    return low === high ? [crossing(Math.floor(low), from.x, to.x)] : Array.range(Math.floor(low), Math.ceil(high) - 1).map((band) => crossing(band, at(Math.max(low, band)), at(Math.min(high, band + 1))));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
