# [RASM_FABRICATION_ARCS]

`ArcAlgebra` owns bulged planar profiles over `CavalierContours`. It offsets profiles and shape forests, evaluates set operations without losing holes, resolves exact arc containment, emits tangent leads and adaptive engagement motion, measures raw winding, cleans profile noise, and lowers arcs to witnessed line approximations. `Loop` remains the package boundary, and every `CavalierContours` carrier stays private to this owner.

## [01]-[INDEX]

- [01]-[ARC_ALGEBRA]: `BoolKind`, `KerfSide`, `LeadKind`, `AdaptiveSense`, `ArcRelation`, `LeadPolicy`, `AdaptivePolicy`, `DensifyReceipt`, and the single `ArcAlgebra` operation owner.

## [02]-[ARC_ALGEBRA]

- Owner: `BoolKind` is the local Boolean vocabulary; the private generated `Native` Map alone projects it to `BooleanOp`, and set execution rides the same generated total dispatch, so a new row breaks both seams at compile time instead of inheriting a default arm. `KerfSide` carries the signed compensation direction. `LeadKind` derives circle side, start posture, and handedness from one `Direction`. `AdaptiveSense` derives orbit side and handedness from one `Scale`. `ArcRelation` translates `PlineContainsResult` without leaking the foreign enum.
- Cases: `BoolKind` has `or`, `and`, `not`, and `xor`; `KerfSide` has `outside` and `inside`; `LeadKind` has `in` and `out`; `AdaptiveSense` has `climb` and `conventional`; `ArcRelation` has `disjoint`, `first-inside-second`, `second-inside-first`, and `intersected`.
- Entry: `KerfArc` compensates a non-empty, single-context closed region set and returns typed `Vanished` or `Overlapped` witnesses. `LeadArc` consumes an admitted `LeadPolicy`. `AdaptiveArc` consumes a point spine and an admitted `AdaptivePolicy`. `ArcBoolean` is one arity-polymorphic operation whose input shape discriminates the exact 1x1 pair from set folding. `ArcOffset`, `ShapeOffset`, `ArcContains`, `ArcClean`, `ArcLength`, `ArcArea`, `ArcWinding`, `SampleAtLength`, and `Densify` share the same `Loop` boundary.
- Auto: `KerfArc` preserves source ordinals through offset fan-out, detects empty and tolerance-degenerate results, and then checks every survivor relation. A 1x1 `ArcBoolean` request reads both `PosPlines` and `NegPlines`, so outer and hole winding survives. Larger sets admit simple positive regions and fail typed if an intermediate result requires forest-level hole semantics the pairwise engine cannot preserve. `ShapeOffset` alone consumes complete winding forests through `Shape<double>`. `ArcClean` composes `RemoveRepeatPos` and `RemoveRedundant`; `AdaptiveArc` places one engagement orbit at every `StepOver` station through every spine edge.
- Receipt: `DensifyReceipt` carries the lowered `Loop`, the requested error bound, and the source/output span census. `FabricationFault.KerfCollision` carries region ordinals rather than an invented collision point. `ArcRelation` carries exact pair containment, and `Seq<Loop>` preserves outer and hole winding.
- Packages: `CavalierContours.Polyline` supplies `Polyline<double>`, `PlineVertex<double>`, `PlineOffset.ParallelOffset`, `PlineBoolean.PolylineBoolean`, `PlineContains.PolylineContains`, `FindPointAtPathLength`, `ArcsToApproxLines`, `RemoveRepeatPos`, and `RemoveRedundant`; `CavalierContours.Shape` supplies `Shape<double>.FromPlines`, `ParallelOffset`, and `OffsetLoop.IndexedPline.Polyline`; `CavalierContours.Spatial` supplies `StaticAABB2DIndex<double>` and `CreateAabbIndex()`; `Loop`, `Move`, `ArcCenter`, `Context`, `FabricationFault`, and `KerfWitness` remain package vocabulary.
- Growth: a new Boolean modality is one `BoolKind` row plus its `Native` Map arm and set-dispatch arm — both generated-total, so the row cannot compile with borrowed semantics; a new lead or engagement posture is one policy value over the existing kind; a new arc-to-line consumer composes `DensifyReceipt` without minting another bridge.
- Boundary: all foreign curve, shape, index, Boolean-result, and containment carriers terminate inside `ArcAlgebra`. Measurement preserves raw winding, offset and Boolean operations reject mixed tolerance contexts and cross-plane inputs, every emitted profile re-enters through `Loop.Admit`, and every result re-emits on the admitted `Loop.Plane` elevation rather than `Z = 0`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
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
    public static readonly BoolKind Or = new("or");
    public static readonly BoolKind And = new("and");
    public static readonly BoolKind Not = new("not");
    public static readonly BoolKind Xor = new("xor");
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
    public static readonly LeadKind In = new("in", direction: -1.0);
    public static readonly LeadKind Out = new("out", direction: 1.0);

    public double Direction { get; }
    public bool StartsOffProfile => Direction < 0.0;
    public RotationSense Sense => Direction < 0.0 ? RotationSense.Clockwise : RotationSense.Counterclockwise;
}

[SmartEnum<string>]
public sealed partial class AdaptiveSense {
    public static readonly AdaptiveSense Climb = new("climb", scale: 1.0);
    public static readonly AdaptiveSense Conventional = new("conventional", scale: -1.0);

    public double Scale { get; }
    public RotationSense Sense => Scale > 0.0 ? RotationSense.Clockwise : RotationSense.Counterclockwise;
}

// The containment verdict vocabulary the PlineContainsResult boundary translates onto — the foreign enum never escapes.
[SmartEnum<string>]
public sealed partial class ArcRelation {
    public static readonly ArcRelation Disjoint = new("disjoint");
    public static readonly ArcRelation FirstInsideSecond = new("first-inside-second");
    public static readonly ArcRelation SecondInsideFirst = new("second-inside-first");
    public static readonly ArcRelation Intersected = new("intersected");
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record LeadPolicy {
    private LeadPolicy(double station, double radius, double feed, LeadKind kind) =>
        (Station, Radius, Feed, Kind) = (station, radius, feed, kind);

    public double Station { get; }
    public double Radius { get; }
    public double Feed { get; }
    public LeadKind Kind { get; }

    public static Fin<LeadPolicy> Admit(double station, double radius, double feed, LeadKind kind) =>
        double.IsFinite(station) && station >= 0.0
        && double.IsFinite(radius) && radius > 0.0
        && double.IsFinite(feed) && feed > 0.0
            ? Fin.Succ(new LeadPolicy(station, radius, feed, kind))
            : Fin.Fail<LeadPolicy>(GeometryFault.DegenerateInput("lead-policy").ToError());
}

public sealed record AdaptivePolicy {
    private AdaptivePolicy(double engagementRadius, double stepOver, double feed, AdaptiveSense sense) =>
        (EngagementRadius, StepOver, Feed, Sense) = (engagementRadius, stepOver, feed, sense);

    public double EngagementRadius { get; }
    public double StepOver { get; }
    public double Feed { get; }
    public AdaptiveSense Sense { get; }

    public static Fin<AdaptivePolicy> Admit(
        double engagementRadius,
        double stepOver,
        double feed,
        AdaptiveSense sense) =>
        double.IsFinite(engagementRadius) && engagementRadius > 0.0
        && double.IsFinite(stepOver) && stepOver > 0.0 && stepOver < engagementRadius * 2.0
        && double.IsFinite(feed) && feed > 0.0
            ? Fin.Succ(new AdaptivePolicy(engagementRadius, stepOver, feed, sense))
            : Fin.Fail<AdaptivePolicy>(GeometryFault.DegenerateInput("adaptive-policy").ToError());
}

public readonly record struct DensifyReceipt(Loop Result, double ErrorBound, int SourceSpans, int OutputSpans);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class ArcAlgebra {
    public static Fin<Seq<Loop>> KerfArc(Seq<Loop> regionOffsets, double kerf, KerfSide side) =>
        regionOffsets.IsEmpty || regionOffsets.Exists(static loop => !loop.Closed)
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("kerf-arc:regions").ToError())
        : regionOffsets.Map(static loop => loop.Tolerance).Distinct().Count > 1
          || regionOffsets.Exists(loop => Math.Abs(loop.Plane - regionOffsets.Head.Plane) > loop.Tolerance.Absolute.Value)
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("kerf-arc:mixed-context").ToError())
        : !double.IsFinite(kerf) || kerf <= 0.0
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("kerf-arc:kerf").ToError())
            : from groups in Range(0, regionOffsets.Count)
                  .Traverse(source => ArcOffset(regionOffsets[source], side.Signed(kerf))
                      .Map(offsets => (Source: source, Offsets: offsets)))
                  .As()
              let survivors = groups.Bind(static group => group.Offsets.Map(loop => (group.Source, Loop: loop)))
              from lengths in survivors.Traverse(row => ArcLength(row.Loop)
                  .Map(length => (row.Source, row.Loop, Length: length)))
                  .As()
              let vanished = groups.Find(static group => group.Offsets.IsEmpty)
                      .Map(static group => (KerfWitness)new KerfWitness.Vanished(group.Source))
                  | lengths.Find(static row => row.Length <= row.Loop.Tolerance.Absolute.Value)
                      .Map(static row => (KerfWitness)new KerfWitness.Vanished(row.Source))
              from admitted in vanished.Match(
                  Some: witness => KerfFailure(witness, kerf),
                  None: () => Collision(survivors).Match(
                      Some: pair => KerfFailure(new KerfWitness.Overlapped(pair.First, pair.Second), kerf),
                      None: () => Fin.Succ(survivors.Map(static row => row.Loop))))
              select admitted;

    // The center-normal and endpoint-tangent construction keeps both endpoints radius-true and tangent at the pierce.
    public static Fin<Seq<Move>> LeadArc(Loop profile, LeadPolicy policy) =>
        FrameAt(profile, policy.Station).Bind(frame => {
            Point3d center = frame.Pierce + (frame.Normal * policy.Kind.Direction * policy.Radius);
            Point3d offProfile = policy.Kind.StartsOffProfile
                ? center - (frame.Tangent * policy.Radius)
                : center + (frame.Tangent * policy.Radius);
            Point3d start = policy.Kind.StartsOffProfile ? offProfile : frame.Pierce;
            Point3d end = policy.Kind.StartsOffProfile ? frame.Pierce : offProfile;
            return Seq(
                policy.Kind.StartsOffProfile
                    ? (Move)new Move.Rapid(start)
                    : new Move.Linear(start, policy.Feed),
                new Move.Circular(end, policy.Feed, new ArcCenter(center, policy.Kind.Sense)))
                .Traverse(Move.Admit)
                .As()
                .Map(static moves => moves.ToSeq());
        });

    public static Fin<Seq<Move>> AdaptiveArc(Seq<Point3d> spine, AdaptivePolicy policy) =>
        spine.Exists(static point => !point.IsValid)
            ? Fin.Fail<Seq<Move>>(GeometryFault.DegenerateInput("adaptive-arc:spine").ToError())
            : Fin.Succ(spine).Bind(admitted => {
                Arr<Point3d> nodes = admitted.ToArr();
                return nodes.Count < 2
                    ? Fin.Fail<Seq<Move>>(GeometryFault.DegenerateInput("adaptive-arc:spine").ToError())
                    : toSeq(Enumerable.Range(0, nodes.Count - 1))
                        .Bind(i => Trochoid(nodes[i], nodes[i + 1], policy))
                        .Traverse(Move.Admit)
                        .As()
                        .Map(static moves => moves.ToSeq());
            });

    public static Fin<Seq<Loop>> ArcOffset(Loop profile, double offset) =>
        !double.IsFinite(offset)
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("arc-offset:distance").ToError())
            : Admit(profile, "arc-offset").Bind(pline => {
                StaticAABB2DIndex<double> index = pline.CreateAabbIndex();
                List<Polyline<double>> result =
                    PlineOffset.ParallelOffset<Polyline<double>, double>(pline, offset, OffsetOptions(index, profile.Tolerance));
                return Loops(result, profile.Tolerance, profile.Plane);
            });

    // One Boolean entry: a 1x1 request runs the exact pairwise engine whose positive and negative output windings
    // preserve outer and hole discrimination; larger sets fold pairwise under simple-region admission through the
    // same total generated BoolKind dispatch.
    public static Fin<Seq<Loop>> ArcBoolean(Seq<Loop> subjects, Seq<Loop> clips, BoolKind operation) =>
        subjects.Count == 1 && clips.Count == 1
            ? PairBoolean(subjects.Head, clips.Head, operation)
            : operation.Switch(
                state: (Subjects: subjects, Clips: clips),
                or: static state => NormalizeUnion(state.Subjects + state.Clips),
                and: static state => Pairwise(state.Subjects, state.Clips, BoolKind.And).Bind(NormalizeUnion),
                not: static state => Difference(state.Subjects, state.Clips).Bind(NormalizeUnion),
                xor: static state => SymmetricDifference(state.Subjects, state.Clips));

    private static Fin<Seq<Loop>> PairBoolean(Loop a, Loop b, BoolKind operation) =>
        SimpleRegion(a, "arc-boolean:a").Bind(pa =>
            SimpleRegion(b, "arc-boolean:b").Bind(pb => {
                if (a.Tolerance != b.Tolerance || Math.Abs(a.Plane - b.Plane) > a.Tolerance.Absolute.Value)
                    return Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("arc-boolean:mixed-context").ToError());
                PlineBooleanOptions<double> options = new() {
                    PosEqualEps = a.Tolerance.Absolute.Value,
                    Pline1AabbIndex = pa.CreateAabbIndex(),
                };
                BooleanResult<Polyline<double>, double> result =
                    PlineBoolean.PolylineBoolean<Polyline<double>, double>(pa, pb, Native(operation), options);
                return (toSeq(result.PosPlines).Map(static row => row.Pline)
                     + toSeq(result.NegPlines).Map(static row => row.Pline))
                    .Traverse(pline => FromPline(pline, a.Tolerance, a.Plane))
                    .As()
                    .Map(static loops => loops.ToSeq());
            }));

    public static Fin<Seq<Loop>> ShapeOffset(Seq<Loop> loops, double offset) =>
        loops.IsEmpty || loops.Exists(static loop => !loop.Closed)
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("shape-offset:empty").ToError())
            : loops.Map(static loop => loop.Tolerance).Distinct().Count > 1
              || loops.Exists(loop => Math.Abs(loop.Plane - loops.Head.Plane) > loop.Tolerance.Absolute.Value)
                ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("shape-offset:mixed-context").ToError())
            : !double.IsFinite(offset)
                ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("shape-offset:distance").ToError())
                : loops.Traverse(loop => Admit(loop, "shape-offset:loop")).As().Bind(plines =>
                    ShapeLoops(
                        Shape<double>.FromPlines(plines).ParallelOffset(
                            offset,
                            new ShapeOffsetOptions<double>(
                                loops.Head.Tolerance.Absolute.Value,
                                loops.Head.Tolerance.Absolute.Value,
                                loops.Head.Tolerance.Absolute.Value)),
                        loops.Head.Tolerance, loops.Head.Plane));

    // Arc-exact containment translates the engine verdict onto the local closed vocabulary.
    public static Fin<ArcRelation> ArcContains(Loop first, Loop second) =>
        !first.Closed || !second.Closed || first.Tolerance != second.Tolerance
        || Math.Abs(first.Plane - second.Plane) > first.Tolerance.Absolute.Value
            ? Fin.Fail<ArcRelation>(GeometryFault.DegenerateInput("arc-contains:mixed-context").ToError())
            : Admit(first, "arc-contains:first").Bind(pa =>
            Admit(second, "arc-contains:second").Bind(pb =>
                PlineContains.PolylineContains(pa, pb, new PlineContainsOptions<double> {
                    PosEqualEps = first.Tolerance.Absolute.Value,
                }) switch {
                    PlineContainsResult.Pline1InsidePline2 => Fin.Succ(ArcRelation.FirstInsideSecond),
                    PlineContainsResult.Pline2InsidePline1 => Fin.Succ(ArcRelation.SecondInsideFirst),
                    PlineContainsResult.Disjoint => Fin.Succ(ArcRelation.Disjoint),
                    PlineContainsResult.Intersected => Fin.Succ(ArcRelation.Intersected),
                    _ => Fin.Fail<ArcRelation>(GeometryFault.DegenerateInput("arc-contains:invalid").ToError()),
                }));

    public static Fin<Loop> ArcClean(Loop profile) =>
        Admit(profile, "arc-clean").Bind(pline => FromPline(
            pline.RemoveRepeatPos(profile.Tolerance.Absolute.Value)
                .RemoveRedundant(profile.Tolerance.Absolute.Value),
            profile.Tolerance, profile.Plane));

    public static Fin<double> ArcLength(Loop profile) =>
        Admit(profile, "arc-length").Map(static pline => pline.PathLength());

    public static Fin<double> ArcArea(Loop profile) =>
        Region(profile, "arc-area").Map(static pline => pline.Area());

    public static Fin<Sign> ArcWinding(Loop profile) =>
        Region(profile, "arc-winding").Map(static pline => pline.Orientation() switch {
            PlineOrientation.CounterClockwise => Sign.Positive,
            PlineOrientation.Clockwise => Sign.Negative,
            _ => Sign.Zero,
        });

    public static Fin<Point3d> SampleAtLength(Loop profile, double pathLength) =>
        !double.IsFinite(pathLength) || pathLength < 0.0
            ? Fin.Fail<Point3d>(GeometryFault.DegenerateInput("arc-sample:length").ToError())
            : Admit(profile, "arc-sample").Bind(pline =>
                pline.FindPointAtPathLength(pathLength) switch {
                    (true, _, Vector2<double> point, _) => Fin.Succ(new Point3d(point.X, point.Y, profile.Plane)),
                    _ => Fin.Fail<Point3d>(GeometryFault.DegenerateInput("arc-sample:outside").ToError()),
                });

    public static Fin<DensifyReceipt> Densify(Loop profile, double errorDistance) =>
        !double.IsFinite(errorDistance) || errorDistance <= 0.0
            ? Fin.Fail<DensifyReceipt>(GeometryFault.DegenerateInput("arc-densify:error").ToError())
            : Admit(profile, "arc-densify").Bind(pline => {
                Polyline<double> dense = pline.ArcsToApproxLines(errorDistance);
                return FromPline(dense, profile.Tolerance, profile.Plane).Map(result =>
                    new DensifyReceipt(result, errorDistance, profile.Spans, result.Spans));
            });

    private static Option<(int First, int Second)> Collision(Seq<(int Source, Loop Loop)> arcs) =>
        toSeq(Enumerable.Range(0, arcs.Count))
            .Bind(first => toSeq(Enumerable.Range(first + 1, arcs.Count - first - 1))
                .Map(second => (First: first, Second: second)))
            .Find(pair => PlineContains.PolylineContains(
                    Pline(arcs[pair.First].Loop),
                    Pline(arcs[pair.Second].Loop),
                    new PlineContainsOptions<double> {
                        PosEqualEps = arcs[pair.First].Loop.Tolerance.Absolute.Value,
                    }) != PlineContainsResult.Disjoint)
            .Map(pair => (arcs[pair.First].Source, arcs[pair.Second].Source));

    private static Fin<Seq<Loop>> KerfFailure(KerfWitness witness, double kerf) =>
        Fin.Fail<Seq<Loop>>(FabricationFault.KerfCollision(witness, kerf).ToError());

    private static Fin<Polyline<double>> Admit(Loop loop, string locus) =>
        loop.Count < 2 || loop.Vertices.Exists(static point => !point.IsValid)
            ? Fin.Fail<Polyline<double>>(GeometryFault.DegenerateInput(locus).ToError())
            : Fin.Succ(Pline(loop));

    private static Fin<Polyline<double>> Region(Loop loop, string locus) =>
        !loop.Closed
            ? Fin.Fail<Polyline<double>>(GeometryFault.DegenerateInput($"{locus}:open").ToError())
            : Admit(loop, locus);

    private static Fin<Polyline<double>> SimpleRegion(Loop loop, string locus) =>
        Region(loop, locus).Bind(_ => loop.Winding() == Sign.Negative
            ? Fin.Fail<Polyline<double>>(GeometryFault.DegenerateInput($"{locus}:hole-input").ToError())
            : Fin.Succ(Pline(loop.AsCcw())));

    private static Fin<Seq<Loop>> Pairwise(Seq<Loop> left, Seq<Loop> right, BoolKind operation) =>
        left.Bind(first => right.Map(second => (First: first, Second: second)))
            .Traverse(pair => PairBoolean(pair.First, pair.Second, operation))
            .As()
            .Map(static sets => sets.Bind(static set => set));

    private static Fin<Seq<Loop>> Difference(Seq<Loop> subjects, Seq<Loop> clips) =>
        clips.Fold(
            Fin.Succ(subjects),
            (state, clip) => state.Bind(current => current
                .Traverse(subject => PairBoolean(subject, clip, BoolKind.Not))
                .As()
                .Map(static sets => sets.Bind(static set => set))));

    private static Fin<Seq<Loop>> SymmetricDifference(Seq<Loop> left, Seq<Loop> right) =>
        from leftOnly in Difference(left, right)
        from rightOnly in Difference(right, left)
        from normalized in NormalizeUnion(leftOnly + rightOnly)
        select normalized;

    private static Fin<Seq<Loop>> NormalizeUnion(Seq<Loop> loops) =>
        loops.Exists(static loop => loop.Winding() == Sign.Negative)
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("arc-boolean:set-hole").ToError())
            : loops.Count <= 1
            ? Fin.Succ(loops)
            : toSeq(Enumerable.Range(0, loops.Count))
                .Bind(first => toSeq(Enumerable.Range(first + 1, loops.Count - first - 1))
                    .Map(second => (First: first, Second: second)))
                .Traverse(pair => ArcContains(loops[pair.First], loops[pair.Second])
                    .Map(relation => (pair.First, pair.Second, Relation: relation)))
                .As()
                .Bind(relations => relations.Find(static row => row.Relation != ArcRelation.Disjoint).Match(
                    Some: row => PairBoolean(loops[row.First], loops[row.Second], BoolKind.Or).Bind(merged =>
                        NormalizeUnion(
                            Range(0, loops.Count)
                                .Filter(index => index != row.First && index != row.Second)
                                .Map(index => loops[index])
                                .ToSeq()
                                .Concat(merged))),
                    None: () => Fin.Succ(loops)));

    private static Seq<Move> Trochoid(Point3d origin, Point3d destination, AdaptivePolicy policy) {
        Vector3d tangent = destination - origin;
        double length = tangent.Length;
        if (!tangent.Unitize()) return Seq<Move>();
        Vector3d normal = new(-tangent.Y, tangent.X, 0.0);
        int stations = int.Max(1, (int)Math.Ceiling(length / policy.StepOver));
        double advance = length / stations;
        return toSeq(Enumerable.Range(1, stations)).Bind(station => {
            Point3d center = origin
                + (tangent * station * advance)
                + (normal * policy.Sense.Scale * policy.EngagementRadius);
            Point3d start = center - (tangent * policy.EngagementRadius);
            Point3d opposite = center + (tangent * policy.EngagementRadius);
            ArcCenter arc = new(center, policy.Sense.Sense);
            return Seq(
                (Move)new Move.Linear(start, policy.Feed),
                new Move.Circular(opposite, policy.Feed, arc),
                new Move.Circular(start, policy.Feed, arc));
        });
    }

    private static Fin<(Point3d Pierce, Vector3d Tangent, Vector3d Normal)> FrameAt(Loop profile, double station) =>
        from length in ArcLength(profile)
        from _ in length > profile.Tolerance.Absolute.Value && station <= length
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(GeometryFault.DegenerateInput("lead-arc:station").ToError())
        let step = Math.Max(profile.Tolerance.Absolute.Value, length * profile.Tolerance.Fractional)
        from before in SampleAtLength(profile, Math.Max(0.0, station - step))
        from at in SampleAtLength(profile, station)
        from after in SampleAtLength(profile, Math.Min(length, station + step))
        let tangent = after - before
        from admitted in tangent.Unitize()
            ? Fin.Succ((Pierce: at, Tangent: tangent, Normal: new Vector3d(-tangent.Y, tangent.X, 0.0)))
            : Fin.Fail<(Point3d, Vector3d, Vector3d)>(GeometryFault.DegenerateInput("lead-arc:tangent").ToError())
        select admitted;

    private static PlineOffsetOptions<double> OffsetOptions(StaticAABB2DIndex<double> index, Context tolerance) =>
        new() {
            AabbIndex = index,
            HandleSelfIntersects = true,
            OffsetDistEps = tolerance.Absolute.Value,
            PosEqualEps = tolerance.Absolute.Value,
            SliceJoinEps = tolerance.Absolute.Value,
        };

    // Generated Map projection: constant native verdicts, total by construction — a new BoolKind row breaks this seam loudly.
    private static BooleanOp Native(BoolKind operation) => operation.Map(
        or: BooleanOp.Or, and: BooleanOp.And, not: BooleanOp.Not, xor: BooleanOp.Xor);

    private static Polyline<double> Pline(Loop loop) {
        Polyline<double> pline = new(loop.Count, loop.Closed);
        for (int i = 0; i < loop.Count; i++)
            pline.AddVertex(PlineVertex<double>.FromVector2(new Vector2<double>(loop.At(i).X, loop.At(i).Y), loop.BulgeAt(i)));
        return pline;
    }

    private static Fin<Seq<Loop>> Loops(List<Polyline<double>> plines, Context tolerance, double plane) =>
        toSeq(plines).Traverse(pline => FromPline(pline, tolerance, plane)).As().Map(static loops => loops.ToSeq());

    private static Fin<Seq<Loop>> ShapeLoops(Shape<double> shape, Context tolerance, double plane) =>
        toSeq(shape.CcwPlines).Concat(toSeq(shape.CwPlines))
            .Traverse(row => FromPline(row.IndexedPline.Polyline, tolerance, plane))
            .As()
            .Map(static loops => loops.ToSeq());

    private static Fin<Loop> FromPline(IPlineSource<double> pline, Context tolerance, double plane) {
        List<Point3d> vertices = new(pline.VertexCount);
        List<double> bulges = new(pline.VertexCount);
        for (int i = 0; i < pline.VertexCount; i++) {
            PlineVertex<double> vertex = pline.Get(i);
            vertices.Add(new Point3d(vertex.X, vertex.Y, plane));
            bulges.Add(vertex.Bulge);
        }
        return Loop.Admit(toArr(vertices), pline.IsClosed, toArr(bulges), tolerance);
    }
}
```
