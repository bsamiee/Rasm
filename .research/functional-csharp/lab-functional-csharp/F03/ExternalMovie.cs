namespace Lab.F03;

internal sealed record ExternalMovie {
    public string? Title { get; init; }
    public string? Director { get; init; }
    public IEnumerable<string>? Cast { get; init; }
}
