namespace Lab.F19;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        ModelProbe,
        SourceProbe,
        OperatorProbe,
        FailureProbe,
        LogicProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> ModelProbe() =>
        Check(
            nameof(ModelProbe),
            ("Observed", Model.Total(Model.Observed).RunSafe() == Pure(6)));

    private static Fin<Unit> SourceProbe() =>
        Check(
            nameof(SourceProbe),
            ("OneValue", Sources.OneValue.Last().RunSafe() == Pure("ready")),
            ("FiniteValues", Model.Total(Sources.FiniteValues).RunSafe() == Pure(6)),
            ("FirstMessage", Sources.FirstMessage.RunSafe() == Pure("hello")));

    private static Fin<Unit> OperatorProbe() {
        (Source<int> passed, Source<int> failed) = Model.Observed.Partition(static item => item > 1);
        return Check(
            nameof(OperatorProbe),
            ("Rates", Queries.Rates(Source.lift(Seq("EURUSD", "GBPUSD"))).Collect().RunSafe() == Pure(Seq(1.1m, 1.2m, 1.3m))),
            ("Passed", passed.Collect().RunSafe() == Pure(Seq(2, 3))),
            ("Failed", failed.Collect().RunSafe() == Pure(Seq(1))),
            ("Rejoined", Branches.Rejoined(Model.Observed).RunSafe() == Pure(51)));
    }

    private static Fin<Unit> FailureProbe() {
        Source<string> pairs = Source.lift(Seq("EURUSD", "XXXYYY"));
        Fin<(Seq<decimal> Rates, Seq<Error> Errors)> partitioned = Failures.Partitioned(pairs).RunSafe();
        return Check(
            nameof(FailureProbe),
            ("Outputs", Failures.Outputs(pairs).Collect().RunSafe() == Pure(Seq("Enter a currency pair", "1.1", "unknown currency pair"))),
            ("PartitionedRates", partitioned.Exists(static outcome => outcome.Rates == Seq(1.1m))),
            ("PartitionedErrors", partitioned.Exists(static outcome => outcome.Errors.Count == 1 && outcome.Errors.Head.Exists(static error => error.HasCode(404)))));
    }

    private static Fin<Unit> LogicProbe() {
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        Source<Transaction> transactions = Source.lift(Seq(
            new Transaction(first, 10m),
            new Transaction(second, 5m),
            new Transaction(first, -15m),
            new Transaction(second, -3m)));
        Conduit<int, int> fresh = Conduit.make(Buffer<int>.Unbounded);
        Seq<decimal> rates = Seq(1.1m, 1.2m, 1.3m);
        return Check(
            nameof(LogicProbe),
            ("PairWithPrevious", Model.Observed.PairWithPrevious().Collect().RunSafe() == Pure(Seq((Previous: 1, Current: 2), (Previous: 2, Current: 3)))),
            ("BalanceInUsd", Backpressure.BalanceInUsd(Source.lift(Seq(100m, 200m)), Source.lift(Seq(1.1m, 1.2m))).Collect().RunSafe() == Pure(Seq(110m, 240m))),
            ("Unbounded", Backpressure.Retained(Buffer<decimal>.Unbounded, rates).RunSafe() == Pure(rates)),
            ("Latest", Backpressure.Retained(Buffer<decimal>.Latest(0m), rates).RunSafe() == Pure(Seq(1.3m))),
            ("Newest", Backpressure.Retained(Buffer<decimal>.Newest(2), rates).RunSafe() == Pure(Seq(1.2m, 1.3m))),
            ("Bounded", Backpressure.Drained(Buffer<decimal>.Bounded(2), rates).RunSafe() == Pure(rates)),
            ("Single", Backpressure.Drained(Buffer<decimal>.Single, rates).RunSafe() == Pure(rates)),
            ("PostLength", Backpressure.PostLength(fresh.Sink, "abc").RunSafe().IsSucc),
            ("Overdrawn", Ledger.Overdrawn(transactions).RunSafe() == Pure(Seq(first))));
    }

}
