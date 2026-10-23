using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Collections;

namespace Rasm.Rhino.Render.Presets;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SetName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed && !trimmed.Any(char.IsControl)
            ? (trimmed, default(ValidationError))
            : (value, new ValidationError("set name is blank or holds a control character"));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SetMembership {
    public static readonly SetMembership Member = new("member", label: "Member", hint: "Renders, takes syncs and writes across sets", syncs: true, renders: true);
    public static readonly SetMembership Locked = new("locked", label: "Locked", hint: "Renders, keeps values through syncs and writes across sets", syncs: false, renders: true);
    public static readonly SetMembership Excluded = new("excluded", label: "Excluded", hint: "Skips render queue, keeps values through syncs and writes across sets", syncs: false, renders: false);

    private string Label { get; }

    private string Hint { get; }

    public bool Syncs { get; }

    public bool Renders { get; }

    public string Caption => RowText.Localize(Label).Local;

    public string Help => RowText.Localize(Hint).Local;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SetCategory {
    public static readonly SetCategory Source = new("source", [], static _ => [RenderSourceState.Owner]);
    public static readonly SetCategory Resolution = new("resolution", [], static _ => [ImageOutputState.Owner, FrameState.Owner]);
    public static readonly SetCategory Sampling = new(
        "sampling",
        [CyclesKey.UseDocumentSamples, CyclesKey.Samples, CyclesKey.UseAdaptiveSampling, CyclesKey.AdaptiveThreshold, CyclesKey.AdaptiveMinSamples, CyclesKey.Seed, CyclesKey.TextureBakeQuality],
        static _ => []);
    public static readonly SetCategory LightPaths = new(
        "light-paths",
        [
            CyclesKey.RenderPreset, CyclesKey.MaxBounce, CyclesKey.MaxDiffuseBounce, CyclesKey.MaxGlossyBounce, CyclesKey.MaxTransmissionBounce,
            CyclesKey.MaxVolumeBounce, CyclesKey.TransparentMaxBounce, CyclesKey.AoBounces, CyclesKey.SampleClampDirect, CyclesKey.SampleClampIndirect,
            CyclesKey.FilterGlossy, CyclesKey.CausticsReflective, CyclesKey.CausticsRefractive, CyclesKey.UseDirectLight, CyclesKey.UseIndirectLight,
        ],
        static _ => []);
    public static readonly SetCategory Channels = new("channels", [], static _ => [RenderChannelsState.Owner]);
    public static readonly SetCategory Environment = new("environment", [], static _ => [BackgroundState.Owner, LightingState.Owner, SunState.Owner]);
    public static readonly SetCategory Color = new(
        "color", [], static effects => [LinearWorkflowState.Owner, DitheringState.Owner, .. effects.Bindings.Map(static binding => binding.Owner)]);

    public Seq<CyclesKey> CyclesKeys { get; }

    public string Caption =>
        RowText.Localize(Map(
            source: "View", resolution: "Resolution", sampling: "Sampling", lightPaths: "Light Paths", channels: "Render Channels", environment: "Environment", color: "Color")).Local;

    [UseDelegateFromConstructor]
    public partial Seq<string> Owners(BindingGroup effects);

    public LanguageExt.HashSet<EntryKey> Keys(BindingGroup bindings, BindingGroup effects) =>
        toHashSet(Owners(effects)) switch {
            var owners => [.. bindings.Keys().Filter(key => owners.Contains(key.Owner)), .. CyclesKeys.Map(static item => new EntryKey(CyclesKey.Owner, [item.Key]))],
        };
}

public sealed record RenderSet(SetMembership Membership, LanguageExt.HashSet<EntryKey> Locks, Option<ValueSet> Stored);

[ComplexValueObject]
public sealed partial class SetBook {
    // --- [STATE]
    public const uint TypeCode = 0x5253_4554u;

    public static SetBook Empty { get; } = new(Map<SetName, RenderSet>());

    public static DictionaryCodec<SetBook> Codec { get; } = DictionaryCodec.Json<SetBook>(nameof(SetBook), 2, new RecordUpgrade<SetBook>(ArchiveV1));

    public Map<SetName, RenderSet> Sets { get; }

    public Option<SetName> Active => toSeq(Sets.Filter(static set => set.Stored.IsNone).Keys).Head;

    public Seq<SetName> Renderable => toSeq(Sets.Filter(static set => set.Membership.Renders).Keys);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Map<SetName, RenderSet> sets) =>
        validationError = sets.Filter(static set => set.Stored.IsNone).Count <= 1 ? null : new ValidationError("set book holds more than one active set");

    // --- [TRANSITIONS]
    public Fin<SetBook> Added(SetName name, ValueSet live) =>
        Sets.ContainsKey(name)
            ? Error.New($"{nameof(Added)} already holds {name}")
            : new SetBook(Sets.Add(name, new RenderSet(SetMembership.Member, [], Active.Map(_ => live))));

    public Fin<SetBook> Removed(SetName name) =>
        Find(name).Map(_ => new SetBook(Sets.Remove(name)));

    public Fin<SetBook> Renamed(SetName name, SetName to) =>
        (Find(name), to == name || !Sets.ContainsKey(to) ? Fin.Succ(unit) : Error.New($"{nameof(Renamed)} already holds {to}"))
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

    public Fin<(SetBook Book, Option<ValueSet> Live)> Spread(Option<SetName> alone, ValueSet incoming) =>
        alone.Match(
            Some: name => Find(name).Map(set => Merged(Map((name, set)), incoming, static _ => [])),
            None: () => Merged(Sets.Filter(static set => set.Membership.Syncs), incoming, static set => set.Locks));

    private (SetBook Book, Option<ValueSet> Live) Merged(Map<SetName, RenderSet> targets, ValueSet incoming, Func<RenderSet, LanguageExt.HashSet<EntryKey>> locks) =>
        (new SetBook(toSeq(targets.AsIterable()).Fold(Sets, (sets, target) => sets.SetItem(target.Key, target.Value with {
            Stored = target.Value.Stored.Map(held => new ValueSet(held.Entries.AddOrUpdateRange(incoming.Entries.Except(locks(target.Value)).AsIterable()))),
        }))),
            toSeq(targets.Values).Find(static set => set.Stored.IsNone).Map(set => new ValueSet(incoming.Entries.Except(locks(set)))));

    // --- [READS]
    public Fin<RenderSet> Find(SetName name) => Sets.Find(name).ToFin(new NotFound("render set", name));

    public Map<SetName, ValueSet> Valued(ValueSet live) => Sets.Map(set => set.Stored.IfNone(live));

    public LanguageExt.HashSet<EntryKey> Marks(ValueSet live) =>
        toSeq(Sets.Filter(static set => set.Membership.Syncs).Values).Map(set => set.Stored.IfNone(live)) switch {
            var members => members.Head.Match(
                Some: first => members.Tail.Fold(LanguageExt.HashSet<EntryKey>.Empty, (marks, other) => marks.Union(first.Diff(other).Changes.Keys)),
                None: static () => []),
        };

    public Fin<Map<SetName, ValueDiff>> Previewed(Option<SetName> alone, ValueSet incoming, ValueSet live) =>
        Spread(alone, incoming).Map(next => Valued(live).Intersect(
            next.Book.Valued(next.Live.Map(written => written.Over(live, [])).IfNone(live)),
            static (_, before, after) => before.Diff(after)));

    // --- [ARCHIVE]
    private const string SetsKey = "Sets";
    private const string LocksKey = "Locks";
    private const string ValuesKey = "Values";
    private const string MembershipKey = "Membership";

    private static Fin<SetBook> ArchiveV1(ArchivableDictionary source) =>
        ArchivableDictionaries.Required<ArchivableDictionary>(source, SetsKey)
            .Bind(ArchivableDictionaries.Children)
            .Bind(static rows => Callbacks.Each(rows, static (row, _) =>
                (SetName.Validated(row.Key), ArchivedSet(row.Value))
                    .Apply(static (name, set) => (Name: name, Set: set))
                    .As()))
            .Bind(static rows => Callbacks.Unique(rows, static row => row.Name, nameof(ArchiveV1)))
            .Bind(static rows => ValidationResults.ToFin(Validate(toMap(rows), out SetBook? book), book));

    private static Fin<RenderSet> ArchivedSet(ArchivableDictionary source) =>
        (ArchivableDictionaries.Required<string>(source, MembershipKey).Bind(SetMembership.Validated),
         ArchivableDictionaries.Required<ArchivableDictionary>(source, LocksKey).Bind(ArchivedLocks),
         ArchivableDictionaries.Find<ArchivableDictionary>(source, ValuesKey).Bind(static found => found.Traverse(ValueSet.ArchiveV1).As()))
            .Apply(static (membership, locks, stored) => new RenderSet(membership, locks, stored))
            .As();

    private static Fin<LanguageExt.HashSet<EntryKey>> ArchivedLocks(ArchivableDictionary source) =>
        ArchivedPaths(source).Map(static paths => toHashSet(paths.Map(static path => new EntryKey(path[0], path.Tail))));

    private static Fin<Seq<Seq<string>>> ArchivedPaths(ArchivableDictionary source) =>
        from rows in ArchivableDictionaries.Children(source)
        from paths in Callbacks.Each(rows, static (row, _) =>
            row.Value.Count is 0 ? Seq(Seq(row.Key)) : ArchivedPaths(row.Value).Map(tails => tails.Map(tail => row.Key.Cons(tail))))
        select paths.Flatten();
}

public sealed record SetRow(SetName Key, SetName Name, SetMembership Membership, bool Active) {
    public static Seq<SetRow> Of(SetBook book) =>
        toSeq(book.Sets.Map(static (name, set) => new SetRow(name, name, set.Membership, set.Stored.IsNone)).Values);

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
public static class RenderSets {
    // --- [ROSTER]
    public static ViewStore Store { get; } = new ViewStore.DocumentStores(new RedrawPolicy.Silent()) {
        Locks = Some(LockSets.Store),
        Bindings = Some<Func<IPlugInViews, RhinoDoc, IO<BindingGroup>>>(static (owner, doc) =>
            RenderHistory.Current.Of(owner).Bind(history => history.Bindings(doc))),
    };

    // --- [READS]
    public static IO<SetBook> Book(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(doc), SetBook.Codec).Map(static book => book.IfNone(SetBook.Empty));

    public static IO<Seq<SetName>> Renderable(RhinoDoc doc) => Book(doc).Map(static book => book.Renderable);

    public static IO<ValueSet> Values(RenderHistory history, RhinoDoc doc, SetName name) =>
        from book in Book(doc)
        from set in IO.lift(book.Find(name))
        from values in set.Stored.Match(Some: static held => IO.pure(held), None: () => history.Settings(doc).Bind(static binding => binding.Group.Capture))
        select values;

    private static IO<ValueSet> Category(RenderHistory history, RhinoDoc doc, SetName source, SetCategory category) =>
        from values in Values(history, doc, source)
        from binding in history.Settings(doc)
        from grading in history.Grading(doc)
        select new ValueSet(values.Entries.Intersect(category.Keys(binding.Group, grading)));

    private static IO<bool> Changes(RenderHistory history, RhinoDoc doc, Option<SetName> alone, ValueSet incoming) =>
        from book in Book(doc)
        from binding in history.Settings(doc)
        from live in binding.Group.Capture
        from diffs in IO.lift(book.Previewed(alone, incoming, live))
        select diffs.Exists(static (_, diff) => !diff.Changes.IsEmpty);

    // --- [WRITES]
    public static IO<SetBook> Add(RenderHistory history, RhinoDoc doc, SetName name) => Booked(history, doc, (book, live) => book.Added(name, live));

    public static IO<SetBook> Remove(RenderHistory history, RhinoDoc doc, SetName name) => Booked(history, doc, (book, _) => book.Removed(name));

    public static IO<SetBook> Rename(RenderHistory history, RhinoDoc doc, SetName name, SetName to) => Booked(history, doc, (book, _) => book.Renamed(name, to));

    public static IO<SetBook> Rule(
        RenderHistory history, RhinoDoc doc, SetName name, Option<SetMembership> membership, Option<(LanguageExt.HashSet<EntryKey> Keys, bool Locked)> locks) =>
        Booked(history, doc, (book, _) => book.Ruled(name, membership, locks));

    public static IO<SetBook> Activate(RenderHistory history, RhinoDoc doc, SetName name) => Committed(history, doc, (book, live) => book.Activated(name, live));

    public static IO<SetBook> Write(RenderHistory history, RhinoDoc doc, Option<SetName> alone, ValueSet incoming) =>
        Committed(history, doc, (book, _) => book.Spread(alone, incoming));

    private static IO<SetBook> Booked(RenderHistory history, RhinoDoc doc, Func<SetBook, ValueSet, Fin<SetBook>> step) =>
        Committed(history, doc, (book, live) => step(book, live).Map(static next => (next, Option<ValueSet>.None)));

    private static IO<SetBook> Committed(RenderHistory history, RhinoDoc doc, Func<SetBook, ValueSet, Fin<(SetBook Book, Option<ValueSet> Live)>> step) =>
        history.Settings(doc).Bind(binding => binding.Scope.Switch(
            (Doc: doc, Binding: binding, Step: step),
            applicationScope: static (state, _) => Stepped(state.Doc, state.Binding, state.Step),
            documentScope: static (state, scope) => Commits.Commit(scope.Doc, state.Binding.Caption, scope.Redraw,
                    Stepped(scope.Doc, state.Binding, state.Step))
                .Map(static committed => committed.Value)));

    private static IO<SetBook> Stepped(
        RhinoDoc doc, HistoryBinding binding, Func<SetBook, ValueSet, Fin<(SetBook Book, Option<ValueSet> Live)>> step) =>
        from before in Book(doc)
        from live in binding.Group.Capture
        from next in IO.lift(step(before, live))
        from written in next.Live.Traverse(values => binding.Commit(values, [])).As()
        from writtenBook in ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(doc), SetBook.Codec, next.Book)
        select next.Book;

    // --- [SNAPSHOTS]
    public static SnapshotState<RhinoDoc> Snapshot(RenderHistory history) =>
        new SnapshotState<RhinoDoc, SetBook>(SetBook.TypeCode, SetBook.Codec, Book, (doc, book) => Booked(history, doc, (_, _) => book).Map(unit));

    // --- [ROWS]
    private const string Owner = "render-sets";

    private static readonly (RowSource<Seq<SetRow>> Source, RowField<Seq<SetRow>> Field) Listed =
        RowSource.Opaque(Owner, "sets", Seq<SetRow>(), static scope => new ValueStore<Seq<SetRow>>(
            Served(scope).Bind(static held => Book(held.Doc)).Map(static book => Some(SetRow.Of(book))),
            items => Served(scope).Bind(held => Edited(held.History, held.Doc, items.IfNone(Seq<SetRow>()))),
            EqualityComparer<Seq<SetRow>>.Default.Equals,
            Applied.Live,
            None));

    private static readonly (RowSource<LanguageExt.HashSet<SetName>> Source, RowField<LanguageExt.HashSet<SetName>> Field) Picked =
        RowSource.Opaque(Owner, "picked", LanguageExt.HashSet<SetName>.Empty);

    private static readonly (RowSource<Seq<BoardRow>> Source, RowField<Seq<BoardRow>> Field) Boarded =
        RowSource.Opaque(Owner, "board", Seq<BoardRow>(), static scope => ValueStore.Readout(Served(scope).Bind(held => Board(held.History, held.Doc, scope.Sink)), None));

    private static readonly (RowSource<LanguageExt.HashSet<BoardKey>> Source, RowField<LanguageExt.HashSet<BoardKey>> Field) Chosen =
        RowSource.Opaque(Owner, "chosen", LanguageExt.HashSet<BoardKey>.Empty);

    private static readonly (RowSource<LanguageExt.HashSet<BoardKey>> Source, RowField<LanguageExt.HashSet<BoardKey>> Field) Expanded =
        RowSource.Opaque(Owner, "expanded", LanguageExt.HashSet<BoardKey>.Empty);

    public static Seq<Child> Rows() =>
        [
            ListRows.List("Render Sets", "Document render sets, checked set holds live settings", new ListRow<SetRow, SetName>(
                    IterableNE.create(
                        ListColumn.Named<SetRow, SetName, string>(None, SetRow.Named, None),
                        new ListColumn<SetRow>.Label(None, static row => row.Membership.Caption, None, None),
                        new ListColumn<SetRow>.Check(None, SetRow.Activated)),
                    static row => row.Key,
                    Listed,
                    Picked) {
                Search = Some(new ListSearch<SetRow>("Search render sets", static (_, row) => row.Name, None)),
                Wording = Wording.Library,
                Pair = Some((
                    new CommandRow.Run(Face("render-set-add", "Add Render Set", "Store live settings as new render set"), None, static scope =>
                        from held in Served(scope)
                        from name in HostDialogs.ShowEditBox<SetName>(RowText.Localize("Add Render Set"), RowText.Localize("Set name"), None)
                        from added in Add(held.History, held.Doc, name)
                        select unit),
                    new CommandRow.Run(Face("render-set-remove", "Remove Render Sets", "Remove selected render sets"), None, static scope =>
                        from held in Served(scope)
                        from picked in scope.Read(Picked.Source)
                        from removed in Callbacks.Each(toSeq(picked).Map(name => Remove(held.History, held.Doc, name)))
                        select unit))),
                Commands = SetCommands,
            },
                RowRules.Always),
            ListRows.List("Settings by Set", "Each set's value per setting", new ListRow<BoardRow, BoardKey>(
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
                Wording = Wording.Library,
                Commands = BoardCommands,
            },
                RowRules.Always),
        ];

    private static Seq<CommandRow> SetCommands(IO<Seq<SetRow>> selection) =>
        [
            new CommandRow.Radio(
                Face("render-set-membership", "Membership", "Sync and render rules of selected sets"), None,
                toSeq(SetMembership.Items).Map(membership => new CommandRow.Check(
                    new CommandFace($"render-set-{membership.Key}", membership.Caption, membership.Help, None, None) { Wording = Wording.Localized }, None,
                    _ => from rows in selection select !rows.IsEmpty && rows.ForAll(row => row.Membership == membership),
                    (scope, enabled) => when(enabled,
                        from held in Served(scope)
                        from rows in selection
                        from ruled in Callbacks.Each(rows.Map(row => Rule(held.History, held.Doc, row.Key, Some(membership), None)))
                        select unit).As()))),
            .. toSeq(SetCategory.Items).Map(category => Writer(
                new CommandFace($"render-set-sync-{category.Key}", RowText.Localize("Sync {0}", arguments: [category.Caption]).Local,
                    RowText.Localize("Copy selected set {0} settings into every synced set", arguments: [category.Caption]).Local, None, None) { Wording = Wording.Localized },
                [Picked.Source], selection,
                (_, held, rows) => rows.Head.Traverse(row => Category(held.History, held.Doc, row.Key, category).Map(Everywhere)).As())),
            Writer(Face("render-set-apply-all", "Apply to All Sets", "Write selected set settings into every synced set"), [Picked.Source], selection,
                static (_, held, rows) => rows.Head.Traverse(row => Values(held.History, held.Doc, row.Key).Map(Everywhere)).As()),
        ];

    private static Seq<CommandRow> BoardCommands(IO<Seq<BoardRow>> selection) =>
        [
            Writer(Face("render-set-board-apply-all", "Apply to All Sets", "Write each selected set value into every synced set"), [Chosen.Source], selection,
                static (_, _, rows) => IO.pure(Incoming(rows).Map(Everywhere))),
            Writer(Face("render-set-board-apply-set", "Apply to Selected Set", "Write selected values into set chosen in set list"), [Chosen.Source, Picked.Source], selection,
                static (scope, _, rows) =>
                    from picked in scope.Read(Picked.Source)
                    select from incoming in Incoming(rows)
                           from target in toSeq(picked).Head
                           select (Some(target), incoming)),
            new CommandRow.Check(Face("render-set-board-lock-set", "Lock in Set", "Keep selected set values out of writes across sets"), None,
                _ => from rows in selection select ValueRows(rows) switch { var values => !values.IsEmpty && values.ForAll(static value => value.Locked) },
                (scope, enabled) =>
                    from held in Served(scope)
                    from rows in selection
                    from ruled in Callbacks.Each(toSeq(ValueRows(rows).GroupBy(static value => value.Set))
                        .Map(set => Rule(held.History, held.Doc, set.Key, None, Some((toHashSet(set.Select(static value => value.Entry)), enabled)))))
                    select unit),
            new CommandRow.Check(Face("render-set-board-lock-section", "Lock for Presets", "Keep selected settings out of preset apply, paste, and reset"), None,
                scope =>
                    from rows in selection
                    from locks in scope.Locked
                    select SettingKeys(rows) switch { var keys => !keys.IsEmpty && keys.ForAll(locks.Contains) },
                (scope, enabled) =>
                    from rows in selection
                    let keys = SettingKeys(rows)
                    from locked in when(!keys.IsEmpty, scope.Lock(keys, enabled)).As()
                    select unit),
        ];

    private static IO<Change<Seq<SetRow>>> Edited(RenderHistory history, RhinoDoc doc, Seq<SetRow> items) =>
        from book in Book(doc)
        from activated in items.Find(row => row.Active && book.Active != Some(row.Key)).Traverse(row => Activate(history, doc, row.Key)).As()
        from renamed in items.Filter(static row => !string.Equals(row.Name, row.Key, StringComparison.Ordinal))
            .Fold(IO.pure(activated.IfNone(book)), (current, row) => current.Bind(_ => Rename(history, doc, row.Key, row.Name)))
        select ValueStore.Between<Seq<SetRow>>(SetRow.Of(book), SetRow.Of(renamed));

    private static IO<Seq<BoardRow>> Board(RenderHistory history, RhinoDoc doc, IPlugInSink sink) =>
        from book in Book(doc)
        from binding in history.Settings(doc)
        from grading in history.Grading(doc)
        from live in binding.Group.Capture
        select BoardRow.Board(book, binding.Group, grading, live, key => binding.Group.Text(key).Map(text => RowText.Localize(text.Caption, sink).Local));

    private static CommandRow.Run Writer<TItem>(
        CommandFace face, Seq<RowSource> reads, IO<Seq<TItem>> selection, Func<RowScope, (RhinoDoc Doc, RenderHistory History), Seq<TItem>, IO<Option<(Option<SetName> Alone, ValueSet Incoming)>>> plan)
        where TItem : notnull =>
        new(face,
            Some(new RowRule([], reads, scope =>
                from held in Served(scope)
                from rows in selection
                from planned in plan(scope, held, rows)
                from changes in planned.Match(Some: write => Changes(held.History, held.Doc, write.Alone, write.Incoming), None: static () => IO.pure(value: false))
                select changes)),
            scope =>
                from held in Served(scope)
                from rows in selection
                from planned in plan(scope, held, rows)
                from written in planned.Traverse(write => Write(held.History, held.Doc, write.Alone, write.Incoming)).As()
                select unit);

    private static (Option<SetName> Alone, ValueSet Incoming) Everywhere(ValueSet incoming) => (None, incoming);

    private static CommandFace Face(string name, string caption, string help) =>
        new(name, caption, help, None, None) { Wording = Wording.Library };

    private static Seq<BoardRow.Value> ValueRows(Seq<BoardRow> rows) =>
        rows.Choose(static row => row.Switch<Option<BoardRow.Value>>(group: static _ => None, setting: static _ => None, value: static value => value));

    private static Seq<EntryKey> SettingKeys(Seq<BoardRow> rows) =>
        rows.Choose(static row => row.Switch<Option<EntryKey>>(group: static _ => None, setting: static setting => setting.Entry, value: static _ => None));

    private static Option<ValueSet> Incoming(Seq<BoardRow> rows) =>
        Callbacks.Unique(ValueRows(rows), static value => value.Entry, nameof(Incoming))
            .ToOption()
            .Map(static values => new ValueSet(toHashMap(values.Choose(static value => value.Text.Map(text => (value.Entry, text))))));

    private static IO<(RhinoDoc Doc, RenderHistory History)> Served(RowScope scope) =>
        from doc in scope.DocumentIO
        from history in RenderHistory.Current.Of(scope.Sink)
        select (doc, history);
}
