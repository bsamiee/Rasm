# [RASM_RHINO_PERSISTENCE_DICTIONARY]

`ArchiveValue` is the folder's ONE typed boxed-host-value carrier: every payload a KV boundary moves — archive scalars, sequences, drawing values, geometry, carriers, enums, and the settings-only shapes — admits through one `ArchiveSlot` row whose columns carry the host-type keys, the boundary reach, the defensive-copy law, and the native write. `ArchiveMap` admits one native dictionary, preserves schema identity, decides equality on content alone, and mints one fresh native dictionary for each egress.

## [01]-[INDEX]

- [02]-[OWNERS]: `ArchiveKey`, `ArchiveName`, `ArchiveReach`, `ArchiveSlot`, `ArchiveValue` — the key vocabulary, the boundary-reach capability, the payload row family, and the one boxed carrier.
- [03]-[MAP_ALGEBRA]: `ArchiveChange`, `ArchiveMerge`, `ArchiveMap` — the diff vocabulary, the conflict policy rows, and the detached dictionary with its detach/mint round trip.

## [02]-[OWNERS]

- Owner: `ArchiveSlot` — one `[SmartEnum<string>]` row per host payload type, carrying the admitted host `Keys`, the boundary `Reach`, and the `Admit`/`Detach`/`Same`/`Mint` delegate columns; `ArchiveValue` — the boxed carrier holding one row beside one admitted payload; `ArchiveReach` — the two-row capability naming which KV boundary a payload may cross; `ArchiveKey`/`ArchiveName` — the admitted string identities.
- Entry: a new payload type is ONE `ArchiveSlot` row. Capture, host write, detached projection, and content equality all read that row, so no consumer enumerates case arms and every boundary is complete by construction.
- Law: the row key is the row's own wire name, never its host type, because a row admits SEVERAL host types — `Rows<T>` answers both `T[]` and `Seq<T>` — so the `Type` index is a MANY-TO-ONE projection off the `Keys` column rather than a generated single-key lookup.
- Law: the `Type` index is accessor-backed. Building the frozen projection inside a static field initializer runs the whole fold under a type initializer whose failure poisons the type for the process life; `Lazy` defers it to the first resolve and every row field stays a plain declaration.
- Law: `ArchiveKey` and `ArchiveName` admit through the kernel `Op.AcceptValidated` string row, the one factory bridge onto the carrier; a folder-local string bridge beside it is the deleted form.
- Law: reach is DATA, not a nullable column — `char`, `DateTime`, `Option<Color>`, and `HashMap<string, string>` hold `Settings` alone because `ArchivableDictionary` publishes no `Set` overload for them, so their `Mint` refuses typed and `AdmitArchive` reads the capability instead of testing a delegate for null.
- Law: enum payloads keep enum identity through one `EnumMint` reflection adapter shared by both host targets; values detached from a native dictionary remain text, because Rhino stores enum names as ordinary strings and exposes no readable enum discriminant.
- Law: copy-slot custody is a TRANSFER at every leg, decompile-proven — the host dictionary stores the reference it is handed (`SetItem` -> `m_items[key] = new DictionaryItem(it, val)`), so the `Admit` clone is the carrier's own stored value for the carrier's lifetime, the `Detach` clone transfers to the caller at `Project<T>` egress, and the `Mint` clone becomes the dictionary's stored entry. No clone is a disposable temp.
- Law: a lost mint race keeps the SEATED handle, so one host/method/enum triple resolves to one closed method for the process and `Cell.Claim` owns the transition rather than a discarded swap.
- Growth: a new payload type is one row; a new boundary is one `ArchiveReach` row with the membership on the rows that reach it; consumers are untouched.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[UseDelegateFromConstructor]`, `[ValueObject<T>]`, `[ValidationError]`, `IDisallowDefaultValue`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `Atom`, `Traverse`); kernel `Domain/results` (`Op`, `Op.Catch`, `Op.Confirm`, `Op.AcceptValidated`, `Cell.Claim`, `Transition`), `Domain/validation` (`ICapability`, `CapabilitySet`); `Persistence/presets` (`PersistenceFault`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[DICTIONARY_VALUE_WRITE]`/`[DICTIONARY_VALUE_READ]`/`[DICTIONARY_LIFECYCLE]` — the `Set` overload roster, `SetEnumValue<T>`, `TryGetValue`, `Keys`, `Version`, `Name`, `ChangeSerialNumber`, `ParentUserData`), RhinoCommon geometry (`api-rhinocommon-geometry.md` — `GeometryBase.Duplicate`, `GeometryBase.GeometryEquals`, `MeshingParameters` copy constructor), RhinoCommon objects (`api-rhinocommon-objects.md` — `ObjRef` copy constructor, `ObjectId`, `GeometryComponentIndex`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Drawing;
using System.Reflection;
using Rasm.Domain;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public readonly partial struct ArchiveKey : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(string.Join(" | ", new object?[] { "Archive key is empty." }))
            : null;
    }
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct ArchiveName : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = null;
    }
}

[SmartEnum<string>]
public sealed partial class ArchiveReach : ICapability<ArchiveReach> {
    public static readonly ArchiveReach Archive = new("archive");
    public static readonly ArchiveReach Settings = new("settings");
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ArchiveSlot {
    public static readonly ArchiveSlot Bool = Scalar<bool>("bool", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Byte = Scalar<byte>("byte", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot SByte = Scalar<sbyte>("sbyte", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Short = Scalar<short>("short", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot UShort = Scalar<ushort>("ushort", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Integer = Scalar<int>("integer", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot UnsignedInteger = Scalar<uint>("unsigned-integer", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Long = Scalar<long>("long", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Float = Scalar<float>("float", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Double = Scalar<double>("double", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Guid = Scalar<Guid>("guid", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Text = Scalar<string>("text", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Color = Scalar<Color>("color", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Point = Scalar<Point>("point", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot PointF = Scalar<PointF>("point-f", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Rectangle = Scalar<Rectangle>("rectangle", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot RectangleF = Scalar<RectangleF>("rectangle-f", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Size = Scalar<Size>("size", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot SizeF = Scalar<SizeF>("size-f", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Interval = Scalar<Interval>("interval", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Point2d = Scalar<Point2d>("point2d", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Point3d = Scalar<Point3d>("point3d", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Point4d = Scalar<Point4d>("point4d", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Vector2d = Scalar<Vector2d>("vector2d", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Vector3d = Scalar<Vector3d>("vector3d", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot BoundingBox = Scalar<BoundingBox>("bounding-box", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Ray3d = Scalar<Ray3d>("ray3d", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Transform = Scalar<Transform>("transform", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Plane = Scalar<Plane>("plane", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Line = Scalar<Line>("line", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Point3f = Scalar<Point3f>("point3f", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Vector3f = Scalar<Vector3f>("vector3f", static (t, k, v) => t.Set(k, v));

    public static readonly ArchiveSlot BoolSeq = Rows<bool>("bool-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot ByteSeq = Rows<byte>("byte-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot SByteSeq = Rows<sbyte>("sbyte-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot ShortSeq = Rows<short>("short-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot IntegerSeq = Rows<int>("integer-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot FloatSeq = Rows<float>("float-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot DoubleSeq = Rows<double>("double-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot GuidSeq = Rows<Guid>("guid-seq", static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot TextSeq = Rows<string>("text-seq", static (t, k, v) => t.Set(k, v));

    public static readonly ArchiveSlot Font = Copy<Font>(
        "font",
        static value => (Font)value.Clone(),
        static (left, right) => left.Equals(right),
        static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Geometry = Copy<GeometryBase>(
        "geometry",
        static value => value.Duplicate(),
        GeometryBase.GeometryEquals,
        static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot GeometrySeq = Copies<GeometryBase>(
        "geometry-seq",
        static value => value.Duplicate(),
        GeometryBase.GeometryEquals,
        static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot ObjectRef = Copy<ObjRef>(
        "obj-ref",
        static value => new ObjRef(value),
        SameRef,
        static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot ObjectRefSeq = Copies<ObjRef>(
        "obj-ref-seq",
        static value => new ObjRef(value),
        SameRef,
        static (t, k, v) => t.Set(k, v));
    public static readonly ArchiveSlot Meshing = Copy<MeshingParameters>(
        "meshing",
        static value => new MeshingParameters(value),
        static (left, right) => left.Equals(right),
        static (t, k, v) => t.Set(k, v));

    public static readonly ArchiveSlot Char = Held<char>("char");
    public static readonly ArchiveSlot Date = Held<DateTime>("date");
    public static readonly ArchiveSlot OptionalColor = Held<Option<Color>>("optional-color");
    public static readonly ArchiveSlot TextMap = Held<HashMap<string, string>>("text-map");

    public static readonly ArchiveSlot Map = new(
        key: "map",
        shape: typeof(ArchiveMap),
        keys: Seq(typeof(ArchiveMap)),
        reach: CapabilitySet<ArchiveReach>.All,
        admit: static (value, op) => value switch {
            ArchivableDictionary native => ArchiveMap.Detach(native).Map(static map => (object)map),
            ArchiveMap detached => Fin.Succ<object>(value: detached),
            _ => Fin.Fail<object>(error: new KernelFault.InvalidInput()),
        },
        detach: static value => value,
        same: static (left, right) => ((ArchiveMap)left).SameContent((ArchiveMap)right),
        mint: static (target, key, value, op) => ((ArchiveMap)value)
            .Mint()
            .Bind(native => Try.lift(() => Admit.Confirm(success: target.Set(key.Value, native))).Run().Bind(static inner => inner)));

    public static readonly ArchiveSlot Enumeration = new(
        key: "enum",
        shape: typeof(System.Enum),
        keys: Seq<Type>(),
        reach: CapabilitySet<ArchiveReach>.All,
        admit: static (value, op) => value is System.Enum boxed
            && boxed.ToString() is { Length: > 0 } name
            && !char.IsDigit(name[0])
            && name[0] != '-'
                ? Fin.Succ<object>(value: new Entry(boxed.GetType(), name))
                : Fin.Fail<object>(error: new KernelFault.InvalidInput()),
        detach: static value => value,
        same: static (left, right) => (Entry)left == (Entry)right,
        mint: static (target, key, value, op) => value is Entry stored
            ? ArchiveValue.EnumMint(target, nameof(ArchivableDictionary.SetEnumValue), key.Value, (stored.EnumType, stored.Name))
            : Fin.Fail<Unit>(error: new KernelFault.InvalidInput()));

    public Type Shape { get; }

    public Seq<Type> Keys { get; }

    public CapabilitySet<ArchiveReach> Reach { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<object> Admit(object value);

    [UseDelegateFromConstructor]
    internal partial object Detach(object value);

    [UseDelegateFromConstructor]
    internal partial bool Same(object left, object right);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Mint(ArchivableDictionary target, ArchiveKey key, object value);

    internal readonly record struct Entry(Type EnumType, string Name);

    internal static Option<ArchiveSlot> Resolve(object source) => source switch {
        System.Enum => Some(Enumeration),
        _ when Index.Value.TryGetValue(source.GetType(), out ArchiveSlot? exact) => Some(exact),
        GeometryBase[] => Some(GeometrySeq),
        ObjRef[] => Some(ObjectRefSeq),
        ArchivableDictionary => Some(Map),
        MeshingParameters => Some(Meshing),
        GeometryBase => Some(Geometry),
        ObjRef => Some(ObjectRef),
        _ => None,
    };

    private static readonly Lazy<FrozenDictionary<Type, ArchiveSlot>> Index = new(static () => toSeq(Items)
        .Bind(static row => row.Keys.Map(key => KeyValuePair.Create(row)))
        .ToFrozenDictionary(static row => row.Key, static row => row.Value));

    private static bool SameRef(ObjRef left, ObjRef right) =>
        left.ObjectId == right.ObjectId && left.GeometryComponentIndex == right.GeometryComponentIndex;

    private static ArchiveSlot Scalar<T>(string key, Func<ArchivableDictionary, string, T, bool> set) where T : notnull => new(shape: typeof(T),
        keys: Seq(typeof(T)),
        reach: CapabilitySet<ArchiveReach>.All,
        admit: static (value, _) => Fin.Succ(value: value),
        detach: static value => value,
        same: static (left, right) => EqualityComparer<T>.Default.Equals((T)left, (T)right),
        mint: (target, key, value, op) => Try.lift(() => Admit.Confirm(success: set(target, key.Value, (T)value))).Run().Bind(static inner => inner));

    private static ArchiveSlot Held<T>(string key) where T : notnull => new(shape: typeof(T),
        keys: Seq(typeof(T)),
        reach: CapabilitySet<ArchiveReach>.Of(ArchiveReach.Settings),
        admit: static (value, _) => Fin.Succ(value: value),
        detach: static value => value,
        same: static (left, right) => EqualityComparer<T>.Default.Equals((T)left, (T)right),
        mint: static (_, _, _, op) => Fin.Fail<Unit>(error: new KernelFault.Unsupported(
            InputType: typeof(T), OutputType: typeof(ArchivableDictionary))));

    private static ArchiveSlot Rows<T>(string key, Func<ArchivableDictionary, string, Seq<T>, bool> set) => new(shape: typeof(T[]),
        keys: Seq(typeof(T[]), typeof(Seq<T>)),
        reach: CapabilitySet<ArchiveReach>.All,
        admit: static (value, _) => Fin.Succ<object>(value: value is T[] host ? toSeq(host) : value),
        detach: static value => value,
        same: static (left, right) => ((Seq<T>)left).SequenceEqual((Seq<T>)right),
        mint: (target, key, value, op) => Try.lift(() => Admit.Confirm(success: set(target, key.Value, (Seq<T>)value))).Run().Bind(static inner => inner));

    private static ArchiveSlot Copy<T>(
        string key,
        Func<T, T> clone,
        Func<T, T, bool> same,
        Func<ArchivableDictionary, string, T, bool> set) where T : class => new(shape: typeof(T),
        keys: Seq(typeof(T)),
        reach: CapabilitySet<ArchiveReach>.All,
        admit: (value, op) => Try.lift(() => Fin.Succ<object>(value: clone((T)value))).Run().Bind(static inner => inner),
        detach: value => clone((T)value),
        same: (left, right) => same((T)left, (T)right),
        mint: (target, key, value, op) => Try.lift(() => Admit.Confirm(success: set(target, key.Value, clone((T)value)))).Run().Bind(static inner => inner));

    private static ArchiveSlot Copies<T>(
        string key,
        Func<T, T> clone,
        Func<T, T, bool> same,
        Func<ArchivableDictionary, string, Seq<T>, bool> set) where T : class => new(shape: typeof(T[]),
        keys: Seq(typeof(T[]), typeof(Seq<T>)),
        reach: CapabilitySet<ArchiveReach>.All,
        admit: (value, op) => Try.lift(() => Fin.Succ<object>(value: (value is T[] host ? toSeq(host) : (Seq<T>)value).Map(clone))).Run().Bind(static inner => inner),
        detach: value => ((Seq<T>)value).Map(clone),
        same: (left, right) => {
            Seq<T> first = (Seq<T>)left;
            Seq<T> second = (Seq<T>)right;
            return first.Count == second.Count && first.Zip(second).ForAll(pair => same(pair.First, pair.Second));
        },
        mint: (target, key, value, op) => Try.lift(() => Admit.Confirm(success: set(target, key.Value, ((Seq<T>)value).Map(clone)))).Run().Bind(static inner => inner));
}

public sealed record ArchiveValue {
    private ArchiveValue(ArchiveSlot row, object payload) => (Row, Payload) = (row, payload);

    private ArchiveSlot Row { get; }

    internal object Payload { get; }

    public Type Shape => Payload is ArchiveSlot.Entry stored ? stored.EnumType : Row.Shape;

    internal Option<(Type EnumType, string Name)> EnumEntry =>
        Payload is ArchiveSlot.Entry stored ? Some((stored.EnumType, stored.Name)) : None;

    internal bool Same(ArchiveValue other) => ReferenceEquals(Row, other.Row) && Row.Same(Payload, other.Payload);

    internal Fin<ArchiveValue> AdmitArchive() => Row.Reach.Admits(ArchiveReach.Archive)
        ? Fin.Succ(value: this)
        : Fin.Fail<ArchiveValue>(error: new KernelFault.Unsupported(InputType: Shape, OutputType: typeof(ArchivableDictionary)));

    public static Fin<ArchiveValue> Of<T>(T source) where T : notnull =>
        Capture(source: source, op: key.OrDefault());

    internal static Fin<ArchiveValue> Capture(object? source) =>
        Optional(source)
            .ToFin(Fail: new KernelFault.InvalidInput())
            .Bind(value => ArchiveSlot.Resolve(source: value)
                .ToFin(Fail: new KernelFault.Unsupported(InputType: value.GetType(), OutputType: typeof(ArchiveValue)))
                .Bind(row => row.Admit(value).Map(payload => new ArchiveValue(row, payload))));

    internal static Fin<ArchiveValue> Enum(object? source) => Optional(source)
        .ToFin(Fail: new KernelFault.InvalidInput())
        .Bind(value => value is System.Enum ? Capture(value) : Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidInput()));

    internal static Fin<Unit> EnumMint(object target, string method, string key, (Type EnumType, string Name) entry) =>
        Minter(new MintKey(target.GetType(), method, entry.EnumType))
            .Bind(closed => Try.lift(() => Admit.Confirm(success: closed.Invoke(
                target,
                [key, System.Enum.Parse(enumType: entry.EnumType, value: entry.Name, ignoreCase: true)]) is not false)).Run().Bind(static inner => inner));

    private readonly record struct MintKey(Type Host, string Method, Type EnumType);

    private static readonly Atom<HashMap<MintKey, MethodInfo>> Minters = Atom(HashMap<MintKey, MethodInfo>());

    private static Fin<MethodInfo> Minter(MintKey row) =>
        Minters.Value.Find(row).Match(
            Some: static held => Fin.Succ(value: held),
            None: () => Try.lift(() => Fin.Succ(value: row.Host
                    .GetMethods()
                    .Single(candidate => candidate.Name == row.Method
                        && candidate.IsGenericMethodDefinition
                        && candidate.GetParameters().Length == 2)
                    .MakeGenericMethod(row.EnumType))).Run().Bind(static inner => inner)
                .Map(minted => Cell.Claim(cell: Minters, key: row, mint: () => minted).Current[row]));

    public Fin<T> Project<T>() {
        return Payload switch {
            ArchiveSlot.Entry stored => typeof(T) == stored.EnumType
                && System.Enum.TryParse(enumType: stored.EnumType, value: stored.Name, ignoreCase: true, result: out object? parsed)
                && parsed is T value
                ? Fin.Succ(value: value)
                : Fin.Fail<T>(error: new KernelFault.Unsupported(InputType: stored.EnumType, OutputType: typeof(T))),
            T typed => Fin.Succ(value: (T)Row.Detach(typed)),
            _ => Fin.Fail<T>(error: new KernelFault.Unsupported(InputType: Row.Shape, OutputType: typeof(T))),
        };
    }

    internal Fin<Unit> Write(ArchivableDictionary target, ArchiveKey key) =>
        AdmitArchive().Bind(_ => Row.Mint(target, Payload));
}
```

## [03]-[MAP_ALGEBRA]

- Owner: `ArchiveMap` — the detached dictionary carrying schema version, name, and admitted entries; `ArchiveChange` — the diff vocabulary the folder README router names; `ArchiveMerge` — the conflict-resolution rows behind one `Resolve` column.
- Entry: `Of` closes construction and admits every archive-capable pair; `Detach` captures one native dictionary; `Mint` answers one fresh native. `SessionSource.Configured` consumes only `Mint`; `ArchiveIo` and `IArchiveCodec` exchange only `ArchiveMap`, so no live `ArchivableDictionary` and no mutable payload crosses either boundary.
- Law: `Detach` captures the native header and every `TryGetValue` result inside ONE `Op.Catch`, rejects the COMPLETE normalized-key collision set before any payload folding, and freezes reference values through the owning row's copy law — a partial collision report names one clash while the caller owns several.
- Law: content decides equality outright. The native change serial is a LOWER BOUND the host leaves unmoved across `Remove` and `Clear`, so it can neither prove nor disprove a difference the fold does not already see.
- Law: `Merge` and `Diff` admit identical names and versions before comparing entries, and an `ArchiveMerge` row resolves on the carrier — a `RejectConflict` collision is a typed fault, never a thrown exception inside a fold.
- Law: `SettingKind` consumes this carrier for every `PersistentSettings` payload — its rows lift and lower through `ArchiveValue.Of`/`Project<T>` and share `EnumMint` — so the folder carries exactly one typed-value vocabulary across both KV boundaries.
- Growth: a new merge policy is one `ArchiveMerge` row; a new diff shape is one `ArchiveChange` case with every reader loudly broken.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<TKey>]`, `[UseDelegateFromConstructor]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `HashMap`, `Traverse`, `Fold`, `Choose`); kernel `Domain/results` (`Op`, `Op.Catch`, `Op.Need`, `Op.AcceptValidated`, `KernelFault.InvalidValue`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[DICTIONARY_LIFECYCLE]` — `ArchivableDictionary(int, string)`, `Keys`, `TryGetValue`, `Version`, `Name`, `ChangeSerialNumber`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rhino.Collections;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveChange {
    private ArchiveChange() { }

    public sealed record AddedCase(ArchiveKey Key, ArchiveValue Current) : ArchiveChange;
    public sealed record ChangedCase(ArchiveKey Key, ArchiveValue Prior, ArchiveValue Current) : ArchiveChange;
    public sealed record RemovedCase(ArchiveKey Key, ArchiveValue Prior) : ArchiveChange;
}

[SmartEnum<string>]
public sealed partial class ArchiveMerge {
    public static readonly ArchiveMerge KeepCurrent = new("keep-current", static (current, _, _) => Fin.Succ(value: current));
    public static readonly ArchiveMerge TakeIncoming = new("take-incoming", static (_, incoming, _) => Fin.Succ(value: incoming));
    public static readonly ArchiveMerge RejectConflict = new(
        "reject-conflict",
        static (current, incoming, op) => current.Same(incoming)
            ? Fin.Succ(value: current)
            : Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidResult(Detail: Some("Archive merge conflict."))));

    [UseDelegateFromConstructor]
    internal partial Fin<ArchiveValue> Resolve(ArchiveValue current, ArchiveValue incoming);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ArchiveMap {
    private static readonly StringComparer KeyOrder = StringComparer.Ordinal;

    private ArchiveMap(int version, ArchiveName name, HashMap<ArchiveKey, ArchiveValue> entries) =>
        (Version, Name, Entries) = (version, name, entries);

    public int Version { get; private init; }
    public ArchiveName Name { get; private init; }
    public HashMap<ArchiveKey, ArchiveValue> Entries { get; private init; }

    public static Fin<ArchiveMap> Of(
        int version,
        ArchiveName name,
        HashMap<ArchiveKey, ArchiveValue> entries) {
        return from admittedName in FactoryBridge.Accept<ArchiveName>(name.Value)
               from admittedEntries in entries
                   .Map(row => (from admittedKey in FactoryBridge.Accept<ArchiveKey>(row.Key.Value)
                                from admittedValue in Admit.Need(row.Value)
                                from archiveValue in admittedValue.AdmitArchive()
                                select (Key: admittedKey, Value: archiveValue)).ToValidation())
                   .Traverse(static row => row)
                   .As()
                   .ToFin()
               select new ArchiveMap(
                   version,
                   admittedName,
                   admittedEntries.Fold(
                       HashMap<ArchiveKey, ArchiveValue>(),
                       static (map, row) => map.Add(row.Key, row.Value)));
    }

    public static Fin<ArchiveMap> Detach(ArchivableDictionary source) {
        return from native in Try.lift(() => toSeq(source.Keys)
                   .Map(entry => source.TryGetValue(entry, out object? value)
                       ? Fin.Succ(value: (Key: entry, Value: value))
                       : Fin.Fail<(string Key, object? Value)>(
                           error: new KernelFault.InvalidResult(Detail: Some($"Archive key '{entry}' disappeared during capture."))))
                   .Traverse(static row => row)
                   .As()
                   .Map(rows => (
                       Version: source.Version,
                       Name: source.Name,
                       Rows: rows))).Run().Bind(static inner => inner)
               from name in FactoryBridge.Accept<ArchiveName>(native.Name)
               from normalized in native.Rows
                   .Map(entry => FactoryBridge.Accept<ArchiveKey>(entry.Key)
                       .Map(archiveKey => (Raw: entry.Key, Key: archiveKey, Source: entry.Value)))
                   .Traverse(static row => row)
               let collisions = toSeq(normalized
                   .Fold(
                       HashMap<ArchiveKey, Seq<string>>(),
                       static (groups, row) => groups.Find(row.Key).Match(
                           Some: keys => groups.SetItem(keys.Add(row.Raw)),
                           None: () => groups.Add(row.Key, Seq(row.Raw))))
                   .AsIterable()
                   .Choose(static row => row.Value.Count > 1 ? Some((row.Key, row.Value)) : None)
                   .OrderBy(static collision => collision.Key.Value, KeyOrder)
                   .Select(static collision => (
                       collision.Key,
                       Keys: toSeq(collision.Value.OrderBy(static raw => raw, KeyOrder)))))
               from _unique in collisions.IsEmpty
                   ? Fin.Succ(unit)
                   : Fin.Fail<Unit>(new KernelFault.InvalidValue(
                       Label: nameof(ArchiveKey),
                       Requirement: string.Join(
                           "; ",
                           collisions.Map(static collision =>
                               $"{collision.Key.Value} <= [{string.Join(", ", collision.Keys)}]"))))
               from rows in normalized
                   .Map(entry => Error.New(entry.Source.Message, entry.Source)
                       .Map(captured => (entry.Key, Captured: captured)))
                   .Traverse(static row => row)
               from detached in Of(
                   native.Version,
                   name,
                   rows.Fold(HashMap<ArchiveKey, ArchiveValue>(), static (map, row) => map.Add(row.Key, row.Captured)))
               select detached;
    }

    public Option<ArchiveValue> Find(ArchiveKey key) => Entries.Find();

    public Fin<ArchiveMap> Put(ArchiveKey key, ArchiveValue value) {
        return from admittedKey in FactoryBridge.Accept<ArchiveKey>(key.Value)
               from admittedValue in Admit.Need(value)
               from archiveValue in admittedValue.AdmitArchive()
               select this with { Entries = Entries.AddOrUpdate(admittedKey, archiveValue) };
    }

    public Fin<ArchiveMap> Remove(ArchiveKey key) =>
        operation.OrDefault().AcceptValidated<ArchiveKey>(key.Value)
            .Map(admitted => this with { Entries = Entries.Remove(admitted) });

    public Fin<ArchiveMap> Merge(ArchiveMap incoming, ArchiveMerge policy) {
        return AdmitSchema(incoming).Bind(_ => incoming.Entries
                .Fold(
                    Fin.Succ(value: Entries),
                    (state, row) => state.Bind(entries => entries.Find(row.Key).Match(
                        Some: current => policy.Resolve(current, row.Value).Map(resolved => entries.SetItem(row.Key, resolved)),
                        None: () => Fin.Succ(value: entries.Add(row.Key, row.Value)))))
                .Map(entries => this with { Entries = entries }));
    }

    public Fin<Seq<ArchiveChange>> Diff(ArchiveMap current) {
        return AdmitSchema(current).Map(_ => toSeq(Entries.Keys.Union(current.Entries.Keys).OrderBy(static item => item.Value, KeyOrder))
                .Choose(item => (Entries.Find(item), current.Entries.Find(item)) switch {
                    ({ IsSome: false }, { IsSome: true, Case: ArchiveValue next }) =>
                        Some<ArchiveChange>(new ArchiveChange.AddedCase(item, next)),
                    ({ IsSome: true, Case: ArchiveValue prior }, { IsSome: false }) =>
                        Some<ArchiveChange>(new ArchiveChange.RemovedCase(item, prior)),
                    ({ IsSome: true, Case: ArchiveValue prior }, { IsSome: true, Case: ArchiveValue next }) when !prior.Same(next) =>
                        Some<ArchiveChange>(new ArchiveChange.ChangedCase(item, prior, next)),
                    _ => None,
                }));
    }

    internal bool SameContent(ArchiveMap other) =>
        Version == other.Version
        && Name == other.Name
        && Entries.Count == other.Entries.Count
        && Entries.Fold(
            true,
            (same, row) => same && other.Entries.Find(row.Key).Exists(value => row.Value.Same(value)));

    private Fin<Unit> AdmitSchema(ArchiveMap other) =>
        Version == other.Version && Name == other.Name
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(error: new KernelFault.InvalidInput());

    public Fin<ArchivableDictionary> Mint() {
        ArchivableDictionary target = new(Version, Name.Value);
        return WriteTo(target).Map(_ => target);
    }

    internal Fin<Unit> WriteTo(ArchivableDictionary target) =>
        toSeq(Entries.AsIterable()
                .OrderBy(static row => row.Key.Value, KeyOrder)
                .Select(row => row.Value.Write(target)))
            .Traverse(static write => write)
            .As()
            .Map(static _ => unit);

    public Fin<ArchiveMap> WithEnum<T>(ArchiveKey key, T value) where T : struct, System.Enum =>
        ArchiveValue.Enum(value).Bind(enumeration => Put(enumeration));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
