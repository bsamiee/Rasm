# [RASM_FABRICATION_PROGRAM]

`Post` owns one dialect-neutral `CutProgram` from admitted source through modal interpretation, cut conditioning, grammar lowering, rendered records, and analytic `PackKind.Toolpath` projection. `GNode.Directive` preserves controller directives and specialized toolpath evidence beside motion; `GWord.Render` is the physical-record correspondence consumed by capacity checks and receipts.

`PostSource`, `PostDialect`, `EmitPolicy`, `SetupPlan`, `Fixture`, `ChainRow`, `ToolChange`, and `ContentKey` arrive as settled seams. `NodeKey` is the ONE structural identity over the AST — a per-node `UInt128` over the `Rasm.Element` `CanonicalWriter`, held on the program so a pass fold pays one digest per node it changed rather than a full serialization per intermediate tree. `QuantityArrow` is the one dimension-text entry, so no policy admission on this page reaches `PhysicsQuantity` directly; `SurfaceSpeed` at `Process/physics#BUDGET_FOLD` is the one spindle law, composed over the CUTTING diameter the tool snapshot measures. A process names NO dialect: the controller is a property of the machine, so `PostDialect.Admits(ProcessModality)` resolves every pairing and the resolving modality rides `ProgramIngress` where two command rows share one wire code.

## [01]-[INDEX]

- [02]-[COMMAND_VOCABULARY]: `ProgramUnits`, `DistanceMode`, `ModalGroup`, `FeedMode`, `CoolingPolicy`, `LeadStyle`, `WordValuePolicy`, `MotionRole`, `CommandGrammar`, `GCommand`, and the wire-code index every resolution reads.
- [03]-[PROGRAM_AST]: `GValue`, `GParam`, `GNode`, `NodeKey`, `GWord`, `RenderReceipt`, `ProgramLocus`, `ProgramEvent`, `ProgramTrace`, `ModalState`, and `CutProgram`.
- [04]-[POLICY_ADMISSION]: `CutPolicy`, `FitPolicy`, `CompPolicy`, `PostPolicy`, `ProgramView`, and `ProgramIngress`.
- [05]-[BOUNDARIES]: `Post.Lower`, `Post.Parse`, `Post.Publish`, and `Post.Interpret`.
- [06]-[CONDITIONING]: placement, tooling, setup, workholding, arc conditioning, tab partition, and the lookahead fold.
- [07]-[PARSING]: RS274 block framing, the linear word split, and command resolution against the wire-code index.

## [02]-[COMMAND_VOCABULARY]

- Owner: `GCommand` owns the closed command roster with its grammar, modal group, motion role, demanded features, and admitting modalities; `CommandGrammar` owns address shape; `WireCode` owns the normalized token identity every resolution keys on.
- Law: `GCommand.Requires` and `GCommand.Modalities` declare what a command demands of a controller, and `GCommand.Admits` decides admissibility against `PostDialect.Features` and `PostDialect.Modalities` — no dialect identity is ever tested, and no roster mirrors the vocabulary.
- Law: two rows MAY share one wire code where their modalities are disjoint — `M7` is mist coolant on a contact controller and torch-on on a thermal one. The resolving discriminant is the PROGRAM's own `ProcessModality`, which `ProgramIngress` carries, so a hybrid controller admitting both modalities still resolves each token to exactly one row; filtering on the dialect's whole modality SET left both rows standing and refused the program the two rows exist to serve.
- Auto: the wire-code index is built ONCE from `GCommand.Items` and keyed by normalized code, so resolution costs one lookup per token rather than a scan of the roster per token.
- Growth: a command is one `GCommand` row with its grammar and demanded features; a modal family is one `ModalGroup` row.
- Boundary: dialect byte spelling stays in `Dialect`; this cluster declares codes as ROW data and renders none.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CavalierContours.Polyline;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Projection;
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

// --- [TYPES] ------------------------------------------------------------------------------------------------------------------------------------------
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
            : Fin.Fail<Arr<GParam>>(new FabricationFault.ProgramParse(line, group));
    }

    public bool Fits(Seq<char> addresses) => Required.ForAll(addresses.Contains) && addresses.ForAll(Allowed.Contains);
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
    // Two rows, one wire code, DISJOINT modalities: the program's own modality resolves the pair, so a controller
    // admitting both contact and thermal work still parses each token to exactly one row.
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
                : Fin.Fail<Arr<GParam>>(new FabricationFault.ProgramParse(line, Group)));

    // Dialect admissibility is the row's own declared demand against the dialect's declared capability, so a new
    // controller is one `PostDialect` row and a new command one `Requires` set, with no roster on either side.
    public bool Admits(PostDialect dialect) =>
        Requires.ForAll(dialect.Features.Contains)
        && (Modalities.IsEmpty || Modalities.Exists(dialect.Modalities.Contains));

    // The program's OWN modality, never the controller's whole set: a row declaring no modality serves every
    // program, and a row declaring one serves only the program running that modality.
    public bool Serves(ProcessModality modality) => Modalities.IsEmpty || Modalities.Contains(modality);

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

// The ONE token identity and the ONE index over it. `WireCode` normalizes letter prefix and decimal tail so `M7`,
// `M07`, and `M7.0` are one key; the index is built once from the roster, so resolution costs a lookup per token
// where the prior scan cost the whole roster per token.
public static class WireCode {
    private static readonly FrozenDictionary<string, Seq<GCommand>> Index = toSeq(GCommand.Items)
        .GroupBy(static command => Of(command.Code))
        .ToDictionary(static group => group.Key, static group => toSeq(group), StringComparer.Ordinal)
        .ToFrozenDictionary(StringComparer.Ordinal);

    public static string Of(string token) {
        int prefixLength = token.TakeWhile(char.IsLetter).Count();
        string prefix = token[..prefixLength].ToUpperInvariant();
        return prefixLength == token.Length
            || !decimal.TryParse(token[prefixLength..], NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value)
                ? prefix
                : $"{prefix}{value.ToString("0.####", CultureInfo.InvariantCulture)}";
    }

    public static Seq<GCommand> Candidates(string token) =>
        Index.TryGetValue(Of(token), out Seq<GCommand> rows) ? rows : Seq<GCommand>();

    public static bool Known(string token) => Index.ContainsKey(Of(token));
}
```

## [03]-[PROGRAM_AST]

- Owner: `CutProgram` mints the canonical AST and `Post` owns every transform that changes it; `NodeKey` owns structural identity; `ModalState` owns the one semantic walk.
- Cases: `GNode` carries block framing beside executable node families; `GValue` preserves numeric, variable, expression, and text evidence; `GCommand.Wcs` and `WcsExtended` retain base and extended coordinate forms; `ProgramEvent` carries the canonical interpretation.
- Law: `NodeKey` is the ONE structural identity and it rides the `Rasm.Element` `CanonicalWriter` through `FabricationCanon` — a second byte codec beside that writer is the deleted form, and the string-framed concatenation it replaces was exactly that. A node's key digests its own subtree, so a rewriting pass re-keys only what it changed and an optimization fold reading a stream of `UInt128` keys pays no serialization at all.
- Law: `CutProgram.Key` is HELD, derived on first read from the node keys and the dialect. A pass chain minting seven intermediate programs paid seven whole-tree serializations for keys six of them never published.
- Law: `ProgramUnits` carries one millimetre scale in both directions, and `GNode.Word.With` preserves the replaced value's source units or the word's established source-unit row. Every `ProgramEvent` carries one structural `ProgramLocus`; motion also carries every admitted axis, resolved plane, and arc center, so consumers never re-derive modal state, discard rotary or auxiliary axes, or substitute a chord.
- Auto: `GCommand.Admit` composes address shape with row-owned scalar policy before AST construction, and `ModalState` threads controller state once.
- Receipt: `CutProgram.Key` identifies the AST; admitted `ProgramTrace` preserves modal state and the complete node-and-repeat path of every expanded executable leaf.
- Exemption: `ModalState.Apply` and `ArcOf` are the modal statement kernels — the arc-centre resolution is a numeric boundary where plane and arc-distance rows are simultaneously in hand.
- Growth: a syntax construct is one `GNode` case, one `NodeKey` arm, and one `ModalState.Push` arm.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------------------------------------------------------------------
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

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Switch(
        state: writer,
        number: static (row, value) => row.String("number").Double(value.Canonical).String(value.Lexeme).Discriminant(value.SourceUnits),
        integer: static (row, value) => row.String("integer").I64(value.Value).String(value.Lexeme),
        variable: static (row, value) => row.String("variable").I64(value.Index).String(value.Lexeme),
        expression: static (row, value) => row.String("expression").String(value.Lexeme),
        text: static (row, value) => row.String("text").String(value.Value));
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

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Value.CanonicalBytes(writer.String(Address.ToString()));
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
    string Source) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Maybe(Program, static (row, value) => row.I64(value))
        .Maybe(Sequence, static (row, value) => row.I64(value))
        .Bool(Optional).Bool(Delimiter)
        .Maybe(Checksum, static (row, value) => row.I64(value))
        .Rows(Comments, static (row, value) => row.String(value))
        .String(Source);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GNode {
    private GNode() { }

    public sealed record Block(BlockFrame Frame, Arr<GNode> Body) : GNode;
    public sealed record Word(GCommand Command, Arr<GParam> Words, Option<FeedMode> Mode) : GNode {
        public Option<double> P(char address) => Words.Find(parameter => parameter.Address == address).Bind(static parameter => parameter.Value.Scalar);
        public ProgramUnits SourceUnits => Words.Choose(static parameter => parameter.Value is GValue.Number number
            ? Some(number.SourceUnits) : None).Head.IfNone(ProgramUnits.Metric);
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

// One structural digest per node over the ONE package codec. A node's key covers its own subtree, so an equality
// test between two ASTs is a `UInt128` compare and a pattern census streams keys rather than re-serializing bodies.
// The quantization grid is the DIALECT's own emitted precision, so two programs a controller cannot distinguish
// key alike and a re-post of one drawing is byte-identical.
public static class NodeKey {
    public static double Grid(PostDialect dialect) => Math.Pow(10.0, -dialect.Decimals);

    public static UInt128 Of(GNode node, double grid) =>
        ContentHash.Of(Write(new CanonicalWriter(grid), node).ToBytes().Span);

    public static Seq<UInt128> Stream(Seq<GNode> nodes, double grid) => nodes.Map(node => Of(node, grid));

    private static CanonicalWriter Write(CanonicalWriter writer, GNode node) => node.Switch(
        state: writer,
        block: static (row, value) => value.Frame.CanonicalBytes(row.String("block"))
            .Rows(value.Body.ToSeq(), Write),
        word: static (row, value) => row.String("word").Discriminant(value.Command)
            .Maybe(value.Mode, static (mode, feed) => mode.Discriminant(feed))
            .Rows(value.Words.ToSeq(), static (param, item) => item.CanonicalBytes(param)),
        cannedCycle: static (row, value) => row.String("cycle").Discriminant(value.Command).Ordinal(value.Repeats)
            .Maybe(value.Mode, static (mode, feed) => mode.Discriminant(feed))
            .Rows(value.SingleBlockWords.ToSeq(), static (param, item) => item.CanonicalBytes(param))
            .Rows(value.ExpandedMoves, WriteMove),
        coordinateFrame: static (row, value) => WriteSlot(
                row.String("coordinate-frame").Ordinal(value.Assignment.Setup), value.Assignment.Slot)
            .Coords(value.Frame.Origin).Coords(value.Frame.XAxis).Coords(value.Frame.YAxis),
        macro: static (row, value) => row.String("macro")
            .Rows(value.Slots.ToSeq(), static (slot, item) => item.Value.CanonicalBytes(slot.Ordinal(item.Index).String(item.Key)))
            .Rows(value.Body.ToSeq(), Write),
        subprogram: static (row, value) => row.String("subprogram").Ordinal(value.Label).Ordinal(value.Repeats)
            .Rows(value.Body.ToSeq(), Write),
        additiveLayer: static (row, value) => row.String("additive").Ordinal(value.Layer)
            .Double(value.Extrusion.Amount).Double(value.Extrusion.Feed)
            .Double(value.Temperatures.Hotend).Double(value.Temperatures.Bed),
        nc1: static (row, value) => value.Receipt.Key.CanonicalBytes(row.String("nc1")),
        directive: static (row, value) => WriteDirective(row, value.Value));

    private static CanonicalWriter WriteDirective(CanonicalWriter writer, MotionDirective directive) => directive.Switch(
        state: writer,
        spindle: static (row, value) => row.String("spindle").Discriminant(value.Control).Discriminant(value.Hand)
            .Double(value.SurfaceMetersPerMinute).Double(value.ResolvedRpm)
            .Maybe(value.CeilingRpm, static (rpm, ceiling) => rpm.Double(ceiling)),
        dwell: static (row, value) => row.String("dwell").Ordinal(value.AfterMove).Discriminant(value.Basis).Double(value.Amount),
        synchronize: static (row, value) => row.String("synchronize").Ordinal(value.FromMove).Ordinal(value.ToMove)
            .Double(value.Rpm).Double(value.Lead).Discriminant(value.Hand),
        orientedStop: static (row, value) => row.String("oriented-stop").Ordinal(value.AfterMove)
            .Double(value.OrientDeg).Coords(value.Retract),
        channelBarrier: static (row, value) => row.String("channel-barrier").Ordinal(value.Step).String(value.Channel)
            .Rows(value.WaitFor, static (wait, item) => wait.String(item))
            .Maybe(value.Signal, static (signal, item) => signal.String(item)),
        specialized: static (row, value) => row.String("specialized").Ordinal(value.AfterMove)
            .Discriminant(value.Payload.Kind).Double(value.Payload.DurationSeconds)
            .Rows(value.Payload.Rows, WriteSpecialized));

    // The specialized rows carry the evidence a posted program must preserve, so each row's own columns enter the
    // preimage: two envelopes differing only in a wire lag or a bevel cross-tilt key apart.
    private static CanonicalWriter WriteSpecialized(CanonicalWriter writer, SpecializedToolpathRow row) => row.Switch(
        state: writer,
        wire: static (at, value) => at.String("wire").Ordinal(value.Pass).Double(value.Station).Double(value.Progress)
            .Double(value.TraversedMm).Coords(value.Lower).Coords(value.Upper).Discriminant(value.Action)
            .Double(value.LagMm).Double(value.UpperCornerRadiusMm)
            .Maybe(value.RotaryDeg, static (rotary, angle) => rotary.Double(angle)),
        bevel: static (at, value) => at.String("bevel").Ordinal(value.Move).Ordinal(value.Pass).Double(value.Station)
            .Ordinal(value.SourceSpan).Double(value.SourceBulge).Coords(value.Point).Coords(value.ToolAxis)
            .Coords(value.Pivot).Double(value.AngleDeg).Double(value.CrossTiltDeg)
            .Double(value.FeedMmPerMin).Double(value.CompensationMm),
        link: static (at, value) => at.String("link").String(value.From).String(value.To).Discriminant(value.Transition)
            .Double(value.DistanceMm).Double(value.DurationSeconds).Double(value.LiftMm).Double(value.ThermalExposure)
            .Double(value.RotationPenalty).Ordinal(value.Retracts).Ordinal(value.Pierces)
            .Ordinal(value.ToolChanges).Ordinal(value.SetupChanges),
        inspection: static (at, value) => at.String("inspection").Ordinal(value.Pass).Ordinal(value.FromBlock)
            .Ordinal(value.ToBlockExclusive).Double(value.NominalAngleDeg).Double(value.NominalOffsetMm)
            .Double(value.AngleDeviationDeg).Double(value.OffsetDeviationMm).Bool(value.Conforming),
        turningThread: static (at, value) => at.String("turning-thread").Discriminant(value.Form)
            .Double(value.LoadFlankDeg).Double(value.ClearanceFlankDeg).Double(value.CrestFlat).Double(value.RootFlat)
            .Double(value.CrestRadius).Double(value.RootRadius).Discriminant(value.Side),
        turningAxial: static (at, value) => at.String("turning-axial").Ordinal(value.FromMove).Ordinal(value.ToMove)
            .Discriminant(value.Kind).Double(value.Diameter).Double(value.Depth).Double(value.TipAngleDeg),
        turningTap: static (at, value) => at.String("turning-tap").Ordinal(value.FromMove).Ordinal(value.ToMove)
            .Double(value.Diameter).Double(value.Depth).Double(value.Pitch)
            .Discriminant(value.Form).Discriminant(value.Hand),
        turningKnurl: static (at, value) => at.String("turning-knurl").Ordinal(value.FromMove).Ordinal(value.ToMove)
            .Discriminant(value.Pattern).Double(value.Pressure),
        turningHandoff: static (at, value) => at.String("turning-handoff").Discriminant(value.Kind)
            .String(value.From).String(value.To).Double(value.GripPlane).Double(value.GripLength).Double(value.PullDistance));

    private static CanonicalWriter WriteMove(CanonicalWriter writer, Move move) => move.Switch(
        state: writer,
        rapid: static (row, value) => row.String("rapid").Coords(value.Target),
        linear: static (row, value) => row.String("linear").Coords(value.Target).Double(value.Feed),
        circular: static (row, value) => row.String("circular").Coords(value.Target).Double(value.Feed)
            .Coords(value.Arc.Center).Discriminant(value.Arc.Sense).Double(value.SweepRadians));

    private static CanonicalWriter WriteSlot(CanonicalWriter writer, WcsSlot slot) => slot.Switch(
        state: writer,
        @base: static (row, value) => row.String("base").Ordinal(value.Ordinal),
        extended: static (row, value) => row.String("extended").Ordinal(value.Ordinal),
        dynamic: static (row, value) => row.String("dynamic").Ordinal(value.Ordinal),
        rotary: static (row, value) => row.String("rotary").Ordinal(value.Ordinal).Double(value.Axis),
        local: static (row, value) => row.String("local").Ordinal(value.Ordinal).Ordinal(value.Parent));
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

// `Locus` is the universal column the root owns; each case threads it as the plain argument and never re-declares
// the base property's name.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramEvent(ProgramLocus Locus) {

    public sealed record Motion(
        ProgramLocus locus,
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
        Option<MotionArc> Arc) : ProgramEvent(locus) {
        public bool Cutting => Role == MotionRole.Cutting;
    }
    public sealed record State(ProgramLocus locus, GNode.Word Word) : ProgramEvent(locus) {
        public GCommand Command => Word.Command;
    }
    public sealed record Boundary(ProgramLocus locus, BlockFrame Frame) : ProgramEvent(locus);
    public sealed record Coordinate(ProgramLocus locus, WcsAssignment Assignment, Plane Frame) : ProgramEvent(locus);
    public sealed record Additive(ProgramLocus locus, int Layer, ExtrusionProfile Extrusion, TemperatureSet Temperatures) : ProgramEvent(locus);
    public sealed record Exchange(ProgramLocus locus, SteelImportReceipt Receipt) : ProgramEvent(locus);
    public sealed record Directive(ProgramLocus locus, MotionDirective Value) : ProgramEvent(locus);
}

public sealed record ProgramTrace {
    private ProgramTrace(ModalState final) => Final = final;

    public ModalState Final { get; }
    public Seq<ProgramEvent> Events => Final.Events;

    internal static Fin<ProgramTrace> Admit(Seq<GNode> nodes) => nodes.IsEmpty
        ? Fin.Fail<ProgramTrace>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:trace-empty"))
        : nodes.Map((node, block) => (node, block)).FoldM<Fin, ModalState>(ModalState.Empty,
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
        macro: static (context, value) => toSeq(value.Body).Map((item, index) => (Item: item, Index: index))
            .FoldM<Fin, ModalState>(context.State, (state, row) => state.Push(context.Locus.Descend(row.Index), row.Item)).As(),
        subprogram: static (context, value) =>
            from _ in value.Label > 0 && value.Repeats > 0
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.ProgramParse(context.Locus.Block, ModalGroup.NonModal))
            from expanded in Range(0, value.Repeats).FoldM<Fin, ModalState>(context.State,
                (state, repeat) => toSeq(value.Body).Map((item, index) => (Item: item, Index: index))
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
            : Fin.Fail<Unit>(new FabricationFault.ProgramParse(locus.Block, ModalGroup.Cycle))
        from admitted in value.Command.Admit(locus.Block, value.SingleBlockWords)
        from expanded in Range(0, value.Repeats).FoldM<Fin, ModalState>(state, (cycle, repeat) =>
            value.ExpandedMoves.IsEmpty
                ? Apply(cycle, new GNode.Word(value.Command, admitted, value.Mode), locus.Repeated(0, repeat))
                : value.ExpandedMoves.Map((move, index) => (Move: move, Index: index))
                    .FoldM<Fin, ModalState>(cycle, (current, row) => GNode.Move(row.Move, current.Position) is GNode.Word word
                        ? PushWord(current, word with { Mode = value.Mode }, locus.Repeated(row.Index, repeat))
                        : Fin.Fail<ModalState>(new FabricationFault.ProgramParse(locus.Block, ModalGroup.Cycle))).As()).As()
        select expanded;

    private static Fin<ModalState> PushBlock(ModalState state, GNode.Block value, ProgramLocus locus) =>
        AdmitBlock(locus.Block, value.Body).Bind(_ =>
            toSeq(value.Body).Map((item, index) => (Item: item, Index: index)).FoldM<Fin, ModalState>(state with {
                Events = state.Events.Add(new ProgramEvent.Boundary(locus, value.Frame)),
            }, (current, item) => current.Push(locus.Descend(item.Index), item.Item)).As());

    internal static Fin<Unit> AdmitBlock(int block, Arr<GNode> body) {
        Seq<ModalGroup> groups = body.Choose(static node => node switch {
            GNode.Word { Command.Group: var group } when group != ModalGroup.NonModal => Some(group),
            GNode.CannedCycle { Command.Group: var group } when group != ModalGroup.NonModal => Some(group),
            _ => None,
        }).ToSeq();
        return groups.Distinct().Exists(group => groups.Count(candidate => candidate == group) > 1)
            ? Fin.Fail<Unit>(new FabricationFault.ProgramParse(block,
                groups.Find(group => groups.Count(candidate => candidate == group) > 1).IfNone(ModalGroup.NonModal)))
            : Fin.Succ(unit);
    }

    private static Fin<ModalState> PushWord(ModalState state, GNode.Word word, ProgramLocus locus) =>
        word.Command.Admit(locus.Block, word.Words).Bind(_ => Apply(state, word, locus));

    // Exemption: the modal apply is the semantic boundary — units, distance, plane, feed, and axis targets settle
    // together, and splitting them puts one block's state on two reads that can disagree.
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
            return Fin.Fail<Option<MotionArc>>(new FabricationFault.ProgramParse(block, ModalGroup.Motion));
        return radius
            ? word.P('R').ToFin(new FabricationFault.ProgramParse(block, ModalGroup.Motion))
                .Bind(value => RadiusCenter(start, target, plane, value, sense, block)
                    .Map<Option<MotionArc>>(resolved => Some(new MotionArc(resolved, sense))))
            : word.Words.Filter(static parameter => parameter.Address is 'I' or 'J' or 'K')
                .ForAll(static parameter => parameter.Value.Scalar.IsSome)
                ? Fin.Succ(Some(new MotionArc(ArcCenter(start, word, plane, arcDistance), sense)))
                : Fin.Fail<Option<MotionArc>>(new FabricationFault.ProgramParse(block, ModalGroup.Motion));
    }

    private static Fin<Point3d> RadiusCenter(
        Point3d start, Point3d target, GCommand plane, double signedRadius, RotationSense sense, int block) {
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
            return Fin.Fail<Point3d>(new FabricationFault.ProgramParse(block, ModalGroup.Motion));
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

// A sealed class rather than a record: `Key` and `Keys` are DERIVED views held on first read, so they stay out of
// equality by construction. A pass chain that minted seven intermediate programs paid seven whole-tree
// serializations under the record form; here an unread intermediate costs nothing.
public sealed class CutProgram {
    private ContentKey? key;
    private Seq<UInt128>? keys;

    private CutProgram(Seq<GNode> nodes, PostDialect dialect) => (Nodes, Dialect) = (nodes, dialect);

    public Seq<GNode> Nodes { get; }
    public PostDialect Dialect { get; }

    public static CutProgram Of(Seq<GNode> nodes, PostDialect dialect) => new(nodes, dialect);

    // The structural key stream every pattern census and equality test reads. One digest per top-level node, each
    // covering its own subtree.
    public Seq<UInt128> Keys => keys ??= NodeKey.Stream(Nodes, NodeKey.Grid(Dialect));

    public ContentKey Key => key ??= ContentKey.Of(EgressKind.CutProgram,
        Keys.Fold(new CanonicalWriter(NodeKey.Grid(Dialect)).Discriminant(Dialect).Ordinal(Keys.Count),
            static (writer, node) => writer.U128(node)).ToBytes().Span);
}
```

## [04]-[POLICY_ADMISSION]

- Owner: `CutPolicy`, `FitPolicy`, and `CompPolicy` own the dimensioned cut, fit, and compensation decisions; `PostPolicy` composes them with tooling, setup, and emission; `ProgramView` owns geometry egress; `ProgramIngress` owns the parse grammar and the modality that resolves it.
- Law: `QuantityArrow` is the ONE dimension-text entry this page reaches — `new QuantityArrow(axis, FabConcern.Posting, locus).Admit(text)` routes to `ProcessPhysics.Admit` and re-raises on the POSTING plane. A `PhysicsQuantity.<axis>.Admit` call here is a second text boundary answering on a foreign plane and is the deleted form.
- Law: `ProgramIngress.Rs274` carries the program's own `ProcessModality` because two command rows may share one wire code under disjoint modalities. Without it a hybrid controller resolves such a token to two candidates and refuses the program.
- Entry: every policy admits through its generated `Validate` and the one `Admitted` bridge; independent dimension failures accumulate through `Validation<Error, _>` before the `Fin` rail.
- Auto: `CompPolicy` derives cantilever stiffness, deflection, and thermal growth from its admitted columns, so no caller re-derives a compensation term; the load that stiffness divides is `CuttingLoad.TangentialPerEdge` off `Tooling/cuttingdata`'s one force evaluation, so this page holds no force body of its own.
- Receipt: `ProgramView.Paths` returns the run partition directly — a coordinate change re-frames every following point, so it closes a run exactly as an excluded move does, and the open run carries as `Option` rather than a null cursor.
- Boundary: dwell is the one posting quantity `PhysicsQuantity` carries no row for, so it admits through `UnitsNet.Duration` at one declared site.

```csharp signature
// --- [POLICIES] ---------------------------------------------------------------------------------------------------------------------------------------
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
public sealed record CompRaw(
    string ToolDiameter,
    string CutWidth,
    string AxialDepth,
    string Stickout,
    int Teeth,
    double Modulus,
    double ThermalCoefficient,
    double TemperatureDelta);

// The one dimension-text arrow family this page reaches. Each row names its own locus, so a refusal is addressable
// at the slot that produced it and every axis routes through `ProcessPhysics.Admit`.
public static class PostArrow {
    public static QuantityArrow Of(PhysicsQuantity axis, string locus) => new(axis, FabConcern.Posting, locus);

    // Dwell is the one posting quantity `PhysicsQuantity` carries no row for; every other dimensioned field admits
    // through the arrow.
    public static Fin<double> Seconds(string source, string locus) =>
        UnitsNet.Duration.TryParse(source, CultureInfo.InvariantCulture, out UnitsNet.Duration value)
            ? Fin.Succ(value.Seconds)
            : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, locus));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError, ref double kerfMm, ref LeadStyle lead,
        ref double leadRadiusMm, ref double tabWidthMm, ref double tabSpacingMm, ref double pierceSeconds,
        ref Option<double> assistBar, ref double feedMmPerMinute, ref double linkFeedFactor) {
        if (!Seq(kerfMm, leadRadiusMm, tabWidthMm, tabSpacingMm, pierceSeconds, feedMmPerMinute, linkFeedFactor).ForAll(double.IsFinite)
            || assistBar.Exists(static value => !Witness.Positive(value))
            || kerfMm < 0.0 || pierceSeconds < 0.0 || feedMmPerMinute <= 0.0
            || linkFeedFactor <= 0.0 || linkFeedFactor > 1.0
            || (lead != LeadStyle.None && leadRadiusMm <= 0.0)
            || tabWidthMm < 0.0 || tabSpacingMm < 0.0 || (tabWidthMm > 0.0 && tabSpacingMm <= tabWidthMm))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post-cut-policy");
    }

    public static Fin<CutPolicy> Admit(CutRaw raw) =>
        (PostArrow.Of(PhysicsQuantity.Length, "post-cut:kerf").Admit(raw.Kerf).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-cut:lead-radius").Admit(raw.LeadRadius).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-cut:tab-width").Admit(raw.TabWidth).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-cut:tab-spacing").Admit(raw.TabSpacing).ToValidation(),
         PostArrow.Seconds(raw.Pierce, "post-cut:pierce").ToValidation(),
         raw.Assist.TraverseM(source => PostArrow.Of(PhysicsQuantity.Pressure, "post-cut:assist").Admit(source)).As().ToValidation(),
         PostArrow.Of(PhysicsQuantity.Feed, "post-cut:feed-ceiling").Admit(raw.FeedCeiling).ToValidation())
        .Apply((kerf, lead, tabWidth, tabSpacing, pierce, assist, feed) =>
            Validate(kerf, raw.Lead, lead, tabWidth, tabSpacing, pierce, assist, feed, raw.LinkFeedFactor, out CutPolicy policy)
                .Admitted(policy))
        .As().ToFin().Bind(static value => value);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class FitPolicy {
    public double ToleranceMm { get; }
    public double MinimumRunMm { get; }
    public double SplitDistanceMm { get; }
    public int ProbeFloor { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError, ref double toleranceMm,
        ref double minimumRunMm, ref double splitDistanceMm, ref int probeFloor) {
        // A biarc fit needs three interior samples before a tangent pair means anything, so the probe floor is the
        // arity the fit itself demands rather than a tuning knob.
        if (!Seq(toleranceMm, minimumRunMm, splitDistanceMm).ForAll(Witness.Positive) || probeFloor < 3)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post-fit-policy");
    }

    public static Fin<FitPolicy> Admit(FitRaw raw) =>
        (PostArrow.Of(PhysicsQuantity.Length, "post-fit:tolerance").Admit(raw.Tolerance).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-fit:minimum-run").Admit(raw.MinimumRun).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-fit:split-distance").Admit(raw.SplitDistance).ToValidation())
        .Apply((tolerance, run, split) =>
            Validate(tolerance, run, split, raw.ProbeFloor, out FitPolicy policy).Admitted(policy))
        .As().ToFin().Bind(static value => value);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CompPolicy {
    public double ToolDiameterMm { get; }
    public double CutWidthMm { get; }

    // The engaged edge length — the chip WIDTH the force model prices its per-edge load over. Radial width alone
    // decides how much of the cutter is in material, never how much of the edge is, so both axes are declared.
    public double AxialDepthMm { get; }

    public double StickoutMm { get; }
    public int Teeth { get; }
    public double Modulus { get; }
    public double ThermalCoefficient { get; }
    public double TemperatureDelta { get; }
    public double Stiffness => 3.0 * Modulus * (Math.PI * Math.Pow(ToolDiameterMm, 4.0) / 64.0) / Math.Pow(StickoutMm, 3.0);
    public double Deflection(double edgeForceN) => edgeForceN / Stiffness;
    public double ThermalGrowth => ThermalCoefficient * StickoutMm * TemperatureDelta;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError, ref double toolDiameterMm,
        ref double cutWidthMm, ref double axialDepthMm, ref double stickoutMm, ref int teeth, ref double modulus,
        ref double thermalCoefficient, ref double temperatureDelta) {
        // The radial width is bounded by the cutter it engages, so the intent this policy builds admits at the
        // tool owner rather than refusing there on a bound this page already knows.
        if (!Seq(toolDiameterMm, cutWidthMm, axialDepthMm, stickoutMm, modulus).ForAll(Witness.Positive)
            || cutWidthMm > toolDiameterMm || teeth <= 0
            || !double.IsFinite(thermalCoefficient) || thermalCoefficient < 0.0
            || !double.IsFinite(temperatureDelta))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post-comp-policy");
    }

    public static Fin<CompPolicy> Admit(CompRaw raw) =>
        (PostArrow.Of(PhysicsQuantity.Length, "post-comp:tool-diameter").Admit(raw.ToolDiameter).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-comp:cut-width").Admit(raw.CutWidth).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-comp:axial-depth").Admit(raw.AxialDepth).ToValidation(),
         PostArrow.Of(PhysicsQuantity.Length, "post-comp:stickout").Admit(raw.Stickout).ToValidation())
        .Apply((diameter, width, axial, stickout) => Validate(diameter, width, axial, stickout, raw.Teeth,
            raw.Modulus, raw.ThermalCoefficient, raw.TemperatureDelta, out CompPolicy policy).Admitted(policy))
        .As().ToFin().Bind(static value => value);
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
[ValidationError<FabricationFault>]
public sealed partial class PostPolicy {
    public CutConditioning Cut { get; }
    public ProgramTooling Tooling { get; }
    public ProgramSetup Setup { get; }
    public EmitPolicy Emit { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError, ref CutConditioning cut,
        ref ProgramTooling tooling, ref ProgramSetup setup, ref EmitPolicy emit) {
        // Posting is the FINAL egress, so a measurement-only block limit here would post a program past the
        // controller's own storage cap; `BlockLimit.Observe` belongs to the optimization measurement leg alone.
        if (emit.Limit is not BlockLimit.Enforce)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post-policy:block-limit");
    }

    public static Fin<PostPolicy> Admit(PostRaw raw) =>
        (raw.Cut.Cut.TraverseM(CutPolicy.Admit).As().ToValidation(),
         raw.Cut.Fit.TraverseM(FitPolicy.Admit).As().ToValidation(),
         raw.Cut.Compensation.TraverseM(CompPolicy.Admit).As().ToValidation())
        .Apply((cut, fit, compensation) => new CutConditioning(cut, fit, raw.Cut.Dynamics, raw.Cut.Cutting,
            compensation, raw.Cut.Cooling, raw.Cut.Chains, raw.Cut.Profiles))
        .As().ToFin()
        .Bind(conditioning => Validate(conditioning, raw.Tooling, raw.Setup, raw.Emit, out PostPolicy policy).Admitted(policy));
}

public readonly record struct OperationBoundary(Operation Op, int Node, HashMap<ToolLifeBasis, double> Consumed);

[SmartEnum<string>]
public sealed partial class ProgramView {
    public static readonly ProgramView AllMotion = new("all-motion", None);
    public static readonly ProgramView Cutting = new("cutting", Some(MotionRole.Cutting));
    public static readonly ProgramView Control = new("control", Some(MotionRole.Control));
    public static readonly ProgramView Probing = new("probing", Some(MotionRole.Probing));
    public static readonly ProgramView Additive = new("additive", Some(MotionRole.Additive));

    public Option<MotionRole> Role { get; }

    // The open run rides `Option`: a null cursor made "no run in progress" and "a run of no spans" the same value,
    // and the fold has no failure mode, so the partition returns directly.
    public Seq<ToolpathPath> Paths(ProgramTrace trace) {
        (Seq<ToolpathPath> Paths, Option<ToolpathPath> Current) folded = trace.Events.Fold(
            (Paths: Seq<ToolpathPath>(), Current: Option<ToolpathPath>.None),
            (state, item) => item switch {
                ProgramEvent.Motion motion when Role.ForAll(role => role == motion.Role) =>
                    (state.Paths, Some(state.Current.Match(
                        Some: held => held with { Spans = held.Spans.Add(Span(motion)) },
                        None: () => new ToolpathPath(motion.From, Seq(Span(motion)))))),
                // A coordinate change re-frames every following point, so it closes the run exactly as an excluded move does.
                ProgramEvent.Motion or ProgramEvent.Coordinate =>
                    (state.Paths.Concat(state.Current.ToSeq()), Option<ToolpathPath>.None),
                _ => state,
            });
        return folded.Paths.Concat(folded.Current.ToSeq());
    }

    private static ToolpathSpan Span(ProgramEvent.Motion motion) => motion.Arc.Match<ToolpathSpan>(
        Some: arc => new ToolpathSpan.Arc(motion.To, arc.Center,
            arc.Sense == RotationSense.Clockwise ? ToolpathArcSense.Clockwise : ToolpathArcSense.Counterclockwise),
        None: () => new ToolpathSpan.Line(motion.To));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramIngress {
    private ProgramIngress() { }

    // The MODALITY is what resolves a wire code two command rows share; the dialect's whole modality set cannot,
    // because a hybrid controller admits both rows and the resolution goes ambiguous.
    public sealed record Rs274(
        string Source,
        PostDialect Dialect,
        ProcessModality Modality,
        Encoding Codec,
        Option<ChecksumRule> Checksum) : ProgramIngress;
    public sealed record Nc1(SteelSource Source, SteelContourPolicy Policy, PostDialect Dialect) : ProgramIngress;
}
```

## [05]-[BOUNDARIES]

- Owner: `Post` composes admitted policy and settled sibling owners into one result rail.
- Entry: `Lower`, `Parse`, and `Publish` each discriminate on an input value rather than an overload or a mode flag.
- Auto: RS274 token coverage fails closed on `ProgramTokenUnresolved`, NC1 enters through `SteelImport.Read`, and every egress key derives from its complete payload.
- Boundary: `Eff<CutProgram>` carries source acquisition; reusable transforms retain `Fin<T>`; rendered records collapse only at `PostedProgram`; every parameter arrives admitted, so no entry guards a null.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------------------------------------------------------------------
public static partial class Post {
    public static Fin<FabricationResult.PostedProgram> Lower(
        PostSource source,
        PostDialect dialect,
        FabricationInput input,
        PostPolicy policy) =>
        from program in Assemble(source, dialect, input, policy)
        from image in Dialect.Emit(program, policy.Emit)
        from _ in image.Kind == EgressKind.CutProgram
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:image-kind:{image.Kind.Key}"))
        select new FabricationResult.PostedProgram(image.Records, image.Key);

    public static Eff<CutProgram> Parse(ProgramIngress ingress) => ingress.Switch(
        rs274: static source => ParseRs274(source).ToEff(),
        nc1: static source => SteelImport.Read(source.Source, source.Policy)
            .Map(receipt => CutProgram.Of(Seq<GNode>(new GNode.Nc1(receipt)), source.Dialect)));

    public static Fin<Seq<EncodedGeometry>> Publish(CutProgram program, ProgramView view, PackPolicy policy) =>
        from trace in Interpret(program)
        let paths = view.Paths(trace)
        from _ in !paths.IsEmpty && paths.ForAll(static path => !path.Spans.IsEmpty)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:publish-points"))
        from encoded in paths.TraverseM(path => Encode.Apply(new PackOp.Toolpath(path, policy))).As()
        select encoded;

    // Interpretation reads the NODES: a caller re-interpreting a rewritten tree pays no content key for a program
    // it may never publish.
    public static Fin<ProgramTrace> Interpret(CutProgram program) => ProgramTrace.Admit(program.Nodes);

    public static Fin<ProgramTrace> Interpret(Seq<GNode> nodes) => ProgramTrace.Admit(nodes);
}
```

## [06]-[CONDITIONING]

- Owner: `CutConditioning` composes cut, fit, compensation, dynamics, and committed-chain policy as admitted values.
- Law: `SpindleNodes` composes `Process/physics#BUDGET_FOLD` `SurfaceSpeed.Rpm` over the CUTTING diameter the tool snapshot measures — a shank diameter is not a cutting diameter and produces a surface speed the cut never sees. A tool carrying no measured cutting diameter refuses rather than posting a spindle word derived from the wrong geometry.
- Law: the specialized envelope arrives ADMITTED — `SpecializedToolpathEnvelope.Admit` folded kind correspondence, non-empty rows, and finite duration once — so a local revalidation here is the deleted form, and its ROWS ride the AST intact so `Dialect` renders each row's own evidence rather than a flattening to moves.
- Entry: motion, placement, and specialized envelopes enter one `Post.Lower` fold and diverge only inside `PostSource.Switch`; every arm opens its program on `Prologue`, which prepends the run's keyed drawing marks as one verbatim comment block ahead of the frame assignments.
- Auto: `ToolMagazine.Schedule` carries lifecycle and process-range evidence; `SetupSchedule.Apply` supplies WCS assignment; `Workholding.Apply` conditions motion; `ArcAlgebra.Apply` owns kerf, lead, and compensation. `Lookahead` interprets the NODES it is handed and never mints a content key for an intermediate tree.
- Exemption: `LookaheadKernel`, `Segments`, `Fit`, and `BulgeArc` are the named numeric kernels; every other join uses `Fold`, `FoldM`, `TraverseM`, generated `Switch`, and query syntax.
- Boundary: only a thermal-only controller spells beam-on as the torch word, and the declared modality set decides it, so no dialect identity is tested.

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
            : Fin.Fail<Unit>(FabricationFault.Pairing(new RelationFault.DialectModality(dialect, input.Process.Modality)))
        from changes in ToolMagazine.Schedule(policy.Tooling.Slots, policy.Tooling.Work, policy.Tooling.Policy)
        from scheduled in SetupSchedule.Apply(new SetupOp.Schedule(policy.Setup.Schedule))
        from schedule in scheduled is SetupResult.Scheduled value
            ? Fin.Succ(value.Schedule)
            : Fin.Fail<SetupSchedule>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:setup-result"))
        from program in source.Switch(
            state: (Dialect: dialect, Input: input, Policy: policy, Changes: changes, Schedule: schedule),
            motion: static (state, value) => MotionProgram(value.Value, state.Dialect, state.Policy, state.Changes, state.Schedule, state.Input.Tags),
            placement: static (state, value) => PlacementProgram(value.Value, state.Dialect, state.Policy, state.Changes, state.Schedule, state.Input.Tags),
            specialized: static (state, value) => SpecializedProgram(value.Value, state.Dialect, state.Schedule, state.Input.Tags))
        select program;

    // The envelope's rows ride the AST whole. A specialized lane's evidence — wire lag, bevel cross-tilt, link
    // transition, inspection deviation, turning form — is exactly what a posted program must carry forward, so the
    // directive keeps the admitted payload and `Dialect` renders one record per row.
    private static Fin<CutProgram> SpecializedProgram(
        SpecializedToolpathEnvelope payload,
        PostDialect dialect,
        SetupSchedule schedule,
        Map<string, Arr<ProfileMarking>> tags) =>
        Fin.Succ(CutProgram.Of(Prologue(schedule, tags)
            .Add(new GNode.Directive(new MotionDirective.Specialized(-1, payload)))
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect));

    private static Fin<CutProgram> MotionProgram(
        FabricationResult.Motion motion,
        PostDialect dialect,
        PostPolicy policy,
        Seq<ToolChange> changes,
        SetupSchedule schedule,
        Map<string, Arr<ProfileMarking>> tags) =>
        from held in Workholding.Apply(new WorkholdingOp.Condition(
            policy.Setup.Workholding.Fixture,
            policy.Setup.Workholding.State,
            motion.Moves))
        from moves in held is WorkholdingResult.Conditioned conditioned
            ? Fin.Succ(conditioned.Moves)
            : Fin.Fail<Seq<Move>>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:workholding-result"))
        from body in ToolSections(GNode.Moves(moves, motion.Directives, Point3d.Origin), changes, policy)
        from looked in Lookahead(body, policy.Cut.Dynamics)
        select CutProgram.Of(Prologue(schedule, tags).Concat(looked)
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect);

    private static Fin<CutProgram> PlacementProgram(
        FabricationResult.Placement placement,
        PostDialect dialect,
        PostPolicy policy,
        Seq<ToolChange> changes,
        SetupSchedule schedule,
        Map<string, Arr<ProfileMarking>> tags) =>
        from paths in policy.Cut.Chains.IsEmpty
            ? Unlinked(placement, dialect, policy)
            : policy.Cut.Chains.TraverseM(chain => ChainPath(chain, dialect, policy)).As().Map(static rows => rows.Bind(identity))
        from body in ToolSections(paths, changes, policy)
        from looked in Lookahead(body, policy.Cut.Dynamics)
        select CutProgram.Of(Prologue(schedule, tags).Concat(looked)
            .Add(new GNode.Word(GCommand.ProgramEnd, Arr<GParam>(), None)), dialect);

    private static Fin<Seq<GNode>> Unlinked(FabricationResult.Placement placement, PostDialect dialect, PostPolicy policy) =>
        from profiles in placement.Parts.Map(transform => policy.Cut.Profiles.Find(transform.PartId)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:profile:{transform.PartId}"))
            .Bind(transform.Apply)).TraverseM(identity).As()
        from ordered in PolygonAlgebra.Apply(new PolygonOp.Topology(profiles.ToSeq(), PolygonFill.NonZero))
        from loops in ordered is PolygonTrace.Regions regions
            ? Fin.Succ(toSeq(regions.Result.Nodes.OrderByDescending(static node => node.Depth)
                .ThenBy(static node => Math.Abs(node.SignedArea)).Select(static node => node.Boundary)))
            : Fin.Fail<Seq<Loop>>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:placement-topology"))
        from paths in loops.TraverseM(loop => Condition(loop, policy.Cut).Bind(conditioned => CutPath(conditioned, dialect, policy.Cut))).As()
        select paths.Bind(identity);

    private static Fin<Seq<GNode>> ChainPath(ChainRow chain, PostDialect dialect, PostPolicy policy) =>
        from _ in chain.Members.IsEmpty
            ? Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:chain:{chain.Chain}"))
            : Fin.Succ(unit)
        let contours = chain.Members.Bind(static member => member.Contours)
        from _shared in chain.Shared.IsEmpty && contours.ForAll(static contour => contour.Omitted.IsEmpty)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:chain-shared:{chain.Chain}"))
        from _routing in contours.Filter(static contour => contour.Pierce).Count == chain.Pierces.Count
            && chain.RapidPaths.Count == chain.Pierces.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:chain-routing:{chain.Chain}"))
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
            ? Fin.Fail<Loop>(new FabricationFault.OpenLoop(FabConcern.Posting, 0))
            : policy.Cut.Match(
                Some: cut =>
                    from forest in ArcForest.Admit(Seq(profile), profile.Tolerance, profile.Plane)
                    from trace in ArcAlgebra.Apply(new ArcOp.Kerf(forest, cut.KerfMm,
                        profile.Winding() == Sign.Negative ? MaterialSide.Inside : MaterialSide.Outside))
                    from loop in trace is ArcTrace.Forest result
                        ? result.Result.Loops.Head.ToFin(FabricationFault.Kerf(new KerfWitness.Vanished(0), cut.KerfMm))
                        : Fin.Fail<Loop>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:kerf-trace"))
                    from compensated in Compensate(loop, policy)
                    select compensated,
                None: () => Compensate(profile, policy));

    private static Fin<Loop> Compensate(Loop loop, CutConditioning policy) => policy.Compensation.Match(
        Some: compensation =>
            from mechanical in policy.Cutting.Match(
                Some: cutting => cutting.FeedBasis == FeedBasis.PerTooth
                    ? Deflection(compensation, cutting)
                    : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:compensation-feed-basis:{cutting.FeedBasis.Key}")),
                None: () => Fin.Succ(0.0))
            let delta = mechanical + compensation.ThermalGrowth
            from offset in Math.Abs(delta) <= loop.Tolerance.Absolute.Value
                ? Fin.Succ(loop)
                : ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(loop),
                        loop.Winding() == Sign.Negative ? -delta : delta))
                    .Bind(trace => trace is ArcTrace.Paths paths
                        ? paths.Result.Head.ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:compensation-empty"))
                        : Fin.Fail<Loop>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:compensation-trace")))
            select offset,
        None: () => Fin.Succ(loop));

    // Cantilever deflection is the load ONE cutting edge carries, so the compensation reads `TangentialPerEdge` off
    // `Tooling/cuttingdata`'s single force evaluation rather than a second force body here — the same receipt a
    // torque or removal-rate consumer reads its engaged column from. The spindle the intent prices at composes the
    // one `SurfaceSpeed` law over the declared cutting diameter, so no rate on this page is derived twice.
    private static Fin<double> Deflection(CompPolicy compensation, CuttingData cutting) {
        double spindle = SurfaceSpeed.Rpm(cutting.SurfaceSpeed, compensation.ToolDiameterMm);
        return CutIntent.Admit(
                chipThickness: Length.FromMillimeters(cutting.Feed),
                chipWidth: Length.FromMillimeters(compensation.AxialDepthMm),
                axialDepth: Length.FromMillimeters(compensation.AxialDepthMm),
                radialDepth: Length.FromMillimeters(compensation.CutWidthMm),
                diameter: Length.FromMillimeters(compensation.ToolDiameterMm),
                teeth: compensation.Teeth,
                spindle: RotationalSpeed.FromRevolutionsPerMinute(spindle),
                feed: Speed.FromMillimetersPerMinutes(cutting.Feed * compensation.Teeth * spindle))
            .Bind(cutting.Evaluate)
            .Map(load => compensation.Deflection(load.TangentialPerEdge.Newtons));
    }

    private static Fin<Seq<GNode>> CutPath(Loop loop, PostDialect dialect, CutConditioning policy) =>
        from pierce in Sample(loop, 0.0)
        from lead in Lead(loop, policy.Cut)
        from body in Walk(loop, dialect, policy)
        select Seq<GNode>(new GNode.Word(GCommand.Rapid,
                XY(lead.Head.Map(GNode.Target).IfNone(pierce)), None))
            .Concat(PierceBlock(policy.Cut, dialect))
            .Concat(lead.IsEmpty ? Seq<GNode>() : GNode.Moves(lead, pierce))
            .Concat(body);

    private static Fin<Seq<Move>> Lead(Loop loop, Option<CutPolicy> policy) => policy.Match(
        Some: cut => cut.Lead.Shape(cut.LeadRadiusMm).Match(
            Some: shape => ArcAlgebra.Apply(new ArcOp.Lead(loop, 0.0, cut.FeedMmPerMinute, shape,
                    loop.Winding() == Sign.Negative ? MaterialSide.Inside : MaterialSide.Outside,
                    LeadRole.Entry))
                .Bind(trace => trace is ArcTrace.Motion motion
                    ? Fin.Succ(motion.Receipt.Moves)
                    : Fin.Fail<Seq<Move>>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:lead-trace"))),
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
            ? Some(Range(0, (int)Math.Floor(total / cut.TabSpacingMm)).ToSeq().Map(index => cut.TabSpacingMm * (index + 0.5))
                .Map(center => new TabWindow(center - cut.TabWidthMm / 2.0, center + cut.TabWidthMm / 2.0))
                .Filter(window => window.Start > loop.Tolerance.Absolute.Value
                    && window.End < total - loop.Tolerance.Absolute.Value))
            : None).IfNone(Seq<TabWindow>());
        Seq<double> stations = toSeq(Range(0, loop.Spans).ToSeq().Map(index => loop.At(index).DistanceTo(loop.At(index + 1)))
            .Fold(Seq(0.0), static (state, length) => state.Add(state.Last.IfNone(0.0) + length))
            .Concat(tabs.Bind(static window => Seq(window.Start, window.End))).Add(total)
            .Distinct().OrderBy(static value => value));
        return Range(0, stations.Count - 1).ToSeq().Map(index =>
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
            : Fin.Fail<ProfileResult.Sampled>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:sample-result")));

    private static Fin<Point3d> Sample(Loop loop, double station) => Sampled(loop, station).Map(static result => result.Point);

    private static Fin<Seq<GNode>> FlushRun(Seq<Point3d> run, Option<FitPolicy> policy, double feed) => policy.Match(
        Some: fit => run.Count < fit.ProbeFloor || run.Zip(run.Skip(1)).Sum(static pair => pair.Item1.DistanceTo(pair.Item2)) < fit.MinimumRunMm
            ? Fin.Succ(Lines(run, feed))
            : Fit(run, fit, feed),
        None: () => Fin.Succ(Lines(run, feed)));

    // Exemption: the biarc fit is a numeric kernel — the tangent pair, the deviation probe, and the admission
    // verdict all read one constructed fit, and splitting them rebuilds it.
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
        _ => Fin.Fail<GNode>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:fit-curve:{curve.GetType().Name}")),
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

    // Exemption: the bulge-to-arc conversion is a numeric kernel — the provider resolves radius and centre from one
    // vertex pair, and the emitted word reads both.
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
            ? Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Posting, "post:tool-boundaries"))
            : Fin.Succ(unit)
        from placements in changes.TraverseM(change => change.Previous.IsNone
            ? Fin.Succ((Node: 0, Change: change))
            : policy.Tooling.Boundaries
                    .Filter(boundary => boundary.Op == change.Op && boundary.Node >= 0 && boundary.Node < nodes.Count
                        && boundary.Consumed.Find(change.LimitingBasis).Exists(consumed => consumed >= change.Trigger))
                    .Fold(Option<OperationBoundary>.None, static (best, boundary) =>
                        best.Filter(held => held.Node <= boundary.Node).IfNone(boundary))
                .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:tool-boundary:{change.Op.Key}:{change.LimitingBasis.Key}"))
                .Map(boundary => (boundary.Node, Change: change))).As()
        from sectioned in Range(0, nodes.Count).ToSeq().TraverseM(index => placements.Filter(row => row.Node == index)
            .TraverseM(row => SpindleNodes(policy.Cut.Cutting, row.Change.Assembly)
                .Map(spindle => ToolChangeNodes(row.Change).Concat(spindle).Concat(CoolingNodes(policy.Cut.Cooling)))).As()
            .Map(prefixes => prefixes.Bind(identity)
                .Add(ClampFeed(nodes[index], placements.Filter(row => row.Node <= index).Last.Map(static row => row.Change.Assembly.Feed))))).As()
        select sectioned.Bind(identity);

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

    // The ONE spindle law composed over the CUTTING diameter: `Process/physics#BUDGET_FOLD` owns `n = vc*1000/(pi*D)`
    // and the measured cutting diameter is what the cut actually sees. A shank diameter posts a surface speed the
    // edge never runs at, so a tool carrying no cutting measurement refuses rather than substituting the shank.
    private static Fin<Seq<GNode>> SpindleNodes(Option<CuttingData> cutting, ToolAssembly assembly) => cutting.Match(
        Some: data => assembly.Snapshot.Metric(ToolMeasure.CuttingDiameter)
            .OrElse(assembly.Snapshot.Metric(ToolMeasure.MaximumCuttingDiameter))
            .Filter(Witness.Positive)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Posting, $"post:cutting-diameter:{assembly.Key.Value}"))
            .Map(diameter => Seq<GNode>(new GNode.Word(GCommand.Spindle,
                Arr(GParam.Number('S', Clamp(SurfaceSpeed.Rpm(data.SurfaceSpeed, diameter), assembly.Spindle), ProgramUnits.Metric)),
                None))),
        None: () => Fin.Succ(Seq<GNode>()));

    private static Seq<GNode> CoolingNodes(CoolingPolicy cooling) => cooling.Word().Map(command =>
        (GNode)new GNode.Word(command, Arr<GParam>(), None)).ToSeq();

    private static GNode ClampFeed(GNode node, Option<ProcessRange> range) => node is GNode.Word word && word.P('F').IsSome
        ? word.With('F', range.Map(value => Clamp(word.P('F').IfNone(0.0), value)).IfNone(word.P('F').IfNone(0.0)))
        : node;

    // An absent bound is `None`, so the clamp reads what the range declares rather than an infinity standing in for
    // a bound the equipment never published.
    private static double Clamp(double requested, ProcessRange range) {
        double selected = Math.Min(requested, range.Resolve(requested));
        double floored = range.Minimum.Map(minimum => Math.Max(minimum, selected)).IfNone(selected);
        return range.Maximum.Map(maximum => Math.Min(maximum, floored)).IfNone(floored);
    }

    // Lookahead interprets the NODES it is handed: the prior form wrapped them in a keyed program, so every pass
    // that ran it paid a whole-tree serialization for a key it discarded.
    internal static Fin<Seq<GNode>> Lookahead(Seq<GNode> nodes, MotionDynamics dynamics) =>
        Interpret(nodes).Map(trace => {
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
            // Absence of a cap is `None`, so a word no cap names keeps its programmed feed rather than reading an
            // infinity a fold seeded.
            word: static (context, word) => context.Caps
                .Filter(cap => cap.Locus.SequenceEqual(context.Locus))
                .Map(static cap => cap.Feed)
                .Fold(Option<double>.None, static (held, feed) => Some(held.Map(value => Math.Min(value, feed)).IfNone(feed)))
                .Match(Some: feed => word.With('F', feed), None: () => word),
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

    // Exemption: the lookahead kernel is a measured numeric pass over one motion array — the forward and reverse
    // sweeps each read the caps the other wrote, so the arrays ARE the algorithm.
    private ref struct LookaheadKernel {
        private readonly ProgramEvent.Motion[] motions;
        private readonly MotionDynamics dynamics;
        private readonly double[] caps;
        private readonly double[] ceilings;
        private readonly double[] distances;
        private readonly bool[] cutting;
        private readonly Vector3d[] vectors;

        public LookaheadKernel(ProgramEvent.Motion[] motions, MotionDynamics dynamics) {
            this.motions = motions;
            this.dynamics = dynamics;
            caps = new double[motions.Length];
            ceilings = new double[motions.Length];
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
                // A span rides the ceiling its own SHAPE declares: the arc law bounds a circular span and the
                // linear law a straight one, so the block the machine cannot hold at its programmed rate is capped
                // by the limit that actually governs it.
                ceilings[index] = motion.Arc.IsSome ? dynamics.ArcFeed : dynamics.LinearFeed;
                caps[index] = cutting[index] ? motion.Word.P('F').IfNone(ceilings[index]) : ceilings[index];
            }
            for (int index = 0; index < motions.Length; index++)
                if (cutting[index])
                    caps[index] = Math.Min(caps[index], Junction(index));
            Sweep(0, motions.Length, 1);
            Sweep(motions.Length - 1, -1, -1);
            return Range(0, motions.Length).ToSeq().Filter(index => cutting[index])
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
            return turn <= 0.0 ? ceilings[index] : Math.Min(ceilings[index], dynamics.JunctionFeed(turn));
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

    // Every program opens on the drawing's keyed marks — part mark, heat number, shop tag — as one comment block
    // ahead of the frame assignments, so an operator verifies the material in the machine against the sheet the
    // program was posted from. Marks ride the dialect's verbatim comment channel and never an executable word, so a
    // controller that ignores comments loses nothing and no dialect needs a marking spelling of its own. A run with
    // no marks emits no block rather than an empty one.
    private static Seq<GNode> Prologue(SetupSchedule schedule, Map<string, Arr<ProfileMarking>> tags) =>
        Marks(tags) + schedule.Wcs.Map(assignment => (GNode)new GNode.CoordinateFrame(
            assignment,
            schedule.Setups[assignment.Setup].Mounting.Frame)).ToSeq();

    // Rows sort by name so two posts of one drawing emit byte-identical headers and a program diff reads as a real
    // change; a tag whose content carries several lines joins them under one row rather than fanning comment lines
    // a controller's line-length rule then truncates independently.
    private static Seq<GNode> Marks(Map<string, Arr<ProfileMarking>> tags) =>
        toSeq(tags.Fold(Seq<string>(), static (rows, name, marks) => rows + marks.ToSeq()
            .Choose(static mark => mark.Content is MarkingContent.Tag tag ? Some(tag.Type.Text.Replace('\n', ' ')) : None)
            .Map(text => $"{name}={text}"))
        .OrderBy(static row => row, StringComparer.Ordinal)) switch {
            { IsEmpty: true } => Seq<GNode>(),
            var rows => Seq<GNode>(new GNode.Block(
                new BlockFrame(None, None, false, false, None, rows, "marks"), Arr<GNode>())),
        };

    private static Arr<GParam> XY(Point3d point) => Arr(
        GParam.Number('X', point.X, ProgramUnits.Metric),
        GParam.Number('Y', point.Y, ProgramUnits.Metric));

    // An absent cut policy falls back to the machine's own straight-span law, which is the ceiling every fed block
    // rides where the job declares none.
    private static double FeedCeiling(CutConditioning policy) =>
        policy.Cut.Map(static value => value.FeedMmPerMinute).IfNone(policy.Dynamics.LinearFeed);
    private static double FeedFloor(CutConditioning policy) =>
        policy.Cut.Map(static value => value.FeedMmPerMinute * value.LinkFeedFactor).IfNone(policy.Dynamics.LinearFeed);

    private readonly record struct TabWindow(double Start, double End);
    private readonly record struct PathSegment(int Span, Point3d From, Point3d To, double Bulge, bool Tab);
}
```

## [07]-[PARSING]

- Owner: `Post.ParseRs274` owns block framing, comment and checksum extraction, the linear word split, and command resolution.
- Law: the word split is LINEAR. Folding with `Init`/`Last` over a `Seq` re-walked the accumulated segments per token, so a program of `n` words cost `n²` list traversals; carrying the open segment in the fold state costs one append per token.
- Law: resolution filters on the DIALECT and the program's MODALITY before the arity gate, so a token two rows share resolves to the one row the running modality serves. An unresolved or ambiguous token lands `ProgramTokenUnresolved` carrying its line and word, so an operator reads which token refused rather than a bare block number.
- Auto: `WireCode.Candidates` reads the prebuilt index, so the roster is never scanned per token.
- Exemption: `ParseBlock` is the framing statement kernel — comment, checksum, optional-delete, and program/sequence extraction all read one record text, and splitting them re-scans it.
- Boundary: parsed `Sequence` and `Checksum` values never survive re-emission, because `RecordFrame` owns numbering and digest.

```csharp signature
// --- [PARSING] ----------------------------------------------------------------------------------------------------------------------------------------
public static partial class Post {
    private static Fin<CutProgram> ParseRs274(ProgramIngress.Rs274 ingress) =>
        Lines(ingress.Source).FoldM<Fin, ParseState>(new ParseState(ModalState.Empty, Seq<GNode>()),
            (state, row) => from parsed in ParseBlock(row.Line, row.Text, ingress, state.Modal, state.Nodes.Count)
                            select new ParseState(parsed.Modal, state.Nodes.Add(parsed.Block)))
        .As().Bind(state => state.Nodes.Exists(static node => node is GNode.Block { Body.IsEmpty: false })
            ? Fin.Succ(CutProgram.Of(state.Nodes, ingress.Dialect))
            : Fin.Fail<CutProgram>(new FabricationFault.ProgramParse(0, ModalGroup.NonModal)));

    private static Fin<(GNode Block, ModalState Modal)> ParseBlock(
        int line, string text, ProgramIngress.Rs274 ingress, ModalState modal, int locus) {
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
        Option<uint> parsedChecksum = ingress.Checksum.Bind(rule => check.Success
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
            && (!check.Success || ingress.Dialect.Features.Contains(DialectFeature.Checksum)
                && ingress.Checksum.Exists(rule => parsedChecksum.Exists(value =>
                    value == rule.Digest(ingress.Codec.GetBytes(record[..check.Index])))));
        if (!frameValid || residue.Any(static character => !char.IsWhiteSpace(character)))
            return Fin.Fail<(GNode, ModalState)>(new FabricationFault.ProgramParse(line, ModalGroup.NonModal));
        Option<int> program = NumberToken(tokens, 'O');
        Option<int> sequence = NumberToken(tokens, 'N');
        Seq<string> words = tokens.Filter(static token => char.ToUpperInvariant(token[0]) is not 'O' and not 'N').ToSeq();
        BlockFrame frame = new(program, sequence, optional, false, checksum, comments, text);
        ModalState entered = modal with { Events = modal.Events.Add(new ProgramEvent.Boundary(at, frame)) };
        return ParseWords(line, words, ingress, entered, at).Bind(parsed => {
            GNode.Block block = new(frame, parsed.Nodes.ToArr());
            return ModalState.AdmitBlock(line, block.Body).Map(_ => ((GNode)block, parsed.Modal));
        });
    }

    // ONE linear pass: the open segment rides the fold state, so a token appends to it directly instead of
    // re-walking the accumulated segment list to replace its tail.
    private static Fin<ParseState> ParseWords(
        int line, Seq<string> tokens, ProgramIngress.Rs274 ingress, ModalState modal, ProgramLocus locus) {
        (Seq<CommandSegment> Closed, Option<CommandSegment> Open, Seq<string> Leading) split = tokens.Fold(
            (Closed: Seq<CommandSegment>(), Open: Option<CommandSegment>.None, Leading: Seq<string>()),
            (state, token) => WireCode.Known(token)
                ? (state.Closed.Concat(state.Open.ToSeq()), Some(new CommandSegment(token, Seq<string>())), state.Leading)
                : state.Open.Match(
                    Some: open => (state.Closed, Some(open with { Parameters = open.Parameters.Add(token) }), state.Leading),
                    None: () => (state.Closed, state.Open, state.Leading.Add(token))));
        Seq<CommandSegment> segments = split.Closed.Concat(split.Open.ToSeq());
        return tokens.IsEmpty
            ? Fin.Succ(new ParseState(modal, Seq<GNode>()))
            : segments.Head.Match(
                Some: head => Seq(head with { Parameters = split.Leading.Concat(head.Parameters) })
                    .Concat(segments.Tail)
                    .FoldM<Fin, ParseState>(new ParseState(modal, Seq<GNode>()), (state, segment) =>
                        from command in Resolve(line, segment, ingress)
                        let normalized = NormalizeWcs(command, segment)
                        from parameters in normalized.TraverseM(token => ParseParam(line, token, command, state.Modal)).As()
                        from admitted in command.Admit(line, parameters.ToArr())
                        let node = (GNode)new GNode.Word(command, admitted, Feed(command))
                        from next in state.Modal.Push(locus.Descend(state.Nodes.Count), node)
                        select new ParseState(next, state.Nodes.Add(node)))
                    .As(),
                None: () => Fin.Fail<ParseState>(new FabricationFault.ProgramParse(line, ModalGroup.NonModal)));
    }

    // Dialect capability, then the PROGRAM's modality, then address shape — the modality filter is what separates
    // two rows sharing one wire code, so it runs before the arity gate rather than after it.
    private static Fin<GCommand> Resolve(int line, CommandSegment segment, ProgramIngress.Rs274 ingress) {
        Seq<GCommand> candidates = WireCode.Candidates(segment.Command)
            .Concat(BaseWcs(segment.Command).Map(static _ => GCommand.Wcs).ToSeq())
            .Distinct();
        Seq<char> addresses = segment.Parameters
            .Filter(static value => value.Length > 1)
            .Map(static value => char.ToUpperInvariant(value[0])).ToSeq();
        Seq<GCommand> admitted = candidates
            .Filter(command => command.Admits(ingress.Dialect))
            .Filter(command => command.Serves(ingress.Modality))
            .Filter(command => command.Grammar.Fits(addresses));
        return admitted.Count == 1
            ? admitted.Head.ToFin(new FabricationFault.ProgramTokenUnresolved(line, segment.Command))
            : Fin.Fail<GCommand>(new FabricationFault.ProgramTokenUnresolved(line, segment.Command));
    }

    private static Seq<string> NormalizeWcs(GCommand command, CommandSegment segment) =>
        command == GCommand.Wcs && !segment.Parameters.Exists(static value => value.StartsWith('P'))
            ? BaseWcs(segment.Command)
                .Map(ordinal => segment.Parameters.Add($"P{ordinal.ToString(CultureInfo.InvariantCulture)}"))
                .IfNone(segment.Parameters)
            : segment.Parameters;

    private static Option<int> BaseWcs(string token) =>
        int.TryParse(WireCode.Of(token).AsSpan(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int code)
        && code is >= 54 and <= 59
            ? Some(code - 53)
            : None;

    private static Fin<GParam> ParseParam(int line, string token, GCommand command, ModalState modal) {
        if (token.Length < 2)
            return Fin.Fail<GParam>(new FabricationFault.ProgramParse(line, command.Group));
        char address = char.ToUpperInvariant(token[0]);
        string lexeme = token[1..];
        if (lexeme.StartsWith('#') && int.TryParse(lexeme[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int variable))
            return Fin.Succ(new GParam(address, new GValue.Variable(variable, lexeme)));
        if (lexeme.StartsWith('[') && lexeme.EndsWith(']'))
            return Fin.Succ(new GParam(address, new GValue.Expression(lexeme)));
        if (!double.TryParse(lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out double received))
            return Fin.Fail<GParam>(new FabricationFault.ProgramParse(line, command.Group));
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

    private static Seq<(int Line, string Text)> Lines(string source) =>
        toSeq(source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n')
            .Select((text, index) => (Line: index + 1, Text: text)));

    private readonly record struct CommandSegment(string Command, Seq<string> Parameters);
    private readonly record struct ParseState(ModalState Modal, Seq<GNode> Nodes);

    [GeneratedRegex(@"\([^)]*\)|;[^\r\n]*")]
    private static partial Regex CommentText { get; }

    [GeneratedRegex(@"[A-Za-z]+(?:#[0-9]+|\[[^\]]+\]|[+-]?(?:\d+(?:\.\d*)?|\.\d+))")]
    private static partial Regex WordText { get; }

    [GeneratedRegex(@"\*([0-9A-Fa-f]+)\s*$")]
    private static partial Regex ChecksumText { get; }
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
