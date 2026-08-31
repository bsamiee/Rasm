namespace Lab.F00;

internal sealed record InvalidAge() : Expected("age out of range", 1001), IValidationError<InvalidAge> {
    public static InvalidAge Create(string message) => new();
}

internal sealed record EmptyName() : Expected("name is empty", 1002);

[ValueObject<int>]
[ValidationError<InvalidAge>]
internal readonly partial struct Age {
    public static Fin<Age> From(int value) => Validate(value, provider: null, out Age item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref InvalidAge? validationError, ref int value) {
        if (value is < 0 or >= 120)
            validationError = new InvalidAge();
    }
}

internal static class Results {
    public static Fin<Unit> Probe() =>
        OptionProbe()
            .Bind(static _ => FinProbe())
            .Bind(static _ => ValidationProbe())
            .Bind(static _ => TryProbe())
            .Bind(static _ => ErrorProbe());

    private static Fin<Unit> OptionProbe() {
        const string? missing = null;
        Option<string> absent = Optional(missing);
        Option<int> present = Some(5).Filter(static x => x > 0).Do(static x => Console.WriteLine(x));
        int fallback = absent.Map(static s => s.Length).IfNone(static () => -1);
        Fin<int> lifted = present.ToFin(new EmptyName());
        Validation<Error, int> validated = present.ToValidation<Error>(new EmptyName());
        Seq<int> asSeq = present.ToSeq();
        Option<string?> someNull = Some(missing);
        Option<int> generic = parseInt<Option>("7").As();
        return Verify.Check(
            nameof(OptionProbe),
            ("absent.IsNone", absent.IsNone),
            ("present == Some(5)", present == Some(5)),
            ("fallback == -1", fallback == -1),
            ("lifted == Pure(5)", lifted == Pure(5)),
            ("validated.IsSuccess", validated.IsSuccess),
            ("asSeq == Seq(5)", asSeq == Seq(5)),
            ("someNull.IsSome", someNull.IsSome),
            ("generic == Some(7)", generic == Some(7)));
    }

    private static Fin<int> Positive(int value) =>
        from v in Pure(value).ToFin()
        from _ in guard(v > 0, Error.New("not positive"))
        select v;

    private static Fin<Unit> FinProbe() {
        Fin<Age> young = Age.From(30);
        Fin<Age> invalid = Age.From(200);
        Fin<int> recovered = invalid.Map(static a => (int)a).Catch(1001, static _ => Pure(0).ToFin()).As();
        Fin<string> described = young.Map(static a => (int)a).BiMap(static v => string.Create(CultureInfo.InvariantCulture, $"{v}"), static e => e);
        Either<Error, Age> asEither = young.ToEither();
        Option<Age> asOption = invalid.ToOption();
        Fin<int> created = Try.lift(static () => (int)Age.Create(200)).Run();
        bool observed = false;
        _ = young.IfSucc(_ => observed = true);
        return Verify.Check(
            nameof(FinProbe),
            ("young.IsSucc", young.IsSucc),
            ("invalid.IsFail", invalid.IsFail),
            ("recovered == Pure(0)", recovered == Pure(0)),
            ("described == Pure(\"30\")", described == Pure("30")),
            ("asEither.IsRight", asEither.IsRight),
            ("asOption.IsNone", asOption.IsNone),
            ("created.IsFail", created.IsFail),
            ("Age.Create(30) == 30", Age.Create(30) == 30),
            ("Positive(1).IsSucc", Positive(1).IsSucc),
            ("Positive(-1).IsFail", Positive(-1).IsFail),
            ("observed", observed));
    }

    private static Validation<Error, string> ValidName(string name) => string.IsNullOrWhiteSpace(name) ? new EmptyName() : name;

    private static Validation<Error, Person> ValidPerson(string name, int age) =>
        (ValidName(name), Age.From(age).ToValidation()).Apply(static (n, a) => new Person(n, (int)a)).As();

    private static Fin<Unit> ValidationProbe() {
        Validation<Error, Person> valid = ValidPerson("Ada", 36);
        Validation<Error, Person> invalid = ValidPerson("", 200);
        int errorCount = invalid.Match(Fail: static e => e.Count, Succ: static _ => 0);
        Validation<Error, Seq<string>> both = ValidName("a") & ValidName("b");
        Validation<Error, string> first = ValidName("") | ValidName("b");
        Validation<Error, Person> observed = valid.Do(static p => Console.WriteLine(p.Name));
        Fin<Person> exit = valid.ToFin();
        Validation<Error, Person> context = invalid.MapFail(static e => Error.New("form", e));
        return Verify.Check(
            nameof(ValidationProbe),
            ("valid.IsSuccess", valid.IsSuccess),
            ("errorCount == 2", errorCount == 2),
            ("both.IsSuccess", both.IsSuccess),
            ("first.IsSuccess", first.IsSuccess),
            ("observed.IsSuccess", observed.IsSuccess),
            ("exit.IsSucc", exit.IsSucc),
            ("context.IsFail", context.IsFail));
    }

    private static Fin<Unit> TryProbe() {
        Fin<int> failed = Try.lift(static () => int.Parse("x", CultureInfo.InvariantCulture)).Run();
        Fin<int> succeeded = Try.lift(static () => int.Parse("4", CultureInfo.InvariantCulture)).Run();
        bool exceptional = failed.Match(Succ: static _ => false, Fail: static e => e.IsExceptional);
        return Verify.Check(
            nameof(TryProbe),
            ("exceptional", exceptional),
            ("succeeded == Pure(4)", succeeded == Pure(4)));
    }

    private static Fin<Unit> ErrorProbe() {
        Error single = new InvalidAge();
        Error many = new InvalidAge() + new EmptyName();
        Error wrapped = Error.New("outer", single);
        Error fromCode = (1001, "age out of range");
        return Verify.Check(
            nameof(ErrorProbe),
            ("single.IsType<InvalidAge>()", single.IsType<InvalidAge>()),
            ("single.HasCode(1001)", single.HasCode(1001)),
            ("single.Is(new InvalidAge())", single.Is(new InvalidAge())),
            ("many.Count == 2", many.Count == 2),
            ("many.Head.HasCode(1001)", many.Head.HasCode(1001)),
            ("wrapped.Inner.IsSome", wrapped.Inner.IsSome),
            ("fromCode.HasCode(1001)", fromCode.HasCode(1001)),
            ("!many.IsEmpty", !many.IsEmpty),
            ("many.Filter<EmptyName>().Count == 1", many.Filter<EmptyName>().Count == 1));
    }
}
