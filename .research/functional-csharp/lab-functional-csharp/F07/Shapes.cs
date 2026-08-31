namespace Lab.F07;

internal static class Shapes {
    public static readonly Func<decimal, decimal, decimal> Add = static (x, y) => x + y;
    public static readonly Func<decimal, Func<decimal, decimal>> CurriedAdd = static x => y => Add(x, y);
    public static readonly Func<decimal, decimal> Add100 = CurriedAdd(100m);
}

internal static class Greetings {
    public static readonly Func<string, string, string> Greet = static (greeting, name) => $"{greeting}, {name}";
    public static readonly Func<string, string> GreetFormally = par(Greet, "Good evening");
}

internal static class Curried {
    public static readonly Func<string, Func<string, string>> Greet = curry(Greetings.Greet);
    public static readonly Func<string, string> GreetInformally = Greet("Hey");
    public static string Message => GreetInformally("Sam");
}

internal static class Helper {
    public static readonly Func<decimal, Func<decimal, decimal>> Add = curry(static (decimal x, decimal y) => x + y);
    public static readonly Func<decimal, decimal> Add10 = Add(10m);
    public static decimal Answer => Add10(100m); // 110
}
