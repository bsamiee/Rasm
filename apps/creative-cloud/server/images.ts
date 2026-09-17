// --- [IMPORTS] -------------------------------------------------------------------------

import { Number, Schema, Struct } from 'effect';

// --- [TABLE] ---------------------------------------------------------------------------

const BUDGET = { detail: { longEdgePx: 1568, pixels: 1_150_000 }, preview: { longEdgePx: 768, pixels: 300_000 } } as const;
const DPI = { minimum: 48, maximum: 600 } as const;

// --- [TYPES] ---------------------------------------------------------------------------

type PixelBudget = (typeof PixelBudget)['Type'];
type Region = (typeof Region)['Type'];
type Bounds = (typeof Bounds)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _POINTS_PER_INCH = 72;

// --- [MODELS] --------------------------------------------------------------------------

const PixelBudget: Schema.Literals<Array<keyof typeof BUDGET>> = Schema.Literals(Struct.keys(BUDGET));
const Dpi: Schema.Int = Schema.Int.pipe(Schema.check(Schema.isBetween(DPI)));
const _unit = Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 1 })));
const Region: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number, Schema.Number]> = Schema.Tuple([_unit, _unit, _unit, _unit]).pipe(
    Schema.check(Schema.makeFilter(([x0, y0, x1, y1]: readonly [number, number, number, number]) => x1 > x0 && y1 > y0, { title: 'region', description: 'x1 > x0 and y1 > y0' })),
);
const Bounds: Schema.Struct<{ readonly left: Schema.Number; readonly top: Schema.Number; readonly right: Schema.Number; readonly bottom: Schema.Number }> = Schema.Struct({
    left: Schema.Number,
    top: Schema.Number,
    right: Schema.Number,
    bottom: Schema.Number,
});

// --- [RESOLUTION] ----------------------------------------------------------------------

const dpi = (budget: PixelBudget, widthPt: number, heightPt: number): number => {
    const widthIn = widthPt / _POINTS_PER_INCH;
    const heightIn = heightPt / _POINTS_PER_INCH;
    const fitted = Math.round(Math.min(BUDGET[budget].longEdgePx / Math.max(widthIn, heightIn), Math.sqrt(BUDGET[budget].pixels / (widthIn * heightIn))));
    return budget === 'detail' ? Number.clamp(fitted, DPI) : fitted;
};

const pixels = (pt: number, resolution: number): number => Math.round((pt * resolution) / _POINTS_PER_INCH);

const points = (px: number, resolution: number): number => (px * _POINTS_PER_INCH) / resolution;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Bounds, BUDGET, DPI, Dpi, dpi, PixelBudget, pixels, points, Region };
