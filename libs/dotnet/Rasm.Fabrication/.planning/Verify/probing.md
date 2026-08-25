# [RASM_FABRICATION_PROBING]

`Probe.Inspect` owns post-cycle metrology truth: one admitted `InspectPolicy` generates feature-complete contact targets, correlates exact controller cycles with repeat observations, compensates calibrated stylus behavior in the probe's own frame, reconciles datum registration with its anisotropic uncertainty, and projects transformed residuals onto `FabricationResult.InspectionResult`.

`FabricationPolicy.Inspect`, `GCommand`, `DatumLineage`, `Fitted`, `Capability.Assess`, and `InspectionFeature` remain frozen seams. Contact generation composes the kernel `Deterministic` equidistribution owner, robust aggregation composes `MathNet.Numerics.Statistics`, primitive fitting composes the kernel `FitKind` roster, and residual statistics compose `AnalysisQuery.Conformance`; this page mints no draw sequence, no summary statistic, and no fit primitive of its own. Decoded measurement rows enter as typed data; controller transport and work-offset mutation remain outside the Verify plane.

## [01]-[INDEX]

- [02]-[FEATURE_SPACE]: nominal feature geometry, the chart-and-sampler contact algebra every analytic case declares, the fit correspondence per case, and the inspection demand.
- [03]-[OBSERVATION_RAIL]: exact cycle-addressed ingress, temporal containment, per-contact outcome evidence, and probe-frame calibration compensation.
- [04]-[DATUM_AND_RESULT]: registration before residuals, lever-arm uncertainty propagation, order-declared primitive fitting, capability projection, and atoms-safe egress.

## [02]-[FEATURE_SPACE]

- Owner: `ProbeFeature` closes the inspection geometry family as pure nominal geometry; `ContactChart` owns one parametric contact surface as a chart plus its own measure and floor; `ContactSampler` owns the parameter draw; `ProbePlan` owns the inspection demand — feature key, tolerance band, `ProbeCycle`, sample count, attempts, feed, clearance, travel, and lateral approach tolerance.
- Law: contact generation is a CHART plus a SAMPLER, never a generator body. Every analytic case declares its chart set as data — a rectangle, a wall of revolution, a disc, a polyline, or a constant — so a plane, a ring, a cylinder, a cone, a torus, a sphere, and a web share one evaluator and differ only in their chart rows. A composite feature is more than one chart under one area-share allocation, which is the single rule the capped and slotted forms each ran as a body of its own.
- Law: equidistribution is the kernel's. `Deterministic.RadicalInverse` is the van der Corput coordinate a page-local golden-conjugate constant stood in for, and a chart sweeping one axis uniformly draws its second axis from it, so contacts spread over the whole chart instead of banding on one meridian. No page-local draw sequence exists.
- Law: a composite's charts share a DIMENSION, so the allocation weight is each chart's own measure and the column never mixes a length with an area.
- Cases: `FeatureSpec` carries the contact floor, the optional ceiling, the substitute-fit kind, and the `FitFilter` naming WHICH contacts feed that fit. `Bore`, `Boss`, and `Cylinder` fit `FitKind.Cylinder` over wall contacts alone, so a cap contact never enters a cylinder's normal matrix. `Web` fits `FitKind.Plane` over the contacts aligned with ONE face, because a plane fitted across two antiparallel faces returns the mid-plane, which is no measured face.
- Law: `Circle` fits the kernel `FitKind.Circle` row over its rim contacts — the coplanar contact set that leaves a six-parameter cylinder's axis unconstrained determines the circle's plane, centre, and radius exactly, so the substitute fit composes the roster member and no rank-deficient stand-in survives. Two features carry no substitute fit, each for its own settled reason. `Slot` is two end arcs and two parallel flanks sharing one width — a constrained composite over a circle pair and a line pair that the one-kind `FeatureSpec` fit column cannot carry, so its composite fit stays a downstream consumer fold over `Fit.Apply` and the slot answers per-contact residuals here. `Surface` carries no fit by its own nature: a free-form feature is measured as deviation to its nominal geometry and no primitive stands in for it. Both answer per-contact residuals, and a page-local fit body is a second fitting owner.
- Entry: `Probe.Inspect` is the sole public operation; each generated `ProbeTarget` carries the exact `ProbeTargetKey` whose one `Text` spelling posting, telemetry, residuals, and result identity all read. Every owner admits through its generated `Validate` onto the `Admission.Admitted` bridge; a throwing `Create` at a construction site is the deleted form.
- Exemption: `ContactChart.Allocate` is a statement kernel — integer budget allocation with floors and a rounding residue has no expression form that spends the budget exactly.
- Auto: one `Validation<Error, Unit>` fan-in proves feature coverage, target uniqueness, observation references, evidence identity, and datum traceability, so an inadmissible demand reports every violated invariant rather than the first.
- Growth: a feature sub-kind is one `ProbeFeature` case, one `ContactSource` arm, and one `FeatureSpec` row; no feature-specific inspection entrypoint and no generator body survives beside it.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.Statistics;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Element.Projection;
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

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
public sealed partial class FitFilter {
    public static readonly FitFilter All = new("all", static (_, _, _) => true);
    public static readonly FitFilter PerpendicularTo = new("perpendicular-to",
        static (normal, axis, tolerance) => Math.Abs(normal * axis) <= tolerance);
    public static readonly FitFilter AlignedWith = new("aligned-with",
        static (normal, axis, tolerance) => (normal * axis) >= 1.0 - tolerance);

    [UseDelegateFromConstructor]
    public partial bool Admits(Vector3d contactNormal, Vector3d axis, double tolerance);
}

[SmartEnum<string>]
internal sealed partial class ContactSampler {
    public static readonly ContactSampler Lattice = new("lattice", Grid);
    public static readonly ContactSampler Equidistributed = new("equidistributed", Equidistribution);

    [UseDelegateFromConstructor]
    internal partial Seq<(double U, double V)> Draw(int count);

    private static Seq<(double U, double V)> Grid(int count) {
        int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));
        int rows = Math.Max(1, (int)Math.Ceiling((double)count / columns));
        return toSeq(Enumerable.Range(0, count)).Map(index => (
            columns == 1 ? 0.5 : (double)(index % columns) / (columns - 1),
            rows == 1 ? 0.5 : (double)(index / columns) / (rows - 1)));
    }

    private static Seq<(double U, double V)> Equidistribution(int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index => Deterministic.Hammersley(index, count));
}

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ContactChart(
    ContactSampler Sampler,
    double Measure,
    int Floor,
    Func<double, double, FeatureSample> At) {
    public static ContactChart Constant(Point3d point, Vector3d normal) =>
        new(ContactSampler.Lattice, Measure: 1.0, Floor: 1, (_, _) => new FeatureSample(point, Probe.Unit(normal)));

    public static ContactChart Span(Point3d from, Point3d to, Vector3d normal) =>
        new(ContactSampler.Lattice, from.DistanceTo(to), Floor: 1,
            (u, _) => new FeatureSample(from + ((to - from) * u), Probe.Unit(normal)));

    public static ContactChart Rectangle(Plane frame, double width, double height, Vector3d normal, int floor) =>
        new(ContactSampler.Lattice, width * height, floor,
            (u, v) => new FeatureSample(
                frame.Origin + (frame.XAxis * ((u - 0.5) * width)) + (frame.YAxis * ((v - 0.5) * height)),
                Probe.Unit(normal)));

    public static ContactChart Wall(
        Plane frame,
        double height,
        ProbeSense sense,
        Func<double, double> radiusAt,
        Func<Vector3d, Vector3d> normalAt,
        int floor) =>
        new(ContactSampler.Equidistributed, Math.Tau * radiusAt(0.5) * Math.Max(height, radiusAt(0.5)), floor,
            (u, v) => {
                double angle = Math.Tau * v;
                Vector3d radial = (frame.XAxis * Math.Cos(angle)) + (frame.YAxis * Math.Sin(angle));
                return new FeatureSample(
                    frame.Origin + (frame.ZAxis * (height * u)) + (radial * radiusAt(u)),
                    sense.Orient(normalAt(radial)));
            });

    public static ContactChart Disc(Plane frame, double radius, double atHeight, Vector3d normal, int floor) =>
        new(ContactSampler.Equidistributed, Math.PI * radius * radius, floor,
            (u, v) => {
                double angle = Math.Tau * v;
                Vector3d radial = (frame.XAxis * Math.Cos(angle)) + (frame.YAxis * Math.Sin(angle));
                return new FeatureSample(
                    frame.Origin + (frame.ZAxis * atHeight) + (radial * (radius * Math.Sqrt(u))),
                    Probe.Unit(normal));
            });

    public static ContactChart Ball(Point3d centre, double radius, int floor) =>
        new(ContactSampler.Equidistributed, 2.0 * Math.Tau * radius * radius, floor,
            (u, v) => {
                double z = 1.0 - (2.0 * u);
                double azimuth = Math.Tau * v;
                double planar = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z)));
                Vector3d normal = new(planar * Math.Cos(azimuth), planar * Math.Sin(azimuth), z);
                return new FeatureSample(centre + (normal * radius), normal);
            });

    public static ContactChart Tube(Plane frame, double major, double minor, ProbeSense sense, int floor) =>
        new(ContactSampler.Equidistributed, Math.Tau * major * Math.Tau * minor, floor,
            (u, v) => {
                double sweep = Math.Tau * u;
                double tube = Math.Tau * v;
                Vector3d radial = (frame.XAxis * Math.Cos(sweep)) + (frame.YAxis * Math.Sin(sweep));
                Vector3d normal = (radial * Math.Cos(tube)) + (frame.ZAxis * Math.Sin(tube));
                return new FeatureSample(frame.Origin + (radial * major) + (normal * minor), sense.Orient(normal));
            });

    public static ContactChart Polyline(Seq<FeatureSample> samples) {
        Seq<(FeatureSample From, FeatureSample To, double Length)> spans = toSeq(samples.AsIterable()
            .Zip(samples.AsIterable().Skip(1), static (from, to) => (from, to, from.Nominal.DistanceTo(to.Nominal))));
        double length = spans.Sum(static row => row.Length);
        return new ContactChart(ContactSampler.Lattice, length, Floor: 2, (u, _) => spans
            .Fold(
                (Remaining: u * length, Sample: samples[0]),
                static (state, span) => state.Remaining <= 0.0 || span.Length <= 0.0
                    ? state
                    : state.Remaining <= span.Length
                        ? (0.0, Interpolated(span.From, span.To, state.Remaining / span.Length))
                        : (state.Remaining - span.Length, span.To))
            .Sample);
    }

    private static FeatureSample Interpolated(FeatureSample from, FeatureSample to, double fraction) =>
        new(from.Nominal + ((to.Nominal - from.Nominal) * fraction),
            Probe.Unit(from.SurfaceNormal + ((to.SurfaceNormal - from.SurfaceNormal) * fraction)));

    internal static Fin<Seq<(ContactChart Chart, int Count)>> Allocate(Seq<ContactChart> charts, int count) {
        if (charts.Count == 1) return Fin.Succ(Seq((charts[0], count)));
        int floors = charts.Sum(static chart => chart.Floor);
        if (count < floors)
            return Fin.Fail<Seq<(ContactChart, int)>>(
                new KernelFault.InvalidValue("probing", "probe:contact-budget"));

        double measure = charts.Sum(static chart => chart.Measure);
        int[] counts = [.. charts.Map(chart => chart.Floor)];
        int spare = count - floors;
        int[] shares = [.. charts.Map(chart => (int)(spare * chart.Measure / measure))];
        for (int index = 0; index < counts.Length; index++) counts[index] += shares[index];

        int widest = 0;
        for (int index = 1; index < counts.Length; index++)
            if (charts[index].Measure > charts[widest].Measure) widest = index;
        counts[widest] += count - counts.Sum();

        return Fin.Succ(toSeq(counts).Map((allocated, index) => (charts[index], allocated)));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record ContactSource {
    private ContactSource() { }

    public sealed record Charted(Seq<ContactChart> Charts) : ContactSource;
    public sealed record Extracted(ExtractionDomain Domain, SampleKind Sampling, Vector3d Normal) : ContactSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProbeFeature {
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

    internal FeatureSpec Spec => Switch(
        point: static _ => new FeatureSpec(1, Some(1), None, FitFilter.All, None),
        line: static _ => new FeatureSpec(FitKind.Line.MinimalSamples, None, Some(FitKind.Line), FitFilter.All, None),
        plane: static _ => new FeatureSpec(FitKind.Plane.MinimalSamples, None, Some(FitKind.Plane), FitFilter.All, None),
        circle: static _ => new FeatureSpec(FitKind.Circle.MinimalSamples, None, Some(FitKind.Circle), FitFilter.All, None),
        bore: static row => new FeatureSpec(
            FitKind.Cylinder.MinimalSamples + 1, None, Some(FitKind.Cylinder), FitFilter.PerpendicularTo, Some(row.Frame.ZAxis)),
        boss: static row => new FeatureSpec(
            FitKind.Cylinder.MinimalSamples + 1, None, Some(FitKind.Cylinder), FitFilter.PerpendicularTo, Some(row.Frame.ZAxis)),
        slot: static _ => new FeatureSpec(5, None, None, FitFilter.All, None),
        web: static row => new FeatureSpec(
            FitKind.Plane.MinimalSamples + 1, None, Some(FitKind.Plane), FitFilter.AlignedWith, Some(row.Frame.ZAxis)),
        sphere: static _ => new FeatureSpec(FitKind.Sphere.MinimalSamples, None, Some(FitKind.Sphere), FitFilter.All, None),
        cylinder: static _ => new FeatureSpec(FitKind.Cylinder.MinimalSamples, None, Some(FitKind.Cylinder), FitFilter.All, None),
        cone: static _ => new FeatureSpec(FitKind.Cone.MinimalSamples, None, Some(FitKind.Cone), FitFilter.All, None),
        torus: static _ => new FeatureSpec(FitKind.Torus.MinimalSamples, None, Some(FitKind.Torus), FitFilter.All, None),
        profile: static row => new FeatureSpec(Math.Min(row.Samples.Count, 2), None, None, FitFilter.All, None),
        surface: static _ => new FeatureSpec(3, None, None, FitFilter.All, None));

    internal ContactSource Source => Switch(
        point: static row => (ContactSource)new ContactSource.Charted(Seq(ContactChart.Constant(row.Nominal, row.Normal))),
        line: static row => new ContactSource.Charted(Seq(ContactChart.Span(row.Nominal.From, row.Nominal.To, row.Normal))),
        plane: static row => new ContactSource.Charted(Seq(
            ContactChart.Rectangle(row.Frame, row.WidthMm, row.HeightMm, row.Frame.ZAxis, FitKind.Plane.MinimalSamples))),
        circle: static row => new ContactSource.Charted(Seq(
            ContactChart.Wall(row.Frame, 0.0, ProbeSense.Outside, _ => row.RadiusMm, static radial => radial,
                FitKind.Circle.MinimalSamples))),
        bore: static row => new ContactSource.Charted(Seq(
            ContactChart.Wall(row.Frame, row.DepthMm, ProbeSense.Inside, _ => row.DiameterMm * 0.5, static radial => radial,
                FitKind.Cylinder.MinimalSamples),
            ContactChart.Disc(row.Frame, row.DiameterMm * 0.5, row.DepthMm, -row.Frame.ZAxis, floor: 1))),
        boss: static row => new ContactSource.Charted(Seq(
            ContactChart.Wall(row.Frame, row.HeightMm, ProbeSense.Outside, _ => row.DiameterMm * 0.5, static radial => radial,
                FitKind.Cylinder.MinimalSamples),
            ContactChart.Disc(row.Frame, row.DiameterMm * 0.5, row.HeightMm, row.Frame.ZAxis, floor: 1))),
        slot: static row => new ContactSource.Charted(SlotCharts(row.Frame, row.LengthMm, row.WidthMm, row.DepthMm)),
        web: static row => new ContactSource.Charted(Seq(
            ContactChart.Rectangle(Offset(row.Frame, row.ThicknessMm * 0.5), row.LengthMm, row.HeightMm,
                row.Frame.ZAxis, FitKind.Plane.MinimalSamples),
            ContactChart.Rectangle(Offset(row.Frame, -row.ThicknessMm * 0.5), row.LengthMm, row.HeightMm,
                -row.Frame.ZAxis, floor: FitKind.Plane.MinimalSamples - 1))),
        sphere: static row => new ContactSource.Charted(Seq(
            ContactChart.Ball(row.Center, row.RadiusMm, FitKind.Sphere.MinimalSamples))),
        cylinder: static row => new ContactSource.Charted(Seq(
            ContactChart.Wall(row.Frame, row.HeightMm, row.Sense, _ => row.RadiusMm, static radial => radial,
                FitKind.Cylinder.MinimalSamples))),
        cone: static row => new ContactSource.Charted(Seq(
            ContactChart.Wall(row.Frame, row.HeightMm, row.Sense,
                fraction => row.BaseRadiusMm * (1.0 - fraction),
                radial => Probe.Unit(radial + (row.Frame.ZAxis * (row.BaseRadiusMm / row.HeightMm))),
                FitKind.Cone.MinimalSamples))),
        torus: static row => new ContactSource.Charted(Seq(
            ContactChart.Tube(row.Frame, row.MajorRadiusMm, row.MinorRadiusMm, row.Sense, FitKind.Torus.MinimalSamples))),
        profile: static row => new ContactSource.Charted(Seq(ContactChart.Polyline(row.Samples))),
        surface: static row => new ContactSource.Extracted(row.Domain, row.Sampling, row.Normal));

    internal bool Admits(int count) =>
        count >= Spec.Minimum && Spec.Maximum.Map(ceiling => count <= ceiling).IfNone(true);

    internal bool FitEligible(Vector3d contactNormal, double tolerance) => Spec.FitAxis
        .Map(axis => Spec.Filter.Admits(Probe.Unit(contactNormal), axis, tolerance))
        .IfNone(true);

    internal bool Valid => Switch(
        point: static row => ValidityClaim.All(
            ValidityClaim.Finite(row.Nominal), ValidityClaim.Direction(row.Normal)),
        line: static row => ValidityClaim.All(row.Nominal.IsValid, ValidityClaim.Direction(row.Normal)),
        plane: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.WidthMm), ValidityClaim.Positive(row.HeightMm)),
        circle: static row => ValidityClaim.All(row.Frame.IsValid, ValidityClaim.Positive(row.RadiusMm)),
        bore: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.DiameterMm), ValidityClaim.Positive(row.DepthMm)),
        boss: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.DiameterMm), ValidityClaim.Positive(row.HeightMm)),
        slot: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.LengthMm), ValidityClaim.Positive(row.WidthMm),
            ValidityClaim.Positive(row.DepthMm), row.LengthMm > row.WidthMm),
        web: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.LengthMm), ValidityClaim.Positive(row.HeightMm),
            ValidityClaim.Positive(row.ThicknessMm)),
        sphere: static row => ValidityClaim.All(
            ValidityClaim.Finite(row.Center), ValidityClaim.Positive(row.RadiusMm)),
        cylinder: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.RadiusMm), ValidityClaim.Positive(row.HeightMm)),
        cone: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.BaseRadiusMm), ValidityClaim.Positive(row.HeightMm)),
        torus: static row => ValidityClaim.All(
            row.Frame.IsValid, ValidityClaim.Positive(row.MajorRadiusMm),
            ValidityClaim.Positive(row.MinorRadiusMm), row.MajorRadiusMm > row.MinorRadiusMm),
        profile: static row => ValidityClaim.All(
            ValidityClaim.CountAtLeast(row.Samples.Count, floor: 2),
            row.Samples.ForAll(static sample => ValidityClaim.All(
                ValidityClaim.Finite(sample.Nominal), ValidityClaim.Direction(sample.SurfaceNormal))),
            row.Samples.AsIterable().Zip(row.Samples.AsIterable().Skip(1),
                static (from, to) => from.Nominal.DistanceTo(to.Nominal)).Fold(0.0, static (sum, value) => sum + value) > 0.0),
        surface: static row => ValidityClaim.Direction(row.Normal));

    internal Fin<Seq<FeatureSample>> Project(int count, Context context) => Source.Switch(
        state: (Count: count, Context: context),
        charted: static (state, row) => ContactChart.Allocate(row.Charts, state.Count)
            .Map(static allocated => allocated.Bind(static row =>
                row.Chart.Sampler.Draw(row.Count).Map(pair => row.Chart.At(pair.U, pair.V)))),
        extracted: static (state, row) => row.Sampling.Project<Seq<Point3d>>(row.Domain, state.Context)
            .Map(points => points.Take(state.Count).Map(point => new FeatureSample(point, Probe.Unit(row.Normal))).ToSeq()));

    private static Seq<ContactChart> SlotCharts(Rhino.Geometry.Plane frame, double length, double width, double depth) =>
        Seq(
            ContactChart.Rectangle(WallFrame(frame, frame.YAxis * (-width * 0.5), frame.XAxis, frame.ZAxis, depth),
                length, depth, frame.YAxis, floor: 1),
            ContactChart.Rectangle(WallFrame(frame, frame.YAxis * (width * 0.5), frame.XAxis, frame.ZAxis, depth),
                length, depth, -frame.YAxis, floor: 1),
            ContactChart.Rectangle(WallFrame(frame, frame.XAxis * (-length * 0.5), frame.YAxis, frame.ZAxis, depth),
                width, depth, frame.XAxis, floor: 1),
            ContactChart.Rectangle(WallFrame(frame, frame.XAxis * (length * 0.5), frame.YAxis, frame.ZAxis, depth),
                width, depth, -frame.XAxis, floor: 1),
            ContactChart.Rectangle(Offset(frame, -depth), length, width, frame.ZAxis, floor: 1));

    private static Rhino.Geometry.Plane WallFrame(
        Rhino.Geometry.Plane frame, Vector3d offset, Vector3d along, Vector3d up, double depth) =>
        new(frame.Origin + offset - (up * (depth * 0.5)), along, up);

    private static Rhino.Geometry.Plane Offset(Rhino.Geometry.Plane frame, double alongNormal) =>
        new(frame.Origin + (frame.ZAxis * alongNormal), frame.XAxis, frame.YAxis);
}

public readonly record struct FeatureSample(Point3d Nominal, Vector3d SurfaceNormal);

public readonly record struct FeatureSpec(
    int Minimum,
    Option<int> Maximum,
    Option<FitKind> Fit,
    FitFilter Filter,
    Option<Vector3d> FitAxis);

[ComplexValueObject]
public sealed partial class ProbeTargetKey {
    public ProbeCycle Cycle { get; }
    public int Feature { get; }
    public int Sample { get; }

    public string Text => $"{Cycle.Key}:{Feature}:{Sample}";

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProbeCycle cycle,
        ref int feature,
        ref int sample) {
        if (!(ValidityClaim.All(ValidityClaim.Nonnegative(feature), ValidityClaim.Nonnegative(sample))))
            validationError = new ValidationError("probe:target-key");
    }

    public static Fin<ProbeTargetKey> Admit(ProbeCycle cycle, int feature, int sample) =>
        Validate(cycle, feature, sample, out ProbeTargetKey key).Admitted(key);
}

[ComplexValueObject]
public sealed partial class ProbeAddress {
    public ProbeTargetKey Target { get; }
    public int Attempt { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProbeTargetKey target,
        ref int attempt) {
        if (!ValidityClaim.Nonnegative(attempt).Holds)
            validationError = new ValidationError("probe:address");
    }

    public static Fin<ProbeAddress> Admit(ProbeTargetKey target, int attempt) =>
        Validate(target, attempt, out ProbeAddress address).Admitted(address);
}

[ComplexValueObject]
public sealed partial class ProbePlan {
    public Dimension Key { get; }
    public ProbeFeature Feature { get; }
    public ProbeCycle Cycle { get; }
    public Length Band { get; }
    public Dimension Count { get; }
    public Dimension Attempts { get; }
    public PositiveMagnitude FeedMmPerMinute { get; }
    public double ClearanceMm { get; }
    public PositiveMagnitude TravelLimitMm { get; }
    public Tolerance Approach { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Dimension key,
        ref ProbeFeature feature,
        ref ProbeCycle cycle,
        ref Length band,
        ref Dimension count,
        ref Dimension attempts,
        ref PositiveMagnitude feedMmPerMinute,
        ref double clearanceMm,
        ref PositiveMagnitude travelLimitMm,
        ref Tolerance approach) {
        if (!ValidityClaim.All(
            feature.Valid, feature.Admits(count.Value),
            ValidityClaim.CountAtLeast(attempts.Value, floor: 1),
            ValidityClaim.Positive(band.Millimeters),
            ValidityClaim.Nonnegative(clearanceMm),
            travelLimitMm.Value > clearanceMm,
            approach.IsValid))
            validationError = new ValidationError("probe:plan");
    }

    public static Fin<ProbePlan> Admit(
        Dimension key,
        ProbeFeature feature,
        ProbeCycle cycle,
        Length band,
        Dimension count,
        Dimension attempts,
        PositiveMagnitude feedMmPerMinute,
        double clearanceMm,
        PositiveMagnitude travelLimitMm,
        Tolerance approach) =>
        Validate(key, feature, cycle, band, count, attempts, feedMmPerMinute, clearanceMm,
            travelLimitMm, approach, out ProbePlan plan).Admitted(plan);
}
```

## [03]-[OBSERVATION_RAIL]

- Owner: `MeasurementSource` is ONE admitted value carrying its `MeasurementKind` row beside the `Interval`, evidence key, and observation sequence every lane shares; a new ingress modality is one row and no consumer changes, because nothing downstream branches on the lane. `StylusCalibration` owns the calibrated stylus behavior and the probe frame its lobing map is measured in; `RepeatBands` is the repeat regime as a named column block of the inspection demand.
- Law: a scalar band is DERIVED off the model context, never anchored. The lobing map's planar floor and the repeat set's acceptance floor both read `ToleranceLane.Neglect` through `Context.For`, because an anchor is what a lane derives FROM and reading one directly prices a micron probe and a bridge girder against the same number. `ProbeTargetKey` and `ProbeAddress` keep their own admission: observations arrive addressed by them, so their gates are the boundary's and not ceremony over an ordinal the fold already produced.
- Cases: `ProbeCycle` rows retain exact `G31`, `G38.2`, `G38.3`, `G38.4`, and `G38.5` semantics, their posted `GCommand`, and the approach direction they orient; `ProbeOutcome` closes contact, optional miss, and rejection so a hit always carries its observation and compensated point.
- Law: lobing is a function of the direction the stylus DEFLECTS, resolved in the calibrated probe frame. A world-XY azimuth gives every probe on every plane the same phase, which measures nothing the calibration recorded; a deflection along the stylus axis has no azimuth at all, so its lobing term is a measured zero stating that reason rather than an arbitrary phase.
- Auto: `Interval.Contains` gates source and calibration time; `ProbeAddress` retains cycle, feature, sample, and attempt, and correlation runs through one keyed index so contact count never drives quadratic scanning. Observation rows sort by attempt then instant before evaluation, so a repeat fold never reads ingress order.
- Law: robust aggregation composes `MathNet.Numerics.Statistics` for the median centre, the median absolute deviation, and the accepted-set RMS repeatability; combined uncertainty then conserves calibration, thermal, lobing, and repeatability contributions.
- Packages: `MathNet.Numerics.Statistics` (`Statistics.Median`, `Statistics.RootMeanSquare`) — every member answers `double.NaN` on an empty population rather than throwing, so an empty accepted set exits on the absence arm BEFORE any statistic is read.
- Boundary: observations carry ball centers; axial travel, lateral approach, and thermal-scale rejection stay on the affected touch, and the aggregate required-hit verdict runs after every target retains its outcomes. Stylus radius and lobing add along the approach while pre-travel subtracts, and inverse thermal scaling restores reference-temperature geometry.

```csharp
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
        ref UInt128 evidenceKey) {
        if (!(ballCenter.IsValid && double.IsFinite(temperatureC) && evidenceKey != UInt128.Zero))
            validationError = new ValidationError("probe:observation");
    }

    public static Fin<ProbeObservation> Admit(
        ProbeAddress address, Point3d ballCenter, Instant at, double temperatureC, UInt128 evidenceKey) =>
        Validate(address, ballCenter, at, temperatureC, evidenceKey, out ProbeObservation observation)
            .Admitted(observation);
}

[SmartEnum<string>]
public sealed partial class MeasurementKind {
    public static readonly MeasurementKind Telemetry = new("telemetry");
    public static readonly MeasurementKind Manual = new("manual");
    public static readonly MeasurementKind FixtureSynthetic = new("fixture-synthetic");
}

[ComplexValueObject]
public sealed partial class MeasurementSource {
    public MeasurementKind Kind { get; }
    public Interval Window { get; }
    public Seq<ProbeObservation> Rows { get; }
    public UInt128 EvidenceKey { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MeasurementKind kind,
        ref Interval window,
        ref Seq<ProbeObservation> rows,
        ref UInt128 evidenceKey) {
        if (evidenceKey == UInt128.Zero || !rows.ForAll(static row => row.EvidenceKey != UInt128.Zero))
            validationError = new ValidationError("probe:measurement-source");
    }

    public static Fin<MeasurementSource> Admit(
        MeasurementKind kind, Interval window, Seq<ProbeObservation> rows, UInt128 evidenceKey) =>
        Validate(kind, window, rows, evidenceKey, out MeasurementSource source).Admitted(source);
}

public readonly record struct ProbeLobe(int Harmonic, double AmplitudeMm, double PhaseRadians);

[ComplexValueObject]
public sealed partial class StylusCalibration {
    public UInt128 Key { get; }
    public PositiveMagnitude RadiusMm { get; }

    public double PreTravelMm { get; }

    public Rhino.Geometry.Plane ProbeFrame { get; }

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
        ref PositiveMagnitude radiusMm,
        ref double preTravelMm,
        ref Rhino.Geometry.Plane probeFrame,
        ref double thermalExpansionPerC,
        ref double referenceTemperatureC,
        ref Point3d thermalReference,
        ref double calibrationUncertaintyMm,
        ref Interval validity,
        ref Seq<ProbeLobe> lobes) {
        if (!ValidityClaim.All(
            key != UInt128.Zero, probeFrame.IsValid, ValidityClaim.Finite(thermalReference),
            ValidityClaim.Nonnegative(preTravelMm), ValidityClaim.Nonnegative(calibrationUncertaintyMm),
            ValidityClaim.Finite(thermalExpansionPerC), ValidityClaim.Finite(referenceTemperatureC),
            validity.HasStart && validity.HasEnd && validity.End > validity.Start,
            lobes.ForAll(static row => ValidityClaim.All(
                ValidityClaim.CountAtLeast(row.Harmonic, floor: 1),
                ValidityClaim.Finite(row.AmplitudeMm), ValidityClaim.Finite(row.PhaseRadians))),
            lobes.Map(static row => row.Harmonic).Distinct().Count == lobes.Count))
            validationError = new ValidationError("probe:calibration");
    }

    public static Fin<StylusCalibration> Admit(
        UInt128 key,
        PositiveMagnitude radiusMm,
        double preTravelMm,
        Rhino.Geometry.Plane probeFrame,
        double thermalExpansionPerC,
        double referenceTemperatureC,
        Point3d thermalReference,
        double calibrationUncertaintyMm,
        Interval validity,
        Seq<ProbeLobe> lobes) =>
        Validate(key, radiusMm, preTravelMm, probeFrame, thermalExpansionPerC, referenceTemperatureC,
            thermalReference, calibrationUncertaintyMm, validity, lobes, out StylusCalibration calibration)
            .Admitted(calibration);

    public double LobeMm(Vector3d approach, Context model) {
        Vector3d planar = approach - (ProbeFrame.ZAxis * (approach * ProbeFrame.ZAxis));
        if (planar.Length <= model.For(ToleranceLane.Neglect).Value) return 0.0;
        double azimuth = Math.Atan2(planar * ProbeFrame.YAxis, planar * ProbeFrame.XAxis);
        return Lobes.Sum(row => row.AmplitudeMm * Math.Cos((row.Harmonic * azimuth) + row.PhaseRadians));
    }
}

public readonly record struct RepeatBands(
    Dimension MinimumAccepted,
    PositiveMagnitude OutlierSigma,
    Tolerance MinimumUncertainty);

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
            GParam.Number('F', Plan.FeedMmPerMinute.Value, ProgramUnits.Metric)),
        None);
}

[SmartEnum<string>]
internal sealed partial class ProbeRejection {
    public static readonly ProbeRejection Overtravel = new("overtravel",
        static (at, limit) => FabricationFault.ProbeOvertravel(at, limit));
    public static readonly ProbeRejection ShortOfSurface = new("short-of-surface",
        static (_, _) => FabricationFault.Inadmissible(FabConcern.Verify, "probe:short-of-surface"));
    public static readonly ProbeRejection LateralDrift = new("lateral-drift",
        static (_, _) => FabricationFault.Inadmissible(FabConcern.Verify, "probe:lateral-drift"));
    public static readonly ProbeRejection ThermalScale = new("thermal-scale",
        static (_, _) => FabricationFault.Inadmissible(FabConcern.Verify, "probe:thermal-scale"));

    [UseDelegateFromConstructor]
    internal partial Error Fault(Point3d at, double limitMm);
}

file readonly record struct CompensatedContact(Point3d Point, double ThermalUncertaintyMm, Instant At);

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

file sealed record ProbeTouch(ProbeTarget Target, ProbeOutcome Outcome);

file sealed record UnregisteredFeature(
    ProbeTarget Target,
    Point3d Measured,
    double RepeatabilityMm,
    double MeasurementUncertaintyMm,
    Instant At);
```

## [04]-[DATUM_AND_RESULT]

- Owner: `DatumPolicy` closes assigned transform, best-fit registration, primitive substitution, and memo replay over the current `DatumLineage` wire; `RegistrationSpread` owns the anisotropic registration budget; `ProbeMemo` mints the registration content identity; `ProbeReport` owns the pre-egress evidence fold.
- Law: NO result or content key depends on hash iteration order. Grouping serves lookup alone — the substitute fit is computed once per plan key over a keyed index and then READ BACK onto the features in their own admitted order, so residual ordinals, the census population, and the projected atoms all keep the deterministic order the target fold assigned. Where a fold must emit groups it orders on the declared `ProbePlan.Key` ascending, so even the refusal that reports first is fixed.
- Law: registration propagates ANISOTROPICALLY. A point residual over an inlier cloud of characteristic radius bounds the residual rotation at residual-over-radius, which displaces a feature at its lever arm by that angle times the arm. One uniform term understates every feature outside the cloud and overstates the datum origin itself, so both the cloud radius and the per-feature lever arm enter the budget; an assigned setup transform carries no alignment residual and states its absence rather than a zero.
- Law: the kernel conformance metrics are defined over an UNSIGNED residual — `ConformanceMetric.Maximum` ranks by magnitude and carries no sign — so the census residual carries the absolute deviation while the signed deviation stays a named column on `MeasuredFeature`. Feeding a signed value into that slot makes the worst-sample rank the most positive rather than the worst, which reports a clean surface for a wholly undersize feature set.
- Entry: `Probe.Inspect(InspectPolicy, FabricationInput, Option<InstrumentSet> set = default, Option<SpanBand> band = default)` — the set and band both default absent, so a headless run measures and traces nothing with no branch of its own.
- Entry: `ProbeBench.Workload` admits the `icp-probe-fit` measured workload — a best-fit datum lane over the feature-census floor — and `ProbeBench.Run` is the fold the corpus gate times against `FabricationBenchClaims.IcpProbeFit`; measurement and benchmark projection stay the bench edge's under the AppHost claim-field map.
- Law: the fit memo lane is one content key and one cache ride on the standing owner pattern — `ProbeMemo.Key` folds every fit-shifting input through `FabricationCanon.Ordered`, the S0 streaming close, the cache key spells the `icp:` prefix the Persistence solver-memo band dispatches on through the branch `HybridCache` L2, a hit re-enters as `DatumPolicy.Replay` with the memoized transform, residual, and radius, and a miss solves `BestFit` then publishes `(Transform, FinalDelta, RadiusMm)`; the lane composes at the cache-owning boundary, so `Probe.Inspect` and the statement kernel stay memo-free and synchronous.
- Law: the memo preimage frames the alignment policy's LANE KEYS and its Procrustes closing row, not the scalars a context resolves those lanes to. A project override moves the number a fit converges against without re-keying every memoized registration, and the scale decision reads off the `PoseFit` row the policy chose rather than a separate flag stating the same fact twice.
- Exemption: the two-cloud registration region is a statement kernel — resource release is not expressible on the `Fin` rail, and one region releasing both clouds on every exit path replaces a compensating dispose inside a failure lambda, which is a second custody path that leaks the moment a third resource joins.
- Auto: `AlignKind.AlignDetailed` projects a transform only through `Alignment.Project<Transform>`; `Fit.Apply` retains per-feature and datum-substitution `Fitted` evidence, and a group thinned below its kind's `MinimalSamples` carries no fit rather than a fabricated one; transformed measured points precede every `ResidualSample`.
- Output: `ProbeReport` closes the pre-egress evidence fold — cycles, datum, fitted features, the kernel residual spread and its worst sample, and the capability study — while the frozen `InspectionResult` projects only `InspectionFeature` atoms. `Probe.Inspect` writes conformance counts and the worst deviation through `FabricationInstruments.ProbeFeatures` and `ProbeDeviation` from that file-scoped report. The worst deviation reads the census's own ranked sample, so the instrument, the result, and the kernel ranking are ONE quantity and no seeded fold stands beside them. The whole fold runs inside the `FabricationEngine.Probe` bracket the run spine's `SpanBand` opens, with `EnginePhase.DatumRegistered` and `EnginePhase.FeaturesFitted` as its span marks; the settled datum alignment writes the ICP iteration count through `FabricationInstruments.Steps`.
- Packages: `Rasm.Analysis` (`Analyze.Run`, `AnalysisQuery.Conformance`, `ConformanceMetric`, `ResidualSample`, `Distribution`), `Rasm.Solving` (`Fit.Apply`, `FitKind`, `FitOp`, `FitPolicy`, `Fitted.Inliers`), `Rasm.Processing` (`AlignKind.AlignDetailed`, `Alignment`), `Rasm.Spatial` (`VectorCloud.Cluster`), `Rasm.Domain` (`ToleranceLane.Neglect` through `Context.For`, `ValidityClaim`, `FabricationCanon.Ordered`), `Rasm.Numerics` (`Dimension`, `PositiveMagnitude`).
- Boundary: one residual tranche feeds both consumers — `Capability.Assess(new CapabilityStudy.Variables(...), tolerance)` for the SPC study and the kernel `AnalysisQuery.Conformance` measured arity for the run's own statistics, whose `Distribution` row carries the public `Stat` summary beside median and interquartile range. Band conformance derives per sample from the tolerance each `ResidualSample` already carries and lands on `InspectionFeature.Pass`, so no second kernel reach and no package-local mean, RMS, or quantile fold stands beside the rows; a local QIF-shaped record claiming a standard contract the package does not admit is the deleted form.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DatumPolicy {
    private DatumPolicy() { }

    public sealed record Setup(DatumLineage Datum, Transform Registration) : DatumPolicy;
    public sealed record BestFit(DatumLineage Datum, AlignKind Kind, AlignmentPolicy Policy) : DatumPolicy;
    public sealed record Substitute(
        DatumLineage Datum,
        Seq<FitKind> Kinds,
        FitPolicy FitPolicy,
        AlignKind Registration,
        AlignmentPolicy Alignment) : DatumPolicy;
    public sealed record Replay(DatumLineage Datum, Transform Registration, double DeltaMm, double RadiusMm) : DatumPolicy;

    public DatumLineage Lineage => Switch(
        setup: static row => row.Datum,
        bestFit: static row => row.Datum,
        substitute: static row => row.Datum,
        replay: static row => row.Datum);
}

public static class ProbeMemo {
    public static UInt128 Key(
        Seq<(Point3d Measured, Point3d Nominal)> pairs, AlignKind kind, AlignmentPolicy policy, Context context) =>
        FabricationCanon.Ordered(context, writer => writer
            .Rows(pairs, static (row, pair) => row.Coords(pair.Measured).Coords(pair.Nominal))
            .String(kind.Key)
            .I64(policy.MaxIterations.Value)
            .String(policy.Convergence.Key).String(policy.Residual.Key)
            .String(policy.Step.Key).String(policy.Ridge.Key)
            .Double(policy.RobustScale.Value)
            .I64(policy.OptimizerBudget.Value)
            .String(policy.Fit.Key.ToString())
            .Maybe(policy.TrimFraction.Map(static trim => trim.Value), static (row, trim) => row.Double(trim))
            .I64(policy.CoarseLevels.Value)
            .Double(context.Relative.Value).Double(context.Angle.Value).Double(context.Unit.MetersPerUnit));
}

[ComplexValueObject]
public sealed partial class InspectPolicy {
    public Seq<ProbePlan> Plans { get; }
    public MeasurementSource Source { get; }
    public DatumPolicy Datum { get; }
    public StylusCalibration Calibration { get; }
    public RepeatBands Repeat { get; }
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
        ref RepeatBands repeat,
        ref FitPolicy featureFit,
        ref Option<CapabilityTolerance> capability,
        ref IClock clock) {
        if (plans.IsEmpty || plans.Map(static row => row.Key).Distinct().Count != plans.Count)
            validationError = new ValidationError("probe:policy");
    }

    public static Fin<InspectPolicy> Admit(
        Seq<ProbePlan> plans,
        MeasurementSource source,
        DatumPolicy datum,
        StylusCalibration calibration,
        RepeatBands repeat,
        FitPolicy featureFit,
        Option<CapabilityTolerance> capability,
        IClock clock) =>
        Validate(plans, source, datum, calibration, repeat, featureFit, capability, clock, out InspectPolicy policy)
            .Admitted(policy);
}

public sealed record MeasuredFeature(
    ProbeTargetKey Key,
    ProbePlan Plan,
    Point3d Nominal,
    Point3d Measured,
    Vector3d SurfaceNormal,
    double SignedDeviationMm,
    ResidualSample Residual,
    double UncertaintyMm,
    double RepeatabilityMm,
    Instant At,
    Option<Fitted> Fit);

file readonly record struct RegistrationSpread(double DeltaMm, double RadiusMm) {
    public double At(double leverArmMm) =>
        Math.Sqrt(Squared(DeltaMm) + Squared(DeltaMm * leverArmMm / RadiusMm));

    private static double Squared(double value) => value * value;
}

file sealed record ProbeDatum(
    DatumLineage Datum,
    Transform Registration,
    Point3d Origin,
    Option<RegistrationSpread> Spread,
    Option<Alignment> Alignment,
    Option<Fitted> Fit);

file sealed record ProbeReport(
    UInt128 SourceEvidence,
    Seq<ProbeTouch> Cycles,
    ProbeDatum Datum,
    Seq<MeasuredFeature> Features,
    Distribution Residuals,
    ResidualSample Worst,
    Option<CapabilityReport> Capability,
    Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Probe {
    private const double MadConsistency = 1.4826;

    internal static readonly Op ProbeOp = Op.Of(name: "fabrication:probe");

    public static Fin<FabricationResult> Inspect(
        InspectPolicy policy, FabricationInput input, Option<InstrumentSet> set = default, Option<SpanBand> band = default) =>
        band.Traced(FabricationEngine.Probe, ProbeOp, span =>
            from context in Context.Millimeters().ToFin()
            from _policy in AdmitPolicy(policy)
            from targets in Targets(policy, context)
            from _targets in AdmitTargets(policy, targets)
            let observed = Index(policy.Source.Rows, static row => row.Address.Target)
            let cycles = targets.Bind(target => Evaluate(target, observed, policy, context))
            let contacted = Index(cycles, static row => row.Target.Key)
            from measured in (
                targets.Traverse(target => Aggregate(target, contacted, policy, context).ToValidation()),
                RequiredContacts(targets, contacted).ToValidation())
                .Apply(static (rows, _) => rows).As().ToFin()
            let unregistered = measured.Bind(static row => row.ToSeq())
            from datum in unregistered.Head
                .ToFin(FabricationFault.Inadmissible(FabConcern.Verify, "probe:no-measurements"))
                .Bind(_ => Reconcile(policy.Datum, unregistered, context))
            let _registered = FabricationTrace.Mark(span, EnginePhase.DatumRegistered)
            from _icp in datum.Alignment.Map(alignment => set.Steps((EnginePhase.IcpIterations, alignment.Iterations))).IfNone(Fin.Succ(unit))
            let transformed = TransformFeatures(unregistered, datum)
            from features in Fits(transformed, policy.FeatureFit, context)
            let _fitted = FabricationTrace.Mark(span, EnginePhase.FeaturesFitted)
            from census in Census(features.Map(static row => row.Residual))
            from capability in policy.Capability
                .Traverse(demand => Capability.Assess(
                    new CapabilityStudy.Variables(features.Map(static row => row.Residual)), demand, set))
                .As()
            let report = new ProbeReport(
                policy.Source.EvidenceKey,
                cycles,
                datum,
                features,
                census.Spread,
                census.Worst,
                capability,
                policy.Clock.GetCurrentInstant())
            from result in ToResult(report, input.Sources + input.ParentRuns, set)
            select result);

    private static Fin<Seq<ProbeTarget>> Targets(InspectPolicy policy, Context context) =>
        policy.Plans.TraverseM(plan =>
                plan.Feature.Project(plan.Count.Value, context).Bind(samples =>
                    samples.Map((sample, index) =>
                        ProbeTargetKey.Admit(plan.Cycle, plan.Key.Value, index).Map(key => {
                            Vector3d outward = Unit(sample.SurfaceNormal);
                            Vector3d direction = plan.Cycle.Approach(outward);
                            Point3d start = sample.Nominal - (direction * plan.ClearanceMm);
                            return new ProbeTarget(
                                key, plan, sample.Nominal, outward, direction,
                                start, start + (direction * plan.TravelLimitMm.Value));
                        }))
                    .Traverse(identity).As()))
            .As()
            .Map(static rows => rows.Bind(identity));

    private static HashMap<TKey, Seq<TRow>> Index<TKey, TRow>(Seq<TRow> rows, Func<TRow, TKey> key) =>
        rows.Fold(
            HashMap<TKey, Seq<TRow>>(),
            (map, row) => map.AddOrUpdate(key(row), existing => existing.Add(row), Seq(row)));

    private static Fin<Unit> AdmitPolicy(InspectPolicy policy) =>
        (AdmissionSlots.Gate(policy.Source.Rows.ForAll(row => policy.Source.Window.Contains(row.At)),
            FabConcern.Verify, "probe:source-window", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(policy.Source.Rows.ForAll(row => row.EvidenceKey == policy.Source.EvidenceKey),
             FabConcern.Verify, "probe:evidence-identity", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(policy.Source.Rows.ForAll(row => policy.Calibration.Validity.Contains(row.At)),
             FabConcern.Verify, "probe:calibration-window", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(DatumValid(policy.Datum) && policy.Datum.Lineage.Traceable,
             FabConcern.Verify, "probe:datum-traceability", FabricationFault.Inadmissible))
        .Apply(static (_, _, _, _) => unit)
        .As()
        .ToFin();

    private static Fin<Unit> AdmitTargets(InspectPolicy policy, Seq<ProbeTarget> targets) =>
        (AdmissionSlots.Gate(targets.Count == policy.Plans.Sum(static row => row.Count)
             && policy.Plans.ForAll(plan => plan.Attempts.Value >= policy.Repeat.MinimumAccepted.Value),
                 FabConcern.Verify, "probe:target-count", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(targets.Map(static row => row.Key).Distinct().Count == targets.Count,
             FabConcern.Verify, "probe:target-key", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(
             policy.Source.Rows.Map(static row => row.Address).Distinct().Count == policy.Source.Rows.Count
             && policy.Source.Rows.ForAll(row => targets.Exists(target =>
                 target.Key == row.Address.Target && row.Address.Attempt < target.Plan.Attempts.Value)),
                     FabConcern.Verify, "probe:observation-address", FabricationFault.Inadmissible))
        .Apply(static (_, _, _) => unit)
        .As()
        .ToFin();


    private static bool DatumValid(DatumPolicy policy) => policy.Switch(
        setup: static row => row.Registration.IsValid,
        bestFit: static _ => true,
        substitute: static row => !row.Kinds.IsEmpty && row.Kinds.Distinct().Count == row.Kinds.Count,
        replay: static row => row.Registration.IsValid && row.DeltaMm >= 0.0 && row.RadiusMm > 0.0);

    private static Seq<ProbeTouch> Evaluate(
        ProbeTarget target,
        HashMap<ProbeTargetKey, Seq<ProbeObservation>> observed,
        InspectPolicy policy,
        Context model) {
        Seq<ProbeObservation> rows = observed.Find(target.Key)
            .Map(static found => toSeq(found.OrderBy(static row => row.Address.Attempt).ThenBy(static row => row.At)))
            .IfNone(Seq<ProbeObservation>());
        return rows.IsEmpty
            ? Seq(new ProbeTouch(target, new ProbeOutcome.Missed()))
            : rows.Map(row => Evaluate(target, row, policy.Calibration, model));
    }

    private static ProbeTouch Evaluate(
        ProbeTarget target,
        ProbeObservation observation,
        StylusCalibration calibration,
        Context model) {
        Vector3d displacement = observation.BallCenter - target.Start;
        double travel = displacement * target.Direction;
        double lateral = (displacement - (target.Direction * travel)).Length;
        return (travel, lateral) switch {
            (var axial, _) when axial > target.Plan.TravelLimitMm.Value => new ProbeTouch(
                target,
                new ProbeOutcome.Rejected(observation, ProbeRejection.Overtravel, axial, target.Plan.TravelLimitMm.Value)),
            (var axial, _) when axial < 0.0 => new ProbeTouch(
                target,
                new ProbeOutcome.Rejected(observation, ProbeRejection.ShortOfSurface, axial, 0.0)),
            (_, var radial) when radial > target.Plan.Approach.Value => new ProbeTouch(
                target,
                new ProbeOutcome.Rejected(observation, ProbeRejection.LateralDrift, radial, target.Plan.Approach.Value)),
            _ => new ProbeTouch(target, Compensate(target, observation, calibration, model)),
        };
    }

    private static ProbeOutcome Compensate(
        ProbeTarget target,
        ProbeObservation observation,
        StylusCalibration calibration,
        Context model) {
        Point3d surface = observation.BallCenter
            + (target.Direction
                * (calibration.RadiusMm.Value - calibration.PreTravelMm + calibration.LobeMm(target.Direction, model)));
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
        HashMap<ProbeTargetKey, Seq<ProbeTouch>> contacted,
        InspectPolicy policy,
        Context model) {
        Seq<CompensatedContact> rows = contacted.Find(target.Key).IfNone(Seq<ProbeTouch>())
            .Bind(static cycle => cycle.Outcome.Contact.ToSeq());
        if (rows.IsEmpty) return Fin.Succ(Option<UnregisteredFeature>.None);

        Point3d centre = MedianPoint(rows.Map(static row => row.Point));
        Seq<double> distances = rows.Map(row => row.Point.DistanceTo(centre));
        double median = Statistics.Median(distances);
        double deviation = Statistics.Median(distances.Map(value => Math.Abs(value - median)));
        double band = policy.Repeat.OutlierSigma.Value
            * Math.Max(deviation * MadConsistency, model.For(ToleranceLane.Neglect).Value);
        Seq<CompensatedContact> accepted = rows.Filter(row => row.Point.DistanceTo(centre) <= median + band);
        if (accepted.Count < policy.Repeat.MinimumAccepted.Value)
            return Fin.Fail<Option<UnregisteredFeature>>(
                FabricationFault.Inadmissible(FabConcern.Verify, "probe:repeatability"));

        Point3d measured = MeanPoint(accepted.Map(static row => row.Point));
        double repeatability = Statistics.RootMeanSquare(accepted.Map(row => row.Point.DistanceTo(measured)));
        double thermal = accepted.Map(static row => row.ThermalUncertaintyMm)
            .Fold(Option<double>.None, static (held, value) =>
                Some(held.Match(Some: peak => Math.Max(peak, value), None: () => value)))
            .IfNone(0.0);
        double uncertainty = Math.Sqrt(
            Squared(policy.Calibration.CalibrationUncertaintyMm)
            + Squared(policy.Repeat.MinimumUncertainty.Value)
            + Squared(repeatability)
            + Squared(thermal));
        Instant at = accepted.Fold(Option<Instant>.None, static (latest, row) =>
            Some(latest.Match(Some: held => held >= row.At ? held : row.At, None: () => row.At)))
            .IfNone(policy.Clock.GetCurrentInstant());
        return Fin.Succ(Some(new UnregisteredFeature(target, measured, repeatability, uncertainty, at)));
    }

    private static Fin<Unit> RequiredContacts(
        Seq<ProbeTarget> targets,
        HashMap<ProbeTargetKey, Seq<ProbeTouch>> contacted) {
        Seq<Error> errors = targets
            .Filter(static target => target.Plan.Cycle.RequiresHit)
            .Choose(target => {
                Seq<ProbeTouch> cycles = contacted.Find(target.Key).IfNone(Seq<ProbeTouch>());
                return cycles.Exists(static cycle => cycle.Outcome.Contact.IsSome)
                    ? Option<Error>.None
                    : Some(cycles.Choose(static cycle => cycle.Outcome.Fault).Head
                        .IfNone(FabricationFault.Inadmissible(FabConcern.Verify, "probe:required-contact")));
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
        setup: static (state, row) => row.Registration.IsValid
            ? Fin.Succ(new ProbeDatum(
                row.Datum, row.Registration, Centroid(state.Features), None, None, None))
            : Fin.Fail<ProbeDatum>(FabricationFault.Inadmissible(FabConcern.Verify, "probe:setup-transform")),
        bestFit: static (state, row) => Align(state.Features, state.Context, row.Kind, row.Policy)
            .Map(aligned => Seated(row.Datum, aligned, state.Features, None, state.Context)),
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
            let inliers = fit.Inliers.ToSeq().Map(index => state.Features[index])
            from aligned in Align(inliers, state.Context, row.Registration, row.Alignment)
            select Seated(row.Datum, aligned, inliers, Some(fit), state.Context),
        replay: static (state, row) => row.Registration.IsValid
            ? Fin.Succ(new ProbeDatum(
                row.Datum, row.Registration, Centroid(state.Features),
                Spread(row.DeltaMm, row.RadiusMm, state.Context), None, None))
            : Fin.Fail<ProbeDatum>(FabricationFault.Inadmissible(FabConcern.Verify, "probe:replay-transform")));

    private static Option<RegistrationSpread> Spread(double deltaMm, double radiusMm, Context model) =>
        radiusMm > model.For(ToleranceLane.Neglect).Value
            ? Some(new RegistrationSpread(deltaMm, radiusMm))
            : None;

    private static ProbeDatum Seated(
        DatumLineage datum,
        (Transform Transform, Alignment Alignment) aligned,
        Seq<UnregisteredFeature> registered,
        Option<Fitted> fit,
        Context model) {
        Point3d origin = Centroid(registered);
        double radius = Statistics.RootMeanSquare(registered.Map(row => row.Measured.DistanceTo(origin)));
        return new ProbeDatum(
            datum,
            aligned.Transform,
            origin,
            Spread(aligned.Alignment.FinalDelta, radius, model),
            Some(aligned.Alignment),
            fit);
    }

    private static Point3d Centroid(Seq<UnregisteredFeature> features) =>
        MeanPoint(features.Map(static row => row.Measured));

    private static Fin<(Transform Transform, Alignment Alignment)> Align(
        Seq<UnregisteredFeature> features,
        Context context,
        AlignKind kind,
        AlignmentPolicy policy) {
        Fin<VectorCloud> source = VectorCloud.Cluster(features.Map(static row => row.Measured), context, key: ProbeOp);
        Fin<VectorCloud> target = VectorCloud.Cluster(features.Map(static row => row.Target.Nominal), context, key: ProbeOp);
        try {
            return from measured in source
                   from nominal in target
                   from alignment in kind.AlignDetailed(measured, nominal, policy, ProbeOp)
                   from transform in alignment.Project<Transform>(ProbeOp)
                   select (transform, alignment);
        } finally {
            (source.ToSeq() + target.ToSeq()).Iter(static cloud => cloud.Dispose());
        }
    }

    private static Seq<MeasuredFeature> TransformFeatures(Seq<UnregisteredFeature> rows, ProbeDatum datum) =>
        rows.Map((row, index) => {
            Point3d measured = Apply(datum.Registration, row.Measured);
            double signed = (measured - row.Target.Nominal) * row.Target.SurfaceNormal;
            double registration = datum.Spread
                .Map(spread => spread.At(measured.DistanceTo(datum.Origin)))
                .IfNone(0.0);
            double uncertainty = Math.Sqrt(
                Squared(row.MeasurementUncertaintyMm)
                + Squared(registration)
                + datum.Fit.Map(static fitted => Squared(fitted.Residual)).IfNone(0.0));
            return new MeasuredFeature(
                row.Target.Key,
                row.Target.Plan,
                row.Target.Nominal,
                measured,
                row.Target.SurfaceNormal,
                signed,
                new ResidualSample(index, row.Target.Nominal, Math.Abs(signed), row.Target.Plan.Band.Millimeters),
                uncertainty,
                row.RepeatabilityMm,
                row.At,
                None);
        });

    private static Fin<(Distribution Spread, ResidualSample Worst)> Census(Seq<ResidualSample> residuals) =>
        from spread in Measured<Distribution>(ConformanceMetric.Distribution, residuals)
        from worst in Measured<ResidualSample>(ConformanceMetric.Maximum, residuals)
        select (spread, worst);

    private static Fin<TOut> Measured<TOut>(ConformanceMetric metric, Seq<ResidualSample> residuals) where TOut : notnull =>
        AnalysisQuery.Conformance(metric)
            .Bind(query => Analyze.Run<ResidualSample, TOut>(query, residuals.ToArray()).ToFin())
            .Bind(values => values.Head.ToFin(
                FabricationFault.Inadmissible(FabConcern.Verify, "probe:residual-census")));

    private static Fin<Seq<MeasuredFeature>> Fits(Seq<MeasuredFeature> features, FitPolicy policy, Context context) =>
        toSeq(Index(features, static feature => feature.Plan.Key))
            .OrderBy(static entry => entry.Key.Value)
            .ToSeq()
            .TraverseM(entry => Fitted(entry.Value, policy, context).Map(fitted => (entry.Key, Fit: fitted)))
            .As()
            .Map(static rows => toMap(rows))
            .Map(fits => features.Map(row =>
                row with { Fit = fits.Find(row.Plan.Key).Bind(identity) }));

    private static Fin<Option<Fitted>> Fitted(Seq<MeasuredFeature> group, FitPolicy policy, Context context) =>
        group.Head
            .ToFin(FabricationFault.Inadmissible(FabConcern.Verify, "probe:fit-group"))
            .Bind(head => {
                Seq<MeasuredFeature> eligible = group.Filter(row =>
                    head.Plan.Feature.FitEligible(row.SurfaceNormal, context.Absolute.Value));
                return head.Plan.Feature.Spec.Fit
                    .Filter(kind => eligible.Count >= kind.MinimalSamples)
                    .Traverse(kind => Fit.Apply(
                        new FitOp(
                            Seq(kind),
                            eligible.Map(static row => row.Measured).ToArray(),
                            kind.NeedsNormals ? Some(eligible.Map(static row => row.SurfaceNormal).ToArray()) : None,
                            policy),
                        context,
                        ProbeOp))
                    .As();
            });

    private static Fin<FabricationResult> ToResult(ProbeReport report, Seq<ContentKey> subjects, Option<InstrumentSet> set) =>
        from atoms in report.Features.TraverseM(ToAtom).As()
        let conforming = atoms.Filter(static row => row.Pass.IfNone(false)).Count
        from _pass in set.Write(FabricationInstruments.ProbeFeatures, conforming, (FabricationInstruments.VerdictSlot, FabricationInstruments.Pass))
        from _fail in set.Write(FabricationInstruments.ProbeFeatures, atoms.Count - conforming, (FabricationInstruments.VerdictSlot, FabricationInstruments.Fail))
        from _worst in set.Write(FabricationInstruments.ProbeDeviation, report.Worst.Distance)
        select (FabricationResult)new FabricationResult.InspectionResult(atoms, subjects.Distinct());

    private static Fin<InspectionFeature> ToAtom(MeasuredFeature feature) =>
        InspectionFeature.Admit(
            PropertyCategory.Fabrication.Row(feature.Key.Text),
            feature.Nominal,
            feature.Measured,
            Some(feature.Plan.Band.Millimeters),
            feature.UncertaintyMm,
            InspectionMethod.Probe);

    private static Point3d Apply(Transform transform, Point3d point) {
        point.Transform(transform);
        return point;
    }

    private static Point3d MedianPoint(Seq<Point3d> points) => new(
        Statistics.Median(points.Map(static point => point.X)),
        Statistics.Median(points.Map(static point => point.Y)),
        Statistics.Median(points.Map(static point => point.Z)));

    private static Point3d MeanPoint(Seq<Point3d> points) => new(
        Statistics.Mean(points.Map(static point => point.X)),
        Statistics.Mean(points.Map(static point => point.Y)),
        Statistics.Mean(points.Map(static point => point.Z)));

    private static double Squared(double value) => value * value;

    internal static Vector3d Unit(Vector3d direction) {
        _ = direction.Unitize();
        return direction;
    }
}

public static class ProbeBench {
    public const int FeatureFloor = 64;

    public static Fin<(InspectPolicy Policy, FabricationInput Input)> Workload(InspectPolicy policy, FabricationInput input) =>
        policy.Datum is DatumPolicy.BestFit
        && policy.Plans.Sum(static row => row.Count) >= FeatureFloor
            ? Fin.Succ((policy, input))
            : Fin.Fail<(InspectPolicy, FabricationInput)>(
                FabricationFault.Inadmissible(FabConcern.Verify, "bench:icp-probe-fit"));

    public static Fin<FabricationResult> Run((InspectPolicy Policy, FabricationInput Input) workload) =>
        Probe.Inspect(workload.Policy, workload.Input);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
