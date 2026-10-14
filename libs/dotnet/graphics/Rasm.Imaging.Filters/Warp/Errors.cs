namespace Rasm.Imaging.Filters.Warp;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidWarp = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidWarp() : Expected("warp value rejected", (int)Codes.InvalidWarp), IValidationError<InvalidWarp> {
    public static InvalidWarp Create(string message) => new();
}
