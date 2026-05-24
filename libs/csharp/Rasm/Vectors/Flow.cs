using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherTableau(
    Seq<Seq<double>> Coupling,
    Seq<double> Abscissae,
    Seq<double> Weights,
    Option<Seq<double>> EmbeddedWeights,
    int MethodOrder,
    Option<int> EmbeddedOrder) {
    internal int StageCount => Weights.Count;
    internal bool IsValid {
        get {
            Seq<double> abscissae = Abscissae;
            return StageCount > 0
                && MethodOrder > 0
                && (EmbeddedOrder is not { IsSome: true, Case: int embedded } || (embedded > 0 && embedded < MethodOrder))
                && Coupling.Count == StageCount
                && abscissae.Count == StageCount
                && CoefficientsAreFinite(values: abscissae)
                && CoefficientsAreFinite(values: Weights)
                && Math.Abs(value: SumCoefficients(values: Weights) - 1.0) <= 1.0e-9
                && Coupling.AsIterable().Select((row, index) => row.Count <= index
                    && CoefficientsAreFinite(values: row)
                    && Math.Abs(value: SumCoefficients(values: row) - abscissae[index]) <= 1.0e-9).All(static ok => ok)
                && (EmbeddedWeights is not { IsSome: true, Case: Seq<double> ew }
                    || (ew.Count == StageCount
                        && CoefficientsAreFinite(values: ew)
                        && Math.Abs(value: SumCoefficients(values: ew) - 1.0) <= 1.0e-9));
        }
    }
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
    internal bool IsAdaptive => Tableau.EmbeddedWeights.IsSome;
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
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Abscissae: toSeq(coupling.Select(static r => r.Sum())), Weights: toSeq(weights), EmbeddedWeights: Option<Seq<double>>.None, MethodOrder: order, EmbeddedOrder: Option<int>.None));
    private static IntegratorKind Adaptive(int key, int order, int embeddedOrder, double[][] coupling, double[] weights, double[] errorWeights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Abscissae: toSeq(coupling.Select(static r => r.Sum())), Weights: toSeq(weights), EmbeddedWeights: Some(toSeq(errorWeights)), MethodOrder: order, EmbeddedOrder: Some(embeddedOrder)));
}

[SmartEnum<int>]
public sealed partial class StreamlineStopKind {
    public static readonly StreamlineStopKind Terminated = new(key: 0);
    public static readonly StreamlineStopKind RejectBudgetExhausted = new(key: 1);
    public static readonly StreamlineStopKind IterationCapExhausted = new(key: 2);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TraceEvent(
    Point3d Previous,
    Point3d Current,
    Point3d Localized,
    double Residual,
    int Iterations);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamlineTrace(
    Seq<Point3d> Trail,
    StreamlineStopKind Stop,
    int AcceptedSteps,
    int RejectedSteps,
    double ArcLength,
    double FinalStep,
    int MethodOrder,
    Option<int> EmbeddedOrder,
    Option<double> LastError,
    double MaxError,
    double MinStep,
    double MaxStep,
    Point3d TerminationPoint,
    Option<TraceEvent> Event) {
    public bool IsComplete => Stop.Equals(StreamlineStopKind.Terminated);
}

[Union]
internal abstract partial record StreamlineStep {
    private StreamlineStep() { }
    public sealed record AcceptedCase(Point3d Next, double SuggestedStep, Option<double> Error) : StreamlineStep;
    public sealed record RejectedCase(double SuggestedStep, Option<double> Error) : StreamlineStep;
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
    internal int MethodOrder => Switch(
        state: 0,
        fixedCase: static (_, c) => c.Kind.Order,
        adaptiveCase: static (_, c) => c.Kind.Order);
    internal Option<int> EmbeddedOrder => Switch(
        state: Option<int>.None,
        fixedCase: static (s, _) => s,
        adaptiveCase: static (_, c) => c.Kind.EmbeddedOrder);
    internal Fin<StreamlineStep> Step(VectorField field, Point3d point, double h, Context context, Op key) => Switch(
        state: (Field: field, Point: point, H: h, Context: context, Key: key),
        fixedCase: static (s, c) =>
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            from next in s.Key.AcceptValue(value: s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks)))
            select (StreamlineStep)new StreamlineStep.AcceptedCase(Next: next, SuggestedStep: s.H, Error: Option<double>.None),
        adaptiveCase: static (s, c) =>
            from embeddedWeights in c.Kind.Tableau.EmbeddedWeights.ToFin(Fail: s.Key.InvalidInput())
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            let primary = s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks))
            let secondary = s.Point + (s.H * Combine(coefficients: embeddedWeights, vectors: ks))
            let err = (primary - secondary).Length
            let scale = err > RhinoMath.ZeroTolerance
                ? Math.Clamp(
                    value: IntegratorKind.AdaptiveSafetyFactor * Math.Pow(x: c.Tolerance.Value / err, y: c.Kind.AdaptiveExponent),
                    min: IntegratorKind.AdaptiveMinScale,
                    max: IntegratorKind.AdaptiveMaxScale)
                : IntegratorKind.AdaptiveMaxScale
            from result in err <= c.Tolerance.Value
                ? s.Key.AcceptValue(value: (StreamlineStep)new StreamlineStep.AcceptedCase(Next: primary, SuggestedStep: s.H * scale, Error: Some(err)))
                : s.Key.AcceptValue(value: (StreamlineStep)new StreamlineStep.RejectedCase(SuggestedStep: s.H * scale, Error: Some(err)))
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
    int RejectBudget,
    int MethodOrder,
    Option<int> EmbeddedOrder,
    double MinStep,
    double MaxStep,
    Option<double> LastError,
    double MaxError,
    Option<TraceEvent> Event,
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
    internal Fin<Option<TraceEvent>> LocateEvent(StreamlineState state, Context context, Op key) => Switch(
        state: (Field: state, Context: context, Key: key),
        stepCountCase: static (s, _) => Fin.Succ(Option<TraceEvent>.None),
        arcLengthCase: static (s, _) => Fin.Succ(Option<TraceEvent>.None),
        magnitudeFloorCase: static (s, _) => Fin.Succ(Option<TraceEvent>.None),
        loopDetectedCase: static (s, _) => Fin.Succ(Option<TraceEvent>.None),
        crossSurfaceCase: static (s, c) => LocateSurfaceEvent(state: s.Field, space: c.Surface, key: s.Key),
        enterRegionCase: static (s, c) => LocateRegionEvent(state: s.Field, region: c.Region, threshold: c.Threshold, context: s.Context, key: s.Key));
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
    private static Fin<Option<TraceEvent>> LocateSurfaceEvent(StreamlineState state, SupportSpace space, Op key) =>
        state.Trail.Count < 2
            ? Fin.Succ(Option<TraceEvent>.None)
            : LocateRoot(previous: state.Trail[state.Trail.Count - 2], current: state.Current, sample: point => SignedDistanceFrom(space: space, sample: point, key: key), key: key).Map(Some);
    private static Fin<Option<TraceEvent>> LocateRegionEvent(StreamlineState state, ScalarField region, double threshold, Context context, Op key) =>
        state.Trail.Count < 2
            ? Fin.Succ(Option<TraceEvent>.None)
            : LocateRoot(previous: state.Trail[state.Trail.Count - 2], current: state.Current, sample: point => region.SampleScalar(sample: point, context: context, key: key).Map(value => value - threshold), key: key).Map(Some);
    private static Fin<TraceEvent> LocateRoot(Point3d previous, Point3d current, Func<Point3d, Fin<double>> sample, Op key) =>
        from a in sample(previous)
        from b in sample(current)
        from bracket in toSeq(Enumerable.Range(start: 0, count: 12)).Fold(
            initialState: Fin.Succ((A: previous, B: current, FA: a, FB: b)),
            f: (acc, _) => acc.Bind(state => {
                Point3d mid = new(
                    x: 0.5 * (state.A.X + state.B.X),
                    y: 0.5 * (state.A.Y + state.B.Y),
                    z: 0.5 * (state.A.Z + state.B.Z));
                return sample(mid).Map(fm => state.FA * fm <= 0.0
                    ? (state.A, mid, state.FA, fm)
                    : (mid, state.B, fm, state.FB));
            }))
        let localized = new Point3d(
            x: 0.5 * (bracket.A.X + bracket.B.X),
            y: 0.5 * (bracket.A.Y + bracket.B.Y),
            z: 0.5 * (bracket.A.Z + bracket.B.Z))
        from residual in sample(localized)
        select new TraceEvent(Previous: previous, Current: current, Localized: localized, Residual: Math.Abs(value: residual), Iterations: 12);
    private static bool LoopDetected(StreamlineState state, double radius) =>
        state.Trail.Count >= 3
        && toSeq(Enumerable.Range(start: 0, count: state.Trail.Count - 2))
            .Exists(i => state.Current.DistanceToSquared(other: state.Trail[i]) <= radius * radius);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FlowKernel {
    private const int MaxIterations = 100000;

    internal static Fin<TOut> Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        from state in TraceState(source: source, seed: seed, initialStep: initialStep, integrator: integrator, termination: termination, context: context, key: key)
        let trace = ToTrace(state: state)
        from output in ProjectTraceOutput<TOut>(trace: trace, key: key)
        select output;
    private static Fin<StreamlineState> TraceState(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        toSeq(Enumerable.Range(start: 0, count: MaxIterations)).Fold(
            initialState: Fin.Succ(new StreamlineState(
                Trail: Seq(seed),
                Current: seed,
                H: initialStep.Value,
                Arc: 0.0,
                Steps: 0,
                Rejects: 0,
                RejectedSteps: 0,
                RejectBudget: integrator.RejectBudget,
                MethodOrder: integrator.MethodOrder,
                EmbeddedOrder: integrator.EmbeddedOrder,
                MinStep: initialStep.Value,
                MaxStep: initialStep.Value,
                LastError: Option<double>.None,
                MaxError: 0.0,
                Event: Option<TraceEvent>.None,
                Stop: Option<StreamlineStopKind>.None)),
            f: (acc, _) => acc.Bind(state => AdvanceState(state: state, source: source, integrator: integrator, termination: termination, context: context, key: key)));
    private static Fin<StreamlineState> AdvanceState(StreamlineState state, VectorField source, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        state.Stop.IsSome
            ? Fin.Succ(state)
            : from vector in source.SampleVector(sample: state.Current, context: context, key: key)
              from stop in termination.ShouldStop(state: state, currentSample: vector, context: context, key: key)
              from next in stop
                  ? termination.LocateEvent(state: state, context: context, key: key)
                      .Map(@event => state with { Event = @event, Stop = Some(StreamlineStopKind.Terminated) })
                  : integrator.Step(field: source, point: state.Current, h: state.H, context: context, key: key)
                      .Map(step => step.Switch(
                          state: state,
                          acceptedCase: static (s, accepted) => s with {
                              Trail = s.Trail.Add(accepted.Next),
                              Current = accepted.Next,
                              H = accepted.SuggestedStep,
                              Arc = s.Arc + accepted.Next.DistanceTo(other: s.Current),
                              Steps = s.Steps + 1,
                              Rejects = 0,
                              MinStep = Math.Min(val1: s.MinStep, val2: s.H),
                              MaxStep = Math.Max(val1: s.MaxStep, val2: s.H),
                              LastError = accepted.Error,
                              MaxError = Math.Max(val1: s.MaxError, val2: accepted.Error.IfNone(0.0)),
                          },
                          rejectedCase: static (s, rejected) => s.Rejects + 1 >= s.RejectBudget
                              ? s with {
                                  H = rejected.SuggestedStep,
                                  RejectedSteps = s.RejectedSteps + 1,
                                  MinStep = Math.Min(val1: s.MinStep, val2: s.H),
                                  MaxStep = Math.Max(val1: s.MaxStep, val2: s.H),
                                  LastError = rejected.Error,
                                  MaxError = Math.Max(val1: s.MaxError, val2: rejected.Error.IfNone(0.0)),
                                  Stop = Some(StreamlineStopKind.RejectBudgetExhausted),
                              }
                              : s with {
                                  H = rejected.SuggestedStep,
                                  Rejects = s.Rejects + 1,
                                  RejectedSteps = s.RejectedSteps + 1,
                                  MinStep = Math.Min(val1: s.MinStep, val2: s.H),
                                  MaxStep = Math.Max(val1: s.MaxStep, val2: s.H),
                                  LastError = rejected.Error,
                                  MaxError = Math.Max(val1: s.MaxError, val2: rejected.Error.IfNone(0.0)),
                              }))
              select next;
    private static StreamlineTrace ToTrace(StreamlineState state) =>
        new(
            Trail: state.Trail,
            Stop: state.Stop.IfNone(StreamlineStopKind.IterationCapExhausted),
            AcceptedSteps: state.Steps,
            RejectedSteps: state.RejectedSteps,
            ArcLength: state.Arc,
            FinalStep: state.H,
            MethodOrder: state.MethodOrder,
            EmbeddedOrder: state.EmbeddedOrder,
            LastError: state.LastError,
            MaxError: state.MaxError,
            MinStep: state.MinStep,
            MaxStep: state.MaxStep,
            TerminationPoint: state.Event.Map(static @event => @event.Localized).IfNone(state.Current),
            Event: state.Event);
    private static Fin<TOut> ProjectTraceOutput<TOut>(StreamlineTrace trace, Op key) =>
        ProjectTrace<TOut>(trace: trace, key: key);
    internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(StreamlineTrace) => ValidateTrace(trace: trace, key: key).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Seq<Point3d>) => trace.Trail.TraverseM(point => key.AcceptValue(value: point)).As().Map(static value => (TOut)(object)value),
            Type t when (t == typeof(Polyline) || t == typeof(Curve)) && !trace.IsComplete => Fin.Fail<TOut>(key.InvalidResult()),
            Type t when t == typeof(Polyline) => PolylineOf(trace: trace, key: key).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Curve) => PolylineOf(trace: trace, key: key)
                    .Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(key.InvalidResult()))
                    .Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(StreamlineTrace), outputType: typeof(TOut))),
        };
    private static Fin<StreamlineTrace> ValidateTrace(StreamlineTrace trace, Op key) =>
        trace.Trail.IsEmpty
        || trace.MethodOrder <= 0
        || trace.EmbeddedOrder.Map(order => order <= 0 || order >= trace.MethodOrder).IfNone(false)
        || trace.AcceptedSteps < 0
        || trace.RejectedSteps < 0
        || trace.AcceptedSteps > trace.Trail.Count - 1
        || !RhinoMath.IsValidDouble(x: trace.ArcLength)
        || !RhinoMath.IsValidDouble(x: trace.FinalStep)
        || !RhinoMath.IsValidDouble(x: trace.MinStep)
        || !RhinoMath.IsValidDouble(x: trace.MaxStep)
        || !RhinoMath.IsValidDouble(x: trace.MaxError)
        || trace.ArcLength < 0.0
        || trace.MinStep < 0.0
        || trace.MaxStep < trace.MinStep
        || trace.LastError.Map(static error => !RhinoMath.IsValidDouble(x: error) || error < 0.0).IfNone(false)
        || trace.Event.Map(static @event => !@event.Previous.IsValid || !@event.Current.IsValid || !@event.Localized.IsValid || !RhinoMath.IsValidDouble(x: @event.Residual) || @event.Residual < 0.0 || @event.Iterations < 0).IfNone(false)
        || !trace.TerminationPoint.IsValid
            ? Fin.Fail<StreamlineTrace>(error: key.InvalidResult())
            : Fin.Succ(trace);
    private static Fin<Polyline> PolylineOf(StreamlineTrace trace, Op key) {
        Polyline polyline = [.. trace.Trail.AsIterable()];
        return polyline.IsValid
            ? key.AcceptValue(value: polyline)
            : Fin.Fail<Polyline>(key.InvalidResult());
    }
}
