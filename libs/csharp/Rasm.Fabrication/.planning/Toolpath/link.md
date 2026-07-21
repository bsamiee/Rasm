# [RASM_FABRICATION_LINK]

`Link.Route` admits cut occurrences, keepout volumes, sequencing constraints, machine clearance, and a weighted objective before selecting orientation, order, and transition posture. `Linked` preserves every cutting and transition move, and `LinkReceipt` explains the winning full route.

`Link.Route`, `Linked`, and `LinkReceipt` are the linking seam. `Linked.SpecializedDirective` projects transition metrics into the specialized-toolpath envelope, while cutting directives remain on source segments. `ArcAlgebra.Apply` owns planar clearance inflation, `QuikGraph` owns reachability and routed transitions, and supplied `Guard` verifies collision admission without importing host state. Tool and work-offset identity remain objective terms.

## [01]-[INDEX]

- [01]-[ADMISSION]: `LinkDemand` materializes dimensions and delegates once; generated owners admit coupled elements, keepouts, precedence, policy, and objective.
- [02]-[ROUTING]: `Link.Route` jointly selects a precedence-safe tour and occurrence orientation against realized transition costs, then lowers each transition through one rail.
- [03]-[EGRESS]: `Linked` projects through a caller-supplied arrow while retaining route, objective, reachability, and guard evidence.

## [02]-[ADMISSION]

`LinkJob` owns every fact that changes ordering or transition safety. Geometry and vertical extent are one `Keepout`; tool, work offset, thermal load, and admissible orientations are one `CutElement`; neither admits a parallel ordinal collection.

- Owner: `CutElement.Identify` admits and mints the complete motion or surface preimage once; `ElementVariant` couples occurrence orientation with exact entry, exit, and cutting moves; `LinkStation` binds the chosen variant to `ToolKey` and `WorkOffset`, and `LinkStation.Park` gives home and return legs the same machine identity.
- Owner: `Keepout` couples the stable obstacle key, each inflated region and its admission-built `IndexedPolyline<double>` flatbush index, and payload-bearing `KeepoutExtent`.
- Owner: `LinkPolicy` carries machine lift, ramp, skim, clearance, tolerance, feed rates, and tool/setup change durations after dimensional conversion, so each change is priced in seconds.
- Owner: generated `LinkObjective` weights admit distance, time, lift, thermal, rotation, retract, pierce, tool change, and setup change; named objectives are seed data over one metric generator.
- Packages: `ProcessPhysics.Admit` owns invariant length-text admission; `TensorPrimitives.IsFiniteAll` admits numeric batches; `Thinktecture` closes construction.
- Boundary: `LinkDemand` crosses the nullable boundary exactly once, and every interior function consumes `LinkJob`.

## [03]-[ROUTING]

`Link.Route` chooses among direct, ramped, skim, lifted, visibility-routed, and controlled-descent transitions. Each posture is a case over one geometric transition; no nullable move family or sentinel cost crosses the rail.

- Entry: `Route<TOut>` parameterizes raw ingress, transition lowering, collision guard, and egress projection.
- Auto: one precedence-aware beam state owns each chosen occurrence, variant, identity-change charge, realized obstacle route, and return-home score.
- Auto: `BeamState` carries pending precedence in-degrees, so frontier readiness is one map probe per candidate; occurrence variants remain part of the ordering choice.
- Auto: each candidate transition lowers, admits, and guards before entering the beam; `BeamState` retains accepted legs, so final connection cannot discover a transition failure.
- Auto: the tour closes — a park leg precedes the first station and follows the last, so return travel is priced and guarded like any transition.
- Auto: direct three-dimensional travel remains direct when the corridor and ramp envelope are safe; differing endpoint heights never force a skim.
- Auto: clearance planes exceed endpoints and bounded obstacle tops; unbounded obstacles enter a visibility graph carrying each bulged span's arc apex beside its corner, whose connected components prove reachability before `ShortestPathsAStar` runs.
- Packages: `ArcAlgebra.Apply` inflates arc-native keepouts; `StaticAABB2DIndex<double>.Query` prunes corridor tests; `PlineSegIntersection.Intersect` preserves bulged boundaries; `PlineSeg.SegMidpoint` places arc apexes; QuikGraph owns DAG, components, and weighted recovery; LanguageExt keeps failures flat.
- Exemption: index-pruned arc intersection and graph search are measured kernel statement boundaries.
- Boundary: `FabricationFault.LinkBlocked` names the stalled cursor and first withheld element; `Guard` verifies each segment without rewriting moves or refuses it without swallowed failure.

## [04]-[EGRESS]

`Linked` is inverse-sufficient: `LinkSegment.Cutting` and `Transiting` keep cut and travel moves distinct, `Linked.Moves` projects guarded motion without erasing that split, and `LinkReceipt` preserves order, rejected alternatives, component evidence, guarded-move count, and full-route metrics.

- Receipt: `TransitionReceipt` records transition endpoints, posture, distance, time, lift, retract, tool-change, and setup-change terms beside its objective score; `LinkReceipt.Total` adds cutting distance, time, thermal exposure, rotation, and pierces exactly once.
- Projection: `Linked.PostingSource` carries transition evidence into canonical posting; the caller arrow retains other result projections.
- Consumer: motion supplies transition lowering; its commit fold conditions and guards the linked program once. Posting and simulation retain typed transition metrics, and estimation consumes the realized motion clock without double-counting receipt duration.
- Growth: a new machine posture is one `RetractKind` policy value; a new cost regime is one admitted `LinkObjective`; a new obstacle occurrence is one `Keepout` admission; a new move classification is one `LinkSegment` case.
- Boundary: no route publishes `double.PositiveInfinity`, a disconnected partial tour, an open tour that never returns home, or unguarded moves.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
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
using QuikGraph.Algorithms.ShortestPath;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
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
    public static readonly RetractKind Direct = new("direct", retracts: 0, requiresPlane: false);
    public static readonly RetractKind Ramp = new("ramp", retracts: 0, requiresPlane: false);
    public static readonly RetractKind Skim = new("skim", retracts: 1, requiresPlane: true);
    public static readonly RetractKind FullLift = new("full-lift", retracts: 1, requiresPlane: true);
    public static readonly RetractKind Routed = new("routed", retracts: 1, requiresPlane: true);
    public static readonly RetractKind ControlledDescent = new("controlled-descent", retracts: 1, requiresPlane: true);

    public int Retracts { get; }
    public bool RequiresPlane { get; }
}

// --- [ADMISSION] ----------------------------------------------------------------------------------------------------------------------------------
public sealed record ElementVariant(
    string Key,
    Point3d Entry,
    Point3d Exit,
    Seq<Move> Moves,
    double RotationPenalty,
    double ThermalExposure,
    int Pierces,
    Seq<MotionDirective> Directives = default);

[Union]
public abstract partial record EntryFamily {
    public sealed record Fixed(ElementVariant Variant) : EntryFamily;
    public sealed record Reversible(ElementVariant Forward, ElementVariant Reverse) : EntryFamily;
    public sealed record Cyclic(Loop Boundary, int Samples, Func<Point3d, Fin<ElementVariant>> AtPoint) : EntryFamily;
}

[Union]
public abstract partial record CutElementIdentity(
    CutStrategy Strategy,
    string ToolKey,
    string WorkOffset,
    string CutterFamily,
    double CutterDiameter,
    double CutterCornerRadius,
    double CutterTaperAngle,
    double CutterFluteLength,
    Seq<Move> Moves) {
    public sealed record Motion(
        int Occurrence,
        CutStrategy Strategy,
        string ToolKey,
        string WorkOffset,
        string CutterFamily,
        double CutterDiameter,
        double CutterCornerRadius,
        double CutterTaperAngle,
        double CutterFluteLength,
        Seq<Move> Moves) : CutElementIdentity(
            Strategy, ToolKey, WorkOffset, CutterFamily, CutterDiameter, CutterCornerRadius,
            CutterTaperAngle, CutterFluteLength, Moves);
    public sealed record Surface(
        int View,
        int Path,
        CutStrategy Strategy,
        string Operation,
        string ToolKey,
        string WorkOffset,
        string CutterFamily,
        double CutterDiameter,
        double CutterCornerRadius,
        double CutterTaperAngle,
        double CutterFluteLength,
        Seq<Move> Moves) : CutElementIdentity(
            Strategy, ToolKey, WorkOffset, CutterFamily, CutterDiameter, CutterCornerRadius,
            CutterTaperAngle, CutterFluteLength, Moves);
}

[ComplexValueObject]
public sealed partial class CutElement {
    public string Key { get; }
    public string ToolKey { get; }
    public string WorkOffset { get; }
    public EntryFamily Entry { get; }
    public Arr<ElementVariant> Variants { get; }

    public static Fin<string> Identify(CutElementIdentity? identity) =>
        from source in Optional(identity)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:element-identity").ToError())
        from moves in Complete(source)
            ? Fin.Succ(source.Moves)
            : Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:element-identity").ToError())
        from admitted in moves.TraverseM(Move.Admit).As()
        select Identity(source, admitted);

    public static Fin<CutElement> Admit(string key, string toolKey, string workOffset, EntryFamily entry) =>
        from variants in Variants(entry)
        from checkedVariants in variants.TraverseM(AdmitVariant).As()
        let admittedVariants = checkedVariants.ToArr()
        from admitted in Validate(key, toolKey, workOffset, entry, admittedVariants, out CutElement element) is { } error
            ? Fin.Fail<CutElement>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(element)
        select admitted;

    private static Fin<ElementVariant> AdmitVariant(ElementVariant? variant) =>
        from source in Optional(variant)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:element-variant").ToError())
        from moves in source.Moves.TraverseM(Move.Admit).As()
        select source with { Moves = moves };

    private static bool Complete(CutElementIdentity identity) =>
        Common(identity.Strategy, identity.ToolKey, identity.WorkOffset, identity.CutterFamily,
            identity.CutterDiameter, identity.CutterCornerRadius, identity.CutterTaperAngle,
            identity.CutterFluteLength, identity.Moves)
        && identity.Switch(
            motion: static row => row.Occurrence >= 0,
            surface: static row => row.View >= 0 && row.Path >= 0 && !string.IsNullOrWhiteSpace(row.Operation));

    private static bool Common(
        CutStrategy strategy,
        string toolKey,
        string workOffset,
        string cutterFamily,
        double cutterDiameter,
        double cutterCornerRadius,
        double cutterTaperAngle,
        double cutterFluteLength,
        Seq<Move> moves) => strategy is not null
        && !string.IsNullOrWhiteSpace(toolKey)
        && !string.IsNullOrWhiteSpace(workOffset)
        && !string.IsNullOrWhiteSpace(cutterFamily)
        && TensorPrimitives.IsFiniteAll<double>(
            [cutterDiameter, cutterCornerRadius, cutterTaperAngle, cutterFluteLength])
        && cutterDiameter > 0.0
        && cutterCornerRadius >= 0.0
        && cutterTaperAngle >= 0.0
        && cutterFluteLength > 0.0
        && !moves.IsEmpty;

    private static string Identity(CutElementIdentity identity, Seq<Move> moves) {
        CanonicalWriter writer = new CanonicalWriter(0.0);
        _ = identity.Switch(
            state: writer,
            motion: static (preimage, row) => preimage
                .String("motion")
                .Ordinal(row.Occurrence)
                .String(row.Strategy.Key),
            surface: static (preimage, row) => preimage
                .String("surface")
                .Ordinal(row.View)
                .Ordinal(row.Path)
                .String(row.Strategy.Key)
                .String(row.Operation));
        writer.String(identity.ToolKey)
            .String(identity.WorkOffset)
            .String(identity.CutterFamily)
            .Double(identity.CutterDiameter)
            .Double(identity.CutterCornerRadius)
            .Double(identity.CutterTaperAngle)
            .Double(identity.CutterFluteLength);
        writer.Ordinal(moves.Count);
        moves.Iter(move => Write(writer, move));
        return ContentHash.Of(writer.ToBytes().Span).ToString("x32", CultureInfo.InvariantCulture);
    }

    private static Unit Write(CanonicalWriter writer, Move move) => move.Switch(
        state: writer,
        rapid: static (preimage, row) => Write(preimage, 0, row.Target, 0.0, Point3d.Origin, 0.0, 0.0),
        linear: static (preimage, row) => Write(preimage, 1, row.Target, row.Feed, Point3d.Origin, 0.0, 0.0),
        circular: static (preimage, row) => Write(preimage, 2, row.Target, row.Feed, row.Arc.Center,
            row.Arc.Sense == RotationSense.Clockwise ? -1.0 : 1.0, row.SweepRadians));

    private static Unit Write(
        CanonicalWriter writer,
        int kind,
        Point3d target,
        double feed,
        Point3d center,
        double sense,
        double sweep) {
        writer.Ordinal(kind)
            .Double(target.X)
            .Double(target.Y)
            .Double(target.Z)
            .Double(feed)
            .Double(center.X)
            .Double(center.Y)
            .Double(center.Z)
            .Double(sense)
            .Double(sweep);
        return unit;
    }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref string toolKey,
        ref string workOffset,
        ref EntryFamily entry,
        ref Arr<ElementVariant> variants) {
        bool valid = !variants.IsEmpty && variants.ForAll(static row => row is not null
            && !string.IsNullOrWhiteSpace(row.Key) && row.Entry.IsValid && row.Exit.IsValid && !row.Moves.IsEmpty
            && TensorPrimitives.IsFiniteAll<double>([row.RotationPenalty, row.ThermalExposure])
            && row.RotationPenalty >= 0.0 && row.ThermalExposure >= 0.0 && row.Pierces >= 0);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(toolKey) || string.IsNullOrWhiteSpace(workOffset)
            || entry is null || !valid)
            validationError = new ValidationError("link:element");
    }

    private static Fin<Arr<ElementVariant>> Variants(EntryFamily? entry) =>
        from family in Optional(entry).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:entry-family").ToError())
        from variants in family.Switch(
            @fixed: row => Optional(row.Variant)
                .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:fixed-entry").ToError())
                .Map(variant => Arr(variant)),
            reversible: row =>
                from forward in Optional(row.Forward)
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:forward-entry").ToError())
                from reverse in Optional(row.Reverse)
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:reverse-entry").ToError())
                select Arr(forward, reverse),
            cyclic: Cyclic)
        select variants;

    private static Fin<Arr<ElementVariant>> Cyclic(EntryFamily.Cyclic row) {
        if (row.Boundary is null || !row.Boundary.Closed || row.Samples < 2 || row.AtPoint is null)
            return Fin.Fail<Arr<ElementVariant>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:cyclic-entry").ToError());
        Polyline<double> path = new(
            row.Boundary.Vertices.Map((point, index) => PlineVertex<double>.FromSlice(
                [point.X, point.Y, row.Boundary.BulgeAt(index)])),
            true);
        double length = path.PathLength();
        return Range(0, row.Samples).ToSeq().TraverseM(index =>
            path.FindPointAtPathLength(index * length / row.Samples) switch {
                (true, _, Vector2<double> point, _) => LinkJob.Invoke(
                    () => row.AtPoint(new Point3d(point.X, point.Y, row.Boundary.Plane))),
                _ => Fin.Fail<ElementVariant>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:cyclic-station").ToError()),
            }).As().Map(static rows => rows.ToArr());
    }
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
        bounded: static row => double.IsFinite(row.FloorZ) && double.IsFinite(row.TopZ) && row.TopZ >= row.FloorZ,
        unbounded: static _ => true);
}

[ComplexValueObject]
public sealed partial class Keepout {
    public string Key { get; }
    public Arr<(Loop Boundary, IndexedPolyline<double> Index)> Geometry { get; }
    public KeepoutExtent Extent { get; }
    public double MarginMm { get; }

    public bool Active(double fromZ, double toZ) => Extent.Active(fromZ, toZ);

    public static Fin<Keepout> Admit(string key, Loop? footprint, KeepoutExtent? extent, string marginText) =>
        from boundary in Optional(footprint).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:keepout-footprint").ToError())
        from volume in Optional(extent).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:keepout-extent").ToError())
        from margin in LinkPolicy.Millimeters(marginText, "link:keepout-margin")
        from regions in Inflate(boundary, margin)
        let geometry = regions.Map(static loop => (Boundary: loop, Index: Index(loop))).ToArr()
        from admitted in Validate(key, geometry, volume, margin, out Keepout keepout) is { } error
            ? Fin.Fail<Keepout>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(keepout)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref Arr<(Loop Boundary, IndexedPolyline<double> Index)> geometry,
        ref KeepoutExtent extent,
        ref double marginMm) {
        if (string.IsNullOrWhiteSpace(key) || geometry.IsEmpty
            || !geometry.ForAll(static row => row.Boundary is not null && row.Boundary.Closed && row.Boundary.Count >= 3)
            || extent is null || !extent.IsValid || !double.IsFinite(marginMm) || marginMm < 0.0)
            validationError = new ValidationError("link:keepout");
    }

    // Keepout admission builds one flatbush index per inflated region; every corridor test queries it instead of walking segments.
    private static IndexedPolyline<double> Index(Loop loop) {
        Polyline<double> path = new(
            loop.Vertices.Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])),
            loop.Closed);
        return new IndexedPolyline<double>(path, path.CreateAabbIndex());
    }

    private static Fin<Seq<Loop>> Inflate(Loop footprint, double margin) =>
        ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(footprint), margin)).Bind(trace => trace.Switch(
            forest: static row => Fin.Succ(row.Result.Loops),
            paths: static row => Fin.Succ(row.Result),
            motion: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:keepout-offset").ToError()),
            inspection: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:keepout-offset").ToError()),
            densified: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:keepout-offset").ToError()),
            recovered: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:keepout-offset").ToError())));

}

public readonly record struct OrderConstraint(int Before, int After);

[ComplexValueObject]
public sealed partial class LinkPolicy {
    public double ClearanceMm { get; }
    public double SkimClearanceMm { get; }
    public double RampRiseMm { get; }
    public double RoutedCornerClearanceMm { get; }
    public double RapidMmPerMin { get; }
    public double PlungeMmPerMin { get; }
    public double ToolChangeSeconds { get; }
    public double SetupChangeSeconds { get; }
    public double ToleranceMm { get; }

    public static Fin<LinkPolicy> Admit(
        string clearance,
        string skimClearance,
        string rampRise,
        string routedCornerClearance,
        double rapidMmPerMin,
        double plungeMmPerMin,
        double toolChangeSeconds,
        double setupChangeSeconds,
        double toleranceMm) =>
        from lift in Millimeters(clearance, "link:clearance")
        from skim in Millimeters(skimClearance, "link:skim")
        from ramp in Millimeters(rampRise, "link:ramp")
        from corner in Millimeters(routedCornerClearance, "link:corner")
        from admitted in Validate(lift, skim, ramp, corner, rapidMmPerMin, plungeMmPerMin,
            toolChangeSeconds, setupChangeSeconds, toleranceMm, out LinkPolicy policy) is { } error
            ? Fin.Fail<LinkPolicy>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(policy)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double clearanceMm,
        ref double skimClearanceMm,
        ref double rampRiseMm,
        ref double routedCornerClearanceMm,
        ref double rapidMmPerMin,
        ref double plungeMmPerMin,
        ref double toolChangeSeconds,
        ref double setupChangeSeconds,
        ref double toleranceMm) {
        if (!TensorPrimitives.IsFiniteAll<double>([clearanceMm, skimClearanceMm, rampRiseMm, routedCornerClearanceMm,
                rapidMmPerMin, plungeMmPerMin, toolChangeSeconds, setupChangeSeconds, toleranceMm])
            || clearanceMm <= 0.0 || skimClearanceMm < 0.0 || rampRiseMm < 0.0 || routedCornerClearanceMm <= 0.0
            || rapidMmPerMin <= 0.0 || plungeMmPerMin <= 0.0 || toolChangeSeconds < 0.0 || setupChangeSeconds < 0.0
            || toleranceMm <= 0.0)
            validationError = new ValidationError("link:policy");
    }

    internal static Fin<double> Millimeters(string text, string field) =>
        ProcessPhysics.Admit(new PhysicsIngress.Quantity(PhysicsQuantity.Length, text)).Bind(admitted =>
            admitted is PhysicsAdmission.Quantity row && row.Kind == PhysicsQuantity.Length
                ? Fin.Succ(row.Canonical)
                : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, field).ToError()));
}

public sealed record LinkDemand(
    Point3d Start,
    Arr<CutElement> Elements,
    Arr<Keepout> Keepouts,
    Arr<OrderConstraint> Precedence,
    LinkPolicy Policy,
    LinkObjective Objective,
    Func<Seq<Point3d>, RetractKind, Fin<Seq<Move>>> Lower,
    Func<Seq<Move>, Fin<Seq<Move>>> Guard);

[ComplexValueObject]
public sealed partial class LinkJob {
    public Point3d Start { get; }
    public Arr<CutElement> Elements { get; }
    public Arr<Keepout> Keepouts { get; }
    public Arr<OrderConstraint> Precedence { get; }
    public LinkPolicy Policy { get; }
    public LinkObjective Objective { get; }
    public Func<Seq<Point3d>, RetractKind, Fin<Seq<Move>>> Lower { get; }
    public Func<Seq<Move>, Fin<Seq<Move>>> Guard { get; }

    public static Fin<LinkJob> Admit(LinkDemand? candidate) =>
        from raw in Optional(candidate).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:demand").ToError())
        from admitted in Validate(raw.Start, raw.Elements, raw.Keepouts, raw.Precedence, raw.Policy,
            raw.Objective, raw.Lower, raw.Guard, out LinkJob job) is { } error
            ? Fin.Fail<LinkJob>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(job)
        select admitted;

    internal static Fin<T> Invoke<T>(Func<Fin<T>> callback) =>
        Try.lift<Fin<T>>(callback).Run().Bind(static result => result);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Point3d start,
        ref Arr<CutElement> elements,
        ref Arr<Keepout> keepouts,
        ref Arr<OrderConstraint> precedence,
        ref LinkPolicy policy,
        ref LinkObjective objective,
        ref Func<Seq<Point3d>, RetractKind, Fin<Seq<Move>>> lower,
        ref Func<Seq<Move>, Fin<Seq<Move>>> guard) {
        bool references = !elements.Exists(static row => row is null)
            && !keepouts.Exists(static row => row is null)
            && policy is not null && objective is not null && lower is not null && guard is not null;
        if (!start.IsValid || elements.IsEmpty || !references) {
            validationError = new ValidationError("link:job");
            return;
        }
        int elementCount = elements.Count;
        bool edges = precedence.ForAll(edge => edge.Before >= 0 && edge.Before < elementCount
            && edge.After >= 0 && edge.After < elementCount && edge.Before != edge.After);
        bool unique = elements.Map(static row => row.Key).Distinct().Count == elements.Count
            && keepouts.Map(static row => row.Key).Distinct().Count == keepouts.Count;
        if (!unique || !edges || !Acyclic(precedence))
            validationError = new ValidationError("link:job");
    }

    private static bool Acyclic(Arr<OrderConstraint> precedence) =>
        precedence.Map(static row => new SEdge<int>(row.Before, row.After))
            .IsDirectedAcyclicGraph<int, SEdge<int>>();
}

// --- [EVIDENCE] -----------------------------------------------------------------------------------------------------------------------------------
public sealed record TransitionReceipt(
    string From,
    string To,
    RetractKind Kind,
    Seq<Point3d> Path,
    LinkMetric Metric,
    double ObjectiveScore,
    int VisibilityComponents);

public sealed record LinkStation(
    string Key,
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
        variant.Key, element.ToolKey, element.WorkOffset, variant.Entry, variant.Exit, variant.Moves, variant.Directives,
        variant.RotationPenalty, variant.ThermalExposure, variant.Pierces);

    // A park inherits its neighbour's machine identity, so a home leg never scores a fabricated tool or setup change.
    public static LinkStation Park(string key, Point3d point, LinkStation neighbour) => new(
        key, neighbour.ToolKey, neighbour.WorkOffset, point, point, Seq<Move>(), Seq<MotionDirective>(), 0.0, 0.0, 0);
}

[Union]
public abstract partial record LinkSegment {
    public sealed record Cutting(string Key, Seq<Move> Moves, Seq<MotionDirective> Directives) : LinkSegment;
    public sealed record Transiting(TransitionReceipt Receipt, Seq<Move> Moves) : LinkSegment;

    public Seq<Move> Emitted => Switch(
        cutting: static row => row.Moves,
        transiting: static row => row.Moves);
}

public sealed record LinkReceipt(
    Arr<string> Order,
    Seq<TransitionReceipt> Transitions,
    LinkMetric Total,
    double ObjectiveScore,
    int RejectedVariants,
    int GuardedMoves);

public sealed record Linked(Seq<LinkSegment> Segments, LinkReceipt Receipt) {
    public Seq<Move> Moves => Segments.Bind(static segment => segment.Emitted);
    public Seq<MotionDirective> Directives => Segments.Bind(static segment => segment is LinkSegment.Cutting cutting
        ? cutting.Directives
        : Seq<MotionDirective>());

    public SpecializedToolpathEnvelope Specialized => new(
        SpecializedToolpathKind.Link,
        Receipt.Transitions.Map(static row => (SpecializedToolpathRow)new SpecializedToolpathRow.Link(
            row.From, row.To, row.Kind.Key, row.Metric.DistanceMm, row.Metric.DurationSeconds,
            row.Metric.LiftMm, row.Metric.ThermalExposure, row.Metric.RotationPenalty,
            row.Metric.Retracts, row.Metric.Pierces, row.Metric.ToolChanges, row.Metric.SetupChanges)),
        Receipt.Transitions.Sum(static row => row.Metric.DurationSeconds));

    public MotionDirective SpecializedDirective => new MotionDirective.Specialized(
        Moves.IsEmpty ? -1 : Moves.Count - 1, Specialized);
    public PostSource PostingSource => new PostSource.Specialized(Specialized);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Link {
    public static Fin<TOut> Route<TOut>(LinkDemand? demand, Func<Linked, TOut> project) =>
        from _ in Optional(project).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:projection").ToError())
        from job in LinkJob.Admit(demand)
        from selected in SelectTour(job)
        from linked in Connect(job, selected)
        from projected in LinkJob.Invoke(() => Fin.Succ(project(linked)))
        select projected;

    private static Fin<BeamState> SelectTour(LinkJob job) =>
        from beam in Range(0, job.Elements.Count).FoldM<Fin, Seq<BeamState>>(
                Seq(new BeamState(
                    Seq<LinkStation>(),
                    Range(0, job.Elements.Count).ToSeq(),
                    Range(0, job.Elements.Count).Fold(
                        HashMap<int, int>.Empty,
                        (pending, index) => pending.Add(index, job.Precedence.Count(edge => edge.After == index))),
                    Option<LinkStation>.None,
                    Seq<(Seq<Move>, TransitionReceipt)>(),
                    0.0,
                    0)),
                (states, _) => Expand(job, states)).As()
        let closed = beam.Bind(state => Close(job, state).Map(Seq).IfFail(Seq<BeamState>()))
        from selected in closed.OrderBy(static state => state.Score).HeadOrNone()
            .ToFin(Blocked(job, beam))
        select selected;

    private readonly record struct BeamState(
        Seq<LinkStation> Rows,
        Seq<int> Remaining,
        HashMap<int, int> Pending,
        Option<LinkStation> Current,
        Seq<(Seq<Move> Moves, TransitionReceipt Receipt)> Legs,
        double Score,
        int Rejected);

    // Pending in-degrees decrement as elements are placed, so readiness is one map probe rather than a chosen-set rescan per edge.
    private static Fin<Seq<BeamState>> Expand(LinkJob job, Seq<BeamState> states) {
        int count = job.Elements.Count;
        int width = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));
        Seq<BeamState> candidates = states.Bind(state => {
            Seq<Fin<BeamState>> attempts = state.Remaining
                .Filter(candidate => state.Pending.Find(candidate).IfNone(0) == 0)
                .Bind(candidate => job.Elements[candidate].Variants.Map(variant => {
                    LinkStation station = LinkStation.Of(job.Elements[candidate], variant);
                    LinkStation from = state.Current.Match(
                        Some: static current => current,
                        None: () => LinkStation.Park("origin", job.Start, station));
                    return Transition(job, from, station).Map(leg => new BeamState(
                        state.Rows.Add(station),
                        state.Remaining.Filter(index => index != candidate),
                        job.Precedence
                            .Filter(edge => edge.Before == candidate)
                            .Fold(
                                state.Pending.Remove(candidate),
                                static (rows, edge) => rows.SetItem(edge.After, rows.Find(edge.After).IfNone(1) - 1)),
                        Some(station),
                        state.Legs.Add(leg),
                        state.Score + leg.Receipt.ObjectiveScore + job.Objective.Score(CuttingMetric(station, job.Policy)),
                        state.Rejected));
                }));
            return attempts.Bind(attempt => attempt
                .Map(next => Seq(next with { Rejected = next.Rejected + attempts.Count - 1 }))
                .IfFail(Seq<BeamState>()));
        });
        int pruned = Math.Max(0, candidates.Count - width);
        Seq<BeamState> next = candidates
            .OrderBy(static state => state.Score)
            .Take(width)
            .Map(state => state with { Rejected = state.Rejected + pruned })
            .ToSeq();
        return next.IsEmpty
            ? Fin.Fail<Seq<BeamState>>(Blocked(job, states))
            : Fin.Succ(next);
    }

    private static Fin<BeamState> Close(LinkJob job, BeamState state) =>
        state.Current.Match(
            Some: current => Transition(job, current, LinkStation.Park("return", job.Start, current)).Map(leg => state with {
                Legs = state.Legs.Add(leg),
                Score = state.Score + leg.Receipt.ObjectiveScore,
            }),
            None: () => Fin.Fail<BeamState>(Blocked(job, Seq(state))));

    // Stalled frontier names the blocked pair: the placed cursor and the first element precedence still withholds.
    private static Error Blocked(LinkJob job, Seq<BeamState> states) =>
        FabricationFault.LinkBlocked(
            states.HeadOrNone()
                .Bind(static state => state.Current)
                .Map(static station => station.Exit)
                .IfNone(job.Start),
            states.HeadOrNone()
                .Bind(static state => state.Remaining.HeadOrNone())
                .Map(index => job.Elements[index].Variants[0].Entry)
                .IfNone(job.Start)).ToError();

    private static Fin<Linked> Connect(LinkJob job, BeamState selected) =>
        from stations in Fin.Succ(selected.Rows.ToArr())
        from _ in !stations.IsEmpty && selected.Legs.Count == stations.Count + 1
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:empty-tour").ToError())
        from segments in toSeq(stations)
            .Map((station, index) => (Leg: selected.Legs[index], Station: station))
            .TraverseM(row =>
                from cut in Guard(job, row.Station.Moves)
                select Seq<LinkSegment>(
                    new LinkSegment.Transiting(row.Leg.Receipt, row.Leg.Moves),
                    new LinkSegment.Cutting(row.Station.Key, cut, row.Station.Directives))).As()
        let parked = selected.Legs[^1]
        let closed = segments.Bind(static row => row) + Seq<LinkSegment>(new LinkSegment.Transiting(parked.Receipt, parked.Moves))
        select Receipt(job.Policy, job.Objective, stations, selected.Legs.Map(static leg => leg.Receipt), closed, selected.Rejected);

    private static Fin<Seq<Move>> Guard(LinkJob job, Seq<Move> source) =>
        LinkJob.Invoke(() => job.Guard(source)).Bind(guarded => guarded.Count == source.Count
            && guarded.Zip(source).ForAll(static pair => pair.First == pair.Second)
                ? Fin.Succ(guarded)
                : Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "link:guard-rewrite").ToError()));

    private static Fin<(Seq<Move> Moves, TransitionReceipt Receipt)> Transition(LinkJob job, LinkStation from, LinkStation to) =>
        from route in Path(job, from.Exit, to.Entry)
        from _ in !route.Kind.RequiresPlane || route.Points.Count >= 3
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.LinkBlocked(from.Exit, to.Entry).ToError())
        from moves in LinkJob.Invoke(() => job.Lower(route.Points, route.Kind))
        from admitted in moves.TraverseM(Move.Admit).As()
        from guarded in Guard(job, admitted)
        let metric = Metric(route.Points, route.Kind, job.Policy,
            from.ToolKey != to.ToolKey, from.WorkOffset != to.WorkOffset)
        select (
            guarded,
            new TransitionReceipt(from.Key, to.Key, route.Kind, route.Points, metric, job.Objective.Score(metric), route.Components));

    private static Fin<(Seq<Point3d> Points, RetractKind Kind, int Components)> Path(LinkJob job, Point3d from, Point3d to) {
        double clearancePlane = Seq(from.Z, to.Z)
            .Concat(job.Keepouts.Bind(static row => row.Extent.Top.ToSeq()))
            .Max() + job.Policy.ClearanceMm;
        double skimZ = Math.Max(from.Z, to.Z) + job.Policy.SkimClearanceMm;
        Point3d skimFrom = new(from.X, from.Y, skimZ);
        Point3d skimTo = new(to.X, to.Y, skimZ);
        bool direct = Clear(from, to, job.Keepouts, job.Policy.ToleranceMm)
            && Math.Abs(to.Z - from.Z) <= job.Policy.RampRiseMm;
        bool skim = Clear(from, skimFrom, job.Keepouts, job.Policy.ToleranceMm)
            && Clear(skimFrom, skimTo, job.Keepouts, job.Policy.ToleranceMm)
            && Clear(skimTo, to, job.Keepouts, job.Policy.ToleranceMm);
        return (direct, skim) switch {
            (true, _) => Fin.Succ((Seq(from, to), from.Z == to.Z ? RetractKind.Direct : RetractKind.Ramp, 1)),
            (false, true) => Fin.Succ((Seq(from, skimFrom, skimTo, to), RetractKind.Skim, 1)),
            _ => Visibility(job, from, to, clearancePlane),
        };
    }

    private static Fin<(Seq<Point3d> Points, RetractKind Kind, int Components)> Visibility(
        LinkJob job,
        Point3d from,
        Point3d to,
        double plane) {
        Point3d liftedFrom = new(from.X, from.Y, plane);
        Point3d liftedTo = new(to.X, to.Y, plane);
        if (!Clear(from, liftedFrom, job.Keepouts, job.Policy.ToleranceMm)
            || !Clear(liftedTo, to, job.Keepouts, job.Policy.ToleranceMm))
            return Fin.Fail<(Seq<Point3d>, RetractKind, int)>(FabricationFault.LinkBlocked(from, to).ToError());
        // A bulged span contributes its arc apex beside its endpoints, so a routed corridor never chords through an arc wall.
        Seq<Point3d> vertices = Seq(liftedFrom, liftedTo)
            + job.Keepouts.Filter(static row => row.Extent is KeepoutExtent.Unbounded).Bind(row => row.Geometry.Bind(region =>
                region.Boundary.Vertices.Bind((_, index) => region.Boundary.BulgeAt(index) == 0.0
                    ? Seq(Corner(region.Boundary, index, plane, job.Policy.RoutedCornerClearanceMm))
                    : Seq(
                        Corner(region.Boundary, index, plane, job.Policy.RoutedCornerClearanceMm),
                        Apex(region.Boundary, index, plane, job.Policy.RoutedCornerClearanceMm)))));
        UndirectedGraph<int, SEdge<int>> reachability = new();
        AdjacencyGraph<int, STaggedEdge<int, double>> graph = new();
        reachability.AddVertexRange(Range(0, vertices.Count));
        graph.AddVertexRange(Range(0, vertices.Count));
        Seq<(int From, int To)> edges = Range(0, vertices.Count).ToSeq().Bind(i =>
            Range(i + 1, vertices.Count - i - 1).ToSeq().Map(j => (i, j)))
            .Filter(edge => Clear(vertices[edge.i], vertices[edge.j], job.Keepouts, job.Policy.ToleranceMm));
        reachability.AddEdgeRange(edges.Map(static edge => new SEdge<int>(edge.From, edge.To)));
        graph.AddEdgeRange(edges.Bind(edge => Seq(
            new STaggedEdge<int, double>(edge.From, edge.To, vertices[edge.From].DistanceTo(vertices[edge.To])),
            new STaggedEdge<int, double>(edge.To, edge.From, vertices[edge.From].DistanceTo(vertices[edge.To])))));
        Dictionary<int, int> components = [];
        int componentCount = reachability.ConnectedComponents(components);
        if (!components.TryGetValue(0, out int startComponent) || !components.TryGetValue(1, out int endComponent)
            || startComponent != endComponent)
            return Fin.Fail<(Seq<Point3d>, RetractKind, int)>(FabricationFault.LinkBlocked(from, to).ToError());
        TryFunc<int, IEnumerable<STaggedEdge<int, double>>> search = graph.ShortestPathsAStar(
            static edge => edge.Tag,
            vertex => vertices[vertex].DistanceTo(vertices[1]),
            0);
        if (!search(1, out IEnumerable<STaggedEdge<int, double>>? path))
            return Fin.Fail<(Seq<Point3d>, RetractKind, int)>(FabricationFault.LinkBlocked(from, to).ToError());
        Seq<STaggedEdge<int, double>> route = toSeq(path);
        RetractKind kind = route.Count > 1
            ? RetractKind.Routed
            : Math.Abs(plane - to.Z) > job.Policy.RampRiseMm
                ? RetractKind.ControlledDescent
                : RetractKind.FullLift;
        return Fin.Succ((
            Seq(from, liftedFrom) + route.Map(edge => vertices[edge.Target]) + Seq(to),
            kind,
            componentCount));
    }

    // Corridor AABB prunes each region's admitted flatbush index; a full segment walk per candidate is the rejected form.
    private static bool Clear(Point3d from, Point3d to, Arr<Keepout> keepouts, double tolerance) =>
        keepouts.Filter(row => row.Active(from.Z, to.Z)).ForAll(row => row.Geometry.ForAll(region =>
                !region.Boundary.Covers(new Point3d(from.X, from.Y, region.Boundary.Plane))
                && !region.Boundary.Covers(new Point3d(to.X, to.Y, region.Boundary.Plane))
                && region.Index.SpatialIndex
                    .Query(
                        Math.Min(from.X, to.X) - tolerance,
                        Math.Min(from.Y, to.Y) - tolerance,
                        Math.Max(from.X, to.X) + tolerance,
                        Math.Max(from.Y, to.Y) + tolerance)
                    .All(segment => PlineSegIntersection.Intersect(
                        PlineVertex<double>.FromSlice([from.X, from.Y, 0.0]),
                        PlineVertex<double>.FromSlice([to.X, to.Y, 0.0]),
                        region.Index.Polyline[segment],
                        region.Index.Polyline[region.Index.Polyline.NextWrappingIndex(segment)],
                        tolerance).Kind == PlineSegIntrKind.NoIntersect)));

    private static Point3d Corner(Loop loop, int index, double plane, double clearance) {
        Point3d previous = loop.At(index - 1);
        Point3d point = loop.At(index);
        Point3d next = loop.At(index + 1);
        Vector2d incoming = new(point.X - previous.X, point.Y - previous.Y);
        Vector2d outgoing = new(next.X - point.X, next.Y - point.Y);
        if (!incoming.Unitize() || !outgoing.Unitize())
            return new Point3d(point.X, point.Y, plane);
        Vector2d direction = loop.Winding() == Sign.Negative
            ? new Vector2d(-incoming.Y - outgoing.Y, incoming.X + outgoing.X)
            : new Vector2d(incoming.Y + outgoing.Y, -incoming.X - outgoing.X);
        return direction.Unitize()
            ? new Point3d(point.X + direction.X * clearance, point.Y + direction.Y * clearance, plane)
            : new Point3d(point.X, point.Y, plane);
    }

    // Exact arc midpoint pushed outward by routed clearance is the bulged span's extreme visibility witness.
    private static Point3d Apex(Loop loop, int index, double plane, double clearance) {
        Polyline<double> path = Native(loop);
        Vector2<double> mid = PlineSeg.SegMidpoint(path[index], path[path.NextWrappingIndex(index)]);
        Point3d start = loop.At(index);
        Point3d end = loop.At(index + 1);
        Vector2d outward = new(mid.X - 0.5 * (start.X + end.X), mid.Y - 0.5 * (start.Y + end.Y));
        return outward.Unitize()
            ? new Point3d(mid.X + outward.X * clearance, mid.Y + outward.Y * clearance, plane)
            : new Point3d(mid.X, mid.Y, plane);
    }

    private static Polyline<double> Native(Loop loop) => new(
        loop.Vertices.Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])),
        loop.Closed);

    private static LinkMetric Metric(
        Seq<Point3d> path,
        RetractKind kind,
        LinkPolicy policy,
        bool toolChange,
        bool setupChange) {
        (double Distance, double Horizontal, double Vertical) lengths = path.Zip(path.Skip(1)).Fold(
            (Distance: 0.0, Horizontal: 0.0, Vertical: 0.0),
            static (sum, pair) => {
                double distance = pair.First.DistanceTo(pair.Second);
                double vertical = Math.Abs(pair.Second.Z - pair.First.Z);
                return (
                    sum.Distance + distance,
                    sum.Horizontal + Math.Sqrt(Math.Max(0.0, distance * distance - vertical * vertical)),
                    sum.Vertical + vertical);
            });
        return new LinkMetric(
            lengths.Distance,
            60.0 * (lengths.Horizontal / policy.RapidMmPerMin + lengths.Vertical / policy.PlungeMmPerMin)
                + (toolChange ? policy.ToolChangeSeconds : 0.0)
                + (setupChange ? policy.SetupChangeSeconds : 0.0),
            lengths.Vertical,
            0.0,
            0.0,
            kind.Retracts,
            0,
            toolChange ? 1 : 0,
            setupChange ? 1 : 0);
    }

    private static LinkMetric CuttingMetric(LinkStation station, LinkPolicy policy) {
        (Point3d Cursor, double Distance, double Duration) walked = station.Moves.Fold(
            (Cursor: station.Entry, Distance: 0.0, Duration: 0.0),
            (state, move) => {
                double distance = MoveDistance(state.Cursor, move);
                double feed = move.Switch(
                    rapid: _ => policy.RapidMmPerMin,
                    linear: static row => row.Feed,
                    circular: static row => row.Feed);
                return (Target(move), state.Distance + distance, state.Duration + 60.0 * distance / feed);
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

    private static double MoveDistance(Point3d from, Move move) => move.Switch(
        state: from,
        rapid: static (start, row) => start.DistanceTo(row.Target),
        linear: static (start, row) => start.DistanceTo(row.Target),
        circular: static (start, row) => Radius(row) * Sweep(start, row.Target, row.Arc));

    private static double Radius(Move.Circular move) {
        Vector3d radius = move.Target - move.Arc.Center;
        radius.Z = 0.0;
        return radius.Length;
    }

    private static double Sweep(Point3d from, Point3d to, ArcCenter arc) {
        Vector3d start = from - arc.Center;
        Vector3d end = to - arc.Center;
        start.Z = 0.0;
        end.Z = 0.0;
        double minor = Vector3d.VectorAngle(start, end);
        double cross = Vector3d.CrossProduct(start, end).Z;
        bool counterclockwise = arc.Sense == RotationSense.Counterclockwise;
        return counterclockwise == cross >= 0.0 ? minor : 2.0 * Math.PI - minor;
    }

    private static Point3d Target(Move move) => move.Switch(
        rapid: static row => row.Target,
        linear: static row => row.Target,
        circular: static row => row.Target);

    private static Linked Receipt(
        LinkPolicy policy,
        LinkObjective objective,
        Arr<LinkStation> stations,
        Seq<TransitionReceipt> transitions,
        Seq<LinkSegment> segments,
        int rejected) {
        LinkMetric cutting = stations.Fold(new LinkMetric(), (sum, row) => sum + CuttingMetric(row, policy));
        return new(segments, new LinkReceipt(
            stations.Map(static row => row.Key),
            transitions,
            transitions.Fold(cutting, static (sum, row) => sum + row.Metric),
            transitions.Sum(static row => row.ObjectiveScore) + objective.Score(cutting),
            rejected,
            segments.Fold(0, static (count, segment) => count + segment.Emitted.Count)));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
