using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.Modeling.Surfaces;
using Rasm.RhinoCommon.Render.Content;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Modeling.Meshes;

// --- [TYPES] ---------------------------------------------------------------------------
public enum QuadSplit { Default = 0, ShortDiagonal = 1, LongDiagonal = 2, MinimumArea = 3, MaximumArea = 4, MinimumNormalAngle = 5, MaximumNormalAngle = 6 }

// --- [MODELS] --------------------------------------------------------------------------
[Union<double, Point3d>(T1Name = "Parameter", T2Name = "Point", MapMethods = SwitchMapMethodsGeneration.None)]
public readonly partial struct EdgeSplit;

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record VertexMask {
    public sealed record All : VertexMask;

    public sealed record Naked : VertexMask;

    public sealed record Chosen(Seq<int> Vertices) : VertexMask;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FacePatch {
    public sealed record VertexEdge(ComponentIndex Vertex, int Edge) : FacePatch;

    public sealed record Edges(int EdgeA, int EdgeB) : FacePatch;

    public sealed record Vertices(ComponentIndex A, ComponentIndex B, ComponentIndex C) : FacePatch;

    internal Fin<Seq<ComponentIndex>> Components(Mesh mesh) =>
        Switch(
            mesh,
            vertexEdge: static (held, of) => Held(held, Seq(of.Vertex), Seq(of.Edge)),
            edges: static (held, of) => Held(held, Seq<ComponentIndex>(), Seq(of.EdgeA, of.EdgeB)),
            vertices: static (held, of) => Held(held, Seq(of.A, of.B, of.C), Seq<int>()));

    private static Fin<Seq<ComponentIndex>> Held(Mesh mesh, Seq<ComponentIndex> vertices, Seq<int> edges) =>
        from corners in Callbacks.Each(vertices, static (vertex, at) => vertex.ComponentIndexType is ComponentIndexType.MeshVertex or ComponentIndexType.MeshTopologyVertex
            ? Fin.Succ(vertex)
            : new RhinoFault(vertex.ComponentIndexType.ToString(), RhinoReason.Mismatched, at))
        from held in Copies.Within(mesh, corners + edges.Map(static edge => new ComponentIndex(ComponentIndexType.MeshTopologyEdge, edge)))
        select held;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshEdit {
    public sealed record Reduce(ReduceMeshParameters Parameters, bool Threaded) : MeshEdit;
    public sealed record Weld(double AngleToleranceRadians, bool PreserveSurfaceParameters) : MeshEdit;
    public sealed record Unweld(double AngleToleranceRadians, bool ModifyNormals) : MeshEdit;
    public sealed record UnweldEdge(Seq<int> EdgeIndices, bool ModifyNormals) : MeshEdit;
    public sealed record UnweldVertices(Seq<int> TopologyVertices, bool ModifyNormals) : MeshEdit;
    public sealed record CombineIdentical(bool IgnoreNormals, bool IgnoreAdditional) : MeshEdit;
    public sealed record Align(double Distance, bool Average, VertexMask Mask) : MeshEdit;
    public sealed record CullUnused : MeshEdit;
    public sealed record CollapseEdge(int Edge) : MeshEdit;
    public sealed record SplitEdge(int Edge, EdgeSplit At) : MeshEdit;
    public sealed record SwapEdge(int Edge) : MeshEdit;
    public sealed record MergeAdjacentFaces(int Edge) : MeshEdit;
    public sealed record PatchSingleFace(FacePatch Patch) : MeshEdit;
    public sealed record DeleteFaces(Seq<int> Faces, bool CompactVertices) : MeshEdit;
    public sealed record ConvertQuadsToTriangles : MeshEdit;
    public sealed record ConvertTrianglesToQuads(double AngleToleranceRadians, double MinimumDiagonalLengthRatio) : MeshEdit;
    public sealed record ConvertNonPlanarQuadsToTriangles(Option<double> PlanarTolerance, QuadSplit SplitMethod) : MeshEdit;
    public sealed record AddPlanarNgons(double PlanarTolerance, int MinimumNgonVertexCount, int MinimumNgonFaceCount, bool AllowHoles) : MeshEdit;
    public sealed record RemoveAllNgons : MeshEdit;
    public sealed record HealNakedEdges(double Distance) : MeshEdit;
    public sealed record FillHoles : MeshEdit;
    public sealed record FillHole(int TopologyEdgeIndex) : MeshEdit;
    public sealed record MatchEdges(double Distance, bool Rachet) : MeshEdit;
    public sealed record MergeAllCoplanarFaces : MeshEdit;
    public sealed record CullDegenerateFaces : MeshEdit;
    public sealed record RemoveZeroAreaFaces : MeshEdit;
    public sealed record Smooth(SmoothOptions Options, int NumSteps, Option<Seq<int>> VertexIndices) : MeshEdit;
    public sealed record CollapseFacesByEdgeLength(bool GreaterThan, double EdgeLength) : MeshEdit;
    public sealed record CollapseFacesByArea(double LessThanArea, double GreaterThanArea) : MeshEdit;
    public sealed record CollapseFacesByByAspectRatio(double AspectRatio) : MeshEdit;
    public sealed record Subdivide(Option<Seq<int>> FaceIndices) : MeshEdit;
    public sealed record ComputeNormals : MeshEdit;
    public sealed record ComputeFaceNormals : MeshEdit;
    public sealed record UnitizeNormals : MeshEdit;
    public sealed record UnitizeFaceNormals : MeshEdit;
    public sealed record RebuildNormals : MeshEdit;
    public sealed record UnifyNormals : MeshEdit;
    public sealed record Flip(bool VertexNormals, bool FaceNormals, bool FaceOrientation, bool NgonsBoundaryDirection) : MeshEdit;
    public sealed record Map(MappingPrimitive Mapping, Transform Xform, bool SeamCheck) : MeshEdit;
    public sealed record NormalizeTextureCoordinates : MeshEdit;
    public sealed record TransposeTextureCoordinates : MeshEdit;
    public sealed record ReverseTextureCoordinates(SurfaceDirection Direction) : MeshEdit;
    public sealed record ClearTextureData : MeshEdit;
    public sealed record ClearSurfaceData : MeshEdit;
    public sealed record SurfaceParametersFromTexture : MeshEdit;
    public sealed record EvaluateOn(Surface Surface) : MeshEdit;
    public sealed record Compact : MeshEdit;
    public sealed record Rebuild(bool PreserveTextureCoordinates, bool PreserveVertexColors) : MeshEdit;
    public sealed record Unwelded : MeshEdit;
    public sealed record WithShutLining(bool Faceted, Seq<ShutLiningCurveInfo> Curves) : MeshEdit;
    public sealed record WithDisplacement(MeshDisplacementInfo Displacement) : MeshEdit;
    public sealed record WithEdgeSoftening(double Radius, bool Chamfer, bool Faceted, bool Force, double AngleThresholdDegrees) : MeshEdit;

    public IO<EditStep<Mesh, int>> Step(Tolerances tolerances, Mesh mesh) =>
        Switch(
            (Mesh: mesh, Tolerances: tolerances),
            reduce: static (at, of) =>
                from token in cancelToken
                from step in Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Reduce(MeshMapper.Reduction((of.Parameters, token)), of.Threaded))
                select step,
            weld: static (at, of) => IO.lift(() => at.Mesh.Weld(of.AngleToleranceRadians, of.PreserveSurfaceParameters)) >> Edits.Completed<Mesh, int>(at.Mesh),
            unweld: static (at, of) => IO.lift(() => at.Mesh.Unweld(of.AngleToleranceRadians, of.ModifyNormals)) >> Edits.Completed<Mesh, int>(at.Mesh),
            unweldEdge: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, of.EdgeIndices))
                .Bind(edges => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.UnweldEdge(edges, of.ModifyNormals))),
            unweldVertices: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyVertex, of.TopologyVertices))
                .Bind(vertices => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.UnweldVertices(vertices, of.ModifyNormals))),
            combineIdentical: static (at, of) => Edits.Changed<Mesh, int>(at.Mesh, at.Mesh.Vertices.CombineIdentical(of.IgnoreNormals, of.IgnoreAdditional)),
            align: static (at, of) => of.Mask.Switch(
                (at.Mesh, of.Distance, of.Average),
                all: static (held, _) => Counted(held.Mesh, held.Mesh.Vertices.Align(held.Distance, held.Average, whichVertices: null)),
                naked: static (held, _) => Counted(held.Mesh, held.Mesh.Vertices.Align(held.Distance, onlyNaked: true, held.Average)),
                chosen: static (held, chosen) => IO.lift(() => Copies.Within(held.Mesh, ComponentIndexType.MeshVertex, chosen.Vertices))
                    .Bind(vertices => Counted(held.Mesh, held.Mesh.Vertices.Align(held.Distance, held.Average, toSeq(Range(0, held.Mesh.Vertices.Count)).Map(toHashSet(vertices).Contains))))),
            cullUnused: static (at, _) => Edits.Count(at.Mesh, at.Mesh.Vertices.CullUnused()),
            collapseEdge: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, [of.Edge]))
                .Bind(_ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TopologyEdges.CollapseEdge(of.Edge))),
            splitEdge: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, [of.Edge]))
                .Bind(_ => Edits.Accepted<Mesh, int>(at.Mesh, of.At.Switch(
                    (at.Mesh, of.Edge),
                    parameter: static (held, t) => held.Mesh.TopologyEdges.SplitEdge(held.Edge, t),
                    point: static (held, point) => held.Mesh.TopologyEdges.SplitEdge(held.Edge, point)))),
            swapEdge: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, [of.Edge]))
                .Bind(_ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TopologyEdges.SwapEdge(of.Edge))),
            mergeAdjacentFaces: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, [of.Edge]))
                .Bind(_ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Faces.MergeAdjacentFaces(of.Edge))),
            patchSingleFace: static (at, of) => IO.lift(() => of.Patch.Components(at.Mesh))
                .Bind(components => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.PatchSingleFace(components))),
            deleteFaces: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshFace, of.Faces))
                .Bind(faces => IO.lift(() => RhinoFault.Unless(at.Mesh.Faces.DeleteFaces(faces, of.CompactVertices) == faces.Count)) >> Edits.Count(at.Mesh, faces.Count)),
            convertQuadsToTriangles: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Faces.ConvertQuadsToTriangles()),
            convertTrianglesToQuads: static (at, of) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Faces.ConvertTrianglesToQuads(of.AngleToleranceRadians, of.MinimumDiagonalLengthRatio)),
            convertNonPlanarQuadsToTriangles: static (at, of) =>
                Edits.Count(at.Mesh, at.Mesh.Faces.ConvertNonPlanarQuadsToTriangles(Conversions.Unset(of.PlanarTolerance), at.Tolerances.Angle, (int)of.SplitMethod)),
            addPlanarNgons: static (at, of) => Edits.Count(at.Mesh, at.Mesh.Ngons.AddPlanarNgons(of.PlanarTolerance, of.MinimumNgonVertexCount, of.MinimumNgonFaceCount, of.AllowHoles)),
            removeAllNgons: static (at, _) => Edits.Count(at.Mesh, at.Mesh.Ngons.RemoveAllNgons()),
            healNakedEdges: static (at, of) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.HealNakedEdges(of.Distance)),
            fillHoles: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FillHoles()),
            fillHole: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, [of.TopologyEdgeIndex]))
                .Bind(_ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FillHole(of.TopologyEdgeIndex))),
            matchEdges: static (at, of) => Edits.Changed<Mesh, int>(at.Mesh, at.Mesh.MatchEdges(of.Distance, of.Rachet)),
            mergeAllCoplanarFaces: static (at, _) => Edits.Changed<Mesh, int>(at.Mesh, at.Mesh.MergeAllCoplanarFaces(at.Tolerances.Absolute, at.Tolerances.Angle)),
            cullDegenerateFaces: static (at, _) => Edits.Count(at.Mesh, at.Mesh.Faces.CullDegenerateFaces()),
            removeZeroAreaFaces: static (at, _) => IO.lift(() => {
                int fixedFaceCount = 0;
                int removed = at.Mesh.Faces.RemoveZeroAreaFaces(ref fixedFaceCount);
                return removed + fixedFaceCount;
            }).Bind(count => Edits.Count(at.Mesh, count)),
            smooth: static (at, of) => of.VertexIndices.Match(
                Some: indices => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshVertex, indices))
                    .Bind(vertices => Edits.Accepted<Mesh, int>(at.Mesh,
                        at.Mesh.Smooth(vertices, of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, of.Options.Coordinates.System, of.Options.Coordinates.Plane))),
                None: () => Edits.Accepted<Mesh, int>(at.Mesh,
                    at.Mesh.Smooth(of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, of.Options.Coordinates.System, of.Options.Coordinates.Plane))),
            collapseFacesByEdgeLength: static (at, of) => Counted(at.Mesh, at.Mesh.CollapseFacesByEdgeLength(of.GreaterThan, of.EdgeLength)),
            collapseFacesByArea: static (at, of) => Edits.Count(at.Mesh, at.Mesh.CollapseFacesByArea(of.LessThanArea, of.GreaterThanArea)),
            collapseFacesByByAspectRatio: static (at, of) => Edits.Count(at.Mesh, at.Mesh.CollapseFacesByByAspectRatio(of.AspectRatio)),
            subdivide: static (at, of) => of.FaceIndices.Match(
                Some: indices => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshFace, indices))
                    .Bind(faces => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Subdivide(faces))),
                None: () => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Subdivide())),
            computeNormals: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Normals.ComputeNormals()),
            computeFaceNormals: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FaceNormals.ComputeFaceNormals()),
            unitizeNormals: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Normals.UnitizeNormals()),
            unitizeFaceNormals: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FaceNormals.UnitizeFaceNormals()),
            rebuildNormals: static (at, _) => IO.lift(at.Mesh.RebuildNormals) >> Edits.Completed<Mesh, int>(at.Mesh),
            unifyNormals: static (at, _) => Edits.Count(at.Mesh, at.Mesh.UnifyNormals(countOnly: false)),
            flip: static (at, of) => IO.lift(() => at.Mesh.Flip(of.VertexNormals, of.FaceNormals, of.FaceOrientation, of.NgonsBoundaryDirection)) >> Edits.Completed<Mesh, int>(at.Mesh),
            map: static (at, of) => Mappings.WithMapping(of.Mapping, mapping =>
                IO.lift(() => at.Mesh.SetTextureCoordinates(mapping, of.Xform, lazy: false, of.SeamCheck)) >> Edits.Completed<Mesh, int>(at.Mesh)),
            normalizeTextureCoordinates: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TextureCoordinates.NormalizeTextureCoordinates()),
            transposeTextureCoordinates: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TextureCoordinates.TransposeTextureCoordinates()),
            reverseTextureCoordinates: static (at, of) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TextureCoordinates.ReverseTextureCoordinates((int)of.Direction)),
            clearTextureData: static (at, _) => IO.lift(at.Mesh.ClearTextureData) >> Edits.Completed<Mesh, int>(at.Mesh),
            clearSurfaceData: static (at, _) => IO.lift(at.Mesh.ClearSurfaceData) >> Edits.Completed<Mesh, int>(at.Mesh),
            surfaceParametersFromTexture: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.SetSurfaceParametersFromTextureCoordinates()),
            evaluateOn: static (at, of) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.EvaluateMeshGeometry(of.Surface)),
            compact: static (at, _) => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Compact()),
            rebuild: static (at, of) => Edits.Replaced<Mesh, int>(Mesh.RebuildMesh(at.Mesh, of.PreserveTextureCoordinates, of.PreserveVertexColors)),
            unwelded: static (at, _) => Edits.Replaced<Mesh, int>(Mesh.CreateUnweldedMesh(at.Mesh)),
            withShutLining: static (at, of) => Thrown(() => Edits.Replaced<Mesh, int>(at.Mesh.WithShutLining(of.Faceted, at.Tolerances.Absolute, of.Curves))),
            withDisplacement: static (at, of) => Thrown(() => Edits.Replaced<Mesh, int>(at.Mesh.WithDisplacement(of.Displacement))),
            withEdgeSoftening: static (at, of) => Thrown(() => Edits.Replaced<Mesh, int>(at.Mesh.WithEdgeSoftening(of.Radius, of.Chamfer, of.Faceted, of.Force, of.AngleThresholdDegrees))));

    // --- [STEPS]
    private static IO<EditStep<Mesh, int>> Counted(Mesh mesh, int count, [CallerArgumentExpression(nameof(count))] string call = "") =>
        IO.lift(() => RhinoFault.Unless(count >= 0, call: call)) >> Edits.Count(mesh, count, call);

    private static IO<EditStep<Mesh, int>> Thrown(Func<IO<EditStep<Mesh, int>>> step, [CallerArgumentExpression(nameof(step))] string call = "") =>
        unitIO.Bind(_ => step()).Catch(static error => error.HasException<InvalidOperationException>(), error => IO.fail<EditStep<Mesh, int>>(new RhinoFault(call, RhinoReason.Declined, Inner: error)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MeshMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapNestedProperties(nameof(MeshEdit.Reduce.Parameters))]
    internal static partial ReduceMeshParameters Reduction((ReduceMeshParameters Parameters, CancellationToken CancelToken) source);
}

public static class MeshEdits {
    // --- [EDITS]
    public static IO<(Mesh Result, Seq<(MeshEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Mesh source, Seq<MeshEdit> edits) =>
        Edits.Apply(source, edits, (mesh, edit) => edit.Step(tolerances, mesh), static _ => IO.pure(unit));

    // --- [DERIVATIONS]
    public static IO<(Mesh Mesh, Seq<int> WallFaces)> Offset(Mesh source, double distance, bool solidify, Option<Vector3d> direction) =>
        Products.Made(source.Offset(distance, solidify, Conversions.Unset(direction), out List<int> walls)).Map(mesh => (Mesh: mesh, WallFaces: toSeq(walls)));

    public static IO<(Mesh Mesh, Seq<ComponentIndex> Components, Seq<int> WallFaces)> Extrude(Mesh source, Seq<ComponentIndex> components, Action<MeshExtruder> configure) =>
        IO.lift(() => Copies.Within(source, components)).Bind(held =>
            from extruder in use(() => new MeshExtruder(source, held))
            from configured in IO.lift(() => configure(extruder))
            from extruded in IO.lift(() => RhinoFault.Unless(extruder.ExtrudedMesh(out Mesh mesh, out List<ComponentIndex> map)).Map(_ => (Mesh: mesh, Map: map)))
            from made in Products.Made(extruded.Mesh)
            select (Mesh: made, Components: toSeq(extruded.Map), WallFaces: toSeq(extruder.GetWallFaces())));
}
