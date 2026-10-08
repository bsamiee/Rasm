namespace Rasm.Imaging.Filters.Generators;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidGenerator = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidGenerator() : Expected("generator value outside its limits", (int)Codes.InvalidGenerator), IValidationError<InvalidGenerator> {
    public static InvalidGenerator Create(string message) => new();
}
