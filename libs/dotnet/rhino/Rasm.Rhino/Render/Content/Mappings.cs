using System.Runtime.CompilerServices;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MappingPrimitive {
    public sealed record SurfaceParameter : MappingPrimitive;

    public sealed record Planar(Plane Plane, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingPrimitive;

    public sealed record Ocs(Plane Plane) : MappingPrimitive;

    public sealed record Cylindrical(Cylinder Cylinder, bool Capped) : MappingPrimitive;

    public sealed record Spherical(Sphere Sphere) : MappingPrimitive;

    public sealed record Box(Plane Plane, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingPrimitive;

    public sealed record CustomMesh(Mesh Mesh) : MappingPrimitive;
}

public sealed record MappingSettings(TextureSpace TextureSpace, Projection Projection, Transform UvwTransform, Transform PrimitiveTransform);

public sealed record MappingState(Option<Guid> Id, Either<TextureMappingType, MappingPrimitive> Shape, MappingSettings Settings);

public sealed record MappingEntry(int Channel, TextureMapping Mapping, Transform ObjectTransform);

public sealed record MappingDecomposition(Vector3d Position, Vector3d Scale, Vector3d Rotation, Vector3d UvwOffset, Vector3d UvwRepeat, Vector3d UvwRotation);

public sealed record MappingSample(Point3d Point, Vector3d Normal);

public sealed record MappedPoint(Point3d Texture, int Side);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Mappings {
    // --- [MAPPINGS]
    public static IO<TValue> WithMapping<TValue>(MappingPrimitive primitive, Func<TextureMapping, IO<TValue>> body) =>
        unitIO.Bind(_ => primitive.Switch(
                surfaceParameter: static _ => Products.Made(TextureMapping.CreateSurfaceParameterMapping()),
                planar: static planar => Products.Made(TextureMapping.CreatePlaneMapping(planar.Plane, planar.Dx, planar.Dy, planar.Dz, planar.Capped)),
                ocs: static ocs => Products.Made(TextureMapping.CreateOcsMapping(ocs.Plane)),
                cylindrical: static cylindrical => Products.Made(TextureMapping.CreateCylinderMapping(cylindrical.Cylinder, cylindrical.Capped)),
                spherical: static spherical => Products.Made(TextureMapping.CreateSphereMapping(spherical.Sphere)),
                box: static box => Products.Made(TextureMapping.CreateBoxMapping(box.Plane, box.Dx, box.Dy, box.Dz, box.Capped)),
                customMesh: static custom => Products.Made(TextureMapping.CreateCustomMeshMapping(custom.Mesh))))
            .Bind(body)
            .Bracket();

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial void ApplySettings([MappingTarget] TextureMapping mapping, MappingSettings settings);

    public static IO<TValue> WithSnapshot<TValue>(TextureMapping mapping, Func<MappingState, IO<TValue>> body) =>
        (from kind in IO.lift(() => mapping.MappingType)
         from primitive in kind switch {
             TextureMappingType.SurfaceParameters => IO.pure(Some<MappingPrimitive>(new MappingPrimitive.SurfaceParameter())),
             TextureMappingType.PlaneMapping => IO.lift(() => RhinoFault.Unless(mapping.TryGetMappingPlane(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped))
                 .Map(_ => Some<MappingPrimitive>(new MappingPrimitive.Planar(plane, dx, dy, dz, capped)))),
             TextureMappingType.OcsMapping => IO.lift(() => RhinoFault.Unless(mapping.TryGetMappingPlane(out Plane plane, out _, out _, out _))
                 .Map(_ => Some<MappingPrimitive>(new MappingPrimitive.Ocs(plane)))),
             TextureMappingType.CylinderMapping => IO.lift(() => RhinoFault.Unless(mapping.TryGetMappingCylinder(out Cylinder cylinder, out bool capped))
                 .Map(_ => Some<MappingPrimitive>(new MappingPrimitive.Cylindrical(cylinder, capped)))),
             TextureMappingType.SphereMapping => IO.lift(() => RhinoFault.Unless(mapping.TryGetMappingSphere(out Sphere sphere))
                 .Map(_ => Some<MappingPrimitive>(new MappingPrimitive.Spherical(sphere)))),
             TextureMappingType.BoxMapping => IO.lift(() => RhinoFault.Unless(mapping.TryGetMappingBox(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped))
                 .Map(_ => Some<MappingPrimitive>(new MappingPrimitive.Box(plane, dx, dy, dz, capped)))),
             TextureMappingType.MeshMappingPrimitive =>
                 from copy in IO.lift(() => RhinoFault.Unless(mapping.TryGetMappingMesh(out Mesh mesh)).Bind(_ => Conversions.Found(mesh)))
                 from held in use(() => copy)
                 select Some<MappingPrimitive>(new MappingPrimitive.CustomMesh(held)),
             TextureMappingType.None or TextureMappingType.SurfaceMappingPrimitive or TextureMappingType.BrepMappingPrimitive or TextureMappingType.FalseColors => IO.pure(Option<MappingPrimitive>.None),
         }
         from value in body(new MappingState(Conversions.Present(mapping.Id), primitive.ToEither(kind), ToState(mapping)))
         select value).Bracket();

    public static IO<Seq<MappedPoint>> Evaluate(TextureMapping mapping, Seq<MappingSample> samples, Transform objectTransform) =>
        from moved in IO.lift(() => !objectTransform.IsIdentity && objectTransform.TryGetInverse(out Transform inverse) ? Some((Points: inverse, Normals: objectTransform.Transpose())) : None)
        from result in IO.lift(() => Callbacks.Each(samples, (sample, index) => moved.Match(
            Some: xforms => Mapped(mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture, xforms.Points, xforms.Normals), texture, index),
            None: () => Mapped(mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture), texture, index))))
        select result;

    private static Fin<MappedPoint> Mapped(int side, Point3d texture, int index, [CallerArgumentExpression(nameof(side))] string call = "") =>
        side is not 0 ? new MappedPoint(texture, side) : new RhinoFault(call, RhinoReason.Declined, index);

    public static IO<MappingDecomposition> Decompose(TextureMapping mapping, Transform localTransform) =>
        IO.lift(() => {
            mapping.Decompose(localTransform, out Vector3d position, out Vector3d scale, out Vector3d rotation, out Vector3d offset, out Vector3d repeat, out Vector3d uvwRotation);
            return (Conversions.Found(Conversions.Present(position)), Conversions.Found(Conversions.Present(scale)), Conversions.Found(Conversions.Present(rotation)),
                    Conversions.Found(Conversions.Present(offset)), Conversions.Found(Conversions.Present(repeat)), Conversions.Found(Conversions.Present(uvwRotation)))
                .Apply(static (p, s, r, o, u, w) => new MappingDecomposition(p, s, r, o, u, w)).As();
        });

    private static partial MappingSettings ToState(TextureMapping mapping);

    // --- [CHANNELS]
    public static IO<Option<TValue>> WithChannel<TValue>(RhinoObject rhinoObject, int channel, Func<MappingEntry, IO<TValue>> body) =>
        unitIO.Bind(_ => Products.Held(rhinoObject.GetTextureMapping(channel, out Transform objectTransform))
                .Bind(found => found.Traverse(mapping => body(new MappingEntry(channel, mapping, objectTransform))).As()))
            .Bracket();

    public static IO<TValue> WithChannels<TValue>(RhinoObject rhinoObject, Func<Seq<MappingEntry>, IO<TValue>> body) =>
        (from channels in IO.lift(() => Conversions.Rows(rhinoObject.GetTextureChannels()))
         from held in channels.TraverseM(channel => Products.Made(rhinoObject.GetTextureMapping(channel, out Transform objectTransform))
             .Map(mapping => new MappingEntry(channel, mapping, objectTransform))).As()
         from value in body(held)
         select value).Bracket();

    public static IO<Unit> Assign(RhinoObject rhinoObject, int channel, Option<TextureMapping> mapping, Option<Transform> objectTransform = default) =>
        from accepted in IO.lift(() => mapping.Traverse(held => RhinoFault.Unless((held.MappingType is TextureMappingType.OcsMapping) == (channel == ObjectAttributes.OCSMappingChannelId))).As())
        let held = mapping.ValueUnsafe()
        from assigned in IO.lift(() => objectTransform.Match(
            Some: xform => RhinoFault.Unless(rhinoObject.SetTextureMapping(channel, held, xform) is not 0),
            None: () => RhinoFault.Unless(rhinoObject.SetTextureMapping(channel, held) is not 0)))
        select assigned;

    // --- [COORDINATES]
    public static IO<Option<Either<Seq<Point2d>, Seq<Point3d>>>> Coordinates(Mesh mesh, Guid mappingId) =>
        IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(mappingId)).Traverse(Copied).As());

    public static IO<Option<Either<Seq<Point2d>, Seq<Point3d>>>> Coordinates(Mesh mesh, RhinoObject rhinoObject, Material material, Texture texture) =>
        from cached in IO.lift(() => mesh.SetCachedTextureCoordinatesFromMaterial(rhinoObject, material))
        from coordinates in IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(rhinoObject, texture)).Traverse(Copied).As())
        select coordinates;

    public static IO<bool> Matches(Mesh mesh, MappingTag tag) => IO.lift(() => tag.CompareTo(mesh.VertexColors.Tag) is 0);

    private static Fin<Either<Seq<Point2d>, Seq<Point3d>>> Copied(CachedTextureCoordinates held) =>
        RhinoFault.Unless(held.Dim is 2 or 3).Map(_ => Conversions.Rows(held) switch {
            var points when held.Dim is 2 => (Either<Seq<Point2d>, Seq<Point3d>>)new Either<Seq<Point2d>, Seq<Point3d>>.Left(points.Map(static point => new Point2d(point.X, point.Y))),
            var points => new Either<Seq<Point2d>, Seq<Point3d>>.Right(points),
        });
}
