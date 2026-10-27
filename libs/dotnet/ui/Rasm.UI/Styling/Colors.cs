using SkiaSharp;
using SuperLinq;
using Wacton.Unicolour;

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

        internal SKColorF Shifted(SKColorF origin) {
            Unicolour colour = origin.ToUnicolour();
            (double first, double second, double third) = colour.GetRepresentation(Space).Tuple;
            return new Unicolour(Space, First.Applied(Defined(first)), Second.Applied(Defined(second)), Third.Applied(Defined(third)), double.Clamp(Alpha.Applied(colour.Alpha.A), 0d, 1d))
                .MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF();
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
}

public sealed record Stop<TColor>(TColor Color, Fraction At);

internal sealed record RampSegment(Stop<SKColorF> Low, Stop<SKColorF> High);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColorRamp {
    private const double JustNoticeable = 0.02d;

    private ColorRamp(Seq<Stop<SKColorF>> stops) => Stops = toSeq(stops.OrderBy(static stop => (float)stop.At));

    public Seq<Stop<SKColorF>> Stops { get; }

    public Material.Gradient Material => new(DenseStops.Map(static stop => new Stop<ColorValue>(new ColorValue.Fixed(stop.Color.ToUnicolour()), stop.At)), 0f);

    private protected abstract Seq<Stop<SKColorF>> DenseStops { get; }

    private protected Seq<RampSegment> Segments => toSeq(Stops.AsIterable().Window(2, static pair => new RampSegment(pair[0], pair[1])));

    private protected Option<SKColorF> Last => Stops.Last.Map(static stop => stop.Color);

    public abstract Option<SKColorF> At(Fraction at);

    public ColorRamp WithStops(Seq<Stop<SKColorF>> stops) =>
        Switch(stops, smooth: static (held, smooth) => (ColorRamp)new Smooth(held) { Space = smooth.Space, Easing = smooth.Easing }, stepped: static (held, _) => new Stepped(held));

    // --- [CASES]
    public sealed record Smooth(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public Easing Easing { get; init; } = Easing.Linear;

        private protected override Seq<Stop<SKColorF>> DenseStops =>
            Stops.Head.ToSeq() + Segments.Bind(segment => Subdivided(at => Mixed(segment, at), segment.Low, segment.High).Add(segment.High));

        public override Option<SKColorF> At(Fraction at) => Segments.Find(segment => (float)at <= (float)segment.High.At).Map(segment => Mixed(segment, at)) | Last;

        private SKColorF Mixed(RampSegment segment, Fraction at) {
            float span = (float)segment.High.At - (float)segment.Low.At;
            return segment.Low.Color.ToUnicolour()
                .Mix(segment.High.Color.ToUnicolour(), Space, Easing.Progress(span > 0f ? float.Clamp(((float)at - (float)segment.Low.At) / span, 0f, 1f) : 1f))
                .MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF();
        }

        private static Seq<Stop<SKColorF>> Subdivided(Func<Fraction, SKColorF> color, Stop<SKColorF> low, Stop<SKColorF> high) =>
            low.At.Midpoint(high.At) switch {
                var at when at == low.At || at == high.At => [],
                var at => new Stop<SKColorF>(color(at), at) switch {
                    var middle when low.Color.ToUnicolour().Mix(high.Color.ToUnicolour(), ColourSpace.Rgb, premultiplyAlpha: false).Difference(middle.Color.ToUnicolour(), DeltaE.Ok) <= JustNoticeable => [],
                    var middle => Subdivided(color, low, middle).Add(middle) + Subdivided(color, middle, high),
                },
            };
    }

    public sealed record Stepped(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        private protected override Seq<Stop<SKColorF>> DenseStops =>
            toSeq(Stops.AsIterable().Lead(1, static (stop, next) => Seq(stop, stop with { At = Optional(next).Map(static held => held.At).IfNone(Fraction.MaxValue) }))).Bind(static held => held);

        public override Option<SKColorF> At(Fraction at) => Segments.Find(segment => (float)at < (float)segment.High.At).Map(static segment => segment.Low.Color) | Last;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class UnicolourExtensions {
    extension(SKColorF color) {
        internal Unicolour ToUnicolour() => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);
    }

    extension(Unicolour colour) {
        internal SKColorF ToSKColorF() => new((float)colour.Rgb.R, (float)colour.Rgb.G, (float)colour.Rgb.B, (float)colour.Alpha.A);
    }
}
