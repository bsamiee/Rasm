namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    NetworkSurfaceSorting = 1,
    NetworkSurfaceInitialization,
    NetworkSurfaceBuild,
    NetworkSurfaceValidity,
    Degenerate,
}

// --- [ERRORS] --------------------------------------------------------------------------
public abstract record NetworkSurfaceFailed : Expected {
    private NetworkSurfaceFailed(string message, Codes code) : base(message, (int)code) { }

    public sealed record Sorting() : NetworkSurfaceFailed("Network surface curve sorting failed", Codes.NetworkSurfaceSorting);

    public sealed record Initialization() : NetworkSurfaceFailed("Network surface initialization failed", Codes.NetworkSurfaceInitialization);

    public sealed record Build() : NetworkSurfaceFailed("Network surface build failed", Codes.NetworkSurfaceBuild);

    public sealed record Validity() : NetworkSurfaceFailed("Network surface is not valid", Codes.NetworkSurfaceValidity);
}

public sealed record Degenerate(string Member) : Expected("{Member} is degenerate", (int)Codes.Degenerate);
