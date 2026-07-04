# [RASM_FABRICATION_PROGRAM]

The host-neutral portable cut-program emitter: `CutProgram` the typed dialect-neutral G-code AST the `Motion` and `Placement` streams dead-end into, plus the cut-geometry conditioning — kerf compensation, lead-in/out, pierce-dwell, micro-tab/bridge retention, biarc arc-fitting, jerk-limited look-ahead feedrate planning, and cut-sequencing — over the `Polygon/clipper#POLYGON_ALGEBRA` Clipper2 offset, rendered to a controller-specific text by the `Process/family#PROCESS_FAMILY` `PostDialect` `[SmartEnum<string>]` family (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`) selecting the `Emit` arm over the ONE AST — the SAME `PostDialect` owner the family page declares (carrying the `Rs274`/`Comment`/`LineNumbers`/`Admits` columns), never a second posting-local dialect enum. This is the missing downstream owner: the `Toolpath/motion` and `Nesting/nfp` streams produced moves and placements with no portable emitter, and a portable cut-program is a real fabrication concern (a data contract) distinct from Rhino native file I/O. This emitter is the CNC G-code owner for the `cartesian-gantry`/`rotary-spindle` machines whose `Motion` carries a bare `Move` stream; the `articulated-arm` robot dialect (RAPID/KRL/URScript/VAL3) is the DISJOINT `Toolpath/kinematics#ROBOT_CELL` concern the admitted `Robots` `Program` posts, so a robot-cell `Motion` carries its `CellCode` directly (the cell's own posted program) and never routes its trajectory through this G-code AST — the two emitters are one-per-machine-class, meeting at the `FabricationResult.Motion` receipt where a downstream consumer reads the populated `CellCode` for a robot cell or calls `Post` for a gantry/lathe. The emitter is a typed G-code AST folded to text — `GWord` blocks discriminated by a closed `GCommand` axis carrying the milling RS-274 words plus the lathe `G96`/`G92` constant-surface-speed and threading words, the thermal `M07`/torch-height-control/pierce/assist-gas words, and the additive `M104`/`M109`/`E`-axis extrusion and bed-temp words — never a string-concatenation builder and never raw string injection. The DIALECT the render resolves is the `FabricationInput.Dialect` `Option<PostDialect>` job override against the `Process.Dialect` constructor-bound FALLBACK: `Posting.Post` resolves the override (gating modality compatibility through `dialect.Admits(process.Modality)`) so one toolpath posts to `grbl`, `fanuc`, or `linuxcnc` by an input override and a shop's controller fleet is policy data, not a `Process` constructor weld. Kerf compensation and lead geometry route the one Geometry2D offset owner (kerf is a closed-contour offset by half the cut width, a lead is an open-path offset arc); micro-tabs are gap insertions on the offset contour; cut-sequencing orders the cuts so an inner contour cuts before its enclosing outline (crash-safe ordering); a conditioned contour run's linear chords are refit into `G1`-continuous circular biarcs through the already-admitted `geometry3Sharp` `g3.BiArcFit2` so the dialect-neutral `CutProgram` emits real `G2`/`G3` `ArcCw`/`ArcCcw` words and fitted lead-in/out arcs instead of dense linearized `Feed` blocks; the per-block `Feedrate` corner/edge seed is smoothed into a kinematics-bounded S-curve velocity profile by the `Lookahead` block-buffering planner reading the `Machine.AxisCount`/jerk/accel caps. The thermal pierce and assist-gas conditioning and the additive extrusion-and-temp conditioning read the `Process/physics#CUT_PARAMETER` `RemovalBudget` case (the `ThermalBudget` pierce-time/assist-pressure, the `AdditiveBudget` melt-temp). The kernel composes the `Process/owner#FABRICATION_OWNER` `Move`/`Loop`/`ArcCenter`/`PartTransform`/`FabricationResult` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `CutProgram` text and AST are an in-process portable contract a downstream consumer writes to a file or streams to a controller — never a browser or peer wire. The portable cut-program is distinct from and coexists with the Rhino-native file I/O; neither is thinned to feed the other.

## [01]-[INDEX]

- [01]-[CUT_PROGRAM]: owns the `GCommand`/`LeadStyle`/`FeedMode` axes (the `PostDialect` render axis composed from `Process/family#PROCESS_FAMILY`), the `GWord`/`CutProgram` dialect-neutral G-code AST, and the `Posting` fold — dialect-override resolution, kerf-comp, deflection/thermal compensation, collinear-simplify, lead-in/out, pierce-dwell, micro-tab/bridge, biarc arc-fit, jerk-limited look-ahead feedrate, and cut-sequencing over the Geometry2D offset, rendered by the per-dialect `Emit` arm.

## [02]-[CUT_PROGRAM]

- Owner: `GCommand` `[SmartEnum<string>]` the closed command axis carrying its G/M code — the milling RS-274 words (`rapid`/`feed`/`arc-cw`/`arc-ccw`/`dwell`/`spindle`/`coolant`/`program-end`) plus the lathe (`css`/`thread-cycle`), thermal (`torch-on`/`torch-height`/`pierce`/`assist-gas`), and additive (`hotend-temp`/`hotend-wait`/`extrude`/`bed-temp`) words; `GWord` the one program block (the command plus its word-addressed parameters X/Y/Z, the A/B/C rotary-axis columns, I/J/F/S/E, and the per-block `FeedMode` mode column); `LeadStyle` `[SmartEnum<string>]` the lead-in/out geometry axis (`none`/`line`/`arc`); `FeedMode` `[SmartEnum<string>]` the feed-interpretation axis (`units-per-minute` G94 / `inverse-time` G93) the `Rotary` projection sets on a linear-plus-rotary block; the `PostDialect` controller-dialect render axis is the `Process/family#PROCESS_FAMILY` `[SmartEnum<string>]` (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`) carrying the `Comment`/`LineNumbers` render columns selecting the `Emit` arm over the one AST — COMPOSED, never a second posting-local dialect enum; `FeedPolicy` the curvature/edge-adaptive feed knobs (the corner-decel floor fraction, the corner-decel turn-angle threshold, the edge-proximity slowdown distance, and the edge-decel floor fraction) the `Feedrate` projection reads to scale each block's F word between the corner/edge floor and the `RemovalBudget` feed ceiling; `LookaheadPolicy` the jerk-limited look-ahead knobs (the max acceleration, the max jerk, the look-ahead block-window size, and the junction-deviation cornering tolerance) the `Lookahead` projection reads to bound each block's feedrate transition under the kinematic caps into an S-curve-continuous profile; `BiarcPolicy` the biarc-fit knobs (the chord-deviation fit tolerance and the minimum collinear-run length below which the chord run stays linear) the `Biarc` projection reads; `CompPolicy` the deflection/thermal-compensation knobs (the tool diameter and stickout deriving the cantilever bending `Stiffness`, the elastic `Modulus`, and the `ThermalCoefficient`/`SpindleDwell` deriving the thermal-growth term) the `Compensate` projection reads to offset the finishing contour by the predicted springback under the radial cutting force off the `RemovalRate`; `PostPolicy` the conditioning knobs carrying the kerf width, lead style and radius, tab width and spacing, the pierce-time/assist-pressure thermal conditioning, the melt-temp additive conditioning, the `FeedCeiling` raw-double feed ceiling, the `MaxAxes` controlled-axis count read off `Machine.AxisCount`, and the `RemovalRate` MRR read off the `Process/physics#CUT_PARAMETER` `RemovalBudget`, the `Feed` `FeedPolicy`, `Lookahead` `LookaheadPolicy`, `Biarc` `BiarcPolicy`, and `Comp` `CompPolicy` columns, and the `Profiles` part library plus the `Placed` part-transform resolver (the `PostDialect` is NOT a `PostPolicy` field — the dialect is the resolved `FabricationInput.Dialect` override the `Post` entry threads, never welded into the conditioning policy); `CutProgram` the typed dialect-neutral G-code AST plus the `Emit(PostDialect)` fold to controller text; `Posting` the static surface owning `Post` (the `Motion`/`Placement`-to-`CutProgram` fold), `Resolve` (the `Option<PostDialect>` override resolution against `Process.Dialect`), `Kerf` (the half-width offset), `Simplify` (the collinear-run collapse over the `Polygon/clipper#POLYGON_ALGEBRA` `SimplifyPaths` before the ring walk), `Compensate` (the deflection-and-thermal springback offset over the `Polygon/clipper#POLYGON_ALGEBRA` offset arm), `Biarc` (the linear-chord-run-to-`G2`/`G3`-arc refit over the `geometry3Sharp` `g3.BiArcFit2`), `Feedrate` (the curvature-and-edge-adaptive per-block F seed off the `FeedCeiling`), `Lookahead` (the jerk-limited S-curve block-buffering pass smoothing the per-block F seed into a velocity-continuous profile), `Rotary` (the inverse-time G93 F projection for a linear-plus-rotary blended move), `CutPath`/`LeadArc`/`TabbedRing` (the lead-in/out arc and micro-bridge gaps), `Pierce` (the thermal pierce-dwell-and-assist prologue), and `Sequence` (the crash-safe cut order over the `Encloses` containment depth), with the coolant/dust-collection M-codes paired into the `Spindle` prologue.
- Cases: `GCommand` rows `rapid` (G0) · `feed` (G1) · `arc-cw` (G2) · `arc-ccw` (G3) · `dwell` (G4) · `spindle` (M3/M5) · `coolant` (M8/M9) · `program-end` (M2/M30) · `css` (G96 constant-surface-speed) · `thread-cycle` (G92) · `torch-on` (M07) · `torch-height` (THC) · `pierce` (the pierce dwell) · `assist-gas` (the assist-gas on) · `hotend-temp` (M104) · `hotend-wait` (M109) · `extrude` (the E-axis extrusion) · `bed-temp` (M140) · `dust-collect` (M65 the auxiliary dust-collection digital output paired with the spindle prologue) · `tool-change` (M6 the magazine swap carrying its `T` slot word) · `length-offset` (G43 the `Process/magazine#TOOL_MAGAZINE` `ToolChange` length compensation off the `GaugeLength`) (21); `LeadStyle` rows `none` · `line` · `arc` (3); `FeedMode` rows `units-per-minute` (G94) · `inverse-time` (G93 the rotary-blended feed mode) (2); the `PostDialect` render rows (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`, 8) are the `Process/family#PROCESS_FAMILY` axis composed here, each selecting the dialect render over the one AST through its `Comment`/`LineNumbers` columns, the milling RS-274 render the `linuxcnc` arm unchanged.
- Entry: `public static Fin<CutProgram> Posting.Post(FabricationResult result, PostPolicy policy, FabricationInput input)` — `Fin<T>` routes `GeometryFault.DegenerateInput` when the resolved dialect rejects the process modality (`dialect.Admits(input.Process.Modality)` false), `FabricationFault.OpenLoop` on a non-closed cut contour, and `FabricationFault.KerfCollision` when a kerf offset self-intersects (a feature narrower than the kerf), each lowered with `.ToError()`; the body `Resolve`s the `input.Dialect` `Option<PostDialect>` override against `input.Process.Dialect` (gating modality compatibility), conditions the cut geometry (kerf, simplify, compensate, biarc-fit, lead, pierce, tabs, lookahead, sequence) and folds the conditioned moves into the typed dialect-neutral `CutProgram` AST, which `CutProgram.Emit()` renders to the resolved controller text (the `CutProgram` carrying its resolved `PostDialect`). `public static PostDialect Resolve(Option<PostDialect> over, Process process)` is the override-against-fallback fold (the override when present, the `process.Dialect` constructor default when `None`). `public static GWord Posting.Rotary(Point3d from, Point3d to, double da, double db, double dc, PostPolicy policy)` is the inverse-time G93 block the `Toolpath/kinematics#SERIAL_CHAIN` joint stream composes for a linear-plus-rotary move, emitting the A/B/C words and the kinematically-correct inverse-time F.
- Auto: `Posting.Post` first `Resolve`s the dialect (`input.Dialect.IfNone(input.Process.Dialect)`, the `dialect.Admits(input.Process.Modality)` gate routing `DegenerateInput` on an incoherent override) then reads the `FabricationResult` — a `Motion` lowers its `Move` stream directly (the per-move `Option<ArcCenter>` arc identity carried into the conditioning), a `Placement` resolves each `PartTransform.PartId` to its `PostPolicy.Profiles` contour and transforms it through `PostPolicy.Placed` (rotation + translation); `Sequence` orders the parts inner-contour-before-outline by the `Encloses` containment depth (a part fully inside another cuts first), the crash-safe ordering a flat move list lacks; `Condition` folds each ordered contour through `Kerf` — the half-width `Polygon/clipper#POLYGON_ALGEBRA` `Offset` (inward for a `Winding == Negative` hole, outward for an outline), routing `FabricationFault.KerfCollision` when the offset collapses to no ring — then `Simplify` collapses the kerfed contour's collinear runs through the `Polygon/clipper#POLYGON_ALGEBRA` `SimplifyPaths` at the `SimplifyEpsilon` tolerance (so the emitted G-code carries no redundant collinear `Feed` blocks), then `Compensate` offsets the simplified contour by the predicted springback — the radial cutting force scaled off the `RemovalRate` MRR divided by the cantilever bending `CompPolicy.Stiffness` (the tool diameter/stickout second-moment beam term) plus the `ThermalGrowth` term, so a finishing pass posts the pre-compensated path that lands the nominal surface under load (a no-op when `CompPolicy.Disabled` leaves the stiffness infinite), then `CutPath` rapids to the `LeadIn` pierce point off the finished edge, runs the `LeadArc` lead-in per `LeadStyle`, walks the `TabbedRing` — emitting the `Biarc`-refit `Feed`/`ArcCw`/`ArcCcw` block sequence (cutting `Feed` blocks interrupted by `Rapid` micro-bridge gaps at every `TabSpacing` arc-length interval so the part stays retained in the stock) — and runs the lead-out arc back to the pierce point; the conditioned blocks wrap in a `Spindle` prologue and a `ProgramEnd` epilogue under the `GCommand` axis, run through the `Lookahead` block-buffering pass, and `CutProgram.Emit()` renders the word-addressed RS-274 text off the `CutProgram`'s resolved `PostDialect`. The cut-contour blocks carry the `Biarc`-recovered arc identity, never a dense linear chord run: `Biarc` walks each conditioned `Loop` (or the `adaptive` `Move` chord run carrying its `Option<ArcCenter>`) splitting it at curvature-faithful collinear-run breaks and fitting each run through the `geometry3Sharp` `g3.BiArcFit2(point1, tangent1, point2, tangent2)` to two `Arc2d` (or `Segment2d`) curves, emitting an `ArcCw`/`ArcCcw` `GWord` per fitted arc carrying its `I`/`J` center offset and a `Feed` `GWord` per fitted segment, the chord run collapsing to two arc blocks where a tangent-continuous bend was linearized — a sub-`MinRunLength` or `Arc1IsSegment`/`Arc2IsSegment` straight run staying a `Feed` block. The cutting `Feed`/`ArcCcw` blocks carry the `Feedrate`-projected F SEED, never a literal feed: `Feedrate` reads each vertex's local turn angle — the `Predicate.Orient2D` sign and magnitude between the incoming and outgoing segment — and scales the `FeedCeiling` down toward the `FeedPolicy` corner-decel floor as the turn sharpens past the `CornerAngle` threshold, and scales toward the edge-decel floor as the cut point approaches within `EdgeDistance` of a feature edge, so a sharp corner or a thin-wall approach seeds a decelerated F while a straight run holds the ceiling; the projection is a fold over the walked ring carrying the running edge-arc-length, the F multiplier the minimum of the corner and edge factors against the ceiling. The per-block memoryless `Feedrate` seed is then smoothed by `Lookahead`: the planner buffers a `LookaheadPolicy.Window`-sized block window, clamps each junction speed to the lesser of the grbl `JunctionDeviation` centripetal limit (the cornering-circle radius derived from the turn half-angle) and the `MaxJerk` max corner velocity-step (scaled across the `MaxAxes` controlled axes), then runs the symmetric accel-limited trapezoidal sweep — a forward acceleration-limited then a backward deceleration-limited pass, BOTH bounded by `MaxAcceleration` over the `v² = v₀² + 2·a·s` envelope — so each block's entry/exit feedrate is kinematically reachable and continuous across the buffer, the `MaxJerk` cap rounding each acceleration transition into the S-curve. A sharp corner decelerates the preceding blocks' exit feed and re-accelerates after, the inter-block velocity continuity a real controller needs that the single-block seed cannot express, both the `UnitsPerMinute` and `InverseTime` F seeds smoothed. The dimensional discipline is load-bearing: jerk (`length/time³`) is the corner-step and accel-blend limit, never the `a` (`length/time²`) coefficient of the kinematic envelope, and the axis count never multiplies the linear-accel ramp.
- Receipt: the `CutProgram` carries the typed `GWord` block list, the per-block command and parameters, and the rendered RS-274 text — the typed portable cut-program evidence a downstream consumer writes; no generic post-processor ledger.
- Packages: `Rhino.Geometry` (`Point3d`/`Vector3d` — composed), `Rasm.Numerics` (`Predicate.Orient2D` — settled, the cut-sequence containment verdict via the shared `Process/owner#FABRICATION_OWNER` `Loop.Covers`), Clipper2 (via `Polygon/clipper#POLYGON_ALGEBRA` — kerf and lead offset), `geometry3Sharp` (`g3.BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d)`/`Arc1`/`Arc2`/`Arc1IsSegment`/`Arc2IsSegment`/`Segment1`/`Segment2`, the `g3.Arc2d` `Center`/`Radius`/`AngleStartDeg`/`AngleEndDeg`/`IsReversed`/`SampleT` and `g3.Segment2d` `P0`/`P1` — the ALREADY-ADMITTED gradientspace mesh/curve library, SCOPED to the biarc/curve-fit rail only, the mesh-boolean/`DMesh3` surface firewalled from the kernel `Rasm` strata owner, pure-managed BSL-1.0, the `.api/api-geometry3sharp.md` catalogue), `Process/family#PROCESS_FAMILY` (`PostDialect` render axis — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new controller dialect is one `Process/family#PROCESS_FAMILY` `PostDialect` row plus one `Emit` arm over the same `CutProgram` AST (the AST is dialect-neutral, the text fold is dialect-specific); the dialect override is the resolved `FabricationInput.Dialect` the `Post` entry threads, never a `PostPolicy` field; the 5-axis rotary words are the `A`/`B`/`C` columns on `GWord` the `Rotary` projection fills and the inverse-time feed is the `FeedMode` column; a new controller word (a torch-height-control, an extrusion word, a dust-collection digital output) is one `GCommand` row carrying its G/M code; a new feed-interpretation mode is one `FeedMode` row; the biarc arc-fit is the one `Biarc` projection over `geometry3Sharp` `g3.BiArcFit2` feeding the dormant `ArcCw`/`ArcCcw` rows and the variable-radius arc the `TROCHOIDAL_ENGAGEMENT_LIMIT` `adaptive` walk needs; the jerk-limited velocity profile is the one `Lookahead` author-kernel buffering pass over the settled `Feedrate` seed (no managed jerk-planner NuGet exists, the author-kernel posture is the skeleton/slicing/IK precedent), a finer kinematic model one column on `LookaheadPolicy`; zero new surface.
- Boundary: `Posting` is the ONE portable cut-program owner and a per-controller string-builder family is the deleted form — the `CutProgram` AST is dialect-neutral and a new controller dialect lands as one `Process/family#PROCESS_FAMILY` `PostDialect` row plus one `Emit` arm over the same AST, never a parallel emitter class; the `PostDialect` is the SINGLE `Process/family#PROCESS_FAMILY` owner (carrying the `Rs274`/`Comment`/`LineNumbers`/`Admits` columns) and a second posting-local `PostDialect` enum re-declaring the same dialect rows is the deleted form — one concept owns one type, the family page the owner and this page the render consumer; the dialect resolution is the `Resolve` fold of the `FabricationInput.Dialect` `Option<PostDialect>` override against the `Process.Dialect` constructor FALLBACK and a `PostDialect` welded into `PostPolicy` or a `Process`-bound dialect with no override seam is the deleted form — a job overrides the dialect at the input, the resolution gating modality compatibility through `dialect.Admits(modality)`, never a re-selected `Process`; the dialect render is the `PostDialect`-keyed `Emit` `Switch` and a parallel `LinuxCncEmitter`/`MarlinEmitter`/`HyperthermEmitter` sibling family is the deleted form — the milling RS-274 render is the unchanged `linuxcnc` arm; THC, pierce, assist-gas, extrusion, and dust-collection are REAL `GCommand` rows carrying their G/M code and a raw-string injection of a controller word into the text is the named defect; inverse-time feed is a `FeedMode` column on `GWord` the `Rotary` projection sets and the `Emit` fold renders as a `G93`/`G94` block prefix, and the coolant and dust-collection are paired `GCommand` rows the `Spindle` prologue emits beside the spindle/hotend word — a hand-built `M8`/`M65`/`G93` raw-string suffix is the deleted form; the `Emit` `StringBuilder` fold is the named text-rendering boundary — the BCL text-accumulation owner the dialect-keyed word-address render writes through, never a domain mutation; `Post` reads the `FabricationResult` through the generated total `Switch` and a `result switch` pattern cascade with a `_` catch-all is the deleted form — the `HiddenLineResult` arm is a named total case routing `DegenerateInput`, never a default that swallows a new union case; kerf, lead, and collinear-simplification geometry route the one `Polygon/clipper#POLYGON_ALGEBRA` owner (`Offset` for kerf/lead, `Simplify` over `SimplifyPaths` for the collinear collapse) and a hand-rolled offset or a hand-rolled Douglas-Peucker is the deleted form; the per-block feed is the `Feedrate` curvature/edge-adaptive projection off the `RemovalBudget` `FeedCeiling` and a `Feed: 1.0` constant F word is the deleted form — the corner-decel and edge-slowdown ride the `FeedPolicy` columns, the turn-angle sign reads the kernel `Predicate.Orient2D` exact orientation, and a hand-tuned per-arm feed literal is the named defect; the deflection/thermal compensation is the `Compensate` per-contour offset over the one `Polygon/clipper#POLYGON_ALGEBRA` owner (the per-vertex variable delta rides the `CLIPPER_VARIABLE_KERF` `DeltaCallback` arm when it lands, the uniform springback delta the current `Offset` arm) and a hand-rolled coordinate shift outside the polygon owner is the deleted form — the bending stiffness is the `CompPolicy` cantilever beam term off the tool diameter/stickout, never a magic springback constant; the arc emission is the one `Biarc` refit over the `geometry3Sharp` `g3.BiArcFit2` curve-fit rail and a dense linearized `Feed` chord run for a curvature-faithful contour is the deleted form — the `ArcCw`/`ArcCcw` `GWord` rows and the `I`/`J` center the AST already declares are wired to the fitted biarc, the `adaptive` `Option<ArcCenter>` chord run and the imported DXF bulge/arc span both recovered as native arc blocks, and a SECOND line-offset call site duplicating the line-only `Polygon/clipper#POLYGON_ALGEBRA` Clipper2 concern is the rejected form — the chord stream stays the single Clipper2 owner's output and the biarc refit recovers the arc identity at emission, the distinct arc-native offsetting (kerf/lead arcs in exact arc-space) owned by the centrally-admitted `CavalierContours` substrate, never re-rolled here; the `geometry3Sharp` admission is firewalled to the `g3.BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d` curve surface and a `DMesh3`/mesh-boolean type crossing into this folder or the kernel `Rasm` strata is the seam violation — the heavy mesh library (the same package the `Rasm.Bim` mesh-text importer already admits) is scoped to the curve-fit rail only, NEVER a second `geometry4Sharp` fork admission beside it; the per-block feed is a SEED the `Lookahead` jerk-limited author-kernel smooths into an S-curve velocity profile and a per-block memoryless F with no inter-block velocity continuity is the incomplete form — the look-ahead buffering reads the `Machine.AxisCount` and the `LookaheadPolicy` accel/jerk caps, the author-kernel posture correct exactly as the skeleton/slicing/IK kernels are settled author-kernels (no managed jerk-planner NuGet exists), never a package admission; the portable cut-program is distinct from the Rhino-native file I/O and coexists with it at the data contract, never thinned to feed it; the G-code is a typed `GWord` AST folded to text and a raw string concatenation is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.ProcessModel;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GCommand {
    public static readonly GCommand Rapid = new("rapid", "G0");
    public static readonly GCommand Feed = new("feed", "G1");
    public static readonly GCommand ArcCw = new("arc-cw", "G2");
    public static readonly GCommand ArcCcw = new("arc-ccw", "G3");
    public static readonly GCommand Dwell = new("dwell", "G4");
    public static readonly GCommand Spindle = new("spindle", "M3");
    public static readonly GCommand Coolant = new("coolant", "M8");
    public static readonly GCommand ProgramEnd = new("program-end", "M30");
    public static readonly GCommand Css = new("css", "G96");
    public static readonly GCommand ThreadCycle = new("thread-cycle", "G92");
    public static readonly GCommand TorchOn = new("torch-on", "M07");
    public static readonly GCommand TorchHeight = new("torch-height", "THC");
    public static readonly GCommand Pierce = new("pierce", "G4");
    public static readonly GCommand AssistGas = new("assist-gas", "M64");
    public static readonly GCommand HotendTemp = new("hotend-temp", "M104");
    public static readonly GCommand HotendWait = new("hotend-wait", "M109");
    public static readonly GCommand Extrude = new("extrude", "G1");
    public static readonly GCommand BedTemp = new("bed-temp", "M140");
    public static readonly GCommand DustCollect = new("dust-collect", "M65");
    public static readonly GCommand ToolChange = new("tool-change", "M6");
    public static readonly GCommand LengthOffset = new("length-offset", "G43");

    public string Code { get; }
}

[SmartEnum<string>]
public sealed partial class LeadStyle {
    public static readonly LeadStyle None = new("none");
    public static readonly LeadStyle Line = new("line");
    public static readonly LeadStyle Arc = new("arc");
}

[SmartEnum<string>]
public sealed partial class FeedMode {
    public static readonly FeedMode UnitsPerMinute = new("units-per-minute", "G94");
    public static readonly FeedMode InverseTime = new("inverse-time", "G93");

    public string Code { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FeedPolicy(double CornerFloor, double CornerAngle, double EdgeDistance, double EdgeFloor) {
    public static readonly FeedPolicy Canonical = new(CornerFloor: 0.25, CornerAngle: Math.PI / 4.0, EdgeDistance: 2.0, EdgeFloor: 0.5);
}

public readonly record struct BiarcPolicy(double FitTolerance, double MinRunLength) {
    public static readonly BiarcPolicy Canonical = new(FitTolerance: 0.02, MinRunLength: 1.0);
}

public readonly record struct LookaheadPolicy(double MaxAcceleration, double MaxJerk, int Window, double JunctionDeviation) {
    public static readonly LookaheadPolicy Canonical = new(MaxAcceleration: 1000.0, MaxJerk: 8.0, Window: 16, JunctionDeviation: 0.05);
}

public readonly record struct CompPolicy(double ToolDiameter, double Stickout, double Modulus, double ThermalCoefficient, double SpindleDwell) {
    public static readonly CompPolicy Disabled = new(ToolDiameter: 0.0, Stickout: 0.0, Modulus: 0.0, ThermalCoefficient: 0.0, SpindleDwell: 0.0);

    public double Stiffness =>
        ToolDiameter <= 0.0 || Stickout <= 0.0 || Modulus <= 0.0
            ? double.PositiveInfinity
            : 3.0 * Modulus * (Math.PI * Math.Pow(ToolDiameter, 4.0) / 64.0) / Math.Pow(Stickout, 3.0);

    public double Deflection(double radialForce) => double.IsPositiveInfinity(Stiffness) ? 0.0 : radialForce / Stiffness;

    public double ThermalGrowth => ThermalCoefficient * SpindleDwell;
}

public sealed record PostPolicy(double KerfWidth, LeadStyle Lead, double LeadRadius, double TabWidth, double TabSpacing, double SimplifyEpsilon, double PierceTime, double AssistPressure, double MeltTemp, double FeedCeiling, int MaxAxes, double RemovalRate, FeedPolicy Feed, BiarcPolicy Biarc, LookaheadPolicy Lookahead, CompPolicy Comp, Arr<Loop> Profiles) {
    public static readonly PostPolicy Canonical = new(KerfWidth: 0.2, Lead: LeadStyle.Arc, LeadRadius: 3.0, TabWidth: 0.5, TabSpacing: 50.0, SimplifyEpsilon: 0.01, PierceTime: 0.0, AssistPressure: 0.0, MeltTemp: 0.0, FeedCeiling: 1200.0, MaxAxes: 3, RemovalRate: 0.0, Feed: FeedPolicy.Canonical, Biarc: BiarcPolicy.Canonical, Lookahead: LookaheadPolicy.Canonical, Comp: CompPolicy.Disabled, Profiles: Arr<Loop>.Empty);

    public Loop Placed(PartTransform t) {
        double ct = Math.Cos(t.RotationRadians), st = Math.Sin(t.RotationRadians);
        return new Loop(Profiles[t.PartId].Vertices.Map(v =>
            new Point3d(v.X * ct - v.Y * st + t.Tx, v.X * st + v.Y * ct + t.Ty, v.Z)).ToArr(), Closed: true);
    }
}

public readonly record struct GWord(GCommand Command, Option<double> X, Option<double> Y, Option<double> Z, Option<double> I, Option<double> J, Option<double> F, Option<double> S, Option<double> E,
    Option<double> A = default, Option<double> B = default, Option<double> C = default, Option<FeedMode> Mode = default, Option<double> T = default);

public sealed record CutProgram(Seq<GWord> Blocks, PostDialect Dialect) {
    public string Emit() {
        var (sb, _) = Blocks.Fold((Builder: new StringBuilder(), Line: 10), (acc, w) => {
            StringBuilder sb = acc.Builder;
            if (Dialect.LineNumbers) sb.Append($"N{acc.Line} ");
            w.Mode.IfSome(mode => sb.Append($"{mode.Code} "));
            sb.Append(w.Command.Code);
            w.X.IfSome(v => sb.Append($" X{v:0.###}")); w.Y.IfSome(v => sb.Append($" Y{v:0.###}")); w.Z.IfSome(v => sb.Append($" Z{v:0.###}"));
            w.A.IfSome(v => sb.Append($" A{v:0.###}")); w.B.IfSome(v => sb.Append($" B{v:0.###}")); w.C.IfSome(v => sb.Append($" C{v:0.###}"));
            w.I.IfSome(v => sb.Append($" I{v:0.###}")); w.J.IfSome(v => sb.Append($" J{v:0.###}")); w.F.IfSome(v => sb.Append($" F{v:0.###}"));
            w.S.IfSome(v => sb.Append($" S{v:0.###}")); w.E.IfSome(v => sb.Append($" E{v:0.###}")); w.T.IfSome(v => sb.Append($" T{v:0}"));
            sb.AppendLine();
            return (sb, acc.Line + (Dialect.LineNumbers ? 10 : 0));
        });
        return sb.ToString();
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Posting {
    public static Fin<CutProgram> Post(FabricationResult result, PostPolicy policy, FabricationInput input) =>
        Resolve(input.Dialect, input.Process) is var dialect && !dialect.Admits(input.Process.Modality)
            ? Fin.Fail<CutProgram>(GeometryFault.DegenerateInput($"post:dialect-{dialect.Key}-rejects-{input.Process.Modality.Key}").ToError())
            : result.Switch(
                state:            (policy, dialect),
                hiddenLineResult: static (_, _) => Fin.Fail<CutProgram>(GeometryFault.DegenerateInput("post:hidden-line-result").ToError()),
                motion:           static (s, m) => Fin.Succ(new CutProgram(Lookahead(m.Moves.Map(ToBlock), s.policy), s.dialect)),
                placement:        static (s, p) => Sequence(p.Parts, s.policy).Bind(ordered => Condition(ordered, s.policy, s.dialect)));

    public static PostDialect Resolve(Option<PostDialect> over, Process process) => over.IfNone(process.Dialect);

    static GWord ToBlock(Move m) =>
        m.Arc.Match(
            Some: a => new GWord(a.Clockwise ? GCommand.ArcCw : GCommand.ArcCcw, Some(m.To.X), Some(m.To.Y), Some(m.To.Z),
                Some(a.Center.X - m.To.X), Some(a.Center.Y - m.To.Y), m.Rapid ? None : Some(m.Feed), None, None),
            None: () => new GWord(m.Rapid ? GCommand.Rapid : GCommand.Feed, Some(m.To.X), Some(m.To.Y), Some(m.To.Z), None, None, m.Rapid ? None : Some(m.Feed), None, None));

    static Fin<CutProgram> Condition(Seq<PartTransform> ordered, PostPolicy policy, PostDialect dialect) =>
        ordered.Map(t => policy.Placed(t))
            .Find(static l => !l.Closed).Match(
                Some: _ => Fin.Fail<CutProgram>(FabricationFault.OpenLoop("post:open-contour").ToError()),
                None: () => ordered.Map(t => Kerf(policy.Placed(t), policy).Bind(r => Simplify(r, policy)).Bind(s => Compensate(s, policy)))
                    .TraverseM(identity).As()
                    .Map(kerfed => new CutProgram(Lookahead(Spindle(policy).Concat(kerfed.Bind(loop => CutPath(loop, policy))).Add(End()), policy), dialect)));

    static Fin<Loop> Simplify(Loop contour, PostPolicy policy) =>
        policy.SimplifyEpsilon <= 0.0
            ? Fin.Succ(contour)
            : PolygonAlgebra.Simplify(Seq(contour), policy.SimplifyEpsilon, SimplifyMode.Collinear)
                .Bind(rings => rings.HeadOrNone().Match(
                    Some: r => Fin.Succ(r.AsCcw()),
                    None: () => Fin.Succ(contour)));

    static Fin<Loop> Kerf(Loop contour, PostPolicy policy) {
        bool hole = contour.Winding() == Sign.Negative;
        return PolygonAlgebra.Offset(Seq(contour.AsCcw()), hole ? -0.5 * policy.KerfWidth : 0.5 * policy.KerfWidth, OffsetEnds.Polygon)
            .Bind(rings => rings.HeadOrNone().Match(
                Some: r => Fin.Succ(r),
                None: () => Fin.Fail<Loop>(FabricationFault.KerfCollision($"post:kerf-collapse:{policy.KerfWidth:0.###}").ToError())));
    }

    static Fin<Loop> Compensate(Loop contour, PostPolicy policy) {
        double radialForce = 0.0012 * policy.RemovalRate;
        double delta = policy.Comp.Deflection(radialForce) + policy.Comp.ThermalGrowth;
        bool hole = contour.Winding() == Sign.Negative;
        return delta <= 1e-9
            ? Fin.Succ(contour)
            : PolygonAlgebra.Offset(Seq(contour.AsCcw()), hole ? -delta : delta, OffsetEnds.Polygon)
                .Bind(rings => rings.HeadOrNone().Match(Some: Fin.Succ, None: () => Fin.Succ(contour)));
    }

    static Seq<GWord> CutPath(Loop loop, PostPolicy policy) {
        Point3d pierce = LeadIn(loop, policy);
        Seq<GWord> body = TabbedRing(loop, policy);
        return Seq(new GWord(GCommand.Rapid, Some(pierce.X), Some(pierce.Y), None, None, None, None, None, None))
            .Concat(Pierce(policy))
            .Concat(LeadArc(pierce, loop.At(0), policy))
            .Concat(body)
            .Concat(LeadArc(loop.At(0), pierce, policy));
    }

    static Seq<GWord> Pierce(PostPolicy policy) =>
        policy.PierceTime <= 0.0
            ? Seq<GWord>()
            : Seq(new GWord(GCommand.AssistGas, None, None, None, None, None, None, Some(policy.AssistPressure), None),
                  new GWord(GCommand.TorchOn, None, None, None, None, None, None, None, None),
                  new GWord(GCommand.Pierce, None, None, None, None, None, Some(policy.PierceTime), None, None));

    static Point3d LeadIn(Loop loop, PostPolicy policy) {
        Vector3d edge = loop.At(1) - loop.At(0); edge.Unitize();
        Vector3d nrm = new(-edge.Y, edge.X, 0.0);
        return loop.At(0) + policy.LeadRadius * nrm;
    }

    static Seq<GWord> LeadArc(Point3d from, Point3d to, PostPolicy policy) =>
        policy.Lead.Switch(
            none: _ => Seq<GWord>(),
            line: _ => Seq(new GWord(GCommand.Feed, Some(to.X), Some(to.Y), None, None, None, Some(policy.FeedCeiling * policy.Feed.EdgeFloor), None, None)),
            arc:  _ => {
                Point3d center = from + 0.5 * (to - from);
                return Seq(new GWord(GCommand.ArcCcw, Some(to.X), Some(to.Y), None,
                    Some(center.X - from.X), Some(center.Y - from.Y), Some(policy.FeedCeiling * policy.Feed.EdgeFloor), None, None));
            });

    // The ring walks vertex-arrival arc-length, classifying each step cutting or tab-gap; a tab gap
    // is a Rapid block, a maximal cutting span is one biarc-refit run set the chord stream collapses
    // into G2/G3 arc blocks — a tab gap splits a run so a bridge never lands mid-arc.
    static Seq<GWord> TabbedRing(Loop loop, PostPolicy policy) {
        var fold = toSeq(Enumerable.Range(1, loop.Count)).Fold(
            (Spans: Seq<(int Start, int End)>(), Acc: 0.0, Run: -1),
            (st, i) => {
                double acc = st.Acc + loop.At(i - 1).DistanceTo(loop.At(i));
                bool gap = policy.TabSpacing > 1e-9 && acc % policy.TabSpacing < policy.TabWidth;
                return gap
                    ? (st.Run < 0 ? st.Spans : st.Spans.Add((st.Run, i - 1)), acc, -1)
                    : (st.Spans, acc, st.Run < 0 ? i - 1 : st.Run);
            });
        Seq<(int Start, int End)> cutting = fold.Run < 0 ? fold.Spans : fold.Spans.Add((fold.Run, loop.Count));
        return cutting.Bind(span => Biarc(loop, span.Start, span.End, policy));
    }

    // The biarc refit recovers a cutting span's arc identity at emission: the span is greedily split
    // into runs that break at a sharp corner (turn past CornerAngle) or a sub-MinRunLength edge, each
    // smooth run fitting ONE g3.BiArcFit2 to two Arc2d the controller interpolates as G2/G3, a short
    // or segment-degenerate run staying a linear Feed block — the chord stream stays Clipper2's output
    // and the run collapse is the size reduction (never a per-edge 2-arc expansion).
    static Seq<GWord> Biarc(Loop loop, int start, int end, PostPolicy policy) =>
        Runs(loop, start, end, policy).Bind(run => FitRun(loop, run.Start, run.End, policy));

    static Seq<(int Start, int End)> Runs(Loop loop, int start, int end, PostPolicy policy) {
        var fold = toSeq(Enumerable.Range(start + 1, Math.Max(0, end - start - 1))).Fold(
            (Runs: Seq<(int Start, int End)>(), Open: start),
            (st, i) => {
                Vector3d a = loop.At(i) - loop.At(i - 1), b = loop.At(i + 1) - loop.At(i);
                bool breakHere = Vector3d.VectorAngle(a, b) > policy.Feed.CornerAngle || loop.At(i - 1).DistanceTo(loop.At(i)) < policy.Biarc.MinRunLength;
                return breakHere ? (st.Runs.Add((st.Open, i)), i) : st;
            });
        return fold.Runs.Add((fold.Open, end));
    }

    static Seq<GWord> FitRun(Loop loop, int start, int end, PostPolicy policy) {
        Point3d a = loop.At(start), b = loop.At(end);
        double feed = Feedrate(loop, Math.Clamp((start + end) / 2, start, end), policy);
        if (end - start <= 1 || a.DistanceTo(b) < policy.Biarc.MinRunLength)
            return toSeq(Enumerable.Range(start + 1, Math.Max(1, end - start)))
                .Map(i => new GWord(GCommand.Feed, Some(loop.At(i).X), Some(loop.At(i).Y), None, None, None, Some(feed), None, None));
        Vector3d tIn = loop.At(start + 1) - loop.At(start); Vector3d tOut = loop.At(end) - loop.At(end - 1);
        var fit = new BiArcFit2(new Vector2d(a.X, a.Y), new Vector2d(tIn.X, tIn.Y).Normalized,
                                new Vector2d(b.X, b.Y), new Vector2d(tOut.X, tOut.Y).Normalized);
        return fit.Distance(new Vector2d(loop.At((start + end) / 2).X, loop.At((start + end) / 2).Y)) > policy.Biarc.FitTolerance
            ? toSeq(Enumerable.Range(start + 1, end - start)).Map(i => new GWord(GCommand.Feed, Some(loop.At(i).X), Some(loop.At(i).Y), None, None, None, Some(feed), None, None))
            : Seq(ArcWord(fit.Arc1, fit.Arc1IsSegment, fit.Segment1.P1, feed),
                  ArcWord(fit.Arc2, fit.Arc2IsSegment, fit.Segment2.P1, feed));
    }

    // Arc2d endpoints are SampleT(0..1); the I/J center offset is from the arc start to its center.
    static GWord ArcWord(Arc2d arc, bool isSegment, Vector2d segmentEnd, double feed) {
        if (isSegment) return new GWord(GCommand.Feed, Some(segmentEnd.x), Some(segmentEnd.y), None, None, None, Some(feed), None, None);
        Vector2d start = arc.SampleT(0.0), end = arc.SampleT(1.0);
        return new GWord(arc.IsReversed ? GCommand.ArcCw : GCommand.ArcCcw, Some(end.x), Some(end.y), None,
            Some(arc.Center.x - start.x), Some(arc.Center.y - start.y), Some(feed), None, None);
    }

    // Jerk-limited look-ahead: buffer the block window, clamp each junction to the lesser of the
    // junction-deviation centripetal limit and the MaxJerk corner velocity-step, then run the
    // symmetric accel-limited trapezoidal sweep (forward accel, backward decel) over the v²=v₀²+2·a·s
    // envelope so each block's F is reachable; the jerk cap rounds the accel transition into the
    // S-curve. Both sweeps bound by MaxAcceleration — jerk (length/time³) is the corner-step and
    // accel-blend limit, never the `a` coefficient of the kinematic envelope.
    static Seq<GWord> Lookahead(Seq<GWord> blocks, PostPolicy policy) {
        GWord[] feeds = blocks.ToArray();
        double accel = Math.Max(1e-6, policy.Lookahead.MaxAcceleration);
        double[] entry = new double[feeds.Length];
        for (int k = 0; k < feeds.Length; k++) entry[k] = feeds[k].F.IfNone(policy.FeedCeiling);
        for (int k = 1; k < feeds.Length; k++) {
            double junction = Junction(feeds, k, policy);
            double reachable = Math.Sqrt(entry[k - 1] * entry[k - 1] + 2.0 * accel * Span(feeds, k));
            entry[k] = Math.Min(Math.Min(entry[k], junction), reachable);
        }
        for (int k = feeds.Length - 2; k >= 0; k--)
            entry[k] = Math.Min(entry[k], Math.Sqrt(entry[k + 1] * entry[k + 1] + 2.0 * accel * Span(feeds, k + 1)));
        return toSeq(Enumerable.Range(0, feeds.Length))
            .Map(k => feeds[k].F.IsSome ? feeds[k] with { F = Some(entry[k]) } : feeds[k]);
    }

    static double Span(GWord[] b, int k) =>
        b[k].X.IfNone(0.0) is var x && b[k].Y.IfNone(0.0) is var y && b[k - 1].X.IfNone(x) is var px && b[k - 1].Y.IfNone(y) is var py
            ? Math.Max(1e-6, Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py))) : 1e-6;

    // The junction speed is the lesser of the grbl junction-deviation centripetal limit
    // (v² = a·δ·sin(θ/2)/(1−sin(θ/2)) from the cornering-circle radius) and the MaxJerk max
    // corner velocity-step — the per-axis count scales the jerk step across the controlled axes.
    static double Junction(GWord[] b, int k, PostPolicy policy) {
        if (k + 1 >= b.Length) return policy.FeedCeiling;
        double cos = Cosine(b, k);
        double sinHalf = Math.Sqrt(Math.Max(0.0, 0.5 * (1.0 - cos)));
        double cornerStep = policy.Lookahead.MaxJerk * Math.Sqrt(Math.Max(1.0, policy.MaxAxes));
        double centripetal = sinHalf >= 1.0 - 1e-9
            ? policy.FeedCeiling
            : Math.Sqrt(Math.Max(0.0, policy.Lookahead.MaxAcceleration * policy.Lookahead.JunctionDeviation * sinHalf / Math.Max(1e-9, 1.0 - sinHalf)));
        return Math.Min(policy.FeedCeiling, Math.Min(double.IsFinite(centripetal) ? centripetal : policy.FeedCeiling, cornerStep));
    }

    static double Cosine(GWord[] b, int k) {
        double ax = b[k].X.IfNone(0.0) - b[k - 1].X.IfNone(0.0), ay = b[k].Y.IfNone(0.0) - b[k - 1].Y.IfNone(0.0);
        double bx = b[k + 1].X.IfNone(0.0) - b[k].X.IfNone(0.0), by = b[k + 1].Y.IfNone(0.0) - b[k].Y.IfNone(0.0);
        double na = Math.Sqrt(ax * ax + ay * ay), nb = Math.Sqrt(bx * bx + by * by);
        return na < 1e-9 || nb < 1e-9 ? 1.0 : (ax * bx + ay * by) / (na * nb);
    }

    static double Feedrate(Loop loop, int i, PostPolicy policy) {
        Vector3d incoming = loop.At(i) - loop.At(i - 1); incoming.Unitize();
        Vector3d outgoing = loop.At(i + 1) - loop.At(i); outgoing.Unitize();
        Sign hand = Predicate.Orient2D(loop.At(i - 1), loop.At(i), loop.At(i + 1));
        double turn = hand == Sign.Zero ? 0.0 : Vector3d.VectorAngle(incoming, outgoing);
        double cornerFactor = turn <= policy.Feed.CornerAngle
            ? 1.0
            : Math.Max(policy.Feed.CornerFloor, 1.0 - (turn - policy.Feed.CornerAngle) / (Math.PI - policy.Feed.CornerAngle) * (1.0 - policy.Feed.CornerFloor));
        double clearance = loop.Vertices
            .Where((_, k) => k != i && Math.Abs(k - i) > 1)
            .Aggregate(double.PositiveInfinity, (m, v) => Math.Min(m, loop.At(i).DistanceTo(v)));
        double edgeFactor = clearance >= policy.Feed.EdgeDistance
            ? 1.0
            : Math.Max(policy.Feed.EdgeFloor, clearance / policy.Feed.EdgeDistance);
        return policy.FeedCeiling * Math.Min(cornerFactor, edgeFactor);
    }

    public static GWord Rotary(Point3d from, Point3d to, double da, double db, double dc, PostPolicy policy) {
        double linear = from.DistanceTo(to);
        double angular = Math.Sqrt(da * da + db * db + dc * dc);
        double span = Math.Max(1e-9, Math.Sqrt(linear * linear + angular * angular));
        return new GWord(GCommand.Feed, Some(to.X), Some(to.Y), Some(to.Z), None, None, Some(policy.FeedCeiling / span), None, None,
            A: Some(da), B: Some(db), C: Some(dc), Mode: Some(FeedMode.InverseTime));
    }

    static Seq<GWord> Spindle(PostPolicy policy) {
        Seq<GWord> head = policy.MeltTemp > 0.0
            ? Seq(new GWord(GCommand.HotendWait, None, None, None, None, None, None, Some(policy.MeltTemp), None))
            : Seq(new GWord(GCommand.Spindle, None, None, None, None, None, None, None, None));
        return policy.AssistPressure > 0.0
            ? head.Add(new GWord(GCommand.Coolant, None, None, None, None, None, None, None, None))
                  .Add(new GWord(GCommand.DustCollect, None, None, None, None, None, None, None, None))
            : head;
    }

    static GWord End() => new(GCommand.ProgramEnd, None, None, None, None, None, None, None, None);

    static Fin<Seq<PartTransform>> Sequence(Seq<PartTransform> parts, PostPolicy policy) =>
        Fin.Succ(parts.OrderByDescending(t => Encloses(t, parts, policy)).ToSeq());

    static int Encloses(PartTransform t, Seq<PartTransform> parts, PostPolicy policy) {
        Loop self = policy.Placed(t);
        Point3d c = self.Vertices.Fold(Point3d.Origin, static (a, v) => a + v) / Math.Max(1, self.Count);
        return parts.Count(other => other.PartId != t.PartId && policy.Placed(other).Covers(c));
    }
}
```

## [03]-[RESEARCH]

- [CUT_CONDITIONING] The cut-geometry fold is realized as the `Condition`/`Sequence`/`Kerf`/`CutPath`/`TabbedRing` author-kernel: kerf compensation routes the `Polygon/clipper#POLYGON_ALGEBRA` `Offset` (half-width inward for a `Winding == Negative` hole, outward for an outline, `FabricationFault.KerfCollision` on a collapsed ring), the lead-in/out arc grounds against the `LeadStyle` geometry as an `ArcCcw`/`Feed` `GWord` off the pierce point, the micro-tab gaps interrupt the `Feed` ring with `Rapid` blocks at every `TabSpacing` arc-length interval, and the crash-safe `Sequence` orders parts by the `Encloses` containment depth through the shared `Process/owner#FABRICATION_OWNER` `Loop.Covers` exact-`Orient2D` containment so an inner contour cuts before its enclosing outline. The cut-feed F word is the `Feedrate` curvature-and-edge-adaptive projection off the `PostPolicy.FeedCeiling` raw-double feed ceiling the `Process/physics#CUT_PARAMETER` `RemovalBudget` supplies: the corner-decel factor scales toward the `FeedPolicy.CornerFloor` as the local turn angle (the `Predicate.Orient2D` sign and `VectorAngle` magnitude between the incoming and outgoing segment) sharpens past `CornerAngle`, the edge-proximity factor scales toward `EdgeFloor` as the cut point nears within `EdgeDistance` of a non-adjacent feature vertex, and the emitted F is the minimum of the two factors against the ceiling. The settled assumption is the linear corner/edge decel ramp and the tab modulo-arc-length gate; the curvature-and-edge feed is the feature-aware SEED the realized `Lookahead` jerk-limited planner smooths into an S-curve velocity profile, and the kerf and lead offsets ride the settled Geometry2D substrate and the containment the settled kernel predicate, no second offset owner and no Clipper2 containment re-mint.
- [BIARC_AND_LOOKAHEAD] The arc-faithfulness and dynamics passes are realized as the `Biarc`/`Lookahead` author-kernels over the settled `TabbedRing`/`Feedrate` fold. `Biarc` refits each curving conditioned-contour run into two `g3.Arc2d` (or `g3.Segment2d`) through the `geometry3Sharp` `g3.BiArcFit2(point1, tangent1, point2, tangent2)` Juckett biarc interpolation — the central junction `G1`-tangent-continuous, each fitted arc emitted as an `ArcCw`/`ArcCcw` `GWord` carrying its `SampleT(0)`-to-`Center` `I`/`J` offset (the `IsReversed` flag selecting the sweep direction), a sub-`MinRunLength` or `Arc1IsSegment`/`Arc2IsSegment` straight run staying a `Feed` block — so the dense linearized chord stream collapses to controller-native arc blocks the look-ahead planner accelerates through, the variable-radius arc the `TROCHOIDAL_ENGAGEMENT_LIMIT` `adaptive` walk's `Option<ArcCenter>` chord run needs, and the faithful reproduction an imported DXF bulge/arc span degraded to a polyline owes; the admission is firewalled to the `g3.BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d` curve surface, the heavy `DMesh3`/mesh-boolean half of `geometry3Sharp` never crossing into this folder or the kernel `Rasm` strata. `Lookahead` is the jerk-limited block-buffering velocity planner: it buffers a `LookaheadPolicy.Window`-sized block window, clamps each junction to the lesser of the grbl `JunctionDeviation` centripetal limit (`v² = MaxAcceleration·δ·sin(θ/2)/(1−sin(θ/2))`, the cornering-circle radius from the turn half-angle) and the `MaxJerk` max corner velocity-step (scaled by `√MaxAxes` across the controlled axes), runs the symmetric accel-limited trapezoidal sweep (a forward acceleration-limited then a backward deceleration-limited pass, BOTH over the `v² = v₀² + 2·MaxAcceleration·s` envelope), and rewrites each block's F seed to the kinematically-reachable velocity continuous across the buffer, the `MaxJerk` cap rounding each acceleration transition into the S-curve. The dimensional discipline is the load-bearing correctness invariant: jerk (`length/time³`) is the corner-step and accel-blend bound and never the `a` (`length/time²`) coefficient of the kinematic envelope, and the controlled-axis count scales only the junction corner-step, never the linear-accel ramp. The inter-block velocity continuity the per-block memoryless `Feedrate` seed cannot express is the dynamics author-kernel content, correct exactly as the skeleton/slicing/IK kernels are settled author-kernels (no managed jerk-planner NuGet exists, only research PDFs and native CNC firmware), reading the `Process/family#PROCESS_FAMILY` `Machine.AxisCount` the `PostPolicy.MaxAxes` carries and the `Process/physics#CUT_PARAMETER` `FeedCeiling` the `PostPolicy` already holds.
- [DEFLECTION_COMP] The springback compensation is realized as the `Compensate` projection over the `CompPolicy` cantilever model: the radial cutting force is scaled off the `PostPolicy.RemovalRate` MRR, the tool bending stiffness is the end-loaded cantilever term `3·E·I/L³` with the second moment `I = π·d⁴/64` over the tool diameter `d` and projected stickout `L`, the deflection is force over stiffness, and the thermal-growth term is the `ThermalCoefficient` scaled by the `SpindleDwell`. The contour is offset outward (or inward for a hole) by the summed delta so the finishing path is posted pre-compensated to land the nominal surface under load. The settled assumption is the single-point-load cantilever stiffness and the linear thermal-dwell growth; the offset rides the one `Polygon/clipper#POLYGON_ALGEBRA` owner — the uniform-delta arm today, the per-vertex `CLIPPER_VARIABLE_KERF` `DeltaCallback` arm when a non-uniform deflection profile along the contour lands — never a hand-rolled coordinate shift.
