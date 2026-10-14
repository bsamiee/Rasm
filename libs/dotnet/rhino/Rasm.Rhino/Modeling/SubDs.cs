using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SubDEdit {
    public sealed record Subdivide(int Count) : SubDEdit;
    public sealed record SubdivideFaces(Seq<int> FaceIndices) : SubDEdit;
    public sealed record InterpolateSurfacePoints(Seq<Point3d> SurfacePoints) : SubDEdit;
    public sealed record SetVertexSurfacePoint(uint VertexIndex, Point3d SurfacePoint) : SubDEdit;
    public sealed record MergeAllCoplanarFaces : SubDEdit;
    public sealed record PackFaces : SubDEdit;
    public sealed record Flip : SubDEdit;
    public sealed record SetVertexTags(Seq<int> VertexIndices, SubDVertexTag Tag) : SubDEdit;
    public sealed record SetEdgeTags(Seq<int> EdgeIndices, SubDEdgeTag Tag) : SubDEdit;
    public sealed record SetEdgeSharpness(Seq<(uint Edge, SubDEdgeSharpness Sharpness)> Edges, bool PreserveSymmetry) : SubDEdit;
    public sealed record ClearEdgeSharpness : SubDEdit;
    public sealed record TransformComponents(Seq<ComponentIndex> Components, Transform Xform, SubDComponentLocation ComponentLocation) : SubDEdit;
    public sealed record DeleteComponents(Seq<ComponentIndex> ComponentIndices, bool MarkDeletedFaceEdges) : SubDEdit;
    public sealed record DissolveOrDeleteComponents(Seq<ComponentIndex> ComponentIndices) : SubDEdit;
}

[Union<Unit, bool, TypeParamRef1>(T1Name = "Completed", T2Name = "Changed", T3Name = "Count", T1IsStateless = true, MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct EditResult<TCount> where TCount : struct, System.Numerics.IBinaryInteger<TCount>;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SubDs {
    // --- [EDITS]
    public static IO<(SubD Copy, bool CopiedCache, Seq<(SubDEdit Edit, EditResult<uint> Result, uint UpdatedTags)> Edits, uint UpdatedCache)> Edit(SubD source, Seq<SubDEdit> edits, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Edit(source, copy =>
            from copiedCache in IO.lift(() => copy.CopyEvaluationCache(source))
            from result in Edited(copy, edits, tolerances)
            select (CopiedCache: copiedCache, result.Edits, result.UpdatedCache),
            nameof(SubD.UpdateSurfaceMeshCache), retain)
            .Map(static answer => (answer.Copy, answer.Result.CopiedCache, answer.Result.Edits, answer.Result.UpdatedCache));

    public static IO<(Seq<(SubDEdit Edit, EditResult<uint> Result, uint UpdatedTags)> Edits, uint UpdatedCache)> Revise(RhinoDoc doc, Guid id, Seq<SubDEdit> edits, Tolerances tolerances) =>
        RhinoObjects.ReplaceGeometry(doc, id, (SubD copy) => Edited(copy, edits, tolerances));

    public static IO<(uint UpdatedTags, uint UpdatedCache)> InterpolateSurfacePoints(RhinoDoc doc, Guid id, Seq<(uint Vertex, Point3d Point)> points) =>
        DisposalOps.Within(() => new ObjRef(doc, id), reference => DisposalOps.Within((Action<IDisposable> retain) =>
            from subd in Copies.Acquire(reference.SubD, nameof(ObjRef.SubD), retain)
            from solved in Solved(subd, points)
            from tags in IO.lift(subd.UpdateAllTagsAndSectorCoefficients)
            from cache in IO.lift(() => subd.UpdateSurfaceMeshCache(lazyUpdate: true))
            from replaced in IO.lift(() => Refused.Unless(doc.Objects.Replace(reference, subd), nameof(ObjectTable.Replace)))
            select (tags, cache),
            DisposalOps.Release), static reference => IO.lift(reference.Dispose));

    private static IO<(Seq<(SubDEdit Edit, EditResult<uint> Result, uint UpdatedTags)> Edits, uint UpdatedCache)> Edited(SubD subd, Seq<SubDEdit> edits, Tolerances tolerances) =>
        from results in edits.TraverseM(edit => Applied(subd, edit, tolerances)).As()
        from updated in IO.lift(() => subd.UpdateSurfaceMeshCache(lazyUpdate: true))
        select (results, updated);

    private static IO<(SubDEdit Edit, EditResult<uint> Result, uint UpdatedTags)> Applied(SubD subd, SubDEdit edit, Tolerances tolerances) =>
        from result in edit.Switch(
            (Subd: subd, Tolerances: tolerances),
            subdivide: static (state, of) => IO.lift(() => Refused.Unless(state.Subd.Subdivide(of.Count), EditResult<uint>.CreateCompleted(), nameof(SubD.Subdivide))),
            subdivideFaces: static (state, of) => IO.lift(() => Refused.Unless(state.Subd.Subdivide(of.FaceIndices), EditResult<uint>.CreateCompleted(), nameof(SubD.Subdivide))),
            interpolateSurfacePoints: static (state, of) => IO.lift(() => Refused.Unless(
                state.Subd.InterpolateSurfacePoints([.. of.SurfacePoints]),
                EditResult<uint>.CreateCompleted(), nameof(SubD.InterpolateSurfacePoints))),
            setVertexSurfacePoint: static (state, of) => IO.lift(() => Refused.Unless(state.Subd.SetVertexSurfacePoint(of.VertexIndex, of.SurfacePoint), EditResult<uint>.CreateCompleted(), nameof(SubD.SetVertexSurfacePoint))),
            mergeAllCoplanarFaces: static (state, _) => IO.lift(() => new EditResult<uint>(state.Subd.MergeAllCoplanarFaces(state.Tolerances.Absolute, state.Tolerances.Angle))),
            packFaces: static (state, _) => IO.lift(() => EditResult<uint>.CreateCount(state.Subd.PackFaces())),
            flip: static (state, _) => IO.lift(() => Refused.Unless(state.Subd.Flip(), EditResult<uint>.CreateCompleted(), nameof(SubD.Flip))),
            setVertexTags: static (state, of) => IO.lift(() => state.Subd.Vertices.SetVertexTags(of.VertexIndices, of.Tag))
                .Map(EditResult<uint>.CreateCompleted())
                .MapFail(static error => error.HasException<ArgumentOutOfRangeException>() ? new Refused(nameof(SubDVertexList.SetVertexTags)) : error),
            setEdgeTags: static (state, of) => IO.lift(() => state.Subd.Edges.SetEdgeTags(of.EdgeIndices, of.Tag))
                .Map(EditResult<uint>.CreateCompleted())
                .MapFail(static error => error.HasException<ArgumentOutOfRangeException>() ? new Refused(nameof(SubDEdgeList.SetEdgeTags)) : error),
            setEdgeSharpness: static (state, of) => IO.lift(() => Callbacks.Each(
                    of.Edges, (row, index) => Optional(state.Subd.Edges.Find(row.Edge)).ToFin(new RefusedElement(nameof(SubDEdgeList.Find), index))))
                .Map(edges => EditResult<uint>.CreateCount(state.Subd.SetEdgeSharpness(edges, of.Edges.Map(static row => row.Sharpness), of.PreserveSymmetry)))
                .MapFail(static error => error.HasException<ArgumentException>() ? new Refused(nameof(SubD.SetEdgeSharpness)) : error),
            clearEdgeSharpness: static (state, _) => IO.lift(() => EditResult<uint>.CreateCount(state.Subd.ClearEdgeSharpness())),
            transformComponents: static (state, of) => IO.lift(() => EditResult<uint>.CreateCount(state.Subd.TransformComponents(of.Components, of.Xform, of.ComponentLocation))),
            deleteComponents: static (state, of) => IO.lift(() => Refused.Unless(state.Subd.DeleteComponents(of.ComponentIndices, of.MarkDeletedFaceEdges), EditResult<uint>.CreateCompleted(), nameof(SubD.DeleteComponents))),
            dissolveOrDeleteComponents: static (state, of) => IO.lift(() => EditResult<uint>.CreateCount(state.Subd.DissolveOrDeleteComponents(of.ComponentIndices))))
        from cleared in when(edit is SubDEdit.TransformComponents, IO.lift(subd.ClearEvaluationCache)).As()
        from updated in IO.lift(subd.UpdateAllTagsAndSectorCoefficients)
        select (edit, result, updated);

    private static IO<Unit> Solved(SubD subd, Seq<(uint Vertex, Point3d Point)> points) =>
        DisposalOps.Within(
            () => (Interpolator: SubDSurfaceInterpolator.CreateFromVertexIdList(subd, points.Map(static row => row.Vertex), out uint count), Count: count),
            made =>
                from interpolator in IO.lift(() => Refused.Unless(made.Count == points.Count, made.Interpolator, nameof(SubD.InterpolateSurfacePoints)))
                from result in IO.lift(() => Refused.Unless(interpolator.Solve([.. points.Map(static row => row.Point)]), nameof(SubDSurfaceInterpolator.Solve)))
                select result,
            static made => IO.lift(made.Interpolator.Dispose));

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
