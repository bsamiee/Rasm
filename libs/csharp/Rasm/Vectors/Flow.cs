using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherTableau(
    Seq<Seq<double>> Coupling,
    Seq<double> Weights,
    Option<Seq<double>> ErrorWeights,
    int MethodOrder,
    Option<int> EmbeddedOrder) {
    internal int StageCount => Weights.Count;
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && (EmbeddedOrder is not { IsSome: true, Case: int embedded } || (embedded > 0 && embedded < MethodOrder))
        && Coupling.Count == StageCount
        && CoefficientsAreFinite(values: Weights)
        && Math.Abs(value: SumCoefficients(values: Weights) - 1.0) <= 1.0e-9
        && Coupling.AsIterable().Select((row, index) => row.Count <= index && CoefficientsAreFinite(values: row)).All(static ok => ok)
        && (ErrorWeights is not { IsSome: true, Case: Seq<double> ew }
            || (ew.Count == StageCount
                && CoefficientsAreFinite(values: ew)
                && Math.Abs(value: SumCoefficients(values: ew) - 1.0) <= 1.0e-9));
    internal Fin<ButcherTableau> Admit(Op key) =>
        IsValid ? Fin.Succ(this) : Fin.Fail<ButcherTableau>(key.InvalidInput());
    private static bool CoefficientsAreFinite(Seq<double> values) =>
        values.ForAll(RhinoMath.IsValidDouble);
    private static double SumCoefficients(Seq<double> values) =>
        values.Fold(initialState: 0.0, f: static (sum, value) => sum + value);
}

[SmartEnum<int>]
public sealed partial class IntegratorKind {
    public static readonly IntegratorKind Euler = Fixed(key: 0, order: 1, coupling: [[]], weights: [1.0]);
    public static readonly IntegratorKind Heun = Fixed(key: 1, order: 2, coupling: [[], [1.0]], weights: [0.5, 0.5]);
    public static readonly IntegratorKind Midpoint = Fixed(key: 2, order: 2, coupling: [[], [0.5]], weights: [0.0, 1.0]);
    public static readonly IntegratorKind Ralston = Fixed(key: 3, order: 2, coupling: [[], [2.0 / 3.0]], weights: [0.25, 0.75]);
    public static readonly IntegratorKind RK4 = Fixed(key: 4, order: 4,
        coupling: [[], [0.5], [0.0, 0.5], [0.0, 0.0, 1.0]],
        weights: [1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 6.0]);
    public static readonly IntegratorKind RK38 = Fixed(key: 5, order: 4,
        coupling: [[], [1.0 / 3.0], [-1.0 / 3.0, 1.0], [1.0, -1.0, 1.0]],
        weights: [1.0 / 8.0, 3.0 / 8.0, 3.0 / 8.0, 1.0 / 8.0]);
    public static readonly IntegratorKind BogackiShampine = Adaptive(key: 6, order: 3, embeddedOrder: 2,
        coupling: [[], [0.5], [0.0, 0.75], [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0]],
        weights: [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0, 0.0],
        errorWeights: [7.0 / 24.0, 0.25, 1.0 / 3.0, 1.0 / 8.0]);
    public static readonly IntegratorKind CashKarp = Adaptive(key: 7, order: 5, embeddedOrder: 4,
        coupling: [[], [0.2], [3.0 / 40.0, 9.0 / 40.0], [0.3, -0.9, 1.2],
            [-11.0 / 54.0, 2.5, -70.0 / 27.0, 35.0 / 27.0],
            [1631.0 / 55296.0, 175.0 / 512.0, 575.0 / 13824.0, 44275.0 / 110592.0, 253.0 / 4096.0]],
        weights: [37.0 / 378.0, 0.0, 250.0 / 621.0, 125.0 / 594.0, 0.0, 512.0 / 1771.0],
        errorWeights: [2825.0 / 27648.0, 0.0, 18575.0 / 48384.0, 13525.0 / 55296.0, 277.0 / 14336.0, 0.25]);
    public static readonly IntegratorKind DormandPrince = Adaptive(key: 8, order: 5, embeddedOrder: 4,
        coupling: [[], [1.0 / 5.0], [3.0 / 40.0, 9.0 / 40.0],
            [44.0 / 45.0, -56.0 / 15.0, 32.0 / 9.0],
            [19372.0 / 6561.0, -25360.0 / 2187.0, 64448.0 / 6561.0, -212.0 / 729.0],
            [9017.0 / 3168.0, -355.0 / 33.0, 46732.0 / 5247.0, 49.0 / 176.0, -5103.0 / 18656.0],
            [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0]],
        weights: [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0, 0.0],
        errorWeights: [5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0]);
    public ButcherTableau Tableau { get; }
    internal bool IsAdaptive => Tableau.ErrorWeights.IsSome;
    internal int StageCount => Tableau.Weights.Count;
    internal int Order => Tableau.MethodOrder;
    internal Option<int> EmbeddedOrder => Tableau.EmbeddedOrder;
    internal double AdaptiveExponent => Tableau.EmbeddedOrder
        .Map(static order => 1.0 / (order + 1.0))
        .IfNone(0.2);
    internal const double AdaptiveSafetyFactor = 0.9;
    internal const double AdaptiveMinScale = 0.2;
    internal const double AdaptiveMaxScale = 10.0;
    private static IntegratorKind Fixed(int key, int order, double[][] coupling, double[] weights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Weights: toSeq(weights), ErrorWeights: Option<Seq<double>>.None, MethodOrder: order, EmbeddedOrder: Option<int>.None));
    private static IntegratorKind Adaptive(int key, int order, int embeddedOrder, double[][] coupling, double[] weights, double[] errorWeights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Weights: toSeq(weights), ErrorWeights: Some(toSeq(errorWeights)), MethodOrder: order, EmbeddedOrder: Some(embeddedOrder)));
}

[SmartEnum<int>]
public sealed partial class StreamlineStopKind {
    public static readonly StreamlineStopKind Terminated = new(key: 0);
    public static readonly StreamlineStopKind RejectBudgetExhausted = new(key: 1);
    public static readonly StreamlineStopKind IterationCapExhausted = new(key: 2);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamlineTrace(
    Seq<Point3d> Trail,
    StreamlineStopKind Stop,
    int AcceptedSteps,
    int RejectedSteps,
    double ArcLength,
    double FinalStep) {
    public bool IsComplete => Stop.Equals(StreamlineStopKind.Terminated);
}

[Union]
internal abstract partial record StreamlineStep {
    private StreamlineStep() { }
    public sealed record AcceptedCase(Point3d Next, double SuggestedStep) : StreamlineStep;
    public sealed record RejectedCase(double SuggestedStep) : StreamlineStep;
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record FieldIntegrator {
    private FieldIntegrator() { }
    public sealed record FixedCase(IntegratorKind Kind) : FieldIntegrator;
    public sealed record AdaptiveCase(IntegratorKind Kind, PositiveMagnitude Tolerance, int MaxRejects) : FieldIntegrator;
    public static FieldIntegrator Euler => new FixedCase(Kind: IntegratorKind.Euler);
    public static FieldIntegrator Heun => new FixedCase(Kind: IntegratorKind.Heun);
    public static FieldIntegrator RK4 => new FixedCase(Kind: IntegratorKind.RK4);
    public static Fin<FieldIntegrator> Adaptive(IntegratorKind kind, double tolerance, int maxRejects = 3, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from _ in guard(active.IsAdaptive && maxRejects >= 0, op.Unsupported(geometryType: active.GetType(), outputType: typeof(AdaptiveCase)))
               from validated in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               select (FieldIntegrator)new AdaptiveCase(Kind: active, Tolerance: validated, MaxRejects: maxRejects);
    }
    public static Fin<FieldIntegrator> RK45Adaptive(double tolerance, int maxRejects = 3, Op? key = null) =>
        Adaptive(kind: IntegratorKind.DormandPrince, tolerance: tolerance, maxRejects: maxRejects, key: key);
    internal int RejectBudget => Switch(
        state: 0,
        fixedCase: static (s, _) => s,
        adaptiveCase: static (_, c) => c.MaxRejects);
    internal Fin<StreamlineStep> Step(VectorField field, Point3d point, double h, Context context, Op key) => Switch(
        state: (Field: field, Point: point, H: h, Context: context, Key: key),
        fixedCase: static (s, c) =>
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            from next in s.Key.AcceptValue(value: s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks)))
            select (StreamlineStep)new StreamlineStep.AcceptedCase(Next: next, SuggestedStep: s.H),
        adaptiveCase: static (s, c) =>
            from errWeights in c.Kind.Tableau.ErrorWeights.ToFin(Fail: s.Key.InvalidInput())
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            let primary = s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks))
            let secondary = s.Point + (s.H * Combine(coefficients: errWeights, vectors: ks))
            let err = (primary - secondary).Length
            let scale = err > RhinoMath.ZeroTolerance
                ? Math.Clamp(
                    value: IntegratorKind.AdaptiveSafetyFactor * Math.Pow(x: c.Tolerance.Value / err, y: c.Kind.AdaptiveExponent),
                    min: IntegratorKind.AdaptiveMinScale,
                    max: IntegratorKind.AdaptiveMaxScale)
                : IntegratorKind.AdaptiveMaxScale
            from result in err <= c.Tolerance.Value
                ? s.Key.AcceptValue(value: (StreamlineStep)new StreamlineStep.AcceptedCase(Next: primary, SuggestedStep: s.H * scale))
                : s.Key.AcceptValue(value: (StreamlineStep)new StreamlineStep.RejectedCase(SuggestedStep: s.H * scale))
            select result);
    private static Fin<Seq<Vector3d>> ComputeStages(ButcherTableau tableau, VectorField field, Point3d point, double h, Context context, Op key) =>
        tableau.Coupling.Fold(
            initialState: Fin.Succ((Seq<Vector3d>)[]),
            f: (acc, row) => acc.Bind(ks =>
                field.SampleVector(sample: point + (h * Combine(coefficients: row, vectors: ks)), context: context, key: key)
                    .Map(k => ks.Add(k))));
    private static Vector3d Combine(Seq<double> coefficients, Seq<Vector3d> vectors) =>
        coefficients.Zip(vectors).Fold(
            initialState: Vector3d.Zero,
            f: static (sum, pair) => sum + (pair.Item1 * pair.Item2));
}

internal readonly record struct StreamlineState(
    Seq<Point3d> Trail,
    Point3d Current,
    double H,
    double Arc,
    int Steps,
    int Rejects,
    int RejectedSteps,
    Option<StreamlineStopKind> Stop);

[Union]
public abstract partial record Termination {
    private Termination() { }
    public sealed record StepCountCase(int Count) : Termination;
    public sealed record ArcLengthCase(PositiveMagnitude Length) : Termination;
    public sealed record MagnitudeFloorCase(PositiveMagnitude Threshold) : Termination;
    public sealed record CrossSurfaceCase(SupportSpace Surface) : Termination;
    public sealed record EnterRegionCase(ScalarField Region, double Threshold) : Termination;
    public sealed record LoopDetectedCase(PositiveMagnitude ClosureRadius) : Termination;
    public static Fin<Termination> Steps(int count, Op? key = null) {
        Op op = key.OrDefault();
        return count > 0
            ? Fin.Succ<Termination>(new StepCountCase(Count: count))
            : Fin.Fail<Termination>(op.InvalidInput());
    }
    public static Fin<Termination> ArcLength(double length, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: length)
            .Map(static l => (Termination)new ArcLengthCase(Length: l));
    }
    public static Fin<Termination> Magnitude(double threshold, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: threshold)
            .Map(static t => (Termination)new MagnitudeFloorCase(Threshold: t));
    }
    public static Fin<Termination> CrossSurface(SupportSpace surface, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(surface).ToFin(op.InvalidInput())
            .Bind(active => GeometryKernel.CanClosest(type: active.SourceType)
                ? Fin.Succ<Termination>(new CrossSurfaceCase(Surface: active))
                : Fin.Fail<Termination>(op.Unsupported(geometryType: active.SourceType, outputType: typeof(double))));
    }
    public static Fin<Termination> EnterRegion(ScalarField region, double threshold, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(region).ToFin(op.InvalidInput())
            .Bind(active => RhinoMath.IsValidDouble(x: threshold)
                ? Fin.Succ<Termination>(new EnterRegionCase(Region: active, Threshold: threshold))
                : Fin.Fail<Termination>(op.InvalidInput()));
    }
    public static Fin<Termination> LoopDetected(double closureRadius, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: closureRadius)
            .Map(static r => (Termination)new LoopDetectedCase(ClosureRadius: r));
    }
    internal Fin<bool> ShouldStop(StreamlineState state, Vector3d currentSample, Context context, Op key) => Switch(
        state: (Field: state, Sample: currentSample, Context: context, Key: key),
        stepCountCase: static (s, c) => Fin.Succ(s.Field.Steps >= c.Count),
        arcLengthCase: static (s, c) => Fin.Succ(s.Field.Arc >= c.Length.Value),
        magnitudeFloorCase: static (s, c) => Fin.Succ(s.Sample.Length < c.Threshold.Value),
        crossSurfaceCase: static (s, c) => CrossSurfaceDetected(state: s.Field, space: c.Surface, key: s.Key),
        enterRegionCase: static (s, c) => EnterRegionDetected(state: s.Field, region: c.Region, threshold: c.Threshold, context: s.Context, key: s.Key),
        loopDetectedCase: static (s, c) => Fin.Succ(LoopDetected(state: s.Field, radius: c.ClosureRadius.Value)));
    private static Fin<double> SignedDistanceFrom(SupportSpace space, Point3d sample, Op key) =>
        from hit in space.Closest(sample: sample, key: key)
        from value in space.AdmitsSignedDistance(hit: hit)
            ? space.SignedDistance(hit: hit, sample: sample, key: key)
            : hit.Distance.ToFin(Fail: key.InvalidResult())
        select value;
    private static Fin<bool> CrossSurfaceDetected(StreamlineState state, SupportSpace space, Op key) =>
        state.Trail.Count < 2
            ? Fin.Succ(false)
            : from prev in SignedDistanceFrom(space: space, sample: state.Trail[state.Trail.Count - 2], key: key)
              from curr in SignedDistanceFrom(space: space, sample: state.Current, key: key)
              select prev * curr < 0.0;
    private static Fin<bool> EnterRegionDetected(StreamlineState state, ScalarField region, double threshold, Context context, Op key) =>
        state.Trail.Count < 2
            ? Fin.Succ(false)
            : from prev in region.SampleScalar(sample: state.Trail[state.Trail.Count - 2], context: context, key: key)
              from curr in region.SampleScalar(sample: state.Current, context: context, key: key)
              select (prev - threshold) * (curr - threshold) < 0.0;
    private static bool LoopDetected(StreamlineState state, double radius) =>
        state.Trail.Count >= 3
        && toSeq(Enumerable.Range(start: 0, count: state.Trail.Count - 2))
            .Exists(i => state.Current.DistanceToSquared(other: state.Trail[i]) <= radius * radius);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FlowKernel {
    private const int MaxIterations = 100000;

    internal static Fin<TOut> Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        from trace in toSeq(Enumerable.Range(start: 0, count: MaxIterations)).Fold(
            initialState: Fin.Succ(new StreamlineState(Trail: Seq(seed), Current: seed, H: initialStep.Value, Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0, Stop: Option<StreamlineStopKind>.None)),
            f: (acc, _) => acc.Bind(s => s.Stop.IsSome
                ? Fin.Succ(s)
                : source.SampleVector(sample: s.Current, context: context, key: key).Bind(vector =>
                    termination.ShouldStop(state: s, currentSample: vector, context: context, key: key).Bind(stop => stop
                        ? Fin.Succ(s with { Stop = Some(StreamlineStopKind.Terminated) })
                        : integrator.Step(field: source, point: s.Current, h: s.H, context: context, key: key).Map(step => step.Switch(
                            state: s,
                            acceptedCase: static (state, accepted) => state with {
                                Trail = state.Trail.Add(accepted.Next),
                                Current = accepted.Next,
                                H = accepted.SuggestedStep,
                                Arc = state.Arc + accepted.Next.DistanceTo(other: state.Current),
                                Steps = state.Steps + 1,
                                Rejects = 0,
                            },
                            rejectedCase: (state, rejected) => state.Rejects >= integrator.RejectBudget
                                ? state with { H = rejected.SuggestedStep, RejectedSteps = state.RejectedSteps + 1, Stop = Some(StreamlineStopKind.RejectBudgetExhausted) }
                                : state with { H = rejected.SuggestedStep, Rejects = state.Rejects + 1, RejectedSteps = state.RejectedSteps + 1 }))))))
            .Map(static s => new StreamlineTrace(
                Trail: s.Trail,
                Stop: s.Stop.IfNone(StreamlineStopKind.IterationCapExhausted),
                AcceptedSteps: s.Steps,
                RejectedSteps: s.RejectedSteps,
                ArcLength: s.Arc,
                FinalStep: s.H))
        from output in typeof(TOut) switch {
            Type t when t == typeof(StreamlineTrace) => key.AcceptValue(value: trace).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Seq<Point3d>) => trace.IsComplete
                ? trace.Trail.TraverseM(point => key.AcceptValue(value: point)).As().Map(static value => (TOut)(object)value)
                : Fin.Fail<TOut>(key.InvalidResult()),
            Type t when t == typeof(Polyline) => trace.IsComplete
                ? key.AcceptValue(value: new Polyline(trace.Trail.AsIterable())).Map(static value => (TOut)(object)value)
                : Fin.Fail<TOut>(key.InvalidResult()),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(FlowKernel), outputType: typeof(TOut))),
        }
        select output;
}
