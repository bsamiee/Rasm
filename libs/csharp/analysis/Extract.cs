using System.Threading;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
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
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => One(key: DomainKey, value: curve.Domain),
                        _ => Fin.Fail<Seq<Interval>>(DomainKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Interval))),
                    })),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Cast<TGeometry, TOut>(key: DomainKey, query: Query<TGeometry, Interval>.Build(
                    key: DomainKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Surface surface => (One(key: DomainKey, value: surface.Domain(direction: 0)), One(key: DomainKey, value: surface.Domain(direction: 1)))
                            .Apply((Seq<Interval> u, Seq<Interval> v) => u + v)
                            .As(),
                        _ => Fin.Fail<Seq<Interval>>(DomainKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Interval))),
                    })),
            _ => DomainKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Line) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<Polyline, Line>.Build(
                    key: SegmentsKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) =>
                        Many(key: SegmentsKey, values: geometry.GetSegments()))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<TGeometry, Curve>.Build(
                    key: SegmentsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => Many(key: SegmentsKey, values: curve.GetSubCurves()),
                        _ => Fin.Fail<Seq<Curve>>(SegmentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
                    })),
            _ => SegmentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Brep, Curve> Edges =>
        Query<Brep, Curve>.Build(key: EdgesKey, evaluator: static (Brep geometry, Fin<GeometryContext> _) => Many(key: EdgesKey, values: geometry.DuplicateEdgeCurves()));
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: NakedEdgesKey, query: Query<TGeometry, Curve>.Build(
                    key: NakedEdgesKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => Many(
                            key: NakedEdgesKey,
                            values: brep.DuplicateNakedEdgeCurves(
                                nakedOuter: true,
                                nakedInner: true)),
                        _ => Fin.Fail<Seq<Curve>>(NakedEdgesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                Cast<TGeometry, TOut>(key: NakedEdgesKey, query: Query<TGeometry, Polyline>.Build(
                    key: NakedEdgesKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => Many(key: NakedEdgesKey, values: mesh.GetNakedEdges()),
                        _ => Fin.Fail<Seq<Polyline>>(NakedEdgesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Polyline))),
                    })),
            _ => NakedEdgesKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Mesh, Polyline> Outlines(Plane plane) =>
        Query<Mesh, Polyline>.Build(key: OutlinesKey, evaluator: (Mesh geometry, Fin<GeometryContext> _) => Many(key: OutlinesKey, values: geometry.GetOutlines(plane: plane)));
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) =>
        Query<Surface, Curve>.Build(
            key: IsoKey,
            requirement: GeometryRequirement.SurfaceEvaluation,
            evaluator: (Surface geometry, Fin<GeometryContext> _) =>
                iso switch {
                    IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North =>
                        Optional(geometry.IsoCurve(iso: iso))
                            .ToFin(IsoKey.InvalidResult())
                            .Map(static (Curve curve) => Seq(curve)),
                    IsoStatus.X or IsoStatus.Y => iso switch {
                        IsoStatus.X => 0,
                        _ => 1,
                    } switch {
                        int direction => geometry.Domain(direction: direction) switch {
                            Interval domain when domain.IsValid && normalized is >= 0.0 and <= 1.0 => geometry switch {
                                BrepFace face => Many(key: IsoKey, values: face.TrimAwareIsoCurve(direction: direction, constantParameter: domain.ParameterAt(normalized))),
                                _ => Optional(geometry.IsoCurve(iso, domain.ParameterAt(normalized)))
                                    .ToFin(IsoKey.InvalidResult())
                                    .Map(static (Curve curve) => Seq(curve)),
                            },
                            _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                        },
                    },
                    _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                });
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
    public static Query<TGeometry, TOut> SolidOrientation<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(BrepSolidOrientation) =>
                Cast<TGeometry, TOut>(key: SolidOrientationKey, query: Query<TGeometry, BrepSolidOrientation>.Build(
                    key: SolidOrientationKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => One(key: SolidOrientationKey, value: brep.SolidOrientation),
                        _ => Fin.Fail<Seq<BrepSolidOrientation>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BrepSolidOrientation))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) =>
                Cast<TGeometry, TOut>(key: SolidOrientationKey, query: Query<TGeometry, int>.Build(
                    key: SolidOrientationKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => One(key: SolidOrientationKey, value: mesh.SolidOrientation()),
                        _ => Fin.Fail<Seq<int>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(int))),
                    })),
            _ => SolidOrientationKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase =>
        typeof(TGeometry) switch {
            Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) || typeof(Mesh).IsAssignableFrom(c: geometry) =>
                Cast<TGeometry, bool>(key: IsPointInsideKey, query: Query<TGeometry, bool>.Build(
                    key: IsPointInsideKey,
                    state: point,
                    requirement: GeometryRequirement.SolidTopology,
                    evaluator: static (Point3d target, TGeometry geometry, Fin<GeometryContext> context) => context.Bind((GeometryContext geometryContext) => geometry switch {
                        Brep brep => One(key: IsPointInsideKey, value: brep.IsPointInside(point: target, tolerance: geometryContext.Absolute.Value, strictlyIn: false)),
                        Mesh mesh => One(key: IsPointInsideKey, value: mesh.IsPointInside(point: target, tolerance: geometryContext.Absolute.Value, strictlyIn: false)),
                        _ => Fin.Fail<Seq<bool>>(IsPointInsideKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                    }))),
            _ => IsPointInsideKey.Unsupported<TGeometry, bool>(),
        };
    public static Query<TGeometry, TOut> Vertices<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                    key: VerticesKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => Many(key: VerticesKey, values: brep.DuplicateVertices()),
                        _ => Fin.Fail<Seq<Point3d>>(VerticesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                    key: VerticesKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => Many(key: VerticesKey, values: mesh.Vertices.ToPoint3dArray()),
                        _ => Fin.Fail<Seq<Point3d>>(VerticesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    })),
            _ => VerticesKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Brep) =>
                Cast<TGeometry, TOut>(key: ComponentsKey, query: Query<TGeometry, Brep>.Build(
                    key: ComponentsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => brep.GetConnectedComponents() switch {
                            Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
                            _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
                        },
                        _ => Fin.Fail<Seq<Brep>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Brep))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) =>
                Cast<TGeometry, TOut>(key: ComponentsKey, query: Query<TGeometry, Mesh>.Build(
                    key: ComponentsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()),
                        _ => Fin.Fail<Seq<Mesh>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Mesh))),
                    })),
            _ => ComponentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Topology<TGeometry, TOut>(Topology aspect) where TGeometry : notnull =>
        (aspect.Kind, typeof(TGeometry), typeof(TOut)) switch {
            (TopologyKind.Boundary, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                NakedEdges<TGeometry, TOut>(),
            (TopologyKind.Boundary, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                NakedEdges<TGeometry, TOut>(),
            (TopologyKind.Adjacency, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
                    key: TopologyKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => Many(key: TopologyKey, values: Enumerable
                            .Range(start: 0, count: brep.Edges.Count)
                            .Select(static (int index) => new ComponentIndex(
                                type: ComponentIndexType.BrepEdge,
                                index: index))),
                        _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
                    })),
            (TopologyKind.Adjacency, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
                    key: TopologyKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => Many(key: TopologyKey, values: Enumerable
                            .Range(start: 0, count: mesh.TopologyEdges.Count)
                            .Select(static (int index) => new ComponentIndex(
                                type: ComponentIndexType.MeshTopologyEdge,
                                index: index))),
                        _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
                    })),
            (TopologyKind.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
                    key: TopologyKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => Many(key: TopologyKey, values: Enumerable
                            .Range(start: 0, count: brep.Edges.Count)
                            .Where((int index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold)
                            .Select(static (int index) => new ComponentIndex(
                                type: ComponentIndexType.BrepEdge,
                                index: index))),
                        _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
                    })),
            (TopologyKind.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, ComponentIndex>.Build(
                    key: TopologyKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => Many(key: TopologyKey, values: Enumerable
                            .Range(start: 0, count: mesh.TopologyEdges.Count)
                            .Where((int index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2)
                            .Select(static (int index) => new ComponentIndex(
                                type: ComponentIndexType.MeshTopologyEdge,
                                index: index))),
                        _ => Fin.Fail<Seq<ComponentIndex>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(ComponentIndex))),
                    })),
            (TopologyKind.NonManifold, Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, bool>.Build(
                    key: TopologyKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => One(key: TopologyKey, value: Enumerable
                            .Range(start: 0, count: brep.Edges.Count)
                            .Any((int index) => brep.Edges[index].Valence == EdgeAdjacency.NonManifold)),
                        _ => Fin.Fail<Seq<bool>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                    })),
            (TopologyKind.NonManifold, Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(bool) =>
                Cast<TGeometry, TOut>(key: TopologyKey, query: Query<TGeometry, bool>.Build(
                    key: TopologyKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => One(key: TopologyKey, value: Enumerable
                            .Range(start: 0, count: mesh.TopologyEdges.Count)
                            .Any((int index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2)),
                        _ => Fin.Fail<Seq<bool>>(TopologyKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                    })),
            _ => TopologyKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Mesh, bool> IsManifold =>
        Query<Mesh, bool>.Build(key: IsManifoldKey, evaluator: static (Mesh geometry, Fin<GeometryContext> _) => One(key: IsManifoldKey, value: geometry.IsManifold()));
    public static Query<Mesh, bool> NakedPointStatus =>
        Query<Mesh, bool>.Build(key: NakedPointStatusKey, evaluator: static (Mesh geometry, Fin<GeometryContext> _) => Many(key: NakedPointStatusKey, values: geometry.GetNakedEdgePointStatus()));
    public static Query<Mesh, MeshCheckParameters> MeshCheck =>
        Query<Mesh, MeshCheckParameters>.Build(
            key: MeshCheckKey,
            evaluator: static (Mesh geometry, Fin<GeometryContext> _) => {
                using TextLog textLog = new();
                MeshCheckParameters parameters = MeshCheckParameters.Defaults();
                return geometry.Check(
                    textLog: textLog,
                    parameters: ref parameters) switch {
                        true or false => One(key: MeshCheckKey, value: parameters),
                    };
            });
    public static Query<Mesh, int> MeshCheckCount(Analysis.MeshCheckCount count) =>
        count switch {
            Analysis.MeshCheckCount.None => Query<Mesh, int>.Reject(
                key: MeshCheckCountKey,
                fault: MeshCheckCountKey.InvalidInput()),
            Analysis.MeshCheckCount.DegenerateFaces
                or Analysis.MeshCheckCount.DisjointMeshes
                or Analysis.MeshCheckCount.DuplicateFaces
                or Analysis.MeshCheckCount.ExtremelyShortEdges
                or Analysis.MeshCheckCount.InvalidNgons
                or Analysis.MeshCheckCount.NakedEdges
                or Analysis.MeshCheckCount.NonManifoldEdges
                or Analysis.MeshCheckCount.NonUnitVectorNormals
                or Analysis.MeshCheckCount.RandomFaceNormals
                or Analysis.MeshCheckCount.SelfIntersectingPairs
                or Analysis.MeshCheckCount.UnusedVertices
                or Analysis.MeshCheckCount.VertexFaceNormalsDiffer
                or Analysis.MeshCheckCount.ZeroLengthNormals => Query<Mesh, int>.Build(
                key: MeshCheckCountKey,
                state: count,
                evaluator: static (Analysis.MeshCheckCount aspect, Mesh geometry, Fin<GeometryContext> context) => (
                    Fin.Succ(aspect),
                    MeshCheck
                        .Apply(
                            geometry: geometry,
                            context: context)
                        .Bind(static (Seq<MeshCheckParameters> values) =>
                            values.Head.ToFin(MeshCheckCountKey.InvalidResult()))
                ).Apply(static (Analysis.MeshCheckCount selected, MeshCheckParameters parameters) => (
                    Count: selected,
                    Parameters: parameters))
                .As()
                .Bind(static ((Analysis.MeshCheckCount Count, MeshCheckParameters Parameters) state) => state.Count switch {
                    Analysis.MeshCheckCount.DegenerateFaces => One(key: MeshCheckCountKey, value: state.Parameters.DegenerateFaceCount),
                    Analysis.MeshCheckCount.DisjointMeshes => One(key: MeshCheckCountKey, value: state.Parameters.DisjointMeshCount),
                    Analysis.MeshCheckCount.DuplicateFaces => One(key: MeshCheckCountKey, value: state.Parameters.DuplicateFaceCount),
                    Analysis.MeshCheckCount.ExtremelyShortEdges => One(key: MeshCheckCountKey, value: state.Parameters.ExtremelyShortEdgeCount),
                    Analysis.MeshCheckCount.InvalidNgons => One(key: MeshCheckCountKey, value: state.Parameters.InvalidNgonCount),
                    Analysis.MeshCheckCount.NakedEdges => One(key: MeshCheckCountKey, value: state.Parameters.NakedEdgeCount),
                    Analysis.MeshCheckCount.NonManifoldEdges => One(key: MeshCheckCountKey, value: state.Parameters.NonManifoldEdgeCount),
                    Analysis.MeshCheckCount.NonUnitVectorNormals => One(key: MeshCheckCountKey, value: state.Parameters.NonUnitVectorNormalCount),
                    Analysis.MeshCheckCount.RandomFaceNormals => One(key: MeshCheckCountKey, value: state.Parameters.RandomFaceNormalCount),
                    Analysis.MeshCheckCount.SelfIntersectingPairs => One(key: MeshCheckCountKey, value: state.Parameters.SelfIntersectingPairsCount),
                    Analysis.MeshCheckCount.UnusedVertices => One(key: MeshCheckCountKey, value: state.Parameters.UnusedVertexCount),
                    Analysis.MeshCheckCount.VertexFaceNormalsDiffer => One(key: MeshCheckCountKey, value: state.Parameters.VertexFaceNormalsDifferCount),
                    Analysis.MeshCheckCount.ZeroLengthNormals => One(key: MeshCheckCountKey, value: state.Parameters.ZeroLengthNormalCount),
                    _ => Fin.Fail<Seq<int>>(MeshCheckCountKey.InvalidInput()),
                })),
            _ => Query<Mesh, int>.Reject(
                key: MeshCheckCountKey,
                fault: MeshCheckCountKey.InvalidInput()),
        };
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(Analysis.MeshFaceMetric metric) =>
        metric switch {
            Analysis.MeshFaceMetric.AspectRatio => Query<Mesh, MeshFaceSample>.Build(
                key: MeshFaceMetricKey,
                requirement: GeometryRequirement.MeshCheck,
                evaluator: static (Mesh geometry, Fin<GeometryContext> _) => Enumerable
                    .Range(start: 0, count: geometry.Faces.Count)
                    .Select((int face) => geometry.Faces.GetFaceAspectRatio(index: face) switch {
                        double value when Rhino.RhinoMath.IsValidDouble(x: value) && value >= 0.0 =>
                            Fin.Succ(new MeshFaceSample(Face: face, Value: value)),
                        _ => Fin.Fail<MeshFaceSample>(MeshFaceMetricKey.InvalidResult()),
                    })
                    .Aggregate(
                        seed: Fin.Succ(Seq<MeshFaceSample>()),
                        func: static (Fin<Seq<MeshFaceSample>> current, Fin<MeshFaceSample> sample) => (
                            current,
                            sample
                        ).Apply(static (Seq<MeshFaceSample> values, MeshFaceSample next) => values.Add(next)).As())),
            _ => Query<Mesh, MeshFaceSample>.Reject(
                key: MeshFaceMetricKey,
                fault: MeshFaceMetricKey.InvalidInput()),
        };
    public static Query<Mesh, Polyline> SelfIntersections =>
        Query<Mesh, Polyline>.Build(
            key: SelfIntersectionsKey,
            requirement: GeometryRequirement.Basic,
            evaluator: static (Mesh geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext geometryContext) => {
                        using TextLog textLog = new();
                        return geometry.GetSelfIntersections(
                            tolerance: geometryContext.MeshIntersectionTolerance,
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
                    }));
}
