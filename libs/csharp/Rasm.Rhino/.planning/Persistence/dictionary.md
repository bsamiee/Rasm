# [RASM_RHINO_PERSISTENCE_DICTIONARY]

Typed archive values and detached dictionary custody (`Rasm.Rhino.Persistence`). `ArchiveValue` admits the complete public `ArchivableDictionary.Set` matrix through one runtime-type invariant. `ArchiveCodec` rows own native capture and exact overload dispatch; scalar and sequence generators derive repeated rows without parallel payload cases. `ArchiveMap` owns immutable entry algebra plus live `Detach` and `Mint` crossings. Host reflection-copy members remain excluded because exact-runtime lookup rejects supported arrays and geometry subtypes, while `Clone()` retains non-`ICloneable` carrier aliases.

## [01]-[INDEX]

- [02]-[ARCHIVE_VALUE]: one invariant-bearing payload and its generated codec matrix.
- [03]-[ARCHIVE_MAP]: detached entries, native minting, and enum text projection.
- [04]-[SURFACE_LEDGER]: ownership and entry points.

## [02]-[ARCHIVE_VALUE]

- Owner: `ArchiveValue` is one archive-item identity, independent of the host overload selected to encode it.
- Admission: `Create` and `Decode` resolve one codec row from the payload runtime type; unsupported values fail before entering `ArchiveMap`.
- Custody: host `Set` stores the caller's reference verbatim and `TryGetValue` returns the stored reference, so every reference-bearing row freezes on both crossings — capture copies the caller's payload and write copies into the minted native. Nested dictionaries recurse, geometry duplicates, arrays copy per crossing, and `MeshingParameters`/`ObjRef` freeze through their host copy constructors because neither implements `ICloneable`.
- Dispatch: `Write` resolves the stored payload row and invokes its exact typed `Set` overload. No indexer setter, reflective copy helper, or silent default arm participates.
- Growth: a new host kind extends the codec matrix with one scalar, sequence, or carrier row; archive consumers remain unchanged.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Immutable;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [VALUE_OWNER] --------------------------------------------------------------------------
public sealed record ArchiveValue {
    private readonly object _payload;

    private ArchiveValue(object payload) => _payload = payload;

    public Type ValueType => _payload.GetType();

    public static Fin<ArchiveValue> Create<T>(T value, Op? key = null) {
        Op op = key.OrDefault();
        object? boxed = value;
        return from present in Optional(boxed).ToFin(Fail: op.InvalidInput())
               from captured in ArchiveCodecs.Capture(source: present, op: op)
               select new ArchiveValue(payload: captured);
    }

    public Fin<T> As<T>(Op? key = null) {
        Op op = key.OrDefault();
        return _payload is T value
            ? Fin.Succ(value: value)
            : Fin.Fail<T>(error: op.Unsupported(geometryType: ValueType, outputType: typeof(T)));
    }

    internal Fin<Unit> Write(ArchivableDictionary target, string name, Op op) =>
        ArchiveCodecs.Write(payload: _payload, target: target, name: name, op: op);

    internal static Fin<ArchiveValue> Decode(object boxed, Op op) =>
        ArchiveCodecs.Capture(source: boxed, op: op).Map(static payload => new ArchiveValue(payload: payload));
}

// --- [CODEC_MATRIX] -------------------------------------------------------------------------
internal static class ArchiveCodecs {
    private interface IArchiveCodec {
        bool AcceptsNative(Type type);
        bool AcceptsStored(Type type);
        Fin<object> Capture(object source, Op op);
        Fin<Unit> Write(object payload, ArchivableDictionary target, string name, Op op);
    }

    private sealed class Codec<TNative, TStored>(
        Func<TNative, Op, Fin<TStored>> capture,
        Func<ArchivableDictionary, string, TStored, Op, Fin<Unit>> write,
        bool nativeSubtypes = false,
        bool storedSubtypes = false) : IArchiveCodec {
        public bool AcceptsNative(Type type) =>
            nativeSubtypes ? typeof(TNative).IsAssignableFrom(type) : type == typeof(TNative);

        public bool AcceptsStored(Type type) =>
            storedSubtypes ? typeof(TStored).IsAssignableFrom(type) : type == typeof(TStored);

        public Fin<object> Capture(object source, Op op) =>
            source is TNative value
                ? capture(value, op).Map(static frozen => (object)frozen!)
                : Fin.Fail<object>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(TStored)));

        public Fin<Unit> Write(object payload, ArchivableDictionary target, string name, Op op) =>
            payload is TStored value
                ? write(target, name, value, op)
                : Fin.Fail<Unit>(error: op.Unsupported(geometryType: payload.GetType(), outputType: typeof(TStored)));
    }

    private static readonly ImmutableArray<IArchiveCodec> Rows = [
        Scalar<bool>(static (d, k, v) => d.Set(k, v)),
        Scalar<byte>(static (d, k, v) => d.Set(k, v)),
        Scalar<sbyte>(static (d, k, v) => d.Set(k, v)),
        Scalar<short>(static (d, k, v) => d.Set(k, v)),
        Scalar<ushort>(static (d, k, v) => d.Set(k, v)),
        Scalar<int>(static (d, k, v) => d.Set(k, v)),
        Scalar<uint>(static (d, k, v) => d.Set(k, v)),
        Scalar<long>(static (d, k, v) => d.Set(k, v)),
        Scalar<float>(static (d, k, v) => d.Set(k, v)),
        Scalar<double>(static (d, k, v) => d.Set(k, v)),
        Scalar<Guid>(static (d, k, v) => d.Set(k, v)),
        Scalar<string>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.Color>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.Point>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.PointF>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.Rectangle>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.RectangleF>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.Size>(static (d, k, v) => d.Set(k, v)),
        Scalar<System.Drawing.SizeF>(static (d, k, v) => d.Set(k, v)),
        new Codec<System.Drawing.Font, System.Drawing.Font>(
            capture: static (v, op) => op.Catch(() => Fin.Succ(value: (System.Drawing.Font)v.Clone())),
            write: static (d, k, v, op) => op.Confirm(success: d.Set(k, v))),
        Scalar<Interval>(static (d, k, v) => d.Set(k, v)),
        Scalar<Point2d>(static (d, k, v) => d.Set(k, v)),
        Scalar<Point3d>(static (d, k, v) => d.Set(k, v)),
        Scalar<Point4d>(static (d, k, v) => d.Set(k, v)),
        Scalar<Vector2d>(static (d, k, v) => d.Set(k, v)),
        Scalar<Vector3d>(static (d, k, v) => d.Set(k, v)),
        Scalar<BoundingBox>(static (d, k, v) => d.Set(k, v)),
        Scalar<Ray3d>(static (d, k, v) => d.Set(k, v)),
        Scalar<Transform>(static (d, k, v) => d.Set(k, v)),
        Scalar<Plane>(static (d, k, v) => d.Set(k, v)),
        Scalar<Line>(static (d, k, v) => d.Set(k, v)),
        Scalar<Point3f>(static (d, k, v) => d.Set(k, v)),
        Scalar<Vector3f>(static (d, k, v) => d.Set(k, v)),
        new Codec<ArchivableDictionary, ArchiveMap>(
            capture: static (v, op) => ArchiveMap.Detach(source: v, key: op),
            write: static (d, k, v, op) => v.Mint(key: op).Bind(minted => op.Confirm(success: d.Set(k, minted)))),
        new Codec<MeshingParameters, MeshingParameters>(
            capture: static (v, op) => op.Catch(() => Fin.Succ(value: new MeshingParameters(source: v))),
            write: static (d, k, v, op) => op.Confirm(success: d.Set(k, new MeshingParameters(source: v)))),
        new Codec<GeometryBase, GeometryBase>(
            capture: static (v, op) => op.Catch(() => Fin.Succ(value: v.Duplicate())),
            write: static (d, k, v, op) => op.Confirm(success: d.Set(k, v.Duplicate())),
            nativeSubtypes: true,
            storedSubtypes: true),
        new Codec<ObjRef, ObjRef>(
            capture: static (v, op) => op.Catch(() => Fin.Succ(value: new ObjRef(other: v))),
            write: static (d, k, v, op) => op.Confirm(success: d.Set(k, new ObjRef(other: v)))),
        Sequence<bool>(static (d, k, v) => d.Set(k, v)),
        Sequence<byte>(static (d, k, v) => d.Set(k, v)),
        Sequence<sbyte>(static (d, k, v) => d.Set(k, v)),
        Sequence<short>(static (d, k, v) => d.Set(k, v)),
        Sequence<int>(static (d, k, v) => d.Set(k, v)),
        Sequence<float>(static (d, k, v) => d.Set(k, v)),
        Sequence<double>(static (d, k, v) => d.Set(k, v)),
        Sequence<Guid>(static (d, k, v) => d.Set(k, v)),
        Sequence<string>(static (d, k, v) => d.Set(k, v)),
        new Codec<GeometryBase[], GeometryBase[]>(
            capture: static (v, op) => op.Catch(() => Fin.Succ(value: v.Map(static x => x.Duplicate()).ToArray())),
            write: static (d, k, v, op) => op.Confirm(success: d.Set(k, v.Map(static x => x.Duplicate())))),
        new Codec<ObjRef[], ObjRef[]>(
            capture: static (v, op) => op.Catch(() => Fin.Succ(value: v.Map(static x => new ObjRef(other: x)).ToArray())),
            write: static (d, k, v, op) => op.Confirm(success: d.Set(k, v.Map(static x => new ObjRef(other: x)))))
    ];

    internal static Fin<object> Capture(object source, Op op) =>
        from codec in ResolveNative(type: source.GetType(), op: op)
        from captured in codec.Capture(source: source, op: op)
        select captured;

    internal static Fin<Unit> Write(object payload, ArchivableDictionary target, string name, Op op) =>
        from codec in ResolveStored(type: payload.GetType(), op: op)
        from written in codec.Write(payload: payload, target: target, name: name, op: op)
        select written;

    private static IArchiveCodec Scalar<T>(Func<ArchivableDictionary, string, T, bool> set) where T : notnull =>
        new Codec<T, T>(
            capture: static (value, _) => Fin.Succ(value: value),
            write: (dictionary, name, value, op) => op.Confirm(success: set(dictionary, name, value)));

    private static IArchiveCodec Sequence<T>(Func<ArchivableDictionary, string, IEnumerable<T>, bool> set) =>
        new Codec<T[], T[]>(
            capture: static (value, _) => Fin.Succ<T[]>(value: [.. value]),
            write: (dictionary, name, value, op) => op.Confirm(success: set(dictionary, name, [.. value])));

    private static Fin<IArchiveCodec> ResolveNative(Type type, Op op) =>
        toSeq(Rows).Find(codec => codec.AcceptsNative(type)).ToFin(
            Fail: op.Unsupported(geometryType: type, outputType: typeof(ArchiveValue)));

    private static Fin<IArchiveCodec> ResolveStored(Type type, Op op) =>
        toSeq(Rows).Find(codec => codec.AcceptsStored(type)).ToFin(
            Fail: op.Unsupported(geometryType: type, outputType: typeof(ArchivableDictionary)));
}
```

## [03]-[ARCHIVE_MAP]

- Owner: `ArchiveMap` combines schema identity and immutable typed entries; private initialization blocks invalid names and entry keys.
- Crossing: `Detach` reads every key through `TryGetValue`, captures the host `ChangeSerialNumber` as `Change` evidence for staleness probes against a later detach, and aborts on the first unsupported or unreadable item. `Mint` creates a fresh native and aborts on the first rejected write.
- Algebra: `Find`, `With`, `Without`, and `Merge` operate only on already-admitted `ArchiveValue` instances.
- Enum projection: host enum helpers encode invariant names as strings, so `WithEnum` and `EnumOf` remain generic text projections instead of new value cases.
- Seam: session acquisition, user-data archives, and snapshot participation compose `Detach` and `Mint`; none receives a caller-mutable native dictionary.

```csharp signature
// --- [MAP_OWNER] ----------------------------------------------------------------------------
public sealed record ArchiveMap : IDetachedDocumentResult {
    private ArchiveMap(HashMap<string, ArchiveValue> entries, int version, string name) =>
        (Entries, Version, Name) = (entries, version, name);

    public HashMap<string, ArchiveValue> Entries { get; private init; }
    public int Version { get; private init; }
    public string Name { get; private init; }
    public uint Change { get; private init; }
    public int Count => Entries.Count;

    public static Fin<ArchiveMap> Create(
        HashMap<string, ArchiveValue> entries,
        int version,
        string name,
        Op? key = null) {
        Op op = key.OrDefault();
        return name is not null && entries.AsIterable().All(static pair => !string.IsNullOrEmpty(pair.Key))
            ? Fin.Succ(value: new ArchiveMap(entries: entries, version: version, name: name))
            : Fin.Fail<ArchiveMap>(error: op.InvalidInput());
    }

    public Option<ArchiveValue> Find(string name) => Entries.Find(key: name);

    public Fin<ArchiveMap> With(string name, ArchiveValue value, Op? key = null) {
        Op op = key.OrDefault();
        return string.IsNullOrEmpty(name)
            ? Fin.Fail<ArchiveMap>(error: op.InvalidInput())
            : Fin.Succ(value: this with { Entries = Entries.AddOrUpdate(key: name, value: value) });
    }

    public ArchiveMap Without(string name) => this with { Entries = Entries.Remove(key: name) };

    public ArchiveMap Merge(ArchiveMap overlay) => this with {
        Entries = overlay.Entries.AsIterable().Fold(
            Entries,
            static (state, pair) => state.AddOrUpdate(key: pair.Key, value: pair.Value))
    };

    public Fin<ArchiveMap> WithEnum<T>(T value, Option<string> name = default, Op? key = null) where T : struct, Enum =>
        ArchiveValue.Create(value: value.ToString(), key: key).Bind(
            text => With(name: name.IfNone(typeof(T).Name), value: text, key: key));

    public Fin<T> EnumOf<T>(Option<string> name = default, Op? key = null) where T : struct, Enum {
        Op op = key.OrDefault();
        return Find(name: name.IfNone(typeof(T).Name)).ToFin(Fail: op.MissingContext())
            .Bind(held => held.As<string>(key: op))
            .Bind(text => Enum.TryParse(value: text, ignoreCase: true, result: out T value)
                ? Fin.Succ(value: value)
                : Fin.Fail<T>(error: op.InvalidResult(detail: text)));
    }

    public static Fin<ArchiveMap> Detach(ArchivableDictionary source, Op? key = null) {
        Op op = key.OrDefault();
        return from live in Optional(source).ToFin(Fail: op.InvalidInput())
               from entries in toSeq(live.Keys).TraverseM(name =>
                       live.TryGetValue(key: name, value: out object boxed)
                           ? ArchiveValue.Decode(boxed: boxed, op: op).Map(value => (name, value))
                           : Fin.Fail<(string, ArchiveValue)>(error: op.MissingContext()))
                   .As()
               from map in Create(
                   entries: entries.Fold(
                       HashMap<string, ArchiveValue>(),
                       static (state, pair) => state.AddOrUpdate(key: pair.Item1, value: pair.Item2)),
                   version: live.Version,
                   name: live.Name ?? string.Empty,
                   key: op)
               select map with { Change = live.ChangeSerialNumber };
    }

    public Fin<ArchivableDictionary> Mint(Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => {
            ArchivableDictionary minted = new(version: Version, name: Name);
            return Entries.AsIterable()
                .TraverseM(pair => pair.Value.Write(target: minted, name: pair.Key, op: op))
                .As()
                .Map(_ => minted);
        });
    }
}
```

## [04]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]      | [OWNER]         | [FORM]                                                   | [ENTRY]                               |
| :-----: | :------------- | :-------------- | :------------------------------------------------------- | :------------------------------------ |
|  [01]   | archive item   | `ArchiveValue`  | one invariant-bearing payload                            | `Create` / `As`                       |
|  [02]   | host matrix    | `ArchiveCodecs` | generated scalar and sequence rows plus carrier policies | `Capture` / `Write`                   |
|  [03]   | detached store | `ArchiveMap`    | schema identity, change evidence, immutable entries      | `Detach` / `Mint`                     |
|  [04]   | entry algebra  | `ArchiveMap`    | immutable lookup, replacement, removal, and overlay      | `Find` / `With` / `Without` / `Merge` |
|  [05]   | enum text      | `ArchiveMap`    | generic invariant-name projection                        | `WithEnum<T>` / `EnumOf<T>`           |
