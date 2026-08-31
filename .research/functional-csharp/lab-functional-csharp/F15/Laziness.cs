namespace Lab.F15;

internal sealed record Cache(HashMap<int, string> Entries) {
    public Option<string> Find(int id) => Entries.Find(id);
}

internal sealed class Database(Atom<int> reads) {
    public Option<string> Find(int id) => Some(Load(id));

    public string Load(int id) => string.Create(CultureInfo.InvariantCulture, $"customer {id} after {reads.Swap(static n => n + 1)} reads");
}

internal static partial class Laziness {
    public static A Pick<A>(bool takeLeft, A left, A right) => takeLeft ? left : right;
}

internal static partial class Laziness {
    public static IO<A> Pick<A>(bool takeLeft, IO<A> left, IO<A> right) => takeLeft ? left : right;
}

internal static partial class Laziness {
    public static int Twice(Func<int> compute) {
        Memo<int> total = memo(compute);
        return total.Value + total.Value;
    }
}

internal static partial class Laziness {
    public static Option<string> Eager(Cache cache, Database database, int id) => cache.Find(id) | database.Find(id);
}

internal static partial class Laziness {
    public static Option<string> Deferred(Cache cache, Database database, int id) => cache.Find(id) || database.Find(id);
}

internal static partial class Laziness {
    public static string Named(Cache cache, int id) => cache.Find(id).IfNone("unknown");

    public static string Loaded(Cache cache, Database database, int id) => cache.Find(id).IfNone(() => database.Load(id));
}
