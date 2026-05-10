using System.Runtime.InteropServices;
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

[StructLayout(LayoutKind.Auto)]
internal readonly record struct FaceProjection(Brep Brep, int FaceIndex, bool Reversed) {
    internal static FaceProjection From(BrepFace face) =>
        new(Brep: face.DuplicateFace(duplicateMeshes: false), FaceIndex: face.FaceIndex, Reversed: face.OrientationIsReversed);
}

public static partial class Query {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                NativeQuery<TGeometry, TOut, Curve, Interval>(
                    key: DomainKey,
                    project: static curve => One(key: DomainKey, value: curve.Domain).ToEff()),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                NativeQuery<TGeometry, TOut, Surface, Interval>(
                    key: DomainKey,
                    project: static surface => (One(key: DomainKey, value: surface.Domain(direction: 0)), One(key: DomainKey, value: surface.Domain(direction: 1))).Apply((u, v) => u + v).As().ToEff()),
            _ => DomainKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Line) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<Polyline, Line>.Build(
                    key: SegmentsKey,
                    evaluator: static geometry => Many(key: SegmentsKey, values: geometry.GetSegments()).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                NativeQuery<TGeometry, TOut, Curve, Curve>(
                    key: SegmentsKey,
                    project: static curve => Many(key: SegmentsKey, values: curve.GetSubCurves()).ToEff()),
            _ => SegmentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Brep, Curve> Edges =>
        Query<Brep, Curve>.Build(key: EdgesKey, evaluator: static geometry => Many(key: EdgesKey, values: geometry.DuplicateEdgeCurves()).ToEff());
    public static Query<TGeometry, TOut> EdgeMidpoints<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box)]) =>
                Cast<TGeometry, TOut>(key: EdgeMidpointsKey, query: Query<TGeometry, Point3d>.Build(
                    key: EdgeMidpointsKey,
                    evaluator: static geometry => geometry switch {
                        Line line => One(key: EdgeMidpointsKey, value: line.PointAt(t: 0.5)).ToEff(),
                        Polyline polyline => Many(key: EdgeMidpointsKey, values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                        BoundingBox box => Many(key: EdgeMidpointsKey, values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                        Curve curve => CurveAtNormalized(geometry: curve, key: EdgeMidpointsKey, project: static (geometry, parameter) => geometry.PointAt(t: parameter)),
                        Brep brep => BrepLeaves(brep: brep, key: EdgeMidpointsKey, primitiveFault: static (key, label) => key.PrimitiveNoEdges(primitive: label), project: static (validated, context) => EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context)),
                        Mesh mesh =>
                            from ctx in Analyze.Asks
                            from validated in ctx.Validate(geometry: mesh, requirement: Requirement.Basic).ToEff()
                            from result in Many(key: EdgeMidpointsKey, values: Enumerable.Range(start: 0, count: validated.TopologyEdges.Count).Select(index => validated.TopologyEdges.EdgeLine(topologyEdgeIndex: index).PointAt(t: 0.5))).ToEff()
                            select result,
                        SubD subd =>
                            from ctx in Analyze.Asks
                            from validated in ctx.Validate(geometry: subd, requirement: Requirement.Basic).ToEff()
                            from result in EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: ctx).ToEff()
                            select result,
                        Box box =>
                            from ctx in Analyze.Asks
                            from result in Optional(box.ToBrep())
                                .ToFin(EdgeMidpointsKey.InvalidResult())
                                .Bind(brep => { using Brep disposable = brep; return EdgeCurveMidpoints(curves: disposable.DuplicateEdgeCurves(), context: ctx); })
                                .ToEff()
                            select result,
                        _ => Fin.Fail<Seq<Point3d>>(EdgeMidpointsKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                NativeQuery<TGeometry, TOut, Brep, Curve>(
                    key: NakedEdgesKey,
                    project: static brep => Many(key: NakedEdgesKey, values: brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true)).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                NativeQuery<TGeometry, TOut, Mesh, Polyline>(
                    key: NakedEdgesKey,
                    project: static mesh => Many(key: NakedEdgesKey, values: mesh.GetNakedEdges()).ToEff()),
            _ => NakedEdgesKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Mesh, Polyline> Outlines(Plane plane) =>
        Query<Mesh, Polyline>.Build(
            key: OutlinesKey,
            state: plane,
            evaluator: static (sectionPlane, geometry) => Many(key: OutlinesKey, values: geometry.GetOutlines(plane: sectionPlane)).ToEff());
    private static Query<TGeometry, TOut> NativeQuery<TGeometry, TOut, TNative, TValue>(
        Op key,
        Func<TNative, Eff<Context, Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TValue))).ToEff(),
            }));
    private static bool Supports(Type geometry, Type output, Type target, params Type[] native) =>
        output == target
        && Supports(geometry: geometry, native: native);
    private static bool Supports(Type geometry, params Type[] native) =>
        typeof(GeometryBase).IsAssignableFrom(c: geometry)
        || geometry == typeof(object)
        || native.Contains(value: geometry);
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
        typeof(TGeometry) switch {
            Type geometry when typeof(Curve).IsAssignableFrom(c: geometry) => typeof(TOut) switch {
                Type output when output == typeof(Circle) => PrimitiveMatch<TGeometry, TOut, Curve, Circle>(project: static (curve, context, out circle) => curve.TryGetCircle(circle: out circle, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Arc) => PrimitiveMatch<TGeometry, TOut, Curve, Arc>(project: static (curve, context, out arc) => curve.TryGetArc(arc: out arc, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Ellipse) => PrimitiveMatch<TGeometry, TOut, Curve, Ellipse>(project: static (curve, context, out ellipse) => curve.TryGetEllipse(ellipse: out ellipse, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Polyline) => PrimitiveMatch<TGeometry, TOut, Curve, Polyline>(project: static (curve, _, out polyline) => curve.TryGetPolyline(polyline: out polyline)),
                _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
            },
            Type geometry when typeof(Surface).IsAssignableFrom(c: geometry) => typeof(TOut) switch {
                Type output when output == typeof(Plane) => PrimitiveMatch<TGeometry, TOut, Surface, Plane>(project: static (surface, context, out plane) => surface.TryGetPlane(plane: out plane, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Cylinder) => PrimitiveMatch<TGeometry, TOut, Surface, Cylinder>(project: static (surface, context, out cylinder) => surface.TryGetCylinder(cylinder: out cylinder, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Sphere) => PrimitiveMatch<TGeometry, TOut, Surface, Sphere>(project: static (surface, context, out sphere) => surface.TryGetSphere(sphere: out sphere, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Cone) => PrimitiveMatch<TGeometry, TOut, Surface, Cone>(project: static (surface, context, out cone) => surface.TryGetCone(cone: out cone, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Torus) => PrimitiveMatch<TGeometry, TOut, Surface, Torus>(project: static (surface, context, out torus) => surface.TryGetTorus(torus: out torus, tolerance: context.Absolute.Value)),
                _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
            },
            Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) && typeof(TOut) == typeof(Box) =>
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
            (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(GeometryKind), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box), typeof(Sphere)]) =>
                Cast<TGeometry, TOut>(key: KindKey, query: Query<TGeometry, GeometryKind>.Build(
                    key: KindKey,
                    evaluator: static geometry => geometry switch {
                        Brep brep =>
                            from ctx in Analyze.Asks
                            from result in One(key: KindKey, value: KindOfBrep(brep: brep, context: ctx)).ToEff()
                            select result,
                        Surface surface =>
                            from ctx in Analyze.Asks
                            from result in One(key: KindKey, value: KindOfSurface(surface: surface, context: ctx, brep: false)).ToEff()
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
                NativeQuery<TGeometry, TOut, Brep, BrepSolidOrientation>(
                    key: SolidOrientationKey,
                    project: static brep => One(key: SolidOrientationKey, value: brep.SolidOrientation).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) =>
                NativeQuery<TGeometry, TOut, Mesh, int>(
                    key: SolidOrientationKey,
                    project: static mesh => One(key: SolidOrientationKey, value: mesh.SolidOrientation()).ToEff()),
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
            (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(Point3d), typeof(BoundingBox), typeof(Box)]) =>
                Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                    key: VerticesKey,
                    requiresContext: true,
                    evaluator: static geometry => geometry switch {
                        Point3d point => One(key: VerticesKey, value: point).ToEff(),
                        Point point => One(key: VerticesKey, value: point.Location).ToEff(),
                        Line line => Many(key: VerticesKey, values: new[] { line.From, line.To }).ToEff(),
                        Polyline polyline => Many(key: VerticesKey, values: polyline).ToEff(),
                        Curve curve => (curve.TryGetPolyline(polyline: out Polyline polyline) switch {
                            true => Many(key: VerticesKey, values: polyline),
                            false => Many(key: VerticesKey, values: new[] { curve.PointAtStart, curve.PointAtEnd }),
                        }).ToEff(),
                        Brep brep => BrepVertices(brep: brep),
                        Mesh mesh => Many(key: VerticesKey, values: mesh.Vertices.ToPoint3dArray()).ToEff(),
                        PointCloud cloud => Many(key: VerticesKey, values: cloud.GetPoints()).ToEff(),
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
        BrepLeaves(
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
                NativeQuery<TGeometry, TOut, Brep, Brep>(
                    key: ComponentsKey,
                    project: static brep => (brep.GetConnectedComponents() switch {
                        Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
                        _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
                    }).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) =>
                NativeQuery<TGeometry, TOut, Mesh, Mesh>(
                    key: ComponentsKey,
                    project: static mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()).ToEff()),
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
                EdgeQuery<TGeometry, TOut, Brep, ComponentIndex>(predicate: static (_, _) => true, count: static brep => brep.Edges.Count, project: static edges => Many(key: TopologyKey, values: edges.Map(static index => new ComponentIndex(type: ComponentIndexType.BrepEdge, index: index)))),
            (Analysis.Topology.Adjacency, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                EdgeQuery<TGeometry, TOut, Mesh, ComponentIndex>(predicate: static (_, _) => true, count: static mesh => mesh.TopologyEdges.Count, project: static edges => Many(key: TopologyKey, values: edges.Map(static index => new ComponentIndex(type: ComponentIndexType.MeshTopologyEdge, index: index)))),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                EdgeQuery<TGeometry, TOut, Brep, ComponentIndex>(predicate: static (brep, index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold, count: static brep => brep.Edges.Count, project: static edges => Many(key: TopologyKey, values: edges.Map(static index => new ComponentIndex(type: ComponentIndexType.BrepEdge, index: index)))),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                EdgeQuery<TGeometry, TOut, Mesh, ComponentIndex>(predicate: static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2, count: static mesh => mesh.TopologyEdges.Count, project: static edges => Many(key: TopologyKey, values: edges.Map(static index => new ComponentIndex(type: ComponentIndexType.MeshTopologyEdge, index: index)))),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                EdgeQuery<TGeometry, TOut, Brep, bool>(predicate: static (brep, index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold, count: static brep => brep.Edges.Count, project: static edges => One(key: TopologyKey, value: !edges.IsEmpty)),
            (Analysis.Topology.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                EdgeQuery<TGeometry, TOut, Mesh, bool>(predicate: static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2, count: static mesh => mesh.TopologyEdges.Count, project: static edges => One(key: TopologyKey, value: !edges.IsEmpty)),
            _ => TopologyKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> EdgeQuery<TGeometry, TOut, TNative, TValue>(
        Func<TNative, int, bool> predicate,
        Func<TNative, int> count,
        Func<Seq<int>, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, TValue>.Build(
            key: TopologyKey,
            state: (Predicate: predicate, Count: count, Project: project),
            evaluator: static (state, geometry) => (geometry switch {
                TNative native => state.Project(arg: toSeq(Enumerable
                    .Range(start: 0, count: state.Count(arg: native))
                    .Where(index => state.Predicate(arg1: native, arg2: index)))),
                _ => Fin.Fail<Seq<TValue>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TValue))),
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
    public static Query<Mesh, int> MeshCheckCount(MeshCheckCount count) =>
        count switch {
            Analysis.MeshCheckCount.None => Query<Mesh, int>.Reject(key: MeshCheckCountKey, fault: MeshCheckCountKey.InvalidInput()),
            Analysis.MeshCheckCount.DegenerateFaces or Analysis.MeshCheckCount.DisjointMeshes or Analysis.MeshCheckCount.DuplicateFaces or Analysis.MeshCheckCount.ExtremelyShortEdges or Analysis.MeshCheckCount.InvalidNgons or Analysis.MeshCheckCount.NakedEdges or Analysis.MeshCheckCount.NonManifoldEdges or Analysis.MeshCheckCount.NonUnitVectorNormals or Analysis.MeshCheckCount.RandomFaceNormals or Analysis.MeshCheckCount.SelfIntersectingPairs or Analysis.MeshCheckCount.UnusedVertices or Analysis.MeshCheckCount.VertexFaceNormalsDiffer or Analysis.MeshCheckCount.ZeroLengthNormals => Query<Mesh, int>.Build(
                key: MeshCheckCountKey,
                state: count,
                evaluator: static (kind, geometry) =>
                    from parameters in MeshCheck.Apply(geometry: geometry)
                    from head in parameters.Head.ToFin(MeshCheckCountKey.InvalidResult()).ToEff()
                    from result in (kind switch {
                        Analysis.MeshCheckCount.DegenerateFaces => One(key: MeshCheckCountKey, value: head.DegenerateFaceCount),
                        Analysis.MeshCheckCount.DisjointMeshes => One(key: MeshCheckCountKey, value: head.DisjointMeshCount),
                        Analysis.MeshCheckCount.DuplicateFaces => One(key: MeshCheckCountKey, value: head.DuplicateFaceCount),
                        Analysis.MeshCheckCount.ExtremelyShortEdges => One(key: MeshCheckCountKey, value: head.ExtremelyShortEdgeCount),
                        Analysis.MeshCheckCount.InvalidNgons => One(key: MeshCheckCountKey, value: head.InvalidNgonCount),
                        Analysis.MeshCheckCount.NakedEdges => One(key: MeshCheckCountKey, value: head.NakedEdgeCount),
                        Analysis.MeshCheckCount.NonManifoldEdges => One(key: MeshCheckCountKey, value: head.NonManifoldEdgeCount),
                        Analysis.MeshCheckCount.NonUnitVectorNormals => One(key: MeshCheckCountKey, value: head.NonUnitVectorNormalCount),
                        Analysis.MeshCheckCount.RandomFaceNormals => One(key: MeshCheckCountKey, value: head.RandomFaceNormalCount),
                        Analysis.MeshCheckCount.SelfIntersectingPairs => One(key: MeshCheckCountKey, value: head.SelfIntersectingPairsCount),
                        Analysis.MeshCheckCount.UnusedVertices => One(key: MeshCheckCountKey, value: head.UnusedVertexCount),
                        Analysis.MeshCheckCount.VertexFaceNormalsDiffer => One(key: MeshCheckCountKey, value: head.VertexFaceNormalsDifferCount),
                        Analysis.MeshCheckCount.ZeroLengthNormals => One(key: MeshCheckCountKey, value: head.ZeroLengthNormalCount),
                        _ => Fin.Fail<Seq<int>>(MeshCheckCountKey.InvalidInput()),
                    }).ToEff()
                    select result),
            _ => Query<Mesh, int>.Reject(key: MeshCheckCountKey, fault: MeshCheckCountKey.InvalidInput()),
        };
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric metric) =>
        metric switch {
            Analysis.MeshFaceMetric.AspectRatio => Query<Mesh, MeshFaceSample>.Build(
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
            { IsSurface: true } single => KindOfSurface(surface: single.Surfaces[0], context: context, brep: true),
            Brep candidate when candidate.IsBox(tolerance: context.Absolute.Value) => GeometryKind.BrepBox,
            _ => GeometryKind.BrepGeneral,
        };
    private static GeometryKind KindOfSurface(Surface surface, Context context, bool brep) =>
        surface switch {
            Surface s when s.TryGetPlane(plane: out Plane _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepPlane : GeometryKind.Plane,
            Surface s when s.TryGetSphere(sphere: out Sphere _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepSphere : GeometryKind.Sphere,
            Surface s when s.TryGetCylinder(cylinder: out Cylinder _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepCylinder : GeometryKind.Cylinder,
            Surface s when s.TryGetCone(cone: out Cone _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepCone : GeometryKind.Cone,
            Surface s when s.TryGetTorus(torus: out Torus _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepTorus : GeometryKind.Torus,
            _ => brep ? GeometryKind.BrepGeneral : GeometryKind.Surface,
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
        Aspect(
            aspect: aspect,
            key: FacesKey,
            dispatch: static selector => Supports(geometry: typeof(TGeometry)) switch {
                true => typeof(TOut) switch {
                    Type output when output == typeof(Brep) =>
                    FaceQuery<TGeometry, TOut, Brep>(
                        selector: selector,
                        requirement: Requirement.None,
                        project: static (chosen, _) => Many(key: FacesKey, values: chosen.Map(static face => face.Brep))),
                    Type output when output == typeof(Plane) =>
                    FaceQuery<TGeometry, TOut, Plane>(
                        selector: selector,
                        requirement: Requirement.SurfaceEvaluation,
                        project: static (chosen, runtime) => chosen.Traverse(face => FrameAtCentroid(face: face, runtime: runtime)).As()),
                    Type output when output == typeof(Point3d) =>
                    FaceQuery<TGeometry, TOut, Point3d>(
                        selector: selector,
                        requirement: Requirement.SurfaceEvaluation,
                        project: static (chosen, runtime) => chosen.Traverse(face => FaceCentroid(face: face, runtime: runtime)).As()),
                    Type output when output == typeof(Vector3d) =>
                    FaceQuery<TGeometry, TOut, Vector3d>(
                        selector: selector,
                        requirement: Requirement.SurfaceEvaluation,
                        project: static (chosen, runtime) => chosen.Traverse(face => FrameAtCentroid(face: face, runtime: runtime).Map(static frame => frame.ZAxis)).As()),
                    Type output when output == typeof(int) =>
                    FaceQuery<TGeometry, TOut, int>(
                        selector: selector,
                        requirement: Requirement.None,
                        project: static (chosen, _) => Many(key: FacesKey, values: chosen.Map(static face => face.FaceIndex))),
                    Type output when output == typeof(Interval) =>
                    FaceQuery<TGeometry, TOut, Interval>(
                        selector: selector,
                        requirement: Requirement.SurfaceEvaluation,
                        project: static (chosen, _) => chosen.Traverse(static face =>
                                (face.Brep.Faces[0].Domain(direction: 0), face.Brep.Faces[0].Domain(direction: 1)) switch {
                                    (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
                                    _ => Fin.Fail<Seq<Interval>>(FacesKey.InvalidResult()),
                                })
                            .Map(static nested => nested.Bind(static domain => domain))
                            .As()),
                    _ => null,
                },
                false => null,
            });
    private static Query<TGeometry, TOut> FaceQuery<TGeometry, TOut, TValue>(
        Faces selector,
        Requirement requirement,
        Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, TValue>.Build(
            key: FacesKey, state: (Selector: selector, Project: project), requirement: requirement,
            evaluator: static (state, geometry) =>
                from ctx in Analyze.Asks
                from faces in DecomposeFaces(geometry: geometry).ToEff()
                from chosen in SelectFaces(faces: faces, selector: state.Selector, runtime: ctx).ToEff()
                from result in state.Project(arg1: chosen, arg2: ctx).ToEff()
                select result));
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull =>
        Aspect(
            aspect: aspect,
            key: CurvesKey,
            dispatch: static selector => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Curve), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) =>
                    Cast<TGeometry, TOut>(key: CurvesKey, query: Query<TGeometry, Curve>.Build(
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
        (aspect.Selector, geometry) switch {
            (CurveSelector selector, _) when selector == CurveSelector.All => CurvesOf(geometry: geometry, boundary: false, runtime: runtime),
            (CurveSelector selector, _) when selector == CurveSelector.Boundary => CurvesOf(geometry: geometry, boundary: true, runtime: runtime),
            (CurveSelector selector, _) when selector == CurveSelector.IsoU => IsoCurves(geometry: geometry, direction: 0),
            (CurveSelector selector, _) when selector == CurveSelector.IsoV => IsoCurves(geometry: geometry, direction: 1),
            (CurveSelector selector, _) when selector == CurveSelector.At => CurvesOf(geometry: geometry, boundary: false, runtime: runtime).Bind(curves => curves.Count switch {
                0 => Fin.Succ(Seq<Curve>()),
                int count => Fin.Succ(Seq(curves[Math.Clamp(value: aspect.Index.IfNone(static () => 0), min: 0, max: count - 1)])),
            }),
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.InvalidInput()),
        };
    private static Fin<Seq<Curve>> CurvesOf<TGeometry>(TGeometry geometry, bool boundary, Context runtime) where TGeometry : notnull =>
        (boundary, geometry) switch {
            (false, Curve curve) => CurvePieces(curve: curve),
            (false, Line line) when line.IsValid => OneCurve(curve: line.ToNurbsCurve()),
            (false, Polyline polyline) when polyline.IsValid => OneCurve(curve: polyline.ToPolylineCurve()),
            (false, Circle circle) when circle.IsValid => OneCurve(curve: circle.ToNurbsCurve()),
            (false, Arc arc) when arc.IsValid => OneCurve(curve: arc.ToNurbsCurve()),
            (false, Brep brep) => ManyCurves(curves: brep.DuplicateEdgeCurves()),
            (false, BrepFace face) => CurvesOf(geometry: face, boundary: true, runtime: runtime),
            (false, Surface surface) => SurfaceBoundaryCurves(surface: surface),
            (false, SubD subd) => ManyCurves(curves: subd.DuplicateEdgeCurves()),
            (false, Mesh mesh) => ManyCurves(curves: Enumerable
                .Range(start: 0, count: mesh.TopologyEdges.Count)
                .Select(index => mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: index).ToNurbsCurve())),
            (true, Brep brep) => ManyCurves(curves: brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true)),
            (true, BrepFace face) => Optional(face.DuplicateFace(duplicateMeshes: false))
                .ToFin(CurvesKey.InvalidResult())
                .Bind(static duplicate => { using Brep disposable = duplicate; return ManyCurves(curves: disposable.DuplicateEdgeCurves()); }),
            (true, Mesh mesh) => ManyCurves(curves: mesh.GetNakedEdges().Select(static polyline => polyline.ToPolylineCurve())),
            (true, _) => CurvesOf(geometry: geometry, boundary: false, runtime: runtime),
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    private static Fin<Seq<Curve>> IsoCurves<TGeometry>(TGeometry geometry, int direction) where TGeometry : notnull =>
        geometry switch {
            Brep brep => toSeq(brep.Faces)
                .TraverseM(face => MidIsoCurve(surface: face, direction: direction))
                .As()
                .Map(static nested => nested.Bind(static curves => curves)),
            Surface surface => MidIsoCurve(surface: surface, direction: direction),
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
    private static Fin<Seq<Curve>> MidIsoCurve(Surface surface, int direction) =>
        surface.Domain(direction: direction) switch {
            Interval domain when domain.IsValid => surface switch {
                BrepFace face => ManyCurves(curves: face.TrimAwareIsoCurve(direction: direction, constantParameter: domain.ParameterAt(normalizedParameter: 0.5))),
                _ => Optional(surface.IsoCurve(direction switch { 0 => IsoStatus.X, _ => IsoStatus.Y }, domain.ParameterAt(normalizedParameter: 0.5))).ToFin(CurvesKey.InvalidResult()).Map(static curve => Seq(curve)),
            },
            _ => Fin.Fail<Seq<Curve>>(CurvesKey.InvalidInput()),
        };
    private static Fin<Seq<Curve>> OneCurve(Curve? curve) =>
        Optional(curve).ToFin(CurvesKey.InvalidResult()).Map(static value => Seq(value));
    private static Fin<Seq<Curve>> ManyCurves(IEnumerable<Curve>? curves) =>
        Optional(curves).ToFin(CurvesKey.InvalidResult()).Map(static values => toSeq(values));
    internal static Fin<Plane> FrameAtCentroid(FaceProjection face, Context runtime) =>
        FaceCentroid(face: face, runtime: runtime)
            .Bind(centroid => {
                BrepFace brepFace = face.Brep.Faces[0];
                return brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: 0.0) switch {
                    true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                        (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() =>
                            Fin.Succ((frame.ZAxis * (face.Reversed ? -normal : normal)) switch {
                                >= 0.0 => frame,
                                _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                            }),
                        _ => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                };
            });
    internal static Fin<Point3d> FaceCentroid(FaceProjection face, Context runtime) =>
        Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Relative.Value, absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Map(static mass => { using AreaMassProperties disposable = mass; return disposable.Centroid; });
    internal static Fin<Seq<FaceProjection>> DecomposeFaces<TGeometry>(TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Brep brep => Fin.Succ(FaceProjections(brep: brep)),
            BrepFace face => Fin.Succ(Seq(FaceProjection.From(face: face))),
            GeometryBase native when native is not Mesh && native.HasBrepForm => Optional(Brep.TryConvertBrep(geometry: native))
                .ToFin(FacesKey.InvalidResult())
                .Map(static brep => FaceProjectionsFromOwnedBrep(brep: brep)),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Brep))),
        };
    private static Seq<FaceProjection> FaceProjections(Brep brep) =>
        toSeq(brep.Faces.Select(static face => FaceProjection.From(face: face)));
    private static Seq<FaceProjection> FaceProjectionsFromOwnedBrep(Brep brep) {
        using Brep disposable = brep;
        return FaceProjections(brep: disposable);
    }
    internal static Fin<Seq<FaceProjection>> SelectFaces(Seq<FaceProjection> faces, Faces selector, Context runtime) =>
        (selector.Selector, faces.Count) switch {
            (_, 0) => Fin.Succ(Seq<FaceProjection>()),
            (FaceSelector all, _) when all == FaceSelector.All => Fin.Succ(faces),
            (FaceSelector top, _) when top == FaceSelector.Top => RankByCentroidZ(faces: faces, descending: true, runtime: runtime),
            (FaceSelector bottom, _) when bottom == FaceSelector.Bottom => RankByCentroidZ(faces: faces, descending: false, runtime: runtime),
            (FaceSelector at, int count) when at == FaceSelector.At => Fin.Succ(Seq(faces[Math.Clamp(value: selector.Index.IfNone(static () => 0), min: 0, max: count - 1)])),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.InvalidInput()),
        };
    private static Fin<Seq<FaceProjection>> RankByCentroidZ(Seq<FaceProjection> faces, bool descending, Context runtime) =>
        faces
            .Traverse(face => FaceCentroid(face: face, runtime: runtime).Map(point => (face, point.Z))).As()
            .Map(ranked => (ranked.IsEmpty, descending) switch {
                (true, _) => Seq<FaceProjection>(),
                (false, true) => ranked.Maxima(projection: static item => item.Z, tolerance: runtime.Absolute.Value).Map(static item => item.face),
                (false, false) => ranked.Minima(projection: static item => item.Z, tolerance: runtime.Absolute.Value).Map(static item => item.face),
            });
}
