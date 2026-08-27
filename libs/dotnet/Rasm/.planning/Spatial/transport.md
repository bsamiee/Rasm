# [RASM_TRANSPORT]

`CloudTransport` owns optimal transport between weighted vector clusters through ONE log-domain stabilized Sinkhorn kernel: balanced marginals, KL-relaxed unbalanced marginals, and Sinkhorn-divergence debiasing are POLICY columns of one iteration, never three solver bodies, and every answer leaves through ONE `Project<TOut>` egress on the solved plan.

Transport mass is the cluster's own admitted normalized mass (`cloud.md` `MassOf`), so a weighted cluster IS a discrete measure and no second measure type exists; `register.md` consumes `CloudCorrespondenceSet` as its soft-assignment input without re-walking the coupling.

Debiasing is the policy's own `Debias` column and the policy is a generated `[ComplexValueObject]` admitted once through `Of`; the iteration budget is a bounded `FoldUntil` over the generated range, and both numeric floors read `Domain/context` lanes — `Convergence` for the residual target and `Neglect` for the sparsification cutoff — so no page literal and no caller override states either.

## [01]-[INDEX]

- [02]-[TRANSPORT_POLICY]: `CloudTransportPolicy` spans the whole solver product in one generated value object.
- [03]-[SINKHORN]: `CloudTransport` solves in log space and its private plan capsule projects every answer.
- [04]-[CORRESPONDENCES]: `CloudCorrespondenceSet` thresholds the coupling into pairings carrying coverage evidence.

## [02]-[TRANSPORT_POLICY]

- Owner: `CloudTransportPolicy` columns span the balanced/unbalanced/debiased product in one generated value object; `CouplingCutoff` is the single sparsification floor below which a coupling entry carries no correspondence.
- Cases: `Debias` names WHICH divergence the solve reports — `false` the raw regularized cost, `true` the Sinkhorn divergence that subtracts both self-transport halves — so the two-solve leg is one guarded bind on the admitted policy column, and an unconverged self-plan faults the debiased answer instead of shifting it. The residual the fold measures derives from `MassRelaxation` alone — marginal mass when balanced, scaling change when relaxed, because the marginal test is meaningless under relaxation — and `Converged` is the fold's own terminal fact beside it, so no roster mirrors either and the stop cannot disagree with the residual it reports; budget exhaustion therefore reads as a partial plan the caller retries under a wider budget, never a failure. NAMED LOSS: the summary no longer re-derives convergence from its own residual pair, because `Advance` is the single comparison site and a second derivation was the second authority the mirror existed to police.
- Law: the two floors are `Domain/context` lanes, not page constants — `ToleranceLane.Convergence` sets the residual target and `ToleranceLane.Neglect` the coupling cutoff, so a model that tightens either tightens this solve with no second knob.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CloudTransportPolicy {
    public PositiveMagnitude Regularization { get; }
    public Dimension MaxIterations { get; }
    public bool Debias { get; }
    public Option<PositiveMagnitude> MassRelaxation { get; }
    public PositiveMagnitude ConvergenceTolerance { get; }
    public PositiveMagnitude CouplingCutoff { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError,
        ref PositiveMagnitude regularization, ref Dimension maxIterations, ref bool debias,
        ref Option<PositiveMagnitude> massRelaxation, ref PositiveMagnitude convergenceTolerance,
        ref PositiveMagnitude couplingCutoff) =>
        validationError = regularization == default || maxIterations == default
            || massRelaxation.Case is PositiveMagnitude relaxation && relaxation == default
            || convergenceTolerance == default || couplingCutoff == default
                ? new ValidationError(message: "CloudTransportPolicy requires admitted positive values.") : null;

    public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, Context context,
        bool debias = false, Option<double> massRelaxation = default) {
        return from model in Admit.NotNull(value: context)
               from reg in FactoryBridge.Accept<PositiveMagnitude>(regularization)
               from cap in FactoryBridge.Accept<Dimension>(maxIterations)
               from relax in massRelaxation.TraverseM(value =>
                   FactoryBridge.Accept<PositiveMagnitude>(value)).As()
               from tolerance in FactoryBridge.Accept<PositiveMagnitude>(model.For(ToleranceLane.Convergence).Value)
               from cutoff in FactoryBridge.Accept<PositiveMagnitude>(model.For(ToleranceLane.Neglect).Value)
               from policy in FactoryBridge.Accept<CloudTransportPolicy>(
                   Validate(reg, cap, debias, relax, tolerance, cutoff,
                       out CloudTransportPolicy? admitted), admitted)
               select policy;
    }
}
```

## [03]-[SINKHORN]

- Owner: `CloudTransport` owns the solve; its private nested `Plan` capsule holds the coupling, the admitted marginals, and the policy once, opaque behind its ONE `Project<TOut>` egress.
- Entry: `Sinkhorn<TOut>` solves once and the requested `TOut` selects the projection row, so every projection caller shares one entry and one solve. `Debias` on the admitted policy selects the two-solve leg as one guarded bind, and a self-plan that exhausts its budget faults the debiased answer.
- Auto: log-domain scalings iterate under a max-shifted `LogSumExp`, so a fully-improbable row degrades to `−∞` gracefully; a non-finite distance or residual faults the solve. `Range.FoldUntil` owns the exact iteration budget and stops on the fold state's own `Converged` fact.
- Law: the budget is bounded and its exhaustion is EVIDENCE, not silence — a run that stops on the schedule leaves the fold state unconverged and the plan publishes `Converged` false. Because that evidence rides the summary alone, `Project<TOut>` REFUSES the evidence-free rows — `double`, `Matrix`, `CloudCorrespondenceSet`, `VectorCloud` — on an unconverged plan, since handing a bare cost out of an exhausted run is exactly the success-shaped fall-through that certifies unconverged as converged. `SinkhornSummary` admits it, because that shape carries `Converged`.
- Packages: RhinoCommon `Point3d.DistanceToSquared` is the cost kernel; System.Numerics.Tensors `TensorPrimitives` folds the LSE rows, the coupling emission, and the entropic-cost reduction; CommunityToolkit.HighPerformance `Memory2D<T>`/`Span2D<T>.GetRowSpan` addresses both `(m, n)` planes and `GetColumn(...).CopyTo` gathers a column, so no stride arithmetic and no stride argument survive; `Rasm.Domain` `Stat<Scalar>`/`Distribution<Scalar>` own every moment and order statistic the correspondence set publishes; LanguageExt.Core carries the types, value objects, and bounded fold; Thinktecture.Runtime.Extensions carries the generated policy owner.
- Growth: a new transport mode is one policy column and one bind over the same kernel and summary vocabulary, never a second solver body; convergence and underflow are the fold's own bool facts, never a mirrored roster.
- Boundary: the two `(m, n)` planes are the named statement kernel with a `Fin` result at both edges, confined to the solve body — ONE rental each, addressed through the `Memory2D<T>` view, so the flat buffer serves only the whole-plane `TensorPrimitives` reductions and no site re-derives an offset; the coupling leaves only as a `matrix.md` `Matrix` through its projection row, whose shape admits through `AcceptValidated<Dimension>` off the plan's marginal counts so a refused shape rides `Fin`, never a throwing factory; the solve's `minPositiveNormal` local is THE underflow anchor and `logUnderflowFloor` derives from it, so an ad-hoc `Math.Exp` on an unfloored exponent re-introduces the silent-zero defect; `typeof(TOut)` resolution routes `ProjectionRow` entries through `atoms.md` `ResultProjection.Rows`, never a reflection ladder.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SinkhornSummary(
    double Distance, Option<(double Raw, double Source, double Target)> Bias,
    CloudTransportPolicy Policy, bool Converged, bool UnderflowFloored,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations,
    CloudCorrespondenceSet Correspondences) : IValidityEvidence {
    public bool IsValid => Policy is { } policy && ValidityClaim.All(
        ValidityClaim.Finite(Distance),
        Bias.Map(static held => ValidityClaim.All(
            ValidityClaim.Nonnegative(held.Raw), ValidityClaim.Nonnegative(held.Source),
            ValidityClaim.Nonnegative(held.Target))).IfNone(!policy.Debias),
        policy.Debias == Bias.IsSome,
        ValidityClaim.Nonnegative(SourceConvergenceResidual),
        ValidityClaim.Nonnegative(TargetConvergenceResidual),
        Iterations >= 1 && Iterations <= policy.MaxIterations.Value,
        ValidityClaim.Evidence(Some(Correspondences)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CloudTransport {
    public static Fin<TOut> Sinkhorn<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
        CloudTransportPolicy policy) {
        return from src in Admit.NotNull(value: source)
               from tgt in Admit.NotNull(value: target)
               from active in Admit.NotNull(value: policy)
               from srcMass in CloudKernel.MassOf(cluster: src)
               from tgtMass in CloudKernel.MassOf(cluster: tgt)
               from plan in Solve(src.Vertices, tgt.Vertices, srcMass, tgtMass, active)
               from bias in active.Debias
                   ? from selfS in Solve(src.Vertices, src.Vertices, srcMass, srcMass, active)
                     from selfT in Solve(tgt.Vertices, tgt.Vertices, tgtMass, tgtMass, active)
                     from _ in guard(selfS.Converged && selfT.Converged,
                         new KernelFault.InvalidResult(Detail: Some("sinkhorn-debias-unconverged")))
                     select (Evidence: Some((Raw: plan.Distance, Source: selfS.Distance, Target: selfT.Distance)),
                         Distance: plan.Distance - (0.5 * selfS.Distance) - (0.5 * selfT.Distance))
                   : Fin.Succ((Evidence: Option<(double Raw, double Source, double Target)>.None,
                       Distance: plan.Distance))
               from output in plan.Project<TOut>(src, tgt, bias.Distance, bias.Evidence)
               select output;
    }

    private static Fin<Plan> Solve(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass,
        Arr<double> targetMass, CloudTransportPolicy policy) {
        (int m, int n, double eps) = (source.Count, target.Count, policy.Regularization.Value);
        double minPositiveNormal = BitConverter.UInt64BitsToDouble(0x0010_0000_0000_0000UL);
        double logUnderflowFloor = Math.Log(minPositiveNormal);
        double[] costs = new double[m * n];
        Memory2D<double> logK = costs.AsMemory().AsMemory2D(height: m, width: n);
        for (int i = 0; i < m; i++) {
            Span<double> row = logK.Span.GetRowSpan(i);
            for (int j = 0; j < n; j++) row[j] = -source[i].DistanceToSquared(other: target[j]) / eps;
        }
        double[] logU = new double[m]; double[] logV = new double[n];
        double[] gather = new double[Math.Max(val1: m, val2: n)];
        double[] fold = new double[Math.Max(val1: m, val2: n)];
        double[] logA = [.. sourceMass.AsIterable().Select(Math.Log)]; double[] logB = [.. targetMass.AsIterable().Select(Math.Log)];
        double exponent = policy.MassRelaxation.Match(Some: l => l.Value / (l.Value + eps), None: () => 1.0);
        double LogSumExp(ReadOnlySpan<double> row, ReadOnlySpan<double> shift, Span<double> scratch) {
            TensorPrimitives.Add(row, shift, scratch);
            double max = TensorPrimitives.Max<double>(scratch);
            if (double.IsNegativeInfinity(max)) return double.NegativeInfinity;
            TensorPrimitives.Subtract(scratch, max, scratch);
            TensorPrimitives.Exp<double>(scratch, scratch);
            return max + Math.Log(TensorPrimitives.Sum<double>(scratch));
        }
        double LogSumExpColumn(Span2D<double> kernel, int column,
            ReadOnlySpan<double> shift, Span<double> gather, Span<double> scratch) {
            kernel.GetColumn(column).CopyTo(gather);
            return LogSumExp(gather, shift, scratch);
        }
        (int Iterations, double Source, double Target, bool Converged) Advance(int iteration) {
            (double deltaU, double deltaV) = (0.0, 0.0);
            for (int i = 0; i < m; i++) {
                double next = exponent * (logA[i] - LogSumExp(logK.Span.GetRowSpan(i), logV, fold.AsSpan(0, n)));
                deltaU = Math.Max(deltaU, Math.Abs(next - logU[i]));
                logU[i] = next;
            }
            for (int j = 0; j < n; j++) {
                double next = exponent * (logB[j] - LogSumExpColumn(logK.Span, j, logU,
                    gather.AsSpan(0, m), fold.AsSpan(0, m)));
                deltaV = Math.Max(deltaV, Math.Abs(next - logV[j]));
                logV[j] = next;
            }
            (double s, double t) = (deltaU, deltaV);
            if (policy.MassRelaxation.IsNone) {
                (s, t) = (0.0, 0.0);
                for (int i = 0; i < m; i++) {
                    double log = logU[i] + LogSumExp(logK.Span.GetRowSpan(i), logV, fold.AsSpan(0, n));
                    s = Math.Max(s, Math.Abs((log < logUnderflowFloor ? 0.0 : Math.Exp(log)) - sourceMass[i]));
                }
                for (int j = 0; j < n; j++) {
                    double log = logV[j] + LogSumExpColumn(logK.Span, j, logU,
                        gather.AsSpan(0, m), fold.AsSpan(0, m));
                    t = Math.Max(t, Math.Abs((log < logUnderflowFloor ? 0.0 : Math.Exp(log)) - targetMass[j]));
                }
            }
            return (Iterations: iteration + 1, Source: s, Target: t,
                Converged: Math.Max(s, t) <= policy.ConvergenceTolerance.Value);
        }
        (int Iterations, double Source, double Target, bool Converged) settled =
            Range(0, policy.MaxIterations.Value).FoldUntil(
            initialState: (Iterations: 0, Source: double.PositiveInfinity, Target: double.PositiveInfinity, Converged: false),
            f: (_, iteration) => Advance(iteration),
            predicate: static pair => pair.State.Converged);
        double[] entries = new double[m * n];
        Memory2D<double> coupling = entries.AsMemory().AsMemory2D(height: m, width: n);
        bool floored = false;
        for (int i = 0; i < m; i++) {
            Span<double> row = coupling.Span.GetRowSpan(i);
            TensorPrimitives.Add(x: logK.Span.GetRowSpan(i), y: logV, destination: row);
            TensorPrimitives.Add(x: row, y: logU[i], destination: row);
            bool rowFloored = TensorPrimitives.Min<double>(row) < logUnderflowFloor;
            TensorPrimitives.Exp<double>(row, row);
            if (rowFloored) {
                floored = true;
                for (int j = 0; j < n; j++) if (row[j] < minPositiveNormal) row[j] = 0.0;
            }
        }
        double distance = -eps * TensorPrimitives.Dot<double>(entries, costs);
        return double.IsFinite(distance) && double.IsFinite(settled.Source) && double.IsFinite(settled.Target)
            ? Fin.Succ(new Plan(distance, entries, sourceMass, targetMass,
                settled.Source, settled.Target, settled.Iterations, settled.Converged, floored, policy))
            : Fin.Fail<Plan>(new KernelFault.InvalidResult());
    }

    private sealed class Plan(
        double distance, double[] coupling, Arr<double> sourceMass, Arr<double> targetMass,
        double sourceResidual, double targetResidual, int iterations,
        bool converged, bool underflowFloored, CloudTransportPolicy policy) {
        internal double Distance => distance;
        internal bool Converged => converged;

        internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
            double distance, Option<(double Raw, double Source, double Target)> bias) {
            Fin<T> Settled<T>(Func<Fin<T>> row) =>
                converged ? row() : Fin.Fail<T>(error: new KernelFault.InvalidResult(Detail: Some($"sinkhorn-unconverged:{iterations}")));
            Memory2D<double> plane = coupling.AsMemory().AsMemory2D(
                height: sourceMass.Count, width: targetMass.Count);
            return ResultProjection.Rows<Plan, TOut>(self: this, owner: typeof(VectorCloud),
                ProjectionRow.Of<double>(() => Settled(() => Acceptance.Value(value: distance))),
                ProjectionRow.Of<SinkhornSummary>(() =>
                    from pairs in CloudCorrespondenceSet.OfCoupling(source, target, plane,
                        policy.CouplingCutoff.Value, sourceMass, targetMass)
                    from summary in Acceptance.Value(new SinkhornSummary(distance, bias, policy,
                        converged, underflowFloored, sourceResidual, targetResidual, iterations, pairs))
                    select summary),
                ProjectionRow.Of<CloudCorrespondenceSet>(() => Settled(() => CloudCorrespondenceSet.OfCoupling(
                    source, target, plane, policy.CouplingCutoff.Value, sourceMass, targetMass))),
                ProjectionRow.Of<Matrix>(() => Settled(() =>
                    from rows in FactoryBridge.Accept<Dimension>(sourceMass.Count)
                    from cols in FactoryBridge.Accept<Dimension>(targetMass.Count)
                    from matrix in Matrix.Of(rows: rows, cols: cols, entries: new Arr<double>([.. coupling]))
                    select matrix)),
                ProjectionRow.Of<VectorCloud>(() => Settled(() => {
                    List<Point3d> image = [];
                    for (int i = 0; i < sourceMass.Count; i++) {
                        ReadOnlySpan<double> row = plane.Span.GetRowSpan(i);
                        (double mass, Vector3d weighted) = (0.0, Vector3d.Zero);
                        for (int j = 0; j < row.Length; j++) {
                            (mass, weighted) = (mass + row[j], weighted + (row[j] * (Vector3d)target.Vertices[j]));
                        }
                        if (mass > policy.CouplingCutoff.Value) image.Add(new Point3d(weighted / mass));
                    }
                    return VectorCloud.Cluster(points: toSeq(image), context: target.Tolerance);
                })));
        }
    }
}
```

## [04]-[CORRESPONDENCES]

- Owner: `CloudCorrespondence` is one thresholded coupling entry and `CloudCorrespondenceSet` the ordered collection; `Confidence` is `min(1, π[i,j]/max(aᵢ, bⱼ))`, how decisively a pairing claims its endpoints.
- Entry: `CloudCorrespondenceSet.OfCoupling` walks the coupling above the cutoff over the plan's admitted marginals and folds every statistic in one pass.
- Auto: `Measurements` is ONE optional product — `Coupling` the coupling-entry moments, `WeightedDistance` the mass-weighted distance moments whose `Rms` is `√(Σ π·d² / Σ π)`, and `Distance` the unweighted order statistics — absent exactly when no pairing survived the cutoff, because an empty pairing set measured no error and a zero there reads as a perfect registration; retained source and target mass derives off the covered `Set<int>` partition after the walk, the sparsification-loss signal an unbalanced solve reads to see how much measure the relaxation dropped.
- Growth: a new pairing statistic is one member of the `Measurements` product over the single-pass sample lists; a new order statistic is one percentile on the `Distribution<Scalar>` call and no new column at all.
- Boundary: the cutoff is the policy's and EVERY floor on this page reads it — the census, the confidence denominator, and the barycentric row mass — so a second ad-hoc epsilon is the double-policy defect and a length-scale anchor never gates a probability mass; order statistics come off `Domain/stats` `Distribution<Scalar>`, the branch's ONE exact-quantile owner, so a transport quantile and a statistics quantile cannot disagree on one sample set; index pairs refer to ADMITTED cluster vertices, so `cloud.md`'s `OriginalToUnique` re-indexing has already happened upstream and correspondence indices never see pre-deduplication positions.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondence(
    int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint,
    double SourceMass, double TargetMass, double CouplingMass, Option<double> Confidence);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudCorrespondenceSet(
    Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount,
    Option<(Stat<Scalar> Coupling, Stat<Scalar> WeightedDistance,
        Distribution<Scalar> Distance)> Measurements,
    int CoveredSourceCount, int CoveredTargetCount, double RetainedSourceMass, double RetainedTargetMass) : IValidityEvidence {
    public bool IsValid {
        get {
            CloudCorrespondenceSet self = this;
            return ValidityClaim.All(
                Measurements.IsSome == !Items.IsEmpty,
                Measurements.Map(e => e.Coupling.Count == self.Items.Count
                    && e.WeightedDistance.Count == self.Items.Count && e.Distance.Summary.Count == self.Items.Count
                    && e.Coupling.IsValid && e.WeightedDistance.IsValid && e.Distance.IsValid).IfNone(true),
                CoveredSourceCount >= 0 && CoveredSourceCount <= SourceCount,
                CoveredTargetCount >= 0 && CoveredTargetCount <= TargetCount,
                ValidityClaim.Nonnegative(RetainedSourceMass), ValidityClaim.Nonnegative(RetainedTargetMass));
        }
    }

    internal static Fin<CloudCorrespondenceSet> OfCoupling(VectorCloud.ClusterCase source,
        VectorCloud.ClusterCase target, Memory2D<double> coupling, double cutoff,
        Arr<double> sourceMass, Arr<double> targetMass) {
        (int rows, int columns) = (coupling.Height, coupling.Width);
        List<CloudCorrespondence> items = [];
        List<Scalar> distances = [];
        List<double> weights = [];
        (Set<int> coveredSource, Set<int> coveredTarget) = (Set<int>(), Set<int>());
        for (int i = 0; i < rows; i++) {
            ReadOnlySpan<double> row = coupling.Span.GetRowSpan(i);
            for (int j = 0; j < columns; j++) {
                double pi = row[j];
                if (pi <= cutoff) continue;
                (Point3d sp, Point3d tp) = (source.Vertices[i], target.Vertices[j]);
                double squared = sp.DistanceToSquared(other: tp), distance = Math.Sqrt(d: squared);
                double denominator = Math.Max(val1: sourceMass[i], val2: targetMass[j]);
                items.Add(new CloudCorrespondence(SourceIndex: i, TargetIndex: j,
                    SourcePoint: sp, TargetPoint: tp,
                    SourceMass: sourceMass[i], TargetMass: targetMass[j], CouplingMass: pi,
                    Confidence: denominator > cutoff
                        ? Some(Math.Min(1.0, pi / denominator)) : Option<double>.None));
                distances.Add((Scalar)distance);
                weights.Add(pi);
                (coveredSource, coveredTarget) = (coveredSource.TryAdd(i), coveredTarget.TryAdd(j));
            }
        }
        Seq<double> mass = toSeq(weights); Seq<Scalar> samples = toSeq(distances);
        Fin<Option<(Stat<Scalar> Coupling, Stat<Scalar> WeightedDistance,
            Distribution<Scalar> Distance)>> measured = samples.IsEmpty
            ? Fin.Succ(Option<(Stat<Scalar>, Stat<Scalar>, Distribution<Scalar>)>.None)
            : from coupling in Stat<Scalar>.Of(mass.Map(static value => (Scalar)value))
              from weighted in Stat<Scalar>.Of(samples, Some(mass))
              from spread in Distribution<Scalar>.Of(samples, Seq(90.0, 95.0),
                  Some(QuantileRule.Interpolated))
              select Some((Coupling: coupling, WeightedDistance: weighted, Distance: spread));
        return from measurements in measured
               from set in Acceptance.Value(new CloudCorrespondenceSet(
                   toSeq(items), rows, columns, measurements,
                   coveredSource.Count, coveredTarget.Count,
                   coveredSource.Fold(0.0, (held, i) => held + sourceMass[i]),
                   coveredTarget.Fold(0.0, (held, j) => held + targetMass[j])))
               select set;
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
