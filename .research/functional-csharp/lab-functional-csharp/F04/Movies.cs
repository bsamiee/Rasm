namespace Lab.F04;

internal readonly struct MovieFields {
    public readonly string Title;
    public readonly string Director;
    public readonly Seq<string> Cast;

    public MovieFields(
        string title,
        string director,
        Seq<string> cast) =>
        (Title, Director, Cast) = (title, director, cast);
}

internal readonly struct MovieInit {
    public string Title { get; init; }
    public string Director { get; init; }
    public Seq<string> Cast { get; init; }
}

internal sealed record Movie {
    public required string Title { get; init; }
    public required string Director { get; init; }
    public Seq<string> Cast { get; init; }
}

internal static class Editions {
    public static Movie DirectorsCut(Movie bladeRunner) =>
        bladeRunner with {
            Title = $"{bladeRunner.Title} - The Director's Cut",
        };
}
