using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IndexRange {
    private static readonly Key First = new Key.Ordinal(0);
    private static readonly Key Last = new Key.Ordinal(1);

    private IndexRange(int low, int high) => (Low, High) = (low, high);

    public int Low { get; }
    public int High { get; }

    public static IndexRange Spanning(int one, int other) => new(int.Min(one, other), int.Max(one, other));

    public static Points<Option<IndexRange>> Ends(PlotPlane plane) {
        (PhysicalAxis along, Axis axis) = plane.Dimensions[0];
        (int first, int last) = ((int)MathF.Ceiling(axis.Domain.Low), (int)MathF.Floor(axis.Domain.High) - 1);
        Vector2 middle = plane.Value(new SKPoint(0.5f, 0.5f));
        int Binned(Vector2 at) => int.Clamp((int)MathF.Floor(along.Of(at)), first, last);
        Vector2 Centered(int index) => along.With(middle, index + 0.5f);
        return new Points<Option<IndexRange>>(
            held => held.Map(range => Seq((First, Centered(range.Low)), (Last, Centered(range.High)))).IfNone([]),
            (held, targets, _) => held.Map(range => targets.Fold((range.Low, range.High), (ends, target) => target.Key == First ? (Binned(target.To), ends.High) : (ends.Low, Binned(target.To)))).Match(
                Some: ends => (Some(Spanning(ends.Low, ends.High)), ends.Low > ends.High ? targets.Map(static target => target.Key == First ? Last : First) : targets.Map(static target => target.Key)),
                None: () => (held, targets.Map(static target => target.Key))),
            plane.Spoken) {
            Placed = Some<Func<Option<IndexRange>, Vector2, Option<Key>, Option<(Option<IndexRange> Value, Key Key)>>>((_, at, _) => Some((Some(Spanning(Binned(at), Binned(at))), Last))),
            Removed = Some<Func<Option<IndexRange>, Seq<Key>, Option<Option<IndexRange>>>>(static (_, _) => Some(Option<IndexRange>.None)),
        };
    }
}

public sealed record Chart(PlotPlane Plane, Seq<Node> Layers) {
    public Seq<Node.Pin> Labels { get; init; }
    public Option<IndexRange> Selection { get; init; }
    public Option<Func<Edit<Option<IndexRange>>, IO<Unit>>> Select { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

    public Node Drawn =>
        new Node.Plot(Plane, Select.Match(
            Some: select => Seq(new Handles<Option<IndexRange>>(Selection, IndexRange.Ends(Plane), select) { GroundCursor = CursorShape.Crosshair }.Drawn(Plane, Marked) with {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            }),
            None: () => Marked)) {
            Role = Role.Well,
            Width = Sizing.Grow.Unit,
            Labels = Labels,
            Hovered = Hovered,
        };

    private Seq<Node> Marked =>
        Selection.Bind(static range => AxisRange.TryCreate(range.Low, range.High + 1, out AxisRange? span) ? Some<Node>(new Node.Mark(new Figure.Region(Some(span), None)) { Role = Role.Selection }) : None)
            .ToSeq() + Layers;

    public Chart Layer(Fraction opacity, Seq<Node> children) =>
        this with { Layers = Layers.Add(new Node.Styled(new Node.Plot(Plane, children) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) { Opacity = opacity }) };

    public Chart Labelled(PhysicalAxis along, Fraction across, float least, Func<float, string> text) =>
        this with {
            Labels = Labels + Plane.Dimension(along).Map(axis => axis.Ticks(least).Map(tick => new Node.Pin(
                along.With(along.Cross.With(Vector2.Zero, Plane.Dimension(along.Cross).Map(cross => cross.Value((float)across)).IfNone((float)across)), tick),
                new Node.Text([text(tick)]) { Role = Role.Label }) {
                Align = along.Map(horizontal: UnitPoint.Create(Fraction.Half, across.Complement), vertical: UnitPoint.Create(across, Fraction.Half)),
            })).IfNone([]),
        };

    public static Option<Node.Mark> Area(ReadOnlySpan<int> counts, Axis count) =>
        AxisRange.TryCreate(0.5f, counts.Length - 0.5f, out AxisRange? domain)
            ? Some(new Node.Mark(new Figure.Area(domain, toSeq(counts.ToArray()).Map(held => float.Clamp(count.Fraction(held), 0f, 1f)).Strict())))
            : None;

    public static Node.Mark Rules(Axis ticks, PhysicalAxis along) => new(new Figure.Grid(along) { Ticks = Some(ticks) });
}

public sealed record GradientKey(ColorRamp Ramp, Seq<(float At, string Label)> Labels) {
    public Node Drawn =>
        new Node.Plot(PlotPlane.Track.Unit(PhysicalAxis.Horizontal), [
            new Node.Styled(new Node.Box { Role = Role.Well, Width = Sizing.Grow.Unit }) {
                Sheet = new StyleSheet { Looks = HashMap((Role.Well, new Look { Fills = Some(Seq<Material>(Ramp.Material)) })) },
            },
        ]) {
            Width = Sizing.Grow.Unit,
            Labels = Labels.Map(static label => new Node.Pin(new Vector2(label.At, 0f), new Node.Text([label.Label]) { Role = Role.Label }) { Align = UnitPoint.Create(Fraction.Half, Fraction.MinValue) }),
        };
}

public sealed record CurveEditor(PlotPlane.Cartesian Plane, Seq<Vector2> Points, Func<float, float> Fit, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public Seq<Func<float, float>> References { get; init; }
    public Option<Seq<float>> Underlay { get; init; }
    public Seq<Node> Ground { get; init; }
    public Series Series { get; init; } = new(0);
    public Selection Picked { get; init; } = new Selection.Many([], None, None);
    public Option<Func<Edit<Selection>, IO<Unit>>> Pick { get; init; }

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
        Points<Seq<Vector2>>.Indexed(Lens<Seq<Vector2>, Seq<Vector2>>.New(static held => held, static next => _ => toSeq(next.OrderBy(static point => point.X))), Plane.Spoken) switch {
            var indexed => indexed with {
                Placed = indexed.Placed.Map<Func<Seq<Vector2>, Vector2, Option<Key>, Option<(Seq<Vector2> Value, Key Key)>>>(place =>
                    (held, at, nearest) => place(held, at with { Y = Plane.Y.Held(Fit(at.X)) }, nearest)),
            },
        };
}
