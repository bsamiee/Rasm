using System.Collections.Generic;
using Rhino;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Transition<TState>(TState Before, TState After);

internal sealed record LiveState<TState>(TState Before, TState After, int Shown, bool Stale, long Revision);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LiveModel<TState> where TState : notnull {
    // --- [STATE]
    private readonly Ref<LiveState<TState>> state;

    internal LiveModel(TState initial) => state = Ref(new LiveState<TState>(initial, initial, Shown: 0, Stale: false, Revision: 0));

    public TState Value => state.Value.After;

    public HostEvent<Transition<TState>> Changed =>
        Subscriptions.Changed(state).DistinctUntilChanged(static _ => unit, static held => held.Revision).Choose(static held => Some(new Transition<TState>(held.Before, held.After)));

    // --- [VISIBILITY]
    internal IO<Unit> Show(IO<TState> read) =>
        from shown in atomicIO(() => state.Swap(static held => held with { Shown = held.Shown + 1 }))
        from hidden in Products.Released(() => atomic(() => state.Swap(static held => held with { Shown = held.Shown - 1 })))
        from fresh in when(shown.Stale, read.Bind(Publish)).As()
        select unit;

    internal IO<Unit> Refresh(IO<TState> read) =>
        atomicIO(() => Some(state.Value).Filter(static held => !held.Stale).Map(held => state.Value = held with { Stale = true }).Exists(static held => held.Shown > 0))
            .Bind(shown => when(shown, read.Bind(Publish)).As());

    private IO<Unit> Publish(TState next) =>
        atomicIO(() => state.Value = state.Value switch {
            var held when EqualityComparer<TState>.Default.Equals(held.After, next) => held with { Stale = false },
            var held => held with { Before = held.After, After = next, Stale = false, Revision = held.Revision + 1 },
        }).Map(unit);
}

public sealed class ReadModel<TState>(Seq<HostEvent<Option<uint>>> marks, Func<RhinoDoc, IO<TState>> read) where TState : notnull {
    private string Label { get; } = string.Join(" | ", marks.Map(static mark => mark.Label));

    public IO<Unit> Attach =>
        from post in Mailboxes.Idle<Option<uint>, Unit>(Refresh)
        from attached in marks.TraverseM(mark => mark.Attach(serial => post(serial, unit))).As()
        select unit;

    public IO<LiveModel<TState>> Watch(RhinoDoc doc) =>
        from found in IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
        from live in found.Match(Some: static held => IO.pure(held), None: () => Registered(doc))
        from shown in live.Show(read(doc))
        select live;

    public HostEvent<Transition<TState>> Changed(RhinoDoc doc) =>
        new(Label, deliver => Watch(doc).Bind(live => live.Changed.Attach(deliver)));

    private IO<LiveModel<TState>> Registered(RhinoDoc doc) =>
        read(doc).Bind(initial => IO.lift(() => doc.RuntimeData.GetValue(this, _ => new LiveModel<TState>(initial))));

    private IO<Unit> Refresh(HashMap<Option<uint>, Unit> marked) =>
        from docs in IO.lift(() => toSeq(marked.Keys).Exists(static serial => serial.IsNone)
            ? Conversions.Rows(RhinoDoc.OpenDocuments())
            : toSeq(marked.Keys).Somes().Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Strict())
        from refreshed in Products.Each(docs.Map(doc =>
            IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
                .Bind(found => found.Match(Some: live => live.Refresh(read(doc)), None: static () => IO.pure(unit)))))
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReadModels {
    public static HostEvent<Option<uint>> Marks<TArgs>(DocumentEvent<TArgs> row) =>
        row.Choose(args => Some(row.Serial(args)));

    public static HostEvent<Option<uint>> Marks<TArgs>(HostEvent<TArgs> row) =>
        row.Choose(static _ => Some(Option<uint>.None));
}
