namespace Lab.F00;

internal sealed class Connection : IDisposable {
    public static int DisposedCount { get; private set; }

    public bool Disposed { get; private set; }

    public int Query() => Disposed ? 0 : 42;

    public void Dispose() {
        Disposed = true;
        DisposedCount++;
    }
}

internal sealed record Runtime(ConsoleIO Console) : Has<Eff<Runtime>, ConsoleIO> {
    static K<Eff<Runtime>, ConsoleIO> Has<Eff<Runtime>, ConsoleIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Console);
}

internal static class Effects {
    public static Fin<Unit> Probe() =>
        LiftProbe()
            .Bind(static _ => ExitProbe())
            .Bind(static _ => RuntimeProbe())
            .Bind(static _ => ResourceProbe())
            .Bind(static _ => ConcurrencyProbe())
            .Bind(static _ => RetryProbe());

    private static Fin<Unit> LiftProbe() {
        IO<int> plain = IO.lift(static () => 1);
        IO<int> folded = IO.lift(static () => Pure(2).ToFin());
        IO<Fin<int>> carried = IO.lift<Fin<int>>(static () => Pure(3));
        IO<int> evaluated = IO.lift(Pure(4).ToFin());
        IO<int> asynchronous = IO.liftAsync(static () => Task.FromResult(5));
        IO<int> tokenAware = IO.liftAsync(static env => Task.FromResult(env.Token.IsCancellationRequested ? 0 : 6));
        IO<int> failed = IO.fail<int>(new InvalidAge());
        IO<int> chained =
            from a in plain
            from b in folded
            from c in evaluated
            select a + b + c;
        Eff<Runtime, int> lifted = plain;
        Eff<Runtime, int> explicitLift = Eff<Runtime, int>.LiftIO(plain);
        return Verify.Check(
            nameof(LiftProbe),
            ("chained.RunSafe() == Pure(7)", chained.RunSafe() == Pure(7)),
            ("carried.RunSafe().Exists(static f => f.IsSucc)", carried.RunSafe().Exists(static f => f.IsSucc)),
            ("asynchronous.RunSafe() == Pure(5)", asynchronous.RunSafe() == Pure(5)),
            ("tokenAware.RunSafe() == Pure(6)", tokenAware.RunSafe() == Pure(6)),
            ("failed.RunSafe().IsFail", failed.RunSafe().IsFail),
            ("lifted.Run(new Runtime(new LanguageExt.Sys.Test.Implementations.ConsoleIO(new MemoryConsole()))) == Pure(1)", lifted.Run(new Runtime(new LanguageExt.Sys.Test.Implementations.ConsoleIO(new MemoryConsole()))) == Pure(1)),
            ("explicitLift.Run(new Runtime(new LanguageExt.Sys.Test.Implementations.ConsoleIO(new MemoryConsole()))) == Pure(1)", explicitLift.Run(new Runtime(new LanguageExt.Sys.Test.Implementations.ConsoleIO(new MemoryConsole()))) == Pure(1)));
    }

    private static Fin<Unit> ExitProbe() {
        IO<int> failing = IO.fail<int>(new InvalidAge());
        Fin<int> safe = failing.RunSafe();
        Fin<int> inside = failing.Try().runFin.As().Run();
        Fin<int> thrown = Try.lift(failing.Run).Run();
        IO<int> recovered = failing.Catch(1001, static _ => IO.pure(9)).As();
        IO<int> alternative = failing | IO.pure(8);
        int hostValue = failing.RunSafe().IfFail(static _ => -1);
        return Verify.Check(
            nameof(ExitProbe),
            ("safe.IsFail", safe.IsFail),
            ("inside.IsFail", inside.IsFail),
            ("thrown.IsFail", thrown.IsFail),
            ("recovered.RunSafe() == Pure(9)", recovered.RunSafe() == Pure(9)),
            ("alternative.RunSafe() == Pure(8)", alternative.RunSafe() == Pure(8)),
            ("hostValue == -1", hostValue == -1));
    }

    private static Eff<RT, Unit> Greet<RT>(string name) where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine(name)
        from line in Console<RT>.readLine
        from __ in Console<RT>.writeLine(line)
        select unit;

    private static Fin<Unit> RuntimeProbe() {
        MemoryConsole memory = new();
        Runtime custom = new(new LanguageExt.Sys.Test.Implementations.ConsoleIO(memory));
        _ = memory.WriteKeyLine("reply");
        Fin<Unit> customRun = Greet<Runtime>("hello").Run(custom);
        LanguageExt.Sys.Test.Runtime test = LanguageExt.Sys.Test.Runtime.New();
        _ = test.Env.Console.WriteKeyLine("again");
        Fin<Unit> testRun = Greet<LanguageExt.Sys.Test.Runtime>("hi").Run(test);
        Seq<string> customLines = toSeq(memory);
        Seq<string> testLines = toSeq(test.Env.Console);
        Eff<Runtime, Runtime> runtime = Eff.runtime<Runtime>();
        return Verify.Check(
            nameof(RuntimeProbe),
            ("customRun.IsSucc", customRun.IsSucc),
            ("testRun.IsSucc", testRun.IsSucc),
            ("customLines == Seq(\"hello\", \"reply\")", customLines == Seq("hello", "reply")),
            ("testLines == Seq(\"hi\", \"again\")", testLines == Seq("hi", "again")),
            ("runtime.Run(custom).IsSucc", runtime.Run(custom).IsSucc));
    }

    private static Fin<Unit> ResourceProbe() {
        int before = Connection.DisposedCount;
        IO<int> viaUse =
            from connection in use(static () => new Connection())
            select connection.Query();
        Fin<int> useResult = viaUse.RunSafe();
        int afterUse = Connection.DisposedCount;
        IO<int> viaBracket = IO.lift(static () => new Connection()).Bracket(Use: static c => IO.pure(c.Query()), Fin: static c => IO.lift(fun(c.Dispose)));
        Fin<int> bracketResult = viaBracket.RunSafe();
        int afterBracket = Connection.DisposedCount;
        IO<int> viaRelease =
            from connection in use(static () => new Connection(), static c => c.Dispose())
            select connection.Query();
        Fin<int> releaseResult = viaRelease.RunSafe();
        int afterRelease = Connection.DisposedCount;
        return Verify.Check(
            nameof(ResourceProbe),
            ("useResult == Pure(42)", useResult == Pure(42)),
            ("afterUse == (before + 1)", afterUse == (before + 1)),
            ("bracketResult == Pure(42)", bracketResult == Pure(42)),
            ("afterBracket == (before + 2)", afterBracket == (before + 2)),
            ("releaseResult == Pure(42)", releaseResult == Pure(42)),
            ("afterRelease == (before + 3)", afterRelease == (before + 3)));
    }

    private static Fin<Unit> ConcurrencyProbe() {
        IO<int> left = IO.pure(1);
        IO<int> right = IO.pure(2);
        IO<int> forked =
            from f1 in left.Fork()
            from f2 in right.Fork()
            from a in f1.Await
            from b in f2.Await
            select a + b;
        Seq<IO<int>> jobs = [left, right];
        IO<Seq<int>> all = awaitAll(jobs);
        IO<int> any = awaitAny(jobs);
        IO<int> bounded = timeout(TimeSpan.FromSeconds(1), left);
        IO<Seq<int>> parallel = Seq(1, 2, 3).Traverse(static x => IO.pure(x * 2)).As();
        IO<Seq<int>> serial = Seq(1, 2, 3).TraverseM(static x => IO.pure(x * 3)).As();
        Seq<K<IO, int>> mixed = [IO.pure(1), IO.fail<int>(new InvalidAge())];
        IO<(Seq<Error> Fails, Seq<int> Succs)> partitioned = mixed.PartitionFallible().As();
        Fin<(Seq<Error> Fails, Seq<int> Succs)> parts = partitioned.RunSafe();
        return Verify.Check(
            nameof(ConcurrencyProbe),
            ("forked.RunSafe() == Pure(3)", forked.RunSafe() == Pure(3)),
            ("all.RunSafe() == Pure(Seq(1, 2))", all.RunSafe() == Pure(Seq(1, 2))),
            ("any.RunSafe().IsSucc", any.RunSafe().IsSucc),
            ("bounded.RunSafe() == Pure(1)", bounded.RunSafe() == Pure(1)),
            ("parallel.RunSafe() == Pure(Seq(2, 4, 6))", parallel.RunSafe() == Pure(Seq(2, 4, 6))),
            ("serial.RunSafe() == Pure(Seq(3, 6, 9))", serial.RunSafe() == Pure(Seq(3, 6, 9))),
            ("parts.Exists(static p => p.Fails.Count == 1 && p.Succs == Seq(1))", parts.Exists(static p => p.Fails.Count == 1 && p.Succs == Seq(1))));
    }

    private static Fin<Unit> RetryProbe() {
        Atom<int> attempts = Atom(0);
        IO<int> flaky = IO.lift(() => attempts.Swap(static n => n + 1)).Bind(static n => n < 3 ? IO.fail<int>(new InvalidAge()) : IO.pure(7));
        Schedule policy = Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(5);
        Schedule backoff = Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3) | Schedule.jitter();
        Schedule capped = Schedule.exponential(TimeSpan.FromMilliseconds(1)) & Schedule.maxDelay(TimeSpan.FromMilliseconds(4));
        Fin<int> retried = flaky.Retry(policy).RunSafe();
        Atom<int> polls = Atom(0);
        IO<int> step = IO.lift(() => polls.Swap(static n => n + 1));
        Fin<int> polled = step.RepeatUntil(static n => n >= 3).RunSafe();
        Fin<int> repeated = step.Repeat(Schedule.recurs(2)).RunSafe();
        return Verify.Check(
            nameof(RetryProbe),
            ("retried == Pure(7)", retried == Pure(7)),
            ("attempts.Value == 3", attempts.Value == 3),
            ("polled == Pure(3)", polled == Pure(3)),
            ("repeated.IsSucc", repeated.IsSucc),
            ("backoff.Run().Count() == 3", backoff.Run().Count() == 3),
            ("capped.Run().Take(3).Count() == 3", capped.Run().Take(3).Count() == 3));
    }
}
