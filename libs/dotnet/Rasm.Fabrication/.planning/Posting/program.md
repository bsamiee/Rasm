# [RASM_FABRICATION_PROGRAM]

`Post` owns one dialect-neutral `CutProgram` — the command vocabulary every controller resolves against, the AST that carries it, the RS274 parse that reconstructs it, and the four boundaries that lower, parse, publish, and interpret it. `GNode.Directive` preserves controller directives and specialized toolpath evidence beside motion; `GWord.Render` is the physical-record correspondence capacity checks and results consume.

`PostSource`, `PostDialect`, `EmitPolicy`, `ContentKey`, `WcsAssignment`, and the `Process/atoms` `Move`/`MotionDirective` floor arrive as settled contracts. `Posting/conditioning#ADMISSION` owns `PostPolicy` and every dimensioned cut, fit, and compensation column, and `Posting/conditioning#CONDITIONING` owns the assembly fold `Post.Lower` composes — this page names those owners and declares none of their columns. `NodeKey` is the ONE structural identity over the AST — a per-node `UInt128` through the `Process/owner#RUN_DISPATCH` `FabricationCanon.Ordered` close, so a pass fold pays one digest per node it changed rather than a full serialization per intermediate tree. A process names NO dialect: the controller is a property of the machine, so `PostDialect.Admits(ProcessModality)` resolves every pairing and the resolving modality rides `ProgramIngress` where two command rows share one wire code.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `ProgramUnits`, `DistanceMode`, `ModalGroup`, `FeedMode`, `CoolingLaw`, `LeadStyle`, `WordValueLaw`, `MotionRole`, `CommandGrammar`, `GCommand`, and the wire-code index every resolution reads.
- [03]-[AST]: `GValue`, `GParam`, `GNode`, `NodeKey`, `GWord`, `ProgramRender`, `ProgramLocus`, `ProgramEvent`, `ProgramTrace`, `ModalState`, and `CutProgram`.
- [04]-[PARSE]: `ProgramIngress`, RS274 block framing, the linear word split, and command resolution against the wire-code index.
- [05]-[BOUNDARIES]: `ProgramView`, `Post.Lower`, `Post.Parse`, `Post.Publish`, and `Post.Interpret`.

## [02]-[VOCABULARY]

- Owner: `GCommand` owns the closed command roster with its grammar, modal group, motion role, demanded features, and admitting modalities; `CommandGrammar` owns address shape; `WordValueLaw` owns which `GValue` shapes an address admits; `WireCode` owns the normalized token identity every resolution keys on.
- Law: `GCommand.Requires` and `GCommand.Modalities` declare what a command demands of a controller, and `GCommand.Admits` decides admissibility against `PostDialect.Features` and `PostDialect.Modalities` — no dialect identity is ever tested, and no roster mirrors the vocabulary.
- Law: two rows MAY share one wire code where their modalities are disjoint — `M7` is mist coolant on a contact controller and torch-on on a thermal one. The resolving discriminant is the PROGRAM's own `ProcessModality`, which `ProgramIngress` carries, so a hybrid controller admitting both modalities still resolves each token to exactly one row; filtering on the dialect's whole modality SET left both rows standing and refused the program the two rows exist to serve.
- Law: `CoolingLaw` and `WordValueLaw` carry no content key, evidence band, or stamp, so neither takes a `*Policy` name the branch reserves for a `FabricationPolicy` payload — each is a delegate row whose behaviour IS its column.
- Auto: the wire-code index is built ONCE from `GCommand.Items` and keyed by normalized code, so resolution costs one lookup per token rather than a scan of the roster per token.
- Growth: a command is one `GCommand` row with its grammar and demanded features; a modal family is one `ModalGroup` row.
- Boundary: dialect byte spelling stays in `Dialect`; this cluster declares codes as ROW data and renders none.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Projection;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] ---------------------------------------------------------------------------
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
public sealed partial class CoolingLaw {
    public static readonly CoolingLaw Off = new("off", static () => None);
    public static readonly CoolingLaw Mist = new("mist", static () => Some(GCommand.CoolantMist));
    public static readonly CoolingLaw Flood = new("flood", static () => Some(GCommand.Coolant));

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
public sealed partial class WordValueLaw {
    public static readonly WordValueLaw Literal = new(static value => value is GValue.Number or GValue.Integer);
    public static readonly WordValueLaw Symbolic = new(static _ => true);

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

public sealed record CommandGrammar(Set<char> Required, Set<char> Allowed, Set<char> Repeatable, WordValueLaw Values) {
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
    private static readonly CommandGrammar Empty = new(Set<char>(), Set<char>(), Set<char>(), WordValueLaw.Literal);

    public static readonly GCommand Rapid = MotionRow("rapid", "G0", MotionRole.Control);
    public static readonly GCommand Feed = MotionRow("feed", "G1", MotionRole.Cutting);
    public static readonly GCommand ArcCw = new("arc-cw", "G2", ModalGroup.Motion,
        new CommandGrammar(Set<char>(), Arc, Set<char>(), WordValueLaw.Symbolic), MotionRole.Cutting,
        Set<DialectFeature>(), Set<ProcessModality>(), None);
    public static readonly GCommand ArcCcw = new("arc-ccw", "G3", ModalGroup.Motion,
        new CommandGrammar(Set<char>(), Arc, Set<char>(), WordValueLaw.Symbolic), MotionRole.Cutting,
        Set<DialectFeature>(), Set<ProcessModality>(), None);
    public static readonly GCommand Extrude = new("extrude", "G1", ModalGroup.Motion,
        new CommandGrammar(Set('E'), Extrusion, Set<char>(), WordValueLaw.Symbolic), MotionRole.Additive,
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
        new CommandGrammar(Set('P'), Set('P', 'A', 'R'), Set<char>(), WordValueLaw.Symbolic), MotionRole.None,
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
    public static readonly GCommand Dwell = new("dwell", "G4", ModalGroup.NonModal,
        new CommandGrammar(Set<char>(), Set('P', 'X', 'U'), Set<char>(), WordValueLaw.Symbolic), MotionRole.None,
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

    public bool Admits(PostDialect dialect) =>
        Requires.ForAll(dialect.Features.Contains)
        && (Modalities.IsEmpty || Modalities.Exists(dialect.Modalities.Contains));

    public bool Serves(ProcessModality modality) => Modalities.IsEmpty || Modalities.Contains(modality);

    private static GCommand MotionRow(string key, string code, MotionRole role, params ReadOnlySpan<DialectFeature> requires) =>
        new(key, code, ModalGroup.Motion, new CommandGrammar(Set<char>(), Motion, Set<char>(), WordValueLaw.Symbolic),
            role, Set(requires.ToArray()), Set<ProcessModality>(), None);
    private static GCommand StateRow(string key, string code, ModalGroup group, params ReadOnlySpan<DialectFeature> requires) =>
        new(key, code, group, Empty, MotionRole.None, Set(requires.ToArray()), Set<ProcessModality>(), None);
    private static GCommand Aux(string key, string code, ModalGroup group, Set<char> allowed, params ReadOnlySpan<DialectFeature> requires) =>
        new(key, code, group, new CommandGrammar(Set<char>(), allowed, Set<char>(), WordValueLaw.Symbolic),
            MotionRole.None, Set(requires.ToArray()), Set<ProcessModality>(), None);
    private static GCommand CycleRow(string key, string code, Set<char> required, params ReadOnlySpan<DialectFeature> requires) => new(code,
        ModalGroup.Cycle,
        new CommandGrammar(required, required + Axes + Set('P', 'Q', 'L'), Set<char>(), WordValueLaw.Symbolic),
        MotionRole.None,
        Set(requires.ToArray()),
        Set<ProcessModality>(),
        None);
}

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

## [03]-[AST]

- Owner: `CutProgram` mints the canonical AST and `Post` owns every transform that changes it; `NodeKey` owns structural identity; `ModalState` owns the one semantic walk; `ProgramRender` owns the rendered-record accumulation and the modal census that decides what a record repeats.
- Cases: `GNode` carries block framing beside executable node families; `GValue` preserves numeric, variable, expression, and text evidence; `GCommand.Wcs` and `WcsExtended` retain base and extended coordinate forms; `ProgramEvent` carries the canonical interpretation.
- Law: `NodeKey` is the ONE structural identity and it rides the `FabricationCanon.Ordered` streaming close — the branch's own order-only digest over the `Rasm.Element` `CanonicalWriter`. A second byte codec beside that writer is the deleted form, the string-framed concatenation it replaces was exactly that, and a `new CanonicalWriter(...)` spelling names no member at all because the writer's constructor is private. A node's key digests its own subtree, so a rewriting pass re-keys only what it changed and an optimization fold reading a stream of `UInt128` keys pays no serialization at all.
- Law: `CutProgram.Key` is HELD, derived on first read from the node keys and the dialect through the `FabricationCanon.Keyed` retaining close. A pass chain minting seven intermediate programs paid seven whole-tree serializations for keys six of them never published; a held `Fin<ContentKey>` also holds the close's own refusal, so a caller reads the same verdict on every read.
- Law: `ProgramUnits` carries one millimetre scale in both directions, and `GNode.Word.With` preserves the replaced value's source units or the word's established source-unit row. Every `ProgramEvent` carries one structural `ProgramLocus`; motion also carries every admitted axis, resolved plane, and arc center, so consumers never re-derive modal state, discard rotary or auxiliary axes, or substitute a chord.
- Law: AST scalars stay bare `double` in canonical millimetres. Every one of them enters a `NodeKey` preimage, and the branch ruling holds a digested column to the raw scalar — a typed quantity here moves the preimage and re-keys every program the shop has already posted.
- Auto: `GCommand.Admit` composes address shape with row-owned scalar policy before AST construction, and `ModalState` threads controller state once.
- Result: `CutProgram.Key` identifies the AST; admitted `ProgramTrace` preserves modal state and the complete node-and-repeat path of every expanded executable leaf.
- Exemption: `ModalState.Apply` and `ArcOf` are the modal statement kernels — the arc-centre resolution is a numeric boundary where plane and arc-distance rows are simultaneously in hand.
- Packages: `Rasm.Element` `CanonicalWriter` through `Process/owner#RUN_DISPATCH` `FabricationCanon`; `LanguageExt.Core` `Fin`, `FoldM`, `Seq`, and `Map`; `Thinktecture.Runtime.Extensions` generated unions and smart enums.
- Growth: a syntax construct is one `GNode` case, one `NodeKey` arm, and one `ModalState.Push` arm.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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
    public sealed record Nc1(ImportedSteel Import) : GNode;
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

public static class NodeKey {
    public static double Grid(PostDialect dialect) => Math.Pow(10.0, -dialect.Decimals);

    public static UInt128 Of(GNode node, double grid) =>
        FabricationCanon.Ordered(grid, writer => Write(writer, node));

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
        nc1: static (row, value) => value.Import.Key.CanonicalBytes(row.String("nc1")),
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

    public static Fin<ProgramRender> Render(Seq<GWord> words) =>
        Render(words, ProgramRender.Empty);

    private static Fin<ProgramRender> Render(Seq<GWord> words, ProgramRender state) =>
        words.FoldM<Fin, ProgramRender>(state, static (current, word) => RenderWord(word, current)).As();

    private static Fin<ProgramRender> RenderWord(GWord word, ProgramRender state) => word.Switch(
        state: state,
        address: static (current, value) => Fin.Succ(AddressRecords(current, value)),
        conversational: static (current, value) => Fin.Succ(current.Add(value.Records)),
        text: static (current, value) => Fin.Succ(current.Add(value.Records)),
        cycleCall: static (current, value) => Fin.Succ(current.Add(value.Records)),
        macro: static (current, value) => Render(value.Body, current.Add(value.Open)).Map(rendered => rendered.Add(value.Close)),
        subprogram: static (current, value) => Render(value.Body, current.Add(value.Open)).Map(rendered => rendered.Add(value.Close)),
        additive: static (current, value) => Fin.Succ(current.Add(value.Records)),
        nc1: static (current, value) => Fin.Succ(current.Add(value.Records)),
        fault: static (_, value) => Fin.Fail<ProgramRender>(value.Error),
        expanded: static (current, value) => Render(value.Words, current));

    private static ProgramRender AddressRecords(ProgramRender state, Address word) {
        bool emitMode = word.Mode.Exists(mode => word.Retention == WordRetention.Explicit
            || !state.Active.Find(ModalGroup.Feed).Contains(mode.Code));
        Map<ModalGroup, string> withMode = word.Mode.Map(mode => state.Active.AddOrUpdate(ModalGroup.Feed, mode.Code)).IfNone(state.Active);
        bool emitCode = word.Group == ModalGroup.NonModal || word.Retention == WordRetention.Explicit || !withMode.Find(word.Group).Contains(word.Code);
        string line = string.Join(" ", Seq(emitMode ? word.Mode.Map(static mode => mode.Code).IfNone(string.Empty) : string.Empty,
                emitCode ? word.Code : string.Empty)
            .Concat(word.Words.Map(Format)).Filter(static token => token.Length > 0).ToArray());
        Map<ModalGroup, string> next = word.Group == ModalGroup.NonModal ? withMode : withMode.AddOrUpdate(word.Group, word.Code);
        return line.Length == 0 ? state with { Active = next } : new ProgramRender(state.Lines.Add(line), next);
    }

    private static string Format(GParam parameter) => $"{parameter.Address}{Value(parameter.Value)}";
    private static string Value(GValue value) => value.Switch(
        number: static item => item.SourceUnits.Native(item.Canonical).ToString("R", CultureInfo.InvariantCulture),
        integer: static item => item.Value.ToString(CultureInfo.InvariantCulture),
        variable: static item => item.Lexeme,
        expression: static item => item.Lexeme,
        text: static item => item.Value);
}

public sealed record ProgramRender(Seq<string> Lines, Map<ModalGroup, string> Active) {
    public static readonly ProgramRender Empty = new(Seq<string>(), Map<ModalGroup, string>());
    public ProgramRender Add(string line) => line.Length == 0 ? this : this with { Lines = Lines.Add(line) };
    public ProgramRender Add(Seq<string> lines) => this with { Lines = Lines.Concat(lines.Filter(static line => line.Length > 0)) };
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
    public sealed record Exchange(ProgramLocus locus, ImportedSteel Import) : ProgramEvent(locus);
    public sealed record Directive(ProgramLocus locus, MotionDirective Value) : ProgramEvent(locus);
}

public sealed record ProgramTrace {
    private ProgramTrace(ModalState final) => Final = final;

    public ModalState Final { get; }
    public Seq<ProgramEvent> Events => Final.Events;

    internal static Fin<ProgramTrace> Admit(Seq<GNode> nodes) => nodes.IsEmpty
        ? Fin.Fail<ProgramTrace>(new KernelFault.InvalidValue("program", "post:trace-empty"))
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
        nc1: static (context, value) => Fin.Succ(context.State with { Events = context.State.Events.Add(new ProgramEvent.Exchange(context.Locus, value.Import)) }),
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

public sealed class CutProgram {

    private Fin<ContentKey>? key;
    private Seq<UInt128>? keys;

    private CutProgram(Seq<GNode> nodes, PostDialect dialect) => (Nodes, Dialect) = (nodes, dialect);

    public Seq<GNode> Nodes { get; }
    public PostDialect Dialect { get; }

    public static CutProgram Of(Seq<GNode> nodes, PostDialect dialect) => new(nodes, dialect);

    public Seq<UInt128> Keys => keys ??= NodeKey.Stream(Nodes, NodeKey.Grid(Dialect));

    public Fin<ContentKey> Key => key ??= FabricationCanon.Keyed(
        EgressKind.CutProgram,
        NodeKey.Grid(Dialect),
        writer => Keys.Fold(writer.Discriminant(Dialect).Ordinal(Keys.Count), static (row, node) => row.U128(node)),
        Mint);
}
```

## [04]-[PARSE]

- Owner: `ProgramIngress` owns the parse grammar and the modality that resolves it; `Post.ParseRs274` owns block framing, comment and checksum extraction, the linear word split, and command resolution.
- Cases: `Rs274` carries source text, dialect, the program's own modality, the text codec, and an optional checksum rule; `Nc1` carries the steel source, its contour policy, and the dialect.
- Law: `ProgramIngress.Rs274` carries the program's own `ProcessModality` because two command rows may share one wire code under disjoint modalities. Without it a hybrid controller resolves such a token to two candidates and refuses the program.
- Law: the word split is LINEAR. Folding with `Init`/`Last` over a `Seq` re-walked the accumulated segments per token, so a program of `n` words cost `n²` list traversals; carrying the open segment in the fold state costs one append per token.
- Law: resolution filters on the DIALECT and the program's MODALITY before the arity gate, so a token two rows share resolves to the one row the running modality serves. An unresolved or ambiguous token lands `ProgramTokenUnresolved` carrying its line and word, so an operator reads which token refused rather than a bare block number.
- Auto: `WireCode.Candidates` reads the prebuilt index, so the roster is never scanned per token.
- Exemption: `ParseBlock` is the framing statement kernel — comment, checksum, optional-delete, and program/sequence extraction all read one record text, and splitting them re-scans it.
- Boundary: parsed `Sequence` and `Checksum` values never survive re-emission, because `RecordFrame` owns numbering and digest.

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramIngress {
    private ProgramIngress() { }

    public sealed record Rs274(
        string Source,
        PostDialect Dialect,
        ProcessModality Modality,
        Encoding Codec,
        Option<ChecksumRule> Checksum) : ProgramIngress;
    public sealed record Nc1(SteelSource Source, SteelContourPolicy Policy, PostDialect Dialect) : ProgramIngress;
}

// --- [PARSING] -------------------------------------------------------------------------
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
        Seq<string> comments = toSeq(CommentText.Matches(text).Select(static match => match.Value));
        string body = CommentText.Replace(text, string.Empty).Trim();
        if (body.Length == 0 || body == "%") {
            GNode.Block empty = new(
                new BlockFrame(None, None, false, body == "%", None, comments, text), Arr<GNode>());
            return modal.Push(at, empty).Map(next => ((GNode)empty, next));
        }
        bool optional = body.StartsWith("/", StringComparison.Ordinal);
        string opened = optional ? body[1..].TrimStart() : body;
        Seq<string> tokens = toSeq(WordText.Matches(opened).Select(static match => match.Value));
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
            : segments.Head
                .ToFin(new FabricationFault.ProgramParse(line, ModalGroup.NonModal))
                .Bind(head => Seq(head with { Parameters = split.Leading.Concat(head.Parameters) })
                    .Concat(segments.Tail)
                    .FoldM<Fin, ParseState>(new ParseState(modal, Seq<GNode>()), (state, segment) =>
                        from command in Resolve(line, segment, ingress)
                        let normalized = NormalizeWcs(command, segment)
                        from parameters in normalized.TraverseM(token => ParseParam(line, token, command, state.Modal)).As()
                        from admitted in command.Admit(line, parameters.ToArr())
                        let node = (GNode)new GNode.Word(command, admitted, Feed(command))
                        from next in state.Modal.Push(locus.Descend(state.Nodes.Count), node)
                        select new ParseState(next, state.Nodes.Add(node)))
                    .As());
    }

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

## [05]-[BOUNDARIES]

- Owner: `Post` composes admitted policy and sibling domain values; `ProgramView` owns the motion-role partition every geometry egress reads off a trace.
- Cases: `ProgramView` closes all-motion, cutting, control, probing, and additive as one `Option<MotionRole>` column, so a view is one row rather than a predicate the caller writes.
- Entry: `Lower`, `Parse`, and `Publish` each discriminate on an input value rather than an overload or a mode flag; `Post.Assemble` at `Posting/conditioning#CONDITIONING` is the fold `Lower` composes, and `PostPolicy` arrives admitted from `Posting/conditioning#ADMISSION`.
- Auto: RS274 token coverage fails closed on `ProgramTokenUnresolved`, NC1 enters through `SteelImport.Read`, and every egress key derives from its complete payload.
- Result: `ProgramView.Paths` returns the run partition directly — a coordinate change re-frames every following point, so it closes a run exactly as an excluded move does, and the open run carries as `Option` rather than a null cursor.
- Boundary: `Eff<CutProgram>` carries source acquisition; reusable transforms retain `Fin<T>`; rendered records collapse only at `PostedProgram`; every parameter arrives admitted, so no entry guards a null.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ProgramView {
    public static readonly ProgramView AllMotion = new("all-motion", None);
    public static readonly ProgramView Cutting = new("cutting", Some(MotionRole.Cutting));
    public static readonly ProgramView Control = new("control", Some(MotionRole.Control));
    public static readonly ProgramView Probing = new("probing", Some(MotionRole.Probing));
    public static readonly ProgramView Additive = new("additive", Some(MotionRole.Additive));

    public Option<MotionRole> Role { get; }

    public Seq<ToolpathPath> Paths(ProgramTrace trace) {
        (Seq<ToolpathPath> Paths, Option<ToolpathPath> Current) folded = trace.Events.Fold(
            (Paths: Seq<ToolpathPath>(), Current: Option<ToolpathPath>.None),
            (state, item) => item switch {
                ProgramEvent.Motion motion when Role.ForAll(role => role == motion.Role) =>
                    (state.Paths, Some(state.Current.Match(
                        Some: held => held with { Spans = held.Spans.Add(Span(motion)) },
                        None: () => new ToolpathPath(motion.From, Seq(Span(motion)))))),
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

// --- [OPERATIONS] ----------------------------------------------------------------------
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
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("program", $"post:image-kind:{image.Kind.Key}"))
        select new FabricationResult.PostedProgram(image.Records, image.Key);

    public static Eff<CutProgram> Parse(ProgramIngress ingress) => ingress.Switch(
        rs274: static source => ParseRs274(source).ToEff(),
        nc1: static source => SteelImport.Read(source.Source, source.Policy)
            .Map(result => CutProgram.Of(Seq<GNode>(new GNode.Nc1(result)), source.Dialect)));

    public static Fin<Seq<EncodedGeometry>> Publish(CutProgram program, ProgramView view, PackPolicy policy) =>
        from trace in Interpret(program)
        let paths = view.Paths(trace)
        from _ in !paths.IsEmpty && paths.ForAll(static path => !path.Spans.IsEmpty)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("program", "post:publish-points"))
        from encoded in paths.TraverseM(path => Encode.Apply(new PackOp.Toolpath(path, policy))).As()
        select encoded;

    public static Fin<ProgramTrace> Interpret(CutProgram program) => ProgramTrace.Admit(program.Nodes);

    public static Fin<ProgramTrace> Interpret(Seq<GNode> nodes) => ProgramTrace.Admit(nodes);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
