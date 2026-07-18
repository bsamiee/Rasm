# [ARCHIVE_DICTIONARY]

`ArchiveValue` is the folder's ONE typed boxed-host-value carrier: every payload a KV boundary moves — archive scalars, sequences, drawing values, geometry, carriers, enums, and the settings-only shapes — admits through one frozen slot registry whose rows carry the host-type keys, the defensive-copy law, and the native write column. `ArchiveMap` admits one native dictionary, preserves schema identity, tracks content independently of the host change serial, and mints one fresh native dictionary for each egress. String owners admit through the kernel `Op.AcceptValidated` string row — the one factory bridge onto the rail; a folder-local bridge beside it is the deleted form.

## [01]-[OWNERS]

One slot row per host payload type replaces enumerated case arms: capture, host write, and detached projection all derive from the row, so a new payload is one registry row and every consumer is complete by construction. Rows without a native `Set` overload — `char`, `DateTime`, `Option<Color>`, `HashMap<string, string>` — are held payloads that refuse the archive boundary with a typed unsupported fault and serve the settings boundary alone. Enum payloads carry `(Type, Name)` evidence through one `EnumMint` reflection seam shared by both host targets.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Collections.Frozen;
using System.Drawing;
using System.Globalization;
using LanguageExt;
using Rasm.Domain;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

// ──────────────────────────── VALUE VOCABULARY ─────────────────────────────

[ValueObject<string>]
public readonly partial struct ArchiveKey
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        value = value.Trim();
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError("Archive key is empty.")
            : null;
    }
}

[ValueObject<string>]
public readonly partial struct ArchiveName
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        value = value.Trim();
        validationError = null;
    }
}

public sealed record ArchiveValue
{
    private ArchiveValue(Slot slot, object payload) => (Row, Payload) = (slot, payload);

    private Slot Row { get; }

    internal object Payload { get; }

    public Type Shape => Payload is Entry stored ? stored.EnumType : Row.Shape;

    internal Option<(Type EnumType, string Name)> EnumEntry =>
        Payload is Entry stored ? Some((stored.EnumType, stored.Name)) : None;

    internal bool Same(ArchiveValue other) => ReferenceEquals(Row, other.Row) && Row.Same(Payload, other.Payload);

    internal Fin<ArchiveValue> AdmitArchive(Op op) => Row.Mint is not null
        ? Fin.Succ(value: this)
        : Fin.Fail<ArchiveValue>(error: op.Unsupported(geometryType: Shape, outputType: typeof(ArchivableDictionary)));

    public static Fin<ArchiveValue> Of<T>(T source, Op? key = null) where T : notnull =>
        Capture(source: source, op: key.OrDefault());

    internal static Fin<ArchiveValue> Capture(object? source, Op op) =>
        Optional(source)
            .ToFin(Fail: op.InvalidInput())
            .Bind(value => Resolve(source: value)
                .ToFin(Fail: op.Unsupported(geometryType: value.GetType(), outputType: typeof(ArchiveValue)))
                .Bind(slot => slot.Admit(value, op).Map(payload => new ArchiveValue(slot, payload))));

    internal static Fin<ArchiveValue> Enum(object? source, Op op) => Optional(source)
        .ToFin(Fail: op.InvalidInput())
        .Bind(value => value is System.Enum ? Capture(value, op) : Fin.Fail<ArchiveValue>(error: op.InvalidInput()));

    internal static Fin<Unit> EnumMint(object target, string method, string key, (Type EnumType, string Name) entry, Op op) =>
        op.Catch(() => op.Confirm(success: target.GetType()
            .GetMethods()
            .Single(candidate => candidate.Name == method && candidate.IsGenericMethodDefinition && candidate.GetParameters().Length == 2)
            .MakeGenericMethod(entry.EnumType)
            .Invoke(target, [key, System.Enum.Parse(enumType: entry.EnumType, value: entry.Name, ignoreCase: true)]) is true));

    public Fin<T> Project<T>(Op? key = null)
    {
        Op op = key.OrDefault();
        return Payload switch
        {
            Entry stored => typeof(T) == stored.EnumType
                && System.Enum.TryParse(enumType: stored.EnumType, value: stored.Name, ignoreCase: true, result: out object? parsed)
                && parsed is T value
                ? Fin.Succ(value: value)
                : Fin.Fail<T>(error: op.Unsupported(geometryType: stored.EnumType, outputType: typeof(T))),
            T typed => Fin.Succ(value: (T)Row.Detach(typed)),
            _ => Fin.Fail<T>(error: op.Unsupported(geometryType: Row.Shape, outputType: typeof(T))),
        };
    }

    internal Fin<Unit> Write(ArchivableDictionary target, ArchiveKey key, Op op) => Row.Mint is { } mint
        ? mint(target, key, Payload, op)
        : Fin.Fail<Unit>(error: op.Unsupported(geometryType: Shape, outputType: typeof(ArchivableDictionary)));

    private static Option<Slot> Resolve(object source) => source switch
    {
        System.Enum => Some(EnumRow),
        _ when Registry.TryGetValue(source.GetType(), out Slot? exact) => Some(exact!),
        _ => source switch
        {
            GeometryBase[] => Some(GeometrySeqRow),
            ObjRef[] => Some(ObjRefSeqRow),
            ArchivableDictionary => Some(MapRow),
            MeshingParameters => Some(MeshingRow),
            GeometryBase => Some(GeometryRow),
            ObjRef => Some(ObjRefRow),
            _ => None,
        },
    };

    private readonly record struct Entry(Type EnumType, string Name);

    private sealed record Slot(
        Type Shape,
        Seq<Type> Keys,
        Func<object, Op, Fin<object>> Admit,
        Func<object, object> Detach,
        Func<object, object, bool> Same,
        Func<ArchivableDictionary, ArchiveKey, object, Op, Fin<Unit>>? Mint);

    private static Slot Scalar<T>(Func<ArchivableDictionary, string, T, bool>? set = null) where T : notnull => new(
        Shape: typeof(T),
        Keys: [typeof(T)],
        Admit: static (value, _) => Fin.Succ(value: value),
        Detach: static value => value,
        Same: static (left, right) => EqualityComparer<T>.Default.Equals((T)left, (T)right),
        Mint: set is null
            ? null
            : (target, key, value, op) => op.Catch(() => op.Confirm(success: set(target, key.Value, (T)value))));

    private static Slot Rows<T>(Func<ArchivableDictionary, string, Seq<T>, bool> set) => new(
        Shape: typeof(T[]),
        Keys: [typeof(T[]), typeof(Seq<T>)],
        Admit: static (value, _) => Fin.Succ<object>(value: value is T[] host ? toSeq(host) : value),
        Detach: static value => value,
        Same: static (left, right) => ((Seq<T>)left).SequenceEqual((Seq<T>)right),
        Mint: (target, key, value, op) => op.Catch(() => op.Confirm(success: set(target, key.Value, (Seq<T>)value))));

    private static Slot Copy<T>(
        Func<T, T> clone,
        Func<T, T, bool> same,
        Func<ArchivableDictionary, string, T, bool> set) where T : class => new(
        Shape: typeof(T),
        Keys: [typeof(T)],
        Admit: (value, op) => op.Catch(() => Fin.Succ<object>(value: clone((T)value))),
        Detach: value => clone((T)value),
        Same: (left, right) => same((T)left, (T)right),
        Mint: (target, key, value, op) => op.Catch(() => op.Confirm(success: set(target, key.Value, clone((T)value)))));

    private static Slot Copies<T>(
        Func<T, T> clone,
        Func<T, T, bool> same,
        Func<ArchivableDictionary, string, Seq<T>, bool> set) where T : class => new(
        Shape: typeof(T[]),
        Keys: [typeof(T[]), typeof(Seq<T>)],
        Admit: (value, op) => op.Catch(() => Fin.Succ<object>(value: (value is T[] host ? toSeq(host) : (Seq<T>)value).Map(clone))),
        Detach: value => ((Seq<T>)value).Map(clone),
        Same: (left, right) =>
        {
            Seq<T> first = (Seq<T>)left;
            Seq<T> second = (Seq<T>)right;
            return first.Count == second.Count && first.Zip(second).ForAll(pair => same(pair.First, pair.Second));
        },
        Mint: (target, key, value, op) => op.Catch(() => op.Confirm(success: set(target, key.Value, ((Seq<T>)value).Map(clone)))));

    private static readonly Slot EnumRow = new(
        Shape: typeof(System.Enum),
        Keys: [],
        Admit: static (value, op) => value is System.Enum boxed
            && boxed.ToString() is { Length: > 0 } name
            && !char.IsDigit(name[0])
            && name[0] != '-'
                ? Fin.Succ<object>(value: new Entry(boxed.GetType(), name))
                : Fin.Fail<object>(error: op.InvalidInput()),
        Detach: static value => value,
        Same: static (left, right) => (Entry)left == (Entry)right,
        Mint: static (target, key, value, op) => value is Entry stored
            ? EnumMint(target, nameof(ArchivableDictionary.SetEnumValue), key.Value, (stored.EnumType, stored.Name), op)
            : Fin.Fail<Unit>(error: op.InvalidInput()));

    private static readonly Slot MapRow = new(
        Shape: typeof(ArchiveMap),
        Keys: [typeof(ArchiveMap)],
        Admit: static (value, op) => value switch
        {
            ArchivableDictionary native => ArchiveMap.Detach(native, op).Map(static map => (object)map),
            ArchiveMap detached => Fin.Succ<object>(value: detached),
            _ => Fin.Fail<object>(error: op.InvalidInput()),
        },
        Detach: static value => value,
        Same: static (left, right) => ((ArchiveMap)left).SameContent((ArchiveMap)right),
        Mint: static (target, key, value, op) => ((ArchiveMap)value)
            .Mint(op)
            .Bind(native => op.Catch(() => op.Confirm(success: target.Set(key.Value, native)))));

    private static readonly Slot GeometryRow = Copy<GeometryBase>(
        static value => value.Duplicate(),
        GeometryBase.GeometryEquals,
        static (t, k, v) => t.Set(k, v));
    private static readonly Slot GeometrySeqRow = Copies<GeometryBase>(
        static value => value.Duplicate(),
        GeometryBase.GeometryEquals,
        static (t, k, v) => t.Set(k, v));
    private static readonly Slot ObjRefRow = Copy<ObjRef>(
        static value => new ObjRef(value),
        static (left, right) => left.ObjectId == right.ObjectId && left.GeometryComponentIndex == right.GeometryComponentIndex,
        static (t, k, v) => t.Set(k, v));
    private static readonly Slot ObjRefSeqRow = Copies<ObjRef>(
        static value => new ObjRef(value),
        static (left, right) => left.ObjectId == right.ObjectId && left.GeometryComponentIndex == right.GeometryComponentIndex,
        static (t, k, v) => t.Set(k, v));
    private static readonly Slot MeshingRow = Copy<MeshingParameters>(
        static value => new MeshingParameters(value),
        static (left, right) => left.Equals(right),
        static (t, k, v) => t.Set(k, v));

    private static readonly FrozenDictionary<Type, Slot> Registry = Seq(
            Scalar<bool>(static (t, k, v) => t.Set(k, v)),
            Scalar<byte>(static (t, k, v) => t.Set(k, v)),
            Scalar<sbyte>(static (t, k, v) => t.Set(k, v)),
            Scalar<short>(static (t, k, v) => t.Set(k, v)),
            Scalar<ushort>(static (t, k, v) => t.Set(k, v)),
            Scalar<int>(static (t, k, v) => t.Set(k, v)),
            Scalar<uint>(static (t, k, v) => t.Set(k, v)),
            Scalar<long>(static (t, k, v) => t.Set(k, v)),
            Scalar<float>(static (t, k, v) => t.Set(k, v)),
            Scalar<double>(static (t, k, v) => t.Set(k, v)),
            Scalar<Guid>(static (t, k, v) => t.Set(k, v)),
            Scalar<string>(static (t, k, v) => t.Set(k, v)),
            Scalar<Color>(static (t, k, v) => t.Set(k, v)),
            Scalar<Point>(static (t, k, v) => t.Set(k, v)),
            Scalar<PointF>(static (t, k, v) => t.Set(k, v)),
            Scalar<Rectangle>(static (t, k, v) => t.Set(k, v)),
            Scalar<RectangleF>(static (t, k, v) => t.Set(k, v)),
            Scalar<Size>(static (t, k, v) => t.Set(k, v)),
            Scalar<SizeF>(static (t, k, v) => t.Set(k, v)),
            Scalar<Interval>(static (t, k, v) => t.Set(k, v)),
            Scalar<Point2d>(static (t, k, v) => t.Set(k, v)),
            Scalar<Point3d>(static (t, k, v) => t.Set(k, v)),
            Scalar<Point4d>(static (t, k, v) => t.Set(k, v)),
            Scalar<Vector2d>(static (t, k, v) => t.Set(k, v)),
            Scalar<Vector3d>(static (t, k, v) => t.Set(k, v)),
            Scalar<BoundingBox>(static (t, k, v) => t.Set(k, v)),
            Scalar<Ray3d>(static (t, k, v) => t.Set(k, v)),
            Scalar<Transform>(static (t, k, v) => t.Set(k, v)),
            Scalar<Plane>(static (t, k, v) => t.Set(k, v)),
            Scalar<Line>(static (t, k, v) => t.Set(k, v)),
            Scalar<Point3f>(static (t, k, v) => t.Set(k, v)),
            Scalar<Vector3f>(static (t, k, v) => t.Set(k, v)),
            Rows<bool>(static (t, k, v) => t.Set(k, v)),
            Rows<byte>(static (t, k, v) => t.Set(k, v)),
            Rows<sbyte>(static (t, k, v) => t.Set(k, v)),
            Rows<short>(static (t, k, v) => t.Set(k, v)),
            Rows<int>(static (t, k, v) => t.Set(k, v)),
            Rows<float>(static (t, k, v) => t.Set(k, v)),
            Rows<double>(static (t, k, v) => t.Set(k, v)),
            Rows<Guid>(static (t, k, v) => t.Set(k, v)),
            Rows<string>(static (t, k, v) => t.Set(k, v)),
            Copy<Font>(static value => (Font)value.Clone(), static (left, right) => left.Equals(right), static (t, k, v) => t.Set(k, v)),
            Scalar<char>(),
            Scalar<DateTime>(),
            Scalar<Option<Color>>(),
            Scalar<HashMap<string, string>>(),
            MapRow,
            GeometryRow,
            GeometrySeqRow,
            ObjRefRow,
            ObjRefSeqRow,
            MeshingRow)
        .Bind(static slot => slot.Keys.Map(key => KeyValuePair.Create(key, slot)))
        .ToFrozenDictionary(static row => row.Key, static row => row.Value);
}

// ───────────────────────────── MAP ALGEBRA ──────────────────────────────

[Union]
public abstract partial record ArchiveChange
{
    public sealed record AddedCase(ArchiveKey Key, ArchiveValue Current) : ArchiveChange;
    public sealed record ChangedCase(ArchiveKey Key, ArchiveValue Prior, ArchiveValue Current) : ArchiveChange;
    public sealed record RemovedCase(ArchiveKey Key, ArchiveValue Prior) : ArchiveChange;
}

[SmartEnum<string>]
public sealed partial class ArchiveMerge
{
    public static readonly ArchiveMerge KeepCurrent = new("keep-current", static (current, _, _) => Fin.Succ(value: current));
    public static readonly ArchiveMerge TakeIncoming = new("take-incoming", static (_, incoming, _) => Fin.Succ(value: incoming));
    public static readonly ArchiveMerge RejectConflict = new(
        "reject-conflict",
        static (current, incoming, op) => current.Same(incoming)
            ? Fin.Succ(value: current)
            : Fin.Fail<ArchiveValue>(error: op.InvalidResult(detail: "Archive merge conflict.")));

    [UseDelegateFromConstructor]
    internal partial Fin<ArchiveValue> Resolve(ArchiveValue current, ArchiveValue incoming, Op op);
}

public sealed record ArchiveMap
{
    private static readonly StringComparer KeyOrder = StringComparer.Ordinal;

    private ArchiveMap(
        int version,
        ArchiveName name,
        HashMap<ArchiveKey, ArchiveValue> entries,
        uint observedChangeSerial) =>
        (Version, Name, Entries, ObservedChangeSerial) = (version, name, entries, observedChangeSerial);

    public int Version { get; private init; }
    public ArchiveName Name { get; private init; }
    public HashMap<ArchiveKey, ArchiveValue> Entries { get; private init; }
    public uint ObservedChangeSerial { get; private init; }

    public static Fin<ArchiveMap> Of(
        int version,
        ArchiveName name,
        HashMap<ArchiveKey, ArchiveValue> entries,
        uint observedChangeSerial,
        Op? key = null)
    {
        Op op = key.OrDefault();
        return from admittedName in op.AcceptValidated<ArchiveName>(name.Value)
               from admittedEntries in entries
                   .Map(row => (from admittedKey in op.AcceptValidated<ArchiveKey>(row.Key.Value)
                                from admittedValue in Optional(row.Value).ToFin(Fail: op.InvalidInput())
                                from archiveValue in admittedValue.AdmitArchive(op)
                                select (Key: admittedKey, Value: archiveValue)).ToValidation())
                   .Traverse(static row => row)
                   .As()
                   .ToFin()
               select new ArchiveMap(
                   version,
                   admittedName,
                   admittedEntries.Fold(
                       HashMap<ArchiveKey, ArchiveValue>(),
                       static (map, row) => map.Add(row.Key, row.Value)),
                   observedChangeSerial);
    }

    public static Fin<ArchiveMap> Detach(ArchivableDictionary source, Op? key = null)
    {
        Op op = key.OrDefault();
        return from native in op.Catch(() => toSeq(source.Keys)
                   .Map(entry => source.TryGetValue(entry, out object? value)
                       ? Fin.Succ(value: (Key: entry, Value: value))
                       : Fin.Fail<(string Key, object? Value)>(
                           error: op.InvalidResult(detail: $"Archive key '{entry}' disappeared during capture.")))
                   .Traverse(static row => row)
                   .Map(rows => (
                       Version: source.Version,
                       Name: source.Name,
                       Serial: source.ChangeSerialNumber,
                       Rows: rows)))
               from name in op.AcceptValidated<ArchiveName>(native.Name)
               from normalized in native.Rows
                   .Map(entry => op.AcceptValidated<ArchiveKey>(entry.Key)
                       .Map(archiveKey => (Raw: entry.Key, Key: archiveKey, Source: entry.Value)))
                   .Traverse(static row => row)
               let collisions = normalized
                   .Fold(
                       HashMap<ArchiveKey, Seq<string>>(),
                       static (groups, row) => groups.Find(row.Key).Match(
                           Some: keys => groups.SetItem(row.Key, keys.Add(row.Raw)),
                           None: () => groups.Add(row.Key, Seq(row.Raw))))
                   .Choose(static row => row.Value.Count > 1 ? Some((row.Key, row.Value)) : None)
                   .OrderBy(static collision => collision.Key.Value, KeyOrder)
                   .Map(static collision => (
                       collision.Key,
                       Keys: collision.Value.OrderBy(static raw => raw, KeyOrder).ToSeq()))
                   .ToSeq()
               from _unique in collisions.IsEmpty
                   ? Fin.Succ(unit)
                   : Fin.Fail<Unit>(new Fault.InvalidValue(
                       Label: nameof(ArchiveKey),
                       Requirement: string.Join(
                           "; ",
                           collisions.Map(static collision =>
                               $"{collision.Key.Value} <= [{string.Join(", ", collision.Keys)}]")),
                       Key: Some(op)))
               from rows in normalized
                   .Map(entry => ArchiveValue.Capture(entry.Source, op)
                       .Map(captured => (entry.Key, Captured: captured)))
                   .Traverse(static row => row)
               from detached in Of(
                   native.Version,
                   name,
                   rows.Fold(HashMap<ArchiveKey, ArchiveValue>(), static (map, row) => map.Add(row.Key, row.Captured)),
                   native.Serial,
                   op)
               select detached;
    }

    public Option<ArchiveValue> Find(ArchiveKey key) => Entries.Find(key);

    public Fin<ArchiveMap> Put(ArchiveKey key, ArchiveValue value, Op? operation = null)
    {
        Op op = operation.OrDefault();
        return from admittedKey in op.AcceptValidated<ArchiveKey>(key.Value)
               from admittedValue in Optional(value).ToFin(Fail: op.InvalidInput())
               from archiveValue in admittedValue.AdmitArchive(op)
               select this with { Entries = Entries.AddOrUpdate(admittedKey, archiveValue) };
    }

    public Fin<ArchiveMap> Remove(ArchiveKey key, Op? operation = null) =>
        operation.OrDefault().AcceptValidated<ArchiveKey>(key.Value)
            .Map(admitted => this with { Entries = Entries.Remove(admitted) });

    public Fin<ArchiveMap> Merge(ArchiveMap incoming, ArchiveMerge policy, Op? key = null)
    {
        Op op = key.OrDefault();
        return AdmitSchema(incoming, op).Bind(_ => incoming.Entries
                .Fold(
                    Fin.Succ(value: Entries),
                    (state, row) => state.Bind(entries => entries.Find(row.Key).Match(
                        Some: current => policy.Resolve(current, row.Value, op).Map(resolved => entries.SetItem(row.Key, resolved)),
                        None: () => Fin.Succ(value: entries.Add(row.Key, row.Value)))))
                .Map(entries => this with { Entries = entries }));
    }

    public Fin<Seq<ArchiveChange>> Diff(ArchiveMap current, Op? key = null)
    {
        Op op = key.OrDefault();
        return AdmitSchema(current, op).Map(_ => Entries.Keys.Union(current.Entries.Keys).OrderBy(static item => item.Value, KeyOrder)
                .Choose(item => (Entries.Find(item), current.Entries.Find(item)) switch
                {
                    ({ IsSome: false }, { IsSome: true } next) =>
                        Some<ArchiveChange>(new ArchiveChange.AddedCase(item, next.Value)),
                    ({ IsSome: true } prior, { IsSome: false }) =>
                        Some<ArchiveChange>(new ArchiveChange.RemovedCase(item, prior.Value)),
                    ({ IsSome: true } prior, { IsSome: true } next) when !prior.Value.Same(next.Value) =>
                        Some<ArchiveChange>(new ArchiveChange.ChangedCase(item, prior.Value, next.Value)),
                    _ => None,
                })
                .ToSeq());
    }

    internal bool SameContent(ArchiveMap other) =>
        Version == other.Version
        && Name == other.Name
        && Entries.Count == other.Entries.Count
        && Entries.Fold(
            true,
            (same, row) => same && other.Entries.Find(row.Key).Exists(value => row.Value.Same(value)));

    private Fin<Unit> AdmitSchema(ArchiveMap other, Op op) =>
        Version == other.Version && Name == other.Name
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(error: op.InvalidInput());

    public Fin<ArchivableDictionary> Mint(Op? key = null)
    {
        Op op = key.OrDefault();
        ArchivableDictionary target = new(Version, Name.Value);
        return WriteTo(target, op).Map(_ => target);
    }

    internal Fin<Unit> WriteTo(ArchivableDictionary target, Op op) =>
        Entries.OrderBy(static row => row.Key.Value, KeyOrder)
            .Map(row => row.Value.Write(target, row.Key, op))
            .Traverse(static write => write)
            .Map(static _ => unit);

    public Fin<ArchiveMap> WithEnum<T>(ArchiveKey key, T value, Op? op = null) where T : struct, System.Enum =>
        ArchiveValue.Enum(value, op.OrDefault()).Bind(enumeration => Put(key, enumeration, op));
}
```

## [02]-[LIFECYCLE]

`ArchiveMap.Of` closes construction and admits every archive-capable key/value pair. `ArchiveMap.Detach` captures the native header and every `TryGetValue` result inside one `Op.Catch`, rejects the complete normalized-key collision set before payload folding, freezes reference values through the owning slot's copy law, and records `ChangeSerialNumber` as observation evidence only. `ArchiveMap.Merge` and `ArchiveMap.Diff` admit identical names and versions before comparing entries. Slot-owned content equality keeps copied geometry, object references, fonts, sequences, and nested maps stable while exposing host `Remove` and `Clear` despite an unchanged native serial.

`ArchiveMap.Mint` creates one `ArchivableDictionary(Version, Name)` and traverses all slot writes on `Fin`. Nested dictionaries recurse through the same currency; geometry, `ObjRef`, `MeshingParameters`, arrays, and fonts copy on both crossings through the slot's one `Detach` law. `ArchiveMerge` rows resolve on the rail, so a `RejectConflict` collision is a typed fault, never a thrown exception inside a fold.

## [03]-[SEAMS]

`SessionSource.Configured` consumes only `ArchiveMap.Mint`. `ArchiveIo` and `SnapshotCodec` exchange only `ArchiveMap`; neither surface receives a live `ArchivableDictionary` or a mutable payload. `SettingKind` consumes this carrier for every `PersistentSettings` payload — its rows lift and lower through `ArchiveValue.Of`/`Project` and share `EnumMint` — so the folder carries exactly one typed-value vocabulary across both KV boundaries.

Enum values admitted through `ArchiveValue.Of` or `ArchiveMap.WithEnum<T>` retain their enum identity and mint through `ArchivableDictionary.SetEnumValue<T>` via the shared reflection seam. Values detached from a native dictionary remain text because Rhino stores enum names as ordinary strings and exposes no readable enum discriminant.
