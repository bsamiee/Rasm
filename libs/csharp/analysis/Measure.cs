using Core;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

public static partial class Query {
    private delegate Fin<Seq<ResidualSample>> ResidualSampleCase<TGeometry, TPrimitive>(
        TGeometry geometry,
        TPrimitive primitive,
        int count,
        GeometryContext context) where TGeometry : notnull where TPrimitive : notnull;
    private delegate Point3d ClosestPointCase<TPrimitive>(
        TPrimitive primitive,
        Point3d point) where TPrimitive : notnull;
    private delegate double DistanceCase<TPrimitive>(
        TPrimitive primitive,
        Point3d point) where TPrimitive : notnull;
    private delegate Fin<Seq<TValue>> ResidualProjection<TValue>(
        Seq<ResidualSample> samples,
        GeometryContext context);
    public static Query<BoundingBox, Point3d> UniqueCorners() =>
        Query<BoundingBox, Point3d>.Build(
            key: UniqueCornersKey,
            requiresContext: true,
            evaluator: static (BoundingBox bbox) =>
                from rt in Analyze.Asks
                from result in DedupeCorners(bbox: bbox, tolerance: rt.Context.Absolute.Value).ToEff()
                select result);
    public static Query<TGeometry, TOut> BoundingCorners<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: UniqueCornersKey, query: Query<TGeometry, Point3d>.Build(
                key: UniqueCornersKey,
                requiresContext: true,
                evaluator: static (TGeometry geom) =>
                    from rt in Analyze.Asks
                    from bbox in BoundingBoxOf(geom: geom).ToEff()
                    from result in DedupeCorners(bbox: bbox, tolerance: rt.Context.Absolute.Value).ToEff()
                    select result)),
            _ => UniqueCornersKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<BoundingBox> BoundingBoxOf<TGeometry>(TGeometry geom) where TGeometry : notnull =>
        geom switch {
            BoundingBox bbox when bbox.IsValid => Fin.Succ(bbox),
            GeometryBase gb when gb.IsValid => Fin.Succ(gb.GetBoundingBox(accurate: true)),
            Box box when box.IsValid => Fin.Succ(box.BoundingBox),
            Sphere sphere when sphere.IsValid => Fin.Succ(sphere.BoundingBox),
            Line line when line.IsValid => Fin.Succ(line.BoundingBox),
            Polyline polyline when polyline.IsValid => Fin.Succ(polyline.BoundingBox),
            _ => Fin.Fail<BoundingBox>(UniqueCornersKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
        };
    private static Fin<Seq<Point3d>> DedupeCorners(BoundingBox bbox, double tolerance) =>
        bbox.IsValid switch {
            true => Fin.Succ(bbox.GetCorners()
                .Aggregate(
                    seed: Seq<Point3d>(),
                    func: (Seq<Point3d> acc, Point3d candidate) => acc.Exists((Point3d existing) => existing.DistanceTo(other: candidate) <= tolerance)
                        ? acc
                        : acc.Add(candidate))),
            false => Fin.Fail<Seq<Point3d>>(UniqueCornersKey.InvalidInput()),
        };
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull =>
        aspect switch {
            Analysis.Bounds.Box => Box<TGeometry, TOut>(),
            Analysis.Bounds.Oriented o => Oriented<TGeometry, TOut>(plane: o.Plane),
            Analysis.Bounds.Transformed t => Transformed<TGeometry, TOut>(transform: t.Transform),
            Analysis.Bounds.Center when typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: BoundsCenterKey, query: Query<TGeometry, Point3d>.Build(
                    key: BoundsCenterKey,
                    evaluator: static (TGeometry geometry) =>
                        ExtractBounds(geometry: geometry)
                            .Bind(static (BoundingBox box) => One(key: BoundsCenterKey, value: box.Center))
                            .ToEff())),
            Analysis.Bounds.Corners when typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: BoundsCornersKey, query: Query<TGeometry, Point3d>.Build(
                    key: BoundsCornersKey,
                    evaluator: static (TGeometry geometry) =>
                        ExtractBounds(geometry: geometry)
                            .Bind(static (BoundingBox box) => Many(key: BoundsCornersKey, values: box.GetCorners()))
                            .ToEff())),
            Analysis.Bounds.Edges when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line) =>
                Cast<TGeometry, TOut>(key: BoxEdgesKey, query: Query<BoundingBox, Line>.Build(
                    key: BoxEdgesKey,
                    evaluator: static (BoundingBox geometry) => Many(key: BoxEdgesKey, values: geometry.GetEdges()).ToEff())),
            Analysis.Bounds.Area when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxAreaKey, query: Query<BoundingBox, double>.Build(
                    key: BoxAreaKey,
                    evaluator: static (BoundingBox geometry) => One(key: BoxAreaKey, value: geometry.Area).ToEff())),
            Analysis.Bounds.Area when typeof(TGeometry) == typeof(Box) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxAreaKey, query: Query<Box, double>.Build(
                    key: BoxAreaKey,
                    evaluator: static (Box geometry) => One(key: BoxAreaKey, value: geometry.Area).ToEff())),
            Analysis.Bounds.Volume when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxVolumeKey, query: Query<BoundingBox, double>.Build(
                    key: BoxVolumeKey,
                    evaluator: static (BoundingBox geometry) => One(key: BoxVolumeKey, value: geometry.Volume).ToEff())),
            Analysis.Bounds.Volume when typeof(TGeometry) == typeof(Box) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxVolumeKey, query: Query<Box, double>.Build(
                    key: BoxVolumeKey,
                    evaluator: static (Box geometry) => One(key: BoxVolumeKey, value: geometry.Volume).ToEff())),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull =>
        aspect switch {
            Analysis.Measure.SpatialMidpoint when typeof(TOut) == typeof(Point3d) => SpatialMidpoint<TGeometry, TOut>(),
            Analysis.Measure.Length => Length<TGeometry, TOut>(),
            Analysis.Measure.Area when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Area)), query: AreaMass<TGeometry, double>(name: nameof(Analysis.Measure.Area), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Area))),
            Analysis.Measure.Volume when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Volume)), query: VolumeMass<TGeometry, double>(name: nameof(Analysis.Measure.Volume), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Volume))),
            Analysis.Measure.Error e => MassMeasure<TGeometry, TOut>(mass: e.Mass, kind: e),
            Analysis.Measure.Centroid c => MassMeasure<TGeometry, TOut>(mass: c.Mass, kind: c),
            Analysis.Measure.CentroidError ce => MassMeasure<TGeometry, TOut>(mass: ce.Mass, kind: ce),
            Analysis.Measure.Radii r => MassMeasure<TGeometry, TOut>(mass: r.Mass, kind: r),
            Analysis.Measure.Principal p => MassMeasure<TGeometry, TOut>(mass: p.Mass, kind: p),
            _ => MeasureKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure kind) where TGeometry : notnull =>
        (kind, mass) switch {
            (_, MassKind.None) => Query<TGeometry, TOut>.Reject(key: MeasureKey, fault: MeasureKey.InvalidInput()),
            (Analysis.Measure.Error, MassKind.Length) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthError"), query: LengthMass<TGeometry, double>(name: "LengthError", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.LengthError))),
            (Analysis.Measure.Error, MassKind.Area) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaError"), query: AreaMass<TGeometry, double>(name: "AreaError", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.AreaError))),
            (Analysis.Measure.Error, MassKind.Volume) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeError"), query: VolumeMass<TGeometry, double>(name: "VolumeError", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.VolumeError))),
            (Analysis.Measure.Centroid, MassKind.Length) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroid"), query: LengthCentroid<TGeometry>(name: "LengthCentroid")),
            (Analysis.Measure.Centroid, MassKind.Area) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroid"), query: AreaCentroid<TGeometry>(name: "AreaCentroid")),
            (Analysis.Measure.Centroid, MassKind.Volume) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroid"), query: VolumeCentroid<TGeometry>(name: "VolumeCentroid")),
            (Analysis.Measure.CentroidError, MassKind.Length) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroidError"), query: LengthMass<TGeometry, Vector3d>(name: "LengthCentroidError", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (Analysis.Measure.CentroidError, MassKind.Area) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroidError"), query: AreaMass<TGeometry, Vector3d>(name: "AreaCentroidError", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (Analysis.Measure.CentroidError, MassKind.Volume) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroidError"), query: VolumeMass<TGeometry, Vector3d>(name: "VolumeCentroidError", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (Analysis.Measure.Radii, MassKind.Length) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthRadii"), query: LengthMass<TGeometry, Vector3d>(name: "LengthRadii", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (Analysis.Measure.Radii, MassKind.Area) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaRadii"), query: AreaMass<TGeometry, Vector3d>(name: "AreaRadii", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (Analysis.Measure.Radii, MassKind.Volume) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeRadii"), query: VolumeMass<TGeometry, Vector3d>(name: "VolumeRadii", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (Analysis.Measure.Principal, MassKind.Length) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthPrincipal"), query: LengthMass<TGeometry, (double Moment, Vector3d Axis)>(name: "LengthPrincipal", project: static (OperationKey key, LengthMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            (Analysis.Measure.Principal, MassKind.Area) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaPrincipal"), query: AreaMass<TGeometry, (double Moment, Vector3d Axis)>(name: "AreaPrincipal", project: static (OperationKey key, AreaMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            (Analysis.Measure.Principal, MassKind.Volume) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumePrincipal"), query: VolumeMass<TGeometry, (double Moment, Vector3d Axis)>(name: "VolumePrincipal", project: static (OperationKey key, VolumeMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            _ => MeasureKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(Point3d)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(object)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)
                    || geometry == typeof(Box)) =>
                Cast<TGeometry, TOut>(key: SpatialMidpointKey, query: Query<TGeometry, Point3d>.Build(
                    key: SpatialMidpointKey,
                    evaluator: static (TGeometry geometry) => geometry switch {
                        Line line => One(key: SpatialMidpointKey, value: line.PointAt(t: 0.5)).ToEff(),
                        Polyline polyline => One(key: SpatialMidpointKey, value: polyline.CenterPoint()).ToEff(),
                        Curve curve =>
                            from rt in Analyze.Asks
                            from result in CurveSpatialMidpoint(curve: curve, context: rt.Context)
                            select result,
                        Brep { IsSolid: true } brep => MassCentroid(geometry: brep, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Brep>(name: SpatialMidpointKey.Name)),
                        Brep brep => MassCentroid(geometry: brep, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Brep>(name: SpatialMidpointKey.Name)),
                        Mesh { IsSolid: true } mesh => MassCentroid(geometry: mesh, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Mesh>(name: SpatialMidpointKey.Name)),
                        Mesh mesh => MassCentroid(geometry: mesh, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Mesh>(name: SpatialMidpointKey.Name)),
                        Surface { IsSolid: true } surface => MassCentroid(geometry: surface, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Surface>(name: SpatialMidpointKey.Name)),
                        Surface surface => MassCentroid(geometry: surface, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Surface>(name: SpatialMidpointKey.Name)),
                        SubD subd => SubDSpatialMidpoint(subd: subd),
                        BoundingBox box => One(key: SpatialMidpointKey, value: box.Center).ToEff(),
                        Box box => One(key: SpatialMidpointKey, value: box.Center).ToEff(),
                        _ => Eff<AnalysisRuntime, Seq<Point3d>>.Fail(error: SpatialMidpointKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))),
                    })),
            _ => SpatialMidpointKey.Unsupported<TGeometry, TOut>(),
        };
    private static Eff<AnalysisRuntime, Seq<Point3d>> CurveSpatialMidpoint(Curve curve, GeometryContext context) =>
        (curve.IsClosed, curve.TryGetPlane(plane: out Plane _, tolerance: context.Absolute.Value)) switch {
            (true, true) => MassCentroid(geometry: curve, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Curve>(name: SpatialMidpointKey.Name)),
            _ => MassCentroid(geometry: curve, requirement: GeometryRequirement.CurveLength, query: LengthCentroid<Curve>(name: SpatialMidpointKey.Name)),
        };
    private static Eff<AnalysisRuntime, Seq<Point3d>> SubDSpatialMidpoint(SubD subd) =>
        from rt in Analyze.Asks
        from validated in rt.Context.Validate(geometry: subd, requirement: GeometryRequirement.Basic).ToEff()
        from brep in Optional(validated.ToBrep())
            .ToFin(SpatialMidpointKey.InvalidResult())
            .ToEff()
        from result in SubDBrepSpatialMidpoint(brep: brep)
        select result;
    private static Eff<AnalysisRuntime, Seq<Point3d>> SubDBrepSpatialMidpoint(Brep brep) {
        using Brep disposable = brep;
        return disposable.IsSolid switch {
            true => MassCentroid(geometry: disposable, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Brep>(name: SpatialMidpointKey.Name)),
            false => MassCentroid(geometry: disposable, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Brep>(name: SpatialMidpointKey.Name)),
        };
    }
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(
        Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        (aspect.Residual, aspect.Count, typeof(TGeometry), typeof(TPrimitive), typeof(TOut)) switch {
            (ConformanceResidual.None, _, _, _, _) or (_, <= 0, _, _, _) => Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(
                key: ConformanceKey,
                fault: ConformanceKey.InvalidInput()),
            (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Line) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Line>(
                    aspect: aspect,
                    requirement: GeometryRequirement.CurveLength,
                    samples: CurveLineSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Circle) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Circle>(
                    aspect: aspect,
                    requirement: GeometryRequirement.CurveLength,
                    samples: CurveCircleSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Arc) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Arc>(
                    aspect: aspect,
                    requirement: GeometryRequirement.CurveLength,
                    samples: CurveArcSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Plane) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Plane>(
                    aspect: aspect,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    samples: SurfacePlaneSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Sphere) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Sphere>(
                    aspect: aspect,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    samples: SurfaceSphereSamples),
            _ => ConformanceKey.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceCases<TGeometry, TPrimitive, TOut, TNativeGeometry, TNativePrimitive>(
        Conformance aspect,
        GeometryRequirement requirement,
        ResidualSampleCase<TNativeGeometry, TNativePrimitive> samples) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        (aspect.Residual, typeof(TOut)) switch {
            (ConformanceResidual.Distance, Type output) when output == typeof(double) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualDistance)),
            (ConformanceResidual.Rms, Type output) when output == typeof(double) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualRms)),
            (ConformanceResidual.WithinTolerance, Type output) when output == typeof(bool) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, bool>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualWithinTolerance)),
            (ConformanceResidual.Profile, Type output) when output == typeof(ResidualProfile) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualProfile>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualProfileProjection)),
            (ConformanceResidual.Maximum, Type output) when output == typeof(ResidualSample) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualSample>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualMaximum)),
            _ => ConformanceKey.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TValue> ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, TValue>(
        int count,
        GeometryRequirement requirement,
        ResidualSampleCase<TNativeGeometry, TNativePrimitive> samples,
        ResidualProjection<TValue> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: ConformanceKey,
            requiresContext: true,
            state: (Count: count, Requirement: requirement, Samples: samples, Project: project),
            evaluator: static ((int Count, GeometryRequirement Requirement, ResidualSampleCase<TNativeGeometry, TNativePrimitive> Samples, ResidualProjection<TValue> Project) state, (TGeometry Geometry, TPrimitive Primitive) geometry) =>
                from rt in Analyze.Asks
                from validated in rt.Context.ValidateOperands(
                        geometry: geometry,
                        a: state.Requirement,
                        b: GeometryRequirement.None)
                    .ToEff()
                from result in ConformanceProject(
                        geometry: validated,
                        context: rt.Context,
                        count: state.Count,
                        samples: state.Samples,
                        project: state.Project)
                    .ToEff()
                select result);
    private static Fin<Seq<TValue>> ConformanceProject<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, TValue>(
        (TGeometry Geometry, TPrimitive Primitive) geometry,
        GeometryContext context,
        int count,
        ResidualSampleCase<TNativeGeometry, TNativePrimitive> samples,
        ResidualProjection<TValue> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        (geometry.Geometry, geometry.Primitive) switch {
            (TNativeGeometry native, TNativePrimitive primitive) => samples(
                    geometry: native,
                    primitive: primitive,
                    count: count,
                    context: context)
                .Bind((Seq<ResidualSample> values) => project(
                    samples: values,
                    context: context)),
            _ => Fin.Fail<Seq<TValue>>(ConformanceKey.Unsupported(
                geometryType: typeof((TGeometry Geometry, TPrimitive Primitive)),
                outputType: typeof(TValue))),
        };
    private static Fin<Seq<ResidualSample>> CurveLineSamples(Curve geometry, Line primitive, int count, GeometryContext context) =>
        CurvePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            closest: static (Line line, Point3d point) => line.ClosestPoint(
                testPoint: point,
                limitToFiniteSegment: false));
    private static Fin<Seq<ResidualSample>> CurveCircleSamples(Curve geometry, Circle primitive, int count, GeometryContext context) =>
        CurvePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            closest: static (Circle circle, Point3d point) => circle.ClosestPoint(testPoint: point));
    private static Fin<Seq<ResidualSample>> CurveArcSamples(Curve geometry, Arc primitive, int count, GeometryContext context) =>
        CurvePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            closest: static (Arc arc, Point3d point) => arc.ClosestPoint(testPoint: point));
    private static Fin<Seq<ResidualSample>> CurvePrimitiveSamples<TPrimitive>(
        Curve geometry,
        TPrimitive primitive,
        int count,
        GeometryContext context,
        ClosestPointCase<TPrimitive> closest) where TPrimitive : notnull =>
        Fractions(count: count, key: ConformanceKey)
            .Bind((Seq<double> fractions) => fractions.Fold(
                    initialState: Fin.Succ(new ResidualState<Curve, TPrimitive>(
                        Samples: Seq<ResidualSample>(),
                        Geometry: geometry,
                        Primitive: primitive,
                        Context: context)),
                    f: (Fin<ResidualState<Curve, TPrimitive>> current, double fraction) => (
                            current,
                            Fin.Succ(fraction)
                        ).Apply((ResidualState<Curve, TPrimitive> state, double sample) => (
                            State: state,
                            Sample: sample))
                        .As()
                        .Bind(((ResidualState<Curve, TPrimitive> State, double Sample) pair) => pair.State.Geometry.NormalizedLengthParameter(
                            s: pair.Sample,
                            t: out double parameter,
                            fractionalTolerance: pair.State.Context.Relative.Value) switch {
                                true => pair.State.Geometry.PointAt(t: parameter) switch {
                                    Point3d point => closest(
                                            primitive: pair.State.Primitive,
                                            point: point) switch {
                                                Point3d closest => Fin.Succ(pair.State with {
                                                    Samples = pair.State.Samples.Add(new ResidualSample(
                                                        Index: pair.State.Samples.Count,
                                                        Location: point,
                                                        Distance: point.DistanceTo(other: closest),
                                                        Tolerance: pair.State.Context.Absolute.Value,
                                                        WithinTolerance: point.DistanceTo(other: closest) <= pair.State.Context.Absolute.Value)),
                                                }),
                                            },
                                },
                                false => Fin.Fail<ResidualState<Curve, TPrimitive>>(ConformanceKey.InvalidResult()),
                            }))
                .Map(static (ResidualState<Curve, TPrimitive> state) => state.Samples));
    private static Fin<Seq<ResidualSample>> SurfacePlaneSamples(Surface geometry, Plane primitive, int count, GeometryContext context) =>
        SurfacePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            distance: static (Plane plane, Point3d point) => Math.Abs(value: plane.DistanceTo(testPoint: point)));
    private static Fin<Seq<ResidualSample>> SurfaceSphereSamples(Surface geometry, Sphere primitive, int count, GeometryContext context) =>
        SurfacePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            distance: static (Sphere sphere, Point3d point) => point.DistanceTo(other: sphere.ClosestPoint(testPoint: point)));
    private static Fin<Seq<ResidualSample>> SurfacePrimitiveSamples<TPrimitive>(
        Surface geometry,
        TPrimitive primitive,
        int count,
        GeometryContext context,
        DistanceCase<TPrimitive> distance) where TPrimitive : notnull =>
        (
            Samples(domain: geometry.Domain(direction: 0), count: count, key: ConformanceKey),
            Samples(domain: geometry.Domain(direction: 1), count: count, key: ConformanceKey)
        ).Apply(static (Seq<double> u, Seq<double> v) => (U: u, V: v)).As()
        .Bind(((Seq<double> U, Seq<double> V) samples) => samples.U
            .Bind((double u) => samples.V.Map((double v) => new Point2d(x: u, y: v)))
            .Fold(
                initialState: Fin.Succ(new ResidualState<Surface, TPrimitive>(
                    Samples: Seq<ResidualSample>(),
                    Geometry: geometry,
                    Primitive: primitive,
                    Context: context)),
                f: (Fin<ResidualState<Surface, TPrimitive>> current, Point2d uv) => current.Map((ResidualState<Surface, TPrimitive> state) => state.Geometry.PointAt(u: uv.X, v: uv.Y) switch {
                    Point3d point => state with {
                        Samples = state.Samples.Add(new ResidualSample(
                            Index: state.Samples.Count,
                            Location: point,
                            Distance: distance(primitive: state.Primitive, point: point),
                            Tolerance: state.Context.Absolute.Value,
                            WithinTolerance: distance(primitive: state.Primitive, point: point) <= state.Context.Absolute.Value)),
                    },
                })))
        .Map(static (ResidualState<Surface, TPrimitive> state) => state.Samples);
    private static Fin<Seq<double>> ResidualDistance(Seq<ResidualSample> samples, GeometryContext _) =>
        ResidualDistances(samples: samples).Bind(static (Seq<double> residuals) => Many(key: ConformanceKey, values: residuals));
    private static Fin<Seq<double>> ResidualRms(Seq<ResidualSample> samples, GeometryContext _) =>
        ResidualDistances(samples: samples).Bind(static (Seq<double> residuals) => ResidualRmsDistances(residuals: residuals));
    private static Fin<Seq<bool>> ResidualWithinTolerance(Seq<ResidualSample> samples, GeometryContext context) =>
        ResidualDistances(samples: samples).Bind((Seq<double> residuals) => ResidualWithinToleranceDistances(
            residuals: residuals,
            context: context));
    private static Fin<Seq<ResidualProfile>> ResidualProfileProjection(Seq<ResidualSample> samples, GeometryContext context) =>
        ResidualDistances(samples: samples).Bind((Seq<double> residuals) => ResidualProfileDistances(
            residuals: residuals,
            context: context));
    private static Fin<Seq<ResidualSample>> ResidualMaximum(Seq<ResidualSample> samples, GeometryContext _) =>
        samples.Fold(
            initialState: (Count: 0, Best: default, Valid: true),
            f: static ((int Count, ResidualSample Best, bool Valid) state, ResidualSample sample) => (
                Count: state.Count + 1,
                Best: (state.Count, sample.Distance > state.Best.Distance) switch {
                    (0, _) or (_, true) => sample,
                    _ => state.Best,
                },
                Valid: state.Valid && sample.Distance >= 0.0 && RhinoMath.IsValidDouble(x: sample.Distance) && sample.Location.IsValid)) switch {
                    ( > 0, ResidualSample best, true) => Fin.Succ(Seq(best)),
                    _ => Fin.Fail<Seq<ResidualSample>>(ConformanceKey.InvalidResult()),
                };
    private static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples) =>
        samples.Fold(
            initialState: Fin.Succ(Seq<double>()),
            f: static (Fin<Seq<double>> current, ResidualSample sample) => sample switch {
                { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) =>
                    current.Map((Seq<double> values) => values.Add(distance)),
                _ => Fin.Fail<Seq<double>>(ConformanceKey.InvalidResult()),
            });
    private static Fin<Seq<double>> ResidualRmsDistances(Seq<double> residuals) =>
        residuals.Fold(
            initialState: (Count: 0, SumSquares: 0.0, Valid: true),
            f: static ((int Count, double SumSquares, bool Valid) state, double residual) => (
                Count: state.Count + 1,
                SumSquares: state.SumSquares + (residual * residual),
                Valid: state.Valid && residual >= 0.0 && RhinoMath.IsValidDouble(x: residual))) switch {
                    (int count, double sumSquares, true) when count > 0 && RhinoMath.IsValidDouble(x: sumSquares) =>
                        One(key: ConformanceKey, value: Math.Sqrt(d: sumSquares / count)),
                    _ => Fin.Fail<Seq<double>>(ConformanceKey.InvalidResult()),
                };
    private static Fin<Seq<bool>> ResidualWithinToleranceDistances(Seq<double> residuals, GeometryContext context) =>
        residuals.Fold(
            initialState: (Count: 0, Maximum: 0.0, Valid: true),
            f: static ((int Count, double Maximum, bool Valid) state, double residual) => (
                Count: state.Count + 1,
                Maximum: Math.Max(val1: state.Maximum, val2: residual),
                Valid: state.Valid && residual >= 0.0 && RhinoMath.IsValidDouble(x: residual))) switch {
                    (int count, double maximum, true) when count > 0 && RhinoMath.IsValidDouble(x: maximum) =>
                        One(key: ConformanceKey, value: maximum <= context.Absolute.Value),
                    _ => Fin.Fail<Seq<bool>>(ConformanceKey.InvalidResult()),
                };
    private static Fin<Seq<ResidualProfile>> ResidualProfileDistances(Seq<double> residuals, GeometryContext context) {
        (int count, double minimum, double maximum, double sum, double sumSquares, bool valid) = residuals.Fold(
            initialState: (Count: 0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, Sum: 0.0, SumSquares: 0.0, Valid: true),
            f: static ((int Count, double Minimum, double Maximum, double Sum, double SumSquares, bool Valid) state, double residual) => (
                Count: state.Count + 1,
                Minimum: Math.Min(val1: state.Minimum, val2: residual),
                Maximum: Math.Max(val1: state.Maximum, val2: residual),
                Sum: state.Sum + residual,
                SumSquares: state.SumSquares + (residual * residual),
                Valid: state.Valid && residual >= 0.0 && RhinoMath.IsValidDouble(x: residual)));
        double mean = count switch { > 0 => sum / count, _ => double.NaN };
        double variance = count switch {
            > 0 => residuals.Fold(
                initialState: (Mean: mean, Total: 0.0),
                f: static ((double Mean, double Total) state, double residual) => (
                    state.Mean,
                    state.Total + ((residual - state.Mean) * (residual - state.Mean)))).Total / count,
            _ => double.NaN,
        };
        double rms = count switch { > 0 => Math.Sqrt(d: sumSquares / count), _ => double.NaN };
        return (count, valid, RhinoMath.IsValidDouble(x: minimum), RhinoMath.IsValidDouble(x: maximum), RhinoMath.IsValidDouble(x: mean), RhinoMath.IsValidDouble(x: variance), RhinoMath.IsValidDouble(x: rms), variance >= 0.0) switch {
            ( > 0, true, true, true, true, true, true, true) => Fin.Succ(Seq(new ResidualProfile(
                Count: count,
                Minimum: minimum,
                Maximum: maximum,
                Mean: mean,
                Variance: variance,
                Rms: rms,
                Tolerance: context.Absolute.Value,
                WithinTolerance: maximum <= context.Absolute.Value))),
            _ => Fin.Fail<Seq<ResidualProfile>>(ConformanceKey.InvalidResult()),
        };
    }
    private readonly record struct ResidualState<TGeometry, TPrimitive>(
        Seq<ResidualSample> Samples,
        TGeometry Geometry,
        TPrimitive Primitive,
        GeometryContext Context) where TGeometry : notnull where TPrimitive : notnull;
    private static Eff<AnalysisRuntime, Seq<Point3d>> MassCentroid<TGeometry>(
        TGeometry geometry,
        GeometryRequirement requirement,
        Query<TGeometry, Point3d> query) where TGeometry : GeometryBase =>
        from rt in Analyze.Asks
        from validated in rt.Context.Validate(geometry: geometry, requirement: requirement).ToEff()
        from result in query.Apply(geometry: validated)
        select result;
    private static Query<TGeometry, Point3d> LengthCentroid<TGeometry>(string name) where TGeometry : notnull =>
        LengthMass<TGeometry, Point3d>(
            name: name,
            project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.Centroid),
            secondMoments: false);
    private static Query<TGeometry, Point3d> AreaCentroid<TGeometry>(string name) where TGeometry : notnull =>
        AreaMass<TGeometry, Point3d>(
            name: name,
            project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Centroid));
    private static Query<TGeometry, Point3d> VolumeCentroid<TGeometry>(string name) where TGeometry : notnull =>
        VolumeMass<TGeometry, Point3d>(
            name: name,
            project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Centroid));
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, LengthMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, AreaMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, VolumeMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Query<TGeometry, TOut> Box<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(BoundingBox)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(object)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)
                    || geometry == typeof(Box)
                    || geometry == typeof(Sphere)) =>
                Cast<TGeometry, TOut>(key: BoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: BoundsKey,
                    evaluator: static (TGeometry geometry) =>
                        ExtractBounds(geometry: geometry)
                            .Bind(static (BoundingBox box) => One(key: BoundsKey, value: box))
                            .ToEff())),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<BoundingBox> ExtractBounds<TGeometry>(TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            GeometryBase native => Fin.Succ(native.GetBoundingBox(accurate: true)),
            Line line => Fin.Succ(line.BoundingBox),
            Polyline polyline => Fin.Succ(polyline.BoundingBox),
            BoundingBox box => Fin.Succ(box),
            Box box => Fin.Succ(box.BoundingBox),
            Sphere sphere => Fin.Succ(sphere.BoundingBox),
            _ => Fin.Fail<BoundingBox>(BoundsKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(BoundingBox))),
        };
    private static Query<TGeometry, TOut> Oriented<TGeometry, TOut>(Plane plane) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                Cast<TGeometry, TOut>(key: OrientedBoundsKey, query: Query<TGeometry, Box>.Build(
                    key: OrientedBoundsKey,
                    state: plane,
                    evaluator: static (Plane orientation, TGeometry geometry) => (geometry switch {
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
                    evaluator: static (Transform xform, TGeometry geometry) => (geometry switch {
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
                    evaluator: static (Line geometry) => One(key: LengthKey, value: geometry.Length).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Polyline, double>.Build(
                    key: LengthKey,
                    evaluator: static (Polyline geometry) => One(key: LengthKey, value: geometry.Length).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<TGeometry, double>.Build(
                    key: LengthKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry) => geometry switch {
                        Curve curve =>
                            from rt in Analyze.Asks
                            from result in One(
                                key: LengthKey,
                                value: curve.GetLength(fractionalTolerance: rt.Context.Relative.Value)).ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<double>>.Fail(error: LengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            _ => LengthKey.Unsupported<TGeometry, TOut>(),
        };
}
