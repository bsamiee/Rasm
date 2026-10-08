namespace Rasm.Rhino.Modeling.Solids;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    BooleanUnionFailed = 1,
    NotSolid,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record BooleanUnionFailed(Seq<Point3d> NakedEdges, Seq<Point3d> BadIntersections, Seq<Point3d> NonManifoldEdges)
    : Expected("Boolean union failed", (int)Codes.BooleanUnionFailed);

public sealed record NotSolid(string Member) : Expected("{Member} requires a closed manifold brep", (int)Codes.NotSolid) {
    public static Fin<Unit> Unless(bool solid, string member) => solid ? unit : new NotSolid(member);
}
