using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Simplify;
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
    public static readonly Interpolation Linear = new();
    public static readonly Interpolation Monotone = new() { Spline = Some<Func<double[], double[], CubicSpline>>(CubicSpline.InterpolatePchipSorted) };
    public static readonly Interpolation Natural = new() { Spline = Some<Func<double[], double[], CubicSpline>>(CubicSpline.InterpolateNaturalSorted) };
    public static readonly Interpolation BumpX = new() { Bump = Some(PhysicalAxis.Horizontal) };
    public static readonly Interpolation BumpY = new() { Bump = Some(PhysicalAxis.Vertical) };

    [IgnoreMember]
    private Option<Func<double[], double[], CubicSpline>> Spline { get; init; }

    [IgnoreMember]
    private Option<PhysicalAxis> Bump { get; init; }

    internal Seq<SKPoint> Traced(Seq<SKPoint> knots, double[] at, float density) =>
        (Spline.Filter(_ => knots.Count >= 3).Map(held => Splined(knots, at, density, held)) | Bump.Filter(_ => knots.Count >= 2).Map(along => Bumped(knots, density, along))).IfNone(knots);

    internal void Edges(SKPathBuilder path, Seq<SKPoint> knots, double[] at, float density, bool closed) =>
        _ = Bump.Filter(_ => knots.Count >= 2).Match(
            Some: along => Curved(path, knots, along, closed),
            None: () => path.AddPoly([.. Spline.IsSome ? Traced(knots, at, density) : Simplified(knots, 0.5f / density, closed)], closed));

    internal static double[] Chords(Seq<SKPoint> knots) => [.. toSeq(knots.AsIterable().Window(2, static pair => (double)SKPoint.Distance(pair[0], pair[1]))).Scan(0d, static (length, step) => length + step)];

    private static void Curved(SKPathBuilder path, Seq<SKPoint> knots, PhysicalAxis along, bool closed) {
        path.MoveTo(knots[0]);
        foreach ((SKPoint from, SKPoint to) in knots.AsIterable().Window(2, static pair => (pair[0], pair[1]))) {
            float middle = (along.Of(from) + along.Of(to)) / 2f;
            path.CubicTo((SKPoint)along.With(from, middle), (SKPoint)along.With(to, middle), to);
        }
        if (closed) path.Close();
    }

    private static Seq<SKPoint> Splined(Seq<SKPoint> knots, double[] at, float density, Func<double[], double[], CubicSpline> spline) {
        (CubicSpline across, CubicSpline up) = (spline(at, [.. knots.Map(static knot => (double)knot.X)]), spline(at, [.. knots.Map(static knot => (double)knot.Y)]));
        return Tabulated(knots, density, (index, step) => double.Lerp(at[index], at[index + 1], step) switch { var t => new SKPoint((float)across.Interpolate(t), (float)up.Interpolate(t)) });
    }

    private static Seq<SKPoint> Bumped(Seq<SKPoint> knots, float density, PhysicalAxis along) =>
        Tabulated(knots, density, (index, step) => {
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

    private static Seq<SKPoint> Simplified(Seq<SKPoint> knots, float tolerance, bool closed) =>
        knots.Count < 3 ? knots : toSeq(DouglasPeuckerSimplifier.Simplify(GeometryFactory.Default.CreateLineString(Figure.Coordinates(closed ? knots + knots.Take(1) : knots)), tolerance).Coordinates)
            .Map(Figure.Point) switch { var line => closed && line.Count > 1 ? line.Init : line, };
}

public sealed record Marker(GlyphSource Art, float Size);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Floor {
    public sealed record Baseline(float Value) : Floor;

    public sealed record Lower(Samples Samples) : Floor;
}

internal sealed record MarkerPlacement(Marker Marker, SKPoint At, float Degrees);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Figure {
    private Figure(bool continuous) => Continuous = continuous;

    internal bool Continuous { get; }

    public abstract void Edges(SKPathBuilder path, PlotFrame frame, Pen pen);

    public virtual void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) { }

    public abstract bool Hits(PlotFrame frame, SKPoint local, float reach);

    public virtual Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => None;

    public Option<Figure> Toward(Figure to, float progress) =>
        Switch<(Figure To, float Progress), Option<Figure>>(
            (to, progress),
            rule: static (held, source) => held.To is Rule other ? Some<Figure>(new Rule(Vector2.Lerp(source.From, other.From, held.Progress), Vector2.Lerp(source.To, other.To, held.Progress))) : None,
            path: static (held, source) => held.To is Path other && source.Vertices.Count == other.Vertices.Count && source.Holes.Map(static hole => hole.Count) == other.Holes.Map(static hole => hole.Count)
                ? Some<Figure>(other with {
                    Vertices = Lerped(source.Vertices, other.Vertices, held.Progress),
                    Holes = toSeq(source.Holes.Zip(other.Holes, (one, two) => Lerped(one, two, held.Progress))),
                    Widths = Blended(source.Widths, other.Widths, (one, two) => one.Toward(two, held.Progress)),
                })
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

    internal virtual Seq<Datum> Data => [];

    internal virtual Option<Datum> DatumAt(int index) => Data.Find(datum => datum.Index == index);

    internal virtual float Weight(PlotFrame frame, Pen pen) => 1f;

    internal virtual Seq<MarkerPlacement> Markers(PlotFrame frame, Pen pen, SKPath edge) => [];

    internal static Coordinate[] Coordinates(Seq<SKPoint> line) => [.. line.Map(static at => new Coordinate(at.X, at.Y))];

    internal static SKPoint Point(Coordinate at) => new((float)at.X, (float)at.Y);

    private static Seq<Vector2> Lerped(Seq<Vector2> from, Seq<Vector2> to, float progress) => toSeq(from.Zip(to, (one, two) => Vector2.Lerp(one, two, progress)));

    private static Option<T> Blended<T>(Option<T> from, Option<T> to, Func<T, T, Option<T>> toward) => to.Map(target => from.Bind(source => toward(source, target)).IfNone(target));

    private static float Reach(Seq<SKPoint> line, SKPoint at) => line.IsEmpty ? float.PositiveInfinity : (float)DistanceComputer.PointToSegmentString(new Coordinate(at.X, at.Y), Coordinates(line));

    private static bool Inside(Seq<Seq<SKPoint>> rings, SKPoint at) =>
        rings.Filter(static ring => !ring.IsEmpty).Count(ring => PointLocation.IsInRing(new Coordinate(at.X, at.Y), Coordinates(ring + ring.Take(1)))) % 2 is 1;

    private static void Filled(SKPathBuilder path, Seq<Seq<SKPoint>> rings) {
        if (rings.Count > 1) path.FillType = SKPathFillType.EvenOdd;
        _ = rings.Iter(ring => path.AddPoly([.. ring], close: true));
    }

    private static Seq<Seq<SKPoint>> Rings(Geometry geometry) =>
        geometry switch {
            Polygon polygon => Ring(polygon.ExteriorRing).Cons(toSeq(polygon.InteriorRings).Map(Ring)),
            GeometryCollection collection => toSeq(collection.Geometries).Bind(Rings),
            _ => [],
        };

    private static Seq<SKPoint> Ring(LineString ring) => toSeq(ring.Coordinates[..^1]).Map(Point);

    private static float Bandwidth(PlotFrame frame) => frame.Plane.Dimension(PhysicalAxis.Horizontal).Map(static axis => axis.Bandwidth).IfNone(1f);

    private static float Distance(SKPoint point, SKPoint local, Option<PhysicalAxis> along) =>
        along.Match(Some: axis => MathF.Abs(axis.Of((Vector2)point - (Vector2)local)), None: () => SKPoint.Distance(point, local));

    private static float Turned(SKPoint from, SKPoint to) => float.RadiansToDegrees(MathF.Atan2(to.Y - from.Y, to.X - from.X));

    public sealed record Rule(Vector2 From, Vector2 To) : Figure(continuous: true) {
        internal override Seq<Datum> Data => [new(0, From), new(1, To)];

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            path.AddPoly([.. frame.Plane.Separable ? pen.Snapped(frame.Point(From), frame.Point(To)) switch { var (from, to) => Seq(from, to) } : frame.Traced([From, To], closed: false, pen)], close: false);

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => Reach(frame.Traced([From, To], closed: false, Pen.Hit), local) <= reach;
    }

    public sealed record Path(Seq<Vector2> Vertices) : Figure(continuous: true) {
        public bool Closed { get; init; }
        public Interpolation Interpolation { get; init; } = Interpolation.Linear;
        public Option<Marker> Start { get; init; }
        public Option<Marker> End { get; init; }
        public Option<Samples> Widths { get; init; }
        public Seq<Seq<Vector2>> Holes { get; init; }

        internal override Seq<Datum> Data => Vertices.Map(static (at, index) => new Datum(index, at));

        internal override Option<Datum> DatumAt(int index) => Vertices.At(index).Map(at => new Datum(index, at));

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Outlined(path, frame, pen, Closed);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = Ribbon(frame).Match(Some: ribbon => Filled(path, ribbon), None: () => {
                if (Closed) Outlined(path, frame, pen, closed: true);
            });

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) =>
            Inside(Ribbon(frame).IfNone(() => Closed ? Loops(frame, Pen.Hit, closing: false) : []), local) || Loops(frame, Pen.Hit, closing: true).Exists(line => Reach(line, local) <= reach);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            Vertices.Map((at, index) => (Datum: new Datum(index, at), Distance: Distance(frame.Point(at), local, along))) switch {
                { IsEmpty: true } => None,
                var near => Some(near.MinBy(static held => held.Distance).Datum),
            };

        internal override Seq<MarkerPlacement> Markers(PlotFrame frame, Pen pen, SKPath edge) =>
            Closed || edge.PointCount < 2
                ? []
                : Start.Map(marker => new MarkerPlacement(marker, edge.GetPoint(0), Turned(edge.GetPoint(1), edge.GetPoint(0)))).ToSeq()
                    + End.Map(marker => new MarkerPlacement(marker, edge.LastPoint, Turned(edge.GetPoint(edge.PointCount - 2), edge.LastPoint))).ToSeq();

        private void Outlined(SKPathBuilder path, PlotFrame frame, Pen pen, bool closed) {
            if (!Holes.IsEmpty) path.FillType = SKPathFillType.EvenOdd;
            Emitted(path, frame, pen, Vertices, closed);
            _ = Holes.Iter(hole => Emitted(path, frame, pen, hole, closed: true));
        }

        private void Emitted(SKPathBuilder path, PlotFrame frame, Pen pen, Seq<Vector2> loop, bool closed) {
            Seq<SKPoint> knots = Knots(frame, pen, loop, closed);
            Interpolation.Edges(path, knots, Interpolation.Chords(knots), pen.Density, closed);
        }

        private Seq<Seq<SKPoint>> Loops(PlotFrame frame, Pen pen, bool closing) =>
            Traced(frame, pen, Vertices, Closed, closing).Cons(Holes.Map(hole => Traced(frame, pen, hole, closed: true, closing)));

        private Seq<SKPoint> Traced(PlotFrame frame, Pen pen, Seq<Vector2> loop, bool closed, bool closing) =>
            Knots(frame, pen, loop, closed) switch {
                var knots => Interpolation.Traced(knots, Interpolation.Chords(knots), pen.Density) switch { var line => closed && closing ? line + line.Take(1) : line },
            };

        private Seq<SKPoint> Knots(PlotFrame frame, Pen pen, Seq<Vector2> loop, bool closed) =>
            toSeq((Interpolation == Interpolation.Linear ? frame.Traced(loop, closed, pen) : loop.Map(frame.Point)).DistinctUntilChanged());

        private Option<Seq<Seq<SKPoint>>> Ribbon(PlotFrame frame) =>
            Widths.Filter(widths => widths.Values.Length == Vertices.Count && Vertices.Count >= 2).Map(widths => {
                double[] distances = new double[widths.Values.Length];
                TensorPrimitives.ConvertTruncating(widths.Values.Span, distances);
                TensorPrimitives.Multiply(distances, 0.5d, distances);
                return Rings(VariableBuffer.Buffer(GeometryFactory.Default.CreateLineString(Coordinates(Vertices.Map(frame.Point))), distances));
            });
    }

    public sealed record Region : Figure {
        public Region() : base(continuous: false) { }

        public Option<AxisRange> First { get; init; }
        public Option<AxisRange> Second { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Interior(path, frame, pen);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            frame.Plane.Enclose(path, frame.Matrix, pen, frame.Plane.Spans(PhysicalAxis.Horizontal, First), frame.Plane.Spans(PhysicalAxis.Vertical, Second));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => frame.Contains(local, First, Second);
    }

    public sealed record Curve(AxisRange Domain, Samples Samples) : Figure(continuous: true) {
        public Option<Fraction> Step { get; init; }
        public Option<Floor> Fill { get; init; }
        public Interpolation Interpolation { get; init; } = Interpolation.Linear;

        internal override Seq<Datum> Data => Samples.Data(Domain);

        internal override Option<Datum> DatumAt(int index) => Samples.DatumAt(Domain, index);

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = Runs(frame, pen, Samples, Shared(frame, None)).Iter(run => Shaping(frame, pen).Edges(path, Knots(frame, pen, run), Abscissa(run), pen.Density, closed: false));

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) => Filled(path, Outlines(frame, pen));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) =>
            Inside(Outlines(frame, Pen.Hit), local) || Runs(frame, Pen.Hit, Samples, Shared(frame, None)).Exists(run => Reach(Traced(frame, Pen.Hit, run), local) <= reach);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => frame.At(local).Bind(at => Samples.Nearest(Domain, at.X));

        private Seq<Seq<SKPoint>> Outlines(PlotFrame frame, Pen pen) =>
            Fill.Map(floor => floor.Switch(
                (Curve: this, Frame: frame, Pen: pen),
                baseline: static (held, baseline) => held.Frame.Plane.Fraction(PhysicalAxis.Vertical, baseline.Value) switch {
                    var level => held.Curve.Runs(held.Frame, held.Pen, held.Curve.Samples, held.Curve.Shared(held.Frame, None))
                        .Map(run => held.Curve.Traced(held.Frame, held.Pen, run) + held.Frame.Walked([run[run.Count - 1] with { Y = level }, run[0] with { Y = level }], closed: false, held.Pen)),
                },
                lower: static (held, lower) => held.Curve.Shared(held.Frame, Some(lower.Samples)) switch {
                    var shared => toSeq(held.Curve.Runs(held.Frame, held.Pen, held.Curve.Samples, shared).Zip(held.Curve.Runs(held.Frame, held.Pen, lower.Samples, shared),
                        (upper, under) => held.Curve.Traced(held.Frame, held.Pen, upper) + held.Curve.Traced(held.Frame, held.Pen, under).Rev())),
                })).IfNone([]);

        private Seq<Run> Shared(PlotFrame frame, Option<Samples> paired) => Samples.Finite(Samples.Visible(frame, Domain), paired);

        private Seq<Seq<Vector2>> Runs(PlotFrame frame, Pen pen, Samples samples, Seq<Run> runs) =>
            samples.Decimated(frame, Domain, Columns(frame, pen), runs).Map(run => Step.Match(
                Some: step => toSeq(run.Window(2).SelectMany(pair => float.Lerp(pair[0].X, pair[1].X, (float)step) switch { var jump => Seq(pair[0], pair[0] with { X = jump }, pair[1] with { X = jump }) })).Add(run[run.Count - 1]),
                None: () => run));

        private Interpolation Shaping(PlotFrame frame, Pen pen) => Step.IsNone && Samples.Values.Length <= Columns(frame, pen) ? Interpolation : Interpolation.Linear;

        private Seq<SKPoint> Knots(PlotFrame frame, Pen pen, Seq<Vector2> run) => Shaping(frame, pen) == Interpolation.Linear ? frame.Walked(run, closed: false, pen) : frame.Projected(run);

        private Seq<SKPoint> Traced(PlotFrame frame, Pen pen, Seq<Vector2> run) => Shaping(frame, pen).Traced(Knots(frame, pen, run), Abscissa(run), pen.Density);

        private static double[] Abscissa(Seq<Vector2> run) => [.. run.Map(static at => (double)at.X)];

        private static float Columns(PlotFrame frame, Pen pen) => frame.Pixels(PhysicalAxis.Horizontal) * pen.Density;
    }

    public sealed record Graph(AxisRange Domain, Func<float, float> Function) : Figure(continuous: true) {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Tabulated(frame, pen).Edges(path, frame, pen);

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => Tabulated(frame, Pen.Hit).Hits(frame, local, reach);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            frame.At(local).Map(at => new Datum(0, new Vector2(at.X, Function(at.X)))).Filter(static datum => float.IsFinite(datum.Value.Y));

        private Curve Tabulated(PlotFrame frame, Pen pen) {
            float spanned = MathF.Abs(frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.To) - frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.From));
            int count = int.Max(2, (int)MathF.Ceiling(spanned * frame.Pixels(PhysicalAxis.Horizontal) * pen.Density) + 1);
            return new Curve(Domain, new Samples(Generate.LinearSpacedMap(count, Domain.From, Domain.To, at => Function((float)at))));
        }
    }

    public sealed record Grid(PhysicalAxis Along) : Figure(continuous: false) {
        public bool Major { get; init; } = true;
        public Option<float> Spacing { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = frame.Plane.Dimension(Along).Iter(axis => axis.Ticks(Least(frame, pen)).Filter(tick => tick.Major == Major).Iter(tick => frame.Plane.GridLine(path, frame.Matrix, pen, Along, axis.Fraction(tick.Value))));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => false;

        internal override float Weight(PlotFrame frame, Pen pen) =>
            Major ? 1f : frame.Plane.Dimension(Along).Map(axis => axis.Ticks(Least(frame, pen)).Map(tick => axis.Fraction(tick.Value)).Window(2)
                .Select(pair => MathF.Abs(pair[1] - pair[0]) * frame.Pixels(Along)).DefaultIfEmpty(0f).Min())
                .Map(spacing => float.Clamp((spacing / pen.Spacing) - 1f, 0f, 1f)).IfNone(0f);

        private float Least(PlotFrame frame, Pen pen) => Spacing.IfNone(pen.Spacing) / frame.Pixels(Along);
    }

    public sealed record Points(Seq<Vector2> At) : Figure(continuous: false) {
        private static readonly ConditionalWeakTable<Points, RBush<Indexed>> Trees = [];

        public Option<Samples> Values { get; init; }
        public Option<Samples> Sizes { get; init; }
        public Option<Samples> Opacity { get; init; }
        public Option<Marker> Marker { get; init; }

        internal override Seq<Datum> Data => At.Map(static (at, index) => new Datum(index, at));

        internal override Option<Datum> DatumAt(int index) => At.At(index).Map(at => new Datum(index, at));

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
            Indexed.Nearest(Trees.GetValue(this, static points => Indexed.Tree(points.At)), frame, local, along, index => Distance(frame.Point(At[index]), local, along)).Map(index => new Datum(index, At[index]));

        internal override Seq<MarkerPlacement> Markers(PlotFrame frame, Pen pen, SKPath edge) => Marker.Map(marker => At.Map(at => new MarkerPlacement(marker, frame.Point(at), 0f))).IfNone([]);

        private Seq<Dot> Dots(PlotFrame frame) => Marker.IsSome ? [] : At.Map((at, index) => new Dot(frame.Point(at), Sizes.Bind(sizes => sizes.At(index))));

        private sealed record Dot(SKPoint At, Option<float> Size);
    }

    public sealed record Bars(AxisRange Domain, Samples Values) : Figure(continuous: false) {
        public float Baseline { get; init; }
        public Option<Samples> Bases { get; init; }
        public Fraction Width { get; init; } = Fraction.MaxValue;

        internal override Seq<Datum> Data => Values.Data(Domain);

        internal override Option<Datum> DatumAt(int index) => Values.DatumAt(Domain, index);

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Interior(path, frame, pen);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = Values.Visible(frame, Domain) switch { var window => toSeq(Range(window.Start, window.Count)).Choose(index => Bar(frame, index)).Iter(bar => bar.Interior(path, frame, pen)) };

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) =>
            frame.Valued(local).Bind(at => Values.Nearest(Domain, at.X)).Bind(datum => Bar(frame, datum.Index)).Exists(bar => bar.Hits(frame, local, reach));

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => frame.At(local).Bind(at => Values.Nearest(Domain, at.X));

        private Option<Region> Bar(PlotFrame frame, int index) {
            float slot = Values.Slot(Domain);
            float half = (float)Width * Bandwidth(frame) * MathF.Abs(slot) / 2f;
            float center = Domain.From + (index * slot);
            return Values.At(index).Bind(value => (AxisRange.Validated(center - half, center + half), AxisRange.Validated(Bases.Bind(bases => bases.At(index)).IfNone(Baseline), value))
                .Apply(static (across, up) => new Region { First = Some(across), Second = Some(up) }).As().ToOption());
        }
    }

    public sealed record Box(float At, Summary Summary) : Figure(continuous: false) {
        public Fraction Width { get; init; } = Fraction.MaxValue;

        internal override Seq<Datum> Data => [new(0, new Vector2(At, Summary.Median))];

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) {
            float half = Half(frame);
            Interior(path, frame, pen);
            _ = Seq(new Rule(new Vector2(At - half, Summary.Median), new Vector2(At + half, Summary.Median)), new Rule(new Vector2(At, Summary.Minimum), new Vector2(At, Summary.Lower)), new Rule(new Vector2(At, Summary.Upper), new Vector2(At, Summary.Maximum)))
                .Iter(rule => rule.Edges(path, frame, pen));
        }

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) => _ = Body(frame).Iter(body => body.Interior(path, frame, pen));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => Body(frame).Exists(body => body.Hits(frame, local, reach));

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) => Data.Head;

        private Option<Region> Body(PlotFrame frame) =>
            (AxisRange.Validated(At - Half(frame), At + Half(frame)), AxisRange.Validated(Summary.Lower, Summary.Upper)).Apply(static (across, up) => new Region { First = Some(across), Second = Some(up) }).As().ToOption();

        private float Half(PlotFrame frame) => (float)Width * Bandwidth(frame) / 2f;
    }

    public sealed record Field : Figure {
        private Field(int columns, int rows, Samples values) : base(continuous: false) => (Columns, Rows, Values) = (columns, rows, values);

        public int Columns { get; }
        public int Rows { get; }
        public Samples Values { get; }

        public static Fin<Field> Validated(int columns, int rows, Samples values) =>
            UIFault.Unless(columns > 0 && rows > 0 && columns * rows == values.Values.Length, UIReason.Range, (columns, rows, values.Values.Length)).Map(_ => new Field(columns, rows, values));

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) { }

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            frame.Plane.Enclose(path, frame.Matrix, pen, frame.Plane.Spans(PhysicalAxis.Horizontal, None), frame.Plane.Spans(PhysicalAxis.Vertical, None));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => frame.Contains(local, None, None);

        public override Option<Datum> Nearest(PlotFrame frame, SKPoint local, Option<PhysicalAxis> along) =>
            from at in frame.At(local)
            let column = int.Clamp((int)MathF.Floor(frame.Plane.Fraction(PhysicalAxis.Horizontal, at.X) * Columns), 0, Columns - 1)
            let row = int.Clamp((int)MathF.Floor(frame.Plane.Fraction(PhysicalAxis.Vertical, at.Y) * Rows), 0, Rows - 1)
            select new Datum((row * Columns) + column, frame.Plane.Valued(new Vector2((column + 0.5f) / Columns, (row + 0.5f) / Rows)));
    }

    public sealed record Label(Figure Along, Seq<Span> Spans) : Figure(continuous: false) {
        public float Offset { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) { }

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => false;
    }
}
