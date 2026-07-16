# [RASM_RHINO_RENDER_MAPPING]

Texture-mapping geometry rail (`Rasm.Rhino.Render`). `MappingSpec` closes the exact factory roster and inverse primitive recovery; custom-mesh recovery carries its mesh through explicit lease custody. `MappingConfig` owns every public settable mapping property, `MappingSnapshot` is the disposing capsule combining classification, identity, configuration, recovered construction state, and the object transform, `MappingFrame` carries UI-parity decomposition, and `MappingProbe` evaluates points with side evidence. `Mappings` binds specs through `TableTarget`, one demand window, one undo record, and `RedrawPolicy`, stamping `Mapped` and undo facts onto the registry's `ContentReceipt` stream. `CoordinateBlock.Read` acquires and disposes the public mesh-side cache wrapper. Every `TextureMapping` mint or object read rides a lease.

## [01]-[INDEX]

- [02]-[SPEC]: `MappingSpec` — the factory roster with its mint and the spec-recovery inverse on one owner.
- [03]-[SNAPSHOT_AND_PROBE]: `MappingConfig`, the `MappingSnapshot` capsule, `MappingFrame`, and `MappingProbe`.
- [04]-[CHANNEL_BINDING]: `ChannelTag`, `CoordinateBlock`, and the `Mappings` bind/read/census rail.
- [05]-[SURFACE_LEDGER]: page owner table.

## [02]-[SPEC]

- Owner: `MappingSpec` `[Union]` — one case per verified factory: surface parameter, planar, OCS, cylinder, sphere, box, and custom mesh. `Planar.Capped` carries behavior, not overload provenance; the five-argument factory generates both capped states. `MeshCustom` accepts a borrowed or owned mesh lease, and inverse recovery returns an owned lease.
- Law: forward and inverse are one owner — `Mint` constructs, `Of` recovers through `TryGetMappingPlane`/`TryGetMappingBox`/`TryGetMappingSphere`/`TryGetMappingCylinder`/`TryGetMappingMesh` dispatched on `MappingType`, and a mapping whose primitive the host cannot recover (surface, brep, false-colors primitives) answers absence rather than a lossy guess.
- Law: the mint is a lease — `TextureMapping` is a `ModelComponent` the caller owns until the bind copies it onto an object; a bind disposes the lease after the host copy, and an unbound mint disposes on the lease.
- Law: an OCS spec binds only on `ObjectAttributes.OCSMappingChannelId` — the bind rail refuses the mismatch typed instead of letting the host misroute the channel.

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
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MappingSpec : IDisposable {
    private MappingSpec() { }
    public sealed record SurfaceParameter : MappingSpec;
    public sealed record Planar(Plane Frame, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingSpec;
    public sealed record Ocs(Plane Frame) : MappingSpec;
    public sealed record Cylindrical(Cylinder Body, bool Capped) : MappingSpec;
    public sealed record Spherical(Sphere Body) : MappingSpec;
    public sealed record Boxed(Plane Frame, Interval Dx, Interval Dy, Interval Dz, bool Capped) : MappingSpec;
    public sealed record MeshCustom(Lease<Mesh> Coordinates) : MappingSpec;

    internal Fin<Lease<TextureMapping>> Mint(Op key) =>
        Switch(
            state: key,
            surfaceParameter: static (op, _) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateSurfaceParameterMapping()).ToFin(Fail: op.InvalidResult()))),
            planar: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreatePlaneMapping(
                    plane: spec.Frame, dx: spec.Dx, dy: spec.Dy, dz: spec.Dz, capped: spec.Capped))
                    .ToFin(Fail: op.InvalidResult()))),
            ocs: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateOcsMapping(plane: spec.Frame)).ToFin(Fail: op.InvalidResult()))),
            cylindrical: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateCylinderMapping(cylinder: spec.Body, capped: spec.Capped)).ToFin(Fail: op.InvalidResult()))),
            spherical: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateSphereMapping(sphere: spec.Body)).ToFin(Fail: op.InvalidResult()))),
            boxed: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateBoxMapping(plane: spec.Frame, dx: spec.Dx, dy: spec.Dy, dz: spec.Dz, capped: spec.Capped))
                    .ToFin(Fail: op.InvalidResult()))),
            meshCustom: static (op, spec) => Owned(op.Catch(() =>
                Optional(TextureMapping.CreateCustomMeshMapping(mesh: spec.Coordinates.Resource)).ToFin(Fail: op.InvalidResult()))));

    internal static Option<MappingSpec> Of(TextureMapping mapping) =>
        mapping.MappingType switch {
            TextureMappingType.SurfaceParameters => Some((MappingSpec)new SurfaceParameter()),
            TextureMappingType.PlaneMapping =>
                mapping.TryGetMappingPlane(out Plane plane, out Interval dx, out Interval dy, out Interval dz, out bool planeCapped)
                    ? Some((MappingSpec)new Planar(Frame: plane, Dx: dx, Dy: dy, Dz: dz, Capped: planeCapped))
                    : Option<MappingSpec>.None,
            TextureMappingType.OcsMapping =>
                mapping.TryGetMappingPlane(out Plane ocs, out Interval _, out Interval _, out Interval _)
                    ? Some((MappingSpec)new Ocs(Frame: ocs))
                    : Option<MappingSpec>.None,
            TextureMappingType.CylinderMapping =>
                mapping.TryGetMappingCylinder(out Cylinder cylinder, out bool cylinderCapped)
                    ? Some((MappingSpec)new Cylindrical(Body: cylinder, Capped: cylinderCapped))
                    : Option<MappingSpec>.None,
            TextureMappingType.SphereMapping =>
                mapping.TryGetMappingSphere(out Sphere sphere)
                    ? Some((MappingSpec)new Spherical(Body: sphere))
                    : Option<MappingSpec>.None,
            TextureMappingType.BoxMapping =>
                mapping.TryGetMappingBox(out Plane box, out Interval bx, out Interval by, out Interval bz, out bool boxCapped)
                    ? Some((MappingSpec)new Boxed(Frame: box, Dx: bx, Dy: by, Dz: bz, Capped: boxCapped))
                    : Option<MappingSpec>.None,
            TextureMappingType.MeshMappingPrimitive => MeshOf(mapping),
            _ => Option<MappingSpec>.None,
        };

    public void Dispose() => Switch(
        surfaceParameter: static _ => unit,
        planar: static _ => unit,
        ocs: static _ => unit,
        cylindrical: static _ => unit,
        spherical: static _ => unit,
        boxed: static _ => unit,
        meshCustom: static spec => spec.Coordinates.Dispose());

    private static Option<MappingSpec> MeshOf(TextureMapping mapping) {
        bool found = mapping.TryGetMappingMesh(out Mesh mesh);
        if (!found) {
            mesh?.Dispose();
            return Option<MappingSpec>.None;
        }
        return Some((MappingSpec)new MeshCustom(Coordinates: new Lease<Mesh>.Owned(Value: mesh)));
    }

    private static Fin<Lease<TextureMapping>> Owned(Fin<TextureMapping> minted) =>
        minted.Map(static mapping => (Lease<TextureMapping>)new Lease<TextureMapping>.Owned(Value: mapping));
}
```

## [03]-[SNAPSHOT_AND_PROBE]

- Owner: `MappingConfig` — every settable mapping property: texture space, projection, cap state, and UVW/primitive/normal transforms. `MappingSnapshot` — native type and id plus configuration, optional recovered spec, and the object transform the read recovered beside it. `MappingFrame` — UI-parity XYZ and UVW decomposition. `MappingProbe` — point, normal, optional transform pair, side index, and mapped coordinate.
- Law: the snapshot is its own disposing capsule because custom-mesh recovery owns a `Mesh`; disposing the snapshot disposes only owned spec payloads, while borrowed input specs remain untouched.
- Law: `Evaluate` returns the side index as evidence — the host contract is nonzero-on-success with `0` the failure sentinel, box and cylinder mappings discriminate faces through the value, so the probe answer is `(Side, Point)` gated on a nonzero side, never a bare point that erases the face or a `>= 0` gate that certifies the failure code.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record MappingConfig(
    TextureSpace Space,
    Projection Projection,
    bool Capped,
    Transform UvwTransform,
    Transform PrimitiveTransform,
    Transform NormalTransform) : IDetachedDocumentResult {
    internal static MappingConfig Of(TextureMapping mapping) =>
        new(
            Space: mapping.TextureSpace,
            Projection: mapping.Projection,
            Capped: mapping.Capped,
            UvwTransform: mapping.UvwTransform,
            PrimitiveTransform: mapping.PrimitiveTransform,
            NormalTransform: mapping.NormalTransform);

    internal Fin<Unit> Apply(TextureMapping mapping, Op key) {
        MappingConfig self = this;
        return key.Catch(() => {
            mapping.TextureSpace = self.Space;
            mapping.Projection = self.Projection;
            mapping.Capped = self.Capped;
            mapping.UvwTransform = self.UvwTransform;
            mapping.PrimitiveTransform = self.PrimitiveTransform;
            mapping.NormalTransform = self.NormalTransform;
            return Fin.Succ(value: unit);
        });
    }
}

public sealed record MappingSnapshot(
    TextureMappingType Type,
    Guid Id,
    MappingConfig Config,
    Option<MappingSpec> Spec,
    Option<Transform> ObjectMotion) : IDisposable, IDetachedDocumentResult {
    internal static MappingSnapshot Of(TextureMapping mapping, Option<Transform> objectMotion) =>
        new(
            Type: mapping.MappingType,
            Id: mapping.Id,
            Config: MappingConfig.Of(mapping: mapping),
            Spec: MappingSpec.Of(mapping: mapping),
            ObjectMotion: objectMotion);

    public void Dispose() => Spec.Iter(static spec => spec.Dispose());
}

public readonly record struct MappingFrame(
    Vector3d Position, Vector3d Scale, Vector3d Rotation,
    Vector3d UvwOffset, Vector3d UvwRepeat, Vector3d UvwRotation) : IDetachedDocumentResult {
    internal static Fin<MappingFrame> Of(TextureMapping mapping, Transform localTransform, Op key) =>
        key.Catch(() => {
            mapping.Decompose(
                localTransform: localTransform,
                xyz_position: out Vector3d position, xyz_scale: out Vector3d scale, xyz_rotation: out Vector3d rotation,
                uvw_offset: out Vector3d offset, uvw_repeat: out Vector3d repeat, uvw_rotation: out Vector3d spin);
            return Fin.Succ(value: new MappingFrame(
                Position: position, Scale: scale, Rotation: rotation,
                UvwOffset: offset, UvwRepeat: repeat, UvwRotation: spin));
        });
}

public sealed record MappingProbe(Point3d Point, Vector3d Normal, Option<(Transform Points, Transform Normals)> Motion = default) {
    internal Fin<(int Side, Point3d Mapped)> Evaluate(TextureMapping mapping, Op key) {
        MappingProbe self = this;
        return from _ in guard(
                   self.Point.IsValid
                   && self.Normal.IsValid
                   && self.Motion.Map(static motion => motion.Points.IsValid && motion.Normals.IsValid).IfNone(true),
                   key.InvalidInput())
               from evaluated in key.Catch(() => {
                   (int side, Point3d mapped) = self.Motion.Case switch {
                       (Transform points, Transform normals) =>
                           (mapping.Evaluate(p: self.Point, n: self.Normal, t: out Point3d moved, pXform: points, nXform: normals), moved),
                       _ => (mapping.Evaluate(p: self.Point, n: self.Normal, t: out Point3d direct), direct),
                   };
                   return side != 0 && mapped.IsValid
                       ? Fin.Succ(value: (side, mapped))
                       : Fin.Fail<(int, Point3d)>(error: key.InvalidResult());
               })
               select evaluated;
    }
}
```

## [04]-[CHANNEL_BINDING]

- Owner: `ChannelTag` — the detached host `MappingTag` value: mapping id, type, CRC, and mesh transform. `CoordinateBlock` — detached dimension, mapping id, and coordinate rows; `Read` acquires through `Mesh.GetCachedTextureCoordinates(Guid)` and disposes the `CachedTextureCoordinates` wrapper. `Mappings` — receipted bind, capsule snapshot read, and the detached `MappingCensus`.
- Law: the channel is a raw `int` — the host carries no channel vocabulary, `ObjectAttributes.OCSMappingChannelId` is the one named constant, and the OCS-spec/channel invariant gates at `Bind` before any host call.
- Law: binding addresses through the document vocabulary — `TableTarget` resolves ids inside the `Demand` window, `SetTextureMapping` confirms per object on a nonzero code, `RedrawPolicy` restores suppression on every exit, and the bound ids land as `Mapped` facts on the registry's `ContentReceipt` beside the sealed undo serial; the minted lease disposes after the host copies it, on success and failure alike.
- Law: no public document mapping table exists — per-object accessors are the whole binding surface, and the document-level mapping-mutation broadcast is the Document events page's `TextureMappingTable` family.
- Law: cached coordinates retrieve publicly from `Mesh.GetCachedTextureCoordinates(Guid)`; `Mesh.SetCachedTextureCoordinatesFromMaterial(RhinoObject, Material)` plus the texture overload populate material-driven caches, and `HasCachedTextureCoordinates`/`InvalidateCachedTextureCoordinates(bool)` probe and invalidate the cache — all on the geometry seam.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ChannelTag(Guid Id, TextureMappingType Type, uint Crc, Transform MeshTransform)
    : IComparable<ChannelTag>, IDetachedDocumentResult {
    internal static ChannelTag Of(MappingTag tag) =>
        new(Id: tag.Id, Type: tag.MappingType, Crc: tag.MappingCRC, MeshTransform: tag.MeshTransform);

    internal MappingTag Native() =>
        new() { Id = Id, MappingType = Type, MappingCRC = Crc, MeshTransform = MeshTransform };

    public int CompareTo(ChannelTag other) => Native().CompareTo(other.Native());
}

public sealed record MappingCensus(Seq<(Guid Object, Seq<int> Channels)> Rows) : IDetachedDocumentResult;

public sealed record CoordinateBlock(int Dim, Guid MappingId, Arr<Point3d> Rows) : IDetachedDocumentResult {
    internal static Fin<CoordinateBlock> Of(CachedTextureCoordinates coordinates, Op key) =>
        key.Catch(() => Fin.Succ(value: new CoordinateBlock(
            Dim: coordinates.Dim,
            MappingId: coordinates.MappingId,
            Rows: toArr(coordinates))));

    public static Fin<CoordinateBlock> Read(Mesh mesh, Guid mappingId, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(mappingId != Guid.Empty, op.InvalidInput())
               from block in op.Catch(() => {
                   using CachedTextureCoordinates coordinates = mesh.GetCachedTextureCoordinates(mappingId);
                   return Optional(coordinates).ToFin(Fail: op.MissingContext()).Bind(active => Of(active, op));
               })
               from __ in guard(block.MappingId == mappingId && block.Dim > 0, op.InvalidResult())
               select block;
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Mappings {
    public static Fin<ContentReceipt> Bind(
        DocumentSession session, TableTarget objects, int channel, RedrawPolicy redraw, MappingSpec spec,
        Option<MappingConfig> config = default, Option<Transform> objectMotion = default) {
        Op op = Op.Of();
        return from activeSession in Optional(session).ToFin(Fail: op.InvalidInput())
               from target in Optional(objects).ToFin(Fail: op.InvalidInput())
               from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
               from source in Optional(spec).ToFin(Fail: op.InvalidInput())
               from _ in guard(
                   source is not MappingSpec.Ocs || channel == ObjectAttributes.OCSMappingChannelId,
                   op.InvalidInput())
               let needs = Seq(SessionNeed.Mutate, SessionNeed.Undo)
                   + (policy.Enabled ? Seq(SessionNeed.Redraw) : Seq<SessionNeed>())
               from bound in activeSession.Demand(
                   use: document => op.Catch(() => {
                       bool priorRedraw = document.Views.RedrawEnabled;
                       Fin<ContentReceipt> outcome;
                       try {
                           _ = Op.SideWhen(policy.Suppress, () =>
                               document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false));
                           outcome =
                               from ids in target.Resolve(document: document, key: op)
                               from lease in source.Mint(key: op)
                               from applied in lease.Use(mapping =>
                                   from _ in config.Traverse(state => state.Apply(mapping: mapping, key: op)).As()
                                   from result in op.Catch(() => {
                                       using UndoBracket undo = UndoBracket.Begin(
                                           document: document, name: nameof(Bind), recordsUndo: true);
                                       Fin<ContentReceipt> folded = guard(undo.Admitted, op.InvalidResult()).ToFin()
                                           .Bind(_ => ids.TraverseM(id =>
                                               from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                                               from code in op.Catch(() => Fin.Succ(value: objectMotion.Case switch {
                                                   Transform motion => native.SetTextureMapping(channel: channel, tm: mapping, objectTransform: motion),
                                                   _ => native.SetTextureMapping(channel: channel, tm: mapping),
                                               }))
                                               from _ in guard(code != 0, op.InvalidResult())
                                               select id).As()
                                               .Map(bound => ContentReceipt.Objects(slot: ContentSlot.Mapped, ids: bound)));
                                       return undo.Seal(
                                           outcome: folded,
                                           stamp: static (receipt, serial) => serial > 0u
                                               ? receipt + ContentReceipt.UndoRecords(serials: Seq(serial))
                                               : receipt,
                                           key: op);
                                   })
                                   select result)
                               select applied;
                       } finally {
                           _ = Op.SideWhen(policy.Suppress, () =>
                               document.Views.EnableRedraw(enable: priorRedraw, redrawDocument: false, redrawLayers: false));
                       }
                       _ = Op.SideWhen(policy.Enabled && outcome.IsSucc, () => document.Views.Redraw(deferred: policy.Defers));
                       return outcome;
                   }),
                   key: op,
                   needs: needs.ToArray())
               select bound;
    }

    public static Fin<MappingSnapshot> Read(DocumentSession session, TableTarget target, int channel) {
        Op op = Op.Of();
        return session.Demand(
            use: document =>
                from ids in target.Resolve(document: document, key: op)
                from id in ids switch { [var only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: op.InvalidInput()) }
                from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                from read in op.Catch(() =>
                    Optional(native.GetTextureMapping(channel: channel, objectTransform: out Transform motion))
                        .ToFin(Fail: op.MissingContext())
                        .Bind(mapping => new Lease<TextureMapping>.Owned(Value: mapping).Use(active =>
                            op.Catch(() => Fin.Succ(value: MappingSnapshot.Of(
                                mapping: active,
                                objectMotion: motion.IsValid ? Some(motion) : Option<Transform>.None))))))
                select read,
            key: op,
            needs: [SessionNeed.Read]);
    }

    public static Fin<MappingCensus> Census(DocumentSession session, TableTarget target) {
        Op op = Op.Of();
        return session.Demand(
            use: document =>
                from ids in target.Resolve(document: document, key: op)
                from rows in ids.TraverseM(id =>
                    from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                    from channels in op.Catch(() => Fin.Succ(value: native.HasTextureMapping()
                        ? toSeq(native.GetTextureChannels())
                        : Seq<int>()))
                    select (Object: id, Channels: channels)).As()
                select new MappingCensus(Rows: rows),
            key: op,
            needs: [SessionNeed.Read]);
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]           | [FORM]                                            | [ENTRY]                                |
| :-----: | :------------------- | :---------------- | :------------------------------------------------ | :------------------------------------- |
|  [01]   | mapping construction | `MappingSpec`     | one factory union with a leased mint              | `Mint(key)`                            |
|  [02]   | spec recovery        | `MappingSpec`     | same-owner inverse through `TryGet*` dispatch     | `Of(mapping)`                          |
|  [03]   | mapping state        | `MappingSnapshot` | classification, configuration, spec, and motion   | `Of(mapping, objectMotion)`            |
|  [04]   | TRS decomposition    | `MappingFrame`    | host `Decompose` product                          | `Of(mapping, localTransform, key)`     |
|  [05]   | point evaluation     | `MappingProbe`    | point, normal, motion, and side-indexed answer    | `Evaluate(mapping, key)`               |
|  [06]   | channel binding      | `Mappings`        | target bind stamping `Mapped` + undo facts        | `Bind(session, objects, channel, ...)` |
|  [07]   | channel evidence     | `ChannelTag`      | detached, reconstructible, comparable host tag    | `Of(tag)` / `CompareTo(other)`         |
|  [08]   | coordinate cache     | `CoordinateBlock` | mapping-id acquisition and detached rows          | `Read(mesh, mappingId, key)`           |
|  [09]   | mapping read         | `Mappings`        | one snapshot capsule with the object transform    | `Read(session, target, channel)`       |
|  [10]   | channel census       | `Mappings`        | detached `MappingCensus` of per-object channels   | `Census(session, target)`              |
