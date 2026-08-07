# [RASM_RHINO_PERSISTENCE_SETTINGS]

`SettingKind` owns the complete `PersistentSettings` value matrix, including explicit/default asymmetry, as one keyed behavior vocabulary whose rows carry the probe, write, default, capture, and host-projection delegates over the shared `ArchiveValue` carrier (dictionary.md). `SettingOperation` closes reads, writes, defaults, metadata, validators, change state, and saved-tree projection behind `Settings.Commit`.

## [01]-[VALUE_AND_KIND]

`ArchiveValue` carries every payload; `SettingKind` rows are the only site naming a host `TryGet*`/`Set*` member, and each kind adds one complete row whose delegate and type columns drive every boundary projection. `Shape` is the carrier payload type `For` matches, while `HostType` is the host runtime type `Accepts` matches — the two diverge exactly where the detached form differs from the host form (`TextList`, `TextMap`, `OptionalColor`). Rows without a default column refuse with a typed unsupported fault; the enum row rides the shared `EnumMint` reflection seam.

Rename tolerance is an EXPLICIT-READ capability only: every `TryGet*` carries a third `IEnumerable<string> legacyKeyList` parameter, so the `probe` column takes the roster and the whole row vocabulary widens with it. `TryGetDefault` publishes no such overload and `TryGetEnumValue<T>` hard-passes `null` to the same resolver, so `probePreset` stays two-argument and the enum row discards the roster its `Read` column receives — the asymmetry is the host's, and a roster threaded into either would be an unread argument.

The enum row reads through its own `Read` column like every other row: it resolves the concrete type from the host's seated `RuntimeType` and mints one closed reader per enum type behind the shared reflection seam. No second operation case, no caller-supplied type argument, and no parallel static spell the same read.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Drawing;
using System.Reflection;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

[SmartEnum<string>]
public sealed partial class SettingDefaultMode {
    public static readonly SettingDefaultMode None = new("none");
    public static readonly SettingDefaultMode WriteOnly = new("write-only");
    public static readonly SettingDefaultMode ReadWrite = new("read-write");
}

[SmartEnum<string>]
public sealed partial class SettingKind {
    public static readonly SettingKind Guid = Of<Guid>(
        key: "guid",
        defaults: SettingDefaultMode.WriteOnly,
        probe: static (node, key, legacy) => node.TryGetGuid(key, out Guid value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetGuid(key, value),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Bool = Of<bool>(
        key: "bool",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetBool(key, out bool value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetBool(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out bool value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Byte = Of<byte>(
        key: "byte",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetByte(key, out byte value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetByte(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out byte value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Integer = Of<int>(
        key: "integer",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetInteger(key, out int value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetInteger(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out int value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind UnsignedInteger = Of<uint>(
        key: "unsigned-integer",
        defaults: SettingDefaultMode.None,
        probe: static (node, key, legacy) => node.TryGetUnsignedInteger(key, out uint value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetUnsignedInteger(key, value));
    public static readonly SettingKind Double = Of<double>(
        key: "double",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetDouble(key, out double value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetDouble(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out double value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Char = Of<char>(
        key: "char",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetChar(key, out char value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetChar(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out char value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Text = Of<string>(
        key: "text",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetString(key, out string value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetString(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out string value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    // Host splice sentinel: a list ELEMENT equal to `PersistentSettings.StringListRootKey` splices the all-users
    // list at its position on read, so a list carrying it round-trips to a DIFFERENT list by host design — a
    // `Same` inequality across that round trip is the splice expanding, never settings drift.
    public static readonly SettingKind TextList = Of<Seq<string>>(
        key: "text-list",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetStringList(key, out string[] value, legacy) ? Some(toSeq(value)) : None,
        put: static (node, key, value) => node.SetStringList(key, value.ToArray()),
        probePreset: static (node, key) => node.TryGetDefault(key, out string[] value) ? Some(toSeq(value)) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value.ToArray()),
        capture: static (source, op) => source switch {
            string[] rows => ArchiveValue.Of(toSeq(rows), op),
            Seq<string> sequence => ArchiveValue.Of(sequence, op),
            _ => Fin.Fail<ArchiveValue>(error: op.InvalidInput()),
        },
        host: static (value, op) => value.Project<Seq<string>>(op).Map(static sequence => (object?)sequence.ToArray()),
        shape: typeof(string[]),
        hostType: typeof(string[]));
    public static readonly SettingKind TextMap = Of<HashMap<string, string>>(
        key: "text-map",
        defaults: SettingDefaultMode.WriteOnly,
        probe: static (node, key, legacy) => node.TryGetStringDictionary(key, out KeyValuePair<string, string>[] value, legacy)
            ? Some(value.ToHashMap())
            : None,
        put: static (node, key, value) => node.SetStringDictionary(key, TextMapRows(value)),
        putPreset: static (node, key, value) => node.SetDefault(key, TextMapRows(value)),
        capture: static (source, op) => source switch {
            KeyValuePair<string, string>[] rows => ArchiveValue.Of(rows.ToHashMap(), op),
            HashMap<string, string> map => ArchiveValue.Of(map, op),
            _ => Fin.Fail<ArchiveValue>(error: op.InvalidInput()),
        },
        host: static (value, op) => value.Project<HashMap<string, string>>(op)
            .Map(static map => (object?)TextMapRows(map)),
        hostType: typeof(KeyValuePair<string, string>[]));
    public static readonly SettingKind Date = Of<DateTime>(
        key: "date",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetDate(key, out DateTime value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetDate(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out DateTime value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Color = Of<Color>(
        key: "color",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetColor(key, out Color value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetColor(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Color value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind OptionalColor = Of<Option<Color>>(
        key: "optional-color",
        defaults: SettingDefaultMode.WriteOnly,
        probe: static (node, key, legacy) => node.TryGetColor(key, out Color? value, legacy) ? Some(Optional(value)) : None,
        put: static (node, key, value) => node.SetColor(key, value.Match<Color?>(Some: static color => color, None: static () => null)),
        putPreset: static (node, key, value) => node.SetDefault(key, value.Match<Color?>(Some: static color => color, None: static () => null)),
        capture: static (source, op) => source switch {
            null => ArchiveValue.Of(Option<Color>.None, op),
            Color color => ArchiveValue.Of(Some(color), op),
            Option<Color> optional => ArchiveValue.Of(optional, op),
            _ => Fin.Fail<ArchiveValue>(error: op.InvalidInput()),
        },
        host: static (value, op) => value.Project<Option<Color>>(op)
            .Map(optional => optional.Match<object?>(Some: static color => color, None: static () => null)),
        hostType: typeof(Color?));
    public static readonly SettingKind Point = Of<Point>(
        key: "point",
        defaults: SettingDefaultMode.WriteOnly,
        probe: static (node, key, legacy) => node.TryGetPoint(key, out Point value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetPoint(key, value),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Point3d = Of<Point3d>(
        key: "point3d",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetPoint3d(key, out Point3d value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetPoint3d(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Point3d value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Size = Of<Size>(
        key: "size",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetSize(key, out Size value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetSize(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Size value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Rectangle = Of<Rectangle>(
        key: "rectangle",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key, legacy) => node.TryGetRectangle(key, out Rectangle value, legacy) ? Some(value) : None,
        put: static (node, key, value) => node.SetRectangle(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Rectangle value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Enum = new(
        key: "enum",
        defaults: SettingDefaultMode.None,
        shape: typeof(System.Enum),
        hostType: typeof(System.Enum),
        read: static (node, key, _, op) => ReadEnum(node, key, op),
        write: static (node, key, value, op) => value.EnumEntry
            .ToFin(Fail: op.InvalidInput())
            .Bind(entry => ArchiveValue.EnumMint(node, nameof(PersistentSettings.SetEnumValue), key.Value, entry, op)),
        readDefault: static (node, key, op) => Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
            geometryType: typeof(System.Enum), outputType: typeof(PersistentSettings))),
        writeDefault: static (node, key, value, op) => Fin.Fail<Unit>(error: op.Unsupported(
            geometryType: typeof(System.Enum), outputType: typeof(PersistentSettings))),
        capture: static (source, op) => source is null
            ? Fin.Fail<ArchiveValue>(error: op.InvalidInput())
            : ArchiveValue.Enum(source, op),
        host: static (value, op) => value.EnumEntry
            .ToFin(Fail: op.InvalidInput())
            .Bind(entry => op.Catch(() => Fin.Succ<object?>(value: System.Enum.Parse(entry.EnumType, entry.Name, ignoreCase: true)))));

    public SettingDefaultMode Defaults { get; }

    public Type Shape { get; }

    public Type HostType { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Option<ArchiveValue>> Read(PersistentSettings node, SettingKey key, Seq<SettingKey> legacy, Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Write(PersistentSettings node, SettingKey key, ArchiveValue value, Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<Option<ArchiveValue>> ReadDefault(PersistentSettings node, SettingKey key, Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> WriteDefault(PersistentSettings node, SettingKey key, ArchiveValue value, Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<ArchiveValue> Capture(object? source, Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<object?> Host(ArchiveValue value, Op op);

    internal bool Accepts(Type type) => type == HostType || (this == Enum && type.IsEnum);

    internal bool Accepts(Type type, ArchiveValue value) => this == Enum
        ? value.EnumEntry.Match(
            Some: entry => entry.EnumType == type,
            None: static () => false)
        : type == HostType;

    internal static Fin<SettingKind> For(ArchiveValue value, Op op) =>
        value.EnumEntry.IsSome
            ? Fin.Succ(value: Enum)
            : Items.Find(kind => kind.Shape == value.Shape)
                .ToFin(Fail: op.Unsupported(geometryType: value.Shape, outputType: typeof(SettingKind)));

    internal static Fin<SettingKind> For(Type type, Op op) =>
        Items.Find(kind => kind.Accepts(type))
            .ToFin(Fail: op.Unsupported(geometryType: type, outputType: typeof(SettingKind)));

    private static KeyValuePair<string, string>[] TextMapRows(HashMap<string, string> map) => map
        .Map(static row => KeyValuePair.Create(row.Key, row.Value))
        .OrderBy(static row => row.Key, StringComparer.Ordinal)
        .ToArray();

    // ONE enum read exists, and it is this row's own `Read` column. The concrete enum type is the SEATED type the host
    // already records — `TryGetSettingType` answers `SettingValue.RuntimeType`, which `SetEnumValue<T>` writes as
    // `typeof(T)` — so no caller carries a parallel type argument and no second operation case spells the same read. An
    // unseated key answers absent, exactly as every other row's `TryGet*` miss does.
    private static Fin<Option<ArchiveValue>> ReadEnum(PersistentSettings source, SettingKey key, Op op) =>
        op.Catch(() => Fin.Succ(value: source.TryGetSettingType(key.Value, out Type seated)
                ? Optional(seated)
                : Option<Type>.None))
            .Bind(seated => seated.Match(
                Some: type => type.IsEnum
                    ? EnumReader(type, op).Bind(read => op.Catch(() => read(source, key.Value, op)))
                    : Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
                        geometryType: type, outputType: typeof(System.Enum))),
                None: static () => Fin.Succ(value: Option<ArchiveValue>.None)));

    // The open handle resolves once at type init and each enum type's closed reader is minted once and held: the
    // per-call `GetMethod` plus `MakeGenericMethod` walk was repeat reflection on the hottest read on the page. A lost
    // mint race keeps the seated reader, so every caller of one enum type shares one delegate.
    private static readonly Option<MethodInfo> EnumReaderTemplate = Optional(typeof(SettingKind).GetMethod(
        nameof(ReadEnumTyped),
        BindingFlags.NonPublic | BindingFlags.Static));

    private static readonly Atom<HashMap<Type, Func<PersistentSettings, string, Op, Fin<Option<ArchiveValue>>>>> EnumReaders =
        Atom(HashMap<Type, Func<PersistentSettings, string, Op, Fin<Option<ArchiveValue>>>>());

    private static Fin<Func<PersistentSettings, string, Op, Fin<Option<ArchiveValue>>>> EnumReader(Type enumType, Op op) =>
        EnumReaders.Value.Find(enumType).Match(
            Some: static held => Fin.Succ(value: held),
            None: () => EnumReaderTemplate
                .ToFin(Fail: op.MissingContext())
                .Bind(open => op.Catch(() => Fin.Succ(value: open
                    .MakeGenericMethod(enumType)
                    .CreateDelegate<Func<PersistentSettings, string, Op, Fin<Option<ArchiveValue>>>>())))
                .Map(minted => EnumReaders
                    .Swap(held => held.ContainsKey(enumType) ? held : held.Add(enumType, minted))
                    .Find(enumType)
                    .IfNone(minted)));

    private static Fin<Option<ArchiveValue>> ReadEnumTyped<T>(PersistentSettings source, string key, Op op)
        where T : struct, IConvertible =>
        source.TryGetEnumValue(key, out T value)
            ? ArchiveValue.Enum(value, op).Map(Some)
            : Fin.Succ(value: Option<ArchiveValue>.None);

    private static SettingKind Of<T>(
        string key,
        SettingDefaultMode defaults,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<PersistentSettings, string, Option<T>>? probePreset = null,
        Action<PersistentSettings, string, T>? putPreset = null,
        Func<object?, Op, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Op, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull =>
        new(
            key,
            defaults,
            shape: shape ?? typeof(T),
            hostType: hostType ?? typeof(T),
            read: (node, settingKey, legacy, op) => op.Catch(() => probe(node, settingKey.Value, legacy.Map(static row => row.Value)).Match(
                Some: value => ArchiveValue.Of(value, op).Map(Some),
                None: () => Fin.Succ(value: Option<ArchiveValue>.None))),
            write: (node, settingKey, value, op) => value.Project<T>(op)
                .Bind(typed => op.Catch(() => put(node, settingKey.Value, typed))),
            readDefault: probePreset is null
                ? (node, settingKey, op) => Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
                    geometryType: typeof(T), outputType: typeof(PersistentSettings)))
                : (node, settingKey, op) => op.Catch(() => probePreset(node, settingKey.Value).Match(
                    Some: value => ArchiveValue.Of(value, op).Map(Some),
                    None: () => Fin.Succ(value: Option<ArchiveValue>.None))),
            writeDefault: putPreset is null
                ? (node, settingKey, value, op) => Fin.Fail<Unit>(error: op.Unsupported(
                    geometryType: typeof(T), outputType: typeof(PersistentSettings)))
                : (node, settingKey, value, op) => value.Project<T>(op)
                    .Bind(typed => op.Catch(() => putPreset(node, settingKey.Value, typed))),
            capture: capture ?? ((source, op) => source is T typed
                ? ArchiveValue.Of(typed, op)
                : Fin.Fail<ArchiveValue>(error: op.InvalidInput())),
            host: host ?? ((value, op) => value.Project<T>(op).Map(static typed => (object?)typed)));
}
```

## [02]-[REQUEST_ALGEBRA]

`SettingPath` carries root and child identity once. `Route` derives missing-child creation from the active operation, so reads never mutate the tree as an accidental consequence. A read carrying a legacy roster is the one deliberate exception: the host renames the resolved roster key in place, which is the read's purpose, and `ValueCase.Adopted` publishes the rename rather than letting it pass silently.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rhino;

namespace Rasm.Rhino.Persistence;

[ValueObject<string>]
public readonly partial struct SettingKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError("Setting key is empty.");
            return;
        }

        value = value.Trim();
        validationError = null;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsRoot {
    private SettingsRoot() { }

    public sealed record ApplicationCase : SettingsRoot;
    public sealed record PlugInCase(Guid PlugInId) : SettingsRoot;
}

public sealed record SettingPath(SettingsRoot Root, Seq<SettingKey> Children);

public interface ISettingGuard {
    SettingKind Kind { get; }
    Type HostType { get; }
    Fin<ArchiveValue> Validate(ArchiveValue current, ArchiveValue proposed);
    void Report(Error error);
}

[SmartEnum<string>]
public sealed partial class SettingsVisibility {
    public static readonly SettingsVisibility Visible = new("visible", false);
    public static readonly SettingsVisibility Hidden = new("hidden", true);
    internal bool IsHidden { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingOperation {
    private SettingOperation() { }

    // `Legacy` is an ORDERED rename-precedence roster the host walks only when the current key is absent; the first
    // roster key it resolves is REMOVED and its value re-seated under the current key, so a legacy read migrates.
    public sealed record ReadCase(SettingPath Path, SettingKey Key, SettingKind Kind, Seq<SettingKey> Legacy) : SettingOperation;
    public sealed record PutCase(SettingPath Path, SettingKey Key, ArchiveValue Value) : SettingOperation;
    public sealed record DeleteCase(SettingPath Path, SettingKey Key) : SettingOperation;
    public sealed record ReadDefaultCase(SettingPath Path, SettingKey Key, SettingKind Kind) : SettingOperation;
    public sealed record PutDefaultCase(SettingPath Path, SettingKey Key, ArchiveValue Value) : SettingOperation;
    public sealed record MetadataCase(SettingPath Path, SettingKey Key) : SettingOperation;
    public sealed record HideCase(SettingPath Path, SettingKey Key) : SettingOperation;
    public sealed record GuardCase(SettingPath Path, SettingKey Key, ISettingGuard Guard) : SettingOperation;
    public sealed record ChangedCase(SettingPath Path, Option<SettingPath> CompareWith) : SettingOperation;
    public sealed record ClearChangedCase(SettingPath Path) : SettingOperation;
    public sealed record DeleteChildCase(SettingPath Path, SettingKey Child) : SettingOperation;
    public sealed record NodeVisibilityCase(SettingPath Path, SettingsVisibility Visibility) : SettingOperation;
    public sealed record TreeCase(SettingPath Path) : SettingOperation;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingObservation {
    private SettingObservation() { }

    public sealed record ObservedCase(Option<ArchiveValue> Value) : SettingObservation;
    public sealed record UnobservableCase(SettingKind Kind) : SettingObservation;
    public sealed record FaultedCase(SettingKind Kind, Error Fault) : SettingObservation;
}

public sealed record SettingMutationReceipt(
    SettingPath Path,
    SettingKey Key,
    SettingObservation Prior,
    SettingObservation Current,
    bool Changed);

public sealed record SettingMetadata(
    SettingKey Key,
    Type RuntimeType,
    bool ReadOnly,
    bool Hidden);

public sealed record SettingsTree(
    SettingPath Path,
    Seq<SettingMetadata> Values,
    Seq<SettingsTree> Children,
    bool HiddenFromUserInterface,
    bool ContainsChangedValues);

// Host truth, decompile-proven: `RegisterSettingsValidator<T>` is one assignment onto a private per-node map
// (`m_settings_validators[key] = validator`), and `PersistentSettings` publishes no unregister, no clear, and no
// null-accepting overload — `GetValidator<T>` is the only other door to that map. A seated guard therefore holds for
// the node's process lifetime exactly as `SnapshotParticipant.Registered` does, so the answer publishes the seat
// instead of a silent void and a caller can hold what it can never hand back.
public sealed record SettingGuardSeat(SettingPath Path, SettingKey Key, SettingKind Kind);

public sealed record SettingNodeReceipt(
    SettingPath Path,
    Option<SettingKey> Child,
    bool PriorHidden,
    bool CurrentHidden,
    bool ChildDeleted);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SavedSettingsRoot {
    private SavedSettingsRoot() { }

    public sealed record PlugInCase : SavedSettingsRoot;
    public sealed record CommandCase(string EnglishCommandName) : SavedSettingsRoot;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingAnswer {
    private SettingAnswer() { }

    // `Adopted` names the legacy key the host renamed away during this read; every other read answers `None`.
    public sealed record ValueCase(Option<ArchiveValue> Value, Option<SettingKey> Adopted) : SettingAnswer;
    public sealed record MutationCase(SettingMutationReceipt Receipt) : SettingAnswer;
    public sealed record MetadataCase(SettingMetadata Metadata) : SettingAnswer;
    public sealed record ChangedCase(bool Changed) : SettingAnswer;
    public sealed record GuardCase(SettingGuardSeat Seat) : SettingAnswer;
    public sealed record NodeCase(SettingNodeReceipt Receipt) : SettingAnswer;
    public sealed record TreeCase(SettingsTree Tree) : SettingAnswer;
}
```

## [03]-[INTERPRETER]

`Settings.Commit` resolves exactly one node and dispatches the operation exhaustively. Mutation receipts read explicit values without invoking defaulted getters; write-only defaults report `UnobservableCase` instead of inventing evidence.

`SettingOperation` derives path and creation policy once before resolution. Persistent-settings writes, validator callbacks, tree mutation, and saved-event adaptation form the platform-forced statement seam; generated dispatch keeps value, operation, and saved-root families exhaustive around it, and every host crossing rides `Op.Catch` onto typed faults.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Reflection;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

public static class Settings {
    public static Fin<SettingAnswer> Commit(SettingOperation operation, Op? key = null) {
        Op op = key.OrDefault();
        return from request in op.Need(operation)
               from active in Admit(request, op)
               let route = Route(active)
               from node in Resolve(route.Path, route.Creates, op)
               from answer in Execute(node, active, op)
               select answer;
    }

    public static Fin<Subscription> Observe(
        PlugIn plugIn,
        SavedSettingsRoot source,
        SettingPath path,
        Action<Fin<SettingsTree>> sink,
        Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(plugIn).ToFin(Fail: op.MissingContext())
               from root in op.Need(source).Bind(value => Admit(value, op))
               from location in Admit(path, op)
               from receiver in op.Need(sink)
               let handler = new EventHandler<PersistentSettingsSavedEventArgs>((_, args) => _ = op.Catch(() => {
                   PersistentSettings node = root.Switch<PersistentSettingsSavedEventArgs, PersistentSettings>(
                       state: args,
                       plugInCase: static (state, _) => state.PlugInSettings,
                       commandCase: static (state, command) => state.CommandSettings(command.EnglishCommandName));
                   receiver(Snapshot(node, location, op));
                   return Fin.Succ(unit);
               }))
               from subscription in Subscription.Attach(
                   subscribe: callback => owner.SettingsSaved += callback,
                   unsubscribe: callback => owner.SettingsSaved -= callback,
                   handler: handler)
               select subscription;
    }

    private static Fin<SettingOperation> Admit(SettingOperation operation, Op op) => operation.Switch<Op, Fin<SettingOperation>>(
        state: op,
        readCase: static (op, read) => op.Need(read.Kind)
            // The enum row is legacy-blind by host construction, so a roster carried on an enum read would ride into a
            // walk the host never makes and publish an `Adopted` rename that never happened.
            .Bind(kind => guard(kind != SettingKind.Enum || read.Legacy.IsEmpty, op.InvalidInput()).ToFin().Map(_ => kind))
            .Bind(kind => Admit(read.Legacy, read.Key, op).Map(legacy => (Kind: kind, Legacy: legacy)))
            .Bind(state => At(
                read.Path,
                read.Key,
                state,
                static (path, key, admitted) => new SettingOperation.ReadCase(path, key, admitted.Kind, admitted.Legacy),
                op)),
        putCase: static (op, put) => op.Need(put.Value)
            .Bind(value => At(
                put.Path,
                put.Key,
                value,
                static (path, key, admitted) => new SettingOperation.PutCase(path, key, admitted),
                op)),
        deleteCase: static (op, delete) => At(
            delete.Path,
            delete.Key,
            unit,
            static (path, key, _) => new SettingOperation.DeleteCase(path, key),
            op),
        readDefaultCase: static (op, read) => op.Need(read.Kind)
            .Bind(kind => At(
                read.Path,
                read.Key,
                kind,
                static (path, key, admitted) => new SettingOperation.ReadDefaultCase(path, key, admitted),
                op)),
        putDefaultCase: static (op, put) => op.Need(put.Value)
            .Bind(value => At(
                put.Path,
                put.Key,
                value,
                static (path, key, admitted) => new SettingOperation.PutDefaultCase(path, key, admitted),
                op)),
        metadataCase: static (op, metadata) => At(
            metadata.Path,
            metadata.Key,
            unit,
            static (path, key, _) => new SettingOperation.MetadataCase(path, key),
            op),
        hideCase: static (op, hide) => At(
            hide.Path,
            hide.Key,
            unit,
            static (path, key, _) => new SettingOperation.HideCase(path, key),
            op),
        guardCase: static (op, request) => Admit(request.Guard, op)
            .Bind(admitted => At(
                request.Path,
                request.Key,
                admitted,
                static (path, key, guard) => new SettingOperation.GuardCase(path, key, guard),
                op)),
        changedCase: static (op, changed) => changed.CompareWith.Match(
                Some: path => Admit(path, op).Map(Some),
                None: static () => Fin.Succ(value: Option<SettingPath>.None))
            .Bind(compare => At(
                changed.Path,
                compare,
                static (path, admitted) => new SettingOperation.ChangedCase(path, admitted),
                op)),
        clearChangedCase: static (op, clear) => At(
            clear.Path,
            unit,
            static (path, _) => new SettingOperation.ClearChangedCase(path),
            op),
        deleteChildCase: static (op, delete) => At(
            delete.Path,
            delete.Child,
            unit,
            static (path, child, _) => new SettingOperation.DeleteChildCase(path, child),
            op),
        nodeVisibilityCase: static (op, visibility) => op.Need(visibility.Visibility)
            .Bind(admitted => At(
                visibility.Path,
                admitted,
                static (path, value) => new SettingOperation.NodeVisibilityCase(path, value),
                op)),
        treeCase: static (op, tree) => At(
            tree.Path,
            unit,
            static (path, _) => new SettingOperation.TreeCase(path),
            op));

    private static Fin<SettingPath> Admit(SettingPath path, Op op) =>
        from source in op.Need(path)
        from root in op.Need(source.Root)
            .Bind(value => value.Switch<Op, Fin<SettingsRoot>>(
                state: op,
                applicationCase: static (_, _) => Fin.Succ<SettingsRoot>(new SettingsRoot.ApplicationCase()),
                plugInCase: static (op, plugIn) => guard(plugIn.PlugInId != Guid.Empty, op.InvalidInput())
                    .ToFin()
                    .Map<SettingsRoot>(_ => new SettingsRoot.PlugInCase(plugIn.PlugInId))))
        from children in source.Children
            .Map(child => op.AcceptValidated<SettingKey>(child.Value))
            .Traverse(static value => value)
        select new SettingPath(root, children);

    // The host stops at the FIRST roster key it resolves, so a repeat and a self-reference name a rank that can
    // never win: both refuse at admission rather than riding into a walk whose outcome they cannot change.
    private static Fin<Seq<SettingKey>> Admit(Seq<SettingKey> legacy, SettingKey key, Op op) =>
        from rows in legacy
            .Map(row => op.AcceptValidated<SettingKey>(row.Value))
            .Traverse(static value => value)
            .As()
        from _distinct in guard(rows.Distinct().Count() == rows.Count, op.InvalidInput()).ToFin()
        from _self in guard(!rows.Exists(row => row == key), op.InvalidInput()).ToFin()
        select rows;

    private static Fin<SavedSettingsRoot> Admit(SavedSettingsRoot source, Op op) => source.Switch<Op, Fin<SavedSettingsRoot>>(
        state: op,
        plugInCase: static (_, _) => Fin.Succ<SavedSettingsRoot>(new SavedSettingsRoot.PlugInCase()),
        commandCase: static (op, command) => op.AcceptText(value: command.EnglishCommandName)
            .Map<SavedSettingsRoot>(static name => new SavedSettingsRoot.CommandCase(name)));

    private static Fin<ISettingGuard> Admit(ISettingGuard? source, Op op) =>
        op.Need(source).Bind(value => op.Catch(() =>
            from kind in op.Need(value.Kind)
            from hostType in op.Need(value.HostType)
            from _shape in guard(kind.Accepts(hostType), op.InvalidInput()).ToFin()
            select value));

    private static Fin<SettingOperation> At<T>(
        SettingPath path,
        SettingKey key,
        T state,
        Func<SettingPath, SettingKey, T, SettingOperation> mint,
        Op op) =>
        from location in Admit(path, op)
        from admittedKey in op.AcceptValidated<SettingKey>(key.Value)
        select mint(location, admittedKey, state);

    private static Fin<SettingOperation> At<T>(
        SettingPath path,
        T state,
        Func<SettingPath, T, SettingOperation> mint,
        Op op) => Admit(path, op).Map(location => mint(location, state));

    private static Fin<SettingAnswer> Execute(PersistentSettings node, SettingOperation operation, Op op) =>
        operation.Switch<(PersistentSettings Node, Op Op), Fin<SettingAnswer>>(
            state: (node, op),
            readCase: static (s, read) => from adopted in Adopted(s.Node, read.Key, read.Legacy, s.Op)
                                          from value in read.Kind.Read(s.Node, read.Key, read.Legacy, s.Op)
                                          select (SettingAnswer)new SettingAnswer.ValueCase(value, adopted),
            putCase: static (s, put) => AdmitTarget(s.Node, put.Key, put.Value, s.Op).Bind(kind => Mutate(
                put.Path,
                put.Key,
                kind,
                SettingDefaultMode.ReadWrite,
                read: () => kind.Read(s.Node, put.Key, Seq<SettingKey>.Empty, s.Op),
                write: () => kind.Write(s.Node, put.Key, put.Value, s.Op))),
            deleteCase: static (s, delete) => Delete(s.Node, delete, s.Op),
            readDefaultCase: static (s, read) => read.Kind.ReadDefault(s.Node, read.Key, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(value, None)),
            putDefaultCase: static (s, put) => AdmitTarget(s.Node, put.Key, put.Value, s.Op).Bind(kind => Mutate(
                put.Path,
                put.Key,
                kind,
                kind.Defaults,
                read: () => kind.ReadDefault(s.Node, put.Key, s.Op),
                write: () => kind.WriteDefault(s.Node, put.Key, put.Value, s.Op))),
            metadataCase: static (s, metadata) => Metadata(s.Node, metadata.Key, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.MetadataCase(value)),
            hideCase: static (s, hide) => s.Op.Catch(() => s.Node.HideSettingFromUserInterface(hide.Key.Value))
                .Bind(_ => Metadata(s.Node, hide.Key, s.Op))
                .Map<SettingAnswer>(static value => new SettingAnswer.MetadataCase(value)),
            guardCase: static (s, guard) => Register(s.Node, guard, s.Op),
            changedCase: static (s, changed) => Changed(s.Node, changed, s.Op),
            clearChangedCase: static (s, _) => s.Op.Catch(() => s.Node.ClearChangedFlag())
                .Map<SettingAnswer>(static _ => new SettingAnswer.ChangedCase(false)),
            deleteChildCase: static (s, delete) => DeleteChild(s.Node, delete, s.Op),
            nodeVisibilityCase: static (s, visibility) => NodeVisibility(s.Node, visibility, s.Op),
            treeCase: static (s, tree) => Snapshot(s.Node, tree.Path, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.TreeCase(value)));

    private static Fin<PersistentSettings> Resolve(SettingPath path, bool create, Op op) =>
        path.Root.Switch<Op, Fin<PersistentSettings>>(
            state: op,
            applicationCase: static (op, _) => op.Catch(() => Fin.Succ(value: PersistentSettings.RhinoAppSettings)),
            plugInCase: static (op, plugIn) => guard(plugIn.PlugInId != Guid.Empty, op.InvalidInput()).ToFin()
                .Bind(_ => op.Catch(() => Fin.Succ(value: PersistentSettings.FromPlugInId(plugIn.PlugInId)))))
        .Bind(root => path.Children.Fold(
            Fin.Succ(value: root),
            (state, child) => state.Bind(parent => op.Catch(() =>
                parent.TryGetChild(child.Value, out PersistentSettings found) ? Fin.Succ(value: found)
                : create ? Fin.Succ(value: parent.AddChild(child.Value))
                : Fin.Fail<PersistentSettings>(error: op.MissingContext())))));

    private static (SettingPath Path, bool Creates) Route(SettingOperation operation) => operation.Switch<
        (SettingPath Path, bool Creates)>(
        readCase: static value => (value.Path, false),
        putCase: static value => (value.Path, true),
        deleteCase: static value => (value.Path, false),
        readDefaultCase: static value => (value.Path, false),
        putDefaultCase: static value => (value.Path, true),
        metadataCase: static value => (value.Path, false),
        hideCase: static value => (value.Path, true),
        guardCase: static value => (value.Path, true),
        changedCase: static value => (value.Path, false),
        clearChangedCase: static value => (value.Path, false),
        deleteChildCase: static value => (value.Path, false),
        nodeVisibilityCase: static value => (value.Path, false),
        treeCase: static value => (value.Path, false));

    private static Fin<SettingAnswer> Mutate(
        SettingPath path,
        SettingKey key,
        SettingKind kind,
        SettingDefaultMode mode,
        Func<Fin<Option<ArchiveValue>>> read,
        Func<Fin<Unit>> write) => mode == SettingDefaultMode.ReadWrite
        ? Observe(path, key, kind, read, write)
        : write().Map(_ => (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutationReceipt(
            path,
            key,
            new SettingObservation.UnobservableCase(kind),
            new SettingObservation.UnobservableCase(kind),
            true)));

    private static Fin<SettingAnswer> Observe(
        SettingPath path,
        SettingKey key,
        SettingKind kind,
        Func<Fin<Option<ArchiveValue>>> read,
        Func<Fin<Unit>> write) =>
        from prior in read()
        from _ in write()
        select read().Match(
            Succ: current => (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutationReceipt(
                path,
                key,
                new SettingObservation.ObservedCase(prior),
                new SettingObservation.ObservedCase(current),
                !Same(prior, current))),
            Fail: fault => new SettingAnswer.MutationCase(new SettingMutationReceipt(
                path,
                key,
                new SettingObservation.ObservedCase(prior),
                new SettingObservation.FaultedCase(kind, fault),
                true)));

    // The host resolves the current key FIRST and reaches the roster only on a miss, so the adopted key is derived
    // by the same order before the read runs: a seated current key adopts nothing, otherwise the first seated
    // roster key is the one the read is about to rename away. Deriving it after the read is impossible — the
    // migration has already erased the evidence.
    private static Fin<Option<SettingKey>> Adopted(PersistentSettings node, SettingKey key, Seq<SettingKey> legacy, Op op) =>
        legacy.IsEmpty
            ? Fin.Succ(value: Option<SettingKey>.None)
            : op.Catch(() => Fin.Succ(value: Seated(node, key)
                ? Option<SettingKey>.None
                : legacy.Find(row => Seated(node, row))));

    private static bool Seated(PersistentSettings node, SettingKey key) => node.TryGetSettingType(key.Value, out Type _);

    private static Fin<SettingKind> AdmitTarget(
        PersistentSettings node,
        SettingKey key,
        ArchiveValue value,
        Op op) =>
        from kind in SettingKind.For(value, op)
        from existing in op.Catch(() => Fin.Succ(value: node.TryGetSettingType(key.Value, out Type found)
            ? Some(found)
            : Option<Type>.None))
        from _compatible in existing.Match(
            Some: found => guard(kind.Accepts(found, value), op.InvalidInput()).ToFin(),
            None: () => Fin.Succ(unit))
        select kind;

    private static bool Same(Option<ArchiveValue> left, Option<ArchiveValue> right) => (left, right) switch {
        ({ IsSome: false }, { IsSome: false }) => true,
        ({ IsSome: true } prior, { IsSome: true } current) => prior.Value.Same(current.Value),
        _ => false,
    };

    private static Fin<SettingAnswer> Delete(PersistentSettings node, SettingOperation.DeleteCase request, Op op) =>
        from type in op.Catch(() => Fin.Succ(value: node.TryGetSettingType(request.Key.Value, out Type found)
            ? Some(found)
            : Option<Type>.None))
        from prior in type.Match(
            Some: found => SettingKind.For(found, op)
                .Bind(kind => kind.Read(node, request.Key, Seq<SettingKey>.Empty, op)),
            None: () => Fin.Succ(value: Option<ArchiveValue>.None))
        from _ in op.Catch(() => node.DeleteItem(request.Key.Value))
        from _absent in op.Catch(() => guard(
            !node.TryGetSettingType(request.Key.Value, out Type _),
            op.InvalidResult()).ToFin())
        select (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutationReceipt(
            request.Path,
            request.Key,
            new SettingObservation.ObservedCase(prior),
            new SettingObservation.ObservedCase(None),
            prior.IsSome));

    private static Fin<SettingAnswer> DeleteChild(PersistentSettings node, SettingOperation.DeleteChildCase request, Op op) =>
        from before in op.Catch(() => Fin.Succ(value: node.HiddenFromUserInterface))
        from _present in op.Catch(() => guard(
            node.TryGetChild(request.Child.Value, out PersistentSettings _),
            op.MissingContext()).ToFin())
        from _delete in op.Catch(() => node.DeleteChild(request.Child.Value))
        from _absent in op.Catch(() => guard(
            !node.TryGetChild(request.Child.Value, out PersistentSettings _),
            op.InvalidResult(detail: $"Settings child '{request.Child.Value}' survived deletion.")).ToFin())
        from after in op.Catch(() => Fin.Succ(value: node.HiddenFromUserInterface))
        select (SettingAnswer)new SettingAnswer.NodeCase(new SettingNodeReceipt(
            request.Path,
            Some(request.Child),
            before,
            after,
            true));

    private static Fin<SettingAnswer> NodeVisibility(
        PersistentSettings node,
        SettingOperation.NodeVisibilityCase request,
        Op op) =>
        from before in op.Catch(() => Fin.Succ(value: node.HiddenFromUserInterface))
        from _write in op.Catch(() => node.HiddenFromUserInterface = request.Visibility.IsHidden)
        from after in op.Catch(() => Fin.Succ(value: node.HiddenFromUserInterface))
        from _proof in guard(after == request.Visibility.IsHidden, op.InvalidResult(detail: "Settings node visibility postcondition failed.")).ToFin()
        select (SettingAnswer)new SettingAnswer.NodeCase(new SettingNodeReceipt(
            request.Path,
            None,
            before,
            after,
            false));

    private static Fin<SettingMetadata> Metadata(PersistentSettings node, SettingKey key, Op op) =>
        op.Catch(() => Fin.Succ(value: new SettingMetadata(
            key,
            node.GetSettingType(key.Value),
            node.GetSettingIsReadOnly(key.Value),
            node.GetSettingIsHiddenFromUserInterface(key.Value))));

    // The open handle resolves once at type init and each host type's closed writer is minted once and held; the typed
    // delegate also retires the `is Fin<Unit>` unbox the reflective invoke forced on every registration.
    private static readonly Option<MethodInfo> ValidatorTemplate = Optional(typeof(Settings).GetMethod(
        nameof(RegisterTyped),
        BindingFlags.NonPublic | BindingFlags.Static));

    private static readonly Atom<HashMap<Type, Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>>> ValidatorWriters =
        Atom(HashMap<Type, Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>>());

    private static Fin<Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>> ValidatorWriter(Type hostType, Op op) =>
        ValidatorWriters.Value.Find(hostType).Match(
            Some: static held => Fin.Succ(value: held),
            None: () => ValidatorTemplate
                .ToFin(Fail: op.MissingContext())
                .Bind(open => op.Catch(() => Fin.Succ(value: open
                    .MakeGenericMethod(hostType)
                    .CreateDelegate<Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>>())))
                .Map(minted => ValidatorWriters
                    .Swap(held => held.ContainsKey(hostType) ? held : held.Add(hostType, minted))
                    .Find(hostType)
                    .IfNone(minted)));

    private static Fin<SettingAnswer> Register(PersistentSettings node, SettingOperation.GuardCase request, Op op) =>
        from write in ValidatorWriter(request.Guard.HostType, op)
        from _wired in write(node, request.Key.Value, request.Guard, op)
        select (SettingAnswer)new SettingAnswer.GuardCase(
            new SettingGuardSeat(request.Path, request.Key, request.Guard.Kind));

    private static Fin<Unit> RegisterTyped<T>(PersistentSettings node, string key, ISettingGuard guard, Op op) =>
        op.Catch(() => {
            // Host truth: `GetValidator<T>` THROWS `InvalidCastException` when `T` disagrees with the registered
            // specialization, so the collision arrives as an exception on one shape and as a non-null probe on the other;
            // both are one refusal, and letting the cast escape to the outer catch reports it as a generic fault instead.
            bool taken;
            try {
                taken = node.GetValidator<T>(key) is not null;
            } catch (InvalidCastException) {
                taken = true;
            }

            if (taken) {
                return Fin.Fail<Unit>(error: op.InvalidResult(detail: $"Settings validator '{key}' is already registered."));
            }

            node.RegisterSettingsValidator<T>(key, (_, args) => _ = op.Catch(() =>
                from current in guard.Kind.Capture(args.CurrentValue, op)
                from proposed in guard.Kind.Capture(args.NewValue, op)
                from accepted in guard.Validate(current, proposed)
                from host in guard.Kind.Host(accepted, op)
                from _assigned in Assign(args, host, op)
                select unit)
                .BindFail(error => op.Catch(() => {
                    args.Cancel = true;
                    guard.Report(error);
                    return Fin.Succ(value: unit);
                })));
            return Fin.Succ(value: unit);
        });

    private static Fin<Unit> Assign<T>(PersistentSettingsEventArgs<T> args, object? host, Op op) => op.Catch(() => {
        if (host is T typed) {
            args.CurrentValue = typed;
            return Fin.Succ(value: unit);
        }

        if (host is not null && Nullable.GetUnderlyingType(typeof(T)) == host.GetType()) {
            args.CurrentValue = (T)host;
            return Fin.Succ(value: unit);
        }

        if (host is null && default(T) is null) {
            args.CurrentValue = default;
            return Fin.Succ(value: unit);
        }

        return Fin.Fail<Unit>(error: op.InvalidResult(
            detail: $"Settings validator host projection '{host?.GetType()}' cannot assign '{typeof(T)}'."));
    });

    private static Fin<SettingAnswer> Changed(PersistentSettings node, SettingOperation.ChangedCase request, Op op) =>
        request.CompareWith.Match(
            Some: path => Resolve(path, create: false, op)
                .Bind(other => op.Catch(() => Fin.Succ(value: node.ContainsModifiedValues(other)))),
            None: () => op.Catch(() => Fin.Succ(value: node.ContainsChangedValues())))
        .Map<SettingAnswer>(static changed => new SettingAnswer.ChangedCase(changed));

    private static Fin<SettingsTree> Snapshot(PersistentSettings node, SettingPath path, Op op) =>
        op.Catch(() =>
        from valueKeys in node.Keys
            .Map(key => op.AcceptValidated<SettingKey>(key))
            .Traverse(static value => value)
            .As()
            .Map(static keys => toSeq(keys.OrderBy(static key => key.Value, StringComparer.Ordinal)))
        from values in valueKeys
            .Map(key => Metadata(node, key, op))
            .Traverse(static value => value)
        from childKeys in node.ChildKeys
            .Map(key => op.AcceptValidated<SettingKey>(key))
            .Traverse(static value => value)
            .Map(static keys => toSeq(keys.OrderBy(static key => key.Value, StringComparer.Ordinal)))
        from children in childKeys
            .Map(admitted => from child in op.Catch(() => Fin.Succ(value: node.GetChild(admitted.Value)))
                        from tree in Snapshot(child, path with { Children = path.Children.Add(admitted) }, op)
                        select tree)
            .Traverse(static value => value)
        select new SettingsTree(
            path,
            values,
            children,
            node.HiddenFromUserInterface,
            node.ContainsChangedValues()));
}
```

## [04]-[LIFECYCLE]

`Settings.Commit` follows operation admission → root resolution → child resolution → typed host action → detached answer. `Route` alone derives missing-child creation; every operation outside that policy fails on a missing path with `MissingContext`.

Explicit reads use `TryGet*` and never call mutating defaulted getters, and every read — enum included — enters through the owning `SettingKind` row's `Read` column. `AdmitTarget` compares each payload row with the existing host type, including exact enum identity, before explicit or default writes. One mutation fold owns observable and write-only receipts; failed post-write reads land as `FaultedCase` evidence, and deletion emits absence only after a host re-probe. A registered guard answers a `SettingGuardSeat` because the host publishes no unregister: the seat is held for the node's process lifetime and never handed back.

Both reflection seams — the per-enum-type reader and the per-host-type validator writer — resolve their open handle once at type init and hold each closed delegate in an `Atom`-guarded map, so a lost mint race settles on the seated delegate rather than a second one.

`ArchiveValue` (dictionary.md) is the one payload carrier across this boundary — `SettingKind` rows lift host values through `ArchiveValue.Of`, lower through `Project<T>`, and mint enum payloads through the shared `EnumMint` seam. `SettingsTree` admits and orders value and child keys before recursive projection. `PlugIn.SettingsSaved` observation encloses root projection and sink delivery in one catch frame under the Document subscription owner; this page owns no parallel event lifecycle.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
