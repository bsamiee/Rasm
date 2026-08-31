namespace Lab.F18;

internal static class Samples {
    private static readonly Flight Cheap = new("a", 100m);
    private static readonly Flight Dear = new("b", 200m);
    private static readonly Guid Known = new("11111111-1111-1111-1111-111111111111");
    private static readonly Map<Guid, AccountState> Accounts = Map((Known, new AccountState(Known, 50m)));

    public static Fin<Unit> Run() =>
        LiftsSample()
            .Bind(static _ => PoliciesSample())
            .Bind(static _ => IndependenceSample())
            .Bind(static _ => TraversalsSample())
            .Bind(static _ => LayersSample())
            .Bind(static _ => StacksSample());

    private static OptionT<IO, AccountState> Lookup(Guid id) =>
        OptionT.lift<IO, AccountState>(IO.pure(Accounts.Find(id)));

    private static Validation<Error, DebitCommand> Validate(DebitCommand command) =>
        command.Amount > 0m ? command : Error.New("amount must be positive");

    private static Fin<Unit> LiftsSample() {
        IO<Flight> fetched = Lifts.Fetch(static () => Task.FromResult(Cheap));
        IO<Flight> withToken = Lifts.FetchWithToken(static token => Task.FromResult(token.IsCancellationRequested ? Dear : Cheap));
        return Check(
            nameof(LiftsSample),
            ("Known", Lifts.Known(Cheap).RunSafe() == Pure(Cheap)),
            ("Fetch", fetched.RunSafe() == Pure(Cheap)),
            ("FetchWithToken", withToken.RunSafe() == Pure(Cheap)),
            ("Price", Lifts.Price(fetched).RunSafe() == Pure(100m)),
            ("Seats", Lifts.Seats(fetched, static f => IO.pure(f.Airline.Length)).RunSafe() == Pure(1)),
            ("Total", Lifts.Total(fetched, Lifts.Known(Dear)).RunSafe() == Pure(300m)));
    }

    private static Fin<Unit> PoliciesSample() {
        IO<Flight> down = IO.fail<Flight>(new ProviderDown());
        IO<Flight> other = IO.fail<Flight>(Error.New("other"));
        Atom<int> attempts = Atom(0);
        IO<Flight> flaky = IO.lift(() => attempts.Swap(static n => n + 1)).Bind(static n => n < 3 ? IO.fail<Flight>(new ProviderDown()) : IO.pure(Cheap));
        Fin<Flight> retried = Policies.Retry(flaky).RunSafe();
        return Check(
            nameof(PoliciesSample),
            ("Fallback", Policies.Fallback(down, IO.pure(Dear)).RunSafe() == Pure(Dear)),
            ("FallbackOnOutage", Policies.FallbackOnOutage(down, IO.pure(Dear)).RunSafe() == Pure(Dear)),
            ("FallbackOnOutage ignores other", Policies.FallbackOnOutage(other, IO.pure(Dear)).RunSafe().IsFail),
            ("Recover", Host.Recover(down, Dear) == Dear),
            ("Retry", retried == Pure(Cheap)),
            ("Retry attempts", attempts.Value == 3));
    }

    private static Fin<Unit> IndependenceSample() =>
        Check(
            nameof(IndependenceSample),
            ("Best", Independence.Best(IO.pure(Dear), IO.pure(Cheap)).RunSafe() == Pure(Cheap)),
            ("BestForked", Independence.BestForked(IO.pure(Dear), IO.pure(Cheap)).RunSafe() == Pure(Cheap)),
            ("All", Independence.All(Seq(IO.pure(Cheap), IO.pure(Dear))).RunSafe() == Pure(Seq(Cheap, Dear))));

    private static Fin<Unit> TraversalsSample() {
        Seq<Airline> airlines = Seq(new Airline("a", IO.pure(Seq(Cheap))), new Airline("b", IO.pure(Seq(Dear))));
        Seq<Airline> mixed = airlines.Add(new Airline("c", IO.fail<Seq<Flight>>(new ProviderDown())));
        return Check(
            nameof(TraversalsSample),
            ("ParseAll", Traversals.ParseAll(Seq("1", "2.5")) == Some(Seq(1.0, 2.5))),
            ("ParseAll none", Traversals.ParseAll(Seq("1", "x")).IsNone),
            ("ValidateAll accumulates", Traversals.ValidateAll(Seq("1", "x", "y"), Number).Match(Fail: static e => e.Count, Succ: static _ => 0) == 2),
            ("ValidateUntilFirstFailure", Traversals.ValidateUntilFirstFailure(Seq("1", "x", "y"), Number).Match(Fail: static e => e.Count, Succ: static _ => 0) == 1),
            ("SearchParallel", Traversals.SearchParallel(airlines).RunSafe() == Pure(Seq(Cheap, Dear))),
            ("SearchSerial", Traversals.SearchSerial(airlines).RunSafe() == Pure(Seq(Cheap, Dear))),
            ("SearchParallel fails", Traversals.SearchParallel(mixed).RunSafe().IsFail),
            ("SearchBestEffort", Traversals.SearchBestEffort(mixed).RunSafe() == Pure(Seq(Cheap, Dear))));
    }

    private static Validation<Error, int> Number(string text) =>
        parseInt(text).ToValidation<Error>(Error.New($"bad {text}"));

    private static Fin<Unit> LayersSample() =>
        Check(
            nameof(LayersSample),
            ("Parse some", Layers.Parse("7").Exists(static v => v.IsSuccess)),
            ("Parse none", Layers.Parse("x").IsNone),
            ("Parse keeps failure", Layers.Parse(Error.New("bad")).Exists(static v => v.IsFail)),
            ("Swap", Layers.Swap(Some(3)).Exists(static v => v.IsSuccess)),
            ("Swap none", Layers.Swap(Option<int>.None).IsNone));

    private static Fin<Unit> StacksSample() {
        Guid unknown = new("22222222-2222-2222-2222-222222222222");
        Atom<int> published = Atom(0);
        Func<Task> publish = () => { _ = published.Swap(static n => n + 1); return Task.CompletedTask; };
        IO<AccountState> debited = Stacks.Debit(Validate, Lookup, publish, new DebitCommand(Known, 20m));
        IO<AccountState> missing = Stacks.Debit(Validate, Lookup, publish, new DebitCommand(unknown, 20m));
        IO<AccountState> overdrawn = Stacks.Debit(Validate, Lookup, publish, new DebitCommand(Known, 80m));
        IO<AccountState> invalid = Stacks.Debit(Validate, Lookup, publish, new DebitCommand(Known, -1m));
        Fin<AccountState> result = debited.RunSafe();
        Fin<AccountState> asynchronous = IO.liftAsync(() => Host.ExitAsync(debited, new Runtime())).RunSafe().Bind(static fin => fin);
        return Check(
            nameof(StacksSample),
            ("GetAccount", Stacks.GetAccount(Lookup, Known).RunSafe().IsSucc),
            ("GetAccount unknown", Stacks.GetAccount(Lookup, unknown).RunSafe().Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.UnknownAccount))),
            ("Converted", Stacks.Converted(Lookup, Known, IO.pure(2m)).Run().As().RunSafe() == Pure(Some(100m))),
            ("SaveAndPublish", Stacks.SaveAndPublish(publish).RunSafe().IsSucc),
            ("Debit", result.Map(static s => s.Balance) == Pure(30m)),
            ("Debit missing", missing.RunSafe().Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.UnknownAccount))),
            ("Debit overdrawn", overdrawn.RunSafe().Match(Succ: static _ => false, Fail: static e => e.HasCode(Codes.InsufficientFunds))),
            ("Debit invalid", invalid.RunSafe().IsFail),
            ("Exit", Host.Exit(debited) == 0),
            ("Exit expected", Host.Exit(overdrawn) == 4),
            ("ExitAsync", asynchronous.IsSucc),
            ("published", published.Value >= 2));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
