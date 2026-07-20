# [RASM_FABRICATION_PROGRAM]

`Post` owns one dialect-neutral `CutProgram` from admitted source through modal interpretation, cut conditioning, grammar lowering, rendered records, and analytic `PackKind.Toolpath` projection. `GNode.Directive` preserves controller directives and specialized toolpath evidence beside motion; `GWord.Render` is the physical-record correspondence consumed by capacity checks and receipts.

`PostSource`, `PostDialect`, `EmitPolicy`, `SetupPlan`, `Fixture`, `ChainRow`, `ToolChange`, and `ContentKey` arrive as settled seams. `ProgramIngress` parameterizes RS274 encoding and checksum admission beside NC1 admission, `PostPolicy` admits dimensioned cut and emission policy once, and `ProgramView` parameterizes geometry egress without a second posting surface.

## [01]-[INDEX]

- [02]-[PROGRAM]: `GNode`, `GWord`, `CutProgram`, command grammar, modal interpretation, and content identity.
- [03]-[BOUNDARIES]: `PostPolicy` admission, `Post.Lower`, `Post.Parse`, and `Post.Publish`.
- [04]-[CONDITIONING]: placement, tooling, setup, workholding, arc conditioning, tab partition, and lookahead folds.

## [02]-[PROGRAM]

- Owner: `CutProgram` mints the canonical AST and `Post` owns every transform that changes it.
- Cases: `GNode` carries block framing beside executable node families; `GValue` preserves numeric, variable, expression, and text evidence; `GCommand.Wcs` and `WcsExtended` retain base and extended coordinate forms; `ProgramEvent` carries the canonical interpretation.
- Entry: `Post.Lower` discriminates on `PostSource`; `Post.Parse` discriminates on `ProgramIngress`; `Post.Publish` projects one `CutProgram` through `ProgramView`.
- Auto: `GCommand.Admit` composes address shape with row-owned scalar policy before AST construction, `ModalState` threads controller state once, and `Dialect.Emit` derives framed physical records, record capacity, bytes, and content identity together.
- Law: `GCommand.Requires` and `GCommand.Modalities` declare what a command demands of a controller, and `GCommand.Admits` decides admissibility against `PostDialect.Features` and `PostDialect.Modalities` — no dialect identity is ever tested, and no roster mirrors the vocabulary.
- Law: `ProgramUnits` carries one millimetre scale in both directions, and `GNode.Word.With` preserves the replaced value's source units or the word's established source-unit row. Every `ProgramEvent` carries one structural `ProgramLocus`; motion also carries every admitted axis, resolved plane, and arc center, so consumers never re-derive modal state, discard rotary or auxiliary axes, or substitute a chord.
- Receipt: `CutProgram.Key` identifies the length-framed AST; `PostedProgram.Key` identifies rendered records; admitted `ProgramTrace` preserves modal state and the complete node-and-repeat path of every expanded executable leaf.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `UnitsNet` through `PhysicsQuantity`, `geometry3Sharp`, `CavalierContours` through `Loop`, `ArcAlgebra`, and `PlineSeg`, `MTConnect.NET-Common` through `ToolAssembly`, and `DSTV.Net` through `SteelImport.Read`.
- Growth: a syntax construct is one `GNode` case; a command is one `GCommand` row with its grammar and demanded features; a parse grammar is one `ProgramIngress` case; a projection is one `ProgramView` row.
- Boundary: dialect byte spelling stays in `Dialect`; AST rewriting stays in `Optimize`; simulation consumes `ProgramTrace`; host file custody stays above this package.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CavalierContours.Polyline;
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
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [VOCABULARY] -------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ProgramUnits {
    public static readonly ProgramUnits Metric = new("metric", Length.FromMillimeters(1.0).Millimeters);
    public static readonly ProgramUnits Imperial = new("imperial", new Length(1.0, UnitsNet.Units.LengthUnit.Inch).Millimeters);

    public double MillimetersPerUnit { get; }

    public double Canonical(double native) => native * MillimetersPerUnit;
    public double Native(double canonical) => canonical / MillimetersPerUnit;
}

[SmartEnum<string>]
public sealed partial class DistanceMode {
    public static readonly DistanceMode Absolute = new("absolute");
    public static readonly DistanceMode Incremental = new("incremental");
}

[SmartEnum<string>]
public sealed partial class ModalGroup {
    public static readonly ModalGroup Motion = new("motion");
    public static readonly ModalGroup Plane = new("plane");
    public static readonly ModalGroup Distance = new("distance");
    public static readonly ModalGroup ArcDistance = new("arc-distance");
    public static readonly ModalGroup Units = new("units");
    public static readonly ModalGroup Feed = new("feed");
    public static readonly ModalGroup Spindle = new("spindle");
    public static readonly ModalGroup Coolant = new("coolant");
    public static readonly ModalGroup CutterComp = new("cutter-comp");
    public static readonly ModalGroup ToolLength = new("tool-length");
    public static readonly ModalGroup Wcs = new("wcs");
    public static readonly ModalGroup Retract = new("retract");
    public static readonly ModalGroup PathControl = new("path-control");
    public static readonly ModalGroup Cycle = new("cycle");
    public static readonly ModalGroup Transform = new("transform");
    public static readonly ModalGroup Stop = new("stop");
    public static readonly ModalGroup NonModal = new("non-modal");
}

[SmartEnum<string>]
public sealed partial class FeedMode {
    public static readonly FeedMode UnitsPerMinute = new("units-per-minute", "G94");
    public static readonly FeedMode InverseTime = new("inverse-time", "G93");

    public string Code { get; }
}

[SmartEnum<string>]
public sealed partial class CoolingPolicy {
    public static readonly CoolingPolicy Off = new("off", static () => None);
    public static readonly CoolingPolicy Mist = new("mist", static () => Some(GCommand.CoolantMist));
    public static readonly CoolingPolicy Flood = new("flood", static () => Some(GCommand.Coolant));

    [UseDelegateFromConstructor]
    public partial Option<GCommand> Word();
}

[SmartEnum<string>]
public sealed partial class LeadStyle {
    public static readonly LeadStyle None = new("none", static _ => Option<LeadShape>.None);
    public static readonly LeadStyle Line = new("line", static radius => Some<LeadShape>(new LeadShape.Linear(radius)));
    public static readonly LeadStyle Arc = new("arc", static radius => Some<LeadShape>(new LeadShape.Tangent(radius, Math.PI / 2.0)));
    public static readonly LeadStyle Loop = new("loop", static radius => Some<LeadShape>(new LeadShape.Loop(radius)));

    [UseDelegateFromConstructor]
    public partial Option<LeadShape> Shape(double radius);
}

[SmartEnum]
public sealed partial class WordValuePolicy {
    public static readonly WordValuePolicy Literal = new(static value => value is GValue.Number or GValue.Integer);
    public static readonly WordValuePolicy Symbolic = new(static _ => true);

    [UseDelegateFromConstructor]
    public partial bool Admits(GValue value);
}

[SmartEnum<string>]
public sealed partial class MotionRole {
    public static readonly MotionRole Control = new("control");
    public static readonly MotionRole Cutting = new("cutting");
    public static readonly MotionRole Probing = new("probing");
    public static readonly MotionRole Additive = new("additive");
    public static readonly MotionRole None = new("none");
}

public sealed record CommandGrammar(Set<char> Required, Set<char> Allowed, Set<char> Repeatable, WordValuePolicy Values) {
    public Fin<Arr<GParam>> Admit(int line, Arr<GParam> parameters, ModalGroup group) {
        Seq<char> addresses = parameters.Map(static parameter => parameter.Address).ToSeq();
        bool required = Required.ForAll(addresses.Contains);
        bool allowed = addresses.ForAll(Allowed.Contains);
        bool unique = addresses.Distinct().ForAll(address => Repeatable.Contains(address) || addresses.Count(value => value == address) == 1);
        bool values = parameters.ForAll(parameter => Values.Admits(parameter.Value));
        return required && allowed && unique && values
            ? Fin.Succ(parameters)
            : Fin.Fail<Arr<GParam>>(FabricationFault.ProgramParse(line, group).ToError());
    }
}

[SmartEnum<string>]
public sealed partial class GCommand {
    internal static readonly Set<char> Axes = Set('X', 'Y', 'Z', 'A', 'B', 'C', 'U', 'V', 'W');
    private static readonly Set<char> Arc = Axes.Add('I').Add('J').Add('K').Add('R').Add('P').Add('F');
    private static readonly Set<char> Motion = Axes.Add('F').Add('S');
    private static readonly Set<char> Extrusion = Motion.Add('E');
    private static readonly CommandGrammar Empty = new(Set<char>(), Set<char>(), Set<char>(), WordValuePolicy.Literal);

    public static readonly GCommand Rapid = MotionRow("rapid", "G0", MotionRole.Control);
    public static readonly GCommand Feed = MotionRow("feed", "G1", MotionRole.Cutting);
    public static readonly GCommand ArcCw = new("arc-cw", "G2", ModalGroup.Motion,
        new CommandGrammar(Set<char>(), Arc, Set<char>(), WordValuePolicy.Symbolic), MotionRole.Cutting,
        Set<DialectFeature>(), Set<ProcessModality>(), None);
    public static readonly GCommand ArcCcw = new("arc-ccw", "G3", ModalGroup.Motion,
        new CommandGrammar(Set<char>(), Arc, Set<char>(), WordValuePolicy.Symbolic), MotionRole.Cutting,
        Set<DialectFeature>(), Set<ProcessModality>(), None);
    public static readonly GCommand Extrude = new("extrude", "G1", ModalGroup.Motion,
        new CommandGrammar(Set('E'), Extrusion, Set<char>(), WordValuePolicy.Symbolic), MotionRole.Additive,
        Set<DialectFeature>(), Set(ProcessModality.Additive), None);
    public static readonly GCommand ThreadCycle = CycleRow("thread-cycle", "G92", Set('X', 'Z', 'F'), DialectFeature.ThreadCycle);
    public static readonly GCommand Drill = CycleRow("drill", "G81", Set('Z', 'R', 'F'));
    public static readonly GCommand DrillDwell = CycleRow("drill-dwell", "G82", Set('Z', 'R', 'P', 'F'), DialectFeature.TimeDwell);
    public static readonly GCommand Peck = CycleRow("peck", "G83", Set('Z', 'R', 'Q', 'F'));
    public static readonly GCommand Tap = CycleRow("tap", "G84", Set('Z', 'R', 'F'), DialectFeature.RigidTap);
    public static readonly GCommand Bore = CycleRow("bore", "G85", Set('Z', 'R', 'F'));
    public static readonly GCommand CycleCancel = StateRow("cycle-cancel", "G80", ModalGroup.Cycle);
    public static readonly GCommand PlaneXy = StateRow("plane-xy", "G17", ModalGroup.Plane, DialectFeature.PlaneSelection);
    public static readonly GCommand PlaneZx = StateRow("plane-zx", "G18", ModalGroup.Plane, DialectFeature.PlaneSelection);
    public static readonly GCommand PlaneYz = StateRow("plane-yz", "G19", ModalGroup.Plane, DialectFeature.PlaneSelection);
    public static readonly GCommand Absolute = StateRow("absolute", "G90", ModalGroup.Distance, DialectFeature.Absolute);
    public static readonly GCommand Relative = StateRow("relative", "G91", ModalGroup.Distance, DialectFeature.Incremental);
    public static readonly GCommand ArcAbsolute = StateRow("arc-absolute", "G90.1", ModalGroup.ArcDistance, DialectFeature.Absolute);
    public static readonly GCommand ArcRelative = StateRow("arc-relative", "G91.1", ModalGroup.ArcDistance, DialectFeature.Incremental);
    public static readonly GCommand Metric = StateRow("metric", "G21", ModalGroup.Units, DialectFeature.Metric);
    public static readonly GCommand Inch = StateRow("inch", "G20", ModalGroup.Units, DialectFeature.Imperial);
    public static readonly GCommand FeedPerMinute = StateRow("feed-per-minute", "G94", ModalGroup.Feed);
    public static readonly GCommand FeedInverseTime = StateRow("feed-inverse-time", "G93", ModalGroup.Feed, DialectFeature.InverseTime);
    public static readonly GCommand Spindle = Aux("spindle", "M3", ModalGroup.Spindle, Set('S'));
    public static readonly GCommand SpindleCcw = Aux("spindle-ccw", "M4", ModalGroup.Spindle, Set('S'));
    public static readonly GCommand SpindleStop = StateRow("spindle-stop", "M5", ModalGroup.Spindle);
    public static readonly GCommand SpindleOrient = Aux("spindle-orient", "M19", ModalGroup.NonModal, Set('R', 'P'));
    public static readonly GCommand Css = Aux("css", "G96", ModalGroup.Spindle, Set('S', 'D'));
    public static readonly GCommand CssCancel = StateRow("css-cancel", "G97", ModalGroup.Spindle);
    public static readonly GCommand Coolant = StateRow("coolant", "M8", ModalGroup.Coolant);
    // M7 is mist coolant on a contact controller and torch-on on a thermal one; the modality column, never a dialect
    // identity test, separates the two rows sharing the wire code.
    public static readonly GCommand CoolantMist = new("coolant-mist", "M7", ModalGroup.Coolant, Empty, MotionRole.None,
        Set<DialectFeature>(), Set(ProcessModality.Subtractive, ProcessModality.Abrasive, ProcessModality.Erosion), None);
    public static readonly GCommand TorchOn = new("torch-on", "M07", ModalGroup.Spindle, Empty, MotionRole.None,
        Set<DialectFeature>(), Set(ProcessModality.Thermal), None);
    public static readonly GCommand CoolantOff = StateRow("coolant-off", "M9", ModalGroup.Coolant);
    public static readonly GCommand AssistGas = Aux("assist-gas", "M64", ModalGroup.Coolant, Set('S'));
    public static readonly GCommand DustCollect = Aux("dust-collect", "M65", ModalGroup.Coolant, Set('S'));
    public static readonly GCommand CompOff = StateRow("comp-off", "G40", ModalGroup.CutterComp);
    public static readonly GCommand CompLeft = Aux("comp-left", "G41", ModalGroup.CutterComp, Set('D'));
    public static readonly GCommand CompRight = Aux("comp-right", "G42", ModalGroup.CutterComp, Set('D'));
    public static readonly GCommand LengthOffset = Aux("length-offset", "G43", ModalGroup.ToolLength, Set('H', 'Z'));
    public static readonly GCommand LengthCancel = StateRow("length-cancel", "G49", ModalGroup.ToolLength);
    public static readonly GCommand Wcs = Aux("wcs", "G54", ModalGroup.Wcs, Set('P', 'A', 'R'));
    public static readonly GCommand WcsExtended = new("wcs-extended", "G54.1", ModalGroup.Wcs,
        new CommandGrammar(Set('P'), Set('P', 'A', 'R'), Set<char>(), WordValuePolicy.Symbolic), MotionRole.None,
        Set<DialectFeature>(), Set<ProcessModality>(), None);
    public static readonly GCommand SetWcs = Aux("set-wcs", "G10", ModalGroup.NonModal, Set('L', 'P', 'X', 'Y', 'Z', 'R'));
    public static readonly GCommand LocalShift = Aux("local-shift", "G52", ModalGroup.Transform, Set('X', 'Y', 'Z'));
    public static readonly GCommand Rotate = Aux("rotate", "G68", ModalGroup.Transform, Set('X', 'Y', 'R'));
    public static readonly GCommand RotateCancel = StateRow("rotate-cancel", "G69", ModalGroup.Transform);
    public static readonly GCommand Scale = Aux("scale", "G51", ModalGroup.Transform, Set('X', 'Y', 'Z', 'P'));
    public static readonly GCommand ScaleCancel = StateRow("scale-cancel", "G50", ModalGroup.Transform);
    public static readonly GCommand RetractInitial = StateRow("retract-initial", "G98", ModalGroup.Retract);
    public static readonly GCommand RetractPlane = StateRow("retract-plane", "G99", ModalGroup.Retract);
    public static readonly GCommand ExactStop = StateRow("exact-stop", "G61", ModalGroup.PathControl);
    public static readonly GCommand ExactStopCheck = StateRow("exact-stop-check", "G61.1", ModalGroup.PathControl);
    public static readonly GCommand Continuous = Aux("continuous", "G64", ModalGroup.PathControl, Set('P', 'Q'));
    public static readonly GCommand ProgramEnd = StateRow("program-end", "M30", ModalGroup.Stop);
    public static readonly GCommand Stop = StateRow("stop", "M0", ModalGroup.Stop);
    public static readonly GCommand OptionalStop = StateRow("optional-stop", "M1", ModalGroup.Stop);
    // One G4 row carries both dwell forms; `P` is the time address every dialect admits and `X`/`U` the revolution
    // addresses `DialectFeature.RevolutionDwell` gates at emission.
    public static readonly GCommand Dwell = new("dwell", "G4", ModalGroup.NonModal,
        new CommandGrammar(Set<char>(), Set('P', 'X', 'U'), Set<char>(), WordValuePolicy.Symbolic), MotionRole.None,
        Set(DialectFeature.TimeDwell), Set<ProcessModality>(), Some('P'));
    public static readonly GCommand Probe = MotionRow("probe", "G31", MotionRole.Probing, DialectFeature.Probing);
    public static readonly GCommand ProbeTowardStop = MotionRow("probe-toward-stop", "G38.2", MotionRole.Probing, DialectFeature.Probing);
    public static readonly GCommand ProbeTowardOptional = MotionRow("probe-toward-optional", "G38.3", MotionRole.Probing, DialectFeature.Probing);
    public static readonly GCommand ProbeAwayStop = MotionRow("probe-away-stop", "G38.4", MotionRole.Probing, DialectFeature.Probing);
    public static readonly GCommand ProbeAwayOptional = MotionRow("probe-away-optional", "G38.5", MotionRole.Probing, DialectFeature.Probing);
    public static readonly GCommand TorchHeight = Aux("torch-height", "THC", ModalGroup.NonModal, Set('V', 'H', 'R', 'P'));
    public static readonly GCommand HotendTemp = Aux("hotend-temp", "M104", ModalGroup.NonModal, Set('S', 'T'));
    public static readonly GCommand HotendWait = Aux("hotend-wait", "M109", ModalGroup.NonModal, Set('S', 'T'));
    public static readonly GCommand BedTemp = Aux("bed-temp", "M140", ModalGroup.NonModal, Set('S'));
    public static readonly GCommand BedWait = Aux("bed-wait", "M190", ModalGroup.NonModal, Set('S'));
    public static readonly GCommand ToolChange = Aux("tool-change", "M6", ModalGroup.NonModal, Set('T'), DialectFeature.ToolChange);

    public string Code { get; }
    public ModalGroup Group { get; }
    public CommandGrammar Grammar { get; }
    public MotionRole Role { get; }
    public Set<DialectFeature> Requires { get; }
    public Set<ProcessModality> Modalities { get; }
    public Option<char> PositiveScalarAddress { get; }

    public Fin<Arr<GParam>> Admit(int line, Arr<GParam> parameters) =>
        Grammar.Admit(line, parameters, Group).Bind(admitted => PositiveScalarAddress.ForAll(address =>
            admitted.Find(parameter => parameter.Address == address)
                .Bind(static parameter => parameter.Value.Scalar)
                .ForAll(static value => value > 0.0))
                ? Fin.Succ(admitted)
                : Fin.Fail<Arr<GParam>>(FabricationFault.ProgramParse(line, Group).ToError()));

    // Dialect admissibility is the row's own declared demand against the dialect's declared capability, so a new
    // controller is one `PostDialect` row and a new command one `Requires` set, with no roster on either side.
    public bool Admits(PostDialect dialect) =>
        Requires.ForAll(dialect.Features.Contains)
        && (Modalities.IsEmpty || Modalities.Exists(dialect.Modalities.Contains));

    private static GCommand MotionRow(string key, string code, MotionRole role, params ReadOnlySpan<DialectFeature> requires) =>
        new(key, code, ModalGroup.Motion, new CommandGrammar(Set<char>(), Motion, Set<char>(), WordValuePolicy.Symbolic),
            role, Set(requires.ToArray()), Set<ProcessModality>(), None);
    private static GCommand StateRow(string key, string code, ModalGroup group, params ReadOnlySpan<DialectFeature> requires) =>
        new(key, code, group, Empty, MotionRole.None, Set(requires.ToArray()), Set<ProcessModality>(), None);
    private static GCommand Aux(string key, string code, ModalGroup group, Set<char> allowed, params ReadOnlySpan<DialectFeature> requires) =>
        new(key, code, group, new CommandGrammar(Set<char>(), allowed, Set<char>(), WordValuePolicy.Symbolic),
            MotionRole.None, Set(requires.ToArray()), Set<ProcessModality>(), None);
    private static GCommand CycleRow(string key, string code, Set<char> required, params ReadOnlySpan<DialectFeature> requires) => new(
        key,
        code,
        ModalGroup.Cycle,
        new CommandGrammar(required, required + Axes + Set('P', 'Q', 'L'), Set<char>(), WordValuePolicy.Symbolic),
        MotionRole.None,
        Set(requires.ToArray()),
        Set<ProcessModality>(),
        None);
}

[SmartEnum<string>]
public sealed partial class ProgramView {
    public static readonly ProgramView AllMotion = new("all-motion", None);
    public static readonly ProgramView Cutting = new("cutting", Some(MotionRole.Cutting));
    public static readonly ProgramView Control = new("control", Some(MotionRole.Control));
    public static readonly ProgramView Probing = new("probing", Some(MotionRole.Probing));
    public static readonly ProgramView Additive = new("additive", Some(MotionRole.Additive));

    public Option<MotionRole> Role { get; }

    public Fin<Seq<ToolpathPath>> Paths(ProgramTrace trace) {
        (Seq<ToolpathPath> Paths, ToolpathPath? Current) folded = trace.Events.Fold(
            (Paths: Seq<ToolpathPath>(), Current: null),
            (state, item) => item switch {
                ProgramEvent.Motion motion when Role.ForAll(role => role == motion.Role) =>
                    (state.Paths, state.Current is null
                        ? new ToolpathPath(motion.From, Seq(Span(motion)))
                        : state.Current with { Spans = state.Current.Spans.Add(Span(motion)) }),
                // A coordinate change re-frames every following point, so it closes the run exactly as an excluded move does.
                ProgramEvent.Motion or ProgramEvent.Coordinate when state.Current is not null =>
                    (state.Paths.Add(state.Current), null),
                _ => state,
            });
        return Fin.Succ(folded.Current is null ? folded.Paths : folded.Paths.Add(folded.Current));
    }

    private static ToolpathSpan Span(ProgramEvent.Motion motion) => motion.Arc.Match<ToolpathSpan>(
        Some: arc => new ToolpathSpan.Arc(motion.To, arc.Center,
            arc.Sense == RotationSense.Clockwise ? ToolpathArcSense.Clockwise : ToolpathArcSense.Counterclockwise),
        None: () => new ToolpathSpan.Line(motion.To));
}

// --- [OWNERS] -----------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GValue {
    private GValue() { }

    public sealed record Number(double Canonical, string Lexeme, ProgramUnits SourceUnits) : GValue;
    public sealed record Integer(int Value, string Lexeme) : GValue;
    public sealed record Variable(int Index, string Lexeme) : GValue;
    public sealed record Expression(string Lexeme) : GValue;
    public sealed record Text(string Value) : GValue;

    public Option<double> Scalar => Switch(
        number: static value => Some(value.Canonical),
        integer: static value => Some((double)value.Value),
        variable: static _ => None,
        expression: static _ => None,
        text: static _ => None);
}

public readonly record struct GParam(char Address, GValue Value) {
    public static GParam Number(char address, double value, ProgramUnits units) =>
        new(char.ToUpperInvariant(address), new GValue.Number(value, value.ToString("R", CultureInfo.InvariantCulture), units));

    // Rounding is a rendering decision, so it lands in the source units the record emits, never in canonical millimetres.
    public GParam Round(int decimals) => Value is GValue.Number number
        ? this with { Value = number with {
            Canonical = number.SourceUnits.Canonical(Math.Round(number.SourceUnits.Native(number.Canonical), decimals)),
            Lexeme = Math.Round(number.SourceUnits.Native(number.Canonical), decimals).ToString("R", CultureInfo.InvariantCulture),
        } }
        : this;
}

public readonly record struct MotionArc(Point3d Center, RotationSense Sense);
public readonly record struct MacroSlot(int Index, string Key, GValue Value);
public readonly record struct TemperatureSet(double Hotend, double Bed);
public readonly record struct ExtrusionProfile(double Amount, double Feed);

public sealed record BlockFrame(
    Option<int> Program,
    Option<int> Sequence,
    bool Optional,
    bool Delimiter,
    Option<int> Checksum,
    Seq<string> Comments,
    string Source);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GNode {
    private GNode() { }

    public sealed record Block(BlockFrame Frame, Arr<GNode> Body) : GNode;
    public sealed record Word(GCommand Command, Arr<GParam> Words, Option<FeedMode> Mode) : GNode {
        public Option<double> P(char address) => Words.Find(parameter => parameter.Address == address).Bind(static parameter => parameter.Value.Scalar);
        public ProgramUnits SourceUnits => Words.Choose(static parameter => parameter.Value is GValue.Number number
            ? Some(number.SourceUnits) : None).HeadOrNone().IfNone(ProgramUnits.Metric);
        public Word With(char address, double value) {
            Option<ProgramUnits> held = Words.Find(parameter => parameter.Address == address)
                .Bind(static parameter => parameter.Value is GValue.Number number ? Some(number.SourceUnits) : None);
            ProgramUnits units = held.Match(
                Some: static source => source,
                None: () => SourceUnits);
            return this with {
                Words = Words.Filter(parameter => parameter.Address != address).Add(GParam.Number(address, value, units)),
            };
        }
        public Word Without(char address) => this with { Words = Words.Filter(parameter => parameter.Address != address) };
    }
    public sealed record CannedCycle(GCommand Command, Arr<GParam> SingleBlockWords, Seq<Move> ExpandedMoves, int Repeats, Option<FeedMode> Mode) : GNode {
        public Option<double> R => SingleBlockWords.Find(static word => word.Address == 'R').Bind(static word => word.Value.Scalar);
        public Option<double> Q => SingleBlockWords.Find(static word => word.Address == 'Q').Bind(static word => word.Value.Scalar);
        public Option<double> P => SingleBlockWords.Find(static word => word.Address == 'P').Bind(static word => word.Value.Scalar);
    }
    public sealed record CoordinateFrame(WcsAssignment Assignment, Plane Frame) : GNode;
    public sealed record Macro(Arr<MacroSlot> Slots, Arr<GNode> Body) : GNode;
    public sealed record Subprogram(int Label, int Repeats, Arr<GNode> Body) : GNode;
    public sealed record AdditiveLayer(int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures) : GNode;
    public sealed record Nc1(SteelImportReceipt Receipt) : GNode;
    public sealed record Directive(MotionDirective Value) : GNode;

    public FaultSubject.ProgramNode Subject => new(Switch(
        block: static _ => "block",
        word: static value => value.Command.Key,
        cannedCycle: static value => value.Command.Key,
        coordinateFrame: static _ => "coordinate-frame",
        macro: static _ => "macro",
        subprogram: static _ => "subprogram",
        additiveLayer: static _ => "additive-layer",
        nc1: static _ => "nc1",
        directive: static value => value.Value.Switch(
            spindle: static _ => "directive-spindle",
            dwell: static _ => "directive-dwell",
            synchronize: static _ => "directive-synchronize",
            orientedStop: static _ => "directive-oriented-stop",
            channelBarrier: static _ => "directive-channel-barrier",
            specialized: static value => $"directive-{value.Payload.Kind.Key}")));

    public static GNode Move(Move move, Point3d from) => move.Switch(
        state: from,
        rapid: static (_, row) => new Word(GCommand.Rapid, Coordinates(row.Target), None),
        linear: static (_, row) => new Word(GCommand.Feed, Coordinates(row.Target).Add(GParam.Number('F', row.Feed, ProgramUnits.Metric)), None),
        circular: static (start, row) => new Word(
            row.Arc.Sense == RotationSense.Clockwise ? GCommand.ArcCw : GCommand.ArcCcw,
            Coordinates(row.Target).Add(GParam.Number('I', row.Arc.Center.X - start.X, ProgramUnits.Metric))
                .Add(GParam.Number('J', row.Arc.Center.Y - start.Y, ProgramUnits.Metric))
                .Add(GParam.Number('F', row.Feed, ProgramUnits.Metric)), None));

    public static Seq<GNode> Moves(Seq<Move> moves, Point3d origin) =>
        moves.Fold((Nodes: Seq<GNode>(), Cursor: origin), static (state, move) =>
            (state.Nodes.Add(Move(move, state.Cursor)), Target(move))).Nodes;

    public static Seq<GNode> Moves(Seq<Move> moves, Seq<MotionDirective> directives, Point3d origin) =>
        moves.Map((move, index) => (move, index)).Fold(
            (Nodes: directives.Filter(static row => row.AfterMove < 0).Map(static row => (GNode)new Directive(row)), Cursor: origin),
            static (state, item) => (
                state.Nodes.Add(Move(item.move, state.Cursor))
                    .Concat(directives.Filter(row => row.AfterMove == item.index).Map(static row => (GNode)new Directive(row))),
                Target(item.move))).Nodes;

    public static Point3d Target(Move move) => move.Switch(
        rapid: static row => row.Target,
        linear: static row => row.Target,
        circular: static row => row.Target);

    private static Arr<GParam> Coordinates(Point3d point) => Arr(
        GParam.Number('X', point.X, ProgramUnits.Metric),
        GParam.Number('Y', point.Y, ProgramUnits.Metric),
        GParam.Number('Z', point.Z, ProgramUnits.Metric));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GWord {
    private GWord() { }

    public sealed record Address(string Code, ModalGroup Group, Arr<GParam> Words, Option<FeedMode> Mode, WordRetention Retention) : GWord;
    public sealed record Conversational(Seq<string> Records) : GWord;
    public sealed record Text(Seq<string> Records) : GWord;
    public sealed record CycleCall(Seq<string> Records) : GWord;
    public sealed record Macro(Seq<string> Open, Seq<GWord> Body, Seq<string> Close) : GWord;
    public sealed record Subprogram(Seq<string> Open, Seq<GWord> Body, Seq<string> Close) : GWord;
    public sealed record Additive(Seq<string> Records) : GWord;
    public sealed record Nc1(Seq<string> Records, ContentKey Key) : GWord;
    public sealed record Fault(Error Error) : GWord;
    public sealed record Expanded(Seq<GWord> Words) : GWord;

    public static Fin<RenderReceipt> Render(Seq<GWord> words) =>
        Render(words, RenderReceipt.Empty);

    private static Fin<RenderReceipt> Render(Seq<GWord> words, RenderReceipt state) =>
        words.FoldM<Fin, RenderReceipt>(state, static (current, word) => RenderWord(word, current)).As();

    private static Fin<RenderReceipt> RenderWord(GWord word, RenderReceipt state) => word.Switch(
        state: state,
        address: static (current, value) => Fin.Succ(AddressRecords(current, value)),
        conversational: static (current, value) => Fin.Succ(current.Add(value.Records)),
        text: static (current, value) => Fin.Succ(current.Add(value.Records)),
        cycleCall: static (current, value) => Fin.Succ(current.Add(value.Records)),
        macro: static (current, value) => Render(value.Body, current.Add(value.Open)).Map(rendered => rendered.Add(value.Close)),
        subprogram: static (current, value) => Render(value.Body, current.Add(value.Open)).Map(rendered => rendered.Add(value.Close)),
        additive: static (current, value) => Fin.Succ(current.Add(value.Records)),
        nc1: static (current, value) => Fin.Succ(current.Add(value.Records)),
        fault: static (_, value) => Fin.Fail<RenderReceipt>(value.Error),
        expanded: static (current, value) => Render(value.Words, current));

    private static RenderReceipt AddressRecords(RenderReceipt state, Address word) {
        bool emitMode = word.Mode.Exists(mode => word.Retention == WordRetention.Explicit
            || !state.Active.Find(ModalGroup.Feed).Contains(mode.Code));
        Map<ModalGroup, string> withMode = word.Mode.Map(mode => state.Active.AddOrUpdate(ModalGroup.Feed, mode.Code)).IfNone(state.Active);
        bool emitCode = word.Group == ModalGroup.NonModal || word.Retention == WordRetention.Explicit || !withMode.Find(word.Group).Contains(word.Code);
        string line = string.Join(" ", Seq(emitMode ? word.Mode.Map(static mode => mode.Code).IfNone(string.Empty) : string.Empty,
                emitCode ? word.Code : string.Empty)
            .Concat(word.Words.Map(Format)).Filter(static token => token.Length > 0).ToArray());
        Map<ModalGroup, string> next = word.Group == ModalGroup.NonModal ? withMode : withMode.AddOrUpdate(word.Group, word.Code);
        return line.Length == 0 ? state with { Active = next } : new RenderReceipt(state.Lines.Add(line), next);
    }

    private static string Format(GParam parameter) => $"{parameter.Address}{Value(parameter.Value)}";
    private static string Value(GValue value) => value.Switch(
        number: static item => item.SourceUnits.Native(item.Canonical).ToString("R", CultureInfo.InvariantCulture),
        integer: static item => item.Value.ToString(CultureInfo.InvariantCulture),
        variable: static item => item.Lexeme,
        expression: static item => item.Lexeme,
        text: static item => item.Value);
}

public sealed record RenderReceipt(Seq<string> Lines, Map<ModalGroup, string> Active) {
    public static readonly RenderReceipt Empty = new(Seq<string>(), Map<ModalGroup, string>());
    public RenderReceipt Add(string line) => line.Length == 0 ? this : this with { Lines = Lines.Add(line) };
    public RenderReceipt Add(Seq<string> lines) => this with { Lines = Lines.Concat(lines.Filter(static line => line.Length > 0)) };
}

public readonly record struct ProgramPathStep(int Node, Option<int> Repeat);

public sealed record ProgramLocus(int Block, Seq<ProgramPathStep> Path) {
    public static ProgramLocus Root(int block, int node) => new(block, Seq(new ProgramPathStep(node, None)));
    public ProgramLocus Descend(int node) => this with { Path = Path.Add(new ProgramPathStep(node, None)) };
    public ProgramLocus Repeated(int node, int repeat) => this with { Path = Path.Add(new ProgramPathStep(node, Some(repeat))) };
    public Seq<int> Source => Path.Map(static step => step.Node);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramEvent(ProgramLocus Locus) {

    public sealed record Motion(
        ProgramLocus Locus,
        GNode.Word Word,
        Point3d From,
        Point3d To,
        Map<char, double> FromAxes,
        Map<char, double> ToAxes,
        ProgramUnits Units,
        double Feed,
        FeedMode Mode,
        MotionRole Role,
        GCommand Plane,
        Option<MotionArc> Arc) : ProgramEvent(Locus) {
        public bool Cutting => Role == MotionRole.Cutting;
    }
    public sealed record State(ProgramLocus Locus, GNode.Word Word) : ProgramEvent(Locus) {
        public GCommand Command => Word.Command;
    }
    public sealed record Boundary(ProgramLocus Locus, BlockFrame Frame) : ProgramEvent(Locus);
    public sealed record Coordinate(ProgramLocus Locus, WcsAssignment Assignment, Plane Frame) : ProgramEvent(Locus);
    public sealed record Additive(ProgramLocus Locus, int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures) : ProgramEvent(Locus);
    public sealed record Exchange(ProgramLocus Locus, SteelImportReceipt Receipt) : ProgramEvent(Locus);
    public sealed record Directive(ProgramLocus Locus, MotionDirective Value) : ProgramEvent(Locus);
}

public sealed record ProgramTrace {
    private ProgramTrace(ModalState final) => Final = final;

    public ModalState Final { get; }
    public Seq<ProgramEvent> Events => Final.Events;

    internal static Fin<ProgramTrace> Admit(CutProgram program) => program.Nodes.IsEmpty
        ? Fin.Fail<ProgramTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:trace-empty").ToError())
        : program.Nodes.Map((node, block) => (node, block)).FoldM<Fin, ModalState>(ModalState.Empty,
            static (state, item) => state.Push(ProgramLocus.Root(item.block, item.block), item.node)).As()
            .Map(static state => new ProgramTrace(state));
}

public sealed record ModalState(
    ProgramUnits Units,
    DistanceMode Distance,
    DistanceMode ArcDistance,
    GCommand Plane,
    Point3d Position,
    Map<char, double> Axes,
    double Feed,
    FeedMode Mode,
    Map<ModalGroup, GCommand> Active,
    Seq<ProgramEvent> Events) {
    // RS274 defaults arc-center offsets to incremental; the plane and arc-distance rows are modal state every
    // consumer reads from the trace rather than re-deriving from the event stream.
    public static readonly ModalState Empty = new(ProgramUnits.Metric, DistanceMode.Absolute, DistanceMode.Incremental,
        GCommand.PlaneXy, Point3d.Origin, OriginAxes(), 0.0, FeedMode.UnitsPerMinute,
        Map<ModalGroup, GCommand>(), Seq<ProgramEvent>());

    public Fin<ModalState> Push(ProgramLocus locus, GNode node) => node.Switch(
        state: (State: this, Locus: locus),
        block: static (context, value) => PushBlock(context.State, value, context.Locus),
        word: static (context, value) => PushWord(context.State, value, context.Locus),
        cannedCycle: static (context, value) => PushCycle(context.State, value, context.Locus),
        coordinateFrame: static (context, value) => Fin.Succ(context.State with {
            Events = context.State.Events.Add(new ProgramEvent.Coordinate(context.Locus, value.Assignment, value.Frame)),
        }),
        macro: static (context, value) => value.Body.Map((item, index) => (Item: item, Index: index)).ToSeq()
            .FoldM<Fin, ModalState>(context.State, (state, row) => state.Push(context.Locus.Descend(row.Index), row.Item)).As(),
        subprogram: static (context, value) =>
            from _ in value.Label > 0 && value.Repeats > 0
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(FabricationFault.ProgramParse(context.Locus.Block, ModalGroup.NonModal).ToError())
            from expanded in Range(0, value.Repeats).FoldM<Fin, ModalState>(context.State,
                (state, repeat) => value.Body.Map((item, index) => (Item: item, Index: index)).ToSeq()
                    .FoldM<Fin, ModalState>(state, (nested, row) => nested.Push(
                        context.Locus.Repeated(row.Index, repeat), row.Item)).As()).As()
            select expanded,
        additiveLayer: static (context, value) => Fin.Succ(context.State with {
            Events = context.State.Events.Add(new ProgramEvent.Additive(
                context.Locus, value.Layer, value.Extrusion, value.Temperatures)),
        }),
        nc1: static (context, value) => Fin.Succ(context.State with { Events = context.State.Events.Add(new ProgramEvent.Exchange(context.Locus, value.Receipt)) }),
        directive: static (context, value) => Fin.Succ(context.State with { Events = context.State.Events.Add(new ProgramEvent.Directive(context.Locus, value.Value)) }));

    private static Fin<ModalState> PushCycle(ModalState state, GNode.CannedCycle value, ProgramLocus locus) =>
        from _ in value.Repeats > 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.ProgramParse(locus.Block, ModalGroup.Cycle).ToError())
        from admitted in value.Command.Admit(locus.Block, value.SingleBlockWords)
        from expanded in Range(0, value.Repeats).FoldM<Fin, ModalState>(state, (cycle, repeat) =>
            value.ExpandedMoves.IsEmpty
                ? Apply(cycle, new GNode.Word(value.Command, admitted, value.Mode), locus.Repeated(0, repeat))
                : value.ExpandedMoves.Map((move, index) => (Move: move, Index: index))
                    .FoldM<Fin, ModalState>(cycle, (current, row) => GNode.Move(row.Move, current.Position) is GNode.Word word
                        ? PushWord(current, word with { Mode = value.Mode }, locus.Repeated(row.Index, repeat))
                        : Fin.Fail<ModalState>(FabricationFault.ProgramParse(locus.Block, ModalGroup.Cycle).ToError())).As()).As()
        select expanded;

    private static Fin<ModalState> PushBlock(ModalState state, GNode.Block value, ProgramLocus locus) {
        return AdmitBlock(locus.Block, value.Body).Bind(_ =>
            value.Body.Map((item, index) => (Item: item, Index: index)).ToSeq().FoldM<Fin, ModalState>(state with {
                Events = state.Events.Add(new ProgramEvent.Boundary(locus, value.Frame)),
            }, (current, item) => current.Push(locus.Descend(item.Index), item.Item)).As());
    }

    internal static Fin<Unit> AdmitBlock(int block, Arr<GNode> body) {
        Seq<ModalGroup> groups = body.Choose(static node => node switch {
            GNode.Word { Command.Group: var group } when group != ModalGroup.NonModal => Some(group),
            GNode.CannedCycle { Command.Group: var group } when group != ModalGroup.NonModal => Some(group),
            _ => None,
        }).ToSeq();
        return groups.Distinct().Exists(group => groups.Count(candidate => candidate == group) > 1)
            ? Fin.Fail<Unit>(FabricationFault.ProgramParse(block, groups.Find(group => groups.Count(candidate => candidate == group) > 1).IfNone(ModalGroup.NonModal)).ToError())
            : Fin.Succ(unit);
    }

    private static Fin<ModalState> PushWord(ModalState state, GNode.Word word, ProgramLocus locus) =>
        word.Command.Admit(locus.Block, word.Words).Bind(_ => Apply(state, word, locus));

    private static Fin<ModalState> Apply(ModalState state, GNode.Word word, ProgramLocus locus) {
        ProgramUnits units = word.Command == GCommand.Metric ? ProgramUnits.Metric
            : word.Command == GCommand.Inch ? ProgramUnits.Imperial : state.Units;
        DistanceMode distance = word.Command == GCommand.Absolute ? DistanceMode.Absolute
            : word.Command == GCommand.Relative ? DistanceMode.Incremental : state.Distance;
        DistanceMode arcDistance = word.Command == GCommand.ArcAbsolute ? DistanceMode.Absolute
            : word.Command == GCommand.ArcRelative ? DistanceMode.Incremental : state.ArcDistance;
        GCommand plane = word.Command.Group == ModalGroup.Plane ? word.Command : state.Plane;
        double feed = word.P('F').IfNone(state.Feed);
        FeedMode feedMode = word.Mode.IfNone(state.Mode);
        Map<char, double> targetAxes = Target(state, word, distance);
        Point3d target = new(
            targetAxes.Find('X').IfNone(0.0),
            targetAxes.Find('Y').IfNone(0.0),
            targetAxes.Find('Z').IfNone(0.0));
        bool motion = word.Command.Group == ModalGroup.Motion;
        Map<ModalGroup, GCommand> active = word.Command.Group == ModalGroup.NonModal
            ? state.Active : state.Active.AddOrUpdate(word.Command.Group, word.Command);
        return ArcOf(state.Position, target, word, plane, arcDistance, locus.Block).Map(arc => {
            Seq<ProgramEvent> events = motion
                ? state.Events.Add(new ProgramEvent.Motion(locus, word, state.Position, target, state.Axes, targetAxes,
                    units, feed, feedMode, word.Command.Role, plane, arc))
                : state.Events.Add(new ProgramEvent.State(locus, word));
            return new ModalState(units, distance, arcDistance, plane, motion ? target : state.Position,
                motion ? targetAxes : state.Axes, feed, feedMode, active, events);
        });
    }

    private static Map<char, double> Target(ModalState state, GNode.Word word, DistanceMode distance) =>
        GCommand.Axes.Fold(state.Axes, (axes, address) => {
            double held = axes.Find(address).IfNone(0.0);
            double target = word.P(address)
                .Map(value => distance == DistanceMode.Absolute ? value : held + value)
                .IfNone(held);
            return axes.AddOrUpdate(address, target);
        });

    private static Map<char, double> OriginAxes() => GCommand.Axes.Fold(
        Map<char, double>(), static (axes, address) => axes.AddOrUpdate(address, 0.0));

    // Arc center resolves once where plane and arc-distance rows are in hand; an event carrying only
    // endpoints forces every consumer to re-derive it or publish a chord in the arc's place.
    private static Fin<Option<MotionArc>> ArcOf(
        Point3d start, Point3d target, GNode.Word word, GCommand plane, DistanceMode arcDistance, int block) {
        if (word.Command != GCommand.ArcCw && word.Command != GCommand.ArcCcw)
            return Fin.Succ(Option<MotionArc>.None);
        RotationSense sense = word.Command == GCommand.ArcCw
            ? RotationSense.Clockwise
            : RotationSense.Counterclockwise;
        bool radius = word.Words.Exists(static parameter => parameter.Address == 'R');
        bool center = word.Words.Exists(static parameter => parameter.Address is 'I' or 'J' or 'K');
        if (radius == center)
            return Fin.Fail<Option<MotionArc>>(FabricationFault.ProgramParse(block, ModalGroup.Motion).ToError());
        return radius
            ? word.P('R').ToFin(FabricationFault.ProgramParse(block, ModalGroup.Motion).ToError())
                .Bind(value => RadiusCenter(start, target, plane, value, sense)
                    .Map<Option<MotionArc>>(resolved => Some(new MotionArc(resolved, sense))))
            : word.Words.Filter(static parameter => parameter.Address is 'I' or 'J' or 'K')
                .ForAll(static parameter => parameter.Value.Scalar.IsSome)
                ? Fin.Succ(Some(new MotionArc(ArcCenter(start, word, plane, arcDistance), sense)))
                : Fin.Fail<Option<MotionArc>>(FabricationFault.ProgramParse(block, ModalGroup.Motion).ToError());
    }

    private static Fin<Point3d> RadiusCenter(
        Point3d start, Point3d target, GCommand plane, double signedRadius, RotationSense sense) {
        (double StartU, double StartV, double TargetU, double TargetV) = plane == GCommand.PlaneZx
            ? (start.Z, start.X, target.Z, target.X)
            : plane == GCommand.PlaneYz
                ? (start.Y, start.Z, target.Y, target.Z)
                : (start.X, start.Y, target.X, target.Y);
        double deltaU = TargetU - StartU;
        double deltaV = TargetV - StartV;
        double chord = Math.Sqrt((deltaU * deltaU) + (deltaV * deltaV));
        double radius = Math.Abs(signedRadius);
        if (!double.IsFinite(signedRadius) || signedRadius == 0.0 || !double.IsFinite(chord)
            || chord == 0.0 || radius < chord / 2.0)
            return Fin.Fail<Point3d>(Error.New("post:arc-radius"));
        double height = Math.Sqrt(Math.Max(0.0, (radius * radius) - ((chord * chord) / 4.0)));
        double side = sense == RotationSense.Counterclockwise ? 1.0 : -1.0;
        if (signedRadius < 0.0) side = -side;
        double centerU = (StartU + TargetU) / 2.0 - (side * deltaV * height / chord);
        double centerV = (StartV + TargetV) / 2.0 + (side * deltaU * height / chord);
        Point3d center = plane == GCommand.PlaneZx
            ? new Point3d(centerV, start.Y, centerU)
            : plane == GCommand.PlaneYz
                ? new Point3d(start.X, centerU, centerV)
                : new Point3d(centerU, centerV, start.Z);
        return Fin.Succ(center);
    }

    private static Point3d ArcCenter(Point3d start, GNode.Word word, GCommand plane, DistanceMode arcDistance) {
        double Offset(char address, double held) => word.P(address)
            .Map(value => arcDistance == DistanceMode.Absolute ? value : held + value).IfNone(held);
        return plane == GCommand.PlaneZx ? new Point3d(Offset('I', start.X), start.Y, Offset('K', start.Z))
            : plane == GCommand.PlaneYz ? new Point3d(start.X, Offset('J', start.Y), Offset('K', start.Z))
            : new Point3d(Offset('I', start.X), Offset('J', start.Y), start.Z);
    }
}

public sealed record CutProgram(Seq<GNode> Nodes, PostDialect Dialect, ContentKey Key) {
    public static CutProgram Of(Seq<GNode> nodes, PostDialect dialect) =>
        new(nodes, dialect, ContentKey.Of(EgressKind.CutProgram, Canonical(nodes, dialect)));

    public static byte[] Canonical(Seq<GNode> nodes, PostDialect dialect) =>
        Encoding.UTF8.GetBytes(Frame(dialect.Key, Frame(nodes.Map(NodeKey).ToArray())));

    private static string NodeKey(GNode node) => node.Switch(
        block: static value => Frame("block", BlockKey(value.Frame), Frame(value.Body.Map(NodeKey).ToArray())),
        word: static value => Frame("word", value.Command.Key, value.Mode.Map(static mode => mode.Key).IfNone(string.Empty), Frame(value.Words.Map(ParamKey).ToArray())),
        cannedCycle: static value => Frame("cycle", value.Command.Key, value.Repeats.ToString(CultureInfo.InvariantCulture),
            value.Mode.Map(static mode => mode.Key).IfNone(string.Empty), Frame(value.SingleBlockWords.Map(ParamKey).ToArray()), Frame(value.ExpandedMoves.Map(MoveKey).ToArray())),
        coordinateFrame: static value => Frame("coordinate-frame", value.Assignment.Setup.ToString(CultureInfo.InvariantCulture),
            WcsKey(value.Assignment.Slot), PlaneKey(value.Frame)),
        macro: static value => Frame("macro", Frame(value.Slots.Map(slot => Frame(slot.Index.ToString(CultureInfo.InvariantCulture), slot.Key, ValueKey(slot.Value))).ToArray()), Frame(value.Body.Map(NodeKey).ToArray())),
        subprogram: static value => Frame("subprogram", value.Label.ToString(CultureInfo.InvariantCulture), value.Repeats.ToString(CultureInfo.InvariantCulture), Frame(value.Body.Map(NodeKey).ToArray())),
        additiveLayer: static value => Frame("additive", value.Layer.ToString(CultureInfo.InvariantCulture), Number(value.Extrusion.Amount), Number(value.Extrusion.Feed), Number(value.Temperatures.Hotend), Number(value.Temperatures.Bed)),
        nc1: static value => Frame("nc1", value.Receipt.Key.Kind.Key, value.Receipt.Key.Digest.ToString("x32", CultureInfo.InvariantCulture)),
        directive: static value => DirectiveKey(value.Value));

    private static string DirectiveKey(MotionDirective directive) => directive.Switch(
        spindle: static value => Frame("spindle", value.Control.Key, Number(value.SurfaceMetersPerMinute), Number(value.ResolvedRpm)),
        dwell: static value => Frame("dwell", value.AfterMove.ToString(CultureInfo.InvariantCulture), Number(value.Revolutions)),
        synchronize: static value => Frame("synchronize", value.FromMove.ToString(CultureInfo.InvariantCulture), value.ToMove.ToString(CultureInfo.InvariantCulture), Number(value.Rpm), Number(value.Lead), value.Hand.Key),
        orientedStop: static value => Frame("oriented-stop", value.AfterMove.ToString(CultureInfo.InvariantCulture), VectorKey(value.Retract)),
        channelBarrier: static value => Frame("channel-barrier", value.Step.ToString(CultureInfo.InvariantCulture), value.Channel, Frame(value.WaitFor.ToArray()), value.Signal.IfNone(string.Empty)),
        specialized: static value => Frame("specialized", value.AfterMove.ToString(CultureInfo.InvariantCulture),
            value.Payload.Kind.Key, Number(value.Payload.DurationSeconds), Frame(value.Payload.Rows.Map(SpecializedKey).ToArray())));

    private static string SpecializedKey(SpecializedToolpathRow row) => row.Switch(
        wire: static value => Frame("wire", value.Pass.ToString(CultureInfo.InvariantCulture), Number(value.Station),
            Number(value.Progress), Number(value.TraversedMm), PointKey(value.Lower), PointKey(value.Upper), value.Action,
            Number(value.LagMm), Number(value.UpperCornerRadiusMm), value.RotaryDeg.Map(Number).IfNone(string.Empty)),
        bevel: static value => Frame("bevel", value.Move.ToString(CultureInfo.InvariantCulture), value.Pass.ToString(CultureInfo.InvariantCulture),
            Number(value.Station), value.SourceSpan.ToString(CultureInfo.InvariantCulture), Number(value.SourceBulge), PointKey(value.Point),
            VectorKey(value.ToolAxis), PointKey(value.Pivot), Number(value.AngleDeg), Number(value.CrossTiltDeg),
            Number(value.FeedMmPerMin), Number(value.CompensationMm)),
        link: static value => Frame("link", value.From, value.To, value.Transition, Number(value.DistanceMm),
            Number(value.DurationSeconds), Number(value.LiftMm), Number(value.ThermalExposure), Number(value.RotationPenalty),
            value.Retracts.ToString(CultureInfo.InvariantCulture), value.Pierces.ToString(CultureInfo.InvariantCulture),
            value.ToolChanges.ToString(CultureInfo.InvariantCulture), value.SetupChanges.ToString(CultureInfo.InvariantCulture)),
        inspection: static value => Frame("inspection", value.Pass.ToString(CultureInfo.InvariantCulture),
            value.FromBlock.ToString(CultureInfo.InvariantCulture), value.ToBlockExclusive.ToString(CultureInfo.InvariantCulture),
            Number(value.NominalAngleDeg), Number(value.NominalOffsetMm), Number(value.AngleDeviationDeg),
            Number(value.OffsetDeviationMm), value.Conforming ? "1" : "0"),
        turningThread: static value => Frame("turning-thread", value.Form, Number(value.LoadFlankDeg),
            Number(value.ClearanceFlankDeg), Number(value.CrestFlat), Number(value.RootFlat),
            Number(value.CrestRadius), Number(value.RootRadius), value.Side),
        turningAxial: static value => Frame("turning-axial", value.FromMove.ToString(CultureInfo.InvariantCulture),
            value.ToMove.ToString(CultureInfo.InvariantCulture), value.Kind, Number(value.Diameter),
            Number(value.Depth), Number(value.TipAngleDeg)),
        turningTap: static value => Frame("turning-tap", value.FromMove.ToString(CultureInfo.InvariantCulture),
            value.ToMove.ToString(CultureInfo.InvariantCulture), Number(value.Diameter), Number(value.Depth),
            Number(value.Pitch), value.Form, value.Hand),
        turningKnurl: static value => Frame("turning-knurl", value.FromMove.ToString(CultureInfo.InvariantCulture),
            value.ToMove.ToString(CultureInfo.InvariantCulture), value.Pattern, Number(value.Pressure)),
        turningHandoff: static value => Frame("turning-handoff", value.Kind, value.From, value.To,
            Number(value.GripPlane), Number(value.GripLength), Number(value.PullDistance)));

    private static string BlockKey(BlockFrame frame) => Frame(
        frame.Program.Map(static value => value.ToString(CultureInfo.InvariantCulture)).IfNone(string.Empty),
        frame.Sequence.Map(static value => value.ToString(CultureInfo.InvariantCulture)).IfNone(string.Empty),
        frame.Optional ? "1" : "0",
        frame.Delimiter ? "1" : "0",
        frame.Checksum.Map(static value => value.ToString(CultureInfo.InvariantCulture)).IfNone(string.Empty),
        Frame(frame.Comments.ToArray()), frame.Source);

    private static string ParamKey(GParam parameter) => Frame(parameter.Address.ToString(), ValueKey(parameter.Value));
    private static string ValueKey(GValue value) => value.Switch(
        number: static item => Frame("number", Number(item.Canonical), item.Lexeme, item.SourceUnits.Key),
        integer: static item => Frame("integer", item.Value.ToString(CultureInfo.InvariantCulture), item.Lexeme),
        variable: static item => Frame("variable", item.Index.ToString(CultureInfo.InvariantCulture), item.Lexeme),
        expression: static item => Frame("expression", item.Lexeme),
        text: static item => Frame("text", item.Value));
    private static string MoveKey(Move move) => move.Switch(
        rapid: static value => Frame("rapid", PointKey(value.Target)),
        linear: static value => Frame("linear", PointKey(value.Target), Number(value.Feed)),
        circular: static value => Frame("circular", PointKey(value.Target), Number(value.Feed), PointKey(value.Arc.Center), value.Arc.Sense.Key));
    private static string PointKey(Point3d point) => Frame(Number(point.X), Number(point.Y), Number(point.Z));
    private static string PlaneKey(Plane plane) => Frame(PointKey(plane.Origin), VectorKey(plane.XAxis), VectorKey(plane.YAxis));
    private static string VectorKey(Vector3d vector) => Frame(Number(vector.X), Number(vector.Y), Number(vector.Z));
    private static string WcsKey(WcsSlot slot) => slot.Switch(
        @base: static value => Frame("base", value.Ordinal.ToString(CultureInfo.InvariantCulture)),
        extended: static value => Frame("extended", value.Ordinal.ToString(CultureInfo.InvariantCulture)),
        dynamic: static value => Frame("dynamic", value.Ordinal.ToString(CultureInfo.InvariantCulture)),
        rotary: static value => Frame("rotary", value.Ordinal.ToString(CultureInfo.InvariantCulture), Number(value.Axis)),
        local: static value => Frame("local", value.Ordinal.ToString(CultureInfo.InvariantCulture), value.Parent.ToString(CultureInfo.InvariantCulture)));
    private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    private static string Frame(params string[] fields) => string.Concat(fields.Select(field => $"{Encoding.UTF8.GetByteCount(field).ToString(CultureInfo.InvariantCulture)}:{field}"));
}

// --- [BOUNDARY_OWNERS] -------------------------------------------------------------------------------------------------------------------------------
public sealed record CutRaw(
    string Kerf,
    LeadStyle Lead,
    string LeadRadius,
    string TabWidth,
    string TabSpacing,
    string Pierce,
    Option<string> Assist,
    string FeedCeiling,
    double LinkFeedFactor);
public sealed record FitRaw(string Tolerance, string MinimumRun, string SplitDistance, int ProbeFloor);
public sealed record CompRaw(string ToolDiameter, string CutWidth, string Stickout, double Modulus, double ThermalCoefficient, double TemperatureDelta);

[ComplexValueObject]
public sealed partial class CutPolicy {
    public double KerfMm { get; }
    public LeadStyle Lead { get; }
    public double LeadRadiusMm { get; }
    public double TabWidthMm { get; }
    public double TabSpacingMm { get; }
    public double PierceSeconds { get; }
    public Option<double> AssistBar { get; }
    public double FeedMmPerMinute { get; }
    public double LinkFeedFactor { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double kerfMm, ref LeadStyle lead,
        ref double leadRadiusMm, ref double tabWidthMm, ref double tabSpacingMm, ref double pierceSeconds,
        ref Option<double> assistBar, ref double feedMmPerMinute, ref double linkFeedFactor) => validationError =
        !Seq(kerfMm, leadRadiusMm, tabWidthMm, tabSpacingMm, pierceSeconds, feedMmPerMinute, linkFeedFactor).ForAll(double.IsFinite)
        || assistBar.Exists(static value => !double.IsFinite(value) || value <= 0.0)
        || kerfMm < 0.0 || pierceSeconds < 0.0 || feedMmPerMinute <= 0.0
        || linkFeedFactor <= 0.0 || linkFeedFactor > 1.0
        || (lead != LeadStyle.None && leadRadiusMm <= 0.0)
        || tabWidthMm < 0.0 || tabSpacingMm < 0.0 || (tabWidthMm > 0.0 && tabSpacingMm <= tabWidthMm)
            ? new ValidationError(message: "post-cut-policy") : null;

    public static Fin<CutPolicy> Admit(CutRaw raw) {
        ArgumentNullException.ThrowIfNull(raw);

        return (PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.Kerf)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.LeadRadius)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.TabWidth)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.TabSpacing)),
         PostPolicy.Slot(PostPolicy.DurationOf(raw.Pierce, "post:pierce")),
         PostPolicy.Slot(raw.Assist.TraverseM(source => PhysicsQuantity.Pressure.Admit(source)).As()),
         PostPolicy.Slot(PhysicsQuantity.Feed.Admit(raw.FeedCeiling)))
        .Apply((kerf, lead, tabWidth, tabSpacing, pierce, assist, feed) =>
            PostPolicy.Rail(Validate(kerf, raw.Lead, lead, tabWidth, tabSpacing, pierce, assist, feed,
                raw.LinkFeedFactor, out CutPolicy policy), policy))
        .As().ToFin().Bind(static value => value);
    }
}

[ComplexValueObject]
public sealed partial class FitPolicy {
    public double ToleranceMm { get; }
    public double MinimumRunMm { get; }
    public double SplitDistanceMm { get; }
    public int ProbeFloor { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double toleranceMm,
        ref double minimumRunMm, ref double splitDistanceMm, ref int probeFloor) => validationError =
        !Seq(toleranceMm, minimumRunMm, splitDistanceMm).ForAll(double.IsFinite)
        || toleranceMm <= 0.0 || minimumRunMm <= 0.0 || splitDistanceMm <= 0.0 || probeFloor < 3
            ? new ValidationError(message: "post-fit-policy") : null;

    public static Fin<FitPolicy> Admit(FitRaw raw) {
        ArgumentNullException.ThrowIfNull(raw);

        return (PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.Tolerance)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.MinimumRun)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.SplitDistance)))
        .Apply((tolerance, run, split) =>
            PostPolicy.Rail(Validate(tolerance, run, split, raw.ProbeFloor, out FitPolicy policy), policy))
        .As().ToFin().Bind(static value => value);
    }
}

[ComplexValueObject]
public sealed partial class CompPolicy {
    public double ToolDiameterMm { get; }
    public double CutWidthMm { get; }
    public double StickoutMm { get; }
    public double Modulus { get; }
    public double ThermalCoefficient { get; }
    public double TemperatureDelta { get; }
    public double Stiffness => 3.0 * Modulus * (Math.PI * Math.Pow(ToolDiameterMm, 4.0) / 64.0) / Math.Pow(StickoutMm, 3.0);
    public double Deflection(double radialForce) => radialForce / Stiffness;
    public double ThermalGrowth => ThermalCoefficient * StickoutMm * TemperatureDelta;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double toolDiameterMm,
        ref double cutWidthMm, ref double stickoutMm, ref double modulus, ref double thermalCoefficient,
        ref double temperatureDelta) => validationError =
        !Seq(toolDiameterMm, cutWidthMm, stickoutMm, modulus, thermalCoefficient, temperatureDelta).ForAll(double.IsFinite)
        || toolDiameterMm <= 0.0 || cutWidthMm <= 0.0 || stickoutMm <= 0.0 || modulus <= 0.0
        || thermalCoefficient < 0.0
            ? new ValidationError(message: "post-comp-policy") : null;

    public static Fin<CompPolicy> Admit(CompRaw raw) {
        ArgumentNullException.ThrowIfNull(raw);

        return (PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.ToolDiameter)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.CutWidth)),
         PostPolicy.Slot(PhysicsQuantity.Length.Admit(raw.Stickout)))
        .Apply((diameter, width, stickout) => PostPolicy.Rail(Validate(diameter, width, stickout, raw.Modulus,
            raw.ThermalCoefficient, raw.TemperatureDelta, out CompPolicy policy), policy))
        .As().ToFin().Bind(static value => value);
    }
}

public sealed record CutConditioning(
    Option<CutPolicy> Cut,
    Option<FitPolicy> Fit,
    MotionDynamics Dynamics,
    Option<CuttingData> Cutting,
    Option<CompPolicy> Compensation,
    CoolingPolicy Cooling,
    Seq<ChainRow> Chains,
    HashMap<int, Loop> Profiles);

public sealed record ProgramTooling(SlotMap Slots, Seq<WorkItem> Work, MagazinePolicy Policy, Seq<OperationBoundary> Boundaries);
public sealed record WorkholdingPlan(Fixture Fixture, FixtureState State);
public sealed record ProgramSetup(SetupPlan Schedule, WorkholdingPlan Workholding);

public sealed record PostRaw(CutConditioningRaw Cut, ProgramTooling Tooling, ProgramSetup Setup, EmitPolicy Emit);
public sealed record CutConditioningRaw(
    Option<CutRaw> Cut,
    Option<FitRaw> Fit,
    MotionDynamics Dynamics,
    Option<CuttingData> Cutting,
    Option<CompRaw> Compensation,
    CoolingPolicy Cooling,
    Seq<ChainRow> Chains,
    HashMap<int, Loop> Profiles);

[ComplexValueObject]
public sealed partial class PostPolicy {
    public CutConditioning Cut { get; }
    public ProgramTooling Tooling { get; }
    public ProgramSetup Setup { get; }
    public EmitPolicy Emit { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref CutConditioning cut,
        ref ProgramTooling tooling, ref ProgramSetup setup, ref EmitPolicy emit) => validationError =
        cut is null || cut.Dynamics is null || cut.Cooling is null || tooling is null || tooling.Slots is null
        || tooling.Policy is null || setup is null || setup.Schedule is null || setup.Workholding is null
        || setup.Workholding.Fixture is null || setup.Workholding.State is null || emit is null
        || emit.Limit is not BlockLimit.Enforce
            ? new ValidationError(message: "post-policy") : null;

    public static Fin<PostPolicy> Admit(PostRaw raw) {
        ArgumentNullException.ThrowIfNull(raw);
        ArgumentNullException.ThrowIfNull(raw.Cut);

        return (Slot(raw.Cut.Cut.TraverseM(CutPolicy.Admit).As()), Slot(raw.Cut.Fit.TraverseM(FitPolicy.Admit).As()),
         Slot(raw.Cut.Compensation.TraverseM(CompPolicy.Admit).As()))
        .Apply((cut, fit, compensation) => new CutConditioning(cut, fit, raw.Cut.Dynamics, raw.Cut.Cutting,
            compensation, raw.Cut.Cooling, raw.Cut.Chains, raw.Cut.Profiles))
        .As().ToFin()
        .Bind(conditioning => Rail(Validate(conditioning, raw.Tooling, raw.Setup, raw.Emit, out PostPolicy policy), policy));
    }

    internal static K<Validation<Error>, A> Slot<A>(Fin<A> value) => value.ToValidation();

    // One bridge lifts every generated `Validate` outcome onto the rail, so no admission body re-spells the hop.
    internal static Fin<A> Rail<A>(ValidationError? error, A admitted) => error is { } rejected
        ? Fin.Fail<A>(new GeometryFault.DegenerateInput(Kind.Curve, -1, rejected.Message).ToError())
        : Fin.Succ(admitted);

    // Dwell is the one posting quantity `PhysicsQuantity` carries no row for; every other dimensioned field admits there.
    internal static Fin<double> DurationOf(string source, string locus) =>
        Duration.TryParse(source, CultureInfo.InvariantCulture, out Duration value)
            ? Fin.Succ(value.Seconds)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, locus).ToError());
}

public readonly record struct OperationBoundary(Operation Op, int Node, HashMap<ToolLifeBasis, double> Consumed);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramIngress {
    private ProgramIngress() { }

    public sealed record Rs274(string Source, PostDialect Dialect, Encoding Codec, Option<ChecksumRule> Checksum) : ProgramIngress;
    public sealed record Nc1(SteelSource Source, SteelContourPolicy Policy, PostDialect Dialect) : ProgramIngress;
}

```

## [03]-[BOUNDARIES]

- Owner: `Post` composes admitted policy and settled sibling owners into one result rail.
- Entry: `Lower`, `Parse`, and `Publish` each discriminate on an input value rather than overload or mode flags.
- Auto: RS274 token coverage fails closed, NC1 enters through `SteelImport.Read`, and every egress key derives from its complete payload.
- Boundary: `Eff<CutProgram>` carries source acquisition; reusable transforms retain `Fin<T>`; rendered records collapse only at `PostedProgram`.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------------------------------------------------------------------
public static partial class Post {
    private static readonly Seq<GCommand> Commands = toSeq(GCommand.Items);

    public static Fin<FabricationResult.PostedProgram> Lower(
        PostSource source,
        PostDialect dialect,
        FabricationInput input,
        PostPolicy policy) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(policy);

        return from program in Assemble(source, dialect, input, policy)
        from image in Dialect.Emit(program, policy.Emit)
        from _ in image.Kind == EgressKind.CutProgram
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:image-kind:{image.Kind.Key}").ToError())
        select new FabricationResult.PostedProgram(image.Records, image.Key);
    }

    public static Eff<CutProgram> Parse(ProgramIngress ingress) {
        ArgumentNullException.ThrowIfNull(ingress);

        return ingress.Switch(
            rs274: static source => ParseRs274(source.Source, source.Dialect, source.Codec, source.Checksum).ToEff(),
            nc1: static source => SteelImport.Read(source.Source, source.Policy)
                .Map(receipt => CutProgram.Of(Seq<GNode>(new GNode.Nc1(receipt)), source.Dialect)));
    }

    public static Fin<Seq<EncodedGeometry>> Publish(CutProgram program, ProgramView view, PackPolicy policy) {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(policy);

        return from trace in Interpret(program)
        from paths in view.Paths(trace)
        from _ in !paths.IsEmpty && paths.ForAll(static path => !path.Spans.IsEmpty)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "post:publish-points").ToError())
        from encoded in paths.TraverseM(path => Encode.Apply(new PackOp.Toolpath(path, policy))).As()
        select encoded;
    }

    public static Fin<ProgramTrace> Interpret(CutProgram program) {
        ArgumentNullException.ThrowIfNull(program);

        return ProgramTrace.Admit(program);
    }
}
```

## [04]-[CONDITIONING]

- Owner: `CutConditioning` composes cut, fit, compensation, dynamics, and committed-chain policy as admitted values.
- Entry: motion, placement, and specialized envelopes enter one `Post.Lower` fold and diverge only inside `PostSource.Switch`.
- Auto: `ToolMagazine.Schedule` carries lifecycle and process-range evidence; `SetupSchedule.Apply` supplies WCS assignment; `Workholding.Apply` conditions motion; `ArcAlgebra.Apply` owns kerf, lead, and compensation.
- Boundary: statement flow is confined to modal, render, and parse boundaries with the `LookaheadKernel`, `Segments`, `Fit`, and `BulgeArc` numeric kernels; joins use `Fold`, `FoldM`, `TraverseM`, generated `Switch`, and query syntax.

```csharp signature
// --- [CONDITIONING] -----------------------------------------------------------------------------------------------------------------------------------
public static partial class Post {

    internal static Fin<CutProgram> Assemble(
        PostSource source,
        PostDialect dialect,
        FabricationInput input,
        PostPolicy policy) =>
        from _ in dialect.Admits(input.Process.Modality)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:dialect:{dialect.Key}:{input.Process.Modality.Key}").ToError())
        from changes in ToolMagazine.Schedule(policy.Tooling.Slots, policy.Tooling.Work, policy.Tooling.Policy)
        from scheduled in SetupSchedule.Apply(new SetupOp.Schedule(policy.Setup.Schedule))
        from schedule in scheduled is SetupResult.Scheduled value
            ? Fin.Succ(value.Schedule)
            : Fin.Fail<SetupSchedule>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:setup-result").ToError())
        from program in source.Switch(
            state: (Dialect: dialect, Input: input, Policy: policy, Changes: changes, Schedule: schedule),
            motion: static (state, value) => MotionProgram(value.Value, state.Dialect, state.Policy, state.Changes, state.Schedule),
            placement: static (state, value) => PlacementProgram(value.Value, state.Dialect, state.Policy, state.Changes, state.Schedule),
            specialized: static (state, value) => SpecializedProgram(value.Value, state.Dialect, state.Schedule))
        select program;

    private static Fin<CutProgram> SpecializedProgram(
        SpecializedToolpathEnvelope payload,
        PostDialect dialect,
        SetupSchedule schedule) => payload.IsValid
            ? Fin.Succ(CutProgram.Of(Prologue(schedule)
                .Add(new GNode.Directive(new MotionDirective.Specialized(-1, payload)))
                .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect))
            : Fin.Fail<CutProgram>(new GeometryFault.DegenerateInput(
                Kind.Curve, -1, $"post:specialized:{payload.Kind.Key}").ToError());

    private static Fin<CutProgram> MotionProgram(
        FabricationResult.Motion motion,
        PostDialect dialect,
        PostPolicy policy,
        Seq<ToolChange> changes,
        SetupSchedule schedule) =>
        from held in Workholding.Apply(new WorkholdingOp.Condition(
            policy.Setup.Workholding.Fixture,
            policy.Setup.Workholding.State,
            motion.Moves))
        from moves in held is WorkholdingResult.Conditioned conditioned
            ? Fin.Succ(conditioned.Moves)
            : Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:workholding-result").ToError())
        from body in ToolSections(GNode.Moves(moves, motion.Directives, Point3d.Origin), changes, policy)
        from looked in Lookahead(body, policy.Cut.Dynamics, dialect)
        let program = CutProgram.Of(Prologue(schedule).Concat(looked)
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect)
        select program;

    private static Fin<CutProgram> PlacementProgram(
        FabricationResult.Placement placement,
        PostDialect dialect,
        PostPolicy policy,
        Seq<ToolChange> changes,
        SetupSchedule schedule) =>
        from paths in policy.Cut.Chains.IsEmpty
            ? Unlinked(placement, dialect, policy)
            : policy.Cut.Chains.TraverseM(chain => ChainPath(chain, dialect, policy)).As().Map(static rows => rows.Bind(identity))
        from body in ToolSections(paths, changes, policy)
        from looked in Lookahead(body, policy.Cut.Dynamics, dialect)
        let program = CutProgram.Of(Prologue(schedule).Concat(looked)
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect)
        select program;

    private static Fin<Seq<GNode>> Unlinked(FabricationResult.Placement placement, PostDialect dialect, PostPolicy policy) =>
        from profiles in placement.Parts.Map(transform => policy.Cut.Profiles.Find(transform.PartId)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:profile:{transform.PartId}").ToError())
            .Bind(transform.Apply)).TraverseM(identity).As()
        from ordered in PolygonAlgebra.Apply(new PolygonOp.Inspect(profiles.ToSeq(), new PolygonQuery.Topology(PolygonFill.NonZero)))
        from loops in ordered is PolygonTrace.Regions regions
            ? Fin.Succ(regions.Result.Nodes.OrderByDescending(static node => node.Depth)
                .ThenBy(static node => Math.Abs(node.SignedArea)).Map(static node => node.Boundary).ToSeq())
            : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:placement-topology").ToError())
        from paths in loops.TraverseM(loop => Condition(loop, policy.Cut).Bind(conditioned => CutPath(conditioned, dialect, policy.Cut))).As()
        select paths.Bind(identity);

    private static Fin<Seq<GNode>> ChainPath(ChainRow chain, PostDialect dialect, PostPolicy policy) =>
        from _ in chain.Members.IsEmpty
            ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:chain:{chain.Chain}").ToError())
            : Fin.Succ(unit)
        let contours = chain.Members.Bind(static member => member.Contours)
        from _ in chain.Shared.IsEmpty && contours.ForAll(static contour => contour.Omitted.IsEmpty)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:chain-shared:{chain.Chain}").ToError())
        from _ in contours.Filter(static contour => contour.Pierce).Count == chain.Pierces.Count
            && chain.RapidPaths.Count == chain.Pierces.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:chain-routing:{chain.Chain}").ToError())
        from folded in contours.FoldM<Fin, (Seq<GNode> Nodes, int Pierce)>(
            (Seq<GNode>(), 0),
            (state, contour) =>
                from loop in Condition(contour.Path, policy.Cut)
                from nodes in Walk(loop, dialect, policy.Cut)
                let prefix = contour.Pierce
                    ? chain.RapidPaths[state.Pierce].Tail.Map(point => (GNode)new GNode.Word(GCommand.Rapid, XY(point), None)).ToSeq()
                        .Concat(PierceBlock(policy.Cut.Cut, dialect))
                    : Seq<GNode>(new GNode.Word(GCommand.Feed,
                        XY(contour.Entry).Add(GParam.Number('F', FeedFloor(policy.Cut), ProgramUnits.Metric)), None))
                select (state.Nodes.Concat(prefix).Concat(nodes), state.Pierce + (contour.Pierce ? 1 : 0))).As()
        select folded.Nodes;

    private static Fin<Loop> Condition(Loop profile, CutConditioning policy) =>
        !profile.Closed
            ? Fin.Fail<Loop>(FabricationFault.OpenLoop(FabConcern.Program, 0).ToError())
            : policy.Cut.Match(
                Some: cut =>
                    from forest in ArcForest.Admit(Seq(profile), profile.Tolerance, profile.Plane).ToFin()
                    from trace in ArcAlgebra.Apply(new ArcOp.Kerf(forest, cut.KerfMm,
                        profile.Winding() == Sign.Negative ? MaterialSide.Inside : MaterialSide.Outside))
                    from loop in trace is ArcTrace.Forest result
                        ? result.Result.Loops.HeadOrNone().ToFin(FabricationFault.KerfCollision(new KerfWitness.Vanished(0), cut.KerfMm).ToError())
                        : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:kerf-trace").ToError())
                    from compensated in Compensate(loop, policy)
                    select compensated,
                None: () => Compensate(profile, policy));

    private static Fin<Loop> Compensate(Loop loop, CutConditioning policy) => policy.Compensation.Match(
        Some: compensation =>
            from mechanical in policy.Cutting.Match(
                Some: cutting => cutting.FeedBasis == FeedBasis.PerTooth
                    ? cutting.Force(compensation.CutWidthMm, cutting.Feed).Map(compensation.Deflection)
                    : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:compensation-feed-basis:{cutting.FeedBasis.Key}").ToError()),
                None: () => Fin.Succ(0.0))
            let delta = mechanical + compensation.ThermalGrowth
            from offset in Math.Abs(delta) <= loop.Tolerance.Absolute.Value
                ? Fin.Succ(loop)
                : ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(loop),
                        loop.Winding() == Sign.Negative ? -delta : delta))
                    .Bind(trace => trace is ArcTrace.Paths paths
                        ? paths.Result.HeadOrNone().ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:compensation-empty").ToError())
                        : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:compensation-trace").ToError()))
            select offset,
        None: () => Fin.Succ(loop));

    private static Fin<Seq<GNode>> CutPath(Loop loop, PostDialect dialect, CutConditioning policy) =>
        from pierce in Sample(loop, 0.0)
        from lead in Lead(loop, policy.Cut)
        from body in Walk(loop, dialect, policy)
        select Seq<GNode>(new GNode.Word(GCommand.Rapid,
                XY(lead.HeadOrNone().Map(GNode.Target).IfNone(pierce)), None))
            .Concat(PierceBlock(policy.Cut, dialect))
            .Concat(lead.IsEmpty ? Seq<GNode>() : GNode.Moves(lead, pierce))
            .Concat(body);

    private static Fin<Seq<Move>> Lead(Loop loop, Option<CutPolicy> policy) => policy.Match(
        Some: cut => cut.Lead.Shape(cut.LeadRadiusMm).Match(
            Some: shape => ArcAlgebra.Apply(new ArcOp.Lead(loop, 0.0, cut.FeedMmPerMinute, shape,
                    loop.Winding() == Sign.Negative ? MaterialSide.Inside : MaterialSide.Outside))
                .Bind(trace => trace is ArcTrace.Motion motion
                    ? Fin.Succ(motion.Receipt.Moves)
                    : Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:lead-trace").ToError())),
            None: () => Fin.Succ(Seq<Move>())),
        None: () => Fin.Succ(Seq<Move>()));

    private static Fin<Seq<GNode>> Walk(Loop loop, PostDialect dialect, CutConditioning policy) =>
        from segments in Segments(loop, policy.Cut)
        from folded in segments.FoldM<Fin, (Seq<GNode> Output, Seq<Point3d> Run)>(
            (Seq<GNode>(), Seq(loop.At(0))),
            (state, segment) => segment.Tab
                ? FlushRun(state.Run, policy.Fit, FeedCeiling(policy)).Map(flushed =>
                    (state.Output.Concat(flushed).Concat(Bridge(segment.To, policy.Cut, dialect)), Seq(segment.To)))
                : Math.Abs(segment.Bulge) <= loop.Tolerance.Absolute.Value
                    ? Fin.Succ((state.Output, state.Run.Add(segment.To)))
                    : FlushRun(state.Run, policy.Fit, FeedCeiling(policy)).Map(flushed =>
                        (state.Output.Concat(flushed).Add(BulgeArc(segment.From, segment.To, segment.Bulge,
                            Feedrate(loop, segment.Span, policy))), Seq(segment.To)))).As()
        from tail in FlushRun(folded.Run, policy.Fit, FeedCeiling(policy))
        select folded.Output.Concat(tail);

    private static Fin<Seq<PathSegment>> Segments(Loop loop, Option<CutPolicy> policy) {
        double total = loop.Length();
        Seq<TabWindow> tabs = policy.Bind(cut => cut.TabSpacingMm > 0.0 && cut.TabWidthMm > 0.0
            ? Some(Range(0, (int)Math.Floor(total / cut.TabSpacingMm)).Map(index => cut.TabSpacingMm * (index + 0.5))
                .Map(center => new TabWindow(center - cut.TabWidthMm / 2.0, center + cut.TabWidthMm / 2.0))
                .Filter(window => window.Start > loop.Tolerance.Absolute.Value
                    && window.End < total - loop.Tolerance.Absolute.Value))
            : None).IfNone(Seq<TabWindow>());
        Seq<double> stations = Range(0, loop.Spans).Map(index => loop.At(index).DistanceTo(loop.At(index + 1)))
            .Fold(Seq(0.0), static (state, length) => state.Add(state.Last.IfNone(0.0) + length))
            .Concat(tabs.Bind(static window => Seq(window.Start, window.End))).Add(total)
            .Distinct().OrderBy(static value => value).ToSeq();
        return Range(0, stations.Count - 1).Map(index =>
            from from in Sampled(loop, stations[index])
            from to in Sampled(loop, stations[index + 1])
            let midpoint = (stations[index] + stations[index + 1]) / 2.0
            let sourceBulge = loop.BulgeAt(from.Segment)
            let sourceLength = Math.Max(loop.Tolerance.Absolute.Value,
                loop.At(from.Segment).DistanceTo(loop.At(from.Segment + 1)))
            let fraction = (stations[index + 1] - stations[index]) / sourceLength
            let bulge = Math.Abs(sourceBulge) <= loop.Tolerance.Absolute.Value
                ? 0.0 : Math.Sign(sourceBulge) * Math.Tan(Math.Atan(Math.Abs(sourceBulge)) * fraction)
            select new PathSegment(from.Segment, from.Point, to.Point, bulge,
                tabs.Exists(window => midpoint > window.Start && midpoint < window.End)))
            .TraverseM(identity).As();
    }

    private static Fin<ProfileResult.Sampled> Sampled(Loop loop, double station) =>
        loop.Apply(new ProfileOp.Sample(Length.FromMillimeters(station))).Bind(result => result is ProfileResult.Sampled sampled
            ? Fin.Succ(sampled)
            : Fin.Fail<ProfileResult.Sampled>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:sample-result").ToError()));

    private static Fin<Point3d> Sample(Loop loop, double station) => Sampled(loop, station).Map(static result => result.Point);

    private static Fin<Seq<GNode>> FlushRun(Seq<Point3d> run, Option<FitPolicy> policy, double feed) => policy.Match(
        Some: fit => run.Count < fit.ProbeFloor || run.Zip(run.Skip(1)).Sum(static pair => pair.Item1.DistanceTo(pair.Item2)) < fit.MinimumRunMm
            ? Fin.Succ(Lines(run, feed))
            : Fit(run, fit, feed),
        None: () => Fin.Succ(Lines(run, feed)));

    private static Fin<Seq<GNode>> Fit(Seq<Point3d> run, FitPolicy policy, double feed) {
        Point3d first = run[0];
        Point3d last = run[run.Count - 1];
        Vector2d start = new(first.X, first.Y);
        Vector2d end = new(last.X, last.Y);
        Vector2d tangentA = new(run[1].X - first.X, run[1].Y - first.Y);
        Vector2d tangentB = new(last.X - run[run.Count - 2].X, last.Y - run[run.Count - 2].Y);
        if (tangentA.Length <= policy.ToleranceMm || tangentB.Length <= policy.ToleranceMm)
            return Fin.Succ(Lines(run, feed));
        BiArcFit2 fit = new(start, tangentA.Normalized, end, tangentB.Normalized, policy.SplitDistanceMm);
        double deviation = run.Tail.Init.Fold(0.0, (held, probe) => {
            Vector2d sample = new(probe.X, probe.Y);
            Vector2d nearest = fit.NearestPoint(sample);
            return Math.Max(held, Math.Max(fit.Distance(sample),
                Math.Sqrt(Math.Pow(nearest.x - sample.x, 2.0) + Math.Pow(nearest.y - sample.y, 2.0))));
        });
        bool admitted = fit.FitD1 > 0.0 && fit.FitD2 > 0.0 && deviation <= policy.ToleranceMm;
        return admitted
            ? toSeq(fit.Curves).TraverseM(curve => CurveNode(curve, feed)).As()
            : Fin.Succ(Lines(run, feed));
    }

    private static Fin<GNode> CurveNode(IParametricCurve2d curve, double feed) => curve switch {
        Arc2d arc => Fin.Succ<GNode>(ArcNode(arc, feed)),
        Segment2d segment => Fin.Succ<GNode>(SegmentNode(segment, feed)),
        _ => Fin.Fail<GNode>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:fit-curve:{curve.GetType().Name}").ToError()),
    };

    private static GNode SegmentNode(Segment2d segment, double feed) {
        Vector2d end = segment.SampleArcLength(segment.Length);
        return new GNode.Word(GCommand.Feed,
            XY(new Point3d(end.x, end.y, 0.0)).Add(GParam.Number('F', feed, ProgramUnits.Metric)), None);
    }

    private static GNode ArcNode(Arc2d arc, double feed) {
        Vector2d start = arc.SampleArcLength(0.0);
        Vector2d end = arc.SampleArcLength(arc.ArcLength);
        return new GNode.Word(arc.IsReversed ? GCommand.ArcCw : GCommand.ArcCcw,
            Arr(GParam.Number('X', end.x, ProgramUnits.Metric), GParam.Number('Y', end.y, ProgramUnits.Metric),
                GParam.Number('I', arc.Center.x - start.x, ProgramUnits.Metric), GParam.Number('J', arc.Center.y - start.y, ProgramUnits.Metric),
                GParam.Number('F', feed, ProgramUnits.Metric)), None);
    }

    private static Seq<GNode> Lines(Seq<Point3d> points, double feed) => points.Tail.Map(point =>
        (GNode)new GNode.Word(GCommand.Feed, XY(point).Add(GParam.Number('F', feed, ProgramUnits.Metric)), None)).ToSeq();

    private static GNode BulgeArc(Point3d first, Point3d last, double bulge, double feed) {
        PlineVertex<double> start = new(first.X, first.Y, bulge);
        PlineVertex<double> end = new(last.X, last.Y, 0.0);
        var (_, center) = PlineSeg.SegArcRadiusAndCenter(start, end);
        return new GNode.Word(bulge > 0.0 ? GCommand.ArcCcw : GCommand.ArcCw,
            XY(last).Add(GParam.Number('I', center.X - first.X, ProgramUnits.Metric))
                .Add(GParam.Number('J', center.Y - first.Y, ProgramUnits.Metric))
                .Add(GParam.Number('F', feed, ProgramUnits.Metric)), None);
    }

    private static Seq<GNode> Bridge(Point3d target, Option<CutPolicy> policy, PostDialect dialect) =>
        Seq<GNode>(new GNode.Word(GCommand.SpindleStop, Arr<GParam>(), None),
            new GNode.Word(GCommand.Rapid, XY(target), None)).Concat(PierceBlock(policy, dialect));

    private static Seq<GNode> PierceBlock(Option<CutPolicy> policy, PostDialect dialect) => policy.Match(
        Some: cut => cut.AssistBar.Map(assist => (GNode)new GNode.Word(
                GCommand.AssistGas, Arr(GParam.Number('S', assist, ProgramUnits.Metric)), None)).ToSeq()
            .Add(new GNode.Word(BeamOn(dialect), Arr<GParam>(), None))
            .Concat(cut.PierceSeconds > 0.0
                ? Seq<GNode>(new GNode.CannedCycle(GCommand.Dwell,
                    Arr(GParam.Number('P', cut.PierceSeconds, ProgramUnits.Metric)), Seq<Move>(), 1, None))
                : Seq<GNode>()),
        None: () => Seq<GNode>());

    // Only a thermal-only controller spells beam-on as the torch word; a controller carrying a contact modality
    // spells it as the spindle word, so the declared modality set decides and no dialect identity is tested.
    private static GCommand BeamOn(PostDialect dialect) =>
        dialect.Modalities.Contains(ProcessModality.Thermal)
        && dialect.Modalities.ForAll(static modality => modality == ProcessModality.Thermal)
            ? GCommand.TorchOn : GCommand.Spindle;

    private static Fin<Seq<GNode>> ToolSections(Seq<GNode> nodes, Seq<ToolChange> changes, PostPolicy policy) =>
        from _ in changes.Exists(static change => change.Previous.IsSome) && policy.Tooling.Boundaries.IsEmpty
            ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "post:tool-boundaries").ToError())
            : Fin.Succ(unit)
        from placements in changes.TraverseM(change => change.Previous.IsNone
            ? Fin.Succ((Node: 0, Change: change))
            : policy.Tooling.Boundaries
                .Filter(boundary => boundary.Op == change.Op && boundary.Node >= 0 && boundary.Node < nodes.Count
                    && boundary.Consumed.Find(change.LimitingBasis).Exists(consumed => consumed >= change.Trigger))
                .OrderBy(static boundary => boundary.Node).HeadOrNone()
                .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"post:tool-boundary:{change.Op.Key}:{change.LimitingBasis.Key}").ToError())
                .Map(boundary => (boundary.Node, Change: change))).As()
        select Range(0, nodes.Count).Bind(index => placements.Filter(row => row.Node == index)
            .Bind(row => ToolChangeNodes(row.Change)
                .Concat(SpindleNodes(policy.Cut.Cutting, row.Change.Assembly))
                .Concat(CoolingNodes(policy.Cut.Cooling)))
            .Add(ClampFeed(nodes[index], placements.Filter(row => row.Node <= index).Last.Map(static row => row.Change.Assembly.Feed))));

    private static Seq<GNode> ToolChangeNodes(ToolChange change) =>
        Seq<GNode>(
            new GNode.Word(GCommand.SpindleStop, Arr<GParam>(), None),
            new GNode.Word(GCommand.CoolantOff, Arr<GParam>(), None),
            new GNode.Word(GCommand.LengthCancel, Arr<GParam>(), None),
            new GNode.Word(GCommand.Rapid, Arr(GParam.Number('Z', change.Retract, ProgramUnits.Metric)), None))
        .Concat(change.Behaviors.Contains(MagazineBehavior.Confirm)
            ? Seq<GNode>(new GNode.Word(GCommand.OptionalStop, Arr<GParam>(), None)) : Seq<GNode>())
        .Add(new GNode.Word(GCommand.ToolChange, Arr(GParam.Number('T', change.ProgramTool, ProgramUnits.Metric)), None))
        .Add(new GNode.Word(GCommand.LengthOffset,
            Arr(GParam.Number('H', change.ProgramTool, ProgramUnits.Metric), GParam.Number('Z', change.LengthOffset, ProgramUnits.Metric)), None));

    private static Seq<GNode> SpindleNodes(Option<CuttingData> cutting, ToolAssembly assembly) => cutting.Map(data => {
        double requested = data.SurfaceSpeed * 1_000.0 / (Math.PI * assembly.ShankDiameter);
        double spindle = Clamp(requested, assembly.Spindle);
        return (GNode)new GNode.Word(GCommand.Spindle, Arr(GParam.Number('S', spindle, ProgramUnits.Metric)), None);
    }).ToSeq();

    private static Seq<GNode> CoolingNodes(CoolingPolicy cooling) => cooling.Word().Map(command =>
        (GNode)new GNode.Word(command, Arr<GParam>(), None)).ToSeq();

    private static GNode ClampFeed(GNode node, Option<ProcessRange> range) => node is GNode.Word word && word.P('F').IsSome
        ? word.With('F', range.Map(value => Clamp(word.P('F').IfNone(0.0), value)).IfNone(word.P('F').IfNone(0.0)))
        : node;

    private static double Clamp(double requested, ProcessRange range) {
        double selected = range.Current.OrElse(range.Nominal).IfNone(requested);
        double minimum = range.Minimum.IfNone(double.NegativeInfinity);
        double maximum = range.Maximum.IfNone(double.PositiveInfinity);
        return Math.Clamp(Math.Min(requested, selected), minimum, maximum);
    }

    internal static Fin<Seq<GNode>> Lookahead(Seq<GNode> nodes, MotionDynamics dynamics, PostDialect dialect) =>
        Interpret(CutProgram.Of(nodes, dialect)).Map(trace => {
            ProgramEvent.Motion[] motions = trace.Events.Choose(static item => item is ProgramEvent.Motion motion
                ? Some(motion) : None).ToArray();
            return RewriteLookahead(nodes, Seq<int>(), new LookaheadKernel(motions, dynamics).Run());
        });

    private static Seq<GNode> RewriteLookahead(Seq<GNode> nodes, Seq<int> prefix, Seq<LookaheadCap> caps) =>
        nodes.Map((node, index) => (Node: node, Locus: prefix.Add(index))).Map(row => row.Node.Switch(
            state: (Locus: row.Locus, Caps: caps),
            block: static (context, block) => block with {
                Body = RewriteLookahead(block.Body.ToSeq(), context.Locus, context.Caps).ToArr(),
            },
            word: static (context, word) => context.Caps
                .Filter(cap => cap.Locus.SequenceEqual(context.Locus))
                .Fold(double.PositiveInfinity, static (held, cap) => Math.Min(held, cap.Feed)) is var feed
                && double.IsFinite(feed) ? word.With('F', feed) : word,
            cannedCycle: static (_, cycle) => cycle,
            coordinateFrame: static (_, frame) => frame,
            macro: static (context, macro) => macro with {
                Body = RewriteLookahead(macro.Body.ToSeq(), context.Locus, context.Caps).ToArr(),
            },
            subprogram: static (context, subprogram) => subprogram with {
                Body = RewriteLookahead(subprogram.Body.ToSeq(), context.Locus, context.Caps).ToArr(),
            },
            additiveLayer: static (_, layer) => layer,
            nc1: static (_, nc1) => nc1,
            directive: static (_, directive) => directive));

    private readonly record struct LookaheadCap(Seq<int> Locus, double Feed);

    private ref struct LookaheadKernel {
        private readonly ProgramEvent.Motion[] motions;
        private readonly MotionDynamics dynamics;
        private readonly double[] caps;
        private readonly double[] distances;
        private readonly bool[] cutting;
        private readonly Vector3d[] vectors;

        public LookaheadKernel(ProgramEvent.Motion[] motions, MotionDynamics dynamics) {
            this.motions = motions;
            this.dynamics = dynamics;
            caps = Enumerable.Repeat(double.PositiveInfinity, motions.Length).ToArray();
            distances = new double[motions.Length];
            cutting = new bool[motions.Length];
            vectors = new Vector3d[motions.Length];
        }

        public Seq<LookaheadCap> Run() {
            for (int index = 0; index < motions.Length; index++) {
                ProgramEvent.Motion motion = motions[index];
                vectors[index] = motion.To - motion.From;
                distances[index] = vectors[index].Length;
                cutting[index] = motion.Cutting && motion.Word.P('F').IsSome && distances[index] > 0.0;
                caps[index] = cutting[index] ? motion.Word.P('F').IfNone(dynamics.CuttingFeed) : caps[index];
            }
            for (int index = 0; index < motions.Length; index++)
                if (cutting[index])
                    caps[index] = Math.Min(caps[index], Junction(index));
            Sweep(0, motions.Length, 1);
            Sweep(motions.Length - 1, -1, -1);
            return Range(0, motions.Length).Filter(index => cutting[index])
                .Map(index => new LookaheadCap(motions[index].Locus.Source, caps[index])).ToSeq();
        }

        private void Sweep(int start, int end, int step) {
            double held = 0.0;
            for (int index = start; index != end; index += step) {
                if (!cutting[index]) {
                    held = 0.0;
                    continue;
                }
                caps[index] = Math.Min(caps[index], Reachable(held, distances[index], dynamics));
                held = caps[index] / 60.0;
            }
        }

        private double Junction(int index) {
            Vector3d incoming = vectors[index];
            _ = incoming.Unitize();
            double turn = 0.0;
            int inspected = 0;
            for (int cursor = index + 1; cursor < motions.Length && inspected < dynamics.LookaheadBlocks; cursor++) {
                if (!cutting[cursor])
                    continue;
                Vector3d outgoing = vectors[cursor];
                _ = outgoing.Unitize();
                turn = Math.Max(turn, Vector3d.VectorAngle(incoming, outgoing));
                incoming = outgoing;
                inspected++;
            }
            return turn <= 0.0 ? dynamics.CuttingFeed : Math.Min(dynamics.CuttingFeed, dynamics.JunctionFeed(turn));
        }
    }

    private static double Reachable(double entry, double distance, MotionDynamics dynamics) => Math.Min(
        Math.Sqrt(entry * entry + 2.0 * dynamics.Acceleration * distance),
        entry + Math.Cbrt(6.0 * dynamics.Jerk * distance * distance)) * 60.0;

    private static double Feedrate(Loop loop, int span, CutConditioning policy) {
        double ceiling = FeedCeiling(policy);
        int before = (span - 1 + loop.Spans) % loop.Spans;
        int after = (span + 1) % loop.Spans;
        Vector3d incoming = loop.At(span) - loop.At(before);
        Vector3d outgoing = loop.At(after) - loop.At(span);
        _ = incoming.Unitize();
        _ = outgoing.Unitize();
        return Math.Min(ceiling, policy.Dynamics.JunctionFeed(Vector3d.VectorAngle(incoming, outgoing)));
    }

    private static Seq<GNode> Prologue(SetupSchedule schedule) =>
        schedule.Wcs.Map(assignment => (GNode)new GNode.CoordinateFrame(
            assignment,
            schedule.Setups[assignment.Setup].Mounting.Frame)).ToSeq();

    private static Fin<CutProgram> ParseRs274(
        string source, PostDialect dialect, Encoding codec, Option<ChecksumRule> checksum) =>
        codec is null
            ? Fin.Fail<CutProgram>(FabricationFault.ProgramParse(0, ModalGroup.NonModal).ToError())
            : Lines(source).FoldM<Fin, ParseState>(new ParseState(ModalState.Empty, Seq<GNode>()),
            (state, row) => from parsed in ParseBlock(
                    row.Line, row.Text, dialect, codec, checksum, state.Modal, state.Nodes.Count)
                select new ParseState(parsed.Modal, state.Nodes.Add(parsed.Block)))
        .As().Bind(state => state.Nodes.Exists(static node => node is GNode.Block { Body.IsEmpty: false })
            ? Fin.Succ(CutProgram.Of(state.Nodes, dialect))
            : Fin.Fail<CutProgram>(FabricationFault.ProgramParse(0, ModalGroup.NonModal).ToError()));

    private static Fin<(GNode Block, ModalState Modal)> ParseBlock(
        int line, string text, PostDialect dialect, Encoding codec, Option<ChecksumRule> checksumRule,
        ModalState modal, int locus) {
        ProgramLocus at = ProgramLocus.Root(line, locus);
        string record = text.Trim();
        Seq<string> comments = CommentText.Matches(text).Select(static match => match.Value).ToSeq();
        string body = CommentText.Replace(text, string.Empty).Trim();
        if (body.Length == 0 || body == "%") {
            GNode.Block empty = new(
                new BlockFrame(None, None, false, body == "%", None, comments, text), Arr<GNode>());
            return modal.Push(at, empty).Map(next => ((GNode)empty, next));
        }
        bool optional = body.StartsWith("/", StringComparison.Ordinal);
        string opened = optional ? body[1..].TrimStart() : body;
        Seq<string> tokens = WordText.Matches(opened).Select(static match => match.Value).ToSeq();
        string residue = WordText.Replace(opened, string.Empty);
        Match check = ChecksumText.Match(record);
        Option<uint> parsedChecksum = checksumRule.Bind(rule => check.Success
            && uint.TryParse(check.Groups[1].Value,
                rule.Width > 0 ? NumberStyles.HexNumber : NumberStyles.Integer,
                CultureInfo.InvariantCulture, out uint checksumValue)
                ? Some(checksumValue)
                : None);
        Option<int> checksum = parsedChecksum.Filter(static value => value <= int.MaxValue)
            .Map(static value => checked((int)value));
        residue = ChecksumText.Replace(residue, string.Empty);
        int checksumCount = ChecksumText.Matches(opened).Count;
        bool frameValid = checksumCount <= 1 && checksumCount == (check.Success ? 1 : 0)
            && tokens.Filter(static token => char.ToUpperInvariant(token[0]) == 'O').Count <= 1
            && tokens.Filter(static token => char.ToUpperInvariant(token[0]) == 'N').Count <= 1
            && (!check.Success || dialect.Features.Contains(DialectFeature.Checksum)
                && checksumRule.Exists(rule => parsedChecksum.Exists(value =>
                    value == rule.Digest(codec.GetBytes(record[..check.Index])))));
        if (!frameValid || residue.Any(static character => !char.IsWhiteSpace(character)))
            return Fin.Fail<(GNode, ModalState)>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError());
        Option<int> program = NumberToken(tokens, 'O');
        Option<int> sequence = NumberToken(tokens, 'N');
        Seq<string> words = tokens.Filter(static token => char.ToUpperInvariant(token[0]) is not 'O' and not 'N').ToSeq();
        BlockFrame frame = new(program, sequence, optional, false, checksum, comments, text);
        ModalState entered = modal with { Events = modal.Events.Add(new ProgramEvent.Boundary(at, frame)) };
        return ParseWords(line, words, dialect, entered, at).Bind(parsed => {
            GNode.Block block = new(frame, parsed.Nodes.ToArr());
            return ModalState.AdmitBlock(line, block.Body).Map(_ => ((GNode)block, parsed.Modal));
        });
    }

    private static Fin<ParseState> ParseWords(
        int line, Seq<string> tokens, PostDialect dialect, ModalState modal, ProgramLocus locus) {
        (Seq<(string Command, Seq<string> Parameters)> Segments, Seq<string> Leading) split = tokens.Fold(
            (Segments: Seq<(string, Seq<string>)>(), Leading: Seq<string>()),
            (state, token) => IsCommand(token)
                ? (state.Segments.Add((token, Seq<string>())), state.Leading)
                : state.Segments.IsEmpty
                    ? (state.Segments, state.Leading.Add(token))
                    : (state.Segments.Init.Add((state.Segments.Last.IfNone((string.Empty, Seq<string>())).Item1,
                        state.Segments.Last.IfNone((string.Empty, Seq<string>())).Item2.Add(token))), state.Leading));
        return tokens.IsEmpty
            ? Fin.Succ(new ParseState(modal, Seq<GNode>()))
            : split.Segments.IsEmpty
                ? Fin.Fail<ParseState>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError())
            : Seq((split.Segments.Head.IfNone((string.Empty, Seq<string>())).Item1,
                    split.Leading.Concat(split.Segments.Head.IfNone((string.Empty, Seq<string>())).Item2)))
                .Concat(split.Segments.Tail)
                .FoldM<Fin, ParseState>(new ParseState(modal, Seq<GNode>()), (state, segment) =>
                    from command in Resolve(line, segment.Item1, segment.Item2, dialect)
                    let normalized = NormalizeWcs(command, segment.Item1, segment.Item2)
                    from parameters in normalized.TraverseM(token => ParseParam(line, token, command, state.Modal)).As()
                    from admitted in command.Admit(line, parameters.ToArr())
                    let node = (GNode)new GNode.Word(command, admitted, Feed(command))
                    from next in state.Modal.Push(locus.Descend(state.Nodes.Count), node)
                    select new ParseState(next, state.Nodes.Add(node)))
                .As();
    }

    private static Fin<GCommand> Resolve(int line, string token, Seq<string> parameters, PostDialect dialect) {
        Seq<GCommand> candidates = Commands.Filter(command => WireCode(command.Code) == WireCode(token)
            || command == GCommand.Wcs && IsWcs(token));
        Seq<char> addresses = parameters.Filter(static value => value.Length > 1).Map(static value => char.ToUpperInvariant(value[0])).ToSeq();
        Seq<GCommand> admitted = candidates.Filter(command => command.Admits(dialect)
            && command.Grammar.Required.ForAll(addresses.Contains)
            && addresses.ForAll(command.Grammar.Allowed.Contains));
        return admitted.Count == 1
            ? admitted.Head.ToFin(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError())
            : Fin.Fail<GCommand>(FabricationFault.ProgramParse(line, ModalGroup.NonModal).ToError());
    }

    private static Seq<string> NormalizeWcs(GCommand command, string token, Seq<string> parameters) =>
        command == GCommand.Wcs
        && BaseWcs(token).Map(ordinal => $"P{ordinal}").Filter(_ => !parameters.Exists(static value => value.StartsWith('P'))).IsSome
            ? parameters.Add(BaseWcs(token).Map(ordinal => $"P{ordinal}").IfNone(string.Empty))
            : parameters;

    private static bool IsWcs(string token) =>
        BaseWcs(token).IsSome;

    private static Option<int> BaseWcs(string token) =>
        int.TryParse(WireCode(token).AsSpan(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int code)
        && code is >= 54 and <= 59
            ? Some(code - 53)
            : None;

    private static Fin<GParam> ParseParam(int line, string token, GCommand command, ModalState modal) {
        if (token.Length < 2)
            return Fin.Fail<GParam>(FabricationFault.ProgramParse(line, command.Group).ToError());
        char address = char.ToUpperInvariant(token[0]);
        string lexeme = token[1..];
        if (lexeme.StartsWith('#') && int.TryParse(lexeme[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int variable))
            return Fin.Succ(new GParam(address, new GValue.Variable(variable, lexeme)));
        if (lexeme.StartsWith('[') && lexeme.EndsWith(']'))
            return Fin.Succ(new GParam(address, new GValue.Expression(lexeme)));
        if (!double.TryParse(lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out double received))
            return Fin.Fail<GParam>(FabricationFault.ProgramParse(line, command.Group).ToError());
        bool dimensioned = address is 'X' or 'Y' or 'Z' or 'U' or 'V' or 'W' or 'I' or 'J' or 'K'
            || (address == 'E' && command == GCommand.Extrude)
            || (address == 'F' && modal.Mode == FeedMode.UnitsPerMinute)
            || (address == 'R' && (command == GCommand.ArcCw || command == GCommand.ArcCcw || command.Group == ModalGroup.Cycle));
        double canonical = dimensioned ? modal.Units.Canonical(received) : received;
        return Fin.Succ(new GParam(address, new GValue.Number(canonical, lexeme, modal.Units)));
    }

    private static Option<int> NumberToken(Seq<string> tokens, char address) => tokens.Find(token =>
        char.ToUpperInvariant(token[0]) == address).Bind(token => int.TryParse(token[1..], NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int value) ? Some(value) : None);

    private static Option<FeedMode> Feed(GCommand command) => command == GCommand.FeedInverseTime
        ? Some(FeedMode.InverseTime) : command == GCommand.FeedPerMinute ? Some(FeedMode.UnitsPerMinute) : None;

    private static bool IsCommand(string token) => IsWcs(token)
        || Commands.Exists(command => WireCode(command.Code) == WireCode(token));

    private static string WireCode(string token) {
        int prefixLength = token.TakeWhile(static character => char.IsLetter(character)).Count();
        string prefix = token[..prefixLength].ToUpperInvariant();
        return prefixLength == token.Length || !decimal.TryParse(token[prefixLength..], NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value)
            ? prefix : $"{prefix}{value.ToString("0.####", CultureInfo.InvariantCulture)}";
    }

    private static Seq<(int Line, string Text)> Lines(string source) =>
        source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n')
            .Select((text, index) => (Line: index + 1, Text: text)).ToSeq();

    private static Arr<GParam> XY(Point3d point) => Arr(
        GParam.Number('X', point.X, ProgramUnits.Metric),
        GParam.Number('Y', point.Y, ProgramUnits.Metric));

    private static double FeedCeiling(CutConditioning policy) =>
        policy.Cut.Map(static value => value.FeedMmPerMinute).IfNone(policy.Dynamics.CuttingFeed);
    private static double FeedFloor(CutConditioning policy) =>
        policy.Cut.Map(static value => value.FeedMmPerMinute * value.LinkFeedFactor).IfNone(policy.Dynamics.CuttingFeed);

    private readonly record struct ParseState(ModalState Modal, Seq<GNode> Nodes);
    private readonly record struct TabWindow(double Start, double End);
    private readonly record struct PathSegment(int Span, Point3d From, Point3d To, double Bulge, bool Tab);

    [GeneratedRegex(@"\([^)]*\)|;[^\r\n]*")]
    private static partial Regex CommentText { get; }

    [GeneratedRegex(@"[A-Za-z]+(?:#[0-9]+|\[[^\]]+\]|[+-]?(?:\d+(?:\.\d*)?|\.\d+))")]
    private static partial Regex WordText { get; }

    [GeneratedRegex(@"\*([0-9A-Fa-f]+)\s*$")]
    private static partial Regex ChecksumText { get; }
}

```
