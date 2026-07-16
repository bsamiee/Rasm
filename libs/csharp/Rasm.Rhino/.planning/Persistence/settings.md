# [RASM_RHINO_PERSISTENCE_SETTINGS]

Typed settings custody (`Rasm.Rhino.Persistence`). `SettingsScope` resolves the plugin or application root and one child path. `SettingValue` carries one admitted payload plus its codec row; codec generators cover the full explicit writer, pure `TryGet`, default-writer, and default-probe matrices without case-per-type DTOs. `SettingRequest` is the sole operation vocabulary, and `Settings.Run` resolves one node before total dispatch. Pure reads never call a defaulted `Get` member because those members stamp defaults and materialize missing keys.

## [01]-[INDEX]

- [02]-[SCOPE]: root and child-path custody.
- [03]-[VALUE_CODEC]: one value owner and the generated host matrix.
- [04]-[REQUEST_RAIL]: mutations, pure questions, validators, metadata, and change evidence.
- [05]-[SAVED_WATCH]: detached saved-event trees and subscription custody.
- [06]-[SURFACE_LEDGER]: ownership and entry points.

## [02]-[SCOPE]

- Owner: `SettingsRoot` is the closed root choice; `SettingsScope` combines one root with an admitted child path.
- Resolution: read-only programs walk `TryGetChild`; path-creating programs walk `AddChild`. Operation semantics derive creation, so no caller grant or mint flag exists.
- Boundary: `PersistentSettings` never escapes a run or saved-event callback.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Immutable;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [SCOPE] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsRoot {
    private SettingsRoot() { }
    public sealed record Plugin(Guid Id) : SettingsRoot;
    public sealed record Application : SettingsRoot;

    internal Fin<PersistentSettings> Resolve(Op op) =>
        Switch(
            op,
            plugin: static (key, root) => root.Id == Guid.Empty
                ? Fin.Fail<PersistentSettings>(error: key.InvalidInput())
                : key.Catch(() => Fin.Succ(value: PersistentSettings.FromPlugInId(pluginId: root.Id))),
            application: static (key, _) => key.Catch(() =>
                Optional(PersistentSettings.RhinoAppSettings).ToFin(Fail: key.MissingContext())));
}

public sealed record SettingsScope {
    private SettingsScope(SettingsRoot root, Seq<string> path) => (Root, Path) = (root, path);

    public SettingsRoot Root { get; }
    public Seq<string> Path { get; }

    public static Fin<SettingsScope> Create(SettingsRoot root, params ReadOnlySpan<string> path) {
        Op op = Op.Of();
        return from source in Optional(root).ToFin(Fail: op.InvalidInput())
               from segments in toSeq(path.ToArray()).TraverseM(segment => op.AcceptText(value: segment)).As()
               select new SettingsScope(root: source, path: segments);
    }

    internal Fin<PersistentSettings> Resolve(bool createPath, Op op) =>
        Root.Resolve(op: op).Bind(node => Path.Fold(
            Fin.Succ(value: node),
            (state, segment) => state.Bind(parent => createPath
                ? op.Catch(() => Fin.Succ(value: parent.AddChild(key: segment)))
                : op.Catch(() => parent.TryGetChild(segment, out PersistentSettings child)
                    ? Fin.Succ(value: child)
                    : Fin.Fail<PersistentSettings>(error: op.MissingContext())))));
}
```

## [03]-[VALUE_CODEC]

- Owner: `SettingValue` is one settings-value identity. Its retained codec discriminates payloads whose boxed runtime forms collide, including `Color` and `Color?`.
- Coverage: codec rows admit `Guid`, every scalar, both color forms, string collections, drawing values, and `Point3d`; repeated capability shapes derive through `Explicit`, `Defaulted`, and `Full` generators.
- Purity: value reads call only `TryGet` members, including their legacy-key-list overloads. Default reads call only `TryGetDefault` and return `None` when the host exposes no probe.
- Defaults: unsupported default writers fail before host dispatch; no no-op arm represents a missing overload.
- Lists: `StringList` optionally inserts `PersistentSettings.StringListRootKey` as a list element, preserving host all-users splicing without exposing the sentinel to callers.

```csharp signature
// --- [VALUE_OWNER] --------------------------------------------------------------------------
public sealed class SettingValue : IDetachedDocumentResult {
    private readonly ISettingCodec _codec;
    private readonly object? _payload;

    private SettingValue(ISettingCodec codec, object? payload) => (_codec, _payload) = (codec, payload);

    public Type ValueType => _codec.ValueType;

    public static Fin<SettingValue> Create<T>(T value, Op? key = null) {
        Op op = key.OrDefault();
        return from codec in SettingCodecs.Resolve(type: typeof(T), op: op)
               from captured in codec.Capture(source: value, op: op)
               select new SettingValue(codec: codec, payload: captured);
    }

    public static Fin<SettingValue> StringList(Seq<string> values, Option<int> inheritAt = default, Op? key = null) {
        Op op = key.OrDefault();
        string[] held = values.ToArray();
        return inheritAt.Match(
            Some: index => index >= 0 && index <= held.Length
                ? Create(value: held.Take(index)
                    .Append(PersistentSettings.StringListRootKey)
                    .Concat(held.Skip(index))
                    .ToArray(), key: op)
                : Fin.Fail<SettingValue>(error: op.InvalidInput()),
            None: () => Create(value: held, key: op));
    }

    public Fin<T> As<T>(Op? key = null) {
        Op op = key.OrDefault();
        if (ValueType != typeof(T)) return Fin.Fail<T>(error: op.Unsupported(geometryType: ValueType, outputType: typeof(T)));
        return _payload is null
            ? Fin.Succ(value: default(T)!)
            : op.Catch(() => Fin.Succ(value: (T)_payload));
    }

    internal Fin<Unit> Write(PersistentSettings node, string name, bool asDefault, Op op) =>
        _codec.Write(node: node, name: name, payload: _payload, asDefault: asDefault, op: op);

    internal static Fin<Option<SettingValue>> Read(
        PersistentSettings node,
        string name,
        Type type,
        Seq<string> legacyNames,
        bool fromDefault,
        Op op) =>
        from codec in SettingCodecs.Resolve(type: type, op: op)
        from probe in codec.Read(node: node, name: name, legacyNames: legacyNames, fromDefault: fromDefault, op: op)
        select probe.Found
            ? Some(new SettingValue(codec: codec, payload: probe.Value))
            : Option<SettingValue>.None;
}

internal readonly record struct SettingProbe(bool Found, object? Value);

internal interface ISettingCodec {
    Type ValueType { get; }
    Fin<object?> Capture(object? source, Op op);
    Fin<SettingProbe> Read(PersistentSettings node, string name, Seq<string> legacyNames, bool fromDefault, Op op);
    Fin<Unit> Write(PersistentSettings node, string name, object? payload, bool asDefault, Op op);
}

internal static class SettingCodecs {
    private sealed class Codec<T>(
        Func<T, T> freeze,
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write,
        Option<Action<PersistentSettings, string, T>> writeDefault,
        Option<Func<PersistentSettings, string, (bool Found, T Value)>> readDefault) : ISettingCodec {
        public Type ValueType => typeof(T);

        public Fin<object?> Capture(object? source, Op op) =>
            source is T value
                ? op.Catch(() => Fin.Succ<object?>(value: freeze(value)))
                : source is null && Nullable.GetUnderlyingType(typeof(T)) is not null
                    ? Fin.Succ<object?>(value: null)
                    : Fin.Fail<object?>(error: op.InvalidInput());

        public Fin<SettingProbe> Read(
            PersistentSettings node,
            string name,
            Seq<string> legacyNames,
            bool fromDefault,
            Op op) =>
            op.Catch(() => fromDefault
                ? readDefault.Match(
                    Some: probe => ToProbe(probe(node, name)),
                    None: static () => Fin.Succ(value: new SettingProbe(Found: false, Value: null)))
                : ToProbe(read(node, name, legacyNames.AsEnumerable())));

        public Fin<Unit> Write(PersistentSettings node, string name, object? payload, bool asDefault, Op op) =>
            Capture(source: payload, op: op).Bind(captured => op.Catch(() => {
                T value = captured is null ? default! : (T)captured;
                return asDefault
                    ? writeDefault.Match(
                        Some: apply => Fin.Succ(value: Op.Side(() => apply(node, name, value))),
                        None: () => Fin.Fail<Unit>(error: op.Unsupported(geometryType: typeof(T), outputType: typeof(PersistentSettings))))
                    : Fin.Succ(value: Op.Side(() => write(node, name, value)));
            }));

        private Fin<SettingProbe> ToProbe((bool Found, T Value) probe) =>
            Fin.Succ(value: new SettingProbe(
                Found: probe.Found,
                Value: probe.Found ? freeze(probe.Value) : null));
    }

    private static readonly ImmutableArray<ISettingCodec> Rows = [
        Defaulted<Guid>(Read(static (PersistentSettings n, string k, out Guid v, IEnumerable<string> l) => n.TryGetGuid(k, out v, l)), static (n, k, v) => n.SetGuid(k, v), static (n, k, v) => n.SetDefault(k, v)),
        Full<bool>(Read(static (PersistentSettings n, string k, out bool v, IEnumerable<string> l) => n.TryGetBool(k, out v, l)), static (n, k, v) => n.SetBool(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out bool v) => n.TryGetDefault(k, out v))),
        Full<byte>(Read(static (PersistentSettings n, string k, out byte v, IEnumerable<string> l) => n.TryGetByte(k, out v, l)), static (n, k, v) => n.SetByte(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out byte v) => n.TryGetDefault(k, out v))),
        Full<int>(Read(static (PersistentSettings n, string k, out int v, IEnumerable<string> l) => n.TryGetInteger(k, out v, l)), static (n, k, v) => n.SetInteger(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out int v) => n.TryGetDefault(k, out v))),
        Explicit<uint>(Read(static (PersistentSettings n, string k, out uint v, IEnumerable<string> l) => n.TryGetUnsignedInteger(k, out v, l)), static (n, k, v) => n.SetUnsignedInteger(k, v)),
        Full<double>(Read(static (PersistentSettings n, string k, out double v, IEnumerable<string> l) => n.TryGetDouble(k, out v, l)), static (n, k, v) => n.SetDouble(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out double v) => n.TryGetDefault(k, out v))),
        Full<char>(Read(static (PersistentSettings n, string k, out char v, IEnumerable<string> l) => n.TryGetChar(k, out v, l)), static (n, k, v) => n.SetChar(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out char v) => n.TryGetDefault(k, out v))),
        Full<string>(Read(static (PersistentSettings n, string k, out string v, IEnumerable<string> l) => n.TryGetString(k, out v, l)), static (n, k, v) => n.SetString(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out string v) => n.TryGetDefault(k, out v))),
        Full<string[]>(Read(static (PersistentSettings n, string k, out string[] v, IEnumerable<string> l) => n.TryGetStringList(k, out v, l)), static (n, k, v) => n.SetStringList(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out string[] v) => n.TryGetDefault(k, out v)), static v => [.. v]),
        Defaulted<KeyValuePair<string, string>[]>(Read(static (PersistentSettings n, string k, out KeyValuePair<string, string>[] v, IEnumerable<string> l) => n.TryGetStringDictionary(k, out v, l)), static (n, k, v) => n.SetStringDictionary(k, v), static (n, k, v) => n.SetDefault(k, v), static v => [.. v]),
        Full<DateTime>(Read(static (PersistentSettings n, string k, out DateTime v, IEnumerable<string> l) => n.TryGetDate(k, out v, l)), static (n, k, v) => n.SetDate(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out DateTime v) => n.TryGetDefault(k, out v))),
        Full<System.Drawing.Color>(Read(static (PersistentSettings n, string k, out System.Drawing.Color v, IEnumerable<string> l) => n.TryGetColor(k, out v, l)), static (n, k, v) => n.SetColor(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out System.Drawing.Color v) => n.TryGetDefault(k, out v))),
        Defaulted<System.Drawing.Color?>(Read(static (PersistentSettings n, string k, out System.Drawing.Color? v, IEnumerable<string> l) => n.TryGetColor(k, out v, l)), static (n, k, v) => n.SetColor(k, v), static (n, k, v) => n.SetDefault(k, v)),
        Defaulted<System.Drawing.Point>(Read(static (PersistentSettings n, string k, out System.Drawing.Point v, IEnumerable<string> l) => n.TryGetPoint(k, out v, l)), static (n, k, v) => n.SetPoint(k, v), static (n, k, v) => n.SetDefault(k, v)),
        Full<Point3d>(Read(static (PersistentSettings n, string k, out Point3d v, IEnumerable<string> l) => n.TryGetPoint3d(k, out v, l)), static (n, k, v) => n.SetPoint3d(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out Point3d v) => n.TryGetDefault(k, out v))),
        Full<System.Drawing.Size>(Read(static (PersistentSettings n, string k, out System.Drawing.Size v, IEnumerable<string> l) => n.TryGetSize(k, out v, l)), static (n, k, v) => n.SetSize(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out System.Drawing.Size v) => n.TryGetDefault(k, out v))),
        Full<System.Drawing.Rectangle>(Read(static (PersistentSettings n, string k, out System.Drawing.Rectangle v, IEnumerable<string> l) => n.TryGetRectangle(k, out v, l)), static (n, k, v) => n.SetRectangle(k, v), static (n, k, v) => n.SetDefault(k, v), Default(static (PersistentSettings n, string k, out System.Drawing.Rectangle v) => n.TryGetDefault(k, out v)))
    ];

    private delegate bool Reader<T>(PersistentSettings node, string name, out T value, IEnumerable<string> legacyNames);
    private delegate bool DefaultReader<T>(PersistentSettings node, string name, out T value);

    internal static Fin<ISettingCodec> Resolve(Type type, Op op) =>
        toSeq(Rows).Find(codec => codec.ValueType == type).ToFin(
            Fail: op.Unsupported(geometryType: type, outputType: typeof(SettingValue)));

    private static Func<PersistentSettings, string, IEnumerable<string>, (bool, T)> Read<T>(Reader<T> read) =>
        (node, name, legacy) => (read(node, name, out T value, legacy), value);

    private static Func<PersistentSettings, string, (bool, T)> Default<T>(DefaultReader<T> read) =>
        (node, name) => (read(node, name, out T value), value);

    private static ISettingCodec Explicit<T>(
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write) =>
        Explicit(read, write, static value => value);

    private static ISettingCodec Explicit<T>(
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write,
        Func<T, T> freeze) =>
        new Codec<T>(freeze, read, write, None, None);

    private static ISettingCodec Defaulted<T>(
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write,
        Action<PersistentSettings, string, T> writeDefault) =>
        Defaulted(read, write, writeDefault, static value => value);

    private static ISettingCodec Defaulted<T>(
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write,
        Action<PersistentSettings, string, T> writeDefault,
        Func<T, T> freeze) =>
        new Codec<T>(freeze, read, write, Some(writeDefault), None);

    private static ISettingCodec Full<T>(
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write,
        Action<PersistentSettings, string, T> writeDefault,
        Func<PersistentSettings, string, (bool Found, T Value)> readDefault) =>
        Full(read, write, writeDefault, readDefault, static value => value);

    private static ISettingCodec Full<T>(
        Func<PersistentSettings, string, IEnumerable<string>, (bool Found, T Value)> read,
        Action<PersistentSettings, string, T> write,
        Action<PersistentSettings, string, T> writeDefault,
        Func<PersistentSettings, string, (bool Found, T Value)> readDefault,
        Func<T, T> freeze) =>
        new Codec<T>(freeze, read, write, Some(writeDefault), Some(readDefault));
}
```

## [04]-[REQUEST_RAIL]

- Owner: `SettingRequest` closes mutation and question verbs under one total dispatch. `Settings.Run` resolves one node, aborts on the first failed operation, and returns ordered answers plus mutation names.
- Clamp: `IntegerClamp` carries lower and upper bounds as one admitted product. Bounded `GetInteger` stays on the mutation side because its fallback overload writes defaults and missing values.
- Guards: host `RegisterSettingsValidator<T>` overwrites an existing registration unconditionally, so `Guard` installs only when `GetValidator<T>` returns null; a duplicate registration fails, and a cross-`T` mismatch throws `InvalidCastException` from the probe and lands on the rail. `Reject` sets `Cancel`, `Coerce` replaces `CurrentValue`, and `Accept` leaves args untouched.
- Metadata: trait reads preserve type, read-only, and hidden absence independently. Host read-only metadata is reported rather than locally enforced.
- Change evidence: changed-baseline, clear, and cross-tree modified probes share the request rail.

```csharp signature
// --- [REQUEST_TYPES] ------------------------------------------------------------------------
public sealed record IntegerClamp {
    private IntegerClamp(Option<int> lower, Option<int> upper) => (Lower, Upper) = (lower, upper);
    public Option<int> Lower { get; }
    public Option<int> Upper { get; }

    public static Fin<IntegerClamp> Create(Option<int> lower, Option<int> upper, Op? key = null) {
        Op op = key.OrDefault();
        return (lower.IsSome || upper.IsSome) && lower.Match(Some: lo => upper.Match(Some: hi => lo <= hi, None: static () => true), None: static () => true)
            ? Fin.Succ(value: new IntegerClamp(lower: lower, upper: upper))
            : Fin.Fail<IntegerClamp>(error: op.InvalidInput());
    }

    internal Fin<int> Apply(PersistentSettings node, string name, int fallback, Op op) =>
        Lower.Match(
            Some: lower => Upper.Match(
                Some: upper => op.Catch(() => Fin.Succ(value: node.GetInteger(name, fallback, lower, upper))),
                None: () => op.Catch(() => Fin.Succ(value: node.GetInteger(name, fallback, lower, boundIsLower: true)))),
            None: () => Upper.Match(
                Some: upper => op.Catch(() => Fin.Succ(value: node.GetInteger(name, fallback, upper, boundIsLower: false))),
                None: () => Fin.Fail<int>(error: op.InvalidInput())));
}

public abstract record GuardVerdict<T> where T : notnull {
    private GuardVerdict() { }
    public sealed record Accept : GuardVerdict<T>;
    public sealed record Reject : GuardVerdict<T>;
    public sealed record Coerce(T Value) : GuardVerdict<T>;
}

public sealed record SettingTrait(Option<Type> StoredType, Option<bool> ReadOnly, Option<bool> Hidden) : IDetachedDocumentResult;

[SmartEnum<int>]
public sealed partial class SettingLayer {
    public static readonly SettingLayer Explicit = new(key: 0);
    public static readonly SettingLayer Default = new(key: 1);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingRequest {
    private SettingRequest() { }
    public sealed record Write(string Name, SettingValue Value, SettingLayer Layer) : SettingRequest;
    public sealed record Read(string Name, Type ValueType, Seq<string> LegacyNames, SettingLayer Layer) : SettingRequest;
    public sealed record Clamp(string Name, int Fallback, IntegerClamp Bounds) : SettingRequest;
    public sealed record Delete(string Name) : SettingRequest;
    public sealed record DropChild(string Name) : SettingRequest;
    public sealed record Hide(string Name) : SettingRequest;
    public sealed record HideNode(bool Hidden) : SettingRequest;
    public sealed record ClearChanged : SettingRequest;
    public sealed record Describe(string Name) : SettingRequest;
    public sealed record Keys : SettingRequest;
    public sealed record Children : SettingRequest;
    public sealed record Changed(Option<SettingsScope> Against = default) : SettingRequest;
    internal sealed record Install(string Name, Func<PersistentSettings, string, Op, Fin<Unit>> Apply) : SettingRequest;
    internal sealed record Custom(
        bool Creates,
        Option<string> Mutates,
        Func<PersistentSettings, Op, Fin<Option<SettingAnswer>>> Apply) : SettingRequest;

    public static Fin<SettingRequest> Put<T>(string name, T value, SettingLayer layer, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: name)
               from target in Optional(layer).ToFin(Fail: op.InvalidInput())
               from held in SettingValue.Create(value: value, key: op)
               select (SettingRequest)new Write(Name: admitted, Value: held, Layer: target);
    }

    public static Fin<SettingRequest> Value<T>(string name, SettingLayer layer, Op? key = null, params ReadOnlySpan<string> legacyNames) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: name)
               from target in Optional(layer).ToFin(Fail: op.InvalidInput())
               from legacy in toSeq(legacyNames.ToArray()).TraverseM(item => op.AcceptText(value: item)).As()
               from _ in SettingCodecs.Resolve(type: typeof(T), op: op)
               select (SettingRequest)new Read(Name: admitted, ValueType: typeof(T), LegacyNames: legacy, Layer: target);
    }

    public static Fin<SettingRequest> PutEnum<T>(string name, T value, Op? key = null) where T : struct, Enum, IConvertible {
        Op op = key.OrDefault();
        return op.AcceptText(value: name).Map(admitted => (SettingRequest)new Custom(
            Creates: true,
            Mutates: Some(admitted),
            Apply: (node, operation) => operation.Catch(() => {
                node.SetEnumValue(admitted, value);
                return Fin.Succ(value: Option<SettingAnswer>.None);
            })));
    }

    public static Fin<SettingRequest> EnumValue<T>(string name, Op? key = null) where T : struct, Enum, IConvertible {
        Op op = key.OrDefault();
        return op.AcceptText(value: name).Map(admitted => (SettingRequest)new Custom(
            Creates: false,
            Mutates: None,
            Apply: (node, operation) => operation.Catch(() => Fin.Succ<Option<SettingAnswer>>(
                value: Some<SettingAnswer>(new SettingAnswer.Projected(
                    ValueType: typeof(T),
                    Value: node.TryGetEnumValue(admitted, out T value) ? Some<object>(value) : None))))));
    }

    public static Fin<SettingRequest> Guard<T>(string name, Func<(T Current, T Next), GuardVerdict<T>> decide, Op? key = null) where T : notnull {
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: name)
               from body in Optional(decide).ToFin(Fail: op.InvalidInput())
               select (SettingRequest)new Install(Name: admitted, Apply: (node, slot, operation) => operation.Catch(() => {
                   if (node.GetValidator<T>(slot) is not null) return Fin.Fail<Unit>(error: operation.InvalidInput());
                   node.RegisterSettingsValidator<T>(slot, (_, args) => {
                       GuardVerdict<T> verdict = body((args.CurrentValue, args.NewValue));
                       args.Cancel = verdict is GuardVerdict<T>.Reject;
                       if (verdict is GuardVerdict<T>.Coerce coerced) args.CurrentValue = coerced.Value;
                   });
                   return Fin.Succ(value: unit);
               }));
    }

    internal bool CreatesPath => this switch {
        Write or Clamp or Hide or HideNode or Install => true,
        Custom request => request.Creates,
        _ => false,
    };

    internal Option<string> MutationName => this switch {
        Write request => Some(request.Name),
        Clamp request => Some(request.Name),
        Delete request => Some(request.Name),
        DropChild request => Some(request.Name),
        Hide request => Some(request.Name),
        HideNode => Some(string.Empty),
        ClearChanged => Some(string.Empty),
        Install request => Some(request.Name),
        Custom request => request.Mutates,
        _ => None,
    };

    internal Fin<SettingRequest> Admit(Op op) =>
        Switch(
            op,
            write: static (key, request) =>
                from name in key.AcceptText(value: request.Name)
                from value in Optional(request.Value).ToFin(Fail: key.InvalidInput())
                from layer in Optional(request.Layer).ToFin(Fail: key.InvalidInput())
                select (SettingRequest)new Write(Name: name, Value: value, Layer: layer),
            read: static (key, request) =>
                from name in key.AcceptText(value: request.Name)
                from type in Optional(request.ValueType).ToFin(Fail: key.InvalidInput())
                from layer in Optional(request.Layer).ToFin(Fail: key.InvalidInput())
                from legacy in request.LegacyNames.TraverseM(item => key.AcceptText(value: item)).As()
                from _ in SettingCodecs.Resolve(type: type, op: key)
                from __ in guard(layer == SettingLayer.Explicit || legacy.IsEmpty, key.InvalidInput()).ToFin()
                select (SettingRequest)new Read(Name: name, ValueType: type, LegacyNames: legacy, Layer: layer),
            clamp: static (key, request) =>
                from name in key.AcceptText(value: request.Name)
                from bounds in Optional(request.Bounds).ToFin(Fail: key.InvalidInput())
                select (SettingRequest)new Clamp(Name: name, Fallback: request.Fallback, Bounds: bounds),
            delete: static (key, request) => key.AcceptText(value: request.Name).Map(name => (SettingRequest)new Delete(Name: name)),
            dropChild: static (key, request) => key.AcceptText(value: request.Name).Map(name => (SettingRequest)new DropChild(Name: name)),
            hide: static (key, request) => key.AcceptText(value: request.Name).Map(name => (SettingRequest)new Hide(Name: name)),
            hideNode: static (_, request) => Fin.Succ<SettingRequest>(value: request),
            clearChanged: static (_, request) => Fin.Succ<SettingRequest>(value: request),
            describe: static (key, request) => key.AcceptText(value: request.Name).Map(name => (SettingRequest)new Describe(Name: name)),
            keys: static (_, request) => Fin.Succ<SettingRequest>(value: request),
            children: static (_, request) => Fin.Succ<SettingRequest>(value: request),
            changed: static (_, request) => Fin.Succ<SettingRequest>(value: request),
            install: static (_, request) => Fin.Succ<SettingRequest>(value: request),
            custom: static (_, request) => Fin.Succ<SettingRequest>(value: request));

    internal Fin<Option<SettingAnswer>> Apply(PersistentSettings node, Op op) =>
        Switch(
            (Node: node, Op: op),
            write: static (context, request) => request.Value.Write(
                context.Node, request.Name, request.Layer == SettingLayer.Default, context.Op).Map(static _ => Option<SettingAnswer>.None),
            read: static (context, request) => SettingValue.Read(
                    context.Node, request.Name, request.ValueType, request.LegacyNames, request.Layer == SettingLayer.Default, context.Op)
                .Map(value => Some<SettingAnswer>(new SettingAnswer.Held(Value: value))),
            clamp: static (context, request) => request.Bounds.Apply(context.Node, request.Name, request.Fallback, context.Op)
                .Bind(clamped => SettingValue.Create(value: clamped, key: context.Op)
                .Map(held => Some<SettingAnswer>(new SettingAnswer.Held(Value: Some(held))))),
            delete: static (context, request) => context.Op.Catch(() => context.Node.DeleteItem(request.Name)).Map(static _ => Option<SettingAnswer>.None),
            dropChild: static (context, request) => context.Op.Catch(() => context.Node.DeleteChild(request.Name)).Map(static _ => Option<SettingAnswer>.None),
            hide: static (context, request) => context.Op.Catch(() => context.Node.HideSettingFromUserInterface(request.Name)).Map(static _ => Option<SettingAnswer>.None),
            hideNode: static (context, request) => context.Op.Catch(() => context.Node.HiddenFromUserInterface = request.Hidden).Map(static _ => Option<SettingAnswer>.None),
            clearChanged: static (context, _) => context.Op.Catch(context.Node.ClearChangedFlag).Map(static _ => Option<SettingAnswer>.None),
            describe: static (context, request) => context.Op.Catch(() => Fin.Succ<Option<SettingAnswer>>(value: Some<SettingAnswer>(new SettingAnswer.Described(
                Trait: new SettingTrait(
                    StoredType: context.Node.TryGetSettingType(request.Name, out Type type) ? Some(type) : None,
                    ReadOnly: context.Node.TryGetSettingIsReadOnly(request.Name, out bool readOnly) ? Some(readOnly) : None,
                    Hidden: context.Node.TryGetSettingIsHiddenFromUserInterface(request.Name, out bool hidden) ? Some(hidden) : None))))),
            keys: static (context, _) => context.Op.Catch(() => Fin.Succ<Option<SettingAnswer>>(value: Some<SettingAnswer>(new SettingAnswer.Names(Values: toSeq(context.Node.Keys))))),
            children: static (context, _) => context.Op.Catch(() => Fin.Succ<Option<SettingAnswer>>(value: Some<SettingAnswer>(new SettingAnswer.Names(Values: toSeq(context.Node.ChildKeys))))),
            changed: static (context, request) => request.Against.Match(
                Some: scope => scope.Resolve(createPath: false, op: context.Op).Bind(other => context.Op.Catch(() => Fin.Succ<Option<SettingAnswer>>(
                    value: Some<SettingAnswer>(new SettingAnswer.Tracked(Value: context.Node.ContainsModifiedValues(allUserSettings: other)))))),
                None: () => context.Op.Catch(() => Fin.Succ<Option<SettingAnswer>>(
                    value: Some<SettingAnswer>(new SettingAnswer.Tracked(Value: context.Node.ContainsChangedValues()))))),
            install: static (context, request) => request.Apply(context.Node, request.Name, context.Op).Map(static _ => Option<SettingAnswer>.None),
            custom: static (context, request) => request.Apply(context.Node, context.Op));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingAnswer : IDetachedDocumentResult {
    private SettingAnswer() { }
    public sealed record Held(Option<SettingValue> Value) : SettingAnswer;
    public sealed record Described(SettingTrait Trait) : SettingAnswer;
    public sealed record Names(Seq<string> Values) : SettingAnswer;
    public sealed record Tracked(bool Value) : SettingAnswer;
    public sealed record Projected(Type ValueType, Option<object> Value) : SettingAnswer;
}

public sealed record SettingsReceipt(Seq<SettingAnswer> Answers, Seq<string> MutatedNames) : IDetachedDocumentResult;

// --- [ENTRY] --------------------------------------------------------------------------------
public static class Settings {
    public static Fin<SettingsReceipt> Run(SettingsScope scope, params ReadOnlySpan<SettingRequest> requests) {
        Op op = Op.Of();
        return from source in Optional(scope).ToFin(Fail: op.InvalidInput())
               from program in toSeq(requests.ToArray()).TraverseM(request =>
                       Optional(request).ToFin(Fail: op.InvalidInput()).Bind(active => active.Admit(op: op)))
                   .As()
               from _ in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
               from node in source.Resolve(createPath: program.Exists(static request => request.CreatesPath), op: op)
               from outcomes in program.TraverseM(request => request.Apply(node: node, op: op)).As()
               select new SettingsReceipt(
                   Answers: outcomes.Fold(Seq<SettingAnswer>(), static (state, answer) => answer.Match(Some: state.Add, None: () => state)),
                   MutatedNames: program.Fold(Seq<string>(), static (state, request) => request.MutationName.Match(Some: state.Add, None: () => state)));
    }
}
```

## [05]-[SAVED_WATCH]

- Owner: `SettingsTree` recursively detaches explicit values, readable defaults, metadata, and children. Unsupported host types fail the entire snapshot.
- Event: `SettingsSaved` carries provenance, the plugin tree, and every requested command tree; no live `PersistentSettings` node crosses the callback.
- Dispatch: `Rhino.PlugIns.PlugIn.SettingsSaved` is an instance `EventHandler<PersistentSettingsSavedEventArgs>`; the `args.PlugInSettings` property and `args.CommandSettings(string)` supply callback custody.
- Subscription: Document's `Subscription` capsule owns wrapper identity, throwing-attach rollback, and idempotent detach; snapshot failure reaches the sink on the same `Fin` rail.

```csharp signature
// --- [DETACHED_TREE] ------------------------------------------------------------------------
public sealed record SettingEntry(SettingValue Value, Option<SettingValue> Default, SettingTrait Trait) : IDetachedDocumentResult;

public sealed record SettingsTree(
    HashMap<string, SettingEntry> Values,
    HashMap<string, SettingsTree> Children) : IDetachedDocumentResult {
    internal static Fin<SettingsTree> Detach(PersistentSettings node, Op op) => op.Catch(() => DetachCore(node: node, op: op));

    private static Fin<SettingsTree> DetachCore(PersistentSettings node, Op op) =>
        from values in toSeq(node.Keys).TraverseM(name =>
                op.Catch(() => node.TryGetSettingType(name, out Type stored)
                        ? Fin.Succ(value: stored)
                        : Fin.Fail<Type>(error: op.MissingContext()))
                    .Bind(type => SettingValue.Read(node, name, type, Seq<string>(), fromDefault: false, op)
                        .Bind(value => value.ToFin(Fail: op.MissingContext()))
                        .Bind(held => SettingValue.Read(node, name, type, Seq<string>(), fromDefault: true, op)
                            .Map(fallback => (name, Entry: new SettingEntry(
                                Value: held,
                                Default: fallback,
                                Trait: new SettingTrait(
                                    StoredType: Some(type),
                                    ReadOnly: node.TryGetSettingIsReadOnly(name, out bool readOnly) ? Some(readOnly) : None,
                                    Hidden: node.TryGetSettingIsHiddenFromUserInterface(name, out bool hidden) ? Some(hidden) : None)))))))
            .As()
        from children in toSeq(node.ChildKeys).TraverseM(name =>
                node.TryGetChild(name, out PersistentSettings child)
                    ? DetachCore(node: child, op: op).Map(tree => (name, tree))
                    : Fin.Fail<(string, SettingsTree)>(error: op.MissingContext()))
            .As()
        select new SettingsTree(
            Values: values.Fold(HashMap<string, SettingEntry>(), static (state, pair) => state.AddOrUpdate(pair.name, pair.Entry)),
            Children: children.Fold(HashMap<string, SettingsTree>(), static (state, pair) => state.AddOrUpdate(pair.name, pair.tree)));
}

public sealed record SettingsSaved(
    bool SavedByThisRhino,
    SettingsTree PlugIn,
    HashMap<string, SettingsTree> Commands) : IDetachedDocumentResult;

public static class SettingsWatch {
    public static Fin<Subscription> Attach(
        global::Rhino.PlugIns.PlugIn owner,
        Seq<string> commandNames,
        Action<Fin<SettingsSaved>> sink) {
        Op op = Op.Of(name: nameof(SettingsWatch));
        return from plugin in Optional(owner).ToFin(Fail: op.InvalidInput())
               from commands in commandNames.TraverseM(name => op.AcceptText(value: name)).As()
               from _ in guard(commands.Distinct().Count == commands.Count, op.InvalidInput()).ToFin()
               from deliver in Optional(sink).ToFin(Fail: op.InvalidInput())
               from capsule in Subscription.Attach<EventHandler<PersistentSettingsSavedEventArgs>>(
                   subscribe: handler => plugin.SettingsSaved += handler,
                   unsubscribe: handler => plugin.SettingsSaved -= handler,
                   handler: (_, args) => deliver(Snapshot(args: args, commands: commands, op: op)))
               select capsule;
    }

    private static Fin<SettingsSaved> Snapshot(PersistentSettingsSavedEventArgs args, Seq<string> commands, Op op) =>
        from pluginTree in SettingsTree.Detach(node: args.PlugInSettings, op: op)
        from commandTrees in commands.TraverseM(name => op.Catch(() =>
                Optional(args.CommandSettings(name)).ToFin(Fail: op.MissingContext()))
                .Bind(node => SettingsTree.Detach(node: node, op: op))
                .Map(tree => (name, tree)))
            .As()
        select new SettingsSaved(
            SavedByThisRhino: args.SavedByThisRhino,
            PlugIn: pluginTree,
            Commands: commandTrees.Fold(
                HashMap<string, SettingsTree>(),
                static (state, pair) => state.AddOrUpdate(pair.name, pair.tree)));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]      | [OWNER]           | [FORM]                                           | [ENTRY]                   |
| :-----: | :------------- | :---------------- | :----------------------------------------------- | :------------------------ |
|  [01]   | settings node  | `SettingsScope`   | root plus admitted child path                    | `Create` / `Resolve`      |
|  [02]   | typed value    | `SettingValue`    | payload plus retained codec discriminant         | `Create` / `As`           |
|  [03]   | host matrix    | `SettingCodecs`   | generated explicit/default/full capability rows  | `Resolve`                 |
|  [04]   | operation rail | `SettingRequest`  | total mutation and question union                | `Settings.Run`            |
|  [05]   | integer clamp  | `IntegerClamp`    | admitted optional lower and upper bounds         | `Apply`                   |
|  [06]   | validator      | `GuardVerdict<T>` | accept, reject, or coerce                        | `SettingRequest.Guard<T>` |
|  [07]   | saved snapshot | `SettingsTree`    | recursive values, defaults, traits, and children | `Detach`                  |
|  [08]   | saved watch    | `SettingsWatch`   | Document subscription capsule, detached payload  | `Attach`                  |
