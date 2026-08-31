namespace Lab.F24;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        PartitionProbe,
        FoldProbe,
        ShapeProbe,
        EqualityProbe,
        LensProbe,
        SharedStateProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> PartitionProbe() {
        List<int> source = [1, 2, 3];
        Seq<int> converted = Partition.Converted(source);
        source.Add(4);
        Lst<int> edited = Partition.Edited(Partition.Editable);
        Seq<int> doubled = Partition.Doubled(source);
        return Check(
            nameof(PartitionProbe),
            ("Strict", Partition.Strict == Seq(1, 2, 3)),
            ("Converted", converted == Seq(1, 2, 3)),
            ("Third", Partition.Third(Partition.Indexed) == 30),
            ("Edited", edited == List(7, 2, 3)),
            ("Ordered", Partition.Ordered.Keys.ToSeq() == Seq("a", "b")),
            ("Hashed", Partition.Hashed.Find("b") == Some(2)),
            ("HashedFrom", Partition.HashedFrom(Seq(("a", 1))).Find("a") == Some(1)),
            ("Sorted", Partition.Sorted.ToSeq() == Seq(1, 2, 3)),
            ("SortedFrom", Partition.SortedFrom(Seq(2, 2, 1)).Count == 2),
            ("Unordered", Partition.Unordered.Contains(3)),
            ("Doubled", doubled == Seq(2, 4, 6, 8)),
            ("NonEmpty", Partition.NonEmpty.Head == 1 && Partition.NonEmpty.Count() == 3),
            ("NonEmptyFrom", Partition.NonEmptyFrom(Seq<int>()).IsNone && Partition.NonEmptyFrom(Seq(1)).IsSome),
            ("Counted", Partition.Counted == Seq(1, 2, 3)));
    }

    private static Fin<Unit> FoldProbe() {
        Atom<int> anyVisited = Atom(0);
        Atom<int> allVisited = Atom(0);
        bool anyEven = Folds.AnyEven(Seq(1, 2, 3, 4), anyVisited);
        bool allPositive = Folds.AllPositive(Seq(1, -1, 3, 4), allVisited);
        return Check(
            nameof(FoldProbe),
            ("Total", Folds.Total(Seq(1, 2, 3)) == 6),
            ("Forward", string.Equals(Folds.Forward(Seq(1, 2, 3)), "123", StringComparison.Ordinal)),
            ("Backward", string.Equals(Folds.Backward(Seq(1, 2, 3)), "321", StringComparison.Ordinal)),
            ("WhileUnderTen", Folds.WhileUnderTen(Seq(4, 4, 4, 4)) == 12),
            ("UntilNegative", Folds.UntilNegative(Seq(1, 2, -1, 5)) == 3),
            ("MonadicTailFirst", Folds.MonadicTailFirst(Seq(1, 2, 3)) == Some("321")),
            ("MonadicHeadFirst", Folds.MonadicHeadFirst(Seq(1, 2, 3)) == Some("123")),
            ("Joined", Folds.Joined(Seq(Seq(1), Seq(2, 3))) == Seq(2, 3, 1)),
            ("AnyEven", anyEven && anyVisited.Value == 2),
            ("AllPositive", !allPositive && allVisited.Value == 2));
    }

    private static Fin<Unit> ShapeProbe() {
        (Seq<int> evens, Seq<int> odds) = Shapes.Split(Seq(1, 2, 3, 4));
        Seq<(int First, string Second)> expected = [(1, "a"), (2, "b")];
        return Check(
            nameof(ShapeProbe),
            ("Parsed", Shapes.Parsed(Seq("1", "x", "3")) == Seq(1, 3)),
            ("Split", evens == Seq(2, 4) && odds == Seq(1, 3)),
            ("Paired", Shapes.Paired(Seq(1, 2), Seq("a", "b")) == expected),
            ("Labelled", Shapes.Labelled(Seq(1, 2), Seq("a", "b")) == Seq("1:a", "2:b")),
            ("Running", Shapes.Running(Seq(1, 2, 3)) == Seq(0, 1, 3, 6)),
            ("Second", Shapes.Second(Seq(1, 2, 3)) == Some(2) && Shapes.Second(Seq(1)).IsNone),
            ("First", Shapes.First(Seq(1, 2)) == Some(1) && Shapes.First(Seq<int>()).IsNone),
            ("Rest", Shapes.Rest(Seq(1, 2, 3)) == Seq(2, 3) && Shapes.Rest(Seq<int>()).IsEmpty),
            ("Final", Shapes.Final(Seq(1, 2, 3)) == Some(3)),
            ("Offset", Shapes.Offset(Seq(10, 20)) == Seq(10, 21)),
            ("Positive", Shapes.Positive(Seq(-1, 1, 2)) == Seq(1, 2)),
            ("Flattened", Shapes.Flattened(Seq(Seq(1), Seq(2, 3))) == Seq(1, 2, 3)),
            ("Present", Shapes.Present(Seq(Some(1), Option<int>.None, Some(3))) == Seq(1, 3)),
            ("Reversed", Shapes.Reversed(Seq(1, 2, 3)) == Seq(3, 2, 1)),
            ("Doubling", Shapes.Doubling(16) == Seq(1, 2, 4, 8, 16)),
            ("Prepended", Shapes.Prepended(0, Seq(1)) == Seq(0, 1)));
    }

    private static Fin<Unit> EqualityProbe() =>
        Check(
            nameof(EqualityProbe),
            ("SameItems", Equality.SameItems(Seq(1, 2), Seq(1, 2)) && !Equality.SameItems(Seq(1, 2), Seq(2, 1))),
            ("SamePairs", Equality.SamePairs(Seq(1, 2), Seq("a", "b"))),
            ("Has", Equality.Has(Seq("a", "b"), "b") && !Equality.Has(Seq("a"), "b")),
            ("Sum", Equality.Sum(Seq(1, 2, 3)) == 6),
            ("Empty", Equality.Empty().IsEmpty),
            ("Ascending", Equality.Ascending(Seq(5, 9, 4)) == Seq(4, 5, 9)));

    private static Fin<Unit> LensProbe() {
        Customer ada = new("Ada", new Address("London", "n1"));
        Customer moved = Lenses.Moved(ada, "e2");
        Customer upper = Lenses.Uppercased(ada);
        return Check(
            nameof(LensProbe),
            ("Moved", string.Equals(moved.Address.Postcode, "e2", StringComparison.Ordinal)),
            ("Unchanged", string.Equals(ada.Address.Postcode, "n1", StringComparison.Ordinal)),
            ("Uppercased", string.Equals(upper.Address.Postcode, "N1", StringComparison.Ordinal)),
            ("City", string.Equals(upper.Address.City, "London", StringComparison.Ordinal)),
            ("Read", string.Equals(Lenses.Read(moved), "e2", StringComparison.Ordinal)));
    }

    private static Fin<Unit> SharedStateProbe() {
        Atom<int> counter = Atom(0);
        int first = SharedState.Increment(counter);
        int capped = SharedState.Capped(counter, 1);
        AtomHashMap<string, int> registry = AtomHashMap<string, int>();
        _ = SharedState.Register(registry, "a", 1);
        _ = SharedState.Register(registry, "a", 9);
        _ = SharedState.Bump(registry, "a");
        _ = SharedState.BumpOrStart(registry, "b");
        int kept = SharedState.ReadOrRegister(registry, "a", 50);
        int added = SharedState.ReadOrRegister(registry, "c", 50);
        Ref<decimal> checking = Ref(100m);
        Ref<decimal> savings = Ref(0m);
        decimal moved = SharedState.Move(checking, savings, 30m);
        TrackingHashMap<string, int> tracked = SharedState.Tracked(TrackingHashMap<string, int>());
        TrackingHashMap<string, int> cleared = SharedState.Cleared(tracked);
        Atom<int> squareCalls = Atom(0);
        Func<int, int> squares = SharedState.Squares(squareCalls);
        int nine = squares(3) + squares(3);
        Atom<int> onceCalls = Atom(0);
        Memo<int> once = SharedState.Once(onceCalls);
        int twice = once.Value + once.Value;
        Atom<int> builds = Atom(0);
        Atom<int> runs = Atom(0);
        Memo<IO, int> built = SharedState.Built(builds, runs);
        Fin<int> firstRun = built.Value.As().RunSafe();
        Fin<int> secondRun = built.Value.As().RunSafe();
        return Check(
            nameof(SharedStateProbe),
            ("Increment", first == 1),
            ("Capped", capped == 1 && counter.Value == 1),
            ("Register", SharedState.Read(registry, "a") == Some(2)),
            ("BumpOrStart", SharedState.Read(registry, "b") == Some(1)),
            ("Kept", kept == 2),
            ("Added", added == 50 && SharedState.Read(registry, "c") == Some(50)),
            ("Moved", moved == 30m && checking.Value == 70m && savings.Value == 30m),
            ("Tracked", tracked.Find("a") == Some(2) && tracked.Find("b").IsNone),
            ("Logged", SharedState.Logged(tracked) == 2),
            ("Cleared", SharedState.Logged(cleared) == 0 && cleared.Find("a") == Some(2)),
            ("Squares", nine == 18 && squareCalls.Value == 1),
            ("Once", twice == 2 && onceCalls.Value == 1),
            ("Built", builds.Value == 1 && runs.Value == 2 && firstRun == Pure(1) && secondRun == Pure(2)));
    }
}
