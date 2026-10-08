using System.Drawing;
using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;

namespace Rasm.Imaging.Output;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidOutput = 1,
    DepthRefused,
    PassTaken,
    WriteRefused,
    TiffRefused,
    TiffBytesRefused,
    DngExtentRefused,
    DocumentEmpty,
    LayerWindowRefused,
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
public sealed record InvalidOutput() : Expected("output value rejected", (int)Codes.InvalidOutput), IValidationError<InvalidOutput> {
    public static InvalidOutput Create(string message) => new();
}

public sealed record DepthRefused(OutputDepth Depth, Seq<OutputDepth> Held) : Expected("file format holds no sample of this depth", (int)Codes.DepthRefused);

public sealed record PassTaken(PassName Name) : Expected("pass name repeats in one pass set", (int)Codes.PassTaken);

public sealed record WriteRefused : Expected {
    public WriteRefused(OutputPath path, Error cause) : base("image file write refused", (int)Codes.WriteRefused, cause) => Path = path;

    public OutputPath Path { get; }
}

public sealed record TiffRefused(TiffCall Call) : Expected("TIFF write refused", (int)Codes.TiffRefused);

public sealed record TiffBytesRefused(long Bytes) : Expected("content exceeds the bytes a classic TIFF holds", (int)Codes.TiffBytesRefused);

public sealed record DngExtentRefused(PixelExtent Size) : Expected("frame outside the DNG extent LibRaw reads", (int)Codes.DngExtentRefused);

public sealed record DocumentEmpty() : Expected("layered document holds no raster layer", (int)Codes.DocumentEmpty);

public sealed record LayerWindowRefused(int Layer, Rectangle Window, PixelExtent Extent) : Expected("layer frame differs from the canvas of its file", (int)Codes.LayerWindowRefused);
