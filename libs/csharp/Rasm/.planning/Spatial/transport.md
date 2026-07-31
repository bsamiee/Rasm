# [RASM_TRANSPORT]

`CloudTransport` owns optimal transport between weighted vector clusters through ONE log-domain stabilized Sinkhorn kernel: balanced marginals, KL-relaxed unbalanced marginals, and Sinkhorn-divergence debiasing are POLICY columns of one iteration, never three solver bodies, and every answer leaves through ONE `SinkhornPlan.Project<TOut>` egress.

Transport mass is the cluster's own admitted normalized mass (`cloud.md` `MassOf`), so a weighted cluster IS a discrete measure and no second measure type exists; `register.md` consumes `CloudCorrespondenceSet` as its soft-assignment input without re-walking the coupling.

## [01]-[INDEX]

- [02]-[TRANSPORT_POLICY]: `CloudTransportPolicy` spans the whole solver product in one record.
- [03]-[SINKHORN]: `CloudTransport` solves in log space and `SinkhornPlan` projects every answer.
- [04]-[CORRESPONDENCES]: `CloudCorrespondenceSet` thresholds the coupling into pairings carrying coverage evidence.

## [02]-[TRANSPORT_POLICY]

- Owner: `CloudTransportPolicy` columns span the balanced/unbalanced/debiased product in one record; `CouplingCutoff` is the single sparsification floor below which a coupling entry carries no correspondence.
- Cases: `SinkhornStopKind` carries a `Converged` column, so budget exhaustion reads as a partial plan the caller retries under a wider budget, never a failure. `SinkhornResidualKind` derives from `MassRelaxation`, never a caller flag — the marginal test is meaningless under relaxation.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
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

- Owner: `CloudTransport` owns the solve; `SinkhornPlan` carries the solved plan behind its ONE `Project<TOut>` egress.
- Entry: `Sinkhorn<TOut>` solves once and the requested `TOut` selects the projection row, so every projection caller shares one entry and one solve.
- Auto: log-domain scalings iterate to tolerance or budget under a max-shifted `LogSumExp`, so a fully-improbable row degrades to `−∞` gracefully; a non-finite distance or residual faults the solve.
- Packages: RhinoCommon `Point3d.DistanceToSquared` is the cost kernel; System.Numerics.Tensors `TensorPrimitives` folds the LSE rows and the entropic-cost reduction; LanguageExt.Core and Thinktecture.Runtime.Extensions carry the rails and value objects.
- Growth: a new transport mode is one entry arm over the same kernel and receipt vocabulary, never a second solver body.
- Boundary: flat row-major `double[]` sweeps are the named statement kernel with a `Fin` rail at both edges, confined to the solve body; the coupling leaves only as a `matrix.md` `Matrix` through its projection row; `LogUnderflowFloor` is THE underflow policy, so an ad-hoc `Math.Exp` on an unfloored exponent re-introduces the silent-zero defect; `typeof(TOut)` resolution routes `ProjectionRow` entries through `atoms.md` `AtomProjection.Rows`, never a reflection ladder.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CloudTransport {
    // ln of the smallest positive NORMAL double — below it Math.Exp degrades subnormal-then-zero, so the coupling floors to 0 and records it.
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

    private static Fin<SinkhornPlan> Solve(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, CloudTransportPolicy policy, Op key) {
        (int m, int n, double eps) = (source.Count, target.Count, policy.Regularization.Value);
        double[] logK = new double[m * n];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                logK[(i * n) + j] = -source[i].DistanceToSquared(other: target[j]) / eps;
        double[] logU = new double[m]; double[] logV = new double[n];
        double[] gather = new double[Math.Max(val1: m, val2: n)];      // strided column pull
        double[] fold = new double[Math.Max(val1: m, val2: n)];        // vectorized LSE destination
        double[] logA = [.. sourceMass.AsIterable().Select(Math.Log)]; double[] logB = [.. targetMass.AsIterable().Select(Math.Log)];
        double exponent = policy.MassRelaxation.Match(Some: l => l.Value / (l.Value + eps), None: () => 1.0);
        bool balanced = policy.MassRelaxation.IsNone;
        (double resS, double resT, int iterations) = (double.PositiveInfinity, double.PositiveInfinity, 0);
        for (int iter = 0; iter < policy.MaxIterations.Value && Math.Max(resS, resT) > policy.ConvergenceTolerance.Value; iter++) {
            iterations = iter + 1;
            (double[] prevU, double[] prevV) = ([.. logU], [.. logV]);
            for (int i = 0; i < m; i++) logU[i] = exponent * (logA[i] - LogSumExp(row: logK.AsSpan(i * n, n), shift: logV, scratch: fold.AsSpan(0, n)));
            for (int j = 0; j < n; j++) logV[j] = exponent * (logB[j] - LogSumExpColumn(logK: logK, column: j, rows: m, stride: n, shift: logU, gather: gather.AsSpan(0, m), fold: fold.AsSpan(0, m)));
            (resS, resT) = balanced
                ? MarginalResiduals(logK: logK, logU: logU, logV: logV, a: sourceMass, b: targetMass, m: m, n: n, gather: gather, fold: fold)
                : (MaxDelta(prev: prevU, next: logU, scratch: fold.AsSpan(0, m)), MaxDelta(prev: prevV, next: logV, scratch: fold.AsSpan(0, n)));
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
        double distance = -eps * TensorPrimitives.Dot<double>(coupling, logK);
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

    // Max-shifted LSE over row[j] + shift[j]. Every shifted exponent is <= 0, so no overflow exists and an underflow
    // to zero is the correct contribution; an all-negative-infinity row answers -inf, the graceful degradation the
    // page's Auto line promises. The fold is TensorPrimitives end to end — the Packages claim's one realization.
    private static double LogSumExp(ReadOnlySpan<double> row, ReadOnlySpan<double> shift, Span<double> scratch) {
        TensorPrimitives.Add(x: row, y: shift, destination: scratch);
        double max = TensorPrimitives.Max<double>(x: scratch);
        if (double.IsNegativeInfinity(d: max)) return double.NegativeInfinity;
        TensorPrimitives.Subtract(x: scratch, y: max, destination: scratch);
        TensorPrimitives.Exp<double>(x: scratch, destination: scratch);
        return max + Math.Log(d: TensorPrimitives.Sum<double>(x: scratch));
    }

    // Column form is the load-bearing one — a strided read over the row-major buffer. The gather is the only
    // scalar sweep; the fold that follows is the same vectorized row body, so one LSE spelling serves both axes.
    private static double LogSumExpColumn(double[] logK, int column, int rows, int stride, ReadOnlySpan<double> shift, Span<double> gather, Span<double> fold) {
        for (int i = 0; i < rows; i++) gather[i] = logK[(i * stride) + column];
        return LogSumExp(row: gather, shift: shift, scratch: fold);
    }

    // Worst |prev - next| over the scaling vector. MaxMagnitude returns the largest-magnitude SIGNED element, so one
    // absolute value closes it; a NaN scaling propagates here and the finiteness gate at the solve's exit catches it.
    private static double MaxDelta(ReadOnlySpan<double> prev, ReadOnlySpan<double> next, Span<double> scratch) {
        TensorPrimitives.Subtract(x: next, y: prev, destination: scratch);
        return Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(x: scratch));
    }

    // Balanced-mode marginal error: row mass exp(logU[i] + LSE(logK[i,:] + logV)) against a, column mass likewise
    // against b, each reduced to its worst absolute deviation. The mass exponent passes the SAME LogUnderflowFloor
    // that the coupling emission uses, so residual and coupling agree on what is zero — an unfloored Math.Exp here
    // re-introduces exactly the silent-zero defect the page's Boundary names.
    private static (double Source, double Target) MarginalResiduals(
        double[] logK, double[] logU, double[] logV, Arr<double> a, Arr<double> b, int m, int n, double[] gather, double[] fold) {
        double resS = 0.0, resT = 0.0;
        for (int i = 0; i < m; i++) {
            double log = logU[i] + LogSumExp(row: logK.AsSpan(i * n, n), shift: logV, scratch: fold.AsSpan(0, n));
            resS = Math.Max(val1: resS, val2: Math.Abs(value: (log < LogUnderflowFloor ? 0.0 : Math.Exp(d: log)) - a[i]));
        }
        for (int j = 0; j < n; j++) {
            double log = logV[j] + LogSumExpColumn(logK: logK, column: j, rows: m, stride: n, shift: logU, gather: gather.AsSpan(0, m), fold: fold.AsSpan(0, m));
            resT = Math.Max(val1: resT, val2: Math.Abs(value: (log < LogUnderflowFloor ? 0.0 : Math.Exp(d: log)) - b[j]));
        }
        return (Source: resS, Target: resT);
    }
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

    // One pass over the coupling fills every census column the receipt's own claim fold reads back; RawDistance
    // carries the undebiased cost, so a debiased divergence keeps its nonnegativity evidence on the raw term alone.
    internal Fin<SinkhornReceipt> ReceiptOf(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance,
        Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
        (double mass, int nonZero, double min, double max) = (0.0, 0, double.PositiveInfinity, 0.0);
        foreach (double entry in Coupling) {
            if (entry <= 0.0) continue;
            (mass, nonZero) = (mass + entry, nonZero + 1);
            (min, max) = (Math.Min(val1: min, val2: entry), Math.Max(val1: max, val2: entry));
        }
        return from pairs in Correspondences.OfCoupling(source: source, target: target, coupling: Coupling,
                   rows: Rows, columns: Columns, cutoff: CouplingCutoff, key: key)
               from receipt in key.AcceptValue(value: new SinkhornReceipt(
                   Distance: distance, RawDistance: policy.Debiased ? Some(Distance) : Option<double>.None,
                   SourceBiasDistance: sourceBias, TargetBiasDistance: targetBias,
                   Regularization: policy.Regularization.Value, MassRelaxation: policy.MassRelaxation.Map(static l => l.Value),
                   ConvergenceTolerance: ConvergenceTolerance, CouplingCutoff: CouplingCutoff, Debiased: policy.Debiased,
                   ResidualKind: policy.ResidualKind,
                   NumericStatus: UnderflowFloored ? SinkhornNumericStatus.UnderflowFloored : SinkhornNumericStatus.FiniteAccepted,
                   SourceConvergenceResidual: SourceConvergenceResidual, TargetConvergenceResidual: TargetConvergenceResidual,
                   Iterations: Iterations, Stop: Stop, CouplingMass: mass, NonZeroCouplings: nonZero,
                   MinPositiveCoupling: nonZero > 0 ? Some(min) : Option<double>.None,
                   MaxCoupling: nonZero > 0 ? Some(max) : Option<double>.None, Correspondences: pairs))
               select receipt;
    }

    // Row i maps to the mass-weighted target barycentre; a vanishing row mass carries no image, so the row drops
    // rather than collapsing onto the origin, and VectorCloud.Cluster re-admits the survivors as a measure.
    internal Fin<VectorCloud> BarycentricImage(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, Op key) {
        Seq<Point3d> image = default;
        for (int i = 0; i < Rows; i++) {
            (double mass, Vector3d weighted) = (0.0, Vector3d.Zero);
            for (int j = 0; j < Columns; j++) {
                double pi = Coupling[(i * Columns) + j];
                (mass, weighted) = (mass + pi, weighted + (pi * (Vector3d)target.Vertices[j]));
            }
            if (mass > EpsilonPolicy.ZeroTolerance) image = image.Add(new Point3d(weighted / mass));
        }
        return VectorCloud.Cluster(points: image, context: target.Tolerance, key: key);
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornReceipt(
    double Distance, Option<double> RawDistance, Option<double> SourceBiasDistance, Option<double> TargetBiasDistance,
    double Regularization, Option<double> MassRelaxation, double ConvergenceTolerance, double CouplingCutoff, bool Debiased,
    SinkhornResidualKind ResidualKind, SinkhornNumericStatus NumericStatus,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop,
    double CouplingMass, int NonZeroCouplings, Option<double> MinPositiveCoupling, Option<double> MaxCoupling,
    CloudCorrespondenceSet Correspondences) : IValidityEvidence {
    // Distance claims finiteness only — a debiased divergence sits epsilon-negative, so nonnegativity rides the raw cost alone.
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

- Owner: `CloudCorrespondence` is one thresholded coupling entry and `CloudCorrespondenceSet` the ordered collection; `Confidence` is `min(1, π[i,j]/max(aᵢ, bⱼ))`, how decisively a pairing claims its endpoints.
- Entry: `Correspondences.OfCoupling` walks the coupling above the cutoff and folds every statistic in one pass.
- Auto: `Rmse = √(Σ π·d² / Σ π)` weights by mass and falls back to count-weighting on a vanishing total; retained source and target mass is the sparsification-loss signal an unbalanced solve reads to see how much measure the relaxation dropped.
- Growth: a new pairing statistic is one column on the set and one term in the single-pass fold.
- Boundary: the cutoff is the policy's, so a consumer re-thresholding retained entries at a second ad-hoc floor is the double-policy defect; quantiles read ONE sort of the distance array; index pairs refer to ADMITTED cluster vertices, so `cloud.md`'s `OriginalToUnique` re-indexing has already happened upstream and correspondence indices never see pre-deduplication positions.

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

// --- [OPERATIONS] -------------------------------------------------------------------------
// THE one coupling-to-pairing fold. Every statistic accumulates in the single walk above the cutoff, and the
// quantile set reads ONE sort of the collected distances — a second sort per quantile is the deleted form.
internal static class Correspondences {
    internal static Fin<CloudCorrespondenceSet> OfCoupling(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
        double[] coupling, int rows, int columns, double cutoff, Op key) =>
        from sourceMass in CloudKernel.MassOf(cluster: source, key: key)
        from targetMass in CloudKernel.MassOf(cluster: target, key: key)
        from set in key.AcceptValue(value: Fold(source: source, target: target, coupling: coupling, rows: rows,
            columns: columns, cutoff: cutoff, a: sourceMass, b: targetMass))
        select set;

    private static CloudCorrespondenceSet Fold(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
        double[] coupling, int rows, int columns, double cutoff, Arr<double> a, Arr<double> b) {
        Seq<CloudCorrespondence> items = default;
        (double total, double weightedSquared, double maxDistance) = (0.0, 0.0, 0.0);
        (double retainedSource, double retainedTarget) = (0.0, 0.0);
        bool[] coveredSource = new bool[rows]; bool[] coveredTarget = new bool[columns];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                double pi = coupling[(i * columns) + j];
                if (pi <= cutoff) continue;
                (Point3d sp, Point3d tp) = (source.Vertices[i], target.Vertices[j]);
                double squared = sp.DistanceToSquared(other: tp), distance = Math.Sqrt(d: squared);
                double denominator = Math.Max(val1: a[i], val2: b[j]);
                items = items.Add(new CloudCorrespondence(SourceIndex: i, TargetIndex: j, SourcePoint: sp, TargetPoint: tp,
                    Residual: tp - sp, Distance: distance, SquaredDistance: squared,
                    SourceMass: Some(a[i]), TargetMass: Some(b[j]), CouplingMass: Some(pi),
                    Confidence: denominator > EpsilonPolicy.ZeroTolerance ? Some(Math.Min(val1: 1.0, val2: pi / denominator)) : Option<double>.None));
                (total, weightedSquared) = (total + pi, weightedSquared + (pi * squared));
                maxDistance = Math.Max(val1: maxDistance, val2: distance);
                if (!coveredSource[i]) { coveredSource[i] = true; retainedSource += a[i]; }
                if (!coveredTarget[j]) { coveredTarget[j] = true; retainedTarget += b[j]; }
            }
        }
        // ONE sort serves median and both tail quantiles; R-7 matches Domain/stats Distribution.Of so a transport
        // quantile and a statistics quantile never disagree on the same sample set.
        double[] sorted = [.. items.Map(static item => item.Distance).OrderBy(static d => d)];
        double rmse = total > EpsilonPolicy.ZeroTolerance
            ? Math.Sqrt(d: weightedSquared / total)
            : sorted.Length > 0 ? Math.Sqrt(d: sorted.Sum(static d => d * d) / sorted.Length) : 0.0;
        return new CloudCorrespondenceSet(Items: items, SourceCount: rows, TargetCount: columns, NonZeroCount: items.Count,
            TotalMass: total, Rmse: rmse, MedianDistance: Quantile(sorted: sorted, p: 0.50),
            MaxDistance: maxDistance, Quantile90: Quantile(sorted: sorted, p: 0.90), Quantile95: Quantile(sorted: sorted, p: 0.95),
            CoveredSourceCount: coveredSource.Count(static c => c), CoveredTargetCount: coveredTarget.Count(static c => c),
            RetainedSourceMass: retainedSource, RetainedTargetMass: retainedTarget);
    }

    // R-7 linear interpolation on the ALREADY-sorted array — the Domain/stats Distribution.Of convention verbatim.
    private static double Quantile(double[] sorted, double p) {
        if (sorted.Length == 0) return 0.0;
        double h = (sorted.Length - 1) * p;
        int lo = (int)Math.Floor(d: h);
        return lo + 1 < sorted.Length ? sorted[lo] + ((h - lo) * (sorted[lo + 1] - sorted[lo])) : sorted[^1];
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
