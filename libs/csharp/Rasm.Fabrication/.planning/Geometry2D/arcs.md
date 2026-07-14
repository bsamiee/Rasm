# [RASM_FABRICATION_ARCS]

`ArcAlgebra` owns the bulge-carrying 2D rail over `CavalierContours`: kerf compensation over kernel-emitted region offsets with a REAL crossing witness, tangent lead arcs whose endpoints sit ON the lead circle, adaptive trochoidal chord trains that march the whole spine, closed-polyline Boolean carrying BOTH winding families (holes survive as CW loops), island-preserving pocket offset, arc-aware measurement over the RAW winding, pair containment, arc-space hygiene, and the explicit arc-to-line bridge. The canonical boundary remains `Loop` with the parallel `Bulges` column; `Polyline<double>`, `PlineVertex<double>`, `Shape<double>`, `BooleanResult<O,T>`, `PlineContainsResult`, and `StaticAABB2DIndex<double>` stay inside the owner, while `Predicate.Orient2D`, `FabricationFault.KerfCollision`, and kernel `Offsetting.Apply` keep their settled authorities. The boundary map preserves winding exactly as the line owner's does — normalization is the explicit `AsCcw` at the offset/Boolean admission, never a silent re-wind on measurement or egress.

## [01]-[INDEX]

- [01]-[ARC_ALGEBRA]: owns `BoolKind`, `KerfSide`, `LeadKind`, `AdaptiveSense`, `ArcRelation`, `ArcAlgebra.KerfArc`, `LeadArc`, `AdaptiveArc`, `ArcOffset`, `ArcBoolean` (pair and set arity), `ShapeOffset`, `ArcContains`, `ArcClean`, `ArcLength`, `ArcArea`, `ArcWinding`, `SampleAtLength`, and `Densify`.

## [02]-[ARC_ALGEBRA]

- Owner: `BoolKind` maps the closed-polyline set-operation axis to `CavalierContours.Polyline.BooleanOp`; `KerfSide` carries the per-side signed kerf delta over K1 region offsets; `LeadKind` carries the tangent lead-in/lead-out posture (`Direction` selects the center side, arc handedness DERIVES from it — a hand-set clockwise column beside the side it is determined by is the deleted knob); `AdaptiveSense` carries the constant-engagement trochoidal side; `ArcRelation` the containment verdict vocabulary the `PlineContainsResult` boundary translates onto (no `CavalierContours` carrier escapes); `ArcAlgebra` maps canonical bulged `Loop` values to `Polyline<double>`, runs the `CavalierContours` engine, and folds results back to atom-safe `Loop`/`Move` values winding-intact.
- Cases: `BoolKind` rows `or` · `and` · `not` · `xor`; `KerfSide` rows `outside` · `inside`; `LeadKind` rows `in` · `out`; `AdaptiveSense` rows `climb` · `conventional`; `ArcRelation` rows `disjoint` · `first-inside-second` · `second-inside-first` · `intersected`.
- Entry: `KerfArc(Seq<Loop> regionOffsets, double kerf, KerfSide side)` offsets only region loops emitted by kernel K1 and witnesses survivor crossings; `LeadArc(Loop profile, double station, double radius, double feed, LeadKind kind)` emits a tangent quarter-arc whose start and end are BOTH at the lead radius from the emitted `ArcCenter`; `AdaptiveArc(Seq<Move> spine, double engagementRadius, double stepover, double chordLength, double feed, AdaptiveSense sense)` emits the constant-engagement chord train marching EVERY spine edge at `chordLength` steps to its endpoint; `ArcBoolean` carries pair and set arity on ONE name (the set form applies each clip successively across the running loop set); `ArcContains(Loop outer, Loop candidate)` is the arc-exact pair-containment verdict; `ArcClean(Loop, double eps)` the duplicate/collinear hygiene pass; `ArcOffset`, `ShapeOffset`, `ArcLength`, `ArcArea`, `ArcWinding`, `SampleAtLength`, and `Densify` preserve the carried arc block.
- Auto: `KerfArc` folds K1 region loops through `PlineOffset.ParallelOffset` and routes `FabricationFault.KerfCollision(...).ToError()` on a vanished or collapsed offset AND on any surviving pair the `PolylineContains` verdict reports `Intersected` — the crossing loop's own vertex is the locus, never the input head standing in for a collision the gate never located; `LeadArc` derives tangent frames through `FindPointAtPathLength`; `AdaptiveArc` subdivides each spine edge into `ceil(length/chordLength)` chords, each an arc move about the engagement center; `ArcBoolean` reads `BooleanResult.PosPlines` AND `NegPlines` — the positive-only read dropped every hole a Boolean produced; `ShapeOffset` reads `row.IndexedPline.Polyline` off both winding lists (`row.Polyline` is the refuted phantom spelling — `OffsetLoop<T>` carries `ParentLoopIdx` + `IndexedPline`); `ArcContains` translates `PolylineContains` onto `ArcRelation`; `ArcClean` composes `RemoveRepeatPos` + `RemoveRedundant`; offset/Boolean options carry `StaticAABB2DIndex<double>`.
- Receipt: `Seq<Loop>` is the arc-carrying profile evidence, winding intact (a Boolean hole reads back CW through `ArcWinding`/`Winding`); `Seq<Move>` is the motion/program evidence; `FabricationFault.KerfCollision` is the 2703 kerf-failure evidence with a REAL crossing locus; `ArcRelation` the containment verdict; no `CavalierContours` carrier escapes.
- Packages: `CavalierContours.Polyline` (`Polyline<double>`, `PlineVertex<double>`, `PlineOffset.ParallelOffset`, `PlineBoolean.PolylineBoolean`, `PlineContains.PolylineContains`, `PlineContainsResult`, `PlineContainsOptions<double>`, `BooleanOp`, `PlineOffsetOptions<double>`, `PlineBooleanOptions<double>`, `BooleanResult.PosPlines`/`NegPlines`, `BooleanResultPline.Pline`, `PlineSourceExtensions` incl. `RemoveRepeatPos`/`RemoveRedundant`), `CavalierContours.Shape` (`Shape<double>.FromPlines`, `ParallelOffset`, `ShapeOffsetOptions<double>`, `OffsetLoop.IndexedPline`, `IndexedPolyline.Polyline`), `CavalierContours.Spatial` (`StaticAABB2DIndex<double>`, `CreateAabbIndex()`), `CavalierContours.Core` (`Vector2<double>`), `Rasm.Numerics` (`Predicate.Orient2D`), `Rasm.Meshing` (`Offsetting.Apply` K1 source), `Rhino.Geometry`, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new set operation is one `BoolKind` row; a new kerf side is one `KerfSide` row; a new lead arc posture is one `LeadKind` row; a new adaptive engagement side is one `AdaptiveSense` row; a new containment verdict is one `ArcRelation` row; a line-only consumer receives only the `Densify` bridge; zero new arc-polyline carrier surface.
- Boundary: `ArcAlgebra` is the one arc-native owner; a Clipper2 corner fan for an arc-walled profile, a post-hoc `g3.BiArcFit2` refit for a bulge-carrying path, a sibling `Polyline<double>` signature, a hand-rolled O(n²) self-intersection scan, a local Boolean result accessor named `Positive`, a second kerf fault arm, a lead move whose endpoints sit at unequal radii from its `ArcCenter`, a single-chord "traversal" of a longer spine edge, and a measurement that re-winds its input before measuring are the deleted forms.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using CavalierContours.Core;
using CavalierContours.Polyline;
using CavalierContours.Shape;
using CavalierContours.Spatial;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BoolKind {
    public static readonly BoolKind Or = new("or", BooleanOp.Or);
    public static readonly BoolKind And = new("and", BooleanOp.And);
    public static readonly BoolKind Not = new("not", BooleanOp.Not);
    public static readonly BoolKind Xor = new("xor", BooleanOp.Xor);

    public BooleanOp Op { get; }
}

[SmartEnum<string>]
public sealed partial class KerfSide {
    public static readonly KerfSide Outside = new("outside", 1.0);
    public static readonly KerfSide Inside = new("inside", -1.0);

    public double Scale { get; }
    public double Signed(double kerf) => Scale * Math.Abs(kerf);
}

// Direction selects the lead-circle side; arc handedness DERIVES from it (Clockwise => Direction < 0) —
// a hand-set handedness column beside its determinant was the deleted knob.
[SmartEnum<string>]
public sealed partial class LeadKind {
    public static readonly LeadKind In = new("in", startsOffProfile: true, direction: -1.0);
    public static readonly LeadKind Out = new("out", startsOffProfile: false, direction: 1.0);

    public double Direction { get; }
    public bool StartsOffProfile { get; }
    public bool Clockwise => Direction < 0.0;
}

[SmartEnum<string>]
public sealed partial class AdaptiveSense {
    public static readonly AdaptiveSense Climb = new("climb", clockwise: true, scale: 1.0);
    public static readonly AdaptiveSense Conventional = new("conventional", clockwise: false, scale: -1.0);

    public bool Clockwise { get; }
    public double Scale { get; }
}

// The containment verdict vocabulary the PlineContainsResult boundary translates onto — the foreign enum never escapes.
[SmartEnum<string>]
public sealed partial class ArcRelation {
    public static readonly ArcRelation Disjoint = new("disjoint");
    public static readonly ArcRelation FirstInsideSecond = new("first-inside-second");
    public static readonly ArcRelation SecondInsideFirst = new("second-inside-first");
    public static readonly ArcRelation Intersected = new("intersected");
}

// --- [CONSTANTS] ----------------------------------------------------------------------------------------------------------------------------------
file static class ArcPrecision {
    public const double OffsetDist = 1e-6;
    public const double PosEqual = 1e-6;
    public const double SampleStep = 1e-4;
    public const double SliceJoin = 1e-4;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct LeadFrame(Point3d Pierce, Vector3d Tangent, Vector3d Normal);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class ArcAlgebra {
    // Kerf collision is TWO witnesses: a vanished/collapsed offset, and a surviving PAIR that crosses (adjacent
    // regions closer than the kerf — the PolylineContains Intersected verdict at the offending loop's own vertex).
    public static Fin<Seq<Loop>> KerfArc(Seq<Loop> regionOffsets, double kerf, KerfSide side) =>
        OffsetMany(regionOffsets, side.Signed(kerf), "kerf-arc:empty").Bind(arcs =>
            arcs.IsEmpty || arcs.Exists(static loop => ArcLength(loop) <= ArcPrecision.PosEqual)
                ? Fin.Fail<Seq<Loop>>(FabricationFault.KerfCollision(regionOffsets.Head().At(0), Math.Abs(kerf)).ToError())
                : Crossing(arcs).Match(
                    Some: at => Fin.Fail<Seq<Loop>>(FabricationFault.KerfCollision(at, Math.Abs(kerf)).ToError()),
                    None: () => Fin.Succ(arcs)));

    // Tangent quarter-arc: center = pierce + normal·direction·radius; the off-profile endpoint sits ON the circle at
    // center ∓ tangent·radius, so both endpoints are radius-true about the emitted ArcCenter and the arc is tangent
    // to travel at the pierce — the chord-offset endpoint at radius·sqrt(2) was the invalid-arc defect.
    public static Fin<Seq<Move>> LeadArc(Loop profile, double station, double radius, double feed, LeadKind kind) =>
        FrameAt(profile, station).Map(frame => {
            Point3d center = frame.Pierce + (frame.Normal * kind.Direction * radius);
            Point3d offProfile = kind.StartsOffProfile ? center - (frame.Tangent * radius) : center + (frame.Tangent * radius);
            Point3d start = kind.StartsOffProfile ? offProfile : frame.Pierce;
            Point3d end = kind.StartsOffProfile ? frame.Pierce : offProfile;
            return Seq(
                new Move(start, Rapid: kind.StartsOffProfile, Feed: 0.0, Arc: Option<ArcCenter>.None),
                new Move(end, Rapid: false, Feed: feed, Arc: Some(new ArcCenter(center, kind.Clockwise))));
        });

    public static Fin<Seq<Move>> AdaptiveArc(
        Seq<Move> spine,
        double engagementRadius,
        double stepover,
        double chordLength,
        double feed,
        AdaptiveSense sense) {
        Arr<Move> nodes = spine.ToArr();
        return nodes.Count < 2 || chordLength <= 0.0
            ? Fin.Fail<Seq<Move>>(GeometryFault.DegenerateInput("adaptive-arc:spine").ToError())
            : Fin.Succ(toSeq(Enumerable.Range(0, nodes.Count - 1))
                .Bind(i => Trochoid(nodes[i], nodes[i + 1], engagementRadius, stepover, chordLength, feed, sense)));
    }

    public static Fin<Seq<Loop>> ArcOffset(Loop profile, double offset) =>
        Admit(profile.AsCcw(), "arc-offset").Bind(pline => {
            StaticAABB2DIndex<double> index = pline.CreateAabbIndex();
            List<Polyline<double>> result =
                PlineOffset.ParallelOffset<Polyline<double>, double>(pline, offset, OffsetOptions(index));
            return Fin.Succ(Loops(result));
        });

    // BOTH winding families return: PosPlines are the outers, NegPlines the holes a Boolean legitimately produces —
    // winding carries the discrimination (ArcWinding reads it), so no hole is silently dropped and no wrapper minted.
    public static Fin<Seq<Loop>> ArcBoolean(Loop a, Loop b, BoolKind op) =>
        Admit(a.AsCcw(), "arc-boolean:a").Bind(pa =>
            Admit(b.AsCcw(), "arc-boolean:b").Map(pb => {
                PlineBooleanOptions<double> options = new() {
                    PosEqualEps = ArcPrecision.PosEqual,
                    Pline1AabbIndex = pa.CreateAabbIndex(),
                };
                BooleanResult<Polyline<double>, double> result =
                    PlineBoolean.PolylineBoolean<Polyline<double>, double>(pa, pb, op.Op, options);
                return toSeq(result.PosPlines).Map(row => FromPline(row.Pline))
                     + toSeq(result.NegPlines).Map(row => FromPline(row.Pline));
            }));

    // Set arity on the same name: each clip applies successively across the running loop set.
    public static Fin<Seq<Loop>> ArcBoolean(Seq<Loop> subjects, Seq<Loop> clips, BoolKind op) =>
        subjects.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("arc-boolean:empty-subject").ToError())
            : clips.Fold(Fin.Succ(subjects), (state, clip) => state.Bind(acc =>
                acc.Fold(Fin.Succ(Seq<Loop>()), (inner, subject) =>
                    inner.Bind(collected => ArcBoolean(subject, clip, op).Map(r => collected + r)))));

    public static Fin<Seq<Loop>> ShapeOffset(Seq<Loop> loops, double offset) =>
        loops.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("shape-offset:empty").ToError())
            : Fin.Succ(ShapeLoops(Shape<double>.FromPlines(loops.Map(Pline)).ParallelOffset(
                offset,
                new ShapeOffsetOptions<double>(ArcPrecision.PosEqual, ArcPrecision.OffsetDist, ArcPrecision.SliceJoin))));

    // Arc-exact pair containment — the verified engine verdict translated onto the local vocabulary; nesting
    // feasibility and remnant admission on arc profiles read THIS, never a chord-densified Covers probe.
    public static Fin<ArcRelation> ArcContains(Loop first, Loop second) =>
        Admit(first, "arc-contains:first").Bind(pa =>
            Admit(second, "arc-contains:second").Bind(pb =>
                PlineContains.PolylineContains(pa, pb, new PlineContainsOptions<double>()) switch {
                    PlineContainsResult.Pline1InsidePline2 => Fin.Succ(ArcRelation.FirstInsideSecond),
                    PlineContainsResult.Pline2InsidePline1 => Fin.Succ(ArcRelation.SecondInsideFirst),
                    PlineContainsResult.Disjoint => Fin.Succ(ArcRelation.Disjoint),
                    PlineContainsResult.Intersected => Fin.Succ(ArcRelation.Intersected),
                    _ => Fin.Fail<ArcRelation>(GeometryFault.DegenerateInput("arc-contains:invalid").ToError()),
                }));

    // Arc-space hygiene: repeat-position then collinear-redundant removal — ingress vertex noise conditions HERE.
    public static Loop ArcClean(Loop profile, double eps) =>
        FromPline(Pline(profile).RemoveRepeatPos(eps).RemoveRedundant(eps));

    public static double ArcLength(Loop profile) => Pline(profile).PathLength();

    // RAW winding measures: signed area (CCW positive) and the true orientation — the AsCcw-normalized reads that
    // could never report a hole are the deleted forms.
    public static double ArcArea(Loop profile) => Pline(profile).Area();

    public static Sign ArcWinding(Loop profile) =>
        Pline(profile).Orientation() switch {
            PlineOrientation.CounterClockwise => Sign.Positive,
            PlineOrientation.Clockwise => Sign.Negative,
            _ => Sign.Zero,
        };

    public static (bool Hit, Point3d Point) SampleAtLength(Loop profile, double pathLength) {
        (bool Success, int SegIndex, Vector2<double> Point, double AccLength) sample =
            Pline(profile).FindPointAtPathLength(pathLength);
        return (sample.Success, new Point3d(sample.Point.X, sample.Point.Y, 0.0));
    }

    public static Loop Densify(Loop profile, double errorDistance) =>
        FromPline(Pline(profile).ArcsToApproxLines(errorDistance));

    static Fin<Seq<Loop>> OffsetMany(Seq<Loop> loops, double offset, string locus) =>
        loops.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput(locus).ToError())
            : loops.Fold(
                Fin.Succ(Seq<Loop>()),
                (state, loop) => state.Bind(acc => ArcOffset(loop, offset).Map(offsets => acc.Concat(offsets))));

    static Option<Point3d> Crossing(Seq<Loop> arcs) =>
        toSeq(Enumerable.Range(0, arcs.Count))
            .Bind(i => toSeq(Enumerable.Range(i + 1, arcs.Count - i - 1)).Map(j => (I: i, J: j)))
            .Find(pair => PlineContains.PolylineContains(Pline(arcs[pair.I]), Pline(arcs[pair.J]), new PlineContainsOptions<double>())
                == PlineContainsResult.Intersected)
            .Map(pair => arcs[pair.J].At(0));

    static Fin<Polyline<double>> Admit(Loop loop, string locus) =>
        loop.Count < 2 || loop.Vertices.Exists(static p => !double.IsFinite(p.X) || !double.IsFinite(p.Y))
            ? Fin.Fail<Polyline<double>>(GeometryFault.DegenerateInput(locus).ToError())
            : Fin.Succ(Pline(loop));

    // The march: each spine edge subdivides into ceil(length/chord) steps, one engagement arc per step, terminating
    // at the edge endpoint — the single bounded chord that abandoned the edge remainder is the deleted thin slice.
    static Seq<Move> Trochoid(
        Move from,
        Move to,
        double engagementRadius,
        double stepover,
        double chordLength,
        double feed,
        AdaptiveSense sense) {
        Vector3d tangent = to.To - from.To;
        double length = tangent.Length;
        if (!tangent.Unitize()) return Seq<Move>();
        Vector3d normal = new(-tangent.Y, tangent.X, 0.0);
        int chords = int.Max(1, (int)Math.Ceiling(length / chordLength));
        double step = length / chords;
        return toSeq(Enumerable.Range(1, chords)).Map(j => {
            Point3d anchor = from.To + (tangent * ((j - 1) * step));
            Point3d target = from.To + (tangent * (j * step)) + (normal * (sense.Scale * stepover));
            return new Move(target, Rapid: false, Feed: feed, Arc: Some(new ArcCenter(anchor + (normal * (sense.Scale * engagementRadius)), sense.Clockwise)));
        });
    }

    static Fin<LeadFrame> FrameAt(Loop profile, double station) {
        double length = ArcLength(profile);
        if (length <= ArcPrecision.PosEqual)
            return Fin.Fail<LeadFrame>(GeometryFault.DegenerateInput("lead-arc:length").ToError());
        (bool Hit, Point3d Point) before = SampleAtLength(profile, Math.Max(0.0, station - ArcPrecision.SampleStep));
        (bool Hit, Point3d Point) at = SampleAtLength(profile, station);
        (bool Hit, Point3d Point) after = SampleAtLength(profile, Math.Min(length, station + ArcPrecision.SampleStep));
        if (!before.Hit || !at.Hit || !after.Hit)
            return Fin.Fail<LeadFrame>(GeometryFault.DegenerateInput("lead-arc:station").ToError());
        Vector3d tangent = after.Point - before.Point;
        if (!tangent.Unitize())
            return Fin.Fail<LeadFrame>(GeometryFault.DegenerateInput("lead-arc:tangent").ToError());
        Vector3d normal = new(-tangent.Y, tangent.X, 0.0);
        return Fin.Succ(new LeadFrame(at.Point, tangent, normal));
    }

    static PlineOffsetOptions<double> OffsetOptions(StaticAABB2DIndex<double> index) =>
        new() {
            AabbIndex = index,
            HandleSelfIntersects = true,
            OffsetDistEps = ArcPrecision.OffsetDist,
            PosEqualEps = ArcPrecision.PosEqual,
            SliceJoinEps = ArcPrecision.SliceJoin,
        };

    // Winding-preserving boundary map: no AsCcw here — normalization is the explicit call at the offset/Boolean
    // admissions, never a hidden re-wind under a measurement or egress.
    static Polyline<double> Pline(Loop loop) {
        Polyline<double> pline = new(loop.Count, loop.Closed);
        for (int i = 0; i < loop.Count; i++)
            pline.AddVertex(PlineVertex<double>.FromVector2(new Vector2<double>(loop.At(i).X, loop.At(i).Y), loop.BulgeAt(i)));
        return pline;
    }

    static Seq<Loop> Loops(List<Polyline<double>> plines) => toSeq(plines).Map(FromPline);

    static Seq<Loop> ShapeLoops(Shape<double> shape) =>
        toSeq(shape.CcwPlines).Concat(toSeq(shape.CwPlines)).Map(row => FromPline(row.IndexedPline.Polyline));

    static Loop FromPline(IPlineSource<double> pline) {
        List<Point3d> vertices = new(pline.VertexCount);
        List<double> bulges = new(pline.VertexCount);
        for (int i = 0; i < pline.VertexCount; i++) {
            PlineVertex<double> vertex = pline.Get(i);
            vertices.Add(new Point3d(vertex.X, vertex.Y, 0.0));
            bulges.Add(vertex.Bulge);
        }
        return new Loop(toArr(vertices), pline.IsClosed, toArr(bulges));
    }
}
```
