// --- [IMPORTS] -------------------------------------------------------------------------

import { HashMap, Option, Record, Result, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

type FontFace = typeof FontFace.Type;
type Measured = typeof Measured.Type;

const CSS = { width: 100, weight: { normal: 400, medium: 500, bold: 700, heavy: 900 }, relative: { normal: 350, bold: 550 }, oblique: 11 } as const;
const CodePoint: Schema.Int = Schema.Int.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 0x10_ff_ff })));
const FontWeight: Schema.Finite = Schema.Finite.pipe(Schema.check(Schema.isBetween({ minimum: 1, maximum: 1000 })));
const _Size: Schema.Finite = Schema.Finite.pipe(Schema.check(Schema.isGreaterThan(0)));
const _Names: Schema.$Array<Schema.NonEmptyString> = Schema.Array(Schema.NonEmptyString);
const FontFace: Schema.Struct<
    Readonly<Record<'file' | 'digest' | 'nameTableDigest' | 'postScriptName' | 'family' | 'style' | 'fullName', Schema.NonEmptyString>> & {
        readonly index: Schema.Int;
        readonly version: Schema.OptionFromNullOr<Schema.String>;
        readonly variationFormat: Schema.OptionFromNullOr<Schema.Literals<readonly ['OpenType', 'TrueTypeGX']>>;
        readonly unitsPerEm: Schema.Finite;
        readonly weight: Schema.OptionFromNullOr<typeof FontWeight>;
        readonly width: Schema.Finite;
        readonly angle: Schema.Finite;
        readonly italic: Schema.Boolean;
        readonly isMonospace: Schema.Boolean;
        readonly aliases: Schema.Struct<Readonly<Record<'family' | 'fullName', Schema.$Array<Schema.NonEmptyString>>>>;
        readonly bbox: Schema.Struct<Readonly<Record<'minX' | 'minY' | 'maxX' | 'maxY', Schema.Finite>>>;
        readonly features: Schema.$Array<Schema.String>;
        readonly namedVariations: Schema.$Array<
            Schema.Struct<{
                readonly name: Schema.NonEmptyString;
                readonly postScriptName: Schema.OptionFromNullOr<Schema.NonEmptyString>;
                readonly coordinates: Schema.$Record<Schema.String, Schema.Finite>;
            }>
        >;
        readonly axes: Schema.$Record<
            Schema.String,
            Schema.Struct<{ readonly axisIndex: Schema.Int; readonly name: Schema.String; readonly min: Schema.Finite; readonly default: Schema.Finite; readonly max: Schema.Finite }>
        >;
        readonly codePoints: Schema.$Array<typeof CodePoint>;
    }
> = Schema.Struct({
    file: Schema.NonEmptyString,
    digest: Schema.NonEmptyString,
    nameTableDigest: Schema.NonEmptyString,
    index: Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0))),
    postScriptName: Schema.NonEmptyString,
    family: Schema.NonEmptyString,
    style: Schema.NonEmptyString,
    fullName: Schema.NonEmptyString,
    version: Schema.OptionFromNullOr(Schema.String),
    variationFormat: Schema.OptionFromNullOr(Schema.Literals(['OpenType', 'TrueTypeGX'])),
    unitsPerEm: _Size,
    weight: Schema.OptionFromNullOr(FontWeight),
    width: _Size,
    angle: Schema.Finite,
    italic: Schema.Boolean,
    isMonospace: Schema.Boolean,
    aliases: Schema.Struct({ family: _Names, fullName: _Names }),
    bbox: Schema.Struct({ minX: Schema.Finite, minY: Schema.Finite, maxX: Schema.Finite, maxY: Schema.Finite }),
    features: Schema.Array(Schema.String),
    namedVariations: Schema.Array(
        Schema.Struct({ name: Schema.NonEmptyString, postScriptName: Schema.OptionFromNullOr(Schema.NonEmptyString), coordinates: Schema.Record(Schema.String, Schema.Finite) }),
    ),
    axes: Schema.Record(Schema.String, Schema.Struct({ axisIndex: Schema.Int, name: Schema.String, min: Schema.Finite, default: Schema.Finite, max: Schema.Finite })),
    codePoints: Schema.Array(CodePoint),
});
const _Metric: Schema.OptionFromNullOr<Schema.Finite> = Schema.OptionFromNullOr(Schema.Finite);
const _Dimensions: Schema.Struct<
    Readonly<
        Record<
            | 'ascent'
            | 'descent'
            | 'lineGap'
            | 'capHeight'
            | 'xHeight'
            | 'underlinePosition'
            | 'underlineThickness'
            | 'strikeoutPosition'
            | 'strikeoutThickness'
            | `${'subscript' | 'superscript'}${'X' | 'Y'}${'Size' | 'Offset'}`,
            typeof _Metric
        >
    >
> = Schema.Struct({
    ascent: _Metric,
    descent: _Metric,
    lineGap: _Metric,
    capHeight: _Metric,
    xHeight: _Metric,
    underlinePosition: _Metric,
    underlineThickness: _Metric,
    strikeoutPosition: _Metric,
    strikeoutThickness: _Metric,
    subscriptXSize: _Metric,
    subscriptYSize: _Metric,
    subscriptXOffset: _Metric,
    subscriptYOffset: _Metric,
    superscriptXSize: _Metric,
    superscriptYSize: _Metric,
    superscriptXOffset: _Metric,
    superscriptYOffset: _Metric,
});
const _HeightSource: Schema.OptionFromNullOr<Schema.Literals<readonly ['OS/2', 'glyph']>> = Schema.OptionFromNullOr(Schema.Literals(['OS/2', 'glyph']));
const _Extents: Schema.Struct<Readonly<Record<'xBearing' | 'yBearing' | 'width' | 'height', Schema.Finite>>> = Schema.Struct({
    xBearing: Schema.Finite,
    yBearing: Schema.Finite,
    width: Schema.Finite,
    height: Schema.Finite,
});
const _Glyphs: Schema.toCodecJson<Schema.HashMap<typeof CodePoint, typeof _Extents>> = Schema.toCodecJson(Schema.HashMap(CodePoint, _Extents));
const Measured: Schema.Struct<
    typeof FontFace.fields & {
        readonly size: typeof _Size;
        readonly coordinates: Schema.$Record<Schema.String, Schema.Finite>;
        readonly designAxes: Schema.$Array<Schema.Finite>;
        readonly metrics: typeof _Dimensions;
        readonly heightSources: Schema.Struct<Readonly<Record<'capHeight' | 'xHeight', typeof _HeightSource>>>;
        readonly glyphs: typeof _Glyphs;
    }
> = Schema.Struct({
    ...FontFace.fields,
    size: _Size,
    coordinates: Schema.Record(Schema.String, Schema.Finite),
    designAxes: Schema.Array(Schema.Finite),
    metrics: _Dimensions,
    heightSources: Schema.Struct({ capHeight: _HeightSource, xHeight: _HeightSource }),
    glyphs: _Glyphs,
});

const _Fields = Schema.NonEmptyArray(Schema.String);
const MetricsError: Schema.TaggedUnion<{
    readonly metricsMissing: Schema.TaggedStruct<'metricsMissing', { readonly postScriptName: Schema.String; readonly fields: Schema.NonEmptyArray<Schema.String> }>;
    readonly fontChanged: Schema.TaggedStruct<'fontChanged', { readonly file: Schema.String }>;
    readonly invalidFontFit: Schema.TaggedStruct<'invalidFontFit', { readonly height: Schema.Number }>;
    readonly invalidFontCoordinates: Schema.TaggedStruct<'invalidFontCoordinates', { readonly file: Schema.String; readonly axes: Schema.NonEmptyArray<Schema.String> }>;
    readonly invalidCodePoints: Schema.TaggedStruct<'invalidCodePoints', { readonly codePoints: Schema.NonEmptyArray<Schema.Number> }>;
}> = Schema.TaggedUnion({
    metricsMissing: { postScriptName: Schema.String, fields: _Fields },
    fontChanged: { file: Schema.String },
    invalidFontFit: { height: Schema.Number },
    invalidFontCoordinates: { file: Schema.String, axes: _Fields },
    invalidCodePoints: { codePoints: Schema.NonEmptyArray(Schema.Number) },
});

// --- [FITTING] -------------------------------------------------------------------------

const fitMetrics = (
    measured: Measured,
    constraint: {
        readonly height: number;
        readonly extent: number;
    },
): Result.Result<Measured, typeof MetricsError.Type> => {
    if (!Number.isFinite(constraint.height) || constraint.height <= 0 || !Number.isFinite(constraint.extent) || constraint.extent <= 0) {
        return Result.fail(MetricsError.cases.invalidFontFit.make({ height: constraint.height }));
    }
    const scale = constraint.height / constraint.extent;
    const fitted = {
        size: measured.size * scale,
        metrics: Record.map(
            measured.metrics,
            Option.map((value) => value * scale),
        ),
        glyphs: HashMap.map(
            measured.glyphs,
            Record.map((value: number) => value * scale),
        ),
    };
    return Number.isFinite(fitted.size) &&
        fitted.size > 0 &&
        Record.every(fitted.metrics, (value) => Option.isNone(value) || Number.isFinite(value.value)) &&
        HashMap.every(fitted.glyphs, Record.every(Number.isFinite))
        ? Result.succeed({ ...measured, ...fitted })
        : Result.fail(MetricsError.cases.invalidFontFit.make({ height: constraint.height }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CodePoint, CSS, FontFace, FontWeight, fitMetrics, Measured, MetricsError };
