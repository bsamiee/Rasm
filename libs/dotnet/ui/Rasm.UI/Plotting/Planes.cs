using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using MathNet.Numerics;
using ProjNet.CoordinateSystems.Transformations;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlaneAxis(PhysicalAxis Along, Axis Axis);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotPlane {
    public abstract Option<Axis> Dimension(PhysicalAxis along);

    public SKMatrix Frame(SKRect content) =>
        Aspect.Match(Some: ratio => content.AspectFit(new SKSize((float)ratio, 1f)), None: () => content) switch {
            var box => SKMatrix.CreateScaleTranslation(box.Width, -box.Height, box.Left, box.Bottom),
        };

    public virtual Option<Factor> Aspect => None;

    public PlotFrame In(SKRect content) => new(this, Frame(content));

    public SKPoint Unit(Vector2 value) => Projected(Fraction(value));

    public Vector2 Value(SKPoint square) => Each(Unprojected(square), static (axis, held) => axis.Value(held));

    public Vector2 Constrained(Vector2 value) => Each(value, static (axis, held) => axis.Constrained(held));

    public Seq<PlaneAxis> Dimensions => toSeq(PhysicalAxis.Items).Choose(along => Dimension(along).Map(axis => new PlaneAxis(along, axis)));

    public string Spoken(Vector2 at, CultureInfo culture) => string.Join($"{culture.TextInfo.ListSeparator} ", Dimensions.Map(held => held.Axis.Spoken(held.Along.Of(at), culture)));

    public Option<PlotPlane> Shifted(Vector2 delta) => Over((along, axis) => axis.Domain.Shifted(along.Of(delta)).Bind(axis.WithDomain));

    public Option<PlotPlane> Scaled(Vector2 from, Vector2 to, Vector2 factors) => Over((along, axis) => axis.Domain.Scaled(along.Of(from), along.Of(to), along.Of(factors)).Bind(axis.WithDomain));

    public Option<PlotPlane> WithDomain(PhysicalAxis along, AxisRange domain) => Over((held, axis) => held == along ? axis.WithDomain(domain) : Some(axis));

    public Option<PlotPlane> Toward(PlotPlane to, float progress) =>
        to.Over((along, _) => Dimension(along)).Filter(same => same == this).Bind(_ => Over((along, axis) => to.Dimension(along).Bind(other => axis.Toward(other, progress))));

    public virtual Option<Vector2> Rotated(Vector2 value, float radians) => None;

    internal virtual bool Separable => true;

    internal virtual SKPoint Projected(Vector2 fractions) => fractions;

    internal virtual void Projected(Span<float> x, Span<float> y) { }

    internal virtual Vector2 Unprojected(SKPoint square) => square;

    internal virtual float Length(PhysicalAxis along) => 1f;

    internal virtual void GridLine(SKPathBuilder path, SKMatrix frame, Pen pen, PhysicalAxis held, float at) {
        (SKPoint from, SKPoint to) = pen.Snapped(frame.MapPoint(held.With(Vector2.Zero, at)), frame.MapPoint(held.With(Vector2.One, at)));
        path.MoveTo(from);
        path.LineTo(to);
    }

    internal virtual void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<FractionRange> across, Seq<FractionRange> up) =>
        _ = (from x in across
             from y in up
             select frame.MapRect(new SKRect(x.Low, y.Low, x.High, y.High)))
            .Iter(cell => {
                SKPoint low = pen.Snapped(cell.Location);
                SKPoint high = pen.Snapped(new SKPoint(cell.Right, cell.Bottom));
                path.AddRect(new SKRect(low.X, low.Y, high.X, high.Y));
            });

    internal Seq<SKPoint> Projected(SKMatrix frame, Seq<Vector2> fractions) {
        float[] x = [.. fractions.Map(static at => at.X)];
        float[] y = [.. fractions.Map(static at => at.Y)];
        Projected(x, y);
        SKPoint[] points = [.. x.Zip(y, static (across, up) => new SKPoint(across, up))];
        frame.MapPoints(points, points);
        return toSeq(points);
    }

    internal Vector2 Fraction(Vector2 value) => Each(value, static (axis, held) => axis.Unit(held));

    internal float Fraction(PhysicalAxis along, float value) => Of(along, value, static (axis, held) => axis.Fraction(held));

    internal float Value(PhysicalAxis along, float fraction) => Of(along, fraction, static (axis, held) => axis.Value(held));

    internal Vector2 Valued(Vector2 fractions) => Each(fractions, static (axis, held) => axis.Value(held));

    internal void Fractions(PhysicalAxis along, float[] values) => _ = Dimension(along).Iter(axis => axis.Fractions(values, values));

    internal bool Contains(PhysicalAxis along, Option<AxisRange> band, float value) =>
        Dimension(along).Match(Some: axis => axis.Contains(band.IfNone(axis.Domain), value), None: () => band.IfNone(AxisRange.Unit).Contains(value));

    internal Seq<FractionRange> Spans(PhysicalAxis along, Option<AxisRange> band) =>
        band.Match(
            Some: held => Dimension(along).Match(Some: axis => axis.Spans(held), None: () => [new FractionRange(held.Low, held.High)]),
            None: static () => [new FractionRange(0f, 1f)]);

    internal static int Density(SKMatrix frame, Pen pen) => int.Max(2, (int)MathF.Ceiling(float.Max(MathF.Abs(frame.ScaleX), MathF.Abs(frame.ScaleY)) * pen.Device.MapRadius(1f)));

    internal static Seq<Vector2> Walked(Vector2 from, Vector2 to, int count) => toSeq(Range(0, count)).Map(step => Vector2.Lerp(from, to, step / (float)count));

    internal static Seq<Vector2> Densified(Seq<Vector2> fractions, bool closed, int density) =>
        toSeq((closed ? fractions + fractions.Take(1) : fractions).Window(2)
            .SelectMany(pair => Walked(pair[0], pair[1], int.Max(1, (int)MathF.Ceiling(float.Max(MathF.Abs(pair[1].X - pair[0].X), MathF.Abs(pair[1].Y - pair[0].Y)) * density)))))
        + (closed ? [] : fractions.Last.ToSeq());

    private protected abstract Option<PlotPlane> Over(Func<PhysicalAxis, Axis, Option<Axis>> map);

    private float Of(PhysicalAxis along, float value, Func<Axis, float, float> map) => Dimension(along).Map(axis => map(axis, value)).IfNone(value);

    private Vector2 Each(Vector2 value, Func<Axis, float, float> map) => new(Of(PhysicalAxis.Horizontal, value.X, map), Of(PhysicalAxis.Vertical, value.Y, map));

    public sealed record Cartesian(Axis X, Axis Y) : PlotPlane {
        public override Option<Axis> Dimension(PhysicalAxis along) => Some(along.Map(horizontal: X, vertical: Y));

        private protected override Option<PlotPlane> Over(Func<PhysicalAxis, Axis, Option<Axis>> map) =>
            from x in map(PhysicalAxis.Horizontal, X)
            from y in map(PhysicalAxis.Vertical, Y)
            select (PlotPlane)new Cartesian(x, y);
    }

    public sealed record Track(PhysicalAxis Along, Axis Axis) : PlotPlane {
        public static Track Unit(PhysicalAxis along) => new(along, new Axis.Linear(AxisRange.Unit));

        public override Option<Axis> Dimension(PhysicalAxis along) => along == Along ? Some(Axis) : None;

        public Node Stretched(Node node) =>
            Along.Map(horizontal: node with { Width = Sizing.Grow.Unit, Height = Sizing.Fit.Content }, vertical: node with { Width = Sizing.Fit.Content, Height = Sizing.Grow.Unit });

        private protected override Option<PlotPlane> Over(Func<PhysicalAxis, Axis, Option<Axis>> map) => map(Along, Axis).Map(axis => (PlotPlane)(this with { Axis = axis }));
    }

    public sealed record Polar(Axis Angle, Option<Axis> Radius) : PlotPlane {
        public override Option<Factor> Aspect => Some(Factor.Unit);

        public override Option<Axis> Dimension(PhysicalAxis along) => along.Map(horizontal: Some(Angle), vertical: Radius);

        public override Option<Vector2> Rotated(Vector2 value, float radians) => Some(value with { X = Angle.Constrained(value.X + (radians / MathF.Tau * Angle.Domain.Span)) });

        internal override bool Separable => false;

        internal override SKPoint Projected(Vector2 fractions) {
            (float sin, float cos) = MathF.SinCos(MathF.Tau * fractions.X);
            return new Vector2(0.5f) + (new Vector2(cos, sin) * (0.5f * fractions.Y));
        }

        internal override void Projected(Span<float> x, Span<float> y) {
            using IMemoryOwner<float> sine = MemoryPool<float>.Shared.Rent(x.Length);
            Span<float> held = sine.Memory.Span[..x.Length];
            TensorPrimitives.Multiply(x, 2f, x);
            TensorPrimitives.SinCosPi(x, held, x);
            TensorPrimitives.Multiply(y, 0.5f, y);
            TensorPrimitives.MultiplyAdd(x, y, 0.5f, x);
            TensorPrimitives.MultiplyAdd(held, y, 0.5f, y);
        }

        internal override Vector2 Unprojected(SKPoint square) {
            Vector2 offset = (Vector2)square - new Vector2(0.5f);
            return new Vector2(MathF.Atan2(offset.Y, offset.X) / MathF.Tau, 2f * offset.Length());
        }

        internal override float Length(PhysicalAxis along) => along.Map(horizontal: MathF.PI, vertical: 0.5f);

        internal override void GridLine(SKPathBuilder path, SKMatrix frame, Pen pen, PhysicalAxis held, float at) =>
            held.Switch(
                (Path: path, Center: frame.MapPoint(Projected(Vector2.Zero)), Rim: frame.MapPoint(Projected(new Vector2(at, 1f))), Radius: Half(frame) * at),
                horizontal: static ray => {
                    ray.Path.MoveTo(ray.Center);
                    ray.Path.LineTo(ray.Rim);
                },
                vertical: static ring => ring.Path.AddCircle(ring.Center.X, ring.Center.Y, ring.Radius));

        internal override void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<FractionRange> across, Seq<FractionRange> up) {
            SKPoint center = frame.MapPoint(Projected(Vector2.Zero));
            float half = Half(frame);
            float extent = across.Sum(static span => span.High - span.Low);
            float turn = MathF.CopySign(360f, frame.ScaleX * frame.ScaleY);
            float start = across.Head.Map(static span => span.Low).IfNone(0f) * turn;
            float sweep = extent * turn;
            _ = up.Iter(ring => {
                if (extent >= 1f) {
                    path.FillType = SKPathFillType.EvenOdd;
                    path.AddCircle(center.X, center.Y, half * ring.High);
                    if (ring.Low > 0f) path.AddCircle(center.X, center.Y, half * ring.Low);
                    return;
                }
                path.ArcTo(Oval(center, half * ring.High), start, sweep, forceMoveTo: true);
                if (ring.Low > 0f) path.ArcTo(Oval(center, half * ring.Low), start + sweep, -sweep, forceMoveTo: false);
                else path.LineTo(center);
                path.Close();
            });
        }

        private protected override Option<PlotPlane> Over(Func<PhysicalAxis, Axis, Option<Axis>> map) =>
            from angle in map(PhysicalAxis.Horizontal, Angle)
            from radius in Radius.Match(Some: held => map(PhysicalAxis.Vertical, held).Map(Some), None: static () => Some(Option<Axis>.None))
            select (PlotPlane)new Polar(angle, radius);

        private static float Half(SKMatrix frame) => MathF.Abs(frame.ScaleX) / 2f;

        private static SKRect Oval(SKPoint center, float radius) => SKRect.Create(center.X - radius, center.Y - radius, 2f * radius, 2f * radius);
    }

    public sealed record Mapped : PlotPlane {
        private const int Lattice = 32;

        private readonly Mapping mapping;
        private readonly SKRect world;

        private Mapped(Axis x, Axis y, Mapping mapping, SKRect world) => (X, Y, this.mapping, this.world) = (x, y, mapping, world);

        public Axis X { get; }
        public Axis Y { get; }

        public override Option<Factor> Aspect => Factor.Validated(world.Width / world.Height).ToOption();

        public override Option<Axis> Dimension(PhysicalAxis along) => Some(along.Map(horizontal: X, vertical: Y));

        public static Fin<Mapped> Validated(Axis x, Axis y, Func<Vector2, Vector2> place, Func<Vector2, Vector2> unplace) => Bounded(x, y, new Mapping.Functions(place, unplace));

        public static Fin<Mapped> Validated(Axis longitude, Axis latitude, MathTransform projection) => Bounded(longitude, latitude, new Mapping.Transformed(projection, projection.Inverse()));

        internal override bool Separable => false;

        internal override SKPoint Projected(Vector2 fractions) =>
            mapping.Placed(new Vector2(X.Value(fractions.X), Y.Value(fractions.Y))) switch {
                var at => new SKPoint((at.X - world.Left) / world.Width, (at.Y - world.Top) / world.Height),
            };

        internal override void Projected(Span<float> x, Span<float> y) {
            using IMemoryOwner<double> across = MemoryPool<double>.Shared.Rent(x.Length);
            using IMemoryOwner<double> down = MemoryPool<double>.Shared.Rent(y.Length);
            Span<double> placedX = across.Memory.Span[..x.Length];
            Span<double> placedY = down.Memory.Span[..y.Length];
            for (int index = 0; index < x.Length; index++) (placedX[index], placedY[index]) = (X.Value(x[index]), Y.Value(y[index]));
            mapping.Placed(placedX, placedY);
            TensorPrimitives.ConvertTruncating(placedX, x);
            TensorPrimitives.ConvertTruncating(placedY, y);
            TensorPrimitives.Subtract(x, world.Left, x);
            TensorPrimitives.Divide(x, world.Width, x);
            TensorPrimitives.Subtract(y, world.Top, y);
            TensorPrimitives.Divide(y, world.Height, y);
        }

        internal override Vector2 Unprojected(SKPoint square) =>
            mapping.Unplaced(new Vector2(world.Left + (square.X * world.Width), world.Top + (square.Y * world.Height))) switch {
                var at => new Vector2(X.Fraction(at.X), Y.Fraction(at.Y)),
            };

        internal override void GridLine(SKPathBuilder path, SKMatrix frame, Pen pen, PhysicalAxis held, float at) =>
            path.AddPoly([.. Projected(frame, Walked(held.With(Vector2.Zero, at), held.With(Vector2.One, at), Density(frame, pen)).Add(held.With(Vector2.One, at)))], close: false);

        internal override void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<FractionRange> across, Seq<FractionRange> up) =>
            _ = (from x in across
                 from y in up
                 select Densified([new(x.Low, y.Low), new(x.High, y.Low), new(x.High, y.High), new(x.Low, y.High)], closed: true, Density(frame, pen)))
                .Iter(cell => path.AddPoly([.. Projected(frame, cell)], close: true));

        private protected override Option<PlotPlane> Over(Func<PhysicalAxis, Axis, Option<Axis>> map) =>
            from x in map(PhysicalAxis.Horizontal, X)
            from y in map(PhysicalAxis.Vertical, Y)
            from mapped in Bounded(x, y, mapping).ToOption()
            select (PlotPlane)mapped;

        private static Fin<Mapped> Bounded(Axis x, Axis y, Mapping mapping) {
            double[] lattice = Generate.LinearSpaced(Lattice, 0d, 1d);
            double[] across = [.. from up in lattice from along in lattice select (double)x.Value((float)along)];
            double[] down = [.. from up in lattice from along in lattice select (double)y.Value((float)up)];
            mapping.Placed(across, down);
            SKRect bounds = new((float)TensorPrimitives.Min(across), (float)TensorPrimitives.Min(down), (float)TensorPrimitives.Max(across), (float)TensorPrimitives.Max(down));
            return UIFault.Unless(float.IsFinite(bounds.Width) && float.IsFinite(bounds.Height) && bounds.Width > 0f && bounds.Height > 0f, UIReason.Range, bounds).Map(_ => new Mapped(x, y, mapping, bounds));
        }

        private abstract record Mapping {
            internal abstract Vector2 Placed(Vector2 value);

            internal abstract void Placed(Span<double> x, Span<double> y);

            internal abstract Vector2 Unplaced(Vector2 world);

            internal sealed record Functions(Func<Vector2, Vector2> Place, Func<Vector2, Vector2> Unplace) : Mapping {
                internal override Vector2 Placed(Vector2 value) => Place(value);

                internal override void Placed(Span<double> x, Span<double> y) {
                    for (int index = 0; index < x.Length; index++) (x[index], y[index]) = Place(new Vector2((float)x[index], (float)y[index])) switch { var at => (at.X, at.Y) };
                }

                internal override Vector2 Unplaced(Vector2 world) => Unplace(world);
            }

            internal sealed record Transformed(MathTransform Forward, MathTransform Inverse) : Mapping {
                internal override Vector2 Placed(Vector2 value) => Forward.Transform(value.X, value.Y) switch { var (x, y) => new((float)x, (float)y) };

                internal override void Placed(Span<double> x, Span<double> y) => Forward.Transform(x, y);

                internal override Vector2 Unplaced(Vector2 world) => Inverse.Transform(world.X, world.Y) switch { var (x, y) => new((float)x, (float)y) };
            }
        }
    }
}

public sealed record Guide(PhysicalAxis Along) {
    public Fraction Edge { get; init; } = Fraction.MinValue;
    public Option<string> Title { get; init; }
    public Option<Func<float, string>> Format { get; init; }
    public float Angle { get; init; }

    internal Vector2 Anchor(PlotPlane plane, float value) => Along.With(Along.Cross.With(Vector2.Zero, plane.Value(Along.Cross, (float)Edge)), value);

    internal string Label(Axis axis, float value, CultureInfo culture) => Format.Match(Some: format => format(value), None: () => axis.Spoken(value, culture));
}

public sealed record PlotFrame {
    private readonly Option<SKMatrix> inverse;

    public PlotFrame(PlotPlane plane, SKMatrix matrix) => (Plane, Matrix) = (plane, matrix);

    public PlotPlane Plane { get; init; }
    public SKMatrix Matrix { get; init { field = value; inverse = value.TryInvert(out SKMatrix inverted) ? Some(inverted) : None; } }

    public bool Resolves => Plane.Dimensions.ForAll(held => held.Axis.Resolves(Pixels(held.Along)));

    public SKPoint Point(Vector2 value) => Matrix.MapPoint(Plane.Unit(value));

    public Option<Vector2> At(SKPoint local) => Valued(local).Map(Plane.Constrained);

    public Option<Vector2> Delta(SKPoint origin, SKPoint target) =>
        from start in Valued(origin)
        from end in Valued(target)
        select Plane.Dimensions.Fold(Vector2.Zero, (travel, held) => held.Along.With(travel, held.Axis.Delta(held.Along.Of(start), held.Along.Of(end))));

    public Option<Vector2> Moved(Vector2 held, SKPoint origin, SKPoint target, float gain) => Delta(origin, target).Map(travel => Plane.Constrained(held + (travel * gain)));

    public Vector2 Stepped(Vector2 held, PhysicalAxis along, int steps, float least) =>
        PlaneAxisOrFirst(along).Map(free => free.Along.With(held, free.Axis.Snapped(free.Along.Of(held), steps, least / Pixels(free.Along)))).IfNone(held);

    public Vector2 Ended(Vector2 held, PhysicalAxis along, int sign) =>
        PlaneAxisOrFirst(along).Map(free => free.Along.With(held, sign > 0 ? free.Axis.Domain.To : free.Axis.Domain.From)).IfNone(held);

    public Option<int> Nearest(Vector2 raw, Seq<Vector2> detents, float reach) {
        SKPoint at = Point(raw);
        return detents.Map((detent, index) => (Index: index, Distance: SKPoint.Distance(Point(detent), at))).Filter(held => held.Distance <= reach) switch {
            { IsEmpty: true } => None,
            var near => Some(near.MinBy(static held => held.Distance).Index),
        };
    }

    public float Pixels(PhysicalAxis along) => MathF.Abs(along.Of(new Vector2(Matrix.ScaleX, Matrix.ScaleY))) * Plane.Length(along);

    public PlotFrame Translated(SKPoint by) => this with { Matrix = Matrix.PostConcat(SKMatrix.CreateTranslation(by.X, by.Y)) };

    internal SKPoint Projected(Vector2 fractions) => Matrix.MapPoint(Plane.Projected(fractions));

    internal Seq<SKPoint> Projected(Seq<Vector2> fractions) => Plane.Projected(Matrix, fractions);

    internal Seq<SKPoint> Walked(Seq<Vector2> fractions, bool closed, Pen pen) =>
        Projected(Plane.Separable || fractions.IsEmpty ? fractions : PlotPlane.Densified(fractions, closed, PlotPlane.Density(Matrix, pen)));

    internal Seq<SKPoint> Traced(Seq<Vector2> values, bool closed, Pen pen) => Walked(values.Map(Plane.Fraction), closed, pen);

    internal Option<Vector2> Valued(SKPoint local) => inverse.Map(held => Plane.Value(held.MapPoint(local)));

    internal bool Contains(SKPoint local, Option<AxisRange> across, Option<AxisRange> up) =>
        Valued(local).Exists(at => Plane.Contains(PhysicalAxis.Horizontal, across, at.X) && Plane.Contains(PhysicalAxis.Vertical, up, at.Y));

    private Option<PlaneAxis> PlaneAxisOrFirst(PhysicalAxis along) => Plane.Dimension(along).Map(axis => new PlaneAxis(along, axis)) | Plane.Dimensions.Head;
}
