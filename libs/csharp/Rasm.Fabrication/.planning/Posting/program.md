# [RASM_FABRICATION_PROGRAM]

`Post` owns the dialect-neutral cut-program AST, the `Run(Post{Motion, PostDialect})` lowering body, the live placement cut-conditioning rail, the RS274 word-parse ingress, and the `PackKind.Toolpath` publish-back egress. `GNode` is the sole program vocabulary consumed by `Dialect.Emit`; `CutProgram` carries the AST plus its `ContentKey` minted over the full canonical node payload, and `PostedProgram` carries lowered block text plus the `EgressKind.CutProgram` content key. Motion enters already CAM-conditioned; placement-style 2D cutting enters the local cut-conditioning rail exactly once through the `Lower(FabricationResult.Placement, ...)` overload — one `Lower` name, the input type the modality discriminant: placed loops project through the atoms-owner `PartTransform.Apply`, arc-native kerf and lead compose `ArcAlgebra`, unlinked contour order composes `PolygonAlgebra.NestingOrder` under an explicit `PolygonFill`, committed chain topology consumes the `Nesting/linking` `ChainRow` rows — one pierce per chain at its recorded point, never per-loop reconstruction — WCS rows compose `Setup.Schedule` and lower through the `GCommand.Wcs` row (never a `G90 P` fiction), tool swaps compose `Magazine.Schedule`, bevel work renders as coupled `Toolpath/bevel` `BevelPass` sections — each pass's `Blocks` and `ThcSpan` windows lowered together, never a detached span bag — and line-run recovery alone composes `g3.BiArcFit2` — a bulged span emits its arc from its own `Bulges` column and never refits.

## [01]-[INDEX]

- [01]-[CUT_PROGRAM]: owns `GCommand`, the fault-only `GNodeKind` taxonomy, `ModalGroup`, `CoolingPolicy`, `GNode`, `GWord`, `CutProgram`, `PostPolicy`, the `Run(Post)` lowering body, placement cut-conditioning, line-run biarc recovery, `Post.Parse`, and `Post.Publish`.

## [02]-[CUT_PROGRAM]

- Owner: `GNode` is the neutral AST consumed by `Dialect.Emit`; `GWord` is the lowered word evidence; `CutProgram` stores nodes, dialect, and the AST content key over the complete canonical node bytes; `OperationBoundary` maps cumulative minutes and parts to operation-owned AST loci; `Post` owns `Lower` (both modalities), `Assemble`, `Parse`, `Publish`, `Lookahead`, and the placement cut-conditioning rail.
- Cases: `GNodeKind` carries only the six-case unsupported-node fault taxonomy; the `GNode` union itself is the executable discriminant, and macro/subprogram cases carry no duplicate kind field. `ModalGroup` rows 14 add retract and path-control state to motion, plane, distance, units, feed, spindle, coolant, cutter-comp, tool-length, WCS, stop, and non-modal; `LeadStyle` rows `none` · `line` · `arc`; `FeedMode` rows `units-per-minute` · `inverse-time`; `CoolingPolicy` rows `off` · `mist` · `flood`; `GCommand` rows 54 cover the four `G38` probe modes, `G10` WCS assignment, retract/path control, temperature waits, and the machining/additive command families.
- Entry: `Run(Post{FabricationResult.Motion, PostDialect})` calls `Post.Lower(FabricationResult.Motion, PostDialect, FabricationInput)`; the policy-bearing overload requires `PostPolicy` rather than encoding absence as `null`. `Post.Lower(FabricationResult.Placement, ...)` is the placement overload under the same name; `Post.Parse` is the packed-word/comment-aware invariant-culture ingress; `Post.Publish` realizes the `PackKind.Toolpath` egress.
- Auto: `Lower` assembles the source, emits `G10 L2 P… X…Y…Z…` datum values from each scheduled `Setup.Frame`, selects its WCS row, places initial tool loads before the first node and every mid-job reload at the first matching cumulative operation boundary, brackets each reload with spindle, coolant, and length-compensation safe state, lowers nodes through `Dialect.Emit`, recursively seals faults and physical-record count, renders `WordRetention` through a stateful modal fold, and mints `ContentKey.Of`. Placement projects each `PartTransform` through the atoms-owner `Apply`, then dispatches on the carried chain topology: empty `Chains` orders loops through `NestingOrder(PolygonFill.NonZero)` with one pierce per loop; committed `ChainRow` rows govern path order and pierce emission directly — one pierce at the chain's recorded point, members threaded torch-live in threaded order. Each `BevelPass` lowers as one coupled section, its own `ThcSpan` windows interleaved at their block boundaries. Cutter compensation binds the fallible Kienzle force result from admitted cut width and a `FeedBasis.PerTooth` chip thickness, then applies dimensionally complete thermal growth `α·L·ΔT`; absent cutting evidence suppresses only the force term. Pierce always energizes the source, while positive dwell adds the cycle. The parser removes parenthesized and semicolon comments, tokenizes packed words, admits multiple commands per block, and routes modal conflicts or malformed parameters typed.
- Receipt: `CutProgram.Key` identifies the AST payload byte-completely; `FabricationResult.PostedProgram(Seq<string> Blocks, ContentKey Key)` identifies the lowered machine artifact; `ProgramParse` and `BlockCapExceeded` carry typed failure evidence.
- Packages: `Geometry2D/arcs` (`KerfArc`, `LeadArc`, `SampleAtLength`, `ArcOffset`); `Geometry2D/algebra` (`NestingOrder` with explicit `PolygonFill`); `Nesting/linking` (`ChainRow` — the committed sheet, instance, source, and pierce topology consumed at lowering, never rebuilt); `Tooling/magazine` (`Schedule`, `ToolChange` with `ToolLifeType` trigger basis); `Fixturing/setups` (`Setup.Schedule`, `WcsSlot` ordinal + family); `Fixturing/workholding` (`Condition`); `Toolpath/bevel` (`BevelPass` coupled sections rendered here, decided there — `ThcDirective` words derive from each pass's own spans); `Posting/dialect` (`Dialect.Emit`, `Seal`, `Nc1Canonical`); `Posting/optimization` (`CutProgram` AST consumer); `Process/owner` (`Move`, `PartTransform.Apply`, `EgressKind`, `ContentKey`); `Process/faults` (`ProgramParse`, `BlockCapExceeded`); `Rasm.Numerics` (`GeometryFault`, `Sign`); kernel `Rasm.Spatial`/`Rasm.Drawing` (`VectorCloud.Polyline`, `PackOp.Toolpath`, `Encode.Apply`, `EncodedGeometry`, `PackPolicy` — the publish-back wire); `geometry3Sharp` (`BiArcFit2`, `Arc2d`, `Segment2d`, `Vector2d`) for line-run fitting only.
- Growth: a controller grammar change lands in `PostDialect` and `Dialect.Emit`; a program syntax construct lands as one `GNode` case and one lowering arm; a parser modal group lands as one `ModalGroup` row with its `GCommand` rows re-grouped in the same edit; a cut-conditioning concern lands as a `PostPolicy` row or an owned page seam, not a second public post fold; every `GCommand` row binds one `Verify/simulate` `Transitions` entry in the same pass — the roster and the transition table are lockstep.
- Boundary: the former public post fold is dead; the former posting-side contour order is dead; posting renders setup WCS rows and bevel passes rather than assigning or deciding them; raw `XxHash128`/`GenerateHash`, `GetHashCode`-derived content identity, a hash-digest scheduler operation key, raw G-code string synthesis, default WCS selection, a `G90 P` word standing in for a WCS row, a second kerf pass over `Motion`, parse success probes, a silent `Somes()` parameter drop, a g3 refit over arc-native (bulged) spans, a tab bridged with the energy source live, a plane-internal `CutProgram` result payload, a page-local placement transform kernel, a detached policy-level THC span bag, chain topology rebuilt from `PartTransform` rows, and per-loop pierce emission for linked chains are rejected shapes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using g3;
using LanguageExt;
using LanguageExt.Common;
using MTConnect.Assets.CuttingTools;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Fabrication.Toolpath;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GNodeKind {
    public static readonly GNodeKind Word = new("word");
    public static readonly GNodeKind CannedCycle = new("canned-cycle");
    public static readonly GNodeKind Macro = new("macro");
    public static readonly GNodeKind Subprogram = new("subprogram");
    public static readonly GNodeKind AdditiveLayer = new("additive-layer");
    public static readonly GNodeKind Nc1 = new("nc1");
}

// The 14-group parse census: every GCommand row binds its TRUE group, so a same-block conflict
// (two words of one modal group in one block) is constructable — the parse law is live, never vacuous.
[SmartEnum<string>]
public sealed partial class ModalGroup {
    public static readonly ModalGroup Motion = new("motion");
    public static readonly ModalGroup Plane = new("plane");
    public static readonly ModalGroup Distance = new("distance");
    public static readonly ModalGroup Units = new("units");
    public static readonly ModalGroup Feed = new("feed");
    public static readonly ModalGroup Spindle = new("spindle");
    public static readonly ModalGroup Coolant = new("coolant");
    public static readonly ModalGroup CutterComp = new("cutter-comp");
    public static readonly ModalGroup ToolLength = new("tool-length");
    public static readonly ModalGroup Wcs = new("wcs");
    public static readonly ModalGroup Retract = new("retract");
    public static readonly ModalGroup PathControl = new("path-control");
    public static readonly ModalGroup Stop = new("stop");
    public static readonly ModalGroup NonModal = new("non-modal");
}

[SmartEnum<string>]
public sealed partial class LeadStyle {
    public static readonly LeadStyle None = new("none", static (_, _) => Fin.Succ(Seq<Move>()));
    public static readonly LeadStyle Line = new("line", Post.LineLead);
    public static readonly LeadStyle Arc = new("arc", Post.ArcLead);

    [UseDelegateFromConstructor]
    public partial Fin<Seq<Move>> Enter(Loop loop, PostPolicy policy);
}

[SmartEnum<string>]
public sealed partial class FeedMode {
    public static readonly FeedMode UnitsPerMinute = new("units-per-minute", "G94");
    public static readonly FeedMode InverseTime = new("inverse-time", "G93");

    public string Code { get; }
}

// Coolant policy rows carry their emission command; ToolSections emits the row's word per section when not Off.
[SmartEnum<string>]
public sealed partial class CoolingPolicy {
    public static readonly CoolingPolicy Off = new("off", static () => None);
    public static readonly CoolingPolicy Mist = new("mist", static () => Some(GCommand.CoolantMist));
    public static readonly CoolingPolicy Flood = new("flood", static () => Some(GCommand.Coolant));

    [UseDelegateFromConstructor]
    public partial Option<GCommand> Word();
}

[SmartEnum<string>]
public sealed partial class GCommand {
    public static readonly GCommand Rapid = new("rapid", "G0", ModalGroup.Motion);
    public static readonly GCommand Feed = new("feed", "G1", ModalGroup.Motion);
    public static readonly GCommand ArcCw = new("arc-cw", "G2", ModalGroup.Motion);
    public static readonly GCommand ArcCcw = new("arc-ccw", "G3", ModalGroup.Motion);
    public static readonly GCommand Extrude = new("extrude", "G1", ModalGroup.Motion);
    public static readonly GCommand ThreadCycle = new("thread-cycle", "G92", ModalGroup.Motion);
    public static readonly GCommand PlaneXy = new("plane-xy", "G17", ModalGroup.Plane);
    public static readonly GCommand PlaneZx = new("plane-zx", "G18", ModalGroup.Plane);
    public static readonly GCommand PlaneYz = new("plane-yz", "G19", ModalGroup.Plane);
    public static readonly GCommand Absolute = new("absolute", "G90", ModalGroup.Distance);
    public static readonly GCommand Relative = new("relative", "G91", ModalGroup.Distance);
    public static readonly GCommand Metric = new("metric", "G21", ModalGroup.Units);
    public static readonly GCommand Inch = new("inch", "G20", ModalGroup.Units);
    public static readonly GCommand FeedPerMinute = new("feed-per-minute", "G94", ModalGroup.Feed);
    public static readonly GCommand FeedInverseTime = new("feed-inverse-time", "G93", ModalGroup.Feed);
    public static readonly GCommand Spindle = new("spindle", "M3", ModalGroup.Spindle);
    public static readonly GCommand SpindleCcw = new("spindle-ccw", "M4", ModalGroup.Spindle);
    public static readonly GCommand SpindleStop = new("spindle-stop", "M5", ModalGroup.Spindle);
    public static readonly GCommand Css = new("css", "G96", ModalGroup.Spindle);
    public static readonly GCommand CssCancel = new("css-cancel", "G97", ModalGroup.Spindle);
    public static readonly GCommand TorchOn = new("torch-on", "M07", ModalGroup.Spindle);
    public static readonly GCommand Coolant = new("coolant", "M8", ModalGroup.Coolant);
    public static readonly GCommand CoolantMist = new("coolant-mist", "M7", ModalGroup.Coolant);
    public static readonly GCommand CoolantOff = new("coolant-off", "M9", ModalGroup.Coolant);
    public static readonly GCommand AssistGas = new("assist-gas", "M64", ModalGroup.Coolant);
    public static readonly GCommand DustCollect = new("dust-collect", "M65", ModalGroup.Coolant);
    public static readonly GCommand CompOff = new("comp-off", "G40", ModalGroup.CutterComp);
    public static readonly GCommand CompLeft = new("comp-left", "G41", ModalGroup.CutterComp);
    public static readonly GCommand CompRight = new("comp-right", "G42", ModalGroup.CutterComp);
    public static readonly GCommand LengthOffset = new("length-offset", "G43", ModalGroup.ToolLength);
    public static readonly GCommand LengthCancel = new("length-cancel", "G49", ModalGroup.ToolLength);
    public static readonly GCommand Wcs = new("wcs", "G54", ModalGroup.Wcs);
    public static readonly GCommand SetWcs = new("set-wcs", "G10", ModalGroup.NonModal);
    public static readonly GCommand RetractInitial = new("retract-initial", "G98", ModalGroup.Retract);
    public static readonly GCommand RetractPlane = new("retract-plane", "G99", ModalGroup.Retract);
    public static readonly GCommand ExactStop = new("exact-stop", "G61", ModalGroup.PathControl);
    public static readonly GCommand ExactStopCheck = new("exact-stop-check", "G61.1", ModalGroup.PathControl);
    public static readonly GCommand Continuous = new("continuous", "G64", ModalGroup.PathControl);
    public static readonly GCommand ProgramEnd = new("program-end", "M30", ModalGroup.Stop);
    public static readonly GCommand Stop = new("stop", "M0", ModalGroup.Stop);
    public static readonly GCommand OptionalStop = new("optional-stop", "M1", ModalGroup.Stop);
    public static readonly GCommand Dwell = new("dwell", "G4", ModalGroup.NonModal);
    public static readonly GCommand Pierce = new("pierce", "G4", ModalGroup.NonModal);
    public static readonly GCommand Probe = new("probe", "G31", ModalGroup.NonModal);
    public static readonly GCommand ProbeTowardStop = new("probe-toward-stop", "G38.2", ModalGroup.NonModal);
    public static readonly GCommand ProbeTowardOptional = new("probe-toward-optional", "G38.3", ModalGroup.NonModal);
    public static readonly GCommand ProbeAwayStop = new("probe-away-stop", "G38.4", ModalGroup.NonModal);
    public static readonly GCommand ProbeAwayOptional = new("probe-away-optional", "G38.5", ModalGroup.NonModal);
    public static readonly GCommand TorchHeight = new("torch-height", "THC", ModalGroup.NonModal);
    public static readonly GCommand HotendTemp = new("hotend-temp", "M104", ModalGroup.NonModal);
    public static readonly GCommand HotendWait = new("hotend-wait", "M109", ModalGroup.NonModal);
    public static readonly GCommand BedTemp = new("bed-temp", "M140", ModalGroup.NonModal);
    public static readonly GCommand BedWait = new("bed-wait", "M190", ModalGroup.NonModal);
    public static readonly GCommand ToolChange = new("tool-change", "M6", ModalGroup.NonModal);

    public string Code { get; }
    public ModalGroup Group { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct GParam(char Address, double Value) {
    public GParam Round(int decimals) => new(Address, Math.Round(Value, decimals));
}

public readonly record struct MacroSlot(int Index, string Key, double Value);

public readonly record struct TemperatureSet(double Hotend, double Bed);

public readonly record struct ExtrusionProfile(double Amount, double Feed);

public readonly record struct FeedPolicy(double EdgeDistance, double EdgeFloor) {
    public static readonly FeedPolicy Canonical = new(EdgeDistance: 2.0, EdgeFloor: 0.5);
}

public readonly record struct BiarcPolicy(double FitTolerance, double MinRunLength) {
    public static readonly BiarcPolicy Canonical = new(FitTolerance: 0.02, MinRunLength: 1.0);
}

public readonly record struct OperationBoundary(Operation Op, int Node, double MinutesConsumed, int PartsConsumed);

public readonly record struct CompPolicy(
    double ToolDiameter,
    double CutWidthMm,
    double Stickout,
    double Modulus,
    double ThermalCoefficientPerC,
    double TemperatureDeltaC) {
    public static readonly CompPolicy Disabled = new(
        ToolDiameter: 0.0, CutWidthMm: 0.0, Stickout: 0.0, Modulus: 0.0,
        ThermalCoefficientPerC: 0.0, TemperatureDeltaC: 0.0);

    public bool Mechanical => ToolDiameter > 0.0 || CutWidthMm > 0.0 || Modulus > 0.0;

    public bool Thermal => ThermalCoefficientPerC != 0.0 || TemperatureDeltaC != 0.0;

    public double Stiffness =>
        ToolDiameter <= 0.0 || Stickout <= 0.0 || Modulus <= 0.0
            ? double.PositiveInfinity
            : 3.0 * Modulus * (Math.PI * Math.Pow(ToolDiameter, 4.0) / 64.0) / Math.Pow(Stickout, 3.0);

    public double Deflection(double radialForce) => double.IsPositiveInfinity(Stiffness) ? 0.0 : radialForce / Stiffness;

    public double ThermalGrowth => ThermalCoefficientPerC * Stickout * TemperatureDeltaC;
}

public sealed record PostPolicy(
    double KerfWidth,
    LeadStyle Lead,
    double LeadRadius,
    double TabWidth,
    double TabSpacing,
    double PierceTime,
    double AssistPressure,
    double MeltTemp,
    double FeedCeiling,
    int MaxAxes,
    double RemovalRate,
    FeedPolicy Feed,
    BiarcPolicy Biarc,
    MotionDynamics Dynamics,
    Option<CuttingData> Cutting,
    CompPolicy Comp,
    CoolingPolicy Cooling,
    Seq<BevelPass> Bevels,
    Seq<ChainRow> Chains,
    Set<OptimizePass> Passes,
    MrrPolicy Mrr,
    SmoothPolicy Smooth,
    CompactPolicy Compact,
    Arr<Loop> Profiles,
    Seq<WorkItem> Work,
    Magazine Magazine,
    SlotMap Slots,
    MagazinePolicy MagazinePolicy,
    Seq<Operation> Operations,
    Seq<OperationBoundary> OperationBoundaries,
    SchedulePolicy<Operation> Schedule,
    Fixture Fixture) {
    // The owner `Run(Post)` derivation binds everything the input carries; shop context absent from the
    // input (magazine, work, operations, fixture, bevel passes, chain rows) seeds empty so the arm is TOTAL
    // and prologue rows degrade to absence — the required policy overload carries richer shop state under the
    // same entry, and a `LinkPlan`'s baseline chains ride `Chains` unchanged (baseline behavior is explicit).
    public static PostPolicy From(FabricationInput input, PostDialect dialect) => new(
        KerfWidth: 0.0, Lead: LeadStyle.None, LeadRadius: 0.0, TabWidth: 0.0, TabSpacing: 0.0,
        PierceTime: 0.0, AssistPressure: 0.0, MeltTemp: 0.0,
        FeedCeiling: MotionDynamics.Canonical.CuttingFeed, MaxAxes: input.Machine.AxisCount,
        RemovalRate: 0.0, Feed: FeedPolicy.Canonical, Biarc: BiarcPolicy.Canonical,
        Dynamics: MotionDynamics.Canonical, Cutting: None,
        Comp: CompPolicy.Disabled, Cooling: CoolingPolicy.Off, Bevels: Seq<BevelPass>(), Chains: Seq<ChainRow>(),
        Passes: Set<OptimizePass>(), Mrr: MrrPolicy.Disabled(MotionDynamics.Canonical.CuttingFeed),
        Smooth: SmoothPolicy.Canonical, Compact: CompactPolicy.Canonical,
        Profiles: input.Profiles, Work: Seq<WorkItem>(), Magazine: Magazine.Manual, Slots: SlotMap.Empty,
        MagazinePolicy: MagazinePolicy.Canonical, Operations: Seq<Operation>(),
        OperationBoundaries: Seq<OperationBoundary>(),
        Schedule: SchedulePolicy<Operation>.Direct(input.Machine, dialect, StableOperationKey),
        Fixture: Fixture.Free);

    // Scheduler identity is the closed Operation roster ordinal — injective by construction over the smart-enum
    // items, so distinct operations can never alias; the 31-fold string digest was the deleted many-to-one form
    // that converted key collisions into duplicate-identity refusals inside Setup.Validate.
    private static readonly Map<string, int> OperationOrdinal =
        toMap(toSeq(Operation.Items).Map((operation, index) => (operation.Key, index)));

    private static int StableOperationKey(Operation operation) => OperationOrdinal[operation.Key];

    // ONE placement projection body: the atoms-owner PartTransform.Apply — nesting, linking, and posting
    // observe identical transform and Loop.Admit semantics; a page-local trigonometric kernel is the deleted form.
    public Fin<Loop> Placed(PartTransform t) =>
        t.PartId >= Profiles.Count
            ? Fin.Fail<Loop>(GeometryFault.DegenerateInput($"post:profile:{t.PartId}").ToError())
            : t.Apply(Profiles[t.PartId]);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GNode {
    private GNode() { }

    public sealed record Word(GCommand Command, Arr<GParam> Words, Option<FeedMode> Mode) : GNode {
        public Option<double> P(char address) => Words.Find(p => p.Address == address).Map(static p => p.Value);
        public Word With(char address, double value) => this with { Words = Words.Filter(p => p.Address != address).Add(new GParam(address, value)) };
        public Word Without(char address) => this with { Words = Words.Filter(p => p.Address != address) };
    }
    public sealed record CannedCycle(GCommand Command, Arr<GParam> SingleBlockWords, Seq<Move> ExpandedMoves, int Repeats, Option<FeedMode> Mode) : GNode {
        public double R => SingleBlockWords.Find(static word => word.Address == 'R').Map(static word => word.Value).IfNone(0.0);
        public double Q => SingleBlockWords.Find(static word => word.Address == 'Q').Map(static word => word.Value).IfNone(0.0);
        public double P => SingleBlockWords.Find(static word => word.Address == 'P').Map(static word => word.Value).IfNone(0.0);
    }
    public sealed record Macro(Arr<MacroSlot> Slots, Arr<GNode> Body) : GNode;
    public sealed record Subprogram(int Label, int Repeats, Arr<GNode> Body) : GNode;
    public sealed record AdditiveLayer(int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures) : GNode;
    public sealed record Nc1(SteelImportReceipt Receipt) : GNode;

    // Arc I/J are relative to the arc START (RS274), so the factory demands the predecessor point; the
    // sequence factory threads the cursor so no call site can hand a wrong start.
    public static GNode Move(Move move, Point3d from) =>
        move.Switch(
            state: from,
            rapid: static (_, row) => new Word(GCommand.Rapid,
                Arr(new GParam('X', row.Target.X), new GParam('Y', row.Target.Y), new GParam('Z', row.Target.Z)), None),
            linear: static (_, row) => new Word(GCommand.Feed,
                Arr(new GParam('X', row.Target.X), new GParam('Y', row.Target.Y), new GParam('Z', row.Target.Z), new GParam('F', row.Feed)), None),
            circular: static (start, row) => new Word(
                row.Arc.Sense == RotationSense.Clockwise ? GCommand.ArcCw : GCommand.ArcCcw,
                Arr(new GParam('X', row.Target.X), new GParam('Y', row.Target.Y), new GParam('Z', row.Target.Z),
                    new GParam('I', row.Arc.Center.X - start.X), new GParam('J', row.Arc.Center.Y - start.Y), new GParam('F', row.Feed)), None));

    public static Seq<GNode> Moves(Seq<Move> moves, Point3d origin) =>
        moves.Fold((Nodes: Seq<GNode>(), Cursor: origin), static (acc, move) =>
            (acc.Nodes.Add(Move(move, acc.Cursor)), Target(move))).Nodes;

    public static Point3d Target(Move move) => move.Switch(
        rapid: static row => row.Target,
        linear: static row => row.Target,
        circular: static row => row.Target);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GWord {
    private GWord() { }

    public sealed record Address(string Code, ModalGroup Group, Arr<GParam> Words, Option<FeedMode> Mode, WordRetention Retention) : GWord;
    public sealed record Conversational(string Verb, Arr<GParam> Words, int Decimals, string Tail) : GWord;
    public sealed record Text(Seq<string> Records) : GWord;
    public sealed record CycleCall(string Dialect, string Code, Arr<GParam> Words, int Repeats) : GWord;
    public sealed record Macro(MacroGrammar Grammar, Arr<MacroSlot> Slots, Seq<GWord> Body) : GWord;
    public sealed record Subprogram(SubprogramGrammar Grammar, int Label, int Repeats, Seq<GWord> Body) : GWord;
    public sealed record Additive(int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures, int Decimals) : GWord;
    public sealed record Nc1(Seq<string> Records, ContentKey Key) : GWord;
    public sealed record Fault(Error Error) : GWord;
    public sealed record Expanded(Seq<GWord> Words) : GWord;
}

public sealed record CutProgram(Seq<GNode> Nodes, PostDialect Dialect, ContentKey Key) {
    public static CutProgram Of(Seq<GNode> nodes, PostDialect dialect) =>
        new(nodes, dialect, ContentKey.Of(EgressKind.CutProgram, Canonical(nodes, dialect)));

    // Content identity serializes EVERY node field — command keys, raw param values, cycle scalars, macro
    // slots, subprogram labels, additive rows, NC1 keys; a GetHashCode digest is the rejected non-content form.
    public static byte[] Canonical(Seq<GNode> nodes, PostDialect dialect) =>
        Encoding.UTF8.GetBytes($"{dialect.Key}\n{string.Join("\n", nodes.Map(NodeKey).ToArray())}");

    private static string NodeKey(GNode node) =>
        node.Switch(
            word:          static w => $"w:{w.Command.Key}:{w.Mode.Map(static m => m.Key).IfNone("")}:{string.Join(",", w.Words.Map(static p => $"{p.Address}={p.Value:R}"))}",
            cannedCycle:   static c => $"c:{c.Command.Key}:{c.Mode.Map(static m => m.Key).IfNone("")}:{c.Repeats}:{string.Join(",", c.SingleBlockWords.Map(static p => $"{p.Address}={p.Value:R}"))}:{string.Join("|", c.ExpandedMoves.Map(MoveKey))}",
            macro:         static m => $"m:{m.Slots.Count}:{string.Join(",", m.Slots.Map(static s => $"{s.Index}:{s.Key.Length}:{s.Key}={s.Value:R}"))}:{m.Body.Count}:[{string.Join(";", m.Body.Map(NodeKey))}]",
            subprogram:    static s => $"s:{s.Label}:{s.Repeats}:{s.Body.Count}:[{string.Join(";", s.Body.Map(NodeKey))}]",
            additiveLayer: static a => $"a:{a.Layer}:{a.Extrusion.Amount:R}:{a.Extrusion.Feed:R}:{a.Temperatures.Hotend:R}:{a.Temperatures.Bed:R}",
            nc1:           static n => $"n:{n.Receipt.Key.Kind.Key}:{n.Receipt.Key.Digest}");

    private static string MoveKey(Move move) => move.Switch(
        rapid: static row => $"r:{row.Target.X:R},{row.Target.Y:R},{row.Target.Z:R}",
        linear: static row => $"l:{row.Target.X:R},{row.Target.Y:R},{row.Target.Z:R}:{row.Feed:R}",
        circular: static row => $"c:{row.Target.X:R},{row.Target.Y:R},{row.Target.Z:R}:{row.Feed:R}:{row.Arc.Center.X:R},{row.Arc.Center.Y:R},{row.Arc.Center.Z:R}:{row.Arc.Sense.Key}");
}

// Modal state tracks the EFFECTIVE command per group across blocks — a cross-block change is a legal state
// transition; the typed conflict is two words of one modal group inside ONE block.
public sealed record ModalState(Map<ModalGroup, GCommand> Active, Seq<GNode> Nodes) {
    public static readonly ModalState Empty = new(Map<ModalGroup, GCommand>(), Seq<GNode>());

    public Fin<ModalState> Push(int line, Seq<GNode.Word> block) {
        Seq<ModalGroup> groups = block.Map(static w => w.Command.Group).Filter(static g => g != ModalGroup.NonModal);
        return groups.Distinct().Find(g => groups.Count(x => x == g) > 1).Match(
            Some: g => Fin.Fail<ModalState>(FabricationFault.ProgramParse(line, g).ToError()),
            None: () => Fin.Succ(new ModalState(
                block.Fold(Active, static (active, w) => w.Command.Group == ModalGroup.NonModal ? active : active.AddOrUpdate(w.Command.Group, w.Command)),
                Nodes.Concat(block.Map(static w => (GNode)w)))));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Post {
    private static readonly Seq<GCommand> Commands = toSeq(GCommand.Items);

    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Motion motion, PostDialect dialect, FabricationInput input) =>
        Lower(new PostSource.Motion(motion), dialect, input, PostPolicy.From(input, dialect));

    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Motion motion, PostDialect dialect, FabricationInput input, PostPolicy policy) =>
        Lower(new PostSource.Motion(motion), dialect, input, policy);

    // The placement modality — same name, input shape the discriminant; the owner Post case re-points here
    // when it widens to carry a Placement.
    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Placement placement, PostDialect dialect, FabricationInput input) =>
        Lower(new PostSource.Placement(placement), dialect, input, PostPolicy.From(input, dialect));

    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Placement placement, PostDialect dialect, FabricationInput input, PostPolicy policy) =>
        Lower(new PostSource.Placement(placement), dialect, input, policy);

    private static Fin<FabricationResult.PostedProgram> Lower(PostSource source, PostDialect dialect, FabricationInput input, PostPolicy policy) =>
        from program in Assemble(source, dialect, input, policy)
        from words in Dialect.Seal(dialect, program.Nodes.Map(node => Dialect.Emit(dialect, node)))
        let blocks = Render(words)
        let key = ContentKey.Of(EgressKind.CutProgram, Encoding.UTF8.GetBytes(string.Join("\n", blocks.ToArray())))
        select new FabricationResult.PostedProgram(blocks, key);

    public static Fin<CutProgram> Parse(string source, PostDialect dialect) =>
        Lines(source).Fold(
            Fin.Succ(ModalState.Empty),
            (state, row) =>
                from modal in state
                from block in ParseBlock(row.Line, row.Text, dialect)
                from next in modal.Push(row.Line, block)
                select next)
        .Map(modal => CutProgram.Of(modal.Nodes, dialect));

    // The branch Posting →[WIRE]: PackKind → Rasm edge: the conditioned motion polyline publishes through
    // the kernel encoder as content-keyed EncodedGeometry — backplot, storage, and analytics read the wire,
    // never re-parsed G-code.
    public static Fin<EncodedGeometry> Publish(FabricationResult.Motion motion, PackPolicy policy) =>
        from context in Context.Millimeters().ToFin()
        from cloud in VectorCloud.Polyline(motion.Moves.Map(GNode.Target), context)
        from encoded in cloud is VectorCloud.PolylineCase poly
            ? Encode.Apply(new PackOp.Toolpath(poly, policy))
            : Fin.Fail<EncodedGeometry>(GeometryFault.DegenerateInput("post:publish-cloud").ToError())
        select encoded;

    internal static Fin<CutProgram> Assemble(PostSource source, PostDialect dialect, FabricationInput input, PostPolicy policy) =>
        from _ in Admit(policy)
        from changes in ToolMagazine.Schedule(policy.Magazine, policy.Slots, policy.Work, policy.MagazinePolicy)
        from schedule in Setup.Schedule(policy.Operations, policy.Schedule)
        from program in source.Switch(
            state:     (dialect, input, policy, changes, schedule),
            motion:    static (s, m) => MotionProgram(m.Value, s.dialect, s.policy, s.changes, s.schedule),
            placement: static (s, p) => PlacementProgram(p.Value, s.dialect, s.policy, s.changes, s.schedule))
        select program;

    private static Fin<Unit> Admit(PostPolicy policy) {
        Seq<Error> errors = Seq<Error>();
        Seq<double> finite = Seq(
            policy.KerfWidth, policy.LeadRadius, policy.TabWidth, policy.TabSpacing, policy.PierceTime,
            policy.AssistPressure, policy.MeltTemp, policy.FeedCeiling, policy.RemovalRate,
            policy.Feed.EdgeDistance, policy.Feed.EdgeFloor, policy.Biarc.FitTolerance, policy.Biarc.MinRunLength,
            policy.Dynamics.RapidFeed, policy.Dynamics.CuttingFeed, policy.Dynamics.Acceleration, policy.Dynamics.Jerk,
            policy.Comp.ToolDiameter, policy.Comp.CutWidthMm, policy.Comp.Stickout, policy.Comp.Modulus,
            policy.Comp.ThermalCoefficientPerC, policy.Comp.TemperatureDeltaC);
        if (!finite.ForAll(double.IsFinite))
            errors = errors.Add(GeometryFault.DegenerateInput("post:non-finite-policy").ToError());
        if (policy.KerfWidth < 0.0 || policy.PierceTime < 0.0 || policy.AssistPressure < 0.0 || policy.MeltTemp < 0.0
            || policy.FeedCeiling <= 0.0 || policy.MaxAxes <= 0 || policy.RemovalRate < 0.0)
            errors = errors.Add(GeometryFault.DegenerateInput("post:process-policy").ToError());
        if ((policy.Lead != LeadStyle.None && policy.LeadRadius <= 0.0)
            || policy.TabWidth < 0.0 || policy.TabSpacing < 0.0
            || ((policy.TabWidth > 0.0 || policy.TabSpacing > 0.0) && (policy.TabWidth <= 0.0 || policy.TabSpacing <= policy.TabWidth)))
            errors = errors.Add(GeometryFault.DegenerateInput("post:lead-tab-policy").ToError());
        if (policy.Feed.EdgeDistance < 0.0 || policy.Feed.EdgeFloor is <= 0.0 or > 1.0
            || policy.Biarc.FitTolerance <= 0.0 || policy.Biarc.MinRunLength <= 0.0
            || policy.Dynamics.RapidFeed <= 0.0 || policy.Dynamics.CuttingFeed <= 0.0
            || policy.Dynamics.Acceleration <= 0.0 || policy.Dynamics.Jerk <= 0.0)
            errors = errors.Add(GeometryFault.DegenerateInput("post:motion-policy").ToError());
        if (policy.Cutting.Exists(data => !double.IsFinite(data.SurfaceSpeed) || data.SurfaceSpeed <= 0.0 || policy.Comp.ToolDiameter <= 0.0))
            errors = errors.Add(GeometryFault.DegenerateInput("post:spindle-policy").ToError());
        if ((policy.Comp.Mechanical && (policy.Comp.ToolDiameter <= 0.0 || policy.Comp.CutWidthMm <= 0.0
                || policy.Comp.Stickout <= 0.0 || policy.Comp.Modulus <= 0.0))
            || (policy.Comp.Thermal && (policy.Comp.ThermalCoefficientPerC <= 0.0
                || policy.Comp.TemperatureDeltaC == 0.0 || policy.Comp.Stickout <= 0.0)))
            errors = errors.Add(GeometryFault.DegenerateInput("post:compensation-policy").ToError());
        if (policy.Bevels.Exists(static pass => pass.Blocks.IsEmpty
                || pass.Thc.Exists(span => span.FromInclusive < 0 || span.ToExclusive <= span.FromInclusive || span.ToExclusive > pass.Blocks.Count)))
            errors = errors.Add(GeometryFault.DegenerateInput("post:bevel-pass").ToError());
        if (policy.Chains.Exists(static chain => chain.SheetIndex < 0 || chain.Instances.IsEmpty
                || chain.Instances.Count != chain.SourceParts.Count))
            errors = errors.Add(GeometryFault.DegenerateInput("post:chain-row").ToError());
        if (policy.OperationBoundaries.Exists(static row => row.Node < 0 || !double.IsFinite(row.MinutesConsumed)
                || row.MinutesConsumed < 0.0 || row.PartsConsumed < 0)
            || policy.OperationBoundaries.Map(static row => (row.Op, row.Node)).Distinct().Count != policy.OperationBoundaries.Count
            || policy.OperationBoundaries.GroupBy(static row => row.Op).Exists(group =>
                group.OrderBy(static row => row.Node).ToSeq().Zip(group.OrderBy(static row => row.Node).ToSeq().Skip(1))
                    .Exists(static pair => pair.Item2.MinutesConsumed < pair.Item1.MinutesConsumed
                        || pair.Item2.PartsConsumed < pair.Item1.PartsConsumed)))
            errors = errors.Add(GeometryFault.DegenerateInput("post:operation-boundary").ToError());
        return errors.IsEmpty ? Fin.Succ(unit) : Fin.Fail<Unit>(Error.Many([.. errors]));
    }

    private static Fin<CutProgram> MotionProgram(FabricationResult.Motion motion, PostDialect dialect, PostPolicy policy, Seq<ToolChange> changes, SetupSchedule schedule) =>
        from conditioned in Workholding.Condition(motion.Moves, policy.Fixture)
        from body in ToolSections(GNode.Moves(conditioned, Point3d.Origin).Concat(BevelSections(policy.Bevels)), changes, policy)
        let program = CutProgram.Of(
            Prologue(schedule).Concat(Lookahead(body, policy)).Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect)
        from optimized in OptimizeProgram(program, policy)
        select optimized;

    // Chain topology GOVERNS when present: committed ChainRow rows carry sheet, instance, source, and pierce
    // truth from Nesting/linking, so the lowering never re-derives what linking already proved; empty Chains is
    // the explicit unlinked baseline — NestingOrder contour order with one pierce per loop.
    private static Fin<CutProgram> PlacementProgram(FabricationResult.Placement placement, PostDialect dialect, PostPolicy policy, Seq<ToolChange> changes, SetupSchedule schedule) =>
        from paths in policy.Chains.IsEmpty
            ? from profiles in placement.Parts.Map(policy.Placed).TraverseM(identity).As()
              from ordered in PolygonAlgebra.NestingOrder(profiles.ToArr(), PolygonFill.NonZero)
              from loops in ordered.Map(loop => Condition(loop, policy)).TraverseM(identity).As()
              from unlinked in loops.Map(loop => CutPath(loop, policy)).TraverseM(identity).As()
              select unlinked.Bind(identity)
            : policy.Chains.Map(chain => ChainPath(placement, chain, policy)).TraverseM(identity).As()
                .Map(static chains => chains.Bind(identity))
        from body in ToolSections(paths.Concat(BevelSections(policy.Bevels)), changes, policy)
        let program = CutProgram.Of(
            Prologue(schedule).Concat(Lookahead(body, policy)).Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect)
        from optimized in OptimizeProgram(program, policy)
        select optimized;

    // The linking seam CONSUMED, never rebuilt: one pierce per chain at its recorded point, members conditioned
    // and walked in threaded order, torch-live feed transits between members — SheetIndex, Instances, SourceParts,
    // and Pierce stay coupled on the row through emission.
    private static Fin<Seq<GNode>> ChainPath(FabricationResult.Placement placement, ChainRow chain, PostPolicy policy) =>
        from _ in chain.Instances.ForAll(instance => instance >= 0 && instance < placement.Parts.Count)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(GeometryFault.DegenerateInput($"post:chain-instance:{chain.Chain}").ToError())
        from loops in chain.Instances.Map(instance => policy.Placed(placement.Parts[instance]).Bind(loop => Condition(loop, policy)))
            .TraverseM(identity).As()
        from members in loops.Map(loop =>
                from lead in policy.Lead.Enter(loop, policy)
                from body in Walk(loop, policy)
                select (Lead: lead, Body: body))
            .TraverseM(identity).As()
        select Seq<GNode>(new GNode.Word(GCommand.Rapid, Arr(new GParam('X', chain.Pierce.X), new GParam('Y', chain.Pierce.Y)), None))
            .Concat(PierceBlock(policy))
            .Concat(members.Map((member, index) => {
                Point3d start = member.Lead.HeadOrNone().Map(GNode.Target).IfNone(chain.Pierce);
                Seq<GNode> entry = member.Lead.IsEmpty ? Seq<GNode>() : GNode.Moves(member.Lead.Tail, start);
                Seq<GNode> transit = index == 0
                    ? Seq<GNode>()
                    : Seq<GNode>(new GNode.Word(GCommand.Feed,
                        Arr(new GParam('X', start.X), new GParam('Y', start.Y), new GParam('F', policy.FeedCeiling * policy.Feed.EdgeFloor)), None));
                return transit.Concat(entry).Concat(member.Body);
            }).Bind(identity));

    private static Fin<Seq<GNode>> ToolSections(Seq<GNode> nodes, Seq<ToolChange> changes, PostPolicy policy) =>
        from _ in !changes.IsEmpty && nodes.IsEmpty
            ? Fin.Fail<Unit>(GeometryFault.DegenerateInput("post:tool-program-empty").ToError())
            : Fin.Succ(unit)
        from __ in changes.Exists(static change => change.MidJob) && policy.Cutting.IsNone
            ? Fin.Fail<Unit>(GeometryFault.DegenerateInput("post:mid-job-spindle-state").ToError())
            : Fin.Succ(unit)
        from placements in changes.TraverseM(change => !change.MidJob
            ? Fin.Succ((Node: 0, Change: change))
            : policy.OperationBoundaries
                .Filter(boundary => boundary.Op == change.Op && boundary.Node < nodes.Count
                    && (change.TriggerBasis == ToolLifeType.PART_COUNT
                        ? boundary.PartsConsumed >= Math.Ceiling(change.Trigger)
                        : boundary.MinutesConsumed >= change.Trigger))
                .OrderBy(static boundary => boundary.Node).HeadOrNone()
                .ToFin(GeometryFault.DegenerateInput($"post:tool-boundary:{change.Op.Key}:{change.Trigger:R}").ToError())
                .Map(boundary => (boundary.Node, Change: change))).As()
        select toSeq(Enumerable.Range(0, nodes.Count)).Bind(index => {
            bool section = index == 0 || placements.Exists(row => row.Node == index);
            return placements.Filter(row => row.Node == index).Bind(static row => ToolChangeNodes(row.Change))
                .Concat(section ? SpindleNodes(policy.Cutting, policy.Comp.ToolDiameter) : Seq<GNode>())
                .Concat(section ? CoolingNodes(policy.Cooling) : Seq<GNode>())
                .Add(nodes[index]);
        });

    private static Seq<GNode> ToolChangeNodes(ToolChange change) =>
        Seq<GNode>(
            new GNode.Word(GCommand.SpindleStop, Arr<GParam>(), None),
            new GNode.Word(GCommand.CoolantOff, Arr<GParam>(), None),
            new GNode.Word(GCommand.LengthCancel, Arr<GParam>(), None),
            new GNode.Word(GCommand.Rapid, Arr(new GParam('Z', change.Retract)), None))
            .Concat(change.ManualConfirm
                ? Seq<GNode>(new GNode.Word(GCommand.OptionalStop, Arr<GParam>(), None))
                : Seq<GNode>())
            .Add(new GNode.Word(GCommand.ToolChange, Arr(new GParam('T', change.ProgramTool)), None))
            .Add(new GNode.Word(GCommand.LengthOffset, Arr(
                new GParam('H', change.ProgramTool), new GParam('Z', change.LengthOffset)), None));

    private static Seq<GNode> CoolingNodes(CoolingPolicy cooling) =>
        cooling.Word().Match(
            Some: word => Seq<GNode>(new GNode.Word(word, Arr<GParam>(), None)),
            None: () => Seq<GNode>());

    private static Seq<GNode> SpindleNodes(Option<CuttingData> cutting, double toolDiameter) =>
        cutting.Map(data => (GNode)new GNode.Word(
            GCommand.Spindle,
            Arr(new GParam('S', data.SurfaceSpeed * 1_000.0 / (Math.PI * toolDiameter))),
            None)).ToSeq();

    private static Fin<CutProgram> OptimizeProgram(CutProgram program, PostPolicy policy) =>
        policy.Passes.IsEmpty
            ? Fin.Succ(program)
            : Optimize.Feeds(program, new OptimizePolicy(policy.Passes, policy.Mrr, policy.Smooth, policy.Compact, policy));

    private static Fin<Loop> Condition(Loop profile, PostPolicy policy) =>
        !profile.Closed
            ? Fin.Fail<Loop>(FabricationFault.OpenLoop(FabConcern.Program, 0).ToError())
            : from loops in ArcAlgebra.KerfArc(
                    Seq(profile), policy.KerfWidth, profile.Winding() == Sign.Negative ? KerfSide.Inside : KerfSide.Outside)
              from loop in loops.Head.ToFin(FabricationFault.KerfCollision(new KerfWitness.Vanished(0), policy.KerfWidth).ToError())
              from compensated in Compensate(loop, policy)
              select compensated;

    private static Fin<Seq<GNode>> CutPath(Loop loop, PostPolicy policy) =>
        from pierce in ArcAlgebra.SampleAtLength(loop, 0.0)
        from lead in policy.Lead.Enter(loop, policy)
        from body in Walk(loop, policy)
        select BuildPath(pierce, lead, body, policy);

    private static Seq<GNode> BuildPath(Point3d pierce, Seq<Move> lead, Seq<GNode> body, PostPolicy policy) {
        Point3d start = lead.HeadOrNone().Map(GNode.Target).IfNone(pierce);
        Seq<GNode> entry = lead.IsEmpty ? Seq<GNode>() : GNode.Moves(lead.Tail, start);
        return Seq<GNode>(new GNode.Word(GCommand.Rapid, Arr(new GParam('X', start.X), new GParam('Y', start.Y)), None))
            .Concat(PierceBlock(policy))
            .Concat(entry)
            .Concat(body);
    }

    internal static Fin<Seq<Move>> ArcLead(Loop loop, PostPolicy policy) =>
        from admitted in LeadPolicy.Admit(0.0, policy.LeadRadius, policy.FeedCeiling * policy.Feed.EdgeFloor, LeadKind.In)
        from lead in ArcAlgebra.LeadArc(loop, admitted)
        select lead;

    internal static Fin<Seq<Move>> LineLead(Loop loop, PostPolicy policy) =>
        from length in ArcAlgebra.ArcLength(loop)
        from pierce in ArcAlgebra.SampleAtLength(loop, 0.0)
        from after in ArcAlgebra.SampleAtLength(loop, Math.Min(length, Math.Max(policy.LeadRadius, loop.Tolerance.Absolute.Value)))
        let tangent = after - pierce
        from _ in policy.LeadRadius > 0.0 && tangent.Unitize()
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(GeometryFault.DegenerateInput("post:line-lead").ToError())
        let start = pierce - tangent * policy.LeadRadius
        select Seq(
            (Move)new Move.Rapid(start),
            new Move.Linear(pierce, policy.FeedCeiling * policy.Feed.EdgeFloor));

    private static Seq<GNode> PierceBlock(PostPolicy policy) =>
        Seq<GNode>(
            new GNode.Word(GCommand.AssistGas, Arr(new GParam('S', policy.AssistPressure)), None),
            new GNode.Word(GCommand.TorchOn, Arr<GParam>(), None))
        + (policy.PierceTime > 0.0
            ? Seq<GNode>(new GNode.CannedCycle(GCommand.Pierce, Arr(new GParam('P', policy.PierceTime)), Seq<Move>(), Repeats: 1, Mode: None))
            : Seq<GNode>());

    // Bevel DECIDES, posting RENDERS: each ThcDirective span lowers to one torch-height word.
    private static GNode Thc(ThcDirective directive) =>
        directive.Switch(
            track:      static t => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('V', t.ArcVoltage)), None),
            capacitive: static c => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('H', c.HeightMm)), None),
            plateRide:  static p => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('R', p.HeightMm)), None),
            hold:       static _ => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('P', 1.0)), None),
            off:        static _ => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('P', 0.0)), None));

    // Each BevelPass lowers as ONE coupled section: block 0 is the pass's pierce station — its target anchors
    // the entry rapid and the pass's own PierceDelaySeconds gates the cycle — then the remaining Blocks thread
    // the cursor with the pass's OWN ThcSpan windows interleaved at their block boundaries. Pass identity,
    // tool-axis blocks, pierce, feed, and THC evidence stay aligned per pass; no span stream detaches.
    private static Seq<GNode> BevelSections(Seq<BevelPass> passes) =>
        passes.Bind(pass => {
            Point3d entry = pass.Blocks.HeadOrNone().Map(static block => GNode.Target(block.Move)).IfNone(Point3d.Origin);
            Seq<GNode> pierce = Seq<GNode>(
                new GNode.Word(GCommand.Rapid, Arr(new GParam('X', entry.X), new GParam('Y', entry.Y)), None),
                new GNode.Word(GCommand.TorchOn, Arr<GParam>(), None))
                .Concat(pass.PierceDelaySeconds > 0.0
                    ? Seq<GNode>(new GNode.CannedCycle(GCommand.Pierce, Arr(new GParam('P', pass.PierceDelaySeconds)), Seq<Move>(), Repeats: 1, Mode: None))
                    : Seq<GNode>())
                .Concat(pass.Thc.Filter(static span => span.FromInclusive == 0).Map(span => Thc(span.Directive)));
            (Seq<GNode> Nodes, Point3d Cursor) walked = toSeq(Enumerable.Range(1, Math.Max(0, pass.Blocks.Count - 1))).Fold(
                (Nodes: Seq<GNode>(), Cursor: entry),
                (acc, index) => (acc.Nodes
                        .Concat(pass.Thc.Filter(span => span.FromInclusive == index).Map(span => Thc(span.Directive)))
                        .Add(GNode.Move(pass.Blocks[index].Move, acc.Cursor)),
                    GNode.Target(pass.Blocks[index].Move)));
            Seq<GNode> closed = pass.Thc.Exists(span => span.ToExclusive >= pass.Blocks.Count && span.Directive is not ThcDirective.Off)
                ? Seq<GNode>(Thc(new ThcDirective.Off()))
                : Seq<GNode>();
            return pierce.Concat(walked.Nodes).Concat(closed);
        });

    // ONE walk per loop — vertex stations and generated tab boundaries partition arc length before emission.
    // The fold preserves bulge sub-arcs and straight-run recovery while every tab bridges its exact width;
    // torch-height words never emit here — THC rides its owning BevelPass section, never a per-loop policy bag.
    private static Fin<Seq<GNode>> Walk(Loop loop, PostPolicy policy) =>
        Segments(loop, policy).Map(segments => {
            (Seq<GNode> Out, Seq<Point3d> Run) state = segments.Fold((Seq<GNode>(), Seq(loop.At(0))), (acc, segment) => {
                if (segment.Tab)
                    return (acc.Out.Concat(FlushRun(acc.Run, policy)).Concat(Bridge(segment.To, policy)), Seq(segment.To));
                if (Math.Abs(segment.Bulge) <= 1e-12)
                    return (acc.Out, acc.Run.Add(segment.To));
                return (acc.Out.Concat(FlushRun(acc.Run, policy)).Add(
                    BulgeArc(segment.From, segment.To, segment.Bulge, Feedrate(loop, segment.Span, policy))), Seq(segment.To));
            });
            return state.Out.Concat(FlushRun(state.Run, policy));
        });

    private static Fin<Seq<(int Span, Point3d From, Point3d To, double Bulge, bool Tab)>> Segments(Loop loop, PostPolicy policy) {
        Seq<double> lengths = toSeq(Enumerable.Range(0, loop.Count)).Map(index => SpanLength(loop, index));
        Seq<double> vertices = lengths.Fold(Seq(0.0), static (stations, length) => stations.Add(stations.Last + length));
        double total = vertices.Last;
        int count = policy.TabSpacing <= 0.0 ? 0 : (int)Math.Floor(total / policy.TabSpacing);
        Seq<(double Start, double End)> tabs = toSeq(Enumerable.Range(0, count))
            .Map(index => policy.TabSpacing * (index + 0.5))
            .Map(center => (Start: center - policy.TabWidth / 2.0, End: center + policy.TabWidth / 2.0))
            .Filter(tab => tab.Start > loop.Tolerance.Absolute.Value && tab.End < total - loop.Tolerance.Absolute.Value);
        Seq<double> boundaries = vertices.Concat(tabs.Bind(static tab => Seq(tab.Start, tab.End)))
            .Distinct().OrderBy(static station => station).ToSeq();
        return toSeq(Enumerable.Range(0, boundaries.Count - 1)).Map(index => {
            double fromStation = boundaries[index];
            double toStation = boundaries[index + 1];
            double midpoint = (fromStation + toStation) / 2.0;
            int span = toSeq(Enumerable.Range(0, loop.Count))
                .Find(i => midpoint >= vertices[i] && midpoint <= vertices[i + 1])
                .IfNone(loop.Count - 1);
            double sourceBulge = loop.Bulges.IsEmpty ? 0.0 : loop.Bulges[span % loop.Bulges.Count];
            double fraction = (toStation - fromStation) / lengths[span];
            double bulge = Math.Abs(sourceBulge) <= 1e-12
                ? 0.0
                : Math.Sign(sourceBulge) * Math.Tan(Math.Atan(Math.Abs(sourceBulge)) * fraction);
            bool tab = tabs.Exists(window => midpoint > window.Start && midpoint < window.End);
            return from start in ArcAlgebra.SampleAtLength(loop, fromStation)
                   from end in ArcAlgebra.SampleAtLength(loop, toStation)
                   select (Span: span, From: start, To: end, Bulge: bulge, Tab: tab);
        }).TraverseM(identity).As();
    }

    private static double SpanLength(Loop loop, int index) {
        double chord = loop.At(index).DistanceTo(loop.At(index + 1));
        double bulge = loop.Bulges.IsEmpty ? 0.0 : loop.Bulges[index % loop.Bulges.Count];
        if (Math.Abs(bulge) <= 1e-12) return chord;
        double sweep = 4.0 * Math.Atan(Math.Abs(bulge));
        double radius = chord * (1.0 + bulge * bulge) / (4.0 * Math.Abs(bulge));
        return radius * sweep;
    }

    private static Seq<GNode> Bridge(Point3d to, PostPolicy policy) =>
        Seq<GNode>(new GNode.Word(GCommand.SpindleStop, Arr<GParam>(), None),
                   new GNode.Word(GCommand.Rapid, Arr(new GParam('X', to.X), new GParam('Y', to.Y)), None))
            .Concat(PierceBlock(policy));

    // Bulge geometry (bulge = tan(theta/4)): radius r = c(1+b^2)/4|b|, sagitta s = |b|c/2, and the center
    // sits at mid - leftNormal * sign(b) * (r - s) — the arc-native span never refits through g3.
    private static GNode BulgeArc(Point3d a, Point3d b, double bulge, double feed) {
        double chord = a.DistanceTo(b);
        double radius = chord * (1.0 + bulge * bulge) / (4.0 * Math.Abs(bulge));
        double sagitta = Math.Abs(bulge) * chord / 2.0;
        Vector3d left = new(-(b.Y - a.Y), b.X - a.X, 0.0);
        left.Unitize();
        Point3d mid = new(0.5 * (a.X + b.X), 0.5 * (a.Y + b.Y), a.Z);
        Point3d center = mid - left * (Math.Sign(bulge) * (radius - sagitta));
        return new GNode.Word(bulge > 0.0 ? GCommand.ArcCcw : GCommand.ArcCw,
            Arr(new GParam('X', b.X), new GParam('Y', b.Y), new GParam('I', center.X - a.X), new GParam('J', center.Y - a.Y), new GParam('F', feed)), None);
    }

    // Straight-run recovery: below MinRunLength the run stays line words; at/above, consecutive point
    // triples fit through BiArcFit2 stepping TWO spans per fit so no span emits twice; I/J read
    // Center - SampleT(0.0) per the admitted contract, and a zero tangent routes straight before fitting.
    private static Seq<GNode> FlushRun(Seq<Point3d> run, PostPolicy policy) {
        if (run.Count < 2) return Seq<GNode>();
        double total = toSeq(Enumerable.Range(0, run.Count - 1)).Map(i => run[i].DistanceTo(run[i + 1])).Sum();
        if (run.Count < 3 || total < policy.Biarc.MinRunLength)
            return toSeq(Enumerable.Range(1, run.Count - 1)).Map(i =>
                (GNode)new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i].X), new GParam('Y', run[i].Y), new GParam('F', policy.FeedCeiling)), None));
        Seq<GNode> outp = Seq<GNode>();
        for (int i = 0; i + 2 < run.Count; i += 2) {
            Vector2d ta = new(run[i + 1].X - run[i].X, run[i + 1].Y - run[i].Y);
            Vector2d tb = new(run[i + 2].X - run[i + 1].X, run[i + 2].Y - run[i + 1].Y);
            if (ta.Length < 1e-9 || tb.Length < 1e-9) {
                outp = outp.Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i + 2].X), new GParam('Y', run[i + 2].Y), new GParam('F', policy.FeedCeiling)), None));
                continue;
            }
            BiArcFit2 fit = new(new Vector2d(run[i].X, run[i].Y), ta.Normalized, new Vector2d(run[i + 2].X, run[i + 2].Y), tb.Normalized);
            outp = fit.Distance(new Vector2d(run[i + 1].X, run[i + 1].Y)) > policy.Biarc.FitTolerance
                ? outp.Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i + 1].X), new GParam('Y', run[i + 1].Y), new GParam('F', policy.FeedCeiling)), None))
                      .Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i + 2].X), new GParam('Y', run[i + 2].Y), new GParam('F', policy.FeedCeiling)), None))
                : outp.Add(ArcNode(fit.Arc1, fit.Arc1IsSegment, fit.Segment1.P1, policy))
                      .Add(ArcNode(fit.Arc2, fit.Arc2IsSegment, fit.Segment2.P1, policy));
        }
        if ((run.Count - 1) % 2 == 1)
            outp = outp.Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[run.Count - 1].X), new GParam('Y', run[run.Count - 1].Y), new GParam('F', policy.FeedCeiling)), None));
        return outp;
    }

    private static GNode ArcNode(Arc2d arc, bool isSegment, Vector2d segmentEnd, PostPolicy policy) {
        if (isSegment)
            return new GNode.Word(GCommand.Feed, Arr(new GParam('X', segmentEnd.x), new GParam('Y', segmentEnd.y), new GParam('F', policy.FeedCeiling)), None);
        Vector2d start = arc.SampleT(0.0);
        Vector2d end = arc.SampleT(1.0);
        return new GNode.Word(arc.IsReversed ? GCommand.ArcCw : GCommand.ArcCcw,
            Arr(new GParam('X', end.x), new GParam('Y', end.y),
                new GParam('I', arc.Center.x - start.x), new GParam('J', arc.Center.y - start.y), new GParam('F', policy.FeedCeiling)), None);
    }

    private static Fin<Loop> Compensate(Loop loop, PostPolicy policy) =>
        !policy.Comp.Mechanical
            ? OffsetCompensation(loop, policy.Comp.ThermalGrowth, policy.KerfWidth)
            : policy.Cutting.Match(
                Some: cutting => cutting.FeedBasis == FeedBasis.PerTooth
                    ? cutting.Force(policy.Comp.CutWidthMm, cutting.Feed)
                        .Bind(force => OffsetCompensation(loop, policy.Comp.Deflection(force) + policy.Comp.ThermalGrowth, policy.KerfWidth))
                    : Fin.Fail<Loop>(GeometryFault.DegenerateInput($"post:compensation-feed-basis:{cutting.FeedBasis.Key}").ToError()),
                None: () => OffsetCompensation(loop, policy.Comp.ThermalGrowth, policy.KerfWidth));

    private static Fin<Loop> OffsetCompensation(Loop loop, double delta, double kerf) =>
        Math.Abs(delta) <= 1e-9
            ? Fin.Succ(loop)
            : ArcAlgebra.ArcOffset(loop, loop.Winding() == Sign.Negative ? -delta : delta)
                .Bind(loops => loops.Head.ToFin(FabricationFault.KerfCollision(new KerfWitness.Vanished(0), Math.Max(Math.Abs(delta), kerf)).ToError()));

    internal static Seq<GNode> Lookahead(Seq<GNode> nodes, PostPolicy policy) {
        GNode[] b = nodes.ToArray();
        double[] caps = Enumerable.Repeat(double.PositiveInfinity, b.Length).ToArray();
        double[] distances = new double[b.Length];
        bool[] cutting = new bool[b.Length];
        bool[] motion = new bool[b.Length];
        Point3d[] at = new Point3d[b.Length];
        Point3d cursor = Point3d.Origin;
        for (int i = 0; i < b.Length; i++) {
            at[i] = cursor;
            if (b[i] is not GNode.Word word || !MotionWord(word)) continue;
            Point3d target = new(
                word.P('X').IfNone(cursor.X), word.P('Y').IfNone(cursor.Y), word.P('Z').IfNone(cursor.Z));
            motion[i] = true;
            at[i] = target;
            distances[i] = cursor.DistanceTo(target);
            cutting[i] = Cutting(word) && word.P('F').IsSome && distances[i] > 1e-9;
            cursor = target;
        }
        for (int i = 0; i < b.Length; i++)
            if (cutting[i] && b[i] is GNode.Word word)
                caps[i] = Math.Min(word.P('F').IfNone(policy.FeedCeiling), JunctionFeed(at, motion, i, policy.Dynamics));
        double entering = 0.0;
        for (int i = 0; i < b.Length; i++) {
            if (!cutting[i]) {
                if (b[i] is GNode.Word word && MotionWord(word)) entering = 0.0;
                continue;
            }
            caps[i] = Math.Min(caps[i], Reachable(entering, distances[i], policy.Dynamics));
            entering = caps[i] / 60.0;
        }
        double leaving = 0.0;
        for (int i = b.Length - 1; i >= 0; i--) {
            if (!cutting[i]) {
                if (b[i] is GNode.Word word && MotionWord(word)) leaving = 0.0;
                continue;
            }
            caps[i] = Math.Min(caps[i], Reachable(leaving, distances[i], policy.Dynamics));
            leaving = caps[i] / 60.0;
        }
        return toSeq(Enumerable.Range(0, b.Length)).Map(i =>
            b[i] is GNode.Word w && cutting[i]
                ? (GNode)w.With('F', caps[i])
                : b[i]);
    }

    private static double Reachable(double entryMmPerSecond, double distanceMm, MotionDynamics dynamics) {
        double acceleration = Math.Sqrt(entryMmPerSecond * entryMmPerSecond + 2.0 * dynamics.Acceleration * distanceMm);
        double jerk = entryMmPerSecond + Math.Cbrt(6.0 * dynamics.Jerk * distanceMm * distanceMm);
        return Math.Min(acceleration, jerk) * 60.0;
    }

    private static bool MotionWord(GNode.Word word) => word.Command.Group == ModalGroup.Motion;

    // The ONE motion-dynamics law COMPOSED: MotionDynamics.JunctionFeed (Kinematics/machine) is the
    // junction-deviation cap, evaluated over the Dynamics.LookaheadBlocks window of CURSOR-THREADED
    // positions — an absent Y or Z word inherits the cursor, never a zero default that corrupts the turn.
    private static double JunctionFeed(Point3d[] at, bool[] motion, int i, MotionDynamics dynamics) {
        Seq<Point3d> ahead = toSeq(Enumerable.Range(i, dynamics.LookaheadBlocks + 1))
            .Filter(k => k < at.Length && motion[k])
            .Map(k => at[k]);
        double turn = ahead.Count < 3 ? 0.0 : toSeq(Enumerable.Range(1, ahead.Count - 2)).Map(j => {
            Vector3d incoming = ahead[j] - ahead[j - 1];
            Vector3d outgoing = ahead[j + 1] - ahead[j];
            incoming.Unitize();
            outgoing.Unitize();
            return Vector3d.VectorAngle(incoming, outgoing);
        }).Max();
        return turn <= 1e-9
            ? dynamics.CuttingFeed
            : Math.Min(dynamics.CuttingFeed, dynamics.JunctionFeed(turn));
    }

    private static double Feedrate(Loop loop, int i, PostPolicy policy) {
        Vector3d incoming = loop.At(i) - loop.At(i - 1);
        Vector3d outgoing = loop.At(i + 1) - loop.At(i);
        double edgeDistance = Math.Min(incoming.Length, outgoing.Length);
        incoming.Unitize();
        outgoing.Unitize();
        double turn = Vector3d.VectorAngle(incoming, outgoing);
        double corner = Math.Min(policy.FeedCeiling, policy.Dynamics.JunctionFeed(turn));
        double edge = edgeDistance < policy.Feed.EdgeDistance ? policy.FeedCeiling * policy.Feed.EdgeFloor : policy.FeedCeiling;
        return Math.Min(corner, edge);
    }

    // WCS lowers as the Wcs ROW carrying slot ordinal + family flag — the dialect roster renders the code;
    // tool sections own coolant restoration; a G90-P WCS fiction is the deleted form.
    private static Seq<GNode> Prologue(SetupSchedule schedule) =>
        schedule.Wcs.Bind(wcs => WcsBlocks(wcs, schedule.Setups[wcs.Setup]));

    private static Seq<GNode> WcsBlocks(WcsAssignment assignment, Setup setup) =>
        Seq<GNode>(
            new GNode.Word(GCommand.SetWcs, Arr(
                new GParam('L', 2.0), new GParam('P', WcsOrdinal(assignment.Slot)),
                new GParam('X', setup.Frame.OriginX), new GParam('Y', setup.Frame.OriginY), new GParam('Z', setup.Frame.OriginZ)), None),
            new GNode.Word(GCommand.Wcs, Arr(
                new GParam('P', WcsOrdinal(assignment.Slot)),
                new GParam('E', assignment.Slot.Family == WcsFamily.Extended ? 1.0 : 0.0)), None));

    private static int WcsOrdinal(WcsSlot slot) => slot.Family == WcsFamily.Base ? slot.Ordinal + 1 : slot.Ordinal;

    // Ordered segment parse: each address run attaches to the command token it follows, so two
    // parameter-owning commands in one block keep their own words; leading params bind the first command.
    private static Fin<Seq<GNode.Word>> ParseBlock(int line, string text, PostDialect dialect) {
        (Seq<(string Command, Seq<string> Params)> Segments, Seq<string> Leading) split = Tokens(text).Fold(
            (Segments: Seq<(string Command, Seq<string> Params)>(), Leading: Seq<string>()),
            (acc, token) => Commands.Exists(c => string.Equals(c.Code, token, StringComparison.OrdinalIgnoreCase))
                ? (acc.Segments.Add((token, Seq<string>())), acc.Leading)
                : acc.Segments.IsEmpty
                    ? (acc.Segments, acc.Leading.Add(token))
                    : (acc.Segments.Init.Add((acc.Segments.Last.Command, acc.Segments.Last.Params.Add(token))), acc.Leading));
        return split.Segments.IsEmpty
            ? Fin.Fail<Seq<GNode.Word>>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError())
            : Seq((split.Segments.Head.Command, split.Leading.Concat(split.Segments.Head.Params))).Concat(split.Segments.Tail)
                .TraverseM(segment =>
                    from parameters in segment.Item2.Map(t => ParseParam(line, t)).TraverseM(identity).As()
                    from command in Command(line, segment.Item1, parameters.ToArr(), dialect)
                    select new GNode.Word(command, parameters.ToArr(), None))
                .As();
    }

    // Emission aliases legitimately SHARE wire codes (a pierce IS a G4 dwell on a torch dialect, an
    // extrusion IS a G1 move carrying an E word) — the lookup disambiguates a shared code by dialect and
    // parameter shape rather than inventing nonstandard codes.
    private static Fin<GCommand> Command(int line, string code, Arr<GParam> parameters, PostDialect dialect) =>
        Commands.Filter(command => string.Equals(command.Code, code, StringComparison.OrdinalIgnoreCase)) switch {
            { Count: 1 } lone => Fin.Succ(lone.Head),
            Seq<GCommand> shared => shared.Find(command =>
                    (command == GCommand.Extrude && parameters.Exists(static p => p.Address == 'E'))
                    || (command == GCommand.Feed && !parameters.Exists(static p => p.Address == 'E'))
                    || (command == GCommand.Pierce && dialect == PostDialect.Hypertherm)
                    || (command == GCommand.Dwell && dialect != PostDialect.Hypertherm))
                .ToFin(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError()),
        };

    private static Fin<GParam> ParseParam(int line, string token) =>
        token.Length < 2 || char.ToUpperInvariant(token[0]) is 'G' or 'M'
            || !double.TryParse(token[1..], NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? Fin.Fail<GParam>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError())
            : Fin.Succ(new GParam(char.ToUpperInvariant(token[0]), value));

    [GeneratedRegex(@"\([^)]*\)|;.*$")]
    private static partial Regex CommentText { get; }

    [GeneratedRegex(@"[A-Za-z]+[+-]?(?:\d+(?:\.\d*)?|\.\d+)")]
    private static partial Regex WordText { get; }

    private static Arr<string> Tokens(string text) =>
        WordText.Matches(CommentText.Replace(text, string.Empty))
            .Select(static match => match.Value)
            .ToArr();

    // Foreign-file framing: `%` tape delimiters and bare O-number program headers are structure, not words —
    // stripped here so a real Fanuc/Haas file parses instead of failing on its first line.
    private static Seq<(int Line, string Text)> Lines(string source) =>
        source.Split('\n')
            .Select((text, index) => (Line: index + 1, Text: text.Trim()))
            .Where(static row => row.Text.Length > 0 && row.Text != "%"
                && !(char.ToUpperInvariant(row.Text[0]) == 'O' && row.Text.Length > 1 && row.Text.Skip(1).All(char.IsDigit)))
            .ToSeq();

    private static Seq<string> Render(Seq<GWord> words) =>
        RenderRows(words, Map<ModalGroup, string>()).Lines.Concat(words.Bind(Definitions));

    private static (Seq<string> Lines, Map<ModalGroup, string> Active) RenderRows(
        Seq<GWord> words, Map<ModalGroup, string> active) =>
        words.Fold((Lines: Seq<string>(), Active: active), (state, word) => {
            (Seq<string> Lines, Map<ModalGroup, string> Active) next = RenderWord(word, state.Active);
            return (state.Lines.Concat(next.Lines), next.Active);
        });

    private static (Seq<string> Lines, Map<ModalGroup, string> Active) RenderWord(
        GWord word, Map<ModalGroup, string> active) => word.Switch(
        state: active,
        address: static (state, w) => {
            bool emitMode = w.Mode.Exists(mode => w.Retention == WordRetention.Explicit
                || !state.Find(ModalGroup.Feed).Exists(code => code == mode.Code));
            Map<ModalGroup, string> withMode = w.Mode.Match(
                Some: mode => state.AddOrUpdate(ModalGroup.Feed, mode.Code),
                None: () => state);
            bool emitCode = w.Group == ModalGroup.NonModal || w.Retention == WordRetention.Explicit
                || !withMode.Find(w.Group).Exists(code => code == w.Code);
            string modeText = emitMode ? w.Mode.Map(static mode => $"{mode.Code} ").IfNone(string.Empty) : string.Empty;
            string codeText = emitCode ? $"{w.Code} " : string.Empty;
            string line = $"{modeText}{codeText}{string.Join(" ", w.Words.Map(p => $"{p.Address}{Number(p.Value)}").ToArray())}".Trim();
            Map<ModalGroup, string> next = w.Group == ModalGroup.NonModal ? withMode : withMode.AddOrUpdate(w.Group, w.Code);
            return (string.IsNullOrEmpty(line) ? Seq<string>() : Seq(line), next);
        },
        conversational: static (_, w) => (Seq(($"{w.Verb} " + string.Join(" ", w.Words.Map(p => p.Address == 'F'
            ? $"F{Number(Math.Round(p.Value, w.Decimals))}"
            : $"{p.Address}{(p.Value >= 0 ? "+" : "")}{Number(Math.Round(p.Value, w.Decimals))}").ToArray())
            + (w.Tail.Length == 0 ? string.Empty : $" {w.Tail}")).Trim()), Map<ModalGroup, string>()),
        text: static (_, w) => (w.Records, Map<ModalGroup, string>()),
        // Grammar-true dialect-cycle rendering keyed on the retained dialect: Sinumerik cycles are parameterized
        // subroutine calls (MCALL for repetition), Klartext cycles are CYCL DEF blocks closed by CYCL CALL, and
        // every word-address dialect-cycle row keeps the word form with L-repeats.
        cycleCall: static (_, w) => (w.Dialect switch {
            "siemens-840d" => Seq((w.Repeats > 1 ? "MCALL " : string.Empty)
                + $"{w.Code}({string.Join(", ", w.Words.Map(p => Number(p.Value)).ToArray())})"),
            "heidenhain-tnc" => Seq($"CYCL DEF {w.Code}")
                .Concat(w.Words.Map(p => $"{p.Address}{Number(p.Value)}"))
                .Add(w.Repeats > 1 ? $"CYCL CALL REP{w.Repeats}" : "CYCL CALL"),
            _ => Seq(($"{w.Code} {string.Join(" ", w.Words.Map(p => $"{p.Address}{Number(p.Value)}").ToArray())}"
                + (w.Repeats > 1 ? $" L{w.Repeats}" : string.Empty)).Trim()),
        }, Map<ModalGroup, string>()),
        fault: static (state, w) => (Seq(w.Error.Message), state),
        macro: static (state, w) => {
            // Slot assignments render one per line with the grammar-owned `=` form (#n= / Rn= / Qn= / Vn=);
            // MacroGrammar.None admits no assignments — a slot-bearing macro on a grammarless dialect is upstream's fault to reject.
            (Seq<string> Lines, Map<ModalGroup, string> Active) body = RenderRows(w.Body, state);
            Seq<string> assignments = w.Grammar == MacroGrammar.None
                ? Seq<string>()
                : w.Slots.Map(s => $"{s.Key}={Number(s.Value)}").ToSeq();
            return (assignments.Concat(body.Lines), body.Active);
        },
        subprogram: static (_, w) => (w.Grammar == SubprogramGrammar.M98
            ? Seq($"M98 P{w.Label} L{w.Repeats}")
            : Seq($"CALL LBL {w.Label} REP {w.Repeats}"), Map<ModalGroup, string>()),
        additive: static (_, w) => (Seq(
            $";LAYER:{w.Layer}",
            $"M104 S{Number(w.Temperatures.Hotend)}",
            $"M140 S{Number(w.Temperatures.Bed)}",
            $"G1 E{Number(w.Extrusion.Amount)} F{Number(w.Extrusion.Feed)}"), Map<ModalGroup, string>()),
        nc1: static (_, w) => (w.Records, Map<ModalGroup, string>()),
        expanded: static (state, w) => RenderRows(w.Words, state));

    private static Seq<string> Definitions(GWord word) => word.Switch(
        address: static _ => Seq<string>(),
        conversational: static _ => Seq<string>(),
        text: static _ => Seq<string>(),
        cycleCall: static _ => Seq<string>(),
        macro: w => w.Body.Bind(Definitions),
        subprogram: w => (w.Grammar == SubprogramGrammar.M98
            ? Seq($"O{w.Label}").Concat(RenderRows(w.Body, Map<ModalGroup, string>()).Lines).Add("M99")
            : Seq($"LBL {w.Label}").Concat(RenderRows(w.Body, Map<ModalGroup, string>()).Lines).Add("LBL 0"))
            .Concat(w.Body.Bind(Definitions)),
        additive: static _ => Seq<string>(),
        nc1: static _ => Seq<string>(),
        fault: static _ => Seq<string>(),
        expanded: w => w.Words.Bind(Definitions));

    private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static bool Cutting(GNode.Word w) =>
        w.Command == GCommand.Feed || w.Command == GCommand.ArcCw || w.Command == GCommand.ArcCcw || w.Command == GCommand.Extrude;
}
```
