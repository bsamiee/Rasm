namespace Rasm.Rhino.Ui;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int WindowsOnly = 1400;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record WindowsOnly(string Member) : Expected("{Member} runs only on Windows", Codes.WindowsOnly) {
    public static Fin<Unit> Unless(bool windows, string member) => windows ? unit : new WindowsOnly(member);
}
