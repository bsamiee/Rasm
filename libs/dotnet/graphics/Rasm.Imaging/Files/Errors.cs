using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;

namespace Rasm.Imaging.Files;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidFileValue = 1,
    DepthRefused,
    PassTaken,
    TiffRefused,
    DngExtentRefused,
    DocumentEmpty,
    LayerWindowRefused,
    WindowRefused,
    BufferBytesRefused,
    LayerCountRefused,
    PngRefused,
    CicpUnsupported,
    LutUnreadable,
    LookupShapeRefused,
    WriteRefused,
    DestinationTaken,
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TiffCall {
    public sealed record Open : TiffCall;
    public sealed record Field(TiffTag Tag) : TiffCall;
    public sealed record Scanline(int Row) : TiffCall;
    public sealed record Flush : TiffCall;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidFileValue() : Expected("file value rejected", (int)Codes.InvalidFileValue), IValidationError<InvalidFileValue> {
    public static InvalidFileValue Create(string message) => new();
}

public sealed record DepthRefused(OutputDepth Depth, Seq<OutputDepth> Held) : Expected("file format holds no sample of this depth", (int)Codes.DepthRefused);

public sealed record PassTaken(PassName Name) : Expected("pass name repeats in one pass set", (int)Codes.PassTaken);

public sealed record TiffRefused(TiffCall Call) : Expected("TIFF write refused", (int)Codes.TiffRefused);

public sealed record DngExtentRefused(PixelExtent Size) : Expected("frame outside the DNG extent LibRaw reads", (int)Codes.DngExtentRefused);

public sealed record DocumentEmpty() : Expected("layered document holds no raster layer", (int)Codes.DocumentEmpty);

public sealed record LayerWindowRefused(int Layer, (long Left, long Top, long Right, long Bottom) Window, PixelExtent Extent) : Expected("layer frame differs from the canvas of its file", (int)Codes.LayerWindowRefused);

public sealed record WindowRefused((long Left, long Top, long Right, long Bottom) Window) : Expected("window exceeds the coordinates its file format holds", (int)Codes.WindowRefused);

public sealed record BufferBytesRefused(long Bytes) : Expected("content exceeds the bytes one native buffer holds", (int)Codes.BufferBytesRefused);

public sealed record LayerCountRefused(int Records) : Expected("layer count exceeds the signed Photoshop field", (int)Codes.LayerCountRefused);

public sealed record PngRefused : Expected {
    public PngRefused(Error cause) : base("PNG decoder refused the file", (int)Codes.PngRefused, cause) { }
}

public sealed record CicpUnsupported(CicpColorPrimaries Primaries, CicpTransferCharacteristics Transfer, CicpMatrixCoefficients Matrix, bool FullRange)
    : Expected("PNG cICP chunk states an encoding Rasm holds no gamut and transfer for", (int)Codes.CicpUnsupported);

public sealed record LutUnreadable : Expected {
    public LutUnreadable(LutPath path, Error cause) : base("LUT file unreadable", (int)Codes.LutUnreadable, cause) => Path = path;

    public LutPath Path { get; }
}

public sealed record LookupShapeRefused(int Width, int Height) : Expected("lookup image size holds no lattice layout", (int)Codes.LookupShapeRefused);

public sealed record WriteRefused : Expected {
    public WriteRefused(OutputPath path, Error cause) : base("file write refused", (int)Codes.WriteRefused, cause) => Path = path;

    public OutputPath Path { get; }
}

public sealed record DestinationTaken(OutputPath Path) : Expected("destination file already exists", (int)Codes.DestinationTaken);
