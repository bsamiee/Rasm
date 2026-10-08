namespace Rasm.Rhino.UI.Rows;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnmatchedValues = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnmatchedValues(int ItemCount) : Expected("{ItemCount} pasted values match no row", (int)Codes.UnmatchedValues) {
    public static Fin<Unit> Unless(bool matched, int itemCount) => matched ? unit : new UnmatchedValues(itemCount);
}
