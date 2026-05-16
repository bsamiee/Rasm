using System.Linq;
using FsCheck;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.FSharp.Core;
using Rasm.Domain;
using Rhino;
using Xunit;
using static LanguageExt.Prelude;

namespace Rasm.Tests.Domain;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class StatsSpec {
    [Fact]
    public void StatOfMatchesNaiveTwoPassWithinTolerance() {
        double[] sample = [.. Enumerable.Range(start: 1, count: 100).Select(static i => (double)i)];
        Seq<double> values = toSeq(sample);
        Op key = Op.Create(value: "stats-numerical-stability");

        Fin<Stat> result = Stat.Of(values: values, key: key);

        double naiveMean = Enumerable.Sum(sample) / sample.Length;
        double naiveVariance = Enumerable.Sum(sample.Select(value => (value - naiveMean) * (value - naiveMean))) / sample.Length;

        Assert.True(condition: result.Match(
            Succ: stat =>
                stat.Count == 100
                && Math.Abs(value: stat.Mean - naiveMean) < 1e-12
                && Math.Abs(value: stat.Variance - naiveVariance) < 1e-9 * Math.Max(val1: naiveVariance, val2: 1.0),
            Fail: static _ => false));
    }

    [Fact]
    public void StatOfRejectsEmpty() =>
        Assert.True(condition: Stat.Of(values: Seq<double>(), key: Op.Create(value: "stats-empty")).IsFail);

    [Fact]
    public void StatOfRejectsNonFinite() =>
        Assert.True(condition: Stat.Of(values: toSeq<double>([1.0, 2.0, double.NaN]), key: Op.Create(value: "stats-non-finite")).IsFail);

    [Fact]
    public void ExtremaFoldPreservesToleranceTiesInInputOrder() {
        Seq<(string Name, double Score)> values = Seq(("low", 1.0), ("first", 10.0), ("tie", 9.95), ("drop", 8.0));

        Assert.Equal(expected: ["first", "tie"], actual: Stat.Extrema(items: values, projection: static value => value.Score, tolerance: 0.1, direction: ExtremumDirection.Maximum).Map(static value => value.Name).ToArray());
        Assert.Equal(expected: ["low"], actual: Stat.Extrema(items: values, projection: static value => value.Score, tolerance: 0.1, direction: ExtremumDirection.Minimum).Map(static value => value.Name).ToArray());
    }

    [Fact]
    public void StatOfPreservesPropertyInvariants() {
        static bool Property(double[] values) =>
            toSeq(values.Where(static value => RhinoMath.IsValidDouble(x: value) && RhinoMath.IsValidDouble(x: value * value)).Take(count: 64))
                .ToArr() switch {
                    { Count: 0 } => true,
                    Arr<double> sample => Stat.Of(values: sample.ToSeq(), key: Op.Create(value: "stats-laws")).Match(
                        Succ: static stat =>
                            stat.Count > 0
                            && stat.Minimum <= stat.Mean
                            && stat.Mean <= stat.Maximum
                            && stat.Variance >= 0.0
                            && stat.Rms >= 0.0,
                        Fail: static _ => false),
                };

        FsCheck.Check.QuickThrowOnFailure(
            property: FsCheck.FSharp.Prop.ForAll<double[], bool>(
                FsCheck.FSharp.ArbMap.defaults.ArbFor<double[]>(),
                FuncConvert.FromFunc<double[], bool>(Property)));
    }
}
