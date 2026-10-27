using System.Numerics;
using Rasm.UI.Plotting;
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
            held => held.Map(range => Seq(new Anchor(First, Centered(range.Low)), new Anchor(Last, Centered(range.High)))).IfNone([]),
            (held, targets, _) => held.Map(range => targets.Fold((range.Low, range.High), (ends, target) => target.Key == First ? (Binned(target.At), ends.High) : (ends.Low, Binned(target.At)))).Match(
                Some: ends => new Dragged<Option<IndexRange>>(Some(Spanning(ends.Low, ends.High)), ends.Low > ends.High ? targets.Map(static target => target.Key == First ? Last : First) : targets.Map(static target => target.Key)),
                None: () => new Dragged<Option<IndexRange>>(held, targets.Map(static target => target.Key)))) {
            Placed = (_, at, _) => Some(new Added<Option<IndexRange>>(Some(Spanning(Binned(at), Binned(at))), Last)),
            Removed = static (_, _) => Some(Option<IndexRange>.None),
        };
    }
}

public sealed record Chart(PlotPlane Plane, Seq<Node> Layers) {
    public Seq<Node.Pin> Labels { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

    public Node Drawn => Plotted(Layers);

    public Node Selecting(Option<IndexRange> selection, Func<Edit<Option<IndexRange>>, IO<Unit>> select) =>
        Plotted([
            new Handles<Option<IndexRange>>(selection, IndexRange.Ends(Plane), select) { GroundCursor = CursorShape.Crosshair }.Drawn(Plane, Marked(selection) + Layers) with {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            },
        ]);

    private Node.Plot Plotted(Seq<Node> children) =>
        new(Plane, children) {
            Role = Role.Well,
            Width = Sizing.Grow.Unit,
            Labels = Labels,
            Hovered = Hovered,
        };

    private static Seq<Node> Marked(Option<IndexRange> selection) =>
        selection.Bind(static range => AxisRange.Validated(range.Low, range.High + 1).ToOption()).Map(static span => (Node)new Node.Mark(new Figure.Region { First = Some(span) }) { Role = Role.Selection }).ToSeq();

    public Chart Layer(Fraction opacity, Seq<Node> children) =>
        this with { Layers = Layers.Add(new Node.Styled(new Node.Plot(Plane, children) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) { Opacity = opacity }) };

    public Chart Labelled(PhysicalAxis along, Fraction across, float least, Func<float, string> text) =>
        this with {
            Labels = Labels + Plane.Dimension(along).Map(axis => axis.Ticks(least).Map(tick => new Node.Pin(
                along.With(along.Cross.With(Vector2.Zero, Plane.Dimension(along.Cross).Map(cross => cross.Value((float)across)).IfNone((float)across)), tick),
                new Node.Text([text(tick)]) { Role = Role.Label }) {
                Align = along.Map(horizontal: new UnitPoint(Fraction.Half, across.Complement), vertical: new UnitPoint(across, Fraction.Half)),
            })).IfNone([]),
        };

    public static Option<Node.Mark> Area(ReadOnlySpan<int> counts, Axis count) {
        Seq<int> held = toSeq(counts.ToArray());
        return AxisRange.Validated(0.5f, held.Count - 0.5f).ToOption().Map(domain => new Node.Mark(new Figure.Area(domain, held.Map(value => float.Clamp(count.Fraction(value), 0f, 1f)).Strict())));
    }

    public static Node.Mark Rules(Axis ticks, PhysicalAxis along) => new(new Figure.Grid(along) { Ticks = Some(ticks) });
}

public sealed record GradientLabel(float At, string Label);

public sealed record GradientKey(ColorRamp Ramp, Seq<GradientLabel> Labels) {
    public Node Drawn =>
        new Node.Plot(PlotPlane.Track.Unit(PhysicalAxis.Horizontal), [
            new Node.Styled(new Node.Box { Role = Role.Well, Width = Sizing.Grow.Unit }) {
                Sheet = new StyleSheet { Looks = HashMap((Role.Well, new Look { Fills = Some(Seq<Material>(Ramp.Material)) })) },
            },
        ]) {
            Width = Sizing.Grow.Unit,
            Labels = Labels.Map(static label => new Node.Pin(new Vector2(label.At, 0f), new Node.Text([label.Label]) { Role = Role.Label }) { Align = new UnitPoint(Fraction.Half, Fraction.MinValue) }),
        };
}
