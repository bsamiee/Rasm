namespace Lab.F16;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        CacheProbe,
        EffectfulCacheProbe,
        GeneratorProbe,
        NumberingProbe,
        TraitProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> CacheProbe() {
        AtomHashMap<string, int> calls = AtomHashMap<string, int>();
        Func<string, decimal> fetch = pair => {
            _ = calls.SwapKey(pair, static n => n.Map(static c => c + 1) | Some(1));
            return 1.5m;
        };
        State<HashMap<string, decimal>, decimal> twice =
            from first in RateCache.GetRate(fetch, "USDEUR")
            from second in RateCache.GetRate(fetch, "USDEUR")
            select first + second;
        (decimal Value, HashMap<string, decimal> State) outcome = twice.Run(HashMap<string, decimal>());
        return Check(
            nameof(CacheProbe),
            ("outcome.Value == 3.0m", outcome.Value == 3.0m),
            ("outcome.State.Find(\"USDEUR\") == Some(1.5m)", outcome.State.Find("USDEUR") == Some(1.5m)),
            ("calls.Find(\"USDEUR\") == Some(1)", calls.Find("USDEUR") == Some(1)));
    }

    private static Fin<Unit> EffectfulCacheProbe() {
        AtomHashMap<string, int> calls = AtomHashMap<string, int>();
        Func<string, IO<decimal>> fetch = pair => IO.lift(() => {
            _ = calls.SwapKey(pair, static n => n.Map(static c => c + 1) | Some(1));
            return 2m;
        });
        Func<string, IO<decimal>> failing = static _ => IO.fail<decimal>(new RateUnavailable());
        StateT<HashMap<string, decimal>, IO, decimal> twice =
            from first in EffectfulRateCache.GetRate(fetch, "USDEUR")
            from second in EffectfulRateCache.GetRate(fetch, "USDEUR")
            select first + second;
        Fin<(decimal Value, HashMap<string, decimal> State)> outcome = twice.Run(HashMap<string, decimal>()).As().RunSafe();
        Fin<(decimal Value, HashMap<string, decimal> State)> hit = EffectfulRateCache.GetRate(failing, "USDEUR").Run(HashMap(("USDEUR", 3m))).As().RunSafe();
        Fin<(decimal Value, HashMap<string, decimal> State)> miss = EffectfulRateCache.GetRate(failing, "USDEUR").Run(HashMap<string, decimal>()).As().RunSafe();
        return Check(
            nameof(EffectfulCacheProbe),
            ("outcome.Exists(static o => o.Value == 4m)", outcome.Exists(static o => o.Value == 4m)),
            ("outcome.Exists(static o => o.State.Find(\"USDEUR\") == Some(2m))", outcome.Exists(static o => o.State.Find("USDEUR") == Some(2m))),
            ("calls.Find(\"USDEUR\") == Some(1)", calls.Find("USDEUR") == Some(1)),
            ("hit.Exists(static o => o.Value == 3m)", hit.Exists(static o => o.Value == 3m)),
            ("miss.IsFail", miss.IsFail),
            ("miss.Match(Succ: static _ => false, Fail: static e => e.HasCode(1600))", miss.Match(Succ: static _ => false, Fail: static e => e.HasCode(1600))));
    }

    private static Fin<Unit> GeneratorProbe() {
        (int Value, int State) first = Generator.NextInt.Run(1);
        (int Value, int State) again = Generator.NextInt.Run(1);
        (bool Value, int State) flag = Generator.NextBool.Run(1);
        ((int First, int Second) Value, int State) pair = Generator.PairOfInts.Run(1);
        (Option<int> Value, int State) option = Generator.OptionInt.Run(1);
        (Seq<int> Value, int State) list = Generator.IntList.Run(1);
        (Seq<int> Value, int State) empty = Generator.Empty.Run(7);
        return Check(
            nameof(GeneratorProbe),
            ("first.Value == first.State", first.Value == first.State),
            ("first == again", first == again),
            ("flag.State == first.State", flag.State == first.State),
            ("pair.Value.First == first.Value", pair.Value.First == first.Value),
            ("pair.Value.Second == Generator.NextInt.Run(first.State).Value", pair.Value.Second == Generator.NextInt.Run(first.State).Value),
            ("pair.Value.First != pair.Value.Second", pair.Value.First != pair.Value.Second),
            ("option.State == pair.State", option.State == pair.State),
            ("list.Value.Count >= 1", list.Value.Count >= 1),
            ("empty == (Seq<int>(), 7)", empty == (Seq<int>(), 7)));
    }

    private static Fin<Unit> NumberingProbe() {
        Tree<string> tree = Tree.Branch(Tree.Leaf("a"), Tree.Branch(Tree.Leaf("b"), Tree.Leaf("c")));
        Tree<(int Number, string Value)> expected = Tree.Branch(Tree.Leaf((0, "a")), Tree.Branch(Tree.Leaf((1, "b")), Tree.Leaf((2, "c"))));
        (Tree<(int Number, string Value)> Value, int State) outcome = tree.Number().Run(0);
        Tree<(int Number, string Value)> numbered = Numbering.Numbered(tree);
        return Check(
            nameof(NumberingProbe),
            ("outcome.Value == expected", outcome.Value == expected),
            ("outcome.State == 3", outcome.State == 3),
            ("numbered == expected", numbered == expected),
            ("Numbering.GetAndIncrement.Run(5) == (5, 6)", Numbering.GetAndIncrement.Run(5) == (5, 6)));
    }

    private static K<M, A> ForkAndAwait<M, A>(K<M, A> ma) where M : Maybe.MonadUnliftIO<M>, Monad<M> =>
        M.AwaitMaybe(M.ForkIOMaybe(ma));

    private static Fin<Unit> TraitProbe() {
        State<int, int> viaTrait = Stateful.state<State<int>, int, int>(static seed => (seed, seed + 1)).As();
        State<int, int> scoped = Stateful.local<State<int>, int, int>(static s => s + 100, Generator.NextInt).As();
        (int Value, int State) local = scoped.Run(1);
        StateT<int, IO, Unit> increment = StateT.modify<IO, int>(static n => n + 1);
        Fin<StateT<int, IO, Unit>> unsupported = Try.lift(() => ForkAndAwait(increment).As()).Run();
        StateT<int, IO, int> forked =
            from fork in StateT.liftIO<int, IO, ForkIO<int>>(IO.pure(41).Fork())
            from value in fork.Await
            from _ in increment
            from n in StateT.get<IO, int>()
            select value + n;
        Fin<(int Value, int State)> parent = forked.Run(0).As().RunSafe();
        return Check(
            nameof(TraitProbe),
            ("viaTrait.Run(5) == (5, 6)", viaTrait.Run(5) == (5, 6)),
            ("local.State == 1", local.State == 1),
            ("local.Value == Generator.NextInt.Run(101).Value", local.Value == Generator.NextInt.Run(101).Value),
            ("unsupported.IsFail", unsupported.IsFail),
            ("parent == Pure((42, 1))", parent == Pure((42, 1))));
    }
}
