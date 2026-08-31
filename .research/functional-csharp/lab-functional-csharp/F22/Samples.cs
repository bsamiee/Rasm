namespace Lab.F22;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        ConstructionProbe,
        ExitProbe,
        ResourceProbe,
        ConcurrencyProbe,
        RecursionProbe,
        ScheduleProbe,
        RuntimeProbe,
        StreamProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> ConstructionProbe() =>
        Check(
            nameof(ConstructionProbe),
            ("Plain", Construction.Plain.RunSafe() == Pure(1)),
            ("Folded", Construction.Folded.RunSafe() == Pure(2)),
            ("FoldedFail", Construction.FoldedFail.RunSafe().IsFail),
            ("Carried", Construction.Carried.RunSafe().Exists(static succ => succ)),
            ("Evaluated", Construction.Evaluated.RunSafe() == Pure(4)),
            ("Fetched", Construction.Fetched.RunSafe() == Pure(50)),
            ("TokenAware", Construction.TokenAware.RunSafe() == Pure(60)),
            ("Failed", Construction.Failed.RunSafe().IsFail),
            ("Total", Construction.Total.RunSafe() == Pure(17)));

    private static Fin<Unit> ExitProbe() {
        Fin<int> cancelled = Exits.Cancelled();
        return Check(
            nameof(ExitProbe),
            ("Safe", Exits.Safe.IsFail),
            ("Thrown", Exits.Thrown.IsFail),
            ("ThrownCode", Exits.Thrown.Match(Succ: static _ => false, Fail: static e => e.HasCode(503))),
            ("Recovered", Exits.Recovered == Pure(9)),
            ("Alternative", Exits.Alternative == Pure(8)),
            ("Cancelled", cancelled.IsFail));
    }

    private static Fin<Unit> ResourceProbe() {
        int before = Connection.Released;
        Fin<int> disposed = Resources.Disposed.RunSafe();
        int afterDisposed = Connection.Released;
        Fin<int> released = Resources.Released.RunSafe();
        int afterReleased = Connection.Released;
        Fin<int> bracketed = Resources.Bracketed.RunSafe();
        int afterBracketed = Connection.Released;
        Atom<int> closed = Atom(0);
        Fin<int> audited = Resources.Audited(closed).RunSafe();
        return Check(
            nameof(ResourceProbe),
            ("Disposed", disposed == Pure(42) && afterDisposed == before + 1),
            ("Released", released == Pure(42) && afterReleased == before + 2),
            ("Bracketed", bracketed == Pure(42) && afterBracketed == before + 3),
            ("Audited", audited.IsFail && closed.Value == 1));
    }

    private static Fin<Unit> ConcurrencyProbe() {
        Seq<IO<int>> jobs = [IO.pure(1), IO.pure(2)];
        return Check(
            nameof(ConcurrencyProbe),
            ("Forked", Concurrency.Forked.RunSafe() == Pure(3)),
            ("All", Concurrency.All(jobs).RunSafe() == Pure(Seq(1, 2))),
            ("First", Concurrency.First(jobs).RunSafe().IsSucc),
            ("Deadline", Concurrency.Deadline(IO.pure(1)).RunSafe() == Pure(1)),
            ("Masked", Concurrency.Masked.RunSafe() == Pure(3)),
            ("Chunked", Concurrency.Chunked(Seq(1, 2, 3, 4, 5), 2, static x => IO.pure(x * 2)).RunSafe() == Pure(Seq(2, 4, 6, 8, 10))),
            ("DrainedBounded", Concurrency.Drained(Buffer<int>.Bounded(2), Seq(1, 2, 3, 4, 5)).RunSafe() == Pure(15)),
            ("DrainedSingle", Concurrency.Drained(Buffer<int>.Single, Seq(1, 2, 3, 4, 5)).RunSafe() == Pure(15)),
            ("DrainedUnbounded", Concurrency.Drained(Buffer<int>.Unbounded, Seq(1, 2, 3, 4, 5)).RunSafe() == Pure(15)));
    }

    private static Fin<Unit> RecursionProbe() {
        const int limit = 100_000;
        Atom<int> polls = Atom(0);
        Atom<int> pending = Atom(3);
        return Check(
            nameof(RecursionProbe),
            ("CountTo", Try.lift(Recursion.CountTo(0, limit).Run).Run() == Pure(limit)),
            ("Recur", Recursion.Recur(limit).RunSafe() == Pure(limit)),
            ("Poll", Recursion.Poll(polls).RunSafe() == Pure(3) && polls.Value == 3),
            ("Drain", Recursion.Drain(pending).RunSafe() == Pure(0) && pending.Value == 0));
    }

    private static Fin<Unit> ScheduleProbe() {
        Atom<int> attempts = Atom(0);
        Atom<int> ticks = Atom(0);
        Fin<int> retried = Schedules.Retried(attempts).RunSafe();
        Fin<int> repeated = Schedules.Repeated(ticks).RunSafe();
        Seq<double> union = toSeq(Schedules.Union.Run().Take(4)).Map(static d => (double)d);
        Seq<double> intersection = toSeq(Schedules.Intersection.Run().Take(4)).Map(static d => (double)d);
        Seq<double> capped = toSeq(Schedules.Capped.Run().Take(5)).Map(static d => (double)d);
        return Check(
            nameof(ScheduleProbe),
            ("Retried", retried == Pure(7) && attempts.Value == 3),
            ("Repeated", repeated.IsSucc && ticks.Value == 3),
            ("Backoff", Schedules.Backoff.Run().Count() == 3),
            ("Capped", capped.ForAll(static d => d <= 4.0)),
            ("Replayed", Schedules.Replayed.Run().Count() == 4),
            ("Union", union == Seq(1.0, 1.0, 1.0, 1.0)),
            ("Intersection", intersection == Seq(3.0, 3.0, 3.0, 3.0)));
    }

    private static Fin<Unit> RuntimeProbe() {
        MemoryConsole memory = new();
        Runtime custom = new(new LanguageExt.Sys.Test.Implementations.ConsoleIO(memory));
        _ = memory.WriteKeyLine("reply");
        Fin<string> customRun = Runtimes.Greet<Runtime>("hello").Run(custom);
        using LanguageExt.Sys.Test.Runtime test = LanguageExt.Sys.Test.Runtime.New();
        _ = test.Env.Console.WriteKeyLine("again");
        Fin<string> testRun = Runtimes.Greet<LanguageExt.Sys.Test.Runtime>("hi").Run(test);
        Fin<string> file = Runtimes.RoundTrip<LanguageExt.Sys.Test.Runtime>(Path.Combine(test.Env.RootPath, "notes.txt"), "kept").Run(test);
        LanguageExt.Sys.Live.Runtime live = LanguageExt.Sys.Live.Runtime.New();
        Fin<LanguageExt.Sys.Live.Runtime> liveRun = Eff.runtime<LanguageExt.Sys.Live.Runtime>().Run(live);
        return Check(
            nameof(RuntimeProbe),
            ("Custom", customRun == Pure("reply") && toSeq(memory) == Seq("hello")),
            ("Test", testRun == Pure("again") && toSeq(test.Env.Console) == Seq("hi")),
            ("File", file == Pure("kept")),
            ("Live", liveRun.IsSucc),
            ("Entered", Runtimes.Entered.Run(custom) == Pure(1)),
            ("Ask", Runtimes.Ask.Run(custom).IsSucc));
    }

    private static Fin<Unit> StreamProbe() {
        Conduit<int, int> inbox = Conduit.make(Buffer<int>.Unbounded);
        Conduit<int, int> replies = Conduit.make(Buffer<int>.Unbounded);
        Conduit<int, int> fresh = Conduit.make(Buffer<int>.Unbounded);
        Conduit<int, int> closing = Conduit.make(Buffer<int>.Unbounded);
        Atom<int> total = Atom(0);
        Fin<int> newest = Streams.Retained(Buffer<int>.Newest(2), Seq(1, 2, 3)).RunSafe();
        Fin<int> latest = Streams.Retained(Buffer<int>.Latest(0), Seq(1, 2, 3)).RunSafe();
        Fin<int> unbounded = Streams.Retained(Buffer<int>.Unbounded, Seq(1, 2, 3)).RunSafe();
        return Check(
            nameof(StreamProbe),
            ("Observed", Streams.Sum(Streams.Observed).RunSafe() == Pure(6)),
            ("Merged", Streams.Sum(Streams.Merged).RunSafe() == Pure(6)),
            ("Zipped", Streams.Zipped.Reduce(Seq<(int First, string Second)>(), static (s, a) => s.Add(a)).RunSafe() == Pure(Seq((1, "a"), (2, "b")))),
            ("UntilTwo", Streams.UntilTwo(Streams.Observed).RunSafe() == Pure(3)),
            ("PostLength", Streams.PostLength(fresh.Sink, "abc").RunSafe().IsSucc),
            ("Closed", Streams.Closed(closing.Sink).RunSafe().IsFail),
            ("Newest", newest == Pure(5)),
            ("Latest", latest == Pure(3)),
            ("Unbounded", unbounded == Pure(6)),
            ("Session", Streams.Session(inbox, replies).RunSafe() == Pure((5, 5))),
            ("Pipeline", Streams.Pipeline(total).RunSafe() == Pure(12)));
    }
}
