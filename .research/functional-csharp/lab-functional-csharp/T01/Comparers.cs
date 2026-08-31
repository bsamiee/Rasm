namespace Lab.T01;

internal sealed class TrimmedOrdinalComparer : IEqualityComparer<string>, IComparer<string>, IAlternateEqualityComparer<ReadOnlySpan<char>, string>, IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly TrimmedOrdinalComparer Instance = new();
    public static IEqualityComparer<string> EqualityComparer => Instance;
    public static IComparer<string> Comparer => Instance;
    public bool Equals(string? x, string? y) => string.Equals(x?.Trim(), y?.Trim(), StringComparison.Ordinal);
    public int GetHashCode(string obj) => string.GetHashCode(obj.AsSpan().Trim(), StringComparison.Ordinal);
    public int Compare(string? x, string? y) => string.CompareOrdinal(x?.Trim(), y?.Trim());
    public string Create(ReadOnlySpan<char> alternate) => alternate.Trim().ToString();
    public bool Equals(ReadOnlySpan<char> alternate, string other) => alternate.Trim().SequenceEqual(other.AsSpan().Trim());
    public int GetHashCode(ReadOnlySpan<char> alternate) => string.GetHashCode(alternate.Trim(), StringComparison.Ordinal);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TrimmedOrdinalComparer, string>]
[KeyMemberComparer<TrimmedOrdinalComparer, string>]
internal sealed partial class Ticker {
    public static readonly Ticker Msft = new("MSFT");
    public static readonly Ticker Aapl = new("AAPL");
}
