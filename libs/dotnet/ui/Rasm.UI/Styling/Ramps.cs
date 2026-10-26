using System.Globalization;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record ColorRamp {
    private const double JustNoticeable = 0.02d;

    private ColorRamp(Seq<Stop<SKColorF>> stops) =>
        Stops = stops.IsEmpty ? throw new ArgumentException("a colour ramp holds at least one stop", nameof(stops)) : toSeq(stops.OrderBy(static stop => (float)stop.At));

    public Seq<Stop<SKColorF>> Stops { get; }

    public Material.Gradient Material => new(Stopped.Map(static stop => new Stop<Ink>(new Ink.Fixed(stop.Color), stop.At)), 0f);

    private protected abstract Seq<Stop<SKColorF>> Stopped { get; }

    private Seq<(Stop<SKColorF> Low, Stop<SKColorF> High)> Segments => Stops.Zip(Stops.Tail);

    public abstract SKColorF At(Fraction at);

    public ColorRamp WithStops(Seq<Stop<SKColorF>> stops) =>
        Switch(stops, smooth: static (held, smooth) => (ColorRamp)new Smooth(held) { Space = smooth.Space, Easing = smooth.Easing }, stepped: static (held, _) => new Stepped(held));

    internal static Seq<float> Divided(Func<float, SKColorF> color, float low, float high) =>
        ((low + high) / 2f) switch {
            var middle when middle <= low || middle >= high => [],
            var middle when color(low).ToUnicolour().Mix(color(high).ToUnicolour(), ColourSpace.Rgb, premultiplyAlpha: false).Difference(color(middle).ToUnicolour(), DeltaE.Ok) <= JustNoticeable => [],
            var middle => Divided(color, low, middle).Add(middle) + Divided(color, middle, high),
        };

    // --- [WIRE]
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out ColorRamp? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : Read(value.Trim()).Match<(ColorRamp?, ValidationError?)>(
                Succ: static ramp => (ramp, null),
                Fail: failure => (null, new ValidationError($"colour ramp '{value}' is neither smooth(in <space> cubic-bezier(<x1>, <y1>, <x2>, <y2>), <ink> <at>, …) nor stepped(<ink> <at>, …): {failure.Message}")));
        return fault;
    }

    public string ToValue() =>
        Switch(
            smooth: static smooth => $"smooth(in {smooth.Space.Key} cubic-bezier({Ink.Written(smooth.Easing.X1)}, {Ink.Written(smooth.Easing.Y1)}, {Ink.Written(smooth.Easing.X2)}, {Ink.Written(smooth.Easing.Y2)}), {smooth.Listed})",
            stepped: static stepped => $"stepped({stepped.Listed})");

    private string Listed => string.Join(", ", Stops.Map(static stop => $"{new Ink.Fixed(stop.Color).ToValue()} {Ink.Written((float)stop.At)}"));

    private static Fin<ColorRamp> Read(string text) =>
        Ink.Called(text).ToFin(Errors.ParseError("no ramp function call")).Bind(static call => (call.Name, Ink.Split(call.Body, static letter => letter is ',')) switch {
            ("smooth", { Length: > 0 } stops) =>
                (Shaped(stops[0]), Listing(stops[1..])).Apply(static (shaped, held) => (ColorRamp)new Smooth(held) { Space = shaped.Space, Easing = shaped.Easing }).As(),
            ("stepped", var stops) => Listing(stops).Map(static held => (ColorRamp)new Stepped(held)),
            (var name, _) => Errors.ParseError($"{name}() is no ramp function"),
        });

    private static Fin<(CssSpace Space, Easing Easing)> Shaped(string shape) =>
        Ink.Split(shape, char.IsWhiteSpace) switch {
            ["in", var space, var curve] =>
                from held in CssSpace.Validated(space)
                from call in Ink.Called(curve).Filter(static call => call.Name is "cubic-bezier").ToFin(Errors.ParseError($"'{curve}' is no cubic-bezier()"))
                from points in toSeq(Ink.Split(call.Body, static letter => letter is ',')).Traverse(static point => Invariant.Number<double>(point).ToFin(Errors.ParseError($"'{point}' is no number"))).As()
                from easing in points is [var x1, var y1, var x2, var y2]
                    ? ValidationResults.ToFin(Easing.Validate(x1, y1, x2, y2, out Easing? made), made)
                    : Errors.ParseError($"cubic-bezier() takes four points, not {points.Count}")
                select (held, easing),
            _ => Errors.ParseError($"'{shape}' is no 'in <space> cubic-bezier(…)'"),
        };

    private static Fin<Seq<Stop<SKColorF>>> Listing(string[] stops) =>
        stops is []
            ? Errors.ParseError("a colour ramp holds at least one stop")
            : toSeq(stops).Traverse(static stop => Ink.Split(stop, char.IsWhiteSpace) switch {
                [var ink, var at] =>
                    from color in Ink.Fixed.Parsed(ink)
                    from position in Invariant.Number<float>(at).ToFin(Errors.ParseError($"'{at}' is no number")).Bind(Fraction.Validated)
                    select new Stop<SKColorF>(color, position),
                _ => Errors.ParseError($"'{stop}' is no '<ink> <at>' stop"),
            }).As();

    // --- [CASES]
    public sealed record Smooth(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        public CssSpace Space { get; init; } = CssSpace.Oklab;
        public Easing Easing { get; init; } = Easing.Linear;

        private protected override Seq<Stop<SKColorF>> Stopped =>
            Stops[0].Cons(Segments.Bind(segment => Divided(Mixed, (float)segment.Low.At, (float)segment.High.At).Map(at => new Stop<SKColorF>(Mixed(at), Fraction.Create(at))).Add(segment.High)));

        public override SKColorF At(Fraction at) => Mixed((float)at);

        private SKColorF Mixed(float at) =>
            Segments.Find(segment => at <= (float)segment.High.At).Match(
                Some: segment => ((float)segment.High.At - (float)segment.Low.At) switch {
                    var span => segment.Low.Color.ToUnicolour()
                        .Mix(segment.High.Color.ToUnicolour(), Space.Space, Easing.Progress(span > 0f ? float.Clamp((at - (float)segment.Low.At) / span, 0f, 1f) : 1f))
                        .MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF(),
                },
                None: () => Stops[Stops.Count - 1].Color);
    }

    public sealed record Stepped(Seq<Stop<SKColorF>> Stops) : ColorRamp(Stops) {
        private protected override Seq<Stop<SKColorF>> Stopped =>
            Stops.Zip(Stops.Tail.Map(static stop => stop.At).Add(Fraction.MaxValue)).Bind(static held => Seq(held.First, held.First with { At = held.Second }));

        public override SKColorF At(Fraction at) => Segments.Find(segment => (float)at < (float)segment.High.At).Map(static segment => segment.Low.Color).IfNone(() => Stops[Stops.Count - 1].Color);
    }
}
