namespace Rasm.Rhino.Modeling;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int NetworkSurfaceSorting = 2000;

    public const int NetworkSurfaceInitialization = 2001;

    public const int NetworkSurfaceBuild = 2002;

    public const int NetworkSurfaceValidity = 2003;

    public const int OpenBoundary = 2004;

    public const int VariationalPatchFailed = 2005;

    public const int BooleanUnionFailed = 2006;

    public const int ParallelLines = 2007;

    public const int OutOfDomain = 2008;

    public const int Degenerate = 2009;
}

// --- [ERRORS] --------------------------------------------------------------------------
public abstract record NetworkSurfaceFailed : Expected {
    private NetworkSurfaceFailed(string message, int code) : base(message, code) { }

    public sealed record Sorting() : NetworkSurfaceFailed("Network surface curve sorting failed", Codes.NetworkSurfaceSorting);

    public sealed record Initialization() : NetworkSurfaceFailed("Network surface initialization failed", Codes.NetworkSurfaceInitialization);

    public sealed record Build() : NetworkSurfaceFailed("Network surface build failed", Codes.NetworkSurfaceBuild);

    public sealed record Validity() : NetworkSurfaceFailed("Network surface is not valid", Codes.NetworkSurfaceValidity);
}

/// <summary>Error for variational patch edges that join into an open loop</summary>
/// <remarks><para><see cref="Brep.CreateVariationalPatch(IEnumerable{Brep.CurveConstraint}, IEnumerable{Brep.CurveConstraint}, IEnumerable{Brep.PointConstraint}, Brep.VariationalPatchSettings, bool, CancellationToken, IProgress{double}, out Brep.VariationalPatchResult)" /> checks nothing before its native call, and edges that join into an open curve end the host process there</para><para>Invalid edge curves return no patch with the host's reason, and an empty edge set returns none</para></remarks>
public sealed record OpenBoundary(string Member) : Expected("{Member} must join into closed loops", Codes.OpenBoundary) {
    public static Fin<Unit> Unless(bool closed, string member) => closed ? unit : new OpenBoundary(member);
}

public sealed record VariationalPatchFailed(string Reason) : Expected("Variational patch failed with reason {Reason}", Codes.VariationalPatchFailed);

public sealed record BooleanUnionFailed(Seq<Point3d> NakedEdges, Seq<Point3d> BadIntersections, Seq<Point3d> NonManifoldEdges)
    : Expected("Boolean union failed", Codes.BooleanUnionFailed);

public sealed record ParallelLines() : Expected("Lines are parallel", Codes.ParallelLines);

public sealed record OutOfDomain(string Member, double Parameter) : Expected("{Member} parameter {Parameter} is outside the domain", Codes.OutOfDomain) {
    public static Fin<Unit> Unless(Interval domain, double parameter, string member) => domain.IncludesParameter(parameter) ? unit : new OutOfDomain(member, parameter);
}

public sealed record Degenerate(string Member) : Expected("{Member} is degenerate", Codes.Degenerate) {
    public static Fin<Unit> Unless(bool sound, string member) => sound ? unit : new Degenerate(member);

    public static Fin<Unit> Unless(Vector3d direction, string member) => direction.IsValid && !direction.IsTiny() ? unit : new Degenerate(member);
}

