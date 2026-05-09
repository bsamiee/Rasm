using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

public static partial class Query {
    public static Query<BoundingBox, Point3d> UniqueCorners() =>
        Query<BoundingBox, Point3d>.Build(
            key: UniqueCornersKey,
            requiresContext: true,
            evaluator: static (BoundingBox bbox) =>
                from ctx in Analyze.Asks
                from result in DedupeCorners(bbox: bbox, tolerance: ctx.Absolute.Value).ToEff()
                select result);
    public static Query<TGeometry, TOut> BoundingCorners<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: UniqueCornersKey, query: Query<TGeometry, Point3d>.Build(
                key: UniqueCornersKey,
                requiresContext: true,
                evaluator: static (TGeometry geom) =>
                    from ctx in Analyze.Asks
                    from bbox in BoundingBoxOf(geom: geom).ToEff()
                    from result in DedupeCorners(bbox: bbox, tolerance: ctx.Absolute.Value).ToEff()
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
            true => Optional(Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: tolerance))
                .ToFin(UniqueCornersKey.InvalidResult())
                .Map(static (Point3d[] unique) => toSeq(unique)),
            false => Fin.Fail<Seq<Point3d>>(UniqueCornersKey.InvalidInput()),
        };
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull =>
        Aspect<TGeometry, TOut, Bounds>(
            aspect: aspect,
            key: BoundsKey,
            dispatch: static (Bounds candidate) => candidate switch {
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
                _ => null,
            });
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull =>
        Aspect<TGeometry, TOut, Measure>(
            aspect: aspect,
            key: MeasureKey,
            dispatch: static (Measure candidate) => candidate switch {
                Analysis.Measure.SpatialMidpoint when typeof(TOut) == typeof(Point3d) => SpatialMidpoint<TGeometry, TOut>(),
                Analysis.Measure.Length => Length<TGeometry, TOut>(),
                Analysis.Measure.Area when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Area)), query: Mass<TGeometry, AreaMassProperties, double>(name: nameof(Analysis.Measure.Area), requirement: GeometryRequirement.AreaMass, compute: ComputeArea, project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Area))),
                Analysis.Measure.Volume when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Volume)), query: Mass<TGeometry, VolumeMassProperties, double>(name: nameof(Analysis.Measure.Volume), requirement: GeometryRequirement.VolumeMass, compute: ComputeVolume, project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Volume))),
                Analysis.Measure.Error e => MassMeasure<TGeometry, TOut>(mass: e.Mass, kind: e),
                Analysis.Measure.Centroid c => MassMeasure<TGeometry, TOut>(mass: c.Mass, kind: c),
                Analysis.Measure.CentroidError ce => MassMeasure<TGeometry, TOut>(mass: ce.Mass, kind: ce),
                Analysis.Measure.Radii r => MassMeasure<TGeometry, TOut>(mass: r.Mass, kind: r),
                Analysis.Measure.Principal p => MassMeasure<TGeometry, TOut>(mass: p.Mass, kind: p),
                _ => null,
            });
    private static Query<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure kind) where TGeometry : notnull =>
        (kind, mass) switch {
            (_, MassKind.None) => Query<TGeometry, TOut>.Reject(key: MeasureKey, fault: MeasureKey.InvalidInput()),
            _ => MassDispatch<TGeometry, TOut>.Table
                .Find(key: (Case: kind.GetType(), Mass: mass))
                .Match(
                    Some: static (Func<Query<TGeometry, TOut>> build) => build(),
                    None: static () => MeasureKey.Unsupported<TGeometry, TOut>()),
        };
    private static class MassDispatch<TGeometry, TOut> where TGeometry : notnull {
        internal static readonly Map<(Type Case, MassKind Mass), Func<Query<TGeometry, TOut>>> Table = Map(
            ((typeof(Analysis.Measure.Error), MassKind.Length), () => typeof(TOut) != typeof(double) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthError"), query: Mass<TGeometry, LengthMassProperties, double>(name: "LengthError", requirement: GeometryRequirement.CurveLength, compute: ComputeLength, project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.LengthError), secondMoments: true))),
            ((typeof(Analysis.Measure.Error), MassKind.Area), () => typeof(TOut) != typeof(double) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaError"), query: Mass<TGeometry, AreaMassProperties, double>(name: "AreaError", requirement: GeometryRequirement.AreaMass, compute: ComputeArea, project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.AreaError)))),
            ((typeof(Analysis.Measure.Error), MassKind.Volume), () => typeof(TOut) != typeof(double) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeError"), query: Mass<TGeometry, VolumeMassProperties, double>(name: "VolumeError", requirement: GeometryRequirement.VolumeMass, compute: ComputeVolume, project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.VolumeError)))),
            ((typeof(Analysis.Measure.Centroid), MassKind.Length), () => typeof(TOut) != typeof(Point3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroid"), query: Mass<TGeometry, LengthMassProperties, Point3d>(name: "LengthCentroid", requirement: GeometryRequirement.CurveLength, compute: ComputeLength, project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.Centroid), secondMoments: true))),
            ((typeof(Analysis.Measure.Centroid), MassKind.Area), () => typeof(TOut) != typeof(Point3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroid"), query: Mass<TGeometry, AreaMassProperties, Point3d>(name: "AreaCentroid", requirement: GeometryRequirement.AreaMass, compute: ComputeArea, project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Centroid)))),
            ((typeof(Analysis.Measure.Centroid), MassKind.Volume), () => typeof(TOut) != typeof(Point3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroid"), query: Mass<TGeometry, VolumeMassProperties, Point3d>(name: "VolumeCentroid", requirement: GeometryRequirement.VolumeMass, compute: ComputeVolume, project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Centroid)))),
            ((typeof(Analysis.Measure.CentroidError), MassKind.Length), () => typeof(TOut) != typeof(Vector3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroidError"), query: Mass<TGeometry, LengthMassProperties, Vector3d>(name: "LengthCentroidError", requirement: GeometryRequirement.CurveLength, compute: ComputeLength, project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidError), secondMoments: true))),
            ((typeof(Analysis.Measure.CentroidError), MassKind.Area), () => typeof(TOut) != typeof(Vector3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroidError"), query: Mass<TGeometry, AreaMassProperties, Vector3d>(name: "AreaCentroidError", requirement: GeometryRequirement.AreaMass, compute: ComputeArea, project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidError)))),
            ((typeof(Analysis.Measure.CentroidError), MassKind.Volume), () => typeof(TOut) != typeof(Vector3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroidError"), query: Mass<TGeometry, VolumeMassProperties, Vector3d>(name: "VolumeCentroidError", requirement: GeometryRequirement.VolumeMass, compute: ComputeVolume, project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidError)))),
            ((typeof(Analysis.Measure.Radii), MassKind.Length), () => typeof(TOut) != typeof(Vector3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthRadii"), query: Mass<TGeometry, LengthMassProperties, Vector3d>(name: "LengthRadii", requirement: GeometryRequirement.CurveLength, compute: ComputeLength, project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true))),
            ((typeof(Analysis.Measure.Radii), MassKind.Area), () => typeof(TOut) != typeof(Vector3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaRadii"), query: Mass<TGeometry, AreaMassProperties, Vector3d>(name: "AreaRadii", requirement: GeometryRequirement.AreaMass, compute: ComputeArea, project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true))),
            ((typeof(Analysis.Measure.Radii), MassKind.Volume), () => typeof(TOut) != typeof(Vector3d) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeRadii"), query: Mass<TGeometry, VolumeMassProperties, Vector3d>(name: "VolumeRadii", requirement: GeometryRequirement.VolumeMass, compute: ComputeVolume, project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true))),
            ((typeof(Analysis.Measure.Principal), MassKind.Length), () => typeof(TOut) != typeof(ValueTuple<double, Vector3d>) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthPrincipal"), query: Mass<TGeometry, LengthMassProperties, (double Moment, Vector3d Axis)>(name: "LengthPrincipal", requirement: GeometryRequirement.CurveLength, compute: ComputeLength, project: static (OperationKey key, LengthMassProperties mass) => key.Principal(mass: mass), secondMoments: true, productMoments: true))),
            ((typeof(Analysis.Measure.Principal), MassKind.Area), () => typeof(TOut) != typeof(ValueTuple<double, Vector3d>) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaPrincipal"), query: Mass<TGeometry, AreaMassProperties, (double Moment, Vector3d Axis)>(name: "AreaPrincipal", requirement: GeometryRequirement.AreaMass, compute: ComputeArea, project: static (OperationKey key, AreaMassProperties mass) => key.Principal(mass: mass), secondMoments: true, productMoments: true))),
            ((typeof(Analysis.Measure.Principal), MassKind.Volume), () => typeof(TOut) != typeof(ValueTuple<double, Vector3d>) ? MeasureKey.Unsupported<TGeometry, TOut>() : Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumePrincipal"), query: Mass<TGeometry, VolumeMassProperties, (double Moment, Vector3d Axis)>(name: "VolumePrincipal", requirement: GeometryRequirement.VolumeMass, compute: ComputeVolume, project: static (OperationKey key, VolumeMassProperties mass) => key.Principal(mass: mass), secondMoments: true, productMoments: true))));
    }
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
                            from ctx in Analyze.Asks
                            from result in (curve.IsClosed, curve.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value)) switch {
                                (true, true) => MassCentroid(geometry: curve, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Curve>(name: SpatialMidpointKey.Name)),
                                _ => MassCentroid(geometry: curve, requirement: GeometryRequirement.CurveLength, query: LengthCentroid<Curve>(name: SpatialMidpointKey.Name)),
                            }
                            select result,
                        Brep { IsSolid: true } brep => MassCentroid(geometry: brep, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Brep>(name: SpatialMidpointKey.Name)),
                        Brep brep => MassCentroid(geometry: brep, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Brep>(name: SpatialMidpointKey.Name)),
                        Mesh { IsSolid: true } mesh => MassCentroid(geometry: mesh, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Mesh>(name: SpatialMidpointKey.Name)),
                        Mesh mesh => MassCentroid(geometry: mesh, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Mesh>(name: SpatialMidpointKey.Name)),
                        Surface { IsSolid: true } surface => MassCentroid(geometry: surface, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Surface>(name: SpatialMidpointKey.Name)),
                        Surface surface => MassCentroid(geometry: surface, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Surface>(name: SpatialMidpointKey.Name)),
                        SubD subd =>
                            from ctx in Analyze.Asks
                            from validated in ctx.Validate(geometry: subd, requirement: GeometryRequirement.Basic).ToEff()
                            from brep in Optional(validated.ToBrep()).ToFin(SpatialMidpointKey.InvalidResult()).ToEff()
                            from result in SubDBrepSpatialMidpoint(brep: brep)
                            select result,
                        BoundingBox box => One(key: SpatialMidpointKey, value: box.Center).ToEff(),
                        Box box => One(key: SpatialMidpointKey, value: box.Center).ToEff(),
                        _ => Fin.Fail<Seq<Point3d>>(SpatialMidpointKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => SpatialMidpointKey.Unsupported<TGeometry, TOut>(),
        };
    // SubD.ToBrep() yields a fresh Brep that must be disposed before MassCentroid finalises;
    // the using-local is intrinsic to this SubD-to-Brep boundary translation.
    [BoundaryImperativeExemption(
        ruleId: "CSP0001",
        reason: BoundaryImperativeReason.CleanupFinally,
        ticket: "RASM-WAVE4",
        expiresOnUtc: "2027-12-31T00:00:00Z")]
    private static Eff<GeometryContext, Seq<Point3d>> SubDBrepSpatialMidpoint(Brep brep) {
        using Brep disposable = brep;
        return disposable.IsSolid switch {
            true => MassCentroid(geometry: disposable, requirement: GeometryRequirement.VolumeMass, query: VolumeCentroid<Brep>(name: SpatialMidpointKey.Name)),
            false => MassCentroid(geometry: disposable, requirement: GeometryRequirement.AreaMass, query: AreaCentroid<Brep>(name: SpatialMidpointKey.Name)),
        };
    }
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(
        Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        Aspect<(TGeometry Geometry, TPrimitive Primitive), TOut, Conformance>(
            aspect: aspect,
            key: ConformanceKey,
            dispatch: static (Conformance candidate) => (candidate.Residual, candidate.Count, typeof(TGeometry), typeof(TPrimitive), typeof(TOut)) switch {
                (ConformanceResidual.None, _, _, _, _) or (_, <= 0, _, _, _) => Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(
                    key: ConformanceKey,
                    fault: ConformanceKey.InvalidInput()),
                (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Line) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Line>(
                        aspect: candidate,
                        requirement: GeometryRequirement.CurveLength,
                        samples: CurveLineSamples),
                (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Circle) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Circle>(
                        aspect: candidate,
                        requirement: GeometryRequirement.CurveLength,
                        samples: CurveCircleSamples),
                (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Arc) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Arc>(
                        aspect: candidate,
                        requirement: GeometryRequirement.CurveLength,
                        samples: CurveArcSamples),
                (_, _, Type geometry, Type primitive, _) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Plane) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Plane>(
                        aspect: candidate,
                        requirement: GeometryRequirement.SurfaceEvaluation,
                        samples: SurfacePlaneSamples),
                (_, _, Type geometry, Type primitive, _) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Sphere) =>
                    ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Sphere>(
                        aspect: candidate,
                        requirement: GeometryRequirement.SurfaceEvaluation,
                        samples: SurfaceSphereSamples),
                _ => null,
            });
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceCases<TGeometry, TPrimitive, TOut, TNativeGeometry, TNativePrimitive>(
        Conformance aspect,
        GeometryRequirement requirement,
        Func<TNativeGeometry, TNativePrimitive, int, GeometryContext, Fin<Seq<ResidualSample>>> samples) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
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
        Func<TNativeGeometry, TNativePrimitive, int, GeometryContext, Fin<Seq<ResidualSample>>> samples,
        Func<Seq<ResidualSample>, GeometryContext, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: ConformanceKey,
            requiresContext: true,
            state: (Count: count, Requirement: requirement, Samples: samples, Project: project),
            evaluator: static ((int Count, GeometryRequirement Requirement, Func<TNativeGeometry, TNativePrimitive, int, GeometryContext, Fin<Seq<ResidualSample>>> Samples, Func<Seq<ResidualSample>, GeometryContext, Fin<Seq<TValue>>> Project) state, (TGeometry Geometry, TPrimitive Primitive) geometry) =>
                from ctx in Analyze.Asks
                from validated in ctx.Validate(
                        shape: new GeometryShape<TGeometry, TPrimitive>.Pair(
                            A: geometry.Geometry,
                            B: geometry.Primitive,
                            RequirementA: state.Requirement,
                            RequirementB: GeometryRequirement.None))
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
        GeometryContext context,
        int count,
        Func<TNativeGeometry, TNativePrimitive, int, GeometryContext, Fin<Seq<ResidualSample>>> samples,
        Func<Seq<ResidualSample>, GeometryContext, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        (geometry.Geometry, geometry.Primitive) switch {
            (TNativeGeometry native, TNativePrimitive primitive) => samples(
                    arg1: native,
                    arg2: primitive,
                    arg3: count,
                    arg4: context)
                .Bind((Seq<ResidualSample> values) => project(
                    arg1: values,
                    arg2: context)),
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
        Func<TPrimitive, Point3d, Point3d> closest) where TPrimitive : notnull =>
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
                                            arg1: pair.State.Primitive,
                                            arg2: point) switch {
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
        Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
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
                            Distance: distance(arg1: state.Primitive, arg2: point),
                            Tolerance: state.Context.Absolute.Value,
                            WithinTolerance: distance(arg1: state.Primitive, arg2: point) <= state.Context.Absolute.Value)),
                    },
                })))
        .Map(static (ResidualState<Surface, TPrimitive> state) => state.Samples);
    private static Fin<Seq<double>> ResidualDistance(Seq<ResidualSample> samples, GeometryContext _) =>
        ResidualDistances(samples: samples).Bind(static (Seq<double> residuals) => Many(key: ConformanceKey, values: residuals));
    private static Fin<Seq<double>> ResidualRms(Seq<ResidualSample> samples, GeometryContext _) =>
        ResidualDistances(samples: samples)
            .Bind(static (Seq<double> residuals) => Stats.From(values: residuals, key: ConformanceKey))
            .Bind(static (Stats s) => One(key: ConformanceKey, value: s.Rms));
    private static Fin<Seq<bool>> ResidualWithinTolerance(Seq<ResidualSample> samples, GeometryContext context) =>
        ResidualDistances(samples: samples)
            .Bind(static (Seq<double> residuals) => Stats.From(values: residuals, key: ConformanceKey))
            .Bind((Stats s) => One(key: ConformanceKey, value: s.Maximum <= context.Absolute.Value));
    private static Fin<Seq<ResidualProfile>> ResidualProfileProjection(Seq<ResidualSample> samples, GeometryContext context) =>
        ResidualDistances(samples: samples)
            .Bind(static (Seq<double> residuals) => Stats.From(values: residuals, key: ConformanceKey))
            .Bind((Stats s) => One(key: ConformanceKey, value: new ResidualProfile(
                Count: s.Count,
                Minimum: s.Minimum,
                Maximum: s.Maximum,
                Mean: s.Mean,
                Variance: s.Variance,
                Rms: s.Rms,
                Tolerance: context.Absolute.Value,
                WithinTolerance: s.Maximum <= context.Absolute.Value)));
    private static Fin<Seq<ResidualSample>> ResidualMaximum(Seq<ResidualSample> samples, GeometryContext _) =>
        samples.TraverseM(static (ResidualSample sample) => sample switch {
            { Distance: double d, Location.IsValid: true } when d >= 0.0 && RhinoMath.IsValidDouble(x: d) => Fin.Succ(sample),
            _ => Fin.Fail<ResidualSample>(ConformanceKey.InvalidResult()),
        }).As()
            .Bind(static (Seq<ResidualSample> validated) => validated.MaxesBy(
                    projection: static (ResidualSample sample) => sample.Distance,
                    tolerance: 0.0)
                .Head
                .Match(
                    Some: static (ResidualSample best) => Fin.Succ(Seq(best)),
                    None: static () => Fin.Fail<Seq<ResidualSample>>(ConformanceKey.InvalidResult())));
    private static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples) =>
        samples.TraverseM(static (ResidualSample sample) => sample switch {
            { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(distance),
            _ => Fin.Fail<double>(ConformanceKey.InvalidResult()),
        }).As();
    private readonly record struct ResidualState<TGeometry, TPrimitive>(
        Seq<ResidualSample> Samples,
        TGeometry Geometry,
        TPrimitive Primitive,
        GeometryContext Context) where TGeometry : notnull where TPrimitive : notnull;
    private static Eff<GeometryContext, Seq<Point3d>> MassCentroid<TGeometry>(
        TGeometry geometry,
        GeometryRequirement requirement,
        Query<TGeometry, Point3d> query) where TGeometry : GeometryBase =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: geometry, requirement: requirement).ToEff()
        from result in query.Apply(geometry: validated)
        select result;
    private static Query<TGeometry, Point3d> LengthCentroid<TGeometry>(string name) where TGeometry : notnull =>
        Mass<TGeometry, LengthMassProperties, Point3d>(
            name: name,
            requirement: GeometryRequirement.CurveLength,
            compute: ComputeLength,
            project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.Centroid));
    private static Query<TGeometry, Point3d> AreaCentroid<TGeometry>(string name) where TGeometry : notnull =>
        Mass<TGeometry, AreaMassProperties, Point3d>(
            name: name,
            requirement: GeometryRequirement.AreaMass,
            compute: ComputeArea,
            project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Centroid));
    private static Query<TGeometry, Point3d> VolumeCentroid<TGeometry>(string name) where TGeometry : notnull =>
        Mass<TGeometry, VolumeMassProperties, Point3d>(
            name: name,
            requirement: GeometryRequirement.VolumeMass,
            compute: ComputeVolume,
            project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Centroid));
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
