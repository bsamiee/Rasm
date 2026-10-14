namespace Rasm.Imaging.Quantization;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidQuantization = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidQuantization() : Expected("quantization key is outside its set", (int)Codes.InvalidQuantization), IValidationError<InvalidQuantization> {
    public static InvalidQuantization Create(string message) => new();
}
