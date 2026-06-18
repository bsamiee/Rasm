# [RASM_FABRICATION_PROGRAM]

The host-neutral portable cut-program emitter: `CutProgram` the typed RS-274/ISO-6983 G-code model the `Motion` and `Placement` streams dead-end into, plus the cut-geometry conditioning — kerf compensation, lead-in/out, micro-tab/bridge retention, and cut-sequencing — over the `geometry2d/clipper#POLYGON_ALGEBRA` Clipper2 offset. This is the missing downstream owner: the `toolpath/motion` and `nesting/nfp` streams produced moves and placements with no portable emitter, and a portable cut-program is a real fabrication concern (a data contract) distinct from Rhino native file I/O. The emitter is a typed G-code AST folded to text — `GWord` blocks discriminated by a closed `GCommand` axis — never a string-concatenation builder. Kerf compensation and lead geometry route the one Geometry2D offset owner (kerf is a closed-contour offset by half the cut width, a lead is an open-path offset arc); micro-tabs are gap insertions on the offset contour; cut-sequencing orders the cuts so an inner contour cuts before its enclosing outline (crash-safe ordering). The kernel composes the `frontier/owner#FABRICATION_OWNER` `Move`/`Loop`/`PartTransform`/`FrontierResult` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `CutProgram` text and AST are an in-process portable contract a downstream consumer writes to a file or streams to a controller — never a browser or peer wire. The portable cut-program is distinct from and coexists with the Rhino-native file I/O; neither is thinned to feed the other.

## [1]-[INDEX]

One cluster: `[2]-[CUT_PROGRAM]` owns the `GCommand`/`LeadStyle` axes, the `GWord`/`CutProgram` RS-274/ISO-6983 G-code AST, and the `Posting` fold — kerf-comp, lead-in/out, micro-tab/bridge, and cut-sequencing over the Geometry2D offset.

## [2]-[CUT_PROGRAM]

- Owner: `GCommand` `[SmartEnum<string>]` the closed RS-274 command axis (`rapid`/`feed`/`arc-cw`/`arc-ccw`/`dwell`/`spindle`/`coolant`/`program-end`) carrying its G/M code; `GWord` the one program block (the command plus its word-addressed parameters X/Y/Z/I/J/F); `LeadStyle` `[SmartEnum<string>]` the lead-in/out geometry axis (`none`/`line`/`arc`); `CutProgram` the typed G-code AST plus the `Emit` fold to RS-274 text; `Posting` the static surface owning `Post` (the `Motion`/`Placement`-to-`CutProgram` fold), `Kerf` (the half-width offset), `Lead` (the lead-in/out arc), `Tabs` (the micro-bridge gaps), and `Sequence` (the crash-safe cut order).
- Cases: `GCommand` rows `rapid` (G0) · `feed` (G1) · `arc-cw` (G2) · `arc-ccw` (G3) · `dwell` (G4) · `spindle` (M3/M5) · `coolant` (M8/M9) · `program-end` (M2/M30) (8); `LeadStyle` rows `none` · `line` · `arc` (3).
- Entry: `public static Fin<CutProgram> Posting.Post(FrontierResult result, PostPolicy policy)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed cut contour and `FabricationFault.KerfCollision` when a kerf offset self-intersects (a feature narrower than the kerf), each lowered with `.ToError()`; the body conditions the cut geometry (kerf, lead, tabs, sequence) and folds the conditioned moves into the typed `CutProgram` AST.
- Auto: `Posting.Post` reads the `FrontierResult` — a `Motion` lowers its `Move` stream directly, a `Placement` lowers each placed part's contour at its `PartTransform`; `Kerf` offsets each closed cut contour inward or outward by half the cut width through the `geometry2d/clipper#POLYGON_ALGEBRA` `Offset` (inside for a hole, outside for an outline), routing `FabricationFault.KerfCollision` when the offset collapses; `Lead` prepends a lead-in and appends a lead-out arc per `LeadStyle` so the pierce point sits off the finished edge; `Tabs` inserts micro-bridge gaps at regular arc-length intervals on the contour so the cut part stays retained in the stock until removal; `Sequence` orders the cuts inner-contour-before-outline (the crash-safe ordering a flat move list lacks); the conditioned moves fold into `GWord` blocks under the `GCommand` axis, and `CutProgram.Emit` renders the word-addressed RS-274 text.
- Receipt: the `CutProgram` carries the typed `GWord` block list, the per-block command and parameters, and the rendered RS-274 text — the typed portable cut-program evidence a downstream consumer writes; no generic post-processor ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, the cut-sequence containment verdict), Clipper2 (via `geometry2d/clipper#POLYGON_ALGEBRA` — kerf and lead offset), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new controller dialect is one `Emit` arm over the same `CutProgram` AST (the AST is dialect-neutral, the text fold is dialect-specific); a 5-axis word set is one parameter column on `GWord`; a torch-height-control word is one `GCommand` row; zero new surface.
- Boundary: `Posting` is the ONE portable cut-program owner and a per-controller string-builder family is the deleted form — the `CutProgram` AST is dialect-neutral and a new controller dialect lands as one `Emit` arm over the same AST, never a parallel emitter class; the `Emit` `StringBuilder` fold is the named text-rendering boundary — the BCL text-accumulation owner the dialect-neutral word-address render writes through, never a domain mutation; `Post` reads the `FrontierResult` through the generated total `Switch` and a `result switch` pattern cascade with a `_` catch-all is the deleted form — the `HiddenLineResult` arm is a named total case routing `DegenerateInput`, never a default that swallows a new union case; kerf and lead geometry route the one `geometry2d/clipper#POLYGON_ALGEBRA` offset owner and a hand-rolled offset is the deleted form; the portable cut-program is distinct from the Rhino-native file I/O and coexists with it at the data contract, never thinned to feed it; the G-code is a typed `GWord` AST folded to text and a raw string concatenation is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Frontier;
using Rasm.Fabrication.Geometry2D;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GCommand {
    public static readonly GCommand Rapid = new("rapid", "G0");
    public static readonly GCommand Feed = new("feed", "G1");
    public static readonly GCommand ArcCw = new("arc-cw", "G2");
    public static readonly GCommand ArcCcw = new("arc-ccw", "G3");
    public static readonly GCommand Dwell = new("dwell", "G4");
    public static readonly GCommand Spindle = new("spindle", "M3");
    public static readonly GCommand Coolant = new("coolant", "M8");
    public static readonly GCommand ProgramEnd = new("program-end", "M30");

    public string Code { get; }
}

[SmartEnum<string>]
public sealed partial class LeadStyle {
    public static readonly LeadStyle None = new("none");
    public static readonly LeadStyle Line = new("line");
    public static readonly LeadStyle Arc = new("arc");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PostPolicy(double KerfWidth, LeadStyle Lead, double LeadRadius, double TabWidth, double TabSpacing) {
    public static readonly PostPolicy Canonical = new(KerfWidth: 0.2, Lead: LeadStyle.Arc, LeadRadius: 3.0, TabWidth: 0.5, TabSpacing: 50.0);
}

public readonly record struct GWord(GCommand Command, Option<double> X, Option<double> Y, Option<double> Z, Option<double> I, Option<double> J, Option<double> F);

public sealed record CutProgram(Seq<GWord> Blocks) {
    public string Emit() =>
        Blocks.Fold(new StringBuilder(), (sb, w) => {
            sb.Append(w.Command.Code);
            w.X.IfSome(v => sb.Append($" X{v:0.###}")); w.Y.IfSome(v => sb.Append($" Y{v:0.###}")); w.Z.IfSome(v => sb.Append($" Z{v:0.###}"));
            w.I.IfSome(v => sb.Append($" I{v:0.###}")); w.J.IfSome(v => sb.Append($" J{v:0.###}")); w.F.IfSome(v => sb.Append($" F{v:0.###}"));
            return sb.AppendLine();
        }).ToString();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Posting {
    public static Fin<CutProgram> Post(FrontierResult result, PostPolicy policy) =>
        result.Switch(
            state:            policy,
            hiddenLineResult: static (_, _) => Fin.Fail<CutProgram>(GeometryFault.DegenerateInput("post:hidden-line-result").ToError()),
            motion:           static (_, m) => Fin.Succ(new CutProgram(m.Moves.Map(ToBlock))),
            placement:        static (pol, p) => Sequence(p.Parts).Bind(ordered => Condition(ordered, pol)));

    static GWord ToBlock(Move m) =>
        new(m.Rapid ? GCommand.Rapid : GCommand.Feed, Some(m.To.X), Some(m.To.Y), Some(m.To.Z), None, None, m.Rapid ? None : Some(m.Feed));

    static Fin<CutProgram> Condition(Seq<PartTransform> ordered, PostPolicy policy) => throw new NotImplementedException();
    static Fin<Seq<PartTransform>> Sequence(Seq<PartTransform> parts) => throw new NotImplementedException();
}
```

## [3]-[RESEARCH]

- [CUT_CONDITIONING] The `Condition`/`Sequence` cut-geometry fold is the author depth the portable emitter requires: kerf compensation routes the `geometry2d/clipper#POLYGON_ALGEBRA` `Offset` (half-width inward for a hole, outward for an outline, `FabricationFault.KerfCollision` on a collapsed offset), the lead-in/out arc grounds against the `LeadStyle` geometry over the same Geometry2D open-path offset, the micro-tab gaps insert at `TabSpacing` arc-length intervals, and the crash-safe sequence orders inner contours before their enclosing outline through the exact `Predicate.Orient2D` point-in-polygon containment verdict — the kerf and lead offsets over the settled Geometry2D substrate, the containment over the settled kernel predicate, no second offset owner and no Clipper2 containment re-mint.
