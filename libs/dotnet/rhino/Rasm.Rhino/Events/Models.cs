using Rhino;

namespace Rasm.Rhino.Events;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LiveModel<TState> where TState : notnull {
    // --- [STATE]
    private readonly Atom<(TState Before, TState After)> published;
    private readonly Atom<(int Shown, bool Stale)> visibility;

    internal LiveModel(TState initial) =>
        (published, visibility) = (Atom((Before: initial, After: initial)), Atom((Shown: 0, Stale: false)));

    public TState Value => published.Value.After;

    public HostEvent<(TState Before, TState After)> Changed => Subscriptions.Changed(published);

    // --- [VISIBILITY]
    internal IO<Unit> Show(IO<TState> read, Action<IDisposable> retain) =>
        from shown in DisposalOps.Acquire(
            IO.lift(() => visibility.Swap(static held => held with { Shown = held.Shown + 1 })),
            _ => IO.lift(Hide), retain)
        from fresh in when(shown.Stale, read.Bind(Publish)).As()
        select fresh;

    internal void Hide() => _ = visibility.Swap(static held => held with { Shown = held.Shown - 1 });

    internal IO<Unit> Refresh(IO<TState> read) =>
        visibility.SwapMaybeIO(static held => held.Stale ? None : Some(held with { Stale = true }))
            .Bind(held => when(held.Shown > 0, read.Bind(Publish)).As());

    private IO<Unit> Publish(TState next) =>
        from fresh in visibility.SwapIO(static held => held with { Stale = false })
        from changed in published.SwapMaybeIO(held => EqualityComparer<TState>.Default.Equals(held.After, next) ? None : Some((Before: held.After, After: next)))
        select unit;
}

public sealed class ReadModel<TState>(Seq<HostEvent<Option<uint>>> marks, Func<RhinoDoc, IO<TState>> read) where TState : notnull {
    public IO<Unit> Attach(IPlugInSink sink, Action<IDisposable> retain) =>
        from site in IO.pure(new CallbackSite(sink, typeof(RhinoApp), nameof(RhinoApp.Idle)))
        from mailbox in Subscriptions.Idle<Option<uint>, Unit>(Refresh)(site, retain)
        let post = fun((Option<uint> serial) => mailbox((serial, unit)))
        from attached in marks.TraverseM(mark => mark.Inline(post, sink, retain)).As()
        select unit;

    public IO<LiveModel<TState>> Watch(RhinoDoc doc, Action<IDisposable> retain) =>
        from found in IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
        from live in found.Match(
            Some: static held => IO.pure(held),
            None: () => Registered(doc))
        from shown in live.Show(read(doc), retain)
        select live;

    public HostEvent<(TState Before, TState After)> Changed(RhinoDoc doc) =>
        new(typeof(ReadModel<TState>), nameof(Changed), (deliver, site, retain) =>
            Watch(doc, retain).Bind(live => live.Changed.Attach(deliver, site, retain)));

    private IO<LiveModel<TState>> Registered(RhinoDoc doc) =>
        read(doc).Bind(initial => IO.lift(() => doc.RuntimeData.GetValue(this, _ => new LiveModel<TState>(initial))));

    private IO<Unit> Refresh(HashMap<Option<uint>, Unit> marked) =>
        from docs in IO.lift(() => toSeq(marked.Keys).Exists(static serial => serial.IsNone)
            ? Conversions.Rows(RhinoDoc.OpenDocuments())
            : toSeq(marked.Keys).Somes().Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Strict())
        from refreshed in Callbacks.Each(docs.Map(doc =>
                IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
                    .Bind(found => found.Match(Some: live => live.Refresh(read(doc)), None: static () => IO.pure(unit)))))
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReadModels {
    public static HostEvent<Option<uint>> Marks<TArgs>(DocumentEvent<TArgs> row) =>
        row.Choose(args => Some(row.DocumentSerial(args)));

    public static HostEvent<Option<uint>> Marks<TArgs>(HostEvent<TArgs> row) =>
        row.Choose(static _ => Some(Option<uint>.None));
}
