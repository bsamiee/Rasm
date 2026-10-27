using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.RhinoCommon.Events;

// --- [MODELS] --------------------------------------------------------------------------
public record HostEvent<TArgs>(string Label, Func<Func<TArgs, IO<Unit>>, object, IO<Unit>> Attach) {
    public IO<Unit> Inline(Func<TArgs, IO<Unit>> deliver, object owner) => Attach(deliver, owner);

    public IO<Unit> Through(Func<object, IO<Func<TArgs, IO<Unit>>>> mailbox, object owner) => mailbox(owner).Bind(post => Attach(post, owner));

    public HostEvent<TValue> Choose<TValue>(Func<TArgs, Option<TValue>> choose) =>
        new(Label, (deliver, owner) => Attach(args => IO.lift(() => choose(args)).Bind(chosen => chosen.Match(deliver, static () => IO.pure(unit))), owner));

    public HostEvent<TArgs> DistinctUntilChanged<TKey, TMark>(Func<TArgs, TKey> key, Func<TArgs, TMark> mark) where TKey : notnull =>
        new(Label, (deliver, owner) =>
            from marks in IO.lift(static () => Ref(HashMap<TKey, TMark>()))
            from attached in Attach(
                args =>
                    from repeat in atomicIO(() => {
                        HashMap<TKey, TMark> seen = marks.Value;
                        TKey raised = key(args);
                        TMark marked = mark(args);
                        marks.Value = seen.AddOrUpdate(raised, marked);
                        return seen.Find(raised) == Some(marked);
                    })
                    from delivered in unless(repeat, deliver(args)).As()
                    select unit,
                owner)
            select attached);
}

public sealed record DocumentEvent<TArgs>(string Label, Func<Func<TArgs, IO<Unit>>, object, IO<Unit>> Attach, Func<TArgs, Option<uint>> Serial)
    : HostEvent<TArgs>(Label, Attach) {
    public HostEvent<TArgs> In(uint serial) => Choose(args => Conversions.Found(Serial(args) == Some(serial), args));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Subscriptions {
    // --- [ROWS]
    public static HostEvent<TArgs> Host<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Host<EventHandler<TArgs>, TArgs>(add, remove, static handler => handler, label);

    public static HostEvent<TArgs> Host<THandler, TArgs>(
        Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerArgumentExpression(nameof(add))] string label = "")
        where THandler : Delegate =>
        new(label, (deliver, owner) => Attached(add, remove, adapt((_, args) => owner.Start(IO.pure(args).Bind(deliver), label))));

    public static HostEvent<TArgs> During<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerArgumentExpression(nameof(add))] string label = "") =>
        During<EventHandler<TArgs>, TArgs>(add, remove, static handler => handler, label);

    public static HostEvent<TArgs> During<THandler, TArgs>(
        Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerArgumentExpression(nameof(add))] string label = "")
        where THandler : Delegate =>
        new(label, (deliver, owner) => Attached(add, remove, adapt((_, args) => owner.Run(IO.pure(args).Bind(deliver), label))));

    public static HostEvent<TArgs> Watched<TArgs>(Action<EventWatcher, EventHandler<TArgs>> add, [CallerArgumentExpression(nameof(add))] string label = "") =>
        new(label, (deliver, owner) =>
            from watcher in use(static () => new EventWatcher(includeHeadlessDocuments: true))
            from added in IO.lift(() => add(watcher, (_, args) => owner.Start(IO.pure(args).Bind(deliver), label)))
            select unit);

    public static HostEvent<TValue> Changed<TValue>(Atom<TValue> cell, [CallerArgumentExpression(nameof(cell))] string label = "") =>
        Host<AtomChangedEvent<TValue>, TValue>(handler => cell.Change += handler, handler => cell.Change -= handler, static handler => value => handler(sender: null, value), label);

    public static HostEvent<TValue> Changed<TValue>(Ref<TValue> cell, [CallerArgumentExpression(nameof(cell))] string label = "") =>
        Host<AtomChangedEvent<TValue>, TValue>(handler => cell.Change += handler, handler => cell.Change -= handler, static handler => value => handler(sender: null, value), label);

    public static HostEvent<TArgs> Merged<TArgs>(params Seq<HostEvent<TArgs>> rows) =>
        new(string.Join(" | ", rows.Map(static row => row.Label)), (deliver, owner) => rows.TraverseM(row => row.Inline(deliver, owner)).As().Map(unit));

    public static IO<TResult> Detached<THandler, TResult>(Action<THandler> add, Action<THandler> remove, THandler handler, IO<TResult> effect) where THandler : Delegate =>
        IO.lift(() => remove(handler)) >> effect.Finally(IO.lift(() => add(handler)));

    private static IO<Unit> Attached<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler) where THandler : Delegate =>
        use(() => {
            add(handler);
            return handler;
        }, remove).Map(unit);

    // --- [DOCUMENTS]
    extension<TArgs>(HostEvent<TArgs> row) where TArgs : DocumentEventArgs {
        public DocumentEvent<TArgs> Documents() => new(row.Label, row.Attach, static args => Conversions.Present(args.DocumentSerialNumber));
    }

    extension<TArgs>(HostEvent<TArgs> row) {
        public DocumentEvent<TArgs> Documents(Func<TArgs, uint> serial) => new(row.Label, row.Attach, args => Conversions.Present(serial(args)));

        public DocumentEvent<TArgs> Documents(Func<TArgs, RhinoDoc?> document) => new(row.Label, row.Attach, args => Conversions.Serial(document(args)));
    }
}
