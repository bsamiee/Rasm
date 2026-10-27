using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using MathNet.Numerics.Statistics;
using RBush;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Samples(ReadOnlyMemory<float> values) {
    private const int Shift = 6;
    private const int Block = 1 << Shift;

    private readonly Lazy<Seq<Level>> pyramid = new(() => toSeq(SuperEnumerable.Generate(Coarsened(values, values), static level => Coarsened(level.Low, level.High)).TakeWhile(static level => level.Low.Length > 1)));

    public ReadOnlyMemory<float> Values => values;

    public Option<AxisRange> Extent =>
        (pyramid.Value.Last.Map(static top => new MinMax(TensorPrimitives.MinNumber(top.Low.Span), TensorPrimitives.MaxNumber(top.High.Span)))
            | (Values.IsEmpty ? None : Some(new MinMax(TensorPrimitives.MinNumber(Values.Span), TensorPrimitives.MaxNumber(Values.Span)))))
        .Bind(static spread => AxisRange.Validated(spread.Low, spread.High).ToOption());

    internal Option<float> At(int index) => (uint)index < (uint)Values.Length ? Some(Values.Span[index]) : None;

    internal float Slot(AxisRange domain) => Values.Length > 1 ? domain.Span / (Values.Length - 1) : domain.Span;

    internal Option<Datum> DatumAt(AxisRange domain, int index) => At(index).Filter(float.IsFinite).Map(value => new Datum(index, new Vector2(domain.From + (index * Slot(domain)), value)));

    internal Seq<Datum> Data(AxisRange domain) => toSeq(Range(0, Values.Length)).Choose(index => DatumAt(domain, index));

    internal Option<Datum> Nearest(AxisRange domain, float across) => DatumAt(domain, int.Clamp((int)MathF.Round((across - domain.From) / Slot(domain), MidpointRounding.ToEven), 0, int.Max(0, Values.Length - 1)));

    internal Run Visible(PlotFrame frame, AxisRange domain) {
        float slot = Slot(domain);
        float Position(float fraction) => (frame.Plane.Value(PhysicalAxis.Horizontal, fraction) - domain.From) / slot;
        (float first, float last) = (Position(0f), Position(1f));
        int start = int.Clamp((int)MathF.Floor(float.Min(first, last)) - 1, 0, Values.Length);
        return new Run(start, int.Clamp((int)MathF.Ceiling(float.Max(first, last)) + 2, start, Values.Length));
    }

    internal Seq<Run> Finite(Run window, Option<Samples> paired) =>
        paired.IsNone && TensorPrimitives.IsFiniteAll(Values.Span[window.Start..window.End])
            ? [window]
            : toSeq(Enumerable.Range(window.Start, window.Count).GroupAdjacent(index => float.IsFinite(Values.Span[index]) && paired.ForAll(other => other.At(index).Exists(float.IsFinite)))
                .Where(static run => run.Key).Select(static run => new Run(run.First(), run.Last() + 1)));

    internal Seq<Seq<Vector2>> Decimated(PlotFrame frame, AxisRange domain, float columns, Seq<Run> runs) {
        PlotPlane plane = frame.Plane;
        float slot = Slot(domain);
        float Across(int index) => domain.From + (index * slot);
        float Column(int index) => MathF.Floor(plane.Fraction(PhysicalAxis.Horizontal, Across(index)) * columns);
        float Position(float fraction) => (plane.Value(PhysicalAxis.Horizontal, fraction) - domain.From) / slot;
        return runs.Map(run => {
            (float start, float end) = (float.Clamp(Column(run.Start), -1f, columns), float.Clamp(Column(run.End - 1), -1f, columns));
            Seq<Vector2> kept = toSeq(toSeq(Range((int)float.Min(start, end) + 1, (int)MathF.Abs(end - start)))
                    .Map(column => int.Clamp((int)MathF.Ceiling(Position(column / columns)), run.Start, run.End))
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

    private MinMax Spread(int start, int end, int level) {
        if (level < 0) return new MinMax(TensorPrimitives.MinNumber(Values.Span[start..end]), TensorPrimitives.MaxNumber(Values.Span[start..end]));
        int size = 1 << (Shift * (level + 1));
        (int first, int last) = ((start + size - 1) / size, end / size);
        if (first >= last) return Spread(start, end, level - 1);
        Level held = pyramid.Value[level];
        return ((start < first * size ? Seq(Spread(start, first * size, level - 1)) : []) + (last * size < end ? Seq(Spread(last * size, end, level - 1)) : []))
            .Fold(new MinMax(TensorPrimitives.MinNumber(held.Low.Span[first..last]), TensorPrimitives.MaxNumber(held.High.Span[first..last])),
                static (one, other) => new MinMax(float.MinNumber(one.Low, other.Low), float.MaxNumber(one.High, other.High)));
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

    private sealed record MinMax(float Low, float High);
}

internal sealed record Run(int Start, int End) {
    public int Count => End - Start;
}

public sealed record Datum(int Index, Vector2 Value);

public sealed record Nearest(Seq<Key> Plot, HashMap<Key, Datum> Data);

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

    internal static Option<int> Nearest(RBush<Indexed> tree, PlotFrame frame, SKPoint local, Option<PhysicalAxis> along, Func<int, float> distance) =>
        frame.Plane.Separable ? Grown(tree, frame, local, along, distance, 1f) : Closest(toSeq(tree.Search()), distance);

    private static Option<int> Grown(RBush<Indexed> tree, PlotFrame frame, SKPoint local, Option<PhysicalAxis> along, Func<int, float> distance, float reach) =>
        Boxed(frame, local, along, reach).Bind(box => Closest(toSeq(tree.Search(box)), distance).Match(
            Some: index => distance(index) <= reach ? Some(index) : Grown(tree, frame, local, along, distance, distance(index)),
            None: () => box.Contains(tree.Envelope) ? None : Grown(tree, frame, local, along, distance, 2f * reach)));

    private static Option<int> Closest(Seq<Indexed> candidates, Func<int, float> distance) =>
        candidates.Map(held => (held.Index, Distance: distance(held.Index))).Filter(static held => float.IsFinite(held.Distance)) switch {
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
}
