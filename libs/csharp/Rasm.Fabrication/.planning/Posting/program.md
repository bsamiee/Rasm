# [RASM_FABRICATION_PROGRAM]

`Post` owns the dialect-neutral cut-program AST, the `Run(Post{Motion, PostDialect})` lowering body, the live placement cut-conditioning rail, the RS274 word-parse ingress, and the `PackKind.Toolpath` publish-back egress. `GNode` is the sole program vocabulary consumed by `Dialect.Emit`; `CutProgram` carries the AST plus its `ContentKey` minted over the full canonical node payload, and `PostedProgram` carries lowered block text plus the `EgressKind.CutProgram` content key. Motion enters already CAM-conditioned; placement-style 2D cutting enters the local cut-conditioning rail exactly once through the `Lower(FabricationResult.Placement, ...)` overload — one `Lower` name, the input type the modality discriminant: arc-native kerf and lead compose `ArcAlgebra`, contour order composes `PolygonAlgebra.NestingOrder`, WCS rows compose `Setup.Schedule` and lower through the `GCommand.Wcs` row (never a `G90 P` fiction), tool swaps compose `Magazine.Schedule`, torch-height spans render the `Toolpath/bevel` `ThcDirective` union, and line-run recovery alone composes `g3.BiArcFit2` — a bulged span emits its arc from its own `Bulges` column and never refits.

## [01]-[INDEX]

- [01]-[CUT_PROGRAM]: owns `GCommand`, `GNodeKind`, `ModalGroup`, `CoolingPolicy`, `GNode`, `GWord`, `CutProgram`, `PostPolicy`, the `Run(Post)` lowering body, placement cut-conditioning, line-run biarc recovery, `Post.Parse`, and `Post.Publish`.

## [02]-[CUT_PROGRAM]

- Owner: `GNode` is the neutral AST consumed by `Dialect.Emit`; `GWord` is the lowered word evidence; `CutProgram` stores nodes, dialect, and the AST content key over the complete canonical node bytes; `Post` owns `Lower` (both modalities), `Assemble`, `Parse`, `Publish`, `Lookahead`, and the placement cut-conditioning rail.
- Cases: `GNodeKind` rows `word` · `canned-cycle` · `macro` · `subprogram` · `additive-layer` · `nc1`; `ModalGroup` rows 12 — `motion` · `plane` · `distance` · `units` · `feed` · `spindle` · `coolant` · `cutter-comp` · `tool-length` · `wcs` · `stop` · `non-modal` — every `GCommand` row binds its TRUE RS274 group, so a same-block group conflict is constructable and the parse law is live; `LeadStyle` rows `none` · `line` · `arc`; `FeedMode` rows `units-per-minute` · `inverse-time`; `CoolingPolicy` rows `off` · `mist` · `flood`, each binding its coolant command; `GCommand` rows 43 spanning motion, plane, distance, units, feed-mode, spindle/torch energy, coolant, cutter-comp, tool-length, WCS, stop, and non-modal words.
- Entry: `Run(Post{FabricationResult.Motion, PostDialect})` calls the frozen `Post.Lower(FabricationResult.Motion, PostDialect, FabricationInput, PostPolicy?)` arm; `Post.Lower(FabricationResult.Placement, ...)` is the placement overload — the same name discriminating on input shape, the public entry the former dead rail lacked; `public static Fin<CutProgram> Post.Parse(string source, PostDialect dialect)` is the ingress round-trip arm; `public static Fin<EncodedGeometry> Post.Publish(FabricationResult.Motion motion, PackPolicy policy)` realizes the branch `Posting →[WIRE]: PackKind → Rasm` edge through kernel `Encode.Apply(PackOp.Toolpath(VectorCloud.PolylineCase, PackPolicy))`.
- Auto: `Lower` assembles the source, inserts WCS, tool-change, and coolant prologue rows, lowers nodes through `Dialect.Emit`, seals faults and the block cap through `Dialect.Seal` (an unsupported pair FAILS the rail typed — it never renders as text), renders blocks, and mints `ContentKey.Of(EgressKind.CutProgram, blockBytes)`. Placement alone runs `Kerf`/`Lead`/`Pierce`/`Thc`/`Tab`/`NestingOrder`; `Motion` never receives a kerf pass. The conditioning walk is ONE pass per loop: a bulged span emits its arc word from the `Bulges` column, a straight run at or above `Biarc.MinRunLength` recovers arcs through `BiArcFit2` (I/J relative to the arc START per `Center - SampleT(0.0)`), a tab window bridges with energy-off → rapid → re-pierce, never a bare rapid with the torch live. `Parse` splits each line into its words, admits multiple commands per block, routes a same-block modal-group conflict through `ProgramParse(line, group)`, and fails a malformed parameter token typed instead of dropping it.
- Receipt: `CutProgram.Key` identifies the AST payload byte-completely; `FabricationResult.PostedProgram(Seq<string> Blocks, ContentKey Key)` identifies the lowered machine artifact; `ProgramParse` and `BlockCapExceeded` carry typed failure evidence.
- Packages: `Geometry2D/arcs#ARC_ALGEBRA` (`KerfArc`, `LeadArc`, `SampleAtLength`, `ArcOffset`); `Geometry2D/algebra#POLYGON_ALGEBRA` (`NestingOrder`); `Tooling/magazine#TOOL_MAGAZINE` (`Schedule`, `ToolChange`); `Fixturing/setups#SETUP_SCHEDULER` (`Setup.Schedule`, `WcsSlot` ordinal + family); `Fixturing/workholding#WORKHOLDING` (`Condition`); `Toolpath/bevel#BEVEL` (`ThcDirective` spans rendered here, decided there); `Posting/dialect#DIALECT_EMIT` (`Dialect.Emit`, `Seal`, `Nc1Canonical`); `Posting/optimization#PROGRAM_OPTIMIZATION` (`CutProgram` AST consumer); `Process/owner#FABRICATION_OWNER` (`Move`, `PartTransform`, `EgressKind`, `ContentKey`); `Process/faults#FAULT_BAND` (`ProgramParse`, `BlockCapExceeded`); `Rasm.Numerics` (`GeometryFault`, `Sign`); kernel `Rasm.Spatial`/`Rasm.Drawing` (`VectorCloud.Polyline`, `PackOp.Toolpath`, `Encode.Apply`, `EncodedGeometry`, `PackPolicy` — the publish-back wire); `geometry3Sharp` (`BiArcFit2`, `Arc2d`, `Segment2d`, `Vector2d`) for line-run fitting only.
- Growth: a controller grammar change lands in `PostDialect` and `Dialect.Emit`; a program syntax construct lands as one `GNode` case and one lowering arm; a parser modal group lands as one `ModalGroup` row with its `GCommand` rows re-grouped in the same edit; a cut-conditioning concern lands as a `PostPolicy` row or an owned page seam, not a second public post fold; every `GCommand` row binds one `Verify/simulate` `Transitions` entry in the same pass — the roster and the transition table are lockstep.
- Boundary: the former public post fold is dead; the former posting-side contour order is dead; posting renders setup WCS rows and bevel THC spans rather than assigning or deciding them; raw `XxHash128`/`GenerateHash`, `GetHashCode`-derived content identity, raw G-code string synthesis, default WCS selection, a `G90 P` word standing in for a WCS row, a second kerf pass over `Motion`, parse success probes, a silent `Somes()` parameter drop, a g3 refit over arc-native (bulged) spans, a tab bridged with the energy source live, and a plane-internal `CutProgram` result payload are rejected shapes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Kinematics;
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

// The 12-group parse census: every GCommand row binds its TRUE group, so a same-block conflict
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
    public static readonly ModalGroup Stop = new("stop");
    public static readonly ModalGroup NonModal = new("non-modal");
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

// Coolant policy rows carry their emission command; Prologue emits the row's word when not Off.
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
    public static readonly GCommand ProgramEnd = new("program-end", "M30", ModalGroup.Stop);
    public static readonly GCommand Stop = new("stop", "M0", ModalGroup.Stop);
    public static readonly GCommand OptionalStop = new("optional-stop", "M1", ModalGroup.Stop);
    public static readonly GCommand Dwell = new("dwell", "G4", ModalGroup.NonModal);
    public static readonly GCommand Pierce = new("pierce", "G4", ModalGroup.NonModal);
    public static readonly GCommand Probe = new("probe", "G31", ModalGroup.NonModal);
    public static readonly GCommand TorchHeight = new("torch-height", "THC", ModalGroup.NonModal);
    public static readonly GCommand HotendTemp = new("hotend-temp", "M104", ModalGroup.NonModal);
    public static readonly GCommand HotendWait = new("hotend-wait", "M109", ModalGroup.NonModal);
    public static readonly GCommand BedTemp = new("bed-temp", "M140", ModalGroup.NonModal);
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

public readonly record struct FeedPolicy(double CornerFloor, double CornerAngle, double EdgeDistance, double EdgeFloor) {
    public static readonly FeedPolicy Canonical = new(CornerFloor: 0.25, CornerAngle: Math.PI / 4.0, EdgeDistance: 2.0, EdgeFloor: 0.5);
}

public readonly record struct BiarcPolicy(double FitTolerance, double MinRunLength) {
    public static readonly BiarcPolicy Canonical = new(FitTolerance: 0.02, MinRunLength: 1.0);
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
    CuttingData Cutting,
    CompPolicy Comp,
    CoolingPolicy Cooling,
    Seq<ThcDirective> Thc,
    Arr<Loop> Profiles,
    Seq<WorkItem> Work,
    Magazine Magazine,
    SlotMap Slots,
    MagazinePolicy MagazinePolicy,
    Seq<Operation> Operations,
    SchedulePolicy Schedule,
    Fixture Fixture) {
    // The owner#run Post-arm derivation: everything the input carries binds; shop context absent from the
    // input (magazine, work, operations, fixture, THC spans) seeds empty so the arm is TOTAL and prologue
    // rows degrade to absence — a richer PostPolicy threads through Lower's optional parameter, never a
    // second entry.
    public static PostPolicy From(FabricationInput input, PostDialect dialect) => new(
        KerfWidth: 0.0, Lead: LeadStyle.None, LeadRadius: 0.0, TabWidth: 0.0, TabSpacing: 0.0,
        PierceTime: 0.0, AssistPressure: 0.0, MeltTemp: 0.0,
        FeedCeiling: MotionDynamics.Canonical.CuttingFeed, MaxAxes: input.Machine.AxisCount,
        RemovalRate: 0.0, Feed: FeedPolicy.Canonical, Biarc: BiarcPolicy.Canonical,
        Dynamics: MotionDynamics.Canonical, Cutting: new CuttingData(Kc11: 0.0, Mc: 0.0, SurfaceSpeed: 0.0, FeedPerTooth: 0.0, ClassFallback: true),
        Comp: CompPolicy.Disabled, Cooling: CoolingPolicy.Off, Thc: Seq<ThcDirective>(),
        Profiles: input.Profiles, Work: Seq<WorkItem>(), Magazine: Magazine.Manual, Slots: SlotMap.Empty,
        MagazinePolicy: MagazinePolicy.Canonical, Operations: Seq<Operation>(), Schedule: SchedulePolicy.Direct(input.Machine, dialect),
        Fixture: Fixture.Free);

    public Loop Placed(PartTransform t) {
        double ct = Math.Cos(t.RotationRadians);
        double st = Math.Sin(t.RotationRadians);
        return new Loop(Profiles[t.PartId].Vertices.Map(v =>
            new Point3d(v.X * ct - v.Y * st + t.Tx, v.X * st + v.Y * ct + t.Ty, v.Z)).ToArr(), Closed: true, Profiles[t.PartId].Bulges);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramSource {
    private ProgramSource() { }

    public sealed record Motion(FabricationResult.Motion Value) : ProgramSource;
    public sealed record Placement(FabricationResult.Placement Value) : ProgramSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GNode {
    private GNode() { }

    public sealed record Word(GCommand Command, Arr<GParam> Words, Option<FeedMode> Mode = default) : GNode {
        public Option<double> P(char address) => Words.Find(p => p.Address == address).Map(static p => p.Value);
        public Word With(char address, double value) => this with { Words = Words.Filter(p => p.Address != address).Add(new GParam(address, value)) };
        public Word Without(char address) => this with { Words = Words.Filter(p => p.Address != address) };
    }
    public sealed record CannedCycle(GCommand Command, Arr<GParam> SingleBlockWords, Seq<Move> ExpandedMoves, double R, double Q, double P, int Repeats, Option<FeedMode> Mode = default) : GNode;
    public sealed record Macro(GNodeKind Kind, Arr<MacroSlot> Slots, Arr<GNode> Body) : GNode;
    public sealed record Subprogram(GNodeKind Kind, int Label, int Repeats, Arr<GNode> Body) : GNode;
    public sealed record AdditiveLayer(int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures) : GNode;
    public sealed record Nc1(SteelPart Part) : GNode;

    public GNodeKind Kind =>
        Switch(
            word:          static _ => GNodeKind.Word,
            cannedCycle:   static _ => GNodeKind.CannedCycle,
            macro:         static _ => GNodeKind.Macro,
            subprogram:    static _ => GNodeKind.Subprogram,
            additiveLayer: static _ => GNodeKind.AdditiveLayer,
            nc1:           static _ => GNodeKind.Nc1);

    // Arc I/J are relative to the arc START (RS274), so the factory demands the predecessor point; the
    // sequence factory threads the cursor so no call site can hand a wrong start.
    public static GNode Move(Move move, Point3d from) =>
        move.Arc.Match(
            Some: arc => new Word(
                arc.Clockwise ? GCommand.ArcCw : GCommand.ArcCcw,
                Arr(new GParam('X', move.To.X), new GParam('Y', move.To.Y), new GParam('Z', move.To.Z),
                    new GParam('I', arc.Center.X - from.X), new GParam('J', arc.Center.Y - from.Y), new GParam('F', move.Feed))),
            None: () => new Word(
                move.Rapid ? GCommand.Rapid : GCommand.Feed,
                move.Rapid
                    ? Arr(new GParam('X', move.To.X), new GParam('Y', move.To.Y), new GParam('Z', move.To.Z))
                    : Arr(new GParam('X', move.To.X), new GParam('Y', move.To.Y), new GParam('Z', move.To.Z), new GParam('F', move.Feed))));

    public static Seq<GNode> Moves(Seq<Move> moves, Point3d origin) =>
        moves.Fold((Nodes: Seq<GNode>(), Cursor: origin), static (acc, move) =>
            (acc.Nodes.Add(Move(move, acc.Cursor)), move.To)).Nodes;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GWord {
    private GWord() { }

    public sealed record Address(string Code, Arr<GParam> Words, Option<FeedMode> Mode, bool Modal, ArcMode Arc) : GWord;
    public sealed record Conversational(string Verb, Arr<GParam> Words, int Decimals) : GWord;
    public sealed record CycleCall(string Dialect, GNodeKind Kind, double R, double Q, double P, int Repeats) : GWord;
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

    static string NodeKey(GNode node) =>
        node.Switch(
            word:          static w => $"w:{w.Command.Key}:{w.Mode.Map(static m => m.Key).IfNone("")}:{string.Join(",", w.Words.Map(static p => $"{p.Address}={p.Value:R}"))}",
            cannedCycle:   static c => $"c:{c.Command.Key}:{c.R:R}:{c.Q:R}:{c.P:R}:{c.Repeats}:{string.Join(",", c.SingleBlockWords.Map(static p => $"{p.Address}={p.Value:R}"))}:{string.Join("|", c.ExpandedMoves.Map(static m => $"{m.To.X:R},{m.To.Y:R},{m.To.Z:R},{m.Feed:R},{(m.Rapid ? 1 : 0)}"))}",
            macro:         static m => $"m:{m.Kind.Key}:{string.Join(",", m.Slots.Map(static s => $"{s.Index}:{s.Key}={s.Value:R}"))}:[{string.Join(";", m.Body.Map(NodeKey))}]",
            subprogram:    static s => $"s:{s.Label}:{s.Repeats}:[{string.Join(";", s.Body.Map(NodeKey))}]",
            additiveLayer: static a => $"a:{a.Layer}:{a.Extrusion.Amount:R}:{a.Extrusion.Feed:R}:{a.Temperatures.Hotend:R}:{a.Temperatures.Bed:R}",
            nc1:           static n => $"n:{n.Part.Key.Kind.Key}:{n.Part.Key}");
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
public static class Post {
    static readonly Seq<GCommand> Commands = toSeq(GCommand.Items);

    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Motion motion, PostDialect dialect, FabricationInput input, PostPolicy? policy = null) =>
        Lower(new ProgramSource.Motion(motion), dialect, input, policy);

    // The placement modality — same name, input shape the discriminant; the owner Post case re-points here
    // when it widens to carry a Placement.
    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Placement placement, PostDialect dialect, FabricationInput input, PostPolicy? policy = null) =>
        Lower(new ProgramSource.Placement(placement), dialect, input, policy);

    static Fin<FabricationResult.PostedProgram> Lower(ProgramSource source, PostDialect dialect, FabricationInput input, PostPolicy? policy) {
        PostPolicy resolved = policy ?? PostPolicy.From(input, dialect);
        return
        Assemble(source, dialect, input, resolved).Bind(program =>
            Dialect.Seal(dialect, program.Nodes.Map(node => Dialect.Emit(dialect, node))).Map(words => {
                Seq<string> blocks = Render(words);
                ContentKey key = ContentKey.Of(EgressKind.CutProgram, Encoding.UTF8.GetBytes(string.Join("\n", blocks.ToArray())));
                return new FabricationResult.PostedProgram(blocks, key);
            }));
    }

    public static Fin<CutProgram> Parse(string source, PostDialect dialect) =>
        Lines(source).Fold(
            Fin.Succ(ModalState.Empty),
            (state, row) => state.Bind(modal => ParseBlock(row.Line, row.Text, dialect).Bind(block => modal.Push(row.Line, block))))
        .Map(modal => CutProgram.Of(modal.Nodes, dialect));

    // The branch Posting →[WIRE]: PackKind → Rasm edge: the conditioned motion polyline publishes through
    // the kernel encoder as content-keyed EncodedGeometry — backplot, storage, and analytics read the wire,
    // never re-parsed G-code.
    public static Fin<EncodedGeometry> Publish(FabricationResult.Motion motion, PackPolicy policy) =>
        Context.Millimeters().ToFin().Bind(context =>
            VectorCloud.Polyline(motion.Moves.Map(static m => m.To), context).Bind(cloud =>
                cloud is VectorCloud.PolylineCase poly
                    ? Encode.Apply(new PackOp.Toolpath(poly, policy))
                    : Fin.Fail<EncodedGeometry>(GeometryFault.DegenerateInput("post:publish-cloud").ToError())));

    internal static Fin<CutProgram> Assemble(ProgramSource source, PostDialect dialect, FabricationInput input, PostPolicy policy) =>
        ToolMagazine.Schedule(policy.Magazine, policy.Slots, policy.Work, policy.MagazinePolicy).Bind(changes =>
            Setup.Schedule(policy.Operations, policy.Schedule).Bind(schedule =>
                source.Switch(
                    state:     (dialect, input, policy, changes, schedule),
                    motion:    static (s, m) => MotionProgram(m.Value, s.dialect, s.policy, s.changes, s.schedule),
                    placement: static (s, p) => PlacementProgram(p.Value, s.dialect, s.policy, s.changes, s.schedule))));

    static Fin<CutProgram> MotionProgram(FabricationResult.Motion motion, PostDialect dialect, PostPolicy policy, Seq<ToolChange> changes, SetupSchedule schedule) =>
        Workholding.Condition(motion.Moves, policy.Fixture).Map(conditioned =>
            CutProgram.Of(Prologue(changes, schedule, policy).Concat(Lookahead(GNode.Moves(conditioned, Point3d.Origin), policy)).Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>())), dialect));

    static Fin<CutProgram> PlacementProgram(FabricationResult.Placement placement, PostDialect dialect, PostPolicy policy, Seq<ToolChange> changes, SetupSchedule schedule) =>
        PolygonAlgebra.NestingOrder(placement.Parts.Map(t => policy.Placed(t)).ToArr())
            .Map(index => placement.Parts[index])
            .Map(part => Condition(policy.Placed(part), policy))
            .TraverseM(identity)
            .As()
            .Map(loops => CutProgram.Of(Prologue(changes, schedule, policy).Concat(loops.Bind(loop => CutPath(loop, policy))).Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>())), dialect));

    static Fin<Loop> Condition(Loop profile, PostPolicy policy) =>
        profile.Closed
            ? ArcAlgebra.KerfArc(Seq(profile), policy.KerfWidth, profile.Winding() == Sign.Negative ? KerfSide.Inside : KerfSide.Outside)
                .Bind(loops => loops.HeadOrNone().ToFin(FabricationFault.KerfCollision(profile.At(0), policy.KerfWidth).ToError()))
                .Bind(loop => Compensate(loop, policy))
            : Fin.Fail<Loop>(FabricationFault.OpenLoop(FabConcern.Program, 0).ToError());

    static Seq<GNode> CutPath(Loop loop, PostPolicy policy) {
        Point3d pierce = ArcAlgebra.SampleAtLength(loop, 0.0).Point;
        Seq<GNode> lead = ArcAlgebra.LeadArc(loop, 0.0, policy.LeadRadius, policy.FeedCeiling * policy.Feed.EdgeFloor, LeadKind.In)
            .Match(Succ: moves => GNode.Moves(moves, pierce), Fail: _ => Seq<GNode>());
        return Seq<GNode>(new GNode.Word(GCommand.Rapid, Arr(new GParam('X', pierce.X), new GParam('Y', pierce.Y))))
            .Concat(PierceBlock(policy))
            .Concat(policy.Thc.Map(Thc))
            .Concat(lead)
            .Concat(Walk(loop, policy));
    }

    static Seq<GNode> PierceBlock(PostPolicy policy) =>
        policy.PierceTime <= 0.0
            ? Seq<GNode>()
            : Seq<GNode>(
                new GNode.Word(GCommand.AssistGas, Arr(new GParam('S', policy.AssistPressure))),
                new GNode.Word(GCommand.TorchOn, Arr<GParam>()),
                new GNode.CannedCycle(GCommand.Pierce, Arr(new GParam('P', policy.PierceTime)), Seq<Move>(), R: 0.0, Q: 0.0, P: policy.PierceTime, Repeats: 1));

    // Bevel DECIDES, posting RENDERS: each ThcDirective span lowers to one torch-height word.
    static GNode Thc(ThcDirective directive) =>
        directive.Switch(
            track: static t => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('V', t.ArcVoltage))),
            hold:  static _ => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('P', 1.0))),
            off:   static _ => new GNode.Word(GCommand.TorchHeight, Arr(new GParam('P', 0.0))));

    // ONE walk per loop — every span emits exactly once. A bulged span emits its arc from the Bulges column
    // (no g3 refit); a straight run at/above MinRunLength recovers arcs through BiArcFit2; a tab window
    // bridges energy-off → rapid → re-pierce. Named kernel exemption: the run accumulator is a measured
    // array-window kernel over the loop vertices.
    static Seq<GNode> Walk(Loop loop, PostPolicy policy) {
        Seq<GNode> outp = Seq<GNode>();
        Seq<Point3d> run = Seq(loop.At(0));
        double station = 0.0;
        for (int i = 0; i < loop.Count; i++) {
            Point3d a = loop.At(i);
            Point3d b = loop.At(i + 1);
            double length = a.DistanceTo(b);
            bool tab = policy.TabSpacing > 0.0 && (station % policy.TabSpacing) + length > policy.TabSpacing && policy.TabWidth > 0.0;
            double bulge = loop.Bulges.IsEmpty ? 0.0 : loop.Bulges[i % loop.Bulges.Count];
            if (tab || Math.Abs(bulge) > 1e-12) {
                outp = outp.Concat(FlushRun(run, policy)).Concat(
                    tab ? Bridge(b, policy)
                        : Seq(BulgeArc(a, b, bulge, Feedrate(loop, i, policy))));
                run = Seq(b);
            }
            else run = run.Add(b);
            station += length;
        }
        return outp.Concat(FlushRun(run, policy));
    }

    static Seq<GNode> Bridge(Point3d to, PostPolicy policy) =>
        Seq<GNode>(new GNode.Word(GCommand.SpindleStop, Arr<GParam>()),
                   new GNode.Word(GCommand.Rapid, Arr(new GParam('X', to.X), new GParam('Y', to.Y))))
            .Concat(PierceBlock(policy));

    // Bulge geometry (bulge = tan(theta/4)): radius r = c(1+b^2)/4|b|, sagitta s = |b|c/2, and the center
    // sits at mid - leftNormal * sign(b) * (r - s) — the arc-native span never refits through g3.
    static GNode BulgeArc(Point3d a, Point3d b, double bulge, double feed) {
        double chord = a.DistanceTo(b);
        double radius = chord * (1.0 + bulge * bulge) / (4.0 * Math.Abs(bulge));
        double sagitta = Math.Abs(bulge) * chord / 2.0;
        Vector3d left = new(-(b.Y - a.Y), b.X - a.X, 0.0);
        left.Unitize();
        Point3d mid = new(0.5 * (a.X + b.X), 0.5 * (a.Y + b.Y), a.Z);
        Point3d center = mid - left * (Math.Sign(bulge) * (radius - sagitta));
        return new GNode.Word(bulge > 0.0 ? GCommand.ArcCcw : GCommand.ArcCw,
            Arr(new GParam('X', b.X), new GParam('Y', b.Y), new GParam('I', center.X - a.X), new GParam('J', center.Y - a.Y), new GParam('F', feed)));
    }

    // Straight-run recovery: below MinRunLength the run stays line words; at/above, consecutive point
    // triples fit through BiArcFit2 stepping TWO spans per fit so no span emits twice; I/J read
    // Center - SampleT(0.0) per the admitted contract, and a zero tangent routes straight before fitting.
    static Seq<GNode> FlushRun(Seq<Point3d> run, PostPolicy policy) {
        if (run.Count < 2) return Seq<GNode>();
        double total = toSeq(Enumerable.Range(0, run.Count - 1)).Map(i => run[i].DistanceTo(run[i + 1])).Sum();
        if (run.Count < 3 || total < policy.Biarc.MinRunLength)
            return toSeq(Enumerable.Range(1, run.Count - 1)).Map(i =>
                (GNode)new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i].X), new GParam('Y', run[i].Y), new GParam('F', policy.FeedCeiling))));
        Seq<GNode> outp = Seq<GNode>();
        for (int i = 0; i + 2 < run.Count; i += 2) {
            Vector2d ta = new(run[i + 1].X - run[i].X, run[i + 1].Y - run[i].Y);
            Vector2d tb = new(run[i + 2].X - run[i + 1].X, run[i + 2].Y - run[i + 1].Y);
            if (ta.Length < 1e-9 || tb.Length < 1e-9) {
                outp = outp.Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i + 2].X), new GParam('Y', run[i + 2].Y), new GParam('F', policy.FeedCeiling))));
                continue;
            }
            BiArcFit2 fit = new(new Vector2d(run[i].X, run[i].Y), ta.Normalized, new Vector2d(run[i + 2].X, run[i + 2].Y), tb.Normalized);
            outp = fit.Distance(new Vector2d(run[i + 1].X, run[i + 1].Y)) > policy.Biarc.FitTolerance
                ? outp.Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i + 1].X), new GParam('Y', run[i + 1].Y), new GParam('F', policy.FeedCeiling))))
                      .Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[i + 2].X), new GParam('Y', run[i + 2].Y), new GParam('F', policy.FeedCeiling))))
                : outp.Add(ArcNode(fit.Arc1, fit.Arc1IsSegment, fit.Segment1.P1, policy))
                      .Add(ArcNode(fit.Arc2, fit.Arc2IsSegment, fit.Segment2.P1, policy));
        }
        if ((run.Count - 1) % 2 == 1)
            outp = outp.Add(new GNode.Word(GCommand.Feed, Arr(new GParam('X', run[run.Count - 1].X), new GParam('Y', run[run.Count - 1].Y), new GParam('F', policy.FeedCeiling))));
        return outp;
    }

    static GNode ArcNode(Arc2d arc, bool isSegment, Vector2d segmentEnd, PostPolicy policy) {
        if (isSegment)
            return new GNode.Word(GCommand.Feed, Arr(new GParam('X', segmentEnd.x), new GParam('Y', segmentEnd.y), new GParam('F', policy.FeedCeiling)));
        Vector2d start = arc.SampleT(0.0);
        Vector2d end = arc.SampleT(1.0);
        return new GNode.Word(arc.IsReversed ? GCommand.ArcCw : GCommand.ArcCcw,
            Arr(new GParam('X', end.x), new GParam('Y', end.y),
                new GParam('I', arc.Center.x - start.x), new GParam('J', arc.Center.y - start.y), new GParam('F', policy.FeedCeiling)));
    }

    static Fin<Loop> Compensate(Loop loop, PostPolicy policy) {
        double delta = policy.Comp.Deflection(policy.Cutting.Force(policy.Comp.ToolDiameter, policy.Cutting.FeedPerTooth)) + policy.Comp.ThermalGrowth;
        return delta <= 1e-9
            ? Fin.Succ(loop)
            : ArcAlgebra.ArcOffset(loop, loop.Winding() == Sign.Negative ? -delta : delta)
                .Bind(loops => loops.HeadOrNone().ToFin(FabricationFault.KerfCollision(loop.At(0), Math.Abs(delta)).ToError()));
    }

    internal static Seq<GNode> Lookahead(Seq<GNode> nodes, PostPolicy policy) {
        GNode[] b = nodes.ToArray();
        return toSeq(Enumerable.Range(0, b.Length)).Map(i =>
            b[i] is GNode.Word w && Cutting(w) && w.P('F').IsSome
                ? (GNode)w.With('F', Math.Min(w.P('F').IfNone(policy.FeedCeiling), JunctionFeed(b, i, policy.Dynamics)))
                : b[i]);
    }

    // The ONE motion-dynamics law COMPOSED: MotionDynamics.JunctionFeed (Kinematics/machine) is the
    // junction-deviation cap, evaluated here over the Dynamics.LookaheadBlocks window — a hand-rolled
    // corner formula beside the homed law is the scattered-law defect.
    static double JunctionFeed(GNode[] nodes, int i, MotionDynamics dynamics) {
        Seq<Point3d> ahead = toSeq(Enumerable.Range(0, dynamics.LookaheadBlocks + 1))
            .Filter(k => i + k < nodes.Length)
            .Map(k => nodes[i + k])
            .Bind(static node => node is GNode.Word w && w.P('X').IsSome && w.P('Y').IsSome
                ? Seq(new Point3d(w.P('X').IfNone(0.0), w.P('Y').IfNone(0.0), w.P('Z').IfNone(0.0)))
                : Seq<Point3d>());
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

    static double Feedrate(Loop loop, int i, PostPolicy policy) {
        Vector3d incoming = loop.At(i) - loop.At(i - 1);
        Vector3d outgoing = loop.At(i + 1) - loop.At(i);
        incoming.Unitize();
        outgoing.Unitize();
        double turn = Vector3d.VectorAngle(incoming, outgoing);
        double corner = turn <= policy.Feed.CornerAngle
            ? 1.0
            : Math.Max(policy.Feed.CornerFloor, 1.0 - (turn - policy.Feed.CornerAngle) / (Math.PI - policy.Feed.CornerAngle));
        return policy.FeedCeiling * corner;
    }

    // WCS lowers as the Wcs ROW carrying slot ordinal + family flag — the dialect roster renders the code;
    // coolant emits the policy row's own word; a G90-P WCS fiction is the deleted form.
    static Seq<GNode> Prologue(Seq<ToolChange> changes, SetupSchedule schedule, PostPolicy policy) =>
        schedule.Wcs.Map(wcs => (GNode)new GNode.Word(GCommand.Wcs, Arr(new GParam('P', wcs.Slot.Ordinal), new GParam('E', wcs.Slot.Family == WcsFamily.Extended ? 1.0 : 0.0))))
            .Concat(changes.Bind(change => Seq<GNode>(
                new GNode.Word(GCommand.ToolChange, Arr(new GParam('T', change.ProgramTool))),
                new GNode.Word(GCommand.LengthOffset, Arr(new GParam('H', change.ProgramTool), new GParam('Z', change.LengthOffset))))))
            .Concat(policy.Cooling.Word().Match(
                Some: word => Seq<GNode>(new GNode.Word(word, Arr<GParam>())),
                None: () => Seq<GNode>()));

    static Fin<Seq<GNode.Word>> ParseBlock(int line, string text, PostDialect dialect) {
        Arr<string> tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArr();
        Arr<string> commandTokens = tokens.Filter(t => Commands.Exists(c => string.Equals(c.Code, t, StringComparison.Ordinal)));
        Arr<string> paramTokens = tokens.Filter(t => !commandTokens.Contains(t));
        return paramTokens.Map(t => ParseParam(line, t)).ToSeq().TraverseM(identity).As().Bind(parameters =>
            commandTokens.IsEmpty
                ? Fin.Fail<Seq<GNode.Word>>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError())
                : Fin.Succ(toSeq(commandTokens).Map((token, k) => {
                    GCommand command = Command(token, parameters.ToArr(), dialect);
                    bool carrier = command.Group == ModalGroup.Motion || (k == 0 && !commandTokens.Exists(t => Command(t, parameters.ToArr(), dialect).Group == ModalGroup.Motion));
                    return new GNode.Word(command, carrier ? parameters.ToArr() : Arr<GParam>());
                })));
    }

    // Emission aliases legitimately SHARE wire codes (a pierce IS a G4 dwell on a torch dialect, an
    // extrusion IS a G1 move carrying an E word) — the lookup disambiguates a shared code by dialect and
    // parameter shape rather than inventing nonstandard codes.
    static GCommand Command(string code, Arr<GParam> parameters, PostDialect dialect) =>
        Commands.Filter(command => string.Equals(command.Code, code, StringComparison.Ordinal)) switch {
            { Count: 1 } lone => lone.Head,
            var shared => shared.Find(command =>
                    (command == GCommand.Extrude && parameters.Exists(static p => p.Address == 'E'))
                    || (command == GCommand.Pierce && dialect == PostDialect.Hypertherm))
                .IfNone(shared.Head),
        };

    static Fin<GParam> ParseParam(int line, string token) =>
        token.Length < 2 || !double.TryParse(token[1..], out double value)
            ? Fin.Fail<GParam>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError())
            : Fin.Succ(new GParam(token[0], value));

    static Seq<(int Line, string Text)> Lines(string source) =>
        source.Split('\n')
            .Select((text, index) => (Line: index + 1, Text: text.Trim()))
            .Where(row => row.Text.Length > 0)
            .ToSeq();

    static Seq<string> Render(Seq<GWord> words) =>
        words.Map(word => word.Switch(
            address: static w => $"{w.Mode.Map(m => $"{m.Code} ").IfNone(string.Empty)}{w.Code} {string.Join(" ", w.Words.Map(p => $"{p.Address}{p.Value:0.###}").ToArray())}".Trim(),
            conversational: static w => $"{w.Verb} {string.Join(" ", w.Words.Map(p => $"{p.Address}{(p.Value >= 0 ? "+" : "")}{Math.Round(p.Value, w.Decimals)}").ToArray())}".Trim(),
            cycleCall: static w => $"{w.Dialect}:{w.Kind.Key}:R{w.R:0.###}:Q{w.Q:0.###}:P{w.P:0.###}:L{w.Repeats}",
            macro: w => $"{string.Join(" ", w.Slots.Map(s => $"{s.Key}{s.Value:0.###}").ToArray())}\n{string.Join("\n", Render(w.Body).ToArray())}",
            subprogram: w => $"O{w.Label} L{w.Repeats}\n{string.Join("\n", Render(w.Body).ToArray())}",
            additive: static w => $"LAYER{w.Layer} E{w.Extrusion.Amount:0.###} F{w.Extrusion.Feed:0.###}",
            nc1: static w => string.Join("\n", w.Records.ToArray()),
            fault: static w => w.Error.Message,
            expanded: w => string.Join("\n", Render(w.Words).ToArray())));

    static bool Cutting(GNode.Word w) =>
        w.Command == GCommand.Feed || w.Command == GCommand.ArcCw || w.Command == GCommand.ArcCcw || w.Command == GCommand.Extrude;
}
```
