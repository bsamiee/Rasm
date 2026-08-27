# 1. Separate curve addresses from unrelated location modes
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L45-L74**
```csharp
[Union]
public abstract partial record Locator {
    private Locator() { }
    public sealed record CurveParameter(double T) : Locator;
    public sealed record ArcLength(double Distance) : Locator;
    public sealed record NormalizedLength(double S) : Locator;
    public sealed record SurfaceParameter(Point2d Uv) : Locator;
    public sealed record ClosestTo(Point3d Probe) : Locator;
    public sealed record PerpendicularParameters(Seq<double> Ts) : Locator;

    public static Locator NormalizedMid => new NormalizedLength(S: 0.5);

    internal Requirement CurveRequirement => Switch(
        curveParameter: static _ => Requirement.Basic,
        arcLength: static _ => Requirement.CurveLength,
        normalizedLength: static _ => Requirement.CurveLength,
        surfaceParameter: static _ => Requirement.Basic,
        closestTo: static _ => Requirement.Basic,
        perpendicularParameters: static _ => Requirement.CurveLength);

    internal Fin<double> ResolveParameter(Curve curve, Context context, Op key) => Switch(
        state: (Curve: curve, Context: context, Key: key),
        curveParameter: static (s, a) => guard(s.Curve.Domain.IncludesParameter(t: a.T), s.Key.InvalidInput()).ToFin().Map(_ => a.T),
        arcLength: static (s, a) => guard(s.Curve.LengthParameter(segmentLength: a.Distance, t: out double t, fractionalTolerance: s.Context.Fractional), s.Key.InvalidResult()).ToFin().Map(_ => t),
        normalizedLength: static (s, a) => guard(double.IsFinite(a.S) && a.S is >= 0.0 and <= 1.0, s.Key.InvalidInput()).ToFin()
            .Bind(_ => guard(s.Curve.NormalizedLengthParameter(s: a.S, t: out double t, fractionalTolerance: s.Context.Fractional), s.Key.InvalidResult()).ToFin().Map(_ => t)),
        surfaceParameter: static (s, _) => Fin.Fail<double>(s.Key.InvalidInput()),
        closestTo: static (s, _) => Fin.Fail<double>(s.Key.InvalidInput()),
        perpendicularParameters: static (s, _) => Fin.Fail<double>(s.Key.InvalidInput()));
}
```
**To**
```csharp
[Union]
public abstract partial record CurveAddress {
    private CurveAddress() { }
    public sealed record Parameter(double Value) : CurveAddress;
    public sealed record Length(double Value) : CurveAddress;
    public sealed record Normalized(UnitInterval Value) : CurveAddress;
    public sealed record Samples(Dimension Count) : CurveAddress;

    internal Requirement Requirement => Map(
        parameter: static _ => Requirement.Basic,
        length: static _ => Requirement.CurveLength,
        normalized: static _ => Requirement.CurveLength,
        samples: static _ => Requirement.CurveLength);

    internal Fin<Seq<double>> Resolve(Curve curve, Context context, Op key) => Switch(
        state: (Curve: curve, Context: context, Key: key),
        parameter: static (s, at) => guard(s.Curve.Domain.IncludesParameter(at.Value), s.Key.InvalidInput()).ToFin().Map(_ => Seq(at.Value)),
        length: static (s, at) => guard(double.IsFinite(at.Value) && at.Value >= 0.0, s.Key.InvalidInput()).ToFin()
            >> guard(s.Curve.LengthParameter(at.Value, out double t, s.Context.Fractional), s.Key.InvalidResult()).ToFin().Map(_ => Seq(t)),
        normalized: static (s, at) => guard(s.Curve.NormalizedLengthParameter(at.Value.Value, out double t, s.Context.Fractional), s.Key.InvalidResult()).ToFin().Map(_ => Seq(t)),
        samples: static (s, at) => Evaluation.CurveSampleParameters(s.Curve, at.Count.Value, s.Context, s.Key));
}
```
**Why**
Surface UVs, proximity probes, and history-dependent perpendicular-frame batches are operations, not curve addresses. Their presence forces impossible failure arms into the curve resolver. `UnitInterval` and `Dimension` already own normalized-position and positive-count admission, while one sequence shape covers a single station and a sampled station set.
**Change**
Retain only curve-station cases, delete the midpoint forwarding property, and keep curve-domain and nonnegative-length admission at the resolver where the live curve is available.
**Ripples**
Register `Curve.LengthParameter(double, out double, double) -> bool`, `Curve.NormalizedLengthParameter(double, out double, double) -> bool`, and `Curve.NormalizedLengthParameters(double[], double, double) -> double[]` in `libs/dotnet/Rasm/.api/api-rhino.md`. `libs/dotnet/Rasm/.planning/Domain/evaluation.md:376-378` remains the sampled-station owner. Repository search resolves no `Locator` consumer outside this target.
**Delta**
LOC -4; types -2; members -1

# 2. Delete the duplicated location-value matrix
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L76-L186**
```csharp
[Union]
public abstract partial record LocationValue {
    private LocationValue() { }
    public sealed record PointCase : LocationValue {
        internal override Op Key => LocationKeys.PointAt;
        internal override Option<SupportProjection> Closest => Some(SupportProjection.Closest);
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, Point3d>(key: LocationKeys.PointAt, locator: locator, project: static (key, curve, t, _) => key.Accept(value: curve.PointAt(t: t)));
        internal override Operation<TGeometry, TOut> OnSurface<TGeometry, TOut>(Point2d uv) =>
            Locate.Surface<TGeometry, TOut, Point3d>(key: LocationKeys.PointAt, uv: uv, project: static (key, surface, p) => key.Accept(value: surface.PointAt(u: p.X, v: p.Y)));
    }
    public sealed record FrameCase : LocationValue {
        internal override Op Key => LocationKeys.FrameAt;
        internal override Option<SupportProjection> Closest => Some(SupportProjection.Frame);
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, Plane>(key: LocationKeys.FrameAt, locator: locator, project: static (key, curve, t, context) =>
                CurveProjection.Frame.Project<Plane>(curve: curve, parameter: t, context: context, key: key).Bind(plane => key.Accept(value: plane)));
        internal override Operation<TGeometry, TOut> OnSurface<TGeometry, TOut>(Point2d uv) =>
            Locate.Surface<TGeometry, TOut, Plane>(key: LocationKeys.FrameAt, uv: uv, project: static (key, surface, p) =>
                Evaluation.FrameAt(surface: surface, uv: p, key: key).Bind(frame => key.Accept(value: frame)));
        internal override Operation<TGeometry, TOut> OnPerpendicular<TGeometry, TOut>(Seq<double> parameters) =>
            Locate.Perpendicular<TGeometry, TOut>(key: LocationKeys.PerpendicularFrameAt, parameters: parameters);
    }
    public sealed record NormalCase : LocationValue {
        internal override Op Key => LocationKeys.NormalAt;
        internal override Option<SupportProjection> Closest => Some(SupportProjection.Normal);
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, Vector3d>(key: LocationKeys.NormalAt, locator: locator, project: static (key, curve, t, context) =>
                CurveProjection.FrameNormal.Project<Vector3d>(curve: curve, parameter: t, context: context, key: key).Bind(normal => key.Accept(value: normal)));
        internal override Operation<TGeometry, TOut> OnSurface<TGeometry, TOut>(Point2d uv) =>
            Locate.Surface<TGeometry, TOut, Vector3d>(key: LocationKeys.NormalAt, uv: uv, project: static (key, surface, p) =>
                Evaluation.NormalAt(surface: surface, uv: p, key: key).Bind(normal => key.Accept(value: normal)));
    }
    public sealed record TangentCase : LocationValue {
        internal override Op Key => LocationKeys.TangentAt;
        internal override Option<SupportProjection> Closest => Some(SupportProjection.Tangent);
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, Vector3d>(key: LocationKeys.TangentAt, locator: locator, project: static (key, curve, t, context) =>
                CurveProjection.Tangent.Project<Vector3d>(curve: curve, parameter: t, context: context, key: key).Bind(tangent => key.Accept(value: tangent)));
    }
    public sealed record CurvatureCase : LocationValue {
        internal override Op Key => LocationKeys.CurvatureAt;
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, Vector3d>(key: LocationKeys.CurvatureAt, locator: locator, project: static (key, curve, t, context) =>
                CurveProjection.Curvature.Project<Vector3d>(curve: curve, parameter: t, context: context, key: key).Bind(curvature => key.Accept(value: curvature)));
        internal override Operation<TGeometry, TOut> OnSurface<TGeometry, TOut>(Point2d uv) =>
            Locate.Surface<TGeometry, TOut, SurfaceCurvature>(key: LocationKeys.CurvatureAt, uv: uv, project: static (key, surface, p) =>
                Optional(surface.CurvatureAt(u: p.X, v: p.Y)).ToFin(key.InvalidResult())
                    .Bind(bundle => bundle.IsSet ? Fin.Succ(Seq(bundle)) : new Lease<SurfaceCurvature>.Owned(Value: bundle).Use(_ => Fin.Fail<Seq<SurfaceCurvature>>(key.InvalidResult()))));
    }
    public sealed record DerivativeCase(Dimension Order) : LocationValue {
        internal override Op Key => LocationKeys.DerivativeAt;
        internal int JetOffset => ((Order.Value - 1) * (Order.Value + 2)) / 2;
        internal int JetWidth => Order.Value + 1;
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, Vector3d>(key: LocationKeys.DerivativeAt, locator: locator, project: (key, curve, t, _) =>
                Optional(curve.DerivativeAt(t: t, derivativeCount: Order.Value)).Filter(derivatives => Order.Value < derivatives.Length)
                    .ToFin(key.InvalidResult()).Bind(derivatives => key.Accept(value: derivatives[Order.Value])));
        internal override Operation<TGeometry, TOut> OnSurface<TGeometry, TOut>(Point2d uv) =>
            Locate.Surface<TGeometry, TOut, Vector3d>(key: LocationKeys.DerivativeAt, uv: uv, project: (key, surface, p) =>
                surface.Evaluate(u: p.X, v: p.Y, numberDerivatives: Order.Value, point: out Point3d _, derivatives: out Vector3d[] derivatives)
                && derivatives.Length >= JetOffset + JetWidth
                    ? Fin.Succ(toSeq(derivatives.Skip(count: JetOffset).Take(count: JetWidth)))
                    : Fin.Fail<Seq<Vector3d>>(key.InvalidResult()));
    }
    public sealed record ParameterCase : LocationValue {
        internal override Op Key => LocationKeys.ParameterAt;
        internal override Option<SupportProjection> Closest => Some(SupportProjection.Parameter);
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, double>(key: LocationKeys.ParameterAt, locator: locator, project: static (key, _, t, _) => key.Accept(value: t));
    }
    public sealed record LengthCase : LocationValue {
        internal override Op Key => LocationKeys.LengthAt;
        internal override Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) =>
            Locate.Curve<TGeometry, TOut, double>(key: LocationKeys.LengthAt, locator: locator, requirement: Some(Requirement.CurveLength), project: static (key, curve, t, context) =>
                curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(t0: curve.Domain.T0, t1: t)) switch {
                    double length when RhinoMath.IsValidDouble(x: length) && length >= 0.0 => key.Accept(value: length),
                    _ => Fin.Fail<Seq<double>>(key.InvalidResult()),
                });
    }

    public static readonly LocationValue Point = new PointCase();
    public static readonly LocationValue Frame = new FrameCase();
    public static readonly LocationValue Normal = new NormalCase();
    public static readonly LocationValue Tangent = new TangentCase();
    public static readonly LocationValue Curvature = new CurvatureCase();
    public static readonly LocationValue Parameter = new ParameterCase();
    public static readonly LocationValue Length = new LengthCase();
    public static LocationValue Derivative(Dimension order) => new DerivativeCase(Order: order);

    internal abstract Op Key { get; }
    internal virtual Option<SupportProjection> Closest => None;
    internal virtual Operation<TGeometry, TOut> OnCurve<TGeometry, TOut>(Locator locator) where TGeometry : notnull => Key.Unsupported<TGeometry, TOut>();
    internal virtual Operation<TGeometry, TOut> OnSurface<TGeometry, TOut>(Point2d uv) where TGeometry : notnull => Key.Unsupported<TGeometry, TOut>();
    internal virtual Operation<TGeometry, TOut> OnPerpendicular<TGeometry, TOut>(Seq<double> parameters) where TGeometry : notnull => Key.Unsupported<TGeometry, TOut>();
    internal Operation<TGeometry, TOut> Resolve<TGeometry, TOut>(Locator locator) where TGeometry : notnull => locator switch {
        Locator.SurfaceParameter sp => OnSurface<TGeometry, TOut>(uv: sp.Uv),
        Locator.ClosestTo ct => Closest.Match(
            Some: projection => Locate.Closest<TGeometry, TOut>(key: Key, target: ct.Probe, projection: projection),
            None: () => Key.Unsupported<TGeometry, TOut>()),
        Locator.PerpendicularParameters ps => OnPerpendicular<TGeometry, TOut>(parameters: ps.Ts),
        _ => OnCurve<TGeometry, TOut>(locator: locator),
    };
}
```
**To**
```csharp
// LocationValue DELETED
```
**Why**
The union mirrors three established selector vocabularies, adds eight forwarding cases, repeats output admission, and embeds dynamic derivative and disposable-curvature policy beside their operation owner. Its successful surface-curvature arm leaks the owned `IDisposable` bundle.
**Change**
Carry `CurveProjection`, `SurfaceProjection`, and `SupportProjection` directly on `Location`; keep dynamic derivatives as direct operations because order is request data; replace raw surface-curvature output with lease-scoped surface selectors or the scalar sweep.
**Ripples**
Add `Point` and `Parameter` rows to `libs/dotnet/Rasm/.planning/Parametric/projections.md` `CurveProjection`; its existing `Frame`, `FrameNormal`, `Tangent`, `Curvature`, and `ArcLength` rows absorb the other curve reads. Existing `SurfaceProjection` and `SupportProjection` rows absorb the surface and closest reads. Repository search resolves no external `LocationValue` consumer.
**Delta**
LOC -110; types -9; members -38

# 3. Make division cases carry admitted policy and lower themselves
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L188-L218**
```csharp
[Union]
public abstract partial record Division {
    private Division() { }
    public sealed record ByCount(int Count) : Division;
    public sealed record ByLength(double Length) : Division;
    public sealed record ByChord(double Distance) : Division;
    public sealed record AsContour(Point3d Start, Point3d End, double Interval) : Division;
    internal Fin<Unit> Admit(Context context, Op key) => Switch(
        state: (Context: context, Key: key),
        byCount: static (s, c) => guard(c.Count > 0, s.Key.InvalidInput()).ToFin(),
        byLength: static (s, l) => Spacing(value: l.Length, band: s.Context.For(lane: ToleranceLane.Length), key: s.Key),
        byChord: static (s, c) => Spacing(value: c.Distance, band: s.Context.For(lane: ToleranceLane.Chord), key: s.Key),
        asContour: static (s, a) => Spacing(value: a.Interval, band: s.Context.For(lane: ToleranceLane.Length), key: s.Key)
            .Bind(_ => guard(ValidityClaim.Finite(value: a.Start).Holds && ValidityClaim.Finite(value: a.End).Holds
                && a.Start.DistanceTo(other: a.End) > s.Context.For(lane: ToleranceLane.Length).Value, s.Key.InvalidInput()).ToFin()));

    static Fin<Unit> Spacing(double value, Tolerance band, Op key) =>
        guard(double.IsFinite(value) && value > band.Value, key.InvalidInput()).ToFin();

    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        byCount: c => Locate.Divide<TGeometry, TOut>(key: LocationKeys.Divide, division: c, requirement: None,
            divide: curve => curve.DivideByCount(segmentCount: c.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None }),
        byLength: l => Locate.Divide<TGeometry, TOut>(key: LocationKeys.Divide, division: l, requirement: Some(Requirement.CurveLength),
            divide: curve => curve.DivideByLength(segmentLength: l.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None }),
        byChord: c => Locate.Divide<TGeometry, TOut>(key: LocationKeys.Divide, division: c, requirement: Some(Requirement.CurveLength),
            divide: curve => curve.DivideEquidistant(distance: c.Distance, curveParameters: out double[] _) switch { Point3d[] points => Optional(points), _ => Option<Point3d[]>.None }),
        asContour: a => Locate.Divide<TGeometry, TOut>(key: LocationKeys.Divide, division: a, requirement: None,
            divide: curve => Optional(curve.DivideAsContour(contourStart: a.Start, contourEnd: a.End, interval: a.Interval)).Filter(static points => points.Length > 0)));
}
```
**To**
```csharp
[Union]
public abstract partial record Division {
    private Division() { }
    public sealed record Count(Dimension Value) : Division;
    public sealed record Length(PositiveMagnitude Value) : Division;
    public sealed record Chord(PositiveMagnitude Value) : Division;
    public sealed record Contour(Point3d Start, Point3d End, PositiveMagnitude Interval) : Division;

    internal Requirement Requirement => Map(
        count: static _ => Requirement.Basic,
        length: static _ => Requirement.CurveLength,
        chord: static _ => Requirement.CurveLength,
        contour: static _ => Requirement.Basic);

    internal Fin<Seq<Point3d>> Apply(Curve curve, Context context, Op key) => Switch(
        state: (Curve: curve, Context: context, Key: key),
        count: static (s, row) => Optional(s.Curve.DivideByCount(row.Value.Value, true, out Point3d[] points) is double[] ? points : null).ToFin(s.Key.InvalidResult()).Map(static values => toSeq(values)),
        length: static (s, row) =>
            from _ in Above(row.Value.Value, s.Context.For(ToleranceLane.Length), s.Key)
            from points in Optional(s.Curve.DivideByLength(row.Value.Value, true, out Point3d[] values) is double[] ? values : null).ToFin(s.Key.InvalidResult())
            select toSeq(points),
        chord: static (s, row) =>
            from _ in Above(row.Value.Value, s.Context.For(ToleranceLane.Chord), s.Key)
            from points in Optional(s.Curve.DivideEquidistant(row.Value.Value, out double[] _)).ToFin(s.Key.InvalidResult())
            select toSeq(points),
        contour: static (s, row) =>
            from start in s.Key.AcceptInput(row.Start)
            from end in s.Key.AcceptInput(row.End)
            from _ in Above(row.Interval.Value, s.Context.For(ToleranceLane.Length), s.Key)
            from __ in guard(start.DistanceTo(end) > s.Context.For(ToleranceLane.Length).Value, s.Key.InvalidInput()).ToFin()
            from points in Optional(s.Curve.DivideAsContour(start, end, row.Interval.Value)).Filter(static values => values.Length > 0).ToFin(s.Key.InvalidResult())
            select toSeq(points));

    private static Fin<Unit> Above(double value, Tolerance band, Op key) => guard(value > band.Value, key.InvalidInput()).ToFin();
}
```
**Why**
The raw primitives repeat admission on every execution, while the union transports one-use delegates through a second operation layer. `Dimension` and `PositiveMagnitude` already own primitive validity; only model-dependent spacing and contour-axis gates remain here.
**Change**
Carry generated scalar owners, derive readiness from the case, and lower each host call inside the generated fold. Keep one contextual band gate and let `Locate.Divide` own normalization and output admission.
**Ripples**
Register `Curve.DivideAsContour(Point3d, Point3d, double) -> Point3d[]` in `libs/dotnet/Rasm/.api/api-rhino.md`. Repository search resolves no external `Division` consumer.
**Delta**
LOC +8; types 0; members 0

# 4. Replace curvature modality with one typed output algebra
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L220-L276**
```csharp
[Union]
public abstract partial record CurvatureMode {
    private CurvatureMode() { }
    public sealed record VectorCase : CurvatureMode {
        internal override Option<Func<Op, Curve, int, Context, Fin<Seq<TOut>>>> OnCurve<TOut>(CurvatureAggregation aggregation) =>
            typeof(TOut) == typeof(Vector3d) && aggregation is CurvatureAggregation.SamplesCase
                ? Some<Func<Op, Curve, int, Context, Fin<Seq<TOut>>>>(static (op, curve, n, ctx) => Locate.CurveCurvatures(key: op, curve: curve, count: n, context: ctx).Bind(values => op.AcceptResults<Vector3d, TOut>(values: values)))
                : Locate.CurveLane<TOut>(aggregation: aggregation, metric: ScalarMetric.Magnitude);
        internal override Option<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>> OnSurface<TOut>(CurvatureAggregation aggregation) =>
            typeof(TOut) == typeof(SurfaceCurvature) && aggregation is CurvatureAggregation.SamplesCase
                ? Some<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>>(static (op, surface, n, ctx) => Locate.SurfaceBundles(key: op, surface: surface, count: n, context: ctx).Bind(values => op.AcceptResults<SurfaceCurvature, TOut>(values: values)))
                : Locate.SurfaceStatLane<TOut>(aggregation: aggregation, metrics: SurfaceMetrics);
    }
    public sealed record ScalarCase(ScalarMetric Metric) : CurvatureMode {
        internal override Option<Func<Op, Curve, int, Context, Fin<Seq<TOut>>>> OnCurve<TOut>(CurvatureAggregation aggregation) => IsCurveMagnitude ? Locate.CurveLane<TOut>(aggregation: aggregation, metric: Metric) : None;
        internal override Option<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>> OnSurface<TOut>(CurvatureAggregation aggregation) => SurfaceMetrics.IsEmpty ? None : Locate.SurfaceLane<TOut>(aggregation: aggregation, metric: Metric);
    }
    public static CurvatureMode Vector => new VectorCase();
    public static CurvatureMode Scalar(ScalarMetric metric) => new ScalarCase(Metric: metric);
    internal bool IsCurveMagnitude => Switch(vectorCase: static _ => true, scalarCase: static scalar => scalar.Metric.Equals(ScalarMetric.Magnitude));
    internal Seq<ScalarMetric> SurfaceMetrics => Switch(
        vectorCase: static _ => Seq(ScalarMetric.Gaussian, ScalarMetric.Mean),
        scalarCase: static scalar => scalar.Metric.Equals(ScalarMetric.Gaussian) || scalar.Metric.Equals(ScalarMetric.Mean) ? Seq(scalar.Metric) : Seq<ScalarMetric>());
    internal abstract Option<Func<Op, Curve, int, Context, Fin<Seq<TOut>>>> OnCurve<TOut>(CurvatureAggregation aggregation);
    internal abstract Option<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>> OnSurface<TOut>(CurvatureAggregation aggregation);
}

[Union]
public abstract partial record CurvatureAggregation {
    private CurvatureAggregation() { }
    public sealed record SamplesCase : CurvatureAggregation;
    public sealed record ExtremaCase(ExtremumDirection Direction, ToleranceLane Band) : CurvatureAggregation;
    public static readonly CurvatureAggregation Samples = new SamplesCase();
    public static CurvatureAggregation Extrema(ExtremumDirection direction, Option<ToleranceLane> band = default) => new ExtremaCase(Direction: direction, Band: band.IfNone(noneValue: ToleranceLane.Neglect));
    internal Op Key => Switch(samplesCase: static _ => LocationKeys.Curvature, extremaCase: static _ => LocationKeys.CurvatureExtrema);
    internal Option<Func<Op, Seq<Locate.CurvatureSample>, Context, Fin<Seq<TOut>>>> Reduce<TOut>(ScalarMetric metric) => Switch(
        samplesCase: _ => Locate.SampleColumn<TOut>(metric: metric),
        extremaCase: extrema => Locate.SampleColumn<TOut>(metric: metric).Map(column =>
            (Func<Op, Seq<Locate.CurvatureSample>, Context, Fin<Seq<TOut>>>)((key, samples, context) => column(key,
                Stat.Extrema(items: samples, projection: static sample => sample.Curvature, band: context.For(lane: extrema.Band), direction: extrema.Direction), context))));
}
```
**To**
```csharp
public readonly record struct CurvatureSample(Point3d Point, double Value) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Finite(Point), ValidityClaim.Finite(Value));
}

[Union]
public abstract partial record CurvatureOutput {
    private CurvatureOutput() { }
    public sealed record Samples : CurvatureOutput;
    public sealed record Summary : CurvatureOutput;
    public sealed record Extrema(ExtremumDirection Direction, ToleranceLane Band) : CurvatureOutput;

    internal bool Accepts(Type output) => Map(
        state: output,
        samples: static (type, _) => type == typeof(CurvatureSample),
        summary: static (type, _) => type == typeof(Stat<Scalar>),
        extrema: static (type, _) => type == typeof(CurvatureSample));

    internal Fin<Seq<TOut>> Reduce<TOut>(Seq<CurvatureSample> samples, ScalarMetric metric, Context context, Op key) => Switch(
        state: (Samples: samples, Metric: metric, Context: context, Key: key),
        samples: static (s, _) => s.Key.AcceptResults<CurvatureSample, TOut>(s.Samples),
        summary: static (s, _) => Stat<Scalar>.Of(values: s.Samples.Map(static sample => (Scalar)sample.Value), key: s.Key, context: Some((StatContext)s.Metric))
            .Bind(stat => s.Key.AcceptResults<Stat<Scalar>, TOut>(Seq(stat))),
        extrema: static (s, row) => s.Key.AcceptResults<CurvatureSample, TOut>(Stat.Extrema(
            items: s.Samples, projection: static sample => sample.Value, band: s.Context.For(row.Band), direction: row.Direction)));
}
```
**Why**
`CurvatureMode` conflates raw selector output, scalar metric compatibility, reduction, and projection. It leaks disposable surface bundles, silently emits two surface metrics, and rejects legitimate negative Gaussian and mean curvature.
**Change**
Carry `ScalarMetric` directly. Samples and extrema return one point-plus-value carrier; summary explicitly returns one `Stat<Scalar>`. Raw curve vectors remain `CurveAddress.Samples` plus `CurveProjection.Curvature`; surface metrics become explicit Gaussian or mean reads.
**Ripples**
`libs/dotnet/Rasm/.planning/Domain/stats.md:38-50` remains the sole metric compatibility table and its order-statistics cluster remains the extremum owner.
**Delta**
LOC -27; types -1; members -8

# 5. Expose the actual operation family and consume the caller key
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L278-L326**
```csharp
[Union]
public abstract partial record Location {
    private Location() { }
    public sealed record AtCase(Locator Locator, LocationValue Value) : Location;
    public sealed record CurvatureCase(int Count, CurvatureMode Mode, CurvatureAggregation Aggregation) : Location;
    public sealed record DivideCase(Division By) : Location;
    public sealed record OrientationCase(Plane Plane) : Location;
    public sealed record ContainsCase(Point3d Probe, Plane Frame) : Location;
    public sealed record ShortPathCase(Point2d Start, Point2d End) : Location;

    public static Location At(Locator at, LocationValue value) => new AtCase(Locator: at, Value: value);
    public static Location Curvature(int count, CurvatureMode mode) => new CurvatureCase(Count: count, Mode: mode, Aggregation: CurvatureAggregation.Samples);
    public static Location CurvatureExtrema(int count, CurvatureMode mode, ExtremumDirection direction, Option<ToleranceLane> band = default) =>
        new CurvatureCase(Count: count, Mode: mode, Aggregation: CurvatureAggregation.Extrema(direction: direction, band: band));
    public static Location DivideByCount(int count) => new DivideCase(By: new Division.ByCount(Count: count));
    public static Location DivideByLength(double length) => new DivideCase(By: new Division.ByLength(Length: length));
    public static Location DivideByChord(double distance) => new DivideCase(By: new Division.ByChord(Distance: distance));
    public static Location DivideAsContour(Point3d start, Point3d end, double interval) => new DivideCase(By: new Division.AsContour(Start: start, End: end, Interval: interval));
    public static Location Orientation(Plane plane) => new OrientationCase(Plane: plane);
    public static Location Contains(Point3d point, Plane plane) => new ContainsCase(Probe: point, Frame: plane);
    public static Location ShortPath(Point2d start, Point2d end) => new ShortPathCase(Start: start, End: end);

    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        atCase: static at => at.Value.Resolve<TGeometry, TOut>(locator: at.Locator),
        curvatureCase: static c => Locate.Curvature<TGeometry, TOut>(count: c.Count, mode: c.Mode, aggregation: c.Aggregation),
        divideCase: static d => d.By.Operation<TGeometry, TOut>(),
        orientationCase: static o => Locate.Orientation<TGeometry, TOut>(frame: o.Plane),
        containsCase: static c => Locate.Contains<TGeometry, TOut>(probe: c.Probe, frame: c.Frame),
        shortPathCase: static sp => Locate.ShortPath<TGeometry, TOut>(start: sp.Start, end: sp.End));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class LocationKeys {
    internal static readonly Op PointAt = Op.Of(name: nameof(PointAt));
    internal static readonly Op FrameAt = Op.Of(name: nameof(FrameAt));
    internal static readonly Op PerpendicularFrameAt = Op.Of(name: nameof(PerpendicularFrameAt));
    internal static readonly Op NormalAt = Op.Of(name: nameof(NormalAt));
    internal static readonly Op TangentAt = Op.Of(name: nameof(TangentAt));
    internal static readonly Op CurvatureAt = Op.Of(name: nameof(CurvatureAt));
    internal static readonly Op DerivativeAt = Op.Of(name: nameof(DerivativeAt));
    internal static readonly Op ParameterAt = Op.Of(name: nameof(ParameterAt));
    internal static readonly Op LengthAt = Op.Of(name: nameof(LengthAt));
    internal static readonly Op Divide = Op.Of(name: nameof(Divide));
    internal static readonly Op Orientation = Op.Of(name: nameof(Orientation));
    internal static readonly Op Contains = Op.Of(name: nameof(Contains));
    internal static readonly Op ShortPath = Op.Of(name: nameof(ShortPath));
    internal static readonly Op Curvature = Op.Of(name: nameof(Curvature));
    internal static readonly Op CurvatureExtrema = Op.Of(name: nameof(CurvatureExtrema));
}
```
**To**
```csharp
[Union]
public abstract partial record Location {
    private Location() { }
    public sealed record CurveAt(CurveAddress Address, CurveProjection Projection) : Location;
    public sealed record SurfaceAt(Point2d Uv, SurfaceProjection Projection) : Location;
    public sealed record Closest(Point3d Probe, SupportProjection Projection) : Location;
    public sealed record PerpendicularFrames(Seq<double> Parameters) : Location;
    public sealed record CurveDerivative(CurveAddress Address, Dimension Order) : Location;
    public sealed record SurfaceDerivative(Point2d Uv, Dimension Order) : Location;
    public sealed record Curvature(Dimension Count, ScalarMetric Metric, CurvatureOutput Output) : Location;
    public sealed record Divide(Division By) : Location;
    public sealed record Orientation(Plane Frame) : Location;
    public sealed record Contains(Point3d Probe, Plane Frame) : Location;
    public sealed record ShortPath(Point2d Start, Point2d End) : Location;

    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>(Op key) where TGeometry : notnull => Switch(
        state: key,
        curveAt: static (op, row) => Locate.Curve<TGeometry, TOut>(row.Address, row.Projection, op),
        surfaceAt: static (op, row) => Locate.Surface<TGeometry, TOut>(row.Uv, row.Projection, op),
        closest: static (op, row) => Locate.Closest<TGeometry, TOut>(row.Probe, row.Projection, op),
        perpendicularFrames: static (op, row) => Locate.Perpendicular<TGeometry, TOut>(row.Parameters, op),
        curveDerivative: static (op, row) => Locate.CurveDerivative<TGeometry, TOut>(row.Address, row.Order, op),
        surfaceDerivative: static (op, row) => Locate.SurfaceDerivative<TGeometry, TOut>(row.Uv, row.Order, op),
        curvature: static (op, row) => Locate.Curvature<TGeometry, TOut>(row.Count, row.Metric, row.Output, op),
        divide: static (op, row) => Locate.Divide<TGeometry, TOut>(row.By, op),
        orientation: static (op, row) => Locate.Orientation<TGeometry, TOut>(row.Frame, op),
        contains: static (op, row) => Locate.Contains<TGeometry, TOut>(row.Probe, row.Frame, op),
        shortPath: static (op, row) => Locate.ShortPath<TGeometry, TOut>(row.Start, row.End, op));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
```
**Why**
`AtCase` hides the real dispatch behind two more unions, while eleven factories only forward into constructors. `LocationKeys` is a second identity plane: `AnalysisQuery.LocationCase.Build` already receives `AnalysisVerb.Location.Op`, then discards it. Generated per-case keys would retain the same duplication under another mechanism.
**Change**
Expose direct operation cases, retain the history-dependent perpendicular-frame batch, delete all factories and private keys, and thread the caller-owned `Op` through the exhaustive switch.
**Ripples**
Change `libs/dotnet/Rasm/.planning/Analysis/query.md:190-192` to `Query.Operation<TGeometry, TOut>(key)`. Repository search resolves no concrete old location-factory call site; `AnalysisQuery.Location(Location)` remains the sole public route.
**Delta**
LOC -12; types +4; members -26

# 6. Collapse family helpers onto selectors and direct operations
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L328-L399**
```csharp
internal static class Locate {
    private static Option<Capability> FamilyOf(Type native) =>
        native == typeof(Curve) ? Some(Capability.CurveForm)
        : native == typeof(Surface) ? Some(Capability.SurfaceForm)
        : Option<Capability>.None;

    private static bool Admits<TGeometry, TOut, TNative, TValue>() =>
        typeof(TOut) == typeof(TValue)
        && FamilyOf(native: typeof(TNative)).Match(
            Some: family => family.Admits(type: typeof(TGeometry)),
            None: () => typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)));

    internal static Operation<TGeometry, TOut> Curve<TGeometry, TOut, TValue>(Op key, Locator locator, Func<Op, Curve, double, Context, Fin<Seq<TValue>>> project, Option<Requirement> requirement = default) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Curve, TValue>()
            ? Operation<TGeometry, TValue>.Build(
                key: key, requirement: Some(requirement.IfNone(noneValue: locator.CurveRequirement)), state: (Key: key, Locator: locator, Project: project),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in Normalization.CurveForm(source: geometry, key: state.Key)
                        .Bind(lease => lease.Use(curve => state.Locator.ResolveParameter(curve: curve, context: context, key: state.Key)
                            .Bind(parameter => state.Project(state.Key, curve, parameter, context)))).ToEff()
                    select result).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Surface<TGeometry, TOut, TValue>(Op key, Point2d uv, Func<Op, Surface, Point2d, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Surface, TValue>()
            ? Operation<TGeometry, TValue>.Build(
                key: key, requirement: Some(Requirement.SurfaceEvaluation), state: (Key: key, Uv: uv, Project: project),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in Normalization.SurfaceForm(source: geometry, key: state.Key)
                        .Bind(lease => lease.Use(surface => Evaluation.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key)
                            .Bind(parameter => state.Project(state.Key, surface, parameter)))).ToEff()
                    select result).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Closest<TGeometry, TOut>(Op key, Point3d target, SupportProjection projection) where TGeometry : notnull =>
        (ValidityClaim.Finite(value: target).Holds, Capability.Closest.Admits(type: typeof(TGeometry))) switch {
            (false, _) => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (true, true) => Operation<TGeometry, TOut>.Build(
                key: key, state: (Key: key, Target: target, Projection: projection),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from space in SupportSpace.Of(value: geometry, key: state.Key).ToEff()
                    from hit in space.Closest(sample: state.Target, key: state.Key).ToEff()
                    from result in state.Projection.Project<TOut>(space: space, hit: hit, sample: state.Target, context: context, key: state.Key).Map(static value => Seq(value)).ToEff()
                    select result),
            _ => key.Unsupported<TGeometry, TOut>(),
        };

    internal static Operation<TGeometry, TOut> Perpendicular<TGeometry, TOut>(Op key, Seq<double> parameters) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Curve, Plane>()
            ? Operation<TGeometry, Plane>.Build(
                key: key, requirement: Some(Requirement.CurveLength), state: (Key: key, Parameters: parameters),
                evaluator: static (state, geometry) => Normalization.CurveForm(source: geometry, key: state.Key)
                    .Bind(lease => lease.Use(curve => Optional(curve.GetPerpendicularFrames(state.Parameters.Distinct().Order()))
                        .ToFin(state.Key.InvalidResult()).Bind(planes => state.Key.Accept(values: planes)))).ToEff()).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Divide<TGeometry, TOut>(Op key, Division division, Option<Requirement> requirement, Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Curve, Point3d>()
            ? Operation<TGeometry, Point3d>.Build(
                key: key, requirement: requirement, requiresContext: true, state: (Key: key, Division: division, Divide: divide),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in state.Division.Admit(context: context, key: state.Key)
                        .Bind(_ => Normalization.CurveForm(source: geometry, key: state.Key))
                        .Bind(lease => lease.Use(curve => state.Divide(arg: curve).ToFin(state.Key.InvalidResult()).Bind(points => state.Key.Accept(values: points)))).ToEff()
                    select result).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
```
**To**
```csharp
internal static class Locate {
    internal static Operation<TGeometry, TOut> Curve<TGeometry, TOut>(CurveAddress address, CurveProjection projection, Op key) where TGeometry : notnull =>
        Capability.CurveForm.Admits(typeof(TGeometry)) && projection.Accepts<TOut>()
            ? Operation<TGeometry, TOut>.Build(key, requirement: Some(address.Requirement), requiresContext: true,
                state: (Address: address, Projection: projection, Key: key),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from values in Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                        state.Address.Resolve(curve, context, state.Key).Bind(parameters => parameters
                            .TraverseM(t => state.Projection.Project<TOut>(curve, t, context, state.Key)).As()))).ToEff()
                    select values)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Surface<TGeometry, TOut>(Point2d uv, SurfaceProjection projection, Op key) where TGeometry : notnull =>
        Capability.SurfaceForm.Admits(typeof(TGeometry)) && projection.Accepts<TOut>()
            ? Operation<TGeometry, TOut>.Build(key, requirement: Some(Requirement.SurfaceEvaluation), requiresContext: true,
                state: (Uv: uv, Projection: projection, Key: key),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from value in Normalization.SurfaceForm(geometry, state.Key).Bind(lease => lease.Use(surface =>
                        state.Projection.Project<TOut>(surface, state.Uv.X, state.Uv.Y, context, state.Key))).ToEff()
                    select Seq(value))
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d probe, SupportProjection projection, Op key) where TGeometry : notnull =>
        (ValidityClaim.Finite(probe).Holds, Capability.Closest.Admits(typeof(TGeometry)), projection.Accepts<TOut>()) switch {
            (false, _, _) => Operation<TGeometry, TOut>.Reject(key, key.InvalidInput()),
            (true, true, true) => Operation<TGeometry, TOut>.Build(key, requiresContext: true,
                state: (Probe: probe, Projection: projection, Key: key),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from space in SupportSpace.Of(geometry, state.Key).ToEff()
                    from hit in space.Closest(state.Probe, state.Key).ToEff()
                    from value in state.Projection.Project<TOut>(space, hit, state.Probe, context, state.Key).ToEff()
                    select Seq(value)),
            _ => key.Unsupported<TGeometry, TOut>(),
        };

    internal static Operation<TGeometry, TOut> Perpendicular<TGeometry, TOut>(Seq<double> parameters, Op key) where TGeometry : notnull =>
        Capability.CurveForm.Admits(typeof(TGeometry))
            ? Operation<TGeometry, Plane>.Build(key, requirement: Some(Requirement.CurveLength), state: (Parameters: parameters, Key: key),
                evaluator: static (state, geometry) => Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                    from ordered in Fin.Succ(toSeq(state.Parameters.Distinct().Order()))
                    from _ in guard(!ordered.IsEmpty && ordered.ForAll(curve.Domain.IncludesParameter), state.Key.InvalidInput()).ToFin()
                    from frames in Optional(curve.GetPerpendicularFrames(ordered)).ToFin(state.Key.InvalidResult())
                    from accepted in state.Key.Accept(values: frames)
                    select accepted)).ToEff()).As<TGeometry, TOut>(key)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> CurveDerivative<TGeometry, TOut>(CurveAddress address, Dimension order, Op key) where TGeometry : notnull =>
        Capability.CurveForm.Admits(typeof(TGeometry))
            ? Operation<TGeometry, Vector3d>.Build(key, requirement: Some(address.Requirement), requiresContext: true,
                state: (Address: address, Order: order, Key: key), evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from values in Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                        state.Address.Resolve(curve, context, state.Key).Bind(parameters => parameters.TraverseM(t =>
                            Optional(curve.DerivativeAt(t, state.Order.Value)).Filter(ds => state.Order.Value < ds.Length).ToFin(state.Key.InvalidResult())
                                .Bind(ds => state.Key.AcceptValue(ds[state.Order.Value]))).As()))).ToEff()
                    select values).As<TGeometry, TOut>(key)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> SurfaceDerivative<TGeometry, TOut>(Point2d uv, Dimension order, Op key) where TGeometry : notnull =>
        Capability.SurfaceForm.Admits(typeof(TGeometry))
            ? Operation<TGeometry, Vector3d>.Build(key, requirement: Some(Requirement.SurfaceEvaluation), requiresContext: true,
                state: (Uv: uv, Order: order, Offset: ((order.Value - 1) * (order.Value + 2)) / 2, Width: order.Value + 1, Key: key),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from values in Normalization.SurfaceForm(geometry, state.Key).Bind(lease => lease.Use(surface =>
                        Evaluation.SurfaceUv(surface, state.Uv, context, state.Key).Bind(at =>
                            surface.Evaluate(at.X, at.Y, state.Order.Value, out Point3d _, out Vector3d[] derivatives)
                            && derivatives.Length >= state.Offset + state.Width
                                ? state.Key.Accept(values: derivatives.Skip(state.Offset).Take(state.Width))
                                : Fin.Fail<Seq<Vector3d>>(state.Key.InvalidResult())))).ToEff()
                    select values).As<TGeometry, TOut>(key)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Divide<TGeometry, TOut>(Division division, Op key) where TGeometry : notnull =>
        Capability.CurveForm.Admits(typeof(TGeometry))
            ? Operation<TGeometry, Point3d>.Build(key, requirement: Some(division.Requirement), requiresContext: true,
                state: (Division: division, Key: key), evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from values in Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                        state.Division.Apply(curve, context, state.Key).Bind(points => state.Key.Accept(values: points)))).ToEff()
                    select values).As<TGeometry, TOut>(key)
            : key.Unsupported<TGeometry, TOut>();
```
**Why**
`FamilyOf`, the four-type `Admits`, and per-call delegates are indirection without ownership. The previous audit also erased `GetPerpendicularFrames`, but that batch minimizes rotation across the ordered sequence and is not equivalent to independent `PerpendicularFrameAt` calls.
**Change**
Derive output support from selector owners, normalize once, traverse the resolved address, retain the batch frame API with sorted in-domain parameters, inline dynamic derivative indexing, and let `Division` lower itself inside one operation.
**Ripples**
In `libs/dotnet/Rasm/.planning/Numerics/atoms.md`, make `ResultProjection` expose the raw-type/output-type compatibility predicate already encoded by `Raw`. In `libs/dotnet/Rasm/.planning/Parametric/projections.md`, store each selector row's raw type and expose `Accepts<TOut>()` through that predicate. In `libs/dotnet/Rasm/.planning/Spatial/support.md`, expose `Accepts<TOut>()` from its existing private `Accepts(Type)` column. Register `Curve.GetPerpendicularFrames(IEnumerable<double>) -> Plane[]` and `Surface.Evaluate(double, double, int, out Point3d, out Vector3d[]) -> bool` in `libs/dotnet/Rasm/.api/api-rhino.md`.
**Delta**
LOC +45; types 0; members -2

# 7. Admit geometric predicates before building their operations
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L401-L437**
```csharp
    internal static Operation<TGeometry, TOut> Orientation<TGeometry, TOut>(Plane frame) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Curve, CurveOrientation>()
            ? Operation<TGeometry, CurveOrientation>.Build(
                key: LocationKeys.Orientation, state: (Key: LocationKeys.Orientation, Frame: frame),
                evaluator: static (state, geometry) => Normalization.CurveForm(source: geometry, key: state.Key)
                    .Bind(lease => lease.Use(curve => state.Key.Accept(value: curve.ClosedCurveOrientation(plane: state.Frame)))).ToEff()).As<TGeometry, TOut>(key: LocationKeys.Orientation)
            : LocationKeys.Orientation.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> Contains<TGeometry, TOut>(Point3d probe, Plane frame) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Curve, PointContainment>()
            ? Operation<TGeometry, PointContainment>.Build(
                key: LocationKeys.Contains, requiresContext: true, state: (Key: LocationKeys.Contains, Probe: probe, Frame: frame),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in Normalization.CurveForm(source: geometry, key: state.Key)
                        .Bind(lease => lease.Use(curve => curve.Contains(testPoint: state.Probe, plane: state.Frame, tolerance: context.Absolute.Value) switch {
                            PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                            PointContainment containment => state.Key.Accept(value: containment),
                        })).ToEff()
                    select result).As<TGeometry, TOut>(key: LocationKeys.Contains)
            : LocationKeys.Contains.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> ShortPath<TGeometry, TOut>(Point2d start, Point2d end) where TGeometry : notnull =>
        Admits<TGeometry, TOut, Surface, Curve>()
            ? Operation<TGeometry, Curve>.Build(
                key: LocationKeys.ShortPath, requirement: Some(Requirement.SurfaceEvaluation), state: (Key: LocationKeys.ShortPath, Start: start, End: end),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in Normalization.SurfaceForm(source: geometry, key: state.Key)
                        .Bind(lease => lease.Use(surface =>
                            Evaluation.SurfaceUv(surface: surface, uv: state.Start, context: context, key: state.Key)
                                .Bind(uvStart => Evaluation.SurfaceUv(surface: surface, uv: state.End, context: context, key: state.Key)
                                    .Bind(uvEnd => Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: context.Absolute.Value))
                                        .ToFin(state.Key.InvalidResult())
                                        .Map(static path => Seq(path)))))).ToEff()
                    select result).As<TGeometry, TOut>(key: LocationKeys.ShortPath)
            : LocationKeys.ShortPath.Unsupported<TGeometry, TOut>();
```
**To**
```csharp
    internal static Operation<TGeometry, TOut> Orientation<TGeometry, TOut>(Plane frame, Op key) where TGeometry : notnull =>
        (frame.IsValid, Capability.CurveForm.Admits(typeof(TGeometry)), typeof(TOut) == typeof(CurveOrientation)) switch {
            (false, _, _) => Operation<TGeometry, TOut>.Reject(key, key.InvalidInput()),
            (true, true, true) => Operation<TGeometry, CurveOrientation>.Build(key, requirement: Some(Requirement.AreaMass), state: (Frame: frame, Key: key),
                evaluator: static (state, geometry) => Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                    curve.ClosedCurveOrientation(state.Frame) switch {
                        CurveOrientation.Undefined => Fin.Fail<Seq<CurveOrientation>>(state.Key.InvalidResult()),
                        CurveOrientation value => state.Key.Accept(value),
                    })).ToEff()).As<TGeometry, TOut>(key),
            _ => key.Unsupported<TGeometry, TOut>(),
        };

    internal static Operation<TGeometry, TOut> Contains<TGeometry, TOut>(Point3d probe, Plane frame, Op key) where TGeometry : notnull =>
        (ValidityClaim.Finite(probe).Holds && frame.IsValid, Capability.CurveForm.Admits(typeof(TGeometry)), typeof(TOut) == typeof(PointContainment)) switch {
            (false, _, _) => Operation<TGeometry, TOut>.Reject(key, key.InvalidInput()),
            (true, true, true) => Operation<TGeometry, PointContainment>.Build(key, requirement: Some(Requirement.AreaMass), requiresContext: true,
                state: (Probe: probe, Frame: frame, Key: key), evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from values in Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                        curve.Contains(state.Probe, state.Frame, context.Absolute.Value) switch {
                            PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                            PointContainment value => state.Key.Accept(value),
                        })).ToEff()
                    select values).As<TGeometry, TOut>(key),
            _ => key.Unsupported<TGeometry, TOut>(),
        };

    internal static Operation<TGeometry, TOut> ShortPath<TGeometry, TOut>(Point2d start, Point2d end, Op key) where TGeometry : notnull =>
        (start.IsValid && end.IsValid && start != end, Capability.SurfaceForm.Admits(typeof(TGeometry)), typeof(TOut) == typeof(Curve)) switch {
            (false, _, _) => Operation<TGeometry, TOut>.Reject(key, key.InvalidInput()),
            (true, true, true) => Operation<TGeometry, Curve>.Build(key, requirement: Some(Requirement.SurfaceEvaluation), requiresContext: true,
                state: (Start: start, End: end, Key: key), evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from paths in Normalization.SurfaceForm(geometry, state.Key).Bind(lease => lease.Use(surface =>
                        from a in Evaluation.SurfaceUv(surface, state.Start, context, state.Key)
                        from b in Evaluation.SurfaceUv(surface, state.End, context, state.Key)
                        from path in Optional(surface.ShortPath(a, b, context.Absolute.Value)).ToFin(state.Key.InvalidResult())
                        select Seq(path))).ToEff()
                    select paths).As<TGeometry, TOut>(key),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
```
**Why**
These builders defer invalid frame, probe, endpoint, and closed-planar-curve failures until host evaluation. Orientation also treats RhinoCommon's `Undefined` sentinel as successful output.
**Change**
Admit request geometry at construction, apply the existing closed-planar readiness owner to orientation and containment, reject host sentinels, and perform each surface endpoint admission once before `ShortPath`.
**Ripples**
Register `Curve.ClosedCurveOrientation(Plane) -> CurveOrientation`, `Curve.Contains(Point3d, Plane, double) -> PointContainment`, and `Surface.ShortPath(Point2d, Point2d, double) -> Curve` in `libs/dotnet/Rasm/.api/api-rhino.md`.
**Delta**
LOC +9; types 0; members 0

# 8. Replace curvature lanes with one sampled scalar pipeline
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L439-L534**
```csharp
    // --- [CURVATURE_SWEEP]
    internal static Operation<TGeometry, TOut> Curvature<TGeometry, TOut>(int count, CurvatureMode mode, CurvatureAggregation aggregation) where TGeometry : notnull {
        Op key = aggregation.Key;
        return count <= 0
            ? Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput())
            : Capability.CurveForm.Admits(type: typeof(TGeometry))
                ? mode.OnCurve<TOut>(aggregation: aggregation).Match(
                    Some: project => Sweep<TGeometry, TOut, Curve>(key: key, count: count, requirement: Requirement.CurveLength, native: Normalization.CurveForm, project: project),
                    None: () => key.Unsupported<TGeometry, TOut>())
                : Capability.SurfaceForm.Admits(type: typeof(TGeometry))
                    ? mode.OnSurface<TOut>(aggregation: aggregation).Match(
                        Some: project => Sweep<TGeometry, TOut, Surface>(key: key, count: count, requirement: Requirement.SurfaceEvaluation, native: Normalization.SurfaceForm, project: project),
                        None: () => key.Unsupported<TGeometry, TOut>())
                    : key.Unsupported<TGeometry, TOut>();
    }

    internal static Option<Func<Op, Curve, int, Context, Fin<Seq<TOut>>>> CurveLane<TOut>(CurvatureAggregation aggregation, ScalarMetric metric) =>
        aggregation.Reduce<TOut>(metric: metric).Map(reduce =>
            (Func<Op, Curve, int, Context, Fin<Seq<TOut>>>)((op, curve, n, ctx) =>
                CurveSamples(key: op, curve: curve, count: n, context: ctx, metric: metric).Bind(samples => reduce(op, samples, ctx))));

    internal static Option<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>> SurfaceLane<TOut>(CurvatureAggregation aggregation, ScalarMetric metric) =>
        aggregation.Reduce<TOut>(metric: metric).Map(reduce =>
            (Func<Op, Surface, int, Context, Fin<Seq<TOut>>>)((op, surface, n, ctx) =>
                SurfaceSamples(key: op, surface: surface, count: n, context: ctx, metric: metric).Bind(samples => reduce(op, samples, ctx))));

    internal static Option<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>> SurfaceStatLane<TOut>(CurvatureAggregation aggregation, Seq<ScalarMetric> metrics) =>
        metrics.IsEmpty || aggregation is not CurvatureAggregation.SamplesCase || typeof(TOut) != typeof(Stat<Scalar>)
            ? Option<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>>.None
            : Some<Func<Op, Surface, int, Context, Fin<Seq<TOut>>>>((op, surface, n, ctx) =>
                SurfaceStats(key: op, surface: surface, count: n, context: ctx, metrics: metrics).Bind(stats => op.AcceptResults<Stat<Scalar>, TOut>(values: stats)));

    internal static Option<Func<Op, Seq<CurvatureSample>, Context, Fin<Seq<TOut>>>> SampleColumn<TOut>(ScalarMetric metric) =>
        typeof(TOut) == typeof(Point3d)
            ? Some<Func<Op, Seq<CurvatureSample>, Context, Fin<Seq<TOut>>>>(static (key, samples, _) =>
                key.AcceptResults<Point3d, TOut>(values: samples.Map(static sample => sample.Point)))
        : typeof(TOut) == typeof(double)
            ? Some<Func<Op, Seq<CurvatureSample>, Context, Fin<Seq<TOut>>>>(static (key, samples, _) =>
                key.AcceptResults<double, TOut>(values: samples.Map(static sample => sample.Curvature)))
        : typeof(TOut) == typeof(Stat<Scalar>)
            ? Some<Func<Op, Seq<CurvatureSample>, Context, Fin<Seq<TOut>>>>((key, samples, _) =>
                Stat<Scalar>.Of(values: samples.Map(static sample => (Scalar)sample.Curvature), key: key, context: Some((StatContext)metric))
                    .Bind(stat => key.AcceptResults<Stat<Scalar>, TOut>(values: Seq(stat))))
        : Option<Func<Op, Seq<CurvatureSample>, Context, Fin<Seq<TOut>>>>.None;

    private static Operation<TGeometry, TOut> Sweep<TGeometry, TOut, TNative>(Op key, int count, Requirement requirement, Func<object?, Op, Fin<Lease<TNative>>> native, Func<Op, TNative, int, Context, Fin<Seq<TOut>>> project)
        where TGeometry : notnull
        where TNative : class, IDisposable =>
        Operation<TGeometry, TOut>.Build(
            key: key, requirement: Some(requirement), state: (Key: key, Count: count, Native: native, Project: project),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in state.Native(arg1: geometry, arg2: state.Key)
                    .Bind(lease => lease.Use((State: state, Context: context), static (s, native) => s.State.Project(arg1: s.State.Key, arg2: native, arg3: s.State.Count, arg4: s.Context))).ToEff()
                select result);

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct CurvatureSample(Point3d Point, double Curvature) : IValidityEvidence {
        public bool IsValid => ValidityClaim.All(ValidityClaim.Finite(value: Point), ValidityClaim.Nonnegative(value: Curvature));
    }

    internal static Fin<Seq<Vector3d>> CurveCurvatures(Op key, Curve curve, int count, Context context) =>
        Evaluation.CurveSampleParameters(curve: curve, count: count, context: context, key: key)
            .Bind(parameters => key.Accept(values: parameters.Map(t => curve.CurvatureAt(t: t))));
    private static Fin<Seq<CurvatureSample>> CurveSamples(Op key, Curve curve, int count, Context context, ScalarMetric metric) =>
        Evaluation.CurveSampleParameters(curve: curve, count: count, context: context, key: key)
            .Bind(parameters => parameters.TraverseM(t => metric.Of(value: curve.CurvatureAt(t: t), key: key)
                .Map(value => new CurvatureSample(Point: curve.PointAt(t: t), Curvature: value))).As())
            .Bind(samples => key.Accept(values: samples));

    private static Fin<T> WithBundle<T>(Op key, Surface surface, Point2d uv, Func<SurfaceCurvature, Fin<T>> project) =>
        Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(key.InvalidResult())
            .Bind(bundle => new Lease<SurfaceCurvature>.Owned(Value: bundle)
                .Use(scoped => scoped.IsSet ? project(arg: scoped) : Fin.Fail<T>(key.InvalidResult())));
    internal static Fin<Seq<SurfaceCurvature>> SurfaceBundles(Op key, Surface surface, int count, Context context) =>
        Evaluation.SurfaceSampleUv(surface: surface, count: count, context: context, key: key)
            .Map(uvs => uvs.Map(uv => surface.CurvatureAt(u: uv.X, v: uv.Y)))
            .Bind(bundles => {
                if (bundles.ForAll(static bundle => bundle is { IsSet: true })) { return Fin.Succ(bundles.Map(static bundle => bundle!)); }
                foreach (SurfaceCurvature? bundle in bundles) { bundle?.Dispose(); }
                return Fin.Fail<Seq<SurfaceCurvature>>(key.InvalidResult());
            });
    private static Fin<Seq<CurvatureSample>> SurfaceSamples(Op key, Surface surface, int count, Context context, ScalarMetric metric) =>
        Evaluation.SurfaceSampleUv(surface: surface, count: count, context: context, key: key)
            .Bind(uvs => uvs.TraverseM(uv => WithBundle(key: key, surface: surface, uv: uv,
                project: bundle => metric.Of(value: bundle, key: key).Map(value => new CurvatureSample(Point: bundle.Point, Curvature: value)))).As())
            .Bind(samples => key.Accept(values: samples));
    internal static Fin<Seq<Stat<Scalar>>> SurfaceStats(Op key, Surface surface, int count, Context context, Seq<ScalarMetric> metrics) =>
        Evaluation.SurfaceSampleUv(surface: surface, count: count, context: context, key: key)
            .Bind(uvs => uvs.TraverseM(uv => WithBundle(key: key, surface: surface, uv: uv,
                project: bundle => metrics.TraverseM(metric => metric.Of(value: bundle, key: key)).As().Map(static row => row.ToArray()))).As())
            .Bind(rows => toSeq(Enumerable.Range(start: 0, count: metrics.Count))
                .TraverseM(index => Stat<Scalar>.Of(
                    values: rows.Map(row => (Scalar)row[index]), key: key,
                    context: Some((StatContext)metrics[index]))).As());
}
```
**To**
```csharp
    internal static Operation<TGeometry, TOut> Curvature<TGeometry, TOut>(Dimension count, ScalarMetric metric, CurvatureOutput output, Op key) where TGeometry : notnull {
        bool curve = Capability.CurveForm.Admits(typeof(TGeometry)) && metric.Vector.IsSome;
        bool surface = Capability.SurfaceForm.Admits(typeof(TGeometry)) && metric.Curvature.IsSome;
        return !output.Accepts(typeof(TOut)) || (!curve && !surface)
            ? key.Unsupported<TGeometry, TOut>()
            : Operation<TGeometry, TOut>.Build(key,
                requirement: Some(curve ? Requirement.CurveLength : Requirement.SurfaceEvaluation), requiresContext: true,
                state: (Curve: curve, Count: count, Metric: metric, Output: output, Key: key), evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from samples in (state.Curve
                        ? Normalization.CurveForm(geometry, state.Key).Bind(lease => lease.Use(curve =>
                            CurveSamples(curve, state.Count, state.Metric, context, state.Key)))
                        : Normalization.SurfaceForm(geometry, state.Key).Bind(lease => lease.Use(surface =>
                            SurfaceSamples(surface, state.Count, state.Metric, context, state.Key)))).ToEff()
                    from result in state.Output.Reduce<TOut>(samples, state.Metric, context, state.Key).ToEff()
                    select result);
    }

    private static Fin<Seq<CurvatureSample>> CurveSamples(Curve curve, Dimension count, ScalarMetric metric, Context context, Op key) =>
        Evaluation.CurveSampleParameters(curve, count.Value, context, key).Bind(parameters => parameters
            .TraverseM(t => metric.Of(curve.CurvatureAt(t), key)
                .Map(value => new CurvatureSample(curve.PointAt(t), value))).As());

    private static Fin<Seq<CurvatureSample>> SurfaceSamples(Surface surface, Dimension count, ScalarMetric metric, Context context, Op key) =>
        Evaluation.SurfaceSampleUv(surface, count.Value, context, key).Bind(uvs => uvs.TraverseM(uv =>
            Optional(surface.CurvatureAt(uv.X, uv.Y)).ToFin(key.InvalidResult()).Bind(bundle =>
                new Lease<SurfaceCurvature>.Owned(bundle).Use(scoped => scoped.IsSet
                    ? metric.Of(scoped, key).Map(value => new CurvatureSample(scoped.Point, value))
                    : Fin.Fail<CurvatureSample>(key.InvalidResult())))).As());
}
```
**Why**
The lane/delegate matrix repeats one sample flow across curve and surface, and `SurfaceBundles` returns successful `IDisposable` handles after their ownership has escaped. The nonnegative check also rejects valid signed Gaussian and mean curvature.
**Change**
Use the metric's existing sparse payload columns for family compatibility, sample each host in one traversal, scope every `SurfaceCurvature` lease at the point of projection, and reduce once through `CurvatureOutput`. Keep raw vector curvature available through `CurveProjection.Curvature` instead of a second sweep lane.
**Ripples**
Register `Surface.CurvatureAt(double, double) -> SurfaceCurvature` in `libs/dotnet/Rasm/.api/api-rhino.md`. `libs/dotnet/Rasm/.planning/Domain/evaluation.md` remains the sole station-grid owner and `libs/dotnet/Rasm/.planning/Domain/stats.md` remains the sole summary/extremum owner.
**Delta**
LOC -70; types -1; members -11

# 9. Remove the deleted layout attribute import
**From — libs/dotnet/Rasm/.planning/Parametric/locate.md:L35-L35**
```csharp
using System.Runtime.InteropServices;
```
**To**
```csharp
// System.Runtime.InteropServices import DELETED
```
**Why**
The only consumer is the deleted hand-written `CurvatureSample` layout attribute; the replacement carrier relies on its ordinary record-struct layout.
**Change**
Delete the unused namespace import with the old carrier.
**Delta**
LOC -1; types 0; members 0
