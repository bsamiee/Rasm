using System.Globalization;
using System.Numerics;
using HarfBuzzSharp;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record ColorValue {
    public sealed record Fixed(Unicolour Color) : ColorValue {
        public Fixed(string hex) : this(new Unicolour(hex)) { }
    }

    public sealed record Host(Option<Role> Role, Channel Channel) : ColorValue {
        public Fraction Alpha { get; init; } = Fraction.MaxValue;
        public Option<Condition> Condition { get; init; }
        public Option<ColorValue> Fallback { get; init; }
    }

    public sealed record Data : ColorValue {
        public Fraction Alpha { get; init; } = Fraction.MaxValue;
    }

    public sealed record Mixed(ColorValue From, ColorValue To, Fraction Amount) : ColorValue {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public HueSpan Hue { get; init; } = HueSpan.Shorter;
    }

    public sealed record Contrasting(ColorValue Over, Seq<ColorValue> Candidates) : ColorValue;

    public sealed record Relative(ColorValue From, ColourSpace Space, ChannelShift First, ChannelShift Second, ChannelShift Third) : ColorValue {
        public ChannelShift Alpha { get; init; } = new ChannelShift.Kept();

        internal SKColorF Shifted(SKColorF origin) =>
            origin.ToUnicolour() switch {
                var colour => colour.GetRepresentation(Space).Tuple switch {
                    var (first, second, third) => new Unicolour(Space, First.Applied(Kept(first)), Second.Applied(Kept(second)), Third.Applied(Kept(third)),
                            double.Clamp(Alpha.Applied(colour.Alpha.A), 0d, 1d))
                        .MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF(),
                },
            };

        private static double Kept(double channel) => double.IsNaN(channel) ? 0d : channel;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelShift {
    internal double Applied(double kept) =>
        Switch(kept, kept: static (held, _) => held, literal: static (_, literal) => literal.Value, offset: static (held, offset) => held + offset.By, scaled: static (held, scaled) => held * scaled.By);

    public sealed record Kept : ChannelShift;

    public sealed record Literal(double Value) : ChannelShift;

    public sealed record Offset(double By) : ChannelShift;

    public sealed record Scaled(double By) : ChannelShift;
}

public sealed record Series(uint Ordinal) {
    public Option<SKColorF> Color { get; init; }
}

public sealed record Halo(ColorValue Color, float Width, float Blur);

public sealed record TextFace : Monoid<TextFace> {
    public static TextFace Empty { get; } = new();

    public Option<string> Family { get; init; }
    public Option<SKFontStyleWeight> Weight { get; init; }
    public Option<SKFontStyleWidth> Width { get; init; }
    public Option<bool> Italic { get; init; }
    public Option<float> LetterSpacing { get; init; }
    public Option<float> LineHeight { get; init; }
    public Option<UnderlineStyle> Underline { get; init; }
    public Option<StrikeThroughStyle> StrikeThrough { get; init; }
    public Option<FontVariant> Variant { get; init; }
    public Option<Seq<Feature>> Features { get; init; }
    public Option<Halo> Halo { get; init; }

    public TextFace Combine(TextFace rhs) =>
        new() {
            Family = rhs.Family | Family,
            Weight = rhs.Weight | Weight,
            Width = rhs.Width | Width,
            Italic = rhs.Italic | Italic,
            LetterSpacing = rhs.LetterSpacing | LetterSpacing,
            LineHeight = rhs.LineHeight | LineHeight,
            Underline = rhs.Underline | Underline,
            StrikeThrough = rhs.StrikeThrough | StrikeThrough,
            Variant = rhs.Variant | Variant,
            Features = rhs.Features | Features,
            Halo = rhs.Halo | Halo,
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterFunction {
    private static readonly Vector3 Luminance = new(0.2126f, 0.7152f, 0.0722f);
    private static readonly Vector3 Saturation = new(0.213f, 0.715f, 0.072f);
    private static readonly (Vector3 Red, Vector3 Green, Vector3 Blue) Aged = (new(0.393f, 0.769f, 0.189f), new(0.349f, 0.686f, 0.168f), new(0.272f, 0.534f, 0.131f));
    private static readonly (Vector3 Red, Vector3 Green, Vector3 Blue) Quadrature = (new(-0.213f, -0.715f, 0.928f), new(0.143f, 0.140f, -0.283f), new(-0.787f, 0.715f, 0.072f));

    public static readonly FilterFunction Blur = new("blur", NonNegative, static (amount, input) => SKImageFilter.CreateBlur(amount, amount, input));
    public static readonly FilterFunction Grayscale = new("grayscale", NonNegative,
        static (amount, input) => Matrixed(Toward((Luminance, Luminance, Luminance), 1f - float.Min(amount, 1f)), Vector3.Zero, input));
    public static readonly FilterFunction Saturate = new("saturate", NonNegative, static (amount, input) => Matrixed(Toward((Saturation, Saturation, Saturation), amount), Vector3.Zero, input));
    public static readonly FilterFunction Brightness = new("brightness", NonNegative, static (amount, input) => Matrixed(Scaled(amount), Vector3.Zero, input));
    public static readonly FilterFunction Contrast = new("contrast", NonNegative, static (amount, input) => Matrixed(Scaled(amount), new Vector3(0.5f - (0.5f * amount)), input));
    public static readonly FilterFunction HueRotate = new("hue-rotate", float.IsFinite, static (amount, input) => Matrixed(Turned(float.DegreesToRadians(amount)), Vector3.Zero, input));
    public static readonly FilterFunction Invert = new("invert", NonNegative,
        static (amount, input) => float.Min(amount, 1f) switch { var held => Matrixed(Scaled(1f - (2f * held)), new Vector3(held), input) });
    public static readonly FilterFunction Sepia = new("sepia", NonNegative, static (amount, input) => Matrixed(Toward(Aged, 1f - float.Min(amount, 1f)), Vector3.Zero, input));

    [UseDelegateFromConstructor]
    public partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Toward((Vector3 Red, Vector3 Green, Vector3 Blue) target, float kept) =>
        (Vector3.Lerp(target.Red, Vector3.UnitX, kept), Vector3.Lerp(target.Green, Vector3.UnitY, kept), Vector3.Lerp(target.Blue, Vector3.UnitZ, kept));

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Scaled(float slope) => (Vector3.UnitX * slope, Vector3.UnitY * slope, Vector3.UnitZ * slope);

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Turned(float radians) =>
        MathF.SinCos(radians) switch {
            var (sin, cos) => (
                Vector3.Lerp(Saturation, Vector3.UnitX, cos) + (Quadrature.Red * sin),
                Vector3.Lerp(Saturation, Vector3.UnitY, cos) + (Quadrature.Green * sin),
                Vector3.Lerp(Saturation, Vector3.UnitZ, cos) + (Quadrature.Blue * sin)),
        };

    private static SKImageFilter Matrixed((Vector3 Red, Vector3 Green, Vector3 Blue) rows, Vector3 offset, SKImageFilter? input) {
        using SKColorFilter color = SKColorFilter.CreateColorMatrix([
            rows.Red.X, rows.Red.Y, rows.Red.Z, 0f, offset.X,
            rows.Green.X, rows.Green.Y, rows.Green.Z, 0f, offset.Y,
            rows.Blue.X, rows.Blue.Y, rows.Blue.Z, 0f, offset.Z,
            0f, 0f, 0f, 1f, 0f,
        ]);
        return SKImageFilter.CreateColorFilter(color, input);
    }
}

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Filter {
    public FilterFunction Function { get; }
    public float Amount { get; }

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref FilterFunction function, ref float amount) =>
        validationError = function.Accepts(amount) ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"{function.Key}() takes no amount {amount}"));

    internal static Option<SKImageFilter> Chained(Seq<Filter> filters) =>
        filters.Fold(Option<SKImageFilter>.None, static (input, filter) => input.Match(
            Some: inner => {
                using SKImageFilter held = inner;
                return Some(filter.Function.Built(filter.Amount, held));
            },
            None: () => Some(filter.Function.Built(filter.Amount, input: null))));
}

public sealed record Stop<TColor>(TColor Color, Fraction At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Material {
    public sealed record Solid(ColorValue Color) : Material;

    public sealed record Gradient(Seq<Stop<ColorValue>> Stops, float Angle) : Material;

    public sealed record Drawn(SKRuntimeEffect Effect, HashMap<string, ColorValue> Uniforms) : Material {
        internal const string Size = "size";
        internal const string Clock = "time";

        public bool Timed => Effect.Uniforms.Contains(Clock, StringComparer.Ordinal);
    }

    public sealed record Backdrop(bool Clear, ColorValue Fallback) : Material {
        public Option<ColorValue> Tint { get; init; }
    }

    public sealed record Blurred(float Sigma, ColorValue Tint) : Material;
}

public sealed record Stroke(ColorValue Color, Seq<Seq<float>> Dash = default) {
    public Seq<float> Rhythm(uint ordinal) => Dash.IsEmpty ? [] : Dash[(int)(ordinal % (uint)Dash.Count)];
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Shadow {
    public required ColorValue Color { get; init; }
    public required float Blur { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Spread { get; init; }

    internal abstract SKClipOperation Clip { get; }

    internal abstract float Overhang { get; }

    internal abstract void Cast(SKCanvas canvas, Func<float, SKPath> outline, SKColorF color);

    internal abstract SKImageFilter Filtered(SKColorF color, float scale);

    private protected T Masked<T>(SKColorF color, Func<SKPaint, T> use) {
        using SKMaskFilter? blur = Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Blur) : null;
        using SKPaint paint = new() { IsAntialias = true, ColorF = color, MaskFilter = blur };
        return use(paint);
    }

    private protected static SKImageFilter? Morphed(float by) =>
        by switch {
            > 0f => SKImageFilter.CreateDilate(by, by),
            < 0f => SKImageFilter.CreateErode(-by, -by),
            _ => null,
        };

    public sealed record Drop : Shadow {
        internal override SKClipOperation Clip => SKClipOperation.Difference;

        internal override float Overhang => Masked(SKColorF.Empty, paint => FastBounds.Beyond(paint, Spread, new SKPoint(X, Y)));

        internal override SKImageFilter Filtered(SKColorF color, float scale) {
            using SKImageFilter? grown = Morphed(Spread * scale);
            return SKImageFilter.CreateDropShadowOnly(X * scale, Y * scale, Blur * scale, Blur * scale, (SKColor)color, grown);
        }

        internal override void Cast(SKCanvas canvas, Func<float, SKPath> outline, SKColorF color) =>
            _ = Masked(color, paint => {
                using SKPath shape = outline(Spread);
                shape.Offset(X, Y);
                canvas.DrawPath(shape, paint);
                return unit;
            });
    }

    public sealed record Inset : Shadow {
        internal override SKClipOperation Clip => SKClipOperation.Intersect;

        internal override float Overhang => 0f;

        internal override SKImageFilter Filtered(SKColorF color, float scale) {
            using SKImageFilter? kept = Morphed(-Spread * scale);
            using SKColorFilter hollow = SKColorFilter.CreateColorMatrix([
                0f, 0f, 0f, 0f, color.Red,
                0f, 0f, 0f, 0f, color.Green,
                0f, 0f, 0f, 0f, color.Blue,
                0f, 0f, 0f, -color.Alpha, color.Alpha,
            ]);
            using SKImageFilter outside = SKImageFilter.CreateColorFilter(hollow, kept);
            using SKImageFilter moved = SKImageFilter.CreateOffset(X * scale, Y * scale, outside);
            using SKImageFilter blurred = SKImageFilter.CreateBlur(Blur * scale, Blur * scale, moved);
            return SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, background: null, foreground: blurred);
        }

        internal override void Cast(SKCanvas canvas, Func<float, SKPath> outline, SKColorF color) =>
            _ = Masked(color, paint => {
                using SKPath frame = outline(0f);
                using SKPath hole = outline(-Spread);
                hole.Offset(X, Y);
                _ = paint.GetFastBounds(frame.Bounds, out SKRect reach);
                using SKPath ring = Figure.Traced(path => {
                    path.FillType = SKPathFillType.EvenOdd;
                    path.AddRect(SKRect.Union(reach, hole.Bounds), SKPathDirection.Clockwise);
                    path.AddPath(hole, SKPathAddMode.Append);
                });
                canvas.DrawPath(ring, paint);
                return unit;
            });
    }
}

public sealed record Look : Monoid<Look> {
    public static Look Empty { get; } = new();

    public Option<Seq<Material>> Fills { get; init; }
    public Option<Seq<Stroke>> Strokes { get; init; }
    public Option<ColorValue> Text { get; init; }
    public TextFace Face { get; init; } = TextFace.Empty;
    public Option<Seq<Shadow>> Shadows { get; init; }
    public Option<Seq<Filter>> Filters { get; init; }
    public Option<Seq<ColorValue>> Palette { get; init; }
    public HashMap<Interaction, ColorValue> Washes { get; init; }
    public Option<SKBlendMode> Blend { get; init; }
    public Option<TimingCurve> Motion { get; init; }
    public HashMap<Condition, Look> When { get; init; }
    public HashMap<Role, Look> Within { get; init; }

    public Look Combine(Look rhs) =>
        new() {
            Fills = rhs.Fills | Fills,
            Strokes = rhs.Strokes | Strokes,
            Text = rhs.Text | Text,
            Face = Face.Combine(rhs.Face),
            Shadows = rhs.Shadows | Shadows,
            Filters = rhs.Filters | Filters,
            Palette = rhs.Palette | Palette,
            Washes = Washes.Union(rhs.Washes, Merge: static (_, _, over) => over),
            Blend = rhs.Blend | Blend,
            Motion = rhs.Motion | Motion,
            When = When.Union(rhs.When, Merge: static (_, under, over) => under.Combine(over)),
            Within = Within.Union(rhs.Within, Merge: static (_, under, over) => under.Combine(over)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FastBounds {
    private static readonly SKRect Probe = SKRect.Create(1f, 1f);

    internal static float Beyond(SKPaint paint, float grown, SKPoint offset) {
        SKRect drawn = SKRect.Inflate(Probe, grown, grown);
        drawn.Offset(offset);
        _ = paint.GetFastBounds(drawn, out SKRect bounds);
        return float.Max(float.Max(Probe.Left - bounds.Left, Probe.Top - bounds.Top), float.Max(bounds.Right - Probe.Right, bounds.Bottom - Probe.Bottom));
    }
}

internal static class Colours {
    extension(SKColorF color) {
        internal Unicolour ToUnicolour() => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);
    }

    extension(Unicolour colour) {
        internal SKColorF ToSKColorF() => new((float)colour.Rgb.R, (float)colour.Rgb.G, (float)colour.Rgb.B, (float)colour.Alpha.A);
    }
}
