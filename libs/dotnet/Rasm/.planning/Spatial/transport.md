# [RASM_TRANSPORT]

`CloudTransport` owns optimal transport between weighted vector clusters through ONE log-domain stabilized Sinkhorn kernel: balanced marginals, KL-relaxed unbalanced marginals, and Sinkhorn-divergence debiasing are POLICY columns of one iteration, never three solver bodies, and every answer leaves through ONE `SinkhornPlan.Project<TOut>` egress.

Transport mass is the cluster's own admitted normalized mass (`cloud.md` `MassOf`), so a weighted cluster IS a discrete measure and no second measure type exists; `register.md` consumes `CloudCorrespondenceSet` as its soft-assignment input without re-walking the coupling.

Estimators ride a `TransportEstimator` row, never a debias flag; the iteration budget is a bounded `FoldUntil` over the generated range, and both numeric floors read `Domain/context` lanes — `Convergence` for the residual target and `Neglect` for the sparsification cutoff — so no page literal states either.

## [01]-[INDEX]

- [02]-[TRANSPORT_POLICY]: `TransportEstimator` rows and `CloudTransportPolicy` span the whole solver product in one record.
- [03]-[SINKHORN]: `CloudTransport` solves in log space and `SinkhornPlan` projects every answer.
- [04]-[CORRESPONDENCES]: `CloudCorrespondenceSet` thresholds the coupling into pairings carrying coverage evidence.

## [02]-[TRANSPORT_POLICY]

- Owner: `CloudTransportPolicy` columns span the balanced/unbalanced/debiased product in one record; `CouplingCutoff` is the single sparsification floor below which a coupling entry carries no correspondence.
- Cases: `TransportEstimator` rows name WHICH divergence the solve reports — `Entropic` the raw regularized cost, `Debiased` the Sinkhorn divergence that subtracts both self-transport halves — so the two-solve leg is a row the fold switches on rather than a boolean the reader re-interprets at every site. `SinkhornResidualKind` derives from `MassRelaxation`, never a caller flag — the marginal test is meaningless under relaxation — and `SinkhornStopKind` is the `(residual kind × converged)` PRODUCT of that derivation, minted only through `Of` so the stop cannot disagree with the residual it reports; budget exhaustion therefore reads as a partial plan the caller retries under a wider budget, never a failure. NAMED LOSS: the receipt no longer re-derives convergence from its own residual pair, because `Advance` is the single comparison site and a second derivation was the second authority the mirror existed to police.
- Law: the two floors are `Domain/context` lanes, not page constants — `ToleranceLane.Convergence` sets the residual target and `ToleranceLane.Neglect` the coupling cutoff, so a model that tightens either tightens this solve with no second knob.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SinkhornResidualKind {
    public static readonly SinkhornResidualKind MarginalMass = new(key: 0);
    public static readonly SinkhornResidualKind ScalingChange = new(key: 1);
}

// Rows ARE the (residual kind x converged) product, so `Of` is that product's own total fold and the pair
// has ONE authority: `SinkhornReceipt.ResidualKind` READS `Stop.Residual` instead of storing a copy beside it.
[SmartEnum<int>]
public sealed partial class SinkhornStopKind {
    public static readonly SinkhornStopKind BalancedMarginalsConverged = new(key: 0, residual: SinkhornResidualKind.MarginalMass, converged: true);
    public static readonly SinkhornStopKind RelaxedScalingConverged = new(key: 1, residual: SinkhornResidualKind.ScalingChange, converged: true);
    public static readonly SinkhornStopKind BalancedMarginalsStoppedWithoutConvergence = new(key: 2, residual: SinkhornResidualKind.MarginalMass, converged: false);
    public static readonly SinkhornStopKind RelaxedScalingStoppedWithoutConvergence = new(key: 3, residual: SinkhornResidualKind.ScalingChange, converged: false);
    public SinkhornResidualKind Residual { get; }
    public bool Converged { get; }

    internal static SinkhornStopKind Of(SinkhornResidualKind residual, bool converged) => residual.Switch(
        marginalMass: () => converged ? BalancedMarginalsConverged : BalancedMarginalsStoppedWithoutConvergence,
        scalingChange: () => converged ? RelaxedScalingConverged : RelaxedScalingStoppedWithoutConvergence);
}

[SmartEnum<int>]
public sealed partial class SinkhornNumericStatus {
    public static readonly SinkhornNumericStatus FiniteAccepted = new(key: 0);
    public static readonly SinkhornNumericStatus UnderflowFloored = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class TransportEstimator {
    public static readonly TransportEstimator Entropic = new(key: 0);
    public static readonly TransportEstimator Debiased = new(key: 1);
}

// The bounded fold advances this continue-or-done step: `Advance` still exceeds its residual target, `Settled` met it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SinkhornStep {
    private SinkhornStep() { }
    public sealed record Advance(int Iterations, double SourceResidual, double TargetResidual) : SinkhornStep;
    public sealed record Settled(int Iterations, double SourceResidual, double TargetResidual) : SinkhornStep;

    internal (int Iterations, double Source, double Target) Reading => Switch(
        advance: static a => (a.Iterations, a.SourceResidual, a.TargetResidual),
        settled: static t => (t.Iterations, t.SourceResidual, t.TargetResidual));
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudTransportPolicy(
    PositiveMagnitude Regularization, Dimension MaxIterations, TransportEstimator Estimator,
    Option<PositiveMagnitude> MassRelaxation, PositiveMagnitude ConvergenceTolerance, PositiveMagnitude CouplingCutoff) {
    public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, Context context,
        Option<TransportEstimator> estimator = default, Option<double> massRelaxation = default,
        Option<double> convergenceTolerance = default, Option<double> couplingCutoff = default, Op? key = null) {
        Op op = key.OrDefault();
        return from reg in op.AcceptValidated<PositiveMagnitude>(candidate: regularization)
               from cap in op.AcceptValidated<Dimension>(candidate: maxIterations)
               from relax in massRelaxation.Match(
                   Some: lambda => op.AcceptValidated<PositiveMagnitude>(candidate: lambda).Map(Some),
                   None: static () => Fin.Succ(Option<PositiveMagnitude>.None))
               from tolerance in op.AcceptValidated<PositiveMagnitude>(
                   candidate: convergenceTolerance.IfNone(context.For(lane: ToleranceLane.Convergence).Value))
               // Neglect is the branch's below-which-ignore lane, which is exactly what a coupling cutoff is: an
               // entry under it carries no correspondence and is dropped rather than rounded.
               from cutoff in op.AcceptValidated<PositiveMagnitude>(
                   candidate: couplingCutoff.IfNone(context.For(lane: ToleranceLane.Neglect).Value))
               select new CloudTransportPolicy(Regularization: reg, MaxIterations: cap,
                   Estimator: estimator.IfNone(TransportEstimator.Entropic),
                   MassRelaxation: relax, ConvergenceTolerance: tolerance, CouplingCutoff: cutoff);
    }
    internal SinkhornResidualKind ResidualKind => MassRelaxation.IsSome ? SinkhornResidualKind.ScalingChange : SinkhornResidualKind.MarginalMass;
}
```

## [03]-[SINKHORN]

- Owner: `CloudTransport` owns the solve; `SinkhornPlan` carries the solved plan behind its ONE `Project<TOut>` egress.
- Entry: `Sinkhorn<TOut>` solves once and the requested `TOut` selects the projection row, so every projection caller shares one entry and one solve. `policy.Estimator` selects the debias leg through a generated `Switch`, so the two-solve arm is a case rather than a branch on a flag.
- Auto: log-domain scalings iterate under a max-shifted `LogSumExp`, so a fully-improbable row degrades to `−∞` gracefully; a non-finite distance or residual faults the solve. `Range.FoldUntil` owns the exact iteration budget and stops on the step's settled case.
- Law: the budget is bounded and its exhaustion is EVIDENCE, not silence — a run that stops on the schedule leaves `SinkhornStep.Advance` and the plan publishes `SinkhornStopKind.*StoppedWithoutConvergence`. Because that evidence rides the receipt alone, `Project<TOut>` REFUSES the evidence-free rows — `double`, `Matrix`, `CloudCorrespondenceSet`, `VectorCloud` — on an unconverged plan, since handing a bare cost out of an exhausted run is exactly the success-shaped fall-through that certifies unconverged as converged. `SinkhornReceipt` admits it, because that shape carries the stop.
- Packages: RhinoCommon `Point3d.DistanceToSquared` is the cost kernel; System.Numerics.Tensors `TensorPrimitives` folds the LSE rows, the coupling emission, and the entropic-cost reduction; CommunityToolkit.HighPerformance `Memory2D<T>`/`Span2D<T>.GetRowSpan` addresses both `(m, n)` planes, so no stride arithmetic and no stride argument survive; `Rasm.Domain` `Stat<Scalar>`/`Distribution<Scalar>` own every moment and order statistic the receipts publish; LanguageExt.Core carries the rails, value objects, and bounded fold; Thinktecture.Runtime.Extensions carries the generated vocabularies.
- Growth: a new transport mode is one `TransportEstimator` row and one arm over the same kernel and receipt vocabulary, never a second solver body; a new stop species is one `SinkhornStopKind` row.
- Boundary: the two `(m, n)` planes are the named statement kernel with a `Fin` rail at both edges, confined to the solve body — ONE rental each, addressed through the `Memory2D<T>` view, so the flat buffer serves only the whole-plane `TensorPrimitives` reductions and no site re-derives an offset; the coupling leaves only as a `matrix.md` `Matrix` through its projection row; `MinPositiveNormal` is THE underflow anchor and `LogUnderflowFloor` derives from it, so an ad-hoc `Math.Exp` on an unfloored exponent re-introduces the silent-zero defect; `typeof(TOut)` resolution routes `ProjectionRow` entries through `atoms.md` `AtomProjection.Rows`, never a reflection ladder.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CloudTransport {
    // The bit pattern IS the smallest positive NORMAL double's definition, so both anchors DERIVE from one read and
    // a transcription slip in a hand-typed mantissa's last digits cannot shift the floor silently. Below the log
    // floor `Math.Exp` degrades subnormal-then-zero, so the coupling floors to exactly 0 and the row records it.
    internal static readonly double MinPositiveNormal = BitConverter.UInt64BitsToDouble(0x0010_0000_0000_0000UL);
    internal static readonly double LogUnderflowFloor = Math.Log(d: MinPositiveNormal);

    public static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) =>
                from srcMass in CloudKernel.MassOf(cluster: src, key: op)
                from tgtMass in CloudKernel.MassOf(cluster: tgt, key: op)
                from plan in Solve(source: src.Vertices, target: tgt.Vertices, sourceMass: srcMass, targetMass: tgtMass, policy: policy, key: op)
                from bias in policy.Estimator.Switch(
                    entropic: () => Fin.Succ((Source: Option<double>.None, Target: Option<double>.None, Distance: plan.Distance)),
                    debiased: () =>
                        from selfS in Solve(source: src.Vertices, target: src.Vertices, sourceMass: srcMass, targetMass: srcMass, policy: policy, key: op)
                        from selfT in Solve(source: tgt.Vertices, target: tgt.Vertices, sourceMass: tgtMass, targetMass: tgtMass, policy: policy, key: op)
                        select (Source: Some(selfS.Distance), Target: Some(selfT.Distance),
                                Distance: plan.Distance - (0.5 * selfS.Distance) - (0.5 * selfT.Distance)))
                from output in plan.Project<TOut>(source: src, target: tgt, distance: bias.Distance,
                    sourceBias: bias.Source, targetBias: bias.Target, policy: policy, key: op)
                select output,
            _ => Fin.Fail<TOut>(op.Unsupported(inputType: source.GetType(), outputType: typeof(TOut))),
        };
    }

    private static Fin<SinkhornPlan> Solve(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, CloudTransportPolicy policy, Op key) {
        (int m, int n, double eps) = (source.Count, target.Count, policy.Regularization.Value);
        // ONE buffer, TWO views: the flat rental is what `TensorPrimitives` reduces whole, the `(m, n)` plane what
        // every row read addresses — so `(i * n) + j` never appears and a stride argument never crosses a signature.
        double[] costs = new double[m * n];
        Memory2D<double> logK = costs.AsMemory2D(height: m, width: n);
        for (int i = 0; i < m; i++) {
            Span<double> row = logK.Span.GetRowSpan(i);
            for (int j = 0; j < n; j++) row[j] = -source[i].DistanceToSquared(other: target[j]) / eps;
        }
        double[] logU = new double[m]; double[] logV = new double[n];
        double[] gather = new double[Math.Max(val1: m, val2: n)];      // strided column pull
        double[] fold = new double[Math.Max(val1: m, val2: n)];        // vectorized LSE destination
        double[] logA = [.. sourceMass.AsIterable().Select(Math.Log)]; double[] logB = [.. targetMass.AsIterable().Select(Math.Log)];
        double exponent = policy.MassRelaxation.Match(Some: l => l.Value / (l.Value + eps), None: () => 1.0);
        int step = 0;
        SinkhornStep Advance() {
            step++;
            (double[] prevU, double[] prevV) = ([.. logU], [.. logV]);
            for (int i = 0; i < m; i++) logU[i] = exponent * (logA[i] - LogSumExp(row: logK.Span.GetRowSpan(i), shift: logV, scratch: fold.AsSpan(0, n)));
            for (int j = 0; j < n; j++) logV[j] = exponent * (logB[j] - LogSumExpColumn(logK: logK.Span, column: j, rows: m, shift: logU, gather: gather.AsSpan(0, m), fold: fold.AsSpan(0, m)));
            (double s, double t) = policy.ResidualKind.Switch<(double Source, double Target)>(
                marginalMass: () => MarginalResiduals(logK: logK, logU: logU, logV: logV, a: sourceMass, b: targetMass, m: m, n: n, gather: gather, fold: fold),
                scalingChange: () => (MaxDelta(prev: prevU, next: logU, scratch: fold.AsSpan(0, m)), MaxDelta(prev: prevV, next: logV, scratch: fold.AsSpan(0, n))));
            return Math.Max(s, t) <= policy.ConvergenceTolerance.Value
                ? new SinkhornStep.Settled(Iterations: step, SourceResidual: s, TargetResidual: t)
                : new SinkhornStep.Advance(Iterations: step, SourceResidual: s, TargetResidual: t);
        }
        SinkhornStep settled = Range(0, policy.MaxIterations.Value).FoldUntil(
            state: (SinkhornStep)new SinkhornStep.Advance(Iterations: 0, SourceResidual: double.PositiveInfinity, TargetResidual: double.PositiveInfinity),
            f: (_, _) => Advance(),
            stateP: static state => state is SinkhornStep.Settled);
        (int iterations, double resS, double resT) = settled.Reading;
        // Emission runs one ROW at a time on the vector rail — add both scalings, read the row's own minimum, then
        // exponentiate — so the per-slot `Seq<int>` and tuple allocation the old fold paid buy nothing a row span
        // does not. Only a row that actually reached the floor pays the zeroing sweep: past the floor `Math.Exp`
        // lands subnormal-or-zero, and the policy is exactly 0, never a drifting denormal a later ratio divides by.
        double[] entries = new double[m * n];
        Memory2D<double> coupling = entries.AsMemory2D(height: m, width: n);
        bool floored = false;
        for (int i = 0; i < m; i++) {
            Span<double> row = coupling.Span.GetRowSpan(i);
            TensorPrimitives.Add(x: logK.Span.GetRowSpan(i), y: logV, destination: row);
            TensorPrimitives.Add(x: row, y: logU[i], destination: row);
            bool rowFloored = TensorPrimitives.Min<double>(x: row) < LogUnderflowFloor;
            TensorPrimitives.Exp<double>(x: row, destination: row);
            if (rowFloored) {
                floored = true;
                for (int j = 0; j < n; j++) if (row[j] < MinPositiveNormal) row[j] = 0.0;
            }
        }
        double distance = -eps * TensorPrimitives.Dot<double>(entries, costs);
        return double.IsFinite(distance) && double.IsFinite(resS) && double.IsFinite(resT)
            ? Fin.Succ(new SinkhornPlan(Distance: distance, Coupling: entries, Rows: m, Columns: n,
                SourceConvergenceResidual: resS, TargetConvergenceResidual: resT, Iterations: iterations,
                Stop: SinkhornStopKind.Of(residual: policy.ResidualKind, converged: settled is SinkhornStep.Settled),
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

    // Column form is the load-bearing one — a column pull across the plane's rows, the stride the plane's own so no
    // caller passes one. The gather is the only scalar sweep; the fold that follows is the same vectorized row body,
    // so one LSE spelling serves both axes.
    private static double LogSumExpColumn(Span2D<double> logK, int column, int rows, ReadOnlySpan<double> shift, Span<double> gather, Span<double> fold) {
        for (int i = 0; i < rows; i++) gather[i] = logK.GetRowSpan(i)[column];
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
        Memory2D<double> logK, double[] logU, double[] logV, Arr<double> a, Arr<double> b, int m, int n, double[] gather, double[] fold) {
        double resS = 0.0, resT = 0.0;
        for (int i = 0; i < m; i++) {
            double log = logU[i] + LogSumExp(row: logK.Span.GetRowSpan(i), shift: logV, scratch: fold.AsSpan(0, n));
            resS = Math.Max(val1: resS, val2: Math.Abs(value: (log < LogUnderflowFloor ? 0.0 : Math.Exp(d: log)) - a[i]));
        }
        for (int j = 0; j < n; j++) {
            double log = logV[j] + LogSumExpColumn(logK: logK.Span, column: j, rows: m, shift: logU, gather: gather.AsSpan(0, m), fold: fold.AsSpan(0, m));
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
    // `Rows`/`Columns` are the declared shape and `Plane` its ONE derived view, so a row read addresses the plane and
    // a whole-buffer reduction addresses the rental — neither re-derives `(i * Columns) + j`.
    internal Memory2D<double> Plane => Coupling.AsMemory2D(height: Rows, width: Columns);

    // Every row but the receipt DROPS the stop evidence, so an unconverged plan refuses them: a bare cost, a
    // coupling matrix, or a correspondence set out of an exhausted run is indistinguishable from a settled one.
    internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance,
        Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
        SinkhornPlan self = this;
        Fin<T> Settled<T>(Func<Fin<T>> row) =>
            self.Stop.Converged ? row() : Fin.Fail<T>(error: key.InvalidResult(detail: $"sinkhorn-unconverged:{self.Iterations}"));
        return AtomProjection.Rows<SinkhornPlan, TOut>(self: self, key: key, owner: typeof(VectorCloud),
            ProjectionRow.Of<double>(() => Settled(() => key.AcceptValue(value: distance))),
            ProjectionRow.Of<SinkhornReceipt>(() => self.ReceiptOf(source: source, target: target, distance: distance,
                sourceBias: sourceBias, targetBias: targetBias, policy: policy, key: key)),
            ProjectionRow.Of<CloudCorrespondenceSet>(() => Settled(() => Correspondences.OfCoupling(source: source, target: target,
                coupling: self.Plane, cutoff: self.CouplingCutoff, key: key))),
            ProjectionRow.Of<Matrix>(() => Settled(() => Matrix.Of(rows: Dimension.Create(value: self.Rows), cols: Dimension.Create(value: self.Columns),
                entries: new Arr<double>([.. self.Coupling]), key: key))),
            ProjectionRow.Of<VectorCloud>(() => Settled(() => self.BarycentricImage(target: target, key: key))));
    }

    // The retained-coupling census reads the SAME `CouplingCutoff` the correspondence fold reads, so the receipt and
    // its pairing set count one population and a reader comparing the two never sees a phantom loss. `Stat<Scalar>`
    // is the branch's ONE moment owner — count, extrema, and the mean whose product with the count is the retained
    // mass — so four hand columns and their reducer roster leave together. RawDistance carries the undebiased cost,
    // so a debiased divergence keeps its nonnegativity evidence on the raw term alone.
    internal Fin<SinkhornReceipt> ReceiptOf(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance,
        Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
        Seq<Scalar> retained = toSeq(Coupling.Where(entry => entry > CouplingCutoff).Select(static entry => (Scalar)entry));
        return from census in retained.IsEmpty
                   ? Fin.Succ(Option<Stat<Scalar>>.None)
                   : Stat<Scalar>.Of(values: retained, key: key).Map(Some)
               from pairs in Correspondences.OfCoupling(source: source, target: target, coupling: Plane,
                   cutoff: CouplingCutoff, key: key)
               from receipt in key.AcceptValue(value: new SinkhornReceipt(
                   Distance: distance, RawDistance: policy.Estimator.Equals(TransportEstimator.Debiased) ? Some(Distance) : Option<double>.None,
                   SourceBiasDistance: sourceBias, TargetBiasDistance: targetBias,
                   Regularization: policy.Regularization.Value, MassRelaxation: policy.MassRelaxation.Map(static l => l.Value),
                   ConvergenceTolerance: ConvergenceTolerance, CouplingCutoff: CouplingCutoff, Estimator: policy.Estimator,
                   NumericStatus: UnderflowFloored ? SinkhornNumericStatus.UnderflowFloored : SinkhornNumericStatus.FiniteAccepted,
                   SourceConvergenceResidual: SourceConvergenceResidual, TargetConvergenceResidual: TargetConvergenceResidual,
                   Iterations: Iterations, Stop: Stop, Coupling: census, Correspondences: pairs))
               select receipt;
    }

    // Row i maps to the mass-weighted target barycentre; a row mass at or under the POLICY cutoff carries no image,
    // so the row drops rather than collapsing onto the origin, and VectorCloud.Cluster re-admits the survivors as a
    // measure. The floor is the plan's own cutoff — a second ad-hoc epsilon here is the double-policy defect the
    // page's [04] Boundary forbids, and a length-scale anchor answers nothing about a probability mass.
    internal Fin<VectorCloud> BarycentricImage(VectorCloud.ClusterCase target, Op key) {
        Seq<Point3d> image = default;
        for (int i = 0; i < Rows; i++) {
            ReadOnlySpan<double> row = Plane.Span.GetRowSpan(i);
            (double mass, Vector3d weighted) = (0.0, Vector3d.Zero);
            for (int j = 0; j < row.Length; j++) {
                (mass, weighted) = (mass + row[j], weighted + (row[j] * (Vector3d)target.Vertices[j]));
            }
            if (mass > CouplingCutoff) image = image.Add(new Point3d(weighted / mass));
        }
        return VectorCloud.Cluster(points: image, context: target.Tolerance, key: key);
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornReceipt(
    double Distance, Option<double> RawDistance, Option<double> SourceBiasDistance, Option<double> TargetBiasDistance,
    double Regularization, Option<double> MassRelaxation, double ConvergenceTolerance, double CouplingCutoff,
    TransportEstimator Estimator, SinkhornNumericStatus NumericStatus,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop,
    Option<Stat<Scalar>> Coupling, CloudCorrespondenceSet Correspondences) : IValidityEvidence {
    public SinkhornResidualKind ResidualKind => Stop.Residual;
    // Retained mass is mean x count on the summary that already holds both; a fifth stored column would be a second
    // authority for a figure the moments own. A `None` census is a fold that ran above the cutoff and retained none.
    public int NonZeroCouplings => Coupling.Map(static census => census.Count).IfNone(0);
    public Option<double> CouplingMass => Coupling.Map(static census => census.Mean * census.Count);
    // Distance claims finiteness only — a debiased divergence sits epsilon-negative, so nonnegativity rides the raw cost alone.
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Distance),
        RawDistance.Map(static d => ValidityClaim.Nonnegative(d).Holds).IfNone(Estimator.Equals(TransportEstimator.Entropic)),
        Estimator.Equals(TransportEstimator.Debiased) == (SourceBiasDistance.IsSome && TargetBiasDistance.IsSome),
        ValidityClaim.Positive(Regularization),
        MassRelaxation.Map(static l => ValidityClaim.Positive(l).Holds).IfNone(true),
        ValidityClaim.Positive(ConvergenceTolerance),
        ValidityClaim.Positive(CouplingCutoff),
        Stop.Residual.Equals(MassRelaxation.IsSome ? SinkhornResidualKind.ScalingChange : SinkhornResidualKind.MarginalMass),
        ValidityClaim.Nonnegative(SourceConvergenceResidual),
        ValidityClaim.Nonnegative(TargetConvergenceResidual),
        ValidityClaim.CountAtLeast(count: Iterations, floor: 1),
        // ONE cutoff, ONE population: the census and the pairing set walk the same coupling above the same floor, so
        // a mismatch is data loss and states so here rather than leaving two readers to notice a discrepancy.
        ValidityClaim.CountExactly(count: NonZeroCouplings, expected: Correspondences.NonZeroCount),
        Coupling.Map(static census => ValidityClaim.Evidence(Some(census)).Holds
            && ValidityClaim.Positive(census.Minimum.To()).Holds
            && ValidityClaim.Positive(census.Mean).Holds).IfNone(true),
        ValidityClaim.Evidence(Some(Correspondences)));
}
```

## [04]-[CORRESPONDENCES]

- Owner: `CloudCorrespondence` is one thresholded coupling entry and `CloudCorrespondenceSet` the ordered collection; `Confidence` is `min(1, π[i,j]/max(aᵢ, bⱼ))`, how decisively a pairing claims its endpoints.
- Entry: `Correspondences.OfCoupling` walks the coupling above the cutoff and folds every statistic in one pass.
- Auto: `Rmse = √(Σ π·d² / Σ π)` weights by mass and falls back to the summary's own unweighted `Rms` on a vanishing total, riding an `Option` because an empty pairing set measured no error and a zero there reads as a perfect registration; retained source and target mass derives off the covered `Set<int>` partition after the walk, the sparsification-loss signal an unbalanced solve reads to see how much measure the relaxation dropped.
- Growth: a new pairing statistic is one column on the set and one term in the single-pass fold; a new order statistic is one percentile on the `Distribution<Scalar>` call and no new column at all.
- Boundary: the cutoff is the policy's and EVERY floor on this rail reads it — the census, the confidence denominator, the RMSE fallback, and the barycentric row mass — so a second ad-hoc epsilon is the double-policy defect and a length-scale anchor never gates a probability mass; order statistics come off `Domain/stats` `Distribution<Scalar>`, the branch's ONE exact-quantile owner, so a transport quantile and a statistics quantile cannot disagree on one sample set; index pairs refer to ADMITTED cluster vertices, so `cloud.md`'s `OriginalToUnique` re-indexing has already happened upstream and correspondence indices never see pre-deduplication positions.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondence(
    int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint, Vector3d Residual,
    double Distance, double SquaredDistance,
    Option<double> SourceMass, Option<double> TargetMass, Option<double> CouplingMass, Option<double> Confidence);

// `Distances` is the ONE order-statistic owner over the pairing distances — median, the two tails, the extrema, and
// the summary moments arrive together — so the four hand columns and their transcribed R-7 body leave with it, and a
// coupling that retained nothing publishes `None` rather than five zeros reading as a perfect registration.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondenceSet(
    Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount, int NonZeroCount,
    double TotalMass, Option<double> Rmse, Option<Distribution<Scalar>> Distances,
    int CoveredSourceCount, int CoveredTargetCount, double RetainedSourceMass, double RetainedTargetMass) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: NonZeroCount, expected: Items.Count),
        CoveredSourceCount >= 0 && CoveredSourceCount <= SourceCount,
        CoveredTargetCount >= 0 && CoveredTargetCount <= TargetCount,
        ValidityClaim.Nonnegative(TotalMass),
        // Both measures ride the SAME emptiness: an empty pairing set measured neither, so neither may read zero.
        Distances.IsSome == (Items.Count > 0),
        Rmse.IsSome == Distances.IsSome,
        Rmse.Map(static r => ValidityClaim.Nonnegative(r).Holds).IfNone(true),
        Distances.Map(static spread => ValidityClaim.Evidence(Some(spread)).Holds).IfNone(true),
        ValidityClaim.Nonnegative(RetainedSourceMass),
        ValidityClaim.Nonnegative(RetainedTargetMass));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// THE one coupling-to-pairing fold. Every mass statistic accumulates in the single walk above the cutoff, and the
// whole order-statistic set comes off `Distribution<Scalar>` — a page-local R-7 body is a second recurrence that
// drifts from the kernel's the first time either moves.
internal static class Correspondences {
    internal static Fin<CloudCorrespondenceSet> OfCoupling(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
        Memory2D<double> coupling, double cutoff, Op key) =>
        from sourceMass in CloudKernel.MassOf(cluster: source, key: key)
        from targetMass in CloudKernel.MassOf(cluster: target, key: key)
        from folded in Fold(source: source, target: target, coupling: coupling, cutoff: cutoff, a: sourceMass, b: targetMass, key: key)
        from set in key.AcceptValue(value: folded)
        select set;

    private static Fin<CloudCorrespondenceSet> Fold(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
        Memory2D<double> coupling, double cutoff, Arr<double> a, Arr<double> b, Op key) {
        (int rows, int columns) = (coupling.Height, coupling.Width);
        Seq<CloudCorrespondence> items = default;
        (double total, double weightedSquared) = (0.0, 0.0);
        // The covered sets are the DECLARED partition carrier, so coverage has one representation and its two counts
        // and two retained masses derive after the walk instead of braiding an accumulation into the emit loop.
        (Set<int> coveredSource, Set<int> coveredTarget) = (Set<int>.Empty, Set<int>.Empty);
        for (int i = 0; i < rows; i++) {
            ReadOnlySpan<double> row = coupling.Span.GetRowSpan(i);
            for (int j = 0; j < columns; j++) {
                double pi = row[j];
                if (pi <= cutoff) continue;
                (Point3d sp, Point3d tp) = (source.Vertices[i], target.Vertices[j]);
                double squared = sp.DistanceToSquared(other: tp), distance = Math.Sqrt(d: squared);
                double denominator = Math.Max(val1: a[i], val2: b[j]);
                items = items.Add(new CloudCorrespondence(SourceIndex: i, TargetIndex: j, SourcePoint: sp, TargetPoint: tp,
                    Residual: tp - sp, Distance: distance, SquaredDistance: squared,
                    SourceMass: Some(a[i]), TargetMass: Some(b[j]), CouplingMass: Some(pi),
                    // The policy cutoff decides negligible mass here too — a second ad-hoc floor is the double-policy
                    // defect, and a length-scale anchor answers nothing about a marginal probability.
                    Confidence: denominator > cutoff ? Some(Math.Min(val1: 1.0, val2: pi / denominator)) : Option<double>.None));
                (total, weightedSquared) = (total + pi, weightedSquared + (pi * squared));
                (coveredSource, coveredTarget) = (coveredSource.TryAdd(i), coveredTarget.TryAdd(j));
            }
        }
        // Percentiles are PERCENT on the kernel entry; the median and the summary's maximum are columns the owner
        // already mints, so the tail reads off it rather than riding a fourth accumulator. `Interpolated` IS the R-7
        // convention the deleted page-local body transcribed, stated rather than inherited.
        return (items.IsEmpty
                ? Fin.Succ(Option<Distribution<Scalar>>.None)
                : Distribution<Scalar>.Of(values: items.Map(static item => (Scalar)item.Distance),
                    percentiles: Seq(90.0, 95.0), key: key, rule: Some(QuantileRule.Interpolated)).Map(Some))
            .Map(spread => new CloudCorrespondenceSet(
                Items: items, SourceCount: rows, TargetCount: columns, NonZeroCount: items.Count, TotalMass: total,
                // Mass weighting needs retained mass above the cutoff; a vanishing total falls back to the unweighted
                // RMS over the same survivors, and an EMPTY set measured no error at all rather than a perfect one.
                Rmse: spread.Map(census => total > cutoff ? Math.Sqrt(d: weightedSquared / total) : census.Summary.Rms),
                Distances: spread,
                CoveredSourceCount: coveredSource.Count, CoveredTargetCount: coveredTarget.Count,
                RetainedSourceMass: coveredSource.Fold(0.0, (held, i) => held + a[i]),
                RetainedTargetMass: coveredTarget.Fold(0.0, (held, j) => held + b[j])));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
