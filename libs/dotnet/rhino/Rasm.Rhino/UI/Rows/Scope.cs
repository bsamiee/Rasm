using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Views;
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

public sealed record RowRule(Func<RowScope, IO<bool>> Holds) {
    public static RowRule operator &(RowRule left, RowRule right) => BitwiseAnd(left, right);

    public static RowRule operator |(RowRule left, RowRule right) => BitwiseOr(left, right);

    public static RowRule operator !(RowRule rule) => LogicalNot(rule);

    public static RowRule BitwiseAnd(RowRule left, RowRule right) => new(scope => left.Holds(scope).Bind(held => held ? right.Holds(scope) : IO.pure(value: false)));

    public static RowRule BitwiseOr(RowRule left, RowRule right) => new(scope => left.Holds(scope).Bind(held => held ? IO.pure(value: true) : right.Holds(scope)));

    public static RowRule LogicalNot(RowRule rule) => new(scope => rule.Holds(scope).Map(static held => !held));

    public static RowRule When<T>(Func<RowScope, IO<T>> read, Func<T, bool> holds) => new(scope => read(scope).Map(holds));
}

public sealed record ControlRows(Func<RowScope, IO<Seq<Row>>> Read, Func<RowScope, IO<Seq<HostEvent<Unit>>>> Signals) {
    public bool Fill { get; init; }
    public Option<Func<RowScope, ValueBinding>> Bound { get; init; }

    public static ControlRows Fixed(Seq<Row> rows) =>
        new(_ => IO.pure(rows), static _ => IO.pure(Seq<HostEvent<Unit>>())) { Fill = rows.Exists(static row => row.Shape.Fills) };

    public static ControlRows Stored<TState>(string owner, Func<RowScope, Preference<TState>> store, TState @default, Func<TState, Func<TState, IO<Unit>>, Seq<Row>> rows)
        where TState : notnull =>
        new(scope => Recalled(owner, new BindingGroup(Seq(ValueBinding.Of(owner, @default, store(scope)))), @default, rows, scope), scope => IO.pure(Seq(new HostEvent<TState>(owner, store(scope).Changed).Choose(static _ => Some(unit))))) {
            Bound = Some<Func<RowScope, ValueBinding>>(scope => ValueBinding.Of(owner, @default, store(scope))),
        };

    private static IO<Seq<Row>> Recalled<TState>(string owner, BindingGroup bound, TState @default, Func<TState, Func<TState, IO<Unit>>, Seq<Row>> rows, RowScope scope)
        where TState : notnull =>
        from shown in scope.Shown(bound)
        from value in IO.lift(shown.Recalled(owner, @default))
        select rows(value, next => scope.Commit(bound, ValueSet.Fields(owner, next)).Map(static _ => unit));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record RowScope {
    // --- [STATE]
    private static readonly HashSet<EntryKey> Unlocked = [];

    private RowScope(Option<RhinoDoc> document, Atom<CommitMode> mode) =>
        (Document, Moded, Listeners, Visibilities) = (document, mode, Atom(HashSet<Func<Seq<EntryKey>, IO<Unit>>>()), Fresh(visible: true));

    public Option<RhinoDoc> Document { get; }
    public IO<RhinoDoc> DocumentIO => IO.lift(() => Conversions.Required(Document));
    public Option<SettingsNode> Settings { get; init; }
    public Option<HistoryBinding> History { get; init; }
    public Option<Preference<HashSet<EntryKey>>> Locks { get; init; }

    private Atom<CommitMode> Moded { get; }
    private Atom<HashSet<Func<Seq<EntryKey>, IO<Unit>>>> Listeners { get; }
    private Atom<(bool Visible, HashSet<Func<bool, IO<Unit>>> Heard)> Visibilities { get; init; }

    private static Atom<(bool Visible, HashSet<Func<bool, IO<Unit>>> Heard)> Fresh(bool visible) => Atom((Visible: visible, Heard: HashSet<Func<bool, IO<Unit>>>()));

    // --- [OPENING]
    public static IO<RowScope> Open(Option<RhinoDoc> document, CommitMode mode) => IO.lift(() => new RowScope(document, Atom(mode)));

    public RowScope Nested(SettingsNode settings, Option<HistoryBinding> history, Option<Preference<HashSet<EntryKey>>> locks) =>
        this with { Settings = Some(settings), History = history, Locks = locks, Visibilities = Fresh(Visibilities.Value.Visible) };

    public RowScope Separated(bool visible) => this with { Visibilities = Fresh(visible) };

    public IO<CommitMode> Mode => Moded.ValueIO;

    // --- [VISIBILITY]
    public IO<bool> Visibility => Visibilities.ValueIO.Map(static held => held.Visible);

    public IO<Unit> Shown(bool visible) => Visibilities.SwapIO(held => held with { Visible = visible })
        .Bind(held => Products.Each(toSeq(held.Heard).Map(listener => listener(visible)))).Map(unit);

    public IO<Unit> OnVisibility(Func<bool, IO<Unit>> heard) =>
        from added in Visibilities.SwapIO(held => held with { Heard = held.Heard.TryAdd(heard) })
        from removal in Products.Released(() => Visibilities.Swap(held => held with { Heard = held.Heard.Remove(heard) }))
        from told in heard(added.Visible)
        select unit;

    // --- [CHANGES]
    public IO<Unit> Changed(Seq<EntryKey> keys) =>
        Listeners.ValueIO.Bind(held => Products.Each(toSeq(held).Map(heard => heard(keys)))).Map(unit);

    public IO<Unit> Listen(Func<Seq<EntryKey>, IO<Unit>> heard) =>
        from added in Listeners.SwapIO(held => held.TryAdd(heard))
        from removal in Products.Released(() => Listeners.Swap(held => held.Remove(heard)))
        select unit;

    internal HostEvent<T> Listened<T>(Func<RowScope, IO<T>> read, [CallerArgumentExpression(nameof(read))] string label = "") =>
        new(label, deliver => Listen(_ => read(this).Bind(deliver)));

    // --- [STORES]
    public IO<Seq<Preference<T>>> Objects<T>(Func<RhinoObject, Preference<T>> store) where T : notnull =>
        Selected(
            targets => IO.lift(() => Conversions.Rows(new ObjectPropertiesPageEventArgs(targets.Page).Objects).Map(store).Strict()),
            static _ => IO.pure(Seq<Preference<T>>()));

    public IO<Seq<Preference<T>>> Viewport<T>(Func<RhinoDoc, ViewportTarget, Preference<T>> store) where T : notnull =>
        Selected(
            targets => DocumentIO.Map(doc => Optional(new ObjectPropertiesPageEventArgs(targets.Page).Viewport)
                .ToSeq().Map(port => store(doc, new ViewportTarget.Id(port.Id))).Strict()),
            static contents => Untargeted<T>(contents));

    public IO<Seq<Preference<T>>> Contents<T>(Func<RenderContent, Preference<T>> store) where T : notnull =>
        Selected(
            static _ => IO.pure(Seq<Preference<T>>()),
            contents => IO.lift(() => toSeq(contents.Section.GetSelection()).Map(store).Strict()));

    private IO<Seq<Preference<T>>> Selected<T>(Func<CommitMode.Targets, IO<Seq<Preference<T>>>> targets, Func<CommitMode.Contents, IO<Seq<Preference<T>>>> contents) where T : notnull =>
        Moded.ValueIO.Bind(held => held.SwitchPartially(
            (Targets: targets, Contents: contents),
            @default: static (_, other) => Untargeted<T>(other),
            targets: static (of, page) => of.Targets(page),
            contents: static (of, section) => of.Contents(section)));

    private static IO<Seq<Preference<T>>> Untargeted<T>(CommitMode held) where T : notnull =>
        IO.fail<Seq<Preference<T>>>(new RhinoFault(held.GetType().Name, RhinoReason.Mismatched));

    // --- [COMMITS]
    public IO<HashSet<EntryKey>> Locked =>
        Locks.Match(Some: static store => store.Read, None: static () => IO.pure(Unlocked));

    public IO<Unit> Lock(Seq<EntryKey> keys, bool locked) =>
        Locks.Match(
            Some: store =>
                from held in Locked
                let next = locked ? held.Union(keys) : held.Except(keys)
                from written in store.Write(next)
                from changed in Changed(keys)
                select unit,
            None: static () => IO.pure(unit));

    public IO<ValueDiff> Commit(BindingGroup bound, ValueSet incoming) =>
        from locks in Locked
        from held in Moded.ValueIO
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
        Moded.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Body: body),
            @default: static (state, _) => state.Body,
            targets: static (state, targets) => Products.Captured<ObjectPropertiesPageEventArgs, A>(targets.Page.ModifyPage, _ => state.Scope.Recorded(targets, state.Body)),
            contents: static (state, contents) => Recorded(contents, state.Body)));

    private IO<A> Recorded<A>(CommitMode.Targets targets, IO<A> body) =>
        from document in DocumentIO
        from committed in Commits.Commit(document, targets.Page.EnglishPageTitle, new RedrawPolicy.AllViews(Deferred: true), body)
        select committed.Value;

    private static IO<A> Recorded<A>(CommitMode.Contents contents, IO<A> body) =>
        from written in SectionModels.UndoRecord(contents.Section, contents.Section.Caption.Local, body)
        from selected in Products.Accepted(() => contents.Section.SetSelection(contents.Section.GetSelection()))
        select written;

    private IO<ValueDiff> Written(BindingGroup bound, ValueSet incoming, HashSet<EntryKey> locks) =>
        History.Match(Some: history => (history with { Group = bound }).Commit(incoming, locks), None: () => bound.Apply(incoming, locks));

    // --- [PAGES]
    public IO<bool> Pending => Edits.Map(static edits => !edits.Entries.IsEmpty);

    public IO<Unit> Opened(BindingGroup bound) =>
        Moded.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Group: bound),
            @default: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Group.Capture.Bind(opening => state.Scope.Swapped(new CommitMode.Revertible(opening))),
            deferred: static (state, _) => state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty))));

    public IO<Unit> Apply(BindingGroup bound) =>
        Moded.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Group: bound),
            @default: static (_, _) => IO.pure(unit),
            revertible: static (state, _) => state.Scope.Opened(state.Group),
            deferred: static (state, deferred) =>
                from written in state.Scope.Written(state.Group, deferred.Edits, Unlocked)
                from cleared in state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty))
                from changed in state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys))
                select unit));

    public IO<Unit> Revert(BindingGroup bound) =>
        Moded.ValueIO.Bind(held => held.SwitchPartially(
            (Scope: this, Group: bound),
            @default: static (_, _) => IO.pure(unit),
            revertible: static (state, revertible) =>
                state.Scope.Written(state.Group, revertible.Opening, Unlocked).Bind(diff => state.Scope.Changed(toSeq(diff.Changes.Keys))),
            deferred: static (state, deferred) =>
                state.Scope.Swapped(new CommitMode.Deferred(ValueSet.Empty)).Bind(_ => state.Scope.Changed(toSeq(deferred.Edits.Entries.Keys)))));

    // --- [DRAFTS]
    private IO<ValueSet> Edits =>
        Moded.ValueIO.Map(static held => held.SwitchPartially(@default: static _ => ValueSet.Empty, deferred: static deferred => deferred.Edits));

    private IO<Unit> Swapped(CommitMode next) => writeIO(Moded, next).Map(unit);

    private IO<ValueDiff> Drafted(BindingGroup bound, ValueSet incoming, HashSet<EntryKey> locks) =>
        from before in bound.Capture
        from edits in Edits
        let shown = edits.Over(before, Unlocked)
        let target = incoming.Over(shown, locks)
        let keys = bound.Keys()
        from recalled in Products.Each(bound.Bindings
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
