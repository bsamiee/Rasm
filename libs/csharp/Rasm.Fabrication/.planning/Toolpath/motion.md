# [RASM_FABRICATION_MOTION]

The CAM-motion owner: the toolpath motion kernel one `Cam` static fold dispatches over the `(RemovalModality, CutStrategy)` cross-product — the engagement `CutStrategy` (the `Process/family#PROCESS_FAMILY` axis: `boundary-pass`/`pocket-clear`/`peck`/`adaptive`/`radial-sweep`/`plunge-dwell`/`helical`/`layer-walk`) lands its move geometry ONCE and the `RemovalModality` off `input.Process` selects the envelope, so a process-agnostic strategy stops masquerading as a process-bound row: the milling `boundary-pass`/`pocket-clear`/`peck`/`adaptive`, the turning `radial-sweep`/`plunge-dwell`/`helical` (a 1-axis radial sweep over the `BarStock` envelope), the thermal `boundary-pass` (laser/plasma/waterjet sharing the contour generator, the pierce/lead differing by modality and conditioned at posting), and the additive `layer-walk` (the perimeter-and-infill reading the `Toolpath/slicing#SLICING` layer set) are all ONE strategy row read across every admitting modality, the `RemovalModality.Admits(CutStrategy)` relation routing an inadmissible pair (turning's `radial-sweep` on a `thermal` laser) to `FabricationFault.InadmissiblePair` rather than a silent empty move set. `boundary-pass` and `pocket-clear` route their offsetting through the `Polygon/clipper#POLYGON_ALGEBRA` Clipper2 substrate — the constant-offset contour rings and the inward pocket clearing are integer-robust polygon offsets, never a hand-rolled per-vertex-normal `OffsetRing` that self-intersects on a reflex vertex. The `adaptive` strategy is the dominant HEM (high-efficiency machining) class — an adaptive-clearing toolpath holding constant material-removal rate and radial engagement — driven by the `Toolpath/skeleton#STRAIGHT_SKELETON` straight-skeleton/medial-axis primitive, the one place no managed library exists and the author-kernel posture is correct and forward; the constant-engagement step is realized over the linear `Move` chord stream and its variable-radius circular-arc identity is recovered at posting by the `Posting/program#CUT_PROGRAM` `BIARC_ARC_EMISSION` biarc refit, the `Move` carrying an `Option<ArcCenter>` column the refit fills so a `G2`/`G3` arc block emits where the chord run is curvature-faithful. The generator reads its per-process budget from the `Process/physics#CUT_PARAMETER` `RemovalBudget` case (the `SubtractiveBudget` MRR for `adaptive`, the `ThermalBudget` cut-speed for the thermal `boundary-pass`, the `AdditiveBudget` layer geometry for `layer-walk`) selected by the `Process.RemovalModality`. The kernel composes the `Process/owner#FABRICATION_OWNER` `Loop`/`Move`/`FabricationPolicy.Cam`/`FabricationResult.Motion` shared vocabulary, hands each serial-chain-targeting move set to the `Toolpath/kinematics#ROBOT_CELL` `RobotProgram.Solve` (the admitted `Robots` look-ahead `Program` owning the FK/IK, the joint-limit/singularity/reach validation, and the cell-dialect post; the gantry-driven `radial-sweep`/thermal/`layer-walk` kinds whose `input.Cell` is `None` take the non-IK arm directly), and reads the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact orientation where a side verdict is needed. It is dispatched by the `Process/owner#FABRICATION_OWNER` `Run` fold's `Cam` policy case; it mints no second owner surface, computes no hash, and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `Motion` toolpath/joint stream crosses only the in-process seam to the `Posting/program#CUT_PROGRAM` emitter — never a browser or peer wire.

## [01]-[INDEX]

- [01]-[CAM_MOTION]: owns the `(RemovalModality, CutStrategy)` cross-product move generators over the Geometry2D offset and the `Cam` fold handing each serial-chain move set to the `Toolpath/kinematics#ROBOT_CELL` robot-cell solve; one motion owner over the strategy×modality dispatch, the `CutStrategy` axis itself owned by `Process/family#PROCESS_FAMILY`.

## [02]-[CAM_MOTION]

- Owner: `Cam` the static motion fold over the `(RemovalModality, CutStrategy)` pair (the modality read off `input.Process`, the strategy off `policy.Strategy`) generating the cut moves through the `Generate` generated total `Switch`, then handing each serial-chain-targeting move set to the `Toolpath/kinematics#ROBOT_CELL` `RobotProgram.Solve` and emitting the `Motion` joint stream; `ArcCenter` the readonly arc-identity column the `Move` carries (`Option<ArcCenter>`) recording the `(Center, Clockwise)` a constant-engagement arc segment resolves to, the linear chord stream the `Posting/program#CUT_PROGRAM` `BIARC_ARC_EMISSION` biarc refit reads; `EngagementPolicy` the constant-engagement knobs (`TargetAngle`/`MaxAxialDepth`). The `CutStrategy` engagement axis and the `RemovalModality.Admits(CutStrategy)` relation are owned at `Process/family#PROCESS_FAMILY` — this page composes them, never re-mints a parallel motion enum.
- Cases: the `(RemovalModality, CutStrategy)` `Generate` arms — `boundary-pass` (constant-offset boundary passes via Geometry2D `Offset`, the milling-contour/thermal-contour/routing strategy a modality envelopes: a `subtractive` modality cuts the rings, a `thermal` modality the pierce/lead conditioned at posting) · `pocket-clear` (inward continuous spiral via repeated Geometry2D `Offset` rings) · `peck` (peck-cycle point set) · `adaptive` (adaptive-clearing HEM over the straight-skeleton medial axis, constant MRR and radial engagement, emitting the `Option<ArcCenter>`-tagged chord stream) · `radial-sweep` (a 1-axis radial sweep over the `BarStock` revolved envelope, the lathe rough/finish/face Z-vs-radius pass) · `plunge-dwell` (the groove plunge-and-dwell radial cut) · `helical` (a constant-lead helical sweep, lathe threading and helical entry) · `layer-walk` (the additive perimeter-and-infill move set walking the `Toolpath/slicing#SLICING` layer contour at the `AdditiveBudget` layer height) (8 strategy arms × the modality envelope), the `(RemovalModality, CutStrategy)` cross-product PARTIAL — an inadmissible pair routes `FabricationFault.InadmissiblePair`, never an empty move set.
- Entry: `public static Fin<FabricationResult> Solve(FabricationPolicy.Cam policy, FabricationInput input)` — `Fin<T>` routes `FabricationFault.InadmissiblePair` when `input.Process.Modality.Admits(policy.Strategy)` is false, `FabricationFault.OpenLoop` on a non-closed toolpath boundary, the kernel `GeometryFault.DegenerateInput` on an empty profile, and `FabricationFault.Unreachable` when a reach-strict `CellPolicy` robot-cell solve folds a non-empty `Robots` `Program.Errors` (the routing the `Toolpath/kinematics#ROBOT_CELL` owner performs, propagated through the `Fin`), each lowered with `.ToError()`; the body gates the `(RemovalModality, CutStrategy)` pair against `Admits`, dispatches it to the move generator, then hands the serial-chain move set to the `Toolpath/kinematics#ROBOT_CELL` solve, emitting the `Motion` joint stream.
- Auto: `Cam.Solve` reads `policy.Strategy` and `input.Process.Modality`, and on `Admits` success dispatches the `CutStrategy` through the generated total `Switch` in `Generate`, threading the `(policy, loop, modality)` state into each arm — `boundary-pass` folds the boundary loop inward by `ToolRadius + k·StepOver` constant Geometry2D offsets for `Passes` rings (the thermal modality emitting one ring, the pierce/lead owned at posting, so the arm is modality-thin); `pocket-clear` generates the inward clearing as successive Geometry2D offset rings stitched into one continuous path so the cutter never lifts; `peck` emits a peck point per profile centroid with retract moves between; `adaptive` reads the `Toolpath/skeleton#STRAIGHT_SKELETON` medial axis of the pocket, then walks it with a variable radial step sized per point from the `StraightSkeleton.ClearanceAt` local channel half-width and the `EngagementPolicy.TargetAngle` so the cutter holds constant radial engagement (a wide channel takes a coarse step, a narrowing channel a finer step), the per-pass step further bounded by the `EngagementPolicy.MaxAxialDepth` stickout-derived cap, and each curving arc segment tagged with its `ArcCenter` (the bisector-normal circle center through the segment endpoints) on the emitted `Move` so the posting biarc refit recovers the `G2`/`G3` block — the constant-engagement HEM strategy a uniform `len/stepOver` march cannot give since it ignores the channel width; `radial-sweep` sweeps the boundary as a 1-axis radial pass over the `BarStock` envelope (a Z-vs-radius profile, not a 2D-pocket offset) so the lathe tool walks the diameter profile in successive depth-of-cut steps; `plunge-dwell` plunges to the groove centroid and dwells; `helical` sweeps the constant-lead helix; `layer-walk` walks the `Toolpath/slicing#SLICING` layer contour set as the per-layer perimeter-and-infill move sequence at the `AdditiveBudget` layer height. After move generation the fold hands the serial-chain move set to the `Toolpath/kinematics#ROBOT_CELL` `RobotProgram.Solve` when `input.Cell` is present — the admitted `Robots` look-ahead `Program` solving every waypoint's FK/IK, threading the previous-pose continuation internally (the wrist-flip / redundant-axis disambiguation the planner owns), and posting the cell dialect; under a permissive `CellPolicy` it emits the `Motion` carrying the per-target joint trajectory, the planned `Duration`, the posted cell `Code`, and the reached flag, and under a reach-strict `CellPolicy` a non-empty `Program.Errors` (unreachable / joint-limit / singularity) routes `FabricationFault.Unreachable` — the robot-cell solve the one producer of `Unreachable`, the reach contract the cell's own joint-limit/singularity/reach validation, not a hand-rolled residual; the gantry-driven `radial-sweep`/thermal/`layer-walk` kinds whose `input.Cell` is `None` take the non-IK arm directly (the empty-cell `Motion` carrying the bare move stream the `Posting/program#CUT_PROGRAM` G-code emitter renders), the robot-cell drive reserved for the `articulated-arm` serial-chain machines.
- Receipt: the `Motion` carries the ordered `Move` list (rapid/feed with feedrate plus the `Option<ArcCenter>` arc identity), the per-target joint-angle stream, the final IK position residual, and the reached flag — the typed motion evidence the posting owner consumes; no generic motion ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, the side verdict), Clipper2 (via `Polygon/clipper#POLYGON_ALGEBRA` — the contour/pocket offset), `Process/family#PROCESS_FAMILY` (`CutStrategy`/`RemovalModality.Admits` — composed), `Toolpath/kinematics#ROBOT_CELL` (`RobotProgram.Solve` — the `Robots` robot-cell FK/IK + look-ahead-program seam the `articulated-arm` path composes, the joint trajectory and posted cell `Code` read back boundary-mapped), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new engagement strategy is one `CutStrategy` row at `Process/family#PROCESS_FAMILY` plus one `Generate` `Switch` arm here and its addition to every admitting modality's `Strategies` set, the generated dispatch breaking the build until the arm lands — a new process reusing an existing strategy adds ZERO arms (a routing contour and a thermal contour are both `boundary-pass`); a collision-aware retract is one `Move`-fold arm reading the settled `SpatialIndex` (the `Toolpath/guard#GUARD` `Lift` verdict the `Guard.Check` per-move gate produces); a 5-axis tilt strategy is one orientation column on the `adaptive` arm; the variable-radius G2/G3 arc emission of the constant-engagement walk is the realized `Option<ArcCenter>` `Move` column the `BIARC_ARC_EMISSION` posting fold renders, the linear `Move` sample stream the chord input the biarc fit refits; zero new surface.
- Boundary: CAM is the ONE motion owner over the `(RemovalModality, CutStrategy)` pair and a `ContourPath`/`PocketPath`/`DrillCycle`/`TurningPass`/`ThermalPath`/`SlicePath` sibling family is the deleted form — every process toolpath is one `Generate` `Switch` arm; the flat 11-row `ToolpathKind` conflating strategy with modality (turn-rough/turn-finish/face/groove/thread/thermal-contour/slice-layer beside contour/pocket/drill/trochoidal) is the deleted form this factoring retires — a strategy lands once on the `CutStrategy` axis and the modality envelopes it, the turning rows collapsing onto `radial-sweep`/`plunge-dwell`/`helical`, the thermal contour onto `boundary-pass`, the additive walk onto `layer-walk`, never an axis re-encoding the modality; the `(RemovalModality, CutStrategy)` cross-product is gated by the `Process/family#PROCESS_FAMILY` `RemovalModality.Admits(CutStrategy)` relation and a silent empty move set on an inadmissible pair is the deleted form — the dispatch queries `Admits` and routes `FabricationFault.InadmissiblePair`; the per-strategy behavior lives in the `Generate` generated total `Switch` arm and a parallel `Spiral`/`Adaptive`/`Thermal` boolean column beside the strategy the dispatch already reads is the deleted form — one axis carries one discriminant, never a second flag the arm re-derives; the thermal `boundary-pass` SHARES the contour generator and a parallel thermal-only contour kernel is the deleted form — the pierce/lead conditioning is owned at posting, not a second offset routine; the boundary-pass and pocket-clear offsetting route the one `Polygon/clipper#POLYGON_ALGEBRA` Clipper2 owner and a hand-rolled `OffsetRing` is the deleted form; the `adaptive` clearing reads the `Toolpath/skeleton#STRAIGHT_SKELETON` medial-axis primitive and a per-vertex spiral approximation of HEM is the rejected form; the radial step holds constant engagement off the `ClearanceAt` local channel half-width and a uniform `stepOver` march ignoring the channel width is the deleted form — the engagement walk reads the wavefront clearance field the skeleton already encodes, never a re-derived distance transform, and the per-pass step is bounded by the `EngagementPolicy.MaxAxialDepth` stickout cap; the variable-radius arc identity rides the `Option<ArcCenter>` `Move` column the posting biarc refit reads and a CAM-side G2/G3 word emission is the deleted form — the move stream stays linear chords, the arc recovered ONCE at posting, never a second arc-aware offset call site; the `layer-walk` generator reads the `Toolpath/slicing#SLICING` layer set and a CAM-local re-slice is the deleted form; the serial-chain FK/IK is owned at `Toolpath/kinematics#ROBOT_CELL` (the admitted `Robots` cell solve) and a CAM-local kinematics re-mint or a hand-rolled DH/IK solver is the deleted form — the `Move` stream hands to `RobotProgram.Solve`, never a re-derived Jacobian; the side verdict reads `Predicate.Orient2D` exact sign and a `double` cross at the call site is the named robustness defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.ProcessModel;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] ------------------------------------------------------------------------------
public readonly record struct EngagementPolicy(double TargetAngle, double MaxAxialDepth) {
    public static readonly EngagementPolicy Default = new(TargetAngle: 60.0, MaxAxialDepth: double.PositiveInfinity);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Cam {
    public static Fin<FabricationResult> Solve(FabricationPolicy.Cam policy, FabricationInput input) =>
        !input.Process.Modality.Admits(policy.Strategy)
            ? Fin.Fail<FabricationResult>(FabricationFault.InadmissiblePair($"cam:{input.Process.Modality.Key}-rejects-{policy.Strategy.Key}").ToError())
            : input.Profiles.IsEmpty
                ? Fin.Fail<FabricationResult>(GeometryFault.DegenerateInput("cam:no-profile").ToError())
                : input.Profiles.Find(static l => !l.Closed).Match(
                    Some: _ => Fin.Fail<FabricationResult>(FabricationFault.OpenLoop("cam:open-boundary").ToError()),
                    None: () => {
                        Seq<Move> moves = toSeq(input.Profiles).Bind(loop => Generate(policy, loop.AsCcw()));
                        // The articulated-arm path hands the TCP Move stream to the Robots cell solve
                        // (the look-ahead Program owning the FK/IK, the previous-pose continuation, and
                        // the Unreachable fault); a gantry/lathe (Cell None) takes the non-IK arm directly.
                        return input.Cell.Match(
                            None: () => Fin.Succ((FabricationResult)new FabricationResult.Motion(moves, Seq<double[]>(), 0.0, true, Seq<string>())),
                            Some: cell => RobotProgram.Solve(cell, moves, policy.Cell)
                                .Map(cm => (FabricationResult)new FabricationResult.Motion(moves, cm.Joints, cm.Duration, cm.Reached, cm.Code)));
                    });

    static Seq<Move> Generate(FabricationPolicy.Cam p, Loop loop) =>
        p.Strategy.Switch(
            state:        (p, loop),
            boundaryPass: static s => Contour(s.loop, s.p.ToolRadius, s.p.StepOver, s.p.Passes),
            pocketClear:  static s => Pocket(s.loop, s.p.ToolRadius, s.p.StepOver),
            peck:         static s => Peck(s.loop, s.p.ToolRadius),
            adaptive:     static s => Adaptive(s.loop, s.p.ToolRadius, s.p.StepOver, s.p.Engagement),
            radialSweep:  static s => Turn(s.loop, s.p.ToolRadius, s.p.StepOver, s.p.Passes),
            plungeDwell:  static s => Plunge(s.loop, s.p.ToolRadius),
            helical:      static s => Turn(s.loop, s.p.ToolRadius, s.p.StepOver, s.p.Passes),
            layerWalk:    static s => SliceWalk(s.loop));

    static Seq<Move> Turn(Loop profile, double radius, double stepOver, int passes) =>
        toSeq(Enumerable.Range(0, Math.Max(1, passes)))
            .Bind(k => toSeq(profile.Vertices)
                .Map(v => new Move(new Point3d(v.X, Math.Max(0.0, v.Y - radius - k * stepOver), 0.0), Rapid: false, Feed: 1.0)));

    static Seq<Move> Plunge(Loop profile, double radius) {
        Point3d c = Centroid(profile);
        return Seq(new Move(c with { Y = c.Y + 5.0 }, Rapid: true, Feed: 0.0), new Move(c, Rapid: false, Feed: 0.3));
    }

    static Seq<Move> SliceWalk(Loop layerContour) =>
        toSeq(layerContour.AsCcw().Vertices).Map(v => new Move(v, Rapid: false, Feed: 1.0));

    static Seq<Move> Contour(Loop loop, double radius, double stepOver, int passes) =>
        toSeq(Enumerable.Range(0, Math.Max(1, passes)))
            .Bind(k => PolygonAlgebra.Offset(Seq(loop), -(radius + k * stepOver), OffsetEnds.Polygon).IfFail(Seq<Loop>()))
            .Bind(ring => toSeq(ring.Vertices))
            .Map(p => new Move(p, Rapid: false, Feed: 1.0));

    static Seq<Move> Pocket(Loop loop, double radius, double stepOver) {
        Seq<Move> Rings(double depth) =>
            PolygonAlgebra.Offset(Seq(loop), -(radius + depth), OffsetEnds.Polygon).Match(
                Succ: ring => ring.IsEmpty
                    ? Seq<Move>()
                    : toSeq(ring).Bind(r => toSeq(r.Vertices)).Map(p => new Move(p, Rapid: false, Feed: 1.0)).Concat(Rings(depth + stepOver)),
                Fail: _ => Seq<Move>());
        return Rings(0.0);
    }

    static Seq<Move> Peck(Loop loop, double radius) {
        Point3d c = Centroid(loop);
        return Seq(new Move(c with { Z = c.Z + 5.0 }, Rapid: true, Feed: 0.0), new Move(c, Rapid: false, Feed: 0.5));
    }

    static Seq<Move> Adaptive(Loop loop, double radius, double stepOver, EngagementPolicy engage) =>
        StraightSkeleton.MedialAxis(loop).Match(
            Succ: axis => axis.Bind(seg => Engage(loop, seg, radius, stepOver, engage)),
            Fail: _ => Seq<Move>());

    static Seq<Move> Engage(Loop loop, Edge3 seg, double radius, double stepOver, EngagementPolicy engage) {
        double len = seg.A.DistanceTo(seg.B);
        double half = Math.Max(radius, 1e-6);
        double engageRatio = Math.Clamp(engage.TargetAngle / 180.0, 1e-3, 1.0);
        double axialCap = double.IsPositiveInfinity(engage.MaxAxialDepth) ? double.MaxValue : Math.Max(1e-3, engage.MaxAxialDepth);
        return toSeq(Walk(loop, seg, len, half, radius, stepOver, engageRatio, axialCap, t: 0.0, first: true));
    }

    // The walk emits linear chords; an adaptive turn carries its osculating ArcCenter so the
    // posting biarc refit recovers the G2/G3 block — never a CAM-side arc word.
    static IEnumerable<Move> Walk(Loop loop, Edge3 seg, double len, double half, double radius, double stepOver, double engageRatio, double axialCap, double t, bool first) {
        double s = t * len;
        Point3d here = seg.A + (s / Math.Max(len, 1e-9)) * (seg.B - seg.A);
        Vector3d dir = seg.B - seg.A; dir.Unitize();
        Vector3d nrm = new(-dir.Y, dir.X, 0.0);
        double clearance = StraightSkeleton.ClearanceAt(loop, here).IfFail(half);
        Option<ArcCenter> arc = first || clearance <= 1e-6 ? None : Some(new ArcCenter(here + clearance * nrm, Clockwise: false));
        yield return new Move(here, Rapid: first, Feed: 1.0, Arc: arc);
        if (s >= len) yield break;
        double radialStep = Math.Max(1e-3, Math.Min(axialCap, engageRatio * Math.Min(clearance, radius + stepOver)));
        foreach (Move m in Walk(loop, seg, len, half, radius, stepOver, engageRatio, axialCap, t + radialStep / Math.Max(len, 1e-9), first: false))
            yield return m;
    }

    static Point3d Centroid(Loop loop) =>
        loop.Vertices.Fold(Point3d.Origin, static (acc, v) => acc + v) / Math.Max(1, loop.Count);
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Process["Process.RemovalModality"] -->|"Admits(CutStrategy) gate"| Cam["Cam"]
    Policy["policy.Strategy CutStrategy"] -->|"(Modality, CutStrategy)"| Cam
    Profiles["Profiles"] --> Cam
    Cam -->|boundary-pass / pocket-clear| Offset["Geometry2D Offset"]
    Cam -->|adaptive| Skeleton["StraightSkeleton medial axis"]
    Cam -->|radial-sweep / plunge-dwell / helical| Radial["BarStock radial sweep"]
    Cam -->|layer-walk| Slice["Toolpath/slicing layer set"]
    Cam -.->|inadmissible pair| Fault["FabricationFault.InadmissiblePair"]
    Skeleton -->|Option&lt;ArcCenter&gt;-tagged chords| Drive["robot-cell solve (input.Cell Some)"]
    Offset -->|serial-chain moves| Drive
    Radial -->|gantry moves| Direct["non-IK arm (input.Cell None)"]
    Slice -->|layer moves| Direct
    Drive -->|Robots Program look-ahead plan| Motion["Motion · joints + cell Code"]
    Direct -->|gantry stream| Motion
    Motion -.->|chord run + ArcCenter| Biarc["Posting biarc refit G2/G3"]
```
