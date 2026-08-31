namespace Lab.F01;

internal static class Samples {
    public static Fin<Unit> Run() =>
        ModelSamples()
            .Bind(static _ => PropertySamples())
            .Bind(static _ => TraitSamples())
            .Bind(static _ => SignatureSamples());

    private static Fin<Unit> ModelSamples() {
        (Seq<int> source, Seq<int> tripled) = FunctionalModel.Tripled();
        List<int> sortedInPlace = FunctionalModel.SortedInPlace();
        (Seq<int> values, Seq<int> sorted, Seq<int> odd) = FunctionalModel.Transformed();
        return Check(
            nameof(ModelSamples),
            ("source == Seq(1, 2, 3)", source == Seq(1, 2, 3)),
            ("tripled == Seq(3, 6, 9)", tripled == Seq(3, 6, 9)),
            ("sortedInPlace == [1, 6, 7]", toSeq(sortedInPlace) == Seq(1, 6, 7)),
            ("values == Seq(7, 6, 1)", values == Seq(7, 6, 1)),
            ("sorted == Seq(1, 6, 7)", sorted == Seq(1, 6, 7)),
            ("odd == Seq(7, 1)", odd == Seq(7, 1)));
    }

    private static Fin<Unit> PropertySamples() {
        (Func<int, int, string> describeSum, Action<string> log) = CoreProperties.Delegates();
        log("ready");
        DateTimeOffset now = new(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        string stamped = CoreProperties.TimestampedGreeting(now, name: null);
        DoctorWho first = new(NumberOfStories: 10, CurrentDoctor: 1, CurrentDoctorActor: "Hartnell", SeasonNumber: 3);
        DoctorWho second = CoreProperties.RegenerateDoctor(first, "Troughton");
        return Check(
            nameof(PropertySamples),
            ("describeSum(1, 2)", string.Equals(describeSum(1, 2), "1 + 2 = 3", StringComparison.Ordinal)),
            ("Describe(10)", string.Equals(CoreProperties.Describe(10), "It was ten", StringComparison.Ordinal)),
            ("Describe(3)", string.Equals(CoreProperties.Describe(3), "It was not ten", StringComparison.Ordinal)),
            ("Add(2, 3) == 5", CoreProperties.Add(2, 3) == 5),
            ("Greeting(Ada)", string.Equals(CoreProperties.Greeting("Ada"), "Hello Ada", StringComparison.Ordinal)),
            ("Greeting(null)", string.Equals(CoreProperties.Greeting(name: null), "Hello Unknown Person", StringComparison.Ordinal)),
            ("stamped", string.Equals(stamped, "01/02/2024 03:04:05 +00:00 - Hello Unknown Person", StringComparison.Ordinal)),
            ("first unchanged", first.CurrentDoctor == 1 && string.Equals(first.CurrentDoctorActor, "Hartnell", StringComparison.Ordinal)),
            ("second.CurrentDoctor == 2", second.CurrentDoctor == 2),
            ("second actor", string.Equals(second.CurrentDoctorActor, "Troughton", StringComparison.Ordinal)),
            ("second keeps stories and season", second.NumberOfStories == 10 && second.SeasonNumber == 3));
    }

    private static Fin<Unit> TraitSamples() {
        Option<int> doubledOption = Traits.Doubled(Some(2)).As();
        Seq<int> doubledSeq = Traits.Doubled(Seq(1, 2)).As();
        Option<int> liftedOption = Traits.Lifted<Option>(1).As();
        Seq<int> liftedSeq = Traits.Lifted<Seq>(1).As();
        return Check(
            nameof(TraitSamples),
            ("TripledOption == Some(6)", Traits.TripledOption() == Some(6)),
            ("TripledSeq == Seq(3, 6, 9)", Traits.TripledSeq() == Seq(3, 6, 9)),
            ("doubledOption == Some(4)", doubledOption == Some(4)),
            ("doubledSeq == Seq(2, 4)", doubledSeq == Seq(2, 4)),
            ("liftedOption == Some(1)", liftedOption == Some(1)),
            ("liftedSeq == Seq(1)", liftedSeq == Seq(1)),
            ("Absent().IsNone", Traits.Absent().IsNone),
            ("Parsed(12) == Some(12)", Traits.Parsed("12") == Some(12)),
            ("Parsed(x).IsNone", Traits.Parsed("x").IsNone),
            ("Ages().Find(Ada) == Some(36)", Traits.Ages().Find("Ada") == Some(36)),
            ("HashedAges().Find(Ada) == Some(36)", Traits.HashedAges().Find("Ada") == Some(36)),
            ("Digits().Contains(2)", Traits.Digits().Contains(2)));
    }

    private static Fin<Unit> SignatureSamples() {
        (Seq<DayOfWeek> days, Seq<DayOfWeek> weekendStarts) = Signatures.Closure();
        return Check(
            nameof(SignatureSamples),
            ("days.Count == 7", days.Count == 7),
            ("weekendStarts == Seq(Sunday, Saturday)", weekendStarts == Seq(DayOfWeek.Sunday, DayOfWeek.Saturday)));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
