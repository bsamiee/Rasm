namespace Lab.T07;

internal static class EmptyShapes {
    public static Action<string, int> Ignore() => Thinktecture.Empty.Action;
    public static System.Collections.IEnumerable Untyped() => Thinktecture.Empty.Collection();
    public static IReadOnlyList<int> NoNumbers() => Thinktecture.Empty.Collection<int>();
}
