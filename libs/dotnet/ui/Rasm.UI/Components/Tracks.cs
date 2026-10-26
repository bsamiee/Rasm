using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SoftBand {
    public float Low { get; }
    public float High { get; }
    public float SoftLow { get; }
    public float SoftHigh { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float low, ref float high, ref float softLow, ref float softHigh) =>
        validationError = float.IsFinite(softLow) && float.IsFinite(softHigh) && softLow <= low && low <= high && high <= softHigh
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"soft band ({softLow}, {low}, {high}, {softHigh}) is not finite and ordered"));
}

[SmartEnum]
public sealed partial class BandHandle {
    public static readonly BandHandle Band = new(None, static _ => new Node.Box { Role = Role.Handle }, static band => (band.Low + band.High) / 2f,
        static (band, by) => SoftBand.Create(band.Low + by, band.High + by, band.SoftLow + by, band.SoftHigh + by));
    public static readonly BandHandle SoftLow = new(None, static _ => new Node.Box { Role = Role.Stop }, static band => band.SoftLow,
        static (band, by) => SoftBand.Create(band.Low, band.High, float.Min(band.SoftLow + by, band.Low), band.SoftHigh));
    public static readonly BandHandle SoftHigh = new(None, static _ => new Node.Box { Role = Role.Stop }, static band => band.SoftHigh,
        static (band, by) => SoftBand.Create(band.Low, band.High, band.SoftLow, float.Max(band.SoftHigh + by, band.High)));
    public static readonly BandHandle Low = new(Some(SoftLow), Needle, static band => band.Low,
        static (band, by) => SoftBand.Create(float.Clamp(band.Low + by, band.SoftLow, band.High), band.High, band.SoftLow, band.SoftHigh));
    public static readonly BandHandle High = new(Some(SoftHigh), Needle, static band => band.High,
        static (band, by) => SoftBand.Create(band.Low, float.Clamp(band.High + by, band.Low, band.SoftHigh), band.SoftLow, band.SoftHigh));

    public Option<BandHandle> Alternate { get; }

    [UseDelegateFromConstructor]
    public partial Node Face(Orientation across);

    [UseDelegateFromConstructor]
    public partial float Position(SoftBand band);

    [UseDelegateFromConstructor]
    public partial SoftBand Moved(SoftBand band, float by);

    private static Node.Box Needle(Orientation across) =>
        across.Map(horizontal: new Node.Box { Role = Role.Needle, Width = Sizing.Grow.Unit }, vertical: new Node.Box { Role = Role.Needle, Height = Sizing.Grow.Unit });
}

public sealed record Slider(PlotPlane.Track Plane, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Option<float> Origin { get; init; } = Some(Plane.Axis.Range.From);
    public Option<float> Rest { get; init; }
    public Seq<Node> Ground { get; init; }
    public Func<float, string> Spoken { get; init; } = Plane.Axis.Spoken;

    public Node Drawn =>
        Plane.Grown(new Handles<float>(Value, Points<float>.Scalar(Plane, Lens<float, float>.New(static held => held, static shown => _ => shown), Spoken), Edited) { Rest = Rest }
            .Drawn(Plane, Plane.Grown(new Node.Box { Role = Role.Track }).Cons(Ground) + Spanned));

    private Seq<Node> Spanned =>
        Origin.Bind(origin => AxisRange.TryCreate(origin, Value, out AxisRange? span) ? Some<Node>(new Node.Mark(new Figure.Region(Some(span), None)) { Role = Role.Span }) : None).ToSeq();
}

public sealed record Dial(Axis.Periodic Axis, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Option<float> Rest { get; init; }
    public Func<float, string> Spoken { get; init; } = Axis.Spoken;

    private PlotPlane.Polar Plane => new(Axis, None);

    public Node Drawn =>
        new Handles<float>(Value, Points<float>.Fixed([Lens<float, Vector2>.New(static held => new Vector2(held, 1f), at => _ => Axis.Held(at.X))], at => Spoken(at.X)), Edited) { Rest = Rest }
            .Drawn(Plane, [
                new Node.Mark(new Figure.Region(None, None)) { Role = Role.Track },
                new Node.Mark(new Figure.Rule(new Vector2(Value, 0f), new Vector2(Value, 1f))) { Role = Role.Needle },
            ]) with {
            Width = Sizing.Grow.Unit,
            Aspect = Plane.Aspect,
        };
}

public sealed record Band(PlotPlane.Track Plane, SoftBand Value, Func<Edit<SoftBand>, IO<Unit>> Edited) {
    private static readonly Seq<(Key Key, BandHandle Handle)> Keyed = toSeq(BandHandle.Items).Map(static (handle, index) => ((Key)new Key.Ordinal((uint)index), handle));
    private static readonly HashMap<Key, BandHandle> Handled = toHashMap(Keyed);

    public Option<SoftBand> Rest { get; init; }
    public Seq<Node> Ground { get; init; }
    public Option<Node> Result { get; init; }
    public Func<float, string> Spoken { get; init; } = Plane.Axis.Spoken;

    public Node Drawn =>
        (Plane.Grown(new Handles<SoftBand>(Value, Edges, Edited) { Rest = Rest }
            .Drawn(Plane, Ground + Membership, Some<Func<Key, Vector2, Node>>((key, at) => new Node.Pin(at, Handled[key].Face(Plane.Along.Cross)) { Align = UnitPoint.Center }))) with {
            Role = Role.Well,
        }) switch {
            var bar => Result.Match(
                Some: result => Plane.Grown(new Node.Flex(Plane.Along.Cross, [bar, Plane.Grown(result) with { Role = Role.Well }])),
                None: () => bar),
        };

    private Points<SoftBand> Edges =>
        new(band => Keyed.Map(held => (held.Key, Plane.Along.With(new Vector2(0.5f), held.Handle.Position(band)))),
            (band, targets, modifiers) => (targets.Fold(band, (moved, target) => Handled[target.Key] switch {
                var dragged => (modifiers.Contains(Modifier.Alternate) ? dragged.Alternate.IfNone(dragged) : dragged).Moved(moved, Plane.Axis.Delta(dragged.Position(band), Plane.Along.Of(target.To))),
            }), targets.Map(static target => target.Key)),
            at => Spoken(Plane.Along.Of(at)));

    private Seq<Node> Membership =>
        Seq((From: Value.SoftLow, To: Value.Low, Role: Role.Highlight), (From: Value.Low, To: Value.High, Role: Role.Selection), (From: Value.High, To: Value.SoftHigh, Role: Role.Highlight))
            .Choose(static span => AxisRange.TryCreate(span.From, span.To, out AxisRange? range) ? Some<Node>(new Node.Mark(new Figure.Region(Some(range), None)) { Role = span.Role }) : None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class TrackExtensions {
    extension(PlotPlane.Track track) {
        internal Node Grown(Node node) =>
            track.Along.Map(horizontal: node with { Width = Sizing.Grow.Unit, Height = Sizing.Fit.Content }, vertical: node with { Width = Sizing.Fit.Content, Height = Sizing.Grow.Unit });
    }
}
