using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
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
    internal const double AdaptiveSafetyFactor = 0.9;
    internal const double AdaptiveMinScale = 0.2;
    internal const double AdaptiveMaxScale = 10.0;
    public ButcherTableau Tableau { get; }
    internal bool IsAdaptive => Tableau.EmbeddedWeights.IsSome;
    internal double AdaptiveExponent => Tableau.EmbeddedOrder
        .Map(static order => 1.0 / (order + 1.0))
        .IfNone(0.2);
    private static IntegratorKind Fixed(int key, int order, double[][] coupling, double[] weights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Abscissae: toSeq(coupling.Select(static r => r.Sum())), Weights: toSeq(weights), EmbeddedWeights: Option<Seq<double>>.None, MethodOrder: order, EmbeddedOrder: Option<int>.None));
    private static IntegratorKind Adaptive(int key, int order, int embeddedOrder, double[][] coupling, double[] weights, double[] errorWeights) =>
        new(key: key, tableau: new ButcherTableau(Coupling: toSeq(coupling.Select(static r => toSeq(r))), Abscissae: toSeq(coupling.Select(static r => r.Sum())), Weights: toSeq(weights), EmbeddedWeights: Some(toSeq(errorWeights)), MethodOrder: order, EmbeddedOrder: Some(embeddedOrder)));
}

[Union]
public abstract partial record FieldIntegrator {
    public sealed record FixedCase : FieldIntegrator { internal FixedCase(IntegratorKind kind) => Kind = kind; public IntegratorKind Kind { get; } }
    public sealed record AdaptiveCase : FieldIntegrator { internal AdaptiveCase(IntegratorKind kind, PositiveMagnitude tolerance, int maxRejects) { Kind = kind; Tolerance = tolerance; MaxRejects = maxRejects; } public IntegratorKind Kind { get; } public PositiveMagnitude Tolerance { get; } public int MaxRejects { get; } }
    private FieldIntegrator() { }
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
    internal Fin<StreamlineStep> Step(VectorField field, Point3d point, double h, Context context, Op key) => Switch(
        state: (Field: field, Point: point, H: h, Context: context, Key: key),
        fixedCase: static (s, c) =>
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            from next in s.Key.AcceptValue(value: s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks)))
            from dense in DenseOutputState.Of(start: s.Point, end: next, step: s.H, stages: ks, tableau: c.Kind.Tableau, key: s.Key)
            select (StreamlineStep)new StreamlineStep.AcceptedCase(Next: next, SuggestedStep: s.H, Error: Option<double>.None, Dense: dense),
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
                ? DenseOutputState.Of(start: s.Point, end: primary, step: s.H, stages: ks, tableau: c.Kind.Tableau, key: s.Key)
                    .Map(dense => (StreamlineStep)new StreamlineStep.AcceptedCase(Next: primary, SuggestedStep: s.H * scale, Error: Some(err), Dense: dense))
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
            f: static (sum, pair) => sum + (pair.First * pair.Second));
}

[SmartEnum<int>]
public sealed partial class StreamlineStopKind {
    public static readonly StreamlineStopKind Terminated = new(key: 0);
    public static readonly StreamlineStopKind RejectBudgetExhausted = new(key: 1);
    public static readonly StreamlineStopKind MaxIterationsExhausted = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class TraceEventKind {
    public static readonly TraceEventKind CrossSurface = new(key: 0);
    public static readonly TraceEventKind RegionThresholdCrossing = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class TraceEventStatus {
    public static readonly TraceEventStatus InitialEndpointTouch = new(key: 0);
    public static readonly TraceEventStatus PreviousEndpointTouch = new(key: 1);
    public static readonly TraceEventStatus CurrentEndpointTouch = new(key: 2);
    public static readonly TraceEventStatus BracketedCrossing = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class TraceEventLocalizationKind {
    public static readonly TraceEventLocalizationKind BoundedBisection = new(key: 0);
    public static readonly TraceEventLocalizationKind DenseOutputRoot = new(key: 1);
}

[Union]
public abstract partial record Termination {
    public sealed record StepCountCase(int Count) : Termination;
    public sealed record ArcLengthCase(PositiveMagnitude Length) : Termination;
    public sealed record MagnitudeFloorCase(PositiveMagnitude Threshold) : Termination;
    public sealed record CrossSurfaceCase(SupportSpace Surface, int MaxLocalizationIterations) : Termination;
    public sealed record RegionThresholdCase(ScalarField Region, double Threshold, int MaxLocalizationIterations) : Termination;
    public sealed record LoopDetectedCase(PositiveMagnitude ClosureRadius) : Termination;
    private const int EventBisectionMaxIterations = 64;
    private Termination() { }
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
    private static Fin<Termination> Positive(double candidate, Func<PositiveMagnitude, Termination> create, Op? key) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: candidate).Map(create);
    private static Fin<int> LocalizationIterations(int candidate, Op key) =>
        candidate > 0 ? Fin.Succ(candidate) : Fin.Fail<int>(key.InvalidInput());
    private static Fin<(bool Stop, Option<TraceEvent> Event)> Decision(bool stop) =>
        Fin.Succ((Stop: stop, Event: Option<TraceEvent>.None));
    private static Fin<Option<TraceEvent>> EvaluateEvent(StreamlineState state, TraceEventKind kind, double tolerance, int maxIterations, Func<Point3d, Fin<double>> sample, Op key) =>
        from currentValue in sample(state.Current)
        from output in state.Trail.Count < 2
            ? EndpointEvent(kind: kind, status: TraceEventStatus.InitialEndpointTouch, previous: state.Current, current: state.Current, previousValue: currentValue, currentValue: currentValue, localized: state.Current, localizedValue: currentValue, parameter: 0.0, tolerance: tolerance)
            : EvaluateSegmentEvent(previous: state.Trail[state.Trail.Count - 2], current: state.Current, dense: state.Dense, currentValue: currentValue, kind: kind, tolerance: tolerance, maxIterations: maxIterations, sample: sample, key: key)
        select output;
    private static Fin<Option<TraceEvent>> EvaluateSegmentEvent(Point3d previous, Point3d current, Option<DenseOutputState> dense, double currentValue, TraceEventKind kind, double tolerance, int maxIterations, Func<Point3d, Fin<double>> sample, Op key) =>
        from previousValue in sample(previous)
        from output in Math.Abs(value: previousValue) <= tolerance
            ? EndpointEvent(kind: kind, status: TraceEventStatus.PreviousEndpointTouch, previous: previous, current: current, previousValue: previousValue, currentValue: currentValue, localized: previous, localizedValue: previousValue, parameter: 0.0, tolerance: tolerance)
            : Math.Abs(value: currentValue) <= tolerance
                ? EndpointEvent(kind: kind, status: TraceEventStatus.CurrentEndpointTouch, previous: previous, current: current, previousValue: previousValue, currentValue: currentValue, localized: current, localizedValue: currentValue, parameter: 1.0, tolerance: tolerance)
                : previousValue * currentValue < 0.0
                    ? LocateRoot(previous: previous, current: current, dense: dense, previousValue: previousValue, currentValue: currentValue, kind: kind, tolerance: tolerance, maxIterations: maxIterations, sample: sample, key: key).Map(Some)
                    : Fin.Succ(Option<TraceEvent>.None)
        select output;
    private static Fin<Option<TraceEvent>> EndpointEvent(TraceEventKind kind, TraceEventStatus status, Point3d previous, Point3d current, double previousValue, double currentValue, Point3d localized, double localizedValue, double parameter, double tolerance) =>
        Math.Abs(value: localizedValue) <= tolerance
            ? EventAt(kind: kind, status: status, points: (previous, current, localized), values: (previousValue, currentValue, localizedValue), parameter: parameter, tolerance: tolerance, iterations: 0, localizationKind: TraceEventLocalizationKind.BoundedBisection, denseOutput: Option<DenseOutputReceipt>.None).Map(Some)
            : Fin.Succ(Option<TraceEvent>.None);
    private static Fin<TraceEvent> EventAt(TraceEventKind kind, TraceEventStatus status, (Point3d Previous, Point3d Current, Point3d Localized) points, (double Previous, double Current, double Localized) values, double parameter, double tolerance, int iterations, TraceEventLocalizationKind localizationKind, Option<DenseOutputReceipt> denseOutput) =>
        Fin.Succ(new TraceEvent(
            Kind: kind,
            Status: status,
            Points: points,
            Values: values,
            Parameter: parameter,
            Tolerance: tolerance,
            Residual: Math.Abs(value: values.Localized),
            Iterations: iterations,
            LocalizationKind: localizationKind,
            DenseOutput: denseOutput));
    private static Fin<TraceEvent> LocateRoot(Point3d previous, Point3d current, Option<DenseOutputState> dense, double previousValue, double currentValue, TraceEventKind kind, double tolerance, int maxIterations, Func<Point3d, Fin<double>> sample, Op key) =>
        from bracket in toSeq(Enumerable.Range(start: 0, count: maxIterations)).Fold(
            initialState: Fin.Succ((A: previous, B: current, FA: previousValue, FB: currentValue, TA: 0.0, TB: 1.0, Localized: previous, FLocalized: previousValue, TLocalized: 0.0, Done: false, Iterations: 0)),
            f: (acc, _) => acc.Bind(state => state.Done
                ? Fin.Succ(state)
                : PointAt(previous: previous, current: current, dense: dense, theta: 0.5 * (state.TA + state.TB), key: key).Bind(mid =>
                    sample(mid).Map(fm => {
                        double tm = 0.5 * (state.TA + state.TB);
                        bool localized = Math.Abs(value: fm) <= tolerance || mid.DistanceTo(other: state.A) <= tolerance || mid.DistanceTo(other: state.B) <= tolerance;
                        return state.FA * fm <= 0.0
                            ? (state.A, mid, state.FA, fm, state.TA, tm, mid, fm, tm, localized, state.Iterations + 1)
                            : (mid, state.B, fm, state.FB, tm, state.TB, mid, fm, tm, localized, state.Iterations + 1);
                    }))))
        from localized in bracket.Done ? Fin.Succ(bracket.Localized) : PointAt(previous: previous, current: current, dense: dense, theta: 0.5 * (bracket.TA + bracket.TB), key: key)
        from residual in bracket.Done ? Fin.Succ(bracket.FLocalized) : sample(localized)
        let parameter = bracket.Done ? bracket.TLocalized : 0.5 * (bracket.TA + bracket.TB)
        from @event in Math.Abs(value: residual) <= tolerance || localized.DistanceTo(other: bracket.A) <= tolerance || localized.DistanceTo(other: bracket.B) <= tolerance
            ? EventAt(kind: kind, status: TraceEventStatus.BracketedCrossing, points: (previous, current, localized), values: (previousValue, currentValue, residual), parameter: parameter, tolerance: tolerance, iterations: bracket.Iterations, localizationKind: dense.Map(static _ => TraceEventLocalizationKind.DenseOutputRoot).IfNone(TraceEventLocalizationKind.BoundedBisection), denseOutput: dense.Map(static state => state.Receipt))
            : Fin.Fail<TraceEvent>(key.InvalidResult())
        select @event;
    private static Fin<Point3d> PointAt(Point3d previous, Point3d current, Option<DenseOutputState> dense, double theta, Op key) =>
        dense.Match(
            Some: state => state.PointAt(theta: theta, key: key),
            None: () => key.AcceptValue(value: previous + (theta * (current - previous))));
    private static bool LoopDetected(StreamlineState state, double radius) =>
        state.Trail.Count >= 3
        && toSeq(Enumerable.Range(start: 0, count: state.Trail.Count - 2))
            .Exists(i => state.Current.DistanceToSquared(other: state.Trail[i]) <= radius * radius);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherTableau(Seq<Seq<double>> Coupling, Seq<double> Abscissae, Seq<double> Weights, Option<Seq<double>> EmbeddedWeights, int MethodOrder, Option<int> EmbeddedOrder) {
    internal const double CoefficientTolerance = 1.0e-9;
    internal int StageCount => Weights.Count;
    public ButcherMomentReceipt MomentReceipt =>
        MomentReceiptOf(weights: Weights, order: MethodOrder, embeddedOrder: EmbeddedOrder);
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && (EmbeddedOrder is not { IsSome: true, Case: int embedded } || (embedded > 0 && embedded < MethodOrder))
        && Coupling.Count == StageCount
        && Abscissae.Count == StageCount
        && Abscissae.ForAll(RhinoMath.IsValidDouble)
        && CoefficientsMatch(values: Weights, expected: 1.0)
        && MomentReceipt.IsValid
        && Coupling.Zip(Abscissae).AsIterable().Select((pair, index) => pair.First.Count <= index
            && CoefficientsMatch(values: pair.First, expected: pair.Second)).All(static ok => ok)
        && (EmbeddedWeights is not { IsSome: true, Case: Seq<double> ew } || (ew.Count == StageCount && CoefficientsMatch(values: ew, expected: 1.0) && MomentReceiptOf(weights: ew, order: EmbeddedOrder.IfNone(1), embeddedOrder: EmbeddedOrder).IsValid));
    internal Fin<ButcherTableau> Admit(Op key) =>
        IsValid ? Fin.Succ(this) : Fin.Fail<ButcherTableau>(key.InvalidInput());
    internal Fin<DenseOutputReceipt> DenseOutputReceipt(Op key) =>
        ButcherDenseOutput.Receipt(tableau: this, key: key);
    internal Fin<Seq<double>> DenseWeightsAt(double theta, Op key) =>
        ButcherDenseOutput.WeightsAt(tableau: this, theta: theta, key: key);
    private static bool CoefficientsMatch(Seq<double> values, double expected) =>
        values.ForAll(RhinoMath.IsValidDouble)
        && Math.Abs(value: values.Fold(initialState: 0.0, f: static (sum, value) => sum + value) - expected) <= CoefficientTolerance;
    private ButcherMomentReceipt MomentReceiptOf(Seq<double> weights, int order, Option<int> embeddedOrder) {
        (int Count, int Failed, double Max) state = (0, 0, 0.0);
        state = Check(state: state, actual: weights.Zip(Abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second)), expected: 0.5, active: order >= 2);
        state = Check(state: state, actual: weights.Zip(Abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second * pair.Second)), expected: 1.0 / 3.0, active: order >= 3);
        state = Check(state: state, actual: weights.Zip(Abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second * pair.Second * pair.Second)), expected: 0.25, active: order >= 4);
        state = Check(state: state, actual: weights.Zip(Ac(coupling: Coupling, abscissae: Abscissae)).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second)), expected: 1.0 / 6.0, active: order >= 3);
        return new ButcherMomentReceipt(StageCount: StageCount, MethodOrder: order, EmbeddedOrder: embeddedOrder, CheckedConditionCount: state.Count, FailedConditionCount: state.Failed, MaxResidual: state.Max);
    }
    private static Seq<double> Ac(Seq<Seq<double>> coupling, Seq<double> abscissae) =>
        coupling.Map(row => row.Zip(abscissae).Fold(initialState: 0.0, f: static (sum, pair) => sum + (pair.First * pair.Second)));
    private static (int Count, int Failed, double Max) Check((int Count, int Failed, double Max) state, double actual, double expected, bool active) {
        if (!active) return state;
        double residual = Math.Abs(value: actual - expected);
        return (Count: state.Count + 1, Failed: state.Failed + (RhinoMath.IsValidDouble(x: residual) && residual <= CoefficientTolerance ? 0 : 1), Max: Math.Max(val1: state.Max, val2: residual));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherMomentReceipt(int StageCount, int MethodOrder, Option<int> EmbeddedOrder, int CheckedConditionCount, int FailedConditionCount, double MaxResidual) {
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && CheckedConditionCount >= 0
        && FailedConditionCount == 0
        && RhinoMath.IsValidDouble(x: MaxResidual)
        && MaxResidual <= ButcherTableau.CoefficientTolerance;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseOutputReceipt(int StageCount, int MethodOrder, int DenseOrder, int CheckedThetaCount, int CheckedConditionCount, int FailedConditionCount, double MaxResidual, bool UsesStageDerivatives) {
    internal bool IsValid =>
        StageCount > 0
        && MethodOrder > 0
        && DenseOrder > 0
        && DenseOrder <= MethodOrder
        && CheckedThetaCount > 0
        && CheckedConditionCount >= CheckedThetaCount
        && FailedConditionCount == 0
        && RhinoMath.IsValidDouble(x: MaxResidual)
        && MaxResidual <= ButcherTableau.CoefficientTolerance
        && UsesStageDerivatives;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TraceEvent(TraceEventKind Kind, TraceEventStatus Status, (Point3d Previous, Point3d Current, Point3d Localized) Points, (double Previous, double Current, double Localized) Values, double Parameter, double Tolerance, double Residual, int Iterations, TraceEventLocalizationKind LocalizationKind, Option<DenseOutputReceipt> DenseOutput) {
    internal bool IsValidFor(Point3d terminationPoint) =>
        Kind is not null && Status is not null
        && LocalizationKind is not null
        && Points.Previous.IsValid && Points.Current.IsValid && Points.Localized.IsValid
        && RhinoMath.IsValidDouble(x: Values.Previous) && RhinoMath.IsValidDouble(x: Values.Current) && RhinoMath.IsValidDouble(x: Values.Localized)
        && RhinoMath.IsValidDouble(x: Parameter) && Parameter is >= 0.0 and <= 1.0 && RhinoMath.IsValidDouble(x: Tolerance) && Tolerance >= 0.0
        && RhinoMath.IsValidDouble(x: Residual) && Residual >= 0.0 && Iterations >= 0
        && (LocalizationKind.Equals(TraceEventLocalizationKind.DenseOutputRoot)
            ? DenseOutput.Map(static receipt => receipt.IsValid).IfNone(noneValue: false)
            : DenseOutput.Map(static receipt => receipt.IsValid).IfNone(noneValue: true))
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
        && !LastError.Map(static error => !RhinoMath.IsValidDouble(x: error) || error < 0.0).IfNone(noneValue: false)
        && (Event is not { IsSome: true, Case: TraceEvent @event } || @event.IsValidFor(terminationPoint: TerminationPoint))
        && TerminationPoint.IsValid;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class ButcherDenseOutput {
    internal static Fin<DenseOutputReceipt> Receipt(ButcherTableau tableau, Op key) =>
        ReceiptAt(tableau: tableau, theta: 0.0, key: key).Bind(zero =>
            ReceiptAt(tableau: tableau, theta: 0.5, key: key).Bind(mid =>
                ReceiptAt(tableau: tableau, theta: 1.0, key: key).Bind(one => {
                    DenseOutputReceipt receipt = new(
                        StageCount: tableau.StageCount,
                        MethodOrder: tableau.MethodOrder,
                        DenseOrder: DenseOrder(tableau: tableau),
                        CheckedThetaCount: 3,
                        CheckedConditionCount: zero.CheckedConditionCount + mid.CheckedConditionCount + one.CheckedConditionCount,
                        FailedConditionCount: zero.FailedConditionCount + mid.FailedConditionCount + one.FailedConditionCount,
                        MaxResidual: Math.Max(val1: zero.MaxResidual, val2: Math.Max(val1: mid.MaxResidual, val2: one.MaxResidual)),
                        UsesStageDerivatives: true);
                    return receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<DenseOutputReceipt>(key.InvalidResult());
                })));
    internal static Fin<Seq<double>> WeightsAt(ButcherTableau tableau, double theta, Op key) =>
        !RhinoMath.IsValidDouble(x: theta) || theta is < 0.0 or > 1.0
            ? Fin.Fail<Seq<double>>(key.InvalidInput())
            : Weights(tableau: tableau, theta: theta, key: key);
    private static int DenseOrder(ButcherTableau tableau) =>
        Math.Max(val1: 1, val2: Math.Min(val1: tableau.MethodOrder, val2: DistinctAbscissaCount(tableau: tableau)));
    private static int DistinctAbscissaCount(ButcherTableau tableau) {
        List<double> distinct = [];
        foreach (double c in tableau.Abscissae.AsIterable())
            if (!distinct.Exists(active => Math.Abs(value: active - c) <= ButcherTableau.CoefficientTolerance)) distinct.Add(c);
        return distinct.Count;
    }
    private static Fin<DenseOutputReceipt> ReceiptAt(ButcherTableau tableau, double theta, Op key) =>
        Weights(tableau: tableau, theta: theta, key: key).Bind(weights => {
            int order = DenseOrder(tableau: tableau);
            (bool failed, double maxResidual) = MomentResidual(tableau: tableau, weights: weights, theta: theta, order: order);
            double endpoint = theta <= ButcherTableau.CoefficientTolerance
                ? weights.Fold(initialState: 0.0, f: static (max, value) => Math.Max(val1: max, val2: Math.Abs(value: value)))
                : 1.0 - theta <= ButcherTableau.CoefficientTolerance
                    ? weights.Zip(tableau.Weights).Fold(initialState: 0.0, f: static (max, pair) => Math.Max(val1: max, val2: Math.Abs(value: pair.First - pair.Second)))
                    : 0.0;
            DenseOutputReceipt receipt = new(
                StageCount: tableau.StageCount,
                MethodOrder: tableau.MethodOrder,
                DenseOrder: order,
                CheckedThetaCount: 1,
                CheckedConditionCount: order + ((theta <= ButcherTableau.CoefficientTolerance || 1.0 - theta <= ButcherTableau.CoefficientTolerance) ? tableau.StageCount : 0),
                FailedConditionCount: (failed ? 1 : 0) + (endpoint <= ButcherTableau.CoefficientTolerance ? 0 : 1),
                MaxResidual: Math.Max(val1: maxResidual, val2: endpoint),
                UsesStageDerivatives: true);
            return receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<DenseOutputReceipt>(key.InvalidResult());
        });
    private static Fin<Seq<double>> Weights(ButcherTableau tableau, double theta, Op key) {
        if (theta <= ButcherTableau.CoefficientTolerance) return Fin.Succ(toSeq(Enumerable.Repeat(element: 0.0, count: tableau.StageCount)));
        if (1.0 - theta <= ButcherTableau.CoefficientTolerance) return Fin.Succ(tableau.Weights);
        double endpointScale = theta * (1.0 - theta);
        double[] baseWeights = [.. tableau.Weights.AsIterable().Select(weight => theta * weight)];
        return Correction(tableau: tableau, theta: theta, order: DenseOrder(tableau: tableau), endpointScale: endpointScale, key: key)
            .Map(correction => toSeq(baseWeights.Zip(correction).Select(pair => pair.First + (endpointScale * pair.Second))));
    }
    private static Fin<double[]> Correction(ButcherTableau tableau, double theta, int order, double endpointScale, Op key) {
        double[] a = [.. Enumerable.Range(start: 0, count: order * tableau.StageCount).Select(index => Math.Pow(x: tableau.Abscissae[index % tableau.StageCount], y: index / tableau.StageCount))];
        double[] rhs = [.. Enumerable.Range(start: 0, count: order).Select(m => (Math.Pow(x: theta, y: m + 1) - theta) / ((m + 1.0) * endpointScale))];
        double[] gram = new double[order * order];
        for (int row = 0; row < order; row++)
            for (int col = 0; col < order; col++) {
                double value = 0.0;
                for (int stage = 0; stage < tableau.StageCount; stage++) value += a[(row * tableau.StageCount) + stage] * a[(col * tableau.StageCount) + stage];
                gram[(row * order) + col] = value;
            }
        return Matrix.Of(rows: Dimension.Create(value: order), cols: Dimension.Create(value: order), entries: new Arr<double>(gram), key: key)
            .Bind(matrix => matrix.SolveDetailed(rhs: new Arr<double>(rhs), key: key))
            .Map(solved => Enumerable.Range(start: 0, count: tableau.StageCount)
                .Select(stage => Enumerable.Range(start: 0, count: order).Sum(row => a[(row * tableau.StageCount) + stage] * solved.Solution[row]))
                .ToArray());
    }
    private static (bool Failed, double Max) MomentResidual(ButcherTableau tableau, Seq<double> weights, double theta, int order) =>
        Enumerable.Range(start: 0, count: order)
            .Select(m => Math.Abs(value: weights.Zip(tableau.Abscissae).Fold(initialState: 0.0, f: (sum, pair) => sum + (pair.First * Math.Pow(x: pair.Second, y: m))) - (Math.Pow(x: theta, y: m + 1) / (m + 1.0))))
            .Aggregate(seed: (Failed: false, Max: 0.0), func: static (state, residual) => (
                Failed: state.Failed || !RhinoMath.IsValidDouble(x: residual) || residual > ButcherTableau.CoefficientTolerance,
                Max: Math.Max(val1: state.Max, val2: residual)));
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct DenseOutputState(Point3d Start, Point3d End, double Step, Seq<Vector3d> Stages, ButcherTableau Tableau, DenseOutputReceipt Receipt) {
    internal bool IsValid =>
        Start.IsValid
        && End.IsValid
        && RhinoMath.IsValidDouble(x: Step)
        && Math.Abs(value: Step) > RhinoMath.ZeroTolerance
        && Stages.Count == Tableau.StageCount
        && Stages.ForAll(static vector => vector.IsValid)
        && Tableau.IsValid
        && Receipt.IsValid;
    internal static Fin<DenseOutputState> Of(Point3d start, Point3d end, double step, Seq<Vector3d> stages, ButcherTableau tableau, Op key) =>
        tableau.DenseOutputReceipt(key: key).Bind(receipt =>
            tableau.DenseWeightsAt(theta: 1.0, key: key).Bind(weights => {
                Point3d endpoint = start + (step * Combine(coefficients: weights, vectors: stages));
                if (endpoint.DistanceTo(other: end) > RhinoMath.SqrtEpsilon) return Fin.Fail<DenseOutputState>(key.InvalidResult());
                DenseOutputState state = new(Start: start, End: end, Step: step, Stages: stages, Tableau: tableau, Receipt: receipt);
                return state.IsValid ? Fin.Succ(state) : Fin.Fail<DenseOutputState>(key.InvalidResult());
            }));
    internal Fin<Point3d> PointAt(double theta, Op key) {
        if (!RhinoMath.IsValidDouble(x: theta) || theta is < 0.0 or > 1.0 || !IsValid) return Fin.Fail<Point3d>(key.InvalidInput());
        Point3d start = Start;
        double step = Step;
        Seq<Vector3d> stages = Stages;
        ButcherTableau tableau = Tableau;
        return tableau.DenseWeightsAt(theta: theta, key: key)
            .Bind(weights => key.AcceptValue(value: start + (step * Combine(coefficients: weights, vectors: stages))));
    }
    private static Vector3d Combine(Seq<double> coefficients, Seq<Vector3d> vectors) =>
        coefficients.Zip(vectors).Fold(
            initialState: Vector3d.Zero,
            f: static (sum, pair) => sum + (pair.First * pair.Second));
}

[Union]
internal abstract partial record StreamlineStep {
    public sealed record AcceptedCase(Point3d Next, double SuggestedStep, Option<double> Error, DenseOutputState Dense) : StreamlineStep;
    public sealed record RejectedCase(double SuggestedStep, Option<double> Error) : StreamlineStep;
    private StreamlineStep() { }
}

internal readonly record struct StreamlineState(Seq<Point3d> Trail, Point3d Current, double H, double Arc, int Steps, int Rejects, int RejectedSteps, double MinStep, double MaxStep, Option<double> LastError, double MaxError, Option<DenseOutputState> Dense, Option<TraceEvent> Event, Option<StreamlineStopKind> Stop) {
    internal static StreamlineState Start(Point3d seed, double h) =>
        new(Trail: Seq(seed), Current: seed, H: h, Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0, MinStep: h, MaxStep: h, LastError: Option<double>.None, MaxError: 0.0, Dense: Option<DenseOutputState>.None, Event: Option<TraceEvent>.None, Stop: Option<StreamlineStopKind>.None);
    internal StreamlineState Accept(StreamlineStep.AcceptedCase accepted) =>
        Step(suggested: accepted.SuggestedStep, error: accepted.Error) with {
            Trail = Trail.Add(accepted.Next),
            Current = accepted.Next,
            Arc = Arc + accepted.Next.DistanceTo(other: Current),
            Steps = Steps + 1,
            Rejects = 0,
            Dense = Some(accepted.Dense),
        };
    internal StreamlineState Reject(StreamlineStep.RejectedCase rejected, int rejectBudget) =>
        Step(suggested: rejected.SuggestedStep, error: rejected.Error) with {
            Rejects = Rejects + 1,
            RejectedSteps = RejectedSteps + 1,
            Dense = Option<DenseOutputState>.None,
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

internal static class FlowKernel {
    private const int MaxIterations = 100000;

    internal static Fin<TOut> Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        from activeIntegrator in FieldIntegrator.Admit(value: integrator, key: key)
        from activeTermination in Termination.Admit(value: termination, key: key)
        from validSeed in key.AcceptValue(value: seed)
        from state in TraceState(source: source, seed: validSeed, initialStep: initialStep, integrator: activeIntegrator, termination: activeTermination, context: context, key: key)
        let trace = ToTrace(state: state, integrator: activeIntegrator)
        from output in ProjectTrace<TOut>(trace: trace, key: key)
        select output;
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
    private static Fin<StreamlineState> TraceState(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key) {
        Fin<StreamlineState> state = Fin.Succ(StreamlineState.Start(seed: seed, h: initialStep.Value));
        for (int i = 0; i < MaxIterations && state.Match(Succ: static active => active.Stop.IsNone, Fail: static _ => false); i++)
            state = state.Bind(active => AdvanceState(state: active, source: source, integrator: integrator, termination: termination, context: context, key: key));
        return state;
    }
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
