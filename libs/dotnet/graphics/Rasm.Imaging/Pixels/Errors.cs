using PeterO.Numbers;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidPixelValue = 1,
    GamutUnmatched,
    LayerAbsent,
    UnequalFrames,
    KernelSizeUnsupported,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidPixelValue() : Expected("pixel value out of range", (int)Codes.InvalidPixelValue), IValidationError<InvalidPixelValue> {
    public static InvalidPixelValue Create(string message) => new();
}

public sealed record GamutUnmatched(Chromaticities Chromaticities) : Expected("file primaries match no gamut", (int)Codes.GamutUnmatched);

public sealed record LayerAbsent(string Layer) : Expected("EXR file holds no such layer", (int)Codes.LayerAbsent);

public sealed record UnequalFrames(PixelExtent Reference, PixelExtent Test) : Expected("frames of unequal size", (int)Codes.UnequalFrames);

public sealed record KernelSizeUnsupported(ERational Radius) : Expected("native kernel size is not representable", (int)Codes.KernelSizeUnsupported);
