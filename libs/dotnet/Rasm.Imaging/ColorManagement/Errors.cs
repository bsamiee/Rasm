using System.Numerics;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.ColorManagement;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidColor = 1,
    GamutUnmatched = 2,
    WhiteWithoutLuminance = 3,
    WhiteUnresolved = 4,
    LutShapeRefused = 5,
    LutSamplesNotFinite = 6,
    LutDomainRefused = 7,
    LutNotInvertible = 8,
    StepNotInvertible = 9,
    LutLineMalformed = 10,
    LutStepRefused = 11,
    LutSizeAbsent = 12,
    LutCountMismatched = 13,
    LutFormatRefused = 14,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidColor() : Expected("color management value rejected", (int)Codes.InvalidColor), IValidationError<InvalidColor> {
    public static InvalidColor Create(string message) => new();
}

public sealed record GamutUnmatched(Chromaticities Chromaticities) : Expected("file primaries match no gamut", (int)Codes.GamutUnmatched);

public sealed record WhiteWithoutLuminance(double Luminance) : Expected("color has no luminance to correlate", (int)Codes.WhiteWithoutLuminance);

public sealed record WhiteUnresolved(Chromaticity Picked) : Expected("picked white has no temperature and tint", (int)Codes.WhiteUnresolved);

public sealed record LutShapeRefused(int Size, int Samples) : Expected("LUT size and sample count form no table", (int)Codes.LutShapeRefused);

public sealed record LutSamplesNotFinite() : Expected("LUT holds a non-finite sample", (int)Codes.LutSamplesNotFinite);

public sealed record LutDomainRefused(Vector3 Minimum, Vector3 Maximum) : Expected("LUT domain is not finite and ascending", (int)Codes.LutDomainRefused);

public sealed record LutNotInvertible(int Channel) : Expected("LUT channel has no exact inverse", (int)Codes.LutNotInvertible);

public sealed record StepNotInvertible(LutTable Step) : Expected("LUT step has no exact inverse", (int)Codes.StepNotInvertible);

public sealed record LutLineMalformed(LutFormat Format, int Line) : Expected("LUT file line misstated", (int)Codes.LutLineMalformed);

public sealed record LutStepRefused(LutFormat Format, int Line, string Element) : Expected("LUT process node outside the step set", (int)Codes.LutStepRefused);

public sealed record LutSizeAbsent(LutFormat Format) : Expected("LUT file states no size", (int)Codes.LutSizeAbsent);

public sealed record LutCountMismatched(LutFormat Format, long Stated, long Read) : Expected("LUT file entry count differs from its size", (int)Codes.LutCountMismatched);

public sealed record LutFormatRefused(LutFormat Format) : Expected("LUT format holds no such table", (int)Codes.LutFormatRefused);
