# [RASM_FABRICATION_PROBING]

`Probe.Inspect` owns post-cycle metrology truth: one admitted `InspectPolicy` generates feature-complete contact targets, correlates exact controller cycles with repeat observations, compensates calibrated stylus behavior in the probe's own frame, reconciles datum registration with its anisotropic uncertainty, and projects transformed residuals onto `FabricationResult.InspectionResult`.

`FabricationPolicy.Inspect`, `GCommand`, `DatumReceipt`, `FitReceipt`, `Capability.Assess`, and `InspectionFeature` remain frozen seams. Contact generation composes the kernel `Deterministic` equidistribution owner, robust aggregation composes `MathNet.Numerics.Statistics`, primitive fitting composes the kernel `FitKind` roster, and residual statistics compose `AnalysisQuery.Conformance`; this page mints no draw sequence, no summary statistic, and no fit primitive of its own. Decoded measurement rows enter as typed data; controller transport and work-offset mutation remain outside the Verify plane.

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
- Law: three features carry no substitute fit, each for its own settled reason. `Circle` and `Slot` await the kernel `FitKind.Circle` row — the kernel primitive roster carries plane, sphere, cylinder, cone, torus, and line, and forcing a circle's coplanar contacts through the six-parameter cylinder leaves the axis direction unconstrained, so the solve is rank-deficient rather than substituted; a slot is two end arcs and two parallel flanks sharing one width, which composes from a circle row and a line row once that member lands. `Surface` carries no fit by its own nature: a free-form feature is measured as deviation to its nominal geometry and no primitive stands in for it. All three answer per-contact residuals meanwhile, and a page-local fit body is a second fitting owner.
- Entry: `Probe.Inspect` is the sole public operation; each generated `ProbeTarget` carries the exact `ProbeTargetKey` whose one `Text` spelling posting, telemetry, residuals, and result identity all read. Every owner admits through its generated `Validate` onto the `Admission.Admitted` bridge; a throwing `Create` at a construction site is the deleted form.
- Exemption: `ContactChart.Allocate` is a statement kernel — integer budget allocation with floors and a rounding residue has no expression form that spends the budget exactly.
- Auto: one `Validation<Error, Unit>` fan-in proves feature coverage, target uniqueness, observation references, evidence identity, and datum traceability, so an inadmissible demand reports every violated invariant rather than the first.
- Growth: a feature sub-kind is one `ProbeFeature` case, one `ContactSource` arm, and one `FeatureSpec` row; no feature-specific inspection entrypoint and no generator body survives beside it.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
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

// Which contacts feed the substitute fit. A cylinder is fitted from WALL contacts, so cap contacts are excluded by
// perpendicularity; a one-face plane is fitted from contacts ALIGNED with that face's normal, so the opposite face
// is excluded by alignment. Both tests run against the admitted context tolerance, never a machine-epsilon literal.
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

// Parameter draws over the unit square. `Lattice` spreads a near-square grid where both chart axes carry
// independent extent; `Equidistributed` reads the kernel `Deterministic.Hammersley` pair, which strides one axis
// uniformly and digit-reverses the second — the owner states that equidistribution is its own member family and
// never a consumer-page kernel, so a page-local golden-angle constant is the deleted form.
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

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// One parametric contact surface: a chart over the unit square, the sampler that draws its parameters, its own
// measure for a composite's area share, and the contact floor its substitute fit demands.
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

    // A rectangle centred on its frame origin: `u` runs the frame X extent and `v` the frame Y extent.
    public static ContactChart Rectangle(Plane frame, double width, double height, Vector3d normal, int floor) =>
        new(ContactSampler.Lattice, width * height, floor,
            (u, v) => new FeatureSample(
                frame.Origin + (frame.XAxis * ((u - 0.5) * width)) + (frame.YAxis * ((v - 0.5) * height)),
                Probe.Unit(normal)));

    // A wall of revolution: `u` runs the axial fraction, `v` the sweep. `radiusAt` carries the taper, so a
    // cylinder and a cone are one chart under two radius laws, and a ring is the zero-height degenerate case.
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

    // A disc at an axial offset: `u` is the radial fraction under an equal-area square root, `v` the sweep.
    public static ContactChart Disc(Plane frame, double radius, double atHeight, Vector3d normal, int floor) =>
        new(ContactSampler.Equidistributed, Math.PI * radius * radius, floor,
            (u, v) => {
                double angle = Math.Tau * v;
                Vector3d radial = (frame.XAxis * Math.Cos(angle)) + (frame.YAxis * Math.Sin(angle));
                return new FeatureSample(
                    frame.Origin + (frame.ZAxis * atHeight) + (radial * (radius * Math.Sqrt(u))),
                    Probe.Unit(normal));
            });

    // A sphere: `u` is the equal-area height fraction and `v` the azimuth.
    public static ContactChart Ball(Point3d centre, double radius, int floor) =>
        new(ContactSampler.Equidistributed, 2.0 * Math.Tau * radius * radius, floor,
            (u, v) => {
                double z = 1.0 - (2.0 * u);
                double azimuth = Math.Tau * v;
                double planar = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z)));
                Vector3d normal = new(planar * Math.Cos(azimuth), planar * Math.Sin(azimuth), z);
                return new FeatureSample(centre + (normal * radius), normal);
            });

    // A torus: `u` sweeps the major circle and `v` the tube.
    public static ContactChart Tube(Plane frame, double major, double minor, ProbeSense sense, int floor) =>
        new(ContactSampler.Equidistributed, Math.Tau * major * Math.Tau * minor, floor,
            (u, v) => {
                double sweep = Math.Tau * u;
                double tube = Math.Tau * v;
                Vector3d radial = (frame.XAxis * Math.Cos(sweep)) + (frame.YAxis * Math.Sin(sweep));
                Vector3d normal = (radial * Math.Cos(tube)) + (frame.ZAxis * Math.Sin(tube));
                return new FeatureSample(frame.Origin + (radial * major) + (normal * minor), sense.Orient(normal));
            });

    // An arc-length reparameterization of a measured polyline: `u` is the normalized distance along it.
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

    // The ONE contact-budget rule every composite reads: each chart takes its declared floor first, the remainder
    // distributes by measure share, and the largest share absorbs the rounding residue, so a composite spends its
    // budget exactly and a fit-bearing chart never falls below its solver's minimal set. Integer allocation with
    // floors and a residue has no expression form that spends the budget exactly, so the body is the declared
    // statement kernel; a per-feature clamp beside it is the deleted form.
    internal static Fin<Seq<(ContactChart Chart, int Count)>> Allocate(Seq<ContactChart> charts, int count) {
        if (charts.Count == 1) return Fin.Succ(Seq((charts[0], count)));
        int floors = charts.Sum(static chart => chart.Floor);
        if (count < floors)
            return Fin.Fail<Seq<(ContactChart, int)>>(
                new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:contact-budget"));

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

// Contact generation has exactly two shapes and the feature declares which it is: a CHARTED feature draws from
// its own chart set, and an EXTRACTED one delegates to the admitted sampler over its domain. One per-case table
// names the shape and its data, so a new feature is one arm and never a new generator body.
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

    // Contact cardinality, the substitute-fit kind, and the filter naming which contacts feed it are one row per
    // case, so a new feature declares its whole metrology contract in one arm and every consumer derives. Floors
    // read the solver's own `MinimalSamples` rather than restating an integer; an absent ceiling is `None`, never
    // a sentinel, because only a point feature has a real upper bound on its contact count.
    internal FeatureSpec Spec => Switch(
        point: static _ => new FeatureSpec(1, Some(1), None, FitFilter.All, None),
        line: static _ => new FeatureSpec(FitKind.Line.MinimalSamples, None, Some(FitKind.Line), FitFilter.All, None),
        plane: static _ => new FeatureSpec(FitKind.Plane.MinimalSamples, None, Some(FitKind.Plane), FitFilter.All, None),
        // Three points in the probe plane determine the circle. No substitute fit is named: the kernel primitive
        // roster carries no circle, and forcing coplanar contacts through the six-parameter cylinder leaves the
        // axis direction unconstrained, so the solve is rank-deficient rather than substituted.
        circle: static _ => new FeatureSpec(3, None, None, FitFilter.All, None),
        bore: static row => new FeatureSpec(
            FitKind.Cylinder.MinimalSamples + 1, None, Some(FitKind.Cylinder), FitFilter.PerpendicularTo, Some(row.Frame.ZAxis)),
        boss: static row => new FeatureSpec(
            FitKind.Cylinder.MinimalSamples + 1, None, Some(FitKind.Cylinder), FitFilter.PerpendicularTo, Some(row.Frame.ZAxis)),
        // Two end arcs and two parallel flanks sharing one width: a constrained composite, not a primitive. It
        // composes from a circle row and a line row once the kernel carries the circle, so the slot answers
        // per-contact residuals meanwhile and a page-local composite fit would be a second fitting owner.
        slot: static _ => new FeatureSpec(5, None, None, FitFilter.All, None),
        // A plane fitted across both antiparallel faces returns the mid-plane, which is no measured face, so the
        // fit takes the contacts aligned with one face alone: that face's minimal set plus one contact proving
        // the opposite face was reached.
        web: static row => new FeatureSpec(
            FitKind.Plane.MinimalSamples + 1, None, Some(FitKind.Plane), FitFilter.AlignedWith, Some(row.Frame.ZAxis)),
        sphere: static _ => new FeatureSpec(FitKind.Sphere.MinimalSamples, None, Some(FitKind.Sphere), FitFilter.All, None),
        cylinder: static _ => new FeatureSpec(FitKind.Cylinder.MinimalSamples, None, Some(FitKind.Cylinder), FitFilter.All, None),
        cone: static _ => new FeatureSpec(FitKind.Cone.MinimalSamples, None, Some(FitKind.Cone), FitFilter.All, None),
        torus: static _ => new FeatureSpec(FitKind.Torus.MinimalSamples, None, Some(FitKind.Torus), FitFilter.All, None),
        profile: static row => new FeatureSpec(Math.Min(row.Samples.Count, 2), None, None, FitFilter.All, None),
        // A free-form surface is measured as deviation to its NOMINAL geometry, so no primitive substitutes for
        // it and the absent fit is the feature's own nature rather than a gap.
        surface: static _ => new FeatureSpec(3, None, None, FitFilter.All, None));

    // The per-case contact declaration: chart data, never a generator body.
    internal ContactSource Source => Switch(
        point: static row => (ContactSource)new ContactSource.Charted(Seq(ContactChart.Constant(row.Nominal, row.Normal))),
        line: static row => new ContactSource.Charted(Seq(ContactChart.Span(row.Nominal.From, row.Nominal.To, row.Normal))),
        plane: static row => new ContactSource.Charted(Seq(
            ContactChart.Rectangle(row.Frame, row.WidthMm, row.HeightMm, row.Frame.ZAxis, FitKind.Plane.MinimalSamples))),
        circle: static row => new ContactSource.Charted(Seq(
            ContactChart.Wall(row.Frame, 0.0, ProbeSense.Outside, _ => row.RadiusMm, static radial => radial, floor: 3))),
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
        // A cone's surface normal tilts off the radial by the half-angle the base radius and height define.
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
        point: static row => row.Nominal.IsValid && Direction(row.Normal).IsSome,
        line: static row => row.Nominal.IsValid && Direction(row.Normal).IsSome,
        plane: static row => row.Frame.IsValid && Positive(row.WidthMm, row.HeightMm),
        circle: static row => row.Frame.IsValid && Positive(row.RadiusMm),
        bore: static row => row.Frame.IsValid && Positive(row.DiameterMm, row.DepthMm),
        boss: static row => row.Frame.IsValid && Positive(row.DiameterMm, row.HeightMm),
        slot: static row => row.Frame.IsValid && Positive(row.LengthMm, row.WidthMm, row.DepthMm) && row.LengthMm > row.WidthMm,
        web: static row => row.Frame.IsValid && Positive(row.LengthMm, row.HeightMm, row.ThicknessMm),
        sphere: static row => row.Center.IsValid && Positive(row.RadiusMm),
        cylinder: static row => row.Frame.IsValid && Positive(row.RadiusMm, row.HeightMm),
        cone: static row => row.Frame.IsValid && Positive(row.BaseRadiusMm, row.HeightMm),
        torus: static row => row.Frame.IsValid && Positive(row.MajorRadiusMm, row.MinorRadiusMm)
            && row.MajorRadiusMm > row.MinorRadiusMm,
        profile: static row => row.Samples.Count >= 2
            && row.Samples.ForAll(static sample => sample.Nominal.IsValid && Direction(sample.SurfaceNormal).IsSome)
            && row.Samples.AsIterable().Zip(row.Samples.AsIterable().Skip(1),
                static (from, to) => from.Nominal.DistanceTo(to.Nominal)).Fold(0.0, static (sum, value) => sum + value) > 0.0,
        surface: static row => Direction(row.Normal).IsSome);

    internal Fin<Seq<FeatureSample>> Project(int count, Context context) => Source.Switch(
        state: (Count: count, Context: context),
        charted: static (state, row) => ContactChart.Allocate(row.Charts, state.Count)
            .Map(static allocated => allocated.Bind(static row =>
                row.Chart.Sampler.Draw(row.Count).Map(pair => row.Chart.At(pair.U, pair.V)))),
        extracted: static (state, row) => row.Sampling.Project<Seq<Point3d>>(row.Domain, state.Context)
            .Map(points => points.Take(state.Count).Map(point => new FeatureSample(point, Probe.Unit(row.Normal))).ToSeq()));

    // A slot is four walls and a floor. Each wall carries its own frame so one rectangle chart serves every face,
    // and the inward normal is the direction the stylus pushes against that wall.
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

    private static Option<Vector3d> Direction(Vector3d value) {
        Vector3d copy = value;
        return copy.IsValid && copy.Unitize() ? Some(copy) : None;
    }

    private static bool Positive(params double[] values) =>
        values.All(static value => double.IsFinite(value) && value > 0.0);
}

public readonly record struct FeatureSample(Point3d Nominal, Vector3d SurfaceNormal);

public readonly record struct FeatureSpec(
    int Minimum,
    Option<int> Maximum,
    Option<FitKind> Fit,
    FitFilter Filter,
    Option<Vector3d> FitAxis);

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ProbeTargetKey {
    public ProbeCycle Cycle { get; }
    public int Feature { get; }
    public int Sample { get; }

    // One spelling serves posting, telemetry correlation, residual rows, and egress identity; a caller
    // re-joining the three parts at its own call site forks the wire token.
    public string Text => $"{Cycle.Key}:{Feature}:{Sample}";

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ProbeCycle cycle,
        ref int feature,
        ref int sample) {
        if (!(Witness.Index(feature) && Witness.Index(sample)))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:target-key");
    }

    public static Fin<ProbeTargetKey> Admit(ProbeCycle cycle, int feature, int sample) =>
        Validate(cycle, feature, sample, out ProbeTargetKey key).Admitted(key);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ProbeAddress {
    public ProbeTargetKey Target { get; }
    public int Attempt { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ProbeTargetKey target,
        ref int attempt) {
        if (!Witness.Index(attempt))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:address");
    }

    public static Fin<ProbeAddress> Admit(ProbeTargetKey target, int attempt) =>
        Validate(target, attempt, out ProbeAddress address).Admitted(address);
}

// Identity and tolerance band are inspection demands, not geometry: two plans may probe the same nominal
// circle under different bands, so the key and the band ride the plan and the feature stays pure geometry.
[ComplexValueObject]
[ValidationError<FabricationFault>]
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
        ref FabricationFault? validationError,
        ref int key,
        ref ProbeFeature feature,
        ref ProbeCycle cycle,
        ref double toleranceMm,
        ref int count,
        ref int attempts,
        ref double feedMmPerMinute,
        ref double clearanceMm,
        ref double travelLimitMm,
        ref double approachToleranceMm) {
        if (!(Witness.Index(key) && feature.Valid && feature.Admits(count) && attempts > 0
            && Witness.Positive(toleranceMm) && Witness.Positive(feedMmPerMinute)
            && double.IsFinite(clearanceMm) && clearanceMm >= 0.0
            && double.IsFinite(travelLimitMm) && travelLimitMm > clearanceMm
            && Witness.Positive(approachToleranceMm)))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:plan");
    }

    public static Fin<ProbePlan> Admit(
        int key,
        ProbeFeature feature,
        ProbeCycle cycle,
        double toleranceMm,
        int count,
        int attempts,
        double feedMmPerMinute,
        double clearanceMm,
        double travelLimitMm,
        double approachToleranceMm) =>
        Validate(key, feature, cycle, toleranceMm, count, attempts, feedMmPerMinute, clearanceMm,
            travelLimitMm, approachToleranceMm, out ProbePlan plan).Admitted(plan);
}
```

## [03]-[OBSERVATION_RAIL]

- Owner: `MeasurementSource` is ONE admitted value carrying its `MeasurementKind` row beside the `Interval`, evidence key, and observation sequence every lane shares; a new ingress modality is one row and no consumer changes, because nothing downstream branches on the lane. `StylusCalibration` owns the calibrated stylus behavior and the probe frame its lobing map is measured in.
- Cases: `ProbeCycle` rows retain exact `G31`, `G38.2`, `G38.3`, `G38.4`, and `G38.5` semantics, their posted `GCommand`, and the approach direction they orient; `ProbeOutcome` closes contact, optional miss, and rejection so a hit always carries its observation and compensated point.
- Law: lobing is a function of the direction the stylus DEFLECTS, resolved in the calibrated probe frame. A world-XY azimuth gives every probe on every plane the same phase, which measures nothing the calibration recorded; a deflection along the stylus axis has no azimuth at all, so its lobing term is a measured zero stating that reason rather than an arbitrary phase.
- Auto: `Interval.Contains` gates source and calibration time; `ProbeAddress` retains cycle, feature, sample, and attempt, and correlation runs through one keyed index so contact count never drives quadratic scanning. Observation rows sort by attempt then instant before evaluation, so a repeat fold never reads ingress order.
- Receipt: robust aggregation composes `MathNet.Numerics.Statistics` for the median centre, the median absolute deviation, and the accepted-set RMS repeatability; combined uncertainty then conserves calibration, thermal, lobing, and repeatability contributions.
- Packages: `MathNet.Numerics.Statistics` (`Statistics.Median`, `Statistics.RootMeanSquare`) — every member answers `double.NaN` on an empty population rather than throwing, so an empty accepted set exits on the absence arm BEFORE any statistic is read.
- Boundary: observations carry ball centers; axial travel, lateral approach, and thermal-scale rejection stay on the affected touch, and the aggregate required-hit verdict runs after every target retains its outcomes. Stylus radius and lobing add along the approach while pre-travel subtracts, and inverse thermal scaling restores reference-temperature geometry.

```csharp signature
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ProbeObservation {
    public ProbeAddress Address { get; }
    public Point3d BallCenter { get; }
    public Instant At { get; }
    public double TemperatureC { get; }
    public UInt128 EvidenceKey { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ProbeAddress address,
        ref Point3d ballCenter,
        ref Instant at,
        ref double temperatureC,
        ref UInt128 evidenceKey) {
        if (!(ballCenter.IsValid && double.IsFinite(temperatureC) && evidenceKey != UInt128.Zero))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:observation");
    }

    public static Fin<ProbeObservation> Admit(
        ProbeAddress address, Point3d ballCenter, Instant at, double temperatureC, UInt128 evidenceKey) =>
        Validate(address, ballCenter, at, temperatureC, evidenceKey, out ProbeObservation observation)
            .Admitted(observation);
}

// Ingress modality is a ROW, not a case family: the three lanes carried a byte-identical window/rows/evidence
// triple, every fold arm returned the same value, and no consumer read the discriminant.
[SmartEnum<string>]
public sealed partial class MeasurementKind {
    public static readonly MeasurementKind Telemetry = new("telemetry");
    public static readonly MeasurementKind Manual = new("manual");
    public static readonly MeasurementKind FixtureSynthetic = new("fixture-synthetic");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class MeasurementSource {
    public MeasurementKind Kind { get; }
    public Interval Window { get; }
    public Seq<ProbeObservation> Rows { get; }
    public UInt128 EvidenceKey { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref MeasurementKind kind,
        ref Interval window,
        ref Seq<ProbeObservation> rows,
        ref UInt128 evidenceKey) {
        if (evidenceKey == UInt128.Zero || !rows.ForAll(static row => row.EvidenceKey != UInt128.Zero))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:measurement-source");
    }

    public static Fin<MeasurementSource> Admit(
        MeasurementKind kind, Interval window, Seq<ProbeObservation> rows, UInt128 evidenceKey) =>
        Validate(kind, window, rows, evidenceKey, out MeasurementSource source).Admitted(source);
}

public readonly record struct ProbeLobe(int Harmonic, double AmplitudeMm, double PhaseRadians);

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class StylusCalibration {
    public UInt128 Key { get; }
    public double RadiusMm { get; }
    public double PreTravelMm { get; }

    // The frame the lobing map was MEASURED in: its Z axis is the stylus axis and its X axis the zero-phase
    // reference. Without it a harmonic phase names no direction and the map degenerates to a constant.
    public Rhino.Geometry.Plane ProbeFrame { get; }

    public double ThermalExpansionPerC { get; }
    public double ReferenceTemperatureC { get; }
    public Point3d ThermalReference { get; }
    public double CalibrationUncertaintyMm { get; }
    public Interval Validity { get; }
    public Seq<ProbeLobe> Lobes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref UInt128 key,
        ref double radiusMm,
        ref double preTravelMm,
        ref Rhino.Geometry.Plane probeFrame,
        ref double thermalExpansionPerC,
        ref double referenceTemperatureC,
        ref Point3d thermalReference,
        ref double calibrationUncertaintyMm,
        ref Interval validity,
        ref Seq<ProbeLobe> lobes) {
        if (!(key != UInt128.Zero && Witness.Positive(radiusMm) && preTravelMm >= 0.0 && probeFrame.IsValid
            && Seq(preTravelMm, thermalExpansionPerC, referenceTemperatureC, calibrationUncertaintyMm).ForAll(double.IsFinite)
            && thermalReference.IsValid && calibrationUncertaintyMm >= 0.0
            && validity.HasStart && validity.HasEnd && validity.End > validity.Start
            && lobes.ForAll(static row => row.Harmonic > 0 && double.IsFinite(row.AmplitudeMm) && double.IsFinite(row.PhaseRadians))
            && lobes.Map(static row => row.Harmonic).Distinct().Count == lobes.Count))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:calibration");
    }

    public static Fin<StylusCalibration> Admit(
        UInt128 key,
        double radiusMm,
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

    // The deflection azimuth in the probe's own frame. A deflection parallel to the stylus axis leaves no planar
    // component, so it carries no azimuth and the lobing map contributes a measured zero.
    public double LobeMm(Vector3d approach) {
        Vector3d planar = approach - (ProbeFrame.ZAxis * (approach * ProbeFrame.ZAxis));
        if (planar.Length <= EpsilonPolicy.SqrtEpsilon) return 0.0;
        double azimuth = Math.Atan2(planar * ProbeFrame.YAxis, planar * ProbeFrame.XAxis);
        return Lobes.Sum(row => row.AmplitudeMm * Math.Cos((row.Harmonic * azimuth) + row.PhaseRadians));
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class RepeatPolicy {
    public int MinimumAccepted { get; }
    public double OutlierSigma { get; }
    public double MinimumUncertaintyMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref int minimumAccepted,
        ref double outlierSigma,
        ref double minimumUncertaintyMm) {
        if (!(minimumAccepted > 0 && Witness.Positive(outlierSigma)
            && double.IsFinite(minimumUncertaintyMm) && minimumUncertaintyMm >= 0.0))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:repeat-policy");
    }

    public static Fin<RepeatPolicy> Admit(int minimumAccepted, double outlierSigma, double minimumUncertaintyMm) =>
        Validate(minimumAccepted, outlierSigma, minimumUncertaintyMm, out RepeatPolicy policy).Admitted(policy);
}

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
        static (_, _) => new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:short-of-surface"));
    public static readonly ProbeRejection LateralDrift = new("lateral-drift",
        static (_, _) => new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:lateral-drift"));
    public static readonly ProbeRejection ThermalScale = new("thermal-scale",
        static (_, _) => new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:thermal-scale"));

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
```

## [04]-[DATUM_AND_RESULT]

- Owner: `DatumPolicy` closes assigned transform, best-fit registration, primitive substitution, and memo replay over the current `DatumReceipt` wire; `RegistrationSpread` owns the anisotropic registration budget; `ProbeMemo` mints the registration content identity; `ProbeReport` owns the pre-egress evidence fold.
- Law: NO result, receipt, or content key depends on hash iteration order. Grouping serves lookup alone — the substitute fit is computed once per plan key over a keyed index and then READ BACK onto the features in their own admitted order, so residual ordinals, the census population, and the projected atoms all keep the deterministic order the target fold assigned. Where a fold must emit groups it orders on the declared `ProbePlan.Key` ascending, so even the refusal that reports first is fixed.
- Law: registration propagates ANISOTROPICALLY. A point residual over an inlier cloud of characteristic radius bounds the residual rotation at residual-over-radius, which displaces a feature at its lever arm by that angle times the arm. One uniform term understates every feature outside the cloud and overstates the datum origin itself, so both the cloud radius and the per-feature lever arm enter the budget; an assigned setup transform carries no alignment residual and states its absence rather than a zero.
- Law: the kernel conformance metrics are defined over an UNSIGNED residual — `ConformanceMetric.Maximum` ranks by magnitude and carries no sign — so the census residual carries the absolute deviation while the signed deviation stays a named column on `MeasuredFeature`. Feeding a signed value into that slot makes the worst-sample rank the most positive rather than the worst, which reports a clean surface for a wholly undersize feature set.
- Entry: `Probe.Inspect(InspectPolicy, FabricationInput, FabricationTap? tap = null, SpanBand? band = null)` — the tap and band both default, so a headless run emits and traces nothing with no branch of its own.
- Entry: `ProbeBench.Workload` admits the `icp-probe-fit` measured workload — a best-fit datum lane over the feature-census floor — and `ProbeBench.Run` is the fold the corpus gate times against `FabricationBenchClaims.IcpProbeFit`; measurement and receipt projection stay the bench edge's under the AppHost claim-field map.
- Law: the fit memo lane is one content key and one cache ride on the standing owner pattern — `ProbeMemo.Key` folds every fit-shifting input, the cache key spells the `icp:` prefix the Persistence solver-memo band dispatches on through the branch `HybridCache` L2, a hit re-enters as `DatumPolicy.Replay` with the memoized transform, residual, and radius, and a miss solves `BestFit` then publishes `(Transform, FinalDelta, RadiusMm)`; the lane composes at the cache-owning boundary, so `Probe.Inspect` and the statement kernel stay memo-free and synchronous.
- Exemption: the two-cloud registration region is a statement kernel — resource release is not expressible on the `Fin` rail, and one region releasing both clouds on every exit path replaces a compensating dispose inside a failure lambda, which is a second custody path that leaks the moment a third resource joins.
- Auto: `AlignKind.AlignDetailed` projects a transform only through `AlignmentReceipt.Project<Transform>`; `Fit.Apply` retains per-feature and datum-substitution `FitReceipt` evidence, and a group thinned below its kind's `MinimalSamples` carries no fit rather than a fabricated one; transformed measured points precede every `ResidualSample`.
- Receipt: `ProbeReport` closes the pre-egress evidence fold — cycles, datum, fitted features, the kernel residual spread and its worst sample, and the capability study — while the frozen `InspectionResult` projects only `InspectionFeature` atoms. `Probe.Inspect` mints `FabricationFact.Probe` beside the frozen result — conformance counts and the worst deviation onto `rasm.fabrication.probe.features` and `rasm.fabrication.probe.deviation` through `Process/telemetry#FACT_PROJECTION` as kind `probe` — because `ProbeReport` is file-scoped and the fact is its one telemetry projection. The worst deviation reads the census's own ranked sample, so the instrument, the receipt, and the kernel ranking are ONE quantity and no seeded fold stands beside them. The whole fold runs inside the `FabricationEngine.Probe` bracket the run spine's `SpanBand` opens, with `EnginePhase.DatumRegistered` and `EnginePhase.FeaturesFitted` as its span marks; the settled datum alignment fires the `FabricationFact.Engine.Of` ICP-iteration row through the same tap.
- Packages: `Rasm.Analysis` (`Analyze.Run`, `AnalysisQuery.Conformance`, `ConformanceMetric`, `ResidualSample`, `Distribution`), `Rasm.Solving` (`Fit.Apply`, `FitKind`, `FitOp`, `FitPolicy`, `FitReceipt`), `Rasm.Processing` (`AlignKind.AlignDetailed`, `AlignmentReceipt`), `Rasm.Spatial` (`VectorCloud.Cluster`), `Rasm.Numerics` (`EpsilonPolicy`).
- Boundary: one residual tranche feeds both consumers — `Capability.Assess(new CapabilityStudy.Variables(...), tolerance)` for the SPC study and the kernel `AnalysisQuery.Conformance` measured arity for the run's own statistics, whose `Distribution` row carries the public `Stat` summary beside median and interquartile range. Band conformance derives per sample from the tolerance each `ResidualSample` already carries and lands on `InspectionFeature.Pass`, so no second kernel reach and no package-local mean, RMS, or quantile fold stands beside the rows; a local QIF-shaped record claiming a standard contract the package does not admit is the deleted form.

```csharp signature
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
    // `Replay` carries the memoized fit's MEASURED residual and lever radius, so the anisotropic budget
    // survives replay — `Setup`'s absent spread states an unmeasured registration, never a replayed one.
    public sealed record Replay(DatumReceipt Datum, Transform Registration, double DeltaMm, double RadiusMm) : DatumPolicy;

    public DatumReceipt Receipt => Switch(
        setup: static row => row.Datum,
        bestFit: static row => row.Datum,
        substitute: static row => row.Datum,
        replay: static row => row.Datum);
}

// `ProbeMemo.Key` folds every fit-shifting input through ONE `CanonicalWriter` pass — count-framed measured
// and nominal triples in admitted order, the kind row key, every policy column, the context tolerances —
// hashed by the kernel seed-zero entry, so a byte-identical observation set under one policy resolves ONE
// key across processes and runs. Cache keys spell `icp:{Key:x32}` — the prefix the Persistence solver-memo
// band dispatches on — and the lane composes at the cache-owning boundary: a hit replays as
// `DatumPolicy.Replay` carrying the memoized transform, residual, and radius, a miss runs `BestFit` and
// publishes; the sync statement kernel inside `Align` stays memo-free.
public static class ProbeMemo {
    public static UInt128 Key(Seq<UnregisteredFeature> features, AlignKind kind, AlignmentPolicy policy, Context context) {
        CanonicalWriter w = new(context.Absolute.Value);
        w.Ordinal(features.Count);
        features.Iter(row => {
            w.Double(row.Measured.X).Double(row.Measured.Y).Double(row.Measured.Z);
            w.Double(row.Target.Nominal.X).Double(row.Target.Nominal.Y).Double(row.Target.Nominal.Z);
        });
        w.String(kind.Key)
            .I64(policy.MaxIterations.Value)
            .Double(policy.ConvergenceTolerance.Value).Double(policy.ResidualTolerance.Value).Double(policy.StepTolerance.Value)
            .Double(policy.RobustScale.Value).Double(policy.CovarianceRidge.Value).Double(policy.MadToSigma.Value)
            .I64(policy.OptimizerBudget.Value)
            .Bool(policy.EstimateScale)
            .Optional(policy.TrimFraction.Map(static trim => trim.Value))
            .I64(policy.CoarseLevels.Value)
            .Double(context.Relative.Value).Double(context.Angle.Value).String(context.Unit.Key);
        return ContentHash.Of(w.ToBytes().Span);
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
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
        ref FabricationFault? validationError,
        ref Seq<ProbePlan> plans,
        ref MeasurementSource source,
        ref DatumPolicy datum,
        ref StylusCalibration calibration,
        ref RepeatPolicy repeat,
        ref FitPolicy featureFit,
        ref Option<CapabilityTolerance> capability,
        ref IClock clock) {
        if (plans.IsEmpty || plans.Map(static row => row.Key).Distinct().Count != plans.Count)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:policy");
    }

    public static Fin<InspectPolicy> Admit(
        Seq<ProbePlan> plans,
        MeasurementSource source,
        DatumPolicy datum,
        StylusCalibration calibration,
        RepeatPolicy repeat,
        FitPolicy featureFit,
        Option<CapabilityTolerance> capability,
        IClock clock) =>
        Validate(plans, source, datum, calibration, repeat, featureFit, capability, clock, out InspectPolicy policy)
            .Admitted(policy);
}

// The measured feature keeps BOTH deviations: the signed normal deviation is the metrology fact an operator acts
// on, and the census residual carries its magnitude because the kernel conformance metrics rank unsigned.
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
    Option<FitReceipt> Fit);

// The registration budget as measured, not as assumed: `DeltaMm` is the alignment's own point residual and
// `RadiusMm` the characteristic lever arm of the cloud it was solved over.
file readonly record struct RegistrationSpread(double DeltaMm, double RadiusMm) {
    public double At(double leverArmMm) =>
        Math.Sqrt(Squared(DeltaMm) + Squared(DeltaMm * leverArmMm / RadiusMm));

    private static double Squared(double value) => value * value;
}

file sealed record ProbeDatum(
    DatumReceipt Datum,
    Transform Registration,
    Point3d Origin,
    Option<RegistrationSpread> Spread,
    Option<AlignmentReceipt> Alignment,
    Option<FitReceipt> Fit);

file sealed record ProbeReport(
    UInt128 SourceEvidence,
    Seq<ProbeCycleReceipt> Cycles,
    ProbeDatum Datum,
    Seq<MeasuredFeature> Features,
    Distribution Residuals,
    ResidualSample Worst,
    Option<CapabilityReport> Capability,
    Instant At);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Probe {
    // The normal-consistency scaling that turns a median absolute deviation into a standard-deviation estimate,
    // which is the axis `RepeatPolicy.OutlierSigma` is stated in.
    private const double MadConsistency = 1.4826;

    internal static readonly Op ProbeOp = Op.Of(name: "fabrication:probe");

    public static Fin<FabricationResult> Inspect(
        InspectPolicy policy, FabricationInput input, FabricationTap? tap = null, SpanBand? band = null) =>
        band.Traced(FabricationEngine.Probe, ProbeOp, span =>
            from context in Context.Millimeters().ToFin()
            from _policy in AdmitPolicy(policy)
            from targets in Targets(policy, context)
            from _targets in AdmitTargets(policy, targets)
            let observed = Index(policy.Source.Rows, static row => row.Address.Target)
            let cycles = targets.Bind(target => Evaluate(target, observed, policy))
            let contacted = Index(cycles, static row => row.Target.Key)
            from measured in (
                targets.Traverse(target => Aggregate(target, contacted, policy).ToValidation()),
                RequiredContacts(targets, contacted).ToValidation())
                .Apply(static (rows, _) => rows).As().ToFin()
            let unregistered = measured.Bind(static row => row.ToSeq())
            from datum in unregistered.Head
                .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:no-measurements"))
                .Bind(_ => Reconcile(policy.Datum, unregistered, context))
            let _registered = FabricationTrace.Mark(span, EnginePhase.DatumRegistered)
            let _icp = datum.Alignment.Map(receipt =>
                FabricationFact.Engine.Of(receipt).Map((tap ?? FabricationTap.Silent).Fire).Strict())
            let transformed = TransformFeatures(unregistered, datum)
            from features in Fits(transformed, policy.FeatureFit, context)
            let _fitted = FabricationTrace.Mark(span, EnginePhase.FeaturesFitted)
            from census in Census(features.Map(static row => row.Residual))
            from capability in policy.Capability
                .Traverse(demand => Capability.Assess(
                    new CapabilityStudy.Variables(features.Map(static row => row.Residual)), demand))
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
            from result in ToResult(report, input.Sources + input.ParentRuns, tap ?? FabricationTap.Silent)
            select result);

    private static Fin<Seq<ProbeTarget>> Targets(InspectPolicy policy, Context context) =>
        policy.Plans.TraverseM(plan =>
                plan.Feature.Project(plan.Count, context).Bind(samples =>
                    samples.Map((sample, index) =>
                        ProbeTargetKey.Admit(plan.Cycle, plan.Key, index).Map(key => {
                            Vector3d outward = Unit(sample.SurfaceNormal);
                            Vector3d direction = plan.Cycle.Approach(outward);
                            Point3d start = sample.Nominal - (direction * plan.ClearanceMm);
                            return new ProbeTarget(
                                key, plan, sample.Nominal, outward, direction,
                                start, start + (direction * plan.TravelLimitMm));
                        }))
                    .Traverse(identity).As()))
            .As()
            .Map(static rows => rows.Bind(identity));

    // Correlation is keyed, never scanned: a linear filter per target makes ingress, aggregation, and
    // grouping quadratic in contact count, which a production inspection run reaches immediately. The index
    // serves LOOKUP alone — every fold that must emit rows reads its own declared order, never this one.
    private static HashMap<TKey, Seq<TRow>> Index<TKey, TRow>(Seq<TRow> rows, Func<TRow, TKey> key) =>
        rows.Fold(
            HashMap<TKey, Seq<TRow>>(),
            (map, row) => map.AddOrUpdate(key(row), existing => existing.Add(row), Seq(row)));

    private static Fin<Unit> AdmitPolicy(InspectPolicy policy) =>
        (Gate(policy.Source.Rows.ForAll(row => policy.Source.Window.Contains(row.At)), "probe:source-window"),
         Gate(policy.Source.Rows.ForAll(row => row.EvidenceKey == policy.Source.EvidenceKey), "probe:evidence-identity"),
         Gate(policy.Source.Rows.ForAll(row => policy.Calibration.Validity.Contains(row.At)), "probe:calibration-window"),
         Gate(DatumValid(policy.Datum) && policy.Datum.Receipt.Traceable, "probe:datum-traceability"))
        .Apply(static (_, _, _, _) => unit)
        .As()
        .ToFin();

    private static Fin<Unit> AdmitTargets(InspectPolicy policy, Seq<ProbeTarget> targets) =>
        (Gate(targets.Count == policy.Plans.Sum(static row => row.Count)
             && policy.Plans.ForAll(plan => plan.Attempts >= policy.Repeat.MinimumAccepted), "probe:target-count"),
         Gate(targets.Map(static row => row.Key).Distinct().Count == targets.Count, "probe:target-key"),
         Gate(
             policy.Source.Rows.Map(static row => row.Address).Distinct().Count == policy.Source.Rows.Count
             && policy.Source.Rows.ForAll(row => targets.Exists(target =>
                 target.Key == row.Address.Target && row.Address.Attempt < target.Plan.Attempts)),
             "probe:observation-address"))
        .Apply(static (_, _, _) => unit)
        .As()
        .ToFin();

    private static K<Validation<Error>, Unit> Gate(bool valid, string locus) =>
        AdmissionSlots.Gate(valid, new FabricationFault.PolicyInadmissible(FabConcern.Verify, locus));

    private static bool DatumValid(DatumPolicy policy) => policy.Switch(
        setup: static row => row.Registration.IsValid,
        bestFit: static _ => true,
        substitute: static row => !row.Kinds.IsEmpty && row.Kinds.Distinct().Count == row.Kinds.Count,
        replay: static row => row.Registration.IsValid && row.DeltaMm >= 0.0 && row.RadiusMm > 0.0);

    // One contact leaving its admitted path is per-contact evidence, never a program verdict: aborting here
    // would destroy every other feature's measurement over a single rejected touch, so the reason rides the
    // outcome and the repeat fold decides whether the target still has enough contacts to stand.
    private static Seq<ProbeCycleReceipt> Evaluate(
        ProbeTarget target,
        HashMap<ProbeTargetKey, Seq<ProbeObservation>> observed,
        InspectPolicy policy) {
        Seq<ProbeObservation> rows = observed.Find(target.Key)
            .Map(static found => toSeq(found.OrderBy(static row => row.Address.Attempt).ThenBy(static row => row.At)))
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
        // Pre-travel is lost motion AFTER contact: the reported ball centre sits that far past the true touch
        // along the approach, so it subtracts where the stylus radius and its lobing term add. The lobing term
        // resolves in the calibrated probe frame off the APPROACH direction, which is what the stylus deflects along.
        Point3d surface = observation.BallCenter
            + (target.Direction * (calibration.RadiusMm - calibration.PreTravelMm + calibration.LobeMm(target.Direction)));
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

    // Robust aggregation composes the statistics owner: the median centre, the median absolute deviation, and the
    // accepted-set RMS repeatability are library rows, and the ONE thing this fold owns is the acceptance band.
    // Every statistics member answers NaN on an empty population, so the absence arm exits before any read.
    private static Fin<Option<UnregisteredFeature>> Aggregate(
        ProbeTarget target,
        HashMap<ProbeTargetKey, Seq<ProbeCycleReceipt>> contacted,
        InspectPolicy policy) {
        Seq<CompensatedContact> rows = contacted.Find(target.Key).IfNone(Seq<ProbeCycleReceipt>())
            .Bind(static cycle => cycle.Outcome.Contact.ToSeq());
        if (rows.IsEmpty) return Fin.Succ(Option<UnregisteredFeature>.None);

        Point3d centre = MedianPoint(rows.Map(static row => row.Point));
        Seq<double> distances = rows.Map(row => row.Point.DistanceTo(centre));
        double median = Statistics.Median(distances);
        double deviation = Statistics.Median(distances.Map(value => Math.Abs(value - median)));
        double band = policy.Repeat.OutlierSigma * Math.Max(deviation * MadConsistency, EpsilonPolicy.SqrtEpsilon);
        Seq<CompensatedContact> accepted = rows.Filter(row => row.Point.DistanceTo(centre) <= median + band);
        if (accepted.Count < policy.Repeat.MinimumAccepted)
            return Fin.Fail<Option<UnregisteredFeature>>(
                new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:repeatability"));

        Point3d measured = MeanPoint(accepted.Map(static row => row.Point));
        double repeatability = Statistics.RootMeanSquare(accepted.Map(row => row.Point.DistanceTo(measured)));
        double thermal = accepted.Map(static row => row.ThermalUncertaintyMm)
            .Fold(Option<double>.None, static (held, value) =>
                Some(held.Match(Some: peak => Math.Max(peak, value), None: () => value)))
            .IfNone(0.0);
        double uncertainty = Math.Sqrt(
            Squared(policy.Calibration.CalibrationUncertaintyMm)
            + Squared(policy.Repeat.MinimumUncertaintyMm)
            + Squared(repeatability)
            + Squared(thermal));
        Instant at = accepted.Fold(Option<Instant>.None, static (latest, row) =>
            Some(latest.Match(Some: held => held >= row.At ? held : row.At, None: () => row.At)))
            .IfNone(policy.Clock.GetCurrentInstant());
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
                        .IfNone(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:required-contact")));
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
        // An assigned transform carries no alignment residual, so the registration budget is ABSENT rather than
        // zero: a caller reading a zero would price an unmeasured registration as a perfect one.
        setup: static (state, row) => row.Registration.IsValid
            ? Fin.Succ(new ProbeDatum(
                row.Datum, row.Registration, Centroid(state.Features), None, None, None))
            : Fin.Fail<ProbeDatum>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:setup-transform")),
        bestFit: static (state, row) => Align(state.Features, state.Context, row.Kind, row.Policy)
            .Map(aligned => Seated(row.Datum, aligned, state.Features, None)),
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
            from aligned in Align(inliers, state.Context, row.Registration, row.Alignment)
            select Seated(row.Datum, aligned, inliers, Some(fit)),
        // Replay seats the memoized fit with its measured spread: no solve, no receipt, and the same
        // degenerate-radius gate `Seated` applies, so a replayed budget is never priced from a dead radius.
        replay: static (state, row) => row.Registration.IsValid
            ? Fin.Succ(new ProbeDatum(
                row.Datum, row.Registration, Centroid(state.Features),
                row.RadiusMm > EpsilonPolicy.SqrtEpsilon ? Some(new RegistrationSpread(row.DeltaMm, row.RadiusMm)) : None,
                None, None))
            : Fin.Fail<ProbeDatum>(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:replay-transform")));

    // A rigid registration's residual rotation acts about the CENTROID of the cloud it was solved over, and that
    // cloud's RMS lever arm is what turns a point residual into a bound on the rotation. `DatumReceipt` carries
    // transfer and correction magnitudes rather than a frame, so both the origin and the radius derive from the
    // registered set itself. A degenerate cloud bounds no rotation, so the spread is absent rather than infinite.
    private static ProbeDatum Seated(
        DatumReceipt datum,
        (Transform Transform, AlignmentReceipt Receipt) aligned,
        Seq<UnregisteredFeature> registered,
        Option<FitReceipt> fit) {
        Point3d origin = Centroid(registered);
        double radius = Statistics.RootMeanSquare(registered.Map(row => row.Measured.DistanceTo(origin)));
        return new ProbeDatum(
            datum,
            aligned.Transform,
            origin,
            radius > EpsilonPolicy.SqrtEpsilon ? Some(new RegistrationSpread(aligned.Receipt.FinalDelta, radius)) : None,
            Some(aligned.Receipt),
            fit);
    }

    private static Point3d Centroid(Seq<UnregisteredFeature> features) =>
        MeanPoint(features.Map(static row => row.Measured));

    // Both clouds acquire and release inside ONE region. A compensating dispose inside a failure lambda is a
    // second custody path, and it leaks the moment a third resource joins the fold.
    private static Fin<(Transform Transform, AlignmentReceipt Receipt)> Align(
        Seq<UnregisteredFeature> features,
        Context context,
        AlignKind kind,
        AlignmentPolicy policy) {
        Fin<VectorCloud> source = VectorCloud.Cluster(features.Map(static row => row.Measured), context, key: ProbeOp);
        Fin<VectorCloud> target = VectorCloud.Cluster(features.Map(static row => row.Target.Nominal), context, key: ProbeOp);
        try {
            return from measured in source
                   from nominal in target
                   from receipt in kind.AlignDetailed(measured, nominal, policy, ProbeOp)
                   from transform in receipt.Project<Transform>(ProbeOp)
                   select (transform, receipt);
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
                + datum.Fit.Map(static receipt => Squared(receipt.Residual)).IfNone(0.0));
            return new MeasuredFeature(
                row.Target.Key,
                row.Target.Plan,
                row.Target.Nominal,
                measured,
                row.Target.SurfaceNormal,
                signed,
                // The census residual carries the MAGNITUDE: the kernel conformance metrics rank unsigned, so a
                // signed value in this slot makes the worst sample the most positive one.
                new ResidualSample(index, row.Target.Nominal, Math.Abs(signed), row.Target.Plan.ToleranceMm),
                uncertainty,
                row.RepeatabilityMm,
                row.At,
                None);
        });

    // Residual statistics ride the kernel conformance rows, never a local fold: the measured arity of
    // `AnalysisQuery.Conformance` takes the tranche the probe already holds, so the admission oracle, the band the
    // samples were measured against, and the worst-sample ranking stay the kernel's single owner. Both reaches
    // carry no scope because the band derives from each sample; a plan whose features disagree on tolerance
    // refuses at the kernel rather than summarizing two populations under one verdict.
    private static Fin<(Distribution Spread, ResidualSample Worst)> Census(Seq<ResidualSample> residuals) =>
        from spread in Measured<Distribution>(ConformanceMetric.Distribution, residuals)
        from worst in Measured<ResidualSample>(ConformanceMetric.Maximum, residuals)
        select (spread, worst);

    private static Fin<TOut> Measured<TOut>(ConformanceMetric metric, Seq<ResidualSample> residuals) where TOut : notnull =>
        Analyze.Run<ResidualSample, TOut>(AnalysisQuery.Conformance(metric), residuals.ToArray())
            .ToFin()
            .Bind(values => values.Head.ToFin(
                new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:residual-census")));

    // The fit receipt is computed ONCE per plan key over a keyed index and then read back onto the features in
    // their own admitted order, so nothing downstream inherits index iteration order. The traverse orders on the
    // declared plan key so even the refusal that reports first is fixed. Wall-contact filtering can starve a
    // group below the kind's own minimal set, and the group then carries no fit rather than a fabricated one.
    private static Fin<Seq<MeasuredFeature>> Fits(Seq<MeasuredFeature> features, FitPolicy policy, Context context) =>
        toSeq(Index(features, static feature => feature.Plan.Key))
            .OrderBy(static entry => entry.Key)
            .ToSeq()
            .TraverseM(entry => Fitted(entry.Value, policy, context).Map(receipt => (entry.Key, Receipt: receipt)))
            .As()
            .Map(static rows => toMap(rows))
            .Map(receipts => features.Map(row =>
                row with { Fit = receipts.Find(row.Plan.Key).Bind(identity) }));

    private static Fin<Option<FitReceipt>> Fitted(Seq<MeasuredFeature> group, FitPolicy policy, Context context) =>
        group.Head
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Verify, "probe:fit-group"))
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

    // Probe facts mint beside the frozen result because `ProbeReport` is file-scoped. The worst deviation reads
    // the census's OWN ranked sample, so the instrument, the receipt, and the kernel ranking are one quantity and
    // no seeded fold stands beside them fabricating an extremum for a population the census already refused.
    private static Fin<FabricationResult> ToResult(ProbeReport report, Seq<ContentKey> subjects, FabricationTap tap) =>
        report.Features.TraverseM(ToAtom).As()
            .Map(atoms => {
                _ = tap.Fire(new FabricationFact.Probe(
                    atoms.Count,
                    atoms.Filter(static row => row.Pass.IfNone(false)).Count,
                    report.Worst.Distance));
                return (FabricationResult)new FabricationResult.InspectionResult(atoms, subjects.Distinct());
            });

    private static Fin<InspectionFeature> ToAtom(MeasuredFeature feature) =>
        InspectionFeature.Admit(
            PropertyCategory.Fabrication.Row(feature.Key.Text),
            feature.Nominal,
            feature.Measured,
            Some(feature.Plan.ToleranceMm),
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

// The icp-probe-fit measuring case for the FabricationBenchClaims.IcpProbeFit no-regression claim: workload
// admission proves the datum lane is the ICP best-fit registration over a non-trivial feature census, and
// the measured fold is `Probe.Inspect` — the narrowest public seam reaching the two-cloud alignment, which
// is why the claim's lane columns spell it. Policy and input arrive admitted through their own factories;
// measurement stays the bench edge's under the AppHost claim-field map.
public static class ProbeBench {
    public const int FeatureFloor = 64;

    public static Fin<(InspectPolicy Policy, FabricationInput Input)> Workload(InspectPolicy policy, FabricationInput input) =>
        policy.Datum is DatumPolicy.BestFit
        && policy.Plans.Sum(static row => row.Count) >= FeatureFloor
            ? Fin.Succ((policy, input))
            : Fin.Fail<(InspectPolicy, FabricationInput)>(
                new FabricationFault.PolicyInadmissible(FabConcern.Verify, "bench:icp-probe-fit"));

    public static Fin<FabricationResult> Run((InspectPolicy Policy, FabricationInput Input) workload) =>
        Probe.Inspect(workload.Policy, workload.Input);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
