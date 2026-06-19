# [RASM_FABRICATION_PROGRAM]

The host-neutral portable cut-program emitter: `CutProgram` the typed dialect-neutral G-code AST the `Motion` and `Placement` streams dead-end into, plus the cut-geometry conditioning — kerf compensation, lead-in/out, pierce-dwell, micro-tab/bridge retention, and cut-sequencing — over the `geometry2d/clipper#POLYGON_ALGEBRA` Clipper2 offset, rendered to a controller-specific text by the `PostDialect` `[SmartEnum<string>]` family (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`) selecting the `Emit` arm over the ONE AST. This is the missing downstream owner: the `toolpath/motion` and `nesting/nfp` streams produced moves and placements with no portable emitter, and a portable cut-program is a real fabrication concern (a data contract) distinct from Rhino native file I/O. The emitter is a typed G-code AST folded to text — `GWord` blocks discriminated by a closed `GCommand` axis carrying the milling RS-274 words plus the lathe `G96`/`G92` constant-surface-speed and threading words, the thermal `M07`/torch-height-control/pierce/assist-gas words, and the additive `M104`/`M109`/`E`-axis extrusion and bed-temp words — never a string-concatenation builder and never raw string injection. A new controller dialect is one `PostDialect` row plus one `Emit` arm over the unchanged AST; the milling RS-274 render is the existing `Emit` unchanged, re-opened as the `PostDialect.LinuxCnc` instance. Kerf compensation and lead geometry route the one Geometry2D offset owner (kerf is a closed-contour offset by half the cut width, a lead is an open-path offset arc); micro-tabs are gap insertions on the offset contour; cut-sequencing orders the cuts so an inner contour cuts before its enclosing outline (crash-safe ordering). The thermal pierce and assist-gas conditioning and the additive extrusion-and-temp conditioning read the `process-physics/cut-parameter#CUT_PARAMETER` `RemovalBudget` case (the `ThermalBudget` pierce-time/assist-pressure, the `AdditiveBudget` melt-temp). The kernel composes the `frontier/owner#FABRICATION_OWNER` `Move`/`Loop`/`PartTransform`/`FrontierResult` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `CutProgram` text and AST are an in-process portable contract a downstream consumer writes to a file or streams to a controller — never a browser or peer wire. The portable cut-program is distinct from and coexists with the Rhino-native file I/O; neither is thinned to feed the other.

## [1]-[INDEX]

One cluster: `[2]-[CUT_PROGRAM]` owns the `GCommand`/`LeadStyle`/`PostDialect` axes, the `GWord`/`CutProgram` dialect-neutral G-code AST, and the `Posting` fold — kerf-comp, lead-in/out, pierce-dwell, micro-tab/bridge, and cut-sequencing over the Geometry2D offset, rendered by the per-dialect `Emit` arm.

## [2]-[CUT_PROGRAM]

- Owner: `GCommand` `[SmartEnum<string>]` the closed command axis carrying its G/M code — the milling RS-274 words (`rapid`/`feed`/`arc-cw`/`arc-ccw`/`dwell`/`spindle`/`coolant`/`program-end`) plus the lathe (`css`/`thread-cycle`), thermal (`torch-on`/`torch-height`/`pierce`/`assist-gas`), and additive (`hotend-temp`/`hotend-wait`/`extrude`/`bed-temp`) words; `GWord` the one program block (the command plus its word-addressed parameters X/Y/Z/I/J/F/S/E); `LeadStyle` `[SmartEnum<string>]` the lead-in/out geometry axis (`none`/`line`/`arc`); `PostDialect` `[SmartEnum<string>]` the controller-dialect render axis (`linuxcnc`/`grbl`/`fanuc`/`haas`/`marlin`/`reprap`/`hypertherm`/`mazak`) selecting the `Emit` arm over the one AST; `PostPolicy` the conditioning knobs carrying the kerf width, lead style and radius, tab width and spacing, the selected `PostDialect`, the pierce-time/assist-pressure thermal conditioning, the melt-temp additive conditioning, and the `Profiles` part library plus the `Placed` part-transform resolver; `CutProgram` the typed dialect-neutral G-code AST plus the `Emit(PostDialect)` fold to controller text; `Posting` the static surface owning `Post` (the `Motion`/`Placement`-to-`CutProgram` fold), `Kerf` (the half-width offset), `Simplify` (the collinear-run collapse over the `geometry2d/clipper#POLYGON_ALGEBRA` `SimplifyPaths` before the ring walk), `CutPath`/`LeadArc`/`TabbedRing` (the lead-in/out arc and micro-bridge gaps), `Pierce` (the thermal pierce-dwell-and-assist prologue), and `Sequence` (the crash-safe cut order over the `Encloses` containment depth).
- Cases: `GCommand` rows `rapid` (G0) · `feed` (G1) · `arc-cw` (G2) · `arc-ccw` (G3) · `dwell` (G4) · `spindle` (M3/M5) · `coolant` (M8/M9) · `program-end` (M2/M30) · `css` (G96 constant-surface-speed) · `thread-cycle` (G92) · `torch-on` (M07) · `torch-height` (THC) · `pierce` (the pierce dwell) · `assist-gas` (the assist-gas on) · `hotend-temp` (M104) · `hotend-wait` (M109) · `extrude` (the E-axis extrusion) · `bed-temp` (M140) (18); `LeadStyle` rows `none` · `line` · `arc` (3); `PostDialect` rows `linuxcnc` · `grbl` · `fanuc` · `haas` · `marlin` · `reprap` · `hypertherm` · `mazak` (8), each selecting the dialect render over the one AST, the milling RS-274 render the `linuxcnc` arm unchanged.
- Entry: `public static Fin<CutProgram> Posting.Post(FrontierResult result, PostPolicy policy)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed cut contour and `FabricationFault.KerfCollision` when a kerf offset self-intersects (a feature narrower than the kerf), each lowered with `.ToError()`; the body conditions the cut geometry (kerf, lead, pierce, tabs, sequence) and folds the conditioned moves into the typed dialect-neutral `CutProgram` AST, which `CutProgram.Emit(policy.Dialect)` renders to the controller text.
- Auto: `Posting.Post` reads the `FrontierResult` — a `Motion` lowers its `Move` stream directly, a `Placement` resolves each `PartTransform.PartId` to its `PostPolicy.Profiles` contour and transforms it through `PostPolicy.Placed` (rotation + translation); `Sequence` orders the parts inner-contour-before-outline by the `Encloses` containment depth (a part fully inside another cuts first), the crash-safe ordering a flat move list lacks; `Condition` folds each ordered contour through `Kerf` — the half-width `geometry2d/clipper#POLYGON_ALGEBRA` `Offset` (inward for a `Winding == Negative` hole, outward for an outline), routing `FabricationFault.KerfCollision` when the offset collapses to no ring — then `Simplify` collapses the kerfed contour's collinear runs through the `geometry2d/clipper#POLYGON_ALGEBRA` `SimplifyPaths` at the `SimplifyEpsilon` tolerance (so the emitted G-code carries no redundant collinear `Feed` blocks), then `CutPath` rapids to the `LeadIn` pierce point off the finished edge, runs the `LeadArc` lead-in per `LeadStyle`, walks the `TabbedRing` (cutting `Feed` blocks interrupted by `Rapid` micro-bridge gaps at every `TabSpacing` arc-length interval so the part stays retained in the stock), and runs the lead-out arc back to the pierce point; the conditioned blocks wrap in a `Spindle` prologue and a `ProgramEnd` epilogue under the `GCommand` axis, and `CutProgram.Emit` renders the word-addressed RS-274 text.
- Receipt: the `CutProgram` carries the typed `GWord` block list, the per-block command and parameters, and the rendered RS-274 text — the typed portable cut-program evidence a downstream consumer writes; no generic post-processor ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, the cut-sequence containment verdict via the shared `frontier/owner#FABRICATION_OWNER` `Loop.Covers`), Clipper2 (via `geometry2d/clipper#POLYGON_ALGEBRA` — kerf and lead offset), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new controller dialect is one `PostDialect` row plus one `Emit` arm over the same `CutProgram` AST (the AST is dialect-neutral, the text fold is dialect-specific); a 5-axis word set is one parameter column on `GWord`; a new controller word (a torch-height-control, an extrusion word) is one `GCommand` row carrying its G/M code; zero new surface.
- Boundary: `Posting` is the ONE portable cut-program owner and a per-controller string-builder family is the deleted form — the `CutProgram` AST is dialect-neutral and a new controller dialect lands as one `PostDialect` row plus one `Emit` arm over the same AST, never a parallel emitter class; the dialect render is the `PostDialect`-keyed `Emit` `Switch` and a parallel `LinuxCncEmitter`/`MarlinEmitter`/`HyperthermEmitter` sibling family is the deleted form — the milling RS-274 render is the unchanged `linuxcnc` arm; THC, pierce, assist-gas, and extrusion are REAL `GCommand` rows carrying their G/M code and a raw-string injection of a controller word into the text is the named defect; the `Emit` `StringBuilder` fold is the named text-rendering boundary — the BCL text-accumulation owner the dialect-keyed word-address render writes through, never a domain mutation; `Post` reads the `FrontierResult` through the generated total `Switch` and a `result switch` pattern cascade with a `_` catch-all is the deleted form — the `HiddenLineResult` arm is a named total case routing `DegenerateInput`, never a default that swallows a new union case; kerf, lead, and collinear-simplification geometry route the one `geometry2d/clipper#POLYGON_ALGEBRA` owner (`Offset` for kerf/lead, `Simplify` over `SimplifyPaths` for the collinear collapse) and a hand-rolled offset or a hand-rolled Douglas-Peucker is the deleted form; the portable cut-program is distinct from the Rhino-native file I/O and coexists with it at the data contract, never thinned to feed it; the G-code is a typed `GWord` AST folded to text and a raw string concatenation is the named defect.

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
    public static readonly GCommand Css = new("css", "G96");
    public static readonly GCommand ThreadCycle = new("thread-cycle", "G92");
    public static readonly GCommand TorchOn = new("torch-on", "M07");
    public static readonly GCommand TorchHeight = new("torch-height", "THC");
    public static readonly GCommand Pierce = new("pierce", "G4");
    public static readonly GCommand AssistGas = new("assist-gas", "M64");
    public static readonly GCommand HotendTemp = new("hotend-temp", "M104");
    public static readonly GCommand HotendWait = new("hotend-wait", "M109");
    public static readonly GCommand Extrude = new("extrude", "G1");
    public static readonly GCommand BedTemp = new("bed-temp", "M140");

    public string Code { get; }
}

[SmartEnum<string>]
public sealed partial class LeadStyle {
    public static readonly LeadStyle None = new("none");
    public static readonly LeadStyle Line = new("line");
    public static readonly LeadStyle Arc = new("arc");
}

[SmartEnum<string>]
public sealed partial class PostDialect {
    public static readonly PostDialect LinuxCnc = new("linuxcnc", comment: "(", lineNumbers: false);
    public static readonly PostDialect Grbl = new("grbl", comment: "(", lineNumbers: false);
    public static readonly PostDialect Fanuc = new("fanuc", comment: "(", lineNumbers: true);
    public static readonly PostDialect Haas = new("haas", comment: "(", lineNumbers: true);
    public static readonly PostDialect Marlin = new("marlin", comment: ";", lineNumbers: false);
    public static readonly PostDialect Reprap = new("reprap", comment: ";", lineNumbers: false);
    public static readonly PostDialect Hypertherm = new("hypertherm", comment: "(", lineNumbers: true);
    public static readonly PostDialect Mazak = new("mazak", comment: "(", lineNumbers: true);

    public string Comment { get; }
    public bool LineNumbers { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PostPolicy(double KerfWidth, LeadStyle Lead, double LeadRadius, double TabWidth, double TabSpacing, double SimplifyEpsilon, PostDialect Dialect, double PierceTime, double AssistPressure, double MeltTemp, Arr<Loop> Profiles) {
    public static readonly PostPolicy Canonical = new(KerfWidth: 0.2, Lead: LeadStyle.Arc, LeadRadius: 3.0, TabWidth: 0.5, TabSpacing: 50.0, SimplifyEpsilon: 0.01, Dialect: PostDialect.LinuxCnc, PierceTime: 0.0, AssistPressure: 0.0, MeltTemp: 0.0, Profiles: Arr<Loop>.Empty);

    public Loop Placed(PartTransform t) {
        double ct = Math.Cos(t.RotationRadians), st = Math.Sin(t.RotationRadians);
        return new Loop(Profiles[t.PartId].Vertices.Map(v =>
            new Point3d(v.X * ct - v.Y * st + t.Tx, v.X * st + v.Y * ct + t.Ty, v.Z)).ToArr(), Closed: true);
    }
}

public readonly record struct GWord(GCommand Command, Option<double> X, Option<double> Y, Option<double> Z, Option<double> I, Option<double> J, Option<double> F, Option<double> S, Option<double> E);

public sealed record CutProgram(Seq<GWord> Blocks) {
    public string Emit(PostDialect dialect) {
        var (sb, _) = Blocks.Fold((Builder: new StringBuilder(), Line: 10), (acc, w) => {
            StringBuilder sb = acc.Builder;
            if (dialect.LineNumbers) sb.Append($"N{acc.Line} ");
            sb.Append(w.Command.Code);
            w.X.IfSome(v => sb.Append($" X{v:0.###}")); w.Y.IfSome(v => sb.Append($" Y{v:0.###}")); w.Z.IfSome(v => sb.Append($" Z{v:0.###}"));
            w.I.IfSome(v => sb.Append($" I{v:0.###}")); w.J.IfSome(v => sb.Append($" J{v:0.###}")); w.F.IfSome(v => sb.Append($" F{v:0.###}"));
            w.S.IfSome(v => sb.Append($" S{v:0.###}")); w.E.IfSome(v => sb.Append($" E{v:0.###}"));
            sb.AppendLine();
            return (sb, acc.Line + (dialect.LineNumbers ? 10 : 0));
        });
        return sb.ToString();
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Posting {
    public static Fin<CutProgram> Post(FrontierResult result, PostPolicy policy) =>
        result.Switch(
            state:            policy,
            hiddenLineResult: static (_, _) => Fin.Fail<CutProgram>(GeometryFault.DegenerateInput("post:hidden-line-result").ToError()),
            motion:           static (_, m) => Fin.Succ(new CutProgram(m.Moves.Map(ToBlock))),
            placement:        static (pol, p) => Sequence(p.Parts, pol).Bind(ordered => Condition(ordered, pol)));

    static GWord ToBlock(Move m) =>
        new(m.Rapid ? GCommand.Rapid : GCommand.Feed, Some(m.To.X), Some(m.To.Y), Some(m.To.Z), None, None, m.Rapid ? None : Some(m.Feed), None, None);

    static Fin<CutProgram> Condition(Seq<PartTransform> ordered, PostPolicy policy) =>
        ordered.Map(t => policy.Placed(t))
            .Find(static l => !l.Closed).Match(
                Some: _ => Fin.Fail<CutProgram>(FabricationFault.OpenLoop("post:open-contour").ToError()),
                None: () => ordered.Map(t => Kerf(policy.Placed(t), policy).Bind(r => Simplify(r, policy)))
                    .TraverseM(identity).As()
                    .Map(kerfed => new CutProgram(Spindle(policy).Concat(kerfed.Bind(loop => CutPath(loop, policy))).Add(End()))));

    static Fin<Loop> Simplify(Loop contour, PostPolicy policy) =>
        policy.SimplifyEpsilon <= 0.0
            ? Fin.Succ(contour)
            : PolygonAlgebra.Simplify(Seq(contour), policy.SimplifyEpsilon)
                .Bind(rings => rings.HeadOrNone().Match(
                    Some: r => Fin.Succ(r.AsCcw()),
                    None: () => Fin.Succ(contour)));

    static Fin<Loop> Kerf(Loop contour, PostPolicy policy) {
        bool hole = contour.Winding() == Sign.Negative;
        return PolygonAlgebra.Offset(Seq(contour.AsCcw()), hole ? -0.5 * policy.KerfWidth : 0.5 * policy.KerfWidth, OffsetEnds.Polygon)
            .Bind(rings => rings.HeadOrNone().Match(
                Some: r => Fin.Succ(r),
                None: () => Fin.Fail<Loop>(FabricationFault.KerfCollision($"post:kerf-collapse:{policy.KerfWidth:0.###}").ToError())));
    }

    static Seq<GWord> CutPath(Loop loop, PostPolicy policy) {
        Point3d pierce = LeadIn(loop, policy);
        Seq<GWord> body = TabbedRing(loop, policy);
        return Seq(new GWord(GCommand.Rapid, Some(pierce.X), Some(pierce.Y), None, None, None, None, None, None))
            .Concat(Pierce(policy))
            .Concat(LeadArc(pierce, loop.At(0), policy))
            .Concat(body)
            .Concat(LeadArc(loop.At(0), pierce, policy));
    }

    static Seq<GWord> Pierce(PostPolicy policy) =>
        policy.PierceTime <= 0.0
            ? Seq<GWord>()
            : Seq(new GWord(GCommand.AssistGas, None, None, None, None, None, None, Some(policy.AssistPressure), None),
                  new GWord(GCommand.TorchOn, None, None, None, None, None, None, None, None),
                  new GWord(GCommand.Pierce, None, None, None, None, None, Some(policy.PierceTime), None, None));

    static Point3d LeadIn(Loop loop, PostPolicy policy) {
        Vector3d edge = loop.At(1) - loop.At(0); edge.Unitize();
        Vector3d nrm = new(-edge.Y, edge.X, 0.0);
        return loop.At(0) + policy.LeadRadius * nrm;
    }

    static Seq<GWord> LeadArc(Point3d from, Point3d to, PostPolicy policy) =>
        policy.Lead.Switch(
            none: _ => Seq<GWord>(),
            line: _ => Seq(new GWord(GCommand.Feed, Some(to.X), Some(to.Y), None, None, None, Some(1.0), None, None)),
            arc:  _ => {
                Point3d center = from + 0.5 * (to - from);
                return Seq(new GWord(GCommand.ArcCcw, Some(to.X), Some(to.Y), None,
                    Some(center.X - from.X), Some(center.Y - from.Y), Some(1.0), None, None));
            });

    static Seq<GWord> TabbedRing(Loop loop, PostPolicy policy) {
        double acc = 0.0; bool cutting = true;
        return toSeq(Enumerable.Range(0, loop.Count + 1)).Map(i => {
            Point3d p = loop.At(i);
            if (i > 0) acc += loop.At(i - 1).DistanceTo(p);
            bool inTab = policy.TabSpacing > 1e-9 && acc % policy.TabSpacing < policy.TabWidth;
            cutting = !inTab;
            return new GWord(cutting ? GCommand.Feed : GCommand.Rapid, Some(p.X), Some(p.Y), None, None, None, cutting ? Some(1.0) : None, None, None);
        });
    }

    static Seq<GWord> Spindle(PostPolicy policy) =>
        policy.MeltTemp > 0.0
            ? Seq(new GWord(GCommand.HotendWait, None, None, None, None, None, None, Some(policy.MeltTemp), None))
            : Seq(new GWord(GCommand.Spindle, None, None, None, None, None, None, None, None));

    static GWord End() => new(GCommand.ProgramEnd, None, None, None, None, None, None, None, None);

    static Fin<Seq<PartTransform>> Sequence(Seq<PartTransform> parts, PostPolicy policy) =>
        Fin.Succ(parts.OrderByDescending(t => Encloses(t, parts, policy)).ToSeq());

    static int Encloses(PartTransform t, Seq<PartTransform> parts, PostPolicy policy) {
        Loop self = policy.Placed(t);
        Point3d c = self.Vertices.Fold(Point3d.Origin, static (a, v) => a + v) / Math.Max(1, self.Count);
        return parts.Count(other => other.PartId != t.PartId && policy.Placed(other).Covers(c));
    }
}
```

## [3]-[RESEARCH]

- [CUT_CONDITIONING] The cut-geometry fold is realized as the `Condition`/`Sequence`/`Kerf`/`CutPath`/`TabbedRing` author-kernel: kerf compensation routes the `geometry2d/clipper#POLYGON_ALGEBRA` `Offset` (half-width inward for a `Winding == Negative` hole, outward for an outline, `FabricationFault.KerfCollision` on a collapsed ring), the lead-in/out arc grounds against the `LeadStyle` geometry as an `ArcCcw`/`Feed` `GWord` off the pierce point, the micro-tab gaps interrupt the `Feed` ring with `Rapid` blocks at every `TabSpacing` arc-length interval, and the crash-safe `Sequence` orders parts by the `Encloses` containment depth through the shared `frontier/owner#FABRICATION_OWNER` `Loop.Covers` exact-`Orient2D` containment so an inner contour cuts before its enclosing outline. The settled assumption is the constant `Feed: 1.0` cut-feed scalar and the tab modulo-arc-length gate; the kerf and lead offsets ride the settled Geometry2D substrate and the containment the settled kernel predicate, no second offset owner and no Clipper2 containment re-mint.
