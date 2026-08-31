namespace Lab.F05;

internal sealed record Film(string Title, string Genre, decimal Budget, decimal BoxOfficeRevenue);

internal sealed record Priced(string Title, decimal Price);

internal sealed record SourceData(
    int Something,
    int SomethingElse,
    int Ping,
    int Pong,
    bool Alternate,
    string FirstChoice,
    string SecondChoice,
    string ThirdChoice,
    string FourthChoice);

internal sealed record ComplexObject {
    public required int PropertyA { get; init; }

    public required int PropertyB { get; init; }

    public required string PropertyC { get; init; }

    public required string PropertyD { get; init; }
}
