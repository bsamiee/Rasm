namespace Lab.F07;

internal sealed class Greeter(string separator) {
    public static readonly Func<string, string, string> Greet = static (greeting, name) => $"{greeting}, {name}";
    public Func<string, string, string> GreetProperty => (greeting, name) => $"{greeting}{separator}{name}";
    public static Func<string, TName, string> CreateGreeter<TName>() => static (greeting, name) => $"{greeting}: {name}";
    public static string GreetInformally(string name) => fun(static (string greeting, string who) => $"{greeting} {who}")("Hey", name);
}
