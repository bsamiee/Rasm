namespace Rasm.Imaging.Filters.Stylize;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidStylize = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidStylize() : Expected("stylize value rejected", (int)Codes.InvalidStylize), IValidationError<InvalidStylize> {
    public static InvalidStylize Create(string message) => new();
}
