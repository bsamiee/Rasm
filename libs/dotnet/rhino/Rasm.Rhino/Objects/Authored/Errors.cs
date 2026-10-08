namespace Rasm.Rhino.Objects.Authored;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    MissingHistoryInput = 1,
    StaleHistory,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record MissingHistoryInput(int Id) : Expected("History record holds no input under id {Id}", (int)Codes.MissingHistoryInput) {
    public static Fin<T> Unless<T>(bool found, T value, int id) => found ? value : new MissingHistoryInput(id);
}

public sealed record StaleHistory(int Recorded, int Current) : Expected("History record version {Recorded} differs from command version {Current}", (int)Codes.StaleHistory);
