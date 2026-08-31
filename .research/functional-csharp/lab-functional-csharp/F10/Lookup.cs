namespace Lab.F10;

internal sealed record Person(int Id, string Name);

internal static class People {
    public static OptionT<IO, Person> GetPerson(Func<int, Person?> database, int id) =>
        OptionT.lift<IO, Person>(IO.lift(() => Optional(database(id))));
}

internal static class Greeting {
    public static IO<string> Describe(OptionT<IO, Person> lookup) =>
        lookup.Match(Some: static person => person.Name, None: static () => "no such person").As();
}

internal static class Mail {
    public static IO<Unit> SendEmail(Action<string> transport, string address) => IO.lift(() => transport(address));
}
