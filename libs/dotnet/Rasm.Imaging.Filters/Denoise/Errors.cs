namespace Rasm.Imaging.Filters.Denoise;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidDenoise = 1,
    OidnUnknown,
    OidnOutOfMemory,
    OidnUnsupportedHardware,
    OidnQualityUnsupported,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidDenoise() : Expected("denoise value out of range", (int)Codes.InvalidDenoise), IValidationError<InvalidDenoise> {
    public static InvalidDenoise Create(string message) => new();
}

public sealed record OidnUnknown() : Expected("Open Image Denoise failed with an unknown error", (int)Codes.OidnUnknown);

public sealed record OidnOutOfMemory() : Expected("Open Image Denoise ran out of memory", (int)Codes.OidnOutOfMemory);

public sealed record OidnUnsupportedHardware() : Expected("Open Image Denoise does not support this hardware", (int)Codes.OidnUnsupportedHardware);

public sealed record OidnQualityUnsupported(DenoiseQuality Quality, int Version) : Expected("denoise quality exceeds the host's Open Image Denoise", (int)Codes.OidnQualityUnsupported);
