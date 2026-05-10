using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using static LanguageExt.Prelude;
namespace Core.Domain;

// --- [MODELS] --------------------------------------------------------------------------

[StructLayout(LayoutKind.Auto)]
internal readonly record struct Stats {
    private Stats(int count, double minimum, double maximum, double mean, double variance, double rms) {
        Count = count;
        Minimum = minimum;
        Maximum = maximum;
        Mean = mean;
        Variance = variance;
        Rms = rms;
    }
    internal int Count { get; }
    internal double Minimum { get; }
    internal double Maximum { get; }
    internal double Mean { get; }
    internal double Variance { get; }
    internal double Rms { get; }
    internal static Fin<Stats> From(Seq<double> values, Op key) =>
        values.Fold(
            initialState: (Count: 0, Sum: 0.0, SumSquares: 0.0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, AllFinite: true),
            f: static ((int Count, double Sum, double SumSquares, double Minimum, double Maximum, bool AllFinite) acc, double v) => (
                Count: acc.Count + 1,
                Sum: acc.Sum + v,
                SumSquares: acc.SumSquares + (v * v),
                Minimum: Math.Min(val1: acc.Minimum, val2: v),
                Maximum: Math.Max(val1: acc.Maximum, val2: v),
                AllFinite: acc.AllFinite && RhinoMath.IsValidDouble(x: v))) switch {
                    (0, _, _, _, _, _) => Fin.Fail<Stats>(key.InvalidResult()),
                    (_, _, _, _, _, false) => Fin.Fail<Stats>(key.InvalidResult()),
                    (int count, double sum, double sumSquares, double minimum, double maximum, _) => Fin.Succ(new Stats(
                        count: count,
                        minimum: minimum,
                        maximum: maximum,
                        mean: sum / count,
                        variance: Math.Max(val1: 0.0, val2: (sumSquares / count) - (sum * sum / (double)count / count)),
                        rms: Math.Sqrt(d: sumSquares / count))),
                };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class FoldExtensions {
    internal static Seq<TItem> Maxima<TItem>(
        this Seq<TItem> items,
        Func<TItem, double> projection,
        double tolerance) =>
        items
            .Fold(
                initialState: (Best: double.NegativeInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection),
                f: static ((double Best, Seq<TItem> Hits, double Tolerance, Func<TItem, double> Projection) acc, TItem item) =>
                    acc.Projection(arg: item) switch {
                        double s when s > acc.Best + acc.Tolerance => acc with { Best = s, Hits = Seq(item) },
                        double s when s >= acc.Best - acc.Tolerance => acc with { Best = Math.Max(val1: acc.Best, val2: s), Hits = acc.Hits.Add(item) },
                        _ => acc,
                    })
            .Hits;
    internal static Seq<TItem> Minima<TItem>(
        this Seq<TItem> items,
        Func<TItem, double> projection,
        double tolerance) =>
        items
            .Fold(
                initialState: (Best: double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection),
                f: static ((double Best, Seq<TItem> Hits, double Tolerance, Func<TItem, double> Projection) acc, TItem item) =>
                    acc.Projection(arg: item) switch {
                        double s when s < acc.Best - acc.Tolerance => acc with { Best = s, Hits = Seq(item) },
                        double s when s <= acc.Best + acc.Tolerance => acc with { Best = Math.Min(val1: acc.Best, val2: s), Hits = acc.Hits.Add(item) },
                        _ => acc,
                    })
            .Hits;
}
