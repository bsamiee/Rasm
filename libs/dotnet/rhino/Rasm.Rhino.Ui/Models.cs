using Rasm.Rhino.Document;
using Rhino;

namespace Rasm.Rhino.Ui;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LiveModel<TState> where TState : notnull {
    private readonly Atom<TState> state;

    internal LiveModel(TState initial) => state = Atom(initial);

    public TState State => state.Value;

    public IO<IDisposable> Watch(Func<TState, IO<Unit>> render, Action<Error> reject) =>
        from shown in state.ValueIO.Bind(render)
        from attached in Events.Attach<AtomChangedEvent<TState>>(h => state.Change += h, h => state.Change -= h, next => _ = Answers.Answer(render(next), reject, unit))
        select attached;

    internal IO<TState> Publish(TState next) =>
        state.SwapMaybeIO(held => EqualityComparer<TState>.Default.Equals(held, next) ? None : Some(next));
}

public sealed class ReadModel<TState>(Seq<EventKind> kinds, Func<RhinoDoc, IO<TState>> read) where TState : notnull {
    private readonly Atom<(LanguageExt.HashSet<uint> Pending, LanguageExt.HashSet<uint> Taken)> dirty =
        Atom((Pending: LanguageExt.HashSet<uint>.Empty, Taken: LanguageExt.HashSet<uint>.Empty));

    public IO<IDisposable> Attach(Action<Error> reject) =>
        Events.AttachAll(
            kinds.Map(kind => kind.Attach(
                    serial =>
                        from serials in serial.Match(Some: static held => IO.pure(Seq(held)), None: static () => DocumentHandles.OpenDocuments(includeHeadless: false))
                        from marked in dirty.SwapIO(held => held with { Pending = held.Pending.TryAddRange(serials) })
                        select unit,
                    reject))
                .Add(Events.OnIdle((_, _) => _ = Answers.Answer(Refresh(), reject, unit))),
            reject);

    public IO<LiveModel<TState>> Live(RhinoDoc doc) =>
        from found in IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
        from live in found.Match(Some: static held => IO.pure(held), None: () => read(doc).Map(static initial => new LiveModel<TState>(initial)))
        from held in IO.lift(() => doc.RuntimeData.GetValue(this, _ => live))
        select held;

    private IO<Unit> Refresh() =>
        from swapped in dirty.SwapIO(static held => (Pending: [], Taken: held.Pending))
        from fails in toSeq(swapped.Taken).Map<K<IO, Unit>>(Refreshed).Fails().As()
        from failed in unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As()
        select unit;

    private IO<Unit> Refreshed(uint serial) =>
        IO.lift(() =>
                from doc in Optional(RhinoDoc.FromRuntimeSerialNumber(serial))
                from live in Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this))
                select (Doc: doc, Live: live))
            .Bind(found => found.Traverse(held => read(held.Doc).Bind(held.Live.Publish)).As())
            .Map(static _ => unit);
}
