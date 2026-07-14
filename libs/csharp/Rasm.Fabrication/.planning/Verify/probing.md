# [RASM_FABRICATION_PROBING]

Probe inspection owns in-process metrology: posting emits the `G31`/`G38` probe nodes, this page owns their cycle semantics, decoded telemetry slices or typed manual rows supply measured points (caller-injected data — the AppHost transport never crosses the strata), setup supplies WCS datum assignment, kernel registration/fitting/sampling surfaces solve datum reconciliation, and capability receives the residual tranche. `Probe.Inspect` is the `FabricationPolicy.Inspect` case body and returns the owner-safe `FabricationResult.InspectionResult`; the richer probe report, QIF MeasurementResults projection, datum offsets, and capability report remain terminal inside the Verify plane. The cycle verdict is LOAD-BEARING: measured features derive ONLY from cycle-admitted contacts — a rejected optional-direction touch is receipt evidence and never a measurement — and every contact compensates the stylus ball radius along the approach direction before any feature projects, because a touch probe reports the ball CENTER, not the surface.

## [01]-[INDEX]

- [01]-[PROBE_INSPECTION]: owns `ProbeCycle`, `MeasurementSource`, `DatumPolicy`, `SamplingPolicy`, `InspectPolicy`, stylus compensation, measured-feature receipts, QIF MeasurementResults projection, WCS reconciliation, capability feed, and the one `Probe.Inspect` entry.

## [02]-[PROBE_INSPECTION]

- Owner: `ProbeCycle` owns controller probe-cycle row semantics for posting AST `G31`/`G38` nodes; `InspectPolicy` owns targets, cycles, datum, sampling, source, stylus radius, optional capability tolerance, and the clock seam.
- Ingress: `MeasurementSource` owns honest ingress from decoded telemetry slices, typed manual rows, or declared fixture-synthetic carriage — the frames are caller-injected data records, never a transport reference.
- Kernels: `DatumPolicy` owns setup datum reuse, ICP best-fit over the kernel `AlignKind`/`AlignmentPolicy` six-row dispatcher (`Rasm.Processing` — `AlignDetailed` the entry, `AlignmentReceipt` the gated evidence), and substitute primitive fitting over K26 `FitKind`; `SamplingPolicy` owns CMM grid generation through `SampleKind` and residual projection through `ConformanceMetric` evidence, and its `Kind` row stamps the report so the sampling law is receipt-recoverable.
- Cases: `ProbeCycle` rows `g31` · `g38.2` · `g38.3` · `g38.4` · `g38.5`; `MeasurementSource` cases `Telemetry` · `Manual` · `FixtureSynthetic`; `DatumPolicy` cases `Setup` · `BestFit` · `Substitute`.
- Entry: `public static Fin<FabricationResult> Probe.Inspect(InspectPolicy policy, FabricationInput input)` — the exact `FabricationPolicy.Inspect` arm; `Fin<T>` routes `FabricationFault.ProbeOvertravel(at, limit).ToError()` for cycle travel beyond the limit and kernel faults from registration, primitive fitting, sampling, or capability.
- Auto: `Inspect` samples target features through the `SamplingPolicy.Grid` K37 delegate, admits measurements only from `MeasurementSource`, EXECUTES the cycle law per `ProbeCyclePlan` — signed travel along the target approach from the plan's approach point, toward/away direction admission, the tighter of plan and target travel limits, required-hit exhaustion routing `ProbeOvertravel` and optional-hit misses landing typed `Hit: false` receipts with `Option<ManualMeasurement>.None` (never a sentinel row), uniformly over telemetry, manual, and fixture-synthetic ingress — compensates each ADMITTED contact by `StylusRadiusMm` along the unit approach, and folds nominal/compensated deltas into `ResidualSample` rows through `SamplingPolicy.Residual`. Features derive from the admitted cycle receipts, never the raw measurement sequence.
- Reconcile: `Setup` reads the assigned `WcsDatum`; `BestFit` composes the kernel ICP dispatcher; `Substitute` composes K26 `FitKind`; QIF MeasurementResults rows feed `Spec/capability`; the return stays the owner-safe result case.
- Receipt: `ProbeReport` carries cycle receipts, datum offsets, measured-feature rows, QIF MeasurementResults with the sampling `Kind` stamp, optional `CapabilityReport`, and an `Instant` stamp; `FabricationResult.InspectionResult` carries `Seq<(Point3d Nominal, Point3d Measured)>` plus `MaxDeviation` only.
- Packages: `Process/owner#FABRICATION_OWNER` (`Move`, `FabricationInput`, `FabricationResult`), `Posting/program#CUT_PROGRAM` (`GNode` probe rows), `Fixturing/setups#SETUP_SCHEDULER` (`WcsDatum`), `Spec/capability#CAPABILITY` (`Capability.Assess`, `CapabilityReport`, `CapabilityTolerance`), kernel `Rasm.Processing` (`AlignKind`, `AlignmentPolicy` — the verified six-row ICP dispatcher), K17 (`ResidualSample`, `ConformanceMetric`), K26 (`FitKind`), K37 (`SampleKind`), NodaTime (`IClock`, `Instant`), Rhino.Geometry, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new controller probe cycle is one `ProbeCycle` row and one posting AST row; a new measurement ingress is one `MeasurementSource` case; a new datum reconciliation path is one `DatumPolicy` case carrying its kernel delegate; a new inspection grid is one `SampleKind` policy row; probe pre-travel and lobing calibration land as columns beside `StylusRadiusMm` when a calibration receipt rides the policy; the public surface stays `Probe.Inspect`.
- Boundary: cycle rendering stays posting-owned and cycle semantics stay here; transport stays AppHost-owned and only decoded data rows enter — a `Rasm.AppHost` type in this plane is the strata violation; fixture-synthetic rows require the explicit `FixtureSynthetic` case and never masquerade as measured telemetry; work-offset assignment stays setup-owned while probing reconciles deltas; capability keeps `CapabilityReport` terminal and only owner#atoms-safe `InspectionResult` crosses the fabrication result case; a feature projected from a raw measurement that bypassed its cycle verdict, an uncompensated ball-center contact, or a sentinel no-contact measurement row is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Analysis;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Numerics;
using Rasm.Processing;
using Rasm.Solving;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ProbeCycle {
    public static readonly ProbeCycle G31 = new("g31", "G31", requiresHit: false, towardSurface: true);
    public static readonly ProbeCycle G38TowardRequired = new("g38.2", "G38.2", requiresHit: true, towardSurface: true);
    public static readonly ProbeCycle G38TowardOptional = new("g38.3", "G38.3", requiresHit: false, towardSurface: true);
    public static readonly ProbeCycle G38AwayRequired = new("g38.4", "G38.4", requiresHit: true, towardSurface: false);
    public static readonly ProbeCycle G38AwayOptional = new("g38.5", "G38.5", requiresHit: false, towardSurface: false);

    public string Word { get; }
    public bool RequiresHit { get; }
    public bool TowardSurface { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeasurementSource {
    private MeasurementSource() { }

    public sealed record Telemetry(Seq<ProbeFrame> Frames) : MeasurementSource;
    public sealed record Manual(Seq<ManualMeasurement> Rows) : MeasurementSource;
    public sealed record FixtureSynthetic(Seq<ManualMeasurement> Rows, TestCarriage Carriage) : MeasurementSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DatumPolicy {
    private DatumPolicy() { }

    public sealed record Setup(WcsDatum Datum) : DatumPolicy;
    public sealed record BestFit(AlignKind Kind, AlignmentPolicy Policy, Func<Seq<MeasuredFeature>, Fin<Seq<WorkOffsetDelta>>> Align) : DatumPolicy;
    public sealed record Substitute(FitKind Primitive, AlignmentPolicy Policy, Func<Seq<MeasuredFeature>, Fin<Seq<WorkOffsetDelta>>> Fit) : DatumPolicy;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record ProbeCyclePlan(int Feature, ProbeCycle Cycle, Move Approach, double TravelLimit, GNode Node);

public sealed record FeatureTarget(int Feature, Point3d Nominal, Vector3d Approach, double Clearance, double TravelLimit);

public sealed record SamplingPolicy(
    SampleKind Kind,
    int Count,
    double Clearance,
    Func<FabricationInput, Seq<FeatureTarget>, Fin<Seq<FeatureTarget>>> Grid,
    Func<FeatureTarget, Point3d, Instant, ResidualSample> Residual);

public sealed record InspectPolicy(
    Seq<ProbeCyclePlan> Cycles,
    Seq<FeatureTarget> Targets,
    DatumPolicy Datum,
    SamplingPolicy Sampling,
    MeasurementSource Source,
    double StylusRadiusMm,
    Option<CapabilityTolerance> Capability,
    IClock Clock);

// The decoded telemetry SLICE — caller-injected data; Device/DataItem identify the transport-side origin
// without any transport type crossing the strata.
public sealed record ProbeFrame(int Feature, Point3d Position, double Travel, double Limit, Instant At, string Device, string DataItem);

public sealed record ManualMeasurement(int Feature, Point3d Measured, Instant At, string Source);

public sealed record TestCarriage(string Fixture, Instant At, string EvidenceKey);

public sealed record MeasuredFeature(int Feature, Point3d Nominal, Point3d Measured, double Deviation, ResidualSample Residual, Instant At);

public sealed record WorkOffsetDelta(WcsDatum Datum, Vector3d Translation, Vector3d Rotation);

public sealed record DatumReceipt(DatumPolicy Policy, Seq<WorkOffsetDelta> Offsets, int SnapshotCount);

public sealed record ProbeCycleReceipt(ProbeCyclePlan Plan, Option<ManualMeasurement> Measurement, bool Hit, Instant At);

public sealed record QifMeasurementResults(SampleKind Sampling, Seq<MeasuredFeature> Features, Seq<ResidualSample> Residuals, Option<CapabilityReport> Capability, Instant At);

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
        from targets in policy.Sampling.Grid(input, policy.Targets)
        from measurements in Measurements(policy.Source)
        from cycles in EvaluateCycles(policy.Cycles, targets, measurements)
        from features in Features(cycles, targets, policy)
        from datum in Reconcile(policy.Datum, input, features)
        from capability in AssessCapability(features, policy)
        let at = policy.Clock.GetCurrentInstant()
        let report = new ProbeReport(
            cycles,
            datum,
            features,
            new QifMeasurementResults(policy.Sampling.Kind, features, features.Map(static feature => feature.Residual), capability, at),
            MaxDeviation(features),
            at)
        select ToResult(report);

    static Fin<Seq<ManualMeasurement>> Measurements(MeasurementSource source) =>
        source.Switch(
            telemetry: static telemetry => telemetry.Frames.TraverseM(Frame).As(),
            manual: static manual => Fin.Succ(manual.Rows),
            fixtureSynthetic: static fixture => Fin.Succ(fixture.Rows));

    static Fin<ManualMeasurement> Frame(ProbeFrame frame) =>
        frame.Travel > frame.Limit
            ? Fin.Fail<ManualMeasurement>(new FabricationFault.ProbeOvertravel(frame.Position, frame.Limit).ToError())
            : Fin.Succ(new ManualMeasurement(frame.Feature, frame.Position, frame.At, $"{frame.Device}:{frame.DataItem}"));

    // Features derive ONLY from cycle-admitted contacts (Hit: true, Measurement: Some) — the cycle verdict
    // gates the projection; a raw-measurement bypass is the deleted parallel derivation. The admitted point
    // compensates the stylus ball radius along the unit approach BEFORE the delta folds.
    static Fin<Seq<MeasuredFeature>> Features(Seq<ProbeCycleReceipt> cycles, Seq<FeatureTarget> targets, InspectPolicy policy) =>
        cycles.Filter(static receipt => receipt.Hit)
            .Bind(receipt => receipt.Measurement.Map(row => (receipt, row)).ToSeq())
            .TraverseM(pair => Feature(pair.row, targets, policy))
            .As();

    static Fin<MeasuredFeature> Feature(ManualMeasurement row, Seq<FeatureTarget> targets, InspectPolicy policy) =>
        targets.Find(target => target.Feature == row.Feature).Match(
            Some: target => {
                Point3d compensated = Compensate(row.Measured, target.Approach, policy.StylusRadiusMm);
                return Fin.Succ(new MeasuredFeature(
                    row.Feature,
                    target.Nominal,
                    compensated,
                    target.Nominal.DistanceTo(compensated),
                    policy.Sampling.Residual(target, compensated, row.At),
                    row.At));
            },
            None: () => Fin.Fail<MeasuredFeature>(GeometryFault.DegenerateInput($"probe:target-missing:{row.Feature}").ToError()));

    // A touch probe reports the ball CENTER; the surface point sits one ball radius further along the
    // approach direction.
    static Point3d Compensate(Point3d center, Vector3d approach, double stylusRadiusMm) {
        Vector3d direction = approach;
        direction.Unitize();
        return center + direction * stylusRadiusMm;
    }

    static Fin<DatumReceipt> Reconcile(DatumPolicy datum, FabricationInput input, Seq<MeasuredFeature> features) =>
        datum.Switch(
            state: (input, features),
            setup: static (state, setup) => Fin.Succ(new DatumReceipt(setup, Seq<WorkOffsetDelta>(), state.input.Snapshots.Count())),
            bestFit: static (state, bestFit) => bestFit.Align(state.features).Map(offsets => new DatumReceipt(bestFit, offsets, state.input.Snapshots.Count())),
            substitute: static (state, substitute) => substitute.Fit(state.features).Map(offsets => new DatumReceipt(substitute, offsets, state.input.Snapshots.Count())));

    static Fin<Option<CapabilityReport>> AssessCapability(Seq<MeasuredFeature> features, InspectPolicy policy) =>
        policy.Capability.Match(
            Some: tolerance => Rasm.Fabrication.Spec.Capability.Assess(features.Map(static feature => feature.Residual), tolerance).Map(Optional),
            None: () => Fin.Succ<Option<CapabilityReport>>(None));

    // The controller cycle LAW, executed per plan BEFORE feature projection and uniformly over every ingress
    // source: signed travel along the target approach from the plan's approach point, direction admission
    // (toward/away), the tighter of the plan and target travel limits, and required-hit semantics. A contact
    // past the limit — or a required-hit exhaustion, which is physically the probe running to its limit —
    // routes ProbeOvertravel 2720; an optional-hit miss is a typed Hit-false receipt carrying Measurement:
    // None. Every receipt's Hit is EVALUATED truth, never a stamped constant.
    static Fin<Seq<ProbeCycleReceipt>> EvaluateCycles(Seq<ProbeCyclePlan> plans, Seq<FeatureTarget> targets, Seq<ManualMeasurement> measurements) =>
        plans.TraverseM(plan => targets.Find(t => t.Feature == plan.Feature).Match(
            None: () => Fin.Fail<ProbeCycleReceipt>(GeometryFault.DegenerateInput($"probe:cycle-target-missing:{plan.Feature}").ToError()),
            Some: target => Evaluate(plan, target, measurements.Find(m => m.Feature == plan.Feature)))).As();

    static Fin<ProbeCycleReceipt> Evaluate(ProbeCyclePlan plan, FeatureTarget target, Option<ManualMeasurement> measurement) {
        double limit = Math.Min(plan.TravelLimit, target.TravelLimit);
        return measurement.Match(
            None: () => plan.Cycle.RequiresHit
                ? Fin.Fail<ProbeCycleReceipt>(new FabricationFault.ProbeOvertravel(plan.Approach.To, limit).ToError())
                : Fin.Succ(new ProbeCycleReceipt(plan, None, Hit: false, Instant.MinValue)),
            Some: row => {
                Vector3d direction = target.Approach;
                direction.Unitize();
                double travel = (row.Measured - plan.Approach.To) * direction;
                bool admitted = plan.Cycle.TowardSurface ? travel >= 0.0 : travel <= 0.0;
                return Math.Abs(travel) > limit
                    ? Fin.Fail<ProbeCycleReceipt>(new FabricationFault.ProbeOvertravel(row.Measured, limit).ToError())
                    : !admitted && plan.Cycle.RequiresHit
                        ? Fin.Fail<ProbeCycleReceipt>(new FabricationFault.ProbeOvertravel(row.Measured, limit).ToError())
                        : Fin.Succ(new ProbeCycleReceipt(plan, Some(row), Hit: admitted, row.At));
            });
    }

    static double MaxDeviation(Seq<MeasuredFeature> features) =>
        features.Fold(0.0, static (max, feature) => double.Max(max, feature.Deviation));

    static FabricationResult ToResult(ProbeReport report) =>
        new FabricationResult.InspectionResult(
            report.Features.Map(static feature => (feature.Nominal, feature.Measured)),
            report.MaxDeviation);
}
```
