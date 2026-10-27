using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class FilterFunction {
    private static readonly Matrix4x4 Luminance = Rows(new(0.2126f, 0.7152f, 0.0722f), new(0.2126f, 0.7152f, 0.0722f), new(0.2126f, 0.7152f, 0.0722f));
    private static readonly Matrix4x4 Saturation = Rows(new(0.213f, 0.715f, 0.072f), new(0.213f, 0.715f, 0.072f), new(0.213f, 0.715f, 0.072f));
    private static readonly Matrix4x4 Aged = Rows(new(0.393f, 0.769f, 0.189f), new(0.349f, 0.686f, 0.168f), new(0.272f, 0.534f, 0.131f));
    private static readonly Matrix4x4 Quadrature = Rows(new(-0.213f, -0.715f, 0.928f), new(0.143f, 0.140f, -0.283f), new(-0.787f, 0.715f, 0.072f));

    public static readonly FilterFunction Blur = new(NonNegative, static (amount, input) => SKImageFilter.CreateBlur(amount, amount, input));
    public static readonly FilterFunction Grayscale = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, 1f - float.Min(amount, 1f)), Vector3.Zero, input));
    public static readonly FilterFunction Saturate = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.Lerp(Saturation, Matrix4x4.Identity, amount), Vector3.Zero, input));
    public static readonly FilterFunction Brightness = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.CreateScale(amount), Vector3.Zero, input));
    public static readonly FilterFunction Contrast = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.CreateScale(amount), new Vector3(0.5f - (0.5f * amount)), input));
    public static readonly FilterFunction HueRotate = new(float.IsFinite, static (amount, input) => Matrixed(Turned(float.DegreesToRadians(amount)), Vector3.Zero, input));
    public static readonly FilterFunction Invert = new(NonNegative,
        static (amount, input) => {
            float held = float.Min(amount, 1f);
            return Matrixed(Matrix4x4.CreateScale(1f - (2f * held)), new Vector3(held), input);
        });
    public static readonly FilterFunction Sepia = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.Lerp(Aged, Matrix4x4.Identity, 1f - float.Min(amount, 1f)), Vector3.Zero, input));

    [UseDelegateFromConstructor]
    public partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static Matrix4x4 Rows(Vector3 red, Vector3 green, Vector3 blue) => new(red.X, red.Y, red.Z, 0f, green.X, green.Y, green.Z, 0f, blue.X, blue.Y, blue.Z, 0f, 0f, 0f, 0f, 1f);

    private static Matrix4x4 Turned(float radians) {
        (float sin, float cos) = MathF.SinCos(radians);
        return Matrix4x4.Lerp(Saturation, Matrix4x4.Identity, cos) + (Quadrature * sin);
    }

    private static SKImageFilter Matrixed(Matrix4x4 rows, Vector3 offset, SKImageFilter? input) {
        using SKColorFilter color = SKColorFilter.CreateColorMatrix([
            rows.M11, rows.M12, rows.M13, 0f, offset.X,
            rows.M21, rows.M22, rows.M23, 0f, offset.Y,
            rows.M31, rows.M32, rows.M33, 0f, offset.Z,
            0f, 0f, 0f, 1f, 0f,
        ]);
        return SKImageFilter.CreateColorFilter(color, input);
    }
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class Filter {
    public FilterFunction Function { get; }
    public float Amount { get; }

    public static Fin<Filter> Validated(FilterFunction function, float amount) => UIFault.Unless(function.Accepts(amount), UIReason.Range, amount).Map(_ => new Filter(function, amount));

    internal static Option<SKImageFilter> Chained(Seq<Filter> filters) =>
        filters.Fold(Option<SKImageFilter>.None, static (input, filter) => input.Match(
            Some: inner => {
                using SKImageFilter held = inner;
                return Some(filter.Function.Built(filter.Amount, held));
            },
            None: () => Some(filter.Function.Built(filter.Amount, input: null))));
}
