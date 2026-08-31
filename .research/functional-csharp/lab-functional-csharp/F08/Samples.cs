namespace Lab.F08;

internal static class Samples {
    public static Fin<Unit> Run() =>
        MapSamples()
            .Bind(static _ => IterSamples())
            .Bind(static _ => BindSamples())
            .Bind(static _ => FilterAndCombineSamples())
            .Bind(static _ => ElevatedSamples());

    private static Fin<Unit> MapSamples() {
        Option<Risk> low = CorePatterns.RiskOf(new Subject("Ada", Age.From(36).ToOption()));
        Option<Risk> unknown = CorePatterns.RiskOf(new Subject("Bob", Option<Age>.None));
        return Check(
            nameof(MapSamples),
            ("Doubled(Seq)", CorePatterns.Doubled(Seq(1, 2, 3)) == Seq(2, 4, 6)),
            ("Doubled(Option)", CorePatterns.Doubled(Some(2)) == Some(4)),
            ("low == Some(Risk(low))", low == Some(new Risk("low"))),
            ("unknown.IsNone", unknown.IsNone),
            ("OptionLengths == Some(2)", CorePatterns.OptionLengths() == Some(2)),
            ("SeqLengths == Seq(1, 2)", CorePatterns.SeqLengths() == Seq(1, 2)));
    }

    private static Fin<Unit> IterSamples() {
        Unit greeted = CorePatterns.Greet(Some("Ada"));
        Unit silent = CorePatterns.Greet(Option<string>.None);
        Option<int> option = CorePatterns.Observed(Some(1));
        Seq<int> seq = CorePatterns.Observed(Seq(2, 3));
        Unit succ = CorePatterns.Observed(Pure(4).ToFin());
        Fin<int> traced = CorePatterns.Traced(IO.pure(5)).RunSafe();
        return Check(
            nameof(IterSamples),
            ("greeted == unit", greeted == unit),
            ("silent == unit", silent == unit),
            ("option == Some(1)", option == Some(1)),
            ("seq == Seq(2, 3)", seq == Seq(2, 3)),
            ("succ == unit", succ == unit),
            ("traced == Pure(5)", traced == Pure(5)));
    }

    private static Fin<Unit> BindSamples() {
        Seq<Neighbor> neighbors = Seq(
            new Neighbor("Ada", Seq(new Pet("Rex"), new Pet("Tom"))),
            new Neighbor("Bob", Seq<Pet>()),
            new Neighbor("Cy", Seq(new Pet("Ann"))));
        Seq<Pet> pets = CorePatterns.PetsOf(neighbors);
        Validation<Error, Unit> laws = MonadLaw<Option>.validate();
        return Check(
            nameof(BindSamples),
            ("ParseAge(30) == Some(Age(30))", CorePatterns.ParseAge("30") == Some(Age.Create(30))),
            ("ParseAge(x).IsNone", CorePatterns.ParseAge("x").IsNone),
            ("ParseAge(200).IsNone", CorePatterns.ParseAge("200").IsNone),
            ("pets.Count == 3", pets.Count == 3),
            ("pets == Seq(Rex, Tom, Ann)", pets == Seq(new Pet("Rex"), new Pet("Tom"), new Pet("Ann"))),
            ("PureOption(1) == Some(1)", CorePatterns.PureOption(1) == Some(1)),
            ("PureFin(1) == Pure(1)", CorePatterns.PureFin(1) == Pure(1)),
            ("PureIO(1).RunSafe() == Pure(1)", CorePatterns.PureIO(1).RunSafe() == Pure(1)),
            ("PureSeq(1) == Seq(1)", CorePatterns.PureSeq(1) == Seq(1)),
            ("laws.IsSuccess", laws.IsSuccess));
    }

    private static Fin<Unit> FilterAndCombineSamples() {
        Seq<Subject> population = Seq(
            new Subject("Ada", Age.From(36).ToOption()),
            new Subject("Bob", Option<Age>.None),
            new Subject("Cy", Age.From(20).ToOption()));
        Seq<Age> stated = CorePatterns.StatedAges(population);
        int total = CorePatterns.TotalAge(population);
        Seq<Age> promoted = CorePatterns.AsSequence(Age.From(5).ToOption());
        Seq<Age> disclosed = CorePatterns.Disclosed(Seq(Age.From(1).ToOption(), Option<Age>.None, Age.From(2).ToOption()));
        Seq<Age> flattened = CorePatterns.Flattened(Some(Seq(Age.Create(3))));
        return Check(
            nameof(FilterAndCombineSamples),
            ("ToNatural(5) == Some(5)", CorePatterns.ToNatural("5") == Some(5)),
            ("ToNatural(-5).IsNone", CorePatterns.ToNatural("-5").IsNone),
            ("ToNatural(x).IsNone", CorePatterns.ToNatural("x").IsNone),
            ("stated == Seq(36, 20)", stated == Seq(Age.Create(36), Age.Create(20))),
            ("total == 56", total == 56),
            ("promoted == Seq(Age(5))", promoted == Seq(Age.Create(5))),
            ("disclosed == Seq(1, 2)", disclosed == Seq(Age.Create(1), Age.Create(2))),
            ("flattened == Seq(3)", flattened == Seq(Age.Create(3))));
    }

    private static Fin<Unit> ElevatedSamples() {
        Seq<string> percentages = CorePatterns.Percentages();
        Fin<Option<Age>> looked = CorePatterns.LookupAge("30").Run().As().RunSafe();
        return Check(
            nameof(ElevatedSamples),
            ("percentages", percentages == Seq("100%", "80%", "60%", "40%", "20%")),
            ("looked == Pure(Some(Age(30)))", looked == Pure(Some(Age.Create(30)))));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
