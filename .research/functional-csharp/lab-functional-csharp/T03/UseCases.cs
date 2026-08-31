namespace Lab.T03;

[System.Text.Json.Serialization.JsonDerivedType(typeof(YearOnly), "Year")]
[System.Text.Json.Serialization.JsonDerivedType(typeof(YearMonth), "YearMonth")]
[System.Text.Json.Serialization.JsonDerivedType(typeof(Exact), "Date")]
[Union]
internal abstract partial record PartiallyKnownDate {
    private PartiallyKnownDate(int year) => Year = year;

    public int Year { get; }
    internal sealed record YearOnly(int Year) : PartiallyKnownDate(Year);
    internal sealed record YearMonth(int Year, int Month) : PartiallyKnownDate(Year);
    internal sealed record Exact(int Year, int Month, int Day) : PartiallyKnownDate(Year);

    public static implicit operator PartiallyKnownDate(DateOnly date) => new Exact(date.Year, date.Month, date.Day);
}
