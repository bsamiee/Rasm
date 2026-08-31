namespace Lab.T07;

internal sealed record User(string Name);

internal static class Projections {
    public static IReadOnlyCollection<User> Sample() => [new User("ada"), new User("grace")];
    public static IReadOnlyCollection<string> Names(IReadOnlyCollection<User> users) => users.ToReadOnlyCollection(static user => user.Name);
    public static IReadOnlyCollection<string> UpperNames(IReadOnlyCollection<User> users) => users.Select(static user => user.Name.ToUpperInvariant()).ToReadOnlyCollection(users.Count);
}
