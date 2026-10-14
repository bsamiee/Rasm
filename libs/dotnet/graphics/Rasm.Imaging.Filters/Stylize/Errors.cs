namespace Rasm.Imaging.Filters.Stylize;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidStylize = 1,
    NativeDimensionsUnsupported,
    KuwaharaFootprintUnsupported,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidStylize() : Expected("stylize value outside its limits", (int)Codes.InvalidStylize), IValidationError<InvalidStylize> {
    public static InvalidStylize Create(string message) => new();
}

public sealed record NativeDimensionsUnsupported(double Width, int Height) : Expected("image dimensions exceed native image limits", (int)Codes.NativeDimensionsUnsupported);

public sealed record KuwaharaFootprintUnsupported(double Across, double Down) : Expected("Kuwahara footprint exceeds native support limits", (int)Codes.KuwaharaFootprintUnsupported);
