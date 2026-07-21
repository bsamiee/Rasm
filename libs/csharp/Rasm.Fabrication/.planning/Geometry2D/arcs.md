# [RASM_FABRICATION_ARCS]

`ArcAlgebra` owns admitted planar arc forests, exact arc set operations, material-side offsets, topology inspection, cutter-center engagement motion, and witnessed chord projection. `Loop` remains the package boundary, and native `CavalierContours` and `geometry3Sharp` carriers terminate inside this owner.

## [01]-[INDEX]

- [02]-[ARC_ALGEBRA]: `ArcForest`, `ArcOp`, `ArcProjection`, `ArcTrace`, `ArcAlgebra.Apply`, and the frozen `ArcAlgebra.Densify` projection seam.

## [02]-[ARC_ALGEBRA]

- Owner: generated `ArcForest.Validate` is the sole coplanar, single-context winding admission and preserves its context when a valid set operation produces the empty forest. `ArcOffsetSource` survives on forest-versus-open-path admission and result identity. `LeadShape` and `ArcProbe` survive on variant arity and payload timing. `ArcOp` closes offset, Boolean, compensation, lead, engagement, inspection, and cleanup under one generated case family. `ArcProjection` survives on its projection consumer and makes chord lowering and residual biarc recovery inverse modalities of the frozen `ArcAlgebra.Densify` seam.
- Cases: `BoolKind` carries `or`, `and`, `not`, and `xor`; `MaterialSide` carries `outside` and `inside`; `CutSense` carries `climb` and `conventional` with the traversal winding each demands; `LeadRole` carries entry and exit; `ArcRelation` carries every provider verdict, including invalid input, and owns the one correspondence both provider enums project through; `LeadShape` carries linear, tangent-arc, and loop forms; `ArcProbe` carries station, point, nearest-point, pair, measure, bounds, and self-intersection queries.
- Entry: `ArcAlgebra.Apply(ArcOp)` dispatches every manufacturing modality. `ArcAlgebra.Densify(ArcProjection)` alone crosses exact arcs and witnessed chords in either direction. `ArcOp` never wraps `ArcProjection`, so each concern has one entrypoint. Both ingress families carry tolerance, plane, requested error, and policy values in their admitted input.
- Auto: Boolean execution evaluates every subject-clip pair, preserves positive and negative result slices, classifies the complete boundary candidate set against the requested truth function, and rebuilds one winding forest. Offset and engagement execution retain `OffsetLoop.ParentLoopIdx`, `IndexedPolyline.SpatialIndex`, raw-offset segment counts, and valid-slice counts. Every loop materializes one `Polyline<double>` and one `StaticAABB2DIndex<double>` that every offset, Boolean, containment, and self-intersection query on that loop reuses. `CutSense` decides traversal: a path whose winding disagrees inverts through `InvertDirection` before emission, so climb and conventional differ in the emitted order and not only in the arc-center sense. `LeadRole.Exit` reverses the lead's arc sense and drops the approach rapid. Chord recovery recursively splits at the largest sampled residual until every accepted `BiArcFit2` span satisfies the requested probe policy.
- Receipt: `ArcReceipt` discriminates offset, Boolean, compensation, and cleanup evidence without empty pair fields or a stage tag. `MotionReceipt` discriminates lead paths from engagement paths without default counters. `DensifyReceipt` retains the provider-enforced error bound, source/output span census, and bounds; `RecoverReceipt` retains requested and sampled achieved error, source/output span census, fit census, and bounds. `ArcInspection` returns exact native measurements without flattening distinct query results: arc-exact area, path length, and winding from the admitted loops, `ClosestPointResult` projection carrying its owning loop, and per-loop self-intersection census beside the total.
- Packages: `CavalierContours.Polyline` supplies raw offset stages, Boolean result metadata and subslices, containment, self-intersection visitors, exact arc queries, bounds, and error-bounded arc lowering; `CavalierContours.Shape` supplies winding-forest construction, offset, parent lineage, and spatial indexes; `geometry3Sharp.BiArcFit2` supplies residual-driven chord recovery; `Clipper2Lib` supplies bounds; `LanguageExt` supplies `Validation`, `Traverse`, immutable collections, and typed `Fin` rails; `Thinktecture` generates every closed case, policy vocabulary, and admitted value owner.
- Growth: a new Boolean posture is one smart-enum row carrying native and truth behavior; a new material posture is one row carrying sign and rotation; a new provider verdict is one `ArcRelation` row carrying its native codes, and both projections derive from it with no arm to add. A new operation, query, lead, or projection modality is one union case and one generated-total dispatch arm. New provider evidence enriches the existing receipts.
- Boundary: Boolean orientation, raw-offset extraction, and residual biarc subdivision are the statement-bearing native and numeric kernels. Mutable lists and indexed native loops exist only while materializing provider input and output. Every probe re-enters the forest's context — a loop proves tolerance and plane through `Compatible`, a query point proves coplanarity — so no cross-context value answers a forest question. Every provider result re-enters through `Loop.Admit` or `ArcForest.Admit`; no provider enum, shape, index, result, or biarc object crosses the owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using CavalierContours.Core;
using CavalierContours.Polyline;
using CavalierContours.Shape;
using CavalierContours.Spatial;
using Clipper2Lib;
using Foundation.CSharp.Analyzers.Contracts;
using g3;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BoolKind {
    public static readonly BoolKind Or = new("or", BooleanOp.Or, static (subject, clip) => subject || clip);
    public static readonly BoolKind And = new("and", BooleanOp.And, static (subject, clip) => subject && clip);
    public static readonly BoolKind Not = new("not", BooleanOp.Not, static (subject, clip) => subject && !clip);
    public static readonly BoolKind Xor = new("xor", BooleanOp.Xor, static (subject, clip) => subject ^ clip);

    internal BooleanOp Native { get; }
    private Func<bool, bool, bool> Rule { get; }

    internal bool Includes(bool subject, bool clip) => Rule(subject, clip);
}

[SmartEnum<string>]
public sealed partial class MaterialSide {
    public static readonly MaterialSide Outside = new("outside", 1.0, RotationSense.Counterclockwise);
    public static readonly MaterialSide Inside = new("inside", -1.0, RotationSense.Clockwise);

    private double Scale { get; }
    internal RotationSense Rotation { get; }

    internal double Signed(double distance) => Scale * distance;
}

[SmartEnum<string>]
public sealed partial class CutSense {
    public static readonly CutSense Climb = new("climb", RotationSense.Counterclockwise, Sign.Positive);
    public static readonly CutSense Conventional = new("conventional", RotationSense.Clockwise, Sign.Negative);

    internal RotationSense Rotation { get; }
    internal Sign Winding { get; }
}

[SmartEnum<string>]
public sealed partial class LeadRole {
    public static readonly LeadRole Entry = new("entry", false);
    public static readonly LeadRole Exit = new("exit", true);

    private bool Departs { get; }

    // Entry rapids to the outboard point then cuts inward; exit resumes at the cut point and
    // sweeps outward, so a reversed arc lead traverses its center the opposite way.
    internal Seq<Move> Emit(Point3d outboard, Point3d cut, double feed, Option<ArcCenter> arc) => Departs
        ? Seq(Cut(outboard, feed, arc.Map(static center => center with { Sense = Opposite(center.Sense) })))
        : Seq<Move>(new Move.Rapid(outboard), Cut(cut, feed, arc));

    private static Move Cut(Point3d target, double feed, Option<ArcCenter> arc) => arc.Match(
        Some: center => new Move.Circular(target, feed, center),
        None: () => (Move)new Move.Linear(target, feed));

    private static RotationSense Opposite(RotationSense sense) => sense == RotationSense.Clockwise
        ? RotationSense.Counterclockwise
        : RotationSense.Clockwise;
}

[SmartEnum<string>]
public sealed partial class ArcRelation {
    public static readonly ArcRelation InvalidInput =
        new("invalid-input", BooleanResultInfo.InvalidInput, PlineContainsResult.InvalidInput);
    public static readonly ArcRelation FirstInsideSecond =
        new("first-inside-second", BooleanResultInfo.Pline1InsidePline2, PlineContainsResult.Pline1InsidePline2);
    public static readonly ArcRelation SecondInsideFirst =
        new("second-inside-first", BooleanResultInfo.Pline2InsidePline1, PlineContainsResult.Pline2InsidePline1);
    public static readonly ArcRelation Disjoint =
        new("disjoint", BooleanResultInfo.Disjoint, PlineContainsResult.Disjoint);
    public static readonly ArcRelation Overlapping = new("overlapping", BooleanResultInfo.Overlapping, null);
    public static readonly ArcRelation Intersected =
        new("intersected", BooleanResultInfo.Intersected, PlineContainsResult.Intersected);

    private BooleanResultInfo BooleanCode { get; }
    private PlineContainsResult? ContainsCode { get; }

    internal static ArcRelation Of(BooleanResultInfo relation) =>
        toSeq(Items).Find(row => row.BooleanCode == relation).IfNone(InvalidInput);

    internal static ArcRelation Of(PlineContainsResult relation) =>
        toSeq(Items).Find(row => row.ContainsCode == relation).IfNone(InvalidInput);
}

// --- [OWNERS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial record ArcForest(Seq<Loop> Loops, Context Tolerance, double Plane) {
    public static Validation<Error, ArcForest> Admit(Seq<Loop> loops, Context tolerance, double plane) =>
        Validate(loops, tolerance, plane, out ArcForest? forest) is null
            ? Validation<Error, ArcForest>.Success(forest!)
            : Validation<Error, ArcForest>.Fail(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-forest:structure").ToError());

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Loop> loops,
        ref Context tolerance,
        ref double plane) =>
        validationError = double.IsFinite(plane)
            && loops.ForAll(loop => loop.Closed
                && loop.Tolerance == tolerance
                && Math.Abs(loop.Plane - plane) <= tolerance.Absolute.Value)
            ? null
            : new ValidationError(message: "Arc forests require closed, coplanar loops in one valid context.");
}

[Union]
public abstract partial record LeadShape {
    public sealed record Linear(double Length) : LeadShape;
    public sealed record Tangent(double Radius, double Sweep) : LeadShape;
    public sealed record Loop(double Radius) : LeadShape;
}

[Union]
public abstract partial record ArcProbe {
    public sealed record AtLength(Loop Loop, double Station) : ArcProbe;
    public sealed record Point(Point3d Point) : ArcProbe;
    public sealed record Near(Point3d Point) : ArcProbe;
    public sealed record Pair(Loop First, Loop Second) : ArcProbe;
    public sealed record Measure : ArcProbe;
    public sealed record Bounds : ArcProbe;
    public sealed record Self : ArcProbe;
}

[Union]
public abstract partial record ArcProjection {
    public sealed record Lower(Loop Loop, double Error) : ArcProjection;
    public sealed record Recover(Loop Chords, double Error, int ProbeFloor) : ArcProjection;
}

[Union]
public abstract partial record ArcOffsetSource {
    public sealed record Forest(ArcForest Value) : ArcOffsetSource;
    public sealed record Path(Loop Value) : ArcOffsetSource;
}

[Union]
public abstract partial record ArcOp {
    public sealed record Offset(ArcOffsetSource Source, double Distance) : ArcOp;
    public sealed record Boolean(ArcForest Subject, ArcForest Clip, BoolKind Kind) : ArcOp;
    public sealed record Kerf(ArcForest Forest, double Width, MaterialSide Side) : ArcOp;
    public sealed record Lead(
        Loop Loop,
        double Station,
        double Feed,
        LeadShape Shape,
        MaterialSide Side,
        LeadRole Role) : ArcOp;
    public sealed record Adaptive(
        ArcForest Stock,
        Option<Loop> Guide,
        double CutterRadius,
        double RadialEngagement,
        double StepOver,
        double Feed,
        CutSense Sense) : ArcOp;
    public sealed record Inspect(ArcForest Forest, ArcProbe Probe) : ArcOp;
    public sealed record Clean(ArcForest Forest) : ArcOp;
}

// --- [EVIDENCE] -----------------------------------------------------------------------------------------------------------------------------------
public readonly record struct ArcLoopEvidence(Loop Output, int Parent, Sign Winding, int IndexItems, int OutputSegments);

public readonly record struct ArcPairEvidence(
    int Subject,
    int Clip,
    ArcRelation Relation,
    int Positive,
    int Negative,
    int SourceSlices);

public readonly record struct ArcSelfEvidence(int Loop, int Intersections);

[Union]
public abstract partial record ArcReceipt {
    public sealed record Offset(Seq<ArcLoopEvidence> Loops) : ArcReceipt;
    public sealed record Boolean(Seq<ArcLoopEvidence> Loops, Seq<ArcPairEvidence> Pairs) : ArcReceipt;
    public sealed record Kerf(Seq<ArcLoopEvidence> Loops) : ArcReceipt;
    public sealed record Clean(Seq<ArcLoopEvidence> Loops) : ArcReceipt;
}

[Union]
public abstract partial record MotionReceipt {
    public sealed record Lead(Seq<Move> Path) : MotionReceipt;
    public sealed record Engagement(
        Seq<Move> Path,
        int Levels,
        int RawSegments,
        int ValidSlices,
        int EmittedSpans,
        double RadialEngagement) : MotionReceipt;

    public Seq<Move> Moves => Switch(
        lead: static receipt => receipt.Path,
        engagement: static receipt => receipt.Path);
}

public sealed record DensifyReceipt(
    Loop Exact,
    Loop Result,
    double ErrorBound,
    int SourceSpans,
    int OutputSpans,
    BoundingBox Bounds);

public sealed record RecoverReceipt(
    Loop Chords,
    Loop Result,
    double ErrorBound,
    double AchievedError,
    int SourceSpans,
    int OutputSpans,
    int Fits,
    int ArcSpans,
    int LinearSpans,
    BoundingBox Bounds);

[Union]
public abstract partial record ArcInspection {
    public sealed record Sample(Point3d Point, double Station, int Span, double Accumulated) : ArcInspection;
    public sealed record Winding(int WindingNumber, bool Covered) : ArcInspection;
    public sealed record Near(Point3d Point, double Distance, int Span, int Loop) : ArcInspection;
    public sealed record Pair(ArcRelation Relation, int BasicIntersections, int Overlaps) : ArcInspection;
    public sealed record Measure(double Area, double Length, Sign Winding) : ArcInspection;
    public sealed record Bounds(BoundingBox Value) : ArcInspection;
    public sealed record Self(Seq<ArcSelfEvidence> Loops, int Intersections) : ArcInspection;
}

[Union]
public abstract partial record ArcTrace {
    public sealed record Forest(ArcForest Result, ArcReceipt Receipt) : ArcTrace;
    public sealed record Paths(Seq<Loop> Result, ArcReceipt Receipt) : ArcTrace;
    public sealed record Motion(MotionReceipt Receipt) : ArcTrace;
    public sealed record Inspection(ArcInspection Result) : ArcTrace;
    public sealed record Densified(DensifyReceipt Receipt) : ArcTrace;
    public sealed record Recovered(RecoverReceipt Receipt) : ArcTrace;

    public Option<DensifyReceipt> DensifiedReceipt => this is Densified row ? Some(row.Receipt) : None;
    public Option<RecoverReceipt> RecoveredReceipt => this is Recovered row ? Some(row.Receipt) : None;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class ArcAlgebra {
    public static Fin<ArcTrace> Apply(ArcOp operation) => operation.Switch(
        offset: static request => request.Source.Switch(
            forest: source => OffsetForest(
                source.Value,
                request.Distance,
                static loops => new ArcReceipt.Offset(loops)),
            path: source => OffsetPath(source.Value, request.Distance)),
        boolean: static request => Boolean(request.Subject, request.Clip, request.Kind),
        kerf: static request => Positive(request.Width, "arc-kerf:width").Bind(width =>
            OffsetForest(
                request.Forest,
                request.Side.Signed(width / 2.0),
                static loops => new ArcReceipt.Kerf(loops))),
        lead: static request => Lead(request),
        adaptive: static request => Adaptive(request),
        inspect: static request => Inspect(request.Forest, request.Probe),
        clean: static request => Clean(request.Forest));

    public static Fin<ArcTrace> Densify(ArcProjection projection) => projection.Switch(
        lower: static request => Lower(request.Loop, request.Error)
            .Map<ArcTrace>(static receipt => new ArcTrace.Densified(receipt)),
        recover: static request => Recover(request.Chords, request.Error, request.ProbeFloor)
            .Map<ArcTrace>(static receipt => new ArcTrace.Recovered(receipt)));

    private static Fin<ArcTrace> OffsetForest(
        ArcForest forest,
        double distance,
        Func<Seq<ArcLoopEvidence>, ArcReceipt> receipt) =>
        Finite(distance, "arc-offset:distance").Bind(value => ForestOf(
            (forest.Loops.IsEmpty
                ? Shape<double>.Empty()
                : Shape<double>.FromPlines(forest.Loops.Map(Pline).ToList()))
                .ParallelOffset(value, ShapeOptions(forest.Tolerance)),
            forest,
            receipt));

    private static Fin<ArcTrace> OffsetPath(Loop path, double distance) =>
        from value in Finite(distance, "arc-offset:distance")
        let source = NativeOf(path)
        from paths in toSeq(PlineOffset.ParallelOffset<Polyline<double>, double>(
                    source.Pline, value, OffsetOptions(path.Tolerance, source.Index)))
                .Traverse(pline => FromPline(pline, path.Tolerance, path.Plane).Map(loop => (Loop: loop, Pline: pline)))
                .As()
        select (ArcTrace)new ArcTrace.Paths(
            paths.Map(static row => row.Loop).ToSeq(),
            new ArcReceipt.Offset(paths.MapIndexed(static (index, row) => new ArcLoopEvidence(
                row.Loop,
                index,
                row.Loop.Winding(),
                row.Pline.CreateAabbIndex().Count,
                row.Pline.SegmentCount())).ToSeq()));

    private static Fin<ArcTrace> Boolean(ArcForest subject, ArcForest clip, BoolKind kind) =>
        Compatible(subject, clip, "arc-boolean").Bind(_ => {
            Seq<Native> subjects = subject.Loops.Map(NativeOf);
            Seq<Native> clips = clip.Loops.Map(NativeOf);
            Seq<(int Subject, int Clip, BooleanResult<Polyline<double>, double> Result)> pairs =
                subjects.MapIndexed((si, first) => clips.MapIndexed((ci, second) => (
                    Subject: si,
                    Clip: ci,
                    Result: PlineBoolean.PolylineBoolean<Polyline<double>, double>(
                        first.Pline, second.Pline, kind.Native,
                        BooleanOptions(first, subject.Tolerance))))).Bind(static rows => rows);
            Seq<ArcPairEvidence> evidence = pairs.Map(row => new ArcPairEvidence(
                row.Subject,
                row.Clip,
                ArcRelation.Of(row.Result.ResultInfo),
                row.Result.PosPlines.Count,
                row.Result.NegPlines.Count,
                toSeq(row.Result.PosPlines).Fold(0, static (count, result) => count + result.Subslices.Count)
                    + toSeq(row.Result.NegPlines).Fold(0, static (count, result) => count + result.Subslices.Count)));
            Seq<(Polyline<double> Pline, Option<Loop> Admitted)> candidates = pairs
                .Bind(static row => toSeq(row.Result.PosPlines).Map(static result => result.Pline)
                    + toSeq(row.Result.NegPlines).Map(static result => result.Pline))
                .Map(static pline => (Pline: pline, Admitted: Option<Loop>.None))
                + subject.Loops.Map(static loop => (Pline: Pline(loop), Admitted: Some(loop)))
                + clip.Loops.Map(static loop => (Pline: Pline(loop), Admitted: Some(loop)));
            return candidates.Traverse(candidate => Boundary(candidate.Pline, candidate.Admitted, subjects, clips,
                    subject.Tolerance, subject.Plane, kind)).As()
                .Bind(classified => {
                    Seq<Polyline<double>> boundary = classified.Bind(static candidate => candidate.ToSeq())
                        .Fold(Seq<Polyline<double>>(), (unique, candidate) =>
                            unique.Exists(item => item.FuzzyEqEps(
                                candidate, subject.Tolerance.Absolute.Value))
                                ? unique
                                : unique.Add(candidate));
                    return pairs.Exists(static row => row.Result.ResultInfo == BooleanResultInfo.InvalidInput)
                        ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1,
                            "arc-boolean:invalid-input").ToError())
                        : ForestOf(
                            boundary.IsEmpty
                                ? Shape<double>.Empty()
                                : Shape<double>.FromPlines(boundary.ToList()),
                            subject,
                            loops => new ArcReceipt.Boolean(loops, evidence));
                });
        });

    private static Fin<Option<Polyline<double>>> Boundary(
        Polyline<double> candidate,
        Option<Loop> admitted,
        Seq<Native> subjects,
        Seq<Native> clips,
        Context tolerance,
        double plane,
        BoolKind kind) =>
        admitted.Match(
                Some: static loop => Fin.Succ(loop),
                None: () => FromPline(candidate, tolerance, plane))
            .Map(Pline)
            .Bind(native => {
                PlineVertex<double> first = native.Get(0);
                PlineVertex<double> second = native.Get(1);
                Vector2<double> point = PlineSeg.SegMidpoint(first, second);
                Vector2<double> tangent = PlineSeg.SegTangentVector(first, second, point);
                double length = Math.Sqrt((tangent.X * tangent.X) + (tangent.Y * tangent.Y));
                if (!double.IsFinite(length) || length <= tolerance.Absolute.Value)
                    return Fin.Fail<Option<Polyline<double>>>(
                        new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-boolean:boundary").ToError());
                Vector2<double> normal = new(-tangent.Y / length, tangent.X / length);
                double epsilon = tolerance.Absolute.Value * 2.0;
                Vector2<double> left = new(point.X + (normal.X * epsilon), point.Y + (normal.Y * epsilon));
                Vector2<double> right = new(point.X - (normal.X * epsilon), point.Y - (normal.Y * epsilon));
                bool materialLeft = kind.Includes(Covers(subjects, left), Covers(clips, left));
                bool materialRight = kind.Includes(Covers(subjects, right), Covers(clips, right));
                if (materialLeft == materialRight) return Fin.Succ(Option<Polyline<double>>.None);
                if (materialRight) native.InvertDirection();
                return Fin.Succ(Some(native));
            });

    private static bool Covers(Seq<Native> natives, Vector2<double> point) =>
        natives.Fold(0, (winding, native) => winding + native.Pline.WindingNumber(point)) != 0;

    private static Fin<ArcTrace> Lead(ArcOp.Lead request) =>
        from admitted in (
                AdmitsMagnitude(request.Feed, "arc-lead:feed"),
                AdmitsStation(request.Station, "arc-lead:station"))
            .Apply(static (feed, station) => (Feed: feed, Station: station)).As().ToFin()
        from frame in Frame(request.Loop, admitted.Station)
        from path in request.Shape.Switch(
            linear: shape => LinearLead(frame, shape.Length, admitted.Feed, request.Side, request.Role),
            tangent: shape => TangentLead(frame, shape.Radius, shape.Sweep, admitted.Feed, request.Side, request.Role),
            loop: shape => LoopLead(frame, shape.Radius, admitted.Feed, request.Side, request.Role))
        from moves in path.Traverse(Move.Admit).As()
        select (ArcTrace)new ArcTrace.Motion(new MotionReceipt.Lead(moves.ToSeq()));

    private static Fin<ArcTrace> Adaptive(ArcOp.Adaptive request) =>
        request.Stock.Loops.IsEmpty
            ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-adaptive:empty-stock").ToError())
            : (from admitted in (
                       AdmitsMagnitude(request.CutterRadius, "arc-adaptive:cutter-radius"),
                       AdmitsMagnitude(request.RadialEngagement, "arc-adaptive:radial-engagement"),
                       AdmitsMagnitude(request.StepOver, "arc-adaptive:step-over"),
                       AdmitsMagnitude(request.Feed, "arc-adaptive:feed"))
                   .Apply(static (radius, engagement, stepOver, feed) =>
                       (Radius: radius, Engagement: engagement, StepOver: stepOver, Feed: feed))
                   .As().ToFin()
               from radial in admitted.Engagement <= admitted.Radius * 2.0
                   && admitted.StepOver <= admitted.Radius * 2.0
                   ? Fin.Succ(double.Min(admitted.StepOver, admitted.Engagement))
                   : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-adaptive:engagement").ToError())
               from _ in request.Guide.ToSeq()
                   .Traverse(guide => Compatible(guide, request.Stock, "arc-adaptive:guide")).As()
                   .Map(static _ => unit)
               let extent = request.Stock.Loops.Map(static loop => loop.Bound().Diagonal.Length).Max()
               from levels in LevelCount(extent, admitted.Radius, radial)
               let distances = toSeq(Enumerable.Range(0, levels))
                   .Map(level => (Level: level, Distance: -(admitted.Radius + (level * radial))))
               let native = request.Stock.Loops.Map(NativeOf)
               let shape = Shape<double>.FromPlines(native.Map(static row => row.Pline).ToList())
               let raw = distances.Bind(row => native.Map(loop =>
                   Raw(loop, request.Stock.Tolerance, row.Distance)))
               let offsets = distances.Map(row => shape.ParallelOffset(
                   row.Distance,
                   ShapeOptions(request.Stock.Tolerance)))
               from admittedPaths in offsets
                   .Bind(static offset => toSeq(offset.CcwPlines).Concat(toSeq(offset.CwPlines)))
                   .Map(static row => row.IndexedPline.Polyline)
                   .Traverse(pline => FromPline(pline, request.Stock.Tolerance, request.Stock.Plane)).As()
               let paths = admittedPaths.ToSeq()
               from pathMoves in request.Guide.ToSeq().Concat(paths)
                   .Traverse(loop => MovePath(loop, admitted.Feed, request.Sense, request.Stock.Tolerance)).As()
               from admittedMoves in pathMoves.Bind(static moves => moves).Traverse(Move.Admit).As()
               let emitted = admittedMoves.ToSeq()
               from trace in emitted.IsEmpty
                   ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-adaptive:no-valid-slices").ToError())
                   : Fin.Succ<ArcTrace>(new ArcTrace.Motion(new MotionReceipt.Engagement(
                       emitted,
                       levels,
                       raw.Fold(0, static (count, level) => count + level.RawSegments),
                       raw.Fold(0, static (count, level) => count + level.ValidSlices),
                       emitted.Count,
                       radial)))
               select trace);

    private static RawLevel Raw(Native source, Context tolerance, double distance) {
        double epsilon = tolerance.Absolute.Value;
        Polyline<double> raw = PlineOffset.CreateRawOffsetPolyline<Polyline<double>, double>(
            source.Pline, distance, epsilon);
        List<RawPlineOffsetSeg<double>> segments = PlineOffset.CreateUntrimmedRawOffsetSegs(source.Pline, distance);
        List<PlineViewData<double>> slices = PlineOffset.SlicesFromRawOffset(
            source.Pline, raw, source.Index, distance, OffsetOptions(tolerance, source.Index));
        return new RawLevel(segments.Count, slices.Count);
    }

    private readonly record struct RawLevel(int RawSegments, int ValidSlices);

    private readonly record struct Native(Polyline<double> Pline, StaticAABB2DIndex<double> Index);

    // Every probe re-enters the forest's own context: a loop or point from another tolerance or
    // plane is admitted material elsewhere, never here.
    private static Fin<ArcTrace> Inspect(ArcForest forest, ArcProbe probe) => probe.Switch(
        atLength: request => Compatible(request.Loop, forest, "arc-inspect:station")
            .Bind(_ => Sample(request.Loop, request.Station)),
        point: request => Coplanar(request.Point, forest, "arc-inspect:point")
            .Map(query => forest.Loops.Fold(0, (winding, loop) => winding + Pline(loop).WindingNumber(query)))
            .Map(static winding => (ArcTrace)new ArcTrace.Inspection(
                new ArcInspection.Winding(winding, winding != 0))),
        near: request => Coplanar(request.Point, forest, "arc-inspect:near").Bind(query =>
            Near(forest, query)),
        pair: request => Compatible(request.First, forest, "arc-inspect:pair-first")
            .Bind(_ => Compatible(request.Second, forest, "arc-inspect:pair-second"))
            .Bind(_ => Pair(request.First, request.Second)),
        measure: () => Fin.Succ<ArcTrace>(new ArcTrace.Inspection(Measured(forest))),
        bounds: () => forest.Loops.IsEmpty
            ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-inspect:empty-bounds").ToError())
            : Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Bounds(Bounds(forest)))),
        self: () => Self(forest));

    private static ArcInspection Measured(ArcForest forest) {
        (double Area, double Length) total = forest.Loops.Map(Pline).Fold(
            (Area: 0.0, Length: 0.0),
            static (running, loop) => (running.Area + loop.Area(), running.Length + loop.PathLength()));
        return new ArcInspection.Measure(total.Area, total.Length, Sign.Of(total.Area));
    }

    private static Fin<ArcTrace> Sample(Loop loop, double station) =>
        NonNegative(station, "arc-inspect:station").Bind(value =>
            Pline(loop).FindPointAtPathLength(value) switch {
                (true, int index, Vector2<double> point, double accumulated) => Fin.Succ<ArcTrace>(
                    new ArcTrace.Inspection(new ArcInspection.Sample(
                        new Point3d(point.X, point.Y, loop.Plane), value, index, accumulated))),
                _ => Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-inspect:outside").ToError()),
            });

    private static Fin<ArcTrace> Near(ArcForest forest, Vector2<double> query) =>
        forest.Loops
            .MapIndexed((index, loop) => (Loop: index, Result: Pline(loop)
                .ClosestPoint(query, forest.Tolerance.Absolute.Value)))
            .Filter(static row => row.Result is not null)
            .Fold(Option<(int Loop, ClosestPointResult<double> Result)>.None, static (nearest, row) =>
                nearest.Filter(best => best.Result.Distance <= row.Result!.Value.Distance).IsSome
                    ? nearest
                    : Some((row.Loop, row.Result!.Value)))
            .Match(
                Some: row => Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Near(
                    new Point3d(row.Result.SegPoint.X, row.Result.SegPoint.Y, forest.Plane),
                    row.Result.Distance,
                    row.Result.SegStartIndex,
                    row.Loop))),
                None: () => Fin.Fail<ArcTrace>(
                    new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-inspect:empty-near").ToError()));

    private static Fin<ArcTrace> Pair(Loop first, Loop second) {
        Native subject = NativeOf(first);
        Polyline<double> clip = Pline(second);
        PlineContainsResult relation = PlineContains.PolylineContains(
            subject.Pline, clip, ContainsOptions(subject, first.Tolerance));
        PlineIntersectsCollection<double> intersects = PlineIntersects.FindIntersects(
            subject.Pline, clip, IntersectOptions(subject, first.Tolerance));
        ArcRelation projected = ArcRelation.Of(relation);
        return projected == ArcRelation.InvalidInput
            ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-inspect:invalid-pair").ToError())
            : Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Pair(
                projected,
                intersects.BasicIntersects.Count,
                intersects.OverlappingIntersects.Count)));
    }

    private static Fin<ArcTrace> Self(ArcForest forest) {
        Seq<ArcSelfEvidence> rows = forest.Loops.Map(NativeOf).MapIndexed((index, native) =>
            new ArcSelfEvidence(index, PlineIntersects.AllSelfIntersectsAsBasic(
                native.Pline, native.Index, true, forest.Tolerance.Absolute.Value).Count));
        return Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Self(
            rows,
            rows.Fold(0, static (count, row) => count + row.Intersections))));
    }

    private static Fin<ArcTrace> Clean(ArcForest forest) => forest.Loops
        .Map(loop => Pline(loop).RemoveRepeatPos(forest.Tolerance.Absolute.Value)
            .RemoveRedundant(forest.Tolerance.Absolute.Value))
        .Map(static pline => new Native(pline, pline.CreateAabbIndex()))
        .Traverse(native => RejectSelf(native, forest)
            .Bind(_ => FromPline(native.Pline, forest.Tolerance, forest.Plane).Map(loop => (Loop: loop, Native: native))))
        .As()
        .Bind(rows => ArcForest
            .Admit(rows.Map(static row => row.Loop).ToSeq(), forest.Tolerance, forest.Plane)
            .ToFin()
            .Map(clean => (Clean: clean, Rows: rows)))
        .Map<ArcTrace>(result => new ArcTrace.Forest(result.Clean, new ArcReceipt.Clean(
            result.Rows.MapIndexed(static (index, row) => new ArcLoopEvidence(
                row.Loop,
                index,
                row.Loop.Winding(),
                row.Native.Index.Count,
                row.Native.Pline.SegmentCount())).ToSeq())));

    private static Fin<Unit> RejectSelf(Native native, ArcForest forest) =>
        PlineIntersects.AllSelfIntersectsAsBasic(
            native.Pline, native.Index, true, forest.Tolerance.Absolute.Value).Count == 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-clean:self-intersection").ToError());

    private static Fin<DensifyReceipt> Lower(Loop loop, double error) =>
        Positive(error, "arc-densify:error").Bind(bound => {
            Polyline<double> dense = Pline(loop).ArcsToApproxLines(bound);
            PathD source = new(dense.IterVertexes().Select(static vertex => new PointD(vertex.X, vertex.Y)));
            return FromPath(source, loop.Closed, loop.Tolerance, loop.Plane).Map(result => new DensifyReceipt(
                loop, result, bound, loop.Spans, result.Spans, Bounds(result)));
        });

    private static Fin<RecoverReceipt> Recover(Loop chords, double error, int probeFloor) =>
        Positive(error, "arc-recover:error").Bind(bound =>
        probeFloor < 1 || chords.Bulges.Exists(static bulge => bulge != 0.0)
            ? Fin.Fail<RecoverReceipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-recover:chords").ToError())
            : FitPath(chords, bound, probeFloor)
                .Bind(spans => RecoveredLoop(spans, chords)
                .Map(result => new RecoverReceipt(
                    chords,
                    result,
                    bound,
                    spans.Map(static span => span.Error).Max(),
                    chords.Spans,
                    result.Spans,
                    spans.Map(static span => span.Fit).Distinct().Count,
                    spans.Count(static span => span.Bulge != 0.0),
                    spans.Count(static span => span.Bulge == 0.0),
                    Bounds(result)))));

    private static Fin<Seq<RecoveredSpan>> FitPath(Loop chords, double error, int probeFloor) {
        Arr<Point3d> nodes = chords.Closed
            ? chords.Vertices.ToSeq().Add(chords.At(0)).ToArr()
            : chords.Vertices;
        if (!chords.Closed) return Fit(nodes, 0, chords.Count - 1, error, probeFloor, chords.Plane);
        int split = int.Max(1, chords.Count / 2);
        return from first in Fit(nodes, 0, split, error, probeFloor, chords.Plane)
               from second in Fit(nodes, split, chords.Count, error, probeFloor, chords.Plane)
               select first + second;
    }

    private static Fin<Loop> RecoveredLoop(Seq<RecoveredSpan> spans, Loop source) =>
        from last in spans.Last.ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-recover:empty-fit").ToError())
        let vertices = source.Closed
            ? spans.Map(static span => span.Start)
            : spans.Map(static span => span.Start).Add(last.End)
        let bulges = source.Closed
            ? spans.Map(static span => span.Bulge)
            : spans.Map(static span => span.Bulge).Add(0.0)
        from loop in Loop.Admit(vertices.ToArr(), source.Closed, bulges.ToArr(), source.Tolerance)
        select loop;

    private static Fin<Seq<RecoveredSpan>> Fit(
        Arr<Point3d> nodes,
        int first,
        int last,
        double error,
        int probeFloor,
        double plane) {
        int interior = last - first - 1;
        if (interior == 0)
            return Fin.Succ(Seq(new RecoveredSpan(nodes[first], nodes[last], 0.0, 0.0, first)));
        Vector2d start = ToG3(nodes[first]);
        Vector2d end = ToG3(nodes[last]);
        Vector2d startTangent = (ToG3(nodes[first + 1]) - start).Normalized;
        Vector2d endTangent = (end - ToG3(nodes[last - 1])).Normalized;
        BiArcFit2 fit = new(start, startTangent, end, endTangent);
        int probes = int.Max(probeFloor, interior);
        Seq<(int Index, double Error)> residuals = toSeq(Enumerable.Range(1, probes))
            .Map(probe => {
                double position = first + ((last - first) * (double)probe / (probes + 1));
                int segment = int.Min(last - 1, (int)Math.Floor(position));
                double fraction = position - segment;
                Vector2d a = ToG3(nodes[segment]);
                Vector2d b = ToG3(nodes[segment + 1]);
                Vector2d point = new(a.x + ((b.x - a.x) * fraction), a.y + ((b.y - a.y) * fraction));
                int split = int.Clamp((int)Math.Round(position), first + 1, last - 1);
                return (split, fit.Distance(point));
            });
        return from valid in residuals
                   .Traverse(row => Finite(row.Error, "arc-recover:residual").Map(value => (row.Index, Error: value)))
                   .As()
               let worst = valid.Fold(
                   (Index: first + 1, Error: double.MinValue),
                   static (maximum, row) => row.Error > maximum.Error ? row : maximum)
               from spans in worst.Error <= error
                   ? Spans(fit, first, worst.Error, plane)
                   : from left in Fit(nodes, first, worst.Index, error, probeFloor, plane)
                     from right in Fit(nodes, worst.Index, last, error, probeFloor, plane)
                     select left + right
               select spans;
    }

    private static Fin<Seq<RecoveredSpan>> Spans(BiArcFit2 fit, int id, double error, double plane) => Seq(
            fit.Arc1IsSegment
                ? Line(fit.Segment1.P0, fit.Segment1.P1, plane, error, id)
                : Arc(fit.Arc1, plane, error, id),
            fit.Arc2IsSegment
                ? Line(fit.Segment2.P0, fit.Segment2.P1, plane, error, id)
                : Arc(fit.Arc2, plane, error, id))
        .Traverse(span => double.IsFinite(span.Bulge) && span.Start.IsValid && span.End.IsValid
            ? Fin.Succ(span)
            : Fin.Fail<RecoveredSpan>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-recover:fit").ToError()))
        .As();

    private static RecoveredSpan Arc(Arc2d arc, double plane, double error, int id) => new(
        ToRhino(arc.P0, plane),
        ToRhino(arc.P1, plane),
        Math.Tan((arc.ArcLength / arc.Radius) * (arc.IsReversed ? -0.25 : 0.25)),
        error,
        id);

    private static RecoveredSpan Line(Vector2d start, Vector2d end, double plane, double error, int id) =>
        new(ToRhino(start, plane), ToRhino(end, plane), 0.0, error, id);

    private readonly record struct RecoveredSpan(Point3d Start, Point3d End, double Bulge, double Error, int Fit);

    private static Fin<ArcTrace> ForestOf(
        Shape<double> shape,
        ArcForest source,
        Func<Seq<ArcLoopEvidence>, ArcReceipt> receipt) {
        Seq<OffsetLoop<double>> native = toSeq(shape.CcwPlines).Concat(toSeq(shape.CwPlines));
        return native.Map(static row => row.IndexedPline.Polyline)
            .Traverse(pline => FromPline(pline, source.Tolerance, source.Plane)).As()
            .Bind(loops => ArcForest.Admit(loops.ToSeq(), source.Tolerance, source.Plane).ToFin())
            .Map<ArcTrace>(forest => new ArcTrace.Forest(forest, receipt(
                native.Zip(forest.Loops).Map(static pair => new ArcLoopEvidence(
                    pair.Second,
                    pair.First.ParentLoopIdx,
                    pair.Second.Winding(),
                    pair.First.IndexedPline.SpatialIndex.Count,
                    pair.First.IndexedPline.Polyline.SegmentCount())))));
    }

    private static Fin<(Point3d Point, Vector3d Normal)> Frame(Loop loop, double station) {
        Polyline<double> source = Pline(loop);
        return source.FindPointAtPathLength(station) switch {
            (true, int index, Vector2<double> point, _) =>
                from tangent in Fin.Succ(PlineSeg.SegTangentVector(
                    source.Get(index),
                    source.Get(source.NextWrappingIndex(index)),
                    point))
                from _ in Positive(
                    Math.Sqrt((tangent.X * tangent.X) + (tangent.Y * tangent.Y)),
                    "arc-lead:tangent")
                select (
                    new Point3d(point.X, point.Y, loop.Plane),
                    new Vector3d(-tangent.Y, tangent.X, 0.0)),
            _ => Fin.Fail<(Point3d, Vector3d)>(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-lead:station").ToError()),
        };
    }

    private static Fin<Seq<Move>> LinearLead(
        (Point3d Point, Vector3d Normal) frame,
        double length,
        double feed,
        MaterialSide side,
        LeadRole role) => Positive(length, "arc-lead:length").Map(value => role.Emit(
            frame.Point + (Unit(frame.Normal) * side.Signed(value)),
            frame.Point,
            feed,
            Option<ArcCenter>.None));

    private static Fin<Seq<Move>> TangentLead(
        (Point3d Point, Vector3d Normal) frame,
        double radius,
        double sweep,
        double feed,
        MaterialSide side,
        LeadRole role) =>
        from admitted in (AdmitsMagnitude(radius, "arc-lead:radius"), AdmitsMagnitude(sweep, "arc-lead:sweep"))
            .Apply(static (value, angle) => (Radius: value, Sweep: angle)).As().ToFin()
        let center = frame.Point + (Unit(frame.Normal) * side.Signed(admitted.Radius))
        let radial = Rotated(frame.Point - center, -side.Signed(admitted.Sweep))
        select role.Emit(center + radial, frame.Point, feed, Some(new ArcCenter(center, side.Rotation)));

    // A loop lead closes on its own start, so both roles traverse the same two arcs; only entry rapids in.
    private static Fin<Seq<Move>> LoopLead(
        (Point3d Point, Vector3d Normal) frame,
        double radius,
        double feed,
        MaterialSide side,
        LeadRole role) => Positive(radius, "arc-lead:radius").Map(value => {
            Point3d center = frame.Point + (Unit(frame.Normal) * side.Signed(value));
            ArcCenter arc = new(center, side.Rotation);
            Seq<Move> orbit = Seq<Move>(
                new Move.Circular(center + (center - frame.Point), feed, arc),
                new Move.Circular(frame.Point, feed, arc));
            return role == LeadRole.Entry ? orbit.Insert(0, new Move.Rapid(frame.Point)) : orbit;
        });

    private static Fin<Seq<Move>> MovePath(Loop loop, double feed, CutSense sense, Context tolerance) =>
        Oriented(loop, sense, tolerance).Map(oriented => {
            Polyline<double> source = Pline(oriented);
            return Seq<Move>(new Move.Rapid(oriented.At(0))).Concat(
                toSeq(Enumerable.Range(0, source.SegmentCount())).Map(index => {
                    PlineVertex<double> start = source.Get(index);
                    PlineVertex<double> end = source.Get(source.NextWrappingIndex(index));
                    Point3d target = new(end.X, end.Y, oriented.Plane);
                    if (start.Bulge == 0.0) return (Move)new Move.Linear(target, feed);
                    Vector2<double> center = PlineSeg.SegArcRadiusAndCenter(start, end).Center;
                    return new Move.Circular(
                        target,
                        feed,
                        new ArcCenter(new Point3d(center.X, center.Y, oriented.Plane), sense.Rotation));
                }));
        });

    // Cut sense is the traversal law, not a label: a path whose winding disagrees reverses before emission.
    private static Fin<Loop> Oriented(Loop loop, CutSense sense, Context tolerance) {
        if (!loop.Closed || loop.Winding() == sense.Winding) return Fin.Succ(loop);
        Polyline<double> source = Pline(loop);
        source.InvertDirection();
        return FromPline(source, tolerance, loop.Plane);
    }

    private static Vector3d Unit(Vector3d value) {
        Vector3d unit = value;
        unit.Unitize();
        return unit;
    }

    private static Vector3d Rotated(Vector3d value, double angle) {
        Vector3d rotated = value;
        rotated.Rotate(angle, Vector3d.ZAxis);
        return rotated;
    }

    private static Fin<Unit> Compatible(ArcForest first, ArcForest second, string field) =>
        first.Tolerance == second.Tolerance
        && Math.Abs(first.Plane - second.Plane) <= first.Tolerance.Absolute.Value
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"{field}:mixed-context").ToError());

    private static Fin<Unit> Compatible(Loop loop, ArcForest forest, string field) =>
        loop.Tolerance == forest.Tolerance
        && Math.Abs(loop.Plane - forest.Plane) <= forest.Tolerance.Absolute.Value
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"{field}:mixed-context").ToError());

    private static Fin<int> LevelCount(double extent, double radius, double step) {
        double count = Math.Ceiling(double.Max(0.0, (extent / 2.0) - radius) / step) + 1.0;
        return double.IsFinite(count) && count is >= 1.0 and <= Array.MaxLength
            ? Fin.Succ((int)count)
            : Fin.Fail<int>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "arc-adaptive:levels").ToError());
    }

    // K-kinded gates fan independent fields into one tuple Apply and report together;
    // Fin projections short-circuit single-gate sites.
    private static K<Validation<Error>, double> AdmitsMagnitude(double value, string field) =>
        double.IsFinite(value) && value > 0.0
            ? Validation<Error, double>.Success(value)
            : Validation<Error, double>.Fail(new GeometryFault.DegenerateInput(Kind.Curve, -1, field).ToError());

    private static K<Validation<Error>, double> AdmitsStation(double value, string field) =>
        double.IsFinite(value) && value >= 0.0
            ? Validation<Error, double>.Success(value)
            : Validation<Error, double>.Fail(new GeometryFault.DegenerateInput(Kind.Curve, -1, field).ToError());

    private static Fin<double> Positive(double value, string field) =>
        AdmitsMagnitude(value, field).As().ToFin();

    private static Fin<double> NonNegative(double value, string field) =>
        AdmitsStation(value, field).As().ToFin();

    private static Fin<double> Finite(double value, string field) =>
        double.IsFinite(value)
            ? Fin.Succ(value)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, field).ToError());

    // Every offset threads the once-built index and admits self-intersecting profiles; a kerf or
    // clearing pass over a re-entrant loop is ordinary input, never a rejected one.
    private static PlineOffsetOptions<double> OffsetOptions(
        Context context,
        StaticAABB2DIndex<double> index) => new() {
        AabbIndex = index,
        HandleSelfIntersects = true,
        OffsetDistEps = context.Absolute.Value,
        PosEqualEps = context.Absolute.Value,
        SliceJoinEps = context.Absolute.Value,
    };

    private static ShapeOffsetOptions<double> ShapeOptions(Context context) =>
        new(context.Absolute.Value, context.Absolute.Value, context.Absolute.Value);

    private static int Precision(Context context) =>
        int.Clamp((int)Math.Ceiling(-Math.Log10(context.Absolute.Value)), -8, 8);

    private static PlineBooleanOptions<double> BooleanOptions(Native subject, Context context) => new() {
        CollapsedAreaEps = context.Absolute.Value * context.Absolute.Value,
        Pline1AabbIndex = subject.Index,
        PosEqualEps = context.Absolute.Value,
    };

    private static PlineContainsOptions<double> ContainsOptions(Native source, Context context) => new() {
        PosEqualEps = context.Absolute.Value,
        Pline1AabbIndex = source.Index,
    };

    private static FindIntersectsOptions<double> IntersectOptions(Native source, Context context) => new() {
        PosEqualEps = context.Absolute.Value,
        Pline1AabbIndex = source.Index,
    };

    private static BoundingBox Bounds(ArcForest forest) =>
        forest.Loops.Fold(BoundingBox.Empty, static (box, loop) => BoundingBox.Union(box, loop.Bound()));

    private static BoundingBox Bounds(Loop loop) => loop.Bound();

    private static Native NativeOf(Loop loop) {
        Polyline<double> pline = Pline(loop);
        return new Native(pline, pline.CreateAabbIndex());
    }

    private static Polyline<double> Pline(Loop loop) => new(
        loop.Vertices.Zip(loop.Bulges)
            .Map(static row => new PlineVertex<double>(row.First.X, row.First.Y, row.Second)).ToList(),
        loop.Closed);

    private static Fin<Vector2<double>> Coplanar(Point3d point, ArcForest forest, string field) =>
        point.IsValid && Math.Abs(point.Z - forest.Plane) <= forest.Tolerance.Absolute.Value
            ? Fin.Succ(new Vector2<double>(point.X, point.Y))
            : Fin.Fail<Vector2<double>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"{field}:off-plane").ToError());

    private static Vector2d ToG3(Point3d point) => new(point.X, point.Y);

    private static Point3d ToRhino(Vector2d point, double plane) => new(point.x, point.y, plane);

    private static Fin<Loop> FromPath(PathD path, bool closed, Context tolerance, double plane) =>
        Loop.Admit(path.Map(point => new Point3d(point.x, point.y, plane)).ToArr(), closed,
            toArr(Enumerable.Repeat(0.0, path.Count)), tolerance);

    private static Fin<Loop> FromPline(IPlineSource<double> pline, Context tolerance, double plane) {
        Seq<PlineVertex<double>> vertices = toSeq(Enumerable.Range(0, pline.VertexCount)).Map(pline.Get);
        return Loop.Admit(
            vertices.Map(vertex => new Point3d(vertex.X, vertex.Y, plane)).ToArr(),
            pline.IsClosed,
            vertices.Map(static vertex => vertex.Bulge).ToArr(),
            tolerance);
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
