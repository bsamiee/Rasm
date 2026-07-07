# [RASM_FABRICATION_DIALECT]

`Dialect` owns the byte-lowering half of `Run(Post)` as one generated total `Switch` over the `PostDialect` grammar table. `Process/family#PROCESS_FAMILY` owns dialect capability data; `Posting/program#CUT_PROGRAM` owns the dialect-neutral `GNode` AST; this page owns the lowering folds that expand nodes into grammar-true `GWord` output, including canned-cycle expansion, macro and subprogram lowering, command-code override resolution, additive layer-stack words, and the NC1 DSTV record-tree target. The fold emits node expansions, never string assembly, so one conditioned `Motion` posts to Fanuc, Haas, GRBL, Klartext, and Marlin as byte-diverse controller files from the same AST.

## [01]-[INDEX]

- [01]-[DIALECT_EMIT]: owns `Dialect.Emit(PostDialect, GNode)`, the generated total dialect switch, the word-address, conversational, additive, macro, subprogram, canned-cycle, override, NC1 record-tree, and block-cap internal folds, plus the route for `DialectUnsupported` 2718 and `BlockCapExceeded` 2728.

## [02]-[DIALECT_EMIT]

- Owner: `Dialect` the static lowering owner; `Emit` the single public entry; `WordAddress` the RS-274 branch; `Conversational` the Klartext, Mazatrol, and OSP branch; `Additive` the Marlin/RepRap branch; `Nc1` the DSTV steel mirror; `Seal` the internal block-cap fold. `PostDialect` supplies capability columns, `GNode` supplies AST shape, and `Dialect` supplies lowering decisions.
- Cases: `PostDialect` composes 13 family rows; `CycleGrammar` lowers `single-block`, `expanded`, and `dialect-cycle`; `MacroGrammar` lowers Macro-B, R-param, Q-param, and user-task; `SubprogramGrammar` lowers M98 and label units; `GNodeKind` routes word, cycle, macro, subprogram, additive-layer, and NC1; `GCommand` codes resolve through `CodeOverride`.
- Entry: `public static GWord Dialect.Emit(PostDialect dialect, GNode node)` is the only public dialect fold. The generated `PostDialect.Switch` is total; a new dialect row fails until its arm lands. `Run(Post{Motion, PostDialect})` invokes this entry inside the owner case body after `Posting/program` assembles the neutral AST from `Motion`; `Motion` receives no second kerf pass.
- Auto: cycles consult `Cycles`; macros consult `Macro`; subprograms consult `Subprogram`; arcs consult `Arc`; precision consults `Decimals`; modal elision consults `Modal`; WCS lowering consults `Wcs`; command words consult `CodeOverride`. Unsupported pairs lower to `DialectUnsupported`; program size runs through `Seal` and routes `BlockCapExceeded` without a `GNodeKind`.
- Receipt: `GWord` is the typed lowered word evidence. `GWord.Address` carries word-address rows, `GWord.CycleCall` carries named-cycle payloads, `GWord.Macro` carries grammar-specific macro payloads, `GWord.Subprogram` carries call/return payloads, `GWord.Additive` carries extrusion and temperature words, `GWord.Nc1` carries the DSTV block record tree plus `ContentKey.Of(EgressKind.Nc1, canonicalBytes)`, and `GWord.Fault` carries typed rail failure evidence for the surrounding `Run(Post)` fold.
- Packages: `Process/family#PROCESS_FAMILY` (`PostDialect`, `PostFamily`, `CycleGrammar`, `MacroGrammar`, `SubprogramGrammar`, `ArcMode`, `WcsRoster`); `Posting/program#CUT_PROGRAM` (`GNode`, `GNodeKind`, `GCommand`, `GWord`, `FeedMode`, cycle, macro, subprogram, and additive node rows); `Ingress/steel#STEEL_IMPORT` (`SteelHeader`, `SteelFeature`, `SteelContour`, `SteelBendSeed`, `SteelPart`); `Process/owner#FABRICATION_OWNER` (`EgressKind.Nc1`, `ContentKey`); `Process/faults#FAULT_BAND` (`DialectUnsupported`, `BlockCapExceeded`); LanguageExt.Core; Thinktecture.Runtime.Extensions; BCL inbox.
- Growth: conversational-dialect breadth lands as one `PostDialect` row plus one `Switch` arm reusing `Conversational`; a controller word deviation lands as one `CodeOverrides` row; a cycle convention lands as one `CycleGrammar` row and one `Cycle` arm; a macro convention lands as one `MacroGrammar` row and one `Macro` arm; a subprogram convention lands as one `SubprogramGrammar` row and one `Subprogram` arm; an NC1 record extension lands as one `SteelFeature` mirror arm under `Nc1`.
- Boundary: dialect data stays on `Process/family`; program AST stays on `Posting/program`; steel read projection stays on `Ingress/steel`; egress hashing stays on `ContentKey.Of`; this page owns lowering only. The byte-diversity kill-test is binding: one `Motion` posted to Fanuc, Haas, GRBL, Klartext, and Marlin yields five grammar-true byte-diverse files; a dialect-invariant `Code` table such as `thread-cycle -> "G92"` for every controller is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [MODELS] ------------------------------------------------------------------------
public sealed record Nc1Header(
    string OrderIdentification,
    string DrawingIdentification,
    string PhaseIdentification,
    string PieceIdentification,
    int QuantityOfPieces,
    string Profile,
    string ProfileCode,
    string SteelQuality,
    double Length,
    double SawLength,
    double ProfileHeight,
    double FlangeWidth,
    double FlangeThickness,
    double WebThickness,
    double Radius);

public readonly record struct Nc1Point(Point3d At, bool IsNotch, double RadiusMm);

public sealed record Nc1Contour(SteelBlockKind Kind, Arr<Nc1Point> Points);

public readonly record struct Nc1Bend(Edge3 Line, string Flange, double ThicknessMm, int SourceLine);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Nc1Feature {
    private Nc1Feature() { }

    public sealed record Hole(Point3d Center, string Flange, double DiameterMm, double DepthMm) : Nc1Feature;
    public sealed record Slot(Point3d Center, string Flange, double DiameterMm, double DepthMm, double SlotLengthMm, double SlotWidthMm, double SlotAngleDeg) : Nc1Feature;
    public sealed record Cut(Point3d At, string Flange) : Nc1Feature;
    public sealed record Numeration(Point3d At, string Flange) : Nc1Feature;
    public sealed record Contour(Nc1Contour Row) : Nc1Feature;
    public sealed record Bend(Nc1Bend Row) : Nc1Feature;
}

public sealed record Nc1Program(Nc1Header Header, Arr<Nc1Feature> Features, Arr<Nc1Contour> Contours, Arr<Nc1Bend> Bends) {
    public byte[] CanonicalBytes() =>
        Encoding.UTF8.GetBytes(string.Join("\n",
            Seq(
                Header.OrderIdentification,
                Header.DrawingIdentification,
                Header.PhaseIdentification,
                Header.PieceIdentification,
                Header.Profile,
                Header.ProfileCode,
                Header.SteelQuality,
                $"{Header.QuantityOfPieces}:{Header.Length:0.###}:{Header.SawLength:0.###}",
                $"{Header.ProfileHeight:0.###}:{Header.FlangeWidth:0.###}:{Header.FlangeThickness:0.###}:{Header.WebThickness:0.###}:{Header.Radius:0.###}")
            .Concat(Features.Map(FeatureKey))
            .Concat(Contours.Map(ContourKey))
            .Concat(Bends.Map(BendKey))
            .ToArray()));

    // Every field distinguishing two NC1 programs serializes — a kind-only label collapses distinct holes,
    // slots, cuts, notched contours, and bend geometry onto one persisted key.
    private static string FeatureKey(Nc1Feature feature) =>
        feature.Switch(
            hole:       static h => $"BO:hole:{h.Flange}:{Coord(h.Center)}:{h.DiameterMm:0.###}:{h.DepthMm:0.###}",
            slot:       static s => $"BO:slot:{s.Flange}:{Coord(s.Center)}:{s.DiameterMm:0.###}:{s.DepthMm:0.###}:{s.SlotLengthMm:0.###}:{s.SlotWidthMm:0.###}:{s.SlotAngleDeg:0.###}",
            cut:        static c => $"SC:{c.Flange}:{Coord(c.At)}",
            numeration: static n => $"SI:{n.Flange}:{Coord(n.At)}",
            contour:    static c => ContourKey(c.Row),
            bend:       static b => BendKey(b.Row));

    private static string ContourKey(Nc1Contour contour) =>
        $"{contour.Kind.Key}:{string.Join("|", contour.Points.Map(static p => $"{Coord(p.At)}:{(p.IsNotch ? 1 : 0)}:{p.RadiusMm:0.###}"))}";

    private static string BendKey(Nc1Bend bend) =>
        $"KA:{bend.Flange}:{bend.SourceLine}:{bend.ThicknessMm:0.###}:{Coord(bend.Line.A)}:{Coord(bend.Line.B)}";

    private static string Coord(Point3d at) => $"{at.X:0.###},{at.Y:0.###},{at.Z:0.###}";
}

// --- [OPERATIONS] --------------------------------------------------------------------
public static class Dialect {
    public static GWord Emit(PostDialect dialect, GNode node) =>
        dialect.Switch(
            state:         node,
            linuxCnc:      static (n, d) => WordAddress(d, n),
            grbl:          static (n, d) => WordAddress(d, n),
            fanuc:         static (n, d) => WordAddress(d, n),
            haas:          static (n, d) => WordAddress(d, n),
            mazak:         static (n, d) => WordAddress(d, n),
            hypertherm:    static (n, d) => WordAddress(d, n),
            siemens840D:   static (n, d) => WordAddress(d, n),
            heidenhainTnc: static (n, d) => Conversational(d, n),
            okumaOsp:      static (n, d) => Conversational(d, n),
            fagor:         static (n, d) => WordAddress(d, n),
            centroid:      static (n, d) => WordAddress(d, n),
            marlin:        static (n, d) => Additive(d, n),
            reprap:        static (n, d) => Additive(d, n));

    internal static Fin<Seq<GWord>> Seal(PostDialect dialect, Seq<GWord> words) =>
        dialect.BlockCap > 0 && words.Count > dialect.BlockCap
            ? Fin.Fail<Seq<GWord>>(new FabricationFault.BlockCapExceeded(dialect, words.Count, dialect.BlockCap).ToError())
            : Fin.Succ(words);

    private static GWord WordAddress(PostDialect dialect, GNode node) =>
        node.Switch(
            state:        dialect,
            word:         static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:  static (d, n) => Cycle(d, n),
            macro:        static (d, n) => Macro(d, n),
            subprogram:   static (d, n) => Subprogram(d, n),
            additive:     static (d, n) => Unsupported(d, n.Kind),
            nc1:          static (_, n) => Nc1(n.Part));

    private static GWord Conversational(PostDialect dialect, GNode node) =>
        node.Switch(
            state:        dialect,
            word:         static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:  static (d, n) => Cycle(d, n),
            macro:        static (d, n) => Macro(d, n),
            subprogram:   static (d, n) => Subprogram(d, n),
            additive:     static (d, n) => Unsupported(d, n.Kind),
            nc1:          static (_, n) => Nc1(n.Part));

    private static GWord Additive(PostDialect dialect, GNode node) =>
        node.Switch(
            state:        dialect,
            word:         static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:  static (d, n) => ExpandedCycle(d, n),
            macro:        static (d, n) => Unsupported(d, n.Kind),
            subprogram:   static (d, n) => Unsupported(d, n.Kind),
            additive:     static (d, n) => GWord.Additive(n.Layer, n.Extrusion, n.Temperatures, d.Decimals),
            nc1:          static (_, n) => Nc1(n.Part));

    private static GWord Address(PostDialect dialect, GCommand command, Arr<GParam> words, Option<FeedMode> mode) =>
        GWord.Address(
            dialect.CodeOverride(command.Key).IfNone(command.Code),
            words.Map(p => p.Round(dialect.Decimals)).ToArr(),
            mode,
            dialect.Modal,
            dialect.Arc);

    private static GWord Cycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        dialect.Cycles.Switch(
            state:        (dialect, cycle),
            singleBlock:  static x => Address(x.dialect, x.cycle.Command, x.cycle.SingleBlockWords, x.cycle.Mode),
            expanded:     static x => ExpandedCycle(x.dialect, x.cycle),
            dialectCycle: static x => GWord.CycleCall(x.dialect.Key, x.cycle.Kind, x.cycle.R, x.cycle.Q, x.cycle.P, x.cycle.Repeats));

    private static GWord ExpandedCycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        GWord.Expanded(cycle.ExpandedMoves.Map(move => Emit(dialect, GNode.Move(move))).ToSeq());

    private static GWord Macro(PostDialect dialect, GNode.Macro macro) =>
        dialect.Macro.Switch(
            state:    (dialect, macro),
            macroB:   static x => GWord.Macro(MacroGrammar.MacroB, x.macro.Slots.Map(MacroBSlot).ToArr(), x.macro.Body),
            rParam:   static x => GWord.Macro(MacroGrammar.RParam, x.macro.Slots.Map(RSlot).ToArr(), x.macro.Body),
            qParam:   static x => GWord.Macro(MacroGrammar.QParam, x.macro.Slots.Map(QSlot).ToArr(), x.macro.Body),
            userTask: static x => GWord.Macro(MacroGrammar.UserTask, x.macro.Slots.Map(OspSlot).ToArr(), x.macro.Body),
            none:     static x => Unsupported(x.dialect, x.macro.Kind));

    private static GWord Subprogram(PostDialect dialect, GNode.Subprogram subprogram) =>
        dialect.Subprogram.Switch(
            state: (dialect, subprogram),
            m98:   static x => GWord.Subprogram(SubprogramGrammar.M98, x.subprogram.Label, x.subprogram.Repeats, x.subprogram.Body),
            label: static x => GWord.Subprogram(SubprogramGrammar.Label, x.subprogram.Label, x.subprogram.Repeats, x.subprogram.Body),
            none:  static x => Unsupported(x.dialect, x.subprogram.Kind));

    private static GWord Nc1(SteelPart part) =>
        Nc1Word(new Nc1Program(
                Header(part.Header),
                part.Features.Map(Feature).ToArr(),
                part.Contours.Map(Contour).ToArr(),
                part.Bends.Map(Bend).ToArr()));

    private static GWord Nc1Word(Nc1Program program) =>
        GWord.Nc1(program, ContentKey.Of(EgressKind.Nc1, program.CanonicalBytes()));

    private static Nc1Header Header(SteelHeader header) =>
        new(
            header.OrderIdentification,
            header.DrawingIdentification,
            header.PhaseIdentification,
            header.PieceIdentification,
            header.QuantityOfPieces,
            header.Profile,
            header.ProfileCode,
            header.SteelQuality,
            header.Length,
            header.SawLength,
            header.ProfileHeight,
            header.FlangeWidth,
            header.FlangeThickness,
            header.WebThickness,
            header.Radius);

    private static Nc1Feature Feature(SteelFeature feature) =>
        feature.Switch(
            hole:       static h => new Nc1Feature.Hole(h.Row.Center, h.Row.Flange, h.Row.DiameterMm, h.Row.DepthMm),
            slot:       static s => new Nc1Feature.Slot(s.Row.Center, s.Row.Flange, s.Row.DiameterMm, s.Row.DepthMm, s.Row.SlotLengthMm, s.Row.SlotWidthMm, s.Row.SlotAngleDeg),
            cut:        static c => new Nc1Feature.Cut(c.Row.At, c.Row.Flange),
            numeration: static n => new Nc1Feature.Numeration(n.Row.At, n.Row.Flange),
            contour:    static c => new Nc1Feature.Contour(Contour(c.Row)),
            bend:       static b => new Nc1Feature.Bend(Bend(b.Row)));

    private static Nc1Contour Contour(SteelContour contour) =>
        new(contour.Kind, contour.Points.Map(p => new Nc1Point(p.At, p.IsNotch, p.RadiusMm)).ToArr());

    private static Nc1Bend Bend(SteelBendSeed bend) =>
        new(bend.Line, bend.Flange, bend.ThicknessMm, bend.SourceLine);

    private static MacroSlot MacroBSlot(MacroSlot slot) => slot with { Key = $"#{slot.Index}" };

    private static MacroSlot RSlot(MacroSlot slot) => slot with { Key = $"R{slot.Index}" };

    private static MacroSlot QSlot(MacroSlot slot) => slot with { Key = $"Q{slot.Index}" };

    private static MacroSlot OspSlot(MacroSlot slot) => slot with { Key = $"V{slot.Index}" };

    private static GWord Unsupported(PostDialect dialect, GNodeKind kind) =>
        GWord.Fault(new FabricationFault.DialectUnsupported(dialect, kind).ToError());
}
```
