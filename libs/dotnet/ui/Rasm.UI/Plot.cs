using System.Globalization;
using System.Numerics;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class AxisRange {
    public float From { get; }
    public float To { get; }

    public float Span => To - From;
    public float Low => float.Min(From, To);
    public float High => float.Max(From, To);

    public Option<AxisRange> Scaled(float about, float factor) =>
        float.IsFinite(factor) && factor > 0f ? Ranged(about + ((From - about) * factor), about + ((To - about) * factor)) : None;

    public Option<AxisRange> Shifted(float delta) => Ranged(From + delta, To + delta);

    private static Option<AxisRange> Ranged(float from, float to) => ValidationResults.ToFin(Validate(from, to, out AxisRange? range), range).ToOption();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float from, ref float to) =>
        validationError = float.IsFinite(from) && float.IsFinite(to) && from.CompareTo(to) is not 0
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"axis range ({from}, {to}) is empty or unbounded"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Axis {
    private Axis(AxisRange range) => Range = range;

    public AxisRange Range { get; init; }

    public virtual float Fraction(float value) => (value - Range.From) / Range.Span;

    public virtual float Value(float fraction) => Range.From + (fraction * Range.Span);

    public float Unit(float value) =>
        Fraction(value) switch {
            var fraction => this is Periodic ? Wrapped(fraction) : fraction,
        };

    public virtual float Delta(float from, float to) => to - from;

    public virtual float Held(float value) => float.Clamp(value, Range.Low, Range.High);

    public virtual bool Contains(AxisRange band, float value) => value >= band.Low && value <= band.High;

    public abstract Option<float> Step(float least);
    public abstract Seq<float> Ticks(float least);
    public abstract float Snapped(float value, int steps, float least);

    internal bool Resolves(float pixels) =>
        float.Max(MathF.Abs(Range.From), MathF.Abs(Range.To)) switch {
            var largest => MathF.Abs(Range.Span) >= pixels * (MathF.BitIncrement(largest) - largest),
        };

    private static float Wrapped(float fraction) => ((fraction % 1f) + 1f) % 1f;

    private static Option<float> Nice(float span) =>
        float.IsFinite(span) && span > 0f
            ? float.Exp10(MathF.Floor(MathF.Log10(span))) switch {
                var decade => Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= span),
            }
            : None;

    public sealed record Linear(AxisRange Range) : Axis(Range) {
        public override Option<float> Step(float least) => Nice(least * MathF.Abs(Range.Span));

        public override Seq<float> Ticks(float least) =>
            Step(least).Map(step => ((int)MathF.Ceiling(Range.Low / step), (int)MathF.Floor(Range.High / step)) switch {
                var (first, last) => toSeq(Prelude.Range(first, int.Max(0, last - first + 1))).Map(multiple => multiple * step),
            }).IfNone([]);

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Held((MathF.Round(value / step, MidpointRounding.ToEven) + steps) * step)).IfNone(value);
    }

    public sealed record Logarithmic(AxisRange Range) : Axis(Range) {
        private float Depth => float.LogP1(MathF.Abs(Range.Span));

        public override float Fraction(float value) =>
            ((value - Range.From) * MathF.Sign(Range.Span)) switch {
                var along => MathF.CopySign(float.LogP1(MathF.Abs(along)), along) / Depth,
            };

        public override float Value(float fraction) => Range.From + (MathF.Sign(Range.Span) * MathF.CopySign(float.ExpM1(MathF.Abs(fraction) * Depth), fraction));

        public override Option<float> Step(float least) =>
            MathF.Ceiling(least * Depth / MathF.Log(10f)) switch {
                var decades when float.IsFinite(decades) => Some(float.Exp10(float.Max(1f, decades))),
                _ => None,
            };

        public override Seq<float> Ticks(float least) =>
            Step(least).Map(factor => Range.From.Cons(toSeq(Prelude.Range(0, int.Max(0, (int)MathF.Floor(MathF.Log10(MathF.Abs(Range.Span)) / MathF.Log10(factor)) + 1)))
                .Map(power => Range.From + (MathF.Sign(Range.Span) * MathF.Pow(factor, power))))).IfNone([]);

        public override float Snapped(float value, int steps, float least) =>
            Ticks(least).Map(static (tick, index) => (Tick: tick, Index: index)) switch {
                { IsEmpty: true } => value,
                var ticks => ticks.MinBy(held => MathF.Abs(Fraction(held.Tick) - Fraction(value))) switch {
                    var nearest => ticks[int.Clamp(nearest.Index + steps, 0, ticks.Count - 1)].Tick,
                },
            };
    }

    public sealed record Scale(AxisRange Range, Func<double, double, double, double> Position, Func<double, double, double, double> Key) : Axis(Range) {
        public override float Fraction(float value) => (float)Position(value, Range.From, Range.To);

        public override float Value(float fraction) => (float)Key(fraction, Range.From, Range.To);

        public override Option<float> Step(float least) => None;

        public override Seq<float> Ticks(float least) =>
            least > 0f ? toSeq(Prelude.Range(0, (int)MathF.Floor(1f / least) + 1)).Map(index => Snapped(Value(index * least), 0, least)).Distinct() : [];

        public override float Snapped(float value, int steps, float least) =>
            Nice(MathF.Abs(Value(Fraction(value) + least) - value)).Map(step => Held((MathF.Round(value / step, MidpointRounding.ToEven) + steps) * step)).IfNone(value);
    }

    public sealed record Periodic(AxisRange Range, int Grid, int Snap) : Axis(Range) {
        private float Turn => MathF.Abs(Range.Span);

        public override float Delta(float from, float to) => (to - from) switch {
            var delta => delta - (Turn * MathF.Round(delta / Turn, MidpointRounding.ToEven)),
        };

        public override float Held(float value) => Value(Unit(value));

        public override Option<float> Step(float least) => Snap > 0 ? Some(Range.Span / Snap) : None;

        public override Seq<float> Ticks(float least) =>
            toSeq(Prelude.Range(1, int.Max(1, Grid))).Rev().Filter(divisor => Grid % divisor is 0).Find(divisor => 1f / divisor >= least).IfNone(1) switch {
                var divisions => toSeq(Prelude.Range(0, divisions)).Map(index => Range.From + (index * Range.Span / divisions)),
            };

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Held(Range.From + ((MathF.Round((value - Range.From) / step, MidpointRounding.ToEven) + steps) * step))).IfNone(value);

        public override bool Contains(AxisRange band, float value) => band.Low + (Wrapped((value - band.Low) / Turn) * Turn) <= band.High;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotPlane {
    public abstract Option<Axis> Dimension(Orientation along);

    public virtual SKMatrix Frame(SKRect content) => SKMatrix.CreateScaleTranslation(content.Width, -content.Height, content.Left, content.Bottom);

    public SKPoint Unit(Vector2 value) => Placed(Each(value, static (axis, held) => axis.Unit(held)));

    public Vector2 Value(SKPoint square) => Each(Unplaced(square), static (axis, held) => axis.Value(held));

    public Vector2 Held(Vector2 value) => Each(value, static (axis, held) => axis.Held(held));

    public Option<Vector2> At(SKMatrix frame, SKPoint local) => Local(frame, local).Map(square => Held(Value(square)));

    public Option<Vector2> Travel(SKPoint origin, SKPoint target, SKMatrix frame) =>
        from start in Local(frame, origin).Map(Value)
        from end in Local(frame, target).Map(Value)
        select new Vector2(Delta(Orientation.Horizontal, start.X, end.X), Delta(Orientation.Vertical, start.Y, end.Y));

    public Option<Vector2> Moved(Vector2 held, SKPoint origin, SKPoint target, SKMatrix frame, float gain) =>
        Travel(origin, target, frame).Map(travel => Held(held + (travel * gain)));

    public Vector2 Stepped(Vector2 held, Orientation along, int steps, SKMatrix frame, float least) =>
        Free(along) switch {
            var free => Dimension(free).Map(axis => free.With(held, axis.Snapped(free.Of(held), steps, least / Pixels(free, frame)))).IfNone(held),
        };

    public Vector2 Ended(Vector2 held, Orientation along, Heading toward) =>
        Free(along) switch {
            var free => Dimension(free).Map(axis => free.With(held, toward is Heading.Back ? axis.Range.From : axis.Range.To)).IfNone(held),
        };

    public Option<int> Nearest(Vector2 raw, Seq<Vector2> detents, SKMatrix frame, float reach) =>
        frame.MapPoint(Unit(raw)) switch {
            var at => detents.Map((detent, index) => (Index: index, Distance: SKPoint.Distance(frame.MapPoint(Unit(detent)), at))).Filter(held => held.Distance <= reach) switch {
                var near => near.IsEmpty ? None : Some(near.MinBy(static held => held.Distance).Index),
            },
        };

    public Seq<(Orientation Along, Axis Axis)> Dimensions => toSeq(Orientation.Items).Choose(along => Dimension(along).Map(axis => (Along: along, Axis: axis)));

    public Option<PlotPlane> Shifted(Vector2 delta) => Mapped((along, axis) => axis.Range.Shifted(along.Of(delta)).Map(range => axis with { Range = range }));

    public Option<PlotPlane> Scaled(Vector2 about, float factor) => Mapped((along, axis) => axis.Range.Scaled(along.Of(about), factor).Map(range => axis with { Range = range }));

    public bool Resolves(SKMatrix frame) => toSeq(Orientation.Items).ForAll(along => Dimension(along).ForAll(axis => axis.Resolves(Pixels(along, frame))));

    internal virtual SKPoint Placed(Vector2 fractions) => fractions;

    internal virtual Vector2 Unplaced(SKPoint square) => square;

    internal virtual float Pixels(Orientation along, SKMatrix frame) => MathF.Abs(along.Of(new Vector2(frame.ScaleX, frame.ScaleY)));

    private protected abstract Option<PlotPlane> Mapped(Func<Orientation, Axis, Option<Axis>> map);

    internal float Fraction(Orientation along, float value) => Of(along, value, static (axis, held) => axis.Fraction(held));

    internal bool Periodic(Orientation along) => Dimension(along).Exists(static axis => axis is Axis.Periodic);

    private float Of(Orientation along, float value, Func<Axis, float, float> map) => Dimension(along).Map(axis => map(axis, value)).IfNone(value);

    private Vector2 Each(Vector2 value, Func<Axis, float, float> map) => new(Of(Orientation.Horizontal, value.X, map), Of(Orientation.Vertical, value.Y, map));

    private float Delta(Orientation along, float from, float to) => Dimension(along).Map(axis => axis.Delta(from, to)).IfNone(0f);

    private Orientation Free(Orientation along) =>
        Dimension(along).IsSome ? along : Dimension(Orientation.Horizontal).IsSome ? Orientation.Horizontal : Orientation.Vertical;

    private static Option<SKPoint> Local(SKMatrix frame, SKPoint local) => frame.TryInvert(out SKMatrix inverse) ? Some(inverse.MapPoint(local)) : None;

    public sealed record Cartesian(Axis X, Axis Y) : PlotPlane {
        public override Option<Axis> Dimension(Orientation along) => Some(along.Map(horizontal: X, vertical: Y));

        private protected override Option<PlotPlane> Mapped(Func<Orientation, Axis, Option<Axis>> map) =>
            from x in map(Orientation.Horizontal, X)
            from y in map(Orientation.Vertical, Y)
            select (PlotPlane)new Cartesian(x, y);
    }

    public sealed record Track(Orientation Along, Axis Axis) : PlotPlane {
        public override Option<Axis> Dimension(Orientation along) => along == Along ? Some(Axis) : None;

        private protected override Option<PlotPlane> Mapped(Func<Orientation, Axis, Option<Axis>> map) => map(Along, Axis).Map(axis => (PlotPlane)(this with { Axis = axis }));
    }

    public sealed record Polar(Axis Angle, Option<Axis> Radius) : PlotPlane {
        public override SKMatrix Frame(SKRect content) => base.Frame(content.AspectFit(new SKSize(1f, 1f)));

        public override Option<Axis> Dimension(Orientation along) => along.Map(horizontal: Some(Angle), vertical: Radius);

        internal override SKPoint Placed(Vector2 fractions) =>
            MathF.SinCos(MathF.Tau * fractions.X) switch {
                var (sin, cos) => new Vector2(0.5f) + (new Vector2(cos, sin) * (0.5f * fractions.Y)),
            };

        internal override Vector2 Unplaced(SKPoint square) =>
            ((Vector2)square - new Vector2(0.5f)) switch {
                var offset => new Vector2(MathF.Atan2(offset.Y, offset.X) / MathF.Tau, 2f * offset.Length()),
            };

        internal override float Pixels(Orientation along, SKMatrix frame) => MathF.Abs(frame.ScaleX) * along.Map(horizontal: MathF.PI, vertical: 0.5f);

        private protected override Option<PlotPlane> Mapped(Func<Orientation, Axis, Option<Axis>> map) =>
            from angle in map(Orientation.Horizontal, Angle)
            from radius in Radius.Match(Some: held => map(Orientation.Vertical, held).Map(Some), None: static () => Some(Option<Axis>.None))
            select (PlotPlane)new Polar(angle, radius);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Figure {
    public abstract void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing);
    public virtual void Interior(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale) { }

    public virtual bool Hits(PlotPlane plane, SKMatrix frame, SKPoint local, float reach) {
        using SKPath interior = Traced(path => Interior(path, plane, frame, 1f));
        using SKPath outline = Traced(path => {
            Interior(path, plane, frame, 1f);
            Edges(path, plane, frame, 1f, 0f, 0f);
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

    private static SKPoint Mapped(PlotPlane plane, SKMatrix frame, Vector2 value) => frame.MapPoint(plane.Unit(value));

    private static SKPoint Fractioned(PlotPlane plane, SKMatrix frame, Vector2 fractions) => frame.MapPoint(plane.Placed(fractions));

    private static Seq<SKPoint> Sampled(PlotPlane plane, SKMatrix frame, Seq<float> samples) =>
        plane.Periodic(Orientation.Horizontal)
            ? samples.Add(samples.Head.IfNone(0f)).Map((sample, index) => Fractioned(plane, frame, new Vector2((float)index / samples.Count, plane.Fraction(Orientation.Vertical, sample))))
            : samples.Map((sample, index) => Fractioned(plane, frame, new Vector2(samples.Count > 1 ? (float)index / (samples.Count - 1) : 0f, plane.Fraction(Orientation.Vertical, sample))));

    private static Seq<SKPoint> Based(PlotPlane plane, SKMatrix frame, Seq<SKPoint> points) =>
        plane is PlotPlane.Polar
            ? points
            : points.Add(new SKPoint(points.Last.Map(static last => last.X).IfNone(0f), frame.MapPoint(SKPoint.Empty).Y))
                .Add(new SKPoint(points.Head.Map(static first => first.X).IfNone(0f), frame.MapPoint(SKPoint.Empty).Y));

    private static Seq<(float Low, float High)> Spans(PlotPlane plane, Orientation along, Option<AxisRange> band) =>
        band.Map(held => (plane.Fraction(along, held.From), plane.Fraction(along, held.To)) switch {
            var (from, to) => (Low: float.Min(from, to), High: float.Max(from, to)),
        })
            .Match(
                Some: held => plane.Periodic(along)
                    ? Seq(-1f, 0f, 1f).Map(turn => (Low: float.Max(0f, held.Low + turn), High: float.Min(1f, held.High + turn))).Filter(static span => span.High > span.Low)
                    : Seq(held),
                None: static () => Seq((Low: 0f, High: 1f)));

    private static Seq<SKRect> Rectangles(PlotPlane plane, SKMatrix frame, Option<AxisRange> first, Option<AxisRange> second) =>
        from across in Spans(plane, Orientation.Horizontal, first)
        from up in Spans(plane, Orientation.Vertical, second)
        select frame.MapRect(new SKRect(across.Low, up.Low, across.High, up.High));

    private static float Snap(float coordinate, float scale, float stroke) =>
        MathF.Round(stroke * scale, MidpointRounding.ToEven) switch {
            var device => (MathF.Round((coordinate * scale) - (device / 2f), MidpointRounding.ToEven) + (device / 2f)) / scale,
        };

    private static (SKPoint From, SKPoint To) Snapped(SKPoint from, SKPoint to, float scale, float stroke) =>
        (from.X.CompareTo(to.X) is 0, from.Y.CompareTo(to.Y) is 0) switch {
            (true, _) => (new SKPoint(Snap(from.X, scale, stroke), from.Y), new SKPoint(Snap(to.X, scale, stroke), to.Y)),
            (_, true) => (new SKPoint(from.X, Snap(from.Y, scale, stroke)), new SKPoint(to.X, Snap(to.Y, scale, stroke))),
            _ => (from, to),
        };

    private static (SKPoint Center, float Half) Disc(SKMatrix frame) => (frame.MapPoint(new SKPoint(0.5f, 0.5f)), MathF.Abs(frame.ScaleX) / 2f);

    private static void Ruled(SKPathBuilder path, (SKPoint From, SKPoint To) rule) {
        path.MoveTo(rule.From);
        path.LineTo(rule.To);
    }

    public sealed record Rule(Vector2 From, Vector2 To) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) =>
            Ruled(path, Snapped(Mapped(plane, frame, From), Mapped(plane, frame, To), scale, stroke));
    }

    public sealed record Polyline(Seq<Vector2> Points) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) =>
            path.AddPoly([.. Points.Map(point => Mapped(plane, frame, point))], close: false);
    }

    public sealed record Polygon(Seq<Vector2> Points) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) => Interior(path, plane, frame, scale);

        public override void Interior(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale) => path.AddPoly([.. Points.Map(point => Mapped(plane, frame, point))], close: true);
    }

    public sealed record Region(Option<AxisRange> First, Option<AxisRange> Second) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) => Interior(path, plane, frame, scale);

        public override void Interior(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale) {
            if (plane is not PlotPlane.Polar) {
                _ = Rectangles(plane, frame, First, Second).Iter(rect => path.AddRect(new SKRect(Snap(rect.Left, scale, 0f), Snap(rect.Top, scale, 0f), Snap(rect.Right, scale, 0f), Snap(rect.Bottom, scale, 0f))));
                return;
            }
            (SKPoint center, float half) = Disc(frame);
            (float inner, float outer) = Spans(plane, Orientation.Vertical, Second).Head.IfNone((Low: 0f, High: 1f));
            (float from, float to) = First.Map(band => (plane.Fraction(Orientation.Horizontal, band.From), plane.Fraction(Orientation.Horizontal, band.To))).IfNone((0f, 1f));
            if (MathF.Abs(to - from) >= 1f) {
                path.FillType = SKPathFillType.EvenOdd;
                path.AddCircle(center.X, center.Y, half * outer);
                if (inner > 0f) path.AddCircle(center.X, center.Y, half * inner);
                return;
            }
            float start = -from * 360f;
            float sweep = -(to - from) * 360f;
            path.ArcTo(SKRect.Create(center.X - (half * outer), center.Y - (half * outer), 2f * half * outer, 2f * half * outer), start, sweep, forceMoveTo: true);
            if (inner > 0f) path.ArcTo(SKRect.Create(center.X - (half * inner), center.Y - (half * inner), 2f * half * inner, 2f * half * inner), start + sweep, -sweep, forceMoveTo: false);
            else path.LineTo(center);
            path.Close();
        }
    }

    public sealed record Curve(Seq<float> Samples) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) =>
            path.AddPoly([.. Sampled(plane, frame, Samples)], close: false);
    }

    public sealed record Area(Seq<float> Samples) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) =>
            path.AddPoly([.. Sampled(plane, frame, Samples)], close: false);

        public override void Interior(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale) =>
            path.AddPoly([.. Based(plane, frame, Sampled(plane, frame, Samples))], close: true);
    }

    public sealed record Grid(Orientation Along) : Figure {
        public override void Edges(SKPathBuilder path, PlotPlane plane, SKMatrix frame, float scale, float stroke, float spacing) =>
            _ = plane.Dimension(Along).Iter(axis => axis.Ticks(spacing / plane.Pixels(Along, frame)).Map(axis.Fraction).Iter(fraction => {
                if (plane is not PlotPlane.Polar) {
                    Ruled(path, Snapped(frame.MapPoint(Along.With(Vector2.Zero, fraction)), frame.MapPoint(Along.With(Vector2.One, fraction)), scale, stroke));
                } else if (Along == Orientation.Horizontal) {
                    Ruled(path, (Disc(frame).Center, Fractioned(plane, frame, new Vector2(fraction, 1f))));
                } else {
                    (SKPoint center, float half) = Disc(frame);
                    path.AddCircle(center.X, center.Y, half * fraction);
                }
            }));

        public override bool Hits(PlotPlane plane, SKMatrix frame, SKPoint local, float reach) => false;
    }
}
