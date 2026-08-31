namespace Lab.F21;

internal sealed record Person(string Name, Age Age);

internal sealed record Guest(string Name);

internal sealed record Member(int Id);

internal sealed record Clock(DateTimeOffset Now);

internal sealed record Runtime(Clock Clock) : Has<Eff<Runtime>, Clock> {
    static K<Eff<Runtime>, Clock> Has<Eff<Runtime>, Clock>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Clock);
}

internal static class Concerns {
    public static Option<Person> Find(Map<string, Person> people, string name) => people.Find(name);

    public static Fin<Age> Admit(int years) => Age.From(years);

    public static Either<Guest, Member> Visitor(string name, int id) => id > 0 ? Right(new Member(id)) : Left(new Guest(name));

    public static Validation<Error, Person> Register(string name, int years) =>
        (ValidName(name), Age.From(years).ToValidation()).Apply(static (n, a) => new Person(n, a)).As();

    public static Try<int> Parse(string text) => Try.lift(() => int.Parse(text, CultureInfo.InvariantCulture));

    public static IO<Person> Load(Map<string, Person> people, string name) => IO.lift(() => people.Find(name).ToFin(new NotFound()));

    public static Eff<RT, DateTimeOffset> Stamp<RT>() where RT : Has<Eff<RT>, Clock> => RT.Ask.Map(static clock => clock.Now).As();

    private static Validation<Error, string> ValidName(string name) => string.IsNullOrWhiteSpace(name) ? new EmptyName() : name;
}
