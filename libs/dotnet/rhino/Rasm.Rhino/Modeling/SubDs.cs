using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.Objects;
using Rhino;

namespace Rasm.RhinoCommon.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SubDEdit {
    public sealed record Subdivide(int Count) : SubDEdit;
    public sealed record SubdivideFaces(Seq<uint> Faces) : SubDEdit;
    public sealed record SetControlNetPoints(HashMap<uint, Point3d> Moved) : SubDEdit;
    public sealed record InterpolateSurfacePoints(HashMap<uint, Point3d> Moved) : SubDEdit;
    public sealed record SetVertexSurfacePoint(uint Vertex, Point3d SurfacePoint) : SubDEdit;
    public sealed record TransformComponents(Seq<ComponentIndex> Components, Transform Xform, SubDComponentLocation ComponentLocation) : SubDEdit;
    public sealed record SetVertexTags(Seq<uint> Vertices, SubDVertexTag Tag) : SubDEdit;
    public sealed record SetEdgeTags(Seq<uint> Edges, SubDEdgeTag Tag) : SubDEdit;
    public sealed record SetEdgeSharpness(Seq<(uint Edge, SubDEdgeSharpness Sharpness)> Edges, bool PreserveSymmetry) : SubDEdit;
    public sealed record ClearEdgeSharpness : SubDEdit;
    public sealed record MergeAllCoplanarFaces : SubDEdit;
    public sealed record PackFaces : SubDEdit;
    public sealed record Flip : SubDEdit;
    public sealed record DeleteComponents(Seq<ComponentIndex> Components) : SubDEdit;
    public sealed record DissolveOrDeleteComponents(Seq<ComponentIndex> Components) : SubDEdit;
    public sealed record FillHole(uint BoundaryEdge) : SubDEdit;
    public sealed record Bridge(uint EdgeA, uint EdgeB) : SubDEdit;
    public sealed record Offset(double Distance, bool Solidify) : SubDEdit;

    public IO<EditStep<SubD, uint>> Step(Tolerances tolerances, SubD subd) =>
        Switch(
            (SubD: subd, Tolerances: tolerances),
            subdivide: static (at, of) => Tagged(at.SubD, Edits.Accepted<SubD, uint>(at.SubD, at.SubD.Subdivide(of.Count))),
            subdivideFaces: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdFace, of.Faces))
                .Bind(_ => Tagged(at.SubD, Edits.Accepted<SubD, uint>(at.SubD, at.SubD.Subdivide(of.Faces.Map(static id => (int)id))))),
            setControlNetPoints: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, toSeq(of.Moved.Keys)))
                .Bind(_ => toSeq(of.Moved.AsIterable()).Map(row => at.SubD.Vertices.Find(row.Key).SetControlNetPoint(row.Value, bClearNeighborhoodCache: false)).Strict() switch {
                    var moved => Tagged(at.SubD, IO.lift(at.SubD.ClearEvaluationCache) >> Edits.Accepted<SubD, uint>(at.SubD, moved.ForAll(static held => held))),
                }),
            interpolateSurfacePoints: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, toSeq(of.Moved.Keys)))
                .Bind(_ => Tagged(at.SubD, Edits.Accepted<SubD, uint>(
                    at.SubD, at.SubD.InterpolateSurfacePoints([.. toSeq(at.SubD.Vertices).Map(vertex => of.Moved.Find(vertex.Id).IfNone(vertex.SurfacePoint))])))),
            setVertexSurfacePoint: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, [of.Vertex]))
                .Bind(_ => Tagged(at.SubD, Edits.Accepted<SubD, uint>(at.SubD, at.SubD.SetVertexSurfacePoint(of.Vertex, of.SurfacePoint)))),
            transformComponents: static (at, of) => IO.lift(() => Copies.Within(at.SubD, of.Components))
                .Bind(_ => Tagged(at.SubD, IO.lift(at.SubD.ClearEvaluationCache) >> Edits.Count(at.SubD, at.SubD.TransformComponents(of.Components, of.Xform, of.ComponentLocation)))),
            setVertexTags: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdVertex, of.Vertices))
                .Bind(_ => IO.lift(() => at.SubD.Vertices.SetVertexTags(of.Vertices.Map(static id => (int)id), of.Tag)))
                .Bind(_ => Tagged(at.SubD, Edits.Completed<SubD, uint>(at.SubD))),
            setEdgeTags: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdEdge, of.Edges))
                .Bind(_ => IO.lift(() => at.SubD.Edges.SetEdgeTags(of.Edges.Map(static id => (int)id), of.Tag)))
                .Bind(_ => Tagged(at.SubD, Edits.Completed<SubD, uint>(at.SubD))),
            setEdgeSharpness: static (at, of) => IO.lift(() => Within(at.SubD, ComponentIndexType.SubdEdge, of.Edges.Map(static row => row.Edge)))
                .Bind(_ => Tagged(at.SubD, Edits.Count(
                    at.SubD, at.SubD.SetEdgeSharpness(of.Edges.Map(row => at.SubD.Edges.Find(row.Edge)), of.Edges.Map(static row => row.Sharpness), of.PreserveSymmetry)))),
            clearEdgeSharpness: static (at, _) => Tagged(at.SubD, Edits.Count(at.SubD, at.SubD.ClearEdgeSharpness())),
            mergeAllCoplanarFaces: static (at, _) => Tagged(at.SubD, Edits.Changed<SubD, uint>(at.SubD, at.SubD.MergeAllCoplanarFaces(at.Tolerances.Absolute, at.Tolerances.Angle))),
            packFaces: static (at, _) => Tagged(at.SubD, Edits.Count(at.SubD, at.SubD.PackFaces())),
            flip: static (at, _) => Tagged(at.SubD, Edits.Accepted<SubD, uint>(at.SubD, at.SubD.Flip())),
            deleteComponents: static (at, of) => IO.lift(() => Copies.Within(at.SubD, of.Components))
                .Bind(_ => Tagged(at.SubD, Edits.Accepted<SubD, uint>(at.SubD, at.SubD.DeleteComponents(of.Components, markDeletedFaceEdges: false)))),
            dissolveOrDeleteComponents: static (at, of) => IO.lift(() => Copies.Within(at.SubD, of.Components))
                .Bind(_ => Tagged(at.SubD, Edits.Count(at.SubD, at.SubD.DissolveOrDeleteComponents(of.Components)))),
            fillHole: static (at, of) =>
                from hole in IO.lift(() => Hole(at.SubD, of.BoundaryEdge))
                from step in Tagged(at.SubD, Edits.Accepted<SubD, uint>(at.SubD, at.SubD.Faces.Add(hole) is not null))
                select step,
            bridge: static (at, of) =>
                from rails in IO.lift(() => Rails(at.SubD, of.EdgeA, of.EdgeB))
                from step in rails.Map(rail => Quad(at.SubD, rail.A, rail.B)).Strict() switch {
                    var faces => Tagged(at.SubD, IO.lift(() => RhinoFault.Unless(faces.ForAll(static face => face.IsSome))) >> Edits.Count(at.SubD, (uint)faces.Count)),
                }
                select step,
            offset: static (at, of) => Edits.Replaced<SubD, uint>(at.SubD.Offset(of.Distance, of.Solidify)));

    internal static Fin<Seq<uint>> Within(SubD subd, ComponentIndexType type, Seq<uint> ids) =>
        Copies.Within(subd, type, ids.Map(static id => (int)id)).Map(_ => ids);

    internal static IO<EditStep<SubD, uint>> Tagged(SubD subd, IO<EditStep<SubD, uint>> step) =>
        IO.lift(() => ignore(subd.UpdateAllTagsAndSectorCoefficients())) >> step;

    private static Fin<Seq<SubDEdge>> Hole(SubD subd, uint edge) =>
        from held in Within(subd, ComponentIndexType.SubdEdge, [edge])
        from boundary in Boundary(Boundaries(subd), edge)
        from closed in RhinoFault.Unless(boundary.Closed)
        select boundary.Edges;

    private static Fin<Seq<(SubDEdge A, SubDEdge B)>> Rails(SubD subd, uint edgeA, uint edgeB) =>
        from held in Within(subd, ComponentIndexType.SubdEdge, [edgeA, edgeB])
        let boundaries = Boundaries(subd)
        from first in Boundary(boundaries, edgeA)
        from second in Boundary(boundaries, edgeB)
        from apart in RhinoFault.Unless(!first.Edges.Exists(link => link.Id == edgeB))
        from even in RhinoFault.Unless(first.Edges.Count == second.Edges.Count)
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
            .ToFin(new RhinoFault($"{nameof(ComponentIndexType.SubdEdge)} {edge}", RhinoReason.Missing));

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
    public static IO<(SubD Result, Seq<(SubDEdit Edit, EditResult<uint> Result)> Results)> Edit(Tolerances tolerances, SubD source, Seq<SubDEdit> edits) =>
        Edits.Apply(source, edits, (subd, edit) => edit.Step(tolerances, subd), Settled);

    public static IO<Seq<(SubDEdit Edit, EditResult<uint> Result)>> Revise(Tolerances tolerances, RhinoDoc doc, Guid id, Seq<SubDEdit> edits) =>
        RhinoObjects.ReplaceGeometry(doc, id, (SubD target) => Edit(tolerances, target, edits));

    public static IO<Unit> InterpolateVertices(RhinoDoc doc, Guid id, HashMap<uint, Point3d> moved) =>
        RhinoObjects.ReplaceGeometry(doc, id, (SubD target) =>
            IO.lift(() => SubDEdit.Within(target, ComponentIndexType.SubdVertex, toSeq(moved.Keys)))
                .Bind(_ => toSeq(moved.AsIterable()) switch {
                    var rows => SubDEdit.Tagged(target, Edits.Accepted<SubD, uint>(target, target.InterpolateSurfacePoints([.. rows.Map(static row => row.Key)], [.. rows.Map(static row => row.Value)]))),
                })
            >> Settled(target)
            >> IO.pure((target, unit)));

    private static IO<Unit> Settled(SubD subd) => IO.lift(() => ignore(subd.UpdateSurfaceMeshCache(lazyUpdate: true)));

    // --- [CHAINS]
    public static IO<Seq<SubDChain>> Chains(SubD subd, ChainSeed seed) =>
        seed.Switch(
            subd,
            sorted: static (at, of) => IO.lift(() => SubDEdit.Within(at, ComponentIndexType.SubdEdge, of.Edges))
                .Bind(_ => toSeq(SubDEdgeChain.SortEdgesIntoEdgeChains(at, of.Edges.Map(edge => at.Edges.Find(edge))))
                    .TraverseM(links => Products.Made(Linked(at, links)).Bind(static chain => Read(chain))).As()),
            grown: static (at, of) => IO.lift(() => SubDEdit.Within(at, ComponentIndexType.SubdEdge, [of.Edge]))
                .Bind(_ =>
                    from chain in Products.Made(new SubDEdgeChain(at, at.Edges.Find(of.Edge)))
                    from grown in IO.lift(() => chain.AddAllNeighbors(of.Direction, of.Type))
                    from read in Read(chain)
                    select Seq(read)));

    private static SubDEdgeChain Linked(SubD subd, SubDEdge[] links) {
        SubDEdgeChain chain = new(subd, links[0]);
        _ = toSeq(links).Tail.Iter(link => _ = chain.AddEdge(link));
        return chain;
    }

    private static IO<SubDChain> Read(SubDEdgeChain chain) =>
        from curve in Products.Held(chain.ToNurbsCurve())
        from loft in Products.Held(chain.ToLoftCurve())
        select new SubDChain(toSeq(chain.Edges).Map(static edge => (Edge: edge.Id, Reversed: edge.ComponentDirection)).Strict(), chain.IsClosedLoop, curve, loft);

    // --- [READS]
    public static IO<Seq<Option<(Point3d Point, SubDComponentParameter Parameter)>>> ClosestPoints(SubD subd, Seq<Point3d> testPoints, Option<double> maximumDistance) =>
        IO.lift(() => {
            _ = subd.ClosestPoints(testPoints, out Point3d[] points, out SubDComponentParameter[] parameters, Conversions.Unset(maximumDistance));
            return toSeq(points).Zip(toSeq(parameters), static (point, parameter) => Conversions.Present(point).Map(found => (Point: found, Parameter: parameter))).Strict();
        });

    public static IO<(Point3d Point, Vector3d Ds, Vector3d Dt, Vector3d Normal)> Evaluate(SubD subd, SubDComponentParameter parameter) =>
        IO.lift(() => RhinoFault.Unless(subd.Evaluate(parameter, out Point3d point, out Vector3d ds, out Vector3d dt, out Vector3d normal))
            .Map(_ => (Point: point, Ds: ds, Dt: dt, Normal: normal)));

    public static IO<(uint Count, SubDEdgeSharpness Range)> SharpEdgeCount(SubD subd) =>
        IO.lift(() => (Count: subd.SharpEdgeCount(out SubDEdgeSharpness range), Range: range));
}
