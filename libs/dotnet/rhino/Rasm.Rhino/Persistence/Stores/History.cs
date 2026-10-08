using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Collections;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct HistoryCapacity : System.Numerics.IMinMaxValue<HistoryCapacity> {
    public static HistoryCapacity MinValue { get; } = new(1);
    public static HistoryCapacity MaxValue { get; } = new(100);
    public static HistoryCapacity Default { get; } = new(10);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MergeWindow : System.Numerics.IMinMaxValue<MergeWindow> {
    public static MergeWindow MinValue { get; } = new(0);
    public static MergeWindow MaxValue { get; } = new(600);
    public static MergeWindow Default { get; } = new(5);
    public static Presentation<MergeWindow, int> Presentation { get; } = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    public Duration Span => Duration.FromSeconds(_value);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

public sealed record HistoryLimits(HistoryCapacity Capacity, MergeWindow Window) {
    public static HistoryLimits Default { get; } = new(HistoryCapacity.Default, MergeWindow.Default);
}

public sealed record HistoryEntry(uint Serial, ValueDiff Diff, Instant At);

public sealed record ValueHistory {
    // --- [STATE]
    private ValueHistory(Seq<HistoryEntry> done, Seq<HistoryEntry> undone, uint next) => (Done, Undone, Next) = (done, undone, next);

    public const uint TypeCode = 0x4849_5354u;

    public static ValueHistory Empty { get; } = new(Seq<HistoryEntry>(), Seq<HistoryEntry>(), 0u);

    public static DictionaryCodec<HashMap<Guid, ValueHistory>> Codec { get; } = new("ValueHistory", 1, Encoded, Histories);

    public Seq<HistoryEntry> Done { get; }

    public Seq<HistoryEntry> Undone { get; }

    private uint Next { get; }

    // --- [TRANSITIONS]
    public ValueHistory Committed(ValueDiff diff, Instant at, HistoryLimits limits) =>
        diff.Changes.IsEmpty
            ? this
            : Done.Head
                .Filter(head => Undone.IsEmpty
                    && toHashSet(head.Diff.Changes.Keys) == toHashSet(diff.Changes.Keys)
                    && new NodaTime.Interval(head.At, head.At.Plus(limits.Window.Span)).Contains(at))
                .Match(
                    Some: head => new ValueHistory(
                        (head.Diff.Then(diff) switch {
                            { Changes.IsEmpty: true } => Done.Tail,
                            var folded => new HistoryEntry(head.Serial, folded, at).Cons(Done.Tail),
                        }).Take(limits.Capacity),
                        Undone,
                        Next),
                    None: () => new ValueHistory(new HistoryEntry(Next, diff, at).Cons(Done).Take(limits.Capacity), Seq<HistoryEntry>(), Next + 1));

    public ValueHistory Undo(uint serial) =>
        Done.Head.Filter(head => head.Serial == serial).Match(Some: head => new ValueHistory(Done.Tail, head.Cons(Undone), Next), None: () => this);

    public ValueHistory Redo(uint serial) =>
        Undone.Head.Filter(head => head.Serial == serial).Match(Some: head => new ValueHistory(head.Cons(Done), Undone.Tail, Next), None: () => this);

    public ValueHistory Followed(ValueDiff taken) =>
        (Undone.Head.Filter(entry => entry.Diff == taken).Map(entry => Redo(entry.Serial))
            | Done.Head.Filter(entry => entry.Diff == taken.Inverse()).Map(entry => Undo(entry.Serial)))
        .IfNone(this);

    public Option<HistoryEntry> Latest(EntryKey key) =>
        Done.Find(entry => entry.Diff.Changes.ContainsKey(key));

    public Option<ValueSet> Toward(uint serial) =>
        Done.Find(entry => entry.Serial == serial)
            .Map(_ => Done.TakeWhile(entry => entry.Serial >= serial).Fold(ValueDiff.Empty, static (later, entry) => entry.Diff.Then(later)).Inverse().After());

    // --- [ARCHIVE]
    private static Fin<ArchivableDictionary> Encoded(HashMap<Guid, ValueHistory> histories) =>
        ArchivableDictionaries.Nested(toSeq(histories.AsIterable()).Map(static pair => (StoredText.Format(pair.Key), Encoded(pair.Value))));

    private static Fin<ArchivableDictionary> Encoded(ValueHistory history) =>
        ArchivableDictionaries.Nested(Seq(("Done", Encoded(history.Done)), ("Undone", Encoded(history.Undone))));

    private static Fin<ArchivableDictionary> Encoded(Seq<HistoryEntry> entries) =>
        ArchivableDictionaries.Nested(entries.Map(static entry => (StoredText.Format(entry.Serial), Encoded(entry))));

    private static Fin<ArchivableDictionary> Encoded(HistoryEntry entry) =>
        from target in ArchivableDictionaries.Nested(Seq(("Before", ValueSet.Codec.ToDictionary(entry.Diff.Inverse().After())), ("After", ValueSet.Codec.ToDictionary(entry.Diff.After()))))
        from at in ArchivableDictionaries.Set(target, "At", entry.At.ToUnixTimeTicks())
        select target;

    private static Fin<HashMap<Guid, ValueHistory>> Histories(ArchivableDictionary source) =>
        ArchivableDictionaries.Children(source)
            .Bind(static rows => Callbacks.Each(rows, static (row, _) =>
                from id in StoredText.Parse<Guid>(row.Key)
                from history in History(row.Value)
                select (id, history)))
            .Map(static pairs => toHashMap(pairs));

    private static Fin<ValueHistory> History(ArchivableDictionary source) =>
        (ArchivableDictionaries.Required<ArchivableDictionary>(source, "Done").Bind(Entries).ToValidation(),
         ArchivableDictionaries.Required<ArchivableDictionary>(source, "Undone").Bind(Entries).ToValidation())
            .Apply(static (done, undone) => new ValueHistory(
                toSeq(done.OrderByDescending(static entry => entry.Serial)),
                toSeq(undone.OrderBy(static entry => entry.Serial)),
                done.Concat(undone).Fold(0u, static (next, entry) => uint.Max(next, entry.Serial + 1))))
            .As()
            .ToFin();

    private static Fin<Seq<HistoryEntry>> Entries(ArchivableDictionary source) =>
        ArchivableDictionaries.Children(source)
            .Bind(static rows => Callbacks.Each(rows, static (row, _) => StoredText.Parse<uint>(row.Key).Bind(serial => Entry(serial, row.Value))));

    private static Fin<HistoryEntry> Entry(uint serial, ArchivableDictionary source) =>
        (ArchivableDictionaries.Required<long>(source, "At").ToValidation(),
         ArchivableDictionaries.Required<ArchivableDictionary>(source, "Before").Bind(ValueSet.Codec.FromDictionary).ToValidation(),
         ArchivableDictionaries.Required<ArchivableDictionary>(source, "After").Bind(ValueSet.Codec.FromDictionary).ToValidation())
            .Apply((at, before, after) => new HistoryEntry(serial, before.Diff(after), Instant.FromUnixTimeTicks(at)))
            .As()
            .ToFin();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HistoryScope {
    public IO<AtomHashMap<Guid, ValueHistory>> Histories =>
        Switch(
            applicationScope: static scope => IO.pure(scope.Registry),
            documentScope: static scope => Of(scope.Doc));

    public static IO<AtomHashMap<Guid, ValueHistory>> Of(RhinoDoc doc) =>
        IO.lift(() => doc.RuntimeData.GetValue(typeof(ValueHistory), static _ => AtomHashMap<Guid, ValueHistory>()));

    public sealed record ApplicationScope(AtomHashMap<Guid, ValueHistory> Registry) : HistoryScope;

    public sealed record DocumentScope(RhinoDoc Doc, RedrawPolicy Redraw, CallbackSite Site) : HistoryScope;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record HistoryBinding(Guid Id, string Caption, BindingGroup Group, HistoryScope Scope, IO<HistoryLimits> Limits, TimeProvider Clock) {
    // --- [READS]
    public IO<ValueHistory> History =>
        Scope.Histories.Map(histories => histories.Find(Id).IfNone(ValueHistory.Empty));

    // --- [WRITES]
    public IO<ValueDiff> Commit(ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        Written(incoming, locks).Bind(Recorded);

    public IO<ValueDiff> Commit(ValueSet incoming, ValueSet from) =>
        Recorded(from.Diff(incoming));

    public IO<Unit> Clear =>
        Scope.Histories.Bind(histories => IO.lift(() => histories.Remove(Id)));

    private IO<ValueDiff> Written(ValueSet target, LanguageExt.HashSet<EntryKey> locks) =>
        Scope.Switch(
            (Binding: this, Target: target, Locks: locks),
            applicationScope: static (state, _) => state.Binding.Group.Apply(state.Target, state.Locks),
            documentScope: static (state, scope) =>
                Commits.Commit(scope.Doc, state.Binding.Caption, scope.Redraw,
                        from diff in state.Binding.Group.Apply(state.Target, state.Locks)
                        from recording in IO.lift(() => scope.Doc.UndoRecordingIsActive)
                        from registered in when(recording && !diff.Changes.IsEmpty,
                            TableOps.Register(scope.Doc, new CustomUndo(state.Binding.Caption, state.Binding.Followed(diff.Inverse()), state.Binding.Followed(diff), scope.Site))).As()
                        select diff)
                    .Map(static committed => committed.Value));

    private IO<ValueDiff> Recorded(ValueDiff diff) =>
        from at in IO.lift(Clock.GetCurrentInstant)
        from limits in Limits
        from recorded in Swapped(history => history.Committed(diff, at, limits))
        select diff;

    private IO<Unit> Followed(ValueDiff taken) =>
        Group.Apply(taken.After(), []).Bind(_ => Swapped(history => history.Followed(taken)));

    private IO<Unit> Swapped(Func<ValueHistory, ValueHistory> step) =>
        Scope.Histories.Bind(histories => IO.lift(() => histories.SwapKey(Id, held => Some(step(held.IfNone(ValueHistory.Empty))))));

    // --- [STEPS]
    public IO<ValueDiff> Undo =>
        History.Bind(history => history.Done.Head.Match(
            Some: entry => Stepped(entry.Diff.Inverse().After(), held => held.Undo(entry.Serial)),
            None: static () => IO.pure(ValueDiff.Empty)));

    public IO<ValueDiff> Redo =>
        History.Bind(history => history.Undone.Head.Match(
            Some: entry => Stepped(entry.Diff.After(), held => held.Redo(entry.Serial)),
            None: static () => IO.pure(ValueDiff.Empty)));

    public IO<ValueDiff> Restore(uint serial) =>
        History.Bind(history => IO.lift(history.Toward(serial).ToFin(new Missing(nameof(Restore)))))
            .Bind(target => Commit(target, []));

    public IO<ValueDiff> Revert(EntryKey key) =>
        History.Bind(history => history.Latest(key).Match(
            Some: entry => Commit(entry.Diff.Inverse().After().Scoped(Some(IterableNE.singleton(key))), []),
            None: static () => IO.pure(ValueDiff.Empty)));

    private IO<ValueDiff> Stepped(ValueSet target, Func<ValueHistory, ValueHistory> step) =>
        from diff in Written(target, [])
        from moved in Swapped(step)
        select diff;
}
