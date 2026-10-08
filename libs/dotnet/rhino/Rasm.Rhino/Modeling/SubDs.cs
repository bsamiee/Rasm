using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record SubDEdit {
    public sealed record Subdivide(int Count) : SubDEdit;

    public sealed record SubdivideFaces(Seq<int> FaceIndices) : SubDEdit;

    public sealed record InterpolateSurfacePoints(Seq<(uint Vertex, Point3d Point)> Points) : SubDEdit;

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SubDs {
    // --- [EDITS]
    public static IO<SubD> Edit(SubD source, Seq<SubDEdit> edits, Tolerances tolerances) =>
        Copies.Edit(
            source,
            copy => {
                _ = copy.CopyEvaluationCache(source);
                return Edited(copy, edits, tolerances);
            },
            nameof(SubD.UpdateSurfaceMeshCache));

    public static IO<Unit> Revise(RhinoDoc doc, Guid id, Seq<SubDEdit> edits, Tolerances tolerances) =>
        RhinoObjects.ReplaceGeometry<SubD>(doc, id, copy => IO.lift(() => Edited(copy, edits, tolerances)));

    private static Fin<Unit> Edited(SubD subd, Seq<SubDEdit> edits, Tolerances tolerances) =>
        edits.TraverseM(edit => Applied(subd, edit, tolerances).Map(_ => ignore(subd.UpdateAllTagsAndSectorCoefficients())))
            .As()
            .Map(_ => ignore(subd.UpdateSurfaceMeshCache(lazyUpdate: true)));

    private static Fin<Unit> Applied(SubD subd, SubDEdit edit, Tolerances tolerances) =>
        edit.Switch(
            (Subd: subd, Tolerances: tolerances),
            subdivide: static (state, of) => Refused.Unless(state.Subd.Subdivide(of.Count), nameof(SubD.Subdivide)),
            subdivideFaces: static (state, of) => Refused.Unless(state.Subd.Subdivide(of.FaceIndices), nameof(SubD.Subdivide)),
            interpolateSurfacePoints: static (state, of) => Refused.Unless(
                state.Subd.InterpolateSurfacePoints([.. of.Points.Map(static row => row.Vertex)], [.. of.Points.Map(static row => row.Point)]),
                nameof(SubD.InterpolateSurfacePoints)),
            setVertexSurfacePoint: static (state, of) => Refused.Unless(state.Subd.SetVertexSurfacePoint(of.VertexIndex, of.SurfacePoint), nameof(SubD.SetVertexSurfacePoint)),
            mergeAllCoplanarFaces: static (state, _) => Fin.Succ(ignore(state.Subd.MergeAllCoplanarFaces(state.Tolerances.Absolute, state.Tolerances.Angle))),
            packFaces: static (state, _) => Fin.Succ(ignore(state.Subd.PackFaces())),
            flip: static (state, _) => Refused.Unless(state.Subd.Flip(), nameof(SubD.Flip)),
            setVertexTags: static (state, of) => Callbacks.Thrown<ArgumentOutOfRangeException, Unit>(
                fun(() => state.Subd.Vertices.SetVertexTags(of.VertexIndices, of.Tag)),
                nameof(SubDVertexList.SetVertexTags)),
            setEdgeTags: static (state, of) => Callbacks.Thrown<ArgumentOutOfRangeException, Unit>(
                fun(() => state.Subd.Edges.SetEdgeTags(of.EdgeIndices, of.Tag)),
                nameof(SubDEdgeList.SetEdgeTags)),
            setEdgeSharpness: static (state, of) =>
                from edges in Callbacks.Each(
                    of.Edges,
                    (row, index) => state.Subd.Edges.Find(row.Edge) is { } edge ? Fin.Succ(edge) : new RefusedElement(nameof(SubDEdgeList.Find), index))
                from changed in Callbacks.Thrown<ArgumentException, uint>(
                    () => state.Subd.SetEdgeSharpness(edges, of.Edges.Map(static row => row.Sharpness), of.PreserveSymmetry),
                    nameof(SubD.SetEdgeSharpness))
                select unit,
            clearEdgeSharpness: static (state, _) => Fin.Succ(ignore(state.Subd.ClearEdgeSharpness())),
            transformComponents: static (state, of) => Fin.Succ(ignore(state.Subd.TransformComponents(of.Components, of.Xform, of.ComponentLocation))),
            deleteComponents: static (state, of) => Refused.Unless(state.Subd.DeleteComponents(of.ComponentIndices, of.MarkDeletedFaceEdges), nameof(SubD.DeleteComponents)),
            dissolveOrDeleteComponents: static (state, of) => Fin.Succ(ignore(state.Subd.DissolveOrDeleteComponents(of.ComponentIndices))));

    // --- [READS]
    public static IO<Option<(Point3d Point, SubDComponentParameter Parameter)>> ClosestPoint(SubD subd, Point3d testPoint, Option<double> maximumDistance) =>
        IO.lift(() => Callbacks.Found(
            subd.ClosestPoint(testPoint, out Point3d point, out SubDComponentParameter parameter, Conversions.Unset(maximumDistance)),
            (Point: point, Parameter: parameter)));

    public static IO<Seq<Option<(Point3d Point, SubDComponentParameter Parameter)>>> ClosestPoints(SubD subd, Seq<Point3d> testPoints, Option<double> maximumDistance) =>
        IO.lift(() => {
            _ = subd.ClosestPoints(testPoints, out Point3d[] points, out SubDComponentParameter[] parameters, Conversions.Unset(maximumDistance));
            return toSeq(points)
                .Zip(toSeq(parameters), static (point, parameter) => Conversions.Present(point).Map(found => (Point: found, Parameter: parameter)))
                .Strict();
        });

    public static IO<(Point3d Point, Vector3d Ds, Vector3d Dt, Vector3d Normal)> Evaluate(SubD subd, SubDComponentParameter parameter) =>
        IO.lift(() => Refused.Unless(
            subd.Evaluate(parameter, out Point3d point, out Vector3d ds, out Vector3d dt, out Vector3d normal),
            (Point: point, Ds: ds, Dt: dt, Normal: normal),
            nameof(SubD.Evaluate)));

    public static IO<(uint Count, SubDEdgeSharpness Range)> SharpEdgeCount(SubD subd) =>
        IO.lift(() => (Count: subd.SharpEdgeCount(out SubDEdgeSharpness range), Range: range));
}
