namespace Rasm.Imaging.Filters.Optics;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidOptics = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidOptics() : Expected("optics value rejected", (int)Codes.InvalidOptics), IValidationError<InvalidOptics> {
    public static InvalidOptics Create(string message) => new();
}
