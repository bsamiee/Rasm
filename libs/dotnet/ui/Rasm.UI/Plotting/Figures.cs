using System.Numerics;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Interpolation;
using Rasm.UI.Rendering;
using RBush;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Pen(float Stroke, float Spacing) {
    private readonly Option<SKMatrix> inverse = Some(SKMatrix.Identity);

    public static Pen Hit { get; } = new(0f, 0f);

    public float Icon { get; init; }
    public SKMatrix Device { get; init { field = value; inverse = value.TryInvert(out SKMatrix inverted) ? Some(inverted) : None; } } = SKMatrix.Identity;

    internal float Density => Device.MapRadius(1f);

    internal SKPoint Snapped(SKPoint local) {
        SKPoint device = Device.MapPoint(local);
        return Local(new SKPoint(Grid(device.X, 0f), Grid(device.Y, 0f)), local);
    }

    internal Line Snapped(SKPoint from, SKPoint to) =>
        (Start: Device.MapPoint(from), End: Device.MapPoint(to), Half: MathF.Round(Device.MapRadius(Stroke), MidpointRounding.ToEven) / 2f) switch {
            var held when held.Start.X.CompareTo(held.End.X) is 0 =>
                new(Local(held.Start with { X = Grid(held.Start.X, held.Half) }, from), Local(held.End with { X = Grid(held.End.X, held.Half) }, to)),
            var held when held.Start.Y.CompareTo(held.End.Y) is 0 =>
                new(Local(held.Start with { Y = Grid(held.Start.Y, held.Half) }, from), Local(held.End with { Y = Grid(held.End.Y, held.Half) }, to)),
            _ => new(from, to),
        };

    private static float Grid(float device, float offset) => MathF.Round(device - offset, MidpointRounding.ToEven) + offset;

    private SKPoint Local(SKPoint device, SKPoint local) => inverse.Map(held => held.MapPoint(device)).IfNone(local);
}

internal sealed record Line(SKPoint From, SKPoint To);

[SmartEnum]
public sealed partial class Interpolation {
    public static readonly Interpolation Linear = new(static (knots, _) => knots);
    public static readonly Interpolation Monotone = new(static (knots, density) => Splined(knots, density, CubicSpline.InterpolatePchipSorted));
    public static readonly Interpolation Natural = new(static (knots, density) => Splined(knots, density, CubicSpline.InterpolateNaturalSorted));
    public static readonly Interpolation BumpX = new(static (knots, density) => Bumped(knots, density, PhysicalAxis.Horizontal));
    public static readonly Interpolation BumpY = new(static (knots, density) => Bumped(knots, density, PhysicalAxis.Vertical));

    [UseDelegateFromConstructor]
    internal partial Seq<SKPoint> Traced(Seq<SKPoint> knots, float density);

    private static Seq<SKPoint> Splined(Seq<SKPoint> knots, float density, Func<double[], double[], CubicSpline> spline) {
        Seq<SKPoint> Curved() {
            double[] at = [.. Enumerable.Range(0, knots.Count).Select(static index => (double)index)];
            (CubicSpline across, CubicSpline up) = (spline(at, [.. knots.Map(static knot => (double)knot.X)]), spline(at, [.. knots.Map(static knot => (double)knot.Y)]));
            return Tabulated(knots, density, (index, step) => new SKPoint((float)across.Interpolate(index + step), (float)up.Interpolate(index + step)));
        }
        return knots.Count < 3 ? knots : Curved();
    }

    private static Seq<SKPoint> Bumped(Seq<SKPoint> knots, float density, PhysicalAxis along) =>
        knots.Count < 2 ? knots : Tabulated(knots, density, (index, step) => {
            (Vector2 from, Vector2 to) = ((Vector2)knots[index], (Vector2)knots[index + 1]);
            float middle = (along.Of(from) + along.Of(to)) / 2f;
            float rest = 1f - step;
            return (SKPoint)((rest * rest * rest * from) + (3f * rest * rest * step * along.With(from, middle)) + (3f * rest * step * step * along.With(to, middle)) + (step * step * step * to));
        });

    private static Seq<SKPoint> Tabulated(Seq<SKPoint> knots, float density, Func<int, float, SKPoint> at) =>
        toSeq(from index in Enumerable.Range(0, knots.Count - 1)
              let steps = int.Max(1, (int)MathF.Ceiling(SKPoint.Distance(knots[index], knots[index + 1]) * density))
              from step in Enumerable.Range(0, steps)
              select at(index, step / (float)steps)).Add(knots[knots.Count - 1]);
}

public sealed record Marker(GlyphSource Art, float Size);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Floor {
    public sealed record Baseline(float Value) : Floor;

    public sealed record Lower(Samples Samples) : Floor;
}

internal sealed record Stamp(Marker Marker, SKPoint At, float Degrees);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Figure {
    public abstract void Edges(SKPathBuilder path, PlotFrame frame, Pen pen);

    public virtual void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) { }

    public virtual bool Hits(PlotFrame frame, SKPoint local, float reach) {
        using SKPath interior = Outline.Built(path => Interior(path, frame, Pen.Hit));
        return interior.Contains(local.X, local.Y);
    }

    public virtual Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => None;

    public Option<Figure> Toward(Figure to, float progress) =>
        Switch<(Figure To, float Progress), Option<Figure>>(
            (to, progress),
            rule: static (held, source) => held.To is Rule other ? Some<Figure>(new Rule(Vector2.Lerp(source.From, other.From, held.Progress), Vector2.Lerp(source.To, other.To, held.Progress))) : None,
            path: static (held, source) => held.To is Path other && source.Vertices.Count == other.Vertices.Count
                ? Some<Figure>(other with { Vertices = Lerped(source.Vertices, other.Vertices, held.Progress), Widths = Blended(source.Widths, other.Widths, (one, two) => one.Toward(two, held.Progress)) })
                : None,
            region: static (held, source) => held.To is Region other
                ? Some<Figure>(other with { First = Blended(source.First, other.First, (one, two) => one.Toward(two, held.Progress)), Second = Blended(source.Second, other.Second, (one, two) => one.Toward(two, held.Progress)) })
                : None,
            curve: static (held, source) => held.To is Curve other
                ? from domain in source.Domain.Toward(other.Domain, held.Progress)
                  from samples in source.Samples.Toward(other.Samples, held.Progress)
                  select (Figure)(other with { Domain = domain, Samples = samples })
                : None,
            graph: static (held, source) => held.To is Graph other
                ? source.Domain.Toward(other.Domain, held.Progress).Map(domain => (Figure)(other with { Domain = domain, Function = at => float.Lerp(source.Function(at), other.Function(at), held.Progress) }))
                : None,
            grid: static (held, _) => held.To is Grid other ? Some<Figure>(other) : None,
            points: static (held, source) => held.To is Points other && source.At.Count == other.At.Count
                ? Some<Figure>(other with {
                    At = Lerped(source.At, other.At, held.Progress),
                    Values = Blended(source.Values, other.Values, (one, two) => one.Toward(two, held.Progress)),
                    Sizes = Blended(source.Sizes, other.Sizes, (one, two) => one.Toward(two, held.Progress)),
                    Opacity = Blended(source.Opacity, other.Opacity, (one, two) => one.Toward(two, held.Progress)),
                })
                : None,
            bars: static (held, source) => held.To is Bars other
                ? from domain in source.Domain.Toward(other.Domain, held.Progress)
                  from values in source.Values.Toward(other.Values, held.Progress)
                  select (Figure)(other with { Domain = domain, Values = values, Baseline = float.Lerp(source.Baseline, other.Baseline, held.Progress), Bases = Blended(source.Bases, other.Bases, (one, two) => one.Toward(two, held.Progress)) })
                : None,
            box: static (held, source) => held.To is Box other
                ? source.Summary.Toward(other.Summary, held.Progress).Map(summary => (Figure)(other with { At = float.Lerp(source.At, other.At, held.Progress), Summary = summary }))
                : None,
            field: static (held, source) => held.To is Field other && source.Columns == other.Columns && source.Rows == other.Rows
                ? source.Values.Toward(other.Values, held.Progress).Bind(values => Field.Validated(other.Columns, other.Rows, values).ToOption()).Map(static field => (Figure)field)
                : None,
            label: static (held, source) => held.To is Label other ? source.Along.Toward(other.Along, held.Progress).Map(along => (Figure)(other with { Along = along })) : None);

    internal virtual float Weight(PlotFrame frame, Pen pen) => 1f;

    internal virtual Seq<Stamp> Stamps(PlotFrame frame, Pen pen) => [];

    private static Seq<Vector2> Lerped(Seq<Vector2> from, Seq<Vector2> to, float progress) => toSeq(from.Zip(to, (one, two) => Vector2.Lerp(one, two, progress)));

    private static Option<T> Blended<T>(Option<T> from, Option<T> to, Func<T, T, Option<T>> toward) => to.Map(target => from.Bind(source => toward(source, target)).IfNone(target));

    private static float Reach(Seq<SKPoint> line, SKPoint at) =>
        line switch {
            { IsEmpty: true } => float.PositiveInfinity,
            { Count: 1 } => SKPoint.Distance(line[0], at),
            _ => line.Window(2).Min(pair => Segment(pair[0], pair[1], at)),
        };

    private static float Segment(SKPoint from, SKPoint to, SKPoint at) {
        Vector2 along = (Vector2)to - (Vector2)from;
        Vector2 offset = (Vector2)at - (Vector2)from;
        return along.LengthSquared() is 0f ? offset.Length() : (offset - (along * float.Clamp(Vector2.Dot(offset, along) / along.LengthSquared(), 0f, 1f))).Length();
    }

    private static float Bandwidth(PlotFrame frame) => frame.Plane.Dimension(PhysicalAxis.Horizontal).Map(static axis => axis.Bandwidth).IfNone(1f);

    private static void Ranged(SKPathBuilder path, PlotFrame frame, Pen pen, Fin<AxisRange> across, Fin<AxisRange> up) =>
        _ = (across, up).Apply(static (x, y) => new Region { First = Some(x), Second = Some(y) }).As().Iter(region => region.Interior(path, frame, pen));

    private static float Distance(SKPoint point, SKPoint local, Option<PhysicalAxis> along) =>
        along.Match(Some: axis => MathF.Abs(axis.Of((Vector2)point - (Vector2)local)), None: () => SKPoint.Distance(point, local));

    public sealed record Rule(Vector2 From, Vector2 To) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) {
            (SKPoint from, SKPoint to) = pen.Snapped(frame.Point(From), frame.Point(To));
            path.MoveTo(from);
            path.LineTo(to);
        }

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => Segment(frame.Point(From), frame.Point(To), local) <= reach;
    }

    public sealed record Path(Seq<Vector2> Vertices) : Figure {
        public bool Closed { get; init; }
        public Interpolation Interpolation { get; init; } = Interpolation.Linear;
        public Option<Marker> Start { get; init; }
        public Option<Marker> End { get; init; }
        public Option<Samples> Widths { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => path.AddPoly([.. Traced(Vertices.Map(frame.Point), pen)], Closed);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = (Widths.Filter(widths => widths.Values.Length == Vertices.Count).Map(widths => Ribbon(frame, pen, widths)) | (Closed ? Some(Traced(Vertices.Map(frame.Point), pen)) : None))
                .Iter(outline => path.AddPoly([.. outline], close: true));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => base.Hits(frame, local, reach) || Reach(Traced(Vertices.Map(frame.Point), Pen.Hit), local) <= reach;

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            Vertices.Map((at, index) => (Datum: new Datum(index, at), Distance: Distance(frame.Point(at), local, along))) switch {
                { IsEmpty: true } => None,
                var near => Some(near.MinBy(static held => held.Distance).Datum),
            };

        internal override Seq<Stamp> Stamps(PlotFrame frame, Pen pen) {
            using SKPath edge = Outline.Built(path => Edges(path, frame, pen));
            using SKPathMeasure measure = new(edge, forceClosed: false);
            Seq<Stamp> Placed(Option<Marker> marker, float distance, float turn) =>
                marker.Bind(held => measure.GetPositionAndTangent(distance, out SKPoint at, out SKPoint tangent) ? Some(new Stamp(held, at, float.RadiansToDegrees(MathF.Atan2(tangent.Y, tangent.X)) + turn)) : None).ToSeq();
            return Placed(Start, 0f, 180f) + Placed(End, measure.Length, 0f);
        }

        private Seq<SKPoint> Traced(Seq<SKPoint> knots, Pen pen) => Interpolation.Traced(knots, pen.Density);

        private Seq<SKPoint> Ribbon(PlotFrame frame, Pen pen, Samples widths) {
            Seq<Vector2> knots = Vertices.Map(at => (Vector2)frame.Point(at));
            Seq<Vector2> normals = knots.Map((_, index) => Vector2.Normalize(knots[int.Min(index + 1, knots.Count - 1)] - knots[int.Max(index - 1, 0)]) switch { var tangent => new Vector2(-tangent.Y, tangent.X) * (widths.Values.Span[index] / 2f) });
            return Traced(toSeq(knots.Zip(normals, static (at, normal) => (SKPoint)(at + normal))), pen) + Traced(toSeq(knots.Zip(normals, static (at, normal) => (SKPoint)(at - normal))).Rev(), pen);
        }
    }

    public sealed record Region : Figure {
        public Option<AxisRange> First { get; init; }
        public Option<AxisRange> Second { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Interior(path, frame, pen);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            frame.Plane.Enclose(path, frame.Matrix, pen, frame.Plane.Spans(PhysicalAxis.Horizontal, First), frame.Plane.Spans(PhysicalAxis.Vertical, Second));
    }

    public sealed record Curve(AxisRange Domain, Samples Samples) : Figure {
        public Option<Fraction> Step { get; init; }
        public Option<Floor> Fill { get; init; }
        public Interpolation Interpolation { get; init; } = Interpolation.Linear;

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => _ = Shaped(frame, pen, Runs(frame, pen, Samples)).Iter(line => path.AddPoly([.. line], close: false));

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = Fill.Iter(floor => floor.Switch(
                (Curve: this, Frame: frame, Pen: pen, Upper: Runs(frame, pen, Samples)),
                baseline: static (held, baseline) => held.Frame.Plane.Fraction(PhysicalAxis.Vertical, baseline.Value) switch {
                    var level => toSeq(held.Upper.Zip(held.Curve.Shaped(held.Frame, held.Pen, held.Upper), (run, line) => line + Seq(new Vector2(run[run.Count - 1].X, level), new Vector2(run[0].X, level)).Map(held.Frame.Projected))),
                },
                lower: static (held, lower) => toSeq(held.Curve.Shaped(held.Frame, held.Pen, held.Upper).Zip(held.Curve.Shaped(held.Frame, held.Pen, held.Curve.Runs(held.Frame, held.Pen, lower.Samples)), static (upper, under) => upper + under.Rev())))
                .Iter(outline => path.AddPoly([.. outline], close: true)));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) =>
            (Fill.IsSome && base.Hits(frame, local, reach)) || Shaped(frame, Pen.Hit, Runs(frame, Pen.Hit, Samples)).Exists(line => Reach(line, local) <= reach);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => frame.At(local).Bind(at => Samples.Nearest(Domain, at.X));

        private Seq<Seq<Vector2>> Runs(PlotFrame frame, Pen pen, Samples samples) =>
            samples.Decimated(frame, Domain, Columns(frame, pen)).Map(run => Step.Match(
                Some: step => toSeq(run.Window(2).SelectMany(pair => float.Lerp(pair[0].X, pair[1].X, (float)step) switch { var jump => Seq(pair[0], pair[0] with { X = jump }, pair[1] with { X = jump }) })).Add(run[run.Count - 1]),
                None: () => run));

        private Seq<Seq<SKPoint>> Shaped(PlotFrame frame, Pen pen, Seq<Seq<Vector2>> runs) =>
            (Step.IsNone && Samples.Values.Length <= Columns(frame, pen) ? Interpolation : Interpolation.Linear) switch {
                var shaping => runs.Map(run => shaping.Traced(run.Map(frame.Projected), pen.Density)),
            };

        private static float Columns(PlotFrame frame, Pen pen) => frame.Pixels(PhysicalAxis.Horizontal) * pen.Density;
    }

    public sealed record Graph(AxisRange Domain, Func<float, float> Function) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Tabulated(frame, pen).Edges(path, frame, pen);

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => Tabulated(frame, Pen.Hit).Hits(frame, local, reach);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            frame.At(local).Map(at => new Datum(0, new Vector2(at.X, Function(at.X)))).Filter(static datum => float.IsFinite(datum.Value.Y));

        private Curve Tabulated(PlotFrame frame, Pen pen) {
            float spanned = MathF.Abs(frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.To) - frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.From));
            int count = int.Max(2, (int)MathF.Ceiling(spanned * frame.Pixels(PhysicalAxis.Horizontal) * pen.Density) + 1);
            return new Curve(Domain, new Samples(Enumerable.Range(0, count).Select(index => Function(Domain.From + (index * Domain.Span / (count - 1)))).ToArray()));
        }
    }

    public sealed record Grid(PhysicalAxis Along) : Figure {
        public bool Major { get; init; } = true;

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = frame.Plane.Dimension(Along).Iter(axis => axis.Ticks(pen.Spacing / frame.Pixels(Along)).Filter(tick => tick.Major == Major).Iter(tick => frame.Plane.GridLine(path, frame.Matrix, pen, Along, axis.Fraction(tick.Value))));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => false;

        internal override float Weight(PlotFrame frame, Pen pen) =>
            Major ? 1f : frame.Plane.Dimension(Along).Map(axis => axis.Ticks(pen.Spacing / frame.Pixels(Along)).Map(tick => axis.Fraction(tick.Value)).Window(2)
                .Select(pair => MathF.Abs(pair[1] - pair[0]) * frame.Pixels(Along)).DefaultIfEmpty(0f).Min())
                .Map(spacing => float.Clamp((spacing / pen.Spacing) - 1f, 0f, 1f)).IfNone(0f);
    }

    public sealed record Points(Seq<Vector2> At) : Figure {
        private static readonly ConditionalWeakTable<Points, RBush<Indexed>> Trees = [];

        public Option<Samples> Values { get; init; }
        public Option<Samples> Sizes { get; init; }
        public Option<Samples> Opacity { get; init; }
        public Option<Marker> Marker { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = Dots(frame).Iter(held => held.Size.Match(
                Some: size => path.AddCircle(held.At.X, held.At.Y, size * pen.Icon / 2f),
                None: () => {
                    path.MoveTo(held.At);
                    path.LineTo(held.At);
                }));

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) => _ = Dots(frame).Iter(held => held.Size.Iter(size => path.AddCircle(held.At.X, held.At.Y, size * pen.Icon / 2f)));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => Nearest(frame, local, None).Exists(datum => SKPoint.Distance(frame.Point(datum.Value), local) <= reach);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            Trees.GetValue(this, static points => Indexed.Tree(points.At)) switch {
                var tree => (frame.Plane.Separable ? Grown(frame, local, along, tree, 1f) : Closest(frame, local, along, toSeq(tree.Search()))).Map(index => new Datum(index, At[index])),
            };

        internal override Seq<Stamp> Stamps(PlotFrame frame, Pen pen) => Marker.Map(marker => At.Map(at => new Stamp(marker, frame.Point(at), 0f))).IfNone([]);

        private Seq<Dot> Dots(PlotFrame frame) => Marker.IsSome ? [] : At.Map((at, index) => new Dot(frame.Point(at), Sizes.Bind(sizes => sizes.At(index))));

        private Option<int> Grown(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along, RBush<Indexed> tree, float reach) =>
            Boxed(frame, local, along, reach).Bind(box => Closest(frame, local, along, toSeq(tree.Search(box))).Match(
                Some: index => Distance(frame.Point(At[index]), local, along) <= reach ? Some(index) : Grown(frame, local, along, tree, Distance(frame.Point(At[index]), local, along)),
                None: () => box.Contains(tree.Envelope) ? None : Grown(frame, local, along, tree, 2f * reach)));

        private Option<int> Closest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along, Seq<Indexed> candidates) =>
            candidates.Map(held => (held.Index, Distance: Distance(frame.Point(At[held.Index]), local, along))).Filter(static held => float.IsFinite(held.Distance)) switch {
                { IsEmpty: true } => None,
                var near => Some(near.MinBy(static held => held.Distance).Index),
            };

        private static Option<Envelope> Boxed(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along, float reach) {
            Vector2 offset = along.Match(Some: axis => axis.With(Vector2.Zero, reach), None: () => new Vector2(reach));
            bool Open(PhysicalAxis dimension) => along.Exists(axis => axis.Cross == dimension);
            return from low in frame.Valued(local - (SKPoint)offset)
                   from high in frame.Valued(local + (SKPoint)offset)
                   select new Envelope(
                       Open(PhysicalAxis.Horizontal) ? double.NegativeInfinity : float.Min(low.X, high.X), Open(PhysicalAxis.Vertical) ? double.NegativeInfinity : float.Min(low.Y, high.Y),
                       Open(PhysicalAxis.Horizontal) ? double.PositiveInfinity : float.Max(low.X, high.X), Open(PhysicalAxis.Vertical) ? double.PositiveInfinity : float.Max(low.Y, high.Y));
        }

        private sealed record Dot(SKPoint At, Option<float> Size);
    }

    public sealed record Bars(AxisRange Domain, Samples Values) : Figure {
        public float Baseline { get; init; }
        public Option<Samples> Bases { get; init; }
        public Fraction Width { get; init; } = Fraction.MaxValue;

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Interior(path, frame, pen);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) {
            float slot = Values.Slot(Domain);
            float half = (float)Width * Bandwidth(frame) * MathF.Abs(slot) / 2f;
            _ = Range(0, Values.Values.Length).Iter(index => Values.At(index).Iter(value => Ranged(path, frame, pen,
                AxisRange.Validated(Domain.From + (index * slot) - half, Domain.From + (index * slot) + half),
                AxisRange.Validated(Bases.Bind(bases => bases.At(index)).IfNone(Baseline), value))));
        }

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => frame.At(local).Bind(at => Values.Nearest(Domain, at.X));
    }

    public sealed record Box(float At, Summary Summary) : Figure {
        public Fraction Width { get; init; } = Fraction.MaxValue;

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) {
            float half = Half(frame);
            Interior(path, frame, pen);
            _ = Seq(new Rule(new Vector2(At - half, Summary.Median), new Vector2(At + half, Summary.Median)), new Rule(new Vector2(At, Summary.Minimum), new Vector2(At, Summary.Lower)), new Rule(new Vector2(At, Summary.Upper), new Vector2(At, Summary.Maximum)))
                .Iter(rule => rule.Edges(path, frame, pen));
        }

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            Ranged(path, frame, pen, AxisRange.Validated(At - Half(frame), At + Half(frame)), AxisRange.Validated(Summary.Lower, Summary.Upper));

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => Some(new Datum(0, new Vector2(At, Summary.Median)));

        private float Half(PlotFrame frame) => (float)Width * Bandwidth(frame) / 2f;
    }

    public sealed record Field : Figure {
        private Field(int columns, int rows, Samples values) => (Columns, Rows, Values) = (columns, rows, values);

        public int Columns { get; }
        public int Rows { get; }
        public Samples Values { get; }

        public static Fin<Field> Validated(int columns, int rows, Samples values) =>
            UIFault.Unless(columns > 0 && rows > 0 && columns * rows == values.Values.Length, UIReason.Range, (columns, rows, values.Values.Length)).Map(_ => new Field(columns, rows, values));

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) { }

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            frame.Plane.Enclose(path, frame.Matrix, pen, frame.Plane.Spans(PhysicalAxis.Horizontal, None), frame.Plane.Spans(PhysicalAxis.Vertical, None));

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            from at in frame.At(local)
            let column = int.Clamp((int)MathF.Floor(frame.Plane.Fraction(PhysicalAxis.Horizontal, at.X) * Columns), 0, Columns - 1)
            let row = int.Clamp((int)MathF.Floor(frame.Plane.Fraction(PhysicalAxis.Vertical, at.Y) * Rows), 0, Rows - 1)
            select new Datum((row * Columns) + column, frame.Plane.Valued(new Vector2((column + 0.5f) / Columns, (row + 0.5f) / Rows)));
    }

    public sealed record Label(Figure Along, Seq<Span> Spans) : Figure {
        public float Offset { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) { }

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => false;
    }
}
