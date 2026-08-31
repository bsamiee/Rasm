namespace Lab.F03;

internal static class Samples {
    public static Fin<Unit> Run() =>
        AgeSample()
            .Bind(static _ => UnitSample())
            .Bind(static _ => OptionSample())
            .Bind(static _ => TotalitySample())
            .Bind(static _ => NullableSample());

    private static Fin<Unit> AgeSample() {
        Fin<Age> young = Age.From(30);
        Fin<Age> invalid = Age.From(200);
        bool typed = invalid.Match(Succ: static _ => false, Fail: static e => e.IsType<InvalidAge>() && e.HasCode(1001));
        Age adult = Age.Create(70);
        return Check(
            nameof(AgeSample),
            ("young.IsSucc", young.IsSucc),
            ("typed", typed),
            ("adult == 70", adult == 70),
            ("Low", Underwriting.CalculateRiskProfile(Age.Create(30)) == Risk.Low),
            ("Medium", Underwriting.CalculateRiskProfile(adult) == Risk.Medium),
            ("30 < 70", Age.Create(30) < adult),
            ("70 > 60", adult > 60));
    }

    private static Fin<Unit> UnitSample() {
        Unit none = Timing.Time("noop", static () => { });
        int value = Timing.Time("value", static () => 5);
        return Check(
            nameof(UnitSample),
            ("none == unit", none == unit),
            ("value == 5", value == 5));
    }

    private static Fin<Unit> OptionSample() {
        const string? missing = null;
        Option<string?> someNull = Some(missing);
        return Check(
            nameof(OptionSample),
            ("Some", string.Equals(Greetings.GreetingFor(Some("ada")), "Dear ADA,", StringComparison.Ordinal)),
            ("None", string.Equals(Greetings.GreetingFor(None), "Dear Subscriber,", StringComparison.Ordinal)),
            ("Present", Construction.Present("x").IsSome),
            ("Absent", Construction.Absent().IsNone),
            ("AtTheBoundary(null)", Construction.AtTheBoundary(missing).IsNone),
            ("AtTheBoundary(x)", Construction.AtTheBoundary("x") == Some("x")),
            ("Lifted(null)", Construction.Lifted(missing).IsNone),
            ("Some(null).IsSome", someNull.IsSome));
    }

    private static Fin<Unit> TotalitySample() {
        HashMap<string, string> settings = HashMap(("age", "42"), ("bad", "x"), ("old", "200"));
        return Check(
            nameof(TotalitySample),
            ("parseInt 42", Totality.ParseAge("42") == Some(42)),
            ("parseInt x", Totality.ParseAge("x").IsNone),
            ("Find present", Totality.Setting(settings, "age") == Some("42")),
            ("Find absent", Totality.Setting(settings, "font").IsNone),
            ("AgeOf", Totality.AgeOf(settings) == Some(Age.Create(42))),
            ("AgeOf old", Totality.AgeOf(HashMap(("age", "200"))).IsNone),
            ("AgeOf missing", Totality.AgeOf(HashMap<string, string>()).IsNone));
    }

    private static Fin<Unit> NullableSample() {
        ExternalMovie movie = new() { Title = "Metropolis" };
        return Check(
            nameof(NullableSample),
            ("Title", Optional(movie.Title) == Some("Metropolis")),
            ("Director", Optional(movie.Director).IsNone),
            ("Cast", Optional(movie.Cast).IsNone));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
