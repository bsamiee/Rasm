namespace Rasm.Imaging.Filters.Stylize;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidStylize = 1,
    CameraRequired = 2,
    CodecDimensionsUnsupported = 3,
    NativeSignalDimensionsUnsupported = 4,
    KuwaharaFootprintUnsupported = 5,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidStylize() : Expected("stylize value outside its limits", (int)Codes.InvalidStylize), IValidationError<InvalidStylize> {
    public static InvalidStylize Create(string message) => new();
}

public sealed record CameraRequired() : Expected("world-width outlines require a camera", (int)Codes.CameraRequired);

public sealed record CodecDimensionsUnsupported(double Width, int Height) : Expected("coded dimensions exceed native image limits", (int)Codes.CodecDimensionsUnsupported);

public sealed record NativeSignalDimensionsUnsupported(double Width, int Height) : Expected("signal dimensions exceed native image limits", (int)Codes.NativeSignalDimensionsUnsupported);

public sealed record KuwaharaFootprintUnsupported(double Across, double Down) : Expected("Kuwahara footprint exceeds native support limits", (int)Codes.KuwaharaFootprintUnsupported);
