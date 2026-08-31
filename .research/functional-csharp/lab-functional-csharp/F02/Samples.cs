namespace Lab.F02;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        FootprintProbe,
        ConcurrencyProbe,
        EffectsProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> FootprintProbe() {
        using LanguageExt.Sys.Test.Runtime test = LanguageExt.Sys.Test.Runtime.New();
        _ = test.Env.Console.WriteKeyLine("Ada");
        Fin<Unit> greeted = Greeting.Greet<LanguageExt.Sys.Test.Runtime>().Run(test);
        Order order = new(Seq(new OrderLine(new Product("Coffee", 2.5m), 2), new OrderLine(new Product("Tea", 1m), 0)));
        (decimal total, Seq<OrderLine> linesToDelete) = Orders.RecomputeTotal(order);
        return Check(
            nameof(FootprintProbe),
            ("Greeted", greeted.IsSucc && toSeq(test.Env.Console) == Seq("Enter your name:", "Hello Ada")),
            ("GreetingFor", string.Equals(Greeting.GreetingFor("Ada"), "Hello Ada", StringComparison.Ordinal)),
            ("Total", total == 5.0m),
            ("LinesToDelete", linesToDelete.Count == 1 && string.Equals(linesToDelete[0].Product.Name, "Tea", StringComparison.Ordinal)));
    }

    private static Fin<Unit> ConcurrencyProbe() {
        Seq<string> items = Seq("coffee", "tEA");
        Seq<string> expected = Seq("1. Coffee", "2. Tea");
        ListFormatter formatter = new();
        Seq<string> counted = formatter.Format(items);
        Seq<string> countedAgain = formatter.Format(items);
        IO<Seq<string>> parallel = items.Traverse(static item => IO.pure(item.ToSentenceCase())).As();
        return Check(
            nameof(ConcurrencyProbe),
            ("Counted", counted == expected),
            ("CounterDrifts", countedAgain == Seq("3. Coffee", "4. Tea")),
            ("Zipped", Formatting.Format(items) == expected),
            ("ZippedAgain", Formatting.Format(items) == expected),
            ("Range", toSeq(Range(1, 3)) == Seq(1, 2, 3)),
            ("Parallel", parallel.RunSafe() == Pure(Seq("Coffee", "Tea"))));
    }

    private static Fin<Unit> EffectsProbe() {
        DateTime today = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        MakeTransfer current = new(today, "ABC");
        MakeTransfer past = new(today.AddDays(-1), "XYZ");
        DateNotPastValidator dateValidator = new(today);
        BicExistsValidator bicValidator = new(Seq("ABC"));
        MemoryConsole memory = new();
        Runtime runtime = new(new Clock(today), new LanguageExt.Sys.Test.Implementations.ConsoleIO(memory));
        _ = memory.WriteKeyLine("Bob");
        Fin<Unit> greeted = Greeting.Greet<Runtime>().Run(runtime);
        return Check(
            nameof(EffectsProbe),
            ("DateValid", dateValidator.IsValid(current)),
            ("DateInvalid", !dateValidator.IsValid(past)),
            ("BicValid", bicValidator.IsValid(current)),
            ("BicInvalid", !bicValidator.IsValid(past)),
            ("BicLoaded", Transfers.BicExists(IO.pure(Seq("ABC")), current).RunSafe().Exists(static ok => ok)),
            ("ClockValid", Capabilities.DateNotPast<Runtime>(current).Run(runtime).Exists(static ok => ok)),
            ("ClockInvalid", Capabilities.DateNotPast<Runtime>(past).Run(runtime).Exists(static ok => !ok)),
            ("GreetCustom", greeted.IsSucc && toSeq(memory) == Seq("Enter your name:", "Hello Bob")));
    }
}
