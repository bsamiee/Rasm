using System.Threading;
using Core.Domain;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [EXTRACT] ---------------------------------------------------------------------------------

public static partial class Query {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Cast<TGeometry, TOut>(key: DomainKey, query: Query<TGeometry, Interval>.Build(
                    key: DomainKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Curve curve => One(key: DomainKey, value: curve.Domain),
                        _ => Fin.Fail<Seq<Interval>>(DomainKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Interval))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Cast<TGeometry, TOut>(key: DomainKey, query: Query<TGeometry, Interval>.Build(
                    key: DomainKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Surface surface => (One(key: DomainKey, value: surface.Domain(direction: 0)), One(key: DomainKey, value: surface.Domain(direction: 1)))
                            .Apply((Seq<Interval> u, Seq<Interval> v) => u + v)
                            .As(),
                        _ => Fin.Fail<Seq<Interval>>(DomainKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Interval))),
                    }).ToEff())),
            _ => DomainKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Line) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<Polyline, Line>.Build(
                    key: SegmentsKey,
                    evaluator: static (Polyline geometry) =>
                        Many(key: SegmentsKey, values: geometry.GetSegments()).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<TGeometry, Curve>.Build(
                    key: SegmentsKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Curve curve => Many(key: SegmentsKey, values: curve.GetSubCurves()),
                        _ => Fin.Fail<Seq<Curve>>(SegmentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
                    }).ToEff())),
            _ => SegmentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Brep, Curve> Edges =>
        Query<Brep, Curve>.Build(key: EdgesKey, evaluator: static (Brep geometry) => Many(key: EdgesKey, values: geometry.DuplicateEdgeCurves()).ToEff());
    public static Query<TGeometry, TOut> EdgeMidpoints<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(Point3d)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(object)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)
                    || geometry == typeof(Box)) =>
                Cast<TGeometry, TOut>(key: EdgeMidpointsKey, query: Query<TGeometry, Point3d>.Build(
                    key: EdgeMidpointsKey,
                    evaluator: static (TGeometry geometry) => geometry switch {
                        Line line => One(key: EdgeMidpointsKey, value: line.PointAt(t: 0.5)).ToEff(),
                        Polyline polyline => Many(
                            key: EdgeMidpointsKey,
                            values: polyline.GetSegments().Select(static (Line segment) => segment.PointAt(t: 0.5))).ToEff(),
                        BoundingBox box => Many(
                            key: EdgeMidpointsKey,
                            values: box.GetEdges().Select(static (Line edge) => edge.PointAt(t: 0.5))).ToEff(),
                        Curve curve => CurveEdgeMidpoint(curve: curve),
                        Brep brep => BrepEdgeMidpoints(brep: brep),
                        Mesh mesh => MeshEdgeMidpoints(mesh: mesh),
                        SubD subd => SubDEdgeMidpoints(subd: subd),
                        Box box =>
                            from ctx in Analyze.Asks
                            from result in Optional(box.ToBrep())
                                .ToFin(EdgeMidpointsKey.InvalidResult())
                                .Bind((Brep brep) => BrepEdgesViaUsing(brep: brep, context: ctx))
                                .ToEff()
                            select result,
                        _ => Fin.Fail<Seq<Point3d>>(EdgeMidpointsKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Eff<GeometryContext, Seq<Point3d>> CurveEdgeMidpoint(Curve curve) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: curve, requirement: GeometryRequirement.CurveLength).ToEff()
        from point in CurveAtNormalizedValue(
                curve: validated,
                context: ctx,
                key: EdgeMidpointsKey,
                project: static (Curve geometry, double parameter) => geometry.PointAt(t: parameter))
            .ToEff()
        from result in One(key: EdgeMidpointsKey, value: point).ToEff()
        select result;
    private static Eff<GeometryContext, Seq<Point3d>> BrepEdgeMidpoints(Brep brep) =>
        BrepLeaves<Point3d>(
            brep: brep,
            key: EdgeMidpointsKey,
            primitiveFault: static (OperationKey key, string label) => key.PrimitiveNoEdges(primitive: label),
            project: static (Brep validated, GeometryContext context) => EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context));
    private static Eff<GeometryContext, Seq<Point3d>> MeshEdgeMidpoints(Mesh mesh) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: mesh, requirement: GeometryRequirement.Basic).ToEff()
        from result in Many(
                key: EdgeMidpointsKey,
                values: Enumerable
                    .Range(start: 0, count: validated.TopologyEdges.Count)
                    .Select((int index) => validated.TopologyEdges.EdgeLine(topologyEdgeIndex: index).PointAt(t: 0.5)))
            .ToEff()
        select result;
    private static Eff<GeometryContext, Seq<Point3d>> SubDEdgeMidpoints(SubD subd) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: subd, requirement: GeometryRequirement.Basic).ToEff()
        from result in EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: ctx).ToEff()
        select result;
    // Box.ToBrep() yields a fresh Brep that must be disposed; the using-local is intrinsic to this
    // boundary path between the Rhino native-types layer and the GeometryBase pipeline.
    [BoundaryImperativeExemption(
        ruleId: "CSP0001",
        reason: BoundaryImperativeReason.CleanupFinally,
        ticket: "RASM-WAVE4",
        expiresOnUtc: "2027-12-31T00:00:00Z")]
    private static Fin<Seq<Point3d>> BrepEdgesViaUsing(Brep brep, GeometryContext context) {
        using Brep disposable = brep;
        return EdgeCurveMidpoints(curves: disposable.DuplicateEdgeCurves(), context: context);
    }
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: NakedEdgesKey, query: Query<TGeometry, Curve>.Build(
                    key: NakedEdgesKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Brep brep => Many(
                            key: NakedEdgesKey,
                            values: brep.DuplicateNakedEdgeCurves(
                                nakedOuter: true,
                                nakedInner: true)),
                        _ => Fin.Fail<Seq<Curve>>(NakedEdgesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                Cast<TGeometry, TOut>(key: NakedEdgesKey, query: Query<TGeometry, Polyline>.Build(
                    key: NakedEdgesKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Mesh mesh => Many(key: NakedEdgesKey, values: mesh.GetNakedEdges()),
                        _ => Fin.Fail<Seq<Polyline>>(NakedEdgesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Polyline))),
                    }).ToEff())),
            _ => NakedEdgesKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Mesh, Polyline> Outlines(Plane plane) =>
        Query<Mesh, Polyline>.Build(
            key: OutlinesKey,
            state: plane,
            evaluator: static (Plane sectionPlane, Mesh geometry) => Many(key: OutlinesKey, values: geometry.GetOutlines(plane: sectionPlane)).ToEff());
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) =>
        Query<Surface, Curve>.Build(
            key: IsoKey,
            requirement: GeometryRequirement.SurfaceEvaluation,
            state: (Iso: iso, Normalized: normalized),
            evaluator: static ((IsoStatus Iso, double Normalized) state, Surface geometry) => (
                state.Iso switch {
                    IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North =>
                        Optional(geometry.IsoCurve(iso: state.Iso))
                            .ToFin(IsoKey.InvalidResult())
                            .Map(static (Curve curve) => Seq(curve)),
                    IsoStatus.X or IsoStatus.Y => state.Iso switch {
                        IsoStatus.X => 0,
                        _ => 1,
                    } switch {
                        int direction => geometry.Domain(direction: direction) switch {
                            Interval domain when domain.IsValid && state.Normalized is >= 0.0 and <= 1.0 => geometry switch {
                                BrepFace face => Many(key: IsoKey, values: face.TrimAwareIsoCurve(direction: direction, constantParameter: domain.ParameterAt(state.Normalized))),
                                _ => Optional(geometry.IsoCurve(state.Iso, domain.ParameterAt(state.Normalized)))
                                    .ToFin(IsoKey.InvalidResult())
                                    .Map(static (Curve curve) => Seq(curve)),
                            },
                            _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                        },
                    },
                    _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                }).ToEff());
    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Circle) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Circle>(
                    project: static (Curve curve, GeometryContext context, out Circle circle) =>
                        curve.TryGetCircle(circle: out circle, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Arc) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Arc>(
                    project: static (Curve curve, GeometryContext context, out Arc arc) =>
                        curve.TryGetArc(arc: out arc, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Ellipse) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Ellipse>(
                    project: static (Curve curve, GeometryContext context, out Ellipse ellipse) =>
                        curve.TryGetEllipse(ellipse: out ellipse, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Polyline>(
                    project: static (Curve curve, GeometryContext _, out Polyline polyline) =>
                        curve.TryGetPolyline(polyline: out polyline)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Plane) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Plane>(
                    project: static (Surface surface, GeometryContext context, out Plane plane) =>
                        surface.TryGetPlane(plane: out plane, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Cylinder) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Cylinder>(
                    project: static (Surface surface, GeometryContext context, out Cylinder cylinder) =>
                        surface.TryGetCylinder(cylinder: out cylinder, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Sphere) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Sphere>(
                    project: static (Surface surface, GeometryContext context, out Sphere sphere) =>
                        surface.TryGetSphere(sphere: out sphere, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Cone) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Cone>(
                    project: static (Surface surface, GeometryContext context, out Cone cone) =>
                        surface.TryGetCone(cone: out cone, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Torus) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Torus>(
                    project: static (Surface surface, GeometryContext context, out Torus torus) =>
                        surface.TryGetTorus(torus: out torus, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                PrimitiveMatch<TGeometry, TOut, Brep, Box>(
                    project: static (Brep brep, GeometryContext context, out Box box) => (
                        brep.IsBox(tolerance: context.Absolute.Value),
                        brep.GetBoundingBox(plane: Plane.WorldXY, worldBox: out box)) switch {
                            (true, BoundingBox local) => local.IsValid,
                            _ => false,
                        }),
            _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(GeometryKind)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(object)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)
                    || geometry == typeof(Box)
                    || geometry == typeof(Sphere)) =>
                Cast<TGeometry, TOut>(key: KindKey, query: Query<TGeometry, GeometryKind>.Build(
                    key: KindKey,
                    evaluator: static (TGeometry geometry) => geometry switch {
                        Brep brep =>
                            from ctx in Analyze.Asks
                            from result in One(key: KindKey, value: KindOfBrep(brep: brep, context: ctx)).ToEff()
                            select result,
                        Surface surface =>
                            from ctx in Analyze.Asks
                            from result in One(key: KindKey, value: surface switch {
                                Surface s when s.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value) => GeometryKind.Plane,
                                Surface s when s.TryGetSphere(sphere: out Sphere _, tolerance: ctx.Absolute.Value) => GeometryKind.Sphere,
                                Surface s when s.TryGetCylinder(cylinder: out Cylinder _, tolerance: ctx.Absolute.Value) => GeometryKind.Cylinder,
                                Surface s when s.TryGetCone(cone: out Cone _, tolerance: ctx.Absolute.Value) => GeometryKind.Cone,
                                Surface s when s.TryGetTorus(torus: out Torus _, tolerance: ctx.Absolute.Value) => GeometryKind.Torus,
                                _ => GeometryKind.Surface,
                            }).ToEff()
                            select result,
                        Mesh => One(key: KindKey, value: GeometryKind.Mesh).ToEff(),
                        SubD => One(key: KindKey, value: GeometryKind.SubD).ToEff(),
                        Curve curve => One(key: KindKey, value: curve.TryGetPolyline(polyline: out Polyline _) switch {
                            true => GeometryKind.Polyline,
                            false => GeometryKind.Curve,
                        }).ToEff(),
                        Polyline => One(key: KindKey, value: GeometryKind.Polyline).ToEff(),
                        Line => One(key: KindKey, value: GeometryKind.Line).ToEff(),
                        Sphere => One(key: KindKey, value: GeometryKind.Sphere).ToEff(),
                        Box _ => One(key: KindKey, value: GeometryKind.Box).ToEff(),
                        BoundingBox => One(key: KindKey, value: GeometryKind.BoundingBox).ToEff(),
                        _ => One(key: KindKey, value: GeometryKind.Unknown).ToEff(),
                    })),
            _ => KindKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> SolidOrientation<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(BrepSolidOrientation) =>
                Cast<TGeometry, TOut>(key: SolidOrientationKey, query: Query<TGeometry, BrepSolidOrientation>.Build(
                    key: SolidOrientationKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Brep brep => One(key: SolidOrientationKey, value: brep.SolidOrientation),
                        _ => Fin.Fail<Seq<BrepSolidOrientation>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BrepSolidOrientation))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) =>
                Cast<TGeometry, TOut>(key: SolidOrientationKey, query: Query<TGeometry, int>.Build(
                    key: SolidOrientationKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Mesh mesh => One(key: SolidOrientationKey, value: mesh.SolidOrientation()),
                        _ => Fin.Fail<Seq<int>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(int))),
                    }).ToEff())),
            _ => SolidOrientationKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase =>
        typeof(TGeometry) switch {
            Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) || typeof(Mesh).IsAssignableFrom(c: geometry) =>
                Cast<TGeometry, bool>(key: IsPointInsideKey, query: Query<TGeometry, bool>.Build(
                    key: IsPointInsideKey,
                    state: point,
                    requirement: GeometryRequirement.SolidTopology,
                    evaluator: static (Point3d target, TGeometry geometry) =>
                        from ctx in Analyze.Asks
                        from result in (geometry switch {
                            Brep brep => One(key: IsPointInsideKey, value: brep.IsPointInside(point: target, tolerance: ctx.Absolute.Value, strictlyIn: false)),
                            Mesh mesh => One(key: IsPointInsideKey, value: mesh.IsPointInside(point: target, tolerance: ctx.Absolute.Value, strictlyIn: false)),
                            _ => Fin.Fail<Seq<bool>>(IsPointInsideKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                        }).ToEff()
                        select result)),
            _ => IsPointInsideKey.Unsupported<TGeometry, bool>(),
        };
    public static Query<TGeometry, TOut> Vertices<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(Point3d)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(object)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)
                    || geometry == typeof(Box)) =>
                Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                    key: VerticesKey,
                    requiresContext: true,
                    evaluator: static (TGeometry geometry) => geometry switch {
                        Line line => Many(key: VerticesKey, values: new[] { line.From, line.To }).ToEff(),
                        Polyline polyline => Many(key: VerticesKey, values: polyline).ToEff(),
                        Curve curve => (curve.TryGetPolyline(polyline: out Polyline polyline) switch {
                            true => Many(key: VerticesKey, values: polyline),
                            false => Many(key: VerticesKey, values: new[] { curve.PointAtStart, curve.PointAtEnd }),
                        }).ToEff(),
                        Brep brep => BrepVertices(brep: brep),
                        Mesh mesh => Many(key: VerticesKey, values: mesh.Vertices.ToPoint3dArray()).ToEff(),
                        SubD subd => Many(
                                key: VerticesKey,
                                values: LanguageExt.List.unfold(
                                    state: (SubDVertex?)subd.Vertices.First,
                                    unfolder: static (SubDVertex? current) => current switch {
                                        SubDVertex vertex => Some((vertex.ControlNetPoint, (SubDVertex?)vertex.Next)),
                                        _ => None,
                                    }))
                            .ToEff(),
                        BoundingBox box => Many(key: VerticesKey, values: box.GetCorners()).ToEff(),
                        Box box => Many(key: VerticesKey, values: box.GetCorners()).ToEff(),
                        _ => Fin.Fail<Seq<Point3d>>(VerticesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => VerticesKey.Unsupported<TGeometry, TOut>(),
        };
    private static Eff<GeometryContext, Seq<Point3d>> BrepVertices(Brep brep) =>
        BrepLeaves<Point3d>(
            brep: brep,
            key: VerticesKey,
            primitiveFault: static (OperationKey key, string label) => key.PrimitiveNoVertices(primitive: label),
            project: static (Brep validated, GeometryContext _) => Many(key: VerticesKey, values: validated.DuplicateVertices()));
    private static Eff<GeometryContext, Seq<TOut>> BrepLeaves<TOut>(
        Brep brep,
        OperationKey key,
        Func<OperationKey, string, Error> primitiveFault,
        Func<Brep, GeometryContext, Fin<Seq<TOut>>> project) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: brep, requirement: GeometryRequirement.Basic).ToEff()
        from result in (KindOfBrep(brep: validated, context: ctx) switch {
            GeometryKind.BrepSphere => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Sphere")),
            GeometryKind.BrepCylinder => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cylinder")),
            GeometryKind.BrepCone => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cone")),
            GeometryKind.BrepTorus => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Torus")),
            _ => project(arg1: validated, arg2: ctx),
        }).ToEff()
        select result;
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Brep) =>
                Cast<TGeometry, TOut>(key: ComponentsKey, query: Query<TGeometry, Brep>.Build(
                    key: ComponentsKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Brep brep => brep.GetConnectedComponents() switch {
                            Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
                            _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
                        },
                        _ => Fin.Fail<Seq<Brep>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Brep))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) =>
                Cast<TGeometry, TOut>(key: ComponentsKey, query: Query<TGeometry, Mesh>.Build(
                    key: ComponentsKey,
                    evaluator: static (TGeometry geometry) => (geometry switch {
                        Mesh mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()),
                        _ => Fin.Fail<Seq<Mesh>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Mesh))),
                    }).ToEff())),
            _ => ComponentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Topology<TGeometry, TOut>(Topology aspect) where TGeometry : notnull =>
        (aspect, typeof(TGeometry), typeof(TOut)) switch {
            (Analysis.Topology.Boundary, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                NakedEdges<TGeometry, TOut>(),
            (Analysis.Topology.Boundary, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                NakedEdges<TGeometry, TOut>(),
            (Analysis.Topology.EdgeMidpoints, _, Type output) when output == typeof(Point3d) =>
                EdgeMidpoints<TGeometry, TOut>(),
            (Analysis.Topology.Adjacency, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                BrepEdgeIndices<TGeometry, TOut>(predicate: static (Brep _, int _) => true),
            (Analysis.Topology.Adjacency, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                MeshEdgeIndices<TGeometry, TOut>(predicate: static (Mesh _, int _) => true),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                BrepEdgeIndices<TGeometry, TOut>(predicate: static (Brep brep, int index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                MeshEdgeIndices<TGeometry, TOut>(predicate: static (Mesh mesh, int index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                BrepEdgeAny<TGeometry, TOut>(predicate: static (Brep brep, int index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                MeshEdgeAny<TGeometry, TOut>(predicate: static (Mesh mesh, int index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2),
            _ => TopologyKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> BrepEdgeIndices<TGeometry, TOut>(Func<Brep, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (Func<Brep, int, bool> filter, TGeometry geometry) => (geometry switch {
                Brep brep => Many(key: TopologyKey, values: Enumerable
                    .Range(start: 0, count: brep.Edges.Count)
                    .Where((int index) => filter(arg1: brep, arg2: index))
                    .Select(static (int index) => new ComponentIndex(type: ComponentIndexType.BrepEdge, index: index))),
                _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
            }).ToEff()));
    private static Query<TGeometry, TOut> MeshEdgeIndices<TGeometry, TOut>(Func<Mesh, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (Func<Mesh, int, bool> filter, TGeometry geometry) => (geometry switch {
                Mesh mesh => Many(key: TopologyKey, values: Enumerable
                    .Range(start: 0, count: mesh.TopologyEdges.Count)
                    .Where((int index) => filter(arg1: mesh, arg2: index))
                    .Select(static (int index) => new ComponentIndex(type: ComponentIndexType.MeshTopologyEdge, index: index))),
                _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
            }).ToEff()));
    private static Query<TGeometry, TOut> BrepEdgeAny<TGeometry, TOut>(Func<Brep, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, bool>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (Func<Brep, int, bool> filter, TGeometry geometry) => (geometry switch {
                Brep brep => One(key: TopologyKey, value: Enumerable
                    .Range(start: 0, count: brep.Edges.Count)
                    .Any((int index) => filter(arg1: brep, arg2: index))),
                _ => Fin.Fail<Seq<bool>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
            }).ToEff()));
    private static Query<TGeometry, TOut> MeshEdgeAny<TGeometry, TOut>(Func<Mesh, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, bool>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (Func<Mesh, int, bool> filter, TGeometry geometry) => (geometry switch {
                Mesh mesh => One(key: TopologyKey, value: Enumerable
                    .Range(start: 0, count: mesh.TopologyEdges.Count)
                    .Any((int index) => filter(arg1: mesh, arg2: index))),
                _ => Fin.Fail<Seq<bool>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
            }).ToEff()));
    public static Query<Mesh, bool> IsManifold =>
        Query<Mesh, bool>.Build(
            key: IsManifoldKey,
            evaluator: static (Mesh geometry) => One(key: IsManifoldKey, value: geometry.IsManifold()).ToEff());
    public static Query<Mesh, bool> NakedPointStatus =>
        Query<Mesh, bool>.Build(
            key: NakedPointStatusKey,
            evaluator: static (Mesh geometry) => Many(key: NakedPointStatusKey, values: geometry.GetNakedEdgePointStatus()).ToEff());
    public static Query<Mesh, MeshCheckParameters> MeshCheck =>
        Query<Mesh, MeshCheckParameters>.Build(
            key: MeshCheckKey,
            evaluator: static (Mesh geometry) => MeshCheckParametersFor(geometry: geometry).ToEff());
    // Mesh.Check requires a TextLog using-local and a by-ref MeshCheckParameters; the imperative
    // shape is intrinsic to this Mesh.Check boundary adapter and cannot be expression-bodied.
    [BoundaryImperativeExemption(
        ruleId: "CSP0001",
        reason: BoundaryImperativeReason.CleanupFinally,
        ticket: "RASM-WAVE4",
        expiresOnUtc: "2027-12-31T00:00:00Z")]
    private static Fin<Seq<MeshCheckParameters>> MeshCheckParametersFor(Mesh geometry) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return geometry.Check(
            textLog: textLog,
            parameters: ref parameters) switch {
                true or false => One(key: MeshCheckKey, value: parameters),
            };
    }
    private static class MeshCheckCountDispatch {
        internal static readonly System.Collections.Generic.Dictionary<Analysis.MeshCheckCount, Func<MeshCheckParameters, int>> Table = new() {
            [Analysis.MeshCheckCount.DegenerateFaces] = static (MeshCheckParameters p) => p.DegenerateFaceCount,
            [Analysis.MeshCheckCount.DisjointMeshes] = static (MeshCheckParameters p) => p.DisjointMeshCount,
            [Analysis.MeshCheckCount.DuplicateFaces] = static (MeshCheckParameters p) => p.DuplicateFaceCount,
            [Analysis.MeshCheckCount.ExtremelyShortEdges] = static (MeshCheckParameters p) => p.ExtremelyShortEdgeCount,
            [Analysis.MeshCheckCount.InvalidNgons] = static (MeshCheckParameters p) => p.InvalidNgonCount,
            [Analysis.MeshCheckCount.NakedEdges] = static (MeshCheckParameters p) => p.NakedEdgeCount,
            [Analysis.MeshCheckCount.NonManifoldEdges] = static (MeshCheckParameters p) => p.NonManifoldEdgeCount,
            [Analysis.MeshCheckCount.NonUnitVectorNormals] = static (MeshCheckParameters p) => p.NonUnitVectorNormalCount,
            [Analysis.MeshCheckCount.RandomFaceNormals] = static (MeshCheckParameters p) => p.RandomFaceNormalCount,
            [Analysis.MeshCheckCount.SelfIntersectingPairs] = static (MeshCheckParameters p) => p.SelfIntersectingPairsCount,
            [Analysis.MeshCheckCount.UnusedVertices] = static (MeshCheckParameters p) => p.UnusedVertexCount,
            [Analysis.MeshCheckCount.VertexFaceNormalsDiffer] = static (MeshCheckParameters p) => p.VertexFaceNormalsDifferCount,
            [Analysis.MeshCheckCount.ZeroLengthNormals] = static (MeshCheckParameters p) => p.ZeroLengthNormalCount,
        };
    }
    public static Query<Mesh, int> MeshCheckCount(Analysis.MeshCheckCount count) =>
        MeshCheckCountDispatch.Table.TryGetValue(key: count, value: out Func<MeshCheckParameters, int>? project) switch {
            true => Query<Mesh, int>.Build(
                key: MeshCheckCountKey,
                state: project!,
                evaluator: static (Func<MeshCheckParameters, int> projector, Mesh geometry) =>
                    from parameters in MeshCheck.Apply(geometry: geometry)
                    from head in parameters.Head.ToFin(MeshCheckCountKey.InvalidResult()).ToEff()
                    from result in One(key: MeshCheckCountKey, value: projector(arg: head)).ToEff()
                    select result),
            false => Query<Mesh, int>.Reject(
                key: MeshCheckCountKey,
                fault: MeshCheckCountKey.InvalidInput()),
        };
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(Analysis.MeshFaceMetric metric) =>
        metric switch {
            Analysis.MeshFaceMetric.AspectRatio => Query<Mesh, MeshFaceSample>.Build(
                key: MeshFaceMetricKey,
                requirement: GeometryRequirement.MeshCheck,
                evaluator: static (Mesh geometry) => toSeq(Enumerable
                    .Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM((int face) => geometry.Faces.GetFaceAspectRatio(index: face) switch {
                        double value when RhinoMath.IsValidDouble(x: value) && value >= 0.0 =>
                            Fin.Succ(new MeshFaceSample(Face: face, Value: value)),
                        _ => Fin.Fail<MeshFaceSample>(MeshFaceMetricKey.InvalidResult()),
                    })
                    .As()
                    .ToEff()),
            _ => Query<Mesh, MeshFaceSample>.Reject(
                key: MeshFaceMetricKey,
                fault: MeshFaceMetricKey.InvalidInput()),
        };
    public static Query<Mesh, Polyline> SelfIntersections =>
        Query<Mesh, Polyline>.Build(
            key: SelfIntersectionsKey,
            requirement: GeometryRequirement.Basic,
            evaluator: static (Mesh geometry) =>
                from ctx in Analyze.Asks
                from result in SelfIntersectionsValue(geometry: geometry, context: ctx).ToEff()
                select result);
    // Mesh.GetSelfIntersections requires by-ref/out parameters and a TextLog using-local; the
    // CleanupFinally exemption permits the using-block at this GeometryBase boundary adapter.
    [BoundaryImperativeExemption(
        ruleId: "CSP0001",
        reason: BoundaryImperativeReason.CleanupFinally,
        ticket: "RASM-WAVE4",
        expiresOnUtc: "2027-12-31T00:00:00Z")]
    private static Fin<Seq<Polyline>> SelfIntersectionsValue(Mesh geometry, GeometryContext context) {
        using TextLog textLog = new();
        return geometry.GetSelfIntersections(
            tolerance: context.MeshIntersectionTolerance,
            perforations: out Polyline[] perforations,
            overlapsPolylines: true,
            overlapsPolylinesResult: out Polyline[] overlaps,
            overlapsMesh: false,
            overlapsMeshResult: out Mesh _,
            textLog: textLog,
            cancel: CancellationToken.None,
            progress: null!) switch {
                true => (Many(key: SelfIntersectionsKey, values: perforations), Many(key: SelfIntersectionsKey, values: overlaps))
                    .Apply((Seq<Polyline> left, Seq<Polyline> right) => left + right)
                    .As(),
                false => Fin.Fail<Seq<Polyline>>(SelfIntersectionsKey.InvalidResult()),
            };
    }
    internal static GeometryKind KindOfBrep(Brep brep, GeometryContext context) =>
        brep switch {
            { IsSurface: true } single => single.Surfaces[0] switch {
                Surface s when s.TryGetPlane(plane: out Plane _, tolerance: context.Absolute.Value) => GeometryKind.BrepPlane,
                Surface s when s.TryGetSphere(sphere: out Sphere _, tolerance: context.Absolute.Value) => GeometryKind.BrepSphere,
                Surface s when s.TryGetCylinder(cylinder: out Cylinder _, tolerance: context.Absolute.Value) => GeometryKind.BrepCylinder,
                Surface s when s.TryGetCone(cone: out Cone _, tolerance: context.Absolute.Value) => GeometryKind.BrepCone,
                Surface s when s.TryGetTorus(torus: out Torus _, tolerance: context.Absolute.Value) => GeometryKind.BrepTorus,
                _ => GeometryKind.BrepGeneral,
            },
            Brep candidate when candidate.IsBox(tolerance: context.Absolute.Value) => GeometryKind.BrepBox,
            _ => GeometryKind.BrepGeneral,
        };
    private static Fin<Seq<Point3d>> EdgeCurveMidpoints(IEnumerable<Curve>? curves, GeometryContext context) =>
        Optional(curves)
            .ToFin(EdgeMidpointsKey.InvalidResult())
            .Bind((IEnumerable<Curve> source) => toSeq(source)
                .TraverseM((Curve curve) => {
                    using Curve disposable = curve;  // BOUNDARY ADAPTER -- DisposableCurve
                    return CurveAtNormalizedValue(
                        curve: disposable,
                        context: context,
                        key: EdgeMidpointsKey,
                        project: static (Curve c, double parameter) => c.PointAt(t: parameter));
                }).As())
            .Bind(static (Seq<Point3d> points) => Many(key: EdgeMidpointsKey, values: points));
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull =>
        Aspect<TGeometry, TOut, Faces>(
            aspect: aspect,
            key: FacesKey,
            dispatch: static (Faces selector) => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when output == typeof(Brep)
                    && (typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object)) =>
                    Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, Brep>.Build<Faces>(
                        key: FacesKey,
                        state: selector,
                        evaluator: static (Faces inner, TGeometry geometry) =>
                            from ctx in Analyze.Asks
                            from faces in DecomposeFaces(geometry: geometry).ToEff()
                            from chosen in SelectFaces(faces: faces, selector: inner, runtime: ctx).ToEff()
                            from result in Many(key: FacesKey, values: chosen).ToEff()
                            select result)),
                (Type geometry, Type output) when output == typeof(Plane)
                    && (typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object)) =>
                    Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, Plane>.Build<Faces>(
                        key: FacesKey,
                        state: selector,
                        requirement: GeometryRequirement.SurfaceEvaluation,
                        evaluator: static (Faces inner, TGeometry geometry) =>
                            from ctx in Analyze.Asks
                            from faces in DecomposeFaces(geometry: geometry).ToEff()
                            from chosen in SelectFaces(faces: faces, selector: inner, runtime: ctx).ToEff()
                            from frames in chosen.Traverse((Brep face) => FrameAtCentroid(face: face, runtime: ctx)).As().ToEff()
                            from result in Many(key: FacesKey, values: frames).ToEff()
                            select result)),
                _ => null,
            });
    // Invariant: face is the single-face Brep produced by DuplicateFace; face.Faces[0] is the BrepFace.
    // BrepFace.NormalAt returns the oriented outward normal directly (Rhino handles OrientationIsReversed
    // internally); the frame's Z therefore points outward on closed solids without manual axis flips.
    internal static Fin<Plane> FrameAtCentroid(Brep face, GeometryContext runtime) =>
        Optional(AreaMassProperties.Compute(
                brep: face,
                area: true,
                firstMoments: true,
                secondMoments: false,
                productMoments: false,
                relativeTolerance: runtime.Relative.Value,
                absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Bind((AreaMassProperties mass) => {
                using AreaMassProperties disposable = mass;
                BrepFace brepFace = face.Faces[0];
                return brepFace.ClosestPoint(testPoint: disposable.Centroid, u: out double u, v: out double v) switch {
                    true => (Origin: brepFace.PointAt(u: u, v: v), Normal: brepFace.NormalAt(u: u, v: v)) switch {
                        (Point3d origin, Vector3d normal) when origin.IsValid && normal.IsValid && !normal.IsTiny() =>
                            Fin.Succ(new Plane(origin: origin, normal: normal)),
                        _ => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                };
            });
    [System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Reliability", checkId: "CA2000",
        Justification = "Brep ownership transfers to caller via Seq<Brep> as analysis output; downstream Grasshopper consumers manage lifetime.")]
    internal static Fin<Seq<Brep>> DecomposeFaces<TGeometry>(TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Brep brep => Fin.Succ(toSeq(brep.Faces.Select(static (BrepFace face) => face.DuplicateFace(duplicateMeshes: false)))),
            BrepFace face => Fin.Succ(Seq(face.DuplicateFace(duplicateMeshes: false))),
            Surface surface => Optional(surface.ToBrep())
                .ToFin(FacesKey.InvalidResult())
                .Map(static (Brep wrapped) => Seq(wrapped)),
            SubD subd => Optional(subd.ToBrep())
                .ToFin(FacesKey.InvalidResult())
                .Map(static (Brep brep) => {
                    using Brep disposable = brep;
                    return toSeq(disposable.Faces.Select(static (BrepFace face) => face.DuplicateFace(duplicateMeshes: false)));
                }),
            _ => Fin.Fail<Seq<Brep>>(FacesKey.Unsupported(
                geometryType: geometry.GetType(),
                outputType: typeof(Brep))),
        };
    internal static Fin<Seq<Brep>> SelectFaces(Seq<Brep> faces, Faces selector, GeometryContext runtime) =>
        (selector.Selector, faces.Count) switch {
            (_, 0) => Fin.Succ(Seq<Brep>()),
            (FaceSelector all, _) when all == FaceSelector.All => Fin.Succ(faces),
            (FaceSelector top, _) when top == FaceSelector.Top => RankByCentroidZ(faces: faces, descending: true, runtime: runtime),
            (FaceSelector bottom, _) when bottom == FaceSelector.Bottom => RankByCentroidZ(faces: faces, descending: false, runtime: runtime),
            (FaceSelector at, int count) when at == FaceSelector.At => Fin.Succ(Seq(faces[Math.Clamp(
                value: selector.Index.IfNone(static () => 0),
                min: 0,
                max: count - 1)])),
            _ => Fin.Fail<Seq<Brep>>(FacesKey.InvalidInput()),
        };
    private static Fin<Seq<Brep>> RankByCentroidZ(Seq<Brep> faces, bool descending, GeometryContext runtime) =>
        faces
            .Traverse((Brep face) => FaceCentroidZ(face: face, runtime: runtime).Map((double z) => (Face: face, Z: z))).As()
            .Map((Seq<(Brep Face, double Z)> ranked) => (ranked.IsEmpty, descending) switch {
                (true, _) => Seq<Brep>(),
                (false, true) => ranked
                    .MaxesBy(projection: static ((Brep Face, double Z) item) => item.Z, tolerance: runtime.Absolute.Value)
                    .Map(static ((Brep Face, double Z) item) => item.Face),
                (false, false) => ranked
                    .MinesBy(projection: static ((Brep Face, double Z) item) => item.Z, tolerance: runtime.Absolute.Value)
                    .Map(static ((Brep Face, double Z) item) => item.Face),
            });
    internal static Fin<double> FaceCentroidZ(Brep face, GeometryContext runtime) =>
        Optional(AreaMassProperties.Compute(
                brep: face,
                area: true,
                firstMoments: true,
                secondMoments: false,
                productMoments: false,
                relativeTolerance: runtime.Relative.Value,
                absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Map(static (AreaMassProperties mass) => {
                using AreaMassProperties disposable = mass;
                return disposable.Centroid.Z;
            });
}
