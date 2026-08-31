namespace Lab.F05;

internal static class Samples {
    private static readonly Seq<Film> Films = Seq(
        new Film(" Alien ", "horror", 100m, 300m),
        new Film("Heat", "crime", 60m, 180m),
        new Film("Scream", "horror", 20m, 120m));

    public static Fin<Unit> Run() =>
        DescribeSamples()
            .Bind(static _ => SequenceSamples())
            .Bind(static _ => ReductionSamples())
            .Bind(static _ => UpdateAndAdjacentSamples())
            .Bind(static _ => ReportSamples());

    private static Fin<Unit> DescribeSamples() {
        Seq<Film> horror = Sequences.FilmsByGenre(Films, "horror");
        ComplexObject made = Sequences.MakeObject(new SourceData(1, 2, 3, 4, Alternate: true, "a", "b", "c", "d"));
        Seq<int> transformed = Sequences.Transformed([1, 2, 3]);
        return Check(
            nameof(DescribeSamples),
            ("horror.Count == 2", horror.Count == 2),
            ("made.PropertyA == 3", made.PropertyA == 3),
            ("made.PropertyB == 12", made.PropertyB == 12),
            ("made.PropertyC == a", string.Equals(made.PropertyC, "a", StringComparison.Ordinal)),
            ("made.PropertyD == c", string.Equals(made.PropertyD, "c", StringComparison.Ordinal)),
            ("transformed == Seq(1, 3, 5)", transformed == Seq(1, 3, 5)));
    }

    private static Fin<Unit> SequenceSamples() {
        Seq<string> rendered = Sequences.Rendered(Films);
        Map<int, Film> catalog = Map((1, Films[0]), (2, Films[1]));
        Map<int, Seq<string>> casts = Map((1, Seq("Weaver")), (2, Seq("Pacino", "De Niro")));
        Seq<string> descriptions = Sequences.Descriptions(
            Seq(1, 2),
            id => catalog[id],
            id => casts[id]);
        string numbered = Sequences.Numbered(Films);
        return Check(
            nameof(SequenceSamples),
            ("rendered", rendered == Seq("Alien: 10", "Heat: 6", "Scream: 2")),
            ("descriptions", descriptions == Seq(" Alien : Weaver", "Heat: Pacino, De Niro")),
            ("numbered", string.Equals(numbered, string.Join(Environment.NewLine, Seq("0 -  Alien ", "1 - Heat", "2 - Scream")), StringComparison.Ordinal)));
    }

    private static Fin<Unit> ReductionSamples() {
        (decimal Budget, decimal Revenue) totals = Sequences.Totals(Films);
        Seq<int> reversed = Seq(1, 2, 3).FoldBack(Seq<int>(), static (acc, value) => acc.Add(value));
        Seq<int> joined = Seq(Seq(1), Seq(2, 3)).Fold();
        return Check(
            nameof(ReductionSamples),
            ("Total == 6", Sequences.Total(Seq(1, 2, 3)) == 6),
            ("Revenue == 600", Sequences.Revenue(Films) == 600m),
            ("Median odd == Some(2)", Sequences.Median(Seq(3, 1, 2)) == Some(2m)),
            ("Median even == Some(2.5)", Sequences.Median(Seq(4, 1, 3, 2)) == Some(2.5m)),
            ("Median empty is None", Sequences.Median(Seq<int>()).IsNone),
            ("totals == (180, 600)", totals == (180m, 600m)),
            ("reversed == Seq(3, 2, 1)", reversed == Seq(3, 2, 1)),
            ("joined == Seq(2, 3, 1)", joined == Seq(2, 3, 1)));
    }

    private static Fin<Unit> UpdateAndAdjacentSamples() {
        Seq<int> replaced = Sequences.ReplaceAt(Seq(1, 2, 3), 1, static value => value * 10);
        Seq<int> sorted = toSeq(Seq(5, 9, 4).Order());
        Seq<int> states = toSeq(LanguageExt.List.unfold(1, static state =>
            state <= 16 ? Some((state, state * 2)) : Option<(int, int)>.None));
        Seq<int> flattened = Seq(Seq(1), Seq(2, 3)).Bind(static inner => inner);
        return Check(
            nameof(UpdateAndAdjacentSamples),
            ("replaced == Seq(1, 20, 3)", replaced == Seq(1, 20, 3)),
            ("AnyAdjacent differs by one", Sequences.AnyAdjacent(sorted, static (left, right) => right - left == 1)),
            ("AnyAdjacent empty is false", !Sequences.AnyAdjacent(Seq<int>(), static (left, right) => left == right)),
            ("AllAdjacent ascending", Sequences.AllAdjacent(sorted, static (left, right) => left < right)),
            ("AllAdjacent single is true", Sequences.AllAdjacent(Seq(1), static (left, right) => left == right)),
            ("states == Seq(1, 2, 4, 8, 16)", states == Seq(1, 2, 4, 8, 16)),
            ("flattened == Seq(1, 2, 3)", flattened == Seq(1, 2, 3)));
    }

    private static Fin<Unit> ReportSamples() {
        string csv = string.Join(
            Environment.NewLine,
            Seq("1,Story A,Writer,Director,4,1", "2,Story B,Writer,Director,6,0", "1,Story C,Writer,Director,4,3"));
        Option<string> report = Sequences.Report(csv);
        string expected = string.Join(
            Environment.NewLine,
            Seq("Season,No Episodes,No Missing Eps,Percentage Missing", "1,8,4,50.0", "2,6,0,0"));
        return Check(
            nameof(ReportSamples),
            ("report == expected", report == Some(expected)),
            ("bad row is None", Sequences.Report("1,Story A,Writer,Director,x,1").IsNone),
            ("short row is None", Sequences.Report("1,Story A").IsNone));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
