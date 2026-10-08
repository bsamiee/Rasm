using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Modeling.Curves;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
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

public sealed record MeshOffset(Mesh Mesh, Seq<int> WallFaces);

public sealed record MeshExtrusion(Mesh Mesh, Seq<ComponentIndex> Components, Seq<int> WallFaces);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MeshMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(ReduceMeshParameters.Error), Justification = "Mesh.Reduce output")]
    [MapperIgnoreSource(nameof(ReduceMeshParameters.CancelToken), Justification = "Run token through cancelToken")]
    internal static partial void Update(ReduceMeshParameters parameters, ReduceMeshParameters copy);
}

public static class MeshEdits {
    // --- [EDITS]
    public static IO<Mesh> Edit(Mesh source, Seq<MeshEdit> edits, Tolerances tolerances) =>
        from token in cancelToken
        from edited in Copies.Edit(source, copy => edits.TraverseM(edit => Applied(copy, edit, tolerances, token)).As().Map(static _ => unit), nameof(Edit))
        select edited;

    private static Fin<Unit> Applied(Mesh copy, MeshEdit edit, Tolerances tolerances, CancellationToken token) =>
        edit.Switch<(Mesh Copy, Tolerances Tolerances, CancellationToken Token), Fin<Unit>>(
            (Copy: copy, Tolerances: tolerances, Token: token),
            reduce: static (state, of) => state.Copy.Reduce(Reduction(of.Parameters, state.Token), of.Threaded)
                ? unit
                : state.Token.IsCancellationRequested ? Errors.Cancelled : new Refused(nameof(Mesh.Reduce)),
            weld: static (state, of) => {
                state.Copy.Weld(of.AngleToleranceRadians, of.PreserveSurfaceParameters);
                return unit;
            },
            unweld: static (state, of) => {
                state.Copy.Unweld(of.AngleToleranceRadians, of.ModifyNormals);
                return unit;
            },
            unweldEdge: static (state, of) => Refused.Unless(state.Copy.UnweldEdge(of.EdgeIndices, of.ModifyNormals), nameof(Mesh.UnweldEdge)),
            healNakedEdges: static (state, of) => ignore(state.Copy.HealNakedEdges(of.Distance)),
            fillHoles: static (state, _) => Refused.Unless(state.Copy.FillHoles(), nameof(Mesh.FillHoles)),
            fillHole: static (state, of) => Refused.Unless(state.Copy.FillHole(of.TopologyEdgeIndex), nameof(Mesh.FillHole)),
            matchEdges: static (state, of) => ignore(state.Copy.MatchEdges(of.Distance, of.Rachet)),
            mergeAllCoplanarFaces: static (state, _) => ignore(state.Copy.MergeAllCoplanarFaces(state.Tolerances.Absolute, state.Tolerances.Angle)),
            smooth: static (state, of) => of.Options.Coordinates switch {
                var (system, plane) => Refused.Unless(
                    of.VertexIndices.Match(
                        Some: vertices => state.Copy.Smooth(vertices, of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane),
                        None: () => state.Copy.Smooth(of.Options.Factor, of.NumSteps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane)),
                    nameof(Mesh.Smooth)),
            },
            collapseFacesByEdgeLength: static (state, of) => Refused.Unless(state.Copy.CollapseFacesByEdgeLength(of.GreaterThan, of.EdgeLength) != -1, nameof(Mesh.CollapseFacesByEdgeLength)),
            collapseFacesByArea: static (state, of) => ignore(state.Copy.CollapseFacesByArea(of.LessThanArea, of.GreaterThanArea)),
            collapseFacesByByAspectRatio: static (state, of) => ignore(state.Copy.CollapseFacesByByAspectRatio(of.AspectRatio)),
            rebuildNormals: static (state, _) => {
                state.Copy.RebuildNormals();
                return unit;
            },
            unifyNormals: static (state, _) => ignore(state.Copy.UnifyNormals(countOnly: false)),
            flip: static (state, of) => {
                state.Copy.Flip(of.VertexNormals, of.FaceNormals, of.FaceOrientation, of.NgonsBoundaryDirection);
                return unit;
            },
            compact: static (state, _) => Refused.Unless(state.Copy.Compact(), nameof(Mesh.Compact)),
            subdivide: static (state, of) => of.FaceIndices.Match(
                Some: faces =>
                    from inside in Copies.InRange(faces, state.Copy.Faces.Count, nameof(Mesh.Subdivide))
                    from subdivided in Refused.Unless(state.Copy.Subdivide(faces), nameof(Mesh.Subdivide))
                    select subdivided,
                None: () => Refused.Unless(state.Copy.Subdivide(), nameof(Mesh.Subdivide))));

    private static ReduceMeshParameters Reduction(ReduceMeshParameters parameters, CancellationToken token) {
        ReduceMeshParameters copy = new() { CancelToken = token };
        MeshMapper.Update(parameters, copy);
        return copy;
    }

    // --- [DERIVATIONS]
    public static IO<MeshOffset> Offset(Mesh source, double distance, bool solidify, Option<Vector3d> direction) =>
        from answer in IO.lift(() => (
            Mesh: source.Offset(distance, solidify, Conversions.Unset(direction), out List<int> wallFacesOut),
            WallFaces: toSeq(wallFacesOut)))
        from offset in Copies.Acquire(() => answer.Mesh, nameof(Mesh.Offset))
        select new MeshOffset(offset, answer.WallFaces);

    public static IO<MeshExtrusion> Extrude(Mesh source, Seq<ComponentIndex> components, Action<MeshExtruder> configure) =>
        use(() => new MeshExtruder(source, components))
            .Bind(extruder => Copies.Owned(
                IO.lift(() => {
                    configure(extruder);
                    return (
                        Accepted: extruder.ExtrudedMesh(out Mesh made, out List<ComponentIndex> map),
                        Mesh: made,
                        Components: toSeq(map),
                        WallFaces: toSeq(extruder.GetWallFaces()));
                }),
                static answer => Refused.Unless(answer.Accepted, nameof(MeshExtruder.ExtrudedMesh))
                    .Bind(_ => Measurements.Valid(answer.Mesh, nameof(MeshExtruder.ExtrudedMesh)))
                    .Map(mesh => new MeshExtrusion(mesh, answer.Components, answer.WallFaces)),
                static answer => IO.lift(answer.Mesh.Dispose)))
            .Bracket();
}
