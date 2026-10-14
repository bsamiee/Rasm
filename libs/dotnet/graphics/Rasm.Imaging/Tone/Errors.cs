namespace Rasm.Imaging.Tone;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidToneValue = 1,
    FrameWhiteOutOfRange,
    EmptyCenter,
    UndefinedHableNormalization,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidToneValue() : Expected("tone value out of range", (int)Codes.InvalidToneValue), IValidationError<InvalidToneValue> {
    public static InvalidToneValue Create(string message) => new();
}

public sealed record FrameWhiteOutOfRange(double Measured) : Expected("frame white falls outside the scene white range", (int)Codes.FrameWhiteOutOfRange);

public sealed record EmptyCenter((long Left, long Top, long Right, long Bottom) Region) : Expected("metered center holds no kept sample at full center weight", (int)Codes.EmptyCenter);

public sealed record UndefinedHableNormalization(float White) : Expected("Hable curve is zero at the normalization white", (int)Codes.UndefinedHableNormalization);
