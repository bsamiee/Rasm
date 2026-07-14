# [RASM_FABRICATION_PROBING]

Probe inspection owns in-process metrology: posting emits the `G31`/`G38` probe nodes, this page owns their cycle semantics, decoded telemetry slices or typed manual rows supply measured points (caller-injected data — the AppHost transport never crosses the strata), setup supplies WCS datum assignment, kernel registration/fitting/sampling surfaces solve datum reconciliation, and capability receives the residual tranche. `Probe.Inspect` is the `FabricationPolicy.Inspect` case body and returns the owner-safe `FabricationResult.InspectionResult`; the richer probe report, QIF MeasurementResults projection, datum offsets, and capability report remain terminal inside the Verify plane. The cycle verdict is LOAD-BEARING: measured features derive ONLY from cycle-admitted contacts — a rejected optional-direction touch is receipt evidence and never a measurement — and every contact compensates the stylus ball radius along the approach direction before any feature projects, because a touch probe reports the ball CENTER, not the surface.

## [01]-[INDEX]

- [01]-[PROBE_INSPECTION]: owns `ProbeCycle`, `MeasurementSource`, `DatumPolicy`, `SamplingPolicy`, `InspectPolicy`, stylus compensation, measured-feature receipts, QIF MeasurementResults projection, WCS reconciliation, capability feed, and the one `Probe.Inspect` entry.

## [02]-[PROBE_INSPECTION]

- Owner: `ProbeCycle` owns controller probe-cycle semantics for `G31` and the four posted `G38` rows; `InspectPolicy` owns sample-addressed targets and cycles, datum, sampling, source, a calibrated stylus row, optional capability tolerance, and the clock seam.
- Ingress: `MeasurementSource` owns honest ingress from decoded telemetry slices, typed manual rows, or declared fixture-synthetic carriage — the frames are caller-injected data records, never a transport reference.
- Kernels: `DatumPolicy.BestFit` invokes `AlignKind.AlignDetailed` over measured/nominal `VectorCloud.Cluster` rows and takes its transform ONLY through the canonical `AlignmentReceipt.Project<Transform>` convergence gate; `Substitute` invokes `Fit.Apply` over its `FitKind` and `FitPolicy` before projection. Delegates consume verified receipts and cannot replace either kernel. `SamplingPolicy` routes point distribution through the kernel `SampleKind.Project` rail over the caller-supplied feature domain, so the stamped kind IS the executed algorithm; `Count` and `Clearance` gate the generated rows, and every generated row re-enters the seed target admission before evaluation.
- Cases: `ProbeCycle` rows `g31` · `g38.2` · `g38.3` · `g38.4` · `g38.5`; `MeasurementSource` cases `Telemetry` · `Manual` · `FixtureSynthetic`; `DatumPolicy` cases `Setup` · `BestFit` · `Substitute`.
- Entry: `public static Fin<FabricationResult> Probe.Inspect(InspectPolicy policy, FabricationInput input)` — the exact `FabricationPolicy.Inspect` arm; `Fin<T>` routes `FabricationFault.ProbeOvertravel(at, limit).ToError()` for cycle travel beyond the limit and kernel faults from registration, primitive fitting, sampling, or capability.
- Auto: admission accumulates stylus, sampling, target-direction, and cycle defects. Observations, plans, and targets all correlate by the stable `(Feature, Sample)` address — the cycle ordinal stays receipt evidence, never the join key, and the address survives every projection through `MeasuredFeature`, QIF characteristic identity, and the inspection result key — so reordered telemetry, repeated touches, and multi-point features never attach a hit to the wrong target or alias a terminal key. Cycle travel measures from the plan's admitted start along the commanded motion direction, so an early toward-surface contact succeeds carrying its true traveled stroke. Optional wrong-direction touches return `Measurement: None`, required direction failures remain distinct from travel overrun, and no-hit receipts use the evaluation clock. Compensation reverses for away cycles and applies calibrated radius, pre-travel, thermal scale about the calibration reference point, and uncertainty before residual projection.
- Reconcile: `Setup` reads the assigned `WcsDatum`; `BestFit` composes the kernel ICP dispatcher; `Substitute` composes K26 `FitKind`; QIF MeasurementResults rows feed `Spec/capability`; the return stays the owner-safe result case.
- Receipt: `ProbeReport` carries cycle receipts, datum offsets, measured features, maximum deviation, and a QIF MeasurementResults projection with document identity, units, characteristic IDs, nominal/actual points, deviation, uncertainty, resource, sampling kind, residuals, capability evidence, and evaluation time. `FabricationResult.InspectionResult` remains the atoms-safe nominal/measured projection.
- Packages: `Process/owner` (`Move`, `FabricationInput`, `FabricationResult`); `Posting/program` (`GNode` probe rows); `Fixturing/setups` (`WcsDatum`); `Spec/capability` (`Capability.Assess`, `CapabilityReport`, `CapabilityTolerance`); kernel `Rasm.Processing` (`AlignKind`, `AlignmentPolicy` — the verified six-row ICP dispatcher); K17 (`ResidualSample`, `ConformanceMetric`); K26 (`FitKind`); K37 (`SampleKind`); NodaTime (`IClock`, `Instant`); Rhino.Geometry; Thinktecture.Runtime.Extensions; LanguageExt.Core; BCL inbox.
- Growth: a new controller probe cycle is one `ProbeCycle` row and one posting AST row; a new measurement ingress is one `MeasurementSource` case; a new datum reconciliation path is one `DatumPolicy` case carrying its kernel delegate; a new inspection grid is one `SampleKind` policy row; probe pre-travel and lobing calibration land as columns beside `StylusRadiusMm` when a calibration receipt rides the policy; the public surface stays `Probe.Inspect`.
- Boundary: cycle rendering stays posting-owned and cycle semantics stay here; transport stays AppHost-owned and only decoded data rows enter — a `Rasm.AppHost` type in this plane is the strata violation; fixture-synthetic rows require the explicit `FixtureSynthetic` case and never masquerade as measured telemetry; work-offset assignment stays setup-owned while probing reconciles deltas; capability keeps `CapabilityReport` terminal and only `InspectionResult` atoms cross the fabrication result case; a feature projected from a raw measurement that bypassed its cycle verdict, an uncompensated ball-center contact, or a sentinel no-contact measurement row is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
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
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeasurementSource {
    private MeasurementSource() { }

    public sealed record Telemetry(Seq<ProbeObservation> Rows) : MeasurementSource;
    public sealed record Manual(Seq<ProbeObservation> Rows) : MeasurementSource;
    public sealed record FixtureSynthetic(Seq<ProbeObservation> Rows, TestCarriage Carriage) : MeasurementSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DatumPolicy {
    private DatumPolicy() { }

    public sealed record Setup(WcsDatum Datum) : DatumPolicy;
    public sealed record BestFit(WcsDatum Datum, AlignKind Kind, AlignmentPolicy Policy, Func<WcsDatum, Transform, Seq<WorkOffsetDelta>> Project) : DatumPolicy;
    public sealed record Substitute(WcsDatum Datum, FitKind Primitive, FitPolicy Policy, Func<WcsDatum, FitReceipt, Seq<WorkOffsetDelta>> Project) : DatumPolicy;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// Start is the ADMITTED approach origin: travel, direction, and limits all measure the displacement from
// this point along the commanded motion, so an early contact carries its true traveled stroke — the
// commanded endpoint is where the move would STOP, never the origin the stroke measures from.
public sealed record ProbeCyclePlan(int Feature, int Sample, ProbeCycle Cycle, Point3d Start, Move.Linear Approach, double TravelLimit) {
    public GNode Node => new GNode.Word(Cycle.Command, Arr(
        new GParam('X', Approach.Target.X), new GParam('Y', Approach.Target.Y), new GParam('Z', Approach.Target.Z),
        new GParam('F', Approach.Feed)), None);
}

public sealed record FeatureTarget(int Feature, int Sample, Point3d Nominal, Vector3d Approach, double Clearance, double TravelLimit);

// The kernel SampleKind OWNS distribution: Domain supplies each seed feature's measured-surface extraction
// domain and Kind.Project draws the points, so declared algorithm and executed distribution cannot diverge.
public sealed record SamplingPolicy(
    SampleKind Kind,
    int Count,
    double Clearance,
    Func<FabricationInput, FeatureTarget, Fin<ExtractionDomain>> Domain,
    Func<FeatureTarget, Point3d, Instant, ResidualSample> Residual);

public sealed record InspectPolicy(
    Seq<ProbeCyclePlan> Cycles,
    Seq<FeatureTarget> Targets,
    DatumPolicy Datum,
    SamplingPolicy Sampling,
    MeasurementSource Source,
    StylusCalibration Stylus,
    Option<CapabilityTolerance> Capability,
    IClock Clock);

public sealed record ProbeObservation(int Cycle, int Feature, int Sample, Point3d Measured, Instant At, string Source);

// ThermalReference anchors the scale: compensation scales the DISPLACEMENT from this calibration point, so
// thermal growth never translates the datum with the measured feature.
public readonly record struct StylusCalibration(double RadiusMm, double PreTravelMm, double ThermalScale, Point3d ThermalReference, double UncertaintyMm);

public sealed record TestCarriage(string Fixture, Instant At, string EvidenceKey);

// Feature/Sample is the stable identity that survives every projection; Cycle rides as receipt evidence.
public sealed record MeasuredFeature(int Cycle, int Feature, int Sample, Point3d Nominal, Point3d Measured, double Deviation, double UncertaintyMm, ResidualSample Residual, Instant At);

public sealed record WorkOffsetDelta(WcsDatum Datum, Vector3d Translation, Vector3d Rotation);

public sealed record DatumReceipt(DatumPolicy Policy, Seq<WorkOffsetDelta> Offsets, int SnapshotCount);

public sealed record ProbeCycleReceipt(ProbeCyclePlan Plan, Option<ProbeObservation> Measurement, bool Hit, Instant At);

public sealed record QifCharacteristic(string Id, Point3d Nominal, Point3d Actual, double DeviationMm, double UncertaintyMm, string Resource, Instant At);

public sealed record QifMeasurementResults(
    string DocumentId,
    string Units,
    SampleKind Sampling,
    Seq<QifCharacteristic> Characteristics,
    Seq<ResidualSample> Residuals,
    Option<CapabilityReport> Capability,
    Instant At);

public sealed record ProbeReport(
    Seq<ProbeCycleReceipt> Cycles,
    DatumReceipt Datum,
    Seq<MeasuredFeature> Features,
    QifMeasurementResults MeasurementResults,
    double MaxDeviation,
    Instant At);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Probe {
    public static Fin<FabricationResult> Inspect(InspectPolicy policy, FabricationInput input) =>
        from admitted in Admit(policy)
        from context in Context.Millimeters().ToFin()
        from targets in GenerateTargets(policy, input, context)
        from measurements in Measurements(policy.Source)
        let at = policy.Clock.GetCurrentInstant()
        from cycles in EvaluateCycles(policy.Cycles, targets, measurements, at)
        from features in Features(cycles, targets, policy)
        from datum in Reconcile(policy.Datum, input, features)
        from capability in AssessCapability(features, policy)
        let report = new ProbeReport(
            cycles,
            datum,
            features,
            new QifMeasurementResults(
                DocumentId: $"qif-measurement-results:{at.ToUnixTimeTicks()}",
                Units: "mm",
                Sampling: policy.Sampling.Kind,
                Characteristics: features.Map(static feature => new QifCharacteristic(
                    $"feature-{feature.Feature}-sample-{feature.Sample}", feature.Nominal, feature.Measured, feature.Deviation,
                    feature.UncertaintyMm, "touch-probe", feature.At)),
                Residuals: features.Map(static feature => feature.Residual),
                Capability: capability,
                At: at),
            MaxDeviation(features),
            at)
        from result in ToResult(report)
        select result;

    private static Fin<Unit> Admit(InspectPolicy policy) {
        Seq<Error> errors = Seq<Error>();
        Seq<double> calibration = Seq(policy.Stylus.RadiusMm, policy.Stylus.PreTravelMm, policy.Stylus.ThermalScale, policy.Stylus.UncertaintyMm);
        if (!calibration.ForAll(double.IsFinite) || policy.Stylus.RadiusMm <= 0.0 || policy.Stylus.PreTravelMm < 0.0
            || policy.Stylus.ThermalScale <= 0.0 || policy.Stylus.UncertaintyMm < 0.0 || !policy.Stylus.ThermalReference.IsValid)
            errors = errors.Add(GeometryFault.DegenerateInput("probe:stylus-calibration").ToError());
        if (policy.Sampling.Count <= 0 || !double.IsFinite(policy.Sampling.Clearance) || policy.Sampling.Clearance < 0.0
            || policy.Sampling.Kind is null || policy.Sampling.Domain is null || policy.Sampling.Residual is null || policy.Clock is null)
            errors = errors.Add(GeometryFault.DegenerateInput("probe:sampling-policy").ToError());
        errors = errors.Concat(TargetErrors(policy.Targets, policy));
        errors = errors.Concat(policy.Cycles
            .Map((cycle, index) => (Cycle: cycle, Index: index))
            .Filter(static row => row.Cycle.Cycle is null || row.Cycle.Feature < 0 || row.Cycle.Sample < 0
                || !row.Cycle.Start.IsValid || !row.Cycle.Approach.Target.IsValid
                || !double.IsFinite(row.Cycle.Approach.Feed) || row.Cycle.Approach.Feed <= 0.0
                || !double.IsFinite(row.Cycle.TravelLimit) || row.Cycle.TravelLimit <= 0.0)
            .Map(row => GeometryFault.DegenerateInput($"probe:cycle:{row.Index}").ToError()));
        if (policy.Targets.Map(static row => (row.Feature, row.Sample)).Distinct().Count != policy.Targets.Count
            || policy.Cycles.Map(static row => (row.Feature, row.Sample)).Distinct().Count != policy.Cycles.Count)
            errors = errors.Add(GeometryFault.DegenerateInput("probe:duplicate-address").ToError());
        if (policy.Source is MeasurementSource.FixtureSynthetic fixture
            && (string.IsNullOrWhiteSpace(fixture.Carriage.Fixture) || string.IsNullOrWhiteSpace(fixture.Carriage.EvidenceKey)))
            errors = errors.Add(GeometryFault.DegenerateInput("probe:fixture-carriage").ToError());
        if ((policy.Datum is DatumPolicy.BestFit bestFit && bestFit.Project is null)
            || (policy.Datum is DatumPolicy.Substitute substitute && substitute.Project is null))
            errors = errors.Add(GeometryFault.DegenerateInput("probe:datum-projector").ToError());
        return errors.Head.Match(
            Some: head => Fin.Fail<Unit>(errors.Tail.Fold(head, static (folded, error) => folded + error)),
            None: () => Fin.Succ(unit));
    }

    private static Seq<Error> TargetErrors(Seq<FeatureTarget> targets, InspectPolicy policy) =>
        targets
            .Filter(target => target.Feature < 0 || target.Sample < 0 || !target.Nominal.IsValid || !target.Approach.IsValid || target.Approach.IsTiny()
                || !double.IsFinite(target.TravelLimit) || target.TravelLimit <= 0.0 || !double.IsFinite(target.Clearance)
                || target.Clearance < policy.Sampling.Clearance)
            .Map(target => GeometryFault.DegenerateInput($"probe:target:{target.Feature}").ToError());

    // SampleKind OWNS distribution: each seed's measured-surface domain projects through the kernel Project
    // rail, generated rows inherit the seed's approach law with kernel-drawn nominals and ordinal Sample
    // addresses, and the WHOLE generated set re-enters the seed target admission — geometry, direction,
    // limits, clearance, address uniqueness, and cardinality are explicit verdicts, never a count check.
    private static Fin<Seq<FeatureTarget>> GenerateTargets(InspectPolicy policy, FabricationInput input, Context context) =>
        policy.Targets
            .TraverseM(seed => policy.Sampling.Domain(input, seed)
                .Bind(domain => policy.Sampling.Kind.Project<Seq<Point3d>>(domain, context))
                .Bind(points => points.Count >= policy.Sampling.Count
                    ? Fin.Succ(toSeq(points.Take(policy.Sampling.Count))
                        .Map((point, ordinal) => seed with { Sample = ordinal, Nominal = point }))
                    : Fin.Fail<Seq<FeatureTarget>>(GeometryFault.DegenerateInput($"probe:sampling-count:{seed.Feature}").ToError())))
            .As()
            .Map(static perSeed => perSeed.Bind(static rows => rows))
            .Bind(generated => AdmitGenerated(generated, policy));

    private static Fin<Seq<FeatureTarget>> AdmitGenerated(Seq<FeatureTarget> rows, InspectPolicy policy) {
        Seq<Error> errors = TargetErrors(rows, policy);
        if (rows.Map(static row => (row.Feature, row.Sample)).Distinct().Count != rows.Count)
            errors = errors.Add(GeometryFault.DegenerateInput("probe:duplicate-address").ToError());
        if (rows.Count < policy.Targets.Count * policy.Sampling.Count)
            errors = errors.Add(GeometryFault.DegenerateInput($"probe:sampling-count:{rows.Count}").ToError());
        return errors.Head.Match(
            Some: head => Fin.Fail<Seq<FeatureTarget>>(errors.Tail.Fold(head, static (folded, error) => folded + error)),
            None: () => Fin.Succ(rows));
    }

    private static Fin<Seq<ProbeObservation>> Measurements(MeasurementSource source) {
        Seq<ProbeObservation> rows = source.Switch(
            telemetry: static telemetry => telemetry.Rows,
            manual: static manual => manual.Rows,
            fixtureSynthetic: static fixture => fixture.Rows.Map(row => row with {
                Source = $"{row.Source}:{fixture.Carriage.Fixture}:{fixture.Carriage.EvidenceKey}:{fixture.Carriage.At.ToUnixTimeTicks()}",
            }));
        return rows.Exists(static row => row.Cycle < 0 || row.Feature < 0 || row.Sample < 0 || !row.Measured.IsValid || string.IsNullOrWhiteSpace(row.Source))
            || rows.Map(static row => (row.Feature, row.Sample)).Distinct().Count != rows.Count
            ? Fin.Fail<Seq<ProbeObservation>>(GeometryFault.DegenerateInput("probe:observation").ToError())
            : Fin.Succ(rows);
    }

    // Features derive ONLY from cycle-admitted contacts (Hit: true, Measurement: Some) — the cycle verdict
    // gates the projection; a raw-measurement bypass is the deleted parallel derivation. The admitted point
    // compensates the stylus ball radius along the unit approach BEFORE the delta folds.
    private static Fin<Seq<MeasuredFeature>> Features(Seq<ProbeCycleReceipt> cycles, Seq<FeatureTarget> targets, InspectPolicy policy) =>
        cycles.Filter(static receipt => receipt.Hit)
            .Bind(receipt => receipt.Measurement.Map(row => (receipt, row)).ToSeq())
            .TraverseM(pair => Feature(pair.receipt, pair.row, targets, policy))
            .As();

    private static Fin<MeasuredFeature> Feature(ProbeCycleReceipt receipt, ProbeObservation row, Seq<FeatureTarget> targets, InspectPolicy policy) =>
        targets.Find(target => target.Feature == row.Feature && target.Sample == row.Sample).Match(
            Some: target => {
                Point3d compensated = Compensate(row.Measured, target.Approach, receipt.Plan.Cycle, policy.Stylus);
                return Fin.Succ(new MeasuredFeature(
                    row.Cycle,
                    row.Feature,
                    row.Sample,
                    target.Nominal,
                    compensated,
                    target.Nominal.DistanceTo(compensated),
                    policy.Stylus.UncertaintyMm,
                    policy.Sampling.Residual(target, compensated, row.At),
                    row.At));
            },
            None: () => Fin.Fail<MeasuredFeature>(GeometryFault.DegenerateInput($"probe:target-missing:{row.Feature}").ToError()));

    // A touch probe reports the ball CENTER; the surface point sits one ball radius further along the
    // approach direction.
    private static Point3d Compensate(Point3d center, Vector3d approach, ProbeCycle cycle, StylusCalibration stylus) {
        Vector3d direction = approach;
        direction.Unitize();
        double signed = cycle.TowardSurface ? 1.0 : -1.0;
        return stylus.ThermalReference + (center - stylus.ThermalReference) * stylus.ThermalScale
            + direction * (signed * (stylus.RadiusMm + stylus.PreTravelMm));
    }

    private static Fin<DatumReceipt> Reconcile(DatumPolicy datum, FabricationInput input, Seq<MeasuredFeature> features) =>
        datum.Switch(
            state: (input, features),
            setup: static (state, setup) => Fin.Succ(new DatumReceipt(setup, Seq<WorkOffsetDelta>(), state.input.Snapshots.Count())),
            // The registration owner gates its own transform: Project<Transform> is the canonical
            // convergence projection — a local Stop switch beside it is the second-gate defect.
            bestFit: static (state, bestFit) =>
                from context in Context.Millimeters().ToFin()
                from source in VectorCloud.Cluster(state.features.Map(static feature => feature.Measured), context)
                from target in VectorCloud.Cluster(state.features.Map(static feature => feature.Nominal), context)
                let key = Op.Of()
                from receipt in bestFit.Kind.AlignDetailed(source, target, bestFit.Policy, key)
                from transform in receipt.Project<Transform>(key)
                select new DatumReceipt(bestFit, bestFit.Project(bestFit.Datum, transform), state.input.Snapshots.Count()),
            substitute: static (state, substitute) =>
                from context in Context.Millimeters().ToFin()
                from receipt in Fit.Apply(
                    new FitOp(Seq(substitute.Primitive), state.features.Map(static feature => feature.Measured).ToArray(), None, substitute.Policy),
                    context,
                    Op.Of())
                select new DatumReceipt(substitute, substitute.Project(substitute.Datum, receipt), state.input.Snapshots.Count()));

    private static Fin<Option<CapabilityReport>> AssessCapability(Seq<MeasuredFeature> features, InspectPolicy policy) =>
        policy.Capability.Match(
            Some: tolerance => Rasm.Fabrication.Spec.Capability.Assess(features.Map(static feature => feature.Residual), tolerance).Map(Optional),
            None: () => Fin.Succ<Option<CapabilityReport>>(None));

    // The controller cycle LAW, executed per plan BEFORE feature projection and uniformly over every ingress
    // source: signed travel along the commanded motion direction from the plan's ADMITTED START, direction
    // admission (toward/away), the tighter of the plan and target travel limits, and required-hit semantics.
    // A contact past the limit — or a required-hit exhaustion, which is physically the probe running to its
    // limit — routes ProbeOvertravel 2720; an optional-hit miss is a typed Hit-false receipt carrying
    // Measurement: None. Every receipt's Hit is EVALUATED truth, never a stamped constant.
    private static Fin<Seq<ProbeCycleReceipt>> EvaluateCycles(
        Seq<ProbeCyclePlan> plans,
        Seq<FeatureTarget> targets,
        Seq<ProbeObservation> measurements,
        Instant evaluatedAt) {
        Seq<Fin<ProbeCycleReceipt>> evaluated = plans.Map(plan => targets.Find(t => t.Feature == plan.Feature && t.Sample == plan.Sample).Match(
            None: () => Fin.Fail<ProbeCycleReceipt>(GeometryFault.DegenerateInput($"probe:cycle-target-missing:{plan.Feature}").ToError()),
            Some: target => Evaluate(plan, target, measurements.Find(m => m.Feature == plan.Feature && m.Sample == plan.Sample), evaluatedAt)));
        Seq<Error> errors = evaluated.Bind(static row => row.Match(
            Succ: static _ => Seq<Error>(),
            Fail: static error => Seq(error)));
        return errors.Head.Match(
            Some: head => Fin.Fail<Seq<ProbeCycleReceipt>>(errors.Tail.Fold(head, static (folded, error) => folded + error)),
            None: () => Fin.Succ(evaluated.Bind(static row => row.Match(Succ: static receipt => Seq(receipt), Fail: static _ => Seq<ProbeCycleReceipt>()))));
    }

    private static Fin<ProbeCycleReceipt> Evaluate(ProbeCyclePlan plan, FeatureTarget target, Option<ProbeObservation> measurement, Instant evaluatedAt) {
        double limit = Math.Min(plan.TravelLimit, target.TravelLimit);
        Vector3d approach = target.Approach;
        approach.Unitize();
        // The commanded motion runs +approach toward the surface and -approach away; the plan's own geometry
        // must agree with that direction before any contact evaluates.
        Vector3d motion = plan.Cycle.TowardSurface ? approach : -approach;
        return (plan.Approach.Target - plan.Start) * motion <= 0.0
            ? Fin.Fail<ProbeCycleReceipt>(GeometryFault.DegenerateInput($"probe:plan-direction:{plan.Cycle.Key}:{plan.Feature}").ToError())
            : measurement.Match(
                None: () => plan.Cycle.RequiresHit
                    ? Fin.Fail<ProbeCycleReceipt>(new FabricationFault.ProbeOvertravel(plan.Approach.Target, limit).ToError())
                    : Fin.Succ(new ProbeCycleReceipt(plan, None, Hit: false, evaluatedAt)),
                Some: row => {
                    double travel = (row.Measured - plan.Start) * motion;
                    bool admitted = travel > 0.0;
                    return travel > limit
                        ? Fin.Fail<ProbeCycleReceipt>(new FabricationFault.ProbeOvertravel(row.Measured, limit).ToError())
                        : !admitted && plan.Cycle.RequiresHit
                            ? Fin.Fail<ProbeCycleReceipt>(GeometryFault.DegenerateInput($"probe:direction:{plan.Cycle.Key}:{plan.Feature}").ToError())
                            : Fin.Succ(new ProbeCycleReceipt(plan, admitted ? Some(row) : None, Hit: admitted, row.At));
                });
    }

    private static double MaxDeviation(Seq<MeasuredFeature> features) =>
        features.Fold(0.0, static (max, feature) => double.Max(max, feature.Deviation));

    // The owner atom admits — InspectionFeature's constructor is private, so the projection rides Admit on
    // the rail with the probe method and calibrated uncertainty stamped.
    private static Fin<FabricationResult> ToResult(ProbeReport report) =>
        report.Features
            .TraverseM(static feature => InspectionFeature.Admit(
                $"feature-{feature.Feature}-sample-{feature.Sample}", feature.Nominal, feature.Measured,
                None, feature.UncertaintyMm, InspectionMethod.Probe))
            .As()
            .Map(static features => (FabricationResult)new FabricationResult.InspectionResult(features));
}
```
