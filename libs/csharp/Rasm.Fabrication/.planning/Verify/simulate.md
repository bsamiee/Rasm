# [RASM_FABRICATION_SIMULATE]

The program-level controller simulation: `Simulate.Execute` the modal-state execution WALK over the landed `Posting/program#CUT_PROGRAM` `CutProgram` AST — a fold threading ONE typed `ControllerState` register vector block-by-block, integrating per-block time under an accel-limited trapezoidal evaluation, and gating every commanded position against the machine envelope. The register vector is the FULL RS274 modal census AS EXECUTION STATE: thirteen `ModalSlot` rows (motion · plane · distance · feed-mode · units · cutter-comp · tool-length · retract · wcs · path-control · spindle · coolant · stop), each carrying its RS274 group ordinal and power-on default. The landed `GNode` stream stamps every modal word into the thirteen-slot `Registers` carrier through the `GroupSlots` lowering (the five landed groups write live; the remaining slots hold their power-on defaults as TYPED stored registers) and becomes fully live as the `Posting/program` canned-cycle/WCS/comp grammar widens — the register CENSUS is this page's, the five-group PARSE state machine and its `ModalGroup` violation payload stay `Posting/program`'s, and a simulate-side parse is the split-owner defect. `Posting/optimization#OPTIMIZATION` names this walk the AUTHORITATIVE cycle-time owner: its `OptimizationReceipt` seconds are pipeline-local estimates, re-measured here; `Verify/estimation` prices the `SimulationReceipt`, never a pass-local integral.

Time is EVALUATION, never planning: the per-block integral runs the trapezoidal `v² = v₀² + 2·a·s` envelope over the block's commanded F (triangle profile when the span cannot reach it), rapids at the limits' rapid rate — under the `MotionDynamics` policy shape that is the TYPE CONTRACT of the ONE jerk/accel motion-dynamics law `Kinematics/machine` HOMES. This page consumes the policy VALUES and never plans: junction clamping and S-curve continuity are `Posting/program`'s internal `Lookahead` certificate, jerk-aware look-ahead re-planning is `Kinematics/machine`'s law, and a second planner here is the named dual-paradigm defect. Envelope truth reads the landed `Kinematics/fleet#MACHINE_FLEET` `MachineInstance.Envelope` travels: a CUT move leaving the envelope routes `EnvelopeExceeded` 2738, a RAPID crossing the soft-limit margin routes `SimulatedOvertravel` 2739 — both on the shared `MachineAxis` carrier, both typed, never a warning string; rotary A/B/C travel limits are `Kinematics/machine`'s per-axis rows, read for gating as the machine descriptor threads (the declared-register discipline, stated not silent). Dwell and pierce carry seconds in the P slot per the program AST law (word and canned-cycle forms alike); tool-change blocks cost the policy's swap seconds and re-arm the length-offset register.

Wire posture: HOST-LOCAL. The `SimulationReceipt` crosses only the in-process seam — estimation pricing, traveler quoting, optimization re-measure; the state vector and ledger never sit between wire and rail.

## [01]-[INDEX]

- [01]-[SIMULATE]: owns the `ModalSlot` register census, the `ControllerState` typed register vector with its `Apply` transition fold, the `MotionDynamics` dynamics policy (the `Kinematics/machine` law consumed directly), the `SimulatePolicy`/`BlockTime`/`SimulationReceipt` evidence family, and the ONE `Simulate.Execute` walk — per-block time integration, envelope/overtravel gating, dwell/tool-change/pierce accounting.

## [02]-[SIMULATE]

- Owner: `ModalSlot` `[SmartEnum<string>]` the thirteen-register RS274 census (`motion` g1 · `plane` g2 · `distance` g3 · `feed-mode` g5 · `units` g6 · `cutter-comp` g7 · `tool-length` g8 · `retract` g10 · `wcs` g12 · `path-control` g13 · `spindle` m7 · `coolant` m8 · `stop` m4), each row binding its group ordinal and power-on default code — the census axis, distinct from the `Posting/program` parse-facing `ModalGroup` payload by charter; `ControllerState` the typed register vector (motion command, feed mode, F/S words, tool slot, XYZ position + ABC rotary registers, spindle/coolant/length-comp flags, the declared default-holding slots) with the row-table transition fold (`Transitions : Map<GCommand, TransitionKind>` — a command without a row fails the walk typed); `MotionDynamics` the dynamics policy shape (per-axis rapid rate, max acceleration — the `Kinematics/machine` motion-dynamics law's TYPE contract, its `Conservative` row the machine-less seed); `SimulatePolicy` the walk knobs (`Option<MachineInstance>` machine, limits, tool-change seconds, soft-limit margin); `BlockTime` the per-block ledger row (ordinal, command, seconds, span, applied feed); `SimulationReceipt` the integrated evidence (cycle/cut/rapid/dwell seconds, tool-change and pierce counts, the block ledger, the final state); `Simulate` the static surface owning `Execute`.
- Cases: `ModalSlot` rows 13; `TransitionKind` rows 11 dispatched by the `Transitions` row table — every landed `GCommand` row maps to exactly one kind (`rapid` · `cut` for feed/extrude/arc/probe · `dwell`/`pierce` P-slot seconds · `tool-swap` · `length-comp` · `spindle-on` · `coolant-on` · `set-register` · `modal-only` for plane/distance/units · `stop`); an UNMAPPED command fails the walk typed and a `_`-catch-all running a new word as motion is unconstructable; the `GNode` walk is the generated total Switch over the six AST cases (canned cycles spend P and walk their expanded moves, macro/subprogram bodies walk recursively × repeats, additive-layer/NC1 nodes carry no controller motion); the time integral cases — trapezoid (span reaches F), triangle (span-limited peak `√(v₀² + 2·a·s)`ⁿ), rapid (limits rate), zero-span register write.
- Entry: `public static Fin<SimulationReceipt> Execute(CutProgram program, SimulatePolicy policy)` — the ONE walk; `Fin<T>` routes `FabricationFault.EnvelopeExceeded` 2738 `(MachineAxis, at, limit)` on a cut move leaving the envelope, `FabricationFault.SimulatedOvertravel` 2739 `(block, MachineAxis, by)` on a rapid crossing the soft-limit margin, and kernel `GeometryFault.DegenerateInput` on an empty program, each lowered with `.ToError()`; the machine-less call (`Machine: None`) integrates time against `MotionDynamics.Canonical` and gates no envelope — a verdict basis for quoting before a fleet match exists.
- Auto: `Execute` seeds `ControllerState.PowerOn` (every slot at its `ModalSlot` default, position at origin, velocity zero) and folds the block stream — each `GNode.Word` lowers through the transition row table producing the next state (its modal slot stamped through `GroupSlots`) plus a `BlockEffect` (span, target feed, seconds-in-F, swap flag); the integrator converts the effect to seconds (feed blocks trapezoidal under `MotionDynamics.Acceleration` with the carried entry velocity, rapids at `RapidMmS`, arc spans measured on the I/J circle, dwell/pierce read P as seconds, tool-change adds `ToolChangeSeconds` and increments the swap census); the envelope gate checks the commanded endpoint (and the arc's axis-extreme quadrant crossings) per axis against `MachineInstance.Envelope` before the clock advances — cut moves hard-gate, rapids gate at `SoftLimitMarginMm`. `Verify/estimation#ESTIMATION` prices the receipt; `Documentation/traveler` quotes it; `Posting/optimization` re-measures its deltas against it.
- Receipt: `SimulationReceipt` IS the typed evidence — integrated cycle seconds split cut/rapid/dwell, tool-change and pierce counts, the per-block `BlockTime` ledger, and the terminal `ControllerState`; no fault arms ride the receipt (a violation FAILS the walk typed) and no generic simulation ledger exists beside it.
- Packages: `Posting/program#CUT_PROGRAM` (`CutProgram`/`GNode`/`GParam`/`GCommand`/`ModalGroup`/`FeedMode` — the landed AST, composed), `Kinematics/fleet#MACHINE_FLEET` (`MachineInstance.Envelope` — the envelope truth), `Process/faults#FAULT_BAND` (`MachineAxis` + the 2738/2739 arms), `Rhino.Geometry` (`Point3d`/`BoundingBox` — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox; landed seams: `Kinematics/machine` (the ONE motion-dynamics law — `MotionDynamics` consumed directly and the per-axis rotary limits land there), `Posting/program` row-30 rebuild (canned-cycle/WCS/comp nodes arm the declared registers; the `Parse ∘ Emit = id` contract stays posting's).
- Growth: a new controller word is one `GCommand` row plus one `Transitions` table entry (a missing entry fails the walk typed, never runs); a queued register going live is its declared `ModalSlot` row gaining transitions, never a new slot family; a finer dynamics model is columns on `MotionDynamics` when `Kinematics/machine` lands its law; per-axis rotary gating is the machine page's limit rows read here as data; a thermal/spindle-load overlay is one `BlockEffect` column; zero new surface.
- Boundary: simulate EVALUATES and never plans — junction speeds, S-curves, and look-ahead are `Posting/program.Lookahead`'s certificate and a re-planned feed here is the dual-paradigm defect; the register census is EXECUTION state and the parse state machine (five tracked groups, `ModalGroup` payload, round-trip law) stays `Posting/program`'s — a simulate-side `Parse` is the split-owner defect; the motion-dynamics LAW homes on `Kinematics/machine` and `MotionDynamics` is its consumed TYPE contract, never a second jerk/accel owner; envelope truth is the fleet instance's measured travels and a page-local machine table is the deleted form; a violation is a TYPED fault on the rail, never a warning row on the receipt; cycle time is THIS page's receipt and any sibling integrating seconds beside it (optimization baselines excepted as pipeline-local, re-measured here) is the second-clock defect.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Kinematics;        // MachineInstance envelope truth + the ONE MotionDynamics law
using Rasm.Fabrication.Posting;           // CutProgram · GNode · GCommand · ModalGroup · FeedMode — the landed AST
using Rasm.Fabrication.Process;           // FabricationFault · MachineAxis · GeometryFault routing
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
using WalkState = (Rasm.Fabrication.Verify.ControllerState State, (double Cut, double Rapid, double Dwell) Clock, int Swaps, int Pierces, LanguageExt.Seq<Rasm.Fabrication.Verify.BlockTime> Ledger, int Block);

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// The thirteen-register RS274 census AS EXECUTION STATE: group ordinal + power-on default per row.
// Distinct by charter from Posting/program's parse-facing ModalGroup (five tracked groups, violation payload).
[SmartEnum<string>]
public sealed partial class ModalSlot {
    public static readonly ModalSlot Motion = new("motion", group: 1, powerOn: "G0");
    public static readonly ModalSlot Plane = new("plane", group: 2, powerOn: "G17");
    public static readonly ModalSlot Distance = new("distance", group: 3, powerOn: "G90");
    public static readonly ModalSlot FeedRateMode = new("feed-mode", group: 5, powerOn: "G94");
    public static readonly ModalSlot Units = new("units", group: 6, powerOn: "G21");
    public static readonly ModalSlot CutterComp = new("cutter-comp", group: 7, powerOn: "G40");
    public static readonly ModalSlot ToolLength = new("tool-length", group: 8, powerOn: "G49");
    public static readonly ModalSlot Retract = new("retract", group: 10, powerOn: "G98");
    public static readonly ModalSlot Wcs = new("wcs", group: 12, powerOn: "G54");
    public static readonly ModalSlot PathControl = new("path-control", group: 13, powerOn: "G64");
    public static readonly ModalSlot Spindle = new("spindle", group: 107, powerOn: "M5");
    public static readonly ModalSlot Coolant = new("coolant", group: 108, powerOn: "M9");
    public static readonly ModalSlot Stop = new("stop", group: 104, powerOn: "");

    public int Group { get; }
    public string PowerOn { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// The ONE Kinematics/machine motion-dynamics law consumed directly (mm/min feed, mm/s² accel); simulate
// EVALUATES the law program.Lookahead planned under — a second limits shape is the dead form.
public sealed record SimulatePolicy(Option<MachineInstance> Machine, MotionDynamics Dynamics, double ToolChangeSeconds, double SoftLimitMarginMm) {
    public static readonly SimulatePolicy Quote = new(Machine: None, Dynamics: MotionDynamics.Canonical, ToolChangeSeconds: 8.0, SoftLimitMarginMm: 0.0);
}

// The transition-law axis: every landed GCommand row maps to exactly ONE row here through the Transitions
// table — a command without a row FAILS the walk typed, so an unknown word can never execute as motion.
[SmartEnum<string>]
public sealed partial class TransitionKind {
    public static readonly TransitionKind Rapid = new("rapid");
    public static readonly TransitionKind Cut = new("cut");
    public static readonly TransitionKind Dwell = new("dwell");
    public static readonly TransitionKind Pierce = new("pierce");
    public static readonly TransitionKind ToolSwap = new("tool-swap");
    public static readonly TransitionKind LengthComp = new("length-comp");
    public static readonly TransitionKind SpindleOn = new("spindle-on");
    public static readonly TransitionKind CoolantOn = new("coolant-on");
    public static readonly TransitionKind SetRegister = new("set-register");
    public static readonly TransitionKind ModalOnly = new("modal-only");
    public static readonly TransitionKind Stop = new("stop");
}

// The typed register vector: named execution fields PLUS the full thirteen-slot Registers carrier — every
// ModalSlot row has explicit storage seeded at its power-on default, and every modal word writes its slot.
public sealed record ControllerState(
    GCommand Motion, FeedMode Feed, double F, double S, Option<double> T,
    Point3d At, double A, double B, double C, double VelocityMmS,
    bool SpindleOn, bool CoolantOn, bool LengthComp, bool Stopped,
    Map<ModalSlot, string> Registers) {
    public static readonly ControllerState PowerOn = new(
        GCommand.Rapid, FeedMode.UnitsPerMinute, F: 0.0, S: 0.0, T: None,
        At: Point3d.Origin, A: 0.0, B: 0.0, C: 0.0, VelocityMmS: 0.0,
        SpindleOn: false, CoolantOn: false, LengthComp: false, Stopped: false,
        Registers: toMap(toSeq(ModalSlot.Items).Map(static slot => (slot, slot.PowerOn))));
}

public readonly record struct BlockEffect(double SpanMm, double TargetFeedMmS, double FixedSeconds, bool ToolSwap, bool Pierce);

public readonly record struct BlockTime(int Block, GCommand Command, double Seconds, double SpanMm, double FeedApplied);

public sealed record SimulationReceipt(
    double CycleSeconds, double CutSeconds, double RapidSeconds, double DwellSeconds,
    int ToolChanges, int Pierces, Seq<BlockTime> Ledger, ControllerState Final);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Simulate {
    // GCommand → TransitionKind row table: EVERY landed command row has exactly one entry; a lookup miss fails
    // the walk typed, so a new landed command cannot simulate until its row lands — the totality fence.
    static readonly Map<GCommand, TransitionKind> Transitions = Map(
        (GCommand.Rapid, TransitionKind.Rapid),
        (GCommand.Feed, TransitionKind.Cut), (GCommand.Extrude, TransitionKind.Cut),
        (GCommand.ArcCw, TransitionKind.Cut), (GCommand.ArcCcw, TransitionKind.Cut),
        (GCommand.Probe, TransitionKind.Cut),
        (GCommand.Dwell, TransitionKind.Dwell), (GCommand.Pierce, TransitionKind.Pierce),
        (GCommand.ToolChange, TransitionKind.ToolSwap), (GCommand.LengthOffset, TransitionKind.LengthComp),
        (GCommand.Spindle, TransitionKind.SpindleOn), (GCommand.TorchOn, TransitionKind.SpindleOn),
        (GCommand.Coolant, TransitionKind.CoolantOn), (GCommand.AssistGas, TransitionKind.CoolantOn), (GCommand.DustCollect, TransitionKind.CoolantOn),
        (GCommand.Css, TransitionKind.SetRegister), (GCommand.ThreadCycle, TransitionKind.SetRegister), (GCommand.TorchHeight, TransitionKind.SetRegister),
        (GCommand.HotendTemp, TransitionKind.SetRegister), (GCommand.HotendWait, TransitionKind.SetRegister), (GCommand.BedTemp, TransitionKind.SetRegister),
        (GCommand.PlaneXy, TransitionKind.ModalOnly), (GCommand.Absolute, TransitionKind.ModalOnly), (GCommand.Metric, TransitionKind.ModalOnly),
        (GCommand.ProgramEnd, TransitionKind.Stop));

    // Modal-group → register-slot lowering: every modal word stamps its slot in the thirteen-slot Registers
    // carrier, so the census IS execution state, never a decorative catalogue.
    static readonly Map<ModalGroup, ModalSlot> GroupSlots = Map(
        (ModalGroup.Motion, ModalSlot.Motion), (ModalGroup.Plane, ModalSlot.Plane),
        (ModalGroup.Distance, ModalSlot.Distance), (ModalGroup.Units, ModalSlot.Units),
        (ModalGroup.Feed, ModalSlot.FeedRateMode));

    // The ONE walk over the CutProgram AST (Seq<GNode>): PowerOn state, generated total Switch over the six
    // node cases, row-table word transitions, trapezoidal time integral, envelope gate before the clock
    // advances. A violation FAILS typed — never a warning row on the receipt.
    public static Fin<SimulationReceipt> Execute(CutProgram program, SimulatePolicy policy) =>
        program.Nodes.IsEmpty
            ? Fin.Fail<SimulationReceipt>(GeometryFault.DegenerateInput("simulate:empty-program").ToError())
            : Walk(program.Nodes,
                Fin.Succ((State: ControllerState.PowerOn, Clock: (Cut: 0.0, Rapid: 0.0, Dwell: 0.0), Swaps: 0, Pierces: 0, Ledger: Seq<BlockTime>(), Block: 0)),
                policy)
              .Map(st => new SimulationReceipt(
                  CycleSeconds: st.Clock.Cut + st.Clock.Rapid + st.Clock.Dwell + st.Swaps * policy.ToolChangeSeconds,
                  CutSeconds: st.Clock.Cut, RapidSeconds: st.Clock.Rapid, DwellSeconds: st.Clock.Dwell,
                  ToolChanges: st.Swaps, Pierces: st.Pierces, Ledger: st.Ledger, Final: st.State));

    static Fin<WalkState> Walk(Seq<GNode> nodes, Fin<WalkState> seed, SimulatePolicy policy) =>
        nodes.Fold(seed, (acc, node) => acc.Bind(st => Node(st, node, policy)));

    // AST dispatch: WORD blocks lower through the transition row table; a CANNED CYCLE spends its P seconds
    // (Pierce counting) and executes its expanded moves; MACRO/SUBPROGRAM bodies walk recursively (× repeats);
    // ADDITIVE-LAYER and NC1 nodes carry no controller motion — their time is the owning plane's.
    static Fin<WalkState> Node(WalkState st, GNode node, SimulatePolicy policy) =>
        node.Switch(
            word: w => Word(st, w, policy),
            cannedCycle: c => Walk(c.ExpandedMoves.Map(GNode.Move), Fin.Succ(Fixed(st, c.Command, c.P, pierce: c.Command == GCommand.Pierce)), policy),
            macro: m => Walk(toSeq(m.Body), Fin.Succ(st), policy),
            subprogram: s => toSeq(Enumerable.Range(0, Math.Max(1, s.Repeats)))
                .Fold(Fin.Succ(st), (acc, _) => acc.Bind(x => Walk(toSeq(s.Body), Fin.Succ(x), policy))),
            additiveLayer: _ => Fin.Succ(st),
            nc1: _ => Fin.Succ(st));

    static Fin<WalkState> Word(WalkState st, GNode.Word w, SimulatePolicy policy) =>
        Transitions.Find(w.Command).Match(
            None: () => Fin.Fail<WalkState>(GeometryFault.DegenerateInput($"simulate:unmapped-command:{w.Command.Key}").ToError()),
            Some: kind => {
                (ControllerState next, BlockEffect effect) = Apply(kind, st.State, w);
                return Gate(next, kind, st.Block, policy).Map(_ => Advance(st, next, kind, w.Command, effect, policy));
            });

    static WalkState Advance(WalkState st, ControllerState next, TransitionKind kind, GCommand command, BlockEffect effect, SimulatePolicy policy) {
        double seconds = Seconds(st.State.VelocityMmS, effect, kind, policy);
        (double Cut, double Rapid, double Dwell) clock =
            kind == TransitionKind.Rapid       ? (st.Clock.Cut, st.Clock.Rapid + seconds, st.Clock.Dwell)
            : effect.FixedSeconds > 0.0        ? (st.Clock.Cut, st.Clock.Rapid, st.Clock.Dwell + seconds)
                                               : (st.Clock.Cut + seconds, st.Clock.Rapid, st.Clock.Dwell);
        return (next with { VelocityMmS = effect.TargetFeedMmS }, clock,
                st.Swaps + (effect.ToolSwap ? 1 : 0), st.Pierces + (effect.Pierce ? 1 : 0),
                st.Ledger.Add(new BlockTime(st.Block, command, seconds, effect.SpanMm, effect.TargetFeedMmS * 60.0)), st.Block + 1);
    }

    // Row-dispatched transition — the kind axis owns the behavior, the word supplies the payload; every modal
    // command stamps its Registers slot through GroupSlots.
    static (ControllerState, BlockEffect) Apply(TransitionKind kind, ControllerState s, GNode.Word w) {
        Point3d to = new(P(w, 'X').IfNone(s.At.X), P(w, 'Y').IfNone(s.At.Y), P(w, 'Z').IfNone(s.At.Z));
        double f = P(w, 'F').IfNone(s.F);
        double span = w.Command == GCommand.ArcCw || w.Command == GCommand.ArcCcw ? ArcSpan(s.At, to, w) : s.At.DistanceTo(to);
        ControllerState stamped = s with {
            Registers = GroupSlots.Find(w.Command.Group).Match(
                Some: slot => s.Registers.AddOrUpdate(slot, w.Command.Code),
                None: () => s.Registers),
        };
        ControllerState moved = stamped with { Motion = w.Command, At = to, F = f, Feed = w.Mode.IfNone(s.Feed) };
        return kind.Switch(
            rapid:       () => (moved, new BlockEffect(span, 0.0, 0.0, false, false)),
            cut:         () => (moved, new BlockEffect(span, f / 60.0, 0.0, false, false)),
            dwell:       () => (stamped, new BlockEffect(0.0, s.VelocityMmS, P(w, 'P').IfNone(0.0), false, false)),
            pierce:      () => (stamped, new BlockEffect(0.0, 0.0, P(w, 'P').IfNone(0.0), false, true)),
            toolSwap:    () => (stamped with { T = P(w, 'T'), LengthComp = false }, new BlockEffect(0.0, 0.0, 0.0, true, false)),
            lengthComp:  () => (stamped with { LengthComp = true }, new BlockEffect(0.0, s.VelocityMmS, 0.0, false, false)),
            spindleOn:   () => (stamped with { SpindleOn = true, S = P(w, 'S').IfNone(s.S) }, new BlockEffect(0.0, s.VelocityMmS, 0.0, false, false)),
            coolantOn:   () => (stamped with { CoolantOn = true }, new BlockEffect(0.0, s.VelocityMmS, 0.0, false, false)),
            setRegister: () => (stamped with { S = P(w, 'S').IfNone(s.S) }, new BlockEffect(0.0, s.VelocityMmS, 0.0, false, false)),
            modalOnly:   () => (stamped, new BlockEffect(0.0, s.VelocityMmS, 0.0, false, false)),
            stop:        () => (stamped with { Stopped = true, SpindleOn = false, CoolantOn = false }, new BlockEffect(0.0, 0.0, 0.0, false, false)));
    }

    // Canned-cycle fixed cost: P seconds on the dwell clock, pierce census when the cycle IS the pierce.
    static WalkState Fixed(WalkState st, GCommand command, double seconds, bool pierce) =>
        (st.State, (st.Clock.Cut, st.Clock.Rapid, st.Clock.Dwell + seconds), st.Swaps, st.Pierces + (pierce ? 1 : 0),
         st.Ledger.Add(new BlockTime(st.Block, command, seconds, 0.0, 0.0)), st.Block + 1);

    // Trapezoid under v² = v₀² + 2·a·s (triangle when the span cannot reach F); rapids at the limits rate;
    // dwell/pierce carry seconds in the P slot per the program AST law. Evaluation only — never planning.
    static double Seconds(double vIn, BlockEffect e, TransitionKind kind, SimulatePolicy policy) {
        if (e.FixedSeconds > 0.0) return e.FixedSeconds;
        if (e.SpanMm <= 1e-9) return 0.0;
        double a = Math.Max(1e-6, policy.Dynamics.Acceleration);
        double target = kind == TransitionKind.Rapid ? policy.Dynamics.RapidFeed / 60.0 : Math.Max(1e-6, e.TargetFeedMmS);
        double accelDist = Math.Abs(target * target - vIn * vIn) / (2.0 * a);
        if (accelDist >= e.SpanMm) {
            double vPeak = Math.Sqrt(Math.Max(vIn * vIn, 2.0 * a * e.SpanMm));
            return (vPeak - Math.Min(vIn, vPeak)) / a + e.SpanMm / Math.Max(1e-6, 0.5 * (vIn + vPeak));
        }
        return Math.Abs(target - vIn) / a + (e.SpanMm - accelDist) / target;
    }

    static double ArcSpan(Point3d from, Point3d to, GNode.Word w) {
        Point3d center = new(from.X + P(w, 'I').IfNone(0.0), from.Y + P(w, 'J').IfNone(0.0), from.Z);
        double r = from.DistanceTo(center);
        if (r <= 1e-9) return from.DistanceTo(to);
        double a0 = Math.Atan2(from.Y - center.Y, from.X - center.X), a1 = Math.Atan2(to.Y - center.Y, to.X - center.X);
        double sweep = w.Command == GCommand.ArcCw ? (a0 - a1 + 2.0 * Math.PI) % (2.0 * Math.PI) : (a1 - a0 + 2.0 * Math.PI) % (2.0 * Math.PI);
        return r * (sweep <= 1e-9 ? 2.0 * Math.PI : sweep);
    }

    static Option<double> P(GNode.Word w, char address) =>
        w.Words.Find(p => p.Address == address).Map(static p => p.Value);

    // Envelope gate: cut moves hard-gate per axis (2738); rapids gate at the soft-limit margin (2739).
    // Rotary A/B/C limits are Kinematics/machine's rows — declared, not gated here.
    static Fin<Unit> Gate(ControllerState next, TransitionKind kind, int block, SimulatePolicy policy) =>
        policy.Machine.Match(
            None: () => Fin.Succ(unit),
            Some: m => Axes(next.At).Fold(Fin.Succ(unit), (acc, axis) => acc.Bind(_ => {
                (MachineAxis key, double at, double lo, double hi) = (axis.Key, axis.At, Lo(m.Envelope, axis.Key), Hi(m.Envelope, axis.Key));
                if (at >= lo && at <= hi) return Fin.Succ(unit);
                double limit = at < lo ? lo : hi;
                return kind == TransitionKind.Rapid && Math.Abs(at - limit) <= policy.SoftLimitMarginMm
                    ? Fin.Succ(unit)
                    : kind == TransitionKind.Rapid
                        ? Fin.Fail<Unit>(new FabricationFault.SimulatedOvertravel(block, key, Math.Abs(at - limit)).ToError())
                        : Fin.Fail<Unit>(new FabricationFault.EnvelopeExceeded(key, at, limit).ToError());
            })));

    static Seq<(MachineAxis Key, double At)> Axes(Point3d p) => Seq((MachineAxis.X, p.X), (MachineAxis.Y, p.Y), (MachineAxis.Z, p.Z));

    static double Lo(BoundingBox e, MachineAxis a) => a == MachineAxis.X ? e.Min.X : a == MachineAxis.Y ? e.Min.Y : e.Min.Z;

    static double Hi(BoundingBox e, MachineAxis a) => a == MachineAxis.X ? e.Max.X : a == MachineAxis.Y ? e.Max.Y : e.Max.Z;
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Program["Posting/program CutProgram AST (landed)"] -->|block stream| Execute["Simulate.Execute"]
    Fleet["Kinematics/fleet MachineInstance.Envelope"] -->|envelope truth| Execute
    Limits["Kinematics/machine MotionDynamics — the ONE motion-dynamics law"] -->|values, never a second planner| Execute
    Execute -->|"GNode Switch + Transitions row table over ControllerState (13-register census)"| Ledger["BlockTime ledger + clocks"]
    Ledger --> Receipt["SimulationReceipt cycle/cut/rapid/dwell · swaps · pierces"]
    Execute -.->|cut move out of envelope| E1["EnvelopeExceeded 2738"]
    Execute -.->|rapid past soft-limit| E2["SimulatedOvertravel 2739"]
    Receipt -->|priced| Estimation["Verify/estimation"]
    Receipt -->|quoted| Traveler["Documentation/traveler"]
    Receipt -->|re-measures deltas| Optimization["Posting/optimization"]
```
