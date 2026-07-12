# [RASM_TRANSPORT]

The optimal-transport owner: ONE log-domain stabilized Sinkhorn kernel over weighted cluster pairs — balanced marginals, unbalanced KL-relaxed marginals, and Sinkhorn-divergence debiasing are POLICY rows of one iteration, never three solver bodies — projecting through ONE `SinkhornPlan.Project<TOut>` into every transport answer: the regularized distance, the full evidence receipt, the thresholded correspondence set, the coupling as a `matrix.md` `Matrix`, and the barycentric-transport image cloud. Transport mass is the cluster's own admitted normalized mass (`cloud.md` `MassOf`), so a weighted cluster IS a discrete measure and no second measure type exists.

The kernel is numerically owned end to end: every scaling update runs in log space through one `LogSumExp` fold, and the exponential-underflow policy is a NAMED constant — `LogUnderflowFloor = -708.396…`, the natural log of the smallest positive NORMAL double, below which `Math.Exp` degrades subnormal-then-zero, so the coupling exponent floors to exactly zero with the flooring recorded as receipt evidence, never a silent denormal (a floor at ln `double.Epsilon` ≈ −745 is the rejected constant — it passes the whole subnormal band (−745, −708.4] through silently). The iteration is a flat row-major `double[]` statement kernel — the retired raw-MathNet dense-matrix reach is deleted; linear algebra enters only at the egress where the coupling projects into the `matrix.md` owner.

## [01]-[INDEX]

- [02]-[TRANSPORT_POLICY]: the one policy record; stop/residual/numeric vocabularies.
- [03]-[SINKHORN]: the log-domain kernel; the plan; the five typed projections; the receipt.
- [04]-[CORRESPONDENCES]: the coupling-thresholded correspondence set and its coverage statistics.

## [02]-[TRANSPORT_POLICY]

- Owner: `CloudTransportPolicy` — `Regularization: PositiveMagnitude` (the entropic ε), `MaxIterations: Dimension`, `Debiased: bool` (Sinkhorn divergence: subtract half of each self-transport), `MassRelaxation: Option<PositiveMagnitude>` (`None` = balanced marginals; `Some(λ)` = unbalanced KL relaxation with scaling exponent `λ/(λ+ε)`), `ConvergenceTolerance: PositiveMagnitude`, `CouplingCutoff: PositiveMagnitude` (the sparsification floor below which a coupling entry carries no correspondence). One record; balanced-versus-unbalanced and plain-versus-debiased are column values, never sibling policies.
- Cases: `SinkhornStopKind` `[SmartEnum<int>]` — `BalancedMarginalsConverged` / `RelaxedScalingConverged` / `BalancedMarginalsStoppedWithoutConvergence` / `RelaxedScalingStoppedWithoutConvergence`, each carrying a `Converged` column so budget exhaustion stays a readable outcome, never a failure (the caller retries with a wider budget or accepts the partial plan). `SinkhornResidualKind` — `MarginalMass` (balanced: worst row/column marginal error against the target measures) / `ScalingChange` (unbalanced: worst log-scaling delta between sweeps — the marginal test is meaningless under relaxation). `SinkhornNumericStatus` — `FiniteAccepted` / `UnderflowFloored`.
- Entry: `public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, bool debiased = false, Option<double> massRelaxation = default, Option<double> convergenceTolerance = default, Option<double> couplingCutoff = default, Op? key = null)` — admits every column once through the value objects; tolerance and cutoff default to `1e-8`.
- Boundary: the residual KIND derives from the policy (`MassRelaxation.IsSome` selects `ScalingChange`), never a caller flag; a `BalancedPolicy`/`UnbalancedPolicy`/`DebiasedPolicy` sibling family is the rejected form — the record's columns span the product.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SinkhornStopKind {
    public static readonly SinkhornStopKind BalancedMarginalsConverged = new(key: 0, converged: true);
    public static readonly SinkhornStopKind RelaxedScalingConverged = new(key: 1, converged: true);
    public static readonly SinkhornStopKind BalancedMarginalsStoppedWithoutConvergence = new(key: 2, converged: false);
    public static readonly SinkhornStopKind RelaxedScalingStoppedWithoutConvergence = new(key: 3, converged: false);
    public bool Converged { get; }
}

[SmartEnum<int>]
public sealed partial class SinkhornResidualKind {
    public static readonly SinkhornResidualKind MarginalMass = new(key: 0);
    public static readonly SinkhornResidualKind ScalingChange = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SinkhornNumericStatus {
    public static readonly SinkhornNumericStatus FiniteAccepted = new(key: 0);
    public static readonly SinkhornNumericStatus UnderflowFloored = new(key: 1);
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudTransportPolicy(
    PositiveMagnitude Regularization, Dimension MaxIterations, bool Debiased,
    Option<PositiveMagnitude> MassRelaxation, PositiveMagnitude ConvergenceTolerance, PositiveMagnitude CouplingCutoff) {
    public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, bool debiased = false,
        Option<double> massRelaxation = default, Option<double> convergenceTolerance = default, Option<double> couplingCutoff = default, Op? key = null) {
        Op op = key.OrDefault();
        return from reg in op.AcceptValidated<PositiveMagnitude>(candidate: regularization)
               from cap in op.AcceptValidated<Dimension>(candidate: maxIterations)
               from relax in massRelaxation.Match(
                   Some: lambda => op.AcceptValidated<PositiveMagnitude>(candidate: lambda).Map(Some),
                   None: static () => Fin.Succ(Option<PositiveMagnitude>.None))
               from tolerance in op.AcceptValidated<PositiveMagnitude>(candidate: convergenceTolerance.IfNone(1.0e-8))
               from cutoff in op.AcceptValidated<PositiveMagnitude>(candidate: couplingCutoff.IfNone(1.0e-8))
               select new CloudTransportPolicy(Regularization: reg, MaxIterations: cap, Debiased: debiased,
                   MassRelaxation: relax, ConvergenceTolerance: tolerance, CouplingCutoff: cutoff);
    }
    internal SinkhornResidualKind ResidualKind => MassRelaxation.IsSome ? SinkhornResidualKind.ScalingChange : SinkhornResidualKind.MarginalMass;
}
```

## [03]-[SINKHORN]

- Owner: `CloudTransport` — the operation surface; `SinkhornPlan` — the internal solved-plan carrier (distance, row-major coupling buffer, convergence residual pair, iteration count, stop row, cutoff, underflow evidence) with the ONE `Project<TOut>` egress.
- Entry: `public static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op? key = null)` — cluster×cluster only (any other case pairing is `Unsupported`); the requested `TOut` selects the projection row, so distance-only callers, receipt callers, correspondence callers, coupling-matrix callers, and transported-cloud callers share one entry and one solve.
- Auto: the kernel iterates the log-domain scalings — `logU[i] = exp·(logA[i] − LSE_j(logK[i,j] + logV[j]))` then the column sweep, where `logK[i,j] = −‖sᵢ−tⱼ‖²/ε` and `exp = λ/(λ+ε)` (balanced: `exp = 1`) — until the policy residual falls under tolerance or the budget exhausts; `LogSumExp` is max-shifted so a fully-improbable row degrades to `−∞` gracefully. Debiasing runs the SAME kernel twice more on the self-pairs (`source×source`, `target×target`) and reports `W = plan − ½·self_s − ½·self_t` with both bias distances carried as receipt evidence. The coupling materializes once at the end — `exp(logU[i] + logK[i,j] + logV[j])` with the `LogUnderflowFloor` policy applied and recorded — and the distance is the entropic transport cost `−ε·Σ π·logK`. Non-finite anywhere (distance, residuals, coupling) faults the solve; budget exhaustion does NOT — it returns the plan under its `StoppedWithoutConvergence` stop row.
- Receipt: `SinkhornReceipt` — distance + raw/bias distances, the full policy echo (regularization, relaxation, tolerance, cutoff, debiased), `ResidualKind`, `NumericStatus`, both convergence residuals, iterations, `Stop`, coupling mass, nonzero-coupling count, min-positive/max coupling extents, and the nested `CloudCorrespondenceSet` — `IValidityEvidence`, `IsValid` one `ValidityClaim.All` fold. `Distance` claims finiteness, NOT sign: a debiased Sinkhorn divergence sits epsilon-negative numerically, so only the raw entropic cost carries the nonnegativity claim.
- Packages: RhinoCommon (`Point3d.DistanceToSquared` — the cost kernel), System.Numerics.Tensors (`TensorPrimitives.Max`/`Sum` inside the LSE row folds), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: DECLARED growth cases on this owner — `Barycenter(Seq<(VectorCloud, double)> weighted, policy)` (fixed-support Wasserstein barycenter: alternate the same scaling sweeps against N cost kernels sharing one support) and `GromovWasserstein(source, target, policy)` (entropic GW: the same Sinkhorn kernel inside an outer cost-tensor fixed-point over intra-cloud distance matrices — the non-rigid matcher `register.md` cannot express); both land as new entry arms over the SAME log-domain kernel and receipt vocabulary, zero new solver bodies.
- Boundary: the scaling sweeps are the named statement kernel — flat `double[]` row-major buffers, index loops, in-place log-scaling updates — confined to the solve body with a `Fin` rail at both edges; MathNet does NOT appear on this page (the retired dense-matrix coupling is deleted) — the coupling exits as a `matrix.md` `Matrix` through its projection row and every downstream consumer reads the owner type; `LogUnderflowFloor` is THE underflow policy and an ad-hoc `Math.Exp` on an unfloored exponent inside any projection is the re-introduced silent-zero defect; the plan's `typeof(TOut)` resolution routes typed `ProjectionRow` entries through `atoms.md` `AtomProjection.Rows` — never a reflection ladder.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CloudTransport {
    // ln(smallest positive NORMAL double): below this exponent Math.Exp is subnormal or zero;
    // the coupling floors to exactly 0 and the flooring is recorded — no denormal survives silently.
    internal const double LogUnderflowFloor = -708.3964185322641;

    public static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) =>
                from srcMass in CloudKernel.MassOf(cluster: src, key: op)
                from tgtMass in CloudKernel.MassOf(cluster: tgt, key: op)
                from plan in Solve(source: src.Vertices, target: tgt.Vertices, sourceMass: srcMass, targetMass: tgtMass, policy: policy, key: op)
                from bias in policy.Debiased
                    ? from selfS in Solve(source: src.Vertices, target: src.Vertices, sourceMass: srcMass, targetMass: srcMass, policy: policy, key: op)
                      from selfT in Solve(source: tgt.Vertices, target: tgt.Vertices, sourceMass: tgtMass, targetMass: tgtMass, policy: policy, key: op)
                      select (Source: Some(selfS.Distance), Target: Some(selfT.Distance),
                              Distance: plan.Distance - (0.5 * selfS.Distance) - (0.5 * selfT.Distance))
                    : Fin.Succ((Source: Option<double>.None, Target: Option<double>.None, Distance: plan.Distance))
                from output in plan.Project<TOut>(source: src, target: tgt, distance: bias.Distance,
                    sourceBias: bias.Source, targetBias: bias.Target, policy: policy, key: op)
                select output,
            _ => Fin.Fail<TOut>(op.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))),
        };
    }

    // Log-domain scaling kernel: named statement exemption — flat row-major buffers, in-place sweeps, Fin at both edges.
    private static Fin<SinkhornPlan> Solve(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, CloudTransportPolicy policy, Op key) {
        (int m, int n, double eps) = (source.Count, target.Count, policy.Regularization.Value);
        double[] logK = new double[m * n];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                logK[(i * n) + j] = -source[i].DistanceToSquared(other: target[j]) / eps;
        double[] logU = new double[m]; double[] logV = new double[n];
        double[] logA = [.. sourceMass.AsIterable().Select(Math.Log)]; double[] logB = [.. targetMass.AsIterable().Select(Math.Log)];
        double exponent = policy.MassRelaxation.Match(Some: l => l.Value / (l.Value + eps), None: () => 1.0);
        bool balanced = policy.MassRelaxation.IsNone;
        (double resS, double resT, int iterations) = (double.PositiveInfinity, double.PositiveInfinity, 0);
        for (int iter = 0; iter < policy.MaxIterations.Value && Math.Max(resS, resT) > policy.ConvergenceTolerance.Value; iter++) {
            iterations = iter + 1;
            (double[] prevU, double[] prevV) = ([.. logU], [.. logV]);
            for (int i = 0; i < m; i++) logU[i] = exponent * (logA[i] - LogSumExp(row: logK.AsSpan(i * n, n), shift: logV));
            for (int j = 0; j < n; j++) logV[j] = exponent * (logB[j] - LogSumExpColumn(logK: logK, column: j, rows: m, stride: n, shift: logU));
            (resS, resT) = balanced
                ? MarginalResiduals(logK: logK, logU: logU, logV: logV, a: sourceMass, b: targetMass, m: m, n: n)
                : (MaxDelta(prevU, logU), MaxDelta(prevV, logV));
        }
        bool floored = false;
        double[] coupling = new double[m * n];
        for (int i = 0; i < m; i++) {
            for (int j = 0; j < n; j++) {
                double log = logU[i] + logK[(i * n) + j] + logV[j];
                floored |= log < LogUnderflowFloor;
                coupling[(i * n) + j] = log < LogUnderflowFloor ? 0.0 : Math.Exp(log);
            }
        }
        double distance = -eps * Enumerable.Range(0, m * n).Sum(k => coupling[k] * logK[k]);
        bool converged = Math.Max(resS, resT) <= policy.ConvergenceTolerance.Value;
        return double.IsFinite(distance) && double.IsFinite(resS) && double.IsFinite(resT)
            ? Fin.Succ(new SinkhornPlan(Distance: distance, Coupling: coupling, Rows: m, Columns: n,
                SourceConvergenceResidual: resS, TargetConvergenceResidual: resT, Iterations: iterations,
                Stop: (balanced, converged) switch {
                    (true, true) => SinkhornStopKind.BalancedMarginalsConverged,
                    (true, false) => SinkhornStopKind.BalancedMarginalsStoppedWithoutConvergence,
                    (false, true) => SinkhornStopKind.RelaxedScalingConverged,
                    (false, false) => SinkhornStopKind.RelaxedScalingStoppedWithoutConvergence,
                },
                ConvergenceTolerance: policy.ConvergenceTolerance.Value, CouplingCutoff: policy.CouplingCutoff.Value, UnderflowFloored: floored))
            : Fin.Fail<SinkhornPlan>(key.InvalidResult());
    }

    private static double LogSumExp(ReadOnlySpan<double> row, double[] shift) { /* max-shifted LSE over row[j]+shift[j] */ return default; }
    // LogSumExpColumn (strided column LSE) / MaxDelta (worst |prev-next| log-scaling delta) /
    // MarginalResiduals (worst row/column marginal error vs a and b): one-body private statics of the kernel.
}

// --- [MODELS] -----------------------------------------------------------------------------
internal sealed record SinkhornPlan(
    double Distance, double[] Coupling, int Rows, int Columns,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop,
    double ConvergenceTolerance, double CouplingCutoff, bool UnderflowFloored) {
    internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance,
        Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
        SinkhornPlan self = this;
        return AtomProjection.Rows<SinkhornPlan, TOut>(self: self, key: key, owner: typeof(VectorCloud),
            ProjectionRow.Of<double>(() => key.AcceptValue(value: distance)),
            ProjectionRow.Of<SinkhornReceipt>(() => self.ReceiptOf(source: source, target: target, distance: distance,
                sourceBias: sourceBias, targetBias: targetBias, policy: policy, key: key)),
            ProjectionRow.Of<CloudCorrespondenceSet>(() => Correspondences.OfCoupling(source: source, target: target,
                coupling: self.Coupling, rows: self.Rows, columns: self.Columns, cutoff: self.CouplingCutoff, key: key)),
            ProjectionRow.Of<Matrix>(() => Matrix.Of(rows: Dimension.Create(value: self.Rows), cols: Dimension.Create(value: self.Columns),
                entries: new Arr<double>([.. self.Coupling]), key: key)),
            ProjectionRow.Of<VectorCloud>(() => self.BarycentricImage(source: source, target: target, key: key)));
    }
    // BarycentricImage: row i maps to (Σ_j π[i,j]·t_j)/rowMass with rowMass as the transported weight — a
    // positive-row-mass gate; re-admitted through VectorCloud.Cluster with mass (context: target.Tolerance —
    // the image lives in the target's tolerance regime) so the image IS a measure.
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornReceipt(
    double Distance, Option<double> RawDistance, Option<double> SourceBiasDistance, Option<double> TargetBiasDistance,
    double Regularization, Option<double> MassRelaxation, double ConvergenceTolerance, double CouplingCutoff, bool Debiased,
    SinkhornResidualKind ResidualKind, SinkhornNumericStatus NumericStatus,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop,
    double CouplingMass, int NonZeroCouplings, Option<double> MinPositiveCoupling, Option<double> MaxCoupling,
    CloudCorrespondenceSet Correspondences) : IValidityEvidence {
    // Distance claims finiteness only: a debiased divergence may sit epsilon-negative; the raw cost is nonnegative.
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Distance),
        ValidityClaim.Of(RawDistance.Map(static d => ValidityClaim.Nonnegative(d).Holds).IfNone(!Debiased)),
        ValidityClaim.Of(Debiased == (SourceBiasDistance.IsSome && TargetBiasDistance.IsSome)),
        ValidityClaim.Positive(Regularization),
        ValidityClaim.Of(MassRelaxation.Map(static l => ValidityClaim.Positive(l).Holds).IfNone(true)),
        ValidityClaim.Positive(ConvergenceTolerance),
        ValidityClaim.Positive(CouplingCutoff),
        ValidityClaim.Of(ResidualKind.Equals(MassRelaxation.IsSome ? SinkhornResidualKind.ScalingChange : SinkhornResidualKind.MarginalMass)),
        ValidityClaim.Nonnegative(SourceConvergenceResidual),
        ValidityClaim.Nonnegative(TargetConvergenceResidual),
        ValidityClaim.Of(Stop.Converged == (Math.Max(SourceConvergenceResidual, TargetConvergenceResidual) <= ConvergenceTolerance)),
        ValidityClaim.CountAtLeast(count: Iterations, floor: 1),
        ValidityClaim.Nonnegative(CouplingMass),
        ValidityClaim.Of(NonZeroCouplings >= 0),
        ValidityClaim.Of(MinPositiveCoupling.Map(static v => ValidityClaim.Positive(v).Holds).IfNone(true)),
        ValidityClaim.Of(MaxCoupling.Map(static v => ValidityClaim.Nonnegative(v).Holds).IfNone(true)),
        ValidityClaim.Evidence(Correspondences));
}
```

## [04]-[CORRESPONDENCES]

- Owner: `CloudCorrespondence` — one thresholded coupling entry: source/target indices and points, residual vector, distance and squared distance, source/target/coupling masses, and a `Confidence` column (`min(1, π[i,j]/max(aᵢ, bⱼ))` — how decisively this pairing claims its endpoints); `CloudCorrespondenceSet` — the ordered collection with its coverage statistics.
- Entry: `Correspondences.OfCoupling(source, target, coupling, rows, columns, cutoff, key)` — walks the coupling above the cutoff, emits the entries, and folds the statistics in one pass.
- Auto: the set's statistics are mass-weighted where mass is meaningful — `Rmse = √(Σ π·d² / Σ π)` falling back to count-weighting on a vanishing total; the distance quantiles (`Median`/`Quantile90`/`Quantile95`/`Max`) read one sorted distance array; coverage is fourfold evidence: `CoveredSourceCount`/`CoveredTargetCount` (endpoints with any retained coupling) and `RetainedSourceMass`/`RetainedTargetMass` (the marginal mass surviving the cutoff — the sparsification-loss signal an unbalanced solve reads to see how much measure the relaxation dropped).
- Receipt: the set IS the receipt — `IValidityEvidence`, `IsValid` one `ValidityClaim.All` fold declaring the count terms (`NonZeroCount == Items.Count`, covered counts bounded by source/target counts) and the ordered quantile chain.
- Growth: a correspondence consumer needing a new pairing statistic adds one column on the set and one term in the single-pass fold; `register.md` consumes this set as its soft-assignment input without re-walking the coupling.
- Boundary: the cutoff is the policy's — a consumer re-thresholding retained entries at a second ad-hoc floor is the double-policy defect; quantiles read ONE sort of the distance array (never a re-sort per statistic); index pairs refer to ADMITTED cluster vertices, so `cloud.md`'s `OriginalToUnique` re-indexing has already happened upstream and correspondence indices never see pre-deduplication input positions.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondence(
    int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint, Vector3d Residual,
    double Distance, double SquaredDistance,
    Option<double> SourceMass, Option<double> TargetMass, Option<double> CouplingMass, Option<double> Confidence);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondenceSet(
    Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount, int NonZeroCount,
    double TotalMass, double Rmse, double MedianDistance, double MaxDistance, double Quantile90, double Quantile95,
    int CoveredSourceCount, int CoveredTargetCount, double RetainedSourceMass, double RetainedTargetMass) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: NonZeroCount, expected: Items.Count),
        ValidityClaim.Of(CoveredSourceCount >= 0 && CoveredSourceCount <= SourceCount),
        ValidityClaim.Of(CoveredTargetCount >= 0 && CoveredTargetCount <= TargetCount),
        ValidityClaim.Nonnegative(TotalMass),
        ValidityClaim.Nonnegative(Rmse),
        ValidityClaim.Ordered(lower: MedianDistance, upper: Quantile90),
        ValidityClaim.Ordered(lower: Quantile90, upper: Quantile95),
        ValidityClaim.Ordered(lower: Quantile95, upper: MaxDistance),
        ValidityClaim.Nonnegative(RetainedSourceMass),
        ValidityClaim.Nonnegative(RetainedTargetMass));
}
```

## [05]-[DENSITY_BAR]

| [INDEX] | [AXIS_CONCERN]        | [OWNER]                         | [KIND]                                               | [RAIL]                                     | [CASES] |
| :-----: | :-------------------- | :------------------------------ | :--------------------------------------------------- | :----------------------------------------- | :-----: |
|  [01]   | Transport policy      | `CloudTransportPolicy`          | one record; balanced/unbalanced/debiased as columns  | `Of → Fin<CloudTransportPolicy>`           |    —    |
|  [02]   | Stop outcome          | `SinkhornStopKind`              | `[SmartEnum<int>]` + `Converged` column              | receipt row                                |    4    |
|  [03]   | Solve + projection    | `CloudTransport`/`SinkhornPlan` | one log-domain kernel; five `ProjectionRow` egresses | `Sinkhorn<TOut> → Fin<TOut>`               |    5    |
|  [04]   | Underflow policy      | `LogUnderflowFloor`             | named constant + `SinkhornNumericStatus` evidence    | receipt row                                |    2    |
|  [05]   | Correspondence answer | `CloudCorrespondenceSet`        | thresholded entries + coverage/quantile fold         | `OfCoupling → Fin<CloudCorrespondenceSet>` |    —    |
