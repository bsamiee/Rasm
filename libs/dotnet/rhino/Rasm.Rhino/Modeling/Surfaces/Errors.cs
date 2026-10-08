namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes { NetworkSurfaceFailed = 1, Degenerate }

public enum NetworkFailure { None = 0, Sorting, Initialization, Build, Validity }

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NetworkSurfaceFailed(NetworkFailure Failure) : Expected("Network surface failed: {Failure}", (int)Codes.NetworkSurfaceFailed);

public sealed record Degenerate(string Member) : Expected("{Member} is degenerate", (int)Codes.Degenerate);
