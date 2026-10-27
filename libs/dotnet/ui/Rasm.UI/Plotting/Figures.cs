using System.Numerics;
using System.Numerics.Tensors;
using Rasm.UI.Rendering;
using SkiaSharp;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Pen(float Stroke, float Spacing) {
    private readonly Option<SKMatrix> inverse = Some(SKMatrix.Identity);

    public static Pen Hit { get; } = new(0f, 0f);

    public SKMatrix Device { get; init { field = value; inverse = value.TryInvert(out SKMatrix inverted) ? Some(inverted) : None; } } = SKMatrix.Identity;

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Figure {
    private static readonly Func<ReadOnlySpan<float>, Seq<int>> Extremes = static column => toSeq(Seq(TensorPrimitives.IndexOfMin(column), TensorPrimitives.IndexOfMax(column)).Order());
    private static readonly Func<ReadOnlySpan<float>, Seq<int>> Peaks = static column => Seq(TensorPrimitives.IndexOfMax(column));

    public abstract void Edges(SKPathBuilder path, PlotFrame frame, Pen pen);

    public virtual void Interior(SKPathBuilder path, PlotFrame frame, Pen pen) { }

    public virtual bool Hits(PlotFrame frame, SKPoint local, float reach) {
        using SKPath interior = Outline.Traced(path => Interior(path, frame, Pen.Hit));
        using SKPath outline = Outline.Traced(path => {
            Interior(path, frame, Pen.Hit);
            Edges(path, frame, Pen.Hit);
        });
        using SKPaint pen = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 2f * reach, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
        using SKPath? reached = pen.GetFillPath(outline);
        return interior.Contains(local.X, local.Y) || (reached?.Contains(local.X, local.Y) ?? false);
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

    public sealed record Region : Figure {
        public Option<AxisRange> First { get; init; }
        public Option<AxisRange> Second { get; init; }

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

        private Seq<float> Tabulated(PlotFrame frame, Pen pen) {
            float spanned = MathF.Abs(frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.To) - frame.Plane.Fraction(PhysicalAxis.Horizontal, Domain.From));
            int count = int.Max(2, (int)MathF.Ceiling(spanned * frame.Pixels(PhysicalAxis.Horizontal) * pen.Device.MapRadius(1f)) + 1);
            return toSeq(Range(0, count)).Map(index => Fit(Domain.From + (index * Domain.Span / (count - 1))));
        }
    }

    public sealed record Grid(PhysicalAxis Along) : Figure {
        public Option<Axis> Ticks { get; init; }

        public override void Edges(SKPathBuilder path, PlotFrame frame, Pen pen) =>
            _ = (Ticks | frame.Plane.Dimension(Along)).Iter(axis => axis.Ticks(pen.Spacing / frame.Pixels(Along)).Map(axis.Fraction).Iter(at => frame.Plane.Trace(path, frame.Matrix, pen, Along, at)));

        public override bool Hits(PlotFrame frame, SKPoint local, float reach) => false;
    }
}
