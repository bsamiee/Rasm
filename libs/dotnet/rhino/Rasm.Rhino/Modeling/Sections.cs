using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Geometry.Collections;
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
                ? Error.New(ErrorCodes.ValidationFailed, $"{geometry.ObjectType} cuts through its own {nameof(SectionSubject)} case, not {nameof(Shape)}")
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
        validationError = values.Count == mesh.Vertices.Count && mesh.TextureCoordinates.Count == mesh.Vertices.Count && values.ForAll(double.IsFinite)
            ? null
            : new RhinoFault($"mesh field needs one finite value and one texture coordinate per vertex, received {values.Count} values and {mesh.TextureCoordinates.Count} texture coordinates for {mesh.Vertices.Count} vertices");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sections {
    // --- [CUTS]
    public static IO<Seq<Section>> Cut(Tolerances tolerances, SectionSubject source, Frames frames, bool joinCurves, Action<IDisposable> retain) =>
        from planes in IO.lift(() => frames.Planes)
        from sections in DisposalOps.Within(static () => new PlaneIntersectionCache(), cache =>
            Copies.Acquire(nameof(Intersection.GeometryPlane), token =>
                planes.Map((plane, index) => Sectioned(tolerances, source, plane, index, joinCurves, cache, token)).Traverse(static section => section).As(), retain))
        select sections;

    private static Answer<Section> Sectioned(Tolerances tolerances, SectionSubject source, Plane plane, int index, bool joinCurves, PlaneIntersectionCache cache, CancellationToken token) =>
        source.Arguments switch {
            var (geometry, pointCloudAsPoints, includeHiddenObjects, document) =>
                Intersection.GeometryPlane(geometry, plane, tolerances.Absolute, joinCurves, pointCloudAsPoints, includeHiddenObjects, document, source.Xform, cache, token, out Curve[] curves, out Point3d[] points) switch {
                    var computed => (Answer.Many(curves), new Answer<Unit>([], computed ? unit : Seq((Reason: Refused.Declined, Index: Some(index)))))
                        .Apply((kept, _) => new Section(plane, kept, toSeq(points)))
                        .As(),
                },
        };

    // --- [LEVELS]
    public static IO<Seq<(double Level, Seq<Curve> Curves)>> Levels(Tolerances tolerances, MeshField field, Seq<double> levels, Action<IDisposable> retain) =>
        DisposalOps.Within<IDisposable, Seq<(double Level, Seq<Curve> Curves)>>(temporary =>
            from lifted in Copies.Duplicate(field.Mesh, temporary)
            from written in IO.lift(() => Callbacks.Each(field.Values, (value, vertex) => Refused.Unless(
                lifted.Vertices.SetVertex(vertex, field.Mesh.TextureCoordinates[vertex].X, field.Mesh.TextureCoordinates[vertex].Y, value),
                nameof(MeshVertexList.SetVertex),
                vertex)))
            from normals in IO.lift(lifted.RebuildNormals)
            from sections in DisposalOps.Within(static () => new MeshIntersectionCache(), cache =>
                levels.Map((level, index) => Level(tolerances, field.Mesh, lifted, cache, level, index, temporary, retain)).TraverseM(static level => level).As())
            select sections,
            static held => DisposalOps.Release(held));

    private static IO<(double Level, Seq<Curve> Curves)> Level(
        Tolerances tolerances, Mesh source, Mesh lifted, MeshIntersectionCache cache, double level, int index, Action<IDisposable> temporary, Action<IDisposable> retain) =>
        from loops in IO.lift(() => Callbacks.Each(
            Conversions.Rows(Intersection.MeshPlane(lifted, cache, [new Plane(new Point3d(0d, 0d, level), Vector3d.ZAxis)], tolerances.MeshIntersection)),
            (loop, _) => Mapped(tolerances, source, lifted, loop, index)))
        from pieces in Copies.Acquire(nameof(Polyline.ToPolylineCurve), _ => Answer.Many(loops.Map(static loop => loop.ToPolylineCurve())), temporary)
        from curves in Copies.Acquire(nameof(Curve.JoinCurves), _ => Answer.Many(Curve.JoinCurves(pieces, tolerances.Absolute)), retain)
        select (level, curves);

    private static Fin<Polyline> Mapped(Tolerances tolerances, Mesh source, Mesh lifted, Polyline loop, int index) =>
        Callbacks.Each(toSeq(loop), (point, _) =>
                from found in Optional(lifted.ClosestMeshPoint(point, tolerances.MeshIntersection)).ToFin(new Refused(nameof(Mesh.ClosestMeshPoint), Refused.Absent, index))
                let mapped = source.PointAt(found.FaceIndex, found.T[0], found.T[1], found.T[2], found.T[3])
                from valid in Refused.Unless(mapped.IsValid, mapped, nameof(Mesh.PointAt), index)
                select valid)
            .Map(static points => new Polyline(points));
}
