using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Modeling.Curves;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshEdit {
    public sealed record Reduce(ReduceMeshParameters Parameters, bool Threaded) : MeshEdit;
    public sealed record Weld(double AngleToleranceRadians, bool PreserveSurfaceParameters) : MeshEdit;
    public sealed record Unweld(double AngleToleranceRadians, bool ModifyNormals) : MeshEdit;
    public sealed record UnweldEdge(Seq<int> EdgeIndices, bool ModifyNormals) : MeshEdit;
    public sealed record HealNakedEdges(double Distance) : MeshEdit;
    public sealed record FillHoles : MeshEdit;
    public sealed record FillHole(int TopologyEdgeIndex) : MeshEdit;
    public sealed record MatchEdges(double Distance, bool Rachet) : MeshEdit;
    public sealed record MergeAllCoplanarFaces : MeshEdit;
    public sealed record Smooth(SmoothOptions Options, int NumSteps, Option<Seq<int>> VertexIndices) : MeshEdit;
    public sealed record CollapseFacesByEdgeLength(bool GreaterThan, double EdgeLength) : MeshEdit;
    public sealed record CollapseFacesByArea(double LessThanArea, double GreaterThanArea) : MeshEdit;
    public sealed record CollapseFacesByByAspectRatio(double AspectRatio) : MeshEdit;
    public sealed record RebuildNormals : MeshEdit;
    public sealed record UnifyNormals : MeshEdit;
    public sealed record Flip(bool VertexNormals, bool FaceNormals, bool FaceOrientation, bool NgonsBoundaryDirection) : MeshEdit;
    public sealed record Compact : MeshEdit;
    public sealed record Subdivide(Option<Seq<int>> FaceIndices) : MeshEdit;
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
    public static IO<Mesh> Edit(Mesh source, Seq<MeshEdit> edits, Tolerances tolerances) =>
        from token in cancelToken
        from copy in Copies.Duplicate(source)
        from edited in DisposalOps.OnFailure(
            from applied in edits.TraverseM(edit => Applied(copy, edit, tolerances, token)).As()
            from valid in IO.lift(() => Measurements.Valid(copy, nameof(Edit)))
            select valid,
            IO.lift(copy.Dispose))
        select edited;

    private static IO<Unit> Applied(Mesh copy, MeshEdit edit, Tolerances tolerances, CancellationToken token) =>
        edit.Switch(
            (Copy: copy, Tolerances: tolerances, Token: token),
            reduce: static (state, of) => IO.lift(Fin<Unit> () => state.Copy.Reduce(MeshMapper.Reduction((of.Parameters, state.Token)), of.Threaded)
                ? unit
                : state.Token.IsCancellationRequested ? Errors.Cancelled : new Refused(nameof(Mesh.Reduce))),
            weld: static (state, of) => IO.lift(() => state.Copy.Weld(of.AngleToleranceRadians, of.PreserveSurfaceParameters)),
            unweld: static (state, of) => IO.lift(() => state.Copy.Unweld(of.AngleToleranceRadians, of.ModifyNormals)),
            unweldEdge: static (state, of) => IO.lift(() => Refused.Unless(state.Copy.UnweldEdge(of.EdgeIndices, of.ModifyNormals), nameof(Mesh.UnweldEdge))),
            healNakedEdges: static (state, of) => IO.lift(() => ignore(state.Copy.HealNakedEdges(of.Distance))),
            fillHoles: static (state, _) => IO.lift(() => Refused.Unless(state.Copy.FillHoles(), nameof(Mesh.FillHoles))),
            fillHole: static (state, of) => IO.lift(() => Refused.Unless(state.Copy.FillHole(of.TopologyEdgeIndex), nameof(Mesh.FillHole))),
            matchEdges: static (state, of) => IO.lift(() => ignore(state.Copy.MatchEdges(of.Distance, of.Rachet))),
            mergeAllCoplanarFaces: static (state, _) => IO.lift(() => ignore(state.Copy.MergeAllCoplanarFaces(state.Tolerances.Absolute, state.Tolerances.Angle))),
            smooth: static (state, of) => of.Options.Coordinates switch {
                var (system, plane) => IO.lift(() => Refused.Unless(
                    of.VertexIndices.IsSome
                        ? state.Copy.Smooth(of.VertexIndices.ValueUnsafe(), of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane)
                        : state.Copy.Smooth(of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane),
                    nameof(Mesh.Smooth))),
            },
            collapseFacesByEdgeLength: static (state, of) => IO.lift(() => Refused.Unless(state.Copy.CollapseFacesByEdgeLength(of.GreaterThan, of.EdgeLength) != -1, nameof(Mesh.CollapseFacesByEdgeLength))),
            collapseFacesByArea: static (state, of) => IO.lift(() => ignore(state.Copy.CollapseFacesByArea(of.LessThanArea, of.GreaterThanArea))),
            collapseFacesByByAspectRatio: static (state, of) => IO.lift(() => ignore(state.Copy.CollapseFacesByByAspectRatio(of.AspectRatio))),
            rebuildNormals: static (state, _) => IO.lift(() => state.Copy.RebuildNormals()),
            unifyNormals: static (state, _) => IO.lift(() => ignore(state.Copy.UnifyNormals(countOnly: false))),
            flip: static (state, of) => IO.lift(() => state.Copy.Flip(of.VertexNormals, of.FaceNormals, of.FaceOrientation, of.NgonsBoundaryDirection)),
            compact: static (state, _) => IO.lift(() => Refused.Unless(state.Copy.Compact(), nameof(Mesh.Compact))),
            subdivide: static (state, of) => of.FaceIndices.IsSome
                ? of.FaceIndices.ValueUnsafe() switch {
                    var faces => IO.lift(() => Copies.InRange(faces, state.Copy.Faces.Count, nameof(Mesh.Subdivide)))
                        >> IO.lift(() => Refused.Unless(state.Copy.Subdivide(faces), nameof(Mesh.Subdivide))),
                }
                : IO.lift(() => Refused.Unless(state.Copy.Subdivide(), nameof(Mesh.Subdivide))));

    // --- [DERIVATIONS]
    public static IO<(Mesh Mesh, Seq<int> WallFaces)> Offset(Mesh source, double distance, bool solidify, Option<Vector3d> direction) =>
        from answer in IO.lift(() => (
            Mesh: source.Offset(distance, solidify, Conversions.Unset(direction), out List<int> wallFacesOut),
            WallFaces: toSeq(wallFacesOut)))
        from offset in Copies.Acquire(() => answer.Mesh, nameof(Mesh.Offset))
        select (offset, answer.WallFaces);

    public static IO<(Mesh Mesh, Seq<ComponentIndex> Components, Seq<int> WallFaces)> Extrude(Mesh source, Seq<ComponentIndex> components, Action<MeshExtruder> configure) =>
        (from extruder in use(() => new MeshExtruder(source, components))
         from configured in IO.lift(() => configure(extruder))
         from answer in IO.lift(() => (Accepted: extruder.ExtrudedMesh(out Mesh made, out List<ComponentIndex> map), Mesh: made, Components: map))
         from result in DisposalOps.OnFailure(
             from accepted in IO.lift(Refused.Unless(answer.Accepted, answer.Mesh, nameof(MeshExtruder.ExtrudedMesh)))
             from mesh in IO.lift(() => Measurements.Valid(accepted, nameof(MeshExtruder.ExtrudedMesh)))
             from walls in IO.lift(() => toSeq(extruder.GetWallFaces()))
             select (Mesh: mesh, Components: toSeq(answer.Components), WallFaces: walls),
             IO.lift(answer.Mesh.Dispose))
         select result).Bracket();
}
