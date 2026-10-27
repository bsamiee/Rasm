using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Swatch(SKColorF Color, Func<SKRect, IO<Unit>> Pressed) {
    public Node Drawn => new Part.Press(Pressed).Drawn(Face(Color));

    public static Node Face(SKColorF color) =>
        new Node.Styled(new Node.Box { Role = Role.Field, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) {
            Sheet = new StyleSheet { Looks = HashMap((Role.Field, new Look { Fills = Some(Seq<Material>(new Material.Solid(new ColorValue.Fixed(color.ToUnicolour())))) })) },
            Accessible = new Accessible { Value = new AccessValue.Text(color.ToUnicolour().Hex) },
        };
}

public sealed record SwatchList(Seq<SKColorF> Colors, Selection Picked, Func<Edit<Selection>, IO<Unit>> Pick) {
    private Seq<(Key Key, SKColorF Color)> Keyed => Colors.Map(static (color, index) => ((Key)new Key.Ordinal((uint)index), color));

    public Node Drawn =>
        toHashMap(Keyed) switch {
            var faces => new Part.Choice(Keyed.Map(static held => held.Key), Picked, Pick)
                .Drawn(key => Swatch.Face(faces[key]), static items => new Node.Flex(PhysicalAxis.Horizontal, items) { Width = Sizing.Grow.Unit }),
        };
}

public sealed record GradientEditor(ColorRamp Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) {
    private static readonly PlotPlane.Track Track = PlotPlane.Track.Unit(PhysicalAxis.Horizontal);

    public Selection Picked { get; init; } = new Selection.Many([]);
    public Func<Edit<Selection>, Option<IO<Unit>>> Pick { get; init; } = static _ => None;

    public Node Drawn =>
        new Handles<ColorRamp>(Value, Stops, Edited) {
            Picked = Picked,
            Pick = Pick,
            Handle = (key, at) => new Node.Pin(at, new Node.Box { Role = Role.Stop, Conditions = Picked.Holds(key) ? [Condition.Selected] : [] }) { Align = UnitPoint.Center },
        }.Drawn(Track, [new GradientKey(Value, []).Drawn]) with {
            Width = Sizing.Grow.Unit,
        };

    private static Points<ColorRamp> Stops { get; } =
        Points<ColorRamp>.Listed(
            static ramp => ramp.Stops,
            static stop => new Vector2((float)stop.At, 0.5f),
            static (stop, to) => Fraction.Validated(to.X).Map(at => stop with { At = at }).IfFail(stop),
            static (ramp, stops) => stops.IsEmpty ? None : Some(ramp.WithStops(stops)),
            static (ramp, at) =>
                from position in Fraction.Validated(at.X).ToOption()
                from color in ramp.At(position)
                select new Stop<SKColorF>(color, position));
}

public sealed record CurveEditor(PlotPlane.Cartesian Plane, Seq<Vector2> Points, Func<float, float> Fit, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public Seq<Func<float, float>> References { get; init; }
    public Option<Seq<float>> Underlay { get; init; }
    public Seq<Node> Ground { get; init; }
    public Series Series { get; init; } = new(0);
    public Selection Picked { get; init; } = new Selection.Many([]);
    public Func<Edit<Selection>, Option<IO<Unit>>> Pick { get; init; } = static _ => None;

    public Node Drawn =>
        new Handles<Seq<Vector2>>(Points, Knots, Edited) { Picked = Picked, Pick = Pick }.Drawn(Plane,
            Ground
            + Underlay.Map(underlay => (Node)new Node.Mark(new Figure.Area(Plane.X.Domain, underlay)) { Role = Role.Trace }).ToSeq()
            + Seq<Node>(new Node.Mark(new Figure.Grid(PhysicalAxis.Horizontal)) { Role = Role.Grid }, new Node.Mark(new Figure.Grid(PhysicalAxis.Vertical)) { Role = Role.Grid })
            + (Plane.X == Plane.Y ? Seq<Node>(new Node.Mark(new Figure.Rule(new Vector2(Plane.X.Domain.From), new Vector2(Plane.X.Domain.To))) { Role = Role.GridFine }) : [])
            + References.Map(reference => (Node)new Node.Mark(new Figure.Graph(Plane.X.Domain, reference)) { Role = Role.Trace })
            + Seq<Node>(new Node.Mark(new Figure.Graph(Plane.X.Domain, Fit)) { Role = Role.Series, Series = Series })) with {
            Role = Role.Well,
            Width = Sizing.Grow.Unit,
            Aspect = Factor.Unit,
        };

    private Points<Seq<Vector2>> Knots =>
        Points<Seq<Vector2>>.Listed(
            static knots => knots,
            static knot => knot,
            static (_, to) => to,
            static (_, knots) => Some(toSeq(knots.OrderBy(static knot => knot.X))),
            (_, at) => Some(at with { Y = Plane.Y.Held(Fit(at.X)) }));
}

public sealed record Slider(PhysicalAxis Along, AxisRange Domain, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
    public Option<AxisRange> Soft { get; init; }
    public Option<float> Step { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<string> Unit { get; init; }
    public Option<float> Origin { get; init; }
    public Option<float> Rest { get; init; }
    public Seq<Node> Ground { get; init; }

    public Node Drawn =>
        new PlotPlane.Track(Along, Placed) switch {
            var plane => plane.Grown(new Handles<float>(Value, Points<float>.Scalar(plane, Lens<float, float>.New(static held => held, shown => _ => Snapped(shown))), Edited) {
                Rest = Rest,
                Decimals = Decimals | Step.Map(static step => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(step)))),
                Unit = Unit,
            }.Drawn(plane, plane.Grown(new Node.Box { Role = Role.Track }).Cons(Ground) + Spanned(plane))),
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
            return Fraction.Validated(position).Match(Succ: _ => soft, Fail: _ => AxisRange.Validated(from, to).Map(Track).IfFail(soft));
        }
    }

    private float Snapped(float shown) => float.Clamp(Step.Map(step => MathF.Round(shown / step, MidpointRounding.ToEven) * step).IfNone(shown), Domain.Low, Domain.High);

    private Seq<Node> Spanned(PlotPlane.Track plane) =>
        AxisRange.Validated(Origin.IfNone(plane.Axis.Domain.From), Value).ToOption().Map(static span => (Node)new Node.Mark(new Figure.Region { First = Some(span) }) { Role = Role.Span }).ToSeq();
}

public sealed record Dial(Axis.Periodic Axis, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Option<float> Rest { get; init; }

    private PlotPlane.Polar Plane => new(Axis, None);

    public Node Drawn =>
        new Handles<float>(Value, Points<float>.Fixed([Lens<float, Vector2>.New(static held => new Vector2(held, 1f), at => _ => Axis.Held(at.X))]), Edited) { Rest = Rest }
            .Drawn(Plane, [
                new Node.Mark(new Figure.Region()) { Role = Role.Track },
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
