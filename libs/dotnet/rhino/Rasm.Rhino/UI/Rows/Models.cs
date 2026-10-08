using Rasm.Rhino.Events;
using Rhino;

namespace Rasm.Rhino.UI.Rows;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LiveModel<TState> where TState : notnull {
    // --- [STATE]
    private readonly Atom<(TState Value, int Shown, bool Stale)> state;

    internal LiveModel(TState initial) => state = Atom((Value: initial, Shown: 0, Stale: false));

    public TState Value => state.Value.Value;

    // --- [VISIBILITY]
    internal IO<Unit> Show(IO<TState> read) =>
        from shown in state.SwapIO(static held => held with { Shown = held.Shown + 1 })
        from fresh in when(shown.Stale, read.Bind(Publish)).As()
        select fresh;

    internal void Hide() => _ = state.Swap(static held => held with { Shown = held.Shown - 1 });

    internal IO<Unit> Refresh(IO<TState> read) =>
        state.SwapMaybeIO(static held => held.Shown > 0 || held.Stale ? None : Some(held with { Stale = true }))
            .Bind(held => when(held.Shown > 0, read.Bind(Publish)).As());

    private IO<Unit> Publish(TState next) =>
        state.SwapMaybeIO(held => !held.Stale && EqualityComparer<TState>.Default.Equals(held.Value, next) ? None : Some(held with { Value = next, Stale = false }))
            .Map(static _ => unit);
}

public sealed class ReadModel<TState>(Seq<HostEvent<Option<uint>>> marks, Func<RhinoDoc, IO<TState>> read) where TState : notnull {
    public IO<IDisposable> Attach(IPlugInSink sink) =>
        from site in IO.pure(new CallbackSite(sink, typeof(RhinoApp), nameof(RhinoApp.Idle)))
        from mailbox in Subscriptions.Idle<Option<uint>, Unit>(Refresh)(site)
        let post = fun((Option<uint> serial) => mailbox.Post((serial, unit)))
        from attached in DisposalOps.OnFailure(DisposalOps.AcquireAll(marks.Map(mark => mark.Inline(post, sink)), DisposalOps.Release), IO.lift(mailbox.Release.Dispose))
        select DisposalOps.Composite(mailbox.Release.Cons(attached), site);

    public IO<Disposal<LiveModel<TState>>> Watch(RhinoDoc doc) =>
        from found in IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
        from live in found.Match(
            Some: static held => IO.pure(held),
            None: () => read(doc).Bind(initial => IO.lift(() => doc.RuntimeData.GetValue(this, _ => new LiveModel<TState>(initial)))))
        from shown in live.Show(read(doc))
        select new Disposal<LiveModel<TState>>(live, static held => held.Hide());

    private IO<Unit> Refresh(HashMap<Option<uint>, Unit> marked) =>
        from docs in IO.lift(() => toSeq(marked.Keys).Exists(static serial => serial.IsNone)
            ? Conversions.Rows(RhinoDoc.OpenDocuments(includeHeadless: false))
            : toSeq(marked.Keys).Somes().Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Strict())
        from fails in docs.Map<K<IO, Unit>>(doc =>
                IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
                    .Bind(found => found.Match(Some: live => live.Refresh(read(doc)), None: static () => IO.pure(unit))))
            .Fails()
            .As()
        from failed in unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As()
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReadModel {
    public static HostEvent<Option<uint>> Marks<TArgs>(DocumentEvent<TArgs> row) =>
        row.Choose(args => Some(row.DocumentSerial(args)));

    public static HostEvent<Option<uint>> Marks<TArgs>(HostEvent<TArgs> row) =>
        row.Choose(static _ => Some(Option<uint>.None));
}
