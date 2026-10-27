using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SectionSubject {
    public Transform Xform { get; init; } = Transform.Identity;

    public sealed record Shape : SectionSubject {
        private Shape(GeometryBase geometry) => Geometry = geometry;

        public GeometryBase Geometry { get; }

        public static Fin<SectionSubject> Of(GeometryBase geometry) =>
            geometry is InstanceReferenceGeometry or PointCloud
                ? new RhinoFault(geometry.ObjectType.ToString(), RhinoReason.Mismatched)
                : new Shape(geometry);
    }

    public sealed record Cloud(PointCloud Geometry, bool PointCloudAsPoints) : SectionSubject;

    public sealed record Instance(InstanceReferenceGeometry Geometry, RhinoDoc Document, bool IncludeHiddenObjects) : SectionSubject;

    internal (GeometryBase Geometry, bool PointCloudAsPoints, bool IncludeHiddenObjects, RhinoDoc? Document) Arguments =>
        Switch<(GeometryBase, bool, bool, RhinoDoc?)>(
            shape: static shape => (shape.Geometry, false, false, null),
            cloud: static cloud => (cloud.Geometry, cloud.PointCloudAsPoints, false, null),
            instance: static instance => (instance.Geometry, false, instance.IncludeHiddenObjects, instance.Document));
}

public sealed record Section(Plane Plane, Seq<Curve> Curves, Seq<Point3d> Points);

[ComplexValueObject(SkipToString = true)]
[ValidationError<RhinoFault>]
public sealed partial class MeshField {
    public Mesh Mesh { get; }
    public Seq<double> Values { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Mesh mesh, ref Seq<double> values) =>
        validationError = RhinoFault.Unless(values.Count == mesh.Vertices.Count && mesh.TextureCoordinates.Count == mesh.Vertices.Count && values.ForAll(double.IsFinite))
            .Match(Succ: static _ => default, Fail: static error => error.Filter<RhinoFault>().Head is RhinoFault fault ? fault : null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sections {
    // --- [CUTS]
    public static IO<Seq<Section>> Cut(Tolerances tolerances, SectionSubject source, Frames frames, bool joinCurves) =>
        from planes in IO.lift(() => frames.Planes)
        from cache in use(static () => new PlaneIntersectionCache())
        from token in cancelToken
        from sections in Callbacks.Each(planes.Map((plane, index) => Sectioned(tolerances, source, plane, index, joinCurves, cache, token)))
        select sections;

    private static IO<Section> Sectioned(Tolerances tolerances, SectionSubject source, Plane plane, int index, bool joinCurves, PlaneIntersectionCache cache, CancellationToken token) =>
        source.Arguments switch {
            var (geometry, pointCloudAsPoints, includeHiddenObjects, document) =>
                from cut in IO.lift(() => RhinoFault.Unless(
                        Intersection.GeometryPlane(geometry, plane, tolerances.Absolute, joinCurves, pointCloudAsPoints, includeHiddenObjects, document, source.Xform, cache, token, out Curve[] curves, out Point3d[] points),
                        index)
                    .Map(_ => (Curves: curves, Points: points)))
                from kept in Products.Made(cut.Curves)
                select new Section(plane, kept, toSeq(cut.Points)),
        };

    // --- [LEVELS]
    public static IO<Seq<(double Level, Seq<Curve> Curves)>> Levels(Tolerances tolerances, MeshField field, Seq<double> levels) =>
        from lifted in Copies.Duplicate(field.Mesh)
        from written in IO.lift(() => Callbacks.Each(field.Values, (value, vertex) => RhinoFault.Unless(
            lifted.Vertices.SetVertex(vertex, field.Mesh.TextureCoordinates[vertex].X, field.Mesh.TextureCoordinates[vertex].Y, value), vertex)))
        from normals in IO.lift(lifted.RebuildNormals)
        from cache in use(static () => new MeshIntersectionCache())
        from sections in levels.Map(static (level, index) => (Level: level, Index: index)).TraverseM(row => Level(tolerances, field.Mesh, lifted, cache, row.Level, row.Index)).As()
        select sections;

    private static IO<(double Level, Seq<Curve> Curves)> Level(Tolerances tolerances, Mesh source, Mesh lifted, MeshIntersectionCache cache, double level, int index) =>
        from loops in IO.lift(() => Callbacks.Each(
            Conversions.Rows(Intersection.MeshPlane(lifted, cache, [new Plane(new Point3d(0d, 0d, level), Vector3d.ZAxis)], tolerances.MeshIntersection)),
            (loop, _) => Mapped(tolerances, source, lifted, loop, index)))
        from pieces in Products.Made(loops.Map(static loop => loop.ToPolylineCurve()))
        from curves in Products.Made(Curve.JoinCurves(pieces, tolerances.Absolute))
        select (level, curves);

    private static Fin<Polyline> Mapped(Tolerances tolerances, Mesh source, Mesh lifted, Polyline loop, int index) =>
        Callbacks.Each(toSeq(loop), (point, _) =>
                from found in Conversions.Found(lifted.ClosestMeshPoint(point, tolerances.MeshIntersection), index)
                from mapped in Conversions.Found(Conversions.Present(source.PointAt(found.FaceIndex, found.T[0], found.T[1], found.T[2], found.T[3])), index)
                select mapped)
            .Map(static points => new Polyline(points));
}
