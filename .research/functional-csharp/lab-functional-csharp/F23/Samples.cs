namespace Lab.F23;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        HigherKindProbe,
        FunctorApplicativeMonadProbe,
        FoldableTraversableProbe,
        FallibleAlternativeProbe,
        ReadableStatefulWritableProbe,
        LawProbe,
        StackSafetyProbe,
        TransformerProbe,
        DerivingProbe,
        TraversalProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> HigherKindProbe() =>
        Check(
            nameof(HigherKindProbe),
            ("OptionPrice", HigherKinds.OptionPrice(Some(new Line("a", 2m))) == Some(2m)),
            ("OptionPrice none", HigherKinds.OptionPrice(Option<Line>.None).IsNone),
            ("SeqPrices", HigherKinds.SeqPrices(Seq(new Line("a", 2m), new Line("b", 3m))) == Seq(2m, 3m)));

    private static Fin<Unit> FunctorApplicativeMonadProbe() {
        Option<int> total = Traits.Total(Some(1), Some(2)).As();
        Fin<int> halved = Traits.Halved(Pure(8).ToFin(), static v => Pure(v / 2).ToFin()).As();
        Validation<Error, int> failedA = Error.New("a");
        Validation<Error, int> failedB = Error.New("b");
        return Check(
            nameof(FunctorApplicativeMonadProbe),
            ("Doubled Option", Traits.Doubled(Some(2)).As() == Some(4)),
            ("Doubled Seq", Traits.Doubled(Seq(1, 2)).As() == Seq(2, 4)),
            ("Lifted Option", Traits.Lifted<Option>(1).As() == Some(1)),
            ("Lifted Seq", Traits.Lifted<Seq>(1).As() == Seq(1)),
            ("Incremented", Traits.Incremented(Some(1)).As() == Some(2)),
            ("Summed Option", Traits.Summed(Some(1), Some(2)).As() == Some(3)),
            ("Summed Validation", Traits.Summed(failedA, failedB).As().Match(Fail: static e => e.Count, Succ: static _ => 0) == 2),
            ("Halved", halved == Pure(4)),
            ("Total", total == Some(3)),
            ("Total IO", Traits.Total(IO.pure(1), IO.pure(2)).As().RunSafe() == Pure(3)));
    }

    private static Fin<Unit> FoldableTraversableProbe() {
        Option<Seq<int>> parsed = Traits.ParseAll(Seq("1", "2")).As().Map(static values => values.As());
        Option<Seq<int>> unparsed = Traits.ParseAll(Seq("1", "x")).As().Map(static values => values.As());
        Fin<Seq<int>> read = Traits.ReadAll(Seq("a", "bb"), static s => IO.pure(s.Length)).As().Map(static values => values.As()).RunSafe();
        return Check(
            nameof(FoldableTraversableProbe),
            ("Sum Seq", Traits.Sum(Seq(1, 2, 3)) == 6),
            ("Sum Option", Traits.Sum(Some(5)) == 5),
            ("Sum None", Traits.Sum(Option<int>.None) == 0),
            ("Reversed", Traits.Reversed(Seq(1, 2, 3)) == Seq(3, 2, 1)),
            ("AnyNegative", Traits.AnyNegative(Seq(1, -2)) && !Traits.AnyNegative(Some(1))),
            ("AllPositive", Traits.AllPositive(Seq(1, 2)) && Traits.AllPositive(Option<int>.None)),
            ("Second", Traits.Second(Seq(1, 2, 3)) == Some(2) && Traits.Second(Some(1)).IsNone),
            ("ParseAll", parsed == Some(Seq(1, 2))),
            ("ParseAll none", unparsed.IsNone),
            ("ReadAll", read == Pure(Seq(1, 2))));
    }

    private static Fin<Unit> FallibleAlternativeProbe() {
        Fin<int> rejected = Traits.Reject<Fin>().As();
        Fin<int> recovered = Traits.Recovered(Traits.Reject<Fin>()).As();
        Fin<int> recoveredIO = Traits.Recovered(Traits.Reject<IO>()).As().RunSafe();
        Fin<int> other = Error.New("other");
        return Check(
            nameof(FallibleAlternativeProbe),
            ("Reject Fin", rejected.Match(Succ: static _ => false, Fail: static e => e.IsType<Rejected>())),
            ("Reject IO", Traits.Reject<IO>().As().RunSafe().IsFail),
            ("Recovered Fin", recovered == Pure(0)),
            ("Recovered IO", recoveredIO == Pure(0)),
            ("Recovered other", Traits.Recovered(other).As().IsFail),
            ("Nothing Option", Traits.Nothing<Option>().As().IsNone),
            ("Nothing Seq", Traits.Nothing<Seq>().As().IsEmpty),
            ("FirstOf Option", Traits.FirstOf(Option<int>.None, Some(2)).As() == Some(2)),
            ("FirstOf Fin", Traits.FirstOf(rejected, Pure(3).ToFin()).As() == Pure(3)),
            ("Chosen", Traits.Chosen(Option<int>.None, Some(4)) == Some(4)));
    }

    private static Fin<Unit> ReadableStatefulWritableProbe() {
        Settings settings = new(3);
        Fin<int> factorT = Traits.Factor<ReaderT<Settings, IO>>().As().Run(settings).As().RunSafe();
        Fin<int> scaledT = Traits.ScaledTwice(Traits.Scaled<ReaderT<Settings, IO>>(5)).As().Run(settings).As().RunSafe();
        (int Value, int State) ticked = Traits.Tick<State<int>>().As().Run(7);
        (_, int resetState) = Traits.Reset<State<int>>().As().Run(7);
        (int Value, int State) counted = Traits.Counted<State<int>>().As().Run(7);
        (int Value, int State) isolated = Traits.Isolated(Traits.Tick<State<int>>()).As().Run(7);
        Fin<(int Value, int State)> tickedT = Traits.Tick<StateT<int, IO>>().As().Run(7).As().RunSafe();
        (_, Seq<string> notedOutput) = Traits.Note<Writer<Seq<string>>>("hello").As().Run();
        Fin<(Unit Value, Seq<string> Output)> notedT = Traits.Note<WriterT<Seq<string>, IO>>("hello").As().Run().As().RunSafe();
        return Check(
            nameof(ReadableStatefulWritableProbe),
            ("Factor Reader", Traits.Factor<Reader<Settings>>().As().Run(settings) == 3),
            ("Scaled Reader", Traits.Scaled<Reader<Settings>>(5).As().Run(settings) == 15),
            ("ScaledTwice Reader", Traits.ScaledTwice(Traits.Scaled<Reader<Settings>>(5)).As().Run(settings) == 30),
            ("Factor ReaderT", factorT == Pure(3)),
            ("ScaledTwice ReaderT", scaledT == Pure(30)),
            ("Tick", ticked == (7, 8)),
            ("Reset", resetState == 0),
            ("Counted", counted == (7, 8)),
            ("Isolated", isolated == (107, 7)),
            ("Tick StateT", tickedT == Pure((7, 8))),
            ("Note Writer", notedOutput == Seq("hello")),
            ("Note WriterT", notedT.Exists(static n => n.Output == Seq("hello"))));
    }

    private static Fin<Unit> LawProbe() =>
        Check(
            nameof(LawProbe),
            ("OptionFunctor", Laws.OptionFunctor.IsSuccess),
            ("OptionApplicative", Laws.OptionApplicative.IsSuccess),
            ("OptionMonad", Laws.OptionMonad.IsSuccess),
            ("FinFunctor", Laws.FinFunctor.IsSuccess),
            ("FinApplicative", Laws.FinApplicative.IsSuccess),
            ("FinMonad", Laws.FinMonad.IsSuccess));

    private static Fin<Unit> StackSafetyProbe() {
        const int limit = 100_000;
        Atom<int> pending = Atom(limit);
        long summed = StackSafety.SumTo(1, limit, 0L).Run();
        Fin<int> drained = StackSafety.Drain(pending).RunSafe();
        Fin<int> counted = StackSafety.Exit(StackSafety.CountTo(0, limit));
        Fin<int> mapped = Try.lift(static () => StackSafety.CountTo(0, 3).Map(static n => n + 1).Run()).Run();
        return Check(
            nameof(StackSafetyProbe),
            ("SumTo", summed == 5_000_050_000L),
            ("Drain", drained == Pure(limit) && pending.Value == 0),
            ("CountTo", counted == Pure(limit)),
            ("CountTo mapped fails", mapped.IsFail));
    }

    private static Fin<Unit> TransformerProbe() {
        Guid known = new("11111111-1111-1111-1111-111111111111");
        Map<Guid, Account> accounts = Map((known, new Account(known, 50m)));
        Settings settings = new(3);
        Fin<Option<decimal>> converted = Transformers.Converted(Lookup(accounts, known), IO.pure(2m)).Run().As().RunSafe();
        Fin<Option<decimal>> missing = Transformers.Converted(Lookup(accounts, Guid.NewGuid()), IO.pure(2m)).Run().As().RunSafe();
        Fin<Account> settled = Transformers.Settled(Transformers.Charged(IO.pure(new Account(known, 50m)), 20m)).RunSafe();
        Fin<Account> overdrawn = Transformers.Settled(Transformers.Charged(IO.pure(new Account(known, 50m)), 80m)).RunSafe();
        Fin<Either<Guest, Member>> member = Transformers.Visitor(IO.pure(7), "ada").Run().As().RunSafe();
        Fin<Either<Guest, Member>> guest = Transformers.Visitor(IO.pure(0), "ada").Run().As().RunSafe();
        Fin<Validation<Error, string>> both = Transformers.Both(IO.pure(" "), IO.pure("")).Run().As().RunSafe();
        Fin<Validation<Error, string>> joined = Transformers.Both(IO.pure("a"), IO.pure("b")).Run().As().RunSafe();
        Fin<Option<int>> priced = Transformers.Run(Transformers.Priced(Some(2), IO.pure(5)), settings);
        Fin<Option<int>> unpriced = Transformers.Run(Transformers.Priced(Option<int>.None, IO.pure(5)), settings);
        Fin<(int Value, Seq<string> Output)> audited = Transformers.Run(Transformers.Audited(IO.pure(4)));
        Fin<(int Value, Seq<string> Output, int State)> stepped = Transformers.Stepped.Run(settings, 2).As().RunSafe();
        return Check(
            nameof(TransformerProbe),
            ("Converted", converted == Pure(Some(100m))),
            ("Converted missing", missing == Pure(Option<decimal>.None)),
            ("Settled", settled.Map(static a => a.Balance) == Pure(30m)),
            ("Overdrawn", overdrawn.Match(Succ: static _ => false, Fail: static e => e.IsType<Overdrawn>())),
            ("Member", member.Exists(static e => e.IsRight)),
            ("Guest", guest.Exists(static e => e.IsLeft)),
            ("Both accumulates", both.Exists(static v => v.Match(Fail: static e => e.Count == 2, Succ: static _ => false))),
            ("Both joined", joined.Exists(static v => v.Match(Fail: static _ => false, Succ: static s => string.Equals(s, "ab", StringComparison.Ordinal)))),
            ("Priced", priced == Pure(Some(30))),
            ("Unpriced", unpriced == Pure(Option<int>.None)),
            ("Audited", audited.Exists(static a => a.Value == 4 && a.Output == Seq("loaded 4"))),
            ("Stepped", stepped.Exists(static s => s.Value == 6 && s.State == 3 && s.Output == Seq("stepped"))));
    }

    private static Fin<Unit> DerivingProbe() =>
        Check(
            nameof(DerivingProbe),
            ("Increment", Counters.Increment.Exit(5) == Pure((5, 6))),
            ("Tick Counter", Traits.Tick<Counter>().As().Exit(5) == Pure((5, 6))),
            ("Counted Counter", Traits.Counted<Counter>().As().Exit(5) == Pure((5, 6))),
            ("Lifted Counter", Counters.Lifted(IO.pure(9)).Exit(5) == Pure((9, 5))));

    private static Fin<Unit> TraversalProbe() {
        Atom<Seq<string>> log = Atom(Seq<string>());
        Func<string, Job> job = name => new Job(name, IO.lift(() => log.Swap(names => names.Add(name)).Count));
        Seq<Job> jobs = Seq(job("a"), job("b"), job("c"));
        Seq<Job> mixed = jobs.Add(new Job("d", IO.fail<int>(new Rejected())));
        Fin<Seq<int>> parallel = Traversals.Overlapped(jobs).RunSafe();
        int afterParallel = log.Value.Count;
        _ = log.Swap(static _ => Seq<string>());
        Fin<Seq<int>> serial = Traversals.Serial(jobs).RunSafe();
        Seq<string> serialOrder = log.Value;
        Fin<Seq<int>> bounded = Traversals.Bounded(jobs, 2).RunSafe();
        Fin<(Seq<Error> Fails, Seq<int> Succs)> bestEffort = Traversals.BestEffort(mixed).RunSafe();
        Fin<Seq<int>> completed = Traversals.Completed(mixed).RunSafe();
        Fin<Seq<Error>> failed = Traversals.Failed(mixed).RunSafe();
        return Check(
            nameof(TraversalProbe),
            ("Accumulated", Traversals.Accumulated(Seq("1", "x", "y"), Number).Match(Fail: static e => e.Count, Succ: static _ => 0) == 2),
            ("FirstFailure", Traversals.FirstFailure(Seq("1", "x", "y"), Number).Match(Fail: static e => e.Count, Succ: static _ => 0) == 1),
            ("Parallel", parallel.Exists(static values => values.Count == 3) && afterParallel == 3),
            ("Serial", serial == Pure(Seq(1, 2, 3)) && serialOrder == Seq("a", "b", "c")),
            ("Bounded", bounded.Exists(static values => values.Count == 3)),
            ("Parallel fails", Traversals.Overlapped(mixed).RunSafe().IsFail),
            ("BestEffort", bestEffort.Exists(static parts => parts.Succs.Count == 3 && parts.Fails.Count == 1)),
            ("Completed", completed.Exists(static values => values.Count == 3)),
            ("Failed", failed.Exists(static errors => errors.Count == 1)));
    }

    private static Validation<Error, int> Number(string text) => parseInt(text).ToValidation<Error>(Error.New($"bad {text}"));

    private static OptionT<IO, Account> Lookup(Map<Guid, Account> accounts, Guid id) =>
        OptionT.lift<IO, Account>(IO.lift(() => accounts.Find(id)));
}
