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
        validationError = outer.Switch(outline: static of => of.Boundary.IsClosed, perimeter: static _ => true) && holes.ForAll(static hole => hole.IsClosed)
            ? null
            : new RhinoFault("mesh patch needs a closed outer polyline and closed inner boundaries");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress, Action<IDisposable> retain) =>
        source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Retain: retain),
            brep: static (state, brep) => Copies.Acquire(nameof(Mesh.QuadRemeshBrep), token =>
                Answer.One(Mesh.QuadRemeshBrep(brep, state.Parameters, state.Guides, state.Progress, token)), state.Retain),
            mesh: static (state, mesh) => Copies.Acquire(nameof(Mesh.QuadRemesh), token =>
                Answer.One(mesh.QuadRemesh(Seq<int>(), state.Parameters, state.Guides, state.Progress, token)), state.Retain),
            blocks: static (state, parts) => Copies.Acquire(nameof(Mesh.QuadRemesh), token => {
                using Mesh joined = new();
                joined.Append(parts);
                return Answer.One(joined.QuadRemesh(parts.Scan(0, static (faces, part) => faces + part.Faces.Count), state.Parameters, state.Guides, state.Progress, token));
            }, state.Retain));

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Mesh.ShrinkWrap), token => Answer.One(source.Switch(
            (Parameters: parameters, Token: token),
            meshes: static (state, meshes) => Mesh.ShrinkWrap(meshes, state.Parameters, state.Token),
            cloud: static (state, cloud) => Mesh.ShrinkWrap(cloud, state.Parameters, state.Token),
            meshable: static (state, of) => Mesh.ShrinkWrap(of.Items, state.Parameters, of.Meshing, state.Token),
            mesh: static (state, mesh) => mesh.ShrinkWrap(state.Parameters, state.Token))), retain);

    public static IO<(Mesh Hull, Seq<Seq<int>> Facets)> Hull(Tolerances tolerances, HullSource source, Action<IDisposable> retain) =>
        source.Switch(
            (Tolerances: tolerances, Retain: retain),
            tolerant: static (at, of) => Copies.Acquire(nameof(Mesh.CreateConvexHull3D), _ =>
                Answer.One(Mesh.CreateConvexHull3D(of.Points, out int[][] facets, at.Tolerances.Absolute, at.Tolerances.Angle))
                    .Map(hull => (Hull: hull, Facets: Facets(facets))), at.Retain),
            quick: static (at, of) => Copies.Acquire(nameof(Mesh.CreateQuickHull3D), _ =>
                Answer.One(Mesh.CreateQuickHull3D(of.Points, out List<int[]> facets))
                    .Map(hull => (Hull: hull, Facets: Facets(facets))), at.Retain));

    public static IO<Mesh> CreatePatch(Tolerances tolerances, PatchBoundary boundary, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Mesh.CreatePatch), _ => boundary.Outer.Switch(
            outline: static of => (Boundary: Some(of.Boundary), Surface: of.Pullback, Trimback: false, Divisions: 0),
            perimeter: static of => (Boundary: Option<Polyline>.None, Surface: Some(of.Surface), of.Trimback, of.Divisions)) switch {
                var patch => Answer.One(Mesh.CreatePatch(patch.Boundary.ValueUnsafe(), tolerances.Angle, patch.Surface.ValueUnsafe(), boundary.Holes, innerBothSides, innerPoints, patch.Trimback, patch.Divisions)),
            }, retain);

    // --- [REBUILT]
    public static IO<Seq<Option<Mesh>>> CreateFromIterativeCleanup(Tolerances tolerances, Seq<Mesh> meshes, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Mesh.CreateFromIterativeCleanup), _ =>
            Answer.Sparse(Mesh.CreateFromIterativeCleanup(meshes, tolerances.Absolute) ?? new Mesh?[meshes.Count]), retain);

    public static IO<Mesh> CreateFromFilteredFaceList(Mesh mesh, Seq<int> faces, Action<IDisposable> retain) =>
        IO.lift(() => Copies.Within(mesh, ComponentIndexType.MeshFace, faces)).Bind(held => Copies.Acquire(nameof(Mesh.CreateFromFilteredFaceList), _ =>
            Answer.One(Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, mesh.Faces.Count)).Map(toHashSet(held).Contains))), retain));

    public static IO<Mesh> Refine(Mesh mesh, Refinement refinement, int level, CreaseEdges nakedEdgeMode, Action<IDisposable> retain) =>
        refinement.Switch(
            (Mesh: mesh, Level: level, NakedEdgeMode: nakedEdgeMode, Retain: retain),
            catmullClark: static (at, _) => Copies.Acquire(nameof(Mesh.CreateRefinedCatmullClarkMesh), token =>
                Answer.One(Mesh.CreateRefinedCatmullClarkMesh(at.Mesh, new RefinementSettings { Level = at.Level, NakedEdgeMode = at.NakedEdgeMode, ContinueRequest = token })), at.Retain),
            loop: static (at, of) => Copies.Acquire(nameof(Mesh.CreateRefinedLoopMesh), token =>
                Answer.One(Mesh.CreateRefinedLoopMesh(at.Mesh, of.Formula, new RefinementSettings { Level = at.Level, NakedEdgeMode = at.NakedEdgeMode, ContinueRequest = token })), at.Retain));

    // --- [FACETS]
    private static Seq<Seq<int>> Facets(IEnumerable<int[]?>? facets) =>
        Conversions.Rows(facets).Map(static facet => toSeq(facet)).Strict();
}
