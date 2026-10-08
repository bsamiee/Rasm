using System.Numerics;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Collections;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct HistoryCapacity : IMinMaxValue<HistoryCapacity> {
    public static HistoryCapacity MinValue { get; } = new(1);
    public static HistoryCapacity MaxValue { get; } = new(100);
    public static HistoryCapacity Default { get; } = new(10);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) < 0 || value.CompareTo(MaxValue._value) > 0 ? new InvalidRhinoValue() : null;
}

[ValueObject<int>(SkipIParsable = true, AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MergeWindow : IMinMaxValue<MergeWindow> {
    public static MergeWindow MinValue { get; } = new(0);
    public static MergeWindow MaxValue { get; } = new(600);
    public static MergeWindow Default { get; } = new(5);
    public static Presentation<MergeWindow, int> Presentation { get; } = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) < 0 || value.CompareTo(MaxValue._value) > 0 ? new InvalidRhinoValue() : null;
}

public sealed record HistoryLimits(HistoryCapacity Capacity, MergeWindow Window) {
    public static HistoryLimits Default { get; } = new(HistoryCapacity.Default, MergeWindow.Default);
}

public sealed record HistoryEntry(BigInteger Serial, ValueDiff Diff, Instant At);

[SmartEnum]
public sealed partial class StepDirection {
    public static readonly StepDirection Undo = new();
    public static readonly StepDirection Redo = new();
}

public sealed record ValueHistory {
    // --- [STATE]
    private ValueHistory(Seq<HistoryEntry> done, Seq<HistoryEntry> undone, BigInteger next) => (Done, Undone, Next) = (done, undone, next);

    public const uint TypeCode = 0x4849_5354u;
    public static ValueHistory Empty { get; } = new([], [], BigInteger.Zero);
    public static DictionaryCodec<HashMap<Guid, ValueHistory>> Codec { get; } = new("ValueHistory", 1, Encoded, Histories);
    public Seq<HistoryEntry> Done { get; }
    public Seq<HistoryEntry> Undone { get; }
    private BigInteger Next { get; }

    // --- [TRANSITIONS]
    public ValueHistory Committed(ValueDiff diff, Instant at, HistoryLimits limits) =>
        diff.Changes.IsEmpty ? this : Done.Head
            .Filter(head => Undone.IsEmpty && toHashSet(head.Diff.Changes.Keys) == toHashSet(diff.Changes.Keys)
                && at >= head.At && at - head.At < Duration.FromSeconds(limits.Window))
            .Match(
                Some: head => new ValueHistory(
                    (head.Diff.Then(diff) switch {
                        { Changes.IsEmpty: true } => Done.Tail,
                        var merged => new HistoryEntry(head.Serial, merged, at).Cons(Done.Tail),
                    }).Take(limits.Capacity), Undone, Next),
                None: () => new ValueHistory(new HistoryEntry(Next, diff, at).Cons(Done).Take(limits.Capacity), [], Next + BigInteger.One));

    public ValueHistory Step(BigInteger serial, StepDirection direction) =>
        direction.Map(undo: Done, redo: Undone).Head.Filter(entry => entry.Serial == serial).Match(
            Some: entry => direction.Switch((History: this, Entry: entry),
                undo: static state => new ValueHistory(state.History.Done.Tail, state.Entry.Cons(state.History.Undone), state.History.Next),
                redo: static state => new ValueHistory(state.Entry.Cons(state.History.Done), state.History.Undone.Tail, state.History.Next)),
            None: () => this);

    public ValueHistory Followed(ValueDiff taken) =>
        (Undone.Head.Filter(entry => entry.Diff == taken).Map(entry => Step(entry.Serial, StepDirection.Redo))
            | Done.Head.Filter(entry => entry.Diff == taken.Inverse()).Map(entry => Step(entry.Serial, StepDirection.Undo))).IfNone(this);

    public Option<ValueSet> Toward(BigInteger serial) =>
        Done.Find(entry => entry.Serial == serial)
            .Map(_ => Done.TakeWhile(entry => entry.Serial >= serial).Fold(ValueDiff.Empty, static (later, entry) => entry.Diff.Then(later)).Inverse().After());

    // --- [ARCHIVE]
    private const string DoneKey = "Done";
    private const string UndoneKey = "Undone";
    private const string BeforeKey = "Before";
    private const string AfterKey = "After";
    private const string AtKey = "At";

    private static Fin<ArchivableDictionary> Encoded(HashMap<Guid, ValueHistory> histories) =>
        ArchivableDictionaries.Nested(toSeq(histories.AsIterable()).Map(static pair => (StoredText.Format(pair.Key),
            ArchivableDictionaries.Nested(Seq((DoneKey, Encoded(pair.Value.Done)), (UndoneKey, Encoded(pair.Value.Undone)))))));

    private static Fin<ArchivableDictionary> Encoded(Seq<HistoryEntry> entries) =>
        ArchivableDictionaries.Nested(entries.Map(static entry => (StoredText.Format(entry.Serial), Encoded(entry))));

    private static Fin<ArchivableDictionary> Encoded(HistoryEntry entry) =>
        from target in ArchivableDictionaries.Nested(Seq((BeforeKey, ValueSet.Codec.ToDictionary(entry.Diff.Inverse().After())), (AfterKey, ValueSet.Codec.ToDictionary(entry.Diff.After()))))
        from at in ArchivableDictionaries.Set(target, AtKey, entry.At.ToUnixTimeTicks())
        select target;

    private static Fin<HashMap<Guid, ValueHistory>> Histories(ArchivableDictionary source) =>
        from rows in ArchivableDictionaries.Children(source)
        from pairs in Callbacks.Each(rows, static (row, _) =>
            (StoredText.Parse<Guid>(row.Key).ToValidation(), History(row.Value).ToValidation())
                .Apply(static (id, history) => (id, history)).As().ToFin())
        select toHashMap(pairs);

    private static Fin<ValueHistory> History(ArchivableDictionary source) =>
        (ArchivableDictionaries.Required<ArchivableDictionary>(source, DoneKey).Bind(Entries).ToValidation(),
         ArchivableDictionaries.Required<ArchivableDictionary>(source, UndoneKey).Bind(Entries).ToValidation())
            .Apply(static (done, undone) => new ValueHistory(
                toSeq(done.OrderByDescending(static entry => entry.Serial)), toSeq(undone.OrderBy(static entry => entry.Serial)),
                done.Concat(undone).Fold(BigInteger.Zero, static (next, entry) => BigInteger.Max(next, entry.Serial + BigInteger.One))))
            .As().ToFin();

    private static Fin<Seq<HistoryEntry>> Entries(ArchivableDictionary source) =>
        from rows in ArchivableDictionaries.Children(source)
        from entries in Callbacks.Each(rows, static (row, _) =>
            ((from serial in StoredText.Parse<BigInteger>(row.Key)
              from accepted in serial >= BigInteger.Zero ? Fin.Succ(serial) : Fin.Fail<BigInteger>(new UnreadText(row.Key, typeof(BigInteger)))
              select accepted).ToValidation(),
             ArchivableDictionaries.Required<long>(row.Value, AtKey).ToValidation(),
             ArchivableDictionaries.Required<ArchivableDictionary>(row.Value, BeforeKey).Bind(ValueSet.Codec.FromDictionary).ToValidation(),
             ArchivableDictionaries.Required<ArchivableDictionary>(row.Value, AfterKey).Bind(ValueSet.Codec.FromDictionary).ToValidation())
                .Apply(static (serial, at, before, after) => new HistoryEntry(serial, before.Diff(after), Instant.FromUnixTimeTicks(at))).As().ToFin())
        select entries;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record HistoryScope {
    public IO<AtomHashMap<Guid, ValueHistory>> Histories =>
        Switch(applicationScope: static scope => IO.pure(scope.Registry), documentScope: static scope => Of(scope.Doc));

    public static IO<AtomHashMap<Guid, ValueHistory>> Of(RhinoDoc doc) =>
        IO.lift(() => doc.RuntimeData.GetValue(typeof(ValueHistory), static _ => AtomHashMap<Guid, ValueHistory>()));

    public sealed record ApplicationScope(AtomHashMap<Guid, ValueHistory> Registry) : HistoryScope;
    public sealed record DocumentScope(RhinoDoc Doc, RedrawPolicy Redraw, CallbackSite Site) : HistoryScope;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record HistoryBinding(Guid Id, string Caption, BindingGroup Group, HistoryScope Scope, IO<HistoryLimits> Limits, TimeProvider Clock) {
    // --- [READS]
    public IO<ValueHistory> History => Scope.Histories.Map(histories => histories.Find(Id).IfNone(ValueHistory.Empty));

    // --- [WRITES]
    public IO<ValueDiff> Commit(ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) => Written(incoming, locks).Bind(Recorded);
    public IO<ValueDiff> Commit(ValueSet incoming, ValueSet from) => Recorded(from.Diff(incoming));
    public IO<Unit> Clear =>
        from histories in Scope.Histories
        from cleared in IO.lift(() => histories.Remove(Id))
        select cleared;

    private IO<ValueDiff> Written(ValueSet target, LanguageExt.HashSet<EntryKey> locks) =>
        Scope.Switch((Binding: this, Target: target, Locks: locks),
            applicationScope: static (state, _) => state.Binding.Group.Apply(state.Target, state.Locks),
            documentScope: static (state, scope) =>
                Commits.Commit(scope.Doc, state.Binding.Caption, scope.Redraw,
                        from diff in state.Binding.Group.Apply(state.Target, state.Locks)
                        from recording in IO.lift(() => scope.Doc.UndoRecordingIsActive)
                        from registered in when(recording && !diff.Changes.IsEmpty,
                            TableOps.Register(scope.Doc, new CustomUndo(state.Binding.Caption, state.Binding.Followed(diff.Inverse()), state.Binding.Followed(diff), scope.Site))).As()
                        select diff).Map(static committed => committed.Value));

    private IO<ValueDiff> Recorded(ValueDiff diff) =>
        from at in IO.lift(Clock.GetCurrentInstant)
        from limits in Limits
        from recorded in Swapped(history => history.Committed(diff, at, limits))
        select diff;

    private IO<Unit> Followed(ValueDiff taken) =>
        from applied in Group.Apply(taken.After(), [])
        from moved in Swapped(history => history.Followed(taken))
        select moved;

    private IO<Unit> Swapped(Func<ValueHistory, ValueHistory> step) =>
        from histories in Scope.Histories
        from swapped in IO.lift(() => histories.SwapKey(Id, held => Some(step(held.IfNone(ValueHistory.Empty)))))
        select swapped;

    // --- [STEPS]
    public IO<ValueDiff> Step(StepDirection direction) =>
        (from history in OptionT.lift(History)
         from entry in direction.Map(undo: history.Done, redo: history.Undone).Head
         from diff in Written(direction.Switch(entry.Diff, undo: static taken => taken.Inverse(), redo: identity).After(), [])
         from moved in Swapped(held => held.Step(entry.Serial, direction))
         select diff).IfNone(ValueDiff.Empty).As();

    public IO<ValueDiff> Restore(BigInteger serial) =>
        from history in History
        from target in IO.lift(history.Toward(serial).ToFin(new Missing(nameof(Restore))))
        from diff in Commit(target, [])
        select diff;

    public IO<ValueDiff> Revert(EntryKey key) =>
        (from history in OptionT.lift(History)
         from entry in history.Done.Find(candidate => candidate.Diff.Changes.ContainsKey(key))
         from diff in Commit(entry.Diff.Inverse().After().Scoped(Some(IterableNE.singleton(key))), [])
         select diff).IfNone(ValueDiff.Empty).As();
}
