using Rasm.Rhino.Document;

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

    public const int InvalidOutput = 2010;
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

public sealed record InvalidOutput(string Member, int ItemCount) : Expected("{Member} returned {ItemCount} invalid items", Codes.InvalidOutput);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeometryResults {
    // --- [ACQUIRE]
    public static IO<T> Acquire<T>(Func<T?> create, string member) where T : GeometryBase =>
        Acquired(() => Fin.Succ(Single(create())), member, emptyFails: false).Map(static kept => kept[0]);

    public static IO<T> Acquire<T>(Func<Fin<T>> create, string member) where T : GeometryBase =>
        Acquired(() => create().Map(Single), member, emptyFails: false).Map(static kept => kept[0]);

    public static IO<Seq<T>> Acquire<T>(Func<T?[]?> create, string member, bool emptyFails) where T : GeometryBase =>
        Acquired(() => Fin.Succ(create()), member, emptyFails);

    public static IO<Seq<T>> Acquire<T>(Func<Fin<T[]>> create, string member, bool emptyFails) where T : GeometryBase =>
        Acquired(() => create().Map<T?[]?>(static results => results), member, emptyFails);

    public static IO<Seq<(T Result, TRow Row)>> Acquire<T, TRow>(Func<(T?[]? Results, IReadOnlyList<TRow>? Rows)> create, string member) where T : GeometryBase =>
        from answer in IO.lift(create)
        from kept in DisposalOps.OnFailure(
            IO.lift(() =>
                from results in Kept(answer.Results, member, emptyFails: false)
                from rows in Missing.Unless(answer.Rows, member)
                from counted in CountMismatch.Unless(results.Count, rows.Count, member)
                select results.Zip(toSeq(rows))),
            Released(answer.Results))
        select kept;

    public static IO<(T Copy, TResult Result)> EditCopy<T, TResult>(T source, Func<T, Fin<TResult>> edit) where T : GeometryBase =>
        from copy in GeometryOps.Duplicated(source)
        from result in DisposalOps.OnFailure(IO.lift(() => edit(copy)), IO.lift(copy.Dispose))
        select (copy, result);

    internal static Fin<Seq<T>> Kept<T>(T?[]? results, string member, bool emptyFails) where T : GeometryBase =>
        from array in Missing.Unless(results, member)
        let valid = toSeq(array).Choose(static result => Optional(result).Filter(static geometry => geometry.IsValid)).Strict()
        from sound in valid.Count == array.Length ? Fin.Succ(unit) : new InvalidOutput(member, array.Length - valid.Count)
        from filled in emptyFails ? Answers.NonEmpty(valid, member) : valid
        select filled;

    internal static IO<Unit> Released<T>(T?[]? results) where T : GeometryBase =>
        DisposalOps.Release(Answers.Present(results));

    private static IO<Seq<T>> Acquired<T>(Func<Fin<T?[]?>> create, string member, bool emptyFails) where T : GeometryBase =>
        from results in IO.lift(create)
        from kept in DisposalOps.OnFailure(IO.lift(() => Kept(results, member, emptyFails)), Released(results))
        select kept;

    private static T?[]? Single<T>(T? result) where T : GeometryBase =>
        result is null ? null : [result];

    // --- [CHECKS]
    internal static Fin<Unit> InRange(Seq<int> indices, int itemCount, string member) =>
        indices.Traverse(index => IndexOutOfRange.Unless(index, itemCount, member)).As().Map(static _ => unit);

    internal static Fin<Seq<Unit>> Sets<T>((Seq<T> Items, string Member) first, (Seq<T> Items, string Member) second) =>
        Seq(first, second).Traverse(static set => Invalid.Unless(!set.Items.IsEmpty, set.Member)).As();
}
