using System.Runtime.CompilerServices;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record CommitMode {
    private CommitMode(bool drafts) => Drafts = drafts;

    public bool Drafts { get; }

    public sealed record Immediate() : CommitMode(drafts: false);

    public sealed record Revertible(ValueSet Opening) : CommitMode(drafts: false);

    public sealed record Deferred(ValueSet Edits) : CommitMode(drafts: true);

    public sealed record Targets : CommitMode {
        internal Targets(ObjectPropertiesPage page) : base(drafts: false) => Page = page;

        public ObjectPropertiesPage Page { get; }
    }

    public sealed record Contents : CommitMode {
        internal Contents(EtoContentUISection3 section) : base(drafts: false) => Section = section;

        public EtoContentUISection3 Section { get; }
    }
}

public sealed record ControlRows(Func<RowScope, IO<Seq<ControlRow>>> Read, Func<RowScope, IO<Seq<HostEvent<Unit>>>> Signals) {
    public bool Fill { get; init; }
    public Option<Func<RowScope, ValueBinding>> Bound { get; init; }

    public static ControlRows Fixed(Seq<ControlRow> rows) =>
        new(_ => IO.pure(rows), static _ => IO.pure(Seq<HostEvent<Unit>>())) { Fill = rows.Exists(static row => row.Shape.Fills) };

    public static ControlRows Stored<TState>(string owner, Func<RowScope, ValueStore<TState>> store, TState @default, Func<TState, Func<TState, IO<Unit>>, Seq<ControlRow>> rows)
        where TState : notnull =>
        new(scope => Recalled(owner, new BindingGroup(Seq(ValueBinding.Of(owner, @default, store(scope)))), @default, rows, scope), scope => IO.pure(store(scope).Changed.ToSeq())) {
            Bound = Some<Func<RowScope, ValueBinding>>(scope => ValueBinding.Of(owner, @default, store(scope))),
        };

    private static IO<Seq<ControlRow>> Recalled<TState>(string owner, BindingGroup bound, TState @default, Func<TState, Func<TState, IO<Unit>>, Seq<ControlRow>> rows, RowScope scope)
        where TState : notnull =>
        from shown in scope.Shown(bound)
        from value in IO.lift(shown.Recalled(owner, @default))
        select rows(value, next => scope.Commit(bound, ValueSet.Fields(owner, next)).Map(static _ => unit));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowScope {
    // --- [STATE]
    private static readonly HashSet<EntryKey> Unlocked = [];

    private readonly Atom<CommitMode> mode;
    private readonly Atom<HashSet<Func<Seq<EntryKey>, IO<Unit>>>> listeners;
    private readonly Atom<(bool Visible, HashSet<Func<bool, IO<Unit>>> Listeners)> visibility;

    private RowScope(
        IPlugInLifetime owner, Option<RhinoDoc> document, Atom<CommitMode> mode, Atom<HashSet<Func<Seq<EntryKey>, IO<Unit>>>> listeners,
        Atom<(bool Visible, HashSet<Func<bool, IO<Unit>>> Listeners)> visibility) =>
        (Owner, Document, this.mode, this.listeners, this.visibility) = (owner, document, mode, listeners, visibility);

    public IPlugInLifetime Owner { get; }
    public Option<RhinoDoc> Document { get; }
    public IO<RhinoDoc> DocumentIO => IO.lift(() => Conversions.Found(Document));
    public Option<SettingsNode> Settings { get; init; }
    public Option<HistoryBinding> History { get; init; }
    public Option<ValueStore<HashSet<EntryKey>>> Locks { get; init; }
    public Option<Control> Host { get; init; }

    // --- [OPENING]
    public static IO<RowScope> Open(IPlugInLifetime owner, Option<RhinoDoc> document, CommitMode mode) =>
        IO.lift(() => new RowScope(owner, document, Atom(mode), Atom(HashSet<Func<Seq<EntryKey>, IO<Unit>>>()), Atom((Visible: true, Listeners: HashSet<Func<bool, IO<Unit>>>()))));

    public RowScope Nested(SettingsNode settings, Option<HistoryBinding> history, Option<ValueStore<HashSet<EntryKey>>> locks) =>
        new(Owner, Document, mode, listeners, Atom((visibility.Value.Visible, HashSet<Func<bool, IO<Unit>>>()))) { Settings = Some(settings), History = history, Locks = locks, Host = Host };

    public RowScope Hosted(Control host) => new(Owner, Document, mode, listeners, visibility) { Settings = Settings, History = History, Locks = Locks, Host = Some(host) };

    public RowScope Separated(bool visible) =>
        new(Owner, Document, mode, listeners, Atom((Visible: visible, Listeners: HashSet<Func<bool, IO<Unit>>>()))) { Settings = Settings, History = History, Locks = Locks, Host = Host };

    public IO<CommitMode> Mode => mode.ValueIO;

    // --- [VISIBILITY]
    public IO<bool> Visibility => visibility.ValueIO.Map(static held => held.Visible);

    public IO<Unit> Shown(bool visible) => visibility.SwapIO(held => held with { Visible = visible })
        .Bind(held => Callbacks.Each(toSeq(held.Listeners).Map(listener => listener(visible)))).Map(unit);

    public IO<Unit> OnVisibility(Func<bool, IO<Unit>> heard) =>
        from added in visibility.SwapIO(held => held with { Listeners = held.Listeners.TryAdd(heard) })
        from removal in Products.Released(() => visibility.Swap(held => held with { Listeners = held.Listeners.Remove(heard) }))
        from visible in Visibility
        from told in heard(visible)
        select unit;

    // --- [CHANGES]
    public IO<Unit> Changed(Seq<EntryKey> keys) =>
        listeners.ValueIO.Bind(held => Callbacks.Each(toSeq(held).Map(heard => heard(keys)))).Map(unit);

    public IO<Unit> Listen(Func<Seq<EntryKey>, IO<Unit>> heard) =>
        from added in listeners.SwapIO(held => held.TryAdd(heard))
        from removal in Products.Released(() => listeners.Swap(held => held.Remove(heard)))
        select unit;

    internal HostEvent<T> Listened<T>(Func<RowScope, IO<T>> read, [CallerArgumentExpression(nameof(read))] string label = "") =>
        new(label, (deliver, _) => Listen(_ => read(this).Bind(deliver)));

    // --- [STORES]
    public IO<Seq<ValueStore<T>>> Objects<T>(Func<RhinoObject, ValueStore<T>> store) where T : notnull =>
        Selected(
            targets => IO.lift(() => Conversions.Rows(new ObjectPropertiesPageEventArgs(targets.Page).Objects).Map(store).Strict()),
            static _ => IO.pure(Seq<ValueStore<T>>()));

    public IO<Seq<ValueStore<T>>> Viewport<T>(Func<RhinoDoc, ViewportTarget, ValueStore<T>> store) where T : notnull =>
        Selected(
            targets => DocumentIO.Map(doc => Optional(new ObjectPropertiesPageEventArgs(targets.Page).Viewport)
                .ToSeq().Map(port => store(doc, new ViewportTarget.Id(port.Id))).Strict()),
            static contents => Untargeted<T>(contents));

    public IO<Seq<ValueStore<T>>> Contents<T>(Func<RenderContent, ValueStore<T>> store) where T : notnull =>
        Selected(
            static _ => IO.pure(Seq<ValueStore<T>>()),
            contents => IO.lift(() => toSeq(contents.Section.GetSelection()).Map(store).Strict()));

    private IO<Seq<ValueStore<T>>> Selected<T>(Func<CommitMode.Targets, IO<Seq<ValueStore<T>>>> targets, Func<CommitMode.Contents, IO<Seq<ValueStore<T>>>> contents) where T : notnull =>
        mode.ValueIO.Bind(held => held.SwitchPartially(
            (Targets: targets, Contents: contents),
            @default: static (_, other) => Untargeted<T>(other),
            targets: static (of, page) => of.Targets(page),
            contents: static (of, section) => of.Contents(section)));

    private static IO<Seq<ValueStore<T>>> Untargeted<T>(CommitMode held) where T : notnull =>
        IO.fail<Seq<ValueStore<T>>>(new RhinoFault(held.GetType().Name, RhinoReason.Mismatched));

    // --- [COMMITS]
    public IO<HashSet<EntryKey>> Locked =>
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
            targets: static (state, targets) => Callbacks.Captured<ObjectPropertiesPageEventArgs, A>(targets.Page.ModifyPage, _ => state.Scope.Recorded(targets, state.Body)),
            contents: static (state, contents) => Recorded(contents, state.Body)));

    private IO<A> Recorded<A>(CommitMode.Targets targets, IO<A> body) =>
        from document in DocumentIO
        from committed in Commits.Commit(document, targets.Page.EnglishPageTitle, new RedrawPolicy.AllViews(Deferred: true), body)
        select committed.Value;

    private static IO<A> Recorded<A>(CommitMode.Contents contents, IO<A> body) =>
        from written in SectionModels.UndoRecord(contents.Section, contents.Section.Caption.Local, body)
        from selected in IO.lift(() => RhinoFault.Unless(contents.Section.SetSelection(contents.Section.GetSelection())))
        select written;

    private IO<ValueDiff> Written(BindingGroup bound, ValueSet incoming, HashSet<EntryKey> locks) =>
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

    private IO<ValueDiff> Drafted(BindingGroup bound, ValueSet incoming, HashSet<EntryKey> locks) =>
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

    private static HashMap<Seq<string>, string> Named(ValueSet values, ValueBinding binding) =>
        toHashMap(binding.Paths().Choose(path => values.Entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))));
}
