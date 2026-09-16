// --- [IMPORTS] -------------------------------------------------------------------------

import { Number, Schema } from 'effect';
import { DPI, type PixelBudget } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Raster {
    readonly widthPx: number;
    readonly heightPx: number;
    readonly dpi: number;
}

interface Extent {
    readonly widthIn: number;
    readonly heightIn: number;
}

// --- [TABLE] ---------------------------------------------------------------------------

const _BUDGET = { detail: { longEdgePx: 1568, areaPx: 1_150_000 }, preview: { longEdgePx: 768, areaPx: 300_000 } } as const satisfies Record<
    PixelBudget,
    { readonly longEdgePx: number; readonly areaPx: number }
>;

// --- [MODELS] --------------------------------------------------------------------------

const Raster: Schema.Codec<Raster> = Schema.Struct({ widthPx: Schema.Int, heightPx: Schema.Int, dpi: Schema.Int });

// --- [BUDGET] --------------------------------------------------------------------------

const budget = (extent: Extent, kind: PixelBudget): Raster => {
    const fit = Math.round(Math.min(_BUDGET[kind].longEdgePx / Math.max(extent.widthIn, extent.heightIn), Math.sqrt(_BUDGET[kind].areaPx / (extent.widthIn * extent.heightIn))));
    const dpi = kind === 'detail' ? Number.clamp(fit, DPI) : fit;
    return { widthPx: Math.round(extent.widthIn * dpi), heightPx: Math.round(extent.heightIn * dpi), dpi };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Extent };
export { budget, Raster };
