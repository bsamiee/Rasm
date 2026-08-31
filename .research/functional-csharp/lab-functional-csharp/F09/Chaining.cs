namespace Lab.F09;

internal static class Chaining {
    public static readonly Func<Person, string> EmailFor = compose<Person, string, string>(Email.AbbreviateName, Email.AppendDomain);

    public static string Chained(Person person) =>
        person
            .AbbreviateName()
            .AppendDomain();
}
