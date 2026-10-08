using System.Runtime.CompilerServices;
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
        public TRecord Bounded<TValue, TKey, TError>(Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation)
            where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
            where TKey : struct, System.Numerics.INumber<TKey>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord OptionalBounded<TValue, TKey, TError>(Lens<TRecord, Gated<TValue>> lens, Presentation<TValue, TKey> presentation)
            where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
            where TKey : struct, System.Numerics.INumber<TKey>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord Choice<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> => Over(lens);

        public TRecord Variant<TValue, TCase, TError>(Lens<TRecord, TValue> lens)
            where TValue : class
            where TCase : class, IStateCase<TValue>, ISmartEnum<string, TCase, TError>
            where TError : Error, IValidationError<TError> => Over(lens);

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

public abstract record RowSource {
    // --- [BINDING]
    private protected RowSource(string owner) => Owner = owner;

    public string Owner { get; }

    public abstract ValueBinding Values(RowScope scope);

    public abstract IO<ValueSet> Defaults { get; }

    public abstract IO<Seq<HostEvent<Unit>>> Signals(RowScope scope);

    // --- [RECORDS]
    public static RowSource<TRecord> Fields<TRecord>(
        string owner, Seq<(IStateParameter<TRecord> Parameter, string Caption)> fields, Func<RowScope, ValueStore<TRecord>> store) where TRecord : IStateRecord<TRecord> =>
        Fields(owner, fields, (RowScope scope) => IO.pure(Seq(store(scope))));

    public static RowSource<TRecord> Fields<TRecord>(
        string owner, Seq<(IStateParameter<TRecord> Parameter, string Caption)> fields, Func<RowScope, IO<Seq<ValueStore<TRecord>>>> stores) where TRecord : IStateRecord<TRecord> =>
        new(owner, stores, TRecord.Default, held => ValueBinding.Fields(owner, held, TRecord.Default, fields));

    // --- [KEYS]
    public static (RowSource<TValue> Source, RowField<TValue> Field) Bounded<TValue, TKey, TError>(
        string owner, ValueKey<TValue, TKey, TError> key, Presentation<TValue, TKey> presentation, Func<RowScope, ValueStore<TValue>> store)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
        where TKey : struct, System.Numerics.INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Keyed(owner, key, new StateParameter<TValue>.Bounded<TValue, TKey, TError>(Lens.identity<TValue>(), presentation), store);

    public static (RowSource<TValue> Source, RowField<TValue> Field) Choice<TValue, TError>(
        string owner, ValueKey<TValue, string, TError> key, Func<RowScope, ValueStore<TValue>> store)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        Keyed(owner, key, new StateParameter<TValue>.Choice<TValue, TError>(Lens.identity<TValue>()), store);

    public static (RowSource<TValue> Source, RowField<TValue> Field) Keyed<TValue, TRaw, TError>(
        string owner, ValueKey<TValue, TRaw, TError> key, Func<RowScope, ValueStore<TValue>> store)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Keyed(owner, key, new StateParameter<TValue>.Keyed<TValue, TRaw, TError>(Lens.identity<TValue>()), store);

    public static (RowSource<bool> Source, RowField<bool> Field) Toggle(
        string owner, ValueKey<bool, bool, InvalidRhinoValue> key, Func<RowScope, ValueStore<bool>> store) =>
        Keyed(owner, key, new StateParameter<bool>.Toggle(Lens.identity<bool>()), store);

    public static (RowSource<TRaw> Source, RowField<TRaw> Field) Raw<TRaw>(
        string owner, ValueKey<TRaw, TRaw, InvalidRhinoValue> key, Func<RowScope, ValueStore<TRaw>> store) where TRaw : notnull, ISpanParsable<TRaw> =>
        Keyed(owner, key, new StateParameter<TRaw>.Raw<TRaw>(Lens.identity<TRaw>()), store);

    public static (RowSource<Option<TValue>> Source, RowField<Option<TValue>> Field) OptionalKeyed<TValue, TRaw, TError>(
        string owner, ValueKey<Option<TValue>, TRaw, TError> key, Func<RowScope, ValueStore<Option<TValue>>> store)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Keyed(owner, key, new StateParameter<Option<TValue>>.OptionalKeyed<TValue, TRaw, TError>(Lens.identity<Option<TValue>>()), store);

    // --- [NAMES]
    public static (RowSource<Gated<TValue>> Source, RowField<Gated<TValue>> Field) OptionalBounded<TValue, TKey, TError>(
        string owner, string name, string caption, string help, TValue @default, Presentation<TValue, TKey> presentation, Func<RowScope, ValueStore<TValue>> store)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
        where TKey : struct, System.Numerics.INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Named(owner, name, caption, help, new Gated<TValue>(Enabled: false, @default),
            new StateParameter<Gated<TValue>>.OptionalBounded<TValue, TKey, TError>(Lens.identity<Gated<TValue>>(), presentation),
            scope => Lifted<TValue, Gated<TValue>>(store(scope), static value => new Gated<TValue>(Enabled: true, value), static gate => gate.Active));

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
        new ConditionalWeakTable<RowScope, Atom<Option<TValue>>>() switch {
            var cells => Opaque(owner, name, @default, scope => RowScope.Cell(cells.GetValue(scope, static _ => Atom(Option<TValue>.None)))),
        };

    // --- [STORES]
    internal static ValueStore<Option<TValue>> Absent<TValue>(ValueStore<TValue> store) where TValue : notnull =>
        Lifted<TValue, Option<TValue>>(store, static value => Some(value), static held => held);

    private static ValueStore<TRecord> Lifted<TValue, TRecord>(ValueStore<TValue> store, Func<TValue, TRecord> held, Func<TRecord, Option<TValue>> value)
        where TValue : notnull
        where TRecord : notnull =>
        new(store.Read.Map(found => found.Map(held)),
            record => store.Put(record.Bind(value)),
            (left, right) => value(left).Match(Some: kept => value(right).Exists(other => store.Same(kept, other)), None: () => value(right).IsNone),
            store.Applied,
            store.Changed);

    private static (RowSource<TValue> Source, RowField<TValue> Field) Keyed<TValue, TRaw, TError>(
        string owner, ValueKey<TValue, TRaw, TError> key, StateParameter<TValue> kind, Func<RowScope, ValueStore<TValue>> store)
        where TValue : notnull
        where TRaw : notnull =>
        (new(owner, scope => IO.pure(Seq(store(scope))), key.Default, held => ValueBinding.Key(owner, key, held)),
         new(new KeyParameter<TValue>(key.Name, kind), key.Caption, key.Help));

    private static (RowSource<TValue> Source, RowField<TValue> Field) Named<TValue>(
        string owner, string name, string caption, string help, TValue @default, StateParameter<TValue> kind, Func<RowScope, ValueStore<TValue>> store) where TValue : notnull =>
        new KeyParameter<TValue>(name, kind) switch {
            var parameter => (new(owner, scope => IO.pure(Seq(store(scope))), @default, held => ValueBinding.Fields(owner, held, @default, Seq<(IStateParameter<TValue>, string)>((parameter, caption)))),
                              new(parameter, caption, help)),
        };
}

public sealed record RowSource<TRecord> : RowSource where TRecord : notnull {
    internal RowSource(string owner, Func<RowScope, IO<Seq<ValueStore<TRecord>>>> stores, TRecord @default, Func<ValueStore<TRecord>, ValueBinding> binding) : base(owner) =>
        (Stores, Default, Binding) = (stores, @default, binding);

    public Func<RowScope, IO<Seq<ValueStore<TRecord>>>> Stores { get; }

    public TRecord Default { get; }

    public Func<ValueStore<TRecord>, ValueBinding> Binding { get; }

    public Seq<EntryKey> Keys(IStateParameter<TRecord> parameter) =>
        FieldTexts.Of(parameter).Map(text => new EntryKey(Owner, text.Path));

    public override ValueBinding Values(RowScope scope) => Binding(scope.Store(this));

    public override IO<ValueSet> Defaults =>
        from memory in RowScope.Held(Some(Default))
        from group in IO.lift(BindingGroup.Of(Seq(Binding(memory))).ToFin())
        from captured in group.Capture
        select captured;

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
            field: static field => Some(field.Source),
            readout: static _ => Option<RowSource>.None,
            command: static _ => Option<RowSource>.None,
            group: static _ => Option<RowSource>.None);

    public Option<RowShape> Layout =>
        Switch(
            field: static field => Some(field.Shape),
            readout: static readout => Some(readout.Shape),
            command: static command => Some(command.Shape),
            group: static _ => Option<RowShape>.None);

    public bool Fill => Layout.Exists(static shape => shape.Fills);

    public static ControlRow Of<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, RowShape shape, Func<RowScope, IO<RowCells>> realize, RowRules rules)
        where TRecord : notnull =>
        Of(source, IterableNE.create(field), field.Caption, field.Help, shape, realize, rules);

    public static ControlRow Of<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, string caption, string help, RowShape shape, Func<RowScope, IO<RowCells>> realize, RowRules rules)
        where TRecord : notnull =>
        new Field(source, toSeq(fields).Bind(field => source.Keys(field.Parameter)), shape, realize, caption, help, rules);

    public sealed record Field : ControlRow {
        internal Field(RowSource source, Seq<EntryKey> keys, RowShape shape, Func<RowScope, IO<RowCells>> realize, string caption, string help, RowRules rules)
            : base(caption, Some(help), keys, rules, Wording.English) =>
            (Source, Shape, Realize) = (source, shape, realize);

        public RowSource Source { get; }
        public RowShape Shape { get; }
        public Func<RowScope, IO<RowCells>> Realize { get; }
    }

    public sealed record Readout : ControlRow {
        public Readout(RowShape shape, Func<RowScope, IO<RowCells>> realize, string caption, string help, RowRules rules)
            : base(caption, Some(help), [], rules, Wording.English) =>
            (Shape, Realize) = (shape, realize);

        public RowShape Shape { get; }
        public Func<RowScope, IO<RowCells>> Realize { get; }
    }

    public sealed record Command(CommandRow Row, RowShape Shape, Func<RowScope, IO<RowCells>> Realize, Option<RowRule> Visible)
        : ControlRow(Row.Face.Caption, Some(Row.Face.Help), [], new RowRules(Row.Enabled, Visible), Row.Face.Wording);

    public sealed record Group(string Caption, RowRules Rules) : ControlRow(Caption, None, [], Rules, Wording.English);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowScope {
    // --- [STATE]
    private static readonly LanguageExt.HashSet<EntryKey> Unlocked = LanguageExt.HashSet<EntryKey>.Empty;

    private readonly Atom<CommitMode> mode;
    private readonly Atom<LanguageExt.HashSet<Func<Seq<EntryKey>, IO<Unit>>>> listeners;

    private RowScope(
        IPlugInSink sink, Option<RhinoDoc> document, Option<SettingsNode> settings, Atom<CommitMode> mode, Option<HistoryBinding> history,
        Option<ValueStore<LanguageExt.HashSet<EntryKey>>> locks, Atom<LanguageExt.HashSet<Func<Seq<EntryKey>, IO<Unit>>>> listeners) =>
        (Sink, Document, Settings, this.mode, History, Locks, this.listeners) = (sink, document, settings, mode, history, locks, listeners);

    public IPlugInSink Sink { get; }
    public Option<RhinoDoc> Document { get; }
    public Option<SettingsNode> Settings { get; }
    public Option<HistoryBinding> History { get; }
    public Option<ValueStore<LanguageExt.HashSet<EntryKey>>> Locks { get; }

    // --- [OPENING]
    public static IO<RowScope> Open(IPlugInSink sink, Option<RhinoDoc> document, CommitMode mode) =>
        IO.lift(() => new RowScope(sink, document, None, Atom(mode), None, None, Atom(LanguageExt.HashSet<Func<Seq<EntryKey>, IO<Unit>>>.Empty)));

    public RowScope Nested(SettingsNode settings, Option<HistoryBinding> history, Option<ValueStore<LanguageExt.HashSet<EntryKey>>> locks) =>
        new(Sink, Document, Some(settings), mode, history, locks, listeners);

    public IO<CommitMode> Mode => mode.ValueIO;

    internal static IO<ValueStore<T>> Held<T>(Option<T> value) where T : notnull =>
        IO.lift(() => Atom(value)).Map(Cell<T>);

    internal static ValueStore<T> Cell<T>(Atom<Option<T>> cell) where T : notnull =>
        ValueStore.Of(cell.ValueIO, next => cell.SwapIO(_ => next).Map(static _ => unit), Applied.Live, None);

    // --- [CHANGES]
    public IO<Unit> Changed(Seq<EntryKey> keys) =>
        listeners.ValueIO
            .Bind(held => toSeq(held).Map<K<IO, Unit>>(heard => heard(keys)).Fails().As())
            .Bind(static fails => unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As());

    public IO<IDisposable> Listen(Func<Seq<EntryKey>, IO<Unit>> heard) =>
        listeners.SwapIO(held => held.TryAdd(heard))
            .Map(_ => (IDisposable)new Disposal<Func<Seq<EntryKey>, IO<Unit>>>(heard, released => _ = listeners.Swap(held => held.Remove(released))));

    // --- [STORES]
    public IO<Seq<ValueStore<T>>> Objects<T>(Func<RhinoObject, ValueStore<T>> store) where T : notnull =>
        Selected(targets => Conversions.Rows(new ObjectPropertiesPageEventArgs(targets.Page).Objects).Map(store).Strict(), static _ => Seq<ValueStore<T>>());

    public IO<Seq<ValueStore<T>>> Viewport<T>(Func<RhinoViewport, ValueStore<T>> store) where T : notnull =>
        Selected(targets => Optional(new ObjectPropertiesPageEventArgs(targets.Page).Viewport).ToSeq().Map(store).Strict(), static _ => Seq<ValueStore<T>>());

    public IO<Seq<ValueStore<T>>> Contents<T>(Func<RenderContent, ValueStore<T>> store) where T : notnull =>
        Selected(static _ => Seq<ValueStore<T>>(), contents => toSeq<RenderContent>(contents.Section.GetSelection()).Map(store).Strict());

    public ValueStore<TRecord> Store<TRecord>(RowSource<TRecord> source) where TRecord : notnull =>
        new(source.Stores(this).Bind(static stores => stores.Head.Match(Some: static store => store.Read, None: static () => IO.pure(Option<TRecord>.None))),
            value => source.Stores(this).Bind(stores => ValueStore.Commit(stores.Map(store => store.Edit(value)))),
            static (_, _) => false,
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

    public IO<ValueDiff> Commit(BindingGroup group, ValueSet incoming) =>
        from locks in Locked
        from held in mode.ValueIO
        from diff in held.Map(immediate: false, revertible: false, deferred: true, targets: false, contents: false)
            ? Drafted(group, incoming, locks)
            : Within(Written(group, incoming, locks))
        from changed in Changed(toSeq(diff.Changes.Keys))
        select diff;

    public IO<ValueSet> Shown(BindingGroup group) =>
        from captured in group.Capture
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
        from written in use(() => new UndoRecord(contents.Section.Caption.Local, model)).Bind(_ => body).Bracket()
        from selected in IO.lift(() => Refused.Unless(contents.Section.SetSelection(contents.Section.GetSelection()), nameof(EtoContentUISection3.SetSelection)))
        select written;

    private IO<ValueDiff> Written(BindingGroup group, ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        History.Match(Some: history => (history with { Group = group }).Commit(incoming, locks), None: () => group.Apply(incoming, locks));

    // --- [PAGES]
    public IO<bool> Pending => Edits.Map(static edits => !edits.Entries.IsEmpty);

    public IO<Unit> Opened(BindingGroup group) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Group: group),
            immediate: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Group.Capture.Bind(opening => state.Scope.Swapped(new CommitMode.Revertible(opening))),
            deferred: static (state, _) => state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty)),
            targets: static (_, _) => IO.pure(unit),
            contents: static (_, _) => IO.pure(unit)));

    public IO<Unit> Apply(BindingGroup group) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Group: group),
            immediate: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Scope.Opened(state.Group),
            deferred: static (state, deferred) =>
                from written in state.Scope.Written(state.Group, deferred.Edits, Unlocked)
                from cleared in state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty))
                from changed in state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys))
                select unit,
            targets: static (_, _) => IO.pure(unit),
            contents: static (_, _) => IO.pure(unit)));

    public IO<Unit> Revert(BindingGroup group) =>
        mode.ValueIO.Bind(held => held.Switch(
            (Scope: this, Group: group),
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

    private IO<ValueDiff> Drafted(BindingGroup group, ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        from before in group.Capture
        from edits in Edits
        let shown = edits.Over(before, Unlocked)
        let target = incoming.Over(shown, locks)
        let keys = group.Keys()
        from recalled in IO.lift(group.Bindings.Traverse(binding => binding.Recall(Named(target, binding))).As().ToFin())
        from swapped in Swapped(new CommitMode.Deferred(new ValueSet(
            edits.Entries.Filter((key, _) => !keys.Exists(held => held == key))
                .Union(target.Entries.Filter((key, text) => !before.Entries.Find(key).Exists(held => held == text))))))
        select shown.Diff(target);

    private ValueStore<TRecord> Drafted<TRecord>(RowSource<TRecord> source, ValueStore<TRecord> store) where TRecord : notnull =>
        store with {
            Read =
                from held in store.Read
                from edits in Edits
                let owned = toHashMap(edits.Entries.AsIterable().Filter(entry => entry.Key.Owner == source.Owner).Map(static entry => (entry.Key.Path, entry.Value)))
                from shown in owned.IsEmpty ? IO.pure(held) : Recalled(source, held.IfNone(source.Default), owned).Map(static record => Some(record))
                select shown,
            Put = value =>
                from texts in Held(Some(value.IfNone(source.Default))).Bind(memory => source.Binding(memory).Capture)
                from stored in source.Binding(store).Capture
                from edits in Edits
                from swapped in Swapped(new CommitMode.Deferred(new ValueSet(
                    edits.Entries.Filter((key, _) => key.Owner != source.Owner)
                        .Union(toHashMap(texts.AsIterable()
                            .Filter(entry => !stored.Find(entry.Key).Exists(held => held == entry.Value))
                            .Map(entry => (new EntryKey(source.Owner, entry.Key), entry.Value)))))))
                select unit,
            Applied = Applied.Live,
        };

    private static IO<TRecord> Recalled<TRecord>(RowSource<TRecord> source, TRecord record, HashMap<Seq<string>, string> texts) where TRecord : notnull =>
        from memory in Held(Some(record))
        from write in IO.lift(source.Binding(memory).Recall(texts).ToFin())
        from written in write
        from read in memory.Read
        select read.IfNone(record);

    private static HashMap<Seq<string>, string> Named(ValueSet values, ValueBinding binding) =>
        toHashMap(binding.Paths().Choose(path => values.Entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))));
}
