using System.Runtime.CompilerServices;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rasm.UI;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record CommitMode {
    public bool Drafts => MapPartially(@default: false, deferred: true);

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

public abstract record RowSource {
    // --- [BINDING]
    private protected RowSource(string owner) => Owner = owner;

    public string Owner { get; }

    public Option<PlugInSetting> Setting { get; init; }

    public abstract ValueBinding Values(RowScope scope);

    public abstract IO<Seq<HostEvent<Unit>>> Signals(RowScope scope);

    // --- [RECORDS]
    public static RowSource<TRecord> Fields<TRecord>(
        string owner, Seq<IStateParameter<TRecord>> fields, Func<RowScope, ValueStore<TRecord>> store) where TRecord : IStateRecord<TRecord> =>
        Fields(owner, fields, scope => IO.pure(Seq(store(scope))));

    public static RowSource<TRecord> Fields<TRecord>(
        string owner, Seq<IStateParameter<TRecord>> fields, Func<RowScope, IO<Seq<ValueStore<TRecord>>>> stores, Option<TRecord> factory = default) where TRecord : IStateRecord<TRecord> =>
        new(owner, stores, factory.IfNone(TRecord.Default), fields);

    // --- [KEYS]
    public static RowSource<TValue> Of<TValue>(string owner, ValueKey<TValue> key, Func<RowScope, ValueStore<TValue>> store) where TValue : notnull =>
        new(owner, scope => IO.pure(Seq(store(scope))), key.Default, [key]);

    public static RowSource<TValue> Of<TValue>(string owner, ValueKey<TValue> key) where TValue : notnull =>
        new ConditionalWeakTable<RowScope, Ref<Option<TValue>>>() switch {
            var cells => Of(owner, key, scope => RowScope.Cell(cells.GetValue(scope, static _ => Ref(Option<TValue>.None)))),
        };

    public static RowSource<TValue> Of<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Of(nameof(PlugInSettings), row.Key, scope => PlugInSettings.Store(((IPlugInViews)scope.Sink).Settings, row)) with { Setting = Some<PlugInSetting>(row) };
}

public sealed record RowSource<TRecord> : RowSource where TRecord : notnull {
    internal RowSource(string owner, Func<RowScope, IO<Seq<ValueStore<TRecord>>>> stores, TRecord @default, Seq<IStateParameter<TRecord>> fields) : base(owner) =>
        (Stores, Default, Fields) = (stores, @default, fields);

    public Func<RowScope, IO<Seq<ValueStore<TRecord>>>> Stores { get; }

    public TRecord Default { get; }

    public Seq<IStateParameter<TRecord>> Fields { get; }

    public ValueBinding Binding(ValueStore<TRecord> store) => ValueBinding.Of(Owner, new FieldCodec<TRecord>(Fields, Default), store).Under(Path);

    public Seq<string> Path { get; private init; }

    public Seq<EntryKey> Keys(IStateParameter<TRecord> parameter) =>
        RecordEntries.Of(parameter).Map(entry => new EntryKey(Owner, Path + entry.Path));

    public RowSource<TNested> Within<TNested>(Seq<string> path, Lens<TRecord, TNested> lens, Seq<IStateParameter<TNested>> fields)
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
                    from prepared in Callbacks.Each(stores.Map(store => Binding(store).Prepare(entries)))
                    select Callbacks.Each(prepared).Map(ValueDiff.Together),
            },
        };

    public ValueSet Capture(TRecord record) => new(toHashMap(Fields.Bind(RecordEntries.Of)
        .Choose(field => field.Capture(record).Map(text => (new EntryKey(Owner, Path + field.Path), text)))));

    public IO<ValueDiff> Commit(RowScope scope, Func<TRecord, TRecord> change, Seq<IStateParameter<TRecord>> writes) =>
        from held in scope.Read(this)
        let written = toHashSet(writes.Bind(Keys))
        from diff in scope.Commit(new BindingGroup(Seq(Values(scope))), new ValueSet(Capture(change(held)).Entries.Filter((key, _) => written.Contains(key))))
        select diff;

    public override IO<Seq<HostEvent<Unit>>> Signals(RowScope scope) =>
        Stores(scope).Map(static stores => stores.Choose(static store => store.Changed));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlRow {
    private ControlRow(ParameterText text, Seq<EntryKey> keys, RowRules rules, Option<(RowShape Shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> Realize)> cells, Option<RowSource> bound) =>
        (Text, Keys, Rules, Cells, Bound) = (text, keys, rules, cells, bound);

    public ParameterText Text { get; }

    public Seq<EntryKey> Keys { get; }

    public RowRules Rules { get; }

    public Option<(RowShape Shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> Realize)> Cells { get; }

    public Option<RowSource> Bound { get; }

    public bool Fill => Cells.Exists(static cells => cells.Shape.Fills);

    public static ControlRow Of<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, RowRules rules)
        where TRecord : notnull =>
        new Field(source, source.Keys(parameter), shape, realize, parameter.Text, rules);

    public static ControlRow Of<TRecord>(
        RowSource<TRecord> source, IterableNE<IStateParameter<TRecord>> parameters, ParameterText text, RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, RowRules rules)
        where TRecord : notnull =>
        new Field(source, toSeq(parameters).Bind(source.Keys), shape, realize, text, rules);

    public sealed record Field : ControlRow {
        public Field(RowSource source, Seq<EntryKey> keys, RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, ParameterText text, RowRules rules)
            : base(text, keys, rules, Some((shape, realize)), Some(source)) { }
    }

    public sealed record Readout : ControlRow {
        public Readout(RowShape shape, Func<RowScope, Action<IDisposable>, IO<RowCells>> realize, ParameterText text, RowRules rules)
            : base(text, [], rules, Some((shape, realize)), None) { }
    }

    public sealed record Group : ControlRow {
        public Group(ParameterText text, RowRules rules) : base(text, [], rules, None, None) { }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowScope {
    // --- [STATE]
    private static readonly LanguageExt.HashSet<EntryKey> Unlocked = [];
    private static readonly Refused Untargeted = new(nameof(CommitMode.Targets), Refused.Absent);

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
    public IO<RhinoDoc> DocumentIO => IO.lift(() => Document.ToFin(new Refused(nameof(Document), Refused.Absent)));
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

    public RowScope Separated(bool visible) =>
        new(Sink, Document, Settings, mode, History, Locks, listeners, Host, Atom((Visible: visible, Listeners: LanguageExt.HashSet<Func<bool, IO<Unit>>>.Empty)));

    public IO<CommitMode> Mode => mode.ValueIO;

    // --- [VISIBILITY]
    public IO<bool> Visibility => visibility.ValueIO.Map(static held => held.Visible);

    public IO<Unit> Shown(bool visible) => visibility.SwapIO(held => held with { Visible = visible })
        .Bind(held => Callbacks.Each(toSeq(held.Listeners).Map(listener => listener(visible)))).Map(unit);

    public IO<Unit> OnVisibility(Func<bool, IO<Unit>> heard, Action<IDisposable> retain) =>
        DisposalOps.Acquire(visibility.SwapIO(held => held with { Listeners = held.Listeners.TryAdd(heard) }),
            _ => visibility.SwapIO(held => held with { Listeners = held.Listeners.Remove(heard) }).Map(unit), retain).Bind(_ => Visibility).Bind(heard);

    // --- [CHANGES]
    public IO<Unit> Changed(Seq<EntryKey> keys) =>
        listeners.ValueIO
            .Bind(held => toSeq(held).Map<K<IO, Unit>>(heard => heard(keys)).Fails().As())
            .Bind(static fails => unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As());

    public IO<Unit> Listen(Func<Seq<EntryKey>, IO<Unit>> heard, Action<IDisposable> retain) =>
        DisposalOps.Acquire(listeners.SwapIO(held => held.TryAdd(heard)), _ => listeners.SwapIO(held => held.Remove(heard)).Map(unit), retain).Map(unit);

    // --- [STORES]
    public IO<Seq<ValueStore<T>>> Objects<T>(Func<RhinoObject, ValueStore<T>> store) where T : notnull =>
        Selected(
            targets => IO.lift(() => Conversions.Rows(new ObjectPropertiesPageEventArgs(targets.Page).Objects).Map(store).Strict()),
            static _ => IO.pure(Seq<ValueStore<T>>()));

    public IO<Seq<ValueStore<T>>> Viewport<T>(Func<RhinoDoc, ViewportTarget, ValueStore<T>> store) where T : notnull =>
        Selected(
            targets => DocumentIO.Map(doc => Optional(new ObjectPropertiesPageEventArgs(targets.Page).Viewport)
                .ToSeq().Map(port => store(doc, new ViewportTarget.Id(port.Id))).Strict()),
            static _ => IO.fail<Seq<ValueStore<T>>>(Untargeted));

    public IO<Seq<ValueStore<T>>> Contents<T>(Func<RenderContent, ValueStore<T>> store) where T : notnull =>
        Selected(
            static _ => IO.pure(Seq<ValueStore<T>>()),
            contents => IO.lift(() => toSeq(contents.Section.GetSelection()).Map(store).Strict()));

    public ValueStore<TRecord> Store<TRecord>(RowSource<TRecord> source) where TRecord : notnull =>
        new(source.Stores(this).Bind(static stores => stores.Head.Match(Some: static store => store.Read, None: static () => IO.pure(Option<TRecord>.None))),
            value => source.Stores(this).Bind(stores =>
                ValueStore.Commit(stores.Map(store => store.Apply(value)))).Map(static changes => changes.Find(static change => change.HasChanged).IfNone(Change<TRecord>.None)),
            EqualityComparer<TRecord>.Default.Equals,
            Applied.Live,
            None);

    public IO<Seq<ValueStore<TRecord>>> Edited<TRecord>(RowSource<TRecord> source) where TRecord : notnull =>
        mode.ValueIO.Bind(held => held.Drafts
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
        }), EqualityComparer<T>.Default.Equals, Applied.Live, Some(Subscriptions.Changed(cell).Choose(static _ => Some(unit))));

    internal IO<bool> Paced => mode.ValueIO.Map(static held => held.MapPartially(@default: true, targets: false, contents: false));

    internal IO<bool> Records => mode.ValueIO.Map(static held => held.MapPartially(@default: false, immediate: true, revertible: true));

    private IO<Seq<ValueStore<T>>> Selected<T>(Func<CommitMode.Targets, IO<Seq<ValueStore<T>>>> targets, Func<CommitMode.Contents, IO<Seq<ValueStore<T>>>> contents) where T : notnull =>
        mode.ValueIO.Bind(held => held.SwitchPartially(
            (Targets: targets, Contents: contents),
            @default: static (_, _) => IO.fail<Seq<ValueStore<T>>>(Untargeted),
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
        from diff in held.Drafts
            ? Drafted(bound, incoming, locks)
            : Within(Written(bound, incoming, locks))
        from changed in Changed(toSeq(diff.Changes.Keys))
        select diff;

    public IO<ValueSet> Shown(BindingGroup bound) =>
        from captured in bound.Capture
        from edits in Edits
        select edits.Over(captured, Unlocked);

    internal IO<A> Within<A>(IO<A> body) =>
        mode.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Body: body),
            @default: static (state, _) => state.Body,
            targets: static (state, targets) => Callbacks.Captured<ObjectPropertiesPageEventArgs, A>(
                targets.Page.ModifyPage, _ => state.Scope.Recorded(targets, state.Body), nameof(ObjectPropertiesPage.ModifyPage)),
            contents: static (state, contents) => Recorded(contents, state.Body)));

    private IO<A> Recorded<A>(CommitMode.Targets targets, IO<A> body) =>
        from document in DocumentIO
        from committed in Commits.Commit(document, targets.Page.EnglishPageTitle, new RedrawPolicy.AllViews(Deferred: true), body)
        select committed.Value;

    private static IO<A> Recorded<A>(CommitMode.Contents contents, IO<A> body) =>
        from written in SectionModels.UndoRecord(contents.Section, contents.Section.Caption.Local, body)
        from selected in IO.lift(() => Refused.Unless(contents.Section.SetSelection(contents.Section.GetSelection()), nameof(EtoContentUISection3.SetSelection)))
        select written;

    private IO<ValueDiff> Written(BindingGroup bound, ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        History.Match(Some: history => (history with { Group = bound }).Commit(incoming, locks), None: () => bound.Apply(incoming, locks));

    // --- [PAGES]
    public IO<bool> Pending => Edits.Map(static edits => !edits.Entries.IsEmpty);

    public IO<Unit> Opened(BindingGroup bound) =>
        mode.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Group: bound),
            @default: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Group.Capture.Bind(opening => state.Scope.Swapped(new CommitMode.Revertible(opening))),
            deferred: static (state, _) => state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty))));

    public IO<Unit> Apply(BindingGroup bound) =>
        mode.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Group: bound),
            @default: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Scope.Opened(state.Group),
            deferred: static (state, deferred) =>
                from written in state.Scope.Written(state.Group, deferred.Edits, Unlocked)
                from cleared in state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty))
                from changed in state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys))
                select unit));

    public IO<Unit> Revert(BindingGroup bound) =>
        mode.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Group: bound),
            @default: static (_, _) => IO.pure(unit),
            revertible: static (state, revertible) =>
                state.Scope.Written(state.Group, revertible.Opening, Unlocked).Bind(diff => state.Scope.Changed(toSeq(diff.Changes.Keys))),
            deferred: static (state, deferred) =>
                state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty)).Bind(_ => state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys)))));

    // --- [DRAFTS]
    private IO<ValueSet> Edits =>
        mode.ValueIO.Map(static held => held.SwitchPartially(@default: static _ => ValueSet.Empty, deferred: static deferred => deferred.Edits));

    private IO<Unit> Swapped(CommitMode next) => writeIO(mode, next).Map(unit);

    private IO<ValueDiff> Drafted(BindingGroup bound, ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        from before in bound.Capture
        from edits in Edits
        let shown = edits.Over(before, Unlocked)
        let target = incoming.Over(shown, locks)
        let keys = bound.Keys()
        from recalled in Callbacks.Each(bound.Bindings
            .Map(binding => (Binding: binding, Entries: Named(new ValueSet(incoming.Entries.Except(locks)), binding)))
            .Filter(static row => !row.Entries.IsEmpty)
            .Map(static row => row.Binding.Prepare(row.Entries)))
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
        new FieldCodec<TRecord>(source.Fields, record).Recalled(source.Owner, path => IO.pure(texts.Find(source.Path + path)));

    private static HashMap<Seq<string>, string> Named(ValueSet values, ValueBinding binding) =>
        toHashMap(binding.Paths().Choose(path => values.Entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))));
}
