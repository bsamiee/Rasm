using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;
namespace Rasm.Analysis;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class Query {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Cast<TGeometry, TOut>(key: DomainKey, query: Query<TGeometry, Interval>.Build(
                    key: DomainKey,
                    evaluator: static geometry => (geometry switch {
                        Curve curve => One(key: DomainKey, value: curve.Domain),
                        _ => Fin.Fail<Seq<Interval>>(DomainKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Interval))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Cast<TGeometry, TOut>(key: DomainKey, query: Query<TGeometry, Interval>.Build(
                    key: DomainKey,
                    evaluator: static geometry => (geometry switch {
                        Surface surface => (One(key: DomainKey, value: surface.Domain(direction: 0)), One(key: DomainKey, value: surface.Domain(direction: 1)))
                            .Apply((u, v) => u + v)
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
                    evaluator: static geometry => Many(key: SegmentsKey, values: geometry.GetSegments()).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<TGeometry, Curve>.Build(
                    key: SegmentsKey,
                    evaluator: static geometry => (geometry switch {
                        Curve curve => Many(key: SegmentsKey, values: curve.GetSubCurves()),
                        _ => Fin.Fail<Seq<Curve>>(SegmentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
                    }).ToEff())),
            _ => SegmentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Brep, Curve> Edges =>
        Query<Brep, Curve>.Build(key: EdgesKey, evaluator: static geometry => Many(key: EdgesKey, values: geometry.DuplicateEdgeCurves()).ToEff());
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
                    evaluator: static geometry => geometry switch {
                        Line line => One(key: EdgeMidpointsKey, value: line.PointAt(t: 0.5)).ToEff(),
                        Polyline polyline => Many(
                            key: EdgeMidpointsKey,
                            values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                        BoundingBox box => Many(
                            key: EdgeMidpointsKey,
                            values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                        Curve curve => CurveEdgeMidpoint(curve: curve),
                        Brep brep => BrepEdgeMidpoints(brep: brep),
                        Mesh mesh => MeshEdgeMidpoints(mesh: mesh),
                        SubD subd => SubDEdgeMidpoints(subd: subd),
                        Box box =>
                            from ctx in Analyze.Asks
                            from result in Optional(box.ToBrep())
                                .ToFin(EdgeMidpointsKey.InvalidResult())
                                .Bind(brep => {
                                    using Brep disposable = brep;
                                    return EdgeCurveMidpoints(curves: disposable.DuplicateEdgeCurves(), context: ctx);
                                })
                                .ToEff()
                            select result,
                        _ => Fin.Fail<Seq<Point3d>>(EdgeMidpointsKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Eff<Context, Seq<Point3d>> CurveEdgeMidpoint(Curve curve) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: curve, requirement: Requirement.CurveLength).ToEff()
        from point in CurveAtNormalizedValue(
                curve: validated,
                context: ctx,
                key: EdgeMidpointsKey,
                project: static (geometry, parameter) => geometry.PointAt(t: parameter))
            .ToEff()
        from result in One(key: EdgeMidpointsKey, value: point).ToEff()
        select result;
    private static Eff<Context, Seq<Point3d>> BrepEdgeMidpoints(Brep brep) =>
        BrepLeaves<Point3d>(
            brep: brep,
            key: EdgeMidpointsKey,
            primitiveFault: static (key, label) => key.PrimitiveNoEdges(primitive: label),
            project: static (validated, context) => EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context));
    private static Eff<Context, Seq<Point3d>> MeshEdgeMidpoints(Mesh mesh) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: mesh, requirement: Requirement.Basic).ToEff()
        from result in Many(
                key: EdgeMidpointsKey,
                values: Enumerable
                    .Range(start: 0, count: validated.TopologyEdges.Count)
                    .Select(index => validated.TopologyEdges.EdgeLine(topologyEdgeIndex: index).PointAt(t: 0.5)))
            .ToEff()
        select result;
    private static Eff<Context, Seq<Point3d>> SubDEdgeMidpoints(SubD subd) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: subd, requirement: Requirement.Basic).ToEff()
        from result in EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: ctx).ToEff()
        select result;
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: NakedEdgesKey, query: Query<TGeometry, Curve>.Build(
                    key: NakedEdgesKey,
                    evaluator: static geometry => (geometry switch {
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
                    evaluator: static geometry => (geometry switch {
                        Mesh mesh => Many(key: NakedEdgesKey, values: mesh.GetNakedEdges()),
                        _ => Fin.Fail<Seq<Polyline>>(NakedEdgesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Polyline))),
                    }).ToEff())),
            _ => NakedEdgesKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Mesh, Polyline> Outlines(Plane plane) =>
        Query<Mesh, Polyline>.Build(
            key: OutlinesKey,
            state: plane,
            evaluator: static (sectionPlane, geometry) => Many(key: OutlinesKey, values: geometry.GetOutlines(plane: sectionPlane)).ToEff());
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) =>
        Query<Surface, Curve>.Build(
            key: IsoKey,
            requirement: Requirement.SurfaceEvaluation,
            state: (Iso: iso, Normalized: normalized),
            evaluator: static (state, geometry) => (
                state.Iso switch {
                    IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North =>
                        Optional(geometry.IsoCurve(iso: state.Iso))
                            .ToFin(IsoKey.InvalidResult())
                            .Map(static curve => Seq(curve)),
                    IsoStatus.X or IsoStatus.Y => state.Iso switch {
                        IsoStatus.X => 0,
                        _ => 1,
                    } switch {
                        int direction => geometry.Domain(direction: direction) switch {
                            Interval domain when domain.IsValid && state.Normalized is >= 0.0 and <= 1.0 => geometry switch {
                                BrepFace face => Many(key: IsoKey, values: face.TrimAwareIsoCurve(direction: direction, constantParameter: domain.ParameterAt(state.Normalized))),
                                _ => Optional(geometry.IsoCurve(state.Iso, domain.ParameterAt(state.Normalized)))
                                    .ToFin(IsoKey.InvalidResult())
                                    .Map(static curve => Seq(curve)),
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
                    project: static (curve, context, out circle) =>
                        curve.TryGetCircle(circle: out circle, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Arc) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Arc>(
                    project: static (curve, context, out arc) =>
                        curve.TryGetArc(arc: out arc, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Ellipse) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Ellipse>(
                    project: static (curve, context, out ellipse) =>
                        curve.TryGetEllipse(ellipse: out ellipse, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                PrimitiveMatch<TGeometry, TOut, Curve, Polyline>(
                    project: static (curve, _, out polyline) =>
                        curve.TryGetPolyline(polyline: out polyline)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Plane) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Plane>(
                    project: static (surface, context, out plane) =>
                        surface.TryGetPlane(plane: out plane, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Cylinder) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Cylinder>(
                    project: static (surface, context, out cylinder) =>
                        surface.TryGetCylinder(cylinder: out cylinder, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Sphere) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Sphere>(
                    project: static (surface, context, out sphere) =>
                        surface.TryGetSphere(sphere: out sphere, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Cone) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Cone>(
                    project: static (surface, context, out cone) =>
                        surface.TryGetCone(cone: out cone, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Torus) =>
                PrimitiveMatch<TGeometry, TOut, Surface, Torus>(
                    project: static (surface, context, out torus) =>
                        surface.TryGetTorus(torus: out torus, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                PrimitiveMatch<TGeometry, TOut, Brep, Box>(
                    project: static (brep, context, out box) => (
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
                    evaluator: static geometry => geometry switch {
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
                    evaluator: static geometry => (geometry switch {
                        Brep brep => One(key: SolidOrientationKey, value: brep.SolidOrientation),
                        _ => Fin.Fail<Seq<BrepSolidOrientation>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BrepSolidOrientation))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) =>
                Cast<TGeometry, TOut>(key: SolidOrientationKey, query: Query<TGeometry, int>.Build(
                    key: SolidOrientationKey,
                    evaluator: static geometry => (geometry switch {
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
                    requirement: Requirement.SolidTopology,
                    evaluator: static (target, geometry) =>
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
                    || geometry == typeof(Point3d)
                    || geometry == typeof(BoundingBox)
                    || geometry == typeof(Box)) =>
                Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                    key: VerticesKey,
                    requiresContext: true,
                    evaluator: static geometry => geometry switch {
                        Point3d point => One(key: VerticesKey, value: point).ToEff(),
                        Rhino.Geometry.Point point => One(key: VerticesKey, value: point.Location).ToEff(),
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
                                    unfolder: static current => current switch {
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
    private static Eff<Context, Seq<Point3d>> BrepVertices(Brep brep) =>
        BrepLeaves<Point3d>(
            brep: brep,
            key: VerticesKey,
            primitiveFault: static (key, label) => key.PrimitiveNoVertices(primitive: label),
            project: static (validated, _) => Many(key: VerticesKey, values: validated.DuplicateVertices()));
    private static Eff<Context, Seq<TOut>> BrepLeaves<TOut>(
        Brep brep,
        Op key,
        Func<Op, string, Error> primitiveFault,
        Func<Brep, Context, Fin<Seq<TOut>>> project) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: brep, requirement: Requirement.Basic).ToEff()
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
                    evaluator: static geometry => (geometry switch {
                        Brep brep => brep.GetConnectedComponents() switch {
                            Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
                            _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
                        },
                        _ => Fin.Fail<Seq<Brep>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Brep))),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) =>
                Cast<TGeometry, TOut>(key: ComponentsKey, query: Query<TGeometry, Mesh>.Build(
                    key: ComponentsKey,
                    evaluator: static geometry => (geometry switch {
                        Mesh mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()),
                        _ => Fin.Fail<Seq<Mesh>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Mesh))),
                    }).ToEff())),
            _ => ComponentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Topology<TGeometry, TOut>(Topology aspect) where TGeometry : notnull =>
        (aspect, typeof(TGeometry), typeof(TOut)) switch {
            (global::Rasm.Analysis.Topology.Boundary, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                NakedEdges<TGeometry, TOut>(),
            (global::Rasm.Analysis.Topology.Boundary, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                NakedEdges<TGeometry, TOut>(),
            (global::Rasm.Analysis.Topology.EdgeMidpoints, _, Type output) when output == typeof(Point3d) =>
                EdgeMidpoints<TGeometry, TOut>(),
            (global::Rasm.Analysis.Topology.Adjacency, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                BrepEdgeIndices<TGeometry, TOut>(predicate: static (_, _) => true),
            (global::Rasm.Analysis.Topology.Adjacency, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                MeshEdgeIndices<TGeometry, TOut>(predicate: static (_, _) => true),
            (global::Rasm.Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                BrepEdgeIndices<TGeometry, TOut>(predicate: static (brep, index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold),
            (global::Rasm.Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                MeshEdgeIndices<TGeometry, TOut>(predicate: static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2),
            (global::Rasm.Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                BrepEdgeAny<TGeometry, TOut>(predicate: static (brep, index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold),
            (global::Rasm.Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                MeshEdgeAny<TGeometry, TOut>(predicate: static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2),
            _ => TopologyKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> BrepEdgeIndices<TGeometry, TOut>(Func<Brep, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (filter, geometry) => (geometry switch {
                Brep brep => Many(key: TopologyKey, values: Enumerable
                    .Range(start: 0, count: brep.Edges.Count)
                    .Where(index => filter(arg1: brep, arg2: index))
                    .Select(static index => new ComponentIndex(type: ComponentIndexType.BrepEdge, index: index))),
                _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
            }).ToEff()));
    private static Query<TGeometry, TOut> MeshEdgeIndices<TGeometry, TOut>(Func<Mesh, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (filter, geometry) => (geometry switch {
                Mesh mesh => Many(key: TopologyKey, values: Enumerable
                    .Range(start: 0, count: mesh.TopologyEdges.Count)
                    .Where(index => filter(arg1: mesh, arg2: index))
                    .Select(static index => new ComponentIndex(type: ComponentIndexType.MeshTopologyEdge, index: index))),
                _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
            }).ToEff()));
    private static Query<TGeometry, TOut> BrepEdgeAny<TGeometry, TOut>(Func<Brep, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, bool>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (filter, geometry) => (geometry switch {
                Brep brep => One(key: TopologyKey, value: Enumerable
                    .Range(start: 0, count: brep.Edges.Count)
                    .Any(index => filter(arg1: brep, arg2: index))),
                _ => Fin.Fail<Seq<bool>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
            }).ToEff()));
    private static Query<TGeometry, TOut> MeshEdgeAny<TGeometry, TOut>(Func<Mesh, int, bool> predicate) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, bool>.Build(
            key: TopologyKey,
            state: predicate,
            evaluator: static (filter, geometry) => (geometry switch {
                Mesh mesh => One(key: TopologyKey, value: Enumerable
                    .Range(start: 0, count: mesh.TopologyEdges.Count)
                    .Any(index => filter(arg1: mesh, arg2: index))),
                _ => Fin.Fail<Seq<bool>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
            }).ToEff()));
    public static Query<Mesh, bool> IsManifold =>
        Query<Mesh, bool>.Build(
            key: IsManifoldKey,
            evaluator: static geometry => One(key: IsManifoldKey, value: geometry.IsManifold()).ToEff());
    public static Query<Mesh, bool> NakedPointStatus =>
        Query<Mesh, bool>.Build(
            key: NakedPointStatusKey,
            evaluator: static geometry => Many(key: NakedPointStatusKey, values: geometry.GetNakedEdgePointStatus()).ToEff());
    public static Query<Mesh, MeshCheckParameters> MeshCheck =>
        Query<Mesh, MeshCheckParameters>.Build(
            key: MeshCheckKey,
            evaluator: static geometry => MeshCheckParametersFor(geometry: geometry).ToEff());
    // Mesh.Check requires a TextLog using-local and a by-ref MeshCheckParameters; the imperative
    // shape is intrinsic to this Mesh.Check boundary adapter and cannot be expression-bodied.
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
        internal static readonly System.Collections.Generic.Dictionary<global::Rasm.Analysis.MeshCheckCount, Func<MeshCheckParameters, int>> Table = new() {
            [global::Rasm.Analysis.MeshCheckCount.DegenerateFaces] = static p => p.DegenerateFaceCount,
            [global::Rasm.Analysis.MeshCheckCount.DisjointMeshes] = static p => p.DisjointMeshCount,
            [global::Rasm.Analysis.MeshCheckCount.DuplicateFaces] = static p => p.DuplicateFaceCount,
            [global::Rasm.Analysis.MeshCheckCount.ExtremelyShortEdges] = static p => p.ExtremelyShortEdgeCount,
            [global::Rasm.Analysis.MeshCheckCount.InvalidNgons] = static p => p.InvalidNgonCount,
            [global::Rasm.Analysis.MeshCheckCount.NakedEdges] = static p => p.NakedEdgeCount,
            [global::Rasm.Analysis.MeshCheckCount.NonManifoldEdges] = static p => p.NonManifoldEdgeCount,
            [global::Rasm.Analysis.MeshCheckCount.NonUnitVectorNormals] = static p => p.NonUnitVectorNormalCount,
            [global::Rasm.Analysis.MeshCheckCount.RandomFaceNormals] = static p => p.RandomFaceNormalCount,
            [global::Rasm.Analysis.MeshCheckCount.SelfIntersectingPairs] = static p => p.SelfIntersectingPairsCount,
            [global::Rasm.Analysis.MeshCheckCount.UnusedVertices] = static p => p.UnusedVertexCount,
            [global::Rasm.Analysis.MeshCheckCount.VertexFaceNormalsDiffer] = static p => p.VertexFaceNormalsDifferCount,
            [global::Rasm.Analysis.MeshCheckCount.ZeroLengthNormals] = static p => p.ZeroLengthNormalCount,
        };
    }
    public static Query<Mesh, int> MeshCheckCount(global::Rasm.Analysis.MeshCheckCount count) =>
        MeshCheckCountDispatch.Table.TryGetValue(key: count, value: out Func<MeshCheckParameters, int>? project) switch {
            true => Query<Mesh, int>.Build(
                key: MeshCheckCountKey,
                state: project!,
                evaluator: static (projector, geometry) =>
                    from parameters in MeshCheck.Apply(geometry: geometry)
                    from head in parameters.Head.ToFin(MeshCheckCountKey.InvalidResult()).ToEff()
                    from result in One(key: MeshCheckCountKey, value: projector(arg: head)).ToEff()
                    select result),
            false => Query<Mesh, int>.Reject(
                key: MeshCheckCountKey,
                fault: MeshCheckCountKey.InvalidInput()),
        };
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(global::Rasm.Analysis.MeshFaceMetric metric) =>
        metric switch {
            global::Rasm.Analysis.MeshFaceMetric.AspectRatio => Query<Mesh, MeshFaceSample>.Build(
                key: MeshFaceMetricKey,
                requirement: Requirement.MeshCheck,
                evaluator: static geometry => toSeq(Enumerable
                    .Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM(face => geometry.Faces.GetFaceAspectRatio(index: face) switch {
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
            requirement: Requirement.Basic,
            evaluator: static geometry => from ctx in Analyze.Asks
                                          from result in SelfIntersectionsValue(geometry: geometry, context: ctx).ToEff()
                                          select result);
    // Mesh.GetSelfIntersections requires by-ref/out parameters and a TextLog using-local; the
    // CleanupFinally exemption permits the using-block at this GeometryBase boundary adapter.
    private static Fin<Seq<Polyline>> SelfIntersectionsValue(Mesh geometry, Context context) {
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
                    .Apply((left, right) => left + right)
                    .As(),
                false => Fin.Fail<Seq<Polyline>>(SelfIntersectionsKey.InvalidResult()),
            };
    }
    internal static GeometryKind KindOfBrep(Brep brep, Context context) =>
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
    private static Fin<Seq<Point3d>> EdgeCurveMidpoints(IEnumerable<Curve>? curves, Context context) =>
        Optional(curves)
            .ToFin(EdgeMidpointsKey.InvalidResult())
            .Bind(source => toSeq(source)
                .TraverseM(curve => {
                    using Curve disposable = curve;  // BOUNDARY ADAPTER -- DisposableCurve
                    return CurveAtNormalizedValue(
                        curve: disposable,
                        context: context,
                        key: EdgeMidpointsKey,
                        project: static (c, parameter) => c.PointAt(t: parameter));
                }).As())
            .Bind(static points => Many(key: EdgeMidpointsKey, values: points));
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull =>
        Aspect<TGeometry, TOut, Faces>(
            aspect: aspect,
            key: FacesKey,
            dispatch: static selector => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when output == typeof(Brep)
                    && (typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object)) =>
                    Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, Brep>.Build<Faces>(
                        key: FacesKey,
                        state: selector,
                        evaluator: static (inner, geometry) =>
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
                        requirement: Requirement.SurfaceEvaluation,
                        evaluator: static (inner, geometry) =>
                            from ctx in Analyze.Asks
                            from faces in DecomposeFaces(geometry: geometry).ToEff()
                            from chosen in SelectFaces(faces: faces, selector: inner, runtime: ctx).ToEff()
                            from frames in chosen.Traverse(face => FrameAtCentroid(face: face, runtime: ctx)).As().ToEff()
                            from result in Many(key: FacesKey, values: frames).ToEff()
                            select result)),
                _ => null,
            });
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull =>
        Aspect<TGeometry, TOut, Curves>(
            aspect: aspect,
            key: CurvesKey,
            dispatch: static selector => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when output == typeof(Curve)
                    && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                        || geometry == typeof(object)
                        || geometry == typeof(Line)
                        || geometry == typeof(Polyline)
                        || geometry == typeof(Circle)
                        || geometry == typeof(Arc)) =>
                    Cast<TGeometry, TOut>(key: CurvesKey, query: Query<TGeometry, Curve>.Build<Curves>(
                        key: CurvesKey,
                        state: selector,
                        evaluator: static (inner, geometry) =>
                            from ctx in Analyze.Asks
                            from curves in ExtractCurves(geometry: geometry, aspect: inner, runtime: ctx).ToEff()
                            from result in Many(key: CurvesKey, values: curves).ToEff()
                            select result)),
                _ => null,
            });
    private static Fin<Seq<Curve>> ExtractCurves<TGeometry>(TGeometry geometry, Curves aspect, Context runtime) where TGeometry : notnull =>
        aspect.Selector switch {
            CurveSelector selector when selector == CurveSelector.All => AllCurves(geometry: geometry, runtime: runtime),
            CurveSelector selector when selector == CurveSelector.Boundary => BoundaryCurves(geometry: geometry, runtime: runtime),
            CurveSelector selector when selector == CurveSelector.IsoU => IsoCurves(geometry: geometry, direction: 0, runtime: runtime),
            CurveSelector selector when selector == CurveSelector.IsoV => IsoCurves(geometry: geometry, direction: 1, runtime: runtime),
            CurveSelector selector when selector == CurveSelector.At => AllCurves(geometry: geometry, runtime: runtime).Bind(curves => IndexedCurve(curves: curves, index: aspect.Index)),
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.InvalidInput()),
        };
    private static Fin<Seq<Curve>> AllCurves<TGeometry>(TGeometry geometry, Context runtime) where TGeometry : notnull =>
        geometry switch {
            Curve curve => CurvePieces(curve: curve),
            Line line when line.IsValid => OneCurve(curve: line.ToNurbsCurve()),
            Polyline polyline when polyline.IsValid => OneCurve(curve: polyline.ToPolylineCurve()),
            Circle circle when circle.IsValid => OneCurve(curve: circle.ToNurbsCurve()),
            Arc arc when arc.IsValid => OneCurve(curve: arc.ToNurbsCurve()),
            Brep brep => ManyCurves(curves: brep.DuplicateEdgeCurves()),
            BrepFace face => BoundaryCurves(geometry: face, runtime: runtime),
            Surface surface => SurfaceBoundaryCurves(surface: surface),
            SubD subd => ManyCurves(curves: subd.DuplicateEdgeCurves()),
            Mesh mesh => ManyCurves(curves: Enumerable
                .Range(start: 0, count: mesh.TopologyEdges.Count)
                .Select(index => mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: index).ToNurbsCurve())),
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    private static Fin<Seq<Curve>> BoundaryCurves<TGeometry>(TGeometry geometry, Context runtime) where TGeometry : notnull =>
        geometry switch {
            Brep brep => ManyCurves(curves: brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true)),
            BrepFace face => Optional(face.DuplicateFace(duplicateMeshes: false))
                .ToFin(CurvesKey.InvalidResult())
                .Bind(static duplicate => {
                    using Brep disposable = duplicate;
                    return ManyCurves(curves: disposable.DuplicateEdgeCurves());
                }),
            Mesh mesh => ManyCurves(curves: mesh.GetNakedEdges().Select(static polyline => polyline.ToPolylineCurve())),
            _ => AllCurves(geometry: geometry, runtime: runtime),
        };
    private static Fin<Seq<Curve>> IsoCurves<TGeometry>(TGeometry geometry, int direction, Context runtime) where TGeometry : notnull =>
        geometry switch {
            Brep brep => toSeq(brep.Faces)
                .TraverseM(face => FaceIsoCurves(face: face, direction: direction, runtime: runtime))
                .As()
                .Map(static nested => nested.Bind(static curves => curves)),
            BrepFace face => FaceIsoCurves(face: face, direction: direction, runtime: runtime),
            Surface surface => SurfaceMidIsoCurve(surface: surface, direction: direction).Map(static curve => Seq(curve)),
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    private static Fin<Seq<Curve>> CurvePieces(Curve curve) =>
        curve.GetSubCurves() switch {
            Curve[] pieces when pieces.Length > 0 => ManyCurves(curves: pieces),
            _ => Optional(curve.DuplicateCurve())
                .ToFin(CurvesKey.InvalidResult())
                .Map(static duplicate => Seq(duplicate)),
        };
    private static Fin<Seq<Curve>> SurfaceBoundaryCurves(Surface surface) =>
        Seq(IsoStatus.South, IsoStatus.East, IsoStatus.North, IsoStatus.West)
            .TraverseM(iso => Optional(surface.IsoCurve(iso: iso)).ToFin(CurvesKey.InvalidResult()))
            .As();
    private static Fin<Seq<Curve>> FaceIsoCurves(BrepFace face, int direction, Context runtime) =>
        face.Domain(direction: direction) switch {
            Interval domain when domain.IsValid =>
                ManyCurves(curves: face.TrimAwareIsoCurve(direction: direction, constantParameter: domain.ParameterAt(normalizedParameter: 0.5))),
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.InvalidInput()),
        };
    private static Fin<Curve> SurfaceMidIsoCurve(Surface surface, int direction) =>
        surface.Domain(direction: direction) switch {
            Interval domain when domain.IsValid => Optional(surface.IsoCurve(direction switch {
                0 => IsoStatus.X,
                _ => IsoStatus.Y,
            }, domain.ParameterAt(normalizedParameter: 0.5)))
                .ToFin(CurvesKey.InvalidResult()),
            _ => Fin.Fail<Curve>(CurvesKey.InvalidInput()),
        };
    private static Fin<Seq<Curve>> IndexedCurve(Seq<Curve> curves, Option<int> index) =>
        curves.Count switch {
            0 => Fin.Succ(Seq<Curve>()),
            int count => Fin.Succ(Seq(curves[Math.Clamp(value: index.IfNone(static () => 0), min: 0, max: count - 1)])),
        };
    private static Fin<Seq<Curve>> OneCurve(Curve? curve) =>
        Optional(curve).ToFin(CurvesKey.InvalidResult()).Map(static value => Seq(value));
    private static Fin<Seq<Curve>> ManyCurves(IEnumerable<Curve>? curves) =>
        Optional(curves).ToFin(CurvesKey.InvalidResult()).Map(static values => toSeq(values));
    // Invariant: face is the single-face Brep produced by DuplicateFace; face.Faces[0] is the BrepFace.
    // BrepFace.FrameAt supplies the native surface U/V frame; NormalAt supplies the oriented face normal.
    // Flipping Y only when Z disagrees preserves X as the surface U direction.
    internal static Fin<Plane> FrameAtCentroid(Brep face, Context runtime) =>
        Optional(AreaMassProperties.Compute(
                brep: face,
                area: true,
                firstMoments: true,
                secondMoments: false,
                productMoments: false,
                relativeTolerance: runtime.Relative.Value,
                absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Bind(mass => {
                using AreaMassProperties disposable = mass;
                BrepFace brepFace = face.Faces[0];
                return brepFace.ClosestPointOnFace(testPoint: disposable.Centroid, u: out double u, v: out double v, maximumDistance: 0.0) switch {
                    true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                        (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() =>
                            Fin.Succ((frame.ZAxis * (brepFace.OrientationIsReversed ? -normal : normal)) switch {
                                >= 0.0 => frame,
                                _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                            }),
                        _ => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                };
            });
    internal static Fin<Seq<Brep>> DecomposeFaces<TGeometry>(TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Brep brep => Fin.Succ(toSeq(brep.Faces.Select(static face => face.DuplicateFace(duplicateMeshes: false)))),
            BrepFace face => Fin.Succ(Seq(face.DuplicateFace(duplicateMeshes: false))),
            Surface surface => Optional(surface.ToBrep())
                .ToFin(FacesKey.InvalidResult())
                .Map(static wrapped => Seq(wrapped)),
            SubD subd => Optional(subd.ToBrep())
                .ToFin(FacesKey.InvalidResult())
                .Map(static brep => {
                    using Brep disposable = brep;
                    return toSeq(disposable.Faces.Select(static face => face.DuplicateFace(duplicateMeshes: false)));
                }),
            _ => Fin.Fail<Seq<Brep>>(FacesKey.Unsupported(
                geometryType: geometry.GetType(),
                outputType: typeof(Brep))),
        };
    internal static Fin<Seq<Brep>> SelectFaces(Seq<Brep> faces, Faces selector, Context runtime) =>
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
    private static Fin<Seq<Brep>> RankByCentroidZ(Seq<Brep> faces, bool descending, Context runtime) =>
        faces
            .Traverse(face => FaceCentroidZ(face: face, runtime: runtime).Map(z => (Face: face, Z: z))).As()
            .Map(ranked => (ranked.IsEmpty, descending) switch {
                (true, _) => Seq<Brep>(),
                (false, true) => ranked
                    .Maxima(projection: static item => item.Z, tolerance: runtime.Absolute.Value)
                    .Map(static item => item.Face),
                (false, false) => ranked
                    .Minima(projection: static item => item.Z, tolerance: runtime.Absolute.Value)
                    .Map(static item => item.Face),
            });
    internal static Fin<double> FaceCentroidZ(Brep face, Context runtime) =>
        Optional(AreaMassProperties.Compute(
                brep: face,
                area: true,
                firstMoments: true,
                secondMoments: false,
                productMoments: false,
                relativeTolerance: runtime.Relative.Value,
                absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Map(static mass => {
                using AreaMassProperties disposable = mass;
                return disposable.Centroid.Z;
            });
}
