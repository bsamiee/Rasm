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
                Cast<TGeometry, TOut>(query: Query<TGeometry, Interval>.Build(
                    key: DomainKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => One(key: DomainKey, value: curve.Domain),
                        _ => Fin.Fail<Seq<Interval>>(DomainKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Interval))),
                    })),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Interval>.Build(
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
                Cast<TGeometry, TOut>(query: Query<Polyline, Line>.Build(
                    key: SegmentsKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) =>
                        Many(key: SegmentsKey, values: geometry.GetSegments()))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Curve>.Build(
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
                Cast<TGeometry, TOut>(query: Query<TGeometry, Curve>.Build(
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
                Cast<TGeometry, TOut>(query: Query<TGeometry, Polyline>.Build(
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
                    IsoStatus.X => geometry.Domain(direction: 0) switch {
                        Interval domain when domain.IsValid && normalized is >= 0.0 and <= 1.0 => geometry switch {
                            BrepFace face => Many(key: IsoKey, values: face.TrimAwareIsoCurve(direction: 0, constantParameter: domain.ParameterAt(normalized))),
                            _ => Optional(geometry.IsoCurve(iso, domain.ParameterAt(normalized)))
                                .ToFin(IsoKey.InvalidResult())
                                .Map(static (Curve curve) => Seq(curve)),
                        },
                        _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                    },
                    IsoStatus.Y => geometry.Domain(direction: 1) switch {
                        Interval domain when domain.IsValid && normalized is >= 0.0 and <= 1.0 => geometry switch {
                            BrepFace face => Many(key: IsoKey, values: face.TrimAwareIsoCurve(direction: 1, constantParameter: domain.ParameterAt(normalized))),
                            _ => Optional(geometry.IsoCurve(iso, domain.ParameterAt(normalized)))
                                .ToFin(IsoKey.InvalidResult())
                                .Map(static (Curve curve) => Seq(curve)),
                        },
                        _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                    },
                    _ => Fin.Fail<Seq<Curve>>(IsoKey.InvalidInput()),
                });

    public static Query<TGeometry, TOut> SolidOrientation<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(BrepSolidOrientation) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, BrepSolidOrientation>.Build(
                    key: SolidOrientationKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => One(key: SolidOrientationKey, value: brep.SolidOrientation),
                        _ => Fin.Fail<Seq<BrepSolidOrientation>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BrepSolidOrientation))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, int>.Build(
                    key: SolidOrientationKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => One(key: SolidOrientationKey, value: mesh.SolidOrientation()),
                        _ => Fin.Fail<Seq<int>>(SolidOrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(int))),
                    })),
            _ => SolidOrientationKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase =>
        typeof(TGeometry) switch {
            Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) =>
                Cast<TGeometry, bool>(query: Query<TGeometry, bool>.Build(
                    key: IsPointInsideKey,
                    state: point,
                    requirement: GeometryRequirement.SolidTopology,
                    evaluator: static (Point3d target, TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                        Brep brep => context
                            .Bind((GeometryContext geometryContext) => One(
                                key: IsPointInsideKey,
                                value: brep.IsPointInside(
                                    point: target,
                                    tolerance: geometryContext.Absolute.Value,
                                    strictlyIn: false))),
                        _ => Fin.Fail<Seq<bool>>(IsPointInsideKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                    })),
            Type geometry when typeof(Mesh).IsAssignableFrom(c: geometry) =>
                Cast<TGeometry, bool>(query: Query<TGeometry, bool>.Build(
                    key: IsPointInsideKey,
                    state: point,
                    requirement: GeometryRequirement.SolidTopology,
                    evaluator: static (Point3d target, TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                        Mesh mesh => context
                            .Bind((GeometryContext geometryContext) => One(
                                key: IsPointInsideKey,
                                value: mesh.IsPointInside(
                                    point: target,
                                    tolerance: geometryContext.Absolute.Value,
                                    strictlyIn: false))),
                        _ => Fin.Fail<Seq<bool>>(IsPointInsideKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                    })),
            _ => IsPointInsideKey.Unsupported<TGeometry, bool>(),
        };

    public static Query<TGeometry, TOut> Vertices<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Point3d>.Build(
                    key: VerticesKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => Many(key: VerticesKey, values: brep.DuplicateVertices()),
                        _ => Fin.Fail<Seq<Point3d>>(VerticesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Point3d>.Build(
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
                Cast<TGeometry, TOut>(query: Query<TGeometry, Brep>.Build(
                    key: ComponentsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Brep brep => brep.GetConnectedComponents() switch {
                            Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
                            _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
                        },
                        _ => Fin.Fail<Seq<Brep>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Brep))),
                    })),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Mesh>.Build(
                    key: ComponentsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Mesh mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()),
                        _ => Fin.Fail<Seq<Mesh>>(ComponentsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Mesh))),
                    })),
            _ => ComponentsKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<Mesh, bool> IsManifold =>
        Query<Mesh, bool>.Build(key: IsManifoldKey, evaluator: static (Mesh geometry, Fin<GeometryContext> _) => One(key: IsManifoldKey, value: geometry.IsManifold()));

    public static Query<Mesh, bool> NakedPointStatus =>
        Query<Mesh, bool>.Build(key: NakedPointStatusKey, evaluator: static (Mesh geometry, Fin<GeometryContext> _) => Many(key: NakedPointStatusKey, values: geometry.GetNakedEdgePointStatus()));

    public static Query<Mesh, Polyline> SelfIntersections =>
        Query<Mesh, Polyline>.Build(
            key: SelfIntersectionsKey,
            requirement: GeometryRequirement.Basic,
            evaluator: static (Mesh geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext geometryContext) => {
                        using TextLog textLog = new();
                        return geometry.GetSelfIntersections(
                            tolerance: geometryContext.Absolute.Value,
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
