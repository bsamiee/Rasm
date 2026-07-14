# [RASM_FABRICATION_DIALECT]

`Dialect` owns the byte-lowering half of `Run(Post)` as one generated total `Switch` over the `PostFamily` grammar column — the family column IS the grammar correspondence, so a dialect row and its emitted grammar can never disagree. `Process/family` owns dialect capability data; `Posting/program` owns the dialect-neutral `GNode` AST; this page owns the lowering folds that expand nodes into grammar-true `GWord` output — word-address, genuinely conversational (Klartext `L`/`C` verb rows, never re-labeled G-code), additive layer-stack, canned-cycle expansion, macro and subprogram lowering with their bodies lowered through `Emit`, command-code override resolution, WCS roster resolution, capability gating over the `Features` and `Compensation` rosters, and the NC1 DSTV record renderer over `SteelPart` directly through one `Nc1Canonical` projection — the NC1 node carries its complete steel receipt and the DSTV grammar is its own, so every family folds it through the one projection. `Seal` is the typed egress gate: a `GWord.Fault` fails the posting rail, so an error message can never render as program text.

## [01]-[INDEX]

- [01]-[DIALECT_EMIT]: owns `Dialect.Emit(PostDialect, GNode)`, the generated total `PostFamily` switch, the word-address, conversational, additive, forming, macro, subprogram, canned-cycle, override, WCS-roster, command-capability, and block-cap internal folds, the `Nc1Canonical` DSTV record renderer, plus the route for `DialectUnsupported` 2718 and `BlockCapExceeded` 2728.

## [02]-[DIALECT_EMIT]

- Owner: `Dialect` the static lowering owner; `Emit` the single public entry dispatching on the `PostFamily` column; `WordAddress` the RS-274/EIA branch; `Conversational` the Klartext branch emitting exact `L`, `CC`, and `C` records with `FMAX`; `Additive` the Marlin/RepRap branch; `Forming` the press-brake branch; `Nc1Canonical` the DSTV `ST`/`BO`/`SI`/`SC`/`AK`/`IK`/`EN` renderer over `SteelPart`; `Seal` the recursive fault-and-physical-record cap gate. A word-address controller wearing a conversational family label is the split-brain defect — the family column decides, and correcting a row re-routes lowering with no second declaration.
- Cases: `PostFamily` routes the word-address, conversational, additive, and forming folds; `CycleGrammar` lowers `single-block`, `expanded`, and `dialect-cycle`; `MacroGrammar` lowers Macro-B, R-param, Q-param, and user-task; `SubprogramGrammar` lowers M98 and label units; `GNodeKind` routes word, cycle, macro, subprogram, additive-layer, and NC1 — the NC1 node lowers dialect-invariant through `Nc1Canonical` in every family because it carries its complete steel receipt and the DSTV grammar is its own; `GCommand` codes resolve through `CodeOverride`; the `GCommand.Wcs` row resolves through the dialect `Wcs` roster — base ordinals render `G54`–`G59`-class codes, extended ordinals render the `G54.1 P` form.
- Entry: `public static GWord Dialect.Emit(PostDialect dialect, GNode node)` is the only public dialect fold. The generated `PostFamily.Switch` is total; a new family fails until its fold lands, and a new dialect row routes through its family column with zero new arms. `Run(Post{Motion, PostDialect})` invokes this entry inside the owner case body after `Posting/program` assembles the neutral AST; `Motion` receives no second kerf pass.
- Auto: cycles consult `Cycles`; named cycles carry the real overridden controller code, parameter words, and repeat count rather than diagnostic text. Macros and subprograms lower their bodies through `Emit`; every lowering family shares the ONE `Arc` admission — the Klartext `CC`/`C` pair renders only from an admitted center representation, never zero-filled offsets; capability-bearing commands gate against the `Features` and `Compensation` rosters and rotary `A`/`B`/`C` addresses against `Rotary`, a missing capability following the typed unsupported rail rather than silent filtering; precision consults `Decimals`; WCS words remain inside the base and extended `Wcs` capacities; command words consult `CodeOverride`; `WordRetention` drives stateful modal-code and feed-mode elision. `Seal` searches faults recursively through macro/subprogram/expanded bodies, counts their physical records, NC1 lines, and conversational record groups, and applies `BlockCapExceeded` only when `BlockCap` carries a configured limit.
- Receipt: `GWord` is the typed lowered evidence. `Address` carries its `ModalGroup`, optional `FeedMode`, `WordRetention`, and rounded parameters; `Text` carries multi-record Klartext; `CycleCall` carries executable named-cycle rows; macro/subprogram cases carry lowered bodies; and `Nc1` carries the rendered records plus `ContentKey.Of(EgressKind.Nc1, recordBytes)`. The NC1 projection consumes all 25 `SteelHeader` fields, every `SteelFeature` case including marking, point radii/notches/bevels, and each feature exactly once.
- Packages: `Process/family` (`PostDialect`, `PostFamily`, `CycleGrammar`, `MacroGrammar`, `SubprogramGrammar`, `ArcMode`, `WcsRoster`); `Posting/program` (`GNode`, `GNodeKind`, `GCommand`, `GWord`, `FeedMode`, cycle, macro, subprogram, and additive node rows); `Ingress/steel` (`SteelHeader`, `SteelFeature`, `SteelContour`, `SteelPoint`, `SteelPart` — consumed directly, never mirrored); `Process/owner` (`EgressKind.Nc1`, `ContentKey`); `Process/faults` (`DialectUnsupported`, `BlockCapExceeded`); LanguageExt.Core; Thinktecture.Runtime.Extensions; BCL inbox.
- Growth: dialect breadth lands as one `PostDialect` row — its family column routes lowering with zero new arms here; a new grammar family lands as one `PostFamily` row plus one fold arm; a capability-bearing command lands as one `CommandGates` row; a controller word deviation lands as one `CodeOverrides` row; a cycle convention lands as one `CycleGrammar` row and one `Cycle` arm; a macro convention lands as one `MacroGrammar` row and one `Macro` arm; a subprogram convention lands as one `SubprogramGrammar` row and one `Subprogram` arm; an NC1 feature or header extension lands first on `SteelPart`, then as one exhaustive render arm or field projection in `Nc1Canonical`.
- Boundary: dialect data stays on `Process/family`; program AST stays on `Posting/program`; steel rows stay `Ingress/steel`'s and cross HERE only as read projections — a parallel `Nc1*` model mirroring `Steel*` field-for-field is the deleted form; egress hashing stays on `ContentKey.Of` over the rendered record bytes; this page owns lowering only. The byte-diversity kill-test is binding: one `Motion` posted to Fanuc, Haas, GRBL, Klartext, and Marlin yields five grammar-true byte-diverse files — and the Klartext file is VERB-shaped (`L X+50.0 Y+30.0 F200`), never re-labeled word-address; a `Conversational` fold whose body equals `WordAddress` is the named illusory-diversity defect.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------
using System;
using System.Globalization;
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
    // The Family COLUMN is the one grammar correspondence: a new dialect row routes through it with zero
    // new arms here, and a family change re-routes lowering by construction — no per-dialect fan to drift.
    public static GWord Emit(PostDialect dialect, GNode node) =>
        dialect.Family.Switch(
            state:          (Dialect: dialect, Node: node),
            wordAddress:    static x => WordAddress(x.Dialect, x.Node),
            conversational: static x => Conversational(x.Dialect, x.Node),
            additiveGcode:  static x => Additive(x.Dialect, x.Node),
            forming:        static x => Forming(x.Dialect, x.Node));

    // The typed egress gate: the FIRST fault word fails the posting rail (2718 rides the word's Error), then
    // the block cap gates size (2728) — an unsupported pair can never render as successful program text.
    internal static Fin<Seq<GWord>> Seal(PostDialect dialect, Seq<GWord> words) =>
        words.Bind(Faults).HeadOrNone().Match(
            Some: error => Fin.Fail<Seq<GWord>>(error),
            None: () => dialect.BlockCap.Match(
                Some: cap => Cap(dialect, words, cap),
                None: () => Fin.Succ(words)));

    private static Fin<Seq<GWord>> Cap(PostDialect dialect, Seq<GWord> words, int cap) {
        int count = words.Sum(PhysicalCount);
        return count > cap
            ? Fin.Fail<Seq<GWord>>(new FabricationFault.BlockCapExceeded(dialect, count, cap).ToError())
            : Fin.Succ(words);
    }

    private static Seq<Error> Faults(GWord word) =>
        word.Switch(
            address: static _ => Seq<Error>(),
            conversational: static _ => Seq<Error>(),
            text: static _ => Seq<Error>(),
            cycleCall: static _ => Seq<Error>(),
            fault: static f => Seq(f.Error),
            macro: static m => m.Body.Bind(Faults),
            subprogram: static s => s.Body.Bind(Faults),
            additive: static _ => Seq<Error>(),
            nc1: static _ => Seq<Error>(),
            expanded: static e => e.Words.Bind(Faults));

    private static int PhysicalCount(GWord word) =>
        word.Switch(
            address: static _ => 1,
            conversational: static _ => 1,
            text: static t => t.Records.Count,
            cycleCall: static _ => 1,
            macro: static m => 1 + m.Body.Sum(PhysicalCount),
            subprogram: static s => 3 + s.Body.Sum(PhysicalCount),
            additive: static _ => 4,
            nc1: static n => n.Records.Count,
            fault: static _ => 0,
            expanded: static e => e.Words.Sum(PhysicalCount)
        );

    private static GWord WordAddress(PostDialect dialect, GNode node) =>
        node.Switch(
            state:         dialect,
            word:          static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:   static (d, n) => Cycle(d, n),
            macro:         static (d, n) => Macro(d, n),
            subprogram:    static (d, n) => Subprogram(d, n),
            additiveLayer: static (d, n) => Unsupported(d, GNodeKind.AdditiveLayer),
            nc1:           static (_, n) => Nc1Canonical.Word(n.Receipt.Part));

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
            nc1:           static (_, n) => Nc1Canonical.Word(n.Receipt.Part));

    // The forming family carries no executable grammar rows; the dialect-invariant NC1 projection is the
    // one lowering it shares with every other family.
    private static GWord Forming(PostDialect dialect, GNode node) =>
        node.Switch(
            state:         dialect,
            word:          static (d, _) => Unsupported(d, GNodeKind.Word),
            cannedCycle:   static (d, _) => Unsupported(d, GNodeKind.CannedCycle),
            macro:         static (d, _) => Unsupported(d, GNodeKind.Macro),
            subprogram:    static (d, _) => Unsupported(d, GNodeKind.Subprogram),
            additiveLayer: static (d, _) => Unsupported(d, GNodeKind.AdditiveLayer),
            nc1:           static (_, n) => Nc1Canonical.Word(n.Receipt.Part));

    // Linear verbs ride the typed Conversational case (Render signs coordinates, F stays unsigned, the rapid Tail carries FMAX); the CC/C
    // center-then-move pair stays a genuinely two-record Text block and renders ONLY from the shared Arc admission — a centerless or R-only arc
    // on an Ijk row fails typed, never as fabricated IX+0 IY+0 offsets; rotary words gate against the Rotary feature instead of silent filtering.
    private static GWord Verb(PostDialect dialect, GNode.Word word) =>
        word.Command == GCommand.Rapid || word.Command == GCommand.Feed
            ? RotaryAdmitted(dialect, word.Words)
                ? GWord.Conversational(
                    "L",
                    Klartext(word.Words, feed: word.Command == GCommand.Feed),
                    dialect.Decimals,
                    word.Command == GCommand.Rapid ? "FMAX" : string.Empty)
                : Unsupported(dialect, GNodeKind.Word)
        : word.Command == GCommand.ArcCw || word.Command == GCommand.ArcCcw
            ? AddressWords(dialect, word.Command, word.Words).Match(
                Some: _ => ArcRecords(word, dialect.Decimals, clockwise: word.Command == GCommand.ArcCw),
                None: () => Unsupported(dialect, GNodeKind.Word))
        : Address(dialect, word.Command, word.Words, word.Mode);

    private static Arr<GParam> Klartext(Arr<GParam> words, bool feed) =>
        words.Filter(p => p.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C' || (feed && p.Address == 'F')).ToArr();

    private static GWord ArcRecords(GNode.Word word, int decimals, bool clockwise) =>
        GWord.Text(Seq(
            $"CC IX{Signed(word.P('I').IfNone(0.0), decimals)} IY{Signed(word.P('J').IfNone(0.0), decimals)}",
            $"C {Coordinates(word.Words.Filter(static p => p.Address is 'X' or 'Y' or 'Z' or 'F').ToArr(), decimals)} DR{(clockwise ? "-" : "+")}"));

    private static string Coordinates(Arr<GParam> words, int decimals) =>
        string.Join(" ", words.Filter(static p => p.Address is 'X' or 'Y' or 'Z' or 'F')
            .Map(p => p.Address == 'F'
                ? $"F{Math.Round(p.Value, decimals).ToString(CultureInfo.InvariantCulture)}"
                : $"{p.Address}{Signed(p.Value, decimals)}")
            .ToArray());

    private static string Signed(double value, int decimals) =>
        $"{(value >= 0.0 ? "+" : string.Empty)}{Math.Round(value, decimals).ToString(CultureInfo.InvariantCulture)}";

    private static GWord Additive(PostDialect dialect, GNode node) =>
        node.Switch(
            state:         dialect,
            word:          static (d, n) => Address(d, n.Command, n.Words, n.Mode),
            cannedCycle:   static (d, n) => ExpandedCycle(d, n),
            macro:         static (d, n) => Unsupported(d, GNodeKind.Macro),
            subprogram:    static (d, n) => Unsupported(d, GNodeKind.Subprogram),
            additiveLayer: static (d, n) => GWord.Additive(n.Layer, n.Extrusion, n.Temperatures, d.Decimals),
            nc1:           static (_, n) => Nc1Canonical.Word(n.Receipt.Part));

    // The WCS row resolves through the dialect roster HERE — base ordinals render G54..G59-class codes,
    // extended ordinals the G54.1 P form; program.Prologue only ASSIGNS the slot, never the code.
    private static GWord Address(PostDialect dialect, GCommand command, Arr<GParam> words, Option<FeedMode> mode) =>
        command == GCommand.Wcs
            ? WcsWord(dialect, words)
            : AddressWords(dialect, command, words).Match(
                Some: admitted => GWord.Address(
                    dialect.CodeOverride(command.Key).IfNone(command.Code),
                    command.Group,
                    admitted.Map(p => p.Round(dialect.Decimals)).ToArr(),
                    mode,
                    dialect.Retention),
                None: () => Unsupported(dialect, GNodeKind.Word));

    // The command-capability correspondence: one gate row per capability-bearing command; a command absent
    // from the table is capability-free by construction. Compensation families read the Compensation roster,
    // feature families read Features — an empty roster can never emit the word, and rejection stays typed.
    private static readonly HashMap<GCommand, Func<PostDialect, bool>> CommandGates = HashMap<GCommand, Func<PostDialect, bool>>(
        (GCommand.CompLeft,            static d => d.Compensation.Contains(CutterCompKind.Radius)),
        (GCommand.CompRight,           static d => d.Compensation.Contains(CutterCompKind.Radius)),
        (GCommand.CompOff,             static d => d.Compensation.Contains(CutterCompKind.Radius)),
        (GCommand.LengthOffset,        static d => d.Compensation.Contains(CutterCompKind.Length)),
        (GCommand.LengthCancel,        static d => d.Compensation.Contains(CutterCompKind.Length)),
        (GCommand.Probe,               static d => d.Features.Contains(DialectFeature.Probing)),
        (GCommand.ProbeTowardStop,     static d => d.Features.Contains(DialectFeature.Probing)),
        (GCommand.ProbeTowardOptional, static d => d.Features.Contains(DialectFeature.Probing)),
        (GCommand.ProbeAwayStop,       static d => d.Features.Contains(DialectFeature.Probing)),
        (GCommand.ProbeAwayOptional,   static d => d.Features.Contains(DialectFeature.Probing)),
        (GCommand.FeedInverseTime,     static d => d.Features.Contains(DialectFeature.InverseTime)),
        (GCommand.ThreadCycle,         static d => d.Features.Contains(DialectFeature.ThreadCycle)),
        (GCommand.ToolChange,          static d => d.Features.Contains(DialectFeature.ToolChange)),
        (GCommand.Metric,              static d => d.Features.Contains(DialectFeature.Metric)),
        (GCommand.Inch,                static d => d.Features.Contains(DialectFeature.Imperial)),
        (GCommand.Absolute,            static d => d.Features.Contains(DialectFeature.Absolute)),
        (GCommand.Relative,            static d => d.Features.Contains(DialectFeature.Incremental)),
        (GCommand.PlaneXy,             static d => d.Features.Contains(DialectFeature.PlaneSelection)),
        (GCommand.PlaneZx,             static d => d.Features.Contains(DialectFeature.PlaneSelection)),
        (GCommand.PlaneYz,             static d => d.Features.Contains(DialectFeature.PlaneSelection)),
        (GCommand.Dwell,               static d => d.Features.Contains(DialectFeature.TimeDwell)),
        (GCommand.Pierce,              static d => d.Features.Contains(DialectFeature.TimeDwell)));

    // A rotary word on a non-rotary row is motion loss either way: dropping the axis mutilates the path, so
    // the whole word fails typed and the caller learns the pair is inadmissible.
    private static bool RotaryAdmitted(PostDialect dialect, Arr<GParam> words) =>
        !words.Exists(static p => p.Address is 'A' or 'B' or 'C') || dialect.Features.Contains(DialectFeature.Rotary);

    private static Option<Arr<GParam>> AddressWords(PostDialect dialect, GCommand command, Arr<GParam> words) =>
        !(CommandGates.Find(command).Map(gate => gate(dialect)).IfNone(true) && RotaryAdmitted(dialect, words))
            ? Option<Arr<GParam>>.None
            : command != GCommand.ArcCw && command != GCommand.ArcCcw
                ? Some(words)
                : dialect.Arc.Bind(mode => {
                    bool radius = words.Exists(static word => word.Address == 'R');
                    bool center = words.Exists(static word => word.Address is 'I' or 'J' or 'K');
                    return (mode == ArcMode.Both && (radius || center))
                        || (mode == ArcMode.RWord && radius)
                        || (mode == ArcMode.Ijk && center)
                            ? Some(words)
                            : Option<Arr<GParam>>.None;
                });

    private static GWord WcsWord(PostDialect dialect, Arr<GParam> words) {
        int ordinal = (int)words.Find(static p => p.Address == 'P').Map(static p => p.Value).IfNone(1.0);
        bool extended = words.Exists(static p => p.Address == 'E' && p.Value > 0.0);
        // Base codes exist only through G59: an ordinal past 6 is a roster-data error, refused typed even
        // when the family row claims more base slots — G60+ are real commands, never invented WCS codes.
        return (extended, ordinal) switch {
            (false, > 0 and <= 6) when ordinal <= dialect.Wcs.Slots =>
                GWord.Address($"G{53 + ordinal}", ModalGroup.Wcs, Arr<GParam>(), None, dialect.Retention),
            (true, > 0) when ordinal <= dialect.Wcs.Extended =>
                GWord.Address("G54.1", ModalGroup.Wcs, Arr(new GParam('P', ordinal)), None, dialect.Retention),
            _ => Unsupported(dialect, GNodeKind.Word),
        };
    }

    private static GWord Cycle(PostDialect dialect, GNode.CannedCycle cycle) =>
        dialect.Cycles.Switch(
            state:        (dialect, cycle),
            singleBlock:  static x => Address(x.dialect, x.cycle.Command, x.cycle.SingleBlockWords, x.cycle.Mode),
            expanded:     static x => ExpandedCycle(x.dialect, x.cycle),
            dialectCycle: static x => GWord.CycleCall(
                x.dialect.Key,
                x.dialect.CodeOverride(x.cycle.Command.Key).IfNone(x.cycle.Command.Code),
                x.cycle.SingleBlockWords,
                x.cycle.Repeats));

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
            .Add("EN");

    private static Seq<string> Header(SteelHeader h) =>
        Seq("ST",
            $"  {h.OrderIdentification}", $"  {h.DrawingIdentification}", $"  {h.PhaseIdentification}", $"  {h.PieceIdentification}",
            $"  {h.SteelQuality}", $"  {h.QuantityOfPieces}", $"  {h.Profile}", $"  {h.ProfileCode}",
            $"  {Number(h.Length)}", $"  {Number(h.SawLength)}",
            $"  {Number(h.ProfileHeight)}", $"  {Number(h.FlangeWidth)}", $"  {Number(h.FlangeThickness)}", $"  {Number(h.WebThickness)}", $"  {Number(h.Radius)}",
            $"  {Number(h.WebStartCut)}", $"  {Number(h.WebEndCut)}", $"  {Number(h.FlangeStartCut)}", $"  {Number(h.FlangeEndCut)}",
            $"  {Number(h.WeightByMeter)}", $"  {Number(h.PaintingSurfaceByMeter)}",
            $"  {h.Text1InfoOnPiece}", $"  {h.Text2InfoOnPiece}", $"  {h.Text3InfoOnPiece}", $"  {h.Text4InfoOnPiece}");

    private static Seq<string> Feature(SteelFeature feature) =>
        feature.Switch(
            hole:       static f => Seq("BO", $"  {f.Row.Flange}  {Coord(f.Row.Center)}  {Number(f.Row.DiameterMm)}  {Number(f.Row.DepthMm)}"),
            slot:       static f => Seq("BO", $"  {f.Row.Flange}  {Coord(f.Row.Center)}  {Number(f.Row.DiameterMm)}  {Number(f.Row.DepthMm)}  l{Number(f.Row.SlotLengthMm)}  w{Number(f.Row.SlotWidthMm)}  a{Number(f.Row.SlotAngleDeg)}"),
            cut:        static f => Seq("SC", $"  {f.Row.Flange}  {Coord(f.Row.At)}"),
            numeration: static f => Seq("SI", $"  {f.Row.Flange}  {Coord(f.Row.At)}"),
            contour:    static f => Contour(f.Row),
            marking:    static f => Contour(f.Row));

    private static Seq<string> Contour(SteelContour contour) =>
        Seq(contour.Kind.Key.ToUpperInvariant())
            .Concat(toSeq(contour.Points).Map(Point));

    private static string Point(SteelPoint point) =>
        $"  {Coord(point.At)}{(point.IsNotch ? "  n" : string.Empty)}{(point.RadiusMm > 0.0 ? $"  r{Number(point.RadiusMm)}" : string.Empty)}" +
        point.Bevel.Map(static bevel => $"  v{Number(bevel.FirstAngleDeg)},{Number(bevel.FirstBlunting)},{Number(bevel.SecondAngleDeg)},{Number(bevel.SecondBlunting)}").IfNone(string.Empty);

    private static string Coord(Point3d at) => $"u{Number(at.X)}  v{Number(at.Y)}  w{Number(at.Z)}";

    private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
```
