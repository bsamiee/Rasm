using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MappingPrimitive {
    public sealed record SurfaceParameter() : MappingPrimitive;

    public sealed record Planar(Plane Plane, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingPrimitive;

    public sealed record Ocs(Plane Plane) : MappingPrimitive;

    public sealed record Cylindrical(Cylinder Cylinder, bool Capped) : MappingPrimitive;

    public sealed record Spherical(Sphere Sphere) : MappingPrimitive;

    public sealed record Box(Plane Plane, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingPrimitive;

    public sealed record CustomMesh(Mesh Mesh) : MappingPrimitive;
}

public readonly record struct MappingSettings(TextureSpace Space, Projection Projection, Transform Uvw);

public sealed record MappingState(
    TextureMappingType Kind,
    Guid Id,
    MappingSettings Settings,
    Transform PrimitiveTransform,
    Transform NormalTransform,
    Option<MappingPrimitive> Primitive);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Mappings {
    // --- [MAPPINGS]
    public static IO<TValue> WithMapping<TValue>(MappingPrimitive primitive, Func<TextureMapping, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Created(primitive)), body);

    public static IO<Unit> ApplySettings(TextureMapping mapping, MappingSettings settings) =>
        IO.lift(() => {
            mapping.TextureSpace = settings.Space;
            mapping.Projection = settings.Projection;
            mapping.UvwTransform = settings.Uvw;
        });

    public static IO<TValue> WithSnapshot<TValue>(TextureMapping mapping, Func<MappingState, IO<TValue>> body) =>
        (from primitive in PrimitiveOf(mapping)
         from state in IO.lift(() => new MappingState(
             mapping.MappingType,
             mapping.Id,
             MappingMapper.ToSettings(mapping),
             mapping.PrimitiveTransform,
             mapping.NormalTransform,
             primitive))
         from value in body(state)
         select value)
        .Bracket();

    public static IO<(int Side, Point3d Point)> Evaluate(TextureMapping mapping, Point3d point, Vector3d normal, Option<(Transform Points, Transform Normals)> xform) =>
        IO.lift(() => xform.Match(
            Some: moved => Sided(mapping.Evaluate(point, normal, out Point3d t, moved.Points, moved.Normals), t),
            None: () => Sided(mapping.Evaluate(point, normal, out Point3d t), t)));

    public static IO<(Vector3d Position, Vector3d Scale, Vector3d Rotation, Vector3d UvwOffset, Vector3d UvwRepeat, Vector3d UvwRotation)> Decompose(TextureMapping mapping, Transform local) =>
        IO.lift(() => {
            mapping.Decompose(local, out Vector3d position, out Vector3d scale, out Vector3d rotation, out Vector3d offset, out Vector3d repeat, out Vector3d uvwRotation);
            return Refused.Unless(
                Seq(position, scale, rotation, offset, repeat, uvwRotation).ForAll(static vector => vector.IsValid),
                (position, scale, rotation, offset, repeat, uvwRotation),
                nameof(TextureMapping.Decompose));
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

    private static IO<Option<MappingPrimitive>> PrimitiveOf(TextureMapping mapping) =>
        mapping.MappingType switch {
            TextureMappingType.None => IO.pure(Option<MappingPrimitive>.None),
            TextureMappingType.SurfaceParameters => IO.pure(Some<MappingPrimitive>(new MappingPrimitive.SurfaceParameter())),
            TextureMappingType.PlaneMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingPlane(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Planar(plane, dx, dy, dz, capped)), nameof(TextureMapping.TryGetMappingPlane))),
            TextureMappingType.OcsMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingPlane(out Plane plane, out _, out _, out _), Some<MappingPrimitive>(new MappingPrimitive.Ocs(plane)), nameof(TextureMapping.TryGetMappingPlane))),
            TextureMappingType.CylinderMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingCylinder(out Cylinder cylinder, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Cylindrical(cylinder, capped)), nameof(TextureMapping.TryGetMappingCylinder))),
            TextureMappingType.SphereMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingSphere(out Sphere sphere), Some<MappingPrimitive>(new MappingPrimitive.Spherical(sphere)), nameof(TextureMapping.TryGetMappingSphere))),
            TextureMappingType.BoxMapping => IO.lift(() => Refused.Unless(mapping.TryGetMappingBox(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool capped), Some<MappingPrimitive>(new MappingPrimitive.Box(plane, dx, dy, dz, capped)), nameof(TextureMapping.TryGetMappingBox))),
            TextureMappingType.MeshMappingPrimitive =>
                from copied in IO.lift(() => Refused.Unless(mapping.TryGetMappingMesh(out Mesh mesh), mesh, nameof(TextureMapping.TryGetMappingMesh)))
                from mesh in use(() => copied)
                select Some<MappingPrimitive>(new MappingPrimitive.CustomMesh(mesh)),
            TextureMappingType.SurfaceMappingPrimitive or TextureMappingType.BrepMappingPrimitive or TextureMappingType.FalseColors => IO.pure(Option<MappingPrimitive>.None),
        };

    private static Fin<(int Side, Point3d Point)> Sided(int code, Point3d point) =>
        Refused.Unless(code != 0, (code, point), nameof(TextureMapping.Evaluate));

    // --- [COORDINATES]
    public static IO<Option<(int Dim, Guid MappingId, Seq<Point3d> Coordinates)>> ReadCoordinates(Mesh mesh, Guid mappingId) =>
        from id in IO.lift(() => Answers.NonEmpty(mappingId, nameof(Mesh.GetCachedTextureCoordinates)))
        from cached in IO.lift(() => Optional(mesh.GetCachedTextureCoordinates(id)))
        from block in cached.Traverse(coordinates =>
            Disposal.Using(() => coordinates, held =>
                from rows in IO.lift(() =>
                    from same in Mismatch.Unless(held.Count == mesh.Vertices.Count, nameof(Mesh.Vertices))
                    select (held.Dim, held.MappingId, Coordinates: toSeq(held).Strict()))
                select rows)).As()
        select block;

    public static IO<Unit> CacheCoordinates(Mesh mesh, RhinoObject o, Material material) =>
        IO.lift(() => {
            mesh.SetCachedTextureCoordinatesFromMaterial(o, material);
            return Refused.Unless(mesh.HasCachedTextureCoordinates, nameof(Mesh.SetCachedTextureCoordinatesFromMaterial));
        });
}

[Mapper]
internal static partial class MappingMapper {
    [MapProperty(nameof(TextureMapping.TextureSpace), nameof(MappingSettings.Space))]
    [MapProperty(nameof(TextureMapping.UvwTransform), nameof(MappingSettings.Uvw))]
    internal static partial MappingSettings ToSettings(TextureMapping mapping);
}
