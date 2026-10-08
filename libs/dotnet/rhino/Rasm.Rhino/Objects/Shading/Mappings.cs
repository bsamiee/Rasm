using Rasm.Rhino.Document.Shapes;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Shading;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
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

[Union]
public abstract partial record TextureCoordinates {
    public sealed record Uv(Seq<Point2d> Points) : TextureCoordinates;

    public sealed record Uvw(Seq<Point3d> Points) : TextureCoordinates;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MappingMapper {
    internal static partial MappingSettings ToState(TextureMapping mapping);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(MappingSettings state, TextureMapping mapping);
}

public static class Mappings {
    // --- [MAPPINGS]
    public static IO<TValue> WithMapping<TValue>(MappingPrimitive primitive, Func<TextureMapping, IO<TValue>> body) =>
        use(IO.lift(() => Created(primitive))).Bind(body).Bracket();

    public static IO<Unit> ApplySettings(TextureMapping mapping, MappingSettings settings) =>
        IO.lift(() => MappingMapper.Update(settings, mapping));

    public static IO<TValue> WithSnapshot<TValue>(TextureMapping mapping, Func<MappingState, IO<TValue>> body) =>
        (from kind in IO.lift(() => mapping.MappingType)
         from primitive in Recovered(mapping, kind)
         from state in IO.lift(() => new MappingState(Conversions.Present(mapping.Id), primitive.ToEither(kind), MappingMapper.ToState(mapping)))
         from value in body(state)
         select value)
        .Bracket();

    public static IO<Seq<(Point3d Texture, int Side)>> Evaluate(TextureMapping mapping, Seq<(Point3d Point, Vector3d Normal)> samples, Transform objectTransform) =>
        IO.lift(() => Callbacks.Each(samples, Sampler(mapping, MappingXforms(objectTransform))));

    public static IO<MappingDecomposition> Decompose(TextureMapping mapping, Transform localTransform) =>
        IO.lift(() => {
            mapping.Decompose(localTransform, out Vector3d position, out Vector3d scale, out Vector3d rotation, out Vector3d offset, out Vector3d repeat, out Vector3d uvwRotation);
            return (Conversions.Present(position), Conversions.Present(scale), Conversions.Present(rotation), Conversions.Present(offset), Conversions.Present(repeat), Conversions.Present(uvwRotation))
                .Apply(static (p, s, r, o, u, w) => new MappingDecomposition(p, s, r, o, u, w))
                .As()
                .ToFin(new Refused(nameof(TextureMapping.Decompose)));
        });

    private static Fin<TextureMapping> Created(MappingPrimitive primitive) =>
        primitive.Switch(
            surfaceParameter: static _ => Missing.Unless(TextureMapping.CreateSurfaceParameterMapping(), nameof(TextureMapping.CreateSurfaceParameterMapping)),
            planar: static planar => Missing.Unless(TextureMapping.CreatePlaneMapping(planar.Plane, planar.Dx, planar.Dy, planar.Dz, planar.Capped), nameof(TextureMapping.CreatePlaneMapping)),
            ocs: static ocs => Missing.Unless(TextureMapping.CreateOcsMapping(ocs.Plane), nameof(TextureMapping.CreateOcsMapping)),
            cylindrical: static cylindrical => Missing.Unless(TextureMapping.CreateCylinderMapping(cylindrical.Cylinder, cylindrical.Capped), nameof(TextureMapping.CreateCylinderMapping)),
            spherical: static spherical => Missing.Unless(TextureMapping.CreateSphereMapping(spherical.Sphere), nameof(TextureMapping.CreateSphereMapping)),
            box: static box => Missing.Unless(TextureMapping.CreateBoxMapping(box.Plane, box.Dx, box.Dy, box.Dz, box.Capped), nameof(TextureMapping.CreateBoxMapping)),
            customMesh: static custom => Missing.Unless(TextureMapping.CreateCustomMeshMapping(custom.Mesh), nameof(TextureMapping.CreateCustomMeshMapping)));

    private static IO<Option<MappingPrimitive>> Recovered(TextureMapping mapping, TextureMappingType kind) =>
        kind switch {
            TextureMappingType.SurfaceParameters => IO.pure(Some<MappingPrimitive>(new MappingPrimitive.SurfaceParameter())),
            TextureMappingType.PlaneMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingPlane(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Planar(plane, dx, dy, dz, capped)), nameof(TextureMapping.TryGetMappingPlane))),
            TextureMappingType.OcsMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingPlane(out Plane plane, out _, out _, out _), Some<MappingPrimitive>(new MappingPrimitive.Ocs(plane)), nameof(TextureMapping.TryGetMappingPlane))),
            TextureMappingType.CylinderMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingCylinder(out Cylinder cylinder, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Cylindrical(cylinder, capped)), nameof(TextureMapping.TryGetMappingCylinder))),
            TextureMappingType.SphereMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingSphere(out Sphere sphere), Some<MappingPrimitive>(new MappingPrimitive.Spherical(sphere)), nameof(TextureMapping.TryGetMappingSphere))),
            TextureMappingType.BoxMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingBox(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Box(plane, dx, dy, dz, capped)), nameof(TextureMapping.TryGetMappingBox))),
            TextureMappingType.MeshMappingPrimitive =>
                from mesh in use(Copies.Acquire(() => Refused.Unless(mapping.TryGetMappingMesh(out Mesh copy), copy, nameof(TextureMapping.TryGetMappingMesh)), nameof(TextureMapping.TryGetMappingMesh)))
                select Some<MappingPrimitive>(new MappingPrimitive.CustomMesh(mesh)),
            TextureMappingType.None or TextureMappingType.SurfaceMappingPrimitive or TextureMappingType.BrepMappingPrimitive or TextureMappingType.FalseColors =>
                IO.pure(Option<MappingPrimitive>.None),
        };

    private static Func<(Point3d Point, Vector3d Normal), int, Fin<(Point3d Texture, int Side)>> Sampler(TextureMapping mapping, Option<(Transform Points, Transform Normals)> moved) =>
        (sample, index) => moved.Match(
                Some: xforms => (Side: mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture, xforms.Points, xforms.Normals), Texture: texture),
                None: () => (Side: mapping.Evaluate(sample.Point, sample.Normal, out Point3d texture), Texture: texture)) switch {
                    var (side, texture) => RefusedElement.Unless(side != 0, (texture, side), nameof(TextureMapping.Evaluate), index),
                };

    private static Option<(Transform Points, Transform Normals)> MappingXforms(Transform objectTransform) =>
        !objectTransform.IsIdentity && objectTransform.TryGetInverse(out Transform inverse) ? Some((inverse, objectTransform.Transpose())) : None;

    // --- [CHANNELS]
    public static IO<Option<TValue>> WithChannel<TValue>(RhinoObject rhinoObject, int channel, Func<MappingEntry, IO<TValue>> body) =>
        Entry(rhinoObject, channel).Bracket(Use: found => found.Traverse(body).As(), Fin: static found => Released(found.ToSeq()));

    public static IO<TValue> WithChannels<TValue>(RhinoObject rhinoObject, Func<Seq<MappingEntry>, IO<TValue>> body) =>
        IO.lift(() => toSeq(rhinoObject.GetTextureChannels()))
            .Bind(channels => DisposalOps.AcquireAll(channels.Map(channel => Entry(rhinoObject, channel).Bind(static found => IO.lift(found.ToFin(new Missing(nameof(RhinoObject.GetTextureMapping)))))), Released)
                .Bracket(Use: body, Fin: Released));

    public static IO<Unit> Assign(RhinoObject rhinoObject, int channel, TextureMapping mapping, Option<Transform> objectTransform) =>
        IO.lift(() => OcsChannelMismatch.Unless(mapping.MappingType, channel).Bind(_ => Refused.Unless(
            objectTransform.Match(Some: xform => rhinoObject.SetTextureMapping(channel, mapping, xform), None: () => rhinoObject.SetTextureMapping(channel, mapping)) != 0,
            nameof(RhinoObject.SetTextureMapping))));

    public static IO<Unit> Clear(RhinoObject rhinoObject, int channel) =>
        IO.lift(() => Refused.Unless(rhinoObject.SetTextureMapping(channel, tm: null) != 0, nameof(RhinoObject.SetTextureMapping)));

    private static IO<Option<MappingEntry>> Entry(RhinoObject rhinoObject, int channel) =>
        IO.lift(() => Optional(rhinoObject.GetTextureMapping(channel, out Transform objectTransform)).Map(mapping => new MappingEntry(channel, mapping, objectTransform)));

    private static IO<Unit> Released(Seq<MappingEntry> held) =>
        DisposalOps.Release(held.Map(static entry => entry.Mapping));

    // --- [COORDINATES]
    public static IO<Option<TextureCoordinates>> Coordinates(Mesh mesh, Guid mappingId) =>
        IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(mappingId)).Traverse(Copied).As());

    public static IO<Option<TextureCoordinates>> Coordinates(Mesh mesh, RhinoObject rhinoObject, Material material, Texture texture) =>
        IO.lift(() => {
            mesh.SetCachedTextureCoordinatesFromMaterial(rhinoObject, material);
            return Optional(mesh.GetCachedTextureCoordinates(rhinoObject, texture)).Traverse(Copied).As();
        });

    public static IO<bool> Matches(Mesh mesh, MappingTag tag) =>
        IO.lift(() => tag.CompareTo(mesh.VertexColors.Tag) == 0);

    private static Fin<TextureCoordinates> Copied(CachedTextureCoordinates held) =>
        held.Dim switch {
            2 => new TextureCoordinates.Uv(toSeq(held).Map(static point => new Point2d(point.X, point.Y))),
            3 => new TextureCoordinates.Uvw(toSeq(held)),
            _ => new InvalidAnswer(nameof(CachedTextureCoordinates.Dim)),
        };
}
