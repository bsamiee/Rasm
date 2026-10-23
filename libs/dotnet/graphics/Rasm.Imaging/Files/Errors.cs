namespace Rasm.Imaging.Files;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DestinationTaken(OutputPath Path) : Expected($"Destination file {Path} already exists", 0);
