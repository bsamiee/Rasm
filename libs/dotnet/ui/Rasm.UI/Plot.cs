using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class AxisRange {
    public static AxisRange Unit { get; } = Create(0f, 1f);

    public float From { get; }
    public float To { get; }

    public float Span => To - From;
    public float Low => float.Min(From, To);
    public float High => float.Max(From, To);

    public Option<AxisRange> Scaled(float about, float factor) =>
        float.IsFinite(factor) && factor > 0f ? Ranged(about + ((From - about) * factor), about + ((To - about) * factor)) : None;

    public Option<AxisRange> Shifted(float delta) => Ranged(From + delta, To + delta);

    private static Option<AxisRange> Ranged(float from, float to) => TryCreate(from, to, out AxisRange? range) ? Some(range) : None;

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref float from, ref float to) =>
        validationError = float.IsFinite(from) && float.IsFinite(to) && from.CompareTo(to) is not 0
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"axis range ({from}, {to}) is empty or unbounded"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Axis {
    private Axis(AxisRange domain) => Domain = domain;

    public AxisRange Domain { get; init; }

    public virtual float Fraction(float value) => (value - Domain.From) / Domain.Span;

    public virtual float Value(float fraction) => Domain.From + (fraction * Domain.Span);

    public float Unit(float value) => Wrapped(Fraction(value));

    public virtual float Delta(float from, float to) => to - from;

    public virtual float Held(float value) => float.Clamp(value, Domain.Low, Domain.High);

    public virtual bool Contains(AxisRange band, float value) => value >= band.Low && value <= band.High;

    public virtual Seq<(float Low, float High)> Spans(AxisRange band) =>
        (Fraction(band.From), Fraction(band.To)) switch {
            var (from, to) => [(float.Min(from, to), float.Max(from, to))],
        };

    public virtual Option<Axis> Over(AxisRange range) => Some(this with { Domain = range });

    public int Decimals => new Linear(Domain).Step(0.01f).Map(static step => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(step)))).IfNone(0);

    public string Spoken(float value) => MathF.Round(value, Decimals, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);

    public abstract Option<float> Step(float least);
    public abstract Seq<float> Ticks(float least);
    public abstract float Snapped(float value, int steps, float least);

    internal bool Resolves(float pixels) =>
        float.Max(MathF.Abs(Domain.From), MathF.Abs(Domain.To)) switch {
            var largest => MathF.Abs(Domain.Span) >= pixels * (MathF.BitIncrement(largest) - largest),
        };

    private protected virtual float Wrapped(float fraction) => fraction;

    private protected float Ticked(float value, int steps, float least) =>
        Ticks(least).Map(static (tick, index) => (Tick: tick, Index: index)) switch {
            { IsEmpty: true } => value,
            var ticks => ticks.MinBy(held => MathF.Abs(Fraction(held.Tick) - Fraction(value))) switch {
                var nearest => ticks[int.Clamp(nearest.Index + steps, 0, ticks.Count - 1)].Tick,
            },
        };

    private static Option<float> Nice(float span) =>
        float.IsFinite(span) && span > 0f
            ? float.Exp10(MathF.Floor(MathF.Log10(span))) switch {
                var decade => Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= span),
            }
            : None;

    public sealed record Linear(AxisRange Domain) : Axis(Domain) {
        public override Option<float> Step(float least) => Nice(least * MathF.Abs(Domain.Span));

        public override Seq<float> Ticks(float least) =>
            Step(least).Map(step => ((int)MathF.Ceiling(Domain.Low / step), (int)MathF.Floor(Domain.High / step)) switch {
                var (first, last) => toSeq(Range(first, int.Max(0, last - first + 1))).Map(multiple => multiple * step),
            }).IfNone([]);

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Held((MathF.Round(value / step, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * step)).IfNone(value);
    }

    public sealed record Logarithmic(AxisRange Domain) : Axis(Domain) {
        private float Depth => float.LogP1(MathF.Abs(Domain.Span));

        public override float Fraction(float value) =>
            ((value - Domain.From) * MathF.Sign(Domain.Span)) switch {
                var along => MathF.CopySign(float.LogP1(MathF.Abs(along)), along) / Depth,
            };

        public override float Value(float fraction) => Domain.From + (MathF.Sign(Domain.Span) * MathF.CopySign(float.ExpM1(MathF.Abs(fraction) * Depth), fraction));

        public override Option<float> Step(float least) =>
            MathF.Ceiling(least * Depth / MathF.Log(10f)) switch {
                var decades when float.IsFinite(decades) => Some(float.Exp10(float.Max(1f, decades))),
                _ => None,
            };

        public override Seq<float> Ticks(float least) =>
            Step(least).Map(factor => Domain.From.Cons(toSeq(Range(0, int.Max(0, (int)MathF.Floor(MathF.Log10(MathF.Abs(Domain.Span)) / MathF.Log10(factor)) + 1)))
                .Map(power => Domain.From + (MathF.Sign(Domain.Span) * MathF.Pow(factor, power))))).IfNone([]);

        public override float Snapped(float value, int steps, float least) => Ticked(value, steps, least);
    }

    public sealed record Log(AxisRange Domain, float Base) : Axis(Domain.Low > 0f ? Domain : throw new ArgumentOutOfRangeException(nameof(Domain), Domain, "a log axis spans positive values alone")) {
        private const float Slack = 1e-4f;

        public float Base { get; } = Base > 1f ? Base : throw new ArgumentOutOfRangeException(nameof(Base), Base, "a log axis base exceeds one");

        private float Depth => MathF.Log(Domain.To / Domain.From);

        public override float Fraction(float value) => MathF.Log(value / Domain.From) / Depth;

        public override float Value(float fraction) => Domain.From * MathF.Exp(fraction * Depth);

        public override Option<Axis> Over(AxisRange range) => range.Low > 0f ? Some<Axis>(new Log(range, Base)) : None;

        public override Option<float> Step(float least) => None;

        public override Seq<float> Ticks(float least) {
            int first = (int)MathF.Ceiling(MathF.Log(Domain.Low, Base) - Slack);
            int last = (int)MathF.Floor(MathF.Log(Domain.High, Base) + Slack);
            float span = MathF.Abs(Depth);
            int stride = int.Max(1, (int)MathF.Ceiling(least * span / MathF.Log(Base)));
            Seq<float> majors = toSeq(Range(first, int.Max(0, last - first + 1))).Filter(power => power % stride is 0).Map(power => MathF.Pow(Base, power));
            Seq<float> minors = stride is 1 && MathF.Log(Base / (Base - 1f)) >= least * span
                ? from power in toSeq(Range(first - 1, int.Max(0, last - first + 2)))
                  from multiple in toSeq(Range(2, int.Max(0, (int)MathF.Ceiling(Base) - 2))).Filter(multiple => multiple < Base)
                  let tick = multiple * MathF.Pow(Base, power)
                  where tick >= Domain.Low && tick <= Domain.High
                  select tick
                : [];
            return toSeq((majors + minors).OrderBy(Fraction));
        }

        public override float Snapped(float value, int steps, float least) => Held(Ticked(value, steps, least));
    }

    public sealed record Scale(AxisRange Domain, Func<float, AxisRange, float> Forward, Func<float, AxisRange, float> Inverse) : Axis(Domain) {
        public override float Fraction(float value) => Forward(value, Domain);

        public override float Value(float fraction) => Inverse(fraction, Domain);

        public override Option<float> Step(float least) => None;

        public override Seq<float> Ticks(float least) =>
            least > 0f ? toSeq(Range(0, (int)MathF.Floor(1f / least) + 1)).Map(index => Snapped(Value(index * least), 0, least)).Distinct() : [];

        public override float Snapped(float value, int steps, float least) =>
            Nice(MathF.Abs(Value(Fraction(value) + least) - value)).Map(step => Held((MathF.Round(value / step, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * step)).IfNone(value);
    }

    public sealed record Periodic(AxisRange Domain, int Grid, int Snap) : Axis(Domain) {
        private float Turn => MathF.Abs(Domain.Span);

        public override float Delta(float from, float to) => (to - from) switch {
            var delta => delta - (Turn * MathF.Round(delta / Turn, MidpointRounding.ToEven)),
        };

        public override float Held(float value) => Value(Unit(value));

        public override bool Contains(AxisRange band, float value) => band.Low + (Wrapped((value - band.Low) / Turn) * Turn) <= band.High;

        public override Seq<(float Low, float High)> Spans(AxisRange band) =>
            (From: Fraction(band.From), To: Fraction(band.To)) switch {
                var whole when MathF.Abs(whole.To - whole.From) >= 1f => [(0f, 1f)],
                var (from, to) => (Wrapped(from), Wrapped(to)) switch {
                    var (low, high) when high > low => [(low, high)],
                    var (low, high) when high > 0f => [(low, 1f), (0f, high)],
                    var (low, _) => [(low, 1f)],
                },
            };

        public override Option<float> Step(float least) => Snap > 0 ? Some(Domain.Span / Snap) : None;

        public override Seq<float> Ticks(float least) =>
            toSeq(Range(1, int.Max(1, Grid))).Rev().Filter(divisor => Grid % divisor is 0).Find(divisor => 1f / divisor >= least).IfNone(1) switch {
                var divisions => toSeq(Range(0, divisions)).Map(index => Domain.From + (index * Domain.Span / divisions)),
            };

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Held(Domain.From + ((MathF.Round((value - Domain.From) / step, MidpointRounding.ToEven) + steps) * step))).IfNone(value);

        private protected override float Wrapped(float fraction) => ((fraction % 1f) + 1f) % 1f;
    }
}

public sealed record Pen(SKMatrix Device, float Stroke, float Spacing) {
    public static Pen Hit { get; } = new(SKMatrix.Identity, 0f, 0f);

    internal SKPoint Snapped(SKPoint local) =>
        Device.MapPoint(local) switch {
            var device => Local(new SKPoint(Grid(device.X, 0f), Grid(device.Y, 0f)), local),
        };

    internal (SKPoint From, SKPoint To) Snapped(SKPoint from, SKPoint to) =>
        (Start: Device.MapPoint(from), End: Device.MapPoint(to), Half: MathF.Round(Device.MapRadius(Stroke), MidpointRounding.ToEven) / 2f) switch {
            var held when held.Start.X.CompareTo(held.End.X) is 0 =>
                (Local(held.Start with { X = Grid(held.Start.X, held.Half) }, from), Local(held.End with { X = Grid(held.End.X, held.Half) }, to)),
            var held when held.Start.Y.CompareTo(held.End.Y) is 0 =>
                (Local(held.Start with { Y = Grid(held.Start.Y, held.Half) }, from), Local(held.End with { Y = Grid(held.End.Y, held.Half) }, to)),
            _ => (from, to),
        };

    private static float Grid(float device, float offset) => MathF.Round(device - offset, MidpointRounding.ToEven) + offset;

    private SKPoint Local(SKPoint device, SKPoint local) => Device.TryInvert(out SKMatrix inverse) ? inverse.MapPoint(device) : local;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotPlane {
    public abstract Option<Axis> Dimension(PhysicalAxis along);

    public virtual SKMatrix Frame(SKRect content) => SKMatrix.CreateScaleTranslation(content.Width, -content.Height, content.Left, content.Bottom);

    public virtual Option<Factor> Aspect => None;

    public PlotFrame In(SKRect content) => new(this, Frame(content));

    public SKPoint Unit(Vector2 value) => Placed(Each(value, static (axis, held) => axis.Unit(held)));

    public Vector2 Value(SKPoint square) => Each(Unplaced(square), static (axis, held) => axis.Value(held));

    public Vector2 Held(Vector2 value) => Each(value, static (axis, held) => axis.Held(held));

    public Seq<(PhysicalAxis Along, Axis Axis)> Dimensions => toSeq(PhysicalAxis.Items).Choose(along => Dimension(along).Map(axis => (Along: along, Axis: axis)));

    public string Spoken(Vector2 value) => string.Join(", ", Dimensions.Map(held => held.Axis.Spoken(held.Along.Of(value))));

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

    internal virtual void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<(float Low, float High)> across, Seq<(float Low, float High)> up) =>
        _ = (from x in across
             from y in up
             select frame.MapRect(new SKRect(x.Low, y.Low, x.High, y.High)))
            .Iter(cell => {
                SKPoint low = pen.Snapped(cell.Location);
                SKPoint high = pen.Snapped(new SKPoint(cell.Right, cell.Bottom));
                path.AddRect(new SKRect(low.X, low.Y, high.X, high.Y));
            });

    internal float Fraction(PhysicalAxis along, float value) => Dimension(along).Map(axis => axis.Fraction(value)).IfNone(value);

    internal Seq<(float Low, float High)> Spans(PhysicalAxis along, Option<AxisRange> band) =>
        band.Match(
            Some: held => Dimension(along).Match(Some: axis => axis.Spans(held), None: () => [(held.Low, held.High)]),
            None: static () => [(0f, 1f)]);

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

        internal override SKPoint Placed(Vector2 fractions) =>
            MathF.SinCos(MathF.Tau * fractions.X) switch {
                var (sin, cos) => new Vector2(0.5f) + (new Vector2(cos, sin) * (0.5f * fractions.Y)),
            };

        internal override Vector2 Unplaced(SKPoint square) =>
            ((Vector2)square - new Vector2(0.5f)) switch {
                var offset => new Vector2(MathF.Atan2(offset.Y, offset.X) / MathF.Tau, 2f * offset.Length()),
            };

        internal override float Length(PhysicalAxis along) => along.Map(horizontal: MathF.PI, vertical: 0.5f);

        internal override void Trace(SKPathBuilder path, SKMatrix frame, Pen pen, PhysicalAxis held, float at) =>
            held.Switch(
                (Path: path, Center: frame.MapPoint(Placed(Vector2.Zero)), Rim: frame.MapPoint(Placed(new Vector2(at, 1f))), Radius: Half(frame) * at),
                horizontal: static ray => {
                    ray.Path.MoveTo(ray.Center);
                    ray.Path.LineTo(ray.Rim);
                },
                vertical: static ring => ring.Path.AddCircle(ring.Center.X, ring.Center.Y, ring.Radius));

        internal override void Enclose(SKPathBuilder path, SKMatrix frame, Pen pen, Seq<(float Low, float High)> across, Seq<(float Low, float High)> up) {
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

public sealed record PlotFrame(PlotPlane Plane, SKMatrix Matrix) {
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

    public Option<int> Nearest(Vector2 raw, Seq<Vector2> detents, float reach) =>
        Point(raw) switch {
            var at => detents.Map((detent, index) => (Index: index, Distance: SKPoint.Distance(Point(detent), at))).Filter(held => held.Distance <= reach) switch {
                { IsEmpty: true } => None,
                var near => Some(near.MinBy(static held => held.Distance).Index),
            },
        };

    public float Pixels(PhysicalAxis along) => MathF.Abs(along.Of(new Vector2(Matrix.ScaleX, Matrix.ScaleY))) * Plane.Length(along);

    public PlotFrame Translated(SKPoint by) => this with { Matrix = Matrix.PostConcat(SKMatrix.CreateTranslation(by.X, by.Y)) };

    internal SKPoint Placed(Vector2 fractions) => Matrix.MapPoint(Plane.Placed(fractions));

    private Option<(PhysicalAxis Along, Axis Axis)> Free(PhysicalAxis along) => Plane.Dimension(along).Map(axis => (Along: along, Axis: axis)) | Plane.Dimensions.Head;

    private Option<SKPoint> Square(SKPoint local) => Matrix.TryInvert(out SKMatrix inverse) ? Some(inverse.MapPoint(local)) : None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Figure {
    private static readonly Func<ReadOnlySpan<float>, Seq<int>> Extremes = static column => toSeq(Seq(TensorPrimitives.IndexOfMin(column), TensorPrimitives.IndexOfMax(column)).Order());
    private static readonly Func<ReadOnlySpan<float>, Seq<int>> Peaks = static column => Seq(TensorPrimitives.IndexOfMax(column));

    public abstract void Edges(SKPathBuilder path, PlotFrame frame, Pen pen);

    public virtual void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) { }

    public virtual bool Hits(PlotFrame frame, SKPoint local, float reach) {
        using SKPath interior = Traced(path => Interior(path, frame, Pen.Hit));
        using SKPath outline = Traced(path => {
            Interior(path, frame, Pen.Hit);
            Edges(path, frame, Pen.Hit);
        });
        using SKPaint pen = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 2f * reach, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
        using SKPath? reached = pen.GetFillPath(outline);
        return interior.Contains(local.X, local.Y) || (reached?.Contains(local.X, local.Y) ?? false);
    }

    internal static SKPath Traced(Action<SKPathBuilder> add) {
        using SKPathBuilder builder = new();
        add(builder);
        return builder.Detach();
    }

    private static Seq<Vector2> Sampled(PlotFrame frame, Pen pen, AxisRange domain, Seq<float> samples, Func<ReadOnlySpan<float>, Seq<int>> kept) {
        float[] across = [.. samples.Map((_, index) => frame.Plane.Fraction(PhysicalAxis.Horizontal, samples.Count > 1 ? domain.From + (index * domain.Span / (samples.Count - 1)) : domain.From))];
        float[] up = [.. samples.Map(sample => frame.Plane.Fraction(PhysicalAxis.Vertical, sample))];
        float columns = frame.Pixels(PhysicalAxis.Horizontal) * pen.Device.MapRadius(1f);
        int[] column = [.. across.Select(fraction => (int)MathF.Floor(fraction * columns))];
        Seq<int> starts = toSeq(Range(0, up.Length)).Filter(index => index is 0 || column[index] != column[index - 1]);
        return up.Length is 0
            ? []
            : (Seq(0)
               + (from run in starts.Zip(starts.Tail.Add(up.Length))
                  from offset in kept(up.AsSpan(run.First, run.Second - run.First))
                  select run.First + offset)
               + Seq(up.Length - 1))
                .Distinct().Map(index => new Vector2(across[index], up[index]));
    }

    public sealed record Rule(Vector2 From, Vector2 To) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) {
            (SKPoint from, SKPoint to) = pen.Snapped(frame.Point(From), frame.Point(To));
            path.MoveTo(from);
            path.LineTo(to);
        }
    }

    public sealed record Polyline(Seq<Vector2> Points) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => path.AddPoly([.. Points.Map(frame.Point)], close: false);
    }

    public sealed record Polygon(Seq<Vector2> Points) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Interior(path, frame, pen);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) => path.AddPoly([.. Points.Map(frame.Point)], close: true);
    }

    public sealed record Region(Option<AxisRange> First, Option<AxisRange> Second) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => Interior(path, frame, pen);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            frame.Plane.Enclose(path, frame.Matrix, pen, frame.Plane.Spans(PhysicalAxis.Horizontal, First), frame.Plane.Spans(PhysicalAxis.Vertical, Second));
    }

    public sealed record Curve(AxisRange Domain, Seq<float> Samples) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => path.AddPoly([.. Sampled(frame, pen, Domain, Samples, Extremes).Map(frame.Placed)], close: false);
    }

    public sealed record Area(AxisRange Domain, Seq<float> Samples) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => path.AddPoly([.. Sampled(frame, pen, Domain, Samples, Extremes).Map(frame.Placed)], close: false);

        public override void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) {
            Seq<Vector2> envelope = Sampled(frame, pen, Domain, Samples, Peaks);
            _ = (from first in envelope.Head
                 from last in envelope.Last
                 select envelope.Add(last with { Y = 0f }).Add(first with { Y = 0f }))
                .Iter(outline => path.AddPoly([.. outline.Map(frame.Placed)], close: true));
        }
    }

    public sealed record Graph(AxisRange Domain, Func<float, float> Fit) : Figure {
        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) => new Curve(Domain, Tabulated(frame, pen)).Edges(path, frame, pen);

        private Seq<float> Tabulated(PlotFrame frame, Pen pen) =>
            MathF.Abs(frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.To) - frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.From)) switch {
                var spanned => int.Max(2, (int)MathF.Ceiling(spanned * frame.Pixels(PhysicalAxis.Horizontal) * pen.Device.MapRadius(1f)) + 1) switch {
                    var count => toSeq(Range(0, count)).Map(index => Fit(Domain.From + (index * Domain.Span / (count - 1)))),
                },
            };
    }

    public sealed record Grid(PhysicalAxis Along) : Figure {
        public Option<Axis> Ticks { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = (Ticks | frame.Plane.Dimension(Along)).Iter(axis => axis.Ticks(pen.Spacing / frame.Pixels(Along)).Map(axis.Fraction).Iter(at => frame.Plane.Trace(path, frame.Matrix, pen, Along, at)));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => false;
    }
}
