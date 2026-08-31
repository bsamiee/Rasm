namespace Lab.F00;

internal static class Collections {
    private static readonly Lens<Person, int> AgeLens = Lens<Person, int>.New(static p => p.Age, static a => p => p with { Age = a });

    public static Fin<Unit> Probe() =>
        SeqProbe()
            .Bind(static _ => TraverseProbe())
            .Bind(static _ => SharedStateProbe());

    private static Fin<Unit> SeqProbe() {
        Seq<(int First, string Second)> pairs = Seq(1, 2).Zip(Seq("a", "b"));
        Seq<(int First, string Second)> expectedPairs = [(1, "a"), (2, "b")];
        Seq<string> projected = Seq(1, 2).Zip(Seq("a", "b"), static (n, s) => string.Create(CultureInfo.InvariantCulture, $"{n}{s}"));
        Seq<int> ranged = toSeq(Range(1, 3));
        Option<int> second = Seq(1, 2, 3).At(1);
        Option<int> missing = Seq(1, 2, 3).At(5);
        Seq<int> chosen = Seq("1", "x", "3").Choose(static s => parseInt(s));
        Seq<int> running = Seq(1, 2, 3).Scan(0, static (acc, x) => acc + x);
        (Seq<int> evens, Seq<int> odds) = Seq(1, 2, 3, 4).Partition(static x => x % 2 == 0);
        bool adjacent = Seq(1, 2, 4).Zip(Seq(1, 2, 4).Tail).Exists(static p => (p.Second - p.First) == 1);
        Seq<int> indexed = Seq(10, 20).Map(static (x, i) => x + i);
        Seq<int> consed = 0.Cons(Seq(1));
        Lst<int> inserted = List(1, 2, 3).Insert(1, 9);
        Option<int> found = HashMap(("a", 1)).Find("a");
        Person older = AgeLens.Update(static a => a + 1, new Person("Ada", 36));
        Person reset = AgeLens.Set(0, older);
        Seq<int> somes = Seq(Some(1), Option<int>.None, Some(3)).Somes();
        return Verify.Check(
            nameof(SeqProbe),
            ("pairs == expectedPairs", pairs == expectedPairs),
            ("projected == Seq(\"1a\", \"2b\")", projected == Seq("1a", "2b")),
            ("ranged == Seq(1, 2, 3)", ranged == Seq(1, 2, 3)),
            ("second == Some(2)", second == Some(2)),
            ("missing.IsNone", missing.IsNone),
            ("chosen == Seq(1, 3)", chosen == Seq(1, 3)),
            ("running == Seq(0, 1, 3, 6)", running == Seq(0, 1, 3, 6)),
            ("evens == Seq(2, 4)", evens == Seq(2, 4)),
            ("odds == Seq(1, 3)", odds == Seq(1, 3)),
            ("adjacent", adjacent),
            ("indexed == Seq(10, 21)", indexed == Seq(10, 21)),
            ("consed == Seq(0, 1)", consed == Seq(0, 1)),
            ("inserted == List(1, 9, 2, 3)", inserted == List(1, 9, 2, 3)),
            ("found == Some(1)", found == Some(1)),
            ("older.Age == 37", older.Age == 37),
            ("reset.Age == 0", reset.Age == 0),
            ("somes == Seq(1, 3)", somes == Seq(1, 3)));
    }

    private static Validation<Error, int> ValidNumber(string text) => parseInt(text).ToValidation<Error>(Error.New($"bad {text}"));

    private static Fin<Unit> TraverseProbe() {
        Validation<Error, Seq<int>> accumulated = Seq("1", "x", "y").Traverse(ValidNumber).As();
        Validation<Error, Seq<int>> shortCircuited = Seq("1", "x", "y").TraverseM(ValidNumber).As();
        Option<Seq<int>> parsed = Seq("1", "2").Traverse(static s => parseInt(s)).As();
        int accumulatedCount = accumulated.Match(Fail: static e => e.Count, Succ: static _ => 0);
        int shortCount = shortCircuited.Match(Fail: static e => e.Count, Succ: static _ => 0);
        return Verify.Check(
            nameof(TraverseProbe),
            ("accumulatedCount == 2", accumulatedCount == 2),
            ("shortCount == 1", shortCount == 1),
            ("parsed == Some(Seq(1, 2))", parsed == Some(Seq(1, 2))));
    }

    private static Fin<Unit> SharedStateProbe() {
        Atom<int> counter = Atom(0);
        int swapped = counter.Swap(static n => n + 1);
        int kept = counter.SwapMaybe(static n => n > 5 ? Some(n + 1) : Option<int>.None);
        AtomHashMap<string, int> registry = AtomHashMap<string, int>();
        _ = registry.TryAdd("a", 1);
        _ = registry.TryAdd("a", 2);
        _ = registry.SwapKey("a", static v => v + 10);
        _ = registry.SwapKey("b", static v => v.Map(static n => n + 1) | Some(0));
        Option<int> a = registry.Find("a");
        Option<int> b = registry.Find("b");
        Ref<int> balance = Ref(100);
        Ref<int> savings = Ref(0);
        _ = atomic(() => { _ = swap(balance, static v => v - 30); return swap(savings, static v => v + 30); });
        Func<int, int> square = memo(static (int x) => x * x);
        Memo<int> once = memo(static () => 9);
        return Verify.Check(
            nameof(SharedStateProbe),
            ("swapped == 1", swapped == 1),
            ("kept == 1", kept == 1),
            ("a == Some(11)", a == Some(11)),
            ("b == Some(0)", b == Some(0)),
            ("balance.Value == 70", balance.Value == 70),
            ("savings.Value == 30", savings.Value == 30),
            ("square(3) == 9", square(3) == 9),
            ("once.Value == 9", once.Value == 9));
    }
}
