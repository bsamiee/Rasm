namespace Rasm.Analysis;

public static partial class Query {
    public static Query<BoundingBox, Point3d> UniqueCorners() => BoundingCorners<BoundingBox, Point3d>();
    public static Query<TGeometry, TOut> BoundingCorners<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: UniqueCornersKey, query: Query<TGeometry, Point3d>.Build(
                key: UniqueCornersKey,
                    requiresContext: true,
                    evaluator: static geom => from ctx in Analyze.Asks
                                              from bbox in BoundingBoxOf(geom: geom, key: UniqueCornersKey, outputType: typeof(Point3d)).ToEff()
                                              from result in (bbox.IsValid switch {
                                                  true => Optional(Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: ctx.Absolute.Value))
                                                      .ToFin(UniqueCornersKey.InvalidResult())
                                                      .Map(static unique => toSeq(unique)),
                                                  false => Fin.Fail<Seq<Point3d>>(UniqueCornersKey.InvalidInput()),
                                              }).ToEff()
                                              select result)),
            _ => UniqueCornersKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<BoundingBox> BoundingBoxOf<TGeometry>(TGeometry geom, Op key, Type outputType) where TGeometry : notnull =>
        geom switch {
            Point3d point when point.IsValid => Fin.Succ(new BoundingBox(min: point, max: point)),
            Point point when point.IsValid => Fin.Succ(point.GetBoundingBox(accurate: true)),
            BoundingBox bbox when bbox.IsValid => Fin.Succ(bbox),
            GeometryBase gb when gb.IsValid => Fin.Succ(gb.GetBoundingBox(accurate: true)),
            Box box when box.IsValid => Fin.Succ(box.BoundingBox),
            Sphere sphere when sphere.IsValid => Fin.Succ(sphere.BoundingBox),
            Line line when line.IsValid => Fin.Succ(line.BoundingBox),
            Polyline polyline => Fin.Succ(polyline.BoundingBox),
            Circle circle => Fin.Succ(circle.BoundingBox),
            Arc arc when arc.IsValid => NativeBounds(source: arc.ToNurbsCurve(), key: key),
            Cylinder cylinder when cylinder.IsValid => NativeBounds(source: cylinder.ToBrep(capBottom: true, capTop: true), key: key),
            Cone cone when cone.IsValid => NativeBounds(source: cone.ToBrep(capBottom: true), key: key),
            Torus torus when torus.IsValid => NativeBounds(source: torus.ToBrep(), key: key),
            _ => Fin.Fail<BoundingBox>(key.Unsupported(geometryType: typeof(TGeometry), outputType: outputType)),
        };
    private static Fin<BoundingBox> NativeBounds<TNative>(TNative? source, Op key) where TNative : GeometryBase =>
        Optional(source)
            .ToFin(key.InvalidResult())
            .Map(static value => {
                using TNative disposable = value;
                return disposable.GetBoundingBox(accurate: true);
            });
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull =>
        Aspect(
            aspect: aspect,
            key: BoundsKey,
            dispatch: static candidate => candidate.Switch(
                box: static _ => Box<TGeometry, TOut>(),
                oriented: static o => Oriented<TGeometry, TOut>(plane: o.Plane),
                transformed: static t => Transformed<TGeometry, TOut>(transform: t.Transform),
                center: static _ => BoundsFromBox<TGeometry, TOut, Point3d>(key: BoundsCenterKey, project: static box => One(key: BoundsCenterKey, value: box.Center)),
                corners: static _ => BoundsFromBox<TGeometry, TOut, Point3d>(key: BoundsCornersKey, project: static box => Many(key: BoundsCornersKey, values: box.GetCorners())),
                edges: static _ => (typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line))
                    ? Cast<TGeometry, TOut>(key: BoxEdgesKey, query: Query<BoundingBox, Line>.Build(
                        key: BoxEdgesKey,
                        evaluator: static geometry => Many(key: BoxEdgesKey, values: geometry.GetEdges()).ToEff()))
                    : null,
                area: static _ => BoxMetric<TGeometry, TOut>(key: BoxAreaKey, boundingBox: static geometry => geometry.Area, box: static geometry => geometry.Area),
                volume: static _ => BoxMetric<TGeometry, TOut>(key: BoxVolumeKey, boundingBox: static geometry => geometry.Volume, box: static geometry => geometry.Volume)));
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull =>
        Aspect(
            aspect: aspect,
            key: MeasureKey,
            dispatch: static candidate => candidate.Switch(
                spatialMidpoint: static _ => typeof(TOut) == typeof(Point3d) ? SpatialMidpoint<TGeometry, TOut>() : null,
                length: static _ => Length<TGeometry, TOut>(),
                area: static _ => typeof(TOut) == typeof(double)
                    ? MassCast<TGeometry, TOut, AreaMassProperties, double>(name: nameof(Analysis.Measure.Area), requirement: Requirement.AreaMass, compute: ComputeArea, aggregate: SumArea, project: static (key, mass) => One(key: key, value: mass.Area))
                    : null,
                volume: static _ => typeof(TOut) == typeof(double)
                    ? MassCast<TGeometry, TOut, VolumeMassProperties, double>(name: nameof(Analysis.Measure.Volume), requirement: Requirement.VolumeMass, compute: ComputeVolume, aggregate: SumVolume, project: static (key, mass) => One(key: key, value: mass.Volume))
                    : null,
                massError: static e => MassMeasure<TGeometry, TOut>(mass: e.Mass, kind: e),
                centroid: static c => MassMeasure<TGeometry, TOut>(mass: c.Mass, kind: c),
                centroidError: static ce => MassMeasure<TGeometry, TOut>(mass: ce.Mass, kind: ce),
                radii: static r => MassMeasure<TGeometry, TOut>(mass: r.Mass, kind: r),
                principalAxes: static p => MassMeasure<TGeometry, TOut>(mass: p.Mass, kind: p)));
    private static Query<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure kind) where TGeometry : notnull =>
        (kind, typeof(TOut)) switch {
            (_, _) when mass is MassKind.None => Query<TGeometry, TOut>.Reject(key: MeasureKey, fault: MeasureKey.InvalidInput()),
            (Analysis.Measure.MassError, Type output) when output == typeof(double) => MassByKind<TGeometry, TOut, double>(mass: mass, suffix: "Error", length: static (key, props) => One(key: key, value: props.LengthError), area: static (key, props) => One(key: key, value: props.AreaError), volume: static (key, props) => One(key: key, value: props.VolumeError), lengthSecond: true),
            (Analysis.Measure.Centroid, Type output) when output == typeof(Point3d) => MassByKind<TGeometry, TOut, Point3d>(mass: mass, suffix: "Centroid", length: static (key, props) => One(key: key, value: props.Centroid), area: static (key, props) => One(key: key, value: props.Centroid), volume: static (key, props) => One(key: key, value: props.Centroid), lengthSecond: true),
            (Analysis.Measure.CentroidError, Type output) when output == typeof(Vector3d) => MassByKind<TGeometry, TOut, Vector3d>(mass: mass, suffix: "CentroidError", length: static (key, props) => One(key: key, value: props.CentroidError), area: static (key, props) => One(key: key, value: props.CentroidError), volume: static (key, props) => One(key: key, value: props.CentroidError), lengthSecond: true),
            (Analysis.Measure.Radii, Type output) when output == typeof(Vector3d) => MassByKind<TGeometry, TOut, Vector3d>(mass: mass, suffix: "Radii", length: static (key, props) => One(key: key, value: props.CentroidCoordinatesRadiiOfGyration), area: static (key, props) => One(key: key, value: props.CentroidCoordinatesRadiiOfGyration), volume: static (key, props) => One(key: key, value: props.CentroidCoordinatesRadiiOfGyration), lengthSecond: true, areaSecond: true, volumeSecond: true),
            (Analysis.Measure.PrincipalAxes, Type output) when output == typeof(ValueTuple<double, Vector3d>) => MassByKind<TGeometry, TOut, (double Moment, Vector3d Axis)>(mass: mass, suffix: "Principal", length: static (key, props) => key.Principal(mass: props), area: static (key, props) => key.Principal(mass: props), volume: static (key, props) => key.Principal(mass: props), lengthSecond: true, areaSecond: true, volumeSecond: true, product: true),
            _ => MeasureKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> MassByKind<TGeometry, TOut, TValue>(MassKind mass, string suffix, Func<Op, LengthMassProperties, Fin<Seq<TValue>>> length, Func<Op, AreaMassProperties, Fin<Seq<TValue>>> area, Func<Op, VolumeMassProperties, Fin<Seq<TValue>>> volume, bool lengthSecond = false, bool areaSecond = false, bool volumeSecond = false, bool product = false) where TGeometry : notnull =>
        mass switch {
            MassKind.Length => MassCast<TGeometry, TOut, LengthMassProperties, TValue>(name: $"Length{suffix}", requirement: Requirement.CurveLength, compute: ComputeLength, aggregate: SumLength, project: length, secondMoments: lengthSecond, productMoments: product),
            MassKind.Area => MassCast<TGeometry, TOut, AreaMassProperties, TValue>(name: $"Area{suffix}", requirement: Requirement.AreaMass, compute: ComputeArea, aggregate: SumArea, project: area, secondMoments: areaSecond, productMoments: product),
            MassKind.Volume => MassCast<TGeometry, TOut, VolumeMassProperties, TValue>(name: $"Volume{suffix}", requirement: Requirement.VolumeMass, compute: ComputeVolume, aggregate: SumVolume, project: volume, secondMoments: volumeSecond, productMoments: product),
            _ => Query<TGeometry, TOut>.Reject(key: MeasureKey, fault: MeasureKey.InvalidInput()),
        };
    private static Query<TGeometry, TOut> MassCast<TGeometry, TOut, TMass, TValue>(string name, Requirement requirement, Func<object, bool, bool, Eff<Analyze.Runtime, TMass>> compute, Func<Seq<TMass>, Fin<TMass>> aggregate, Func<Op, TMass, Fin<Seq<TValue>>> project, bool secondMoments = false, bool productMoments = false) where TGeometry : notnull where TMass : class, IDisposable => Cast<TGeometry, TOut>(key: new Op(name: name), query: Mass<TGeometry, TMass, TValue>(name: name, requirement: requirement, compute: compute, aggregate: aggregate, project: project, secondMoments: secondMoments, productMoments: productMoments));
    public static Query<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(Point3d)
                && SupportsBounds(geometry: geometry, includeSphere: false) =>
                Cast<TGeometry, TOut>(key: SpatialMidpointKey, query: Query<TGeometry, Point3d>.Build(
                    key: SpatialMidpointKey,
                    evaluator: static geometry => geometry switch {
                        Point3d point => One(key: SpatialMidpointKey, value: point).ToEff(),
                        Point point => One(key: SpatialMidpointKey, value: point.Location).ToEff(),
                        Line line => One(key: SpatialMidpointKey, value: line.PointAt(t: 0.5)).ToEff(),
                        Polyline polyline => One(key: SpatialMidpointKey, value: polyline.CenterPoint()).ToEff(),
                        Curve curve =>
                            from ctx in Analyze.Asks
                            from result in (curve.IsClosed, curve.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value)) switch {
                                (true, true) => MassCentroid(geometry: curve, mass: MassKind.Area),
                                _ => MassCentroid(geometry: curve, mass: MassKind.Length),
                            }
                            select result,
                        Brep brep => MassCentroid(geometry: brep, mass: brep.IsSolid ? MassKind.Volume : MassKind.Area),
                        Mesh mesh => MassCentroid(geometry: mesh, mass: mesh.IsSolid ? MassKind.Volume : MassKind.Area),
                        Surface surface => MassCentroid(geometry: surface, mass: surface.IsSolid ? MassKind.Volume : MassKind.Area),
                        SubD subd =>
                            from runtime in Analyze.RuntimeAsks
                            from validated in runtime.Context.Validate(geometry: subd, requirement: Requirement.Basic).ToEff()
                            from result in Optional(validated.ToBrep(options: SubDToBrepOptions.Default))
                                .ToFin(SpatialMidpointKey.InvalidResult())
                                .Bind(brep => {
                                    using Brep disposable = brep;
                                    return MassCentroid(geometry: disposable, mass: disposable.IsSolid ? MassKind.Volume : MassKind.Area).Run(env: runtime);
                                })
                                .ToEff()
                            select result,
                        BoundingBox box => One(key: SpatialMidpointKey, value: box.Center).ToEff(),
                        Box box => One(key: SpatialMidpointKey, value: box.Center).ToEff(),
                        PointCloud or Circle or Arc or Cylinder or Cone or Torus =>
                            BoundingBoxOf(geom: geometry, key: SpatialMidpointKey, outputType: typeof(Point3d))
                                .Bind(static box => One(key: SpatialMidpointKey, value: box.Center))
                                .ToEff(),
                        _ => Fin.Fail<Seq<Point3d>>(SpatialMidpointKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => SpatialMidpointKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(
        Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        Optional(aspect)
            .Map(candidate => Aspect(
                aspect: candidate,
                key: ConformanceKey,
                dispatch: static candidate => (candidate, typeof(TGeometry), typeof(TPrimitive)) switch {
                    (Conformance.Distance { Count: <= 0 } or Conformance.Rms { Count: <= 0 } or Conformance.WithinTolerance { Count: <= 0 } or Conformance.ProfileResidual { Count: <= 0 } or Conformance.Maximum { Count: <= 0 }, _, _) => Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: ConformanceKey, fault: ConformanceKey.InvalidInput()),
                    (_, Type geometry, Type primitive) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Line) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Line>(
                        aspect: candidate,
                        requirement: Requirement.CurveLength,
                        samples: static (geometry, primitive, count, context) => CurvePrimitiveSamples(geometry: geometry, primitive: primitive, count: count, context: context, distance: static (line, point) => point.DistanceTo(other: line.ClosestPoint(testPoint: point, limitToFiniteSegment: false)))),
                    (_, Type geometry, Type primitive) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Circle) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Circle>(
                        aspect: candidate,
                        requirement: Requirement.CurveLength,
                        samples: static (geometry, primitive, count, context) => CurvePrimitiveSamples(geometry: geometry, primitive: primitive, count: count, context: context, distance: static (circle, point) => point.DistanceTo(other: circle.ClosestPoint(testPoint: point)))),
                    (_, Type geometry, Type primitive) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Arc) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Arc>(
                        aspect: candidate,
                        requirement: Requirement.CurveLength,
                        samples: static (geometry, primitive, count, context) => CurvePrimitiveSamples(geometry: geometry, primitive: primitive, count: count, context: context, distance: static (arc, point) => point.DistanceTo(other: arc.ClosestPoint(testPoint: point)))),
                    (_, Type geometry, Type primitive) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Plane) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Plane>(
                        aspect: candidate,
                        requirement: Requirement.SurfaceEvaluation,
                        samples: static (geometry, primitive, count, context) => SurfacePrimitiveSamples(geometry: geometry, primitive: primitive, count: count, context: context, distance: static (plane, point) => Math.Abs(value: plane.DistanceTo(testPoint: point)))),
                    (_, Type geometry, Type primitive) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Sphere) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Sphere>(
                        aspect: candidate,
                        requirement: Requirement.SurfaceEvaluation,
                        samples: static (geometry, primitive, count, context) => SurfacePrimitiveSamples(geometry: geometry, primitive: primitive, count: count, context: context, distance: static (sphere, point) => point.DistanceTo(other: sphere.ClosestPoint(testPoint: point)))),
                    _ => null,
                }))
            .IfNone(() => Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: ConformanceKey, fault: ConformanceKey.InvalidInput()));
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceCases<TGeometry, TPrimitive, TOut, TNativeGeometry, TNativePrimitive>(
        Conformance aspect,
        Requirement requirement,
        Func<TNativeGeometry, TNativePrimitive, int, Context, Fin<Seq<ResidualSample>>> samples) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        (aspect, typeof(TOut)) switch {
            (Conformance.Distance item, Type output) when output == typeof(double) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                    count: item.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualDistance)),
            (Conformance.Rms item, Type output) when output == typeof(double) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                    count: item.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualRms)),
            (Conformance.WithinTolerance item, Type output) when output == typeof(bool) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, bool>(
                    count: item.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualWithinTolerance)),
            (Conformance.ProfileResidual item, Type output) when output == typeof(ResidualProfile) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualProfile>(
                    count: item.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualProfileProjection)),
            (Conformance.Maximum item, Type output) when output == typeof(ResidualSample) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualSample>(
                    count: item.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualMaximum)),
            _ => ConformanceKey.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TValue> ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, TValue>(
        int count,
        Requirement requirement,
        Func<TNativeGeometry, TNativePrimitive, int, Context, Fin<Seq<ResidualSample>>> samples,
        Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: ConformanceKey,
            requiresContext: true,
            state: (Count: count, Requirement: requirement, Samples: samples, Project: project),
            evaluator: static (state, geometry) =>
                from ctx in Analyze.Asks
                from validated in ctx.ValidatePair(a: geometry.Geometry,
                        b: geometry.Primitive,
                        requirementA: state.Requirement,
                        requirementB: Requirement.None)
                    .ToEff()
                from result in ConformanceProject(
                        geometry: validated,
                        context: ctx,
                        count: state.Count,
                        samples: state.Samples,
                        project: state.Project)
                    .ToEff()
                select result);
    private static Fin<Seq<TValue>> ConformanceProject<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, TValue>(
        (TGeometry Geometry, TPrimitive Primitive) geometry,
        Context context,
        int count,
        Func<TNativeGeometry, TNativePrimitive, int, Context, Fin<Seq<ResidualSample>>> samples,
        Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        (geometry.Geometry, geometry.Primitive) switch {
            (TNativeGeometry native, TNativePrimitive primitive) => samples(
                    arg1: native,
                    arg2: primitive,
                    arg3: count,
                    arg4: context)
                .Bind(values => project(
                    arg1: values,
                    arg2: context)),
            _ => Fin.Fail<Seq<TValue>>(ConformanceKey.Unsupported(
                geometryType: typeof((TGeometry Geometry, TPrimitive Primitive)),
                outputType: typeof(TValue))),
        };
    private static Fin<Seq<ResidualSample>> CurvePrimitiveSamples<TPrimitive>(
        Curve geometry,
        TPrimitive primitive,
        int count,
        Context context,
        Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        Fractions(count: count, key: ConformanceKey)
            .Bind(fractions => Optional(geometry.NormalizedLengthParameters(
                    s: [.. fractions.AsIterable()],
                    absoluteTolerance: context.Absolute.Value,
                    fractionalTolerance: context.Relative.Value))
                .ToFin(ConformanceKey.InvalidResult())
                .Map(parameters => toSeq(parameters).Map(geometry.PointAt)))
            .Bind(points => ResidualSamples(
                points: points,
                primitive: primitive,
                context: context,
                distance: distance));
    private static Fin<Seq<ResidualSample>> SurfacePrimitiveSamples<TPrimitive>(
        Surface geometry,
        TPrimitive primitive,
        int count,
        Context context,
        Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        (
            Samples(domain: geometry.Domain(direction: 0), count: count, key: ConformanceKey),
            Samples(domain: geometry.Domain(direction: 1), count: count, key: ConformanceKey)
        ).Apply(static (u, v) => (U: u, V: v)).As()
        .Bind(samples => ResidualSamples(points: samples.U.Bind(u => samples.V.Map(v => geometry.PointAt(u: u, v: v))), primitive: primitive, context: context, distance: distance));
    private static Fin<Seq<ResidualSample>> ResidualSamples<TPrimitive>(Seq<Point3d> points, TPrimitive primitive, Context context, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        Fin.Succ(toSeq(points.AsIterable().Select((point, index) => new ResidualSample(
            Index: index,
            Location: point,
            Distance: distance(arg1: primitive, arg2: point),
            Tolerance: context.Absolute.Value,
            WithinTolerance: distance(arg1: primitive, arg2: point) <= context.Absolute.Value))));
    private static Fin<Seq<double>> ResidualDistance(Seq<ResidualSample> samples, Context _) => ResidualDistances(samples: samples).Bind(static residuals => Many(key: ConformanceKey, values: residuals));
    private static Fin<Seq<double>> ResidualRms(Seq<ResidualSample> samples, Context _) =>
        ResidualDistances(samples: samples)
            .Bind(static residuals => Stats.From(values: residuals, key: ConformanceKey))
            .Bind(static s => One(key: ConformanceKey, value: s.Rms));
    private static Fin<Seq<bool>> ResidualWithinTolerance(Seq<ResidualSample> samples, Context context) =>
        ResidualDistances(samples: samples)
            .Bind(static residuals => Stats.From(values: residuals, key: ConformanceKey))
            .Bind(s => One(key: ConformanceKey, value: s.Maximum <= context.Absolute.Value));
    private static Fin<Seq<ResidualProfile>> ResidualProfileProjection(Seq<ResidualSample> samples, Context context) =>
        ResidualDistances(samples: samples)
            .Bind(static residuals => Stats.From(values: residuals, key: ConformanceKey))
            .Bind(s => One(key: ConformanceKey, value: new ResidualProfile(
                Count: s.Count,
                Minimum: s.Minimum,
                Maximum: s.Maximum,
                Mean: s.Mean,
                Variance: s.Variance,
                Rms: s.Rms,
                Tolerance: context.Absolute.Value,
                WithinTolerance: s.Maximum <= context.Absolute.Value)));
    private static Fin<Seq<ResidualSample>> ResidualMaximum(Seq<ResidualSample> samples, Context _) =>
        samples.TraverseM(static sample => sample switch {
            { Distance: double d, Location.IsValid: true } when d >= 0.0 && RhinoMath.IsValidDouble(x: d) => Fin.Succ(sample),
            _ => Fin.Fail<ResidualSample>(ConformanceKey.InvalidResult()),
        }).As()
            .Bind(static validated => validated.Maxima(
                    projection: static sample => sample.Distance,
                    tolerance: 0.0)
                .Head
                .ToFin(ConformanceKey.InvalidResult())
                .Map(static best => Seq(best)));
    private static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples) =>
        samples.TraverseM(static sample => sample switch {
            { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(distance),
            _ => Fin.Fail<double>(ConformanceKey.InvalidResult()),
        }).As();
    private static Eff<Analyze.Runtime, Seq<Point3d>> MassCentroid<TGeometry>(
        TGeometry geometry,
        MassKind mass) where TGeometry : GeometryBase =>
        mass switch {
            MassKind.Length => Mass<TGeometry, LengthMassProperties, Point3d>(name: SpatialMidpointKey.Name, requirement: Requirement.CurveLength, compute: ComputeLength, aggregate: SumLength, project: static (key, props) => One(key: key, value: props.Centroid)).Apply(geometry: geometry),
            MassKind.Area => Mass<TGeometry, AreaMassProperties, Point3d>(name: SpatialMidpointKey.Name, requirement: Requirement.AreaMass, compute: ComputeArea, aggregate: SumArea, project: static (key, props) => One(key: key, value: props.Centroid)).Apply(geometry: geometry),
            MassKind.Volume => Mass<TGeometry, VolumeMassProperties, Point3d>(name: SpatialMidpointKey.Name, requirement: Requirement.VolumeMass, compute: ComputeVolume, aggregate: SumVolume, project: static (key, props) => One(key: key, value: props.Centroid)).Apply(geometry: geometry),
            _ => Fin.Fail<Seq<Point3d>>(SpatialMidpointKey.InvalidInput()).ToEff(),
        };
    private static Query<TGeometry, TOut>? BoundsFromBox<TGeometry, TOut, TValue>(Op key, Func<BoundingBox, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        typeof(TOut) == typeof(TValue)
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(key: key, evaluator: static (state, geometry) => ExtractBounds(geometry: geometry).Bind(state).ToEff(), state: project))
            : null;
    private static Query<TGeometry, TOut>? BoxMetric<TGeometry, TOut>(Op key, Func<BoundingBox, double> boundingBox, Func<Box, double> box) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(BoundingBox) && output == typeof(double) => Cast<TGeometry, TOut>(key: key, query: Query<BoundingBox, double>.Build(
                key: key,
                evaluator: geometry => key.RequireValid(value: geometry).Bind(validated => One(key: key, value: boundingBox(arg: validated))).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Box) && output == typeof(double) => Cast<TGeometry, TOut>(key: key, query: Query<Box, double>.Build(
                key: key,
                evaluator: geometry => key.RequireValid(value: geometry).Bind(validated => One(key: key, value: box(arg: validated))).ToEff())),
            _ => null,
        };
    private static Query<TGeometry, TOut> Box<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(BoundingBox)
                && SupportsBounds(geometry: geometry, includeSphere: true) =>
                Cast<TGeometry, TOut>(key: BoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: BoundsKey,
                    evaluator: static geometry => ExtractBounds(geometry: geometry)
                            .Bind(static box => One(key: BoundsKey, value: box))
                            .ToEff())),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<BoundingBox> ExtractBounds<TGeometry>(TGeometry geometry) where TGeometry : notnull => BoundingBoxOf(geom: geometry, key: BoundsKey, outputType: typeof(BoundingBox));
    private static bool SupportsBounds(Type geometry, bool includeSphere) => typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object) || geometry == typeof(Point3d) || geometry == typeof(Line) || geometry == typeof(Polyline) || geometry == typeof(BoundingBox) || geometry == typeof(Box) || geometry == typeof(Circle) || geometry == typeof(Arc) || geometry == typeof(Cylinder) || geometry == typeof(Cone) || geometry == typeof(Torus) || (includeSphere && geometry == typeof(Sphere));
    private static Query<TGeometry, TOut> Oriented<TGeometry, TOut>(Plane plane) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                Cast<TGeometry, TOut>(key: OrientedBoundsKey, query: Query<TGeometry, Box>.Build(
                    key: OrientedBoundsKey,
                    state: plane,
                    evaluator: static (orientation, geometry) => (geometry switch {
                        GeometryBase native => native.GetBoundingBox(plane: orientation, worldBox: out Box box) switch {
                            BoundingBox local when local.IsValid => One(key: OrientedBoundsKey, value: box),
                            _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.InvalidResult()),
                        },
                        _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Box))),
                    }).ToEff())),
            _ => OrientedBoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Transformed<TGeometry, TOut>(Transform transform) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(BoundingBox) =>
                Cast<TGeometry, TOut>(key: TransformedBoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: TransformedBoundsKey,
                    state: transform,
                    evaluator: static (xform, geometry) => (geometry switch {
                        GeometryBase native => One(key: TransformedBoundsKey, value: native.GetBoundingBox(xform: xform)),
                        _ => Fin.Fail<Seq<BoundingBox>>(TransformedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BoundingBox))),
                    }).ToEff())),
            _ => TransformedBoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Line, double>.Build(
                    key: LengthKey,
                    evaluator: static geometry => LengthKey.RequireValid(value: geometry).Bind(static validated => One(key: LengthKey, value: validated.Length)).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Polyline, double>.Build(
                    key: LengthKey,
                    evaluator: static geometry => LengthKey.RequireValid(value: geometry).Bind(static validated => One(key: LengthKey, value: validated.Length)).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<TGeometry, double>.Build(
                    key: LengthKey,
                    requirement: Requirement.CurveLength,
                    evaluator: static geometry => geometry switch {
                        Curve curve =>
                            from ctx in Analyze.Asks
                            from result in One(
                                key: LengthKey,
                                value: curve.GetLength(fractionalTolerance: ctx.Relative.Value)).ToEff()
                            select result,
                        _ => Fin.Fail<Seq<double>>(LengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))).ToEff(),
                    })),
            _ => LengthKey.Unsupported<TGeometry, TOut>(),
        };
}
