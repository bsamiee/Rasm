using Rasm.Rhino.Document.Shapes;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Shading;

// --- [MODELS] ---------------------------------------------------------------------------
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

// --- [OPERATIONS] -----------------------------------------------------------------------
[Mapper]
public static partial class Mappings {
    // --- [MAPPINGS]
    public static IO<TValue> WithMapping<TValue>(MappingPrimitive primitive, Func<TextureMapping, IO<TValue>> body) =>
        use(IO.lift(() => primitive.Switch(
            surfaceParameter: static _ => Missing.Unless(TextureMapping.CreateSurfaceParameterMapping(), nameof(TextureMapping.CreateSurfaceParameterMapping)),
            planar: static planar => Missing.Unless(TextureMapping.CreatePlaneMapping(planar.Plane, planar.Dx, planar.Dy, planar.Dz, planar.Capped), nameof(TextureMapping.CreatePlaneMapping)),
            ocs: static ocs => Missing.Unless(TextureMapping.CreateOcsMapping(ocs.Plane), nameof(TextureMapping.CreateOcsMapping)),
            cylindrical: static cylindrical => Missing.Unless(TextureMapping.CreateCylinderMapping(cylindrical.Cylinder, cylindrical.Capped), nameof(TextureMapping.CreateCylinderMapping)),
            spherical: static spherical => Missing.Unless(TextureMapping.CreateSphereMapping(spherical.Sphere), nameof(TextureMapping.CreateSphereMapping)),
            box: static box => Missing.Unless(TextureMapping.CreateBoxMapping(box.Plane, box.Dx, box.Dy, box.Dz, box.Capped), nameof(TextureMapping.CreateBoxMapping)),
            customMesh: static custom => Missing.Unless(TextureMapping.CreateCustomMeshMapping(custom.Mesh), nameof(TextureMapping.CreateCustomMeshMapping)))))
        .Bind(body).Bracket();

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial void ApplySettings([MappingTarget] TextureMapping mapping, MappingSettings settings);

    public static IO<TValue> WithSnapshot<TValue>(TextureMapping mapping, Func<MappingState, IO<TValue>> body) =>
        (from kind in IO.lift(() => mapping.MappingType)
         from primitive in kind switch {
             TextureMappingType.SurfaceParameters => IO.pure(Some<MappingPrimitive>(new MappingPrimitive.SurfaceParameter())),
             TextureMappingType.PlaneMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingPlane(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Planar(plane, dx, dy, dz, capped)), nameof(TextureMapping.TryGetMappingPlane))),
             TextureMappingType.OcsMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingPlane(out Plane plane, out _, out _, out _), Some<MappingPrimitive>(new MappingPrimitive.Ocs(plane)), nameof(TextureMapping.TryGetMappingPlane))),
             TextureMappingType.CylinderMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingCylinder(out Cylinder cylinder, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Cylindrical(cylinder, capped)), nameof(TextureMapping.TryGetMappingCylinder))),
             TextureMappingType.SphereMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingSphere(out Sphere sphere), Some<MappingPrimitive>(new MappingPrimitive.Spherical(sphere)), nameof(TextureMapping.TryGetMappingSphere))),
             TextureMappingType.BoxMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingBox(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Box(plane, dx, dy, dz, capped)), nameof(TextureMapping.TryGetMappingBox))),
             TextureMappingType.MeshMappingPrimitive =>
                 from mesh in use(Copies.Acquire(() => Refused.Unless(mapping.TryGetMappingMesh(out Mesh copy), copy, nameof(TextureMapping.TryGetMappingMesh)), nameof(TextureMapping.TryGetMappingMesh)))
                 select Some<MappingPrimitive>(new MappingPrimitive.CustomMesh(mesh)),
             TextureMappingType.None or TextureMappingType.SurfaceMappingPrimitive or TextureMappingType.BrepMappingPrimitive or TextureMappingType.FalseColors => IO.pure(Option<MappingPrimitive>.None),
         }
         from value in body(new MappingState(Conversions.Present(mapping.Id), primitive.ToEither(kind), ToState(mapping)))
         select value).Bracket();

    public static IO<Seq<(Point3d Texture, int Side)>> Evaluate(TextureMapping mapping, Seq<(Point3d Point, Vector3d Normal)> samples, Transform objectTransform) =>
        from moved in IO.lift(() => !objectTransform.IsIdentity && objectTransform.TryGetInverse(out Transform inverse) ? Some((Points: inverse, Normals: objectTransform.Transpose())) : None)
        from result in IO.lift(Callbacks.Each(samples, (sample, index) => moved.Match(
            Some: xforms => (Side: mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture, xforms.Points, xforms.Normals), Texture: texture),
            None: () => (Side: mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture), Texture: texture)) switch {
                var (side, texture) => RefusedElement.Unless(side != 0, (texture, side), nameof(TextureMapping.Evaluate), index),
            }))
        select result;

    public static IO<MappingDecomposition> Decompose(TextureMapping mapping, Transform localTransform) =>
        IO.lift(() => {
            mapping.Decompose(localTransform, out Vector3d position, out Vector3d scale, out Vector3d rotation, out Vector3d offset, out Vector3d repeat, out Vector3d uvwRotation);
            return (Conversions.Present(position), Conversions.Present(scale), Conversions.Present(rotation), Conversions.Present(offset), Conversions.Present(repeat), Conversions.Present(uvwRotation))
                .Apply(static (p, s, r, o, u, w) => new MappingDecomposition(p, s, r, o, u, w)).As().ToFin(new Refused(nameof(TextureMapping.Decompose)));
        });

    private static partial MappingSettings ToState(TextureMapping mapping);

    // --- [CHANNELS]
    public static IO<Option<TValue>> WithChannel<TValue>(RhinoObject rhinoObject, int channel, Func<MappingEntry, IO<TValue>> body) =>
        (from found in use(Entry(rhinoObject, channel), static found => Released(found.ToSeq()))
         from value in found.Traverse(body).As()
         select value).Bracket();

    public static IO<TValue> WithChannels<TValue>(RhinoObject rhinoObject, Func<Seq<MappingEntry>, IO<TValue>> body) =>
        (from channels in IO.lift(() => Conversions.Rows(rhinoObject.GetTextureChannels()))
         from held in use(DisposalOps.AcquireAll(channels.Map(channel => Entry(rhinoObject, channel).Bind(static found => IO.lift(found.ToFin(new Missing(nameof(RhinoObject.GetTextureMapping)))))), Released), Released)
         from value in body(held)
         select value).Bracket();

    public static IO<Unit> Assign(RhinoObject rhinoObject, int channel, Option<TextureMapping> mapping, Option<Transform> objectTransform = default) =>
        from accepted in IO.lift(() => mapping.Traverse(held => OcsChannelMismatch.Unless(held.MappingType, channel)).As())
        let held = mapping.ValueUnsafe()
        from assigned in IO.lift(() => Refused.Unless(objectTransform.Match(
            Some: xform => rhinoObject.SetTextureMapping(channel, held, xform),
            None: () => rhinoObject.SetTextureMapping(channel, held)) != 0, nameof(RhinoObject.SetTextureMapping)))
        select assigned;

    private static IO<Option<MappingEntry>> Entry(RhinoObject rhinoObject, int channel) =>
        IO.lift(() => Optional(rhinoObject.GetTextureMapping(channel, out Transform objectTransform)).Map(mapping => new MappingEntry(channel, mapping, objectTransform)));

    private static IO<Unit> Released(Seq<MappingEntry> held) => DisposalOps.Release(held.Map(static entry => entry.Mapping));

    // --- [COORDINATES]
    public static IO<Option<Either<Seq<Point2d>, Seq<Point3d>>>> Coordinates(Mesh mesh, Guid mappingId) =>
        IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(mappingId)).Traverse(Copied).As());

    public static IO<Option<Either<Seq<Point2d>, Seq<Point3d>>>> Coordinates(Mesh mesh, RhinoObject rhinoObject, Material material, Texture texture) =>
        from cached in IO.lift(() => mesh.SetCachedTextureCoordinatesFromMaterial(rhinoObject, material))
        from coordinates in IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(rhinoObject, texture)).Traverse(Copied).As())
        select coordinates;

    public static IO<bool> Matches(Mesh mesh, MappingTag tag) => IO.lift(() => tag.CompareTo(mesh.VertexColors.Tag) == 0);

    private static Fin<Either<Seq<Point2d>, Seq<Point3d>>> Copied(CachedTextureCoordinates held) => (held.Dim, Conversions.Rows(held)) switch {
        (2, var points) => new Either<Seq<Point2d>, Seq<Point3d>>.Left(points.Map(static point => new Point2d(point.X, point.Y))),
        (3, var points) => new Either<Seq<Point2d>, Seq<Point3d>>.Right(points),
        _ => new InvalidAnswer(nameof(CachedTextureCoordinates.Dim)),
    };
}
