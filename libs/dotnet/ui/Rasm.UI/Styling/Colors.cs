using SkiaSharp;
using SuperLinq;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record ColorValue {
    public sealed record Fixed(Unicolour Color) : ColorValue {
        public Fixed(string hex) : this(new Unicolour(hex)) { }
    }

    public sealed record Host(Channel Channel) : ColorValue {
        public Option<Role> Role { get; init; }
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
        public ChannelShift Alpha { get; init; } = ChannelShift.Identity;

        internal SKColorF Shifted(SKColorF origin, Configuration gamut) {
            Unicolour colour = origin.ToUnicolour();
            (double first, double second, double third) = colour.GetRepresentation(Space).Tuple;
            return new Unicolour(Space, First.Applied(Defined(first)), Second.Applied(Defined(second)), Third.Applied(Defined(third)), double.Clamp(Alpha.Applied(colour.Alpha.A), 0d, 1d)).Mapped(gamut);
        }

        private static double Defined(double channel) => double.IsNaN(channel) ? 0d : channel;
    }
}

public sealed record ChannelShift(double Scale, double Offset) {
    public static ChannelShift Identity { get; } = new(1d, 0d);

    internal double Applied(double kept) => Math.FusedMultiplyAdd(kept, Scale, Offset);
}

public sealed record DataSeries(uint Ordinal) {
    public Option<SKColorF> Color { get; init; }
    public Option<double> Value { get; init; }
}

public sealed record Stop<TColor>(TColor Color, Fraction At);

internal sealed record RampSegment(Stop<SKColorF> Low, Stop<SKColorF> High) {
    internal float Progress(Fraction at) => ((float)at - (float)Low.At) / ((float)High.At - (float)Low.At);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ColorRamp {
    // --- [STOPS]
    private ColorRamp(Seq<Stop<SKColorF>> stops) => Stops = toSeq(stops.OrderBy(static stop => stop.At));

    public Seq<Stop<SKColorF>> Stops { get; }

    private protected Seq<RampSegment> Segments => Paired(Stops);

    private protected Option<SKColorF> Last => Stops.Last.Map(static stop => stop.Color);

    public ColorRamp WithStops(Seq<Stop<SKColorF>> stops) =>
        Switch(stops,
            smooth: static (held, smooth) => (ColorRamp)new Smooth(held) { Space = smooth.Space, Easing = smooth.Easing },
            stepped: static (held, _) => new Stepped(held),
            mapped: static (held, _) => new Smooth(held));

    // --- [SAMPLING]
    public abstract Option<SKColorF> At(Fraction at);

    internal virtual Option<SKColorF> Valued(double value) =>
        Fraction.Validated((float)value).Match(Succ: At, Fail: _ => (value < 0d ? Stops.Head : Stops.Last).Map(static stop => stop.Color));

    // --- [DENSITY]
    private const double JustNoticeable = 0.02d;

    internal static Seq<Stop<SKColorF>> Dense(Seq<Stop<SKColorF>> stops, Func<RampSegment, Fraction, SKColorF> color) =>
        stops.Head.ToSeq() + Paired(stops).Bind(segment => Subdivided(at => color(segment, at), segment.Low, segment.High).Add(segment.High));

    private static Seq<RampSegment> Paired(Seq<Stop<SKColorF>> stops) => toSeq(stops.AsIterable().Window(2, static pair => new RampSegment(pair[0], pair[1])));

    private static Seq<Stop<SKColorF>> Subdivided(Func<Fraction, SKColorF> color, Stop<SKColorF> low, Stop<SKColorF> high) =>
        low.At.Midpoint(high.At) switch {
            var at when at == low.At || at == high.At => [],
            var at => new Stop<SKColorF>(color(at), at) switch {
                var middle when low.Color.ToUnicolour().Mix(high.Color.ToUnicolour(), ColourSpace.Rgb, premultiplyAlpha: false).Difference(middle.Color.ToUnicolour(), DeltaE.Ok) <= JustNoticeable => [],
                var middle => Subdivided(color, low, middle).Add(middle) + Subdivided(color, middle, high),
            },
        };

    // --- [CASES]
    public sealed record Smooth(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public Easing Easing { get; init; } = Easing.Linear;

        public override Option<SKColorF> At(Fraction at) =>
            Segments.Find(segment => at <= segment.High.At).Bind(segment => at <= segment.Low.At ? Some(segment.Low.Color) : Mixed(segment, at)) | Last;

        private Option<SKColorF> Mixed(RampSegment segment, Fraction at) =>
            Fraction.Validated(segment.Progress(at)).ToOption().Map(Easing.Progress)
                .Map(progress => segment.Low.Color.ToUnicolour().Mix(segment.High.Color.ToUnicolour(), Space, progress).Mapped(Configuration.Default));
    }

    public sealed record Stepped(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        public override Option<SKColorF> At(Fraction at) => Segments.Find(segment => at < segment.High.At).Map(static segment => segment.Low.Color) | Last;
    }

    public sealed record Mapped(Colourmap Map) : ColorRamp(Sampled(Map)) {
        public Option<Unicolour> Under { get; init; }
        public Option<Unicolour> Over { get; init; }

        public override Option<SKColorF> At(Fraction at) => Map.Map((float)at).ToSKColorF();

        internal override Option<SKColorF> Valued(double value) =>
            Map.MapWithClipping(value, Under.ValueUnsafe(), Over.ValueUnsafe()).ToSKColorF();

        private static Seq<Stop<SKColorF>> Sampled(Colourmap map) =>
            Dense([new Stop<SKColorF>(map.Map(0d).ToSKColorF(), Fraction.MinValue), new Stop<SKColorF>(map.Map(1d).ToSKColorF(), Fraction.MaxValue)], (_, at) => map.Map((float)at).ToSKColorF());
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class UnicolourExtensions {
    extension(SKColorF color) {
        internal Unicolour ToUnicolour() => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);
    }

    extension(Unicolour colour) {
        internal SKColorF ToSKColorF() =>
            colour.ConvertToConfiguration(Configuration.Default) switch {
                var standard => new((float)standard.Rgb.R, (float)standard.Rgb.G, (float)standard.Rgb.B, (float)standard.Alpha.A),
            };

        internal SKColorF Mapped(Configuration gamut) => colour.ConvertToConfiguration(gamut).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF();
    }
}
