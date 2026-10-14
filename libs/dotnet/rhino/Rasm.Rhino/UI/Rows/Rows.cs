using System.Runtime.CompilerServices;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommitMode {
    public sealed record Immediate : CommitMode;

    public sealed record Revertible(ValueSet Opening) : CommitMode;

    public sealed record Deferred(ValueSet Edits) : CommitMode;

    public sealed record Targets : CommitMode {
        internal Targets(ObjectPropertiesPage page) => Page = page;

        public ObjectPropertiesPage Page { get; }
    }

    public sealed record Contents : CommitMode {
        internal Contents(EtoContentUISection3 section) => Section = section;

        public EtoContentUISection3 Section { get; }
    }
}

public sealed record RowField<TRecord>(IStateParameter<TRecord> Parameter, string Caption, string Help) where TRecord : notnull {
    public TRecord Taken(TRecord from, TRecord into) => Parameter.Kind.Accept(new Copy(from, into));

    private sealed class Copy(TRecord from, TRecord into) : IStateParameterVisitor<TRecord, TRecord> {
        public TRecord Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> => Over(lens);

        public TRecord OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> => Over(lens);

        public TRecord Choice<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class => Over(lens);

        public TRecord Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum => Over(lens);

        public TRecord Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> => Over(lens);

        public TRecord OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> => Over(lens);

        public TRecord Toggle(Lens<TRecord, bool> lens) => Over(lens);

        public TRecord Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => Over(lens);

        public TRecord OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => Over(lens);

        public TRecord Color(Lens<TRecord, Swatch> lens) => Over(lens);

        public TRecord OptionalColor(Lens<TRecord, Gated<Swatch>> lens) => Over(lens);

        public TRecord Gradient(Lens<TRecord, Ramp> lens) => Over(lens);

        public TRecord Swatches<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord Keyed<TValue, TRaw, TError>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord OptionalKeyed<TValue, TRaw, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord Loaded<TValue, TKey, TRaw, TError>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull => Over(lens);

        private TRecord Over<TFocus>(Lens<TRecord, TFocus> lens) => lens.Set(lens.Get(from), into);
    }
}

public sealed record ParameterText(
    string Caption, string Help, HashMap<string, (string Caption, HashMap<string, ParameterText> Parameters)> Items, Option<string> Absent) {
    public HashMap<string, ParameterText> Parameters { get; init; }

    public static ParameterText Of(string caption, string help) => new(caption, help, [], None);

    public static ParameterText Choice<TValue, TError>(string caption, string help, Func<TValue, string> items)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        new(caption, help, toHashMap(toSeq(TValue.Items).Map(item => (item.ToValue(), (items(item), HashMap<string, ParameterText>())))), None);

    public static ParameterText Choice<TValue, TError>(string caption, string help, string absent, Func<TValue, string> items)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        Choice<TValue, TError>(caption, help, items) with { Absent = absent };

    public static ParameterText Variant<TValue>(
        string caption, string help, Func<TValue, (string Caption, HashMap<string, ParameterText> Parameters)> items) where TValue : class =>
        new(caption, help, toHashMap(StateCase<TValue>.Items.Map(item => (item.Key, items(item.Default)))), None);

    public static ParameterText Enumeration<TEnum>(string caption, string help, Func<TEnum, string> members) where TEnum : struct, Enum =>
        new(caption, help, toHashMap(toSeq(Enum.GetValues<TEnum>()).Map(member => (StoredText.Format(member), (members(member), HashMap<string, ParameterText>())))), None);

    public static HashMap<string, ParameterText> Fields<TState, TParameter, TError>(Func<TParameter, ParameterText> text)
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        toHashMap(Join<TState, TParameter, TError>(text).Map(static pair => (pair.Parameter.Key, pair.Text)));

    public static Seq<(TParameter Parameter, ParameterText Text)> Join<TState, TParameter, TError>(Func<TParameter, ParameterText> text)
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        toSeq(TParameter.Items).Map(parameter => (Parameter: parameter, Text: text(parameter))).Strict();
}

public abstract record RowSource {
    // --- [BINDING]
    private protected RowSource(string owner) => Owner = owner;

    public string Owner { get; }

    public abstract ValueBinding Values(RowScope scope);

    public abstract IO<Seq<HostEvent<Unit>>> Signals(RowScope scope);

    // --- [RECORDS]
    public static RowSource<TRecord> Fields<TRecord>(
        string owner, Seq<(IStateParameter<TRecord> Parameter, string Caption)> fields, Func<RowScope, ValueStore<TRecord>> store) where TRecord : IStateRecord<TRecord> =>
        Fields(owner, fields, scope => IO.pure(Seq(store(scope))));

    public static RowSource<TRecord> Fields<TRecord>(
        string owner, Seq<(IStateParameter<TRecord> Parameter, string Caption)> fields, Func<RowScope, IO<Seq<ValueStore<TRecord>>>> stores, Option<TRecord> factory = default) where TRecord : IStateRecord<TRecord> =>
        factory.IfNone(TRecord.Default) switch {
            var initial => new(owner, stores, initial, fields),
        };

    // --- [KEYS]
    public static (RowSource<TValue> Source, RowField<TValue> Field) Bounded<TValue, TKey>(
        string owner, ValueKey<TValue, TKey> key, Presentation<TKey> presentation, Func<RowScope, ValueStore<TValue>> store)
        where TValue : IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
        where TKey : struct, System.Numerics.INumber<TKey> =>
        Keyed(owner, key, new StateParameter<TValue>.Numeric<TValue, TKey>(Lens.identity<TValue>(), key.From, key.ToRaw, (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), presentation), store);

    public static (RowSource<TValue> Source, RowField<TValue> Field) Choice<TValue, TError>(
        string owner, ValueKey<TValue, string> key, Func<RowScope, ValueStore<TValue>> store)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        Keyed(owner, key, new StateParameter<TValue>.Choice<TValue, TError>(Lens.identity<TValue>()), store);

    public static (RowSource<TValue> Source, RowField<TValue> Field) Keyed<TValue, TRaw>(
        string owner, ValueKey<TValue, TRaw> key, Func<RowScope, ValueStore<TValue>> store)
        where TValue : notnull
        where TRaw : notnull =>
        Named(owner, key.Name, key.Caption, key.Help, key.Default, key.Kind, store);

    public static (RowSource<bool> Source, RowField<bool> Field) Toggle(
        string owner, ValueKey<bool, bool> key, Func<RowScope, ValueStore<bool>> store) =>
        Keyed(owner, key, new StateParameter<bool>.Toggle(Lens.identity<bool>()), store);

    public static (RowSource<TRaw> Source, RowField<TRaw> Field) Raw<TRaw>(
        string owner, ValueKey<TRaw, TRaw> key, Func<RowScope, ValueStore<TRaw>> store) where TRaw : notnull, ISpanParsable<TRaw> =>
        Keyed(owner, key, new StateParameter<TRaw>.Raw<TRaw>(Lens.identity<TRaw>()), store);

    public static (RowSource<Option<TValue>> Source, RowField<Option<TValue>> Field) OptionalKeyed<TValue, TRaw, TError>(
        string owner, ValueKey<Option<TValue>, TRaw> key, Func<RowScope, ValueStore<Option<TValue>>> store)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Keyed(owner, key, new StateParameter<Option<TValue>>.OptionalKeyed<TValue, TRaw, TError>(Lens.identity<Option<TValue>>()), store);

    // --- [NAMES]
    public static (RowSource<Gated<TValue>> Source, RowField<Gated<TValue>> Field) OptionalBounded<TValue, TKey, TError>(
        string owner, string name, string caption, string help, TValue @default, Presentation<TKey> presentation, Func<RowScope, ValueStore<TValue>> store)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
        where TKey : struct, System.Numerics.INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Named(owner, name, caption, help, new Gated<TValue>(Enabled: false, @default),
            StateParameter.OptionalBounded<Gated<TValue>, TValue, TKey, TError>(Lens.identity<Gated<TValue>>(), presentation),
            scope => Lifted(store(scope), static value => new Gated<TValue>(Enabled: true, value), static gate => gate.Active));

    public static (RowSource<Option<TValue>> Source, RowField<Option<TValue>> Field) OptionalChoice<TValue, TError>(
        string owner, string name, string caption, string help, Func<RowScope, ValueStore<TValue>> store)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        Named(owner, name, caption, help, Option<TValue>.None,
            new StateParameter<Option<TValue>>.OptionalChoice<TValue, TError>(Lens.identity<Option<TValue>>()), scope => Absent(store(scope)));

    public static (RowSource<Option<TValue>> Source, RowField<Option<TValue>> Field) OptionalKeyed<TValue, TRaw, TError>(
        string owner, string name, string caption, string help, Func<RowScope, ValueStore<TValue>> store)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Named(owner, name, caption, help, Option<TValue>.None,
            new StateParameter<Option<TValue>>.OptionalKeyed<TValue, TRaw, TError>(Lens.identity<Option<TValue>>()), scope => Absent(store(scope)));

    public static (RowSource<TEnum> Source, RowField<TEnum> Field) Enumerated<TEnum>(
        string owner, string name, string caption, string help, TEnum @default, Func<RowScope, ValueStore<TEnum>> store) where TEnum : struct, Enum =>
        Named(owner, name, caption, help, @default, new StateParameter<TEnum>.Enumerated<TEnum>(Lens.identity<TEnum>()), store);

    public static (RowSource<Option<TValue>> Source, RowField<Option<TValue>> Field) Loaded<TValue, TKey, TRaw, TError>(
        string owner, string name, string caption, string help, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key, Func<RowScope, ValueStore<TValue>> store)
        where TValue : notnull
        where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Named(owner, name, caption, help, Option<TValue>.None,
            new StateParameter<Option<TValue>>.Loaded<TValue, TKey, TRaw, TError>(Lens.identity<Option<TValue>>(), load, key), scope => Absent(store(scope)));

    public static (RowSource<TValue> Source, RowField<TValue> Field) Opaque<TValue>(
        string owner, string name, TValue @default, Func<RowScope, ValueStore<TValue>> store) where TValue : notnull =>
        Named(owner, name, name, "", @default, new StateParameter<TValue>.Opaque<TValue>(Lens.identity<TValue>()), store);

    public static (RowSource<TValue> Source, RowField<TValue> Field) Opaque<TValue>(string owner, string name, TValue @default) where TValue : notnull =>
        new ConditionalWeakTable<RowScope, Ref<Option<TValue>>>() switch {
            var cells => Opaque(owner, name, @default, scope => RowScope.Cell(cells.GetValue(scope, static _ => Ref(Option<TValue>.None)))),
        };

    // --- [STORES]
    internal static ValueStore<Option<TValue>> Absent<TValue>(ValueStore<TValue> store) where TValue : notnull =>
        Lifted(store, static value => Some(value), static held => held);

    private static ValueStore<TRecord> Lifted<TValue, TRecord>(ValueStore<TValue> store, Func<TValue, TRecord> held, Func<TRecord, Option<TValue>> value)
        where TValue : notnull
        where TRecord : notnull =>
        new(store.Read.Map(found => found.Map(held)),
            record => store.Put(record.Bind(value)).Map(change => ValueStore.Map(change, held, EqualityComparer<TRecord>.Default.Equals)),
            (left, right) => value(left).Match(Some: kept => value(right).Exists(other => store.Same(kept, other)), None: () => value(right).IsNone),
            store.Applied,
            store.Changed);

    private static (RowSource<TValue> Source, RowField<TValue> Field) Keyed<TValue, TRaw>(
        string owner, ValueKey<TValue, TRaw> key, StateParameter<TValue> kind, Func<RowScope, ValueStore<TValue>> store)
        where TValue : notnull
        where TRaw : notnull =>
        Named(owner, key.Name, key.Caption, key.Help, key.Default, kind, store);

    private static (RowSource<TValue> Source, RowField<TValue> Field) Named<TValue>(
        string owner, string name, string caption, string help, TValue @default, StateParameter<TValue> kind, Func<RowScope, ValueStore<TValue>> store) where TValue : notnull =>
        new KeyParameter<TValue>(name, kind) switch {
            var parameter => (new(owner, scope => IO.pure(Seq(store(scope))), @default, Seq<(IStateParameter<TValue>, string)>((parameter, caption))),
                              new(parameter, caption, help)),
        };
}

public sealed record RowSource<TRecord> : RowSource where TRecord : notnull {
    internal RowSource(string owner, Func<RowScope, IO<Seq<ValueStore<TRecord>>>> stores, TRecord @default, Seq<(IStateParameter<TRecord> Parameter, string Caption)> fields) : base(owner) =>
        (Stores, Default, Fields) = (stores, @default, fields);

    public Func<RowScope, IO<Seq<ValueStore<TRecord>>>> Stores { get; }

    public TRecord Default { get; }

    public Seq<(IStateParameter<TRecord> Parameter, string Caption)> Fields { get; }

    public ValueBinding Binding(ValueStore<TRecord> store) => ValueBinding.Fields(Owner, store, Default, Fields).Under(Path);

    public Seq<string> Path { get; private init; }

    public Seq<EntryKey> Keys(IStateParameter<TRecord> parameter) =>
        FieldTexts.Of(parameter).Map(text => new EntryKey(Owner, Path + text.Path));

    public RowSource<TNested> Within<TNested>(Seq<string> path, Lens<TRecord, TNested> lens, Seq<(IStateParameter<TNested> Parameter, string Caption)> fields)
        where TNested : notnull =>
        (Prefix: Path + path, Default: lens.Get(Default)) switch {
            var nested => new RowSource<TNested>(
                Owner,
                scope =>
                    from stores in Stores(scope)
                    select stores.Map(store => new ValueStore<TNested>(
                        store.Read.Map(held => held.Map(lens.Get)),
                        next => store.Read.Bind(held => store.Put(Some(lens.Set(next.IfNone(nested.Default), held.IfNone(Default)))))
                            .Map(change => ValueStore.Map(change, lens.Get, EqualityComparer<TNested>.Default.Equals)),
                        EqualityComparer<TNested>.Default.Equals,
                        store.Applied,
                        store.Changed)),
                nested.Default,
                fields) { Path = nested.Prefix },
        };

    public override ValueBinding Values(RowScope scope) =>
        Binding(scope.Store(this)) switch {
            var bound => bound with {
                Prepare = entries =>
                    from stores in Stores(scope)
                    from prepared in stores.Map<K<IO, IO<ValueDiff>>>(store => Binding(store).Prepare(entries)).PartitionFallible().As()
                    from accepted in unless(prepared.Fails.IsEmpty, IO.fail<Unit>(Error.Many(prepared.Fails))).As()
                    select Callbacks.Each(prepared.Succs).Map(ValueDiff.Together),
            },
        };

    public ValueSet Capture(TRecord record) => new(toHashMap(Fields.Bind(static field => FieldTexts.Of(field.Parameter))
        .Choose(field => field.Capture(record).Map(text => (new EntryKey(Owner, Path + field.Path), text)))));

    public override IO<Seq<HostEvent<Unit>>> Signals(RowScope scope) =>
        Stores(scope).Map(static stores => stores.Choose(static store => store.Changed));
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class Wording {
    public static readonly Wording English = new(static (text, sink) => RowText.Localize(text, table: Some<object>(sink)).Local);
    public static readonly Wording Localized = new(static (text, _) => text);

    [UseDelegateFromConstructor]
    public partial string Shown(string text, IPlugInSink sink);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlRow {
    private ControlRow(string caption, Option<string> help, Seq<EntryKey> keys, RowRules rules, Wording wording) =>
        (Caption, Help, Keys, Rules, Wording) = (caption, help, keys, rules, wording);

    public string Caption { get; }

    public Option<string> Help { get; }

    public Seq<EntryKey> Keys { get; }

    public RowRules Rules { get; }

    public Wording Wording { get; init; }

    public Option<RowSource> Bound =>
        Switch(
            field: static row => Some(row.Source),
            readout: static _ => Option<RowSource>.None,
            command: static _ => Option<RowSource>.None,
            group: static _ => Option<RowSource>.None);

    public Option<RowShape> Layout =>
        Switch(
            field: static row => Some(row.Shape),
            readout: static readout => Some(readout.Shape),
            command: static command => Some(command.Shape),
            group: static _ => Option<RowShape>.None);

    public bool Fill => Layout.Exists(static shape => shape.Fills);

    public static ControlRow Of<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, RowRules rules)
        where TRecord : notnull =>
        Of(source, IterableNE.create(field), field.Caption, field.Help, shape, realize, rules);

    public static ControlRow Of<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, string caption, string help, RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, RowRules rules)
        where TRecord : notnull =>
        new Field(source, toSeq(fields).Bind(field => source.Keys(field.Parameter)), shape, realize, caption, help, rules);

    public sealed record Field : ControlRow {
        public Field(RowSource source, Seq<EntryKey> keys, RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, string caption, string help, RowRules rules)
            : base(caption, Some(help), keys, rules, Wording.English) =>
            (Source, Shape, Realize) = (source, shape, realize);

        public RowSource Source { get; }
        public RowShape Shape { get; }
        public Func<RowScope, Action<IDisposable>, IO<RowCells>> Realize { get; }
    }

    public sealed record Readout : ControlRow {
        public Readout(RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, string caption, string help, RowRules rules)
            : base(caption, Some(help), [], rules, Wording.English) =>
            (Shape, Realize) = (shape, realize);

        public RowShape Shape { get; }
        public Func<RowScope, Action<IDisposable>, IO<RowCells>> Realize { get; }
    }

    public sealed record Command(CommandRow Row, RowShape Shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> Realize, Option<RowRule> Visible)
        : ControlRow(Row.Face.Caption, Some(Row.Face.Help), [], new RowRules(Row.Enabled, Visible), Row.Face.Wording);

    public sealed record Group(string Caption, RowRules Rules) : ControlRow(Caption, None, [], Rules, Wording.English);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowScope {
    // --- [STATE]
    private static readonly LanguageExt.HashSet<EntryKey> Unlocked = [];

    private readonly Atom<CommitMode> mode;
    private readonly Atom<LanguageExt.HashSet<Func<Seq<EntryKey>, IO<Unit>>>> listeners;
    private readonly Atom<(bool Visible, LanguageExt.HashSet<Func<bool, IO<Unit>>> Listeners)> visibility;

    private RowScope(
        IPlugInSink sink, Option<RhinoDoc> document, Option<SettingsNode> settings, Atom<CommitMode> mode, Option<HistoryBinding> history,
        Option<ValueStore<LanguageExt.HashSet<EntryKey>>> locks, Atom<LanguageExt.HashSet<Func<Seq<EntryKey>, IO<Unit>>>> listeners, Option<Control> host,
        Atom<(bool Visible, LanguageExt.HashSet<Func<bool, IO<Unit>>> Listeners)> visibility) =>
        (Sink, Document, Settings, this.mode, History, Locks, this.listeners, Host, this.visibility) = (sink, document, settings, mode, history, locks, listeners, host, visibility);

    public IPlugInSink Sink { get; }
    public Option<RhinoDoc> Document { get; }
    public Option<SettingsNode> Settings { get; }
    public Option<HistoryBinding> History { get; }
    public Option<ValueStore<LanguageExt.HashSet<EntryKey>>> Locks { get; }
    public Option<Control> Host { get; }

    // --- [OPENING]
    public static IO<RowScope> Open(IPlugInSink sink, Option<RhinoDoc> document, CommitMode mode) =>
        IO.lift(() => new RowScope(sink, document, None, Atom(mode), None, None, Atom(LanguageExt.HashSet<Func<Seq<EntryKey>, IO<Unit>>>.Empty), None,
            Atom((Visible: true, Listeners: LanguageExt.HashSet<Func<bool, IO<Unit>>>.Empty))));

    public RowScope Nested(SettingsNode settings, Option<HistoryBinding> history, Option<ValueStore<LanguageExt.HashSet<EntryKey>>> locks) =>
        new(Sink, Document, Some(settings), mode, history, locks, listeners, Host,
            Atom((visibility.Value.Visible, LanguageExt.HashSet<Func<bool, IO<Unit>>>.Empty)));

    public RowScope Hosted(Control host) => new(Sink, Document, Settings, mode, History, Locks, listeners, Some(host), visibility);

    public IO<CommitMode> Mode => mode.ValueIO;

    // --- [VISIBILITY]
    public IO<bool> Visibility => visibility.ValueIO.Map(static held => held.Visible);

    public IO<Unit> Shown(bool visible) => visibility.SwapIO(held => held with { Visible = visible })
        .Bind(held => Callbacks.Each(toSeq(held.Listeners).Map(listener => listener(visible)))).Map(static _ => unit);

    public IO<Unit> OnVisibility(Func<bool, IO<Unit>> heard, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => {
            _ = ignore(visibility.Swap(held => held with { Listeners = held.Listeners.TryAdd(heard) }));
            return new Disposal<Func<bool, IO<Unit>>>(heard,
                released => ignore(visibility.Swap(held => held with { Listeners = held.Listeners.Remove(released) })));
        }), static lease => IO.lift(lease.Dispose), retain).Bind(_ => Visibility).Bind(heard);

    // --- [CHANGES]
    public IO<Unit> Changed(Seq<EntryKey> keys) =>
        listeners.ValueIO
            .Bind(held => toSeq(held).Map<K<IO, Unit>>(heard => heard(keys)).Fails().As())
            .Bind(static fails => unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As());

    public IO<Unit> Listen(Func<Seq<EntryKey>, IO<Unit>> heard, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            IO.lift(() => {
                _ = ignore(listeners.Swap(held => held.TryAdd(heard)));
                return new Disposal<Func<Seq<EntryKey>, IO<Unit>>>(heard, released => ignore(listeners.Swap(held => held.Remove(released))));
            }),
            static lease => IO.lift(lease.Dispose), retain).Map(static _ => unit);

    // --- [STORES]
    public IO<Seq<ValueStore<T>>> Objects<T>(Func<RhinoObject, ValueStore<T>> store) where T : notnull =>
        Selected(targets => Conversions.Rows(new ObjectPropertiesPageEventArgs(targets.Page).Objects).Map(store).Strict(), static _ => Seq<ValueStore<T>>());

    public IO<Seq<ValueStore<T>>> Viewport<T>(Func<RhinoViewport, ValueStore<T>> store) where T : notnull =>
        Selected(targets => Optional(new ObjectPropertiesPageEventArgs(targets.Page).Viewport).ToSeq().Map(store).Strict(), static _ => Seq<ValueStore<T>>());

    public IO<Seq<ValueStore<T>>> Contents<T>(Func<RenderContent, ValueStore<T>> store) where T : notnull =>
        Selected(static _ => Seq<ValueStore<T>>(), contents => toSeq(contents.Section.GetSelection()).Map(store).Strict());

    public ValueStore<TRecord> Store<TRecord>(RowSource<TRecord> source) where TRecord : notnull =>
        new(source.Stores(this).Bind(static stores => stores.Head.Match(Some: static store => store.Read, None: static () => IO.pure(Option<TRecord>.None))),
            value => source.Stores(this).Bind(stores =>
                ValueStore.Commit(stores.Map(store => store.Apply(value)))).Map(static changes => changes.Find(static change => change.HasChanged).IfNone(Change<TRecord>.None)),
            EqualityComparer<TRecord>.Default.Equals,
            Applied.Live,
            None);

    public IO<Seq<ValueStore<TRecord>>> Edited<TRecord>(RowSource<TRecord> source) where TRecord : notnull =>
        mode.ValueIO.Bind(held => held.Map(immediate: false, revertible: false, deferred: true, targets: false, contents: false)
            ? source.Stores(this).Map(stores => stores.Map(store => Drafted(source, store)))
            : source.Stores(this));

    public IO<TRecord> Read<TRecord>(RowSource<TRecord> source) where TRecord : notnull =>
        Edited(source)
            .Bind(static stores => stores.Head.Match(Some: static store => store.Read, None: static () => IO.pure(Option<TRecord>.None)))
            .Map(held => held.IfNone(source.Default));

    internal static ValueStore<T> Cell<T>(Ref<Option<T>> cell) where T : notnull =>
        new(cell.ValueIO, next => atomicIO(() => {
            Option<T> before = cell.Value;
            cell.Value = next;
            return ValueStore.Between(before, next);
        }), EqualityComparer<T>.Default.Equals, Applied.Live, None);

    internal IO<bool> Paced => mode.ValueIO.Map(static held => held.Map(immediate: true, revertible: true, deferred: true, targets: false, contents: false));

    internal IO<bool> Records => mode.ValueIO.Map(static held => held.Map(immediate: true, revertible: true, deferred: false, targets: false, contents: false));

    private IO<Seq<ValueStore<T>>> Selected<T>(Func<CommitMode.Targets, Seq<ValueStore<T>>> targets, Func<CommitMode.Contents, Seq<ValueStore<T>>> contents) where T : notnull =>
        mode.ValueIO.Map(held => held.Switch(
            (Targets: targets, Contents: contents),
            immediate: static (_, _) => Seq<ValueStore<T>>(),
            revertible: static (_, _) => Seq<ValueStore<T>>(),
            deferred: static (_, _) => Seq<ValueStore<T>>(),
            targets: static (of, page) => of.Targets(page),
            contents: static (of, section) => of.Contents(section)));

    // --- [COMMITS]
    public IO<LanguageExt.HashSet<EntryKey>> Locked =>
        Locks.Match(Some: static store => store.Read.Map(static held => held.IfNone(Unlocked)), None: static () => IO.pure(Unlocked));

    public IO<Unit> Lock(Seq<EntryKey> keys, bool locked) =>
        Locks.Match(
            Some: store =>
                from held in Locked
                let next = locked ? held.Union(keys) : held.Except(keys)
                from written in store.Write(Some(next).Filter(static set => !set.IsEmpty))
                from changed in Changed(keys)
                select unit,
            None: static () => IO.pure(unit));

    public IO<ValueDiff> Commit(BindingGroup bound, ValueSet incoming) =>
        from locks in Locked
        from held in mode.ValueIO
        from diff in held.Map(immediate: false, revertible: false, deferred: true, targets: false, contents: false)
            ? Drafted(bound, incoming, locks)
            : Within(Written(bound, incoming, locks))
        from changed in Changed(toSeq(diff.Changes.Keys))
        select diff;

    public IO<ValueSet> Shown(BindingGroup bound) =>
        from captured in bound.Capture
        from edits in Edits
        select edits.Over(captured, Unlocked);

    internal IO<A> Within<A>(IO<A> body) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Body: body),
            immediate: static (state, _) => state.Body,
            revertible: static (state, _) => state.Body,
            deferred: static (state, _) => state.Body,
            targets: static (state, targets) => Callbacks.Captured<ObjectPropertiesPageEventArgs, A>(
                targets.Page.ModifyPage, _ => state.Scope.Recorded(targets, state.Body), nameof(ObjectPropertiesPage.ModifyPage)),
            contents: static (state, contents) => Recorded(contents, state.Body)));

    private IO<A> Recorded<A>(CommitMode.Targets targets, IO<A> body) =>
        from document in IO.lift(Document.ToFin(new Missing(nameof(Document))))
        from committed in Commits.Commit(document, targets.Page.EnglishPageTitle, new RedrawPolicy.AllViews(Deferred: true), body)
        select committed.Value;

    private static IO<A> Recorded<A>(CommitMode.Contents contents, IO<A> body) =>
        from model in IO.lift(() => Missing.Unless(contents.Section.ViewModel, nameof(EtoContentUISection3.ViewModel)))
        from written in DisposalOps.Within(() => new UndoRecord(contents.Section.Caption.Local, model), _ => body, static record => IO.lift(record.Dispose))
        from selected in IO.lift(() => Refused.Unless(contents.Section.SetSelection(contents.Section.GetSelection()), nameof(EtoContentUISection3.SetSelection)))
        select written;

    private IO<ValueDiff> Written(BindingGroup bound, ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        History.Match(Some: history => (history with { Group = bound }).Commit(incoming, locks), None: () => bound.Apply(incoming, locks));

    // --- [PAGES]
    public IO<bool> Pending => Edits.Map(static edits => !edits.Entries.IsEmpty);

    public IO<Unit> Opened(BindingGroup bound) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Group: bound),
            immediate: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Group.Capture.Bind(opening => state.Scope.Swapped(new CommitMode.Revertible(opening))),
            deferred: static (state, _) => state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty)),
            targets: static (_, _) => IO.pure(unit),
            contents: static (_, _) => IO.pure(unit)));

    public IO<Unit> Apply(BindingGroup bound) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Group: bound),
            immediate: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Scope.Opened(state.Group),
            deferred: static (state, deferred) =>
                from written in state.Scope.Written(state.Group, deferred.Edits, Unlocked)
                from cleared in state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty))
                from changed in state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys))
                select unit,
            targets: static (_, _) => IO.pure(unit),
            contents: static (_, _) => IO.pure(unit)));

    public IO<Unit> Revert(BindingGroup bound) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Group: bound),
            immediate: static (_, _) => IO.pure(unit),
            revertible: static (state, revertible) =>
                state.Scope.Written(state.Group, revertible.Opening, Unlocked).Bind(diff => state.Scope.Changed(toSeq(diff.Changes.Keys))),
            deferred: static (state, deferred) =>
                state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty)).Bind(_ => state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys))),
            targets: static (_, _) => IO.pure(unit),
            contents: static (_, _) => IO.pure(unit)));

    // --- [DRAFTS]
    private IO<ValueSet> Edits =>
        mode.ValueIO.Map(static held => held.Switch(
            immediate: static _ => ValueSet.Empty,
            revertible: static _ => ValueSet.Empty,
            deferred: static deferred => deferred.Edits,
            targets: static _ => ValueSet.Empty,
            contents: static _ => ValueSet.Empty));

    private IO<Unit> Swapped(CommitMode next) => mode.SwapIO(_ => next).Map(static _ => unit);

    private IO<ValueDiff> Drafted(BindingGroup bound, ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        from before in bound.Capture
        from edits in Edits
        let shown = edits.Over(before, Unlocked)
        let target = incoming.Over(shown, locks)
        let keys = bound.Keys()
        from recalled in bound.Bindings
            .Map(binding => (Binding: binding, Entries: Named(new ValueSet(incoming.Entries.Except(locks)), binding)))
            .Filter(static row => !row.Entries.IsEmpty)
            .Map<K<IO, IO<ValueDiff>>>(static row => row.Binding.Prepare(row.Entries)).PartitionFallible().As()
        from accepted in unless(recalled.Fails.IsEmpty, IO.fail<Unit>(Error.Many(recalled.Fails))).As()
        from swapped in Swapped(new CommitMode.Deferred(new ValueSet(
            edits.Entries.Filter((key, _) => !keys.Exists(held => held == key))
                .Union(target.Entries.Filter((key, text) => !before.Entries.Find(key).Exists(held => string.Equals(held, text, StringComparison.Ordinal)))))))
        select shown.Diff(target);

    private ValueStore<TRecord> Drafted<TRecord>(RowSource<TRecord> source, ValueStore<TRecord> store) where TRecord : notnull =>
        store with {
            Read =
                from held in store.Read
                from edits in Edits
                let owned = toHashMap(edits.Entries.AsIterable().Filter(entry => string.Equals(entry.Key.Owner, source.Owner, StringComparison.Ordinal)).Map(static entry => (entry.Key.Path, entry.Value)))
                from shown in owned.IsEmpty ? IO.pure(held) : Recalled(source, held.IfNone(source.Default), owned).Map(static record => Some(record))
                select shown,
            Put = value =>
                from before in Drafted(source, store).Read
                let texts = toHashMap(source.Capture(value.IfNone(source.Default)).Entries.AsIterable().Map(static entry => (entry.Key.Path, entry.Value)))
                from stored in source.Binding(store).Capture
                from edits in Edits
                let owned = toHashSet(source.Binding(store).Paths().Map(path => new EntryKey(source.Owner, path)))
                from swapped in Swapped(new CommitMode.Deferred(new ValueSet(
                    edits.Entries.Filter((key, _) => !owned.Contains(key))
                        .Union(toHashMap(texts.AsIterable()
                            .Filter(entry => !stored.Find(entry.Key).Exists(held => string.Equals(held, entry.Value, StringComparison.Ordinal)))
                            .Map(entry => (new EntryKey(source.Owner, entry.Key), entry.Value)))))))
                select ValueStore.Between(before, value, store.Same),
            Applied = Applied.Live,
        };

    private static IO<TRecord> Recalled<TRecord>(RowSource<TRecord> source, TRecord record, HashMap<Seq<string>, string> texts) where TRecord : notnull =>
        FieldTexts.Recalled(source.Owner, source.Fields.Map(static field => field.Parameter), path => IO.pure(texts.Find(source.Path + path)), record);

    private static HashMap<Seq<string>, string> Named(ValueSet values, ValueBinding binding) =>
        toHashMap(binding.Paths().Choose(path => values.Entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))));
}
