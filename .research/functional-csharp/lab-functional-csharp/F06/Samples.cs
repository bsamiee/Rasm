namespace Lab.F06;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        DelegationProbe,
        AdapterProbe,
        LifecycleProbe,
        CombinatorProbe,
        DataProbe,
        EdgeProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> DelegationProbe() {
        Guid known = Guid.NewGuid();
        Cache<string> cache = new(HashMap((known, "hit")));
        Atom<int> misses = Atom(0);
        string hit = cache.Get(known, () => string.Create(CultureInfo.InvariantCulture, $"miss {misses.Swap(static n => n + 1)}"));
        int afterHit = misses.Value;
        string miss = cache.Get(Guid.NewGuid(), () => string.Create(CultureInfo.InvariantCulture, $"miss {misses.Swap(static n => n + 1)}"));
        Seq<EnemyShip> ships = [new("Fighter", "Low"), new("Fighter", "High"), new("Cruiser", "High")];
        Report byType = Summaries.ByType(ships);
        Report byWeaponry = Summaries.ByWeaponry(ships);
        return Check(
            nameof(DelegationProbe),
            ("Hit", string.Equals(hit, "hit", StringComparison.Ordinal) && afterHit == 0),
            ("Miss", string.Equals(miss, "miss 1", StringComparison.Ordinal) && misses.Value == 1),
            ("ByType", byType.Rows == Seq(new ReportItem("Fighter", "2"), new ReportItem("Cruiser", "1"))),
            ("ByWeaponry", byWeaponry.Rows == Seq(new ReportItem("Low", "1"), new ReportItem("High", "2"))),
            ("Titles", string.Equals(byType.Title, "Enemy Ship Type", StringComparison.Ordinal) && string.Equals(byWeaponry.Title, "Enemy Ship Weaponry Level", StringComparison.Ordinal)));
    }

    private static Fin<Unit> AdapterProbe() =>
        Check(
            nameof(AdapterProbe),
            ("Subtract", Adapters.Subtract(10m, 3m) == 7m),
            ("SubtractFrom", Adapters.SubtractFrom(10m, 3m) == -7m),
            ("IsMod", Factories.IsMod(3)(9) && !Factories.IsMod(3)(10)),
            ("MultiplesOfThree", Factories.MultiplesOfThree == Seq(3, 6, 9, 12, 15, 18)));

    private static Fin<Unit> LifecycleProbe() {
        int before = Connection.Released;
        Fin<int> scoped = Lifecycles.Scoped.RunSafe();
        int afterScoped = Connection.Released;
        Fin<int> bracketed = Lifecycles.Bracketed.RunSafe();
        int afterBracketed = Connection.Released;
        return Check(
            nameof(LifecycleProbe),
            ("Scoped", scoped == Pure(42) && afterScoped == before + 1),
            ("Bracketed", bracketed == Pure(42) && afterBracketed == before + 2));
    }

    private static Fin<Unit> CombinatorProbe() {
        Atom<Seq<int>> seen = Atom(Seq<int>());
        Option<int> logged = Observers.Logged(Some(5), x => ignore(seen.Swap(s => s.Add(x))));
        Seq<int> traced = Observers.Traced(Seq(1, 2), x => ignore(seen.Swap(s => s.Add(x))));
        Atom<Seq<string>> notices = Atom(Seq<string>());
        Fin<Unit> empty = Guards.WarnWhenEmpty(0, message => ignore(notices.Swap(s => s.Add(message)))).RunSafe();
        Fin<Unit> stocked = Guards.WarnWhenEmpty(3, message => ignore(notices.Swap(s => s.Add(message)))).RunSafe();
        Fin<Unit> full = Guards.WarnWhenFull(150, message => ignore(notices.Swap(s => s.Add(message)))).RunSafe();
        Fin<Unit> normal = Guards.WarnWhenFull(50, message => ignore(notices.Swap(s => s.Add(message)))).RunSafe();
        Seq<int> summed = 4.Fork(static parts => parts, Seq<Func<int, int>>(static x => x + 1, static x => x * 2));
        return Check(
            nameof(CombinatorProbe),
            ("Celsius", string.Equals(Piping.Celsius(212m), "100 degrees C", StringComparison.Ordinal)),
            ("Average", Forks.Average(Seq(1.0, 2.0, 3.0)) == 2.0),
            ("ManyProngs", summed == Seq(5, 8)),
            ("Compose", string.Equals(Conversions.FormattedConversion(212m), "100 degrees", StringComparison.Ordinal)),
            ("ComposeOrder", compose(static (int x) => x + 1, static (int x) => x * 10)(1) == 20),
            ("Logged", logged == Some(5) && traced == Seq(1, 2) && seen.Value == Seq(5, 1, 2)),
            ("Unless", empty.IsSucc && stocked.IsSucc && full.IsSucc && normal.IsSucc && notices.Value == Seq("out of stock", "overstocked")));
    }

    private static Fin<Unit> DataProbe() {
        Atom<int> evaluated = Atom(0);
        Seq<Func<int, bool>> rules = [
            x => { _ = evaluated.Swap(static n => n + 1); return x > 0; },
            x => { _ = evaluated.Swap(static n => n + 1); return x < 10; },
        ];
        bool valid = 5.IsValid(rules);
        bool invalid = (-1).IsValid(rules);
        int afterValid = evaluated.Value;
        bool violated = (-1).IsInvalid(rules.Map(static rule => (Func<int, bool>)(x => !rule(x))));
        int afterViolation = evaluated.Value;
        Func<int, string> actorByNumber = Lookups.ActorByNumber(HashMap((5, "Ada")));
        string expected = string.Join(Environment.NewLine, "First name: Ada", "Last name: Lovelace", "Role: Analyst");
        return Check(
            nameof(DataProbe),
            ("Describe", string.Equals(Descriptions.Describe(new Employee("Ada", "Lovelace", "Analyst")), expected, StringComparison.Ordinal)),
            ("Valid", valid && !invalid && afterValid == 3),
            ("Invalid", violated && afterViolation == 4),
            ("EmptyRules", 5.IsValid(Seq<Func<int, bool>>()) && !5.IsInvalid(Seq<Func<int, bool>>())),
            ("NetIncomeFirst", RuleTables.NetIncome(10_000m) == 10_000m),
            ("NetIncomeMiddle", RuleTables.NetIncome(20_000m) == 16_000m),
            ("NetIncomeFallback", RuleTables.NetIncome(200_000m) == 110_000m),
            ("Known", string.Equals(actorByNumber(5), "Ada", StringComparison.Ordinal)),
            ("Unknown", string.Equals(actorByNumber(7), "Unknown", StringComparison.Ordinal)),
            ("Parsed", Parsing.ToInt("42", 0) == 42 && Parsing.ToInt("x", 7) == 7));
    }

    private static Trampoline<int> CountDown(int n) =>
        n == 0 ? Trampoline.Pure(0) : Trampoline.More(() => CountDown(n - 1));

    private static IO<int> CountUp(int limit) =>
        Monad.recur<IO, int, int>(0, i => i >= limit ? IO.pure(Next.Done<int, int>(i)) : IO.pure(Next.Loop<int, int>(i + 1))).As();

    private static Fin<Unit> EdgeProbe() {
        const int depth = 100_000;
        Fin<int> captured = Try.lift(static () => int.Parse("x", CultureInfo.InvariantCulture)).Run();
        Fin<int> deferred = IO.lift(static () => int.Parse("x", CultureInfo.InvariantCulture)).RunSafe();
        Seq<int> values = Seq(1, 2, 4, 5);
        bool adjacent = values.Zip(values.Tail).Exists(static pair => pair.Second - pair.First == 2);
        return Check(
            nameof(EdgeProbe),
            ("Captured", captured.IsFail),
            ("Deferred", deferred.IsFail),
            ("Adjacent", adjacent),
            ("Trampoline", CountDown(depth).Run() == 0),
            ("Recur", CountUp(depth).RunSafe() == Pure(depth)));
    }
}
