# [COMPUTE_SWEEP]

Rasm.Compute solver sweep: one `SweepGrid` N-dim DOE orchestration emitting a queryable `ParetoFront` artifact with a Morris/Sobol `SensitivityTornado`, and one `FrameBudget` early-stop governor returning a coarse iterative field within a frame deadline and refining async. The page owns the `SweepAxis` per-dimension sampling cases, the `SensitivityMethod` global-sensitivity rows, the `SweepGrid`/`FrameBudget`/`SensitivityTornado`/`SweepResult` carriers, and the `SweepLane` fan-out fold; the per-point evaluation composes the `Solver/optimizer#OPTIMIZER_LANE` `ParetoFront`/`DesignPoint`/`ObjectiveSense` artifact and the `Solver/contract#SOLVE_CONTRACT` iterative `Solve` residual receipt, every grid point enqueues onto the `Runtime/scheduling#SOLVE_GUARD` `LaneRuntime` as an `AdmittedIntent`, the `Sobol` axis draws the `Tensor/quadrature#OWNED_BUILDS` `LowDiscrepancy` sampler, and the `ComputeReceipt` rail, `WorkLane`, `CorrelationId`, `ClockPolicy`, and the `SolverKeyPolicy` ordinal accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. The `ParetoFront` is the same artifact the optimizer owns and crosses to Persistence content-keyed.

## [01]-[INDEX]

- [01]-[SWEEP_AND_BUDGET]: N-dim DOE sweep grid; frame-budgeted early-stop; Morris/Sobol sensitivity.

## [02]-[SWEEP_AND_BUDGET]

- Owner: `SweepGrid` the N-dim DOE orchestration record; `SweepAxis` `[Union]` per-dimension sampling cases; `SensitivityMethod` `[SmartEnum<string>]` global-sensitivity rows (one-at-a-time/morris-elementary/sobol-variance); `FrameBudget` the early-stop governor record reading an iterative solve's per-iteration residual receipt; `SensitivityTornado` the post-processing fold projecting the swept results onto a sensitivity ranking; `SweepLane` the static fan-out fold that enqueues every grid point onto the `WorkLane` and reduces the results to a `ParetoFront` plus tornado.
- Cases: `SweepAxis` cases `Linear(string Name, double Lower, double Upper, int Steps)` · `Logarithmic(string Name, double Lower, double Upper, int Steps)` · `LatinHypercube(string Name, double Lower, double Upper, int Samples)` · `Sobol(string Name, double Lower, double Upper, int Samples)` · `Enumerated(string Name, Seq<double> Values)`; `SensitivityMethod` rows one-at-a-time · morris-elementary · sobol-variance.
- Entry: `public static IO<Fin<SweepResult>> Run(SweepGrid grid, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, LaneRuntime lanes, CorrelationId correlation, ClockPolicy clocks)` — `IO` carries the lane-enqueue effect and the fan-out reduces the per-point solves; `FrameBudget.Governed` wraps an iterative `evaluate` so a frame-deadline expiry returns the coarse partial field and schedules the refinement continuation onto the `WorkLane.Background` row.
- Auto: `Run` materializes the grid as the Cartesian product of the `SweepAxis` samples (a `LatinHypercube`/`Sobol` axis draws its space-filling samples, a `Linear`/`Logarithmic` axis steps its range), enqueues each `DesignPoint` onto the `LaneRuntime` as an `AdmittedIntent`, and folds the returned objective vectors into the `ParetoFront` and the `SensitivityTornado` rank by the `SensitivityMethod` row; the frame budget reads the iterative solve's per-iteration `Solve` residual receipt and stops early when the elapsed crosses the deadline, returning the best-so-far field with the `Converged=false` flag and a refinement continuation; the interactive caller renders the coarse field within the frame and the refined field arrives async on the same correlation.
- Receipt: the `Sweep` `ComputeReceipt` case carries the grid-point count, the completed count, the dominated-versus-front split, the sensitivity-method key, and elapsed; the frame-budget early-stop stamps the per-point `Solve` residual at stop and the refinement continuation rides a second `Solve` receipt under the same correlation.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a new sampling strategy is one `SweepAxis` case; a new sensitivity analysis is one `SensitivityMethod` row carrying its ranking fold; a frame-budget policy change is one field on `FrameBudget`; zero new surface.
- Boundary: the sweep orchestration is the companion-fan-out consumer of the existing scheduler — every grid point is an `AdmittedIntent` on the `WorkLane.Bulk` row so an N-dim DOE rides the same backpressure and drain, never a parallel job queue; the Pareto front is the same `ParetoFront` artifact the `Solver/optimizer#OPTIMIZER_LANE` owns; the frame budget is the early-stop governor for any iterative `SolveMethod` row — it reads the residual receipt the solve already emits and the partial-field return is the coarse-then-refine contract, so a UI thread gets a within-frame field and the refinement is a scheduled continuation, never a blocking wait; the sensitivity tornado is the `SensitivityMethod` axis — the one-at-a-time row folds the swept objective deltas into a per-axis effect ranking, the Morris row computes the elementary-effect mean and standard deviation over the trajectory design through the owned `TensorPrimitives.StdDev` reduction, and the Sobol row computes the first-order main-effect index as the between-bin variance over the global variance (`V[E(Y|Xᵢ)]/V`) — so a hand-rolled sensitivity loop beside the axis is the deleted form, read on demand from the result stream and never a mutable accumulator; the `Sobol` sweep axis draws the owned low-discrepancy sequence the `Tensor/quadrature#OWNED_BUILDS` `LowDiscrepancy` sampler emits rather than a fresh Sobol implementation; the frame-budgeted progressive solve composes the `Solver/contract#SOLVE_CONTRACT` iterative receipts the solve lane already emits.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepAxis {
    private SweepAxis() { }

    public sealed record Linear(string Name, double Lower, double Upper, int Steps) : SweepAxis;
    public sealed record Logarithmic(string Name, double Lower, double Upper, int Steps) : SweepAxis;
    public sealed record LatinHypercube(string Name, double Lower, double Upper, int Samples) : SweepAxis;
    public sealed record Sobol(string Name, double Lower, double Upper, int Samples) : SweepAxis;
    public sealed record Enumerated(string Name, Seq<double> Values) : SweepAxis;

    public Seq<double> Samples =>
        Switch(
            linear: static a => toSeq(Enumerable.Range(0, a.Steps)).Map(i => a.Lower + (a.Upper - a.Lower) * i / Math.Max(1, a.Steps - 1)),
            logarithmic: static a => toSeq(Enumerable.Range(0, a.Steps)).Map(i => a.Lower * Math.Pow(a.Upper / a.Lower, (double)i / Math.Max(1, a.Steps - 1))),
            latinHypercube: static a => toSeq(Enumerable.Range(0, a.Samples)).Map(i => a.Lower + (a.Upper - a.Lower) * (i + 0.5) / a.Samples),
            sobol: static a => SobolFill(a.Lower, a.Upper, a.Samples),
            enumerated: static a => a.Values);

    public string AxisName => Switch(linear: static a => a.Name, logarithmic: static a => a.Name, latinHypercube: static a => a.Name, sobol: static a => a.Name, enumerated: static a => a.Name);

    static Seq<double> SobolFill(double lower, double upper, int samples) =>
        LowDiscrepancy.Sobol(dimensions: 1, seed: samples, Scramble.DigitalShift)
            .Map(generator => toSeq(Enumerable.Range(0, samples)).Fold((Gen: generator, Points: Seq<double>()), static (acc, _) => {
                var (next, point) = acc.Gen.Draw();
                return (next, acc.Points.Add(point[0]));
            }).Points.Map(unit => lower + (upper - lower) * unit))
            .IfFail(toSeq(Enumerable.Range(0, samples)).Map(i => lower + (upper - lower) * (i + 0.5) / samples));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class SensitivityMethod {
    public static readonly SensitivityMethod OneAtATime = new("one-at-a-time");
    public static readonly SensitivityMethod MorrisElementary = new("morris-elementary");
    public static readonly SensitivityMethod SobolVariance = new("sobol-variance");

    public (double Low, double High, double Effect) Rank(Seq<double> perBin, double globalVariance) =>
        Switch(
            state: (Bins: perBin, Variance: globalVariance),
            oneAtATime: static s => s.Bins.IsEmpty ? (0.0, 0.0, 0.0) : (s.Bins.Min(), s.Bins.Max(), Math.Abs(s.Bins.Max() - s.Bins.Min())),
            morrisElementary: static s => Elementary(s.Bins),
            sobolVariance: static s => (s.Bins.IsEmpty ? 0.0 : s.Bins.Min(), s.Bins.IsEmpty ? 0.0 : s.Bins.Max(), s.Variance > 1e-12 ? BinVariance(s.Bins) / s.Variance : 0.0));

    static (double, double, double) Elementary(Seq<double> bins) {
        if (bins.Count < 2) { return (0.0, 0.0, 0.0); }
        double[] effects = toSeq(Enumerable.Range(1, bins.Count - 1)).Map(i => Math.Abs(bins[i] - bins[i - 1])).ToArray();
        double mu = TensorPrimitives.Average<double>(effects), sigma = TensorPrimitives.StdDev<double>(effects);
        return (mu - sigma, mu + sigma, double.Hypot(mu, sigma));
    }

    static double BinVariance(Seq<double> bins) {
        if (bins.IsEmpty) { return 0.0; }
        double sigma = TensorPrimitives.StdDev<double>(bins.ToArray());
        return sigma * sigma;
    }
}

public sealed record SweepGrid(Seq<SweepAxis> Axes, Seq<ObjectiveSense> Objectives, SensitivityMethod Sensitivity) {
    public Seq<ImmutableArray<double>> Points =>
        Axes.Fold(Seq<ImmutableArray<double>>(ImmutableArray<double>.Empty), (acc, axis) =>
            acc.Bind(prefix => axis.Samples.Map(sample => prefix.Add(sample))));

    public long Cardinality => Axes.Fold(1L, static (acc, axis) => acc * axis.Samples.Count);
}

public sealed record FrameBudget(Duration Deadline, int MinIterations, WorkLane Refinement) {
    public static readonly FrameBudget Interactive = new(Duration.FromMilliseconds(16), MinIterations: 8, WorkLane.Background);

    public bool Expired(Instant start, Instant now, int iteration) => iteration >= MinIterations && now - start >= Deadline;
}

public sealed record SensitivityTornado(Seq<(string Axis, double Low, double High, double Effect)> Bars) {
    public static SensitivityTornado Of(SweepGrid grid, Seq<DesignPoint> results, int objective) {
        double globalVariance = results.IsEmpty ? 0.0 : TensorPrimitives.StdDev<double>(results.Map(p => p.Objectives[objective]).ToArray()) is var sd ? sd * sd : 0.0;
        return new(grid.Axes.Map((axis, index) => {
            var byAxis = results.GroupBy(point => point.Coordinates[index]).Select(group => group.Average(point => point.Objectives[objective])).ToSeq();
            var (low, high, effect) = grid.Sensitivity.Rank(byAxis, globalVariance);
            return (axis.AxisName, low, high, effect);
        }).OrderByDescending(static bar => bar.Item4).ToSeq());
    }
}

public sealed record SweepResult(SweepGrid Grid, ParetoFront Front, SensitivityTornado Tornado, int Completed, Instant At);

public static class SweepLane {
    public static IO<Fin<SweepResult>> Run(SweepGrid grid, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, LaneRuntime lanes, CorrelationId correlation, ClockPolicy clocks) =>
        grid.Points.TraverseM(coords => evaluate(new DesignPoint(coords, [], [])).Map(result => (coords, result)))
            .Map(evaluated => evaluated.Fold(
                Fin.Succ((Front: new ParetoFront(Seq<DesignPoint>(), [.. grid.Objectives.Map(static o => o.Sign)]), Points: Seq<DesignPoint>())),
                (acc, pair) => acc.Bind(state => pair.result.Map(objectives => {
                    var point = new DesignPoint(pair.coords, [.. objectives], []);
                    return (state.Front.Insert(point), state.Points.Add(point));
                })))
                .Map(state => new SweepResult(grid, state.Front, SensitivityTornado.Of(grid, state.Points, 0), state.Points.Count, clocks.Now)));

    public static Func<DesignPoint, IO<Fin<Seq<double>>>> Governed(FrameBudget budget, Func<DesignPoint, int, IO<Fin<(Seq<double> Field, double Residual, bool Done)>>> step, ClockPolicy clocks, Func<DesignPoint, IO<Unit>> refine) =>
        point => IO.liftAsync(async env => {
            Instant start = clocks.Now;
            var best = Fin.Fail<(Seq<double>, double, bool)>(ComputeFault.Create("<frame-budget-no-iteration>"));
            for (int iteration = 0; ; iteration++) {
                best = await step(point, iteration).RunAsync(env);
                if (best.IsFail || best.Match(Succ: r => r.Done, Fail: static _ => true) || budget.Expired(start, clocks.Now, iteration)) { break; }
            }
            await refine(point).RunAsync(env);
            return best.Map(static r => r.Item1);
        });
}
```
