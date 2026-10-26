using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColorRamp {
    private const double JustNoticeable = 0.02d;

    private ColorRamp(Seq<Stop<SKColorF>> stops) => Stops = toSeq(stops.OrderBy(static stop => (float)stop.At));

    public Seq<Stop<SKColorF>> Stops { get; }

    public Material.Gradient Material => new(Stopped.Map(static stop => new Stop<ColorValue>(new ColorValue.Fixed(stop.Color.ToUnicolour()), stop.At)), 0f);

    private protected abstract Seq<Stop<SKColorF>> Stopped { get; }

    private protected Seq<(Stop<SKColorF> Low, Stop<SKColorF> High)> Segments => Stops.Zip(Stops.Tail);

    private protected Option<SKColorF> Last => Stops.Last.Map(static stop => stop.Color);

    public abstract Option<SKColorF> At(Fraction at);

    public ColorRamp WithStops(Seq<Stop<SKColorF>> stops) =>
        Switch(stops, smooth: static (held, smooth) => (ColorRamp)new Smooth(held) { Space = smooth.Space, Easing = smooth.Easing }, stepped: static (held, _) => new Stepped(held));

    // --- [CASES]
    public sealed record Smooth(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public Easing Easing { get; init; } = Easing.Linear;

        private protected override Seq<Stop<SKColorF>> Stopped =>
            Stops.Head.ToSeq() + Segments.Bind(segment => Divided(at => Mixed(segment, at), (float)segment.Low.At, (float)segment.High.At)
                .Choose(at => Fraction.TryCreate(at, out Fraction? held) ? Some(new Stop<SKColorF>(Mixed(segment, at), held)) : None)
                .Add(segment.High));

        public override Option<SKColorF> At(Fraction at) => Segments.Find(segment => (float)at <= (float)segment.High.At).Map(segment => Mixed(segment, (float)at)) | Last;

        private SKColorF Mixed((Stop<SKColorF> Low, Stop<SKColorF> High) segment, float at) =>
            ((float)segment.High.At - (float)segment.Low.At) switch {
                var span => segment.Low.Color.ToUnicolour()
                    .Mix(segment.High.Color.ToUnicolour(), Space, Easing.Progress(span > 0f ? float.Clamp((at - (float)segment.Low.At) / span, 0f, 1f) : 1f))
                    .MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF(),
            };

        private static Seq<float> Divided(Func<float, SKColorF> color, float low, float high) =>
            ((low + high) / 2f) switch {
                var middle when middle <= low || middle >= high => [],
                var middle when color(low).ToUnicolour().Mix(color(high).ToUnicolour(), ColourSpace.Rgb, premultiplyAlpha: false).Difference(color(middle).ToUnicolour(), DeltaE.Ok) <= JustNoticeable => [],
                var middle => Divided(color, low, middle).Add(middle) + Divided(color, middle, high),
            };
    }

    public sealed record Stepped(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        private protected override Seq<Stop<SKColorF>> Stopped =>
            Stops.Zip(Stops.Tail.Map(static stop => stop.At).Add(Fraction.MaxValue)).Bind(static held => Seq(held.First, held.First with { At = held.Second }));

        public override Option<SKColorF> At(Fraction at) => Segments.Find(segment => (float)at < (float)segment.High.At).Map(static segment => segment.Low.Color) | Last;
    }
}
