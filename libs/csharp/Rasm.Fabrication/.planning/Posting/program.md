# [RASM_FABRICATION_PROGRAM]

`Post` owns the dialect-neutral cut-program AST, the `Run(Post{Motion, PostDialect})` lowering body, and the `Post.Parse` ingress arm. `GNode` is the sole program vocabulary consumed by `Dialect.Emit`; `CutProgram` carries the AST plus its `ContentKey`, and `PostedProgram` carries lowered block text plus the `EgressKind.CutProgram` content key. Motion enters already CAM-conditioned, while placement-style 2D cutting enters the local cut-conditioning rail exactly once: arc-native kerf and lead compose `ArcAlgebra`, contour order composes `PolygonAlgebra.NestingOrder`, WCS rows compose `Setup.Schedule`, tool swaps compose `Magazine.Schedule`, height directives compose `Bevel`, and line-sourced kernel mesh-section chords alone compose `g3.BiArcFit2`.

## [01]-[INDEX]

- [01]-[CUT_PROGRAM]: owns `GCommand`, `GNodeKind`, `ModalGroup`, `GNode`, `GWord`, `CutProgram`, `PostPolicy`, the `Run(Post)` lowering body, placement cut-conditioning, line-sourced biarc recovery, and `Post.Parse`.

## [02]-[CUT_PROGRAM]

- Owner: `GNode` is the neutral AST consumed by `Dialect.Emit`; `GWord` is the lowered block evidence; `CutProgram` stores nodes, dialect, and the AST content key; `Post` owns `Lower`, `Assemble`, `Parse`, `Lookahead`, and the placement-only cut-conditioning rail.
- Cases: `GNodeKind` rows `word` · `canned-cycle` · `macro` · `subprogram` · `additive-layer` · `nc1`; `ModalGroup` rows `motion` · `plane` · `distance` · `units` · `feed`; `LeadStyle` rows `none` · `line` · `arc`; `FeedMode` rows `units-per-minute` · `inverse-time`; `GCommand` rows carry motion, arc, cycle, spindle, coolant, THC, pierce, additive, tool-change, WCS, probing, and program-control words.
- Entry: `Run(Post{FabricationResult.Motion, PostDialect})` calls `Post.Lower` in the owner switch body; `public static Fin<CutProgram> Post.Parse(string source, PostDialect dialect)` is the ingress round-trip arm.
- Auto: `Lower` assembles `ProgramSource.Motion`, inserts workholding, tool-change, and WCS rows, lowers nodes through `Dialect.Emit`, seals the block cap, renders text, and mints `ContentKey.Of(EgressKind.CutProgram, canonicalBytes)`. `ProgramSource.Placement` alone runs `Kerf`/`Lead`/`Pierce`/`Tab`/`NestingOrder`; `Motion` never receives a kerf pass. `Parse` walks the five RS274 modal groups and routes `ProgramParse(line, group)` on conflicts.
- Receipt: `CutProgram.Key` identifies the AST payload; `FabricationResult.PostedProgram(Seq<string> Blocks, ContentKey Key)` identifies the lowered machine artifact; `ProgramParse` and `BlockCapExceeded` carry typed failure evidence.
- Packages: `Geometry2D/arcs#ARC_ALGEBRA` (`KerfArc`, `LeadArc`, `AdaptiveArc`); `Geometry2D/algebra#POLYGON_ALGEBRA` (`NestingOrder`); `Tooling/magazine#TOOL_MAGAZINE` (`Schedule`, `ToolChange`); `Fixturing/setups#SETUP_SCHEDULER` (`Setup.Schedule`, WCS assignments); `Fixturing/workholding#WORKHOLDING` (`Condition`); `Toolpath/bevel#BEVEL` (`ThcDirective` spans); `Posting/dialect#DIALECT_EMIT` (`Dialect.Emit`, `Seal`); `Posting/optimization#PROGRAM_OPTIMIZATION` (`CutProgram` AST consumer); `Process/owner#FABRICATION_OWNER` (`Move`, `PartTransform`, `EgressKind`, `ContentKey`); `Process/faults#FAULT_BAND` (`ProgramParse`, `BlockCapExceeded`); `geometry3Sharp` (`BiArcFit2`, `Arc2d`, `Segment2d`, `Vector2d`) for line-sourced fitting only.
- Growth: a controller grammar change lands in `PostDialect` and `Dialect.Emit`; a program syntax construct lands as one `GNode` case and one lowering arm; a parser modal group lands as one `ModalGroup` row; a cut-conditioning concern lands as a `PostPolicy` row or an owned page seam, not a second public post fold.
- Boundary: the former public post fold is dead; the former posting-side contour order is dead; posting renders setup WCS rows and bevel THC spans rather than assigning or deciding them; raw `XxHash128`/`GenerateHash`, raw G-code string synthesis, default WCS selection, a second kerf pass over `Motion`, parse success probes, a g3 refit over arc-native contours, and a plane-internal `CutProgram` result payload are rejected shapes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Fabrication.Toolpath;
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

[SmartEnum<string>]
public sealed partial class ModalGroup {
    public static readonly ModalGroup Motion = new("motion");
    public static readonly ModalGroup Plane = new("plane");
    public static readonly ModalGroup Distance = new("distance");
    public static readonly ModalGroup Units = new("units");
    public static readonly ModalGroup Feed = new("feed");
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

[SmartEnum<string>]
public sealed partial class GCommand {
    public static readonly GCommand Rapid = new("rapid", "G0", ModalGroup.Motion);
    public static readonly GCommand Feed = new("feed", "G1", ModalGroup.Motion);
    public static readonly GCommand ArcCw = new("arc-cw", "G2", ModalGroup.Motion);
    public static readonly GCommand ArcCcw = new("arc-ccw", "G3", ModalGroup.Motion);
    public static readonly GCommand Dwell = new("dwell", "G4", ModalGroup.Motion);
    public static readonly GCommand PlaneXy = new("plane-xy", "G17", ModalGroup.Plane);
    public static readonly GCommand Absolute = new("absolute", "G90", ModalGroup.Distance);
    public static readonly GCommand Metric = new("metric", "G21", ModalGroup.Units);
    public static readonly GCommand Spindle = new("spindle", "M3", ModalGroup.Motion);
    public static readonly GCommand Coolant = new("coolant", "M8", ModalGroup.Motion);
    public static readonly GCommand ProgramEnd = new("program-end", "M30", ModalGroup.Motion);
    public static readonly GCommand Css = new("css", "G96", ModalGroup.Motion);
    public static readonly GCommand ThreadCycle = new("thread-cycle", "G92", ModalGroup.Motion);
    public static readonly GCommand TorchOn = new("torch-on", "M07", ModalGroup.Motion);
    public static readonly GCommand TorchHeight = new("torch-height", "THC", ModalGroup.Motion);
    public static readonly GCommand Pierce = new("pierce", "G4", ModalGroup.Motion);
    public static readonly GCommand AssistGas = new("assist-gas", "M64", ModalGroup.Motion);
    public static readonly GCommand HotendTemp = new("hotend-temp", "M104", ModalGroup.Motion);
    public static readonly GCommand HotendWait = new("hotend-wait", "M109", ModalGroup.Motion);
    public static readonly GCommand Extrude = new("extrude", "G1", ModalGroup.Motion);
    public static readonly GCommand BedTemp = new("bed-temp", "M140", ModalGroup.Motion);
    public static readonly GCommand DustCollect = new("dust-collect", "M65", ModalGroup.Motion);
    public static readonly GCommand ToolChange = new("tool-change", "M6", ModalGroup.Motion);
    public static readonly GCommand LengthOffset = new("length-offset", "G43", ModalGroup.Motion);
    public static readonly GCommand Probe = new("probe", "G31", ModalGroup.Motion);

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
    Arr<Loop> Profiles,
    Seq<WorkItem> Work,
    Magazine Magazine,
    SlotMap Slots,
    MagazinePolicy MagazinePolicy,
    Seq<Operation> Operations,
    SchedulePolicy Schedule,
    Fixture Fixture) {
    // The owner#run Post-arm derivation: everything the input carries binds; shop context absent from the
    // input (magazine, work, operations, fixture) seeds empty so the arm is TOTAL and prologue rows degrade
    // to absence — a richer PostPolicy threads through Lower's optional parameter, never a second entry.
    public static PostPolicy From(FabricationInput input, PostDialect dialect) => new(
        KerfWidth: 0.0, Lead: LeadStyle.None, LeadRadius: 0.0, TabWidth: 0.0, TabSpacing: 0.0,
        PierceTime: 0.0, AssistPressure: 0.0, MeltTemp: 0.0,
        FeedCeiling: MotionDynamics.Canonical.CuttingFeed, MaxAxes: input.Machine.AxisCount,
        RemovalRate: 0.0, Feed: FeedPolicy.Canonical, Biarc: BiarcPolicy.Canonical,
        Dynamics: MotionDynamics.Canonical, Cutting: new CuttingData(Kc11: 0.0, Mc: 0.0, SurfaceSpeed: 0.0, FeedPerTooth: 0.0, ClassFallback: true),
        Comp: CompPolicy.Disabled, Cooling: CoolingPolicy.Off,
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

    public sealed record Word(GCommand Command, Arr<GParam> Words, Option<FeedMode> Mode = default) : GNode;
    public sealed record CannedCycle(GCommand Command, Arr<GParam> SingleBlockWords, Seq<Move> ExpandedMoves, double R, double Q, double P, int Repeats, Option<FeedMode> Mode = default) : GNode;
    public sealed record Macro(GNodeKind Kind, Arr<MacroSlot> Slots, Arr<GNode> Body) : GNode;
    public sealed record Subprogram(GNodeKind Kind, int Label, int Repeats, Arr<GNode> Body) : GNode;
    public sealed record AdditiveLayer(int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures) : GNode;
    public sealed record Nc1(SteelPart Part) : GNode;

    public GNodeKind Kind =>
        Switch(
            word:         static _ => GNodeKind.Word,
            cannedCycle:  static _ => GNodeKind.CannedCycle,
            macro:        static _ => GNodeKind.Macro,
            subprogram:   static _ => GNodeKind.Subprogram,
            additiveLayer: static _ => GNodeKind.AdditiveLayer,
            nc1:          static _ => GNodeKind.Nc1);

    public static GNode Move(Move move) =>
        move.Arc.Match(
            Some: arc => new Word(
                arc.Clockwise ? GCommand.ArcCw : GCommand.ArcCcw,
                Arr(new GParam('X', move.To.X), new GParam('Y', move.To.Y), new GParam('Z', move.To.Z), new GParam('I', arc.Center.X - move.To.X), new GParam('J', arc.Center.Y - move.To.Y), new GParam('F', move.Feed))),
            None: () => new Word(
                move.Rapid ? GCommand.Rapid : GCommand.Feed,
                move.Rapid
                    ? Arr(new GParam('X', move.To.X), new GParam('Y', move.To.Y), new GParam('Z', move.To.Z))
                    : Arr(new GParam('X', move.To.X), new GParam('Y', move.To.Y), new GParam('Z', move.To.Z), new GParam('F', move.Feed))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GWord {
    private GWord() { }

    public sealed record Address(string Code, Arr<GParam> Words, Option<FeedMode> Mode, bool Modal, ArcMode Arc) : GWord;
    public sealed record CycleCall(string Dialect, GNodeKind Kind, double R, double Q, double P, int Repeats) : GWord;
    public sealed record Macro(MacroGrammar Grammar, Arr<MacroSlot> Slots, Arr<GNode> Body) : GWord;
    public sealed record Subprogram(SubprogramGrammar Grammar, int Label, int Repeats, Arr<GNode> Body) : GWord;
    public sealed record Additive(int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures, int Decimals) : GWord;
    public sealed record Nc1(Nc1Program Program, ContentKey Key) : GWord;
    public sealed record Fault(Error Error) : GWord;
    public sealed record Expanded(Seq<GWord> Words) : GWord;
}

public sealed record CutProgram(Seq<GNode> Nodes, PostDialect Dialect, ContentKey Key) {
    public static CutProgram Of(Seq<GNode> nodes, PostDialect dialect) =>
        new(nodes, dialect, ContentKey.Of(EgressKind.CutProgram, Canonical(nodes, dialect)));

    public static byte[] Canonical(Seq<GNode> nodes, PostDialect dialect) =>
        Encoding.UTF8.GetBytes(string.Join("\n", nodes.Map(node => $"{dialect.Key}:{node.Kind.Key}:{node.GetHashCode():x8}").ToArray()));
}

public sealed record ModalState(Map<ModalGroup, GCommand> Active, Seq<GNode> Nodes) {
    public static readonly ModalState Empty = new(Map<ModalGroup, GCommand>(), Seq<GNode>());

    public Fin<ModalState> Push(int line, GNode.Word node) =>
        Active.Find(node.Command.Group).Match(
            Some: active => active == node.Command || node.Command.Group == ModalGroup.Motion
                ? Fin.Succ(new ModalState(Active.AddOrUpdate(node.Command.Group, node.Command), Nodes.Add(node)))
                : Fin.Fail<ModalState>(FabricationFault.ProgramParse(line, node.Command.Group).ToError()),
            None: () => Fin.Succ(new ModalState(Active.Add(node.Command.Group, node.Command), Nodes.Add(node))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Post {
    static readonly Seq<GCommand> Commands = Seq(
        GCommand.Rapid, GCommand.Feed, GCommand.ArcCw, GCommand.ArcCcw, GCommand.Dwell, GCommand.PlaneXy, GCommand.Absolute, GCommand.Metric,
        GCommand.Spindle, GCommand.Coolant, GCommand.ProgramEnd, GCommand.Css, GCommand.ThreadCycle, GCommand.TorchOn, GCommand.TorchHeight,
        GCommand.Pierce, GCommand.AssistGas, GCommand.HotendTemp, GCommand.HotendWait, GCommand.Extrude, GCommand.BedTemp, GCommand.DustCollect,
        GCommand.ToolChange, GCommand.LengthOffset, GCommand.Probe);

    public static Fin<FabricationResult.PostedProgram> Lower(FabricationResult.Motion motion, PostDialect dialect, FabricationInput input, PostPolicy? policy = null) {
        PostPolicy resolved = policy ?? PostPolicy.From(input, dialect);
        return
        Assemble(new ProgramSource.Motion(motion), dialect, input, resolved).Bind(program =>
            Dialect.Seal(dialect, program.Nodes.Map(node => Dialect.Emit(dialect, node))).Map(words => {
                Seq<string> blocks = Render(words);
                ContentKey key = ContentKey.Of(EgressKind.CutProgram, Encoding.UTF8.GetBytes(string.Join("\n", blocks.ToArray())));
                return new FabricationResult.PostedProgram(blocks, key);
            }));
    }

    public static Fin<CutProgram> Parse(string source, PostDialect dialect) =>
        Lines(source).Fold(
            Fin.Succ(ModalState.Empty),
            (state, row) => state.Bind(modal => ParseWord(row.Line, row.Text, dialect).Bind(modal.Push)))
        .Map(modal => CutProgram.Of(modal.Nodes, dialect));

    internal static Fin<CutProgram> Assemble(ProgramSource source, PostDialect dialect, FabricationInput input, PostPolicy policy) =>
        ToolMagazine.Schedule(policy.Magazine, policy.Slots, policy.Work, policy.MagazinePolicy).Bind(changes =>
            Setup.Schedule(policy.Operations, policy.Schedule).Bind(schedule =>
                source.Switch(
                    state:     (dialect, input, policy, changes, schedule),
                    motion:    static (s, m) => MotionProgram(m.Value, s.dialect, s.policy, s.changes, s.schedule),
                    placement: static (s, p) => PlacementProgram(p.Value, s.dialect, s.policy, s.changes, s.schedule))));

    static Fin<CutProgram> MotionProgram(FabricationResult.Motion motion, PostDialect dialect, PostPolicy policy, Seq<ToolChange> changes, SetupSchedule schedule) =>
        Workholding.Condition(motion.Moves, policy.Fixture).Map(conditioned =>
            CutProgram.Of(Prologue(changes, schedule).Concat(Lookahead(conditioned.Map(GNode.Move), policy)).Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>())), dialect));

    static Fin<CutProgram> PlacementProgram(FabricationResult.Placement placement, PostDialect dialect, PostPolicy policy, Seq<ToolChange> changes, SetupSchedule schedule) =>
        PolygonAlgebra.NestingOrder(placement.Parts.Map(t => policy.Placed(t)).ToArr()) is Seq<int> order
            ? order.Map(index => placement.Parts[index])
                .Map(part => Condition(policy.Placed(part), policy))
                .TraverseM(identity)
                .Map(loops => CutProgram.Of(Prologue(changes, schedule).Concat(loops.Bind(loop => CutPath(loop, policy))).Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>())), dialect))
            : Fin.Fail<CutProgram>(GeometryFault.DegenerateInput("post:placement-order").ToError());

    static Fin<Loop> Condition(Loop profile, PostPolicy policy) =>
        profile.Closed
            ? ArcAlgebra.KerfArc(Seq(profile), policy.KerfWidth, profile.Winding() == Sign.Negative ? KerfSide.Inside : KerfSide.Outside)
                .Bind(loops => loops.HeadOrNone().ToFin(FabricationFault.KerfCollision(profile.At(0), policy.KerfWidth).ToError()))
                .Bind(loop => Compensate(loop, policy))
            : Fin.Fail<Loop>(FabricationFault.OpenLoop(FabConcern.Program, 0).ToError());

    static Seq<GNode> CutPath(Loop loop, PostPolicy policy) {
        Point3d pierce = ArcAlgebra.SampleAtLength(loop, 0.0).Point;
        Seq<GNode> lead = ArcAlgebra.LeadArc(loop, 0.0, policy.LeadRadius, policy.FeedCeiling * policy.Feed.EdgeFloor, LeadKind.In)
            .Match(Succ: moves => moves.Map(GNode.Move), Fail: _ => Seq<GNode>());
        return Seq<GNode>(new GNode.Word(GCommand.Rapid, Arr(new GParam('X', pierce.X), new GParam('Y', pierce.Y))))
            .Concat(Pierce(policy))
            .Concat(lead)
            .Concat(Tabbed(loop, policy))
            .Concat(LineBiarc(loop, policy));
    }

    static Seq<GNode> Pierce(PostPolicy policy) =>
        policy.PierceTime <= 0.0
            ? Seq<GNode>()
            : Seq<GNode>(
                new GNode.Word(GCommand.AssistGas, Arr(new GParam('S', policy.AssistPressure))),
                new GNode.Word(GCommand.TorchOn, Arr<GParam>()),
                new GNode.CannedCycle(GCommand.Pierce, Arr(new GParam('P', policy.PierceTime)), Seq<Move>(), R: 0.0, Q: 0.0, P: policy.PierceTime, Repeats: 1));

    static Seq<GNode> Tabbed(Loop loop, PostPolicy policy) =>
        toSeq(Enumerable.Range(0, loop.Count)).Bind(index => {
            double length = loop.At(index).DistanceTo(loop.At(index + 1));
            bool bridge = policy.TabSpacing > 0.0 && (index * length) % policy.TabSpacing < policy.TabWidth;
            return bridge
                ? Seq<GNode>(new GNode.Word(GCommand.Rapid, Arr(new GParam('X', loop.At(index + 1).X), new GParam('Y', loop.At(index + 1).Y))))
                : Seq(GNode.Move(new Move(loop.At(index + 1), Rapid: false, Feed: Feedrate(loop, index, policy), Arc: Option<ArcCenter>.None)));
        });

    static Seq<GNode> LineBiarc(Loop loop, PostPolicy policy) =>
        toSeq(Enumerable.Range(1, Math.Max(0, loop.Count - 2))).Bind(index => {
            Point3d a = loop.At(index - 1);
            Point3d b = loop.At(index + 1);
            Vector3d ta = loop.At(index) - a;
            Vector3d tb = b - loop.At(index);
            BiArcFit2 fit = new(new Vector2d(a.X, a.Y), new Vector2d(ta.X, ta.Y).Normalized, new Vector2d(b.X, b.Y), new Vector2d(tb.X, tb.Y).Normalized);
            Vector2d probe = new(loop.At(index).X, loop.At(index).Y);
            return fit.Distance(probe) > policy.Biarc.FitTolerance
                ? Seq(GNode.Move(new Move(loop.At(index), Rapid: false, Feed: Feedrate(loop, index, policy), Arc: Option<ArcCenter>.None)))
                : Seq(ArcNode(fit.Arc1, fit.Arc1IsSegment, fit.Segment1.P1, policy), ArcNode(fit.Arc2, fit.Arc2IsSegment, fit.Segment2.P1, policy));
        });

    static GNode ArcNode(Arc2d arc, bool isSegment, Vector2d segmentEnd, PostPolicy policy) {
        if (isSegment)
            return GNode.Move(new Move(new Point3d(segmentEnd.x, segmentEnd.y, 0.0), Rapid: false, Feed: policy.FeedCeiling, Arc: Option<ArcCenter>.None));
        Vector2d end = arc.SampleT(1.0);
        return GNode.Move(new Move(new Point3d(end.x, end.y, 0.0), Rapid: false, Feed: policy.FeedCeiling,
            Arc: Some(new ArcCenter(new Point3d(arc.Center.x, arc.Center.y, 0.0), arc.IsReversed))));
    }

    static Fin<Loop> Compensate(Loop loop, PostPolicy policy) {
        double delta = policy.Comp.Deflection(policy.Cutting.Force(policy.Comp.ToolDiameter, policy.Cutting.FeedPerTooth)) + policy.Comp.ThermalGrowth;
        return delta <= 1e-9
            ? Fin.Succ(loop)
            : ArcAlgebra.ArcOffset(loop, loop.Winding() == Sign.Negative ? -delta : delta)
                .Bind(loops => loops.HeadOrNone().ToFin(FabricationFault.KerfCollision(loop.At(0), Math.Abs(delta)).ToError()));
    }

    internal static Seq<GNode> Lookahead(Seq<GNode> nodes, PostPolicy policy) =>
        nodes.Map((i, node) => node is GNode.Move move
            ? GNode.Move(move.Value with { Feed = JunctionFeed(nodes, i, move.Value, policy.Dynamics) })
            : node).ToSeq();

    // The ONE motion-dynamics law (Kinematics/machine) evaluated per junction: corner angle clamps the
    // commanded feed to the accel-limited junction speed v = sqrt(a · CornerTolerance / tan(theta/2)),
    // windowed over Dynamics.LookaheadBlocks; simulate re-reads the SAME law as its time certificate.
    static double JunctionFeed(Seq<GNode> nodes, int i, Move move, MotionDynamics dynamics) {
        Seq<Point3d> ahead = Enumerable.Range(0, dynamics.LookaheadBlocks + 1)
            .TakeWhile(k => i + k < nodes.Count)
            .Select(k => nodes[i + k]).OfType<GNode.Move>()
            .Select(n => n.Value.To).ToSeq();
        double turn = ahead.Count < 3 ? 0.0 : Enumerable.Range(1, ahead.Count - 2).Max(j => {
            Vector3d incoming = ahead[j] - ahead[j - 1];
            Vector3d outgoing = ahead[j + 1] - ahead[j];
            incoming.Unitize();
            outgoing.Unitize();
            return Vector3d.VectorAngle(incoming, outgoing);
        });
        double junction = turn <= 1e-9
            ? dynamics.FeedFor(move)
            : Math.Sqrt(dynamics.Acceleration * dynamics.CornerTolerance / Math.Tan(turn / 2.0)) * 60.0;
        return Math.Min(dynamics.FeedFor(move), junction);
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

    static Seq<GNode> Prologue(Seq<ToolChange> changes, SetupSchedule schedule) =>
        schedule.Wcs.Map(wcs => new GNode.Word(GCommand.Absolute, Arr(new GParam('P', wcs.Slot.Ordinal)))).Concat(
            changes.Bind(change => Seq<GNode>(
                new GNode.Word(GCommand.ToolChange, Arr(new GParam('T', change.ProgramTool))),
                new GNode.Word(GCommand.LengthOffset, Arr(new GParam('H', change.ProgramTool), new GParam('Z', change.LengthOffset))))));

    static Fin<GNode.Word> ParseWord(int line, string text, PostDialect dialect) {
        Arr<string> tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArr();
        Arr<GParam> parameters = tokens.Tail.Map(ParseParam).Somes().ToArr();
        return tokens.HeadOrNone().Bind(code => Command(code, parameters, dialect)).Match(
            Some: command => Fin.Succ(new GNode.Word(command, parameters)),
            None: () => Fin.Fail<GNode.Word>(FabricationFault.ProgramParse(line, ModalGroup.Motion).ToError()));
    }

    // Emission aliases legitimately SHARE wire codes (a pierce IS a G4 dwell on a torch dialect, an
    // extrusion IS a G1 move carrying an E word) — the parse lookup therefore disambiguates a shared code
    // by dialect and parameter shape rather than inventing nonstandard codes: G1 with an E word reads
    // Extrude, G4 on the torch dialect reads Pierce, and the roster-first neutral command carries the rest.
    static Option<GCommand> Command(string code, Arr<GParam> parameters, PostDialect dialect) =>
        Commands.Filter(command => string.Equals(command.Code, code, StringComparison.Ordinal)) switch {
            { IsEmpty: true } => None,
            { Count: 1 } lone => Some(lone.Head),
            var shared => Some(shared.Find(command =>
                    (command == GCommand.Extrude && parameters.Exists(static p => p.Address == 'E'))
                    || (command == GCommand.Pierce && dialect == PostDialect.Hypertherm))
                .IfNone(shared.Head)),
        };

    static Option<GParam> ParseParam(string token) =>
        token.Length < 2 || !double.TryParse(token[1..], out double value)
            ? Option<GParam>.None
            : Some(new GParam(token[0], value));

    static Seq<(int Line, string Text)> Lines(string source) =>
        source.Split('\n')
            .Select((text, index) => (Line: index + 1, Text: text.Trim()))
            .Where(row => row.Text.Length > 0)
            .ToSeq();

    static Seq<string> Render(Seq<GWord> words) =>
        words.Map(word => word.Switch(
            address: static w => $"{w.Mode.Map(m => $"{m.Code} ").IfNone(string.Empty)}{w.Code} {string.Join(" ", w.Words.Map(p => $"{p.Address}{p.Value:0.###}").ToArray())}".Trim(),
            cycleCall: static w => $"{w.Dialect}:{w.Kind.Key}:R{w.R:0.###}:Q{w.Q:0.###}:P{w.P:0.###}:L{w.Repeats}",
            macro: static w => string.Join(" ", w.Slots.Map(s => $"{s.Key}{s.Value:0.###}").ToArray()),
            subprogram: static w => $"O{w.Label} L{w.Repeats}",
            additive: static w => $"LAYER{w.Layer} E{w.Extrusion.Amount:0.###} F{w.Extrusion.Feed:0.###}",
            nc1: static w => $"NC1 {w.Key.Kind.Key}",
            fault: static w => w.Error.Message,
            expanded: static w => string.Join("\n", Render(w.Words).ToArray())));
}
```
