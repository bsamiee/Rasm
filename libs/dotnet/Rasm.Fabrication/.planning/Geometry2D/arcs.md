# [RASM_FABRICATION_ARCS]

`ArcAlgebra` owns admitted planar arc forests, exact arc set operations, material-side offsets, topology inspection, cutter-center engagement motion, and witnessed chord projection. `Loop` remains the package boundary, and native `CavalierContours` and `geometry3Sharp` carriers terminate inside this owner.

`Loop` already holds ONE `Polyline<double>` and ONE `StaticAABB2DIndex<double>` per admitted value, so every offset, Boolean, containment, and intersection query on this page reads `Loop.View` and this owner materializes no second polyline; a provider call that mutates in place takes a detached copy first, because the held view answers every later query on that loop. `Move` construction runs through the sealed `Move.Rapid.Of`, `Move.Linear.Of`, and `Move.Circular.Of` factories, so an emitted path is admitted before it exists.

## [01]-[INDEX]

- [02]-[ARC_VOCABULARY]: `MaterialSide`, `CutSense`, `LeadRole`, `ArcRelation`, `ArcBound`, `ArcForest`, and the `LeadShape`, `ArcProbe`, `ArcProjection`, `ArcOffsetSource`, `ArcOp` request families.
- [03]-[ARC_EVIDENCE]: `ArcLoopEvidence`, `ArcPairEvidence`, `ArcSelfEvidence`, `ArcEvidence`, `ArcMotionEvidence`, `DensifyEvidence`, `RecoverEvidence`, `ArcInspection`, and the `ArcTrace` egress.
- [04]-[ARC_EXECUTION]: `ArcAlgebra.Apply`, the frozen `ArcAlgebra.Densify` projection boundary, and the native kernels behind them.

## [02]-[ARC_VOCABULARY]

- Owner: `ArcForest` owns the coplanar single-context winding admission and preserves its context when a valid set operation produces the empty forest; `MaterialSide`, `CutSense`, and `LeadRole` own their behavioural columns while the Boolean posture is the S0 `BoolKind` this page consumes; `ArcRelation` owns the ONE provider-verdict correspondence both native enums project through; `ArcBound` owns every scalar admission this page runs; `ArcOp` closes offset, Boolean, compensation, lead, engagement, inspection, and cleanup under one generated case family.
- Cases: `MaterialSide` carries `outside` and `inside` with their sign and rotation; `CutSense` carries `climb` and `conventional` with the traversal winding each demands; `LeadRole` carries entry and exit; `LeadShape` carries linear, tangent-arc, and loop forms; `ArcProbe` carries point, nearest-point, pair, measure, bounds, and self-intersection queries — every one a FOREST-level question the per-loop `ProfileOp` family cannot ask, which is why a station probe forwarding `ProfileOp.Sample` for one loop is the deleted form; `ArcProjection` makes chord lowering and residual biarc recovery inverse modalities of one owner.
- Law: `ArcForest` is a MEMBER-declared `[ComplexValueObject]`, never a positional record — a positional declaration mints a public primary constructor beside the generated private one and a record equality pair beside the generated value equality, so construction bypasses `Validate` and the two generated halves collide. `ArcBound` carries the predicate, so one gate axis with three rows replaces a family of near-identical scalar admissions.
- Entry: `ArcForest.Admit` answers on `Fin`, the package's one admission carrier; `Validation` appears only inside an accumulating fan and exits through `.As().ToFin()` at the same expression. `ArcOffsetSource` survives on forest-versus-open-path admission and result identity, and `LeadShape` and `ArcProbe` survive on variant arity and payload timing.
- Auto: `ArcRelation` resolves both provider verdicts through Items-derived frozen indexes forced once, so a verdict costs a hash rather than a roster scan at every Boolean pair and containment probe.
- Growth: a new material posture is one row carrying sign and rotation; a new provider verdict is one `ArcRelation` row carrying its native codes, and both projections derive from it with no arm to add. A new operation, query, lead, or projection modality is one union case and one generated-total dispatch arm.
- Boundary: `ArcOp` never wraps `ArcProjection`, so each concern has one entrypoint. Both ingress families carry tolerance, plane, requested error, and policy values in their admitted input, and every probe re-enters the forest's context — a loop proves tolerance and plane through `Compatible`, a query point proves coplanarity — so no cross-context value answers a forest question.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using CavalierContours.Core;
using CavalierContours.Polyline;
using CavalierContours.Shape;
using CavalierContours.Spatial;
using Foundation.CSharp.Analyzers.Contracts;
using g3;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [VOCABULARY] ----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class MaterialSide {
    public static readonly MaterialSide Outside = new("outside", 1.0, RotationSense.Counterclockwise);
    public static readonly MaterialSide Inside = new("inside", -1.0, RotationSense.Clockwise);

    private double Scale { get; }

    internal double Signed(double distance) => Scale * distance;
}

[SmartEnum<string>]
public sealed partial class CutSense {
    public static readonly CutSense Climb = new("climb", Sign.Positive);
    public static readonly CutSense Conventional = new("conventional", Sign.Negative);

    internal Sign Winding { get; }
}

[SmartEnum<string>]
public sealed partial class ArcBound {
    public static readonly ArcBound Positive = new("positive", static value => double.IsFinite(value) && value > 0.0);
    public static readonly ArcBound NonNegative = new("non-negative", static value => double.IsFinite(value) && value >= 0.0);
    public static readonly ArcBound Finite = new("finite", double.IsFinite);

    internal Func<double, bool> Holds { get; }
}

[SmartEnum<string>]
public sealed partial class LeadRole {
    public static readonly LeadRole Entry = new("entry", false);
    public static readonly LeadRole Exit = new("exit", true);

    private bool Departs { get; }

    internal Fin<Seq<Move>> Emit(Point3d outboard, Point3d cut, double feed, Option<(ArcCenter Centre, double Sweep)> arc) =>
        Departs
            ? Cut(outboard, feed, arc.Map(static row =>
                    (row.Centre with { Sense = row.Centre.Sense.Flipped }, -row.Sweep)))
                .Map(static move => Seq(move))
            : from rapid in Move.Rapid.Of(outboard)
              from move in Cut(cut, feed, arc)
              select Seq(rapid, move);

    private static Fin<Move> Cut(Point3d target, double feed, Option<(ArcCenter Centre, double Sweep)> arc) => arc.Match(
        Some: row => Move.Circular.Of(target, feed, row.Centre, row.Sweep),
        None: () => Move.Linear.Of(target, feed));
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

    private static readonly Lazy<FrozenDictionary<BooleanResultInfo, ArcRelation>> ByBoolean = new(
        static () => Items.ToFrozenDictionary(static row => row.BooleanCode));

    private static readonly Lazy<FrozenDictionary<PlineContainsResult, ArcRelation>> ByContains = new(
        static () => Items
            .Where(static row => row.ContainsCode is not null)
            .ToFrozenDictionary(static row => row.ContainsCode!.Value));

    internal static ArcRelation Of(BooleanResultInfo relation) =>
        ByBoolean.Value.TryGetValue(relation, out ArcRelation? row) ? row : InvalidInput;

    internal static ArcRelation Of(PlineContainsResult relation) =>
        ByContains.Value.TryGetValue(relation, out ArcRelation? row) ? row : InvalidInput;
}

// --- [OWNERS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ArcForest {
    public Seq<Loop> Loops { get; }
    public Context Tolerance { get; }
    public double Plane { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Loop> loops,
        ref Context tolerance,
        ref double plane) {
        if (!(double.IsFinite(plane)
            && loops.ForAll(loop => loop.Closed
                && loop.Tolerance == tolerance
                && Math.Abs(loop.Plane - plane) <= tolerance.Absolute.Value)))
            validationError = new ValidationError(string.Join(" | ", new object?[] { Kind.Curve, None, "arc-forest:structure" }));
    }

    public static Fin<ArcForest> Admit(Seq<Loop> loops, Context tolerance, double plane) =>
        Validate(loops, tolerance, plane, out ArcForest forest).Admitted(forest);
}

[Union]
public abstract partial record LeadShape {
    public sealed record Linear(double Length) : LeadShape;
    public sealed record Tangent(double Radius, double Sweep) : LeadShape;
    public sealed record Loop(double Radius) : LeadShape;
}

[Union]
public abstract partial record ArcProbe {
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
```

## [03]-[ARC_EVIDENCE]

- Owner: `ArcEvidence` owns offset, Boolean, compensation, and cleanup evidence; `ArcMotionEvidence` owns lead and engagement paths with their solver census; `DensifyEvidence` and `RecoverEvidence` own the two projection directions; `ArcInspection` owns exact native measurement; `ArcTrace` owns the one egress family.
- Cases: `ArcEvidence` discriminates its four lanes without empty pair fields or a stage tag, and `ArcMotionEvidence` discriminates lead paths from engagement paths without default counters. `ArcInspection` returns exact native measurements without flattening distinct query results — arc-exact area, path length, and winding from the admitted loops, `ClosestPointResult` projection carrying its owning loop, and per-loop self-intersection census beside the total.
- Law: every engagement column is MEASURED. `Levels` is how many offset levels the stock actually admitted before it exhausted, `DepthMm` the inward reach those levels attained, and the raw census the provider's own segment, slice, and reachable-vertex counts — a column echoing back a caller's own request argument is false evidence and is the deleted form.
- Evidence: `DensifyEvidence` retains the provider-enforced error bound, source and output span census, and bounds; `RecoverEvidence` retains requested and sampled achieved error, source and output span census, fit census, and bounds. `ArcEvidence` and `ArcMotionEvidence` close their operation-specific evidence variants.
- Result: `ArcTrace.Lowering` and `ArcTrace.Recovery` are TOTAL projections over the whole family, so both directions read the same way and a widened family breaks at compile time; the `Option`-shaped half-projection that served only the lowering direction left every recovery consumer spelling its own type test.
- Growth: new provider evidence enriches the existing results rather than minting a parallel one.

```csharp
// --- [EVIDENCE] ------------------------------------------------------------------------
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
public abstract partial record ArcEvidence {
    public sealed record Offset(Seq<ArcLoopEvidence> Loops) : ArcEvidence;
    public sealed record Boolean(Seq<ArcLoopEvidence> Loops, Seq<ArcPairEvidence> Pairs) : ArcEvidence;
    public sealed record Kerf(Seq<ArcLoopEvidence> Loops) : ArcEvidence;
    public sealed record Clean(Seq<ArcLoopEvidence> Loops) : ArcEvidence;
}

[Union]
public abstract partial record ArcMotionEvidence {
    public sealed record Lead(Seq<Move> Path) : ArcMotionEvidence;
    public sealed record Engagement(
        Seq<Move> Path,
        int Levels,
        int RawSegments,
        int ValidSlices,
        int ReachableVertices,
        int EmittedSpans,
        double DepthMm) : ArcMotionEvidence;

    public Seq<Move> Moves => Switch(
        lead: static result => result.Path,
        engagement: static result => result.Path);
}

public sealed record DensifyEvidence(
    Loop Exact,
    Loop Output,
    double ErrorBound,
    int SourceSpans,
    int OutputSpans,
    BoundingBox Bounds);

public sealed record RecoverEvidence(
    Loop Chords,
    Loop Output,
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
    public sealed record Winding(int WindingNumber, bool Covered) : ArcInspection;
    public sealed record Near(Point3d Point, double Distance, int Span, int Loop) : ArcInspection;
    public sealed record Pair(ArcRelation Relation, int BasicIntersections, int Overlaps) : ArcInspection;
    public sealed record Measure(double Area, double Length, Sign Winding) : ArcInspection;
    public sealed record Bounds(BoundingBox Value) : ArcInspection;
    public sealed record Self(Seq<ArcSelfEvidence> Loops, int Intersections) : ArcInspection;
}

[Union]
public abstract partial record ArcTrace {
    public sealed record Forest(ArcForest Geometry, ArcEvidence Evidence) : ArcTrace;
    public sealed record Paths(Seq<Loop> Geometry, ArcEvidence Evidence) : ArcTrace;
    public sealed record Motion(ArcMotionEvidence Evidence) : ArcTrace;
    public sealed record Inspection(ArcInspection Evidence) : ArcTrace;
    public sealed record Densified(DensifyEvidence Evidence) : ArcTrace;
    public sealed record Recovered(RecoverEvidence Evidence) : ArcTrace;

    public Fin<DensifyEvidence> Lowering(FabricationFault refusal) => Switch(
        state: refusal,
        forest: static (fault, _) => Fin.Fail<DensifyEvidence>(fault),
        paths: static (fault, _) => Fin.Fail<DensifyEvidence>(fault),
        motion: static (fault, _) => Fin.Fail<DensifyEvidence>(fault),
        inspection: static (fault, _) => Fin.Fail<DensifyEvidence>(fault),
        densified: static (_, value) => Fin.Succ(value.Evidence),
        recovered: static (fault, _) => Fin.Fail<DensifyEvidence>(fault));

    public Fin<RecoverEvidence> Recovery(FabricationFault refusal) => Switch(
        state: refusal,
        forest: static (fault, _) => Fin.Fail<RecoverEvidence>(fault),
        paths: static (fault, _) => Fin.Fail<RecoverEvidence>(fault),
        motion: static (fault, _) => Fin.Fail<RecoverEvidence>(fault),
        inspection: static (fault, _) => Fin.Fail<RecoverEvidence>(fault),
        densified: static (fault, _) => Fin.Fail<RecoverEvidence>(fault),
        recovered: static (_, value) => Fin.Succ(value.Evidence));
}
```

## [04]-[ARC_EXECUTION]

- Owner: `ArcAlgebra.Apply(ArcOp)` dispatches every manufacturing modality and `ArcAlgebra.Densify(ArcProjection)` alone crosses exact arcs and witnessed chords in either direction.
- Law: this owner emits the offset FAMILY and owns no walk grammar — the strategy, the component partition, and the binding engagement are `Toolpath/skeleton`'s, which selects the case and its arguments; the family walks inward until the stock itself exhausts it, so the depth is what the material admitted rather than a bounding-box heuristic re-deriving a clearance the caller already bound.
- Law: `Loop.View` is the package's ONE materialization per loop, so this owner builds no second polyline and no second index; a provider call that rewrites its receiver — `InvertDirection` — runs on a detached copy, because the held view answers every later query on that loop.
- Law: the two self-intersection questions take DIFFERENT provider walks. The inspection lane publishes a count and takes the collecting `AllSelfIntersectsAsBasic`; the hygiene lane asks only WHETHER one exists and takes `VisitLocalSelfIntersects` plus `VisitGlobalSelfIntersects` under a visitor whose `false` stops the descent at the first hit — the collecting form is those same two walks over an accumulating visitor, so reading `.Count == 0` off it paid for every hit past the first. Both walks are required in either lane: the local one alone answers adjacent-span pairs and a closed two-vertex loop's opposed bulges, and the global one skips exactly those.
- Exemption: `Boundary`, `Family`, and `Fit` are named statement kernels — the material-side probe, the inward level walk, and the residual biarc subdivision are measured numeric bodies.
- Auto: Boolean execution evaluates every subject-clip pair, preserves positive and negative result slices, classifies the complete boundary candidate set against the requested truth function, deduplicates it on the ONE canonical loop preimage, and rebuilds one winding forest. Offset and engagement execution retain `OffsetLoop.ParentLoopIdx`, `IndexedPolyline.SpatialIndex`, raw-offset segment counts, valid-slice counts, and the per-level reachable-vertex census `PlineOffset.PointValidForOffset` decides against the loop's own index over one pooled traversal stack, folded in ONE pass. `CutSense` decides traversal: a path whose winding disagrees inverts before emission, so climb and conventional differ in the emitted order and not only in the arc-center sense. `LeadRole.Exit` reverses the lead's arc sense and drops the approach rapid. Chord recovery splits at the largest sampled residual under a depth budget the chord census itself bounds, and a request that exhausts it answers a typed non-convergence rather than recursing.
- Packages: `CavalierContours.Polyline` supplies raw offset stages, the `PointValidForOffset` collision predicate, Boolean result metadata and subslices, containment, self-intersection visitors, exact arc queries, and error-bounded arc lowering; `CavalierContours.Shape` supplies winding-forest construction, offset, parent lineage, and spatial indexes; `geometry3Sharp.BiArcFit2` supplies residual-driven chord recovery; `LanguageExt` supplies `Validation`, `Traverse`, immutable collections, and typed `Fin` results; `Thinktecture` generates every closed case, policy vocabulary, and admitted value owner.
- Boundary: mutable lists and indexed native loops exist only while materializing provider input and output. Every provider result re-enters through `Loop.Admit` or `ArcForest.Admit`; no provider enum, shape, index, result, or biarc object crosses the owner. Arc space references NO line-space engine: the chord lowering answers a `Polyline<double>` this owner admits directly, and `FromPline` is the one admission the sibling line-space owner composes for the two hygiene rules the atom's provider answers.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArcAlgebra {
    public static Fin<ArcTrace> Apply(ArcOp operation) => operation.Switch(
        offset: static request => request.Source.Switch(
            forest: source => OffsetForest(
                source.Value,
                request.Distance,
                static loops => new ArcEvidence.Offset(loops)),
            path: source => OffsetPath(source.Value, request.Distance)),
        boolean: static request => Boolean(request.Subject, request.Clip, request.Kind),
        kerf: static request => Admits(request.Width, ArcBound.Positive, "arc-kerf:width").Bind(width =>
            OffsetForest(
                request.Forest,
                request.Side.Signed(width / 2.0),
                static loops => new ArcEvidence.Kerf(loops))),
        lead: static request => Lead(request),
        adaptive: static request => Adaptive(request),
        inspect: static request => Inspect(request.Forest, request.Probe),
        clean: static request => Clean(request.Forest));

    public static Fin<ArcTrace> Densify(ArcProjection projection) => projection.Switch(
        lower: static request => Lower(request.Loop, request.Error)
            .Map<ArcTrace>(static result => new ArcTrace.Densified(result)),
        recover: static request => Recover(request.Chords, request.Error, request.ProbeFloor)
            .Map<ArcTrace>(static result => new ArcTrace.Recovered(result)));

    private static Fin<ArcTrace> OffsetForest(
        ArcForest forest,
        double distance,
        Func<Seq<ArcLoopEvidence>, ArcEvidence> result) =>
        Admits(distance, ArcBound.Finite, "arc-offset:distance").Bind(value => ForestOf(
            (forest.Loops.IsEmpty ? Shape<double>.Empty() : ShapeOf(forest.Loops))
                .ParallelOffset(value, ShapeOptions(forest.Tolerance)),
            forest,
            result));

    private static Fin<ArcTrace> OffsetPath(Loop path, double distance) =>
        from value in Admits(distance, ArcBound.Finite, "arc-offset:distance")
        from paths in toSeq(PlineOffset.ParallelOffset<Polyline<double>, double>(
                    path.View.Pline, value, OffsetOptions(path)))
                .Traverse(pline => FromPline(pline, path.Tolerance, path.Plane))
                .As()
        let loops = paths.ToSeq()
        select (ArcTrace)new ArcTrace.Paths(loops, new ArcEvidence.Offset(Census(loops)));

    private static Fin<ArcTrace> Boolean(ArcForest subject, ArcForest clip, BoolKind kind) =>
        Compatible(subject, clip, "arc-boolean").Bind(_ => {
            Seq<(int Subject, int Clip, BooleanResult<Polyline<double>, double> Result)> pairs =
                subject.Loops.Map((first, si) => clip.Loops.Map((second, ci) => (
                    Subject: si,
                    Clip: ci,
                    Result: PlineBoolean.PolylineBoolean<Polyline<double>, double>(
                        first.View.Pline, second.View.Pline, kind.Native,
                        BooleanOptions(first)))))
                    .Bind(static rows => rows);
            Seq<ArcPairEvidence> evidence = pairs.Map(row => new ArcPairEvidence(
                row.Subject,
                row.Clip,
                ArcRelation.Of(row.Result.ResultInfo),
                row.Result.PosPlines.Count,
                row.Result.NegPlines.Count,
                toSeq(row.Result.PosPlines).Fold(0, static (count, result) => count + result.Subslices.Count)
                    + toSeq(row.Result.NegPlines).Fold(0, static (count, result) => count + result.Subslices.Count)));
            return pairs.Exists(static row => row.Result.ResultInfo == BooleanResultInfo.InvalidInput)
                ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, None,
                    "arc-boolean:invalid-input"))
                : pairs
                    .Bind(static row => toSeq(row.Result.PosPlines).Map(static result => result.Pline)
                        + toSeq(row.Result.NegPlines).Map(static result => result.Pline))
                    .Traverse(pline => FromPline(pline, subject.Tolerance, subject.Plane)).As()
                    .Map(rebuilt => rebuilt.ToSeq() + subject.Loops + clip.Loops)
                    .Bind(candidates => candidates
                        .Traverse(candidate => Boundary(candidate, subject.Loops, clip.Loops, kind)).As())
                    .Map(static classified => toSeq(classified
                        .Bind(static candidate => candidate.ToSeq())
                        .DistinctBy(static loop => loop.Canonical())))
                    .Bind(boundary => ForestOf(
                        boundary.IsEmpty ? Shape<double>.Empty() : ShapeOf(boundary),
                        subject,
                        loops => new ArcEvidence.Boolean(loops, evidence)));
        });

    private static Fin<Option<Loop>> Boundary(Loop candidate, Seq<Loop> subjects, Seq<Loop> clips, BoolKind kind) {
        Polyline<double> native = candidate.View.Pline;
        PlineVertex<double> first = native.Get(0);
        PlineVertex<double> second = native.Get(1);
        Vector2<double> point = PlineSeg.SegMidpoint(first, second);
        Vector2<double> tangent = PlineSeg.SegTangentVector(first, second, point);
        double length = Math.Sqrt((tangent.X * tangent.X) + (tangent.Y * tangent.Y));
        double epsilon = candidate.Tolerance.Absolute.Value;
        if (!double.IsFinite(length) || length <= epsilon)
            return Fin.Fail<Option<Loop>>(
                new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-boolean:boundary"));
        double step = Math.Max(epsilon, PlineSeg.SegLength(first, second) * 0.25);
        Vector2<double> normal = new(-tangent.Y / length, tangent.X / length);
        Vector2<double> left = new(point.X + (normal.X * step), point.Y + (normal.Y * step));
        Vector2<double> right = new(point.X - (normal.X * step), point.Y - (normal.Y * step));
        bool materialLeft = kind.Includes(Covers(subjects, left), Covers(clips, left));
        bool materialRight = kind.Includes(Covers(subjects, right), Covers(clips, right));
        return materialLeft == materialRight ? Fin.Succ(Option<Loop>.None)
            : materialRight ? FromPline(Inverted(native), candidate.Tolerance, candidate.Plane).Map(Some)
            : Fin.Succ(Some(candidate));
    }

    private static int Winding(Seq<Loop> loops, Vector2<double> point) =>
        loops.Fold(0, static (winding, loop) => winding + loop.View.Pline.WindingNumber(point));

    private static bool Covers(Seq<Loop> loops, Vector2<double> point) => Winding(loops, point) != 0;

    private static Fin<ArcTrace> Lead(ArcOp.Lead request) =>
        from admitted in (
                Gate(request.Feed, ArcBound.Positive, "arc-lead:feed"),
                Gate(request.Station, ArcBound.NonNegative, "arc-lead:station"))
            .Apply(static (feed, station) => (Feed: feed, Station: station)).As().ToFin()
        from frame in Frame(request.Loop, admitted.Station)
        from path in request.Shape.Switch(
            linear: shape => LinearLead(frame, shape.Length, admitted.Feed, request.Side, request.Role),
            tangent: shape => TangentLead(frame, shape.Radius, shape.Sweep, admitted.Feed, request.Side, request.Role),
            loop: shape => LoopLead(frame, shape.Radius, admitted.Feed, request.Side, request.Role))
        select (ArcTrace)new ArcTrace.Motion(new ArcMotionEvidence.Lead(path));

    private static Fin<ArcTrace> Adaptive(ArcOp.Adaptive request) =>
        request.Stock.Loops.IsEmpty
            ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-adaptive:empty-stock"))
            : (from admitted in (
                       Gate(request.CutterRadius, ArcBound.Positive, "arc-adaptive:cutter-radius"),
                       Gate(request.RadialEngagement, ArcBound.Positive, "arc-adaptive:radial-engagement"),
                       Gate(request.StepOver, ArcBound.Positive, "arc-adaptive:step-over"),
                       Gate(request.Feed, ArcBound.Positive, "arc-adaptive:feed"))
                   .Apply(static (radius, engagement, stepOver, feed) =>
                       (Radius: radius, Engagement: engagement, StepOver: stepOver, Feed: feed))
                   .As().ToFin()
               from radial in admitted.Engagement <= admitted.Radius * 2.0
                   && admitted.StepOver <= admitted.Radius * 2.0
                   ? Fin.Succ(double.Min(admitted.StepOver, admitted.Engagement))
                   : Fin.Fail<double>(new KernelFault.InvalidValue("arcs", "arc-adaptive:engagement"))
               from _ in request.Guide.ToSeq()
                   .Traverse(guide => Compatible(guide, request.Stock, "arc-adaptive:guide")).As()
                   .Map(static _ => unit)
               let family = Family(request.Stock, admitted.Radius, radial)
               from paths in family.Rings
                   .Traverse(pline => FromPline(pline, request.Stock.Tolerance, request.Stock.Plane)).As()
               from pathMoves in request.Guide.ToSeq().Concat(paths.ToSeq())
                   .Traverse(loop => MovePath(loop, admitted.Feed, request.Sense)).As()
               let emitted = pathMoves.Bind(static moves => moves).ToSeq()
               from trace in emitted.IsEmpty
                   ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-adaptive:no-valid-slices"))
                   : Fin.Succ<ArcTrace>(new ArcTrace.Motion(new ArcMotionEvidence.Engagement(
                       emitted,
                       family.Levels,
                       family.Census.RawSegments,
                       family.Census.ValidSlices,
                       family.Census.ReachableVertices,
                       emitted.Count,
                       family.DepthMm)))
               select trace);

    private static (Seq<Polyline<double>> Rings, int Levels, RawCensus Census, double DepthMm) Family(
        ArcForest stock,
        double radius,
        double radial) {
        Shape<double> shape = ShapeOf(stock.Loops);
        List<int> scratch = [];
        int ceiling = (int)Math.Ceiling(stock.Loops.Max(static loop => loop.Bound().Diagonal.Length) / radial) + 1;
        Seq<Polyline<double>> rings = Seq<Polyline<double>>();
        RawCensus census = RawCensus.Empty;
        int level = 0;
        double depth = 0.0;
        while (level < ceiling) {
            double distance = -(radius + (level * radial));
            Shape<double> offset = shape.ParallelOffset(distance, ShapeOptions(stock.Tolerance));
            Seq<Polyline<double>> emitted = toSeq(offset.CcwPlines).Concat(toSeq(offset.CwPlines))
                .Map(static row => row.IndexedPline.Polyline);
            if (emitted.IsEmpty) { break; }
            rings += emitted;
            census = stock.Loops.Fold(census, (running, loop) => running.Add(Raw(loop, distance, scratch)));
            depth = -distance;
            level++;
        }
        return (rings, level, census, depth);
    }

    private static RawCensus Raw(Loop source, double distance, List<int> scratch) {
        double epsilon = source.Tolerance.Absolute.Value;
        Polyline<double> raw = PlineOffset.CreateRawOffsetPolyline<Polyline<double>, double>(
            source.View.Pline, distance, epsilon);
        List<RawPlineOffsetSeg<double>> segments = PlineOffset.CreateUntrimmedRawOffsetSegs(source.View.Pline, distance);
        List<PlineViewData<double>> slices = PlineOffset.SlicesFromRawOffset(
            source.View.Pline, raw, source.View.Index, distance, OffsetOptions(source));
        int reachable = Range(0, raw.Count).Count(index => PlineOffset.PointValidForOffset(
            source.View.Pline, distance, source.View.Index, raw[index].Pos(), scratch, epsilon, epsilon));
        return new RawCensus(segments.Count, slices.Count, reachable);
    }

    private readonly record struct RawCensus(int RawSegments, int ValidSlices, int ReachableVertices) {
        public static readonly RawCensus Empty = new(0, 0, 0);

        public RawCensus Add(RawCensus level) => new(
            RawSegments + level.RawSegments,
            ValidSlices + level.ValidSlices,
            ReachableVertices + level.ReachableVertices);
    }

    private static Fin<ArcTrace> Inspect(ArcForest forest, ArcProbe probe) => probe.Switch(
        point: request => Coplanar(request.Point, forest, "arc-inspect:point")
            .Map(query => Winding(forest.Loops, query))
            .Map(static winding => (ArcTrace)new ArcTrace.Inspection(
                new ArcInspection.Winding(winding, winding != 0))),
        near: request => Coplanar(request.Point, forest, "arc-inspect:near").Bind(query =>
            Near(forest, query)),
        pair: request => Compatible(request.First, forest, "arc-inspect:pair-first")
            .Bind(_ => Compatible(request.Second, forest, "arc-inspect:pair-second"))
            .Bind(_ => Pair(request.First, request.Second)),
        measure: () => Fin.Succ<ArcTrace>(new ArcTrace.Inspection(Measured(forest))),
        bounds: () => forest.Loops.IsEmpty
            ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-inspect:empty-bounds"))
            : Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Bounds(Bounds(forest)))),
        self: () => Self(forest));

    private static ArcInspection Measured(ArcForest forest) {
        (double Area, double Length) total = forest.Loops.Fold(
            (Area: 0.0, Length: 0.0),
            static (running, loop) => (running.Area + loop.Area(), running.Length + loop.Length()));
        return new ArcInspection.Measure(total.Area, total.Length, Sign.Of(total.Area));
    }

    private static Fin<ArcTrace> Near(ArcForest forest, Vector2<double> query) =>
        forest.Loops
            .Map(static (loop, index) => (Loop: index, Result: loop.View.Pline
                .ClosestPoint(query, loop.Tolerance.Absolute.Value)))
            .Filter(static row => row.Result is not null)
            .Fold(Option<(int Loop, ClosestPointResult<double> Result)>.None, static (nearest, row) =>
                nearest.Filter(best => best.Result.Distance <= row.Result!.Value.Distance).IsSome
                    ? nearest
                    : Some((row.Loop, row.Result!.Value)))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-inspect:empty-near"))
            .Map<ArcTrace>(row => new ArcTrace.Inspection(new ArcInspection.Near(
                    new Point3d(row.Result.SegPoint.X, row.Result.SegPoint.Y, forest.Plane),
                    row.Result.Distance,
                    row.Result.SegStartIndex,
                    row.Loop)));

    private static Fin<ArcTrace> Pair(Loop first, Loop second) {
        ArcRelation projected = ArcRelation.Of(PlineContains.PolylineContains(
            first.View.Pline, second.View.Pline, ContainsOptions(first)));
        PlineIntersectsCollection<double> intersects = PlineIntersects.FindIntersects(
            first.View.Pline, second.View.Pline, IntersectOptions(first));
        return projected == ArcRelation.InvalidInput
            ? Fin.Fail<ArcTrace>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-inspect:invalid-pair"))
            : Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Pair(
                projected,
                intersects.BasicIntersects.Count,
                intersects.OverlappingIntersects.Count)));
    }

    private static Fin<ArcTrace> Self(ArcForest forest) {
        Seq<ArcSelfEvidence> rows = forest.Loops.Map(static (loop, index) =>
            new ArcSelfEvidence(index, SelfIntersects(loop)));
        return Fin.Succ<ArcTrace>(new ArcTrace.Inspection(new ArcInspection.Self(
            rows,
            rows.Fold(0, static (count, row) => count + row.Intersections))));
    }

    private static Fin<ArcTrace> Clean(ArcForest forest) => forest.Loops
        .Map(static loop => Reduced(loop))
        .Traverse(pline => FromPline(pline, forest.Tolerance, forest.Plane)
            .Bind(loop => SelfIntersecting(loop)
                ? Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-clean:self-intersection"))
                : Fin.Succ(loop)))
        .As()
        .Bind(rows => ArcForest.Admit(rows.ToSeq(), forest.Tolerance, forest.Plane))
        .Map<ArcTrace>(clean => new ArcTrace.Forest(clean, new ArcEvidence.Clean(Census(clean.Loops))));

    private static Polyline<double> Reduced(Loop loop) {
        double epsilon = loop.Tolerance.Absolute.Value;
        Polyline<double> deduplicated = loop.View.Pline.RemoveRepeatPos(epsilon) ?? loop.View.Pline;
        return deduplicated.RemoveRedundant(epsilon) ?? deduplicated;
    }

    private static int SelfIntersects(Loop loop) => PlineIntersects.AllSelfIntersectsAsBasic(
        loop.View.Pline, loop.View.Index, true, loop.Tolerance.Absolute.Value).Count;

    private sealed class FirstIntersect : IPlineIntersectVisitor<double> {
        public bool VisitBasicIntr(PlineBasicIntersect<double> intr) => false;
        public bool VisitOverlappingIntr(PlineOverlappingIntersect<double> intr) => false;
    }

    private static bool SelfIntersecting(Loop loop) {
        FirstIntersect probe = new();
        double epsilon = loop.Tolerance.Absolute.Value;
        return !PlineIntersects.VisitLocalSelfIntersects(loop.View.Pline, probe, epsilon)
            || !PlineIntersects.VisitGlobalSelfIntersects(loop.View.Pline, loop.View.Index, probe, epsilon);
    }

    private static Fin<DensifyEvidence> Lower(Loop loop, double error) =>
        Admits(error, ArcBound.Positive, "arc-densify:error").Bind(bound =>
            FromPline(loop.View.Pline.ArcsToApproxLines(bound), loop.Tolerance, loop.Plane)
                .Map(result => new DensifyEvidence(loop, result, bound, loop.Spans, result.Spans, result.Bound())));

    private static Fin<RecoverEvidence> Recover(Loop chords, double error, int probeFloor) =>
        Admits(error, ArcBound.Positive, "arc-recover:error").Bind(bound =>
        probeFloor < 1 || chords.Bulges.Exists(static bulge => bulge != 0.0)
            ? Fin.Fail<RecoverEvidence>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-recover:chords"))
            : FitPath(chords, bound, probeFloor)
                .Bind(spans => RecoveredLoop(spans, chords)
                .Map(result => new RecoverEvidence(
                    chords,
                    result,
                    bound,
                    spans.Max(static span => span.Error),
                    chords.Spans,
                    result.Spans,
                    spans.Map(static span => span.Fit).Distinct().Count,
                    spans.Count(static span => span.Bulge != 0.0),
                    spans.Count(static span => span.Bulge == 0.0),
                    result.Bound()))));

    private static Fin<Seq<RecoveredSpan>> FitPath(Loop chords, double error, int probeFloor) {
        Arr<Point3d> nodes = chords.Closed
            ? chords.Vertices.ToSeq().Add(chords.At(0)).ToArr()
            : chords.Vertices;
        if (!chords.Closed) return Fit(nodes, 0, chords.Count - 1, error, probeFloor, chords.Count, chords.Plane);
        int split = int.Max(1, chords.Count / 2);
        return from first in Fit(nodes, 0, split, error, probeFloor, chords.Count, chords.Plane)
               from second in Fit(nodes, split, chords.Count, error, probeFloor, chords.Count, chords.Plane)
               select first + second;
    }

    private static Fin<Loop> RecoveredLoop(Seq<RecoveredSpan> spans, Loop source) =>
        from last in spans.Last.ToFin(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-recover:empty-fit"))
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
        int depth,
        double plane) {
        int interior = last - first - 1;
        if (interior == 0)
            return Fin.Succ(Seq(new RecoveredSpan(nodes[first], nodes[last], 0.0, 0.0, first)));
        if (depth <= 0)
            return Fin.Fail<Seq<RecoveredSpan>>(
                new KernelFault.InvalidValue("arcs", "arc-recover:no-convergence"));
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
                   .Traverse(row => Admits(row.Error, ArcBound.Finite, "arc-recover:residual")
                       .Map(value => (row.Index, Error: value)))
                   .As()
               let worst = valid.Fold(
                   (Index: first + 1, Error: double.MinValue),
                   static (maximum, row) => row.Error > maximum.Error ? row : maximum)
               from spans in worst.Error <= error
                   ? Spans(fit, first, worst.Error, plane)
                   : from left in Fit(nodes, first, worst.Index, error, probeFloor, depth - 1, plane)
                     from right in Fit(nodes, worst.Index, last, error, probeFloor, depth - 1, plane)
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
            : Fin.Fail<RecoveredSpan>(new GeometryFault.DegenerateInput(Kind.Curve, None, "arc-recover:fit")))
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
        Func<Seq<ArcLoopEvidence>, ArcEvidence> result) {
        Seq<OffsetLoop<double>> native = toSeq(shape.CcwPlines).Concat(toSeq(shape.CwPlines));
        return native.Map(static row => row.IndexedPline.Polyline)
            .Traverse(pline => FromPline(pline, source.Tolerance, source.Plane)).As()
            .Bind(loops => ArcForest.Admit(loops.ToSeq(), source.Tolerance, source.Plane))
            .Map<ArcTrace>(forest => new ArcTrace.Forest(forest, result(
                native.Zip(forest.Loops).Map(static pair => new ArcLoopEvidence(
                    pair.Second,
                    pair.First.ParentLoopIdx,
                    pair.Second.Winding(),
                    pair.First.IndexedPline.SpatialIndex.Count,
                    pair.First.IndexedPline.Polyline.SegmentCount())))));
    }

    private static Seq<ArcLoopEvidence> Census(Seq<Loop> loops) => loops.Map(static (loop, index) =>
        new ArcLoopEvidence(loop, index, loop.Winding(), loop.View.Index.Count, loop.View.Pline.SegmentCount()));

    private static Fin<(Point3d Point, Vector3d Normal)> Frame(Loop loop, double station) {
        Polyline<double> source = loop.View.Pline;
        return source.FindPointAtPathLength(station) switch {
            (true, int index, Vector2<double> point, _) =>
                from tangent in Fin.Succ(PlineSeg.SegTangentVector(
                    source.Get(index),
                    source.Get(source.NextWrappingIndex(index)),
                    point))
                from _ in Admits(
                    Math.Sqrt((tangent.X * tangent.X) + (tangent.Y * tangent.Y)),
                    ArcBound.Positive,
                    "arc-lead:tangent")
                select (
                    new Point3d(point.X, point.Y, loop.Plane),
                    new Vector3d(-tangent.Y, tangent.X, 0.0)),
            _ => Fin.Fail<(Point3d, Vector3d)>(
                new KernelFault.InvalidValue("arcs", "arc-lead:station")),
        };
    }

    private static Fin<Seq<Move>> LinearLead(
        (Point3d Point, Vector3d Normal) frame,
        double length,
        double feed,
        MaterialSide side,
        LeadRole role) =>
        Admits(length, ArcBound.Positive, "arc-lead:length").Bind(value => role.Emit(
            frame.Point + (Unit(frame.Normal) * side.Signed(value)),
            frame.Point,
            feed,
            Option<(ArcCenter, double)>.None));

    private static Fin<Seq<Move>> TangentLead(
        (Point3d Point, Vector3d Normal) frame,
        double radius,
        double sweep,
        double feed,
        MaterialSide side,
        LeadRole role) =>
        from admitted in (
                Gate(radius, ArcBound.Positive, "arc-lead:radius"),
                Gate(sweep, ArcBound.Positive, "arc-lead:sweep"))
            .Apply(static (value, angle) => (Radius: value, Sweep: angle)).As().ToFin()
        let center = frame.Point + (Unit(frame.Normal) * side.Signed(admitted.Radius))
        let radial = Rotated(frame.Point - center, -side.Signed(admitted.Sweep))
        from path in role.Emit(
            center + radial,
            frame.Point,
            feed,
            Some((new ArcCenter(center, side.Rotation), side.Signed(admitted.Sweep))))
        select path;

    private static Fin<Seq<Move>> LoopLead(
        (Point3d Point, Vector3d Normal) frame,
        double radius,
        double feed,
        MaterialSide side,
        LeadRole role) =>
        from value in Admits(radius, ArcBound.Positive, "arc-lead:radius")
        let center = frame.Point + (Unit(frame.Normal) * side.Signed(value))
        let arc = new ArcCenter(center, side.Rotation)
        let half = side.Rotation == RotationSense.Counterclockwise ? Math.PI : -Math.PI
        from far in Move.Circular.Of(center + (center - frame.Point), feed, arc, half)
        from back in Move.Circular.Of(frame.Point, feed, arc, half)
        from approach in role == LeadRole.Entry
            ? Move.Rapid.Of(frame.Point).Map(static rapid => Seq(rapid))
            : Fin.Succ(Seq<Move>())
        select approach + Seq(far, back);

    private static Fin<Seq<Move>> MovePath(Loop loop, double feed, CutSense sense) =>
        from oriented in Oriented(loop, sense)
        let source = oriented.View.Pline
        from spans in toSeq(Enumerable.Range(0, source.SegmentCount()))
            .Traverse(index => Span(source, index, oriented.Plane, feed)).As()
        from approach in Move.Rapid.Of(oriented.At(0))
        select Seq(approach) + spans.ToSeq();

    private static Fin<Move> Span(Polyline<double> source, int index, double plane, double feed) {
        PlineVertex<double> start = source.Get(index);
        PlineVertex<double> end = source.Get(source.NextWrappingIndex(index));
        Point3d target = new(end.X, end.Y, plane);
        if (start.Bulge == 0.0) { return Move.Linear.Of(target, feed); }
        Vector2<double> center = PlineSeg.SegArcRadiusAndCenter(start, end).Center;
        double sweep = 4.0 * Math.Atan(start.Bulge);
        return Move.Circular.Of(
            target,
            feed,
            new ArcCenter(
                new Point3d(center.X, center.Y, plane),
                sweep > 0.0 ? RotationSense.Counterclockwise : RotationSense.Clockwise),
            sweep);
    }

    private static Fin<Loop> Oriented(Loop loop, CutSense sense) =>
        !loop.Closed || loop.Winding() == sense.Winding
            ? Fin.Succ(loop)
            : FromPline(Inverted(loop.View.Pline), loop.Tolerance, loop.Plane);

    private static Polyline<double> Inverted(Polyline<double> source) {
        Polyline<double> detached = new(source.IterVertexes(), source.IsClosed);
        detached.InvertDirection();
        return detached;
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
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, None, $"{field}:mixed-context"));

    private static Fin<Unit> Compatible(Loop loop, ArcForest forest, string field) =>
        loop.Tolerance == forest.Tolerance
        && Math.Abs(loop.Plane - forest.Plane) <= forest.Tolerance.Absolute.Value
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, None, $"{field}:mixed-context"));

    private static K<Validation<Error>, double> Gate(double value, ArcBound bound, string field) =>
        bound.Holds(value)
            ? Validation<Error, double>.Success(value)
            : Validation<Error, double>.Fail(new GeometryFault.DegenerateInput(Kind.Curve, None, field));

    private static Fin<double> Admits(double value, ArcBound bound, string field) =>
        Gate(value, bound, field).As().ToFin();

    private static PlineOffsetOptions<double> OffsetOptions(Loop source) => new() {
        AabbIndex = source.View.Index,
        HandleSelfIntersects = true,
        OffsetDistEps = source.Tolerance.Absolute.Value,
        PosEqualEps = source.Tolerance.Absolute.Value,
        SliceJoinEps = source.Tolerance.Absolute.Value,
    };

    private static ShapeOffsetOptions<double> ShapeOptions(Context context) => new(
        posEqualEps: context.Absolute.Value,
        offsetDistEps: context.Absolute.Value,
        sliceJoinEps: context.Absolute.Value);

    private static PlineBooleanOptions<double> BooleanOptions(Loop subject) => new() {
        CollapsedAreaEps = subject.Tolerance.Absolute.Value * subject.Tolerance.Absolute.Value,
        Pline1AabbIndex = subject.View.Index,
        PosEqualEps = subject.Tolerance.Absolute.Value,
    };

    private static PlineContainsOptions<double> ContainsOptions(Loop source) => new() {
        PosEqualEps = source.Tolerance.Absolute.Value,
        Pline1AabbIndex = source.View.Index,
    };

    private static FindIntersectsOptions<double> IntersectOptions(Loop source) => new() {
        PosEqualEps = source.Tolerance.Absolute.Value,
        Pline1AabbIndex = source.View.Index,
    };

    private static BoundingBox Bounds(ArcForest forest) =>
        forest.Loops.Fold(BoundingBox.Empty, static (box, loop) => BoundingBox.Union(box, loop.Bound()));

    private static Shape<double> ShapeOf(Seq<Loop> loops) =>
        Shape<double>.FromPlines(loops.Map(static loop => loop.View.Pline).ToList());

    private static Fin<Vector2<double>> Coplanar(Point3d point, ArcForest forest, string field) =>
        point.IsValid && Math.Abs(point.Z - forest.Plane) <= forest.Tolerance.Absolute.Value
            ? Fin.Succ(new Vector2<double>(point.X, point.Y))
            : Fin.Fail<Vector2<double>>(new GeometryFault.DegenerateInput(Kind.Curve, None, $"{field}:off-plane"));

    private static Vector2d ToG3(Point3d point) => new(point.X, point.Y);

    private static Point3d ToRhino(Vector2d point, double plane) => new(point.x, point.y, plane);

    internal static Fin<Loop> FromPline(IPlineSource<double> pline, Context tolerance, double plane) {
        Seq<PlineVertex<double>> vertices = toSeq(Enumerable.Range(0, pline.VertexCount)).Map(pline.Get);
        return Loop.Admit(
            vertices.Map(vertex => new Point3d(vertex.X, vertex.Y, plane)).ToArr(),
            pline.IsClosed,
            vertices.Map(static vertex => vertex.Bulge).ToArr(),
            tolerance);
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
