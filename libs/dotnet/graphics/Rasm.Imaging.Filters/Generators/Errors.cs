namespace Rasm.Imaging.Filters.Generators;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidGenerator = 1,
    SceneLightsUnavailable,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidGenerator() : Expected("generator value outside its limits", (int)Codes.InvalidGenerator), IValidationError<InvalidGenerator> {
    public static InvalidGenerator Create(string message) => new();
}

public sealed record SceneLightsUnavailable() : Expected("scene lights are unavailable", (int)Codes.SceneLightsUnavailable);
