using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling.Curves;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshEdit {
    public sealed record Reduce(ReduceMeshParameters Parameters) : MeshEdit;

    public sealed record Weld(double AngleRadians, bool PreserveSurfaceParameters) : MeshEdit;

    public sealed record Unweld(double AngleRadians, bool ModifyNormals) : MeshEdit;

    public sealed record UnweldEdges(Seq<int> TopologyEdges, bool ModifyNormals) : MeshEdit;

    public sealed record UnweldVertices(Seq<int> TopologyVertices, bool ModifyNormals) : MeshEdit;

    public sealed record HealNakedEdges(double Distance) : MeshEdit;

    public sealed record FillHoles() : MeshEdit;

    public sealed record FillHole(int TopologyEdge) : MeshEdit;

    public sealed record MatchEdges(double Distance, bool Rachet) : MeshEdit;

    public sealed record MergeCoplanar(double Tolerance, double AngleTolerance) : MeshEdit;

    public sealed record Smooth(SmoothOptions Options, int Steps, Seq<int> Vertices) : MeshEdit;

    public sealed record CollapseByEdgeLength(bool GreaterThan, double Length) : MeshEdit;

    public sealed record CollapseByArea(double LessThan, double GreaterThan) : MeshEdit;

    public sealed record CollapseByAspectRatio(double Ratio) : MeshEdit;

    public sealed record RebuildNormals() : MeshEdit;

    public sealed record UnifyNormals() : MeshEdit;

    public sealed record Flip(bool VertexNormals, bool FaceNormals, bool FaceOrientation, bool NgonsBoundaryDirection) : MeshEdit;

    public sealed record Compact() : MeshEdit;

    public sealed record Subdivide(Seq<int> Faces) : MeshEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshDerivation {
    public sealed record EdgeSoften(double Radius, bool Chamfer, bool Faceted, bool Force, double AngleThreshold) : MeshDerivation;

    public sealed record ShutLine(bool Faceted, double Tolerance, Seq<ShutLiningCurveInfo> Curves) : MeshDerivation;

    public sealed record Displace(MeshDisplacementInfo Info) : MeshDerivation;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrudeFrame {
    public sealed record Xform(Transform Value) : ExtrudeFrame;

    public sealed record Uvn() : ExtrudeFrame;

    public sealed record EdgeUvn() : ExtrudeFrame;
}

public sealed record ExtrudeOptions(ExtrudeFrame Frame, bool KeepOriginalFaces, MeshExtruderParameterMode TextureCoordinateMode, MeshExtruderParameterMode SurfaceParameterMode, MeshExtruderFaceDirectionMode FaceDirectionMode);

public sealed record MeshExtrudeResult(Mesh Mesh, Seq<ComponentIndex> Components);

public sealed record MeshOffsetResult(Mesh Mesh, Seq<int> WallFaces);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MeshMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(ReduceMeshParameters.Error), Justification = "Mesh.Reduce output")]
    internal static partial void Update(ReduceMeshParameters parameters, ReduceMeshParameters copy);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(ExtrudeOptions.Frame), Justification = "MeshExtruder.Transform, UVN, or EdgeBasedUVN per ExtrudeFrame case")]
    internal static partial void Update(ExtrudeOptions options, MeshExtruder extruder);
}

public static class MeshEdits {
    // --- [EDITS]
    public static IO<Mesh> Edit(Mesh source, MeshEdit edit) =>
        from apply in IO.lift(() => Validated(source, edit))
        from edited in GeometryResults.EditCopy(source, apply)
        select edited.Copy;

    private static Fin<Func<Mesh, Fin<Unit>>> Validated(Mesh source, MeshEdit edit) =>
        edit.Switch(
            source,
            reduce: static (_, of) => fun((Mesh copy) => {
                ReduceMeshParameters parameters = new();
                MeshMapper.Update(of.Parameters, parameters);
                return Refused.Unless(copy.Reduce(parameters, threaded: false), nameof(Mesh.Reduce));
            }),
            weld: static (_, of) => fun((Mesh copy) => {
                copy.Weld(of.AngleRadians, of.PreserveSurfaceParameters);
                return Fin.Succ(unit);
            }),
            unweld: static (_, of) => fun((Mesh copy) => {
                copy.Unweld(of.AngleRadians, of.ModifyNormals);
                return Fin.Succ(unit);
            }),
            unweldEdges: static (_, of) => fun((Mesh copy) => Refused.Unless(copy.UnweldEdge(of.TopologyEdges, of.ModifyNormals), nameof(Mesh.UnweldEdge))),
            unweldVertices: static (_, of) => fun((Mesh copy) => Refused.Unless(copy.UnweldVertices(of.TopologyVertices, of.ModifyNormals), nameof(Mesh.UnweldVertices))),
            healNakedEdges: static (_, of) => fun((Mesh copy) => Refused.Unless(copy.HealNakedEdges(of.Distance), nameof(Mesh.HealNakedEdges))),
            fillHoles: static (_, _) => fun(static (Mesh copy) => Refused.Unless(copy.FillHoles(), nameof(Mesh.FillHoles))),
            fillHole: static (_, of) => fun((Mesh copy) => Refused.Unless(copy.FillHole(of.TopologyEdge), nameof(Mesh.FillHole))),
            matchEdges: static (_, of) => fun((Mesh copy) => Refused.Unless(copy.MatchEdges(of.Distance, of.Rachet), nameof(Mesh.MatchEdges))),
            mergeCoplanar: static (_, of) => fun((Mesh copy) => Fin.Succ(ignore(copy.MergeAllCoplanarFaces(of.Tolerance, of.AngleTolerance)))),
            smooth: static (_, of) =>
                from planar in Invalid.Unless(of.Options.System != SmoothingCoordinateSystem.CPlane || of.Options.Plane.IsValid, nameof(of.Options))
                select fun((Mesh copy) => Refused.Unless(
                    of.Vertices.IsEmpty
                        ? copy.Smooth(of.Options.Factor, of.Steps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, of.Options.System, of.Options.Plane)
                        : copy.Smooth(of.Vertices, of.Options.Factor, of.Steps, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, of.Options.System, of.Options.Plane),
                    nameof(Mesh.Smooth))),
            collapseByEdgeLength: static (_, of) => fun((Mesh copy) => Collapsed(copy, mesh => mesh.CollapseFacesByEdgeLength(of.GreaterThan, of.Length), nameof(Mesh.CollapseFacesByEdgeLength))),
            collapseByArea: static (_, of) => fun((Mesh copy) => Collapsed(copy, mesh => mesh.CollapseFacesByArea(of.LessThan, of.GreaterThan), nameof(Mesh.CollapseFacesByArea))),
            collapseByAspectRatio: static (_, of) => fun((Mesh copy) => Collapsed(copy, mesh => mesh.CollapseFacesByByAspectRatio(of.Ratio), nameof(Mesh.CollapseFacesByByAspectRatio))),
            rebuildNormals: static (_, _) => fun(static (Mesh copy) => {
                copy.RebuildNormals();
                return Fin.Succ(unit);
            }),
            unifyNormals: static (_, _) => fun(static (Mesh copy) => Fin.Succ(ignore(copy.UnifyNormals()))),
            flip: static (_, of) => fun((Mesh copy) => {
                copy.Flip(of.VertexNormals, of.FaceNormals, of.FaceOrientation, of.NgonsBoundaryDirection);
                return Fin.Succ(unit);
            }),
            compact: static (_, _) => fun(static (Mesh copy) => Refused.Unless(copy.Compact(), nameof(Mesh.Compact))),
            subdivide: static (mesh, of) =>
                from inside in GeometryResults.InRange(of.Faces, mesh.Faces.Count, nameof(Mesh.Faces))
                select fun((Mesh copy) => Refused.Unless(of.Faces.IsEmpty ? copy.Subdivide() : copy.Subdivide(of.Faces), nameof(Mesh.Subdivide))));

    private static Fin<Unit> Collapsed(Mesh copy, Func<Mesh, int> collapse, string member) =>
        (collapse(copy), copy.Faces.Count) switch {
            (_, 0) => new Degenerate(member),
            ( < 0, _) => new Refused(member),
            _ => unit,
        };

    // --- [DERIVATIONS]
    public static IO<Mesh> Derive(Mesh mesh, MeshDerivation derivation) =>
        derivation.Switch(
            mesh,
            edgeSoften: static (source, of) => GeometryResults.Acquire(() => source.WithEdgeSoftening(of.Radius, of.Chamfer, of.Faceted, of.Force, of.AngleThreshold), nameof(Mesh.WithEdgeSoftening)),
            shutLine: static (source, of) => GeometryResults.Acquire(
                () => Invalid.Unless(!of.Curves.IsEmpty, nameof(of.Curves))
                    .Map(_ => source.WithShutLining(of.Faceted, of.Tolerance, of.Curves)),
                nameof(Mesh.WithShutLining)),
            displace: static (source, of) => GeometryResults.Acquire(() => source.WithDisplacement(of.Info), nameof(Mesh.WithDisplacement)));

    public static IO<MeshOffsetResult> Offset(Mesh mesh, double distance, bool solidify, Option<Vector3d> direction) =>
        from answer in IO.lift(() => (Mesh: mesh.Offset(distance, solidify, direction.IfNone(Vector3d.Unset), out List<int> wallFaces), WallFaces: wallFaces))
        from offset in GeometryResults.Acquire(() => answer.Mesh, nameof(Mesh.Offset))
        select new MeshOffsetResult(offset, toSeq(answer.WallFaces));

    public static IO<MeshExtrudeResult> Extrude(Mesh mesh, Seq<ComponentIndex> components, ExtrudeOptions options) =>
        from present in IO.lift(() => Invalid.Unless(!components.IsEmpty, nameof(components)))
        from extruded in DisposalOps.Using(() => new MeshExtruder(mesh, components), extruder =>
            from configured in IO.lift(() => {
                MeshMapper.Update(options, extruder);
                options.Frame.Switch(
                    extruder,
                    xform: static (target, of) => target.Transform = of.Value,
                    uvn: static (target, _) => target.UVN = true,
                    edgeUvn: static (target, _) => target.EdgeBasedUVN = true);
            })
            from extruded in IO.lift(() =>
                Refused.Unless(extruder.ExtrudedMesh(out Mesh extruded, out List<ComponentIndex> map), new MeshExtrudeResult(extruded, toSeq(map)), nameof(MeshExtruder.ExtrudedMesh)))
            select extruded)
        select extruded;
}
