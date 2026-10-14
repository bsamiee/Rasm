using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Detail;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidDetail = 1,
    GrainScanRefused,
    GrainFrameRefused,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidDetail() : Expected("detail value outside its limits", (int)Codes.InvalidDetail), IValidationError<InvalidDetail> {
    public static InvalidDetail Create(string message) => new();
}

public sealed record GrainScanRefused : Expected {
    public GrainScanRefused((double Width, double Height) size, Exception cause) : base("grain scan operation refused", (int)Codes.GrainScanRefused, New(cause)) => Size = size;

    public (double Width, double Height) Size { get; }
}

public sealed record GrainFrameRefused : Expected {
    public GrainFrameRefused(PixelExtent size, Exception cause) : base("grain frame operation refused", (int)Codes.GrainFrameRefused, New(cause)) => Size = size;

    public PixelExtent Size { get; }
}
