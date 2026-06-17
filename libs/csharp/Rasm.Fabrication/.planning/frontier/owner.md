# [RASM_FABRICATION_OWNER]

The polymorphic `Fabrication` owner closes the entire 3D-to-fabrication concern over a `FrontierKind` `[SmartEnum<string>]` (`project`/`toolpath`/`place`) folded by one `Run` data-table dispatch that lowers a per-kind `FrontierPolicy` `[Union]` onto its kernel and returns a per-kind `FrontierResult` `[Union]` (`HiddenLineResult`/`Motion`/`Placement`). The owner holds the shared fabrication atoms every kernel reads — `Loop`, `Edge3`, `Move`, `PartTransform`, `FrontierInput` — and the one `Run` entrypoint that discriminates by `FrontierPolicy` case over a `FrozenDictionary` builder table, never a `policy switch` cascade. A per-concern projector/post/packer class triple is the deleted form: the three concerns differ only in their kernel fold, never in their entrypoint. The owner composes the kernel `Rasm/Geometry/geometry-kernel#ROBUST_PREDICATES` `Predicate.Orient2D` exact orientation as the winding floor and `Rasm`/Vectors `MeshSpace`/`Point3d`/`Vector3d`/`Matrix` primitives as native vocabulary — read public shapes, compose, never re-mint. It computes no hash, mints no second `Viewport2D`, and operates on raw coordinate doubles at the kernel interior because a coordinate is the domain's native scalar, never a unit-bearing quantity.

Wire posture: HOST-LOCAL. The fabrication outputs cross only the in-process seam — the world-space `HiddenLineResult` edge sets to the AppUi `Viewport2D` consumer, the `Motion` toolpath/joint stream to the `posting/program#CUT_PROGRAM` emitter, the `Placement` transforms to the same posting owner — never a browser or peer wire. The `FrontierKind` discriminant, the `FrontierPolicy`/`FrontierResult` unions, and the shared atoms are host-local types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                                                       |
| :-----: | :---------------- | :---------------------------------------------------------------------------------------------------------- |
|   [1]   | FABRICATION_OWNER | `FrontierKind`/`FrontierPolicy`/`FrontierResult` unions, the shared `Loop`/`Edge3`/`Move`/`PartTransform` atoms, and the one `Run` data-table fold to the cluster kernels |

## [2]-[FABRICATION_OWNER]

- Owner: `FrontierKind` `[SmartEnum<string>]` the frontier discriminant (`project`/`toolpath`/`place`); `FrontierPolicy` `[Union]` the per-kind policy (`HiddenLine`/`Cam`/`Nest`) the `Run` fold dispatches on; `FrontierResult` `[Union]` the per-kind result (`HiddenLineResult`/`Motion`/`Placement`); `FrontierInput` the one input carrier every kernel reads; `Fabrication` the static surface whose ONE `Run` entrypoint discriminates by `FrontierPolicy` case onto the cluster kernel. The cluster kernels (`Hlr`, `Cam`, `Nest`) are sibling owners in their own sub-domains, dispatched from the `Builders` table, never sibling public surfaces beside `Run`.
- Cases: `FrontierKind` rows `project` · `toolpath` · `place` (3); `FrontierPolicy` cases `HiddenLine` · `Cam` · `Nest` (3); `FrontierResult` cases `HiddenLineResult` · `Motion` · `Placement` (3).
- Entry: `public static Fin<FrontierResult> Run(FrontierPolicy policy, FrontierInput input)` — the ONE fabrication entrypoint; `Fin<T>` routes a `FabricationFault` (`NoFit`, `Unreachable`, `KerfCollision`, `OpenLoop`) or the composed kernel `GeometryFault` band-2400 (`DegenerateInput`), each lowered with `.ToError()` per `faults/faults#FAULT_BAND`; the fold lowers `HiddenLine` to `projection/hidden-line#PROJECTION_HIDDEN_LINE`, `Cam` to `toolpath/motion#CAM_MOTION`, and `Nest` to `nesting/nfp#NESTING`.
- Auto: `Run` reads a `FrozenDictionary<Type, Func<FrontierPolicy, FrontierInput, Fin<FrontierResult>>>` keyed by the policy case so kernel selection is a data-table row, never a `policy switch` cascade in the body. Each kernel composes the settled `Predicate.Orient2D` for the exact left-turn/segment-intersection floor and the `geometry2d/clipper#POLYGON_ALGEBRA` Clipper2 substrate for offset/clip/Minkowski, so the fabrication owner adds the fabrication ALGORITHM atop the settled exact-geometry and polygon-algebra substance rather than re-deriving either.
- Receipt: `FrontierResult` IS the typed evidence — `HiddenLineResult` carries the visible/hidden/silhouette edge partition, `Motion` carries the ordered move list plus the joint-angle stream and the IK convergence residual, `Placement` carries the per-part transform and the sheet utilization scalar; no generic fabrication ledger, each kind carries its own typed result.
- Packages: `Rasm`/Vectors (`MeshSpace`/`Point3d`/`Vector3d`/`Matrix` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D`/`Sign` — settled kernel), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new frontier is one `FrontierKind` row + one `FrontierPolicy` case + one `FrontierResult` case + one kernel fold arm + one `Builders` row; zero new entrypoint surface.
- Boundary: the frontier is the ONE polymorphic `Fabrication` owner and a per-concern projector/post/packer class triple is the deleted form — the three concerns differ only in their kernel fold, never in their entrypoint, so `Run` dispatches by `FrontierPolicy` case over the `Builders` table; the shared `Loop`/`Edge3`/`Move`/`PartTransform`/`FrontierInput` atoms live here and every kernel composes them, never re-mints a parallel atom; segment intersection and convex orientation read the settled `Predicate.Orient2D` exact sign and a naive `double` cross-product sign at the call site is the named robustness defect — a sign verdict is exact or it is a defect; the interior coordinate doubles inside every kernel are the sanctioned native-scalar posture and a unit-bearing quantity in a kernel signature is the seam violation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.Projection;
using Rasm.Fabrication.Toolpath;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rasm.Vectors;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Frontier;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<FabricationKeyPolicy, string>]
[KeyMemberComparer<FabricationKeyPolicy, string>]
public sealed partial class FrontierKind {
    public static readonly FrontierKind Project = new("project");
    public static readonly FrontierKind Toolpath = new("toolpath");
    public static readonly FrontierKind Place = new("place");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Loop(Arr<Point3d> Vertices, bool Closed) {
    public int Count => Vertices.Count;
    public Point3d At(int i) => Vertices[((i % Count) + Count) % Count];

    public Sign Winding() =>
        Sign.Of(Enumerable.Range(1, Count - 2).Sum(i => Predicate.Orient2D(At(0), At(i), At(i + 1)).Key));

    public Loop AsCcw() => Winding() == Sign.Negative ? this with { Vertices = Vertices.Rev().ToArr() } : this;
    public BoundingBox Bound() => new(Vertices);
}

public readonly record struct Edge3(Point3d A, Point3d B);

public readonly record struct Move(Point3d To, bool Rapid, double Feed);

public sealed record PartTransform(int PartId, double Tx, double Ty, double RotationRadians);

public readonly record struct FrontierInput(
    Option<MeshSpace> Model,
    ProjectionDir View,
    Arr<Loop> Profiles,
    Arr<DhJoint> Chain,
    Point3d IkTarget,
    SheetBounds Sheet);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrontierPolicy {
    private FrontierPolicy() { }

    public sealed record HiddenLine(double FacetTolerance, int SpatialLeaf) : FrontierPolicy;
    public sealed record Cam(ToolpathKind Kind, double StepOver, double ToolRadius, int Passes, IkPolicy Ik) : FrontierPolicy;
    public sealed record Nest(NestPolicy Nesting) : FrontierPolicy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrontierResult {
    private FrontierResult() { }

    public sealed record HiddenLineResult(Seq<Edge3> Visible, Seq<Edge3> Hidden, Seq<Edge3> Silhouette) : FrontierResult;
    public sealed record Motion(Seq<Move> Moves, Seq<double[]> Joints, double IkResidual, bool Reached) : FrontierResult;
    public sealed record Placement(Seq<PartTransform> Parts, double Utilization, int Unplaced) : FrontierResult;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Fabrication {
    static readonly FrozenDictionary<Type, Func<FrontierPolicy, FrontierInput, Fin<FrontierResult>>> Builders =
        new (Type Case, Func<FrontierPolicy, FrontierInput, Fin<FrontierResult>> Run)[] {
            (typeof(FrontierPolicy.HiddenLine), static (p, i) => Hlr.Solve((FrontierPolicy.HiddenLine)p, i)),
            (typeof(FrontierPolicy.Cam), static (p, i) => Cam.Solve((FrontierPolicy.Cam)p, i)),
            (typeof(FrontierPolicy.Nest), static (p, i) => Nest.Solve((FrontierPolicy.Nest)p, i)),
        }.ToFrozenDictionary(static row => row.Case, static row => row.Run);

    public static Fin<FrontierResult> Run(FrontierPolicy policy, FrontierInput input) =>
        Builders.TryGetValue(policy.GetType(), out var run)
            ? run(policy, input)
            : Fin.Fail<FrontierResult>(GeometryFault.DegenerateInput($"frontier-policy-miss:{policy.GetType().Name}").ToError());
}
```
