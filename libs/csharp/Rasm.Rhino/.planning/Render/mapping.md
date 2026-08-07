# [RASM_RHINO_RENDER_MAPPING]

`MappingSpec` owns texture-mapping construction and recoverable primitive evidence, while `Mappings.Run` owns every document-bound channel operation through one request family. Host classifications close at admission, capped primitive cases carry their own construction policy, and every native mapping, mesh, or coordinate cache remains inside its boundary scope.

## [01]-[INDEX]

- [02]-[VOCABULARY]: admitted mapping classifications and policy rows.
- [03]-[SPEC_AND_STATE]: construction, inverse recovery, profile, snapshot, tag, evaluation, decomposition, and coordinate-cache owners.
- [04]-[CHANNEL_RAIL]: one request/result family over document-bound channel mutation and inspection.
- [05]-[SURFACE_LEDGER]: page-owned surfaces and growth rules.

## [02]-[VOCABULARY]

- Owner: `MappingKind` closes every `TextureMappingType` value and carries primitive recovery behavior over `MappingRecovery`; `SideCode` closes the host's evaluated side ordinals; `MappingSpace`, `MappingProjection`, `MappingCap`, and `CoordinateInvalidation` close the host policy axes.
- Law: host enums exist only inside each correspondence owner; detached state carries generated rows, native lookup derives from `Items`, and an uncatalogued host ordinal rejects.
- Law: `MappingRecovery` splits the inverse into a value channel and a native-mesh channel because the host does — the mesh inverse is the only one answering a native, so nothing else pays a lease and the detached snapshot carries values alone.
- Law: the surface-primitive, brep-primitive, and false-colors kinds share the one `Bare` behavior pair — the host publishes no inverse accessor and no factory for them, so their spec is structurally absent; each keeps its own row because the native discriminant round-trips through `MappingSnapshot.Kind` and `ChannelTag.Native`.
- Law: a magic evaluation ordinal never appears inline — `SideCode` rows carry the host's documented side codes per mapping type, and a type owning no rows answers `General` on any positive code.
- Law: `MappingCap` is the sole cap authority on capped `MappingSpec` cases; mint and recovery consume the same payload, so construction and inverse evidence cannot disagree.
- Growth: a host classification adds one row with its recovery behavior; a policy value adds one row with its host projection; a new evaluated side is one `SideCode` row.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class MappingCap {
    public static readonly MappingCap Open = new(key: false);
    public static readonly MappingCap Closed = new(key: true);

    internal bool Native => Key;

    internal static MappingCap Of(bool native) => native ? Closed : Open;
}

[SmartEnum<string>]
public sealed partial class MappingSpace {
    public static readonly MappingSpace Single = new("single", TextureSpace.Single);
    public static readonly MappingSpace Divided = new("divided", TextureSpace.Divided);

    internal TextureSpace Native { get; }

    internal static Fin<MappingSpace> Of(TextureSpace native, Op key) => key.Row(Items, native, static item => item.Native);
}

[SmartEnum<string>]
public sealed partial class MappingProjection {
    public static readonly MappingProjection None = new("none", Projection.None);
    public static readonly MappingProjection Closest = new("closest-point", Projection.ClosestPoint);
    public static readonly MappingProjection Ray = new("ray", Projection.Ray);

    internal Projection Native { get; }

    internal static Fin<MappingProjection> Of(Projection native, Op key) => key.Row(Items, native, static item => item.Native);
}

[SmartEnum<bool>]
public sealed partial class CoordinateInvalidation {
    public static readonly CoordinateInvalidation All = new(key: false);
    public static readonly CoordinateInvalidation SurfaceParameters = new(key: true);

    internal bool Native => Key;
}

// Recovery answers on two channels because the host does: every inverse but the mesh one yields pure values, and
// `TryGetMappingMesh` yields a NATIVE mesh. Keeping them one field forced a lease into the detached snapshot; splitting
// them lets the snapshot carry values alone while the mesh rides its own `Lease<Mesh>` out of the read window.
public readonly record struct MappingRecovery(Option<MappingSpec> Spec, Option<Lease<Mesh>> Coordinates) {
    public static MappingRecovery Bare { get; } = new(Spec: None, Coordinates: None);

    internal static MappingRecovery Of(MappingSpec spec) => new(Spec: Some(spec), Coordinates: None);
}

[SmartEnum<string>]
public sealed partial class MappingKind {
    public static readonly MappingKind None = new(
        "none", TextureMappingType.None, Specless, Bare);
    public static readonly MappingKind SurfaceParameter = new(
        "surface-parameter", TextureMappingType.SurfaceParameters,
        static recovered => recovered.Spec is { IsSome: true, Case: MappingSpec.SurfaceParameter }
            && recovered.Coordinates.IsNone,
        static (_, _) => Fin.Succ(MappingRecovery.Of(new MappingSpec.SurfaceParameter())));
    public static readonly MappingKind Plane = new(
        "plane", TextureMappingType.PlaneMapping,
        Values<MappingSpec.Planar>(), RecoverPlane);
    public static readonly MappingKind Cylinder = new(
        "cylinder", TextureMappingType.CylinderMapping,
        Values<MappingSpec.Cylindrical>(), RecoverCylinder);
    public static readonly MappingKind Sphere = new(
        "sphere", TextureMappingType.SphereMapping,
        Values<MappingSpec.Spherical>(), RecoverSphere);
    public static readonly MappingKind Box = new(
        "box", TextureMappingType.BoxMapping,
        Values<MappingSpec.Boxed>(), RecoverBox);
    public static readonly MappingKind Mesh = new(
        "mesh", TextureMappingType.MeshMappingPrimitive,
        static recovered => recovered.Spec.IsNone && recovered.Coordinates.IsSome, RecoverMesh);
    public static readonly MappingKind Ocs = new(
        "ocs", TextureMappingType.OcsMapping,
        Values<MappingSpec.Ocs>(), RecoverOcs);
    // Host truth: `TextureMapping` publishes `TryGetMappingPlane`/`Box`/`Sphere`/`Cylinder`/`Mesh` and NOTHING for the
    // surface-primitive, brep-primitive, or false-colors kinds — no inverse accessor and no `Create*` factory either. Their
    // spec is structurally absent rather than merely unread, so the three share the one `Bare` behavior pair. Each keeps its
    // own row because the native discriminant round-trips through `MappingSnapshot.Kind` and `ChannelTag.Native`.
    public static readonly MappingKind Surface = new(
        "surface", TextureMappingType.SurfaceMappingPrimitive, Specless, Bare);
    public static readonly MappingKind Brep = new(
        "brep", TextureMappingType.BrepMappingPrimitive, Specless, Bare);
    public static readonly MappingKind FalseColors = new(
        "false-colors", TextureMappingType.FalseColors, Specless, Bare);

    internal TextureMappingType Native { get; }

    [UseDelegateFromConstructor]
    internal partial bool Accepts(MappingRecovery recovered);

    [UseDelegateFromConstructor]
    internal partial Fin<MappingRecovery> Recover(TextureMapping mapping, Op key);

    internal static Fin<MappingKind> Of(TextureMappingType native, Op key) => key.Row(Items, native, static item => item.Native);

    private static Func<MappingRecovery, bool> Specless =>
        static recovered => recovered is { Spec.IsNone: true, Coordinates.IsNone: true };

    private static Fin<MappingRecovery> Bare(TextureMapping _, Op __) => Fin.Succ(MappingRecovery.Bare);

    private static Func<MappingRecovery, bool> Values<TCase>() where TCase : MappingSpec =>
        static recovered => recovered.Spec is { IsSome: true, Case: TCase } && recovered.Coordinates.IsNone;

    private static Fin<MappingRecovery> RecoverPlane(TextureMapping mapping, Op key) => key.Catch(() =>
        mapping.TryGetMappingPlane(out Plane frame, out Interval dx, out Interval dy, out Interval dz, out bool capped)
            ? MappingRecovery.Of(new MappingSpec.Planar(frame, dx, dy, dz, MappingCap.Of(capped)))
            : MappingRecovery.Bare);

    private static Fin<MappingRecovery> RecoverOcs(TextureMapping mapping, Op key) => key.Catch(() =>
        mapping.TryGetMappingPlane(out Plane frame, out Interval _, out Interval _, out Interval _)
            ? MappingRecovery.Of(new MappingSpec.Ocs(frame))
            : MappingRecovery.Bare);

    private static Fin<MappingRecovery> RecoverCylinder(TextureMapping mapping, Op key) => key.Catch(() =>
        mapping.TryGetMappingCylinder(out Cylinder body, out bool capped)
            ? MappingRecovery.Of(new MappingSpec.Cylindrical(body, MappingCap.Of(capped)))
            : MappingRecovery.Bare);

    private static Fin<MappingRecovery> RecoverSphere(TextureMapping mapping, Op key) => key.Catch(() =>
        mapping.TryGetMappingSphere(out Sphere body)
            ? MappingRecovery.Of(new MappingSpec.Spherical(body))
            : MappingRecovery.Bare);

    private static Fin<MappingRecovery> RecoverBox(TextureMapping mapping, Op key) => key.Catch(() =>
        mapping.TryGetMappingBox(out Plane frame, out Interval dx, out Interval dy, out Interval dz, out bool capped)
            ? MappingRecovery.Of(new MappingSpec.Boxed(frame, dx, dy, dz, MappingCap.Of(capped)))
            : MappingRecovery.Bare);

    // The losing branch still receives an out mesh, so the refusal path releases it before answering bare.
    private static Fin<MappingRecovery> RecoverMesh(TextureMapping mapping, Op key) => key.Catch(() =>
        mapping.TryGetMappingMesh(out Mesh mesh)
            ? Fin.Succ(new MappingRecovery(
                Spec: None, Coordinates: Some((Lease<Mesh>)new Lease<Mesh>.Owned(Value: mesh))))
            : Fin.Succ((Op.Side(() => mesh?.Dispose()), MappingRecovery.Bare).Item2));
}
```

## [03]-[SPEC_AND_STATE]

- Owner: `MappingSpec` closes the verified factory family and owns custom-mesh custody; `MappingProfile` owns texture space, projection, and UVW exactly once, while the primitive transform is minted from the spec frame and read back as snapshot evidence, so profile application never overwrites the constructed primitive and the same axis cannot carry two authorities.
- Owner: `MappingSnapshot` carries classification, identity, total profile, derived primitive and normal evidence, value-only inverse evidence, and object motion; absence of `Spec` records that the host mapping destroys or withholds inverse-sufficient construction data, or that the kind's inverse is the mesh channel. A mesh mapping's native coordinates exit beside the snapshot on their own `Lease<Mesh>` owned by `MappingResult.Snapshot`, and every refusal path releases both recovery channels before the fault leaves — custody transfers only on success.
- Owner: `MappingProbe`, `MappingSide`, `MappingFrame`, `ChannelTag`, and `CoordinateBlock` admit evaluation, side taxonomy, decomposition, tag, and coordinate-cache evidence without leaking provider classifications.
- Law: `MappingSpec.Mint` and `MappingKind.Recover` are the two directions of one correspondence — mint takes the lease-carrying spec, recovery answers the split `MappingRecovery`; unsupported inverse kinds retain their admitted kind and profile while both recovery channels stay absent.
- Law: `TextureCoordinates.Run` owns cache prime, read, presence, and invalidation modalities; invalidation scope is a policy row, never a boolean knob.
- Boundary: `MappingTag` crosses only through `ChannelTag.Of` and `ChannelTag.Native`; custom meshes transfer through `Lease<Mesh>`, and native property application, cache mutation, losing mesh recovery, plus coordinate-wrapper disposal are the platform-forced statement seams.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MappingSpec : IDisposable {
    private MappingSpec() { }
    public sealed record SurfaceParameter : MappingSpec;
    public sealed record Planar(Plane Frame, Interval Dx, Interval Dy, Interval Dz, MappingCap Cap) : MappingSpec;
    public sealed record Ocs(Plane Frame) : MappingSpec;
    public sealed record Cylindrical(Cylinder Body, MappingCap Cap) : MappingSpec;
    public sealed record Spherical(Sphere Body) : MappingSpec;
    public sealed record Boxed(Plane Frame, Interval Dx, Interval Dy, Interval Dz, MappingCap Cap) : MappingSpec;
    public sealed record MeshCustom(Lease<Mesh> Coordinates) : MappingSpec;

    internal Fin<Lease<TextureMapping>> Mint(Op key) =>
        Switch(
            context: key,
            surfaceParameter: static (op, _) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateSurfaceParameterMapping()).ToFin(Fail: op.InvalidResult()))),
            planar: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreatePlaneMapping(spec.Frame, spec.Dx, spec.Dy, spec.Dz, spec.Cap.Native))
                    .ToFin(Fail: op.InvalidResult()))),
            ocs: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateOcsMapping(spec.Frame)).ToFin(Fail: op.InvalidResult()))),
            cylindrical: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateCylinderMapping(spec.Body, spec.Cap.Native)).ToFin(Fail: op.InvalidResult()))),
            spherical: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateSphereMapping(spec.Body)).ToFin(Fail: op.InvalidResult()))),
            boxed: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateBoxMapping(spec.Frame, spec.Dx, spec.Dy, spec.Dz, spec.Cap.Native))
                    .ToFin(Fail: op.InvalidResult()))),
            meshCustom: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateCustomMeshMapping(spec.Coordinates.Resource)).ToFin(Fail: op.InvalidResult()))));

    public void Dispose() => Switch(
        surfaceParameter: static _ => unit,
        planar: static _ => unit,
        ocs: static _ => unit,
        cylindrical: static _ => unit,
        spherical: static _ => unit,
        boxed: static _ => unit,
        meshCustom: static spec => spec.Coordinates.Dispose());

    private static Fin<Lease<TextureMapping>> Owned(Fin<TextureMapping> mapping) =>
        mapping.Map(static value => (Lease<TextureMapping>)new Lease<TextureMapping>.Owned(Value: value));
}

[ComplexValueObject]
public sealed partial class MappingProfile {
    public MappingSpace Space { get; }
    public MappingProjection Projection { get; }
    public Transform Uvw { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MappingSpace space,
        ref MappingProjection projection,
        ref Transform uvw) {
        validationError = space is not null && projection is not null && uvw.IsValid
            ? validationError
            : new ValidationError(message: "mapping profile is invalid");
    }

    internal static Fin<MappingProfile> Of(TextureMapping mapping, Op key) =>
        from space in MappingSpace.Of(mapping.TextureSpace, key)
        from projection in MappingProjection.Of(mapping.Projection, key)
        from profile in key.AcceptValidated(Validate(space, projection, mapping.UvwTransform, out MappingProfile? value), value)
        select profile;

    internal Fin<Unit> Apply(TextureMapping mapping, Op key) {
        MappingProfile self = this;
        return key.Catch(() => {
            mapping.TextureSpace = self.Space.Native;
            mapping.Projection = self.Projection.Native;
            mapping.UvwTransform = self.Uvw;
            return Fin.Succ(unit);
        });
    }
}

// The detached result carries VALUES alone — no lease reaches a field, so nothing about it needs disposing and it survives
// the read window as an ordinary value. A mesh mapping's native coordinates leave beside it on their own `Lease<Mesh>`,
// which the result case that carries them owns.
[ComplexValueObject]
public sealed partial class MappingSnapshot : IDetachedDocumentResult {
    public MappingKind Kind { get; }
    public Guid Id { get; }
    public MappingProfile Profile { get; }
    public Transform Primitive { get; }
    public Transform Normal { get; }
    public Option<MappingSpec> Spec { get; }
    public Option<Transform> ObjectMotion { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MappingKind kind,
        ref Guid id,
        ref MappingProfile profile,
        ref Transform primitive,
        ref Transform normal,
        ref Option<MappingSpec> spec,
        ref Option<Transform> objectMotion) {
        validationError = kind is not null && id != Guid.Empty && profile is not null
            && primitive.IsValid && normal.IsValid
            && spec.ForAll(static held => held is not MappingSpec.MeshCustom)
            && objectMotion.Map(static motion => motion.IsValid).IfNone(true)
            ? validationError
            : new ValidationError(message: "mapping snapshot is invalid");
    }

    internal static Fin<(MappingSnapshot Value, Option<Lease<Mesh>> Coordinates)> Of(
        TextureMapping mapping, Option<Transform> motion, Op key) =>
        from kind in MappingKind.Of(mapping.MappingType, key)
        from profile in MappingProfile.Of(mapping, key)
        from recovered in kind.Recover(mapping, key)
        from _ in guard(kind.Accepts(recovered), key.InvalidResult())
            .ToFin()
            .MapFail(fault => Released(recovered, fault))
        from snapshot in key.AcceptValidated(
                Validate(kind, mapping.Id, profile, mapping.PrimitiveTransform, mapping.NormalTransform, recovered.Spec, motion, out MappingSnapshot? value),
                value)
            .MapFail(fault => Released(recovered, fault))
        select (Value: snapshot, Coordinates: recovered.Coordinates);

    // Custody transfers to the caller only on success; every refusal path releases both channels before the fault leaves.
    private static Error Released(MappingRecovery recovered, Error fault) {
        _ = recovered.Spec.Iter(static held => held.Dispose());
        _ = recovered.Coordinates.Iter(static lease => lease.Dispose());
        return fault;
    }
}

[ComplexValueObject]
public sealed partial class MappingProbe {
    public Point3d Point { get; }
    public Vector3d Normal { get; }
    public Option<(Transform Points, Transform Normals)> Motion { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Point3d point,
        ref Vector3d normal,
        ref Option<(Transform Points, Transform Normals)> motion) {
        validationError = point.IsValid && normal.IsValid
            && motion.Map(static pair => pair.Points.IsValid && pair.Normals.IsValid).IfNone(true)
            ? validationError
            : new ValidationError(message: "mapping probe is invalid");
    }

    internal Fin<MappingEvaluation> Evaluate(TextureMapping mapping, Op key) =>
        key.Catch(() => {
            (int side, Point3d mapped) = Motion switch {
                { IsSome: true, Case: (Transform points, Transform normals) } =>
                    (mapping.Evaluate(Point, Normal, out Point3d moved, points, normals), moved),
                _ => (mapping.Evaluate(Point, Normal, out Point3d direct), direct),
            };
            return from admittedSide in MappingSide.Of(mapping.MappingType, side, key)
                   from evaluation in MappingEvaluation.Of(admittedSide, mapped, key)
                   select evaluation;
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MappingSide {
    private MappingSide() { }
    public sealed record General(MappingKind Kind) : MappingSide;
    public sealed record CylinderWall : MappingSide;
    public sealed record CylinderBottom : MappingSide;
    public sealed record CylinderTop : MappingSide;
    public sealed record BoxFront : MappingSide;
    public sealed record BoxRight : MappingSide;
    public sealed record BoxBack : MappingSide;
    public sealed record BoxLeft : MappingSide;
    public sealed record BoxBottom : MappingSide;
    public sealed record BoxTop : MappingSide;

    internal static Fin<MappingSide> Of(TextureMappingType type, int side, Op key) =>
        toSeq(SideCode.Items).Find(row => row.Owner == type && row.Ordinal == side).Case switch {
            SideCode row => Fin.Succ(value: row.Side()),
            _ when type is TextureMappingType.None
                || side <= 0
                || toSeq(SideCode.Items).Exists(row => row.Owner == type) =>
                Fin.Fail<MappingSide>(key.InvalidResult(detail: $"{type}:{side}")),
            _ => MappingKind.Of(type, key).Map(static kind => (MappingSide)new General(kind)),
        };
}

// Host truth: `TextureMapping.Evaluate` answers nonzero on success, and for BOX and CAPPED-CYLINDER mappings ALONE that
// value names the side. These rows are the host's own documented ordinals; every other mapping type publishes no side
// vocabulary, so its positive code is bare success, and `TextureMappingType.None` evaluates to nothing at all.
[SmartEnum<string>]
public sealed partial class SideCode {
    public static readonly SideCode CylinderWall = new(
        "cylinder-wall", TextureMappingType.CylinderMapping, 1, static () => new MappingSide.CylinderWall());
    public static readonly SideCode CylinderBottom = new(
        "cylinder-bottom", TextureMappingType.CylinderMapping, 2, static () => new MappingSide.CylinderBottom());
    public static readonly SideCode CylinderTop = new(
        "cylinder-top", TextureMappingType.CylinderMapping, 3, static () => new MappingSide.CylinderTop());
    public static readonly SideCode BoxFront = new(
        "box-front", TextureMappingType.BoxMapping, 1, static () => new MappingSide.BoxFront());
    public static readonly SideCode BoxRight = new(
        "box-right", TextureMappingType.BoxMapping, 2, static () => new MappingSide.BoxRight());
    public static readonly SideCode BoxBack = new(
        "box-back", TextureMappingType.BoxMapping, 3, static () => new MappingSide.BoxBack());
    public static readonly SideCode BoxLeft = new(
        "box-left", TextureMappingType.BoxMapping, 4, static () => new MappingSide.BoxLeft());
    public static readonly SideCode BoxBottom = new(
        "box-bottom", TextureMappingType.BoxMapping, 5, static () => new MappingSide.BoxBottom());
    public static readonly SideCode BoxTop = new(
        "box-top", TextureMappingType.BoxMapping, 6, static () => new MappingSide.BoxTop());

    internal TextureMappingType Owner { get; }
    internal int Ordinal { get; }

    [UseDelegateFromConstructor]
    internal partial MappingSide Side();
}

[ComplexValueObject]
public sealed partial class MappingEvaluation : IDetachedDocumentResult {
    public MappingSide Side { get; }
    public Point3d Point { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MappingSide side,
        ref Point3d point) {
        validationError = side is not null && point.IsValid
            ? validationError
            : new ValidationError(message: "mapping evaluation is invalid");
    }

    internal static Fin<MappingEvaluation> Of(MappingSide side, Point3d point, Op key) =>
        key.AcceptValidated(Validate(side, point, out MappingEvaluation? value), value);
}

[ComplexValueObject]
public sealed partial class MappingFrame : IDetachedDocumentResult {
    public Vector3d Position { get; }
    public Vector3d Scale { get; }
    public Vector3d Rotation { get; }
    public Vector3d UvwOffset { get; }
    public Vector3d UvwRepeat { get; }
    public Vector3d UvwRotation { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Vector3d position,
        ref Vector3d scale,
        ref Vector3d rotation,
        ref Vector3d uvwOffset,
        ref Vector3d uvwRepeat,
        ref Vector3d uvwRotation) {
        validationError = position.IsValid && scale.IsValid && rotation.IsValid
            && uvwOffset.IsValid && uvwRepeat.IsValid && uvwRotation.IsValid
            ? validationError
            : new ValidationError(message: "mapping decomposition is invalid");
    }

    internal static Fin<MappingFrame> Of(TextureMapping mapping, Transform local, Op key) => key.Catch(() => {
        mapping.Decompose(local, out Vector3d position, out Vector3d scale, out Vector3d rotation,
            out Vector3d offset, out Vector3d repeat, out Vector3d spin);
        return key.AcceptValidated(Validate(position, scale, rotation, offset, repeat, spin, out MappingFrame? frame), frame);
    });
}

[ValueObject<int>]
public readonly partial struct MappingChannel {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        validationError = value <= 0 ? new ValidationError(message: "mapping channel is not positive") : validationError;
    }

    internal static Fin<MappingChannel> Of(int value, Op key) => key.AcceptValidated<MappingChannel>(value);
}

[ComplexValueObject]
public sealed partial class ChannelTag : IComparable<ChannelTag>, IDetachedDocumentResult {
    public Guid Id { get; }
    public MappingKind Kind { get; }
    public uint Crc { get; }
    public Transform MeshTransform { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid id,
        ref MappingKind kind,
        ref uint crc,
        ref Transform meshTransform) {
        validationError = id != Guid.Empty && kind is not null && meshTransform.IsValid
            ? validationError
            : new ValidationError(message: "mapping tag is invalid");
    }

    public static Fin<ChannelTag> Of(MappingTag tag, Op? key = null) {
        Op op = key.OrDefault();
        return from source in op.Need(tag)
               from kind in MappingKind.Of(source.MappingType, op)
               from value in op.AcceptValidated(Validate(source.Id, kind, source.MappingCRC, source.MeshTransform, out ChannelTag? admitted), admitted)
               select value;
    }

    public MappingTag Native() =>
        new() { Id = Id, MappingType = Kind.Native, MappingCRC = Crc, MeshTransform = MeshTransform };

    public int CompareTo(ChannelTag other) => Native().CompareTo(other.Native());
}

[ComplexValueObject]
public sealed partial class CoordinateBlock : IDetachedDocumentResult {
    public int Dim { get; }
    public Guid MappingId { get; }
    public int VertexCount { get; }
    public Arr<Point3d> Rows { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int dim,
        ref Guid mappingId,
        ref int vertexCount,
        ref Arr<Point3d> rows) {
        validationError = dim is 2 or 3 && mappingId != Guid.Empty && vertexCount >= 0
            && rows.Count == vertexCount && rows.ForAll(static point => point.IsValid)
            ? validationError
            : new ValidationError(message: "cached texture coordinates are invalid");
    }

    internal static Fin<CoordinateBlock> Of(CachedTextureCoordinates coordinates, int expected, Op key) {
        Arr<Point3d> rows = toArr(coordinates);
        return key.AcceptValidated(Validate(coordinates.Dim, coordinates.MappingId, expected, rows, out CoordinateBlock? value), value);
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateRequest {
    private CoordinateRequest() { }
    public sealed record Read(Guid MappingId) : CoordinateRequest;
    public sealed record Prime(RhinoObject Object, Material Material) : CoordinateRequest;
    public sealed record Invalidate(CoordinateInvalidation Scope) : CoordinateRequest;
    public sealed record Probe : CoordinateRequest;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateResult : IDetachedDocumentResult {
    private CoordinateResult() { }
    public sealed record Block(CoordinateBlock Value) : CoordinateResult;
    public sealed record Primed(bool Present) : CoordinateResult;
    public sealed record Invalidated(bool Present) : CoordinateResult;
    public sealed record Presence(bool Value) : CoordinateResult;
}

public static class TextureCoordinates {
    public static Fin<CoordinateResult> Run(Mesh mesh, CoordinateRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return from activeMesh in op.Need(mesh)
               from activeRequest in op.Need(request)
               from result in activeRequest.Switch(
                   context: (Mesh: activeMesh, Op: op),
                   read: static (context, query) => Read(context.Mesh, query.MappingId, context.Op),
                   prime: static (context, command) =>
                       from source in context.Op.Need(command.Object)
                       from material in context.Op.Need(command.Material)
                       from state in context.Op.Catch(() => {
                           context.Mesh.SetCachedTextureCoordinatesFromMaterial(source, material);
                           return Fin.Succ<CoordinateResult>(new CoordinateResult.Primed(context.Mesh.HasCachedTextureCoordinates));
                       })
                       select state,
                   invalidate: static (context, command) =>
                       from scope in context.Op.Need(command.Scope)
                       from state in context.Op.Catch(() => {
                           context.Mesh.InvalidateCachedTextureCoordinates(scope.Native);
                           return Fin.Succ<CoordinateResult>(new CoordinateResult.Invalidated(context.Mesh.HasCachedTextureCoordinates));
                       })
                       select state,
                   probe: static (context, _) => context.Op.Catch(() =>
                       Fin.Succ<CoordinateResult>(new CoordinateResult.Presence(context.Mesh.HasCachedTextureCoordinates))))
               select result;
    }

    private static Fin<CoordinateResult> Read(Mesh mesh, Guid mappingId, Op key) =>
        from _ in guard(mappingId != Guid.Empty, key.InvalidInput())
        from block in key.Catch(() => {
            using var coordinates = mesh.GetCachedTextureCoordinates(mappingId);
            return from active in Optional(coordinates).ToFin(Fail: key.MissingContext())
                   from value in CoordinateBlock.Of(active, mesh.Vertices.Count, key)
                   select value;
        })
        select (CoordinateResult)new CoordinateResult.Block(block);
}
```

## [04]-[CHANNEL_RAIL]

- Owner: `MappingRequest` stores bind, snapshot, evaluation, decomposition, or census modality; `MappingResult` keeps each answer case explicit; `Mappings.Run` is the sole document entry.
- Law: a request admits target, channel, profile, spec, transforms, and redraw policy before the demand window; the host document and native mapping never leave it.
- Law: bind resolves every object once, mints one mapping lease, applies one profile, records one undo bracket, restores redraw suppression on every exit, and appends `ContentSlot.Mapped` facts to `ContentReceipt`.
- Law: census composes `ObjectAttributes.HasMapping` as the cheap attribute gate and `RhinoObject.HasTextureMapping` as the texture-specific gate before reading channels.
- Law: channel-bearing requests reject a default-initialized `MappingChannel` at the request seam before any document grant opens — the generated owner raises on the uninitialized key read rather than answering zero, so the seam traps that raise onto the rail instead of letting it escape `Admit`.
- Law: the host reports no object motion as `Transform.Identity`, so a read carries the returned transform as `Some(motion)` and an invalid readback transform is malformed host data failing typed — never collapsed into `None`.
- Boundary: `MappingSpec.Ocs` binds only to `ObjectAttributes.OCSMappingChannelId`; unsupported inverse kinds remain visible through `MappingSnapshot.Kind` and absent through `MappingSnapshot.Spec`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record MappingCensus(Seq<(Guid Object, Seq<MappingChannel> Channels)> Rows) : IDetachedDocumentResult;

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MappingRequest {
    private MappingRequest() { }
    public sealed record Bind(
        TableTarget Objects,
        MappingChannel Channel,
        MappingProfile Profile,
        MappingSpec Spec,
        RedrawPolicy Redraw,
        Option<Transform> ObjectMotion = default) : MappingRequest;
    public sealed record Snapshot(TableTarget Object, MappingChannel Channel) : MappingRequest;
    public sealed record Evaluate(TableTarget Object, MappingChannel Channel, MappingProbe Probe) : MappingRequest;
    public sealed record Decompose(TableTarget Object, MappingChannel Channel, Transform Local) : MappingRequest;
    public sealed record Census(TableTarget Objects) : MappingRequest;

    internal Fin<MappingRequest> Admit(Op key) => Switch(
        context: key,
        bind: static (op, request) => Channel(request.Channel, request, op),
        snapshot: static (op, request) => Channel(request.Channel, request, op),
        evaluate: static (op, request) => Channel(request.Channel, request, op),
        decompose: static (op, request) => Channel(request.Channel, request, op),
        census: static (_, request) => Fin.Succ<MappingRequest>(request));

    // A `default`-initialized generated struct owner THROWS on its key read rather than answering zero, so the ghost a
    // public case constructor can still forge is trapped here and lands typed at the request seam, never at the host.
    private static Fin<MappingRequest> Channel(MappingChannel channel, MappingRequest request, Op key) =>
        key.Catch(() => MappingChannel.Of(channel.Value, key)).Map(_ => request);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MappingResult : IDetachedDocumentResult {
    private MappingResult() { }
    public sealed record Changed(ContentReceipt Receipt) : MappingResult;
    public sealed record Snapshot(MappingSnapshot Value, Option<Lease<Mesh>> Coordinates) : MappingResult, IDisposable {
        public void Dispose() => Coordinates.Iter(static lease => lease.Dispose());
    }
    public sealed record Evaluated(MappingEvaluation Value) : MappingResult;
    public sealed record Decomposed(MappingFrame Value) : MappingResult;
    public sealed record Census(MappingCensus Value) : MappingResult;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Mappings {
    public static Fin<MappingResult> Run(DocumentSession session, MappingRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return from activeSession in op.Need(session)
               from activeRequest in op.Need(request).Bind(command => command.Admit(op))
               from result in activeRequest.Switch(
                   context: (Session: activeSession, Op: op),
                   bind: static (state, command) => Bind(state.Session, command, state.Op)
                       .Map(static receipt => (MappingResult)new MappingResult.Changed(receipt)),
                   snapshot: static (state, query) => Read(state.Session, query.Object, query.Channel, state.Op, unit,
                       static (_, mapping, motion, key) => MappingSnapshot.Of(mapping, motion, key)
                           .Map(static recovered => (MappingResult)new MappingResult.Snapshot(
                               Value: recovered.Value, Coordinates: recovered.Coordinates))),
                   evaluate: static (state, query) =>
                       from probe in state.Op.Need(query.Probe)
                       from result in Read(state.Session, query.Object, query.Channel, state.Op, probe,
                           static (admitted, mapping, _, key) => admitted.Evaluate(mapping, key)
                               .Map(static value => (MappingResult)new MappingResult.Evaluated(value)))
                       select result,
                   decompose: static (state, query) =>
                       from _ in guard(query.Local.IsValid, state.Op.InvalidInput()).ToFin()
                       from result in Read(state.Session, query.Object, query.Channel, state.Op, query.Local,
                           static (local, mapping, _, key) => MappingFrame.Of(mapping, local, key)
                               .Map(static value => (MappingResult)new MappingResult.Decomposed(value)))
                       select result,
                   census: static (state, query) => Census(state.Session, query.Objects, state.Op)
                       .Map(static value => (MappingResult)new MappingResult.Census(value)))
               select result;
    }

    private static Fin<ContentReceipt> Bind(DocumentSession session, MappingRequest.Bind command, Op op) =>
        from objects in op.Need(command.Objects)
        from profile in op.Need(command.Profile)
        from spec in op.Need(command.Spec)
        from redraw in op.Need(command.Redraw)
        from _ in guard(
            (spec is not MappingSpec.Ocs || command.Channel.Value == ObjectAttributes.OCSMappingChannelId)
            && command.ObjectMotion.Map(static motion => motion.IsValid).IfNone(true),
            op.InvalidInput())
        from receipt in session.Demand(
            use: document => DocumentCommit.Sealed(
                document: document,
                name: nameof(Mappings),
                recordsUndo: true,
                redraw: redraw,
                run: () =>
                    from ids in objects.Resolve(document, op)
                    from lease in spec.Mint(op)
                    from applied in lease.Use(mapping =>
                        from _ in profile.Apply(mapping, op)
                        from bound in ids.TraverseM(id =>
                            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                            from code in op.Catch(() => Fin.Succ(command.ObjectMotion switch {
                                { IsSome: true, Case: Transform motion } =>
                                    native.SetTextureMapping(command.Channel.Value, mapping, motion),
                                _ => native.SetTextureMapping(command.Channel.Value, mapping),
                            }))
                            from __ in guard(code != 0, op.InvalidResult())
                            select id).As()
                        select ContentReceipt.Objects(ContentSlot.Mapped, bound))
                    select applied,
                stamp: static (value, serial) => serial > 0u ? value + ContentReceipt.UndoRecords(Seq(serial)) : value,
                op: op),
            key: op,
            needs: SessionNeed.Mutation(undo: true, redraw: redraw).ToArray())
        select receipt;

    private static Fin<MappingResult> Read<TState>(
        DocumentSession session,
        TableTarget target,
        MappingChannel channel,
        Op op,
        TState state,
        Func<TState, TextureMapping, Option<Transform>, Op, Fin<MappingResult>> project) =>
        from activeTarget in op.Need(target)
        from activeProject in op.Need(project)
        from answer in session.Demand(
            use: document =>
                from ids in activeTarget.Resolve(document, op)
                from id in ids switch {
                    [var only] => Fin.Succ(only),
                    _ => Fin.Fail<Guid>(op.InvalidInput()),
                }
                from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                from result in op.Catch(() =>
                    Optional(native.GetTextureMapping(channel.Value, out Transform motion))
                        .ToFin(Fail: op.MissingContext())
                        .Bind(mapping => new Lease<TextureMapping>.Owned(Value: mapping).Use(active =>
                            from _ in guard(motion.IsValid, op.InvalidResult()).ToFin()
                            from projected in activeProject(state, active, Some(motion), op)
                            select projected)))
                select result,
            key: op,
            needs: [SessionNeed.Read])
        select answer;

    private static Fin<MappingCensus> Census(DocumentSession session, TableTarget target, Op op) =>
        from activeTarget in op.Need(target)
        from census in session.Demand(
            use: document =>
                from ids in activeTarget.Resolve(document, op)
                from rows in ids.TraverseM(id =>
                    from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                    from channels in op.Catch(() => native.Attributes.HasMapping && native.HasTextureMapping()
                        ? toSeq(native.GetTextureChannels()).TraverseM(value => MappingChannel.Of(value, op)).As()
                        : Fin.Succ(Seq<MappingChannel>()))
                    select (Object: id, Channels: channels)).As()
                select new MappingCensus(rows),
            key: op,
            needs: [SessionNeed.Read])
        select census;
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]             | [FORM]                                     | [ENTRY]                     |
| :-----: | :--------------- | :------------------ | :----------------------------------------- | :-------------------------- |
|  [01]   | host vocabulary  | `MappingKind`       | keyed rows with inverse behavior           | `Of` / `Recover`            |
|  [02]   | inverse channels | `MappingRecovery`   | value spec beside the native mesh lease    | `Of` / `Bare`               |
|  [03]   | mapping policy   | `MappingProfile`    | admitted non-derived host state            | `Validate` / `Of` / `Apply` |
|  [04]   | construction     | `MappingSpec`       | factory union with custom-mesh custody     | `Mint`                      |
|  [05]   | inverse evidence | `MappingSnapshot`   | value-only kind plus recoverable spec      | `Of`                        |
|  [06]   | side taxonomy    | `SideCode`          | host side ordinals per mapping type        | `MappingSide.Of`            |
|  [07]   | point and frame  | `MappingRequest`    | evaluation and decomposition cases         | `Mappings.Run`              |
|  [08]   | channel mutation | `MappingRequest`    | one bind case under demand and undo        | `Mappings.Run`              |
|  [09]   | channel census   | `MappingRequest`    | attribute-gated per-object channel rows    | `Mappings.Run`              |
|  [10]   | tag round trip   | `ChannelTag`        | admitted kind with native projection       | `Of` / `Native`             |
|  [11]   | coordinate cache | `CoordinateRequest` | prime, read, probe, or scoped invalidation | `TextureCoordinates.Run`    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
