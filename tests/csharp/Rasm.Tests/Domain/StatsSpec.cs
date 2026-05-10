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
    public void StatsOfMatchesNaiveTwoPassWithinTolerance() {
        double[] sample = [.. Enumerable.Range(start: 1, count: 100).Select(static i => (double)i)];
        Seq<double> values = toSeq(sample);
        Op key = new(name: "stats-numerical-stability");

        Fin<Stats> result = Stats.From(values: values, key: key);

        double naiveMean = Enumerable.Sum(sample) / sample.Length;
        double naiveVariance = Enumerable.Sum(sample.Select(value => (value - naiveMean) * (value - naiveMean))) / sample.Length;

        Assert.True(condition: result.Match(
            Succ: stats =>
                stats.Count == 100
                && Math.Abs(value: stats.Mean - naiveMean) < 1e-12
                && Math.Abs(value: stats.Variance - naiveVariance) < 1e-9 * Math.Max(val1: naiveVariance, val2: 1.0),
            Fail: static _ => false));
    }

    [Fact]
    public void StatsOfRejectsEmpty() =>
        Assert.True(condition: Stats.From(values: Seq<double>(), key: new Op(name: "stats-empty")).IsFail);

    [Fact]
    public void StatsOfRejectsNonFinite() =>
        Assert.True(condition: Stats.From(values: toSeq<double>([1.0, 2.0, double.NaN]), key: new Op(name: "stats-non-finite")).IsFail);

    [Fact]
    public void StatsFromPreservesPropertyInvariants() {
        static bool Property(double[] values) =>
            toSeq(values.Where(static value => RhinoMath.IsValidDouble(x: value) && RhinoMath.IsValidDouble(x: value * value)).Take(count: 64))
                .ToArr() switch {
                    { Count: 0 } => true,
                    Arr<double> sample => Stats.From(values: sample.ToSeq(), key: new Op(name: "stats-laws")).Match(
                        Succ: static stats =>
                            stats.Count > 0
                            && stats.Minimum <= stats.Mean
                            && stats.Mean <= stats.Maximum
                            && stats.Variance >= 0.0
                            && stats.Rms >= 0.0,
                        Fail: static _ => false),
                };

        FsCheck.Check.QuickThrowOnFailure(
            property: FsCheck.FSharp.Prop.ForAll<double[], bool>(
                FsCheck.FSharp.ArbMap.defaults.ArbFor<double[]>(),
                FuncConvert.FromFunc<double[], bool>(Property)));
    }
}
