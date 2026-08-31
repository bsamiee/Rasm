namespace Lab.F21;

internal static class CorrectForms {
    public static Fin<Age> Older(int years) => Age.From(years).Bind(static age => Age.From(age + 1));

    public static Fin<Age> Required(Option<int> years) => years.ToFin(new NotFound()).Bind(Age.From);

    public static OptionT<IO, Person> Lookup(Map<string, Person> people, string name) => OptionT.lift<IO, Person>(IO.lift(() => people.Find(name)));

    public static Option<string> Nickname(string? raw) => Optional(raw);
}
