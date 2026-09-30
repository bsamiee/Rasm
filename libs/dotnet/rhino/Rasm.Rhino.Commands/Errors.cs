using Rhino.Input;

namespace Rasm.Rhino.Commands;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int UnexpectedGetResult = 1100;

    public const int OptionNotAdded = 1101;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnexpectedGetResult(GetResult Result) : Expected("Getter returned the unhandled GetResult {Result}", Codes.UnexpectedGetResult);

public sealed record OptionNotAdded(string EnglishName) : Expected("Option {EnglishName} was not added", Codes.OptionNotAdded) {
    public static Fin<T> Unless<T>(bool added, T value, string englishName) => added ? value : new OptionNotAdded(englishName);
}
