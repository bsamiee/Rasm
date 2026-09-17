/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum ColorConvertPurpose {}
    enum DocumentColorSpace {}
    enum ImageColorSpace {}

    type Triple = [number, number, number];
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, flatten, fold, items, pairs, range, run, select, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

type Space = 'oklab' | 'oklch' | 'lch' | 'hsl';
type HueArc = 'shorter' | 'longer' | 'decreasing' | 'increasing';

interface Blend {
    readonly precision: number;
    readonly space: Space;
    readonly hueArc: HueArc;
    readonly removeIntermediateStops: boolean;
    readonly attributes: ('fill' | 'stroke')[];
}

interface Stop {
    readonly position: number;
    readonly opacity: number;
    readonly color: Color;
}

interface Found {
    readonly gradient: Gradient;
    readonly objects: string[];
}

const PERCENT = 100;
const CHANNEL = 255;
const DEGREES = 360;
const HALF_TURN = 180;
const HALF = 0.5;
const LAST_STOP = 100;

// --- [COLOR_SPACES] --------------------------------------------------------------------

const SRGB = { threshold: 0.040_45, offset: 0.055, slope: 12.92, gamma: 2.4, linearThreshold: 0.003_130_8 };
const CUBE = 3;
const HUE_SECTORS = 6;
const HSL_SECTORS = 12;
const HSL_RED = 0;
const HSL_GREEN = 8;
const HSL_BLUE = 4;
const RADIANS = Math.PI / HALF_TURN;
const LMS = {
    long: { red: 0.412_221_470_8, green: 0.536_332_536_3, blue: 0.051_445_992_9 },
    medium: { red: 0.211_903_498_2, green: 0.680_699_545_1, blue: 0.107_396_956_6 },
    short: { red: 0.088_302_461_9, green: 0.281_718_837_6, blue: 0.629_978_700_5 },
};
const OKLAB = {
    lightness: { long: 0.210_454_255_3, medium: 0.793_617_785, short: -0.004_072_046_8 },
    a: { long: 1.977_998_495_1, medium: -2.428_592_205, short: 0.450_593_709_9 },
    b: { long: 0.025_904_037_1, medium: 0.782_771_766_2, short: -0.808_675_766 },
};
const OKLAB_INVERSE = {
    long: { lightness: 1, a: 0.396_337_777_4, b: 0.215_803_757_3 },
    medium: { lightness: 1, a: -0.105_561_345_8, b: -0.063_854_172_8 },
    short: { lightness: 1, a: -0.089_484_177_5, b: -1.291_485_548 },
};
const LMS_INVERSE = {
    red: { long: 4.076_741_662_1, medium: -3.307_711_591_3, short: 0.230_969_929_2 },
    green: { long: -1.268_438_004_6, medium: 2.609_757_401_1, short: -0.341_319_396_5 },
    blue: { long: -0.004_196_086_3, medium: -0.703_418_614_7, short: 1.707_614_701 },
};

const linear = (channel: number): number => {
    const c = channel / CHANNEL;
    return c <= SRGB.threshold ? c / SRGB.slope : ((c + SRGB.offset) / (1 + SRGB.offset)) ** SRGB.gamma;
};

const encoded = (channel: number): number => {
    const c = channel <= SRGB.linearThreshold ? channel * SRGB.slope : (1 + SRGB.offset) * channel ** (1 / SRGB.gamma) - SRGB.offset;
    return Math.min(CHANNEL, Math.max(0, c * CHANNEL));
};

const cubeRoot = (value: number): number => (value < 0 ? -((-value) ** (1 / CUBE)) : value ** (1 / CUBE));

const isTriple = (channels: number[]): channels is Triple => channels.length === CUBE;

const triple = (channels: number[]): Triple => {
    if (isTriple(channels)) {
        return channels;
    }
    throw new Error(`Expected ${CUBE} channels, got ${channels.length}`);
};

const toOklab = ([r, g, b]: Triple): Triple => {
    const [lr, lg, lb] = [linear(r), linear(g), linear(b)];
    const cone = (row: { readonly red: number; readonly green: number; readonly blue: number }): number => cubeRoot(row.red * lr + row.green * lg + row.blue * lb);
    const [cl, cm, cs] = [cone(LMS.long), cone(LMS.medium), cone(LMS.short)];
    const mix = (row: { readonly long: number; readonly medium: number; readonly short: number }): number => row.long * cl + row.medium * cm + row.short * cs;
    return [mix(OKLAB.lightness), mix(OKLAB.a), mix(OKLAB.b)];
};

const fromOklab = ([l, a, b]: Triple): Triple => {
    const cone = (row: { readonly lightness: number; readonly a: number; readonly b: number }): number => (row.lightness * l + row.a * a + row.b * b) ** CUBE;
    const [cl, cm, cs] = [cone(OKLAB_INVERSE.long), cone(OKLAB_INVERSE.medium), cone(OKLAB_INVERSE.short)];
    const mix = (row: { readonly long: number; readonly medium: number; readonly short: number }): number => row.long * cl + row.medium * cm + row.short * cs;
    return [encoded(mix(LMS_INVERSE.red)), encoded(mix(LMS_INVERSE.green)), encoded(mix(LMS_INVERSE.blue))];
};

const polar = ([l, a, b]: Triple): Triple => [l, (a ** 2 + b ** 2) ** HALF, (Math.atan2(b, a) / RADIANS + DEGREES) % DEGREES];

const cartesian = ([l, c, h]: Triple): Triple => [l, c * Math.cos(h * RADIANS), c * Math.sin(h * RADIANS)];

const sectorOf = (r: number, g: number, b: number, max: number, delta: number): number => {
    if (delta === 0) {
        return 0;
    }
    if (max === r) {
        return ((g - b) / delta) % HUE_SECTORS;
    }
    return max === g ? (b - r) / delta + 2 : (r - g) / delta + 2 * 2;
};

const toHsl = ([red, green, blue]: Triple): Triple => {
    const [r, g, b] = [red / CHANNEL, green / CHANNEL, blue / CHANNEL];
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const delta = max - min;
    const l = (max + min) * HALF;
    const s = delta === 0 ? 0 : delta / (1 - Math.abs(2 * l - 1));
    return [(sectorOf(r, g, b, max, delta) * (DEGREES / HUE_SECTORS) + DEGREES) % DEGREES, s, l];
};

const fromHsl = ([h, s, l]: Triple): Triple => {
    const a = s * Math.min(l, 1 - l);
    const channel = (n: number): number => {
        const k = (n + h / (DEGREES / HSL_SECTORS)) % HSL_SECTORS;
        return (l - a * Math.max(-1, Math.min(k - CUBE, HUE_SECTORS + CUBE - k, 1))) * CHANNEL;
    };
    return [channel(HSL_RED), channel(HSL_GREEN), channel(HSL_BLUE)];
};

const toLch = (rgb: Triple): Triple => polar(triple(app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.LAB, ColorConvertPurpose.defaultpurpose)));

const fromLch = (lch: Triple): Triple => triple(app.convertSampleColor(ImageColorSpace.LAB, cartesian(lch), ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose));

const forward = (space: Space, rgb: Triple): Triple => {
    if (space === 'oklab') {
        return toOklab(rgb);
    }
    if (space === 'oklch') {
        return polar(toOklab(rgb));
    }
    return space === 'lch' ? toLch(rgb) : toHsl(rgb);
};

const backward = (space: Space, value: Triple): Triple => {
    if (space === 'oklab') {
        return fromOklab(value);
    }
    if (space === 'oklch') {
        return fromOklab(cartesian(value));
    }
    return space === 'lch' ? fromLch(value) : fromHsl(value);
};

const arced = (from: number, to: number, arc: HueArc): [number, number] => {
    const diff = to - from;
    if (arc === 'shorter') {
        if (diff > HALF_TURN) {
            return [from + DEGREES, to];
        }
        return diff < -HALF_TURN ? [from, to + DEGREES] : [from, to];
    }
    if (arc === 'longer') {
        if (diff > 0 && diff < HALF_TURN) {
            return [from + DEGREES, to];
        }
        return diff > -HALF_TURN && diff <= 0 ? [from, to + DEGREES] : [from, to];
    }
    if (arc === 'increasing') {
        return diff < 0 ? [from, to + DEGREES] : [from, to];
    }
    return diff > 0 ? [from + DEGREES, to] : [from, to];
};

const hued = (space: Space, a: Triple, b: Triple, arc: HueArc, t: number): Triple => {
    if (space === 'oklab') {
        return [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t];
    }
    const index = space === 'hsl' ? 0 : 2;
    const [fromHue, toHue] = arced(a[index], b[index], arc);
    const hue = (fromHue + (toHue - fromHue) * t + DEGREES) % DEGREES;
    const [first, second] = space === 'hsl' ? [a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t] : [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t];
    return space === 'hsl' ? [hue, first, second] : [first, second, hue];
};

// --- [ENDPOINTS] -----------------------------------------------------------------------

interface Endpoint {
    readonly rgb: Triple;
    readonly black: number[];
}

const endpoint = (value: Color): Endpoint => {
    if (typed<SpotColor>('SpotColor')(value)) {
        const base = endpoint(value.spot.color);
        const white = 1 - value.tint / PERCENT;
        const [r, g, b] = base.rgb;
        return { rgb: [r + (CHANNEL - r) * white, g + (CHANNEL - g) * white, b + (CHANNEL - b) * white], black: base.black };
    }
    if (typed<GrayColor>('GrayColor')(value)) {
        return { rgb: triple(app.convertSampleColor(ImageColorSpace.GrayScale, [value.gray], ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose)), black: [] };
    }
    if (typed<CMYKColor>('CMYKColor')(value)) {
        const cmyk = [value.cyan, value.magenta, value.yellow, value.black];
        return {
            rgb: triple(app.convertSampleColor(ImageColorSpace.CMYK, cmyk, ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose)),
            black: value.cyan === 0 && value.magenta === 0 && value.yellow === 0 ? [value.black] : [],
        };
    }
    if (typed<RGBColor>('RGBColor')(value)) {
        return { rgb: [value.red, value.green, value.blue], black: [] };
    }
    throw new Error(`Gradient stop colour ${value.typename} cannot be blended`);
};

const mixed = (doc: Document, from: Endpoint, to: Endpoint, t: number, space: Space, arc: HueArc): Color => {
    const [fromK] = from.black;
    const [toK] = to.black;
    if (fromK !== undefined && toK !== undefined) {
        return color({ model: 'CMYK', values: [0, 0, 0, fromK + (toK - fromK) * t] });
    }
    const rgb = backward(space, hued(space, forward(space, from.rgb), forward(space, to.rgb), arc, t));
    if (doc.documentColorSpace === DocumentColorSpace.CMYK) {
        return color({ model: 'CMYK', values: app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.CMYK, ColorConvertPurpose.defaultpurpose) });
    }
    return color({ model: 'RGB', values: collect(rgb, Math.round) });
};

// --- [GRADIENTS] -----------------------------------------------------------------------

const gradientOf = (value: Color): Gradient[] => (typed<GradientColor>('GradientColor')(value) ? [value.gradient] : []);

const carrier = (item: PageItem): PathItem[] => {
    if (typed<PathItem>('PathItem')(item)) {
        return [item];
    }
    return typed<CompoundPathItem>('CompoundPathItem')(item) ? items<PathItem>(item.pathItems).slice(0, 1) : [];
};

const carrying = (item: PageItem, attributes: Blend['attributes']): Gradient[] =>
    fold<PathItem, Gradient[]>(carrier(item), [], (list, path): Gradient[] =>
        fold(attributes, list, (found, attribute): Gradient[] => found.concat(gradientOf(attribute === 'fill' ? path.fillColor : path.strokeColor))),
    );

const merged = (list: Found[], gradient: Gradient, uuid: string): Found[] =>
    select(list, (row): boolean => row.gradient.name === gradient.name).length === 0
        ? list.concat([{ gradient, objects: [uuid] }])
        : collect(list, (row): Found => (row.gradient.name === gradient.name ? { gradient: row.gradient, objects: row.objects.concat([uuid]) } : row));

const collected = (doc: Document, attributes: Blend['attributes']): Found[] =>
    fold<PageItem, Found[]>(flatten(items<PageItem>(doc.selection)), [], (found, item): Found[] =>
        fold(carrying(item, attributes), found, (list, gradient): Found[] => merged(list, gradient, item.uuid)),
    );

const blended = (doc: Document, gradient: Gradient, blend: Blend): number => {
    const stops = gradient.gradientStops;
    if (blend.removeIntermediateStops) {
        visit(items(stops).slice(1, -1), (stop): void => stop.remove());
    }
    const originals: Stop[] = collect(items(stops), (stop): Stop => ({ position: stop.rampPoint, opacity: stop.opacity, color: stop.color }));
    const added = (blend.precision - 1) * (originals.length - 1);
    visit(range(added), (): void => {
        stops.add();
    });
    const listed = items(stops);
    visit(listed.slice(-1), (last): void => {
        const target = last;
        target.rampPoint = LAST_STOP;
    });
    visit(pairs(originals), ([current, next], pair): void => {
        const from = endpoint(current.color);
        const to = endpoint(next.color);
        visit(listed.slice(pair * blend.precision, (pair + 1) * blend.precision), (stop, step): void => {
            const t = step / blend.precision;
            const target = stop;
            target.rampPoint = current.position + (next.position - current.position) * t;
            target.opacity = current.opacity + (next.opacity - current.opacity) * t;
            target.color = step === 0 ? current.color : mixed(doc, from, to, t, blend.space, blend.hueArc);
        });
    });
    visit(listed.slice(-1), (last): void => {
        visit(originals.slice(-1), (original): void => {
            const target = last;
            target.rampPoint = original.position;
            target.opacity = original.opacity;
            target.color = original.color;
        });
    });
    return added;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const gradientBlend = (request: Blend, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const found = collected(doc, request.attributes);
    const applied: JsonObject[] = [];
    const rejected: JsonObject[] = [];
    visit(found, ({ gradient }): void => {
        try {
            applied.push({ gradient: gradient.name, stopsAdded: blended(doc, gradient, request) });
        } catch (error) {
            rejected.push({ gradient: gradient.name, reason: error instanceof Error ? error.message : String(error) });
        }
    });
    app.redraw();
    const shared = collect(
        select(found, (row): boolean => row.objects.length > 1),
        (row): JsonObject => ({ gradient: row.gradient.name, objects: row.objects }),
    );
    return { value: { kind: 'gradientsBlended', applied, rejected, shared }, unavailable: [] };
};

run(gradientBlend);
