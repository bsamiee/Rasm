using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling;

// --- [ERRORS] --------------------------------------------------------------------------
public abstract record NetworkSurfaceFailed : Expected {
    private NetworkSurfaceFailed(string message, int code) : base(message, code) { }

    public sealed record Sorting() : NetworkSurfaceFailed("Network surface curve sorting failed", ErrorOps.Code<Sorting>());

    public sealed record Initialization() : NetworkSurfaceFailed("Network surface initialization failed", ErrorOps.Code<Initialization>());

    public sealed record Build() : NetworkSurfaceFailed("Network surface build failed", ErrorOps.Code<Build>());

    public sealed record Validity() : NetworkSurfaceFailed("Network surface is not valid", ErrorOps.Code<Validity>());
}

public abstract record VariationalPatchFailed : Expected {
    private VariationalPatchFailed(string message, int code) : base(message, code) { }

    public sealed record WithReason(string Reason) : VariationalPatchFailed("Variational patch failed with {Reason}", ErrorOps.Code<WithReason>());

    public sealed record WithoutReason() : VariationalPatchFailed("Variational patch failed", ErrorOps.Code<WithoutReason>());
}

public sealed record BooleanUnionFailed(Seq<Point3d> NakedEdges, Seq<Point3d> BadIntersections, Seq<Point3d> NonManifoldEdges)
    : Expected("Boolean union failed", ErrorOps.Code<BooleanUnionFailed>());

public sealed record ParallelLines(string Member) : Expected("{Member} found the lines parallel", ErrorOps.Code<ParallelLines>());

public sealed record OutOfDomain(string Member, double Parameter) : Expected("{Member} parameter {Parameter} is outside the domain", ErrorOps.Code<OutOfDomain>()) {
    public static Fin<Unit> Unless(Interval domain, double parameter, string member) => domain.IncludesParameter(parameter) ? unit : new OutOfDomain(member, parameter);
}

public sealed record Degenerate(string Member) : Expected("{Member} is degenerate", ErrorOps.Code<Degenerate>()) {
    public static Fin<Unit> Unless(bool sound, string member) => sound ? unit : new Degenerate(member);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeometryResults {
    public static IO<Seq<T>> Acquire<T>(Func<T?[]?> create, string member) where T : GeometryBase =>
        from results in IO.lift(create)
        from kept in GeometryOps.OnFailure(IO.lift(() => Kept(results, member)), Released(results))
        select kept;

    public static IO<Seq<(T Result, TRow Row)>> Acquire<T, TRow>(Func<(T?[]? Results, IReadOnlyList<TRow>? Rows)> create, string member) where T : GeometryBase =>
        from answer in IO.lift(create)
        from kept in GeometryOps.OnFailure(
            IO.lift(() =>
                from results in Kept(answer.Results, member)
                from rows in Missing.Unless(answer.Rows, member)
                from counted in CountMismatch.Unless(results.Count, rows.Count, member)
                select results.Zip(toSeq(rows))),
            Released(answer.Results))
        select kept;

    public static IO<Seq<T>> Complete<T>(Seq<Option<T>> answers, Func<Seq<T>, IO<Unit>> release, string member) =>
        from kept in IO.lift(() => answers.Somes())
        from complete in GeometryOps.OnFailure(IO.lift(() => Missing.Unless(kept.Count == answers.Count, member)), release(kept))
        select kept;

    internal static Fin<Seq<T>> Kept<T>(T?[]? results, string member) where T : GeometryBase =>
        from array in Missing.Unless(results, member)
        let valid = toSeq(array).Choose(static result => Optional(result).Filter(static geometry => geometry.IsValid)).Strict()
        from sound in valid.Count == array.Length ? Fin.Succ(unit) : new InvalidOutput(member, array.Length - valid.Count)
        from filled in Answers.NonEmpty(valid, member)
        select filled;

    internal static IO<Unit> Released<T>(T?[]? results) where T : GeometryBase =>
        Disposal.Release(Answers.Present(results));

    internal static Fin<Seq<Unit>> Sets<T>((Seq<T> Items, string Member) first, (Seq<T> Items, string Member) second) =>
        (Invalid.Unless(!first.Items.IsEmpty, first.Member).ToValidation() & Invalid.Unless(!second.Items.IsEmpty, second.Member).ToValidation()).ToFin();
}
