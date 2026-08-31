namespace Lab.F05;

internal static partial class Sequences {
    public static Seq<Film> FilmsByGenre(Seq<Film> films, string genre) =>
        films.Filter(film => string.Equals(film.Genre, genre, StringComparison.Ordinal));
}

internal static partial class Sequences {
    public static ComplexObject MakeObject(SourceData source) =>
        new() {
            PropertyA = source.Something + source.SomethingElse,
            PropertyB = source.Ping * source.Pong,
            PropertyC = source.Alternate
                ? source.FirstChoice
                : source.SecondChoice,
            PropertyD = source.Alternate
                ? source.ThirdChoice
                : source.FourthChoice,
        };
}

internal static partial class Sequences {
    public static Seq<int> Transformed(IEnumerable<int> input) {
        Iterable<int> transformed = input.AsIterable()
            .Map(First)
            .Map(Second)
            .Map(Third);

        return transformed.ToSeq();
    }
}

internal static partial class Sequences {
    public static Seq<string> Rendered(Seq<Film> films) {
        Seq<Film> normalized = films.Map(Normalize);
        Seq<Priced> priced = normalized.Map(CalculatePrice);
        return priced.Map(Render);
    }
}

internal static partial class Sequences {
    public static Seq<string> Descriptions(
        Seq<int> filmIds,
        Func<int, Film> getFilm,
        Func<int, Seq<string>> getCastList) {
        Seq<(Film Film, Seq<string> Cast)> filmsWithCast = filmIds.Map(id => (
            Film: getFilm(id),
            Cast: getCastList(id)));

        return filmsWithCast.Map(static item =>
            $"{item.Film.Title}: {string.Join(", ", item.Cast)}");
    }
}

internal static partial class Sequences {
    public static string Numbered(Seq<Film> orderedFilms) {
        Seq<string> lines = orderedFilms.Map(static (film, index) =>
            string.Create(CultureInfo.InvariantCulture, $"{index} - {film.Title}"));

        return string.Join(Environment.NewLine, lines);
    }
}

internal static partial class Sequences {
    public static int Total(Seq<int> values) =>
        values.Fold(0, static (sum, value) => sum + value);

    public static decimal Revenue(Seq<Film> films) =>
        films.Fold(0m, static (sum, film) => sum + film.BoxOfficeRevenue);
}

internal static partial class Sequences {
    public static Option<decimal> Median(Seq<int> numbers) {
        Seq<int> sorted = toSeq(numbers.Order());
        int middle = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted.At(middle).Map(static value => (decimal)value)
            : sorted.At(middle).Bind(right =>
                sorted.At(middle - 1).Map(left => (left + right) / 2m));
    }
}

internal static partial class Sequences {
    public static (decimal Budget, decimal Revenue) Totals(Seq<Film> films) =>
        films.Fold(
            (Budget: 0.0m, Revenue: 0.0m),
            static (state, film) => (
                state.Budget + film.Budget,
                state.Revenue + film.BoxOfficeRevenue));
}

internal static partial class Sequences {
    public static Seq<A> ReplaceAt<A>(Seq<A> source, int index, Func<A, A> replace) =>
        source.Map((item, currentIndex) =>
            currentIndex == index ? replace(item) : item);
}

internal static partial class Sequences {
    public static bool AnyAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).Exists(pair => matches(pair.First, pair.Second));

    public static bool AllAdjacent<A>(Seq<A> source, Func<A, A, bool> matches) =>
        source.Zip(source.Tail).ForAll(pair => matches(pair.First, pair.Second));
}

internal sealed record Story(
    int SeasonNumber,
    string StoryName,
    string Writer,
    string Director,
    int NumberOfEpisodes,
    int NumberOfMissingEpisodes);

internal static partial class Sequences {
    public static Option<Story> ParseStory(string line) {
        Seq<string> fields = toSeq(line.Split(','));
        return from season in fields.At(0).Bind(static field => parseInt(field))
               from name in fields.At(1)
               from writer in fields.At(2)
               from director in fields.At(3)
               from episodes in fields.At(4).Bind(static field => parseInt(field))
               from missing in fields.At(5).Bind(static field => parseInt(field))
               select new Story(season, name, writer, director, episodes, missing);
    }

    public static Option<Seq<Story>> Stories(string csv) =>
        toSeq(csv.Split(Environment.NewLine)).Traverse(static line => ParseStory(line)).As();

    public static Map<int, (int Episodes, int Missing)> SeasonTotals(Seq<Story> stories) =>
        stories.Fold(
            Map<int, (int Episodes, int Missing)>(),
            static (state, story) => state.AddOrUpdate(
                story.SeasonNumber,
                total => (
                    total.Episodes + story.NumberOfEpisodes,
                    total.Missing + story.NumberOfMissingEpisodes),
                (story.NumberOfEpisodes, story.NumberOfMissingEpisodes)));

    public static decimal MissingPercentage(int missing, int episodes) =>
        episodes == 0 ? 0m : (decimal)missing / episodes * 100m;

    public static Seq<string> ReportLines(Map<int, (int Episodes, int Missing)> totals) =>
        totals.ToSeq().Map(static total => string.Create(
            CultureInfo.InvariantCulture,
            $"{total.Key},{total.Value.Episodes},{total.Value.Missing},{MissingPercentage(total.Value.Missing, total.Value.Episodes)}"));

    public static Option<string> Report(string csv) =>
        Stories(csv).Map(static stories => {
            string reportBody = string.Join(Environment.NewLine, ReportLines(SeasonTotals(stories)));
            const string reportHeader = "Season,No Episodes,No Missing Eps,Percentage Missing";
            return $"{reportHeader}{Environment.NewLine}{reportBody}";
        });
}

internal static partial class Sequences {
    public static int First(int value) => value + 1;

    public static int Second(int value) => value * 2;

    public static int Third(int value) => value - 3;

    public static Film Normalize(Film film) => film with { Title = film.Title.Trim() };

    public static Priced CalculatePrice(Film film) => new(film.Title, film.Budget / 10m);

    public static string Render(Priced priced) =>
        string.Create(CultureInfo.InvariantCulture, $"{priced.Title}: {priced.Price}");
}
