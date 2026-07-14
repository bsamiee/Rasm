# [RASM_FABRICATION_DIALECT]

`Dialect` owns the byte-lowering half of `Run(Post)` as one generated total `Switch` over the `PostDialect` grammar table. `Process/family#PROCESS_FAMILY` owns dialect capability data; `Posting/program#CUT_PROGRAM` owns the dialect-neutral `GNode` AST; this page owns the lowering folds that expand nodes into grammar-true `GWord` output — word-address, genuinely conversational (Klartext `L`/`C` verb rows, never re-labeled G-code), additive layer-stack, canned-cycle expansion, macro and subprogram lowering with their BODIES lowered through `Emit` (a raw-`GNode` body in a `GWord` is the truncation defect), command-code override resolution, WCS roster resolution, and the NC1 DSTV record renderer over `SteelPart` DIRECTLY through one `Nc1Canonical` projection — a parallel record family mirroring the steel rows field-for-field is the deleted rename adapter. `Seal` is the typed egress gate: a `GWord.Fault` FAILS the posting rail — an error message can never render as program text.

## [01]-[INDEX]

- [01]-[DIALECT_EMIT]: owns `Dialect.Emit(PostDialect, GNode)`, the generated total dialect switch, the word-address, conversational, additive, macro, subprogram, canned-cycle, override, WCS-roster, and block-cap internal folds, the `Nc1Canonical` DSTV record renderer, plus the route for `DialectUnsupported` 2718 and `BlockCapExceeded` 2728.

## [02]-[DIALECT_EMIT]

- Owner: `Dialect` the static lowering owner; `Emit` the single public entry; `WordAddress` the RS-274 branch; `Conversational` the Klartext, Mazatrol, and OSP branch emitting `GWord.Conversational` verb rows for motion words; `Additive` the Marlin/RepRap branch; `Nc1Canonical` the DSTV ST/BO/SI/SC/AK/IK/KA/EN record renderer over `SteelPart`; `Seal` the internal fault-and-cap gate. `PostDialect` supplies capability columns, `GNode` supplies AST shape, and `Dialect` supplies lowering decisions.
- Cases: `PostDialect` composes 13 family rows; `CycleGrammar` lowers `single-block`, `expanded`, and `dialect-cycle`; `MacroGrammar` lowers Macro-B, R-param, Q-param, and user-task; `SubprogramGrammar` lowers M98 and label units; `GNodeKind` routes word, cycle, macro, subprogram, additive-layer, and NC1; `GCommand` codes resolve through `CodeOverride`; the `GCommand.Wcs` row resolves through the dialect `Wcs` roster — base ordinals render `G54`–`G59`-class codes, extended ordinals render the `G54.1 P` form.
- Entry: `public static GWord Dialect.Emit(PostDialect dialect, GNode node)` is the only public dialect fold. The generated `PostDialect.Switch` is total; a new dialect row fails until its arm lands. `Run(Post{Motion, PostDialect})` invokes this entry inside the owner case body after `Posting/program` assembles the neutral AST; `Motion` receives no second kerf pass.
- Auto: cycles consult `Cycles`; macros consult `Macro` and lower their bodies through `Emit`; subprograms consult `Subprogram` likewise; arcs consult `Arc`; precision consults `Decimals`; modal elision consults `Modal`; WCS words consult the `Wcs` roster; command words consult `CodeOverride`. Unsupported pairs lower to `GWord.Fault`, and `Seal` routes the FIRST fault as `DialectUnsupported` 2718 on the `Fin` rail before gating program size through `BlockCapExceeded` 2728 — a fault word never survives into rendered text.
- Receipt: `GWord` is the typed lowered word evidence. `GWord.Address` carries word-address rows, `GWord.Conversational` carries verb-form rows, `GWord.CycleCall` carries named-cycle payloads, `GWord.Macro`/`GWord.Subprogram` carry grammar payloads with LOWERED bodies, `GWord.Additive` carries extrusion and temperature words, `GWord.Nc1` carries the rendered DSTV records plus `ContentKey.Of(EgressKind.Nc1, recordBytes)` — the key hashes the file bytes themselves, one representation — and `GWord.Fault` carries typed rail failure evidence `Seal` consumes.
- Packages: `Process/family#PROCESS_FAMILY` (`PostDialect`, `PostFamily`, `CycleGrammar`, `MacroGrammar`, `SubprogramGrammar`, `ArcMode`, `WcsRoster`); `Posting/program#CUT_PROGRAM` (`GNode`, `GNodeKind`, `GCommand`, `GWord`, `FeedMode`, cycle, macro, subprogram, and additive node rows); `Ingress/steel#STEEL_IMPORT` (`SteelHeader`, `SteelFeature`, `SteelContour`, `SteelBendSeed`, `SteelPoint`, `SteelPart` — consumed directly, never mirrored); `Process/owner#FABRICATION_OWNER` (`EgressKind.Nc1`, `ContentKey`); `Process/faults#FAULT_BAND` (`DialectUnsupported`, `BlockCapExceeded`); LanguageExt.Core; Thinktecture.Runtime.Extensions; BCL inbox.
- Growth: conversational-dialect breadth lands as one `PostDialect` row plus one `Switch` arm reusing `Conversational`; a controller word deviation lands as one `CodeOverrides` row; a cycle convention lands as one `CycleGrammar` row and one `Cycle` arm; a macro convention lands as one `MacroGrammar` row and one `Macro` arm; a subprogram convention lands as one `SubprogramGrammar` row and one `Subprogram` arm; an NC1 record extension lands as one `SteelFeature` render arm in `Nc1Canonical`; a wider DSTV header (weight-per-meter, paint surface, free-text rows) lands when `SteelHeader` widens on `Ingress/steel`.
- Boundary: dialect data stays on `Process/family`; program AST stays on `Posting/program`; steel rows stay `Ingress/steel`'s and cross HERE only as read projections — a parallel `Nc1*` model mirroring `Steel*` field-for-field is the deleted form; egress hashing stays on `ContentKey.Of` over the rendered record bytes; this page owns lowering only. The byte-diversity kill-test is binding: one `Motion` posted to Fanuc, Haas, GRBL, Klartext, and Marlin yields five grammar-true byte-diverse files — and the Klartext file is VERB-shaped (`L X+50.0 Y+30.0 F200`), never re-labeled word-address; a `Conversational` fold whose body equals `WordAddress` is the named illusory-diversity defect.

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

    // The typed egress gate: the FIRST fault word fails the posting rail (2718 rides the word's Error), then
    // the block cap gates size (2728) — an unsupported pair can never render as successful program text.
    internal static Fin<Seq<GWord>> Seal(PostDialect dialect, Seq<GWord> words) =>
        words.Find(static w => w is GWord.Fault).Match(
            Some: fault => Fin.Fail<Seq<GWord>>(((GWord.Fault)fault).Error),
            None: () => dialect.BlockCap > 0 && words.Count > dialect.BlockCap
                ? Fin.Fail<Seq<GWord>>(new FabricationFault.BlockCapExceeded(dialect, words.Count, dialect.BlockCap).ToError())
                : Fin.Succ(words));

    private static GWord WordAddress(PostDialect dialect, GNode node) =>
        node.Switch(
            state:         dialect,
            word:          static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:   static (d, n) => Cycle(d, n),
            macro:         static (d, n) => Macro(d, n),
            subprogram:    static (d, n) => Subprogram(d, n),
            additiveLayer: static (d, n) => Unsupported(d, GNodeKind.AdditiveLayer),
            nc1:           static (_, n) => Nc1Canonical.Word(n.Part));

    // Genuinely conversational: motion words lower to Klartext-class VERB rows (G0 -> L ... FMAX, G1 -> L,
    // G2/G3 -> C with signed coordinates); non-motion machine functions keep their M/auxiliary address form.
    private static GWord Conversational(PostDialect dialect, GNode node) =>
        node.Switch(
            state:         dialect,
            word:          static (d, n) => Verb(d, n),
            cannedCycle:   static (d, n) => Cycle(d, n),
            macro:         static (d, n) => Macro(d, n),
            subprogram:    static (d, n) => Subprogram(d, n),
            additiveLayer: static (d, n) => Unsupported(d, GNodeKind.AdditiveLayer),
            nc1:           static (_, n) => Nc1Canonical.Word(n.Part));

    private static GWord Verb(PostDialect dialect, GNode.Word word) =>
        word.Command == GCommand.Rapid    ? GWord.Conversational("L", word.Words.Add(new GParam('F', double.MaxValue)), dialect.Decimals)
        : word.Command == GCommand.Feed   ? GWord.Conversational("L", word.Words, dialect.Decimals)
        : word.Command == GCommand.ArcCw  ? GWord.Conversational("C", word.Words.Add(new GParam('R', -1.0)), dialect.Decimals)
        : word.Command == GCommand.ArcCcw ? GWord.Conversational("C", word.Words.Add(new GParam('R', 1.0)), dialect.Decimals)
        : Address(dialect, word.Command, word.Words, word.Mode);

    private static GWord Additive(PostDialect dialect, GNode node) =>
        node.Switch(
            state:         dialect,
            word:          static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:   static (d, n) => ExpandedCycle(d, n),
            macro:         static (d, n) => Unsupported(d, GNodeKind.Macro),
            subprogram:    static (d, n) => Unsupported(d, GNodeKind.Subprogram),
            additiveLayer: static (d, n) => GWord.Additive(n.Layer, n.Extrusion, n.Temperatures, d.Decimals),
            nc1:           static (_, n) => Nc1Canonical.Word(n.Part));

    // The WCS row resolves through the dialect roster HERE — base ordinals render G54..G59-class codes,
    // extended ordinals the G54.1 P form; program.Prologue only ASSIGNS the slot, never the code.
    private static GWord Address(PostDialect dialect, GCommand command, Arr<GParam> words, Option<FeedMode> mode) =>
        command == GCommand.Wcs
            ? WcsWord(dialect, words)
            : GWord.Address(
                dialect.CodeOverride(command.Key).IfNone(command.Code),
                words.Map(p => p.Round(dialect.Decimals)).ToArr(),
                mode,
                dialect.Modal,
                dialect.Arc);

    private static GWord WcsWord(PostDialect dialect, Arr<GParam> words) {
        int ordinal = (int)words.Find(static p => p.Address == 'P').Map(static p => p.Value).IfNone(1.0);
        bool extended = words.Exists(static p => p.Address == 'E' && p.Value > 0.0);
        return !extended && ordinal <= dialect.Wcs.Slots
            ? GWord.Address($"G{53 + ordinal}", Arr<GParam>(), None, dialect.Modal, dialect.Arc)
            : GWord.Address("G54.1", Arr(new GParam('P', ordinal)), None, dialect.Modal, dialect.Arc);
    }

    private static GWord Cycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        dialect.Cycles.Switch(
            state:        (dialect, cycle),
            singleBlock:  static x => Address(x.dialect, x.cycle.Command, x.cycle.SingleBlockWords, x.cycle.Mode),
            expanded:     static x => ExpandedCycle(x.dialect, x.cycle),
            dialectCycle: static x => GWord.CycleCall(x.dialect.Key, x.cycle.Kind, x.cycle.R, x.cycle.Q, x.cycle.P, x.cycle.Repeats));

    private static GWord ExpandedCycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        GWord.Expanded(GNode.Moves(cycle.ExpandedMoves, Point3d.Origin).Map(move => Emit(dialect, move)));

    // Macro/subprogram BODIES lower through Emit at the fold — a GWord carrying raw GNodes starves Render
    // of its body blocks, the named truncation defect.
    private static GWord Macro(PostDialect dialect, GNode.Macro macro) =>
        dialect.Macro.Switch(
            state:    (dialect, macro),
            macroB:   static x => GWord.Macro(MacroGrammar.MacroB, x.macro.Slots.Map(MacroBSlot).ToArr(), Lower(x.dialect, x.macro.Body)),
            rParam:   static x => GWord.Macro(MacroGrammar.RParam, x.macro.Slots.Map(RSlot).ToArr(), Lower(x.dialect, x.macro.Body)),
            qParam:   static x => GWord.Macro(MacroGrammar.QParam, x.macro.Slots.Map(QSlot).ToArr(), Lower(x.dialect, x.macro.Body)),
            userTask: static x => GWord.Macro(MacroGrammar.UserTask, x.macro.Slots.Map(OspSlot).ToArr(), Lower(x.dialect, x.macro.Body)),
            none:     static x => Unsupported(x.dialect, GNodeKind.Macro));

    private static GWord Subprogram(PostDialect dialect, GNode.Subprogram subprogram) =>
        dialect.Subprogram.Switch(
            state: (dialect, subprogram),
            m98:   static x => GWord.Subprogram(SubprogramGrammar.M98, x.subprogram.Label, x.subprogram.Repeats, Lower(x.dialect, x.subprogram.Body)),
            label: static x => GWord.Subprogram(SubprogramGrammar.Label, x.subprogram.Label, x.subprogram.Repeats, Lower(x.dialect, x.subprogram.Body)),
            none:  static x => Unsupported(x.dialect, GNodeKind.Subprogram));

    private static Seq<GWord> Lower(PostDialect dialect, Arr<GNode> body) =>
        toSeq(body).Map(node => Emit(dialect, node));

    private static MacroSlot MacroBSlot(MacroSlot slot) => slot with { Key = $"#{slot.Index}" };

    private static MacroSlot RSlot(MacroSlot slot) => slot with { Key = $"R{slot.Index}" };

    private static MacroSlot QSlot(MacroSlot slot) => slot with { Key = $"Q{slot.Index}" };

    private static MacroSlot OspSlot(MacroSlot slot) => slot with { Key = $"V{slot.Index}" };

    private static GWord Unsupported(PostDialect dialect, GNodeKind kind) =>
        GWord.Fault(new FabricationFault.DialectUnsupported(dialect, kind).ToError());
}

// The DSTV NC1 renderer over SteelPart DIRECTLY: the rendered records ARE the file bytes AND the content-key
// payload — one representation, no mirror model, no second serialization.
public static class Nc1Canonical {
    public static GWord Word(SteelPart part) {
        Seq<string> records = Render(part);
        return GWord.Nc1(records, ContentKey.Of(EgressKind.Nc1, Encoding.UTF8.GetBytes(string.Join("\n", records.ToArray()))));
    }

    public static Seq<string> Render(SteelPart part) =>
        Header(part.Header)
            .Concat(part.Features.Bind(Feature))
            .Concat(part.Contours.Bind(Contour))
            .Concat(part.Bends.Bind(Bend))
            .Add("EN");

    static Seq<string> Header(SteelHeader h) =>
        Seq("ST",
            $"  {h.OrderIdentification}", $"  {h.DrawingIdentification}", $"  {h.PhaseIdentification}", $"  {h.PieceIdentification}",
            $"  {h.SteelQuality}", $"  {h.QuantityOfPieces}", $"  {h.Profile}", $"  {h.ProfileCode}",
            $"  {h.Length:0.###}", $"  {h.SawLength:0.###}",
            $"  {h.ProfileHeight:0.###}", $"  {h.FlangeWidth:0.###}", $"  {h.FlangeThickness:0.###}", $"  {h.WebThickness:0.###}", $"  {h.Radius:0.###}");

    static Seq<string> Feature(SteelFeature feature) =>
        feature.Switch(
            hole:       static f => Seq("BO", $"  {f.Row.Flange}  {Coord(f.Row.Center)}  {f.Row.DiameterMm:0.###}  {f.Row.DepthMm:0.###}"),
            slot:       static f => Seq("BO", $"  {f.Row.Flange}  {Coord(f.Row.Center)}  {f.Row.DiameterMm:0.###}  {f.Row.DepthMm:0.###}  l{f.Row.SlotLengthMm:0.###}  w{f.Row.SlotWidthMm:0.###}  a{f.Row.SlotAngleDeg:0.###}"),
            cut:        static f => Seq("SC", $"  {f.Row.Flange}  {Coord(f.Row.At)}"),
            numeration: static f => Seq("SI", $"  {f.Row.Flange}  {Coord(f.Row.At)}"),
            contour:    static f => Contour(f.Row),
            bend:       static f => Bend(f.Row));

    static Seq<string> Contour(SteelContour contour) =>
        Seq(contour.Kind.Key.ToUpperInvariant())
            .Concat(toSeq(contour.Points).Map(static p => $"  {Coord(p.At)}{(p.IsNotch ? "  n" : "")}{(p.RadiusMm > 0.0 ? $"  r{p.RadiusMm:0.###}" : "")}"));

    static Seq<string> Bend(SteelBendSeed bend) =>
        Seq("KA", $"  {bend.Flange}  {Coord(bend.Line.A)}  {Coord(bend.Line.B)}  t{bend.ThicknessMm:0.###}");

    static string Coord(Point3d at) => $"u{at.X:0.###}  v{at.Y:0.###}  w{at.Z:0.###}";
}
```
