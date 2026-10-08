using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects.SnapShots;
using Rhino.PlugIns;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SetName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed && !trimmed.Any(char.IsControl)
            ? (trimmed, default(InvalidRhinoValue))
            : (value, new InvalidRhinoValue());
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SetMembership {
    public static readonly SetMembership Member = new("member", syncs: true, renders: true);
    public static readonly SetMembership Locked = new("locked", syncs: false, renders: true);
    public static readonly SetMembership Excluded = new("excluded", syncs: false, renders: false);

    public bool Syncs { get; }

    public bool Renders { get; }

    public string Caption => RowText.Localize(Map(member: "Member", locked: "Locked", excluded: "Excluded")).Local;

    public string Help =>
        RowText.Localize(Map(
            member: "Takes syncs and writes across sets, and renders",
            locked: "Keeps its values through syncs and writes across sets, and renders",
            excluded: "Keeps its values through syncs and writes across sets, and stays out of the render queue")).Local;
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SetCategory {
    public static readonly SetCategory Source = new("source", Seq<CyclesKey>(), static _ => Seq(RenderSourceState.Owner));
    public static readonly SetCategory Resolution = new("resolution", Seq<CyclesKey>(), static _ => Seq(ImageOutputState.Owner, FrameState.Owner));
    public static readonly SetCategory Sampling = new(
        "sampling",
        Seq(CyclesKey.UseDocumentSamples, CyclesKey.Samples, CyclesKey.UseAdaptiveSampling, CyclesKey.AdaptiveThreshold, CyclesKey.AdaptiveMinSamples,
            CyclesKey.Seed, CyclesKey.TextureBakeQuality),
        static _ => Seq<string>());
    public static readonly SetCategory LightPaths = new(
        "light-paths",
        Seq(CyclesKey.RenderPreset, CyclesKey.MaxBounce, CyclesKey.MaxDiffuseBounce, CyclesKey.MaxGlossyBounce, CyclesKey.MaxTransmissionBounce,
            CyclesKey.MaxVolumeBounce, CyclesKey.TransparentMaxBounce, CyclesKey.AoBounces, CyclesKey.SampleClampDirect, CyclesKey.SampleClampIndirect,
            CyclesKey.FilterGlossy, CyclesKey.CausticsReflective, CyclesKey.CausticsRefractive, CyclesKey.UseDirectLight, CyclesKey.UseIndirectLight),
        static _ => Seq<string>());
    public static readonly SetCategory Channels = new("channels", Seq<CyclesKey>(), static _ => Seq(RenderChannelsState.Owner));
    public static readonly SetCategory Environment = new("environment", Seq<CyclesKey>(), static _ => Seq(BackgroundState.Owner, LightingState.Owner, SunState.Owner));
    public static readonly SetCategory Color = new(
        "color", Seq<CyclesKey>(), static effects => Seq(LinearWorkflowState.Owner, DitheringState.Owner) + effects.Bindings.Map(static binding => binding.Owner));

    public Seq<CyclesKey> CyclesKeys { get; }

    public string Caption =>
        RowText.Localize(Map(
            source: "View", resolution: "Resolution", sampling: "Sampling", lightPaths: "Light Paths", channels: "Render Channels", environment: "Environment", color: "Color")).Local;

    [UseDelegateFromConstructor]
    public partial Seq<string> Owners(BindingGroup effects);

    public LanguageExt.HashSet<EntryKey> Keys(BindingGroup bindings, BindingGroup effects) =>
        toHashSet(Owners(effects)) switch {
            var owners => toHashSet(
                bindings.Keys().Filter(key => owners.Contains(key.Owner)) + CyclesKeys.Map(static item => new EntryKey(CyclesKey.Owner, Seq(item.Key)))),
        };
}

[Union]
public abstract partial record SetScope {
    public sealed record Every : SetScope;

    public sealed record Alone(SetName Name) : SetScope;
}

public sealed record RenderSet(SetMembership Membership, LanguageExt.HashSet<EntryKey> Locks, Option<ValueSet> Stored);

public sealed record SetBook {
    // --- [STATE]
    public const uint TypeCode = 0x5253_4554u;

    private SetBook(Map<SetName, RenderSet> sets) => Sets = sets;

    public static SetBook Empty { get; } = new(Map<SetName, RenderSet>());

    public static DictionaryCodec<SetBook> Codec { get; } = new("SetBook", 1, Encoded, Decoded);

    public Map<SetName, RenderSet> Sets { get; }

    public Option<SetName> Active => toSeq(Sets.Filter(static set => set.Stored.IsNone).Keys).Head;

    public Seq<SetName> Renderable => toSeq(Sets.Filter(static set => set.Membership.Renders).Keys);

    // --- [TRANSITIONS]
    public Fin<SetBook> Added(SetName name, ValueSet live) =>
        Taken.Unless(!Sets.ContainsKey(name), nameof(Added), name)
            .Map(_ => new SetBook(Sets.Add(name, new RenderSet(SetMembership.Member, [], Active.Map(_ => live)))));

    public Fin<SetBook> Removed(SetName name) =>
        Find(name).Map(_ => new SetBook(Sets.Remove(name)));

    public Fin<SetBook> Renamed(SetName name, SetName to) =>
        (Find(name), Taken.Unless(to == name || !Sets.ContainsKey(to), nameof(Renamed), to))
            .Apply((set, _) => new SetBook(Sets.Remove(name).Add(to, set)))
            .As();

    public Fin<SetBook> Ruled(SetName name, Option<SetMembership> membership, Option<(LanguageExt.HashSet<EntryKey> Keys, bool Locked)> locks) =>
        Find(name).Map(set => new SetBook(Sets.SetItem(name, set with {
            Membership = membership.IfNone(set.Membership),
            Locks = locks.Match(Some: edit => edit.Locked ? set.Locks.Union(edit.Keys) : set.Locks.Except(edit.Keys), None: () => set.Locks),
        })));

    public Fin<(SetBook Book, Option<ValueSet> Live)> Activated(SetName name, ValueSet live) =>
        Find(name).Map(set => set.Stored.Match(
            Some: held => (new SetBook(Active.Fold(Sets, (sets, active) => sets.SetItem(active, current => current with { Stored = live })).SetItem(name, set with { Stored = None })), Some(held)),
            None: () => (this, Option<ValueSet>.None)));

    public Fin<(SetBook Book, Option<ValueSet> Live)> Spread(SetScope scope, ValueSet incoming) =>
        scope.Switch((Book: this, Incoming: incoming),
            every: static (state, _) => Fin.Succ(state.Book.Merged(state.Book.Sets.Filter(static set => set.Membership.Syncs), state.Incoming, static set => set.Locks)),
            alone: static (state, alone) => state.Book.Find(alone.Name).Map(set => state.Book.Merged(Map((alone.Name, set)), state.Incoming, static _ => [])));

    private (SetBook Book, Option<ValueSet> Live) Merged(Map<SetName, RenderSet> targets, ValueSet incoming, Func<RenderSet, LanguageExt.HashSet<EntryKey>> locks) =>
        (new SetBook(toSeq(targets.AsIterable()).Fold(Sets, (sets, target) => sets.SetItem(target.Key, target.Value with {
            Stored = target.Value.Stored.Map(held => new ValueSet(held.Entries.AddOrUpdateRange(incoming.Entries.Except(locks(target.Value)).AsIterable()))),
        }))),
            toSeq(targets.Values).Find(static set => set.Stored.IsNone).Map(set => new ValueSet(incoming.Entries.Except(locks(set)))));

    // --- [READS]
    public Fin<RenderSet> Find(SetName name) => Sets.Find(name).ToFin(new UnknownRenderSet(name));

    public Map<SetName, ValueSet> Valued(ValueSet live) => Sets.Map(set => set.Stored.IfNone(live));

    public LanguageExt.HashSet<EntryKey> Marks(ValueSet live) =>
        toSeq(Sets.Filter(static set => set.Membership.Syncs).Values).Map(set => set.Stored.IfNone(live)) switch {
            var members => members.Head.Match(
                Some: first => members.Tail.Fold(LanguageExt.HashSet<EntryKey>.Empty, (marks, other) => marks.Union(first.Diff(other).Changes.Keys)),
                None: static () => []),
        };

    public Fin<Map<SetName, ValueDiff>> Previewed(SetScope scope, ValueSet incoming, ValueSet live) =>
        Spread(scope, incoming).Map(next => Valued(live).Intersect(
            next.Book.Valued(next.Live.Map(written => written.Over(live, [])).IfNone(live)),
            static (_, before, after) => before.Diff(after)));

    // --- [ARCHIVE]
    private const string SetsKey = "Sets";
    private const string LocksKey = "Locks";
    private const string ValuesKey = "Values";
    private const string MembershipKey = "Membership";

    private static Fin<ArchivableDictionary> Encoded(SetBook book) =>
        ArchivableDictionaries.Nested(Seq((SetsKey, ArchivableDictionaries.Nested(toSeq(book.Sets.AsIterable()).Map(static row => ((string)row.Key, Encoded(row.Value)))))));

    private static Fin<ArchivableDictionary> Encoded(RenderSet set) =>
        from target in ArchivableDictionaries.Nested(
            Seq((Key: LocksKey, Value: LockSets.Keys.ToDictionary(set.Locks))) + set.Stored.Map(static held => (Key: ValuesKey, Value: ValueSet.Codec.ToDictionary(held))).ToSeq())
        from membership in ArchivableDictionaries.Set(target, MembershipKey, set.Membership.Key)
        select target;

    private static Fin<SetBook> Decoded(ArchivableDictionary source) =>
        ArchivableDictionaries.Required<ArchivableDictionary>(source, SetsKey)
            .Bind(ArchivableDictionaries.Children)
            .Bind(static rows => Callbacks.Each(rows, static (row, _) =>
                (Conversions.Validated<SetName, string, InvalidRhinoValue>(row.Key).ToValidation(), DecodedSet(row.Value).ToValidation())
                    .Apply(static (name, set) => (name, set))
                    .As()
                    .ToFin()))
            .Map(static rows => new SetBook(toMap(rows)));

    private static Fin<RenderSet> DecodedSet(ArchivableDictionary source) =>
        (ArchivableDictionaries.Required<string>(source, MembershipKey).Bind(Conversions.Validated<SetMembership, string, InvalidRhinoValue>).ToValidation(),
         ArchivableDictionaries.Required<ArchivableDictionary>(source, LocksKey).Bind(LockSets.Keys.FromDictionary).ToValidation(),
         ArchivableDictionaries.Find<ArchivableDictionary>(source, ValuesKey).Bind(static found => found.Traverse(ValueSet.Codec.FromDictionary).As()).ToValidation())
            .Apply(static (membership, locks, stored) => new RenderSet(membership, locks, stored))
            .As()
            .ToFin();
}

public sealed record SetRow(SetName Key, SetName Name, SetMembership Membership, bool Active) {
    public static Lens<SetRow, SetName> Named { get; } = Lens<SetRow, SetName>.New(static row => row.Name, static name => row => row with { Name = name });

    public static Lens<SetRow, bool> Activated { get; } = Lens<SetRow, bool>.New(static row => row.Active, static active => row => row with { Active = active });
}

public readonly record struct BoardKey(SetCategory Category, Option<EntryKey> Entry, Option<SetName> Set);

[Union]
public abstract partial record BoardRow {
    private BoardRow(BoardKey id) => Id = id;

    public BoardKey Id { get; }

    public static Lens<BoardRow, Seq<BoardRow>> Children { get; } = Lens<BoardRow, Seq<BoardRow>>.New(
        static row => row.Switch(group: static group => group.Settings, setting: static setting => setting.Sets, value: static _ => Seq<BoardRow>()),
        static rows => row => row.Switch<Seq<BoardRow>, BoardRow>(rows,
            group: static (held, group) => group with { Settings = held },
            setting: static (held, setting) => setting with { Sets = held },
            value: static (_, value) => value));

    public static Seq<BoardRow> Board(SetBook book, BindingGroup bindings, BindingGroup effects, ValueSet live, Func<EntryKey, Option<string>> captioned) =>
        (Marks: book.Marks(live), Valued: book.Valued(live)) switch {
            var read => toSeq(SetCategory.Items).Map<BoardRow>(category => new Group(category, toSeq(category.Keys(bindings, effects).Order())
                .Choose(entry => captioned(entry).Map(caption => Setting.Of(category, entry, read.Marks.Contains(entry) ? RowText.Localize("{0} (varies)", arguments: [caption]).Local : caption, book, read.Valued))))),
        };

    public sealed record Group(SetCategory Category, Seq<BoardRow> Settings) : BoardRow(new BoardKey(Category, None, None));

    public sealed record Setting(SetCategory Category, EntryKey Entry, string Caption, Seq<BoardRow> Sets) : BoardRow(new BoardKey(Category, Entry, None)) {
        public static BoardRow Of(SetCategory category, EntryKey entry, string caption, SetBook book, Map<SetName, ValueSet> valued) =>
            new Setting(category, entry, caption, toSeq(valued.AsIterable()).Map<BoardRow>(set => new Value(
                category, entry, set.Key, set.Value.Entries.Find(entry), book.Sets.Find(set.Key).Exists(held => held.Locks.Contains(entry)))));
    }

    public sealed record Value(SetCategory Category, EntryKey Entry, SetName Set, Option<string> Text, bool Locked) : BoardRow(new BoardKey(Category, Entry, Set));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record RenderSets(RenderHistory History) {
    // --- [ROSTER]
    public static ViewStore Store { get; } = new ViewStore.DocumentStores(new RedrawPolicy.Silent()) {
        Locks = Some(LockSets.Store),
        Bindings = Some<Func<IPlugInViews, RhinoDoc, Fin<BindingGroup>>>(static (owner, doc) =>
            IPlugInRendering.Holding(((IPlugInRendering)owner).History, nameof(IPlugInRendering.History)).Bind(history => history.Bindings(doc).ToFin())),
    };

    // --- [READS]
    public static IO<Atom<SetBook>> Held(RhinoDoc doc) =>
        IO.lift(() => doc.RuntimeData.GetValue(typeof(SetBook), static _ => Atom(SetBook.Empty)));

    public static IO<SetBook> Book(RhinoDoc doc) => Held(doc).Bind(static held => held.ValueIO);

    public static IO<Seq<SetName>> Renderable(RhinoDoc doc) => Book(doc).Map(static book => book.Renderable);

    public IO<ValueSet> Values(RhinoDoc doc, SetName name) =>
        from book in Book(doc)
        from set in IO.lift(book.Find(name))
        from values in set.Stored.Match(Some: static held => IO.pure(held), None: () => IO.lift(() => History.Settings(doc)).Bind(static binding => binding.Group.Capture))
        select values;

    private IO<ValueSet> Category(RhinoDoc doc, SetName source, SetCategory category) =>
        from values in Values(doc, source)
        from binding in IO.lift(() => History.Settings(doc))
        from grading in IO.lift(() => History.Grading(doc).ToFin())
        select new ValueSet(values.Entries.Intersect(category.Keys(binding.Group, grading)));

    private IO<bool> Changes(RhinoDoc doc, SetScope scope, ValueSet incoming) =>
        from book in Book(doc)
        from binding in IO.lift(() => History.Settings(doc))
        from live in binding.Group.Capture
        from diffs in IO.lift(book.Previewed(scope, incoming, live))
        select diffs.Exists(static (_, diff) => !diff.Changes.IsEmpty);

    // --- [WRITES]
    public IO<SetBook> Add(RhinoDoc doc, SetName name) => Booked(doc, (book, live) => book.Added(name, live));

    public IO<SetBook> Remove(RhinoDoc doc, SetName name) => Booked(doc, (book, _) => book.Removed(name));

    public IO<SetBook> Rename(RhinoDoc doc, SetName name, SetName to) => Booked(doc, (book, _) => book.Renamed(name, to));

    public IO<SetBook> Rule(RhinoDoc doc, SetName name, Option<SetMembership> membership, Option<(LanguageExt.HashSet<EntryKey> Keys, bool Locked)> locks) =>
        Booked(doc, (book, _) => book.Ruled(name, membership, locks));

    public IO<SetBook> Activate(RhinoDoc doc, SetName name) => Committed(doc, (book, live) => book.Activated(name, live));

    public IO<SetBook> Write(RhinoDoc doc, SetScope scope, ValueSet incoming) => Committed(doc, (book, _) => book.Spread(scope, incoming));

    private IO<SetBook> Booked(RhinoDoc doc, Func<SetBook, ValueSet, Fin<SetBook>> step) =>
        Committed(doc, (book, live) => step(book, live).Map(static next => (next, Option<ValueSet>.None)));

    private IO<SetBook> Committed(RhinoDoc doc, Func<SetBook, ValueSet, Fin<(SetBook Book, Option<ValueSet> Live)>> step) =>
        IO.lift(() => History.Settings(doc)).Bind(binding => binding.Scope.Switch(
            (Doc: doc, Binding: binding, Step: step),
            applicationScope: static (state, _) => Stepped(state.Doc, state.Binding, state.Step, static (_, _, _) => IO.pure(unit)),
            documentScope: static (state, scope) => Commits.Commit(scope.Doc, state.Binding.Caption, scope.Redraw,
                    Stepped(scope.Doc, state.Binding, state.Step, (held, before, after) => Reversed(scope, state.Binding.Caption, held, before, after)))
                .Map(static committed => committed.Value)));

    private static IO<SetBook> Stepped(
        RhinoDoc doc, HistoryBinding binding, Func<SetBook, ValueSet, Fin<(SetBook Book, Option<ValueSet> Live)>> step, Func<Atom<SetBook>, SetBook, SetBook, IO<Unit>> reversed) =>
        from held in Held(doc)
        from before in held.ValueIO
        from live in binding.Group.Capture
        from next in IO.lift(step(before, live))
        from written in next.Live.Traverse(values => binding.Commit(values, [])).As()
        from swapped in held.SwapIO(_ => next.Book)
        from registered in reversed(held, before, next.Book)
        select next.Book;

    private static IO<Unit> Reversed(HistoryScope.DocumentScope scope, string caption, Atom<SetBook> held, SetBook before, SetBook after) =>
        from recording in IO.lift(() => scope.Doc.UndoRecordingIsActive)
        from registered in when(recording && before != after, TableOps.Register(scope.Doc, new CustomUndo(caption,
            held.SwapIO(_ => before).Map(static _ => unit),
            held.SwapIO(_ => after).Map(static _ => unit),
            scope.Site))).As()
        select unit;

    // --- [SNAPSHOTS]
    public SnapshotState<RhinoDoc> Snapshot =>
        new SnapshotState<RhinoDoc, SetBook>(SetBook.TypeCode, SetBook.Codec, Book, (doc, book) => Booked(doc, (_, _) => book).Map(static _ => unit));

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(RenderHistory history, Func<RenderSets, DefinedSnapshotClient> client) =>
        (plugIn, _) =>
            from sets in IO.pure(new RenderSets(history))
            from registered in IO.lift(() => ignore(SnapShotsClient.RegisterSnapShotClient(client(sets))))
            let cell = ((IPlugInRendering)plugIn).Sets
            from filled in cell.SwapIO(_ => Some(sets))
            select (IDisposable)new Disposal<Atom<Option<RenderSets>>>(cell, static held => ignore(held.Swap(static _ => None)));

    // --- [ROWS]
    private const string Owner = "render-sets";

    private static readonly (RowSource<Seq<SetRow>> Source, RowField<Seq<SetRow>> Field) Listed =
        RowSource.Opaque(Owner, "sets", Seq<SetRow>(), static scope => ValueStore.Of(
            Served(scope).Bind(static held => Book(held.Doc)).Map(static book => Some(toSeq(book.Sets.Map(static (name, set) => new SetRow(name, name, set.Membership, set.Stored.IsNone)).Values))),
            items => Served(scope).Bind(held => held.Sets.Edited(held.Doc, items.IfNone(Seq<SetRow>()))),
            Applied.Live,
            None));

    private static readonly (RowSource<LanguageExt.HashSet<SetName>> Source, RowField<LanguageExt.HashSet<SetName>> Field) Picked =
        RowSource.Opaque(Owner, "picked", LanguageExt.HashSet<SetName>.Empty);

    private static readonly (RowSource<Seq<BoardRow>> Source, RowField<Seq<BoardRow>> Field) Boarded =
        RowSource.Opaque(Owner, "board", Seq<BoardRow>(), static scope => ValueStore.Of(
            Served(scope).Bind(held => held.Sets.Board(held.Doc, scope.Sink)).Map(static rows => Some(rows)),
            static _ => IO.pure(unit),
            Applied.Live,
            None));

    private static readonly (RowSource<LanguageExt.HashSet<BoardKey>> Source, RowField<LanguageExt.HashSet<BoardKey>> Field) Chosen =
        RowSource.Opaque(Owner, "chosen", LanguageExt.HashSet<BoardKey>.Empty);

    private static readonly (RowSource<LanguageExt.HashSet<BoardKey>> Source, RowField<LanguageExt.HashSet<BoardKey>> Field) Expanded =
        RowSource.Opaque(Owner, "expanded", LanguageExt.HashSet<BoardKey>.Empty);

    public static Seq<Child> Rows() =>
        Seq<Child>(
            ListRows.List(RowText.Localize("Render Sets").Local, RowText.Localize("Render sets of this document, the checked set holding the live settings").Local, new ListRow<SetRow, SetName>(
                    IterableNE.create(
                        ListColumn.Named<SetRow, SetName, string, InvalidRhinoValue>(None, SetRow.Named, None),
                        new ListColumn<SetRow>.Label(None, static row => row.Membership.Caption, None, None),
                        new ListColumn<SetRow>.Check(None, SetRow.Activated)),
                    static row => row.Key,
                    Listed,
                    Picked) {
                Search = Some(new ListSearch<SetRow>(RowText.Localize("Search render sets").Local, static (_, row) => row.Name, None)),
                Wording = Wording.Localized,
                Pair = Some((
                    new CommandRow.Run(Face("render-set-add", "Add Render Set", "Store the live settings as a new render set"), None, static scope =>
                        from held in Served(scope)
                        from name in HostDialogs.ShowEditBox<SetName, InvalidRhinoValue>(RowText.Localize("Add Render Set"), RowText.Localize("Set name"), None)
                        from added in held.Sets.Add(held.Doc, name)
                        select unit),
                    new CommandRow.Run(Face("render-set-remove", "Remove Render Sets", "Remove the selected render sets"), None, static scope =>
                        from held in Served(scope)
                        from picked in scope.Read(Picked.Source)
                        from removed in toSeq(picked).TraverseM(name => held.Sets.Remove(held.Doc, name)).As()
                        select unit))),
                Commands = SetCommands,
            },
                RowRules.Always),
            ListRows.List(RowText.Localize("Settings by Set").Local, RowText.Localize("Each category's settings with the value every set holds").Local, new ListRow<BoardRow, BoardKey>(
                    IterableNE.create<ListColumn<BoardRow>>(
                        new ListColumn<BoardRow>.Label(None, static row => row.Switch(
                            group: static group => group.Category.Caption,
                            setting: static setting => setting.Caption,
                            value: static value => value.Set), None, None),
                        new ListColumn<BoardRow>.Label(None, static row => row.Switch(
                            group: static _ => "",
                            setting: static _ => "",
                            value: static value => value.Text.IfNone("")), None, None)),
                    static row => row.Id,
                    Boarded,
                    Chosen) {
                Branches = Some(new ListBranches<BoardRow, BoardKey>(BoardRow.Children, Expanded)),
                AllowMultipleSelection = true,
                Wording = Wording.Localized,
                Commands = BoardCommands,
            },
                RowRules.Always));

    private static Seq<CommandRow> SetCommands(IO<Seq<SetRow>> selection) =>
        Seq<CommandRow>(new CommandRow.Radio(
                Face("render-set-membership", "Membership", "Which writes the selected sets take and whether they render"), None,
                toSeq(SetMembership.Items).Map(membership => new CommandRow.Check(
                    new CommandFace($"render-set-{membership.Key}", membership.Caption, membership.Help, None, None) { Wording = Wording.Localized }, None,
                    _ => from rows in selection select !rows.IsEmpty && rows.ForAll(row => row.Membership == membership),
                    (scope, enabled) => when(enabled,
                        from held in Served(scope)
                        from rows in selection
                        from ruled in rows.TraverseM(row => held.Sets.Rule(held.Doc, row.Key, Some(membership), None)).As()
                        select unit).As()))))
            + toSeq(SetCategory.Items).Map<CommandRow>(category => Writer(
                new CommandFace($"render-set-sync-{category.Key}", RowText.Localize("Sync {0}", arguments: [category.Caption]).Local,
                    RowText.Localize("Copy the selected set's {0} settings into every synced set", arguments: [category.Caption]).Local, None, None) { Wording = Wording.Localized },
                Seq<RowSource>(Picked.Source), selection,
                (_, held, rows) => rows.Head.Traverse(row => held.Sets.Category(held.Doc, row.Key, category).Map(Everywhere)).As()))
                .Add(Writer(Face("render-set-apply-all", "Apply to All Sets", "Write the selected set's settings into every synced set"), Seq<RowSource>(Picked.Source), selection,
                    static (_, held, rows) => rows.Head.Traverse(row => held.Sets.Values(held.Doc, row.Key).Map(Everywhere)).As()));

    private static Seq<CommandRow> BoardCommands(IO<Seq<BoardRow>> selection) =>
        Seq<CommandRow>(
            Writer(Face("render-set-board-apply-all", "Apply to All Sets", "Write each selected set value into every synced set"), Seq<RowSource>(Chosen.Source), selection,
                static (_, _, rows) => IO.pure(Incoming(rows).Map(Everywhere))),
            Writer(Face("render-set-board-apply-set", "Apply to Selected Set", "Write each selected set value into the set selected in the set list"), Seq<RowSource>(Chosen.Source, Picked.Source), selection,
                static (scope, _, rows) =>
                    from picked in scope.Read(Picked.Source)
                    select from incoming in Incoming(rows)
                           from target in toSeq(picked).Head
                           select ((SetScope)new SetScope.Alone(target), incoming)),
            new CommandRow.Check(Face("render-set-board-lock-set", "Lock in Set", "Keep the selected set values out of every write across sets"), None,
                _ => from rows in selection select ValueRows(rows) switch { var values => !values.IsEmpty && values.ForAll(static value => value.Locked) },
                (scope, enabled) =>
                    from held in Served(scope)
                    from rows in selection
                    from ruled in toSeq(ValueRows(rows).GroupBy(static value => value.Set))
                        .TraverseM(set => held.Sets.Rule(held.Doc, set.Key, None, Some((toHashSet(set.Select(static value => value.Entry)), enabled)))).As()
                    select unit),
            new CommandRow.Check(Face("render-set-board-lock-section", "Lock for Presets", "Keep the selected settings out of every preset apply, paste, and reset"), None,
                scope =>
                    from rows in selection
                    from locks in scope.Locked
                    select SettingKeys(rows) switch { var keys => !keys.IsEmpty && keys.ForAll(locks.Contains) },
                (scope, enabled) =>
                    from rows in selection
                    let keys = SettingKeys(rows)
                    from locked in when(!keys.IsEmpty, scope.Lock(keys, enabled)).As()
                    select unit));

    private IO<Unit> Edited(RhinoDoc doc, Seq<SetRow> items) =>
        from book in Book(doc)
        from activated in items.Find(row => row.Active && book.Active != Some(row.Key)).Traverse(row => Activate(doc, row.Key)).As()
        from renamed in items.Filter(static row => !string.Equals(row.Name, row.Key, StringComparison.Ordinal)).TraverseM(row => Rename(doc, row.Key, row.Name)).As()
        select unit;

    private IO<Seq<BoardRow>> Board(RhinoDoc doc, IPlugInSink sink) =>
        from book in Book(doc)
        from binding in IO.lift(() => History.Settings(doc))
        from grading in IO.lift(() => History.Grading(doc).ToFin())
        from live in binding.Group.Capture
        select BoardRow.Board(book, binding.Group, grading, live, key => binding.Group.Text(key).Map(text => RowText.Localize(text.Caption, table: Some<object>(sink)).Local));

    private static CommandRow.Run Writer<TItem>(
        CommandFace face, Seq<RowSource> reads, IO<Seq<TItem>> selection, Func<RowScope, (RhinoDoc Doc, RenderSets Sets), Seq<TItem>, IO<Option<(SetScope Scope, ValueSet Incoming)>>> plan)
        where TItem : notnull =>
        new(face,
            Some(new RowRule(Seq<EntryKey>(), reads, scope =>
                from held in Served(scope)
                from rows in selection
                from planned in plan(scope, held, rows)
                from changes in planned.Match(Some: write => held.Sets.Changes(held.Doc, write.Scope, write.Incoming), None: static () => IO.pure(value: false))
                select changes)),
            scope =>
                from held in Served(scope)
                from rows in selection
                from planned in plan(scope, held, rows)
                from written in planned.Traverse(write => held.Sets.Write(held.Doc, write.Scope, write.Incoming)).As()
                select unit);

    private static (SetScope Scope, ValueSet Incoming) Everywhere(ValueSet incoming) => (new SetScope.Every(), incoming);

    private static CommandFace Face(string name, string caption, string help) =>
        new(name, RowText.Localize(caption).Local, RowText.Localize(help).Local, None, None) { Wording = Wording.Localized };

    private static Seq<BoardRow.Value> ValueRows(Seq<BoardRow> rows) =>
        rows.Choose(static row => row.Switch<Option<BoardRow.Value>>(group: static _ => None, setting: static _ => None, value: static value => value));

    private static Seq<EntryKey> SettingKeys(Seq<BoardRow> rows) =>
        rows.Choose(static row => row.Switch<Option<EntryKey>>(group: static _ => None, setting: static setting => setting.Entry, value: static _ => None));

    private static Option<ValueSet> Incoming(Seq<BoardRow> rows) =>
        Callbacks.Unique(ValueRows(rows), static value => value.Entry, nameof(Incoming))
            .ToOption()
            .Map(static values => new ValueSet(toHashMap(values.Choose(static value => value.Text.Map(text => (value.Entry, text))))));

    private static IO<(RhinoDoc Doc, RenderSets Sets)> Served(RowScope scope) =>
        from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
        from sets in IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Sets, nameof(IPlugInRendering.Sets))
        select (doc, sets);
}
