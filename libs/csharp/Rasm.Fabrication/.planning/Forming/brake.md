# [RASM_FABRICATION_BEND_SEQUENCE]

The press-brake planning owner: `BendSequence.Plan` orders the `Forming/sheet#FLAT_PATTERN` bend-line set into an executable brake program — the best-first search over the per-state feasibility matrix, emitting the `BendStep` atoms rows (`Order`/`Line`/`AngleDeg`/`RadiusMm`/`KFactor`/`OverbendDeg`/`TonnageKn`/`Flip`) the `Run(Form)` case body mints into `FormedResult` under the `bend-program` content key. Die selection is table law: the die opening `V = f·T` from the thickness-band rows (`T ≤ 3 → f=8`, `3 < T ≤ 10 → f=10`, `T > 10 → f=12`), the air-bend working radius `Ri ≈ 0.16·V` (the working radius DEFEATS the nominal drawing radius — the plan re-projects `BA` through `FlatPattern.Project` when `Ri` displaces `R`), and the minimum flange `b ≥ c(A)·V` from the angle-band rows (`≥90° → 0.7`, `60-90° → 0.9`, `45-60° → 1.1`, `30-45° → 1.5`). Air-bend tonnage is `F = (C·Rm·S²·L)/(V·1000)` in kN with `C = 1.33`, `Rm` the physics `RemovalBudget.Formed.TensileRm`, `S` thickness, `L` bend length, `V` the die opening — scaled by the `BendMethod` tonnage multiplier (`air ×1` · `bottoming ×4` · `coining ×8`); a bend whose demand exceeds the machine envelope routes `TonnageExceeded` 2742. Springback resolves to a per-bend overbend: `OverbendDeg = A·(1 − Ks)/Ks · methodScale` with `Ks` the `Formed.SpringbackRatio` (air carries full springback, bottoming half, coining a tenth) — the overbend rides the `BendStep` row, never a post-pass correction.

The sequence search is state-space, not heuristic-ordered: a state is the set of completed bends plus part orientation; a candidate bend is FEASIBLE when the back gauge reaches its reference edge (`X ≤ GaugeTravelMm`, the gauged flange still flat), no intermediate flange sweeps the die/ram section (the folded-profile silhouette against the tooling section — the 2D check composes `Geometry2D/algebra` clipping and `Loop.Covers`, never a bespoke intersector), and no prior upstanding flange occludes the gauge face; expansion orders by flips-then-gauge-moves cost and the first goal state IS the plan (`BendStep.Flip` marks each reorientation). Search exhaustion routes `BendSequenceInfeasible` 2741 carrying the blocking bend and the tried-order count. The machine envelope (`CapacityKn`/`GaugeTravelMm`/`OpenHeightMm`) is a projected row from the `Kinematics/fleet#MACHINE_FLEET` capability registry — the brake never keeps a private machine table; the `press-brake-cnc` `Machine` row admits the `press-brake` `ProcessKind` and the envelope arrives as fleet-registry data at the fold.

Wire posture: HOST-LOCAL. The plan crosses only as `Seq<BendStep>` on `FormedResult` plus the `bend-program` `ContentKey`; bender-native program text is a dialect concern that stays OFF this page — the step rows are the neutral model exactly as `CutProgram` is posting's.

## [01]-[BEND_SEQUENCE]

- [01]-[BEND_SEQUENCE]: owns the `BendMethod` axis with its tonnage/springback columns, the `DieRow`/`FlangeRow` selection tables, the `BrakeEnvelope` projected machine row, the tonnage/overbend/flange formulas, and the ONE `BendSequence.Plan` best-first fold from `UnfoldResult` to the ordered `Seq<BendStep>` — the brake half the `Run(Form)` case body composes after `Forming/sheet#FLAT_PATTERN`.

## [02]-[BEND_SEQUENCE]

- Owner: `BendMethod` `[SmartEnum<string>]` (`air`/`bottoming`/`coining`) carrying `TonnageMultiplier` and `SpringbackScale` — the forming-method axis `Forming/sheet`'s `KFactorTable` also keys; `DieRow` the thickness-band die rule (`V = f·T`); `FlangeRow` the angle-band minimum-flange rule (`b ≥ c(A)·V`); `BrakeEnvelope` the projected machine capability row (capacity kN, gauge travel, open height — fleet-registry data, never a page-local machine table); `BendState` the plane-local search node (done-set, orientation, gauge position); `BendSequence` the static surface owning `Plan` and the die/tonnage/overbend projections.
- Cases: `BendMethod` rows 3 (air 1.0/1.0 · bottoming 4.0/0.5 · coining 8.0/0.1); `DieRow` rows 3; `FlangeRow` rows 4; the search discriminates feasibility per state — back-gauge reach, minimum flange, gauge-side clearance (flatness + occlusion on one side predicate), die/ram section clearance — as the predicate columns of ONE admission matrix fold, never sibling validators.
- Entry: `public static Fin<Seq<BendStep>> Plan(UnfoldResult unfold, FormPolicy policy, BrakeEnvelope envelope)` — the ONE plan fold the `Run(Form)` case body composes; `DieV`/`InsideRadiusAir`/`MinFlange`/`TonnagePerMeter`/`Overbend` are the pure projections consumers (estimation, traveler, manufacturability) read without re-deriving.
- Auto: `Plan` selects `V` per the die rows (the `FormPolicy.DieWidthFactor` override displaces the band factor), re-projects each bend's `BA` through `FlatPattern.Project` at the working radius `max(R, 0.16·V)`, prices tonnage per bend against `envelope.CapacityKn` (fail → 2742), gates every flange against `MinFlange` (an under-flange bend re-orders behind its neighbor or fails the state), and best-first expands `BendState` until the done-set closes — flips minimized first, gauge moves second; the winning path projects straight into `BendStep` rows with `OverbendDeg` resolved per method; `Verify/estimation` prices the plan from the same rows; `Documentation/traveler` renders them as the bend card.
- Receipt: `Seq<BendStep>` IS the plan evidence — ordered, per-bend priced, flip-marked; no parallel `BendPlan` wrapper and no plane-internal search type on the result (ruling 5: the state graph dies inside the fold).
- Packages: `Forming/sheet#FLAT_PATTERN` (`UnfoldResult`/`BendLine`/`FormPolicy`/`Project` — composed), `Process/physics#CUT_PARAMETER` (`RemovalBudget.Formed` Rm/springback), `Process/owner#FABRICATION_OWNER` atoms (`BendStep`/`Edge3`/`ContentKey`/`EgressKind.BendProgram`), `Process/family#PROCESS_FAMILY` (`Machine.PressBrakeCnc`/`ProcessKind.PressBrake` admission), `Kinematics/fleet#MACHINE_FLEET` (`BrakeEnvelope` projection rows — the capability registry), `Geometry2D/algebra#POLYGON_ALGEBRA` (collision clipping), Thinktecture.Runtime.Extensions, LanguageExt.Core, `Rasm.Numerics`, BCL inbox.
- Growth: a new forming method is one `BendMethod` row (multiplier + scale columns); a new die family is rows, not folds; hemming/staged-bottoming tooling lands as die rows plus a `FlangeRow` band, never a second planner; a bender-dialect emission target is a posting-plane concern riding the `bend-program` key; zero new entrypoint surface.
- Boundary: this page owns SEQUENCING and pricing — unfold algebra is `Forming/sheet`'s and a re-derived `BA` here (outside the working-radius re-projection) is the split-brain defect; the machine table is the fleet registry's and a page-local capacity/gauge table is the deleted form; the search state never escapes the fold; tonnage/springback constants are row data (`C`, band factors, method columns) and an inline formula literal at a call site is the named defect; bender program TEXT never lands here.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BendMethod {
    public static readonly BendMethod Air = new("air", tonnageMultiplier: 1.0, springbackScale: 1.0);
    public static readonly BendMethod Bottoming = new("bottoming", tonnageMultiplier: 4.0, springbackScale: 0.5);
    public static readonly BendMethod Coining = new("coining", tonnageMultiplier: 8.0, springbackScale: 0.1);

    public double TonnageMultiplier { get; }
    public double SpringbackScale { get; }
}

// --- [CONSTANTS] ----------------------------------------------------------------------------------------------------------------------------------
// Air-bend die constant C in F = C·Rm·S²·L/(V·1000); the gauge-reposition band and the bounded-search cap are
// row data of the same law table, never inline literals in a fold body.
public static class BrakeLaw {
    public const double DieConstant = 1.33;
    public const double GaugeRepositionToleranceMm = 0.5;
    public const int SearchCap = 1 << 14;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct DieRow(double ThicknessLowMm, double ThicknessHighMm, double WidthFactor);

public readonly record struct FlangeRow(double AngleLowDeg, double AngleHighDeg, double FlangeFactor);

// Projected from the Kinematics/fleet capability registry — never a page-local machine table.
public readonly record struct BrakeEnvelope(double CapacityKn, double GaugeTravelMm, double OpenHeightMm) {
    // The machine-less planning floor: envelope gates disabled — the quote posture simulate mirrors.
    public static readonly BrakeEnvelope Unbounded = new(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class BendSequence {
    static readonly Arr<DieRow> Dies = Array(new DieRow(0.0, 3.0, 8.0), new DieRow(3.0, 10.0, 10.0), new DieRow(10.0, double.MaxValue, 12.0));
    static readonly Arr<FlangeRow> Flanges = Array(
        new FlangeRow(90.0, 180.0, 0.7), new FlangeRow(60.0, 90.0, 0.9), new FlangeRow(45.0, 60.0, 1.1), new FlangeRow(30.0, 45.0, 1.5));

    public static double DieV(double thicknessMm, Option<double> factorOverride) =>
        factorOverride.IfNone(Dies.Filter(d => thicknessMm > d.ThicknessLowMm && thicknessMm <= d.ThicknessHighMm).Head().WidthFactor) * thicknessMm;

    public static double InsideRadiusAir(double dieVMm) => 0.16 * dieVMm;

    public static double MinFlange(double angleDeg, double dieVMm) =>
        Flanges.Filter(f => angleDeg > f.AngleLowDeg && angleDeg <= f.AngleHighDeg).HeadOrNone().Match(f => f.FlangeFactor, () => 1.5) * dieVMm;

    // kN for a full bend: Rm [MPa], S/V/L [mm] → C·Rm·S²/(1000·V) is kN per metre of bend line.
    public static double Tonnage(double rmMpa, double thicknessMm, double dieVMm, double lengthMm, BendMethod method) =>
        BrakeLaw.DieConstant * rmMpa * thicknessMm * thicknessMm / (1000.0 * dieVMm) * (lengthMm / 1000.0) * method.TonnageMultiplier;

    public static double Overbend(double angleDeg, double springbackRatio, BendMethod method) =>
        angleDeg * (1.0 - springbackRatio) / springbackRatio * method.SpringbackScale;

    // Best-first over (done-set, orientation, quantized gauge): reach → flange → gauge-clear → section-clear predicate columns;
    // flips then gauge moves as cost. The winning path IS the Seq<BendStep>; the state graph dies inside the fold
    // (ruling 5).
    public static Fin<Seq<BendStep>> Plan(UnfoldResult unfold, FormPolicy policy, BrakeEnvelope envelope) =>
        FlatPattern.FormedRow(unfold.Material).Bind(formed => {
            double v = DieV(unfold.ThicknessMm, policy.DieWidthFactor);
            Seq<(BendLine Bend, double Kn)> priced = unfold.Bends
                .Map(b => (b, Tonnage(formed.TensileRm, unfold.ThicknessMm, v, b.Line.A.DistanceTo(b.Line.B), policy.Method)));
            return priced.Filter(p => p.Kn > envelope.CapacityKn).HeadOrNone().Match(
                Some: p => Fin.Fail<Seq<BendStep>>(FabricationFault.TonnageExceeded(p.Kn, envelope.CapacityKn).ToError()),
                None: () => Search(priced, formed, policy, v, unfold, envelope));
        });

    // The plane-local search node — dies inside the fold (ruling 5). FlatDeltaMm accumulates each completed bend's
    // working-radius BA reprojection (FlatPattern.Project at max(R, 0.16·V)), shifting every later gauge target.
    sealed record BendState(Set<int> Done, bool FlippedUp, double GaugeX, double FlatDeltaMm, Seq<BendStep> Path, int Flips, int GaugeMoves) {
        public static readonly BendState Start = new(Set<int>(), FlippedUp: false, GaugeX: 0.0, FlatDeltaMm: 0.0, Seq<BendStep>(), Flips: 0, GaugeMoves: 0);

        public long Cost => ((long)Flips << 32) + GaugeMoves;
    }

    // Best-first: pop the cheapest state, expand every unbent line through the admission columns, first closed
    // done-set wins. Exhaustion (or the bounded-search cap) routes BendSequenceInfeasible 2741 carrying the last
    // blocked bend and the tried-order census.
    static Fin<Seq<BendStep>> Search(Seq<(BendLine Bend, double Kn)> priced, ModalityPhysics.Forming formed, FormPolicy policy, double v, UnfoldResult unfold, BrakeEnvelope envelope) {
        (BendLine Bend, double Kn)[] bends = priced.ToArray();
        Lst<BendState> frontier = List(BendState.Start);
        // The dedup key is the FULL forward-reachable state: Done + orientation + the gauge position
        // quantized by the reposition tolerance — GaugeX prices every later gauge move, so two orders
        // sharing a Done set but parking the gauge differently are distinct states; FlatDeltaMm is an
        // order-independent sum over Done and rides free.
        Set<(Set<int> Done, bool Flipped, long Gauge)> visited = Set<(Set<int>, bool, long)>();
        int tried = 0, blocked = 0, guard = 0;
        while (!frontier.IsEmpty && guard++ < BrakeLaw.SearchCap) {
            int bestIdx = Enumerable.Range(0, frontier.Count).Aggregate(0, (best, idx) => frontier[idx].Cost < frontier[best].Cost ? idx : best);
            BendState state = frontier[bestIdx];
            frontier = frontier.RemoveAt(bestIdx);
            if (state.Done.Count == bends.Length)
                return Fin.Succ(state.Path);
            (Set<int>, bool, long) key = (state.Done, state.FlippedUp, (long)Math.Round(state.GaugeX / BrakeLaw.GaugeRepositionToleranceMm));
            if (visited.Contains(key))
                continue;
            visited = visited.Add(key);
            foreach (int i in Enumerable.Range(0, bends.Length).Where(i => !state.Done.Contains(i))) {
                tried++;
                Seq<BendState> next = Expand(state, i, bends, formed, policy, v, unfold, envelope);
                if (next.IsEmpty)
                    blocked = i;
                frontier = next.Fold(frontier, static (f, s) => f.Add(s));
            }
        }
        return Fin.Fail<Seq<BendStep>>(FabricationFault.BendSequenceInfeasible(blocked, tried).ToError());
    }

    // ONE admission matrix per candidate, four predicate columns: back-gauge REACH (travel bound, reprojected by
    // the accumulated flat delta), minimum FLANGE (angle-band law), GAUGE-CLEAR (the gauged flange still flat and
    // its face unshadowed by an upstanding prior flange — one side predicate serves both), SECTION-CLEAR (the
    // folded-profile silhouette against the ±V/2 die/ram window). Both orientations expand; a flip costs first.
    static Seq<BendState> Expand(BendState state, int i, (BendLine Bend, double Kn)[] bends, ModalityPhysics.Forming formed, FormPolicy policy, double v, UnfoldResult unfold, BrakeEnvelope envelope) =>
        Seq(false, true).Bind(flip => {
            BendLine bend = bends[i].Bend;
            double gauge = GaugeReach(unfold, bend) + state.FlatDeltaMm;
            bool admissible =
                gauge <= envelope.GaugeTravelMm
                && FlangeWidth(unfold, bend) >= MinFlange(bend.AngleDeg, v)
                && GaugeSideClear(unfold, bend, state.Done, bends)
                && SectionClear(unfold, bend, state.Done, bends, v, envelope.OpenHeightMm);
            if (!admissible)
                return Seq<BendState>();
            double working = Math.Max(bend.InsideRadiusMm, InsideRadiusAir(v));
            BendProjection projected = FlatPattern.Project(bend.AngleDeg, working, unfold.ThicknessMm, bend.K);
            BendStep step = new(
                Order: state.Path.Count + 1,
                Line: bend.Line,
                AngleDeg: bend.AngleDeg,
                RadiusMm: working,
                KFactor: bend.K,
                OverbendDeg: Overbend(bend.AngleDeg, formed.SpringbackRatio, policy.Method),
                TonnageKn: bends[i].Kn,
                Flip: flip);
            return Seq1(state with {
                Done = state.Done.Add(i),
                FlippedUp = state.FlippedUp ^ flip,
                GaugeX = gauge,
                FlatDeltaMm = state.FlatDeltaMm + projected.FlatDeltaMm,
                Path = state.Path.Add(step),
                Flips = state.Flips + (flip ? 1 : 0),
                GaugeMoves = state.GaugeMoves + (Math.Abs(gauge - state.GaugeX) > BrakeLaw.GaugeRepositionToleranceMm ? 1 : 0),
            });
        });

    // Gauge-face law: the gauged (larger) flange must stay flat and unshadowed — a completed bend whose line sits
    // on the gauge side both breaks flatness and occludes the gauge face; ONE side predicate serves both columns.
    static bool GaugeSideClear(UnfoldResult unfold, BendLine bend, Set<int> done, (BendLine Bend, double Kn)[] bends) {
        bool positive = SideExtent(unfold, bend, positive: true) >= SideExtent(unfold, bend, positive: false);
        return !done.ToSeq().Exists(j => {
            double d = Signed(Mid(bends[j].Bend.Line), bend.Line);
            return positive ? d > 0.0 : d < 0.0;
        });
    }

    // The die/ram section is the ±V/2 window at the candidate line raised to the open height; each completed flange
    // projects its folded silhouette (thickness-wide, rising to its folded extent) at its signed offset. The overlap
    // check composes the ONE Geometry2D Clip owner; a clip failure reads as a collision — fail-closed, never a pass.
    static bool SectionClear(UnfoldResult unfold, BendLine bend, Set<int> done, (BendLine Bend, double Kn)[] bends, double v, double openHeightMm) =>
        double.IsPositiveInfinity(openHeightMm)
        || done.ToSeq().ForAll(j => {
            BendLine prior = bends[j].Bend;
            double folded = Math.Min(SideExtent(unfold, prior, positive: true), SideExtent(unfold, prior, positive: false)) + unfold.ThicknessMm;
            if (folded > openHeightMm)
                return false;
            double at = Signed(Mid(prior.Line), bend.Line);
            Loop section = Box(-v / 2.0, 0.0, v / 2.0, openHeightMm);
            Loop silhouette = Box(at - unfold.ThicknessMm, 0.0, at + unfold.ThicknessMm, folded);
            return PolygonAlgebra.Clip(Seq1(silhouette), Seq1(section), ClipOp.Intersect)
                .Map(static overlap => overlap.IsEmpty)
                .IfFail(false);
        });

    // Boundary extent algebra over the flat pattern: the gauge reach is the larger perpendicular side extent, the
    // flange width the smaller — one Signed primitive serves reach, flange, gauge-side, and silhouette columns.
    static double GaugeReach(UnfoldResult unfold, BendLine bend) =>
        Math.Max(SideExtent(unfold, bend, positive: true), SideExtent(unfold, bend, positive: false));

    static double FlangeWidth(UnfoldResult unfold, BendLine bend) =>
        Math.Min(SideExtent(unfold, bend, positive: true), SideExtent(unfold, bend, positive: false));

    static double SideExtent(UnfoldResult unfold, BendLine bend, bool positive) =>
        unfold.Flat.ToSeq()
            .Bind(static loop => loop.Vertices.ToSeq())
            .Map(pt => Signed(pt, bend.Line))
            .Filter(d => positive ? d > 0.0 : d < 0.0)
            .Map(Math.Abs)
            .Fold(0.0, Math.Max);

    static double Signed(Point3d pt, Edge3 line) {
        double dx = line.B.X - line.A.X, dy = line.B.Y - line.A.Y;
        double len = Math.Max(1e-9, Math.Sqrt((dx * dx) + (dy * dy)));
        return (((pt.X - line.A.X) * dy) - ((pt.Y - line.A.Y) * dx)) / len;
    }

    static Point3d Mid(Edge3 line) => new((line.A.X + line.B.X) / 2.0, (line.A.Y + line.B.Y) / 2.0, 0.0);

    static Loop Box(double x0, double y0, double x1, double y1) =>
        new(Arr(new Point3d(x0, y0, 0.0), new Point3d(x1, y0, 0.0), new Point3d(x1, y1, 0.0), new Point3d(x0, y1, 0.0)), Closed: true);
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Unfold["sheet UnfoldResult flat + BendLines"] --> Plan["BendSequence.Plan"]
    Formed["RemovalBudget.Formed Rm · Ks · radius factor"] --> Plan
    Fleet["Kinematics/fleet BrakeEnvelope projection"] --> Plan
    Dies["DieRow V=f·T · FlangeRow b≥c(A)·V"] --> Plan
    Plan -->|"Seq<BendStep> + bend-program ContentKey"| Formed2["owner FormedResult"]
    Plan -->|per-bend rows| Est["Verify/estimation · Documentation/traveler"]
    Plan -.->|demand > capacity| F2742["TonnageExceeded 2742"]
    Plan -.->|search exhausted| F2741["BendSequenceInfeasible 2741"]
```
