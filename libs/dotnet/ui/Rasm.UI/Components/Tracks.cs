using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Slider(PhysicalAxis Along, AxisRange Domain, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
    public Option<AxisRange> Soft { get; init; }
    public Option<float> Step { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<string> Unit { get; init; }
    public CultureInfo Culture { get; init; } = CultureInfo.InvariantCulture;
    public Option<float> Origin { get; init; }
    public Option<float> Rest { get; init; }
    public Seq<Node> Ground { get; init; }

    public Node Drawn =>
        new PlotPlane.Track(Along, Placed) switch {
            var plane => plane.Grown(new Handles<float>(Value, Points<float>.Scalar(plane, Lens<float, float>.New(static held => held, shown => _ => Snapped(shown)), Spoken), Edited) { Rest = Rest }
                .Drawn(plane, plane.Grown(new Node.Box { Role = Role.Track }).Cons(Ground) + Spanned(plane))),
        };

    private Axis Placed {
        get {
            Axis soft = Track(Soft.IfNone(Domain));
            float position = soft.Fraction(Value);
            (float from, float to) = position switch {
                < 0f => (float.Max(soft.Value((2f * position) - 1f), Domain.Low), soft.Domain.To),
                > 1f => (soft.Domain.From, float.Min(soft.Value(2f * position), Domain.High)),
                _ => (soft.Domain.From, soft.Domain.To),
            };
            return position is >= 0f and <= 1f || !AxisRange.TryCreate(from, to, out AxisRange? range) ? soft : Track(range);
        }
    }

    private float Snapped(float shown) => float.Clamp(Step.Map(step => MathF.Round(shown / step, MidpointRounding.ToEven) * step).IfNone(shown), Domain.Low, Domain.High);

    private string Spoken(float value) =>
        (Decimals | Step.Map(static step => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(step))))).IfNone(() => Placed.Decimals) switch {
            var digits => value.ToString(string.Create(CultureInfo.InvariantCulture, $"F{digits}"), Culture) switch {
                var number => Unit.Match(Some: unit => $"{number} {unit}", None: () => number),
            },
        };

    private Seq<Node> Spanned(PlotPlane.Track plane) =>
        AxisRange.TryCreate(Origin.IfNone(plane.Axis.Domain.From), Value, out AxisRange? span) ? Seq<Node>(new Node.Mark(new Figure.Region(Some(span), None)) { Role = Role.Span }) : [];
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

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class TrackExtensions {
    extension(PlotPlane.Track track) {
        internal Node Grown(Node node) =>
            track.Along.Map(horizontal: node with { Width = Sizing.Grow.Unit, Height = Sizing.Fit.Content }, vertical: node with { Width = Sizing.Fit.Content, Height = Sizing.Grow.Unit });
    }
}
