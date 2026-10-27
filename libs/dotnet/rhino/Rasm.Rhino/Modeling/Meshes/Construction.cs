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

[Union<Seq<Mesh>, PointCloud, Meshable, Mesh>(T1Name = "Meshes", T2Name = "Cloud", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ShrinkWrapSource {
    public sealed record Meshable(Seq<GeometryBase> Items, MeshingParameters Meshing);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record HullSource {
    public sealed record Tolerant(Seq<Point3d> Points) : HullSource;

    public sealed record Quick(Seq<Point3d> Points) : HullSource;
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
        validationError = RhinoFault.Unless(outer.Switch(outline: static of => of.Boundary.IsClosed, perimeter: static _ => true) && holes.ForAll(static hole => hole.IsClosed))
            .Match(Succ: static _ => default, Fail: static error => error.Filter<RhinoFault>().Head is RhinoFault fault ? fault : null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress) =>
        from token in cancelToken
        from remeshed in source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Token: token),
            brep: static (state, brep) => Products.Made(Mesh.QuadRemeshBrep(brep, state.Parameters, state.Guides, state.Progress, state.Token)),
            mesh: static (state, mesh) => Products.Made(mesh.QuadRemesh(Seq<int>(), state.Parameters, state.Guides, state.Progress, state.Token)),
            blocks: static (state, parts) =>
                from joined in use(static () => new Mesh())
                from appended in IO.lift(() => joined.Append(parts))
                from remeshed in Products.Made(joined.QuadRemesh(parts.Scan(0, static (faces, part) => faces + part.Faces.Count), state.Parameters, state.Guides, state.Progress, state.Token))
                select remeshed)
        select remeshed;

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters) =>
        from token in cancelToken
        from wrapped in Products.Made(source.Switch(
            (Parameters: parameters, Token: token),
            meshes: static (state, meshes) => Mesh.ShrinkWrap(meshes, state.Parameters, state.Token),
            cloud: static (state, cloud) => Mesh.ShrinkWrap(cloud, state.Parameters, state.Token),
            meshable: static (state, of) => Mesh.ShrinkWrap(of.Items, state.Parameters, of.Meshing, state.Token),
            mesh: static (state, mesh) => mesh.ShrinkWrap(state.Parameters, state.Token)))
        select wrapped;

    public static IO<(Mesh Hull, Seq<Seq<int>> Facets)> Hull(Tolerances tolerances, HullSource source) =>
        source.Switch(
            tolerances,
            tolerant: static (at, of) => Products.Made(Mesh.CreateConvexHull3D(of.Points, out int[][] facets, at.Absolute, at.Angle))
                .Map(hull => (Hull: hull, Facets: Facets(facets))),
            quick: static (_, of) => Products.Made(Mesh.CreateQuickHull3D(of.Points, out List<int[]> facets))
                .Map(hull => (Hull: hull, Facets: Facets(facets))));

    public static IO<Mesh> CreatePatch(Tolerances tolerances, PatchBoundary boundary, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints) =>
        from patch in IO.pure(boundary.Outer.Switch(
            outline: static of => (Boundary: Some(of.Boundary), Surface: of.Pullback, Trimback: false, Divisions: 0),
            perimeter: static of => (Boundary: Option<Polyline>.None, Surface: Some(of.Surface), of.Trimback, of.Divisions)))
        from mesh in Products.Made(Mesh.CreatePatch(
            patch.Boundary.ValueUnsafe(), tolerances.Angle, patch.Surface.ValueUnsafe(), boundary.Holes, innerBothSides, innerPoints, patch.Trimback, patch.Divisions))
        select mesh;

    // --- [REBUILT]
    public static IO<Seq<Option<Mesh>>> CreateFromIterativeCleanup(Tolerances tolerances, Seq<Mesh> meshes) =>
        from slots in IO.lift(() => toSeq(Mesh.CreateFromIterativeCleanup(meshes, tolerances.Absolute) ?? new Mesh?[meshes.Count]).Map(static mesh => Optional(mesh)).Strict())
        from cleaned in Products.Made(slots.Somes())
        select slots;

    public static IO<Mesh> CreateFromFilteredFaceList(Mesh mesh, Seq<int> faces) =>
        IO.lift(() => Copies.Within(mesh, ComponentIndexType.MeshFace, faces)).Bind(held =>
            Products.Made(Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, mesh.Faces.Count)).Map(toHashSet(held).Contains))));

    public static IO<Mesh> Refine(Mesh mesh, Refinement refinement, int level, CreaseEdges nakedEdgeMode) =>
        from token in cancelToken
        from refined in refinement.Switch(
            (Mesh: mesh, Settings: new RefinementSettings { Level = level, NakedEdgeMode = nakedEdgeMode, ContinueRequest = token }),
            catmullClark: static (at, _) => Products.Made(Mesh.CreateRefinedCatmullClarkMesh(at.Mesh, at.Settings)),
            loop: static (at, of) => Products.Made(Mesh.CreateRefinedLoopMesh(at.Mesh, of.Formula, at.Settings)))
        select refined;

    // --- [FACETS]
    private static Seq<Seq<int>> Facets(IEnumerable<int[]?>? facets) =>
        Conversions.Rows(facets).Map(static facet => toSeq(facet)).Strict();
}
