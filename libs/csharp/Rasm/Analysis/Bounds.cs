namespace Rasm.Analysis;

// --- [OPERATIONS] ------------------------------------------------------------------------
public static partial class Query {
    public static Query<BoundingBox, Point3d> UniqueCorners() => BoundingCorners<BoundingBox, Point3d>();
    public static Query<TGeometry, TOut> BoundingCorners<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Point3d)
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => from context in Analyze.Asks
                                                    from bbox in geometry.Bounds(op: op).ToEff()
                                                    from result in (bbox.IsValid switch {
                                                        true => Many(key: op, values: Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: context.Absolute.Value)),
                                                        false => Fin.Fail<Seq<Point3d>>(op.InvalidInput()),
                                                    }).ToEff()
                                                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return aspect switch {
            null => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => aspect.Apply<TGeometry, TOut>(),
        };
    }
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return aspect switch {
            null => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => aspect.Apply<TGeometry, TOut>(),
        };
    }
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(Conformance aspect) where TGeometry : notnull where TPrimitive : notnull {
        Op key = Op.Of();
        return aspect switch {
            null => Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => aspect.Apply<TGeometry, TPrimitive, TOut>(),
        };
    }
    public static Query<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut) == typeof(Point3d) && typeof(TGeometry).SupportsBounds(includeSphere: false))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => from context in Analyze.Asks
                                                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                                                    from centroid in kind.Centroid(value: geometry, ctx: context, op: op).ToEff()
                                                    from result in One(key: op, value: centroid).ToEff()
                                                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Query<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure kind) where TGeometry : notnull {
        Op key = Op.Of();
        return (kind, typeof(TOut)) switch {
            (_, _) when mass.Equals(MassKind.None) => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (Analysis.Measure.Area, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: MassKind.Area, suffix: string.Empty, project: static (k, props) => props switch {
                AreaMassProperties area => One(key: k, value: area.Area),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.Volume, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: MassKind.Volume, suffix: string.Empty, project: static (k, props) => props switch {
                VolumeMassProperties volume => One(key: k, value: volume.Volume),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.MassError, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: mass, suffix: "Error", project: (k, props) => MassProject<double>(kind: kind, key: k, props: props), secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.Centroid, Type output) when output == typeof(Point3d) => MassCast<TGeometry, TOut, Point3d>(mass: mass, suffix: "Centroid", project: (k, props) => MassProject<Point3d>(kind: kind, key: k, props: props), secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.CentroidError, Type output) when output == typeof(Vector3d) => MassCast<TGeometry, TOut, Vector3d>(mass: mass, suffix: "CentroidError", project: (k, props) => MassProject<Vector3d>(kind: kind, key: k, props: props), secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.Radii, Type output) when output == typeof(Vector3d) => MassCast<TGeometry, TOut, Vector3d>(mass: mass, suffix: "Radii", project: (k, props) => MassProject<Vector3d>(kind: kind, key: k, props: props), secondMoments: true),
            (Analysis.Measure.PrincipalAxes, Type output) when output == typeof(ValueTuple<double, Vector3d>) => MassCast<TGeometry, TOut, (double Moment, Vector3d Axis)>(mass: mass, suffix: "Principal", project: (k, props) => MassProject<(double Moment, Vector3d Axis)>(kind: kind, key: k, props: props), secondMoments: true, productMoments: true),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static Query<TGeometry, TOut> MassCast<TGeometry, TOut, TValue>(MassKind mass, string suffix, Func<Op, IDisposable, Fin<Seq<TValue>>> project, bool secondMoments = false, bool productMoments = false) where TGeometry : notnull {
        Op key = Op.Of(name: $"{mass.Label}{suffix}");
        return Cast<TGeometry, TOut>(key: key, query: mass.Build<TGeometry, TValue>(key: key, project: project, secondMoments: secondMoments, productMoments: productMoments));
    }
    private static Fin<Seq<TValue>> MassProject<TValue>(Measure kind, Op key, IDisposable props) =>
        kind switch {
            Analysis.Measure.MassError => key.PickMass<double, TValue>(props: props, length: static l => l.LengthError, area: static a => a.AreaError, volume: static v => v.VolumeError),
            Analysis.Measure.Centroid => key.PickMass<Point3d, TValue>(props: props, length: static l => l.Centroid, area: static a => a.Centroid, volume: static v => v.Centroid),
            Analysis.Measure.CentroidError => key.PickMass<Vector3d, TValue>(props: props, length: static l => l.CentroidError, area: static a => a.CentroidError, volume: static v => v.CentroidError),
            Analysis.Measure.Radii => key.PickMass<Vector3d, TValue>(props: props, length: static l => l.CentroidCoordinatesRadiiOfGyration, area: static a => a.CentroidCoordinatesRadiiOfGyration, volume: static v => v.CentroidCoordinatesRadiiOfGyration),
            Analysis.Measure.PrincipalAxes => key.Principal(mass: props).Bind(values => key.Results<(double Moment, Vector3d Axis), TValue>(values: values)),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    private static Fin<Seq<TValue>> PickMass<TProp, TValue>(this Op key, IDisposable props, Func<LengthMassProperties, TProp> length, Func<AreaMassProperties, TProp> area, Func<VolumeMassProperties, TProp> volume) =>
        props switch {
            LengthMassProperties l => key.Results<TProp, TValue>(values: Seq(length(arg: l))),
            AreaMassProperties a => key.Results<TProp, TValue>(values: Seq(area(arg: a))),
            VolumeMassProperties v => key.Results<TProp, TValue>(values: Seq(volume(arg: v))),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    internal static Query<TGeometry, TOut> BoxMetric<TGeometry, TOut>(Op key, Func<BoundingBox, double> boundingBox, Func<Box, double> box) where TGeometry : notnull =>
        (typeof(TOut) == typeof(double), typeof(TGeometry)) switch {
            (true, Type geometry) when geometry == typeof(BoundingBox) => Cast<TGeometry, TOut>(key: key, query: Query<BoundingBox, double>.Build(
                key: key, state: (Key: key, Project: boundingBox),
                evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
            (true, Type geometry) when geometry == typeof(Box) => Cast<TGeometry, TOut>(key: key, query: Query<Box, double>.Build(
                key: key, state: (Key: key, Project: box),
                evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    internal static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut) == typeof(double) && (typeof(TGeometry) == typeof(Line) || typeof(TGeometry) == typeof(Polyline) || typeof(Curve).IsAssignableFrom(c: typeof(TGeometry))))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, double>.Build(
                key: key,
                requirement: typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) ? Requirement.CurveLength : Requirement.None,
                state: key,
                evaluator: static (op, geometry) => geometry switch {
                    Line line => op.RequireValid(value: line).Bind(validated => One(key: op, value: validated.Length)).ToEff(),
                    Polyline polyline => op.RequireValid(value: polyline).Bind(validated => One(key: op, value: validated.Length)).ToEff(),
                    Curve curve => from context in Analyze.Asks
                                   from result in One(key: op, value: curve.GetLength(fractionalTolerance: context.Relative.Value)).ToEff()
                                   select result,
                    _ => Fin.Fail<Seq<double>>(op.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))).ToEff(),
                }))
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceCases<TGeometry, TPrimitive, TOut, TNativeGeometry, TNativePrimitive>(
        Conformance aspect, Requirement requirement,
        Func<TNativeGeometry, TNativePrimitive, int, Context, Fin<Seq<ResidualSample>>> samples) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull {
        Op key = Op.Of(name: nameof(Conformance));
        return (aspect, typeof(TOut)) switch {
            (Conformance.Distance item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: key, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                count: item.Count, requirement: requirement, samples: samples, project: static (residuals, _) => ResidualDistances(key: Op.Of(name: nameof(Conformance)), samples: residuals).Bind(values => Many(key: Op.Of(name: nameof(Conformance)), values: values)))),
            (Conformance.Rms item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: key, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                count: item.Count, requirement: requirement, samples: samples, project: static (residuals, _) => ResidualDistances(key: Op.Of(name: nameof(Conformance)), samples: residuals).Bind(values => Stats.From(values: values, key: Op.Of(name: nameof(Conformance)))).Bind(stats => One(key: Op.Of(name: nameof(Conformance)), value: stats.Rms)))),
            (Conformance.WithinTolerance item, Type output) when output == typeof(bool) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: key, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, bool>(
                count: item.Count, requirement: requirement, samples: samples, project: static (residuals, context) => ResidualDistances(key: Op.Of(name: nameof(Conformance)), samples: residuals).Bind(values => Stats.From(values: values, key: Op.Of(name: nameof(Conformance)))).Bind(stats => One(key: Op.Of(name: nameof(Conformance)), value: stats.Maximum <= context.Absolute.Value)))),
            (Conformance.ProfileResidual item, Type output) when output == typeof(ResidualProfile) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: key, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualProfile>(
                count: item.Count, requirement: requirement, samples: samples, project: static (residuals, context) => ResidualDistances(key: Op.Of(name: nameof(Conformance)), samples: residuals)
                    .Bind(values => Stats.From(values: values, key: Op.Of(name: nameof(Conformance))))
                    .Bind(stats => One(key: Op.Of(name: nameof(Conformance)), value: new ResidualProfile(Count: stats.Count, Minimum: stats.Minimum, Maximum: stats.Maximum, Mean: stats.Mean, Variance: stats.Variance, Rms: stats.Rms, Tolerance: context.Absolute.Value, WithinTolerance: stats.Maximum <= context.Absolute.Value))))),
            (Conformance.Maximum item, Type output) when output == typeof(ResidualSample) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: key, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualSample>(
                count: item.Count, requirement: requirement, samples: samples, project: static (residuals, _) => residuals
                    .TraverseM(static sample => sample switch {
                        { Distance: double d, Location.IsValid: true } when d >= 0.0 && RhinoMath.IsValidDouble(x: d) => Fin.Succ(sample),
                        _ => Fin.Fail<ResidualSample>(Op.Of(name: nameof(Conformance)).InvalidResult()),
                    })
                    .As()
                    .Bind(values => values.Maxima(projection: static sample => sample.Distance, tolerance: 0.0).Head.ToFin(Op.Of(name: nameof(Conformance)).InvalidResult()).Map(static best => Seq(best))))),
            _ => key.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    }
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TValue> ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, TValue>(
        int count, Requirement requirement,
        Func<TNativeGeometry, TNativePrimitive, int, Context, Fin<Seq<ResidualSample>>> samples,
        Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: Op.Of(name: nameof(Conformance)), requiresContext: true,
            state: (Op: Op.Of(name: nameof(Conformance)), Count: count, Requirement: requirement, Samples: samples, Project: project),
            evaluator: static (state, geometry) => from context in Analyze.Asks
                                                   from validated in context.ValidatePair(a: geometry.Geometry, b: geometry.Primitive, requirementA: state.Requirement, requirementB: Requirement.None)
                                                       .ToEff()
                                                   from result in ((validated.A, validated.B) switch {
                                                       (TNativeGeometry native, TNativePrimitive primitive) => state.Samples(arg1: native, arg2: primitive, arg3: state.Count, arg4: context).Bind(values => state.Project(arg1: values, arg2: context)),
                                                       _ => Fin.Fail<Seq<TValue>>(state.Op.Unsupported(geometryType: typeof((TGeometry Geometry, TPrimitive Primitive)), outputType: typeof(TValue))),
                                                   }).ToEff()
                                                   select result);
    internal static Fin<Seq<ResidualSample>> CurvePrimitiveSamples<TPrimitive>(Curve geometry, TPrimitive primitive, int count, Context context, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull {
        Op key = Op.Of(name: nameof(Conformance));
        return Fractions(count: count, key: key)
            .Bind(fractions => Optional(geometry.NormalizedLengthParameters(s: [.. fractions.AsIterable()], absoluteTolerance: context.Absolute.Value, fractionalTolerance: context.Relative.Value))
                .ToFin(key.InvalidResult())
                .Map(parameters => toSeq(parameters).Map(geometry.PointAt)))
            .Bind(points => ResidualSamples(points: points, primitive: primitive, context: context, distance: distance));
    }
    internal static Fin<Seq<ResidualSample>> SurfacePrimitiveSamples<TPrimitive>(Surface geometry, TPrimitive primitive, int count, Context context, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull {
        Op key = Op.Of(name: nameof(Conformance));
        return (Samples(domain: geometry.Domain(direction: 0), count: count, key: key),
                Samples(domain: geometry.Domain(direction: 1), count: count, key: key))
            .Apply(static (u, v) => (U: u, V: v)).As()
            .Bind(samples => ResidualSamples(points: samples.U.Bind(u => samples.V.Map(v => geometry.PointAt(u: u, v: v))), primitive: primitive, context: context, distance: distance));
    }
    private static Fin<Seq<ResidualSample>> ResidualSamples<TPrimitive>(Seq<Point3d> points, TPrimitive primitive, Context context, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        Fin.Succ(toSeq(points.AsIterable().Select((point, index) => new ResidualSample(
            Index: index, Location: point,
            Distance: distance(arg1: primitive, arg2: point),
            Tolerance: context.Absolute.Value,
            WithinTolerance: distance(arg1: primitive, arg2: point) <= context.Absolute.Value))));
    private static Fin<Seq<double>> ResidualDistances(Op key, Seq<ResidualSample> samples) =>
        samples.TraverseM(sample => sample switch {
            { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(distance),
            _ => Fin.Fail<double>(key.InvalidResult()),
        }).As();
}

// --- [BOUNDS_ROLE] -----------------------------------------------------------------------
public static class BoundsRole {
    extension(Bounds aspect) {
        internal Query<TGeometry, TOut> Apply<TGeometry, TOut>() where TGeometry : notnull => aspect.Switch(
            box: static _ => (typeof(TOut) == typeof(BoundingBox) && typeof(TGeometry).SupportsBounds(includeSphere: true))
                ? Query.Cast<TGeometry, TOut>(key: Op.Of(name: nameof(Bounds)), query: Query<TGeometry, BoundingBox>.Build(
                    key: Op.Of(name: nameof(Bounds)), state: Op.Of(name: nameof(Bounds)),
                    evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(box => Query.One(key: op, value: box)).ToEff()))
                : Op.Of(name: nameof(Bounds)).Unsupported<TGeometry, TOut>(),
            oriented: static o => (typeof(TOut) == typeof(Box) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
                ? Query.Native<TGeometry, TOut, GeometryBase, Box, (Op Key, Plane Plane)>(
                    key: Op.Of(name: "OrientedBounds"), state: (Op.Of(name: "OrientedBounds"), o.Plane),
                    project: static (state, native) => (native.GetBoundingBox(plane: state.Plane, worldBox: out Box box) switch {
                        BoundingBox local when local.IsValid => Query.One(key: state.Key, value: box),
                        _ => Fin.Fail<Seq<Box>>(state.Key.InvalidResult()),
                    }).ToEff())
                : Op.Of(name: "OrientedBounds").Unsupported<TGeometry, TOut>(),
            transformed: static t => (typeof(TOut) == typeof(BoundingBox) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
                ? Query.Native<TGeometry, TOut, GeometryBase, BoundingBox, (Op Key, Transform Xform)>(
                    key: Op.Of(name: "TransformedBounds"), state: (Key: Op.Of(name: "TransformedBounds"), Xform: t.Transform),
                    project: static (state, native) => Query.One(key: state.Key, value: native.GetBoundingBox(xform: state.Xform)).ToEff())
                : Op.Of(name: "TransformedBounds").Unsupported<TGeometry, TOut>(),
            center: static _ => typeof(TOut) == typeof(Point3d)
                ? Query.Cast<TGeometry, TOut>(key: Op.Of(name: "BoundsCenter"), query: Query<TGeometry, Point3d>.Build(
                    key: Op.Of(name: "BoundsCenter"), state: Op.Of(name: "BoundsCenter"),
                    evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(box => Query.One(key: op, value: box.Center)).ToEff()))
                : Op.Of(name: "BoundsCenter").Unsupported<TGeometry, TOut>(),
            corners: static _ => typeof(TOut) == typeof(Point3d)
                ? Query.Cast<TGeometry, TOut>(key: Op.Of(name: "BoundsCorners"), query: Query<TGeometry, Point3d>.Build(
                    key: Op.Of(name: "BoundsCorners"), state: Op.Of(name: "BoundsCorners"),
                    evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(box => Query.Many(key: op, values: box.GetCorners())).ToEff()))
                : Op.Of(name: "BoundsCorners").Unsupported<TGeometry, TOut>(),
            edges: static _ => (typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line))
                ? Query.Cast<TGeometry, TOut>(key: Op.Of(name: "BoxEdges"), query: Query<BoundingBox, Line>.Build(
                    key: Op.Of(name: "BoxEdges"), state: Op.Of(name: "BoxEdges"),
                    evaluator: static (op, geometry) => Query.Many(key: op, values: geometry.GetEdges()).ToEff()))
                : Op.Of(name: "BoxEdges").Unsupported<TGeometry, TOut>(),
            area: static _ => Query.BoxMetric<TGeometry, TOut>(key: Op.Of(name: "BoxArea"), boundingBox: static g => g.Area, box: static g => g.Area),
            volume: static _ => Query.BoxMetric<TGeometry, TOut>(key: Op.Of(name: "BoxVolume"), boundingBox: static g => g.Volume, box: static g => g.Volume));
    }
}

// --- [MEASURE_ROLE] ----------------------------------------------------------------------
internal static class MeasureRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Measure aspect) where TGeometry : notnull => aspect.Switch(
        spatialMidpoint: static _ => typeof(TOut) == typeof(Point3d) ? Query.SpatialMidpoint<TGeometry, TOut>() : Op.Of(name: "SpatialMidpoint").Unsupported<TGeometry, TOut>(),
        length: static _ => Query.Length<TGeometry, TOut>(),
        area: static a => Query.MassMeasure<TGeometry, TOut>(mass: MassKind.Area, kind: a),
        volume: static v => Query.MassMeasure<TGeometry, TOut>(mass: MassKind.Volume, kind: v),
        massError: static e => Query.MassMeasure<TGeometry, TOut>(mass: e.Mass, kind: e),
        centroid: static c => Query.MassMeasure<TGeometry, TOut>(mass: c.Mass, kind: c),
        centroidError: static ce => Query.MassMeasure<TGeometry, TOut>(mass: ce.Mass, kind: ce),
        radii: static r => Query.MassMeasure<TGeometry, TOut>(mass: r.Mass, kind: r),
        principalAxes: static p => Query.MassMeasure<TGeometry, TOut>(mass: p.Mass, kind: p));
}

// --- [CONFORMANCE_ROLE] ------------------------------------------------------------------
internal static class ConformanceRole {
    internal static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Apply<TGeometry, TPrimitive, TOut>(this Conformance aspect) where TGeometry : notnull where TPrimitive : notnull {
        Op key = Op.Of(name: nameof(Conformance));
        return (aspect, typeof(TGeometry), typeof(TPrimitive)) switch {
            (Conformance.Distance { Count: <= 0 } or Conformance.Rms { Count: <= 0 } or Conformance.WithinTolerance { Count: <= 0 } or Conformance.ProfileResidual { Count: <= 0 } or Conformance.Maximum { Count: <= 0 }, _, _) =>
                Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, Type geometry, Type primitive) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Line) => Query.ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Line>(
                aspect: aspect, requirement: Requirement.CurveLength, samples: static (g, p, count, context) => Query.CurvePrimitiveSamples(geometry: g, primitive: p, count: count, context: context, distance: static (line, point) => point.DistanceTo(other: line.ClosestPoint(testPoint: point, limitToFiniteSegment: false)))),
            (_, Type geometry, Type primitive) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Circle) => Query.ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Circle>(
                aspect: aspect, requirement: Requirement.CurveLength, samples: static (g, p, count, context) => Query.CurvePrimitiveSamples(geometry: g, primitive: p, count: count, context: context, distance: static (circle, point) => point.DistanceTo(other: circle.ClosestPoint(testPoint: point)))),
            (_, Type geometry, Type primitive) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Arc) => Query.ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Arc>(
                aspect: aspect, requirement: Requirement.CurveLength, samples: static (g, p, count, context) => Query.CurvePrimitiveSamples(geometry: g, primitive: p, count: count, context: context, distance: static (arc, point) => point.DistanceTo(other: arc.ClosestPoint(testPoint: point)))),
            (_, Type geometry, Type primitive) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Plane) => Query.ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Plane>(
                aspect: aspect, requirement: Requirement.SurfaceEvaluation, samples: static (g, p, count, context) => Query.SurfacePrimitiveSamples(geometry: g, primitive: p, count: count, context: context, distance: static (plane, point) => Math.Abs(value: plane.DistanceTo(testPoint: point)))),
            (_, Type geometry, Type primitive) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Sphere) => Query.ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Sphere>(
                aspect: aspect, requirement: Requirement.SurfaceEvaluation, samples: static (g, p, count, context) => Query.SurfacePrimitiveSamples(geometry: g, primitive: p, count: count, context: context, distance: static (sphere, point) => point.DistanceTo(other: sphere.ClosestPoint(testPoint: point)))),
            _ => key.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    }
}
