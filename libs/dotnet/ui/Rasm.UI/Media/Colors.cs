using System.Numerics;
using Rasm.UI.Motion;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Media;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ColorValue {
    public Fraction Alpha { get; init; } = Fraction.MaxValue;
    public Option<ColorValue> Fallback { get; init; }

    public sealed record Fixed(Unicolour Color) : ColorValue;

    public sealed record Host(Token Token) : ColorValue;

    public sealed record Current(Channel Channel) : ColorValue {
        public Option<Condition> Condition { get; init; }
        public Option<Interaction> Interaction { get; init; }
    }

    public sealed record Data : ColorValue;

    public sealed record Mixed(ColorValue From, ColorValue To, Fraction Amount) : ColorValue {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public HueSpan Hue { get; init; } = HueSpan.Shorter;
    }

    public sealed record Contrasting(ColorValue Over, Seq<ColorValue> Candidates) : ColorValue;

    public sealed record Relative(ColorValue From, ColourSpace Space) : ColorValue {
        public Vector4 Scale { get; init; } = Vector4.One;
        public Vector4 Offset { get; init; } = Vector4.Zero;

        internal Unicolour Shifted(SKColorF origin) {
            Unicolour colour = origin.ToUnicolour();
            (double first, double second, double third) = colour.GetRepresentation(Space).Tuple;
            Vector4 shifted = Vector4.FusedMultiplyAdd(new Vector4(Defined(first), Defined(second), Defined(third), (float)colour.Alpha.A), Scale, Offset);
            return new Unicolour(Space, shifted.X, shifted.Y, shifted.Z, float.Clamp(shifted.W, 0f, 1f));
        }

        private static float Defined(double channel) => double.IsNaN(channel) ? 0f : (float)channel;
    }
}

public sealed record DataSeries(uint Ordinal) {
    public Option<ColorValue> Color { get; init; }
    public Option<double> Value { get; init; }
}

public sealed record Stop(ColorValue Color, Fraction At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ColorRamp {

    // --- [STOPS]

    private ColorRamp(Seq<Stop> stops) => Stops = toSeq(stops.OrderBy(static stop => stop.At));

    public Seq<Stop> Stops { get; private protected init; }

    private protected Seq<(Stop Low, Stop High)> Segments => toSeq(Stops.AsIterable().Window(2, static pair => (Low: pair[0], High: pair[1])));

    private protected Option<ColorValue> Last => Stops.Last.Map(static stop => stop.Color);

    public ColorRamp WithStops(Seq<Stop> stops) =>
        Switch(stops,
            smooth: static (held, smooth) => (ColorRamp)new Smooth(held) { Space = smooth.Space, Hue = smooth.Hue, Easing = smooth.Easing },
            stepped: static (held, _) => new Stepped(held),
            mapped: static (held, _) => new Smooth(held));

    public ColorRamp Banded(uint count) =>
        new Stepped(toSeq(Enumerable.Range(0, (int)count)).Choose(band =>
            At(new Fraction((band + 0.5f) / count)).Map(color => new Stop(color, new Fraction((float)band / count)))));

    // --- [SAMPLING]

    public abstract Option<ColorValue> At(Fraction at);

    internal virtual Option<ColorValue> Valued(double value) =>
        Fraction.Validated((float)value).Match(Succ: At, Fail: _ => (value < 0d ? Stops.Head : Stops.Last).Map(static stop => stop.Color));

    // --- [DENSITY]

    private const double JustNoticeable = 0.02d;

    internal virtual Option<Seq<(Fraction At, SKColorF Color)>> Dense(Func<ColorValue, Option<SKColorF>> resolve, ColourSpace blend) =>
        Subdivided(Stops.Map(static stop => stop.At), at => At(at).Bind(resolve), blend);

    private static Option<Seq<(Fraction At, SKColorF Color)>> Subdivided(Seq<Fraction> knots, Func<Fraction, Option<SKColorF>> color, ColourSpace blend) =>
        from resolved in knots.Traverse(at => color(at).Map(held => (At: at, Color: held))).As()
        from segments in toSeq(resolved.AsIterable().Window(2, static pair => (Low: pair[0], High: pair[1]))).Traverse(pair => Split(color, blend, pair.Low, pair.High).Map(inner => inner.Add(pair.High))).As()
        select resolved.Take(1) + segments.Bind(static inner => inner);

    private static Option<Seq<(Fraction At, SKColorF Color)>> Split(Func<Fraction, Option<SKColorF>> color, ColourSpace blend, (Fraction At, SKColorF Color) low, (Fraction At, SKColorF Color) high) =>
        low.At.Midpoint(high.At) switch {
            var at when at == low.At || at == high.At => Some(Seq<(Fraction At, SKColorF Color)>()),
            var at => color(at).Bind(middle =>
                low.Color.ToUnicolour().Mix(high.Color.ToUnicolour(), blend, premultiplyAlpha: false).Difference(middle.ToUnicolour(), DeltaE.Ok) <= JustNoticeable
                    ? Some(Seq<(Fraction At, SKColorF Color)>())
                    : from left in Split(color, blend, low, (at, middle))
                      from right in Split(color, blend, (at, middle), high)
                      select left.Add((at, middle)) + right),
        };

    // --- [CASES]

    public sealed record Smooth(Seq<Stop> Stops) : ColorRamp(Stops) {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public HueSpan Hue { get; init; } = HueSpan.Shorter;
        public Easing Easing { get; init; } = Easing.Linear;

        public override Option<ColorValue> At(Fraction at) =>
            Segments.Find(segment => at <= segment.High.At).Bind(segment => at <= segment.Low.At ? Some(segment.Low.Color) : Mixed(segment.Low, segment.High, at)) | Last;

        private Option<ColorValue> Mixed(Stop low, Stop high, Fraction at) =>
            from eased in Easing.At(new Fraction(((float)at - (float)low.At) / ((float)high.At - (float)low.At))).Map(static sampled => (float)sampled.Progress)
            select Fraction.Validated(eased).Match(
                Succ: amount => new ColorValue.Mixed(low.Color, high.Color, amount) { Space = Space, Hue = Hue },
                Fail: _ => eased < 0f ? low.Color : high.Color);
    }

    public sealed record Stepped(Seq<Stop> Stops) : ColorRamp(Stops) {
        public override Option<ColorValue> At(Fraction at) => Segments.Find(segment => at < segment.High.At).Map(static segment => segment.Low.Color) | Last;

        internal override Option<Seq<(Fraction At, SKColorF Color)>> Dense(Func<ColorValue, Option<SKColorF>> resolve, ColourSpace blend) =>
            Stops.Traverse(stop => resolve(stop.Color).Map(color => (From: stop.At, Color: color))).As().Map(static resolved =>
                resolved.Zip(resolved.Tail.Map(static held => held.From).Add(Fraction.MaxValue), static (held, until) => Seq((held.From, held.Color), (until, held.Color))).Bind(static pair => pair));
    }

    public sealed record Mapped(Colourmap Map) : ColorRamp(Sampled(Map)) {
        public Colourmap Map {
            get;
            init {
                field = value;
                Stops = Sampled(value);
            }
        } = Map;

        public Option<Unicolour> Under { get; init; }
        public Option<Unicolour> Over { get; init; }

        public override Option<ColorValue> At(Fraction at) => new ColorValue.Fixed(Map.Map((float)at));

        internal override Option<ColorValue> Valued(double value) => new ColorValue.Fixed(Map.MapWithClipping(value, Under.ValueUnsafe(), Over.ValueUnsafe()));

        private static Seq<Stop> Sampled(Colourmap map) =>
            Subdivided([Fraction.MinValue, Fraction.MaxValue], at => Some(map.Map((float)at).ToSKColorF()), ColourSpace.Rgb).ToSeq().Bind(static stops => stops)
                .Map(static stop => new Stop(new ColorValue.Fixed(stop.Color.ToUnicolour()), stop.At));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class UnicolourExtensions {
    extension(SKColorF color) {
        internal Unicolour ToUnicolour() => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);

        internal double Contrast(SKColorF ground) =>
            ground.WithAlpha(1f).ToUnicolour() switch {
                var opaque => color.ToUnicolour().Blend(opaque, BlendMode.Normal).Contrast(opaque),
            };
    }

    extension(Unicolour colour) {
        internal SKColorF ToSKColorF() =>
            colour.ConvertToConfiguration(Configuration.Default) switch {
                var standard => new((float)standard.Rgb.R, (float)standard.Rgb.G, (float)standard.Rgb.B, (float)standard.Alpha.A),
            };

        internal SKColorF Mapped(Configuration gamut) => colour.ConvertToConfiguration(gamut).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF();
    }
}
