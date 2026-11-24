import { Array, Data, Option, Order, Struct, UndefinedOr } from 'effect';
import type { SvgPathCommand } from 'harfbuzzjs';
import type { ShapedText } from '#/shaping/shaped-text';

// --- [TYPES] ---------------------------------------------------------------------------

interface Point {
    readonly x: number;
    readonly y: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ORIGIN: Point = { x: 0, y: 0 };
const BY_FIT = Order.Struct({ gap: Order.Number, shift: Order.flip(Order.mapInput(Order.Number, Math.abs)) });

// --- [MODELS] --------------------------------------------------------------------------

class Profile extends Data.Class<{
    readonly left: ReadonlyMap<number, number>;
    readonly right: ReadonlyMap<number, number>;
    readonly advance: number;
}> {
    static readonly outline = (commands: readonly SvgPathCommand[]) => {
        const [, drawn] = Array.mapAccum(commands, { cursor: ORIGIN, start: ORIGIN, contour: 0 }, ({ cursor, start, contour }, { type, values }) => {
            const points = Array.chunksOf(values, 2).map(([x, y = 0]) => ({ x, y }));
            const end = Option.getOrElse(Array.last(points), () => start);
            const moving = type === 'M';
            const next = moving ? contour + 1 : contour;
            return [
                { cursor: end, start: moving ? end : start, contour: next },
                { contour: next, segments: moving ? [] : Array.window([cursor, ...flattened([cursor, ...Array.dropRight(points, 1), end])], 2) },
            ];
        });
        return [...Map.groupBy(drawn, Struct.get('contour')).values()].map((strokes) => strokes.flatMap(Struct.get('segments')));
    };
    static readonly of = (shaped: ShapedText, unitsPerEm: number, outline: (glyph: number) => ReturnType<typeof Profile.outline>) => {
        const [, placed] = Array.mapAccum(shaped.glyphs, ORIGIN, ({ x, y }, { glyph, xAdvance, yAdvance, xOffset, yOffset, scale }) => {
            const by = { x: (x + xOffset) * unitsPerEm, y: (y + yOffset) * unitsPerEm };
            return [{ x: x + xAdvance, y: y + yAdvance }, outline(glyph).flatMap((contour) => contour.flatMap((segment) => crossings(moved(segment, by, scale))))];
        });
        const bands = [...Map.groupBy(placed.flat(), Struct.get('band'))];
        return new Profile({ left: new Map(bands.map(([band, rows]) => [band, Math.min(...rows.map(Struct.get('left')))])), right: new Map(bands.map(([band, rows]) => [band, Math.max(...rows.map(Struct.get('right')))])), advance: shaped.advance * unitsPerEm });
    };
    static readonly gap = (before: Profile, after: Profile, rise = 0, limit = Number.POSITIVE_INFINITY) => {
        const fits = Array.getSomes(Array.range(-rise, rise).map((shift) => Option.map(minClearance(before.right, after.left, shift), (clearance) => ({ gap: Math.min(before.advance + clearance, limit), shift }))));
        return Option.map(Option.liftPredicate(fits, Array.isArrayNonEmpty), Array.max(BY_FIT));
    };

    bounded({ slant, top, foot }: { readonly slant: number; readonly top: number; readonly foot: number }) {
        const line = Array.range(Math.floor(foot), Math.ceil(top))
            .filter((height) => foot <= height && height < top)
            .map((height) => [height, (slant * (height - foot)) / (top - foot)] as const);
        return Option.match(minClearance(line, this.left, 0), {
            onNone: () => this,
            onSome: (offset) => new Profile({ ...this, left: new Map([...this.left, ...line.map(([height, shift]) => [height, offset + shift] as const)]) }),
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const moved = ([from, to]: readonly [Point, Point], by: Point, scale: number) =>
    [
        { x: from.x * scale + by.x, y: from.y + by.y },
        { x: to.x * scale + by.x, y: to.y + by.y },
    ] as const;
const minClearance = (right: Iterable<readonly [number, number]>, left: Profile['left'], shift: number) => {
    const clearances = Array.flatMapNullishOr(Array.fromIterable(right), ([height, x]) => UndefinedOr.map(left.get(height + shift), (edge) => edge - x));
    return Option.map(Option.liftPredicate(clearances, Array.isArrayNonEmpty), Array.min(Order.Number));
};
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
const crossing = (band: number, entry: number, exit: number) => ({ band, left: Math.min(entry, exit), right: Math.max(entry, exit) });
const crossings = ([from, to]: readonly [Point, Point]) => {
    const [low, high] = [Math.min(from.y, to.y), Math.max(from.y, to.y)];
    const at = (y: number) => from.x + ((to.x - from.x) * (y - from.y)) / (to.y - from.y);
    return low === high ? [crossing(Math.floor(low), from.x, to.x)] : Array.range(Math.floor(low), Math.ceil(high) - 1).map((band) => crossing(band, at(Math.max(low, band)), at(Math.min(high, band + 1))));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
