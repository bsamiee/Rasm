using System.Runtime.CompilerServices;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MappingPrimitive {
    public sealed record Framed(Plane Plane, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingPrimitive;

    public sealed record Cylindrical(Cylinder Cylinder, bool Capped) : MappingPrimitive;

    public sealed record Spherical(Sphere Sphere) : MappingPrimitive;

    public sealed record CustomMesh(Mesh Mesh) : MappingPrimitive;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Mappings {
    // --- [DECODE]
    public static IO<Option<MappingPrimitive>> Primitive(TextureMapping mapping) =>
        IO.lift(() => mapping.MappingType).Bind(kind => kind switch {
            TextureMappingType.PlaneMapping or TextureMappingType.OcsMapping => Products.Accepted(() =>
                (mapping.TryGetMappingPlane(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), (MappingPrimitive)new MappingPrimitive.Framed(plane, dx, dy, dz, capped))).Map(static primitive => Some(primitive)),
            TextureMappingType.BoxMapping => Products.Accepted(() =>
                (mapping.TryGetMappingBox(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), (MappingPrimitive)new MappingPrimitive.Framed(plane, dx, dy, dz, capped))).Map(static primitive => Some(primitive)),
            TextureMappingType.CylinderMapping => Products.Accepted(() =>
                (mapping.TryGetMappingCylinder(out Cylinder cylinder, out bool capped), (MappingPrimitive)new MappingPrimitive.Cylindrical(cylinder, capped))).Map(static primitive => Some(primitive)),
            TextureMappingType.SphereMapping => Products.Accepted(() =>
                (mapping.TryGetMappingSphere(out Sphere sphere), (MappingPrimitive)new MappingPrimitive.Spherical(sphere))).Map(static primitive => Some(primitive)),
            TextureMappingType.MeshMappingPrimitive => Products.Accepted(() => (mapping.TryGetMappingMesh(out Mesh mesh), mesh))
                .Bind(static mesh => use(() => mesh)).Map(static mesh => Some<MappingPrimitive>(new MappingPrimitive.CustomMesh(mesh))),
            TextureMappingType.None or TextureMappingType.SurfaceParameters or TextureMappingType.SurfaceMappingPrimitive
                or TextureMappingType.BrepMappingPrimitive or TextureMappingType.FalseColors => IO.pure(Option<MappingPrimitive>.None),
        });

    public static IO<(Vector3d Position, Vector3d Scale, Vector3d Rotation, Vector3d UvwOffset, Vector3d UvwRepeat, Vector3d UvwRotation)> Decompose(TextureMapping mapping, Transform localTransform) =>
        IO.lift(() => {
            mapping.Decompose(localTransform, out Vector3d position, out Vector3d scale, out Vector3d rotation, out Vector3d offset, out Vector3d repeat, out Vector3d uvwRotation);
            return (position, scale, rotation, offset, repeat, uvwRotation);
        });

    // --- [EVALUATION]
    public static IO<Seq<(Point3d Texture, int Side)>> Evaluate(TextureMapping mapping, Seq<(Point3d Point, Vector3d Normal)> samples, Transform objectTransform) =>
        from moved in IO.lift(() => !objectTransform.IsIdentity && objectTransform.TryGetInverse(out Transform inverse) ? Some((Points: inverse, Normals: objectTransform.Transpose())) : None)
        from result in IO.lift(() => Products.Each(samples, (sample, index) => moved.Match(
            Some: xforms => Mapped(mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture, xforms.Points, xforms.Normals), texture, index),
            None: () => Mapped(mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture), texture, index))))
        select result;

    private static Fin<(Point3d Texture, int Side)> Mapped(int side, Point3d texture, int index, [CallerArgumentExpression(nameof(side))] string call = "") =>
        side is not 0 ? (texture, side) : new RhinoFault(call, RhinoReason.Declined, index);

    // --- [CHANNELS]
    public static IO<Seq<(int Channel, TextureMapping Mapping, Transform ObjectTransform)>> Channels(RhinoObject rhinoObject) =>
        from channels in IO.lift(() => Conversions.Rows(rhinoObject.GetTextureChannels()))
        from held in channels.TraverseM(channel => Products.Of(() => (rhinoObject.GetTextureMapping(channel, out Transform objectTransform), objectTransform))
            .Map(mapping => (Channel: channel, Mapping: mapping.Value, ObjectTransform: mapping.Out))).As()
        select held;

    public static IO<Unit> Assign(RhinoObject rhinoObject, int channel, TextureMapping mapping, Option<Transform> objectTransform = default) =>
        from accepted in IO.lift(() => RhinoFault.Unless((mapping.MappingType is TextureMappingType.OcsMapping) == (channel == ObjectAttributes.OCSMappingChannelId), RhinoReason.Mismatched, (mapping.MappingType, channel)))
        from assigned in objectTransform.Match(
            Some: xform => Products.Accepted(() => rhinoObject.SetTextureMapping(channel, mapping, xform) is not 0),
            None: () => Products.Accepted(() => rhinoObject.SetTextureMapping(channel, mapping) is not 0))
        select assigned;

    // --- [COORDINATES]
    public static IO<Option<Either<Seq<Point2d>, Seq<Point3d>>>> Coordinates(Mesh mesh, Guid mappingId) =>
        IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(mappingId)).Traverse(Copied).As());

    public static IO<Option<Either<Seq<Point2d>, Seq<Point3d>>>> Coordinates(Mesh mesh, RhinoObject rhinoObject, Material material, Texture texture) =>
        from cached in IO.lift(() => mesh.SetCachedTextureCoordinatesFromMaterial(rhinoObject, material))
        from coordinates in IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(rhinoObject, texture)).Traverse(Copied).As())
        select coordinates;

    private static Fin<Either<Seq<Point2d>, Seq<Point3d>>> Copied(CachedTextureCoordinates held) =>
        RhinoFault.Unless(held.Dim is 2 or 3, RhinoReason.Unsupported, held.Dim).Map(_ => Conversions.Rows(held) switch {
            var points when held.Dim is 2 => (Either<Seq<Point2d>, Seq<Point3d>>)new Either<Seq<Point2d>, Seq<Point3d>>.Left(points.Map(static point => new Point2d(point.X, point.Y))),
            var points => new Either<Seq<Point2d>, Seq<Point3d>>.Right(points),
        });
}
