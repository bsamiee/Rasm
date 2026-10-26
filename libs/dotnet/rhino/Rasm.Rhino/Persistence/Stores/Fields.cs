using Rasm.UI;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EntryText(Seq<string> Path, ParameterText Text);

public sealed record FieldCodec<TValue>(Seq<IStateParameter<TValue>> Parameters, TValue Default) where TValue : notnull {
    // --- [ENTRIES]
    public Seq<RecordEntry<TValue>> Entries => Parameters.Bind(RecordEntries.Of);

    public Seq<EntryText> Texts => Parameters.Bind(static parameter => RecordEntries.Of(parameter).Map(entry => new EntryText(entry.Path, parameter.Text)));

    private Seq<RecordEntry<TValue>> Roots => Parameters.Bind(static parameter => parameter.Kind.Entries(parameter.Key));

    // --- [WRITE]
    public HashMap<Seq<string>, string> Capture(TValue value) => toHashMap(Slots(Some(value)).Choose(static slot => slot.Text.Map(text => (slot.Path, text))));

    public Seq<(Seq<string> Path, Option<string> Text)> Slots(Option<TValue> value) => Entries.Map(entry => (entry.Path, value.Bind(entry.Capture)));

    // --- [READ]
    public IO<(TValue Value, Seq<Error> Refused)> Folded(string owner, Func<Seq<string>, IO<Option<string>>> stored) =>
        from parts in Read(Roots, owner, stored, Default)
        select (parts.Accepted.Fold(Default, static (record, set) => set(record)), parts.Refused);

    public IO<TValue> Recalled(string owner, Func<Seq<string>, IO<Option<string>>> stored) =>
        from read in Folded(owner, stored)
        from _ in unless(read.Refused.IsEmpty, IO.fail<Unit>(Error.Many(read.Refused))).As()
        select read.Value;

    public IO<Option<TValue>> Stored(string owner, Func<Seq<string>, IO<Option<string>>> slot) =>
        from texts in Entries.TraverseM(entry => slot(entry.Path).Map(text => text.Map(held => (entry.Path, held)))).As()
        let stored = toHashMap(texts.Somes())
        from value in stored.IsEmpty ? IO.pure(Option<TValue>.None) : Recalled(owner, path => IO.pure(stored.Find(path))).Map(static value => Some(value))
        select value;

    private static IO<(Seq<Func<TValue, TValue>> Accepted, Seq<Error> Refused)> Read(
        Seq<RecordEntry<TValue>> entries, string owner, Func<Seq<string>, IO<Option<string>>> stored, TValue current) =>
        from parts in entries.Map<K<IO, (Seq<Func<TValue, TValue>> Accepted, Seq<Error> Refused)>>(entry =>
                from text in stored(entry.Path)
                from found in IO.lift(() => text.Traverse(held => new EntryKey(owner, entry.Path).Read(held, entry.Recall)).As())
                from loaded in found.Traverse(static step => step).As()
                let next = loaded.Match(Some: set => set(current), None: () => current)
                from children in Read(entry.Children.Filter(child => child.Active(next)), owner, stored, next)
                select (loaded.ToSeq() + children.Accepted, children.Refused))
            .PartitionFallible()
            .As()
        select (parts.Succs.Bind(static part => part.Accepted), parts.Fails + parts.Succs.Bind(static part => part.Refused));
}
