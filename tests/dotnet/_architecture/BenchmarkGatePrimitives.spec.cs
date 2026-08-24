using System.Reflection;
using Rasm.Benchmarks;
using Rasm.TestKit;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------
// Synthetic BDN full-report rows drive the gate end-to-end: the tri-state verdict, the exact
// FullName registry match, and the Potts/BIC sustained segmenter prove without a live session.
internal static class BdnRows {
    public static BdnBenchmark Bench(string fullName, double medianNs, double iqrNs) => new() {
        FullName = fullName,
        Statistics = new BdnStatistics {
            Min = medianNs * 0.9,
            Mean = medianNs * 1.05,
            Median = medianNs,
            Q1 = medianNs - (iqrNs / 2.0),
            Q3 = medianNs + (iqrNs / 2.0),
            InterquartileRange = iqrNs,
        },
    };

    public static BdnReport Report(params BdnBenchmark[] rows) => new() { Benchmarks = rows };

    public static BenchCase Case(string fullName, double budgetMs = 5.0) =>
        new(FullName: fullName, BudgetMs: budgetMs, GateStat: GateStat.Median);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Law(typeof(Regression), "benchmark-gate")]
public sealed class BenchmarkGateLaws {
    [Fact]
    [Law(typeof(Regression), nameof(Regression.Gate), Member = nameof(Regression.Gate))]
    public void GateFoldsTheTriStateVerdictPerRegistryRow() {
        BdnReport report = BdnRows.Report(
            BdnRows.Bench(fullName: "Kit.Fast", medianNs: 1_000_000.0, iqrNs: 10_000.0),
            BdnRows.Bench(fullName: "Kit.Noisy", medianNs: 1_000_000.0, iqrNs: 600_000.0),
            BdnRows.Bench(fullName: "Kit.Slow", medianNs: 50_000_000.0, iqrNs: 100_000.0));
        Seq<GateVerdict> verdicts = Regression.Gate(report: report, cases: Seq(
            BdnRows.Case(fullName: "Kit.Fast"),
            BdnRows.Case(fullName: "Kit.Noisy"),
            BdnRows.Case(fullName: "Kit.Slow"),
            // Exact-match law: a registry prefix of a report FullName resolves nothing.
            BdnRows.Case(fullName: "Kit.Fa"),
            BdnRows.Case(fullName: "Kit.Absent")));
        Assert.Equal(expected: 5, actual: verdicts.Count);
        GateVerdict.Pass pass = Assert.IsType<GateVerdict.Pass>(@object: verdicts[0]);
        Spec.Equal(left: pass.ObservedMs, right: 1.0, tolerance: 1.0e-9, what: "observed median ms");
        GateVerdict.TooNoisy noisy = Assert.IsType<GateVerdict.TooNoisy>(@object: verdicts[1]);
        Spec.Holds(condition: noisy.RelIqr > noisy.MaxRelIqr, label: "TooNoisy must carry a dispersion over its ceiling");
        _ = Assert.IsType<GateVerdict.Breach>(@object: verdicts[2]);
        _ = Assert.IsType<GateVerdict.Breach>(@object: verdicts[3]);
        _ = Assert.IsType<GateVerdict.Breach>(@object: verdicts[4]);
    }

    [Fact]
    [Law(typeof(Regression), "ungateable-is-visible")]
    public void StatlessAndAbsentBenchmarksAreVisibleBreachesNeverSilence() {
        BdnReport report = BdnRows.Report(new BdnBenchmark { FullName = "Kit.NoStats" });
        Seq<GateVerdict> verdicts = Regression.Gate(report: report, cases: Seq(BdnRows.Case(fullName: "Kit.NoStats")));
        GateVerdict.Breach breach = Assert.IsType<GateVerdict.Breach>(@object: verdicts[0]);
        Assert.Contains(expectedSubstring: "no statistics", actualString: breach.Detail, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    [Law(typeof(GateStat), "gate-stat-projection")]
    public void GateStatRowsProjectTheirOwnStatistic() {
        BdnStatistics stats = new() { Min = 1.0, Mean = 3.0, Median = 2.0 };
        Spec.Matrix(
            ("min", () => GateStat.Min.NanosecondsOf(stats: stats) == 1.0, true),
            ("median", () => GateStat.Median.NanosecondsOf(stats: stats) == 2.0, true),
            ("mean", () => GateStat.Mean.NanosecondsOf(stats: stats) == 3.0, true));
    }

    [Fact]
    [Law(typeof(Regression), nameof(Regression.Sustained), Member = nameof(Regression.Sustained))]
    public void SustainedFailsSteppedSeriesAndPassesFlatAndSubToleranceDrift() {
        Spec.Fail(result: Regression.Sustained(seriesByKey: HashMap(("Kit.Fast", Seq(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 3.0, 3.0, 3.0, 3.0)))),
            then: static error => Assert.Contains(expectedSubstring: "Kit.Fast", actualString: error.Message, comparisonType: StringComparison.Ordinal));
        Spec.Succ(result: Regression.Sustained(seriesByKey: HashMap(("Kit.Fast", Seq(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0)))));
        // A 50% final-segment jump sits under the 70% tolerance: segmentation fires, the gate does not.
        Spec.Succ(result: Regression.Sustained(seriesByKey: HashMap(("Kit.Fast", Seq(1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.5, 1.5, 1.5, 1.5)))));
    }

    [Fact]
    [Law(typeof(Regression), nameof(Regression.SeriesFromReports), Member = nameof(Regression.SeriesFromReports))]
    public void SeriesFoldOrdersMediansOldestFirstAndSkipsStatlessRows() {
        BdnReport older = BdnRows.Report(BdnRows.Bench(fullName: "Kit.Fast", medianNs: 1.0, iqrNs: 0.1));
        BdnReport newer = BdnRows.Report(BdnRows.Bench(fullName: "Kit.Fast", medianNs: 2.0, iqrNs: 0.1), new BdnBenchmark { FullName = "Kit.NoStats" });
        HashMap<string, Seq<double>> series = Regression.SeriesFromReports(older, newer);
        Assert.Equal(expected: Seq(1.0, 2.0), actual: series["Kit.Fast"]);
        Assert.False(condition: series.ContainsKey("Kit.NoStats"), "a stats-free benchmark must not seed a series");
    }

    [Fact]
    [Law(typeof(Regression), nameof(Regression.ReadReport), Member = nameof(Regression.ReadReport))]
    public void ReadReportFailsTypedOnAnAbsentReport() =>
        Spec.Fail(result: Regression.ReadReport(path: Path.Combine(path1: Path.GetTempPath(), path2: "rasm-kit-absent-report.json")), then: static error =>
            Assert.Contains(expectedSubstring: "read failed", actualString: error.Message, comparisonType: StringComparison.Ordinal));

    [Fact]
    [Law(typeof(Regression), nameof(Regression.ReadReport), Member = nameof(Regression.ReadReport))]
    public void ReadReportDecodesAFullReportAndFailsTypedOnMalformedOrNullJson() {
        DirectoryInfo root = Directory.CreateTempSubdirectory(prefix: "rasm-bdn-");
        try {
            string good = Path.Combine(path1: root.FullName, path2: "report-full.json");
            File.WriteAllText(path: good, contents: """{"Benchmarks":[{"FullName":"Kit.Fast","Statistics":{"Min":1.0,"Mean":3.0,"Median":2.0,"Q1":1.5,"Q3":2.5,"InterquartileRange":1.0}}]}""");
            Spec.Succ(result: Regression.ReadReport(path: good), then: static report => {
                BdnBenchmark row = Assert.Single(collection: report.Benchmarks);
                Assert.Equal(expected: "Kit.Fast", actual: row.FullName);
                Assert.Equal(expected: 2.0, actual: row.Statistics!.Median);
            });
            string broken = Path.Combine(path1: root.FullName, path2: "broken.json");
            File.WriteAllText(path: broken, contents: "{not json");
            Spec.Fail(result: Regression.ReadReport(path: broken), then: static error =>
                Assert.Contains(expectedSubstring: "read failed", actualString: error.Message, comparisonType: StringComparison.Ordinal));
            // A literal "null" body decodes to a null report: the empty-report guard fails typed, never NRE.
            string empty = Path.Combine(path1: root.FullName, path2: "null.json");
            File.WriteAllText(path: empty, contents: "null");
            Spec.Fail(result: Regression.ReadReport(path: empty));
        } finally {
            root.Delete(recursive: true);
        }
    }

    // The discovery-parity gate: a [Benchmark] landing in Rasm.Benchmarks without its registry row
    // fails HERE, so a measurement can never exist silently ungated.
    [Fact]
    [Law(typeof(Regression), nameof(Regression.RegistryParity), Member = nameof(Regression.RegistryParity))]
    public void RegistryParityNamesUngatedAndPhantomRowsAndHoldsForTheLiveAssembly() {
        Spec.Succ(result: Regression.RegistryParity(
            discovered: Seq("Kit.A", "Kit.B"),
            cases: Seq(BdnRows.Case(fullName: "Kit.A"), BdnRows.Case(fullName: "Kit.B(N: 4)"))));
        Spec.FailMany(result: Regression.RegistryParity(discovered: Seq("Kit.A"), cases: Seq(BdnRows.Case(fullName: "Kit.Ghost"))),
            expectedCount: 2, "ungated benchmark: 'Kit.A'", "phantom registry row: 'Kit.Ghost'");
        // Bare-prefix ownership is refused: "Kit.AB" is not owned by the "Kit.A" method key.
        Spec.FailMany(result: Regression.RegistryParity(discovered: Seq("Kit.A"), cases: Seq(BdnRows.Case(fullName: "Kit.AB"))),
            expectedCount: 2, "'Kit.A'", "'Kit.AB'");
        Spec.Succ(result: Regression.RegistryParity(discovered: DiscoveredBenchmarks(), cases: BenchRegistry.Cases));
    }

    private static Seq<string> DiscoveredBenchmarks() =>
        toSeq(typeof(Regression).Assembly.GetTypes())
            .Bind(type => toSeq(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Filter(predicate: static method => method.IsDefined(attributeType: typeof(BenchmarkDotNet.Attributes.BenchmarkAttribute), inherit: false))
                .Map(method => $"{type.FullName}.{method.Name}"));
}
