using System.Runtime.InteropServices;
using Core.Domain;
using Rhino;
namespace Core.Runtime;

// --- [MODELS] ----------------------------------------------------------------------------------

[StructLayout(LayoutKind.Auto)]
internal readonly record struct Stats(
    int Count,
    double Minimum,
    double Maximum,
    double Mean,
    double Variance,
    double Rms);

// --- [OPERATIONS] ------------------------------------------------------------------------------

internal static class FoldExtensions {
    internal static Fin<S> FoldFin<A, S>(
        this Seq<A> items,
        Fin<S> seed,
        Func<S, A, Fin<S>> step) =>
        items.Fold(
            initialState: seed,
            f: (Fin<S> acc, A item) => acc.Bind(s => step(s, item)));
    internal static Fin<Seq<A>> TraverseFin<A>(this Seq<Fin<A>> items) =>
        items.FoldFin(
            seed: Fin.Succ(Seq<A>()),
            step: static (Seq<A> acc, Fin<A> fa) => fa.Map(a => acc.Add(a)));
    internal static Fin<Stats> StatsOf(this Seq<double> values, OperationKey key) =>
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
                        Count: count,
                        Minimum: minimum,
                        Maximum: maximum,
                        Mean: sum / count,
                        Variance: Math.Max(val1: 0.0, val2: (sumSquares / count) - (sum * sum / (double)count / count)),
                        Rms: Math.Sqrt(d: sumSquares / count))),
                };
    internal static Seq<TItem> MaxesBy<TItem>(
        this Seq<TItem> items,
        Func<TItem, double> projection,
        double tolerance) =>
        items
            .Fold(
                initialState: (Best: double.NegativeInfinity, Hits: Seq<TItem>()),
                f: ((double Best, Seq<TItem> Hits) acc, TItem item) =>
                    projection(arg: item) switch {
                        double s when s > acc.Best + tolerance => (Best: s, Hits: Seq(item)),
                        double s when s >= acc.Best - tolerance => (Best: Math.Max(val1: acc.Best, val2: s), Hits: acc.Hits.Add(item)),
                        _ => acc,
                    })
            .Hits;
    internal static Seq<TItem> MinesBy<TItem>(
        this Seq<TItem> items,
        Func<TItem, double> projection,
        double tolerance) =>
        items
            .Fold(
                initialState: (Best: double.PositiveInfinity, Hits: Seq<TItem>()),
                f: ((double Best, Seq<TItem> Hits) acc, TItem item) =>
                    projection(arg: item) switch {
                        double s when s < acc.Best - tolerance => (Best: s, Hits: Seq(item)),
                        double s when s <= acc.Best + tolerance => (Best: Math.Min(val1: acc.Best, val2: s), Hits: acc.Hits.Add(item)),
                        _ => acc,
                    })
            .Hits;
}
