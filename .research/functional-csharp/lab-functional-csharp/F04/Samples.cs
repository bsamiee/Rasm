namespace Lab.F04;

internal static class Samples {
    public static Fin<Unit> Run() =>
        TupleSample()
            .Bind(static _ => PatternSample())
            .Bind(static _ => AlternativesSample())
            .Bind(static _ => ImmutabilitySample())
            .Bind(static _ => NullableSample());

    private static Fin<Unit> TupleSample() {
        Seq<string> rendered = FilmReport.Render(Seq(1, 2));
        return Check(
            nameof(TupleSample),
            ("rendered.Count == 2", rendered.Count == 2),
            ("first title", rendered.Head.Exists(static line => line.StartsWith("Title: Blade Runner", StringComparison.Ordinal))),
            ("first cast", rendered.Head.Exists(static line => line.EndsWith("Cast: Harrison Ford, Rutger Hauer", StringComparison.Ordinal))),
            ("last director", rendered.Last.Exists(static line => line.Contains("Director: Ridley Scott", StringComparison.Ordinal))));
    }

    private static Fin<Unit> PatternSample() {
        Player simon = new("Simon", "Painter");
        Player alice = new("Alice", "Liddell");
        return Check(
            nameof(PatternSample),
            ("premium high", Interest.CalculateInterest(new PremiumBankAccount(25_000m, 0.02m, 0.01m)) == 812.5m),
            ("premium mid", Interest.CalculateInterest(new PremiumBankAccount(15_000m, 0.02m, 0.01m)) == 450m),
            ("premium low", Interest.CalculateInterest(new PremiumBankAccount(5_000m, 0.02m, 0.01m)) == 100m),
            ("millionaire", Interest.CalculateInterest(new MillionairesBankAccount(1_000_000m, 0.01m, 500_000m)) == 15_000m),
            ("monopoly free", Interest.CalculateInterest(new MonopolyPlayersBankAccount(1_500m, 0.1m, simon, "Go", 200m)) == 350m),
            ("monopoly jailed", Interest.CalculateInterest(new MonopolyPlayersBankAccount(1_500m, 0.1m, simon, "InJail", 200m)) == 150m),
            ("closed", Interest.CalculateInterest(new ClosedBankAccount(1_000m, 0.05m)) == 0m),
            ("is simon", Recognition.IsSimon(new MonopolyPlayersBankAccount(1m, 0.1m, simon, "Go", 200m))),
            ("is not simon", !Recognition.IsSimon(new MonopolyPlayersBankAccount(1m, 0.1m, alice, "Go", 200m))),
            ("closed is not simon", !Recognition.IsSimon(new ClosedBankAccount(1m, 0.1m))));
    }

    private static Fin<Unit> AlternativesSample() {
        Option<int> none = None;
        Fin<int> failed = new NotFound();
        Either<string, int> left = Left("left");
        Validation<Error, int> valid = 4;
        Validation<Error, int> invalid = new NotFound();
        return Check(
            nameof(AlternativesSample),
            ("option some", string.Equals(Alternatives.Describe(Some(1)), "some 1", StringComparison.Ordinal)),
            ("option none", string.Equals(Alternatives.Describe(none), "none", StringComparison.Ordinal)),
            ("fin succ", string.Equals(Alternatives.Describe(Pure(2).ToFin()), "succ 2", StringComparison.Ordinal)),
            ("fin fail", string.Equals(Alternatives.Describe(failed), "not found", StringComparison.Ordinal)),
            ("either right", string.Equals(Alternatives.Describe(Right<string, int>(3)), "right 3", StringComparison.Ordinal)),
            ("either left", string.Equals(Alternatives.Describe(left), "left", StringComparison.Ordinal)),
            ("validation succ", string.Equals(Alternatives.Describe(valid), "valid 4", StringComparison.Ordinal)),
            ("validation fail", string.Equals(Alternatives.Describe(invalid), "not found", StringComparison.Ordinal)));
    }

    private static Fin<Unit> ImmutabilitySample() {
        Seq<string> cast = Seq("Harrison Ford", "Rutger Hauer");
        MovieFields fields = new("Blade Runner", "Ridley Scott", cast);
        MovieInit init = new() { Title = "Blade Runner", Director = "Ridley Scott", Cast = cast };
        Movie bladeRunner = new() { Title = "Blade Runner", Director = "Ridley Scott", Cast = cast };
        Movie directorsCut = Editions.DirectorsCut(bladeRunner);
        return Check(
            nameof(ImmutabilitySample),
            ("fields title", string.Equals(fields.Title, "Blade Runner", StringComparison.Ordinal)),
            ("fields cast", fields.Cast == cast),
            ("init director", string.Equals(init.Director, "Ridley Scott", StringComparison.Ordinal)),
            ("init cast", init.Cast == cast),
            ("cut title", string.Equals(directorsCut.Title, "Blade Runner - The Director's Cut", StringComparison.Ordinal)),
            ("original title", string.Equals(bladeRunner.Title, "Blade Runner", StringComparison.Ordinal)),
            ("cut director", string.Equals(directorsCut.Director, bladeRunner.Director, StringComparison.Ordinal)),
            ("cut cast", directorsCut.Cast == bladeRunner.Cast),
            ("cut differs", directorsCut != bladeRunner));
    }

    private static Fin<Unit> NullableSample() {
        const string? missing = null;
        Option<string> absent = Optional(missing);
        Option<string> present = Optional("Simon");
        return Check(
            nameof(NullableSample),
            ("absent", absent.IsNone),
            ("present", present == Some("Simon")));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
