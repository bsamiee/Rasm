/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum ColorConvertPurpose {}
    enum DocumentColorSpace {}
    enum ImageColorSpace {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, flatten, fold, items, run, select, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

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

const toOklab = (rgb: number[]): number[] => {
    const [r, g, b] = collect(rgb, linear) as [number, number, number];
    const cone = (row: { readonly red: number; readonly green: number; readonly blue: number }): number => cubeRoot(row.red * r + row.green * g + row.blue * b);
    const cones = [cone(LMS.long), cone(LMS.medium), cone(LMS.short)] as [number, number, number];
    const mix = (row: { readonly long: number; readonly medium: number; readonly short: number }): number => row.long * cones[0] + row.medium * cones[1] + row.short * cones[2];
    return [mix(OKLAB.lightness), mix(OKLAB.a), mix(OKLAB.b)];
};

const fromOklab = (lab: number[]): number[] => {
    const [l, a, b] = lab as [number, number, number];
    const cone = (row: { readonly lightness: number; readonly a: number; readonly b: number }): number => (row.lightness * l + row.a * a + row.b * b) ** CUBE;
    const cones = [cone(OKLAB_INVERSE.long), cone(OKLAB_INVERSE.medium), cone(OKLAB_INVERSE.short)] as [number, number, number];
    const mix = (row: { readonly long: number; readonly medium: number; readonly short: number }): number => row.long * cones[0] + row.medium * cones[1] + row.short * cones[2];
    return collect([mix(LMS_INVERSE.red), mix(LMS_INVERSE.green), mix(LMS_INVERSE.blue)], encoded);
};

const polar = ([l, a, b]: number[]): number[] => {
    const hue = (Math.atan2(b ?? 0, a ?? 0) / RADIANS + DEGREES) % DEGREES;
    return [l ?? 0, (((a ?? 0) ** 2 + (b ?? 0) ** 2) ** HALF), hue];
};

const cartesian = ([l, c, h]: number[]): number[] => [l ?? 0, (c ?? 0) * Math.cos((h ?? 0) * RADIANS), (c ?? 0) * Math.sin((h ?? 0) * RADIANS)];

const sectorOf = (r: number, g: number, b: number, max: number, delta: number): number => {
    if (delta === 0) {
        return 0;
    }
    if (max === r) {
        return ((g - b) / delta) % (CUBE * 2);
    }
    return max === g ? (b - r) / delta + 2 : (r - g) / delta + 2 * 2;
};

const toHsl = (rgb: number[]): number[] => {
    const [r, g, b] = collect(rgb, (channel): number => channel / CHANNEL) as [number, number, number];
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const delta = max - min;
    const l = (max + min) * HALF;
    const s = delta === 0 ? 0 : delta / (1 - Math.abs(2 * l - 1));
    const hue = ((sectorOf(r, g, b, max, delta) * (DEGREES / (CUBE * 2))) + DEGREES) % DEGREES;
    return [hue, s, l];
};

const fromHsl = ([h, s, l]: number[]): number[] => {
    const hue = h ?? 0;
    const chroma = (1 - Math.abs(2 * (l ?? 0) - 1)) * (s ?? 0);
    const x = chroma * (1 - Math.abs(((hue / (DEGREES / (CUBE * 2))) % 2) - 1));
    const m = (l ?? 0) - chroma * HALF;
    const sector = Math.floor(hue / (DEGREES / (CUBE * 2)));
    const sectors: number[][] = [
        [chroma, x, 0],
        [x, chroma, 0],
        [0, chroma, x],
        [0, x, chroma],
        [x, 0, chroma],
        [chroma, 0, x],
    ];
    return collect(sectors[sector] ?? [0, 0, 0], (channel): number => (channel + m) * CHANNEL);
};

const toLch = (rgb: number[]): number[] => polar(app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.LAB, ColorConvertPurpose.defaultpurpose));

const fromLch = (lch: number[]): number[] => app.convertSampleColor(ImageColorSpace.LAB, cartesian(lch), ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose);

const forward = (space: Space, rgb: number[]): number[] => {
    if (space === 'oklab') {
        return toOklab(rgb);
    }
    if (space === 'oklch') {
        return polar(toOklab(rgb));
    }
    return space === 'lch' ? toLch(rgb) : toHsl(rgb);
};

const backward = (space: Space, value: number[]): number[] => {
    if (space === 'oklab') {
        return fromOklab(value);
    }
    if (space === 'oklch') {
        return fromOklab(cartesian(value));
    }
    return space === 'lch' ? fromLch(value) : fromHsl(value);
};

const hueIndex = (space: Space): number => (space === 'hsl' ? 0 : 2);

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

// --- [ENDPOINTS] -----------------------------------------------------------------------

interface Endpoint {
    readonly rgb: number[];
    readonly black: number[];
}

const rgbOf = (value: Color): number[] => {
    const rgb = value as RGBColor;
    return [rgb.red, rgb.green, rgb.blue];
};

const cmykOf = (value: Color): number[] => {
    const cmyk = value as CMYKColor;
    return [cmyk.cyan, cmyk.magenta, cmyk.yellow, cmyk.black];
};

const endpoint = (value: Color): Endpoint => {
    if (value.typename === 'SpotColor') {
        const spot = value as SpotColor;
        const base = endpoint(spot.spot.color);
        const white = 1 - spot.tint / PERCENT;
        return { rgb: collect(base.rgb, (channel): number => channel + (CHANNEL - channel) * white), black: base.black };
    }
    if (value.typename === 'GrayColor') {
        return { rgb: app.convertSampleColor(ImageColorSpace.GrayScale, [(value as GrayColor).gray], ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose), black: [] };
    }
    if (value.typename === 'CMYKColor') {
        const [c, m, y, k] = cmykOf(value);
        return { rgb: app.convertSampleColor(ImageColorSpace.CMYK, cmykOf(value), ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose), black: c === 0 && m === 0 && y === 0 ? [k ?? 0] : [] };
    }
    if (value.typename === 'RGBColor') {
        return { rgb: rgbOf(value), black: [] };
    }
    throw new Error(`Gradient stop colour ${value.typename} cannot be blended`);
};

const mixed = (doc: Document, from: Endpoint, to: Endpoint, t: number, space: Space, arc: HueArc): Color => {
    const [fromK] = from.black;
    const [toK] = to.black;
    if (fromK !== undefined && toK !== undefined) {
        return color({ model: 'CMYK', values: [0, 0, 0, fromK + (toK - fromK) * t] });
    }
    const a = forward(space, from.rgb);
    const b = forward(space, to.rgb);
    const index = hueIndex(space);
    if (space !== 'oklab') {
        const [fromHue, toHue] = arced(a[index] ?? 0, b[index] ?? 0, arc);
        a[index] = fromHue;
        b[index] = toHue;
    }
    const out = collect(a, (channel, position): number => channel + ((b[position] ?? 0) - channel) * t);
    if (index < out.length && space !== 'oklab') {
        out[index] = ((out[index] ?? 0) + DEGREES) % DEGREES;
    }
    const rgb = backward(space, out);
    if (doc.documentColorSpace === DocumentColorSpace.CMYK) {
        return color({ model: 'CMYK', values: app.convertSampleColor(ImageColorSpace.RGB, rgb, ImageColorSpace.CMYK, ColorConvertPurpose.defaultpurpose) });
    }
    return color({ model: 'RGB', values: collect(rgb, (channel): number => Math.round(channel)) });
};

// --- [GRADIENTS] -----------------------------------------------------------------------

const gradientOf = (value: Color): Gradient[] => (value.typename === 'GradientColor' ? [(value as GradientColor).gradient] : []);

const carrying = (item: PageItem, attributes: Blend['attributes']): Gradient[] => {
    if (item.typename !== 'PathItem' && item.typename !== 'CompoundPathItem') {
        return [];
    }
    const path = item.typename === 'PathItem' ? (item as PathItem) : (items((item as CompoundPathItem).pathItems)[0] as PathItem | undefined);
    if (path === undefined) {
        return [];
    }
    return fold(attributes, [] as Gradient[], (list, attribute): Gradient[] => list.concat(gradientOf(attribute === 'fill' ? path.fillColor : path.strokeColor)));
};

const collected = (doc: Document, attributes: Blend['attributes']): Found[] =>
    fold(flatten(items<PageItem>(doc.selection)), [] as Found[], (found, item): Found[] =>
        fold(carrying(item, attributes), found, (list, gradient): Found[] => {
            const [known] = select(list, (row): boolean => row.gradient.name === gradient.name);
            if (known === undefined) {
                return list.concat([{ gradient, objects: [item.uuid] }]);
            }
            known.objects.push(item.uuid);
            return list;
        }),
    );

const blended = (doc: Document, gradient: Gradient, blend: Blend): number => {
    const stops = gradient.gradientStops;
    if (blend.removeIntermediateStops) {
        for (let index = stops.length - 2; index > 0; index -= 1) {
            (stops[index] as GradientStop).remove();
        }
    }
    const originals: Stop[] = collect(items(stops), (stop): Stop => ({ position: stop.rampPoint, opacity: stop.opacity, color: stop.color }));
    const added = (blend.precision - 1) * (originals.length - 1);
    for (let count = 0; count < added; count += 1) {
        stops.add();
    }
    const last = stops.length - 1;
    (stops[last] as GradientStop).rampPoint = LAST_STOP;
    let pair = 0;
    let from = endpoint((originals[0] as Stop).color);
    let to = endpoint((originals[1] as Stop).color);
    for (let index = 0; index < stops.length; index += 1) {
        const stop = stops[index] as GradientStop;
        if (index % blend.precision === 0) {
            const original = originals[index / blend.precision] as Stop;
            stop.rampPoint = original.position;
            stop.opacity = original.opacity;
            stop.color = original.color;
            pair = index / blend.precision;
            if (pair + 1 < originals.length) {
                from = endpoint((originals[pair] as Stop).color);
                to = endpoint((originals[pair + 1] as Stop).color);
            }
        } else {
            const previous = stops[index - 1] as GradientStop;
            const current = originals[pair] as Stop;
            const next = originals[pair + 1] as Stop;
            stop.rampPoint = previous.rampPoint + (next.position - current.position) / blend.precision;
            stop.opacity = previous.opacity + (next.opacity - current.opacity) / blend.precision;
            stop.color = mixed(doc, from, to, (index % blend.precision) / blend.precision, blend.space, blend.hueArc);
        }
    }
    const final = originals.length - 1;
    (stops[last] as GradientStop).rampPoint = (originals[final] as Stop).position;
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
            rejected.push({ gradient: gradient.name, reason: (error as Error).message });
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
