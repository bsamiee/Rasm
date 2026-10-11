namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes { NetworkSurfaceFailed = 1, Degenerate = 2 }

public enum NetworkFailure { None = 0, Sorting = 1, Initialization = 2, Build = 3, Validity = 4 }

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NetworkSurfaceFailed(NetworkFailure Failure) : Expected("Network surface failed: {Failure}", (int)Codes.NetworkSurfaceFailed);

public sealed record Degenerate(string Member) : Expected("{Member} is degenerate", (int)Codes.Degenerate);
