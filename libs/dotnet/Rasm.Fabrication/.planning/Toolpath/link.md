# [RASM_FABRICATION_LINK]

`Link.Route` admits cut occurrences, keepout volumes, sequencing constraints, machine clearance, and a weighted objective before selecting orientation, order, and transition posture, then refines the closed tour under the same precedence it was built against. `Linked` preserves every cutting and transition move, and `LinkTour` explains the winning full route beside the solver evidence that produced it.

`Link.Route`, `Linked`, and `LinkTour` are the linking contract. `Linked.SpecializedDirective` projects transition metrics into the admitted `SpecializedToolpathEnvelope`, while cutting directives remain on source segments. `ArcAlgebra.Apply` owns planar clearance inflation, `QuikGraph` owns precedence in-degrees, reachability, and routed transitions, and supplied `Guard` verifies collision admission without importing host state. Tool and work-offset identity remain objective terms.

## [01]-[INDEX]

- [02]-[ADMISSION]: `LinkDemand` materializes delegates once; generated owners admit coupled elements, keepouts, precedence, policy, and objective; `CutSignature` declares the shared element-key columns and `LinkStationKey` closes what a station key can name.
- [03]-[ROUTING]: `Link.Route` jointly selects a precedence-safe tour and occurrence orientation against realized transition costs, refines it by precedence-safe exchange, and lowers each transition through one result.
- [04]-[EGRESS]: `Linked` projects through a caller-supplied arrow while retaining route, objective, reachability, solver, and guard evidence.

## [02]-[ADMISSION]

`LinkJob` owns every fact that changes ordering or transition safety. Geometry and vertical extent are one `Keepout`; tool, work offset, thermal load, and admissible orientations are one `CutElement`; neither admits a parallel ordinal collection.

- Owner: `CutSignature` is the ONE declaration of the columns every element key shares — strategy, tool, work offset, all four cutter dimensions, and the emitted moves — so a widened cutter or a renamed offset changes one record instead of being transcribed onto each identity case. `CutElementIdentity` cases then carry their DISCRIMINANTS alone.
- Owner: `CutElement.Identify` is the package's ONE element-key mint — it admits every move, digests the discriminating motion, surface, or skeleton preimage beside the shared signature through the `Process/owner#RUN_DISPATCH` `FabricationCanon.Ordered` close, so a generator minting its own key is the deleted form and two arcs differing only in sweep never hash equal. `ElementVariant.Of` is the ONE derivation of entry, exit, rotation, thermal exposure, and pierces off emitted motion; `LinkStation` binds the chosen variant to `ToolKey` and `WorkOffset`, and `LinkStation.Park` gives home and return legs the same machine identity.
- Owner: `LinkStationKey` closes what a station key can NAME — a cut element's own digest or one `ParkLeg` row — so home legs stop riding literal words in the slot a content hash also inhabits, and a consumer reading an order or a transition endpoint discriminates on the value instead of comparing against a spelling. `ParkLeg` carries those legs as rows, so every site that spelled a reserved word reads one declaration.
- Owner: `Keepout` couples the stable obstacle key, each inflated region and its admission-built `IndexedPolyline<double>` flatbush index, and payload-bearing `KeepoutExtent`; `ArcOffset` is the ONE arc-offset ingress the specialized lanes share, so a trace shape that is not an offset family refuses in one place rather than in a ladder per caller.
- Owner: `LinkPolicy` carries three DIMENSIONED sub-owners and the solver's own declared bounds. `LinkClearance` holds the five lengths one job routes under, `LinkRate` the two traverse speeds, and `LinkChangeover` the two identity-change durations — each as the `UnitsNet` quantity it is, so no column spells its unit in its identifier and a millimetre can never be handed where a second was wanted; beam width and refinement budget stay bare ordinals because a search bound has no dimension, and each is a policy value a caller reads and tunes rather than a square-root heuristic buried in the expansion.
- Owner: each dimensioned sub-owner declares its OWN `QuantityArrow` and its own `Admit` arm, so all three axes a caller can state as text cross the one boundary — the arrow answers the canonical magnitude and the owner re-quantifies it once, where a lone length arrow no member called left the speed and duration columns with no text lane at all.
- Owner: generated `LinkObjective` weights admit distance, time, lift, thermal, rotation, retract, pierce, tool change, and setup change; named objectives are seed data over one metric generator, and every weight has a producer on both metric legs — the station supplies cutting thermal, rotation, and pierce counts while the transition derives thermal from `RetractKind.ThermalCoupling` over its horizontal span and rotation from the emitted path's exterior turn angles, so no weight multiplies a constant.
- Packages: `Process/owner#RUN_DISPATCH` `QuantityArrow` is the ONE dimension-text entry a caller reaches before admission and `FabricationCanon.Ordered` the ONE digest close the element mint rides; `UnitsNet` seats `Length`, `Speed`, and `Duration` on the policy head; `TensorPrimitives.IsFiniteAll` admits numeric batches; `AdmissionSlots.Gate` accumulates the policy's own axes; `Thinktecture` closes construction.
- Boundary: `LinkDemand` crosses the nullable boundary exactly once, and every interior function consumes `LinkJob`. The guard slot is a VERIFICATION — it returns `Fin<Unit>` and cannot rewrite the moves it inspects, so a rewriting guard is a compile rejection rather than a runtime equality check.

## [03]-[ROUTING]

`Link.Route` chooses among direct, ramped, skim, lifted, visibility-routed, and controlled-descent transitions. Each posture is a case over one geometric transition; no nullable move family or sentinel cost crosses the boundary.

- Exemption: corridor construction, index-pruned arc intersection, and graph search are measured kernel statement boundaries.
- Entry: `Route<TOut>` parameterizes raw ingress, transition lowering, collision guard, and egress projection.
- Auto: precedence rides ONE `BidirectionalGraph` built at tour selection — `InDegree` seeds the pending map in one probe per element and `OutEdges` decrements it per placement, so neither reads a linear scan of the constraint list.
- Auto: one precedence-aware beam state owns each chosen occurrence, variant, identity-change charge, realized obstacle route, and return-home score; `BeamState` carries pending in-degrees and the placed element ordinals, so frontier readiness is one map probe per candidate and the refinement can re-check precedence on the tour it produced.
- Auto: each candidate transition lowers, admits, and guards before entering the beam; `BeamState` retains accepted legs, so final connection cannot discover a transition failure. A refused variant carries its cause — an empty frontier raises every accumulated refusal rather than a bare stall, and refused variants and beam-width prunes are counted apart.
- Auto: the tour closes — a park leg precedes the first station and follows the last, so return travel is priced and guarded like any transition.
- Auto: refinement exchanges a station PAIR, never reverses a segment: an exchange re-routes at most four legs and its improvement is measured on the routes the swap actually produced, where a reversal would assume a corridor symmetry a directed route does not hold. A pair exchanges only when no precedence edge leaving the earlier element lands inside the moved span and none entering the later element originates in it, so every in-degree that was satisfied stays satisfied.
- Auto: direct three-dimensional travel remains direct when the corridor and the ramp's swept envelope are safe; differing endpoint heights never force a skim.
- Auto: clearance planes exceed endpoints and bounded obstacle tops, so only UNBOUNDED keepouts reach the plane — corner-to-corner visibility is therefore a property of the job, built once as `LinkCorridor` and joined per transition by its own two lifted endpoints alone, where a per-candidate rebuild re-solved the same quadratic for every pair the beam considered.
- Packages: `ArcAlgebra.Apply` inflates arc-native keepouts; `StaticAABB2DIndex<double>.VisitQueryWithStack` prunes corridor tests inside the descent over one pooled traversal stack; `PlineSegIntersection.Intersect` returns the SIX-way verdict a corridor test reads whole — a crossing blocks, while a tangent touch and a collinear or arc overlap are the wall contact an inflated keepout's own boundary produces and never a crossing; `PlineSeg.SegMidpoint` places arc apexes; QuikGraph owns the DAG gate, in-degrees, components, and weighted recovery; LanguageExt keeps failures flat.
- Boundary: `FabricationFault.LinkBlocked` names the stalled cursor and first withheld element; `Guard` verifies each segment without the ability to rewrite it, or refuses it without swallowed failure.

## [04]-[EGRESS]

`Linked` is inverse-sufficient: `LinkSegment.Cutting` and `Transiting` keep cut and travel moves distinct, `Linked.Moves` projects guarded motion without erasing that split, and `LinkTour` preserves order, solver evidence, component evidence, guarded-move count, and full-route metrics.

- Law: `LinkLeg` holds the transition WHOLE — its endpoints, posture, corridor, emitted moves, metric, and score in one row — so the routing fold, the beam state, and the transiting segment stop each carrying a moves-and-evidence pair that never travels apart, keeping one correspondence in three places that each drift from the others.
- Law: the `SpecializedToolpathEnvelope` is admitted ONCE at tour construction through the S0 factory, so `Linked` holds a proven payload and no consumer re-walks its rows; `ToolpathRowMap` owns the leg-to-row transcription, so the twelve columns generate from the leg's own members and the station keys lower to their wire spelling through the mapper's one key projection.
- Law: motion supplies transition lowering; its commit fold conditions and guards the linked program once. Posting and simulation retain typed transition metrics, and estimation consumes the realized motion clock without double-counting tour duration.
- Output: `Linked.PostingSource` carries transition evidence into canonical posting; the caller arrow retains other result projections.
- Output: `LinkLeg` records transition endpoints as typed station keys, posture, its S0 `LinkTransition` classification, the corridor it rode, the moves it emitted, and distance, time, lift, retract, tool-change, and setup-change terms beside its objective score; `LinkSolver` records the declared beam width, refused variants, pruned states, accepted exchanges, the improvement they bought, and the runner-up tour's score — the margin the winner actually beat, which is measured evidence where an estimated bound would not be; `LinkTour.Total` adds cutting distance, time, thermal exposure, rotation, and pierces exactly once.
- Growth: a new machine posture is one `RetractKind` policy value carrying its S0 classification; a new cost regime is one admitted `LinkObjective`; a new obstacle occurrence is one `Keepout` admission; a new move classification is one `LinkSegment` case.
- Boundary: no route publishes `double.PositiveInfinity`, a disconnected partial tour, an open tour that never returns home, or unguarded moves.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Numerics.Tensors;
using CavalierContours.Core;
using CavalierContours.Polyline;
using CavalierContours.Shape;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ParkLeg {
    public static readonly ParkLeg Origin = new("origin");
    public static readonly ParkLeg Return = new("return");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinkStationKey {
    private LinkStationKey() { }

    public sealed record Element(string Digest) : LinkStationKey;
    public sealed record Park(ParkLeg Leg) : LinkStationKey;

    public string Key => Switch(
        element: static row => row.Digest,
        park: static row => row.Leg.Key);
}

public readonly record struct LinkMetric(
    double DistanceMm,
    double DurationSeconds,
    double LiftMm,
    double ThermalExposure,
    double RotationPenalty,
    int Retracts,
    int Pierces,
    int ToolChanges,
    int SetupChanges) {
    public static LinkMetric operator +(LinkMetric left, LinkMetric right) => new(
        left.DistanceMm + right.DistanceMm,
        left.DurationSeconds + right.DurationSeconds,
        left.LiftMm + right.LiftMm,
        left.ThermalExposure + right.ThermalExposure,
        left.RotationPenalty + right.RotationPenalty,
        left.Retracts + right.Retracts,
        left.Pierces + right.Pierces,
        left.ToolChanges + right.ToolChanges,
        left.SetupChanges + right.SetupChanges);
}

[ComplexValueObject]
public sealed partial class LinkObjective {
    public static readonly LinkObjective Distance = Create(1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
    public static readonly LinkObjective CycleTime = Create(0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 25.0, 90.0);
    public static readonly LinkObjective Surface = Create(0.15, 0.1, 0.4, 1.0, 0.3, 0.5, 1.0, 8.0, 30.0);
    public static readonly LinkObjective Balanced = Create(0.35, 0.5, 0.25, 0.65, 0.2, 0.4, 0.75, 20.0, 75.0);

    public double DistanceWeight { get; }
    public double TimeWeight { get; }
    public double LiftWeight { get; }
    public double ThermalWeight { get; }
    public double RotationWeight { get; }
    public double RetractWeight { get; }
    public double PierceWeight { get; }
    public double ToolChangeWeight { get; }
    public double SetupChangeWeight { get; }

    public double Score(LinkMetric metric) =>
        metric.DistanceMm * DistanceWeight
        + metric.DurationSeconds * TimeWeight
        + metric.LiftMm * LiftWeight
        + metric.ThermalExposure * ThermalWeight
        + metric.RotationPenalty * RotationWeight
        + metric.Retracts * RetractWeight
        + metric.Pierces * PierceWeight
        + metric.ToolChanges * ToolChangeWeight
        + metric.SetupChanges * SetupChangeWeight;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double distanceWeight,
        ref double timeWeight,
        ref double liftWeight,
        ref double thermalWeight,
        ref double rotationWeight,
        ref double retractWeight,
        ref double pierceWeight,
        ref double toolChangeWeight,
        ref double setupChangeWeight) {
        ReadOnlySpan<double> weights = [distanceWeight, timeWeight, liftWeight, thermalWeight, rotationWeight,
            retractWeight, pierceWeight, toolChangeWeight, setupChangeWeight];
        if (!TensorPrimitives.IsFiniteAll<double>(weights)
            || TensorPrimitives.Min<double>(weights) < 0.0
            || TensorPrimitives.Sum<double>(weights) <= 0.0)
            validationError = new ValidationError("link:objective");
    }
}

[SmartEnum<string>]
public sealed partial class RetractKind {
    public static readonly RetractKind Direct = new(
        "direct", retracts: 0, requiresPlane: false, thermalCoupling: 1.0, LinkTransition.Direct);
    public static readonly RetractKind Ramp = new(
        "ramp", retracts: 0, requiresPlane: false, thermalCoupling: 1.0, LinkTransition.Direct);
    public static readonly RetractKind Skim = new(
        "skim", retracts: 1, requiresPlane: true, thermalCoupling: 0.5, LinkTransition.Retract);
    public static readonly RetractKind FullLift = new(
        "full-lift", retracts: 1, requiresPlane: true, thermalCoupling: 0.0, LinkTransition.Retract);
    public static readonly RetractKind Routed = new(
        "routed", retracts: 1, requiresPlane: true, thermalCoupling: 0.0, LinkTransition.Clearance);
    public static readonly RetractKind ControlledDescent = new(
        "controlled-descent", retracts: 1, requiresPlane: true, thermalCoupling: 0.25, LinkTransition.Retract);

    public int Retracts { get; }
    public bool RequiresPlane { get; }

    public double ThermalCoupling { get; }

    public LinkTransition Transition { get; }
}

// --- [ADMISSION] -----------------------------------------------------------------------
public sealed record ElementVariant(
    string Key,
    Point3d Entry,
    Point3d Exit,
    Seq<Move> Moves,
    double RotationPenalty,
    double ThermalExposure,
    int Pierces,
    Seq<MotionDirective> Directives = default) {
    public static ElementVariant Of(
        string key,
        Seq<Move> moves,
        ProcessModality modality,
        Seq<MotionDirective> directives = default) =>
        moves.Fold(ElementWalk.Empty, static (walk, move) => walk.Advanced(move)).Apply(walked => new ElementVariant(moves.Head.Map(static move => move.Target).IfNone(Point3d.Origin),
            walked.Cursor.IfNone(Point3d.Origin),
            moves,
            RotationPenalty: walked.Turning,
            ThermalExposure: modality.ThermalCoupling * walked.Engaged,
            Pierces: walked.Pierces,
            Directives: directives));
}

public readonly record struct ElementWalk(
    Option<Point3d> Cursor,
    Option<Vector3d> Heading,
    double Turning,
    double Engaged,
    int Pierces,
    bool WasRapid) {
    public static readonly ElementWalk Empty =
        new(Option<Point3d>.None, Option<Vector3d>.None, 0.0, 0.0, 0, WasRapid: true);

    public ElementWalk Advanced(Move move) => Cursor.Match(
        Some: cursor => Stepped(cursor, move),
        None: () => this with { Cursor = Some(move.Target), WasRapid = move is Move.Rapid });

    private ElementWalk Stepped(Point3d cursor, Move move) =>
        (move.Target - cursor) is var span && span.IsTiny()
            ? this with { Cursor = Some(move.Target), WasRapid = move is Move.Rapid }
            : new ElementWalk(
                Some(move.Target),
                Some(span),
                Turning + Heading.Map(prior => Vector3d.VectorAngle(prior, span)).IfNone(0.0),
                Engaged + Engagement(cursor, move),
                Pierces + (WasRapid && move is not Move.Rapid ? 1 : 0),
                move is Move.Rapid);

    private static double Engagement(Point3d from, Move move) => move.Switch(
        state: from,
        rapid: static (_, _) => 0.0,
        linear: static (start, row) => start.DistanceTo(row.Target) / row.Feed,
        circular: static (start, row) => Link.Swept(start, row) / row.Feed);
}

[Union]
public abstract partial record EntryFamily {
    public sealed record Fixed(ElementVariant Variant) : EntryFamily;
    public sealed record Reversible(ElementVariant Forward, ElementVariant Reverse) : EntryFamily;
    public sealed record Cyclic(Loop Boundary, int Samples, Func<Point3d, Fin<ElementVariant>> AtPoint) : EntryFamily;
}

public readonly record struct CutSignature(
    CutStrategy Strategy,
    string ToolKey,
    string WorkOffset,
    string CutterFamily,
    double CutterDiameter,
    double CutterCornerRadius,
    double CutterTaperAngle,
    double CutterFluteLength,
    Seq<Move> Moves) {
    public static CutSignature Of(
        CutStrategy strategy, string toolKey, string workOffset, CutterForm cutter, Seq<Move> moves) =>
        new(strategy, toolKey, workOffset, cutter.Family.Key, cutter.Diameter, cutter.CornerRadius,
            cutter.TaperAngle, cutter.FluteLength, moves);

    public bool Complete =>
        Strategy is not null
        && Witness.Keyed(ToolKey) && Witness.Keyed(WorkOffset) && Witness.Keyed(CutterFamily)
        && TensorPrimitives.IsFiniteAll<double>(
            [CutterDiameter, CutterCornerRadius, CutterTaperAngle, CutterFluteLength])
        && CutterDiameter > 0.0
        && CutterCornerRadius >= 0.0
        && CutterTaperAngle >= 0.0
        && CutterFluteLength > 0.0
        && !Moves.IsEmpty;

    public CanonicalWriter Write(CanonicalWriter writer) {
        writer.String(ToolKey)
            .String(WorkOffset)
            .String(CutterFamily)
            .Double(CutterDiameter)
            .Double(CutterCornerRadius)
            .Double(CutterTaperAngle)
            .Double(CutterFluteLength)
            .Ordinal(Moves.Count);
        return writer;
    }
}

[Union]
public abstract partial record CutElementIdentity(CutSignature Signature) {
    public sealed record Motion(int Occurrence, CutSignature Signature) : CutElementIdentity(Signature);
    public sealed record Surface(int View, int Path, string Operation, CutSignature Signature)
        : CutElementIdentity(Signature);
    public sealed record Skeleton(int Component, Seq<int> OriginEdges, int Path, CutSignature Signature)
        : CutElementIdentity(Signature);
}

[ComplexValueObject]
public sealed partial class CutElement {
    public string Key { get; }
    public string ToolKey { get; }
    public string WorkOffset { get; }
    public EntryFamily Entry { get; }
    public Arr<ElementVariant> Variants { get; }

    public static Fin<string> Identify(CutElementIdentity identity) =>
        from _ in identity.Signature.Complete && Discriminated(identity)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("link", "link:element-identity"))
        from admitted in identity.Signature.Moves.TraverseM(Move.Admit).As()
        select Digest(identity, admitted);

    public static Fin<CutElement> Admit(string key, string toolKey, string workOffset, EntryFamily entry) =>
        from variants in Variants(entry)
        from checkedVariants in variants.TraverseM(AdmitVariant).As()
        from admitted in Validate(key, toolKey, workOffset, entry, checkedVariants.ToArr(), out CutElement element)
            .Admitted(element)
        select admitted;

    private static Fin<ElementVariant> AdmitVariant(ElementVariant variant) =>
        variant.Moves.TraverseM(Move.Admit).As().Map(moves => variant with { Moves = moves });

    private static bool Discriminated(CutElementIdentity identity) => identity.Switch(
        motion: static row => ValidityClaim.Nonnegative(row.Occurrence),
        surface: static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.View), ValidityClaim.Nonnegative(row.Path), Witness.Keyed(row.Operation)),
        skeleton: static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Component), ValidityClaim.Nonnegative(row.Path), !row.OriginEdges.IsEmpty,
            row.OriginEdges.ForAll(static value => ValidityClaim.Nonnegative(value).Holds)));

    private static string Digest(CutElementIdentity identity, Seq<Move> moves) =>
        FabricationCanon
            .Ordered(0.0, preimage => moves.Fold(identity.Signature.Write(Discriminate(preimage, identity)), Write))
            .ToString("x32", CultureInfo.InvariantCulture);

    private static CanonicalWriter Discriminate(CanonicalWriter writer, CutElementIdentity identity) => identity.Switch(
        state: writer,
        motion: static (preimage, row) => preimage
            .String("motion")
            .Ordinal(row.Occurrence)
            .String(row.Signature.Strategy.Key),
        surface: static (preimage, row) => preimage
            .String("surface")
            .Ordinal(row.View)
            .Ordinal(row.Path)
            .String(row.Signature.Strategy.Key)
            .String(row.Operation),
        skeleton: static (preimage, row) => row.OriginEdges.Fold(
            preimage
                .String("skeleton")
                .Ordinal(row.Component)
                .Ordinal(row.Path)
                .String(row.Signature.Strategy.Key)
                .Ordinal(row.OriginEdges.Count),
            static (writer, edge) => writer.Ordinal(edge)));

    private static CanonicalWriter Write(CanonicalWriter writer, Move move) => move.Switch(
        rapid: static row => (Kind: 0, row.Target, Feed: 0.0, Center: Point3d.Origin, Sense: 0.0, Sweep: 0.0),
        linear: static row => (Kind: 1, row.Target, row.Feed, Center: Point3d.Origin, Sense: 0.0, Sweep: 0.0),
        circular: static row => (Kind: 2, row.Target, row.Feed, row.Arc.Center,
            Sense: row.Arc.Sense == RotationSense.Clockwise ? -1.0 : 1.0, Sweep: row.SweepRadians))
        .Apply(row => writer
            .Ordinal(row.Kind)
            .Coords(row.Target)
            .Double(row.Feed)
            .Coords(row.Center)
            .Double(row.Sense)
            .Double(row.Sweep));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref string toolKey,
        ref string workOffset,
        ref EntryFamily entry,
        ref Arr<ElementVariant> variants) {
        bool valid = !variants.IsEmpty && variants.ForAll(static row =>
            Witness.Keyed(row.Key) && row.Entry.IsValid && row.Exit.IsValid && !row.Moves.IsEmpty
            && TensorPrimitives.IsFiniteAll<double>([row.RotationPenalty, row.ThermalExposure])
            && row.RotationPenalty >= 0.0 && row.ThermalExposure >= 0.0 && row.Pierces >= 0);
        if (!(Witness.Keyed(key) && Witness.Keyed(toolKey) && Witness.Keyed(workOffset) && valid))
            validationError = new ValidationError("link:element");
    }

    private static Fin<Arr<ElementVariant>> Variants(EntryFamily entry) => entry.Switch(
        @fixed: static row => Fin.Succ(Arr(row.Variant)),
        reversible: static row => Fin.Succ(Arr(row.Forward, row.Reverse)),
        cyclic: Cyclic);

    private static Fin<Arr<ElementVariant>> Cyclic(EntryFamily.Cyclic row) {
        if (!row.Boundary.Closed || row.Samples < 2)
            return Fin.Fail<Arr<ElementVariant>>(new KernelFault.InvalidValue("link", "link:cyclic-entry"));
        Polyline<double> path = new(
            toSeq(row.Boundary.Vertices).Map((point, index) => PlineVertex<double>.FromSlice(
                [point.X, point.Y, row.Boundary.BulgeAt(index)])),
            true);
        double length = path.PathLength();
        return Range(0, row.Samples).ToSeq().TraverseM(index =>
            path.FindPointAtPathLength(index * length / row.Samples) switch {
                (true, _, Vector2<double> point, _) => LinkJob.Invoke(
                    () => row.AtPoint(new Point3d(point.X, point.Y, row.Boundary.Plane))),
                _ => Fin.Fail<ElementVariant>(new GeometryFault.DegenerateInput(Kind.Curve, None, "link:cyclic-station")),
            }).As().Map(static rows => rows.ToArr());
    }
}

internal static class ArcOffset {
    public static Fin<Seq<Loop>> Family(Loop source, double distance, string locus) =>
        ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(source), distance)).Bind(trace => trace.Switch(
            state: locus,
            forest: static (_, row) => Fin.Succ(row.Result.Loops),
            paths: static (_, row) => Fin.Succ(row.Result),
            motion: static (slot, _) => Refused(slot),
            inspection: static (slot, _) => Refused(slot),
            densified: static (slot, _) => Refused(slot),
            recovered: static (slot, _) => Refused(slot)));

    public static Fin<Loop> Single(Loop source, double distance, string locus) =>
        Family(source, distance, locus).Bind(rows => rows.Count == 1
            ? Fin.Succ(rows[0])
            : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, rows.Count, $"{locus}:topology")));

    private static Fin<Seq<Loop>> Refused(string locus) =>
        Fin.Fail<Seq<Loop>>(new KernelFault.InvalidValue("link", locus));
}

[Union]
public abstract partial record KeepoutExtent {
    public sealed record Bounded(double FloorZ, double TopZ) : KeepoutExtent;
    public sealed record Unbounded : KeepoutExtent;

    public bool Active(double fromZ, double toZ) => Switch(
        bounded: row => Math.Max(fromZ, toZ) >= row.FloorZ && Math.Min(fromZ, toZ) <= row.TopZ,
        unbounded: static _ => true);

    public Option<double> Top => Switch(
        bounded: static row => Some(row.TopZ),
        unbounded: static _ => Option<double>.None);

    public bool IsValid => Switch(
        bounded: static row => ValidityClaim.All(ValidityClaim.Finite([row.FloorZ, row.TopZ]), row.TopZ >= row.FloorZ),
        unbounded: static _ => true);
}

[ComplexValueObject]
public sealed partial class Keepout {
    public string Key { get; }
    public Arr<(Loop Boundary, IndexedPolyline<double> Index)> Geometry { get; }
    public KeepoutExtent Extent { get; }
    public double MarginMm { get; }

    public bool Active(double fromZ, double toZ) => Extent.Active(fromZ, toZ);

    public static Fin<Keepout> Admit(string key, Loop footprint, KeepoutExtent extent, double marginMm) =>
        from regions in ArcOffset.Family(footprint, marginMm, "link:keepout-offset")
        let geometry = regions.Map(static loop => (Boundary: loop, Index: Index(loop))).ToArr()
        from admitted in Validate(key, geometry, extent, marginMm, out Keepout keepout).Admitted(keepout)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref Arr<(Loop Boundary, IndexedPolyline<double> Index)> geometry,
        ref KeepoutExtent extent,
        ref double marginMm) {
        if (!(Witness.Keyed(key) && !geometry.IsEmpty
            && geometry.ForAll(static row => row.Boundary.Closed && row.Boundary.Count >= 3)
            && extent.IsValid && double.IsFinite(marginMm) && marginMm >= 0.0))
            validationError = new ValidationError("keepout");
    }

    private static IndexedPolyline<double> Index(Loop loop) {
        Polyline<double> path = new(
            toSeq(loop.Vertices).Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])),
            loop.Closed);
        return new IndexedPolyline<double>(path) { SpatialIndex = path.CreateAabbIndex() };
    }
}

public readonly record struct OrderConstraint(int Before, int After);

public readonly record struct LinkClearance(Length Plane, Length Skim, Length RampRise, Length Corner, Length Slack) {
    private static readonly QuantityArrow Text =
        new(PhysicsQuantity.Length, FabConcern.Toolpath, "link:clearance");

    public static Fin<LinkClearance> Admit(
        string plane, string skim, string rampRise, string corner, string slack) =>
        Text.Admit(Seq(plane, skim, rampRise, corner, slack)).Map(static rows => new LinkClearance(
            Length.FromMillimeters(rows[0]),
            Length.FromMillimeters(rows[1]),
            Length.FromMillimeters(rows[2]),
            Length.FromMillimeters(rows[3]),
            Length.FromMillimeters(rows[4])));

    public bool Sound =>
        TensorPrimitives.IsFiniteAll<double>([(double)Plane.Millimeters, (double)Skim.Millimeters,
            (double)RampRise.Millimeters, (double)Corner.Millimeters, (double)Slack.Millimeters])
        && (double)Plane.Millimeters > 0.0
        && (double)Skim.Millimeters >= 0.0
        && (double)RampRise.Millimeters >= 0.0
        && (double)Corner.Millimeters > 0.0
        && (double)Slack.Millimeters > 0.0;
}

public readonly record struct LinkRate(Speed Rapid, Speed Plunge) {
    private static readonly QuantityArrow Text = new(PhysicsQuantity.Feed, FabConcern.Toolpath, "link:rate");

    public static Fin<LinkRate> Admit(string rapid, string plunge) =>
        Text.Admit(Seq(rapid, plunge)).Map(static rows => new LinkRate(
            Speed.FromMillimetersPerMinutes(rows[0]), Speed.FromMillimetersPerMinutes(rows[1])));

    public bool Sound =>
        TensorPrimitives.IsFiniteAll<double>(
            [(double)Rapid.MillimetersPerMinutes, (double)Plunge.MillimetersPerMinutes])
        && (double)Rapid.MillimetersPerMinutes > 0.0
        && (double)Plunge.MillimetersPerMinutes > 0.0;
}

public readonly record struct LinkChangeover(UnitsNet.Duration Tool, UnitsNet.Duration Setup) {
    private static readonly QuantityArrow Text =
        new(PhysicsQuantity.Duration, FabConcern.Toolpath, "link:changeover");

    public static Fin<LinkChangeover> Admit(string tool, string setup) =>
        Text.Admit(Seq(tool, setup)).Map(static rows => new LinkChangeover(
            UnitsNet.Duration.FromSeconds(rows[0]), UnitsNet.Duration.FromSeconds(rows[1])));

    public bool Sound =>
        TensorPrimitives.IsFiniteAll<double>([(double)Tool.Seconds, (double)Setup.Seconds])
        && (double)Tool.Seconds >= 0.0
        && (double)Setup.Seconds >= 0.0;
}

[ComplexValueObject]
public sealed partial class LinkPolicy {
    public LinkClearance Clearance { get; }
    public LinkRate Rate { get; }
    public LinkChangeover Changeover { get; }

    public int BeamWidth { get; }
    public int RefinementPairs { get; }

    public static Fin<LinkPolicy> Admit(
        LinkClearance clearance,
        LinkRate rate,
        LinkChangeover changeover,
        int beamWidth,
        int refinementPairs) =>
        Validate(clearance, rate, changeover, beamWidth, refinementPairs, out LinkPolicy policy).Admitted(policy);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref LinkClearance clearance,
        ref LinkRate rate,
        ref LinkChangeover changeover,
        ref int beamWidth,
        ref int refinementPairs) =>
        validationError = (
            AdmissionSlots.Gate(clearance.Sound, FabConcern.Toolpath, "link-policy:clearance", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(rate.Sound, FabConcern.Toolpath, "link-policy:rate", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(changeover.Sound, FabConcern.Toolpath, "link-policy:changeover", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(beamWidth >= 1 && refinementPairs >= 0, FabConcern.Toolpath, "link-policy:search", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .Match<ValidationError?>(
                Fail: static _ => new ValidationError("link-policy"),
                Succ: static _ => null);

}

public sealed record LinkDemand(
    Point3d Start,
    Arr<CutElement> Elements,
    Arr<Keepout> Keepouts,
    Arr<OrderConstraint> Precedence,
    LinkPolicy Policy,
    LinkObjective Objective,
    Func<Seq<Point3d>, RetractKind, Fin<Seq<Move>>> Lower,
    Func<Seq<Move>, Fin<Unit>> Guard);

[ComplexValueObject]
public sealed partial class LinkJob {
    public Point3d Start { get; }
    public Arr<CutElement> Elements { get; }
    public Arr<Keepout> Keepouts { get; }
    public Arr<OrderConstraint> Precedence { get; }
    public LinkPolicy Policy { get; }
    public LinkObjective Objective { get; }
    public Func<Seq<Point3d>, RetractKind, Fin<Seq<Move>>> Lower { get; }

    public Func<Seq<Move>, Fin<Unit>> Guard { get; }

    public static Fin<LinkJob> Admit(LinkDemand? candidate) =>
        from raw in Optional(candidate).ToFin(FabricationFault.Inadmissible(FabConcern.Toolpath, "link:demand"))
        from admitted in Validate(raw.Start, raw.Elements, raw.Keepouts, raw.Precedence, raw.Policy,
            raw.Objective, raw.Lower, raw.Guard, out LinkJob job).Admitted(job)
        select admitted;

    internal static Fin<T> Invoke<T>(Func<Fin<T>> callback) =>
        Try.lift(callback).Run().Bind(static inner => inner);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Point3d start,
        ref Arr<CutElement> elements,
        ref Arr<Keepout> keepouts,
        ref Arr<OrderConstraint> precedence,
        ref LinkPolicy policy,
        ref LinkObjective objective,
        ref Func<Seq<Point3d>, RetractKind, Fin<Seq<Move>>> lower,
        ref Func<Seq<Move>, Fin<Unit>> guard) {
        int count = elements.Count;
        bool edges = precedence.ForAll(edge => Witness.Pair(edge.Before, edge.After)
            && edge.Before < count && edge.After < count);
        bool unique = elements.Map(static row => row.Key).Distinct().Count == count
            && keepouts.Map(static row => row.Key).Distinct().Count == keepouts.Count;
        if (!(start.IsValid && !elements.IsEmpty && lower is not null && guard is not null
            && unique && edges && Acyclic(precedence)))
            validationError = new ValidationError("link:job");
    }

    private static bool Acyclic(Arr<OrderConstraint> precedence) =>
        precedence.Map(static row => new SEdge<int>(row.Before, row.After))
            .IsDirectedAcyclicGraph<int, SEdge<int>>();
}

// --- [EVIDENCE] ------------------------------------------------------------------------
public sealed record LinkLeg(
    LinkStationKey From,
    LinkStationKey To,
    RetractKind Kind,
    Seq<Point3d> Path,
    Seq<Move> Moves,
    LinkMetric Metric,
    double ObjectiveScore,
    int VisibilityComponents) {
    public LinkTransition Transition => Kind.Transition;
}

public sealed record LinkStation(
    LinkStationKey Key,
    string ToolKey,
    string WorkOffset,
    Point3d Entry,
    Point3d Exit,
    Seq<Move> Moves,
    Seq<MotionDirective> Directives,
    double RotationPenalty,
    double ThermalExposure,
    int Pierces) {
    public static LinkStation Of(CutElement element, ElementVariant variant) => new(
        new LinkStationKey.Element(variant.Key), element.ToolKey, element.WorkOffset, variant.Entry, variant.Exit,
        variant.Moves, variant.Directives, variant.RotationPenalty, variant.ThermalExposure, variant.Pierces);

    public static LinkStation Park(ParkLeg leg, Point3d point, LinkStation neighbour) => new(
        new LinkStationKey.Park(leg), neighbour.ToolKey, neighbour.WorkOffset, point, point,
        Seq<Move>(), Seq<MotionDirective>(), 0.0, 0.0, 0);
}

[Union]
public abstract partial record LinkSegment {
    public sealed record Cutting(LinkStationKey Key, Seq<Move> Moves, Seq<MotionDirective> Directives) : LinkSegment;
    public sealed record Transiting(LinkLeg Leg) : LinkSegment;

    public Seq<Move> Emitted => Switch(
        cutting: static row => row.Moves,
        transiting: static row => row.Leg.Moves);
}

public readonly record struct LinkSolver(
    int BeamWidth,
    int RejectedVariants,
    int PrunedStates,
    int Exchanges,
    double ImprovementDelta,
    Option<double> RunnerUpScore);

public sealed record LinkTour(
    Arr<LinkStationKey> Order,
    Seq<LinkLeg> Legs,
    LinkMetric Total,
    double ObjectiveScore,
    LinkSolver Solver,
    int GuardedMoves);

public sealed record Linked(
    Seq<LinkSegment> Segments,
    LinkTour Tour,
    SpecializedToolpathEnvelope Specialized) {
    public Seq<Move> Moves => Segments.Bind(static segment => segment.Emitted);
    public Seq<MotionDirective> Directives => Segments.Bind(static segment => segment is LinkSegment.Cutting cutting
        ? cutting.Directives
        : Seq<MotionDirective>());

    public MotionDirective SpecializedDirective => new MotionDirective.Specialized(
        Moves.IsEmpty ? -1 : Moves.Count - 1, Specialized);
    public PostSource PostingSource => new PostSource.Specialized(Specialized);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class ToolpathRowMap {
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.DistanceMm)], [nameof(SpecializedToolpathRow.Link.DistanceMm)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.DurationSeconds)], [nameof(SpecializedToolpathRow.Link.DurationSeconds)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.LiftMm)], [nameof(SpecializedToolpathRow.Link.LiftMm)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.ThermalExposure)], [nameof(SpecializedToolpathRow.Link.ThermalExposure)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.RotationPenalty)], [nameof(SpecializedToolpathRow.Link.RotationPenalty)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.Retracts)], [nameof(SpecializedToolpathRow.Link.Retracts)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.Pierces)], [nameof(SpecializedToolpathRow.Link.Pierces)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.ToolChanges)], [nameof(SpecializedToolpathRow.Link.ToolChanges)])]
    [MapProperty([nameof(LinkLeg.Metric), nameof(LinkMetric.SetupChanges)], [nameof(SpecializedToolpathRow.Link.SetupChanges)])]
    [MapperIgnoreSource(nameof(LinkLeg.Kind))]
    [MapperIgnoreSource(nameof(LinkLeg.Path))]
    [MapperIgnoreSource(nameof(LinkLeg.Moves))]
    [MapperIgnoreSource(nameof(LinkLeg.ObjectiveScore))]
    [MapperIgnoreSource(nameof(LinkLeg.VisibilityComponents))]
    public static partial SpecializedToolpathRow.Link ToRow(LinkLeg leg);

    private static string ToKey(LinkStationKey station) => station.Key;
}

public static class Link {
    public static Fin<TOut> Route<TOut>(LinkDemand? demand, Func<Linked, TOut> project) =>
        from _ in Optional(project).ToFin(new KernelFault.InvalidValue("link", "link:projection"))
        from job in LinkJob.Admit(demand)
        let scratch = new List<int>()
        let corridor = LinkCorridor.Of(job, scratch)
        from selected in SelectTour(job, corridor, scratch)
        from refined in Refine(job, corridor, scratch, selected)
        from linked in Connect(job, refined.State, refined.Solver)
        from projected in LinkJob.Invoke(() => Fin.Succ(project(linked)))
        select projected;

    private readonly record struct BeamState(
        Seq<LinkStation> Rows,
        Seq<int> Placed,
        Seq<int> Remaining,
        HashMap<int, int> Pending,
        Option<LinkStation> Current,
        Seq<LinkLeg> Legs,
        double Score,
        int Rejected,
        int Pruned);

    private static Fin<(BeamState State, Option<double> RunnerUp, int Rejected, int Pruned)> SelectTour(
        LinkJob job, LinkCorridor corridor, List<int> scratch) {
        BidirectionalGraph<int, SEdge<int>> order = job.Precedence
            .Map(static edge => new SEdge<int>(edge.Before, edge.After))
            .ToBidirectionalGraph<int, SEdge<int>>(allowParallelEdges: false);
        order.AddVertexRange(Range(0, job.Elements.Count));
        return from beam in Range(0, job.Elements.Count).FoldM<Fin, Seq<BeamState>>(
                    Seq(new BeamState(
                        Seq<LinkStation>(),
                        Seq<int>(),
                        Range(0, job.Elements.Count).ToSeq(),
                        Range(0, job.Elements.Count).Fold(
                            HashMap<int, int>(),
                            (pending, index) => pending.Add(index, order.InDegree(index))),
                        Option<LinkStation>.None,
                        Seq<LinkLeg>(),
                        0.0,
                        0,
                        0)),
                    (states, _) => Expand(job, corridor, scratch, order, states)).As()
               from closed in Closed(job, corridor, scratch, beam)
               let ranked = toSeq(closed.OrderBy(static state => state.Score))
               from selected in ranked.Head.ToFin(Blocked(job, beam))
               select (
                   selected,
                   ranked.Skip(1).Head.Map(static state => state.Score),
                   selected.Rejected,
                   selected.Pruned);
    }

    private static Fin<Seq<BeamState>> Expand(
        LinkJob job,
        LinkCorridor corridor,
        List<int> scratch,
        BidirectionalGraph<int, SEdge<int>> order,
        Seq<BeamState> states) {
        Seq<Fin<BeamState>> attempts = states.Bind(state => state.Remaining
            .Filter(candidate => state.Pending.Find(candidate).IfNone(0) == 0)
            .Bind(candidate => job.Elements[candidate].Variants.ToSeq().Map(variant => {
                LinkStation station = LinkStation.Of(job.Elements[candidate], variant);
                LinkStation from = state.Current.Match(
                    Some: static current => current,
                    None: () => LinkStation.Park(ParkLeg.Origin, job.Start, station));
                return Transition(job, corridor, scratch, from, station).Map(leg => new BeamState(
                    state.Rows.Add(station),
                    state.Placed.Add(candidate),
                    state.Remaining.Filter(index => index != candidate),
                    toSeq(order.OutEdges(candidate)).Fold(
                        state.Pending.Remove(candidate),
                        static (rows, edge) => rows.SetItem(edge.Target, rows.Find(edge.Target).IfNone(1) - 1)),
                    Some(station),
                    state.Legs.Add(leg),
                    state.Score + leg.ObjectiveScore + job.Objective.Score(CuttingMetric(station, job.Policy)),
                    state.Rejected,
                    state.Pruned));
            })));
        var (fails, succs) = attempts.Partition();
        int pruned = Math.Max(0, succs.Count - job.Policy.BeamWidth);
        return succs.IsEmpty
            ? Fin.Fail<Seq<BeamState>>(fails.IsEmpty ? Blocked(job, states) : Error.Many(fails))
            : Fin.Succ(toSeq(succs
                .OrderBy(static state => state.Score)
                .Take(job.Policy.BeamWidth)
                .Select(state => state with {
                    Rejected = state.Rejected + fails.Count,
                    Pruned = state.Pruned + pruned,
                })));
    }

    private static Fin<Seq<BeamState>> Closed(
        LinkJob job, LinkCorridor corridor, List<int> scratch, Seq<BeamState> beam) {
        Seq<Fin<BeamState>> attempts = beam.Map(state => Close(job, corridor, scratch, state));
        var (fails, succs) = attempts.Partition();
        return succs.IsEmpty
            ? Fin.Fail<Seq<BeamState>>(Error.Many(fails))
            : Fin.Succ(succs);
    }

    private static Fin<BeamState> Close(LinkJob job, LinkCorridor corridor, List<int> scratch, BeamState state) =>
        state.Current
            .ToFin(Blocked(job, Seq(state)))
            .Bind(current => Transition(
                    job, corridor, scratch, current, LinkStation.Park(ParkLeg.Return, job.Start, current))
                .Map(leg => state with {
                    Legs = state.Legs.Add(leg),
                    Score = state.Score + leg.ObjectiveScore,
                }));

    private static Fin<(BeamState State, LinkSolver Solver)> Refine(
        LinkJob job,
        LinkCorridor corridor,
        List<int> scratch,
        (BeamState State, Option<double> RunnerUp, int Rejected, int Pruned) selected) =>
        Pairs(selected.State.Placed.Count)
            .Take(job.Policy.RefinementPairs)
            .FoldM<Fin, (BeamState State, int Exchanges, double Delta)>(
                (selected.State, 0, 0.0),
                (state, pair) => Admissible(job, state.State, pair.Earlier, pair.Later)
                    ? Exchanged(job, corridor, scratch, state.State, pair.Earlier, pair.Later).Map(swapped =>
                        swapped.Score < state.State.Score
                            ? (swapped, state.Exchanges + 1, state.Delta + state.State.Score - swapped.Score)
                            : state)
                    : Fin.Succ(state))
            .As()
            .Map(refined => (refined.State, new LinkSolver(
                job.Policy.BeamWidth,
                selected.Rejected,
                selected.Pruned,
                refined.Exchanges,
                refined.Delta,
                selected.RunnerUp)));

    private static Seq<(int Earlier, int Later)> Pairs(int count) =>
        Range(0, count).ToSeq().Bind(earlier => Range(earlier + 1, Math.Max(0, count - earlier - 1)).ToSeq()
            .Map(later => (earlier, later)));

    private static bool Admissible(LinkJob job, BeamState state, int earlier, int later) {
        int from = state.Placed[earlier];
        int to = state.Placed[later];
        Seq<int> span = state.Placed.Skip(earlier).Take(later - earlier + 1);
        return !job.Precedence.Exists(edge =>
            (edge.Before == from && span.Skip(1).Contains(edge.After))
            || (edge.After == to && span.Take(span.Count - 1).Contains(edge.Before)));
    }

    private static Fin<BeamState> Exchanged(
        LinkJob job, LinkCorridor corridor, List<int> scratch, BeamState state, int earlier, int later) {
        Seq<LinkStation> rows = state.Rows.Map((row, index) =>
            index == earlier ? state.Rows[later] : index == later ? state.Rows[earlier] : row);
        Seq<int> placed = state.Placed.Map((row, index) =>
            index == earlier ? state.Placed[later] : index == later ? state.Placed[earlier] : row);
        Seq<int> touched = Seq(earlier, earlier + 1, later, later + 1).Distinct().Filter(index => index <= rows.Count);
        return state.Legs
            .Map((leg, index) => touched.Contains(index)
                ? Transition(job, corridor, scratch, Anchor(job, rows, index - 1), Anchor(job, rows, index))
                : Fin.Succ(leg))
            .TraverseM(identity)
            .As()
            .Map(legs => state with {
                Rows = rows,
                Placed = placed,
                Legs = legs,
                Current = rows.Last,
                Score = legs.Sum(static leg => leg.ObjectiveScore)
                    + rows.Sum(row => job.Objective.Score(CuttingMetric(row, job.Policy))),
            });
    }

    private static LinkStation Anchor(LinkJob job, Seq<LinkStation> rows, int index) =>
        index < 0 ? LinkStation.Park(ParkLeg.Origin, job.Start, rows[0])
        : index >= rows.Count ? LinkStation.Park(ParkLeg.Return, job.Start, rows[^1])
        : rows[index];

    private static Error Blocked(LinkJob job, Seq<BeamState> states) =>
        new FabricationFault.LinkBlocked(
            states.Head
                .Bind(static state => state.Current)
                .Map(static station => station.Exit)
                .IfNone(job.Start),
            states.Head
                .Bind(static state => state.Remaining.Head)
                .Bind(index => job.Elements[index].Variants.ToSeq().Head)
                .Map(static variant => variant.Entry)
                .IfNone(job.Start));

    private static Fin<Linked> Connect(LinkJob job, BeamState selected, LinkSolver solver) =>
        from stations in Fin.Succ(selected.Rows.ToArr())
        from _ in !stations.IsEmpty && selected.Legs.Count == stations.Count + 1
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("link", "link:empty-tour"))
        from segments in toSeq(stations)
            .Map((station, index) => (Leg: selected.Legs[index], Station: station))
            .TraverseM(row =>
                from _guarded in Guard(job, row.Station.Moves)
                select Seq<LinkSegment>(
                    new LinkSegment.Transiting(row.Leg),
                    new LinkSegment.Cutting(row.Station.Key, row.Station.Moves, row.Station.Directives))).As()
        let closed = segments.Bind(static row => row)
            + Seq<LinkSegment>(new LinkSegment.Transiting(selected.Legs[^1]))
        from linked in Tour(job, stations, selected.Legs, closed, solver)
        select linked;

    private static Fin<Unit> Guard(LinkJob job, Seq<Move> source) => LinkJob.Invoke(() => job.Guard(source));

    private static Fin<LinkLeg> Transition(
        LinkJob job, LinkCorridor corridor, List<int> scratch, LinkStation from, LinkStation to) =>
        from route in Path(job, corridor, scratch, from.Exit, to.Entry)
        from _ in !route.Kind.RequiresPlane || route.Points.Count >= 3
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.LinkBlocked(from.Exit, to.Entry))
        from moves in LinkJob.Invoke(() => job.Lower(route.Points, route.Kind))
        from admitted in moves.TraverseM(Move.Admit).As()
        from _guarded in Guard(job, admitted)
        let metric = Metric(route.Points, route.Kind, job.Policy,
            from.ToolKey != to.ToolKey, from.WorkOffset != to.WorkOffset)
        select new LinkLeg(
            from.Key, to.Key, route.Kind, route.Points, admitted, metric,
            job.Objective.Score(metric), route.Components);

    private static Fin<(Seq<Point3d> Points, RetractKind Kind, int Components)> Path(
        LinkJob job, LinkCorridor corridor, List<int> scratch, Point3d from, Point3d to) {
        LinkClearance clearance = job.Policy.Clearance;
        double clearancePlane = job.Keepouts
            .Bind(static row => row.Extent.Top.ToSeq())
            .Fold(Math.Max(from.Z, to.Z), Math.Max) + (double)clearance.Plane.Millimeters;
        double skimZ = Math.Max(from.Z, to.Z) + (double)clearance.Skim.Millimeters;
        Point3d skimFrom = new(from.X, from.Y, skimZ);
        Point3d skimTo = new(to.X, to.Y, skimZ);
        bool direct = Clear(from, to, job.Keepouts, clearance.Slack, scratch)
            && Math.Abs(to.Z - from.Z) <= (double)clearance.RampRise.Millimeters;
        bool skim = Clear(from, skimFrom, job.Keepouts, clearance.Slack, scratch)
            && Clear(skimFrom, skimTo, job.Keepouts, clearance.Slack, scratch)
            && Clear(skimTo, to, job.Keepouts, clearance.Slack, scratch);
        return (direct, skim) switch {
            (true, _) => Fin.Succ((Seq(from, to), from.Z == to.Z ? RetractKind.Direct : RetractKind.Ramp, 1)),
            (false, true) => Fin.Succ((Seq(from, skimFrom, skimTo, to), RetractKind.Skim, 1)),
            _ => corridor.Route(job, scratch, from, to, clearancePlane),
        };
    }

    private sealed class LinkCorridor {
        private LinkCorridor(Arr<Keepout> walls, Arr<(double X, double Y)> corners, Arr<(int From, int To)> edges) =>
            (Walls, Corners, Edges) = (walls, corners, edges);

        private Arr<Keepout> Walls { get; }
        private Arr<(double X, double Y)> Corners { get; }
        private Arr<(int From, int To)> Edges { get; }

        public static LinkCorridor Of(LinkJob job, List<int> scratch) {
            LinkClearance clearance = job.Policy.Clearance;
            Arr<Keepout> walls = job.Keepouts.Filter(static row => row.Extent is KeepoutExtent.Unbounded);
            Arr<(double X, double Y)> corners = walls.Bind(row => row.Geometry.Bind(region =>
                Range(0, region.Boundary.Vertices.Count).ToArr().Bind(index => region.Boundary.BulgeAt(index) == 0.0
                    ? Arr(Corner(region.Boundary, index, clearance.Corner))
                    : Arr(
                        Corner(region.Boundary, index, clearance.Corner),
                        Apex(region.Boundary, index, clearance.Corner)))));
            Arr<(int From, int To)> edges = Range(0, corners.Count).ToArr()
                .Bind(i => Range(i + 1, corners.Count - i - 1).ToArr().Map(j => (From: i, To: j)))
                .Filter(edge => Visible(walls, corners[edge.From], corners[edge.To], clearance.Slack, scratch));
            return new LinkCorridor(walls, corners, edges);
        }

        public Fin<(Seq<Point3d> Points, RetractKind Kind, int Components)> Route(
            LinkJob job, List<int> scratch, Point3d from, Point3d to, double plane) {
            LinkClearance clearance = job.Policy.Clearance;
            Point3d liftedFrom = new(from.X, from.Y, plane);
            Point3d liftedTo = new(to.X, to.Y, plane);
            if (!Clear(from, liftedFrom, job.Keepouts, clearance.Slack, scratch)
                || !Clear(liftedTo, to, job.Keepouts, clearance.Slack, scratch))
                return Fin.Fail<(Seq<Point3d>, RetractKind, int)>(new FabricationFault.LinkBlocked(from, to));
            const int Start = 0;
            const int End = 1;
            Arr<(double X, double Y)> vertices = Arr((liftedFrom.X, liftedFrom.Y), (liftedTo.X, liftedTo.Y)) + Corners;
            BidirectionalGraph<int, TaggedEdge<int, double>> graph = (
                    Edges.Map(edge => (From: edge.From + 2, To: edge.To + 2))
                    + Seq(Start, End).ToArr().Bind(terminal => Range(0, Corners.Count).ToArr()
                        .Map(corner => (From: terminal, To: corner + 2)))
                    + Arr((From: Start, To: End)))
                .Filter(edge => (edge.From >= 2 && edge.To >= 2)
                    || Visible(Walls, vertices[edge.From], vertices[edge.To], clearance.Slack, scratch))
                .Bind(edge => Seq(
                    new TaggedEdge<int, double>(edge.From, edge.To, Span(vertices, edge.From, edge.To)),
                    new TaggedEdge<int, double>(edge.To, edge.From, Span(vertices, edge.From, edge.To))))
                .ToBidirectionalGraph<int, TaggedEdge<int, double>>(allowParallelEdges: false);
            graph.AddVertexRange(Range(0, vertices.Count));
            Dictionary<int, int> components = [];
            int componentCount = graph.WeaklyConnectedComponents(components);
            if (components[Start] != components[End])
                return Fin.Fail<(Seq<Point3d>, RetractKind, int)>(new FabricationFault.LinkBlocked(from, to));
            TryFunc<int, IEnumerable<TaggedEdge<int, double>>> search = graph.ShortestPathsAStar(
                static edge => edge.Tag,
                vertex => Span(vertices, vertex, End),
                Start);
            if (!search(End, out IEnumerable<TaggedEdge<int, double>>? path))
                return Fin.Fail<(Seq<Point3d>, RetractKind, int)>(new FabricationFault.LinkBlocked(from, to));
            Seq<TaggedEdge<int, double>> route = toSeq(path);
            RetractKind kind = route.Count > 1
                ? RetractKind.Routed
                : Math.Abs(plane - to.Z) > (double)clearance.RampRise.Millimeters
                    ? RetractKind.ControlledDescent
                    : RetractKind.FullLift;
            return Fin.Succ((
                Seq(from, liftedFrom)
                    + route.Map(edge => new Point3d(vertices[edge.Target].X, vertices[edge.Target].Y, plane))
                    + Seq(to),
                kind,
                componentCount));
        }

        private static double Span(Arr<(double X, double Y)> vertices, int from, int to) =>
            Math.Sqrt(Math.Pow(vertices[to].X - vertices[from].X, 2.0) + Math.Pow(vertices[to].Y - vertices[from].Y, 2.0));

        private static bool Visible(
            Arr<Keepout> walls, (double X, double Y) from, (double X, double Y) to, Length slack, List<int> scratch) =>
            Clear(new Point3d(from.X, from.Y, 0.0), new Point3d(to.X, to.Y, 0.0), walls, slack, scratch);
    }

    private readonly struct SegmentClear(Polyline<double> boundary, Point3d from, Point3d to, double tolerance)
        : IQueryVisitor {
        public bool Visit(int segment) => PlineSegIntersection.Intersect(
            PlineVertex<double>.FromSlice([from.X, from.Y, 0.0]),
            PlineVertex<double>.FromSlice([to.X, to.Y, 0.0]),
            boundary[segment],
            boundary[boundary.NextWrappingIndex(segment)],
            tolerance).Kind is not (PlineSegIntrKind.OneIntersect or PlineSegIntrKind.TwoIntersects);
    }

    private static bool Clear(Point3d from, Point3d to, Arr<Keepout> keepouts, Length slack, List<int> scratch) {
        double tolerance = slack.Millimeters;
        return keepouts.Filter(row => row.Active(from.Z, to.Z)).ForAll(row => row.Geometry.ForAll(region => {
            if (region.Boundary.Covers(new Point3d(from.X, from.Y, region.Boundary.Plane))
                || region.Boundary.Covers(new Point3d(to.X, to.Y, region.Boundary.Plane)))
                return false;
            SegmentClear visitor = new(region.Index.Polyline, from, to, tolerance);
            return region.Index.SpatialIndex.VisitQueryWithStack(
                Math.Min(from.X, to.X) - tolerance,
                Math.Min(from.Y, to.Y) - tolerance,
                Math.Max(from.X, to.X) + tolerance,
                Math.Max(from.Y, to.Y) + tolerance,
                ref visitor,
                scratch);
        }));
    }

    private static (double X, double Y) Corner(Loop loop, int index, Length offset) {
        double clearance = offset.Millimeters;
        Point3d previous = loop.At(index - 1);
        Point3d point = loop.At(index);
        Point3d next = loop.At(index + 1);
        Vector2d incoming = new(point.X - previous.X, point.Y - previous.Y);
        Vector2d outgoing = new(next.X - point.X, next.Y - point.Y);
        if (!incoming.Unitize() || !outgoing.Unitize())
            return (point.X, point.Y);
        Vector2d direction = loop.Winding() == Sign.Negative
            ? new Vector2d(-incoming.Y - outgoing.Y, incoming.X + outgoing.X)
            : new Vector2d(incoming.Y + outgoing.Y, -incoming.X - outgoing.X);
        return direction.Unitize()
            ? (point.X + direction.X * clearance, point.Y + direction.Y * clearance)
            : (point.X, point.Y);
    }

    private static (double X, double Y) Apex(Loop loop, int index, Length offset) {
        double clearance = offset.Millimeters;
        Polyline<double> path = new(
            toSeq(loop.Vertices).Map((point, ordinal) => PlineVertex<double>.FromSlice(
                [point.X, point.Y, loop.BulgeAt(ordinal)])),
            loop.Closed);
        Vector2<double> mid = PlineSeg.SegMidpoint(path[index], path[path.NextWrappingIndex(index)]);
        Point3d start = loop.At(index);
        Point3d end = loop.At(index + 1);
        Vector2d outward = new(mid.X - 0.5 * (start.X + end.X), mid.Y - 0.5 * (start.Y + end.Y));
        return outward.Unitize()
            ? (mid.X + outward.X * clearance, mid.Y + outward.Y * clearance)
            : (mid.X, mid.Y);
    }

    private static LinkMetric Metric(
        Seq<Point3d> path,
        RetractKind kind,
        LinkPolicy policy,
        bool toolChange,
        bool setupChange) {
        double rapid = policy.Rate.Rapid.MillimetersPerMinutes;
        double plunge = policy.Rate.Plunge.MillimetersPerMinutes;
        (double Distance, double Horizontal, double Vertical, double Turning) lengths = path.Zip(path.Skip(1))
            .Zip(path.Skip(2).Map(static point => Some(point)).Add(Option<Point3d>.None))
            .Fold(
                (Distance: 0.0, Horizontal: 0.0, Vertical: 0.0, Turning: 0.0),
                static (sum, leg) => {
                    ((Point3d first, Point3d second), Option<Point3d> third) = leg;
                    double distance = first.DistanceTo(second);
                    double vertical = Math.Abs(second.Z - first.Z);
                    return (
                        sum.Distance + distance,
                        sum.Horizontal + Math.Sqrt(Math.Max(0.0, distance * distance - vertical * vertical)),
                        sum.Vertical + vertical,
                        sum.Turning + third.Match(
                            Some: next => Vector3d.VectorAngle(second - first, next - second),
                            None: static () => 0.0));
                });
        return new LinkMetric(
            lengths.Distance,
            60.0 * ((lengths.Horizontal / rapid) + (lengths.Vertical / plunge))
                + (toolChange ? (double)policy.Changeover.Tool.Seconds : 0.0)
                + (setupChange ? (double)policy.Changeover.Setup.Seconds : 0.0),
            lengths.Vertical,
            kind.ThermalCoupling * lengths.Horizontal,
            lengths.Turning,
            kind.Retracts,
            0,
            toolChange ? 1 : 0,
            setupChange ? 1 : 0);
    }

    private static LinkMetric CuttingMetric(LinkStation station, LinkPolicy policy) {
        double rapid = policy.Rate.Rapid.MillimetersPerMinutes;
        (Point3d Cursor, double Distance, double Duration) walked = station.Moves.Fold(
            (Cursor: station.Entry, Distance: 0.0, Duration: 0.0),
            (state, move) => {
                double distance = Swept(state.Cursor, move);
                double feed = move.Switch(
                    rapid: _ => rapid,
                    linear: static row => row.Feed,
                    circular: static row => row.Feed);
                return (move.Target, state.Distance + distance, state.Duration + 60.0 * distance / feed);
            });
        return new LinkMetric(
            walked.Distance,
            walked.Duration,
            0.0,
            station.ThermalExposure,
            station.RotationPenalty,
            0,
            station.Pierces,
            0,
            0);
    }

    internal static double Swept(Point3d from, Move move) => move.Switch(
        state: from,
        rapid: static (start, row) => start.DistanceTo(row.Target),
        linear: static (start, row) => start.DistanceTo(row.Target),
        circular: static (start, row) => Math.Sqrt(
            Math.Pow(row.Radius * Math.Abs(row.SweepRadians), 2.0) + Math.Pow(row.Target.Z - start.Z, 2.0)));

    private static Fin<Linked> Tour(
        LinkJob job,
        Arr<LinkStation> stations,
        Seq<LinkLeg> legs,
        Seq<LinkSegment> segments,
        LinkSolver solver) {
        LinkMetric cutting = stations.Fold(new LinkMetric(), (sum, row) => sum + CuttingMetric(row, job.Policy));
        return SpecializedToolpathEnvelope.Admit(
                SpecializedToolpathKind.Link,
                legs.Map(static row => (SpecializedToolpathRow)ToolpathRowMap.ToRow(row)),
                legs.Sum(static row => row.Metric.DurationSeconds))
            .Map(envelope => new Linked(
                segments,
                new LinkTour(
                    stations.Map(static row => row.Key),
                    legs,
                    legs.Fold(cutting, static (sum, row) => sum + row.Metric),
                    legs.Sum(static row => row.ObjectiveScore) + job.Objective.Score(cutting),
                    solver,
                    segments.Fold(0, static (count, segment) => count + segment.Emitted.Count)),
                envelope));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
