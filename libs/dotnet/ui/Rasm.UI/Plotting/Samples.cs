using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using MathNet.Numerics.Statistics;
using RBush;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Samples(ReadOnlyMemory<float> values) {
    private const int Block = 64;

    private readonly Lazy<Seq<Level>> pyramid = new(() => toSeq(SuperEnumerable.Generate(Coarsened(values, values), static level => Coarsened(level.Low, level.High)).TakeWhile(static level => level.Low.Length > 1)));

    public ReadOnlyMemory<float> Values => values;

    internal Option<float> At(int index) => index < Values.Length ? Some(Values.Span[index]) : None;

    internal float Slot(AxisRange domain) => Values.Length > 1 ? domain.Span / (Values.Length - 1) : domain.Span;

    internal Option<Datum> Nearest(AxisRange domain, float across) {
        float slot = Slot(domain);
        int index = int.Clamp((int)MathF.Round((across - domain.From) / slot, MidpointRounding.ToEven), 0, int.Max(0, Values.Length - 1));
        return At(index).Filter(float.IsFinite).Map(value => new Datum(index, new Vector2(domain.From + (index * slot), value)));
    }

    internal Seq<Seq<Vector2>> Decimated(PlotFrame frame, AxisRange domain, float columns) {
        PlotPlane plane = frame.Plane;
        float slot = Slot(domain);
        float Across(int index) => domain.From + (index * slot);
        float Column(int index) => MathF.Floor(plane.Fraction(PhysicalAxis.Horizontal, Across(index)) * columns);
        float Indexed(float fraction) => (plane.Value(PhysicalAxis.Horizontal, fraction) - domain.From) / slot;
        (float first, float last) = (Indexed(0f), Indexed(1f));
        int lo = int.Clamp((int)MathF.Floor(float.Min(first, last)) - 1, 0, Values.Length);
        int hi = int.Clamp((int)MathF.Ceiling(float.Max(first, last)) + 2, lo, Values.Length);
        Seq<Run> finite = TensorPrimitives.IsFiniteAll(Values.Span[lo..hi])
            ? [new Run(lo, hi)]
            : toSeq(Enumerable.Range(lo, hi - lo).GroupAdjacent(index => float.IsFinite(Values.Span[index])).Where(static run => run.Key).Select(static run => new Run(run.First(), run.Last() + 1)));
        return finite.Map(run => {
            (float start, float end) = (float.Clamp(Column(run.Start), -1f, columns), float.Clamp(Column(run.End - 1), -1f, columns));
            Seq<Vector2> kept = toSeq(toSeq(Range((int)float.Min(start, end) + 1, (int)MathF.Abs(end - start)))
                    .Map(column => int.Clamp((int)MathF.Ceiling(Indexed(column / columns)), run.Start, run.End))
                    .Add(run.Start).Add(run.End).Distinct().Order()
                    .Window(2)
                    .SelectMany(cut => Kept(cut[0], cut[1], Across)));
            float[] xs = [.. kept.Map(static at => at.X)];
            float[] ys = [.. kept.Map(static at => at.Y)];
            plane.Fractions(PhysicalAxis.Horizontal, xs);
            plane.Fractions(PhysicalAxis.Vertical, ys);
            return toSeq(xs.Zip(ys, static (x, y) => new Vector2(x, y)));
        }).Filter(static line => !line.IsEmpty);
    }

    internal Option<Samples> Toward(Samples to, float progress) {
        Samples Blended() {
            float[] blended = new float[Values.Length];
            TensorPrimitives.Lerp(Values.Span, to.Values.Span, progress, blended);
            return new Samples(blended);
        }
        return to.Values.Length == Values.Length ? Some(Blended()) : None;
    }

    private Seq<Vector2> Kept(int start, int end, Func<int, float> across) {
        ReadOnlySpan<float> run = Values.Span[start..end];
        return (end - start) switch {
            <= 0 => [],
            <= 4 => toSeq(Range(start, end - start)).Map(index => new Vector2(across(index), Values.Span[index])),
            <= Block => toSeq(Seq(start, start + TensorPrimitives.IndexOfMin(run), start + TensorPrimitives.IndexOfMax(run), end - 1).Distinct().Order()).Map(index => new Vector2(across(index), Values.Span[index])),
            _ => Spread(start, end, pyramid.Value.Count - 1) switch {
                var spread => Seq(new Vector2(across(start), Values.Span[start]))
                    + (Values.Span[start] <= Values.Span[end - 1] ? Seq(spread.Low, spread.High) : Seq(spread.High, spread.Low)).Map(value => new Vector2(across((start + end) / 2), value))
                    + Seq(new Vector2(across(end - 1), Values.Span[end - 1])),
            },
        };
    }

    private Extent Spread(int start, int end, int level) {
        if (level < 0) return new Extent(TensorPrimitives.MinNumber(Values.Span[start..end]), TensorPrimitives.MaxNumber(Values.Span[start..end]));
        int size = (int)Math.Pow(Block, level + 1);
        (int first, int last) = ((start + size - 1) / size, end / size);
        if (first >= last) return Spread(start, end, level - 1);
        Level held = pyramid.Value[level];
        return ((start < first * size ? Seq(Spread(start, first * size, level - 1)) : []) + (last * size < end ? Seq(Spread(last * size, end, level - 1)) : []))
            .Fold(new Extent(TensorPrimitives.MinNumber(held.Low.Span[first..last]), TensorPrimitives.MaxNumber(held.High.Span[first..last])),
                static (one, other) => new Extent(float.MinNumber(one.Low, other.Low), float.MaxNumber(one.High, other.High)));
    }

    private static Level Coarsened(ReadOnlyMemory<float> low, ReadOnlyMemory<float> high) {
        int count = (low.Length + Block - 1) / Block;
        float[] lows = new float[count];
        float[] highs = new float[count];
        for (int block = 0; block < count; block++) {
            lows[block] = TensorPrimitives.MinNumber(Chunk(low.Span, block));
            highs[block] = TensorPrimitives.MaxNumber(Chunk(high.Span, block));
        }
        return new Level(lows, highs);

        static ReadOnlySpan<float> Chunk(ReadOnlySpan<float> span, int block) => span[(block * Block)..int.Min((block + 1) * Block, span.Length)];
    }

    private sealed record Level(ReadOnlyMemory<float> Low, ReadOnlyMemory<float> High);

    private sealed record Run(int Start, int End);

    private sealed record Extent(float Low, float High);
}

public sealed record Datum(int Index, Vector2 Value);

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Summary {
    public float Minimum { get; }
    public float Lower { get; }
    public float Median { get; }
    public float Upper { get; }
    public float Maximum { get; }

    public static Fin<Summary> Validated(float minimum, float lower, float median, float upper, float maximum) =>
        UIFault.Unless(float.IsFinite(minimum) && float.IsFinite(maximum) && minimum <= lower && lower <= median && median <= upper && upper <= maximum, UIReason.Order, (minimum, lower, median, upper, maximum))
            .Map(_ => new Summary(minimum, lower, median, upper, maximum));

    public static Fin<Summary> Of(Samples samples) =>
        MemoryMarshal.ToEnumerable(samples.Values).Where(float.IsFinite).FiveNumberSummary() switch {
            var five => Validated(five[0], five[1], five[2], five[3], five[4]),
        };

    internal Option<Summary> Toward(Summary to, float progress) =>
        Validated(float.Lerp(Minimum, to.Minimum, progress), float.Lerp(Lower, to.Lower, progress), float.Lerp(Median, to.Median, progress), float.Lerp(Upper, to.Upper, progress), float.Lerp(Maximum, to.Maximum, progress)).ToOption();
}

internal sealed class Indexed(int index, Envelope envelope) : ISpatialData {
    private readonly Envelope envelope = envelope;

    internal int Index { get; } = index;

    public ref readonly Envelope Envelope => ref envelope;

    internal static RBush<Indexed> Tree(Seq<Vector2> points) {
        RBush<Indexed> tree = new();
        tree.BulkLoad(points.Map(static (at, index) => new Indexed(index, new Envelope(at.X, at.Y, at.X, at.Y))));
        return tree;
    }
}
