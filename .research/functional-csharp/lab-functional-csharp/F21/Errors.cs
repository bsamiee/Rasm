namespace Lab.F21;

internal static class Codes {
    public const int InvalidAge = 2101;
    public const int EmptyName = 2102;
    public const int NotFound = 2103;
    public const int Underage = 2104;
    public const int TooLarge = 2105;
}

internal sealed record InvalidAge() : Expected("age out of range", Codes.InvalidAge), IValidationError<InvalidAge> {
    public static InvalidAge Create(string message) => new();
}

internal sealed record EmptyName() : Expected("name is empty", Codes.EmptyName);

internal sealed record NotFound() : Expected("person not found", Codes.NotFound);

internal sealed record Underage() : Expected("person is under age", Codes.Underage);

internal sealed record TooLarge() : Expected("value is too large", Codes.TooLarge);

internal static class Classify {
    public static Fin<int> Captured(string text) => Try.lift(() => int.Parse(text, CultureInfo.InvariantCulture)).Run();

    public static bool Retryable(Error error) => error.Is(Errors.TimedOut) || error.IsType<Exceptional>();

    public static bool Rejected(Error error) => error.HasCode(Codes.InvalidAge) || error.IsType<EmptyName>();

    public static int AgeFaults(Error error) => error.Filter<InvalidAge>().Count;
}
