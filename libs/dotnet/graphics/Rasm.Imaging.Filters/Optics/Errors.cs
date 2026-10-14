namespace Rasm.Imaging.Filters.Optics;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidOptics = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidOptics() : Expected("optics value out of range", (int)Codes.InvalidOptics), IValidationError<InvalidOptics> {
    public static InvalidOptics Create(string message) => new();
}
