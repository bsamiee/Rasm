using System.Drawing;

namespace Rasm.Imaging.Tone;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidToneValue = 1,
    EmptyRegion,
    FrameWhiteOutOfRange,
    EmptyCenter,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidToneValue() : Expected("tone value out of range", (int)Codes.InvalidToneValue), IValidationError<InvalidToneValue> {
    public static InvalidToneValue Create(string message) => new();
}

public sealed record EmptyRegion(Rectangle Region) : Expected("region holds no covered pixel of finite value", (int)Codes.EmptyRegion);

public sealed record FrameWhiteOutOfRange(float Measured) : Expected("frame white falls outside the scene white range", (int)Codes.FrameWhiteOutOfRange);

public sealed record EmptyCenter(Rectangle Region) : Expected("metered center holds no kept sample at full center weight", (int)Codes.EmptyCenter);
