# [RASM_FABRICATION_OWNER]

The polymorphic `Fabrication` owner closes the entire 3D-to-fabrication concern over the `FrontierPolicy` `[Union]` (`HiddenLine`/`Cam`/`Nest`) — the one discriminant `Run` reads — folded by one `Run` generated total `Switch` that lowers each case onto its kernel and returns a per-case `FrontierResult` `[Union]` (`HiddenLineResult`/`Motion`/`Placement`). The owner holds the shared fabrication atoms every kernel reads — `Loop`, `Edge3`, `Move`, `PartTransform`, `FrontierInput` — and the one `Run` entrypoint that discriminates by `FrontierPolicy` case through the generated `Switch`, never a `policy switch` cascade or a `Type`-keyed dictionary that loses compile-time totality. The `FrontierInput` carries the selected `Process/family#PROCESS_FAMILY` `Process`/`Machine` axis pair as input state every kernel reads to select its per-process behavior (the `RemovalModality` the `Process/physics#CUT_PARAMETER` budget switches on, the toolpath generator the `(Process, ToolpathKind)` pair dispatches), and the `Nesting/nfp#NESTING` `Stock` union as the stock the nest and post read — `Run` is unchanged and no second frontier discriminant rides beside the `FrontierPolicy` union: the process/machine selection is policy DATA on the existing input, not a parallel entrypoint axis. A per-concern projector/post/packer class triple is the deleted form: the three concerns differ only in their kernel fold, never in their entrypoint. A parallel string `[SmartEnum]` frontier discriminant beside the `FrontierPolicy` union is the deleted form — one axis carries one discriminant, the union case the dispatch already reads, never a second name the union case projects onto. The owner composes the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact orientation as the winding floor and `Rasm`/Vectors `MeshSpace`/`Point3d`/`Vector3d`/`Matrix` primitives as native vocabulary — read public shapes, compose, never re-mint. It computes no hash, mints no second `Viewport2D`, and operates on raw coordinate doubles at the kernel interior because a coordinate is the domain's native scalar, never a unit-bearing quantity.

Wire posture: HOST-LOCAL. The fabrication outputs cross only the in-process seam — the world-space `HiddenLineResult` edge sets to the AppUi `Viewport2D` consumer, the `Motion` toolpath/joint stream to the `Posting/program#CUT_PROGRAM` emitter, the `Placement` transforms to the same posting owner — never a browser or peer wire. The `FrontierPolicy`/`FrontierResult` unions and the shared atoms are host-local types that never sit between wire and rail.

## [1]-[INDEX]

- [1]-[FABRICATION_OWNER]: owns the `FrontierPolicy`/`FrontierResult` unions, the shared `Loop`/`Edge3`/`Move`/`PartTransform`/`FrontierInput` atoms, and the one `Run` generated total `Switch` to the cluster kernels.

## [2]-[FABRICATION_OWNER]

- Owner: `FrontierPolicy` `[Union]` the per-concern policy (`HiddenLine`/`Cam`/`Nest`) — the ONE frontier discriminant the `Run` fold dispatches on; `FrontierResult` `[Union]` the per-case result (`HiddenLineResult`/`Motion`/`Placement`); `FrontierInput` the one input carrier every kernel reads, carrying the selected `Process`/`Machine` axis pair (the `Process/family#PROCESS_FAMILY` removal-modality/kinematic discriminant) and the `Nesting/nfp#NESTING` `Stock` union as input state beside the geometry/chain atoms; `Fabrication` the static surface whose ONE `Run` entrypoint discriminates by `FrontierPolicy` case onto the cluster kernel. The cluster kernels (`Hlr`, `Cam`, `Nest`) are sibling owners in their own sub-domains, dispatched from the generated total `Switch`, never sibling public surfaces beside `Run`.
- Cases: `FrontierPolicy` cases `HiddenLine` · `Cam` · `Nest` (3); `FrontierResult` cases `HiddenLineResult` · `Motion` · `Placement` (3); each policy case pairs one-to-one with its result case across the fold, the union case the sole discriminant, never a parallel string-keyed kind.
- Entry: `public static Fin<FrontierResult> Run(FrontierPolicy policy, FrontierInput input)` — the ONE fabrication entrypoint; `Fin<T>` routes a `FabricationFault` (`NoFit`, `Unreachable`, `KerfCollision`, `OpenLoop`) or the composed kernel `GeometryFault` band-2400 (`DegenerateInput`), each lowered with `.ToError()` per `Process/faults#FAULT_BAND`; the fold lowers `HiddenLine` to `Posting/projection#PROJECTION_HIDDEN_LINE`, `Cam` to `Toolpath/motion#CAM_MOTION`, and `Nest` to `Nesting/nfp#NESTING`.
- Auto: `Run` dispatches through the Thinktecture-generated total `Switch(state, hiddenLine, cam, nest)` threading the `FrontierInput` as state into each case-typed arm, so kernel selection is the closed-family dispatch the union owns and a new `FrontierPolicy` case fails the build until its arm lands, never a `policy switch` cascade or a `Type`-keyed lookup that silently misses. Each kernel composes the settled `Predicate.Orient2D` for the exact left-turn/segment-intersection floor and the `Polygon/clipper#POLYGON_ALGEBRA` Clipper2 substrate for offset/clip/Minkowski, so the fabrication owner adds the fabrication ALGORITHM atop the settled exact-geometry and polygon-algebra substance rather than re-deriving either.
- Receipt: `FrontierResult` IS the typed evidence — `HiddenLineResult` carries the visible/hidden/silhouette edge partition, `Motion` carries the ordered move list plus the joint-angle stream and the IK convergence residual, `Placement` carries the per-part transform and the sheet utilization scalar; no generic fabrication ledger, each kind carries its own typed result.
- Packages: `Rasm`/Vectors (`MeshSpace`/`Point3d`/`Vector3d`/`Matrix` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D`/`Sign` — settled kernel), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new frontier is one `FrontierPolicy` case + one `FrontierResult` case + one `Run` `Switch` arm lowering to its kernel fold; the generated dispatch breaks the build until the arm lands; a new process or machine is one row on the `Process/family#PROCESS_FAMILY` `Process`/`Machine` axis the `FrontierInput` already carries, never a `FrontierInput` shape change; zero new entrypoint surface. The `FrontierInput` `Process`/`Machine` state lands first and the `Sheet`-rectangle-to-`Stock`-union field is the second settled change on this record (the `Nesting/nfp#NESTING` `Stock` union replacing the old `SheetBounds`), the two together the final input shape every kernel reads.
- Boundary: the frontier is the ONE polymorphic `Fabrication` owner and a per-concern projector/post/packer class triple is the deleted form — the three concerns differ only in their kernel fold, never in their entrypoint, so `Run` dispatches by `FrontierPolicy` case through the generated total `Switch`; the `FrontierPolicy` union case is the ONE frontier discriminant and a parallel `FrontierKind` string `[SmartEnum]` carrying the same `project`/`toolpath`/`place` axis the union already cases is the deleted form — the generated `Switch` reads the union case directly, never a second key the case projects onto, and a `Type`-keyed `FrozenDictionary` dispatch is the rejected form because it loses the compile-time totality the closed family owns; the shared `Loop`/`Edge3`/`Move`/`PartTransform`/`FrontierInput` atoms live here and every kernel composes them, never re-mints a parallel atom — the `Loop.Covers` exact point-in-CCW-polygon containment is the ONE owner the nesting `Remnant.Holds`/`NoFitPolygon.Feasible`, the posting `Encloses`, and the fixturing `ExclusionZone.Covers` all read, never a per-page hand-rolled `Orient2D` containment loop; segment intersection and convex orientation read the settled `Predicate.Orient2D` exact sign and a naive `double` cross-product sign at the call site is the named robustness defect — a sign verdict is exact or it is a defect; the interior coordinate doubles inside every kernel are the sanctioned native-scalar posture and a unit-bearing quantity in a kernel signature is the seam violation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.ProcessModel;
using Rasm.Fabrication.Projection;
using Rasm.Fabrication.Toolpath;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rasm.Vectors;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Frontier;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Loop(Arr<Point3d> Vertices, bool Closed) {
    public int Count => Vertices.Count;
    public Point3d At(int i) => Vertices[((i % Count) + Count) % Count];

    public Sign Winding() =>
        Sign.Of(Enumerable.Range(1, Count - 2).Sum(i => Predicate.Orient2D(At(0), At(i), At(i + 1)).Key));

    public Loop AsCcw() => Winding() == Sign.Negative ? this with { Vertices = Vertices.Rev().ToArr() } : this;
    public BoundingBox Bound() => new(Vertices);

    public bool Covers(Point3d p) {
        Loop ccw = AsCcw();
        for (int i = 0; i < ccw.Count; i++)
            if (Predicate.Orient2D(ccw.At(i), ccw.At(i + 1), p) == Sign.Negative) return false;
        return true;
    }
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
    Stock Stock,
    Process Process,
    Machine Machine);

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
    public static Fin<FrontierResult> Run(FrontierPolicy policy, FrontierInput input) =>
        policy.Switch(
            state:      input,
            hiddenLine: static (i, p) => Hlr.Solve(p, i),
            cam:        static (i, p) => Cam.Solve(p, i),
            nest:       static (i, p) => Nest.Solve(p, i));
}
```
