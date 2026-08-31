namespace Lab.F07;

internal sealed class MemoryConnection(Seq<Employee> employees) : ConnectionIO {
    private readonly Atom<Option<(SqlTemplate Template, object Parameters)>> last = Atom(Option<(SqlTemplate Template, object Parameters)>.None);

    public Option<SqlTemplate> LastTemplate => last.Value.Map(static query => query.Template);

    public Seq<T> Query<T>(SqlTemplate template, object parameters) {
        _ = last.Swap(_ => Some((template, parameters)));
        return toSeq(employees.OfType<T>());
    }
}

internal sealed class MemoryStore {
    private readonly Atom<Seq<BookTransfer>> saved = Atom(Seq<BookTransfer>());

    public int Count => saved.Value.Count;

    public IO<Unit> Save(BookTransfer transfer) => IO.lift(() => ignore(saved.Swap(transfers => transfers.Add(transfer))));
}

internal static class Samples {
    private static readonly Employee Ada = new(Guid.NewGuid(), "Lovelace");

    private static readonly DateTime Today = new(2030, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    public static Fin<Unit> Run() =>
        ShapeSample()
            .Bind(static _ => ParsingSample())
            .Bind(static _ => PipelineSample())
            .Bind(static _ => ResolutionSample())
            .Bind(static _ => BoundarySample())
            .Bind(static _ => DependencySample())
            .Bind(static _ => RootSample());

    private static Fin<Unit> ShapeSample() =>
        Check(
            nameof(ShapeSample),
            ("Add", Shapes.Add(100m, 200m) == 300m),
            ("First", Shapes.Add100(200m) == 300m),
            ("Second", Shapes.Add100(900m) == 1000m),
            ("par", string.Equals(Greetings.GreetFormally("Sam"), "Good evening, Sam", StringComparison.Ordinal)),
            ("curry", string.Equals(Curried.Message, "Hey, Sam", StringComparison.Ordinal)),
            ("Helper", Helper.Answer == 110m));

    private static Fin<Unit> ParsingSample() {
        const string linux = "Title,Author,Year\nDune,Herbert,1965\nEmma,Austen,1815";
        string windowsComma = linux.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        string windowsPipe = windowsComma.Replace(',', '|');
        Book dune = new("Dune", "Herbert", "1965");
        return Check(
            nameof(ParsingSample),
            ("Linux comma keeps header", Partials.ParseLinuxComma(linux).Count == 3),
            ("Windows comma", Partials.ParseWindowsComma(windowsComma).Head == Some(dune)),
            ("Curried comma", Families.ParseWindowsComma(windowsComma) == Partials.ParseWindowsComma(windowsComma)),
            ("Curried pipe", Families.ParseWindowsPipe(windowsPipe).Count == 2));
    }

    private static Fin<Unit> PipelineSample() =>
        Check(
            nameof(PipelineSample),
            ("Fahrenheit to Celsius", Temperature.FahrenheitToCelsius(212m) == 100m),
            ("Freezing", Temperature.FahrenheitToCelsius(32m) == 0m));

    private static Fin<Unit> ResolutionSample() {
        Greeter greeter = new(" - ");
        return Check(
            nameof(ResolutionSample),
            ("Field", string.Equals(Greeter.Greet("Hi", "Sam"), "Hi, Sam", StringComparison.Ordinal)),
            ("Property", string.Equals(greeter.GreetProperty("Hi", "Sam"), "Hi - Sam", StringComparison.Ordinal)),
            ("Factory", string.Equals(Greeter.CreateGreeter<int>()("Hi", 1), "Hi: 1", StringComparison.Ordinal)),
            ("fun", string.Equals(Greeter.GreetInformally("Sam"), "Hey Sam", StringComparison.Ordinal)));
    }

    private static Fin<Unit> BoundarySample() {
        MemoryConnection connection = new(Seq(Ada));
        Runtime runtime = new(connection, static () => Today);
        Fin<Option<Employee>> found = Lookups<Runtime>.LookupEmployee(Ada.Id).Run(runtime);
        Option<SqlTemplate> byId = connection.LastTemplate;
        Fin<Option<Employee>> missing = Lookups<Runtime>.LookupEmployee(Guid.NewGuid()).Run(new Runtime(new MemoryConnection(Seq<Employee>()), static () => Today));
        return Check(
            nameof(BoundarySample),
            ("Found", found == Pure(Some(Ada))),
            ("Template by id", byId == Some(Queries.EmployeeById)),
            ("Missing", missing == Pure(Option<Employee>.None)));
    }

    private static Fin<Unit> DependencySample() {
        Func<BookTransfer, Validation<Error, BookTransfer>> dateNotPast = Validators.DateNotPast(static () => Today);
        BookTransfer future = new(Ada.Id, Today.AddDays(1), 10m);
        BookTransfer past = new(Ada.Id, Today.AddDays(-1), 10m);
        return Check(
            nameof(DependencySample),
            ("Future", dateNotPast(future).IsSuccess),
            ("Past", dateNotPast(past).Match(Fail: static errors => errors.IsType<TransferDateIsPast>(), Succ: static _ => false)));
    }

    private static Fin<Unit> RootSample() {
        MemoryStore store = new();
        Runtime runtime = new(new MemoryConnection(Seq(Ada)), static () => Today);
        Runtime empty = new(new MemoryConnection(Seq<Employee>()), static () => Today);
        BookTransfer command = new(Ada.Id, Today.AddDays(1), 10m);
        Fin<Employee> booked = Host.Book(runtime, store.Save, command);
        Fin<Employee> rejected = Host.Book(runtime, store.Save, command with { Date = Today.AddDays(-1) });
        Fin<Employee> unknown = Host.Book(empty, store.Save, command);
        return Check(
            nameof(RootSample),
            ("Booked", booked == Pure(Ada)),
            ("Saved once", store.Count == 1),
            ("Rejected", rejected.Match(Succ: static _ => false, Fail: static error => error.IsType<TransferDateIsPast>())),
            ("Unknown", unknown.Match(Succ: static _ => false, Fail: static error => error.IsType<OwnerNotFound>())));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
