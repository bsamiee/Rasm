namespace Lab.F04;

internal sealed record Film(string Title, string Director);

internal static class Catalogue {
    private static readonly HashMap<int, Film> Films = HashMap(
        (1, new Film("Blade Runner", "Ridley Scott")),
        (2, new Film("Alien", "Ridley Scott")));

    private static readonly HashMap<int, Seq<string>> Casts = HashMap(
        (1, Seq("Harrison Ford", "Rutger Hauer")),
        (2, Seq("Sigourney Weaver", "Tom Skerritt")));

    public static Film GetFilm(int id) => Films.Find(id).IfNone(new Film("Unknown", "Unknown"));

    public static Seq<string> GetCastList(int id) => Casts.Find(id).IfNone(Seq<string>());
}

internal static class FilmReport {
    public static Seq<string> Render(Seq<int> filmIds) =>
        filmIds
            .Map(static id => (
                Film: Catalogue.GetFilm(id),
                Cast: Catalogue.GetCastList(id)))
            .Map(static x => string.Join(
                Environment.NewLine,
                $"Title: {x.Film.Title}",
                $"Director: {x.Film.Director}",
                $"Cast: {string.Join(", ", x.Cast)}"));
}
