# [RASM_FABRICATION_PROBING]

`Probe.Inspect` owns post-cycle metrology truth: one admitted `InspectPolicy` generates feature-complete contact targets, correlates exact controller cycles with repeat observations, compensates calibrated stylus behavior, reconciles datum registration, and projects transformed residuals onto `FabricationResult.InspectionResult`.

`FabricationPolicy.Inspect`, `GCommand`, `DatumReceipt`, `FitReceipt`, `Capability.Assess`, and `InspectionFeature` remain frozen seams. Decoded measurement rows enter as typed data; controller transport and work-offset mutation remain outside the Verify plane.

## [01]-[INDEX]

- [01]-[FEATURE_SPACE]: generated analytic, toroidal, profile, and sampled-surface contact targets under one metrology spec row per case.
- [02]-[OBSERVATION_RAIL]: exact cycle-addressed ingress, temporal containment, per-contact outcome evidence, and calibration compensation.
- [03]-[DATUM_AND_RESULT]: registration before residuals, primitive-fit evidence, capability projection, and atoms-safe egress.

## [02]-[FEATURE_SPACE]

- Owner: `ProbeFeature` closes the inspection geometry family as pure nominal geometry, while `ProbePlan` owns the inspection demand — feature key, tolerance band, `ProbeCycle`, sample count, attempts, feed, clearance, travel, and lateral approach tolerance.
- Cases: analytic feature cases generate their contact space from dimensional data; `Surface` delegates distribution to the admitted `SampleKind.Project` rail over one `ExtractionDomain`.
- Entry: `Probe.Inspect` is the sole public operation; each generated `ProbeTarget` carries the exact `ProbeTargetKey` whose one `Text` spelling posting, telemetry, residuals, and result identity all read.
- Auto: `FeatureSpec` carries contact cardinality, primitive-fit kind, and the wall-contact axis as one row per case, so `Bore` and `Boss` separate cylinder-fit wall contacts from axial caps and every sample floor derives from the solver's own `FitKind.MinimalSamples`; one `Validation<Error, Unit>` fan-in proves feature coverage, target uniqueness, observation references, evidence identity, and datum traceability.
- Growth: a feature sub-kind is one `ProbeFeature` case, one `FeatureSpec` row, and one contact generator; no feature-specific inspection entrypoint survives beside it.

## [03]-[OBSERVATION_RAIL]

- Owner: `MeasurementSource` closes telemetry, manual, and fixture-synthetic ingress with one `Interval`, evidence key, and observation sequence per case.
- Cases: `ProbeCycle` rows retain exact `G31`, `G38.2`, `G38.3`, `G38.4`, and `G38.5` semantics, their posted `GCommand`, and the approach direction they orient; `ProbeOutcome` closes contact, optional miss, and rejection so a hit always carries its observation and compensated point.
- Auto: `Interval.Contains` gates source and calibration time; `ProbeAddress` retains cycle, feature, sample, and attempt, and correlation runs through one keyed index so contact count never drives quadratic scanning.
- Receipt: robust median/MAD aggregation rejects contact outliers, then combined uncertainty conserves calibration, thermal, lobing, and repeatability contributions.
- Boundary: observations carry ball centers; axial travel, lateral approach, and thermal-scale rejection stay on the affected touch, and the aggregate required-hit verdict runs after every target retains its outcomes. Stylus radius and lobing add along the approach while pre-travel subtracts, and inverse thermal scaling restores reference-temperature geometry.

## [04]-[DATUM_AND_RESULT]

- Owner: `DatumPolicy` closes assigned transform, best-fit registration, and primitive substitution over the current `DatumReceipt` wire.
- Auto: `AlignKind.AlignDetailed` projects a transform only through `AlignmentReceipt.Project<Transform>`; `Fit.Apply` retains per-feature and datum-substitution `FitReceipt` evidence, and a group thinned below its kind's `MinimalSamples` carries no fit rather than a fabricated one; transformed measured points precede every `ResidualSample`.
- Receipt: `ProbeReport` closes the pre-egress evidence fold, while the frozen `InspectionResult` projects only `InspectionFeature` atoms and drops cycle, datum, fit, capability, source, and time evidence. `Probe.Inspect` mints `FabricationFact.Probe` beside the frozen result — conformance counts and worst deviation onto `rasm.fabrication.probe.features` and `rasm.fabrication.probe.deviation` through `Process/telemetry#FACT_PROJECTION` as kind `probe` — because `ProbeReport` is file-scoped and the fact is its one telemetry projection; the fact fires through the `FabricationTap` the run spine passes, defaulting silent for headless callers, and the whole fold runs inside the `EngineSpan.ProbeFit` span with datum registration and feature fitting as phase events; the settled datum alignment fires the `FabricationFact.Engine.Of` ICP-iteration row through the same tap, so probe-fit cost attribution rides the engine census with zero kernel writes.
- Boundary: `Capability.Assess(new CapabilityStudy.Variables(...), tolerance)` consumes the residual tranche; no local QIF-shaped record claims a standard contract the package does not admit.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Numerics;
using Rasm.Processing;
using Rasm.Solving;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ProbeCycle {
    public static readonly ProbeCycle G31 = new("g31", GCommand.Probe, requiresHit: false, towardSurface: true);
    public static readonly ProbeCycle G38TowardRequired = new("g38.2", GCommand.ProbeTowardStop, requiresHit: true, towardSurface: true);
    public static readonly ProbeCycle G38TowardOptional = new("g38.3", GCommand.ProbeTowardOptional, requiresHit: false, towardSurface: true);
    public static readonly ProbeCycle G38AwayRequired = new("g38.4", GCommand.ProbeAwayStop, requiresHit: true, towardSurface: false);
    public static readonly ProbeCycle G38AwayOptional = new("g38.5", GCommand.ProbeAwayOptional, requiresHit: false, towardSurface: false);

    public GCommand Command { get; }
    public bool RequiresHit { get; }
    public bool TowardSurface { get; }

    public Vector3d Approach(Vector3d outward) => TowardSurface ? -outward : outward;
}

[SmartEnum<string>]
public sealed partial class ProbeSense {
    public static readonly ProbeSense Outside = new("outside", 1.0);
    public static readonly ProbeSense Inside = new("inside", -1.0);

    public double Sign { get; }

    public Vector3d Orient(Vector3d normal) => normal * Sign;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProbeFeature {
    private const double GoldenConjugate = 0.6180339887498949;

    private ProbeFeature() { }

    public sealed record Point(Point3d Nominal, Vector3d Normal) : ProbeFeature;
    public sealed record Line(Rhino.Geometry.Line Nominal, Vector3d Normal) : ProbeFeature;
    public sealed record Plane(Rhino.Geometry.Plane Frame, double WidthMm, double HeightMm) : ProbeFeature;
    public sealed record Circle(Rhino.Geometry.Plane Frame, double RadiusMm) : ProbeFeature;
    public sealed record Bore(Rhino.Geometry.Plane Frame, double DiameterMm, double DepthMm) : ProbeFeature;
    public sealed record Boss(Rhino.Geometry.Plane Frame, double DiameterMm, double HeightMm) : ProbeFeature;
    public sealed record Slot(Rhino.Geometry.Plane Frame, double LengthMm, double WidthMm, double DepthMm) : ProbeFeature;
    public sealed record Web(Rhino.Geometry.Plane Frame, double LengthMm, double HeightMm, double ThicknessMm) : ProbeFeature;
    public sealed record Sphere(Point3d Center, double RadiusMm) : ProbeFeature;
    public sealed record Cylinder(Rhino.Geometry.Plane Frame, double RadiusMm, double HeightMm, ProbeSense Sense) : ProbeFeature;
    public sealed record Cone(Rhino.Geometry.Plane Frame, double BaseRadiusMm, double HeightMm, ProbeSense Sense) : ProbeFeature;
    public sealed record Torus(Rhino.Geometry.Plane Frame, double MajorRadiusMm, double MinorRadiusMm, ProbeSense Sense) : ProbeFeature;
    public sealed record Profile(Seq<FeatureSample> Samples) : ProbeFeature;
    public sealed record Surface(ExtractionDomain Domain, SampleKind Sampling, Vector3d Normal) : ProbeFeature;

    // Contact cardinality, primitive-fit eligibility, and the axis a wall contact must be perpendicular to
    // are one per-case row, so a new feature declares its whole metrology contract in one arm and every
    // consumer derives; the sample floors read the solver's own `MinimalSamples` rather than restating it.
    internal FeatureSpec Spec => Switch(
        point: static _ => new FeatureSpec(1, 1, None, None),
        line: static _ => new FeatureSpec(FitKind.Line.MinimalSamples, int.MaxValue, Some(FitKind.Line), None),
        plane: static _ => new FeatureSpec(FitKind.Plane.MinimalSamples, int.MaxValue, Some(FitKind.Plane), None),
        circle: static _ => new FeatureSpec(3, int.MaxValue, None, None),
        bore: static row => new FeatureSpec(FitKind.Cylinder.MinimalSamples + 1, int.MaxValue, Some(FitKind.Cylinder), Some(row.Frame.ZAxis)),
        boss: static row => new FeatureSpec(FitKind.Cylinder.MinimalSamples + 1, int.MaxValue, Some(FitKind.Cylinder), Some(row.Frame.ZAxis)),
        slot: static _ => new FeatureSpec(5, int.MaxValue, None, None),
        web: static _ => new FeatureSpec(4, int.MaxValue, None, None),
        sphere: static _ => new FeatureSpec(FitKind.Sphere.MinimalSamples, int.MaxValue, Some(FitKind.Sphere), None),
        cylinder: static _ => new FeatureSpec(FitKind.Cylinder.MinimalSamples, int.MaxValue, Some(FitKind.Cylinder), None),
        cone: static _ => new FeatureSpec(FitKind.Cone.MinimalSamples, int.MaxValue, Some(FitKind.Cone), None),
        torus: static _ => new FeatureSpec(FitKind.Torus.MinimalSamples, int.MaxValue, Some(FitKind.Torus), None),
        profile: static row => new FeatureSpec(Math.Min(row.Samples.Count, 2), int.MaxValue, None, None),
        surface: static _ => new FeatureSpec(3, int.MaxValue, None, None));

    internal bool Admits(int count) => count >= Spec.Minimum && count <= Spec.Maximum;

    internal bool FitEligible(Vector3d normal) => Spec.FitAxis
        .Map(axis => Math.Abs(Unit(normal) * axis) <= Math.Sqrt(double.Epsilon))
        .IfNone(true);

    internal bool Valid => Switch(
        point: static row => row.Nominal.IsValid && Direction(row.Normal).IsSome,
        line: static row => row.Nominal.IsValid && Direction(row.Normal).IsSome,
        plane: static row => row.Frame.IsValid && Positive(row.WidthMm, row.HeightMm),
        circle: static row => row.Frame.IsValid && Positive(row.RadiusMm),
        bore: static row => row.Frame.IsValid && Positive(row.DiameterMm, row.DepthMm),
        boss: static row => row.Frame.IsValid && Positive(row.DiameterMm, row.HeightMm),
        slot: static row => row.Frame.IsValid && Positive(row.LengthMm, row.WidthMm, row.DepthMm) && row.LengthMm > row.WidthMm,
        web: static row => row.Frame.IsValid && Positive(row.LengthMm, row.HeightMm, row.ThicknessMm),
        sphere: static row => row.Center.IsValid && Positive(row.RadiusMm),
        cylinder: static row => row.Frame.IsValid && Positive(row.RadiusMm, row.HeightMm) && row.Sense is not null,
        cone: static row => row.Frame.IsValid && Positive(row.BaseRadiusMm, row.HeightMm) && row.Sense is not null,
        torus: static row => row.Frame.IsValid && Positive(row.MajorRadiusMm, row.MinorRadiusMm)
            && row.MajorRadiusMm > row.MinorRadiusMm && row.Sense is not null,
        profile: static row => row.Samples is not null && row.Samples.Count >= 2
            && row.Samples.ForAll(static sample => sample.Nominal.IsValid && Direction(sample.SurfaceNormal).IsSome)
            && PolylineLength(row.Samples.Map(static sample => sample.Nominal)) > 0.0,
        surface: static row => row.Domain is not null && row.Sampling is not null && Direction(row.Normal).IsSome);

    internal Fin<Seq<FeatureSample>> Project(int count, Context context) => Switch(
        state: (Count: count, Context: context),
        point: static (state, row) => Fin.Succ(Repeat(row.Nominal, row.Normal, state.Count)),
        line: static (state, row) => Fin.Succ(Linear(row.Nominal, row.Normal, state.Count)),
        plane: static (state, row) => Fin.Succ(Grid(row.Frame, row.WidthMm, row.HeightMm, state.Count)),
        circle: static (state, row) => Fin.Succ(Ring(row.Frame, row.RadiusMm, 0.0, state.Count, ProbeSense.Outside)),
        bore: static (state, row) => Fin.Succ(Capped(
            row.Frame, row.DepthMm, state.Count, ProbeSense.Inside, row.DiameterMm * 0.5, -row.Frame.ZAxis)),
        boss: static (state, row) => Fin.Succ(Capped(
            row.Frame, row.HeightMm, state.Count, ProbeSense.Outside, row.DiameterMm * 0.5, row.Frame.ZAxis)),
        slot: static (state, row) => Fin.Succ(SlotFaces(row.Frame, row.LengthMm, row.WidthMm, row.DepthMm, state.Count)),
        web: static (state, row) => Fin.Succ(WebFaces(row.Frame, row.LengthMm, row.HeightMm, row.ThicknessMm, state.Count)),
        sphere: static (state, row) => Fin.Succ(Spherical(row.Center, row.RadiusMm, state.Count)),
        cylinder: static (state, row) => Fin.Succ(Layered(
            row.Frame, row.HeightMm, state.Count, row.Sense, _ => row.RadiusMm, static radial => radial)),
        cone: static (state, row) => Fin.Succ(Layered(
            row.Frame,
            row.HeightMm,
            state.Count,
            row.Sense,
            fraction => row.BaseRadiusMm * (1.0 - fraction),
            radial => Unit(radial + (row.Frame.ZAxis * (row.BaseRadiusMm / row.HeightMm))))),
        torus: static (state, row) => Fin.Succ(Toroidal(
            row.Frame, row.MajorRadiusMm, row.MinorRadiusMm, state.Count, row.Sense)),
        profile: static (state, row) => Fin.Succ(Resample(row.Samples, state.Count)),
        surface: static (state, row) => row.Sampling.Project<Seq<Point3d>>(row.Domain, state.Context)
            .Map(points => points.Take(state.Count).Map(point => new FeatureSample(point, Unit(row.Normal))).ToSeq()));

    private static Seq<FeatureSample> Repeat(Point3d point, Vector3d normal, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(_ => new FeatureSample(point, Unit(normal)));

    private static Seq<FeatureSample> Linear(Rhino.Geometry.Line line, Vector3d normal, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index =>
            new FeatureSample(line.PointAt(count == 1 ? 0.5 : (double)index / (count - 1)), Unit(normal)));

    private static Seq<FeatureSample> Grid(Rhino.Geometry.Plane frame, double width, double height, int count) {
        int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));
        int rows = Math.Max(1, (int)Math.Ceiling((double)count / columns));
        return toSeq(Enumerable.Range(0, count)).Map(index => {
            double u = columns == 1 ? 0.0 : ((double)(index % columns) / (columns - 1)) - 0.5;
            double v = rows == 1 ? 0.0 : ((double)(index / columns) / (rows - 1)) - 0.5;
            return new FeatureSample(frame.Origin + (frame.XAxis * (u * width)) + (frame.YAxis * (v * height)), frame.ZAxis);
        });
    }

    private static Seq<FeatureSample> Ring(Rhino.Geometry.Plane frame, double radius, double z, int count, ProbeSense sense) =>
        toSeq(Enumerable.Range(0, count)).Map(index => {
            double angle = Math.Tau * index / count;
            Vector3d radial = (frame.XAxis * Math.Cos(angle)) + (frame.YAxis * Math.Sin(angle));
            return new FeatureSample(frame.Origin + (frame.ZAxis * z) + (radial * radius), sense.Orient(radial));
        });

    private static Seq<FeatureSample> Layered(
        Rhino.Geometry.Plane frame,
        double height,
        int count,
        ProbeSense sense,
        Func<double, double> radiusAt,
        Func<Vector3d, Vector3d> normalAt) =>
        toSeq(Enumerable.Range(0, count)).Map(index => {
            double fraction = count == 1 ? 0.5 : (double)index / (count - 1);
            double angle = Math.Tau * index * GoldenConjugate;
            double radius = radiusAt(fraction);
            Vector3d radial = (frame.XAxis * Math.Cos(angle)) + (frame.YAxis * Math.Sin(angle));
            return new FeatureSample(
                frame.Origin + (frame.ZAxis * (height * fraction)) + (radial * radius),
                sense.Orient(normalAt(radial)));
        });

    private static Seq<FeatureSample> Capped(
        Rhino.Geometry.Plane frame,
        double height,
        int count,
        ProbeSense sense,
        double radius,
        Vector3d capNormal) {
        double capArea = Math.PI * radius * radius;
        double wallArea = Math.Tau * radius * height;
        int capCount = Math.Clamp(
            (int)Math.Round(count * capArea / (capArea + wallArea)),
            1,
            count - FitKind.Cylinder.MinimalSamples);
        int wallCount = count - capCount;
        Seq<FeatureSample> cap = toSeq(Enumerable.Range(0, capCount)).Map(index => {
            double radialFraction = Math.Sqrt((index + 0.5) / capCount);
            double angle = Math.Tau * index * GoldenConjugate;
            Vector3d radial = (frame.XAxis * Math.Cos(angle)) + (frame.YAxis * Math.Sin(angle));
            return new FeatureSample(
                frame.Origin + (frame.ZAxis * height) + (radial * (radius * radialFraction)),
                capNormal);
        });
        return Layered(frame, height, wallCount, sense, _ => radius, static radial => radial) + cap;
    }

    private static Seq<FeatureSample> SlotFaces(Rhino.Geometry.Plane frame, double length, double width, double depth, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index => {
            double bottomArea = length * width;
            double sideArea = 2.0 * (length + width) * depth;
            int bottomCount = Math.Clamp((int)Math.Round(count * bottomArea / (bottomArea + sideArea)), 1, count - 4);
            int sideCount = count - bottomCount;
            if (index >= sideCount) {
                int at = index - sideCount;
                int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(bottomCount)));
                int rows = Math.Max(1, (int)Math.Ceiling((double)bottomCount / columns));
                double u = columns == 1 ? 0.0 : ((double)(at % columns) / (columns - 1)) - 0.5;
                double v = rows == 1 ? 0.0 : ((double)(at / columns) / (rows - 1)) - 0.5;
                return new FeatureSample(
                    frame.Origin + (frame.XAxis * (u * length)) + (frame.YAxis * (v * width)) - (frame.ZAxis * depth),
                    frame.ZAxis);
            }
            double perimeter = 2.0 * (length + width);
            double distance = perimeter * index / sideCount;
            (double X, double Y, Vector3d Normal) row = distance switch {
                var at when at < length => (-length * 0.5 + at, -width * 0.5, frame.YAxis),
                var at when at < length + width => (length * 0.5, -width * 0.5 + (at - length), -frame.XAxis),
                var at when at < (2.0 * length) + width => (length * 0.5 - (at - length - width), width * 0.5, -frame.YAxis),
                var at => (-length * 0.5, width * 0.5 - (at - (2.0 * length) - width), frame.XAxis),
            };
            double depthFraction = index switch {
                0 => 0.0,
                _ when index == sideCount - 1 => 1.0,
                _ => (index * GoldenConjugate) % 1.0,
            };
            return new FeatureSample(
                frame.Origin + (frame.XAxis * row.X) + (frame.YAxis * row.Y) - (frame.ZAxis * (depth * depthFraction)),
                row.Normal);
        });

    private static Seq<FeatureSample> WebFaces(
        Rhino.Geometry.Plane frame,
        double length,
        double height,
        double thickness,
        int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index => {
            int perFace = Math.Max(1, (int)Math.Ceiling(count * 0.5));
            int at = index / 2;
            int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(perFace)));
            int rows = Math.Max(1, (int)Math.Ceiling((double)perFace / columns));
            double u = columns == 1 ? 0.0 : ((double)(at % columns) / (columns - 1)) - 0.5;
            double v = rows == 1 ? 0.0 : ((double)(at / columns) / (rows - 1)) - 0.5;
            bool positive = index % 2 == 0;
            return new FeatureSample(
                frame.Origin + (frame.XAxis * (u * length)) + (frame.YAxis * (v * height)) + (frame.ZAxis * (positive ? thickness * 0.5 : -thickness * 0.5)),
                positive ? frame.ZAxis : -frame.ZAxis);
        });

    // Golden-angle on the tube against a uniform major sweep spreads contacts over the whole torus rather
    // than banding them on one meridian, so a groove's form error is sampled, not aliased.
    private static Seq<FeatureSample> Toroidal(
        Rhino.Geometry.Plane frame,
        double major,
        double minor,
        int count,
        ProbeSense sense) =>
        toSeq(Enumerable.Range(0, count)).Map(index => {
            double sweep = Math.Tau * (index + 0.5) / count;
            double tube = Math.Tau * index * GoldenConjugate;
            Vector3d radial = (frame.XAxis * Math.Cos(sweep)) + (frame.YAxis * Math.Sin(sweep));
            Vector3d normal = (radial * Math.Cos(tube)) + (frame.ZAxis * Math.Sin(tube));
            return new FeatureSample(frame.Origin + (radial * major) + (normal * minor), sense.Orient(normal));
        });

    private static Seq<FeatureSample> Spherical(Point3d center, double radius, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index => {
            double z = 1.0 - (2.0 * (index + 0.5) / count);
            double azimuth = Math.Tau * index * GoldenConjugate;
            double radial = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z)));
            Vector3d normal = new(radial * Math.Cos(azimuth), radial * Math.Sin(azimuth), z);
            return new FeatureSample(center + (normal * radius), normal);
        });

    private static Seq<FeatureSample> Resample(Seq<FeatureSample> samples, int count) {
        Seq<(FeatureSample From, FeatureSample To, double Length)> segments = toSeq(samples.AsIterable()
            .Zip(samples.AsIterable().Skip(1), static (from, to) => (from, to, from.Nominal.DistanceTo(to.Nominal))));
        double length = segments.Map(static row => row.Length).Sum();
        return toSeq(Enumerable.Range(0, count)).Map(index => {
            double distance = count == 1 ? length * 0.5 : length * index / (count - 1);
            FeatureSample sample = segments.Fold(
                (Remaining: distance, Sample: samples[0]),
                static (state, segment) => state.Remaining <= 0.0
                    ? state
                    : state.Remaining <= segment.Length
                        ? (0.0, new FeatureSample(
                            segment.From.Nominal + ((segment.To.Nominal - segment.From.Nominal) * (state.Remaining / Math.Max(segment.Length, double.Epsilon))),
                            Unit(segment.From.SurfaceNormal + ((segment.To.SurfaceNormal - segment.From.SurfaceNormal)
                                * (state.Remaining / Math.Max(segment.Length, double.Epsilon))))))
                        : (state.Remaining - segment.Length, segment.To))
                .Sample;
            return sample;
        });
    }

    private static double PolylineLength(Seq<Point3d> points) => points.AsIterable()
        .Zip(points.AsIterable().Skip(1), static (from, to) => from.DistanceTo(to))
        .Sum();

    private static Option<Vector3d> Direction(Vector3d value) {
        Vector3d copy = value;
        return copy.IsValid && copy.Unitize() ? Some(copy) : None;
    }

    private static bool Positive(params double[] values) =>
        values.All(static value => double.IsFinite(value) && value > 0.0);

    private static Vector3d Unit(Vector3d value) {
        _ = value.Unitize();
        return value;
    }
}

[ComplexValueObject]
public sealed partial class ProbeTargetKey {
    public ProbeCycle Cycle { get; }
    public int Feature { get; }
    public int Sample { get; }

    // One spelling serves posting, telemetry correlation, residual rows, and egress identity; a caller
    // re-joining the three parts at its own call site forks the wire token.
    public string Text => $"{Cycle.Key}:{Feature}:{Sample}";

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProbeCycle cycle,
        ref int feature,
        ref int sample) =>
        validationError = cycle is not null && feature >= 0 && sample >= 0
            ? null
            : new ValidationError("probe:target-key");
}

[ComplexValueObject]
public sealed partial class ProbeAddress {
    public ProbeTargetKey Target { get; }
    public int Attempt { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProbeTargetKey target,
        ref int attempt) =>
        validationError = target is not null && attempt >= 0
            ? null
            : new ValidationError("probe:address");
}

// Identity and tolerance band are inspection demands, not geometry: two plans may probe the same nominal
// circle under different bands, so the key and the band ride the plan and the feature stays pure geometry.
[ComplexValueObject]
public sealed partial class ProbePlan {
    public int Key { get; }
    public ProbeFeature Feature { get; }
    public ProbeCycle Cycle { get; }
    public double ToleranceMm { get; }
    public int Count { get; }
    public int Attempts { get; }
    public double FeedMmPerMinute { get; }
    public double ClearanceMm { get; }
    public double TravelLimitMm { get; }
    public double ApproachToleranceMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int key,
        ref ProbeFeature feature,
        ref ProbeCycle cycle,
        ref double toleranceMm,
        ref int count,
        ref int attempts,
        ref double feedMmPerMinute,
        ref double clearanceMm,
        ref double travelLimitMm,
        ref double approachToleranceMm) =>
        validationError = key >= 0 && feature is not null && feature.Valid && feature.Admits(count)
            && cycle is not null && attempts > 0
            && double.IsFinite(toleranceMm) && toleranceMm > 0.0
            && double.IsFinite(feedMmPerMinute) && feedMmPerMinute > 0.0
            && double.IsFinite(clearanceMm) && clearanceMm >= 0.0
            && double.IsFinite(travelLimitMm) && travelLimitMm > clearanceMm
            && double.IsFinite(approachToleranceMm) && approachToleranceMm > 0.0
                ? null
                : new ValidationError("probe:plan");
}

[ComplexValueObject]
public sealed partial class ProbeObservation {
    public ProbeAddress Address { get; }
    public Point3d BallCenter { get; }
    public Instant At { get; }
    public double TemperatureC { get; }
    public UInt128 EvidenceKey { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProbeAddress address,
        ref Point3d ballCenter,
        ref Instant at,
        ref double temperatureC,
        ref UInt128 evidenceKey) =>
        validationError = address is not null && ballCenter.IsValid && at != default && double.IsFinite(temperatureC) && evidenceKey != 0
            ? null
            : new ValidationError("probe:observation");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeasurementSource {
    private MeasurementSource() { }

    public sealed record Telemetry(Interval Span, Seq<ProbeObservation> Observations, UInt128 Evidence) : MeasurementSource;
    public sealed record Manual(Interval Span, Seq<ProbeObservation> Observations, UInt128 Evidence) : MeasurementSource;
    public sealed record FixtureSynthetic(Interval Span, Seq<ProbeObservation> Observations, UInt128 Evidence) : MeasurementSource;

    private (Interval Window, Seq<ProbeObservation> Rows, UInt128 EvidenceKey) Payload => Switch(
        telemetry: static row => (row.Span, row.Observations, row.Evidence),
        manual: static row => (row.Span, row.Observations, row.Evidence),
        fixtureSynthetic: static row => (row.Span, row.Observations, row.Evidence));

    public Interval Window => Payload.Window;
    public Seq<ProbeObservation> Rows => Payload.Rows;
    public UInt128 EvidenceKey => Payload.EvidenceKey;

    internal bool Valid => Window is not null && Rows is not null && EvidenceKey != 0
        && Rows.ForAll(static row => row is not null && row.EvidenceKey != 0);
}

public readonly record struct ProbeLobe(int Harmonic, double AmplitudeMm, double PhaseRadians);

[ComplexValueObject]
public sealed partial class StylusCalibration {
    public UInt128 Key { get; }
    public double RadiusMm { get; }
    public double PreTravelMm { get; }
    public double ThermalExpansionPerC { get; }
    public double ReferenceTemperatureC { get; }
    public Point3d ThermalReference { get; }
    public double CalibrationUncertaintyMm { get; }
    public Interval Validity { get; }
    public Seq<ProbeLobe> Lobes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UInt128 key,
        ref double radiusMm,
        ref double preTravelMm,
        ref double thermalExpansionPerC,
        ref double referenceTemperatureC,
        ref Point3d thermalReference,
        ref double calibrationUncertaintyMm,
        ref Interval validity,
        ref Seq<ProbeLobe> lobes) =>
        validationError = key != 0 && radiusMm > 0.0 && preTravelMm >= 0.0
            && Seq(radiusMm, preTravelMm, thermalExpansionPerC, referenceTemperatureC, calibrationUncertaintyMm).ForAll(double.IsFinite)
            && thermalReference.IsValid && calibrationUncertaintyMm >= 0.0 && validity is not null
            && validity.HasStart && validity.HasEnd && validity.End > validity.Start
            && lobes.ForAll(static row => row.Harmonic > 0 && double.IsFinite(row.AmplitudeMm) && double.IsFinite(row.PhaseRadians))
            && lobes.Map(static row => row.Harmonic).Distinct().Count == lobes.Count
                ? null
                : new ValidationError("probe:calibration");
}

[ComplexValueObject]
public sealed partial class RepeatPolicy {
    public int MinimumAccepted { get; }
    public double OutlierSigma { get; }
    public double MinimumUncertaintyMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimumAccepted,
        ref double outlierSigma,
        ref double minimumUncertaintyMm) =>
        validationError = minimumAccepted > 0 && double.IsFinite(outlierSigma) && outlierSigma > 0.0
            && double.IsFinite(minimumUncertaintyMm) && minimumUncertaintyMm >= 0.0
                ? null
                : new ValidationError("probe:repeat-policy");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DatumPolicy {
    private DatumPolicy() { }

    public sealed record Setup(DatumReceipt Datum, Transform Registration) : DatumPolicy;
    public sealed record BestFit(DatumReceipt Datum, AlignKind Kind, AlignmentPolicy Policy) : DatumPolicy;
    public sealed record Substitute(
        DatumReceipt Datum,
        Seq<FitKind> Kinds,
        FitPolicy FitPolicy,
        AlignKind Registration,
        AlignmentPolicy Alignment) : DatumPolicy;

    public DatumReceipt Receipt => Switch(
        setup: static row => row.Datum,
        bestFit: static row => row.Datum,
        substitute: static row => row.Datum);
}

[ComplexValueObject]
public sealed partial class InspectPolicy {
    public Seq<ProbePlan> Plans { get; }
    public MeasurementSource Source { get; }
    public DatumPolicy Datum { get; }
    public StylusCalibration Calibration { get; }
    public RepeatPolicy Repeat { get; }
    public FitPolicy FeatureFit { get; }
    public Option<CapabilityTolerance> Capability { get; }
    public IClock Clock { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<ProbePlan> plans,
        ref MeasurementSource source,
        ref DatumPolicy datum,
        ref StylusCalibration calibration,
        ref RepeatPolicy repeat,
        ref FitPolicy featureFit,
        ref Option<CapabilityTolerance> capability,
        ref IClock clock) =>
        validationError = !plans.IsEmpty && plans.ForAll(static row => row is not null)
            && source is { Valid: true } && datum is not null && calibration is not null && repeat is not null && featureFit is not null
            && capability.ForAll(static row => row is not null) && clock is not null
                ? null
                : new ValidationError("probe:policy");
}

public readonly record struct FeatureSample(Point3d Nominal, Vector3d SurfaceNormal);
public readonly record struct FeatureSpec(int Minimum, int Maximum, Option<FitKind> Fit, Option<Vector3d> FitAxis);
public sealed record MeasuredFeature(
    ProbeTargetKey Key,
    ProbePlan Plan,
    Point3d Nominal,
    Point3d Measured,
    Vector3d SurfaceNormal,
    ResidualSample Residual,
    double UncertaintyMm,
    double RepeatabilityMm,
    Instant At,
    Option<FitReceipt> Fit);

file sealed record ProbeTarget(
    ProbeTargetKey Key,
    ProbePlan Plan,
    Point3d Nominal,
    Vector3d SurfaceNormal,
    Vector3d Direction,
    Point3d Start,
    Point3d End) {
    public GNode Node => new GNode.Word(
        Plan.Cycle.Command,
        Arr(
            GParam.Number('X', End.X, ProgramUnits.Metric),
            GParam.Number('Y', End.Y, ProgramUnits.Metric),
            GParam.Number('Z', End.Z, ProgramUnits.Metric),
            GParam.Number('F', Plan.FeedMmPerMinute, ProgramUnits.Metric)),
        None);
}

// Every rejection row carries the fault a target raises when no attempt survives, so a new rejection mode
// is one row and no call site re-derives which failure it names.
[SmartEnum<string>]
internal sealed partial class ProbeRejection {
    public static readonly ProbeRejection Overtravel = new("overtravel",
        static (at, limit) => FabricationFault.ProbeOvertravel(at, limit).ToError());
    public static readonly ProbeRejection ShortOfSurface = new("short-of-surface",
        static (_, _) => new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:short-of-surface").ToError());
    public static readonly ProbeRejection LateralDrift = new("lateral-drift",
        static (_, _) => new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:lateral-drift").ToError());
    public static readonly ProbeRejection ThermalScale = new("thermal-scale",
        static (_, _) => new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:thermal-scale").ToError());

    [UseDelegateFromConstructor]
    internal partial Error Fault(Point3d at, double limitMm);
}

file readonly record struct CompensatedContact(Point3d Point, double ThermalUncertaintyMm, Instant At);

// Verdict, observation, and contact are one discriminant: a hit always has both, a miss has neither, and a
// rejection always retains the observation plus the measure that rejected it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
file abstract partial record ProbeOutcome {
    private ProbeOutcome() { }

    public sealed record Contacted(ProbeObservation Observation, CompensatedContact Contact) : ProbeOutcome;
    public sealed record Missed : ProbeOutcome;
    public sealed record Rejected(ProbeObservation Observation, ProbeRejection Reason, double MeasuredMm, double LimitMm) : ProbeOutcome;

    public Option<CompensatedContact> Contact => Switch(
        contacted: static row => Some(row.Contact),
        missed: static _ => Option<CompensatedContact>.None,
        rejected: static _ => Option<CompensatedContact>.None);

    public Option<Error> Fault => Switch(
        contacted: static _ => Option<Error>.None,
        missed: static _ => Option<Error>.None,
        rejected: static row => Some(row.Reason.Fault(row.Observation.BallCenter, row.LimitMm)));
}

file sealed record ProbeCycleReceipt(ProbeTarget Target, ProbeOutcome Outcome);
file sealed record UnregisteredFeature(
    ProbeTarget Target,
    Point3d Measured,
    double RepeatabilityMm,
    double MeasurementUncertaintyMm,
    Instant At);
file sealed record ProbeDatum(
    DatumReceipt Datum,
    Transform Registration,
    Option<AlignmentReceipt> Alignment,
    Option<FitReceipt> Fit);
file sealed record ProbeReport(
    UInt128 SourceEvidence,
    Seq<ProbeCycleReceipt> Cycles,
    ProbeDatum Datum,
    Seq<MeasuredFeature> Features,
    Option<CapabilityReport> Capability,
    Instant At);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Probe {
    private const double MadConsistency = 1.4826;

    internal static readonly Op ProbeOp = Op.Of(name: "fabrication:probe");

    public static Fin<FabricationResult> Inspect(InspectPolicy policy, FabricationInput input, FabricationTap? tap = null) =>
        EngineSpan.ProbeFit.Traced(span =>
            from admitted in Admit(policy, input)
            from context in Context.Millimeters().ToFin()
            from targets in Targets(admitted.Policy, context)
            from _targets in AdmitTargets(admitted.Policy, targets)
            let observed = Index(admitted.Policy.Source.Rows, static row => row.Address.Target)
            let cycles = targets.Bind(target => Evaluate(target, observed, admitted.Policy))
            let contacted = Index(cycles, static row => row.Target.Key)
            from measured in (
                targets.Traverse(target => Aggregate(target, contacted, admitted.Policy).ToValidation()),
                RequiredContacts(targets, contacted).ToValidation())
                .Apply(static (rows, _) => rows).As().ToFin()
            let unregistered = measured.Bind(static row => row.ToSeq())
            from datum in unregistered.Head
                .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:no-measurements").ToError())
                .Bind(_ => Reconcile(admitted.Policy.Datum, unregistered, context))
            let _registered = EngineSpan.Event(span, "datum-registered")
            let _icp = datum.Alignment.Map(receipt =>
                FabricationFact.Engine.Of(receipt).Map((tap ?? FabricationTap.Silent).Fire).Strict())
            let transformed = TransformFeatures(unregistered, datum)
            from features in Fits(transformed, admitted.Policy.FeatureFit, context)
            let _fitted = EngineSpan.Event(span, "features-fitted")
            from capability in admitted.Policy.Capability
                .Traverse(demand => Capability.Assess(new CapabilityStudy.Variables(features.Map(static row => row.Residual)), demand))
                .As()
            let report = new ProbeReport(
                admitted.Policy.Source.EvidenceKey,
                cycles,
                datum,
                features,
                capability,
                admitted.Policy.Clock.GetCurrentInstant())
            from result in ToResult(report, admitted.Input.Sources + admitted.Input.ParentRuns, tap ?? FabricationTap.Silent)
            select result);

    private static Fin<Seq<ProbeTarget>> Targets(InspectPolicy policy, Context context) =>
        policy.Plans.TraverseM(plan =>
                plan.Feature.Project(plan.Count, context).Map(samples =>
                    samples.Map((sample, index) => {
                        Vector3d outward = Unit(sample.SurfaceNormal);
                        Vector3d direction = plan.Cycle.Approach(outward);
                        Point3d start = sample.Nominal - (direction * plan.ClearanceMm);
                        return new ProbeTarget(
                            ProbeTargetKey.Create(plan.Cycle, plan.Key, index),
                            plan,
                            sample.Nominal,
                            outward,
                            direction,
                            start,
                            start + (direction * plan.TravelLimitMm));
                    })))
            .As()
            .Map(static rows => rows.Bind(identity));

    // Correlation is keyed, never scanned: a linear filter per target makes ingress, aggregation, and
    // grouping quadratic in contact count, which a production inspection run reaches immediately.
    private static HashMap<TKey, Seq<TRow>> Index<TKey, TRow>(Seq<TRow> rows, Func<TRow, TKey> key) =>
        rows.Fold(
            HashMap<TKey, Seq<TRow>>(),
            (map, row) => map.AddOrUpdate(key(row), existing => existing.Add(row), Seq(row)));

    private static Fin<(InspectPolicy Policy, FabricationInput Input)> Admit(InspectPolicy? policy, FabricationInput? input) =>
        (Optional(policy).ToValidation(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:policy").ToError()),
         Optional(input).ToValidation(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:input").ToError()))
        .Apply(static (acceptedPolicy, acceptedInput) => (Policy: acceptedPolicy, Input: acceptedInput))
        .As()
        .ToFin()
        .Bind(admitted => (
            Gate(admitted.Policy.Source.Valid
                && admitted.Policy.Source.Rows.ForAll(row => admitted.Policy.Source.Window.Contains(row.At)), "probe:source-window"),
            Gate(admitted.Policy.Source.Rows.ForAll(row => row.EvidenceKey == admitted.Policy.Source.EvidenceKey), "probe:evidence-identity"),
            Gate(admitted.Policy.Source.Rows.ForAll(row => admitted.Policy.Calibration.Validity.Contains(row.At)), "probe:calibration-window"),
            Gate(DatumValid(admitted.Policy.Datum) && admitted.Policy.Datum.Receipt.Traceable, "probe:datum-traceability"))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .ToFin()
            .Map(_ => admitted));

    private static Fin<Unit> AdmitTargets(InspectPolicy policy, Seq<ProbeTarget> targets) =>
        (Gate(targets.Count == policy.Plans.Map(static row => row.Count).Sum()
             && policy.Plans.ForAll(plan => plan.Attempts >= policy.Repeat.MinimumAccepted), "probe:target-count"),
         Gate(targets.Map(static row => row.Key).Distinct().Count == targets.Count, "probe:target-key"),
         Gate(Index(policy.Plans, static plan => plan.Key)
             .ForAll(static entry => entry.Value.ForAll(peer => peer.Feature == entry.Value[0].Feature)), "probe:feature-identity"),
         Gate(
             policy.Source.Rows.Map(static row => row.Address).Distinct().Count == policy.Source.Rows.Count
             && policy.Source.Rows.ForAll(row => targets.Exists(target =>
                 target.Key == row.Address.Target && row.Address.Attempt < target.Plan.Attempts)),
             "probe:observation-address"))
        .Apply(static (_, _, _, _) => unit)
        .As()
        .ToFin();

    private static K<Validation<Error>, Unit> Gate(bool valid, string locus) =>
        guard(valid, new GeometryFault.DegenerateInput(Kind.Mesh, -1, locus).ToError()).ToFin().ToValidation();

    private static bool DatumValid(DatumPolicy policy) => policy.Switch(
        setup: static row => row.Registration.IsValid,
        bestFit: static row => row.Kind is not null,
        substitute: static row => !row.Kinds.IsEmpty
            && row.Kinds.ForAll(static kind => kind is not null)
            && row.Kinds.Distinct().Count == row.Kinds.Count && row.FitPolicy is not null
            && row.Registration is not null);

    // One contact leaving its admitted path is per-contact evidence, never a program verdict: aborting here
    // would destroy every other feature's measurement over a single rejected touch, so the reason rides the
    // outcome and the repeat fold decides whether the target still has enough contacts to stand.
    private static Seq<ProbeCycleReceipt> Evaluate(
        ProbeTarget target,
        HashMap<ProbeTargetKey, Seq<ProbeObservation>> observed,
        InspectPolicy policy) {
        Seq<ProbeObservation> rows = observed.Find(target.Key)
            .Map(static found => found.OrderBy(static row => row.Address.Attempt).ThenBy(static row => row.At).ToSeq())
            .IfNone(Seq<ProbeObservation>());
        return rows.IsEmpty
            ? Seq(new ProbeCycleReceipt(target, new ProbeOutcome.Missed()))
            : rows.Map(row => Evaluate(target, row, policy.Calibration));
    }

    private static ProbeCycleReceipt Evaluate(
        ProbeTarget target,
        ProbeObservation observation,
        StylusCalibration calibration) {
        Vector3d displacement = observation.BallCenter - target.Start;
        double travel = displacement * target.Direction;
        double lateral = (displacement - (target.Direction * travel)).Length;
        return (travel, lateral) switch {
            (var axial, _) when axial > target.Plan.TravelLimitMm => new ProbeCycleReceipt(
                target,
                new ProbeOutcome.Rejected(observation, ProbeRejection.Overtravel, axial, target.Plan.TravelLimitMm)),
            (var axial, _) when axial < 0.0 => new ProbeCycleReceipt(
                target,
                new ProbeOutcome.Rejected(observation, ProbeRejection.ShortOfSurface, axial, 0.0)),
            (_, var radial) when radial > target.Plan.ApproachToleranceMm => new ProbeCycleReceipt(
                target,
                new ProbeOutcome.Rejected(observation, ProbeRejection.LateralDrift, radial, target.Plan.ApproachToleranceMm)),
            _ => new ProbeCycleReceipt(target, Compensate(target, observation, calibration)),
        };
    }

    private static ProbeOutcome Compensate(
        ProbeTarget target,
        ProbeObservation observation,
        StylusCalibration calibration) {
        double angle = Math.Atan2(target.SurfaceNormal.Y, target.SurfaceNormal.X);
        double lobe = calibration.Lobes.Map(row =>
                row.AmplitudeMm * Math.Cos((row.Harmonic * angle) + row.PhaseRadians))
            .Sum();
        // Pre-travel is lost motion AFTER contact: the reported ball centre sits that far past the true
        // touch along the approach, so it subtracts where the stylus radius and its lobing term add.
        Point3d surface = observation.BallCenter
            + (target.Direction * (calibration.RadiusMm - calibration.PreTravelMm + lobe));
        double deltaTemperature = observation.TemperatureC - calibration.ReferenceTemperatureC;
        double scale = 1.0 + (calibration.ThermalExpansionPerC * deltaTemperature);
        Vector3d displacement = surface - calibration.ThermalReference;
        return scale > 0.0 && double.IsFinite(scale)
            ? new ProbeOutcome.Contacted(observation, new CompensatedContact(
                calibration.ThermalReference + (displacement / scale),
                Math.Abs((1.0 / scale) - 1.0) * displacement.Length,
                observation.At))
            : new ProbeOutcome.Rejected(observation, ProbeRejection.ThermalScale, scale, 0.0);
    }

    private static Fin<Option<UnregisteredFeature>> Aggregate(
        ProbeTarget target,
        HashMap<ProbeTargetKey, Seq<ProbeCycleReceipt>> contacted,
        InspectPolicy policy) {
        Seq<ProbeCycleReceipt> cycles = contacted.Find(target.Key).IfNone(Seq<ProbeCycleReceipt>());
        Seq<CompensatedContact> rows = cycles.Bind(static cycle => cycle.Outcome.Contact.ToSeq());
        if (rows.IsEmpty) return Fin.Succ(Option<UnregisteredFeature>.None);
        Point3d centre = MedianPoint(rows.Map(static row => row.Point));
        Seq<double> distances = rows.Map(row => row.Point.DistanceTo(centre));
        double median = Median(distances);
        double mad = Median(distances.Map(value => Math.Abs(value - median)));
        double band = policy.Repeat.OutlierSigma * Math.Max(mad * MadConsistency, double.Epsilon);
        Seq<CompensatedContact> accepted = rows.Filter(row => row.Point.DistanceTo(centre) <= median + band);
        if (accepted.Count < policy.Repeat.MinimumAccepted)
            return Fin.Fail<Option<UnregisteredFeature>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:repeatability").ToError());
        Point3d measured = Mean(accepted.Map(static row => row.Point));
        double repeatability = Math.Sqrt(accepted.Map(row => Math.Pow(row.Point.DistanceTo(measured), 2.0)).Sum() / accepted.Count);
        double thermal = accepted.Map(static row => row.ThermalUncertaintyMm).Fold(0.0, double.Max);
        double uncertainty = Math.Sqrt(
            Math.Pow(policy.Calibration.CalibrationUncertaintyMm, 2.0)
            + Math.Pow(policy.Repeat.MinimumUncertaintyMm, 2.0)
            + Math.Pow(repeatability, 2.0)
            + Math.Pow(thermal, 2.0));
        Instant at = accepted.Map(static row => row.At).Order().Last.IfNone(policy.Clock.GetCurrentInstant());
        return Fin.Succ(Some(new UnregisteredFeature(target, measured, repeatability, uncertainty, at)));
    }

    private static Fin<Unit> RequiredContacts(
        Seq<ProbeTarget> targets,
        HashMap<ProbeTargetKey, Seq<ProbeCycleReceipt>> contacted) {
        Seq<Error> errors = targets
            .Filter(static target => target.Plan.Cycle.RequiresHit)
            .Choose(target => {
                Seq<ProbeCycleReceipt> cycles = contacted.Find(target.Key).IfNone(Seq<ProbeCycleReceipt>());
                return cycles.Exists(static cycle => cycle.Outcome.Contact.IsSome)
                    ? Option<Error>.None
                    : Some(cycles.Choose(static cycle => cycle.Outcome.Fault).Head
                        .IfNone(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:required-contact").ToError()));
            });
        return errors.Head.Match(
            None: static () => Fin.Succ(unit),
            Some: first => Fin.Fail<Unit>(errors.Tail.Fold(first, static (combined, error) => combined + error)));
    }

    private static Fin<ProbeDatum> Reconcile(
        DatumPolicy policy,
        Seq<UnregisteredFeature> features,
        Context context) => policy.Switch(
        state: (Features: features, Context: context),
        setup: static (_, row) => row.Registration.IsValid
            ? Fin.Succ(new ProbeDatum(row.Datum, row.Registration, None, None))
            : Fin.Fail<ProbeDatum>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:setup-transform").ToError()),
        bestFit: static (state, row) => Align(state.Features, state.Context, row.Kind, row.Policy)
            .Map(receipt => new ProbeDatum(row.Datum, receipt.Transform, Some(receipt.Receipt), None)),
        substitute: static (state, row) =>
            from fit in Fit.Apply(
                new FitOp(
                    row.Kinds,
                    state.Features.Map(static feature => feature.Measured).ToArray(),
                    row.Kinds.Exists(static kind => kind.NeedsNormals)
                        ? Some(state.Features.Map(static feature => feature.Target.SurfaceNormal).ToArray())
                        : None,
                    row.FitPolicy),
                state.Context,
                ProbeOp)
            let inliers = state.Features
                .Map((feature, index) => (Feature: feature, Index: index))
                .Filter(pair => fit.Inliers[pair.Index])
                .Map(static pair => pair.Feature)
            from alignment in Align(inliers, state.Context, row.Registration, row.Alignment)
            select new ProbeDatum(row.Datum, alignment.Transform, Some(alignment.Receipt), Some(fit)));

    private static Fin<(Transform Transform, AlignmentReceipt Receipt)> Align(
        Seq<UnregisteredFeature> features,
        Context context,
        AlignKind kind,
        AlignmentPolicy policy) =>
        from source in VectorCloud.Cluster(features.Map(static row => row.Measured), context, key: ProbeOp)
        from clouds in VectorCloud.Cluster(features.Map(static row => row.Target.Nominal), context, key: ProbeOp)
            .Map(target => (Source: source, Target: target))
            .MapFail(error => {
                source.Dispose();
                return error;
            })
        from aligned in Align(clouds, kind, policy)
        select aligned;

    private static Fin<(Transform Transform, AlignmentReceipt Receipt)> Align(
        (VectorCloud Source, VectorCloud Target) clouds,
        AlignKind kind,
        AlignmentPolicy policy) {
        using (clouds.Source)
        using (clouds.Target)
            return kind.AlignDetailed(clouds.Source, clouds.Target, policy, ProbeOp)
                .Bind(receipt => receipt.Project<Transform>(ProbeOp).Map(transform => (transform, receipt)));
    }

    private static Seq<MeasuredFeature> TransformFeatures(
        Seq<UnregisteredFeature> rows,
        ProbeDatum datum) =>
        rows.Map((row, index) => {
            Point3d measured = Apply(datum.Registration, row.Measured);
            double deviation = (measured - row.Target.Nominal) * row.Target.SurfaceNormal;
            double registrationSquared = datum.Alignment.Map(static receipt => Math.Pow(receipt.FinalDelta, 2.0)).IfNone(0.0)
                + datum.Fit.Map(static receipt => Math.Pow(receipt.Residual, 2.0)).IfNone(0.0);
            double uncertainty = Math.Sqrt(
                Math.Pow(row.MeasurementUncertaintyMm, 2.0)
                + registrationSquared);
            return new MeasuredFeature(
                row.Target.Key,
                row.Target.Plan,
                row.Target.Nominal,
                measured,
                row.Target.SurfaceNormal,
                new ResidualSample(index, row.Target.Nominal, deviation, row.Target.Plan.ToleranceMm, Math.Abs(deviation) <= row.Target.Plan.ToleranceMm),
                uncertainty,
                row.RepeatabilityMm,
                row.At,
                None);
        });

    // Wall-contact filtering can starve a group below the kind's own minimal set, and the solver answers a
    // short cloud with an accumulated admission fault; the group then carries no fit rather than a failure.
    private static Fin<Seq<MeasuredFeature>> Fits(Seq<MeasuredFeature> features, FitPolicy policy, Context context) =>
        toSeq(Index(features, static feature => feature.Plan.Key)).TraverseM(entry => entry.Value.Head
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "probe:fit-group").ToError())
            .Bind(head => {
                Seq<MeasuredFeature> fitGroup = entry.Value.Filter(row => head.Plan.Feature.FitEligible(row.SurfaceNormal));
                return head.Plan.Feature.Spec.Fit
                    .Filter(kind => fitGroup.Count >= kind.MinimalSamples)
                    .Traverse(kind => Fit.Apply(
                        new FitOp(
                            Seq(kind),
                            fitGroup.Map(static row => row.Measured).ToArray(),
                            kind.NeedsNormals ? Some(fitGroup.Map(static row => row.SurfaceNormal).ToArray()) : None,
                            policy),
                        context,
                        ProbeOp))
                    .As();
            })
            .Map(receipt => entry.Value.Map(row => row with { Fit = receipt })))
            .As()
            .Map(static groups => groups.Bind(identity));

    // The Probe fact mints beside the frozen result because `ProbeReport` is file-scoped; conformance and
    // worst deviation read off the projected atoms so the fact and the result derive from one fold.
    private static Fin<FabricationResult> ToResult(ProbeReport report, Seq<ContentKey> subjects, FabricationTap tap) =>
        report.Features.TraverseM(ToAtom).As()
            .Map(atoms => (
                tap.Fire(new FabricationFact.Probe(
                    atoms.Count,
                    atoms.Filter(static row => row.Pass.IfNone(false)).Count,
                    atoms.Map(static row => row.DeviationMm).Fold(0.0, double.Max))),
                (FabricationResult)new FabricationResult.InspectionResult(atoms, subjects.Distinct())).Item2);

    private static Fin<InspectionFeature> ToAtom(MeasuredFeature feature) =>
        InspectionFeature.Admit(
            feature.Key.Text,
            feature.Nominal,
            feature.Measured,
            Some(feature.Plan.ToleranceMm),
            feature.UncertaintyMm,
            InspectionMethod.Probe);

    private static Point3d Apply(Transform transform, Point3d point) {
        point.Transform(transform);
        return point;
    }

    private static Point3d MedianPoint(Seq<Point3d> points) =>
        new(Median(points.Map(static point => point.X)), Median(points.Map(static point => point.Y)), Median(points.Map(static point => point.Z)));

    private static Point3d Mean(Seq<Point3d> points) =>
        Point3d.Origin + (points.Map(static point => point - Point3d.Origin)
            .Fold(new Vector3d(), static (sum, vector) => sum + vector) / points.Count);

    private static double Median(Seq<double> values) {
        Seq<double> ordered = values.Order().ToSeq();
        int middle = ordered.Count / 2;
        return ordered.Count % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) * 0.5 : ordered[middle];
    }

    private static Vector3d Unit(Vector3d direction) {
        _ = direction.Unitize();
        return direction;
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
