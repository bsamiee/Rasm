namespace Lab.F06;

internal sealed record Employee(string FirstName, string LastName, string Role);

internal static class Descriptions {
    private static readonly Seq<Func<Employee, string>> Descriptors = [
        static employee => $"First name: {employee.FirstName}",
        static employee => $"Last name: {employee.LastName}",
        static employee => $"Role: {employee.Role}",
    ];

    public static string Describe(Employee employee) => string.Join(Environment.NewLine, Descriptors.Map(describe => describe(employee)));
}

internal static class Policies {
    public static bool IsValid<T>(this T value, Seq<Func<T, bool>> rules) => rules.ForAll(rule => rule(value));

    public static bool IsInvalid<T>(this T value, Seq<Func<T, bool>> violations) => violations.Exists(rule => rule(value));
}

internal static class RuleTables {
    public static TOutput Match<TInput, TOutput>(
        this TInput value,
        Func<TInput, TOutput> fallback,
        Seq<(Func<TInput, bool> When, Func<TInput, TOutput> Then)> cases) =>
        cases.Find(c => c.When(value)).Match(Some: c => c.Then(value), None: () => fallback(value));

    public static decimal NetIncome(decimal income) =>
        income.Match(static x => x * 0.55m, [
            (static x => x <= 12_570m, static x => x),
            (static x => x <= 50_270m, static x => x * 0.80m),
            (static x => x <= 150_000m, static x => x * 0.60m),
        ]);
}

internal static class Lookups {
    public static Func<int, string> ActorByNumber(HashMap<int, string> actors) =>
        number => actors.Find(number).IfNone("Unknown");
}

internal static class Parsing {
    public static int ToInt(string text, int fallback) => parseInt(text).IfNone(fallback);
}
