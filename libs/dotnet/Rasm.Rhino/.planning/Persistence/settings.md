# [RASM_RHINO_PERSISTENCE_SETTINGS]

`SettingKind` owns the complete `PersistentSettings` value matrix, including explicit/default asymmetry, as one keyed behavior vocabulary whose rows carry the probe, write, default, capture, and host-projection delegates over the shared `ArchiveValue` carrier (dictionary.md). `SettingOperation` closes reads, writes, defaults, clamped reads, metadata, validators, change state, and saved-tree projection behind `SettingStore.Commit`.

## [01]-[INDEX]

- [02]-[VALUE_AND_KIND]: `SettingReach`, `SettingKind` — the layer-reach capability and the one row family naming a host `TryGet*`/`Set*` member.
- [03]-[REQUEST_ALGEBRA]: `SettingKey`, `SettingsRoot`, `SettingPath`, `ChildPolicy`, `ISettingGuard`, `SettingsVisibility`, `SettingTrait`, `SettingDelta`, `ChangeVerdict`, `SaveOrigin`, `IntegerBound`, `SettingOperation`, `SettingObservation`, `SettingMutationReceipt`, `SettingMetadata`, `SettingsTree`, `SettingsSaved`, `SettingGuardSeat`, `SettingNodeReceipt`, `SavedSettingsRoot`, `SettingAnswer` — the addressing, request, and answer vocabularies.
- [04]-[INTERPRETER]: `SettingStore` — node resolution, total dispatch, the two reflection seams, and the guard seat.

## [02]-[VALUE_AND_KIND]

- Owner: `SettingKind` — one row per host value kind, carrying `Shape`, `HostType`, the layer-reach column, and the `Read`/`Write`/`ReadDefault`/`WriteDefault`/`Capture`/`Host` delegates; `SettingReach` — the two-row capability naming which halves of a storage layer the host publishes.
- Entry: three factory arms mint a row and their SIGNATURES demand exactly the delegates each claims — `OfNone` takes no preset column, `OfWriteOnly` demands the preset writer, `OfReadWrite` demands both — so the reach column is DERIVED at the mint and a row whose declared mode and delegate set disagree is unspellable.
- Law: `Shape` is the carrier payload type `For` matches and `HostType` is the host runtime type `Accepts` matches; the two diverge exactly where the detached form differs from the host form — `TextList`, `TextMap`, `OptionalColor`.
- Law: the default layer's reach is the HOST's, catalogue-proven — `SetDefault` covers sixteen kinds and `TryGetDefault` twelve, and every probe-able kind is a writable one, so a readable-but-unwritable default layer is a corner the host never publishes and the three arms are the whole legal roster.
- Law: rename tolerance is an EXPLICIT-READ capability only. Every `TryGet*` carries a third `IEnumerable<string> legacyKeyList` parameter, so the `probe` column takes the roster; `TryGetDefault` publishes no such overload and `TryGetEnumValue<T>` hard-passes `null` to the same resolver, so the preset probe stays two-argument and the enum row discards the roster its `Read` column receives — the asymmetry is the host's, and a roster threaded into either is an unread argument.
- Law: ONE enum read exists and it is the enum row's own `Read` column. The concrete type is the SEATED type the host already records — `TryGetSettingType` answers `SettingValue.RuntimeType`, which `SetEnumValue<T>` writes as `typeof(T)` — so no caller carries a parallel type argument, no second operation case spells the same read, and an unseated key answers absent exactly as every other row's miss does.
- Law: every `bool Try*(out T)` crossing lifts through the kernel `Op.Probe` — absence is ordinary and answers `Option<T>`, so no row re-spells the `? Some : None` ternary and no host miss reaches the rail as a fault.
- Law: `For(Type, Op)` refuses a foreign-written kind through `Unsupported` carrying the HOST TYPE TOKEN, so a value another writer seated under a type this vocabulary does not model names itself in the refusal instead of collapsing to a bare invalid-input.
- Growth: a new host value kind is one row through the arm matching its default reach; a new layer half is one `SettingReach` row.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[UseDelegateFromConstructor]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `HashMap`, `Atom`); kernel `Domain/rails` (`Op`, `Op.Probe`, `Op.Catch`, `Cell.Claim`, `Transition`), `Domain/validation` (`ICapability`, `CapabilitySet`); `Persistence/dictionary` (`ArchiveValue`, `ArchiveValue.Of`/`Project`/`Enum`/`EnumMint`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[SETTINGS_TYPED_READ]`/`[SETTINGS_TYPED_WRITE]`/`[SETTINGS_DEFAULTS]` — the sixteen `TryGet*`/`Set*` pairs with their `legacyKeyList` siblings, `SetDefault`/`TryGetDefault`, `SetEnumValue<T>`/`TryGetEnumValue<T>`, `TryGetSettingType`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Drawing;
using System.Reflection;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ----------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SettingReach : ICapability<SettingReach> {
    public static readonly SettingReach Write = new("write");
    public static readonly SettingReach Read = new("read");
}

// --- [MODELS] ---------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SettingKind {
    public static readonly SettingKind Guid = OfWriteOnly<Guid>(
        key: "guid",
        probe: static (node, key, legacy) => Op.Probe<Guid>((out Guid value) => node.TryGetGuid(key, out value, legacy)),
        put: static (node, key, value) => node.SetGuid(key, value),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Bool = OfReadWrite<bool>(
        key: "bool",
        probe: static (node, key, legacy) => Op.Probe<bool>((out bool value) => node.TryGetBool(key, out value, legacy)),
        put: static (node, key, value) => node.SetBool(key, value),
        probePreset: static (node, key) => Op.Probe<bool>((out bool value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Byte = OfReadWrite<byte>(
        key: "byte",
        probe: static (node, key, legacy) => Op.Probe<byte>((out byte value) => node.TryGetByte(key, out value, legacy)),
        put: static (node, key, value) => node.SetByte(key, value),
        probePreset: static (node, key) => Op.Probe<byte>((out byte value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Integer = OfReadWrite<int>(
        key: "integer",
        probe: static (node, key, legacy) => Op.Probe<int>((out int value) => node.TryGetInteger(key, out value, legacy)),
        put: static (node, key, value) => node.SetInteger(key, value),
        probePreset: static (node, key) => Op.Probe<int>((out int value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind UnsignedInteger = OfNone<uint>(
        key: "unsigned-integer",
        probe: static (node, key, legacy) => Op.Probe<uint>((out uint value) => node.TryGetUnsignedInteger(key, out value, legacy)),
        put: static (node, key, value) => node.SetUnsignedInteger(key, value));
    public static readonly SettingKind Double = OfReadWrite<double>(
        key: "double",
        probe: static (node, key, legacy) => Op.Probe<double>((out double value) => node.TryGetDouble(key, out value, legacy)),
        put: static (node, key, value) => node.SetDouble(key, value),
        probePreset: static (node, key) => Op.Probe<double>((out double value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Char = OfReadWrite<char>(
        key: "char",
        probe: static (node, key, legacy) => Op.Probe<char>((out char value) => node.TryGetChar(key, out value, legacy)),
        put: static (node, key, value) => node.SetChar(key, value),
        probePreset: static (node, key) => Op.Probe<char>((out char value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Text = OfReadWrite<string>(
        key: "text",
        probe: static (node, key, legacy) => Op.Probe<string>((out string value) => node.TryGetString(key, out value, legacy)),
        put: static (node, key, value) => node.SetString(key, value),
        probePreset: static (node, key) => Op.Probe<string>((out string value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind TextList = OfReadWrite<Seq<string>>(
        key: "text-list",
        probe: static (node, key, legacy) => Op
            .Probe<string[]>((out string[] value) => node.TryGetStringList(key, out value, legacy))
            .Map(toSeq),
        put: static (node, key, value) => node.SetStringList(key, value.ToArray()),
        probePreset: static (node, key) => Op
            .Probe<string[]>((out string[] value) => node.TryGetDefault(key, out value))
            .Map(toSeq),
        putPreset: static (node, key, value) => node.SetDefault(key, value.ToArray()),
        capture: static (source, op) => source switch {
            string[] rows => ArchiveValue.Of(toSeq(rows), op),
            Seq<string> sequence => ArchiveValue.Of(sequence, op),
            _ => Fin.Fail<ArchiveValue>(error: op.InvalidInput()),
        },
        host: static (value, op) => value.Project<Seq<string>>(op).Map(static sequence => (object?)sequence.ToArray()),
        shape: typeof(string[]),
        hostType: typeof(string[]));
    public static readonly SettingKind TextMap = OfWriteOnly<HashMap<string, string>>(
        key: "text-map",
        probe: static (node, key, legacy) => Op
            .Probe<KeyValuePair<string, string>[]>((out KeyValuePair<string, string>[] value) =>
                node.TryGetStringDictionary(key, out value, legacy))
            .Map(static rows => rows.ToHashMap()),
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
    public static readonly SettingKind Date = OfReadWrite<DateTime>(
        key: "date",
        probe: static (node, key, legacy) => Op.Probe<DateTime>((out DateTime value) => node.TryGetDate(key, out value, legacy)),
        put: static (node, key, value) => node.SetDate(key, value),
        probePreset: static (node, key) => Op.Probe<DateTime>((out DateTime value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Color = OfReadWrite<Color>(
        key: "color",
        probe: static (node, key, legacy) => Op.Probe<Color>((out Color value) => node.TryGetColor(key, out value, legacy)),
        put: static (node, key, value) => node.SetColor(key, value),
        probePreset: static (node, key) => Op.Probe<Color>((out Color value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind OptionalColor = OfWriteOnly<Option<Color>>(
        key: "optional-color",
        probe: static (node, key, legacy) => Op
            .Probe<Color?>((out Color? value) => node.TryGetColor(key, out value, legacy))
            .Map(Optional),
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
    public static readonly SettingKind Point = OfWriteOnly<Point>(
        key: "point",
        probe: static (node, key, legacy) => Op.Probe<Point>((out Point value) => node.TryGetPoint(key, out value, legacy)),
        put: static (node, key, value) => node.SetPoint(key, value),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Point3d = OfReadWrite<Point3d>(
        key: "point3d",
        probe: static (node, key, legacy) => Op.Probe<Point3d>((out Point3d value) => node.TryGetPoint3d(key, out value, legacy)),
        put: static (node, key, value) => node.SetPoint3d(key, value),
        probePreset: static (node, key) => Op.Probe<Point3d>((out Point3d value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Size = OfReadWrite<Size>(
        key: "size",
        probe: static (node, key, legacy) => Op.Probe<Size>((out Size value) => node.TryGetSize(key, out value, legacy)),
        put: static (node, key, value) => node.SetSize(key, value),
        probePreset: static (node, key) => Op.Probe<Size>((out Size value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Rectangle = OfReadWrite<Rectangle>(
        key: "rectangle",
        probe: static (node, key, legacy) => Op.Probe<Rectangle>((out Rectangle value) => node.TryGetRectangle(key, out value, legacy)),
        put: static (node, key, value) => node.SetRectangle(key, value),
        probePreset: static (node, key) => Op.Probe<Rectangle>((out Rectangle value) => node.TryGetDefault(key, out value)),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Enum = new(
        key: "enum",
        defaults: CapabilitySet<SettingReach>.None,
        shape: typeof(System.Enum),
        hostType: typeof(System.Enum),
        read: static (node, key, _, op) => ReadEnum(node, key, op),
        write: static (node, key, value, op) => value.EnumEntry
            .ToFin(Fail: op.InvalidInput())
            .Bind(entry => ArchiveValue.EnumMint(node, nameof(PersistentSettings.SetEnumValue), key.Value, entry, op)),
        readDefault: static (_, _, op) => Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
            inputType: typeof(System.Enum), outputType: typeof(PersistentSettings))),
        writeDefault: static (_, _, _, op) => Fin.Fail<Unit>(error: op.Unsupported(
            inputType: typeof(System.Enum), outputType: typeof(PersistentSettings))),
        capture: static (source, op) => source is null
            ? Fin.Fail<ArchiveValue>(error: op.InvalidInput())
            : ArchiveValue.Enum(source, op),
        host: static (value, op) => value.EnumEntry
            .ToFin(Fail: op.InvalidInput())
            .Bind(entry => op.Catch(() => Fin.Succ<object?>(value: System.Enum.Parse(entry.EnumType, entry.Name, ignoreCase: true)))));

    // Which halves of the DEFAULT layer this row's host kind publishes; the explicit layer is always both.
    public CapabilitySet<SettingReach> Defaults { get; }

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
        ? value.EnumEntry.Match(Some: entry => entry.EnumType == type, None: static () => false)
        : type == HostType;

    internal static Fin<SettingKind> For(ArchiveValue value, Op op) =>
        value.EnumEntry.IsSome
            ? Fin.Succ(value: Enum)
            : Items.Find(kind => kind.Shape == value.Shape)
                .ToFin(Fail: op.Unsupported(inputType: value.Shape, outputType: typeof(SettingKind)));

    internal static Fin<SettingKind> For(Type type, Op op) =>
        Items.Find(kind => kind.Accepts(type))
            .ToFin(Fail: op.Unsupported(inputType: type, outputType: typeof(SettingKind)));

    private static KeyValuePair<string, string>[] TextMapRows(HashMap<string, string> map) => map
        .Map(static row => KeyValuePair.Create(row.Key, row.Value))
        .OrderBy(static row => row.Key, StringComparer.Ordinal)
        .ToArray();

    private static Fin<Option<ArchiveValue>> ReadEnum(PersistentSettings source, SettingKey key, Op op) =>
        op.Catch(() => Fin.Succ(value: Op.Probe<Type>((out Type value) => source.TryGetSettingType(key.Value, out value))))
            .Bind(seated => seated.Match(
                Some: type => type.IsEnum
                    ? EnumReader(type, op).Bind(read => op.Catch(() => read(source, key.Value, op)))
                    : Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
                        inputType: type, outputType: typeof(System.Enum))),
                None: static () => Fin.Succ(value: Option<ArchiveValue>.None)));

    // The open handle resolves once at type init and each enum type's closed reader is minted once and held: the
    // per-call `GetMethod` plus `MakeGenericMethod` walk was repeat reflection on the hottest read on the page.
    // `Cell.Claim` owns the first-writer-wins transition, so every caller of one enum type shares one delegate.
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
                .Map(minted => Cell.Claim(cell: EnumReaders, key: enumType, mint: () => minted).Current[enumType]));

    private static Fin<Option<ArchiveValue>> ReadEnumTyped<T>(PersistentSettings source, string key, Op op)
        where T : struct, IConvertible =>
        source.TryGetEnumValue(key, out T value)
            ? ArchiveValue.Enum(value, op).Map(Some)
            : Fin.Succ(value: Option<ArchiveValue>.None);

    private static SettingKind OfNone<T>(
        string key,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<object?, Op, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Op, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull => Mint(
        key, CapabilitySet<SettingReach>.None, probe, put, probePreset: null, putPreset: null, capture, host, shape, hostType);

    private static SettingKind OfWriteOnly<T>(
        string key,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Action<PersistentSettings, string, T> putPreset,
        Func<object?, Op, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Op, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull => Mint(
        key, CapabilitySet<SettingReach>.Of(SettingReach.Write), probe, put, probePreset: null, putPreset, capture, host, shape, hostType);

    private static SettingKind OfReadWrite<T>(
        string key,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<PersistentSettings, string, Option<T>> probePreset,
        Action<PersistentSettings, string, T> putPreset,
        Func<object?, Op, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Op, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull => Mint(
        key, CapabilitySet<SettingReach>.All, probe, put, probePreset, putPreset, capture, host, shape, hostType);

    private static SettingKind Mint<T>(
        string key,
        CapabilitySet<SettingReach> defaults,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<PersistentSettings, string, Option<T>>? probePreset,
        Action<PersistentSettings, string, T>? putPreset,
        Func<object?, Op, Fin<ArchiveValue>>? capture,
        Func<ArchiveValue, Op, Fin<object?>>? host,
        Type? shape,
        Type? hostType) where T : notnull =>
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
                ? (_, _, op) => Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
                    inputType: typeof(T), outputType: typeof(PersistentSettings)))
                : (node, settingKey, op) => op.Catch(() => probePreset(node, settingKey.Value).Match(
                    Some: value => ArchiveValue.Of(value, op).Map(Some),
                    None: () => Fin.Succ(value: Option<ArchiveValue>.None))),
            writeDefault: putPreset is null
                ? (_, _, _, op) => Fin.Fail<Unit>(error: op.Unsupported(
                    inputType: typeof(T), outputType: typeof(PersistentSettings)))
                : (node, settingKey, value, op) => value.Project<T>(op)
                    .Bind(typed => op.Catch(() => putPreset(node, settingKey.Value, typed))),
            capture: capture ?? ((source, op) => source is T typed
                ? ArchiveValue.Of(typed, op)
                : Fin.Fail<ArchiveValue>(error: op.InvalidInput())),
            host: host ?? ((value, op) => value.Project<T>(op).Map(static typed => (object?)typed)));
}
```

## [03]-[REQUEST_ALGEBRA]

- Owner: `SettingPath` — root and child chain, the one settings address; `SettingsRoot` — the three addressable node roots; `SettingOperation` — the closed request family carrying its own `Route`; `SettingAnswer` — the closed answer family; `SettingTrait` — the node and value flag vocabulary.
- Entry: an operation carries its own path and child policy on `Route`, so the interpreter reads routing off the request rather than re-deciding it in a parallel fold, and a read can never mutate the tree as an accidental consequence.
- Law: `SettingsRoot.PlugInCase` keys on `PluginKey`, never a raw `Guid` — the branch RULING makes the typed plug-in identity the only spelling, and the value object already refuses the empty key the host throws on.
- Law: the addressable command root is `Rhino.Commands.Command.Settings`, an INSTANCE member, so `SettingsRoot.CommandCase` carries the command itself while `SavedSettingsRoot.CommandCase` keeps its ENGLISH NAME key — the saved-event payload's `CommandSettings(string)` is a name-keyed door and no host member resolves a command node from a name outside it. A plug-in addressing its own command node holds the instance; the observer names it.
- Law: a read carrying a legacy roster is the ONE deliberate mutation on a read path — the host renames the resolved roster key in place, which is the read's purpose — so `ValueCase.Adopted` publishes the rename rather than letting it pass silently.
- Law: `Legacy` is an ORDERED rename-precedence roster the host walks only when the current key is absent, stopping at the FIRST roster key it resolves, so a repeat and a self-reference name a rank that can never win and both refuse at admission.
- Law: the enum row is legacy-blind by host construction, so a roster carried on an enum read rides into a walk the host never makes and publishes an `Adopted` rename that never happened.
- Law: `ClampCase` is a MUTATING read. Every defaulted `Get<Kind>(key, default)` stamps the default layer on a hit and registers the default AND materializes the key on a miss, so the clamped integer reads route with `ChildPolicy.Create` and answer a value the tree now holds. `int` is the only kind carrying clamp overloads, and the case names it.
- Law: every flag on a node or value rides `CapabilitySet<SettingTrait>` and every two-state host answer rides its own `[SmartEnum<bool>]` row, so `ReadOnly`, `Hidden`, and `Changed` are one held set and no receipt carries a bare bool a reader must interpret from position.
- Law: `SettingNodeReceipt` carries the prior and current visibility rows alone — a child-deleted flag beside an `Option<SettingKey> Child` was a second authority over the same fact, true exactly when the child is present.
- Law: `SettingGuardSeat` is published rather than swallowed: `PersistentSettings` publishes no unregister, no clear, and no null-accepting overload, so a seated guard holds for the node's process lifetime and the caller holds what it can never hand back.
- Growth: a new request is one `SettingOperation` case with its `Route` arm and its `Execute` arm; a new flag is one `SettingTrait` row.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<TKey>]`, `[Union]`, `[ValueObject<T>]`, `[ValidationError]`, `IDisallowDefaultValue`); LanguageExt.Core (`Fin`, `Option`, `Seq`); kernel `Domain/rails` (`Op`), `Domain/validation` (`ICapability`, `CapabilitySet`); `Document/events` (`PluginKey`); `Persistence/dictionary` (`ArchiveValue`), `Persistence/presets` (`PersistenceFault`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[SETTINGS_TREE]`/`[SETTINGS_METADATA]` — `FromPlugInId`, `RhinoAppSettings`, `Keys`, `ChildKeys`, `HiddenFromUserInterface`, `GetSettingType`, `GetSettingIsReadOnly`, `GetSettingIsHiddenFromUserInterface`, `StringListRootKey`, `PersistentSettingsSavedEventArgs`), RhinoCommon commands (`api-rhinocommon-commands.md` — `Command.Settings`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ----------------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public readonly partial struct SettingKey : IDisallowDefaultValue {
    // Host splice sentinel: a list ELEMENT equal to this key splices the all-users ProgramData list at its position
    // on read, so a list carrying it round-trips to a DIFFERENT list by host design and a `Same` inequality across
    // that trip is the splice expanding, never settings drift. Accessor-backed, because a static field would read a
    // host static inside a type initializer.
    public static SettingKey ListSplice => Create(PersistentSettings.StringListRootKey);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { "Setting key is empty." }));
            return;
        }

        value = value.Trim();
        validationError = null;
    }
}

// Each row's key IS the host value it lowers to or lifts from, so no member restates the bool the row already is.
[SmartEnum<bool>]
public sealed partial class SettingsVisibility {
    public static readonly SettingsVisibility Visible = new(key: false);
    public static readonly SettingsVisibility Hidden = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class ChildPolicy {
    public static readonly ChildPolicy Require = new(key: false);
    public static readonly ChildPolicy Create = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class ChangeVerdict {
    public static readonly ChangeVerdict Clean = new(key: false);
    public static readonly ChangeVerdict Modified = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class SaveOrigin {
    public static readonly SaveOrigin ForeignRhino = new(key: false);
    public static readonly SaveOrigin ThisRhino = new(key: true);
}

// The three meanings a mutation receipt's change column carries, kept apart: an observed difference, an observed
// identity, and a write whose layer publishes no reader at all.
[SmartEnum<string>]
public sealed partial class SettingDelta {
    public static readonly SettingDelta Unchanged = new("unchanged");
    public static readonly SettingDelta Changed = new("changed");
    public static readonly SettingDelta Unobserved = new("unobserved");
}

[SmartEnum<string>]
public sealed partial class SettingTrait : ICapability<SettingTrait> {
    public static readonly SettingTrait ReadOnly = new("read-only");
    public static readonly SettingTrait Hidden = new("hidden");
    public static readonly SettingTrait Changed = new("changed");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsRoot {
    private SettingsRoot() { }

    public sealed record ApplicationCase : SettingsRoot;
    public sealed record PlugInCase(PluginKey Plugin) : SettingsRoot;
    public sealed record CommandCase(Command Owner) : SettingsRoot;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SavedSettingsRoot {
    private SavedSettingsRoot() { }

    public sealed record PlugInCase : SavedSettingsRoot;
    public sealed record CommandCase(string EnglishCommandName) : SavedSettingsRoot;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IntegerBound {
    private IntegerBound() { }

    public sealed record LowerCase(int Floor) : IntegerBound;
    public sealed record UpperCase(int Ceiling) : IntegerBound;
    public sealed record RangeCase(int Floor, int Ceiling) : IntegerBound;

    internal int Clamped(PersistentSettings node, SettingKey key, int fallback) =>
        Switch<(PersistentSettings Node, SettingKey Key, int Fallback), int>(
            state: (node, key, fallback),
            lowerCase: static (s, row) => s.Node.GetInteger(s.Key.Value, s.Fallback, row.Floor, boundIsLower: true),
            upperCase: static (s, row) => s.Node.GetInteger(s.Key.Value, s.Fallback, row.Ceiling, boundIsLower: false),
            rangeCase: static (s, row) => s.Node.GetInteger(s.Key.Value, s.Fallback, row.Floor, row.Ceiling));
}

// --- [SERVICES] -------------------------------------------------------------------------------
public interface ISettingGuard {
    SettingKind Kind { get; }
    Type HostType { get; }
    Fin<ArchiveValue> Validate(ArchiveValue current, ArchiveValue proposed);
    void Report(Error error);
}

// --- [MODELS] ---------------------------------------------------------------------------------
public sealed record SettingPath(SettingsRoot Root, Seq<SettingKey> Children);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingOperation {
    private SettingOperation() { }

    public sealed record ReadCase(SettingPath Path, SettingKey Key, SettingKind Kind, Seq<SettingKey> Legacy) : SettingOperation;
    public sealed record ClampCase(SettingPath Path, SettingKey Key, int Fallback, IntegerBound Bound) : SettingOperation;
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

    // The request owns its own routing: the child policy is a property of what the operation DOES, so a new case
    // states it once beside its path instead of in a second parallel fold the interpreter has to keep aligned.
    internal (SettingPath Path, ChildPolicy Children) Route => Switch<(SettingPath, ChildPolicy)>(
        readCase: static value => (value.Path, ChildPolicy.Require),
        clampCase: static value => (value.Path, ChildPolicy.Create),
        putCase: static value => (value.Path, ChildPolicy.Create),
        deleteCase: static value => (value.Path, ChildPolicy.Require),
        readDefaultCase: static value => (value.Path, ChildPolicy.Require),
        putDefaultCase: static value => (value.Path, ChildPolicy.Create),
        metadataCase: static value => (value.Path, ChildPolicy.Require),
        hideCase: static value => (value.Path, ChildPolicy.Create),
        guardCase: static value => (value.Path, ChildPolicy.Create),
        changedCase: static value => (value.Path, ChildPolicy.Require),
        clearChangedCase: static value => (value.Path, ChildPolicy.Require),
        deleteChildCase: static value => (value.Path, ChildPolicy.Require),
        nodeVisibilityCase: static value => (value.Path, ChildPolicy.Require),
        treeCase: static value => (value.Path, ChildPolicy.Require));
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
    SettingDelta Delta);

// `Kind` is optional because a value another writer seated under a type this vocabulary does not model still has
// readable metadata; refusing the whole read would make a foreign neighbour hide its own node's facts.
public sealed record SettingMetadata(
    SettingKey Key,
    Type RuntimeType,
    Option<SettingKind> Kind,
    CapabilitySet<SettingTrait> Traits);

public sealed record SettingsTree(
    SettingPath Path,
    Seq<SettingMetadata> Values,
    Seq<SettingsTree> Children,
    CapabilitySet<SettingTrait> Traits);

public sealed record SettingsSaved(SettingsTree Tree, SaveOrigin Origin);

public sealed record SettingGuardSeat(SettingPath Path, SettingKey Key, SettingKind Kind);

public sealed record SettingNodeReceipt(
    SettingPath Path,
    Option<SettingKey> Child,
    SettingsVisibility Prior,
    SettingsVisibility Current);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingAnswer {
    private SettingAnswer() { }

    // `Adopted` names the legacy key the host renamed away during this read; every other read answers `None`.
    public sealed record ValueCase(Option<ArchiveValue> Value, Option<SettingKey> Adopted) : SettingAnswer;
    public sealed record MutationCase(SettingMutationReceipt Receipt) : SettingAnswer;
    public sealed record MetadataCase(SettingMetadata Metadata) : SettingAnswer;
    public sealed record ChangedCase(ChangeVerdict Verdict) : SettingAnswer;
    public sealed record GuardCase(SettingGuardSeat Seat) : SettingAnswer;
    public sealed record NodeCase(SettingNodeReceipt Receipt) : SettingAnswer;
    public sealed record TreeCase(SettingsTree Tree) : SettingAnswer;
}
```

## [04]-[INTERPRETER]

- Owner: `SettingStore` — the one settings entry: `Commit` resolves exactly one node and dispatches the operation exhaustively, `Observe` attaches the saved-settings watch.
- Entry: admission → root resolution → child resolution under the request's own `ChildPolicy` → typed host action → detached answer. Every operation outside the creating policy fails on a missing path with `MissingContext`.
- Law: explicit reads use `TryGet*` and never call mutating defaulted getters, and every read — enum included — enters through the owning `SettingKind` row's `Read` column. `ClampCase` is the one declared exception and says so in its own case.
- Law: one mutation fold owns observable and write-only receipts. A failed post-write read lands as `FaultedCase` evidence with `SettingDelta.Unobserved`, and deletion emits absence only after a host re-probe.
- Law: `AdmitTarget` compares each payload row with the existing host type, INCLUDING exact enum identity, before explicit or default writes, so a write against a key's seated kind refuses before it lands.
- Law: the guard seat is CLAIMED, not probed. `RegisterSettingsValidator<T>` is one assignment onto a private per-node map that OVERWRITES unconditionally, so a probe-then-write pair is a TOCTOU two callers both pass; the claim is first-writer-wins over one process-wide seat map keyed by path and key, and the `GetValidator<T>` probe deletes with the `InvalidCastException` arm it needed. NAMED LOSS: a validator seated by FOREIGN code on the same node and key is invisible to the claim, so the claim commits and the host silently overwrites it; witness — the deleted probe was already blind to exactly that case whenever the foreign specialization differed, because `GetValidator<T>` throws rather than answering for a mismatched `T`.
- Law: a claim whose host write then refuses RELEASES its seat, so a transient host refusal does not strand the key against every later attempt.
- Law: both reflection seams — the per-enum-type reader and the per-host-type validator writer — resolve their open handle once at type init and hold each closed delegate through `Cell.Claim`, so a lost mint race settles on the seated delegate rather than a second one.
- Law: the saved-settings handler has no return path, so the rail lands on the RECEIVER: a root projection or snapshot fault reaches the sink as a failed read rather than dying inside the frame, and only a sink that itself throws is converted and dropped, because there is nowhere left to report it.
- Law: the tree walk carries a depth budget and answers a TYPED exhaustion fault naming it, because the child chain is host-shaped and an unbounded walk fails the stack instead of the rail.
- Law: `ArchiveValue` (dictionary.md) is the one payload carrier across this boundary; `SettingsTree` admits and orders value and child keys before recursive projection, and this page owns no parallel event lifecycle beside `PlugIn.SettingsSaved`.
- Growth: a new operation is one case, one `Route` arm, and one `Execute` arm; the resolver and the receipt folds are untouched.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` with the generated total `Switch`, `[SmartEnum<TKey>]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `HashMap`, `Atom`, `Traverse`); kernel `Domain/rails` (`Op`, `Op.Probe`, `Op.Catch`, `Op.Side`, `Op.AcceptValidated`, `Cell.Claim`, `Cell.Step`, `Transition`, `KernelFault.InvalidValue`), `Numerics/atoms` (`Dimension`); `Document/lifetime` (`Subscription`), `Document/events` (`PluginKey`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[SETTINGS_TREE]`/`[SETTINGS_TYPED_READ]`/`[SETTINGS_METADATA]` — `FromPlugInId`, `RhinoAppSettings`, `TryGetChild`, `AddChild`, `DeleteChild`, `GetChild`, `DeleteItem`, `GetInteger` clamp overloads, `HideSettingFromUserInterface`, `RegisterSettingsValidator<T>`, `ContainsChangedValues`, `ClearChangedFlag`, `ContainsModifiedValues`, `PersistentSettingsEventArgs<T>`), RhinoCommon plug-ins (`api-rhinocommon-plugins.md` — `PlugIn.SettingsSaved`), RhinoCommon commands (`api-rhinocommon-commands.md` — `Command.Settings`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Globalization;
using System.Reflection;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] -----------------------------------------------------------------------------
public static class SettingStore {
    public static Fin<SettingAnswer> Commit(SettingOperation operation, Op? key = null) {
        Op op = key.OrDefault();
        return from request in op.Need(operation)
               from active in Admit(request, op)
               from node in Resolve(active.Route.Path, active.Route.Children, op)
               from answer in Execute(node, active, op)
               select answer;
    }

    public static Fin<Subscription> Observe(
        PlugIn plugIn,
        SavedSettingsRoot source,
        SettingPath path,
        Action<Fin<SettingsSaved>> sink,
        Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(plugIn).ToFin(Fail: op.MissingContext())
               from root in op.Need(source).Bind(value => Admit(value, op))
               from location in Admit(path, op)
               from receiver in op.Need(sink)
               let handler = new EventHandler<PersistentSettingsSavedEventArgs>((_, args) => ignore(op.Catch(() =>
                   Fin.Succ(value: Op.Side(() => receiver(
                       from node in op.Catch(() => Fin.Succ(value: root.Switch<PersistentSettingsSavedEventArgs, PersistentSettings>(
                           state: args,
                           plugInCase: static (state, _) => state.PlugInSettings,
                           commandCase: static (state, command) => state.CommandSettings(command.EnglishCommandName))))
                       from tree in Snapshot(node, location, DepthBudget.Value, op)
                       select new SettingsSaved(tree, (SaveOrigin)args.SavedByThisRhino))))))
               from subscription in Subscription.Attach(
                   subscribe: callback => owner.SettingsSaved += callback,
                   unsubscribe: callback => owner.SettingsSaved -= callback,
                   handler: handler)
               select subscription;
    }

    private static Fin<SettingOperation> Admit(SettingOperation operation, Op op) => operation.Switch<Op, Fin<SettingOperation>>(
        state: op,
        readCase: static (op, read) => op.Need(read.Kind)
            .Bind(kind => guard(kind != SettingKind.Enum || read.Legacy.IsEmpty, op.InvalidInput()).ToFin().Map(_ => kind))
            .Bind(kind => Admit(read.Legacy, read.Key, op).Map(legacy => (Kind: kind, Legacy: legacy)))
            .Bind(state => At(
                read.Path,
                read.Key,
                state,
                static (path, key, admitted) => new SettingOperation.ReadCase(path, key, admitted.Kind, admitted.Legacy),
                op)),
        clampCase: static (op, clamp) => Admit(clamp.Bound, op)
            .Bind(bound => At(
                clamp.Path,
                clamp.Key,
                (clamp.Fallback, Bound: bound),
                static (path, key, admitted) => new SettingOperation.ClampCase(path, key, admitted.Fallback, admitted.Bound),
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
                plugInCase: static (op, plugIn) => plugIn.Plugin.Admit(op).Map<SettingsRoot>(_ => plugIn),
                commandCase: static (op, command) => op.Need(command.Owner).Map<SettingsRoot>(_ => command)))
        from children in source.Children
            .Map(child => op.AcceptValidated<SettingKey>(child.Value))
            .Traverse(static value => value)
        select new SettingPath(root, children);

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

    private static Fin<IntegerBound> Admit(IntegerBound bound, Op op) => bound.Switch<Op, Fin<IntegerBound>>(
        state: op,
        lowerCase: static (_, row) => Fin.Succ<IntegerBound>(row),
        upperCase: static (_, row) => Fin.Succ<IntegerBound>(row),
        rangeCase: static (op, row) => guard(row.Floor <= row.Ceiling, op.InvalidInput())
            .ToFin()
            .Map<IntegerBound>(_ => row));

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
            clampCase: static (s, clamp) => s.Op
                .Catch(() => Fin.Succ(value: clamp.Bound.Clamped(s.Node, clamp.Key, clamp.Fallback)))
                .Bind(resolved => ArchiveValue.Of(resolved, s.Op))
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(Some(value), None)),
            putCase: static (s, put) => AdmitTarget(s.Node, put.Key, put.Value, s.Op).Bind(kind => Mutate(
                put.Path,
                put.Key,
                kind,
                CapabilitySet<SettingReach>.All,
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
                .Map<SettingAnswer>(static _ => new SettingAnswer.ChangedCase(ChangeVerdict.Clean)),
            deleteChildCase: static (s, delete) => DeleteChild(s.Node, delete, s.Op),
            nodeVisibilityCase: static (s, visibility) => NodeVisibility(s.Node, visibility, s.Op),
            treeCase: static (s, tree) => Snapshot(s.Node, tree.Path, DepthBudget.Value, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.TreeCase(value)));

    private static Fin<PersistentSettings> Resolve(SettingPath path, ChildPolicy children, Op op) =>
        path.Root.Switch<Op, Fin<PersistentSettings>>(
            state: op,
            applicationCase: static (op, _) => op.Catch(() => Fin.Succ(value: PersistentSettings.RhinoAppSettings)),
            plugInCase: static (op, plugIn) => plugIn.Plugin.Admit(op)
                .Bind(_ => op.Catch(() => Fin.Succ(value: PersistentSettings.FromPlugInId(plugIn.Plugin.ToValue())))),
            commandCase: static (op, command) => op.Catch(() => Fin.Succ(value: command.Owner.Settings)))
        .Bind(root => path.Children.Fold(
            Fin.Succ(value: root),
            (state, child) => state.Bind(parent => op.Catch(() =>
                Op.Probe<PersistentSettings>((out PersistentSettings found) => parent.TryGetChild(child.Value, out found))
                    .Match(
                        Some: found => Fin.Succ(value: found),
                        None: () => children.Key
                            ? Fin.Succ(value: parent.AddChild(child.Value))
                            : Fin.Fail<PersistentSettings>(error: op.MissingContext()))))));

    private static Fin<SettingAnswer> Mutate(
        SettingPath path,
        SettingKey key,
        SettingKind kind,
        CapabilitySet<SettingReach> reach,
        Func<Fin<Option<ArchiveValue>>> read,
        Func<Fin<Unit>> write) => reach.Admits(SettingReach.Read)
        ? Observe(path, key, kind, read, write)
        : write().Map(_ => (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutationReceipt(
            path,
            key,
            new SettingObservation.UnobservableCase(kind),
            new SettingObservation.UnobservableCase(kind),
            SettingDelta.Unobserved)));

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
                Same(prior, current) ? SettingDelta.Unchanged : SettingDelta.Changed)),
            Fail: fault => new SettingAnswer.MutationCase(new SettingMutationReceipt(
                path,
                key,
                new SettingObservation.ObservedCase(prior),
                new SettingObservation.FaultedCase(kind, fault),
                SettingDelta.Unobserved)));

    // The host resolves the current key FIRST and reaches the roster only on a miss, so the adopted key is derived
    // by the same order BEFORE the read runs: a seated current key adopts nothing, otherwise the first seated
    // roster key is the one the read is about to rename away. Deriving it after the read is impossible — the
    // rename has already erased the evidence.
    private static Fin<Option<SettingKey>> Adopted(PersistentSettings node, SettingKey key, Seq<SettingKey> legacy, Op op) =>
        legacy.IsEmpty
            ? Fin.Succ(value: Option<SettingKey>.None)
            : op.Catch(() => Fin.Succ(value: SeatedType(node, key).IsSome
                ? Option<SettingKey>.None
                : legacy.Find(row => SeatedType(node, row).IsSome)));

    private static Option<Type> SeatedType(PersistentSettings node, SettingKey key) =>
        Op.Probe<Type>((out Type value) => node.TryGetSettingType(key.Value, out value));

    private static Fin<SettingKind> AdmitTarget(PersistentSettings node, SettingKey key, ArchiveValue value, Op op) =>
        from kind in SettingKind.For(value, op)
        from existing in op.Catch(() => Fin.Succ(value: SeatedType(node, key)))
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
        from type in op.Catch(() => Fin.Succ(value: SeatedType(node, request.Key)))
        from prior in type.Match(
            Some: found => SettingKind.For(found, op)
                .Bind(kind => kind.Read(node, request.Key, Seq<SettingKey>.Empty, op)),
            None: () => Fin.Succ(value: Option<ArchiveValue>.None))
        from _ in op.Catch(() => node.DeleteItem(request.Key.Value))
        from _absent in op.Catch(() => guard(SeatedType(node, request.Key).IsNone, op.InvalidResult()).ToFin())
        select (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutationReceipt(
            request.Path,
            request.Key,
            new SettingObservation.ObservedCase(prior),
            new SettingObservation.ObservedCase(None),
            prior.IsSome ? SettingDelta.Changed : SettingDelta.Unchanged));

    private static Fin<SettingAnswer> DeleteChild(PersistentSettings node, SettingOperation.DeleteChildCase request, Op op) =>
        from before in Visibility(node, op)
        from _present in op.Catch(() => guard(Child(node, request.Child).IsSome, op.MissingContext()).ToFin())
        from _delete in op.Catch(() => node.DeleteChild(request.Child.Value))
        from _absent in op.Catch(() => guard(
            Child(node, request.Child).IsNone,
            op.InvalidResult(detail: $"Settings child '{request.Child.Value}' survived deletion.")).ToFin())
        from after in Visibility(node, op)
        select (SettingAnswer)new SettingAnswer.NodeCase(new SettingNodeReceipt(
            request.Path,
            Some(request.Child),
            before,
            after));

    private static Fin<SettingAnswer> NodeVisibility(
        PersistentSettings node,
        SettingOperation.NodeVisibilityCase request,
        Op op) =>
        from before in Visibility(node, op)
        from _write in op.Catch(() => node.HiddenFromUserInterface = request.Visibility.Key)
        from after in Visibility(node, op)
        from _proof in guard(
            after == request.Visibility,
            op.InvalidResult(detail: "Settings node visibility postcondition failed.")).ToFin()
        select (SettingAnswer)new SettingAnswer.NodeCase(new SettingNodeReceipt(request.Path, None, before, after));

    private static Fin<SettingsVisibility> Visibility(PersistentSettings node, Op op) =>
        op.Catch(() => Fin.Succ(value: (SettingsVisibility)node.HiddenFromUserInterface));

    private static Option<PersistentSettings> Child(PersistentSettings node, SettingKey key) =>
        Op.Probe<PersistentSettings>((out PersistentSettings found) => node.TryGetChild(key.Value, out found));

    private static Fin<SettingMetadata> Metadata(PersistentSettings node, SettingKey key, Op op) =>
        op.Catch(() => Fin.Succ(value: (
                Runtime: node.GetSettingType(key.Value),
                Traits: Held(
                    (node.GetSettingIsReadOnly(key.Value), SettingTrait.ReadOnly),
                    (node.GetSettingIsHiddenFromUserInterface(key.Value), SettingTrait.Hidden)))))
            .Map(read => new SettingMetadata(key, read.Runtime, SettingKind.For(read.Runtime, op).ToOption(), read.Traits));

    private static CapabilitySet<SettingTrait> Held(params ReadOnlySpan<(bool Holds, SettingTrait Trait)> rows) =>
        CapabilitySet<SettingTrait>.Of(rows
            .ToArray()
            .Where(static row => row.Holds)
            .Select(static row => row.Trait)
            .ToArray());

    // The open handle resolves once at type init and each host type's closed writer is minted once and held; the
    // typed delegate also retires the `is Fin<Unit>` unbox the reflective invoke forced on every registration.
    private static readonly Option<MethodInfo> ValidatorTemplate = Optional(typeof(SettingStore).GetMethod(
        nameof(RegisterTyped),
        BindingFlags.NonPublic | BindingFlags.Static));

    private static readonly Atom<HashMap<Type, Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>>> ValidatorWriters =
        Atom(HashMap<Type, Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>>());

    private static readonly Atom<HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>> GuardSeats =
        Atom(HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>());

    private static Fin<Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>> ValidatorWriter(Type hostType, Op op) =>
        ValidatorWriters.Value.Find(hostType).Match(
            Some: static held => Fin.Succ(value: held),
            None: () => ValidatorTemplate
                .ToFin(Fail: op.MissingContext())
                .Bind(open => op.Catch(() => Fin.Succ(value: open
                    .MakeGenericMethod(hostType)
                    .CreateDelegate<Func<PersistentSettings, string, ISettingGuard, Op, Fin<Unit>>>())))
                .Map(minted => Cell.Claim(cell: ValidatorWriters, key: hostType, mint: () => minted).Current[hostType]));

    private static Fin<SettingAnswer> Register(PersistentSettings node, SettingOperation.GuardCase request, Op op) {
        (SettingPath Path, SettingKey Key) seat = (request.Path, request.Key);
        return Cell.Claim(
                cell: GuardSeats,
                key: seat,
                mint: () => new SettingGuardSeat(request.Path, request.Key, request.Guard.Kind))
            .Switch(
                state: (Node: node, Request: request, Seat: seat, Op: op),
                committed: static (ctx, landed) => ValidatorWriter(ctx.Request.Guard.HostType, ctx.Op)
                    .Bind(write => write(ctx.Node, ctx.Request.Key.Value, ctx.Request.Guard, ctx.Op))
                    .Map(_ => (SettingAnswer)new SettingAnswer.GuardCase(landed.State[ctx.Seat]))
                    .BindFail(error => Released(ctx.Seat, error, ctx.Op)),
                ceded: static (ctx, _) => Fin.Fail<SettingAnswer>(error: ctx.Op.InvalidResult(
                    detail: $"Settings validator '{ctx.Request.Key.Value}' is already seated.")),
                refused: static (_, declined) => Fin.Fail<SettingAnswer>(error: declined.Cause),
                contended: static (ctx, _) => Fin.Fail<SettingAnswer>(error: ctx.Op.InvalidResult()));
    }

    private static Fin<SettingAnswer> Released((SettingPath Path, SettingKey Key) seat, Error primary, Op op) {
        Transition<HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>> released = Cell.Step(
            cell: GuardSeats,
            step: held => held.ContainsKey(seat) ? Some(held.Remove(seat)) : None,
            declined: op.InvalidContext());
        return released switch {
            Transition<HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>>.Committed =>
                Fin.Fail<SettingAnswer>(error: primary),
            Transition<HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>>.Refused row =>
                Fin.Fail<SettingAnswer>(error: primary + row.Cause),
            _ => Fin.Fail<SettingAnswer>(error: primary + op.InvalidResult()),
        };
    }

    private static Fin<Unit> RegisterTyped<T>(PersistentSettings node, string key, ISettingGuard guard, Op op) =>
        op.Catch(() => {
            node.RegisterSettingsValidator<T>(key, (_, args) => ignore(op.Catch(() =>
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
                }))));
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
            Some: path => Resolve(path, ChildPolicy.Require, op)
                .Bind(other => op.Catch(() => Fin.Succ(value: node.ContainsModifiedValues(other)))),
            None: () => op.Catch(() => Fin.Succ(value: node.ContainsChangedValues())))
        .Map<SettingAnswer>(static changed => new SettingAnswer.ChangedCase((ChangeVerdict)changed));

    // The child chain is host-shaped and a pathological depth would fail the stack instead of the rail, so the walk
    // spends a budget and answers a typed exhaustion naming it.
    private static readonly Rasm.Numerics.Dimension DepthBudget = Rasm.Numerics.Dimension.Create(value: 32);

    private static Fin<SettingsTree> Snapshot(PersistentSettings node, SettingPath path, int remaining, Op op) =>
        remaining <= 0
            ? Fin.Fail<SettingsTree>(error: new KernelFault.InvalidValue(
                Label: nameof(SettingsTree),
                Requirement: $"a settings subtree within {DepthBudget.Value.ToString(CultureInfo.InvariantCulture)} levels",
                Key: Some(op)))
            : op.Catch(() =>
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
                                     from tree in Snapshot(
                                         child,
                                         path with { Children = path.Children.Add(admitted) },
                                         remaining - 1,
                                         op)
                                     select tree)
                    .Traverse(static value => value)
                select new SettingsTree(
                    path,
                    values,
                    children,
                    Held(
                        (node.HiddenFromUserInterface, SettingTrait.Hidden),
                        (node.ContainsChangedValues(), SettingTrait.Changed))));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
