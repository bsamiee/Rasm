import { Schema } from 'effect';
import { defaulted } from './bridge.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HIGH_RESOLUTION = { edge: 2576, tokens: 4784 };

// --- [MODELS] --------------------------------------------------------------------------

const Tokens = defaulted(Schema.Int.annotate({ description: 'Most visual tokens the image costs' }).check(Schema.isBetween({ minimum: 1, maximum: HIGH_RESOLUTION.tokens })), HIGH_RESOLUTION.tokens);

// --- [OPERATIONS] ----------------------------------------------------------------------

const fitted = (tokens: number, width: number, height: number): { readonly width: number; readonly height: number } => {
    const patch = 28;
    const patches = (pixels: number): number => Math.ceil(pixels / patch);
    const across = (pixels: number): number => Math.max(Math.round((pixels * Math.min(width, height)) / Math.max(width, height)), 1);
    const largest = (fits: number, fails: number): number => {
        const middle = Math.floor((fits + fails) / 2);
        const [low, high] = patches(middle) * patches(across(middle)) <= tokens ? [middle, fails] : [fits, middle];
        return high - low > 1 ? largest(low, high) : low;
    };
    const long = largest(1, Math.floor(HIGH_RESOLUTION.edge / patch) * patch + 1);
    return width >= height ? { width: long, height: across(long) } : { width: across(long), height: long };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { fitted, HIGH_RESOLUTION, Tokens };
