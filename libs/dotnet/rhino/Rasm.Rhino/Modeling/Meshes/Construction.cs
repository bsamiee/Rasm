using System.Collections.Generic;
using Rasm.Rhino.Document.Units;
using Rhino.Geometry.MeshRefinements;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Refinement {
    public sealed record CatmullClark : Refinement;

    public sealed record Loop(LoopFormula Formula) : Refinement;
}

[Union<Brep, Mesh, Seq<Mesh>>(T3Name = "Blocks", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class QuadSource;

public sealed record Meshable(Seq<GeometryBase> Items, MeshingParameters Meshing);

[Union<Seq<Mesh>, PointCloud, Meshable, Mesh>(T1Name = "Meshes", T2Name = "Cloud", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ShrinkWrapSource;

[SmartEnum]
public sealed partial class HullMethod {
    public static readonly HullMethod Tolerant = new(static (points, tolerances) => (Mesh.CreateConvexHull3D(points, out int[][] facets, tolerances.Absolute, tolerances.Angle), facets));
    public static readonly HullMethod Quick = new(static (points, _) => (Mesh.CreateQuickHull3D(points, out List<int[]> facets), facets));

    [UseDelegateFromConstructor]
    internal partial (Mesh? Hull, IEnumerable<int[]>? Facets) Build(Seq<Point3d> points, Tolerances tolerances);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PatchOuter {
    public sealed record Outline(Polyline Boundary, Option<Surface> Pullback) : PatchOuter;

    public sealed record Perimeter(Surface Surface, bool Trimback, int Divisions) : PatchOuter;
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<RhinoFault>]
public sealed partial class PatchBoundary {
    public PatchOuter Outer { get; }
    public Seq<Curve> Holes { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref PatchOuter outer, ref Seq<Curve> holes) =>
        validationError = (Closed: outer.Switch(outline: static of => of.Boundary.IsClosed, perimeter: static _ => true), Open: holes.Count(static hole => !hole.IsClosed)) switch {
            (Closed: true, Open: 0) => null,
            var held => new RhinoFault(held.ToString(), RhinoReason.Invalid),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress) =>
        from token in cancelToken
        from remeshed in source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Token: token),
            brep: static (state, brep) => Products.Of(() => Mesh.QuadRemeshBrep(brep, state.Parameters, state.Guides, state.Progress, state.Token)),
            mesh: static (state, mesh) => Products.Of(() => mesh.QuadRemesh(Seq<int>(), state.Parameters, state.Guides, state.Progress, state.Token)),
            blocks: static (state, parts) =>
                from joined in use(static () => new Mesh())
                from appended in IO.lift(() => joined.Append(parts))
                from remeshed in Products.Of(() => joined.QuadRemesh(parts.Scan(0, static (faces, part) => faces + part.Faces.Count), state.Parameters, state.Guides, state.Progress, state.Token))
                select remeshed)
        select remeshed;

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters) =>
        from token in cancelToken
        from wrapped in Products.Of(() => source.Switch(
            (Parameters: parameters, Token: token),
            meshes: static (state, meshes) => Mesh.ShrinkWrap(meshes, state.Parameters, state.Token),
            cloud: static (state, cloud) => Mesh.ShrinkWrap(cloud, state.Parameters, state.Token),
            meshable: static (state, of) => Mesh.ShrinkWrap(of.Items, state.Parameters, of.Meshing, state.Token),
            mesh: static (state, mesh) => mesh.ShrinkWrap(state.Parameters, state.Token)))
        select wrapped;

    public static IO<(Mesh Hull, Seq<Seq<int>> Facets)> Hull(Tolerances tolerances, Seq<Point3d> points, HullMethod method) =>
        from answered in IO.lift(() => method.Build(points, tolerances))
        from hull in Products.Of(() => answered.Hull)
        select (Hull: hull, Facets: Conversions.Rows(answered.Facets).Map(static facet => toSeq(facet)).Strict());

    public static IO<Mesh> CreatePatch(Tolerances tolerances, PatchBoundary boundary, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints) =>
        Products.Of(() => boundary.Outer.Switch(
            (boundary.Holes, tolerances.Angle, Inner: innerBothSides, Points: innerPoints),
            outline: static (at, of) => Mesh.CreatePatch(of.Boundary, at.Angle, of.Pullback.ValueUnsafe(), at.Holes, at.Inner, at.Points, trimback: false, divisions: 0),
            perimeter: static (at, of) => Mesh.CreatePatch(outerBoundary: null, at.Angle, of.Surface, at.Holes, at.Inner, at.Points, of.Trimback, of.Divisions)));

    // --- [REBUILT]
    public static IO<Seq<Option<Mesh>>> CreateFromIterativeCleanup(Tolerances tolerances, Seq<Mesh> meshes) =>
        from slots in IO.lift(() => toSeq(Mesh.CreateFromIterativeCleanup(meshes, tolerances.Absolute) ?? new Mesh?[meshes.Count]).Map(static mesh => Optional(mesh)).Strict())
        from cleaned in Products.Many(() => slots.Somes(), required: false)
        select slots;

    public static IO<Mesh> CreateFromFilteredFaceList(Mesh mesh, Seq<int> faces) =>
        from held in IO.lift(() => mesh.Within(ComponentIndexType.MeshFace, faces))
        from filtered in Products.Of(() => Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, mesh.Faces.Count)).Map(toHashSet(held).Contains)))
        select filtered;

    public static IO<Mesh> Refine(Mesh mesh, Refinement refinement, int level, CreaseEdges nakedEdgeMode) =>
        from token in cancelToken
        from refined in refinement.Switch(
            (Mesh: mesh, Settings: new RefinementSettings { Level = level, NakedEdgeMode = nakedEdgeMode, ContinueRequest = token }),
            catmullClark: static (at, _) => Products.Of(() => Mesh.CreateRefinedCatmullClarkMesh(at.Mesh, at.Settings)),
            loop: static (at, of) => Products.Of(() => Mesh.CreateRefinedLoopMesh(at.Mesh, of.Formula, at.Settings)))
        select refined;

    // --- [DERIVED]
    public static IO<(Mesh Mesh, Seq<int> WallFaces)> Offset(Mesh source, double distance, bool solidify, Option<Vector3d> direction) =>
        from answered in IO.lift(() => (Mesh: source.Offset(distance, solidify, Conversions.Unset(direction), out List<int> walls), Walls: walls))
        from offset in Products.Of(() => answered.Mesh, nameof(Mesh.Offset))
        select (Mesh: offset, WallFaces: toSeq(answered.Walls));

    public static IO<(Mesh Mesh, Seq<ComponentIndex> Components, Seq<int> WallFaces)> Extrude(Mesh source, Seq<ComponentIndex> components, Action<MeshExtruder> configure) =>
        from held in IO.lift(() => source.Within(components))
        from extruder in Products.Of(() => new MeshExtruder(source, held))
        from configured in IO.lift(() => configure(extruder))
        from answered in IO.lift(() => (Accepted: extruder.ExtrudedMesh(out Mesh mesh, out List<ComponentIndex> map), Mesh: mesh, Map: map))
        from extruded in Products.Of(() => answered.Mesh, nameof(MeshExtruder.ExtrudedMesh))
        from accepted in IO.lift(() => RhinoFault.Unless(answered.Accepted, RhinoReason.Declined, components.Count))
        select (Mesh: extruded, Components: toSeq(answered.Map), WallFaces: toSeq(extruder.GetWallFaces()));
}
