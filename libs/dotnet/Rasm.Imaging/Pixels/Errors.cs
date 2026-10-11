using PeterO.Numbers;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidPixelValue = 1,
    CodecRefused = 2,
    LayerAbsent = 3,
    PngRefused = 4,
    CicpUnsupported = 5,
    UnequalFrames = 6,
    KernelSizeUnsupported = 7,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidPixelValue() : Expected("pixel value out of range", (int)Codes.InvalidPixelValue), IValidationError<InvalidPixelValue> {
    public static InvalidPixelValue Create(string message) => new();
}

public sealed record CodecRefused : Expected {
    public CodecRefused(ExrResult status, Option<Exception> exception)
        : base("EXR codec refused the operation", (int)Codes.CodecRefused, exception.Map(static thrown => New(thrown))) => Status = status;

    public ExrResult Status { get; }

    public static Fin<T> Unless<T>(ReaderResult<T> result) where T : class => Optional(result.Value).ToFin(new CodecRefused(result.Status, Optional(result.Error)));

    public static Fin<Unit> Unless(WriterResult result) => result.IsSuccess ? unit : new CodecRefused(result.Status, Optional(result.Error));
}

public sealed record LayerAbsent(string Layer) : Expected("EXR file holds no such layer", (int)Codes.LayerAbsent);

public sealed record PngRefused : Expected {
    public PngRefused(Error cause) : base("PNG decoder refused the file", (int)Codes.PngRefused, cause) { }
}

public sealed record CicpUnsupported(CicpColorPrimaries Primaries, CicpTransferCharacteristics Transfer, CicpMatrixCoefficients Matrix, bool FullRange)
    : Expected("PNG cICP chunk states an encoding Rasm holds no gamut and transfer for", (int)Codes.CicpUnsupported);

public sealed record UnequalFrames(PixelExtent Reference, PixelExtent Test) : Expected("frames of unequal size", (int)Codes.UnequalFrames);

public sealed record KernelSizeUnsupported(ERational Radius) : Expected("native kernel size is not representable", (int)Codes.KernelSizeUnsupported);
