using System.Globalization;
using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Dimension(PhysicalAxis Along, Axis Axis);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotPlane {
    public abstract Option<Axis> Dimension(PhysicalAxis along);

    public virtual SKMatrix Frame(SKRect content) => SKMatrix.CreateScaleTranslation(content.Width, -content.Height, content.Left, content.Bottom);

    public virtual Option<Factor> Aspect => None;

    public PlotFrame In(SKRect content) => new(this, Frame(content));

    public SKPoint Unit(Vector2 value) => Placed(Each(value, static (axis, held) => axis.Unit(held)));

    public Vector2 Value(SKPoint square) => Each(Unplaced(square), static (axis, held) => axis.Value(held));

    public Vector2 Held(Vector2 value) => Each(value, static (axis, held) => axis.Held(held));

    public Seq<Dimension> Dimensions => toSeq(PhysicalAxis.Items).Choose(along => Dimension(along).Map(axis => new Dimension(along, axis)));

    public string Spoken(Vector2 at, CultureInfo culture) => string.Join($"{culture.TextInfo.ListSeparator} ", Dimensions.Map(held => held.Axis.Spoken(held.Along.Of(at), culture)));

    public Option<PlotPlane> Shifted(Vector2 delta) => Mapped((along, axis) => axis.Domain.Shifted(along.Of(delta)).Bind(axis.Over));

    public Option<PlotPlane> Scaled(Vector2 about, float factor) => Mapped((along, axis) => axis.Domain.Scaled(along.Of(about), factor).Bind(axis.Over));

    public virtual Option<Vector2> Turned(Vector2 value, float radians) => None;

    internal virtual SKPoint Placed(Vector2 fractions) => fractions;

    internal virtual Vector2 Unplaced(SKPoint square) => square;

    internal virtual float Length(PhysicalAxis along) => 1f;

    internal virtual void Trace(SKPathBuilder path, SKMatrix frame, Pen pen, PhysicalAxis held, float at) {
        (SKPoint from, SKPoint to) = pen.Snapped(frame.MapPoint(held.With(Vector2.Zero, at)), frame.MapPoint(held.With(Vector2.One, at)));
        path.MoveTo(from);
        path.LineTo(to);
    }

    internal virtual void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<Stretch> across, Seq<Stretch> up) =>
        _ = (from x in across
             from y in up
             select frame.MapRect(new SKRect(x.Low, y.Low, x.High, y.High)))
            .Iter(cell => {
                SKPoint low = pen.Snapped(cell.Location);
                SKPoint high = pen.Snapped(new SKPoint(cell.Right, cell.Bottom));
                path.AddRect(new SKRect(low.X, low.Y, high.X, high.Y));
            });

    internal float Fraction(PhysicalAxis along, float value) => Dimension(along).Map(axis => axis.Fraction(value)).IfNone(value);

    internal Seq<Stretch> Spans(PhysicalAxis along, Option<AxisRange> band) =>
        band.Match(
            Some: held => Dimension(along).Match(Some: axis => axis.Spans(held), None: () => [new Stretch(held.Low, held.High)]),
            None: static () => [new Stretch(0f, 1f)]);

    private protected abstract Option<PlotPlane> Mapped(Func<PhysicalAxis, Axis, Option<Axis>> map);

    private float Of(PhysicalAxis along, float value, Func<Axis, float, float> map) => Dimension(along).Map(axis => map(axis, value)).IfNone(value);

    private Vector2 Each(Vector2 value, Func<Axis, float, float> map) => new(Of(PhysicalAxis.Horizontal, value.X, map), Of(PhysicalAxis.Vertical, value.Y, map));

    public sealed record Cartesian(Axis X, Axis Y) : PlotPlane {
        public override Option<Axis> Dimension(PhysicalAxis along) => Some(along.Map(horizontal: X, vertical: Y));

        private protected override Option<PlotPlane> Mapped(Func<PhysicalAxis, Axis, Option<Axis>> map) =>
            from x in map(PhysicalAxis.Horizontal, X)
            from y in map(PhysicalAxis.Vertical, Y)
            select (PlotPlane)new Cartesian(x, y);
    }

    public sealed record Track(PhysicalAxis Along, Axis Axis) : PlotPlane {
        public static Track Unit(PhysicalAxis along) => new(along, new Axis.Linear(AxisRange.Unit));

        public override Option<Axis> Dimension(PhysicalAxis along) => along == Along ? Some(Axis) : None;

        private protected override Option<PlotPlane> Mapped(Func<PhysicalAxis, Axis, Option<Axis>> map) => map(Along, Axis).Map(axis => (PlotPlane)(this with { Axis = axis }));
    }

    public sealed record Polar(Axis Angle, Option<Axis> Radius) : PlotPlane {
        public override SKMatrix Frame(SKRect content) => base.Frame(content.AspectFit(new SKSize(1f, 1f)));

        public override Option<Factor> Aspect => Some(Factor.Unit);

        public override Option<Axis> Dimension(PhysicalAxis along) => along.Map(horizontal: Some(Angle), vertical: Radius);

        public override Option<Vector2> Turned(Vector2 value, float radians) => Some(value with { X = Angle.Held(value.X + (radians / MathF.Tau * Angle.Domain.Span)) });

        internal override SKPoint Placed(Vector2 fractions) {
            (float sin, float cos) = MathF.SinCos(MathF.Tau * fractions.X);
            return new Vector2(0.5f) + (new Vector2(cos, sin) * (0.5f * fractions.Y));
        }

        internal override Vector2 Unplaced(SKPoint square) {
            Vector2 offset = (Vector2)square - new Vector2(0.5f);
            return new Vector2(MathF.Atan2(offset.Y, offset.X) / MathF.Tau, 2f * offset.Length());
        }

        internal override float Length(PhysicalAxis along) => along.Map(horizontal: MathF.PI, vertical: 0.5f);

        internal override void Trace(SKPathBuilder path, SKMatrix frame, Pen pen, PhysicalAxis held, float at) =>
            held.Switch(
                (Path: path, Center: frame.MapPoint(Placed(Vector2.Zero)), Rim: frame.MapPoint(Placed(new Vector2(at, 1f))), Radius: Half(frame) * at),
                horizontal: static ray => {
                    ray.Path.MoveTo(ray.Center);
                    ray.Path.LineTo(ray.Rim);
                },
                vertical: static ring => ring.Path.AddCircle(ring.Center.X, ring.Center.Y, ring.Radius));

        internal override void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<Stretch> across, Seq<Stretch> up) {
            SKPoint center = frame.MapPoint(Placed(Vector2.Zero));
            float half = Half(frame);
            float extent = across.Fold(0f, static (sum, span) => sum + (span.High - span.Low));
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

        private protected override Option<PlotPlane> Mapped(Func<PhysicalAxis, Axis, Option<Axis>> map) =>
            from angle in map(PhysicalAxis.Horizontal, Angle)
            from radius in Radius.Match(Some: held => map(PhysicalAxis.Vertical, held).Map(Some), None: static () => Some(Option<Axis>.None))
            select (PlotPlane)new Polar(angle, radius);

        private static float Half(SKMatrix frame) => MathF.Abs(frame.ScaleX) / 2f;

        private static SKRect Oval(SKPoint center, float radius) => SKRect.Create(center.X - radius, center.Y - radius, 2f * radius, 2f * radius);
    }
}

public sealed record PlotFrame {
    private readonly Option<SKMatrix> inverse;

    public PlotFrame(PlotPlane plane, SKMatrix matrix) => (Plane, Matrix) = (plane, matrix);

    public PlotPlane Plane { get; init; }
    public SKMatrix Matrix { get; init { field = value; inverse = value.TryInvert(out SKMatrix inverted) ? Some(inverted) : None; } }

    public bool Resolves => Plane.Dimensions.ForAll(held => held.Axis.Resolves(Pixels(held.Along)));

    public SKPoint Point(Vector2 value) => Matrix.MapPoint(Plane.Unit(value));

    public Option<Vector2> At(SKPoint local) => Square(local).Map(square => Plane.Held(Plane.Value(square)));

    public Option<Vector2> Travel(SKPoint origin, SKPoint target) =>
        from start in Square(origin).Map(Plane.Value)
        from end in Square(target).Map(Plane.Value)
        select Plane.Dimensions.Fold(Vector2.Zero, (travel, held) => held.Along.With(travel, held.Axis.Delta(held.Along.Of(start), held.Along.Of(end))));

    public Option<Vector2> Moved(Vector2 held, SKPoint origin, SKPoint target, float gain) => Travel(origin, target).Map(travel => Plane.Held(held + (travel * gain)));

    public Vector2 Stepped(Vector2 held, PhysicalAxis along, int steps, float least) =>
        Free(along).Map(free => free.Along.With(held, free.Axis.Snapped(free.Along.Of(held), steps, least / Pixels(free.Along)))).IfNone(held);

    public Vector2 Ended(Vector2 held, PhysicalAxis along, int sign) =>
        Free(along).Map(free => free.Along.With(held, sign > 0 ? free.Axis.Domain.To : free.Axis.Domain.From)).IfNone(held);

    public Option<int> Nearest(Vector2 raw, Seq<Vector2> detents, float reach) {
        SKPoint at = Point(raw);
        return detents.Map((detent, index) => (Index: index, Distance: SKPoint.Distance(Point(detent), at))).Filter(held => held.Distance <= reach) switch {
            { IsEmpty: true } => None,
            var near => Some(near.MinBy(static held => held.Distance).Index),
        };
    }

    public float Pixels(PhysicalAxis along) => MathF.Abs(along.Of(new Vector2(Matrix.ScaleX, Matrix.ScaleY))) * Plane.Length(along);

    public PlotFrame Translated(SKPoint by) => this with { Matrix = Matrix.PostConcat(SKMatrix.CreateTranslation(by.X, by.Y)) };

    internal SKPoint Placed(Vector2 fractions) => Matrix.MapPoint(Plane.Placed(fractions));

    private Option<Dimension> Free(PhysicalAxis along) => Plane.Dimension(along).Map(axis => new Dimension(along, axis)) | Plane.Dimensions.Head;

    private Option<SKPoint> Square(SKPoint local) => inverse.Map(held => held.MapPoint(local));
}
