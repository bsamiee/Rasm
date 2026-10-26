using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class IndexRange {
    private static readonly Key First = new Key.Ordinal(0);
    private static readonly Key Last = new Key.Ordinal(1);

    public int Low { get; }
    public int High { get; }

    public static Points<Option<IndexRange>> Ends(PlotPlane plane) {
        (Orientation along, Axis axis) = plane.Dimensions[0];
        (int first, int last) = ((int)MathF.Ceiling(axis.Range.Low), (int)MathF.Floor(axis.Range.High) - 1);
        Vector2 middle = plane.Value(new SKPoint(0.5f, 0.5f));
        int Binned(Vector2 at) => int.Clamp((int)MathF.Floor(along.Of(at)), first, last);
        Vector2 Centered(int index) => along.With(middle, index + 0.5f);
        return new Points<Option<IndexRange>>(
            held => held.Map(range => Seq((First, Centered(range.Low)), (Last, Centered(range.High)))).IfNone([]),
            (held, targets, _) => held.Map(range => targets.Fold((range.Low, range.High), (ends, target) => target.Key == First ? (Binned(target.To), ends.High) : (ends.Low, Binned(target.To)))).Match(
                Some: ends => (Some(Create(int.Min(ends.Low, ends.High), int.Max(ends.Low, ends.High))),
                    ends.Low > ends.High ? targets.Map(static target => target.Key == First ? Last : First) : targets.Map(static target => target.Key)),
                None: () => (held, targets.Map(static target => target.Key))),
            plane.Spoken) {
            Placed = Some<Func<Option<IndexRange>, Vector2, Option<Key>, Option<(Option<IndexRange> Value, Key Key)>>>((_, at, _) =>
                Binned(at) switch {
                    var bin => Some((Some(Create(bin, bin)), Last)),
                }),
            Removed = Some<Func<Option<IndexRange>, Seq<Key>, Option<Option<IndexRange>>>>(static (_, _) => Some(Option<IndexRange>.None)),
        };
    }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int low, ref int high) =>
        validationError = low <= high ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"index range ({low}, {high}) is not ordered"));
}

public sealed record Chart(PlotPlane Plane, Seq<Node> Layers) {
    public Seq<Node.Pin> Labels { get; init; }
    public Option<IndexRange> Selection { get; init; }
    public Option<Func<Edit<Option<IndexRange>>, IO<Unit>>> Select { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

    public Node Drawn =>
        new Node.Plot(Plane, Select.Match(
            Some: select => Seq(new Handles<Option<IndexRange>>(Selection, IndexRange.Ends(Plane), select) { GroundCursor = Cursor.Crosshair }.Drawn(Plane, Marked) with {
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
        Selection.Map(static range => (Node)new Node.Mark(new Figure.Region(Some(AxisRange.Create(range.Low, range.High + 1)), None)) { Role = Role.Selection }).ToSeq() + Layers;

    public Chart Layer(Fraction opacity, Seq<Node> children) =>
        this with { Layers = Layers.Add(new Node.Styled(new Node.Plot(Plane, children) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) { Opacity = opacity }) };

    public Chart Labelled(Orientation along, Fraction across, float least, Func<float, string> text) =>
        this with {
            Labels = Labels + Plane.Dimension(along).Map(axis => axis.Ticks(least).Map(tick => new Node.Pin(
                along.With(along.Cross.With(Vector2.Zero, Plane.Dimension(along.Cross).Map(cross => cross.Value((float)across)).IfNone((float)across)), tick),
                new Node.Text([text(tick)]) { Role = Role.Label }) {
                Align = along.Map(horizontal: UnitPoint.Create(0.5f, 1f - (float)across), vertical: UnitPoint.Create((float)across, 0.5f)),
            })).IfNone([]),
        };

    public static Node.Mark Area(ReadOnlySpan<int> counts, Axis count) =>
        new(new Figure.Area(AxisRange.Create(0.5f, counts.Length - 0.5f), toSeq(counts.ToArray()).Map(held => float.Clamp(count.Fraction(held), 0f, 1f)).Strict()));

    public static SKImage Density(ReadOnlySpan<int> grid, int columns, int rows) {
        float[] weights = new float[columns * rows];
        for (int row = 0; row < rows; row++)
            TensorPrimitives.ConvertTruncating(grid.Slice(row * columns, columns), weights.AsSpan((rows - 1 - row) * columns, columns));
        TensorPrimitives.Divide(weights, int.Max(1, TensorPrimitives.Max(grid)), weights);
        Half[] halves = new Half[weights.Length];
        TensorPrimitives.ConvertToHalf(weights, halves);
        return SKImage.FromPixelCopy(new SKImageInfo(columns, rows, SKColorType.AlphaF16, SKAlphaType.Premul), MemoryMarshal.AsBytes(halves.AsSpan()));
    }

    public static Node.Mark Rules(Axis ticks, Orientation along) => new(new Figure.Grid(along) { Ticks = Some(ticks) });
}

public sealed record GradientKey(ColorRamp Ramp, Seq<(float At, string Label)> Labels) {
    public Node Drawn =>
        new Node.Plot(PlotPlane.Track.Unit(Orientation.Horizontal), Checked.Add(new Node.Styled(new Node.Box { Role = Role.Well, Width = Sizing.Grow.Unit }) {
            Sheet = new StyleSheet { Looks = HashMap((Role.Well, new Look { Fills = Some(Seq<Material>(Ramp.Material)) })) },
        })) {
            Width = Sizing.Grow.Unit,
            Labels = Labels.Map(static label => new Node.Pin(new Vector2(label.At, 0f), new Node.Text([label.Label]) { Role = Role.Label }) { Align = UnitPoint.Create(0.5f, 0f) }),
        };

    private Seq<Node> Checked => Ramp.Stops.Exists(static stop => stop.Color.Alpha < 1f) ? [new Node.Box { Role = Role.Checker }] : [];
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
            + Underlay.Map(underlay => (Node)new Node.Mark(new Figure.Area(Plane.X.Range, underlay)) { Role = Role.Trace }).ToSeq()
            + Seq<Node>(new Node.Mark(new Figure.Grid(Orientation.Horizontal)) { Role = Role.Grid }, new Node.Mark(new Figure.Grid(Orientation.Vertical)) { Role = Role.Grid })
            + (Plane.X == Plane.Y ? Seq<Node>(new Node.Mark(new Figure.Rule(new Vector2(Plane.X.Range.From), new Vector2(Plane.X.Range.To))) { Role = Role.GridFine }) : [])
            + References.Map(reference => (Node)new Node.Mark(new Figure.Graph(Plane.X.Range, reference)) { Role = Role.Trace })
            + Seq<Node>(new Node.Mark(new Figure.Graph(Plane.X.Range, Fit)) { Role = Role.Series, Series = Series })) with {
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
