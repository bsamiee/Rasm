using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SubdivisionCount : System.Numerics.IMinMaxValue<SubdivisionCount> {
    public static SubdivisionCount MinValue { get; } = new(1);
    public static SubdivisionCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<SubDVertexTag>(SkipIParsable = true, SkipIComparable = true)]
public readonly partial struct VertexTag {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SubDVertexTag value) =>
        validationError = value is SubDVertexTag.Smooth or SubDVertexTag.Corner ? null : new ValidationError($"{value} is no tag a vertex keeps");
}

[ValueObject<SubDEdgeTag>(SkipIParsable = true, SkipIComparable = true)]
public readonly partial struct EdgeTag {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SubDEdgeTag value) =>
        validationError = value is SubDEdgeTag.Smooth or SubDEdgeTag.Crease ? null : new ValidationError($"{value} is no tag an edge keeps");
}

[ValueObject<SubDEdgeSharpness>(SkipIParsable = true)]
public readonly partial struct EdgeSharpness {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SubDEdgeSharpness value) =>
        validationError = value.IsValidOrCrease ? null : new ValidationError($"{value} is neither a valid sharpness nor a crease");
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SubDEdit {
    public sealed record Subdivide(SubdivisionCount Count) : SubDEdit;
    public sealed record SubdivideFaces(Seq<uint> Faces) : SubDEdit;
    public sealed record SetControlNetPoints(HashMap<uint, Point3d> Moved) : SubDEdit;
    public sealed record InterpolateSurfacePoints(HashMap<uint, Point3d> Moved) : SubDEdit;
    public sealed record SetVertexSurfacePoint(uint Vertex, Point3d SurfacePoint) : SubDEdit;
    public sealed record TransformComponents(Seq<ComponentIndex> Components, Transform Xform, SubDComponentLocation ComponentLocation) : SubDEdit;
    public sealed record SetVertexTags(Seq<uint> Vertices, VertexTag Tag) : SubDEdit;
    public sealed record SetEdgeTags(Seq<uint> Edges, EdgeTag Tag) : SubDEdit;
    public sealed record SetEdgeSharpness(Seq<(uint Edge, EdgeSharpness Sharpness)> Edges, bool PreserveSymmetry) : SubDEdit;
    public sealed record ClearEdgeSharpness : SubDEdit;
    public sealed record MergeAllCoplanarFaces : SubDEdit;
    public sealed record PackFaces : SubDEdit;
    public sealed record Flip : SubDEdit;
    public sealed record DeleteComponents(Seq<ComponentIndex> Components) : SubDEdit;
    public sealed record DissolveOrDeleteComponents(Seq<ComponentIndex> Components) : SubDEdit;
    public sealed record FillHole(uint BoundaryEdge) : SubDEdit;
    public sealed record Bridge(uint EdgeA, uint EdgeB) : SubDEdit;
    public sealed record Offset(double Distance, bool Solidify) : SubDEdit;

    public IO<EditStep<SubD, uint>> Step(Tolerances tolerances, SubD subd, Action<IDisposable> retain) =>
        Switch(
            (SubD: subd, Tolerances: tolerances, Retain: retain),
            subdivide: static (at, of) => Copies.Acquire(nameof(SubD.Subdivide), _ => Tagged(at.SubD, at.SubD.Subdivide((int)of.Count), Edits.Accepted<SubD, uint>), at.Retain),
            subdivideFaces: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdFace, of.Faces))
                >> Copies.Acquire(nameof(SubD.Subdivide), _ => Tagged(at.SubD, at.SubD.Subdivide(of.Faces.Map(static id => (int)id)), Edits.Accepted<SubD, uint>), at.Retain),
            setControlNetPoints: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, toSeq(of.Moved.Keys)))
                >> Copies.Acquire(nameof(SubDVertex.SetControlNetPoint), _ => Tagged(
                    at.SubD,
                    toSeq(of.Moved.AsIterable()).Map(row => at.SubD.Vertices.Find(row.Key).SetControlNetPoint(row.Value, bClearNeighborhoodCache: false)).Strict(),
                    static (edited, moved) => {
                        edited.ClearEvaluationCache();
                        return Edits.Accepted<SubD, uint>(edited, moved.ForAll(static held => held));
                    }), at.Retain),
            interpolateSurfacePoints: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, toSeq(of.Moved.Keys)))
                >> Copies.Acquire(nameof(SubD.InterpolateSurfacePoints), _ => Tagged(
                    at.SubD,
                    at.SubD.InterpolateSurfacePoints([.. toSeq(at.SubD.Vertices).Map(vertex => of.Moved.Find(vertex.Id).IfNone(vertex.SurfacePoint))]),
                    Edits.Accepted<SubD, uint>), at.Retain),
            setVertexSurfacePoint: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, [of.Vertex]))
                >> Copies.Acquire(nameof(SubD.SetVertexSurfacePoint), _ => Tagged(at.SubD, at.SubD.SetVertexSurfacePoint(of.Vertex, of.SurfacePoint), Edits.Accepted<SubD, uint>), at.Retain),
            transformComponents: static (at, of) =>
                IO.lift(() => Copies.Within(at.SubD, of.Components))
                >> Copies.Acquire(nameof(SubD.TransformComponents), _ => Tagged(at.SubD, at.SubD.TransformComponents(of.Components, of.Xform, of.ComponentLocation), static (edited, count) => {
                    edited.ClearEvaluationCache();
                    return Edits.Count(edited, count);
                }), at.Retain),
            setVertexTags: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, of.Vertices))
                >> Copies.Acquire(nameof(SubDVertexList.SetVertexTags), _ => {
                    at.SubD.Vertices.SetVertexTags(of.Vertices.Map(static id => (int)id), (SubDVertexTag)of.Tag);
                    return Tagged(at.SubD, unit, static (edited, _) => Edits.Completed<SubD, uint>(edited));
                }, at.Retain),
            setEdgeTags: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdEdge, of.Edges))
                >> Copies.Acquire(nameof(SubDEdgeList.SetEdgeTags), _ => {
                    at.SubD.Edges.SetEdgeTags(of.Edges.Map(static id => (int)id), (SubDEdgeTag)of.Tag);
                    return Tagged(at.SubD, unit, static (edited, _) => Edits.Completed<SubD, uint>(edited));
                }, at.Retain),
            setEdgeSharpness: static (at, of) =>
                IO.lift(() => Within(at.SubD, ComponentIndexType.SubdEdge, of.Edges.Map(static row => row.Edge)))
                >> Copies.Acquire(nameof(SubD.SetEdgeSharpness), _ => Tagged(
                    at.SubD,
                    at.SubD.SetEdgeSharpness(of.Edges.Map(row => at.SubD.Edges.Find(row.Edge)), of.Edges.Map(static row => (SubDEdgeSharpness)row.Sharpness), of.PreserveSymmetry),
                    Edits.Count), at.Retain),
            clearEdgeSharpness: static (at, _) => Copies.Acquire(nameof(SubD.ClearEdgeSharpness), _ => Tagged(at.SubD, at.SubD.ClearEdgeSharpness(), Edits.Count), at.Retain),
            mergeAllCoplanarFaces: static (at, _) => Copies.Acquire(nameof(SubD.MergeAllCoplanarFaces), _ =>
                Tagged(at.SubD, at.SubD.MergeAllCoplanarFaces(at.Tolerances.Absolute, at.Tolerances.Angle), Edits.Changed<SubD, uint>), at.Retain),
            packFaces: static (at, _) => Copies.Acquire(nameof(SubD.PackFaces), _ => Tagged(at.SubD, at.SubD.PackFaces(), Edits.Count), at.Retain),
            flip: static (at, _) => Copies.Acquire(nameof(SubD.Flip), _ => Tagged(at.SubD, at.SubD.Flip(), Edits.Accepted<SubD, uint>), at.Retain),
            deleteComponents: static (at, of) =>
                IO.lift(() => Copies.Within(at.SubD, of.Components))
                >> Copies.Acquire(nameof(SubD.DeleteComponents), _ => Tagged(at.SubD, at.SubD.DeleteComponents(of.Components, markDeletedFaceEdges: false), Edits.Accepted<SubD, uint>), at.Retain),
            dissolveOrDeleteComponents: static (at, of) =>
                IO.lift(() => Copies.Within(at.SubD, of.Components))
                >> Copies.Acquire(nameof(SubD.DissolveOrDeleteComponents), _ => Tagged(at.SubD, at.SubD.DissolveOrDeleteComponents(of.Components), Edits.Count), at.Retain),
            fillHole: static (at, of) =>
                from hole in IO.lift(() => Hole(at.SubD, of.BoundaryEdge))
                from step in Copies.Acquire(nameof(SubDFaceList.Add), _ => Tagged(at.SubD, at.SubD.Faces.Add(hole), static (edited, face) => Edits.Accepted<SubD, uint>(edited, face is not null)), at.Retain)
                select step,
            bridge: static (at, of) =>
                from rails in IO.lift(() => Rails(at.SubD, of.EdgeA, of.EdgeB))
                from step in Copies.Acquire(nameof(SubDFaceList.Add), _ => Tagged(
                    at.SubD,
                    rails.Map(rail => Quad(at.SubD, rail.A, rail.B)).Strict(),
                    static (edited, faces) => faces.ForAll(static face => face.IsSome)
                        ? Edits.Count(edited, (uint)faces.Count)
                        : Edits.Accepted<SubD, uint>(edited, accepted: false)), at.Retain)
                select step,
            offset: static (at, of) => Copies.Acquire(nameof(SubD.Offset), _ => Edits.Replaced<SubD, uint>(at.SubD.Offset(of.Distance, of.Solidify)), at.Retain));

    internal static Fin<Seq<uint>> Within(SubD subd, ComponentIndexType type, Seq<uint> ids) =>
        Copies.Within(subd, type, ids.Map(static id => (int)id)).Map(_ => ids);

    internal static Answer<EditStep<SubD, uint>> Tagged<THost>(SubD subd, THost host, Func<SubD, THost, Answer<EditStep<SubD, uint>>> answer) {
        _ = subd.UpdateAllTagsAndSectorCoefficients();
        return answer(subd, host);
    }

    private static Fin<Seq<SubDEdge>> Hole(SubD subd, uint edge) =>
        from held in Within(subd, ComponentIndexType.SubdEdge, [edge])
        from boundary in Boundary(Boundaries(subd), edge)
        from closed in guard(boundary.Closed, Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"SubdEdge {edge} lies on an open boundary chain of {boundary.Edges.Count} edges")))
        select boundary.Edges;

    private static Fin<Seq<(SubDEdge A, SubDEdge B)>> Rails(SubD subd, uint edgeA, uint edgeB) =>
        from held in Within(subd, ComponentIndexType.SubdEdge, [edgeA, edgeB])
        let boundaries = Boundaries(subd)
        from first in Boundary(boundaries, edgeA)
        from second in Boundary(boundaries, edgeB)
        from apart in guard(!first.Edges.Exists(link => link.Id == edgeB), Error.New(ErrorCodes.ValidationFailed, $"SubdEdge {edgeA} and {edgeB} lie on one boundary chain"))
        from even in guard(first.Edges.Count == second.Edges.Count, Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"boundary chains of SubdEdge {edgeA} and {edgeB} hold {first.Edges.Count} and {second.Edges.Count} edges")))
        select Anchored(first, edgeA).Zip(Anchored(second with { Edges = second.Edges.Rev() }, edgeB), static (a, b) => (A: a, B: b)).Strict();

    private static Seq<(Seq<SubDEdge> Edges, bool Closed)> Boundaries(SubD subd) =>
        toSeq(SubDEdgeChain.SortEdgesIntoEdgeChains(subd, toSeq(subd.Edges).Filter(static edge => edge.FaceCount is 1))).Map(Free).Strict();

    private static (Seq<SubDEdge> Edges, bool Closed) Free(SubDEdge[] links) =>
        toSeq(links).Map(static edge => edge.RelativeFaceLeft is null ? edge : (SubDEdge)edge.ReverseComponentDirection()).Strict() switch {
            var free when free.Count > 1 && free[0].RelativeVertexTo.Id != free[1].RelativeVertexFrom.Id => Closure(free.Rev()),
            var free => Closure(free),
        };

    private static (Seq<SubDEdge> Edges, bool Closed) Closure(Seq<SubDEdge> edges) =>
        (Edges: edges, Closed: edges.Count > 2 && edges[0].RelativeVertexFrom.Id == edges[^1].RelativeVertexTo.Id);

    private static Fin<(Seq<SubDEdge> Edges, bool Closed)> Boundary(Seq<(Seq<SubDEdge> Edges, bool Closed)> boundaries, uint edge) =>
        boundaries.Filter(boundary => boundary.Edges.Exists(link => link.Id == edge)).Head
            .ToFin(Error.New(ErrorCodes.ValidationFailed, $"SubdEdge {edge} is no boundary edge"));

    private static Seq<SubDEdge> Anchored((Seq<SubDEdge> Edges, bool Closed) boundary, uint edge) =>
        (boundary.Closed ? boundary.Edges.TakeWhile(link => link.Id != edge).Count : 0) switch {
            var lead => boundary.Edges.Skip(lead) + boundary.Edges.Take(lead),
        };

    private static Option<SubDFace> Quad(SubD subd, SubDEdge a, SubDEdge b) =>
        from up in Rung(subd, a.RelativeVertexTo, b.RelativeVertexFrom)
        from down in Rung(subd, b.RelativeVertexTo, a.RelativeVertexFrom)
        from face in Optional(subd.Faces.Add([a, up, b, down]))
        select face;

    private static Option<SubDEdge> Rung(SubD subd, SubDVertex from, SubDVertex to) =>
        Optional(subd.Edges.FindOrAdd(from, to)).Map(rung => {
            rung.ComponentDirection = rung.VertexFrom.Id != from.Id;
            return rung;
        });
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ChainSeed {
    public sealed record Sorted(Seq<uint> Edges) : ChainSeed;
    public sealed record Grown(uint Edge, ChainDirection Direction, SubDChainType Type) : ChainSeed;
}

public sealed record SubDChain(Seq<(uint Edge, bool Reversed)> Edges, bool Closed, Option<NurbsCurve> Curve, Option<NurbsCurve> LoftCurve);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SubDs {
    // --- [EDITS]
    public static IO<(SubD Result, Seq<(SubDEdit Edit, EditResult<uint> Result)> Results)> Edit(Tolerances tolerances, SubD source, Seq<SubDEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (subd, edit, held) => edit.Step(tolerances, subd, held), Settled, retain);

    public static IO<Seq<(SubDEdit Edit, EditResult<uint> Result)>> Revise(Tolerances tolerances, RhinoDoc doc, Guid id, Seq<SubDEdit> edits) =>
        RhinoObjects.ReplaceGeometry(doc, id, (SubD target, Action<IDisposable> held) => Edit(tolerances, target, edits, held));

    public static IO<Unit> InterpolateVertices(RhinoDoc doc, Guid id, HashMap<uint, Point3d> moved) =>
        RhinoObjects.ReplaceGeometry(doc, id, (SubD target, Action<IDisposable> held) =>
            IO.lift(() => SubDEdit.Within(target, ComponentIndexType.SubdVertex, toSeq(moved.Keys)))
            >> Copies.Acquire(nameof(SubD.InterpolateSurfacePoints), _ => toSeq(moved.AsIterable()) switch {
                var rows => SubDEdit.Tagged(target, target.InterpolateSurfacePoints([.. rows.Map(static row => row.Key)], [.. rows.Map(static row => row.Value)]), Edits.Accepted<SubD, uint>),
            }, held)
            >> Settled(target)
            >> IO.pure((target, unit)));

    private static IO<Unit> Settled(SubD subd) => IO.lift(() => ignore(subd.UpdateSurfaceMeshCache(lazyUpdate: true)));

    // --- [CHAINS]
    public static IO<Seq<SubDChain>> Chains(SubD subd, ChainSeed seed, Action<IDisposable> retain) =>
        seed.Switch(
            (SubD: subd, Retain: retain),
            sorted: static (at, of) =>
                IO.lift(() => SubDEdit.Within(at.SubD, ComponentIndexType.SubdEdge, of.Edges))
                >> Copies.Acquire(nameof(SubDEdgeChain.SortEdgesIntoEdgeChains), _ =>
                    toSeq(SubDEdgeChain.SortEdgesIntoEdgeChains(at.SubD, of.Edges.Map(edge => at.SubD.Edges.Find(edge))))
                        .Map(links => Answer.Successor(Linked(at.SubD, links), Read))
                        .Traverse(static chain => chain).As(), at.Retain),
            grown: static (at, of) =>
                IO.lift(() => SubDEdit.Within(at.SubD, ComponentIndexType.SubdEdge, [of.Edge]))
                >> Copies.Acquire(nameof(SubDEdgeChain.AddAllNeighbors), _ => Answer.Successor(new SubDEdgeChain(at.SubD, at.SubD.Edges.Find(of.Edge)), chain => Grown(chain, of.Direction, of.Type)), at.Retain));

    private static SubDEdgeChain Linked(SubD subd, SubDEdge[] links) {
        SubDEdgeChain chain = new(subd, links[0]);
        _ = toSeq(links).Tail.Iter(link => _ = chain.AddEdge(link));
        return chain;
    }

    private static Answer<Seq<SubDChain>> Grown(SubDEdgeChain chain, ChainDirection direction, SubDChainType type) {
        _ = chain.AddAllNeighbors(direction, type);
        return Read(chain).Map(static read => Seq(read));
    }

    private static Answer<SubDChain> Read(SubDEdgeChain chain) =>
        (Answer.Optional(chain.ToNurbsCurve()), Answer.Optional(chain.ToLoftCurve())).Apply((curve, loft) => new SubDChain(
            toSeq(chain.Edges).Map(static edge => (Edge: edge.Id, Reversed: edge.ComponentDirection)).Strict(),
            chain.IsClosedLoop,
            curve,
            loft)).As();

    // --- [READS]
    public static IO<Seq<Option<(Point3d Point, SubDComponentParameter Parameter)>>> ClosestPoints(SubD subd, Seq<Point3d> testPoints, Option<double> maximumDistance) =>
        IO.lift(() => {
            _ = subd.ClosestPoints(testPoints, out Point3d[] points, out SubDComponentParameter[] parameters, Conversions.Unset(maximumDistance));
            return toSeq(points).Zip(toSeq(parameters), static (point, parameter) => Conversions.Present(point).Map(found => (Point: found, Parameter: parameter))).Strict();
        });

    public static IO<(Point3d Point, Vector3d Ds, Vector3d Dt, Vector3d Normal)> Evaluate(SubD subd, SubDComponentParameter parameter) =>
        IO.lift(() => Refused.Unless(
            subd.Evaluate(parameter, out Point3d point, out Vector3d ds, out Vector3d dt, out Vector3d normal),
            (Point: point, Ds: ds, Dt: dt, Normal: normal), nameof(SubD.Evaluate)));

    public static IO<(uint Count, SubDEdgeSharpness Range)> SharpEdgeCount(SubD subd) =>
        IO.lift(() => (Count: subd.SharpEdgeCount(out SubDEdgeSharpness range), Range: range));
}
