using System.Numerics;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class FilterFunction {
    private static readonly Vector3 Luma = new((float)YbrConfiguration.Rec709.Kr, (float)YbrConfiguration.Rec709.Kg, (float)YbrConfiguration.Rec709.Kb);
    private static readonly Matrix4x4 Luminance = Rows(Luma, Luma, Luma);
    private static readonly Matrix4x4 SepiaMatrix = Rows(new(0.393f, 0.769f, 0.189f), new(0.349f, 0.686f, 0.168f), new(0.272f, 0.534f, 0.131f));
    private static readonly Matrix4x4 HueRotation = Rows(new(-Luma.X, -Luma.Y, 1f - Luma.Z), new(0.143f, 0.140f, -0.283f), new(Luma.X - 1f, Luma.Y, Luma.Z));

    public static readonly FilterFunction Blur = new(NonNegative, static (amount, input, _) => SKImageFilter.CreateBlur(amount, amount, input));
    public static readonly FilterFunction Grayscale = new(NonNegative, Matrixed(static amount => (Toward(Luminance, amount), Vector3.Zero)));
    public static readonly FilterFunction Saturate = new(NonNegative, Matrixed(static amount => (Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, amount), Vector3.Zero)));
    public static readonly FilterFunction Brightness = new(NonNegative, Matrixed(static amount => (Matrix4x4.CreateScale(amount), Vector3.Zero)));
    public static readonly FilterFunction Contrast = new(NonNegative, Matrixed(static amount => (Matrix4x4.CreateScale(amount), new Vector3(0.5f - (0.5f * amount)))));
    public static readonly FilterFunction HueRotate = new(float.IsFinite, Matrixed(static amount => (HueRotated(float.DegreesToRadians(amount)), Vector3.Zero)));
    public static readonly FilterFunction Invert = new(NonNegative, Matrixed(static amount => float.Min(amount, 1f) switch { var held => (Matrix4x4.CreateScale(1f - (2f * held)), new Vector3(held)) }));
    public static readonly FilterFunction Sepia = new(NonNegative, Matrixed(static amount => (Toward(SepiaMatrix, amount), Vector3.Zero)));

    [UseDelegateFromConstructor]
    internal partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input, SKColorSpace target);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static Matrix4x4 Rows(Vector3 red, Vector3 green, Vector3 blue) => new(red.X, red.Y, red.Z, 0f, green.X, green.Y, green.Z, 0f, blue.X, blue.Y, blue.Z, 0f, 0f, 0f, 0f, 1f);

    private static Matrix4x4 Toward(Matrix4x4 full, float amount) => Matrix4x4.Lerp(full, Matrix4x4.Identity, 1f - float.Min(amount, 1f));

    private static Matrix4x4 HueRotated(float radians) {
        (float sin, float cos) = MathF.SinCos(radians);
        return Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, cos) + (HueRotation * sin);
    }

    private static Func<float, SKImageFilter?, SKColorSpace, SKImageFilter> Matrixed(Func<float, (Matrix4x4 Rows, Vector3 Offset)> matrix) =>
        (amount, input, target) => {
            (Matrix4x4 rows, Vector3 offset) = matrix(amount);
            using SKColorFilter color = target.ColorMatrix([
                rows.M11, rows.M12, rows.M13, 0f, offset.X,
                rows.M21, rows.M22, rows.M23, 0f, offset.Y,
                rows.M31, rows.M32, rows.M33, 0f, offset.Z,
                0f, 0f, 0f, 1f, 0f,
            ]);
            return SKImageFilter.CreateColorFilter(color, input);
        };
}

internal sealed record FilterTarget(SKColorSpace Space, SKRect Frame, Func<ColorValue, Option<SKColorF>> Resolve, Func<Metric, float> Metric, Func<Material, Option<SKShader>> Shader) {
    internal Corners Corners { get; init; } = Corners.Square;
    internal float Clock { get; init; }
    internal Option<SKPoint> Pointer { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Filter {
    // --- [CHAIN]
    internal static readonly FilterTarget Probe = new(SKColorSpace.CreateSrgb(), SKRect.Create(1f, 1f), static _ => None, static metric => metric.Default, static _ => SKShader.CreateEmpty());

    internal static readonly Func<Seq<Filter>, float> Outset = memo<Seq<Filter>, float>(static filters => Chained(filters, Probe).Map(Reach).IfNone(0f));

    internal virtual Seq<GlyphSource> Arts => [];

    internal virtual bool Timed => false;

    internal virtual bool Pointing => false;

    internal abstract Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input);

    internal static Option<SKImageFilter> Chained(Seq<Filter> filters, FilterTarget target) =>
        filters.Fold(Option<SKImageFilter>.None, (input, filter) =>
            filter.Built(target, input.ValueUnsafe()).Map(built => {
                _ = input.Iter(static held => held.Dispose());
                return built;
            }) | input);

    // --- [REACH]
    internal static float Reach(SKImageFilter graph) {
        using SKImageFilter owned = graph;
        using SKPaint probe = new() { ImageFilter = owned };
        _ = probe.GetFastBounds(SKRect.Create(1f, 1f), out SKRect bounds);
        return float.Max(float.Max(-bounds.Left, -bounds.Top), float.Max(bounds.Right - 1f, bounds.Bottom - 1f));
    }

    // --- [CASES]
    public sealed record Css : Filter {
        private Css(FilterFunction function, float amount) => (Function, Amount) = (function, amount);

        public FilterFunction Function { get; }
        public float Amount { get; }

        public static Fin<Css> Validated(FilterFunction function, float amount) => UIFault.Unless(function.Accepts(amount), UIReason.Range, amount).Map(_ => new Css(function, amount));

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) => Function.Built(Amount, input, target.Space);
    }

    public sealed record Displace(Material Source, float Scale) : Filter {
        internal override Seq<GlyphSource> Arts => Source.Arts;

        internal override bool Timed => Source.Timed;

        internal override bool Pointing => Source.Pointing;

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            target.Shader(Source).Map(map => {
                using SKShader owned = map;
                using SKImageFilter displacement = SKImageFilter.CreateShader(owned);
                return SKImageFilter.CreateDisplacementMapEffect(SKColorChannel.R, SKColorChannel.G, Scale, displacement, input);
            });
    }

    public sealed record Lit(SKPoint3 Direction, ColorValue Light, float SurfaceScale, float Strength) : Filter {
        public Option<float> Shine { get; init; }

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            target.Resolve(Light).Map(light => Shine.Match(
                Some: shine => SKImageFilter.CreateDistantLitSpecular(Direction, (SKColor)light, SurfaceScale, Strength, shine, input),
                None: () => SKImageFilter.CreateDistantLitDiffuse(Direction, (SKColor)light, SurfaceScale, Strength, input)));
    }

    public sealed record Magnify(float Zoom, float Inset) : Filter {
        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            SKImageFilter.CreateMagnifier(target.Frame, Zoom, Inset, new SKSamplingOptions(SKFilterMode.Linear), input);
    }

    public sealed record Convolve : Filter {
        private Convolve(SKSizeI size, Seq<float> kernel, float gain, float bias) => (Size, Kernel, Gain, Bias) = (size, kernel, gain, bias);

        public SKSizeI Size { get; }
        public Seq<float> Kernel { get; }
        public float Gain { get; }
        public float Bias { get; }

        public static Fin<Convolve> Validated(SKSizeI size, Seq<float> kernel, float gain, float bias) =>
            UIFault.Unless(size is { Width: > 0, Height: > 0 } && kernel.Count == size.Width * size.Height, UIReason.Range, (size, kernel.Count)).Map(_ => new Convolve(size, kernel, gain, bias));

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            SKImageFilter.CreateMatrixConvolution(Size, [.. Kernel], Gain, Bias, new SKPointI(Size.Width / 2, Size.Height / 2), SKShaderTileMode.Clamp, convolveAlpha: false, input);
    }

    public sealed record Effect : Filter {
        private readonly RuntimeEffect effect;

        private Effect(RuntimeEffect effect) => this.effect = effect;

        internal override Seq<GlyphSource> Arts => effect.Arts;

        internal override bool Timed => effect.Timed;

        internal override bool Pointing => effect.Pointing;

        public static Fin<Effect> Validated(string sksl, HashMap<string, ColorValue> colors = default, HashMap<string, Metric> floats = default, HashMap<string, Material> children = default) =>
            RuntimeEffect.Validated(SKRuntimeEffect.CreateColorFilter, sksl, colors, floats, children).Map(static effect => new Effect(effect));

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            effect.Bound(target.Resolve, target.Metric, target.Shader, target.Frame, target.Corners, target.Clock, target.Pointer, SKMatrix.Identity, static (program, uniforms, children) => program.ToColorFilter(uniforms, children))
                .Map(filter => {
                    using SKColorFilter owned = filter;
                    return SKImageFilter.CreateColorFilter(owned, input);
                });
    }

    public sealed record ColorFilter(SKColorFilter Source) : Filter {
        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) => SKImageFilter.CreateColorFilter(Source, input);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Blenders {
    public static SKBlender PlusDarker { get; } = Built("half4 main(half4 src, half4 dst) { half a = src.a + dst.a - (src.a * dst.a); return half4(max(half3(0.0), a - (dst.a - dst.rgb) - (src.a - src.rgb)), a); }");

    private static SKBlender Built(string sksl) {
        using SKRuntimeBlenderBuilder builder = SKRuntimeEffect.BuildBlender(sksl);
        return builder.Build();
    }
}

internal static class ColorSpaceExtensions {
    extension(SKColorSpace target) {
        internal SKColorFilter ColorMatrix(ReadOnlySpan<float> srgb) {
            SKColorFilter matrix = SKColorFilter.CreateColorMatrix(srgb);
            if (!target.GammaIsLinear) return matrix;
            using (matrix) {
                using SKColorFilter encoded = SKColorFilter.CreateLinearToSrgbGamma();
                using SKColorFilter decoded = SKColorFilter.CreateSrgbToLinearGamma();
                using SKColorFilter inner = SKColorFilter.CreateCompose(matrix, encoded);
                return SKColorFilter.CreateCompose(decoded, inner);
            }
        }
    }
}
