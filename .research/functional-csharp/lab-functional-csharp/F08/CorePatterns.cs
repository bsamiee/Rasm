namespace Lab.F08;

internal static partial class CorePatterns {
    public static Seq<int> Doubled(Seq<int> values) =>
        values.Map(static value => value * 2);
}

internal static partial class CorePatterns {
    public static Option<int> Doubled(Option<int> value) =>
        value.Map(static v => v * 2);
}

internal static partial class CorePatterns {
    public static Option<Risk> RiskOf(Subject subject) =>
        subject.Age.Map(CalculateRiskProfile);
}

internal static partial class CorePatterns {
    public static K<F, int> Lengths<F>(K<F, string> values)
        where F : Functor<F> =>
        values.Map(static value => value.Length);

    public static Option<int> OptionLengths() =>
        Lengths(Some("ab")).As();

    public static Seq<int> SeqLengths() =>
        Lengths(Seq("a", "bb")).As();
}

internal static partial class CorePatterns {
    public static Unit Greet(Option<string> name) =>
        name
            .Map(static value => $"Hello {value}")
            .Iter(Console.WriteLine);
}

internal static partial class CorePatterns {
    public static IO<int> Traced(IO<int> effect) =>
        from value in effect
        from _ in IO.lift(() => Console.WriteLine(value))
        select value;
}

internal static partial class CorePatterns {
    public static Option<Age> ParseAge(string input) =>
        parseInt(input).Bind(static value => Age.From(value).ToOption());
}

internal static partial class CorePatterns {
    public static Seq<Pet> PetsOf(Seq<Neighbor> neighbors) =>
        neighbors.Bind(static neighbor => neighbor.Pets);
}

internal static partial class CorePatterns {
    public static Option<int> PureOption(int value) => Pure(value);

    public static Fin<int> PureFin(int value) => Pure(value);

    public static IO<int> PureIO(int value) => Pure(value);
}

internal static partial class CorePatterns {
    public static Option<int> ToNatural(string input) =>
        parseInt(input).Filter(static value => value >= 0);
}

internal static partial class CorePatterns {
    public static Seq<Age> AsSequence(Option<Age> age) => age.ToSeq();
}

internal static partial class CorePatterns {
    public static Seq<Age> StatedAges(Seq<Subject> population) =>
        population.Choose(static subject => subject.Age);

    public static Seq<Age> Disclosed(Seq<Option<Age>> ages) => ages.Somes();

    public static Seq<Age> Flattened(Option<Seq<Age>> ages) => ages.ToSeq().Flatten();
}

internal static partial class CorePatterns {
    public static int TotalAge(Seq<Subject> population) =>
        StatedAges(population).Fold(0, static (total, age) => total + age);
}

internal static partial class CorePatterns {
    public static Seq<string> Percentages() =>
        toSeq(Range(1, 100))
            .Filter(static value => value % 20 == 0)
            .Rev()
            .Map(static value => string.Create(CultureInfo.InvariantCulture, $"{value}%"));
    // ["100%", "80%", "60%", "40%", "20%"]
}

internal static partial class CorePatterns {
    public static Risk CalculateRiskProfile(Age age) =>
        new(age < 60 ? "low" : "high");

    public static Option<int> Observed(Option<int> value) =>
        value.Do(static v => Console.WriteLine(v));

    public static Seq<int> Observed(Seq<int> values) =>
        values.Do(static v => Console.WriteLine(v));

    public static Unit Observed(Fin<int> value) =>
        value.IfSucc(static v => Console.WriteLine(v));

    public static K<F, A> Lift<F, A>(A value)
        where F : Applicative<F> =>
        F.Pure(value);

    public static Seq<int> PureSeq(int value) => Lift<Seq, int>(value).As();

    public static OptionT<IO, Age> LookupAge(string input) =>
        OptionT.lift<IO, Age>(ParseAge(input));
}
