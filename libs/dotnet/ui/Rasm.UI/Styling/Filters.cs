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
    public static readonly FilterFunction Grayscale = new(NonNegative, static (amount, input, target) => ColorMatrix(Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, 1f - float.Min(amount, 1f)), Vector3.Zero, input, target));
    public static readonly FilterFunction Saturate = new(NonNegative, static (amount, input, target) => ColorMatrix(Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, amount), Vector3.Zero, input, target));
    public static readonly FilterFunction Brightness = new(NonNegative, static (amount, input, target) => ColorMatrix(Matrix4x4.CreateScale(amount), Vector3.Zero, input, target));
    public static readonly FilterFunction Contrast = new(NonNegative, static (amount, input, target) => ColorMatrix(Matrix4x4.CreateScale(amount), new Vector3(0.5f - (0.5f * amount)), input, target));
    public static readonly FilterFunction HueRotate = new(float.IsFinite, static (amount, input, target) => ColorMatrix(HueRotated(float.DegreesToRadians(amount)), Vector3.Zero, input, target));
    public static readonly FilterFunction Invert = new(NonNegative,
        static (amount, input, target) => {
            float held = float.Min(amount, 1f);
            return ColorMatrix(Matrix4x4.CreateScale(1f - (2f * held)), new Vector3(held), input, target);
        });
    public static readonly FilterFunction Sepia = new(NonNegative, static (amount, input, target) => ColorMatrix(Matrix4x4.Lerp(SepiaMatrix, Matrix4x4.Identity, 1f - float.Min(amount, 1f)), Vector3.Zero, input, target));

    [UseDelegateFromConstructor]
    public partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input, SKColorSpace target);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static Matrix4x4 Rows(Vector3 red, Vector3 green, Vector3 blue) => new(red.X, red.Y, red.Z, 0f, green.X, green.Y, green.Z, 0f, blue.X, blue.Y, blue.Z, 0f, 0f, 0f, 0f, 1f);

    private static Matrix4x4 HueRotated(float radians) {
        (float sin, float cos) = MathF.SinCos(radians);
        return Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, cos) + (HueRotation * sin);
    }

    private static SKImageFilter ColorMatrix(Matrix4x4 rows, Vector3 offset, SKImageFilter? input, SKColorSpace target) {
        using SKColorFilter color = target.ColorMatrix([
            rows.M11, rows.M12, rows.M13, 0f, offset.X,
            rows.M21, rows.M22, rows.M23, 0f, offset.Y,
            rows.M31, rows.M32, rows.M33, 0f, offset.Z,
            0f, 0f, 0f, 1f, 0f,
        ]);
        return SKImageFilter.CreateColorFilter(color, input);
    }
}

internal sealed record FilterTarget(SKColorSpace Space, SKRect Frame, Func<ColorValue, Option<SKColorF>> Resolve, Func<Child, Option<SKShader>> Child);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Filter {
    // --- [CHAIN]
    internal virtual Seq<GlyphSource> Arts => [];

    internal abstract Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input);

    internal static Option<SKImageFilter> Chained(Seq<Filter> filters, FilterTarget target) =>
        filters.Fold(Option<SKImageFilter>.None, (input, filter) =>
            filter.Built(target, input.ValueUnsafe()).Map(built => {
                _ = input.Iter(static held => held.Dispose());
                return built;
            }) | input);

    // --- [CASES]
    public sealed record Css : Filter {
        private Css(FilterFunction function, float amount) => (Function, Amount) = (function, amount);

        public FilterFunction Function { get; }
        public float Amount { get; }

        public static Fin<Css> Validated(FilterFunction function, float amount) => UIFault.Unless(function.Accepts(amount), UIReason.Range, amount).Map(_ => new Css(function, amount));

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) => Function.Built(Amount, input, target.Space);
    }

    public sealed record Displace(Child Source, float Scale) : Filter {
        internal override Seq<GlyphSource> Arts => Source.Arts;

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            target.Child(Source).Map(map => {
                using SKShader owned = map;
                using SKImageFilter displacement = SKImageFilter.CreateShader(owned);
                return SKImageFilter.CreateDisplacementMapEffect(SKColorChannel.R, SKColorChannel.G, Scale, displacement, input);
            });
    }

    public sealed record Lit(SKPoint3 Direction, ColorValue Light, float SurfaceScale, float Specular, float Shine) : Filter {
        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) =>
            target.Resolve(Light).Map(light => SKImageFilter.CreateDistantLitSpecular(Direction, (SKColor)light, SurfaceScale, Specular, Shine, input));
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
        private Effect(SKRuntimeEffect program) => Program = program;

        public SKRuntimeEffect Program { get; }

        public static Fin<Effect> Validated(string sksl) =>
            SKRuntimeEffect.CreateColorFilter(sksl, out string errors) switch {
                null => new UIFault(errors, UIReason.Refused),
                var program => new Effect(program),
            };

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) {
            using SKColorFilter program = Program.ToColorFilter();
            return SKImageFilter.CreateColorFilter(program, input);
        }
    }

    public sealed record HighContrast : Filter {
        private HighContrast(bool grayscale, SKHighContrastConfigInvertStyle invert, float contrast) => (Grayscale, Invert, Contrast) = (grayscale, invert, contrast);

        public bool Grayscale { get; }
        public SKHighContrastConfigInvertStyle Invert { get; }
        public float Contrast { get; }

        public static Fin<HighContrast> Validated(bool grayscale, SKHighContrastConfigInvertStyle invert, float contrast) =>
            UIFault.Unless(new SKHighContrastConfig(grayscale, invert, contrast).IsValid, UIReason.Range, (invert, contrast)).Map(_ => new HighContrast(grayscale, invert, contrast));

        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) {
            using SKColorFilter contrast = SKColorFilter.CreateHighContrast(Grayscale, Invert, Contrast);
            return SKImageFilter.CreateColorFilter(contrast, input);
        }
    }

    public sealed record Luma : Filter {
        internal override Option<SKImageFilter> Built(FilterTarget target, SKImageFilter? input) {
            using SKColorFilter luma = SKColorFilter.CreateLumaColor();
            return SKImageFilter.CreateColorFilter(luma, input);
        }
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
