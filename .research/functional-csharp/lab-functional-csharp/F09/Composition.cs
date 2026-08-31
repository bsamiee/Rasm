namespace Lab.F09;

internal sealed record Person(string FirstName, string LastName, decimal Earnings);

internal static class Email {
    public static string AbbreviateName(this Person person) => person.FirstName[..1] + person.LastName;

    public static string AppendDomain(this string name) => name + "@example.com";

    public static string Nested(Person person) => AppendDomain(AbbreviateName(person));
}
