using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Surfaces;
using Rasm.Rhino.Render.Content;
using Rhino.Geometry.Collections;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

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
            : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{vertex.ComponentIndexType} at element {at} is no {ComponentIndexType.MeshVertex} or {ComponentIndexType.MeshTopologyVertex}")))
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

    public IO<EditStep<Mesh, int>> Step(Tolerances tolerances, Mesh mesh, Action<IDisposable> retain) =>
        Switch(
            (Mesh: mesh, Tolerances: tolerances, Retain: retain),
            reduce: static (at, of) => Copies.Acquire(nameof(Mesh.Reduce), token =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Reduce(MeshMapper.Reduction((of.Parameters, token)), of.Threaded)), at.Retain),
            weld: static (at, of) => Copies.Acquire(nameof(Mesh.Weld), _ => {
                at.Mesh.Weld(of.AngleToleranceRadians, of.PreserveSurfaceParameters);
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain),
            unweld: static (at, of) => Copies.Acquire(nameof(Mesh.Unweld), _ => {
                at.Mesh.Unweld(of.AngleToleranceRadians, of.ModifyNormals);
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain),
            unweldEdge: static (at, of) =>
                IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyEdge, of.EdgeIndices))
                    .Bind(edges => Copies.Acquire(nameof(Mesh.UnweldEdge), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.UnweldEdge(edges, of.ModifyNormals)), at.Retain)),
            unweldVertices: static (at, of) =>
                IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshTopologyVertex, of.TopologyVertices))
                    .Bind(vertices => Copies.Acquire(nameof(Mesh.UnweldVertices), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.UnweldVertices(vertices, of.ModifyNormals)), at.Retain)),
            combineIdentical: static (at, of) => Copies.Acquire(nameof(MeshVertexList.CombineIdentical), _ =>
                Edits.Changed<Mesh, int>(at.Mesh, at.Mesh.Vertices.CombineIdentical(of.IgnoreNormals, of.IgnoreAdditional)), at.Retain),
            align: static (at, of) => of.Mask.Switch(
                (at.Mesh, at.Retain, of.Distance, of.Average),
                all: static (held, _) => Copies.Acquire(nameof(MeshVertexList.Align), _ =>
                    Counted(held.Mesh, held.Mesh.Vertices.Align(held.Distance, held.Average, whichVertices: null)), held.Retain),
                naked: static (held, _) => Copies.Acquire(nameof(MeshVertexList.Align), _ =>
                    Counted(held.Mesh, held.Mesh.Vertices.Align(held.Distance, onlyNaked: true, held.Average)), held.Retain),
                chosen: static (held, chosen) => IO.lift(() => Copies.Within(held.Mesh, ComponentIndexType.MeshVertex, chosen.Vertices))
                    .Bind(vertices => Copies.Acquire(nameof(MeshVertexList.Align), _ =>
                        Counted(held.Mesh, held.Mesh.Vertices.Align(held.Distance, held.Average, toSeq(Range(0, held.Mesh.Vertices.Count)).Map(toHashSet(vertices).Contains))), held.Retain))),
            cullUnused: static (at, _) => Copies.Acquire(nameof(MeshVertexList.CullUnused), _ => Edits.Count(at.Mesh, at.Mesh.Vertices.CullUnused()), at.Retain),
            collapseEdge: static (at, of) => TopologyEdge(at.Mesh, of.Edge, at.Retain, nameof(MeshTopologyEdgeList.CollapseEdge), static (mesh, edge) => mesh.TopologyEdges.CollapseEdge(edge)),
            splitEdge: static (at, of) => TopologyEdge(at.Mesh, of.Edge, at.Retain, nameof(MeshTopologyEdgeList.SplitEdge), (mesh, edge) => of.At.Switch(
                (Mesh: mesh, Edge: edge),
                parameter: static (held, t) => held.Mesh.TopologyEdges.SplitEdge(held.Edge, t),
                point: static (held, point) => held.Mesh.TopologyEdges.SplitEdge(held.Edge, point))),
            swapEdge: static (at, of) => TopologyEdge(at.Mesh, of.Edge, at.Retain, nameof(MeshTopologyEdgeList.SwapEdge), static (mesh, edge) => mesh.TopologyEdges.SwapEdge(edge)),
            mergeAdjacentFaces: static (at, of) => TopologyEdge(at.Mesh, of.Edge, at.Retain, nameof(MeshFaceList.MergeAdjacentFaces), static (mesh, edge) => mesh.Faces.MergeAdjacentFaces(edge)),
            patchSingleFace: static (at, of) => IO.lift(() => of.Patch.Components(at.Mesh))
                .Bind(components => Copies.Acquire(nameof(Mesh.PatchSingleFace), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.PatchSingleFace(components)), at.Retain)),
            deleteFaces: static (at, of) => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshFace, of.Faces))
                .Bind(faces => Copies.Acquire(nameof(MeshFaceList.DeleteFaces), _ =>
                    (Answer.Accepted(at.Mesh.Faces.DeleteFaces(faces, of.CompactVertices) == faces.Count, $"deleted fewer than the {faces.Count} faces named"), Edits.Count(at.Mesh, faces.Count))
                        .Apply(static (_, step) => step).As(), at.Retain)),
            convertQuadsToTriangles: static (at, _) => Copies.Acquire(nameof(MeshFaceList.ConvertQuadsToTriangles), _ =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Faces.ConvertQuadsToTriangles()), at.Retain),
            convertTrianglesToQuads: static (at, of) => Copies.Acquire(nameof(MeshFaceList.ConvertTrianglesToQuads), _ =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Faces.ConvertTrianglesToQuads(of.AngleToleranceRadians, of.MinimumDiagonalLengthRatio)), at.Retain),
            convertNonPlanarQuadsToTriangles: static (at, of) => Copies.Acquire(nameof(MeshFaceList.ConvertNonPlanarQuadsToTriangles), _ =>
                Edits.Count(at.Mesh, at.Mesh.Faces.ConvertNonPlanarQuadsToTriangles(Conversions.Unset(of.PlanarTolerance), at.Tolerances.Angle, (int)of.SplitMethod)), at.Retain),
            addPlanarNgons: static (at, of) => Copies.Acquire(nameof(MeshNgonList.AddPlanarNgons), _ =>
                Edits.Count(at.Mesh, at.Mesh.Ngons.AddPlanarNgons(of.PlanarTolerance, of.MinimumNgonVertexCount, of.MinimumNgonFaceCount, of.AllowHoles)), at.Retain),
            removeAllNgons: static (at, _) => Copies.Acquire(nameof(MeshNgonList.RemoveAllNgons), _ => Edits.Count(at.Mesh, at.Mesh.Ngons.RemoveAllNgons()), at.Retain),
            healNakedEdges: static (at, of) => Copies.Acquire(nameof(Mesh.HealNakedEdges), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.HealNakedEdges(of.Distance)), at.Retain),
            fillHoles: static (at, _) => Copies.Acquire(nameof(Mesh.FillHoles), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FillHoles()), at.Retain),
            fillHole: static (at, of) => TopologyEdge(at.Mesh, of.TopologyEdgeIndex, at.Retain, nameof(Mesh.FillHole), static (mesh, edge) => mesh.FillHole(edge)),
            matchEdges: static (at, of) => Copies.Acquire(nameof(Mesh.MatchEdges), _ => Edits.Changed<Mesh, int>(at.Mesh, at.Mesh.MatchEdges(of.Distance, of.Rachet)), at.Retain),
            mergeAllCoplanarFaces: static (at, _) => Copies.Acquire(nameof(Mesh.MergeAllCoplanarFaces), _ =>
                Edits.Changed<Mesh, int>(at.Mesh, at.Mesh.MergeAllCoplanarFaces(at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            cullDegenerateFaces: static (at, _) => Copies.Acquire(nameof(MeshFaceList.CullDegenerateFaces), _ => Edits.Count(at.Mesh, at.Mesh.Faces.CullDegenerateFaces()), at.Retain),
            removeZeroAreaFaces: static (at, _) => Copies.Acquire(nameof(MeshFaceList.RemoveZeroAreaFaces), _ => {
                int fixedFaceCount = 0;
                int removed = at.Mesh.Faces.RemoveZeroAreaFaces(ref fixedFaceCount);
                return Edits.Count(at.Mesh, removed + fixedFaceCount);
            }, at.Retain),
            smooth: static (at, of) => of.VertexIndices.Match(
                Some: indices => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshVertex, indices))
                    .Bind(vertices => Copies.Acquire(nameof(Mesh.Smooth), _ => Edits.Accepted<Mesh, int>(at.Mesh,
                        at.Mesh.Smooth(vertices, of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, of.Options.Coordinates.System, of.Options.Coordinates.Plane)), at.Retain)),
                None: () => Copies.Acquire(nameof(Mesh.Smooth), _ => Edits.Accepted<Mesh, int>(at.Mesh,
                    at.Mesh.Smooth(of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, of.Options.Coordinates.System, of.Options.Coordinates.Plane)), at.Retain)),
            collapseFacesByEdgeLength: static (at, of) => Copies.Acquire(nameof(Mesh.CollapseFacesByEdgeLength), _ =>
                Counted(at.Mesh, at.Mesh.CollapseFacesByEdgeLength(of.GreaterThan, of.EdgeLength)), at.Retain),
            collapseFacesByArea: static (at, of) => Copies.Acquire(nameof(Mesh.CollapseFacesByArea), _ => Edits.Count(at.Mesh, at.Mesh.CollapseFacesByArea(of.LessThanArea, of.GreaterThanArea)), at.Retain),
            collapseFacesByByAspectRatio: static (at, of) => Copies.Acquire(nameof(Mesh.CollapseFacesByByAspectRatio), _ => Edits.Count(at.Mesh, at.Mesh.CollapseFacesByByAspectRatio(of.AspectRatio)), at.Retain),
            subdivide: static (at, of) => of.FaceIndices.Match(
                Some: indices => IO.lift(() => Copies.Within(at.Mesh, ComponentIndexType.MeshFace, indices))
                    .Bind(faces => Copies.Acquire(nameof(Mesh.Subdivide), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Subdivide(faces)), at.Retain)),
                None: () => Copies.Acquire(nameof(Mesh.Subdivide), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Subdivide()), at.Retain)),
            computeNormals: static (at, _) => Copies.Acquire(nameof(MeshVertexNormalList.ComputeNormals), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Normals.ComputeNormals()), at.Retain),
            computeFaceNormals: static (at, _) => Copies.Acquire(nameof(MeshFaceNormalList.ComputeFaceNormals), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FaceNormals.ComputeFaceNormals()), at.Retain),
            unitizeNormals: static (at, _) => Copies.Acquire(nameof(MeshVertexNormalList.UnitizeNormals), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Normals.UnitizeNormals()), at.Retain),
            unitizeFaceNormals: static (at, _) => Copies.Acquire(nameof(MeshFaceNormalList.UnitizeFaceNormals), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.FaceNormals.UnitizeFaceNormals()), at.Retain),
            rebuildNormals: static (at, _) => Copies.Acquire(nameof(Mesh.RebuildNormals), _ => {
                at.Mesh.RebuildNormals();
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain),
            unifyNormals: static (at, _) => Copies.Acquire(nameof(Mesh.UnifyNormals), _ => Edits.Count(at.Mesh, at.Mesh.UnifyNormals(countOnly: false)), at.Retain),
            flip: static (at, of) => Copies.Acquire(nameof(Mesh.Flip), _ => {
                at.Mesh.Flip(of.VertexNormals, of.FaceNormals, of.FaceOrientation, of.NgonsBoundaryDirection);
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain),
            map: static (at, of) => Mappings.WithMapping(of.Mapping, mapping => Copies.Acquire(nameof(Mesh.SetTextureCoordinates), _ => {
                at.Mesh.SetTextureCoordinates(mapping, of.Xform, lazy: false, of.SeamCheck);
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain)),
            normalizeTextureCoordinates: static (at, _) => Copies.Acquire(nameof(MeshTextureCoordinateList.NormalizeTextureCoordinates), _ =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TextureCoordinates.NormalizeTextureCoordinates()), at.Retain),
            transposeTextureCoordinates: static (at, _) => Copies.Acquire(nameof(MeshTextureCoordinateList.TransposeTextureCoordinates), _ =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TextureCoordinates.TransposeTextureCoordinates()), at.Retain),
            reverseTextureCoordinates: static (at, of) => Copies.Acquire(nameof(MeshTextureCoordinateList.ReverseTextureCoordinates), _ =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.TextureCoordinates.ReverseTextureCoordinates((int)of.Direction)), at.Retain),
            clearTextureData: static (at, _) => Copies.Acquire(nameof(Mesh.ClearTextureData), _ => {
                at.Mesh.ClearTextureData();
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain),
            clearSurfaceData: static (at, _) => Copies.Acquire(nameof(Mesh.ClearSurfaceData), _ => {
                at.Mesh.ClearSurfaceData();
                return Edits.Completed<Mesh, int>(at.Mesh);
            }, at.Retain),
            surfaceParametersFromTexture: static (at, _) => Copies.Acquire(nameof(Mesh.SetSurfaceParametersFromTextureCoordinates), _ =>
                Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.SetSurfaceParametersFromTextureCoordinates()), at.Retain),
            evaluateOn: static (at, of) => Copies.Acquire(nameof(Mesh.EvaluateMeshGeometry), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.EvaluateMeshGeometry(of.Surface)), at.Retain),
            compact: static (at, _) => Copies.Acquire(nameof(Mesh.Compact), _ => Edits.Accepted<Mesh, int>(at.Mesh, at.Mesh.Compact()), at.Retain),
            rebuild: static (at, of) => Copies.Acquire(nameof(Mesh.RebuildMesh), _ =>
                Edits.Replaced<Mesh, int>(Mesh.RebuildMesh(at.Mesh, of.PreserveTextureCoordinates, of.PreserveVertexColors)), at.Retain),
            unwelded: static (at, _) => Copies.Acquire(nameof(Mesh.CreateUnweldedMesh), _ => Edits.Replaced<Mesh, int>(Mesh.CreateUnweldedMesh(at.Mesh)), at.Retain),
            withShutLining: static (at, of) => Thrown(nameof(Mesh.WithShutLining), Copies.Acquire(nameof(Mesh.WithShutLining), _ =>
                Edits.Replaced<Mesh, int>(at.Mesh.WithShutLining(of.Faceted, at.Tolerances.Absolute, of.Curves)), at.Retain)),
            withDisplacement: static (at, of) => Thrown(nameof(Mesh.WithDisplacement), Copies.Acquire(nameof(Mesh.WithDisplacement), _ =>
                Edits.Replaced<Mesh, int>(at.Mesh.WithDisplacement(of.Displacement)), at.Retain)),
            withEdgeSoftening: static (at, of) => Thrown(nameof(Mesh.WithEdgeSoftening), Copies.Acquire(nameof(Mesh.WithEdgeSoftening), _ =>
                Edits.Replaced<Mesh, int>(at.Mesh.WithEdgeSoftening(of.Radius, of.Chamfer, of.Faceted, of.Force, of.AngleThresholdDegrees)), at.Retain)));

    // --- [STEPS]
    private static IO<EditStep<Mesh, int>> TopologyEdge(Mesh mesh, int edge, Action<IDisposable> retain, string member, Func<Mesh, int, bool> step) =>
        IO.lift(() => Copies.Within(mesh, ComponentIndexType.MeshTopologyEdge, Seq(edge)))
        >> Copies.Acquire(member, _ => Edits.Accepted<Mesh, int>(mesh, step(mesh, edge)), retain);

    private static Answer<EditStep<Mesh, int>> Counted(Mesh mesh, int count) =>
        (Answer.Accepted(count >= 0), Edits.Count(mesh, count)).Apply(static (_, step) => step).As();

    private static IO<EditStep<Mesh, int>> Thrown(string member, IO<EditStep<Mesh, int>> step) =>
        step.Catch(static error => error.HasException<InvalidOperationException>(), error => IO.fail<EditStep<Mesh, int>>(new Refused(member, error.Message) + error));
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
    public static IO<(Mesh Result, Seq<(MeshEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Mesh source, Seq<MeshEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (mesh, edit, held) => edit.Step(tolerances, mesh, held), static _ => IO.pure(unit), retain);

    // --- [DERIVATIONS]
    public static IO<(Mesh Mesh, Seq<int> WallFaces)> Offset(Mesh source, double distance, bool solidify, Option<Vector3d> direction, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Mesh.Offset), _ =>
            Answer.One(source.Offset(distance, solidify, Conversions.Unset(direction), out List<int> walls)).Map(mesh => (Mesh: mesh, WallFaces: toSeq(walls))), retain);

    public static IO<(Mesh Mesh, Seq<ComponentIndex> Components, Seq<int> WallFaces)> Extrude(Mesh source, Seq<ComponentIndex> components, Action<MeshExtruder> configure, Action<IDisposable> retain) =>
        IO.lift(() => Copies.Within(source, components))
            .Bind(held => DisposalOps.Within(() => new MeshExtruder(source, held), extruder =>
                IO.lift(() => configure(extruder)) >> Copies.Acquire(nameof(MeshExtruder.ExtrudedMesh), _ =>
                    (Answer.Accepted(extruder.ExtrudedMesh(out Mesh mesh, out List<ComponentIndex> map)), Answer.One(mesh))
                        .Apply((_, extruded) => (Mesh: extruded, Components: toSeq(map), WallFaces: toSeq(extruder.GetWallFaces()))).As(), retain)));
}
