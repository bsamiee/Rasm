# [PERSISTENT_SETTINGS]

`SettingKind` owns the complete `PersistentSettings` value matrix, including explicit/default asymmetry, as one keyed behavior vocabulary whose rows carry the probe, write, default, capture, and host-projection delegates over the shared `ArchiveValue` carrier (dictionary.md). `SettingOperation` closes reads, writes, defaults, metadata, validators, change state, and saved-tree projection behind `Settings.Commit`.

## [01]-[VALUE_AND_KIND]

`ArchiveValue` carries every payload; `SettingKind` rows are the only site naming a host `TryGet*`/`Set*` member, and each kind adds one complete row whose delegate and type columns drive every boundary projection. `Shape` is the carrier payload type `For` matches, while `HostType` is the host runtime type `Accepts` matches — the two diverge exactly where the detached form differs from the host form (`TextList`, `TextMap`, `OptionalColor`). Rows without a default column refuse with a typed unsupported fault; the enum row rides the shared `EnumMint` reflection seam.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Drawing;
using LanguageExt;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

[SmartEnum<string>]
public sealed partial class SettingDefaultMode
{
    public static readonly SettingDefaultMode None = new("none");
    public static readonly SettingDefaultMode WriteOnly = new("write-only");
    public static readonly SettingDefaultMode ReadWrite = new("read-write");
}

[SmartEnum<string>]
public sealed partial class SettingKind
{
    public static readonly SettingKind Guid = Of<Guid>(
        key: "guid",
        defaults: SettingDefaultMode.WriteOnly,
        probe: static (node, key) => node.TryGetGuid(key, out Guid value) ? Some(value) : None,
        put: static (node, key, value) => node.SetGuid(key, value),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Bool = Of<bool>(
        key: "bool",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetBool(key, out bool value) ? Some(value) : None,
        put: static (node, key, value) => node.SetBool(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out bool value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Byte = Of<byte>(
        key: "byte",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetByte(key, out byte value) ? Some(value) : None,
        put: static (node, key, value) => node.SetByte(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out byte value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Integer = Of<int>(
        key: "integer",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetInteger(key, out int value) ? Some(value) : None,
        put: static (node, key, value) => node.SetInteger(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out int value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind UnsignedInteger = Of<uint>(
        key: "unsigned-integer",
        defaults: SettingDefaultMode.None,
        probe: static (node, key) => node.TryGetUnsignedInteger(key, out uint value) ? Some(value) : None,
        put: static (node, key, value) => node.SetUnsignedInteger(key, value));
    public static readonly SettingKind Double = Of<double>(
        key: "double",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetDouble(key, out double value) ? Some(value) : None,
        put: static (node, key, value) => node.SetDouble(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out double value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Char = Of<char>(
        key: "char",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetChar(key, out char value) ? Some(value) : None,
        put: static (node, key, value) => node.SetChar(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out char value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Text = Of<string>(
        key: "text",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetString(key, out string value) ? Some(value) : None,
        put: static (node, key, value) => node.SetString(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out string value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind TextList = Of<Seq<string>>(
        key: "text-list",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetStringList(key, out string[] value) ? Some(toSeq(value)) : None,
        put: static (node, key, value) => node.SetStringList(key, value.ToArray()),
        probePreset: static (node, key) => node.TryGetDefault(key, out string[] value) ? Some(toSeq(value)) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value.ToArray()),
        capture: static (source, op) => source switch
        {
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
        probe: static (node, key) => node.TryGetStringDictionary(key, out KeyValuePair<string, string>[] value)
            ? Some(value.ToHashMap())
            : None,
        put: static (node, key, value) => node.SetStringDictionary(key, TextMapRows(value)),
        putPreset: static (node, key, value) => node.SetDefault(key, TextMapRows(value)),
        capture: static (source, op) => source switch
        {
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
        probe: static (node, key) => node.TryGetDate(key, out DateTime value) ? Some(value) : None,
        put: static (node, key, value) => node.SetDate(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out DateTime value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Color = Of<Color>(
        key: "color",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetColor(key, out Color value) ? Some(value) : None,
        put: static (node, key, value) => node.SetColor(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Color value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind OptionalColor = Of<Option<Color>>(
        key: "optional-color",
        defaults: SettingDefaultMode.WriteOnly,
        probe: static (node, key) => node.TryGetColor(key, out Color? value) ? Some(Optional(value)) : None,
        put: static (node, key, value) => node.SetColor(key, value.Match<Color?>(Some: static color => color, None: static () => null)),
        putPreset: static (node, key, value) => node.SetDefault(key, value.Match<Color?>(Some: static color => color, None: static () => null)),
        capture: static (source, op) => source switch
        {
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
        probe: static (node, key) => node.TryGetPoint(key, out Point value) ? Some(value) : None,
        put: static (node, key, value) => node.SetPoint(key, value),
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Point3d = Of<Point3d>(
        key: "point3d",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetPoint3d(key, out Point3d value) ? Some(value) : None,
        put: static (node, key, value) => node.SetPoint3d(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Point3d value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Size = Of<Size>(
        key: "size",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetSize(key, out Size value) ? Some(value) : None,
        put: static (node, key, value) => node.SetSize(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Size value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Rectangle = Of<Rectangle>(
        key: "rectangle",
        defaults: SettingDefaultMode.ReadWrite,
        probe: static (node, key) => node.TryGetRectangle(key, out Rectangle value) ? Some(value) : None,
        put: static (node, key, value) => node.SetRectangle(key, value),
        probePreset: static (node, key) => node.TryGetDefault(key, out Rectangle value) ? Some(value) : None,
        putPreset: static (node, key, value) => node.SetDefault(key, value));
    public static readonly SettingKind Enum = new(
        key: "enum",
        defaults: SettingDefaultMode.None,
        shape: typeof(System.Enum),
        hostType: typeof(System.Enum),
        read: static (node, key, op) => Fin.Fail<Option<ArchiveValue>>(error: op.Unsupported(
            geometryType: typeof(System.Enum), outputType: typeof(PersistentSettings))),
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
    internal partial Fin<Option<ArchiveValue>> Read(PersistentSettings node, SettingKey key, Op op);

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

    internal static Fin<Option<ArchiveValue>> ReadEnum(PersistentSettings source, SettingKey key, Type enumType, Op op) =>
        from _shape in guard(enumType.IsEnum, op.InvalidInput()).ToFin()
        from method in op.Catch(() => Optional(typeof(SettingKind).GetMethod(
                nameof(ReadEnumTyped),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            .ToFin(Fail: op.MissingContext())
            .Map(open => open.MakeGenericMethod(enumType)))
        from result in op.Catch(() => method.Invoke(null, [source, key.Value, op]) is Fin<Option<ArchiveValue>> typed
            ? typed
            : Fin.Fail<Option<ArchiveValue>>(error: op.InvalidResult()))
        select result;

    private static Fin<Option<ArchiveValue>> ReadEnumTyped<T>(PersistentSettings source, string key, Op op)
        where T : struct, IConvertible =>
        source.TryGetEnumValue(key, out T value)
            ? ArchiveValue.Enum(value, op).Map(Some)
            : Fin.Succ(value: Option<ArchiveValue>.None);

    private static SettingKind Of<T>(
        string key,
        SettingDefaultMode defaults,
        Func<PersistentSettings, string, Option<T>> probe,
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
            read: (node, settingKey, op) => op.Catch(() => probe(node, settingKey.Value).Match(
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

`SettingPath` carries root and child identity once. `Route` derives missing-child creation from the active operation, so reads never mutate the tree as an accidental consequence.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Domain;
using Rhino;
using Thinktecture;

[ValueObject<string>]
public readonly partial struct SettingKey
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Setting key is empty.");
            return;
        }

        value = value.Trim();
        validationError = null;
    }
}

[Union]
public abstract partial record SettingsRoot
{
    public sealed record ApplicationCase : SettingsRoot;
    public sealed record PlugInCase(Guid PlugInId) : SettingsRoot;
}

public sealed record SettingPath(SettingsRoot Root, Seq<SettingKey> Children);

public interface ISettingGuard
{
    SettingKind Kind { get; }
    Type HostType { get; }
    Fin<ArchiveValue> Validate(ArchiveValue current, ArchiveValue proposed);
    void Report(Error error);
}

[SmartEnum<string>]
public sealed partial class SettingsVisibility
{
    public static readonly SettingsVisibility Visible = new("visible", false);
    public static readonly SettingsVisibility Hidden = new("hidden", true);
    internal bool IsHidden { get; }
}

[Union]
public abstract partial record SettingOperation
{
    public sealed record ReadCase(SettingPath Path, SettingKey Key, SettingKind Kind) : SettingOperation;
    public sealed record ReadEnumCase(SettingPath Path, SettingKey Key, Type EnumType) : SettingOperation;
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

[Union]
public abstract partial record SettingObservation
{
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

public sealed record SettingNodeReceipt(
    SettingPath Path,
    Option<SettingKey> Child,
    bool PriorHidden,
    bool CurrentHidden,
    bool ChildDeleted);

[Union]
public abstract partial record SavedSettingsRoot
{
    public sealed record PlugInCase : SavedSettingsRoot;
    public sealed record CommandCase(string EnglishCommandName) : SavedSettingsRoot;
}

[Union]
public abstract partial record SettingAnswer
{
    public sealed record ValueCase(Option<ArchiveValue> Value) : SettingAnswer;
    public sealed record MutationCase(SettingMutationReceipt Receipt) : SettingAnswer;
    public sealed record MetadataCase(SettingMetadata Metadata) : SettingAnswer;
    public sealed record ChangedCase(bool Changed) : SettingAnswer;
    public sealed record GuardCase(SettingKind Kind) : SettingAnswer;
    public sealed record NodeCase(SettingNodeReceipt Receipt) : SettingAnswer;
    public sealed record TreeCase(SettingsTree Tree) : SettingAnswer;
}
```

## [03]-[INTERPRETER]

`Settings.Commit` resolves exactly one node and dispatches the operation exhaustively. Mutation receipts read explicit values without invoking defaulted getters; write-only defaults report `UnobservableCase` instead of inventing evidence.

`SettingOperation` derives path and creation policy once before resolution. Persistent-settings writes, validator callbacks, tree mutation, and saved-event adaptation form the platform-forced statement seam; generated dispatch keeps value, operation, and saved-root families exhaustive around it, and every host crossing rides `Op.Catch` onto typed faults.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;
using Thinktecture;
using static LanguageExt.Prelude;

public static class Settings
{
    public static Fin<SettingAnswer> Commit(SettingOperation operation, Op? key = null)
    {
        Op op = key.OrDefault();
        return from request in Optional(operation).ToFin(Fail: op.InvalidInput())
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
        Op? key = null)
    {
        Op op = key.OrDefault();
        return from owner in Optional(plugIn).ToFin(Fail: op.MissingContext())
               from root in Optional(source).ToFin(Fail: op.InvalidInput()).Bind(value => Admit(value, op))
               from location in Admit(path, op)
               from receiver in Optional(sink).ToFin(Fail: op.InvalidInput())
               let handler = new EventHandler<PersistentSettingsSavedEventArgs>((_, args) => _ = op.Catch(() =>
               {
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
        readCase: static (op, read) => Optional(read.Kind).ToFin(Fail: op.InvalidInput())
            .Bind(kind => At(
                read.Path,
                read.Key,
                kind,
                static (path, key, admitted) => new SettingOperation.ReadCase(path, key, admitted),
                op)),
        readEnumCase: static (op, read) => Optional(read.EnumType).ToFin(Fail: op.InvalidInput())
            .Bind(type => guard(type.IsEnum, op.InvalidInput()).ToFin().Map(_ => type))
            .Bind(type => At(
                read.Path,
                read.Key,
                type,
                static (path, key, admitted) => new SettingOperation.ReadEnumCase(path, key, admitted),
                op)),
        putCase: static (op, put) => Optional(put.Value).ToFin(Fail: op.InvalidInput())
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
        readDefaultCase: static (op, read) => Optional(read.Kind).ToFin(Fail: op.InvalidInput())
            .Bind(kind => At(
                read.Path,
                read.Key,
                kind,
                static (path, key, admitted) => new SettingOperation.ReadDefaultCase(path, key, admitted),
                op)),
        putDefaultCase: static (op, put) => Optional(put.Value).ToFin(Fail: op.InvalidInput())
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
        nodeVisibilityCase: static (op, visibility) => Optional(visibility.Visibility).ToFin(Fail: op.InvalidInput())
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
        from source in Optional(path).ToFin(Fail: op.InvalidInput())
        from root in Optional(source.Root).ToFin(Fail: op.InvalidInput())
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

    private static Fin<SavedSettingsRoot> Admit(SavedSettingsRoot source, Op op) => source.Switch<Op, Fin<SavedSettingsRoot>>(
        state: op,
        plugInCase: static (_, _) => Fin.Succ<SavedSettingsRoot>(new SavedSettingsRoot.PlugInCase()),
        commandCase: static (op, command) => op.AcceptText(value: command.EnglishCommandName)
            .Map<SavedSettingsRoot>(static name => new SavedSettingsRoot.CommandCase(name)));

    private static Fin<ISettingGuard> Admit(ISettingGuard? source, Op op) =>
        Optional(source).ToFin(Fail: op.InvalidInput()).Bind(value => op.Catch(() =>
            from kind in Optional(value.Kind).ToFin(Fail: op.InvalidInput())
            from hostType in Optional(value.HostType).ToFin(Fail: op.InvalidInput())
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
            readCase: static (s, read) => read.Kind.Read(s.Node, read.Key, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(value)),
            readEnumCase: static (s, read) => SettingKind.ReadEnum(s.Node, read.Key, read.EnumType, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(value)),
            putCase: static (s, put) => AdmitTarget(s.Node, put.Key, put.Value, s.Op).Bind(kind => Mutate(
                put.Path,
                put.Key,
                kind,
                SettingDefaultMode.ReadWrite,
                read: () => kind.Read(s.Node, put.Key, s.Op),
                write: () => kind.Write(s.Node, put.Key, put.Value, s.Op))),
            deleteCase: static (s, delete) => Delete(s.Node, delete, s.Op),
            readDefaultCase: static (s, read) => read.Kind.ReadDefault(s.Node, read.Key, s.Op)
                .Map<SettingAnswer>(static value => new SettingAnswer.ValueCase(value)),
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
        readEnumCase: static value => (value.Path, false),
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

    private static bool Same(Option<ArchiveValue> left, Option<ArchiveValue> right) => (left, right) switch
    {
        ({ IsSome: false }, { IsSome: false }) => true,
        ({ IsSome: true } prior, { IsSome: true } current) => prior.Value.Same(current.Value),
        _ => false,
    };

    private static Fin<SettingAnswer> Delete(PersistentSettings node, SettingOperation.DeleteCase request, Op op) =>
        from type in op.Catch(() => Fin.Succ(value: node.TryGetSettingType(request.Key.Value, out Type found)
            ? Some(found)
            : Option<Type>.None))
        from prior in type.Match(
            Some: found => found.IsEnum
                ? SettingKind.ReadEnum(node, request.Key, found, op)
                : SettingKind.For(found, op).Bind(kind => kind.Read(node, request.Key, op)),
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

    private static Fin<SettingAnswer> Register(PersistentSettings node, SettingOperation.GuardCase request, Op op) =>
        from method in op.Catch(() => Optional(typeof(Settings).GetMethod(
                nameof(RegisterTyped),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            .ToFin(Fail: op.MissingContext())
            .Map(open => open.MakeGenericMethod(request.Guard.HostType)))
        from _wired in op.Catch(() => method.Invoke(null, [node, request.Key.Value, request.Guard, op]) is Fin<Unit> wired
            ? wired
            : Fin.Fail<Unit>(error: op.InvalidResult(detail: "Settings validator registration returned no receipt.")))
        select (SettingAnswer)new SettingAnswer.GuardCase(request.Guard.Kind);

    private static Fin<Unit> RegisterTyped<T>(PersistentSettings node, string key, ISettingGuard guard, Op op) =>
        op.Catch(() =>
        {
            if (node.GetValidator<T>(key) is not null)
            {
                return Fin.Fail<Unit>(error: op.InvalidResult(detail: $"Settings validator '{key}' is already registered."));
            }

            node.RegisterSettingsValidator<T>(key, (_, args) => _ = op.Catch(() =>
                from current in guard.Kind.Capture(args.CurrentValue, op)
                from proposed in guard.Kind.Capture(args.NewValue, op)
                from accepted in guard.Validate(current, proposed)
                from host in guard.Kind.Host(accepted, op)
                from _assigned in Assign(args, host, op)
                select unit)
                .BindFail(error => op.Catch(() =>
                {
                    args.Cancel = true;
                    guard.Report(error);
                    return Fin.Succ(value: unit);
                })));
            return Fin.Succ(value: unit);
        });

    private static Fin<Unit> Assign<T>(PersistentSettingsEventArgs<T> args, object? host, Op op) => op.Catch(() =>
    {
        if (host is T typed)
        {
            args.CurrentValue = typed;
            return Fin.Succ(value: unit);
        }

        if (host is not null && Nullable.GetUnderlyingType(typeof(T)) == host.GetType())
        {
            args.CurrentValue = (T)host;
            return Fin.Succ(value: unit);
        }

        if (host is null && default(T) is null)
        {
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
            .Map(static keys => keys.OrderBy(static key => key.Value, StringComparer.Ordinal).ToSeq())
        from values in valueKeys
            .Map(key => Metadata(node, key, op))
            .Traverse(static value => value)
        from childKeys in node.ChildKeys
            .Map(key => op.AcceptValidated<SettingKey>(key))
            .Traverse(static value => value)
            .Map(static keys => keys.OrderBy(static key => key.Value, StringComparer.Ordinal).ToSeq())
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

Explicit reads use `TryGet*` and never call mutating defaulted getters. `AdmitTarget` compares each payload row with the existing host type, including exact enum identity, before explicit or default writes. One mutation fold owns observable and write-only receipts; failed post-write reads land as `FaultedCase` evidence, and deletion emits absence only after a host re-probe.

## [05]-[SEAMS]

`ArchiveValue` (dictionary.md) is the one payload carrier across this boundary — `SettingKind` rows lift host values through `ArchiveValue.Of`, lower through `Project<T>`, and mint enum payloads through the shared `EnumMint` seam. `SettingsTree` admits and orders value and child keys before recursive projection. `PlugIn.SettingsSaved` observation encloses root projection and sink delivery in one catch frame under the Document subscription owner; this page owns no parallel event lifecycle.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
