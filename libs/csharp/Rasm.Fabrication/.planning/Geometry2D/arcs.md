# [RASM_FABRICATION_ARCS]

`ArcAlgebra` owns the bulge-carrying 2D rail over `CavalierContours`: kerf compensation over kernel-emitted region offsets, tangent lead arcs, adaptive trochoidal arc chords, closed-polyline Boolean, island-preserving pocket offset, arc-aware measurement, and the explicit arc-to-line bridge. The canonical boundary remains `Loop` with the parallel `Bulges` column; `Polyline<double>`, `PlineVertex<double>`, `Shape<double>`, `BooleanResult<O,T>`, and `StaticAABB2DIndex<double>` stay inside the owner, while `Predicate.Orient2D`, `FabricationFault.KerfCollision`, and kernel `Offsetting.Apply` keep their settled authorities.

## [01]-[INDEX]

- [01]-[ARC_ALGEBRA]: owns `BoolKind`, `KerfSide`, `LeadKind`, `AdaptiveSense`, `ArcAlgebra.KerfArc`, `LeadArc`, `AdaptiveArc`, `ArcOffset`, `ArcBoolean`, `ShapeOffset`, `ArcLength`, `ArcArea`, `ArcWinding`, `SampleAtLength`, and `Densify`.

## [02]-[ARC_ALGEBRA]

- Owner: `BoolKind` maps the closed-polyline set-operation axis to `CavalierContours.Polyline.BooleanOp`; `KerfSide` carries the per-side signed kerf delta over K1 region offsets; `LeadKind` carries the tangent lead-in/lead-out direction and arc orientation; `AdaptiveSense` carries the constant-engagement trochoidal side; `ArcAlgebra` maps canonical bulged `Loop` values to `Polyline<double>`, runs the `CavalierContours` engine, and folds results back to atom-safe `Loop`/`Move` values.
- Cases: `BoolKind` rows `or` · `and` · `not` · `xor`; `KerfSide` rows `outside` · `inside`; `LeadKind` rows `in` · `out`; `AdaptiveSense` rows `climb` · `conventional`.
- Entry: `KerfArc(Seq<Loop> regionOffsets, double kerf, KerfSide side)` offsets only region loops emitted by kernel K1; `LeadArc(Loop profile, double station, double radius, double feed, LeadKind kind)` emits tangent arc moves at a true arc-length station; `AdaptiveArc(Seq<Move> spine, double engagementRadius, double stepover, double chordLength, double feed, AdaptiveSense sense)` emits constant-engagement chord arcs over a skeleton or motion spine; `ArcOffset`, `ArcBoolean`, `ShapeOffset`, `ArcLength`, `ArcArea`, `ArcWinding`, `SampleAtLength`, and `Densify` preserve the carried arc block.
- Auto: `KerfArc` folds K1 region loops through `PlineOffset.ParallelOffset` and routes empty or collapsed output through `FabricationFault.KerfCollision(...).ToError()`; `LeadArc` derives tangent frames through `FindPointAtPathLength`; `AdaptiveArc` offsets spine normals; `ArcBoolean` reads `BooleanResult.PosPlines[].Pline`; `ShapeOffset` reads `IndexedPolyline.Polyline`; offset/Boolean options carry `StaticAABB2DIndex<double>`.
- Receipt: `Seq<Loop>` is the arc-carrying profile evidence; `Seq<Move>` is the motion/program evidence; `FabricationFault.KerfCollision` is the 2703 kerf-failure evidence; no `CavalierContours` carrier escapes.
- Packages: `CavalierContours.Polyline` (`Polyline<double>`, `PlineVertex<double>`, `PlineOffset.ParallelOffset`, `PlineBoolean.PolylineBoolean`, `BooleanOp`, `PlineOffsetOptions<double>`, `PlineBooleanOptions<double>`, `BooleanResult.PosPlines`, `BooleanResultPline.Pline`, `PlineSourceExtensions`), `CavalierContours.Shape` (`Shape<double>.FromPlines`, `ParallelOffset`, `ShapeOffsetOptions<double>`, `IndexedPolyline.Polyline`), `CavalierContours.Spatial` (`StaticAABB2DIndex<double>`, `CreateAabbIndex()`), `CavalierContours.Core` (`Vector2<double>`), `Rasm.Numerics` (`Predicate.Orient2D`), `Rasm.Meshing` (`Offsetting.Apply` K1 source), `Rhino.Geometry`, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new set operation is one `BoolKind` row; a new kerf side is one `KerfSide` row; a new lead arc posture is one `LeadKind` row; a new adaptive engagement side is one `AdaptiveSense` row; a line-only consumer receives only the `Densify` bridge; zero new arc-polyline carrier surface.
- Boundary: `ArcAlgebra` is the one arc-native owner; a Clipper2 corner fan for an arc-walled profile, a post-hoc `g3.BiArcFit2` refit for a bulge-carrying path, a sibling `Polyline<double>` signature, a hand-rolled O(n²) self-intersection scan, a local Boolean result accessor named `Positive`, or a second kerf fault arm is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

// --- [TYPES] ------------------------------------------------------------------------------
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

[SmartEnum<string>]
public sealed partial class LeadKind {
    public static readonly LeadKind In = new("in", startsOffProfile: true, clockwise: false, direction: -1.0);
    public static readonly LeadKind Out = new("out", startsOffProfile: false, clockwise: true, direction: 1.0);

    public bool Clockwise { get; }
    public double Direction { get; }
    public bool StartsOffProfile { get; }
}

[SmartEnum<string>]
public sealed partial class AdaptiveSense {
    public static readonly AdaptiveSense Climb = new("climb", clockwise: true, scale: 1.0);
    public static readonly AdaptiveSense Conventional = new("conventional", clockwise: false, scale: -1.0);

    public bool Clockwise { get; }
    public double Scale { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
file static class ArcPrecision {
    public const double OffsetDist = 1e-6;
    public const double PosEqual = 1e-6;
    public const double SampleStep = 1e-4;
    public const double SliceJoin = 1e-4;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct LeadFrame(Point3d Pierce, Vector3d Tangent, Vector3d Normal);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ArcAlgebra {
    // A kerf that swallows the whole region leaves ZERO offset loops — vacuously ForAll-true — so the
    // non-empty gate is load-bearing; the collision locus reads the INPUT head, the survivors may be empty.
    public static Fin<Seq<Loop>> KerfArc(Seq<Loop> regionOffsets, double kerf, KerfSide side) =>
        OffsetMany(regionOffsets, side.Signed(kerf), "kerf-arc:empty").Bind(arcs =>
            !arcs.IsEmpty && arcs.ForAll(static loop => ArcLength(loop) > ArcPrecision.PosEqual)
                ? Fin.Succ(arcs)
                : Fin.Fail<Seq<Loop>>(FabricationFault.KerfCollision(regionOffsets.Head().At(0), Math.Abs(kerf)).ToError()));

    public static Fin<Seq<Move>> LeadArc(Loop profile, double station, double radius, double feed, LeadKind kind) =>
        FrameAt(profile, station).Map(frame => {
            Vector3d normal = frame.Normal * kind.Direction;
            Point3d center = frame.Pierce + (normal * radius);
            Point3d offProfile = kind.StartsOffProfile
                ? frame.Pierce - (frame.Tangent * radius)
                : frame.Pierce + (frame.Tangent * radius);
            Point3d start = kind.StartsOffProfile ? offProfile : frame.Pierce;
            Point3d end = kind.StartsOffProfile ? frame.Pierce : offProfile;
            Seq<Move> lead = Seq(
                new Move(start, Rapid: kind.StartsOffProfile, Feed: 0.0, Arc: Option<ArcCenter>.None),
                new Move(end, Rapid: false, Feed: feed, Arc: Some(new ArcCenter(center, kind.Clockwise))));
            return lead;
        });

    public static Fin<Seq<Move>> AdaptiveArc(
        Seq<Move> spine,
        double engagementRadius,
        double stepover,
        double chordLength,
        double feed,
        AdaptiveSense sense) {
        Arr<Move> nodes = spine.ToArr();
        return nodes.Count < 2
            ? Fin.Fail<Seq<Move>>(GeometryFault.DegenerateInput("adaptive-arc:spine").ToError())
            : Fin.Succ(AdaptiveChords(nodes, engagementRadius, stepover, chordLength, feed, sense));
    }

    public static Fin<Seq<Loop>> ArcOffset(Loop profile, double offset) =>
        Admit(profile, "arc-offset").Bind(pline => {
            StaticAABB2DIndex<double> index = pline.CreateAabbIndex();
            List<Polyline<double>> result =
                PlineOffset.ParallelOffset<Polyline<double>, double>(pline, offset, OffsetOptions(index));
            return Fin.Succ(Loops(result));
        });

    public static Fin<Seq<Loop>> ArcBoolean(Loop a, Loop b, BoolKind op) =>
        Admit(a, "arc-boolean:a").Bind(pa =>
            Admit(b, "arc-boolean:b").Map(pb => {
                PlineBooleanOptions<double> options = new() {
                    PosEqualEps = ArcPrecision.PosEqual,
                    Pline1AabbIndex = pa.CreateAabbIndex(),
                };
                BooleanResult<Polyline<double>, double> result =
                    PlineBoolean.PolylineBoolean<Polyline<double>, double>(pa, pb, op.Op, options);
                return toSeq(result.PosPlines).Map(row => FromPline(row.Pline));
            }));

    public static Fin<Seq<Loop>> ShapeOffset(Seq<Loop> loops, double offset) =>
        loops.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("shape-offset:empty").ToError())
            : Fin.Succ(ShapeLoops(Shape<double>.FromPlines(loops.Map(Pline)).ParallelOffset(
                offset,
                new ShapeOffsetOptions<double>(ArcPrecision.PosEqual, ArcPrecision.OffsetDist, ArcPrecision.SliceJoin))));

    public static double ArcLength(Loop profile) => Pline(profile).PathLength();

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

    static Fin<Polyline<double>> Admit(Loop loop, string locus) =>
        loop.Count < 2 || loop.Vertices.Exists(static p => !double.IsFinite(p.X) || !double.IsFinite(p.Y))
            ? Fin.Fail<Polyline<double>>(GeometryFault.DegenerateInput(locus).ToError())
            : Fin.Succ(Pline(loop));

    static Seq<Move> AdaptiveChords(
        Arr<Move> nodes,
        double engagementRadius,
        double stepover,
        double chordLength,
        double feed,
        AdaptiveSense sense) =>
        toSeq(Enumerable.Range(0, nodes.Count - 1))
            .Bind(i => Trochoid(nodes[i], nodes[i + 1], engagementRadius, stepover, chordLength, feed, sense));

    static Seq<Move> Trochoid(
        Move from,
        Move to,
        double engagementRadius,
        double stepover,
        double chordLength,
        double feed,
        AdaptiveSense sense) {
        Vector3d tangent = to.To - from.To;
        if (!tangent.Unitize()) return Seq<Move>();
        Vector3d normal = new(-tangent.Y, tangent.X, 0.0);
        Point3d center = from.To + (normal * sense.Scale * engagementRadius);
        Point3d target = from.To + (tangent * Math.Min(chordLength, from.To.DistanceTo(to.To)));
        Point3d shifted = target + (normal * sense.Scale * stepover);
        return Seq(new Move(shifted, Rapid: false, Feed: feed, Arc: Some(new ArcCenter(center, sense.Clockwise))));
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

    static Polyline<double> Pline(Loop loop) {
        Loop ccw = loop.AsCcw();
        Polyline<double> pline = new(ccw.Count, ccw.Closed);
        for (int i = 0; i < ccw.Count; i++)
            pline.AddVertex(PlineVertex<double>.FromVector2(new Vector2<double>(ccw.At(i).X, ccw.At(i).Y), ccw.BulgeAt(i)));
        return pline;
    }

    static Seq<Loop> Loops(List<Polyline<double>> plines) => toSeq(plines).Map(FromPline);

    static Seq<Loop> ShapeLoops(Shape<double> shape) =>
        toSeq(shape.CcwPlines).Concat(toSeq(shape.CwPlines)).Map(row => FromPline(row.Polyline));

    static Loop FromPline(IPlineSource<double> pline) {
        List<Point3d> vertices = new(pline.VertexCount);
        List<double> bulges = new(pline.VertexCount);
        for (int i = 0; i < pline.VertexCount; i++) {
            PlineVertex<double> vertex = pline.Get(i);
            vertices.Add(new Point3d(vertex.X, vertex.Y, 0.0));
            bulges.Add(vertex.Bulge);
        }
        return new Loop(toArr(vertices), pline.IsClosed, toArr(bulges)).AsCcw();
    }
}
```
