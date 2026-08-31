namespace Lab.T07;

internal static class Samples {
    public static Fin<Unit> Run() =>
        EmptySample()
            .Bind(static _ => SingleItemSample())
            .Bind(static _ => ToReadOnlyCollectionSample())
            .Bind(static _ => TrimOrNullifySample())
            .Bind(static _ => ComparersSample());

    private static Fin<Unit> EmptySample() {
        Action<string, int> ignore = EmptyShapes.Ignore();
        ignore("x", 1);
        Thinktecture.Empty.Disposable().Dispose();
        IReadOnlySet<string> names = Thinktecture.Empty.Set<string>();
        IReadOnlyDictionary<string, int> entries = Thinktecture.Empty.Dictionary<string, int>();
        ILookup<string, int> groups = Thinktecture.Empty.Lookup<string, int>();
        return Check(
            nameof(EmptySample),
            ("Untyped is object array", EmptyShapes.Untyped() is object[]),
            ("NoNumbers is int array", EmptyShapes.NoNumbers() is int[]),
            ("NoNumbers is IList at runtime", EmptyShapes.NoNumbers() is IList<int>),
            ("SameInstance", ReferenceEquals(Thinktecture.Empty.Collection<int>(), Thinktecture.Empty.Collection<int>())),
            ("Dictionary ContainsKey", !entries.ContainsKey("x")),
            ("Dictionary TryGetValue", !entries.TryGetValue("x", out int missing) && missing == 0),
            ("Lookup Contains", !groups.Contains("x")),
            ("Lookup indexer empty", !groups["x"].Any()),
            ("IsSubsetOf", names.IsSubsetOf(["a"])),
            ("IsProperSubsetOf empty", !names.IsProperSubsetOf([])),
            ("IsProperSubsetOf one", names.IsProperSubsetOf(["a"])),
            ("IsSupersetOf empty", names.IsSupersetOf([])),
            ("IsSupersetOf one", !names.IsSupersetOf(["a"])),
            ("IsProperSupersetOf", !names.IsProperSupersetOf([])),
            ("Overlaps", !names.Overlaps(["a"])),
            ("SetEquals empty", names.SetEquals([])),
            ("SetEquals one", !names.SetEquals(["a"])));
    }

    private static Fin<Unit> SingleItemSample() {
        List<string> aliases = ["x"];
        ILookup<int, string> lookup = SingleItem.Lookup(1, aliases);
        ILookup<int, string> snapshot = Recipients.Aliases(1, [.. aliases]);
        int before = lookup[1].Count();
        aliases.Add("y");
        IReadOnlySet<string> single = SingleItem.Set("a");
        IReadOnlyDictionary<string, int> quota = Recipients.Quota("k", 3);
        return Check(
            nameof(SingleItemSample),
            ("Notify delegates", Recipients.Notify("a") == 1),
            ("Quota ignores case", quota.ContainsKey("K")),
            ("Quota value", quota["K"] == 3),
            ("Quota TryGetValue miss", !quota.TryGetValue("other", out int absent) && absent == 0),
            ("Lookup before", before == 1),
            ("Lookup sees live list", lookup[1].SequenceEqual(["x", "y"], StringComparer.Ordinal)),
            ("Snapshot lookup fixed", snapshot[1].SequenceEqual(["x"], StringComparer.Ordinal)),
            ("Lookup other key", !lookup[2].Any()),
            ("Lookup Count", lookup.Count == 1),
            ("Lookup grouping", lookup.Single().Key == 1),
            ("Set Contains", single.Contains("a") && !single.Contains("A")),
            ("IsSubsetOf", !single.IsSubsetOf([]) && single.IsSubsetOf(["a", "b"])),
            ("IsProperSubsetOf", !single.IsProperSubsetOf(["a", "a"]) && single.IsProperSubsetOf(["a", "b"])),
            ("IsSupersetOf", single.IsSupersetOf([]) && single.IsSupersetOf(["a", "a"]) && !single.IsSupersetOf(["a", "b"])),
            ("IsProperSupersetOf", single.IsProperSupersetOf([]) && !single.IsProperSupersetOf(["a"])),
            ("Overlaps", single.Overlaps(["b", "a"]) && !single.Overlaps(["b"])),
            ("SetEquals", !single.SetEquals([]) && single.SetEquals(["a", "a"]) && !single.SetEquals(["a", "b"])));
    }

    private static Fin<Unit> ToReadOnlyCollectionSample() {
        List<User> users = [.. Projections.Sample()];
        IReadOnlyCollection<string> names = Projections.Names(users);
        int calls = 0;
        IReadOnlyCollection<int> counted = users.ToReadOnlyCollection(user => { calls++; return user.Name.Length; });
        _ = counted.ToList();
        _ = counted.ToList();
        users.Add(new User("linus"));
        IReadOnlyCollection<int> wrong = Enumerable.Range(1, 3).Select(static x => x).ToReadOnlyCollection(10);
        return Check(
            nameof(ToReadOnlyCollectionSample),
            ("Names", names.SequenceEqual(["ada", "grace", "linus"], StringComparer.Ordinal)),
            ("Names Count fixed at call", names.Count == 2),
            ("UpperNames", Projections.UpperNames(Projections.Sample()).SequenceEqual(["ADA", "GRACE"], StringComparer.Ordinal)),
            ("Selector re-runs", calls == 4),
            ("Wrong count reported", wrong.Count == 10),
            ("Wrong count enumeration", wrong.ToArray().Length == 3 && wrong.ToList().Count == 3));
    }

    private static Fin<Unit> TrimOrNullifySample() =>
        Check(
            nameof(TrimOrNullifySample),
            ("Trim", string.Equals("  abc  ".TrimOrNullify(), "abc", StringComparison.Ordinal)),
            ("Whitespace", "   ".TrimOrNullify() is null),
            ("Empty", "".TrimOrNullify() is null),
            ("Null", ((string?)null).TrimOrNullify() is null),
            ("Shortened", string.Equals(Trimming.Shortened("  Widgetry XL "), "Widgetry", StringComparison.Ordinal)),
            ("Shortened whitespace", Trimming.Shortened("   ") is null),
            ("Cut after trim", string.Equals("ab cd".TrimOrNullify(3), "ab ", StringComparison.Ordinal)),
            ("Surrogate split", "😀x".TrimOrNullify(1) is { Length: 1 } lone && char.IsHighSurrogate(lone[0])),
            ("Stored", string.Equals(Trimming.Stored("  Widget  "), "Widget", StringComparison.Ordinal)),
            ("Stored cut", Trimming.Stored(new string(c: 'a', count: 51)).Length == 50),
            ("TryCreate whitespace", !ProductName.TryCreate("   ", out _)),
            ("Validate whitespace", ProductName.Validate("   ", provider: null, out _) is not null),
            ("IConvertible", ProductName.Create("x") is IConvertible<string>));

    private static Fin<Unit> ComparersSample() =>
        Check(
            nameof(ComparersSample),
            ("Default equality ignores case", ProductName.Create("Widget") == ProductName.Create("WIDGET")),
            ("ExactNames exact", Comparers.ExactNames(ProductName.Create("Widget")).Contains(ProductName.Create("Widget"))),
            ("ExactNames case", !Comparers.ExactNames(ProductName.Create("Widget")).Contains(ProductName.Create("WIDGET"))),
            ("ByName", Comparers.ByName(new User("ada")).Contains(new User("ADA"))),
            ("ByName other", !Comparers.ByName(new User("ada")).Contains(new User("bob"))));

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
