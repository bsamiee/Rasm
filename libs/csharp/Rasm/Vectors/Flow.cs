using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherTableau(Seq<Seq<double>> Coupling, Seq<double> Abscissae, Seq<double> Weights, Option<Seq<double>> EmbeddedWeights, int MethodOrder, Option<int> EmbeddedOrder) {
    private const double CoefficientTolerance = 1.0e-9;
    internal int StageCount => Weights.Count;
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && (EmbeddedOrder is not { IsSome: true, Case: int embedded } || (embedded > 0 && embedded < MethodOrder))
        && Coupling.Count == StageCount
        && Abscissae.Count == StageCount
        && Abscissae.ForAll(RhinoMath.IsValidDouble)
        && CoefficientsMatch(values: Weights, expected: 1.0)
        && Coupling.Zip(Abscissae).AsIterable().Select((pair, index) => pair.Item1.Count <= index
            && CoefficientsMatch(values: pair.Item1, expected: pair.Item2)).All(static ok => ok)
        && (EmbeddedWeights is not { IsSome: true, Case: Seq<double> ew } || (ew.Count == StageCount && CoefficientsMatch(values: ew, expected: 1.0)));
    internal Fin<ButcherTableau> Admit(Op key) =>
        IsValid ? Fin.Succ(this) : Fin.Fail<ButcherTableau>(key.InvalidInput());
    private static bool CoefficientsMatch(Seq<double> values, double expected) =>
        values.ForAll(RhinoMath.IsValidDouble)
        && Math.Abs(value: values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - expected) <= CoefficientTolerance;
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
    public static readonly StreamlineStopKind Terminated = new(key: 0), RejectBudgetExhausted = new(key: 1), MaxIterationsExhausted = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class TraceEventKind {
    public static readonly TraceEventKind CrossSurface = new(key: 0), RegionThresholdCrossing = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class TraceEventStatus {
    public static readonly TraceEventStatus InitialEndpointTouch = new(key: 0), PreviousEndpointTouch = new(key: 1);
    public static readonly TraceEventStatus CurrentEndpointTouch = new(key: 2), BracketedCrossing = new(key: 3);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TraceEvent(TraceEventKind Kind, TraceEventStatus Status, (Point3d Previous, Point3d Current, Point3d Localized) Points, (double Previous, double Current, double Localized) Values, double Parameter, double Tolerance, double Residual, int Iterations) {
    internal bool IsValidFor(Point3d terminationPoint) =>
        Kind is not null && Status is not null
        && Points.Previous.IsValid && Points.Current.IsValid && Points.Localized.IsValid
        && RhinoMath.IsValidDouble(x: Values.Previous) && RhinoMath.IsValidDouble(x: Values.Current) && RhinoMath.IsValidDouble(x: Values.Localized)
        && RhinoMath.IsValidDouble(x: Parameter) && Parameter is >= 0.0 and <= 1.0 && RhinoMath.IsValidDouble(x: Tolerance) && Tolerance >= 0.0
        && RhinoMath.IsValidDouble(x: Residual) && Residual >= 0.0 && Iterations >= 0
        && terminationPoint.DistanceTo(other: Points.Localized) <= RhinoMath.ZeroTolerance
        && Math.Abs(value: Residual - Math.Abs(value: Values.Localized)) <= RhinoMath.SqrtEpsilon;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamlineTrace(Seq<Point3d> Trail, StreamlineStopKind Stop, int AcceptedSteps, int RejectedSteps, double ArcLength, double FinalStep, int MethodOrder, Option<int> EmbeddedOrder, Option<double> LastError, double MaxError, double MinStep, double MaxStep, Point3d TerminationPoint, Option<TraceEvent> Event) {
    public bool IsComplete => Stop.Equals(StreamlineStopKind.Terminated);
    internal bool IsValid =>
        !Trail.IsEmpty
        && MethodOrder > 0
        && (EmbeddedOrder is not { IsSome: true, Case: int order } || (order > 0 && order < MethodOrder))
        && AcceptedSteps >= 0 && RejectedSteps >= 0 && AcceptedSteps == Trail.Count - 1 && Trail.ForAll(static point => point.IsValid)
        && RhinoMath.IsValidDouble(x: ArcLength) && RhinoMath.IsValidDouble(x: FinalStep) && RhinoMath.IsValidDouble(x: MinStep)
        && RhinoMath.IsValidDouble(x: MaxStep) && RhinoMath.IsValidDouble(x: MaxError) && ArcLength >= 0.0 && MinStep >= 0.0 && MaxStep >= MinStep
        && !LastError.Map(static error => !RhinoMath.IsValidDouble(x: error) || error < 0.0).IfNone(false)
        && (Event is not { IsSome: true, Case: TraceEvent @event } || @event.IsValidFor(terminationPoint: TerminationPoint))
        && TerminationPoint.IsValid;
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
    public sealed record FixedCase : FieldIntegrator { internal FixedCase(IntegratorKind kind) => Kind = kind; public IntegratorKind Kind { get; } }
    public sealed record AdaptiveCase : FieldIntegrator { internal AdaptiveCase(IntegratorKind kind, PositiveMagnitude tolerance, int maxRejects) { Kind = kind; Tolerance = tolerance; MaxRejects = maxRejects; } public IntegratorKind Kind { get; } public PositiveMagnitude Tolerance { get; } public int MaxRejects { get; } }
    public static Fin<FieldIntegrator> Fixed(IntegratorKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from _ in active.Tableau.Admit(key: op)
               from __ in guard(!active.IsAdaptive, op.Unsupported(geometryType: active.GetType(), outputType: typeof(FixedCase)))
               select (FieldIntegrator)new FixedCase(kind: active);
    }
    public static Fin<FieldIntegrator> Adaptive(IntegratorKind kind, double tolerance, int maxRejects = 3, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from _ in active.Tableau.Admit(key: op)
               from __ in guard(maxRejects >= 0, op.InvalidInput())
               from ___ in guard(active.IsAdaptive, op.Unsupported(geometryType: active.GetType(), outputType: typeof(AdaptiveCase)))
               from validated in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               select (FieldIntegrator)new AdaptiveCase(kind: active, tolerance: validated, maxRejects: maxRejects);
    }
    internal Fin<FieldIntegrator> Admit(Op key) =>
        Switch(
            state: key,
            fixedCase: static (op, integrator) =>
                from kind in FieldNabla.NotNull(value: integrator.Kind, key: op)
                from tableau in kind.Tableau.Admit(key: op)
                from fixedKind in guard(!kind.IsAdaptive, op.Unsupported(geometryType: kind.GetType(), outputType: typeof(FixedCase)))
                select (FieldIntegrator)integrator,
            adaptiveCase: static (op, integrator) =>
                from kind in FieldNabla.NotNull(value: integrator.Kind, key: op)
                from tableau in kind.Tableau.Admit(key: op)
                from rejects in guard(integrator.MaxRejects >= 0, op.InvalidInput())
                from adaptiveKind in guard(kind.IsAdaptive, op.Unsupported(geometryType: kind.GetType(), outputType: typeof(AdaptiveCase)))
                from tolerance in FieldNabla.Positive(value: integrator.Tolerance, key: op)
                select (FieldIntegrator)integrator);
    internal static Fin<FieldIntegrator> Admit(FieldIntegrator value, Op key) =>
        FieldNabla.NotNull(value: value, key: key).Bind(integrator => integrator.Admit(key: key));
    internal static Fin<FieldIntegrator> AdmitOrFixed(FieldIntegrator? value, Op key) =>
        value is null ? Fixed(kind: IntegratorKind.RK4, key: key) : Admit(value: value, key: key);
    internal int RejectBudget => Switch(
        state: 0,
        fixedCase: static (s, _) => s,
        adaptiveCase: static (_, c) => c.MaxRejects);
    internal ButcherTableau Tableau => Switch(
        state: default(ButcherTableau),
        fixedCase: static (_, c) => c.Kind.Tableau,
        adaptiveCase: static (_, c) => c.Kind.Tableau);
    internal int MethodOrder => Tableau.MethodOrder;
    internal Option<int> EmbeddedOrder => Tableau.EmbeddedOrder;
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

internal readonly record struct StreamlineState(Seq<Point3d> Trail, Point3d Current, double H, double Arc, int Steps, int Rejects, int RejectedSteps, double MinStep, double MaxStep, Option<double> LastError, double MaxError, Option<TraceEvent> Event, Option<StreamlineStopKind> Stop) {
    internal static StreamlineState Start(Point3d seed, double h) =>
        new(Trail: Seq(seed), Current: seed, H: h, Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0, MinStep: h, MaxStep: h, LastError: Option<double>.None, MaxError: 0.0, Event: Option<TraceEvent>.None, Stop: Option<StreamlineStopKind>.None);
    internal StreamlineState Accept(StreamlineStep.AcceptedCase accepted) =>
        Step(suggested: accepted.SuggestedStep, error: accepted.Error) with {
            Trail = Trail.Add(accepted.Next),
            Current = accepted.Next,
            Arc = Arc + accepted.Next.DistanceTo(other: Current),
            Steps = Steps + 1,
            Rejects = 0,
        };
    internal StreamlineState Reject(StreamlineStep.RejectedCase rejected, int rejectBudget) =>
        Step(suggested: rejected.SuggestedStep, error: rejected.Error) with {
            Rejects = Rejects + 1,
            RejectedSteps = RejectedSteps + 1,
            Stop = (Rejects + 1 >= rejectBudget) switch { true => Some(StreamlineStopKind.RejectBudgetExhausted), false => Stop },
        };
    private StreamlineState Step(double suggested, Option<double> error) =>
        this with {
            H = suggested,
            MinStep = Math.Min(val1: MinStep, val2: suggested),
            MaxStep = Math.Max(val1: MaxStep, val2: suggested),
            LastError = error,
            MaxError = Math.Max(val1: MaxError, val2: error.IfNone(0.0)),
        };
}

[Union]
public abstract partial record Termination {
    private const int EventBisectionMaxIterations = 64;
    private Termination() { }
    public sealed record StepCountCase(int Count) : Termination;
    public sealed record ArcLengthCase(PositiveMagnitude Length) : Termination;
    public sealed record MagnitudeFloorCase(PositiveMagnitude Threshold) : Termination;
    public sealed record CrossSurfaceCase(SupportSpace Surface, int MaxLocalizationIterations) : Termination;
    public sealed record RegionThresholdCase(ScalarField Region, double Threshold, int MaxLocalizationIterations) : Termination;
    public sealed record LoopDetectedCase(PositiveMagnitude ClosureRadius) : Termination;
    public static Fin<Termination> Steps(int count, Op? key = null) =>
        count > 0
            ? Fin.Succ<Termination>(new StepCountCase(Count: count))
            : Fin.Fail<Termination>(key.OrDefault().InvalidInput());
    public static Fin<Termination> ArcLength(double length, Op? key = null) =>
        Positive(candidate: length, create: static l => new ArcLengthCase(Length: l), key: key);
    public static Fin<Termination> Magnitude(double threshold, Op? key = null) =>
        Positive(candidate: threshold, create: static t => new MagnitudeFloorCase(Threshold: t), key: key);
    public static Fin<Termination> CrossSurface(SupportSpace surface, int maxLocalizationIterations = EventBisectionMaxIterations, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(surface).ToFin(op.InvalidInput())
               from iterations in LocalizationIterations(candidate: maxLocalizationIterations, key: op)
               from _ in guard(GeometryKernel.CanClosest(type: active.SourceType) && active.CanSignedDistance, op.Unsupported(geometryType: active.SourceType, outputType: typeof(double)))
               select (Termination)new CrossSurfaceCase(Surface: active, MaxLocalizationIterations: iterations);
    }
    public static Fin<Termination> RegionThreshold(ScalarField region, double threshold, int maxLocalizationIterations = EventBisectionMaxIterations, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(region).ToFin(op.InvalidInput())
               from iterations in LocalizationIterations(candidate: maxLocalizationIterations, key: op)
               from _ in guard(RhinoMath.IsValidDouble(x: threshold), op.InvalidInput())
               select (Termination)new RegionThresholdCase(Region: active, Threshold: threshold, MaxLocalizationIterations: iterations);
    }
    public static Fin<Termination> LoopDetected(double closureRadius, Op? key = null) =>
        Positive(candidate: closureRadius, create: static r => new LoopDetectedCase(ClosureRadius: r), key: key);
    private static Fin<Termination> Positive(double candidate, Func<PositiveMagnitude, Termination> create, Op? key) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: candidate).Map(create);
    private static Fin<int> LocalizationIterations(int candidate, Op key) =>
        candidate > 0 ? Fin.Succ(candidate) : Fin.Fail<int>(key.InvalidInput());
    internal Fin<Termination> Admit(Op key) => Switch(
        state: key,
        stepCountCase: static (op, termination) => termination.Count > 0 ? Fin.Succ<Termination>(termination) : Fin.Fail<Termination>(op.InvalidInput()),
        arcLengthCase: static (op, termination) => FieldNabla.Positive(value: termination.Length, key: op).Map(_ => (Termination)termination),
        magnitudeFloorCase: static (op, termination) => FieldNabla.Positive(value: termination.Threshold, key: op).Map(_ => (Termination)termination),
        crossSurfaceCase: static (op, termination) =>
            from surface in FieldNabla.NotNull(value: termination.Surface, key: op)
            from iterations in LocalizationIterations(candidate: termination.MaxLocalizationIterations, key: op)
            from admits in guard(GeometryKernel.CanClosest(type: surface.SourceType) && surface.CanSignedDistance, op.Unsupported(geometryType: surface.SourceType, outputType: typeof(double)))
            select (Termination)termination,
        regionThresholdCase: static (op, termination) =>
            from region in FieldNabla.NotNull(value: termination.Region, key: op)
            from iterations in LocalizationIterations(candidate: termination.MaxLocalizationIterations, key: op)
            from threshold in FieldNabla.Finite(value: termination.Threshold, key: op)
            select (Termination)termination,
        loopDetectedCase: static (op, termination) => FieldNabla.Positive(value: termination.ClosureRadius, key: op).Map(_ => (Termination)termination));
    internal static Fin<Termination> Admit(Termination value, Op key) =>
        FieldNabla.NotNull(value: value, key: key).Bind(termination => termination.Admit(key: key));
    private static Fin<(bool Stop, Option<TraceEvent> Event)> Decision(bool stop) =>
        Fin.Succ((Stop: stop, Event: Option<TraceEvent>.None));
    internal Fin<(bool Stop, Option<TraceEvent> Event)> Evaluate(StreamlineState state, Vector3d currentSample, Context context, Op key) => Switch(
        state: (Field: state, Sample: currentSample, Context: context, Key: key),
        stepCountCase: static (s, c) => Decision(stop: s.Field.Steps >= c.Count),
        arcLengthCase: static (s, c) => Decision(stop: s.Field.Arc >= c.Length.Value),
        magnitudeFloorCase: static (s, c) => Decision(stop: s.Sample.Length < c.Threshold.Value),
        loopDetectedCase: static (s, c) => Decision(stop: LoopDetected(state: s.Field, radius: c.ClosureRadius.Value)),
        crossSurfaceCase: static (s, c) => EvaluateEvent(
            state: s.Field,
            kind: TraceEventKind.CrossSurface,
            tolerance: s.Context.Absolute.Value,
            maxIterations: c.MaxLocalizationIterations,
            sample: point =>
                from hit in c.Surface.Closest(sample: point, key: s.Key)
                from value in c.Surface.AdmitsSignedDistance(hit: hit)
                    ? c.Surface.SignedDistance(hit: hit, sample: point, key: s.Key)
                    : Fin.Fail<double>(s.Key.InvalidResult())
                select value,
            key: s.Key).Map(@event => (Stop: @event.IsSome, Event: @event)),
        regionThresholdCase: static (s, c) => EvaluateEvent(
            state: s.Field,
            kind: TraceEventKind.RegionThresholdCrossing,
            tolerance: s.Context.Fractional * Math.Max(val1: 1.0, val2: Math.Abs(value: c.Threshold)),
            maxIterations: c.MaxLocalizationIterations,
            sample: point => c.Region.SampleScalar(sample: point, context: s.Context, key: s.Key).Map(value => value - c.Threshold),
            key: s.Key).Map(@event => (Stop: @event.IsSome, Event: @event)));
    private static Fin<Option<TraceEvent>> EvaluateEvent(StreamlineState state, TraceEventKind kind, double tolerance, int maxIterations, Func<Point3d, Fin<double>> sample, Op key) =>
        from currentValue in sample(state.Current)
        from output in state.Trail.Count < 2
            ? EndpointEvent(kind: kind, status: TraceEventStatus.InitialEndpointTouch, previous: state.Current, current: state.Current, previousValue: currentValue, currentValue: currentValue, localized: state.Current, localizedValue: currentValue, parameter: 0.0, tolerance: tolerance)
            : EvaluateSegmentEvent(previous: state.Trail[state.Trail.Count - 2], current: state.Current, currentValue: currentValue, kind: kind, tolerance: tolerance, maxIterations: maxIterations, sample: sample, key: key)
        select output;
    private static Fin<Option<TraceEvent>> EvaluateSegmentEvent(Point3d previous, Point3d current, double currentValue, TraceEventKind kind, double tolerance, int maxIterations, Func<Point3d, Fin<double>> sample, Op key) =>
        from previousValue in sample(previous)
        from output in Math.Abs(value: previousValue) <= tolerance
            ? EndpointEvent(kind: kind, status: TraceEventStatus.PreviousEndpointTouch, previous: previous, current: current, previousValue: previousValue, currentValue: currentValue, localized: previous, localizedValue: previousValue, parameter: 0.0, tolerance: tolerance)
            : Math.Abs(value: currentValue) <= tolerance
                ? EndpointEvent(kind: kind, status: TraceEventStatus.CurrentEndpointTouch, previous: previous, current: current, previousValue: previousValue, currentValue: currentValue, localized: current, localizedValue: currentValue, parameter: 1.0, tolerance: tolerance)
                : previousValue * currentValue < 0.0
                    ? LocateRoot(previous: previous, current: current, previousValue: previousValue, currentValue: currentValue, kind: kind, tolerance: tolerance, maxIterations: maxIterations, sample: sample, key: key).Map(Some)
                    : Fin.Succ(Option<TraceEvent>.None)
        select output;
    private static Fin<Option<TraceEvent>> EndpointEvent(TraceEventKind kind, TraceEventStatus status, Point3d previous, Point3d current, double previousValue, double currentValue, Point3d localized, double localizedValue, double parameter, double tolerance) =>
        Math.Abs(value: localizedValue) <= tolerance
            ? EventAt(kind: kind, status: status, points: (previous, current, localized), values: (previousValue, currentValue, localizedValue), parameter: parameter, tolerance: tolerance, iterations: 0).Map(Some)
            : Fin.Succ(Option<TraceEvent>.None);
    private static Fin<TraceEvent> EventAt(TraceEventKind kind, TraceEventStatus status, (Point3d Previous, Point3d Current, Point3d Localized) points, (double Previous, double Current, double Localized) values, double parameter, double tolerance, int iterations) =>
        Fin.Succ(new TraceEvent(
            Kind: kind,
            Status: status,
            Points: points,
            Values: values,
            Parameter: parameter,
            Tolerance: tolerance,
            Residual: Math.Abs(value: values.Localized),
            Iterations: iterations));
    private static Fin<TraceEvent> LocateRoot(Point3d previous, Point3d current, double previousValue, double currentValue, TraceEventKind kind, double tolerance, int maxIterations, Func<Point3d, Fin<double>> sample, Op key) =>
        from bracket in toSeq(Enumerable.Range(start: 0, count: maxIterations)).Fold(
            initialState: Fin.Succ((A: previous, B: current, FA: previousValue, FB: currentValue, TA: 0.0, TB: 1.0, Localized: previous, FLocalized: previousValue, TLocalized: 0.0, Done: false, Iterations: 0)),
            f: (acc, _) => acc.Bind(state => state.Done
                ? Fin.Succ(state)
                : Fin.Succ(state.A + (0.5 * (state.B - state.A))).Bind(mid =>
                    sample(mid).Map(fm => {
                        double tm = 0.5 * (state.TA + state.TB);
                        bool localized = Math.Abs(value: fm) <= tolerance || mid.DistanceTo(other: state.A) <= tolerance || mid.DistanceTo(other: state.B) <= tolerance;
                        return state.FA * fm <= 0.0
                            ? (state.A, mid, state.FA, fm, state.TA, tm, mid, fm, tm, localized, state.Iterations + 1)
                            : (mid, state.B, fm, state.FB, tm, state.TB, mid, fm, tm, localized, state.Iterations + 1);
                    }))))
        let localized = bracket.Done ? bracket.Localized : bracket.A + (0.5 * (bracket.B - bracket.A))
        from residual in bracket.Done ? Fin.Succ(bracket.FLocalized) : sample(localized)
        let parameter = bracket.Done ? bracket.TLocalized : 0.5 * (bracket.TA + bracket.TB)
        from @event in Math.Abs(value: residual) <= tolerance || localized.DistanceTo(other: bracket.A) <= tolerance || localized.DistanceTo(other: bracket.B) <= tolerance
            ? EventAt(kind: kind, status: TraceEventStatus.BracketedCrossing, points: (previous, current, localized), values: (previousValue, currentValue, residual), parameter: parameter, tolerance: tolerance, iterations: bracket.Iterations)
            : Fin.Fail<TraceEvent>(key.InvalidResult())
        select @event;
    private static bool LoopDetected(StreamlineState state, double radius) =>
        state.Trail.Count >= 3
        && toSeq(Enumerable.Range(start: 0, count: state.Trail.Count - 2))
            .Exists(i => state.Current.DistanceToSquared(other: state.Trail[i]) <= radius * radius);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FlowKernel {
    private const int MaxIterations = 100000;

    internal static Fin<TOut> Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        from activeIntegrator in FieldIntegrator.Admit(value: integrator, key: key)
        from activeTermination in Termination.Admit(value: termination, key: key)
        from state in TraceState(source: source, seed: seed, initialStep: initialStep, integrator: activeIntegrator, termination: activeTermination, context: context, key: key)
        let trace = ToTrace(state: state, integrator: activeIntegrator)
        from output in ProjectTrace<TOut>(trace: trace, key: key)
        select output;
    private static Fin<StreamlineState> TraceState(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        toSeq(Enumerable.Range(start: 0, count: MaxIterations)).Fold(
            initialState: Fin.Succ(StreamlineState.Start(seed: seed, h: initialStep.Value)),
            f: (acc, _) => acc.Bind(state => AdvanceState(state: state, source: source, integrator: integrator, termination: termination, context: context, key: key)));
    private static Fin<StreamlineState> AdvanceState(StreamlineState state, VectorField source, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        state.Stop.IsSome
            ? Fin.Succ(state)
            : from vector in source.SampleVector(sample: state.Current, context: context, key: key)
              from decision in termination.Evaluate(state: state, currentSample: vector, context: context, key: key)
              from next in decision.Stop
                  ? Fin.Succ(state with { Event = decision.Event, Stop = Some(StreamlineStopKind.Terminated) })
                  : integrator.Step(field: source, point: state.Current, h: state.H, context: context, key: key)
                      .Map(step => step.Switch(
                          state: (state, integrator.RejectBudget),
                          acceptedCase: static (s, accepted) => s.state.Accept(accepted: accepted),
                          rejectedCase: static (s, rejected) => s.state.Reject(rejected: rejected, rejectBudget: s.RejectBudget)))
              select next;
    private static StreamlineTrace ToTrace(StreamlineState state, FieldIntegrator integrator) =>
        new(
            Trail: state.Trail,
            Stop: state.Stop.IfNone(StreamlineStopKind.MaxIterationsExhausted),
            AcceptedSteps: state.Steps,
            RejectedSteps: state.RejectedSteps,
            ArcLength: state.Arc,
            FinalStep: state.H,
            MethodOrder: integrator.MethodOrder,
            EmbeddedOrder: integrator.EmbeddedOrder,
            LastError: state.LastError,
            MaxError: state.MaxError,
            MinStep: state.MinStep,
            MaxStep: state.MaxStep,
            TerminationPoint: state.Event.Map(static @event => @event.Points.Localized).IfNone(state.Current),
            Event: state.Event);
    internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace, Op key) =>
        from valid in trace.IsValid ? Fin.Succ(trace) : Fin.Fail<StreamlineTrace>(error: key.InvalidResult())
        from output in typeof(TOut) switch {
            Type t when t == typeof(StreamlineTrace) => Fin.Succ((TOut)(object)valid),
            Type t when t == typeof(Seq<Point3d>) => valid.Trail.TraverseM(point => key.AcceptValue(value: point)).As().Map(static value => (TOut)(object)value),
            Type t when (t == typeof(Polyline) || t == typeof(Curve)) && !valid.IsComplete => Fin.Fail<TOut>(key.InvalidResult()),
            Type t when t == typeof(Polyline) => PolylineOf(trace: valid, key: key).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Curve) => PolylineOf(trace: valid, key: key)
                .Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(key.InvalidResult()))
                .Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(StreamlineTrace), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<Polyline> PolylineOf(StreamlineTrace trace, Op key) {
        Point3d[] points = trace.Event.Match(
            Some: @event => trace.Trail.AsIterable().Select((point, index) => index == trace.Trail.Count - 1 ? @event.Points.Localized : point).ToArray(),
            None: () => [.. trace.Trail.AsIterable()]);
        Polyline polyline = [.. points];
        return polyline.IsValid
            ? key.AcceptValue(value: polyline)
            : Fin.Fail<Polyline>(key.InvalidResult());
    }
}
