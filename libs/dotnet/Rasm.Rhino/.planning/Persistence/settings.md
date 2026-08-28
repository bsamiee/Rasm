# [RASM_RHINO_PERSISTENCE_SETTINGS]

`SettingKind` owns the complete `PersistentSettings` value matrix, including explicit/default asymmetry, as one keyed behavior vocabulary whose rows carry the probe, write, default, capture, and host-projection delegates over the shared `ArchiveValue` carrier (dictionary.md). `SettingOperation` closes reads, writes, defaults, clamped reads, metadata, validators, change state, and saved-tree projection behind `SettingStore.Commit`.

## [01]-[INDEX]

- [02]-[VALUE_AND_KIND]: `SettingReach`, `SettingKind` — the layer-reach capability and the one row family naming a host `TryGet*`/`Set*` member.
- [03]-[REQUEST_ALGEBRA]: `SettingKey`, `SettingsRoot`, `SettingPath`, `ChildPolicy`, `ISettingGuard`, `SettingsVisibility`, `SettingTrait`, `SettingDelta`, `ChangeVerdict`, `SaveOrigin`, `IntegerBound`, `SettingOperation`, `SettingObservation`, `SettingMutation`, `SettingMetadata`, `SettingsTree`, `SettingsSaved`, `SettingGuardSeat`, `NodeMutation`, `SavedSettingsRoot`, `SettingAnswer` — the addressing, request, and answer vocabularies.
- [04]-[INTERPRETER]: `SettingStore` — node resolution, total dispatch, the two reflection adapters, and the guard seat.

## [02]-[VALUE_AND_KIND]

- Owner: `SettingKind` — one row per host value kind, carrying `Shape`, `HostType`, the layer-reach column, and the `Read`/`Write`/`ReadDefault`/`WriteDefault`/`Capture`/`Host` delegates; `SettingReach` — the two-row capability naming which halves of a storage layer the host publishes.
- Entry: three factory arms mint a row and their SIGNATURES demand exactly the delegates each claims — `OfNone` takes no preset column, `OfWriteOnly` demands the preset writer, `OfReadWrite` demands both — so the reach column is DERIVED at the mint and a row whose declared mode and delegate set disagree is unspellable.
- Law: `Shape` is the carrier payload type `For` matches and `HostType` is the host runtime type `Accepts` matches; the two diverge exactly where the detached form differs from the host form — `TextList`, `TextMap`, `OptionalColor`.
- Law: the default layer's reach is the HOST's, catalogue-proven — `SetDefault` covers sixteen kinds and `TryGetDefault` twelve, and every probe-able kind is a writable one, so a readable-but-unwritable default layer is a corner the host never publishes and the three arms are the whole legal roster.
- Law: rename tolerance is an EXPLICIT-READ capability only. Every `TryGet*` carries a third `IEnumerable<string> legacyKeyList` parameter, so the `probe` column takes the roster; `TryGetDefault` publishes no such overload and `TryGetEnumValue<T>` hard-passes `null` to the same resolver, so the preset probe stays two-argument and the enum row discards the roster its `Read` column receives — the asymmetry is the host's, and a roster threaded into either is an unread argument.
- Law: ONE enum read exists and it is the enum row's own `Read` column. The concrete type is the SEATED type the host already records — `TryGetSettingType` answers `SettingValue.RuntimeType`, which `SetEnumValue<T>` writes as `typeof(T)` — so no caller carries a parallel type argument, no second operation case spells the same read, and an unseated key answers absent exactly as every other row's miss does.
- Law: every `bool Try*(out T)` crossing lifts through the kernel `HostEdge.Probe` — absence is ordinary and answers `Option<T>`, so no row re-spells the `? Some : None` ternary and no host miss reaches the carrier as a fault.
- Law: `For(Type)` refuses a foreign-written kind through `Unsupported` carrying the HOST TYPE TOKEN, so a value another writer seated under a type this vocabulary does not model names itself in the refusal instead of collapsing to a bare invalid-input.
- Growth: a new host value kind is one row through the arm matching its default reach; a new layer half is one `SettingReach` row.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[UseDelegateFromConstructor]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `HashMap`, `Atom`); kernel `Domain/results` (`HostEdge.Probe`, `Try.lift`, `Cell.Claim`, `Transition`), `Domain/validation` (`ICapability`, `CapabilitySet`); `Persistence/dictionary` (`ArchiveValue`, `ArchiveValue.Of`/`Project`/`Enum`/`EnumMint`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[SETTINGS_TYPED_READ]`/`[SETTINGS_TYPED_WRITE]`/`[SETTINGS_DEFAULTS]` — the sixteen `TryGet*`/`Set*` pairs with their `legacyKeyList` siblings, `SetDefault`/`TryGetDefault`, `SetEnumValue<T>`/`TryGetEnumValue<T>`, `TryGetSettingType`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Drawing;
using System.Reflection;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SettingReach : ICapability<SettingReach> {
    public static readonly SettingReach Write = new("write");
    public static readonly SettingReach Read = new("read");
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SettingKind {
    public static readonly SettingKind Guid = OfWriteOnly<Guid>(
        key: "guid",
        probe: static (node, key, legacy) => Admit.Probe<Guid>((out Guid value) => node.TryGetGuid(out value, legacy)),
        put: static (node, key, value) => node.SetGuid(value),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Bool = OfReadWrite<bool>(
        key: "bool",
        probe: static (node, key, legacy) => Admit.Probe<bool>((out bool value) => node.TryGetBool(out value, legacy)),
        put: static (node, key, value) => node.SetBool(value),
        probePreset: static (node, key) => Admit.Probe<bool>((out bool value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Byte = OfReadWrite<byte>(
        key: "byte",
        probe: static (node, key, legacy) => Admit.Probe<byte>((out byte value) => node.TryGetByte(out value, legacy)),
        put: static (node, key, value) => node.SetByte(value),
        probePreset: static (node, key) => Admit.Probe<byte>((out byte value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Integer = OfReadWrite<int>(
        key: "integer",
        probe: static (node, key, legacy) => Admit.Probe<int>((out int value) => node.TryGetInteger(out value, legacy)),
        put: static (node, key, value) => node.SetInteger(value),
        probePreset: static (node, key) => Admit.Probe<int>((out int value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind UnsignedInteger = OfNone<uint>(
        key: "unsigned-integer",
        probe: static (node, key, legacy) => Admit.Probe<uint>((out uint value) => node.TryGetUnsignedInteger(out value, legacy)),
        put: static (node, key, value) => node.SetUnsignedInteger(value));
    public static readonly SettingKind Double = OfReadWrite<double>(
        key: "double",
        probe: static (node, key, legacy) => Admit.Probe<double>((out double value) => node.TryGetDouble(out value, legacy)),
        put: static (node, key, value) => node.SetDouble(value),
        probePreset: static (node, key) => Admit.Probe<double>((out double value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Char = OfReadWrite<char>(
        key: "char",
        probe: static (node, key, legacy) => Admit.Probe<char>((out char value) => node.TryGetChar(out value, legacy)),
        put: static (node, key, value) => node.SetChar(value),
        probePreset: static (node, key) => Admit.Probe<char>((out char value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Text = OfReadWrite<string>(
        key: "text",
        probe: static (node, key, legacy) => Admit.Probe<string>((out string value) => node.TryGetString(out value, legacy)),
        put: static (node, key, value) => node.SetString(value),
        probePreset: static (node, key) => Admit.Probe<string>((out string value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind TextList = OfReadWrite<Seq<string>>(
        key: "text-list",
        probe: static (node, key, legacy) => Admit.Probe<string[]>((out string[] value) => node.TryGetStringList(out value, legacy))
            .Map(toSeq),
        put: static (node, key, value) => node.SetStringList(value.ToArray()),
        probePreset: static (node, key) => Admit.Probe<string[]>((out string[] value) => node.TryGetDefault(out value))
            .Map(toSeq),
        putPreset: static (node, key, value) => node.SetDefault(value.ToArray()),
        capture: static (source, op) => source switch {
            string[] rows => ArchiveValue.Of(toSeq(rows), op),
            Seq<string> sequence => ArchiveValue.Of(sequence, op),
            _ => Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidInput()),
        },
        host: static (value, op) => value.Project<Seq<string>>().Map(static sequence => (object?)sequence.ToArray()),
        shape: typeof(string[]),
        hostType: typeof(string[]));
    public static readonly SettingKind TextMap = OfWriteOnly<HashMap<string, string>>(
        key: "text-map",
        probe: static (node, key, legacy) => Admit.Probe<KeyValuePair<string, string>[]>((out KeyValuePair<string, string>[] value) =>
                node.TryGetStringDictionary(out value, legacy))
            .Map(static rows => rows.ToHashMap()),
        put: static (node, key, value) => node.SetStringDictionary(TextMapRows(value)),
        putPreset: static (node, key, value) => node.SetDefault(TextMapRows(value)),
        capture: static (source, op) => source switch {
            KeyValuePair<string, string>[] rows => ArchiveValue.Of(rows.ToHashMap(), op),
            HashMap<string, string> map => ArchiveValue.Of(map, op),
            _ => Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidInput()),
        },
        host: static (value, op) => value.Project<HashMap<string, string>>()
            .Map(static map => (object?)TextMapRows(map)),
        hostType: typeof(KeyValuePair<string, string>[]));
    public static readonly SettingKind Date = OfReadWrite<DateTime>(
        key: "date",
        probe: static (node, key, legacy) => Admit.Probe<DateTime>((out DateTime value) => node.TryGetDate(out value, legacy)),
        put: static (node, key, value) => node.SetDate(value),
        probePreset: static (node, key) => Admit.Probe<DateTime>((out DateTime value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Color = OfReadWrite<Color>(
        key: "color",
        probe: static (node, key, legacy) => Admit.Probe<Color>((out Color value) => node.TryGetColor(out value, legacy)),
        put: static (node, key, value) => node.SetColor(value),
        probePreset: static (node, key) => Admit.Probe<Color>((out Color value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind OptionalColor = OfWriteOnly<Option<Color>>(
        key: "optional-color",
        probe: static (node, key, legacy) => Admit.Probe<Color?>((out Color? value) => node.TryGetColor(out value, legacy))
            .Map(Optional),
        put: static (node, key, value) => node.SetColor(value.Match<Color?>(Some: static color => color, None: static () => null)),
        putPreset: static (node, key, value) => node.SetDefault(value.Match<Color?>(Some: static color => color, None: static () => null)),
        capture: static (source, op) => source switch {
            null => ArchiveValue.Of(Option<Color>.None, op),
            Color color => ArchiveValue.Of(Some(color), op),
            Option<Color> optional => ArchiveValue.Of(optional, op),
            _ => Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidInput()),
        },
        host: static (value, op) => value.Project<Option<Color>>()
            .Map(optional => optional.Match<object?>(Some: static color => color, None: static () => null)),
        hostType: typeof(Color?));
    public static readonly SettingKind Point = OfWriteOnly<Point>(
        key: "point",
        probe: static (node, key, legacy) => Admit.Probe<Point>((out Point value) => node.TryGetPoint(out value, legacy)),
        put: static (node, key, value) => node.SetPoint(value),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Point3d = OfReadWrite<Point3d>(
        key: "point3d",
        probe: static (node, key, legacy) => Admit.Probe<Point3d>((out Point3d value) => node.TryGetPoint3d(out value, legacy)),
        put: static (node, key, value) => node.SetPoint3d(value),
        probePreset: static (node, key) => Admit.Probe<Point3d>((out Point3d value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Size = OfReadWrite<Size>(
        key: "size",
        probe: static (node, key, legacy) => Admit.Probe<Size>((out Size value) => node.TryGetSize(out value, legacy)),
        put: static (node, key, value) => node.SetSize(value),
        probePreset: static (node, key) => Admit.Probe<Size>((out Size value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Rectangle = OfReadWrite<Rectangle>(
        key: "rectangle",
        probe: static (node, key, legacy) => Admit.Probe<Rectangle>((out Rectangle value) => node.TryGetRectangle(out value, legacy)),
        put: static (node, key, value) => node.SetRectangle(value),
        probePreset: static (node, key) => Admit.Probe<Rectangle>((out Rectangle value) => node.TryGetDefault(out value)),
        putPreset: static (node, key, value) => node.SetDefault(value));
    public static readonly SettingKind Enum = new(
        key: "enum",
        defaults: CapabilitySet<SettingReach>.None,
        shape: typeof(System.Enum),
        hostType: typeof(System.Enum),
        read: static (node, key, _, op) => ReadEnum(node),
        write: static (node, key, value, op) => value.EnumEntry
            .ToFin(Fail: new KernelFault.InvalidInput())
            .Bind(entry => ArchiveValue.EnumMint(node, nameof(PersistentSettings.SetEnumValue), key.Value, entry)),
        readDefault: static (_, _, op) => Fin.Fail<Option<ArchiveValue>>(error: new KernelFault.Unsupported(
            InputType: typeof(System.Enum), OutputType: typeof(PersistentSettings))),
        writeDefault: static (_, _, _, op) => Fin.Fail<Unit>(error: new KernelFault.Unsupported(
            InputType: typeof(System.Enum), OutputType: typeof(PersistentSettings))),
        capture: static (source, op) => source is null
            ? Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidInput())
            : ArchiveValue.Enum(source, op),
        host: static (value, op) => value.EnumEntry
            .ToFin(Fail: new KernelFault.InvalidInput())
            .Bind(entry => Try.lift(() => Fin.Succ<object?>(value: System.Enum.Parse(entry.EnumType, entry.Name, ignoreCase: true))).Run().Bind(static inner => inner)));

    public CapabilitySet<SettingReach> Defaults { get; }

    public Type Shape { get; }

    public Type HostType { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Option<ArchiveValue>> Read(PersistentSettings node, SettingKey key, Seq<SettingKey> legacy);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Write(PersistentSettings node, SettingKey key, ArchiveValue value);

    [UseDelegateFromConstructor]
    internal partial Fin<Option<ArchiveValue>> ReadDefault(PersistentSettings node, SettingKey key);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> WriteDefault(PersistentSettings node, SettingKey key, ArchiveValue value);

    [UseDelegateFromConstructor]
    internal partial Fin<ArchiveValue> Capture(object? source);

    [UseDelegateFromConstructor]
    internal partial Fin<object?> Host(ArchiveValue value);

    internal bool Accepts(Type type) => type == HostType || (this == Enum && type.IsEnum);

    internal bool Accepts(Type type, ArchiveValue value) => this == Enum
        ? value.EnumEntry.Match(Some: entry => entry.EnumType == type, None: static () => false)
        : type == HostType;

    internal static Fin<SettingKind> For(ArchiveValue value) =>
        value.EnumEntry.IsSome
            ? Fin.Succ(value: Enum)
            : toSeq(SettingKind.Items).Find(kind => kind.Shape == value.Shape)
                .ToFin(Fail: new KernelFault.Unsupported(InputType: value.Shape, OutputType: typeof(SettingKind)));

    internal static Fin<SettingKind> For(Type type) =>
        toSeq(SettingKind.Items).Find(kind => kind.Accepts(type))
            .ToFin(Fail: new KernelFault.Unsupported(InputType: type, OutputType: typeof(SettingKind)));

    private static KeyValuePair<string, string>[] TextMapRows(HashMap<string, string> map) => map
        .AsIterable()
        .ToSeq()
        .Map(static row => KeyValuePair.Create(row.Key, row.Value))
        .OrderBy(static row => row.Key, StringComparer.Ordinal)
        .ToArray();

    private static Fin<Option<ArchiveValue>> ReadEnum(PersistentSettings source, SettingKey key) =>
        Try.lift(() => Admit.Probe<Type>((out Type value) => source.TryGetSettingType(key.Value, out value))).Run()
            .Bind(seated => seated.Match(
                Some: type => type.IsEnum
                    ? EnumReader(type).Bind(read => Try.lift(() => read(source, key.Value)).Run().Bind(static inner => inner))
                    : Fin.Fail<Option<ArchiveValue>>(error: new KernelFault.Unsupported(
                        InputType: type, OutputType: typeof(System.Enum))),
                None: static () => Fin.Succ(value: Option<ArchiveValue>.None)));

    private static readonly Option<MethodInfo> EnumReaderTemplate = Optional(typeof(SettingKind).GetMethod(
        nameof(ReadEnumTyped),
        BindingFlags.NonPublic | BindingFlags.Static));

    private static readonly Atom<HashMap<Type, Func<PersistentSettings, string, Fin<Option<ArchiveValue>>>>> EnumReaders =
        Atom(HashMap<Type, Func<PersistentSettings, string, Fin<Option<ArchiveValue>>>>());

    private static Fin<Func<PersistentSettings, string, Fin<Option<ArchiveValue>>>> EnumReader(Type enumType) =>
        EnumReaders.Value.Find(enumType).Match(
            Some: static held => Fin.Succ(value: held),
            None: () => EnumReaderTemplate
                .ToFin(Fail: new KernelFault.MissingContext())
                .Bind(open => Try.lift(() => open
                    .MakeGenericMethod(enumType)
                    .CreateDelegate<Func<PersistentSettings, string, Fin<Option<ArchiveValue>>>>()).Run())
                .Map(minted => Cell.Claim(cell: EnumReaders, key: enumType, mint: () => minted).Current[enumType]));

    private static Fin<Option<ArchiveValue>> ReadEnumTyped<T>(PersistentSettings source, string key)
        where T : struct, IConvertible =>
        source.TryGetEnumValue(out T value)
            ? ArchiveValue.Enum(value).Map(Some)
            : Fin.Succ(value: Option<ArchiveValue>.None);

    private static SettingKind OfNone<T>(
        string key,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<object?, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull => Mint(CapabilitySet<SettingReach>.None, probe, put, probePreset: null, putPreset: null, capture, host, shape, hostType);

    private static SettingKind OfWriteOnly<T>(
        string key,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Action<PersistentSettings, string, T> putPreset,
        Func<object?, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull => Mint(CapabilitySet<SettingReach>.Of(SettingReach.Write), probe, put, probePreset: null, putPreset, capture, host, shape, hostType);

    private static SettingKind OfReadWrite<T>(
        string key,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<PersistentSettings, string, Option<T>> probePreset,
        Action<PersistentSettings, string, T> putPreset,
        Func<object?, Fin<ArchiveValue>>? capture = null,
        Func<ArchiveValue, Fin<object?>>? host = null,
        Type? shape = null,
        Type? hostType = null) where T : notnull => Mint(CapabilitySet<SettingReach>.All, probe, put, probePreset, putPreset, capture, host, shape, hostType);

    private static SettingKind Mint<T>(
        string key,
        CapabilitySet<SettingReach> defaults,
        Func<PersistentSettings, string, IEnumerable<string>, Option<T>> probe,
        Action<PersistentSettings, string, T> put,
        Func<PersistentSettings, string, Option<T>>? probePreset,
        Action<PersistentSettings, string, T>? putPreset,
        Func<object?, Fin<ArchiveValue>>? capture,
        Func<ArchiveValue, Fin<object?>>? host,
        Type? shape,
        Type? hostType) where T : notnull =>
        new(defaults,
            shape: shape ?? typeof(T),
            hostType: hostType ?? typeof(T),
            read: (node, settingKey, legacy, op) => Try.lift(() => probe(
                node, settingKey.Value, legacy.Map(static row => row.Value))
                .TraverseM(value => ArchiveValue.Of(value, op))
                .As()).Run().Bind(static inner => inner),
            write: (node, settingKey, value, op) => value.Project<T>()
                .Bind(typed => Try.lift(() => put(node, settingKey.Value, typed)).Run().Bind(static inner => inner)),
            readDefault: probePreset is null
                ? (_, _, op) => Fin.Fail<Option<ArchiveValue>>(error: new KernelFault.Unsupported(
                    InputType: typeof(T), OutputType: typeof(PersistentSettings)))
                : (node, settingKey, op) => Try.lift(() => probePreset(node, settingKey.Value)
                    .TraverseM(value => ArchiveValue.Of(value, op))
                    .As()).Run().Bind(static inner => inner),
            writeDefault: putPreset is null
                ? (_, _, _, op) => Fin.Fail<Unit>(error: new KernelFault.Unsupported(
                    InputType: typeof(T), OutputType: typeof(PersistentSettings)))
                : (node, settingKey, value, op) => value.Project<T>()
                    .Bind(typed => Try.lift(() => putPreset(node, settingKey.Value, typed)).Run().Bind(static inner => inner)),
            capture: capture ?? ((source, op) => source is T typed
                ? ArchiveValue.Of(typed, op)
                : Fin.Fail<ArchiveValue>(error: new KernelFault.InvalidInput())),
            host: host ?? ((value, op) => value.Project<T>().Map(static typed => (object?)typed)));
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
- Law: `ClampCase` is a MUTATING read. Every defaulted `Get<Kind>(default)` stamps the default layer on a hit and registers the default AND materializes the key on a miss, so the clamped integer reads route with `ChildPolicy.Create` and answer a value the tree now holds. `int` is the only kind carrying clamp overloads, and the case names it.
- Law: every flag on a node or value rides `CapabilitySet<SettingTrait>` and every two-state host answer rides its own `[SmartEnum<bool>]` row, so `ReadOnly`, `Hidden`, and `Changed` are one held set and no answer carries a bare bool a reader must interpret from position.
- Law: `NodeMutation` carries the prior and current visibility rows alone — a child-deleted flag beside an `Option<SettingKey> Child` was a second authority over the same fact, true exactly when the child is present.
- Law: `SettingGuardSeat` is published rather than swallowed: `PersistentSettings` publishes no unregister, no clear, and no null-accepting overload, so a seated guard holds for the node's process lifetime and the caller holds what it can never hand back.
- Growth: a new request is one `SettingOperation` case with its `Route` arm and its `Execute` arm; a new flag is one `SettingTrait` row.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<TKey>]`, `[Union]`, `[ValueObject<T>]`, `[ValidationError]`, `IDisallowDefaultValue`); LanguageExt.Core (`Fin`, `Option`, `Seq`); kernel `Domain/results` , `Domain/validation` (`ICapability`, `CapabilitySet`); `Document/events` (`PluginKey`); `Persistence/dictionary` (`ArchiveValue`), `Persistence/presets` (`PersistenceFault`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[SETTINGS_TREE]`/`[SETTINGS_METADATA]` — `FromPlugInId`, `RhinoAppSettings`, `Keys`, `ChildKeys`, `HiddenFromUserInterface`, `GetSettingType`, `GetSettingIsReadOnly`, `GetSettingIsHiddenFromUserInterface`, `StringListRootKey`, `PersistentSettingsSavedEventArgs`), RhinoCommon commands (`api-rhinocommon-commands.md` — `Command.Settings`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public readonly partial struct SettingKey : IDisallowDefaultValue {
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

// --- [SERVICES] ------------------------------------------------------------------------
public interface ISettingGuard {
    SettingKind Kind { get; }
    Type HostType { get; }
    Fin<ArchiveValue> Validate(ArchiveValue current, ArchiveValue proposed);
    void Report(Error error);
}

// --- [MODELS] --------------------------------------------------------------------------
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

public sealed record SettingMutation(
    SettingPath Path,
    SettingKey Key,
    SettingObservation Prior,
    SettingObservation Current,
    SettingDelta Delta);

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

public sealed record NodeMutation(
    SettingPath Path,
    Option<SettingKey> Child,
    SettingsVisibility Prior,
    SettingsVisibility Current);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingAnswer {
    private SettingAnswer() { }

    public sealed record ValueCase(Option<ArchiveValue> Value, Option<SettingKey> Adopted) : SettingAnswer;
    public sealed record MutationCase(SettingMutation Mutation) : SettingAnswer;
    public sealed record MetadataCase(SettingMetadata Metadata) : SettingAnswer;
    public sealed record ChangedCase(ChangeVerdict Verdict) : SettingAnswer;
    public sealed record GuardCase(SettingGuardSeat Seat) : SettingAnswer;
    public sealed record NodeCase(NodeMutation Mutation) : SettingAnswer;
    public sealed record TreeCase(SettingsTree Tree) : SettingAnswer;
}
```

## [04]-[INTERPRETER]

- Owner: `SettingStore` — the one settings entry: `Commit` resolves exactly one node and dispatches the operation exhaustively, `Observe` attaches the saved-settings watch.
- Entry: admission → root resolution → child resolution under the request's own `ChildPolicy` → typed host action → detached answer. Every operation outside the creating policy fails on a missing path with `MissingContext`.
- Law: explicit reads use `TryGet*` and never call mutating defaulted getters, and every read — enum included — enters through the owning `SettingKind` row's `Read` column. `ClampCase` is the one declared exception and says so in its own case.
- Law: one mutation fold owns observable and write-only mutations. A failed post-write read lands as `FaultedCase` evidence with `SettingDelta.Unobserved`, and deletion emits absence only after a host re-probe.
- Law: `AdmitTarget` compares each payload row with the existing host type, INCLUDING exact enum identity, before explicit or default writes, so a write against a key's seated kind refuses before it lands.
- Law: the guard seat is CLAIMED, not probed. `RegisterSettingsValidator<T>` is one assignment onto a private per-node map that OVERWRITES unconditionally, so a probe-then-write pair is a TOCTOU two callers both pass; the claim is first-writer-wins over one process-wide seat map keyed by path and key, and the `GetValidator<T>` probe deletes with the `InvalidCastException` arm it needed. NAMED LOSS: a validator seated by FOREIGN code on the same node and key is invisible to the claim, so the claim commits and the host silently overwrites it; witness — the deleted probe was already blind to exactly that case whenever the foreign specialization differed, because `GetValidator<T>` throws rather than answering for a mismatched `T`.
- Law: a claim whose host write then refuses RELEASES its seat, so a transient host refusal does not strand the key against every later attempt.
- Law: both reflection adapters — the per-enum-type reader and the per-host-type validator writer — resolve their open handle once at type init and hold each closed delegate through `Cell.Claim`, so a lost mint race settles on the seated delegate rather than a second one.
- Law: the saved-settings handler has no return path, so the result lands on the RECEIVER: a root projection or snapshot fault reaches the sink as a failed read rather than dying inside the frame, and only a sink that itself throws is converted and dropped, because there is nowhere left to report it.
- Law: the tree walk carries a depth budget and answers a TYPED exhaustion fault naming it, because the child chain is host-shaped and an unbounded walk fails the stack instead of the carrier.
- Law: `ArchiveValue` (dictionary.md) is the one payload carrier across this boundary; `SettingsTree` admits and orders value and child keys before recursive projection, and this page owns no parallel event lifecycle beside `PlugIn.SettingsSaved`.
- Growth: a new operation is one case, one `Route` arm, and one `Execute` arm; the resolver and the mutation folds are untouched.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` with the generated total `Switch`, `[SmartEnum<TKey>]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `HashMap`, `Atom`, `Traverse`); kernel `Domain/results` (`HostEdge.Probe`, `Try.lift`, `HostEdge.Side`, `FactoryBridge.Accept`, `Cell.Claim`, `Cell.Step`, `Transition`, `KernelFault.InvalidValue`), `Numerics/atoms` (`Dimension`); `Document/lifetime` (`Subscription`), `Document/events` (`PluginKey`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[SETTINGS_TREE]`/`[SETTINGS_TYPED_READ]`/`[SETTINGS_METADATA]` — `FromPlugInId`, `RhinoAppSettings`, `TryGetChild`, `AddChild`, `DeleteChild`, `GetChild`, `DeleteItem`, `GetInteger` clamp overloads, `HideSettingFromUserInterface`, `RegisterSettingsValidator<T>`, `ContainsChangedValues`, `ClearChangedFlag`, `ContainsModifiedValues`, `PersistentSettingsEventArgs<T>`), RhinoCommon plug-ins (`api-rhinocommon-plugins.md` — `PlugIn.SettingsSaved`), RhinoCommon commands (`api-rhinocommon-commands.md` — `Command.Settings`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Reflection;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SettingStore {
    public static Fin<SettingAnswer> Commit(SettingOperation operation) {
        return from request in Admit.Need(operation)
               from active in Admit(request)
               from node in Resolve(active.Route.Path, active.Route.Children)
               from answer in Execute(node, active)
               select answer;
    }

    public static Fin<Subscription> Observe(
        PlugIn plugIn,
        SavedSettingsRoot source,
        SettingPath path,
        Action<Fin<SettingsSaved>> sink) {
        return from owner in Optional(plugIn).ToFin(Fail: new KernelFault.MissingContext())
               from root in Admit.Need(source).Bind(value => Admit(value))
               from location in Admit(path)
               from receiver in Admit.Need(sink)
               let handler = new EventHandler<PersistentSettingsSavedEventArgs>((_, args) => ignore(Try.lift(() =>
                   Fin.Succ(value: HostEdge.Side(() => receiver(
                       from node in Try.lift(() => root.Switch<PersistentSettingsSavedEventArgs, PersistentSettings>(
                           state: args,
                           plugInCase: static (state, _) => state.PlugInSettings,
                           commandCase: static (state, command) => state.CommandSettings(command.EnglishCommandName))).Run()
                       from tree in Snapshot(node, location, DepthBudget.Value)
                       select new SettingsSaved(tree, (SaveOrigin)args.SavedByThisRhino))))).Run().Bind(static inner => inner))
               from subscription in Subscription.Attach(
                   subscribe: callback => owner.SettingsSaved += callback,
                   unsubscribe: callback => owner.SettingsSaved -= callback,
                   handler: handler)
               select subscription;
    }

    private static Fin<SettingOperation> Admit(SettingOperation operation) => operation.Switch< Fin<SettingOperation>>(
        readCase: static (read) => Admit.Need(read.Kind)
            .Bind(kind => guard(kind != SettingKind.Enum || read.Legacy.IsEmpty, new KernelFault.InvalidInput()).ToFin().Map(_ => kind))
            .Bind(kind => Admit(read.Legacy).Map(legacy => (Kind: kind, Legacy: legacy)))
            .Bind(state => At(
                read.Path,
                state,
                static (path, key, admitted) => new SettingOperation.ReadCase(path, admitted.Kind, admitted.Legacy))),
        clampCase: static (clamp) => Admit(clamp.Bound)
            .Bind(bound => At(
                clamp.Path,
                (clamp.Fallback, Bound: bound),
                static (path, key, admitted) => new SettingOperation.ClampCase(path, admitted.Fallback, admitted.Bound))),
        putCase: static (put) => Admit.Need(put.Value)
            .Bind(value => At(
                put.Path,
                value,
                static (path, key, admitted) => new SettingOperation.PutCase(path, admitted))),
        deleteCase: static (delete) => At(
            delete.Path,
            unit,
            static (path, key, _) => new SettingOperation.DeleteCase(path)),
        readDefaultCase: static (read) => Admit.Need(read.Kind)
            .Bind(kind => At(
                read.Path,
                kind,
                static (path, key, admitted) => new SettingOperation.ReadDefaultCase(path, admitted))),
        putDefaultCase: static (put) => Admit.Need(put.Value)
            .Bind(value => At(
                put.Path,
                value,
                static (path, key, admitted) => new SettingOperation.PutDefaultCase(path, admitted))),
        metadataCase: static (metadata) => At(
            metadata.Path,
            unit,
            static (path, key, _) => new SettingOperation.MetadataCase(path)),
        hideCase: static (hide) => At(
            hide.Path,
            unit,
            static (path, key, _) => new SettingOperation.HideCase(path)),
        guardCase: static (request) => Admit(request.Guard)
            .Bind(admitted => At(
                request.Path,
                admitted,
                static (path, key, guard) => new SettingOperation.GuardCase(path, guard))),
        changedCase: static (changed) => changed.CompareWith
            .TraverseM(path => Admit(path))
            .As()
            .Bind(compare => At(
                changed.Path,
                compare,
                static (path, admitted) => new SettingOperation.ChangedCase(path, admitted))),
        clearChangedCase: static (clear) => At(
            clear.Path,
            unit,
            static (path, _) => new SettingOperation.ClearChangedCase(path)),
        deleteChildCase: static (delete) => At(
            delete.Path,
            delete.Child,
            unit,
            static (path, child, _) => new SettingOperation.DeleteChildCase(path, child)),
        nodeVisibilityCase: static (visibility) => Admit.Need(visibility.Visibility)
            .Bind(admitted => At(
                visibility.Path,
                admitted,
                static (path, value) => new SettingOperation.NodeVisibilityCase(path, value))),
        treeCase: static (tree) => At(
            tree.Path,
            unit,
            static (path, _) => new SettingOperation.TreeCase(path)));

    private static Fin<SettingPath> Admit(SettingPath path) =>
        from source in Admit.Need(path)
        from root in Admit.Need(source.Root)
            .Bind(value => value.Switch< Fin<SettingsRoot>>(
                applicationCase: static (_, _) => Fin.Succ<SettingsRoot>(new SettingsRoot.ApplicationCase()),
                plugInCase: static (plugIn) => plugIn.Plugin.Admit(op).Map<SettingsRoot>(_ => plugIn),
                commandCase: static (command) => Admit.Need(command.Owner).Map<SettingsRoot>(_ => command)))
        from children in source.Children
            .Map(child => FactoryBridge.Accept<SettingKey>(child.Value))
            .Traverse(static value => value)
        select new SettingPath(root, children);

    private static Fin<Seq<SettingKey>> Admit(Seq<SettingKey> legacy, SettingKey key) =>
        from rows in legacy
            .Map(row => FactoryBridge.Accept<SettingKey>(row.Value))
            .Traverse(static value => value)
            .As()
        from _distinct in guard(rows.Distinct().Count() == rows.Count, new KernelFault.InvalidInput())
        from _self in guard(!rows.Exists(row => row == key), new KernelFault.InvalidInput())
        select rows;

    private static Fin<SavedSettingsRoot> Admit(SavedSettingsRoot source) => source.Switch< Fin<SavedSettingsRoot>>(
        plugInCase: static (_, _) => Fin.Succ<SavedSettingsRoot>(new SavedSettingsRoot.PlugInCase()),
        commandCase: static (command) => Acceptance.Text(value: command.EnglishCommandName)
            .Map<SavedSettingsRoot>(static name => new SavedSettingsRoot.CommandCase(name)));

    private static Fin<IntegerBound> Admit(IntegerBound bound) => bound.Switch< Fin<IntegerBound>>(
        lowerCase: static (_, row) => Fin.Succ<IntegerBound>(row),
        upperCase: static (_, row) => Fin.Succ<IntegerBound>(row),
        rangeCase: static (row) => guard(row.Floor <= row.Ceiling, new KernelFault.InvalidInput())
            .ToFin()
            .Map<IntegerBound>(_ => row));

    private static Fin<ISettingGuard> Admit(ISettingGuard? source) =>
        Admit.Need(source).Bind(value => Try.lift(() =>
            from kind in Admit.Need(value.Kind)
            from hostType in Admit.Need(value.HostType)
            from _shape in guard(kind.Accepts(hostType), new KernelFault.InvalidInput())
            select value).Run().Bind(static inner => inner));

    private static Fin<SettingOperation> At<T>(
        SettingPath path,
        SettingKey key,
        T state,
        Func<SettingPath, SettingKey, T, SettingOperation> mint) =>
        from location in Admit(path)
        from admittedKey in FactoryBridge.Accept<SettingKey>(key.Value)
        select mint(location, admittedKey, state);

    private static Fin<SettingOperation> At<T>(
        SettingPath path,
        T state,
        Func<SettingPath, T, SettingOperation> mint) => Admit(path).Map(location => mint(location, state));

    private static Fin<SettingAnswer> Execute(PersistentSettings node, SettingOperation operation) =>
        operation.Switch<(PersistentSettings Node), Fin<SettingAnswer>>(
            state: (node),
            readCase: static (s, read) => from adopted in Adopted(s.Node, read.Legacy)
                                          from value in read.Kind.Read(s.Node, read.Legacy)
                                          select (SettingAnswer)new SettingAnswer.ValueCase(value, adopted),
            clampCase: static (s, clamp) => Try.lift(() => clamp.Bound.Clamped(s.Node, clamp.Key, clamp.Fallback)).Run()
                .Bind(resolved => ArchiveValue.Of(resolved))
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(Some(value), None)),
            putCase: static (s, put) => AdmitTarget(s.Node, put.Value).Bind(kind => Mutate(
                put.Path,
                kind,
                CapabilitySet<SettingReach>.All,
                read: () => kind.Read(s.Node, Seq<SettingKey>()),
                write: () => kind.Write(s.Node, put.Value))),
            deleteCase: static (s, delete) => Delete(s.Node, delete),
            readDefaultCase: static (s, read) => read.Kind.ReadDefault(s.Node)
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(value, None)),
            putDefaultCase: static (s, put) => AdmitTarget(s.Node, put.Value).Bind(kind => Mutate(
                put.Path,
                kind,
                kind.Defaults,
                read: () => kind.ReadDefault(s.Node),
                write: () => kind.WriteDefault(s.Node, put.Value))),
            metadataCase: static (s, metadata) => Metadata(s.Node)
                .Map<SettingAnswer>(static value => new SettingAnswer.MetadataCase(value)),
            hideCase: static (s, hide) => Try.lift(() => s.Node.HideSettingFromUserInterface(hide.Key.Value)).Run().Bind(static inner => inner)
                .Bind(_ => Metadata(s.Node))
                .Map<SettingAnswer>(static value => new SettingAnswer.MetadataCase(value)),
            guardCase: static (s, guard) => Register(s.Node, guard),
            changedCase: static (s, changed) => Changed(s.Node, changed),
            clearChangedCase: static (s, _) => Try.lift(() => s.Node.ClearChangedFlag()).Run().Bind(static inner => inner)
                .Map<SettingAnswer>(static _ => new SettingAnswer.ChangedCase(ChangeVerdict.Clean)),
            deleteChildCase: static (s, delete) => DeleteChild(s.Node, delete),
            nodeVisibilityCase: static (s, visibility) => NodeVisibility(s.Node, visibility),
            treeCase: static (s, tree) => Snapshot(s.Node, tree.Path, DepthBudget.Value)
                .Map<SettingAnswer>(static value => new SettingAnswer.TreeCase(value)));

    private static Fin<PersistentSettings> Resolve(SettingPath path, ChildPolicy children) =>
        path.Root.Switch< Fin<PersistentSettings>>(
            applicationCase: static (_) => Try.lift(() => PersistentSettings.RhinoAppSettings).Run(),
            plugInCase: static (plugIn) => plugIn.Plugin.Admit(op)
                .Bind(_ => Try.lift(() => PersistentSettings.FromPlugInId(plugIn.Plugin.ToValue())).Run()),
            commandCase: static (command) => Try.lift(() => command.Owner.Settings).Run())
        .Bind(root => path.Children.Fold(
            Fin.Succ(value: root),
            (state, child) => state.Bind(parent => Try.lift(() =>
                Admit.Probe<PersistentSettings>((out PersistentSettings found) => parent.TryGetChild(child.Value, out found))
                    .Match(
                        Some: found => Fin.Succ(value: found),
                        None: () => children.Key
                            ? Fin.Succ(value: parent.AddChild(child.Value))
                            : Fin.Fail<PersistentSettings>(error: new KernelFault.MissingContext()))).Run().Bind(static inner => inner))));

    private static Fin<SettingAnswer> Mutate(
        SettingPath path,
        SettingKey key,
        SettingKind kind,
        CapabilitySet<SettingReach> reach,
        Func<Fin<Option<ArchiveValue>>> read,
        Func<Fin<Unit>> write) => reach.Admits(SettingReach.Read)
        ? Observe(path, key, kind, read, write)
        : write().Map(_ => (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutation(
            path,
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
            Succ: current => (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutation(
                path,
                new SettingObservation.ObservedCase(prior),
                new SettingObservation.ObservedCase(current),
                Same(prior, current) ? SettingDelta.Unchanged : SettingDelta.Changed)),
            Fail: fault => new SettingAnswer.MutationCase(new SettingMutation(
                path,
                new SettingObservation.ObservedCase(prior),
                new SettingObservation.FaultedCase(kind, fault),
                SettingDelta.Unobserved)));

    private static Fin<Option<SettingKey>> Adopted(PersistentSettings node, SettingKey key, Seq<SettingKey> legacy) =>
        legacy.IsEmpty
            ? Fin.Succ(value: Option<SettingKey>.None)
            : Try.lift(() => SeatedType(node).IsSome
                ? Option<SettingKey>.None
                : legacy.Find(row => SeatedType(node, row).IsSome)).Run();

    private static Option<Type> SeatedType(PersistentSettings node, SettingKey key) =>
        Admit.Probe<Type>((out Type value) => node.TryGetSettingType(key.Value, out value));

    private static Fin<SettingKind> AdmitTarget(PersistentSettings node, SettingKey key, ArchiveValue value) =>
        from kind in SettingKind.For(value)
        from existing in Try.lift(() => SeatedType(node)).Run()
        from _compatible in existing
            .TraverseM(found => guard(kind.Accepts(found, value), new KernelFault.InvalidInput()).ToFin())
            .As()
            .Map(static _ => unit)
        select kind;

    private static bool Same(Option<ArchiveValue> left, Option<ArchiveValue> right) => (left, right) switch {
        ({ IsSome: false }, { IsSome: false }) => true,
        ({ IsSome: true } prior, { IsSome: true } current) => prior.Value.Same(current.Value),
        _ => false,
    };

    private static Fin<SettingAnswer> Delete(PersistentSettings node, SettingOperation.DeleteCase request) =>
        from type in Try.lift(() => SeatedType(node, request.Key)).Run()
        from prior in type.Match(
            Some: found => SettingKind.For(found)
                .Bind(kind => kind.Read(node, Seq<SettingKey>())),
            None: () => Fin.Succ(value: Option<ArchiveValue>.None))
        from _ in Try.lift(() => node.DeleteItem(request.Key.Value)).Run().Bind(static inner => inner)
        from _absent in Try.lift(() => guard(SeatedType(node, request.Key).IsNone, new KernelFault.InvalidResult()).ToFin()).Run().Bind(static inner => inner)
        select (SettingAnswer)new SettingAnswer.MutationCase(new SettingMutation(
            request.Path,
            request.Key,
            new SettingObservation.ObservedCase(prior),
            new SettingObservation.ObservedCase(None),
            prior.IsSome ? SettingDelta.Changed : SettingDelta.Unchanged));

    private static Fin<SettingAnswer> DeleteChild(PersistentSettings node, SettingOperation.DeleteChildCase request) =>
        from before in Visibility(node)
        from _present in Try.lift(() => guard(Child(node, request.Child).IsSome, new KernelFault.MissingContext()).ToFin()).Run().Bind(static inner => inner)
        from _delete in Try.lift(() => node.DeleteChild(request.Child.Value)).Run().Bind(static inner => inner)
        from _absent in Try.lift(() => guard(
            Child(node, request.Child).IsNone,
            new KernelFault.InvalidResult(Detail: Some($"Settings child '{request.Child.Value}' survived deletion."))).ToFin()).Run().Bind(static inner => inner)
        from after in Visibility(node)
        select (SettingAnswer)new SettingAnswer.NodeCase(new NodeMutation(
            request.Path,
            Some(request.Child),
            before,
            after));

    private static Fin<SettingAnswer> NodeVisibility(
        PersistentSettings node,
        SettingOperation.NodeVisibilityCase request) =>
        from before in Visibility(node)
        from _write in Try.lift(() => node.HiddenFromUserInterface = request.Visibility.Key).Run().Bind(static inner => inner)
        from after in Visibility(node)
        from _proof in guard(
            after == request.Visibility,
            new KernelFault.InvalidResult(Detail: Some("Settings node visibility postcondition failed.")))
        select (SettingAnswer)new SettingAnswer.NodeCase(new NodeMutation(request.Path, None, before, after));

    private static Fin<SettingsVisibility> Visibility(PersistentSettings node) =>
        Try.lift(() => (SettingsVisibility)node.HiddenFromUserInterface).Run();

    private static Option<PersistentSettings> Child(PersistentSettings node, SettingKey key) =>
        Admit.Probe<PersistentSettings>((out PersistentSettings found) => node.TryGetChild(key.Value, out found));

    private static Fin<SettingMetadata> Metadata(PersistentSettings node, SettingKey key) =>
        Try.lift(() => (
                Runtime: node.GetSettingType(key.Value),
                Traits: Held(
                    (node.GetSettingIsReadOnly(key.Value), SettingTrait.ReadOnly),
                    (node.GetSettingIsHiddenFromUserInterface(key.Value), SettingTrait.Hidden)))).Run()
            .Map(read => new SettingMetadata(read.Runtime, SettingKind.For(read.Runtime).ToOption(), read.Traits));

    private static CapabilitySet<SettingTrait> Held(params ReadOnlySpan<(bool Holds, SettingTrait Trait)> rows) =>
        CapabilitySet<SettingTrait>.Of(rows
            .ToArray()
            .Where(static row => row.Holds)
            .Select(static row => row.Trait)
            .ToArray());

    private static readonly Option<MethodInfo> ValidatorTemplate = Optional(typeof(SettingStore).GetMethod(
        nameof(RegisterTyped),
        BindingFlags.NonPublic | BindingFlags.Static));

    private static readonly Atom<HashMap<Type, Func<PersistentSettings, string, ISettingGuard, Fin<Unit>>>> ValidatorWriters =
        Atom(HashMap<Type, Func<PersistentSettings, string, ISettingGuard, Fin<Unit>>>());

    private static readonly Atom<HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>> GuardSeats =
        Atom(HashMap<(SettingPath Path, SettingKey Key), SettingGuardSeat>());

    private static Fin<Func<PersistentSettings, string, ISettingGuard, Fin<Unit>>> ValidatorWriter(Type hostType) =>
        ValidatorWriters.Value.Find(hostType).Match(
            Some: static held => Fin.Succ(value: held),
            None: () => ValidatorTemplate
                .ToFin(Fail: new KernelFault.MissingContext())
                .Bind(open => Try.lift(() => open
                    .MakeGenericMethod(hostType)
                    .CreateDelegate<Func<PersistentSettings, string, ISettingGuard, Fin<Unit>>>()).Run())
                .Map(minted => Cell.Claim(cell: ValidatorWriters, key: hostType, mint: () => minted).Current[hostType]));

    private static Fin<SettingAnswer> Register(PersistentSettings node, SettingOperation.GuardCase request) {
        (SettingPath Path, SettingKey Key) seat = (request.Path, request.Key);
        return Cell.Claim(
                cell: GuardSeats,
                mint: () => new SettingGuardSeat(request.Path, request.Key, request.Guard.Kind))
            .Switch(
                state: (Node: node, Request: request, Seat: seat),
                committed: static (ctx, landed) => ValidatorWriter(ctx.Request.Guard.HostType)
                    .Bind(write => write(ctx.Node, ctx.Request.Key.Value, ctx.Request.Guard))
                    .Map(_ => (SettingAnswer)new SettingAnswer.GuardCase(landed.State[ctx.Seat]))
                    .Rollback(
                        release: () => Cell.Step(
                            cell: GuardSeats,
                            step: held => held.ContainsKey(ctx.Seat) ? Some(held.Remove(ctx.Seat)) : None,
                            declined: new KernelFault.InvalidContext())
                            .Switch(
                                committed: static _ => Fin.Succ(value: unit),
                                ceded: _ => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()),
                                refused: static row => Fin.Fail<Unit>(error: row.Cause),
                                contended: _ => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()))),
                ceded: static (ctx, _) => Fin.Fail<SettingAnswer>(error: new KernelFault.InvalidResult(Detail: Some($"Settings validator '{ctx.Request.Key.Value}' is already seated."))),
                refused: static (_, declined) => Fin.Fail<SettingAnswer>(error: declined.Cause),
                contended: static (ctx, _) => Fin.Fail<SettingAnswer>(error: new KernelFault.InvalidResult()));
    }

    private static Fin<Unit> RegisterTyped<T>(PersistentSettings node, string key, ISettingGuard guard) =>
        Try.lift(() => {
            node.RegisterSettingsValidator<T>((_, args) => ignore(Try.lift(() =>
                from current in guard.Kind.Capture(args.CurrentValue)
                from proposed in guard.Kind.Capture(args.NewValue)
                from accepted in guard.Validate(current, proposed)
                from host in guard.Kind.Host(accepted)
                from _assigned in Assign(args, host)
                select unit).Run().Bind(static inner => inner)
                .BindFail(error => Try.lift(() => {
                    args.Cancel = true;
                    guard.Report(error);
                    return Fin.Succ(value: unit);
                }).Run().Bind(static inner => inner))));
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner);

    private static Fin<Unit> Assign<T>(PersistentSettingsEventArgs<T> args, object? host) => Try.lift(() => {
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

        return Fin.Fail<Unit>(error: new KernelFault.InvalidResult(Detail: Some($"Settings validator host projection '{host?.GetType()}' cannot assign '{typeof(T)}'.")));
    }).Run().Bind(static inner => inner);

    private static Fin<SettingAnswer> Changed(PersistentSettings node, SettingOperation.ChangedCase request) =>
        request.CompareWith.Match(
            Some: path => Resolve(path, ChildPolicy.Require)
                .Bind(other => Try.lift(() => node.ContainsModifiedValues(other)).Run()),
            None: () => Try.lift(() => node.ContainsChangedValues()).Run())
        .Map<SettingAnswer>(static changed => new SettingAnswer.ChangedCase((ChangeVerdict)changed));

    private static readonly Rasm.Numerics.Dimension DepthBudget = Rasm.Numerics.Dimension.Create(value: 32);

    private static Fin<SettingsTree> Snapshot(PersistentSettings node, SettingPath path, int remaining) =>
        remaining <= 0
            ? Fin.Fail<SettingsTree>(error: new KernelFault.InvalidValue(
                Label: nameof(SettingsTree),
                Requirement: $"a settings subtree within {DepthBudget.Value.ToString(CultureInfo.InvariantCulture)} levels"))
            : Try.lift(() =>
                from valueKeys in node.Keys
                    .Map(key => FactoryBridge.Accept<SettingKey>())
                    .Traverse(static value => value)
                    .As()
                    .Map(static keys => toSeq(keys.OrderBy(static key => key.Value, StringComparer.Ordinal)))
                from values in valueKeys
                    .Map(key => Metadata(node))
                    .Traverse(static value => value)
                from childKeys in node.ChildKeys
                    .Map(key => FactoryBridge.Accept<SettingKey>())
                    .Traverse(static value => value)
                    .Map(static keys => toSeq(keys.OrderBy(static key => key.Value, StringComparer.Ordinal)))
                from children in childKeys
                    .Map(admitted => from child in Try.lift(() => node.GetChild(admitted.Value)).Run()
                                     from tree in Snapshot(
                                         child,
                                         path with { Children = path.Children.Add(admitted) },
                                         remaining - 1)
                                     select tree)
                    .Traverse(static value => value)
                select new SettingsTree(
                    path,
                    values,
                    children,
                    Held(
                        (node.HiddenFromUserInterface, SettingTrait.Hidden),
                        (node.ContainsChangedValues(), SettingTrait.Changed)))).Run().Bind(static inner => inner);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
