using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Denoise;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidDenoise = 1,
    OidnUnknown = 2,
    OidnOutOfMemory = 3,
    OidnUnsupportedHardware = 4,
    OidnQualityUnsupported = 5,
    DenoiseGuideAbsent = 6,
    DenoiseGuideUncovered = 7,
    OidnInvalidArgument = 8,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidDenoise() : Expected("denoise value out of range", (int)Codes.InvalidDenoise), IValidationError<InvalidDenoise> {
    public static InvalidDenoise Create(string message) => new();
}

public sealed record OidnUnknown() : Expected("Open Image Denoise failed with an unknown error", (int)Codes.OidnUnknown);

public sealed record OidnOutOfMemory() : Expected("Open Image Denoise ran out of memory", (int)Codes.OidnOutOfMemory);

public sealed record OidnUnsupportedHardware() : Expected("Open Image Denoise does not support this hardware", (int)Codes.OidnUnsupportedHardware);

public sealed record OidnQualityUnsupported(DenoiseQuality Quality, int Version) : Expected("denoise quality exceeds the host's Open Image Denoise", (int)Codes.OidnQualityUnsupported);

public sealed record DenoiseGuideAbsent(GuideChannel Channel) : Expected("selected denoise guide is absent", (int)Codes.DenoiseGuideAbsent);

public sealed record DenoiseGuideUncovered(GuideChannel Channel) : Expected("selected denoise guide does not cover the color window on its canvas", (int)Codes.DenoiseGuideUncovered);

public sealed record OidnInvalidArgument() : Expected("Open Image Denoise refused the image arguments", (int)Codes.OidnInvalidArgument);
