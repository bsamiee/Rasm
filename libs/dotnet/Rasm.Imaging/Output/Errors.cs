namespace Rasm.Imaging.Output;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidOutput = 1,
    WriteRefused = 2,
    DestinationTaken = 3,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidOutput() : Expected("output value rejected", (int)Codes.InvalidOutput), IValidationError<InvalidOutput> {
    public static InvalidOutput Create(string message) => new();
}

public sealed record WriteRefused : Expected {
    public WriteRefused(OutputPath path, Error cause) : base("file write refused", (int)Codes.WriteRefused, cause) => Path = path;

    public OutputPath Path { get; }
}

public sealed record DestinationTaken(OutputPath Path) : Expected("destination file already exists", (int)Codes.DestinationTaken);
