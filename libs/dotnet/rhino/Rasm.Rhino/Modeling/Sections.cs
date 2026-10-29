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

public sealed record MeshField {
    private MeshField(Mesh mesh, Seq<double> values) => (Mesh, Values) = (mesh, values);

    public Mesh Mesh { get; }

    public Seq<double> Values { get; }

    public static Fin<MeshField> Validated(Mesh mesh, Seq<double> values) =>
        (RhinoFault.Unless(values.Count == mesh.Vertices.Count && mesh.TextureCoordinates.Count == mesh.Vertices.Count, RhinoReason.Mismatched, (values.Count, mesh.Vertices.Count, mesh.TextureCoordinates.Count)),
         Products.Each(values, static (value, index) => RhinoFault.Unless(double.IsFinite(value), RhinoReason.Outside, value, index)))
            .Apply((_, _) => new MeshField(mesh, values)).As();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sections {
    // --- [CUTS]
    public static IO<Seq<Section>> Cut(Tolerances tolerances, SectionSubject source, Frames frames, bool joinCurves) =>
        from planes in IO.lift(() => frames.Planes(tolerances))
        from cache in use(static () => new PlaneIntersectionCache())
        from token in cancelToken
        from sections in Products.Each(planes.Map((plane, index) => Sectioned(tolerances, source, plane, index, joinCurves, cache, token)))
        select sections;

    private static IO<Section> Sectioned(Tolerances tolerances, SectionSubject source, Plane plane, int index, bool joinCurves, PlaneIntersectionCache cache, CancellationToken token) =>
        source.Arguments switch {
            var (geometry, pointCloudAsPoints, includeHiddenObjects, document) =>
                from cut in IO.lift(() => (
                    Accepted: Intersection.GeometryPlane(geometry, plane, tolerances.Absolute, joinCurves, pointCloudAsPoints, includeHiddenObjects, document, source.Xform, cache, token, out Curve[] curves, out Point3d[] points),
                    Curves: curves, Points: points))
                from kept in Products.Many(() => cut.Curves, required: false)
                from accepted in IO.lift(() => RhinoFault.Unless(cut.Accepted, RhinoReason.Declined, plane, index))
                select new Section(plane, kept, Conversions.Rows(cut.Points)),
        };

    // --- [LEVELS]
    public static IO<Seq<(double Level, Seq<Curve> Curves)>> Levels(Tolerances tolerances, MeshField field, Seq<double> levels) =>
        from lifted in Products.Of(field.Mesh.DuplicateMesh)
        from written in IO.lift(() => Products.Each(field.Values, (value, vertex) => RhinoFault.Unless(
            lifted.Vertices.SetVertex(vertex, field.Mesh.TextureCoordinates[vertex].X, field.Mesh.TextureCoordinates[vertex].Y, value), RhinoReason.Declined, value, vertex)))
        from normals in IO.lift(lifted.RebuildNormals)
        from cache in use(static () => new MeshIntersectionCache())
        from sections in levels.Map(static (level, index) => (Level: level, Index: index)).TraverseM(row => Level(tolerances, field.Mesh, lifted, cache, row.Level, row.Index)).As()
        select sections;

    private static IO<(double Level, Seq<Curve> Curves)> Level(Tolerances tolerances, Mesh source, Mesh lifted, MeshIntersectionCache cache, double level, int index) =>
        from loops in IO.lift(() => Products.Each(
            Conversions.Rows(Intersection.MeshPlane(lifted, cache, [new Plane(new Point3d(0d, 0d, level), Vector3d.ZAxis)], tolerances.MeshIntersection)),
            (loop, _) => Mapped(tolerances, source, lifted, loop, index)))
        from pieces in Products.Many(() => loops.Map(static loop => loop.ToPolylineCurve()), required: false)
        from curves in Products.Many(() => Curve.JoinCurves(pieces, tolerances.Absolute), required: false)
        select (level, curves);

    private static Fin<Polyline> Mapped(Tolerances tolerances, Mesh source, Mesh lifted, Polyline loop, int index) =>
        Products.Each(toSeq(loop), (point, _) =>
                from found in Conversions.Required(lifted.ClosestMeshPoint(point, tolerances.MeshIntersection), index)
                from mapped in Conversions.Required(Conversions.Present(source.PointAt(found.FaceIndex, found.T[0], found.T[1], found.T[2], found.T[3])), index)
                select mapped)
            .Map(static points => new Polyline(points));
}
