namespace Lab.T07;

internal static class Recipients {
    public static int Notify(IReadOnlySet<string> names) => names.Count;
    public static int Notify(string name) => Notify(SingleItem.Set(name));
    public static IReadOnlyDictionary<string, int> Quota(string user, int limit) => SingleItem.Dictionary(user, limit, StringComparer.OrdinalIgnoreCase);
    public static ILookup<int, string> Aliases(int id, ImmutableArray<string> names) => SingleItem.Lookup(id, names);
}
